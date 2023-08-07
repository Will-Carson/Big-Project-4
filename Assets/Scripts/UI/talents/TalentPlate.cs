using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Needs to support:
/// displaying # of allocated points
/// change color if talent is allocated
/// fire event when clicked 
/// fire event when hovered
/// </summary>

public class TalentPlate : VisualElement
{
    public new class UxmlFactory : UxmlFactory<TalentPlate, UxmlTraits> { }

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

    TalentDefinition talent;
    int pointsAllocated;
    VisualElement background;
    Label talentAllocation;
    Button talentButton;
    string talentTooltip;

    public TalentDefinition Talent
    {
        get => talent;
        set
        {
            if (value == talent)
                return;
            talent = value;
            talentButton.text = talent.name;
            talentTooltip = talent.GenerateTooltip();
            PointsAllocated = 0;
        }
    }

    public int PointsAllocated
    {
        get => pointsAllocated;
        set
        {
            pointsAllocated = value;
            talentAllocation.text = $"{pointsAllocated} / {talent.maxTalentLevel}";
        }
    }

    public TalentPlate()
    {
        background = new VisualElement { name = "TalentPlateBackground" };
        background.style.flexDirection = FlexDirection.Row;
        //background.AddToClassList("");
        Add(background);

        talentAllocation = new Label { name = "TalentAllocation" };
        talentAllocation.text = "0/0";
        background.Add(talentAllocation);

        talentButton = new Button { name = "TalentButton" };
        talentButton.style.width = 240;
        talentButton.style.height = 40;
        talentButton.text = "Test";
        talentButton.RegisterCallback<MouseOverEvent>((type) =>
        {
            Debug.Log($"In {talent.talentName}!");
        });
        talentButton.RegisterCallback<MouseOutEvent>((type) =>
        {
            Debug.Log($"Out {talent.talentName}!");
        });
        background.Add(talentButton);
    }
}
