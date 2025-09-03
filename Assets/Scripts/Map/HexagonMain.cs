using UnityEngine;

public class HexagonMain : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    public Material highlightMaterial; // Материал для подсветки (например, зелёный)

    public Material redHighlightMaterial;
    public bool isOccupied; // Занят ли гексагон зданием
    public Vector3 center; // Центр гексагона

    public int Index;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
        center = transform.position; // Центр гексагона — его позиция
    }

    // Подсветить гексагон
    public void Highlight(bool highlight)
    {
        meshRenderer.material = highlight ? highlightMaterial : originalMaterial;
    }

    public void RedHighlight(bool highlight)
    {
        meshRenderer.material = highlight ? redHighlightMaterial : originalMaterial;
    }
}