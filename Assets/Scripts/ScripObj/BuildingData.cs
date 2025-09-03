using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Resource System/Building")]
public class BuildingData : ScriptableObject
{
    public int TypeNumber;

    [System.Serializable]
    public class UpgradeLevel
    {
        public string levelName;
        public List<ResourceCost> cost;

        public List<ResourceCost> minings;

        public List<ResourceCost> expenses;

        public List<ResourceCost> electricityConsumption;
    }

    public List<UpgradeLevel> upgradeLevels;

}
