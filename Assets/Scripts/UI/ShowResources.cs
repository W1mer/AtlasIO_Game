using UnityEngine;
using TMPro;
using System.Text;

public class ShowResources : MonoBehaviour
{
    TextMeshProUGUI text;
    ResourceManager resourceManager;

    [SerializeField]
    private ResourceData resourceData;
    void Start()
    {
        resourceManager = ResourceManager.Instance;

        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        ShowResource(resourceData);
    }

    public void ShowResource(ResourceData resourceData)
    {
        StringBuilder sb = new StringBuilder();

        int playerAmount = resourceManager.GetResourceAmount(resourceData);
        int playerMaining = resourceManager.GetResourceMaining(resourceData);
        string iconTag = $"<sprite name=\"{resourceData.resourceName.ToLower()}\">";

        sb.Append($"{iconTag} x{playerAmount}\n\n");
        sb.Append($"{iconTag} {playerMaining:+#;-#;0}/day");
        
        text.text = sb.ToString();
    }
}
