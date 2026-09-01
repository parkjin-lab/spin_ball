using AlienCrusher.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string SpendChangeReadyVfxId = "VFX_SpendChange_Ready";
		private const string SpendChangeReadyLabelName = "VFX_SpendChange_Ready_Label";
		private const string SpendChangeReadyPipName = "VFX_SpendChange_Ready_Pip";
		private static readonly Color SpendChangeJade = new Color(0.32f, 0.86f, 0.58f, 0.94f);
		private static readonly Color SpendChangeIvory = new Color(0.94f, 0.98f, 0.92f, 1f);

		private enum SpendChangeKind
		{
			None,
			Form,
			Meta
		}

		private Sprite spendChangeReadySprite;
		private bool spendChangeReadyArmed;
		private SpendChangeKind spendChangeReadyKind;
		private FormType spendChangeReadyForm;
		private FormUnlockSystem.MetaUpgradeType spendChangeReadyMeta;

		private void ArmSpendChangeReadyFromForm(FormType form)
		{
			spendChangeReadyArmed = true;
			spendChangeReadyKind = SpendChangeKind.Form;
			spendChangeReadyForm = form;
		}

		private void ArmSpendChangeReadyFromMeta(FormUnlockSystem.MetaUpgradeType upgradeType)
		{
			spendChangeReadyArmed = true;
			spendChangeReadyKind = SpendChangeKind.Meta;
			spendChangeReadyMeta = upgradeType;
		}

		private void PlaySpendChangeReadyPulse()
		{
			if (!spendChangeReadyArmed || spendChangeReadyKind == SpendChangeKind.None)
			{
				return;
			}

			spendChangeReadyArmed = false;
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}

			Transform hud = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)hud == (Object)null)
			{
				return;
			}

			Image plate = EnsureNamedIconImage(hud, SpendChangeReadyVfxId);
			if ((Object)(object)plate == (Object)null)
			{
				return;
			}

			RectTransform rect = plate.rectTransform;
			rect.anchorMin = new Vector2(0.5f, 1f);
			rect.anchorMax = new Vector2(0.5f, 1f);
			rect.pivot = new Vector2(0.5f, 1f);
			rect.sizeDelta = new Vector2(248f, 52f);
			rect.anchoredPosition = new Vector2(0f, -86f);
			plate.sprite = EnsureSpendChangeReadySprite();
			plate.color = SpendChangeJade;
			plate.preserveAspect = false;
			plate.raycastTarget = false;
			plate.enabled = true;
			plate.transform.SetAsLastSibling();

			PlaceSpendChangeReadyPip(plate.transform);
			PlaceSpendChangeReadyLabel(plate.transform, ResolveSpendChangeReadyLabel());

			SpendChangeReadyDriver driver = plate.GetComponent<SpendChangeReadyDriver>();
			if ((Object)(object)driver == (Object)null)
			{
				driver = plate.gameObject.AddComponent<SpendChangeReadyDriver>();
			}

			driver.Play(SpendChangeJade, 1.12f);
			spendChangeReadyKind = SpendChangeKind.None;
		}

		private void PlaceSpendChangeReadyPip(Transform parent)
		{
			Image pip = EnsureNamedIconImage(parent, SpendChangeReadyPipName);
			if ((Object)(object)pip == (Object)null)
			{
				return;
			}

			Sprite sprite = ResolveSpendChangeReadyPipSprite();
			RectTransform rect = pip.rectTransform;
			rect.anchorMin = new Vector2(0f, 0.5f);
			rect.anchorMax = new Vector2(0f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(34f, 34f);
			rect.anchoredPosition = new Vector2(28f, 0f);
			pip.sprite = sprite;
			pip.color = Color.white;
			pip.preserveAspect = true;
			pip.raycastTarget = false;
			pip.enabled = (Object)(object)sprite != (Object)null;
		}

		private void PlaceSpendChangeReadyLabel(Transform parent, string label)
		{
			Transform existing = FindDirectChild(parent, SpendChangeReadyLabelName);
			if ((Object)(object)existing == (Object)null)
			{
				GameObject go = new GameObject(SpendChangeReadyLabelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
				existing = go.transform;
				existing.SetParent(parent, false);
			}

			Text text = existing.GetComponent<Text>();
			if ((Object)(object)text == (Object)null)
			{
				text = existing.gameObject.AddComponent<Text>();
			}

			RectTransform rect = text.rectTransform;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = new Vector2(50f, 6f);
			rect.offsetMax = new Vector2(-12f, -6f);
			text.text = label;
			text.alignment = TextAnchor.MiddleLeft;
			text.fontSize = 18;
			text.fontStyle = FontStyle.Bold;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Overflow;
			text.color = SpendChangeIvory;
			text.raycastTarget = false;
			if ((Object)(object)text.font == (Object)null)
			{
				text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			}
		}

		private string ResolveSpendChangeReadyLabel()
		{
			if (spendChangeReadyKind == SpendChangeKind.Meta)
			{
				return GetMetaUpgradeName(spendChangeReadyMeta) + " READY";
			}

			return FormCatalog.GetDisplayName(spendChangeReadyForm) + " READY";
		}

		private Sprite ResolveSpendChangeReadyPipSprite()
		{
			if (spendChangeReadyKind == SpendChangeKind.Meta)
			{
				CacheOutgameProgressionVisuals();
				return spendChangeReadyMeta switch
				{
					FormUnlockSystem.MetaUpgradeType.ImpactCore => metaNodeImpactCoreSprite,
					FormUnlockSystem.MetaUpgradeType.DpAmplifier => metaNodeDpAmplifierSprite,
					_ => metaNodeSizeCoreSprite
				};
			}

			CacheFormIdentityIconSprites();
			return spendChangeReadyForm switch
			{
				FormType.Ram => iconFormRamSprite,
				FormType.Saucer => iconFormSaucerSprite,
				FormType.Spike => iconFormSpikeSprite,
				FormType.Crusher => iconFormCrusherSprite,
				_ => iconFormSphereSprite
			};
		}

		private Sprite EnsureSpendChangeReadySprite()
		{
			if ((Object)(object)spendChangeReadySprite != (Object)null)
			{
				return spendChangeReadySprite;
			}

			const int width = 160;
			const int height = 56;
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
			{
				name = SpendChangeReadyVfxId,
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
					float plate = Mathf.Clamp01(1f - Mathf.Max(dx - 0.86f, dy - 0.62f) / 0.12f);
					float notch = (nx < 0.08f && Mathf.Abs(ny - 0.5f) < 0.22f) ? 0.35f : 0f;
					float rim = Mathf.Clamp01(1f - Mathf.Abs(Mathf.Max(dx - 0.78f, dy - 0.46f)) / 0.1f);
					float slit = (nx > 0.18f && nx < 0.22f && Mathf.Abs(ny - 0.5f) < 0.28f) ? 0.55f : 0f;
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((plate * 0.88f + rim * 0.22f + slit * 0.18f - notch) * 255f), 0, 255);
					pixels[y * width + x] = new Color32(92, 220, 150, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			spendChangeReadySprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
			spendChangeReadySprite.name = SpendChangeReadyVfxId;
			return spendChangeReadySprite;
		}

		private sealed class SpendChangeReadyDriver : MonoBehaviour
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
				duration = Mathf.Max(0.35f, life);
				age = 0f;
				startScale = Vector3.one * 0.9f;
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one, hold);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.72f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.72f) / 0.28f);
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
