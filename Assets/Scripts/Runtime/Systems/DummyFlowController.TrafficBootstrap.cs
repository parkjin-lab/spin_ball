using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void RegisterExistingTrafficVehicles(Transform streetProps)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)streetProps == (Object)null)
			{
				return;
			}
			for (int i = 0; i < streetProps.childCount; i++)
			{
				Transform child = streetProps.GetChild(i);
				if (!((Object)(object)child == (Object)null) && ((Object)child).name.StartsWith("Car_"))
				{
					TrafficSpawnPoint trafficSpawnPoint = default(TrafficSpawnPoint);
					trafficSpawnPoint.Position = child.position;
					trafficSpawnPoint.Yaw = child.eulerAngles.y;
					trafficSpawnPoint.BodyColor = SampleTrafficBodyColor(child);
					TrafficSpawnPoint spawnPoint = trafficSpawnPoint;
					RegisterTrafficVehicle(child, spawnPoint);
				}
			}
		}

		private void RegisterTrafficVehicle(Transform root, TrafficSpawnPoint spawnPoint)
		{
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)root == (Object)null)
			{
				return;
			}
			for (int i = 0; i < trafficVehicles.Count; i++)
			{
				if (trafficVehicles[i] != null && (Object)(object)trafficVehicles[i].Root == (Object)(object)root)
				{
					return;
				}
			}
			Rigidbody val = ((Component)root).GetComponent<Rigidbody>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)root).gameObject.AddComponent<Rigidbody>();
			}
			val.isKinematic = true;
			val.useGravity = false;
			val.linearDamping = 0f;
			val.angularDamping = 0f;
			val.interpolation = (RigidbodyInterpolation)1;
			val.constraints = (RigidbodyConstraints)116;
			BoxCollider val2 = ((Component)root).GetComponent<BoxCollider>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)root).gameObject.AddComponent<BoxCollider>();
			}
			val2.center = new Vector3(0f, 0.34f, 0f);
			val2.size = new Vector3(1.28f, 0.72f, 2.14f);
			((Collider)val2).isTrigger = false;
			EnsureStreetPropReactiveRuntime(((Component)root).gameObject, root, DummyStreetPropReactive.PropKind.Vehicle);
			DummyStreetPropReactive[] componentsInChildren = ((Component)root).GetComponentsInChildren<DummyStreetPropReactive>(true);
			foreach (DummyStreetPropReactive dummyStreetPropReactive in componentsInChildren)
			{
				if (!((Object)(object)dummyStreetPropReactive == (Object)null) && !((Object)(object)((Component)dummyStreetPropReactive).gameObject == (Object)(object)((Component)root).gameObject) && dummyStreetPropReactive.Kind == DummyStreetPropReactive.PropKind.Vehicle)
				{
					if (Application.isPlaying)
					{
						Object.Destroy((Object)(object)dummyStreetPropReactive);
					}
					else
					{
						Object.DestroyImmediate((Object)(object)dummyStreetPropReactive);
					}
				}
			}
			Vector3 val3 = root.forward;
			val3.y = 0f;
			if (val3.sqrMagnitude < 0.001f)
			{
				val3 = Quaternion.Euler(0f, spawnPoint.Yaw, 0f) * Vector3.forward;
			}
			if (val3.sqrMagnitude < 0.001f)
			{
				val3 = Vector3.forward;
			}
			val3.Normalize();
			bool flag = Mathf.Abs(val3.x) >= Mathf.Abs(val3.z);
			int directionSign = ((!flag) ? ((val3.z >= 0f) ? 1 : (-1)) : ((val3.x >= 0f) ? 1 : (-1)));
			float randomTrafficSpeed = GetRandomTrafficSpeed();
			float num = (flag ? root.position.z : root.position.x);
			num += GetRandomTrafficLaneOffset();
			trafficVehicles.Add(new TrafficVehicleState
			{
				Root = root,
				Body = val,
				CruiseSpeed = randomTrafficSpeed,
				CurrentSpeed = randomTrafficSpeed,
				AlongX = flag,
				DirectionSign = directionSign,
				LaneCoord = num,
				StopTimer = 0f,
				IntersectionCooldown = Mathf.Lerp(0.12f, 0.55f, (float)trafficRng.NextDouble())
			});
		}

		private void ResolveTrafficBounds(Transform mapRoot)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			trafficMapCenter = (((Object)(object)mapRoot != (Object)null) ? mapRoot.position : Vector3.zero);
			trafficMapHalfExtents = new Vector2(25f, 25f);
			if ((Object)(object)mapRoot == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(mapRoot, "Ground");
			if ((Object)(object)val != (Object)null)
			{
				Vector3 lossyScale = val.lossyScale;
				trafficMapCenter = val.position;
				trafficMapHalfExtents = new Vector2(Mathf.Max(12f, Mathf.Abs(lossyScale.x) * 0.5f), Mathf.Max(12f, Mathf.Abs(lossyScale.z) * 0.5f));
				return;
			}
			Renderer[] componentsInChildren = ((Component)mapRoot).GetComponentsInChildren<Renderer>(true);
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				return;
			}
			Bounds bounds = componentsInChildren[0].bounds;
			for (int i = 1; i < componentsInChildren.Length; i++)
			{
				Renderer val2 = componentsInChildren[i];
				if (!((Object)(object)val2 == (Object)null))
				{
					bounds.Encapsulate(val2.bounds);
				}
			}
			trafficMapCenter = bounds.center;
			trafficMapHalfExtents = new Vector2(Mathf.Max(12f, bounds.extents.x), Mathf.Max(12f, bounds.extents.z));
		}

		private static Transform EnsureTrafficVehicleRuntime(Transform parent, string name, Vector3 localPosition, float yaw, Color bodyColor)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			Transform orCreateDirectChild = GetOrCreateDirectChild(parent, name);
			orCreateDirectChild.localPosition = localPosition;
			orCreateDirectChild.localRotation = Quaternion.Euler(0f, yaw, 0f);
			orCreateDirectChild.localScale = Vector3.one;
			((Component)orCreateDirectChild).gameObject.SetActive(true);
			GameObject val = EnsurePrimitive(orCreateDirectChild, "Body", (PrimitiveType)3, new Vector3(0f, 0.24f, 0f), new Vector3(1.15f, 0.35f, 2.05f), bodyColor);
			EnsurePrimitive(orCreateDirectChild, "Cabin", (PrimitiveType)3, new Vector3(0f, 0.5f, -0.12f), new Vector3(0.82f, 0.28f, 0.95f), Color.Lerp(bodyColor, Color.white, 0.18f));
			Color color = default(Color);
			color = new Color(0.08f, 0.08f, 0.1f, 1f);
			EnsurePrimitive(orCreateDirectChild, "Wheel_FL", (PrimitiveType)2, new Vector3(0.48f, 0.12f, 0.62f), new Vector3(0.2f, 0.12f, 0.2f), color);
			EnsurePrimitive(orCreateDirectChild, "Wheel_FR", (PrimitiveType)2, new Vector3(-0.48f, 0.12f, 0.62f), new Vector3(0.2f, 0.12f, 0.2f), color);
			EnsurePrimitive(orCreateDirectChild, "Wheel_RL", (PrimitiveType)2, new Vector3(0.48f, 0.12f, -0.62f), new Vector3(0.2f, 0.12f, 0.2f), color);
			EnsurePrimitive(orCreateDirectChild, "Wheel_RR", (PrimitiveType)2, new Vector3(-0.48f, 0.12f, -0.62f), new Vector3(0.2f, 0.12f, 0.2f), color);
			BoxCollider val2 = ((Component)orCreateDirectChild).GetComponent<BoxCollider>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)orCreateDirectChild).gameObject.AddComponent<BoxCollider>();
			}
			val2.center = new Vector3(0f, 0.34f, 0f);
			val2.size = new Vector3(1.28f, 0.72f, 2.14f);
			((Collider)val2).isTrigger = false;
			DummyStreetPropReactive dummyStreetPropReactive = (((Object)(object)val != (Object)null) ? val.GetComponent<DummyStreetPropReactive>() : null);
			if ((Object)(object)dummyStreetPropReactive != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)dummyStreetPropReactive);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)dummyStreetPropReactive);
				}
			}
			EnsureStreetPropReactiveRuntime(((Component)orCreateDirectChild).gameObject, orCreateDirectChild, DummyStreetPropReactive.PropKind.Vehicle);
			return orCreateDirectChild;
		}
	}
}
