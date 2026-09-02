using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string StageSelectConfirmVfxId = "VFX_StageSelect_Confirm";
		private static readonly Color StageSelectIce = new Color(0.7f, 0.82f, 0.94f, 0.94f);

		private Sprite stageSelectConfirmSprite;

		private void PlayStageSelectConfirmPulse()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			Text stageText = FindText(canvasRootTransform, "StageSelectText");
			if ((Object)(object)stageText == (Object)null)
			{
				return;
			}

			Transform parent = stageText.transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(parent, StageSelectConfirmVfxId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform source = stageText.rectTransform;
			RectTransform rect = image.rectTransform;
			rect.anchorMin = source.anchorMin;
			rect.anchorMax = source.anchorMax;
			rect.pivot = source.pivot;
			rect.anchoredPosition = source.anchoredPosition;
			rect.sizeDelta = source.sizeDelta + new Vector2(36f, 22f);
			image.sprite = EnsureStageSelectConfirmSprite();
			image.color = StageSelectIce;
			image.preserveAspect = false;
			image.raycastTarget = false;
			image.enabled = true;
			image.transform.SetAsLastSibling();
			stageText.transform.SetAsLastSibling();

			StageSelectPulseDriver driver = image.GetComponent<StageSelectPulseDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<StageSelectPulseDriver>();
			}

			driver.Play(StageSelectIce, 0.28f);
		}

		private Sprite EnsureStageSelectConfirmSprite()
		{
			if ((Object)(object)stageSelectConfirmSprite != (Object)null)
			{
				return stageSelectConfirmSprite;
			}

			const int width = 96;
			const int height = 48;
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
			{
				name = StageSelectConfirmVfxId,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] pixels = new Color32[width * height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float nx = x / (float)(width - 1);
					float ny = y / (float)(height - 1);
					bool left = nx < 0.14f;
					bool right = nx > 0.86f;
					bool bottom = ny < 0.22f;
					bool top = ny > 0.78f;
					bool corner = ((left && (bottom || top)) || (right && (bottom || top)));
					float edge = 0f;
					if (corner)
					{
						edge = 1f;
					}
					else if ((left || right) && ny > 0.28f && ny < 0.72f)
					{
						edge = 0.22f;
					}

					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt(edge * 255f), 0, 255);
					pixels[y * width + x] = new Color32(188, 214, 240, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			stageSelectConfirmSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
			stageSelectConfirmSprite.name = StageSelectConfirmVfxId;
			return stageSelectConfirmSprite;
		}

		private sealed class StageSelectPulseDriver : MonoBehaviour
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
				duration = Mathf.Max(0.12f, life);
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.08f, t);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = Mathf.Lerp(0.94f, 0f, t);
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
