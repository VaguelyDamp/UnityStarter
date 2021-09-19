using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMeter : Meter
{
    public GameObject needle;

    public Transform lowPoint;
    public Transform highPoint;

    protected override void UpdateValue()
    {
        needle.transform.position = Vector3.Lerp(lowPoint.position, highPoint.position, normalizedVal);
        needle.transform.rotation = Quaternion.Lerp(lowPoint.rotation, highPoint.rotation, normalizedVal);
    }
}
