using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string MetaUpgradeConfirmVfxId = "VFX_MetaUpgrade_Confirm";
		private static readonly Color MetaUpgradeCopper = new Color(0.94f, 0.48f, 0.22f, 0.94f);

		private Sprite metaUpgradeConfirmSprite;

		private void PlayMetaUpgradeConfirmPulse(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			Button button = FindButton(GetMetaUpgradeButtonName(upgradeType));
			if ((Object)(object)button == (Object)null)
			{
				return;
			}

			Image image = EnsureNamedIconImage(button.transform, MetaUpgradeConfirmVfxId);
			if ((Object)(object)image == (Object)null)
			{
				return;
			}

			RectTransform rect = image.rectTransform;
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(56f, 56f);
			rect.anchoredPosition = Vector2.zero;
			image.sprite = EnsureMetaUpgradeConfirmSprite();
			image.color = MetaUpgradeCopper;
			image.preserveAspect = true;
			image.raycastTarget = false;
			image.enabled = true;
			image.transform.SetAsLastSibling();

			MetaUpgradePulseDriver driver = image.GetComponent<MetaUpgradePulseDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = image.gameObject.AddComponent<MetaUpgradePulseDriver>();
			}

			driver.Play(MetaUpgradeCopper, 0.26f);
		}

		private static string GetMetaUpgradeButtonName(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			return upgradeType switch
			{
				FormUnlockSystem.MetaUpgradeType.ImpactCore => "MetaUpgrade_ImpactButton",
				FormUnlockSystem.MetaUpgradeType.DpAmplifier => "MetaUpgrade_DpButton",
				_ => "MetaUpgrade_SizeButton"
			};
		}

		private Sprite EnsureMetaUpgradeConfirmSprite()
		{
			if ((Object)(object)metaUpgradeConfirmSprite != (Object)null)
			{
				return metaUpgradeConfirmSprite;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = MetaUpgradeConfirmVfxId,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] pixels = new Color32[size * size];
			Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dx = Mathf.Abs(x - center.x) / (size * 0.5f);
					float dy = Mathf.Abs(y - center.y) / (size * 0.5f);
					float diamond = dx + dy;
					float band = Mathf.Clamp01(1f - Mathf.Abs(diamond - 0.72f) / 0.14f);
					float core = Mathf.Clamp01((0.34f - diamond) / 0.14f);
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((band * 0.94f + core * 0.22f) * 255f), 0, 255);
					pixels[y * size + x] = new Color32(240, 122, 56, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			metaUpgradeConfirmSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
			metaUpgradeConfirmSprite.name = MetaUpgradeConfirmVfxId;
			return metaUpgradeConfirmSprite;
		}

		private sealed class MetaUpgradePulseDriver : MonoBehaviour
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
				startScale = Vector3.one * 0.72f;
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.28f, t);
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
