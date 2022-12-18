using UnityEngine;
using UnityEngine.UIElements;

public class UISetup : MonoBehaviour
{
    private void Start()
    {
        TalentUIElementSetup();
    }

    private void TalentUIElementSetup()
    {
        var talentsRoot = FindObjectOfType<UIDocument>().rootVisualElement.Q("talents-root");

        var item = new VisualElement();
        item.style.width = 100;
        item.style.height = 100;
        item.style.backgroundColor = Color.white;

        talentsRoot.Add(item);

        // Get all talents
        // Create tooltips for the talents
        // Populate talent-columns with different talents
    }
}
