using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapController : MonoBehaviour
{
    private List<HexagonMain> allHexes = new List<HexagonMain>();


    public List<HexagonMain> AllHexes => allHexes;
    void Awake()
    {
        CollectAndIndexHexagons();
    }
    
    void CollectAndIndexHexagons()
{
    HexagonMain[] hexesInScene = FindObjectsByType<HexagonMain>(FindObjectsSortMode.None);

    // Сортировка: сначала по Z (строки), потом по X (столбцы)
    var sortedHexes = hexesInScene.OrderBy(h => h.transform.position.z)
                                  .ThenBy(h => h.transform.position.x)
                                  .ToList();

    allHexes = sortedHexes;

    for (int i = 0; i < allHexes.Count; i++)
    {
        allHexes[i].Index = i;
    }
}


}
