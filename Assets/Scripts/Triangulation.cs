using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Sort the points along one axis. The first 3 points form a triangle. Consider the next point and connect it with all
//previously connected points which are visible to the point. An edge is visible if the center of the edge is visible to the point.
public static class Triangulation
{
    public static List<Triangle> TriangulateConvexPolygon(List<Vertex> convexHullPoints)
    {
        List<Triangle> triangles = new List<Triangle>();

        convexHullPoints = convexHullPoints.OrderBy(c => Mathf.Atan2(c.position.x, c.position.z)).ToList(); // sort hull points clockwise

        for (int i = 2; i < convexHullPoints.Count; i++)
        {
            Vertex a = convexHullPoints[0];
            Vertex b = convexHullPoints[i - 1];
            Vertex c = convexHullPoints[i];

            triangles.Add(new Triangle(a, b, c));
        }

        return triangles;
    }

    public static List<Triangle> TriangleSplittingAlgorithm(List<Vertex> points, List<Triangle> convexPolygon)
    {
        //Add the remaining points and split the triangles
        for (int i = 0; i < points.Count; i++)
        {
            Vertex currentPoint = points[i];

            //2d space
            Vector2 p = new Vector2(currentPoint.position.x, currentPoint.position.z);

            //Which triangle is this point in?
            for (int j = 0; j < convexPolygon.Count; j++)
            {
                Triangle t = convexPolygon[j];

                Vector2 p1 = new Vector2(t.v1.position.x, t.v1.position.z);
                Vector2 p2 = new Vector2(t.v2.position.x, t.v2.position.z);
                Vector2 p3 = new Vector2(t.v3.position.x, t.v3.position.z);

                if (IsPointInTriangle(p1, p2, p3, p))
                {
                    //Create 3 new triangles
                    Triangle t1 = new Triangle(t.v1, t.v2, currentPoint);
                    Triangle t2 = new Triangle(t.v2, t.v3, currentPoint);
                    Triangle t3 = new Triangle(t.v3, t.v1, currentPoint);

                    //Remove the old triangle
                    convexPolygon.Remove(t);

                    //Add the new triangles
                    convexPolygon.Add(t1);
                    convexPolygon.Add(t2);
                    convexPolygon.Add(t3);

                    break;
                }
            }
        }

        return convexPolygon;
    }

    public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {
        bool isWithinTriangle = false;

        //Based on Barycentric coordinates
        float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

        float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
        float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
        float c = 1 - a - b;

        //the point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        //{
        //    isWithinTriangle = true;
        //}

        //The point is within the triangle
        if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }

    public static List<Triangle> IncrementalAlgorithm(List<Vertex> points)
    {
        List<Triangle> triangles = new List<Triangle>();

        //Sort the points along x-axis
        //OrderBy is always sorting in ascending order - use OrderByDescending to get in the other order
        points = points.OrderBy(n => n.position.x).ToList();

        //The first 3 vertices are always forming a triangle
        Triangle newTriangle = new Triangle(points[0], points[1], points[2]);

        triangles.Add(newTriangle);

        //All edges that form the triangles, so we have something to test against
        List<Edge> edges = new List<Edge>();

        edges.Add(new Edge(newTriangle.v1, newTriangle.v2));
        edges.Add(new Edge(newTriangle.v2, newTriangle.v3));
        edges.Add(new Edge(newTriangle.v3, newTriangle.v1));

        //Add the other triangles one by one
        //Starts at 3 because we have already added 0,1,2
        for (int i = 3; i < points.Count; i++)
        {
            Vertex currentPoint = points[i];

            //The edges we add this loop or we will get stuck in an endless loop
            List<Edge> newEdges = new List<Edge>();

            //Is this edge visible? We only need to check if the midpoint of the edge is visible 
            for (int j = 0; j < edges.Count; j++)
            {
                Edge currentEdge = edges[j];

                Vertex midPoint = new Vertex((currentEdge.v1.position + currentEdge.v2.position) / 2);

                Edge edgeToMidpoint = new Edge(currentPoint, midPoint);

                //Check if this line is intersecting
                bool canSeeEdge = true;

                for (int k = 0; k < edges.Count; k++)
                {
                    //Dont compare the edge with itself
                    if (k == j)
                    {
                        continue;
                    }

                    if (AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                    {
                        canSeeEdge = false;

                        break;
                    }
                }

                //This is a valid triangle
                if (canSeeEdge)
                {
                    Edge edgeToPoint1 = new Edge(currentEdge.v1, currentPoint);
                    Edge edgeToPoint2 = new Edge(currentEdge.v2, currentPoint);

                    newEdges.Add(edgeToPoint1);
                    newEdges.Add(edgeToPoint2);

                    Triangle newTri = new Triangle(edgeToPoint1.v1, edgeToPoint1.v2, edgeToPoint2.v1);

                    triangles.Add(newTri);
                }
            }


            for (int j = 0; j < newEdges.Count; j++)
            {
                edges.Add(newEdges[j]);
            }
        }


        return triangles;
    }



    private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
    {
        Vector2 l1_p1 = new Vector2(edge1.v1.position.x, edge1.v1.position.z);
        Vector2 l1_p2 = new Vector2(edge1.v2.position.x, edge1.v2.position.z);

        Vector2 l2_p1 = new Vector2(edge2.v1.position.x, edge2.v1.position.z);
        Vector2 l2_p2 = new Vector2(edge2.v2.position.x, edge2.v2.position.z);

        bool isIntersecting = AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

        return isIntersecting;
    }
    

    public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
    {
        bool isIntersecting = false;

        float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

        //Make sure the denominator is > 0, if not the lines are parallel
        if (denominator != 0f)
        {
            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
            float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            //Are the line segments intersecting if the end points are the same
            if (shouldIncludeEndPoints)
            {
                //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
                {
                    isIntersecting = true;
                }
            }
            else
            {
                //Is intersecting if u_a and u_b are between 0 and 1
                if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
                {
                    isIntersecting = true;
                }
            }

        }

        return isIntersecting;
    }
}

