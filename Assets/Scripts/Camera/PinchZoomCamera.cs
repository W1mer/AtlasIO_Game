using UnityEngine;
using UnityEngine.InputSystem;

public class PinchZoomCamera : MonoBehaviour
{
    public Camera cam;
    public float zoomSpeed = 0.5f;
    public float minZoom = 2.5f;
    public float maxZoom = 10f;
    public float smoothSpeed = 0.2f;

    public CameraController cameraController;

    private float targetZoomLevel;
    private float zoomVelocity = 0f;
    private float? forcedZoomTarget = null;
    private float lastTouchDistance = -1f;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        if (cameraController == null) cameraController = GetComponent<CameraController>();

        targetZoomLevel = cam.orthographicSize;
    }

    private void Update()
    {
        if (cam == null || cameraController == null) return;

        if (!cameraController.CanZoom && !forcedZoomTarget.HasValue)
            return;

        if (forcedZoomTarget.HasValue)
        {
            SetTargetZoom(forcedZoomTarget.Value);

            if (Mathf.Abs(cam.orthographicSize - forcedZoomTarget.Value) < 0.01f)
            {
                cam.orthographicSize = forcedZoomTarget.Value;
                targetZoomLevel = forcedZoomTarget.Value;
                forcedZoomTarget = null;
                zoomVelocity = 0f;
                return;
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseWheelZoom();
#else
        HandleTouchPinchZoom();
#endif

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoomLevel,
            ref zoomVelocity,
            smoothSpeed
        );
    }

    private void HandleMouseWheelZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float zoomChange = scroll * zoomSpeed * 0.1f;
            SetTargetZoom(targetZoomLevel - zoomChange);
        }
    }

    private void HandleTouchPinchZoom()
    {
        if (Touchscreen.current == null || Touchscreen.current.touches.Count < 2)
        {
            lastTouchDistance = -1f;
            return;
        }

        var touch0 = Touchscreen.current.touches[0];
        var touch1 = Touchscreen.current.touches[1];

        if (!touch0.isInProgress || !touch1.isInProgress)
        {
            lastTouchDistance = -1f;
            return;
        }

        Vector2 pos0 = touch0.position.ReadValue();
        Vector2 pos1 = touch1.position.ReadValue();
        float currentDistance = Vector2.Distance(pos0, pos1);

        if (lastTouchDistance > 0f)
        {
            float delta = currentDistance - lastTouchDistance;
            float zoomChange = -delta * zoomSpeed * 0.01f;
            SetTargetZoom(targetZoomLevel + zoomChange);
        }

        lastTouchDistance = currentDistance;
    }

    private void SetTargetZoom(float newZoom)
    {
        targetZoomLevel = Mathf.Clamp(newZoom, minZoom, maxZoom);
    }

    public void MoveZoomTo(float size)
    {
        forcedZoomTarget = Mathf.Clamp(size, minZoom, maxZoom);
    }

    public float GetCurrentZoomLevel()
    {
        return cam.orthographicSize;
    }
}
