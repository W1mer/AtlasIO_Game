using System.Collections.Generic;

[System.Serializable]
public class BuildingDataSave
{
    public int buildingID;
    public int level;
    public int hexIndex;
    public int continentId; 
}

[System.Serializable]
public class SaveData
{
    public List<BuildingDataSave> buildings = new List<BuildingDataSave>();
}
