using UnityEngine;
using System;
using System.ComponentModel;

public class EncounterComponent : MonoBehaviour
{
    public EncounterAuthoring encounter;
}

[Serializable]
public struct EncounterAuthoring
{
    public EncounterFlags tags;
    public float weight;
    public GameObject prefab;
}
