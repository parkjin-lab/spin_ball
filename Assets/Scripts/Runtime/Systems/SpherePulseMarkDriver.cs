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
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = "VFX_SpherePulse_Mark";
			Collider collider = go.GetComponent<Collider>();
			if (collider != null)
			{
				Destroy(collider);
			}

			go.transform.position = worldCenter;
			go.transform.rotation = Quaternion.identity;

			SpherePulseMarkDriver driver = go.AddComponent<SpherePulseMarkDriver>();
			float reach = Mathf.Max(1.2f, radius * 1.15f);
			driver.startScale = new Vector3(0.42f, 0.12f, 0.42f);
			driver.endScale = new Vector3(reach, 0.12f, reach);
			driver.markColor = new Color(0.58f, 1f, 0.42f, 0.92f);
			go.transform.localScale = driver.startScale;
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
