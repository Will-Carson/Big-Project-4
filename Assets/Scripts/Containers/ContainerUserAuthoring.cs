using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ContainerUserAuthoring : MonoBehaviour
{
    class Baker : Baker<ContainerUserAuthoring>
    {
        public override void Bake(ContainerUserAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<SelectedItem>(entity);
            AddBuffer<ContainerChild>(entity);
        }
    }
}
