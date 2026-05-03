using System.Collections.Generic;
using AlienCrusher.Systems;
using DG.Tweening;
using UnityEngine;

namespace AlienCrusher.Gameplay
{
    [DisallowMultipleComponent]
    public class DummyStreetPropReactive : MonoBehaviour
    {
        public enum PropKind
        {
            Vehicle = 0,
            Lamp = 1,
            Tree = 2,
            ChainBarrel = 3,
            Transformer = 4
        }

        public struct PropBreakInfo
        {
            public PropKind Kind;
            public Vector3 Position;
            public float Impact01;
            public bool DrillMode;
        }

        public static event System.Action<PropBreakInfo> PropBroken;

        [SerializeField] private PropKind propKind = PropKind.Vehicle;
        [SerializeField] private Transform propRoot;
        [SerializeField] private float breakImpactThreshold = 4.8f;
        [SerializeField] private float wobbleImpactThreshold = 2.3f;
        [SerializeField] private float scatterForce = 5.8f;
        [SerializeField] private float upwardForce = 1.9f;
        [SerializeField] private int scoreReward = 22;
        [SerializeField] private float cleanupDelay = 4.6f;

        [Header("Chain Explosion")]
        [SerializeField] private bool enableChainExplosion = false;
        [SerializeField] private float chainExplosionRadius = 5.6f;
        [SerializeField] private Vector2 chainExplosionDamageRange = new Vector2(32f, 110f);
        [SerializeField] [Range(0f, 1f)] private float chainPropTriggerChance = 0.82f;
        [SerializeField] private int chainPropTriggerCap = 8;
        [SerializeField] private int chainDestructibleCap = 16;
        [SerializeField] private int chainBonusScore = 44;
        [SerializeField] [Range(0f, 1f)] private float chainFeedbackIntensity = 0.86f;

        private bool broken;
        private bool chainBurstTriggered;
        private bool stageLayoutVisible = true;
        private float baseBreakImpactThreshold;
        private float baseWobbleImpactThreshold;
        private int baseScoreReward;
        private float baseScatterForce;
        private float baseUpwardForce;
        private bool baseStatsCaptured;
        private FeedbackSystem feedbackSystem;
        private ScoreSystem scoreSystem;
        private DamageNumberSystem damageNumberSystem;
        private readonly HashSet<Transform> pieces = new HashSet<Transform>();

        public PropKind Kind => propKind;
        public bool IsBroken => broken;

        public void ConfigureForScaffolder(PropKind kind, Transform root)
        {
            propKind = kind;
            propRoot = root;
            enableChainExplosion = false;

            switch (propKind)
            {
                case PropKind.Vehicle:
                    wobbleImpactThreshold = 1.7f;
                    breakImpactThreshold = 3.25f;
                    scatterForce = 7.8f;
                    upwardForce = 2.8f;
                    scoreReward = 42;
                    cleanupDelay = 5.2f;
                    break;
                case PropKind.Lamp:
                    wobbleImpactThreshold = 2f;
                    breakImpactThreshold = 3.9f;
                    scatterForce = 4.5f;
                    upwardForce = 1.6f;
                    scoreReward = 20;
                    cleanupDelay = 4.2f;
                    break;
                case PropKind.Tree:
                    wobbleImpactThreshold = 1.8f;
                    breakImpactThreshold = 3.5f;
                    scatterForce = 4.1f;
                    upwardForce = 1.4f;
                    scoreReward = 18;
                    cleanupDelay = 4f;
                    break;
                case PropKind.ChainBarrel:
                    wobbleImpactThreshold = 1.6f;
                    breakImpactThreshold = 2.9f;
                    scatterForce = 6.8f;
                    upwardForce = 2.3f;
                    scoreReward = 46;
                    cleanupDelay = 4.8f;
                    enableChainExplosion = true;
                    chainExplosionRadius = 5.2f;
                    chainExplosionDamageRange = new Vector2(36f, 120f);
                    chainPropTriggerChance = 0.78f;
                    chainPropTriggerCap = 7;
                    chainDestructibleCap = 14;
                    chainBonusScore = 56;
                    chainFeedbackIntensity = 0.82f;
                    break;
                case PropKind.Transformer:
                    wobbleImpactThreshold = 1.5f;
                    breakImpactThreshold = 2.6f;
                    scatterForce = 7.4f;
                    upwardForce = 2.7f;
                    scoreReward = 54;
                    cleanupDelay = 5f;
                    enableChainExplosion = true;
                    chainExplosionRadius = 6.4f;
                    chainExplosionDamageRange = new Vector2(52f, 150f);
                    chainPropTriggerChance = 0.9f;
                    chainPropTriggerCap = 10;
                    chainDestructibleCap = 18;
                    chainBonusScore = 72;
                    chainFeedbackIntensity = 0.94f;
                    break;
            }

            CaptureBaseStats();
        }

        private void Awake()
        {
            if (propRoot == null)
            {
                propRoot = transform.parent != null ? transform.parent : transform;
            }

            CaptureBaseStats();

            feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
            scoreSystem = Object.FindFirstObjectByType<ScoreSystem>();
            damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
        }

        private void OnDisable()
        {
            if (propRoot != null)
            {
                propRoot.DOKill(false);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (broken || !isActiveAndEnabled || !stageLayoutVisible)
            {
                return;
            }

            var player = collision.gameObject.GetComponent<PlayerBallDummyController>()
                         ?? collision.gameObject.GetComponentInParent<PlayerBallDummyController>();
            if (player == null)
            {
                return;
            }

            var playerBody = player.GetComponent<Rigidbody>();
            var playerSpeed = playerBody != null
                ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z).magnitude
                : 0f;

            var root = propRoot != null ? propRoot : transform;
            var vehicleBody = root.GetComponent<Rigidbody>() ?? root.GetComponentInParent<Rigidbody>();
            var vehicleSpeed = vehicleBody != null
                ? new Vector3(vehicleBody.linearVelocity.x, 0f, vehicleBody.linearVelocity.z).magnitude
                : 0f;

            var relativeSpeed = collision.relativeVelocity.magnitude;
            var fallbackSpeed = Mathf.Max(relativeSpeed, playerSpeed * 0.78f, Mathf.Abs(playerSpeed - vehicleSpeed) * 1.05f);
            var impact = fallbackSpeed * Mathf.Max(0.58f, player.ImpactMultiplier);
            if (impact < wobbleImpactThreshold)
            {
                return;
            }

            var contact = collision.contactCount > 0 ? collision.GetContact(0).point : root.position;
            var impact01 = Mathf.InverseLerp(wobbleImpactThreshold, breakImpactThreshold * 2f, impact);

            if (impact < breakImpactThreshold)
            {
                root.DOKill(false);
                var wobble = Mathf.Lerp(6f, 15f, impact01);
                root.DOPunchRotation(new Vector3(0f, wobble, wobble * 0.45f), 0.12f, 8, 0.7f);
                feedbackSystem?.PlayHitFeedback(contact, Mathf.Clamp01(impact01 * 0.8f));
                return;
            }

            BreakProp(root, contact, impact01, player.DrillMode);
        }

        public void ApplyExternalBreak(Vector3 hitPoint, float impact01, bool drillMode, bool suppressFeedback = true)
        {
            if (broken || !isActiveAndEnabled || !stageLayoutVisible)
            {
                return;
            }

            var root = propRoot != null ? propRoot : transform;
            BreakProp(root, hitPoint, Mathf.Clamp01(impact01), drillMode, suppressFeedback);
        }

        private void BreakProp(Transform root, Vector3 hitPoint, float impact01, bool drillMode, bool suppressFeedback = false)
        {
            broken = true;
            feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
            scoreSystem ??= Object.FindFirstObjectByType<ScoreSystem>();
            damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();

            if (!suppressFeedback)
            {
                feedbackSystem?.PlayDestroyFeedback(hitPoint, Mathf.Clamp01(impact01));
            }

            scoreSystem?.AddScore(scoreReward);
            scoreSystem?.RegisterChainHit();
            var breakPosition = root != null ? root.position : transform.position;
            PropBroken?.Invoke(new PropBreakInfo
            {
                Kind = propKind,
                Position = breakPosition,
                Impact01 = Mathf.Clamp01(impact01),
                DrillMode = drillMode
            });

            pieces.Clear();
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null)
                {
                    continue;
                }

                var piece = col.transform;
                if (piece == null)
                {
                    continue;
                }

                pieces.Add(piece);
            }

            var drillMul = drillMode ? 1.25f : 1f;
            foreach (var piece in pieces)
            {
                if (piece == null)
                {
                    continue;
                }

                piece.SetParent(null, true);

                var rb = piece.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = piece.gameObject.AddComponent<Rigidbody>();
                }

                rb.mass = Mathf.Clamp(rb.mass, 0.08f, 0.9f);
                rb.linearDamping = 0.2f;
                rb.angularDamping = 0.05f;

                var forceDir = piece.position - hitPoint;
                if (forceDir.sqrMagnitude < 0.0001f)
                {
                    forceDir = Random.insideUnitSphere;
                }

                forceDir = forceDir.normalized;
                forceDir.y = Mathf.Abs(forceDir.y) + 0.35f;

                var force = scatterForce * drillMul * Mathf.Lerp(0.8f, 1.45f, Random.value) * Mathf.Lerp(0.65f, 1.2f, impact01);
                rb.AddForce(forceDir * force + Vector3.up * upwardForce, ForceMode.Impulse);

                Object.Destroy(piece.gameObject, cleanupDelay);
            }

            var shouldChainBurst = enableChainExplosion && IsChainKind(propKind) && !chainBurstTriggered;
            ShowBreakTag(breakPosition, impact01, shouldChainBurst);
            PlayBreakFlavorFeedback(breakPosition, impact01, suppressFeedback, shouldChainBurst);
            if (shouldChainBurst)
            {
                TriggerChainExplosion(hitPoint, impact01, drillMode, suppressFeedback);
            }

            if (root != null)
            {
                Object.Destroy(root.gameObject);
            }
        }

        private void TriggerChainExplosion(Vector3 center, float impact01, bool drillMode, bool suppressFeedback)
        {
            if (chainBurstTriggered)
            {
                return;
            }

            chainBurstTriggered = true;
            feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
            scoreSystem ??= Object.FindFirstObjectByType<ScoreSystem>();

            var radius = Mathf.Max(0.9f, chainExplosionRadius) * (drillMode ? 1.08f : 1f);
            var minDamage = Mathf.Max(6f, chainExplosionDamageRange.x) * (drillMode ? 1.06f : 1f);
            var maxDamage = Mathf.Max(minDamage + 6f, chainExplosionDamageRange.y) * (drillMode ? 1.1f : 1f);

            var destructibleHits = 0;
            var destructibles = Object.FindObjectsByType<DummyDestructibleBlock>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var radiusSqr = radius * radius;
            for (var i = 0; i < destructibles.Length; i++)
            {
                if (destructibleHits >= Mathf.Max(1, chainDestructibleCap))
                {
                    break;
                }

                var block = destructibles[i];
                if (block == null || !block.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var delta = block.transform.position - center;
                delta.y = 0f;
                var distSqr = delta.sqrMagnitude;
                if (distSqr > radiusSqr)
                {
                    continue;
                }

                var dist = Mathf.Sqrt(distSqr);
                var t = 1f - Mathf.Clamp01(dist / Mathf.Max(0.01f, radius));
                var damage = Mathf.Lerp(minDamage, maxDamage, t);
                var hitPoint = block.transform.position + Vector3.up * 0.42f;
                block.ApplyExternalImpactDamage(damage, hitPoint, Mathf.Lerp(0.58f, 1f, t), suppressFeedback: true);
                destructibleHits++;
            }

            var propHits = 0;
            var props = Object.FindObjectsByType<DummyStreetPropReactive>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < props.Length; i++)
            {
                if (propHits >= Mathf.Max(1, chainPropTriggerCap))
                {
                    break;
                }

                var prop = props[i];
                if (prop == null || prop == this || prop.broken || !prop.gameObject.activeInHierarchy)
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
                var chance = Mathf.Lerp(chainPropTriggerChance * 0.42f, chainPropTriggerChance, t);
                if (Random.value > Mathf.Clamp01(chance))
                {
                    continue;
                }

                var hitPoint = prop.transform.position + Vector3.up * 0.22f;
                prop.ApplyExternalBreak(hitPoint, Mathf.Lerp(0.55f, 1f, t), drillMode, suppressFeedback: true);
                propHits++;
            }

            var bonusScore = Mathf.Max(0, chainBonusScore + destructibleHits * 8 + propHits * 11);
            scoreSystem?.AddScore(bonusScore);

            if (!suppressFeedback && feedbackSystem != null)
            {
                var intensity = Mathf.Clamp01(chainFeedbackIntensity + impact01 * 0.18f + destructibleHits * 0.02f + propHits * 0.03f);
                feedbackSystem.PlayComboRushFeedback(center + Vector3.up * 0.18f, intensity, radius * 0.92f);
            }
        }

        private static bool IsChainKind(PropKind kind)
        {
            return kind == PropKind.ChainBarrel || kind == PropKind.Transformer;
        }

        private void ShowBreakTag(Vector3 breakPosition, float impact01, bool volatileBreak)
        {
            if (damageNumberSystem == null)
            {
                return;
            }

            var emphasis = volatileBreak || propKind == PropKind.Vehicle || impact01 >= 0.72f;
            damageNumberSystem.ShowPropTag(
                breakPosition + GetBreakTagOffset(),
                BuildBreakTagText(),
                emphasis,
                propKind == PropKind.Vehicle,
                volatileBreak);
        }

        private void PlayBreakFlavorFeedback(Vector3 breakPosition, float impact01, bool suppressFeedback, bool volatileBreak)
        {
            if (suppressFeedback || feedbackSystem == null)
            {
                return;
            }

            var vehicleBreak = propKind == PropKind.Vehicle;
            if (!vehicleBreak && !volatileBreak)
            {
                return;
            }

            var radius = vehicleBreak ? 2.4f : 3.2f;
            var intensity = vehicleBreak
                ? Mathf.Lerp(0.42f, 0.74f, Mathf.Clamp01(impact01))
                : Mathf.Lerp(0.62f, 0.92f, Mathf.Clamp01(impact01));
            feedbackSystem.PlayComboRushFeedback(breakPosition + Vector3.up * 0.18f, intensity, radius);
        }

        private string BuildBreakTagText()
        {
            switch (propKind)
            {
                case PropKind.Vehicle:
                    return $"CAR CRUSH +{Mathf.Max(0, scoreReward)}";
                case PropKind.Lamp:
                    return $"LAMP DOWN +{Mathf.Max(0, scoreReward)}";
                case PropKind.Tree:
                    return $"TREE DOWN +{Mathf.Max(0, scoreReward)}";
                case PropKind.ChainBarrel:
                    return $"BARREL BURST +{Mathf.Max(0, scoreReward)}";
                case PropKind.Transformer:
                    return $"GRID BURST +{Mathf.Max(0, scoreReward)}";
                default:
                    return $"+{Mathf.Max(0, scoreReward)}";
            }
        }

        private Vector3 GetBreakTagOffset()
        {
            switch (propKind)
            {
                case PropKind.Vehicle:
                    return new Vector3(0f, 1.05f, 0f);
                case PropKind.ChainBarrel:
                case PropKind.Transformer:
                    return new Vector3(0f, 1.12f, 0f);
                default:
                    return new Vector3(0f, 0.88f, 0f);
            }
        }

        public void ApplyStageLayoutTuning(int stageNumber)
        {
            CaptureBaseStats();

            var safeStage = Mathf.Max(1, stageNumber);
            var pos = propRoot != null ? propRoot.position : transform.position;
            var laneDepth = pos.z;
            var unlockStage = 1;

            switch (propKind)
            {
                case PropKind.Vehicle:
                    unlockStage = laneDepth > 10f ? 2 : 1;
                    break;
                case PropKind.Lamp:
                    unlockStage = laneDepth > 12f ? 2 : 1;
                    break;
                case PropKind.Tree:
                    unlockStage = laneDepth > 8f ? 2 : 1;
                    break;
                case PropKind.ChainBarrel:
                    unlockStage = laneDepth > 6f ? 2 : 1;
                    break;
                case PropKind.Transformer:
                    unlockStage = laneDepth > 8f ? 3 : 2;
                    break;
            }

            stageLayoutVisible = safeStage >= unlockStage;

            wobbleImpactThreshold = baseWobbleImpactThreshold;
            breakImpactThreshold = baseBreakImpactThreshold;
            scatterForce = baseScatterForce;
            upwardForce = baseUpwardForce;
            scoreReward = baseScoreReward;

            if (stageLayoutVisible)
            {
                var stageAdvance = Mathf.Max(0, safeStage - unlockStage);
                var toughnessScale = 1f + Mathf.Clamp(stageAdvance * 0.08f, 0f, 0.36f);
                var rewardScale = 1f + Mathf.Clamp(stageAdvance * 0.1f, 0f, 0.5f);
                breakImpactThreshold *= toughnessScale;
                wobbleImpactThreshold *= Mathf.Lerp(0.95f, 1.18f, Mathf.Clamp01(stageAdvance / 4f));
                scoreReward = Mathf.Max(1, Mathf.RoundToInt(baseScoreReward * rewardScale));
            }

            var target = propRoot != null ? propRoot.gameObject : gameObject;
            if (target != null && target.activeSelf != stageLayoutVisible)
            {
                target.SetActive(stageLayoutVisible);
            }
        }

        private void CaptureBaseStats()
        {
            if (baseStatsCaptured)
            {
                return;
            }

            baseBreakImpactThreshold = breakImpactThreshold;
            baseWobbleImpactThreshold = wobbleImpactThreshold;
            baseScoreReward = scoreReward;
            baseScatterForce = scatterForce;
            baseUpwardForce = upwardForce;
            baseStatsCaptured = true;
        }
    }
}
