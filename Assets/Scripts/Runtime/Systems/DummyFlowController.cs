using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AlienCrusher.Gameplay;
using AlienCrusher.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController : MonoBehaviour
	{
		private enum RuntimeCityThemeProfile
		{
			Random,
			DenseCore,
			IndustrialHarbor,
			GardenResidential
		}

		private enum FormActiveSkill
		{
			None,
			SpikeBurst,
			RamBreach,
			SaucerDash,
			CrusherSlam,
			SpherePulse
		}

		private struct TrafficSpawnPoint
		{
			public Vector3 Position;

			public float Yaw;

			public Color BodyColor;
		}

		private sealed class TrafficVehicleState
		{
			public Transform Root;

			public Rigidbody Body;

			public float CruiseSpeed;

			public float CurrentSpeed;

			public bool AlongX;

			public int DirectionSign;

			public float LaneCoord;

			public float StopTimer;

			public float IntersectionCooldown;

			public float PanicSlowTimer;

			public float PanicSpeedScale = 1f;
		}

		private enum LevelUpChoice
		{
			Speed,
			Impact,
			Drill,
			ComboPower,
			ComboTrigger,
			Shockwave,
			TrafficPanic
		}

		private enum StageEndResult
		{
			None,
			Victory,
			Defeat
		}

		private enum StageEndReason
		{
			None,
			GoalReached,
			TotalDestruction,
			BossDefeated,
			TimerExpired,
			Manual
		}

		private enum StageVictoryFlow
		{
			Immediate,
			TotalDestructionDelay,
			StageFinale,
			BossCascade
		}

		private enum UiViewState
		{
			None,
			Lobby,
			Hud,
			LevelUp,
			Pause,
			Result
		}

		[Header("Runtime")]
		[SerializeField]
		private float levelUpDurationSeconds = 5f;

		[SerializeField]
		private int scorePerSecond = 120;

		[SerializeField]
		private int skillPickBonusScore = 500;

		[SerializeField]
		private bool useStageEntryImpact = true;

		[SerializeField]
		private float stageEntryDelaySeconds = 0.12f;

		[SerializeField]
		private float stageEntryRadius = 7f;

		[SerializeField]
		private int stageEntryMaxTargets = 14;

		[SerializeField]
		private Vector2 stageEntryDamageRange = new Vector2(25f, 90f);

		[SerializeField]
		private int levelUpDestroyedBase = 8;

		[SerializeField]
		private float levelUpRequirementGrowth = 1.35f;

		[SerializeField]
		private bool showActionButtons;

		[SerializeField]
		private float stageHintDurationSeconds = 10f;

		[SerializeField]
		private bool autoPickLevelUpOnTimeout = true;

		[SerializeField]
		private bool emitReadinessReportOnAwake = true;

		[Header("Total Destruction")]
		[SerializeField]
		private bool enableTotalDestructionEvent = true;

		[SerializeField]
		private bool totalDestructionInstantClear = true;

		[SerializeField]
		private float totalDestructionClearDelay = 1.15f;

		[SerializeField]
		private int totalDestructionFlatBonusScore = 850;

		[SerializeField]
		private int totalDestructionTimeBonusPerSecond = 42;

		[SerializeField]
		private int totalDestructionChainBonusStep = 26;

		[SerializeField]
		private float totalDestructionAnnouncementSeconds = 1.85f;

		[Header("Level Up Draft")]
		[SerializeField]
		private bool enableLevelUpReroll = true;

		[SerializeField]
		private int levelUpBaseRerolls = 1;

		[SerializeField]
		private int levelUpMaxRerolls = 3;

		[SerializeField]
		private int levelUpRerollChainThresholdA = 15;

		[SerializeField]
		private int levelUpRerollChainThresholdB = 30;

		[SerializeField]
		private int levelUpMobilityPityThreshold = 3;

		[Header("Traffic Panic Mastery Upgrade")]
		[SerializeField]
		private int trafficPanicMasteryBonusBaseAdd = 12;

		[SerializeField]
		private int trafficPanicMasteryBonusPerVehicleAdd = 5;

		[SerializeField]
		private float trafficPanicMasteryRadiusAdd = 0.45f;

		[SerializeField]
		private float trafficPanicMasteryChainWindowAdd = 0.24f;

		[SerializeField]
		private float trafficPanicMasteryCooldownReduction = 0.05f;

		[SerializeField]
		private int trafficPanicMasteryJackpotAdd = 38;

		[SerializeField]
		private float trafficPanicMasteryCounterSurgePerChainAdd = 0.012f;

		[SerializeField]
		private float trafficPanicMasteryCounterSurgeDurationAdd = 0.1f;

		[SerializeField]
		[Min(1f)]
		private int trafficPanicMasteryCounterSurgeThresholdStep = 2;

		[SerializeField]
		[Range(0f, 1f)]
		private float trafficPanicMasteryGainFalloff = 0.18f;

		[SerializeField]
		[Range(0.2f, 1f)]
		private float trafficPanicMasteryMinGainScale = 0.4f;

		[Header("Level Up Choice UI")]
		[SerializeField]
		private Color choicePowerColor = new Color(0.2f, 0.28f, 0.44f, 0.96f);

		[SerializeField]
		private Color choiceMobilityColor = new Color(0.19f, 0.33f, 0.42f, 0.96f);

		[SerializeField]
		private Color choiceFormColor = new Color(0.34f, 0.25f, 0.18f, 0.96f);

		[SerializeField]
		private Color choiceRushColor = new Color(0.4f, 0.2f, 0.17f, 0.96f);

		[SerializeField]
		private Color choiceTextColor = new Color(0.96f, 0.97f, 1f, 1f);

		[SerializeField]
		private Color choiceTagTextColor = new Color(1f, 0.95f, 0.74f, 1f);

		[Header("Overdrive")]
		[SerializeField]
		private int overdriveChainThreshold = 12;

		[SerializeField]
		private float overdriveDurationSeconds = 5f;

		[SerializeField]
		private float overdriveCooldownSeconds = 12f;

		[SerializeField]
		private float overdriveSpeedScale = 1.4f;

		[SerializeField]
		private float overdriveImpactScale = 1.55f;

		[SerializeField]
		private int overdriveBonusScorePerSecond = 260;

		[SerializeField]
		private int overdriveStartBonusScore = 400;

		[Header("Combo Rush")]
		[SerializeField]
		private int comboRushChainStep = 8;

		[SerializeField]
		private float comboRushRadius = 4.8f;

		[SerializeField]
		private float comboRushRadiusPerLevel = 0.4f;

		[SerializeField]
		private int comboRushDestructibleCap = 14;

		[SerializeField]
		private Vector2 comboRushDamageRange = new Vector2(42f, 130f);

		[SerializeField]
		[Range(0f, 1f)]
		private float comboRushStreetBreakChance = 0.68f;

		[SerializeField]
		private int comboRushScoreBonus = 260;

		[SerializeField]
		private float comboRushAnnouncementSeconds = 1.25f;

		[Header("Momentum Chain")]
		[SerializeField]
		private int momentumChainLostMinChain = 4;

		[SerializeField]
		private float momentumChainLostFeedbackCooldown = 0.85f;

		[SerializeField]
		private bool endOverdriveOnMomentumBreak = true;

		[Header("Alley Line Bonus")]
		[SerializeField]
		private bool enableAlleyLineBonus = true;

		[SerializeField]
		private float alleyLineWindowSeconds = 1.45f;

		[SerializeField]
		private float alleyLineLinkRadius = 4.2f;

		[SerializeField]
		private int alleyLineMinHitsForBonus = 2;

		[SerializeField]
		private int alleyLineBaseScore = 35;

		[SerializeField]
		private int alleyLinePerHitScore = 18;

		[SerializeField]
		private int alleyLineJackpotHits = 5;

		[SerializeField]
		private int alleyLineJackpotBonusScore = 140;

		[Header("Retail Frenzy")]
		[SerializeField]
		private bool enableRetailFrenzy = true;

		[SerializeField]
		private int retailFrenzyTriggerHits = 4;

		[SerializeField]
		private float retailFrenzyDuration = 6.5f;

		[SerializeField]
		private int retailFrenzyActivationScore = 120;

		[SerializeField]
		private int retailFrenzyShopHitScore = 55;

		[SerializeField]
		private int retailFrenzyChainBonusPerHit = 24;

		[SerializeField]
		private int retailFrenzyJackpotHits = 7;

		[SerializeField]
		private int retailFrenzyJackpotScore = 180;

		[Header("Strip Clear Bonus")]
		[SerializeField]
		private bool enableStripClearBonus = true;

		[SerializeField]
		private int stripClearRequiredHits = 5;

		[SerializeField]
		private float stripClearTravelDistance = 2.8f;

		[SerializeField]
		private int stripClearBonusScore = 220;

		[Header("Strip Clear Mission")]
		[SerializeField]
		private bool enableStripClearMission = true;

		[SerializeField]
		private int stripClearMissionBaseTarget = 1;

		[SerializeField]
		private int stripClearMissionHighStageTarget = 2;

		[SerializeField]
		private int stripClearMissionCompleteScore = 260;

		[SerializeField]
		private int stripClearMissionCompleteDp = 6;

		[Header("Stage Goal")]
		[SerializeField]
		private bool enableStageAdvanceGoal = true;

		[SerializeField]
		private int stageAdvanceBaseTarget = 16;

		[SerializeField]
		private int stageAdvanceTargetPerStage = 3;

		[SerializeField]
		[Range(0.2f, 0.95f)]
		private float stageAdvanceTargetRatio = 0.48f;

		[SerializeField]
		private float stageAdvanceGoalAnnouncementSeconds = 1.2f;

		[SerializeField]
		private int stageAdvanceMilestoneScore = 120;

		[SerializeField]
		private int earlyCrushFlowBonusScore = 70;

		[SerializeField]
		private float earlyCrushFlowWindowSeconds = 18f;

		[SerializeField]
		private int earlyCrushFirstBreakTarget = 2;

		[SerializeField]
		private int earlyCrushStartTarget = 5;

		[SerializeField]
		private int earlyCrushLaneBreakTarget = 9;

		[SerializeField]
		private float earlyCrushRecoveryWindowSeconds = 16f;

		[SerializeField]
		private float earlyCrushRecoveryBonusScoreScale = 0.7f;

		[SerializeField]
		private float routeHoldWindowSeconds = 38f;

		[SerializeField]
		[Range(0.25f, 0.75f)]
		private float routeHoldProgressThreshold = 0.45f;

		[SerializeField]
		private int routeHoldBonusScore = 135;

		[SerializeField]
		private float routeHoldCounterDuration = 1.05f;

		[SerializeField]
		private float stageAdvanceNearCompleteThreshold = 0.82f;

		[SerializeField]
		private float stageAdvanceGaugePulseSpeed = 6.8f;

		[SerializeField]
		private Color stageAdvanceRouteMarkerColor = new Color(1f, 0.78f, 0.24f, 1f);

		[SerializeField]
		[Range(3, 8)]
		private int routeHoldTrailPipCount = 5;

		[SerializeField]
		private float routeHoldTrailMaxDistance = 18f;

		[SerializeField]
		private Color routeHoldTrailColor = new Color(1f, 0.88f, 0.28f, 0.88f);

		[SerializeField]
		private float stageAdvanceRouteRewardDistance = 3.4f;

		[SerializeField]
		private int stageAdvanceRouteRewardScore = 180;

		[SerializeField]
		private float stageAdvanceRouteRewardCounterDuration = 1.8f;

		[SerializeField]
		private float stageAdvanceRouteRewardSpeedScale = 1.14f;

		[SerializeField]
		private float stageAdvanceRouteRewardImpactScale = 1.22f;

		[SerializeField]
		private float stageAdvanceRouteRewardCameraImpulse = 0.7f;

		[SerializeField]
		private int forwardSmashBonusScore = 260;

		[SerializeField]
		private float forwardSmashBonusRadius = 6.2f;

		[SerializeField]
		private Vector2 forwardSmashBonusDamageRange = new Vector2(42f, 128f);

		[SerializeField]
		private bool enableStageAdvanceFinale = true;

		[SerializeField]
		private float stageAdvanceFinaleDelay = 0.18f;

		[SerializeField]
		private float stageAdvanceFinaleDuration = 0.7f;

		[SerializeField]
		private float stageAdvanceFinaleRadius = 7.6f;

		[SerializeField]
		private Vector2 stageAdvanceFinaleDamageRange = new Vector2(28f, 92f);

		[Header("Form Active Skills")]
		[SerializeField]
		private bool enableFormActiveSkills = true;

		[SerializeField]
		private float spherePulseRadius = 3.8f;

		[SerializeField]
		private Vector2 spherePulseDamageRange = new Vector2(30f, 88f);

		[SerializeField]
		private int spherePulseDestructibleCap = 12;

		[SerializeField]
		[Range(0f, 1f)]
		private float spherePulseStreetBreakChance = 0.7f;

		[SerializeField]
		private float spherePulseCooldown = 3.9f;

		[SerializeField]
		private float spikeBurstRadius = 4.1f;

		[SerializeField]
		private Vector2 spikeBurstDamageRange = new Vector2(34f, 102f);

		[SerializeField]
		private float spikeBurstCooldown = 4.6f;

		[SerializeField]
		private float ramBreachVelocityBoost = 11.5f;

		[SerializeField]
		private float ramBreachRadius = 4.9f;

		[SerializeField]
		private Vector2 ramBreachDamageRange = new Vector2(56f, 162f);

		[SerializeField]
		private float ramBreachCooldown = 5.4f;

		[SerializeField]
		private float saucerDashVelocityBoost = 13f;

		[SerializeField]
		private float saucerDashRadius = 3.6f;

		[SerializeField]
		private Vector2 saucerDashDamageRange = new Vector2(28f, 82f);

		[SerializeField]
		private float saucerDashCooldown = 4.2f;

		[SerializeField]
		private float crusherSlamRadius = 6.4f;

		[SerializeField]
		private int crusherSlamDestructibleCap = 24;

		[SerializeField]
		private Vector2 crusherSlamDamageRange = new Vector2(72f, 215f);

		[SerializeField]
		[Range(0f, 1f)]
		private float crusherSlamStreetBreakChance = 0.92f;

		[SerializeField]
		private float crusherSlamCooldown = 7.8f;

		[SerializeField]
		private int formSkillBaseScoreBonus = 140;

		[Header("Form Skill Synergy")]
		[SerializeField]
		private bool enableFormSkillSynergy = true;

		[SerializeField]
		[Range(0.45f, 1f)]
		private float formSkillSynergyCooldownFloor = 0.55f;

		[SerializeField]
		private int formSkillSynergyBonusScoreBase = 34;

		[Header("Landing Shockwave")]
		[SerializeField]
		private bool enableLandingShockwave = true;

		[SerializeField]
		private float landingShockwaveRadiusMin = 2.7f;

		[SerializeField]
		private float landingShockwaveRadiusMax = 7.2f;

		[SerializeField]
		private Vector2 landingShockwaveDamageRangeMin = new Vector2(26f, 66f);

		[SerializeField]
		private Vector2 landingShockwaveDamageRangeMax = new Vector2(86f, 228f);

		[SerializeField]
		private int landingShockwaveTargetCapMin = 4;

		[SerializeField]
		private int landingShockwaveTargetCapMax = 22;

		[SerializeField]
		[Range(0f, 1f)]
		private float landingShockwaveStreetBreakChance = 0.76f;

		[SerializeField]
		private int landingShockwaveBonusScoreBase = 42;

		[Header("Seismic Upgrade")]
		[SerializeField]
		private float seismicLandingRadiusPerLevel = 0.34f;

		[SerializeField]
		private Vector2 seismicLandingDamagePerLevel = new Vector2(10f, 24f);

		[SerializeField]
		private int seismicLandingTargetCapPerLevel = 2;

		[SerializeField]
		[Range(0f, 1f)]
		private float seismicLandingStreetChancePerLevel = 0.03f;

		[SerializeField]
		private int seismicLandingBonusScorePerLevel = 16;

		[SerializeField]
		private float seismicBuildingShockwaveRadiusBonusPerLevel = 0.12f;

		[SerializeField]
		private float seismicBuildingShockwaveDamageBonusPerLevel = 0.16f;

		[SerializeField]
		[Range(0f, 0.2f)]
		private float seismicBuildingShockwavePropChanceBonusPerLevel = 0.025f;

		[SerializeField]
		private int seismicBuildingShockwaveDestructibleCapBonusPerLevel = 1;

		[SerializeField]
		private int seismicBuildingShockwavePropCapBonusPerLevel = 1;

		[SerializeField]
		private float seismicBuildingShockwaveImpulseBonusPerLevel = 0.08f;

		[SerializeField]
		private int seismicBuildingShockwaveFlatScoreBonusPerLevel = 3;

		[Header("Seismic Pulse")]
		[SerializeField]
		private float seismicUpgradePulseRadiusBase = 3.4f;

		[SerializeField]
		private float seismicUpgradePulseRadiusPerLevel = 0.42f;

		[SerializeField]
		private Vector2 seismicUpgradePulseDamageBase = new Vector2(20f, 62f);

		[SerializeField]
		private Vector2 seismicUpgradePulseDamagePerLevel = new Vector2(6f, 18f);

		[SerializeField]
		private int seismicUpgradePulseTargetCapBase = 5;

		[SerializeField]
		private int seismicUpgradePulseTargetCapPerLevel = 2;

		[SerializeField]
		[Range(0f, 1f)]
		private float seismicUpgradePulseStreetChanceBase = 0.62f;

		[SerializeField]
		[Range(0f, 1f)]
		private float seismicUpgradePulseStreetChancePerLevel = 0.04f;

		[SerializeField]
		private int seismicUpgradePulseBonusScoreBase = 28;

		[SerializeField]
		private int seismicUpgradePulseBonusScorePerLevel = 12;

		[SerializeField]
		private float seismicUpgradePulseAnnouncementSeconds = 1.05f;

		[Header("Seismic Resonance")]
		[SerializeField]
		private int seismicResonanceInterval = 3;

		[SerializeField]
		private float seismicResonanceRadiusMultiplier = 1.45f;

		[SerializeField]
		private float seismicResonanceDamageMultiplier = 1.5f;

		[SerializeField]
		private int seismicResonanceTargetBonus = 5;

		[SerializeField]
		[Range(0f, 1f)]
		private float seismicResonanceStreetChanceBonus = 0.16f;

		[SerializeField]
		private int seismicResonanceScoreBonusBase = 160;

		[SerializeField]
		private float seismicResonanceAnnouncementSeconds = 1.35f;

		[Header("Action Skill UI Feedback")]
		[SerializeField]
		private bool enableActionSkillButtonFeedback = true;

		[SerializeField]
		[Range(0f, 0.25f)]
		private float actionSkillReadyIdleScaleAmplitude = 0.045f;

		[SerializeField]
		private float actionSkillReadyIdlePulseSpeed = 6.2f;

		[SerializeField]
		private Color actionSkillReadyTint = new Color(1f, 0.88f, 0.46f, 1f);

		[SerializeField]
		private float actionSkillReadyTintStrength = 0.32f;

		[SerializeField]
		private float actionSkillReadyPulseDuration = 0.22f;

		[SerializeField]
		[Range(0f, 0.45f)]
		private float actionSkillReadyPulseScaleKick = 0.2f;

		[SerializeField]
		private float actionSkillTapConfirmDuration = 0.17f;

		[SerializeField]
		[Range(0.7f, 1f)]
		private float actionSkillTapConfirmMinScale = 0.88f;

		[SerializeField]
		private Color actionSkillTapConfirmTint = new Color(1f, 0.62f, 0.28f, 1f);

		[SerializeField]
		[Range(0f, 1f)]
		private float actionSkillTapConfirmTintStrength = 0.72f;

		[Header("Traffic Simulation")]
		[SerializeField]
		private bool enableTrafficSimulation = true;

		[SerializeField]
		private int trafficDesiredCars = 24;

		[SerializeField]
		private Vector2 trafficSpeedRange = new Vector2(2.9f, 5.8f);

		[SerializeField]
		private float trafficRespawnInterval = 1f;

		[SerializeField]
		private float trafficLaneJitter = 0.16f;

		[SerializeField]
		private float trafficFollowingDistance = 3.4f;

		[SerializeField]
		private float trafficBrakeDistance = 1.75f;

		[SerializeField]
		private float trafficIntersectionRadius = 2.2f;

		[SerializeField]
		[Range(0f, 1f)]
		private float trafficIntersectionStopChance = 0.45f;

		[SerializeField]
		private Vector2 trafficIntersectionStopDuration = new Vector2(0.35f, 1.05f);

		[SerializeField]
		private bool enableTrafficSignals = true;

		[SerializeField]
		private float trafficSignalPhaseSeconds = 4.5f;

		[SerializeField]
		private float trafficSignalAllRedSeconds = 0.35f;

		[SerializeField]
		[Range(0f, 1f)]
		private float trafficYieldChanceOnGreen = 0.08f;

		[Header("Traffic Panic")]
		[SerializeField]
		private bool enableTrafficPanic = true;

		[SerializeField]
		private float trafficPanicRadius = 10.5f;

		[SerializeField]
		private Vector2 trafficPanicStopDuration = new Vector2(0.65f, 1.65f);

		[SerializeField]
		[Range(0.25f, 1f)]
		private float trafficPanicSpeedScale = 0.62f;

		[SerializeField]
		[Range(0f, 1f)]
		private float trafficPanicSwerveChance = 0.38f;

		[SerializeField]
		private float trafficPanicSwerveOffset = 0.78f;

		[SerializeField]
		private int trafficPanicBonusBaseScore = 34;

		[SerializeField]
		private int trafficPanicBonusPerVehicle = 12;

		[SerializeField]
		[Min(1f)]
		private int trafficPanicBonusMinAffected = 3;

		[SerializeField]
		private float trafficPanicBonusCooldown = 0.85f;

		[SerializeField]
		private bool showTrafficPanicBonusTag = true;

		[SerializeField]
		private float trafficPanicChainWindow = 4.2f;

		[SerializeField]
		[Range(0f, 0.5f)]
		private float trafficPanicChainBonusPerStack = 0.16f;

		[SerializeField]
		[Min(1f)]
		private int trafficPanicChainMaxStacks = 6;

		[SerializeField]
		[Min(1f)]
		private int trafficPanicChainTagMinStack = 2;

		[SerializeField]
		private int trafficPanicJackpotScore = 220;

		[SerializeField]
		private bool showTrafficPanicJackpotTag = true;

		[SerializeField]
		private bool enableTrafficPanicCounterSurgeReward = true;

		[SerializeField]
		[Min(1f)]
		private int trafficPanicCounterSurgeMinChain = 2;

		[SerializeField]
		[Range(1f, 2.2f)]
		private float trafficPanicCounterSurgeSpeedScale = 1.12f;

		[SerializeField]
		[Range(1f, 2.8f)]
		private float trafficPanicCounterSurgeImpactScale = 1.22f;

		[SerializeField]
		[Range(0f, 0.28f)]
		private float trafficPanicCounterSurgePerChain = 0.06f;

		[SerializeField]
		private float trafficPanicCounterSurgeDuration = 1.35f;

		[SerializeField]
		private bool showTrafficPanicCounterSurgeTag = true;

		[Header("Traffic Collision")]
		[SerializeField]
		private Vector3 trafficObstacleProbeHalfExtents = new Vector3(0.58f, 0.24f, 1.08f);

		[SerializeField]
		private float trafficObstacleLookAhead = 0.28f;

		[SerializeField]
		private float trafficObstacleBuffer = 0.08f;

		[SerializeField]
		private float trafficObstacleStopDuration = 0.3f;

		[SerializeField]
		private float trafficObstacleStopDurationJitter = 0.2f;

		[Header("Stage Encounter")]
		[SerializeField]
		private bool enableStageBossEncounter = true;

		[SerializeField]
		[Min(2f)]
		private int bossStageStart = 4;

		[SerializeField]
		private int eliteWeakPointBaseCount = 2;

		[SerializeField]
		private int eliteWeakPointPerStage = 1;

		[SerializeField]
		private int eliteWeakPointMaxCount = 8;

		[SerializeField]
		private int bossHpBonusBase = 12;

		[SerializeField]
		private int bossHpBonusPerStage = 4;

		[SerializeField]
		private Color eliteWeakPointColor = new Color(0.95f, 0.78f, 0.42f, 1f);

		[SerializeField]
		private Color bossSentinelColor = new Color(0.35f, 0.6f, 0.9f, 1f);

		[SerializeField]
		private bool enableBossTelemetry = true;

		[SerializeField]
		private float bossThreatPulseBaseInterval = 10f;

		[SerializeField]
		private float bossThreatPulseFastInterval = 6.2f;

		[SerializeField]
		[Range(0.1f, 0.95f)]
		private float bossThreatLockHealthThreshold = 0.72f;

		[SerializeField]
		[Range(0.05f, 0.8f)]
		private float bossThreatFuryHealthThreshold = 0.38f;

		[SerializeField]
		private bool enableBossPhaseTwo = true;

		[SerializeField]
		[Range(0.05f, 0.7f)]
		private float bossPhaseTwoHealthThreshold = 0.32f;

		[SerializeField]
		[Range(1f, 2.5f)]
		private float bossPhaseTwoPressureMultiplier = 1.35f;

		[SerializeField]
		[Range(1f, 2.5f)]
		private float bossPhaseTwoRegenMultiplier = 1.4f;

		[SerializeField]
		[Range(0.4f, 1f)]
		private float bossPhaseTwoBreakDurationScale = 0.72f;

		[SerializeField]
		private bool enableBossPhaseTwoDrones = true;

		[SerializeField]
		private int bossPhaseTwoDroneCount = 2;

		[SerializeField]
		private float bossPhaseTwoDroneOrbitRadius = 5.2f;

		[SerializeField]
		private float bossPhaseTwoDroneOrbitHeight = 2.8f;

		[SerializeField]
		private float bossPhaseTwoDroneOrbitSpeed = 52f;

		[SerializeField]
		private float bossPhaseTwoDronePulseInterval = 2.6f;

		[SerializeField]
		private float bossPhaseTwoDronePulseRadius = 6.4f;

		[SerializeField]
		[Range(0.2f, 1f)]
		private float bossPhaseTwoDroneSlowScale = 0.7f;

		[SerializeField]
		private float bossPhaseTwoDroneSlowDuration = 0.9f;

		[SerializeField]
		private Color bossPhaseTwoDroneColor = new Color(1f, 0.32f, 0.16f, 1f);

		[SerializeField]
		private float bossPhaseTwoDroneRecoveryDuration = 6.5f;

		[SerializeField]
		private float bossPhaseTwoDroneRespawnWarningDuration = 1.4f;

		[SerializeField]
		private float bossPhaseTwoDronePulseDelayOnDestroy = 1.6f;

		[SerializeField]
		private int bossPhaseTwoDroneScoreBonus = 110;

		[SerializeField]
		private float bossPhaseTwoDroneCounterSurgeDuration = 1.75f;

		[SerializeField]
		private float bossPhaseTwoDroneCounterSurgeSpeedScale = 1.08f;

		[SerializeField]
		private float bossPhaseTwoDroneCounterSurgeImpactScale = 1.16f;

		[SerializeField]
		private bool enableBossFinishSlowMotion = true;

		[SerializeField]
		[Range(0.45f, 1f)]
		private float bossFinishSlowTimeScale = 0.72f;

		[SerializeField]
		private float bossFinishSlowDuration = 0.16f;

		[SerializeField]
		private bool enableBossClearCascade = true;

		[SerializeField]
		private float bossClearCascadeDelay = 0.24f;

		[SerializeField]
		private float bossClearCascadeDuration = 1.15f;

		[SerializeField]
		private float bossClearCascadeRadius = 28f;

		[SerializeField]
		private int bossClearCascadeMaxTargets = 20;

		[SerializeField]
		private Vector2 bossClearCascadeDamageRange = new Vector2(88f, 240f);

		[SerializeField]
		private float bossClearCascadeImpact = 0.92f;

		[SerializeField]
		private float bossClearCascadeFeedbackRadius = 6.2f;

		[SerializeField]
		private bool enableBossPressurePulse = true;

		[SerializeField]
		private bool enableBossShieldPylons = true;

		[SerializeField]
		private int bossShieldPylonCount = 2;

		[SerializeField]
		private int bossShieldPylonBonusHp = 3;

		[SerializeField]
		private float bossShieldSearchRadius = 13.5f;

		[SerializeField]
		[Range(0.05f, 1f)]
		private float bossShieldProtectedDamageScale = 0.24f;

		[SerializeField]
		private Color bossShieldPylonColor = new Color(1f, 0.5f, 0.18f, 1f);

		[SerializeField]
		private bool enableBossShieldRegen = true;

		[SerializeField]
		private float bossShieldRegenInterval = 3.8f;

		[SerializeField]
		[Range(0.01f, 0.2f)]
		private float bossShieldRegenPercentPerPylon = 0.05f;

		[SerializeField]
		private bool enableBossBreakWindow = true;

		[SerializeField]
		private float bossBreakWindowDuration = 6.5f;

		[SerializeField]
		[Range(1f, 4f)]
		private float bossBreakDamageMultiplier = 1.85f;

		[SerializeField]
		private float bossPressurePulseInterval = 8.4f;

		[SerializeField]
		private float bossPressurePulseRadius = 7.5f;

		[SerializeField]
		private float bossPressurePulseRadiusPerThreat = 1.15f;

		[SerializeField]
		[Range(0.2f, 1f)]
		private float bossPressureSlowScale = 0.72f;

		[SerializeField]
		private float bossPressureSlowDuration = 1.1f;

		[SerializeField]
		private float bossPressurePushForce = 7.8f;

		[SerializeField]
		[Range(0f, 1f)]
		private float bossPressurePropBreakChance = 0.28f;

		[Header("Map Generation")]
		[SerializeField]
		private RuntimeCityThemeProfile fallbackCityTheme;

		[SerializeField]
		private bool rebuildRuntimeMapOnStageStart = true;

		[SerializeField]
		[Min(1)]
		private int runtimeMapMaxGrowthStage = 7;

		[SerializeField]
		private bool enableMapLayoutDebugHotkeys = true;

		[SerializeField]
		[Min(1)]
		private int mapLayoutDebugMaxStage = 7;

		[SerializeField]
		private bool showMapLayoutDebugOverlay = true;

		[SerializeField]
		[Min(0.05f)]
		private float mapLayoutDebugSweepStepSeconds = 0.35f;

		private string runtimeMapLayoutDebugSummary = string.Empty;
		private string runtimeMapLayoutDebugWarning = string.Empty;
		private float runtimeMapLayoutDebugUpdatedAt;
		private GUIStyle runtimeMapLayoutOverlayStyle;
		private GUIStyle runtimeMapLayoutWarningStyle;
		private Coroutine mapLayoutDebugSweepRoutine;
		private int mapLayoutDebugSweepRestoreStage = 1;

		[Header("Scene References")]
		[SerializeField]
		private string playerBallName = "PlayerBall";

		[SerializeField]
		private string playerSpawnName = "PlayerSpawn";

		[Header("UI Names")]
		[SerializeField]
		private string canvasName = "Canvas_Dummy";

		[SerializeField]
		private string hudPanelName = "HUD_Dummy";

		[SerializeField]
		private string resultPanelName = "ResultPopup_Dummy";

		[SerializeField]
		private string levelUpPanelName = "LevelUpPopup_Dummy";

		[SerializeField]
		private string lobbyPanelName = "LobbyPanel_Dummy";

		[SerializeField]
		private string pausePanelName = "PausePanel_Dummy";

		private GameFlowSystem gameFlowSystem;

		private ScoreSystem scoreSystem;

		private BallGrowthSystem ballGrowthSystem;

		private CameraFollowSystem cameraFollowSystem;

		private FeedbackSystem feedbackSystem;

		private DamageNumberSystem damageNumberSystem;

		private FormUnlockSystem formUnlockSystem;

		private DummyPanelTag hudPanel;

		private DummyPanelTag resultPanel;

		private DummyPanelTag levelUpPanel;

		private DummyPanelTag lobbyPanel;

		private DummyPanelTag pausePanel;

		private UiViewState currentUiViewState;

		private Text hudInfoText;

		private Text hudObjectiveText;

		private Text hudHintText;

		private Text hudProgressText;

		private Text hudStageGoalText;

		private Text hudRouteIndicatorText;

		private Text hudBossStatusText;

		private RectTransform hudRouteArrowRect;

		private Text hudRouteArrowText;

		private Text levelUpTimerText;

		private Text resultSummaryText;

		private Text resultBreakdownText;

		private Text resultContinueHintText;

		private Text resultHighlightText;

		private Text resultMetaProgressText;

		private Text resultStatusBadgeText;

		private Text resultAdviceText;

		private Text hudChainText;

		private Text hudUpgradeListText;

		private Text lobbyDpText;

		private Text lobbyMissionText;

		private Text lobbyRecommendationText;

		private Text lobbyMetaDetailText;

		private Image hudStageGoalFillImage;

		private Image hudStageGoalBackgroundImage;

		private Transform canvasRootTransform;

		private Transform playerTransform;

		private Rigidbody playerBody;

		private PlayerBallDummyController cachedPlayerController;

		private PlayerBallDummyController landingShockwaveEventSource;

		private Transform playerSpawn;

		private float remainingStageTime;

		private float levelUpRemaining;

		private float scoreTick;

		private float overdriveRemaining;

		private float overdriveCooldownRemaining;

		private float overdriveScoreCarry;

		private bool overdriveActive;

		private int nextOverdriveChainGate;

		private Coroutine overdrivePulseRoutine;

		private int nextComboRushChainGate;

		private float alleyLineLastBreakTime = -10f;

		private int alleyLineChainCount;

		private Vector3 alleyLineLastBreakPosition = Vector3.zero;

		private bool alleyLineHasAnchor;

		private float retailFrenzyRemaining;

		private int retailFrenzyActivationCount;

		private Vector3 alleyLineChainStartPosition = Vector3.zero;

		private bool alleyLineHasStartPosition;

		private bool stripClearAwardedThisChain;

		private int stageStripClearCount;

		private int stageStripClearTarget;

		private bool stripClearMissionCompleted;

		private int stripClearMissionScoreAwarded;

		private int stripClearMissionDpAwarded;

		private float comboRushAnnouncementRemaining;

		private string comboRushAnnouncementText = string.Empty;

		private int comboRushAnnouncementPriority = -1;

		private float comboRushAnnouncementCooldown = 0f;

		private float pendingComboRushAnnouncementRemaining;

		private string pendingComboRushAnnouncementText = string.Empty;

		private int pendingComboRushAnnouncementPriority = -1;

		private float momentumChainLostFeedbackCooldownRemaining;

		private bool stageRunning;

		private bool stagePaused;

		private bool levelUpOpen;

		private float hudUrgencyPulseCooldownRemaining;

		private string lastHudUrgencyHint = string.Empty;

		private float hudHintHoldRemaining;

		private string lastStableHudHint = string.Empty;

		private int hudHintPriority = -1;

		private FormUnlockSystem.MetaUpgradeType lastRecommendedMetaUpgrade;

		private string lastRecommendedUpgradeReason = string.Empty;

		private FormType lastRecommendedFormUnlock = FormType.Sphere;

		private string lastLobbyActionStatus = string.Empty;

		[SerializeField]
		private float lobbyRecommendationFocusDuration = 2.4f;

		private float lobbyRecommendationFocusRemaining;

		private Color lobbyRecommendationBaseColor = Color.white;

		private Color lobbyMetaDetailBaseColor = Color.white;

		private float formSkillCooldownRemaining;

		private int levelUpRerollsRemaining;

		private bool levelUpChainRerollARewardGranted;

		private bool levelUpChainRerollBRewardGranted;

		private bool levelUpHasLockedChoice;

		private int levelUpLockedSlot = -1;

		private LevelUpChoice levelUpLockedChoice;

		private readonly int[] levelUpChoiceMissStreak = new int[7];

		private readonly LevelUpChoice[] previousLevelUpChoices = new LevelUpChoice[3];

		private bool hasPreviousLevelUpChoices;

		private string cachedTransformButtonLabel = string.Empty;

		private string cachedSpecial1ButtonLabel = string.Empty;

		private string cachedSpecial2ButtonLabel = string.Empty;

		private Button transformActionButton;

		private Button supportActionButton;

		private Image transformActionButtonImage;

		private Image supportActionButtonImage;

		private Vector3 transformActionButtonBaseScale = Vector3.one;

		private Vector3 supportActionButtonBaseScale = Vector3.one;

		private Color transformActionButtonBaseColor = Color.white;

		private Color supportActionButtonBaseColor = Color.white;

		private bool actionSkillReadyVisualActive;

		private bool previousActionSkillReadyVisualState;

		private float actionSkillReadyPulseRemaining;

		private float actionSkillTapConfirmRemaining;

		private bool transformActionButtonVisualCached;

		private bool supportActionButtonVisualCached;

		private int lastChainValue = 1;

		private float chainPulse;

		private float stageStartHintRemaining;

		private int currentStageNumber = 1;

		private int currentLevel = 1;

		private int queuedLevelUps;

		private int nextLevelDestroyedRequirement;

		private int speedUpgradeCount;

		private int lastEarnedDp;

		private bool stageRewardGranted;

		private bool stageEndLocked;

		private StageEndResult stageEndResult;

		private StageEndReason stageEndReason;

		private bool totalDestructionAchieved;

		private int totalDestructionBonusAwarded;

		private int stageTotalDestructibleCount;

		private int stageAdvanceDestroyTarget;

		private bool stageAdvanceGoalReached;

		private int stageAdvanceMilestoneIndex;

		private int earlyCrushFlowBonusIndex;

		private bool earlyCrushRecoveryBonusGranted;

		private bool routeHoldBonusGranted;

		private Transform stageAdvanceRouteMarkerA;

		private Transform stageAdvanceRouteMarkerB;

		private Transform activeStageAdvanceRouteMarker;

		private Transform routeHoldTrailRoot;

		private Transform[] routeHoldTrailPips;

		private bool stageAdvanceRouteGuidanceActive;

		private bool stageAdvanceRouteRewardGranted;

		private DummyDestructibleBlock forwardSmashTargetBlock;

		private bool forwardSmashBonusPending;

		private Coroutine stageAdvanceFinaleRoutine;

		private bool stageAdvanceFinaleActive;

		private DummyDestructibleBlock stageBossBlock;

		private bool stageBossEncounterActive;

		private int stageEliteWeakPointCount;

		private bool stageBossIntroAnnounced;

		private bool stageBossDestroyedAnnounced;

		private int stageBossThreatLevel;

		private bool stageBossPhaseTwoActive;

		private bool stageBossPhaseTwoAnnounced;

		private float stageBossThreatPulseRemaining;

		private readonly List<Transform> stageBossPhaseTwoDrones = new List<Transform>(4);

		private readonly List<BossPhaseTwoDroneDummy> stageBossPhaseTwoDroneUnits = new List<BossPhaseTwoDroneDummy>(4);

		private Transform stageBossPhaseTwoDroneRoot;

		private float stageBossPhaseTwoDronePulseRemaining;

		private float stageBossPhaseTwoDroneRecoveryRemaining;

		private bool stageBossPhaseTwoDroneRespawnWarned;

		private float stageBossPressurePulseRemaining;

		private readonly List<DummyDestructibleBlock> stageBossShieldPylons = new List<DummyDestructibleBlock>(4);

		private int stageBossShieldActiveCount;

		private bool stageBossShieldIntroAnnounced;

		private float stageBossShieldRegenRemaining;

		private float stageBossBreakRemaining;

		private Coroutine totalDestructionCompleteRoutine;

		private Coroutine bossFinishSlowMotionRoutine;

		private Coroutine bossClearCascadeRoutine;

		private bool bossClearCascadeActive;

		private bool pendingBossStageCompletion;

		private int impactUpgradeCount;

		private int drillUpgradeCount;

		private int comboPowerUpgradeCount;

		private int comboTriggerUpgradeCount;

		private int seismicUpgradeCount;

		private int trafficPanicUpgradeCount;

		private bool pendingTrafficPanicMasteryPulse;

		private int pendingTrafficPanicMasteryLevel;

		private float pendingTrafficPanicMasteryGainScale = 1f;

		private int metaUpgradePreviewIndex;

		private readonly List<DummyDestructibleBlock> destructibleCache = new List<DummyDestructibleBlock>(256);

		private readonly LevelUpChoice[] currentChoices = new LevelUpChoice[3];

		private System.Random levelUpRng = new System.Random();

		private readonly List<TrafficVehicleState> trafficVehicles = new List<TrafficVehicleState>(64);

		private readonly List<TrafficSpawnPoint> trafficSpawnPoints = new List<TrafficSpawnPoint>(96);

		private readonly List<Vector2> trafficIntersections = new List<Vector2>(48);

		private readonly RaycastHit[] trafficObstacleHits = (RaycastHit[])(object)new RaycastHit[18];

		private readonly Collider[] trafficSpawnObstacleHits = (Collider[])(object)new Collider[18];

		private readonly System.Random trafficRng = new System.Random();

		private Transform trafficStreetPropsRoot;

		private Vector3 trafficMapCenter = Vector3.zero;

		private Vector2 trafficMapHalfExtents = new Vector2(25f, 25f);

		private float trafficRespawnTick;

		private int trafficSpawnSerial;

		private bool trafficAlongXGreen = true;

		private bool trafficSignalAllRed;

		private float trafficSignalPhaseTimer;

		private float trafficPanicBonusCooldownRemaining;

		private float trafficPanicChainRemaining;

		private int trafficPanicChainStack;

		private bool trafficPanicJackpotTriggered;

		private static Material runtimeStylizedMaterial;

	}
}


