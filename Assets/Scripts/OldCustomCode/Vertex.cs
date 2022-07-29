using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public Vector3 position;

    //The outgoing halfedge (a halfedge that starts at this vertex)
    //Doesnt matter which edge we connect to it
    public HalfEdge halfEdge;

    //Which triangle is this vertex a part of?
    public Triangle triangle;

    //The previous and next vertex this vertex is attached to
    public Vertex prevVertex;
    public Vertex nextVertex;

    //Properties this vertex may have
    //Reflex is concave
    public bool isReflex;
    public bool isConvex;
    public bool isEar;

    //own variables
    public List<Vector3> Centroids; // in local space
    public List<Vertex> Neighbours;
    public bool once; // needed for neighboursNeighbours
    public bool isScaffolding;

    public Vertex(Vector3 position)
    {
        this.position = position;
        Centroids = new List<Vector3>();
        Neighbours = new List<Vertex>();
        once = false;
    }

    public Vertex(Vector3 position, bool isScaffolding)
    {
        this.position = position;
        Centroids = new List<Vector3>();
        Neighbours = new List<Vertex>();
        once = false;
        this.isScaffolding = isScaffolding;
    }

    //Get 2d pos of this vertex
    public Vector2 GetPos2D_XZ()
    {
        Vector2 pos_2d_xz = new Vector2(position.x, position.z);

        return pos_2d_xz;
    }

    //fill allNeighbours recoursively
    public void NeighboursNeighbours(int distance, int recDepthCounter, ref List<Vertex> allNeighbours)
    {
        if (!once)
        {
            allNeighbours.Add(this);
            once = true;
        }

        if (recDepthCounter >= distance)
        {
            return;
        }

        recDepthCounter++;

        foreach(Vertex n in Neighbours)
        {
            n.NeighboursNeighbours(distance, recDepthCounter, ref allNeighbours);
        }

        return;
    }
}

