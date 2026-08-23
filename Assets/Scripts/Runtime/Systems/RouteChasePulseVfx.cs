using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteChasePulseVfx
	{
		public const string ChaseVfxId = "VFX_RouteChase_Pulse";

		private static readonly Color ChaseCobalt = new Color(0.22f, 0.52f, 0.96f, 0.92f);
		private static readonly Color ChaseIce = new Color(0.74f, 0.86f, 1f, 0.9f);

		public static void Play(Vector3 clusterOrigin, Vector3 smashTarget)
		{
			Vector3 origin = clusterOrigin;
			origin.y += 0.08f;
			Vector3 aim = smashTarget;
			aim.y = origin.y;
			Vector3 flat = aim - origin;
			flat.y = 0f;
			if (flat.sqrMagnitude < 0.64f)
			{
				flat = Vector3.forward * 4f;
			}

			Vector3 dir = flat.normalized;
			float reach = Mathf.Clamp(flat.magnitude, 2.4f, 7.2f);
			SpawnChaseWedges(origin, dir, reach);
			SpawnTargetPing(origin + dir * reach + Vector3.up * 0.12f);
		}

		private static void SpawnChaseWedges(Vector3 origin, Vector3 dir, float reach)
		{
			const int count = 3;
			for (int i = 0; i < count; i++)
			{
				float t = 0.22f + i * 0.22f;
				Color color = Color.Lerp(ChaseCobalt, ChaseIce, i / (count - 1f));
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = ChaseVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * (reach * t) + Vector3.up * 0.03f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(0f, 0f, 0f);
				go.transform.localScale = new Vector3(0.18f, 0.06f, 0.22f);
				ApplyUnlitColor(go, color, "M_Runtime_RouteChasePulse");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.14f, 0.05f, 0.34f), color, 0.2f + i * 0.03f, dir * (3.1f + i * 0.4f));
			}
		}

		private static void SpawnTargetPing(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = ChaseVfxId;
			DestroyCollider(go);
			go.transform.position = origin;
			go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
			go.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
			ApplyUnlitColor(go, ChaseIce, "M_Runtime_RouteChasePulse");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.32f, 0.06f, 0.32f), ChaseCobalt, 0.24f, Vector3.zero);
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

			Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
			if ((Object)(object)shader == (Object)null)
			{
				return;
			}

			Material material = new Material(shader)
			{
				name = materialName
			};
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
