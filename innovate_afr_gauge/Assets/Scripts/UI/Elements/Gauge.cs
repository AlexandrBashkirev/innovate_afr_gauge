using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Gauge : MonoBehaviour
{
    #region Fields

    [SerializeField] private GaugeIndicator mainIndicator;
    [SerializeField] private GaugeIndicator minIndicator;
    [SerializeField] private GaugeIndicator maхIndicator;

    [SerializeField] private Text valueLabel;

    [SerializeField] private float minAngle = 180.0f;
    [SerializeField] private float maxAngle = -90.0f;
    
    [SerializeField] private float minValue = 8.0f;
    [SerializeField] private float maxValue = 18.0f;

    #endregion



    #region Methods

    public void SetValue(float value)
    {
        value = Mathf.Clamp(value, minValue, maxValue);

        valueLabel.text = value.ToString("F2");

        float t = (value - minValue) / (maxValue - minValue);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        
        mainIndicator.SetAngle(angle);
    }


    public void MinMaxDrop()
    {
        
    }

    #endregion

}
