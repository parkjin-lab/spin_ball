using UnityEngine;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private enum TimeScaleChannel
		{
			Pause,
			OverdrivePulse,
			BossFinish
		}

		private bool timeControlInitialized;
		private float baseFixedDeltaTime = 0.02f;
		private bool pauseTimeScaleActive;
		private bool overdriveTimeScaleActive;
		private bool bossFinishTimeScaleActive;
		private float overdriveTimeScale = 1f;
		private float bossFinishTimeScale = 1f;

		private void InitializeTimeControl()
		{
			if (timeControlInitialized)
			{
				return;
			}

			float currentScale = Time.timeScale;
			baseFixedDeltaTime = currentScale > 0.0001f
				? Mathf.Max(0.0001f, Time.fixedDeltaTime / currentScale)
				: Mathf.Max(0.0001f, Time.fixedDeltaTime);
			timeControlInitialized = true;
		}

		private void SetTimeScaleChannel(TimeScaleChannel channel, bool active, float scale = 1f)
		{
			InitializeTimeControl();
			float safeScale = Mathf.Clamp(scale, 0.01f, 1f);
			switch (channel)
			{
				case TimeScaleChannel.Pause:
					pauseTimeScaleActive = active;
					break;
				case TimeScaleChannel.OverdrivePulse:
					overdriveTimeScaleActive = active;
					overdriveTimeScale = safeScale;
					break;
				case TimeScaleChannel.BossFinish:
					bossFinishTimeScaleActive = active;
					bossFinishTimeScale = safeScale;
					break;
			}

			ApplyTimeScaleChannels();
		}

		private void ApplyTimeScaleChannels()
		{
			float targetScale = ResolveTargetTimeScale(
				pauseTimeScaleActive,
				overdriveTimeScaleActive,
				overdriveTimeScale,
				bossFinishTimeScaleActive,
				bossFinishTimeScale);
			Time.timeScale = targetScale;
			Time.fixedDeltaTime = ResolveFixedDeltaTime(baseFixedDeltaTime, targetScale);
		}

		private static float ResolveTargetTimeScale(
			bool pauseActive,
			bool overdriveActive,
			float requestedOverdriveScale,
			bool bossFinishActive,
			float requestedBossFinishScale)
		{
			float targetScale = 1f;
			if (overdriveActive)
			{
				targetScale = Mathf.Min(targetScale, Mathf.Clamp(requestedOverdriveScale, 0.01f, 1f));
			}
			if (bossFinishActive)
			{
				targetScale = Mathf.Min(targetScale, Mathf.Clamp(requestedBossFinishScale, 0.01f, 1f));
			}

			return pauseActive ? 0f : targetScale;
		}

		private static float ResolveFixedDeltaTime(float unscaledFixedDeltaTime, float targetScale)
		{
			float safeBaseDeltaTime = Mathf.Max(0.0001f, unscaledFixedDeltaTime);
			return targetScale > 0f ? safeBaseDeltaTime * targetScale : safeBaseDeltaTime;
		}

#if UNITY_EDITOR
		public static Vector2 ResolveTimeStateForValidation(
			bool pauseActive,
			bool overdriveActive,
			float requestedOverdriveScale,
			bool bossFinishActive,
			float requestedBossFinishScale,
			float unscaledFixedDeltaTime = 0.02f)
		{
			float targetScale = ResolveTargetTimeScale(
				pauseActive,
				overdriveActive,
				requestedOverdriveScale,
				bossFinishActive,
				requestedBossFinishScale);
			return new Vector2(targetScale, ResolveFixedDeltaTime(unscaledFixedDeltaTime, targetScale));
		}
#endif
		private void ResetTimeControl()
		{
			InitializeTimeControl();
			stagePaused = false;
			pauseTimeScaleActive = false;
			overdriveTimeScaleActive = false;
			bossFinishTimeScaleActive = false;
			overdrivePulseRoutine = null;
			bossFinishSlowMotionRoutine = null;
			Time.timeScale = 1f;
			Time.fixedDeltaTime = baseFixedDeltaTime;
		}
	}
}
