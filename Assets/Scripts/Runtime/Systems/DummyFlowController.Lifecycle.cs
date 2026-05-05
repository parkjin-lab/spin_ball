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

			if (current.f10Key.wasPressedThisFrame)
			{
				ToggleMapLayoutDebugSweep();
				return true;
			}
#endif
			return false;
		}

		private void StartMapLayoutDebugStage(int stageNumber)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			StopMapLayoutDebugSweep(false);
			int maxStage = Mathf.Max(1, mapLayoutDebugMaxStage);
			currentStageNumber = Mathf.Clamp(stageNumber, 1, maxStage);
			Debug.Log((object)$"[AlienCrusher][MapLayout][Debug] Starting layout test stage {currentStageNumber:00}. F6 previous, F7 next, F8 reset, F9 overlay, F10 sweep.");
			StartStage();
#endif
		}

		private bool StopMapLayoutDebugSweep(bool restoreStage)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (mapLayoutDebugSweepRoutine == null)
			{
				return false;
			}

			StopCoroutine(mapLayoutDebugSweepRoutine);
			mapLayoutDebugSweepRoutine = null;
			int restoredStage = restoreStage ? Mathf.Max(1, mapLayoutDebugSweepRestoreStage) : Mathf.Max(1, currentStageNumber);
			if (restoreStage)
			{
				currentStageNumber = restoredStage;
				RebuildRuntimeStageMap();
			}
			LogPlaytestSweepEnd(restoredStage, completed: false);

			return true;
#else
			return false;
#endif
		}

		private void ToggleMapLayoutDebugSweep()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (StopMapLayoutDebugSweep(true))
			{
				Debug.Log((object)"[AlienCrusher][MapLayout][Debug] Stage sweep stopped.");
				return;
			}

			mapLayoutDebugSweepRestoreStage = Mathf.Max(1, currentStageNumber);
			LogPlaytestSweepStart(Mathf.Max(1, mapLayoutDebugMaxStage), mapLayoutDebugSweepRestoreStage);
			mapLayoutDebugSweepRoutine = StartCoroutine(MapLayoutDebugSweepRoutine());
#endif
		}

		private System.Collections.IEnumerator MapLayoutDebugSweepRoutine()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int maxStage = Mathf.Max(1, mapLayoutDebugMaxStage);
			float stepSeconds = Mathf.Max(0.05f, mapLayoutDebugSweepStepSeconds);
			Debug.Log((object)$"[AlienCrusher][MapLayout][Debug] Sweeping stages 1-{maxStage:00}. F10 stops.");
			for (int stage = 1; stage <= maxStage; stage++)
			{
				currentStageNumber = stage;
				RebuildRuntimeStageMap();
				Debug.Log((object)$"[AlienCrusher][MapLayout][Debug] Sweep stage {stage:00}/{maxStage:00} complete.");
				yield return new WaitForSecondsRealtime(stepSeconds);
			}

			currentStageNumber = Mathf.Max(1, mapLayoutDebugSweepRestoreStage);
			RebuildRuntimeStageMap();
			mapLayoutDebugSweepRoutine = null;
			LogPlaytestSweepEnd(currentStageNumber, completed: true);
			Debug.Log((object)$"[AlienCrusher][MapLayout][Debug] Sweep complete. Restored stage {currentStageNumber:00}.");
#else
			yield break;
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
			bool hasWarnings = !string.IsNullOrEmpty(runtimeMapLayoutDebugWarning) && runtimeMapLayoutDebugWarning != "OK";
			float height = hasWarnings ? 152f : 132f;
			Rect box = new Rect(12f, 12f, width, height);
			GUI.Box(box, GUIContent.none);
			float ageSeconds = Mathf.Max(0f, Time.unscaledTime - runtimeMapLayoutDebugUpdatedAt);
			string sweepText = mapLayoutDebugSweepRoutine == null ? "F10 SWEEP" : "F10 STOP";
			GUI.Label(new Rect(box.x + 10f, box.y + 8f, box.width - 20f, 22f), $"MAP LAYOUT TEST  /  F6 PREV  F7 NEXT  F8 RESET  F9 HIDE  {sweepText}  /  {ageSeconds:0.0}s", runtimeMapLayoutOverlayStyle);
			GUI.Label(new Rect(box.x + 10f, box.y + 32f, box.width - 20f, 40f), runtimeMapLayoutDebugSummary, runtimeMapLayoutOverlayStyle);
			string warningText = runtimeMapLayoutDebugWarning == "OK" ? "warnings: none" : $"warnings: {runtimeMapLayoutDebugWarning}";
			GUI.Label(new Rect(box.x + 10f, box.y + 68f, box.width - 20f, 28f), warningText, runtimeMapLayoutDebugWarning == "OK" ? runtimeMapLayoutOverlayStyle : runtimeMapLayoutWarningStyle);
			GUI.Label(new Rect(box.x + 10f, box.y + 96f, box.width - 20f, 24f), GetRouteDebugPrimaryLine(), runtimeMapLayoutOverlayStyle);
			GUI.Label(new Rect(box.x + 10f, box.y + 116f, box.width - 20f, 32f), GetRouteDebugSecondaryLine(), runtimeMapLayoutOverlayStyle);
#endif
		}

		private string GetRouteDebugPrimaryLine()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int destroyedCount = (scoreSystem != null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0;
			string laneBreakText = $"LANE {Mathf.Min(destroyedCount, GetEarlyCrushLaneBreakTarget()):0}/{GetEarlyCrushLaneBreakTarget():0}";
			string routeOpenText = IsRouteOpenBeatActive() ? $"OPEN {Mathf.CeilToInt(routeOpenBeatRemaining):0}s" : "OPEN idle";
			string routeHoldText = routeHoldBonusGranted
				? "HOLD done"
				: (IsRouteHoldObjectiveActive(destroyedCount)
					? $"HOLD {Mathf.RoundToInt(GetRouteHoldProgress01(destroyedCount) * 100f):0}%  {GetRouteHoldRemainingWrecks(destroyedCount):0} left  {Mathf.CeilToInt(GetRouteHoldRemainingSeconds()):0}s"
					: "HOLD waiting");
			return $"ROUTE TEST  /  {laneBreakText}  /  {routeOpenText}  /  {routeHoldText}";
#else
			return string.Empty;
#endif
		}

		private string GetRouteDebugSecondaryLine()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			string targetText = "target none";
			if (activeStageAdvanceRouteMarker != null && playerTransform != null)
			{
				float distance = Vector3.Distance(playerTransform.position, activeStageAdvanceRouteMarker.position);
				targetText = $"{activeStageAdvanceRouteMarker.name}  {distance:0.#}m";
			}
			string rewardText = stageAdvanceRouteRewardGranted ? "BONUS claimed" : (stageAdvanceRouteGuidanceActive ? "BONUS pending" : "BONUS idle");
			string smashText = IsForwardSmashTargetActive() ? $"SMASH {forwardSmashTargetBlock.name}" : "SMASH idle";
			string payoffText = GetRouteDistrictPayoffLabel();
			return $"FOLLOWUP  /  {rewardText}  /  {smashText}  /  TARGET {targetText}  /  PAYOFF {payoffText}";
#else
			return string.Empty;
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
