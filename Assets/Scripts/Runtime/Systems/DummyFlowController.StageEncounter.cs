using System;
using System.Collections.Generic;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void ResetAllDestructibles()
		{
			RefreshDestructibleCache();
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if (!((Object)(object)dummyDestructibleBlock == (Object)null))
				{
					dummyDestructibleBlock.ApplyStageLayoutTuning(currentStageNumber);
				}
			}
		}

		private void ConfigureStageEncounter()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_033d: Unknown result type (might be due to invalid IL or missing references)
			RefreshDestructibleCache();
			stageBossBlock = null;
			stageBossEncounterActive = false;
			stageEliteWeakPointCount = 0;
			ResetStageBossTelemetry();
			if (destructibleCache.Count <= 0)
			{
				return;
			}
			List<DummyDestructibleBlock> list = new List<DummyDestructibleBlock>(destructibleCache.Count);
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null)
				{
					continue;
				}
				dummyDestructibleBlock.ApplyStageEncounterRole(DummyDestructibleBlock.StageEncounterRole.Standard);
				if (((Component)dummyDestructibleBlock).gameObject.activeInHierarchy && !((Object)dummyDestructibleBlock).name.StartsWith("Prop_", StringComparison.OrdinalIgnoreCase))
				{
					Vector3 lossyScale = ((Component)dummyDestructibleBlock).transform.lossyScale;
					float num = Mathf.Abs(lossyScale.x * lossyScale.z);
					if (!(Mathf.Abs(lossyScale.y) < 0.7f) || !(num < 1.4f))
					{
						list.Add(dummyDestructibleBlock);
					}
				}
			}
			if (list.Count <= 0)
			{
				return;
			}
			stageBossEncounterActive = enableStageBossEncounter && currentStageNumber >= Mathf.Max(2, bossStageStart);
			if (stageBossEncounterActive)
			{
				stageBossBlock = SelectStageBossCandidate(list);
				if ((Object)(object)stageBossBlock != (Object)null)
				{
					list.Remove(stageBossBlock);
					int num2 = Mathf.Max(0, currentStageNumber - Mathf.Max(2, bossStageStart));
					int bonusHp = Mathf.Max(0, bossHpBonusBase + num2 * Mathf.Max(0, bossHpBonusPerStage));
					stageBossBlock.ApplyStageEncounterRole(DummyDestructibleBlock.StageEncounterRole.BossSentinel, bonusHp, bossSentinelColor);
					ConfigureBossShieldPylons(list, num2);
				}
				else
				{
					stageBossEncounterActive = false;
				}
			}
			int num3 = eliteWeakPointBaseCount + Mathf.Max(0, currentStageNumber - 1) * Mathf.Max(0, eliteWeakPointPerStage);
			num3 = Mathf.Clamp(num3, 0, Mathf.Max(0, eliteWeakPointMaxCount));
			if (num3 <= 0 || list.Count <= 0)
			{
				return;
			}
			list.Sort((DummyDestructibleBlock a, DummyDestructibleBlock b) => GetDestructibleThreatScore(b).CompareTo(GetDestructibleThreatScore(a)));
			List<DummyDestructibleBlock> list2 = new List<DummyDestructibleBlock>(num3);
			float num4 = 36f;
			for (int j = 0; j < list.Count; j++)
			{
				if (list2.Count >= num3)
				{
					break;
				}
				DummyDestructibleBlock dummyDestructibleBlock2 = list[j];
				if ((Object)(object)dummyDestructibleBlock2 == (Object)null)
				{
					continue;
				}
				Vector3 position = ((Component)dummyDestructibleBlock2).transform.position;
				bool flag = false;
				for (int k = 0; k < list2.Count; k++)
				{
					if (GetHorizontalDistanceSq(position, ((Component)list2[k]).transform.position) < num4)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2.Add(dummyDestructibleBlock2);
				}
			}
			if (list2.Count < num3)
			{
				for (int l = 0; l < list.Count; l++)
				{
					if (list2.Count >= num3)
					{
						break;
					}
					DummyDestructibleBlock dummyDestructibleBlock3 = list[l];
					if (!((Object)(object)dummyDestructibleBlock3 == (Object)null) && !list2.Contains(dummyDestructibleBlock3))
					{
						list2.Add(dummyDestructibleBlock3);
					}
				}
			}
			int bonusHp2 = Mathf.Clamp(Mathf.FloorToInt((float)(currentStageNumber - 1) * 0.5f), 0, 8);
			stageEliteWeakPointCount = 0;
			for (int m = 0; m < list2.Count; m++)
			{
				DummyDestructibleBlock dummyDestructibleBlock4 = list2[m];
				if (!((Object)(object)dummyDestructibleBlock4 == (Object)null))
				{
					dummyDestructibleBlock4.ApplyStageEncounterRole(DummyDestructibleBlock.StageEncounterRole.EliteWeakPoint, bonusHp2, eliteWeakPointColor);
					stageEliteWeakPointCount++;
				}
			}
			string arg = (((Object)(object)stageBossBlock != (Object)null) ? ((Object)stageBossBlock).name : "none");
			UpdateBossShieldState();
			Debug.Log((object)$"[AlienCrusher] Stage {currentStageNumber:00} encounter set: elite weak-points {stageEliteWeakPointCount}, boss {arg}");
		}

		private DummyDestructibleBlock SelectStageBossCandidate(List<DummyDestructibleBlock> candidates)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if (candidates == null || candidates.Count <= 0)
			{
				return null;
			}
			Vector3 b = (Vector3)(((Object)(object)playerSpawn != (Object)null) ? playerSpawn.position : new Vector3(0f, 0f, -18f));
			DummyDestructibleBlock result = null;
			float num = float.MinValue;
			for (int i = 0; i < candidates.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = candidates[i];
				if (!((Object)(object)dummyDestructibleBlock == (Object)null))
				{
					Vector3 position = ((Component)dummyDestructibleBlock).transform.position;
					float num2 = Mathf.InverseLerp(-14f, 20f, position.z);
					float num3 = Mathf.Sqrt(GetHorizontalDistanceSq(position, b));
					float num4 = GetDestructibleThreatScore(dummyDestructibleBlock) + num2 * 4.8f + num3 * 0.06f;
					if (num4 > num)
					{
						num = num4;
						result = dummyDestructibleBlock;
					}
				}
			}
			return result;
		}

		private bool IsStageBossAlive()
		{
			if (stageBossEncounterActive && (Object)(object)stageBossBlock != (Object)null)
			{
				return ((Component)stageBossBlock).gameObject.activeInHierarchy;
			}
			return false;
		}

		private void ResetStageBossTelemetry()
		{
			stageBossIntroAnnounced = false;
			stageBossDestroyedAnnounced = false;
			stageBossShieldIntroAnnounced = false;
			stageBossPhaseTwoActive = false;
			stageBossPhaseTwoAnnounced = false;
			stageBossPhaseTwoDronePulseRemaining = Mathf.Max(0.8f, bossPhaseTwoDronePulseInterval * 0.7f);
			stageBossPhaseTwoDroneRecoveryRemaining = 0f;
			stageBossPhaseTwoDroneRespawnWarned = false;
			this.stageBossThreatLevel = 0;
			stageBossThreatPulseRemaining = Mathf.Max(1.2f, bossThreatPulseBaseInterval * 0.45f);
			stageBossPressurePulseRemaining = Mathf.Max(1.4f, bossPressurePulseInterval * 0.65f);
			stageBossShieldRegenRemaining = Mathf.Max(1f, bossShieldRegenInterval * 0.7f);
			stageBossBreakRemaining = 0f;
			stageBossShieldActiveCount = 0;
			pendingBossStageCompletion = false;
			bossClearCascadeActive = false;
			stageBossShieldPylons.Clear();
			SetBossPhaseTwoDronesActive(active: false);
			if ((Object)(object)stageBossBlock != (Object)null)
			{
				stageBossBlock.SetStageEncounterDamageScale(1f);
			}
		}

		private void UpdateStageBossTelemetry(float deltaTime)
		{
			if (!enableBossTelemetry || !stageBossEncounterActive)
			{
				return;
			}
			bool flag = IsStageBossAlive();
			if (!flag)
			{
				SetBossPhaseTwoDronesActive(active: false);
				stageBossBlock?.SetBossCoreExposure(false);
				if (!stageBossDestroyedAnnounced)
				{
					stageBossDestroyedAnnounced = true;
					this.stageBossThreatLevel = 0;
					PushAnnouncement("JUSTICE SENTINEL DOWN", AnnouncementTone.BossMajor, 1.45f);
					Vector3 val = (((Object)(object)stageBossBlock != (Object)null) ? ((Component)stageBossBlock).transform.position : (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero));
					damageNumberSystem?.ShowTag(val + Vector3.up * 1.8f, "SENTINEL DOWN", true);
					feedbackSystem?.PlayCounterSurgeFeedback(val + Vector3.up * 0.4f, 1f, major: true);
					cameraFollowSystem ??= Object.FindFirstObjectByType<CameraFollowSystem>();
					cameraFollowSystem?.PlayFinishShot(val + Vector3.up * 0.85f, 1f);
					TriggerBossFinishSlowMotion();
					RouteStageVictoryEnd(StageEndReason.BossDefeated, val, StageVictoryFlow.BossCascade);
				}
				return;
			}
			UpdateBossShieldState();
			UpdateBossShieldRegen(deltaTime);
			UpdateBossBreakWindow(deltaTime);
			UpdateBossPhaseTwoDrones(deltaTime);
			float stageBossDurabilityRatio = GetStageBossDurabilityRatio();
			UpdateBossPhaseTwoState(stageBossDurabilityRatio);
			int threatLevel = GetStageBossThreatLevel(stageBossDurabilityRatio);
			if (!stageBossIntroAnnounced)
			{
				stageBossIntroAnnounced = true;
				this.stageBossThreatLevel = threatLevel;
				stageBossThreatPulseRemaining = GetBossThreatPulseInterval(threatLevel);
				AnnounceBossThreat(threatLevel, intro: true);
				return;
			}
			if (threatLevel != this.stageBossThreatLevel)
			{
				this.stageBossThreatLevel = threatLevel;
				stageBossThreatPulseRemaining = Mathf.Min(stageBossThreatPulseRemaining, 0.4f);
				AnnounceBossThreat(threatLevel, intro: false);
			}
			stageBossThreatPulseRemaining -= Mathf.Max(0f, deltaTime);
			if (stageBossThreatPulseRemaining <= 0f)
			{
				stageBossThreatPulseRemaining = GetBossThreatPulseInterval(threatLevel);
				AnnounceBossThreat(threatLevel, intro: false);
			}
			UpdateStageBossPressurePulse(deltaTime, threatLevel);
		}

		private float GetStageBossDurabilityRatio()
		{
			if ((Object)(object)stageBossBlock == (Object)null)
			{
				return 0f;
			}
			return stageBossBlock.DurabilityRatio;
		}

		private int GetStageBossThreatLevel(float durabilityRatio)
		{
			if (!stageBossEncounterActive || !IsStageBossAlive())
			{
				return 0;
			}
			float num = Mathf.Clamp01(durabilityRatio);
			if (num <= Mathf.Clamp(bossThreatFuryHealthThreshold, 0.05f, 0.8f))
			{
				return 3;
			}
			if (num <= Mathf.Clamp(bossThreatLockHealthThreshold, 0.1f, 0.95f))
			{
				return 2;
			}
			return 1;
		}

		private float GetBossThreatPulseInterval(int threatLevel)
		{
			float num = Mathf.Max(2.5f, bossThreatPulseBaseInterval);
			float num2 = Mathf.Max(1.6f, bossThreatPulseFastInterval);
			switch (Mathf.Clamp(threatLevel, 1, 3))
			{
			case 3:
				return Mathf.Min(num, num2);
			case 2:
				return Mathf.Lerp(num, num2, 0.55f);
			default:
				return num;
			}
		}

		private string GetBossThreatLabel()
		{
			if (IsBossBreakWindowActive())
			{
				return "BROKEN";
			}
			if (stageBossPhaseTwoActive)
			{
				return "PHASE 2";
			}
			switch (Mathf.Clamp(this.stageBossThreatLevel, 1, 3))
			{
			case 3:
				return "FURY";
			case 2:
				return "LOCKED";
			default:
				return "SCANNING";
			}
		}

		private string GetBossTelemetryHint()
		{
			if (IsBossBreakWindowActive())
			{
				return $"CORE EXPOSED: unload everything for {Mathf.CeilToInt(stageBossBreakRemaining):0}s";
			}
			if (stageBossShieldActiveCount > 0)
			{
				return $"JUSTICE SENTINEL SHIELDED: break {stageBossShieldActiveCount:0} pylon(s) to expose the core";
			}
			if (stageBossPhaseTwoActive)
			{
				if (stageBossPhaseTwoDroneRecoveryRemaining > 0.001f)
				{
					if (stageBossPhaseTwoDroneRecoveryRemaining <= Mathf.Max(0.45f, bossPhaseTwoDroneRespawnWarningDuration))
					{
						return $"JUSTICE SENTINEL PHASE 2: drone swarm returning in {Mathf.CeilToInt(stageBossPhaseTwoDroneRecoveryRemaining):0}s";
					}
					return $"JUSTICE SENTINEL PHASE 2: drones down, push damage for {Mathf.CeilToInt(stageBossPhaseTwoDroneRecoveryRemaining):0}s";
				}
				return "JUSTICE SENTINEL PHASE 2: break drones to open a safe push window";
			}
			switch (Mathf.Clamp(this.stageBossThreatLevel, 1, 3))
			{
			case 3:
				return "JUSTICE SENTINEL FURY: avoid burst pulses and break the weak point now";
			case 2:
				return "JUSTICE SENTINEL LOCKED: circle wide between pulses, then drive through the weak point";
			default:
				return "JUSTICE SENTINEL SCANNING: watch scan pulses, build speed, then line up the weak point";
			}
		}

		private void ConfigureBossShieldPylons(List<DummyDestructibleBlock> candidates, int stageDepth)
		{
			stageBossShieldPylons.Clear();
			stageBossShieldActiveCount = 0;
			stageBossShieldIntroAnnounced = false;
			if (!enableBossShieldPylons || (Object)(object)stageBossBlock == (Object)null || candidates == null || candidates.Count <= 0)
			{
				stageBossBlock?.SetStageEncounterDamageScale(1f);
				return;
			}
			Vector3 position = ((Component)stageBossBlock).transform.position;
			float num = Mathf.Max(5f, bossShieldSearchRadius);
			float num2 = num * num;
			List<DummyDestructibleBlock> list = new List<DummyDestructibleBlock>(candidates.Count);
			for (int i = 0; i < candidates.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = candidates[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !((Component)dummyDestructibleBlock).gameObject.activeInHierarchy)
				{
					continue;
				}
				if (GetHorizontalDistanceSq(((Component)dummyDestructibleBlock).transform.position, position) <= num2)
				{
					list.Add(dummyDestructibleBlock);
				}
			}
			if (list.Count <= 0)
			{
				stageBossBlock.SetStageEncounterDamageScale(1f);
				return;
			}
			list.Sort((DummyDestructibleBlock a, DummyDestructibleBlock b) =>
			{
				float num3 = GetDestructibleThreatScore(b) - GetDestructibleThreatScore(a);
				if (Mathf.Abs(num3) > 0.01f)
				{
					return (num3 > 0f) ? 1 : (-1);
				}
				float horizontalDistanceSq = GetHorizontalDistanceSq(((Component)a).transform.position, position);
				float horizontalDistanceSq2 = GetHorizontalDistanceSq(((Component)b).transform.position, position);
				return horizontalDistanceSq.CompareTo(horizontalDistanceSq2);
			});
			int num4 = Mathf.Clamp(bossShieldPylonCount + Mathf.FloorToInt(stageDepth * 0.25f), 0, 4);
			for (int j = 0; j < list.Count && stageBossShieldPylons.Count < num4; j++)
			{
				DummyDestructibleBlock dummyDestructibleBlock2 = list[j];
				if (!stageBossShieldPylons.Contains(dummyDestructibleBlock2))
				{
					dummyDestructibleBlock2.ApplyStageEncounterRole(DummyDestructibleBlock.StageEncounterRole.EliteWeakPoint, Mathf.Max(0, bossShieldPylonBonusHp + stageDepth / 2), bossShieldPylonColor);
					stageBossShieldPylons.Add(dummyDestructibleBlock2);
				}
			}
			UpdateBossShieldState();
		}

		private void UpdateBossShieldState()
		{
			if ((Object)(object)stageBossBlock == (Object)null)
			{
				stageBossShieldActiveCount = 0;
				return;
			}
			int num = 0;
			for (int i = stageBossShieldPylons.Count - 1; i >= 0; i--)
			{
				DummyDestructibleBlock dummyDestructibleBlock = stageBossShieldPylons[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !dummyDestructibleBlock.IsAlive)
				{
					stageBossShieldPylons.RemoveAt(i);
				}
				else
				{
					num++;
				}
			}
			if (num > 0)
			{
				stageBossBlock.SetBossCoreExposure(false);
				stageBossBlock.SetStageEncounterDamageScale(Mathf.Clamp(bossShieldProtectedDamageScale, 0.05f, 1f));
				if (!stageBossShieldIntroAnnounced)
				{
					stageBossShieldIntroAnnounced = true;
					stageBossShieldRegenRemaining = Mathf.Min(stageBossShieldRegenRemaining, Mathf.Max(0.6f, bossShieldRegenInterval * 0.5f));
					PushAnnouncement($"SENTINEL SHIELD x{num}", AnnouncementTone.BossMajor, 1.2f);
					damageNumberSystem?.ShowTag(((Component)stageBossBlock).transform.position + Vector3.up * 2.1f, $"SHIELD x{num}", true);
				}
				else if (num != stageBossShieldActiveCount)
				{
					PushAnnouncement($"SHIELD PYLONS {num:0} LEFT", AnnouncementTone.BossMinor, 1f);
					damageNumberSystem?.ShowTag(((Component)stageBossBlock).transform.position + Vector3.up * 1.9f, $"PYLONS {num:0}", num <= 1);
					feedbackSystem?.PlayCounterSurgeFeedback(((Component)stageBossBlock).transform.position + Vector3.up * 0.35f, Mathf.InverseLerp(Mathf.Max(1, bossShieldPylonCount), 1f, num), num <= 1);
				}
			}
			else
			{
				stageBossBlock.SetBossCoreExposure(true, 1.25f);
				stageBossBlock.SetStageEncounterDamageScale(IsBossBreakWindowActive() ? Mathf.Max(1f, bossBreakDamageMultiplier) : 1f);
				if (stageBossShieldActiveCount > 0)
				{
					StartBossBreakWindow();
					PushAnnouncement("CORE EXPOSED", AnnouncementTone.BossMajor, 1.35f);
					damageNumberSystem?.ShowTag(((Component)stageBossBlock).transform.position + Vector3.up * 2f, "CORE EXPOSED", true);
					feedbackSystem?.PlayRetailFrenzyFeedback(((Component)stageBossBlock).transform.position + Vector3.up * 0.3f, 1f, 5.6f);
				}
			}
			stageBossShieldActiveCount = num;
		}

		private void UpdateBossShieldRegen(float deltaTime)
		{
			if (!enableBossShieldRegen || IsBossBreakWindowActive() || stageBossShieldActiveCount <= 0 || (Object)(object)stageBossBlock == (Object)null)
			{
				return;
			}
			stageBossBlock.SetBossCoreExposure(false);
			stageBossShieldRegenRemaining -= Mathf.Max(0f, deltaTime);
			if (stageBossShieldRegenRemaining > 0f)
			{
				return;
			}
			float num = stageBossPhaseTwoActive ? Mathf.Max(1f, bossShieldRegenInterval * 0.72f) : Mathf.Max(1f, bossShieldRegenInterval);
			stageBossShieldRegenRemaining = num;
			float num2 = Mathf.Clamp01((float)stageBossShieldActiveCount * Mathf.Clamp(bossShieldRegenPercentPerPylon, 0.01f, 0.2f));
			if (stageBossPhaseTwoActive)
			{
				num2 = Mathf.Clamp01(num2 * Mathf.Max(1f, bossPhaseTwoRegenMultiplier));
			}
			float num3 = Mathf.Max(18f, stageBossBlock.MaxDurability * num2);
			if (!stageBossBlock.RestoreDurability(num3))
			{
				return;
			}
			Vector3 position = ((Component)stageBossBlock).transform.position;
			if (damageNumberSystem == null)
			{
				damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
			}
			if ((Object)(object)damageNumberSystem != (Object)null)
			{
				string text = stageBossPhaseTwoActive ? $"PHASE 2 REGEN +{Mathf.RoundToInt(num3):0}" : $"CORE REGEN +{Mathf.RoundToInt(num3):0}";
				damageNumberSystem.ShowTag(position + Vector3.up * 1.9f, text, stageBossShieldActiveCount >= 2 || stageBossPhaseTwoActive);
			}
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				feedbackSystem.PlayCounterSurgeFeedback(position + Vector3.up * 0.28f, Mathf.Clamp01(0.42f + stageBossShieldActiveCount * 0.18f), major: stageBossShieldActiveCount >= 2);
			}
			PushAnnouncement(stageBossPhaseTwoActive ? $"PHASE 2 REGEN {stageBossShieldActiveCount:0} LINK" : $"CORE REGEN {stageBossShieldActiveCount:0} LINK", AnnouncementTone.BossMinor, 0.95f);
		}

		private void StartBossBreakWindow()
		{
			if (!enableBossBreakWindow || (Object)(object)stageBossBlock == (Object)null)
			{
				stageBossBreakRemaining = 0f;
				stageBossBlock?.SetStageEncounterDamageScale(1f);
				return;
			}
			float num = stageBossPhaseTwoActive ? Mathf.Clamp(bossBreakWindowDuration * Mathf.Clamp(bossPhaseTwoBreakDurationScale, 0.4f, 1f), 1f, bossBreakWindowDuration) : Mathf.Max(1f, bossBreakWindowDuration);
			stageBossBreakRemaining = num;
			stageBossPressurePulseRemaining = Mathf.Max(stageBossBreakRemaining, stageBossPressurePulseRemaining);
			stageBossShieldRegenRemaining = Mathf.Max(stageBossBreakRemaining, stageBossShieldRegenRemaining);
			stageBossBlock.SetBossCoreExposure(true, stageBossPhaseTwoActive ? 1.85f : 1.45f);
			stageBossBlock.SetStageEncounterDamageScale(Mathf.Max(1f, bossBreakDamageMultiplier));
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				feedbackSystem.PlayTotalDestructionFeedback(((Component)stageBossBlock).transform.position + Vector3.up * 0.3f, 0.82f);
			}
		}

		private void UpdateBossBreakWindow(float deltaTime)
		{
			if (!IsBossBreakWindowActive())
			{
				return;
			}
			stageBossBreakRemaining = Mathf.Max(0f, stageBossBreakRemaining - Mathf.Max(0f, deltaTime));
			if (stageBossBreakRemaining > 0f || (Object)(object)stageBossBlock == (Object)null)
			{
				stageBossBlock.SetBossCoreExposure(true, stageBossPhaseTwoActive ? 1.85f : 1.45f);
				return;
			}
			stageBossBlock.SetBossCoreExposure(false);
			stageBossBlock.SetStageEncounterDamageScale(stageBossShieldActiveCount > 0 ? Mathf.Clamp(bossShieldProtectedDamageScale, 0.05f, 1f) : 1f);
			PushAnnouncement("SENTINEL RECOVERS", AnnouncementTone.BossMinor, 1f);
			damageNumberSystem?.ShowTag(((Component)stageBossBlock).transform.position + Vector3.up * 1.85f, "RECOVERY", false);
		}

		private bool IsBossBreakWindowActive()
		{
			return stageBossBreakRemaining > 0.001f;
		}

		private void UpdateBossPhaseTwoState(float durabilityRatio)
		{
			if (!enableBossPhaseTwo || stageBossPhaseTwoActive || stageBossPhaseTwoAnnounced || (Object)(object)stageBossBlock == (Object)null)
			{
				return;
			}
			if (durabilityRatio > Mathf.Clamp(bossPhaseTwoHealthThreshold, 0.05f, 0.7f))
			{
				return;
			}
			stageBossPhaseTwoActive = true;
			stageBossPhaseTwoAnnounced = true;
			stageBossPressurePulseRemaining = Mathf.Min(stageBossPressurePulseRemaining, 0.55f);
			stageBossShieldRegenRemaining = Mathf.Min(stageBossShieldRegenRemaining, 1.2f);
			stageBossPhaseTwoDronePulseRemaining = Mathf.Min(stageBossPhaseTwoDronePulseRemaining, 0.9f);
			PushAnnouncement("JUSTICE SENTINEL PHASE 2", AnnouncementTone.BossMajor, 1.6f);
			Vector3 position = ((Component)stageBossBlock).transform.position;
			damageNumberSystem?.ShowTag(position + Vector3.up * 2.1f, "PHASE 2", true);
			feedbackSystem?.PlayTotalDestructionFeedback(position + Vector3.up * 0.35f, 0.92f);
			SetBossPhaseTwoDronesActive(active: true);
		}

		private void UpdateBossPhaseTwoDrones(float deltaTime)
		{
			if (!enableBossPhaseTwoDrones || !stageBossPhaseTwoActive || (Object)(object)stageBossBlock == (Object)null)
			{
				SetBossPhaseTwoDronesActive(active: false);
				return;
			}
			if (IsBossBreakWindowActive())
			{
				SetBossPhaseTwoDronesActive(active: false);
				return;
			}
			EnsureBossPhaseTwoDrones();
			if (stageBossPhaseTwoDroneRecoveryRemaining > 0f)
			{
				stageBossBlock.SetBossCoreExposure(true, 1.9f);
				stageBossPhaseTwoDroneRecoveryRemaining = Mathf.Max(0f, stageBossPhaseTwoDroneRecoveryRemaining - Mathf.Max(0f, deltaTime));
				UpdateBossPhaseTwoDroneRespawnPreview();
				if (stageBossPhaseTwoDroneRecoveryRemaining > 0f)
				{
					return;
				}
				RestoreBossPhaseTwoDrones();
				stageBossPhaseTwoDroneRespawnWarned = false;
				stageBossBlock.SetBossCoreExposure(false);
				PushAnnouncement("DRONE SWARM RESTORED", AnnouncementTone.BossMinor, 0.95f);
				damageNumberSystem?.ShowTag(((Component)stageBossBlock).transform.position + Vector3.up * 1.9f, "DRONES BACK", false);
			}
			stageBossBlock.SetBossCoreExposure(false);
			SetBossPhaseTwoDronesActive(active: true);
			Vector3 position = ((Component)stageBossBlock).transform.position;
			float num = Mathf.Max(1.8f, bossPhaseTwoDroneOrbitRadius);
			float num2 = Mathf.Max(1.2f, bossPhaseTwoDroneOrbitHeight);
			float num3 = Mathf.Max(12f, bossPhaseTwoDroneOrbitSpeed);
			int num4 = 0;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < stageBossPhaseTwoDrones.Count; i++)
			{
				Transform val = stageBossPhaseTwoDrones[i];
				BossPhaseTwoDroneDummy bossPhaseTwoDroneDummy = ((i < stageBossPhaseTwoDroneUnits.Count) ? stageBossPhaseTwoDroneUnits[i] : null);
				if ((Object)(object)val == (Object)null || bossPhaseTwoDroneDummy == null || !bossPhaseTwoDroneDummy.IsAlive)
				{
					continue;
				}
				float num5 = Time.time * num3 + (360f / Mathf.Max(1, stageBossPhaseTwoDrones.Count)) * i;
				Vector3 val2 = Quaternion.Euler(0f, num5, 0f) * Vector3.forward;
				val.position = position + val2 * num + Vector3.up * (num2 + Mathf.Sin(Time.time * 3.5f + i) * 0.18f);
				val.rotation = Quaternion.LookRotation((position + Vector3.up * 1.2f) - val.position, Vector3.up);
				zero += val.position;
				num4++;
			}
			if (num4 <= 0)
			{
				stageBossPhaseTwoDroneRecoveryRemaining = Mathf.Max(stageBossPhaseTwoDroneRecoveryRemaining, Mathf.Max(1.8f, bossPhaseTwoDroneRecoveryDuration));
				SetBossPhaseTwoDronesActive(active: false);
				return;
			}
			stageBossPhaseTwoDronePulseRemaining -= Mathf.Max(0f, deltaTime);
			if (stageBossPhaseTwoDronePulseRemaining > 0f)
			{
				return;
			}
			float num6 = Mathf.Clamp01((float)num4 / Mathf.Max(1f, bossPhaseTwoDroneCount));
			stageBossPhaseTwoDronePulseRemaining = Mathf.Max(0.85f, Mathf.Lerp(Mathf.Max(1.1f, bossPhaseTwoDronePulseInterval * 1.25f), Mathf.Max(0.7f, bossPhaseTwoDronePulseInterval), num6));
			FireBossPhaseTwoDronePulse(zero / (float)Mathf.Max(1, num4));
		}

		private void EnsureBossPhaseTwoDrones()
		{
			if ((Object)(object)stageBossPhaseTwoDroneRoot == (Object)null)
			{
				Transform val = FindChildByName(null, "_Gameplay");
				if ((Object)(object)val == (Object)null)
				{
					val = new GameObject("_Gameplay").transform;
				}
				stageBossPhaseTwoDroneRoot = GetOrCreateDirectChild(val, "BossPhaseTwoDrones");
			}
			int num = Mathf.Clamp(bossPhaseTwoDroneCount, 1, 4);
			for (int i = 0; i < num; i++)
			{
				string name = $"BossDrone_{i:00}";
				GameObject val2 = EnsurePrimitive(stageBossPhaseTwoDroneRoot, name, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.62f, 0.24f, 0.62f), bossPhaseTwoDroneColor);
				Transform orCreateDirectChild = GetOrCreateDirectChild(val2.transform, "Core");
				EnsurePrimitive(orCreateDirectChild, "Eye", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.32f, 0.32f, 0.32f), new Color(1f, 0.9f, 0.52f, 1f));
				Transform orCreateDirectChild2 = GetOrCreateDirectChild(val2.transform, "Wing_L");
				EnsurePrimitive(orCreateDirectChild2, "WingMesh", PrimitiveType.Cube, new Vector3(-0.56f, 0f, 0f), new Vector3(0.7f, 0.06f, 0.16f), new Color(0.22f, 0.26f, 0.34f, 1f));
				Transform orCreateDirectChild3 = GetOrCreateDirectChild(val2.transform, "Wing_R");
				EnsurePrimitive(orCreateDirectChild3, "WingMesh", PrimitiveType.Cube, new Vector3(0.56f, 0f, 0f), new Vector3(0.7f, 0.06f, 0.16f), new Color(0.22f, 0.26f, 0.34f, 1f));
				Collider component = val2.GetComponent<Collider>();
				if ((Object)(object)component != (Object)null)
				{
					component.isTrigger = false;
				}
				BossPhaseTwoDroneDummy component2 = val2.GetComponent<BossPhaseTwoDroneDummy>();
				if ((Object)(object)component2 == (Object)null)
				{
					component2 = val2.AddComponent<BossPhaseTwoDroneDummy>();
				}
				component2.Configure(bossPhaseTwoDroneColor, OnBossPhaseTwoDroneDestroyed);
				while (stageBossPhaseTwoDrones.Count <= i)
				{
					stageBossPhaseTwoDrones.Add(null);
				}
				while (stageBossPhaseTwoDroneUnits.Count <= i)
				{
					stageBossPhaseTwoDroneUnits.Add(null);
				}
				stageBossPhaseTwoDrones[i] = val2.transform;
				stageBossPhaseTwoDroneUnits[i] = component2;
			}
		}

		private void SetBossPhaseTwoDronesActive(bool active)
		{
			for (int i = 0; i < stageBossPhaseTwoDrones.Count; i++)
			{
				Transform val = stageBossPhaseTwoDrones[i];
				if (!((Object)(object)val == (Object)null))
				{
					BossPhaseTwoDroneDummy bossPhaseTwoDroneDummy = ((i < stageBossPhaseTwoDroneUnits.Count) ? stageBossPhaseTwoDroneUnits[i] : null);
					if (bossPhaseTwoDroneDummy != null)
					{
						bossPhaseTwoDroneDummy.SetDroneVisible(active);
					}
					else
					{
						((Component)val).gameObject.SetActive(active);
					}
				}
			}
		}

		private void UpdateBossPhaseTwoDroneRespawnPreview()
		{
			float num = Mathf.Max(0.45f, bossPhaseTwoDroneRespawnWarningDuration);
			bool flag = stageBossPhaseTwoDroneRecoveryRemaining <= num;
			float num2 = flag ? Mathf.Clamp01(1f - stageBossPhaseTwoDroneRecoveryRemaining / num) : 0f;
			for (int i = 0; i < stageBossPhaseTwoDroneUnits.Count; i++)
			{
				BossPhaseTwoDroneDummy bossPhaseTwoDroneDummy = stageBossPhaseTwoDroneUnits[i];
				if (bossPhaseTwoDroneDummy == null)
				{
					continue;
				}
				if (flag)
				{
					bossPhaseTwoDroneDummy.SetRespawnPreview(num2);
				}
				else
				{
					bossPhaseTwoDroneDummy.SetDroneVisible(false);
				}
			}
			if (flag && !stageBossPhaseTwoDroneRespawnWarned)
			{
				stageBossPhaseTwoDroneRespawnWarned = true;
				Vector3 position = ((Component)stageBossBlock).transform.position;
				PushAnnouncement("DRONE SWARM RETURNING", AnnouncementTone.BossMinor, 0.9f);
				damageNumberSystem?.ShowTag(position + Vector3.up * 1.9f, "DRONES RETURNING", false);
				feedbackSystem?.PlayCounterSurgeFeedback(position + Vector3.up * 0.3f, 0.46f, major: false);
			}
		}

		private void RestoreBossPhaseTwoDrones()
		{
			for (int i = 0; i < stageBossPhaseTwoDroneUnits.Count; i++)
			{
				BossPhaseTwoDroneDummy bossPhaseTwoDroneDummy = stageBossPhaseTwoDroneUnits[i];
				if (bossPhaseTwoDroneDummy != null)
				{
					bossPhaseTwoDroneDummy.Restore();
				}
			}
			stageBossPhaseTwoDronePulseRemaining = Mathf.Max(1.1f, bossPhaseTwoDronePulseInterval * 0.9f);
		}

		private void OnBossPhaseTwoDroneDestroyed(BossPhaseTwoDroneDummy drone)
		{
			if (drone == null)
			{
				return;
			}
			if ((Object)(object)scoreSystem != (Object)null)
			{
				scoreSystem.AddScore(Mathf.Max(0, bossPhaseTwoDroneScoreBonus));
			}
			ResolvePlayerController();
			if ((Object)(object)cachedPlayerController != (Object)null)
			{
				cachedPlayerController.ApplyCounterSurge(Mathf.Max(1f, bossPhaseTwoDroneCounterSurgeSpeedScale), Mathf.Max(1f, bossPhaseTwoDroneCounterSurgeImpactScale), Mathf.Max(0.2f, bossPhaseTwoDroneCounterSurgeDuration));
			}
			stageBossPhaseTwoDronePulseRemaining = Mathf.Max(stageBossPhaseTwoDronePulseRemaining, Mathf.Max(0.8f, bossPhaseTwoDronePulseDelayOnDestroy));
			int num = 0;
			for (int i = 0; i < stageBossPhaseTwoDroneUnits.Count; i++)
			{
				BossPhaseTwoDroneDummy bossPhaseTwoDroneDummy = stageBossPhaseTwoDroneUnits[i];
				if (bossPhaseTwoDroneDummy != null && bossPhaseTwoDroneDummy.IsAlive)
				{
					num++;
				}
			}
			Vector3 position = drone.transform.position;
			if (num <= 0)
			{
				stageBossPhaseTwoDroneRecoveryRemaining = Mathf.Max(1.8f, bossPhaseTwoDroneRecoveryDuration);
				stageBossPhaseTwoDroneRespawnWarned = false;
				stageBossBlock?.SetBossCoreExposure(true, 1.9f);
				PushAnnouncement("DRONE SWARM BROKEN", AnnouncementTone.BossMajor, 1.25f);
				damageNumberSystem?.ShowTag(position + Vector3.up * 1.2f, $"WINDOW {Mathf.CeilToInt(stageBossPhaseTwoDroneRecoveryRemaining):0}s", true);
				feedbackSystem?.PlayDroneBreakFeedback(position + Vector3.up * 0.18f, 0.95f, swarmBroken: true);
				feedbackSystem?.PlayTotalDestructionFeedback(position + Vector3.up * 0.2f, 0.72f);
				return;
			}
			PushAnnouncement($"DRONE DOWN {num:0} LEFT", AnnouncementTone.BossMinor, 0.9f);
			damageNumberSystem?.ShowTag(position + Vector3.up * 1f, $"{num:0} LEFT", num <= 1);
			feedbackSystem?.PlayDroneBreakFeedback(position + Vector3.up * 0.12f, Mathf.InverseLerp(Mathf.Max(1, bossPhaseTwoDroneCount), 1f, num), swarmBroken: false);
		}

		private void FireBossPhaseTwoDronePulse(Vector3 bossPosition)
		{
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			float num = Mathf.Max(3.2f, bossPhaseTwoDronePulseRadius);
			bool flag = false;
			if ((Object)(object)playerTransform != (Object)null)
			{
				Vector3 val = playerTransform.position - bossPosition;
				val.y = 0f;
				if (val.sqrMagnitude <= num * num)
				{
					ResolvePlayerController();
					cachedPlayerController?.ApplyExternalSlow(Mathf.Clamp(bossPhaseTwoDroneSlowScale, 0.2f, 1f), Mathf.Max(0.15f, bossPhaseTwoDroneSlowDuration));
					flag = true;
				}
			}
			ApplyTrafficPanic(bossPosition, num, 0.58f);
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				feedbackSystem.PlayCounterSurgeFeedback(bossPosition + Vector3.up * 0.45f, 0.72f, major: false);
			}
			if ((Object)(object)damageNumberSystem != (Object)null)
			{
				damageNumberSystem.ShowTag(bossPosition + Vector3.up * 2f, flag ? "DRONE JAM" : "DRONE SWEEP", flag);
			}
			PushAnnouncement(flag ? "PHASE 2 DRONE JAM" : "PHASE 2 DRONE SWEEP", AnnouncementTone.BossMinor, 0.82f);
		}

		private void UpdateStageBossPressurePulse(float deltaTime, int threatLevel)
		{
			if (!enableBossPressurePulse || IsBossBreakWindowActive() || levelUpOpen || !stageRunning || (Object)(object)stageBossBlock == (Object)null)
			{
				return;
			}
			stageBossPressurePulseRemaining -= Mathf.Max(0f, deltaTime);
			if (stageBossPressurePulseRemaining > 0f)
			{
				return;
			}
			stageBossPressurePulseRemaining = GetBossPressurePulseInterval(threatLevel);
			ExecuteStageBossPressurePulse(threatLevel);
		}

		private float GetBossPressurePulseInterval(int threatLevel)
		{
			float num = Mathf.Max(2.8f, bossPressurePulseInterval);
			if (stageBossPhaseTwoActive)
			{
				num = Mathf.Max(1.5f, num / Mathf.Max(1f, bossPhaseTwoPressureMultiplier));
			}
			switch (Mathf.Clamp(threatLevel, 1, 3))
			{
			case 3:
				return Mathf.Max(1.8f, num * 0.48f);
			case 2:
				return Mathf.Max(2.2f, num * 0.72f);
			default:
				return num;
			}
		}

		private void ExecuteStageBossPressurePulse(int threatLevel)
		{
			if ((Object)(object)stageBossBlock == (Object)null)
			{
				return;
			}
			Vector3 position = ((Component)stageBossBlock).transform.position;
			float num = Mathf.Clamp01((float)(Mathf.Clamp(threatLevel, 1, 3) - 1) / 2f);
			float num2 = Mathf.Max(3.5f, bossPressurePulseRadius + bossPressurePulseRadiusPerThreat * (float)Mathf.Max(0, threatLevel - 1));
			float num3 = Mathf.Max(0.3f, bossPressureSlowDuration * Mathf.Lerp(0.9f, 1.4f, num));
			float num4 = Mathf.Clamp(Mathf.Lerp(1f, Mathf.Clamp(bossPressureSlowScale, 0.2f, 1f), Mathf.Lerp(0.55f, 1f, num)), 0.2f, 1f);
			float num5 = Mathf.Max(0f, bossPressurePushForce * Mathf.Lerp(0.75f, 1.2f, num));
			float num6 = Mathf.Clamp01(bossPressurePropBreakChance + num * 0.22f);
			ResolvePlayerController();
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform != (Object)null)
			{
				Vector3 val = playerTransform.position - position;
				val.y = 0f;
				float magnitude = val.magnitude;
				if (magnitude <= num2)
				{
					cachedPlayerController?.ApplyExternalSlow(num4, num3);
					if ((Object)(object)playerBody != (Object)null)
					{
						Vector3 val2 = ((magnitude > 0.001f) ? (val / magnitude) : Vector3.forward);
						float num7 = 1f - Mathf.Clamp01(magnitude / num2);
						playerBody.AddForce((val2 + Vector3.up * 0.2f) * (num5 * Mathf.Lerp(0.7f, 1.25f, num7)), ForceMode.VelocityChange);
					}
				}
			}
			ApplyTrafficPanic(position, num2 * Mathf.Lerp(1.05f, 1.25f, num), Mathf.Lerp(0.48f, 0.95f, num));
			if (threatLevel >= 2)
			{
				ApplyAreaBreakToStreetProps(position, num2 * 0.82f, num6, drillMode: false);
			}
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				feedbackSystem.PlayCounterSurgeFeedback(position + Vector3.up * 0.28f, Mathf.Lerp(0.55f, 1f, num), threatLevel >= 3);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(Mathf.Lerp(0.34f, 0.82f, num));
			if (damageNumberSystem == null)
			{
				damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
			}
			if ((Object)(object)damageNumberSystem != (Object)null)
			{
				damageNumberSystem.ShowTag(position + Vector3.up * 1.5f, GetBossPressurePulseTag(threatLevel), threatLevel >= 2);
			}
			comboRushAnnouncementText = GetBossPressurePulseAnnouncement(threatLevel);
			comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining, 0.9f);
		}

		private void TriggerBossFinishSlowMotion()
		{
			if (!enableBossFinishSlowMotion || stagePaused)
			{
				return;
			}
			if (bossFinishSlowMotionRoutine != null)
			{
				StopCoroutine(bossFinishSlowMotionRoutine);
				bossFinishSlowMotionRoutine = null;
			}
			bossFinishSlowMotionRoutine = StartCoroutine(BossFinishSlowMotionRoutine());
		}

		private System.Collections.IEnumerator BossFinishSlowMotionRoutine()
		{
			float originalTimeScale = Mathf.Max(0.0001f, Time.timeScale);
			float originalFixedDelta = Time.fixedDeltaTime;
			float targetScale = Mathf.Clamp(bossFinishSlowTimeScale, 0.45f, 1f);
			float duration = Mathf.Max(0.05f, bossFinishSlowDuration);
			Time.timeScale = Mathf.Min(originalTimeScale, targetScale);
			Time.fixedDeltaTime = originalFixedDelta * (Time.timeScale / originalTimeScale);
			yield return new WaitForSecondsRealtime(duration);
			if (!stagePaused)
			{
				Time.timeScale = originalTimeScale;
				Time.fixedDeltaTime = originalFixedDelta;
			}
			bossFinishSlowMotionRoutine = null;
		}

		private void BeginBossClearCascade(Vector3 center)
		{
			if (pendingBossStageCompletion || bossClearCascadeActive)
			{
				return;
			}
			pendingBossStageCompletion = true;
			stageRunning = false;
			levelUpOpen = false;
			SetPaused(paused: false);
			ApplyUiViewState(UiViewState.None);
			if (enableBossClearCascade && bossClearCascadeDuration > 0.05f)
			{
				StopBossClearCascadeRoutine();
				pendingBossStageCompletion = true;
				bossClearCascadeActive = true;
				bossClearCascadeRoutine = StartCoroutine(BossClearCascadeRoutine(center));
			}
			else
			{
				CompleteStage();
			}
		}

		private void StopBossClearCascadeRoutine()
		{
			if (bossClearCascadeRoutine != null)
			{
				StopCoroutine(bossClearCascadeRoutine);
				bossClearCascadeRoutine = null;
			}
			bossClearCascadeActive = false;
			pendingBossStageCompletion = false;
		}

		private System.Collections.IEnumerator BossClearCascadeRoutine(Vector3 center)
		{
			float delay = Mathf.Max(0f, bossClearCascadeDelay);
			if (delay > 0f)
			{
				yield return new WaitForSecondsRealtime(delay);
			}
			RefreshDestructibleCache();
			List<DummyDestructibleBlock> cascadeTargets = new List<DummyDestructibleBlock>(destructibleCache.Count);
			float radius = Mathf.Max(8f, bossClearCascadeRadius);
			float radiusSqr = radius * radius;
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock block = destructibleCache[i];
				if ((Object)(object)block == (Object)null || block == stageBossBlock || !block.IsAlive)
				{
					continue;
				}
				float distanceSqr = GetHorizontalDistanceSq(((Component)block).transform.position, center);
				if (distanceSqr <= radiusSqr)
				{
					cascadeTargets.Add(block);
				}
			}
			cascadeTargets.Sort((DummyDestructibleBlock a, DummyDestructibleBlock b) => GetHorizontalDistanceSq(((Component)a).transform.position, center).CompareTo(GetHorizontalDistanceSq(((Component)b).transform.position, center)));
			int targetCount = Mathf.Min(cascadeTargets.Count, Mathf.Max(1, bossClearCascadeMaxTargets));
			float duration = Mathf.Max(0.2f, bossClearCascadeDuration);
			float minDamage = Mathf.Max(16f, bossClearCascadeDamageRange.x);
			float maxDamage = Mathf.Max(minDamage + 8f, bossClearCascadeDamageRange.y);
			float feedbackRadius = Mathf.Max(1.5f, bossClearCascadeFeedbackRadius);
			for (int i = 0; i < targetCount; i++)
			{
				DummyDestructibleBlock block2 = cascadeTargets[i];
				if ((Object)(object)block2 == (Object)null || !block2.IsAlive)
				{
					continue;
				}
				float normalized = (targetCount <= 1) ? 1f : (float)i / (float)(targetCount - 1);
				float damage = Mathf.Lerp(maxDamage, minDamage, normalized);
				Vector3 position = ((Component)block2).transform.position;
				Vector3 hitPoint = position + Vector3.up * Mathf.Max(0.4f, ((Component)block2).transform.lossyScale.y * 0.35f);
				block2.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Clamp01(bossClearCascadeImpact - normalized * 0.18f), suppressFeedback: false, allowDestructionShockwave: true);
				if ((Object)(object)feedbackSystem != (Object)null)
				{
					float intensity = Mathf.Clamp01(0.74f - normalized * 0.18f);
					feedbackSystem.PlayComboRushFeedback(hitPoint, intensity, feedbackRadius + normalized * 2.2f);
				}
				if ((Object)(object)damageNumberSystem != (Object)null && i == 0)
				{
					damageNumberSystem.ShowTag(hitPoint + Vector3.up * 0.8f, "CITY COLLAPSE", true);
				}
				float wait = duration / Mathf.Max(1, targetCount);
				if (wait > 0.01f)
				{
					yield return new WaitForSecondsRealtime(wait);
				}
			}
			bossClearCascadeRoutine = null;
			bossClearCascadeActive = false;
			pendingBossStageCompletion = false;
			CompleteStage();
		}

		private static string GetBossPressurePulseTag(int threatLevel)
		{
			switch (Mathf.Clamp(threatLevel, 1, 3))
			{
			case 3:
				return "FURY BURST";
			case 2:
				return "LOCKDOWN PULSE";
			default:
				return "SCAN PULSE";
			}
		}

		private static string GetBossPressurePulseAnnouncement(int threatLevel)
		{
			switch (Mathf.Clamp(threatLevel, 1, 3))
			{
			case 3:
				return "JUSTICE SENTINEL FURY BURST";
			case 2:
				return "JUSTICE SENTINEL LOCKDOWN";
			default:
				return "JUSTICE SENTINEL SCAN PULSE";
			}
		}

		private void AnnounceBossThreat(int threatLevel, bool intro)
		{
			if ((Object)(object)stageBossBlock == (Object)null)
			{
				return;
			}
			Vector3 position = ((Component)stageBossBlock).transform.position;
			string text = GetBossThreatLabel();
			if (intro)
			{
				comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining, 1.6f);
				comboRushAnnouncementText = "JUSTICE SENTINEL INBOUND";
				damageNumberSystem?.ShowTag(position + Vector3.up * 1.9f, "JUSTICE SENTINEL", true);
				feedbackSystem?.PlayStageStartFeedback(position + Vector3.up * 0.35f);
				return;
			}
			string text2 = $"SENTINEL {text}";
			comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining, 1.1f);
			comboRushAnnouncementText = text2;
			damageNumberSystem?.ShowTag(position + Vector3.up * 1.65f, text2, threatLevel >= 2);
			feedbackSystem?.PlayCounterSurgeFeedback(position + Vector3.up * 0.3f, Mathf.InverseLerp(1f, 3f, threatLevel), threatLevel >= 3);
		}

		private static float GetDestructibleThreatScore(DummyDestructibleBlock block)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)block == (Object)null)
			{
				return 0f;
			}
			Vector3 lossyScale = ((Component)block).transform.lossyScale;
			float num = Mathf.Max(0.05f, Mathf.Abs(lossyScale.x));
			float num2 = Mathf.Max(0.05f, Mathf.Abs(lossyScale.y));
			float num3 = Mathf.Max(0.05f, Mathf.Abs(lossyScale.z));
			float num4 = num * num2 * num3;
			return num2 * 2.2f + (num + num3) * 0.7f + Mathf.Sqrt(num4) * 1.3f;
		}

		private static float GetHorizontalDistanceSq(Vector3 a, Vector3 b)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float num = a.x - b.x;
			float num2 = a.z - b.z;
			return num * num + num2 * num2;
		}

		private void ResetPlayerToSpawn()
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null || (Object)(object)playerSpawn == (Object)null)
			{
				CacheSceneReferences();
			}
			if (!((Object)(object)playerTransform == (Object)null) && !((Object)(object)playerSpawn == (Object)null))
			{
				playerTransform.position = playerSpawn.position;
				playerTransform.rotation = Quaternion.identity;
				if ((Object)(object)playerBody != (Object)null)
				{
					playerBody.linearVelocity = Vector3.zero;
					playerBody.angularVelocity = Vector3.zero;
				}
			}
		}

		private void RepairZeroScaleMapObjects()
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			Transform val = FindChildByName(null, "MapRoot");
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform[] componentsInChildren = ((Component)val).GetComponentsInChildren<Transform>(true);
			Vector3 one = default(Vector3);
			foreach (Transform val2 in componentsInChildren)
			{
				if ((Object)(object)val2 == (Object)null || (Object)(object)val2 == (Object)(object)val)
				{
					continue;
				}
				bool flag = IsGroundedMapObject(((Object)val2).name);
				Vector3 val3 = val2.localScale;
				bool flag2 = Mathf.Abs(val3.x) <= 0.001f || Mathf.Abs(val3.y) <= 0.001f || Mathf.Abs(val3.z) <= 0.001f;
				float num = (flag ? Mathf.Max(0.05f, val3.y * 0.5f) : val2.localPosition.y);
				bool flag3 = flag && Mathf.Abs(val2.localPosition.y - num) > 0.15f;
				if (!flag2 && !flag3)
				{
					continue;
				}
				if (flag2)
				{
					if (((Object)val2).name.StartsWith("Block_"))
					{
						float z = val2.localPosition.z;
						if (z < 3f)
						{
							one = new Vector3(1.6f, 1.4f, 1.6f);
						}
						else if (z < 12f)
						{
							one = new Vector3(2.7f, 3.1f, 2.7f);
						}
						else
						{
							one = new Vector3(4.2f, 4.5f, 4.2f);
						}
					}
					else if (((Object)val2).name.StartsWith("Prop_"))
					{
						one = new Vector3(0.9f, 0.9f, 0.9f);
					}
					else if (((Object)val2).name == "Ground")
					{
						one = new Vector3(50f, 1f, 50f);
					}
					else if (((Object)val2).name.StartsWith("Wall_"))
					{
						one = new Vector3(1f, 4f, 50f);
					}
					else
					{
						one = Vector3.one;
					}
					Vector3 val4 = SanitizeScale(one);
					val2.localScale = val4;
					val3 = val4;
				}
				if (flag)
				{
					Vector3 localPosition = val2.localPosition;
					localPosition.y = Mathf.Max(0.05f, val3.y * 0.5f);
					val2.localPosition = localPosition;
				}
			}
		}

		private static bool IsGroundedMapObject(string objectName)
		{
			if (string.IsNullOrWhiteSpace(objectName))
			{
				return false;
			}
			string text = objectName.ToLowerInvariant();
			if (text.Contains("heli") || text.Contains("drone") || text.Contains("flying") || text.Contains("ufo") || text.Contains("airborne") || text.StartsWith("air_") || text.EndsWith("_air"))
			{
				return false;
			}
			if (!text.StartsWith("block_") && !text.StartsWith("prop_") && !text.Contains("building") && !text.Contains("house"))
			{
				return text.Contains("tower");
			}
			return true;
		}
	}
}
