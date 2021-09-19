using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Meter : MonoBehaviour
{
    public float minVal;
    public float maxVal;

    protected float curVal;
    protected float normalizedVal;

    public float Value {
        get { return curVal; }
        set 
        { 
            curVal = Mathf.Clamp(value, minVal, maxVal); 
            normalizedVal = Helpers.Remap(curVal, minVal, maxVal, 0, 1); 
            UpdateValue(); 
        }
    }

    protected abstract void UpdateValue();
}
