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
			if (ProcessMapLayoutDebugHotkeys())
			{
				return;
			}
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

		private bool ProcessMapLayoutDebugHotkeys()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!enableMapLayoutDebugHotkeys)
			{
				return false;
			}

			Keyboard current = Keyboard.current;
			if (current == null)
			{
				return false;
			}

			if (current.f6Key.wasPressedThisFrame)
			{
				StartMapLayoutDebugStage(Mathf.Max(1, currentStageNumber - 1));
				return true;
			}

			if (current.f7Key.wasPressedThisFrame)
			{
				StartMapLayoutDebugStage(Mathf.Min(Mathf.Max(1, mapLayoutDebugMaxStage), currentStageNumber + 1));
				return true;
			}

			if (current.f8Key.wasPressedThisFrame)
			{
				StartMapLayoutDebugStage(1);
				return true;
			}

			if (current.f9Key.wasPressedThisFrame)
			{
				showMapLayoutDebugOverlay = !showMapLayoutDebugOverlay;
				Debug.Log((object)$"[AlienCrusher][MapLayout][Debug] Overlay {(showMapLayoutDebugOverlay ? "shown" : "hidden")}.");
				return true;
			}
#endif
			return false;
		}

		private void StartMapLayoutDebugStage(int stageNumber)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int maxStage = Mathf.Max(1, mapLayoutDebugMaxStage);
			currentStageNumber = Mathf.Clamp(stageNumber, 1, maxStage);
			Debug.Log((object)$"[AlienCrusher][MapLayout][Debug] Starting layout test stage {currentStageNumber:00}. F6 previous, F7 next, F8 reset, F9 overlay.");
			StartStage();
#endif
		}

		private void OnGUI()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!showMapLayoutDebugOverlay || string.IsNullOrEmpty(runtimeMapLayoutDebugSummary))
			{
				return;
			}

			EnsureMapLayoutOverlayStyles();
			float width = Mathf.Min(Mathf.Max(280f, Screen.width - 24f), 760f);
			float height = string.IsNullOrEmpty(runtimeMapLayoutDebugWarning) || runtimeMapLayoutDebugWarning == "OK" ? 86f : 106f;
			Rect box = new Rect(12f, 12f, width, height);
			GUI.Box(box, GUIContent.none);
			float ageSeconds = Mathf.Max(0f, Time.unscaledTime - runtimeMapLayoutDebugUpdatedAt);
			GUI.Label(new Rect(box.x + 10f, box.y + 8f, box.width - 20f, 22f), $"MAP LAYOUT TEST  /  F6 PREV  F7 NEXT  F8 RESET  F9 HIDE  /  {ageSeconds:0.0}s", runtimeMapLayoutOverlayStyle);
			GUI.Label(new Rect(box.x + 10f, box.y + 32f, box.width - 20f, 40f), runtimeMapLayoutDebugSummary, runtimeMapLayoutOverlayStyle);
			string warningText = runtimeMapLayoutDebugWarning == "OK" ? "warnings: none" : $"warnings: {runtimeMapLayoutDebugWarning}";
			GUI.Label(new Rect(box.x + 10f, box.y + 68f, box.width - 20f, 32f), warningText, runtimeMapLayoutDebugWarning == "OK" ? runtimeMapLayoutOverlayStyle : runtimeMapLayoutWarningStyle);
#endif
		}

		private void EnsureMapLayoutOverlayStyles()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (runtimeMapLayoutOverlayStyle != null && runtimeMapLayoutWarningStyle != null)
			{
				return;
			}

			runtimeMapLayoutOverlayStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 13,
				wordWrap = true
			};
			runtimeMapLayoutOverlayStyle.normal.textColor = new Color(0.94f, 0.97f, 1f, 1f);

			runtimeMapLayoutWarningStyle = new GUIStyle(runtimeMapLayoutOverlayStyle);
			runtimeMapLayoutWarningStyle.normal.textColor = new Color(1f, 0.72f, 0.32f, 1f);
#endif
		}
	}
}
