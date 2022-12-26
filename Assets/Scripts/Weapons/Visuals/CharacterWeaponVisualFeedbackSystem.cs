using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Rival;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct CharacterWeaponVisualFeedbackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        CharacterWeaponVisualFeedbackJob job = new CharacterWeaponVisualFeedbackJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ElapsedTime = (float)SystemAPI.Time.ElapsedTime,
            WeaponVisualFeedbackLookup = SystemAPI.GetComponentLookup<WeaponVisualFeedback>(true),
            WeaponControlLookup = SystemAPI.GetComponentLookup<WeaponControl>(true),
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false),
            MainEntityCameraLookup = SystemAPI.GetComponentLookup<MainEntityCamera>(false),
        };
        job.Schedule();
    }

    [BurstCompile]
    public partial struct CharacterWeaponVisualFeedbackJob : IJobEntity
    {
        public float DeltaTime;
        public float ElapsedTime;
        [ReadOnly]
        public ComponentLookup<WeaponVisualFeedback> WeaponVisualFeedbackLookup;
        [ReadOnly]
        public ComponentLookup<WeaponControl> WeaponControlLookup;
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public ComponentLookup<MainEntityCamera> MainEntityCameraLookup;

        void Execute(Entity entity, ref CharacterWeaponVisualFeedback characterWeaponVisualFeedback, in PlatformerCharacterComponent character, in KinematicCharacterBody characterBody, in ActiveWeapon activeWeapon)
        {
            bool isAiming = false;
            float characterMaxSpeed = characterBody.IsGrounded ? character.GroundRunMaxSpeed : character.AirMaxSpeed;

            if (WeaponVisualFeedbackLookup.TryGetComponent(activeWeapon.Entity, out WeaponVisualFeedback weaponFeedback))
            {
                float characterVelocityRatio = math.length(characterBody.RelativeVelocity) / characterMaxSpeed;

                // Weapon bob
                {
                    float3 targetBobPos = default;
                    if (characterBody.IsGrounded)
                    {
                        float bobSpeedMultiplier = isAiming ? weaponFeedback.WeaponBobAimRatio : 1f;
                        float hBob = math.sin(ElapsedTime * weaponFeedback.WeaponBobFrequency) * weaponFeedback.WeaponBobHAmount * bobSpeedMultiplier * characterVelocityRatio;
                        float vBob = ((math.sin(ElapsedTime * weaponFeedback.WeaponBobFrequency * 2f) * 0.5f) + 0.5f) * weaponFeedback.WeaponBobVAmount * bobSpeedMultiplier * characterVelocityRatio;
                        targetBobPos = new float3(hBob, vBob, 0f);
                    }

                    characterWeaponVisualFeedback.WeaponLocalPosBob = math.lerp(characterWeaponVisualFeedback.WeaponLocalPosBob, targetBobPos, math.saturate(weaponFeedback.WeaponBobSharpness * DeltaTime));
                }

                // Weapon recoil
                {
                    // Clamp current recoil
                    characterWeaponVisualFeedback.CurrentRecoil = math.clamp(characterWeaponVisualFeedback.CurrentRecoil, 0f, weaponFeedback.RecoilMaxDistance);
                    
                    // go towards recoil
                    if (characterWeaponVisualFeedback.WeaponLocalPosRecoil.z >= -characterWeaponVisualFeedback.CurrentRecoil * 0.99f)
                    {
                        characterWeaponVisualFeedback.WeaponLocalPosRecoil = math.lerp(characterWeaponVisualFeedback.WeaponLocalPosRecoil, math.forward() * -characterWeaponVisualFeedback.CurrentRecoil, math.saturate(weaponFeedback.RecoilSharpness * DeltaTime));
                    }
                    // go towards restitution
                    else
                    {
                        characterWeaponVisualFeedback.WeaponLocalPosRecoil = math.lerp(characterWeaponVisualFeedback.WeaponLocalPosRecoil, float3.zero, math.saturate(weaponFeedback.RecoilRestitutionSharpness * DeltaTime));
                        characterWeaponVisualFeedback.CurrentRecoil = -characterWeaponVisualFeedback.WeaponLocalPosRecoil.z;
                    }
                }

                // Final weapon pose
                float3 targetWeaponAnimSocketLocalPosition = characterWeaponVisualFeedback.WeaponLocalPosBob + characterWeaponVisualFeedback.WeaponLocalPosRecoil;
                LocalTransformLookup[character.WeaponAnimationSocketEntity] = LocalTransform.FromPosition(targetWeaponAnimSocketLocalPosition);
            }
        }
    }
}
