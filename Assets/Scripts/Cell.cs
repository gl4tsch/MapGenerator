using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell
{
    public Vector2 Center { get; private set; }
    public float Height { get; private set; }
    public IVoronoiCell CornerData { get; private set; }
    public List<int> NeighbourIdx { get; private set; }
    public List<Cell> Neighbours { get; private set; }

    public Cell(Vector2 center, float height, IVoronoiCell cornerData, List<int> neighbourIdx)
    {
        Center = center;
        Height = height;
        CornerData = cornerData;
        NeighbourIdx = neighbourIdx;
    }

    public void SetNeighbours(List<Cell> neighbours)
    {
        Neighbours = neighbours;
    }
}
