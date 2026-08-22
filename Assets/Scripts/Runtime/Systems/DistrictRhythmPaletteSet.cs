using UnityEngine;

namespace AlienCrusher.Systems
{
	public static class DistrictRhythmPaletteSet
	{
		public const string StarterResidentialId = "PAL_District_StarterResidential";
		public const string MarketPlazaId = "PAL_District_MarketPlaza";
		public const string SentinelCheckpointId = "PAL_District_SentinelCheckpoint";
		public const string SkylineBlockId = "PAL_District_SkylineBlock";

		public static bool TryApplyCoreRhythmPalette(
			int stageNumber,
			ref Color ground,
			ref Color wall,
			ref Color asphalt,
			ref Color stripe,
			ref Color sidewalk,
			ref Color foliageA,
			ref Color foliageB,
			ref Color neutralA,
			ref Color neutralB,
			ref Color accentA,
			ref Color accentB,
			ref Color hazardA,
			ref Color hazardB)
		{
			if (stageNumber <= 1)
			{
				ApplyStarterResidential(ref ground, ref wall, ref asphalt, ref stripe, ref sidewalk, ref foliageA, ref foliageB, ref neutralA, ref neutralB, ref accentA, ref accentB, ref hazardA, ref hazardB);
				return true;
			}

			if (stageNumber == 3)
			{
				ApplyMarketPlaza(ref ground, ref wall, ref asphalt, ref stripe, ref sidewalk, ref foliageA, ref foliageB, ref neutralA, ref neutralB, ref accentA, ref accentB, ref hazardA, ref hazardB);
				return true;
			}

			if (stageNumber == 4)
			{
				ApplySentinelCheckpoint(ref ground, ref wall, ref asphalt, ref stripe, ref sidewalk, ref foliageA, ref foliageB, ref neutralA, ref neutralB, ref accentA, ref accentB, ref hazardA, ref hazardB);
				return true;
			}

			if (stageNumber == 7)
			{
				ApplySkylineBlock(ref ground, ref wall, ref asphalt, ref stripe, ref sidewalk, ref foliageA, ref foliageB, ref neutralA, ref neutralB, ref accentA, ref accentB, ref hazardA, ref hazardB);
				return true;
			}

			return false;
		}

		private static void ApplyStarterResidential(
			ref Color ground,
			ref Color wall,
			ref Color asphalt,
			ref Color stripe,
			ref Color sidewalk,
			ref Color foliageA,
			ref Color foliageB,
			ref Color neutralA,
			ref Color neutralB,
			ref Color accentA,
			ref Color accentB,
			ref Color hazardA,
			ref Color hazardB)
		{
			// Clean opener road. Warm gray-green, not navy DenseCore and not orchid nav.
			ground = new Color(0.18f, 0.20f, 0.17f, 1f);
			wall = new Color(0.30f, 0.34f, 0.28f, 1f);
			asphalt = new Color(0.22f, 0.23f, 0.21f, 1f);
			stripe = new Color(0.92f, 0.90f, 0.80f, 0.92f);
			sidewalk = new Color(0.88f, 0.86f, 0.78f, 0.90f);
			foliageA = new Color(0.28f, 0.62f, 0.30f, 1f);
			foliageB = new Color(0.52f, 0.78f, 0.40f, 1f);
			neutralA = new Color(0.90f, 0.88f, 0.82f, 1f);
			neutralB = new Color(0.98f, 0.86f, 0.62f, 1f);
			accentA = new Color(0.86f, 0.42f, 0.32f, 1f);
			accentB = new Color(0.22f, 0.70f, 0.78f, 1f);
			hazardA = new Color(1f, 0.72f, 0.38f, 1f);
			hazardB = new Color(1f, 0.46f, 0.22f, 1f);
		}

		private static void ApplyMarketPlaza(
			ref Color ground,
			ref Color wall,
			ref Color asphalt,
			ref Color stripe,
			ref Color sidewalk,
			ref Color foliageA,
			ref Color foliageB,
			ref Color neutralA,
			ref Color neutralB,
			ref Color accentA,
			ref Color accentB,
			ref Color hazardA,
			ref Color hazardB)
		{
			// Density pivot. Warm stall street, busier than the starter lane.
			ground = new Color(0.16f, 0.11f, 0.10f, 1f);
			wall = new Color(0.34f, 0.20f, 0.18f, 1f);
			asphalt = new Color(0.18f, 0.13f, 0.12f, 1f);
			stripe = new Color(0.90f, 0.55f, 0.22f, 0.92f);
			sidewalk = new Color(0.82f, 0.70f, 0.52f, 0.90f);
			foliageA = new Color(0.22f, 0.48f, 0.24f, 1f);
			foliageB = new Color(0.40f, 0.62f, 0.30f, 1f);
			neutralA = new Color(0.78f, 0.62f, 0.48f, 1f);
			neutralB = new Color(0.94f, 0.58f, 0.28f, 1f);
			accentA = new Color(0.86f, 0.18f, 0.22f, 1f);
			accentB = new Color(0.98f, 0.78f, 0.22f, 1f);
			hazardA = new Color(0.92f, 0.38f, 0.16f, 1f);
			hazardB = new Color(1f, 0.62f, 0.18f, 1f);
		}

		private static void ApplySentinelCheckpoint(
			ref Color ground,
			ref Color wall,
			ref Color asphalt,
			ref Color stripe,
			ref Color sidewalk,
			ref Color foliageA,
			ref Color foliageB,
			ref Color neutralA,
			ref Color neutralB,
			ref Color accentA,
			ref Color accentB,
			ref Color hazardA,
			ref Color hazardB)
		{
			// Boss-approach warning. Cool steel, amber caution, not market warmth.
			ground = new Color(0.07f, 0.09f, 0.14f, 1f);
			wall = new Color(0.12f, 0.16f, 0.24f, 1f);
			asphalt = new Color(0.10f, 0.12f, 0.18f, 1f);
			stripe = new Color(0.86f, 0.62f, 0.28f, 0.92f);
			sidewalk = new Color(0.52f, 0.58f, 0.68f, 0.90f);
			foliageA = new Color(0.16f, 0.28f, 0.22f, 1f);
			foliageB = new Color(0.22f, 0.36f, 0.28f, 1f);
			neutralA = new Color(0.42f, 0.50f, 0.62f, 1f);
			neutralB = new Color(0.72f, 0.58f, 0.32f, 1f);
			accentA = new Color(0.28f, 0.48f, 0.72f, 1f);
			accentB = new Color(0.86f, 0.70f, 0.32f, 1f);
			hazardA = new Color(0.96f, 0.42f, 0.16f, 1f);
			hazardB = new Color(1f, 0.70f, 0.22f, 1f);
		}

		private static void ApplySkylineBlock(
			ref Color ground,
			ref Color wall,
			ref Color asphalt,
			ref Color stripe,
			ref Color sidewalk,
			ref Color foliageA,
			ref Color foliageB,
			ref Color neutralA,
			ref Color neutralB,
			ref Color accentA,
			ref Color accentB,
			ref Color hazardA,
			ref Color hazardB)
		{
			// Late climax. Night plaza with a pale crown, not sentinel navy.
			ground = new Color(0.05f, 0.05f, 0.07f, 1f);
			wall = new Color(0.10f, 0.10f, 0.14f, 1f);
			asphalt = new Color(0.08f, 0.08f, 0.10f, 1f);
			stripe = new Color(0.72f, 0.86f, 0.94f, 0.92f);
			sidewalk = new Color(0.76f, 0.80f, 0.88f, 0.90f);
			foliageA = new Color(0.14f, 0.22f, 0.20f, 1f);
			foliageB = new Color(0.22f, 0.32f, 0.28f, 1f);
			neutralA = new Color(0.70f, 0.74f, 0.82f, 1f);
			neutralB = new Color(0.88f, 0.90f, 0.94f, 1f);
			accentA = new Color(0.36f, 0.78f, 0.92f, 1f);
			accentB = new Color(0.92f, 0.94f, 0.98f, 1f);
			hazardA = new Color(0.90f, 0.36f, 0.20f, 1f);
			hazardB = new Color(1f, 0.58f, 0.24f, 1f);
		}
	}
}
