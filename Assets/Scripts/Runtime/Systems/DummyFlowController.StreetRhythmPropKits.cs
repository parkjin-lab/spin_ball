using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string PropStreetLampId = "PROP_StreetLamp";
		private const string PropTrafficLightId = "PROP_TrafficLight";
		private const string PropRoadsideTreeId = "PROP_RoadsideTree";
		private const string PropBenchId = "PROP_Bench";

		private static readonly Color StreetPostColor = new Color(0.58f, 0.62f, 0.68f, 1f);
		private static readonly Color StreetCapColor = new Color(0.98f, 0.86f, 0.48f, 1f);
		private static readonly Color StreetHousingColor = new Color(0.17f, 0.18f, 0.20f, 1f);
		private static readonly Color StreetSignalRed = new Color(0.78f, 0.16f, 0.14f, 1f);
		private static readonly Color StreetSignalAmber = new Color(0.90f, 0.58f, 0.16f, 1f);
		private static readonly Color StreetSignalGreen = new Color(0.18f, 0.62f, 0.28f, 1f);
		private static readonly Color StreetTrunkColor = new Color(0.34f, 0.24f, 0.16f, 1f);

		private static void ApplyStreetLampOrTrafficLightKit(Transform host, string propName)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetNamedChildRendererEnabled(host, "Pole", false);
			SetNamedChildRendererEnabled(host, "Head", false);

			string kitId = ResolveStreetLampKitId(propName);
			SetNamedChildActive(host, PropStreetLampId, kitId == PropStreetLampId);
			SetNamedChildActive(host, PropTrafficLightId, kitId == PropTrafficLightId);

			Transform kit = GetOrCreateDirectChild(host, kitId);
			kit.localPosition = Vector3.zero;
			kit.localRotation = Quaternion.identity;
			kit.localScale = Vector3.one;
			kit.gameObject.SetActive(true);

			if (kitId == PropTrafficLightId)
			{
				BuildTrafficLightKit(kit);
				return;
			}

			BuildStreetLampKit(kit);
		}

		private static void ApplyRoadsideTreeKit(Transform host, Color leafColor)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetNamedChildRendererEnabled(host, "Trunk", false);
			SetNamedChildRendererEnabled(host, "Leaves", false);
			SetNamedChildActive(host, PropRoadsideTreeId, true);

			Transform kit = GetOrCreateDirectChild(host, PropRoadsideTreeId);
			kit.localPosition = Vector3.zero;
			kit.localRotation = Quaternion.identity;
			kit.localScale = Vector3.one;
			kit.gameObject.SetActive(true);
			BuildRoadsideTreeKit(kit, leafColor);
		}

		private static void ApplyBenchKit(Transform host, Color color)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "BenchSeat", false);
			SetNamedChildRendererEnabled(host, "BenchBack", false);
			SetNamedChildRendererEnabled(host, "BenchLeg_L", false);
			SetNamedChildRendererEnabled(host, "BenchLeg_R", false);
			SetNamedChildActive(host, PropBenchId, true);

			Transform kit = GetOrCreateDirectChild(host, PropBenchId);
			kit.localPosition = Vector3.zero;
			kit.localRotation = Quaternion.identity;
			kit.localScale = Vector3.one;
			kit.gameObject.SetActive(true);
			BuildBenchKit(kit, color);
		}

		private static string ResolveStreetLampKitId(string propName)
		{
			int serial = ParseTrailingNumber(propName);
			int variant = serial >= 0 ? serial % 2 : StableNameVariant(propName) % 2;
			return variant == 1 ? PropTrafficLightId : PropStreetLampId;
		}

		private static void BuildStreetLampKit(Transform kit)
		{
			EnsureTrafficKitPiece(kit, "Post", PrimitiveType.Cylinder, new Vector3(0f, 1.08f, 0f), new Vector3(0.10f, 1.08f, 0.10f), StreetPostColor);
			EnsureTrafficKitPiece(kit, "Collar", PrimitiveType.Cylinder, new Vector3(0f, 2.08f, 0f), new Vector3(0.16f, 0.08f, 0.16f), Color.Lerp(StreetPostColor, Color.black, 0.18f));
			EnsureTrafficKitPiece(kit, "Cap", PrimitiveType.Cube, new Vector3(0f, 2.22f, 0f), new Vector3(0.28f, 0.10f, 0.28f), Color.Lerp(StreetPostColor, Color.white, 0.12f));
			EnsureTrafficKitPiece(kit, "Glow", PrimitiveType.Sphere, new Vector3(0f, 2.10f, 0f), new Vector3(0.20f, 0.14f, 0.20f), StreetCapColor);
		}

		private static void BuildTrafficLightKit(Transform kit)
		{
			EnsureTrafficKitPiece(kit, "Post", PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f), new Vector3(0.10f, 1.05f, 0.10f), StreetPostColor);
			EnsureTrafficKitPiece(kit, "Arm", PrimitiveType.Cube, new Vector3(0f, 1.92f, 0.12f), new Vector3(0.08f, 0.08f, 0.18f), StreetHousingColor);
			EnsureTrafficKitPiece(kit, "Housing", PrimitiveType.Cube, new Vector3(0f, 1.92f, 0.24f), new Vector3(0.18f, 0.52f, 0.14f), StreetHousingColor);
			EnsureTrafficKitPiece(kit, "Signal_R", PrimitiveType.Cube, new Vector3(0f, 2.10f, 0.32f), new Vector3(0.12f, 0.10f, 0.08f), StreetSignalRed);
			EnsureTrafficKitPiece(kit, "Signal_A", PrimitiveType.Cube, new Vector3(0f, 1.94f, 0.32f), new Vector3(0.12f, 0.10f, 0.08f), StreetSignalAmber);
			EnsureTrafficKitPiece(kit, "Signal_G", PrimitiveType.Cube, new Vector3(0f, 1.78f, 0.32f), new Vector3(0.12f, 0.10f, 0.08f), StreetSignalGreen);
			EnsureTrafficKitPiece(kit, "Hood", PrimitiveType.Cube, new Vector3(0f, 2.20f, 0.24f), new Vector3(0.20f, 0.08f, 0.16f), Color.Lerp(StreetHousingColor, Color.black, 0.12f));
		}

		private static void BuildRoadsideTreeKit(Transform kit, Color leafColor)
		{
			Color canopy = Color.Lerp(leafColor, new Color(0.22f, 0.42f, 0.20f, 1f), 0.18f);
			Color canopyB = Color.Lerp(canopy, Color.black, 0.10f);
			EnsureTrafficKitPiece(kit, "Trunk", PrimitiveType.Cylinder, new Vector3(0f, 0.52f, 0f), new Vector3(0.16f, 0.52f, 0.16f), StreetTrunkColor);
			EnsureTrafficKitPiece(kit, "Canopy_C", PrimitiveType.Sphere, new Vector3(0f, 1.22f, 0f), new Vector3(0.42f, 0.36f, 0.42f), canopy);
			EnsureTrafficKitPiece(kit, "Canopy_L", PrimitiveType.Sphere, new Vector3(0.22f, 1.14f, 0.08f), new Vector3(0.30f, 0.26f, 0.28f), canopyB);
			EnsureTrafficKitPiece(kit, "Canopy_R", PrimitiveType.Sphere, new Vector3(-0.18f, 1.18f, -0.10f), new Vector3(0.28f, 0.24f, 0.26f), Color.Lerp(canopy, Color.white, 0.08f));
			EnsureTrafficKitPiece(kit, "Canopy_T", PrimitiveType.Sphere, new Vector3(0.04f, 1.42f, 0.08f), new Vector3(0.26f, 0.22f, 0.24f), canopy);
		}

		private static void BuildBenchKit(Transform kit, Color color)
		{
			Color seat = Color.Lerp(color, new Color(0.42f, 0.32f, 0.20f, 1f), 0.28f);
			Color frame = Color.Lerp(color, StreetHousingColor, 0.42f);
			EnsureTrafficKitPiece(kit, "Seat_A", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.04f), new Vector3(0.88f, 0.08f, 0.12f), seat);
			EnsureTrafficKitPiece(kit, "Seat_B", PrimitiveType.Cube, new Vector3(0f, 0.02f, -0.08f), new Vector3(0.88f, 0.08f, 0.12f), Color.Lerp(seat, Color.black, 0.08f));
			EnsureTrafficKitPiece(kit, "Back", PrimitiveType.Cube, new Vector3(0f, 0.16f, -0.16f), new Vector3(0.88f, 0.20f, 0.08f), Color.Lerp(seat, frame, 0.35f));
			EnsureTrafficKitPiece(kit, "Leg_L", PrimitiveType.Cube, new Vector3(-0.32f, -0.12f, 0f), new Vector3(0.08f, 0.20f, 0.18f), frame);
			EnsureTrafficKitPiece(kit, "Leg_R", PrimitiveType.Cube, new Vector3(0.32f, -0.12f, 0f), new Vector3(0.08f, 0.20f, 0.18f), frame);
		}

		private static void SetNamedChildRendererEnabled(Transform parent, string name, bool enabled)
		{
			Transform child = FindDirectChild(parent, name);
			if ((Object)(object)child == (Object)null)
			{
				return;
			}

			Renderer renderer = child.GetComponent<Renderer>();
			if ((Object)(object)renderer != (Object)null)
			{
				renderer.enabled = enabled;
			}
		}
	}
}
