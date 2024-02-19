using DelaunatorSharp;
using DelaunatorSharp.Unity;
using DelaunatorSharp.Unity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int seed;
    public int Seed { get => seed; set => seed = value; }
    [SerializeField] float mapSize = 40;
    public float MapSize { get => mapSize; set => mapSize = value; }
    [SerializeField] float poissonDiscRadius = 1;
    public float PoissonDiscRadius { get => poissonDiscRadius; set => poissonDiscRadius = value; }

    [Header("Visuals")]
    [SerializeField] bool drawDelaunayPoints;
    [SerializeField] bool drawDelaunayEdges;
    [SerializeField] GameObject delaunayPointPrefab;
    [SerializeField] Material delaunayEdgeMat;

    [SerializeField] bool drawVoronoiPoints;
    [SerializeField] bool drawVoronoiEdges;
    [SerializeField] GameObject voronoiPointPrefab;
    [SerializeField] Material voronoiEdgeMat;

    [Header("Cell")]
    [SerializeField] CellBehaviour cellPrefab;
    [SerializeField] bool drawCells;
    [SerializeField] bool showNeighbourCounts;
    public bool ShowNeighbourCounts
    {
        get => showNeighbourCounts;
        set
        {
            showNeighbourCounts = value;
            ToggleDraw();
        }
    }

    Transform delaunayPointsContainer, delaunayEdgesContainer, voronoiPointsContainer, voronoiEdgesContainer, cellsContainer;
    Map map = null;
    List<CellBehaviour> cellInstances = new();

    private void Awake()
    {
        RandomizeSeed();
        GenerateMap();
    }

    void Clear()
    {
        cellInstances.Clear();
        if (delaunayPointsContainer != null)
        {
            Destroy(delaunayPointsContainer.gameObject);
            delaunayPointsContainer = null;
        }
        if (delaunayEdgesContainer != null)
        {
            Destroy(delaunayEdgesContainer.gameObject);
            delaunayEdgesContainer = null;
        }
        if (voronoiPointsContainer != null)
        {
            Destroy(voronoiPointsContainer.gameObject);
            voronoiPointsContainer = null;
        }
        if (voronoiEdgesContainer != null)
        {
            Destroy(voronoiEdgesContainer.gameObject);
            voronoiEdgesContainer = null;
        }
        if (cellsContainer != null)
        {
            Destroy(cellsContainer.gameObject);
            cellsContainer = null;
        }
    }

    public int RandomizeSeed()
    {
        //seed = (int)DateTime.Now.Ticks;
        seed = Random.Range(int.MinValue, int.MaxValue);
        return seed;
    }

    public void GenerateMap()
    {
        Clear();
        Random.InitState(seed);
        map = GenerateMap(mapSize, poissonDiscRadius);
        SpawnCells(map);
        ToggleDraw();
    }

    Map GenerateMap(float mapSize, float poissonRadius)
    {
        List<Vector2> blueNoisePoints = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, mapSize / 2, poissonRadius);
        Delaunator delaunator = new Delaunator(blueNoisePoints.ToPoints());
        // fill heightmap
        List<float> pointHeights = HeightField.PerlinIslands(blueNoisePoints, mapSize, 0f, .7f, 4f, 4f, 6);
        return new Map(blueNoisePoints, delaunator, pointHeights);
    }

    void SpawnDelaunay(Map map)
    {
        if (delaunayPointsContainer == null)
        {
            delaunayPointsContainer = new GameObject("Delaunay Points").transform;
        }

        foreach (var point in map.BlueNoisePoints)
        {
            Instantiate(delaunayPointPrefab, new Vector3(point.x, 0, point.y), Quaternion.identity, delaunayPointsContainer);
        }

        if (delaunayEdgesContainer == null)
        {
            delaunayEdgesContainer = new GameObject("Delaunay Edges").transform;
        }

        map.Delaunator.ForEachTriangleEdge(edge =>
        {
            CreateLine(delaunayEdgesContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { new Vector3((float)edge.P.X, 0, (float)edge.P.Y), new Vector3((float)edge.Q.X, 0, (float)edge.Q.Y) }, delaunayEdgeMat, 0.1f, 0);
        });
    }

    void SpawnVoronoi(Map map)
    {
        if (voronoiPointsContainer == null)
        {
            voronoiPointsContainer = new GameObject("Voronoi Points").transform;
        }

        if (voronoiEdgesContainer == null)
        {
            voronoiEdgesContainer = new GameObject("Voronoi Edges").transform;
        }

        map.Delaunator.ForEachVoronoiEdge(edge =>
        {
            var pointGameObject = Instantiate(voronoiPointPrefab, new Vector3((float)edge.P.X, 0, (float)edge.P.Y), Quaternion.identity, voronoiPointsContainer);
            CreateLine(voronoiEdgesContainer, $"Voronoi Edge", new Vector3[] { new Vector3((float)edge.P.X, 0, (float)edge.P.Y), new Vector3((float)edge.Q.X, 0, (float)edge.Q.Y) }, voronoiEdgeMat, 0.1f, 2);
        });
    }

    void CreateLine(Transform container, string name, Vector3[] points, Material mat, float width, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = mat;
        //lineRenderer.startColor = color;
        //lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }

    public void SpawnCells(Map map)
    {
        if (cellsContainer == null)
        {
            cellsContainer = new GameObject("Map").transform;
        }

        foreach (var cell in map.Cells)
        {
            CellBehaviour cellInstance = Instantiate(cellPrefab, cellsContainer);
            cellInstance.Init(cell);
            cellInstances.Add(cellInstance);
        }
    }

    void ToggleDraw()
    {
        // delaunay
        if (drawDelaunayPoints && delaunayPointsContainer == null || drawDelaunayEdges && delaunayEdgesContainer == null)
        {
            SpawnDelaunay(map);
        }
        if (delaunayPointsContainer != null)
        {
            delaunayPointsContainer.gameObject.SetActive(drawDelaunayPoints);
        }
        if (delaunayEdgesContainer != null)
        {
            delaunayEdgesContainer.gameObject.SetActive(drawDelaunayEdges);
        }

        // voronoi
        if (drawVoronoiPoints && voronoiPointsContainer == null || drawVoronoiEdges && voronoiEdgesContainer == null)
        {
            SpawnVoronoi(map);
        }
        if (voronoiPointsContainer != null)
        { 
            voronoiPointsContainer.gameObject.SetActive(drawVoronoiPoints);
        }
        if (voronoiEdgesContainer != null)
        {
            voronoiEdgesContainer.gameObject.SetActive(drawVoronoiEdges);
        }

        // cells
        if (cellsContainer != null)
        {
            cellsContainer.gameObject.SetActive(drawCells);
            foreach (var cell in cellInstances)
            {
                cell.ToggleNeighbourCountLabel(showNeighbourCounts);
            }
        }
    }
}
