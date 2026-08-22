using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string BossSentinelBodyKitId = "BOSS_Sentinel_Body_Kit";
		private const string BossShieldPylonKitId = "BOSS_Shield_Pylon_Kit";
		private const string BossPhase2DroneKitId = "BOSS_Phase2_Drone_Kit";

		private static void ClearBossIdentityKits(DummyDestructibleBlock block)
		{
			if ((Object)(object)block == (Object)null)
			{
				return;
			}

			SetNamedChildActive(block.transform, BossSentinelBodyKitId, false);
			SetNamedChildActive(block.transform, BossShieldPylonKitId, false);
			SetHostPrimaryRendererEnabled(block.transform, enabled: true);
		}

		private void ApplyNamedBossIdentityKit(DummyDestructibleBlock block, string kitId, Color roleColor)
		{
			if ((Object)(object)block == (Object)null)
			{
				return;
			}

			ApplyNamedBossIdentityKit(block.transform, kitId, roleColor);
		}

		private void ApplyNamedBossIdentityKit(Transform host, string kitId, Color roleColor)
		{
			if ((Object)(object)host == (Object)null || string.IsNullOrWhiteSpace(kitId))
			{
				return;
			}

			if (kitId == BossSentinelBodyKitId)
			{
				SetNamedChildActive(host, BossShieldPylonKitId, false);
			}
			else if (kitId == BossShieldPylonKitId)
			{
				SetNamedChildActive(host, BossSentinelBodyKitId, false);
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			HideLegacyDronePropMeshes(host);

			Transform kit = GetOrCreateDirectChild(host, kitId);
			kit.localPosition = Vector3.zero;
			kit.localRotation = Quaternion.identity;
			kit.localScale = Vector3.one;
			kit.gameObject.SetActive(true);

			if (kitId == BossSentinelBodyKitId)
			{
				BuildSentinelBodyKit(kit, roleColor);
				return;
			}

			if (kitId == BossShieldPylonKitId)
			{
				BuildShieldPylonKit(kit, roleColor);
				return;
			}

			if (kitId == BossPhase2DroneKitId)
			{
				BuildPhase2DroneKit(kit, roleColor);
			}
		}

		private static void BuildSentinelBodyKit(Transform kit, Color roleColor)
		{
			Color armor = Color.Lerp(roleColor, new Color(0.1f, 0.16f, 0.24f, 1f), 0.42f);
			Color plate = Color.Lerp(roleColor, Color.black, 0.28f);
			Color visor = Color.Lerp(roleColor, Color.white, 0.55f);
			Color core = new Color(0.72f, 0.94f, 1f, 1f);

			Vector3 size = AbsLossyScale(kit);
			float height = Mathf.Max(3.9f, size.y * 1.06f);
			float width = Mathf.Max(3.6f, size.x * 1.28f);
			float depth = Mathf.Max(2.15f, size.z * 0.84f);

			EnsureVisualPrimitive(kit, "StancePlinth", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(0f, -height * 0.42f, 0f)), WorldToLocalScale(kit, new Vector3(width * 0.72f, 0.1f, depth * 0.7f)), plate);
			EnsureVisualPrimitive(kit, "Torso", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, height * 0.04f, 0f)), WorldToLocalScale(kit, new Vector3(width * 0.72f, height * 0.5f, depth * 0.62f)), armor);
			EnsureVisualPrimitive(kit, "ChestPlate", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, height * 0.08f, depth * 0.22f)), WorldToLocalScale(kit, new Vector3(width * 0.42f, height * 0.28f, depth * 0.16f)), plate);
			EnsureVisualPrimitive(kit, "ChestCore", PrimitiveType.Sphere, WorldToLocalOffset(kit, new Vector3(0f, height * 0.1f, depth * 0.3f)), WorldToLocalScale(kit, new Vector3(0.42f, 0.42f, 0.18f)), core);
			EnsureVisualPrimitive(kit, "Head", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, height * 0.4f, depth * 0.04f)), WorldToLocalScale(kit, new Vector3(width * 0.3f, height * 0.2f, depth * 0.38f)), armor);
			EnsureVisualPrimitive(kit, "VisorAccent", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, height * 0.41f, depth * 0.22f)), WorldToLocalScale(kit, new Vector3(width * 0.22f, height * 0.07f, 0.1f)), visor);
			EnsureVisualPrimitive(kit, "CrownBlade", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, height * 0.56f, 0f)), WorldToLocalScale(kit, new Vector3(width * 0.08f, height * 0.16f, depth * 0.12f)), visor);
			EnsureVisualPrimitive(kit, "Shoulder_L", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(-width * 0.48f, height * 0.2f, 0f)), WorldToLocalScale(kit, new Vector3(width * 0.22f, height * 0.16f, depth * 0.48f)), plate);
			EnsureVisualPrimitive(kit, "Shoulder_R", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(width * 0.48f, height * 0.2f, 0f)), WorldToLocalScale(kit, new Vector3(width * 0.22f, height * 0.16f, depth * 0.48f)), plate);
			EnsureVisualPrimitive(kit, "Cannon_L", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(-width * 0.48f, height * 0.2f, depth * 0.28f)), WorldToLocalScale(kit, new Vector3(0.28f, depth * 0.28f, 0.28f)), visor, new Vector3(90f, 0f, 0f));
			EnsureVisualPrimitive(kit, "Cannon_R", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(width * 0.48f, height * 0.2f, depth * 0.28f)), WorldToLocalScale(kit, new Vector3(0.28f, depth * 0.28f, 0.28f)), visor, new Vector3(90f, 0f, 0f));
			EnsureVisualPrimitive(kit, "Hip", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, -height * 0.18f, 0f)), WorldToLocalScale(kit, new Vector3(width * 0.5f, height * 0.12f, depth * 0.5f)), plate);
			EnsureVisualPrimitive(kit, "Leg_L", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(-width * 0.18f, -height * 0.34f, depth * 0.04f)), WorldToLocalScale(kit, new Vector3(width * 0.16f, height * 0.28f, depth * 0.28f)), armor);
			EnsureVisualPrimitive(kit, "Leg_R", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(width * 0.18f, -height * 0.34f, depth * 0.04f)), WorldToLocalScale(kit, new Vector3(width * 0.16f, height * 0.28f, depth * 0.28f)), armor);
		}

		private static void BuildShieldPylonKit(Transform kit, Color roleColor)
		{
			Color post = Color.Lerp(roleColor, new Color(0.28f, 0.1f, 0.04f, 1f), 0.38f);
			Color beam = Color.Lerp(roleColor, Color.white, 0.34f);
			Color cap = Color.Lerp(roleColor, new Color(1f, 0.86f, 0.4f, 1f), 0.4f);

			Vector3 size = AbsLossyScale(kit);
			float height = Mathf.Max(2.7f, size.y * 1.1f);
			float span = Mathf.Clamp(Mathf.Max(size.x, size.z) * 0.48f, 1.15f, 2.7f);
			float postWidth = 0.38f;

			EnsureVisualPrimitive(kit, "Threshold", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, -height * 0.42f, 0f)), WorldToLocalScale(kit, new Vector3(span + 0.55f, 0.14f, 0.46f)), post);
			EnsureVisualPrimitive(kit, "Post_L", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(-span * 0.5f, 0f, 0f)), WorldToLocalScale(kit, new Vector3(postWidth, height * 0.52f, postWidth)), post);
			EnsureVisualPrimitive(kit, "Post_R", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(span * 0.5f, 0f, 0f)), WorldToLocalScale(kit, new Vector3(postWidth, height * 0.52f, postWidth)), post);
			EnsureVisualPrimitive(kit, "Lintel", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, height * 0.42f, 0f)), WorldToLocalScale(kit, new Vector3(span + 0.42f, 0.2f, 0.32f)), post);
			EnsureVisualPrimitive(kit, "EnergyPaneAccent", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, 0.02f, 0f)), WorldToLocalScale(kit, new Vector3(span * 0.72f, height * 0.72f, 0.08f)), beam);
			EnsureVisualPrimitive(kit, "Cap_L", PrimitiveType.Sphere, WorldToLocalOffset(kit, new Vector3(-span * 0.5f, height * 0.5f, 0f)), WorldToLocalScale(kit, new Vector3(0.32f, 0.22f, 0.32f)), cap);
			EnsureVisualPrimitive(kit, "Cap_R", PrimitiveType.Sphere, WorldToLocalOffset(kit, new Vector3(span * 0.5f, height * 0.5f, 0f)), WorldToLocalScale(kit, new Vector3(0.32f, 0.22f, 0.32f)), cap);
		}

		private static void BuildPhase2DroneKit(Transform kit, Color roleColor)
		{
			Color hull = Color.Lerp(roleColor, new Color(0.28f, 0.06f, 0.04f, 1f), 0.22f);
			Color rotor = new Color(0.16f, 0.12f, 0.14f, 1f);
			Color visor = Color.Lerp(roleColor, new Color(1f, 0.92f, 0.45f, 1f), 0.55f);

			EnsureVisualPrimitive(kit, "Hull", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, 0.04f, 0f)), WorldToLocalScale(kit, new Vector3(0.34f, 0.22f, 0.4f)), hull);
			EnsureVisualPrimitive(kit, "Keel", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, -0.04f, 0.02f)), WorldToLocalScale(kit, new Vector3(0.18f, 0.1f, 0.3f)), Color.Lerp(hull, Color.black, 0.2f));
			EnsureVisualPrimitive(kit, "RotorAccent_L", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(-0.28f, 0.1f, 0f)), WorldToLocalScale(kit, new Vector3(0.36f, 0.035f, 0.36f)), rotor);
			EnsureVisualPrimitive(kit, "RotorAccent_R", PrimitiveType.Cylinder, WorldToLocalOffset(kit, new Vector3(0.28f, 0.1f, 0f)), WorldToLocalScale(kit, new Vector3(0.36f, 0.035f, 0.36f)), rotor);
			EnsureVisualPrimitive(kit, "VisorAccent", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, 0.05f, 0.2f)), WorldToLocalScale(kit, new Vector3(0.2f, 0.08f, 0.06f)), visor);
			EnsureVisualPrimitive(kit, "TailFin", PrimitiveType.Cube, WorldToLocalOffset(kit, new Vector3(0f, 0.08f, -0.2f)), WorldToLocalScale(kit, new Vector3(0.06f, 0.16f, 0.14f)), hull);
		}

		private static void HideLegacyDronePropMeshes(Transform host)
		{
			if ((Object)(object)host == (Object)null || !host.name.StartsWith("BossDrone_", System.StringComparison.Ordinal))
			{
				return;
			}

			SetNamedChildActive(host, "Core", false);
			SetNamedChildActive(host, "Wing_L", false);
			SetNamedChildActive(host, "Wing_R", false);
		}

		private static void SetNamedChildActive(Transform parent, string name, bool active)
		{
			Transform child = FindDirectChild(parent, name);
			if ((Object)(object)child != (Object)null)
			{
				child.gameObject.SetActive(active);
			}
		}

		private static void SetHostPrimaryRendererEnabled(Transform host, bool enabled)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			Renderer renderer = host.GetComponent<Renderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.enabled = enabled;
			}
		}

		private static GameObject EnsureVisualPrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, Vector3? localEuler = null)
		{
			Transform existing = FindDirectChild(parent, name);
			GameObject go;
			if ((Object)(object)existing == (Object)null)
			{
				go = GameObject.CreatePrimitive(type);
				go.name = name;
				go.transform.SetParent(parent, false);
			}
			else
			{
				go = existing.gameObject;
			}

			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.DestroyImmediate(collider);
			}

			go.transform.localPosition = localPosition;
			go.transform.localRotation = Quaternion.Euler(localEuler ?? Vector3.zero);
			go.transform.localScale = SanitizeVisualScale(localScale);
			TintObject(go, color);
			return go;
		}

		private static Vector3 AbsLossyScale(Transform target)
		{
			Vector3 scale = target.lossyScale;
			return new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
		}

		private static Vector3 WorldToLocalScale(Transform parent, Vector3 worldScale)
		{
			Vector3 lossy = AbsLossyScale(parent);
			return new Vector3(
				worldScale.x / Mathf.Max(0.05f, lossy.x),
				worldScale.y / Mathf.Max(0.05f, lossy.y),
				worldScale.z / Mathf.Max(0.05f, lossy.z));
		}

		private static Vector3 WorldToLocalOffset(Transform parent, Vector3 worldOffset)
		{
			return WorldToLocalScale(parent, worldOffset);
		}

		private static Vector3 SanitizeVisualScale(Vector3 scale)
		{
			return new Vector3(
				Mathf.Max(0.015f, Mathf.Abs(scale.x)) * Mathf.Sign(scale.x == 0f ? 1f : scale.x),
				Mathf.Max(0.015f, Mathf.Abs(scale.y)) * Mathf.Sign(scale.y == 0f ? 1f : scale.y),
				Mathf.Max(0.015f, Mathf.Abs(scale.z)) * Mathf.Sign(scale.z == 0f ? 1f : scale.z));
		}
	}
}
