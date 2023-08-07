using UnityEngine;
using UnityEngine.UIElements;

public class TalentScreen : VisualElement
{
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
}