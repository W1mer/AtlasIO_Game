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
