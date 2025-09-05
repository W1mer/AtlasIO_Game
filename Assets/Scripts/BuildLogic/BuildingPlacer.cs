using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacer : MonoBehaviour
{
    [SerializeField] private BuildingInstance selectedBuildingPrefab;
    [SerializeField] private ResourceDisplay infoUi;
    [SerializeField] private Material phantomMaterial;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private ResourceTag resourceTag;

    private ResourceManager resourceManager;

    private HexagonMain currentHexagon;
    private HexagonMain lastHexagon;


    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero; // вспомогательная переменная для SmoothDamp
    private float smoothTime = 0.14f; // время сглаживания движения

    public static BuildingPlacer Instance;
    public GameObject CurrentBuilding { get; private set; }
    public bool IsDragging { get; private set; }

    public event Action<BuildingInstance> OnStartDragging;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 144;
        resourceManager = ResourceManager.Instance;
        saveManager = SaveManager.Instance;
    }

    void Update()
    {
        if (!IsDragging || CurrentBuilding == null) return;

        UpdateRaycastFromCameraCenter();

        // Плавное движение с инерцией
        CurrentBuilding.transform.position = Vector3.SmoothDamp(CurrentBuilding.transform.position, targetPosition, ref velocity, smoothTime);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryToConfirmPlacement();
        }

    }

    public void SelectBuilding(BuildingInstance prefab)
    {
        if (IsDragging) return;

        selectedBuildingPrefab = prefab;
        StartDragging();
    }

    public void StartDragging()
    {
        if (selectedBuildingPrefab == null) return;


        CurrentBuilding = Instantiate(selectedBuildingPrefab.gameObject);

        BuildingInstance instance = selectedBuildingPrefab.GetComponent<BuildingInstance>();

        OnStartDragging?.Invoke(instance);

        instance.GetComponent<BoxCollider>().enabled = false;

        ApplyPhantomMaterial(CurrentBuilding);

        IsDragging = true;

        targetPosition = CurrentBuilding.transform.position;
        velocity = Vector3.zero;
    }

    private void UpdateRaycastFromCameraCenter()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.45f, 0.4f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        HexagonMain hex = null;
        resourceTag = null;

        foreach (var hit in hits)
        {
            int layer = hit.collider.gameObject.layer;

            if (layer == LayerMask.NameToLayer("Hexagon") && hex == null)
            {
                hex = hit.collider.GetComponent<HexagonMain>();
                currentHexagon = hex;
            }
        }

        // теперь мы точно знаем hex, можно искать resourceTag, который над ним
        if (hex != null)
        {
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("ResourceHex"))
                {
                    // Проверяем: collider точно совпадает по позиции
                    if (Vector3.Distance(hit.collider.transform.position, hex.transform.position) < 0.1f)
                    {
                        resourceTag = hit.collider.GetComponent<ResourceTag>();
                        break;
                    }
                }
            }

            // теперь логика проверки
            if (hex != lastHexagon)
            {
                if (lastHexagon != null)
                {
                    lastHexagon.Highlight(false);
                    lastHexagon.RedHighlight(false);
                }

                if (hex.isOccupied == false && CanPlaceOnHex(currentHexagon, resourceTag) == true) // ✅ проверка ресурса
                {
                    hex.Highlight(true);
                }
                else
                {
                    hex.RedHighlight(true);
                    currentHexagon = null;
                }

                lastHexagon = hex;
            }
        }
        else
        {
            if (lastHexagon != null)
            {
                lastHexagon.Highlight(false);
                lastHexagon.RedHighlight(false);
                lastHexagon = null;
            }

            currentHexagon = null;
        }

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
        SetTargetPositionToHex(hex);
    }


    private void SetTargetPositionToHex(HexagonMain hex)
    {
        Vector3 offset = Vector3.up * 0.5f;
        targetPosition = hex.center + offset;
    }

    private void TryToConfirmPlacement()
    {
        if (currentHexagon == null || currentHexagon.isOccupied)
        {
            Debug.Log("Can't place: no valid hex.");
            return;
        }

        IsDragging = false;
    }

    // private bool CanPlaceBuilding() {

    //     if (resourceManager.HasEnoughResources(selectedBuildingPrefab.buildingInstance.GetCurrentUpgradeCost()) == false)
    //     {
    //         Debug.Log("Не хватает ресурсов");
    //         return false;
    //     }

    //     return true;
    // }

    public void ConfirmPlacement()
    {
        if (CanPlaceOnHex(currentHexagon, resourceTag) == false|| resourceManager.TrySpendResources(selectedBuildingPrefab.baseData.upgradeLevels[0].cost) == false) 
        {
            CancelPlacement();

            return;
        }

        var instance = Instantiate(selectedBuildingPrefab, currentHexagon.center, selectedBuildingPrefab.transform.rotation);

        currentHexagon.isOccupied = true;
        currentHexagon.Highlight(false);

        // Получаем компонент Building и заполняем данные
        BuildingInstance building = instance.GetComponent<BuildingInstance>();
        
        if (building != null)
        {
            building.hexIndex = currentHexagon.Index;
            building.currentLevel = 1;
            building.continentBuilded = resourceManager.CurrentContinent;

            // Добавляем в список зданий SaveManager
            if (saveManager != null)
                saveManager.AddBuilding(building);
            else
                Debug.LogWarning("SaveManager не назначен в BuildingPlacer");
        }
        else
        {
            Debug.LogWarning("У префаба нет компонента Building!");
        }

        Destroy(CurrentBuilding);

        currentHexagon = null;
        selectedBuildingPrefab = null;
        CurrentBuilding = null;
        IsDragging = false;

        building.GetComponent<BoxCollider>().enabled = true;
    }

    public void CancelPlacement()
    {
        if (lastHexagon != null)
            lastHexagon.Highlight(false);

        Destroy(CurrentBuilding);

        currentHexagon = null;
        lastHexagon = null;
        selectedBuildingPrefab = null;
        CurrentBuilding = null;
        IsDragging = false;
    }

    private void ApplyPhantomMaterial(GameObject building)
    {
        foreach (var renderer in building.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = phantomMaterial;
        }
    }

    private bool CanPlaceOnHex(HexagonMain hex, ResourceTag resource)
    {
        if (hex.isOccupied) return false;

        if (selectedBuildingPrefab.requireSpecificResource)
        {
            if (resource == null) return false;
            if (resource.Type.ToString() != selectedBuildingPrefab.requiredResourceType.resourceName)
                return false;
        }

        return true;
    }
}
