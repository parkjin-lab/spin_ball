using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string IconFormSphereId = "Icon_Form_Sphere";
		private const string IconFormRamId = "Icon_Form_Ram";
		private const string IconFormSaucerId = "Icon_Form_Saucer";
		private const string IconFormSpikeId = "Icon_Form_Spike";
		private const string IconFormCrusherId = "Icon_Form_Crusher";
		private const string IconSkillSpherePulseId = "Icon_Skill_SpherePulse";
		private const string IconSkillRamBreachId = "Icon_Skill_RamBreach";
		private const string IconSkillSaucerDashId = "Icon_Skill_SaucerDash";
		private const string IconSkillSpikeBurstId = "Icon_Skill_SpikeBurst";
		private const string IconSkillCrusherSlamId = "Icon_Skill_CrusherSlam";
		private const string IconFormSkillChildId = "Icon_FormSkill";

		private Sprite iconFormSphereSprite;
		private Sprite iconFormRamSprite;
		private Sprite iconFormSaucerSprite;
		private Sprite iconFormSpikeSprite;
		private Sprite iconFormCrusherSprite;
		private Sprite iconSkillSpherePulseSprite;
		private Sprite iconSkillRamBreachSprite;
		private Sprite iconSkillSaucerDashSprite;
		private Sprite iconSkillSpikeBurstSprite;
		private Sprite iconSkillCrusherSlamSprite;
		private bool formIdentityIconsCached;

		private void EnsureFormIdentityIcons()
		{
			CacheFormIdentityIconSprites();
			EnsureLobbyFormIcon("Form_Sphere", IconFormSphereId, iconFormSphereSprite);
			EnsureLobbyFormIcon("Form_Ram", IconFormRamId, iconFormRamSprite);
			EnsureLobbyFormIcon("Form_Saucer", IconFormSaucerId, iconFormSaucerSprite);
			EnsureLobbyFormIcon("Form_Spike", IconFormSpikeId, iconFormSpikeSprite);
			EnsureLobbyFormIcon("Form_Crusher", IconFormCrusherId, iconFormCrusherSprite);
			RefreshFormIdentitySkillIcons();
			EnsureResultLobbyBadges();
			EnsureOutgameDpEconomy();
		}

		private void CacheFormIdentityIconSprites()
		{
			if (formIdentityIconsCached)
			{
				return;
			}

			iconFormSphereSprite = LoadFormIdentityIconSprite(IconFormSphereId);
			iconFormRamSprite = LoadFormIdentityIconSprite(IconFormRamId);
			iconFormSaucerSprite = LoadFormIdentityIconSprite(IconFormSaucerId);
			iconFormSpikeSprite = LoadFormIdentityIconSprite(IconFormSpikeId);
			iconFormCrusherSprite = LoadFormIdentityIconSprite(IconFormCrusherId);
			iconSkillSpherePulseSprite = LoadFormIdentityIconSprite(IconSkillSpherePulseId);
			iconSkillRamBreachSprite = LoadFormIdentityIconSprite(IconSkillRamBreachId);
			iconSkillSaucerDashSprite = LoadFormIdentityIconSprite(IconSkillSaucerDashId);
			iconSkillSpikeBurstSprite = LoadFormIdentityIconSprite(IconSkillSpikeBurstId);
			iconSkillCrusherSlamSprite = LoadFormIdentityIconSprite(IconSkillCrusherSlamId);
			formIdentityIconsCached = true;
		}

		private static Sprite LoadFormIdentityIconSprite(string iconId)
		{
			Sprite sprite = Resources.Load<Sprite>("UI/Icons/" + iconId);
			if ((Object)(object)sprite != (Object)null)
			{
				return sprite;
			}

			Texture2D texture = Resources.Load<Texture2D>("UI/Icons/" + iconId);
			if ((Object)(object)texture == (Object)null)
			{
				return null;
			}

			sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
			sprite.name = iconId;
			return sprite;
		}

		private void EnsureLobbyFormIcon(string buttonName, string iconId, Sprite sprite)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null || (Object)(object)sprite == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, iconId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = new Vector2(0.5f, 1f);
			rect.anchorMax = new Vector2(0.5f, 1f);
			rect.pivot = new Vector2(0.5f, 1f);
			rect.sizeDelta = new Vector2(52f, 52f);
			rect.anchoredPosition = new Vector2(0f, -8f);
			image.sprite = sprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = true;
		}

		private void RefreshFormIdentitySkillIcons()
		{
			CacheFormIdentityIconSprites();
			FormActiveSkill skill = enableFormActiveSkills ? GetFormActiveSkill(GetCurrentSelectedForm()) : FormActiveSkill.None;
			Sprite sprite = skill switch
			{
				FormActiveSkill.SpherePulse => iconSkillSpherePulseSprite,
				FormActiveSkill.RamBreach => iconSkillRamBreachSprite,
				FormActiveSkill.SaucerDash => iconSkillSaucerDashSprite,
				FormActiveSkill.SpikeBurst => iconSkillSpikeBurstSprite,
				FormActiveSkill.CrusherSlam => iconSkillCrusherSlamSprite,
				_ => null
			};
			bool show = (Object)(object)sprite != (Object)null;
			PlaceSkillIconOnButton("TransformButton", IconFormSkillChildId, sprite, show, new Vector2(18f, -16f), 34f);
			PlaceSkillIconOnButton("Special1Button", IconFormSkillChildId, sprite, show, new Vector2(16f, -14f), 30f);
			HideLegacySkillIcon("TransformButton", IconSkillSpherePulseId);
			HideLegacySkillIcon("Special1Button", IconSkillSpherePulseId);
		}

		private void HideLegacySkillIcon(string buttonName, string iconId)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Transform existing = FindDirectChild(button.transform, iconId);
			if ((Object)(object)existing != (Object)null)
			{
				existing.gameObject.SetActive(false);
			}
		}

		private void PlaceSkillIconOnButton(string buttonName, string iconId, Sprite sprite, bool visible, Vector2 anchoredPosition, float size)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, iconId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = new Vector2(0f, 1f);
			rect.anchorMax = new Vector2(0f, 1f);
			rect.pivot = new Vector2(0.5f, 1f);
			rect.sizeDelta = new Vector2(size, size);
			rect.anchoredPosition = anchoredPosition;
			image.sprite = sprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = visible && (Object)(object)sprite != (Object)null;
		}
	}
}
