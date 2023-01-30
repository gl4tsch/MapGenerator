using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    // Maps a value from ome arbitrary range to another arbitrary range
    public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}
