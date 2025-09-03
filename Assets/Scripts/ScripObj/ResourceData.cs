using UnityEngine;

[CreateAssetMenu(menuName = "Resource System/Resource")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public Sprite icon;

    public int MainingCount;
}
