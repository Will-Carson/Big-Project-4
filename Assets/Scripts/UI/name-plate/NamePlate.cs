using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class NamePlate : VisualElement
{
    static class ClassNames
    {
        public static string NamePlateBackground = "name-plate__background";
        public static string NamePlateProgress = "name-plate__progress";
        public static string NamePlateTitle = "name-plate__title";
        public static string NamePlateLabel = "name-plate__label";
        public static string NamePlateContainer = "name-plate__container";
        public static string NamePlateTitleBackground = "name-plate__title_background";
    }

    public new class UxmlFactory : UxmlFactory<NamePlate, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlIntAttributeDescription _currentHealth = new UxmlIntAttributeDescription
        { name = "Current_Health", defaultValue = 0 };

        readonly UxmlIntAttributeDescription _maximumHealth = new UxmlIntAttributeDescription
        { name = "Maximum_Health", defaultValue = 100 };

        readonly UxmlStringAttributeDescription _namePlateTitle = new UxmlStringAttributeDescription
        { name = "Name_Plate_Title", defaultValue = string.Empty };

        readonly UxmlColorAttributeDescription _healthColor = new UxmlColorAttributeDescription
        { name = "Health_Color", defaultValue = Color.red };

        readonly UxmlColorAttributeDescription _barColor = new UxmlColorAttributeDescription
        { name = "Bar_Color", defaultValue = Color.gray };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var namePlate = (NamePlate)ve;
            namePlate.CurrentHealth = _currentHealth.GetValueFromBag(bag, cc);
            namePlate.MaximumHealth = _maximumHealth.GetValueFromBag(bag, cc);
            namePlate.NamePlateTitle = _namePlateTitle.GetValueFromBag(bag, cc);
            namePlate.HealthColor = _healthColor.GetValueFromBag(bag, cc);
            namePlate.BarColor = _barColor.GetValueFromBag(bag, cc);
        }
    }

    private float _currentHealth;
    private float _maximumHealth;
    private string _namePlateTitle;
    private readonly Label _titleLabel;
    private readonly Label _healthStat;
    private VisualElement _progress;
    private VisualElement _background;
    private VisualElement _titleBackground;

    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            if (value == _currentHealth)
                return;
            _currentHealth = value;
            SetHealth(_currentHealth, _maximumHealth);
        }
    }
    public float MaximumHealth
    {
        get => _maximumHealth;
        set
        {
            if (value == _maximumHealth)
                return;
            _maximumHealth = value;
            SetHealth(_currentHealth, _maximumHealth);
        }
    }


    public string NamePlateTitle
    {
        get => _namePlateTitle;
        set => _titleLabel.text = value;
    }

    public StyleColor HealthColor 
    {
        get => _progress.style.backgroundColor;
        set 
        {
            if (value == _progress.style.backgroundColor)
                return;
            _progress.style.backgroundColor = value;
        }
    }
    public StyleColor BarColor
    {
        get => _background.style.backgroundColor;
        set
        {
            if (value == _background.style.backgroundColor)
                return;
            _background.style.backgroundColor = value;
        }
    }

    public NamePlate()
    {
        _titleBackground = new VisualElement { name = "NamePlateTitleBackground" };
        _titleBackground.AddToClassList(ClassNames.NamePlateTitleBackground);
        Add(_titleBackground);

        _titleLabel = new Label() { name = "NamePlateTitle" };
        _titleLabel.AddToClassList(ClassNames.NamePlateTitle);
        _titleBackground.Add(_titleLabel);

        //AddToClassList(ClassNames.NamePlateContainer);
        //Add Elements and their class selectors to the Component.
        _background = new VisualElement { name = "NamePlateBackground" };
        _background.AddToClassList(ClassNames.NamePlateBackground);
        Add(_background);

        _progress = new VisualElement { name = "NamePlateProgress" };
        _progress.AddToClassList(ClassNames.NamePlateProgress);
        _background.Add(_progress);

        _healthStat = new Label() { name = "NamePlateStat" };
        _healthStat.AddToClassList(ClassNames.NamePlateLabel);
        _progress.Add(_healthStat);
    }

    private void SetHealth(float currentHealth, float maxHealth)
    {
        _healthStat.text = $"{currentHealth}/{maxHealth}";
        if (maxHealth > 0)
        {
            float w = math.clamp(currentHealth / maxHealth * 100, 0f, 100f);
            _progress.style.width = new StyleLength(Length.Percent(w));
        }
    }
}