using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacer : MonoBehaviour
{
    BuildingInstance building;
    RaycastHit hitToPlaycer;

    BuildingPlacer Instance;

    ResourceTag resourceChoosed;
    HexagonMain hexChoosed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {

    }

    public void StartPlacment(BuildingData prefab)
    {
        if (prefab = null) return;

        Instantiate(prefab);
    }

    private void ConfrimPlacment()
    {
        if (CanPlaceOnHex(hexChoosed, resourceChoosed))
        {
            
        }
    }

    private void CancelPlacment()
    {

    }

    private void CleanupPreview()
    {

    }

    private void UpdateRaycast()
    {

    }
<<<<<<< HEAD
    
=======

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

>>>>>>> parent of 2b0b618 (Delete many Continents logic)
    private bool CanPlaceOnHex(HexagonMain hex, ResourceTag resource)
    {
        if (hex.isOccupied) return false;

        if (building.requireSpecificResource)
        {
            if (resource == null) return false;

            if (resource.Type.ToString() != building.requiredResourceType.resourceName)
                return false;
        }

        return true;
    }

    private enum PlacementState
    {
        None,
        Preview
    }

}
