using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class DestructionBreakFeedbackVfx
	{
		public const string DebrisLightId = "VFX_Debris_Light";
		public const string DebrisHeavyId = "VFX_Debris_Heavy";
		public const string SmokeDamageId = "VFX_Smoke_Damage";
		public const string WeakPointHitId = "VFX_WeakPoint_Hit";

		private static readonly Color LightDebrisColor = new Color(0.84f, 0.76f, 0.62f, 1f);
		private static readonly Color HeavyDebrisColor = new Color(0.22f, 0.2f, 0.18f, 1f);
		private static readonly Color HeavyDebrisEmber = new Color(0.72f, 0.32f, 0.1f, 1f);
		private static readonly Color SmokeColor = new Color(0.2f, 0.2f, 0.22f, 0.55f);
		private static readonly Color WeakPointSpark = new Color(1f, 0.84f, 0.16f, 1f);
		private static readonly Color ExposedCoreSpark = new Color(1f, 0.42f, 0.1f, 1f);

		private static ParticleSystem debrisLight;
		private static ParticleSystem debrisHeavy;
		private static ParticleSystem weakPointHit;
		private static Material sharedParticleMaterial;

		public static void PlayDebrisLight(Vector3 worldPosition, float intensity = 0.55f)
		{
			EmitBurst(EnsureDebrisLight(), worldPosition, Mathf.Clamp01(intensity), 4, 8, LightDebrisColor, LightDebrisColor);
		}

		public static void PlayDebrisHeavy(Vector3 worldPosition, float intensity = 0.85f)
		{
			intensity = Mathf.Clamp01(intensity);
			EmitBurst(EnsureDebrisHeavy(), worldPosition, intensity, 8, 16, HeavyDebrisColor, Color.Lerp(HeavyDebrisColor, HeavyDebrisEmber, 0.45f));
		}

		public static void PlayWeakPointHit(Vector3 worldPosition, bool bossCore)
		{
			Color color = bossCore ? ExposedCoreSpark : WeakPointSpark;
			EmitBurst(EnsureWeakPointHit(), worldPosition, bossCore ? 0.9f : 0.7f, bossCore ? 8 : 6, bossCore ? 12 : 9, color, Color.Lerp(color, Color.white, 0.35f));
			SpawnWeakPointFlash(worldPosition, color, bossCore);
		}

		public static void SetSmokeOn(Transform host, float damageRatio, bool forceStop)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			ParticleSystem smoke = EnsureSmokeOn(host);
			if ((Object)(object)smoke == (Object)null)
			{
				return;
			}

			if (forceStop || damageRatio <= 0.28f)
			{
				smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				if (forceStop)
				{
					smoke.Clear(true);
				}

				return;
			}

			var emission = smoke.emission;
			emission.rateOverTime = Mathf.Lerp(2f, 16f, Mathf.InverseLerp(0.28f, 0.92f, Mathf.Clamp01(damageRatio)));
			smoke.transform.position = host.position + Vector3.up * Mathf.Max(0.35f, host.lossyScale.y * 0.52f);
			if (!smoke.isPlaying)
			{
				smoke.Play(true);
			}
		}

		public static void RenameExistingBreakVfx(ParticleSystem debris, ParticleSystem smoke)
		{
			if ((Object)(object)debris != (Object)null && debris.gameObject.name != DebrisHeavyId)
			{
				debris.gameObject.name = DebrisHeavyId;
			}

			if ((Object)(object)smoke != (Object)null && smoke.gameObject.name != SmokeDamageId)
			{
				smoke.gameObject.name = SmokeDamageId;
			}
		}

		private static ParticleSystem EnsureSmokeOn(Transform host)
		{
			Transform existing = host.Find(SmokeDamageId);
			if ((Object)(object)existing != (Object)null)
			{
				return existing.GetComponent<ParticleSystem>();
			}

			GameObject go = new GameObject(SmokeDamageId, typeof(ParticleSystem));
			go.transform.SetParent(host, false);
			ParticleSystem ps = go.GetComponent<ParticleSystem>();
			ConfigureSmoke(ps, host.lossyScale);
			return ps;
		}

		private static ParticleSystem EnsureDebrisLight()
		{
			if ((Object)(object)debrisLight != (Object)null)
			{
				return debrisLight;
			}

			debrisLight = CreateWorldBurst(DebrisLightId, LightDebrisColor, 0.05f, 0.12f, 1.6f, 4.2f, 0.12f, 0.26f, 24);
			return debrisLight;
		}

		private static ParticleSystem EnsureDebrisHeavy()
		{
			if ((Object)(object)debrisHeavy != (Object)null)
			{
				return debrisHeavy;
			}

			debrisHeavy = CreateWorldBurst(DebrisHeavyId, HeavyDebrisColor, 0.1f, 0.24f, 3f, 6.8f, 0.2f, 0.42f, 40);
			return debrisHeavy;
		}

		private static ParticleSystem EnsureWeakPointHit()
		{
			if ((Object)(object)weakPointHit != (Object)null)
			{
				return weakPointHit;
			}

			weakPointHit = CreateWorldBurst(WeakPointHitId, WeakPointSpark, 0.05f, 0.12f, 2.4f, 5.6f, 0.08f, 0.16f, 28);
			var main = weakPointHit.main;
			main.gravityModifier = 0.15f;
			return weakPointHit;
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
			main.duration = 0.28f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.maxParticles = maxParticles;
			main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
			main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
			main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
			main.startColor = color;
			main.gravityModifier = 0.95f;

			var emission = ps.emission;
			emission.enabled = false;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.angle = 26f;
			shape.radius = 0.16f;

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.material = GetSharedParticleMaterial();
			}

			return ps;
		}

		private static void ConfigureSmoke(ParticleSystem ps, Vector3 hostScale)
		{
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Clear(true);

			var main = ps.main;
			main.loop = true;
			main.playOnAwake = false;
			main.duration = 1.4f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.maxParticles = 90;
			main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
			main.startSpeed = new ParticleSystem.MinMaxCurve(0.22f, 0.7f);
			main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.62f);
			main.startColor = SmokeColor;

			var emission = ps.emission;
			emission.enabled = true;
			emission.rateOverTime = 0f;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Box;
			shape.scale = new Vector3(Mathf.Max(0.2f, Mathf.Abs(hostScale.x) * 0.45f), 0.16f, Mathf.Max(0.2f, Mathf.Abs(hostScale.z) * 0.45f));

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.material = GetSharedParticleMaterial();
			}
		}

		private static void EmitBurst(ParticleSystem ps, Vector3 worldPosition, float intensity, int minCount, int maxCount, Color colorA, Color colorB)
		{
			if ((Object)(object)ps == (Object)null)
			{
				return;
			}

			var main = ps.main;
			main.startColor = Color.Lerp(colorA, colorB, intensity);
			ps.transform.position = worldPosition + Vector3.up * 0.22f;
			ps.transform.rotation = Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f);
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Emit(Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(minCount, maxCount, intensity))));
			ps.Play();
		}

		private static void SpawnWeakPointFlash(Vector3 worldPosition, Color color, bool bossCore)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = WeakPointHitId;
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}

			float reach = bossCore ? 0.72f : 0.46f;
			go.transform.position = worldPosition;
			go.transform.localScale = new Vector3(0.16f, 0.06f, 0.16f);
			Renderer renderer = go.GetComponent<Renderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
				if ((Object)(object)shader != (Object)null)
				{
					Material material = new Material(shader)
					{
						name = "M_Runtime_WeakPointHit"
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
						material.SetColor("_EmissionColor", color * 1.2f);
					}

					renderer.sharedMaterial = material;
				}
			}

			WeakPointHitFlash flash = go.AddComponent<WeakPointHitFlash>();
			flash.Configure(new Vector3(reach, 0.06f, reach), color, bossCore ? 0.2f : 0.14f);
		}

		private static Transform GetRuntimeRoot()
		{
			GameObject root = GameObject.Find("_RuntimeDestructionBreakVfx");
			if ((Object)(object)root == (Object)null)
			{
				root = new GameObject("_RuntimeDestructionBreakVfx");
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
				name = "M_Runtime_DestructionBreakVfx"
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

		private sealed class WeakPointHitFlash : MonoBehaviour
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
