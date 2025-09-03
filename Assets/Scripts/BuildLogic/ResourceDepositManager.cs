using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using Unity.VisualScripting;

[System.Serializable]
public class ContinentDepositsEntry
{
    public ContinentData continent;
    public List<ResourceDeposits> deposits;
}

public class ResourceDepositManager : MonoBehaviour
{
    public static ResourceDepositManager Instance { get; private set; }

    private List<ResourceDeposits> allDeposits = new List<ResourceDeposits>();

    [SerializeField]
    private List<ContinentDepositsEntry> depositsOnContinent = new();

    public List<ContinentDepositsEntry> DepositsOnContinentCopy => new(depositsOnContinent);

    private const string saveFileName = "deposits_save.json";

    [System.Serializable]
    private class DepositSaveData
    {
        public int Id;
        public bool IsDiscovered;
    }

    [System.Serializable]
    private class SaveWrapper
    {
        public List<DepositSaveData> deposits = new List<DepositSaveData>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterDeposit(ResourceDeposits deposit)
    {
        allDeposits.Add(deposit);
    }

    void Start()
    {
        StartCoroutine(LoadCor());
    }

    private IEnumerator LoadCor()
    {
        yield return new WaitForSeconds(0.1f);

        LoadDeposits();
    }

    public void SaveDeposits()
    {
        SaveWrapper wrapper = new SaveWrapper();

        foreach (var deposit in allDeposits)
        {
            wrapper.deposits.Add(new DepositSaveData
            {
                Id = deposit.Id,
                IsDiscovered = deposit.IsDiscovered
            });
        }

        string json = JsonUtility.ToJson(wrapper);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            PlayerPrefs.SetString(saveFileName, json);
            PlayerPrefs.Save();
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            File.WriteAllText(path, json);
        }
    }

    public void LoadDeposits()
    {
        string json = null;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            json = PlayerPrefs.GetString(saveFileName, null);
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(path))
                json = File.ReadAllText(path);
        }

        if (!string.IsNullOrEmpty(json))
        {
            SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(json);
            foreach (var data in wrapper.deposits)
            {
                var deposit = allDeposits.Find(d => d.Id == data.Id);
                if (deposit != null)
                    deposit.SetDiscovered(data.IsDiscovered);
            }
        }
    }

    public void DeleteSave()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            PlayerPrefs.DeleteAll();
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
