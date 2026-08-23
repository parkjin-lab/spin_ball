using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string NextActionResidualVfxId = "VFX_NextAction_Residual";
		private const string ResultNextActionResidualName = "ResultNextActionResidual";
		private static readonly Color NextActionMist = new Color(0.88f, 0.80f, 0.96f, 0.68f);

		private Sprite nextActionResidualSprite;
		private bool nextActionResidualPlayed;

		private void PlaceNextActionResidual()
		{
			if (currentUiViewState != UiViewState.Result)
			{
				nextActionResidualPlayed = false;
				HideNextActionResidual();
				return;
			}

			if (!ShouldShowNextActionReadyPulse())
			{
				HideNextActionResidual();
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

			Image image = EnsureNamedIconImage(parent, ResultNextActionResidualName);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = resultAdviceText.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMin;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(62f, 62f);
			rect.anchoredPosition = source.anchoredPosition + ResolveNextActionReadyOffset();
			image.sprite = EnsureNextActionResidualSprite();
			image.color = NextActionMist;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.transform.SetAsLastSibling();
			if (nextActionResidualPlayed)
			{
				return;
			}

			nextActionResidualPlayed = true;
			NextActionResidualDriver driver = image.GetComponent<NextActionResidualDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<NextActionResidualDriver>();
			}

			driver.Play(NextActionMist, 1.45f);
		}

		private void HideNextActionResidual()
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

			Transform existing = FindDirectChild(parent, ResultNextActionResidualName);
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

		private Sprite EnsureNextActionResidualSprite()
		{
			if ((Object)(object)nextActionResidualSprite != (Object)null)
			{
				return nextActionResidualSprite;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = NextActionResidualVfxId,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] pixels = new Color32[size * size];
			Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dist = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
					float glow = Mathf.Clamp01(1f - dist);
					float halo = Mathf.Clamp01(1f - Mathf.Abs(dist - 0.6f) / 0.28f);
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((glow * 0.26f + halo * 0.62f) * 255f), 0, 255);
					pixels[y * size + x] = new Color32(224, 204, 245, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			nextActionResidualSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
			nextActionResidualSprite.name = NextActionResidualVfxId;
			return nextActionResidualSprite;
		}

		private sealed class NextActionResidualDriver : MonoBehaviour
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
				startScale = Vector3.one * 0.92f;
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.2f, t);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.4f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.4f) / 0.6f);
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
