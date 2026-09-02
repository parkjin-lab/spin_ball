using UnityEngine;

namespace AlienCrusher.Systems
{
    /// <summary>
    /// Per-frame GPU safety for smash/crush feedback. A destruction shockwave
    /// can fire many PlayDestroyFeedback / debris / combo-rush bursts in one
    /// tick; on old D3D12 drivers that can TDR the GPU (device removal 887a0005).
    /// Gameplay damage, score, and break thresholds never consult this budget.
    /// </summary>
    internal static class SmashVfxBudget
    {
        private const int MaxBurstSpawnsPerFrame = 12;
        private const int MaxDestroyVisualsPerFrame = 5;
        private const int MaxComboRushVisualsPerFrame = 1;
        private const int MaxDebrisVisualsPerFrame = 8;
        private const int MaxWeakPointFlashesPerFrame = 2;
        private const int MaxScreenFlashesPerFrame = 4;

        private static int frame = -1;
        private static int burstSpawns;
        private static int destroyVisuals;
        private static int comboRushVisuals;
        private static int debrisVisuals;
        private static int weakPointFlashes;
        private static int screenFlashes;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            frame = -1;
            burstSpawns = 0;
            destroyVisuals = 0;
            comboRushVisuals = 0;
            debrisVisuals = 0;
            weakPointFlashes = 0;
            screenFlashes = 0;
        }

        private static void ResetIfNeeded()
        {
            int current = Time.frameCount;
            if (current == frame)
            {
                return;
            }

            frame = current;
            burstSpawns = 0;
            destroyVisuals = 0;
            comboRushVisuals = 0;
            debrisVisuals = 0;
            weakPointFlashes = 0;
            screenFlashes = 0;
        }

        public static bool TryConsumeBurstSpawn()
        {
            ResetIfNeeded();
            if (burstSpawns >= MaxBurstSpawnsPerFrame)
            {
                return false;
            }

            burstSpawns++;
            return true;
        }

        public static bool TryConsumeDestroyVisual()
        {
            ResetIfNeeded();
            if (destroyVisuals >= MaxDestroyVisualsPerFrame)
            {
                return false;
            }

            destroyVisuals++;
            return true;
        }

        public static bool TryConsumeComboRushVisual()
        {
            ResetIfNeeded();
            if (comboRushVisuals >= MaxComboRushVisualsPerFrame)
            {
                return false;
            }

            comboRushVisuals++;
            return true;
        }

        public static bool TryConsumeDebrisVisual()
        {
            ResetIfNeeded();
            if (debrisVisuals >= MaxDebrisVisualsPerFrame)
            {
                return false;
            }

            debrisVisuals++;
            return true;
        }

        public static bool TryConsumeWeakPointFlash()
        {
            ResetIfNeeded();
            if (weakPointFlashes >= MaxWeakPointFlashesPerFrame)
            {
                return false;
            }

            weakPointFlashes++;
            return true;
        }

        public static bool TryConsumeScreenFlash()
        {
            ResetIfNeeded();
            if (screenFlashes >= MaxScreenFlashesPerFrame)
            {
                return false;
            }

            screenFlashes++;
            return true;
        }
    }
}
