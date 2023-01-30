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
    // https://mapbox.github.io/delaunator/
    Delaunator delaunator; // data in 2D
    int[] pointToIncomingHalfEdge; // this[idx] => incoming halfedge id of point idx
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

        // fill point->incomingHalfedge helper
        pointToIncomingHalfEdge = new int[delaunator.Points.Length];
        for (int i = 0; i < pointToIncomingHalfEdge.Length; i++)
        {
            pointToIncomingHalfEdge[i] = -1;
        }
        for (int e = 0; e < delaunator.Triangles.Length; e++)
        {
            var endPoint = delaunator.Triangles[Delaunator.NextHalfedge(e)];
            if (pointToIncomingHalfEdge[endPoint] != -1 || endPoint == -1)
                continue;
            pointToIncomingHalfEdge[endPoint] = e;
        }

        // fill heightmap
        pointHeights = HeightField.PerlinIslands(blueNoisePoints, mapSize, 0f, .7f, 4f, 4f, 6);

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
            Vector2 center = cellPositions[dCell.Index];
            float height = cellHeights[dCell.Index];
            List<int> neighbours = GetNeighbours(dCell.Index).ToList();
            Mesh cellMesh = BuildMeshForCell(dCell, center, height);

            Cell cell = Instantiate(cellPrefab, new Vector3(center.x, height, center.y), Quaternion.identity, mapContainer);
            cell.Init(map, center, height, neighbours, cellMesh);
        });
    }

    // Delaunator extension
    IEnumerable<int> GetNeighbours(int point)
    {
        // delaunay.triangles[halfedgeID] => pointID where halfedge starts
        // delaunay.halfedges[halfedgeID] => opposite halfedge in adjacent triangle
        // delaunator.EdgesAroundPoint takes any incoming halfedgeID to a point, so a point -> incoming halfedge function is needed
        int anyIncomingHalfedgeId = pointToIncomingHalfEdge[point];
        foreach (var e in delaunator.EdgesAroundPoint(anyIncomingHalfedgeId))
        {
            yield return delaunator.Triangles[e];
        }
    }

    Mesh BuildMeshForCell(IVoronoiCell cornerData, Vector2 center, float height)
    {
        List<Vector2> localCornerPositions = cornerData.Points.Select(p => p.ToVector2() - center).ToList();
        List<Vector3> topCornerPositions = localCornerPositions.Select(p => new Vector3(p.x, 0, p.y)).ToList();

        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Color> colors = new List<Color>(); // for border shader

        verts.InsertRange(0, topCornerPositions); // centroids as top
        verts.Add(Vector3.zero); // add top middle vertex

        for (int c = 0; c < topCornerPositions.Count; c++) // triangulation
        {
            // top face
            tris.Add(topCornerPositions.Count); // middle
            tris.Add((c + 1) % topCornerPositions.Count); // next corner
            tris.Add(c); // current corner

            // side faces
            verts.Add(verts[c]); // top right
            verts.Add(new Vector3(verts[c].x, -height, verts[c].z)); // bottom right
            verts.Add(verts[(c + 1) % topCornerPositions.Count]); // top left
            verts.Add(new Vector3(verts[(c + 1) % topCornerPositions.Count].x, -height, verts[(c + 1) % topCornerPositions.Count].z)); // bottom left

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
