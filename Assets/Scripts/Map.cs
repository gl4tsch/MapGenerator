using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    public List<Vector2> BlueNoisePoints => blueNoisePoints;
    List<Vector2> blueNoisePoints;

    // https://mapbox.github.io/delaunator/
    public Delaunator Delaunator => delaunator;
    Delaunator delaunator; // data in 2D

    public List<float> PointHeights => pointHeights;
    List<float> pointHeights;

    int[] pointToIncomingHalfEdge; // this[idx] => incoming halfedge id of point idx

    public List<Cell> Cells;

    public Map(List<Vector2> blueNoisePoints, Delaunator delaunator, List<float> pointHeights)
    {
        this.blueNoisePoints = blueNoisePoints;
        this.delaunator = delaunator;
        this.pointHeights = pointHeights;

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

        GenerateCells(delaunator, blueNoisePoints, pointHeights);
    }

    void GenerateCells(Delaunator delaunator, List<Vector2> cellPositions, List<float> cellHeights)
    {
        Cells = new List<Cell>(cellPositions.Count);
        delaunator.ForEachVoronoiCell(dCell =>
        {
            Vector2 center = cellPositions[dCell.Index];
            float height = cellHeights[dCell.Index];
            List<int> neighbours = GetNeighbours(dCell.Index).ToList();
            Cell cell = new(center, height, dCell, neighbours);
            Cells.Add(cell);
        });

        // fill in cell neighbours
        foreach (var cell in Cells)
        {
            cell.SetNeighbours(cell.NeighbourIdx.Select(i => Cells[i]).ToList());
        }
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
}
