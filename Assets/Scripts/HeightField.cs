using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightField
{
    public static List<Vertex> LiftPlateaus(List<Vertex> points, int minNumberPlateaus, int maxNumberPlateaus, int plateauSize)
    {
        for (int i = 0; i < Random.Range(minNumberPlateaus, maxNumberPlateaus); i++) // # plateaus
        {
            int rndPoint = Random.Range(0, points.Count - 1);
            var nn = new List<Vertex>();
            points[rndPoint].NeighboursNeighbours(plateauSize-1, 0, ref nn);

            foreach (Vertex v in nn)
            {
                v.once = false;
                v.position.y += 0.3f;
            }
        }
        return points;
    }

    public static List<Vertex> PerlinIsland(List<Vertex> points, float mapSize, float yShift, float edgeDown, float dropOffScale, float frequency, int numElevationLevels)
    {
        float a = yShift;               // y shift
        float b = edgeDown;             // push edges down
        float c = dropOffScale;         // drop off scale
        float d = 0f;                   // distance from center
        float f = frequency;            // frequency
        float e = numElevationLevels;   // # elevation levels
        var r = Random.Range(0, 1000);  // random offset

        foreach(Vertex p in points)
        {
            if (p.isScaffolding)
                continue;
            var nx = (p.position.x - (mapSize / 2)) / mapSize; // normalized position relative to center
            var nz = (p.position.z - (mapSize / 2)) / mapSize;
            d = 2 * Mathf.Max(Mathf.Abs(nx), Mathf.Abs(nz)); // Manhatten Distance
            var h = Mathf.PerlinNoise(p.position.x / mapSize * f + r, p.position.z / mapSize * f + r);
            //p.position.y = e;
            h = (h + a) * (1 - b * Mathf.Pow(d, c));
            p.position.y = Mathf.Round(h * e) / e; // round to fix number of elevation levels
        }
        return points;
    }

    // new format
    public static List<float> PerlinIslands(List<Vector2> points, float mapSize, float yShift, float edgeDown, float dropOffScale, float frequency, int numElevationLevels, int seed)
    {
        Random.InitState(seed);

        float a = yShift;               // y shift
        float b = edgeDown;             // push edges down
        float c = dropOffScale;         // drop off scale
        float d = 0f;                   // normalized distance from center
        float f = frequency;            // frequency
        float e = numElevationLevels;   // # elevation levels
        var r = Random.Range(0, 1000);  // random offset

        List<float> outPointHeights = new List<float>();

        float maxH = Mathf.NegativeInfinity;
        foreach (var p in points)
        {
            var nx = p.x / mapSize; // normalized position relative to center
            var nz = p.y / mapSize;
            d = 2 * Mathf.Sqrt(nx * nx + nz * nz); //2 * Mathf.Max(Mathf.Abs(nx), Mathf.Abs(nz)); // Manhatten Distance
            var h = Mathf.PerlinNoise(p.x / mapSize * f + r, p.y / mapSize * f + r);
            h = (h + a) * (1 - b * Mathf.Pow(d, c));
            // add unrounded for now
            maxH = Mathf.Max(maxH, h);
            outPointHeights.Add(h);
        }
        for (int i = 0; i < outPointHeights.Count; i++)
        {
            float h = outPointHeights[i];
            h = h.Map(0, maxH, 0, 1);
            h = Mathf.Clamp01(h);
            h = Mathf.Round(h * e) / e; // round to fix number of elevation levels
            outPointHeights[i] = h;
        }
        return outPointHeights;
    }

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

    }
}
