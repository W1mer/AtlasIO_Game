using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    [SerializeField] private UpgradeUIManager upgradeUIManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject electricityIndicator;

    [SerializeField] private Animator animator;

    public GameObject indicatorInstance { get; private set; }
    public GameObject resourceIndicator { get; private set; }
    public BuildingData baseData;
    public ContinentData continentBuilded;
    public int hexIndex;
    public bool requireSpecificResource = false;
    public ResourceData requiredResourceType;
    public int currentLevel;


    protected ResourceDepositManager resourceDepositManager;
    protected ResourceManager resourceManager;
    protected SaveManager saveManager;

    public virtual void Start()
    {
        upgradeUIManager = FindAnyObjectByType<UpgradeUIManager>();
        resourceManager = ResourceManager.Instance;
        saveManager = SaveManager.Instance;
        resourceDepositManager = ResourceDepositManager.Instance;

        Animator foundAnimator;

        if (TryGetComponent(out foundAnimator))
        {
            animator = foundAnimator;
        }
        else if (transform.childCount > 1 && transform.GetChild(1) != null && transform.GetChild(1).TryGetComponent(out foundAnimator))
        {
            animator = foundAnimator;
        }


        if (baseData == null)
        {
            Debug.LogError("Base data is not assigned for this building.");
            return;
        }

        LoadElectricityIndicator();

        cameraController = Camera.main?.GetComponentInParent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogWarning("CameraController not found.");
        }

        if (currentLevel > 0)
        {
            resourceManager?.AddNewBuild(this);
            saveManager?.AddBuilding(this);
            GetComponent<BoxCollider>().enabled = true;

            if (animator != null)
            {
                animator.SetBool("CanMaining", true);
                Debug.Log("CanMaining после установки: " + animator.GetBool("CanMaining"));
            }

            //baseData.upgradeLevels[currentLevel].minings[0].last_amount = baseData.upgradeLevels[currentLevel].minings[0].amount;
        }
        else
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    protected void LoadElectricityIndicator()
    {
        electricityIndicator = Resources.Load<GameObject>("Indicators/electricity_indicator");

        if (electricityIndicator != null)
        {
            Vector3 indicatorPos = new Vector3(transform.position.x, 1, transform.position.z);
            indicatorInstance = Instantiate(electricityIndicator, indicatorPos, Quaternion.Euler(0, 90, 0));
            indicatorInstance.SetActive(false);
        }
        else
        {
            Debug.LogError("Не удалось загрузить electricity_indicator. Убедись, что он в папке Resources/Indicators");
        }
    }

    protected bool CanUpgrade()
    {
        return baseData != null && currentLevel < baseData.upgradeLevels.Count &&
               resourceManager.TrySpendResources(baseData.upgradeLevels[currentLevel].cost);
    }

    public List<ResourceCost> GetCurrentUpgradeCost()
    {
        if (baseData == null || currentLevel >= baseData.upgradeLevels.Count)
            return new List<ResourceCost>();

        return baseData.upgradeLevels[currentLevel].cost;
    }

    public void Upgrade()
    {
        if (baseData == null || currentLevel + 1 >= baseData.upgradeLevels.Count)
        {
            Debug.LogWarning("Улучшение невозможно: достигнут максимальный уровень или отсутствует baseData.");
            return;
        }

        if (CanUpgrade())
        {
            currentLevel++;

            OnUpgrade();

            baseData.upgradeLevels[currentLevel].minings[0].ChangeAmount(baseData.upgradeLevels[currentLevel].minings[0].amount);

            int i = 0;

            foreach (var res in baseData.upgradeLevels[currentLevel].expenses)
            {
                res.ChangeAmount(baseData.upgradeLevels[currentLevel].expenses[i].amount);
                i++;
            }

            resourceManager.CheckAllBuildingsOnMap();

            Debug.Log($"{baseData.TypeNumber} улучшено до уровня {currentLevel}");
        }
        else
        {
            Debug.LogWarning($"{baseData.TypeNumber} не хватает ресурсов на улучшение");
        }
    }

    public virtual void OnUpgrade()
    {
    }
    public virtual void OnDayEnd()
    {
        
    }

    public void OnClicked()
    {
        if (upgradeUIManager == null)
        {
            Debug.LogWarning("UpgradeUIManager не назначен");
            return;
        }

        upgradeUIManager.OpenUpgradePanel(this);

        if (cameraController != null)
        {
            cameraController.MoveToTarget(transform.position);
            cameraController.CanMove = false;
            cameraController.CanZoom = false;

        }
    }

    public bool CanMaining()
    {
        if (baseData == null || baseData.upgradeLevels.Count <= currentLevel)
            return false;

        List<ResourceCost> exp = new List<ResourceCost>(baseData.upgradeLevels[currentLevel].expenses);

        if (exp != null && resourceManager.TrySpendResources(exp) == false)
        {
            Debug.Log("Не хватает ресурсов для добычи");
            return false;
        }

        return true;
    }

    public int[] UpdateExpensesAmount(bool haveElectricity)
    {
        List<ResourceCost> exp = new List<ResourceCost>(baseData.upgradeLevels[currentLevel].expenses);

        int[] resourceExp = new int[exp.Count];

        if (resourceManager.CanSpendResources(exp) && haveElectricity)
        {
            int i = 0;

            foreach (var res in exp)
            {
                resourceExp[i] = res.amount;

                Debug.Log($"{res.resourceType} + {resourceExp[i]}");

                i++;
            }

            Debug.Log(resourceExp[0]);

            return resourceExp;
        }
        else //(haveElectricity == false || resourceManager.CanSpendResources(exp) == false)
        {
            foreach (var res in exp)
            {
                resourceExp.Append(0);
            }

            Debug.Log(resourceExp[0]);

            return resourceExp;
        }
    }

    public int UpdateMainingAmount(bool haveElectricity)
    {
        List<ResourceCost> exp = new List<ResourceCost>(baseData.upgradeLevels[currentLevel].expenses);

        if (haveElectricity && resourceManager.CanSpendResources(exp))
        {
            return baseData.upgradeLevels[currentLevel].minings[0].amount;
        }
        else if (haveElectricity == false || resourceManager.CanSpendResources(exp) == false)
        {
            return 0;
        }

        return baseData.upgradeLevels[currentLevel].minings[0].amount;
    }

    public int GetResourceAmountMainingPerCycle()
    {
        if (baseData == null || baseData.upgradeLevels.Count <= currentLevel)
            return 0;

        var levelData = baseData.upgradeLevels[currentLevel];

        if (levelData.minings == null || levelData.minings.Count == 0)
            return 0;

        if (levelData.expenses != null && CanMaining() == false)
        {
            return 0;
        }
        if (CanMaining())
        {
            return levelData.minings[0].amount;
        }

        return levelData.minings[0].amount;
    }

    public ResourceData GetResourceMaining()
    {
        if (baseData == null || baseData.upgradeLevels.Count <= currentLevel && baseData.upgradeLevels[currentLevel].minings != null)
            return null;

        return baseData.upgradeLevels[currentLevel].minings?[0].resourceType;
    }

    public void DestroyBuilding()
    {
        resourceManager?.RemoveBuilding(this);
        saveManager?.RemoveBuilding(this);

        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        Destroy(gameObject);

        resourceManager.CheckAllBuildingsOnMap();
    }

    public void SetElectricityIndicatorState(bool isActive)
    {
        if (indicatorInstance != null)
        {
            indicatorInstance.SetActive(isActive);
        }
    }

    public int GetElectricityConsumption()
    {
        if (baseData == null || baseData.upgradeLevels.Count <= currentLevel)
            return 0;

        var levelData = baseData.upgradeLevels[currentLevel];

        if (baseData.TypeNumber != 13)
        {
            return levelData.electricityConsumption?[0].amount ?? 0;
        }

        var expense = levelData.expenses?[0];
        if (expense == null || resourceManager == null)
            return 0;

        int available = resourceManager.GetResourceAmount(expense.resourceType);
        return available >= expense.amount ? levelData.electricityConsumption?[0].amount ?? 0 : 0;
    }
}