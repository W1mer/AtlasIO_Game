using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private List<HexagonMain> allHexes = new();
    [SerializeField] private List<GameObject> buildingPrefabs = new();

    [SerializeField]
    private bool isDeleting = false;

    private MapController mapController;

    private List<BuildingInstance> placedBuildings = new();

    private string SaveKey => "save"; // Ключ для PlayerPrefs
    private string SavePath => Application.persistentDataPath + "/savefile.json";

    public static SaveManager Instance { get; private set; }


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
        mapController = GetComponent<MapController>();

        allHexes = mapController.AllHexes;

        Debug.Log("AllHexes count at LoadGame: " + allHexes.Count);

        LoadGame();
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        foreach (var building in placedBuildings)
        {
            BuildingDataSave data = new BuildingDataSave
            {
                buildingID = building.baseData.TypeNumber,
                level = building.currentLevel,
                hexIndex = building.hexIndex,
                continentId = building.continentBuilded.Id
            };

            saveData.buildings.Add(data);
        }

        string json = JsonUtility.ToJson(saveData);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            Debug.Log("Сохранено в PlayerPrefs (WebGL)");
        }
        else
        {
            File.WriteAllText(SavePath, json);
            Debug.Log("Сохранено в файл: " + SavePath);
        }

        ResourceManager.Instance.SaveResources();
        ResourceDepositManager.Instance.SaveDeposits();
    }

    private void ClearMap()
    {
        foreach (var building in placedBuildings)
        {
            if (building != null)
                Destroy(building.gameObject);
        }

        placedBuildings.Clear();
    }

    public void AddBuilding(BuildingInstance building)
    {
        if (!placedBuildings.Contains(building))
            placedBuildings.Add(building);
    }

    public void RemoveBuilding(BuildingInstance building)
    {
        if (placedBuildings.Contains(building))
        {
            placedBuildings.Remove(building);

            // Обновляем хекс, на котором стояло здание
            if (building.hexIndex >= 0 && building.hexIndex < allHexes.Count)
            {
                allHexes[building.hexIndex].isOccupied = false;
            }

            Destroy(building.gameObject);
            SaveGame(); // Сохраняем изменения
            Debug.Log("Здание удалено и сохранение обновлено.");
        }
    }

    public void LoadGame()
    {
        string json = LoadFromStorage();

        ContinentDatabase continentDb = ContinentDatabase.Instance;

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Нет сохранённых данных для загрузки.");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        ClearMap();

        foreach (var data in saveData.buildings)
        {

            Debug.Log($"Попытка загрузить: ID={data.buildingID}, HexIndex={data.hexIndex}, PrefabsCount={buildingPrefabs.Count}");

            if (data.hexIndex < 0 || data.hexIndex >= allHexes.Count)
            {
                Debug.LogWarning($"Некорректный индекс хекса: {data.hexIndex}");
                continue;
            }

            var hex = allHexes[data.hexIndex];
            var prefab = buildingPrefabs[data.buildingID];

            var instance = Instantiate(prefab, hex.center, prefab.transform.rotation);

            BuildingInstance building = instance.GetComponent<BuildingInstance>();
            building.baseData.TypeNumber = data.buildingID;
            building.currentLevel = data.level;
            building.hexIndex = data.hexIndex;
            building.continentBuilded = continentDb.GetById(data.continentId);

            hex.isOccupied = true;
            hex.Highlight(false);

            placedBuildings.Add(building);
        }

        Debug.Log("Загрузка завершена, зданий: " + placedBuildings.Count);
    }

    private string LoadFromStorage()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return PlayerPrefs.HasKey("save") ? PlayerPrefs.GetString("save") : null;
        }
        else
        {
            string path = Application.persistentDataPath + "/savefile.json";

            Debug.Log(path);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
    }

    public void DeleteSave()
    {
        isDeleting = true;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            PlayerPrefs.DeleteAll();
            ResourceManager.Instance.DeleteResources();
        }
        else
        {
            string path = Application.persistentDataPath + "/savefile.json";

            Debug.Log(path);
            File.Delete(path);

            ResourceManager.Instance.DeleteResources();

            ClearMap();
        }

        ResourceDepositManager.Instance.DeleteSave();
        ResourceManager.Instance.DeleteResources();
    }

    void OnApplicationQuit()
    {
        if (isDeleting == false)
            SaveGame();
    }


}
