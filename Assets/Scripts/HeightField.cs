using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeightField
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="points">expects points centered around 0,0</param>
    /// <param name="frequency"></param>
    /// <param name="edgeDown"></param>
    /// <param name="dropOffScale"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static List<float> PerlinHeights(List<Vector2> points, float mapRadius, float frequency = 0.1f, float edgeDropOffset = 0.5f, float dropExponent = 3f, int? seed = null)
    {
        List<float> outPointHeights = new();

        if (seed.HasValue)
        {
            Random.InitState(seed.Value);
        }

        float offsetX = Random.Range(100, 10000);
        float offsetY = Random.Range(100, 10000);

        foreach (Vector2 p in points)
        {
            float sampleX = (p.x + offsetX) * frequency;
            float sampleY = (p.y + offsetY) * frequency;
            float h = Mathf.PerlinNoise(sampleX, sampleY);
            h = Mathf.Clamp01(h);
            float normDistFromCenter = p.magnitude / mapRadius;
            float offsetDist = (normDistFromCenter - edgeDropOffset) / (1 - edgeDropOffset);
            offsetDist = Mathf.Clamp01(offsetDist);
            float dropOffFactor = 1 - Mathf.Pow(offsetDist, dropExponent);
            h *= dropOffFactor;

            outPointHeights.Add(h);
        }

        return outPointHeights;
    }

    public static void Discretize(ref List<float> heights, int numElevationLevels, bool fillZeroToOneRange = true)
    {
        float maxH = heights.Max();
        float minH = heights.Min();

        for (int i = 0; i < heights.Count; i++)
        {
            float h = heights[i];
            h = h.Map(minH, maxH, 0, 1);
            h = Mathf.Round(h * numElevationLevels) / numElevationLevels;
            heights[i] = h;
        }
    }
}
