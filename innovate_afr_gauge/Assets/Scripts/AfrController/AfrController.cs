using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TechTweaking.Bluetooth;
using Zenject;


public class AfrController : IAfrController
{
    #region Fields

    [Inject] 
    private IBluetoothConnector bluetoothConnector;

    List<IAfrControllerDelegate> delegates = new List<IAfrControllerDelegate>();
    #endregion



    #region Methids
    
    public void Initialize()
    {
        EventDispatcher.Bind(BluetoothConnector.eBT_CONNECTED, BTConnected);
        EventDispatcher.Bind(BluetoothConnector.eBT_CONNECTION_ERROR, BTConnectionError);
        EventDispatcher.Bind(BluetoothConnector.eBT_CONNECTION_STATUS, BTConnectionStatus);
        EventDispatcher.Bind(BluetoothConnector.eBT_DISCONNECTED, BTDisonnected);
        EventDispatcher.Bind(BluetoothConnector.eBT_DATA_RECIVED, BTDataRecived);

        
        BluetoothAdapter.askEnableBluetooth();

        if (BluetoothAdapter.isBluetoothEnabled())
            checkConnection();
    }

    public void Subscribe(IAfrControllerDelegate controllerDelegate)
    {
        delegates.Add(controllerDelegate);
    }
    
    
    public void UnSubscribe(IAfrControllerDelegate controllerDelegate)
    {
        delegates.Remove(controllerDelegate);
    }


    public void TryConnect()
    {
        checkConnection();
    }
    
    
    public bool IsConnected()
    {
        return bluetoothConnector.IsConnected();
    }
    
    
    void checkConnection()
    {
        UpdateConnectionStatus();
        
        if (!BluetoothAdapter.isBluetoothEnabled())
        {
            return;
        }

        if (!bluetoothConnector.IsConnected())
        {
            StartConnection();
            bluetoothConnector.Connect();
        }
    }


    void DecodeData(byte[] data)
    {
        byte[] bufferLC = new byte[] {0,0,0,0,0,0};

        if (data[0] == 0xb2)
        {
            for (int i= 0; i < 6; i++)
            {
                bufferLC[i] = data[i];
            }
        }

        uint afr1 = ((uint)bufferLC[4] & 0x7F) << 7;
        uint afr2 = (uint)bufferLC[5] & 0x7F;
        afr1 = afr1 + afr2;
        float afr = afr1;
        float olevel = afr;
        afr = afr + 500;
        afr = 14.7f* afr;
        afr = afr / 1000;

        AfrData result = new AfrData();
        
        result.afr = (float)Math.Round(afr*100)/100.0f;
        result.o2level = (float)(Math.Round(olevel * 100) / 100.0f) / 10.0f;

        int status = ((bufferLC[2] & 0x1C) >> 2);

        switch (status)
        {
            case 0: // 000 Lambda valid and Aux data valid, normal operation.
                result.status = AfrStatus.AfrValid;
                //TextLabel.text = String(afr)
                //Status.text = "AIR / FUEL RATIO"
            break;
            case 1: // 001 Lambda value contains O2 level in 1/10%
                result.status = AfrStatus.O2LevelValid;
                //TextLabel.text = String(olevel)
                //Status.text = "  O2 LAVEL %"
                result.afr = 7.5f + 13.5f * (olevel / 100.0f);
                break;
            case 2: // 010 Free air Calib in progress, Lambda data not valid
                result.status = AfrStatus.FreeAirCalib;
                break;
            case 3: // 011 Need Free air Calibration Request, Lambda data not valid
                result.status = AfrStatus.NeedFreeAirCalib;
                break;
            case 4: // 100 Warming up, Lambda value is temp in 1/10% of operating temp.
                result.status = AfrStatus.WarmingUp;
                result.afr = 7.5f + 13.5f * (olevel / 100.0f);
                break;
            case 5: // 101 Heater Calibration, Lambda value contains calibration countdown.
                result.status = AfrStatus.HeaterCalib;
                break;
            case 6: // 110 Error code in Lambda value
                result.status = AfrStatus.Error;
                break;
        }

        AfrRecived(result);
    }
    
    #endregion
    
    
    
    #region Delegate callback

    void StartConnection()
    {
        foreach (var del in delegates)
        {
            del.StartConnection();
        }
    }

    void ConnectionInfo(string info)
    {
        foreach (var del in delegates)
        {
            del.ConnectionInfo(info);
        }
    }

    void Connected(bool c)
    {
        foreach (var del in delegates)
        {
            del.Connected(c);
        }
    }

    void UpdateConnectionStatus()
    {
        foreach (var del in delegates)
        {
            del.UpdateConnectionStatus(bluetoothConnector.IsConnected());
        }
    }


    void AfrRecived(AfrData afr)
    {
        foreach (var del in delegates)
        {
            del.AfrRecived(afr);
        }
    }
    
    #endregion
    
    
    
    #region Bluetooth callbacks
    
    void BTConnectionError(object data)
    {
        UpdateConnectionStatus();
        Connected(false);
    }

    
    void BTConnectionStatus(object data)
    {
        ConnectionInfo(data.ToString());
    }
    
    
    void BTDisonnected(object data)
    {
        UpdateConnectionStatus();
        Connected(false);
    }
    
    
    void BTConnected(object data)
    {
        UpdateConnectionStatus();
        Connected(true);
    }


    void BTDataRecived(object data)
    {
        byte[] bytes = (byte[])data;

        DecodeData(bytes);
    }
    
    #endregion
}
