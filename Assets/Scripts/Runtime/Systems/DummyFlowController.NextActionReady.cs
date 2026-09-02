using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string NextActionReadyVfxId = "VFX_NextAction_Ready";
		private const string ResultNextActionReadyName = "ResultNextActionReady";
		private static readonly Color NextActionLilac = new Color(0.80f, 0.58f, 0.96f, 0.94f);

		private Sprite nextActionReadySprite;
		private bool nextActionReadyPlayed;

		private void PlaceNextActionReadyPulse()
		{
			if (currentUiViewState != UiViewState.Result)
			{
				nextActionReadyPlayed = false;
				HideNextActionReadyPulse();
				return;
			}

			if (!ShouldShowNextActionReadyPulse())
			{
				HideNextActionReadyPulse();
				return;
			}

			if ((Object)(object)resultAdviceText == (Object)null)
			{
				return;
			}

			Transform parent = resultAdviceText.transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(parent, ResultNextActionReadyName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = resultAdviceText.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMin;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(48f, 48f);
			rect.anchoredPosition = source.anchoredPosition + ResolveNextActionReadyOffset();
			image.sprite = EnsureNextActionReadySprite();
			image.color = NextActionLilac;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.transform.SetAsLastSibling();
			if (nextActionReadyPlayed)
			{
				return;
			}

			nextActionReadyPlayed = true;
			NextActionReadyDriver driver = image.GetComponent<NextActionReadyDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<NextActionReadyDriver>();
			}

			driver.Play(NextActionLilac, 1.05f);
		}

		private bool ShouldShowNextActionReadyPulse()
		{
			int dp = ((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.DpBalance : 0;
			return HasReadyFormUnlock(dp) || HasReadyMetaUpgrade(dp) || lastRecommendedFormUnlock != FormType.Sphere;
		}

		private Vector2 ResolveNextActionReadyOffset()
		{
			if (ShouldShowFormReadyBadge())
			{
				return new Vector2(68f, -22f);
			}

			if (ShouldShowMetaReadyBadge())
			{
				return new Vector2(104f, -22f);
			}

			return new Vector2(68f, -22f);
		}

		private void HideNextActionReadyPulse()
		{
			if ((Object)(object)resultAdviceText == (Object)null)
			{
				return;
			}

			Transform parent = resultAdviceText.transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}

			Transform existing = FindDirectChild(parent, ResultNextActionReadyName);
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

		private Sprite EnsureNextActionReadySprite()
		{
			if ((Object)(object)nextActionReadySprite != (Object)null)
			{
				return nextActionReadySprite;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = NextActionReadyVfxId,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] pixels = new Color32[size * size];
			Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float nx = (x - center.x) / (size * 0.5f);
					float ny = (y - center.y) / (size * 0.5f);
					float chevron = Mathf.Abs(ny) * 0.72f + nx;
					float band = Mathf.Clamp01(1f - Mathf.Abs(chevron - 0.08f) / 0.16f);
					float inner = (nx > -0.62f && nx < 0.42f && Mathf.Abs(ny) < 0.78f) ? band : 0f;
					float tick = (nx > -0.18f && nx < 0.02f && Mathf.Abs(Mathf.Abs(ny) - 0.42f) < 0.08f) ? 0.7f : 0f;
					float cap = (nx > 0.28f && nx < 0.46f && Mathf.Abs(ny) < 0.18f) ? 0.55f : 0f;
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((inner * 0.92f + tick * 0.28f + cap * 0.2f) * 255f), 0, 255);
					pixels[y * size + x] = new Color32(204, 148, 245, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			nextActionReadySprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
			nextActionReadySprite.name = NextActionReadyVfxId;
			return nextActionReadySprite;
		}

		private sealed class NextActionReadyDriver : MonoBehaviour
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
				startScale = Vector3.one * 0.82f;
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
				float hold = Mathf.Clamp01(t / 0.16f);
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.12f, hold);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.62f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.62f) / 0.38f);
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
