#include "SPIFlash.h"
#include <SPI.h>
#include <avr/wdt.h>
//////////////////////////////////////////
// flash(SPI_CS, MANUFACTURER_ID)
// SPI_CS          - CS pin attached to SPI flash chip (8 in case of Moteino)
// MANUFACTURER_ID - OPTIONAL, 0x1F44 for adesto(ex atmel) 4mbit flash
//                             0xEF30 for windbond 4mbit flash
//////////////////////////////////////////
SPIFlash flash(2, 0);
byte buf[1024];

unsigned int CRC(byte input[], int array_size)
{
  unsigned int crc16 = 0xFFFF;

  /*Main Code*/
  for (int pos = 0; pos < array_size; pos++) {
    crc16 ^= input[pos];          // XOR byte into least sig. byte of crc

    for (int i = 8; i != 0; i--) {    // Loop over each bit
      if ((crc16 & 0x0001) != 0) {      // If the LSB is set
        crc16 >>= 1;                    // Shift right and XOR 0xA001
        crc16 ^= 0xA001;
      }
      else                            // Else LSB is not set
        crc16 >>= 1;                    // Just shift right
    }
  }
  /*Note, this number has low and high bytes swapped, so swap bytes*/
  return crc16;
} 

void setup() {
  Serial.begin(115200);
  while (!Serial);
  if (flash.initialize())
    Serial.println("Init OK!");
  else
    Serial.println("Init FAIL!");
}
 
void loop() {
  char cmd;
  if (!Serial.available()) return;
  cmd = Serial.read();
  if (cmd == 't') {
    Serial.print("COM ok\n");
    return;
  }
  if (cmd == 'i')
  {
    Serial.print("DeviceID: ");
    Serial.print(flash.readDeviceId(), HEX);
    Serial.print('\n');
    return;
  }
  if (cmd == 'a')
  {
    //flash.chipErase();
    while (flash.busy());
    Serial.print("OK");
    Serial.print('\n');
    return;
  }
  if (cmd == 'e')
  {
    long  sector = Serial.parseInt();
    Serial.read(); // разделитель
    flash.blockErase4K(sector);
    Serial.print("OK");
    Serial.print(sector);
    Serial.print('\n');
    return;
  }
  if (cmd == 'w')
  {
    byte crcBuf[2];
    long addr = Serial.parseInt();
    Serial.read(); // разделитель
    for (int bufsz = 0; bufsz < 128+2; bufsz++)
    {
      while (Serial.available() == 0);
      buf[bufsz] = Serial.read();
    }
    unsigned int crc = CRC(buf, 128);
    crcBuf[0] = (byte)(crc >> 8);
    crcBuf[1] = (byte)(crc & 0xFF);

    if(crcBuf[0] == buf[128] && crcBuf[1] == buf[129]){
      flash.writeBytes(addr, buf, 128);
      Serial.print("OK\n");
    }else
      Serial.print("NO\n");

    return;
  }
  if (cmd == 'r') {
    long addr = Serial.parseInt();
    Serial.read(); // разделитель
    byte crcBuf[8];
    for (int i = 0; i < 4; i++)
    {
      flash.readBytes(addr + (i * 1024) + 0, buf, 1024);

      unsigned int crc = CRC(buf, 1024);
      crcBuf[i*2] = (byte)(crc >> 8);
      crcBuf[i*2 + 1] = (byte)(crc & 0xFF);

      for (int j = 0; j < 1024; j++)
        Serial.write(buf[j]);
    }

    for (int j = 0; j < 8; j++)
        Serial.write(crcBuf[j]);
    return;
  }
}
