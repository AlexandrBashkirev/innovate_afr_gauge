using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "AppInstaller")]
public class AppInstaller : ScriptableObjectInstaller<AppInstaller>
{

    [SerializeField] private GameObject bluetoothConnectorPrefab;
    
    public override void InstallBindings()
    {
        GameObject bluetoothConnector = GameObject.Instantiate(bluetoothConnectorPrefab);
        Container.Bind<IBluetoothConnector>().FromInstance(bluetoothConnector.GetComponent<BluetoothConnector>()).AsSingle();
    }
}
