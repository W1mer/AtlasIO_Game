using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Map/Continent")]
public class ContinentData : ScriptableObject
{
    public int Id;
    public List<ResourceData> resources;

    public List<ResourceCost> startingResources = new List<ResourceCost>();

    public Vector3 newContinentPos = new Vector2(); // новая позиция материка
    public Vector2 xLimits = new Vector2();
    public Vector2 zLimits = new Vector2();
}
