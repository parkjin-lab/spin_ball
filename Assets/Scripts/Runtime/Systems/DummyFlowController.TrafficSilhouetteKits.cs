using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string PropCarCompactAId = "PROP_Car_Compact_A";
		private const string PropCarCompactBId = "PROP_Car_Compact_B";
		private const string PropVanBusId = "PROP_Van_Bus";

		private static readonly Color TrafficTireColor = new Color(0.09f, 0.09f, 0.11f, 1f);
		private static readonly Color TrafficGlassColor = new Color(0.16f, 0.24f, 0.30f, 1f);
		private static readonly Color TrafficTrimColor = new Color(0.22f, 0.23f, 0.25f, 1f);
		private static readonly Color TrafficHeadlightColor = new Color(0.96f, 0.86f, 0.58f, 1f);
		private static readonly Color TrafficTaillightColor = new Color(0.72f, 0.16f, 0.14f, 1f);

		private static void ApplyTrafficSilhouetteKit(Transform host, string vehicleName, Color bodyColor)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			HideLegacyTrafficMeshes(host);

			string kitId = ResolveTrafficSilhouetteKitId(vehicleName);
			SetNamedChildActive(host, PropCarCompactAId, kitId == PropCarCompactAId);
			SetNamedChildActive(host, PropCarCompactBId, kitId == PropCarCompactBId);
			SetNamedChildActive(host, PropVanBusId, kitId == PropVanBusId);

			Transform kit = GetOrCreateDirectChild(host, kitId);
			kit.localPosition = Vector3.zero;
			kit.localRotation = Quaternion.identity;
			kit.localScale = Vector3.one;
			kit.gameObject.SetActive(true);

			if (kitId == PropCarCompactBId)
			{
				BuildCompactCarBKit(kit, bodyColor);
				return;
			}

			if (kitId == PropVanBusId)
			{
				BuildVanBusKit(kit, bodyColor);
				return;
			}

			BuildCompactCarAKit(kit, bodyColor);
		}

		private static string ResolveTrafficSilhouetteKitId(string vehicleName)
		{
			int serial = ParseTrailingNumber(vehicleName);
			int variant = serial >= 0 ? serial % 3 : StableNameVariant(vehicleName);
			if (variant == 1)
			{
				return PropCarCompactBId;
			}

			if (variant == 2)
			{
				return PropVanBusId;
			}

			return PropCarCompactAId;
		}

		private static int StableNameVariant(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return 0;
			}

			int hash = 0;
			for (int i = 0; i < value.Length; i++)
			{
				hash = (hash * 31) + value[i];
			}

			return Mathf.Abs(hash) % 3;
		}

		private static void HideLegacyTrafficMeshes(Transform host)
		{
			SetNamedChildActive(host, "Body", false);
			SetNamedChildActive(host, "Cabin", false);
			SetNamedChildActive(host, "Wheel_FL", false);
			SetNamedChildActive(host, "Wheel_FR", false);
			SetNamedChildActive(host, "Wheel_RL", false);
			SetNamedChildActive(host, "Wheel_RR", false);
		}

		private static void BuildCompactCarAKit(Transform kit, Color bodyColor)
		{
			Color cabin = Color.Lerp(bodyColor, Color.white, 0.14f);
			Color hood = Color.Lerp(bodyColor, TrafficTrimColor, 0.18f);

			EnsureTrafficKitPiece(kit, "Body", PrimitiveType.Cube, new Vector3(0f, 0.22f, 0.02f), new Vector3(1.12f, 0.30f, 1.92f), bodyColor);
			EnsureTrafficKitPiece(kit, "Hood", PrimitiveType.Cube, new Vector3(0f, 0.34f, 0.62f), new Vector3(0.98f, 0.14f, 0.58f), hood);
			EnsureTrafficKitPiece(kit, "Cabin", PrimitiveType.Cube, new Vector3(0f, 0.48f, -0.18f), new Vector3(0.78f, 0.24f, 0.82f), cabin);
			EnsureTrafficKitPiece(kit, "Hatch", PrimitiveType.Cube, new Vector3(0f, 0.40f, -0.68f), new Vector3(0.74f, 0.16f, 0.28f), Color.Lerp(cabin, bodyColor, 0.35f));
			EnsureTrafficKitPiece(kit, "Glass", PrimitiveType.Cube, new Vector3(0f, 0.50f, 0.16f), new Vector3(0.70f, 0.14f, 0.12f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "Bumper_F", PrimitiveType.Cube, new Vector3(0f, 0.16f, 0.98f), new Vector3(1.08f, 0.12f, 0.14f), TrafficTrimColor);
			EnsureTrafficKitPiece(kit, "Bumper_R", PrimitiveType.Cube, new Vector3(0f, 0.16f, -0.96f), new Vector3(1.08f, 0.12f, 0.14f), TrafficTrimColor);
			EnsureTrafficKitPiece(kit, "Light_L", PrimitiveType.Cube, new Vector3(0.34f, 0.22f, 0.99f), new Vector3(0.16f, 0.08f, 0.08f), TrafficHeadlightColor);
			EnsureTrafficKitPiece(kit, "Light_R", PrimitiveType.Cube, new Vector3(-0.34f, 0.22f, 0.99f), new Vector3(0.16f, 0.08f, 0.08f), TrafficHeadlightColor);
			EnsureTrafficKitPiece(kit, "Tail_L", PrimitiveType.Cube, new Vector3(0.34f, 0.24f, -0.98f), new Vector3(0.14f, 0.08f, 0.08f), TrafficTaillightColor);
			EnsureTrafficKitPiece(kit, "Tail_R", PrimitiveType.Cube, new Vector3(-0.34f, 0.24f, -0.98f), new Vector3(0.14f, 0.08f, 0.08f), TrafficTaillightColor);
			AddTrafficWheels(kit, 0.46f, 0.56f, 0.12f, 0.22f);
		}

		private static void BuildCompactCarBKit(Transform kit, Color bodyColor)
		{
			Color cabin = Color.Lerp(bodyColor, TrafficGlassColor, 0.12f);
			Color roof = Color.Lerp(bodyColor, Color.white, 0.22f);

			EnsureTrafficKitPiece(kit, "Body", PrimitiveType.Cube, new Vector3(0f, 0.21f, 0f), new Vector3(1.12f, 0.28f, 1.94f), bodyColor);
			EnsureTrafficKitPiece(kit, "Hood", PrimitiveType.Cube, new Vector3(0f, 0.33f, 0.52f), new Vector3(1.00f, 0.16f, 0.72f), Color.Lerp(bodyColor, TrafficTrimColor, 0.12f));
			EnsureTrafficKitPiece(kit, "Cabin", PrimitiveType.Cube, new Vector3(0f, 0.50f, -0.06f), new Vector3(0.80f, 0.28f, 0.66f), cabin);
			EnsureTrafficKitPiece(kit, "Roof", PrimitiveType.Cube, new Vector3(0f, 0.66f, -0.08f), new Vector3(0.72f, 0.08f, 0.52f), roof);
			EnsureTrafficKitPiece(kit, "Trunk", PrimitiveType.Cube, new Vector3(0f, 0.32f, -0.74f), new Vector3(0.92f, 0.18f, 0.40f), Color.Lerp(bodyColor, Color.black, 0.08f));
			EnsureTrafficKitPiece(kit, "Glass", PrimitiveType.Cube, new Vector3(0.41f, 0.52f, -0.06f), new Vector3(0.08f, 0.16f, 0.48f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "Glass_R", PrimitiveType.Cube, new Vector3(-0.41f, 0.52f, -0.06f), new Vector3(0.08f, 0.16f, 0.48f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "Windshield", PrimitiveType.Cube, new Vector3(0f, 0.52f, 0.24f), new Vector3(0.70f, 0.14f, 0.10f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "Bumper_F", PrimitiveType.Cube, new Vector3(0f, 0.15f, 0.98f), new Vector3(1.10f, 0.12f, 0.14f), TrafficTrimColor);
			EnsureTrafficKitPiece(kit, "Bumper_R", PrimitiveType.Cube, new Vector3(0f, 0.15f, -0.96f), new Vector3(1.10f, 0.12f, 0.14f), TrafficTrimColor);
			EnsureTrafficKitPiece(kit, "Light_L", PrimitiveType.Cube, new Vector3(0.36f, 0.22f, 0.99f), new Vector3(0.18f, 0.08f, 0.08f), TrafficHeadlightColor);
			EnsureTrafficKitPiece(kit, "Light_R", PrimitiveType.Cube, new Vector3(-0.36f, 0.22f, 0.99f), new Vector3(0.18f, 0.08f, 0.08f), TrafficHeadlightColor);
			AddTrafficWheels(kit, 0.46f, 0.58f, 0.12f, 0.22f);
		}

		private static void BuildVanBusKit(Transform kit, Color bodyColor)
		{
			Color cabin = Color.Lerp(bodyColor, TrafficTrimColor, 0.16f);
			Color stripe = Color.Lerp(bodyColor, Color.white, 0.28f);

			EnsureTrafficKitPiece(kit, "Body", PrimitiveType.Cube, new Vector3(0f, 0.26f, 0.04f), new Vector3(1.16f, 0.36f, 2.28f), bodyColor);
			EnsureTrafficKitPiece(kit, "Cabin", PrimitiveType.Cube, new Vector3(0f, 0.62f, -0.12f), new Vector3(1.08f, 0.40f, 1.72f), cabin);
			EnsureTrafficKitPiece(kit, "Snout", PrimitiveType.Cube, new Vector3(0f, 0.32f, 0.92f), new Vector3(1.04f, 0.24f, 0.40f), Color.Lerp(bodyColor, TrafficTrimColor, 0.10f));
			EnsureTrafficKitPiece(kit, "RoofRail", PrimitiveType.Cube, new Vector3(0f, 0.84f, -0.10f), new Vector3(0.92f, 0.08f, 1.48f), Color.Lerp(cabin, Color.white, 0.10f));
			EnsureTrafficKitPiece(kit, "WindowBand", PrimitiveType.Cube, new Vector3(0.55f, 0.64f, -0.08f), new Vector3(0.08f, 0.18f, 1.42f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "WindowBand_R", PrimitiveType.Cube, new Vector3(-0.55f, 0.64f, -0.08f), new Vector3(0.08f, 0.18f, 1.42f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "Windshield", PrimitiveType.Cube, new Vector3(0f, 0.62f, 0.72f), new Vector3(0.92f, 0.22f, 0.10f), TrafficGlassColor);
			EnsureTrafficKitPiece(kit, "Stripe", PrimitiveType.Cube, new Vector3(0f, 0.42f, 0f), new Vector3(1.18f, 0.08f, 2.10f), stripe);
			EnsureTrafficKitPiece(kit, "Bumper_F", PrimitiveType.Cube, new Vector3(0f, 0.16f, 1.16f), new Vector3(1.14f, 0.14f, 0.16f), TrafficTrimColor);
			EnsureTrafficKitPiece(kit, "Bumper_R", PrimitiveType.Cube, new Vector3(0f, 0.16f, -1.12f), new Vector3(1.14f, 0.14f, 0.16f), TrafficTrimColor);
			EnsureTrafficKitPiece(kit, "Light_L", PrimitiveType.Cube, new Vector3(0.38f, 0.24f, 1.18f), new Vector3(0.18f, 0.08f, 0.08f), TrafficHeadlightColor);
			EnsureTrafficKitPiece(kit, "Light_R", PrimitiveType.Cube, new Vector3(-0.38f, 0.24f, 1.18f), new Vector3(0.18f, 0.08f, 0.08f), TrafficHeadlightColor);
			EnsureTrafficKitPiece(kit, "Tail_L", PrimitiveType.Cube, new Vector3(0.38f, 0.28f, -1.14f), new Vector3(0.16f, 0.10f, 0.08f), TrafficTaillightColor);
			EnsureTrafficKitPiece(kit, "Tail_R", PrimitiveType.Cube, new Vector3(-0.38f, 0.28f, -1.14f), new Vector3(0.16f, 0.10f, 0.08f), TrafficTaillightColor);
			AddTrafficWheels(kit, 0.50f, 0.78f, 0.13f, 0.26f);
		}

		private static void AddTrafficWheels(Transform kit, float halfWidth, float halfLength, float height, float diameter)
		{
			Vector3 scale = new Vector3(diameter, 0.10f, diameter);
			Vector3 spin = new Vector3(0f, 0f, 90f);
			EnsureTrafficKitPiece(kit, "Wheel_FL", PrimitiveType.Cylinder, new Vector3(halfWidth, height, halfLength), scale, TrafficTireColor, spin);
			EnsureTrafficKitPiece(kit, "Wheel_FR", PrimitiveType.Cylinder, new Vector3(-halfWidth, height, halfLength), scale, TrafficTireColor, spin);
			EnsureTrafficKitPiece(kit, "Wheel_RL", PrimitiveType.Cylinder, new Vector3(halfWidth, height, -halfLength), scale, TrafficTireColor, spin);
			EnsureTrafficKitPiece(kit, "Wheel_RR", PrimitiveType.Cylinder, new Vector3(-halfWidth, height, -halfLength), scale, TrafficTireColor, spin);
		}

		private static GameObject EnsureTrafficKitPiece(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, Vector3? localEuler = null)
		{
			GameObject go = EnsureVisualPrimitive(parent, name, type, localPosition, localScale, color, localEuler);
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider == (Object)null)
			{
				collider = go.AddComponent<BoxCollider>();
			}

			collider.isTrigger = true;
			return go;
		}
	}
}
