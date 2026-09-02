using System;
using System.Collections.Generic;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void FillOpeningStretchStreetProps(Transform streetPropsRoot, Transform microPropsRoot, List<Vector4> footprints, RuntimeStageMapLayout layout, Color carA, Color carB, Color barrelA, Color barrelB, Color shopA, Color shopB)
		{
			if (layout.Stage < 1 || layout.Stage > 10 || (Object)(object)streetPropsRoot == (Object)null || footprints == null)
			{
				return;
			}

			Transform kitParent = (Object)(object)microPropsRoot != (Object)null ? microPropsRoot : streetPropsRoot;
			System.Random random = new System.Random(3317 + layout.Stage * 97);
			int maxPlace = 12;
			float nearbyRadius = 1.45f;
			float stepScale = 0.58f;
			int curbCount = 2;
			if (layout.Stage >= 8)
			{
				maxPlace = Mathf.Clamp(22 + (layout.Stage - 8) * 5, 22, 32);
				nearbyRadius = 1.28f;
				stepScale = 0.5f;
				curbCount = 4;
			}

			int placed = 0;
			Vector3 targetA = new Vector3(-layout.TargetX, 0f, layout.TargetForwardZ);
			Vector3 targetB = new Vector3(layout.TargetX, 0f, layout.TargetReturnZ);
			Vector2 cluster = ResolveStarterClusterCenter(layout);
			float innerCurbX = Mathf.Clamp(layout.SpawnLaneHalfWidth - 1.05f, 2.55f, 3.55f);
			float outerCurbX = Mathf.Clamp(layout.SpawnLaneHalfWidth - 0.42f, innerCurbX + 1.55f, 6.2f);
			float zStart = layout.GridStartZ + layout.CellSize * 0.2f;
			float zEnd = Mathf.Max(layout.SpawnLaneEndZ + layout.CellSize * 0.35f, cluster.y + 6.8f);
			zEnd = Mathf.Min(zEnd, layout.TargetForwardZ - 4.2f);
			if (layout.Stage >= 8)
			{
				zStart = Mathf.Max(zStart, cluster.y - 4.8f);
				zEnd = Mathf.Min(zEnd, cluster.y + 8.4f);
				zEnd = Mathf.Min(zEnd, layout.MidDistrictEndZ - 2.4f);
			}
			else if (layout.Stage >= 4)
			{
				zEnd = Mathf.Min(zEnd, layout.SpawnLaneEndZ + layout.CellSize * 0.85f);
				zEnd = Mathf.Min(zEnd, layout.MidDistrictEndZ - 2.4f);
			}

			float stepZ = layout.CellSize * stepScale;

			for (float z = zStart; z <= zEnd && placed < maxPlace; z += stepZ)
			{
				for (int curb = 0; curb < curbCount && placed < maxPlace; curb++)
				{
					float x = ResolveOpeningCurbX(curb, innerCurbX, outerCurbX);
					Vector3 pos = new Vector3(x, 0f, z);
					if (Mathf.Abs(pos.x) < 1.72f
						|| IsTooCloseToRouteMarker(pos.x, pos.z, targetA, targetB, 3.4f)
						|| IsBlockedOpeningSmashSite(pos.x, pos.z, layout))
					{
						continue;
					}

					if (HasNearbyStreetProp(streetPropsRoot, kitParent, pos, nearbyRadius) || OverlapsAnyFootprintRuntime(footprints, pos.x, pos.z, 0.42f, 0.42f))
					{
						continue;
					}

					string suffix = $"Open_{layout.Stage:00}_{placed:00}";
					PlaceOpeningStretchKit(streetPropsRoot, kitParent, suffix, placed, pos, carA, carB, barrelA, barrelB, shopA, shopB, random);
					AddFootprintRuntime(footprints, pos.x, pos.z, 0.42f, 0.42f);
					placed++;
				}
			}
		}

		private static float ResolveOpeningCurbX(int curbIndex, float innerCurbX, float outerCurbX)
		{
			switch (curbIndex)
			{
				case 0:
					return -innerCurbX;
				case 1:
					return innerCurbX;
				case 2:
					return -outerCurbX;
				default:
					return outerCurbX;
			}
		}

		private static void PlaceOpeningStretchKit(Transform streetPropsRoot, Transform kitParent, string suffix, int placed, Vector3 pos, Color carA, Color carB, Color barrelA, Color barrelB, Color shopA, Color shopB, System.Random random)
		{
			switch (placed % 5)
			{
				case 0:
					EnsureStreetLampRuntime(streetPropsRoot, "GapLamp_" + suffix, pos);
					break;
				case 1:
					EnsureTrafficVehicleRuntime(streetPropsRoot, "GapCar_" + suffix, pos, 0f, Color.Lerp(carA, carB, (float)random.NextDouble()));
					break;
				case 2:
					EnsureCommercialBenchRuntime(kitParent, "GapBench_" + suffix, new Vector3(pos.x, 0.22f, pos.z), Color.Lerp(shopA, shopB, 0.35f));
					break;
				case 3:
					EnsureCommercialKioskRuntime(kitParent, "GapKiosk_" + suffix, new Vector3(pos.x, 0.48f, pos.z), new Vector3(0.68f, 0.68f, 0.58f), Color.Lerp(shopA, shopB, 0.45f));
					break;
				default:
					EnsureExplosiveBarrelRuntime(streetPropsRoot, "GapBarrel_" + suffix, pos, Color.Lerp(barrelA, barrelB, (float)random.NextDouble()));
					break;
			}
		}

		private void FillRouteGapStreetProps(Transform streetPropsRoot, Transform microPropsRoot, List<Vector4> footprints, RuntimeStageMapLayout layout, Color carA, Color carB, Color barrelA, Color barrelB, Color shopA, Color shopB)
		{
			if ((Object)(object)streetPropsRoot == (Object)null || footprints == null)
			{
				return;
			}

			FillOpeningStretchStreetProps(streetPropsRoot, microPropsRoot, footprints, layout, carA, carB, barrelA, barrelB, shopA, shopB);

			Transform kitParent = (Object)(object)microPropsRoot != (Object)null ? microPropsRoot : streetPropsRoot;
			System.Random random = new System.Random(7741 + layout.Stage * 211);
			int maxPlace = layout.Stage <= 1
				? Mathf.Clamp(16 + layout.GrowthTier * 2, 16, 28)
				: Mathf.Clamp(20 + layout.Stage * 4 + layout.GrowthTier * 2, 24, 52);
			float nearbyRadius = layout.Stage <= 1 ? 1.65f : 1.48f;
			int placed = 0;
			Vector3 targetA = new Vector3(-layout.TargetX, 0f, layout.TargetForwardZ);
			Vector3 targetB = new Vector3(layout.TargetX, 0f, layout.TargetReturnZ);

			for (int i = 0; i < layout.ZCells && placed < maxPlace; i++)
			{
				float z = layout.GridStartZ + i * layout.CellSize;
				bool roadRow = i % layout.RoadRowStride == 2;
				for (int j = 0; j < layout.XCells && placed < maxPlace; j++)
				{
					float x = layout.GridStartX + j * layout.CellSize;
					bool roadCol = j % layout.RoadColumnStride == 3;
					if (!roadRow && !roadCol)
					{
						continue;
					}

					bool spawnLane = z < layout.SpawnLaneEndZ && Mathf.Abs(x) < layout.SpawnLaneHalfWidth;
					if (spawnLane || !IsBetweenMainRouteTargets(x, z, layout) || IsInsideRuntimeLandmarkClearance(x, z, layout))
					{
						continue;
					}

					float side = ((i + j) & 1) == 0 ? -1f : 1f;
					Vector3 pos = roadRow && !roadCol
						? new Vector3(x + side * 0.92f, 0f, z)
						: new Vector3(x, 0f, z + side * 0.92f);
					if (IsTooCloseToRouteMarker(pos.x, pos.z, targetA, targetB, 3.4f))
					{
						continue;
					}

					if (HasNearbyStreetProp(streetPropsRoot, kitParent, pos, nearbyRadius) || OverlapsAnyFootprintRuntime(footprints, pos.x, pos.z, 0.42f, 0.42f))
					{
						continue;
					}

					string suffix = $"{placed:00}";
					int variety = layout.Stage <= 1 ? 5 : 7;
					switch (placed % variety)
					{
						case 0:
							EnsureStreetLampRuntime(streetPropsRoot, "GapLamp_" + suffix, pos);
							break;
						case 1:
							EnsureTrafficVehicleRuntime(streetPropsRoot, "GapCar_" + suffix, pos, roadRow ? 90f : 0f, Color.Lerp(carA, carB, (float)random.NextDouble()));
							break;
						case 2:
							EnsureCommercialBenchRuntime(kitParent, "GapBench_" + suffix, new Vector3(pos.x, 0.22f, pos.z), Color.Lerp(shopA, shopB, 0.35f));
							break;
						case 3:
							EnsureCommercialKioskRuntime(kitParent, "GapKiosk_" + suffix, new Vector3(pos.x, 0.48f, pos.z), new Vector3(0.68f, 0.68f, 0.58f), Color.Lerp(shopA, shopB, 0.45f));
							break;
						case 4:
							EnsureExplosiveBarrelRuntime(streetPropsRoot, "GapBarrel_" + suffix, pos, Color.Lerp(barrelA, barrelB, (float)random.NextDouble()));
							break;
						case 5:
							EnsureStreetTreeRuntime(streetPropsRoot, "GapTree_" + suffix, pos, Color.Lerp(shopA, barrelA, 0.55f));
							break;
						default:
							if ((placed / variety) % 2 == 0)
							{
								EnsureCommercialVendingRuntime(kitParent, "GapVending_" + suffix, new Vector3(pos.x, 0.51f, pos.z), Color.Lerp(shopA, shopB, 0.4f));
							}
							else
							{
								EnsureCommercialBusStopRuntime(kitParent, "GapBusStop_" + suffix, new Vector3(pos.x, 0.63f, pos.z), Color.Lerp(shopA, shopB, 0.5f));
							}
							break;
					}

					AddFootprintRuntime(footprints, pos.x, pos.z, 0.42f, 0.42f);
					placed++;
				}
			}
		}

		private static bool HasAuthoredOpeningStretch(Transform mapRoot)
		{
			if ((Object)(object)mapRoot == (Object)null)
			{
				return false;
			}

			return CountAuthoredOpeningStretchKits(FindChildByName(mapRoot, "StreetProps")) > 0
				|| CountAuthoredOpeningStretchKits(FindChildByName(mapRoot, "MicroProps")) > 0
				|| CountAuthoredOpeningStretchKits(mapRoot) > 0;
		}

		private static int CountAuthoredOpeningStretchKits(Transform root)
		{
			if ((Object)(object)root == (Object)null)
			{
				return 0;
			}

			int count = 0;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform child = root.GetChild(i);
				if ((Object)(object)child == (Object)null)
				{
					continue;
				}

				string objectName = ((UnityEngine.Object)child).name;
				if (!string.IsNullOrEmpty(objectName)
					&& objectName.StartsWith("Gap", System.StringComparison.OrdinalIgnoreCase)
					&& objectName.IndexOf("_Open_", System.StringComparison.Ordinal) >= 0)
				{
					count++;
				}

				count += CountAuthoredOpeningStretchKits(child);
			}

			return count;
		}

		private static int CountOpeningStreetProps(Transform mapRoot, RuntimeStageMapLayout layout)
		{
			if ((Object)(object)mapRoot == (Object)null)
			{
				return 0;
			}

			DummyStreetPropReactive[] props = Object.FindObjectsByType<DummyStreetPropReactive>((FindObjectsInactive)1);
			if (props == null || props.Length == 0)
			{
				return 0;
			}

			Vector2 center = ResolveStarterClusterCenter(layout);
			int count = 0;
			for (int i = 0; i < props.Length; i++)
			{
				DummyStreetPropReactive prop = props[i];
				if ((Object)(object)prop == (Object)null || !prop.transform.IsChildOf(mapRoot))
				{
					continue;
				}

				if (IsMovingTrafficProp(prop.transform))
				{
					continue;
				}

				Vector3 localPosition = mapRoot.InverseTransformPoint(prop.transform.position);
				if (Mathf.Abs(localPosition.x - center.x) <= layout.SpawnLaneHalfWidth + 1.4f && localPosition.z >= center.y - 4.8f && localPosition.z <= center.y + 8.4f)
				{
					count++;
				}
			}

			return count;
		}

		private static bool IsMovingTrafficProp(Transform node)
		{
			while ((Object)(object)node != (Object)null)
			{
				if (((UnityEngine.Object)node).name.StartsWith("Car_RT_", System.StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				node = node.parent;
			}

			return false;
		}

		private static bool IsBlockedOpeningSmashSite(float x, float z, RuntimeStageMapLayout layout)
		{
			if (z >= layout.MidDistrictEndZ)
			{
				return true;
			}

			if (IsInsideRuntimeLandmarkClearance(x, z, layout))
			{
				return true;
			}

			if (layout.Stage < 4)
			{
				return false;
			}

			// Stage 4+ Sentinel / late-route boss kit: keep opening Gap* off the arena, core, pylons, and weak-points.
			Vector2 sentinel = ResolveRuntimeLandmarkCenter(layout, 2);
			float dx = x - sentinel.x;
			float dz = z - sentinel.y;
			return dx * dx + dz * dz < 6.4f * 6.4f;
		}

		private static bool IsBetweenMainRouteTargets(float x, float z, RuntimeStageMapLayout layout)
		{
			float minZ = Mathf.Min(layout.TargetReturnZ, layout.SpawnLaneEndZ) - 0.4f;
			float maxZ = layout.TargetForwardZ + 1.1f;
			if (z < minZ || z > maxZ)
			{
				return false;
			}

			return Mathf.Abs(x) <= layout.TargetX + 2.6f;
		}

		private static bool IsTooCloseToRouteMarker(float x, float z, Vector3 targetA, Vector3 targetB, float minDistance)
		{
			float dxA = x - targetA.x;
			float dzA = z - targetA.z;
			if (dxA * dxA + dzA * dzA < minDistance * minDistance)
			{
				return true;
			}

			float dxB = x - targetB.x;
			float dzB = z - targetB.z;
			return dxB * dxB + dzB * dzB < minDistance * minDistance;
		}

		private static bool HasNearbyStreetProp(Transform streetPropsRoot, Transform microPropsRoot, Vector3 localPosition, float radius)
		{
			return HasNearbyChild(streetPropsRoot, localPosition, radius) || HasNearbyChild(microPropsRoot, localPosition, radius);
		}

		private static bool HasNearbyChild(Transform root, Vector3 localPosition, float radius)
		{
			if ((Object)(object)root == (Object)null)
			{
				return false;
			}

			float limit = radius * radius;
			for (int i = 0; i < root.childCount; i++)
			{
				Transform child = root.GetChild(i);
				if ((Object)(object)child == (Object)null)
				{
					continue;
				}

				Vector3 delta = child.localPosition - localPosition;
				delta.y = 0f;
				if (delta.sqrMagnitude <= limit)
				{
					return true;
				}
			}

			return false;
		}
	}
}
