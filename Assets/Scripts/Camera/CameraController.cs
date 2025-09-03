using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed = 9f;
    public float minMoveSpeed = 2f;

    [Header("Touch Settings")]
    public float touchSensitivity = 0.01f;
    private Vector2 lastTouchPos;
    private int lastFingerId = -1;

    private float minX = -50f, maxX = 50f, minZ = -50f, maxZ = 50f;

    [Header("Target Follow")]
    public float followSpeed = 5f;
    private Vector3? targetPosition = null;
    private float targetSize = 4f;

    private Camera cam;
    public bool CanMove = true;
    public bool CanZoom = true;

    [Header("References")]
    public PinchZoomCamera pinchZoomCamera;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        pinchZoomCamera = GetComponent<PinchZoomCamera>();

        baseMoveSpeed = 9f;
        minMoveSpeed = 2f;
    }

    private void Update()
    {
        if (CanMove)
        {
            HandleKeyboardInput();
            HandleTouchInput();
        }

        HandleSmoothMoveToTarget();
    }

    private float GetAdaptiveMoveSpeed()
    {
        if (pinchZoomCamera == null || cam == null)
            return baseMoveSpeed;

        float zoomFactor = Mathf.InverseLerp(
            pinchZoomCamera.minZoom,
            pinchZoomCamera.maxZoom,
            cam.orthographicSize
        );

        // При ближнем зуме скорость → minMoveSpeed, при дальнем → baseMoveSpeed
        return Mathf.Lerp(minMoveSpeed, baseMoveSpeed, zoomFactor);
    }

    private void HandleKeyboardInput()
    {
        Vector3 input = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) input += Vector3.back;
        if (Keyboard.current.aKey.isPressed) input += Vector3.left;
        if (Keyboard.current.dKey.isPressed) input += Vector3.right;

        Vector3 movement = input.normalized * GetAdaptiveMoveSpeed() * Time.deltaTime;
        Vector3 targetPos = transform.position + transform.TransformDirection(movement);

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        transform.position = targetPos;
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        if (Touchscreen.current.touches.Count != 1) // только один палец
        {
            lastFingerId = -1;
            return;
        }

        var touch = Touchscreen.current.primaryTouch;

        if (!touch.press.isPressed)
        {
            lastFingerId = -1;
            return;
        }

        Vector2 touchPos = touch.position.ReadValue();

        if (IsTouchOverUI(touchPos))
        {
            lastFingerId = -1;
            return;
        }

        if (lastFingerId != touch.touchId.ReadValue())
        {
            lastFingerId = touch.touchId.ReadValue();
            lastTouchPos = touchPos;
            return;
        }

        Vector2 delta = touchPos - lastTouchPos;
        lastTouchPos = touchPos;

        if (delta.sqrMagnitude < 1f) return;

        Vector3 move = new Vector3(-delta.x, 0f, -delta.y) * touchSensitivity * GetAdaptiveMoveSpeed();
        transform.Translate(move, Space.Self);

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.z = Mathf.Clamp(clampedPos.z, minZ, maxZ);
        transform.position = clampedPos;
    }

    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent<CanvasRenderer>(out _))
                return true;
        }

        return false;
    }

    private void HandleSmoothMoveToTarget()
    {
        if (targetPosition.HasValue)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.Value, followSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition.Value) < 0.01f)
            {
                transform.position = targetPosition.Value;
                targetPosition = null;
            }
        }
    }

    public void SwitchRegion(Vector3 newPosition, Vector2 xLimits, Vector2 zLimits)
    {
        targetPosition = newPosition;

        minX = xLimits.x;
        maxX = xLimits.y;
        minZ = zLimits.x;
        maxZ = zLimits.y;

        CanMove = true;
        CanZoom = true;

        if (pinchZoomCamera != null)
            pinchZoomCamera.MoveZoomTo(4f);
    }

    public void MoveToTarget(Vector3 buildingPosition)
    {
        float height = 10f;
        float distance = 10f;

        Vector3 offset = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * new Vector3(0, height, -distance);
        targetPosition = buildingPosition + offset;

        targetSize = 2.5f;

        if (pinchZoomCamera != null)
            pinchZoomCamera.MoveZoomTo(targetSize);

        CanMove = false;
        CanZoom = false;
    }

    public void SetCanMove()
    {
        CanMove = true;
        CanZoom = true;

        if (pinchZoomCamera != null)
            pinchZoomCamera.MoveZoomTo(4f);
    }
}
