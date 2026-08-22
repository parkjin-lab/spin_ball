using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class ForwardSmashConfirmVfx
	{
		public const string ConfirmVfxId = "VFX_ForwardSmash_Confirm";

		private static readonly Color ConfirmMint = new Color(0.48f, 0.98f, 0.76f, 0.94f);
		private static readonly Color ConfirmIce = new Color(0.88f, 1f, 0.94f, 0.9f);

		private static ParticleSystem confirmBurst;
		private static Material sharedParticleMaterial;

		public static void Play(Vector3 worldPosition)
		{
			Vector3 origin = worldPosition + Vector3.up * 0.42f;
			SpawnImpactStar(origin);
			SpawnBrokenRing(origin);
			EmitBurst(EnsureConfirmBurst(), origin + Vector3.up * 0.12f);
		}

		private static void SpawnImpactStar(Vector3 origin)
		{
			for (int i = 0; i < 8; i++)
			{
				float angle = 45f * i + 8f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(ConfirmMint, ConfirmIce, (i % 2) * 0.7f);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = ConfirmVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.28f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.1f, 0.12f, 0.46f);
				ApplyUnlitColor(go, color, "M_Runtime_ForwardSmashConfirm");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.1f, 0.1f, 1.15f), color, 0.2f, dir * 3.6f);
			}
		}

		private static void SpawnBrokenRing(Vector3 origin)
		{
			const int segments = 6;
			for (int i = 0; i < segments; i++)
			{
				float angle = (360f / segments) * i;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(ConfirmMint, ConfirmIce, 0.35f);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
				go.name = ConfirmVfxId;
				DestroyCollider(go);
				go.transform.position = origin + dir * 0.72f;
				go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
				go.transform.localScale = new Vector3(0.42f, 0.1f, 0.14f);
				ApplyUnlitColor(go, color, "M_Runtime_ForwardSmashConfirm");
				PulseFlash flash = go.AddComponent<PulseFlash>();
				flash.Configure(new Vector3(0.7f, 0.1f, 0.12f), color, 0.24f, dir * 2.2f);
			}
		}

		private static ParticleSystem EnsureConfirmBurst()
		{
			if ((Object)(object)confirmBurst != (Object)null)
			{
				return confirmBurst;
			}

			confirmBurst = CreateWorldBurst(ConfirmVfxId, ConfirmIce, 0.08f, 0.16f, 2.4f, 4.8f, 0.1f, 0.18f, 16);
			var main = confirmBurst.main;
			main.gravityModifier = -0.2f;
			return confirmBurst;
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
			main.gravityModifier = 0.15f;

			var emission = ps.emission;
			emission.enabled = false;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.angle = 28f;
			shape.radius = 0.2f;

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
			main.startColor = ConfirmIce;
			ps.transform.position = worldPosition;
			ps.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Emit(7);
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
				material.SetColor("_EmissionColor", color * 1.12f);
			}

			renderer.sharedMaterial = material;
		}

		private static Transform GetRuntimeRoot()
		{
			GameObject root = GameObject.Find("_RuntimeForwardSmashConfirmVfx");
			if ((Object)(object)root == (Object)null)
			{
				root = new GameObject("_RuntimeForwardSmashConfirmVfx");
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
				name = "M_Runtime_ForwardSmashConfirmVfx"
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
