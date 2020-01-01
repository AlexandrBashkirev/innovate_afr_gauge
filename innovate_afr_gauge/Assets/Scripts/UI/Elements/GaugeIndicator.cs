using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaugeIndicator : MonoBehaviour
{
    private RectTransform rt;
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void SetAngle(float angel)
    {
        rt.Rotate(Vector3.up, angel);
    }
}
