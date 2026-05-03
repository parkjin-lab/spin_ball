using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private float GetFormActiveSkillCooldown(FormActiveSkill skill)
		{
			return skill switch
			{
				FormActiveSkill.SpherePulse => Mathf.Max(0.1f, spherePulseCooldown), 
				FormActiveSkill.SpikeBurst => Mathf.Max(0.1f, spikeBurstCooldown), 
				FormActiveSkill.RamBreach => Mathf.Max(0.1f, ramBreachCooldown), 
				FormActiveSkill.SaucerDash => Mathf.Max(0.1f, saucerDashCooldown), 
				FormActiveSkill.CrusherSlam => Mathf.Max(0.1f, crusherSlamCooldown), 
				_ => 0f, 
			};
		}

		private bool ShouldShowActionButtons()
		{
			if (!enableFormActiveSkills)
			{
				return showActionButtons;
			}
			if (!showActionButtons)
			{
				return GetFormActiveSkill(GetCurrentSelectedForm()) != FormActiveSkill.None;
			}
			return true;
		}

		private void ResetFormActiveSkillRuntime()
		{
			formSkillCooldownRemaining = 0f;
			cachedTransformButtonLabel = string.Empty;
			cachedSpecial1ButtonLabel = string.Empty;
			cachedSpecial2ButtonLabel = string.Empty;
			actionSkillReadyVisualActive = false;
			previousActionSkillReadyVisualState = false;
			actionSkillReadyPulseRemaining = 0f;
			actionSkillTapConfirmRemaining = 0f;
			ResetActionSkillButtonVisuals();
		}

		private void UpdateFormActiveSkillCooldown(float deltaTime)
		{
			if (enableFormActiveSkills && !(formSkillCooldownRemaining <= 0f))
			{
				int num = Mathf.CeilToInt(formSkillCooldownRemaining);
				formSkillCooldownRemaining = Mathf.Max(0f, formSkillCooldownRemaining - Mathf.Max(0f, deltaTime));
				if (Mathf.CeilToInt(formSkillCooldownRemaining) != num)
				{
					RefreshActionSkillButtons(force: false);
				}
			}
		}

		private void ProcessActionSkillInput()
		{
			Keyboard current = Keyboard.current;
			if (current != null)
			{
				if (current.escapeKey.wasPressedThisFrame || current.pKey.wasPressedThisFrame)
				{
					TogglePause();
				}
				if (current.spaceKey.wasPressedThisFrame || current.eKey.wasPressedThisFrame)
				{
					HandlePrimaryActionSkill();
				}
			}
		}

		private void HandlePrimaryActionSkill()
		{
			if (TryTriggerFormActiveSkill())
			{
				TriggerActionSkillTapConfirmFeedback();
			}
		}

		private bool TryTriggerFormActiveSkill()
		{
			if (!stageRunning || levelUpOpen || !enableFormActiveSkills)
			{
				return false;
			}
			FormActiveSkill formActiveSkill = GetFormActiveSkill(GetCurrentSelectedForm());
			if (formActiveSkill == FormActiveSkill.None)
			{
				return false;
			}
			if (formSkillCooldownRemaining > 0f)
			{
				return false;
			}
			bool drillSynergy = IsDrillSynergyActive();
			bool overdriveSynergy = IsOverdriveSynergyActive();
			bool flag = false;
			switch (formActiveSkill)
			{
			case FormActiveSkill.SpherePulse:
				flag = ExecuteSpherePulseSkill();
				break;
			case FormActiveSkill.SpikeBurst:
				flag = ExecuteSpikeBurstSkill();
				break;
			case FormActiveSkill.RamBreach:
				flag = ExecuteRamBreachSkill();
				break;
			case FormActiveSkill.SaucerDash:
				flag = ExecuteSaucerDashSkill();
				break;
			case FormActiveSkill.CrusherSlam:
				flag = ExecuteCrusherSlamSkill();
				break;
			}
			if (!flag)
			{
				return false;
			}
			ApplyFormSkillSynergyEffects(formActiveSkill, drillSynergy, overdriveSynergy);
			float formSkillCooldownScale = GetFormSkillCooldownScale(formActiveSkill, drillSynergy, overdriveSynergy);
			formSkillCooldownRemaining = GetFormActiveSkillCooldown(formActiveSkill) * formSkillCooldownScale;
			RefreshActionSkillButtons(force: true);
			return true;
		}

		private bool ExecuteSpherePulseSkill()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				return false;
			}
			Vector3 position = playerTransform.position;
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			int num2 = ApplyAreaImpactToDestructibles(position, spherePulseRadius, spherePulseDamageRange, Mathf.Max(4, spherePulseDestructibleCap), 0.46f, 0.95f, Mathf.Lerp(1f, 1.18f, Mathf.Clamp01(((float)num - 2f) / 16f)), suppressFeedback: true);
			int num3 = ApplyAreaBreakToStreetProps(position, spherePulseRadius, spherePulseStreetBreakChance, drillMode: false);
			int num4 = num2 + num3;
			if ((Object)(object)playerBody == (Object)null)
			{
				playerBody = ((Component)playerTransform).GetComponent<Rigidbody>();
			}
			if ((Object)(object)playerBody != (Object)null)
			{
				Vector3 val = default(Vector3);
				val = new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z);
				if (val.sqrMagnitude > 0.02f)
				{
					playerBody.AddForce(val.normalized * 2.8f, (ForceMode)2);
				}
			}
			int num5 = Mathf.Max(0, formSkillBaseScoreBonus - 10 + num4 * 24);
			scoreSystem?.AddScore(num5);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.52f + (float)num4 * 0.05f);
				feedbackSystem.PlayComboRushFeedback(position + Vector3.up * 0.16f, normalizedIntensity, spherePulseRadius);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(0.58f);
			PushAnnouncement($"SPHERE PULSE +{num5}", AnnouncementTone.Burst, Mathf.Max(0.56f, comboRushAnnouncementSeconds * 0.7f));
			return true;
		}

		private bool ExecuteSpikeBurstSkill()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				return false;
			}
			Vector3 position = playerTransform.position;
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			int num2 = ApplyAreaImpactToDestructibles(position, spikeBurstRadius, spikeBurstDamageRange, Mathf.Max(5, comboRushDestructibleCap / 2), 0.55f, 1f, Mathf.Lerp(1.05f, 1.28f, Mathf.Clamp01(((float)num - 2f) / 18f)), suppressFeedback: true);
			int num3 = ApplyAreaBreakToStreetProps(position, spikeBurstRadius, 0.78f, drillMode: true);
			int num4 = num2 + num3;
			int num5 = Mathf.Max(0, formSkillBaseScoreBonus + num4 * 30);
			scoreSystem?.AddScore(num5);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.58f + (float)num4 * 0.05f);
				feedbackSystem.PlayComboRushFeedback(position + Vector3.up * 0.18f, normalizedIntensity, spikeBurstRadius);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(0.7f);
			PushAnnouncement($"SPIKE BURST +{num5}", AnnouncementTone.Burst, Mathf.Max(0.6f, comboRushAnnouncementSeconds * 0.75f));
			return true;
		}

		private bool ExecuteRamBreachSkill()
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				return false;
			}
			if ((Object)(object)playerBody == (Object)null)
			{
				playerBody = ((Component)playerTransform).GetComponent<Rigidbody>();
			}
			Vector3 val = (Vector3)(((Object)(object)playerBody != (Object)null) ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z) : Vector3.zero);
			if (val.sqrMagnitude < 0.03f)
			{
				Vector3 forward = playerTransform.forward;
				val = new Vector3(forward.x, 0f, forward.z);
			}
			if (val.sqrMagnitude < 0.001f)
			{
				val = Vector3.forward;
			}
			Vector3 normalized = val.normalized;
			if ((Object)(object)playerBody != (Object)null)
			{
				playerBody.AddForce(normalized * Mathf.Max(2f, ramBreachVelocityBoost), (ForceMode)2);
			}
			Vector3 center = playerTransform.position + normalized * Mathf.Max(1.2f, ramBreachRadius * 0.55f);
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			int num2 = ApplyAreaImpactToDestructibles(center, ramBreachRadius, ramBreachDamageRange, Mathf.Max(6, comboRushDestructibleCap), 0.5f, 0.98f, Mathf.Lerp(1.12f, 1.4f, Mathf.Clamp01(((float)num - 2f) / 22f)), suppressFeedback: true);
			int num3 = ApplyAreaBreakToStreetProps(center, ramBreachRadius, 0.86f, drillMode: false);
			int num4 = num2 + num3;
			int num5 = Mathf.Max(0, formSkillBaseScoreBonus + 50 + num4 * 34);
			scoreSystem?.AddScore(num5);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.62f + (float)num4 * 0.05f);
				feedbackSystem.PlayComboRushFeedback(playerTransform.position + normalized * 0.4f + Vector3.up * 0.22f, normalizedIntensity, ramBreachRadius);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(0.82f);
			PushAnnouncement($"RAM BREACH +{num5}", AnnouncementTone.Burst, Mathf.Max(0.64f, comboRushAnnouncementSeconds * 0.78f));
			return true;
		}

		private bool ExecuteSaucerDashSkill()
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				return false;
			}
			if ((Object)(object)playerBody == (Object)null)
			{
				playerBody = ((Component)playerTransform).GetComponent<Rigidbody>();
			}
			Vector3 val = (Vector3)(((Object)(object)playerBody != (Object)null) ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z) : Vector3.zero);
			if (val.sqrMagnitude < 0.03f)
			{
				Vector3 forward = playerTransform.forward;
				val = new Vector3(forward.x, 0f, forward.z);
			}
			if (val.sqrMagnitude < 0.001f)
			{
				val = Vector3.forward;
			}
			Vector3 normalized = val.normalized;
			if ((Object)(object)playerBody != (Object)null)
			{
				playerBody.AddForce(normalized * Mathf.Max(2f, saucerDashVelocityBoost), (ForceMode)2);
			}
			Vector3 center = playerTransform.position + normalized * Mathf.Max(1f, saucerDashRadius * 0.45f);
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			int num2 = ApplyAreaImpactToDestructibles(center, saucerDashRadius, saucerDashDamageRange, Mathf.Max(4, comboRushDestructibleCap / 2), 0.4f, 0.92f, Mathf.Lerp(1f, 1.2f, Mathf.Clamp01(((float)num - 2f) / 20f)), suppressFeedback: true);
			int num3 = ApplyAreaBreakToStreetProps(center, saucerDashRadius, 0.74f, drillMode: true);
			int num4 = num2 + num3;
			int num5 = Mathf.Max(0, formSkillBaseScoreBonus + num4 * 26);
			scoreSystem?.AddScore(num5);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.5f + (float)num4 * 0.06f);
				feedbackSystem.PlayComboRushFeedback(playerTransform.position + normalized * 0.75f + Vector3.up * 0.2f, normalizedIntensity, saucerDashRadius);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(0.62f);
			PushAnnouncement($"SAUCER DASH +{num5}", AnnouncementTone.Burst, Mathf.Max(0.55f, comboRushAnnouncementSeconds * 0.7f));
			return true;
		}

		private bool ExecuteCrusherSlamSkill()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				return false;
			}
			if ((Object)(object)playerBody == (Object)null)
			{
				playerBody = ((Component)playerTransform).GetComponent<Rigidbody>();
			}
			if ((Object)(object)playerBody != (Object)null)
			{
				Vector3 linearVelocity = playerBody.linearVelocity;
				playerBody.linearVelocity = new Vector3(linearVelocity.x * 0.35f, linearVelocity.y, linearVelocity.z * 0.35f);
			}
			Vector3 position = playerTransform.position;
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			int num2 = ApplyAreaImpactToDestructibles(position, crusherSlamRadius, crusherSlamDamageRange, Mathf.Max(4, crusherSlamDestructibleCap), 0.82f, 1f, Mathf.Lerp(1.12f, 1.42f, Mathf.Clamp01(((float)num - 3f) / 24f)), suppressFeedback: true);
			int num3 = ApplyAreaBreakToStreetProps(position, crusherSlamRadius, crusherSlamStreetBreakChance, drillMode: false);
			int num4 = num2 + num3;
			int num5 = Mathf.Max(0, formSkillBaseScoreBonus + 90 + num4 * 38);
			scoreSystem?.AddScore(num5);
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.66f + (float)num4 * 0.05f);
				feedbackSystem.PlayComboRushFeedback(position + Vector3.up * 0.24f, normalizedIntensity, crusherSlamRadius);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(0.9f);
			PushAnnouncement($"CRUSHER SLAM +{num5}", AnnouncementTone.Destruction, Mathf.Max(0.7f, comboRushAnnouncementSeconds * 0.82f));
			return true;
		}

		private bool IsDrillSynergyActive()
		{
			if (!enableFormSkillSynergy)
			{
				return false;
			}
			ResolvePlayerController();
			if ((Object)(object)cachedPlayerController != (Object)null)
			{
				return cachedPlayerController.DrillMode;
			}
			return false;
		}

		private bool IsOverdriveSynergyActive()
		{
			if (enableFormSkillSynergy)
			{
				return overdriveActive;
			}
			return false;
		}

		private float GetFormSkillCooldownScale(FormActiveSkill skill, bool drillSynergy, bool overdriveSynergy)
		{
			if (!enableFormSkillSynergy || (!drillSynergy && !overdriveSynergy))
			{
				return 1f;
			}
			float num = 0f;
			if (drillSynergy)
			{
				num += 0.08f;
			}
			if (overdriveSynergy)
			{
				num += 0.14f;
			}
			switch (skill)
			{
			case FormActiveSkill.SpikeBurst:
				num += (drillSynergy ? 0.04f : 0f);
				break;
			case FormActiveSkill.RamBreach:
				num += (overdriveSynergy ? 0.04f : 0f);
				break;
			case FormActiveSkill.CrusherSlam:
				num += 0.05f;
				break;
			}
			return Mathf.Clamp(1f - num, formSkillSynergyCooldownFloor, 1f);
		}

		private void ApplyFormSkillSynergyEffects(FormActiveSkill skill, bool drillSynergy, bool overdriveSynergy)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_032b: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			//IL_0349: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0386: Unknown result type (might be due to invalid IL or missing references)
			//IL_038b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0420: Unknown result type (might be due to invalid IL or missing references)
			//IL_0422: Unknown result type (might be due to invalid IL or missing references)
			//IL_0432: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0502: Unknown result type (might be due to invalid IL or missing references)
			if (!enableFormSkillSynergy || (!drillSynergy && !overdriveSynergy))
			{
				return;
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				return;
			}
			Vector3 val = playerTransform.position;
			int num = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
			float num2 = Mathf.Lerp(1f, 1.12f, Mathf.Clamp01(((float)num - 2f) / 20f));
			num2 *= (drillSynergy ? 1.08f : 1f) * (overdriveSynergy ? 1.12f : 1f);
			float num3 = 2.8f;
			Vector2 damageRange = default(Vector2);
			damageRange = new Vector2(24f, 64f);
			int num4 = 4;
			float edgeChance = 0.42f;
			float centerChance = 0.86f;
			float num5 = 0.56f;
			bool flag = drillSynergy;
			float num6 = 0.2f;
			switch (skill)
			{
			case FormActiveSkill.SpherePulse:
				num3 = spherePulseRadius * 0.58f;
				damageRange = spherePulseDamageRange * 0.44f;
				num4 = Mathf.Max(3, spherePulseDestructibleCap / 3);
				num5 = spherePulseStreetBreakChance * 0.74f;
				num6 = 0.16f;
				break;
			case FormActiveSkill.SpikeBurst:
				num3 = spikeBurstRadius * 0.62f;
				damageRange = spikeBurstDamageRange * 0.46f;
				num4 = Mathf.Max(4, comboRushDestructibleCap / 3);
				num5 = 0.72f;
				flag = true;
				num6 = 0.2f;
				break;
			case FormActiveSkill.RamBreach:
			{
				Vector3 val4 = (Vector3)(((Object)(object)playerBody != (Object)null) ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z) : Vector3.zero);
				if (val4.sqrMagnitude < 0.01f)
				{
					Vector3 forward2 = playerTransform.forward;
					val4 = new Vector3(forward2.x, 0f, forward2.z);
				}
				Vector3 val5 = ((val4.sqrMagnitude > 0.001f) ? val4.normalized : Vector3.forward);
				val += val5 * (ramBreachRadius * 0.28f);
				num3 = ramBreachRadius * 0.6f;
				damageRange = ramBreachDamageRange * 0.4f;
				num4 = Mathf.Max(4, comboRushDestructibleCap / 3);
				num5 = 0.68f;
				num6 = 0.22f;
				break;
			}
			case FormActiveSkill.SaucerDash:
			{
				Vector3 val2 = (Vector3)(((Object)(object)playerBody != (Object)null) ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z) : Vector3.zero);
				if (val2.sqrMagnitude < 0.01f)
				{
					Vector3 forward = playerTransform.forward;
					val2 = new Vector3(forward.x, 0f, forward.z);
				}
				Vector3 val3 = ((val2.sqrMagnitude > 0.001f) ? val2.normalized : Vector3.forward);
				val += val3 * (saucerDashRadius * 0.32f);
				num3 = saucerDashRadius * 0.62f;
				damageRange = saucerDashDamageRange * 0.42f;
				num4 = Mathf.Max(3, comboRushDestructibleCap / 4);
				num5 = 0.64f;
				flag = true;
				num6 = 0.18f;
				break;
			}
			case FormActiveSkill.CrusherSlam:
				num3 = crusherSlamRadius * 0.74f;
				damageRange = crusherSlamDamageRange * 0.48f;
				num4 = Mathf.Max(6, crusherSlamDestructibleCap / 3);
				edgeChance = 0.66f;
				centerChance = 1f;
				num5 = crusherSlamStreetBreakChance * 0.86f;
				num6 = 0.26f;
				break;
			}
			num3 *= (drillSynergy ? 1.06f : 1f) * (overdriveSynergy ? 1.1f : 1f);
			num4 += (drillSynergy ? 1 : 0) + (overdriveSynergy ? 2 : 0);
			num5 = Mathf.Clamp01(num5 + (drillSynergy ? 0.08f : 0f) + (overdriveSynergy ? 0.1f : 0f));
			int num7 = ApplyAreaImpactToDestructibles(val, num3, damageRange, num4, edgeChance, centerChance, num2, suppressFeedback: true);
			int num8 = ApplyAreaBreakToStreetProps(val, num3, num5, flag || overdriveSynergy);
			int num9 = num7 + num8;
			float num10 = (drillSynergy ? 1.14f : 1f) * (overdriveSynergy ? 1.22f : 1f);
			int num11 = Mathf.Max(0, Mathf.RoundToInt(((float)formSkillSynergyBonusScoreBase + (float)num9 * 9f) * num10));
			if (num11 > 0)
			{
				scoreSystem?.AddScore(num11);
			}
			if (feedbackSystem == null)
			{
				feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
			}
			if ((Object)(object)feedbackSystem != (Object)null && num9 > 0)
			{
				float normalizedIntensity = Mathf.Clamp01(0.38f + (float)num9 * 0.05f + (overdriveSynergy ? 0.08f : 0f));
				feedbackSystem.PlayComboRushFeedback(val + Vector3.up * 0.2f, normalizedIntensity, num3 * 0.82f);
			}
			if (cameraFollowSystem == null)
			{
				cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
			}
			cameraFollowSystem?.AddImpulse(num6 * (overdriveSynergy ? 1.2f : 1f));
			string text = BuildSynergySuffix(drillSynergy, overdriveSynergy);
			if (!string.IsNullOrEmpty(comboRushAnnouncementText))
			{
				comboRushAnnouncementText += text;
				if (num11 > 0)
				{
					comboRushAnnouncementText = $"{comboRushAnnouncementText} +{num11}";
				}
			}
			comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining, GetAnnouncementDuration(AnnouncementTone.Burst, comboRushAnnouncementSeconds * 0.58f));
		}

		private int ApplyAreaImpactToDestructibles(Vector3 center, float radius, Vector2 damageRange, int maxTargets, float edgeChance, float centerChance, float chainBoost, bool suppressFeedback)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			RefreshDestructibleCache();
			float num = radius * radius;
			int num2 = Mathf.Max(1, maxTargets);
			float num3 = Mathf.Min(damageRange.x, damageRange.y);
			float num4 = Mathf.Max(damageRange.x, damageRange.y);
			float num5 = Mathf.Max(0.3f, chainBoost);
			int num6 = 0;
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				if (num6 >= num2)
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
					float num7 = Mathf.Sqrt(sqrMagnitude);
					float num8 = 1f - Mathf.Clamp01(num7 / Mathf.Max(0.01f, radius));
					float num9 = Mathf.Lerp(Mathf.Clamp01(edgeChance), Mathf.Clamp01(centerChance), num8);
					if (!(Random.value > num9))
					{
						float damage = Mathf.Lerp(num3, num4, num8) * num5;
						Vector3 hitPoint = ((Component)dummyDestructibleBlock).transform.position + Vector3.up * 0.45f;
						dummyDestructibleBlock.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.5f, 1f, num8), suppressFeedback);
						num6++;
					}
				}
			}
			return num6;
		}

		private int ApplyAreaBreakToStreetProps(Vector3 center, float radius, float baseChance, bool drillMode)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			DummyStreetPropReactive[] array = Object.FindObjectsByType<DummyStreetPropReactive>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			if (array == null || array.Length == 0)
			{
				return 0;
			}
			float num = radius * radius;
			float num2 = Mathf.Clamp01(baseChance);
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
						dummyStreetPropReactive.ApplyExternalBreak(hitPoint, Mathf.Lerp(0.5f, 1f, num5), drillMode);
						num3++;
					}
				}
			}
			return num3;
		}

		private void RefreshActionSkillButtons(bool force)
		{
			FormActiveSkill formActiveSkill = GetFormActiveSkill(GetCurrentSelectedForm());
			bool flag = enableFormActiveSkills && formActiveSkill != FormActiveSkill.None;
			bool flag2 = flag && formSkillCooldownRemaining <= 0f;
			string text = ((!flag) ? "FORM\nPASSIVE" : (flag2 ? (GetFormActiveSkillName(formActiveSkill) + "\nREADY") : $"{GetFormActiveSkillName(formActiveSkill)}\n{Mathf.CeilToInt(formSkillCooldownRemaining)}s"));
			string text2 = (flag ? "TAP: TRIGGER" : "NO ACTIVE");
			string text3 = (flag ? "AUTO" : "---");
			if (force || cachedTransformButtonLabel != text)
			{
				SetButtonLabel("TransformButton", text);
				cachedTransformButtonLabel = text;
			}
			if (force || cachedSpecial1ButtonLabel != text2)
			{
				SetButtonLabel("Special1Button", text2);
				cachedSpecial1ButtonLabel = text2;
			}
			if (force || cachedSpecial2ButtonLabel != text3)
			{
				SetButtonLabel("Special2Button", text3);
				cachedSpecial2ButtonLabel = text3;
			}
			SetButtonInteractable("TransformButton", flag && flag2 && stageRunning && !levelUpOpen);
			SetButtonInteractable("Special1Button", flag && flag2 && stageRunning && !levelUpOpen);
			SetButtonInteractable("Special2Button", interactable: false);
			bool flag3 = (actionSkillReadyVisualActive = flag && flag2 && stageRunning && !levelUpOpen);
			if ((force || flag3 != previousActionSkillReadyVisualState) && flag3)
			{
				TriggerActionSkillReadyFeedbackPulse();
			}
			if (!flag3)
			{
				actionSkillReadyPulseRemaining = 0f;
			}
			previousActionSkillReadyVisualState = flag3;
		}

		private void CacheActionSkillButtons()
		{
			if ((Object)(object)transformActionButton == (Object)null)
			{
				transformActionButtonVisualCached = false;
				transformActionButton = FindButton("TransformButton");
			}
			if ((Object)(object)supportActionButton == (Object)null)
			{
				supportActionButtonVisualCached = false;
				supportActionButton = FindButton("Special1Button");
			}
			CacheActionButtonVisualDefaults(transformActionButton, ref transformActionButtonImage, ref transformActionButtonBaseScale, ref transformActionButtonBaseColor, ref transformActionButtonVisualCached);
			CacheActionButtonVisualDefaults(supportActionButton, ref supportActionButtonImage, ref supportActionButtonBaseScale, ref supportActionButtonBaseColor, ref supportActionButtonVisualCached);
		}

		private static void CacheActionButtonVisualDefaults(Button button, ref Image image, ref Vector3 baseScale, ref Color baseColor, ref bool cached)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)button == (Object)null)
			{
				image = null;
				cached = false;
			}
			else if (!cached)
			{
				baseScale = ((Component)button).transform.localScale;
				image = ((Component)button).GetComponent<Image>();
				if ((Object)(object)image != (Object)null)
				{
					baseColor = image.color;
				}
				cached = true;
			}
		}

		private void UpdateActionSkillButtonFeedback(float deltaTime)
		{
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			if (enableActionSkillButtonFeedback)
			{
				CacheActionSkillButtons();
				actionSkillReadyPulseRemaining = Mathf.Max(0f, actionSkillReadyPulseRemaining - Mathf.Max(0f, deltaTime));
				actionSkillTapConfirmRemaining = Mathf.Max(0f, actionSkillTapConfirmRemaining - Mathf.Max(0f, deltaTime));
				bool flag = actionSkillReadyVisualActive && actionSkillTapConfirmRemaining <= 0f;
				float readyPulseNormalized = ((actionSkillReadyPulseRemaining > 0f) ? (actionSkillReadyPulseRemaining / Mathf.Max(0.01f, actionSkillReadyPulseDuration)) : 0f);
				float confirmNormalized = ((actionSkillTapConfirmRemaining > 0f) ? (actionSkillTapConfirmRemaining / Mathf.Max(0.01f, actionSkillTapConfirmDuration)) : 0f);
				float idleWave = 0f;
				if (flag)
				{
					idleWave = (Mathf.Sin(Time.unscaledTime * Mathf.Max(0.5f, actionSkillReadyIdlePulseSpeed)) + 1f) * 0.5f;
				}
				ApplyActionButtonFeedbackVisual(transformActionButton, transformActionButtonImage, transformActionButtonBaseScale, transformActionButtonBaseColor, flag, idleWave, readyPulseNormalized, confirmNormalized);
				ApplyActionButtonFeedbackVisual(supportActionButton, supportActionButtonImage, supportActionButtonBaseScale, supportActionButtonBaseColor, flag, idleWave, readyPulseNormalized, confirmNormalized);
			}
		}

		private void ApplyActionButtonFeedbackVisual(Button button, Image image, Vector3 baseScale, Color baseColor, bool emphasizeReady, float idleWave, float readyPulseNormalized, float confirmNormalized)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)button == (Object)null))
			{
				Vector3 val = baseScale;
				Color val2 = baseColor;
				if (emphasizeReady)
				{
					float num = (0.62f + idleWave * 0.38f) * Mathf.Clamp01(actionSkillReadyTintStrength);
					float num2 = 1f + actionSkillReadyIdleScaleAmplitude * idleWave;
					val *= num2;
					val2 = Color.Lerp(val2, actionSkillReadyTint, num);
				}
				if (readyPulseNormalized > 0f)
				{
					float num3 = Mathf.Sin((1f - readyPulseNormalized) * Mathf.PI);
					float num4 = 1f + num3 * Mathf.Max(0f, actionSkillReadyPulseScaleKick);
					val *= num4;
					val2 = Color.Lerp(val2, actionSkillReadyTint, num3 * Mathf.Clamp01(actionSkillReadyTintStrength + 0.18f));
				}
				if (confirmNormalized > 0f)
				{
					float num5 = 1f - confirmNormalized;
					float num6 = ((!(num5 < 0.42f)) ? Mathf.Lerp(actionSkillTapConfirmMinScale, 1f, (num5 - 0.42f) / 0.58f) : Mathf.Lerp(1f, actionSkillTapConfirmMinScale, num5 / 0.42f));
					val *= num6;
					val2 = Color.Lerp(val2, actionSkillTapConfirmTint, confirmNormalized * Mathf.Clamp01(actionSkillTapConfirmTintStrength));
				}
				((Component)button).transform.localScale = val;
				if ((Object)(object)image != (Object)null)
				{
					image.color = val2;
				}
			}
		}

		private void TriggerActionSkillReadyFeedbackPulse()
		{
			if (enableActionSkillButtonFeedback)
			{
				actionSkillReadyPulseRemaining = Mathf.Max(0.05f, actionSkillReadyPulseDuration);
			}
		}

		private void TriggerActionSkillTapConfirmFeedback()
		{
			if (enableActionSkillButtonFeedback)
			{
				actionSkillTapConfirmRemaining = Mathf.Max(0.05f, actionSkillTapConfirmDuration);
			}
		}

		private void ResetActionSkillButtonVisuals()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			CacheActionSkillButtons();
			RestoreActionButtonVisual(transformActionButton, transformActionButtonImage, transformActionButtonBaseScale, transformActionButtonBaseColor);
			RestoreActionButtonVisual(supportActionButton, supportActionButtonImage, supportActionButtonBaseScale, supportActionButtonBaseColor);
		}

		private static void RestoreActionButtonVisual(Button button, Image image, Vector3 baseScale, Color baseColor)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)button != (Object)null)
			{
				((Component)button).transform.localScale = baseScale;
			}
			if ((Object)(object)image != (Object)null)
			{
				image.color = baseColor;
			}
		}

		private void SetButtonInteractable(string buttonName, bool interactable)
		{
			Button button = FindButton(buttonName);
			if (!((Object)(object)button == (Object)null))
			{
				button.interactable = interactable;
			}
		}
	}
}
