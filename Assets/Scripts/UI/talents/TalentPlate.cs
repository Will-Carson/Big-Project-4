using System;
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
    public Action<Stat> clicked;

    TalentDefinition talent;
    float pointsAllocated;
    VisualElement background;
    Label talentAllocation;
    Button talentButton;
    string talentTooltip;

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
            talentAllocation.text = $"{(int)PointsAllocated} / {talent.maxTalentLevel}";
        }
    }

    public float PointsAllocated
    {
        get => pointsAllocated;
        set
        {
            if (value == pointsAllocated)
                return;
            pointsAllocated = value;
            talentAllocation.text = $"{(int)pointsAllocated} / {talent.maxTalentLevel}";
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

        talentButton = new Button { name = "talent-button" };
        talentButton.style.width = 240;
        talentButton.style.height = 40;
        talentButton.text = "Test";
        talentButton.clicked += TalentButton_clicked;
        talentButton.RegisterCallback<MouseOverEvent>((type) =>
        {

        });
        talentButton.RegisterCallback<MouseOutEvent>((type) =>
        {

        });
        background.Add(talentButton);
    }

    private void TalentButton_clicked()
    {
        clicked.Invoke(talent.stat);
    }
}
