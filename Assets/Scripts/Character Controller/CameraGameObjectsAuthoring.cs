using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CameraGameObjectsAuthoring : MonoBehaviour
{
    public GameObject characterFollower;
    public GameObject targetFollower;
    public CinemachineVirtualCamera virtualCamera;

    class Baker : Baker<CameraGameObjectsAuthoring>
    {
        public override void Bake(CameraGameObjectsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponentObject(entity, new CameraGameObjects
            {
                characterFollower = authoring.characterFollower,
                targetFollower = authoring.targetFollower,
                virtualCamera = authoring.virtualCamera,
            });
        }
    }
}

public class CameraGameObjects : IComponentData
{
    public GameObject characterFollower;
    public GameObject targetFollower;
    public CinemachineVirtualCamera virtualCamera;

    public CameraGameObjects()
    {

    }
}