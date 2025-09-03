using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickDetector : MonoBehaviour
{
    [SerializeField] private LayerMask buildingLayer;
    private Camera mainCamera;
    private BuildingPlacer buildingPlacer;

    private BuildingInstance pressedBuilding = null;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("[ClickDetector] Main camera not found!");
    }

    void Start()
    {
        buildingPlacer = BuildingPlacer.Instance;
    }

    private void Update()
    {
        if (buildingPlacer.IsDragging) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 position = Mouse.current.position.ReadValue();
                pressedBuilding = GetBuildingUnderPosition(position);
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector2 position = Mouse.current.position.ReadValue();
                BuildingInstance releasedBuilding = GetBuildingUnderPosition(position);

                if (releasedBuilding != null && releasedBuilding == pressedBuilding)
                {
                    releasedBuilding.OnClicked();
                }

                pressedBuilding = null;
            }
        }
#elif UNITY_ANDROID || UNITY_IOS
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                Vector2 position = touch.position.ReadValue();
                pressedBuilding = GetBuildingUnderPosition(position);
            }

            if (touch.press.wasReleasedThisFrame)
            {
                Vector2 position = touch.position.ReadValue();
                BuildingInstance releasedBuilding = GetBuildingUnderPosition(position);

                if (releasedBuilding != null && releasedBuilding == pressedBuilding)
                {
                    releasedBuilding.OnClicked();
                }

                pressedBuilding = null;
            }
        }
#endif
    }

    private BuildingInstance GetBuildingUnderPosition(Vector2 screenPosition)
    {
        // Сначала проверим, есть ли BuildingInstance в UI
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var uiResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, uiResults);

        foreach (var result in uiResults)
        {
            if (result.gameObject.GetComponentInParent<BuildingInstance>() is BuildingInstance buildingFromUI)
            {
                return buildingFromUI;
            }

            // Если попали просто в UI — блокируем клик
            if (result.gameObject != null)
                return null;
        }

        // Если не попали в UI — проверяем мир
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, buildingLayer))
        {
            return hit.collider.GetComponent<BuildingInstance>();
        }

        return null;
    }
}
