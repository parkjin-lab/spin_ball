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
			ClearOutgameProgressionTransientVisuals();
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
			LogPlaytestStageStart();
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			feedbackSystem?.PlayStageStartFeedback(val + Vector3.up * 0.22f);
			PlaySpendChangeReadyPulse();
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
			LogPlaytestStageEnd();
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
				string textMission = enableStripClearMission ? (stripClearMissionCompleted ? $"STRIP DONE" : $"STRIP {stageStripClearCount:0}/{stageStripClearTarget:0}") : "STRIP OFF";
				string text2 = ((!stageBossEncounterActive) ? "BOSS  NONE" : (IsStageBossAlive() ? "BOSS  LIVE" : "BOSS  DOWN"));
				string text3 = ((!stageBossEncounterActive) ? "N/A" : (IsStageBossAlive() ? "ALIVE" : "DOWN"));
				string text4 = flag ? "CLEAR" : $"FAIL  {textReason}";
				string text5 = flag ? string.Empty : GetFailureActionLine();
				resultSummaryText.text = string.IsNullOrEmpty(text5)
					? $"STAGE {currentStageNumber:00}  {text4}\nSCORE {num:0}  DP +{num2:0}\nWRECK {num5:0}/{num6:0}  CHAIN x{num4}\n{textMission}  {text2}"
					: $"STAGE {currentStageNumber:00}  {text4}\n{text5}\nSCORE {num:0}  DP +{num2:0}\nWRECK {num5:0}/{num6:0}  CHAIN x{num4}";
				if ((Object)(object)resultBreakdownText != (Object)null)
				{
					resultBreakdownText.text = $"WRECK {num5:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}  {num7}%\nTIME  {Mathf.CeilToInt(Mathf.Max(0f, remainingStageTime)):0}s  STRIP {stageStripClearCount:0}/{stageStripClearTarget:0}\nBOSS {text3}  {textReason}";
				}
				if ((Object)(object)resultContinueHintText != (Object)null)
				{
					resultContinueHintText.text = flag ? $"NEXT  {currentStageNumber + 1:00}" : "RESTART";
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
			RefreshResultLobbyBadges();
			RefreshOutgameDpEconomy();
			RefreshOutgameProgressionVisuals();
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
					StageEndReason.TimerExpired => "TIMEOUT",
					_ => "BROKEN"
				};
			}
			if (stageBossEncounterActive && bossState == "DOWN")
			{
				return "SENTINEL DOWN";
			}
			if (totalDestructionAchieved || destroyedPercent >= 96)
			{
				return "WIPE";
			}
			if (highestChain >= 12)
			{
				return "CHAIN";
			}
			if (destroyedPercent >= 72)
			{
				return "SHATTER";
			}
			return "CLEAR";
		}

		private string GetResultMomentumTag(int highestChain, int destroyedCount)
		{
			if (routeHoldBonusGranted)
			{
				return $"HOLD  {destroyedCount:0}";
			}
			if (earlyCrushRecoveryBonusGranted)
			{
				return $"RECOVER  {destroyedCount:0}";
			}
			if (earlyCrushFlowBonusIndex >= 3)
			{
				return $"BREAK  {destroyedCount:0}";
			}
			if (!DidStageEndInSuccess())
			{
				if (destroyedCount >= Mathf.Max(8, stageAdvanceDestroyTarget / 2))
				{
					return $"OPEN  {destroyedCount:0}";
				}
				return $"CHAIN x{Mathf.Max(1, highestChain):0}";
			}
			if (highestChain >= 14)
			{
				return $"CHAIN x{highestChain:0}";
			}
			if (highestChain >= 8)
			{
				return $"CHAIN x{highestChain:0}";
			}
			if (destroyedCount >= Mathf.Max(14, stageAdvanceDestroyTarget))
			{
				return $"PUSH  {destroyedCount:0}";
			}
			return $"CHAIN x{Mathf.Max(1, highestChain):0}";
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
						return $"OPEN  {num2:0} left";
					}
					return $"GOAL  {num2:0} left";
				}
				return "REBUILD";
			}
			if (stripClearMissionCompleted)
			{
				return $"STRIP  DP +{Mathf.Max(0, stripClearMissionDpAwarded):0}";
			}
			if (stageAdvanceGoalReached)
			{
				return $"GATE  {Mathf.Max(0, stageAdvanceDestroyTarget):0}";
			}
			return $"STRIP {Mathf.Max(0, stageStripClearCount):0}/{Mathf.Max(0, stageStripClearTarget):0}";
		}

		private string GetResultStyleTag(int highestChain, int destroyedPercent)
		{
			if (!DidStageEndInSuccess())
			{
				return "REBUILD";
			}
			if (forwardSmashTargetBlock == null && highestChain >= 10)
			{
				return "COMBO";
			}
			if (destroyedPercent >= 90)
			{
				return "WIPE";
			}
			if (HasCurrentSelectedFormDrillBias())
			{
				return "DRILL";
			}
			return GetCurrentSelectedForm().ToString().ToUpperInvariant();
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
			string text = GetResultNextFormHint(num);
			string text2 = GetResultNextMetaHint(num);
			string text3 = $"BEST STAGE  {num2:00}";
			resultMetaProgressText.text = $"DP {num:0}  (+{Mathf.Max(0, stageDpEarned):0})\n{text3}\n{text}  {text2}";
		}

		private string GetFailureActionLine()
		{
			string firstActionLine = GetLastRunFirstActionLine();
			if (!string.IsNullOrWhiteSpace(firstActionLine))
			{
				return firstActionLine;
			}
			return "CHAIN";
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
			string text3 = cleared ? "RESTART" : "RESTART";
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
			if (cleared)
			{
				resultAdviceText.text = GetClearAdviceText(destroyedCount, destroyedPercent);
				resultAdviceText.color = new Color(0.88f, 1f, 0.92f, 1f);
			}
			else
			{
				resultAdviceText.text = GetFailureAdviceText(destroyedCount, destroyedPercent);
				resultAdviceText.color = new Color(1f, 0.92f, 0.74f, 1f);
			}
		}

		private string GetResultProgressRevealLine()
		{
			if (!string.IsNullOrWhiteSpace(lastLobbyActionStatus))
			{
				return lastLobbyActionStatus;
			}
			if (DidStageEndInSuccess())
			{
				return "LOBBY";
			}
			return "LOBBY";
		}

		private string GetClearAdviceText(int destroyedCount, int destroyedPercent)
		{
			if (routeHoldBonusGranted)
			{
				return "HOLD  keep tempo";
			}
			if (earlyCrushRecoveryBonusGranted)
			{
				return "RECOVER  earlier lane";
			}
			if (earlyCrushFlowBonusIndex >= 3)
			{
				return "OPEN  keep speed";
			}
			if (totalDestructionAchieved || destroyedPercent >= 96)
			{
				return "CLEAR  next stage";
			}
			if (destroyedCount < stageAdvanceDestroyTarget + 4)
			{
				return "CLOSE  add speed";
			}
			return "STABLE  next";
		}

		private string GetFailureAdviceText(int destroyedCount, int destroyedPercent)
		{
			string actionLine = GetLastRunFirstActionLine();
			if (!string.IsNullOrWhiteSpace(actionLine))
			{
				return actionLine;
			}
			string whyLine = GetLastRunFailureWhyLine(destroyedCount, destroyedPercent);
			return string.IsNullOrWhiteSpace(whyLine) ? "CHAIN" : whyLine;
		}

		private string GetLastRunFirstActionLine()
		{
			string bucket = GetLastRunFailureBucket();
			return bucket switch
			{
				"OPENING FAILED" => "LOW-RISE",
				"ROUTE HOLD MISSED" => "BEACON",
				"MID-RUN DRIFT" => "CLUSTER",
				"FINAL PUSH FAILED" => "GOAL LANE",
				"BOSS PHASE" => "PYLONS",
				"RUN COLLAPSE" => "CHAIN",
				_ => string.Empty,
			};
		}

		private string GetLastRunFailureWhyLine(int destroyedCount, int destroyedPercent)
		{
			string bucket = GetLastRunFailureBucket();
			return bucket switch
			{
				"OPENING FAILED" => "thin open",
				"ROUTE HOLD MISSED" => "HOLD missed",
				"MID-RUN DRIFT" => "drifted",
				"FINAL PUSH FAILED" => "goal close",
				"BOSS PHASE" => stageBossShieldActiveCount > 0 ? "pylons blocked core" : "burst on open core",
				"RUN COLLAPSE" => destroyedPercent < 35 ? "open chain broke" : "route collapsed",
				_ => destroyedCount < stageAdvanceDestroyTarget ? "target out of reach" : "lost momentum",
			};
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
				return "FORM OFF";
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
					return (num <= 0) ? $"{formType.ToString().ToUpperInvariant()} READY" : $"{formType.ToString().ToUpperInvariant()}  NEED {num:0}";
				}
			}
			return GetCurrentSelectedForm().ToString().ToUpperInvariant();
		}

		private string GetResultNextMetaHint(int currentDp)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return "META OFF";
			}
			FormUnlockSystem.MetaUpgradeType previewMetaUpgradeType = GetPreviewMetaUpgradeType();
			int metaUpgradeLevel = formUnlockSystem.GetMetaUpgradeLevel(previewMetaUpgradeType);
			int metaUpgradeMaxLevel = formUnlockSystem.GetMetaUpgradeMaxLevel(previewMetaUpgradeType);
			int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(previewMetaUpgradeType);
			if (metaUpgradeCost <= 0)
			{
				return $"{GetMetaUpgradeShortTag(previewMetaUpgradeType)} MAXED";
			}
			int num = Mathf.Max(0, metaUpgradeCost - currentDp);
			return (num <= 0) ? $"{GetMetaUpgradeShortTag(previewMetaUpgradeType)} READY" : $"{GetMetaUpgradeShortTag(previewMetaUpgradeType)}  NEED {num:0}";
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
			PlayStageDefeatFeedback();
			CompleteStage();
			return true;
		}

		private void PlayStageDefeatFeedback()
		{
			bool bossRelated = GetLastRunFailureBucket() == "BOSS PHASE" || (stageBossEncounterActive && IsStageBossAlive());
			Vector3 center = playerTransform != null ? playerTransform.position : Vector3.zero;
			if (bossRelated && stageBossBlock != null)
			{
				center = stageBossBlock.transform.position;
			}
			feedbackSystem?.PlayFailureBeatFeedback(center + Vector3.up * 0.25f, bossRelated, bossRelated ? 1f : 0.82f);
			FailureBeatVfx.Play(center + Vector3.up * 0.25f, bossRelated);
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

