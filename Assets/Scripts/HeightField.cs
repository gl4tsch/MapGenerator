using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeightField
{
    /// <param name="points">expects points centered around 0,0</param>
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
            float sampleX = p.x * frequency + offsetX;
            float sampleY = p.y * frequency + offsetY;
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

    public static void Discretize(List<float> heights, int numElevationLevels, bool fillZeroToOneRange = true)
    {
        if (numElevationLevels <= 0)
        {
            return;
        }

        if (fillZeroToOneRange)
        {
            float maxH = heights.Max();
            float minH = heights.Min();

            for (int i = 0; i < heights.Count; i++)
            {
                float h = heights[i];
                h = h.Map(minH, maxH, 0, 1);
                heights[i] = h;
            }
        }

        for (int i = 0; i < heights.Count; i++)
        {
            float h = heights[i];
            h = Mathf.Round(h * numElevationLevels) / numElevationLevels;
            heights[i] = h;
        }
    }
}
