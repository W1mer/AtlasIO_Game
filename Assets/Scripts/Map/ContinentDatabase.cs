using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ContinentDatabase : MonoBehaviour
{
    public static ContinentDatabase Instance;
    
    [SerializeField]
    private List<ContinentData> continents;

    public List<ContinentData> ContinentsCopy => new(continents);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public ContinentData GetById(int id)
    {
        return continents.FirstOrDefault(c => c.Id == id);
    }
}
