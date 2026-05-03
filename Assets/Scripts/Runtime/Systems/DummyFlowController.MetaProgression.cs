using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void LoadSavedStageProgress()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)formUnlockSystem != (Object)null)
			{
				currentStageNumber = Mathf.Max(1, formUnlockSystem.CurrentLobbyStage);
			}
		}

		private void RegisterStageClearProgress()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return;
			}
			int highestUnlockedStage = formUnlockSystem.HighestUnlockedStage;
			if (formUnlockSystem.RegisterClearedStage(currentStageNumber))
			{
				lastLobbyActionStatus = BuildStageClearLobbyStatus(highestUnlockedStage, formUnlockSystem.HighestUnlockedStage);
			}
		}

		private void GrantStageDpReward()
		{
			if (!stageRewardGranted)
			{
				stageRewardGranted = true;
				int score = (((Object)(object)scoreSystem != (Object)null) ? scoreSystem.CurrentScore : 0);
				lastEarnedDp = CalculateStageDpReward(score) + Mathf.Max(0, stripClearMissionDpAwarded);
				if (formUnlockSystem == null)
				{
					formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
				}
				formUnlockSystem?.AddDp(lastEarnedDp);
			}
		}

		private int CalculateStageDpReward(int score)
		{
			int num = Mathf.Max(0, Mathf.FloorToInt((float)score * 0.03f));
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return num;
			}
			float num2 = Mathf.Max(1f, formUnlockSystem.GetMetaDpRewardMultiplier());
			return Mathf.Max(0, Mathf.RoundToInt((float)num * num2));
		}

		private string BuildStageClearLobbyStatus(int previousHighestStage, int currentHighestStage)
		{
			int num = Mathf.Max(previousHighestStage + 1, currentHighestStage);
			if (previousHighestStage <= 1 && currentHighestStage > previousHighestStage)
			{
				return $"Stage {num:00} unlocked. SPIKE is now your first form target.";
			}
			if (previousHighestStage <= 2 && currentHighestStage > previousHighestStage)
			{
				return $"Stage {num:00} unlocked. RAM and SAUCER routes are now your next medium-term targets.";
			}
			if (previousHighestStage <= 3 && currentHighestStage > previousHighestStage)
			{
				return $"Stage {num:00} unlocked. BOSS-TIER progression is getting closer.";
			}
			if (currentHighestStage > previousHighestStage)
			{
				return $"Stage {num:00} unlocked. Lobby progression advanced.";
			}
			return $"Stage {currentStageNumber:00} cleared.";
		}

		private void UpdateMetaProgressUi()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)lobbyDpText != (Object)null)
			{
				int num = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.DpBalance : 0);
				int num2 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.GetMetaUpgradeLevel(FormUnlockSystem.MetaUpgradeType.SizeCore) : 0);
				int num3 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.GetMetaUpgradeLevel(FormUnlockSystem.MetaUpgradeType.ImpactCore) : 0);
				int num4 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.GetMetaUpgradeLevel(FormUnlockSystem.MetaUpgradeType.DpAmplifier) : 0);
				float num5 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.GetMetaDpRewardMultiplier() : 1f);
				int num6 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.HighestUnlockedStage : Mathf.Max(1, currentStageNumber));
				lobbyDpText.text = $"DP {num:0}  |  BEST STAGE {num6:00}\nSIZE {num2}  IMPACT {num3}  DPx{num5:0.00} (LV {num4})";
			}
			UpdateLobbyRecommendationUi();
			UpdateLobbyMetaDetailUi();
			UpdateFormButtons();
			UpdateMetaShopButtons();
		}

		private void HandleCycleMetaUpgrade()
		{
			metaUpgradePreviewIndex = (metaUpgradePreviewIndex + 1) % 3;
			UpdateMetaProgressUi();
		}

		private void HandleBuyMetaUpgrade()
		{
			TryPurchaseMetaUpgrade(GetPreviewMetaUpgradeType());
		}

		private void HandleBuyMetaSize()
		{
			TryPurchaseMetaUpgrade(FormUnlockSystem.MetaUpgradeType.SizeCore);
		}

		private void HandleBuyMetaImpact()
		{
			TryPurchaseMetaUpgrade(FormUnlockSystem.MetaUpgradeType.ImpactCore);
		}

		private void HandleBuyMetaDp()
		{
			TryPurchaseMetaUpgrade(FormUnlockSystem.MetaUpgradeType.DpAmplifier);
		}

		private void TryPurchaseMetaUpgrade(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return;
			}
			if (formUnlockSystem.IsMetaUpgradeMaxed(upgradeType))
			{
				lastLobbyActionStatus = $"{GetMetaUpgradeName(upgradeType)} is already maxed.";
				UpdateMetaProgressUi();
				return;
			}
			if (!formUnlockSystem.TryPurchaseMetaUpgrade(upgradeType, out var requiredCost))
			{
				Debug.Log((object)$"[AlienCrusher] Need {requiredCost} DP to buy {GetMetaUpgradeName(upgradeType)}. Current DP: {formUnlockSystem.DpBalance}");
				lastLobbyActionStatus = $"Need {Mathf.Max(0, requiredCost - formUnlockSystem.DpBalance):0} more DP for {GetMetaUpgradeName(upgradeType)}.";
				UpdateMetaProgressUi();
				return;
			}
			ApplyPermanentMetaUpgrades();
			lastLobbyActionStatus = $"{GetMetaUpgradeName(upgradeType)} purchased.";
			if (stageRunning)
			{
				SetHudActionButtonsVisible(ShouldShowActionButtons());
			}
			RefreshActionSkillButtons(force: true);
			UpdateMetaProgressUi();
		}

		private FormUnlockSystem.MetaUpgradeType GetPreviewMetaUpgradeType()
		{
			return (Mathf.Abs(metaUpgradePreviewIndex) % 3) switch
			{
				1 => FormUnlockSystem.MetaUpgradeType.ImpactCore, 
				2 => FormUnlockSystem.MetaUpgradeType.DpAmplifier, 
				_ => FormUnlockSystem.MetaUpgradeType.SizeCore, 
			};
		}

		private static string GetMetaUpgradeName(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			return upgradeType switch
			{
				FormUnlockSystem.MetaUpgradeType.SizeCore => "SIZE CORE", 
				FormUnlockSystem.MetaUpgradeType.ImpactCore => "IMPACT CORE", 
				FormUnlockSystem.MetaUpgradeType.DpAmplifier => "DP AMPLIFIER", 
				_ => "UPGRADE", 
			};
		}

		private static string GetMetaUpgradeShortTag(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			return upgradeType switch
			{
				FormUnlockSystem.MetaUpgradeType.SizeCore => "SIZE", 
				FormUnlockSystem.MetaUpgradeType.ImpactCore => "IMPACT", 
				FormUnlockSystem.MetaUpgradeType.DpAmplifier => "DP", 
				_ => "META", 
			};
		}

		private void UpdateMetaShopButtons()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if (!((Object)(object)formUnlockSystem == (Object)null))
			{
				FormUnlockSystem.MetaUpgradeType previewMetaUpgradeType = GetPreviewMetaUpgradeType();
				string metaUpgradeName = GetMetaUpgradeName(previewMetaUpgradeType);
				int metaUpgradeLevel = formUnlockSystem.GetMetaUpgradeLevel(previewMetaUpgradeType);
				int metaUpgradeMaxLevel = formUnlockSystem.GetMetaUpgradeMaxLevel(previewMetaUpgradeType);
				int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(previewMetaUpgradeType);
				bool flag = ShouldShowAdvancedLobbyGuidance();
				if (!flag)
				{
					SetButtonLabel("TreeButton", "META CORE\nUNLOCKS AFTER FIRST RUN");
					SetButtonLabel("ShopButton", "PLAY A RUN\nTO EARN DP");
					UpdateMetaUpgradeButtonLabel("MetaUpgrade_SizeButton", FormUnlockSystem.MetaUpgradeType.SizeCore);
					UpdateMetaUpgradeButtonLabel("MetaUpgrade_ImpactButton", FormUnlockSystem.MetaUpgradeType.ImpactCore);
					UpdateMetaUpgradeButtonLabel("MetaUpgrade_DpButton", FormUnlockSystem.MetaUpgradeType.DpAmplifier);
					UpdateLobbyMetaDetailUi();
					ApplyLobbyRecommendationButtonFocus();
					return;
				}
				string label = $"NEXT UPGRADE\n{metaUpgradeName}  LV {metaUpgradeLevel}/{metaUpgradeMaxLevel}";
				SetButtonLabel("TreeButton", label);
				string label2 = ((metaUpgradeCost <= 0) ? ("MAXED\n" + metaUpgradeName) : $"BUY {metaUpgradeCost} DP\n{metaUpgradeName}");
				SetButtonLabel("ShopButton", label2);
				UpdateMetaUpgradeButtonLabel("MetaUpgrade_SizeButton", FormUnlockSystem.MetaUpgradeType.SizeCore);
				UpdateMetaUpgradeButtonLabel("MetaUpgrade_ImpactButton", FormUnlockSystem.MetaUpgradeType.ImpactCore);
				UpdateMetaUpgradeButtonLabel("MetaUpgrade_DpButton", FormUnlockSystem.MetaUpgradeType.DpAmplifier);
				UpdateLobbyMetaDetailUi();
				ApplyLobbyRecommendationButtonFocus();
			}
		}

		private void UpdateLobbyMetaDetailUi()
		{
			if ((Object)(object)lobbyMetaDetailText == (Object)null)
			{
				return;
			}
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				lobbyMetaDetailText.text = "META DETAIL  /  Progression system offline";
				lobbyMetaDetailBaseColor = new Color(0.82f, 0.88f, 0.96f, 1f);
				ApplyLobbyMetaDetailPresentation();
				return;
			}
			if (!ShouldShowAdvancedLobbyGuidance())
			{
				lobbyMetaDetailText.text = "META DETAIL  /  Start a run, earn DP, then return here to shape your build.";
				lobbyMetaDetailBaseColor = new Color(0.72f, 0.8f, 0.92f, 1f);
				ApplyLobbyMetaDetailPresentation();
				return;
			}
			FormUnlockSystem.MetaUpgradeType previewMetaUpgradeType = GetPreviewMetaUpgradeType();
			int metaUpgradeLevel = formUnlockSystem.GetMetaUpgradeLevel(previewMetaUpgradeType);
			int metaUpgradeMaxLevel = formUnlockSystem.GetMetaUpgradeMaxLevel(previewMetaUpgradeType);
			int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(previewMetaUpgradeType);
			bool flag = metaUpgradeCost > 0 && formUnlockSystem.DpBalance >= metaUpgradeCost;
			string metaUpgradeDetailSummary = GetMetaUpgradeDetailSummary(previewMetaUpgradeType, metaUpgradeLevel, metaUpgradeCost <= 0);
			string text = metaUpgradeCost <= 0 ? "MAX LEVEL REACHED" : (flag ? $"READY  /  COST {metaUpgradeCost:0} DP" : $"NEXT COST  {metaUpgradeCost:0} DP");
			lobbyMetaDetailText.text = $"META DETAIL  /  {GetMetaUpgradeName(previewMetaUpgradeType)}  LV {metaUpgradeLevel}/{metaUpgradeMaxLevel}\n{text}\n{metaUpgradeDetailSummary}";
			lobbyMetaDetailBaseColor = flag ? new Color(0.88f, 1f, 0.88f, 1f) : new Color(0.82f, 0.88f, 0.96f, 1f);
			ApplyLobbyMetaDetailPresentation();
		}

		private string GetMetaUpgradeDetailSummary(FormUnlockSystem.MetaUpgradeType upgradeType, int currentLevel, bool isMaxed)
		{
			int num = currentLevel + 1;
			int num2 = ((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(1, formUnlockSystem.HighestUnlockedStage) : 1;
			return upgradeType switch
			{
				FormUnlockSystem.MetaUpgradeType.SizeCore => isMaxed ? "Opening sweep width is capped. Early-route cleanup is fully stabilized." : ((num2 <= 1) ? $"Best first upgrade. NEXT LV {num:0} widens opening crush coverage for low-rise routes." : $"Bigger starting size for easier early sweeps. NEXT LV {num:0} widens opening crush coverage."),
				FormUnlockSystem.MetaUpgradeType.ImpactCore => isMaxed ? "Collision damage scaling is capped. Boss and elite crack pressure is stabilized." : ((num2 <= 2) ? $"Best second-step upgrade. NEXT LV {num:0} boosts collision damage against sturdier targets." : $"Harder collisions for walls, elites, and bosses. NEXT LV {num:0} boosts collision damage output."),
				FormUnlockSystem.MetaUpgradeType.DpAmplifier => isMaxed ? "District reward scaling is capped. Long-term progression payout is stabilized." : ((num2 <= 3) ? $"Best midgame economy step. NEXT LV {num:0} raises district reward payout for unlock pacing." : $"More DP from each clear for faster unlock pacing. NEXT LV {num:0} raises district reward payout."),
				_ => "No detail available."
			};
		}

		private void UpdateMetaUpgradeButtonLabel(string buttonName, FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			if (!((Object)(object)FindButton(buttonName) == (Object)null) && !((Object)(object)formUnlockSystem == (Object)null))
			{
				if (!ShouldShowAdvancedLobbyGuidance())
				{
					SetButtonLabel(buttonName, $"{GetMetaUpgradeShortTag(upgradeType)}\nLOCKED UNTIL RUN");
					return;
				}
				int metaUpgradeLevel = formUnlockSystem.GetMetaUpgradeLevel(upgradeType);
				int metaUpgradeMaxLevel = formUnlockSystem.GetMetaUpgradeMaxLevel(upgradeType);
				int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(upgradeType);
				string metaUpgradeShortTag = GetMetaUpgradeShortTag(upgradeType);
				bool flag = upgradeType == lastRecommendedMetaUpgrade;
				string text = flag ? GetRecommendedMetaContextTag(upgradeType) : string.Empty;
				string label = ((metaUpgradeCost <= 0) ? $"{metaUpgradeShortTag}  LV {metaUpgradeLevel}/{metaUpgradeMaxLevel}\nMAX" : $"{metaUpgradeShortTag}  LV {metaUpgradeLevel}/{metaUpgradeMaxLevel}\n{metaUpgradeCost} DP");
				if (!string.IsNullOrEmpty(text))
				{
					label += $"\n{text}";
				}
				SetButtonLabel(buttonName, label);
			}
		}

		private string GetRecommendedMetaContextTag(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			string lastRunFailureBucket = GetLastRunFailureBucket();
			return upgradeType switch
			{
				FormUnlockSystem.MetaUpgradeType.SizeCore => lastRunFailureBucket == "OPENING FAILED" ? "OPENING FIX" : "EARLY FLOW",
				FormUnlockSystem.MetaUpgradeType.ImpactCore => lastRunFailureBucket == "BOSS PHASE"
					? "BOSS BREAK"
					: (lastRunFailureBucket == "FINAL PUSH FAILED"
						? "FINISH PUSH"
						: (lastRunFailureBucket == "MID-RUN DRIFT" ? "ROUTE HOLD" : "HARD TARGETS")),
				FormUnlockSystem.MetaUpgradeType.DpAmplifier => "ECONOMY",
				_ => string.Empty,
			};
		}

		private void ApplyPermanentMetaUpgrades()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			ResolvePlayerController();
			float permanentBaseScaleBonus = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.GetPermanentStartScaleBonus() : 0f);
			float permanentImpactMultiplier = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.GetPermanentImpactMultiplier() : 1f);
			ballGrowthSystem?.SetPermanentBaseScaleBonus(permanentBaseScaleBonus);
			cachedPlayerController?.SetPermanentImpactMultiplier(permanentImpactMultiplier);
		}

		private void UpdateLobbyRecommendationUi()
		{
			if ((Object)(object)lobbyRecommendationText == (Object)null)
			{
				return;
			}
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				lobbyRecommendationText.text = "RECOMMENDED UPGRADE  /  Progression system offline";
				return;
			}
			if (!ShouldShowAdvancedLobbyGuidance())
			{
				lobbyRecommendationText.text = "NEXT STEP  /  Enter the district and start crushing.\nRECOMMENDATIONS unlock after your first real run.";
				lobbyRecommendationText.color = new Color(0.86f, 0.9f, 0.96f, 1f);
				return;
			}
			int num = Mathf.Max(0, formUnlockSystem.DpBalance);
			FormUnlockSystem.MetaUpgradeType previewMetaUpgradeType = GetRecommendedMetaUpgrade(out var reason);
			lastRecommendedMetaUpgrade = previewMetaUpgradeType;
			lastRecommendedUpgradeReason = reason ?? string.Empty;
			int metaUpgradeLevel = formUnlockSystem.GetMetaUpgradeLevel(previewMetaUpgradeType);
			int metaUpgradeMaxLevel = formUnlockSystem.GetMetaUpgradeMaxLevel(previewMetaUpgradeType);
			int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(previewMetaUpgradeType);
			string text = GetMetaUpgradeName(previewMetaUpgradeType);
			string text2 = (metaUpgradeCost <= 0) ? $"MAXED ({metaUpgradeLevel}/{metaUpgradeMaxLevel})" : ((num >= metaUpgradeCost) ? $"READY  /  {metaUpgradeCost:0} DP" : $"NEED {Mathf.Max(0, metaUpgradeCost - num):0} MORE DP");
			string text3 = GetRecommendedFormUnlockHint(num);
			string text4 = string.IsNullOrWhiteSpace(lastLobbyActionStatus) ? string.Empty : $"ACTION  /  {lastLobbyActionStatus}";
			string text5 = $"NEXT STEP  /  {GetMetaUpgradeShortTag(previewMetaUpgradeType)}  LV {metaUpgradeLevel}/{metaUpgradeMaxLevel}\n{text2}";
			if (!string.IsNullOrWhiteSpace(reason))
			{
				text5 = $"{text5}\nWHY  /  {GetCompactLobbyReason(reason, text)}";
			}
			if (!string.IsNullOrWhiteSpace(text3) && !text3.Contains("is still a valid pick") && !text3.Contains("HOLD SPHERE"))
			{
				text5 = $"{text5}\n{text3}";
			}
			if (!string.IsNullOrWhiteSpace(text4) && !text4.Contains("equipped.", System.StringComparison.OrdinalIgnoreCase))
			{
				text5 = $"{text5}\n{text4}";
			}
			lobbyRecommendationText.text = text5;
			lobbyRecommendationBaseColor = (metaUpgradeCost > 0 && num >= metaUpgradeCost) ? new Color(0.9f, 0.98f, 0.9f, 1f) : new Color(0.9f, 0.93f, 0.98f, 1f);
			ApplyLobbyRecommendationPresentation();
		}

		private void UpdateLobbyRecommendationPresentation(float deltaTime)
		{
			if ((Object)(object)lobbyRecommendationText == (Object)null || currentUiViewState != UiViewState.Lobby)
			{
				return;
			}
			if (lobbyRecommendationFocusRemaining > 0f)
			{
				lobbyRecommendationFocusRemaining = Mathf.Max(0f, lobbyRecommendationFocusRemaining - Mathf.Max(0f, deltaTime));
			}
			ApplyLobbyRecommendationPresentation();
			ApplyLobbyMetaDetailPresentation();
		}

		private void ApplyLobbyRecommendationPresentation()
		{
			if ((Object)(object)lobbyRecommendationText == (Object)null)
			{
				return;
			}
			float num = Mathf.Max(0.01f, lobbyRecommendationFocusDuration);
			float num2 = Mathf.Clamp01(lobbyRecommendationFocusRemaining / num);
			float num3 = num2 * num2 * (3f - 2f * num2);
			Color color = Color.Lerp(new Color(lobbyRecommendationBaseColor.r * 0.86f, lobbyRecommendationBaseColor.g * 0.86f, lobbyRecommendationBaseColor.b * 0.86f, lobbyRecommendationBaseColor.a), Color.Lerp(lobbyRecommendationBaseColor, Color.white, 0.12f), num3);
			lobbyRecommendationText.color = color;
			float num4 = 1f + 0.045f * num3;
			((Transform)lobbyRecommendationText.rectTransform).localScale = new Vector3(num4, num4, 1f);
		}

		private void ApplyLobbyMetaDetailPresentation()
		{
			if ((Object)(object)lobbyMetaDetailText == (Object)null)
			{
				return;
			}
			float num = Mathf.Max(0.01f, lobbyRecommendationFocusDuration);
			float num2 = Mathf.Clamp01(lobbyRecommendationFocusRemaining / num);
			float num3 = num2 * num2 * (3f - 2f * num2);
			Color color = Color.Lerp(new Color(lobbyMetaDetailBaseColor.r * 0.9f, lobbyMetaDetailBaseColor.g * 0.9f, lobbyMetaDetailBaseColor.b * 0.9f, lobbyMetaDetailBaseColor.a), Color.Lerp(lobbyMetaDetailBaseColor, Color.white, 0.06f), num3);
			lobbyMetaDetailText.color = color;
			float num4 = 1f + 0.02f * num3;
			((Transform)lobbyMetaDetailText.rectTransform).localScale = new Vector3(num4, num4, 1f);
		}

		private static string GetCompactLobbyReason(string reason, string upgradeName)
		{
			if (string.IsNullOrWhiteSpace(reason))
			{
				return string.Empty;
			}
			if (reason.Contains("OPENING FAILED", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} fixes the opening.";
			}
			if (reason.Contains("RECOVERY BREAK", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} makes recovery unnecessary.";
			}
			if (reason.Contains("ROUTE HOLD", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} stabilizes post-opening routes.";
			}
			if (reason.Contains("LANE BREAK", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} extends strong openings.";
			}
			if (reason.Contains("FINAL PUSH FAILED", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} helps close the stage.";
			}
			if (reason.Contains("MID-RUN DRIFT", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} stabilizes the route.";
			}
			if (reason.Contains("BOSS PHASE", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} helps crack the boss.";
			}
			if (reason.Contains("Early districts", System.StringComparison.Ordinal) || reason.Contains("opening route", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} smooths early crush flow.";
			}
			if (reason.Contains("midgame", System.StringComparison.Ordinal) || reason.Contains("fund", System.StringComparison.Ordinal))
			{
				return $"{upgradeName} speeds long-term growth.";
			}
			return reason;
		}

		private bool ShouldShowAdvancedLobbyGuidance()
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return false;
			}
			if (!string.IsNullOrWhiteSpace(lastLobbyActionStatus))
			{
				return true;
			}
			if (stageEndResult != StageEndResult.None)
			{
				return true;
			}
			if (formUnlockSystem.DpBalance > 0)
			{
				return true;
			}
			return formUnlockSystem.HighestUnlockedStage > 1;
		}

		private float GetLastRunGoalRatio()
		{
			if (!enableStageAdvanceGoal || stageAdvanceDestroyTarget <= 0)
			{
				return 0f;
			}
			int num = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
			return Mathf.Clamp01((float)num / Mathf.Max(1f, stageAdvanceDestroyTarget));
		}

		private string GetLastRunFailureBucket()
		{
			if (stageEndResult != StageEndResult.Defeat)
			{
				return string.Empty;
			}
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				return "BOSS PHASE";
			}
			if (stageEndReason == StageEndReason.TimerExpired)
			{
				float lastRunGoalRatio = GetLastRunGoalRatio();
				if (lastRunGoalRatio < 0.45f)
				{
					return "OPENING FAILED";
				}
				if (!stageAdvanceGoalReached && lastRunGoalRatio >= Mathf.Clamp(stageAdvanceNearCompleteThreshold, 0.6f, 0.98f))
				{
					return "FINAL PUSH FAILED";
				}
				if (earlyCrushFlowBonusIndex >= 3 && !routeHoldBonusGranted)
				{
					return "ROUTE HOLD MISSED";
				}
				return "MID-RUN DRIFT";
			}
			return "RUN COLLAPSE";
		}

		private FormUnlockSystem.MetaUpgradeType GetRecommendedMetaUpgrade(out string reason)
		{
			int num = ((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(1, formUnlockSystem.HighestUnlockedStage) : 1;
			if (num <= 1)
			{
				reason = "Early districts are about opening momentum fast. SIZE CORE is the simplest first upgrade.";
				return FormUnlockSystem.MetaUpgradeType.SizeCore;
			}
			if (earlyCrushRecoveryBonusGranted)
			{
				reason = "RECOVERY BREAK last run. SIZE CORE turns late recovery into earlier LANE BREAK tempo.";
				return FormUnlockSystem.MetaUpgradeType.SizeCore;
			}
			if (routeHoldBonusGranted)
			{
				reason = "ROUTE HOLD carried the run. DP AMPLIFIER converts stable route tempo into faster form and meta growth.";
				return FormUnlockSystem.MetaUpgradeType.DpAmplifier;
			}
			if (num == 2)
			{
				reason = "Stage pressure is rising. IMPACT CORE is the safest second step for sturdier targets.";
				return FormUnlockSystem.MetaUpgradeType.ImpactCore;
			}
			if (num == 3 && ((Object)(object)scoreSystem == (Object)null || scoreSystem.HighestChain < 12))
			{
				reason = "Your build path is entering midgame. DP AMPLIFIER helps fund form unlocks and follow-up upgrades.";
				return FormUnlockSystem.MetaUpgradeType.DpAmplifier;
			}
			reason = "Push a balanced core build before the next district.";
			if (stageEndResult == StageEndResult.Defeat)
			{
				string lastRunFailureBucket = GetLastRunFailureBucket();
				switch (stageEndReason)
				{
				case StageEndReason.TimerExpired:
					if (lastRunFailureBucket == "OPENING FAILED")
					{
						reason = "OPENING FAILED last run. SIZE CORE is the cleanest fix for faster low-rise sweeps and earlier crush flow.";
						return FormUnlockSystem.MetaUpgradeType.SizeCore;
					}
					if (lastRunFailureBucket == "FINAL PUSH FAILED")
					{
						reason = "FINAL PUSH FAILED last run. IMPACT CORE helps convert late momentum into faster finish breaks.";
						return FormUnlockSystem.MetaUpgradeType.ImpactCore;
					}
					if (lastRunFailureBucket == "ROUTE HOLD MISSED")
					{
						reason = "ROUTE HOLD MISSED last run. IMPACT CORE keeps starter-lane tempo alive into sturdier mid-run blocks.";
						return FormUnlockSystem.MetaUpgradeType.ImpactCore;
					}
					reason = "MID-RUN DRIFT last run. IMPACT CORE is the safest fix when route pressure rises and sturdier blocks slow the push.";
					return FormUnlockSystem.MetaUpgradeType.ImpactCore;
				default:
					if (lastRunFailureBucket == "BOSS PHASE")
					{
						reason = "BOSS PHASE stopped the run. IMPACT CORE gives better pylon cracking and stronger core punish.";
						return FormUnlockSystem.MetaUpgradeType.ImpactCore;
					}
					reason = "Your run lost value before payoff. DP AMPLIFIER helps every future clear fund upgrades faster.";
					return FormUnlockSystem.MetaUpgradeType.DpAmplifier;
				}
			}
			int num3 = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.HighestChain) : 0);
			if (num3 >= 12)
			{
				reason = "Your momentum is already strong. DP AMPLIFIER converts clean runs into faster long-term scaling.";
				return FormUnlockSystem.MetaUpgradeType.DpAmplifier;
			}
			if (earlyCrushFlowBonusIndex >= 3)
			{
				reason = "LANE BREAK was stable last run. IMPACT CORE converts that opening speed into stronger mid-run breaks.";
				return FormUnlockSystem.MetaUpgradeType.ImpactCore;
			}
			if (currentStageNumber >= Mathf.Max(2, bossStageStart))
			{
				reason = "Pressure is rising and fortified targets are ahead. IMPACT CORE is the safest push upgrade.";
				return FormUnlockSystem.MetaUpgradeType.ImpactCore;
			}
			reason = "Early districts reward wider crush lanes. SIZE CORE makes the opening route smoother and more forgiving.";
			return FormUnlockSystem.MetaUpgradeType.SizeCore;
		}

		private void ApplyLobbyRecommendationButtonFocus()
		{
			if (!ShouldShowAdvancedLobbyGuidance())
			{
				ApplyRecommendedMetaButtonStyle("MetaUpgrade_SizeButton", (FormUnlockSystem.MetaUpgradeType)(-1));
				ApplyRecommendedMetaButtonStyle("MetaUpgrade_ImpactButton", (FormUnlockSystem.MetaUpgradeType)(-1));
				ApplyRecommendedMetaButtonStyle("MetaUpgrade_DpButton", (FormUnlockSystem.MetaUpgradeType)(-1));
				ApplyRecommendedFormButtonStyle("Form_Sphere", FormType.Sphere);
				ApplyRecommendedFormButtonStyle("Form_Spike", FormType.Sphere);
				ApplyRecommendedFormButtonStyle("Form_Ram", FormType.Sphere);
				ApplyRecommendedFormButtonStyle("Form_Saucer", FormType.Sphere);
				ApplyRecommendedFormButtonStyle("Form_Crusher", FormType.Sphere);
				return;
			}
			ApplyRecommendedMetaButtonStyle("MetaUpgrade_SizeButton", FormUnlockSystem.MetaUpgradeType.SizeCore);
			ApplyRecommendedMetaButtonStyle("MetaUpgrade_ImpactButton", FormUnlockSystem.MetaUpgradeType.ImpactCore);
			ApplyRecommendedMetaButtonStyle("MetaUpgrade_DpButton", FormUnlockSystem.MetaUpgradeType.DpAmplifier);
			ApplyRecommendedFormButtonStyle("Form_Sphere", FormType.Sphere);
			ApplyRecommendedFormButtonStyle("Form_Spike", FormType.Spike);
			ApplyRecommendedFormButtonStyle("Form_Ram", FormType.Ram);
			ApplyRecommendedFormButtonStyle("Form_Saucer", FormType.Saucer);
			ApplyRecommendedFormButtonStyle("Form_Crusher", FormType.Crusher);
		}

		private void ApplyRecommendedMetaButtonStyle(string buttonName, FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null || (Object)(object)formUnlockSystem == (Object)null)
			{
				return;
			}
			Image component = ((Component)button).GetComponent<Image>();
			if ((Object)(object)component == (Object)null)
			{
				return;
			}
			bool flag = upgradeType == lastRecommendedMetaUpgrade;
			int metaUpgradeCost = formUnlockSystem.GetMetaUpgradeCost(upgradeType);
			bool flag2 = metaUpgradeCost > 0 && formUnlockSystem.DpBalance >= metaUpgradeCost;
			bool flag3 = metaUpgradeCost <= 0;
			component.color = flag
				? (flag2 ? new Color(0.26f, 0.68f, 0.38f, 0.96f) : new Color(0.24f, 0.42f, 0.7f, 0.96f))
				: (flag3 ? new Color(0.16f, 0.2f, 0.18f, 0.72f) : (flag2 ? new Color(0.16f, 0.22f, 0.18f, 0.78f) : new Color(0.1f, 0.12f, 0.16f, 0.64f)));
		}

		private string GetRecommendedFormUnlockHint(int dpBalance)
		{
			lastRecommendedFormUnlock = GetRecommendedFormUnlock();
			string lastRunFailureBucket = GetLastRunFailureBucket();
			if (lastRecommendedFormUnlock == FormType.Sphere)
			{
				if (lastRunFailureBucket == "OPENING FAILED")
				{
					return "FORM FOCUS  /  HOLD SPHERE. Fix the opening with SIZE CORE and denser lane routing first.";
				}
				return GetStageBasedFormPathHint();
			}
			int unlockCost = formUnlockSystem.GetUnlockCost(lastRecommendedFormUnlock);
			int num = Mathf.Max(0, unlockCost - dpBalance);
			string text = lastRecommendedFormUnlock.ToString().ToUpperInvariant();
			string text2 = lastRunFailureBucket switch
			{
				"ROUTE HOLD MISSED" => "  /  route hold target",
				"MID-RUN DRIFT" => "  /  route stability target",
				"FINAL PUSH FAILED" => "  /  finish-line pressure target",
				"BOSS PHASE" => "  /  boss breach target",
				_ => string.Empty,
			};
			return num <= 0
				? $"FORM FOCUS  /  {text} READY ({unlockCost:0} DP){text2}"
				: $"FORM FOCUS  /  {text} NEXT STEP, NEED {num:0} MORE DP{text2}";
		}

		private FormType GetRecommendedFormUnlock()
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return FormType.Sphere;
			}
			string lastRunFailureBucket = GetLastRunFailureBucket();
			if (lastRunFailureBucket == "OPENING FAILED")
			{
				return FormType.Sphere;
			}
			if ((lastRunFailureBucket == "ROUTE HOLD MISSED" || lastRunFailureBucket == "MID-RUN DRIFT" || lastRunFailureBucket == "FINAL PUSH FAILED") && !formUnlockSystem.IsUnlocked(FormType.Ram))
			{
				return FormType.Ram;
			}
			if (lastRunFailureBucket == "BOSS PHASE" && !formUnlockSystem.IsUnlocked(FormType.Crusher))
			{
				return FormType.Crusher;
			}
			if (!formUnlockSystem.IsUnlocked(FormType.Spike))
			{
				return FormType.Spike;
			}
			if ((lastRunFailureBucket == "ROUTE HOLD MISSED" || lastRunFailureBucket == "MID-RUN DRIFT" || lastRunFailureBucket == "FINAL PUSH FAILED" || (stageEndResult == StageEndResult.Defeat && stageEndReason == StageEndReason.TimerExpired)) && !formUnlockSystem.IsUnlocked(FormType.Ram))
			{
				return FormType.Ram;
			}
			if ((lastRunFailureBucket == "BOSS PHASE" || stageBossEncounterActive) && !formUnlockSystem.IsUnlocked(FormType.Crusher))
			{
				return FormType.Crusher;
			}
			if (!formUnlockSystem.IsUnlocked(FormType.Saucer))
			{
				return FormType.Saucer;
			}
			if (!formUnlockSystem.IsUnlocked(FormType.Ram))
			{
				return FormType.Ram;
			}
			if (!formUnlockSystem.IsUnlocked(FormType.Crusher))
			{
				return FormType.Crusher;
			}
			return FormType.Sphere;
		}

		private string GetStageBasedFormPathHint()
		{
			int num = ((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(1, formUnlockSystem.HighestUnlockedStage) : 1;
			if (num <= 1)
			{
				return "FORM PATH  /  Clear the first district, then aim for SPIKE.";
			}
			if (num == 2)
			{
				return "FORM PATH  /  SPIKE first, then build toward RAM or SAUCER.";
			}
			if (num == 3)
			{
				return "FORM PATH  /  Mid-tier forms open route variety. CRUSHER remains the boss-tier target.";
			}
			return $"FORM PATH  /  Current {GetCurrentSelectedForm().ToString().ToUpperInvariant()} build is valid. Push deeper stages for boss-tier unlocks.";
		}

		private void ApplyRecommendedFormButtonStyle(string buttonName, FormType formType)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null || (Object)(object)formUnlockSystem == (Object)null)
			{
				return;
			}
			Image component = ((Component)button).GetComponent<Image>();
			if ((Object)(object)component == (Object)null)
			{
				return;
			}
			bool flag = formUnlockSystem.IsUnlocked(formType);
			bool flag2 = formUnlockSystem.CurrentForm == formType;
			bool flag3 = formType == lastRecommendedFormUnlock;
			bool flag4 = !flag && flag3 && formUnlockSystem.DpBalance >= formUnlockSystem.GetUnlockCost(formType);
			component.color = flag2
				? (flag3 ? new Color(0.3f, 0.62f, 0.88f, 0.98f) : new Color(0.24f, 0.52f, 0.78f, 0.96f))
				: (flag3
					? (flag
						? new Color(0.24f, 0.46f, 0.62f, 0.94f)
						: (flag4 ? new Color(0.6f, 0.34f, 0.18f, 0.96f) : new Color(0.34f, 0.24f, 0.16f, 0.94f)))
					: (flag ? new Color(0.16f, 0.22f, 0.28f, 0.72f) : new Color(0.1f, 0.12f, 0.16f, 0.64f)));
		}
	}
}

