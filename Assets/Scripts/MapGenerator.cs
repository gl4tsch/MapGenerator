using DelaunatorSharp;
using DelaunatorSharp.Unity;
using DelaunatorSharp.Unity.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float mapSize = 40;
    [SerializeField] float rMin = 1;

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

    [Header("Cell")]
    [SerializeField] Cell cellPrefab;

    Transform delaunayVisualsContainer, voronoiVisualContainer;

    List<Vector2> blueNoisePoints;
    List<float> pointHeights;
    Delaunator delaunator; // data in 2D
    Map map;

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
            delaunayVisualsContainer = null;
        }

        if(voronoiVisualContainer != null)
        {
            Destroy(voronoiVisualContainer.gameObject);
            voronoiVisualContainer = null;
        }
    }

    public void GenerateMap(float mapSize, float delaunayRMin)
    {
        blueNoisePoints = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, mapSize / 2, delaunayRMin);
        delaunator = new Delaunator(blueNoisePoints.ToPoints());

        DrawDelaunay();
        DrawVoronoi();

        // fill heightmap
        pointHeights = PerlinIslands(blueNoisePoints, mapSize, 0f, .7f, 4f, 4f, 6);

        map = new Map(blueNoisePoints.Count);
        SpawnCells(delaunator, blueNoisePoints, pointHeights);

        var camHeight = mapSize * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.transform.position = new Vector3(0, camHeight * 1.1f, 0);
    }

    void SpawnCells(Delaunator delaunator, List<Vector2> cellPositions, List<float> cellHeights)
    {
        var mapContainer = new GameObject("Map").transform;
        delaunator.ForEachVoronoiCell(dCell =>
        {
            var center = cellPositions[dCell.Index];
            float height = cellHeights[dCell.Index];
            List<int> neighbours = GetNeighbours(dCell.Index).ToList();
            Mesh cellMesh = BuildMeshForCell(dCell, center, height);

            Cell cell = Instantiate(cellPrefab, new Vector3(center.x, 0, center.y), Quaternion.identity, mapContainer);
            cell.Init(map, center, height, neighbours, cellMesh);
        });
    }

    Mesh BuildMeshForCell(IVoronoiCell cornerData, Vector2 center, float height)
    {
        List<Vector2> localCornerPositions = cornerData.Points.Select(p => p.ToVector2() - center).ToList();
        List<Vector3> topCornerPositions = localCornerPositions.Select(p => new Vector3(p.x, height, p.y)).ToList();

        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Color> colors = new List<Color>(); // for border shader

        verts.InsertRange(0, topCornerPositions); // centroids as top
        verts.Add(Vector3.up * height); // add top middle vertex

        for (int c = 0; c < topCornerPositions.Count; c++) // triangulation
        {
            // top face
            tris.Add(topCornerPositions.Count); // middle
            tris.Add((c + 1) % topCornerPositions.Count); // next corner
            tris.Add(c); // current corner

            // side faces
            verts.Add(verts[c]); // top right
            verts.Add(new Vector3(verts[c].x, 0, verts[c].z)); // bottom right
            verts.Add(verts[(c + 1) % topCornerPositions.Count]); // top left
            verts.Add(new Vector3(verts[(c + 1) % topCornerPositions.Count].x, 0, verts[(c + 1) % topCornerPositions.Count].z)); // bottom left

            tris.Add(verts.Count - 4); // top right
            tris.Add(verts.Count - 2); // top left
            tris.Add(verts.Count - 3); // bottom right
            tris.Add(verts.Count - 2); // top left
            tris.Add(verts.Count - 1); // bottom left
            tris.Add(verts.Count - 3); // bottom right
        }

        // fill colors
        int i = 0;
        // start with top face excluding middle vert
        for(; i < topCornerPositions.Count; i++)
        {
            colors.Add(Color.red); // red for top face border vertices
        }
        // continue after top face corner verts
        for(; i < verts.Count; i++)
        {
            colors.Add(Color.black); // black for no border in shader
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    // Delaunator extension
    IEnumerable<int> GetNeighbours(int point)
    {
        foreach(var e in delaunator.EdgesAroundPoint(point))
        {
            yield return delaunator.Triangles[e];
        }
    }

    // HeightField
    List<float> PerlinIslands(List<Vector2> points, float mapSize, float yShift, float edgeDown, float dropOffScale, float frequency, int numElevationLevels)
    {
        float a = yShift;               // y shift
        float b = edgeDown;             // push edges down
        float c = dropOffScale;         // drop off scale
        float d = 0f;                   // normalized distance from center
        float f = frequency;            // frequency
        float e = numElevationLevels;   // # elevation levels
        var r = Random.Range(0, 1000);  // random offset

        List<float> outPointHeights = new List<float>();

        foreach (var p in points)
        {
            var nx = p.x / mapSize; // normalized position relative to center
            var nz = p.y / mapSize;
            d = 2 * Mathf.Sqrt(nx * nx + nz * nz); //2 * Mathf.Max(Mathf.Abs(nx), Mathf.Abs(nz)); // Manhatten Distance
            var h = Mathf.PerlinNoise(p.x / mapSize * f + r, p.y / mapSize * f + r);
            h = (h + a) * (1 - b * Mathf.Pow(d, c));
            h = Mathf.Round(h * e) / (e-1); // round to fix number of elevation levels
            h = Mathf.Clamp01(h);
            outPointHeights.Add(h);
        }
        return outPointHeights;
    }

    void DrawDelaunay()
    {
        if (delaunator == null)
            return;

        if (delaunayVisualsContainer == null)
        {
            delaunayVisualsContainer = new GameObject("Delaunay Visuals").transform;
        }

        if (drawDelaunayPoints)
        {
            foreach(var point in blueNoisePoints)
            {
                Instantiate(delaunayPointPrefab, new Vector3(point.x, 0, point.y), Quaternion.identity, delaunayVisualsContainer);
            }
        }

        if (drawDelaunayEdges)
        {
            delaunator.ForEachTriangleEdge(edge =>
            {
                CreateLine(delaunayVisualsContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { new Vector3((float)edge.P.X, 0, (float)edge.P.Y), new Vector3((float)edge.Q.X, 0, (float)edge.Q.Y) }, delaunayEdgeColor, 0.1f, 0);
            });
        }
    }

    void DrawVoronoi()
    {
        if (delaunator == null)
            return;

        if (voronoiVisualContainer == null)
        {
            voronoiVisualContainer = new GameObject("Voronoi Visuals").transform;
        }

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
