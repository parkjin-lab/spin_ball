using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private enum AnnouncementTone
		{
			Burst,
			Milestone,
			Destruction,
			BossMinor,
			BossMajor
		}

		private float GetAnnouncementDuration(AnnouncementTone tone, float baseDuration = 0f)
		{
			float num = (baseDuration > 0f) ? baseDuration : comboRushAnnouncementSeconds;
			switch (tone)
			{
			case AnnouncementTone.Destruction:
				return Mathf.Clamp(num, 1.08f, 1.45f);
			case AnnouncementTone.BossMinor:
				return Mathf.Clamp(num, 0.88f, 1.15f);
			case AnnouncementTone.BossMajor:
				return Mathf.Clamp(num, 1.2f, 1.75f);
			case AnnouncementTone.Milestone:
				return Mathf.Clamp(num, 0.78f, 1.02f);
			default:
				return Mathf.Clamp(num, 0.72f, 0.96f);
			}
		}

		private static int GetAnnouncementPriority(AnnouncementTone tone)
		{
			switch (tone)
			{
			case AnnouncementTone.BossMajor:
				return 4;
			case AnnouncementTone.Destruction:
				return 3;
			case AnnouncementTone.BossMinor:
				return 2;
			case AnnouncementTone.Milestone:
				return 1;
			default:
				return 0;
			}
		}

		private void PushAnnouncement(string text, AnnouncementTone tone, float baseDuration = 0f)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			float announcementDuration = GetAnnouncementDuration(tone, baseDuration);
			int announcementPriority = GetAnnouncementPriority(tone);
			bool hasActiveAnnouncement = !string.IsNullOrEmpty(comboRushAnnouncementText) && comboRushAnnouncementRemaining > 0.001f;
			if (!hasActiveAnnouncement)
			{
				comboRushAnnouncementText = text;
				comboRushAnnouncementRemaining = announcementDuration;
				comboRushAnnouncementPriority = announcementPriority;
				comboRushAnnouncementCooldown = Mathf.Min(0.24f, announcementDuration * 0.3f);
				return;
			}
			bool sameText = string.Equals(comboRushAnnouncementText, text, System.StringComparison.Ordinal);
			if (sameText)
			{
				comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining, announcementDuration);
				comboRushAnnouncementPriority = Mathf.Max(comboRushAnnouncementPriority, announcementPriority);
				return;
			}
			bool canOverride = announcementPriority > comboRushAnnouncementPriority
				|| comboRushAnnouncementRemaining <= 0.2f
				|| comboRushAnnouncementCooldown <= 0f;
			if (!canOverride)
			{
				bool samePending = string.Equals(pendingComboRushAnnouncementText, text, System.StringComparison.Ordinal);
				bool shouldQueue = samePending
					|| string.IsNullOrEmpty(pendingComboRushAnnouncementText)
					|| announcementPriority > pendingComboRushAnnouncementPriority
					|| (announcementPriority == pendingComboRushAnnouncementPriority && pendingComboRushAnnouncementRemaining <= announcementDuration * 0.85f);
				if (shouldQueue)
				{
					pendingComboRushAnnouncementText = text;
					pendingComboRushAnnouncementRemaining = announcementDuration;
					pendingComboRushAnnouncementPriority = announcementPriority;
				}
				return;
			}
			comboRushAnnouncementText = text;
			comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining * 0.22f, announcementDuration);
			comboRushAnnouncementPriority = announcementPriority;
			comboRushAnnouncementCooldown = Mathf.Min(0.28f, announcementDuration * 0.32f);
		}

		private void InitializeRoguelikeProgression()
		{
			currentLevel = 1;
			queuedLevelUps = 0;
			nextLevelDestroyedRequirement = GetDestroyedRequirementForLevel(1);
			RefreshDestructibleCache();
		}

		private void InitializeTotalDestructionState()
		{
			totalDestructionAchieved = false;
			totalDestructionBonusAwarded = 0;
			stageAdvanceGoalReached = false;
			stageAdvanceMilestoneIndex = 0;
			earlyCrushFlowBonusIndex = 0;
			earlyCrushRecoveryBonusGranted = false;
			routeHoldBonusGranted = false;
			stageAdvanceRouteGuidanceActive = false;
			stageAdvanceRouteRewardGranted = false;
			routeOpenBeatRemaining = 0f;
			activeStageAdvanceRouteMarker = null;
			forwardSmashTargetBlock = null;
			forwardSmashBonusPending = false;
			stageAdvanceDestroyTarget = 0;
			StopTotalDestructionCompletionRoutine();
			RefreshDestructibleCache();
			stageTotalDestructibleCount = 0;
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if (!((Object)(object)dummyDestructibleBlock == (Object)null) && ((Component)dummyDestructibleBlock).gameObject.activeInHierarchy)
				{
					stageTotalDestructibleCount++;
				}
			}
			stageAdvanceDestroyTarget = GetStageAdvanceDestroyTarget();
		}

		private void EvaluateTotalDestructionProgress()
		{
			if (enableTotalDestructionEvent && !totalDestructionAchieved && stageRunning && !((Object)(object)scoreSystem == (Object)null) && stageTotalDestructibleCount > 0 && scoreSystem.DestroyedCount >= stageTotalDestructibleCount)
			{
				TriggerTotalDestructionEvent();
			}
		}

		private void TriggerTotalDestructionEvent()
		{
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			totalDestructionAchieved = true;
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			int num2 = Mathf.RoundToInt(Mathf.Max(0f, remainingStageTime) * (float)Mathf.Max(0, totalDestructionTimeBonusPerSecond));
			int num3 = Mathf.Max(0, num - 1) * Mathf.Max(0, totalDestructionChainBonusStep);
			int num4 = (totalDestructionBonusAwarded = Mathf.Max(0, Mathf.Max(0, totalDestructionFlatBonusScore) + num2 + num3));
			if (num4 > 0)
			{
				scoreSystem?.AddScore(num4);
			}
			PushAnnouncement((num4 > 0) ? $"TOTAL DESTRUCTION +{num4}" : "TOTAL DESTRUCTION", AnnouncementTone.Destruction, totalDestructionAnnouncementSeconds);
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			feedbackSystem?.PlayTotalDestructionFeedback(val + Vector3.up * 0.25f, 1f);
			if (totalDestructionInstantClear)
			{
				RouteStageVictoryEnd(StageEndReason.TotalDestruction, val, StageVictoryFlow.TotalDestructionDelay);
			}
		}

		private void StopTotalDestructionCompletionRoutine()
		{
			if (totalDestructionCompleteRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(totalDestructionCompleteRoutine);
				totalDestructionCompleteRoutine = null;
			}
		}

		private void EvaluateLevelUpProgression()
		{
			if (stageRunning && !levelUpOpen && !((Object)(object)scoreSystem == (Object)null))
			{
				int num = Mathf.Max(0, scoreSystem.DestroyedCount);
				while (num >= nextLevelDestroyedRequirement)
				{
					queuedLevelUps++;
					currentLevel++;
					ballGrowthSystem?.RegisterLevelUpGrowth();
					nextLevelDestroyedRequirement += GetDestroyedRequirementForLevel(currentLevel);
				}
				TryOpenQueuedLevelUp();
			}
		}

		private void EvaluateStageAdvanceProgress()
		{
			if (!enableStageAdvanceGoal || !stageRunning || levelUpOpen || stageEndLocked || (Object)(object)scoreSystem == (Object)null)
			{
				return;
			}
			if (stageAdvanceDestroyTarget <= 0)
			{
				return;
			}
			int num = Mathf.Max(0, scoreSystem.DestroyedCount);
			EvaluateEarlyCrushFlowBonuses(num);
			EvaluateRouteHoldBonus(num);
			EvaluateStageAdvanceMilestones(num);
			if (stageAdvanceGoalReached)
			{
				return;
			}
			if (num < stageAdvanceDestroyTarget)
			{
				return;
			}
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				return;
			}
			stageAdvanceGoalReached = true;
			PushAnnouncement($"STAGE {currentStageNumber:00} CLEAR", AnnouncementTone.Milestone, stageAdvanceGoalAnnouncementSeconds);
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			feedbackSystem?.PlayCounterSurgeFeedback(val + Vector3.up * 0.25f, 0.86f, major: true);
			damageNumberSystem?.ShowTag(val + Vector3.up * 1.5f, "NEXT STAGE OPEN", true);
			RouteStageVictoryEnd(StageEndReason.GoalReached, val, StageVictoryFlow.StageFinale);
		}

		private void EvaluateEarlyCrushFlowBonuses(int destroyedCount)
		{
			if (!stageRunning || earlyCrushFlowBonusScore <= 0)
			{
				return;
			}
			float num = (((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f);
			float num2 = Mathf.Max(0f, num - remainingStageTime);
			if (num2 > GetEarlyCrushFlowWindowSeconds())
			{
				TryTriggerEarlyCrushRecoveryBonus(destroyedCount, num2);
				return;
			}
			int num3 = GetEarlyCrushBonusTier(destroyedCount);
			while (earlyCrushFlowBonusIndex < num3)
			{
				earlyCrushFlowBonusIndex++;
				TriggerEarlyCrushFlowBonus(earlyCrushFlowBonusIndex);
			}
		}

		private float GetEarlyCrushFlowWindowSeconds()
		{
			return Mathf.Max(4f, earlyCrushFlowWindowSeconds);
		}

		private int GetEarlyCrushFirstBreakTarget()
		{
			return Mathf.Max(1, earlyCrushFirstBreakTarget);
		}

		private int GetEarlyCrushStartTarget()
		{
			return Mathf.Max(GetEarlyCrushFirstBreakTarget() + 1, earlyCrushStartTarget);
		}

		private int GetEarlyCrushLaneBreakTarget()
		{
			return Mathf.Max(GetEarlyCrushStartTarget() + 1, earlyCrushLaneBreakTarget);
		}

		private int GetEarlyCrushBonusTier(int destroyedCount)
		{
			if (destroyedCount >= GetEarlyCrushLaneBreakTarget())
			{
				return 3;
			}
			if (destroyedCount >= GetEarlyCrushStartTarget())
			{
				return 2;
			}
			if (destroyedCount >= GetEarlyCrushFirstBreakTarget())
			{
				return 1;
			}
			return 0;
		}

		private void TryTriggerEarlyCrushRecoveryBonus(int destroyedCount, float elapsedSeconds)
		{
			if (earlyCrushRecoveryBonusGranted || earlyCrushFlowBonusIndex >= 3)
			{
				return;
			}
			float num = GetEarlyCrushFlowWindowSeconds() + Mathf.Max(0f, earlyCrushRecoveryWindowSeconds);
			if (elapsedSeconds > num || destroyedCount < GetEarlyCrushLaneBreakTarget())
			{
				return;
			}
			earlyCrushRecoveryBonusGranted = true;
			int num2 = Mathf.Max(0, Mathf.RoundToInt((float)earlyCrushFlowBonusScore * Mathf.Max(0f, earlyCrushRecoveryBonusScoreScale)));
			if (num2 > 0)
			{
				scoreSystem?.AddScore(num2);
			}
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			PushAnnouncement((num2 > 0) ? $"RECOVERY BREAK +{num2:0}" : "RECOVERY BREAK", AnnouncementTone.Burst, 0.82f);
			damageNumberSystem?.ShowTag(val + Vector3.up * 1.2f, "RECOVERY BREAK", false);
			ResolvePlayerController();
			cachedPlayerController?.ApplyCounterSurge(1.04f, 1.08f, 0.9f);
			feedbackSystem?.PlayComboRushFeedback(val + Vector3.up * 0.18f, 0.48f, 3.2f);
			cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
			cameraFollowSystem?.AddImpulse(0.24f);
		}
		private void TriggerEarlyCrushFlowBonus(int bonusIndex)
		{
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			int num = bonusIndex switch
			{
				3 => Mathf.Max(0, Mathf.RoundToInt((float)earlyCrushFlowBonusScore * 1.35f)),
				1 => Mathf.Max(0, Mathf.RoundToInt((float)earlyCrushFlowBonusScore * 0.7f)),
				2 => Mathf.Max(0, earlyCrushFlowBonusScore),
				_ => 0,
			};
			string text = bonusIndex switch
			{
				1 => "FIRST BREAK",
				2 => "CRUSH START",
				3 => "LANE BREAK -> ROUTE OPEN",
				_ => string.Empty,
			};
			if (num > 0)
			{
				scoreSystem?.AddScore(num);
			}
			if (!string.IsNullOrEmpty(text))
			{
				PushAnnouncement((num > 0) ? $"{text} +{num}" : text, AnnouncementTone.Burst, 0.78f);
				damageNumberSystem?.ShowTag(val + Vector3.up * 1.2f, bonusIndex >= 3 ? "ROUTE OPEN" : text, false);
				feedbackSystem?.PlayComboRushFeedback(val + Vector3.up * 0.18f, bonusIndex == 1 ? 0.46f : 0.58f, bonusIndex == 1 ? 2.8f : 3.6f);
				if (bonusIndex >= 3)
				{
					ActivateStageAdvanceRouteGuidance(resetReward: false);
					StartRouteOpenBeat();
					ResolvePlayerController();
					cachedPlayerController?.ApplyCounterSurge(1.08f, 1.12f, 1.15f);
					feedbackSystem?.PlayCounterSurgeFeedback(val + Vector3.up * 0.24f, 0.62f, major: false);
					cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
					cameraFollowSystem?.AddImpulse(0.32f);
				}
			}
		}

		private void EvaluateRouteHoldBonus(int destroyedCount)
		{
			if (routeHoldBonusGranted || stageAdvanceDestroyTarget <= 0 || earlyCrushFlowBonusIndex < 3 || earlyCrushRecoveryBonusGranted)
			{
				return;
			}
			if (GetRouteHoldRemainingSeconds() <= 0.001f)
			{
				return;
			}
			if (destroyedCount < GetRouteHoldTarget())
			{
				return;
			}
			routeHoldBonusGranted = true;
			int num4 = Mathf.Max(0, routeHoldBonusScore);
			if (num4 > 0)
			{
				scoreSystem?.AddScore(num4);
			}
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			PushAnnouncement((num4 > 0) ? $"ROUTE HOLD +{num4:0}" : "ROUTE HOLD", AnnouncementTone.Milestone, 0.88f);
			damageNumberSystem?.ShowTag(val + Vector3.up * 1.25f, "ROUTE HOLD", false);
			ResolvePlayerController();
			cachedPlayerController?.ApplyCounterSurge(1.05f, 1.1f, Mathf.Max(0.3f, routeHoldCounterDuration));
			feedbackSystem?.PlayCounterSurgeFeedback(val + Vector3.up * 0.2f, 0.58f, major: false);
			cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
			cameraFollowSystem?.AddImpulse(0.28f);
		}
		private void EvaluateStageAdvanceMilestones(int destroyedCount)
		{
			if (!enableStageAdvanceGoal || stageAdvanceDestroyTarget <= 0)
			{
				return;
			}
			float num = Mathf.Clamp01((float)Mathf.Max(0, destroyedCount) / (float)Mathf.Max(1, stageAdvanceDestroyTarget));
			int num2 = 0;
			float num3 = Mathf.Clamp(stageAdvanceNearCompleteThreshold, 0.6f, 0.98f);
			if (num >= num3)
			{
				num2 = 3;
			}
			else if (num >= 0.45f)
			{
				num2 = 2;
			}
			else if (num >= 0.25f)
			{
				num2 = 1;
			}
			while (stageAdvanceMilestoneIndex < num2)
			{
				stageAdvanceMilestoneIndex++;
				TriggerStageAdvanceMilestone(stageAdvanceMilestoneIndex, num);
			}
		}

		private void TriggerStageAdvanceMilestone(int milestoneIndex, float progress01)
		{
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			int num = 0;
			string text = string.Empty;
			switch (milestoneIndex)
			{
			case 1:
				num = Mathf.Max(0, Mathf.RoundToInt((float)stageAdvanceMilestoneScore * 0.65f));
				text = "CRUSH FLOW";
				feedbackSystem?.PlayComboRushFeedback(val + Vector3.up * 0.2f, 0.68f, 4.4f);
				break;
			case 2:
				num = Mathf.Max(0, stageAdvanceMilestoneScore);
				text = "CITY ROUTE OPEN";
				feedbackSystem?.PlayCounterSurgeFeedback(val + Vector3.up * 0.24f, 0.72f, major: false);
				break;
			case 3:
				num = Mathf.Max(0, Mathf.RoundToInt((float)stageAdvanceMilestoneScore * 1.35f));
				text = "FINAL PUSH";
				ActivateStageAdvanceRouteGuidance(resetReward: !stageAdvanceRouteRewardGranted);
				feedbackSystem?.PlayCounterSurgeFeedback(val + Vector3.up * 0.24f, 0.84f, major: true);
				break;
			}
			if (num > 0)
			{
				scoreSystem?.AddScore(num);
			}
			if (!string.IsNullOrEmpty(text))
			{
				PushAnnouncement((num > 0) ? $"{text} +{num}" : text, AnnouncementTone.Milestone, Mathf.Lerp(0.75f, 1.1f, progress01));
				damageNumberSystem?.ShowTag(val + Vector3.up * 1.35f, text, milestoneIndex >= 2);
			}
		}

		private void ActivateStageAdvanceRouteGuidance(bool resetReward = true)
		{
			ResolveStageAdvanceRouteMarkers();
			Transform previousMarker = activeStageAdvanceRouteMarker;
			Transform selectedMarker = SelectStageAdvanceRouteMarker();
			activeStageAdvanceRouteMarker = selectedMarker;
			stageAdvanceRouteGuidanceActive = (Object)(object)activeStageAdvanceRouteMarker != (Object)null;
			if (resetReward && (Object)(object)selectedMarker != (Object)(object)previousMarker)
			{
				stageAdvanceRouteRewardGranted = false;
			}
		}

		private void StartRouteOpenBeat()
		{
			routeOpenBeatRemaining = Mathf.Max(routeOpenBeatRemaining, Mathf.Max(0.1f, routeOpenBeatSeconds));
		}

		private bool IsRouteOpenBeatActive()
		{
			return routeOpenBeatRemaining > 0.001f && stageAdvanceRouteGuidanceActive && (Object)(object)activeStageAdvanceRouteMarker != (Object)null;
		}

		private float GetRouteOpenBeat01()
		{
			return Mathf.Clamp01(routeOpenBeatRemaining / Mathf.Max(0.1f, routeOpenBeatSeconds));
		}

		private void ResolveStageAdvanceRouteMarkers()
		{
			if ((Object)(object)stageAdvanceRouteMarkerA != (Object)null && (Object)(object)stageAdvanceRouteMarkerB != (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(null, "MapRoot");
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "TargetMarkers");
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = val;
			}
			stageAdvanceRouteMarkerA = FindChildByName(val2, "Target_A");
			stageAdvanceRouteMarkerB = FindChildByName(val2, "Target_B");
		}

		private Transform SelectStageAdvanceRouteMarker()
		{
			Transform[] array = new Transform[2] { stageAdvanceRouteMarkerA, stageAdvanceRouteMarkerB };
			Vector3 zero = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			Transform val = null;
			float num = float.MinValue;
			for (int i = 0; i < array.Length; i++)
			{
				Transform val2 = array[i];
				if ((Object)(object)val2 == (Object)null || !((Component)val2).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 position = val2.position;
				float num2 = position.z - zero.z;
				float num3 = Mathf.Abs(position.x - zero.x);
				float num4 = num2 * 1.8f - num3 * 0.12f;
				if (num4 > num)
				{
					num = num4;
					val = val2;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				val = (((Object)(object)stageAdvanceRouteMarkerA != (Object)null) ? stageAdvanceRouteMarkerA : stageAdvanceRouteMarkerB);
			}
			return val;
		}

		private void EvaluateStageAdvanceRouteReward()
		{
			if (!stageRunning || !stageAdvanceRouteGuidanceActive || stageAdvanceRouteRewardGranted || (Object)(object)activeStageAdvanceRouteMarker == (Object)null || (Object)(object)playerTransform == (Object)null)
			{
				return;
			}
			Vector3 position = activeStageAdvanceRouteMarker.position;
			Vector3 position2 = playerTransform.position;
			float num = position.x - position2.x;
			float num2 = position.z - position2.z;
			float num3 = Mathf.Max(1.2f, stageAdvanceRouteRewardDistance);
			if (num * num + num2 * num2 > num3 * num3)
			{
				return;
			}
			stageAdvanceRouteRewardGranted = true;
			scoreSystem?.AddScore(Mathf.Max(0, stageAdvanceRouteRewardScore));
			ResolvePlayerController();
			cachedPlayerController?.ApplyCounterSurge(Mathf.Max(1f, stageAdvanceRouteRewardSpeedScale), Mathf.Max(1f, stageAdvanceRouteRewardImpactScale), Mathf.Max(0.2f, stageAdvanceRouteRewardCounterDuration));
			Vector3 val = position + Vector3.up * 0.28f;
			feedbackSystem?.PlayTotalDestructionFeedback(val, 0.8f);
			feedbackSystem?.PlayComboRushFeedback(val, 0.72f, 4.8f);
			cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
			cameraFollowSystem?.AddImpulse(Mathf.Max(0.1f, stageAdvanceRouteRewardCameraImpulse));
			damageNumberSystem?.ShowTag(val + Vector3.up * 1.05f, $"ROUTE BONUS +{Mathf.Max(0, stageAdvanceRouteRewardScore):0}", true);
			PushAnnouncement($"ROUTE BONUS +{Mathf.Max(0, stageAdvanceRouteRewardScore):0}", AnnouncementTone.Milestone, 1f);
			SpawnStageAdvanceRewardProps(activeStageAdvanceRouteMarker);
			PreviewStageAdvanceFollowupTarget(activeStageAdvanceRouteMarker);
		}

		private void SpawnStageAdvanceRewardProps(Transform marker)
		{
			if ((Object)(object)marker == (Object)null)
			{
				return;
			}
			Transform val = marker.parent;
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val.parent, "StreetProps");
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			Vector3 localPosition = marker.localPosition;
			EnsureExplosiveBarrelRuntime(val2, $"RouteRewardBarrel_{currentStageNumber:00}_A", localPosition + new Vector3(-1.15f, 0f, 1.25f), new Color(1f, 0.56f, 0.22f));
			EnsureExplosiveBarrelRuntime(val2, $"RouteRewardBarrel_{currentStageNumber:00}_B", localPosition + new Vector3(1.05f, 0f, 1.38f), new Color(1f, 0.46f, 0.18f));
			EnsureTransformerRuntime(val2, $"RouteRewardTransformer_{currentStageNumber:00}", localPosition + new Vector3(0f, 0f, 2.2f), new Color(1f, 0.82f, 0.36f));
		}

		private void PreviewStageAdvanceFollowupTarget(Transform marker)
		{
			if ((Object)(object)marker == (Object)null)
			{
				return;
			}
			RefreshDestructibleCache();
			Vector3 position = marker.position;
			DummyDestructibleBlock val = null;
			float num = float.MinValue;
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !dummyDestructibleBlock.IsAlive)
				{
					continue;
				}
				Vector3 position2 = ((Component)dummyDestructibleBlock).transform.position;
				float num2 = position2.z - position.z;
				if (num2 < 2.5f)
				{
					continue;
				}
				float num3 = Mathf.Abs(position2.x - position.x);
				float num4 = num2 * 1.35f - num3 * 0.28f + ((Component)dummyDestructibleBlock).transform.lossyScale.y * 0.45f;
				if (num4 > num)
				{
					num = num4;
					val = dummyDestructibleBlock;
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			forwardSmashTargetBlock = val;
			forwardSmashBonusPending = true;
			Vector3 val2 = ((Component)val).transform.position + Vector3.up * Mathf.Max(1f, ((Component)val).transform.lossyScale.y * 0.75f);
			damageNumberSystem?.ShowTag(val2, "BIGGER TARGET AHEAD", true);
			feedbackSystem?.PlayComboRushFeedback(((Component)val).transform.position + Vector3.up * 0.22f, 0.62f, 5.6f);
		}

		private void EvaluateForwardSmashBonus()
		{
			if (!stageRunning || !forwardSmashBonusPending || (Object)(object)forwardSmashTargetBlock == (Object)null)
			{
				return;
			}
			if (forwardSmashTargetBlock.IsAlive)
			{
				return;
			}
			forwardSmashBonusPending = false;
			Vector3 position = ((Component)forwardSmashTargetBlock).transform.position;
			int num = Mathf.Max(0, forwardSmashBonusScore);
			if (num > 0)
			{
				scoreSystem?.AddScore(num);
			}
			damageNumberSystem?.ShowTag(position + Vector3.up * 1.5f, $"FORWARD SMASH +{num:0}", true);
			PushAnnouncement($"FORWARD SMASH +{num:0}", AnnouncementTone.Destruction, 1.15f);
			feedbackSystem?.PlayTotalDestructionFeedback(position + Vector3.up * 0.26f, 0.92f);
			feedbackSystem?.PlayComboRushFeedback(position + Vector3.up * 0.18f, 0.88f, Mathf.Max(3.2f, forwardSmashBonusRadius));
			cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
			cameraFollowSystem?.AddImpulse(0.95f);
			TriggerForwardSmashChain(position);
			forwardSmashTargetBlock = null;
		}

		private void TriggerForwardSmashChain(Vector3 center)
		{
			RefreshDestructibleCache();
			float num = Mathf.Max(2.2f, forwardSmashBonusRadius);
			float num2 = num * num;
			float num3 = Mathf.Max(12f, forwardSmashBonusDamageRange.x);
			float num4 = Mathf.Max(num3 + 6f, forwardSmashBonusDamageRange.y);
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !dummyDestructibleBlock.IsAlive)
				{
					continue;
				}
				Vector3 position = ((Component)dummyDestructibleBlock).transform.position;
				float horizontalDistanceSq = GetHorizontalDistanceSq(position, center);
				if (horizontalDistanceSq > num2 || horizontalDistanceSq < 0.02f)
				{
					continue;
				}
				float num5 = Mathf.Sqrt(horizontalDistanceSq);
				float t = 1f - Mathf.Clamp01(num5 / Mathf.Max(0.01f, num));
				float damage = Mathf.Lerp(num3, num4, t);
				Vector3 hitPoint = position + Vector3.up * Mathf.Max(0.35f, ((Component)dummyDestructibleBlock).transform.lossyScale.y * 0.28f);
				dummyDestructibleBlock.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.55f, 1f, t), suppressFeedback: false, allowDestructionShockwave: true);
			}
		}

		private void BeginStageAdvanceFinale(Vector3 center)
		{
			if (stageAdvanceFinaleActive)
			{
				return;
			}
			if (!enableStageAdvanceFinale || stageAdvanceFinaleDuration <= 0.05f)
			{
				CompleteStage();
				return;
			}
			StopStageAdvanceFinaleRoutine();
			stageRunning = false;
			levelUpOpen = false;
			SetPaused(paused: false);
			ApplyUiViewState(UiViewState.None);
			stageAdvanceFinaleActive = true;
			stageAdvanceFinaleRoutine = ((MonoBehaviour)this).StartCoroutine(StageAdvanceFinaleRoutine(center));
		}

		private void StopStageAdvanceFinaleRoutine()
		{
			if (stageAdvanceFinaleRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(stageAdvanceFinaleRoutine);
				stageAdvanceFinaleRoutine = null;
			}
			stageAdvanceFinaleActive = false;
		}

		private System.Collections.IEnumerator StageAdvanceFinaleRoutine(Vector3 center)
		{
			float num = Mathf.Max(0f, stageAdvanceFinaleDelay);
			if (num > 0f)
			{
				yield return new WaitForSecondsRealtime(num);
			}
			cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
			cameraFollowSystem?.PlayFinishShot(center + Vector3.up * 0.45f, 0.8f);
			cameraFollowSystem?.AddImpulse(0.85f);
			feedbackSystem?.PlayTotalDestructionFeedback(center + Vector3.up * 0.24f, 0.74f);
			damageNumberSystem?.ShowTag(center + Vector3.up * 1.6f, "DISTRICT CRUSHED", true);
			RefreshDestructibleCache();
			float num2 = Mathf.Max(2.2f, stageAdvanceFinaleRadius);
			float num3 = num2 * num2;
			float num4 = Mathf.Max(12f, stageAdvanceFinaleDamageRange.x);
			float num5 = Mathf.Max(num4 + 8f, stageAdvanceFinaleDamageRange.y);
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !dummyDestructibleBlock.IsAlive)
				{
					continue;
				}
				Vector3 position = ((Component)dummyDestructibleBlock).transform.position;
				float horizontalDistanceSq = GetHorizontalDistanceSq(position, center);
				if (horizontalDistanceSq > num3)
				{
					continue;
				}
				float num6 = Mathf.Sqrt(horizontalDistanceSq);
				float t = 1f - Mathf.Clamp01(num6 / Mathf.Max(0.01f, num2));
				float damage = Mathf.Lerp(num4, num5, t);
				Vector3 hitPoint = position + Vector3.up * Mathf.Max(0.35f, ((Component)dummyDestructibleBlock).transform.lossyScale.y * 0.28f);
				dummyDestructibleBlock.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.52f, 0.92f, t), suppressFeedback: false, allowDestructionShockwave: true);
			}
			yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, stageAdvanceFinaleDuration));
			stageAdvanceFinaleRoutine = null;
			stageAdvanceFinaleActive = false;
			CompleteStage();
		}

		private int GetStageAdvanceDestroyTarget()
		{
			if (!enableStageAdvanceGoal)
			{
				return 0;
			}
			int num = Mathf.Max(1, currentStageNumber);
			int num2 = Mathf.Max(0, stageTotalDestructibleCount);
			float num3a = stageAdvanceTargetRatio;
			if (num <= 2)
			{
				num3a -= 0.11f;
			}
			else if (num == 3)
			{
				num3a -= 0.03f;
			}
			int num3 = Mathf.RoundToInt((float)num2 * Mathf.Clamp(num3a, 0.2f, 0.95f));
			int num4 = Mathf.Max(4, stageAdvanceBaseTarget + Mathf.Max(0, num - 1) * Mathf.Max(0, stageAdvanceTargetPerStage));
			if (num == 1)
			{
				num4 = Mathf.Max(4, num4 - 2);
			}
			else if (num == 2)
			{
				num4 = Mathf.Max(4, num4 - 1);
			}
			int num5 = Mathf.Max(num4, num3);
			if (stageBossEncounterActive)
			{
				num5 = Mathf.Max(num5, Mathf.RoundToInt((float)Mathf.Max(1, num2) * 0.44f));
			}
			return Mathf.Clamp(num5, 4, Mathf.Max(4, num2));
		}

		private int GetDestroyedRequirementForLevel(int fromLevel)
		{
			int num = Mathf.Max(0, fromLevel - 1);
			float num2 = (float)levelUpDestroyedBase * Mathf.Pow(Mathf.Max(1f, levelUpRequirementGrowth), (float)num);
			return Mathf.Max(1, Mathf.RoundToInt(num2));
		}

		private void TryOpenQueuedLevelUp()
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			if (stageRunning && !levelUpOpen && queuedLevelUps > 0)
			{
				queuedLevelUps--;
				levelUpOpen = true;
				levelUpRemaining = levelUpDurationSeconds;
				ClearLevelUpLockSelection();
				RollLevelUpChoices();
				ApplyUiViewState(UiViewState.LevelUp);
				UpdateLevelUpTimer();
				Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
				feedbackSystem?.PlayLevelUpOpenFeedback(val + Vector3.up * 0.2f);
			}
		}

		private void RefreshDestructibleCache()
		{
			destructibleCache.Clear();
			DummyDestructibleBlock[] array = Object.FindObjectsByType<DummyDestructibleBlock>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			foreach (DummyDestructibleBlock dummyDestructibleBlock in array)
			{
				if (!((Object)(object)dummyDestructibleBlock == (Object)null))
				{
					destructibleCache.Add(dummyDestructibleBlock);
				}
			}
		}

		private void ApplyLevelUpChoice(LevelUpChoice choice)
			{
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			ResolvePlayerController();
			switch (choice)
			{
			case LevelUpChoice.Speed:
				if ((Object)(object)cachedPlayerController != (Object)null)
				{
					cachedPlayerController.ApplySpeedBoost(0.1f);
					speedUpgradeCount++;
				}
				break;
			case LevelUpChoice.Impact:
				if ((Object)(object)cachedPlayerController != (Object)null)
				{
					cachedPlayerController.ApplyImpactBoost(0.2f);
					impactUpgradeCount++;
				}
				break;
			case LevelUpChoice.Drill:
				if ((Object)(object)cachedPlayerController != (Object)null)
				{
					cachedPlayerController.ApplyDrillMode();
					drillUpgradeCount++;
				}
				break;
			case LevelUpChoice.ComboPower:
				comboPowerUpgradeCount++;
				comboRushDamageRange = new Vector2(Mathf.Clamp(comboRushDamageRange.x + 8f, 16f, 260f), Mathf.Clamp(comboRushDamageRange.y + 22f, 36f, 420f));
				if (comboRushDamageRange.y < comboRushDamageRange.x + 10f)
				{
					comboRushDamageRange.y = comboRushDamageRange.x + 10f;
				}
				comboRushScoreBonus += 55;
				comboRushStreetBreakChance = Mathf.Clamp01(comboRushStreetBreakChance + 0.04f);
				comboRushAnnouncementSeconds = Mathf.Clamp(comboRushAnnouncementSeconds + 0.05f, 0.8f, 2.2f);
				break;
			case LevelUpChoice.ComboTrigger:
				comboTriggerUpgradeCount++;
				comboRushChainStep = Mathf.Max(3, comboRushChainStep - 1);
				comboRushRadius = Mathf.Clamp(comboRushRadius + 0.45f, 3.2f, 14f);
				comboRushRadiusPerLevel = Mathf.Clamp(comboRushRadiusPerLevel + 0.06f, 0.12f, 1.2f);
				comboRushDestructibleCap = Mathf.Clamp(comboRushDestructibleCap + 2, 6, 40);
				RecalculateNextComboRushGate();
				break;
				case LevelUpChoice.Shockwave:
					seismicUpgradeCount++;
					ApplySeismicShockwaveTuning();
					TriggerSeismicUpgradePulse();
					TriggerSeismicResonanceBurst(seismicUpgradeCount);
					break;
				case LevelUpChoice.TrafficPanic:
				{
					trafficPanicUpgradeCount++;
					float trafficPanicMasteryGainScale = GetTrafficPanicMasteryGainScale(trafficPanicUpgradeCount);
					trafficPanicBonusBaseScore += Mathf.Max(1, Mathf.RoundToInt((float)trafficPanicMasteryBonusBaseAdd * trafficPanicMasteryGainScale));
					trafficPanicBonusPerVehicle += Mathf.Max(1, Mathf.RoundToInt((float)trafficPanicMasteryBonusPerVehicleAdd * trafficPanicMasteryGainScale));
					trafficPanicRadius = Mathf.Clamp(trafficPanicRadius + trafficPanicMasteryRadiusAdd * trafficPanicMasteryGainScale, 3.5f, 40f);
					trafficPanicChainWindow = Mathf.Clamp(trafficPanicChainWindow + trafficPanicMasteryChainWindowAdd * trafficPanicMasteryGainScale, 1.2f, 12f);
					trafficPanicBonusCooldown = Mathf.Clamp(trafficPanicBonusCooldown - trafficPanicMasteryCooldownReduction * trafficPanicMasteryGainScale, 0.12f, 5f);
					trafficPanicJackpotScore += Mathf.Max(0, Mathf.RoundToInt((float)trafficPanicMasteryJackpotAdd * trafficPanicMasteryGainScale));
					trafficPanicCounterSurgePerChain = Mathf.Clamp(trafficPanicCounterSurgePerChain + trafficPanicMasteryCounterSurgePerChainAdd * trafficPanicMasteryGainScale, 0f, 0.8f);
					trafficPanicCounterSurgeDuration = Mathf.Clamp(trafficPanicCounterSurgeDuration + trafficPanicMasteryCounterSurgeDurationAdd * trafficPanicMasteryGainScale, 0.1f, 6f);
					int num = Mathf.Max(1, trafficPanicMasteryCounterSurgeThresholdStep);
					if (trafficPanicUpgradeCount % num == 0)
					{
						trafficPanicCounterSurgeMinChain = Mathf.Max(1, trafficPanicCounterSurgeMinChain - 1);
					}
					TriggerTrafficPanicMasteryPulse(trafficPanicUpgradeCount);
					break;
				}
				}
			}

		private void ResetUpgradeTracking()
		{
			speedUpgradeCount = 0;
			impactUpgradeCount = 0;
			drillUpgradeCount = 0;
			comboPowerUpgradeCount = 0;
			comboTriggerUpgradeCount = 0;
			seismicUpgradeCount = 0;
			trafficPanicUpgradeCount = 0;
			pendingTrafficPanicMasteryPulse = false;
			pendingTrafficPanicMasteryLevel = 0;
			pendingTrafficPanicMasteryGainScale = 1f;
			ApplySeismicShockwaveTuning();
			UpdateUpgradeListText();
		}

		private void ApplySeismicShockwaveTuning()
		{
			int num = Mathf.Max(0, seismicUpgradeCount);
			float radiusMultiplier = 1f + Mathf.Max(0f, seismicBuildingShockwaveRadiusBonusPerLevel) * (float)num;
			float damageMultiplier = 1f + Mathf.Max(0f, seismicBuildingShockwaveDamageBonusPerLevel) * (float)num;
			float propChanceBonus = Mathf.Max(0f, seismicBuildingShockwavePropChanceBonusPerLevel) * (float)num;
			int destructibleCapBonus = Mathf.Max(0, seismicBuildingShockwaveDestructibleCapBonusPerLevel) * num;
			int propCapBonus = Mathf.Max(0, seismicBuildingShockwavePropCapBonusPerLevel) * num;
			float impulseMultiplier = 1f + Mathf.Max(0f, seismicBuildingShockwaveImpulseBonusPerLevel) * (float)num;
			int bonusScoreFlatAdd = Mathf.Max(0, seismicBuildingShockwaveFlatScoreBonusPerLevel) * num;
			DummyDestructibleBlock.ConfigureRuntimeShockwaveTuning(radiusMultiplier, damageMultiplier, propChanceBonus, destructibleCapBonus, propCapBonus, impulseMultiplier, bonusScoreFlatAdd);
		}
	}
}



