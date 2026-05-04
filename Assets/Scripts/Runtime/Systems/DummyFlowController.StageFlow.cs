using System.Collections;
using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		public void StartStage()
		{
			StopStageAdvanceFinaleRoutine();
			StopBossClearCascadeRoutine();
			ResetStageEndState();
			scoreSystem?.ResetScore();
			scoreSystem?.BreakChain();
			ballGrowthSystem?.ResetGrowth();
			RebuildRuntimeStageMap();
			ResetAllDestructibles();
			ConfigureStageEncounter();
			ResetPlayerToSpawn();
			ResolvePlayerController();
			cachedPlayerController?.ResetUpgrades();
			cameraFollowSystem?.TriggerStageStartFocus();
			cameraFollowSystem?.SnapToTarget();
			((MonoBehaviour)this).CancelInvoke("PlayStageEntryImpact");
			ResetUpgradeTracking();
			ApplySelectedFormToPlayer();
			ResetFormActiveSkillRuntime();
			remainingStageTime = (((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f);
			scoreTick = 0f;
			stageRunning = true;
			levelUpOpen = false;
			SetPaused(paused: false);
			stageRewardGranted = false;
			lastEarnedDp = 0;
			stageStartHintRemaining = Mathf.Max(0f, stageHintDurationSeconds);
			lastChainValue = 1;
			chainPulse = 0f;
			hudUrgencyPulseCooldownRemaining = 0f;
			lastHudUrgencyHint = string.Empty;
			hudHintHoldRemaining = 0f;
			lastStableHudHint = string.Empty;
			ResetOverdriveState();
			ResetComboRushState();
			ResetTrafficPanicRuntime();
			ResetAlleyLineRuntime();
			hudUrgencyPulseCooldownRemaining = 0f;
			lastHudUrgencyHint = string.Empty;
			hudHintHoldRemaining = 0f;
			lastStableHudHint = string.Empty;
			ResetStageBossTelemetry();
			ResetLevelUpDraftRuntime();
			InitializeRoguelikeProgression();
			InitializeTotalDestructionState();
			InitializeTrafficSystem();
			ApplyUiViewState(UiViewState.Hud);
			UpdateHudInfo();
			UpdateUpgradeListText();
			UpdateFormButtons();
			UpdateMetaProgressUi();
			UpdateLobbyMissionGuide();
			RefreshActionSkillButtons(force: true);
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			feedbackSystem?.PlayStageStartFeedback(val + Vector3.up * 0.22f);
			if (useStageEntryImpact)
			{
				((MonoBehaviour)this).Invoke("PlayStageEntryImpact", Mathf.Max(0f, stageEntryDelaySeconds));
			}
		}

		public void ShowLobby()
		{
			LoadSavedStageProgress();
			ResetStageEndState();
			stageRunning = false;
			levelUpOpen = false;
			SetPaused(paused: false);
			StopStageAdvanceFinaleRoutine();
			StopBossClearCascadeRoutine();
			StopTotalDestructionCompletionRoutine();
			ResetOverdriveState();
			ResetComboRushState();
			ResetTrafficPanicRuntime();
			ResetAlleyLineRuntime();
			ResetStageBossTelemetry();
			hudHintHoldRemaining = 0f;
			lastStableHudHint = string.Empty;
			ApplyUiViewState(UiViewState.Lobby);
			lobbyRecommendationFocusRemaining = Mathf.Max(0f, lobbyRecommendationFocusDuration);
			ApplySelectedFormToPlayer();
			UpdateFormButtons();
			UpdateMetaProgressUi();
			UpdateLobbyMissionGuide();
		}

		public void CompleteStage()
		{
			if (stageEndResult == StageEndResult.None && !TryEnterStageEnd(StageEndResult.Victory, StageEndReason.Manual))
			{
				return;
			}
			StopStageAdvanceFinaleRoutine();
			StopBossClearCascadeRoutine();
			stageRunning = false;
			levelUpOpen = false;
			SetPaused(paused: false);
			StopTotalDestructionCompletionRoutine();
			ResetOverdriveState();
			ResetComboRushState();
			ResetTrafficPanicRuntime();
			ResetAlleyLineRuntime();
			ResetStageBossTelemetry();
			ApplyUiViewState(UiViewState.Result);
			if (DidStageEndInSuccess())
			{
				RegisterStageClearProgress();
				GrantStageDpReward();
			}
			else
			{
				stageRewardGranted = false;
				lastEarnedDp = 0;
			}
			UpdateMetaProgressUi();
			UpdateResultSummary();
		}

		public void FailStage()
		{
			RouteStageDefeatEnd(StageEndReason.TimerExpired);
		}

		public void WatchAdAndRestart()
		{
			scoreSystem?.AddScore(1000);
			StartStage();
		}

		public void StartNextStage()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			int num = Mathf.Max(1, currentStageNumber + 1);
			if ((Object)(object)formUnlockSystem != (Object)null)
			{
				num = Mathf.Max(num, formUnlockSystem.HighestUnlockedStage);
				formUnlockSystem.SetCurrentLobbyStage(num);
			}
			currentStageNumber = num;
			StartStage();
		}

		public void TogglePause()
		{
			if (stageRunning && !levelUpOpen)
			{
				SetPaused(!stagePaused);
			}
		}

		public void ResumeFromPause()
		{
			SetPaused(paused: false);
		}

		public void RestartFromPause()
		{
			SetPaused(paused: false);
			StartStage();
		}

		public void ReturnLobbyFromPause()
		{
			SetPaused(paused: false);
			ShowLobby();
		}

		private void UpdateResultSummary()
		{
			if (!((Object)(object)resultSummaryText == (Object)null))
			{
				if (formUnlockSystem == null)
				{
					formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
				}
				int num = (((Object)(object)scoreSystem != (Object)null) ? scoreSystem.CurrentScore : 0);
				bool flag = DidStageEndInSuccess();
				int num2 = (flag ? (stageRewardGranted ? lastEarnedDp : CalculateStageDpReward(num)) : 0);
				int num3 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.DpBalance : 0);
				int num4 = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.HighestChain));
				int num5 = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
				int num6 = Mathf.Max(1, stageTotalDestructibleCount);
				int num7 = Mathf.RoundToInt(Mathf.Clamp01((float)num5 / (float)num6) * 100f);
				string textReason = GetStageEndReasonLabel().ToUpperInvariant();
				string text = (totalDestructionAchieved ? $"TOTAL DESTRUCTION +{Mathf.Max(0, totalDestructionBonusAwarded):0}" : $"DESTROYED {num5:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}");
				string textMission = enableStripClearMission ? (stripClearMissionCompleted ? $"STRIP MISSION CLEAR +{Mathf.Max(0, stripClearMissionScoreAwarded):0} / DP+{Mathf.Max(0, stripClearMissionDpAwarded):0}" : $"STRIP CLEAR {stageStripClearCount:0}/{stageStripClearTarget:0}") : "STRIP MISSION OFF";
				string text2 = ((!stageBossEncounterActive) ? "BOSS: NONE" : (IsStageBossAlive() ? "BOSS: JUSTICE SENTINEL ACTIVE" : "BOSS: JUSTICE SENTINEL DESTROYED"));
				string text3 = ((!stageBossEncounterActive) ? "N/A" : (IsStageBossAlive() ? "ALIVE" : "DOWN"));
				string text4 = flag ? "STATUS CLEAR" : $"STATUS FAILED / {textReason}";
				string text5 = flag ? "CITY SECTOR COLLAPSED" : GetFailureActionLine();
				resultSummaryText.text = $"STAGE {currentStageNumber:00}\n{text4}\n{text5}\nSCORE {num:0}\nDP +{num2:0}\nTOTAL DP {num3:0}\nCHAIN x{num4}\n{text}\n{textMission}\n{text2}";
				if ((Object)(object)resultBreakdownText != (Object)null)
				{
					resultBreakdownText.text = $"DESTROYED {num5:0}/{Mathf.Max(0, stageTotalDestructibleCount):0} ({num7}%)\nTIME LEFT {Mathf.CeilToInt(Mathf.Max(0f, remainingStageTime)):0}s\nSTRIP CLEAR {stageStripClearCount:0}/{stageStripClearTarget:0}\nELITE WP {stageEliteWeakPointCount:0} / BOSS {text3}\nBONUS +{(flag ? Mathf.Max(0, totalDestructionBonusAwarded) : 0):0}\nEND REASON {textReason}";
				}
				if ((Object)(object)resultContinueHintText != (Object)null)
				{
					resultContinueHintText.text = flag ? $"PRIMARY  NEXT STAGE ({currentStageNumber + 1:00})\nSECONDARY  RESTART THIS STAGE FOR A BIGGER SCORE" : $"PRIMARY  RESTART STAGE\nSECONDARY  RETURN TO LOBBY AND REBUILD YOUR LOADOUT";
				}
				UpdateResultButtonStates(flag);
				UpdateResultAdvice(flag, num5, num7);
				UpdateResultStatusBadges(num2, num3, num7);
				UpdateResultHighlights(num4, num5, num7, text3);
				UpdateResultMetaProgress(num2);
			}
		}

		private void UpdateResultStatusBadges(int stageDpEarned, int totalDp, int destroyedPercent)
		{
			if ((Object)(object)resultStatusBadgeText == (Object)null)
			{
				return;
			}
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			string text = string.Empty;
			int num = 0;
			if ((Object)(object)formUnlockSystem != (Object)null)
			{
				bool flag = DidStageEndInSuccess() && currentStageNumber >= formUnlockSystem.HighestUnlockedStage;
				bool flag2 = HasReadyFormUnlock(totalDp);
				bool flag3 = HasReadyMetaUpgrade(totalDp);
				if (flag)
				{
					text += "[NEW BEST]  ";
					num++;
				}
				if (flag2)
				{
					text += "[FORM READY]  ";
					num++;
				}
				if (flag3 && num < 2)
				{
					text += "[META READY]  ";
					num++;
				}
			}
			if (routeHoldBonusGranted && num < 2)
			{
				text += "[ROUTE HOLD]  ";
				num++;
			}
			if (earlyCrushRecoveryBonusGranted && num < 2)
			{
				text += "[RECOVERY BREAK]  ";
				num++;
			}
			else if (earlyCrushFlowBonusIndex >= 3 && num < 2)
			{
				text += "[LANE BREAK]  ";
				num++;
			}
			if ((totalDestructionAchieved || destroyedPercent >= 96) && num < 2)
			{
				text += "[TOTAL WIPE]  ";
				num++;
			}
			if (stripClearMissionCompleted && num < 2)
			{
				text += "[MISSION CLEAR]  ";
				num++;
			}
			if (stageBossEncounterActive && !IsStageBossAlive() && num < 2)
			{
				text += "[BOSS DOWN]  ";
				num++;
			}
			if (stageDpEarned >= 100 && num < 2)
			{
				text += "[BIG DP]  ";
				num++;
			}
			if (!DidStageEndInSuccess())
			{
				string failureBadge = stageEndReason switch
				{
					StageEndReason.TimerExpired => "[TIMEOUT]",
					_ => "[REGROUP]"
				};
				text = string.IsNullOrWhiteSpace(text) ? failureBadge : $"{failureBadge}  {text.TrimEnd()}";
			}
			resultStatusBadgeText.text = (string.IsNullOrWhiteSpace(text) ? "[CITY BREACH]" : text.TrimEnd());
		}

		private void UpdateResultHighlights(int highestChain, int destroyedCount, int destroyedPercent, string bossState)
		{
			if ((Object)(object)resultHighlightText == (Object)null)
			{
				return;
			}
			string text = GetResultHeadline(highestChain, destroyedPercent, bossState);
			string text2 = GetResultMomentumTag(highestChain, destroyedCount);
			string text3 = GetResultObjectiveTag();
			string text4 = GetResultStyleTag(highestChain, destroyedPercent);
			resultHighlightText.text = $"{text}\n{text2}   |   {text3}\n{text4}";
		}

		private string GetResultHeadline(int highestChain, int destroyedPercent, string bossState)
		{
			if (!DidStageEndInSuccess())
			{
				return stageEndReason switch
				{
					StageEndReason.TimerExpired => "ASSAULT STALLED OUT",
					_ => "INVASION PUSH BROKEN"
				};
			}
			if (stageBossEncounterActive && bossState == "DOWN")
			{
				return "JUSTICE SENTINEL ERADICATED";
			}
			if (totalDestructionAchieved || destroyedPercent >= 96)
			{
				return "DISTRICT ANNIHILATED";
			}
			if (highestChain >= 12)
			{
				return "CHAIN RAMPAGE CONFIRMED";
			}
			if (destroyedPercent >= 72)
			{
				return "CITY BLOCK SHATTERED";
			}
			return "ALIEN IMPACT SUCCESS";
		}

		private string GetResultMomentumTag(int highestChain, int destroyedCount)
		{
			if (routeHoldBonusGranted)
			{
				return $"Momentum: Route Hold carried tempo / {destroyedCount:0} wrecks";
			}
			if (earlyCrushRecoveryBonusGranted)
			{
				return $"Momentum: Recovery Break reclaimed pace / {destroyedCount:0} wrecks";
			}
			if (earlyCrushFlowBonusIndex >= 3)
			{
				return $"Momentum: Lane Break opening / {destroyedCount:0} wrecks";
			}
			if (!DidStageEndInSuccess())
			{
				if (destroyedCount >= Mathf.Max(8, stageAdvanceDestroyTarget / 2))
				{
					return $"Momentum: Strong opening / {destroyedCount:0} wrecks before collapse";
				}
				return $"Momentum: Chain broke at x{Mathf.Max(1, highestChain):0}";
			}
			if (highestChain >= 14)
			{
				return $"Momentum: Overwhelming chain x{highestChain:0}";
			}
			if (highestChain >= 8)
			{
				return $"Momentum: Crush rhythm x{highestChain:0}";
			}
			if (destroyedCount >= Mathf.Max(14, stageAdvanceDestroyTarget))
			{
				return $"Momentum: Relentless push {destroyedCount:0} wrecks";
			}
			return $"Momentum: Breakthrough run x{Mathf.Max(1, highestChain):0}";
		}

		private string GetResultObjectiveTag()
		{
			if (!DidStageEndInSuccess())
			{
				if (enableStageAdvanceGoal && stageAdvanceDestroyTarget > 0)
				{
					int num = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
					int num2 = Mathf.Max(0, stageAdvanceDestroyTarget - num);
					if ((float)num / Mathf.Max(1f, stageAdvanceDestroyTarget) < 0.45f)
					{
						return $"Objective: Open on low-rise lanes first / {num2:0} left";
					}
					return $"Objective: Push the goal lane harder / {num2:0} left";
				}
				return "Objective: Return stronger and keep the chain alive";
			}
			if (stripClearMissionCompleted)
			{
				return $"Objective: Strip mission complete / DP +{Mathf.Max(0, stripClearMissionDpAwarded):0}";
			}
			if (stageAdvanceGoalReached)
			{
				return $"Objective: Stage gate opened at {Mathf.Max(0, stageAdvanceDestroyTarget):0} wrecks";
			}
			return $"Objective: {Mathf.Max(0, stageStripClearCount):0}/{Mathf.Max(0, stageStripClearTarget):0} strip clears";
		}

		private string GetResultStyleTag(int highestChain, int destroyedPercent)
		{
			if (!DidStageEndInSuccess())
			{
				return "Style: Rebuild route, protect momentum, strike again";
			}
			if (forwardSmashTargetBlock == null && highestChain >= 10)
			{
				return "Style: High-speed combo predator";
			}
			if (destroyedPercent >= 90)
			{
				return "Style: Full-route urban crusher";
			}
			if (HasCurrentSelectedFormDrillBias())
			{
				return "Style: Drill breach demolition";
			}
			return $"Style: {GetCurrentSelectedForm().ToString().ToUpperInvariant()} assault pattern";
		}

		private void UpdateResultMetaProgress(int stageDpEarned)
		{
			if ((Object)(object)resultMetaProgressText == (Object)null)
			{
				return;
			}
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			int num = (((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(0, formUnlockSystem.DpBalance) : 0);
			int num2 = (((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(1, formUnlockSystem.HighestUnlockedStage) : Mathf.Max(1, currentStageNumber));
			int num3 = Mathf.Max(1, num2 + 1);
			string text = GetResultNextFormHint(num);
			string text2 = GetResultNextMetaHint(num);
			string text3 = (DidStageEndInSuccess() && currentStageNumber >= num2) ? $"BEST STAGE UPDATED  {num2:00}" : $"BEST STAGE  {num2:00}";
			string text4 = $"NEXT LOBBY TARGET  STAGE {num3:00}";
			string text5 = GetResultProgressRevealLine();
			resultMetaProgressText.text = $"META STATUS  |  DP TOTAL {num:0}  (+{Mathf.Max(0, stageDpEarned):0})\n{text3}  |  {text4}\n{text}\n{text2}\n{text5}";
		}

		private string GetFailureActionLine()
		{
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				return "BOSS PHASE / BREAK PYLONS, THEN BURST CORE ON OPEN";
			}
			if (stageEndReason == StageEndReason.TimerExpired)
			{
				int num = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
				float num2 = (enableStageAdvanceGoal && stageAdvanceDestroyTarget > 0) ? ((float)num / Mathf.Max(1f, stageAdvanceDestroyTarget)) : 0f;
				if (num2 < 0.45f)
				{
					return "OPENING FAILED / ENTER DENSER LOW-RISE LANES IMMEDIATELY";
				}
				if (enableStageAdvanceGoal && !stageAdvanceGoalReached && num2 >= Mathf.Clamp(stageAdvanceNearCompleteThreshold, 0.6f, 0.98f))
				{
					return "FINAL PUSH FAILED / STAY ON THE GOAL LANE TO FINISH";
				}
				if (earlyCrushFlowBonusIndex >= 3 && !routeHoldBonusGranted)
				{
					return "ROUTE HOLD MISSED / KEEP THE GOAL LANE AFTER LANE BREAK";
				}
				return "MID-RUN DRIFT / HOLD THE ROUTE AND KEEP CHAIN PRESSURE";
			}
			return stageEndReason switch
			{
				_ => "ASSAULT LOST MOMENTUM / RE-ENTER THE DISTRICT"
			};
		}

		private void UpdateResultButtonStates(bool cleared)
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			int num = ((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(0, formUnlockSystem.DpBalance) : 0;
			bool flag = HasReadyFormUnlock(num);
			bool flag2 = HasReadyMetaUpgrade(num);
			bool flag3 = flag || flag2;
			string text = flag3 ? "RETURN TO LOBBY / SPEND DP" : "RETURN TO LOBBY";
			Color backgroundColor = flag3 ? new Color(0.3f, 0.56f, 0.72f, 0.98f) : new Color(0.2f, 0.24f, 0.3f, 0.94f);
			Color backgroundColor2 = cleared
				? (flag3 ? new Color(0.18f, 0.3f, 0.22f, 0.86f) : new Color(0.26f, 0.7f, 0.4f, 1f))
				: new Color(0.28f, 0.28f, 0.32f, 0.85f);
			string text2 = cleared ? (flag3 ? $"NEXT STAGE {currentStageNumber + 1:00} / WAIT" : $"NEXT STAGE {currentStageNumber + 1:00}") : "NEXT STAGE LOCKED";
			Color backgroundColor3 = cleared
				? (flag3 ? new Color(0.3f, 0.28f, 0.2f, 0.92f) : new Color(0.19f, 0.46f, 0.72f, 1f))
				: new Color(0.78f, 0.34f, 0.24f, 1f);
			string text3 = cleared ? (flag3 ? "RESTART FOR SCORE ONLY" : "RESTART FOR HIGHER SCORE") : "RESTART STAGE";
			SetButtonPresentation("NextStageButton", text2, cleared, backgroundColor2, Color.white);
			SetButtonPresentation("WatchAdButton", text3, true, backgroundColor3, Color.white);
			SetButtonPresentation("ResultLobbyButton", text, true, backgroundColor, Color.white);
		}

		private void UpdateResultAdvice(bool cleared, int destroyedCount, int destroyedPercent)
		{
			if ((Object)(object)resultAdviceText == (Object)null)
			{
				return;
			}
			string resultProgressRevealLine = GetResultProgressRevealLine();
			if (cleared)
			{
				resultAdviceText.text = $"{GetClearAdviceText(destroyedCount, destroyedPercent)}\n{resultProgressRevealLine}";
				resultAdviceText.color = new Color(0.78f, 0.96f, 0.86f, 1f);
			}
			else
			{
				resultAdviceText.text = $"{GetFailureAdviceText(destroyedCount, destroyedPercent)}\n{resultProgressRevealLine}";
				resultAdviceText.color = new Color(1f, 0.84f, 0.64f, 1f);
			}
		}

		private string GetResultProgressRevealLine()
		{
			if (!string.IsNullOrWhiteSpace(lastLobbyActionStatus))
			{
				return $"PROGRESSION  {lastLobbyActionStatus}";
			}
			if (DidStageEndInSuccess())
			{
				return "PROGRESSION  Return to lobby to review newly available goals.";
			}
			return "PROGRESSION  Return to lobby to strengthen your build path.";
		}

		private string GetClearAdviceText(int destroyedCount, int destroyedPercent)
		{
			if (routeHoldBonusGranted)
			{
				return "TACTICAL NOTE  Opening carried into mid-run. Keep this route tempo into harder districts.";
			}
			if (earlyCrushRecoveryBonusGranted)
			{
				return "TACTICAL NOTE  Recovery worked. Next run, hit the starter lane earlier for full LANE BREAK tempo.";
			}
			if (earlyCrushFlowBonusIndex >= 3)
			{
				return "TACTICAL NOTE  Opening route is stable. Preserve that speed into the mid-run goal lane.";
			}
			if (totalDestructionAchieved || destroyedPercent >= 96)
			{
				return "TACTICAL NOTE  Full route control. Push the next stage now.";
			}
			if (destroyedCount < stageAdvanceDestroyTarget + 4)
			{
				return "TACTICAL NOTE  Narrow clear. Add speed or impact, then keep the opening rush cleaner.";
			}
			return "TACTICAL NOTE  Route is stable. Restart only for score, otherwise move on.";
		}

		private string GetFailureAdviceText(int destroyedCount, int destroyedPercent)
		{
			float num = (enableStageAdvanceGoal && stageAdvanceDestroyTarget > 0) ? ((float)Mathf.Max(0, destroyedCount) / Mathf.Max(1f, stageAdvanceDestroyTarget)) : 0f;
			if (earlyCrushRecoveryBonusGranted)
			{
				return "TACTICAL NOTE  Recovery saved the opening late. Convert it into earlier LANE BREAK next run.";
			}
			if (earlyCrushFlowBonusIndex >= 3 && !routeHoldBonusGranted && stageEndReason == StageEndReason.TimerExpired)
			{
				return "TACTICAL NOTE  LANE BREAK worked, but ROUTE HOLD was missed. Stay on the goal lane after the opening.";
			}
			if (earlyCrushFlowBonusIndex >= 3 && stageEndReason == StageEndReason.TimerExpired)
			{
				return "TACTICAL NOTE  Opening was strong. Failure came after tempo drop, so hold the route lane longer.";
			}
			if (stageEndReason == StageEndReason.TimerExpired)
			{
				if (enableStageAdvanceGoal && destroyedCount < stageAdvanceDestroyTarget)
				{
					if (num < 0.45f)
					{
						return "TACTICAL NOTE  Opening was too thin. Enter a denser low-rise lane immediately.";
					}
					if (num >= Mathf.Clamp(stageAdvanceNearCompleteThreshold, 0.6f, 0.98f))
					{
						return "TACTICAL NOTE  Final push broke late. Ignore side props and force the goal lane to close the stage.";
					}
					return "TACTICAL NOTE  Mid-run pace slipped. Stop drifting and stay on the goal lane.";
				}
				return "TACTICAL NOTE  Time expired. Commit to dense low-rise clusters sooner.";
			}
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				if (stageBossShieldActiveCount > 0)
				{
					return "TACTICAL NOTE  Sentinel held the district. Break pylons first, then save burst for the core window.";
				}
				if (stageBossPhaseTwoDroneRecoveryRemaining > 0.001f)
				{
					return "TACTICAL NOTE  Core window was open. Stay near center and cash damage in before the drones return.";
				}
				return "TACTICAL NOTE  Sentinel held the district. Break pylons, then burst the core on BREAK.";
			}
			if (destroyedPercent < 35)
			{
				return "TACTICAL NOTE  Opening stalled early. Build speed on props and low-rise blocks first.";
			}
			return "TACTICAL NOTE  Momentum collapsed mid-run. Protect the opening chain and stay inside dense lanes.";
		}

		private void SetButtonPresentation(string buttonName, string label, bool interactable, Color backgroundColor, Color textColor)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}
			button.interactable = interactable;
			Image component = ((Component)button).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				component.color = backgroundColor;
			}
			Text componentInChildren = ((Component)button).GetComponentInChildren<Text>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				componentInChildren.text = label;
				componentInChildren.color = textColor;
			}
		}

		private string GetResultNextFormHint(int currentDp)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return "NEXT FORM  Form system offline";
			}
			FormType[] array = new FormType[5]
			{
				FormType.Sphere,
				FormType.Spike,
				FormType.Ram,
				FormType.Saucer,
				FormType.Crusher
			};
			for (int i = 0; i < array.Length; i++)
			{
				FormType formType = array[i];
				if (!formUnlockSystem.IsUnlocked(formType))
				{
					int unlockCost = formUnlockSystem.GetUnlockCost(formType);
					int num = Mathf.Max(0, unlockCost - currentDp);
					return (num <= 0) ? $"NEXT FORM  {formType.ToString().ToUpperInvariant()} READY ({unlockCost:0} DP)" : $"NEXT FORM  {formType.ToString().ToUpperInvariant()} / NEED {num:0} DP MORE";
				}
			}
			return $"NEXT FORM  ALL FORMS UNLOCKED / ACTIVE {GetCurrentSelectedForm().ToString().ToUpperInvariant()}";
		}

		private string GetResultNextMetaHint(int currentDp)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return "NEXT META  Meta system offline";
			}
			FormUnlockSystem.MetaUpgradeType previewMetaUpgradeType = GetPreviewMetaUpgradeType();
			int metaUpgradeLevel = formUnlockSystem.GetMetaUpgradeLevel(previewMetaUpgradeType);
			int metaUpgradeMaxLevel = formUnlockSystem.GetMetaUpgradeMaxLevel(previewMetaUpgradeType);
			int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(previewMetaUpgradeType);
			if (metaUpgradeCost <= 0)
			{
				return $"NEXT META  {GetMetaUpgradeName(previewMetaUpgradeType)} MAXED ({metaUpgradeLevel}/{metaUpgradeMaxLevel})";
			}
			int num = Mathf.Max(0, metaUpgradeCost - currentDp);
			return (num <= 0) ? $"NEXT META  {GetMetaUpgradeName(previewMetaUpgradeType)} LV {metaUpgradeLevel}/{metaUpgradeMaxLevel} READY ({metaUpgradeCost:0} DP)" : $"NEXT META  {GetMetaUpgradeName(previewMetaUpgradeType)} / NEED {num:0} DP MORE";
		}

		private bool HasReadyFormUnlock(int currentDp)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return false;
			}
			FormType[] array = new FormType[5]
			{
				FormType.Sphere,
				FormType.Spike,
				FormType.Ram,
				FormType.Saucer,
				FormType.Crusher
			};
			for (int i = 0; i < array.Length; i++)
			{
				FormType formType = array[i];
				if (!formUnlockSystem.IsUnlocked(formType) && currentDp >= formUnlockSystem.GetUnlockCost(formType))
				{
					return true;
				}
			}
			return false;
		}

		private bool HasReadyMetaUpgrade(int currentDp)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return false;
			}
			FormUnlockSystem.MetaUpgradeType[] array = new FormUnlockSystem.MetaUpgradeType[3]
			{
				FormUnlockSystem.MetaUpgradeType.SizeCore,
				FormUnlockSystem.MetaUpgradeType.ImpactCore,
				FormUnlockSystem.MetaUpgradeType.DpAmplifier
			};
			for (int i = 0; i < array.Length; i++)
			{
				FormUnlockSystem.MetaUpgradeType metaUpgradeType = array[i];
				int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(metaUpgradeType);
				if (metaUpgradeCost > 0 && currentDp >= metaUpgradeCost)
				{
					return true;
				}
			}
			return false;
		}

		private void ResetStageEndState()
		{
			stageEndLocked = false;
			stageEndResult = StageEndResult.None;
			stageEndReason = StageEndReason.None;
		}

		private bool TryEnterStageEnd(StageEndResult result, StageEndReason reason)
		{
			if (stageEndLocked)
			{
				return false;
			}
			stageEndLocked = true;
			stageEndResult = result;
			stageEndReason = reason;
			return true;
		}

		private bool RouteStageVictoryEnd(StageEndReason reason, Vector3 center, StageVictoryFlow flow)
		{
			if (!TryEnterStageEnd(StageEndResult.Victory, reason))
			{
				return false;
			}
			switch (flow)
			{
			case StageVictoryFlow.TotalDestructionDelay:
				PrepareStageEndForVictoryFlow();
				StopTotalDestructionCompletionRoutine();
				totalDestructionCompleteRoutine = ((MonoBehaviour)this).StartCoroutine(CompleteStageAfterTotalDestructionDelay());
				return true;
			case StageVictoryFlow.StageFinale:
				BeginStageAdvanceFinale(center);
				return true;
			case StageVictoryFlow.BossCascade:
				BeginBossClearCascade(center);
				return true;
			default:
				CompleteStage();
				return true;
			}
		}

		private bool RouteStageDefeatEnd(StageEndReason reason)
		{
			if (!TryEnterStageEnd(StageEndResult.Defeat, reason))
			{
				return false;
			}
			CompleteStage();
			return true;
		}

		private void PrepareStageEndForVictoryFlow()
		{
			if (levelUpOpen)
			{
				levelUpOpen = false;
				ClearLevelUpLockSelection();
				ApplyUiViewState(UiViewState.Hud);
			}
		}

		private bool DidStageEndInSuccess()
		{
			return stageEndResult == StageEndResult.Victory;
		}

		private string GetStageEndReasonLabel()
		{
			return stageEndReason switch
			{
				StageEndReason.TimerExpired => "TIME UP",
				StageEndReason.BossDefeated => "BOSS DOWN",
				StageEndReason.TotalDestruction => "TOTAL WIPE",
				StageEndReason.GoalReached => "GOAL REACHED",
				_ => "RUN ENDED",
			};
		}

		private bool HasCurrentSelectedFormDrillBias()
		{
			return drillUpgradeCount > 0 || GetCurrentSelectedForm() == FormType.Spike || GetCurrentSelectedForm() == FormType.Crusher;
		}

		private IEnumerator CompleteStageAfterTotalDestructionDelay()
		{
			float wait = Mathf.Max(0f, totalDestructionClearDelay);
			while (wait > 0f)
			{
				if (stageEndResult != StageEndResult.Victory && !stageRunning)
				{
					totalDestructionCompleteRoutine = null;
					yield break;
				}
				wait -= Time.deltaTime;
				yield return null;
			}
			totalDestructionCompleteRoutine = null;
			if (stageRunning || stageEndResult == StageEndResult.Victory)
			{
				CompleteStage();
			}
		}
	}
}

