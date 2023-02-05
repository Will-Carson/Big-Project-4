using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlatformerMonsterAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlatformerMonsterAuthoring>
    {
        public override void Bake(PlatformerMonsterAuthoring authoring)
        {
            AddComponent(new PlatformerMonster());
            AddComponent(new PlatformerMonsterInputs());
        }
    }
}