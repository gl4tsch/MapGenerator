using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBlueNoiseMap : MonoBehaviour {

    public float mapSize;
    public float rMin;
    public float rMax;

    public bool DrawSites;
    public bool DrawDelaunayTriangulation;
    public bool DrawCentroids;
    public bool DrawCentroidEdges;
    public bool DrawCells;

    public GameObject dot;
    public GameObject centroid;
    public GameObject cell;
    public GameObject mainCamera;

    public List<Vertex> points;
    public List<Triangle> triangles;
    public List<Vector3> centroids;
    public List<GameObject> cells;

    // Use this for initialization
    void Start () {
        var startTime = System.DateTime.Now;
        var timer = System.DateTime.Now - startTime;
        Debug.Log("Start " + timer);
        
        points = GenerateSeedPoints.PoissonDiscSampling(rMin, rMax, 30, mapSize, mainCamera);
        List<Vertex> scaffolding = GenerateSeedPoints.Scaffolding(mapSize);
        points.AddRange(scaffolding);
        timer = System.DateTime.Now - startTime;
        Debug.Log("SeedPoints Done " + timer);

        List<Triangle> convexPoly = Triangulation.TriangulateConvexPolygon(scaffolding);
        triangles = Triangulation.TriangleSplittingAlgorithm(points, convexPoly);
        //triangles = Triangulation.IncrementalAlgorithm(points);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Triangulation Done " + timer);

        triangles = DelaunayTriangulation.MakeTriangulationDelaunay(triangles);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Delaunayification Done " + timer);

        triangles = DelaunayTriangulation.FillInNeighbours(triangles);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Neighbours Done " + timer);

        // fill heightmap into y coordinates
        points = HeightField.PerlinIsland(points, mapSize, 0.1f, 0.7f, 4f, 3f, 6);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Heightmap Done " + timer);

        triangles = Voronoi.GenerateCentroids(triangles, points);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Centroids Done " + timer);

        cells = CellMeshCreator.SpawnMeshes(points, cell);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Meshes Done " + timer);

        DebugDraw();
        timer = System.DateTime.Now - startTime;
        Debug.Log("Finished " + timer);
    }

    private void DebugDraw()
    {
        foreach (Vertex v in points)
        {
            if (DrawSites)
                Instantiate(dot, v.position, Quaternion.identity, transform);

            for (int i = 0; i < v.Centroids.Count; i++)
            {
                if (DrawCentroids)
                    Instantiate(centroid, v.position + v.Centroids[i], Quaternion.identity, transform);
                if (DrawCentroidEdges)
                    Debug.DrawLine(v.Centroids[i] + v.position, v.Centroids[(i + 1) % v.Centroids.Count] + v.position, Color.yellow, float.PositiveInfinity);
            }
        }

        if (DrawDelaunayTriangulation)
        {
            foreach (Triangle t in triangles)
            {
                Debug.DrawLine(t.v1.position, t.v2.position, Color.red, float.PositiveInfinity);
                Debug.DrawLine(t.v2.position, t.v3.position, Color.red, float.PositiveInfinity);
                Debug.DrawLine(t.v3.position, t.v1.position, Color.red, float.PositiveInfinity);
            }
        }

        if (!DrawCells)
        {
            foreach (GameObject c in cells)
            {
                c.GetComponent<Renderer>().enabled = false;
            }
        }
    }
}
