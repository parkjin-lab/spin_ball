using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class LaneBreakResidualVfx
	{
		public const string ResidualVfxId = "VFX_LaneBreak_Residual";

		private static readonly Color ResidualIvory = new Color(0.96f, 0.90f, 0.78f, 0.9f);
		private static readonly Color ResidualAsh = new Color(0.38f, 0.36f, 0.34f, 0.86f);

		private static Vector3 lastSmash = Vector3.zero;
		private static float lastSmashTime = -10f;

		public static void NoteSmash(Vector3 worldPosition)
		{
			lastSmash = worldPosition;
			lastSmashTime = Time.unscaledTime;
		}

		public static void Play(Vector3 fallbackPosition)
		{
			Vector3 origin = ((Time.unscaledTime - lastSmashTime) <= 0.4f) ? lastSmash : fallbackPosition;
			origin.y += 0.05f;
			SpawnResidualCrack(origin);
			SpawnImpactSliver(origin);
		}

		private static void SpawnResidualCrack(Vector3 origin)
		{
			for (int i = 0; i < 2; i++)
			{
				float yaw = 28f + i * 62f;
				Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
				Color color = (i == 0) ? ResidualAsh : ResidualIvory;
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = ResidualVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.04f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.05f, 0.03f, 0.22f);
				ApplyUnlitColor(go, color, "M_Runtime_LaneBreakResidual");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.04f, 0.02f, 0.38f), color, 0.28f, Vector3.zero);
			}
		}

		private static void SpawnImpactSliver(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = ResidualVfxId;
			DestroyCollider(go);
			go.transform.position = origin + Vector3.up * 0.1f;
			go.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
			ApplyUnlitColor(go, ResidualIvory, "M_Runtime_LaneBreakResidual");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.05f, 0.28f, 0.05f), ResidualIvory, 0.14f, Vector3.up * 0.35f);
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
				material.SetColor("_EmissionColor", color * 1.04f);
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
				duration = Mathf.Max(0.08f, life);
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
					faded.a = Mathf.Lerp(0.88f, 0f, t);
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
