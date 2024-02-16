using System.Linq;
using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
    [Header("OrbitTransforms")]
    [SerializeField] Transform orbitCenter;
    [SerializeField] Transform camTransform;

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
            camTransform.RotateAround(orbitCenter.position, Vector3.up, yRotDelta);
            yRotSum += yRotDelta;
        }

        // vertical rotation
        var xRotDelta = rotValue.y * -xRotSpeed * 0.01f;
        // clamp
        if (!clampXAngle || (xRotSum + xRotDelta >= minXAngle && xRotSum + xRotDelta <= maxXAngle))
        {
            camTransform.RotateAround(orbitCenter.position, camTransform.right, xRotDelta);
            xRotSum += xRotDelta;
        }
    }

    public virtual void Zoom(float zoomValue)
    {
        float dist = (orbitCenter.position - camTransform.position).magnitude;
        float newDist = dist + zoomValue * zoomSpeed * 0.01f;
        newDist = Mathf.Clamp(newDist, minCamDistance, maxCamDistance);
        newDist = Mathf.Abs(newDist);
        SetZoomDistance(newDist);
    }

    public void SetZoomDistance(float dist)
    {
        camTransform.position = (camTransform.position - orbitCenter.position).normalized * dist;
    }

    public void ResetCamera()
    {
        //var camHeight = mapSize * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //Camera.main.transform.position = new Vector3(0, camHeight * 1.1f, 0);
        //Camera.main.transform.LookAt(Vector3.zero);
    }
}