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
    [SerializeField] float mapSize = 40;
    [SerializeField] float rMin = 1;

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

    Transform delaunayPointsContainer, delaunayEdgesContainer, voronoiPointsContainer, voronoiEdgesContainer, cellsContainer;
    Map map = null;
    List<CellBehaviour> cellInstances = new();

    private void OnValidate()
    {
        // ToggleDraw();
    }

    private void Start()
    {
        GenerateNewMap();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            GenerateNewMap();
        }
    }

    void Clear()
    {
        cellInstances.Clear();
        if (delaunayPointsContainer != null)
        {
            Destroy(delaunayPointsContainer.gameObject);
        }
        if (delaunayEdgesContainer != null)
        {
            Destroy(delaunayEdgesContainer.gameObject);
        }
        if (voronoiPointsContainer != null)
        {
            Destroy(voronoiPointsContainer.gameObject);
        }
        if (voronoiEdgesContainer != null)
        {
            Destroy(voronoiEdgesContainer.gameObject);
        }
        if (cellsContainer != null)
        {
            Destroy(cellsContainer.gameObject);
        }
    }

    public void GenerateNewMap()
    {
        Clear();
        map = GenerateMap(mapSize, rMin);
        SpawnDelaunay(map);
        SpawnVoronoi(map);
        SpawnCells(map);
        ToggleDraw();
    }

    public Map GenerateMap(float mapSize, float delaunayRMin, int? seed = null)
    {
        Random.InitState(seed ?? (int)DateTime.Now.Ticks);

        List<Vector2> blueNoisePoints = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, mapSize / 2, delaunayRMin);
        Delaunator delaunator = new Delaunator(blueNoisePoints.ToPoints());
        // fill heightmap
        List<float> pointHeights = HeightField.PerlinIslands(blueNoisePoints, mapSize, 0f, .7f, 4f, 4f, 6);

        Map map = new Map(blueNoisePoints, delaunator, pointHeights);

        var camHeight = mapSize * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.transform.position = new Vector3(0, camHeight * 1.1f, 0);

        return map;
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
            cellsContainer = new GameObject("Cells").transform;
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
        if (delaunayPointsContainer != null)
        {
            delaunayPointsContainer.gameObject.SetActive(drawDelaunayPoints);
        }
        if (delaunayEdgesContainer != null)
        {
            delaunayEdgesContainer.gameObject.SetActive(drawDelaunayEdges);
        }
        if (voronoiPointsContainer != null)
        {
            voronoiPointsContainer.gameObject.SetActive(drawVoronoiPoints);
        }
        if (voronoiEdgesContainer != null)
        {
            voronoiEdgesContainer.gameObject.SetActive(drawVoronoiEdges);
        }
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
