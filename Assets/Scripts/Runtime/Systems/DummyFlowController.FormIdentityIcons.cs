using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string IconFormSphereId = "Icon_Form_Sphere";
		private const string IconSkillSpherePulseId = "Icon_Skill_SpherePulse";

		private Sprite iconFormSphereSprite;
		private Sprite iconSkillSpherePulseSprite;
		private bool formIdentityIconsCached;

		private void EnsureFormIdentityIcons()
		{
			CacheFormIdentityIconSprites();
			EnsureLobbySphereFormIcon();
			RefreshFormIdentitySkillIcons();
		}

		private void CacheFormIdentityIconSprites()
		{
			if (formIdentityIconsCached)
			{
				return;
			}

			iconFormSphereSprite = LoadFormIdentityIconSprite(IconFormSphereId);
			iconSkillSpherePulseSprite = LoadFormIdentityIconSprite(IconSkillSpherePulseId);
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

		private void EnsureLobbySphereFormIcon()
		{
			Button button = FindButton("Form_Sphere");
			if ((Object)(object)button == (Object)null || (Object)(object)iconFormSphereSprite == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, IconFormSphereId);
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
			image.sprite = iconFormSphereSprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
		}

		private void RefreshFormIdentitySkillIcons()
		{
			CacheFormIdentityIconSprites();
			bool showPulse = enableFormActiveSkills && GetFormActiveSkill(GetCurrentSelectedForm()) == FormActiveSkill.SpherePulse;
			PlaceSkillIconOnButton("TransformButton", IconSkillSpherePulseId, iconSkillSpherePulseSprite, showPulse, new Vector2(18f, -16f), 34f);
			PlaceSkillIconOnButton("Special1Button", IconSkillSpherePulseId, iconSkillSpherePulseSprite, showPulse, new Vector2(16f, -14f), 30f);
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
