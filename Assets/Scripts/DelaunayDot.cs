using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayDot{
    public Vector3 Position;
    public float Height;
    public DelaunayDot[] Neighbours; // starting bottom going clockwise
    public List<Vector3> Centroids; // local space
    public bool once; // flag for NeighboursNeighbours

    public DelaunayDot(Vector3 position)
    {
        Position = position;
        Height = position.y;
        Neighbours = new DelaunayDot[8];
        Centroids = new List<Vector3>();
        once = false;
    }

    public void NeighboursNeighbours(int distance, int recDepth, ref List<DelaunayDot> allNeighbours)
    {
        if (!once)
        {
            allNeighbours.Add(this);
            once = true;
        }

        if (recDepth >= distance)
        {
            return;
        }

        recDepth++;

        for (int i = 0; i < 8; i++)
        {
            if(Neighbours[i] != null)
            {
                Neighbours[i].NeighboursNeighbours(distance, recDepth, ref allNeighbours);
            }
        }

        return;
    }
}
