using AlienCrusher.Gameplay;
using UnityEngine;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void ResetAlleyLineRuntime()
		{
			alleyLineLastBreakTime = -10f;
			alleyLineChainCount = 0;
			alleyLineLastBreakPosition = Vector3.zero;
			alleyLineHasAnchor = false;
			alleyLineChainStartPosition = Vector3.zero;
			alleyLineHasStartPosition = false;
			stripClearAwardedThisChain = false;
			retailFrenzyRemaining = 0f;
			retailFrenzyActivationCount = 0;
			stageStripClearCount = 0;
			stageStripClearTarget = GetStripClearMissionTarget();
			stripClearMissionCompleted = false;
			stripClearMissionScoreAwarded = 0;
			stripClearMissionDpAwarded = 0;
		}

		private void HandleSmallPropDestroyed(DummyDestructibleBlock.SmallPropBreakInfo info)
		{
			if (!Application.isPlaying || (!enableAlleyLineBonus && !enableRetailFrenzy) || !stageRunning || levelUpOpen)
			{
				return;
			}

			float num = Time.time;
			float num2 = Mathf.Max(0.2f, alleyLineWindowSeconds);
			float num3 = Mathf.Max(0.8f, alleyLineLinkRadius);
			bool flag = alleyLineHasAnchor && num - alleyLineLastBreakTime <= num2 && Vector3.Distance(alleyLineLastBreakPosition, info.Position) <= num3;
			alleyLineChainCount = (flag ? (alleyLineChainCount + 1) : 1);
			if (!flag || !alleyLineHasStartPosition)
			{
				alleyLineChainStartPosition = info.Position;
				alleyLineHasStartPosition = true;
				stripClearAwardedThisChain = false;
			}
			alleyLineLastBreakTime = num;
			alleyLineLastBreakPosition = info.Position;
			alleyLineHasAnchor = true;

			if (!enableAlleyLineBonus)
			{
				HandleRetailFrenzy(info);
				return;
			}

			int num4 = Mathf.Max(2, alleyLineMinHitsForBonus);
			if (alleyLineChainCount < num4)
			{
				HandleRetailFrenzy(info);
				return;
			}

			int num5 = Mathf.Max(0, alleyLineBaseScore + Mathf.Max(0, alleyLineChainCount - num4) * Mathf.Max(0, alleyLinePerHitScore));
			if (alleyLineChainCount >= Mathf.Max(num4 + 2, alleyLineJackpotHits))
			{
				num5 += Mathf.Max(0, alleyLineJackpotBonusScore);
			}

			if (num5 > 0)
			{
				scoreSystem?.AddScore(num5);
			}

			damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();
			if (damageNumberSystem != null)
			{
				string text = BuildAlleyLineTag(info.Kind, alleyLineChainCount, num5);
				damageNumberSystem.ShowTag(info.Position + Vector3.up * 0.9f, text, alleyLineChainCount >= Mathf.Max(num4 + 1, alleyLineJackpotHits - 1));
			}

			feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
			if (feedbackSystem != null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.34f + (float)alleyLineChainCount * 0.1f + info.Impact01 * 0.2f);
				feedbackSystem.PlayComboRushFeedback(info.Position + Vector3.up * 0.16f, normalizedIntensity, Mathf.Lerp(1.8f, 4.6f, normalizedIntensity));
			}

			PushAnnouncement(BuildAlleyLineAnnouncement(alleyLineChainCount, num5, info.Kind), AnnouncementTone.Burst, Mathf.Lerp(0.55f, 1.15f, Mathf.Clamp01((float)alleyLineChainCount / 6f)));
			chainPulse = 1f;

			HandleStripClearBonus(info);
			HandleRetailFrenzy(info);
		}

		private void HandleStripClearBonus(DummyDestructibleBlock.SmallPropBreakInfo info)
		{
			if (!enableStripClearBonus || !IsShopKind(info.Kind) || stripClearAwardedThisChain || !alleyLineHasStartPosition)
			{
				return;
			}

			int requiredHits = Mathf.Max(4, stripClearRequiredHits);
			if (alleyLineChainCount < requiredHits)
			{
				return;
			}

			float travel = Vector3.Distance(alleyLineChainStartPosition, info.Position);
			if (travel < Mathf.Max(1.4f, stripClearTravelDistance))
			{
				return;
			}

			stripClearAwardedThisChain = true;
			stageStripClearCount++;
			int bonusScore = Mathf.Max(0, stripClearBonusScore + Mathf.Max(0, alleyLineChainCount - requiredHits) * 32);
			if (bonusScore > 0)
			{
				scoreSystem?.AddScore(bonusScore);
			}

			damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();
			if (damageNumberSystem != null)
			{
				damageNumberSystem.ShowRetailTag(info.Position + Vector3.up * 1.22f, $"STRIP CLEAR +{bonusScore}", true);
			}

			feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
			if (feedbackSystem != null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.72f + (float)Mathf.Max(0, alleyLineChainCount - requiredHits) * 0.05f + info.Impact01 * 0.18f);
				feedbackSystem.PlayRetailFrenzyFeedback(info.Position + Vector3.up * 0.26f, normalizedIntensity, Mathf.Lerp(3.8f, 6.2f, normalizedIntensity));
			}

			PushAnnouncement($"STRIP CLEAR x{alleyLineChainCount} +{bonusScore}", AnnouncementTone.Milestone, 1.05f);
			chainPulse = 1f;
			TryCompleteStripClearMission(info);
			UpdateHudInfo();
		}

		private void TryCompleteStripClearMission(DummyDestructibleBlock.SmallPropBreakInfo info)
		{
			if (!enableStripClearMission || stripClearMissionCompleted || stageStripClearTarget <= 0 || stageStripClearCount < stageStripClearTarget)
			{
				return;
			}

			stripClearMissionCompleted = true;
			stripClearMissionScoreAwarded = Mathf.Max(0, stripClearMissionCompleteScore + Mathf.Max(0, stageStripClearTarget - 1) * 80);
			stripClearMissionDpAwarded = Mathf.Max(0, stripClearMissionCompleteDp + Mathf.Max(0, currentStageNumber - 1) / 2);
			if (stripClearMissionScoreAwarded > 0)
			{
				scoreSystem?.AddScore(stripClearMissionScoreAwarded);
			}

			damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();
			if (damageNumberSystem != null)
			{
				damageNumberSystem.ShowRetailTag(info.Position + Vector3.up * 1.32f, $"MISSION CLEAR +{stripClearMissionScoreAwarded}", true);
			}

			feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
			if (feedbackSystem != null)
			{
				feedbackSystem.PlayRetailFrenzyFeedback(info.Position + Vector3.up * 0.28f, 1f, 6.4f);
			}

			PushAnnouncement($"STRIP MISSION CLEAR +{stripClearMissionScoreAwarded}  DP+{stripClearMissionDpAwarded}", AnnouncementTone.Milestone, 1.2f);
			chainPulse = 1f;
		}

		private int GetStripClearMissionTarget()
		{
			if (!enableStripClearMission)
			{
				return 0;
			}

			int stage = Mathf.Max(1, currentStageNumber);
			if (stage <= 2)
			{
				return Mathf.Max(0, stripClearMissionBaseTarget);
			}
			if (stage <= 5)
			{
				return Mathf.Max(stripClearMissionBaseTarget, stripClearMissionHighStageTarget);
			}

			return Mathf.Max(stripClearMissionHighStageTarget, stripClearMissionBaseTarget + 1);
		}

		private bool HasActiveStripClearMission()
		{
			return stageStripClearTarget > 0;
		}

		private void HandleRetailFrenzy(DummyDestructibleBlock.SmallPropBreakInfo info)
		{
			if (!enableRetailFrenzy || !IsShopKind(info.Kind))
			{
				return;
			}

			float duration = Mathf.Max(1.5f, retailFrenzyDuration);
			int triggerHits = Mathf.Max(3, retailFrenzyTriggerHits);
			bool frenzyActive = retailFrenzyRemaining > 0f;
			if (!frenzyActive && alleyLineChainCount >= triggerHits)
			{
				retailFrenzyRemaining = duration;
				retailFrenzyActivationCount++;

				int activationScore = Mathf.Max(0, retailFrenzyActivationScore + Mathf.Max(0, alleyLineChainCount - triggerHits) * 10);
				if (activationScore > 0)
				{
					scoreSystem?.AddScore(activationScore);
				}

				damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();
				if (damageNumberSystem != null)
				{
					damageNumberSystem.ShowRetailTag(info.Position + Vector3.up * 1.15f, $"RETAIL FRENZY +{activationScore}", true);
				}

				feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
				if (feedbackSystem != null)
				{
					float normalizedIntensity2 = Mathf.Clamp01(0.64f + info.Impact01 * 0.26f);
					feedbackSystem.PlayRetailFrenzyFeedback(info.Position + Vector3.up * 0.24f, normalizedIntensity2, Mathf.Lerp(3.2f, 5.8f, normalizedIntensity2));
				}

				PushAnnouncement($"RETAIL FRENZY {duration:0.0}s +{activationScore}", AnnouncementTone.Milestone, Mathf.Max(1f, comboRushAnnouncementSeconds));
				chainPulse = 1f;
				frenzyActive = true;
			}

			if (!frenzyActive)
			{
				return;
			}

			retailFrenzyRemaining = Mathf.Max(retailFrenzyRemaining, duration * Mathf.Lerp(0.45f, 0.75f, info.Impact01));
			int hitScore = Mathf.Max(0, retailFrenzyShopHitScore + Mathf.Max(0, alleyLineChainCount - triggerHits) * Mathf.Max(0, retailFrenzyChainBonusPerHit));
			if (alleyLineChainCount >= Mathf.Max(triggerHits + 2, retailFrenzyJackpotHits))
			{
				hitScore += Mathf.Max(0, retailFrenzyJackpotScore);
			}

			if (hitScore > 0)
			{
				scoreSystem?.AddScore(hitScore);
			}

			damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();
			if (damageNumberSystem != null)
			{
				damageNumberSystem.ShowRetailTag(info.Position + Vector3.up * 1.02f, BuildRetailFrenzyTag(info.Kind, hitScore), alleyLineChainCount >= Mathf.Max(triggerHits + 1, retailFrenzyJackpotHits - 1));
			}

			feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
			if (feedbackSystem != null)
			{
				float normalizedIntensity = Mathf.Clamp01(0.46f + (float)Mathf.Max(0, alleyLineChainCount - 1) * 0.08f + info.Impact01 * 0.22f);
				feedbackSystem.PlayRetailFrenzyFeedback(info.Position + Vector3.up * 0.2f, normalizedIntensity, Mathf.Lerp(2.4f, 5.2f, normalizedIntensity));
			}

			PushAnnouncement(BuildRetailFrenzyAnnouncement(hitScore), AnnouncementTone.Burst, 0.82f);
			chainPulse = 1f;
		}

		private void UpdateRetailFrenzy(float deltaTime)
		{
			if (retailFrenzyRemaining <= 0f)
			{
				return;
			}

			retailFrenzyRemaining = Mathf.Max(0f, retailFrenzyRemaining - Mathf.Max(0f, deltaTime));
			if (retailFrenzyRemaining <= 0f && comboRushAnnouncementText.StartsWith("RETAIL FRENZY"))
			{
				comboRushAnnouncementText = string.Empty;
			}
		}

		private bool IsRetailFrenzyActive()
		{
			return stageRunning && retailFrenzyRemaining > 0f;
		}

		private static bool IsShopKind(DummyDestructibleBlock.SmallPropBreakKind kind)
		{
			return kind == DummyDestructibleBlock.SmallPropBreakKind.ShopAwning
				|| kind == DummyDestructibleBlock.SmallPropBreakKind.ShopSign
				|| kind == DummyDestructibleBlock.SmallPropBreakKind.Kiosk
				|| kind == DummyDestructibleBlock.SmallPropBreakKind.Bench
				|| kind == DummyDestructibleBlock.SmallPropBreakKind.BusStop
				|| kind == DummyDestructibleBlock.SmallPropBreakKind.Vending;
		}

		private static string BuildRetailFrenzyTag(DummyDestructibleBlock.SmallPropBreakKind kind, int score)
		{
			return $"FRENZY {GetAlleyKindLabel(kind)} +{Mathf.Max(0, score)}";
		}

		private string BuildRetailFrenzyAnnouncement(int score)
		{
			return $"RETAIL FRENZY {Mathf.Max(0f, retailFrenzyRemaining):0.0}s +{Mathf.Max(0, score)}";
		}

		private static string BuildAlleyLineTag(DummyDestructibleBlock.SmallPropBreakKind kind, int chainCount, int score)
		{
			string text = GetAlleyKindLabel(kind);
			return $"{text} x{Mathf.Max(1, chainCount)} +{Mathf.Max(0, score)}";
		}

		private static string BuildAlleyLineAnnouncement(int chainCount, int score, DummyDestructibleBlock.SmallPropBreakKind kind)
		{
			string text = GetAlleyKindLabel(kind);
			bool flag = IsShopKind(kind);
			if (flag)
			{
				if (chainCount >= 5)
				{
					return $"STRIP SWEEP {text} x{chainCount} +{score}";
				}

				return $"SHOP CRUSH {text} x{chainCount} +{score}";
			}

			if (chainCount >= 5)
			{
				return $"ALLEY SWEEP {text} x{chainCount} +{score}";
			}

			return $"ALLEY LINE {text} x{chainCount} +{score}";
		}

		private static string GetAlleyKindLabel(DummyDestructibleBlock.SmallPropBreakKind kind)
		{
			switch (kind)
			{
				case DummyDestructibleBlock.SmallPropBreakKind.Mailbox:
					return "MAIL";
				case DummyDestructibleBlock.SmallPropBreakKind.Fence:
					return "FENCE";
				case DummyDestructibleBlock.SmallPropBreakKind.Shed:
					return "SHED";
				case DummyDestructibleBlock.SmallPropBreakKind.ShopAwning:
					return "AWNING";
				case DummyDestructibleBlock.SmallPropBreakKind.ShopSign:
					return "SIGN";
				case DummyDestructibleBlock.SmallPropBreakKind.Kiosk:
					return "KIOSK";
				case DummyDestructibleBlock.SmallPropBreakKind.Bench:
					return "BENCH";
				case DummyDestructibleBlock.SmallPropBreakKind.BusStop:
					return "STOP";
				case DummyDestructibleBlock.SmallPropBreakKind.Vending:
					return "VEND";
				default:
					return "ALLEY";
			}
		}
	}
}
