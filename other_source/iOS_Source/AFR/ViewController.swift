//
//  ViewController.swift
//  AFR
//
//  Created by Dima on 26/02/2019.
//  Copyright © 2019 DK. All rights reserved.
//

import UIKit
import CoreBluetooth

struct DisplayPeripheral: Hashable {
    let peripheral: CBPeripheral
    let lastRSSI: NSNumber
    let isConnectable: Bool
    
    var hashValue: Int { return peripheral.hashValue }
    
    static func ==(lhs: DisplayPeripheral, rhs: DisplayPeripheral) -> Bool {
        return lhs.peripheral == rhs.peripheral
    }
}


extension UIColor{
    static var btBlue: UIColor {
        return UIColor(red: 26.0/255.0, green: 118.0/255.0, blue: 245.0/255.0, alpha: 1.0)
    }
    
    static var btGreen: UIColor {
        return UIColor(red: 0.0/255.0, green: 212.0/255.0, blue: 142.0/255.0, alpha: 1.0)
    }
    
    static var btRed: UIColor {
        return UIColor(red: 246.0/255.0, green: 65.0/255.0, blue: 56.0/255.0, alpha: 1.0)
    }
    
    static var btOrange: UIColor {
        return UIColor(red: 255.0/255.0, green: 128.0/255.0, blue: 0/255.0, alpha: 1.0)
    }
}

class ViewController: UIViewController,CBCentralManagerDelegate,CBPeripheralDelegate {
    

    @IBOutlet weak var ButtonCon: UIButton!
    
    @IBOutlet weak var TextLabel: UILabel!
    @IBOutlet weak var Label: UILabel!
    
    @IBOutlet weak var Image: UIImageView!
    
    @IBOutlet weak var Status: UILabel!
    
    private var rssiReloadTimer: Timer?
    var manager:CBCentralManager!
    var peripheral:CBPeripheral!
    var status=0
    var statusconnected=0
    
    @IBAction func connect(_ sender: UIButton) {
        if (status==0){
            if manager.state == .poweredOn {
                manager.scanForPeripherals(withServices: nil, options: nil)
            }
            print("\(status)")
            status=1
            ButtonCon.layer.backgroundColor=(UIColor(red: 246.0/255.0, green: 65.0/255.0, blue: 56.0/255.0, alpha: 1.0)).cgColor
            ButtonCon.setTitle("STOP", for: .normal);
            
        }
        else
        {
            if (statusconnected==1){
            manager.cancelPeripheralConnection(peripheral)
            statusconnected=0
            }
            print("\(status)")
            status=0
            TextLabel.text=""
            ButtonCon.layer.backgroundColor=(UIColor(red: 0.0/255.0, green: 212.0/255.0, blue: 142.0/255.0, alpha: 1.0)).cgColor
            ButtonCon.setTitle("RUN", for: .normal);
            
        }
    }
    
   
    override func viewDidLoad() {
        super.viewDidLoad()
        ButtonCon.layer.cornerRadius = 5    /// радиус закругления закругление
        ButtonCon.layer.borderWidth = 3.0   // толщина обводки
        ButtonCon.layer.backgroundColor=(UIColor(red: 0.0/255.0, green: 212.0/255.0, blue: 142.0/255.0, alpha: 1.0)).cgColor
        ButtonCon.layer.borderColor = (UIColor(red: 0.0/255.0, green: 212.0/255.0, blue: 142.0/255.0, alpha: 1.0)).cgColor // цвет обводки
        ButtonCon.setTitle("Включить", for: .normal);
        TextLabel.text=""
        ButtonCon.clipsToBounds = true  // не забудь это, а то не закруглиться
     //   ButtonCon.setTitle("STOP", for: .normal);
   //     ButtonCon.isHidden=true;
        manager = CBCentralManager(delegate: self, queue: nil)
      /*  if manager.state == .poweredOn {
            manager.scanForPeripherals(withServices: nil, options: nil)
        }*/
        // Do any additional setup after loading the view, typically from a nib.
    }
    
    @objc private func refreshRSSI(){
        peripheral.readRSSI()
    }
    
    
    func centralManagerDidUpdateState(_ central: CBCentralManager)
    {
    }
    
    
    func centralManager(_ central: CBCentralManager, didDiscover peripheral: CBPeripheral, advertisementData: [String : Any], rssi RSSI: NSNumber){
        
        let isConnectable = advertisementData["kCBAdvDataIsConnectable"] as! Bool
        let displayPeripheral = DisplayPeripheral(peripheral: peripheral, lastRSSI: RSSI,isConnectable: isConnectable)
        print("добавлено")
        print("\(displayPeripheral.peripheral.description)")
        if ((peripheral.name=="BlueNRG")||(peripheral.name=="ATOMIC"))
        {
            central.stopScan()
            print("найдено")
            statusconnected=1
            //peripheral.delegate = self
            self.peripheral = peripheral
            self.peripheral.delegate = self
            UIApplication.shared.isIdleTimerDisabled = true;
           
            manager.connect(peripheral, options: nil)
           // ButtonCon.isHidden=false;
        }
    }
    
    func centralManager(_ central: CBCentralManager, didConnect peripheral: CBPeripheral) {
        peripheral.discoverServices(nil)
        peripheral.readRSSI()
        print("сервисы")
        status=1
        
        rssiReloadTimer = Timer.scheduledTimer(timeInterval: 1.0, target: self, selector: #selector(ViewController.refreshRSSI), userInfo: nil, repeats: true)
    }
    
    func centralManager(_ central: CBCentralManager, didDisconnect peripheral: CBPeripheral)
    {
        print("Disconnect");
        status=0
    }
    
    func peripheral(_ peripheral: CBPeripheral, didDiscoverServices error: Error?) {
        if let error = error {
            print("Error discovering services: \(error.localizedDescription)")
        }
        peripheral.services?.forEach(
            { (service) in
                print("\(service.description)")
                peripheral.delegate=self
                peripheral.discoverCharacteristics(nil, for: service)
            }
        )
    }
    
    func peripheral(_ peripheral: CBPeripheral, didDiscoverCharacteristicsFor service: CBService, error: Error?) {
        if let error = error {
            print("Error discovering service characteristics: \(error.localizedDescription)")
        }
        
        service.characteristics?.forEach({ characteristic in
            peripheral.setNotifyValue(true, for: characteristic)
            peripheral.readValue(for: characteristic)
            print(characteristic.description)
        })
    }
    
    func peripheral(_ peripheral: CBPeripheral, didUpdateValueFor characteristic: CBCharacteristic, error: Error?)
    {
        let DATA_UUID2 = CBUUID(string: "E23E78A0-CF4A-11E1-8FFC-0002A5D5C51B")
        let DATA_UUID = CBUUID(string: "09366E80-CF3A-11E1-9AB4-0002A5D5C51B")
        let DATA_UUID3 = CBUUID(string: "FFE1")
        if ((characteristic.uuid==(DATA_UUID))||(characteristic.uuid==(DATA_UUID2))||(characteristic.uuid==(DATA_UUID3)))
        {
            let data = characteristic.value
            var bufferLC:[UInt8]=[0,0,0,0,0,0]
            var flagenable:[UInt8]=[0];
       //     if (data!=0){
            data?.copyBytes(to: &flagenable, count: 1)
            if (flagenable[0]==0xB2) {
                data?.copyBytes(to: &bufferLC, count: 6)}//}
          //  let valueInInt = (Int(databytearr[1]))*255+Int(databytearr[0])
            var afr1:UInt16=0
            var afr2:UInt16=0
            var afr:Float=1.1
            var olevel:Float=1.1
            var status:UInt8=0
            // расчет АФР
            //B2 82 43 13 03 74
            afr1=((UInt16(bufferLC[4]) & 0x7F) << 7)
            afr2=(UInt16(bufferLC[5]) & 0x7F);
            afr1=(afr1+afr2);
            afr=Float(afr1);
            olevel=afr;
            afr=afr+500;
            afr=14.7*afr;
            afr=afr/1000;
            afr=(round(afr*100)/100);
            olevel=(round(olevel*100)/100)/10
            TextLabel.text = String(afr)
            status=((bufferLC[2] & 0x1C) >> 2)
            switch status {
            case 0:
                TextLabel.text = String(afr)
                Status.text="AIR / FUEL RATIO"
            case 1:
                TextLabel.text = String(olevel)
                Status.text="  O2 LAVEL %"
                afr=7.5+13.5*(olevel/100)
            case 2:
                TextLabel.text = "AIR"
                Status.text="AIR Calib..."
            case 4:
                TextLabel.text = String(olevel)
                Status.text="Warming up..."
                afr=7.5+13.5*(olevel/100)
            case 6:
                TextLabel.text = String(olevel)
                Status.text="Error"
              //  afr=7.5+13.5*(olevel/100)
            default:
                print("не удалось распознать число")
            }
            /*
             000 Lambda valid and Aux data valid, normal operation.
             001 Lambda value contains O2 level in 1/10%
             010 Free air Calib in progress, Lambda data not valid
             011 Need Free air Calibration Request, Lambda data not valid
             100 Warming up, Lambda value is temp in 1/10% of operating temp.
             101 Heater Calibration, Lambda value contains calibration countdown.
             110 Error code in Lambda value
             */
            
            if status==0
            {
            
            if (afr < 7.5){Image.image = UIImage(named:"1g")}
            if ((afr < 8.20) && (afr>7.49)){Image.image = UIImage(named:"2g")}
            if ((afr < 8.90) && (afr>8.19)){Image.image = UIImage(named:"3g")}
            if ((afr < 9.60) && (afr>8.89)){Image.image = UIImage(named:"4g")}
            if ((afr < 10.3) && (afr>9.59)){Image.image = UIImage(named:"5g")}
            if ((afr < 11.00) && (afr>10.29)){Image.image = UIImage(named:"6g")}
            if ((afr < 11.70) && (afr>10.99)){Image.image = UIImage(named:"7g")}
            if ((afr < 12.50) && (afr>11.69)){Image.image = UIImage(named:"8g")}
            if ((afr < 13.20) && (afr>12.49)){Image.image = UIImage(named:"9g")}
            if ((afr < 13.90) && (afr>13.19)){Image.image = UIImage(named:"10g")}
            if ((afr < 14.60) && (afr>13.89)){Image.image = UIImage(named:"11g")}
            if ((afr < 15.30) && (afr>14.59)){Image.image = UIImage(named:"12g")}
            if ((afr < 16.00) && (afr>15.29)){Image.image = UIImage(named:"13g")}
            if ((afr < 16.70) && (afr>15.99)){Image.image = UIImage(named:"14g")}
            if ((afr < 17.40) && (afr>16.69)){Image.image = UIImage(named:"15g")}
            if ((afr < 18.10) && (afr>17.39)){Image.image = UIImage(named:"16g")}
            if ((afr < 18.80) && (afr>18.09)){Image.image = UIImage(named:"17g")}
            if ((afr < 19.50) && (afr>18.79)){Image.image = UIImage(named:"18g")}
            if ((afr < 20.20) && (afr>19.49)){Image.image = UIImage(named:"19g")}
            if ((afr < 20.90) && (afr>20.19)){Image.image = UIImage(named:"20g")}
            if (afr>20.89){Image.image = UIImage(named:"21g")}
            } else {Image.image = UIImage(named:"0g")}
        }
    }
    
    func peripheral(_ peripheral: CBPeripheral, didReadRSSI RSSI: NSNumber, error: Error?) {
        switch RSSI.intValue {
        case -90 ... -60:
            Label.textColor = .btOrange
            break
        case -200 ... -90:
            Label.textColor = .btRed
            break
        default:
            Label.textColor = .btGreen
        }
        
        Label.text = "\(RSSI)dB"
    }
    
    
}
