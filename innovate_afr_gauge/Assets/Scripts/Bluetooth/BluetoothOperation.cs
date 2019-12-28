using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BluetoothOperation {

    public string request { get; private set; }

    string tmpRespose = "";
    public string response { get; private set; }

    public byte[] responseByte {
        get{
            if (isOperation)
                Debug.Log("BluetoothOperation Error. Trying get responseByte before operation completed");

            return Enumerable.Range(0, response.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(response.Substring(x, 2), 16))
                     .ToArray();
        }
    }

    bool isOperation = true;
    public bool isSuccess { get; private set; }

    public BluetoothOperation(string _deviceAddress, 
                              string ServiceUUID, 
                              string CharacteristicUUID,
                              byte[] data)
    {
        request = bytesToStr(data);

        Debug.Log("WriteCharacteristic request = " + request);
        isOperation = true;
        isSuccess = false;
        BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, ServiceUUID, CharacteristicUUID, data, data.Length, true, (str)=>{
        });
    }

    public void addResponce(byte[] data)
    {
        tmpRespose += bytesToStr(data);

        Debug.Log("addResponce tmpRespose = " + tmpRespose);

        int endIndx = tmpRespose.IndexOf("4F4B210A", System.StringComparison.Ordinal); // 4F4B210A == OK!\n
        if (endIndx > 0)
        {
            response = tmpRespose.Substring(0, endIndx);
            if (response.Contains(request))
                response = response.Substring(request.Length, response.Length - request.Length);

            Debug.Log("BluetoothOperation end Successful tmpRespose = " + tmpRespose + " response = " + response);

            isOperation = false;
            isSuccess = true;
        }else if(tmpRespose.Length > 40 || tmpRespose.Contains("4552524F523A")) // 4552524F523A == ERROR
        {
            isOperation = false;
            isSuccess = false;

            Debug.Log("BluetoothOperation end Error tmpRespose = " + tmpRespose);
        }
    }

    public IEnumerator WaitFor()
    {
        while (isOperation)
            yield return null;
    }

    string bytesToStr(byte[] bytes)
    {
        string data = "";
        foreach (var b in bytes)
            data += b.ToString("X2");

        return data;
    }
}
