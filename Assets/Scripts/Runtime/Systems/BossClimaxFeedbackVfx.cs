using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class BossClimaxFeedbackVfx
	{
		public const string WarningRingId = "VFX_Boss_Warning_Ring";
		public const string DefeatCascadeId = "VFX_Boss_Defeat_Cascade";
		public const string BossWarningSfxId = "SFX_Boss_Warning";
		public const string BossBreakSfxId = "SFX_Boss_Break";
		public const string BossDownSfxId = "SFX_Boss_Down";

		private static readonly Color WarningRingColor = new Color(0.88f, 0.34f, 0.16f, 0.92f);
		private static readonly Color WarningRingEdge = new Color(0.96f, 0.52f, 0.18f, 0.78f);
		private static readonly Color DefeatSteel = new Color(0.68f, 0.82f, 0.96f, 1f);
		private static readonly Color DefeatEmber = new Color(0.42f, 0.58f, 0.78f, 1f);

		private static ParticleSystem warningSparks;
		private static ParticleSystem defeatBurst;
		private static Material sharedParticleMaterial;

		public static void PlayWarningRing(Vector3 worldPosition, float radius, int threatLevel)
		{
			float reach = Mathf.Max(2.4f, radius);
			float intensity = Mathf.InverseLerp(1f, 3f, Mathf.Clamp(threatLevel, 1, 3));
			SpawnExpandingRing(WarningRingId, worldPosition, reach, Color.Lerp(WarningRingColor, WarningRingEdge, intensity), Mathf.Lerp(0.28f, 0.4f, intensity), 0.1f);
			EmitBurst(EnsureWarningSparks(), worldPosition + Vector3.up * 0.16f, intensity, 5, 9, WarningRingColor, WarningRingEdge);
		}

		public static void PlayDefeatCascade(Vector3 worldPosition)
		{
			SpawnExpandingRing(DefeatCascadeId, worldPosition, 7.2f, DefeatSteel, 0.42f, 0.18f);
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

			warningSparks = CreateWorldBurst(WarningRingId, WarningRingColor, 0.05f, 0.11f, 1.4f, 3.2f, 0.1f, 0.2f, 20);
			var main = warningSparks.main;
			main.gravityModifier = 0.08f;
			return warningSparks;
		}

		private static ParticleSystem EnsureDefeatBurst()
		{
			if ((Object)(object)defeatBurst != (Object)null)
			{
				return defeatBurst;
			}

			defeatBurst = CreateWorldBurst(DefeatCascadeId, DefeatSteel, 0.12f, 0.28f, 4.2f, 8.4f, 0.22f, 0.46f, 48);
			var main = defeatBurst.main;
			main.gravityModifier = 1.15f;
			return defeatBurst;
		}

		private static ParticleSystem CreateWorldBurst(string id, Color color, float sizeMin, float sizeMax, float speedMin, float speedMax, float lifeMin, float lifeMax, int maxParticles)
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
				renderer.material = GetSharedParticleMaterial();
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
			ApplyUnlitColor(go, color, "M_Runtime_BossWarningRing");
			ClimaxRingFlash flash = go.AddComponent<ClimaxRingFlash>();
			flash.Configure(new Vector3(reach * 2f, Mathf.Max(0.08f, thickness), reach * 2f), color, life);
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
			ApplyUnlitColor(go, DefeatSteel, "M_Runtime_BossDefeatShard");
			ClimaxRingFlash flash = go.AddComponent<ClimaxRingFlash>();
			flash.Configure(new Vector3(0.1f, 1.8f, 0.1f), DefeatSteel, 0.38f);
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

		private static Material GetSharedParticleMaterial()
		{
			if ((Object)(object)sharedParticleMaterial != (Object)null)
			{
				return sharedParticleMaterial;
			}

			Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
			                ?? Shader.Find("Particles/Standard Unlit")
			                ?? Shader.Find("Sprites/Default");
			if ((Object)(object)shader == (Object)null)
			{
				return null;
			}

			sharedParticleMaterial = new Material(shader)
			{
				name = "M_Runtime_BossClimaxVfx"
			};
			if (sharedParticleMaterial.HasProperty("_Surface"))
			{
				sharedParticleMaterial.SetFloat("_Surface", 1f);
			}

			if (sharedParticleMaterial.HasProperty("_Blend"))
			{
				sharedParticleMaterial.SetFloat("_Blend", 0f);
			}

			return sharedParticleMaterial;
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
