using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteOpenTrailVfx
	{
		public const string TrailVfxId = "VFX_RouteOpen_Trail";

		private static readonly Color OpenMagenta = new Color(0.94f, 0.18f, 0.7f, 0.92f);
		private static readonly Color OpenOrchid = new Color(1f, 0.58f, 0.94f, 0.9f);

		private static ParticleSystem trailBurst;
		private static Material sharedParticleMaterial;

		public static void Play(Vector3 worldPosition, Vector3 routeAim)
		{
			Vector3 origin = worldPosition;
			origin.y += 0.08f;
			Vector3 flat = routeAim - origin;
			flat.y = 0f;
			if (flat.sqrMagnitude < 0.36f)
			{
				flat = Vector3.forward * 4f;
			}

			Vector3 dir = flat.normalized;
			float reach = Mathf.Clamp(flat.magnitude, 3.2f, 8.5f);
			SpawnPathDashes(origin, dir, reach);
			SpawnBeaconPing(origin + dir * reach + Vector3.up * 0.14f);
			EmitBurst(EnsureTrailBurst(), origin + dir * 0.6f + Vector3.up * 0.06f, dir);
		}

		private static void SpawnPathDashes(Vector3 origin, Vector3 dir, float reach)
		{
			const int count = 5;
			for (int i = 0; i < count; i++)
			{
				float t = (i + 1f) / (count + 1f);
				Color color = Color.Lerp(OpenMagenta, OpenOrchid, t);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = TrailVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * (reach * t) + Vector3.up * 0.02f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.12f, 0.05f, 0.28f);
				ApplyUnlitColor(go, color, "M_Runtime_RouteOpenTrail");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.1f, 0.04f, 0.52f), color, 0.28f + i * 0.03f, dir * (2.4f + i * 0.35f));
			}
		}

		private static void SpawnBeaconPing(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = TrailVfxId;
			DestroyCollider(go);
			go.transform.position = origin;
			go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
			go.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
			ApplyUnlitColor(go, OpenOrchid, "M_Runtime_RouteOpenTrail");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.38f, 0.07f, 0.38f), OpenMagenta, 0.3f, Vector3.zero);
		}

		private static ParticleSystem EnsureTrailBurst()
		{
			if ((Object)(object)trailBurst != (Object)null)
			{
				return trailBurst;
			}

			trailBurst = CreateWorldBurst(TrailVfxId, OpenOrchid, 0.06f, 0.12f, 1.2f, 2.6f, 0.1f, 0.18f, 12);
			var main = trailBurst.main;
			main.gravityModifier = 0.02f;
			return trailBurst;
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
			main.duration = 0.24f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.maxParticles = maxParticles;
			main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
			main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
			main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
			main.startColor = color;
			main.gravityModifier = 0.08f;

			var emission = ps.emission;
			emission.enabled = false;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Box;
			shape.scale = new Vector3(0.18f, 0.04f, 1.1f);

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.material = GetSharedParticleMaterial();
			}

			return ps;
		}

		private static void EmitBurst(ParticleSystem ps, Vector3 worldPosition, Vector3 dir)
		{
			if ((Object)(object)ps == (Object)null)
			{
				return;
			}

			var main = ps.main;
			main.startColor = OpenOrchid;
			ps.transform.position = worldPosition;
			ps.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Emit(6);
			ps.Play();
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

			Material material = RoutePayoffVfxDrafts.Instantiate(TrailVfxId);
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
				material.SetColor("_EmissionColor", color * 1.08f);
			}

			renderer.sharedMaterial = material;
		}

		private static Transform GetRuntimeRoot()
		{
			GameObject root = GameObject.Find("_RuntimeRouteOpenTrailVfx");
			if ((Object)(object)root == (Object)null)
			{
				root = new GameObject("_RuntimeRouteOpenTrailVfx");
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

			Material draft = RoutePayoffVfxDrafts.Instantiate(TrailVfxId);
			if ((Object)(object)draft != (Object)null)
			{
				sharedParticleMaterial = draft;
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
				name = "M_Runtime_RouteOpenTrailVfx"
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
