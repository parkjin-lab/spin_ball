using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string IconDpId = "Icon_DP";
		private const string IconStageId = "Icon_Stage";
		private const string IconNextStepId = "Icon_NextStep";
		private const string IconRouteId = "Icon_Route";

		private Sprite iconDpSprite;
		private Sprite iconStageSprite;
		private Sprite iconNextStepSprite;
		private Sprite iconRouteSprite;
		private bool runEssentialIconsCached;

		private void EnsureRunEssentialIcons()
		{
			CacheRunEssentialIconSprites();
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			Transform hud = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)hud != (Object)null)
			{
				EnsureHudRunEssentialIconStrip(hud);
				EnsureIconBesideText(FindText(canvasRootTransform, "InfoText"), IconStageId, new Vector2(-36f, 0f), 40f);
				EnsureIconBesideText(FindText(canvasRootTransform, "ObjectiveText"), IconNextStepId, new Vector2(-22f, -4f), 32f);
				EnsureIconBesideText(hudRouteIndicatorText, IconRouteId, new Vector2(-92f, 0f), 36f);
			}

			if ((Object)(object)lobbyPanel != (Object)null)
			{
				EnsureIconBesideText(lobbyDpText, IconDpId, new Vector2(-48f, -8f), 40f);
				EnsureIconBesideText(FindText(canvasRootTransform, "StageSelectText"), IconStageId, new Vector2(-40f, 0f), 36f);
				EnsureIconBesideText(lobbyMissionText, IconNextStepId, new Vector2(-48f, -8f), 36f);
			}

			if ((Object)(object)resultPanel != (Object)null)
			{
				EnsureIconBesideText(resultSummaryText, IconDpId, new Vector2(-52f, 18f), 40f);
				EnsureIconBesideText(resultSummaryText, IconStageId, new Vector2(-52f, -22f), 36f);
				EnsureIconBesideText(resultAdviceText, IconNextStepId, new Vector2(-36f, -2f), 32f);
			}

			EnsureResultLobbyBadges();
			EnsureOutgameDpEconomy();
		}

		private void CacheRunEssentialIconSprites()
		{
			if (runEssentialIconsCached)
			{
				return;
			}

			iconDpSprite = LoadRunEssentialIconSprite(IconDpId);
			iconStageSprite = LoadRunEssentialIconSprite(IconStageId);
			iconNextStepSprite = LoadRunEssentialIconSprite(IconNextStepId);
			iconRouteSprite = LoadRunEssentialIconSprite(IconRouteId);
			runEssentialIconsCached = true;
		}

		private static Sprite LoadRunEssentialIconSprite(string iconId)
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

		private Sprite GetRunEssentialIconSprite(string iconId)
		{
			if (iconId == IconDpId)
			{
				return iconDpSprite;
			}

			if (iconId == IconStageId)
			{
				return iconStageSprite;
			}

			if (iconId == IconNextStepId)
			{
				return iconNextStepSprite;
			}

			if (iconId == IconRouteId)
			{
				return iconRouteSprite;
			}

			Sprite bossSprite = GetBossReadabilityIconSprite(iconId);
			if ((Object)(object)bossSprite != (Object)null)
			{
				return bossSprite;
			}

			return GetUpgradeStatusIconSprite(iconId);
		}

		private void EnsureHudRunEssentialIconStrip(Transform hud)
		{
			Transform root = FindDirectChild(hud, "HudRunEssentialIcons");
			if ((Object)(object)root == (Object)null)
			{
				GameObject go = new GameObject("HudRunEssentialIcons", typeof(RectTransform));
				root = go.transform;
				root.SetParent(hud, false);
				RectTransform rect = root as RectTransform;
				if ((Object)(object)rect != (Object)null)
				{
					rect.anchorMin = new Vector2(0f, 1f);
					rect.anchorMax = new Vector2(0f, 1f);
					rect.pivot = new Vector2(0f, 1f);
					rect.anchoredPosition = new Vector2(16f, -12f);
					rect.sizeDelta = new Vector2(220f, 48f);
				}
			}

			string[] ids = { IconDpId, IconStageId, IconNextStepId, IconRouteId };
			for (int i = 0; i < ids.Length; i++)
			{
				EnsureStripIcon(root, ids[i], i);
			}
		}

		private void EnsureStripIcon(Transform root, string iconId, int index)
		{
			Image image = EnsureNamedIconImage(root, iconId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = new Vector2(0f, 0.5f);
			rect.anchorMax = new Vector2(0f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(44f, 44f);
			rect.anchoredPosition = new Vector2(24f + index * 52f, 0f);
			image.sprite = GetRunEssentialIconSprite(iconId);
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = (Object)(object)image.sprite != (Object)null;
		}

		private void EnsureIconBesideText(Text text, string iconId, Vector2 anchoredOffset, float size)
		{
			if ((Object)(object)text == (Object)null)
			{
				return;
			}

			Transform parent = text.transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}

			string childName = iconId + "_" + text.name;
			Image image = EnsureNamedIconImage(parent, childName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = text.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMin;
			rect.pivot = new Vector2(1f, 1f);
			rect.sizeDelta = new Vector2(size, size);
			rect.anchoredPosition = source.anchoredPosition + anchoredOffset;
			image.sprite = GetRunEssentialIconSprite(iconId);
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = (Object)(object)image.sprite != (Object)null;
		}

		private static Image EnsureNamedIconImage(Transform parent, string name)
		{
			Transform existing = FindDirectChild(parent, name);
			if ((Object)(object)existing == (Object)null)
			{
				GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				existing = go.transform;
				existing.SetParent(parent, false);
			}

			Image image = existing.GetComponent<Image>();
			if ((Object)(object)image == (Object)null)
			{
				image = existing.gameObject.AddComponent<Image>();
			}

			return image;
		}
	}
}
