using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteHoldWarningVfx
	{
		public const string WarningVfxId = "VFX_RouteHold_Warning";

		private static readonly Color WarningRose = new Color(0.96f, 0.18f, 0.42f, 0.92f);
		private static readonly Color WarningBlush = new Color(1f, 0.62f, 0.72f, 0.9f);

		public static void Play(Vector3 beacon)
		{
			Vector3 origin = beacon;
			origin.y += 0.1f;
			SpawnClosingTicks(origin);
			SpawnBeaconPing(origin + Vector3.up * 0.08f);
		}

		private static void SpawnClosingTicks(Vector3 origin)
		{
			const int count = 4;
			for (int i = 0; i < count; i++)
			{
				float angle = 90f * i + 12f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(WarningRose, WarningBlush, (i % 2) * 0.7f);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = WarningVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.72f;
				go.transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
				go.transform.localScale = new Vector3(0.1f, 0.05f, 0.2f);
				ApplyUnlitColor(go, color, "M_Runtime_RouteHoldWarning");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.08f, 0.04f, 0.28f), color, 0.18f + i * 0.015f, -dir * 2.4f);
			}
		}

		private static void SpawnBeaconPing(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = WarningVfxId;
			DestroyCollider(go);
			go.transform.position = origin;
			go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
			go.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
			ApplyUnlitColor(go, WarningBlush, "M_Runtime_RouteHoldWarning");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.26f, 0.05f, 0.26f), WarningRose, 0.2f, Vector3.zero);
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

			Material material = RoutePayoffVfxDrafts.Instantiate(WarningVfxId);
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
