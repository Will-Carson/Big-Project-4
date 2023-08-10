using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ContainerAuthoring : MonoBehaviour
{
    class Baker : Baker<ContainerAuthoring>
    {
        public override void Bake(ContainerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddBuffer<ContainerChild>(entity);
            //AddBuffer<ContainerChildRestrictions>(entity);
            //AddComponent<ContainerRestrictions>(entity);
        }
    }
}
