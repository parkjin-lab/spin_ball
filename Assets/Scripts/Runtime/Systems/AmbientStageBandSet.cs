using UnityEngine;

namespace AlienCrusher.Systems
{
	public static class AmbientStageBandSet
	{
		public const string AmbientStageBandsId = "PAL_Ambient_StageBands";

		public static void Apply(int stageNumber)
		{
			Color ambient = Resolve(stageNumber);
			RenderSettings.ambientLight = ambient;
			RenderSettings.fogColor = ambient;
			RenderSettings.fog = false;
		}

		public static Color Resolve(int stageNumber)
		{
			int stage = Mathf.Max(1, stageNumber);
			if (stage <= 1)
			{
				// Opener daylight. Warm, not orchid nav.
				return new Color(0.68f, 0.66f, 0.60f, 1f);
			}

			if (stage == 2)
			{
				// Park cut. Soft green fill, still mid-gray so plaster/concrete read.
				return new Color(0.58f, 0.68f, 0.60f, 1f);
			}

			if (stage == 3)
			{
				// Market density. Warm stall fill, not magenta route paint.
				return new Color(0.70f, 0.63f, 0.56f, 1f);
			}

			if (stage == 4)
			{
				// Sentinel approach. Cool steel, close to the old shared default.
				return new Color(0.56f, 0.60f, 0.72f, 1f);
			}

			if (stage == 5)
			{
				// Yard blast. Dusty warm, not Stage 3 stall orange.
				return new Color(0.68f, 0.61f, 0.50f, 1f);
			}

			if (stage == 6)
			{
				// Power block. Cool utility, not teal stripe chroma.
				return new Color(0.54f, 0.66f, 0.67f, 1f);
			}

			if (stage == 7)
			{
				// Skyline climax. Cooler night fill that still keeps markers bright.
				return new Color(0.50f, 0.54f, 0.66f, 1f);
			}

			return new Color(0.52f, 0.56f, 0.68f, 1f);
		}
	}
}
