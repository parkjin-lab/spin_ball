using System;
using System.IO;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private string playtestTelemetryLogPath;
		private bool playtestTelemetryLogWriteFailed;

		private void EmitPlaytestTelemetry(string eventName, string detail = "")
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!enablePlaytestTelemetry)
			{
				return;
			}
			string suffix = string.IsNullOrWhiteSpace(detail) ? string.Empty : $" {detail}";
			string line = $"[AlienCrusher][Playtest] time={DateTime.Now:yyyy-MM-dd HH:mm:ss} stage={Mathf.Max(1, currentStageNumber):00} event={eventName}{suffix}";
			Debug.Log((object)line);
			AppendPlaytestTelemetryLine(line);
#endif
		}

		private void AppendPlaytestTelemetryLine(string line)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (playtestTelemetryLogWriteFailed || string.IsNullOrWhiteSpace(line))
			{
				return;
			}
			try
			{
				string logPath = GetPlaytestTelemetryLogPath();
				if (string.IsNullOrWhiteSpace(logPath))
				{
					return;
				}
				string directoryPath = Path.GetDirectoryName(logPath);
				if (!string.IsNullOrWhiteSpace(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}
				File.AppendAllText(logPath, line + Environment.NewLine);
			}
			catch (Exception exception)
			{
				playtestTelemetryLogWriteFailed = true;
				Debug.LogWarning((object)$"[AlienCrusher][Playtest] Failed to append telemetry log: {exception.Message}");
			}
#endif
		}

		private string GetPlaytestTelemetryLogPath()
		{
#if UNITY_EDITOR
			if (!string.IsNullOrWhiteSpace(playtestTelemetryLogPath))
			{
				return playtestTelemetryLogPath;
			}
			string projectRoot = Path.GetDirectoryName(Application.dataPath);
			if (string.IsNullOrWhiteSpace(projectRoot))
			{
				return string.Empty;
			}
			playtestTelemetryLogPath = Path.Combine(projectRoot, "Logs", "AlienCrusherPlaytestTelemetry.log");
			return playtestTelemetryLogPath;
#elif DEVELOPMENT_BUILD
			if (!string.IsNullOrWhiteSpace(playtestTelemetryLogPath))
			{
				return playtestTelemetryLogPath;
			}
			playtestTelemetryLogPath = Path.Combine(Application.persistentDataPath, "AlienCrusherPlaytestTelemetry.log");
			return playtestTelemetryLogPath;
#else
			return string.Empty;
#endif
		}

		private void LogPlaytestStageStart()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			ResolveStageAdvanceRouteMarkers();
			float stageTimerSeconds = ((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f;
			string markerAText = FormatPlaytestMarkerSnapshot(stageAdvanceRouteMarkerA);
			string markerBText = FormatPlaytestMarkerSnapshot(stageAdvanceRouteMarkerB);
			EmitPlaytestTelemetry(
				"STAGE_START",
				$"timer={stageTimerSeconds:0.#}s target={Mathf.Max(0, stageAdvanceDestroyTarget):0} routeTarget={GetRouteHoldTarget():0} deadline={GetRouteHoldDeadlineSeconds():0.#}s destructibles={Mathf.Max(0, stageTotalDestructibleCount):0} payoff={GetRouteDistrictPayoffLabel()} markers={markerAText} {markerBText}");
#endif
		}

		private void LogPlaytestRouteOpen(int destroyedCount)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			EmitPlaytestTelemetry(
				"ROUTE_OPEN",
				$"destroyed={Mathf.Max(0, destroyedCount):0} laneBreak={GetEarlyCrushLaneBreakTarget():0} routeTarget={GetRouteHoldTarget():0} deadlineLeft={GetRouteHoldRemainingSeconds():0.#}s target={FormatPlaytestActiveTarget()} {FormatRouteHoldTelemetrySnapshot()}");
#endif
		}

		private void LogPlaytestRouteHoldSuccess(int destroyedCount)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			EmitPlaytestTelemetry(
				"ROUTE_HOLD_CLEAR",
				$"destroyed={Mathf.Max(0, destroyedCount):0} routeTarget={GetRouteHoldTarget():0} rewardScore={Mathf.Max(0, routeHoldBonusScore):0} payoff={GetRouteDistrictPayoffLabel()} next={FormatPlaytestActiveTarget()} {FormatRouteHoldTelemetrySnapshot()}");
#endif
		}

		private void LogPlaytestRouteBonusClaim(string districtLabel)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			string smashTarget = ((Object)(object)forwardSmashTargetBlock != (Object)null) ? ((Component)forwardSmashTargetBlock).transform.name : "none";
			EmitPlaytestTelemetry(
				"ROUTE_BONUS",
				$"score={Mathf.Max(0, stageAdvanceRouteRewardScore):0} payoff={districtLabel} marker={FormatPlaytestActiveTarget()} smash={smashTarget}");
#endif
		}

		private void LogPlaytestForwardSmash(string targetName, int bonusScore)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int highestChain = ((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(1, scoreSystem.HighestChain) : 1;
			EmitPlaytestTelemetry(
				"FORWARD_SMASH",
				$"target={targetName} score={Mathf.Max(0, bonusScore):0} radius={Mathf.Max(0f, forwardSmashBonusRadius):0.#} chain={highestChain:0}");
#endif
		}

		private void LogPlaytestStageEnd()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int destroyedCount = ((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0;
			int highestChain = ((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(1, scoreSystem.HighestChain) : 1;
			int totalDestructibles = Mathf.Max(1, stageTotalDestructibleCount);
			int destroyedPercent = Mathf.RoundToInt(Mathf.Clamp01((float)destroyedCount / (float)totalDestructibles) * 100f);
			string resultLabel = DidStageEndInSuccess() ? "VICTORY" : "DEFEAT";
			string bossLabel = !stageBossEncounterActive ? "NONE" : (IsStageBossAlive() ? "ALIVE" : "DOWN");
			string bucket = string.IsNullOrWhiteSpace(GetLastRunFailureBucket()) ? "NONE" : GetLastRunFailureBucket();
			EmitPlaytestTelemetry(
				"STAGE_END",
				$"result={resultLabel} reason={GetStageEndReasonLabel().ToUpperInvariant()} bucket={bucket} destroyed={destroyedCount:0}/{Mathf.Max(0, stageTotalDestructibleCount):0} pct={destroyedPercent:0} chain={highestChain:0} laneBreakTier={earlyCrushFlowBonusIndex:0} routeHold={(routeHoldBonusGranted ? "yes" : "no")} routeBonus={(stageAdvanceRouteRewardGranted ? "yes" : "no")} boss={bossLabel} payoff={GetRouteDistrictPayoffLabel()} {FormatRouteHoldTelemetrySnapshot()}");
			routeHoldTelemetryActive = false;
#endif
		}

		private void LogPlaytestSweepStart(int maxStage, int restoreStage)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			EmitPlaytestTelemetry(
				"SWEEP_START",
				$"maxStage={Mathf.Max(1, maxStage):00} restore={Mathf.Max(1, restoreStage):00}");
#endif
		}

		private void LogPlaytestSweepEnd(int restoredStage, bool completed)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			EmitPlaytestTelemetry(
				"SWEEP_END",
				$"restored={Mathf.Max(1, restoredStage):00} completed={(completed ? "yes" : "no")}");
#endif
		}

		private string FormatPlaytestActiveTarget()
		{
			if ((Object)(object)activeStageAdvanceRouteMarker == (Object)null)
			{
				return "none";
			}
			return FormatPlaytestMarkerSnapshot(activeStageAdvanceRouteMarker);
		}

		private string FormatPlaytestMarkerSnapshot(Transform marker)
		{
			if ((Object)(object)marker == (Object)null)
			{
				return "none";
			}
			Vector3 position = marker.position;
			string distanceText = "n/a";
			if ((Object)(object)playerTransform != (Object)null)
			{
				distanceText = $"{Vector3.Distance(playerTransform.position, position):0.#}m";
			}
			return $"{marker.name}({position.x:0.#},{position.z:0.#},{distanceText})";
		}

		private void ResetRouteHoldTelemetryState()
		{
			routeHoldTelemetryActive = false;
			routeHoldTelemetrySamples = 0;
			routeHoldTelemetryInTrailRangeSamples = 0;
			routeHoldTelemetryElapsed = 0f;
			routeHoldTelemetryDistanceSum = 0f;
			routeHoldTelemetryClosestDistance = float.MaxValue;
			routeHoldTelemetryFarthestDistance = 0f;
		}

		private void StartRouteHoldTelemetry()
		{
			ResetRouteHoldTelemetryState();
			routeHoldTelemetryActive = stageAdvanceRouteGuidanceActive
				&& (Object)(object)activeStageAdvanceRouteMarker != (Object)null
				&& (Object)(object)playerTransform != (Object)null;
			if (routeHoldTelemetryActive)
			{
				SampleRouteHoldTelemetry(0f);
			}
		}

		private void UpdateRouteHoldTelemetry(float deltaTime)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!routeHoldTelemetryActive || routeHoldBonusGranted || !stageRunning || levelUpOpen || stagePaused)
			{
				return;
			}
			SampleRouteHoldTelemetry(deltaTime);
#endif
		}

		private void SampleRouteHoldTelemetry(float deltaTime)
		{
			if ((Object)(object)activeStageAdvanceRouteMarker == (Object)null || (Object)(object)playerTransform == (Object)null)
			{
				return;
			}
			Vector3 target = activeStageAdvanceRouteMarker.position;
			Vector3 player = playerTransform.position;
			float dx = target.x - player.x;
			float dz = target.z - player.z;
			float distance = Mathf.Sqrt(dx * dx + dz * dz);
			routeHoldTelemetrySamples++;
			routeHoldTelemetryElapsed += Mathf.Max(0f, deltaTime);
			routeHoldTelemetryDistanceSum += distance;
			routeHoldTelemetryClosestDistance = Mathf.Min(routeHoldTelemetryClosestDistance, distance);
			routeHoldTelemetryFarthestDistance = Mathf.Max(routeHoldTelemetryFarthestDistance, distance);
			float readableRouteRange = Mathf.Max(stageAdvanceRouteRewardDistance, routeHoldTrailMaxDistance);
			if (distance <= readableRouteRange)
			{
				routeHoldTelemetryInTrailRangeSamples++;
			}
		}

		private string FormatRouteHoldTelemetrySnapshot()
		{
			if (routeHoldTelemetrySamples <= 0)
			{
				return "routeSamples=0 routeClosest=n/a routeAvg=n/a routeFarthest=n/a routeInRange=n/a routeElapsed=0s";
			}
			float averageDistance = routeHoldTelemetryDistanceSum / Mathf.Max(1, routeHoldTelemetrySamples);
			float inRangePercent = routeHoldTelemetryInTrailRangeSamples / (float)Mathf.Max(1, routeHoldTelemetrySamples) * 100f;
			return $"routeSamples={routeHoldTelemetrySamples:0} routeClosest={routeHoldTelemetryClosestDistance:0.#}m routeAvg={averageDistance:0.#}m routeFarthest={routeHoldTelemetryFarthestDistance:0.#}m routeInRange={inRangePercent:0}% routeElapsed={routeHoldTelemetryElapsed:0.#}s";
		}
	}
}
