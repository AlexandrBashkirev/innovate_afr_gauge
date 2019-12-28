using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TechTweaking.Bluetooth;
using Zenject;


enum HoldType
{
    None,
    AngleUp,
    AngleDown,
    CorrectorPlus,
    CorrectorMinus,
}
public class MainWindow : Window
{
    const string eNEED_READ_DEVICE = "eNEED_READ_DEVICE";

    #region Fields

    [Inject] 
    private IBluetoothConnector bluetoothConnector;
    
    private Loader loader = null;

    float lastHoldUpdate = 0;
    float holdStartTime = 0;
    byte holdedParamValue;
    HoldType holdType = HoldType.None;
    bool isHolding = false;


    bool initialDataRecived = false;
    private byte[] deviceParamData = new byte[13];

    #endregion



    #region Unity lifecicle

    public override void Start()
    {
        base.Start();

        EventDispatcher.Bind(BluetoothConnector.eBT_CONNECTED, BTConnected);
        EventDispatcher.Bind(BluetoothConnector.eBT_CONNECTION_ERROR, BTConnectionError);
        EventDispatcher.Bind(BluetoothConnector.eBT_CONNECTION_STATUS, BTConnectionStatus);
        EventDispatcher.Bind(BluetoothConnector.eBT_DISCONNECTED, BTDisonnected);
        EventDispatcher.Bind(BluetoothConnector.eBT_DATA_RECIVED, BTDataRecived);

        EventDispatcher.Bind(eNEED_READ_DEVICE, ReadDeviceData);

        Debug.Log("Start");
        
        BluetoothAdapter.askEnableBluetooth();

        if (BluetoothAdapter.isBluetoothEnabled())
            checkConnection();
    }

    
    public override void Update()
    {
        base.Update();

        updateHolding();
    }

    
    void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log("OnApplicationFocus hasFocus = " + hasFocus);

        if (hasFocus && BluetoothAdapter.isBluetoothEnabled())
            checkConnection();
    }
    
    #endregion
    
    
    
    #region Bluetooth callbacks
    
    void BTConnectionError(object data)
    {
        Debug.Log("BTConnectionError");
        initialDataRecived = false;
        setBTStatus();
        if (loader != null)
            SceneManager.instance.PullWin(loader);
    }

    
    void BTConnectionStatus(object data)
    {
        Debug.Log("BTConnectionStatus");
        if (loader != null)
            loader.setText(data.ToString());
    }
    
    
    void BTDisonnected(object data)
    {
        Debug.Log("BTDisonnected");
        initialDataRecived = false;
        setBTStatus();
    }
    
    
    void BTConnected(object data)
    {
        Debug.Log("BTConnected");
        setBTStatus();
        ReadDeviceData(null);
    }

    
    void ReadDeviceData(object data)
    {
        Debug.Log("ReadDeviceData");
        if (loader == null)
            loader = SceneManager.instance.PushWin(WindowsType.LoaderWin).GetComponent<Loader>();

        loader.setText("Читаем состояние устройства.");
        /*BluetoothConnector.instance.SendData(new byte[] { 0x42, 0x54, 0x2b, 0x0d, 0x0a },
                                             (byte[] obj) =>
                                             {
                                                 deviceParamData = obj;
                                                 initialDataRecived = true;
                                                 if (loader != null)
                                                     SceneManager.instance.PullWin(loader);

                                                 gearBoxBtn.isActive = deviceParamData[10] != 0;
                                                 lockIcon.isActive = deviceParamData[11] != 0;

                                                 setValueStatus();
                                                 string strData = "";
                                                 foreach (var b in obj)
                                                     strData += b.ToString("X2");

                                                 Debug.Log("BTConnected responce = " + strData + " data.Length = " + obj.Length);
                                             }, () =>
                                             {
                                                 BluetoothConnector.instance.DisConnect();
                                                 if (loader != null)
                                                     SceneManager.instance.PullWin(loader);
                                             });*/
    }
    
    
    void BTDataRecived(object data)
    {
        /*byte[] bytes = (byte[])data;

        string strData = "";
        foreach (var b in bytes)
            strData += b.ToString("X2");

        Debug.Log("BTDataRecived = " + strData + " data.Length = " + bytes.Length);

        if (bytes[3] == 0x0A)
        {
            deviceParamData[10] = bytes[4];
            gearBoxBtn.isActive = deviceParamData[10] != 0;
        }
        else if (bytes[3] == 0x0b)
        {
            deviceParamData[11] = bytes[4];
            lockIcon.isActive = deviceParamData[11] != 0;
        }*/
    }
    
    #endregion



    #region Private methods

    void checkConnection()
    {
        Debug.Log("checkConnection IsConnected = " + bluetoothConnector.IsConnected());

        setBTStatus();
        if (!bluetoothConnector.IsConnected())
        {
            if (loader == null)
                loader = SceneManager.instance.PushWin(WindowsType.LoaderWin).GetComponent<Loader>();
            bluetoothConnector.Connect();
        }
    }
    
    
    void setValueStatus()
    {
       /* angleLabel.text = (((int)deviceParamData[leftRightBtn.isActive ? 0 : 1]) / 10.0f).ToString("f1");

        byte corValue = deviceParamData[leftRightBtn.isActive ? 2 : 3];
        if (corValue >= 0x00 && corValue <= 0x32)
            correctLabel.text = (((int)corValue) / 10.0f).ToString("f1");
        else
            correctLabel.text = "-" + (((int)128 - corValue) / 10.0f).ToString("f1");*/
    }
    
    
    void setBTStatus()
    {
        /*if (!bluetoothConnector.IsConnected())
        {
            angleLabel.text = "-.-";
            correctLabel.text = "-.-";
        }
        statusLabel.text = bluetoothConnector.IsConnected() ? "подключен" : "не подключен";
        statusImg.color = bluetoothConnector.IsConnected() ? Color.green : Color.red;*/
    }

    
    byte Clamp(byte value, byte min, byte max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }

    
    void updateHolding(bool forceAdd = false)
    {
        /*if (holdType == HoldType.None ||
            (!isHolding && !forceAdd) )
            return;
            
        lastHoldUpdate += Time.deltaTime;

        float holdTime = Time.time - holdStartTime;
        int changeValue = 0;

        if (lastHoldUpdate >= 0.1f && holdTime > 2)
            changeValue = 1; 
        else if (lastHoldUpdate >= 0.25f && holdTime > 1)
            changeValue = 1;
        else if (lastHoldUpdate >= 0.5f && holdTime < 1)
            changeValue = 1;
        else if(forceAdd)
            changeValue = 1;

        if (changeValue == 0)
            return;
            
        switch (holdType)
        {
            case HoldType.AngleDown:
                if(holdedParamValue > 0)
                    holdedParamValue = Clamp((byte)(holdedParamValue - 1), 0x00, 0xc8);
                angleLabel.text = (((int)holdedParamValue) / 10.0f).ToString("f1");

                break;
            case HoldType.AngleUp:
                if (holdedParamValue < 200)
                    holdedParamValue = Clamp((byte)(holdedParamValue + 1), 0x00, 0xc8);
                angleLabel.text = (((int)holdedParamValue) / 10.0f).ToString("f1");
                break;
            case HoldType.CorrectorMinus:

                if (holdedParamValue == 0x00)
                    holdedParamValue = 0x7f; 
                else if (holdedParamValue <= 50)
                    holdedParamValue = Clamp((byte)(holdedParamValue - 1), 0x00, 0x32);
                else
                    holdedParamValue = Clamp((byte)(holdedParamValue - 1), 0x4e, 0x7f);

                if (holdedParamValue >= 0x00 && holdedParamValue <= 0x32)
                    correctLabel.text = (((int)holdedParamValue) / 10.0f).ToString("f1");
                else
                    correctLabel.text = "-" + (((int)128 - holdedParamValue) / 10.0f).ToString("f1");
                break;
            case HoldType.CorrectorPlus:

                if (holdedParamValue <= 50)
                    holdedParamValue = Clamp((byte)(holdedParamValue + 1), 0x00, 0x32);
                else if (holdedParamValue == 0x7f)
                    holdedParamValue = 0x00;
                else
                    holdedParamValue = Clamp((byte)(holdedParamValue + 1), 0x4e, 0x7f);

                if (holdedParamValue >= 0x00 && holdedParamValue <= 0x32)
                    correctLabel.text = (((int)holdedParamValue) / 10.0f).ToString("f1");
                else
                    correctLabel.text = "-" + (((int)128 - holdedParamValue) / 10.0f).ToString("f1");

                break;
        }

        lastHoldUpdate = 0;*/
    }

    
    void ShopNoConnection()
    {
        /*WarningPopup.push("Ошибка",
                              "Подключение к устройству отсутствует. Проверьте включено ли устройство и попробуйте подключиться.",
                              "Подключиться",
                              () =>
                              {
                                  if (BluetoothAdapter.isBluetoothEnabled())
                                      checkConnection();
                                  else
                                      WarningPopup.push("Подключение",
                                                    "Bluetooth на устройстве выключен. Включите Bluetooth для подключения к контроллеру опускания зеркал",
                                                    "Подключиться",
                                                        () =>
                                                        {
                                                            BluetoothAdapter.askEnableBluetooth();

                                                            if (BluetoothAdapter.isBluetoothEnabled())
                                                                checkConnection();
                                                        });
                              });*/
    }
    
    
    void dropHoldValue()
    {
        if(!isHolding && holdType != HoldType.None)
        {
            setValueStatus();
        }
    }

    #endregion

    
    
    #region button calbacks

    /*public void HoldEnd()
    {
        Debug.Log("HoldEnd");

        isHolding = false;
        Invoke("dropHoldValue", 0.1f);
    }

    
    public void correctorMinusHoldStart()
    {
        Debug.Log("correctorMinusHoldStart");

        if (BluetoothConnector.instance.IsReadyToSend())
        {
            isHolding = true;
            holdType = HoldType.CorrectorMinus;
            holdStartTime = Time.time;
            holdedParamValue = deviceParamData[leftRightBtn.isActive ? 2 : 3];
        }
    }
    
    
    public void correctorPlusHoldStart()
    {
        Debug.Log("correctorPlusHoldStart");
        if (BluetoothConnector.instance.IsReadyToSend())
        {
            isHolding = true;
            holdType = HoldType.CorrectorPlus;
            holdStartTime = Time.time;
            holdedParamValue = deviceParamData[leftRightBtn.isActive ? 2 : 3];
        }
    }
    
    
    public void angleUpHoldStart()
    {
        Debug.Log("angleUpHoldStart");

        if (BluetoothConnector.instance.IsReadyToSend())
        {
            isHolding = true;
            holdType = HoldType.AngleUp;
            holdStartTime = Time.time;
            holdedParamValue = deviceParamData[leftRightBtn.isActive ? 0 : 1];
        }
    }
    
    
    public void angleDownHoldStart()
    {
        Debug.Log("angleDownHoldStart");

        if (BluetoothConnector.instance.IsReadyToSend())
        {
            isHolding = true;
            holdType = HoldType.AngleDown;
            holdStartTime = Time.time;
            holdedParamValue = deviceParamData[leftRightBtn.isActive ? 0 : 1];
        }
    }

    public void angleUpClicked()
    {
        Debug.Log("angleUpClicked");
        if (BluetoothConnector.instance.IsReadyToSend())
        {
            if (deviceParamData[leftRightBtn.isActive ? 0 : 1] < 200) //0xc8 === 200 
            {
                if (Time.time - holdStartTime <= 0.4f)
                    updateHolding(true);

                if (holdType == HoldType.AngleUp)
                    deviceParamData[leftRightBtn.isActive ? 0 : 1] = holdedParamValue;
                BluetoothConnector.instance.SendData(new byte[] { 0x42, 0x54, 0x2b,
                (byte)(leftRightBtn.isActive ? 0x00 : 0x01), deviceParamData[leftRightBtn.isActive ? 0 : 1], 0x0d, 0x0a });
                setValueStatus();
            }
        }
        else if (!BluetoothConnector.instance.IsConnected())
            ShopNoConnection();

        holdType = HoldType.None;
        holdStartTime = 0;
    }
    
    
    public void angleDownClicked()
    {
        Debug.Log("angleDownClicked");
        if (BluetoothConnector.instance.IsReadyToSend())
        {
            if (deviceParamData[leftRightBtn.isActive ? 0 : 1] > 0)
            {
                if (Time.time - holdStartTime <= 0.4f)
                    updateHolding(true);

                if (holdType == HoldType.AngleDown)
                    deviceParamData[leftRightBtn.isActive ? 0 : 1] = holdedParamValue;

                BluetoothConnector.instance.SendData(new byte[] { 0x42, 0x54, 0x2b,
                (byte)(leftRightBtn.isActive ? 0x00 : 0x01), deviceParamData[leftRightBtn.isActive ? 0 : 1], 0x0d, 0x0a });
                setValueStatus();
            }
        }
        else if (!BluetoothConnector.instance.IsConnected())
            ShopNoConnection();
    }

    
    public void correctorMinusClicked()
	{
		Debug.Log ("correctorMinusClicked");
        if (BluetoothConnector.instance.IsReadyToSend())
        {
            if (Time.time - holdStartTime <= 0.4f)
                updateHolding(true);
                
            if (holdType == HoldType.CorrectorMinus)
                deviceParamData[leftRightBtn.isActive ? 2 : 3] = holdedParamValue;
            BluetoothConnector.instance.SendData(new byte[] { 0x42, 0x54, 0x2b,
                (byte)(leftRightBtn.isActive ? 0x02 : 0x03), (byte)holdedParamValue, 0x0d, 0x0a });

            setValueStatus();
        }
        else if (!BluetoothConnector.instance.IsConnected())
            ShopNoConnection();
    }
    
    
	public void correctorPlusClicked()
	{
		Debug.Log ("correctorPlusClicked");
        if (BluetoothConnector.instance.IsReadyToSend())
        {
            if (Time.time - holdStartTime <= 0.4f)
                updateHolding(true);

            if (holdType == HoldType.CorrectorPlus)
                deviceParamData[leftRightBtn.isActive ? 2 : 3] = holdedParamValue;
            BluetoothConnector.instance.SendData(new byte[] { 0x42, 0x54, 0x2b,
                (byte)(leftRightBtn.isActive ? 0x02 : 0x03), holdedParamValue, 0x0d, 0x0a });

            setValueStatus();
        } else if (!BluetoothConnector.instance.IsConnected())
            ShopNoConnection();
    }

    
	public void settingsClicked()
	{
        Debug.Log("settingsClicked");
        if (BluetoothConnector.instance.IsReadyToSend())
            SceneManager.instance.PushWin(WindowsType.SettingsWin).GetComponent<SettingsWin>().setData(deviceParamData);
        else if(!BluetoothConnector.instance.IsConnected())
            ShopNoConnection();
	}
    
    
	public void helpClicked()
	{
		Debug.Log ("helpClicked");
        SceneManager.instance.PushWin(WindowsType.InfoWindow);
    }

    
    public void lockClicked(bool b)
    {
        Debug.Log("lockClicked");
        if (!BluetoothConnector.instance.IsReadyToSend())
        {
            lockIcon.isActive = !b;
            if (!BluetoothConnector.instance.IsConnected())
                ShopNoConnection();
        }
    }
    
    
    public void gearClicked(bool b)
	{
		Debug.Log ("gearClicked");
        if (BluetoothConnector.instance.IsReadyToSend())
        {
            bool currentValue = (0x01 & deviceParamData[10]) != 0;

            int res = 0;

            if (!currentValue)
                res = res | 0x01;

            deviceParamData[10] = (byte)res;
            BluetoothConnector.instance.SendData(new byte[] { 0x42, 0x54, 0x2b, 0x0b, deviceParamData[10], 0x0d, 0x0a }, null, () =>
             {
                 gearBoxBtn.isActive = !b;
             });
        }
        else
        {
            gearBoxBtn.isActive = !b;
            if (!BluetoothConnector.instance.IsConnected())
                ShopNoConnection();
        }
    }

    
    public void sideChanged(bool b)
    {
        Debug.Log("sideChanged");
        if (!BluetoothConnector.instance.IsReadyToSend())
        {
            leftRightBtn.isActive = !b;
            if (!BluetoothConnector.instance.IsConnected())
                ShopNoConnection();
        }else
            setValueStatus();
    }*/
    
    #endregion
}
