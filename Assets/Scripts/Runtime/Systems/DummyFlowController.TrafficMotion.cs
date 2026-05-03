using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void AdvanceTrafficVehicle(TrafficVehicleState vehicle, float deltaTime)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0453: Unknown result type (might be due to invalid IL or missing references)
			//IL_0468: Unknown result type (might be due to invalid IL or missing references)
			//IL_051a: Unknown result type (might be due to invalid IL or missing references)
			//IL_051f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0535: Unknown result type (might be due to invalid IL or missing references)
			//IL_0542: Unknown result type (might be due to invalid IL or missing references)
			//IL_0550: Unknown result type (might be due to invalid IL or missing references)
			//IL_0552: Unknown result type (might be due to invalid IL or missing references)
			if (vehicle == null || (Object)(object)vehicle.Root == (Object)null)
			{
				return;
			}
			vehicle.StopTimer = Mathf.Max(0f, vehicle.StopTimer - deltaTime);
			vehicle.IntersectionCooldown = Mathf.Max(0f, vehicle.IntersectionCooldown - deltaTime);
			Vector3 position = vehicle.Root.position;
			float num = vehicle.CruiseSpeed;
			if (vehicle.StopTimer > 0f)
			{
				num = 0f;
			}
			else if (TryBeginTrafficIntersectionStop(vehicle, position))
			{
				num = 0f;
			}
			num *= GetTrafficSpeedScaleForFrontVehicle(vehicle, position);
			if (vehicle.PanicSlowTimer > 0f)
			{
				float num5 = ((vehicle.PanicSpeedScale > 0f) ? vehicle.PanicSpeedScale : 1f);
				num *= Mathf.Clamp(num5, 0.25f, 1f);
			}
			float num2 = Mathf.Max(2.2f, vehicle.CruiseSpeed * 2.35f);
			vehicle.CurrentSpeed = Mathf.MoveTowards(vehicle.CurrentSpeed, num, num2 * deltaTime);
			Vector3 val = (vehicle.AlongX ? new Vector3((float)vehicle.DirectionSign, 0f, 0f) : new Vector3(0f, 0f, (float)vehicle.DirectionSign));
			float num3 = Mathf.Max(0f, vehicle.CurrentSpeed * deltaTime);
			float num4 = num3;
			if (num3 > 0.0001f && TryLimitTrafficMoveByObstacle(vehicle, position, val, num3, out var constrainedDistance))
			{
				num4 = Mathf.Clamp(constrainedDistance, 0f, num3);
				if (num4 + 0.001f < num3)
				{
					float num5 = Mathf.Max(0.0001f, deltaTime);
					float num6 = num4 / num5;
					vehicle.CurrentSpeed = Mathf.Min(vehicle.CurrentSpeed, num6);
					float num7 = Mathf.Max(0.12f, trafficObstacleStopDuration);
					float num8 = Mathf.Max(0f, trafficObstacleStopDurationJitter);
					vehicle.StopTimer = Mathf.Max(vehicle.StopTimer, num7 + Mathf.Lerp(0f, num8, (float)trafficRng.NextDouble()));
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, 0.25f);
				}
			}
			Vector3 val2 = position + val * num4;
			val2.y = position.y;
			float num9 = Mathf.Clamp01(deltaTime * 7.5f);
			if (vehicle.AlongX)
			{
				val2.z = Mathf.Lerp(val2.z, vehicle.LaneCoord, num9);
				float num10 = trafficMapCenter.x - trafficMapHalfExtents.x - 2.2f;
				float num11 = trafficMapCenter.x + trafficMapHalfExtents.x + 2.2f;
				if (val2.x > num11)
				{
					val2.x = num10;
					vehicle.LaneCoord = val2.z + GetRandomTrafficLaneOffset();
					val2.z = vehicle.LaneCoord;
					vehicle.CruiseSpeed = GetRandomTrafficSpeed();
					vehicle.CurrentSpeed = Mathf.Max(vehicle.CurrentSpeed * 0.6f, vehicle.CruiseSpeed * 0.7f);
					vehicle.StopTimer = 0f;
					vehicle.PanicSlowTimer = 0f;
					vehicle.PanicSpeedScale = 1f;
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, 0.45f);
				}
				else if (val2.x < num10)
				{
					val2.x = num11;
					vehicle.LaneCoord = val2.z + GetRandomTrafficLaneOffset();
					val2.z = vehicle.LaneCoord;
					vehicle.CruiseSpeed = GetRandomTrafficSpeed();
					vehicle.CurrentSpeed = Mathf.Max(vehicle.CurrentSpeed * 0.6f, vehicle.CruiseSpeed * 0.7f);
					vehicle.StopTimer = 0f;
					vehicle.PanicSlowTimer = 0f;
					vehicle.PanicSpeedScale = 1f;
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, 0.45f);
				}
			}
			else
			{
				val2.x = Mathf.Lerp(val2.x, vehicle.LaneCoord, num9);
				float num12 = trafficMapCenter.z - trafficMapHalfExtents.y - 2.2f;
				float num13 = trafficMapCenter.z + trafficMapHalfExtents.y + 2.2f;
				if (val2.z > num13)
				{
					val2.z = num12;
					vehicle.LaneCoord = val2.x + GetRandomTrafficLaneOffset();
					val2.x = vehicle.LaneCoord;
					vehicle.CruiseSpeed = GetRandomTrafficSpeed();
					vehicle.CurrentSpeed = Mathf.Max(vehicle.CurrentSpeed * 0.6f, vehicle.CruiseSpeed * 0.7f);
					vehicle.StopTimer = 0f;
					vehicle.PanicSlowTimer = 0f;
					vehicle.PanicSpeedScale = 1f;
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, 0.45f);
				}
				else if (val2.z < num12)
				{
					val2.z = num13;
					vehicle.LaneCoord = val2.x + GetRandomTrafficLaneOffset();
					val2.x = vehicle.LaneCoord;
					vehicle.CruiseSpeed = GetRandomTrafficSpeed();
					vehicle.CurrentSpeed = Mathf.Max(vehicle.CurrentSpeed * 0.6f, vehicle.CruiseSpeed * 0.7f);
					vehicle.StopTimer = 0f;
					vehicle.PanicSlowTimer = 0f;
					vehicle.PanicSpeedScale = 1f;
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, 0.45f);
				}
			}
			float num14 = ((!vehicle.AlongX) ? ((vehicle.DirectionSign >= 0) ? 0f : 180f) : ((vehicle.DirectionSign >= 0) ? 90f : 270f));
			Quaternion val3 = Quaternion.Euler(0f, num14, 0f);
			if ((Object)(object)vehicle.Body != (Object)null)
			{
				vehicle.Body.MovePosition(val2);
				vehicle.Body.MoveRotation(val3);
			}
			else
			{
				vehicle.Root.SetPositionAndRotation(val2, val3);
			}
		}

		private bool TryLimitTrafficMoveByObstacle(TrafficVehicleState vehicle, Vector3 currentPosition, Vector3 moveDirection, float desiredDistance, out float constrainedDistance)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			constrainedDistance = desiredDistance;
			if (vehicle == null || (Object)(object)vehicle.Root == (Object)null || desiredDistance <= 0.0001f)
			{
				return false;
			}
			Vector3 val = default(Vector3);
			val = new Vector3(Mathf.Max(0.25f, trafficObstacleProbeHalfExtents.x), Mathf.Max(0.16f, trafficObstacleProbeHalfExtents.y), Mathf.Max(0.35f, trafficObstacleProbeHalfExtents.z));
			float num = Mathf.Max(0.42f, val.y + 0.18f);
			Vector3 val2 = new Vector3(currentPosition.x, num, currentPosition.z);
			Quaternion rotation = vehicle.Root.rotation;
			float num2 = desiredDistance + Mathf.Max(0f, trafficObstacleLookAhead);
			int num3 = Physics.BoxCastNonAlloc(val2, val, moveDirection, trafficObstacleHits, rotation, num2, -1, (QueryTriggerInteraction)1);
			if (num3 <= 0)
			{
				return false;
			}
			float num4 = num2;
			bool flag = false;
			Rigidbody body = vehicle.Body;
			for (int i = 0; i < num3; i++)
			{
				RaycastHit val3 = trafficObstacleHits[i];
				Collider collider = val3.collider;
				if ((Object)(object)collider == (Object)null || !collider.enabled || collider.isTrigger || ((Object)collider).name == "Ground")
				{
					continue;
				}
				Transform transform = ((Component)collider).transform;
				if (((Object)(object)transform != (Object)null && transform.IsChildOf(vehicle.Root)) || ((Object)(object)body != (Object)null && (Object)(object)val3.rigidbody == (Object)(object)body))
				{
					continue;
				}
				DummyStreetPropReactive dummyStreetPropReactive = ((Component)collider).GetComponent<DummyStreetPropReactive>() ?? ((Component)collider).GetComponentInParent<DummyStreetPropReactive>();
				if ((Object)(object)dummyStreetPropReactive != (Object)null && dummyStreetPropReactive.Kind == DummyStreetPropReactive.PropKind.Vehicle)
				{
					continue;
				}
				Rigidbody rigidbody = val3.rigidbody;
				if (!((Object)(object)rigidbody != (Object)null) || rigidbody.isKinematic || !(rigidbody.mass <= 2f))
				{
					flag = true;
					if (val3.distance < num4)
					{
						num4 = val3.distance;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			float num5 = Mathf.Clamp(trafficObstacleBuffer, 0f, 0.4f);
			constrainedDistance = Mathf.Max(0f, Mathf.Min(desiredDistance, num4 - num5));
			return true;
		}

		private void UpdateTrafficSignalCycle(float deltaTime)
		{
			if (!enableTrafficSignals)
			{
				return;
			}
			float num = Mathf.Max(1f, trafficSignalPhaseSeconds);
			float num2 = Mathf.Clamp(trafficSignalAllRedSeconds, 0f, 2f);
			trafficSignalPhaseTimer -= deltaTime;
			if (!(trafficSignalPhaseTimer > 0f))
			{
				if (trafficSignalAllRed)
				{
					trafficSignalAllRed = false;
					trafficAlongXGreen = !trafficAlongXGreen;
					trafficSignalPhaseTimer = num;
				}
				else if (num2 > 0.01f)
				{
					trafficSignalAllRed = true;
					trafficSignalPhaseTimer = num2;
				}
				else
				{
					trafficAlongXGreen = !trafficAlongXGreen;
					trafficSignalPhaseTimer = num;
				}
			}
		}

		private bool HasTrafficGreenSignal(TrafficVehicleState vehicle)
		{
			if (!enableTrafficSignals || vehicle == null)
			{
				return true;
			}
			if (trafficSignalAllRed)
			{
				return false;
			}
			if (!vehicle.AlongX)
			{
				return !trafficAlongXGreen;
			}
			return trafficAlongXGreen;
		}

		private float GetTrafficSignalWaitTime(TrafficVehicleState vehicle)
		{
			if (vehicle == null || !enableTrafficSignals)
			{
				return GetRandomTrafficIntersectionStopTime();
			}
			float num = Mathf.Max(0.12f, trafficSignalPhaseTimer);
			if (!trafficSignalAllRed)
			{
				num += Mathf.Clamp(trafficSignalAllRedSeconds, 0f, 2f);
			}
			return Mathf.Clamp(num + 0.06f, 0.18f, 8f);
		}

		private bool TryBeginTrafficIntersectionStop(TrafficVehicleState vehicle, Vector3 currentPosition)
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			if (vehicle == null || trafficIntersections.Count <= 0)
			{
				return false;
			}
			if (vehicle.StopTimer > 0f || vehicle.IntersectionCooldown > 0f)
			{
				return false;
			}
			float num = Mathf.Max(0.8f, trafficIntersectionRadius);
			float num2 = 0.6f + Mathf.Max(0f, trafficLaneJitter) * 2f;
			float num3 = float.MaxValue;
			for (int i = 0; i < trafficIntersections.Count; i++)
			{
				Vector2 val = trafficIntersections[i];
				float num4;
				if (vehicle.AlongX)
				{
					if (Mathf.Abs(val.y - vehicle.LaneCoord) > num2)
					{
						continue;
					}
					num4 = (val.x - currentPosition.x) * (float)vehicle.DirectionSign;
				}
				else
				{
					if (Mathf.Abs(val.x - vehicle.LaneCoord) > num2)
					{
						continue;
					}
					num4 = (val.y - currentPosition.z) * (float)vehicle.DirectionSign;
				}
				if (!(num4 < 0f) && !(num4 > num) && num4 < num3)
				{
					num3 = num4;
				}
			}
			if (num3 == float.MaxValue)
			{
				return false;
			}
			if (enableTrafficSignals)
			{
				if (!HasTrafficGreenSignal(vehicle))
				{
					vehicle.StopTimer = GetTrafficSignalWaitTime(vehicle);
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, vehicle.StopTimer + 0.2f);
					return true;
				}
				if ((float)trafficRng.NextDouble() <= Mathf.Clamp01(trafficYieldChanceOnGreen))
				{
					vehicle.StopTimer = Mathf.Min(GetRandomTrafficIntersectionStopTime(), 0.55f);
					vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, vehicle.StopTimer + 0.2f);
					return true;
				}
				return false;
			}
			if ((float)trafficRng.NextDouble() > Mathf.Clamp01(trafficIntersectionStopChance))
			{
				vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, 0.85f);
				return false;
			}
			vehicle.StopTimer = GetRandomTrafficIntersectionStopTime();
			vehicle.IntersectionCooldown = Mathf.Max(vehicle.IntersectionCooldown, vehicle.StopTimer + 0.35f);
			return true;
		}

		private float GetTrafficSpeedScaleForFrontVehicle(TrafficVehicleState vehicle, Vector3 currentPosition)
		{
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			if (vehicle == null)
			{
				return 1f;
			}
			float num = Mathf.Max(1.8f, trafficFollowingDistance);
			float num2 = Mathf.Clamp(trafficBrakeDistance, 0.7f, num - 0.2f);
			float num3 = 0.62f + Mathf.Max(0f, trafficLaneJitter) * 2f;
			float num4 = float.MaxValue;
			for (int i = 0; i < trafficVehicles.Count; i++)
			{
				TrafficVehicleState trafficVehicleState = trafficVehicles[i];
				if (trafficVehicleState == null || (Object)(object)trafficVehicleState.Root == (Object)null || !((Component)trafficVehicleState.Root).gameObject.activeInHierarchy || trafficVehicleState == vehicle || trafficVehicleState.AlongX != vehicle.AlongX || trafficVehicleState.DirectionSign != vehicle.DirectionSign)
				{
					continue;
				}
				Vector3 position = trafficVehicleState.Root.position;
				if (!((vehicle.AlongX ? Mathf.Abs(position.z - vehicle.LaneCoord) : Mathf.Abs(position.x - vehicle.LaneCoord)) > num3))
				{
					float num5 = (vehicle.AlongX ? ((position.x - currentPosition.x) * (float)vehicle.DirectionSign) : ((position.z - currentPosition.z) * (float)vehicle.DirectionSign));
					if (!(num5 <= 0f) && num5 < num4)
					{
						num4 = num5;
					}
				}
			}
			if (num4 == float.MaxValue || num4 >= num)
			{
				return 1f;
			}
			if (num4 <= num2)
			{
				return 0f;
			}
			return Mathf.InverseLerp(num2, num, num4);
		}

		private float GetRandomTrafficIntersectionStopTime()
		{
			float num = Mathf.Max(0.1f, Mathf.Min(trafficIntersectionStopDuration.x, trafficIntersectionStopDuration.y));
			float num2 = Mathf.Max(num + 0.05f, Mathf.Max(trafficIntersectionStopDuration.x, trafficIntersectionStopDuration.y));
			return Mathf.Lerp(num, num2, (float)trafficRng.NextDouble());
		}
	}
}
