using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void ApplySelectedFormToPlayer()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindAnyObjectByType<FormUnlockSystem>();
			}
			ResolvePlayerController();
			if (!((Object)(object)formUnlockSystem == (Object)null) && !((Object)(object)cachedPlayerController == (Object)null))
			{
				formUnlockSystem.ApplyToPlayer(cachedPlayerController);
				ApplyPermanentMetaUpgrades();
				if (stageRunning)
				{
					SetHudActionButtonsVisible(ShouldShowActionButtons());
				}
				RefreshActionSkillButtons(force: true);
			}
		}

		private void HandleFormSelection(FormType form)
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindAnyObjectByType<FormUnlockSystem>();
			}
			if (!((Object)(object)formUnlockSystem == (Object)null))
			{
				int requiredCost;
				bool unlockedNow = false;
				if (formUnlockSystem.IsUnlocked(form))
				{
					formUnlockSystem.TrySelect(form);
					lastLobbyActionStatus = $"{FormCatalog.GetDisplayName(form)} ON";
					PlayProgressionConfirmCue();
				}
				else if (!formUnlockSystem.TryUnlockAndSelectWithCost(form, out requiredCost))
				{
#if UNITY_EDITOR
					formUnlockSystem.TryEditorPreviewSelect(form);
					lastLobbyActionStatus = $"{FormCatalog.GetDisplayName(form)} EDITOR PREVIEW";
					ApplySelectedFormToPlayer();
					UpdateFormButtons();
					UpdateMetaProgressUi();
					PlayFormEquipConfirmPulse(form);
					return;
#else
					Debug.Log((object)$"[AlienCrusher] Need {requiredCost} DP to unlock {FormCatalog.GetDisplayName(form)}. Current DP: {formUnlockSystem.DpBalance}");
					lastLobbyActionStatus = $"{FormCatalog.GetDisplayName(form)}  NEED {Mathf.Max(0, requiredCost - formUnlockSystem.DpBalance):0}";
					SignalOutgameDpInsufficient();
					UpdateFormButtons();
					UpdateMetaProgressUi();
					return;
#endif
				}
				else
				{
					lastLobbyActionStatus = $"{FormCatalog.GetDisplayName(form)} UNLOCKED";
					SignalOutgameDpSpend();
					unlockedNow = true;
				}
				ApplySelectedFormToPlayer();
				UpdateFormButtons();
				UpdateMetaProgressUi();
				if (unlockedNow)
				{
					PlayFormUnlockConfirmPulse(form);
				}
				else
				{
					PlayFormEquipConfirmPulse(form);
				}
				ArmSpendChangeReadyFromForm(form);
			}
		}

		private void UpdateFormButtons()
		{
			if (!((Object)(object)formUnlockSystem == (Object)null))
			{
				for (int i = 0; i < FormCatalog.All.Length; i++)
				{
					FormCatalog.Entry entry = FormCatalog.All[i];
					UpdateFormButton(entry.ButtonName, entry.DisplayName, entry.Type);
				}

				EnsureFormStrategyHints();
				EnsureFormIdentityIcons();
			}
		}

		private void UpdateFormButton(string buttonName, string label, FormType form)
		{
			if (!((Object)(object)formUnlockSystem == (Object)null))
			{
				if (!ShouldShowAdvancedLobbyGuidance())
				{
					string earlyHint = form == FormType.Sphere ? " (DEFAULT)" : GetEarlyLobbyFormUnlockHint(form);
					SetButtonLabel(buttonName, label + earlyHint);
					return;
				}
				bool flag = formUnlockSystem.IsUnlocked(form);
				bool num = formUnlockSystem.CurrentForm == form;
				int unlockCost = formUnlockSystem.GetUnlockCost(form);
				int dpBalance = formUnlockSystem.DpBalance;
				bool flag2 = form == lastRecommendedFormUnlock;
				string suffix = num ? " (EQUIPPED)" : (flag ? "" : ((dpBalance < unlockCost) ? $" (NEED {unlockCost} DP)" : $" ({unlockCost} DP)"));
				if (flag2)
				{
					suffix = (string.IsNullOrEmpty(suffix) ? "" : suffix) + (num ? " [STAY]" : (flag ? " [FOCUS]" : " [RECOMMENDED]"));
				}
				SetButtonLabel(buttonName, label + suffix);
			}
		}

		private string GetEarlyLobbyFormUnlockHint(FormType form)
		{
			int num = ((Object)(object)formUnlockSystem != (Object)null) ? Mathf.Max(1, formUnlockSystem.HighestUnlockedStage) : 1;
			return form switch
			{
				FormType.Spike => (num <= 1) ? " (FIRST UNLOCK)" : " (EARLY FORM)", 
				FormType.Ram => (num <= 2) ? " (AFTER DRILL)" : " (MID TIER)", 
				FormType.Saucer => (num <= 2) ? " (AFTER DRILL)" : " (MID TIER)", 
				FormType.Crusher => " (BOSS TIER)", 
				_ => string.Empty, 
			};
		}

		private FormType GetCurrentSelectedForm()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindAnyObjectByType<FormUnlockSystem>();
			}
			if (!((Object)(object)formUnlockSystem != (Object)null))
			{
				return FormType.Sphere;
			}
			return formUnlockSystem.CurrentForm;
		}

		private FormActiveSkill GetFormActiveSkill(FormType form)
		{
			return FormCatalog.GetSmashMethod(form) switch
			{
				FormSmashMethod.BodyRam => FormActiveSkill.SpherePulse, 
				FormSmashMethod.DrillBurrow => FormActiveSkill.DrillBurrow, 
				FormSmashMethod.ChargeBurst => FormActiveSkill.ChargeBurst, 
				FormSmashMethod.UfoRay => FormActiveSkill.UfoRay, 
				FormSmashMethod.MagnetGrab => FormActiveSkill.MagnetGrab, 
				_ => FormActiveSkill.None, 
			};
		}

		private string GetFormActiveSkillName(FormActiveSkill skill)
		{
			return skill switch
			{
				FormActiveSkill.SpherePulse => "SPHERE PULSE", 
				FormActiveSkill.SpikeBurst => "DRILL BURROW", 
				FormActiveSkill.RamBreach => "TANK BURST", 
				FormActiveSkill.SaucerDash => "UFO RAY", 
				FormActiveSkill.CrusherSlam => "MAGNET PULL", 
				FormActiveSkill.DrillBurrow => "DRILL BURROW", 
				FormActiveSkill.ChargeBurst => "TANK CHARGE", 
				FormActiveSkill.UfoRay => "UFO RAY", 
				FormActiveSkill.MagnetGrab => "MAGNET PULL", 
				_ => "FORM", 
			};
		}

		private string GetFormSkillHudToken()
		{
			if (!enableFormActiveSkills)
			{
				return string.Empty;
			}
			FormActiveSkill formActiveSkill = GetFormActiveSkill(GetCurrentSelectedForm());
			if (formActiveSkill == FormActiveSkill.None)
			{
				return string.Empty;
			}
			if (formSkillCooldownRemaining > 0f)
			{
				return $"    {GetFormActiveSkillName(formActiveSkill)} {Mathf.CeilToInt(formSkillCooldownRemaining):00}";
			}
			return "    " + GetFormActiveSkillName(formActiveSkill) + " READY";
		}
	}
}

