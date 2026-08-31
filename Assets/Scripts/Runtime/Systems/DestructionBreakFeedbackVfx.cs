using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Systems
{
	public static class DestructionBreakFeedbackVfx
	{
		public const string DebrisLightId = "VFX_Debris_Light";
		public const string DebrisHeavyId = "VFX_Debris_Heavy";
		public const string SmokeDamageId = "VFX_Smoke_Damage";
		public const string WeakPointHitId = "VFX_WeakPoint_Hit";
		private const string DebrisLightResourcesPath = "VFX/Destruction/VFX_Debris_Light";
		private const string DebrisHeavyResourcesPath = "VFX/Destruction/VFX_Debris_Heavy";
		private const string SmokeDamageResourcesPath = "VFX/Destruction/VFX_Smoke_Damage";
		private const string WeakPointHitResourcesPath = "VFX/Destruction/VFX_WeakPoint_Hit";
		private const string DebrisLightAssetPath = "Assets/Art/VFX/Destruction/VFX_Debris_Light.mat";
		private const string DebrisHeavyAssetPath = "Assets/Art/VFX/Destruction/VFX_Debris_Heavy.mat";
		private const string SmokeDamageAssetPath = "Assets/Art/VFX/Destruction/VFX_Smoke_Damage.mat";
		private const string WeakPointHitAssetPath = "Assets/Art/VFX/Destruction/VFX_WeakPoint_Hit.mat";

		private static readonly Color LightDebrisColor = new Color(0.84f, 0.76f, 0.62f, 1f);
		private static readonly Color HeavyDebrisColor = new Color(0.22f, 0.2f, 0.18f, 1f);
		private static readonly Color HeavyDebrisEmber = new Color(0.72f, 0.32f, 0.1f, 1f);
		private static readonly Color SmokeColor = new Color(0.2f, 0.2f, 0.22f, 0.55f);
		private static readonly Color WeakPointSpark = new Color(1f, 0.84f, 0.16f, 1f);
		private static readonly Color ExposedCoreSpark = new Color(1f, 0.42f, 0.1f, 1f);

		private static ParticleSystem debrisLight;
		private static ParticleSystem debrisHeavy;
		private static ParticleSystem weakPointHit;
		private static Material debrisLightMaterial;
		private static Material debrisHeavyMaterial;
		private static Material smokeDamageMaterial;
		private static Material weakPointHitMaterial;

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

			debrisLight = CreateWorldBurst(DebrisLightId, LightDebrisColor, 0.05f, 0.12f, 1.6f, 4.2f, 0.12f, 0.26f, 24, EnsureDebrisLightMaterial());
			return debrisLight;
		}

		private static ParticleSystem EnsureDebrisHeavy()
		{
			if ((Object)(object)debrisHeavy != (Object)null)
			{
				return debrisHeavy;
			}

			debrisHeavy = CreateWorldBurst(DebrisHeavyId, HeavyDebrisColor, 0.1f, 0.24f, 3f, 6.8f, 0.2f, 0.42f, 40, EnsureDebrisHeavyMaterial());
			return debrisHeavy;
		}

		private static ParticleSystem EnsureWeakPointHit()
		{
			if ((Object)(object)weakPointHit != (Object)null)
			{
				return weakPointHit;
			}

			weakPointHit = CreateWorldBurst(WeakPointHitId, WeakPointSpark, 0.05f, 0.12f, 2.4f, 5.6f, 0.08f, 0.16f, 28, EnsureWeakPointHitMaterial());
			var main = weakPointHit.main;
			main.gravityModifier = 0.15f;
			return weakPointHit;
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
				renderer.sharedMaterial = material;
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
				renderer.sharedMaterial = EnsureSmokeDamageMaterial();
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
				Material source = EnsureWeakPointHitMaterial();
				Material material;
				if ((Object)(object)source != (Object)null)
				{
					material = new Material(source)
					{
						name = WeakPointHitId
					};
				}
				else
				{
					Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
					if ((Object)(object)shader == (Object)null)
					{
						material = null;
					}
					else
					{
						material = new Material(shader)
						{
							name = WeakPointHitId
						};
					}
				}

				if ((Object)(object)material != (Object)null)
				{
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

		private static Material EnsureDebrisLightMaterial()
		{
			return CoalesceVfxMaterial(
				ref debrisLightMaterial,
				DebrisLightId,
				DebrisLightResourcesPath,
				DebrisLightAssetPath,
				LightDebrisColor,
				Color.black);
		}

		private static Material EnsureDebrisHeavyMaterial()
		{
			return CoalesceVfxMaterial(
				ref debrisHeavyMaterial,
				DebrisHeavyId,
				DebrisHeavyResourcesPath,
				DebrisHeavyAssetPath,
				HeavyDebrisColor,
				HeavyDebrisEmber);
		}

		private static Material EnsureSmokeDamageMaterial()
		{
			return CoalesceVfxMaterial(
				ref smokeDamageMaterial,
				SmokeDamageId,
				SmokeDamageResourcesPath,
				SmokeDamageAssetPath,
				SmokeColor,
				new Color(0.05f, 0.05f, 0.06f, 1f));
		}

		private static Material EnsureWeakPointHitMaterial()
		{
			return CoalesceVfxMaterial(
				ref weakPointHitMaterial,
				WeakPointHitId,
				WeakPointHitResourcesPath,
				WeakPointHitAssetPath,
				WeakPointSpark,
				WeakPointSpark * 1.2f);
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

			Material loaded = LoadDraftVfxMaterial(resourcesPath, assetPath);
			if ((Object)(object)loaded != (Object)null)
			{
				cache = loaded;
				return cache;
			}

			cache = CreateParticleMaterial(id, color, emission);
			return cache;
		}

		private static Material LoadDraftVfxMaterial(string resourcesPath, string assetPath)
		{
			Material material = Resources.Load<Material>(resourcesPath);
			if ((Object)(object)material != (Object)null)
			{
				return material;
			}

#if UNITY_EDITOR
			return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
			_ = assetPath;
			return null;
#endif
		}

		private static Material CreateParticleMaterial(string id, Color color, Color emission)
		{
			Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
			                ?? Shader.Find("Particles/Standard Unlit")
			                ?? Shader.Find("Sprites/Default");
			if ((Object)(object)shader == (Object)null)
			{
				return null;
			}

			Material material = new Material(shader)
			{
				name = id
			};
			if (material.HasProperty("_Surface"))
			{
				material.SetFloat("_Surface", 1f);
			}

			if (material.HasProperty("_Blend"))
			{
				material.SetFloat("_Blend", 0f);
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
				material.SetColor("_EmissionColor", emission);
			}

			return material;
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
