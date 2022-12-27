using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ParticleAuthoring : MonoBehaviour
{
    public class Baker : Baker<ParticleAuthoring>
    {
        public override void Bake(ParticleAuthoring authoring)
        {
            AddComponent(new Particle());
        }
    }
}
