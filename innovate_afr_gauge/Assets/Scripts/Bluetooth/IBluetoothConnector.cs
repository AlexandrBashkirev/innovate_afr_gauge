using System;


public interface IBluetoothConnector
{
    void Connect();
    void DisConnect();
    bool IsConnected();
    bool IsReadyToSend();
    void SendData(byte[] data, Action<byte[]> response = null, Action error = null);
}
