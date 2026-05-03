using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void BindLandingShockwaveEvent(PlayerBallDummyController controller)
		{
			if (!((Object)(object)landingShockwaveEventSource == (Object)(object)controller))
			{
				if ((Object)(object)landingShockwaveEventSource != (Object)null)
				{
					landingShockwaveEventSource.LandingShockwaveTriggered -= HandleLandingShockwaveTriggered;
				}
				landingShockwaveEventSource = controller;
				if ((Object)(object)landingShockwaveEventSource != (Object)null)
				{
					landingShockwaveEventSource.LandingShockwaveTriggered -= HandleLandingShockwaveTriggered;
					landingShockwaveEventSource.LandingShockwaveTriggered += HandleLandingShockwaveTriggered;
				}
			}
		}

		private void UnbindLandingShockwaveEvent()
		{
			if (!((Object)(object)landingShockwaveEventSource == (Object)null))
			{
				landingShockwaveEventSource.LandingShockwaveTriggered -= HandleLandingShockwaveTriggered;
				landingShockwaveEventSource = null;
			}
		}

		private void HandleLandingShockwaveTriggered(PlayerBallDummyController.LandingShockwaveData data)
		{
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_032b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0330: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_033f: Unknown result type (might be due to invalid IL or missing references)
			if (!enableLandingShockwave || !stageRunning || levelUpOpen)
			{
				return;
			}
			float num = Mathf.Clamp01(data.Intensity);
			if (!(num <= 0.01f))
			{
				bool flag = overdriveActive;
				int num2 = Mathf.Max(0, seismicUpgradeCount);
				float num3 = Mathf.Lerp(landingShockwaveRadiusMin, landingShockwaveRadiusMax, num);
				num3 += Mathf.Max(0f, seismicLandingRadiusPerLevel) * (float)num2;
				num3 *= (data.DrillMode ? 1.1f : 1f) * (flag ? 1.14f : 1f);
				float num4 = Mathf.Lerp(landingShockwaveDamageRangeMin.x, landingShockwaveDamageRangeMax.x, num);
				num4 += Mathf.Max(0f, seismicLandingDamagePerLevel.x) * (float)num2;
				float num5 = Mathf.Lerp(landingShockwaveDamageRangeMin.y, landingShockwaveDamageRangeMax.y, num);
				num5 += Mathf.Max(0f, seismicLandingDamagePerLevel.y) * (float)num2;
				float num6 = Mathf.Clamp(data.ImpactMultiplier, 0.75f, 3f);
				Vector2 damageRange = default(Vector2);
				damageRange = new Vector2(num4 * num6, num5 * num6);
				int num7 = Mathf.RoundToInt(Mathf.Lerp((float)landingShockwaveTargetCapMin, (float)landingShockwaveTargetCapMax, num));
				num7 += Mathf.Max(0, seismicLandingTargetCapPerLevel) * num2;
				if (data.DrillMode)
				{
					num7 += 2;
				}
				if (flag)
				{
					num7 += 3;
				}
				int num8 = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
				float num9 = Mathf.Lerp(0.95f, 1.35f, Mathf.Clamp01(((float)num8 - 2f) / 18f));
				num9 *= (data.DrillMode ? 1.1f : 1f);
				num9 *= (flag ? 1.16f : 1f);
				int num10 = ApplyAreaImpactToDestructibles(data.Point, num3, damageRange, Mathf.Max(1, num7), 0.56f, 1f, num9, suppressFeedback: true);
				int num11 = ApplyAreaBreakToStreetProps(baseChance: Mathf.Clamp01(landingShockwaveStreetBreakChance + Mathf.Max(0f, seismicLandingStreetChancePerLevel) * (float)num2 + (data.DrillMode ? 0.11f : 0f) + (flag ? 0.08f : 0f)), center: data.Point, radius: num3, drillMode: data.DrillMode || flag);
				int num12 = num10 + num11;
				float num13 = (data.DrillMode ? 1.12f : 1f) * (flag ? 1.2f : 1f);
				int num14 = landingShockwaveBonusScoreBase + Mathf.Max(0, seismicLandingBonusScorePerLevel) * num2;
				int num15 = Mathf.Max(0, Mathf.RoundToInt(((float)num14 + (float)num12 * 13f) * num13));
				if (num15 > 0)
				{
					scoreSystem?.AddScore(num15);
				}
				if (feedbackSystem == null)
				{
					feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
				}
				if ((Object)(object)feedbackSystem != (Object)null)
				{
					float normalizedIntensity = Mathf.Clamp01(0.48f + num * 0.42f + (float)num12 * 0.02f);
					feedbackSystem.PlayComboRushFeedback(data.Point + Vector3.up * 0.2f, normalizedIntensity, num3);
				}
				if (cameraFollowSystem == null)
				{
					cameraFollowSystem = Object.FindFirstObjectByType<CameraFollowSystem>();
				}
				float magnitude = Mathf.Lerp(0.34f, 1.02f, num) * (flag ? 1.08f : 1f);
				cameraFollowSystem?.AddImpulse(magnitude);
				PushAnnouncement($"SKY CRASH{BuildSynergySuffix(data.DrillMode, flag)} +{num15}", AnnouncementTone.Destruction, Mathf.Max(comboRushAnnouncementSeconds * 0.66f, 0.62f));
			}
		}

		private static string BuildSynergySuffix(bool drillSynergy, bool overdriveSynergy)
		{
			if (drillSynergy && overdriveSynergy)
			{
				return " [DRILL+OD]";
			}
			if (drillSynergy)
			{
				return " [DRILL]";
			}
			if (overdriveSynergy)
			{
				return " [OD]";
			}
			return string.Empty;
		}
	}
}
