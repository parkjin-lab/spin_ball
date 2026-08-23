using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string FormCardStateSetId = "UI_FormCard_StateSet";
		private const string MetaNodeSizeCoreId = "UI_MetaNode_SizeCore";
		private const string MetaNodeImpactCoreId = "UI_MetaNode_ImpactCore";
		private const string MetaNodeDpAmplifierId = "UI_MetaNode_DpAmplifier";
		private const string BannerStageUnlockedId = "Banner_StageUnlocked";
		private const string ToastProgressionSavedId = "Toast_ProgressionSaved";
		private const string BadgeFormReadyId = "Badge_FormReady";
		private const string BadgeMetaReadyId = "Badge_MetaReady";
		private const string FormCardFrameName = "UI_FormCard_StateSet";
		private const string ResultUnlockBannerName = "ResultStageUnlockBanner";
		private const string LobbyUnlockBannerName = "LobbyStageUnlockBanner";
		private const string ResultSaveToastName = "ResultSaveToast";
		private const string LobbySaveToastName = "LobbySaveToast";

		private Sprite formCardStateSetSprite;
		private Sprite metaNodeSizeCoreSprite;
		private Sprite metaNodeImpactCoreSprite;
		private Sprite metaNodeDpAmplifierSprite;
		private Sprite bannerStageUnlockedSprite;
		private Sprite toastProgressionSavedSprite;
		private Sprite badgeFormReadySprite;
		private Sprite badgeMetaReadySprite;
		private bool outgameProgressionVisualsCached;
		private bool outgameStageUnlockBannerArmed;
		private bool outgameSaveToastArmed;

		private void EnsureOutgameProgressionVisuals()
		{
			CacheOutgameProgressionVisuals();
			RefreshOutgameProgressionVisuals();
		}

		private void CacheOutgameProgressionVisuals()
		{
			if (outgameProgressionVisualsCached)
			{
				return;
			}

			formCardStateSetSprite = LoadUiSprite("UI/Lobby/" + FormCardStateSetId, FormCardStateSetId);
			metaNodeSizeCoreSprite = LoadUiSprite("UI/Meta/" + MetaNodeSizeCoreId, MetaNodeSizeCoreId);
			metaNodeImpactCoreSprite = LoadUiSprite("UI/Meta/" + MetaNodeImpactCoreId, MetaNodeImpactCoreId);
			metaNodeDpAmplifierSprite = LoadUiSprite("UI/Meta/" + MetaNodeDpAmplifierId, MetaNodeDpAmplifierId);
			bannerStageUnlockedSprite = LoadUiSprite("UI/Rewards/" + BannerStageUnlockedId, BannerStageUnlockedId);
			toastProgressionSavedSprite = LoadUiSprite("UI/Rewards/" + ToastProgressionSavedId, ToastProgressionSavedId);
			badgeFormReadySprite = LoadUiSprite("UI/Badges/" + BadgeFormReadyId, BadgeFormReadyId);
			badgeMetaReadySprite = LoadUiSprite("UI/Badges/" + BadgeMetaReadyId, BadgeMetaReadyId);
			outgameProgressionVisualsCached = true;
		}

		private static Sprite LoadUiSprite(string resourcesPath, string assetId)
		{
			Sprite sprite = Resources.Load<Sprite>(resourcesPath);
			if ((Object)(object)sprite != (Object)null)
			{
				return sprite;
			}

			Texture2D texture = Resources.Load<Texture2D>(resourcesPath);
			if ((Object)(object)texture == (Object)null)
			{
				return null;
			}

			sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
			sprite.name = assetId;
			return sprite;
		}

		private void SignalOutgameStageUnlocked()
		{
			outgameStageUnlockBannerArmed = true;
			outgameSaveToastArmed = true;
			RefreshOutgameProgressionVisuals();
		}

		private void SignalOutgameProgressionSaved()
		{
			outgameSaveToastArmed = true;
			RefreshOutgameProgressionVisuals();
		}

		private void ClearOutgameProgressionTransientVisuals()
		{
			outgameStageUnlockBannerArmed = false;
			outgameSaveToastArmed = false;
		}

		private void RefreshOutgameProgressionVisuals()
		{
			CacheOutgameProgressionVisuals();
			ApplyFormCardStateFrame("Form_Sphere", FormType.Sphere);
			ApplyFormCardStateFrame("Form_Spike", FormType.Spike);
			ApplyFormCardStateFrame("Form_Ram", FormType.Ram);
			ApplyFormCardStateFrame("Form_Saucer", FormType.Saucer);
			ApplyFormCardStateFrame("Form_Crusher", FormType.Crusher);
			ApplyMetaNodeVisual("MetaUpgrade_SizeButton", FormUnlockSystem.MetaUpgradeType.SizeCore, metaNodeSizeCoreSprite);
			ApplyMetaNodeVisual("MetaUpgrade_ImpactButton", FormUnlockSystem.MetaUpgradeType.ImpactCore, metaNodeImpactCoreSprite);
			ApplyMetaNodeVisual("MetaUpgrade_DpButton", FormUnlockSystem.MetaUpgradeType.DpAmplifier, metaNodeDpAmplifierSprite);
			PlacePanelSprite(resultPanel != null ? resultPanel.transform : null, ResultUnlockBannerName, bannerStageUnlockedSprite, ShouldShowStageUnlockBanner(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(220f, 56f), new Vector2(0f, -78f));
			PlacePanelSprite(lobbyPanel != null ? lobbyPanel.transform : null, LobbyUnlockBannerName, bannerStageUnlockedSprite, ShouldShowStageUnlockBanner() && currentUiViewState == UiViewState.Lobby, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(220f, 56f), new Vector2(0f, -8f));
			PlacePanelSprite(resultPanel != null ? resultPanel.transform : null, ResultSaveToastName, toastProgressionSavedSprite, ShouldShowSaveToast() && currentUiViewState == UiViewState.Result, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(140f, 40f), new Vector2(-24f, 24f));
			PlacePanelSprite(lobbyPanel != null ? lobbyPanel.transform : null, LobbySaveToastName, toastProgressionSavedSprite, ShouldShowSaveToast() && currentUiViewState == UiViewState.Lobby, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(140f, 40f), new Vector2(-24f, 24f));
			PlaceReadyBadge(resultAdviceText, "ResultFormReadyBadge", badgeFormReadySprite, ShouldShowFormReadyBadge());
			PlaceReadyBadge(resultAdviceText, "ResultMetaReadyBadge", badgeMetaReadySprite, ShouldShowMetaReadyBadge());
			PlaceNextActionReadyPulse();
		}

		private bool ShouldShowStageUnlockBanner()
		{
			return outgameStageUnlockBannerArmed && DidStageEndInSuccess();
		}

		private bool ShouldShowSaveToast()
		{
			return outgameSaveToastArmed;
		}

		private bool ShouldShowFormReadyBadge()
		{
			int dp = ((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.DpBalance : 0;
			return currentUiViewState == UiViewState.Result && HasReadyFormUnlock(dp);
		}

		private bool ShouldShowMetaReadyBadge()
		{
			int dp = ((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.DpBalance : 0;
			return currentUiViewState == UiViewState.Result && HasReadyMetaUpgrade(dp);
		}

		private void ApplyFormCardStateFrame(string buttonName, FormType formType)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, FormCardFrameName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.offsetMin = new Vector2(2f, 2f);
			rect.offsetMax = new Vector2(-2f, -2f);
			image.sprite = formCardStateSetSprite;
			image.color = ResolveFormCardStateColor(formType);
			image.preserveAspect = false;
			image.raycastTarget = false;
			image.enabled = (Object)(object)formCardStateSetSprite != (Object)null;
			image.transform.SetAsFirstSibling();
		}

		private Color ResolveFormCardStateColor(FormType formType)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return new Color(0.4f, 0.46f, 0.52f, 0.7f);
			}

			bool unlocked = formUnlockSystem.IsUnlocked(formType);
			bool equipped = formUnlockSystem.CurrentForm == formType;
			bool recommended = formType == lastRecommendedFormUnlock && ShouldShowAdvancedLobbyGuidance();
			bool affordable = !unlocked && formUnlockSystem.DpBalance >= formUnlockSystem.GetUnlockCost(formType);
			if (equipped)
			{
				return new Color(0.42f, 0.78f, 0.92f, 0.96f);
			}

			if (recommended)
			{
				return new Color(0.92f, 0.78f, 0.28f, 0.96f);
			}

			if (affordable)
			{
				return new Color(0.28f, 0.78f, 0.58f, 0.92f);
			}

			if (!unlocked)
			{
				return new Color(0.22f, 0.24f, 0.28f, 0.78f);
			}

			return new Color(0.52f, 0.6f, 0.66f, 0.82f);
		}

		private void ApplyMetaNodeVisual(string buttonName, FormUnlockSystem.MetaUpgradeType upgradeType, Sprite sprite)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			string childName = upgradeType == FormUnlockSystem.MetaUpgradeType.SizeCore
				? MetaNodeSizeCoreId
				: (upgradeType == FormUnlockSystem.MetaUpgradeType.ImpactCore ? MetaNodeImpactCoreId : MetaNodeDpAmplifierId);
			Image image = EnsureNamedIconImage(button.transform, childName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = new Vector2(0.5f, 1f);
			rect.anchorMax = new Vector2(0.5f, 1f);
			rect.pivot = new Vector2(0.5f, 1f);
			rect.sizeDelta = new Vector2(40f, 40f);
			rect.anchoredPosition = new Vector2(0f, -6f);
			image.sprite = sprite;
			image.color = ResolveMetaNodeStateColor(upgradeType);
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = (Object)(object)sprite != (Object)null;
		}

		private Color ResolveMetaNodeStateColor(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				return new Color(0.45f, 0.5f, 0.55f, 0.8f);
			}

			int cost = formUnlockSystem.GetMetaUpgradeCost(upgradeType);
			int level = formUnlockSystem.GetMetaUpgradeLevel(upgradeType);
			bool recommended = upgradeType == lastRecommendedMetaUpgrade && ShouldShowAdvancedLobbyGuidance();
			bool maxed = cost <= 0;
			bool affordable = cost > 0 && formUnlockSystem.DpBalance >= cost;
			if (recommended)
			{
				return new Color(0.96f, 0.82f, 0.32f, 1f);
			}

			if (maxed)
			{
				return new Color(0.82f, 0.88f, 0.94f, 1f);
			}

			if (affordable)
			{
				return new Color(0.36f, 0.9f, 0.62f, 1f);
			}

			if (level > 0)
			{
				return new Color(0.46f, 0.72f, 0.86f, 1f);
			}

			return new Color(0.34f, 0.38f, 0.44f, 0.82f);
		}

		private void PlaceReadyBadge(Text text, string childName, Sprite sprite, bool visible)
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

			Image image = EnsureNamedIconImage(parent, childName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = text.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMin;
			rect.pivot = new Vector2(0f, 1f);
			rect.sizeDelta = new Vector2(32f, 32f);
			rect.anchoredPosition = source.anchoredPosition + (childName.Contains("Meta") ? new Vector2(88f, -6f) : new Vector2(52f, -6f));
			image.sprite = sprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = visible && (Object)(object)sprite != (Object)null;
		}

		private void PlacePanelSprite(Transform parent, string name, Sprite sprite, bool visible, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
		{
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(parent, name);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = anchor;
			rect.anchorMax = anchor;
			rect.pivot = pivot;
			rect.sizeDelta = size;
			rect.anchoredPosition = anchoredPosition;
			image.sprite = sprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = visible && (Object)(object)sprite != (Object)null;
		}
	}
}
