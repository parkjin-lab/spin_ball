using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string FormEquipConfirmVfxId = "VFX_FormEquip_Confirm";
		private static readonly Color FormEquipChampagne = new Color(1f, 0.9f, 0.72f, 0.94f);

		private Sprite formEquipConfirmSprite;

		private void PlayFormEquipConfirmPulse(FormType form)
		{
			Button button = FindButton(GetFormEquipButtonName(form));
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, FormEquipConfirmVfxId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.offsetMin = new Vector2(-10f, -10f);
			rect.offsetMax = new Vector2(10f, 10f);
			image.sprite = EnsureFormEquipConfirmSprite();
			image.color = FormEquipChampagne;
			image.preserveAspect = false;
			image.raycastTarget = false;
			image.enabled = true;
			image.transform.SetAsLastSibling();
			FormEquipPulseDriver driver = image.GetComponent<FormEquipPulseDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<FormEquipPulseDriver>();
			}

			driver.Play(FormEquipChampagne, 0.26f);
		}

		private static string GetFormEquipButtonName(FormType form)
		{
			return form switch
			{
				FormType.Spike => "Form_Spike",
				FormType.Ram => "Form_Ram",
				FormType.Saucer => "Form_Saucer",
				FormType.Crusher => "Form_Crusher",
				_ => "Form_Sphere"
			};
		}

		private Sprite EnsureFormEquipConfirmSprite()
		{
			if ((Object)(object)formEquipConfirmSprite != (Object)null)
			{
				return formEquipConfirmSprite;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = FormEquipConfirmVfxId,
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
					float ring = Mathf.Clamp01(1f - Mathf.Abs(dist - 0.78f) / 0.14f);
					float inner = Mathf.Clamp01((0.68f - dist) / 0.12f);
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((ring * 0.92f + inner * 0.18f) * 255f), 0, 255);
					pixels[y * size + x] = new Color32(255, 236, 196, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			formEquipConfirmSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
			formEquipConfirmSprite.name = FormEquipConfirmVfxId;
			return formEquipConfirmSprite;
		}

		private sealed class FormEquipPulseDriver : MonoBehaviour
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
				startScale = Vector3.one * 0.86f;
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.22f, t);
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
