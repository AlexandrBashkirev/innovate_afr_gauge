using System;

public enum AfrStatus
{
    None,
    AfrValid,            // Lambda valid and Aux data valid, normal operation.
    O2LevelValid,        // Lambda value contains O2 level in 1/10%
    FreeAirCalib,        // Free air Calib in progress, Lambda data not valid
    NeedFreeAirCalib,    // Need Free air Calibration Request, Lambda data not valid
    WarmingUp,           // Warming up, Lambda value is temp in 1/10% of operating temp.
    HeaterCalib,         // Heater Calibration, Lambda value contains calibration countdown.
    Error,               // Error code in Lambda value
}


public struct AfrData
{
    public float afr;
    public float o2level;
    public AfrStatus status;
}


public interface IAfrController
{
    void Initialize();

    void Subscribe(IAfrControllerDelegate controllerDelegate);
    void UnSubscribe(IAfrControllerDelegate controllerDelegate);
    void TryConnect();
    bool IsConnected();
}


public interface IAfrControllerDelegate
{
    void StartConnection();
    void ConnectionInfo(string info);
    void Connected(bool isConnected);
    void UpdateConnectionStatus(bool isConnected);
    void AfrRecived(AfrData afr);

}
