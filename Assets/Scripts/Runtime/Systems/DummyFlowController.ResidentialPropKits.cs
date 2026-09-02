using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string PropFenceId = "PROP_Fence";
		private const string PropMailboxId = "PROP_Mailbox";
		private const string PropShedId = "PROP_Shed";

		private static readonly Color ResidentialWood = new Color(0.46f, 0.34f, 0.22f, 1f);
		private static readonly Color ResidentialPost = new Color(0.32f, 0.28f, 0.24f, 1f);
		private static readonly Color ResidentialFlag = new Color(0.86f, 0.28f, 0.18f, 1f);

		private static void ApplyFenceKit(Transform host, Color color)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "RailTop", false);
			SetNamedChildRendererEnabled(host, "RailBottom", false);
			SetNamedChildRendererEnabled(host, "Post_L", false);
			SetNamedChildRendererEnabled(host, "Post_R", false);
			ApplyNamedStreetKit(host, PropFenceId, kit => BuildFenceKit(kit, color));
		}

		private static void ApplyMailboxKit(Transform host, Color boxColor)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "Post", false);
			SetNamedChildRendererEnabled(host, "Flag", false);
			ApplyNamedStreetKit(host, PropMailboxId, kit => BuildMailboxKit(kit, boxColor));
		}

		private static void ApplyShedKit(Transform host, Color color)
		{
			if ((Object)(object)host == (Object)null)
			{
				return;
			}

			SetHostPrimaryRendererEnabled(host, enabled: false);
			SetNamedChildRendererEnabled(host, "Roof", false);
			SetNamedChildRendererEnabled(host, "Door", false);
			ApplyNamedStreetKit(host, PropShedId, kit => BuildShedKit(kit, color));
		}

		private static void BuildFenceKit(Transform kit, Color color)
		{
			Vector3 size = AbsLossyScale(kit);
			float width = Mathf.Clamp(size.x, 0.72f, 1.05f);
			float height = Mathf.Clamp(size.y, 0.32f, 0.48f);
			Color rail = Color.Lerp(color, ResidentialWood, 0.35f);
			Color post = Color.Lerp(color, ResidentialPost, 0.42f);

			EnsureStreetKitWorld(kit, "Post_L", PrimitiveType.Cube, new Vector3(-width * 0.42f, height * 0.02f, 0f), new Vector3(0.08f, height * 0.92f, 0.08f), post);
			EnsureStreetKitWorld(kit, "Post_R", PrimitiveType.Cube, new Vector3(width * 0.42f, height * 0.02f, 0f), new Vector3(0.08f, height * 0.92f, 0.08f), post);
			EnsureStreetKitWorld(kit, "Post_C", PrimitiveType.Cube, new Vector3(0f, height * 0.02f, 0f), new Vector3(0.08f, height * 0.86f, 0.08f), post);
			EnsureStreetKitWorld(kit, "Rail_Top", PrimitiveType.Cube, new Vector3(0f, height * 0.28f, 0f), new Vector3(width * 0.94f, 0.08f, 0.08f), rail);
			EnsureStreetKitWorld(kit, "Rail_Mid", PrimitiveType.Cube, new Vector3(0f, -height * 0.04f, 0f), new Vector3(width * 0.94f, 0.08f, 0.08f), Color.Lerp(rail, Color.black, 0.12f));
		}

		private static void BuildMailboxKit(Transform kit, Color boxColor)
		{
			Color post = ResidentialPost;
			Color box = Color.Lerp(boxColor, new Color(0.22f, 0.34f, 0.48f, 1f), 0.28f);

			EnsureStreetKitWorld(kit, "Post", PrimitiveType.Cylinder, new Vector3(0f, -0.04f, 0f), new Vector3(0.08f, 0.18f, 0.08f), post);
			EnsureStreetKitWorld(kit, "Box", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0f), new Vector3(0.22f, 0.14f, 0.16f), box);
			EnsureStreetKitWorld(kit, "Lid", PrimitiveType.Cube, new Vector3(0f, 0.20f, 0f), new Vector3(0.24f, 0.08f, 0.18f), Color.Lerp(box, Color.black, 0.16f));
			EnsureStreetKitWorld(kit, "Slot", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0.08f), new Vector3(0.14f, 0.08f, 0.08f), Color.Lerp(box, Color.black, 0.28f));
			EnsureStreetKitWorld(kit, "Flag", PrimitiveType.Cube, new Vector3(0.14f, 0.16f, 0f), new Vector3(0.08f, 0.12f, 0.08f), ResidentialFlag);
		}

		private static void BuildShedKit(Transform kit, Color color)
		{
			Vector3 size = AbsLossyScale(kit);
			float width = Mathf.Clamp(size.x, 0.62f, 1.18f);
			float height = Mathf.Clamp(size.y, 0.48f, 0.95f);
			float depth = Mathf.Clamp(size.z, 0.62f, 1.22f);
			Color wall = Color.Lerp(color, ResidentialWood, 0.22f);
			Color roof = Color.Lerp(color, new Color(0.28f, 0.24f, 0.22f, 1f), 0.48f);
			Color door = Color.Lerp(wall, Color.black, 0.22f);

			EnsureStreetKitWorld(kit, "Walls", PrimitiveType.Cube, new Vector3(0f, -height * 0.04f, 0f), new Vector3(width * 0.88f, height * 0.72f, depth * 0.82f), wall);
			EnsureStreetKitWorld(kit, "Roof", PrimitiveType.Cube, new Vector3(0f, height * 0.38f, 0f), new Vector3(width * 1.02f, 0.12f, depth * 0.98f), roof);
			EnsureStreetKitWorld(kit, "Ridge", PrimitiveType.Cube, new Vector3(0f, height * 0.48f, 0f), new Vector3(width * 0.18f, 0.10f, depth * 1.02f), Color.Lerp(roof, Color.black, 0.16f));
			EnsureStreetKitWorld(kit, "Door", PrimitiveType.Cube, new Vector3(0f, -height * 0.10f, depth * 0.42f), new Vector3(width * 0.28f, height * 0.48f, 0.08f), door);
			EnsureStreetKitWorld(kit, "Window", PrimitiveType.Cube, new Vector3(width * 0.22f, height * 0.08f, depth * 0.42f), new Vector3(0.12f, 0.12f, 0.08f), new Color(0.42f, 0.56f, 0.58f, 1f));
		}
	}
}
