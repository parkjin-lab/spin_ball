using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string DpGainResidualVfxId = "VFX_DP_Gain_Residual";
		private const string ResultDpGainResidualName = "ResultDpGainResidual";
		private const string LobbyDpGainResidualName = "LobbyDpGainResidual";
		private static readonly Color DpGainResidualAqua = new Color(0.52f, 0.9f, 0.88f, 0.72f);

		private Sprite dpGainResidualSprite;

		private void PlaceDpGainResidual(Text text, string childName, OutgameDpSignal signal, Vector2 anchoredOffset, float size)
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

			image.enabled = false;
		}

		private Sprite EnsureDpGainResidualSprite()
		{
			if ((Object)(object)dpGainResidualSprite != (Object)null)
			{
				return dpGainResidualSprite;
			}

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = DpGainResidualVfxId,
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
					float halo = Mathf.Clamp01(1f - Mathf.Abs(dist - 0.62f) / 0.28f);
					byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((glow * 0.28f + halo * 0.62f) * 255f), 0, 255);
					pixels[y * size + x] = new Color32(168, 236, 230, alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			dpGainResidualSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
			dpGainResidualSprite.name = DpGainResidualVfxId;
			return dpGainResidualSprite;
		}

		private sealed class DpGainResidualDriver : MonoBehaviour
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
				duration = Mathf.Max(0.4f, life);
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
				transform.localScale = Vector3.Lerp(startScale, Vector3.one * 1.18f, t);
				if ((Object)(object)image != (Object)null)
				{
					Color faded = color;
					faded.a = t < 0.35f ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.35f) / 0.65f);
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
