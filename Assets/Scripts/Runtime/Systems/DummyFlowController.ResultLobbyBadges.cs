using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string BadgeResultClearId = "Badge_Result_Clear";
		private const string BadgeResultFailureId = "Badge_Result_Failure";
		private const string BadgeLockedId = "Badge_Locked";
		private const string BadgeRecommendedId = "Badge_Recommended";
		private const string ResultOutcomeBadgeName = "ResultOutcomeBadge";
		private const string ResultAdviceBadgeName = "ResultAdviceBadge";
		private const string LobbyRecommendBadgeName = "LobbyRecommendBadge";
		private const string CardStateBadgeName = "CardStateBadge";

		private Sprite badgeResultClearSprite;
		private Sprite badgeResultFailureSprite;
		private Sprite badgeLockedSprite;
		private Sprite badgeRecommendedSprite;
		private bool resultLobbyBadgesCached;

		private void EnsureResultLobbyBadges()
		{
			CacheResultLobbyBadgeSprites();
			RefreshResultLobbyBadges();
		}

		private void CacheResultLobbyBadgeSprites()
		{
			if (resultLobbyBadgesCached)
			{
				return;
			}

			badgeResultClearSprite = LoadResultLobbyBadgeSprite(BadgeResultClearId);
			badgeResultFailureSprite = LoadResultLobbyBadgeSprite(BadgeResultFailureId);
			badgeLockedSprite = LoadResultLobbyBadgeSprite(BadgeLockedId);
			badgeRecommendedSprite = LoadResultLobbyBadgeSprite(BadgeRecommendedId);
			resultLobbyBadgesCached = true;
		}

		private static Sprite LoadResultLobbyBadgeSprite(string badgeId)
		{
			Sprite sprite = Resources.Load<Sprite>("UI/Badges/" + badgeId);
			if ((Object)(object)sprite != (Object)null)
			{
				return sprite;
			}

			Texture2D texture = Resources.Load<Texture2D>("UI/Badges/" + badgeId);
			if ((Object)(object)texture == (Object)null)
			{
				return null;
			}

			sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
			sprite.name = badgeId;
			return sprite;
		}

		private Sprite GetResultLobbyBadgeSprite(string badgeId)
		{
			CacheResultLobbyBadgeSprites();
			if (badgeId == BadgeResultClearId)
			{
				return badgeResultClearSprite;
			}

			if (badgeId == BadgeResultFailureId)
			{
				return badgeResultFailureSprite;
			}

			if (badgeId == BadgeLockedId)
			{
				return badgeLockedSprite;
			}

			if (badgeId == BadgeRecommendedId)
			{
				return badgeRecommendedSprite;
			}

			return null;
		}

		private void RefreshResultLobbyBadges()
		{
			CacheResultLobbyBadgeSprites();
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			if ((Object)(object)resultPanel != (Object)null)
			{
				PlaceIndependentBadge(resultPanel.transform, ResultOutcomeBadgeName, ResolveResultOutcomeBadgeId(), new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(96f, 48f), new Vector2(-390f, -24f));
				PlaceBadgeBesideText(resultAdviceText, ResultAdviceBadgeName, ResolveResultAdviceBadgeId(), new Vector2(52f, -6f), 36f);
				ApplyCardStateBadge("NextStageButton", DidStageEndInSuccess() ? null : BadgeLockedId);
			}

			if ((Object)(object)lobbyPanel != (Object)null)
			{
				PlaceBadgeBesideText(lobbyRecommendationText, LobbyRecommendBadgeName, ShouldShowAdvancedLobbyGuidance() ? BadgeRecommendedId : null, new Vector2(-52f, -8f), 36f);
			}

			ApplyFormCardStateBadge("Form_Sphere", FormType.Sphere);
			ApplyFormCardStateBadge("Form_Spike", FormType.Spike);
			ApplyFormCardStateBadge("Form_Ram", FormType.Ram);
			ApplyFormCardStateBadge("Form_Saucer", FormType.Saucer);
			ApplyFormCardStateBadge("Form_Crusher", FormType.Crusher);
			ApplyMetaCardStateBadge("MetaUpgrade_SizeButton", FormUnlockSystem.MetaUpgradeType.SizeCore);
			ApplyMetaCardStateBadge("MetaUpgrade_ImpactButton", FormUnlockSystem.MetaUpgradeType.ImpactCore);
			ApplyMetaCardStateBadge("MetaUpgrade_DpButton", FormUnlockSystem.MetaUpgradeType.DpAmplifier);
		}

		private string ResolveResultOutcomeBadgeId()
		{
			if (stageEndResult == StageEndResult.None)
			{
				return null;
			}

			return DidStageEndInSuccess() ? BadgeResultClearId : BadgeResultFailureId;
		}

		private string ResolveResultAdviceBadgeId()
		{
			if (stageEndResult == StageEndResult.None)
			{
				return null;
			}

			int dp = ((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.DpBalance : 0;
			if (HasReadyFormUnlock(dp) || HasReadyMetaUpgrade(dp) || lastRecommendedFormUnlock != FormType.Sphere)
			{
				return BadgeRecommendedId;
			}

			return DidStageEndInSuccess() ? BadgeResultClearId : BadgeResultFailureId;
		}

		private void ApplyFormCardStateBadge(string buttonName, FormType formType)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				ApplyCardStateBadge(buttonName, null);
				return;
			}

			bool unlocked = formUnlockSystem.IsUnlocked(formType);
			bool recommended = formType == lastRecommendedFormUnlock && ShouldShowAdvancedLobbyGuidance();
			if (recommended)
			{
				ApplyCardStateBadge(buttonName, BadgeRecommendedId);
				return;
			}

			ApplyCardStateBadge(buttonName, unlocked ? null : BadgeLockedId);
		}

		private void ApplyMetaCardStateBadge(string buttonName, FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			if ((Object)(object)formUnlockSystem == (Object)null)
			{
				ApplyCardStateBadge(buttonName, null);
				return;
			}

			bool recommended = upgradeType == lastRecommendedMetaUpgrade && ShouldShowAdvancedLobbyGuidance();
			if (recommended)
			{
				ApplyCardStateBadge(buttonName, BadgeRecommendedId);
				return;
			}

			int cost = formUnlockSystem.GetMetaUpgradeCost(upgradeType);
			bool locked = cost > 0 && formUnlockSystem.DpBalance < cost;
			ApplyCardStateBadge(buttonName, locked ? BadgeLockedId : null);
		}

		private void ApplyCardStateBadge(string buttonName, string badgeId)
		{
			Button button = FindButton(buttonName);
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			PlaceIndependentBadge(button.transform, CardStateBadgeName, badgeId, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(28f, 28f), new Vector2(-6f, -6f));
		}

		private void PlaceBadgeBesideText(Text text, string childName, string badgeId, Vector2 anchoredOffset, float size)
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
			rect.pivot = new Vector2(1f, 1f);
			rect.sizeDelta = new Vector2(size, size);
			rect.anchoredPosition = source.anchoredPosition + anchoredOffset;
			Sprite sprite = GetResultLobbyBadgeSprite(badgeId);
			image.sprite = sprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = (Object)(object)sprite != (Object)null;
		}

		private void PlaceIndependentBadge(Transform parent, string name, string badgeId, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
		{
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
			Sprite sprite = GetResultLobbyBadgeSprite(badgeId);
			image.sprite = sprite;
			image.color = Color.white;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = (Object)(object)sprite != (Object)null;
		}
	}
}
