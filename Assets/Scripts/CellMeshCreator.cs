using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMeshCreator : MonoBehaviour {
    
    public static List<GameObject> SpawnMeshes (List<Vertex> points, GameObject cell)
    {
        List<GameObject> cells = new List<GameObject>(points.Count);

        var maxHeight = 0f; // needed for remapping colorscale
        foreach(Vertex v in points)
        {
            if (v.position.y > maxHeight)
            {
                maxHeight = v.position.y;
            }
        }
        
        foreach (Vertex v in points)
        {
            var bottomHeight = -v.position.y;
            GameObject currentCell = Instantiate(cell, v.position, Quaternion.identity);
            cells.Add(currentCell);
            currentCell.GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.green, v.position.y / maxHeight); // recolor cells
            //currentCell.GetComponent<LineRenderer>().positionCount = v.Centroids.Count;
            Mesh mesh = currentCell.GetComponent<MeshFilter>().mesh = new Mesh();
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
                
            verts.InsertRange(0, v.Centroids); // centroids as top
            verts.Add(Vector3.zero); // add top middle vertex

            for (int c = 0; c < v.Centroids.Count; c++) // triangulation
            {
                // top face
                tris.Add(v.Centroids.Count); // middle
                tris.Add(c);
                tris.Add((c + 1) % v.Centroids.Count);
                //// LineRenderer
                //currentCell.GetComponent<LineRenderer>().SetPosition(c, v.Centroids[c]);

                // side faces
                verts.Add(verts[c]); // top right
                verts.Add(new Vector3(verts[c].x, bottomHeight, verts[c].z)); // bottom right
                verts.Add(verts[(c + 1) % v.Centroids.Count]); // top left
                verts.Add(new Vector3(verts[(c + 1) % v.Centroids.Count].x,
                    bottomHeight,
                    verts[(c + 1) % v.Centroids.Count].z)); // bottom left

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
        return cells;
    }
}
