using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIManager : MonoBehaviour
{
    private ResourceManager resourceManager;
    private BuildingInstance currentInstance;
    public GameObject upgradePanel;
    public ResourceDisplay resourceDisplay;
    public Button upgradeButton;

    public void OpenUpgradePanel(BuildingInstance instance)
    {
        upgradePanel.SetActive(true);

        currentInstance = instance;
        resourceManager = ResourceManager.Instance;

        resourceDisplay.ShowCost(instance);

        Debug.Log("Открыл Юа для апргрейда");
    }

    private void Start()
    {
        upgradePanel.SetActive(true);
        upgradeButton.onClick.AddListener(UpgradeBuilding);
        upgradePanel.SetActive(false);
    }

    private void UpgradeBuilding()
    {
        if (currentInstance != null)
        {
            resourceManager.SubtrElectricityConsumption(currentInstance.GetElectricityConsumption());
            currentInstance.Upgrade();
            resourceManager.SumElectricityConsumption(currentInstance.GetElectricityConsumption());
            resourceDisplay.ShowCost(currentInstance); // обновить цену
        }
    }

    public void ClosePanel()
    {
        upgradePanel.SetActive(false);
        currentInstance = null;
    }

    public void DestroyBuilding()
    {
        currentInstance.DestroyBuilding();
    }
}
