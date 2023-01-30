using DelaunatorSharp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2 Center { get; set; }
    public float Height { get; set; }
    [SerializeField] Gradient numberColors;

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
        transform.position = new Vector3(center.x, height, center.y);

        // neighbour count text
        var neighbourCountLabel = GetComponentInChildren<TextMeshPro>();
        neighbourCountLabel.text = neighbourIdx.Count.ToString();
        neighbourCountLabel.color = numberColors.Evaluate(((float)neighbourIdx.Count).Map(4, 10, 0, 1));
    }

    public IEnumerable<Cell> GetNeighbours()
    {
        foreach(int n in neighbourIdx)
        {
            yield return map.Cells[n];
        }
    }
}
