using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void ResetTrafficPanicRuntime()
		{
			trafficPanicBonusCooldownRemaining = 0f;
			trafficPanicChainRemaining = 0f;
			trafficPanicChainStack = 0;
			trafficPanicJackpotTriggered = false;
			pendingTrafficPanicMasteryPulse = false;
			pendingTrafficPanicMasteryLevel = 0;
			pendingTrafficPanicMasteryGainScale = 1f;
			for (int i = 0; i < trafficVehicles.Count; i++)
			{
				TrafficVehicleState trafficVehicleState = trafficVehicles[i];
				if (trafficVehicleState != null)
				{
					trafficVehicleState.PanicSlowTimer = 0f;
					trafficVehicleState.PanicSpeedScale = 1f;
				}
			}
		}

		private float GetTrafficPanicMasteryGainScale(int masteryLevel)
		{
			if (masteryLevel <= 1)
			{
				return 1f;
			}
			float num = Mathf.Clamp01(trafficPanicMasteryGainFalloff);
			float num2 = Mathf.Clamp(trafficPanicMasteryMinGainScale, 0.2f, 1f);
			float num3 = Mathf.Pow(Mathf.Clamp01(1f - num), Mathf.Max(0, masteryLevel - 1));
			return Mathf.Clamp(num3, num2, 1f);
		}

		private void TriggerTrafficPanicMasteryPulse(int masteryLevel)
		{
			pendingTrafficPanicMasteryPulse = true;
			pendingTrafficPanicMasteryLevel = Mathf.Max(1, masteryLevel);
			pendingTrafficPanicMasteryGainScale = GetTrafficPanicMasteryGainScale(pendingTrafficPanicMasteryLevel);
		}

		private void ConsumePendingTrafficPanicMasteryPulse()
		{
			if (!pendingTrafficPanicMasteryPulse)
			{
				return;
			}
			int num = Mathf.Max(1, pendingTrafficPanicMasteryLevel);
			float num2 = Mathf.Clamp(pendingTrafficPanicMasteryGainScale, 0.2f, 1f);
			pendingTrafficPanicMasteryPulse = false;
			pendingTrafficPanicMasteryLevel = 0;
			pendingTrafficPanicMasteryGainScale = 1f;
			if (!stageRunning)
			{
				return;
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : trafficMapCenter);
			float radius = Mathf.Max(2.2f, trafficPanicRadius + trafficPanicMasteryRadiusAdd * num2 * (float)Mathf.Max(1, num));
			float intensity = Mathf.Lerp(0.62f, 1f, num2);
			ApplyTrafficPanic(val, radius, intensity);
			PushAnnouncement($"CHAOS SURGE Lv.{num}", AnnouncementTone.Milestone, Mathf.Lerp(0.85f, 1.4f, num2));
			if (damageNumberSystem == null)
			{
				damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
			}
			if ((Object)(object)damageNumberSystem != (Object)null && !levelUpOpen)
			{
				damageNumberSystem.ShowTag(val + Vector3.up * 1.1f, $"CHAOS SURGE Lv.{num}", emphasis: true);
			}
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null && !levelUpOpen)
			{
				feedbackSystem.PlayComboRushFeedback(val + Vector3.up * 0.24f, Mathf.Lerp(0.56f, 1f, num2), Mathf.Clamp(radius * 0.5f, 2.2f, 6.2f));
			}
		}

		private void HandleStreetPropBroken(DummyStreetPropReactive.PropBreakInfo info)
		{
			if (!Application.isPlaying || !enableTrafficSimulation || !enableTrafficPanic || !stageRunning || trafficVehicles.Count <= 0)
			{
				return;
			}
			if (info.Kind != DummyStreetPropReactive.PropKind.Vehicle && info.Kind != DummyStreetPropReactive.PropKind.ChainBarrel && info.Kind != DummyStreetPropReactive.PropKind.Transformer)
			{
				return;
			}
			float num = Mathf.Max(2f, trafficPanicRadius);
			if (info.Kind == DummyStreetPropReactive.PropKind.ChainBarrel)
			{
				num += 1f;
			}
			else if (info.Kind == DummyStreetPropReactive.PropKind.Transformer)
			{
				num += 1.8f;
			}
			float intensity01 = Mathf.Clamp01(info.Impact01 + ((info.Kind == DummyStreetPropReactive.PropKind.Vehicle) ? 0f : 0.2f));
			ApplyTrafficPanic(info.Position, num, intensity01);
		}

		private void ApplyTrafficPanic(Vector3 center, float radius, float intensity01)
		{
			if (!enableTrafficPanic || trafficVehicles.Count <= 0)
			{
				return;
			}
			float num = Mathf.Max(0.5f, radius);
			float num2 = num * num;
			float num3 = Mathf.Max(0.15f, Mathf.Min(trafficPanicStopDuration.x, trafficPanicStopDuration.y));
			float num4 = Mathf.Max(num3 + 0.1f, Mathf.Max(trafficPanicStopDuration.x, trafficPanicStopDuration.y));
			float num5 = Mathf.Clamp(trafficPanicSpeedScale, 0.25f, 1f);
			float num6 = Mathf.Clamp01(trafficPanicSwerveChance);
			float num7 = Mathf.Max(0f, trafficPanicSwerveOffset);
			float num8 = Mathf.Clamp01(intensity01);
			int num9 = 0;
			for (int i = 0; i < trafficVehicles.Count; i++)
			{
				TrafficVehicleState trafficVehicleState = trafficVehicles[i];
				if (trafficVehicleState == null || (Object)(object)trafficVehicleState.Root == (Object)null || !((Component)trafficVehicleState.Root).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 val = trafficVehicleState.Root.position - center;
				val.y = 0f;
				float sqrMagnitude = val.sqrMagnitude;
				if (sqrMagnitude > num2)
				{
					continue;
				}
				float num10 = Mathf.Sqrt(sqrMagnitude);
				float num11 = 1f - Mathf.Clamp01(num10 / num);
				float num12 = Mathf.Lerp(num3, num4, num11) * Mathf.Lerp(0.85f, 1.2f, num8);
				trafficVehicleState.StopTimer = Mathf.Max(trafficVehicleState.StopTimer, num12);
				trafficVehicleState.IntersectionCooldown = Mathf.Max(trafficVehicleState.IntersectionCooldown, 0.2f + 0.45f * num11);
				trafficVehicleState.PanicSlowTimer = Mathf.Max(trafficVehicleState.PanicSlowTimer, num12 + Mathf.Lerp(0.1f, 0.75f, num11));
				float num13 = Mathf.Lerp(1f, num5, num11 * Mathf.Lerp(0.6f, 1f, num8));
				if (trafficVehicleState.PanicSpeedScale <= 0f)
				{
					trafficVehicleState.PanicSpeedScale = 1f;
				}
				trafficVehicleState.PanicSpeedScale = Mathf.Min(trafficVehicleState.PanicSpeedScale, num13);
				if (num7 > 0.001f && num11 > 0.28f && (float)trafficRng.NextDouble() <= num6 * num11)
				{
					float num14 = ((trafficRng.Next(0, 2) == 0) ? (-1f) : 1f);
					float num15 = num14 * num7 * Mathf.Lerp(0.35f, 1f, num11);
					if (trafficVehicleState.AlongX)
					{
						float min = trafficMapCenter.z - trafficMapHalfExtents.y - 1.4f;
						float max = trafficMapCenter.z + trafficMapHalfExtents.y + 1.4f;
						trafficVehicleState.LaneCoord = Mathf.Clamp(trafficVehicleState.LaneCoord + num15, min, max);
					}
					else
					{
						float min2 = trafficMapCenter.x - trafficMapHalfExtents.x - 1.4f;
						float max2 = trafficMapCenter.x + trafficMapHalfExtents.x + 1.4f;
						trafficVehicleState.LaneCoord = Mathf.Clamp(trafficVehicleState.LaneCoord + num15, min2, max2);
					}
				}
				num9++;
			}
			if (num9 > 0 && (Object)(object)feedbackSystem != (Object)null && !levelUpOpen)
			{
				float normalizedIntensity = Mathf.Clamp01(0.22f + num8 * 0.45f + (float)num9 * 0.03f);
				feedbackSystem.PlayHitFeedback(center + Vector3.up * 0.2f, normalizedIntensity);
			}
			int num16 = Mathf.Max(1, trafficPanicBonusMinAffected);
			if (num9 >= num16 && trafficPanicBonusCooldownRemaining <= 0f)
			{
				int num17 = Mathf.Max(0, num9 - num16 + 1);
				int num18 = Mathf.Max(0, trafficPanicBonusBaseScore);
				int num19 = Mathf.Max(0, trafficPanicBonusPerVehicle);
				int num20 = num18 + num17 * num19;
				float num21 = Mathf.Max(0.4f, trafficPanicChainWindow);
				int num22 = Mathf.Max(1, trafficPanicChainMaxStacks);
				if (trafficPanicChainRemaining > 0.001f)
				{
					trafficPanicChainStack = Mathf.Min(num22, trafficPanicChainStack + 1);
				}
				else
				{
					trafficPanicChainStack = 1;
				}
				trafficPanicChainRemaining = num21;
				float num23 = 1f + Mathf.Max(0f, trafficPanicChainBonusPerStack) * (float)Mathf.Max(0, trafficPanicChainStack - 1);
				num20 = Mathf.RoundToInt((float)num20 * Mathf.Lerp(0.85f, 1.28f, num8) * num23);
				if (num20 > 0)
				{
					scoreSystem?.AddScore(num20);
				}
				trafficPanicBonusCooldownRemaining = Mathf.Max(0.1f, trafficPanicBonusCooldown) * Mathf.Lerp(0.74f, 1.18f, Mathf.Clamp01((float)num9 / 10f));
				if (showTrafficPanicBonusTag)
				{
					damageNumberSystem = (damageNumberSystem ?? Object.FindFirstObjectByType<DamageNumberSystem>());
					if ((Object)(object)damageNumberSystem != (Object)null && !levelUpOpen)
					{
						int num24 = Mathf.Max(1, trafficPanicChainTagMinStack);
						bool emphasis = num9 >= num16 + 2 || trafficPanicChainStack >= num24;
						string text = ((trafficPanicChainStack >= num24) ? $"PANIC CHAIN x{trafficPanicChainStack} +{num20}" : (emphasis ? $"PANIC x{num9} +{num20}" : $"PANIC +{num20}"));
						damageNumberSystem.ShowTag(center + Vector3.up * 0.95f, text, emphasis);
					}
				}
				if ((Object)(object)feedbackSystem != (Object)null && !levelUpOpen)
				{
					float normalizedIntensity2 = Mathf.Clamp01(0.42f + num8 * 0.3f + (float)num17 * 0.06f + (float)(trafficPanicChainStack - 1) * 0.05f);
					feedbackSystem.PlayComboRushFeedback(center + Vector3.up * 0.18f, normalizedIntensity2, Mathf.Clamp(num * 0.55f, 2.2f, 5.5f));
				}
				ResolvePlayerController();
				int num25 = Mathf.Max(1, trafficPanicCounterSurgeMinChain);
				if (enableTrafficPanicCounterSurgeReward && (Object)(object)cachedPlayerController != (Object)null && trafficPanicChainStack >= num25)
				{
					int num26 = Mathf.Max(0, trafficPanicChainStack - num25);
					float num27 = 1f + Mathf.Max(0f, trafficPanicCounterSurgePerChain) * (float)num26;
					float num28 = Mathf.Lerp(1f, 1.18f, Mathf.Clamp01((float)num17 / 6f));
					float num29 = Mathf.Lerp(0.92f, 1.2f, num8);
					float speedScale = Mathf.Max(1f, Mathf.Max(1f, trafficPanicCounterSurgeSpeedScale) * num27 * num28 * num29);
					float impactScale = Mathf.Max(1f, Mathf.Max(1f, trafficPanicCounterSurgeImpactScale) * num27 * num28 * Mathf.Lerp(0.95f, 1.24f, num8));
					float duration = Mathf.Max(0.12f, Mathf.Max(0.1f, trafficPanicCounterSurgeDuration) * Mathf.Lerp(0.9f, 1.35f, num8));
					cachedPlayerController.ApplyCounterSurge(speedScale, impactScale, duration);
					if (showTrafficPanicCounterSurgeTag)
					{
						damageNumberSystem = (damageNumberSystem ?? Object.FindFirstObjectByType<DamageNumberSystem>());
						if ((Object)(object)damageNumberSystem != (Object)null && !levelUpOpen)
						{
							string tag = ((trafficPanicChainStack >= Mathf.Max(num25 + 2, trafficPanicChainTagMinStack + 1)) ? $"PANIC SURGE x{trafficPanicChainStack}" : "PANIC SURGE");
							damageNumberSystem.ShowTag(center + Vector3.up * 1.18f, tag, emphasis: true);
						}
					}
				}
				if (!trafficPanicJackpotTriggered && trafficPanicChainStack >= num22)
				{
					trafficPanicJackpotTriggered = true;
					int num30 = Mathf.Max(0, trafficPanicJackpotScore);
					int num31 = Mathf.RoundToInt((float)num30 * Mathf.Lerp(1f, 1.35f, num8));
					if (num31 > 0)
					{
						scoreSystem?.AddScore(num31);
					}
					PushAnnouncement($"PANIC JACKPOT +{num31}", AnnouncementTone.Destruction, 1.05f);
					if (showTrafficPanicJackpotTag)
					{
						damageNumberSystem = (damageNumberSystem ?? Object.FindFirstObjectByType<DamageNumberSystem>());
						if ((Object)(object)damageNumberSystem != (Object)null && !levelUpOpen)
						{
							damageNumberSystem.ShowTag(center + Vector3.up * 1.05f, $"JACKPOT +{num31}", emphasis: true);
						}
					}
					if ((Object)(object)feedbackSystem != (Object)null && !levelUpOpen)
					{
						float normalizedIntensity3 = Mathf.Clamp01(0.58f + num8 * 0.3f);
						feedbackSystem.PlayTotalDestructionFeedback(center + Vector3.up * 0.22f, normalizedIntensity3);
					}
				}
			}
		}
	}
}
