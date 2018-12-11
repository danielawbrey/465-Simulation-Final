using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformConfigurationData
{
    public int M = 16;
    public int N = 9;

    public float deltaSpace = 0.1f;
    public float RandomHeight = 1.0f;

    public override string ToString()
    {
        return string.Format("{0},{1},{2},{3}",M,N,deltaSpace,RandomHeight);
    }
}
