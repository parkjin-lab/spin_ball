using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void EmitPlaytestTelemetry(string eventName, string detail = "")
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!enablePlaytestTelemetry)
			{
				return;
			}
			string suffix = string.IsNullOrWhiteSpace(detail) ? string.Empty : $" {detail}";
			Debug.Log((object)$"[AlienCrusher][Playtest] stage={Mathf.Max(1, currentStageNumber):00} event={eventName}{suffix}");
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
				$"destroyed={Mathf.Max(0, destroyedCount):0} laneBreak={GetEarlyCrushLaneBreakTarget():0} routeTarget={GetRouteHoldTarget():0} deadlineLeft={GetRouteHoldRemainingSeconds():0.#}s target={FormatPlaytestActiveTarget()}");
#endif
		}

		private void LogPlaytestRouteHoldSuccess(int destroyedCount)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			EmitPlaytestTelemetry(
				"ROUTE_HOLD_CLEAR",
				$"destroyed={Mathf.Max(0, destroyedCount):0} routeTarget={GetRouteHoldTarget():0} rewardScore={Mathf.Max(0, routeHoldBonusScore):0} payoff={GetRouteDistrictPayoffLabel()} next={FormatPlaytestActiveTarget()}");
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
				$"result={resultLabel} reason={GetStageEndReasonLabel().ToUpperInvariant()} bucket={bucket} destroyed={destroyedCount:0}/{Mathf.Max(0, stageTotalDestructibleCount):0} pct={destroyedPercent:0} chain={highestChain:0} laneBreakTier={earlyCrushFlowBonusIndex:0} routeHold={(routeHoldBonusGranted ? "yes" : "no")} routeBonus={(stageAdvanceRouteRewardGranted ? "yes" : "no")} boss={bossLabel} payoff={GetRouteDistrictPayoffLabel()}");
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
	}
}
