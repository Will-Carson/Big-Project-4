using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.CharacterController;
using Unity.Physics;

[Serializable]
public struct PlatformerCharacterComponent : IComponentData
{
    [Header("References")]
    public Entity MeshRootEntity;
    public Entity RopePrefabEntity;
    public Entity RollballMeshEntity;
    public Entity WeaponAnimationSocketEntity;

    [Header("Ground movement")]
    public float GroundRunMaxSpeed;
    public float GroundSprintMaxSpeed;
    public float GroundedMovementSharpness;
    public float GroundedRotationSharpness;

    [Header("Crouching")]
    public float CrouchedMaxSpeed;
    public float CrouchedMovementSharpness;
    public float CrouchedRotationSharpness;

    [Header("Air movement")]
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float AirRotationSharpness;

    [Header("Rolling")]
    public float RollingAcceleration;

    [Header("Wall run")]
    public float WallRunAcceleration;
    public float WallRunMaxSpeed;
    public float WallRunDrag;
    public float WallRunGravityFactor;
    public float WallRunJumpRatioFromCharacterUp;
    public float WallRunDetectionDistance;

    [Header("Flying")]
    public float FlyingMaxSpeed;
    public float FlyingMovementSharpness;

    [Header("Jumping")]
    public float GroundJumpSpeed;
    public float AirJumpSpeed;
    public float WallRunJumpSpeed;
    public float JumpHeldAcceleration;
    public float MaxHeldJumpTime;
    public byte MaxUngroundedJumps;
    public float JumpAfterUngroundedGraceTime;
    public float JumpBeforeGroundedGraceTime;

    [Header("Ledge Detection")]
    public float LedgeMoveSpeed;
    public float LedgeRotationSharpness;
    public float LedgeSurfaceProbingHeight;
    public float LedgeSurfaceObstructionProbingHeight;
    public float LedgeSideProbingLength;

    [Header("Dashing")]
    public float DashDuration;
    public float DashSpeed;

    [Header("Swimming")]
    public float SwimmingAcceleration;
    public float SwimmingMaxSpeed;
    public float SwimmingDrag;
    public float SwimmingRotationSharpness;
    public float SwimmingStandUpDistanceFromSurface;
    public float WaterDetectionDistance;
    public float SwimmingJumpSpeed;
    public float SwimmingSurfaceDiveThreshold;

    [Header("RopeSwing")]
    public float RopeSwingAcceleration;
    public float RopeSwingMaxSpeed;
    public float RopeSwingDrag;
    public float RopeLength;
    public float3 LocalRopeAnchorPoint;

    [Header("Climbing")]
    public float ClimbingDistanceFromSurface;
    public float ClimbingSpeed;
    public float ClimbingMovementSharpness;
    public float ClimbingRotationSharpness;

    [Header("Step & Slope")]
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    [Header("Misc")]
    public CustomPhysicsBodyTags StickySurfaceTag;
    public CustomPhysicsBodyTags ClimbableTag;
    public float UpOrientationAdaptationSharpness;
    public CapsuleGeometryDefinition StandingGeometry;
    public CapsuleGeometryDefinition CrouchingGeometry;
    public CapsuleGeometryDefinition RollingGeometry;
    public CapsuleGeometryDefinition ClimbingGeometry;
    public CapsuleGeometryDefinition SwimmingGeometry;
    public CollisionFilter WaterPhysicsCategory;
    public CollisionFilter RopeAnchorCategory;

    [HideInInspector]
    public float3 LocalLedgeDetectionPoint;
    [HideInInspector]
    public float3 LocalSwimmingDetectionPoint;
    [HideInInspector]
    public byte CurrentUngroundedJumps;
    [HideInInspector]
    public float HeldJumpTimeCounter;
    [HideInInspector]
    public bool JumpPressedBeforeBecameGrounded;
    [HideInInspector]
    public bool AllowJumpAfterBecameUngrounded;
    [HideInInspector]
    public bool AllowHeldJumpInAir;
    [HideInInspector]
    public float LastTimeJumpPressed;
    [HideInInspector]
    public float LastTimeWasGrounded;
    [HideInInspector]
    public bool HasDetectedMoveAgainstWall;
    [HideInInspector]
    public float3 LastKnownWallNormal;
    [HideInInspector]
    public float LedgeGrabBlockCounter;
    [HideInInspector]
    public float DistanceFromWaterSurface;
    [HideInInspector]
    public float3 DirectionToWaterSurface;
    [HideInInspector]
    public bool IsSprinting;
    [HideInInspector]
    public bool IsOnStickySurface;

    public static PlatformerCharacterComponent GetDefault()
    {
        var result = new PlatformerCharacterComponent
        {
            GroundRunMaxSpeed = 5,
            GroundSprintMaxSpeed = 10,
            GroundedMovementSharpness = 8,
            GroundedRotationSharpness = 15,

            CrouchedMaxSpeed = 3,
            CrouchedMovementSharpness = 15,
            CrouchedRotationSharpness = 15,

            AirAcceleration = 15,
            AirMaxSpeed = 5,
            AirDrag = 0,
            AirRotationSharpness = 10,

            RollingAcceleration = 5,

            WallRunAcceleration = 50,
            WallRunMaxSpeed = 10,
            WallRunDrag = 0,
            WallRunGravityFactor = .375f,
            WallRunJumpRatioFromCharacterUp = .75f,
            WallRunDetectionDistance = .2f,

            FlyingMaxSpeed = 10,
            FlyingMovementSharpness = 15,

            GroundJumpSpeed = 5,
            AirJumpSpeed = 10,
            WallRunJumpSpeed = 12,
            JumpHeldAcceleration = 75,
            MaxHeldJumpTime = .1f,
            MaxUngroundedJumps = 1,
            JumpAfterUngroundedGraceTime = .1f,
            JumpBeforeGroundedGraceTime = .1f,

            LedgeMoveSpeed = 3,
            LedgeRotationSharpness = 10,
            LedgeSurfaceProbingHeight = .3f,
            LedgeSurfaceObstructionProbingHeight = .3f,
            LedgeSideProbingLength = .15f,

            DashDuration = .3f,
            DashSpeed = 20,

            SwimmingAcceleration = 30,
            SwimmingMaxSpeed = 4,
            SwimmingDrag = 4.7f,
            SwimmingRotationSharpness = 3,
            SwimmingStandUpDistanceFromSurface = -.5f,
            WaterDetectionDistance = 10,
            SwimmingJumpSpeed = 14,
            SwimmingSurfaceDiveThreshold = -.75f,

            RopeSwingAcceleration = 10,
            RopeSwingMaxSpeed = 10,
            RopeSwingDrag = .1f,
            RopeLength = 5,
            LocalRopeAnchorPoint = new float3(0, 1.1f, 0),

            ClimbingDistanceFromSurface = .375f,
            ClimbingSpeed = 5,
            ClimbingMovementSharpness = 20,
            ClimbingRotationSharpness = 5,

            StepAndSlopeHandling = new BasicStepAndSlopeHandlingParameters
            {
                MaxStepHeight = .5f,
                ExtraStepChecksDistance = .1f,
                PreventGroundingWhenMovingTowardsNoGrounding = true,
                MaxDownwardSlopeChangeAngle = 90,
                ConstrainVelocityToGroundPlane = true
            },

            // Set tags in editor
            UpOrientationAdaptationSharpness = 8,

            StandingGeometry = new CapsuleGeometryDefinition
            {
                Radius = .3f,
                Height = 1.4f,
                Center = new float3(0, .7f, 0)
            },
            CrouchingGeometry = new CapsuleGeometryDefinition
            {
                Radius = .3f,
                Height = .9f,
                Center = new float3(0, .45f, 0)
            },
            RollingGeometry = new CapsuleGeometryDefinition
            {
                Radius = .3f,
                Height = .6f,
                Center = new float3(0, .3f, 0)
            },
            ClimbingGeometry = new CapsuleGeometryDefinition
            {
                Radius = 1,
                Height = 2,
                Center = new float3(0, .7f, 0)
            },
            SwimmingGeometry = new CapsuleGeometryDefinition
            {
                Radius = .3f,
                Height = 1.4f,
                Center = new float3(0, .7f, 0)
            },
        };

        return result;
    }
}

[Serializable]
public struct PlatformerCharacterControl : IComponentData
{
    public float3 MoveVector;
    public float3 Target;
    
    public bool JumpHeld;
    public bool RollHeld;
    public bool SprintHeld;
    
    public bool JumpPressed;
    public bool DashPressed;
    public bool CrouchPressed;
    public bool RopePressed;
    public bool ClimbPressed;
    public bool FlyNoCollisionsPressed;

    public bool Fire1Pressed;
    public bool Fire1Released;
    public bool Fire2Pressed;
    public bool Fire2Released;

    public float3 GetLookFromPosition(float3 position)
    {
        return Target - position;
    }
}

public struct PlatformerCharacterInitialized : IComponentData
{ }

[Serializable]
public struct CapsuleGeometryDefinition
{
    public float Radius;
    public float Height;
    public float3 Center;

    public CapsuleGeometry ToCapsuleGeometry()
    {
        Height = math.max(Height, (Radius + math.EPSILON) * 2f);
        float halfHeight = Height * 0.5f;

        return new CapsuleGeometry
        {
            Radius = Radius,
            Vertex0 = Center + (-math.up() * (halfHeight - Radius)),
            Vertex1 = Center + (math.up() * (halfHeight - Radius)),
        };
    }
}