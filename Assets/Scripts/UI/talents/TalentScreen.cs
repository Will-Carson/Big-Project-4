using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class TalentScreen : VisualElement
{
    public Action<TalentAllocationRequestRpc> talentClicked;

    Dictionary<int, VisualElement> columnsByLevel = new Dictionary<int, VisualElement>();
    Dictionary<Stat, TalentPlate> talents = new Dictionary<Stat, TalentPlate>();

    Toggle refundToggle;

    public new class UxmlFactory : UxmlFactory<TalentScreen, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlColorAttributeDescription allocatedColor = new UxmlColorAttributeDescription
        { name = "Allocated_Color", defaultValue = Color.red };

        readonly UxmlColorAttributeDescription unallocatedColor = new UxmlColorAttributeDescription
        { name = "Unallocated_Color", defaultValue = Color.gray };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
        }
    }

    public TalentScreen()
    {
        var background = new VisualElement { name = "background" };
        background.style.flexDirection = FlexDirection.Row;
        background.style.flexGrow = 1;
        //background.AddToClassList("");
        Add(background);

        var leftPanel = new VisualElement { name = "left-panel" };
        leftPanel.style.width = 400;
        leftPanel.style.backgroundColor = Color.gray;
        background.Add(leftPanel);

        var refundToggleBackground = new VisualElement { name = "refund-toggle-background" };
        refundToggleBackground.style.flexDirection = FlexDirection.Row;
        refundToggleBackground.style.width = 100;
        refundToggleBackground.style.height = 40;
        leftPanel.Add(refundToggleBackground);

        var refundToggleLabel = new Label { name = "refund-toggle-label" };
        refundToggleLabel.text = "Refund";
        refundToggleBackground.Add(refundToggleLabel);

        refundToggle = new Toggle { name = "refund-toggle" };
        refundToggleBackground.Add(refundToggle);

        var talentScrollView = new ScrollView { name = "talent-scroll-view" };
        talentScrollView.style.flexGrow = 1;
        background.Add(talentScrollView);

        var talentColumnParent = talentScrollView.Q<VisualElement>("unity-content-container");
        talentColumnParent.style.flexDirection = FlexDirection.Row;
        talentColumnParent.style.flexGrow = 1;
        talentColumnParent.style.alignItems = Align.Stretch;
        talentColumnParent.style.alignSelf = Align.Stretch;
        talentColumnParent.style.backgroundColor = Color.white;
    }

    public void BuildTalentScreen(TalentDefinition[] talentDefinitions, VisualTreeAsset talentColumnTemplate, VisualTreeAsset talentPlateTemplate)
    {
        var talentColumnParent = this.Q<VisualElement>("unity-content-container");
        foreach (var talent in talentDefinitions)
        {
            var talentPlateInstance = talentPlateTemplate.Instantiate();
            var talentPlate = talentPlateInstance.Q<TalentPlate>();
            talents.Add(talent.stat, talentPlate);
            talentPlate.Talent = talent;
            talentPlate.clicked += TalentPlate_clicked;

            if (columnsByLevel.TryGetValue(talent.levelRequirement, out var talentColumn))
            {
                talentColumn.Q<VisualElement>("talent-parent").Add(talentPlateInstance);
            }
            else
            {
                var newTalentColumn = talentColumnTemplate.Instantiate();
                newTalentColumn.Q<Label>("level-label").text = talent.levelRequirement.ToString();
                talentColumnParent.Add(newTalentColumn);
                columnsByLevel.Add(talent.levelRequirement, newTalentColumn);
                newTalentColumn.Q<VisualElement>("talent-parent").Add(talentPlateInstance);
            }
        }
    }

    private void TalentPlate_clicked(Stat stat)
    {
        talentClicked.Invoke(new TalentAllocationRequestRpc
        {
            stat = stat,
            refund = refundToggle.value,
        });
    }

    public void OnStatsChange(DynamicBuffer<StatElement> stats)
    {
        var talentsEnum = talents.GetEnumerator();

        while (talentsEnum.MoveNext())
        {
            var talent = talentsEnum.Current.Key;
            var plate = talentsEnum.Current.Value;

            var value = StatElement.GetStatValue(stats, talent);
            plate.PointsAllocated = value;
        }
    }
}