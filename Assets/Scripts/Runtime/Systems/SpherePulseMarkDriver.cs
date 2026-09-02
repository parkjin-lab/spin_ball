using UnityEngine;

namespace AlienCrusher.Systems
{
	public sealed class SpherePulseMarkDriver : MonoBehaviour
	{
		private Renderer cachedRenderer;
		private Material markMaterial;
		private Color markColor;
		private Vector3 startScale;
		private Vector3 endScale;
		private float duration = 0.32f;
		private float age;

		public static void Spawn(Vector3 worldCenter, float radius)
		{
			float reach = Mathf.Max(1.2f, radius * 1.15f);
			Spawn(
				worldCenter,
				"VFX_SpherePulse_Mark",
				PrimitiveType.Cylinder,
				Quaternion.identity,
				new Vector3(0.42f, 0.12f, 0.42f),
				new Vector3(reach, 0.12f, reach),
				new Color(0.58f, 1f, 0.42f, 0.92f));
		}

		public static void SpawnRamBreach(Vector3 worldCenter, float radius, Vector3 forward)
		{
			if (forward.sqrMagnitude < 0.001f)
			{
				forward = Vector3.forward;
			}

			forward.y = 0f;
			Quaternion rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
			float reach = Mathf.Max(1.4f, radius * 0.85f);
			Spawn(
				worldCenter,
				"VFX_RamBreach_Mark",
				PrimitiveType.Cube,
				rotation,
				new Vector3(0.28f, 0.16f, 0.42f),
				new Vector3(0.38f, 0.16f, reach),
				new Color(0.96f, 0.62f, 0.18f, 0.92f));
		}

		public static void SpawnSaucerDash(Vector3 worldCenter, float radius)
		{
			float reach = Mathf.Max(1.6f, radius * 1.35f);
			Spawn(
				worldCenter,
				"VFX_SaucerDash_Mark",
				PrimitiveType.Cylinder,
				Quaternion.identity,
				new Vector3(0.7f, 0.1f, 0.7f),
				new Vector3(reach, 0.1f, reach),
				new Color(0.32f, 0.88f, 0.94f, 0.9f));
		}

		public static void SpawnSpikeBurst(Vector3 worldCenter, float radius)
		{
			float reach = Mathf.Max(1.05f, radius * 0.52f);
			Color acid = new Color(0.82f, 1f, 0.18f, 0.92f);
			Vector3[] directions =
			{
				Vector3.forward,
				Vector3.back,
				Vector3.left,
				Vector3.right,
				new Vector3(0.55f, 0.85f, 0.2f).normalized
			};
			for (int i = 0; i < directions.Length; i++)
			{
				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, directions[i]);
				Spawn(
					worldCenter,
					"VFX_SpikeBurst_Mark",
					PrimitiveType.Cylinder,
					rotation,
					new Vector3(0.12f, 0.16f, 0.12f),
					new Vector3(0.12f, reach, 0.12f),
					acid,
					0.26f);
			}
		}

		public static void SpawnCrusherSlam(Vector3 worldCenter, float radius)
		{
			float reach = Mathf.Max(1.8f, radius * 0.72f);
			Spawn(
				worldCenter,
				"VFX_CrusherSlam_Mark",
				PrimitiveType.Cube,
				Quaternion.identity,
				new Vector3(0.72f, 0.55f, 0.72f),
				new Vector3(reach, 0.7f, reach),
				new Color(0.38f, 0.7f, 1f, 0.9f),
				0.46f);
		}

		private static void Spawn(
			Vector3 worldCenter,
			string name,
			PrimitiveType type,
			Quaternion rotation,
			Vector3 startScale,
			Vector3 endScale,
			Color color,
			float duration = 0.32f)
		{
			GameObject go = GameObject.CreatePrimitive(type);
			go.name = name;
			Collider collider = go.GetComponent<Collider>();
			if (collider != null)
			{
				Destroy(collider);
			}

			go.transform.position = worldCenter;
			go.transform.rotation = rotation;

			SpherePulseMarkDriver driver = go.AddComponent<SpherePulseMarkDriver>();
			driver.startScale = startScale;
			driver.endScale = endScale;
			driver.markColor = color;
			driver.duration = Mathf.Max(0.12f, duration);
			go.transform.localScale = startScale;
			driver.ApplyMaterial();
		}

		private void ApplyMaterial()
		{
			cachedRenderer = GetComponent<Renderer>();
			if (cachedRenderer == null)
			{
				return;
			}

			Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
			if (shader == null)
			{
				return;
			}

			markMaterial = new Material(shader)
			{
				name = "M_Runtime_SpherePulseMark"
			};
			if (markMaterial.HasProperty("_Surface"))
			{
				markMaterial.SetFloat("_Surface", 1f);
			}
			if (markMaterial.HasProperty("_Blend"))
			{
				markMaterial.SetFloat("_Blend", 0f);
			}
			if (markMaterial.HasProperty("_BaseColor"))
			{
				markMaterial.SetColor("_BaseColor", markColor);
			}
			if (markMaterial.HasProperty("_Color"))
			{
				markMaterial.SetColor("_Color", markColor);
			}
			if (markMaterial.HasProperty("_EmissionColor"))
			{
				markMaterial.EnableKeyword("_EMISSION");
				markMaterial.SetColor("_EmissionColor", markColor * 1.15f);
			}

			cachedRenderer.sharedMaterial = markMaterial;
		}

		private void Update()
		{
			age += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(age / Mathf.Max(0.08f, duration));
			transform.localScale = Vector3.Lerp(startScale, endScale, t);
			if (markMaterial != null)
			{
				Color faded = markColor;
				faded.a = Mathf.Lerp(markColor.a, 0f, t);
				if (markMaterial.HasProperty("_BaseColor"))
				{
					markMaterial.SetColor("_BaseColor", faded);
				}
				if (markMaterial.HasProperty("_Color"))
				{
					markMaterial.SetColor("_Color", faded);
				}
			}

			if (t >= 1f)
			{
				if (markMaterial != null)
				{
					Destroy(markMaterial);
				}

				Destroy(gameObject);
			}
		}
	}
}
