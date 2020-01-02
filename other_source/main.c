#include <windows.h>
#include <iostream>
//#include <conio.h>

using namespace std;
HANDLE hSerial;


int main(int argc, CHAR* argv[])
{ char data[]={0xB2,0x82,0x43,0x13,0x03,0x44};
  unsigned int ticks=0;
  int i=0;

  LPCTSTR sPortName = "\\\\.\\COM36";
  hSerial = ::CreateFile(sPortName,
                         GENERIC_READ | GENERIC_WRITE,
                         0,
                         0,
                         OPEN_EXISTING,
                         FILE_ATTRIBUTE_NORMAL,
                         0);

  if(hSerial==INVALID_HANDLE_VALUE)
{
  MessageBox(NULL,"1","ERROR", 0);
}

DCB dcbSerialParams = {0};
dcbSerialParams.DCBlength=sizeof(dcbSerialParams);

if (!GetCommState(hSerial, &dcbSerialParams))
{
   // cout << "getting state error\n";
}

dcbSerialParams.BaudRate=CBR_19200;
dcbSerialParams.ByteSize=8;
dcbSerialParams.StopBits=ONESTOPBIT;
dcbSerialParams.Parity=NOPARITY;

if(!SetCommState(hSerial, &dcbSerialParams))
{
   // cout << "error setting serial port state\n";
}
//char data[] = "UART";  // строка для передачи

DWORD dwSize = sizeof(data);   // размер этой строки
DWORD dwBytesWritten;    // тут будет количество собственно переданных байт
//BOOL iRet = WriteFile (hSerial,data,dwSize,&dwBytesWritten,NULL);
//cout << "\n ARGV[1] " <<str<<"\n";// endl;

//Sleep(1000);

  while (1) // цикл сообщений
  {
  BOOL iRet = WriteFile (hSerial,data,dwSize,&dwBytesWritten,NULL);
  ticks++;
  i++;
  if (i>10){data[5]+=7;i=0; } 
  if (data[5]>0x77){data[5]=0x44;}
  cout << "\n SENT: " << ticks<<"\n";// endl;
  Sleep(82);    
  } 
}
