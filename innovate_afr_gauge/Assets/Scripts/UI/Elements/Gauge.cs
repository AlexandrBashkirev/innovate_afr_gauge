using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gauge : MonoBehaviour
{
    #region Fields

    [SerializeField] private GaugeIndicator mainIndicator;
    [SerializeField] private GaugeIndicator minIndicator;
    [SerializeField] private GaugeIndicator maхIndicator;

    [SerializeField] private float minAngle = 180.0f;
    [SerializeField] private float maxAngle = -90.0f;
    
    [SerializeField] private float minValue = 8.0f;
    [SerializeField] private float maxValue = 18.0f;

    #endregion

}
