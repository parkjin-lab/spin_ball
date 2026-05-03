using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		public void OpenLevelUp()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if (stageRunning && !levelUpOpen)
			{
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

		public void ChooseSkillOptionA()
		{
			SelectSkill(currentChoices[0]);
		}

		public void ChooseSkillOptionB()
		{
			SelectSkill(currentChoices[1]);
		}

		public void ChooseSkillOptionC()
		{
			SelectSkill(currentChoices[2]);
		}

		public void ToggleLevelUpLockA()
		{
			ToggleLevelUpChoiceLock(0);
		}

		public void ToggleLevelUpLockB()
		{
			ToggleLevelUpChoiceLock(1);
		}

		public void ToggleLevelUpLockC()
		{
			ToggleLevelUpChoiceLock(2);
		}

		public void HandleLevelUpRerollButton()
		{
			TryRerollLevelUpChoices();
		}

		private void SelectSkill(LevelUpChoice choice)
		{
			if (levelUpOpen)
			{
				scoreSystem?.AddScore(skillPickBonusScore);
				ApplyLevelUpChoice(choice);
				UpdateUpgradeListText();
				levelUpOpen = false;
				ApplyUiViewState(UiViewState.Hud);
				ClearLevelUpLockSelection();
				ConsumePendingTrafficPanicMasteryPulse();
				TryOpenQueuedLevelUp();
				UpdateHudInfo();
			}
		}

		private void CloseLevelUpWithoutSelection()
		{
			if (autoPickLevelUpOnTimeout && TryAutoPickLevelUpChoice(out var choice))
			{
				ApplyLevelUpChoice(choice);
				UpdateUpgradeListText();
				comboRushAnnouncementText = "AUTO PICK: " + GetChoiceTag(choice);
				comboRushAnnouncementRemaining = Mathf.Max(comboRushAnnouncementRemaining, 0.9f);
			}
			levelUpOpen = false;
			ApplyUiViewState(UiViewState.Hud);
			ClearLevelUpLockSelection();
			ConsumePendingTrafficPanicMasteryPulse();
			TryOpenQueuedLevelUp();
			UpdateHudInfo();
		}

		private bool TryAutoPickLevelUpChoice(out LevelUpChoice choice)
		{
			if (levelUpHasLockedChoice && levelUpLockedSlot >= 0 && levelUpLockedSlot < currentChoices.Length)
			{
				choice = currentChoices[levelUpLockedSlot];
				return true;
			}
			if (currentChoices == null || currentChoices.Length == 0)
			{
				choice = LevelUpChoice.Speed;
				return false;
			}
			int num = Mathf.Clamp(Random.Range(0, currentChoices.Length), 0, currentChoices.Length - 1);
			choice = currentChoices[num];
			return true;
		}

		private void UpdateLevelUpTimer()
		{
			if (!((Object)(object)levelUpTimerText == (Object)null))
			{
				float num = Mathf.Max(0f, levelUpRemaining);
				if (!enableLevelUpReroll)
				{
					levelUpTimerText.text = $"Choose one skill ({Mathf.CeilToInt(num)}s)\nAUTO PICK ON TIMEOUT";
					UpdateLevelUpChoiceButtons();
				}
				else
				{
					string arg = (levelUpHasLockedChoice ? $"SLOT {levelUpLockedSlot + 1}" : "NONE");
					levelUpTimerText.text = $"Choose one skill ({Mathf.CeilToInt(num)}s)\nREROLL {levelUpRerollsRemaining}  LOCK {arg}  AUTO PICK";
					UpdateLevelUpChoiceButtons();
				}
			}
		}

		private void ProcessLevelUpDraftInput()
		{
			Keyboard current = Keyboard.current;
			if (current != null)
			{
				if (current.rKey.wasPressedThisFrame)
				{
					TryRerollLevelUpChoices();
				}
				if (current.digit1Key.wasPressedThisFrame || current.numpad1Key.wasPressedThisFrame)
				{
					ToggleLevelUpChoiceLock(0);
				}
				if (current.digit2Key.wasPressedThisFrame || current.numpad2Key.wasPressedThisFrame)
				{
					ToggleLevelUpChoiceLock(1);
				}
				if (current.digit3Key.wasPressedThisFrame || current.numpad3Key.wasPressedThisFrame)
				{
					ToggleLevelUpChoiceLock(2);
				}
			}
		}

		private void UpdateLevelUpRerollRewardsFromChain()
		{
			if (enableLevelUpReroll && !((Object)(object)scoreSystem == (Object)null))
			{
				int num = Mathf.Max(0, scoreSystem.ChainCount);
				if (!levelUpChainRerollARewardGranted && num >= Mathf.Max(1, levelUpRerollChainThresholdA))
				{
					levelUpChainRerollARewardGranted = true;
					levelUpRerollsRemaining = Mathf.Min(Mathf.Max(0, levelUpMaxRerolls), levelUpRerollsRemaining + 1);
				}
				if (!levelUpChainRerollBRewardGranted && num >= Mathf.Max(levelUpRerollChainThresholdA + 1, levelUpRerollChainThresholdB))
				{
					levelUpChainRerollBRewardGranted = true;
					levelUpRerollsRemaining = Mathf.Min(Mathf.Max(0, levelUpMaxRerolls), levelUpRerollsRemaining + 1);
				}
			}
		}

		private void ResetLevelUpDraftRuntime()
		{
			levelUpRerollsRemaining = Mathf.Clamp(levelUpBaseRerolls, 0, Mathf.Max(0, levelUpMaxRerolls));
			levelUpChainRerollARewardGranted = false;
			levelUpChainRerollBRewardGranted = false;
			ClearLevelUpLockSelection();
			hasPreviousLevelUpChoices = false;
			for (int i = 0; i < levelUpChoiceMissStreak.Length; i++)
			{
				levelUpChoiceMissStreak[i] = 0;
			}
		}

		private void TryRerollLevelUpChoices()
		{
			if (enableLevelUpReroll && levelUpOpen)
			{
				if (levelUpRerollsRemaining <= 0)
				{
					UpdateLevelUpTimer();
					return;
				}
				levelUpRerollsRemaining = Mathf.Max(0, levelUpRerollsRemaining - 1);
				RollLevelUpChoices();
				UpdateLevelUpTimer();
			}
		}

		private void ToggleLevelUpChoiceLock(int slot)
		{
			if (enableLevelUpReroll && levelUpOpen && slot >= 0 && slot < currentChoices.Length)
			{
				if (levelUpHasLockedChoice && levelUpLockedSlot == slot)
				{
					ClearLevelUpLockSelection();
				}
				else
				{
					levelUpHasLockedChoice = true;
					levelUpLockedSlot = slot;
					levelUpLockedChoice = currentChoices[slot];
				}
				RollLevelUpChoices();
				UpdateLevelUpTimer();
			}
		}

		private void ClearLevelUpLockSelection()
		{
			levelUpHasLockedChoice = false;
			levelUpLockedSlot = -1;
		}
	}
}
