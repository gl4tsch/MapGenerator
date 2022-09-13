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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="points"></param>
    /// <param name="mapSize"></param>
    /// <param name="yShift"></param>
    /// <param name="edgeDown"></param>
    /// <param name="dropOffScale"></param>
    /// <param name="frequency"></param>
    /// <param name="numElevationLevels"></param>
    /// <returns>height list corresponding to input points</returns>
    public static List<float> PerlinIsland(List<Vector2> points, float mapSize, float yShift, float edgeDown, float dropOffScale, float frequency, int numElevationLevels)
    {
        float a = yShift;               // y shift
        float b = edgeDown;             // push edges down
        float c = dropOffScale;         // drop off scale
        float d = 0f;                   // distance from center
        float f = frequency;            // frequency
        float e = numElevationLevels;   // # elevation levels
        var r = Random.Range(0, 1000);  // random offset

        List<float> outPointHeights = new List<float>();

        foreach (var p in points)
        {
            var nx = p.x / mapSize; // normalized position relative to center
            var nz = p.y / mapSize;
            d = Mathf.Sqrt(nx * nx + nz * nz); //2 * Mathf.Max(Mathf.Abs(nx), Mathf.Abs(nz)); // Manhatten Distance
            var h = Mathf.PerlinNoise(p.x / mapSize * f + r, p.y / mapSize * f + r);
            //p.position.y = e;
            h = (h + a) * (1 - b * Mathf.Pow(d, c));
            h = Mathf.Round(h * e) / e; // round to fix number of elevation levels
            outPointHeights.Add(h);
        }
        return outPointHeights;
    }
}
