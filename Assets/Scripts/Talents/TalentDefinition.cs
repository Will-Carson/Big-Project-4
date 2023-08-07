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
    public Requirement[] requires;

    // Stats granted by allocating a talent
    public Granted[] grants;
}

[Serializable]
public struct Requirement
{
    public Stat stat;
    public Range range;
}

[Serializable]
public struct Granted
{
    public Stat stat;
    public float value;
}