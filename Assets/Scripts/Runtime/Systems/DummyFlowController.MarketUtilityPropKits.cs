using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string PropKioskId = "PROP_Kiosk";
		private const string PropVendingId = "PROP_Vending";
		private const string PropBusStopId = "PROP_BusStop";
		private const string PropTransformerId = "PROP_Transformer";
		private const string PropExplosiveBarrelId = "PROP_ExplosiveBarrel";

		private static readonly Color UtilityHazardYellow = new Color(0.92f, 0.78f, 0.18f, 1f);
		private static readonly Color UtilityHazardBlack = new Color(0.12f, 0.12f, 0.13f, 1f);
		private static readonly Color BarrelBandOrange = new Color(0.92f, 0.38f, 0.10f, 1f);
		private static readonly Color MarketGlassColor = new Color(0.20f, 0.36f, 0.40f, 1f);

		private static void ApplyKioskKit(Transform host, Color color)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "ShopRoof", false);
			SetNamedChildRendererEnabled(host, "ShopCounter", false);
			SetNamedChildRendererEnabled(host, "ShopStripe", false);
			ApplyNamedStreetKit(host, PropKioskId, kit => BuildKioskKit(kit, color));
		}

		private static void ApplyVendingKit(Transform host, Color color)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "VendFace", false);
			SetNamedChildRendererEnabled(host, "VendSlot", false);
			SetNamedChildRendererEnabled(host, "VendCap", false);
			ApplyNamedStreetKit(host, PropVendingId, kit => BuildVendingKit(kit, color));
		}

		private static void ApplyBusStopKit(Transform host, Color color)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "StopRoof", false);
			SetNamedChildRendererEnabled(host, "StopPanel", false);
			SetNamedChildRendererEnabled(host, "StopPole_L", false);
			SetNamedChildRendererEnabled(host, "StopPole_R", false);
			ApplyNamedStreetKit(host, PropBusStopId, kit => BuildBusStopKit(kit, color));
		}

		private static void ApplyTransformerKit(Transform host, Color baseColor)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetNamedChildRendererEnabled(host, "Base", false);
			SetNamedChildRendererEnabled(host, "Top", false);
			SetNamedChildRendererEnabled(host, "Pole_L", false);
			SetNamedChildRendererEnabled(host, "Pole_R", false);
			SetNamedChildRendererEnabled(host, "Core", false);
			ApplyNamedStreetKit(host, PropTransformerId, kit => BuildTransformerKit(kit, baseColor));
		}

		private static void ApplyExplosiveBarrelKit(Transform host, Color bodyColor)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetNamedChildRendererEnabled(host, "Body", false);
			SetNamedChildRendererEnabled(host, "Band_A", false);
			SetNamedChildRendererEnabled(host, "Band_B", false);
			SetNamedChildRendererEnabled(host, "Core", false);
			ApplyNamedStreetKit(host, PropExplosiveBarrelId, kit => BuildExplosiveBarrelKit(kit, bodyColor));
		}

		private static void ApplyNamedStreetKit(Transform host, string kitId, System.Action<Transform> build)
		{
			SetNamedChildActive(host, kitId, true);
			Transform kit = GetOrCreateDirectChild(host, kitId);
			kit.localPosition = Vector3.zero;
			kit.localRotation = Quaternion.identity;
			kit.localScale = Vector3.one;
			kit.gameObject.SetActive(true);
			build(kit);
		}

		private static void BuildKioskKit(Transform kit, Color color)
		{
			Vector3 size = AbsLossyScale(kit);
			float width = Mathf.Clamp(size.x, 0.55f, 1.15f);
			float height = Mathf.Clamp(size.y, 0.55f, 1.05f);
			float depth = Mathf.Clamp(size.z, 0.48f, 0.95f);
			Color stall = Color.Lerp(color, new Color(0.72f, 0.42f, 0.28f, 1f), 0.22f);
			Color awning = Color.Lerp(color, new Color(0.18f, 0.46f, 0.42f, 1f), 0.38f);
			Color counter = Color.Lerp(stall, Color.white, 0.18f);

			EnsureStreetKitWorld(kit, "Stall", PrimitiveType.Cube, new Vector3(0f, height * 0.02f, 0f), new Vector3(width * 0.92f, height * 0.78f, depth * 0.82f), stall);
			EnsureStreetKitWorld(kit, "Awning", PrimitiveType.Cube, new Vector3(0f, height * 0.46f, depth * 0.06f), new Vector3(width * 1.12f, 0.10f, depth * 1.08f), awning);
			EnsureStreetKitWorld(kit, "Counter", PrimitiveType.Cube, new Vector3(0f, -height * 0.12f, depth * 0.48f), new Vector3(width * 0.78f, 0.10f, 0.12f), counter);
			EnsureStreetKitWorld(kit, "Window", PrimitiveType.Cube, new Vector3(0f, height * 0.10f, depth * 0.42f), new Vector3(width * 0.42f, height * 0.28f, 0.08f), MarketGlassColor);
			EnsureStreetKitWorld(kit, "Sign", PrimitiveType.Cube, new Vector3(0f, height * 0.28f, depth * 0.44f), new Vector3(width * 0.56f, 0.10f, 0.08f), Color.Lerp(awning, Color.white, 0.22f));
		}

		private static void BuildVendingKit(Transform kit, Color color)
		{
			Vector3 size = AbsLossyScale(kit);
			float width = Mathf.Clamp(size.x, 0.48f, 0.78f);
			float height = Mathf.Clamp(size.y, 0.82f, 1.15f);
			float depth = Mathf.Clamp(size.z, 0.42f, 0.68f);
			Color cabinet = Color.Lerp(color, new Color(0.22f, 0.28f, 0.34f, 1f), 0.42f);
			Color face = Color.Lerp(color, new Color(0.55f, 0.86f, 0.78f, 1f), 0.35f);

			EnsureStreetKitWorld(kit, "Cabinet", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(width * 0.92f, height * 0.92f, depth * 0.88f), cabinet);
			EnsureStreetKitWorld(kit, "Face", PrimitiveType.Cube, new Vector3(0f, height * 0.04f, depth * 0.42f), new Vector3(width * 0.68f, height * 0.58f, 0.08f), face);
			EnsureStreetKitWorld(kit, "Glass", PrimitiveType.Cube, new Vector3(0f, height * 0.10f, depth * 0.46f), new Vector3(width * 0.52f, height * 0.38f, 0.08f), MarketGlassColor);
			EnsureStreetKitWorld(kit, "Buttons", PrimitiveType.Cube, new Vector3(width * 0.22f, -height * 0.08f, depth * 0.46f), new Vector3(0.10f, height * 0.22f, 0.08f), UtilityHazardYellow);
			EnsureStreetKitWorld(kit, "Slot", PrimitiveType.Cube, new Vector3(0f, -height * 0.32f, depth * 0.46f), new Vector3(width * 0.36f, 0.08f, 0.08f), UtilityHazardBlack);
			EnsureStreetKitWorld(kit, "Cap", PrimitiveType.Cube, new Vector3(0f, height * 0.46f, 0f), new Vector3(width * 1.02f, 0.08f, depth * 0.96f), Color.Lerp(cabinet, Color.black, 0.16f));
		}

		private static void BuildBusStopKit(Transform kit, Color color)
		{
			Vector3 size = AbsLossyScale(kit);
			float width = Mathf.Clamp(size.x, 0.85f, 1.25f);
			float height = Mathf.Clamp(size.y, 0.95f, 1.35f);
			float depth = Mathf.Clamp(size.z, 0.28f, 0.42f);
			Color post = Color.Lerp(color, StreetPostColor, 0.45f);
			Color panel = Color.Lerp(color, new Color(0.78f, 0.82f, 0.74f, 1f), 0.28f);

			EnsureStreetKitWorld(kit, "Post_L", PrimitiveType.Cylinder, new Vector3(-width * 0.36f, 0f, 0f), new Vector3(0.10f, height * 0.48f, 0.10f), post);
			EnsureStreetKitWorld(kit, "Post_R", PrimitiveType.Cylinder, new Vector3(width * 0.36f, 0f, 0f), new Vector3(0.10f, height * 0.48f, 0.10f), post);
			EnsureStreetKitWorld(kit, "Roof", PrimitiveType.Cube, new Vector3(0f, height * 0.46f, 0.04f), new Vector3(width * 1.02f, 0.08f, depth * 1.15f), Color.Lerp(post, Color.black, 0.12f));
			EnsureStreetKitWorld(kit, "Panel", PrimitiveType.Cube, new Vector3(0f, height * 0.04f, -depth * 0.18f), new Vector3(width * 0.62f, height * 0.52f, 0.08f), panel);
			EnsureStreetKitWorld(kit, "RouteSign", PrimitiveType.Cube, new Vector3(0f, height * 0.18f, -depth * 0.28f), new Vector3(width * 0.28f, 0.16f, 0.08f), Color.Lerp(color, new Color(0.16f, 0.42f, 0.48f, 1f), 0.4f));
		}

		private static void BuildTransformerKit(Transform kit, Color baseColor)
		{
			Color tank = Color.Lerp(baseColor, new Color(0.28f, 0.32f, 0.24f, 1f), 0.38f);
			Color pipe = Color.Lerp(StreetPostColor, UtilityHazardYellow, 0.18f);

			EnsureStreetKitWorld(kit, "Tank", PrimitiveType.Cube, new Vector3(0f, 0.32f, 0f), new Vector3(0.98f, 0.64f, 0.68f), tank);
			EnsureStreetKitWorld(kit, "Lid", PrimitiveType.Cube, new Vector3(0f, 0.68f, 0f), new Vector3(0.78f, 0.12f, 0.50f), Color.Lerp(tank, Color.black, 0.16f));
			EnsureStreetKitWorld(kit, "Stripe", PrimitiveType.Cube, new Vector3(0f, 0.32f, 0.34f), new Vector3(0.88f, 0.12f, 0.08f), UtilityHazardYellow);
			EnsureStreetKitWorld(kit, "StripeDark", PrimitiveType.Cube, new Vector3(0f, 0.20f, 0.34f), new Vector3(0.88f, 0.08f, 0.08f), UtilityHazardBlack);
			EnsureStreetKitWorld(kit, "Bushing_L", PrimitiveType.Cylinder, new Vector3(-0.22f, 0.88f, 0f), new Vector3(0.12f, 0.18f, 0.12f), pipe);
			EnsureStreetKitWorld(kit, "Bushing_R", PrimitiveType.Cylinder, new Vector3(0.22f, 0.88f, 0f), new Vector3(0.12f, 0.18f, 0.12f), pipe);
			EnsureStreetKitWorld(kit, "Cap_L", PrimitiveType.Sphere, new Vector3(-0.22f, 1.04f, 0f), new Vector3(0.16f, 0.12f, 0.16f), UtilityHazardYellow);
			EnsureStreetKitWorld(kit, "Cap_R", PrimitiveType.Sphere, new Vector3(0.22f, 1.04f, 0f), new Vector3(0.16f, 0.12f, 0.16f), UtilityHazardYellow);
		}

		private static void BuildExplosiveBarrelKit(Transform kit, Color bodyColor)
		{
			Color drum = Color.Lerp(bodyColor, BarrelBandOrange, 0.22f);
			Color hoop = Color.Lerp(drum, UtilityHazardBlack, 0.55f);

			EnsureStreetKitWorld(kit, "Drum", PrimitiveType.Cylinder, new Vector3(0f, 0.40f, 0f), new Vector3(0.52f, 0.40f, 0.52f), drum);
			EnsureStreetKitWorld(kit, "Lid", PrimitiveType.Cylinder, new Vector3(0f, 0.80f, 0f), new Vector3(0.54f, 0.08f, 0.54f), hoop);
			EnsureStreetKitWorld(kit, "BaseRim", PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0f), new Vector3(0.54f, 0.08f, 0.54f), hoop);
			EnsureStreetKitWorld(kit, "Band", PrimitiveType.Cylinder, new Vector3(0f, 0.42f, 0f), new Vector3(0.56f, 0.10f, 0.56f), BarrelBandOrange);
			EnsureStreetKitWorld(kit, "BandEdge", PrimitiveType.Cylinder, new Vector3(0f, 0.52f, 0f), new Vector3(0.56f, 0.08f, 0.56f), UtilityHazardBlack);
			EnsureStreetKitWorld(kit, "Bung", PrimitiveType.Cube, new Vector3(0.16f, 0.82f, 0f), new Vector3(0.10f, 0.08f, 0.10f), UtilityHazardYellow);
		}

		private static void EnsureStreetKitWorld(Transform kit, string name, PrimitiveType type, Vector3 worldOffset, Vector3 worldScale, Color color, Vector3? localEuler = null)
		{
			EnsureTrafficKitPiece(kit, name, type, WorldToLocalOffset(kit, worldOffset), WorldToLocalScale(kit, worldScale), color, localEuler);
		}
	}
}
