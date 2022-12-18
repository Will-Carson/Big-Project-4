using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ActionCharacterAuthoring : MonoBehaviour
{


    public class Baker : Baker<ActionCharacterAuthoring>
    {
        public override void Bake(ActionCharacterAuthoring authoring)
        {
            AddComponent<Player>();
            AddComponent<PlayerCommands>();
        }
    }
}
