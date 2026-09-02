using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteBonusSuccessVfx
	{
		public const string BonusVfxId = "VFX_RouteBonus_Success";

		private static readonly Color BonusAmethyst = new Color(0.62f, 0.28f, 0.96f, 0.92f);
		private static readonly Color BonusPearl = new Color(0.9f, 0.78f, 1f, 0.9f);

		public static void Play(Vector3 clusterOrigin)
		{
			Vector3 origin = clusterOrigin;
			origin.y += 0.06f;
			SpawnClusterBloom(origin);
			SpawnUpwardPetals(origin);
		}

		private static void SpawnClusterBloom(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = BonusVfxId;
			DestroyCollider(go);
			go.transform.position = origin;
			go.transform.localScale = new Vector3(0.28f, 0.03f, 0.28f);
			ApplyUnlitColor(go, BonusPearl, "M_Runtime_RouteBonusSuccess");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.82f, 0.025f, 0.82f), BonusAmethyst, 0.22f, Vector3.zero);
		}

		private static void SpawnUpwardPetals(Vector3 origin)
		{
			const int count = 3;
			for (int i = 0; i < count; i++)
			{
				float angle = 120f * i + 8f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(BonusAmethyst, BonusPearl, i / (count - 1f));
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = BonusVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.16f + Vector3.up * 0.08f;
				go.transform.rotation = Quaternion.LookRotation(Vector3.up, dir);
				go.transform.localScale = new Vector3(0.08f, 0.08f, 0.16f);
				ApplyUnlitColor(go, color, "M_Runtime_RouteBonusSuccess");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.07f, 0.07f, 0.34f), color, 0.2f + i * 0.02f, Vector3.up * (1.8f + i * 0.2f));
			}
		}

		private static void DestroyCollider(GameObject go)
		{
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}
		}

		private static void ApplyUnlitColor(GameObject go, Color color, string materialName)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			if ((Object)(object)renderer == (Object)null)
			{
				return;
			}

			Material material = RoutePayoffVfxDrafts.Instantiate(BonusVfxId);
			if ((Object)(object)material == (Object)null)
			{
				Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
				if ((Object)(object)shader == (Object)null)
				{
					return;
				}

				material = new Material(shader);
			}

			material.name = materialName;
			if (material.HasProperty("_BaseColor"))
			{
				material.SetColor("_BaseColor", color);
			}

			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", color);
			}

			if (material.HasProperty("_EmissionColor"))
			{
				material.EnableKeyword("_EMISSION");
				material.SetColor("_EmissionColor", color * 1.06f);
			}

			renderer.sharedMaterial = material;
		}

		private sealed class PulseFlash : MonoBehaviour
		{
			private Vector3 endScale;
			private Color color;
			private float duration;
			private float age;
			private Vector3 travel;
			private Material material;

			public void Configure(Vector3 targetScale, Color markColor, float life, Vector3 travelVelocity)
			{
				endScale = targetScale;
				color = markColor;
				duration = Mathf.Max(0.1f, life);
				travel = travelVelocity;
				Renderer renderer = GetComponent<Renderer>();
				if ((Object)(object)renderer != (Object)null)
				{
					material = renderer.material;
				}
			}

			private void Update()
			{
				age += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(age / duration);
				transform.localScale = Vector3.Lerp(transform.localScale, endScale, t);
				transform.position += travel * Time.unscaledDeltaTime;
				if ((Object)(object)material != (Object)null)
				{
					Color faded = color;
					faded.a = Mathf.Lerp(0.9f, 0f, t);
					if (material.HasProperty("_BaseColor"))
					{
						material.SetColor("_BaseColor", faded);
					}

					if (material.HasProperty("_Color"))
					{
						material.SetColor("_Color", faded);
					}
				}

				if (t >= 1f)
				{
					if ((Object)(object)material != (Object)null)
					{
						Object.Destroy(material);
					}

					Object.Destroy(gameObject);
				}
			}
		}
	}
}
