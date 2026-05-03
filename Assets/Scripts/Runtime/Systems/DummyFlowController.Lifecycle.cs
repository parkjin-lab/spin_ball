using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void Awake()
		{
			EnsureRuntimeMapFallback();
			RepairZeroScaleMapObjects();
			EnsureUiInputModuleMode();
			EnsureGameplayLighting();
			ApplyStylizedLookToScene();
			ApplyMapContrastPalette();
			gameFlowSystem = Object.FindFirstObjectByType<GameFlowSystem>();
			scoreSystem = Object.FindFirstObjectByType<ScoreSystem>();
			ballGrowthSystem = Object.FindFirstObjectByType<BallGrowthSystem>();
			cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
			formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			EnsureGameFlowSystem();
			EnsureScoreSystem();
			EnsureBallGrowthSystem();
			EnsureCameraFollowSystem();
			EnsureFeedbackSystem();
			EnsureDamageNumberSystem();
			EnsureFormUnlockSystem();
			LoadSavedStageProgress();
			CacheSceneReferences();
			CacheUiReferences();
			WireUiButtons();
			ApplyUiViewState(UiViewState.None);
			DummyStreetPropReactive.PropBroken += HandleStreetPropBroken;
			DummyDestructibleBlock.SmallPropDestroyed += HandleSmallPropDestroyed;
			if (emitReadinessReportOnAwake)
			{
				EmitReadinessReport();
			}
		}

		private void OnDisable()
		{
			DummyStreetPropReactive.PropBroken -= HandleStreetPropBroken;
			DummyDestructibleBlock.SmallPropDestroyed -= HandleSmallPropDestroyed;
			ResetTrafficPanicRuntime();
			ResetAlleyLineRuntime();
			UnbindLandingShockwaveEvent();
			SetPaused(paused: false);
			Time.timeScale = 1f;
		}

		private void Start()
		{
			if ((Object)(object)gameFlowSystem != (Object)null && gameFlowSystem.AutoStartStage)
			{
				StartStage();
			}
			else
			{
				ShowLobby();
			}
		}

		private void Update()
		{
			UpdateActionSkillButtonFeedback(Time.deltaTime);
			if (!stageRunning)
			{
				if (currentUiViewState == UiViewState.Lobby)
				{
					UpdateLobbyRecommendationPresentation(Time.unscaledDeltaTime);
				}
				return;
			}
			if (stagePaused)
			{
				Keyboard current = Keyboard.current;
				if (current != null && (current.escapeKey.wasPressedThisFrame || current.pKey.wasPressedThisFrame))
				{
					TogglePause();
				}
				return;
			}
			EvaluateTotalDestructionProgress();
			if (!stageRunning)
			{
				return;
			}
			UpdateTrafficSimulation(Time.deltaTime);
			UpdateLevelUpRerollRewardsFromChain();
			if (levelUpOpen)
			{
				ProcessLevelUpDraftInput();
				levelUpRemaining -= Time.deltaTime;
				UpdateLevelUpTimer();
				if (levelUpRemaining <= 0f)
				{
					CloseLevelUpWithoutSelection();
				}
				return;
			}
			remainingStageTime -= Time.deltaTime;
			stageStartHintRemaining = Mathf.Max(0f, stageStartHintRemaining - Time.deltaTime);
			scoreTick += Time.deltaTime;
			UpdateMomentumChain(Time.deltaTime);
			UpdateOverdrive(Time.deltaTime);
			UpdateComboRush(Time.deltaTime);
			UpdateRetailFrenzy(Time.deltaTime);
			UpdateStageBossTelemetry(Time.deltaTime);
			UpdateFormActiveSkillCooldown(Time.deltaTime);
			while (scoreTick >= 1f)
			{
				scoreSystem?.AddScore(scorePerSecond);
				scoreTick -= 1f;
			}
			if (remainingStageTime <= 0f)
			{
				FailStage();
				return;
			}
			ProcessActionSkillInput();
			UpdateHudInfo();
			EvaluateStageAdvanceRouteReward();
			if (!stageRunning)
			{
				return;
			}
			EvaluateForwardSmashBonus();
			if (!stageRunning)
			{
				return;
			}
			EvaluateStageAdvanceProgress();
			if (!stageRunning)
			{
				return;
			}
			EvaluateLevelUpProgression();
		}
	}
}
