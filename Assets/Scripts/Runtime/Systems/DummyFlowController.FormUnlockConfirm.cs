using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string FormUnlockConfirmVfxId = "VFX_FormUnlock_Confirm";
		private static readonly Color FormUnlockPeriwinkle = new Color(0.62f, 0.70f, 0.98f, 0.94f);

		private Sprite formUnlockConfirmSprite;

		private void PlayFormUnlockConfirmPulse(FormType form)
		{
			Button button = FindButton(GetFormEquipButtonName(form));
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, FormUnlockConfirmVfxId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.offsetMin = new Vector2(-14f, -14f);
			rect.offsetMax = new Vector2(14f, 14f);
			image.sprite = EnsureFormUnlockConfirmSprite();
			image.color = FormUnlockPeriwinkle;
			image.preserveAspect = false;
			image.raycastTarget = false;
			image.enabled = true;
			image.transform.SetAsLastSibling();
			FormUnlockPulseDriver driver = image.GetComponent<FormUnlockPulseDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<FormUnlockPulseDriver>();
			}

			driver.Play(FormUnlockPeriwinkle, 0.42f);
		}

		private Sprite EnsureFormUnlockConfirmSprite()
		{
			if ((Object)(object)formUnlockConfirmSprite != (Object)null)
			{
				return formUnlockConfirmSprite;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = FormUnlockConfirmVfxId,
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
					float dist = Mathf.Sqrt(nx * nx + ny * ny);
					float ang = Mathf.Atan2(ny, nx);
					float petal = Mathf.Abs(Mathf.Cos(ang * 2f));
					float burst = Mathf.Clamp01(1f - Mathf.Abs(dist - (0.38f + petal * 0.28f)) / 0.12f);
					float core = Mathf.Clamp01((0.22f - dist) / 0.12f);
					float tick = (dist > 0.18f && dist < 0.86f && petal > 0.72f) ? burst : 0f;
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((tick * 0.92f + core * 0.22f) * 255f), 0, 255);
					pixels[y * size + x] = new Color32(158, 178, 250, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			formUnlockConfirmSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
			formUnlockConfirmSprite.name = FormUnlockConfirmVfxId;
			return formUnlockConfirmSprite;
		}

		private sealed class FormUnlockPulseDriver : MonoBehaviour
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
				duration = Mathf.Max(0.18f, life);
				age = 0f;
				startScale = Vector3.one * 0.78f;
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.18f, t);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.55f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.55f) / 0.45f);
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
