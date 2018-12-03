using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Voronoi {

	public static List<Triangle> GenerateCentroids(List<Triangle> triangles, List<Vertex> points)
    {
        foreach (Triangle t in triangles)
        {
            Vector3 v = t.CalculateCentroid();
            t.v1.Centroids.Add(v + new Vector3(0, t.v1.position.y, 0) - t.v1.position); // add centroids with hight of site in local space of site
            t.v2.Centroids.Add(v + new Vector3(0, t.v2.position.y, 0) - t.v2.position);
            t.v3.Centroids.Add(v + new Vector3(0, t.v3.position.y, 0) - t.v3.position);
        }
        foreach (Vertex p in points)
        {
            p.Centroids = p.Centroids.OrderBy(c => Mathf.Atan2(c.x, c.z)).ToList(); // sort centroids clockwise
        }
        return triangles;
    }
}
