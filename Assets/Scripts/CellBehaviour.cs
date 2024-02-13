using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CellBehaviour : MonoBehaviour
{
    [SerializeField] Gradient heightColors;
    [SerializeField] Gradient numberColors;
    [Header("References")]
    [SerializeField] TextMeshPro neighbourCountLabel;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;

    Cell cell;

    public void Init(Cell cell)
    {
        this.cell = cell;
        Mesh cellMesh = BuildMeshForCell(cell.CornerData, cell.Center, cell.Height);
        meshFilter.mesh = cellMesh;
        meshRenderer.material.color = heightColors.Evaluate(cell.Height);
        transform.position = new Vector3(cell.Center.x, cell.Height, cell.Center.y);

        // neighbour count text
        neighbourCountLabel.text = cell.Neighbours.Count.ToString();
        neighbourCountLabel.color = numberColors.Evaluate(((float)cell.Neighbours.Count).Map(4, 9, 0, 1));
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
        for (; i < topCornerPositions.Count; i++)
        {
            colors.Add(Color.red); // red for top face border vertices
        }
        // continue after top face corner verts
        for (; i < verts.Count; i++)
        {
            colors.Add(Color.black); // black for no border in shader
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    public void ToggleNeighbourCountLabel(bool show)
    {
        neighbourCountLabel.gameObject.SetActive(show);
    }
}
