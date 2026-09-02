using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Systems
{
	public static class BossClimaxFeedbackVfx
	{
		public const string WarningRingId = "VFX_Boss_Warning_Ring";
		public const string CoreExposeBurstId = "VFX_Boss_Core_Expose_Burst";
		public const string DefeatCascadeId = "VFX_Boss_Defeat_Cascade";
		public const string BossWarningSfxId = "SFX_Boss_Warning";
		public const string BossBreakSfxId = "SFX_Boss_Break";
		public const string BossDownSfxId = "SFX_Boss_Down";
		private const string WarningRingResourcesPath = "VFX/Boss/VFX_Boss_Warning_Ring";
		private const string CoreExposeBurstResourcesPath = "VFX/Boss/VFX_Boss_Core_Expose_Burst";
		private const string DefeatCascadeResourcesPath = "VFX/Boss/VFX_Boss_Defeat_Cascade";
		private const string WarningRingAssetPath = "Assets/Art/VFX/Boss/VFX_Boss_Warning_Ring.mat";
		private const string CoreExposeBurstAssetPath = "Assets/Art/VFX/Boss/VFX_Boss_Core_Expose_Burst.mat";
		private const string DefeatCascadeAssetPath = "Assets/Art/VFX/Boss/VFX_Boss_Defeat_Cascade.mat";

		private static readonly Color WarningRingColor = new Color(0.88f, 0.34f, 0.16f, 0.92f);
		private static readonly Color WarningRingEdge = new Color(0.96f, 0.52f, 0.18f, 0.78f);
		private static readonly Color DefeatSteel = new Color(0.68f, 0.82f, 0.96f, 1f);
		private static readonly Color DefeatEmber = new Color(0.42f, 0.58f, 0.78f, 1f);
		private static readonly Color BreakBurstColor = new Color(1f, 0.62f, 0.18f, 1f);
		private static readonly Color BreakBurstFlash = new Color(1f, 0.92f, 0.72f, 1f);
		private static ParticleSystem warningSparks;
		private static ParticleSystem breakSparks;
		private static ParticleSystem defeatBurst;
		private static Material warningRingMaterial;
		private static Material coreExposeBurstMaterial;
		private static Material defeatCascadeMaterial;

		public static void PlayWarningRing(Vector3 worldPosition, float radius, int threatLevel)
		{
			float reach = Mathf.Max(2.4f, radius);
			float intensity = Mathf.InverseLerp(1f, 3f, Mathf.Clamp(threatLevel, 1, 3));
			SpawnExpandingRing(WarningRingId, worldPosition, reach, Color.Lerp(WarningRingColor, WarningRingEdge, intensity), Mathf.Lerp(0.28f, 0.4f, intensity), 0.1f);
			EmitBurst(EnsureWarningSparks(), worldPosition + Vector3.up * 0.16f, intensity, 5, 9, WarningRingColor, WarningRingEdge);
		}

		public static void PlayBreakWindowBurst(Vector3 worldPosition)
		{
			Vector3 core = worldPosition + Vector3.up * 0.95f;
			SpawnCoreColumn(core);
			EmitBurst(EnsureBreakSparks(), core, 1f, 8, 12, BreakBurstColor, BreakBurstFlash);
		}

		public static void PlayDefeatCascade(Vector3 worldPosition)
		{
			EmitBurst(EnsureDefeatBurst(), worldPosition + Vector3.up * 0.55f, 1f, 14, 22, DefeatSteel, DefeatEmber);
			for (int i = 0; i < 6; i++)
			{
				float angle = (360f / 6f) * i;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				SpawnDefeatShard(worldPosition + dir * 0.85f + Vector3.up * 0.2f, dir);
			}
		}

		private static ParticleSystem EnsureWarningSparks()
		{
			if ((Object)(object)warningSparks != (Object)null)
			{
				return warningSparks;
			}

			warningSparks = CreateWorldBurst(WarningRingId, WarningRingColor, 0.05f, 0.11f, 1.4f, 3.2f, 0.1f, 0.2f, 20, EnsureWarningRingMaterial());
			var main = warningSparks.main;
			main.gravityModifier = 0.08f;
			return warningSparks;
		}

		private static ParticleSystem EnsureBreakSparks()
		{
			if ((Object)(object)breakSparks != (Object)null)
			{
				return breakSparks;
			}

			breakSparks = CreateWorldBurst(CoreExposeBurstId, BreakBurstFlash, 0.04f, 0.1f, 2.8f, 5.4f, 0.08f, 0.16f, 24, EnsureCoreExposeBurstMaterial());
			var main = breakSparks.main;
			main.gravityModifier = -0.35f;
			return breakSparks;
		}

		private static ParticleSystem EnsureDefeatBurst()
		{
			if ((Object)(object)defeatBurst != (Object)null)
			{
				return defeatBurst;
			}

			defeatBurst = CreateWorldBurst(DefeatCascadeId, DefeatSteel, 0.12f, 0.28f, 4.2f, 8.4f, 0.22f, 0.46f, 48, EnsureDefeatCascadeMaterial());
			var main = defeatBurst.main;
			main.gravityModifier = 1.15f;
			return defeatBurst;
		}

		private static ParticleSystem CreateWorldBurst(string id, Color color, float sizeMin, float sizeMax, float speedMin, float speedMax, float lifeMin, float lifeMax, int maxParticles, Material material)
		{
			Transform root = GetRuntimeRoot();
			GameObject go = new GameObject(id, typeof(ParticleSystem));
			go.transform.SetParent(root, false);
			ParticleSystem ps = go.GetComponent<ParticleSystem>();
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Clear(true);

			var main = ps.main;
			main.loop = false;
			main.playOnAwake = false;
			main.duration = 0.32f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.maxParticles = maxParticles;
			main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
			main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
			main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
			main.startColor = color;
			main.gravityModifier = 0.8f;

			var emission = ps.emission;
			emission.enabled = false;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.angle = 32f;
			shape.radius = 0.22f;

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.sharedMaterial = material;
			}

			return ps;
		}

		private static void EmitBurst(ParticleSystem ps, Vector3 worldPosition, float intensity, int minCount, int maxCount, Color colorA, Color colorB)
		{
			if ((Object)(object)ps == (Object)null)
			{
				return;
			}

			var main = ps.main;
			main.startColor = Color.Lerp(colorA, colorB, intensity);
			ps.transform.position = worldPosition;
			ps.transform.rotation = Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f);
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Emit(Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(minCount, maxCount, intensity))));
			ps.Play();
		}

		private static void SpawnExpandingRing(string id, Vector3 worldPosition, float reach, Color color, float life, float thickness)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = id;
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}

			go.transform.position = worldPosition + Vector3.up * 0.08f;
			go.transform.localScale = new Vector3(0.4f, Mathf.Max(0.08f, thickness), 0.4f);
			ApplyUnlitColor(go, color, WarningRingId, EnsureWarningRingMaterial());
			ClimaxRingFlash flash = go.AddComponent<ClimaxRingFlash>();
			flash.Configure(new Vector3(reach * 2f, Mathf.Max(0.08f, thickness), reach * 2f), color, life);
		}

		private static void SpawnCoreColumn(Vector3 worldPosition)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = CoreExposeBurstId;
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}

			go.transform.position = worldPosition;
			go.transform.localScale = new Vector3(0.22f, 0.2f, 0.22f);
			ApplyUnlitColor(go, BreakBurstFlash, CoreExposeBurstId, EnsureCoreExposeBurstMaterial());
			ClimaxRingFlash flash = go.AddComponent<ClimaxRingFlash>();
			flash.Configure(new Vector3(0.38f, 2.4f, 0.38f), BreakBurstColor, 0.2f);
		}

		private static void SpawnDefeatShard(Vector3 worldPosition, Vector3 outward)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = DefeatCascadeId;
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}

			go.transform.position = worldPosition;
			go.transform.rotation = Quaternion.LookRotation(outward, Vector3.up);
			go.transform.localScale = new Vector3(0.14f, 0.7f, 0.14f);
			ApplyUnlitColor(go, DefeatSteel, DefeatCascadeId, EnsureDefeatCascadeMaterial());
			ClimaxRingFlash flash = go.AddComponent<ClimaxRingFlash>();
			flash.Configure(new Vector3(0.1f, 1.8f, 0.1f), DefeatSteel, 0.38f);
		}

		private static void ApplyUnlitColor(GameObject go, Color color, string materialName, Material source)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			if ((Object)(object)renderer == (Object)null)
			{
				return;
			}

			Material material;
			if ((Object)(object)source != (Object)null)
			{
				material = new Material(source)
				{
					name = materialName
				};
			}
			else
			{
				Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
				if ((Object)(object)shader == (Object)null)
				{
					return;
				}

				material = new Material(shader)
				{
					name = materialName
				};
			}

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
				material.SetColor("_EmissionColor", color * 1.1f);
			}

			renderer.sharedMaterial = material;
		}

		private static Transform GetRuntimeRoot()
		{
			GameObject root = GameObject.Find("_RuntimeBossClimaxVfx");
			if ((Object)(object)root == (Object)null)
			{
				root = new GameObject("_RuntimeBossClimaxVfx");
				Object.DontDestroyOnLoad(root);
			}

			return root.transform;
		}

		private static Material EnsureWarningRingMaterial()
		{
			return CoalesceVfxMaterial(
				ref warningRingMaterial,
				WarningRingId,
				WarningRingResourcesPath,
				WarningRingAssetPath,
				WarningRingColor,
				WarningRingEdge);
		}

		private static Material EnsureCoreExposeBurstMaterial()
		{
			return CoalesceVfxMaterial(
				ref coreExposeBurstMaterial,
				CoreExposeBurstId,
				CoreExposeBurstResourcesPath,
				CoreExposeBurstAssetPath,
				BreakBurstColor,
				BreakBurstFlash);
		}

		private static Material EnsureDefeatCascadeMaterial()
		{
			return CoalesceVfxMaterial(
				ref defeatCascadeMaterial,
				DefeatCascadeId,
				DefeatCascadeResourcesPath,
				DefeatCascadeAssetPath,
				DefeatSteel,
				DefeatEmber);
		}

		private static Material CoalesceVfxMaterial(
			ref Material cache,
			string id,
			string resourcesPath,
			string assetPath,
			Color color,
			Color emission)
		{
			if ((Object)(object)cache != (Object)null)
			{
				return cache;
			}

			Material loaded = Resources.Load<Material>(resourcesPath);
			if ((Object)(object)loaded == (Object)null)
			{
#if UNITY_EDITOR
				loaded = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
				_ = assetPath;
#endif
			}

			if ((Object)(object)loaded != (Object)null)
			{
				cache = loaded;
				return cache;
			}

			Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
			                ?? Shader.Find("Particles/Standard Unlit")
			                ?? Shader.Find("Sprites/Default");
			if ((Object)(object)shader == (Object)null)
			{
				return null;
			}

			cache = new Material(shader)
			{
				name = id
			};
			if (cache.HasProperty("_Surface"))
			{
				cache.SetFloat("_Surface", 1f);
			}

			if (cache.HasProperty("_Blend"))
			{
				cache.SetFloat("_Blend", 0f);
			}

			if (cache.HasProperty("_BaseColor"))
			{
				cache.SetColor("_BaseColor", color);
			}

			if (cache.HasProperty("_Color"))
			{
				cache.SetColor("_Color", color);
			}

			if (cache.HasProperty("_EmissionColor"))
			{
				cache.EnableKeyword("_EMISSION");
				cache.SetColor("_EmissionColor", emission);
			}

			return cache;
		}

		private sealed class ClimaxRingFlash : MonoBehaviour
		{
			private Vector3 endScale;
			private Color color;
			private float duration;
			private float age;
			private Material material;

			public void Configure(Vector3 targetScale, Color markColor, float life)
			{
				endScale = targetScale;
				color = markColor;
				duration = Mathf.Max(0.1f, life);
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
				transform.position += Vector3.up * (endScale.y > 1f ? Time.unscaledDeltaTime * 2.4f : 0f);
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
