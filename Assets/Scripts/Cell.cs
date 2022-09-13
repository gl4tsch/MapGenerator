using DelaunatorSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2 Center { get; set; }
    public float Height { get; set; }

    List<int> neighbourIdx;
    Map map;

    public void Init(Map map, Vector2 center, float height, List<int> neighbours, Mesh mesh)
    {
        this.map = map;
        Center = center;
        Height = height;
        neighbourIdx = neighbours;

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.green, height);
        transform.position = new Vector3(center.x, 0, center.y);
    }

    public IEnumerable<Cell> GetNeighbours()
    {
        foreach(int n in neighbourIdx)
        {
            yield return map.Cells[n];
        }
    }
}
