using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saves  {

    public static string getPiredDeviceMAC()
    {
        return PlayerPrefs.GetString("PiredDeviceMAC", "");
    }
    public static void setPiredDeviceMAC(string mac)
    {
        PlayerPrefs.SetString("PiredDeviceMAC", mac);
    }
}
