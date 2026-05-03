using System.Collections;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void ResetOverdriveState()
		{
			overdriveActive = false;
			overdriveRemaining = 0f;
			overdriveCooldownRemaining = 0f;
			overdriveScoreCarry = 0f;
			nextOverdriveChainGate = GetCurrentOverdriveChainGate();
			ResolvePlayerController();
			cachedPlayerController?.ClearTemporaryOverdrive();
			if (damageNumberSystem == null)
			{
				damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
			}
			damageNumberSystem?.SetOverdriveMode(active: false);
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.SetOverdriveCameraState(active: false);
			StopOverdriveTimePulse();
		}

		private void UpdateOverdrive(float deltaTime)
		{
			if (!stageRunning || levelUpOpen)
			{
				return;
			}
			overdriveCooldownRemaining = Mathf.Max(0f, overdriveCooldownRemaining - deltaTime);
			int num = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.ChainCount) : 0);
			if (!overdriveActive)
			{
				nextOverdriveChainGate = Mathf.Min(nextOverdriveChainGate, GetCurrentOverdriveChainGate());
			}
			if (!overdriveActive && num >= nextOverdriveChainGate && overdriveCooldownRemaining <= 0f)
			{
				ActivateOverdrive(num);
			}
			if (overdriveActive)
			{
				overdriveRemaining -= deltaTime;
				overdriveScoreCarry += (float)Mathf.Max(0, overdriveBonusScorePerSecond) * deltaTime;
				int num2 = Mathf.FloorToInt(overdriveScoreCarry);
				if (num2 > 0)
				{
					scoreSystem?.AddScore(num2);
					overdriveScoreCarry -= num2;
				}
				if (overdriveRemaining <= 0f)
				{
					DeactivateOverdrive();
				}
			}
		}

		private void ActivateOverdrive(int chain)
		{
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			overdriveActive = true;
			float num = (float)Mathf.Clamp(chain - overdriveChainThreshold, 0, 8) * 0.2f;
			overdriveRemaining = Mathf.Max(1f, overdriveDurationSeconds + num);
			overdriveCooldownRemaining = Mathf.Max(overdriveCooldownRemaining, overdriveCooldownSeconds);
			overdriveScoreCarry = 0f;
			nextOverdriveChainGate = chain + Mathf.Max(3, GetCurrentOverdriveChainStep());
			ResolvePlayerController();
			cachedPlayerController?.ApplyTemporaryOverdrive(Mathf.Max(1f, overdriveSpeedScale), Mathf.Max(1f, overdriveImpactScale));
			if (damageNumberSystem == null)
			{
				damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
			}
			damageNumberSystem?.SetOverdriveMode(active: true);
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.SetOverdriveCameraState(active: true);
			scoreSystem?.AddScore(Mathf.Max(0, overdriveStartBonusScore));
			StartOverdriveTimePulse();
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)playerTransform != (Object)null)
			{
				feedbackSystem?.PlayDestroyFeedback(playerTransform.position + Vector3.up * 0.35f, 1f);
			}
		}

		private void DeactivateOverdrive()
		{
			if (overdriveActive)
			{
				overdriveActive = false;
				overdriveRemaining = 0f;
				overdriveScoreCarry = 0f;
				ResolvePlayerController();
				cachedPlayerController?.ClearTemporaryOverdrive();
				if (damageNumberSystem == null)
				{
					damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
				}
				damageNumberSystem?.SetOverdriveMode(active: false);
				if (cameraFollowSystem == null)
				{
					cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
				}
				cameraFollowSystem?.SetOverdriveCameraState(active: false);
			}
		}

		private void StartOverdriveTimePulse()
		{
			StopOverdriveTimePulse();
			overdrivePulseRoutine = ((MonoBehaviour)this).StartCoroutine(OverdriveTimePulseRoutine());
		}

		private void StopOverdriveTimePulse()
		{
			if (overdrivePulseRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(overdrivePulseRoutine);
				overdrivePulseRoutine = null;
			}
			if (Mathf.Abs(Time.timeScale - 1f) > 0.0001f)
			{
				Time.timeScale = 1f;
			}
			if (Mathf.Abs(Time.fixedDeltaTime - 0.02f) > 0.0001f)
			{
				Time.fixedDeltaTime = 0.02f;
			}
		}

		private IEnumerator OverdriveTimePulseRoutine()
		{
			float originalTimeScale = Mathf.Max(0.0001f, Time.timeScale);
			float originalFixedDelta = Time.fixedDeltaTime;
			Time.timeScale = 0.92f;
			Time.fixedDeltaTime = originalFixedDelta * (Time.timeScale / originalTimeScale);
			yield return (object)new WaitForSecondsRealtime(0.12f);
			Time.timeScale = originalTimeScale;
			Time.fixedDeltaTime = originalFixedDelta;
			overdrivePulseRoutine = null;
		}

		private void ResetComboRushState()
		{
			int num = (nextComboRushChainGate = GetCurrentComboRushChainGate());
			comboRushAnnouncementRemaining = 0f;
			comboRushAnnouncementText = string.Empty;
			comboRushAnnouncementPriority = -1;
			comboRushAnnouncementCooldown = 0f;
			pendingComboRushAnnouncementRemaining = 0f;
			pendingComboRushAnnouncementText = string.Empty;
			pendingComboRushAnnouncementPriority = -1;
			momentumChainLostFeedbackCooldownRemaining = 0f;
		}

		private void UpdateMomentumChain(float deltaTime)
		{
			momentumChainLostFeedbackCooldownRemaining = Mathf.Max(0f, momentumChainLostFeedbackCooldownRemaining - deltaTime);
			if (!stageRunning || levelUpOpen || (Object)(object)scoreSystem == (Object)null)
			{
				return;
			}
			if (scoreSystem.TickChainTimer(deltaTime, out int brokenChain))
			{
				HandleMomentumChainLost(brokenChain);
			}
		}

		private void HandleMomentumChainLost(int brokenChain)
		{
			nextComboRushChainGate = GetCurrentComboRushChainGate();
			if (!overdriveActive)
			{
				nextOverdriveChainGate = GetCurrentOverdriveChainGate();
			}
			if (brokenChain < Mathf.Max(2, momentumChainLostMinChain) || momentumChainLostFeedbackCooldownRemaining > 0f)
			{
				return;
			}

			momentumChainLostFeedbackCooldownRemaining = Mathf.Max(0.1f, momentumChainLostFeedbackCooldown);
			PushAnnouncement($"CHAIN LOST x{brokenChain}", AnnouncementTone.Burst, 0.82f);
			if (endOverdriveOnMomentumBreak && overdriveActive)
			{
				DeactivateOverdrive();
				nextOverdriveChainGate = GetCurrentOverdriveChainGate();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			damageNumberSystem = (damageNumberSystem ?? Object.FindFirstObjectByType<DamageNumberSystem>());
			damageNumberSystem?.ShowTag(val + Vector3.up * 1.15f, "CHAIN LOST", brokenChain >= GetCurrentComboRushChainGate());
			feedbackSystem = (feedbackSystem ?? Object.FindFirstObjectByType<FeedbackSystem>());
			feedbackSystem?.PlayHitFeedback(val + Vector3.up * 0.2f, Mathf.Clamp01(0.32f + (float)brokenChain * 0.015f));
			cameraFollowSystem = (cameraFollowSystem ?? Object.FindFirstObjectByType<CameraFollowSystem>());
			cameraFollowSystem?.AddImpulse(Mathf.Clamp(0.18f + (float)brokenChain * 0.008f, 0.2f, 0.42f));
		}

		private void UpdateComboRush(float deltaTime)
		{
			comboRushAnnouncementCooldown = Mathf.Max(0f, comboRushAnnouncementCooldown - deltaTime);
			pendingComboRushAnnouncementRemaining = Mathf.Max(0f, pendingComboRushAnnouncementRemaining - deltaTime);
			if (pendingComboRushAnnouncementRemaining <= 0f)
			{
				pendingComboRushAnnouncementText = string.Empty;
				pendingComboRushAnnouncementPriority = -1;
			}
			if (comboRushAnnouncementRemaining > 0f)
			{
				comboRushAnnouncementRemaining = Mathf.Max(0f, comboRushAnnouncementRemaining - deltaTime);
				if (comboRushAnnouncementRemaining <= 0f)
				{
					comboRushAnnouncementText = string.Empty;
					comboRushAnnouncementPriority = -1;
					comboRushAnnouncementCooldown = 0f;
				}
			}
			if (comboRushAnnouncementRemaining <= 0f && !string.IsNullOrEmpty(pendingComboRushAnnouncementText) && pendingComboRushAnnouncementRemaining > 0f)
			{
				comboRushAnnouncementText = pendingComboRushAnnouncementText;
				comboRushAnnouncementRemaining = pendingComboRushAnnouncementRemaining;
				comboRushAnnouncementPriority = pendingComboRushAnnouncementPriority;
				comboRushAnnouncementCooldown = Mathf.Min(0.18f, comboRushAnnouncementRemaining * 0.18f);
				pendingComboRushAnnouncementText = string.Empty;
				pendingComboRushAnnouncementRemaining = 0f;
				pendingComboRushAnnouncementPriority = -1;
			}
			if (stageRunning && !levelUpOpen && !((Object)(object)scoreSystem == (Object)null))
			{
				int num = GetCurrentComboRushChainGate();
				int num2 = Mathf.Max(0, scoreSystem.ChainCount);
				if (num2 >= num && num2 >= nextComboRushChainGate)
				{
					TriggerComboRush(num2);
					nextComboRushChainGate = (num2 / num + 1) * num;
				}
			}
		}

		private bool IsEarlyChainAssistWindow()
		{
			if (!stageRunning)
			{
				return false;
			}
			if (Mathf.Max(1, currentStageNumber) > 2)
			{
				return false;
			}
			float num = (((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f);
			float num2 = Mathf.Max(0f, num - remainingStageTime);
			if (num2 > 18f)
			{
				return false;
			}
			int num3 = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
			return num3 < 8;
		}

		private int GetCurrentComboRushChainGate()
		{
			int num = Mathf.Max(2, comboRushChainStep);
			if (IsEarlyChainAssistWindow())
			{
				num = Mathf.Max(6, num - 2);
			}
			else if (Mathf.Max(1, currentStageNumber) >= 5)
			{
				num += 1;
			}
			return num;
		}

		private int GetCurrentOverdriveChainGate()
		{
			int num = Mathf.Max(2, overdriveChainThreshold);
			if (IsEarlyChainAssistWindow())
			{
				num = Mathf.Max(8, num - 3);
			}
			else if (Mathf.Max(1, currentStageNumber) >= 6)
			{
				num += 2;
			}
			return num;
		}

		private int GetCurrentOverdriveChainStep()
		{
			int num = Mathf.Max(3, overdriveChainThreshold / 3);
			if (IsEarlyChainAssistWindow())
			{
				num = Mathf.Max(2, num - 1);
			}
			else if (Mathf.Max(1, currentStageNumber) >= 6)
			{
				num += 1;
			}
			return num;
		}

		private void TriggerComboRush(int chain)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			float num = comboRushRadius + (float)Mathf.Max(0, currentLevel - 1) * comboRushRadiusPerLevel;
			if (overdriveActive)
			{
				num += 0.9f;
			}
			int num2 = ApplyComboRushToDestructibles(val, num, chain);
			int num3 = ApplyComboRushToStreetProps(val, num, chain);
			int num4 = num2 + num3;
			float num5a = Mathf.Clamp01((float)Mathf.Max(0, chain - GetCurrentComboRushChainGate()) / 22f);
			int num5 = Mathf.Max(0, comboRushScoreBonus + num4 * 45 + Mathf.RoundToInt((float)chain * Mathf.Lerp(9f, 13f, num5a)));
			scoreSystem?.AddScore(num5);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			float normalizedIntensity = Mathf.Clamp01(0.45f + (float)num4 * 0.03f + (float)chain * Mathf.Lerp(0.015f, 0.021f, num5a));
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				feedbackSystem.PlayComboRushFeedback(val + Vector3.up * 0.15f, normalizedIntensity, num);
			}
			else
			{
				if (cameraFollowSystem == null)
				{
					cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
				}
				cameraFollowSystem?.AddImpulse(Mathf.Lerp(0.42f, 0.88f, Mathf.Clamp01((float)chain / 24f)));
			}
			PushAnnouncement($"CRUSH RUSH x{chain}  +{num5}", AnnouncementTone.Burst, comboRushAnnouncementSeconds);
		}

		private int ApplyComboRushToDestructibles(Vector3 center, float radius, int chain)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			RefreshDestructibleCache();
			float num = radius * radius;
			int num2 = Mathf.Max(1, comboRushDestructibleCap + Mathf.FloorToInt((float)Mathf.Clamp(chain - comboRushChainStep, 0, 28) * 0.25f));
			float num3 = Mathf.Lerp(1f, 1.35f, Mathf.Clamp01((float)(chain - comboRushChainStep) / 20f));
			int num4 = 0;
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				if (num4 >= num2)
				{
					break;
				}
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !((Component)dummyDestructibleBlock).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 val = ((Component)dummyDestructibleBlock).transform.position - center;
				val.y = 0f;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude > num))
				{
					float num5 = Mathf.Sqrt(sqrMagnitude);
					float num6 = 1f - Mathf.Clamp01(num5 / Mathf.Max(0.01f, radius));
					float num7 = Mathf.Lerp(0.3f, 0.92f, num6);
					if (!(Random.value > num7))
					{
						float damage = Mathf.Lerp(comboRushDamageRange.x, comboRushDamageRange.y, num6) * num3;
						Vector3 hitPoint = ((Component)dummyDestructibleBlock).transform.position + Vector3.up * 0.45f;
						dummyDestructibleBlock.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.45f, 1f, num6));
						num4++;
					}
				}
			}
			return num4;
		}

		private int ApplyComboRushToStreetProps(Vector3 center, float radius, int chain)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			DummyStreetPropReactive[] array = Object.FindObjectsByType<DummyStreetPropReactive>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			if (array == null || array.Length == 0)
			{
				return 0;
			}
			float num = radius * radius;
			float num2 = Mathf.Clamp01(comboRushStreetBreakChance + (float)Mathf.Clamp(chain - comboRushChainStep, 0, 30) * 0.01f);
			int num3 = 0;
			foreach (DummyStreetPropReactive dummyStreetPropReactive in array)
			{
				if ((Object)(object)dummyStreetPropReactive == (Object)null || !((Component)dummyStreetPropReactive).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 val = ((Component)dummyStreetPropReactive).transform.position - center;
				val.y = 0f;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude > num))
				{
					float num4 = Mathf.Sqrt(sqrMagnitude);
					float num5 = 1f - Mathf.Clamp01(num4 / Mathf.Max(0.01f, radius));
					float num6 = Mathf.Lerp(num2 * 0.45f, num2, num5);
					if (!(Random.value > num6))
					{
						Vector3 hitPoint = ((Component)dummyStreetPropReactive).transform.position + Vector3.up * 0.2f;
						dummyStreetPropReactive.ApplyExternalBreak(hitPoint, Mathf.Lerp(0.5f, 1f, num5), drillMode: false);
						num3++;
					}
				}
			}
			if (num3 > 0 && enableTrafficPanic)
			{
				float intensity = Mathf.Clamp01(0.26f + Mathf.Clamp01((float)num3 / 8f) * 0.62f);
				ApplyTrafficPanic(center, Mathf.Max(2f, radius * 1.08f), intensity);
			}
			return num3;
		}

		private void PlayStageEntryImpact()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			if (!stageRunning)
			{
				return;
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			Vector3 val = (((Object)(object)playerTransform != (Object)null) ? playerTransform.position : Vector3.zero);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			feedbackSystem?.PlayDestroyFeedback(val + Vector3.up * 0.35f, 1f);
			RefreshDestructibleCache();
			int num = ((stageEntryMaxTargets <= 0) ? int.MaxValue : stageEntryMaxTargets);
			int num2 = 0;
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				DummyDestructibleBlock dummyDestructibleBlock = destructibleCache[i];
				if ((Object)(object)dummyDestructibleBlock == (Object)null || !((Component)dummyDestructibleBlock).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 val2 = ((Component)dummyDestructibleBlock).transform.position - val;
				val2.y = 0f;
				float magnitude = val2.magnitude;
				if (magnitude > stageEntryRadius)
				{
					continue;
				}
				float num3 = 1f - Mathf.Clamp01(magnitude / Mathf.Max(0.01f, stageEntryRadius));
				float num4 = Mathf.Lerp(0.35f, 0.95f, num3);
				if (!(Random.value > num4))
				{
					float damage = Mathf.Lerp(stageEntryDamageRange.x, stageEntryDamageRange.y, num3);
					Vector3 hitPoint = ((Component)dummyDestructibleBlock).transform.position + Vector3.up * 0.45f;
					dummyDestructibleBlock.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.45f, 1f, num3));
					num2++;
					if (num2 >= num)
					{
						break;
					}
				}
			}
		}
	}
}
