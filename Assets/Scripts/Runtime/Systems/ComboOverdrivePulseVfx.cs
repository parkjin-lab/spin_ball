using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class ComboOverdrivePulseVfx
	{
		public const string ComboRisePulseId = "VFX_Combo_Rise_Pulse";
		public const string OverdrivePulseId = "VFX_Overdrive_Pulse";

		private static readonly Color ComboLime = new Color(0.58f, 0.96f, 0.22f, 0.94f);
		private static readonly Color ComboGold = new Color(1f, 0.86f, 0.22f, 0.9f);
		private static readonly Color OverdriveOrange = new Color(1f, 0.48f, 0.06f, 0.94f);
		private static readonly Color OverdriveFlame = new Color(1f, 0.74f, 0.16f, 0.88f);

		private static ParticleSystem comboRiseBurst;
		private static ParticleSystem overdriveBurst;
		private static Material sharedParticleMaterial;

		public static void PlayComboRisePulse(Vector3 worldPosition)
		{
			Vector3 origin = worldPosition + Vector3.up * 0.28f;
			for (int i = 0; i < 6; i++)
			{
				float angle = 60f * i + 12f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(ComboLime, ComboGold, (i % 2) * 0.72f);
				SpawnRisingTick(origin + dir * 0.36f, color);
			}

			EmitBurst(EnsureComboRiseBurst(), origin + Vector3.up * 0.18f, ComboGold);
		}

		public static void PlayOverdrivePulse(Vector3 worldPosition)
		{
			Vector3 origin = worldPosition + Vector3.up * 0.34f;
			SpawnSpeedRing(origin);
			for (int i = 0; i < 6; i++)
			{
				float angle = 60f * i;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				Color color = Color.Lerp(OverdriveOrange, OverdriveFlame, (i % 2) * 0.65f);
				SpawnFlameChevron(origin + dir * 0.52f, dir, color);
			}

			EmitBurst(EnsureOverdriveBurst(), origin, OverdriveFlame);
		}

		private static void SpawnRisingTick(Vector3 worldPosition, Color color)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = ComboRisePulseId;
			DestroyCollider(go);
			go.transform.position = worldPosition;
			go.transform.localScale = new Vector3(0.1f, 0.32f, 0.1f);
			ApplyUnlitColor(go, color, "M_Runtime_ComboRisePulse");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.1f, 1.15f, 0.1f), color, 0.22f, Vector3.up * 3.4f);
		}

		private static void SpawnSpeedRing(Vector3 worldPosition)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			go.name = OverdrivePulseId;
			DestroyCollider(go);
			go.transform.position = worldPosition;
			go.transform.localScale = new Vector3(0.7f, 0.1f, 0.7f);
			ApplyUnlitColor(go, OverdriveOrange, "M_Runtime_OverdrivePulse");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(2.6f, 0.1f, 2.6f), OverdriveFlame, 0.28f, Vector3.zero);
		}

		private static void SpawnFlameChevron(Vector3 worldPosition, Vector3 outward, Color color)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = OverdrivePulseId;
			DestroyCollider(go);
			go.transform.position = worldPosition + Vector3.up * 0.04f;
			go.transform.rotation = Quaternion.LookRotation(outward, Vector3.up) * Quaternion.Euler(18f, 0f, 45f);
			go.transform.localScale = new Vector3(0.22f, 0.12f, 0.42f);
			ApplyUnlitColor(go, color, "M_Runtime_OverdrivePulse");
			PulseFlash flash = go.AddComponent<PulseFlash>();
			flash.Configure(new Vector3(0.16f, 0.1f, 0.72f), color, 0.26f, outward * 2.8f);
		}

		private static ParticleSystem EnsureComboRiseBurst()
		{
			if ((Object)(object)comboRiseBurst != (Object)null)
			{
				return comboRiseBurst;
			}

			comboRiseBurst = CreateWorldBurst(ComboRisePulseId, ComboLime, 0.08f, 0.14f, 2.2f, 4.6f, 0.1f, 0.2f, 18);
			var main = comboRiseBurst.main;
			main.gravityModifier = -0.85f;
			return comboRiseBurst;
		}

		private static ParticleSystem EnsureOverdriveBurst()
		{
			if ((Object)(object)overdriveBurst != (Object)null)
			{
				return overdriveBurst;
			}

			overdriveBurst = CreateWorldBurst(OverdrivePulseId, OverdriveOrange, 0.1f, 0.18f, 2.6f, 5.2f, 0.12f, 0.22f, 22);
			var main = overdriveBurst.main;
			main.gravityModifier = 0.05f;
			return overdriveBurst;
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
			main.gravityModifier = 0.2f;

			var emission = ps.emission;
			emission.enabled = false;

			var shape = ps.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.angle = 22f;
			shape.radius = 0.18f;

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.renderMode = ParticleSystemRenderMode.Billboard;
				renderer.material = GetSharedParticleMaterial();
			}

			return ps;
		}

		private static void EmitBurst(ParticleSystem ps, Vector3 worldPosition, Color color)
		{
			if ((Object)(object)ps == (Object)null)
			{
				return;
			}

			var main = ps.main;
			main.startColor = color;
			ps.transform.position = worldPosition;
			ps.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
			ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			ps.Emit(8);
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
				material.SetColor("_EmissionColor", color * 1.15f);
			}

			renderer.sharedMaterial = material;
		}

		private static Transform GetRuntimeRoot()
		{
			GameObject root = GameObject.Find("_RuntimeComboOverdrivePulseVfx");
			if ((Object)(object)root == (Object)null)
			{
				root = new GameObject("_RuntimeComboOverdrivePulseVfx");
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
				name = "M_Runtime_ComboOverdrivePulseVfx"
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
