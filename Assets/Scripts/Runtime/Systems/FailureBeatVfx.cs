using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Systems
{
	public static class FailureBeatVfx
	{
		public const string OrdinaryVfxId = "VFX_Failure_Ordinary";
		public const string BossVfxId = "VFX_Failure_Boss";
		private const string OrdinaryResourcesPath = "VFX/Failure/VFX_Failure_Ordinary";
		private const string BossResourcesPath = "VFX/Failure/VFX_Failure_Boss";
		private const string OrdinaryAssetPath = "Assets/Art/VFX/Failure/VFX_Failure_Ordinary.mat";
		private const string BossAssetPath = "Assets/Art/VFX/Failure/VFX_Failure_Boss.mat";

		private static readonly Color OrdinaryUmber = new Color(0.64f, 0.3f, 0.16f, 0.92f);
		private static readonly Color OrdinaryTan = new Color(0.78f, 0.48f, 0.28f, 0.9f);
		private static readonly Color BossSlag = new Color(0.4f, 0.42f, 0.46f, 0.92f);
		private static readonly Color BossAsh = new Color(0.56f, 0.54f, 0.52f, 0.9f);

		public static void Play(Vector3 center, bool bossRelated)
		{
			Vector3 origin = center;
			origin.y += 0.08f;
			if (bossRelated)
			{
				SpawnBossCollapse(origin);
			}
			else
			{
				SpawnOrdinaryBreak(origin);
			}
		}

		private static void SpawnOrdinaryBreak(Vector3 origin)
		{
			for (int i = 0; i < 3; i++)
			{
				float angle = 120f * i + 18f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(OrdinaryUmber, OrdinaryTan, (i % 2) * 0.7f);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = OrdinaryVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.28f + Vector3.up * 0.22f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(55f, 0f, 0f);
				go.transform.localScale = new Vector3(0.16f, 0.05f, 0.22f);
				ApplyUnlitColor(go, color, "M_Runtime_FailureOrdinary", OrdinaryResourcesPath, OrdinaryAssetPath);
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.12f, 0.04f, 0.3f), color, 0.26f + i * 0.02f, Vector3.down * 1.6f);
			}

			GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
			bar.name = OrdinaryVfxId;
			DestroyCollider(bar);
			bar.transform.position = origin;
			bar.transform.localScale = new Vector3(0.62f, 0.05f, 0.08f);
			ApplyUnlitColor(bar, OrdinaryUmber, "M_Runtime_FailureOrdinary", OrdinaryResourcesPath, OrdinaryAssetPath);
			PulseFlash barFlash = bar.AddComponent<PulseFlash>();
			barFlash.Configure(new Vector3(0.18f, 0.04f, 0.06f), OrdinaryTan, 0.24f, Vector3.down * 0.8f);
		}

		private static void SpawnBossCollapse(Vector3 origin)
		{
			for (int i = 0; i < 5; i++)
			{
				float angle = 72f * i + 8f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(BossSlag, BossAsh, (i % 2) * 0.65f);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = BossVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.42f + Vector3.up * 0.38f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.1f, 0.28f, 0.08f);
				ApplyUnlitColor(go, color, "M_Runtime_FailureBoss", BossResourcesPath, BossAssetPath);
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.08f, 0.12f, 0.06f), color, 0.3f + i * 0.012f, Vector3.down * 2.2f);
			}

			GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
			slab.name = BossVfxId;
			DestroyCollider(slab);
			slab.transform.position = origin + Vector3.up * 0.06f;
			slab.transform.localScale = new Vector3(0.72f, 0.06f, 0.72f);
			ApplyUnlitColor(slab, BossAsh, "M_Runtime_FailureBoss", BossResourcesPath, BossAssetPath);
			PulseFlash slabFlash = slab.AddComponent<PulseFlash>();
			slabFlash.Configure(new Vector3(0.34f, 0.04f, 0.34f), BossSlag, 0.28f, Vector3.down * 0.6f);
		}

		private static void DestroyCollider(GameObject go)
		{
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}
		}

		private static void ApplyUnlitColor(GameObject go, Color color, string materialName, string resourcesPath, string assetPath)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			if ((Object)(object)renderer == (Object)null)
			{
				return;
			}

			Material material = TryLoadDraftMaterial(resourcesPath, assetPath);
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
				material.SetColor("_EmissionColor", color * 1.04f);
			}

			renderer.sharedMaterial = material;
		}

		private static Material TryLoadDraftMaterial(string resourcesPath, string assetPath)
		{
			Material loaded = Resources.Load<Material>(resourcesPath);
			if ((Object)(object)loaded == (Object)null)
			{
#if UNITY_EDITOR
				loaded = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
				_ = assetPath;
#endif
			}

			if ((Object)(object)loaded == (Object)null)
			{
				return null;
			}

			return new Material(loaded);
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
