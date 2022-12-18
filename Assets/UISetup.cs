using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation 
    | WorldSystemFilterFlags.Default)]
public partial class UISetup : SystemBase
{
    Dictionary<Button, string> tooltips = new Dictionary<Button, string>();

    protected override void OnUpdate() 
    {
        TalentUIElementSetup();
        Enabled = false;
    }

    private void TalentUIElementSetup()
    {
        var talentsRoot = Object.FindObjectOfType<UIDocument>().rootVisualElement.Q("talents-root");

        var talentColumnParent = talentsRoot.Q("talent-column-parent");
        var talentColumnTemplate = Resources.Load<VisualTreeAsset>("talent-column");
        var talentButtonTemplate = Resources.Load<VisualTreeAsset>("talent-button");

        var tooltip = talentsRoot.Q<Label>("tooltip-text");
        var talentRefundToggle = talentsRoot.Q<Toggle>("talent-refund-toggle");

        var talents = TalentDefinitions.Talents;
        var talentColumnDictionary = new Dictionary<int, TemplateContainer>();

        foreach (var talent in talents)
        {
            // If necessary build the new column for this talent, otherwise get an existing column
            if (!talentColumnDictionary.TryGetValue(talent.levelRequirement, out var column))
            {
                column = talentColumnTemplate.Instantiate();
                talentColumnDictionary.Add(talent.levelRequirement, column);

                var levelLabel = column.Q<Label>("level-label");
                levelLabel.text = talent.levelRequirement.ToString();

                column.style.flexShrink = 0; // I have to set this here like a damn fool
                talentColumnParent.Add(column);
            }

            // Build the talent button info and add it to its column
            var talentButton = talentButtonTemplate.Instantiate();
            var button = talentButton.Q<Button>("button");
            button.text = talent.name;
            talentButton.Q<Label>("points").text = "0 / " + talent.maxTalentLevel.ToString();

            // build the tooltip string
            var tooltipString = $"{talent.name}\n\nRequirements:\n\n";
            
            foreach (var requirement in talent.requires)
            {
                tooltipString += requirement.ToString() + "\n";
            }

            tooltipString += "\nGrants:\n\n";

            foreach (var granted in talent.grants)
            {
                tooltipString += granted.ToString() + "\n";
            }

            tooltips.Add(button, tooltipString);

            // Register the callback
            button.RegisterCallback<MouseEnterEvent>(e => tooltip.text = tooltips[button]);
            button.RegisterCallback<MouseLeaveEvent>(e => tooltip.text = "");
            button.RegisterCallback<MouseUpEvent>(e => Debug.Log($"stat: {talent.stat}, refund: {talentRefundToggle.value}"));
            button.RegisterCallback<MouseUpEvent>(e => SystemAPI.GetSingleton<NetworkClientSystem.Singleton>().SendRpc(
                new TalentAllocationRequestRPC
                    {
                        deallocate = talentRefundToggle.value,
                        stat = talent.stat,
                    }));

            column.Add(talentButton);
        }
    }
}
