using AlienCrusher.Systems;
using UnityEngine;

namespace AlienCrusher.Gameplay
{
    public partial class DummyDestructibleBlock
    {
        private bool coreExposeBurstArmed = true;

        private void PlayCoreExposeBurstIfNeeded()
        {
            if (!bossCoreExposureActive)
            {
                coreExposeBurstArmed = true;
                return;
            }

            if (!coreExposeBurstArmed)
            {
                return;
            }

            coreExposeBurstArmed = false;
            Vector3 center = transform.position + Vector3.up * Mathf.Max(0.6f, initialScale.y * 0.22f);
            DestructionBreakFeedbackVfx.PlayWeakPointHit(center, bossCore: true);
        }

        private void PlayNamedBreakFeedback(Vector3 hitPoint, float damageRatio, float remainingRatio, bool heavyHit, bool weakPointHit, bool suppressFeedback, bool destroyed)
        {
            DestructionBreakFeedbackVfx.RenameExistingBreakVfx(debrisParticle, smokeParticle);

            if (!suppressFeedback && !isLargeBuilding && !weakPointHit)
            {
                if (destroyed && !IsSmallBuildingTier())
                {
                    DestructionBreakFeedbackVfx.PlayDebrisHeavy(hitPoint, 0.58f);
                }
                else
                {
                    DestructionBreakFeedbackVfx.PlayDebrisLight(hitPoint, destroyed ? 0.72f : (heavyHit ? 0.55f : 0.38f));
                }
            }

            bool skipSmoke = IsSmallBuildingTier() && !isLargeBuilding;
            if (skipSmoke || destroyed || remainingRatio >= 0.72f)
            {
                if (smokeParticle != null)
                {
                    UpdateSmokeFromDamage(0f, forceStop: true);
                }

                DestructionBreakFeedbackVfx.SetSmokeOn(transform, 0f, forceStop: true);
                return;
            }

            if (smokeParticle != null)
            {
                UpdateSmokeFromDamage(damageRatio, forceStop: false);
                return;
            }

            DestructionBreakFeedbackVfx.SetSmokeOn(transform, damageRatio, forceStop: false);
        }
    }
}
