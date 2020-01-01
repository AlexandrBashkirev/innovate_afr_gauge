using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class WinPathAttribute : System.Attribute
{
    public string path { get; set; }

    public WinPathAttribute(string _path)
    {
        path = _path;
    }
}
