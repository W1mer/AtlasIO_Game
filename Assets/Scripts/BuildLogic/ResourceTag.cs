using UnityEngine;

public class ResourceTag : MonoBehaviour
{
    public ResourceType Type;

    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            meshRenderer.enabled = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            meshRenderer.enabled = true;
        }
    }
}


public enum ResourceType
{
    Coal,
    Wood,
    Stone,
    Gold,
    Iron,
    Copper,
    Sand,
    Oil
}

