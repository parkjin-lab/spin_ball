using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void TriggerSeismicUpgradePulse()
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			if (!stageRunning)
			{
				return;
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if (!((Object)(object)playerTransform == (Object)null))
			{
				int num = Mathf.Max(1, seismicUpgradeCount);
				Vector3 position = playerTransform.position;
				float radius = Mathf.Max(1.2f, seismicUpgradePulseRadiusBase + Mathf.Max(0f, seismicUpgradePulseRadiusPerLevel) * (float)(num - 1));
				Vector2 val = default(Vector2);
				val = new Vector2(seismicUpgradePulseDamageBase.x + Mathf.Max(0f, seismicUpgradePulseDamagePerLevel.x) * (float)(num - 1), seismicUpgradePulseDamageBase.y + Mathf.Max(0f, seismicUpgradePulseDamagePerLevel.y) * (float)(num - 1));
				val.y = Mathf.Max(val.x + 4f, val.y);
				int maxTargets = Mathf.Max(1, seismicUpgradePulseTargetCapBase + Mathf.Max(0, seismicUpgradePulseTargetCapPerLevel) * (num - 1));
				float baseChance = Mathf.Clamp01(seismicUpgradePulseStreetChanceBase + Mathf.Max(0f, seismicUpgradePulseStreetChancePerLevel) * (float)(num - 1));
				int num2 = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
				float chainBoost = Mathf.Lerp(0.95f, 1.14f, Mathf.Clamp01(((float)num2 - 1f) / 16f));
				int num3 = ApplyAreaImpactToDestructibles(position, radius, val, maxTargets, 0.58f, 1f, chainBoost, suppressFeedback: true);
				int num4 = ApplyAreaBreakToStreetProps(position, radius, baseChance, IsDrillSynergyActive());
				int num5 = num3 + num4;
				int num6 = seismicUpgradePulseBonusScoreBase + Mathf.Max(0, seismicUpgradePulseBonusScorePerLevel) * (num - 1);
				int num7 = Mathf.Max(0, num6 + num5 * 10);
				if (num7 > 0)
				{
					scoreSystem?.AddScore(num7);
				}
				if (feedbackSystem == null)
				{
					feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
				}
				if ((Object)(object)feedbackSystem != (Object)null)
				{
					float normalizedIntensity = Mathf.Clamp01(0.52f + (float)num * 0.08f + (float)num5 * 0.03f);
					feedbackSystem.PlayComboRushFeedback(position + Vector3.up * 0.22f, normalizedIntensity, radius);
				}
				if (cameraFollowSystem == null)
				{
					cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
				}
				cameraFollowSystem?.AddImpulse(Mathf.Clamp(0.34f + (float)num * 0.06f, 0.2f, 1.2f));
				PushAnnouncement($"SEISMIC CORE LV.{num} +{num7}", AnnouncementTone.Milestone, Mathf.Max(0.5f, seismicUpgradePulseAnnouncementSeconds));
			}
		}

		private void TriggerSeismicResonanceBurst(int level)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
			if (!stageRunning || level <= 0)
			{
				return;
			}
			int num = Mathf.Max(2, seismicResonanceInterval);
			if (level % num != 0)
			{
				return;
			}
			if ((Object)(object)playerTransform == (Object)null)
			{
				CacheSceneReferences();
			}
			if (!((Object)(object)playerTransform == (Object)null))
			{
				int num2 = Mathf.Max(1, level / num);
				Vector3 position = playerTransform.position;
				float num3 = Mathf.Max(1.2f, seismicUpgradePulseRadiusBase + Mathf.Max(0f, seismicUpgradePulseRadiusPerLevel) * (float)(level - 1));
				float num4 = Mathf.Max(1.05f, seismicResonanceRadiusMultiplier + (float)(num2 - 1) * 0.08f);
				float radius = num3 * num4;
				float num5 = (seismicUpgradePulseDamageBase.x + Mathf.Max(0f, seismicUpgradePulseDamagePerLevel.x) * (float)(level - 1)) * Mathf.Max(1.1f, seismicResonanceDamageMultiplier);
				float num6 = (seismicUpgradePulseDamageBase.y + Mathf.Max(0f, seismicUpgradePulseDamagePerLevel.y) * (float)(level - 1)) * Mathf.Max(1.1f, seismicResonanceDamageMultiplier + (float)num2 * 0.06f);
				num6 = Mathf.Max(num5 + 8f, num6);
				Vector2 damageRange = default(Vector2);
				damageRange = new Vector2(num5, num6);
				int maxTargets = Mathf.Max(2, seismicUpgradePulseTargetCapBase + Mathf.Max(0, seismicUpgradePulseTargetCapPerLevel) * (level - 1) + Mathf.Max(0, seismicResonanceTargetBonus) + num2 * 2);
				float baseChance = Mathf.Clamp01(seismicUpgradePulseStreetChanceBase + Mathf.Max(0f, seismicUpgradePulseStreetChancePerLevel) * (float)(level - 1) + Mathf.Max(0f, seismicResonanceStreetChanceBonus));
				int num7 = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
				float num8 = Mathf.Lerp(1f, 1.22f, Mathf.Clamp01(((float)num7 - 1f) / 14f));
				num8 *= (overdriveActive ? 1.08f : 1f);
				int num9 = ApplyAreaImpactToDestructibles(position, radius, damageRange, maxTargets, 0.7f, 1f, num8, suppressFeedback: true);
				int num10 = ApplyAreaBreakToStreetProps(position, radius, baseChance, drillMode: true);
				int num11 = num9 + num10;
				int num12 = Mathf.Max(0, Mathf.Max(0, seismicResonanceScoreBonusBase) * num2 + num11 * 18);
				if (num12 > 0)
				{
					scoreSystem?.AddScore(num12);
				}
				if (feedbackSystem == null)
				{
					feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
				}
				if ((Object)(object)feedbackSystem != (Object)null)
				{
					float normalizedIntensity = Mathf.Clamp01(0.68f + (float)num2 * 0.09f + (float)num11 * 0.035f);
					feedbackSystem.PlayComboRushFeedback(position + Vector3.up * 0.26f, normalizedIntensity, radius);
				}
				if (cameraFollowSystem == null)
				{
					cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
				}
				cameraFollowSystem?.AddImpulse(Mathf.Clamp(0.58f + (float)num2 * 0.1f, 0.2f, 1.4f));
				PushAnnouncement($"SEISMIC RESONANCE x{num2} +{num12}", AnnouncementTone.Destruction, Mathf.Max(0.65f, seismicResonanceAnnouncementSeconds));
			}
		}
	}
}


