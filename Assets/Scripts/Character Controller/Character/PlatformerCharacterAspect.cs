using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Rival;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Material = Unity.Physics.Material;
using Unity.NetCode;

public struct PlatformerCharacterUpdateContext
{
    public int ChunkIndex;
    public EntityCommandBuffer.ParallelWriter EndFrameECB;
    [ReadOnly] public ComponentLookup<CharacterFrictionModifier> CharacterFrictionModifierLookup;
    [ReadOnly] public BufferLookup<LinkedEntityGroup> LinkedEntityGroupLookup;
    [ReadOnly] public ComponentLookup<WeaponVisualFeedback> WeaponVisualFeedbackLookup;
    [ReadOnly] public ComponentLookup<WeaponControl> WeaponControlLookup;
    [ReadOnly] public ComponentLookup<InterpolationDelay> InterpolationDelayLookup;

    public void OnIterateEntity(int chunkIndex)
    {
        ChunkIndex = chunkIndex;
    }

    public void OnSystemCreate(ref SystemState state)
    {
        CharacterFrictionModifierLookup = state.GetComponentLookup<CharacterFrictionModifier>(true);
        LinkedEntityGroupLookup = state.GetBufferLookup<LinkedEntityGroup>(true);
        WeaponVisualFeedbackLookup = state.GetComponentLookup<WeaponVisualFeedback>(true);
        WeaponControlLookup = state.GetComponentLookup<WeaponControl>(true);
        InterpolationDelayLookup = state.GetComponentLookup<InterpolationDelay>(true);
    }

    public void OnSystemUpdate(ref SystemState state, EntityCommandBuffer endFrameECB)
    {
        EndFrameECB = endFrameECB.AsParallelWriter();
        CharacterFrictionModifierLookup.Update(ref state);
        LinkedEntityGroupLookup.Update(ref state);
        WeaponVisualFeedbackLookup.Update(ref state);
        WeaponControlLookup.Update(ref state);
        InterpolationDelayLookup.Update(ref state);
    }
}

public readonly partial struct PlatformerCharacterAspect : IAspect, IKinematicCharacterProcessor<PlatformerCharacterUpdateContext>
{
    public readonly KinematicCharacterAspect CharacterAspect;
    public readonly RefRW<PlatformerCharacterComponent> Character;
    public readonly RefRW<PlatformerCharacterControl> CharacterControl;
    public readonly RefRW<PlatformerCharacterStateMachine> StateMachine;
    public readonly RefRW<CustomGravity> CustomGravity;

    public readonly DynamicBuffer<StatContainer> StatContainer;
    public readonly RefRW<ActiveWeapon> ActiveWeapon;

    public void PhysicsUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref PlatformerCharacterComponent character = ref Character.ValueRW;
        ref PlatformerCharacterControl characterControl = ref CharacterControl.ValueRW;
        ref PlatformerCharacterStateMachine stateMachine = ref StateMachine.ValueRW;

        // Common pre-update logic across states
        {
            // Handle initial state transition
            if (stateMachine.CurrentState == CharacterState.Uninitialized)
            {
                stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in this);
            }

            if (characterControl.JumpHeld)
            {
                character.HeldJumpTimeCounter += baseContext.Time.DeltaTime;
            }
            else
            {
                character.HeldJumpTimeCounter = 0f;
                character.AllowHeldJumpInAir = false;
            }
            if (characterControl.JumpPressed)
            {
                character.LastTimeJumpPressed = (float)baseContext.Time.ElapsedTime;
            }
            
            character.HasDetectedMoveAgainstWall = false;
            if (characterBody.IsGrounded)
            {
                character.LastTimeWasGrounded = (float)baseContext.Time.ElapsedTime;
                
                character.CurrentUngroundedJumps = 0;
                character.AllowJumpAfterBecameUngrounded = true;
                character.AllowHeldJumpInAir = true;
            }
            if (character.LedgeGrabBlockCounter > 0f)
            {
                character.LedgeGrabBlockCounter -= baseContext.Time.DeltaTime;
            }
        }
        
        stateMachine.OnStatePhysicsUpdate(stateMachine.CurrentState, ref context, ref baseContext, in this);
        
        // Common post-update logic across states
        {
            character.JumpPressedBeforeBecameGrounded = false;
        }
    }

    public void VariableUpdate(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref PlatformerCharacterStateMachine stateMachine = ref StateMachine.ValueRW;
        ref quaternion characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;
        
        KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation, characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);
        stateMachine.OnStateVariableUpdate(stateMachine.CurrentState, ref context, ref baseContext, in this);
    }

    public bool DetectGlobalTransitions(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref var stateMachine = ref StateMachine.ValueRW;
        ref var characterControl = ref CharacterControl.ValueRW;

        // If health is 0, transition to "dead" state.
        

        if (stateMachine.CurrentState != CharacterState.Swimming && stateMachine.CurrentState != CharacterState.FlyingNoCollisions)
        {
            if (SwimmingState.DetectWaterZones(ref context, ref baseContext, in this, out float3 tmpDirection, out float tmpDistance))
            {
                if (tmpDistance < 0f)
                {
                    stateMachine.TransitionToState(CharacterState.Swimming, ref context, ref baseContext, in this);
                    return true;
                }
            }
        }

        if (characterControl.FlyNoCollisionsPressed)
        {
            if (stateMachine.CurrentState == CharacterState.FlyingNoCollisions)
            {
                stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in this);
                return true;
            }
            else
            {
                stateMachine.TransitionToState(CharacterState.FlyingNoCollisions, ref context, ref baseContext, in this);
                return true;
            }
        }

        return false;
    }

    public void HandlePhysicsUpdatePhase1(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        bool allowParentHandling,
        bool allowGroundingDetection)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        
        CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
        if (allowParentHandling)
        {
            CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        }
        if (allowGroundingDetection)
        {
            CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        }
    }

    public void HandlePhysicsUpdatePhase2(
        ref PlatformerCharacterUpdateContext context, 
        ref KinematicCharacterUpdateContext baseContext,
        bool allowPreventGroundingFromFutureSlopeChange,
        bool allowGroundingPushing,
        bool allowMovementAndDecollisions,
        bool allowMovingPlatformDetection,
        bool allowParentHandling)
    {
        ref PlatformerCharacterComponent character = ref Character.ValueRW;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        CustomGravity customGravity = CustomGravity.ValueRO;

        if (allowPreventGroundingFromFutureSlopeChange)
        {
            CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in character.StepAndSlopeHandling);
        }
        if (allowGroundingPushing)
        {
            CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, customGravity.Gravity);
        }
        if (allowMovementAndDecollisions)
        {
            CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        }
        if (allowMovingPlatformDetection)
        {
            CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        }
        if (allowParentHandling)
        {
            CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        }
        CharacterAspect.Update_ProcessStatefulCharacterHits();
    }

    public unsafe void SetCapsuleGeometry(CapsuleGeometry capsuleGeometry)
    {
        ref PhysicsCollider physicsCollider = ref CharacterAspect.PhysicsCollider.ValueRW;
        
        CapsuleCollider* capsuleCollider = (CapsuleCollider*)physicsCollider.ColliderPtr;
        capsuleCollider->Geometry = capsuleGeometry;
    }

    public float3 GetGeometryCenter(CapsuleGeometryDefinition geometry)
    {
        float3 characterPosition = CharacterAspect.LocalTransform.ValueRW.Position;
        quaternion characterRotation = CharacterAspect.LocalTransform.ValueRW.Rotation;

        RigidTransform characterTransform = new RigidTransform(characterRotation, characterPosition);
        float3 geometryCenter = math.transform(characterTransform, geometry.Center);
        return geometryCenter;
    }

    public unsafe void SetCollisionResponse(CollisionResponsePolicy collisionResponse)
    {
        ref PhysicsCollider physicsCollider = ref CharacterAspect.PhysicsCollider.ValueRW;
        
        CapsuleCollider* capsuleCollider = ((CapsuleCollider*)physicsCollider.ColliderPtr);
        Material mat = capsuleCollider->Material;
        mat.CollisionResponse = collisionResponse;
        capsuleCollider->Material = mat;
    }

    public unsafe bool CanStandUp(ref PlatformerCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref PhysicsCollider physicsCollider = ref CharacterAspect.PhysicsCollider.ValueRW;
        ref PlatformerCharacterComponent character = ref Character.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        ref quaternion characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;
        ref KinematicCharacterProperties characterProperties = ref CharacterAspect.CharacterProperties.ValueRW;
        
        // Overlap test with standing geometry to see if we have space to stand
        CapsuleCollider* capsuleCollider = ((CapsuleCollider*)physicsCollider.ColliderPtr);

        CapsuleGeometry initialGeometry = capsuleCollider->Geometry;
        capsuleCollider->Geometry = character.StandingGeometry.ToCapsuleGeometry();

        bool isObstructed = false;
        if (CharacterAspect.CalculateDistanceClosestCollisions(
                in this,
                ref context,
                ref baseContext,
                characterPosition,
                characterRotation,
                0f,
                characterProperties.ShouldIgnoreDynamicBodies(),
                out DistanceHit hit))
        {
            isObstructed = true;
        }

        capsuleCollider->Geometry = initialGeometry;

        return !isObstructed;
    }

    public static bool CanBeAffectedByWindZone(CharacterState currentCharacterState)
    {
        if (currentCharacterState == CharacterState.GroundMove ||
            currentCharacterState == CharacterState.AirMove ||
            currentCharacterState == CharacterState.Crouched ||
            currentCharacterState == CharacterState.Rolling)
        {
            return true;
        }

        return false;
    }

    public static CapsuleGeometry CreateCharacterCapsuleGeometry(float radius, float height, bool centered)
    {
        height = math.max(height, radius * 2f);
        float halfHeight = height * 0.5f;

        return new CapsuleGeometry
        {
            Radius = radius,
            Vertex0 = centered ? (-math.up() * (halfHeight - radius)) : (math.up() * radius),
            Vertex1 = centered ? (math.up() * (halfHeight - radius)) : (math.up() * (height - radius)),
        };
    }

    public static void GetCommonMoveVectorFromPlayerInput(in PlatformerPlayerInputs inputs, quaternion lookRotation, out float3 moveVector)
    {
        moveVector = (math.mul(lookRotation, math.right()) * inputs.Move.x) + (math.mul(lookRotation, math.forward()) * inputs.Move.y);
    }

    public static void HandleWeaponSubstate(
        EntityCommandBuffer.ParallelWriter commandBuffer,
        int chunkIndex,
        ActiveWeapon activeWeapon,
        ComponentLookup<WeaponControl> weaponControlLookup,
        ComponentLookup<InterpolationDelay> interpolationDelayLookup,
        PlatformerCharacterControl control,
        uint delay)
    {
        // Weapon
        if (weaponControlLookup.TryGetComponent(activeWeapon.entity, out var weaponControl))
        {
            var interpolationDelay = interpolationDelayLookup[activeWeapon.entity];

            // Shoot
            weaponControl.Fire1Pressed = control.Fire1Pressed;
            weaponControl.Fire1Released = control.Fire1Released;
            weaponControl.Fire2Pressed = control.Fire2Pressed;
            weaponControl.Fire2Released = control.Fire2Released;

            // Interp delay
            //interpolationDelay.Value = commandDataInterpolationDelayLookup[owningPlayer.entity].Delay;
            interpolationDelay.Value = delay;

            commandBuffer.SetComponent(chunkIndex, activeWeapon.entity, weaponControl);
            commandBuffer.SetComponent(chunkIndex, activeWeapon.entity, interpolationDelay);
        }
    }

    #region Character Processor Callbacks
    public void UpdateGroundingUp(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        
        CharacterAspect.Default_UpdateGroundingUp(ref characterBody);
    }
    
    public bool CanCollideWithHit(
        ref PlatformerCharacterUpdateContext context, 
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit)
    {
        return KinematicCharacterUtilities.IsHitCollidableOrCharacter(
            in baseContext.StoredCharacterBodyPropertiesLookup, 
            hit.Material, 
            hit.Entity);
    }

    public bool IsGroundedOnHit(
        ref PlatformerCharacterUpdateContext context, 
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit, 
        int groundingEvaluationType)
    {
        PlatformerCharacterComponent characterComponent = Character.ValueRO;
        
        return CharacterAspect.Default_IsGroundedOnHit(
            in this,
            ref context,
            ref baseContext,
            in hit,
            in characterComponent.StepAndSlopeHandling,
            groundingEvaluationType);
    }

    public void OnMovementHit(
            ref PlatformerCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref KinematicCharacterHit hit,
            ref float3 remainingMovementDirection,
            ref float remainingMovementLength,
            float3 originalVelocityDirection,
            float hitDistance)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        PlatformerCharacterComponent characterComponent = Character.ValueRO;
        
        CharacterAspect.Default_OnMovementHit(
            in this,
            ref context,
            ref baseContext,
            ref characterBody,
            ref characterPosition,
            ref hit,
            ref remainingMovementDirection,
            ref remainingMovementLength,
            originalVelocityDirection,
            hitDistance,
            characterComponent.StepAndSlopeHandling.StepHandling,
            characterComponent.StepAndSlopeHandling.MaxStepHeight);
    }

    public void OverrideDynamicHitMasses(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref PhysicsMass characterMass,
        ref PhysicsMass otherMass,
        BasicHit hit)
    {
    }

    public void ProjectVelocityOnHits(
        ref PlatformerCharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref float3 velocity,
        ref bool characterIsGrounded,
        ref BasicHit characterGroundHit,
        in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
        float3 originalVelocityDirection)
    {
        PlatformerCharacterComponent characterComponent = Character.ValueRO;
        
        CharacterAspect.Default_ProjectVelocityOnHits(
            ref velocity,
            ref characterIsGrounded,
            ref characterGroundHit,
            in velocityProjectionHits,
            originalVelocityDirection,
            characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);
    }
    #endregion
}
