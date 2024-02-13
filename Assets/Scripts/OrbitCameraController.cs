using System.Linq;
using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
    [Header("OrbitTransforms")]
    [SerializeField] Transform yRotTransform;
    [SerializeField] Transform xRotTransform;
    [SerializeField] Transform distanceTransform;
    [SerializeField] Camera cam;
    public Camera Cam => cam;

    [Header("Settings")]
    [SerializeField] float yRotSpeed = 20;
    [SerializeField] float xRotSpeed = 10;
    [SerializeField] float zoomSpeed = 10;
    [Space]
    [SerializeField] bool clampYAngle = false;
    [Tooltip("angle in degrees relative to start")]
    [SerializeField] float minYAngle = -180f;
    [Tooltip("angle in degrees relative to start")]
    [SerializeField] float maxYAngle = 180;
    [Space]
    [SerializeField] bool clampXAngle = true;
    [Tooltip("angle in degrees relative to start")]
    [SerializeField] float minXAngle = -90f;
    [Tooltip("angle in degrees relative to start")]
    [SerializeField] float maxXAngle = 90f;
    [Space]
    [SerializeField] float minCamDistance = 2f;
    [SerializeField] float maxCamDistance = 100f;

    float yRotSum = 0;
    float xRotSum = 0;
    Vector2? prevMousePos = null;

    private void OnValidate()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            prevMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(1) && prevMousePos.HasValue)
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 mouseDelta = currentMousePos - prevMousePos.Value;
            Rotate(mouseDelta);
            prevMousePos = currentMousePos;
        }
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            Zoom(Input.mouseScrollDelta.y);
        }
    }

    public virtual void Rotate(Vector2 rotValue)
    {
        // horizontal rotation
        var yRotDelta = rotValue.x * yRotSpeed * 0.01f;
        // clamp
        if (!clampYAngle || (yRotSum + yRotDelta >= minYAngle && yRotSum + yRotDelta <= maxYAngle))
        {
            yRotTransform.Rotate(Vector3.up * yRotDelta, Space.World);
            yRotSum += yRotDelta;
        }

        // vertical rotation
        var xRotDelta = rotValue.y * -xRotSpeed * 0.01f;
        // clamp
        if (!clampXAngle || (xRotSum + xRotDelta >= minXAngle && xRotSum + xRotDelta <= maxXAngle))
        {
            xRotTransform.Rotate(Vector3.right * xRotDelta, Space.Self);
            xRotSum += xRotDelta;
        }
    }

    public virtual void Zoom(float zoomValue)
    {
        float newDist = distanceTransform.localPosition.magnitude + zoomValue * zoomSpeed * 0.01f;
        newDist = Mathf.Clamp(newDist, minCamDistance, maxCamDistance);
        newDist = Mathf.Abs(newDist);
        SetZoomDistance(newDist);
    }

    public void SetZoomDistance(float dist)
    {
        distanceTransform.localPosition = distanceTransform.localPosition.normalized * dist;
    }

    /// <summary>
    /// searches for overall bounds of all mesh renderes in children and sets camera distance to get them in focus
    /// </summary>
    public void ResetZoomToMeshBounds(Transform target, float marginToScreenEdgePercent = 0.1f, bool includeInactive = true)
    {
        var allBounds = target.GetComponentsInChildren<MeshRenderer>(includeInactive).Select(m => m.bounds);

        Vector3 averageCenter = Vector3.zero;
        foreach (var bounds in allBounds)
        {
            averageCenter += bounds.center;
        }
        averageCenter /= allBounds.Count();

        float minX = allBounds.Select(m => m.min.x).Min();
        float minY = allBounds.Select(m => m.min.y).Min();
        float minZ = allBounds.Select(m => m.min.z).Min();

        float maxX = allBounds.Select(m => m.max.x).Max();
        float maxY = allBounds.Select(m => m.max.y).Max();
        float maxZ = allBounds.Select(m => m.max.z).Max();

        Bounds combinedBounds = new Bounds(averageCenter, new Vector3(maxX - minX, maxY - minY, maxZ - minZ));

        ResetZoomToBounds(combinedBounds, marginToScreenEdgePercent);
    }

    public void ResetZoomToBounds(Bounds bounds, float marginToScreenEdgePercent = 0.1f)
    {
        // needed height or width if camera were centered at, and looking at mesh origin
        float neededFrustumSize = Mathf.Max(bounds.max.magnitude, bounds.min.magnitude) * 2f;
        neededFrustumSize *= (1 + marginToScreenEdgePercent);

        float isPortraitModifyer = cam.pixelWidth < cam.pixelHeight ? 1f / cam.aspect : 1f;

        // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        var distance = neededFrustumSize * isPortraitModifyer * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        distance += cam.nearClipPlane;

        SetZoomDistance(distance);
    }
}