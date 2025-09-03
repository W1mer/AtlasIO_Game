using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    public BuildingInstance buildingPrefab; // Префаб здания для этой кнопки
    private BuildingPlacer buildingPlacer;

    void Start()
    {
        buildingPlacer = FindAnyObjectByType<BuildingPlacer>();
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        if (buildingPlacer.CurrentBuilding != null)
        {
            buildingPlacer.CancelPlacement();
        }

        buildingPlacer.SelectBuilding(buildingPrefab);
    }
}