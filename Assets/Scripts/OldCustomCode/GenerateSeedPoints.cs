using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DelaunatorSharp.Unity;

public static class BlueNoiseGenerator {

    private static List<Vertex> dotList;
    private static int[][] backgroundGrid;
    private static List<int> activeSamples;

    public static List<Vertex> Scaffolding(float mapSize)
    {
        List<Vertex> scaffolding = new List<Vertex>();
        scaffolding.Add(new Vertex(new Vector3(mapSize/2, 0, -mapSize), true));
        scaffolding.Add(new Vertex(new Vector3(mapSize/2, 0, mapSize + mapSize), true));
        scaffolding.Add(new Vertex(new Vector3(mapSize + mapSize, 0, mapSize/2), true));
        scaffolding.Add(new Vertex(new Vector3(-mapSize, 0, mapSize/2), true));
        return scaffolding;
    }

    public static List<Vertex> PoissonDiscSampling(float rMin, float rMax, int rejectionLimit, float mapSize)
    {
        /* Step 0.
         * Initialize an n-dimensional background grid for storing
         * samples and accelerating spatial searches. We pick the cell size to
         * be bounded by r/√n, so that each grid cell will contain at most
         * one sample, and thus the grid can be implemented as a simple ndimensional
         * array of integers: the default −1 indicates no sample, a
         * non-negative integer gives the index of the sample located in a cell.
         * Step 1.
         * Select the initial sample, x0, randomly chosen uniformly
         * from the domain. Insert it into the background grid, and initialize
         * the “active list” (an array of sample indices) with this index (zero).
         * Step 2.
         * While the active list is not empty, choose a random index
         * from it (say i). Generate up to k points chosen uniformly from the
         * spherical annulus between radius r and 2r around xi. For each
         * point in turn, check if it is within distance r of existing samples
         * (using the background grid to only test nearby samples). If a point
         * is adequately far from existing samples, emit it as the next sample
         * and add it to the active list. If after k attempts no such point is
         * found, instead remove i from the active list.
         */
        
        var cellSize = rMin / Mathf.Sqrt(2);
        int gridSize = (int)(mapSize / cellSize)+1;

        //initialize backgroundGrid
        backgroundGrid = new int[gridSize][];
        for (int i = 0; i < gridSize; i++)
        {
            backgroundGrid[i] = new int[gridSize];
            for (int ii = 0; ii < gridSize; ii++)
            {
                backgroundGrid[i][ii] = -1;
            }
        }

        
        dotList = new List<Vertex>();
        //first sample random
        dotList.Add(new Vertex(new Vector3(Random.Range(0f,mapSize), 0, Random.Range(0f,mapSize))));
        backgroundGrid[gridSize / 2][gridSize / 2] = 0;
        activeSamples = new List<int>();
        activeSamples.Add(0);

        int chosenIndex = 0;
        int testIndexX, testIndexY;
        Vector3 testPos;
        int k;

        //generate Seed Points
        while (activeSamples.Count > 0)
        {
            chosenIndex = Random.Range(0, activeSamples.Count());
            for (k = 0; k < rejectionLimit; k++)
            {
                testPos = randomPosInAnnulus(rMin, rMax, dotList[activeSamples[chosenIndex]].position);
                if(testPos.x < 0 || testPos.z < 0 || testPos.x > mapSize || testPos.z > mapSize)
                {
                    continue;
                }
                //check backgroundGrid if Position is viable
                testIndexX = (int)(testPos.x / cellSize);
                testIndexY = (int)(testPos.z / cellSize);

                if (!isInGridBounds(testIndexX, testIndexY, gridSize))
                {
                    continue;
                }
                else if (backgroundGrid[testIndexX][testIndexY] >= 0) //middle
                {
                    continue;
                }
                else if (isInGridBounds(testIndexX - 1, testIndexY - 1, gridSize) && backgroundGrid[testIndexX - 1][testIndexY - 1] >= 0) //top left
                {
                    //test distance. continue if hit
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX - 1][testIndexY - 1]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX, testIndexY - 1, gridSize) && backgroundGrid[testIndexX][testIndexY - 1] >= 0) //top
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX][testIndexY - 1]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX + 1, testIndexY - 1, gridSize) && backgroundGrid[testIndexX + 1][testIndexY - 1] >= 0) //top right
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX + 1][testIndexY - 1]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX + 1, testIndexY, gridSize) && backgroundGrid[testIndexX + 1][testIndexY] >= 0) //right
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX + 1][testIndexY]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX + 1, testIndexY + 1, gridSize) && backgroundGrid[testIndexX + 1][testIndexY + 1] >= 0) //bottom right
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX + 1][testIndexY + 1]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX, testIndexY + 1, gridSize) && backgroundGrid[testIndexX][testIndexY + 1] >= 0) //bottom
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX][testIndexY + 1]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX - 1, testIndexY + 1, gridSize) && backgroundGrid[testIndexX - 1][testIndexY + 1] >= 0) //bottom left
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX - 1][testIndexY + 1]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else if (isInGridBounds(testIndexX - 1, testIndexY, gridSize) && backgroundGrid[testIndexX - 1][testIndexY] >= 0) //left
                {
                    if (Vector3.Distance(dotList[backgroundGrid[testIndexX - 1][testIndexY]].position, testPos) < rMin)
                    {
                        continue;
                    }
                }
                else //area clear. Add new dot to dotList backgroundGrid and activeSamples
                {
                    //Instantiate(dot, testPos, Quaternion.identity, transform);
                    dotList.Add(new Vertex(testPos));
                    backgroundGrid[testIndexX][testIndexY] = dotList.Count - 1;
                    activeSamples.Add(dotList.Count - 1);
                    break;
                }
            }
            if (k == rejectionLimit) //remove from active set
            {
                activeSamples.RemoveAt(chosenIndex);
            }
        }
        
        return dotList;
    }

    static Vector3 randomPosInAnnulus(float rMin, float rMax, Vector3 origin)
    {
        var u = Random.Range(rMin, rMax);
        var v = Random.Range(0, rMin + rMax);
        var t = Random.Range(0f, 1f) * 2 * Mathf.PI;
        var r = v < u ? u : rMin + rMax - u;
        return origin + new Vector3(r * Mathf.Cos(t), 0, r * Mathf.Sin(t));
    }

    static bool isInGridBounds(int x, int y, int gridSize)
    {
        if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
