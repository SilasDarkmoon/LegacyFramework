using System;
using UnityEngine;
public static class Constants
{
    public static readonly float TOLERANCE = 1E-6f;
    public const float FPS = 30f;
    //animator state
    public const string DuplicateStateSuffix = "_d";
    public const string ACDuplicateParameter = "duplicate";

    //game hub
    public const int MatchAthleteCount = 22;
    public const int TeamAthleteCount = 11;

    //ball
    public const float BallRadius = 0.1f;

    //goal
    public const float GoalHeight = 2.44f;
    public const float GoalWidth = 3.66f;
    public const float GoalPostHalfDistance = 3.8f;
    public const float GoalOutsideWidth = 3.95f;
    public const float GoalPositionZ = 55f;

    //pitch
    public const float PenaltyAreaX = 20.16f;
    public const float PenaltyAreaZ = 38.5f;
    public const float GoalAreaX = 9.08f;
    public const float GoalAreaZ = 49.5f;
    public const float SideLinePosX = 37.3f;
    public const float PenaltySpotZ = 44f;
    public const float PitchXLength = 74f;
    public const float PitchZLength = 110f;
    public const float PitchXHalfLength = 37f;
    public const float PitchZHalfLength = 55f;
    public const float CornerKickBallPosX = 36.2f;
    public const float CornerKickBallPosZ = 54.5f;
    public const float ThrowInResetPosX = 36.7f;
    public static readonly Vector2 NorthCelebrateCorner = new Vector3(30f, 45f);
    public static readonly Vector2 SouthCelebrateCorner = new Vector3(-30f, -45f);
    public static readonly Vector3 ExitCorner = new Vector3(-40f, 0f, 0f);

    //id
    public const int BallId = 8000;
    public const int RefereeId = 22;
    public const int AssistantRefereeId = 23;
    public const int FourthOfficialId = 24;
    public const int SubstitutionDownPlayerId = 10;

    public const int InvalidId = -1;
    public const int LeftShoulderId = 100;
    public const int RightShoulderId = 101;
    public const int GateId = 200;

    //head ik
    public const float BallDeltaYAboveHead = 0.2f;
    public const float BallDeltaYBelowHead = 0.4f;

    //ball free fly -- start
    public const float SignedGravity = -9.8f;
    public const float GrassDrag = -4f;//草坪阻力
    public const float AirGrag = -.5f;//空气阻力，仅考虑水平方向

    public const float RollToIdleSpeedThreshold = 0.4f;
    public const float DropToRollSpeedThreshold = .98f;

    public const float HorizontalMinSpeedDrop = 0.5f;

    public const float GroundBounceHorizontalSpeedAttenuation = 0.6f;
    public const float GroundBounceVeritcalSpeedAttenuation = -0.6f;

    public const float AdBoardFrontPositionZ = 60f;
    public const float AdBoardFrontPositionY = 1.2f;
    public const float AdBoardBackPositionZ = 65f;
    public const float AdBoardBackPositionY = 2.8f;

    public const float CheckAdBoardDuration = 1f;

    public const float GoalGateInsidePositionX = 3.65f;
    public const float GoalGateOutsidePositionX = 3.75f;
    public const float GoalGateInsidePositionY = 2.27f;
    public const float GoalGateOutsidePositionY = 2.47f;
    public const float GoalGatePositionZ = 57.3f;
    public const float GoalLinePositionZ = 55f;

    public const float GoalBackBounceSpeedAttenuationX = .4f;
    public const float GoalBackBounceSpeedAttenuationZ = .2f;
    public const float GoalBackBounceSpeedAttenuationY = .4f;
    public const float GoalBackBounceSpeedAttenuationYUp = .8f;
    public const float GoalBackBounceHorizontalStopDistance = .1f;
    public const float GoalBackBounceHorizontalVerticalConverter = .2f;
    public const float GoalBackBounceHorizontalVerticalConverterUp = .6f;

    public const float GoalSideBounceSpeedAttenuationX = .2f;
    public const float GoalSideBounceSpeedAttenuationZ = .3f;
    public const float GoalSideBounceSpeedAttenuationY = .4f;
    public const float GoalSideBounceSpeedAttenuationYUp = .6f;
    public const float GoalSideBounceHorizontalVerticalConverter = .3f;

    public const float GoalUpBounceSpeedAttenuationX = .5f;
    public const float GoalUpBounceSpeedAttenuationZ = .5f;
    public const float GoalUpBounceSpeedAttenuationY = .4f;
    public const float GoalUpBounceHorizontalVerticalConverter = .2f;

    public const float AdBoardHorizontalSpeedAttenuation = 0.5f;
    public const float AdBoardMaxVerticalSpeed = 3f;
    public const float AdBoardMinVerticalSpeed = 1f;

    public const float SaveBounceTargetX = 6.5f;
    public const float SaveBounceSpeedMag = 25f;
    public const float SaveBounceSpeedMegPlayer = 10f;
    public const float SaveBounceYSpeedPlayer = 10f;
    //ball free fly -- end

    //ball actions -- start
    public const float InterDribbleRotateAngleMaxAngle = 25f;
    public const float InterDribbleRotateAngleMinAngle = 12f;
    public const float InterDribbleRotateAngleMaxDistance = 5f;
    public const float InterDribbleRotateAngleMinDistance = 1f;

    public const float InterDribbleAccelerationMaxAcc = .45f;
    public const float InterDribbleAccelerationMinAcc = .1f;
    public const float InterDribbleAccelerationMaxDistance = 6f;
    public const float InterDribbleAccelerationMinDistance = 1f;

    public const float Gravity = 9.8f;
    public const float ShootGravity = 9.8f;//暂定9.8
    public const float ProjectileRetardFactor = 2f;

    public const float BezierCurveMidFactor = .6f;
    public const float BezierCurveOffsetScaleFactor = .004f;
    public const float BezierCurveOffsetMaxScale = 4f;

    public const float PassBounceWindDragCoefficient = -.65f;
    public const float PassBounceVerticalSpeedAttenuationRate = .8f;//弹地时垂直方向速度衰减系数
    public const float PassBounceHorizontalSpeedAttenuationRate = 1f;//弹地时垂直方向速度衰减系数

    // Assume wind drag is F = - k * V
    // WindDragCoefficient = - k / M
    // e.g. k = 0.26, M = 0.4kg, WindDragCoefficient = -0.65
    public const float WindDragCoefficient = -.65f;
    public const float WindDragCoefficientPassLine = -.8f;

    public const float ShootDefaultRotateAngle = 32f;
    public const float ShootFastRotateAngle = 48f;
    public const float ShootSlowRotateAngle = 16f;
    public const float PassDefaultRotateAngle = 53f;
    public const float PassRotateAngleFactorLob = -.6f;
    public const float PassRotateAngleFactorUnloadBall = -.4f;
    public const float PassRotateAngleFactorAirStraight = -.6f;
    public const float PassRotateAngleFactorRainbow = -1f;
    public const float PassRotateAngleFactorBounceOnce = -.8f;
    public const float PassRotateAngleFactorDoubleHandsThrow = -.2f;
    public const float PassRotateAngleFactorHeader = -.8f;

    public const float ShootStraightMaxOffsetSqrDistance = .04f;
    //ball actions -- end

    //math - angle related
    public static readonly float Cos45 = (float)Math.Cos(Math.PI * .25f);

    //upper body layer action
    public const float InvokeDefenseTriggerRange = 2f;
    public const float ChaseDefenseTriggerRange = 4f;
    public const float MarkDefenseTriggerRange = 2f;

    //head ik -- start
    public const float StareCatcherTimeBeforePass = 0.8f;
    public const float PeekBallTimeBeforePass = 0.3f;
    public const float PeekBallTimeBeforeNPOPass = 0.5f;

    public const float PeekGateTimeBeforeShoot = 0.5f;
    public const float PeekBallTimeBeforeShoot = 0.2f;
    public const float PeekBallTimeBeforeNPOShoot = 0.7f;

    public const float PeekGateOrPlayerTimeBeforeCatch = 0.8f;
    public const float PeekBallTimeBeforeCatch = 0.4f;

    public const float PeekGateDefaultDuration = 0.3f;
    public const float PeekBallDefaultDuration = 0.3f;
    public const float PeekPlayerDefaultDuration = 0.4f;
    public const float PeekPlayerManualOperateDuration = 0.5f;
    public const float PeekPlayerCoolDownDuration = 1f;
    public const float PeekGateCoolDownDuration = 1f;

    public const float DisableIKDurationBeforeShoot = 0.2f;
    //head ik -- end

    //base layer blend tree
    public const float BaseLayerBlendTreeIncreaseSmoother = 10f;
    public const float BaseLayerBlendTreeDecreaseSmoother = 20f;
    public const int BaseLayerBlendTreeFocusBall = 101;
    public const int BaseLayerBlendTreeFocusPlayerGate = 102;
    public const int BaseLayerBlendTreeFocusOpponentGate = 103;

    //dead ball time
    public const int DeadBallTimeCharacterCount = 25;
    public static readonly Vector3 OutOfSightPosition = new Vector3(0f, -100f, 0f);
    public const float MinSupportRoleSqrDistance = 100f;
    public const float SupportRoleLimitSqrDistanceNear = 25f;
    public const float SupportRoleLimitSqrDistanceFar = 900f;

    //playback
    public const float DefaultPlaybackBufferLength = 5f;
}
