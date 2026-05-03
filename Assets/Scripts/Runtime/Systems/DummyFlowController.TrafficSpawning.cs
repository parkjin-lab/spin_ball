using System.Collections.Generic;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void CollectTrafficSpawnPoints(Transform streetProps)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
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
					TrafficSpawnPoint point = trafficSpawnPoint;
					AddTrafficSpawnPoint(point);
					int num = ParseTrailingNumber(((Object)child).name);
					if (num >= trafficSpawnSerial)
					{
						trafficSpawnSerial = num + 1;
					}
				}
			}
		}

		private void EnsureTrafficVehiclePopulation(bool fillImmediately)
		{
			int num = GetRuntimeTrafficDesiredCars();
			if (num <= 0 || (Object)(object)trafficStreetPropsRoot == (Object)null || trafficSpawnPoints.Count <= 0)
			{
				return;
			}
			int num2 = num - trafficVehicles.Count;
			if (num2 <= 0)
			{
				return;
			}
			int num3 = ((!fillImmediately) ? 1 : num2);
			for (int i = 0; i < num3; i++)
			{
				if (!TrySpawnTrafficVehicle())
				{
					break;
				}
			}
		}

		private bool TrySpawnTrafficVehicle()
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)trafficStreetPropsRoot == (Object)null || trafficSpawnPoints.Count <= 0)
			{
				return false;
			}
			int num = Mathf.Clamp(trafficSpawnPoints.Count * 2, 8, 28);
			for (int i = 0; i < num; i++)
			{
				TrafficSpawnPoint trafficSpawnPoint = trafficSpawnPoints[trafficRng.Next(trafficSpawnPoints.Count)];
				Vector3 val = Quaternion.Euler(0f, trafficSpawnPoint.Yaw, 0f) * Vector3.forward;
				val.y = 0f;
				if (val.sqrMagnitude < 0.001f)
				{
					val = Vector3.forward;
				}
				val.Normalize();
				bool num2 = Mathf.Abs(val.x) >= Mathf.Abs(val.z);
				Vector3 position = trafficSpawnPoint.Position;
				float randomTrafficLaneOffset = GetRandomTrafficLaneOffset();
				if (num2)
				{
					position.z += randomTrafficLaneOffset;
				}
				else
				{
					position.x += randomTrafficLaneOffset;
				}
				if (!IsTrafficSpawnOccupied(position, 2.2f) && !IsTrafficSpawnBlockedByObstacle(position, trafficSpawnPoint.Yaw))
				{
					Vector3 localPosition = trafficStreetPropsRoot.InverseTransformPoint(position);
					Color bodyColor = ((trafficSpawnPoint.BodyColor.a > 0.001f) ? trafficSpawnPoint.BodyColor : GetRandomTrafficBodyColor());
					string name = $"Car_RT_{trafficSpawnSerial:000}";
					trafficSpawnSerial++;
					Transform val2 = EnsureTrafficVehicleRuntime(trafficStreetPropsRoot, name, localPosition, trafficSpawnPoint.Yaw, bodyColor);
					if (!((Object)(object)val2 == (Object)null))
					{
						TrafficSpawnPoint trafficSpawnPoint2 = default(TrafficSpawnPoint);
						trafficSpawnPoint2.Position = val2.position;
						trafficSpawnPoint2.Yaw = trafficSpawnPoint.Yaw;
						trafficSpawnPoint2.BodyColor = bodyColor;
						TrafficSpawnPoint spawnPoint = trafficSpawnPoint2;
						RegisterTrafficVehicle(val2, spawnPoint);
						return true;
					}
				}
			}
			return false;
		}

		private bool IsTrafficSpawnOccupied(Vector3 worldPosition, float radius)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			float num = radius * radius;
			for (int i = 0; i < trafficVehicles.Count; i++)
			{
				TrafficVehicleState trafficVehicleState = trafficVehicles[i];
				if (trafficVehicleState != null && !((Object)(object)trafficVehicleState.Root == (Object)null) && ((Component)trafficVehicleState.Root).gameObject.activeInHierarchy)
				{
					Vector3 position = trafficVehicleState.Root.position;
					float num2 = position.x - worldPosition.x;
					float num3 = position.z - worldPosition.z;
					if (num2 * num2 + num3 * num3 <= num)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsTrafficSpawnBlockedByObstacle(Vector3 worldPosition, float yaw)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = default(Vector3);
			val = new Vector3(Mathf.Max(0.25f, trafficObstacleProbeHalfExtents.x), Mathf.Max(0.16f, trafficObstacleProbeHalfExtents.y), Mathf.Max(0.35f, trafficObstacleProbeHalfExtents.z));
			float num = Mathf.Max(0.42f, val.y + 0.18f);
			Vector3 val2 = new Vector3(worldPosition.x, num, worldPosition.z);
			Quaternion val3 = Quaternion.Euler(0f, yaw, 0f);
			int num2 = Physics.OverlapBoxNonAlloc(val2, val, trafficSpawnObstacleHits, val3, -1, (QueryTriggerInteraction)1);
			if (num2 <= 0)
			{
				return false;
			}
			for (int i = 0; i < num2; i++)
			{
				Collider val4 = trafficSpawnObstacleHits[i];
				if ((Object)(object)val4 == (Object)null || !val4.enabled || val4.isTrigger || ((Object)val4).name == "Ground")
				{
					continue;
				}
				DummyStreetPropReactive dummyStreetPropReactive = ((Component)val4).GetComponent<DummyStreetPropReactive>() ?? ((Component)val4).GetComponentInParent<DummyStreetPropReactive>();
				if (!((Object)(object)dummyStreetPropReactive != (Object)null) || dummyStreetPropReactive.Kind != 0)
				{
					if ((Object)(object)((Component)val4).GetComponentInParent<DummyDestructibleBlock>() != (Object)null)
					{
						return true;
					}
					if ((Object)(object)dummyStreetPropReactive != (Object)null)
					{
						return true;
					}
					Rigidbody attachedRigidbody = val4.attachedRigidbody;
					if ((Object)(object)attachedRigidbody == (Object)null || attachedRigidbody.isKinematic)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void PopulateDefaultTrafficSpawnPoints()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = trafficMapCenter;
			float num = Mathf.Max(10f, trafficMapHalfExtents.x - 4f);
			float num2 = Mathf.Max(10f, trafficMapHalfExtents.y - 4f);
			float num3 = Mathf.Min(6.5f, num2 * 0.45f);
			float num4 = Mathf.Min(6.5f, num * 0.45f);
			TrafficSpawnPoint point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x - num, 0f, val.z - num3),
				Yaw = 90f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x + num, 0f, val.z - num3),
				Yaw = 270f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x - num, 0f, val.z + num3),
				Yaw = 90f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x + num, 0f, val.z + num3),
				Yaw = 270f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x - num4, 0f, val.z - num2),
				Yaw = 0f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x - num4, 0f, val.z + num2),
				Yaw = 180f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x + num4, 0f, val.z - num2),
				Yaw = 0f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x + num4, 0f, val.z + num2),
				Yaw = 180f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			float num5 = Mathf.Min(3.4f, num2 * 0.22f);
			float num6 = Mathf.Min(3.4f, num * 0.22f);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x - num, 0f, val.z - num5),
				Yaw = 90f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x + num, 0f, val.z + num5),
				Yaw = 270f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x - num6, 0f, val.z - num2),
				Yaw = 0f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
			point = new TrafficSpawnPoint
			{
				Position = new Vector3(val.x + num6, 0f, val.z + num2),
				Yaw = 180f,
				BodyColor = GetRandomTrafficBodyColor()
			};
			AddTrafficSpawnPoint(point);
		}

		private void AddTrafficSpawnPoint(TrafficSpawnPoint point)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < trafficSpawnPoints.Count; i++)
			{
				TrafficSpawnPoint trafficSpawnPoint = trafficSpawnPoints[i];
				float num = trafficSpawnPoint.Position.x - point.Position.x;
				float num2 = trafficSpawnPoint.Position.z - point.Position.z;
				if (num * num + num2 * num2 <= 0.25f)
				{
					return;
				}
			}
			trafficSpawnPoints.Add(point);
		}

		private Color SampleTrafficBodyColor(Transform carRoot)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)carRoot == (Object)null)
			{
				return GetRandomTrafficBodyColor();
			}
			Transform val = FindDirectChild(carRoot, "Body");
			Renderer val2 = (((Object)(object)val != (Object)null) ? ((Component)val).GetComponent<Renderer>() : ((Component)carRoot).GetComponentInChildren<Renderer>(true));
			if ((Object)(object)val2 != (Object)null)
			{
				MaterialPropertyBlock val3 = new MaterialPropertyBlock();
				val2.GetPropertyBlock(val3);
				Color color = val3.GetColor("_BaseColor");
				if (color.a > 0.001f || color.maxColorComponent > 0.001f)
				{
					return color;
				}
				color = val3.GetColor("_Color");
				if (color.a > 0.001f || color.maxColorComponent > 0.001f)
				{
					return color;
				}
				Material sharedMaterial = val2.sharedMaterial;
				if ((Object)(object)sharedMaterial != (Object)null)
				{
					if (sharedMaterial.HasProperty("_BaseColor"))
					{
						return sharedMaterial.GetColor("_BaseColor");
					}
					if (sharedMaterial.HasProperty("_Color"))
					{
						return sharedMaterial.GetColor("_Color");
					}
				}
			}
			return GetRandomTrafficBodyColor();
		}

		private float GetRandomTrafficSpeed()
		{
			Vector2 runtimeTrafficSpeedRange = GetRuntimeTrafficSpeedRange();
			float num = Mathf.Max(0.6f, Mathf.Min(runtimeTrafficSpeedRange.x, runtimeTrafficSpeedRange.y));
			float num2 = Mathf.Max(num + 0.1f, Mathf.Max(runtimeTrafficSpeedRange.x, runtimeTrafficSpeedRange.y));
			return Mathf.Lerp(num, num2, (float)trafficRng.NextDouble());
		}

		private float GetRandomTrafficLaneOffset()
		{
			float num = Mathf.Max(0f, trafficLaneJitter);
			if (num <= 0.0001f)
			{
				return 0f;
			}
			return Mathf.Lerp(0f - num, num, (float)trafficRng.NextDouble());
		}

		private Color GetRandomTrafficBodyColor()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			Color val = new Color(0.86f, 0.24f, 0.2f, 1f);
			Color val2 = default(Color);
			val2 = new Color(0.18f, 0.62f, 0.92f, 1f);
			return Color.Lerp(val, val2, (float)trafficRng.NextDouble());
		}

		private void RebuildTrafficIntersections()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			trafficIntersections.Clear();
			if (trafficSpawnPoints.Count <= 0)
			{
				return;
			}
			List<float> list = new List<float>(16);
			List<float> list2 = new List<float>(16);
			for (int i = 0; i < trafficSpawnPoints.Count; i++)
			{
				TrafficSpawnPoint trafficSpawnPoint = trafficSpawnPoints[i];
				Vector3 val = Quaternion.Euler(0f, trafficSpawnPoint.Yaw, 0f) * Vector3.forward;
				val.y = 0f;
				if (!(val.sqrMagnitude < 0.001f))
				{
					if (Mathf.Abs(val.x) >= Mathf.Abs(val.z))
					{
						AddUniqueLaneCoordinate(list2, trafficSpawnPoint.Position.z, 0.62f);
					}
					else
					{
						AddUniqueLaneCoordinate(list, trafficSpawnPoint.Position.x, 0.62f);
					}
				}
			}
			float num = trafficMapCenter.x - trafficMapHalfExtents.x - 1.5f;
			float num2 = trafficMapCenter.x + trafficMapHalfExtents.x + 1.5f;
			float num3 = trafficMapCenter.z - trafficMapHalfExtents.y - 1.5f;
			float num4 = trafficMapCenter.z + trafficMapHalfExtents.y + 1.5f;
			for (int j = 0; j < list.Count; j++)
			{
				float num5 = list[j];
				if (num5 < num || num5 > num2)
				{
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					float num6 = list2[k];
					if (!(num6 < num3) && !(num6 > num4))
					{
						trafficIntersections.Add(new Vector2(num5, num6));
					}
				}
			}
		}

		private static void AddUniqueLaneCoordinate(List<float> lanes, float value, float tolerance)
		{
			for (int i = 0; i < lanes.Count; i++)
			{
				if (Mathf.Abs(lanes[i] - value) <= tolerance)
				{
					return;
				}
			}
			lanes.Add(value);
		}

		private static int ParseTrailingNumber(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return -1;
			}
			int num = value.Length - 1;
			while (num >= 0 && char.IsDigit(value[num]))
			{
				num--;
			}
			if (num >= value.Length - 1)
			{
				return -1;
			}
			if (!int.TryParse(value.Substring(num + 1), out var result))
			{
				return -1;
			}
			return result;
		}
	}
}
