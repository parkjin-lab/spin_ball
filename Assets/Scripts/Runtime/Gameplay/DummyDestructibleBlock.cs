using System.Collections;
using System.Collections.Generic;
using AlienCrusher.Systems;
using DG.Tweening;
using UnityEngine;

namespace AlienCrusher.Gameplay
{
    [DisallowMultipleComponent]
    public class DummyDestructibleBlock : MonoBehaviour
    {
        public enum SmallPropBreakKind
        {
            None = 0,
            Mailbox = 1,
            Fence = 2,
            Shed = 3,
            ShopAwning = 4,
            ShopSign = 5,
            Kiosk = 6,
            Bench = 7,
            BusStop = 8,
            Vending = 9
        }

        public struct SmallPropBreakInfo
        {
            public SmallPropBreakKind Kind;
            public Vector3 Position;
            public float Impact01;
        }

        public static event System.Action<SmallPropBreakInfo> SmallPropDestroyed;

        private enum SmallPropStyle
        {
            Default = 0,
            Mailbox = 1,
            Fence = 2,
            Shed = 3,
            ShopAwning = 4,
            ShopSign = 5,
            Kiosk = 6,
            Bench = 7,
            BusStop = 8,
            Vending = 9
        }

        public enum StageEncounterRole
        {
            Standard = 0,
            EliteWeakPoint = 1,
            BossSentinel = 2
        }

        [Header("Durability")]
        [SerializeField] private int maxHp = 3;
        [SerializeField] private float durabilityPerHp = 55f;
        [SerializeField] private int scoreOnDestroyed = 120;

        [Header("Physics To Damage")]
        [SerializeField] private float minimumImpact = 1.8f;
        [SerializeField] private float heavyImpact = 11f;
        [SerializeField] private float minDamagePerHit = 5f;
        [SerializeField] private float maxDamagePerHit = 240f;
        [SerializeField] private float linearEnergyToDamage = 0.2f;
        [SerializeField] private float angularEnergyToDamage = 0.14f;
        [SerializeField] private float impulseToDamage = 0.2f;
        [SerializeField] private float hitScoreMultiplier = 1.15f;

        [Header("Visual")]
        [SerializeField] private Color healthyColor = new Color(0.78f, 0.75f, 0.7f, 1f);
        [SerializeField] private Color hitColor = new Color(1f, 0.58f, 0.26f, 1f);
        [SerializeField] private Color destroyedColor = new Color(0.16f, 0.14f, 0.12f, 1f);

        [Header("Feedback")]
        [SerializeField] private float hitPunchScale = 0.16f;
        [SerializeField] private float destroyShrinkScale = 0.2f;
        [SerializeField] private float destroyDisableDelay = 0.18f;

        [Header("Weak Point")]
        [SerializeField] private bool enableWeakPointCritical = true;
        [SerializeField] private bool weakPointLargeBuildingsOnly = false;
        [SerializeField] [Range(1.1f, 3.5f)] private float weakPointDamageMultiplier = 1.95f;
        [SerializeField] private int weakPointHitBonusScore = 36;
        [SerializeField] [Range(0.06f, 0.45f)] private float weakPointRadiusRatio = 0.14f;
        [SerializeField] [Range(0.8f, 1.6f)] private float weakPointHitTolerance = 1.1f;
        [SerializeField] private bool moveWeakPointAfterCriticalHit = true;
        [SerializeField] private Color weakPointColorA = new Color(1f, 0.82f, 0.24f, 1f);
        [SerializeField] private Color weakPointColorB = new Color(1f, 0.36f, 0.12f, 1f);
        [SerializeField] private float weakPointPulseSpeed = 6.2f;
        [SerializeField] private int bossCoreCriticalBonusScore = 180;

        [Header("Large Building FX")]
        [SerializeField] private int largeBuildingMinHp = 5;
        [SerializeField] private float largeBuildingMinVolume = 55f;
        [SerializeField] private float largeBuildingMinHeight = 3.6f;
        [SerializeField] [Range(0f, 0.4f)] private float maxDamageShrinkRatio = 0.22f;
        [SerializeField] private int debrisBurstMin = 6;
        [SerializeField] private int debrisBurstMax = 24;
        [SerializeField] private float smokeRateAtMaxDamage = 22f;
        [SerializeField] private bool enableCrackMeshes = false;
        [SerializeField] private int crackPieceCount = 8;
        [SerializeField] [Range(0.02f, 0.25f)] private float crackThickness = 0.06f;
        [SerializeField] private Color crackColor = new Color(0.1f, 0.08f, 0.07f, 1f);

        [Header("Destruction Shockwave")]
        [SerializeField] private bool enableDestructionShockwave = true;
        [SerializeField] private float destructionShockwaveRadius = 4.8f;
        [SerializeField] private Vector2 destructionShockwaveDamageRange = new Vector2(18f, 74f);
        [SerializeField] private int shockwaveMaxDestructibleHits = 9;
        [SerializeField] private int shockwaveMaxPropHits = 7;
        [SerializeField] [Range(0f, 1f)] private float shockwavePropBreakChance = 0.56f;
        [SerializeField] private float shockwaveImpulse = 8f;
        [SerializeField] private int shockwaveBonusScorePerHit = 6;

        [Header("Boss Collapse")]
        [SerializeField] private float bossCollapseDuration = 0.85f;
        [SerializeField] private int bossCollapseBursts = 3;
        [SerializeField] [Range(0.1f, 1f)] private float bossCollapseShockwaveDamageScale = 0.42f;
        [SerializeField] [Range(0.2f, 1.2f)] private float bossCollapseFinalShrinkScale = 0.22f;

        private Renderer cachedRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Coroutine destroyRoutine;
        private Vector3 initialScale;
        private Vector3 baseInitialScale;
        private Vector3 currentDamageScale;

        private float maxDurability;
        private float currentDurability;

        private ScoreSystem scoreSystem;
        private FeedbackSystem feedbackSystem;
        private DamageNumberSystem damageNumberSystem;

        private Transform weakPointVisual;
        private Renderer weakPointRenderer;
        private MaterialPropertyBlock weakPointPropertyBlock;
        private float weakPointWorldRadius;
        private bool weakPointActive;
        private Transform bossCoreRingVisual;
        private Renderer bossCoreRingRenderer;
        private MaterialPropertyBlock bossCoreRingPropertyBlock;
        private Transform bossCoreGroundTelegraphVisual;
        private Renderer bossCoreGroundTelegraphRenderer;
        private MaterialPropertyBlock bossCoreGroundTelegraphPropertyBlock;
        private bool bossCoreExposureActive;
        private float bossCoreExposureIntensity = 1f;
        private int scaffolderBaseHp;
        private Color scaffolderBaseColor;
        private bool scaffolderBaseCaptured;
        private int sourceScaffolderHp;
        private Color sourceScaffolderColor;
        private bool sourceScaffolderCaptured;
        private bool stageLayoutVisible = true;
        private StageEncounterRole stageEncounterRole;
        private SmallPropStyle smallPropStyle;
        private float stageEncounterDamageScale = 1f;

        private bool isLargeBuilding;
        private Transform fxRoot;
        private ParticleSystem debrisParticle;
        private ParticleSystem smokeParticle;
        private readonly List<Transform> crackPieces = new List<Transform>(16);
        private bool cracksBuilt;

        private static Material sharedFxParticleMaterial;
        private static readonly Collider[] shockwaveHitBuffer = new Collider[96];
        private static float runtimeShockwaveRadiusMultiplier = 1f;
        private static float runtimeShockwaveDamageMultiplier = 1f;
        private static float runtimeShockwavePropChanceBonus;
        private static float runtimeShockwaveImpulseMultiplier = 1f;
        private static int runtimeShockwaveDestructibleCapBonus;
        private static int runtimeShockwavePropCapBonus;
        private static int runtimeShockwaveBonusScoreFlatAdd;

        public static void ConfigureRuntimeShockwaveTuning(
            float radiusMultiplier,
            float damageMultiplier,
            float propChanceBonus,
            int destructibleCapBonus,
            int propCapBonus,
            float impulseMultiplier,
            int bonusScoreFlatAdd)
        {
            runtimeShockwaveRadiusMultiplier = Mathf.Clamp(radiusMultiplier, 0.55f, 4f);
            runtimeShockwaveDamageMultiplier = Mathf.Clamp(damageMultiplier, 0.45f, 5f);
            runtimeShockwavePropChanceBonus = Mathf.Clamp(propChanceBonus, -0.45f, 0.45f);
            runtimeShockwaveDestructibleCapBonus = Mathf.Clamp(destructibleCapBonus, -12, 36);
            runtimeShockwavePropCapBonus = Mathf.Clamp(propCapBonus, -10, 28);
            runtimeShockwaveImpulseMultiplier = Mathf.Clamp(impulseMultiplier, 0.2f, 4f);
            runtimeShockwaveBonusScoreFlatAdd = Mathf.Clamp(bonusScoreFlatAdd, -400, 1200);
        }

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();

            initialScale = SanitizeScale(transform.localScale);
            baseInitialScale = initialScale;
            currentDamageScale = initialScale;
            transform.localScale = initialScale;

            scoreSystem = Object.FindFirstObjectByType<ScoreSystem>();
            feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
            damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
            smallPropStyle = ResolveSmallPropStyle();

            EvaluateLargeBuildingType();
            EnsureLargeBuildingFxIfNeeded();
            EnsureWeakPointSetup();
            ResetBlock();
        }

        private void OnDisable()
        {
            transform.DOKill();
            StopAndClearParticles();
            SetWeakPointVisible(false);
            SetBossCoreTelegraphVisible(false);
        }

        private void Update()
        {
            UpdateWeakPointVisualPulse();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled || currentDurability <= 0f)
            {
                return;
            }

            var player = collision.gameObject.GetComponent<PlayerBallDummyController>()
                         ?? collision.gameObject.GetComponentInParent<PlayerBallDummyController>();
            if (player == null)
            {
                return;
            }

            var body = player.GetComponent<Rigidbody>();
            var relativeSpeed = collision.relativeVelocity.magnitude;
            var impactMultiplier = player.ImpactMultiplier;
            var drillMode = player.DrillMode;
            var impactThreshold = drillMode ? minimumImpact * 0.4f : minimumImpact;
            if (relativeSpeed < impactThreshold)
            {
                return;
            }

            var damage = ComputeDamage(collision, body, relativeSpeed, player.transform, impactMultiplier, drillMode);
            if (damage <= 0.001f)
            {
                return;
            }

            var hitPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            var weakPointHit = IsWeakPointHit(hitPoint);
            if (weakPointHit && enableWeakPointCritical)
            {
                damage *= Mathf.Max(1.05f, weakPointDamageMultiplier);
            }

            var impact01 = Mathf.InverseLerp(minimumImpact, heavyImpact, relativeSpeed);
            if (weakPointHit)
            {
                impact01 = Mathf.Clamp01(impact01 + 0.24f);
            }

            ApplyDamageInternal(damage, hitPoint, impact01, forceHeavy: false, suppressFeedback: false, suppressDamageNumber: false, weakPointHit: weakPointHit, allowDestructionShockwave: true);
        }

        public void ApplyExternalImpactDamage(float damage, Vector3 hitPoint, float impact01 = 0.8f, bool suppressFeedback = true, bool allowDestructionShockwave = false)
        {
            ApplyDamageInternal(damage, hitPoint, Mathf.Clamp01(impact01), forceHeavy: true, suppressFeedback: suppressFeedback, suppressDamageNumber: suppressFeedback, weakPointHit: false, allowDestructionShockwave: allowDestructionShockwave);
        }

        public bool IsStageBoss => stageEncounterRole == StageEncounterRole.BossSentinel;
        public bool IsAlive => currentDurability > 0f && gameObject.activeInHierarchy;
        public float DurabilityRatio => maxDurability > 0f ? Mathf.Clamp01(currentDurability / maxDurability) : 0f;
        public float CurrentDurability => currentDurability;
        public float MaxDurability => maxDurability;
        public float StageEncounterDamageScale => stageEncounterDamageScale;

        public void ConfigureForScaffolder(int hp, Color baseColor)
        {
            sourceScaffolderHp = Mathf.Max(1, hp);
            sourceScaffolderColor = baseColor;
            sourceScaffolderCaptured = true;
            scaffolderBaseHp = sourceScaffolderHp;
            scaffolderBaseColor = sourceScaffolderColor;
            scaffolderBaseCaptured = true;
            ApplyStageEncounterRole(StageEncounterRole.Standard);
        }

        public void ApplyStageEncounterRole(StageEncounterRole role, int bonusHp = 0, Color? colorOverride = null)
        {
            EnsureStageBaseCaptured();
            if (!scaffolderBaseCaptured)
            {
                scaffolderBaseHp = Mathf.Max(1, maxHp);
                scaffolderBaseColor = healthyColor;
                scaffolderBaseCaptured = true;
            }

            var targetHp = role == StageEncounterRole.Standard
                ? scaffolderBaseHp
                : Mathf.Max(1, scaffolderBaseHp + Mathf.Max(0, bonusHp));
            var targetColor = colorOverride ?? scaffolderBaseColor;

            ApplyScaffolderConfiguration(targetHp, targetColor, role);
            if (!stageLayoutVisible)
            {
                gameObject.SetActive(false);
                SetWeakPointVisible(false);
                StopAndClearParticles();
            }
        }

        public void SetStageEncounterDamageScale(float damageScale)
        {
            stageEncounterDamageScale = Mathf.Clamp(damageScale, 0.05f, 4f);
        }

        public void SetBossCoreExposure(bool exposed, float intensity = 1f)
        {
            bossCoreExposureActive = exposed && stageEncounterRole == StageEncounterRole.BossSentinel;
            bossCoreExposureIntensity = Mathf.Clamp(intensity, 0.5f, 2.5f);

            if (!bossCoreExposureActive)
            {
                SetBossCoreTelegraphVisible(false);
                ConfigureWeakPointForCurrentState();
                return;
            }

            EnsureWeakPointSetup();
            MoveWeakPoint(exposedCenter: true);
            EnsureBossCoreTelegraphSetup();
            SetBossCoreTelegraphVisible(true);
        }

        public bool RestoreDurability(float amount)
        {
            if (!isActiveAndEnabled || currentDurability <= 0f || amount <= 0.001f || maxDurability <= 0f)
            {
                return false;
            }

            var previousDurability = currentDurability;
            currentDurability = Mathf.Min(maxDurability, currentDurability + amount);
            if (currentDurability <= previousDurability + 0.001f)
            {
                return false;
            }

            var remainingRatio = Mathf.Clamp01(currentDurability / maxDurability);
            currentDamageScale = ComputeDamageScale(remainingRatio);
            transform.localScale = currentDamageScale;
            UpdateSmokeFromDamage(1f - remainingRatio, forceStop: remainingRatio >= 0.995f);
            UpdateCrackPieces(1f - remainingRatio);
            ConfigureWeakPointForCurrentState();
            UpdateTint();
            return true;
        }

        public void ApplyStageLayoutTuning(int stageNumber)
        {
            EnsureStageBaseCaptured();

            var safeStage = Mathf.Max(1, stageNumber);
            var stageDepth = Mathf.Clamp01((safeStage - 1f) / 5f);
            var baseHeight = Mathf.Max(0.05f, Mathf.Abs(baseInitialScale.y));
            var footprint = Mathf.Max(0.05f, Mathf.Abs(baseInitialScale.x * baseInitialScale.z));
            var threatScore = baseHeight * 1.7f + Mathf.Sqrt(footprint) * 0.95f;
            var laneDepth = transform.position.z;

            var unlockStage = 1;
            if (baseHeight >= 6.2f || threatScore >= 10.2f || laneDepth > 17.5f)
            {
                unlockStage = 5;
            }
            else if (baseHeight >= 5f || threatScore >= 7.6f || laneDepth > 13.5f)
            {
                unlockStage = 4;
            }
            else if (baseHeight >= 3.4f || threatScore >= 5.1f || laneDepth > 9.5f)
            {
                unlockStage = 3;
            }
            else if (baseHeight >= 2.4f || threatScore >= 3.4f || laneDepth > 6.5f)
            {
                unlockStage = 2;
            }

            var visible = safeStage >= unlockStage;
            stageLayoutVisible = visible;
            var stageAdvance = Mathf.Max(0, safeStage - unlockStage);
            var scaleMultiplier = visible ? Mathf.Clamp(1f + stageDepth * 0.14f + stageAdvance * 0.05f, 0.96f, 1.26f) : 0.9f;
            var hpBonus = visible ? Mathf.Clamp(Mathf.FloorToInt(stageAdvance * 0.75f + stageDepth * 2.2f), 0, 8) : 0;
            var tintWeight = visible ? Mathf.Clamp01(0.08f + stageDepth * 0.24f + stageAdvance * 0.05f) : 0f;
            var tunedColor = Color.Lerp(sourceScaffolderColor, new Color(0.96f, 0.76f, 0.48f, 1f), tintWeight);

            scaffolderBaseHp = Mathf.Max(1, sourceScaffolderHp + hpBonus);
            scaffolderBaseColor = tunedColor;
            initialScale = SanitizeScale(baseInitialScale * scaleMultiplier);
            if (visible)
            {
                float stagePresenceBoost = Mathf.InverseLerp(1f, 4f, safeStage);
                float minWidth = Mathf.Lerp(0.95f, 0.72f, stagePresenceBoost);
                float minDepth = Mathf.Lerp(0.95f, 0.72f, stagePresenceBoost);
                float minHeight = Mathf.Lerp(0.9f, 0.7f, stagePresenceBoost);
                initialScale = new Vector3(
                    Mathf.Sign(initialScale.x) * Mathf.Max(minWidth, Mathf.Abs(initialScale.x)),
                    Mathf.Sign(initialScale.y) * Mathf.Max(minHeight, Mathf.Abs(initialScale.y)),
                    Mathf.Sign(initialScale.z) * Mathf.Max(minDepth, Mathf.Abs(initialScale.z)));
                initialScale = SanitizeScale(initialScale);
            }

            ApplyStageEncounterRole(StageEncounterRole.Standard);
            if (!visible)
            {
                gameObject.SetActive(false);
                SetWeakPointVisible(false);
                StopAndClearParticles();
            }
        }

        private void ApplyScaffolderConfiguration(int hp, Color baseColor, StageEncounterRole role)
        {
            stageEncounterRole = role;
            stageEncounterDamageScale = 1f;
            bossCoreExposureActive = false;
            bossCoreExposureIntensity = 1f;
            maxHp = Mathf.Max(1, hp);
            healthyColor = baseColor;
            scoreOnDestroyed = Mathf.Max(45, maxHp * 75);

            ApplyWeakPointRoleTuning(role);
            EvaluateLargeBuildingType();
            EnsureLargeBuildingFxIfNeeded();
            EnsureWeakPointSetup();
            ResetBlock();
        }

        private void EnsureStageBaseCaptured()
        {
            if (!sourceScaffolderCaptured)
            {
                sourceScaffolderHp = Mathf.Max(1, scaffolderBaseCaptured ? scaffolderBaseHp : maxHp);
                sourceScaffolderColor = scaffolderBaseCaptured ? scaffolderBaseColor : healthyColor;
                sourceScaffolderCaptured = true;
            }

            if (!scaffolderBaseCaptured)
            {
                scaffolderBaseHp = sourceScaffolderHp;
                scaffolderBaseColor = sourceScaffolderColor;
                scaffolderBaseCaptured = true;
            }

            if (baseInitialScale == Vector3.zero)
            {
                baseInitialScale = SanitizeScale(initialScale);
            }
        }

        private void ApplyWeakPointRoleTuning(StageEncounterRole role)
        {
            weakPointLargeBuildingsOnly = false;
            moveWeakPointAfterCriticalHit = true;

            switch (role)
            {
                case StageEncounterRole.EliteWeakPoint:
                    enableWeakPointCritical = true;
                    weakPointDamageMultiplier = 2.05f;
                    weakPointHitBonusScore = 48;
                    weakPointRadiusRatio = 0.145f;
                    weakPointHitTolerance = 1.08f;
                    weakPointPulseSpeed = 7.1f;
                    weakPointColorA = new Color(1f, 0.86f, 0.34f, 1f);
                    weakPointColorB = new Color(1f, 0.42f, 0.16f, 1f);
                    break;
                case StageEncounterRole.BossSentinel:
                    enableWeakPointCritical = true;
                    moveWeakPointAfterCriticalHit = false;
                    weakPointDamageMultiplier = 2.45f;
                    weakPointHitBonusScore = 120;
                    weakPointRadiusRatio = 0.185f;
                    weakPointHitTolerance = 1.22f;
                    weakPointPulseSpeed = 4.35f;
                    weakPointColorA = new Color(0.85f, 0.94f, 1f, 1f);
                    weakPointColorB = new Color(0.28f, 0.72f, 1f, 1f);
                    break;
                default:
                    enableWeakPointCritical = false;
                    weakPointDamageMultiplier = 1.95f;
                    weakPointHitBonusScore = 0;
                    weakPointRadiusRatio = 0.14f;
                    weakPointHitTolerance = 1.1f;
                    weakPointPulseSpeed = 6.2f;
                    weakPointColorA = new Color(1f, 0.82f, 0.24f, 1f);
                    weakPointColorB = new Color(1f, 0.36f, 0.12f, 1f);
                    break;
            }
        }
        public void ResetBlock()
        {
            smallPropStyle = ResolveSmallPropStyle();
            maxDurability = Mathf.Max(20f, maxHp * durabilityPerHp);
            currentDurability = maxDurability;

            transform.DOKill();
            currentDamageScale = initialScale;
            transform.localScale = initialScale;
            transform.localRotation = Quaternion.identity;
            gameObject.SetActive(true);

            UpdateSmokeFromDamage(0f, forceStop: true);
            UpdateCrackPieces(0f);
            ConfigureWeakPointForCurrentState();
            UpdateTint();
        }

        private void ApplyDamageInternal(float damage, Vector3 hitPoint, float impact01, bool forceHeavy, bool suppressFeedback = false, bool suppressDamageNumber = false, bool weakPointHit = false, bool allowDestructionShockwave = true)
        {
            if (!isActiveAndEnabled || currentDurability <= 0f)
            {
                return;
            }

            var maxCap = weakPointHit ? maxDamagePerHit * Mathf.Max(1f, weakPointDamageMultiplier) : maxDamagePerHit;
            var safeDamage = Mathf.Clamp(damage, minDamagePerHit, maxCap);
            if (safeDamage <= 0f)
            {
                return;
            }

            safeDamage *= Mathf.Clamp(stageEncounterDamageScale, 0.05f, 4f);
            currentDurability = Mathf.Max(0f, currentDurability - safeDamage);
            var remainingRatio = maxDurability > 0f ? Mathf.Clamp01(currentDurability / maxDurability) : 0f;
            var damageRatio = 1f - remainingRatio;
            var heavyHit = weakPointHit || forceHeavy || safeDamage >= maxDamagePerHit * 0.4f || impact01 > 0.65f;

            scoreSystem ??= Object.FindFirstObjectByType<ScoreSystem>();
            feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
            damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();

            scoreSystem?.AddScore(Mathf.RoundToInt(safeDamage * hitScoreMultiplier * GetHitScoreRewardScale()));
            if (weakPointHit)
            {
                scoreSystem?.AddScore(Mathf.Max(0, weakPointHitBonusScore));
                if (bossCoreExposureActive && stageEncounterRole == StageEncounterRole.BossSentinel)
                {
                    scoreSystem?.AddScore(Mathf.Max(0, bossCoreCriticalBonusScore));
                }
            }
            if (!suppressDamageNumber)
            {
                damageNumberSystem?.ShowDamage(hitPoint, safeDamage, heavyHit, destroyed: currentDurability <= 0f);
            }

            currentDamageScale = ComputeDamageScale(remainingRatio);
            transform.localScale = currentDamageScale;

            if (isLargeBuilding)
            {
                PlayLargeBuildingImpactFx(hitPoint, damageRatio, heavyHit, suppressFeedback);
                UpdateCrackPieces(damageRatio);
            }

            if (!suppressFeedback)
            {
                PlayHitFeedback(impact01, destroyed: currentDurability <= 0f, heavyHit: heavyHit, baseScale: currentDamageScale);
            }

            UpdateTint();

            if (currentDurability <= 0f)
            {
                SetWeakPointVisible(false);
                SetBossCoreTelegraphVisible(false);
                scoreSystem?.RegisterDestruction(scoreOnDestroyed);
                EmitSmallPropDestroyed(hitPoint, impact01);
                if (!suppressFeedback)
                {
                    feedbackSystem?.PlayDestroyFeedback(hitPoint, Mathf.Clamp01(impact01 + 0.25f));
                }

                if (isLargeBuilding)
                {
                    EmitDebris(hitPoint + Vector3.up * 0.35f, Mathf.Clamp01(damageRatio + 0.35f), heavy: true);
                    UpdateSmokeFromDamage(0f, forceStop: true);
                    UpdateCrackPieces(1f);
                }

                if (allowDestructionShockwave)
                {
                    TriggerDestructionShockwave(hitPoint, impact01, heavyHit, suppressFeedback);
                }

                if (destroyRoutine != null)
                {
                    StopCoroutine(destroyRoutine);
                }

                destroyRoutine = StartCoroutine(IsStageBoss ? DisableBossAfterCollapse(hitPoint, Mathf.Clamp01(impact01 + damageRatio * 0.2f), allowDestructionShockwave) : DisableAfterDelay(destroyDisableDelay));
                return;
            }

            if (!suppressFeedback)
            {
                if (weakPointHit)
                {
                    bool bossCoreCritical = bossCoreExposureActive && stageEncounterRole == StageEncounterRole.BossSentinel;
                    if ((Object)(object)damageNumberSystem != (Object)null)
                    {
                        damageNumberSystem.ShowTag(hitPoint + Vector3.up * 0.75f, bossCoreCritical ? $"CORE CRITICAL +{Mathf.Max(0, bossCoreCriticalBonusScore):0}" : "CRITICAL", bossCoreCritical || impact01 >= 0.6f);
                    }
                    feedbackSystem?.PlayWeakPointCriticalFeedback(hitPoint, Mathf.Clamp01(impact01 + (bossCoreCritical ? 0.28f : 0.14f)), bossCoreCritical);
                    feedbackSystem?.PlayDestroyFeedback(hitPoint, Mathf.Clamp01(impact01 + 0.2f));
                }
                else
                {
                    feedbackSystem?.PlayHitFeedback(hitPoint, impact01);
                }
            }

            if (weakPointHit && currentDurability > 0f && moveWeakPointAfterCriticalHit)
            {
                MoveWeakPoint();
            }
        }

        private void TriggerDestructionShockwave(Vector3 center, float impact01, bool heavyHit, bool suppressFeedback, float damageScale = 1f, float radiusScale = 1f, int destructibleCapBonus = 0, int propCapBonus = 0)
        {
            if (!enableDestructionShockwave || !isActiveAndEnabled)
            {
                return;
            }

            var radius = Mathf.Max(1.2f, destructionShockwaveRadius) * (isLargeBuilding ? 1.18f : 0.95f);
            radius *= runtimeShockwaveRadiusMultiplier * Mathf.Max(0.2f, radiusScale);
            var radiusSqr = radius * radius;
            var minDamage = Mathf.Max(4f, destructionShockwaveDamageRange.x) * runtimeShockwaveDamageMultiplier * Mathf.Max(0.1f, damageScale);
            var maxDamage = Mathf.Max(minDamage + 4f, destructionShockwaveDamageRange.y * runtimeShockwaveDamageMultiplier * Mathf.Max(0.1f, damageScale));
            if (heavyHit)
            {
                minDamage *= 1.12f;
                maxDamage *= 1.18f;
            }

            var hitCount = Physics.OverlapSphereNonAlloc(center, radius, shockwaveHitBuffer, ~0, QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return;
            }

            var destructibleHits = 0;
            var propHits = 0;
            var totalShockwaveHits = 0;
            var touchedBlocks = new HashSet<DummyDestructibleBlock>();
            var touchedProps = new HashSet<DummyStreetPropReactive>();

            for (var i = 0; i < hitCount; i++)
            {
                var col = shockwaveHitBuffer[i];
                if (col == null || !col.enabled)
                {
                    continue;
                }

                var colTransform = col.transform;
                if (colTransform == null)
                {
                    continue;
                }

                var block = col.GetComponent<DummyDestructibleBlock>() ?? col.GetComponentInParent<DummyDestructibleBlock>();
                if (block != null && block != this && block.gameObject.activeInHierarchy && touchedBlocks.Add(block))
                {
                    var destructibleCap = Mathf.Max(1, shockwaveMaxDestructibleHits + runtimeShockwaveDestructibleCapBonus + destructibleCapBonus);
                    if (destructibleHits < destructibleCap)
                    {
                        var delta = block.transform.position - center;
                        delta.y = 0f;
                        var distSqr = delta.sqrMagnitude;
                        if (distSqr <= radiusSqr)
                        {
                            var dist = Mathf.Sqrt(distSqr);
                            var t = 1f - Mathf.Clamp01(dist / Mathf.Max(0.01f, radius));
                            var damage = Mathf.Lerp(minDamage, maxDamage, t);
                            var hitPoint = block.transform.position + Vector3.up * Mathf.Max(0.32f, block.transform.lossyScale.y * 0.2f);
                            block.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.52f, 1f, t), suppressFeedback: true, allowDestructionShockwave: false);
                            destructibleHits++;
                            totalShockwaveHits++;
                        }
                    }

                    continue;
                }

                var prop = col.GetComponent<DummyStreetPropReactive>() ?? col.GetComponentInParent<DummyStreetPropReactive>();
                if (prop != null && prop.gameObject.activeInHierarchy && touchedProps.Add(prop))
                {
                    var propCap = Mathf.Max(1, shockwaveMaxPropHits + runtimeShockwavePropCapBonus + propCapBonus);
                    if (propHits >= propCap)
                    {
                        continue;
                    }

                    var delta = prop.transform.position - center;
                    delta.y = 0f;
                    var distSqr = delta.sqrMagnitude;
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    var dist = Mathf.Sqrt(distSqr);
                    var t = 1f - Mathf.Clamp01(dist / Mathf.Max(0.01f, radius));
                    var safeChanceBase = Mathf.Clamp01(shockwavePropBreakChance + runtimeShockwavePropChanceBonus);
                    var chance = Mathf.Lerp(safeChanceBase * 0.35f, safeChanceBase, t);
                    if (Random.value > Mathf.Clamp01(chance))
                    {
                        continue;
                    }

                    var hitPoint = prop.transform.position + Vector3.up * 0.24f;
                    prop.ApplyExternalBreak(hitPoint, Mathf.Lerp(0.55f, 1f, t), drillMode: false, suppressFeedback: true);
                    propHits++;
                    totalShockwaveHits++;
                    continue;
                }

                var rb = col.attachedRigidbody;
                if (rb != null && !rb.isKinematic && rb.mass > 0.01f)
                {
                    rb.AddExplosionForce(Mathf.Max(0f, shockwaveImpulse * runtimeShockwaveImpulseMultiplier), center, radius, 0.55f, ForceMode.Impulse);
                }
            }

            if (totalShockwaveHits <= 0)
            {
                return;
            }

            scoreSystem ??= Object.FindFirstObjectByType<ScoreSystem>();
            feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();

            var bonus = Mathf.Max(0, totalShockwaveHits * Mathf.Max(0, shockwaveBonusScorePerHit) + runtimeShockwaveBonusScoreFlatAdd);
            if (bonus > 0)
            {
                scoreSystem?.AddScore(bonus);
            }

            if (!suppressFeedback)
            {
                var intensity = Mathf.Clamp01(0.54f + impact01 * 0.2f + totalShockwaveHits * 0.04f + (heavyHit ? 0.1f : 0f));
                feedbackSystem?.PlayComboRushFeedback(center + Vector3.up * 0.18f, intensity, radius * 0.9f);
            }
        }

        private float ComputeDamage(Collision collision, Rigidbody playerBody, float relativeSpeed, Transform playerTransform, float impactMultiplier, bool drillMode)
        {
            var mass = playerBody != null ? Mathf.Max(1f, playerBody.mass) : 10f;
            var linearVelocity = playerBody != null
                ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z).magnitude
                : relativeSpeed;
            var angularVelocity = playerBody != null ? playerBody.angularVelocity.magnitude : 0f;
            var impulse = collision.impulse.magnitude;

            var radius = 0.5f;
            if (playerTransform != null)
            {
                radius = Mathf.Max(0.25f, playerTransform.lossyScale.x * 0.5f);
            }

            var linearEnergy = 0.5f * mass * relativeSpeed * relativeSpeed;
            var inertia = 0.4f * mass * radius * radius;
            var angularEnergy = 0.5f * inertia * angularVelocity * angularVelocity;

            var speedContribution = linearVelocity * 1.4f;
            var spinContribution = angularVelocity * 0.9f;
            var massContribution = mass * 0.35f;

            var computed = minDamagePerHit
                         + linearEnergy * linearEnergyToDamage
                         + angularEnergy * angularEnergyToDamage
                         + impulse * impulseToDamage
                         + speedContribution
                         + spinContribution
                         + massContribution;

            if (relativeSpeed >= heavyImpact)
            {
                var bonus = Mathf.InverseLerp(heavyImpact, heavyImpact * 1.8f, relativeSpeed);
                computed *= 1f + bonus * 0.4f;
            }

            computed *= Mathf.Max(0.2f, impactMultiplier);
            if (drillMode)
            {
                computed *= 1.2f;
            }

            return Mathf.Clamp(computed, minDamagePerHit, maxDamagePerHit);
        }

        private IEnumerator DisableAfterDelay(float delay)
        {
            transform.DOKill();
            var targetScale = currentDamageScale * Mathf.Clamp(destroyShrinkScale, 0.05f, 0.95f);
            var targetRotation = new Vector3(0f, Random.Range(-28f, 28f), 0f);

            switch (smallPropStyle)
            {
                case SmallPropStyle.Mailbox:
                    targetScale = Vector3.Scale(currentDamageScale, new Vector3(0.32f, 0.12f, 0.28f));
                    targetRotation = new Vector3(Random.Range(-8f, 12f), Random.Range(-12f, 12f), Random.Range(-72f, 72f));
                    transform.DOJump(transform.position + transform.right * Random.Range(-0.18f, 0.18f), 0.08f, 1, delay * 0.82f).SetEase(Ease.OutQuad);
                    break;
                case SmallPropStyle.Fence:
                    targetScale = Vector3.Scale(currentDamageScale, new Vector3(0.94f, 0.18f, 0.22f));
                    targetRotation = new Vector3(0f, Random.Range(-8f, 8f), Random.Range(-92f, 92f));
                    break;
                case SmallPropStyle.Shed:
                    targetScale = Vector3.Scale(currentDamageScale, new Vector3(0.78f, 0.16f, 0.78f));
                    targetRotation = new Vector3(Random.Range(-10f, 10f), Random.Range(-22f, 22f), Random.Range(-12f, 12f));
                    PulseNamedChild("Roof", new Vector3(1.06f, 1f, 1.06f), delay * 0.72f);
                    break;
            }

            transform.DOScale(SanitizeScale(targetScale), delay).SetEase(Ease.InBack);
            transform.DORotate(targetRotation, delay, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        private IEnumerator DisableBossAfterCollapse(Vector3 hitPoint, float intensity, bool allowDestructionShockwave)
        {
            transform.DOKill();
            EnsureLargeBuildingFxIfNeeded();
            SetBossCoreExposure(false);
            SetBossCoreTelegraphVisible(false);

            float duration = Mathf.Max(0.45f, bossCollapseDuration);
            int bursts = Mathf.Clamp(bossCollapseBursts, 2, 5);
            float step = duration / Mathf.Max(1, bursts);
            Vector3 baseScale = SanitizeScale(currentDamageScale);
            Vector3 finalScale = SanitizeScale(baseScale * Mathf.Clamp(bossCollapseFinalShrinkScale, 0.08f, 0.95f));

            UpdateSmokeFromDamage(1f, forceStop: false);

            for (int i = 0; i < bursts; i++)
            {
                float burst01 = bursts <= 1 ? 1f : (float)i / (float)(bursts - 1);
                float angle = (360f / bursts) * i + Random.Range(-16f, 16f);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 burstPoint = transform.position + dir * Mathf.Lerp(0.25f, Mathf.Max(0.6f, baseScale.x * 0.18f + baseScale.z * 0.18f), burst01) + Vector3.up * Mathf.Lerp(0.5f, Mathf.Max(1.2f, baseScale.y * 0.42f), burst01);

                EmitDebris(burstPoint, Mathf.Lerp(0.68f, 1f, burst01), heavy: true);
                feedbackSystem?.PlayDestroyFeedback(burstPoint, Mathf.Lerp(Mathf.Clamp01(intensity), 1f, burst01 * 0.65f));

                if (allowDestructionShockwave)
                {
                    float collapseDamageScale = Mathf.Lerp(Mathf.Max(0.18f, bossCollapseShockwaveDamageScale), Mathf.Max(0.36f, bossCollapseShockwaveDamageScale * 1.75f), burst01);
                    float collapseRadiusScale = Mathf.Lerp(1.05f, 1.45f, burst01);
                    int collapseDestructibleBonus = Mathf.RoundToInt(Mathf.Lerp(2f, 7f, burst01));
                    int collapsePropBonus = Mathf.RoundToInt(Mathf.Lerp(1f, 5f, burst01));
                    TriggerDestructionShockwave(
                        burstPoint,
                        Mathf.Clamp01(intensity * Mathf.Lerp(0.72f, 1f, burst01)),
                        heavyHit: true,
                        suppressFeedback: true,
                        damageScale: collapseDamageScale,
                        radiusScale: collapseRadiusScale,
                        destructibleCapBonus: collapseDestructibleBonus,
                        propCapBonus: collapsePropBonus);
                }

                float shrink01 = (float)(i + 1) / bursts;
                Vector3 targetScale = Vector3.Lerp(baseScale, finalScale, Mathf.SmoothStep(0f, 1f, shrink01));
                transform.DOScale(SanitizeScale(targetScale), Mathf.Max(0.08f, step * 0.75f)).SetEase(Ease.InBack);
                transform.DORotate(new Vector3(Random.Range(-8f, 8f), Random.Range(-32f, 32f), Random.Range(-10f, 10f)), Mathf.Max(0.08f, step * 0.7f), RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad);

                yield return new WaitForSeconds(Mathf.Max(0.08f, step));
            }

            UpdateSmokeFromDamage(0f, forceStop: true);
            gameObject.SetActive(false);
        }

        private void PlayHitFeedback(float impact01, bool destroyed, bool heavyHit, Vector3 baseScale)
        {
            transform.DOKill();
            transform.localScale = baseScale;

            var punch = Mathf.Lerp(hitPunchScale * 0.8f, hitPunchScale * 1.5f, Mathf.Clamp01(impact01));
            if (heavyHit)
            {
                punch *= 1.2f;
            }

            if (destroyed)
            {
                punch *= 1.25f;
            }

            var duration = destroyed ? 0.16f : (heavyHit ? 0.14f : 0.11f);
            switch (smallPropStyle)
            {
                case SmallPropStyle.Mailbox:
                    transform.DOPunchPosition(transform.right * Mathf.Lerp(0.03f, 0.09f, Mathf.Clamp01(impact01)), duration, 8, 0.6f);
                    transform.DOPunchRotation(new Vector3(0f, 0f, Mathf.Lerp(6f, 18f, Mathf.Clamp01(impact01))), duration, 8, 0.68f);
                    transform.DOPunchScale(new Vector3(punch * 0.7f, punch * 0.4f, punch * 0.7f), duration, 7, 0.58f);
                    break;
                case SmallPropStyle.Fence:
                    transform.DOPunchRotation(new Vector3(0f, 0f, Mathf.Lerp(8f, 24f, Mathf.Clamp01(impact01))), duration, 8, 0.72f);
                    transform.DOPunchScale(new Vector3(punch * 0.25f, punch * 0.55f, punch * 0.12f), duration, 6, 0.5f);
                    break;
                case SmallPropStyle.Shed:
                    PulseNamedChild("Roof", new Vector3(1f + punch * 0.35f, 1f, 1f + punch * 0.28f), duration);
                    transform.DOPunchScale(new Vector3(punch * 0.45f, punch * 0.25f, punch * 0.45f), duration, 7, 0.6f);
                    break;
                case SmallPropStyle.ShopAwning:
                    PulseNamedChild("AwningTrim", new Vector3(1f + punch * 0.4f, 1f, 1f + punch * 0.2f), duration);
                    transform.DOPunchRotation(new Vector3(Mathf.Lerp(4f, 12f, Mathf.Clamp01(impact01)), 0f, 0f), duration, 7, 0.62f);
                    transform.DOPunchScale(new Vector3(punch * 0.55f, punch * 0.18f, punch * 0.22f), duration, 7, 0.58f);
                    break;
                case SmallPropStyle.ShopSign:
                    PulseNamedChild("SignFace", new Vector3(1f + punch * 0.32f, 1f + punch * 0.18f, 1f), duration);
                    transform.DOPunchRotation(new Vector3(0f, Mathf.Lerp(5f, 14f, Mathf.Clamp01(impact01)), Mathf.Lerp(10f, 28f, Mathf.Clamp01(impact01))), duration, 8, 0.7f);
                    transform.DOPunchScale(new Vector3(punch * 0.34f, punch * 0.24f, punch * 0.12f), duration, 7, 0.56f);
                    break;
                case SmallPropStyle.Kiosk:
                    PulseNamedChild("ShopRoof", new Vector3(1f + punch * 0.28f, 1f, 1f + punch * 0.24f), duration);
                    PulseNamedChild("ShopCounter", new Vector3(1f + punch * 0.3f, 1f + punch * 0.12f, 1f), duration);
                    transform.DOPunchScale(new Vector3(punch * 0.48f, punch * 0.22f, punch * 0.38f), duration, 7, 0.6f);
                    break;
                case SmallPropStyle.Bench:
                    PulseNamedChild("BenchSeat", new Vector3(1f + punch * 0.34f, 1f, 1f + punch * 0.1f), duration);
                    transform.DOPunchRotation(new Vector3(0f, 0f, Mathf.Lerp(6f, 18f, Mathf.Clamp01(impact01))), duration, 7, 0.64f);
                    transform.DOPunchScale(new Vector3(punch * 0.42f, punch * 0.16f, punch * 0.18f), duration, 7, 0.56f);
                    break;
                case SmallPropStyle.BusStop:
                    PulseNamedChild("StopRoof", new Vector3(1f + punch * 0.22f, 1f, 1f + punch * 0.16f), duration);
                    PulseNamedChild("StopPanel", new Vector3(1f + punch * 0.18f, 1f + punch * 0.24f, 1f), duration);
                    transform.DOPunchRotation(new Vector3(0f, Mathf.Lerp(4f, 12f, Mathf.Clamp01(impact01)), Mathf.Lerp(5f, 16f, Mathf.Clamp01(impact01))), duration, 7, 0.66f);
                    break;
                case SmallPropStyle.Vending:
                    PulseNamedChild("VendFace", new Vector3(1f + punch * 0.2f, 1f + punch * 0.16f, 1f), duration);
                    transform.DOPunchScale(new Vector3(punch * 0.28f, punch * 0.24f, punch * 0.18f), duration, 7, 0.58f);
                    break;
                default:
                    transform.DOPunchScale(Vector3.one * punch, duration, 8, 0.62f);
                    break;
            }
        }

        private SmallPropStyle ResolveSmallPropStyle()
        {
            if (transform.Find("Flag") != null)
            {
                return SmallPropStyle.Mailbox;
            }

            if (transform.Find("RailTop") != null || transform.Find("RailBottom") != null)
            {
                return SmallPropStyle.Fence;
            }

            if (transform.Find("AwningTrim") != null)
            {
                return SmallPropStyle.ShopAwning;
            }

            if (transform.Find("SignFace") != null)
            {
                return SmallPropStyle.ShopSign;
            }

            if (transform.Find("ShopCounter") != null || transform.Find("ShopStripe") != null)
            {
                return SmallPropStyle.Kiosk;
            }

            if (transform.Find("BenchSeat") != null)
            {
                return SmallPropStyle.Bench;
            }

            if (transform.Find("StopPanel") != null || transform.Find("StopRoof") != null)
            {
                return SmallPropStyle.BusStop;
            }

            if (transform.Find("VendFace") != null)
            {
                return SmallPropStyle.Vending;
            }

            if (transform.Find("Roof") != null && transform.Find("Door") != null)
            {
                return SmallPropStyle.Shed;
            }

            return SmallPropStyle.Default;
        }

        private float GetHitScoreRewardScale()
        {
            if (smallPropStyle != SmallPropStyle.Default)
            {
                return 1.22f;
            }

            var baseHeight = Mathf.Max(0.05f, Mathf.Abs(baseInitialScale.y));
            var footprint = Mathf.Max(0.05f, Mathf.Abs(baseInitialScale.x * baseInitialScale.z));
            if (!isLargeBuilding && baseHeight <= 1.8f && footprint <= 2.8f)
            {
                return 1.1f;
            }

            if (isLargeBuilding || baseHeight >= 4.6f)
            {
                return 0.94f;
            }

            return 1f;
        }

        private void EmitSmallPropDestroyed(Vector3 hitPoint, float impact01)
        {
            var kind = MapSmallPropBreakKind(smallPropStyle);
            if (kind == SmallPropBreakKind.None)
            {
                return;
            }

            SmallPropDestroyed?.Invoke(new SmallPropBreakInfo
            {
                Kind = kind,
                Position = hitPoint,
                Impact01 = Mathf.Clamp01(impact01)
            });
        }

        private static SmallPropBreakKind MapSmallPropBreakKind(SmallPropStyle style)
        {
            switch (style)
            {
                case SmallPropStyle.Mailbox:
                    return SmallPropBreakKind.Mailbox;
                case SmallPropStyle.Fence:
                    return SmallPropBreakKind.Fence;
                case SmallPropStyle.Shed:
                    return SmallPropBreakKind.Shed;
                case SmallPropStyle.ShopAwning:
                    return SmallPropBreakKind.ShopAwning;
                case SmallPropStyle.ShopSign:
                    return SmallPropBreakKind.ShopSign;
                case SmallPropStyle.Kiosk:
                    return SmallPropBreakKind.Kiosk;
                case SmallPropStyle.Bench:
                    return SmallPropBreakKind.Bench;
                case SmallPropStyle.BusStop:
                    return SmallPropBreakKind.BusStop;
                case SmallPropStyle.Vending:
                    return SmallPropBreakKind.Vending;
                default:
                    return SmallPropBreakKind.None;
            }
        }

        private void PulseNamedChild(string childName, Vector3 scaleMultiplier, float duration)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            child.DOKill();
            var baseChildScale = SanitizeScale(child.localScale);
            child.localScale = baseChildScale;
            child.DOPunchScale(Vector3.Scale(baseChildScale, scaleMultiplier - Vector3.one), Mathf.Max(0.08f, duration), 6, 0.55f);
        }

        private Vector3 ComputeDamageScale(float remainingRatio)
        {
            var ratio = Mathf.Clamp01(remainingRatio);
            var shrink = (1f - ratio) * Mathf.Clamp(maxDamageShrinkRatio, 0f, 0.4f);
            var mul = Mathf.Clamp01(1f - shrink);
            var scaled = initialScale * mul;
            return SanitizeScale(scaled);
        }

        private void EvaluateLargeBuildingType()
        {
            var volume = Mathf.Abs(initialScale.x * initialScale.y * initialScale.z);
            isLargeBuilding = maxHp >= largeBuildingMinHp
                              || volume >= largeBuildingMinVolume
                              || initialScale.y >= largeBuildingMinHeight;
        }

        private void EnsureLargeBuildingFxIfNeeded()
        {
            if (!isLargeBuilding)
            {
                if (fxRoot == null)
                {
                    fxRoot = transform.Find("_LargeHitFx");
                }

                ClearCrackPieces();

                if (fxRoot != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(fxRoot.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(fxRoot.gameObject);
                    }

                    fxRoot = null;
                    debrisParticle = null;
                    smokeParticle = null;
                }

                return;
            }

            if (fxRoot == null)
            {
                var root = transform.Find("_LargeHitFx");
                if (root == null)
                {
                    var rootGo = new GameObject("_LargeHitFx");
                    root = rootGo.transform;
                    root.SetParent(transform, false);
                }

                fxRoot = root;
            }

            if (debrisParticle == null)
            {
                var go = new GameObject("Debris", typeof(ParticleSystem));
                go.transform.SetParent(fxRoot, false);
                debrisParticle = go.GetComponent<ParticleSystem>();
                ConfigureDebrisParticle(debrisParticle);
            }

            if (smokeParticle == null)
            {
                var go = new GameObject("Smoke", typeof(ParticleSystem));
                go.transform.SetParent(fxRoot, false);
                smokeParticle = go.GetComponent<ParticleSystem>();
                ConfigureSmokeParticle(smokeParticle);
            }

            if (enableCrackMeshes)
            {
                EnsureCrackPieces();
            }
            else
            {
                ClearCrackPieces();
            }
        }

        private void ConfigureDebrisParticle(ParticleSystem ps)
        {
            if (ps == null)
            {
                return;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.55f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.46f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 7.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.26f);
            main.gravityModifier = 0.95f;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.2f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.material = GetSharedFxParticleMaterial();
            }
        }

        private void ConfigureSmokeParticle(ParticleSystem ps)
        {
            if (ps == null)
            {
                return;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.duration = 1.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 220;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.95f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
            main.startColor = new Color(0.18f, 0.18f, 0.18f, 0.6f);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(Mathf.Max(0.2f, initialScale.x * 0.55f), 0.2f, Mathf.Max(0.2f, initialScale.z * 0.55f));

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.2f;
            noise.frequency = 0.35f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.material = GetSharedFxParticleMaterial();
            }
        }

        private void PlayLargeBuildingImpactFx(Vector3 hitPoint, float damageRatio, bool heavy, bool suppressFeedback)
        {
            EnsureLargeBuildingFxIfNeeded();
            if (debrisParticle == null || smokeParticle == null)
            {
                return;
            }

            UpdateSmokeFromDamage(damageRatio, forceStop: false);

            if (suppressFeedback)
            {
                return;
            }

            EmitDebris(hitPoint, damageRatio, heavy);
        }

        private void EnsureCrackPieces()
        {
            if (!enableCrackMeshes || cracksBuilt || crackPieceCount <= 0)
            {
                return;
            }

            cracksBuilt = true;
            crackPieces.Clear();

            var rng = new System.Random(GetInstanceID());
            var size = SanitizeScale(initialScale);
            var half = size * 0.5f;
            var baseLength = Mathf.Max(0.2f, Mathf.Min(size.x, size.z) * 0.35f);

            for (var i = 0; i < crackPieceCount; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Crack_{i:00}";
                go.transform.SetParent(fxRoot, false);

                var collider = go.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(collider);
                    }
                    else
                    {
                        DestroyImmediate(collider);
                    }
                }
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null && cachedRenderer != null && cachedRenderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial = cachedRenderer.sharedMaterial;
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", crackColor);
                    block.SetColor("_Color", crackColor);
                    renderer.SetPropertyBlock(block);
                }

                var face = rng.Next(0, 6);
                var length = Mathf.Lerp(baseLength * 0.6f, baseLength * 1.2f, (float)rng.NextDouble());
                var width = Mathf.Lerp(0.06f, 0.16f, (float)rng.NextDouble()) * Mathf.Max(0.4f, Mathf.Min(size.x, size.z));
                var thickness = Mathf.Clamp(crackThickness, 0.02f, 0.25f);

                var localScale = new Vector3(width, thickness, length);
                var localPosition = Vector3.zero;
                var localRotation = Quaternion.identity;

                switch (face)
                {
                    case 0: // +X
                        localPosition = new Vector3(half.x + thickness * 0.35f, RandomRange(rng, -half.y * 0.6f, half.y * 0.6f), RandomRange(rng, -half.z * 0.6f, half.z * 0.6f));
                        localRotation = Quaternion.Euler(0f, 90f, 0f);
                        localScale = new Vector3(thickness, width, length);
                        break;
                    case 1: // -X
                        localPosition = new Vector3(-half.x - thickness * 0.35f, RandomRange(rng, -half.y * 0.6f, half.y * 0.6f), RandomRange(rng, -half.z * 0.6f, half.z * 0.6f));
                        localRotation = Quaternion.Euler(0f, 90f, 0f);
                        localScale = new Vector3(thickness, width, length);
                        break;
                    case 2: // +Z
                        localPosition = new Vector3(RandomRange(rng, -half.x * 0.6f, half.x * 0.6f), RandomRange(rng, -half.y * 0.6f, half.y * 0.6f), half.z + thickness * 0.35f);
                        localRotation = Quaternion.identity;
                        localScale = new Vector3(width, thickness, length);
                        break;
                    case 3: // -Z
                        localPosition = new Vector3(RandomRange(rng, -half.x * 0.6f, half.x * 0.6f), RandomRange(rng, -half.y * 0.6f, half.y * 0.6f), -half.z - thickness * 0.35f);
                        localRotation = Quaternion.identity;
                        localScale = new Vector3(width, thickness, length);
                        break;
                    case 4: // +Y
                        localPosition = new Vector3(RandomRange(rng, -half.x * 0.6f, half.x * 0.6f), half.y + thickness * 0.35f, RandomRange(rng, -half.z * 0.6f, half.z * 0.6f));
                        localRotation = Quaternion.Euler(90f, 0f, 0f);
                        localScale = new Vector3(width, thickness, length);
                        break;
                    default: // -Y
                        localPosition = new Vector3(RandomRange(rng, -half.x * 0.6f, half.x * 0.6f), -half.y - thickness * 0.35f, RandomRange(rng, -half.z * 0.6f, half.z * 0.6f));
                        localRotation = Quaternion.Euler(90f, 0f, 0f);
                        localScale = new Vector3(width, thickness, length);
                        break;
                }

                go.transform.localPosition = localPosition;
                go.transform.localRotation = localRotation;
                go.transform.localScale = SanitizeScale(localScale);
                go.SetActive(false);
                crackPieces.Add(go.transform);
            }
        }

        private void UpdateCrackPieces(float damageRatio)
        {
            if (!enableCrackMeshes || !isLargeBuilding || crackPieces.Count == 0)
            {
                return;
            }

            var count = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(damageRatio) * crackPieces.Count), 0, crackPieces.Count);
            for (var i = 0; i < crackPieces.Count; i++)
            {
                var piece = crackPieces[i];
                if (piece == null)
                {
                    continue;
                }

                var shouldBeActive = i < count;
                if (piece.gameObject.activeSelf != shouldBeActive)
                {
                    piece.gameObject.SetActive(shouldBeActive);
                }
            }
        }

        private static float RandomRange(System.Random rng, float min, float max)
        {
            return Mathf.Lerp(min, max, (float)rng.NextDouble());
        }

        private void EmitDebris(Vector3 hitPoint, float damageRatio, bool heavy)
        {
            if (debrisParticle == null)
            {
                return;
            }

            var main = debrisParticle.main;
            main.startSpeed = heavy
                ? new ParticleSystem.MinMaxCurve(4.5f, 9.2f)
                : new ParticleSystem.MinMaxCurve(2.3f, 6.3f);

            var burstCount = Mathf.RoundToInt(Mathf.Lerp(debrisBurstMin, debrisBurstMax, Mathf.Clamp01(damageRatio)));
            if (heavy)
            {
                burstCount = Mathf.RoundToInt(burstCount * 1.2f);
            }

            debrisParticle.transform.position = hitPoint;
            debrisParticle.transform.rotation = Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f);
            debrisParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            debrisParticle.Emit(Mathf.Max(1, burstCount));
            debrisParticle.Play();
        }

        private void UpdateSmokeFromDamage(float damageRatio, bool forceStop)
        {
            if (smokeParticle == null)
            {
                return;
            }

            if (forceStop)
            {
                smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                smokeParticle.Clear(true);
                return;
            }

            var damage01 = Mathf.Clamp01(damageRatio);
            var emission = smokeParticle.emission;
            emission.rateOverTime = Mathf.Lerp(0f, smokeRateAtMaxDamage, damage01);

            var smokePos = transform.position + Vector3.up * Mathf.Max(0.35f, currentDamageScale.y * 0.52f);
            smokeParticle.transform.position = smokePos;

            if (damage01 > 0.05f)
            {
                if (!smokeParticle.isPlaying)
                {
                    smokeParticle.Play(true);
                }
            }
            else if (smokeParticle.isPlaying)
            {
                smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void StopAndClearParticles()
        {
            if (debrisParticle != null)
            {
                debrisParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                debrisParticle.Clear(true);
            }

            if (smokeParticle != null)
            {
                smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                smokeParticle.Clear(true);
            }
        }

        private void ClearCrackPieces()
        {
            if (crackPieces.Count > 0)
            {
                for (var i = 0; i < crackPieces.Count; i++)
                {
                    var piece = crackPieces[i];
                    if (piece == null)
                    {
                        continue;
                    }

                    if (Application.isPlaying)
                    {
                        Destroy(piece.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(piece.gameObject);
                    }
                }
            }

            if (fxRoot != null)
            {
                for (var i = fxRoot.childCount - 1; i >= 0; i--)
                {
                    var child = fxRoot.GetChild(i);
                    if (child == null || !child.name.StartsWith("Crack_", System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }

            crackPieces.Clear();
            cracksBuilt = false;
        }
        private void EnsureWeakPointSetup()
        {
            if (!enableWeakPointCritical)
            {
                SetWeakPointVisible(false);
                return;
            }

            if (weakPointVisual == null)
            {
                var node = transform.Find("_WeakPoint");
                if (node == null)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.name = "_WeakPoint";
                    node = go.transform;
                    node.SetParent(transform, false);
                }

                weakPointVisual = node;
                var collider = weakPointVisual.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(collider);
                    }
                    else
                    {
                        DestroyImmediate(collider);
                    }
                }

                weakPointRenderer = weakPointVisual.GetComponent<Renderer>();
                if (weakPointRenderer != null && cachedRenderer != null && cachedRenderer.sharedMaterial != null)
                {
                    weakPointRenderer.sharedMaterial = cachedRenderer.sharedMaterial;
                }
            }

            weakPointPropertyBlock ??= new MaterialPropertyBlock();
            EnsureBossCoreTelegraphSetup();
        }

        private void EnsureBossCoreTelegraphSetup()
        {
            if (bossCoreRingVisual == null)
            {
                var ringNode = transform.Find("_BossCoreRing");
                if (ringNode == null)
                {
                    var ringGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ringGo.name = "_BossCoreRing";
                    ringNode = ringGo.transform;
                    ringNode.SetParent(transform, false);
                }

                bossCoreRingVisual = ringNode;
                var ringCollider = bossCoreRingVisual.GetComponent<Collider>();
                if (ringCollider != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(ringCollider);
                    }
                    else
                    {
                        DestroyImmediate(ringCollider);
                    }
                }

                bossCoreRingRenderer = bossCoreRingVisual.GetComponent<Renderer>();
                if (bossCoreRingRenderer != null && cachedRenderer != null && cachedRenderer.sharedMaterial != null)
                {
                    bossCoreRingRenderer.sharedMaterial = cachedRenderer.sharedMaterial;
                }
            }

            if (bossCoreGroundTelegraphVisual == null)
            {
                var groundNode = transform.Find("_BossCoreGround");
                if (groundNode == null)
                {
                    var groundGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    groundGo.name = "_BossCoreGround";
                    groundNode = groundGo.transform;
                    groundNode.SetParent(transform, false);
                }

                bossCoreGroundTelegraphVisual = groundNode;
                var groundCollider = bossCoreGroundTelegraphVisual.GetComponent<Collider>();
                if (groundCollider != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(groundCollider);
                    }
                    else
                    {
                        DestroyImmediate(groundCollider);
                    }
                }

                bossCoreGroundTelegraphRenderer = bossCoreGroundTelegraphVisual.GetComponent<Renderer>();
                if (bossCoreGroundTelegraphRenderer != null && cachedRenderer != null && cachedRenderer.sharedMaterial != null)
                {
                    bossCoreGroundTelegraphRenderer.sharedMaterial = cachedRenderer.sharedMaterial;
                }
            }

            bossCoreRingPropertyBlock ??= new MaterialPropertyBlock();
            bossCoreGroundTelegraphPropertyBlock ??= new MaterialPropertyBlock();
        }

        private void ConfigureWeakPointForCurrentState()
        {
            if (!enableWeakPointCritical)
            {
                SetWeakPointVisible(false);
                return;
            }

            var shouldActivate = !weakPointLargeBuildingsOnly || isLargeBuilding;
            if (!shouldActivate)
            {
                SetWeakPointVisible(false);
                return;
            }

            EnsureWeakPointSetup();
            MoveWeakPoint(exposedCenter: bossCoreExposureActive);
        }

        private void MoveWeakPoint(bool exposedCenter = false)
        {
            if (weakPointVisual == null)
            {
                return;
            }

            var safeScale = SanitizeScale(initialScale);
            var half = safeScale * 0.5f;
            var minExtent = Mathf.Max(0.06f, Mathf.Min(safeScale.x, Mathf.Min(safeScale.y, safeScale.z)));
            var radiusScale = exposedCenter ? Mathf.Lerp(1.35f, 1.9f, Mathf.Clamp01((bossCoreExposureIntensity - 0.5f) / 2f)) : 1f;
            weakPointWorldRadius = Mathf.Clamp(minExtent * Mathf.Clamp(weakPointRadiusRatio, 0.06f, 0.45f) * radiusScale, 0.08f, exposedCenter ? 1.1f : 0.65f);

            var insetY = Mathf.Max(weakPointWorldRadius * 0.8f, 0.08f);
            var insetXZ = Mathf.Max(weakPointWorldRadius * 0.8f, 0.08f);
            Vector3 localPos;
            if (exposedCenter)
            {
                localPos = new Vector3(0f, Mathf.Clamp(half.y * 0.08f, -half.y * 0.1f, half.y * 0.22f), 0f);
            }
            else
            {
                var face = Random.Range(0, 4);
                var offset = 0.035f;
                switch (face)
                {
                    case 0:
                        localPos = new Vector3(half.x + offset, Random.Range(-half.y + insetY, half.y - insetY), Random.Range(-half.z + insetXZ, half.z - insetXZ));
                        break;
                    case 1:
                        localPos = new Vector3(-half.x - offset, Random.Range(-half.y + insetY, half.y - insetY), Random.Range(-half.z + insetXZ, half.z - insetXZ));
                        break;
                    case 2:
                        localPos = new Vector3(Random.Range(-half.x + insetXZ, half.x - insetXZ), Random.Range(-half.y + insetY, half.y - insetY), half.z + offset);
                        break;
                    default:
                        localPos = new Vector3(Random.Range(-half.x + insetXZ, half.x - insetXZ), Random.Range(-half.y + insetY, half.y - insetY), -half.z - offset);
                        break;
                }
            }

            weakPointVisual.localPosition = localPos;
            weakPointVisual.localRotation = Quaternion.identity;
            weakPointVisual.localScale = Vector3.one * (weakPointWorldRadius * 2f);
            SetWeakPointVisible(true);
        }

        private bool IsWeakPointHit(Vector3 hitPoint)
        {
            if (!enableWeakPointCritical || !weakPointActive || weakPointVisual == null)
            {
                return false;
            }

            var tolerance = Mathf.Max(0.8f, weakPointHitTolerance);
            var distance = Vector3.Distance(hitPoint, weakPointVisual.position);
            return distance <= weakPointWorldRadius * tolerance;
        }

        private void UpdateWeakPointVisualPulse()
        {
            if (!weakPointActive || weakPointRenderer == null || weakPointVisual == null || !weakPointVisual.gameObject.activeSelf)
            {
                return;
            }

            var pulseSpeed = Mathf.Max(0.1f, weakPointPulseSpeed * (bossCoreExposureActive ? Mathf.Lerp(1.2f, 1.8f, Mathf.Clamp01((bossCoreExposureIntensity - 0.5f) / 2f)) : 1f));
            var pulse = 0.5f + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
            var colorA = bossCoreExposureActive ? Color.Lerp(weakPointColorA, Color.white, 0.28f) : weakPointColorA;
            var colorB = bossCoreExposureActive ? Color.Lerp(weakPointColorB, new Color(1f, 0.28f, 0.08f, 1f), 0.5f) : weakPointColorB;
            var color = Color.Lerp(colorA, colorB, pulse);

            weakPointRenderer.GetPropertyBlock(weakPointPropertyBlock);
            weakPointPropertyBlock.SetColor("_BaseColor", color);
            weakPointPropertyBlock.SetColor("_Color", color);
            weakPointRenderer.SetPropertyBlock(weakPointPropertyBlock);

            var sizePulse = bossCoreExposureActive
                ? 1.08f + (pulse - 0.5f) * Mathf.Lerp(0.26f, 0.4f, Mathf.Clamp01((bossCoreExposureIntensity - 0.5f) / 2f))
                : 1f + (pulse - 0.5f) * 0.14f;
            weakPointVisual.localScale = Vector3.one * (weakPointWorldRadius * 2f * sizePulse);

            UpdateBossCoreTelegraph(pulse, colorA, colorB);
        }

        private void SetWeakPointVisible(bool visible)
        {
            weakPointActive = visible;
            if (weakPointVisual == null)
            {
                return;
            }

            if (weakPointVisual.gameObject.activeSelf != visible)
            {
                weakPointVisual.gameObject.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            if (weakPointRenderer != null)
            {
                weakPointRenderer.GetPropertyBlock(weakPointPropertyBlock);
                weakPointPropertyBlock.SetColor("_BaseColor", weakPointColorA);
                weakPointPropertyBlock.SetColor("_Color", weakPointColorA);
                weakPointRenderer.SetPropertyBlock(weakPointPropertyBlock);
            }
        }

        private void UpdateBossCoreTelegraph(float pulse, Color colorA, Color colorB)
        {
            if (!bossCoreExposureActive || bossCoreRingVisual == null || bossCoreGroundTelegraphVisual == null)
            {
                return;
            }

            float exposure01 = Mathf.Clamp01((bossCoreExposureIntensity - 0.5f) / 2f);
            Color ringColor = Color.Lerp(Color.Lerp(colorA, Color.white, 0.2f), colorB, pulse);
            Color groundColor = Color.Lerp(colorA, new Color(1f, 0.46f, 0.16f, 1f), pulse * 0.7f);

            float ringRadius = weakPointWorldRadius * Mathf.Lerp(3.8f, 5.6f, exposure01) * (1.02f + pulse * 0.12f);
            float groundRadius = weakPointWorldRadius * Mathf.Lerp(7.5f, 10.5f, exposure01) * (0.98f + pulse * 0.06f);

            bossCoreRingVisual.localPosition = weakPointVisual != null ? new Vector3(weakPointVisual.localPosition.x, Mathf.Clamp(weakPointVisual.localPosition.y, -0.1f, 0.3f), weakPointVisual.localPosition.z) : Vector3.zero;
            bossCoreRingVisual.localRotation = Quaternion.identity;
            bossCoreRingVisual.localScale = new Vector3(ringRadius, 0.025f, ringRadius);

            bossCoreGroundTelegraphVisual.localPosition = new Vector3(0f, -Mathf.Max(0.04f, initialScale.y * 0.48f), 0f);
            bossCoreGroundTelegraphVisual.localRotation = Quaternion.identity;
            bossCoreGroundTelegraphVisual.localScale = new Vector3(groundRadius, 0.01f, groundRadius);

            if (bossCoreRingRenderer != null)
            {
                bossCoreRingRenderer.GetPropertyBlock(bossCoreRingPropertyBlock);
                bossCoreRingPropertyBlock.SetColor("_BaseColor", ringColor);
                bossCoreRingPropertyBlock.SetColor("_Color", ringColor);
                bossCoreRingRenderer.SetPropertyBlock(bossCoreRingPropertyBlock);
            }

            if (bossCoreGroundTelegraphRenderer != null)
            {
                bossCoreGroundTelegraphRenderer.GetPropertyBlock(bossCoreGroundTelegraphPropertyBlock);
                bossCoreGroundTelegraphPropertyBlock.SetColor("_BaseColor", Color.Lerp(groundColor, Color.white, 0.1f));
                bossCoreGroundTelegraphPropertyBlock.SetColor("_Color", Color.Lerp(groundColor, Color.white, 0.1f));
                bossCoreGroundTelegraphRenderer.SetPropertyBlock(bossCoreGroundTelegraphPropertyBlock);
            }
        }

        private void SetBossCoreTelegraphVisible(bool visible)
        {
            if (bossCoreRingVisual != null && bossCoreRingVisual.gameObject.activeSelf != visible)
            {
                bossCoreRingVisual.gameObject.SetActive(visible);
            }

            if (bossCoreGroundTelegraphVisual != null && bossCoreGroundTelegraphVisual.gameObject.activeSelf != visible)
            {
                bossCoreGroundTelegraphVisual.gameObject.SetActive(visible);
            }
        }
        private static Material GetSharedFxParticleMaterial()
        {
            if (sharedFxParticleMaterial != null)
            {
                return sharedFxParticleMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            sharedFxParticleMaterial = new Material(shader)
            {
                name = "M_Runtime_LargeBuildingFX"
            };

            if (sharedFxParticleMaterial.HasProperty("_Surface")) sharedFxParticleMaterial.SetFloat("_Surface", 1f);
            if (sharedFxParticleMaterial.HasProperty("_Blend")) sharedFxParticleMaterial.SetFloat("_Blend", 0f);
            return sharedFxParticleMaterial;
        }

        private static Vector3 SanitizeScale(Vector3 scale)
        {
            const float min = 0.08f;
            const float fallback = 1f;
            var x = Mathf.Abs(scale.x) < min ? fallback : scale.x;
            var y = Mathf.Abs(scale.y) < min ? fallback : scale.y;
            var z = Mathf.Abs(scale.z) < min ? fallback : scale.z;
            return new Vector3(x, y, z);
        }

        private void UpdateTint()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            var remainingRatio = maxDurability > 0f ? Mathf.Clamp01(currentDurability / maxDurability) : 0f;
            var brokenRatio = 1f - remainingRatio;

            var targetColor = currentDurability <= 0f
                ? destroyedColor
                : Color.Lerp(healthyColor, hitColor, brokenRatio);

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", targetColor);
            propertyBlock.SetColor("_Color", targetColor);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}


































