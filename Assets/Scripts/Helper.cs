using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    // Maps a value from ome arbitrary range to another arbitrary range
    public static float Map(this float value, float leftMin, float leftMax, float rightMin, float rightMax)
    {
        return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
    }
}
