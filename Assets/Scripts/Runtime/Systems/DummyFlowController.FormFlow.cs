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
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
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
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if (!((Object)(object)formUnlockSystem == (Object)null))
			{
				int requiredCost;
				if (formUnlockSystem.IsUnlocked(form))
				{
					formUnlockSystem.TrySelect(form);
					lastLobbyActionStatus = $"{form.ToString().ToUpperInvariant()} equipped.";
				}
				else if (!formUnlockSystem.TryUnlockAndSelectWithCost(form, out requiredCost))
				{
					Debug.Log((object)$"[AlienCrusher] Need {requiredCost} DP to unlock {form}. Current DP: {formUnlockSystem.DpBalance}");
					lastLobbyActionStatus = $"Need {Mathf.Max(0, requiredCost - formUnlockSystem.DpBalance):0} more DP for {form.ToString().ToUpperInvariant()}.";
					UpdateFormButtons();
					UpdateMetaProgressUi();
					return;
				}
				else
				{
					lastLobbyActionStatus = $"{form.ToString().ToUpperInvariant()} unlocked and equipped.";
				}
				ApplySelectedFormToPlayer();
				UpdateFormButtons();
				UpdateMetaProgressUi();
			}
		}

		private void UpdateFormButtons()
		{
			if (!((Object)(object)formUnlockSystem == (Object)null))
			{
				UpdateFormButton("Form_Sphere", "SPHERE", FormType.Sphere);
				UpdateFormButton("Form_Spike", "SPIKE", FormType.Spike);
				UpdateFormButton("Form_Ram", "RAM", FormType.Ram);
				UpdateFormButton("Form_Saucer", "SAUCER", FormType.Saucer);
				UpdateFormButton("Form_Crusher", "CRUSHER", FormType.Crusher);
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
				FormType.Ram => (num <= 2) ? " (AFTER SPIKE)" : " (MID TIER)", 
				FormType.Saucer => (num <= 2) ? " (AFTER SPIKE)" : " (MID TIER)", 
				FormType.Crusher => " (BOSS TIER)", 
				_ => string.Empty, 
			};
		}

		private FormType GetCurrentSelectedForm()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			if (!((Object)(object)formUnlockSystem != (Object)null))
			{
				return FormType.Sphere;
			}
			return formUnlockSystem.CurrentForm;
		}

		private FormActiveSkill GetFormActiveSkill(FormType form)
		{
			return form switch
			{
				FormType.Sphere => FormActiveSkill.SpherePulse, 
				FormType.Spike => FormActiveSkill.SpikeBurst, 
				FormType.Ram => FormActiveSkill.RamBreach, 
				FormType.Saucer => FormActiveSkill.SaucerDash, 
				FormType.Crusher => FormActiveSkill.CrusherSlam, 
				_ => FormActiveSkill.None, 
			};
		}

		private string GetFormActiveSkillName(FormActiveSkill skill)
		{
			return skill switch
			{
				FormActiveSkill.SpherePulse => "SPHERE PULSE", 
				FormActiveSkill.SpikeBurst => "SPIKE BURST", 
				FormActiveSkill.RamBreach => "RAM BREACH", 
				FormActiveSkill.SaucerDash => "SAUCER DASH", 
				FormActiveSkill.CrusherSlam => "CRUSHER SLAM", 
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

