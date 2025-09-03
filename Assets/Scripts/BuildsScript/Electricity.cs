using UnityEngine;

public class Electricity : BuildingInstance
{
    [SerializeField] private GameObject asphalt;
    void Awake()
    {
        if (transform.childCount > 1)
            if (transform.GetChild(1).TryGetComponent<Animator>(out Animator T) == false)
                asphalt = transform.GetChild(1).gameObject;
    }
    public override void Start()
    {
        base.Start();
        UpdateModel();
    }
    private void UpdateModel()
    {
        if (currentLevel == 0 && asphalt != null)
        {
            asphalt.SetActive(false);
        }
        if (currentLevel == 1 && asphalt != null)
        {
            asphalt.SetActive(true);
        }
    }
}
