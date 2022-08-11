using DelaunatorSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector3 Center { get; set; }

    List<int> neighbourIdx;
    Map map;

    public void Init(Map map, IVoronoiCell cellData, Vector3 center, List<int> neighbours)
    {
        neighbourIdx = neighbours;
    }

    public IEnumerable<Cell> GetNeighbours()
    {
        foreach(int n in neighbourIdx)
        {
            yield return map.Cells[n];
        }
    }
}
