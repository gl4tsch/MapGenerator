using DelaunatorSharp;
using DelaunatorSharp.Unity;
using DelaunatorSharp.Unity.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float mapSize = 10;
    [SerializeField] float rMin = 0.2f;

    [Header("Debug Visuals")]
    [SerializeField] Material lineMaterial;

    [SerializeField] bool drawDelaunayPoints;
    [SerializeField] bool drawDelaunayEdges;
    [SerializeField] GameObject delaunayPointPrefab;
    [SerializeField] Color delaunayEdgeColor = Color.red;

    [SerializeField] bool drawVoronoiPoints;
    [SerializeField] bool drawVoronoiEdges;
    [SerializeField] GameObject voronoiPointPrefab;
    [SerializeField] Color voronoiEdgeColor = Color.cyan;

    Transform delaunayVisualsContainer, voronoiVisualContainer;

    Delaunator delaunator; // data in 2D

    private void Start()
    {
        Clear();
        GenerateMap(mapSize, rMin);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Clear();
            GenerateMap(mapSize, rMin);
        }
    }

    void Clear()
    {
        delaunator = null;

        if (delaunayVisualsContainer != null)
        {
            Destroy(delaunayVisualsContainer.gameObject);
        }

        if(voronoiVisualContainer != null)
        {
            Destroy(voronoiVisualContainer.gameObject);
        }
    }

    public void GenerateMap(float mapSize, float delaunayRMin)
    {
        var blueNoisePoints = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, mapSize / 2, delaunayRMin);
        delaunator = new Delaunator(blueNoisePoints.ToPoints());

        DrawDelaunay();
        DrawVoronoi();

        var delaunatorCells = delaunator.GetVoronoiCells();

        //foreach(var cell in delaunatorCells)
        //{
        //    var pos = Delaunator.GetCentroid(cell.Points);
        //    Instantiate(delaunayPointPrefab, new Vector3((float)pos.X, 0, (float)pos.Y), Quaternion.identity);

        //    foreach(var point in cell.Points)
        //    {
        //        Instantiate(voronoiPointPrefab, new Vector3((float)point.X, 0, (float)point.Y), Quaternion.identity);
        //    }
        //}

        var camHeight = mapSize * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.transform.position = new Vector3(0, camHeight * 1.1f, 0);
    }

    void DrawDelaunay()
    {
        if (delaunator == null)
            return;

        if(delaunayVisualsContainer == null)
        delaunayVisualsContainer = new GameObject("Delaunay Visuals").transform;

        delaunator.ForEachTriangleEdge(edge =>
        {
            if (drawDelaunayPoints)
            {
                CreateLine(delaunayVisualsContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { new Vector3((float)edge.P.X, 0, (float)edge.P.Y), new Vector3((float)edge.Q.X, 0, (float)edge.Q.Y) }, delaunayEdgeColor, 0.1f, 0);
            }

            if (drawDelaunayEdges)
            {
                var pointGameObject = Instantiate(delaunayPointPrefab, new Vector3((float)edge.P.X, 0, (float)edge.P.Y), Quaternion.identity, delaunayVisualsContainer);
            }
        });
    }

    void DrawVoronoi()
    {
        if (delaunator == null)
            return;

        if(voronoiVisualContainer == null)
            voronoiVisualContainer = new GameObject("Voronoi Visuals").transform;

        delaunator.ForEachVoronoiEdge(edge =>
        {
            if (drawVoronoiEdges)
            {
                CreateLine(voronoiVisualContainer, $"Voronoi Edge", new Vector3[] { new Vector3((float)edge.P.X, 0, (float)edge.P.Y), new Vector3((float)edge.Q.X, 0, (float)edge.Q.Y) }, voronoiEdgeColor, 0.1f, 2);
            }
            if (drawVoronoiPoints)
            {
                var pointGameObject = Instantiate(voronoiPointPrefab, new Vector3((float)edge.P.X, 0, (float)edge.P.Y), Quaternion.identity, voronoiVisualContainer);
            }
        });
    }

    void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }
}
