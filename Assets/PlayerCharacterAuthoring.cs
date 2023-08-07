using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerCharacterAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerCharacterAuthoring>
    {
        public override void Bake(PlayerCharacterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<TalentsComponent>(entity);
        }
    }
}
