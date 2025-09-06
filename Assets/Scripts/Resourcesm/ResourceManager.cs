using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    [System.Serializable]
    public class ResourceEntry
    {
        public ResourceData resource;
        public int amount;
    }

    [SerializeField] private List<BuildingInstance> allBuildingsOnMap = new();
    [SerializeField] private List<ResourceEntry> resources = new();
    [SerializeField] private TextMeshProUGUI electricityText;
    [SerializeField] private TextMeshProUGUI dayTimerText;

    [SerializeField] private float dayTiming = 5f;

    private Dictionary<ResourceData, int> ActiveDict = new();
    public int electricityConsumptionMain;
    public int electricityMain;

    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeDictionary();
        LoadResources();
    }

    private void Start()
    {
        dayTiming = 60;

        CheckAllBuildingsOnMap();

        StartCoroutine(DayCycle());
        StartCoroutine(DayTimer());
        UpdateElectricityShow();
    }

    private void InitializeDictionary()
    {
        ActiveDict.Clear();

        foreach (var entry in resources)
        {
            if (entry.resource != null)
                ActiveDict[entry.resource] = entry.amount;
        }
    }

    private IEnumerator DayCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(dayTiming);

            try
            {
                List<BuildingInstance> workingBuildings = new();
                int localElectricity = electricityMain;

                // 1. Проверяем здания
                foreach (var building in allBuildingsOnMap)
                {
                    if (building == null) continue;

                    int consumption = building.GetElectricityConsumption();

                    if (localElectricity < -consumption)
                    {
                        building.SetElectricityIndicatorState(true);
                        continue;
                    }

                    var expenses = building.baseData.upgradeLevels[building.currentLevel].expenses;
                    if (!CanSpendResources(expenses))
                    {
                        building.SetElectricityIndicatorState(true);
                        continue;
                    }

                    workingBuildings.Add(building);
                    localElectricity += consumption; // резервируем энергию
                }

                // 2. Списываем расходы
                foreach (var building in workingBuildings)
                {
                    var expenses = building.baseData.upgradeLevels[building.currentLevel].expenses;
                    TrySpendResources(expenses);
                    electricityMain += building.GetElectricityConsumption();
                }

                // 3. Производим добычу
                foreach (var building in workingBuildings)
                {
                    int amount = building.GetResourceAmountMainingPerCycle();
                    ResourceData resource = building.GetResourceMaining();

                    if (resource != null && amount > 0)
                    {
                        AddResource(resource, amount);
                        Debug.Log($"{building.baseData.name} добыл {amount} {resource.name}");
                    }

                    building.OnDayEnd();
                    building.SetElectricityIndicatorState(false);
                }

                CheckAllBuildingsOnMap();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DayCycle] Ошибка: {ex}");
            }
        }
    }

    public IEnumerator DayTimer()
    {
        float duration = dayTiming;
        float remainingTime = duration;

        while (true)
        {
            while (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;

                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

                if (dayTimerText != null)
                    dayTimerText.text = $"До конца дня: {formattedTime}";

                yield return null;
            }
            remainingTime = duration;
        }
    }

    private void UpdateElectricityShow()
    {
        if (electricityText != null)
        {
            electricityText.text = $"{electricityConsumptionMain} / {electricityMain}";
        }
    }

    public void CheckAllBuildingsOnMap()
    {
        int localElectricity = 0;

        electricityMain = 0;
        electricityConsumptionMain = 0;

        ClearResourceMaining();

        if (allBuildingsOnMap == null || allBuildingsOnMap.Count == 0)
        {
            UpdateElectricityShow();
            ClearResourceMaining();

            Debug.Log("Пустой список зданий, обнуляем электроэнергию");
            return;
        }

        var buildingsSortedByEnergyPriority = allBuildingsOnMap
            .Where(b => b != null)
            .Select(b => new { building = b, consumption = b.GetElectricityConsumption() })
            .OrderByDescending(b => b.consumption)
            .ToList();

        foreach (var entry in buildingsSortedByEnergyPriority)
        {
            UpdateElectricityCon(entry.building, entry.consumption, ref localElectricity);
        }

        UpdateElectricityShow();
    }

    private void UpdateElectricityCon(BuildingInstance building, int consumption, ref int localElectricity)
    {
        if (consumption > 0)
        {
            localElectricity += consumption;
            building.SetElectricityIndicatorState(false);
        }
        else if (consumption < 0)
        {
            if (localElectricity >= -consumption)
            {
                localElectricity += consumption;
                building.SetElectricityIndicatorState(false);
                UpdateExpensesAmount(building, true);
                UpdateMainingAmount(building, true);
            }
            else
            {
                building.SetElectricityIndicatorState(true);
                UpdateExpensesAmount(building, false);
                UpdateMainingAmount(building, false);
            }
        }

        SumElectricityConsumption(consumption);
    }

    private void UpdateMainingAmount(BuildingInstance building, bool haveElectricity)
    {
        if (building != null)
        {
            ResourceData resource = building.GetResourceMaining();

            if (resource != null)
            {
                resource.MainingCount += building.UpdateMainingAmount(haveElectricity);
            }
        }
    }

    private void UpdateExpensesAmount(BuildingInstance building, bool haveElectricity)
    {
        if (building != null)
        {
            int i = 0;

            foreach (var res in building.baseData.upgradeLevels[building.currentLevel].expenses)
            {
                res.resourceType.MainingCount -= building.UpdateExpensesAmount(haveElectricity)[i];
                i++;
            }
        }
    }

    private void ClearResourceMaining()
    {
        foreach (var resource in GetAllResources())
        {
            resource.Key.MainingCount = 0;
        }
    }

    private string GetSavePath() =>
        Application.persistentDataPath + "/resources_Main.json";

    public bool HasEnoughResources(List<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            if (!ActiveDict.ContainsKey(cost.resourceType) || ActiveDict[cost.resourceType] < cost.amount)
                return false;
        }

        return true;
    }

    public bool TrySpendResources(List<ResourceCost> costs)
    {
        if (costs != null && !HasEnoughResources(costs))
            return false;

        foreach (var cost in costs)
        {
            ActiveDict[cost.resourceType] -= cost.amount;
        }

        SaveResources();
        return true;
    }

    public bool CanSpendResources(List<ResourceCost> costs)
    {
        if (costs != null && !HasEnoughResources(costs))
            return false;

        return true;
    }

    public void AddResource(ResourceData resource, int amount)
    {
        if (ActiveDict.ContainsKey(resource))
            ActiveDict[resource] += amount;
        else
            ActiveDict[resource] = amount;

        SaveResources();
    }

    public int GetResourceMaining(ResourceData resource)
    {
        return resource.MainingCount;
    }

    public int GetResourceAmount(ResourceData resource)
    {
        return ActiveDict.ContainsKey(resource) ? ActiveDict[resource] : 0;
    }

    public void SaveResources()
    {
        var data = new ResourceSaveData();

        foreach (var pair in ActiveDict)
        {
            data.savedResources.Add(new ResourceEntry { resource = pair.Key, amount = pair.Value });
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(GetSavePath(), json);
    }

    public void LoadResources()
    {
        string path = GetSavePath();

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<ResourceSaveData>(json);

        ActiveDict.Clear();
        foreach (var entry in data.savedResources)
        {
            if (entry.resource != null)
                ActiveDict[entry.resource] = entry.amount;
        }

        Debug.Log("[ResourceManager] Ресурсы загружены");
    }

    public void DeleteResources()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[ResourceManager] Удалён файл: {Path.GetFileName(path)}");
        }

        ActiveDict.Clear();
        resources.Clear();

        Debug.Log("[ResourceManager] Все ресурсы удалены");
    }

    public Dictionary<ResourceData, int> GetAllResources() => new(ActiveDict);

    public void AddNewBuild(BuildingInstance build)
    {
        if (!allBuildingsOnMap.Contains(build))
            allBuildingsOnMap.Add(build);

        CheckAllBuildingsOnMap();
    }

    public void RemoveBuilding(BuildingInstance build)
    {
        allBuildingsOnMap.Remove(build);
        CheckAllBuildingsOnMap();
    }

    public void SubtrElectricityConsumption(int buildElectricityConsumption)
    {
        if (buildElectricityConsumption < 0)
            electricityConsumptionMain += buildElectricityConsumption;
        else
            electricityMain -= buildElectricityConsumption;

        UpdateElectricityShow();
    }

    public void SumElectricityConsumption(int buildElectricityConsumption)
    {
        if (buildElectricityConsumption < 0)
            electricityConsumptionMain += -buildElectricityConsumption;
        else
            electricityMain += buildElectricityConsumption;

        UpdateElectricityShow();
    }

    [System.Serializable]
    private class ResourceSaveData
    {
        public List<ResourceEntry> savedResources = new List<ResourceEntry>();
    }

    [ContextMenu("Log All Resources")]
    private void LogAllResources()
    {
        foreach (var pair in ActiveDict)
        {
            Debug.Log($"[ResourceManager] {pair.Key.name}: {pair.Value}");
        }
    }

    [ContextMenu("Sync Dict → List")]
    private void SyncDictToList()
    {
        resources.Clear();
        foreach (var pair in ActiveDict)
        {
            resources.Add(new ResourceEntry { resource = pair.Key, amount = pair.Value });
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
