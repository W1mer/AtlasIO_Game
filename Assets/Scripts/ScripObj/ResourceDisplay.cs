using UnityEngine;
using TMPro;
using System.Text;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private BuildingPlacer buildingPlacer;
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private ResourceManager resourceManager;
    public TextMeshProUGUI costText;

    private void Awake()
    {
        //resourceManager = ResourceManager.Instance;
        //buildingPlacer = BuildingPlacer.Instance;
    }
    void OnEnable()
    {
        buildingPlacer.OnStartDragging += ShowCost;
    }
    void OnDisable()
    {
        buildingPlacer.OnStartDragging -= ShowCost;
    }

    public void ShowCost(BuildingInstance buildingInstace)
    {
        buildPanel.SetActive(true);

        if (buildingInstace == null || costText == null)
        {
            Debug.LogWarning("Не назначен instance или costText");
            return;
        }

        var costs = buildingInstace.GetCurrentUpgradeCost();
        StringBuilder sb = new StringBuilder();

        foreach (var cost in costs)
        {
            if (cost == null)
            {
                Debug.LogWarning("В списке costs найден null элемент");
                continue;
            }
            if (cost.resourceType == null)
            {
                Debug.LogWarning("ResourceCost.resourceType == null");
                continue;
            }
            if (string.IsNullOrEmpty(cost.resourceType.resourceName))
            {
                Debug.LogWarning("resourceType.resourceName пустой или null");
                continue;
            }

            string iconTag = $"<sprite name=\"{cost.resourceType.resourceName.ToLower()}\">";
            int playerAmount = resourceManager.GetResourceAmount(cost.resourceType);


            string color = playerAmount >= cost.amount ? "green" : "red";
            sb.Append($"<color={color}>{iconTag} x{cost.amount}</color> \n");
        }

        costText.text = sb.ToString();
    }

    public void ShowResource(ResourceData resourceData)
    {
        StringBuilder sb = new StringBuilder();

        int playerAmount = resourceManager.GetResourceAmount(resourceData);
        string iconTag = $"<sprite name=\"{resourceData.resourceName.ToLower()}\">";

        sb.Append($"{iconTag} x{playerAmount}\n");
    }
}
