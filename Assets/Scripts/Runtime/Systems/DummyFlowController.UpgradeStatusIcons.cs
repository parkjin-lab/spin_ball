using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string IconOverdriveId = "Icon_Overdrive";
		private const string IconPanicId = "Icon_Panic";
		private const string IconSeismicId = "Icon_Seismic";
		private const string IconRetailId = "Icon_Retail";
		private const string IconTrafficId = "Icon_Traffic";

		private Sprite iconOverdriveSprite;
		private Sprite iconPanicSprite;
		private Sprite iconSeismicSprite;
		private Sprite iconRetailSprite;
		private Sprite iconTrafficSprite;
		private bool upgradeStatusIconsCached;

		private void EnsureUpgradeStatusIcons()
		{
			CacheUpgradeStatusIconSprites();
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			Transform hud = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)hud != (Object)null)
			{
				EnsureHudUpgradeStatusIconStrip(hud);
				EnsureHudChainStatusIcon(hud);
				EnsureIconBesideText(hudUpgradeListText, IconSeismicId, new Vector2(-36f, 2f), 28f);
				EnsureIconBesideText(hudUpgradeListText, IconRetailId, new Vector2(-68f, 2f), 28f);
			}

			if ((Object)(object)lobbyPanel != (Object)null)
			{
				EnsureLobbyUpgradeStatusIconStrip(lobbyPanel.transform);
				EnsureIconBesideText(lobbyMissionText, IconTrafficId, new Vector2(-48f, -88f), 32f);
			}

			RefreshHudUpgradeStatusIcons();
		}

		private void CacheUpgradeStatusIconSprites()
		{
			if (upgradeStatusIconsCached)
			{
				return;
			}

			iconOverdriveSprite = LoadRunEssentialIconSprite(IconOverdriveId);
			iconPanicSprite = LoadRunEssentialIconSprite(IconPanicId);
			iconSeismicSprite = LoadRunEssentialIconSprite(IconSeismicId);
			iconRetailSprite = LoadRunEssentialIconSprite(IconRetailId);
			iconTrafficSprite = LoadRunEssentialIconSprite(IconTrafficId);
			upgradeStatusIconsCached = true;
		}

		private Sprite GetUpgradeStatusIconSprite(string iconId)
		{
			CacheUpgradeStatusIconSprites();
			if (iconId == IconOverdriveId)
			{
				return iconOverdriveSprite;
			}

			if (iconId == IconPanicId)
			{
				return iconPanicSprite;
			}

			if (iconId == IconSeismicId)
			{
				return iconSeismicSprite;
			}

			if (iconId == IconRetailId)
			{
				return iconRetailSprite;
			}

			if (iconId == IconTrafficId)
			{
				return iconTrafficSprite;
			}

			return null;
		}

		private void EnsureHudUpgradeStatusIconStrip(Transform hud)
		{
			EnsureCompactStatusIconStrip(hud, "HudUpgradeStatusIcons", new Vector2(16f, -66f), new Vector2(0f, 1f));
		}

		private void EnsureLobbyUpgradeStatusIconStrip(Transform lobby)
		{
			EnsureCompactStatusIconStrip(lobby, "LobbyUpgradeStatusIcons", new Vector2(16f, -16f), new Vector2(0f, 1f));
		}

		private void EnsureCompactStatusIconStrip(Transform parent, string rootName, Vector2 anchoredPosition, Vector2 topLeftAnchor)
		{
			Transform root = FindDirectChild(parent, rootName);
			if ((Object)(object)root == (Object)null)
			{
				GameObject go = new GameObject(rootName, typeof(RectTransform));
				root = go.transform;
				root.SetParent(parent, false);
				RectTransform rect = root as RectTransform;
				if ((Object)(object)rect != (Object)null)
				{
					rect.anchorMin = topLeftAnchor;
					rect.anchorMax = topLeftAnchor;
					rect.pivot = new Vector2(0f, 1f);
					rect.anchoredPosition = anchoredPosition;
					rect.sizeDelta = new Vector2(210f, 40f);
				}
			}

			string[] ids = { IconOverdriveId, IconPanicId, IconSeismicId, IconRetailId, IconTrafficId };
			for (int i = 0; i < ids.Length; i++)
			{
				Image image = EnsureNamedIconImage(root, ids[i]);
				if ((Object)(object)image == (Object)null)
				{
					continue;
				}

				RectTransform rect = image.rectTransform;
				rect.anchorMin = new Vector2(0f, 0.5f);
				rect.anchorMax = new Vector2(0f, 0.5f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.sizeDelta = new Vector2(34f, 34f);
				rect.anchoredPosition = new Vector2(20f + i * 40f, 0f);
				image.sprite = GetUpgradeStatusIconSprite(ids[i]);
				image.color = Color.white;
				image.preserveAspect = true;
				image.raycastTarget = false;
			}
		}

		private void EnsureHudChainStatusIcon(Transform hud)
		{
			if ((Object)(object)hudChainText == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(hud, "HudChainStatusIcon");
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = hudChainText.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMin;
			rect.pivot = new Vector2(1f, 0.5f);
			rect.sizeDelta = new Vector2(32f, 32f);
			rect.anchoredPosition = source.anchoredPosition + new Vector2(-12f, 0f);
			image.preserveAspect = true;
			image.raycastTarget = false;
		}

		private void RefreshHudUpgradeStatusIcons()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			Transform hud = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)hud == (Object)null)
			{
				return;
			}

			EnsureHudChainStatusIcon(hud);
			Image image = EnsureNamedIconImage(hud, "HudChainStatusIcon");
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			string iconId = ResolveHudChainStatusIconId();
			if (string.IsNullOrEmpty(iconId))
			{
				image.enabled = false;
				return;
			}

			image.enabled = true;
			image.sprite = GetUpgradeStatusIconSprite(iconId);
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
		}

		private string ResolveHudChainStatusIconId()
		{
			if (trafficPanicChainStack > 0 && trafficPanicChainRemaining > 0.001f)
			{
				return IconPanicId;
			}

			if (overdriveActive)
			{
				return IconOverdriveId;
			}

			return null;
		}
	}
}
