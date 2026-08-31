using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace AlienCrusher.Systems
{
    public enum SmashBreakWeight
    {
        Light = 0,
        Mid = 1,
        Heavy = 2
    }

    [DisallowMultipleComponent]
    public class FeedbackSystem : MonoBehaviour
    {
        [Header("Policy")]
        [SerializeField] private bool allowDotween = true;
        [SerializeField] private bool allowFeel = true;
        [SerializeField] private bool allowAudio = true;

        [Header("Audio Hooks")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] [Range(0f, 1f)] private float audioVolume = 0.8f;
        [SerializeField] private AudioClip hitLightClip;
        [SerializeField] private AudioClip hitMediumClip;
        [SerializeField] private AudioClip hitHeavyClip;
        [SerializeField] private AudioClip breakSmallClip;
        [SerializeField] private AudioClip breakLargeClip;
        [SerializeField] private AudioClip comboRiseClip;
        [SerializeField] private AudioClip routeOpenClip;
        [SerializeField] private AudioClip routeHoldWarningClip;
        [SerializeField] private AudioClip routeBonusClip;
        [SerializeField] private AudioClip bossWarningClip;
        [SerializeField] private AudioClip bossBreakClip;
        [SerializeField] private AudioClip bossDownClip;
        [SerializeField] private AudioClip levelUpClip;
        [SerializeField] private AudioClip failureWarningClip;
        [SerializeField] private AudioClip failureBossClip;

        [Header("Screen Feedback")]
        [SerializeField] [Range(0f, 0.35f)] private float hitFlashAlpha = 0.12f;
        [SerializeField] [Range(0f, 0.45f)] private float destroyFlashAlpha = 0.24f;
        [SerializeField] private float hitCameraImpulse = 0.34f;
        [SerializeField] private float destroyCameraImpulse = 0.7f;
        [SerializeField] private float fovPunchOnHit = 1.25f;
        [SerializeField] private float fovPunchOnDestroy = 2.7f;

        [Header("Debris Burst")]
        [SerializeField] private int particlePoolSize = 10;
        [SerializeField] private Color burstBaseColor = new Color(1f, 0.78f, 0.36f, 1f);
        [SerializeField] private Color burstHotColor = new Color(1f, 0.5f, 0.22f, 1f);

        
        [Header("Combo Rush Feedback")]
        [SerializeField] private Color comboRushFlashColorA = new Color(1f, 0.94f, 0.42f, 1f);
        [SerializeField] private Color comboRushFlashColorB = new Color(1f, 0.36f, 0.14f, 1f);
        [SerializeField] [Range(0f, 0.6f)] private float comboRushFlashAlpha = 0.34f;
        [SerializeField] private float comboRushCameraImpulse = 1.05f;
        [SerializeField] private float comboRushFovPunch = 4.2f;
        [SerializeField] private int comboRushBurstCount = 4;
        [SerializeField] private float comboRushBurstSpread = 2.4f;
        [SerializeField] private Color comboRushRingColorA = new Color(1f, 0.9f, 0.36f, 1f);
        [SerializeField] private Color comboRushRingColorB = new Color(1f, 0.42f, 0.12f, 1f);
        [SerializeField] [Range(0f, 1f)] private float comboRushRingAlpha = 0.52f;
        [SerializeField] private float comboRushRingScaleStart = 0.74f;
        [SerializeField] private float comboRushRingScaleEnd = 1.48f;
        [SerializeField] private float comboRushRingDuration = 0.34f;
        [Header("Retail Frenzy Feedback")]
        [SerializeField] private Color retailFrenzyFlashColorA = new Color(0.24f, 0.86f, 1f, 1f);
        [SerializeField] private Color retailFrenzyFlashColorB = new Color(1f, 0.56f, 0.22f, 1f);
        [SerializeField] [Range(0f, 0.6f)] private float retailFrenzyFlashAlpha = 0.32f;
        [SerializeField] private float retailFrenzyCameraImpulse = 0.94f;
        [SerializeField] private float retailFrenzyFovPunch = 3.8f;
        [SerializeField] private int retailFrenzyBurstCount = 5;
        [SerializeField] private float retailFrenzyBurstSpread = 2.8f;
        [SerializeField] private Color retailFrenzyRingColorA = new Color(0.22f, 0.84f, 1f, 1f);
        [SerializeField] private Color retailFrenzyRingColorB = new Color(1f, 0.68f, 0.24f, 1f);
        [SerializeField] [Range(0f, 1f)] private float retailFrenzyRingAlpha = 0.6f;
        [SerializeField] private float retailFrenzyRingScaleStart = 0.82f;
        [SerializeField] private float retailFrenzyRingScaleEnd = 1.62f;
        [SerializeField] private float retailFrenzyRingDuration = 0.38f;
        [Header("Milestone Feedback")]
        [SerializeField] private Color stageStartFlashColor = new Color(0.48f, 0.86f, 1f, 1f);
        [SerializeField] private Color levelUpOpenFlashColor = new Color(0.9f, 0.74f, 1f, 1f);
        [SerializeField] private Color totalDestructionFlashColor = new Color(1f, 0.75f, 0.3f, 1f);
        [SerializeField] [Range(0f, 0.6f)] private float stageStartFlashAlpha = 0.22f;
        [SerializeField] [Range(0f, 0.6f)] private float levelUpOpenFlashAlpha = 0.26f;
        [SerializeField] [Range(0f, 0.6f)] private float totalDestructionFlashAlpha = 0.38f;
        [SerializeField] private float stageStartCameraImpulse = 0.42f;
        [SerializeField] private float levelUpOpenCameraImpulse = 0.58f;
        [SerializeField] private float totalDestructionCameraImpulse = 1.22f;
        [SerializeField] private float stageStartFovPunch = 1.7f;
        [SerializeField] private float levelUpOpenFovPunch = 2.4f;
        [SerializeField] private float totalDestructionFovPunch = 4.9f;
        [SerializeField] private int stageStartBurstCount = 3;
        [SerializeField] private int levelUpOpenBurstCount = 4;
        [SerializeField] private int totalDestructionBurstCount = 8;
        [SerializeField] private float milestoneBurstSpread = 1.7f;
        [SerializeField] private float milestoneRingIntensity = 0.86f;
        [Header("Counter Surge Feedback")]
        [SerializeField] private Color counterSurgeFlashColorA = new Color(0.38f, 0.92f, 1f, 1f);
        [SerializeField] private Color counterSurgeFlashColorB = new Color(0.14f, 0.52f, 1f, 1f);
        [SerializeField] [Range(0f, 0.45f)] private float counterSurgeFlashAlphaMajor = 0.24f;
        [SerializeField] [Range(0f, 0.35f)] private float counterSurgeFlashAlphaPulse = 0.08f;
        [SerializeField] private float counterSurgeCameraImpulseMajor = 0.34f;
        [SerializeField] private float counterSurgeFovPunchMajor = 1.4f;
        [SerializeField] private int counterSurgeMajorBurstCount = 4;
        [SerializeField] private int counterSurgePulseBurstCount = 2;
        [SerializeField] private float counterSurgeBurstSpread = 1.35f;

        [Header("HUD Warning Feedback")]
        [SerializeField] private Color hudWarningFlashColor = new Color(1f, 0.66f, 0.24f, 1f);
        [SerializeField] private Color hudBossWarningFlashColor = new Color(1f, 0.46f, 0.2f, 1f);
        [SerializeField] [Range(0f, 0.3f)] private float hudWarningFlashAlpha = 0.12f;
        [SerializeField] private float hudWarningCameraImpulse = 0.12f;
        [SerializeField] private float hudWarningFovPunch = 0.45f;

        [Header("Notes")]
        [SerializeField] private string guideline = "Use concise feedback only when it improves clarity and impact.";

        private const float HapticCooldown = 0.05f;

        private Camera mainCamera;
        private CameraFollowSystem cameraFollowSystem;
        private float baseFov = 50f;
        private float lastHapticTime = -10f;

        private Image flashOverlay;
        private Image comboRushRingOverlay;
        private Tween flashTween;
        private Tween comboRushRingTween;
        private Tween fovTween;

        private ParticleSystem[] burstPool;
        private int burstCursor;
        private Material particleMaterial;
        private Sprite comboRushRingSprite;

        public bool AllowDotween => allowDotween;
        public bool AllowFeel => allowFeel;
        public bool AllowAudio => allowAudio;
        public string Guideline => guideline;

        private void Awake()
        {
            ResolveReferences();
            EnsureDraftRhythmClips();
            EnsureFlashOverlay();
            BuildBurstPool();
        }

        private void OnDisable()
        {
            flashTween?.Kill();
            comboRushRingTween?.Kill();
            fovTween?.Kill();
        }

        public void PlayHitFeedback(Vector3 worldPosition, float normalizedImpact)
        {
            PlayHitFeedback(worldPosition, normalizedImpact, forceHeavy: false);
        }

        public void PlayHitFeedback(Vector3 worldPosition, float normalizedImpact, bool forceHeavy)
        {
            normalizedImpact = Mathf.Clamp01(normalizedImpact);

            PlayScreenFlash(Color.Lerp(new Color(1f, 0.95f, 0.82f, 1f), burstBaseColor, normalizedImpact),
                Mathf.Lerp(hitFlashAlpha * 0.65f, hitFlashAlpha, normalizedImpact), 0.045f, 0.12f);

            SpawnBurst(worldPosition, normalizedImpact, heavy: false);
            ApplyCameraFeedback(Mathf.Lerp(hitCameraImpulse * 0.6f, hitCameraImpulse, normalizedImpact),
                Mathf.Lerp(fovPunchOnHit * 0.55f, fovPunchOnHit, normalizedImpact));

            PlayAudio(ResolveHitWeightClip(normalizedImpact, forceHeavy), Mathf.Lerp(0.62f, 0.86f, normalizedImpact), Mathf.Lerp(0.96f, 1.05f, normalizedImpact));
            PlayHaptic(destroyed: false, normalizedImpact);
        }

        public void PlayDestroyFeedback(Vector3 worldPosition, float normalizedImpact)
        {
            normalizedImpact = Mathf.Clamp01(normalizedImpact);

            if (SmashVfxBudget.TryConsumeDestroyVisual())
            {
                PlayScreenFlash(Color.Lerp(burstBaseColor, burstHotColor, normalizedImpact),
                    Mathf.Lerp(destroyFlashAlpha * 0.72f, destroyFlashAlpha, normalizedImpact), 0.05f, 0.17f);

                SpawnBurst(worldPosition, normalizedImpact, heavy: true);
                ApplyCameraFeedback(Mathf.Lerp(destroyCameraImpulse * 0.7f, destroyCameraImpulse, normalizedImpact),
                    Mathf.Lerp(fovPunchOnDestroy * 0.65f, fovPunchOnDestroy, normalizedImpact));
            }

            PlayAudio(normalizedImpact > 0.7f ? breakLargeClip : breakSmallClip, Mathf.Lerp(0.72f, 1f, normalizedImpact), Mathf.Lerp(0.94f, 1.02f, normalizedImpact));
            PlayHaptic(destroyed: true, normalizedImpact);
        }

        public void PlayDestroyFeedback(Vector3 worldPosition, float normalizedImpact, SmashBreakWeight weight)
        {
            if (weight == SmashBreakWeight.Light)
            {
                normalizedImpact = Mathf.Clamp01(normalizedImpact);
                if (SmashVfxBudget.TryConsumeDestroyVisual())
                {
                    PlayScreenFlash(Color.Lerp(new Color(1f, 0.95f, 0.82f, 1f), burstBaseColor, normalizedImpact),
                        Mathf.Lerp(hitFlashAlpha * 0.7f, hitFlashAlpha, normalizedImpact), 0.04f, 0.11f);
                    SpawnBurst(worldPosition, normalizedImpact, heavy: false);
                    ApplyCameraFeedback(Mathf.Lerp(hitCameraImpulse * 0.55f, hitCameraImpulse * 0.85f, normalizedImpact),
                        Mathf.Lerp(fovPunchOnHit * 0.5f, fovPunchOnHit * 0.8f, normalizedImpact));
                }

                PlayAudio(breakSmallClip, Mathf.Lerp(0.68f, 0.88f, normalizedImpact), Mathf.Lerp(0.98f, 1.04f, normalizedImpact));
                PlayHaptic(destroyed: true, normalizedImpact);
                return;
            }

            if (weight == SmashBreakWeight.Heavy)
            {
                PlayDestroyFeedback(worldPosition, 1f);
                return;
            }

            PlayDestroyFeedback(worldPosition, Mathf.Clamp(normalizedImpact, 0.72f, 0.86f));
        }
        public void PlayComboRushFeedback(Vector3 worldCenter, float normalizedIntensity, float radius)
        {
            normalizedIntensity = Mathf.Clamp01(normalizedIntensity);
            if (SmashVfxBudget.TryConsumeComboRushVisual())
            {
                var flashColor = Color.Lerp(comboRushFlashColorA, comboRushFlashColorB, normalizedIntensity);
                var flashAlpha = Mathf.Lerp(comboRushFlashAlpha * 0.65f, comboRushFlashAlpha, normalizedIntensity);
                PlayScreenFlash(flashColor, flashAlpha, 0.035f, 0.22f);
                PlayComboRushRing(normalizedIntensity);
                var bursts = Mathf.Max(2, comboRushBurstCount + Mathf.RoundToInt(normalizedIntensity * 3f));
                var spread = Mathf.Max(0.85f, comboRushBurstSpread + Mathf.Min(radius, 6f) * 0.25f);
                for (var i = 0; i < bursts; i++)
                {
                    var angle = (360f / bursts) * i + Random.Range(-18f, 18f);
                    var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                    var pos = worldCenter + dir * spread * Random.Range(0.55f, 1f);
                    SpawnBurst(pos + Vector3.up * 0.35f, Mathf.Clamp01(0.62f + normalizedIntensity * 0.38f), heavy: true);
                }
                SpawnBurst(worldCenter + Vector3.up * 0.4f, Mathf.Clamp01(0.8f + normalizedIntensity * 0.2f), heavy: true);
                ApplyCameraFeedback(Mathf.Lerp(comboRushCameraImpulse * 0.68f, comboRushCameraImpulse, normalizedIntensity),
                    Mathf.Lerp(comboRushFovPunch * 0.6f, comboRushFovPunch, normalizedIntensity));
            }

            PlayAudio(comboRiseClip, Mathf.Lerp(0.72f, 1f, normalizedIntensity), Mathf.Lerp(1f, 1.12f, normalizedIntensity));
            PlayHaptic(destroyed: true, Mathf.Clamp01(0.75f + normalizedIntensity * 0.25f));
        }

        public void PlaySpherePulseMark(Vector3 worldCenter, float radius)
        {
            SpherePulseMarkDriver.Spawn(worldCenter, radius);
        }

        public void PlayRamBreachMark(Vector3 worldCenter, float radius, Vector3 forward)
        {
            SpherePulseMarkDriver.SpawnRamBreach(worldCenter, radius, forward);
        }

        public void PlaySaucerDashMark(Vector3 worldCenter, float radius)
        {
            SpherePulseMarkDriver.SpawnSaucerDash(worldCenter, radius);
        }

        public void PlaySpikeBurstMark(Vector3 worldCenter, float radius)
        {
            SpherePulseMarkDriver.SpawnSpikeBurst(worldCenter, radius);
        }

        public void PlayCrusherSlamMark(Vector3 worldCenter, float radius)
        {
            SpherePulseMarkDriver.SpawnCrusherSlam(worldCenter, radius);
        }

        public void PlayRetailFrenzyFeedback(Vector3 worldCenter, float normalizedIntensity, float radius)
        {
            normalizedIntensity = Mathf.Clamp01(normalizedIntensity);
            var flashColor = Color.Lerp(retailFrenzyFlashColorA, retailFrenzyFlashColorB, normalizedIntensity);
            var flashAlpha = Mathf.Lerp(retailFrenzyFlashAlpha * 0.68f, retailFrenzyFlashAlpha, normalizedIntensity);
            PlayScreenFlash(flashColor, flashAlpha, 0.03f, 0.2f);
            PlayCustomRing(
                Color.Lerp(retailFrenzyRingColorA, retailFrenzyRingColorB, normalizedIntensity),
                Mathf.Lerp(retailFrenzyRingAlpha * 0.7f, retailFrenzyRingAlpha, normalizedIntensity),
                Mathf.Lerp(retailFrenzyRingScaleStart * 0.9f, retailFrenzyRingScaleStart, normalizedIntensity),
                Mathf.Lerp(retailFrenzyRingScaleEnd * 0.95f, retailFrenzyRingScaleEnd, normalizedIntensity),
                Mathf.Max(0.18f, retailFrenzyRingDuration));

            var bursts = Mathf.Max(3, retailFrenzyBurstCount + Mathf.RoundToInt(normalizedIntensity * 4f));
            var spread = Mathf.Max(1f, retailFrenzyBurstSpread + Mathf.Min(radius, 7f) * 0.22f);
            for (var i = 0; i < bursts; i++)
            {
                var angle = (360f / bursts) * i + Random.Range(-14f, 14f);
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var pos = worldCenter + dir * spread * Random.Range(0.58f, 1f);
                SpawnBurst(pos + Vector3.up * 0.34f, Mathf.Clamp01(0.66f + normalizedIntensity * 0.34f), heavy: true);
            }

            SpawnBurst(worldCenter + Vector3.up * 0.45f, Mathf.Clamp01(0.85f + normalizedIntensity * 0.15f), heavy: true);
            ApplyCameraFeedback(
                Mathf.Lerp(retailFrenzyCameraImpulse * 0.72f, retailFrenzyCameraImpulse, normalizedIntensity),
                Mathf.Lerp(retailFrenzyFovPunch * 0.64f, retailFrenzyFovPunch, normalizedIntensity));
            PlayAudio(comboRiseClip, Mathf.Lerp(0.68f, 0.96f, normalizedIntensity), Mathf.Lerp(1.02f, 1.14f, normalizedIntensity));
            PlayHaptic(destroyed: true, Mathf.Clamp01(0.72f + normalizedIntensity * 0.28f));
        }

        public void PlayStageStartFeedback(Vector3 center)
        {
            PlayMilestoneFeedback(
                center,
                stageStartFlashColor,
                stageStartFlashAlpha,
                stageStartCameraImpulse,
                stageStartFovPunch,
                stageStartBurstCount,
                Mathf.Clamp01(milestoneRingIntensity * 0.78f),
                Mathf.Max(0.45f, milestoneBurstSpread * 0.9f),
                heavyBursts: false,
                destroyedHaptic: false,
                hapticIntensity: 0.3f);
        }

        public void PlayLevelUpOpenFeedback(Vector3 center)
        {
            PlayMilestoneFeedback(
                center,
                levelUpOpenFlashColor,
                levelUpOpenFlashAlpha * 0.92f,
                levelUpOpenCameraImpulse * 0.82f,
                levelUpOpenFovPunch * 0.86f,
                levelUpOpenBurstCount,
                Mathf.Clamp01(milestoneRingIntensity * 0.82f),
                Mathf.Max(0.48f, milestoneBurstSpread * 0.88f),
                heavyBursts: false,
                destroyedHaptic: false,
                hapticIntensity: 0.32f);
            PlayAudio(levelUpClip, 0.86f, 1.04f);
        }

        public void PlayTotalDestructionFeedback(Vector3 center, float intensity, bool playAudio = true)
        {
            intensity = Mathf.Clamp01(intensity);
            PlayMilestoneFeedback(
                center,
                totalDestructionFlashColor,
                Mathf.Lerp(totalDestructionFlashAlpha * 0.8f, totalDestructionFlashAlpha, intensity),
                Mathf.Lerp(totalDestructionCameraImpulse * 0.75f, totalDestructionCameraImpulse, intensity),
                Mathf.Lerp(totalDestructionFovPunch * 0.72f, totalDestructionFovPunch, intensity),
                Mathf.Max(1, totalDestructionBurstCount + Mathf.RoundToInt(intensity * 4f)),
                Mathf.Lerp(Mathf.Clamp01(milestoneRingIntensity), 1f, intensity),
                Mathf.Lerp(milestoneBurstSpread * 1.15f, milestoneBurstSpread * 1.9f, intensity),
                heavyBursts: true,
                destroyedHaptic: true,
                hapticIntensity: Mathf.Lerp(0.7f, 1f, intensity));
            if (playAudio)
            {
                PlayAudio(routeBonusClip, Mathf.Lerp(0.78f, 1f, intensity), Mathf.Lerp(0.96f, 1.04f, intensity));
            }
        }

        public void PlayBossDownFeedback(Vector3 center, float intensity)
        {
            intensity = Mathf.Clamp01(intensity);
            PlayTotalDestructionFeedback(center, intensity);
            PlayAudio(bossDownClip, Mathf.Lerp(0.82f, 1f, intensity), Mathf.Lerp(0.88f, 0.98f, intensity));
        }


        public void PlayCounterSurgeFeedback(Vector3 center, float normalizedIntensity, bool major)
        {
            normalizedIntensity = Mathf.Clamp01(normalizedIntensity);

            var flashColor = Color.Lerp(counterSurgeFlashColorA, counterSurgeFlashColorB, normalizedIntensity);
            var flashAlpha = major
                ? Mathf.Lerp(counterSurgeFlashAlphaMajor * 0.72f, counterSurgeFlashAlphaMajor, normalizedIntensity)
                : Mathf.Lerp(counterSurgeFlashAlphaPulse * 0.6f, counterSurgeFlashAlphaPulse, normalizedIntensity);
            var rise = major ? 0.035f : 0.02f;
            var fall = major ? 0.16f : 0.09f;
            PlayScreenFlash(flashColor, flashAlpha, rise, fall);

            if (major)
            {
                PlayComboRushRing(Mathf.Lerp(0.55f, 0.9f, normalizedIntensity));
            }

            var burstCount = major
                ? Mathf.Max(1, counterSurgeMajorBurstCount + Mathf.RoundToInt(normalizedIntensity * 2f))
                : Mathf.Max(1, counterSurgePulseBurstCount);
            var spread = Mathf.Max(0.45f, counterSurgeBurstSpread) * (major ? 1f : 0.72f);
            for (var i = 0; i < burstCount; i++)
            {
                var angle = (360f / burstCount) * i + Random.Range(-18f, 18f);
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var pos = center + dir * spread * Random.Range(0.58f, 1f);
                SpawnBurst(pos + Vector3.up * 0.3f, Mathf.Clamp01(0.52f + normalizedIntensity * 0.4f), heavy: major);
            }

            if (major)
            {
                ApplyCameraFeedback(
                    Mathf.Lerp(counterSurgeCameraImpulseMajor * 0.75f, counterSurgeCameraImpulseMajor, normalizedIntensity),
                    Mathf.Lerp(counterSurgeFovPunchMajor * 0.72f, counterSurgeFovPunchMajor, normalizedIntensity));
                PlayAudio(routeOpenClip, Mathf.Lerp(0.72f, 0.96f, normalizedIntensity), Mathf.Lerp(1f, 1.08f, normalizedIntensity));
                PlayHaptic(destroyed: false, Mathf.Lerp(0.35f, 0.7f, normalizedIntensity));
            }
        }

        public void PlayDroneBreakFeedback(Vector3 center, float normalizedIntensity, bool swarmBroken)
        {
            normalizedIntensity = Mathf.Clamp01(normalizedIntensity);

            var flashColor = Color.Lerp(
                new Color(1f, 0.88f, 0.42f, 1f),
                new Color(1f, 0.34f, 0.14f, 1f),
                normalizedIntensity);
            var peakAlpha = swarmBroken
                ? Mathf.Lerp(0.16f, 0.28f, normalizedIntensity)
                : Mathf.Lerp(0.08f, 0.16f, normalizedIntensity);
            PlayScreenFlash(flashColor, peakAlpha, 0.025f, swarmBroken ? 0.16f : 0.1f);

            var burstCount = swarmBroken
                ? Mathf.RoundToInt(Mathf.Lerp(5f, 8f, normalizedIntensity))
                : Mathf.RoundToInt(Mathf.Lerp(3f, 5f, normalizedIntensity));
            var spread = swarmBroken
                ? Mathf.Lerp(0.95f, 1.65f, normalizedIntensity)
                : Mathf.Lerp(0.45f, 0.9f, normalizedIntensity);

            for (var i = 0; i < Mathf.Max(2, burstCount); i++)
            {
                var angle = (360f / Mathf.Max(1, burstCount)) * i + Random.Range(-24f, 24f);
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var pos = center + dir * spread * Random.Range(0.45f, 1f);
                SpawnBurst(pos + Vector3.up * Random.Range(0.15f, 0.5f), Mathf.Clamp01(0.68f + normalizedIntensity * 0.3f), heavy: true);
            }

            SpawnBurst(center + Vector3.up * 0.28f, Mathf.Clamp01(0.82f + normalizedIntensity * 0.18f), heavy: true);

            ApplyCameraFeedback(
                swarmBroken ? Mathf.Lerp(0.22f, 0.54f, normalizedIntensity) : Mathf.Lerp(0.08f, 0.2f, normalizedIntensity),
                swarmBroken ? Mathf.Lerp(0.8f, 1.9f, normalizedIntensity) : Mathf.Lerp(0.25f, 0.7f, normalizedIntensity));

            PlayAudio(swarmBroken ? bossBreakClip : bossWarningClip, Mathf.Lerp(0.62f, 0.95f, normalizedIntensity), swarmBroken ? 1f : 0.94f);
            PlayHaptic(destroyed: swarmBroken, swarmBroken ? Mathf.Lerp(0.45f, 0.82f, normalizedIntensity) : Mathf.Lerp(0.2f, 0.4f, normalizedIntensity));
        }

        public void PlayWeakPointCriticalFeedback(Vector3 center, float normalizedIntensity, bool bossCore)
        {
            normalizedIntensity = Mathf.Clamp01(normalizedIntensity);

            var flashColor = bossCore
                ? Color.Lerp(new Color(0.82f, 0.96f, 1f, 1f), new Color(1f, 0.4f, 0.14f, 1f), normalizedIntensity)
                : Color.Lerp(new Color(1f, 0.92f, 0.48f, 1f), new Color(1f, 0.46f, 0.14f, 1f), normalizedIntensity);
            var flashAlpha = bossCore
                ? Mathf.Lerp(0.14f, 0.26f, normalizedIntensity)
                : Mathf.Lerp(0.08f, 0.18f, normalizedIntensity);
            PlayScreenFlash(flashColor, flashAlpha, 0.02f, bossCore ? 0.18f : 0.12f);
            DestructionBreakFeedbackVfx.PlayWeakPointHit(center, bossCore);

            var ringIntensity = bossCore ? Mathf.Lerp(0.68f, 1f, normalizedIntensity) : Mathf.Lerp(0.45f, 0.8f, normalizedIntensity);
            PlayComboRushRing(ringIntensity);
            if (bossCore)
            {
                PlayCustomRing(
                    Color.Lerp(new Color(1f, 0.78f, 0.3f, 1f), new Color(1f, 0.32f, 0.08f, 1f), normalizedIntensity),
                    Mathf.Lerp(0.2f, 0.34f, normalizedIntensity),
                    Mathf.Lerp(0.72f, 0.9f, normalizedIntensity),
                    Mathf.Lerp(1.34f, 1.72f, normalizedIntensity),
                    Mathf.Lerp(0.22f, 0.3f, normalizedIntensity));
            }

            var burstCount = bossCore
                ? Mathf.RoundToInt(Mathf.Lerp(5f, 9f, normalizedIntensity))
                : Mathf.RoundToInt(Mathf.Lerp(3f, 6f, normalizedIntensity));
            var spread = bossCore
                ? Mathf.Lerp(0.85f, 1.8f, normalizedIntensity)
                : Mathf.Lerp(0.4f, 1.05f, normalizedIntensity);

            for (var i = 0; i < Mathf.Max(2, burstCount); i++)
            {
                var angle = (360f / Mathf.Max(1, burstCount)) * i + Random.Range(-20f, 20f);
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var pos = center + dir * spread * Random.Range(0.42f, 1f);
                SpawnBurst(pos + Vector3.up * Random.Range(0.15f, 0.48f), Mathf.Clamp01(0.75f + normalizedIntensity * 0.25f), heavy: true);
            }

            SpawnBurst(center + Vector3.up * 0.24f, Mathf.Clamp01(0.88f + normalizedIntensity * 0.12f), heavy: true);

            ApplyCameraFeedback(
                bossCore ? Mathf.Lerp(0.22f, 0.58f, normalizedIntensity) : Mathf.Lerp(0.08f, 0.24f, normalizedIntensity),
                bossCore ? Mathf.Lerp(0.95f, 2.2f, normalizedIntensity) : Mathf.Lerp(0.25f, 0.75f, normalizedIntensity));

            PlayAudio(bossCore ? bossBreakClip : hitHeavyClip, Mathf.Lerp(0.62f, 1f, normalizedIntensity), bossCore ? 0.96f : 1.05f);
            PlayHaptic(destroyed: bossCore, bossCore ? Mathf.Lerp(0.44f, 0.84f, normalizedIntensity) : Mathf.Lerp(0.18f, 0.36f, normalizedIntensity));
        }

        public void PlayHudWarningFeedback(bool bossRelated, float intensity = 1f)
        {
            intensity = Mathf.Clamp01(intensity);
            var flashColor = bossRelated
                ? Color.Lerp(hudWarningFlashColor, hudBossWarningFlashColor, 0.65f + intensity * 0.35f)
                : hudWarningFlashColor;
            var alpha = Mathf.Lerp(hudWarningFlashAlpha * 0.65f, hudWarningFlashAlpha, intensity);
            PlayScreenFlash(flashColor, alpha, 0.018f, 0.12f);
            ApplyCameraFeedback(
                Mathf.Lerp(hudWarningCameraImpulse * 0.6f, hudWarningCameraImpulse, intensity),
                Mathf.Lerp(hudWarningFovPunch * 0.55f, hudWarningFovPunch, intensity));
            PlayAudio(bossRelated ? bossWarningClip : routeHoldWarningClip, Mathf.Lerp(0.45f, 0.72f, intensity), bossRelated ? 0.92f : 1f);
        }

        public void PlayRouteOpenCue(float intensity = 0.62f)
        {
            intensity = Mathf.Clamp01(intensity);
            PlayAudio(routeOpenClip, Mathf.Lerp(0.72f, 0.96f, intensity), Mathf.Lerp(1f, 1.08f, intensity));
        }

        public void PlayBossWarningCue(float intensity = 0.7f)
        {
            intensity = Mathf.Clamp01(intensity);
            PlayAudio(bossWarningClip, Mathf.Lerp(0.55f, 0.82f, intensity), Mathf.Lerp(0.9f, 0.98f, intensity));
        }

        public void PlayBossBreakCue(float intensity = 0.78f)
        {
            intensity = Mathf.Clamp01(intensity);
            PlayAudio(bossBreakClip, Mathf.Lerp(0.68f, 0.94f, intensity), Mathf.Lerp(0.96f, 1.02f, intensity));
        }

        public void PlayFailureBeatFeedback(Vector3 center, bool bossRelated, float intensity = 1f)
        {
            intensity = Mathf.Clamp01(intensity);
            var flashColor = bossRelated
                ? Color.Lerp(hudBossWarningFlashColor, new Color(1f, 0.1f, 0.06f, 1f), Mathf.Lerp(0.24f, 0.46f, intensity))
                : Color.Lerp(hudWarningFlashColor, new Color(1f, 0.22f, 0.08f, 1f), Mathf.Lerp(0.18f, 0.38f, intensity));
            var alpha = Mathf.Lerp(hudWarningFlashAlpha * 1.1f, hudWarningFlashAlpha * 1.65f, intensity);
            PlayScreenFlash(flashColor, alpha, 0.018f, 0.22f);
            PlayCustomRing(
                flashColor,
                Mathf.Lerp(0.22f, 0.42f, intensity),
                Mathf.Lerp(0.64f, 0.82f, intensity),
                Mathf.Lerp(1.18f, 1.72f, intensity),
                Mathf.Lerp(0.2f, 0.32f, intensity));

            SpawnBurst(center, Mathf.Lerp(0.58f, 0.9f, intensity), heavy: bossRelated);
            ApplyCameraFeedback(
                Mathf.Lerp(hudWarningCameraImpulse * 1.15f, hudWarningCameraImpulse * 2.6f, intensity),
                Mathf.Lerp(hudWarningFovPunch * 1.1f, hudWarningFovPunch * 2.4f, intensity));
            PlayAudio(ResolveFailureClip(bossRelated), Mathf.Lerp(0.58f, 0.9f, intensity), bossRelated ? 0.84f : 0.92f);
            PlayHaptic(destroyed: bossRelated, Mathf.Lerp(0.34f, 0.68f, intensity));
        }

        private AudioClip ResolveFailureClip(bool bossRelated)
        {
            if (bossRelated && failureBossClip != null)
            {
                return failureBossClip;
            }

            if (!bossRelated && failureWarningClip != null)
            {
                return failureWarningClip;
            }

            if (failureWarningClip != null)
            {
                return failureWarningClip;
            }

            return bossRelated ? bossWarningClip : routeHoldWarningClip;
        }

        private void PlayMilestoneFeedback(
            Vector3 center,
            Color flashColor,
            float flashAlpha,
            float cameraImpulse,
            float fovPunch,
            int burstCount,
            float ringIntensity,
            float burstSpread,
            bool heavyBursts,
            bool destroyedHaptic,
            float hapticIntensity)
        {
            flashAlpha = Mathf.Clamp01(flashAlpha);
            ringIntensity = Mathf.Clamp01(ringIntensity);
            hapticIntensity = Mathf.Clamp01(hapticIntensity);

            PlayScreenFlash(flashColor, flashAlpha, 0.04f, 0.16f);
            PlayComboRushRing(ringIntensity);

            var safeCount = Mathf.Max(1, burstCount);
            var spread = Mathf.Max(0.35f, burstSpread);
            for (var i = 0; i < safeCount; i++)
            {
                var angle = (360f / safeCount) * i + Random.Range(-22f, 22f);
                var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                var pos = center + dir * spread * Random.Range(0.5f, 1f);
                SpawnBurst(pos + Vector3.up * 0.34f, Mathf.Clamp01(0.55f + hapticIntensity * 0.45f), heavyBursts);
            }

            SpawnBurst(center + Vector3.up * 0.42f, Mathf.Clamp01(0.7f + hapticIntensity * 0.3f), heavyBursts);
            ApplyCameraFeedback(Mathf.Max(0f, cameraImpulse), Mathf.Max(0f, fovPunch));
            PlayHaptic(destroyedHaptic, hapticIntensity);
        }

        private void ResolveReferences()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
                if (cameras.Length > 0)
                {
                    mainCamera = cameras[0];
                }
            }

            if (mainCamera != null)
            {
                baseFov = mainCamera.fieldOfView;
            }

            cameraFollowSystem = UnityEngine.Object.FindAnyObjectByType<CameraFollowSystem>();
            EnsureAudioSource();
        }

        private void EnsureDraftRhythmClips()
        {
            routeOpenClip = CoalesceDraftClip(routeOpenClip, "Audio/SFX/Skills/SFX_Route_Open", "Assets/Audio/SFX/Skills/SFX_Route_Open.wav");
            routeHoldWarningClip = CoalesceDraftClip(routeHoldWarningClip, "Audio/SFX/Skills/SFX_Route_HoldWarning", "Assets/Audio/SFX/Skills/SFX_Route_HoldWarning.wav");
            routeBonusClip = CoalesceDraftClip(routeBonusClip, "Audio/SFX/Skills/SFX_Route_Bonus", "Assets/Audio/SFX/Skills/SFX_Route_Bonus.wav");
            failureWarningClip = CoalesceDraftClip(failureWarningClip, "Audio/SFX/Failure/SFX_Failure_Warning", "Assets/Audio/SFX/Failure/SFX_Failure_Warning.wav");
            failureBossClip = CoalesceDraftClip(failureBossClip, "Audio/SFX/Failure/SFX_Failure_Boss", "Assets/Audio/SFX/Failure/SFX_Failure_Boss.wav");
            hitLightClip = CoalesceDraftClip(hitLightClip, "Audio/SFX/Impact/SFX_Hit_Light", "Assets/Audio/SFX/Impact/SFX_Hit_Light.wav");
            hitMediumClip = CoalesceDraftClip(hitMediumClip, "Audio/SFX/Impact/SFX_Hit_Medium", "Assets/Audio/SFX/Impact/SFX_Hit_Medium.wav");
            hitHeavyClip = CoalesceDraftClip(hitHeavyClip, "Audio/SFX/Impact/SFX_Hit_Heavy", "Assets/Audio/SFX/Impact/SFX_Hit_Heavy.wav");
            breakSmallClip = CoalesceDraftClip(breakSmallClip, "Audio/SFX/Impact/SFX_Break_Small", "Assets/Audio/SFX/Impact/SFX_Break_Small.wav");
            breakLargeClip = CoalesceDraftClip(breakLargeClip, "Audio/SFX/Impact/SFX_Break_LargeCollapse", "Assets/Audio/SFX/Impact/SFX_Break_LargeCollapse.wav");
            bossWarningClip = CoalesceDraftClip(bossWarningClip, "Audio/SFX/Boss/SFX_Boss_Warning", "Assets/Audio/SFX/Boss/SFX_Boss_Warning.wav");
            bossBreakClip = CoalesceDraftClip(bossBreakClip, "Audio/SFX/Boss/SFX_Boss_Break", "Assets/Audio/SFX/Boss/SFX_Boss_Break.wav");
            bossDownClip = CoalesceDraftClip(bossDownClip, "Audio/SFX/Boss/SFX_Boss_Down", "Assets/Audio/SFX/Boss/SFX_Boss_Down.wav");
            comboRiseClip = CoalesceDraftClip(comboRiseClip, "Audio/SFX/Skills/SFX_Combo_Rise", "Assets/Audio/SFX/Skills/SFX_Combo_Rise.wav");
            levelUpClip = CoalesceDraftClip(levelUpClip, "Audio/SFX/UI/SFX_LevelUp_Open", "Assets/Audio/SFX/UI/SFX_LevelUp_Open.wav");
        }

        private AudioClip ResolveHitWeightClip(float normalizedImpact, bool forceHeavy)
        {
            if (forceHeavy || normalizedImpact > 0.82f)
            {
                return hitHeavyClip;
            }

            return normalizedImpact > 0.62f ? hitMediumClip : hitLightClip;
        }

        private static AudioClip CoalesceDraftClip(AudioClip assigned, string resourcesPath, string assetPath)
        {
            if (assigned != null)
            {
                return assigned;
            }

            return LoadDraftClip(resourcesPath, assetPath);
        }

        private static AudioClip LoadDraftClip(string resourcesPath, string assetPath)
        {
            var clip = Resources.Load<AudioClip>(resourcesPath);
            if (clip != null)
            {
                return clip;
            }

#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
#else
            _ = assetPath;
            return null;
#endif
        }

        private void EnsureAudioSource()
        {
            if (!allowAudio || audioSource != null)
            {
                return;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        private void PlayAudio(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (!allowAudio || clip == null)
            {
                return;
            }

            EnsureAudioSource();
            if (audioSource == null)
            {
                return;
            }

            audioSource.pitch = Mathf.Clamp(pitch, 0.5f, 1.5f);
            audioSource.PlayOneShot(clip, Mathf.Clamp01(audioVolume * Mathf.Max(0f, volumeScale)));
        }

        private void EnsureFlashOverlay()
        {
            Canvas canvas = null;
            var allCanvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            for (var i = 0; i < allCanvas.Length; i++)
            {
                if (allCanvas[i].name == "Canvas_Dummy")
                {
                    canvas = allCanvas[i];
                    break;
                }
            }

            canvas ??= UnityEngine.Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                return;
            }

            var existing = FindDirectChild(canvas.transform, "FeedbackFlashOverlay");
            if (existing == null)
            {
                var go = new GameObject("FeedbackFlashOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(canvas.transform, false);
                existing = go.transform;
            }

            var rect = existing as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            flashOverlay = existing.GetComponent<Image>();
            if (flashOverlay == null)
            {
                flashOverlay = existing.gameObject.AddComponent<Image>();
            }

            flashOverlay.raycastTarget = false;
            flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            flashOverlay.transform.SetAsLastSibling();

            EnsureComboRushRingOverlay(canvas.transform);
        }

        private void BuildBurstPool()
        {
            if (particlePoolSize <= 0 || burstPool != null)
            {
                return;
            }

            var root = new GameObject("_FeedbackBurstPool").transform;
            root.SetParent(transform, false);

            burstPool = new ParticleSystem[particlePoolSize];
            for (var i = 0; i < particlePoolSize; i++)
            {
                var go = new GameObject($"Burst_{i:00}", typeof(ParticleSystem));
                go.transform.SetParent(root, false);

                var ps = go.GetComponent<ParticleSystem>();
                ConfigureBurstParticle(ps);
                burstPool[i] = ps;
            }
        }

        private void ConfigureBurstParticle(ParticleSystem ps)
        {
            if (ps == null)
            {
                return;
            }

            // Ensure module properties are changed only on a fully stopped system.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 180;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.44f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.6f, 7.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.24f);
            main.startColor = burstBaseColor;
            main.gravityModifier = 0.9f;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 28f;
            shape.radius = 0.18f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.92f, 0.84f), 0f),
                    new GradientColorKey(new Color(1f, 0.58f, 0.28f), 0.45f),
                    new GradientColorKey(new Color(0.25f, 0.22f, 0.2f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.9f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.14f;
            noise.frequency = 0.5f;
            noise.scrollSpeed = 0.18f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.material = GetParticleMaterial();
            }
        }

        private Material GetParticleMaterial()
        {
            if (particleMaterial != null)
            {
                return particleMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Sprites/Default");

            if (shader == null)
            {
                return null;
            }

            particleMaterial = new Material(shader)
            {
                name = "M_Runtime_FeedbackBurst"
            };

            if (particleMaterial.HasProperty("_Surface")) particleMaterial.SetFloat("_Surface", 1f);
            if (particleMaterial.HasProperty("_Blend")) particleMaterial.SetFloat("_Blend", 0f);

            return particleMaterial;
        }

        private void SpawnBurst(Vector3 worldPosition, float normalizedImpact, bool heavy)
        {
            if (burstPool == null || burstPool.Length == 0 || !SmashVfxBudget.TryConsumeBurstSpawn())
            {
                return;
            }

            var ps = burstPool[burstCursor];
            burstCursor = (burstCursor + 1) % burstPool.Length;
            if (ps == null)
            {
                return;
            }

            var main = ps.main;
            var color = Color.Lerp(burstBaseColor, burstHotColor, normalizedImpact);
            main.startColor = color;
            main.startSpeed = heavy
                ? new ParticleSystem.MinMaxCurve(5.2f, 11.2f)
                : new ParticleSystem.MinMaxCurve(2.4f, 6.8f);
            main.startSize = heavy
                ? new ParticleSystem.MinMaxCurve(0.11f, 0.34f)
                : new ParticleSystem.MinMaxCurve(0.08f, 0.22f);

            ps.transform.position = worldPosition + Vector3.up * 0.35f;
            ps.transform.rotation = Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f);

            var count = heavy
                ? Mathf.RoundToInt(Mathf.Lerp(16f, 36f, normalizedImpact))
                : Mathf.RoundToInt(Mathf.Lerp(6f, 15f, normalizedImpact));

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Emit(Mathf.Max(1, count));
            ps.Play();
        }

        private void PlayScreenFlash(Color color, float peakAlpha, float riseDuration, float fallDuration)
        {
            if (!allowDotween || flashOverlay == null || !SmashVfxBudget.TryConsumeScreenFlash())
            {
                return;
            }

            flashTween?.Kill();

            color.a = 0f;
            flashOverlay.color = color;

            flashTween = DOTween.Sequence()
                .Append(flashOverlay.DOFade(Mathf.Clamp01(peakAlpha), riseDuration))
                .Append(flashOverlay.DOFade(0f, fallDuration))
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        private void PlayComboRushRing(float normalizedIntensity)
        {
            if (!allowDotween || comboRushRingOverlay == null)
            {
                return;
            }

            comboRushRingTween?.Kill();

            var rect = comboRushRingOverlay.rectTransform;
            var edge = Mathf.Max(Screen.width, Screen.height);
            var baseSize = Mathf.Max(900f, edge * 1.35f);
            rect.sizeDelta = new Vector2(baseSize, baseSize);

            var startScale = Mathf.Lerp(comboRushRingScaleStart * 0.85f, comboRushRingScaleStart, normalizedIntensity);
            var endScale = Mathf.Lerp(comboRushRingScaleEnd * 0.95f, comboRushRingScaleEnd, normalizedIntensity);
            rect.localScale = Vector3.one * startScale;

            var ringColor = Color.Lerp(comboRushRingColorA, comboRushRingColorB, normalizedIntensity);
            ringColor.a = 0f;
            comboRushRingOverlay.color = ringColor;

            var peak = Mathf.Lerp(comboRushRingAlpha * 0.65f, comboRushRingAlpha, normalizedIntensity);
            var duration = Mathf.Max(0.15f, comboRushRingDuration);
            comboRushRingTween = DOTween.Sequence()
                .Append(comboRushRingOverlay.DOFade(peak, duration * 0.32f))
                .Join(rect.DOScale(endScale, duration).SetEase(Ease.OutCubic))
                .Append(comboRushRingOverlay.DOFade(0f, duration * 0.68f))
                .SetUpdate(true);
        }

        private void PlayCustomRing(Color ringColor, float peakAlpha, float startScale, float endScale, float duration)
        {
            if (!allowDotween || comboRushRingOverlay == null)
            {
                return;
            }

            comboRushRingTween?.Kill();

            var rect = comboRushRingOverlay.rectTransform;
            var edge = Mathf.Max(Screen.width, Screen.height);
            var baseSize = Mathf.Max(900f, edge * 1.35f);
            rect.sizeDelta = new Vector2(baseSize, baseSize);
            rect.localScale = Vector3.one * Mathf.Max(0.1f, startScale);

            ringColor.a = 0f;
            comboRushRingOverlay.color = ringColor;

            comboRushRingTween = DOTween.Sequence()
                .Append(comboRushRingOverlay.DOFade(Mathf.Clamp01(peakAlpha), duration * 0.32f))
                .Join(rect.DOScale(Mathf.Max(0.15f, endScale), duration).SetEase(Ease.OutCubic))
                .Append(comboRushRingOverlay.DOFade(0f, duration * 0.68f))
                .SetUpdate(true);
        }

        private void EnsureComboRushRingOverlay(Transform canvasTransform)
        {
            if (canvasTransform == null)
            {
                return;
            }

            var existing = FindDirectChild(canvasTransform, "FeedbackComboRushRing");
            if (existing == null)
            {
                var go = new GameObject("FeedbackComboRushRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(canvasTransform, false);
                existing = go.transform;
            }

            var rect = existing as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(1200f, 1200f);
                rect.localScale = Vector3.one;
            }

            comboRushRingOverlay = existing.GetComponent<Image>();
            if (comboRushRingOverlay == null)
            {
                comboRushRingOverlay = existing.gameObject.AddComponent<Image>();
            }

            comboRushRingOverlay.sprite = GetOrCreateComboRushRingSprite();
            comboRushRingOverlay.preserveAspect = true;
            comboRushRingOverlay.type = Image.Type.Simple;
            comboRushRingOverlay.raycastTarget = false;
            comboRushRingOverlay.color = new Color(1f, 1f, 1f, 0f);
            comboRushRingOverlay.transform.SetAsLastSibling();
        }

        private Sprite GetOrCreateComboRushRingSprite()
        {
            if (comboRushRingSprite != null)
            {
                return comboRushRingSprite;
            }

            const int size = 256;
            const float outerRadius = 0.48f;
            const float innerRadius = 0.33f;
            const float feather = 0.03f;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "TX_Runtime_ComboRushRing",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            var half = size * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nx = (x + 0.5f - half) / half;
                    var ny = (y + 0.5f - half) / half;
                    var distance = Mathf.Sqrt(nx * nx + ny * ny);

                    var outerMask = 1f - Mathf.Clamp01((distance - outerRadius) / feather);
                    var innerMask = Mathf.Clamp01((distance - innerRadius) / feather);
                    var alpha = Mathf.Clamp01(outerMask * innerMask);
                    var a = (byte)Mathf.RoundToInt(alpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            comboRushRingSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            comboRushRingSprite.name = "SP_Runtime_ComboRushRing";
            return comboRushRingSprite;
        }
        private void ApplyCameraFeedback(float impulseMagnitude, float fovPunch)
        {
            cameraFollowSystem ??= UnityEngine.Object.FindAnyObjectByType<CameraFollowSystem>();
            cameraFollowSystem?.AddImpulse(impulseMagnitude);

            if (!allowDotween)
            {
                return;
            }

            if (mainCamera == null)
            {
                ResolveReferences();
            }

            if (mainCamera == null)
            {
                return;
            }

            fovTween?.Kill();
            var targetFov = baseFov + Mathf.Max(0f, fovPunch);
            fovTween = DOTween.Sequence()
                .Append(mainCamera.DOFieldOfView(targetFov, 0.05f))
                .Append(mainCamera.DOFieldOfView(baseFov, 0.16f))
                .SetEase(Ease.OutSine)
                .SetUpdate(true);
        }

        private void PlayHaptic(bool destroyed, float normalizedImpact)
        {
            if (!allowFeel)
            {
                return;
            }

            if (Time.unscaledTime - lastHapticTime < HapticCooldown)
            {
                return;
            }

            lastHapticTime = Time.unscaledTime;

#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
            var preset = destroyed
                ? HapticPatterns.PresetType.HeavyImpact
                : (normalizedImpact > 0.62f ? HapticPatterns.PresetType.MediumImpact : HapticPatterns.PresetType.LightImpact);
            HapticPatterns.PlayPreset(preset);
#elif (UNITY_ANDROID || UNITY_IOS)
            Handheld.Vibrate();
#endif
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }
    }
}



