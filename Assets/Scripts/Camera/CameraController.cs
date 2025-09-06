using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Touch Settings")]
    public float touchSensitivity = 0.01f;
    private Vector2 lastTouchPos;
    private bool isTouching;

    private float minX = -50f, maxX = 50f, minZ = -50f, maxZ = 50f;


    [Header("Target Follow")]
    public float followSpeed = 5f; // скорость движения к цели
    private Vector3? targetPosition = null; // null — нет цели
    private float targetSize = 4f;

    private Camera cam;

    public bool CanMove;
    public bool CanZoom;

    [Header("References")]
    public PinchZoomCamera pinchZoomCamera; // ссылка на твой скрипт зума

    private void Start()
    {
        touchSensitivity = 0.01f;
        moveSpeed = 3f;

        CanMove = true;
        CanZoom = true;
        cam = GetComponentInChildren<Camera>();
        pinchZoomCamera = GetComponent<PinchZoomCamera>();
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

    private void HandleKeyboardInput()
    {
        Vector3 input = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) input += Vector3.back;
        if (Keyboard.current.aKey.isPressed) input += Vector3.left;
        if (Keyboard.current.dKey.isPressed) input += Vector3.right;

        Vector3 movement = input.normalized * moveSpeed * Time.deltaTime;
        Vector3 targetPos = transform.position + transform.TransformDirection(movement);

        // Ограничение по x и z
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.z = Mathf.Clamp(targetPos.z, minZ, maxZ);

        transform.position = targetPos;

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

        // Можно задать и дефолтный зум, если нужно:
        if (pinchZoomCamera != null)
        {
            pinchZoomCamera.MoveZoomTo(4f);
        }
    }

    private void ChangeLimit()
    {
        
    }


    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        int activeTouchCount = 0;
        if (Touchscreen.current.primaryTouch.press.isPressed) activeTouchCount++;

        if (Touchscreen.current.touches.Count > 1 && Touchscreen.current.touches[1].press.isPressed)
        {
            activeTouchCount++;
        }

        if (activeTouchCount > 1)
        {
            isTouching = false;
            return;
        }

        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();

            if (IsTouchOverUI(touchPos))
            {
                isTouching = false;
                return;
            }

            if (!isTouching)
            {
                isTouching = true;
                lastTouchPos = touchPos;
            }
            else
            {
                Vector2 delta = touchPos - lastTouchPos;
                if (delta.magnitude < 1f) return;

                lastTouchPos = touchPos;

                Vector3 move = new Vector3(-delta.x, 0f, -delta.y) * touchSensitivity;
                transform.Translate(move, Space.Self);

                // Ограничение позиции камеры после перемещения
                Vector3 clampedPos = transform.position;
                clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
                clampedPos.z = Mathf.Clamp(clampedPos.z, minZ, maxZ);
                transform.position = clampedPos;
            }
        }
        else
        {
            isTouching = false;
        }
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
            {
                return true;
            }
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

    // Метод для перемещения камеры к зданию и установки зума
    public void MoveToTarget(Vector3 buildingPosition)
    {
        float height = 10f;
        float distance = 10f;

        Vector3 offset = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * new Vector3(0, height, -distance);
        targetPosition = buildingPosition + offset;

        targetSize = 2.5f;

        // Плавно меняем зум через скрипт зума
        if (pinchZoomCamera != null)
        {
            pinchZoomCamera.MoveZoomTo(targetSize);
        }

        CanMove = false;
        CanZoom = false;
    }

    public void SetCanMove()
    {
        CanMove = true;
        CanZoom = true;

        // При возврате контроля — сбросить зум к дефолту
        if (pinchZoomCamera != null)
        {
            pinchZoomCamera.MoveZoomTo(4f);
        }
    }
}
