using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteHoldSuccessVfx
	{
		public const string SuccessVfxId = "VFX_RouteHold_Success";

		private static readonly Color HoldGold = new Color(1f, 0.82f, 0.28f, 0.92f);
		private static readonly Color HoldCyan = new Color(0.38f, 0.9f, 1f, 0.9f);

		private static ParticleSystem successBurst;
		private static Material sharedParticleMaterial;

		public static void Play(Vector3 worldPosition, Vector3 routeAim)
		{
			Vector3 origin = worldPosition + Vector3.up * 0.16f;
			SpawnLockRing(origin);
			SpawnLockDashes(origin);
			SpawnBonusAimPips(origin, routeAim);
			EmitBurst(EnsureSuccessBurst(), origin + Vector3.up * 0.08f);
			if ((routeAim - worldPosition).sqrMagnitude > 1.2f)
			{
				SpawnMarkerPing(routeAim + Vector3.up * 0.2f);
			}
		}

		private static void SpawnLockRing(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = SuccessVfxId;
			DestroyCollider(go);
			go.transform.position = origin;
			go.transform.localScale = new Vector3(0.55f, 0.045f, 0.55f);
			ApplyUnlitColor(go, HoldGold, "M_Runtime_RouteHoldSuccess");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(2.15f, 0.04f, 2.15f), HoldCyan, 0.3f, Vector3.zero);
		}

		private static void SpawnLockDashes(Vector3 origin)
		{
			for (int i = 0; i < 4; i++)
			{
				float angle = 90f * i + 18f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(HoldCyan, HoldGold, (i % 2) * 0.55f);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = SuccessVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.48f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.12f, 0.07f, 0.34f);
				ApplyUnlitColor(go, color, "M_Runtime_RouteHoldSuccess");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.1f, 0.06f, 0.62f), color, 0.26f, dir * 1.8f);
			}
		}

		private static void SpawnBonusAimPips(Vector3 origin, Vector3 routeAim)
		{
			Vector3 flat = routeAim - origin;
			flat.y = 0f;
			if (flat.sqrMagnitude < 0.25f)
			{
				flat = Vector3.forward;
			}

			Vector3 dir = flat.normalized;
			for (int i = 0; i < 3; i++)
			{
				float t = 0.42f + i * 0.38f;
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = SuccessVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * t + Vector3.up * 0.04f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.1f, 0.1f, 0.16f);
				ApplyUnlitColor(go, HoldCyan, "M_Runtime_RouteHoldSuccess");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.08f, 0.08f, 0.28f), HoldGold, 0.28f, dir * 2.1f);
			}
		}

		private static void SpawnMarkerPing(Vector3 origin)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = SuccessVfxId;
			DestroyCollider(go);
			go.transform.position = origin;
			go.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
			go.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
			ApplyUnlitColor(go, HoldCyan, "M_Runtime_RouteHoldSuccess");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.34f, 0.08f, 0.34f), HoldGold, 0.32f, Vector3.zero);
		}

		private static ParticleSystem EnsureSuccessBurst()
		{
			if ((Object)(object)successBurst != (Object)null)
			{
				return successBurst;
			}

			successBurst = CreateWorldBurst(SuccessVfxId, HoldGold, 0.07f, 0.13f, 1.4f, 2.8f, 0.1f, 0.2f, 14);
			var main = successBurst.main;
			main.gravityModifier = 0.05f;
			return successBurst;
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
			main.duration = 0.26f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.maxParticles = maxParticles;
			main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
			main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
			main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
			main.startColor = color;
			main.gravityModifier = 0.1f;

			var emission = ps.emission;
			emission.enabled = false;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Circle;
			shape.radius = 0.32f;

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.material = GetSharedParticleMaterial();
			}

			return ps;
		}

		private static void EmitBurst(ParticleSystem ps, Vector3 worldPosition)
		{
			if ((Object)(object)ps == (Object)null)
			{
				return;
			}

			var main = ps.main;
			main.startColor = HoldCyan;
			ps.transform.position = worldPosition;
			ps.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
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

			Material material = RoutePayoffVfxDrafts.Instantiate(SuccessVfxId);
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
				material.SetColor("_EmissionColor", color * 1.1f);
			}

			renderer.sharedMaterial = material;
		}

		private static Transform GetRuntimeRoot()
		{
			GameObject root = GameObject.Find("_RuntimeRouteHoldSuccessVfx");
			if ((Object)(object)root == (Object)null)
			{
				root = new GameObject("_RuntimeRouteHoldSuccessVfx");
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

			Material draft = RoutePayoffVfxDrafts.Instantiate(SuccessVfxId);
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
				name = "M_Runtime_RouteHoldSuccessVfx"
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
					faded.a = Mathf.Lerp(0.92f, 0f, t);
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
