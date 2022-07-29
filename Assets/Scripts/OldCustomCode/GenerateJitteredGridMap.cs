using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenerateJitteredGridMap : MonoBehaviour {

    public bool DrawDelaunayDots;
    public bool DrawDelauneyEdges;
    public bool DrawVoronoiCentroids;
    public bool DrawVoronoiEdges;
    public GameObject dot;
    public GameObject centroid;
    public GameObject cell;
    public GameObject mainCamera;

    //used for JitteredGrid
    private DelaunayDot[][] dotGrid;
    private GameObject[][] cellGrid;
    

    // Use this for initialization
    void Start ()
    {
        JitteredGrid(15, 15, 1, 0.2f);
        DebugDraw();
    }

    void JitteredGrid(int cellCountX, int cellCountY, float cellSize, float margin)
    {
        dotGrid = new DelaunayDot[cellCountX][];

        // dots and triangles
        for (int x = 0; x < cellCountX; x++) // fill dot array from bottom left going top -> right
        {
            dotGrid[x] = new DelaunayDot[cellCountY];

            for (int y = 0; y < cellCountY; y++)
            {
                dotGrid[x][y] = new DelaunayDot(new Vector3(Random.Range(x * cellSize + margin, x * cellSize + cellSize - margin), 0,
                    Random.Range(y * cellSize + margin, y * cellSize + cellSize - margin)));

                if(y > 0 && x > 0) // fill in dot Neighbours with top right of squares as anchor
                {
                    // square
                    dotGrid[x - 1][y - 1].Neighbours[4] = dotGrid[x - 1][y];  // bottom left to top left
                    dotGrid[x - 1][y - 1].Neighbours[6] = dotGrid[x][y - 1];  // bottom left to bottom right
                    dotGrid[x - 1][y].Neighbours[0] = dotGrid[x - 1][y - 1];  // top left to bottom left
                    dotGrid[x - 1][y].Neighbours[6] = dotGrid[x][y];          // top left to top right
                    dotGrid[x][y].Neighbours[2] = dotGrid[x - 1][y];          // top right to top left
                    dotGrid[x][y].Neighbours[0] = dotGrid[x][y - 1];          // top right to bottom right
                    dotGrid[x][y - 1].Neighbours[2] = dotGrid[x - 1][y - 1];  // bottom right to bottom left
                    dotGrid[x][y - 1].Neighbours[4] = dotGrid[x][y];          // bottom right to top right

                    // split -> delaunay triangulation
                    var dist1 = Vector3.Distance(dotGrid[x][y].Position, dotGrid[x - 1][y - 1].Position); // top right to bottom left
                    var dist2 = Vector3.Distance(dotGrid[x - 1][y].Position, dotGrid[x][y - 1].Position); // top left to bottom right

                    if (dist1 - dist2 < 0) // split top right to bottom left
                    {
                        dotGrid[x - 1][y - 1].Neighbours[5] = dotGrid[x][y];  // bottom left to top right
                        dotGrid[x][y].Neighbours[1] = dotGrid[x - 1][y - 1];  // top right to bottom left
                    }
                    else // split top left to bottom right
                    {
                        dotGrid[x - 1][y].Neighbours[7] = dotGrid[x][y - 1];  // top left to bottom right
                        dotGrid[x][y - 1].Neighbours[3] = dotGrid[x - 1][y];  // bottom right to top left
                    }
                }
            }
        }

        // apply height field
        for (int i = 0; i < Random.Range(50,70); i++) // # plateaus
        {
            var rndXpos = Random.Range(0, dotGrid.Length - 1);
            var rndYpos = Random.Range(0, dotGrid[0].Length - 1);
            var nn = new List<DelaunayDot>();
            dotGrid[rndXpos][rndYpos].NeighboursNeighbours(1,0,ref nn);

            foreach(DelaunayDot d in nn)
            {
                d.once = false;
                d.Height += 0.3f;
            }
        }
        var maxHeight = 0f; // needed for remapping greyscale
        for (int i = 0; i < dotGrid.Length; i++)
        {
            for (int ii = 0; ii < dotGrid[0].Length; ii++)
            {
                if (dotGrid[i][ii].Height > maxHeight)
                {
                    maxHeight = dotGrid[i][ii].Height;
                }
            }
        }

        // centroids
        for (int x = 1; x < dotGrid.Length - 1; x++) // consider only non border dots
        {
            for (int y = 1; y < dotGrid[0].Length - 1; y++)
            {
                List<DelaunayDot> neighbourList = new List<DelaunayDot>();

                for (int n = 0; n < dotGrid[x][y].Neighbours.Length; n++)
                {
                    if (dotGrid[x][y].Neighbours[n] != null)
                    {
                        neighbourList.Add(dotGrid[x][y].Neighbours[n]);
                    }
                }
                for (int n = 0; n < neighbourList.Count; n++) // calculate centroids starting with bottom neighbour and next going clockwise -> triangles
                {
                    dotGrid[x][y].Centroids.Add(new Vector3((dotGrid[x][y].Position.x + neighbourList[n].Position.x + neighbourList[(n + 1) % neighbourList.Count].Position.x) / 3, dotGrid[x][y].Height,
                        (dotGrid[x][y].Position.z + neighbourList[n].Position.z + neighbourList[(n + 1) % neighbourList.Count].Position.z) / 3)
                        - dotGrid[x][y].Position); // world space -> local space
                }
            }
        }

        // cell meshes
        cellGrid = new GameObject[dotGrid.Length - 2][];

        for (int x = 0; x < dotGrid.Length - 2; x++)
        {
            cellGrid[x] = new GameObject[dotGrid[0].Length - 2];

            for (int y = 0; y < dotGrid[0].Length - 2; y++)
            {
                var bottomHeight = 0;
                cellGrid[x][y] = Instantiate(cell, dotGrid[x + 1][y + 1].Position, Quaternion.identity);
                cellGrid[x][y].GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.green, dotGrid[x + 1][y + 1].Height / maxHeight); // recolor cells
                Mesh mesh = cellGrid[x][y].GetComponent<MeshFilter>().mesh = new Mesh();
                List<Vector3> verts = new List<Vector3>();
                List<int> tris = new List<int>();
                
                verts.InsertRange(0, dotGrid[x + 1][y + 1].Centroids); // add top face
                verts.Add(new Vector3(0, dotGrid[x + 1][y + 1].Height, 0)); // add top middle vertex

                for (int c = 0; c < dotGrid[x + 1][y + 1].Centroids.Count; c++)
                {
                    // top face
                    verts[c] = new Vector3(verts[c].x, verts[c].y, verts[c].z);             // elevate top
                    verts[c + 1] = new Vector3(verts[c + 1].x, verts[c + 1].y, verts[c + 1].z); // elevate top

                    tris.Add(dotGrid[x + 1][y + 1].Centroids.Count);
                    tris.Add(c);
                    tris.Add((c + 1) % dotGrid[x + 1][y + 1].Centroids.Count);

                    // side faces
                    verts.Add(new Vector3(verts[c].x, verts[c].y, verts[c].z)); // top right
                    verts.Add(new Vector3(verts[c].x, bottomHeight, verts[c].z)); // bottom right
                    verts.Add(new Vector3(verts[(c + 1) % dotGrid[x + 1][y + 1].Centroids.Count].x,
                        verts[(c + 1) % dotGrid[x + 1][y + 1].Centroids.Count].y,
                        verts[(c + 1) % dotGrid[x + 1][y + 1].Centroids.Count].z)); // top left
                    verts.Add(new Vector3(verts[(c + 1) % dotGrid[x + 1][y + 1].Centroids.Count].x,
                        bottomHeight,
                        verts[(c + 1) % dotGrid[x + 1][y + 1].Centroids.Count].z)); // bottom left

                    tris.Add(verts.Count - 4); // top right
                    tris.Add(verts.Count - 3); // bottom right
                    tris.Add(verts.Count - 2); // top left
                    tris.Add(verts.Count - 2); // top left
                    tris.Add(verts.Count - 3); // bottom right
                    tris.Add(verts.Count - 1); // bottom left
                }

                mesh.vertices = verts.ToArray();
                mesh.triangles = tris.ToArray();
                mesh.RecalculateNormals();
            }
        }

        mainCamera.transform.Translate(new Vector3(cellCountX / 2 * cellSize, cellCountY / 2 * cellSize));
        mainCamera.GetComponent<Camera>().orthographicSize = cellCountX / 2 + cellSize;
    }

    private void DebugDraw()
    {
        for (int x = 0; x < dotGrid.Length; x++)
        {
            for (int y = 0; y < dotGrid[0].Length; y++)
            {
                if (DrawDelaunayDots)
                {
                    Instantiate(dot, dotGrid[x][y].Position, Quaternion.identity, transform);
                }
                for (int n = 0; n < dotGrid[x][y].Neighbours.Length; n++)
                {
                    if (DrawDelauneyEdges && dotGrid[x][y].Neighbours[n] != null)
                    {
                        Debug.DrawLine(dotGrid[x][y].Position, dotGrid[x][y].Neighbours[n].Position, Color.red, float.PositiveInfinity);
                    }
                }
                for (int c = 0; c < dotGrid[x][y].Centroids.Count; c++)
                {
                    if (DrawVoronoiCentroids)
                    {
                        Instantiate(centroid, dotGrid[x][y].Position + dotGrid[x][y].Centroids[c], Quaternion.identity, transform);
                    }
                    if (DrawVoronoiEdges)
                    {
                        Debug.DrawLine(dotGrid[x][y].Position +  dotGrid[x][y].Centroids[c], dotGrid[x][y].Position + dotGrid[x][y].Centroids[(c + 1) % dotGrid[x][y].Centroids.Count], Color.yellow, float.PositiveInfinity);
                    }
                }
            }
        }
    }
}
