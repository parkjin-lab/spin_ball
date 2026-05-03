using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void RollLevelUpChoices()
		{
			List<LevelUpChoice> list = BuildLevelUpChoicePool();
			if (list.Count <= 0)
			{
				return;
			}
			HashSet<LevelUpChoice> hashSet = new HashSet<LevelUpChoice>();
			List<int> list2 = new List<int>(currentChoices.Length);
			if (levelUpHasLockedChoice)
			{
				bool num = levelUpLockedSlot >= 0 && levelUpLockedSlot < currentChoices.Length;
				bool flag = list.Contains(levelUpLockedChoice);
				if (num && flag)
				{
					currentChoices[levelUpLockedSlot] = levelUpLockedChoice;
					hashSet.Add(levelUpLockedChoice);
				}
				else
				{
					ClearLevelUpLockSelection();
				}
			}
			for (int i = 0; i < currentChoices.Length; i++)
			{
				if (!levelUpHasLockedChoice || levelUpLockedSlot != i)
				{
					list2.Add(i);
				}
			}
			if (levelUpMobilityPityThreshold > 0 && GetChoiceMissStreak(LevelUpChoice.Speed) >= levelUpMobilityPityThreshold && list.Contains(LevelUpChoice.Speed) && !hashSet.Contains(LevelUpChoice.Speed) && list2.Count > 0)
			{
				int num2 = list2[0];
				list2.RemoveAt(0);
				currentChoices[num2] = LevelUpChoice.Speed;
				hashSet.Add(LevelUpChoice.Speed);
			}
			for (int j = 0; j < list2.Count; j++)
			{
				int num3 = list2[j];
				LevelUpChoice? levelUpChoice = PickWeightedLevelUpChoice(list, hashSet);
				if (!levelUpChoice.HasValue)
				{
					levelUpChoice = PickFallbackLevelUpChoice(list, hashSet);
				}
				if (!levelUpChoice.HasValue)
				{
					levelUpChoice = list[Mathf.Abs(num3) % list.Count];
				}
				currentChoices[num3] = levelUpChoice.Value;
				hashSet.Add(levelUpChoice.Value);
			}
			UpdateLevelUpDraftHistory(list);
			UpdateLevelUpChoiceButtons();
		}

		private List<LevelUpChoice> BuildLevelUpChoicePool()
		{
			List<LevelUpChoice> list = new List<LevelUpChoice>(7)
			{
				LevelUpChoice.Speed,
				LevelUpChoice.Impact,
				LevelUpChoice.ComboPower,
				LevelUpChoice.ComboTrigger,
				LevelUpChoice.Shockwave
			};
			if (drillUpgradeCount <= 0)
			{
				list.Add(LevelUpChoice.Drill);
			}
			if (enableTrafficSimulation && enableTrafficPanic)
			{
				list.Add(LevelUpChoice.TrafficPanic);
			}
			return list;
		}

		private void RecalculateNextComboRushGate()
		{
			int num = Mathf.Max(2, comboRushChainStep);
			int num2 = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.ChainCount) : 0);
			nextComboRushChainGate = Mathf.Max(num, (num2 / num + 1) * num);
		}

		private LevelUpChoice? PickWeightedLevelUpChoice(List<LevelUpChoice> options, HashSet<LevelUpChoice> usedChoices)
		{
			if (options == null || options.Count <= 0)
			{
				return null;
			}
			LevelUpChoice[] array = new LevelUpChoice[options.Count];
			float[] array2 = new float[options.Count];
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < options.Count; i++)
			{
				LevelUpChoice levelUpChoice = options[i];
				if (!usedChoices.Contains(levelUpChoice))
				{
					float levelUpChoiceWeight = GetLevelUpChoiceWeight(levelUpChoice);
					if (!(levelUpChoiceWeight <= 0f))
					{
						num2 += levelUpChoiceWeight;
						array[num] = levelUpChoice;
						array2[num] = num2;
						num++;
					}
				}
			}
			if (num <= 0 || num2 <= 0f)
			{
				return null;
			}
			float num3 = (float)levelUpRng.NextDouble() * num2;
			for (int j = 0; j < num; j++)
			{
				if (num3 <= array2[j])
				{
					return array[j];
				}
			}
			return array[num - 1];
		}

		private static LevelUpChoice? PickFallbackLevelUpChoice(List<LevelUpChoice> options, HashSet<LevelUpChoice> usedChoices)
		{
			if (options == null)
			{
				return null;
			}
			for (int i = 0; i < options.Count; i++)
			{
				LevelUpChoice levelUpChoice = options[i];
				if (!usedChoices.Contains(levelUpChoice))
				{
					return levelUpChoice;
				}
			}
			return null;
		}

		private float GetLevelUpChoiceWeight(LevelUpChoice choice)
		{
			float num = 1f;
			if (hasPreviousLevelUpChoices && ContainsChoice(previousLevelUpChoices, choice))
			{
				num *= 0.45f;
			}
			if (choice == LevelUpChoice.Speed && levelUpMobilityPityThreshold > 0 && GetChoiceMissStreak(LevelUpChoice.Speed) >= Mathf.Max(1, levelUpMobilityPityThreshold - 1))
			{
				num *= 1.9f;
			}
			if (choice == LevelUpChoice.TrafficPanic)
			{
				if (!enableTrafficSimulation || !enableTrafficPanic)
				{
					return 0f;
				}
				num *= Mathf.Lerp(0.92f, 1.45f, Mathf.Clamp01((float)trafficPanicUpgradeCount / 6f));
				if (trafficPanicChainStack >= 2)
				{
					num *= 1.08f;
				}
			}
			return Mathf.Max(0.05f, num);
		}

		private static bool ContainsChoice(LevelUpChoice[] source, LevelUpChoice choice)
		{
			if (source == null)
			{
				return false;
			}
			for (int i = 0; i < source.Length; i++)
			{
				if (source[i] == choice)
				{
					return true;
				}
			}
			return false;
		}

		private void UpdateLevelUpDraftHistory(List<LevelUpChoice> availableChoices)
		{
			HashSet<LevelUpChoice> hashSet = new HashSet<LevelUpChoice>();
			for (int i = 0; i < currentChoices.Length; i++)
			{
				LevelUpChoice levelUpChoice = currentChoices[i];
				hashSet.Add(levelUpChoice);
				previousLevelUpChoices[i] = levelUpChoice;
			}
			hasPreviousLevelUpChoices = true;
			for (int j = 0; j < levelUpChoiceMissStreak.Length; j++)
			{
				LevelUpChoice item = (LevelUpChoice)j;
				if (availableChoices == null || !availableChoices.Contains(item))
				{
					levelUpChoiceMissStreak[j] = 0;
				}
				else if (hashSet.Contains(item))
				{
					levelUpChoiceMissStreak[j] = 0;
				}
				else
				{
					levelUpChoiceMissStreak[j] = Mathf.Min(99, levelUpChoiceMissStreak[j] + 1);
				}
			}
		}

		private int GetChoiceMissStreak(LevelUpChoice choice)
		{
			int num = Mathf.Clamp((int)choice, 0, levelUpChoiceMissStreak.Length - 1);
			return levelUpChoiceMissStreak[num];
		}

		private void UpdateLevelUpChoiceButtons()
		{
			for (int i = 0; i < currentChoices.Length; i++)
			{
				string buttonName = i switch
				{
					1 => "SkillOption_B",
					0 => "SkillOption_A",
					_ => "SkillOption_C"
				};
				string text = GetChoiceLabel(currentChoices[i]);
				if (levelUpHasLockedChoice && levelUpLockedSlot == i)
				{
					text = "[LOCK]\n" + text;
				}
				SetButtonLabel(buttonName, text);
				ApplyChoiceButtonVisual(buttonName, currentChoices[i]);
				string buttonName2 = i switch
				{
					1 => "SkillLock_B",
					0 => "SkillLock_A",
					_ => "SkillLock_C"
				};
				string label = ((levelUpHasLockedChoice && levelUpLockedSlot == i) ? "LOCKED" : "LOCK");
				SetButtonLabel(buttonName2, label);
			}
			string label2 = (enableLevelUpReroll ? $"REROLL x{levelUpRerollsRemaining}" : "REROLL");
			SetButtonLabel("SkillRerollButton", label2);
		}

		private string GetChoiceLabel(LevelUpChoice choice)
		{
			switch (choice)
			{
			case LevelUpChoice.Speed:
			{
				float num9 = Mathf.Pow(1.1f, (float)speedUpgradeCount);
				float num10 = num9 * 1.1f;
				return $"SPEED\n{Mathf.RoundToInt(num9 * 100f)}% -> {Mathf.RoundToInt(num10 * 100f)}%";
			}
			case LevelUpChoice.Impact:
			{
				float num11 = Mathf.Pow(1.2f, (float)impactUpgradeCount);
				float num12 = num11 * 1.2f;
				return $"IMPACT\n{Mathf.RoundToInt(num11 * 100f)}% -> {Mathf.RoundToInt(num12 * 100f)}%";
			}
			case LevelUpChoice.Drill:
				if (drillUpgradeCount <= 0)
				{
					return "DRILL FORM\nUNLOCK";
				}
				return "DRILL FORM\nACTIVE";
			case LevelUpChoice.ComboPower:
			{
				float num7 = Mathf.Clamp(comboRushDamageRange.x + 8f, 16f, 260f);
				float num8 = Mathf.Clamp(comboRushDamageRange.y + 22f, 36f, 420f);
				if (num8 < num7 + 10f)
				{
					num8 = num7 + 10f;
				}
				return $"RUSH POWER\nDMG {Mathf.RoundToInt(comboRushDamageRange.x)}-{Mathf.RoundToInt(comboRushDamageRange.y)} -> {Mathf.RoundToInt(num7)}-{Mathf.RoundToInt(num8)}";
			}
			case LevelUpChoice.ComboTrigger:
			{
				int num5 = Mathf.Max(3, comboRushChainStep - 1);
				float num6 = Mathf.Clamp(comboRushRadius + 0.45f, 3.2f, 14f);
				return $"RUSH ENGINE\nTRIGGER {comboRushChainStep}->{num5}  RAD {comboRushRadius:0.0}->{num6:0.0}";
			}
			case LevelUpChoice.Shockwave:
			{
				float num = (float)seismicUpgradeCount * Mathf.Max(0f, seismicLandingRadiusPerLevel);
				float num2 = (float)(seismicUpgradeCount + 1) * Mathf.Max(0f, seismicLandingRadiusPerLevel);
				float num3 = 1f + (float)seismicUpgradeCount * Mathf.Max(0f, seismicBuildingShockwaveDamageBonusPerLevel);
				float num4 = 1f + (float)(seismicUpgradeCount + 1) * Mathf.Max(0f, seismicBuildingShockwaveDamageBonusPerLevel);
				return $"SEISMIC CORE\nLAND +{num:0.0}->{num2:0.0}  BREAK x{num3:0.00}->{num4:0.00}";
			}
			case LevelUpChoice.TrafficPanic:
			{
				float trafficPanicMasteryGainScale = GetTrafficPanicMasteryGainScale(trafficPanicUpgradeCount + 1);
				int num13 = Mathf.Max(1, Mathf.RoundToInt((float)trafficPanicMasteryBonusBaseAdd * trafficPanicMasteryGainScale));
				int num14 = Mathf.Max(1, Mathf.RoundToInt((float)trafficPanicMasteryBonusPerVehicleAdd * trafficPanicMasteryGainScale));
				float num15 = Mathf.Clamp(trafficPanicRadius + trafficPanicMasteryRadiusAdd * trafficPanicMasteryGainScale, 3.5f, 40f);
				float num16 = Mathf.Clamp(trafficPanicBonusCooldown - trafficPanicMasteryCooldownReduction * trafficPanicMasteryGainScale, 0.12f, 5f);
				return $"CHAOS ENGINE\n+{num13}/{num14}  RAD {trafficPanicRadius:0.0}->{num15:0.0}  CD {trafficPanicBonusCooldown:0.00}->{num16:0.00}";
			}
			default:
				return "UNKNOWN";
			}
		}

		private void SetButtonLabel(string buttonName, string label)
		{
			Button button = FindButton(buttonName);
			if (!((Object)(object)button == (Object)null))
			{
				Text text = null;
				Transform val = ((Component)button).transform.Find("Label");
				if ((Object)(object)val != (Object)null)
				{
					text = ((Component)val).GetComponent<Text>();
				}
				if ((Object)(object)text == (Object)null)
				{
					text = ((Component)button).GetComponentInChildren<Text>(true);
				}
				if ((Object)(object)text != (Object)null)
				{
					text.text = label;
					text.alignment = (TextAnchor)4;
					text.horizontalOverflow = (HorizontalWrapMode)0;
					text.verticalOverflow = (VerticalWrapMode)1;
					text.lineSpacing = 0.9f;
					text.resizeTextForBestFit = true;
					text.resizeTextMinSize = 14;
					text.resizeTextMaxSize = 34;
				}
			}
		}

		private void ApplyChoiceButtonVisual(string buttonName, LevelUpChoice choice)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			Button button = FindButton(buttonName);
			if (!((Object)(object)button == (Object)null))
			{
				Color choiceBackgroundColor = GetChoiceBackgroundColor(choice);
				Image component = ((Component)button).GetComponent<Image>();
				if ((Object)(object)component != (Object)null)
				{
					component.color = choiceBackgroundColor;
				}
				ColorBlock colors = button.colors;
				colors.normalColor = choiceBackgroundColor;
				colors.highlightedColor = Color.Lerp(choiceBackgroundColor, Color.white, 0.14f);
				colors.pressedColor = Color.Lerp(choiceBackgroundColor, Color.black, 0.22f);
				colors.selectedColor = colors.highlightedColor;
				button.colors = colors;
				Text text = null;
				Transform val = ((Component)button).transform.Find("Label");
				if ((Object)(object)val != (Object)null)
				{
					text = ((Component)val).GetComponent<Text>();
				}
				if ((Object)(object)text == (Object)null)
				{
					text = ((Component)button).GetComponentInChildren<Text>(true);
				}
				if ((Object)(object)text != (Object)null)
				{
					text.color = choiceTextColor;
				}
				Text text2 = EnsureChoiceTagText(((Component)button).transform, ((Object)(object)text != (Object)null) ? text.font : null);
				if ((Object)(object)text2 != (Object)null)
				{
					text2.text = GetChoiceTag(choice);
					text2.color = choiceTagTextColor;
				}
			}
		}

		private static string GetChoiceTag(LevelUpChoice choice)
		{
			switch (choice)
			{
			case LevelUpChoice.Speed:
				return "MOBILITY";
			case LevelUpChoice.Impact:
				return "POWER";
			case LevelUpChoice.Drill:
				return "FORM";
			case LevelUpChoice.ComboPower:
			case LevelUpChoice.ComboTrigger:
				return "RUSH";
			case LevelUpChoice.Shockwave:
				return "SEISMIC";
			case LevelUpChoice.TrafficPanic:
				return "CHAOS";
			default:
				return "SKILL";
			}
		}

		private Color GetChoiceBackgroundColor(LevelUpChoice choice)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			switch (choice)
			{
			case LevelUpChoice.Speed:
				return choiceMobilityColor;
			case LevelUpChoice.Impact:
				return choicePowerColor;
			case LevelUpChoice.Drill:
				return choiceFormColor;
			case LevelUpChoice.ComboPower:
			case LevelUpChoice.ComboTrigger:
				return choiceRushColor;
			case LevelUpChoice.Shockwave:
				return choicePowerColor;
			case LevelUpChoice.TrafficPanic:
				return Color.Lerp(choiceRushColor, new Color(0.82f, 0.4f, 0.18f, 0.97f), 0.46f);
			default:
				return choicePowerColor;
			}
		}

		private static Text EnsureChoiceTagText(Transform buttonTransform, Font fallbackFont)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)buttonTransform == (Object)null)
			{
				return null;
			}
			Transform val = buttonTransform.Find("ChoiceTag");
			if ((Object)(object)val == (Object)null)
			{
				GameObject val2 = new GameObject("ChoiceTag", new Type[1] { typeof(RectTransform) });
				val2.transform.SetParent(buttonTransform, false);
				val = val2.transform;
			}
			RectTransform val3 = (RectTransform)(object)((val is RectTransform) ? val : null);
			if ((Object)(object)val3 != (Object)null)
			{
				val3.anchorMin = new Vector2(0f, 1f);
				val3.anchorMax = new Vector2(1f, 1f);
				val3.pivot = new Vector2(0f, 1f);
				val3.sizeDelta = new Vector2(-18f, 26f);
				val3.anchoredPosition = new Vector2(10f, -8f);
			}
			Text text = ((Component)val).GetComponent<Text>();
			if ((Object)(object)text == (Object)null)
			{
				text = ((Component)val).gameObject.AddComponent<Text>();
			}
			text.font = fallbackFont ?? text.font ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.fontSize = 16;
			text.alignment = (TextAnchor)0;
			text.horizontalOverflow = (HorizontalWrapMode)1;
			text.verticalOverflow = (VerticalWrapMode)1;
			text.raycastTarget = false;
			return text;
		}
	}
}
