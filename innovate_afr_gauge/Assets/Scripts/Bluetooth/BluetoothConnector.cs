using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class BluetoothConnector : MonoBehaviour, IBluetoothConnector {

    #region Fields

    public const string eBT_CONNECTED = "BT_CONNECTED";
    public const string eBT_CONNECTION_ERROR = "eBT_CONNECTION_ERROR";
    public const string eBT_CONNECTION_STATUS = "eBT_CONNECTION_STATUS";
    public const string eBT_DISCONNECTED = "eBT_DISCONNECTED";
    public const string eBT_DATA_RECIVED = "eBT_DATA_RECIVED";

    protected enum BtStates
    {
        None,
        Initializing,
        Scan,
        Connect,
        Subscribe,
        Connected,
        DisConnecting,
        ConnectingError,

        NullState,
    }
    
    [SerializeField] string DeviceName = "MIRB6";
    [SerializeField]  string ServiceUUID = "0000ffe0-0000-1000-8000-00805f9b34fb";
    [SerializeField]  string CharacteristicUUID = "0000ffe1-0000-1000-8000-00805f9b34fb";
    
    protected class BtFSM : FiniteStateMachine<BtStates> { }
    protected BtFSM state;
    private string deviceAddress;

    BtStates nextState = BtStates.NullState;
    float nextStateTimeout;

    BluetoothOperation curretnRequest = null;

    #endregion



    #region Unity lifecicle

    void Start()
    {
        state = new BtFSM();

        state.AddTransition(BtStates.None, BtStates.Initializing, StartScan);
        state.AddTransition(BtStates.Initializing, BtStates.Scan, StartScan);
        state.AddTransition(BtStates.Scan, BtStates.Connect, StartConnection);
        state.AddTransition(BtStates.Connect, BtStates.Subscribe, StartSubscribe);
        state.AddTransition(BtStates.Subscribe, BtStates.Connected, StartConnected);

        state.AddTransition(BtStates.Initializing, BtStates.ConnectingError, ConnectingError);
        state.AddTransition(BtStates.Scan, BtStates.ConnectingError, ConnectingError);
        state.AddTransition(BtStates.Connect, BtStates.ConnectingError, ConnectingError);
        state.AddTransition(BtStates.Subscribe, BtStates.ConnectingError, ConnectingError);
        state.AddTransition(BtStates.Connected, BtStates.ConnectingError, ConnectingError);

        state.AddTransition(BtStates.DisConnecting, BtStates.None, StartNon);
        state.AddTransition(BtStates.ConnectingError, BtStates.None, StartNon);

        state.AddTransition(BtStates.Scan, BtStates.DisConnecting, null);
        state.AddTransition(BtStates.Connect, BtStates.DisConnecting, null);
        state.AddTransition(BtStates.Subscribe, BtStates.DisConnecting, null);
        state.AddTransition(BtStates.Connected, BtStates.DisConnecting, null);

        state.Initialise(BtStates.None);
    }
    

    void Update()
    {
        if (nextStateTimeout > 0f && nextState != BtStates.NullState)
        {
            nextStateTimeout -= Time.deltaTime;
            if (nextStateTimeout <= 0f)
            {
                Debug.Log("nextState = " + nextState + " state = " + state.GetState());
                nextStateTimeout = 0f;
                BtStates tmp = nextState;
                nextState = BtStates.NullState;
                state.Advance(tmp);
            }
        }
    }

    #endregion



    #region BTInterface

    public void Connect()
    {
        if (state.GetState() != BtStates.None)
            return;

        SetState(BtStates.Initializing);

        EventDispatcher.SendEvent(eBT_CONNECTION_STATUS, "Подождите секунду.");
        Reset();

        if (Application.isEditor)
        {
            Debug.Log("Initialize failed. error = edior");
            SetState(BtStates.ConnectingError, 0.3f);
            return;
        }
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            Debug.Log("Initialize ok.");
            SetState(BtStates.Scan, 0.1f);

        }, null);
    }
    
    
    public void DisConnect()
    {
        SetState(BtStates.DisConnecting);

        StartCoroutine(disConnectProcess());
    }

    
    public bool IsConnected()
    {
        return state.GetState() == BtStates.Connected;
    }

    
    public bool IsReadyToSend()
    {
        return IsConnected() && curretnRequest == null;
    }

    
    public void SendData(byte[] data, Action<byte[]> response = null, Action error = null)
    {
        if (curretnRequest != null)
        {
            Debug.Log("Send data failed. Cuerrent data does not sended");

            if (error != null)
                error();
            return;
        }

        StartCoroutine(RequestProces(data, response, error));
    }

    #endregion



    #region Sate calbacks
    
    void StartScan()
    {
        EventDispatcher.SendEvent(eBT_CONNECTION_STATUS, "Ищем устройство.");

        int tryCount = 0;
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, null, 
            (address, deviceName, rssi, bytes) => {

                tryCount++;
                if (deviceName.Contains(DeviceName))
                {
                    BluetoothLEHardwareInterface.StopScan();

                    deviceAddress = address;
                    SetState(BtStates.Connect, 0.5f);
                }else if(tryCount == 20)
                {
                    EventDispatcher.SendEvent(eBT_CONNECTION_STATUS, "Ищем устройство. \n Возможно контроллер зеркал отключил свой модуль bluetoth по истечению таймера — выключите и включите зажигание.");
                }
                else if(tryCount > 50)
                {
                    BluetoothLEHardwareInterface.StopScan();
                    Debug.Log("Scan failed. Service Does not found");
                    
                    SetState(BtStates.ConnectingError, 0.3f);
                }

            }, false);
    }
    
    
    void StartConnection()
    {
        EventDispatcher.SendEvent(eBT_CONNECTION_STATUS, "Подключаемся.");

        BluetoothLEHardwareInterface.ConnectToPeripheral(deviceAddress, (str) =>
        {
            Debug.Log("StartConnection deviceAddress = " + deviceAddress + " str = " + str);
            if (str.Equals(deviceAddress))
            {
                SetState(BtStates.Subscribe, 2.0f);
            }else
            {
                Debug.Log("ConnectToPeripheral failed. Connection error 1 " + str);
                SetState(BtStates.ConnectingError, 0.3f);
            }
        }, null, null, (str) =>
        {
            Debug.Log("ConnectToPeripheral failed. Connection error 2 " + str);
            SetState(BtStates.ConnectingError, 0.3f);
        });
    }
    
    
    void StartSubscribe()
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(deviceAddress, ServiceUUID, CharacteristicUUID, null, (address, characteristicUUID, bytes) => {

            Debug.Log("StartSubscribe : Recive data ");
            if (curretnRequest != null)
                curretnRequest.addResponce(bytes);
            else
                EventDispatcher.SendEvent(eBT_DATA_RECIVED, bytes);
        });

        SetState(BtStates.Connected, 2f);
    }
    

    void StartConnected()
    {
        Debug.Log("StartConnected deviceAddress = " + deviceAddress);

        EventDispatcher.SendEvent(eBT_CONNECTED);
    }
    
    
    void ConnectingError()
    {
        Debug.Log("ConnectingError");

        StartCoroutine(disConnectProcess());

        EventDispatcher.SendEvent(eBT_CONNECTION_ERROR);
    }

    
    void StartNon()
    {
        Reset();
        EventDispatcher.SendEvent(eBT_DISCONNECTED);
    }

    #endregion
    


    #region Private methods

    IEnumerator disConnectProcess()
    {
        BluetoothLEHardwareInterface.StopScan();
        yield return new WaitForSeconds(0.2f);
        BluetoothLEHardwareInterface.DisconnectAll();
        yield return new WaitForSeconds(0.2f);
        BluetoothLEHardwareInterface.DeInitialize(() => {
            SetState(BtStates.None);
        });
    }

    
    void SetState(BtStates newState, float timeout = 0.0f)
    {
        if (timeout > 0.0f)
        {
            nextState = newState;
            nextStateTimeout = timeout;
        }
        else
            state.Advance(newState);
    }

    
    void Reset()
    {
        deviceAddress = null;
    }

    
    IEnumerator RequestProces(byte[] data, Action<byte[]> response, Action error)
    {
        Debug.Log("SendBytes Started");

        yield return new WaitForSeconds(0.1f);

        curretnRequest = new BluetoothOperation(deviceAddress, ServiceUUID, CharacteristicUUID, data);
        yield return StartCoroutine(curretnRequest.WaitFor());

        yield return new WaitForSeconds(0.1f);

        if (curretnRequest.isSuccess)
        {
            if(response != null)
                response(curretnRequest.responseByte);
        } else {
            if (error != null)
                error();
        }

        curretnRequest = null;
        Debug.Log("SendBytes Ended");
    }

    #endregion
}
