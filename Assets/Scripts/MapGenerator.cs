using DelaunatorSharp;
using DelaunatorSharp.Unity;
using DelaunatorSharp.Unity.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float mapSize = 10;
    [SerializeField] float rMin = 0.2f;

    [Header("Visuals")]
    [SerializeField] bool showDelaunayDots;
    [SerializeField] GameObject delaunayDot;
    [SerializeField] bool showVoronoiDots;
    [SerializeField] GameObject voronoiDot;

    GameObject delaunayVisualsContainer;

    Delaunator delaunator; // data in 2D
    List<Vector3> bluePoints;

    private void Start()
    {
        Clear();
        GenerateMap(mapSize, rMin);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Clear();
            GenerateMap(mapSize, rMin);
        }
    }

    void Clear()
    {
        delaunator = null;
        bluePoints?.Clear();

        if (delaunayVisualsContainer != null)
        {
            Destroy(delaunayVisualsContainer);
        }
    }

    public void GenerateMap(float mapSize, float delaunayRMin)
    {
        var blueNoisePoints = UniformPoissonDiskSampler.SampleCircle(Vector2.zero, mapSize / 2, delaunayRMin);
        delaunator = new Delaunator(blueNoisePoints.ToPoints());
        bluePoints = blueNoisePoints.Select(p => new Vector3(p.x, 0f, p.y)).ToList();

        if(showDelaunayDots)
        {
            delaunayVisualsContainer = new GameObject("DelaunayVisuals");
            foreach(var point in bluePoints)
            {
                Instantiate(delaunayDot, point, Quaternion.identity, delaunayVisualsContainer.transform);
            }
        }

        var camHeight = mapSize * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.transform.position = new Vector3(0, camHeight * 1.1f, 0);
    }
}
