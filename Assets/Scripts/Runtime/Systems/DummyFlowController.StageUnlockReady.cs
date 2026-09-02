using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string StageUnlockReadyVfxId = "VFX_StageUnlock_Ready";
		private const string ResultStageUnlockResidualName = "ResultStageUnlockResidual";
		private const string LobbyStageUnlockResidualName = "LobbyStageUnlockResidual";
		private static readonly Color StageUnlockHoney = new Color(0.98f, 0.86f, 0.52f, 0.72f);

		private Sprite stageUnlockReadySprite;
		private bool stageUnlockReadyPlayedOnResult;
		private bool stageUnlockReadyPlayedOnLobby;

		private void PlaceStageUnlockReadyPulse()
		{
			stageUnlockReadyPlayedOnResult = false;
			stageUnlockReadyPlayedOnLobby = false;
			HideStageUnlockReadyPulse(resultPanel != null ? resultPanel.transform : null, ResultStageUnlockResidualName);
			HideStageUnlockReadyPulse(lobbyPanel != null ? lobbyPanel.transform : null, LobbyStageUnlockResidualName);
		}

		private void ResetStageUnlockReadyPulse()
		{
			stageUnlockReadyPlayedOnResult = false;
			stageUnlockReadyPlayedOnLobby = false;
		}

		private void PlayStageUnlockReadyBesideBanner(Transform panel, string bannerName, string residualName, ref bool played)
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

			Image image = EnsureNamedIconImage(panel, residualName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = banner.GetComponent<RectTransform>();
			if ((Object)(object)source == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMax;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = source.sizeDelta + new Vector2(36f, 22f);
			rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, -6f);
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

			driver.Play(StageUnlockHoney, 1.35f);
		}

		private void HideStageUnlockReadyPulse(Transform panel, string residualName)
		{
			if ((Object)(object)panel == (Object)null)
			{
				return;
			}

			Transform existing = FindDirectChild(panel, residualName);
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
					float dx = Mathf.Abs(x - center.x) / (width * 0.5f);
					float dy = Mathf.Abs(y - center.y) / (height * 0.5f);
					float dist = Mathf.Max(dx * 0.72f, dy);
					float glow = Mathf.Clamp01(1f - dist);
					float halo = Mathf.Clamp01(1f - Mathf.Abs(dist - 0.58f) / 0.26f);
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((glow * 0.26f + halo * 0.64f) * 255f), 0, 255);
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

			public void Play(Color glowColor, float life)
			{
				image = GetComponent<Image>();
				color = glowColor;
				duration = Mathf.Max(0.5f, life);
				age = 0f;
				startScale = Vector3.one * 0.94f;
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.16f, t);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.38f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.38f) / 0.62f);
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
