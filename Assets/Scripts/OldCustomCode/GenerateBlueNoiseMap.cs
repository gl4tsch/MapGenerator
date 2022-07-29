using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public List<Vertex> points;
    public List<Triangle> triangles;
    public List<Vector3> centroids;
    public List<GameObject> cells;

    private void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        var startTime = DateTime.Now;
        
        points = BlueNoiseGenerator.PoissonDiscSampling(rMin, rMax, 30, mapSize);
        List<Vertex> scaffolding = BlueNoiseGenerator.Scaffolding(mapSize);
        points.AddRange(scaffolding);

        var duration = DateTime.Now - startTime;
        Debug.Log("SeedPoints Done in " + duration.TotalSeconds + "s");
        startTime = DateTime.Now;

        List<Triangle> convexPoly = Triangulation.TriangulateConvexPolygon(scaffolding);
        triangles = Triangulation.TriangleSplittingAlgorithm(points, convexPoly);
        //triangles = Triangulation.IncrementalAlgorithm(points);

        duration = DateTime.Now - startTime;
        Debug.Log("Triangulation Done in " + duration.TotalSeconds + "s");
        startTime = DateTime.Now;

        triangles = DelaunayTriangulation.MakeTriangulationDelaunay(triangles);

        duration = DateTime.Now - startTime;
        Debug.Log("Delaunayification Done in " + duration.TotalSeconds + "s");
        startTime = DateTime.Now;

        triangles = DelaunayTriangulation.FillInNeighbours(triangles);

        duration = DateTime.Now - startTime;
        Debug.Log("Neighbours Done in " + duration.TotalSeconds + "s");
        startTime = DateTime.Now;

        // fill heightmap into y coordinates
        points = HeightField.PerlinIsland(points, mapSize, 0.1f, 0.7f, 4f, 3f, 6);

        duration = DateTime.Now - startTime;
        Debug.Log("Heightmap Done in " + duration.TotalSeconds + "s");
        startTime = DateTime.Now;

        triangles = Voronoi.GenerateCentroids(triangles, points);

        duration = DateTime.Now - startTime;
        Debug.Log("Centroids Done in " + duration.TotalSeconds + "s");
        startTime = DateTime.Now;

        cells = CellMeshCreator.SpawnMeshes(points, cell);

        duration = DateTime.Now - startTime;
        Debug.Log("Meshes Done in " + duration.TotalSeconds + "s");

        var camHeight = mapSize * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.transform.position = new Vector3(mapSize / 2, camHeight * 1.1f, mapSize / 2);

        //DebugDraw();
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
