using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string StageUnlockReadyVfxId = "VFX_StageUnlock_Ready";
		private static readonly Color StageUnlockHoney = new Color(0.98f, 0.86f, 0.52f, 0.92f);

		private Sprite stageUnlockReadySprite;
		private bool stageUnlockReadyPlayedOnResult;
		private bool stageUnlockReadyPlayedOnLobby;

		private void PlaceStageUnlockReadyPulse()
		{
			if (!ShouldShowStageUnlockBanner())
			{
				stageUnlockReadyPlayedOnResult = false;
				stageUnlockReadyPlayedOnLobby = false;
				HideStageUnlockReadyPulse(resultPanel != null ? resultPanel.transform : null, ResultUnlockBannerName);
				HideStageUnlockReadyPulse(lobbyPanel != null ? lobbyPanel.transform : null, LobbyUnlockBannerName);
				return;
			}

			if (currentUiViewState == UiViewState.Result)
			{
				PlayStageUnlockReadyOnBanner(resultPanel != null ? resultPanel.transform : null, ResultUnlockBannerName, ref stageUnlockReadyPlayedOnResult);
				HideStageUnlockReadyPulse(lobbyPanel != null ? lobbyPanel.transform : null, LobbyUnlockBannerName);
				return;
			}

			if (currentUiViewState == UiViewState.Lobby)
			{
				PlayStageUnlockReadyOnBanner(lobbyPanel != null ? lobbyPanel.transform : null, LobbyUnlockBannerName, ref stageUnlockReadyPlayedOnLobby);
				HideStageUnlockReadyPulse(resultPanel != null ? resultPanel.transform : null, ResultUnlockBannerName);
			}
		}

		private void ResetStageUnlockReadyPulse()
		{
			stageUnlockReadyPlayedOnResult = false;
			stageUnlockReadyPlayedOnLobby = false;
		}

		private void PlayStageUnlockReadyOnBanner(Transform panel, string bannerName, ref bool played)
		{
			if ((Object)(object)panel == (Object)null)
			{
				return;
			}

			Transform banner = FindDirectChild(panel, bannerName);
			if ((Object)(object)banner == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(banner, StageUnlockReadyVfxId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.offsetMin = new Vector2(-14f, -10f);
			rect.offsetMax = new Vector2(14f, 10f);
			image.sprite = EnsureStageUnlockReadySprite();
			image.color = StageUnlockHoney;
			image.preserveAspect = false;
			image.raycastTarget = false;
			image.transform.SetAsLastSibling();
			if (played)
			{
				return;
			}

			played = true;
			StageUnlockReadyDriver driver = image.GetComponent<StageUnlockReadyDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<StageUnlockReadyDriver>();
			}

			driver.Play(StageUnlockHoney, 1.08f);
		}

		private void HideStageUnlockReadyPulse(Transform panel, string bannerName)
		{
			if ((Object)(object)panel == (Object)null)
			{
				return;
			}

			Transform banner = FindDirectChild(panel, bannerName);
			if ((Object)(object)banner == (Object)null)
			{
				return;
			}

			Transform existing = FindDirectChild(banner, StageUnlockReadyVfxId);
			if ((Object)(object)existing == (Object)null)
			{
				return;
			}

			Image image = existing.GetComponent<Image>();
			if ((Object)(object)image != (Object)null)
			{
				image.enabled = false;
			}
		}

		private Sprite EnsureStageUnlockReadySprite()
		{
			if ((Object)(object)stageUnlockReadySprite != (Object)null)
			{
				return stageUnlockReadySprite;
			}

			const int width = 176;
			const int height = 64;
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
			{
				name = StageUnlockReadyVfxId,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] pixels = new Color32[width * height];
			Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float nx = x / (float)(width - 1);
					float ny = y / (float)(height - 1);
					float dx = Mathf.Abs(x - center.x) / (width * 0.5f);
					float dy = Mathf.Abs(y - center.y) / (height * 0.5f);
					float corner = ((nx < 0.12f || nx > 0.88f) && (ny < 0.22f || ny > 0.78f)) ? 0.85f : 0f;
					float rise = (ny > 0.72f && Mathf.Abs(nx - 0.5f) < 0.18f) ? Mathf.Clamp01(1f - Mathf.Abs(nx - 0.5f) / 0.18f) * 0.7f : 0f;
					float underline = (ny < 0.16f && dx < 0.86f) ? 0.55f : 0f;
					float slit = ((nx < 0.08f || nx > 0.92f) && Mathf.Abs(ny - 0.5f) < 0.28f) ? 0.4f : 0f;
					float rim = Mathf.Clamp01(1f - Mathf.Abs(Mathf.Max(dx - 0.9f, dy - 0.72f)) / 0.1f);
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((corner * 0.9f + rise * 0.35f + underline * 0.28f + slit * 0.22f + rim * 0.18f) * 255f), 0, 255);
					pixels[y * width + x] = new Color32(250, 220, 132, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			stageUnlockReadySprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
			stageUnlockReadySprite.name = StageUnlockReadyVfxId;
			return stageUnlockReadySprite;
		}

		private sealed class StageUnlockReadyDriver : MonoBehaviour
		{
			private Image image;
			private Color color;
			private float duration;
			private float age;
			private Vector3 startScale;

			public void Play(Color pulseColor, float life)
			{
				image = GetComponent<Image>();
				color = pulseColor;
				duration = Mathf.Max(0.4f, life);
				age = 0f;
				startScale = Vector3.one * 0.88f;
				transform.localScale = startScale;
				if ((Object)(object)image != (Object)null)
				{
					image.enabled = true;
					image.color = color;
				}

				enabled = true;
			}

			private void Update()
			{
				age += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(age / duration);
				float hold = Mathf.Clamp01(t / 0.18f);
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.08f, hold);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.64f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.64f) / 0.36f);
					image.color = faded;
				}

				if (t >= 1f)
				{
					if ((Object)(object)image != (Object)null)
					{
						image.enabled = false;
					}

					enabled = false;
				}
			}
		}
	}
}
