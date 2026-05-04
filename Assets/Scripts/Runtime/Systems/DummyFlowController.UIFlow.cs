using System;
using AlienCrusher.Gameplay;
using AlienCrusher.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void UpdateHudInfo()
			{
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0349: Unknown result type (might be due to invalid IL or missing references)
			//IL_034b: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)hudInfoText == (Object)null)
			{
				return;
			}
			int num = (((Object)(object)scoreSystem != (Object)null) ? scoreSystem.CurrentScore : 0);
			float num2 = Mathf.Max(0f, remainingStageTime);
			int num3 = Mathf.FloorToInt(num2 / 60f);
			int num4 = Mathf.FloorToInt(num2 % 60f);
			string text = (overdriveActive ? $"    OVERDRIVE {Mathf.CeilToInt(Mathf.Max(0f, overdriveRemaining)):00}" : string.Empty);
			string formSkillHudToken = GetFormSkillHudToken();
			string text2 = string.Empty;
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				text2 = $"    SNTL {Mathf.RoundToInt(GetStageBossDurabilityRatio() * 100f):000}%";
				if (IsBossBreakWindowActive())
				{
					text2 = $"{text2}    BREAK {Mathf.CeilToInt(stageBossBreakRemaining):00}";
				}
				else if (stageBossPhaseTwoDroneRecoveryRemaining > 0.001f)
				{
					text2 = $"{text2}    DRN {Mathf.CeilToInt(stageBossPhaseTwoDroneRecoveryRemaining):00}";
				}
				else if (stageBossPhaseTwoActive)
				{
					text2 = $"{text2}    P2";
				}
			}
			hudInfoText.text = $"STG {currentStageNumber:00}    TIME {num3:00}:{num4:00}    SCORE {num:000000}    LVL {currentLevel}{text}{formSkillHudToken}{text2}";
			if ((Object)(object)hudChainText != (Object)null)
			{
				int num5 = ((!((Object)(object)scoreSystem != (Object)null)) ? 1 : Mathf.Max(1, scoreSystem.ChainCount));
				if (num5 != lastChainValue)
				{
					chainPulse = 1f;
					lastChainValue = num5;
				}
				chainPulse = Mathf.MoveTowards(chainPulse, 0f, Time.deltaTime * 3.5f);
				if (comboRushAnnouncementRemaining > 0f)
				{
					GetComboAnnouncementVisualProfile(comboRushAnnouncementText, out Color colorA, out Color colorB, out float baseScale, out float scaleAmplitude, out float scaleFrequency, out float colorFrequency);
					float num6 = baseScale + Mathf.Sin(Time.time * scaleFrequency) * scaleAmplitude;
					float num7 = Mathf.PingPong(Time.time * colorFrequency, 1f);
					hudChainText.text = comboRushAnnouncementText;
					hudChainText.color = Color.Lerp(colorA, colorB, num7);
					((Transform)hudChainText.rectTransform).localScale = new Vector3(num6, num6, 1f);
				}
				else if (trafficPanicChainStack > 0 && trafficPanicChainRemaining > 0.001f)
				{
					int num8 = Mathf.CeilToInt(Mathf.Max(0f, trafficPanicChainRemaining));
					float num9 = Mathf.PingPong(Time.time * 6.3f, 1f);
					float num10 = GetLateHudWeight();
					hudChainText.text = $"PANIC CHAIN x{trafficPanicChainStack} ({num8:00}s)";
					hudChainText.color = Color.Lerp(Color.Lerp(new Color(1f, 0.86f, 0.34f, 1f), new Color(1f, 0.95f, 0.82f, 1f), num10 * 0.22f), Color.Lerp(new Color(1f, 0.42f, 0.12f, 1f), new Color(0.96f, 0.22f, 0.08f, 1f), num10 * 0.3f), num9);
					float num11 = 1.08f + 0.04f * num10 + Mathf.Sin(Time.time * Mathf.Lerp(14f, 11.5f, num10)) * (0.05f + 0.015f * num10);
					((Transform)hudChainText.rectTransform).localScale = new Vector3(num11, num11, 1f);
				}
				else if (overdriveActive)
				{
					float num11 = Mathf.PingPong(Time.time * 5.5f, 1f);
					float num12 = GetLateHudWeight();
					hudChainText.text = $"OVERDRIVE x{num5}";
					hudChainText.color = Color.Lerp(Color.Lerp(new Color(1f, 0.6f, 0.2f, 1f), new Color(1f, 0.82f, 0.54f, 1f), num12 * 0.24f), Color.Lerp(new Color(1f, 0.22f, 0.08f, 1f), new Color(0.86f, 0.16f, 0.06f, 1f), num12 * 0.32f), num11);
					float num13 = 1.16f + 0.05f * num12 + Mathf.Sin(Time.time * Mathf.Lerp(18f, 14f, num12)) * (0.06f + 0.015f * num12);
					((Transform)hudChainText.rectTransform).localScale = new Vector3(num13, num13, 1f);
				}
				else
				{
					bool flag = (Object)(object)scoreSystem != (Object)null && scoreSystem.HasActiveChainTimer && num5 > 1;
					float num16 = flag ? scoreSystem.ChainTimerRatio : 1f;
					hudChainText.text = flag ? $"CHAIN x{num5}  {scoreSystem.ChainTimerRemaining:0.0}s" : $"CHAIN x{num5}";
					float num13 = Mathf.Clamp01((float)(num5 - 1) / 8f);
					float num14 = GetLateHudWeight();
					Color val = default(Color);
					val = Color.Lerp(new Color(1f, 0.8f, 0.3f, 1f), new Color(1f, 0.9f, 0.62f, 1f), num14 * 0.2f);
					Color val2 = default(Color);
					val2 = Color.Lerp(new Color(1f, 0.42f, 0.16f, 1f), new Color(0.92f, 0.28f, 0.12f, 1f), num14 * 0.28f);
					Color val3 = Color.Lerp(val, val2, num13);
					if (flag && num16 < 0.36f)
					{
						val3 = Color.Lerp(val3, new Color(1f, 0.18f, 0.08f, 1f), (1f - num16 / 0.36f) * Mathf.PingPong(Time.time * 8.2f, 1f));
					}
					hudChainText.color = val3;
					float num15 = 1f + chainPulse * (0.22f + 0.04f * num14) + (flag && num16 < 0.3f ? Mathf.PingPong(Time.time * 7.4f, 1f) * 0.045f : 0f);
					((Transform)hudChainText.rectTransform).localScale = new Vector3(num15, num15, 1f);
				}
			}
			UpdateHudGuidanceTexts();
			UpdateStageGoalGaugeUi();
			UpdateStageRouteGuidanceVisuals();
		}

		private void GetComboAnnouncementVisualProfile(string announcement, out Color colorA, out Color colorB, out float baseScale, out float scaleAmplitude, out float scaleFrequency, out float colorFrequency)
		{
			colorA = new Color(1f, 0.92f, 0.4f, 1f);
			colorB = new Color(1f, 0.48f, 0.16f, 1f);
			baseScale = 1.12f;
			scaleAmplitude = 0.06f;
			scaleFrequency = 18f;
			colorFrequency = 8f;
			if (string.IsNullOrEmpty(announcement))
			{
				return;
			}
			if (IsBossAnnouncement(announcement))
			{
				colorA = new Color(1f, 0.84f, 0.46f, 1f);
				colorB = new Color(1f, 0.34f, 0.16f, 1f);
				baseScale = 1.08f;
				scaleAmplitude = 0.045f;
				scaleFrequency = 15f;
				colorFrequency = 6.2f;
				return;
			}
			if (announcement.Contains("TOTAL DESTRUCTION", StringComparison.Ordinal) || announcement.Contains("FORWARD SMASH", StringComparison.Ordinal))
			{
				colorA = new Color(1f, 0.96f, 0.62f, 1f);
				colorB = new Color(1f, 0.56f, 0.18f, 1f);
				baseScale = 1.18f;
				scaleAmplitude = 0.075f;
				scaleFrequency = 20f;
				colorFrequency = 8.8f;
				return;
			}
			if (announcement.Contains("STAGE", StringComparison.Ordinal) || announcement.Contains("ROUTE", StringComparison.Ordinal) || announcement.Contains("FINAL PUSH", StringComparison.Ordinal) || announcement.Contains("CITY", StringComparison.Ordinal))
			{
				colorA = new Color(0.76f, 0.94f, 1f, 1f);
				colorB = new Color(0.36f, 0.74f, 1f, 1f);
				baseScale = 1.06f;
				scaleAmplitude = 0.035f;
				scaleFrequency = 12f;
				colorFrequency = 5.4f;
			}
			float num = GetLateHudWeight();
			if (num > 0f)
			{
				colorA = Color.Lerp(colorA, new Color(1f, 0.92f, 0.72f, 1f), num * 0.28f);
				colorB = Color.Lerp(colorB, new Color(1f, 0.3f, 0.1f, 1f), num * 0.34f);
				baseScale += 0.04f * num;
				scaleAmplitude += 0.015f * num;
				scaleFrequency = Mathf.Lerp(scaleFrequency, scaleFrequency * 0.84f, num);
				colorFrequency = Mathf.Lerp(colorFrequency, colorFrequency * 0.78f, num);
			}
		}

		private float GetLateHudWeight()
		{
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				return 1f;
			}
			int num = Mathf.Max(1, currentStageNumber);
			if (num >= 6)
			{
				return 0.8f;
			}
			if (num >= 5)
			{
				return 0.5f;
			}
			return 0f;
		}

		private bool IsBossAnnouncement(string announcement)
		{
			return announcement.Contains("SENTINEL", StringComparison.Ordinal)
				|| announcement.Contains("CORE", StringComparison.Ordinal)
				|| announcement.Contains("DRONE", StringComparison.Ordinal)
				|| announcement.Contains("PHASE", StringComparison.Ordinal)
				|| announcement.Contains("SHIELD", StringComparison.Ordinal)
				|| announcement.Contains("PYLON", StringComparison.Ordinal)
				|| announcement.Contains("BREAK", StringComparison.Ordinal);
		}

		private static int GetHudHintPriority(bool levelUpOpenState, bool hasUrgencyHint, bool stageStartHintActive, bool bossAlive, bool panicChainActive, bool overdriveActiveState, bool stripMissionActive, bool stripMissionCompletedState, bool routeActive)
		{
			if (levelUpOpenState)
			{
				return 5;
			}
			if (hasUrgencyHint)
			{
				return 4;
			}
			if (stageStartHintActive || bossAlive)
			{
				return 3;
			}
			if (panicChainActive || overdriveActiveState || stripMissionActive || stripMissionCompletedState || routeActive)
			{
				return 2;
			}
			return 1;
		}

		private string ResolveStableHudHint(string desiredHint, int desiredPriority, bool immediatePriority)
		{
			if (string.IsNullOrEmpty(desiredHint))
			{
				lastStableHudHint = string.Empty;
				hudHintHoldRemaining = 0f;
				hudHintPriority = -1;
				return string.Empty;
			}
			if (immediatePriority || string.IsNullOrEmpty(lastStableHudHint))
			{
				lastStableHudHint = desiredHint;
				hudHintHoldRemaining = immediatePriority ? 0.22f : 0.3f;
				hudHintPriority = desiredPriority;
				return desiredHint;
			}
			hudHintHoldRemaining = Mathf.Max(0f, hudHintHoldRemaining - Time.deltaTime);
			bool changed = !string.Equals(lastStableHudHint, desiredHint, StringComparison.Ordinal);
			bool higherPriority = desiredPriority > hudHintPriority;
			if (higherPriority)
			{
				lastStableHudHint = desiredHint;
				hudHintHoldRemaining = 0.24f;
				hudHintPriority = desiredPriority;
				return desiredHint;
			}
			if (changed && hudHintHoldRemaining > 0f && desiredPriority <= hudHintPriority)
			{
				return lastStableHudHint;
			}
			lastStableHudHint = desiredHint;
			hudHintHoldRemaining = changed ? 0.42f : 0.2f;
			hudHintPriority = desiredPriority;
			return desiredHint;
		}

		private static string CompactHudHint(string hint)
		{
			if (string.IsNullOrEmpty(hint))
			{
				return string.Empty;
			}
			if (hint.StartsWith("BREAK WINDOW OPEN:", StringComparison.Ordinal))
			{
				return hint.Replace("BREAK WINDOW OPEN:", "BREAK WINDOW  /", StringComparison.Ordinal).Replace("unload now", "dump damage", StringComparison.Ordinal);
			}
			if (hint.StartsWith("DRONES DOWN:", StringComparison.Ordinal))
			{
				return hint.Replace("DRONES DOWN:", "DRONES DOWN  /", StringComparison.Ordinal).Replace("core pressure window", "push core", StringComparison.Ordinal);
			}
			if (hint.StartsWith("BOSS BLOCKING GATE:", StringComparison.Ordinal))
			{
				return hint.Replace("BOSS BLOCKING GATE:", "BLOCKED  /", StringComparison.Ordinal).Replace("destroy", "break", StringComparison.Ordinal);
			}
			if (hint.StartsWith("TIME CRITICAL:", StringComparison.Ordinal))
			{
				return hint.Replace("TIME CRITICAL:", "TIME CRITICAL  /", StringComparison.Ordinal).Replace("need", "need", StringComparison.Ordinal).Replace("more wrecks in", "more in", StringComparison.Ordinal);
			}
			if (hint.StartsWith("PACE TOO LOW:", StringComparison.Ordinal))
			{
				return hint.Replace("PACE TOO LOW:", "PACE LOW  /", StringComparison.Ordinal).Replace("cut through small blocks,", "cut small blocks,", StringComparison.Ordinal).Replace("wrecks still needed", "left", StringComparison.Ordinal);
			}
			if (hint.StartsWith("FALLING BEHIND:", StringComparison.Ordinal))
			{
				return "FALLING BEHIND  /  enter a denser lane";
			}
			if (hint.StartsWith("OPENING MISSED:", StringComparison.Ordinal))
			{
				return hint.Replace("OPENING MISSED:", "OPENING MISSED  /", StringComparison.Ordinal).Replace("find dense low-rise lane", "recover in dense lane", StringComparison.Ordinal);
			}
			if (hint.StartsWith("FINAL SECONDS:", StringComparison.Ordinal))
			{
				return "FINAL SECONDS  /  hold momentum on route";
			}
			if (hint.StartsWith("JUSTICE SENTINEL SHIELDED:", StringComparison.Ordinal))
			{
				return hint.Replace("JUSTICE SENTINEL SHIELDED:", "SENTINEL SHIELDED  /", StringComparison.Ordinal).Replace("destroy", "break", StringComparison.Ordinal).Replace("to expose the core", "for core", StringComparison.Ordinal);
			}
			if (hint.StartsWith("JUSTICE SENTINEL PHASE 2:", StringComparison.Ordinal))
			{
				return hint.Replace("JUSTICE SENTINEL PHASE 2:", "PHASE 2  /", StringComparison.Ordinal).Replace("drone swarm returning in", "drones back in", StringComparison.Ordinal).Replace("drones down, push damage for", "push core for", StringComparison.Ordinal).Replace("break drones to open a safe push window", "break drones for core window", StringComparison.Ordinal);
			}
			if (hint.StartsWith("JUSTICE SENTINEL FURY:", StringComparison.Ordinal))
			{
				return "FURY  /  dodge burst and hit weak point";
			}
			if (hint.StartsWith("JUSTICE SENTINEL LOCKED:", StringComparison.Ordinal))
			{
				return "LOCKED  /  circle wide then breach weak point";
			}
			if (hint.StartsWith("JUSTICE SENTINEL SCANNING:", StringComparison.Ordinal))
			{
				return "SCANNING  /  build speed then breach weak point";
			}
			return hint;
		}

		private void UpdateHudGuidanceTexts()
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			int num = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
			int num2 = Mathf.Max(1, stageTotalDestructibleCount);
			float num3 = Mathf.Clamp01((float)num / (float)num2);
			bool flag = IsStageBossAlive();
			string hudUrgencyHint = GetHudUrgencyHint(num, flag);
			bool flag2 = !string.IsNullOrEmpty(hudUrgencyHint);
			bool flag3a = IsEarlyOnboardingWindow(num);
			bool openingMissed = HasMissedEarlyCrushLaneBreak(num);
			bool routeOpenBeatActive = IsRouteOpenBeatActive();
			hudUrgencyPulseCooldownRemaining = Mathf.Max(0f, hudUrgencyPulseCooldownRemaining - Time.deltaTime);
			if (flag2)
			{
				TryPlayHudUrgencyPulse(hudUrgencyHint, flag);
			}
			else
			{
				lastHudUrgencyHint = string.Empty;
			}
			if ((Object)(object)hudObjectiveText != (Object)null)
			{
				string text = (flag ? "NEXT STEP\nDestroy city and defeat Justice Sentinel" : "NEXT STEP\nCrush city structures");
				if (!flag && flag3a && GetEarlyCrushBonusTier(num) < 3)
				{
					text = $"NEXT STEP\nCrush starter lane\nLANE BREAK {Mathf.Min(num, GetEarlyCrushLaneBreakTarget()):0}/{GetEarlyCrushLaneBreakTarget():0}";
				}
				else if (!flag && IsRouteHoldObjectiveActive(num))
				{
					int routeHoldTarget = GetRouteHoldTarget();
					text = $"NEXT STEP\n{(routeOpenBeatActive ? "ROUTE OPEN" : "Hold the route")}\nROUTE HOLD {Mathf.Min(num, routeHoldTarget):0}/{routeHoldTarget:0}  {Mathf.CeilToInt(GetRouteHoldRemainingSeconds()):0}s";
				}
				else if (!flag && routeOpenBeatActive)
				{
					text = "NEXT STEP\nRoute opened\nFollow beacon";
				}
				else if (!flag && openingMissed)
				{
					text = $"NEXT STEP\nRecover momentum\nFind dense low-rise lane";
				}
				if (enableStageAdvanceGoal && stageAdvanceDestroyTarget > 0)
				{
					text = $"{text}\nSTAGE TARGET {Mathf.Max(0, num):0}/{stageAdvanceDestroyTarget:0}";
				}
				if (HasActiveStripClearMission())
				{
					string text2 = stripClearMissionCompleted ? "COMPLETE" : $"{stageStripClearCount:0}/{stageStripClearTarget:0}";
					text = $"{text}\nMISSION\nStrip Clear {text2}";
				}
				hudObjectiveText.text = text;
				Color val = default(Color);
				val = new Color(0.72f, 0.82f, 0.98f, 1f);
				Color val2 = (flag ? new Color(1f, 0.74f, 0.38f, 1f) : new Color(1f, 0.86f, 0.52f, 1f));
				Color val3 = Color.Lerp(val, val2, num3);
				if (flag2)
				{
					val3 = Color.Lerp(val3, new Color(1f, 0.62f, 0.3f, 1f), Mathf.PingPong(Time.time * 4.8f, 1f) * 0.75f);
				}
				else if (routeOpenBeatActive)
				{
					val3 = Color.Lerp(val3, Color.white, Mathf.PingPong(Time.time * 5.4f, 1f) * 0.55f);
				}
				hudObjectiveText.color = val3;
			}
			if ((Object)(object)hudProgressText != (Object)null)
			{
				hudProgressText.text = flag2 ? $"DESTRUCTION {num:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}  ({Mathf.RoundToInt(num3 * 100f)}%)  /  NEXT STEP" : $"DESTRUCTION {num:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}  ({Mathf.RoundToInt(num3 * 100f)}%)";
				hudProgressText.color = flag2 ? Color.Lerp(Color.white, new Color(1f, 0.72f, 0.36f, 1f), Mathf.PingPong(Time.time * 5.6f, 1f)) : Color.white;
			}
			UpdateHudBossStatus(flag);
			if ((Object)(object)hudHintText != (Object)null)
			{
				bool flag3 = trafficPanicChainStack > 0 && trafficPanicChainRemaining > 0.001f;
				bool flag4 = HasActiveStripClearMission() && stageStripClearCount < stageStripClearTarget;
				bool flag5 = stageAdvanceRouteGuidanceActive && (Object)(object)activeStageAdvanceRouteMarker != (Object)null;
				string text4;
				if (levelUpOpen)
				{
					text4 = "NEXT STEP  /  Pick one upgrade";
				}
				else if (flag2)
				{
					text4 = hudUrgencyHint;
				}
				else if (stageStartHintRemaining > 0f && flag3a)
				{
					text4 = GetStageStartHint();
				}
				else if (flag)
				{
					text4 = GetBossTelemetryHint();
				}
				else if (flag3)
				{
					text4 = $"NEXT STEP  /  Keep PANIC CHAIN x{trafficPanicChainStack} alive";
				}
				else if (routeOpenBeatActive)
				{
					text4 = "ROUTE OPEN  /  Follow beacon, keep crushing";
				}
				else if (IsRouteHoldObjectiveActive(num))
				{
					int routeHoldTarget = GetRouteHoldTarget();
					text4 = $"NEXT STEP  /  ROUTE HOLD {Mathf.Min(num, routeHoldTarget):0}/{routeHoldTarget:0}, {Mathf.CeilToInt(GetRouteHoldRemainingSeconds()):0}s left";
				}
				else if (overdriveActive)
				{
					text4 = "NEXT STEP  /  Keep the chain alive during OVERDRIVE";
				}
				else if (flag4)
				{
					text4 = $"NEXT STEP  /  STRIP CLEAR {stageStripClearCount}/{stageStripClearTarget}";
				}
				else if (stripClearMissionCompleted)
				{
					text4 = "MISSION COMPLETE  /  Bonus score and DP secured";
				}
				else if (flag5)
				{
					text4 = "NEXT STEP  /  Follow the route beacon and keep crushing forward";
				}
				else
				{
					text4 = "NEXT STEP  /  Build chain for stronger bursts";
				}
				text4 = CompactHudHint(text4);
				int num4 = GetHudHintPriority(levelUpOpen, flag2, stageStartHintRemaining > 0f && flag3a, flag, flag3, overdriveActive, flag4, stripClearMissionCompleted, flag5);
				text4 = ResolveStableHudHint(text4, num4, flag2 || levelUpOpen || routeOpenBeatActive);
				hudHintText.text = text4;
				hudHintText.color = flag2
					? Color.Lerp(new Color(1f, 0.95f, 0.68f, 1f), new Color(1f, 0.52f, 0.22f, 1f), Mathf.PingPong(Time.time * 5.2f, 1f))
					: (routeOpenBeatActive ? Color.Lerp(new Color(0.72f, 1f, 0.9f, 1f), Color.white, Mathf.PingPong(Time.time * 5.7f, 1f) * 0.38f) : Color.white);
			}
		}

		private bool IsEarlyOnboardingWindow(int destroyedCount)
		{
			if (!stageRunning || stageAdvanceGoalReached)
			{
				return false;
			}
			int num = Mathf.Max(1, stageAdvanceDestroyTarget);
			if (destroyedCount >= Mathf.Max(6, Mathf.RoundToInt((float)num * 0.45f)))
			{
				return false;
			}
			float num2 = (((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f);
			float num3 = Mathf.Max(0f, num2 - remainingStageTime);
			return num3 <= GetEarlyCrushFlowWindowSeconds();
		}

		private float GetStageElapsedSeconds()
		{
			float num = (((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f);
			return Mathf.Max(0f, num - remainingStageTime);
		}

		private float GetRouteHoldDeadlineSeconds()
		{
			return Mathf.Max(GetEarlyCrushFlowWindowSeconds(), routeHoldWindowSeconds);
		}

		private float GetRouteHoldRemainingSeconds()
		{
			return Mathf.Max(0f, GetRouteHoldDeadlineSeconds() - GetStageElapsedSeconds());
		}

		private int GetRouteHoldTarget()
		{
			if (stageAdvanceDestroyTarget <= 0)
			{
				return GetEarlyCrushLaneBreakTarget();
			}
			return Mathf.Max(GetEarlyCrushLaneBreakTarget(), Mathf.CeilToInt((float)stageAdvanceDestroyTarget * Mathf.Clamp(routeHoldProgressThreshold, 0.25f, 0.75f)));
		}

		private bool IsRouteHoldObjectiveActive(int destroyedCount)
		{
			return stageRunning && !stageAdvanceGoalReached && stageAdvanceDestroyTarget > 0 && earlyCrushFlowBonusIndex >= 3 && !routeHoldBonusGranted && !earlyCrushRecoveryBonusGranted && GetRouteHoldRemainingSeconds() > 0.001f && destroyedCount < GetRouteHoldTarget();
		}

		private bool HasMissedEarlyCrushLaneBreak(int destroyedCount)
		{
			if (!stageRunning || stageAdvanceGoalReached || earlyCrushRecoveryBonusGranted || destroyedCount >= GetEarlyCrushLaneBreakTarget())
			{
				return false;
			}
			float num = (((Object)(object)gameFlowSystem != (Object)null) ? gameFlowSystem.StageDurationSeconds : 90f);
			float num2 = Mathf.Max(0f, num - remainingStageTime);
			float num3 = GetEarlyCrushFlowWindowSeconds();
			return num2 > num3 && num2 <= num3 + Mathf.Max(0f, earlyCrushRecoveryWindowSeconds);
		}

		private bool IsLatePressureWindow(int destroyedCount, bool bossAlive)
		{
			if (!stageRunning || stageAdvanceGoalReached)
			{
				return false;
			}
			if (bossAlive)
			{
				return true;
			}
			if (enableStageAdvanceGoal && stageAdvanceDestroyTarget > 0)
			{
				float num = Mathf.Clamp01((float)Mathf.Max(0, destroyedCount) / (float)Mathf.Max(1, stageAdvanceDestroyTarget));
				if (num >= Mathf.Clamp(stageAdvanceNearCompleteThreshold, 0.6f, 0.98f))
				{
					return true;
				}
			}
			return remainingStageTime <= 18f;
		}

		private bool IsBossApproachWindow()
		{
			if (!stageRunning || (Object)(object)playerTransform == (Object)null)
			{
				return false;
			}
			if (stageBossEncounterActive && IsStageBossAlive())
			{
				return true;
			}
			if (currentStageNumber < Mathf.Max(1, bossStageStart - 1))
			{
				return false;
			}
			Vector3 position = playerTransform.position;
			return position.z > 14.5f && Mathf.Abs(position.x) < 5.2f;
		}

		private string GetStageStartHint()
		{
			if (stageBossEncounterActive)
			{
				return "NEXT STEP  /  Build speed, break pylons, burst core on BREAK";
			}
			int num = Mathf.Max(1, currentStageNumber);
			if (num <= 2)
			{
				return $"NEXT STEP  /  Crush starter lane to {GetEarlyCrushLaneBreakTarget():0} wrecks for LANE BREAK";
			}
			if (num <= 4)
			{
				return "NEXT STEP  /  Hit storefront lanes first, then fold into mid-rise blocks";
			}
			if (num <= 6)
			{
				return "NEXT STEP  /  Crack dense mid-rise lanes before towers slow you down";
			}
			return "NEXT STEP  /  Keep to fortified lanes, protect speed, then breach hard targets";
		}

		private void UpdateHudBossStatus(bool bossAlive)
		{
			if ((Object)(object)hudBossStatusText == (Object)null)
			{
				return;
			}
			if (!stageBossEncounterActive)
			{
				hudBossStatusText.text = string.Empty;
				return;
			}
			if (!bossAlive)
			{
				hudBossStatusText.text = "SENTINEL\nDOWN";
				hudBossStatusText.color = new Color(0.82f, 1f, 0.8f, 1f);
				return;
			}
			string text = $"SENTINEL  {Mathf.RoundToInt(GetStageBossDurabilityRatio() * 100f):0}%\nTHREAT  {GetBossThreatLabel()}";
			if (IsBossBreakWindowActive())
			{
				text = $"{text}\nCORE OPEN  {Mathf.CeilToInt(stageBossBreakRemaining):0}s";
			}
			else if (stageBossShieldActiveCount > 0)
			{
				text = $"{text}\nPYLONS  {stageBossShieldActiveCount:0}  /  REGEN";
			}
			else if (stageBossPhaseTwoDroneRecoveryRemaining > 0.001f)
			{
				text = $"{text}\nDRONES DOWN  {Mathf.CeilToInt(stageBossPhaseTwoDroneRecoveryRemaining):0}s";
			}
			else if (stageBossPhaseTwoActive)
			{
				text = $"{text}\nPHASE 2 ACTIVE";
			}
			hudBossStatusText.text = text;
			bool flag = IsLatePressureWindow((((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0), bossAlive);
			bool flag2 = IsBossApproachWindow();
			hudBossStatusText.color = IsBossBreakWindowActive()
				? Color.Lerp(new Color(1f, 0.84f, 0.52f, 1f), Color.white, Mathf.PingPong(Time.time * 5.4f, 1f) * 0.35f)
				: ((flag || flag2)
					? Color.Lerp(new Color(1f, 0.8f, 0.58f, 1f), new Color(1f, 0.96f, 0.84f, 1f), Mathf.PingPong(Time.time * (flag2 ? 4.2f : 3.8f), 1f) * (flag2 ? 0.3f : 0.22f))
					: new Color(1f, 0.82f, 0.62f, 1f));
		}

		private string GetHudUrgencyHint(int destroyedCount, bool bossAlive)
		{
			if (!stageRunning || levelUpOpen || stagePaused)
			{
				return string.Empty;
			}
			if (bossAlive)
			{
				if (IsBossBreakWindowActive())
				{
					return $"BREAK WINDOW OPEN: unload now ({Mathf.CeilToInt(stageBossBreakRemaining):0}s)";
				}
				if (stageBossPhaseTwoDroneRecoveryRemaining > 0.001f)
				{
					return $"DRONES DOWN: core pressure window ({Mathf.CeilToInt(stageBossPhaseTwoDroneRecoveryRemaining):0}s)";
				}
				if (stageBossShieldActiveCount > 0)
				{
					return $"BOSS BLOCKING GATE: destroy {stageBossShieldActiveCount:0} shield pylons";
				}
			}
			if (IsBossApproachWindow() && currentStageNumber >= bossStageStart)
			{
				return "SENTINEL AHEAD: hold center lane and keep speed";
			}
			if (!bossAlive && IsRouteHoldObjectiveActive(destroyedCount) && GetRouteHoldRemainingSeconds() <= 10f)
			{
				int routeHoldTarget = GetRouteHoldTarget();
				return $"ROUTE HOLD CLOSING: need {Mathf.Max(0, routeHoldTarget - destroyedCount):0} wrecks in {Mathf.CeilToInt(GetRouteHoldRemainingSeconds()):0}s";
			}
			if (!bossAlive && HasMissedEarlyCrushLaneBreak(destroyedCount))
			{
				return $"OPENING MISSED: find dense low-rise lane ({Mathf.Min(destroyedCount, GetEarlyCrushLaneBreakTarget()):0}/{GetEarlyCrushLaneBreakTarget():0})";
			}
			if (enableStageAdvanceGoal && stageAdvanceDestroyTarget > 0 && !stageAdvanceGoalReached)
			{
				int num = Mathf.Max(0, stageAdvanceDestroyTarget - destroyedCount);
				float num2 = Mathf.Max(0f, remainingStageTime);
				float num3 = (num2 <= 0.01f) ? (float)stageAdvanceDestroyTarget : (float)num / Mathf.Max(1f, num2);
				if (num2 <= 18f && num > 0)
				{
					return $"TIME CRITICAL: need {num:0} more wrecks in {Mathf.CeilToInt(num2):0}s";
				}
				if (num2 <= 30f && num3 > 0.6f)
				{
					return $"PACE TOO LOW: cut through small blocks, {num:0} wrecks still needed";
				}
				if (num > 0 && destroyedCount < Mathf.RoundToInt(stageAdvanceDestroyTarget * 0.45f) && num2 <= 45f)
				{
					return "FALLING BEHIND: ignore isolated targets and enter a denser lane";
				}
			}
			if (trafficPanicChainStack <= 0 && !overdriveActive && remainingStageTime <= 16f)
			{
				return "FINAL SECONDS: protect momentum and commit to the goal route";
			}
			return string.Empty;
		}

		private void TryPlayHudUrgencyPulse(string urgencyHint, bool bossRelated)
		{
			if (string.IsNullOrEmpty(urgencyHint))
			{
				return;
			}
			bool flag = !string.Equals(lastHudUrgencyHint, urgencyHint, StringComparison.Ordinal);
			if (!flag && hudUrgencyPulseCooldownRemaining > 0f)
			{
				return;
			}
			float num = 0.62f;
			float num2 = flag ? 0.55f : 1.5f;
			if (urgencyHint.Contains("BREAK WINDOW", StringComparison.Ordinal) || urgencyHint.Contains("TIME CRITICAL", StringComparison.Ordinal) || urgencyHint.Contains("ROUTE HOLD CLOSING", StringComparison.Ordinal))
			{
				num = 1f;
				num2 = flag ? 0.3f : 0.9f;
			}
			else if (urgencyHint.Contains("FALLING BEHIND", StringComparison.Ordinal) || urgencyHint.Contains("PACE TOO LOW", StringComparison.Ordinal) || urgencyHint.Contains("OPENING MISSED", StringComparison.Ordinal))
			{
				num = 0.84f;
				num2 = flag ? 0.45f : 1.15f;
			}
			else if (urgencyHint.Contains("FINAL SECONDS", StringComparison.Ordinal) || urgencyHint.Contains("BOSS BLOCKING", StringComparison.Ordinal))
			{
				num = 0.74f;
				num2 = flag ? 0.5f : 1.25f;
			}
			feedbackSystem?.PlayHudWarningFeedback(bossRelated, num);
			hudUrgencyPulseCooldownRemaining = num2;
			lastHudUrgencyHint = urgencyHint;
		}

		private void UpdateStageRouteGuidanceVisuals()
		{
			Transform[] array = new Transform[2] { stageAdvanceRouteMarkerA, stageAdvanceRouteMarkerB };
			int num2 = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
			routeOpenBeatRemaining = Mathf.Max(0f, routeOpenBeatRemaining - Time.deltaTime);
			float routeOpenBeat01 = GetRouteOpenBeat01();
			bool routeOpenBeatActive = IsRouteOpenBeatActive();
			bool flag2 = IsEarlyOnboardingWindow(num2);
			bool flag3 = IsLatePressureWindow(num2, stageBossEncounterActive && IsStageBossAlive());
			bool flag4 = IsBossApproachWindow();
			float num = 1f;
			if (stageAdvanceRouteGuidanceActive && (Object)(object)activeStageAdvanceRouteMarker != (Object)null)
			{
				num = flag2
					? Mathf.Lerp(1f, 1.38f, Mathf.PingPong(Time.time * 3.2f, 1f))
					: ((flag3 || flag4)
						? Mathf.Lerp(1f, flag4 ? 1.3f : 1.24f, Mathf.PingPong(Time.time * (flag4 ? 3.6f : 3f), 1f))
						: Mathf.Lerp(1f, 1.18f, Mathf.PingPong(Time.time * 2.1f, 1f)));
				if (routeOpenBeatActive)
				{
					float openPulse = 1f + (routeOpenBeat01 * (0.28f + Mathf.PingPong(Time.time * 5.6f, 1f) * 0.16f));
					num = Mathf.Max(num, openPulse);
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				Transform val = array[i];
				if ((Object)(object)val == (Object)null)
				{
					continue;
				}
				bool flag = stageAdvanceRouteGuidanceActive && (Object)(object)val == (Object)(object)activeStageAdvanceRouteMarker;
				val.localScale = flag
					? new Vector3((flag2 ? 1.5f : 1.28f) * num, 0.18f, (flag2 ? 1.5f : 1.28f) * num)
					: new Vector3(flag2 ? 1.1f : 1f, 0.12f, flag2 ? 1.1f : 1f);
				GameObject gameObject = ((Component)val).gameObject;
				Renderer component = gameObject.GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					MaterialPropertyBlock val2 = new MaterialPropertyBlock();
					component.GetPropertyBlock(val2);
					Color color = flag
						? Color.Lerp(stageAdvanceRouteMarkerColor, Color.white, Mathf.PingPong(Time.time * (flag2 ? 4.4f : (flag3 ? 3.5f : 2.8f)), 1f) * (flag2 ? 0.35f : (flag3 ? 0.24f : 0.18f)))
						: new Color(stageAdvanceRouteMarkerColor.r, stageAdvanceRouteMarkerColor.g, stageAdvanceRouteMarkerColor.b, flag2 ? 0.28f : ((flag3 || flag4) ? 0.24f : 0.18f));
					if (flag && routeOpenBeatActive)
					{
						color = Color.Lerp(color, Color.white, routeOpenBeat01 * (0.32f + Mathf.PingPong(Time.time * 6f, 1f) * 0.18f));
					}
					if (flag && flag4)
					{
						color = Color.Lerp(color, Color.white, Mathf.PingPong(Time.time * 4.1f, 1f) * 0.14f);
					}
					val2.SetColor("_BaseColor", color);
					val2.SetColor("_Color", color);
					component.SetPropertyBlock(val2);
				}
			}
			UpdateRouteHoldTrail(num2);
			UpdateHudRouteIndicator();
		}


		private void UpdateRouteHoldTrail(int destroyedCount)
		{
			bool active = IsRouteHoldObjectiveActive(destroyedCount) && (Object)(object)activeStageAdvanceRouteMarker != (Object)null && (Object)(object)playerTransform != (Object)null && !levelUpOpen && !stagePaused;
			if (!active)
			{
				SetRouteHoldTrailVisible(false);
				return;
			}
			EnsureRouteHoldTrailPips();
			if (routeHoldTrailPips == null || routeHoldTrailPips.Length == 0)
			{
				return;
			}
			Vector3 start = playerTransform.position;
			Vector3 end = activeStageAdvanceRouteMarker.position;
			start.y = 0.045f;
			end.y = 0.045f;
			Vector3 delta = end - start;
			delta.y = 0f;
			float distance = delta.magnitude;
			if (distance <= Mathf.Max(0.75f, routeHoldTrailCloseHideDistance))
			{
				SetRouteHoldTrailVisible(false);
				return;
			}
			Vector3 direction = delta / distance;
			float visibleDistance = Mathf.Min(distance, Mathf.Max(4f, routeHoldTrailMaxDistance));
			float pulse = Mathf.PingPong(Time.time * 3.8f, 1f);
			float routeOpenBeat01 = IsRouteOpenBeatActive() ? GetRouteOpenBeat01() : 0f;
			int count = routeHoldTrailPips.Length;
			int activeCount = ResolveRouteHoldTrailActivePipCount(visibleDistance, count);
			float distanceScale = Mathf.InverseLerp(Mathf.Max(0.75f, routeHoldTrailCloseHideDistance), Mathf.Max(4f, routeHoldTrailMaxDistance), visibleDistance);
			for (int i = 0; i < count; i++)
			{
				Transform pip = routeHoldTrailPips[i];
				if ((Object)(object)pip == (Object)null)
				{
					continue;
				}
				if (i >= activeCount)
				{
					((Component)pip).gameObject.SetActive(false);
					continue;
				}
				float t = (i + 1f) / (activeCount + 1f);
				Vector3 position = start + direction * (visibleDistance * t);
				position.y = 0.055f + 0.012f * Mathf.Sin((Time.time * 5.2f) + i * 0.85f);
				pip.position = position;
				pip.rotation = Quaternion.LookRotation(direction, Vector3.up);
				float scalePulse = 1f + 0.18f * Mathf.Sin((Time.time * 5.8f) - i * 0.75f);
				float width = Mathf.Lerp(0.2f, 0.28f, distanceScale) + 0.045f * pulse + routeOpenBeat01 * 0.035f;
				float length = Mathf.Lerp(0.4f, 0.55f, distanceScale) + 0.18f * t + routeOpenBeat01 * 0.08f;
				pip.localScale = new Vector3(width, 0.018f, length * scalePulse);
				((Component)pip).gameObject.SetActive(true);
				Renderer renderer = ((Component)pip).GetComponent<Renderer>();
				if ((Object)(object)renderer != (Object)null)
				{
					MaterialPropertyBlock block = new MaterialPropertyBlock();
					renderer.GetPropertyBlock(block);
					Color color = Color.Lerp(routeHoldTrailColor, Color.white, (pulse * 0.18f) + t * 0.08f);
					if (routeOpenBeat01 > 0f)
					{
						color = Color.Lerp(color, Color.white, routeOpenBeat01 * (0.2f + pulse * 0.14f));
					}
					block.SetColor("_BaseColor", color);
					block.SetColor("_Color", color);
					renderer.SetPropertyBlock(block);
				}
			}
		}

		private int ResolveRouteHoldTrailActivePipCount(float visibleDistance, int allocatedCount)
		{
			int maxCount = Mathf.Clamp(allocatedCount, 0, Mathf.Clamp(routeHoldTrailPipCount, 3, 8));
			if (maxCount <= 0)
			{
				return 0;
			}

			float spacing = Mathf.Max(0.75f, routeHoldTrailMinPipSpacing);
			int distanceCount = Mathf.FloorToInt(visibleDistance / spacing);
			return Mathf.Clamp(distanceCount, Mathf.Min(2, maxCount), maxCount);
		}

		private void EnsureRouteHoldTrailPips()
		{
			int count = Mathf.Clamp(routeHoldTrailPipCount, 3, 8);
			if (routeHoldTrailPips != null && routeHoldTrailPips.Length == count && (Object)(object)routeHoldTrailRoot != (Object)null)
			{
				return;
			}
			Transform mapRoot = FindChildByName(null, "MapRoot");
			if ((Object)(object)routeHoldTrailRoot == (Object)null)
			{
				GameObject root = new GameObject("RouteHoldTrail");
				routeHoldTrailRoot = root.transform;
				if ((Object)(object)mapRoot != (Object)null)
				{
					routeHoldTrailRoot.SetParent(mapRoot, false);
				}
			}
			routeHoldTrailPips = new Transform[count];
			for (int i = 0; i < count; i++)
			{
				string name = $"RouteHoldTrailPip_{i:00}";
				Transform existing = FindDirectChild(routeHoldTrailRoot, name);
				GameObject pipObject = (Object)(object)existing != (Object)null ? ((Component)existing).gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
				((Object)pipObject).name = name;
				pipObject.transform.SetParent(routeHoldTrailRoot, false);
				Collider collider = pipObject.GetComponent<Collider>();
				if ((Object)(object)collider != (Object)null)
				{
					collider.enabled = false;
				}
				TintObject(pipObject, routeHoldTrailColor);
				pipObject.SetActive(false);
				routeHoldTrailPips[i] = pipObject.transform;
			}
		}

		private void SetRouteHoldTrailVisible(bool visible)
		{
			if (routeHoldTrailPips == null)
			{
				return;
			}
			for (int i = 0; i < routeHoldTrailPips.Length; i++)
			{
				Transform pip = routeHoldTrailPips[i];
				if ((Object)(object)pip != (Object)null)
				{
					((Component)pip).gameObject.SetActive(visible);
				}
			}
		}
		private void UpdateHudRouteIndicator()
		{
			if ((Object)(object)hudRouteIndicatorText == (Object)null)
			{
				return;
			}
			if (!stageRunning || levelUpOpen || stagePaused)
			{
				hudRouteIndicatorText.text = string.Empty;
				UpdateHudRouteArrow(null, Color.white);
				return;
			}
			Transform val = null;
			string text = string.Empty;
			Color color = new Color(1f, 0.92f, 0.7f, 1f);
			bool routeOpenBeatActive = IsRouteOpenBeatActive();
			if (stageBossEncounterActive && IsStageBossAlive() && (Object)(object)stageBossBlock != (Object)null)
			{
				val = ((Component)stageBossBlock).transform;
				text = "SENTINEL";
				color = new Color(1f, 0.68f, 0.42f, 1f);
			}
			else if (stageAdvanceRouteGuidanceActive && (Object)(object)activeStageAdvanceRouteMarker != (Object)null)
			{
				val = activeStageAdvanceRouteMarker;
				text = routeOpenBeatActive ? "OPEN" : (IsRouteHoldObjectiveActive((((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0)) ? "HOLD" : "ROUTE");
				color = routeOpenBeatActive ? new Color(0.62f, 1f, 0.86f, 1f) : new Color(1f, 0.86f, 0.36f, 1f);
			}
			if ((Object)(object)val == (Object)null || (Object)(object)playerTransform == (Object)null)
			{
				hudRouteIndicatorText.text = string.Empty;
				UpdateHudRouteArrow(null, color);
				return;
			}
			Camera val2 = Camera.main;
			Vector3 position = playerTransform.position;
			Vector3 position2 = val.position;
			Vector3 val3 = position2 - position;
			float num = new Vector2(val3.x, val3.z).magnitude;
			string text2 = GetDirectionLabel(val3, val2);
			if (!string.IsNullOrEmpty(text2))
			{
				hudRouteIndicatorText.text = $"{text} {text2}  {Mathf.RoundToInt(num):0}m";
			}
			else
			{
				hudRouteIndicatorText.text = $"{text}  {Mathf.RoundToInt(num):0}m";
			}
			bool flag2 = IsEarlyOnboardingWindow((((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0));
			bool flag3 = IsLatePressureWindow((((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0), stageBossEncounterActive && IsStageBossAlive());
			bool flag4 = IsBossApproachWindow();
			float num2 = Mathf.PingPong(Time.time * (flag2 ? 4.6f : ((flag3 || flag4) ? 3.5f : 2.4f)), 1f);
			hudRouteIndicatorText.color = Color.Lerp(color, Color.white, num2 * (routeOpenBeatActive ? 0.62f : (flag2 ? 0.35f : ((flag3 || flag4) ? 0.26f : 0.16f))));
			UpdateHudRouteArrow(val, color);
		}

		private static string GetDirectionLabel(Vector3 worldDelta, Camera targetCamera)
		{
			Vector3 val = worldDelta;
			val.y = 0f;
			if (val.sqrMagnitude <= 0.01f)
			{
				return "AHEAD";
			}
			if ((Object)(object)targetCamera != (Object)null)
			{
				Vector3 forward = targetCamera.transform.forward;
				forward.y = 0f;
				if (forward.sqrMagnitude > 0.0001f)
				{
					forward.Normalize();
					Vector3 rhs = targetCamera.transform.right;
					rhs.y = 0f;
					if (rhs.sqrMagnitude > 0.0001f)
					{
						rhs.Normalize();
						float num = Vector3.Dot(rhs, val.normalized);
						float num2 = Vector3.Dot(forward, val.normalized);
						if (num2 >= 0.55f)
						{
							return num >= 0.3f ? "AHEAD-RIGHT" : (num <= -0.3f ? "AHEAD-LEFT" : "AHEAD");
						}
						if (num2 <= -0.35f)
						{
							return num >= 0f ? "TURN RIGHT" : "TURN LEFT";
						}
						return num >= 0f ? "RIGHT" : "LEFT";
					}
				}
			}
			return val.x >= 0f ? "RIGHT" : "LEFT";
		}

		private void UpdateHudRouteArrow(Transform target, Color color)
		{
			if ((Object)(object)hudRouteArrowRect == (Object)null)
			{
				return;
			}
			if (!stageRunning || levelUpOpen || stagePaused || (Object)(object)target == (Object)null)
			{
				((Component)hudRouteArrowRect).gameObject.SetActive(false);
				return;
			}
			Camera val = Camera.main;
			Transform val2 = FindChildByName(canvasRootTransform, hudPanelName);
			RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
			if ((Object)(object)val == (Object)null || (Object)(object)val3 == (Object)null)
			{
				((Component)hudRouteArrowRect).gameObject.SetActive(false);
				return;
			}
			Vector3 vector = val.WorldToViewportPoint(target.position + Vector3.up * 0.8f);
			bool flag = vector.z > 0f && vector.x >= 0.08f && vector.x <= 0.92f && vector.y >= 0.12f && vector.y <= 0.88f;
			if (flag)
			{
				((Component)hudRouteArrowRect).gameObject.SetActive(false);
				return;
			}
			Vector2 vector2 = new Vector2((vector.x - 0.5f) * val3.rect.width, (vector.y - 0.5f) * val3.rect.height);
			if (vector.z < 0f)
			{
				vector2 *= -1f;
			}
			if (vector2.sqrMagnitude < 0.01f)
			{
				vector2 = Vector2.up;
			}
			Vector2 normalized = vector2.normalized;
			float num = Mathf.Max(100f, val3.rect.width * 0.5f - 54f);
			float num2 = Mathf.Max(90f, val3.rect.height * 0.5f - 54f);
			float num3 = Mathf.Min(Mathf.Abs(num / Mathf.Max(0.001f, normalized.x)), Mathf.Abs(num2 / Mathf.Max(0.001f, normalized.y)));
			if (float.IsInfinity(num3) || float.IsNaN(num3))
			{
				num3 = Mathf.Min(num, num2);
			}
			Vector2 anchoredPosition = normalized * Mathf.Min(num3, Mathf.Min(num, num2));
			hudRouteArrowRect.anchoredPosition = anchoredPosition;
			float num4 = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f - 90f;
			hudRouteArrowRect.localRotation = Quaternion.Euler(0f, 0f, num4);
			bool flag2 = IsEarlyOnboardingWindow((((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0));
			bool flag3 = IsLatePressureWindow((((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0), stageBossEncounterActive && IsStageBossAlive());
			bool flag4 = IsBossApproachWindow();
			float routeOpenBeat01 = IsRouteOpenBeatActive() ? GetRouteOpenBeat01() : 0f;
			float num5 = Mathf.PingPong(Time.time * (flag2 ? 4.8f : ((flag3 || flag4) ? 3.4f : 2.3f)), 1f);
			float num6 = flag2 ? 1.08f : ((flag3 || flag4) ? 1.07f : 1.02f);
			float num7 = flag2 ? 0.1f : ((flag3 || flag4) ? 0.07f : 0.035f);
			num6 += routeOpenBeat01 * 0.08f;
			num7 += routeOpenBeat01 * 0.08f;
			hudRouteArrowRect.localScale = Vector3.one * (num6 + num5 * num7);
			((Component)hudRouteArrowRect).gameObject.SetActive(true);
			if ((Object)(object)hudRouteArrowText != (Object)null)
			{
				hudRouteArrowText.color = Color.Lerp(color, Color.white, num5 * (routeOpenBeat01 > 0f ? 0.36f : (flag2 ? 0.26f : ((flag3 || flag4) ? 0.2f : 0.12f))));
			}
		}

		private void UpdateLobbyMissionGuide()
		{
			if (!((Object)(object)lobbyMissionText == (Object)null))
			{
				if (formUnlockSystem == null)
				{
					formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
				}
				string text = GetCurrentSelectedForm().ToString().ToUpperInvariant();
				int num = Mathf.Max(2, bossStageStart);
				int num2 = Mathf.Clamp(eliteWeakPointBaseCount + Mathf.Max(0, currentStageNumber - 1) * Mathf.Max(0, eliteWeakPointPerStage), 0, Mathf.Max(0, eliteWeakPointMaxCount));
				int num3 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.HighestUnlockedStage : Mathf.Max(1, currentStageNumber));
				string text2 = ((currentStageNumber >= num) ? "BOSS PATH  /  Justice Sentinel active" : $"BOSS PATH  /  Justice Sentinel from stage {num:00}");
				string text3 = DescribeStageDistrict(currentStageNumber);
				string text4 = DescribeStageTraffic(currentStageNumber);
				string text5 = DescribeStagePressure(currentStageNumber);
				string text6 = DescribeStageObjective(currentStageNumber);
				string text7 = enableStripClearMission ? $"MISSION BONUS  /  SCORE +{Mathf.Max(0, stripClearMissionCompleteScore):0}  DP +{Mathf.Max(0, stripClearMissionCompleteDp):0}+" : "MISSION BONUS  /  OFF";
				lobbyMissionText.text = $"MISSION BRIEF  /  STAGE {currentStageNumber:00}  /  BEST {num3:00}\nFORM  /  {text}\nNEXT STEP  /  {text6}\nDISTRICT  /  {text3}\nTRAFFIC  /  {text4}\nPRESSURE  /  {text5}\nWEAK POINTS  /  ELITE ONLY ({num2:0})\n{text7}\n{text2}";
				UpdateLobbyRecommendationUi();
				UpdateLobbyStageSelectUi();
			}
		}

		private void UpdateStageGoalGaugeUi()
		{
			if ((Object)(object)hudStageGoalText == (Object)null && (Object)(object)hudStageGoalFillImage == (Object)null)
			{
				return;
			}
			int num = (((Object)(object)scoreSystem != (Object)null) ? Mathf.Max(0, scoreSystem.DestroyedCount) : 0);
			int num2 = Mathf.Max(1, stageAdvanceDestroyTarget);
			float num3 = enableStageAdvanceGoal ? Mathf.Clamp01((float)num / (float)num2) : 0f;
			bool flag = stageBossEncounterActive && IsStageBossAlive();
			bool flag2a = !stageAdvanceGoalReached && num3 >= 0.18f && num3 < 0.45f;
			bool flag2 = !stageAdvanceGoalReached && num3 >= Mathf.Clamp(stageAdvanceNearCompleteThreshold, 0.6f, 0.98f);
			float num4 = flag2
				? Mathf.Lerp(1f, 1.16f, Mathf.PingPong(Time.time * Mathf.Max(1f, stageAdvanceGaugePulseSpeed), 1f))
				: (flag2a ? Mathf.Lerp(1f, 1.08f, Mathf.PingPong(Time.time * Mathf.Max(1f, stageAdvanceGaugePulseSpeed * 0.72f), 1f)) : 1f);
			if ((Object)(object)hudStageGoalFillImage != (Object)null)
			{
				hudStageGoalFillImage.fillAmount = num3;
				hudStageGoalFillImage.color = stageAdvanceGoalReached
					? new Color(1f, 0.78f, 0.3f, 1f)
					: (flag
						? Color.Lerp(new Color(0.34f, 0.76f, 1f, 1f), new Color(1f, 0.48f, 0.18f, 1f), num3)
						: (flag2a
							? Color.Lerp(new Color(0.34f, 0.98f, 0.78f, 1f), new Color(1f, 0.84f, 0.34f, 1f), Mathf.InverseLerp(0.18f, 0.45f, num3))
							: Color.Lerp(new Color(0.3f, 0.94f, 0.74f, 1f), new Color(1f, 0.78f, 0.28f, 1f), num3)));
			}
			if ((Object)(object)hudStageGoalBackgroundImage != (Object)null)
			{
				hudStageGoalBackgroundImage.color = stageAdvanceGoalReached
					? new Color(0.18f, 0.16f, 0.12f, 0.92f)
					: (flag2
						? Color.Lerp(new Color(0.08f, 0.1f, 0.14f, 0.82f), new Color(0.18f, 0.12f, 0.08f, 0.9f), Mathf.PingPong(Time.time * 3.4f, 1f))
						: (flag2a
							? Color.Lerp(new Color(0.08f, 0.12f, 0.1f, 0.82f), new Color(0.12f, 0.16f, 0.1f, 0.88f), Mathf.PingPong(Time.time * 2.8f, 1f))
							: new Color(0.08f, 0.1f, 0.14f, 0.82f)));
				((Transform)hudStageGoalBackgroundImage.rectTransform).localScale = new Vector3(num4, 1f, 1f);
			}
			if ((Object)(object)hudStageGoalFillImage != (Object)null)
			{
				((Transform)hudStageGoalFillImage.rectTransform).localScale = new Vector3(num4, 1f, 1f);
			}
			if ((Object)(object)hudStageGoalText != (Object)null)
			{
				if (!enableStageAdvanceGoal)
				{
					hudStageGoalText.text = "STAGE GOAL OFF";
				}
				else if (stageAdvanceGoalReached)
				{
					hudStageGoalText.text = $"NEXT STAGE READY  {num:0}/{num2:0}";
				}
				else if (flag)
				{
					hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}  /  BOSS BLOCKING";
				}
				else if (flag2)
				{
					hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}  /  FINAL PUSH";
				}
				else if (flag2a)
				{
					hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}  /  GOOD START";
				}
				else
				{
					hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}";
				}
				hudStageGoalText.color = stageAdvanceGoalReached
					? new Color(1f, 0.9f, 0.56f, 1f)
					: (flag2
						? Color.Lerp(Color.white, new Color(1f, 0.86f, 0.4f, 1f), Mathf.PingPong(Time.time * 4.6f, 1f))
						: (flag2a
							? Color.Lerp(Color.white, new Color(0.76f, 1f, 0.78f, 1f), Mathf.PingPong(Time.time * 3.6f, 1f))
							: Color.white));
				((Transform)hudStageGoalText.rectTransform).localScale = new Vector3(num4, num4, 1f);
			}
		}

		private static string DescribeStageDistrict(int stageNumber)
		{
			int num = Mathf.Max(1, stageNumber);
			if (num <= 2)
			{
				return "low-rise suburb / open on tight house rows";
			}
			if (num <= 4)
			{
				return "mixed commercial strip / enter storefront lanes first";
			}
			if (num <= 6)
			{
				return "dense core / crack mid-rise lanes before towers";
			}
			return "fortified skyline / heavy structures hold the route";
		}

		private static string DescribeStageTraffic(int stageNumber)
		{
			int num = Mathf.Max(1, stageNumber);
			if (num <= 2)
			{
				return "light traffic / props are easy speed fuel";
			}
			if (num <= 4)
			{
				return "moderate traffic / cars and props refill lanes";
			}
			if (num <= 6)
			{
				return "dense traffic / explosive utilities can open routes";
			}
			return "high-alert streets / hazard chains can swing tempo";
		}

		private static string DescribeStagePressure(int stageNumber)
		{
			int num = Mathf.Max(1, stageNumber);
			if (num <= 2)
			{
				return "stay in dense low-rise lanes and build chain";
			}
			if (num <= 4)
			{
				return "switch between props and mid-rise blocks without drifting";
			}
			if (num <= 6)
			{
				return "protect combo windows while cracking elites";
			}
			return "plan around boss pressure and fortified fronts";
		}

		private string DescribeStageObjective(int stageNumber)
		{
			if (!enableStripClearMission)
			{
				return "open on low-rise blocks and keep pushing forward";
			}

			int num = Mathf.Max(1, stageNumber);
			int target = GetStripClearMissionTarget();
			if (target <= 0)
			{
				return "open on low-rise blocks and keep pushing forward";
			}

			if (num <= 2)
			{
				return $"open with low-rise crush, then finish {target:0} storefront clear";
			}
			if (num <= 5)
			{
				return $"push storefront lanes and finish {target:0} strip clears";
			}

			return $"finish {target:0} strip clears before fortified pressure closes in";
		}

		private void UpdateLobbyStageSelectUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			Text text = FindText(canvasRootTransform, "StageSelectText");
			if ((Object)(object)text != (Object)null)
			{
				int num = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.HighestUnlockedStage : Mathf.Max(1, currentStageNumber));
				text.text = $"STAGE {currentStageNumber:00} / MAX {num:00}";
			}
			Button button = FindButton("StagePrevButton");
			if (!((Object)(object)button == (Object)null))
			{
				button.interactable = currentStageNumber > 1;
			}
			Button button2 = FindButton("StageNextButton");
			if (!((Object)(object)button2 == (Object)null))
			{
				int num2 = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.HighestUnlockedStage : Mathf.Max(1, currentStageNumber));
				button2.interactable = currentStageNumber < num2;
			}
		}

		private void HandlePreviousLobbyStage()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			currentStageNumber = Mathf.Max(1, currentStageNumber - 1);
			formUnlockSystem?.SetCurrentLobbyStage(currentStageNumber);
			UpdateMetaProgressUi();
			UpdateLobbyMissionGuide();
		}

		private void HandleNextLobbyStage()
		{
			if (formUnlockSystem == null)
			{
				formUnlockSystem = Object.FindFirstObjectByType<FormUnlockSystem>();
			}
			int num = (((Object)(object)formUnlockSystem != (Object)null) ? formUnlockSystem.HighestUnlockedStage : Mathf.Max(1, currentStageNumber));
			currentStageNumber = Mathf.Clamp(currentStageNumber + 1, 1, num);
			formUnlockSystem?.SetCurrentLobbyStage(currentStageNumber);
			UpdateMetaProgressUi();
			UpdateLobbyMissionGuide();
		}

			private void CacheUiReferences()
			{
			canvasRootTransform = FindChildByName(null, canvasName);
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				Debug.LogWarning((object)"[AlienCrusher] Canvas_Dummy not found. Run the scene scaffolder first.");
				return;
			}
			EnsureCanvasMobileAdaptor(canvasRootTransform);
			hudPanel = GetPanel(canvasRootTransform, hudPanelName);
			resultPanel = GetPanel(canvasRootTransform, resultPanelName);
			levelUpPanel = GetPanel(canvasRootTransform, levelUpPanelName);
			lobbyPanel = GetPanel(canvasRootTransform, lobbyPanelName);
			pausePanel = GetPanel(canvasRootTransform, pausePanelName);
			hudInfoText = FindText(canvasRootTransform, "InfoText");
			hudObjectiveText = FindText(canvasRootTransform, "ObjectiveText");
			hudHintText = FindText(canvasRootTransform, "HintText");
			hudProgressText = FindText(canvasRootTransform, "DestructionProgressText");
			hudChainText = FindText(canvasRootTransform, "ChainText");
			hudUpgradeListText = FindText(canvasRootTransform, "UpgradeListText");
			EnsureHudStageGoalUi();
			EnsureHudRouteIndicatorUi();
			EnsureHudRouteArrowUi();
			EnsureHudBossStatusUi();
			lobbyDpText = FindText(canvasRootTransform, "MetaDpText");
			lobbyMissionText = FindText(canvasRootTransform, "MissionText");
			if ((Object)(object)lobbyMissionText != (Object)null)
			{
				lobbyMissionText.alignment = TextAnchor.UpperCenter;
				lobbyMissionText.lineSpacing = 1.12f;
				lobbyMissionText.horizontalOverflow = HorizontalWrapMode.Wrap;
				lobbyMissionText.verticalOverflow = VerticalWrapMode.Overflow;
			}
			EnsureLobbyRecommendationUi();
			EnsureLobbyMetaDetailUi();
			levelUpTimerText = FindText(canvasRootTransform, "Timer");
				resultSummaryText = FindText(canvasRootTransform, "Summary");
				resultBreakdownText = FindText(canvasRootTransform, "ResultBreakdownText");
			resultContinueHintText = FindText(canvasRootTransform, "ContinueHint");
			EnsureResultHighlightUi();
			EnsureResultMetaProgressUi();
			EnsureResultStatusBadgeUi();
			EnsureResultAdviceUi();
			EnsureResultLobbyButton();
		}

		private void EnsureHudStageGoalUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "StageGoalGaugeRoot");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("StageGoalGaugeRoot", typeof(RectTransform));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 1f);
					val3.anchorMax = new Vector2(0.5f, 1f);
					val3.pivot = new Vector2(0.5f, 1f);
					val3.anchoredPosition = new Vector2(0f, -14f);
					val3.sizeDelta = new Vector2(420f, 54f);
				}
			}
			Transform val4 = FindChildByName(val2, "StageGoalGaugeBg");
			if ((Object)(object)val4 == (Object)null)
			{
				GameObject obj2 = new GameObject("StageGoalGaugeBg", typeof(RectTransform), typeof(Image));
				val4 = obj2.transform;
				val4.SetParent(val2, false);
				RectTransform val5 = (RectTransform)(object)((val4 is RectTransform) ? val4 : null);
				if ((Object)(object)val5 != (Object)null)
				{
					val5.anchorMin = new Vector2(0f, 0f);
					val5.anchorMax = new Vector2(1f, 1f);
					val5.offsetMin = Vector2.zero;
					val5.offsetMax = Vector2.zero;
				}
			}
			hudStageGoalBackgroundImage = ((Component)val4).GetComponent<Image>();
			if ((Object)(object)hudStageGoalBackgroundImage != (Object)null)
			{
				hudStageGoalBackgroundImage.color = new Color(0.08f, 0.1f, 0.14f, 0.82f);
			}
			Transform val6 = FindChildByName(val2, "StageGoalGaugeFill");
			if ((Object)(object)val6 == (Object)null)
			{
				GameObject obj3 = new GameObject("StageGoalGaugeFill", typeof(RectTransform), typeof(Image));
				val6 = obj3.transform;
				val6.SetParent(val4, false);
				RectTransform val7 = (RectTransform)(object)((val6 is RectTransform) ? val6 : null);
				if ((Object)(object)val7 != (Object)null)
				{
					val7.anchorMin = new Vector2(0f, 0f);
					val7.anchorMax = new Vector2(1f, 1f);
					val7.offsetMin = Vector2.zero;
					val7.offsetMax = Vector2.zero;
				}
			}
			hudStageGoalFillImage = ((Component)val6).GetComponent<Image>();
			if ((Object)(object)hudStageGoalFillImage != (Object)null)
			{
				hudStageGoalFillImage.type = Image.Type.Filled;
				hudStageGoalFillImage.fillMethod = Image.FillMethod.Horizontal;
				hudStageGoalFillImage.fillOrigin = 0;
				hudStageGoalFillImage.fillAmount = 0f;
				hudStageGoalFillImage.color = new Color(0.3f, 0.94f, 0.74f, 1f);
			}
			Transform val8 = FindChildByName(val2, "StageGoalGaugeLabel");
			if ((Object)(object)val8 == (Object)null)
			{
				GameObject obj4 = new GameObject("StageGoalGaugeLabel", typeof(RectTransform), typeof(Text));
				val8 = obj4.transform;
				val8.SetParent(val2, false);
				RectTransform val9 = (RectTransform)(object)((val8 is RectTransform) ? val8 : null);
				if ((Object)(object)val9 != (Object)null)
				{
					val9.anchorMin = new Vector2(0f, 0f);
					val9.anchorMax = new Vector2(1f, 1f);
					val9.offsetMin = new Vector2(14f, 6f);
					val9.offsetMax = new Vector2(-14f, -6f);
				}
			}
			hudStageGoalText = ((Component)val8).GetComponent<Text>();
			if ((Object)(object)hudStageGoalText != (Object)null)
			{
				hudStageGoalText.alignment = TextAnchor.MiddleCenter;
				hudStageGoalText.fontSize = 22;
				hudStageGoalText.fontStyle = FontStyle.Bold;
				hudStageGoalText.horizontalOverflow = HorizontalWrapMode.Overflow;
				hudStageGoalText.verticalOverflow = VerticalWrapMode.Overflow;
				hudStageGoalText.color = Color.white;
				if ((Object)(object)hudStageGoalText.font == (Object)null)
				{
					hudStageGoalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
			}
		}

		private void EnsureHudRouteIndicatorUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "HudRouteIndicatorText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("HudRouteIndicatorText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 1f);
					val3.anchorMax = new Vector2(0.5f, 1f);
					val3.pivot = new Vector2(0.5f, 1f);
					val3.anchoredPosition = new Vector2(0f, -74f);
					val3.sizeDelta = new Vector2(720f, 42f);
				}
			}
			hudRouteIndicatorText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)hudRouteIndicatorText != (Object)null)
			{
				hudRouteIndicatorText.alignment = TextAnchor.MiddleCenter;
				hudRouteIndicatorText.fontSize = 20;
				hudRouteIndicatorText.fontStyle = FontStyle.Bold;
				hudRouteIndicatorText.horizontalOverflow = HorizontalWrapMode.Overflow;
				hudRouteIndicatorText.verticalOverflow = VerticalWrapMode.Overflow;
				hudRouteIndicatorText.color = new Color(1f, 0.92f, 0.7f, 1f);
				if ((Object)(object)hudRouteIndicatorText.font == (Object)null)
				{
					hudRouteIndicatorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
			}
		}

		private void EnsureHudRouteArrowUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "HudRouteArrow");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("HudRouteArrow", typeof(RectTransform));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0.5f);
					val3.anchorMax = new Vector2(0.5f, 0.5f);
					val3.pivot = new Vector2(0.5f, 0.5f);
					val3.anchoredPosition = Vector2.zero;
					val3.sizeDelta = new Vector2(84f, 84f);
				}
				GameObject obj2 = new GameObject("ArrowText", typeof(RectTransform), typeof(Text));
				Transform transform = obj2.transform;
				transform.SetParent(val2, false);
				RectTransform val4 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
				if ((Object)(object)val4 != (Object)null)
				{
					StretchRect(val4);
				}
				Text component = ((Component)transform).GetComponent<Text>();
				if ((Object)(object)component != (Object)null)
				{
					component.alignment = TextAnchor.MiddleCenter;
					component.fontSize = 46;
					component.fontStyle = FontStyle.Bold;
					component.text = "^";
					component.color = new Color(1f, 0.9f, 0.54f, 1f);
					if ((Object)(object)component.font == (Object)null)
					{
						component.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
					}
				}
			}
			hudRouteArrowRect = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
			if ((Object)(object)hudRouteArrowRect != (Object)null)
			{
				((Component)hudRouteArrowRect).gameObject.SetActive(false);
				Transform val5 = FindChildByName(hudRouteArrowRect, "ArrowText");
				if ((Object)(object)val5 != (Object)null)
				{
					hudRouteArrowText = ((Component)val5).GetComponent<Text>();
				}
			}
		}

		private void EnsureHudBossStatusUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, hudPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "HudBossStatusText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("HudBossStatusText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(1f, 1f);
					val3.anchorMax = new Vector2(1f, 1f);
					val3.pivot = new Vector2(1f, 1f);
					val3.anchoredPosition = new Vector2(-18f, -18f);
					val3.sizeDelta = new Vector2(360f, 120f);
				}
			}
			hudBossStatusText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)hudBossStatusText != (Object)null)
			{
				hudBossStatusText.alignment = TextAnchor.UpperRight;
				hudBossStatusText.fontSize = 18;
				hudBossStatusText.fontStyle = FontStyle.Bold;
				hudBossStatusText.horizontalOverflow = HorizontalWrapMode.Wrap;
				hudBossStatusText.verticalOverflow = VerticalWrapMode.Overflow;
				hudBossStatusText.color = new Color(1f, 0.82f, 0.62f, 1f);
				if ((Object)(object)hudBossStatusText.font == (Object)null)
				{
					hudBossStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
			}
		}

		private void EnsureLobbyRecommendationUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, lobbyPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "LobbyRecommendationText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("LobbyRecommendationText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0f);
					val3.anchorMax = new Vector2(0.5f, 0f);
					val3.pivot = new Vector2(0.5f, 0f);
					val3.anchoredPosition = new Vector2(0f, 48f);
					val3.sizeDelta = new Vector2(760f, 96f);
				}
			}
			lobbyRecommendationText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)lobbyRecommendationText != (Object)null)
			{
				lobbyRecommendationText.alignment = TextAnchor.UpperCenter;
				lobbyRecommendationText.fontSize = 19;
				lobbyRecommendationText.fontStyle = FontStyle.Bold;
				lobbyRecommendationText.horizontalOverflow = HorizontalWrapMode.Wrap;
				lobbyRecommendationText.verticalOverflow = VerticalWrapMode.Overflow;
				lobbyRecommendationText.color = new Color(0.92f, 0.95f, 1f, 1f);
				if ((Object)(object)lobbyRecommendationText.font == (Object)null)
				{
					lobbyRecommendationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
				lobbyRecommendationText.lineSpacing = 1.08f;
			}
		}

		private void EnsureLobbyMetaDetailUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, lobbyPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "LobbyMetaDetailText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("LobbyMetaDetailText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0f);
					val3.anchorMax = new Vector2(0.5f, 0f);
					val3.pivot = new Vector2(0.5f, 0f);
					val3.anchoredPosition = new Vector2(0f, 154f);
					val3.sizeDelta = new Vector2(760f, 88f);
				}
			}
			lobbyMetaDetailText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)lobbyMetaDetailText != (Object)null)
			{
				lobbyMetaDetailText.alignment = TextAnchor.UpperCenter;
				lobbyMetaDetailText.fontSize = 17;
				lobbyMetaDetailText.fontStyle = FontStyle.Bold;
				lobbyMetaDetailText.horizontalOverflow = HorizontalWrapMode.Wrap;
				lobbyMetaDetailText.verticalOverflow = VerticalWrapMode.Overflow;
				lobbyMetaDetailText.color = new Color(0.82f, 0.88f, 0.96f, 1f);
				if ((Object)(object)lobbyMetaDetailText.font == (Object)null)
				{
					lobbyMetaDetailText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
				lobbyMetaDetailText.lineSpacing = 1.08f;
			}
		}

		private void EnsureResultHighlightUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, resultPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "ResultHighlightText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("ResultHighlightText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0.5f);
					val3.anchorMax = new Vector2(0.5f, 0.5f);
					val3.pivot = new Vector2(0.5f, 0.5f);
					val3.anchoredPosition = new Vector2(0f, -52f);
					val3.sizeDelta = new Vector2(760f, 180f);
				}
			}
			resultHighlightText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)resultHighlightText != (Object)null)
			{
				resultHighlightText.alignment = TextAnchor.MiddleCenter;
				resultHighlightText.fontSize = 23;
				resultHighlightText.fontStyle = FontStyle.Bold;
				resultHighlightText.horizontalOverflow = HorizontalWrapMode.Wrap;
				resultHighlightText.verticalOverflow = VerticalWrapMode.Overflow;
				resultHighlightText.color = new Color(1f, 0.9f, 0.62f, 1f);
				if ((Object)(object)resultHighlightText.font == (Object)null)
				{
					resultHighlightText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
				resultHighlightText.lineSpacing = 1.04f;
			}
		}

		private void EnsureResultMetaProgressUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, resultPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "ResultMetaProgressText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("ResultMetaProgressText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0f);
					val3.anchorMax = new Vector2(0.5f, 0f);
					val3.pivot = new Vector2(0.5f, 0f);
					val3.anchoredPosition = new Vector2(0f, 88f);
					val3.sizeDelta = new Vector2(760f, 148f);
				}
			}
			resultMetaProgressText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)resultMetaProgressText != (Object)null)
			{
				resultMetaProgressText.alignment = TextAnchor.UpperCenter;
				resultMetaProgressText.fontSize = 19;
				resultMetaProgressText.fontStyle = FontStyle.Bold;
				resultMetaProgressText.horizontalOverflow = HorizontalWrapMode.Wrap;
				resultMetaProgressText.verticalOverflow = VerticalWrapMode.Overflow;
				resultMetaProgressText.color = new Color(0.82f, 0.94f, 1f, 1f);
				if ((Object)(object)resultMetaProgressText.font == (Object)null)
				{
					resultMetaProgressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
				resultMetaProgressText.lineSpacing = 1.08f;
			}
		}

		private void EnsureResultStatusBadgeUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, resultPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "ResultStatusBadgeText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("ResultStatusBadgeText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 1f);
					val3.anchorMax = new Vector2(0.5f, 1f);
					val3.pivot = new Vector2(0.5f, 1f);
					val3.anchoredPosition = new Vector2(0f, -24f);
					val3.sizeDelta = new Vector2(760f, 52f);
				}
			}
			resultStatusBadgeText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)resultStatusBadgeText != (Object)null)
			{
				resultStatusBadgeText.alignment = TextAnchor.MiddleCenter;
				resultStatusBadgeText.fontSize = 20;
				resultStatusBadgeText.fontStyle = FontStyle.Bold;
				resultStatusBadgeText.horizontalOverflow = HorizontalWrapMode.Wrap;
				resultStatusBadgeText.verticalOverflow = VerticalWrapMode.Overflow;
				resultStatusBadgeText.color = Color.white;
				resultStatusBadgeText.supportRichText = true;
				if ((Object)(object)resultStatusBadgeText.font == (Object)null)
				{
					resultStatusBadgeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
			}
		}

		private void EnsureResultAdviceUi()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, resultPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "ResultAdviceText");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("ResultAdviceText", typeof(RectTransform), typeof(Text));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0f);
					val3.anchorMax = new Vector2(0.5f, 0f);
					val3.pivot = new Vector2(0.5f, 0f);
					val3.anchoredPosition = new Vector2(0f, 220f);
					val3.sizeDelta = new Vector2(760f, 126f);
				}
			}
			resultAdviceText = ((Component)val2).GetComponent<Text>();
			if ((Object)(object)resultAdviceText != (Object)null)
			{
				resultAdviceText.alignment = TextAnchor.UpperCenter;
				resultAdviceText.fontSize = 17;
				resultAdviceText.fontStyle = FontStyle.Bold;
				resultAdviceText.horizontalOverflow = HorizontalWrapMode.Wrap;
				resultAdviceText.verticalOverflow = VerticalWrapMode.Overflow;
				resultAdviceText.color = new Color(0.95f, 0.88f, 0.68f, 1f);
				if ((Object)(object)resultAdviceText.font == (Object)null)
				{
					resultAdviceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
				}
				resultAdviceText.lineSpacing = 1.1f;
			}
		}

		private void EnsureResultLobbyButton()
		{
			if ((Object)(object)canvasRootTransform == (Object)null)
			{
				return;
			}
			Transform val = FindChildByName(canvasRootTransform, resultPanelName);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildByName(val, "ResultLobbyButton");
			if ((Object)(object)val2 == (Object)null)
			{
				GameObject obj = new GameObject("ResultLobbyButton", typeof(RectTransform), typeof(Image), typeof(Button));
				val2 = obj.transform;
				val2.SetParent(val, false);
				RectTransform val3 = (RectTransform)(object)((val2 is RectTransform) ? val2 : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.anchorMin = new Vector2(0.5f, 0f);
					val3.anchorMax = new Vector2(0.5f, 0f);
					val3.pivot = new Vector2(0.5f, 0f);
					val3.anchoredPosition = new Vector2(0f, 18f);
					val3.sizeDelta = new Vector2(420f, 110f);
				}
				Image component = ((Component)val2).GetComponent<Image>();
				if ((Object)(object)component != (Object)null)
				{
					component.color = new Color(0.2f, 0.24f, 0.3f, 0.94f);
				}
				GameObject obj2 = new GameObject("Text", typeof(RectTransform), typeof(Text));
				Transform transform = obj2.transform;
				transform.SetParent(val2, false);
				RectTransform val4 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
				if ((Object)(object)val4 != (Object)null)
				{
					StretchRect(val4);
				}
				Text component2 = ((Component)transform).GetComponent<Text>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.alignment = TextAnchor.MiddleCenter;
					component2.fontSize = 28;
					component2.fontStyle = FontStyle.Bold;
					component2.lineSpacing = 1.02f;
					component2.text = "RETURN TO LOBBY";
					component2.color = Color.white;
					if ((Object)(object)component2.font == (Object)null)
					{
						component2.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
					}
				}
			}
		}

		private void WireUiButtons()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Expected O, but got Unknown
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Expected O, but got Unknown
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected O, but got Unknown
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Expected O, but got Unknown
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Expected O, but got Unknown
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Expected O, but got Unknown
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Expected O, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Expected O, but got Unknown
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Expected O, but got Unknown
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Expected O, but got Unknown
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Expected O, but got Unknown
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Expected O, but got Unknown
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Expected O, but got Unknown
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Expected O, but got Unknown
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Expected O, but got Unknown
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Expected O, but got Unknown
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Expected O, but got Unknown
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Expected O, but got Unknown
			WireButton("PlayButton", new UnityAction(StartStage));
			WireButton("NextStageButton", new UnityAction(StartNextStage));
			WireButton("WatchAdButton", new UnityAction(WatchAdAndRestart));
			WireButton("ResultLobbyButton", new UnityAction(ShowLobby));
			WireButton("StagePrevButton", new UnityAction(HandlePreviousLobbyStage));
			WireButton("StageNextButton", new UnityAction(HandleNextLobbyStage));
			WireButton("Form_Sphere", (UnityAction)delegate
			{
				HandleFormSelection(FormType.Sphere);
			});
			WireButton("Form_Spike", (UnityAction)delegate
			{
				HandleFormSelection(FormType.Spike);
			});
			WireButton("Form_Ram", (UnityAction)delegate
			{
				HandleFormSelection(FormType.Ram);
			});
			WireButton("Form_Saucer", (UnityAction)delegate
			{
				HandleFormSelection(FormType.Saucer);
			});
			WireButton("Form_Crusher", (UnityAction)delegate
			{
				HandleFormSelection(FormType.Crusher);
			});
			WireButton("TransformButton", new UnityAction(HandlePrimaryActionSkill));
			WireButton("Special1Button", new UnityAction(HandlePrimaryActionSkill));
			WireButton("Special2Button", new UnityAction(HandleActionButtonPlaceholder));
			WireButton("SkillOption_A", new UnityAction(ChooseSkillOptionA));
			WireButton("SkillOption_B", new UnityAction(ChooseSkillOptionB));
			WireButton("SkillOption_C", new UnityAction(ChooseSkillOptionC));
			WireButton("SkillRerollButton", new UnityAction(HandleLevelUpRerollButton));
			WireButton("SkillLock_A", new UnityAction(ToggleLevelUpLockA));
			WireButton("SkillLock_B", new UnityAction(ToggleLevelUpLockB));
			WireButton("SkillLock_C", new UnityAction(ToggleLevelUpLockC));
			WireButton("TreeButton", new UnityAction(HandleCycleMetaUpgrade));
			WireButton("ShopButton", new UnityAction(HandleBuyMetaUpgrade));
			WireButton("MetaUpgrade_SizeButton", new UnityAction(HandleBuyMetaSize));
			WireButton("MetaUpgrade_ImpactButton", new UnityAction(HandleBuyMetaImpact));
			WireButton("MetaUpgrade_DpButton", new UnityAction(HandleBuyMetaDp));
			WireButton("PauseButton", new UnityAction(TogglePause));
			WireButton("PauseResumeButton", new UnityAction(ResumeFromPause));
			WireButton("PauseRestartButton", new UnityAction(RestartFromPause));
			WireButton("PauseLobbyButton", new UnityAction(ReturnLobbyFromPause));
		}

		private void HandleActionButtonPlaceholder()
		{
		}

		private void ApplyUiViewState(UiViewState state)
		{
			if (currentUiViewState == state)
			{
				bool allowGameplayInput = state == UiViewState.Hud && stageRunning;
				SetGameplayInputEnabled(allowGameplayInput);
				return;
			}
			currentUiViewState = state;
			bool showLobby = state == UiViewState.Lobby;
			bool showHud = state == UiViewState.Hud || state == UiViewState.LevelUp || state == UiViewState.Pause;
			bool showLevelUp = state == UiViewState.LevelUp;
			bool showPause = state == UiViewState.Pause;
			bool showResult = state == UiViewState.Result;
			SetPanelState(lobbyPanel, showLobby);
			SetPanelState(hudPanel, showHud);
			SetPanelState(levelUpPanel, showLevelUp);
			SetPanelState(pausePanel, showPause);
			SetPanelState(resultPanel, showResult);
			SetLobbyPanelVisible(showLobby);
			SetHudPanelVisible(showHud);
			SetLevelUpPanelVisible(showLevelUp);
			SetPausePanelVisible(showPause);
			SetResultPanelVisible(showResult);
			SetHudActionButtonsVisible(showHud && !showLevelUp && !showPause && stageRunning && ShouldShowActionButtons());
			SetGameplayInputEnabled(showHud && !showLevelUp && !showPause && stageRunning);
		}

		private void SetPaused(bool paused)
		{
			stagePaused = paused && stageRunning;
			Time.timeScale = stagePaused ? 0f : 1f;
			if (stagePaused)
			{
				ApplyUiViewState(UiViewState.Pause);
			}
			else if (!stageRunning)
			{
				SetGameplayInputEnabled(enabled: false);
			}
			else if (levelUpOpen)
			{
				ApplyUiViewState(UiViewState.LevelUp);
			}
			else
			{
				ApplyUiViewState(UiViewState.Hud);
			}
		}

		private void SetGameplayInputEnabled(bool enabled)
		{
			VirtualJoystickUI virtualJoystickUI = Object.FindFirstObjectByType<VirtualJoystickUI>(FindObjectsInactive.Include);
			if ((Object)(object)virtualJoystickUI != (Object)null)
			{
				virtualJoystickUI.SetInputEnabled(enabled);
			}
		}

		private void SetHudActionButtonsVisible(bool visible)
		{
			SetObjectsActiveByName("SkillButtons", visible);
		}

		private void SetHudPanelVisible(bool visible)
		{
			SetObjectsActiveByName(hudPanelName, visible);
		}

		private void SetResultPanelVisible(bool visible)
		{
			SetObjectsActiveByName(resultPanelName, visible);
		}

		private void SetLevelUpPanelVisible(bool visible)
		{
			SetObjectsActiveByName(levelUpPanelName, visible);
		}

		private void SetLobbyPanelVisible(bool visible)
		{
			SetObjectsActiveByName(lobbyPanelName, visible);
		}

		private void SetPausePanelVisible(bool visible)
		{
			SetObjectsActiveByName(pausePanelName, visible);
		}

		private static void SetObjectsActiveByName(string objectName, bool active)
		{
			if (string.IsNullOrWhiteSpace(objectName))
			{
				return;
			}
			Transform[] array = Object.FindObjectsByType<Transform>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			foreach (Transform val in array)
			{
				if (!((Object)(object)val == (Object)null) && !(((Object)val).name != objectName))
				{
					((Component)val).gameObject.SetActive(active);
				}
			}
		}

		private void WireButton(string buttonName, UnityAction callback)
		{
			Button button = FindButton(buttonName);
			if (!((Object)(object)button == (Object)null))
			{
				((UnityEventBase)button.onClick).RemoveAllListeners();
				((UnityEvent)button.onClick).AddListener(callback);
			}
		}

		private DummyPanelTag GetPanel(Transform root, string panelName)
		{
			Transform val = FindChildByName(root, panelName);
			if ((Object)(object)val == (Object)null)
			{
				return null;
			}
			return ((Component)val).GetComponent<DummyPanelTag>();
		}

		private Text FindText(Transform root, string objectName)
		{
			Transform val = FindChildByName(root, objectName);
			if ((Object)(object)val == (Object)null)
			{
				return null;
			}
			return ((Component)val).GetComponent<Text>();
		}

		private Button FindButton(string objectName)
		{
			Transform val = FindChildByName(canvasRootTransform, objectName);
			if ((Object)(object)val == (Object)null)
			{
				val = FindChildByName(null, objectName);
			}
			if ((Object)(object)val == (Object)null)
			{
				return null;
			}
			return ((Component)val).GetComponent<Button>();
		}

		private void EnsureCanvasMobileAdaptor(Transform canvasRoot)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			CanvasScaler canvasScaler = ((Component)canvasRoot).GetComponent<CanvasScaler>();
			if ((Object)(object)canvasScaler == (Object)null)
			{
				canvasScaler = ((Component)canvasRoot).gameObject.AddComponent<CanvasScaler>();
			}
			Transform val = FindDirectChild(canvasRoot, "SafeAreaRoot");
			if ((Object)(object)val == (Object)null)
			{
				val = new GameObject("SafeAreaRoot", new Type[1] { typeof(RectTransform) }).transform;
				val.SetParent(canvasRoot, false);
			}
			MoveChildIfExists(canvasRoot, val, hudPanelName);
			MoveChildIfExists(canvasRoot, val, resultPanelName);
			MoveChildIfExists(canvasRoot, val, levelUpPanelName);
			MoveChildIfExists(canvasRoot, val, lobbyPanelName);
			RectTransform val2 = (RectTransform)(object)((val is RectTransform) ? val : null);
			if ((Object)(object)val2 != (Object)null)
			{
				StretchRect(val2);
			}
			MobileCanvasAdaptor mobileCanvasAdaptor = ((Component)canvasRoot).GetComponent<MobileCanvasAdaptor>();
			if ((Object)(object)mobileCanvasAdaptor == (Object)null)
			{
				mobileCanvasAdaptor = ((Component)canvasRoot).gameObject.AddComponent<MobileCanvasAdaptor>();
			}
			mobileCanvasAdaptor.Configure(canvasScaler, val2);
		}

		private static void EnsureUiInputModuleMode()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
			if ((Object)(object)eventSystem == (Object)null)
			{
				eventSystem = new GameObject("EventSystem", new Type[1] { typeof(EventSystem) }).GetComponent<EventSystem>();
			}
			StandaloneInputModule component = ((Component)eventSystem).GetComponent<StandaloneInputModule>();
			if ((Object)(object)component != (Object)null)
			{
				Object.Destroy((Object)(object)component);
			}
			if ((Object)(object)((Component)eventSystem).GetComponent<InputSystemUIInputModule>() == (Object)null)
			{
				((Component)eventSystem).gameObject.AddComponent<InputSystemUIInputModule>();
			}
		}
	}
}



