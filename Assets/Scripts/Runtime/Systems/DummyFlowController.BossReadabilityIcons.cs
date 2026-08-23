using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string IconBreakWindowId = "Icon_BreakWindow";
		private const string IconShieldId = "Icon_Shield";
		private const string IconWeakPointId = "Icon_WeakPoint";
		private const string IconBossId = "Icon_Boss";
		private const string IconBossSentinelId = "Icon_Boss_Sentinel";

		private Sprite iconBreakWindowSprite;
		private Sprite iconShieldSprite;
		private Sprite iconWeakPointSprite;
		private Sprite iconBossSprite;
		private Sprite iconBossSentinelSprite;
		private bool bossReadabilityIconsCached;

		private void EnsureBossReadabilityIcons()
		{
			CacheBossReadabilityIconSprites();
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			Transform hud = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)hud != (Object)null)
			{
				EnsureHudBossReadabilityIconStrip(hud);
				EnsureHudBossStateIcon(hud);
				EnsureIconBesideText(FindText(canvasRootTransform, "ObjectiveText"), IconWeakPointId, new Vector2(-72f, -8f), 32f);
			}

			if ((Object)(object)lobbyPanel != (Object)null)
			{
				EnsureIconBesideText(lobbyMissionText, IconWeakPointId, new Vector2(-96f, -8f), 36f);
			}

			if ((Object)(object)resultPanel != (Object)null)
			{
				EnsureIconBesideText(resultBreakdownText, IconWeakPointId, new Vector2(-48f, -8f), 36f);
			}

			RefreshHudBossReadabilityIcon(IsStageBossAlive());
			RefreshBossSentinelLobbyResultIcon();
		}

		private void CacheBossReadabilityIconSprites()
		{
			if (bossReadabilityIconsCached)
			{
				return;
			}

			iconBreakWindowSprite = LoadRunEssentialIconSprite(IconBreakWindowId);
			iconShieldSprite = LoadRunEssentialIconSprite(IconShieldId);
			iconWeakPointSprite = LoadRunEssentialIconSprite(IconWeakPointId);
			iconBossSprite = LoadRunEssentialIconSprite(IconBossId);
			iconBossSentinelSprite = LoadRunEssentialIconSprite(IconBossSentinelId);
			bossReadabilityIconsCached = true;
		}

		private Sprite GetBossReadabilityIconSprite(string iconId)
		{
			CacheBossReadabilityIconSprites();
			if (iconId == IconBreakWindowId)
			{
				return iconBreakWindowSprite;
			}

			if (iconId == IconShieldId)
			{
				return iconShieldSprite;
			}

			if (iconId == IconWeakPointId)
			{
				return iconWeakPointSprite;
			}

			if (iconId == IconBossId)
			{
				return iconBossSprite;
			}

			if (iconId == IconBossSentinelId)
			{
				return iconBossSentinelSprite;
			}

			return null;
		}

		private void EnsureHudBossReadabilityIconStrip(Transform hud)
		{
			Transform root = FindDirectChild(hud, "HudBossReadabilityIcons");
			if ((Object)(object)root == (Object)null)
			{
				GameObject go = new GameObject("HudBossReadabilityIcons", typeof(RectTransform));
				root = go.transform;
				root.SetParent(hud, false);
				RectTransform rect = root as RectTransform;
				if ((Object)(object)rect != (Object)null)
				{
					rect.anchorMin = new Vector2(1f, 1f);
					rect.anchorMax = new Vector2(1f, 1f);
					rect.pivot = new Vector2(1f, 1f);
					rect.anchoredPosition = new Vector2(-16f, -148f);
					rect.sizeDelta = new Vector2(220f, 48f);
				}
			}

			string[] ids = { IconBreakWindowId, IconShieldId, IconWeakPointId, IconBossId };
			for (int i = 0; i < ids.Length; i++)
			{
				EnsureStripIcon(root, ids[i], i);
			}
		}

		private void EnsureHudBossStateIcon(Transform hud)
		{
			Image image = EnsureNamedIconImage(hud, "HudBossStateIcon");
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = new Vector2(1f, 1f);
			rect.anchorMax = new Vector2(1f, 1f);
			rect.pivot = new Vector2(1f, 1f);
			rect.sizeDelta = new Vector2(44f, 44f);
			rect.anchoredPosition = new Vector2(-378f, -20f);
			image.preserveAspect = true;
			image.raycastTarget = false;
		}

		private void RefreshHudBossReadabilityIcon(bool bossAlive)
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

			EnsureHudBossStateIcon(hud);
			Image image = EnsureNamedIconImage(hud, "HudBossStateIcon");
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			string iconId = ResolveHudBossStateIconId(bossAlive);
			if (string.IsNullOrEmpty(iconId))
			{
				image.enabled = false;
				return;
			}

			image.enabled = true;
			image.sprite = GetBossReadabilityIconSprite(iconId);
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
		}

		private string ResolveHudBossStateIconId(bool bossAlive)
		{
			if (!stageBossEncounterActive)
			{
				return null;
			}

			if (IsBossBreakWindowActive())
			{
				return IconBreakWindowId;
			}

			if (bossAlive && stageBossShieldActiveCount > 0)
			{
				return IconShieldId;
			}

			return IconBossId;
		}
	}
}
