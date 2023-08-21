using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TalentDefinition", menuName = "ARPG/Talent Definition", order = 1)]
public class TalentDefinition : ScriptableObject
{
    public string talentName;
    public Stat stat;
    public int pointCost;
    public int levelRequirement;
    public int maxTalentLevel;

    // Stats required to allocate a talent
    public StatRangeElement[] requires;

    // Stats granted by allocating a talent
    public StatElement[] grants;

    public string GenerateTooltip()
    {
        var tooltip = $"{talentName}\n" +
            $"\n" +
            $"Cost: {pointCost}\n" +
            $"\n";

        if (requires.Length > 0)
        {
            tooltip += "Requirements:\n";
            foreach (var requirement in requires)
            {
                tooltip += $"{requirement.stat}: at least {requirement.range.min}";
                tooltip += (requirement.range.max == float.MaxValue) ? "\n" : $", at most {requirement.range.max}\n";
            }
        }

        if (grants.Length > 0)
        {
            tooltip += "Grants:\n";

            foreach (var granted in grants)
            {
                tooltip += $"{granted.stat} - {granted.value}\n";
            }
        }

        return tooltip;
    }
}