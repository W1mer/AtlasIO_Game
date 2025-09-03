using UnityEngine;

public class Factory : BuildingInstance
{
    [SerializeField] private GameObject asphalt;
    void Awake()
    {
        asphalt = transform.GetChild(1).gameObject;
    }
    public override void Start()
    {
        base.Start();
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
    }
}
