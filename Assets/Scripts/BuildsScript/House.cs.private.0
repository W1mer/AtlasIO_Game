using Unity.VisualScripting;
using UnityEngine;

public class House : BuildingInstance
{
    [SerializeField] private GameObject asphalt;
    void Awake()
    {
        asphalt = transform.GetChild(3).gameObject;
    }
    public override void Start()
    {
        base.Start();
        UpdateModel();
    }

    public override void OnUpgrade()
    {
        UpdateModel();
    }


    private void UpdateModel()
    {
        if (currentLevel == 0)
        {
            asphalt.SetActive(false);
        }
        if (currentLevel == 1)
        {
            asphalt.SetActive(true);
        }
        if (currentLevel == 2)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            asphalt.SetActive(true);
        }
        if (currentLevel == 3)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(true);
            asphalt.SetActive(true);
        }

    }
}
