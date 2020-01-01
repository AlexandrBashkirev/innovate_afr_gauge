using System;
using System.Collections;
using System.Collections.Generic;
using TechTweaking.Bluetooth;
using UnityEngine.UI;
using UnityEngine;
using Zenject;


[WinPathAttribute("Prefabs/Windows/MainWindow")]
public class MainWindow : Window , IAfrControllerDelegate
{
    #region Fields

    [Inject] private IAfrController afrController;

    [SerializeField] private Image connectionStatusIcon;
    [SerializeField] private Text connectionStatusLabel;
    
    [SerializeField] private Gauge gauge;
    
    [SerializeField] private Text afrStatusLabel;

    private Loader loader = null;

    #endregion



    #region Unity lifecicle

    public override void Start()
    {
        base.Start();
        
        afrController.Initialize();
        afrController.Subscribe(this);
    }


    private void OnDestroy()
    {
        afrController.UnSubscribe(this);
    }

    
    void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log("OnApplicationFocus hasFocus = " + hasFocus);

        if (hasFocus)
        {
            afrController.TryConnect();
        }
    }
    
    #endregion
    


    #region Private methods

    void ShowNoConnection()
    {
        WarningPopup.push("Ошибка",
                              "Подключение к устройству отсутствует. Проверьте включено ли устройство и попробуйте подключиться.",
                              "Подключиться",
                              () =>
                              {
                                  if (BluetoothAdapter.isBluetoothEnabled())
                                      afrController.TryConnect();
                                  else
                                      WarningPopup.push("Подключение",
                                                    "Bluetooth на устройстве выключен. Включите Bluetooth для подключения к контроллеру опускания зеркал",
                                                    "Подключиться",
                                                        () =>
                                                        {
                                                            BluetoothAdapter.askEnableBluetooth();

                                                            if (BluetoothAdapter.isBluetoothEnabled())
                                                                afrController.TryConnect();
                                                        });
                              });
    }

    #endregion



    #region Afr delegate

    public void StartConnection()
    {
        if (loader == null)
        {
            loader = SceneManager.Instance.PushWin<Loader>();
        }
    }
    
    
    public void ConnectionInfo(string info)
    {
        if (loader != null)
        {
            loader.setText(info);
        }
    }
    
    
    public void Connected(bool isConnected)
    {
        SceneManager.Instance.PullWin(loader);
        
        if (!isConnected)
        {
            ShowNoConnection();
        }
    }
    
    
    public void UpdateConnectionStatus(bool isConnected)
    {
        connectionStatusIcon.color = isConnected ? Color.green : Color.red;
        connectionStatusLabel.text = isConnected ? "Connected" : "Not connected";

        if (!isConnected)
        {
            afrStatusLabel.text = "";
            gauge.MinMaxDrop();
            gauge.SetValue(0.0f);
        }
    }

    
    public void AfrRecived(AfrData afr)
    {
        if (afr.status == AfrStatus.AfrValid ||
            afr.status == AfrStatus.O2LevelValid)
        {
            gauge.SetValue(afr.afr);
        }
        else
        {
            switch (afr.status)
            {
                case AfrStatus.HeaterCalib:
                    afrStatusLabel.text = "Heater Calibration...";
                    break;
                case AfrStatus.WarmingUp:
                    afrStatusLabel.text = "Warming up...";
                    break;
                case AfrStatus.Error:
                    afrStatusLabel.text = "Unexpected Error";
                    break;
                case AfrStatus.FreeAirCalib:
                    afrStatusLabel.text = "AIR Calibration...";
                    break;
                case AfrStatus.NeedFreeAirCalib:
                    afrStatusLabel.text = "Need Free air Calibration";
                    break;
            }
        }
    }

    #endregion
    
    
    
    #region button calbacks

    public void MinMaxDrop()
    {
        gauge.MinMaxDrop();
    }
    
    #endregion
}
