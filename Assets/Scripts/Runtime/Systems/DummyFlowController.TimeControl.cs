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
			float targetScale = 1f;
			if (overdriveTimeScaleActive)
			{
				targetScale = Mathf.Min(targetScale, overdriveTimeScale);
			}
			if (bossFinishTimeScaleActive)
			{
				targetScale = Mathf.Min(targetScale, bossFinishTimeScale);
			}
			if (pauseTimeScaleActive)
			{
				targetScale = 0f;
			}

			Time.timeScale = targetScale;
			Time.fixedDeltaTime = targetScale > 0f
				? baseFixedDeltaTime * targetScale
				: baseFixedDeltaTime;
		}

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
