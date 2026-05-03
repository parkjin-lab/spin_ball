using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private int runtimeTrafficDesiredCars;
		private Vector2 runtimeTrafficSpeedRange;
		private float runtimeTrafficRespawnInterval;

		private void InitializeTrafficSystem()
		{
			trafficVehicles.Clear();
			trafficSpawnPoints.Clear();
			trafficIntersections.Clear();
			trafficStreetPropsRoot = null;
			trafficSpawnSerial = 0;
			trafficAlongXGreen = true;
			trafficSignalAllRed = false;
			trafficSignalPhaseTimer = Mathf.Max(1f, trafficSignalPhaseSeconds);
			ApplyStageTrafficTuning();
			trafficRespawnTick = Mathf.Max(0.2f, GetRuntimeTrafficRespawnInterval());
			if (!enableTrafficSimulation || !Application.isPlaying)
			{
				return;
			}
			Transform val = FindChildByName(null, "MapRoot");
			if (!((Object)(object)val == (Object)null))
			{
				ResolveTrafficBounds(val);
				Transform val2 = FindChildByName(val, "StreetProps");
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = GetOrCreateDirectChild(val, "StreetProps");
				}
				trafficStreetPropsRoot = val2;
				ApplyStageStreetPropTuning(val2);
				CollectTrafficSpawnPoints(val2);
				if (trafficSpawnPoints.Count <= 0)
				{
					PopulateDefaultTrafficSpawnPoints();
				}
				RebuildTrafficIntersections();
				RegisterExistingTrafficVehicles(val2);
				EnsureTrafficVehiclePopulation(fillImmediately: true);
			}
		}

		private void UpdateTrafficSimulation(float deltaTime)
		{
			if (!enableTrafficSimulation || !Application.isPlaying)
			{
				return;
			}
			if ((Object)(object)trafficStreetPropsRoot == (Object)null)
			{
				InitializeTrafficSystem();
				if ((Object)(object)trafficStreetPropsRoot == (Object)null)
				{
					return;
				}
			}
			UpdateTrafficSignalCycle(deltaTime);
			trafficPanicBonusCooldownRemaining = Mathf.Max(0f, trafficPanicBonusCooldownRemaining - deltaTime);
			trafficPanicChainRemaining = Mathf.Max(0f, trafficPanicChainRemaining - deltaTime);
			if (trafficPanicChainRemaining <= 0.001f)
			{
				trafficPanicChainStack = 0;
				trafficPanicJackpotTriggered = false;
			}
			for (int num = trafficVehicles.Count - 1; num >= 0; num--)
			{
				TrafficVehicleState trafficVehicleState = trafficVehicles[num];
				if (trafficVehicleState == null || (Object)(object)trafficVehicleState.Root == (Object)null || !((Component)trafficVehicleState.Root).gameObject.activeInHierarchy)
				{
					trafficVehicles.RemoveAt(num);
				}
				else
				{
					trafficVehicleState.PanicSlowTimer = Mathf.Max(0f, trafficVehicleState.PanicSlowTimer - deltaTime);
					if (trafficVehicleState.PanicSlowTimer <= 0.001f)
					{
						trafficVehicleState.PanicSpeedScale = 1f;
					}
					AdvanceTrafficVehicle(trafficVehicleState, deltaTime);
				}
			}
			if (GetRuntimeTrafficRespawnInterval() <= 0f)
			{
				EnsureTrafficVehiclePopulation(fillImmediately: true);
				return;
			}
			trafficRespawnTick -= deltaTime;
			if (!(trafficRespawnTick > 0f))
			{
				trafficRespawnTick = Mathf.Max(0.2f, GetRuntimeTrafficRespawnInterval());
				EnsureTrafficVehiclePopulation(fillImmediately: false);
			}
		}

		private void ApplyStageTrafficTuning()
		{
			int num = Mathf.Max(1, currentStageNumber);
			float num2 = Mathf.Clamp01((float)(num - 1) / 6f);
			runtimeTrafficDesiredCars = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(12f, 30f, num2)), 8, 40);
			float num3 = Mathf.Lerp(2.3f, 3.5f, num2);
			float num4 = Mathf.Lerp(4.2f, 7.2f, num2);
			runtimeTrafficSpeedRange = new Vector2(num3, Mathf.Max(num3 + 0.2f, num4));
			runtimeTrafficRespawnInterval = Mathf.Lerp(1.35f, 0.65f, num2);
			if (num <= 2)
			{
				runtimeTrafficDesiredCars += 2;
				runtimeTrafficRespawnInterval *= 0.82f;
			}
			else if (num == 3)
			{
				runtimeTrafficDesiredCars += 1;
				runtimeTrafficRespawnInterval *= 0.9f;
			}
		}

		private void ApplyStageStreetPropTuning(Transform streetPropsRoot)
		{
			if ((Object)(object)streetPropsRoot == (Object)null)
			{
				return;
			}

			AlienCrusher.Gameplay.DummyStreetPropReactive[] componentsInChildren = ((Component)streetPropsRoot).GetComponentsInChildren<AlienCrusher.Gameplay.DummyStreetPropReactive>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				AlienCrusher.Gameplay.DummyStreetPropReactive dummyStreetPropReactive = componentsInChildren[i];
				if (!((Object)(object)dummyStreetPropReactive == (Object)null))
				{
					dummyStreetPropReactive.ApplyStageLayoutTuning(currentStageNumber);
				}
			}
		}

		private int GetRuntimeTrafficDesiredCars()
		{
			return Mathf.Max(0, runtimeTrafficDesiredCars > 0 ? runtimeTrafficDesiredCars : trafficDesiredCars);
		}

		private Vector2 GetRuntimeTrafficSpeedRange()
		{
			if (runtimeTrafficSpeedRange.y > 0.001f)
			{
				return runtimeTrafficSpeedRange;
			}

			return trafficSpeedRange;
		}

		private float GetRuntimeTrafficRespawnInterval()
		{
			if (runtimeTrafficRespawnInterval > 0f)
			{
				return runtimeTrafficRespawnInterval;
			}

			return trafficRespawnInterval;
		}
	}
}
