using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Physics;
using Unity.CharacterController;
using System;
using System.Collections.Generic;

/// DREAMING
/// 
/// 
/// . Player shoots the podium
/// . If all players are on the podium, spawn a dream. Teleport the players to the dream.
/// . Initial encounter area is spawned. Two choice gates are spawned.
/// . A player runs through a choice gate. The other gate despawns. A new encounter area is spawned. <summary>

/// I have to be able to choose from a list of encounters
/// Encounters must have a list of tags and weights
/// When you defeat a boss and gain a key, you can jump off of an encounter


public struct DreamOrb : IComponentData
{

}

public struct Jumper : IComponentData
{
    public float delay;
    public float lastActivated;

    internal bool CanJump(double elapsedTime)
    {
        return lastActivated + delay < elapsedTime;
    }
}