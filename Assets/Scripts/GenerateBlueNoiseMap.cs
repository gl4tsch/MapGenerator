using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBlueNoiseMap : MonoBehaviour {

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

    // Use this for initialization
    void Start () {
        var startTime = System.DateTime.Now;
        var timer = System.DateTime.Now - startTime;
        if (DrawCells)
        {
            cell.GetComponent<Renderer>().enabled = true;
        }
        else
        {
            cell.GetComponent<Renderer>().enabled = false;
        }
        Debug.Log("Start " + timer);

        points = PoissonDiscSampling.GenerateSeedPoints(0.5f, 2f, 30, 6f, mainCamera);
        timer = System.DateTime.Now - startTime;
        Debug.Log("SeedPoints Done " + timer);

        //triangles = Triangulation.ConvexHullTriangulation(points,scaffolding); //TODO: implement hull triangulation
        triangles = Triangulation.IncrementalTriangulation(points);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Triangulation Done " + timer);

        triangles = DelaunayTriangulation.MakeTriangulationDelaunay(triangles);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Delaunayification Done " + timer);

        triangles = DelaunayTriangulation.FillInNeighbours(triangles);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Neighbours Done " + timer);

        points = HeightField.PerlinIsland(points, 0.1f, 0.7f, 4f, 3f, 6); // fill heightmap into y coordinates
        timer = System.DateTime.Now - startTime;
        Debug.Log("Heightmap Done " + timer);

        triangles = Voronoi.GenerateCentroids(triangles, points);
        timer = System.DateTime.Now - startTime;
        Debug.Log("Centroids Done " + timer);

        CellMeshCreator.SpawnMeshes(points, cell);
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
    }
}
