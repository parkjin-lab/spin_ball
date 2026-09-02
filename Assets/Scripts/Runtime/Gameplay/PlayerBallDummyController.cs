using System;
using System.Collections.Generic;
using AlienCrusher.Systems;
using AlienCrusher.UI;
using UnityEngine;
using Object = UnityEngine.Object;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AlienCrusher.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerBallDummyController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float acceleration = 26f;
        [SerializeField] private float maxPlanarSpeed = 9f;
        [SerializeField] private float planarDrag = 4.5f;
        [SerializeField] private float inputSmoothingAttack = 22f;
        [SerializeField] private float inputSmoothingRelease = 12f;
        [SerializeField] [Range(0.6f, 1.5f)] private float inputResponseExponent = 0.85f;
        [SerializeField] [Range(0f, 0.45f)] private float inputSensitivityBoost = 0.22f;

        [Header("Collision")]
        [SerializeField] private float bounceCoefficient = 0.7f;

        [Header("Landing Shockwave")]
        [SerializeField] private bool enableLandingShockwaveSignal = true;
        [SerializeField] private float landingShockwaveMinDownwardSpeed = 6.2f;
        [SerializeField] private float landingShockwaveMinPlanarSpeed = 4.8f;
        [SerializeField] private float landingShockwaveCooldown = 0.28f;
        [SerializeField] [Range(0.2f, 0.95f)] private float landingShockwaveGroundNormalMinY = 0.48f;

        [Header("References")]
        [SerializeField] private VirtualJoystickUI virtualJoystick;
        [SerializeField] private Transform vfxAnchor;

        [Header("Form")]
        [SerializeField] private FormType baseForm = FormType.Sphere;

        [Header("Counter Surge Visual")]
        [SerializeField] private bool enableCounterSurgeVisual = true;
        [SerializeField] private Color counterSurgeTintA = new Color(0.4f, 0.92f, 1f, 1f);
        [SerializeField] private Color counterSurgeTintB = new Color(0.15f, 0.54f, 1f, 1f);
        [SerializeField] [Range(0f, 1f)] private float counterSurgeTintStrength = 0.38f;
        [SerializeField] private float counterSurgePulseSpeed = 8.8f;
        [SerializeField] private float counterSurgeEmissionBoost = 0.95f;
        [SerializeField] private bool enableCounterSurgeTrail = true;
        [SerializeField] private float counterSurgeTrailTime = 0.2f;
        [SerializeField] private float counterSurgeTrailMinWidth = 0.14f;
        [SerializeField] private float counterSurgeTrailMaxWidth = 0.42f;
        [SerializeField] [Range(0f, 1f)] private float counterSurgeTrailAlpha = 0.68f;

        private Rigidbody body;
        private FeedbackSystem feedbackSystem;
        private Renderer cachedRenderer;
        private Transform drillVisual;
        private Renderer drillRenderer;
        private Renderer drillTipRenderer;
        private Transform spikeVisual;
        private Transform ramVisual;
        private Transform saucerVisual;
        private Transform crusherVisual;
        private Transform saucerRayEmitter;
        private Transform crusherMagnetRing;
        private Transform sphereVisual;
        private Renderer sphereBeltRenderer;
        private Material sphereBeltMaterial;
        private Color sphereBeltBaseColor = new Color(0.62f, 1f, 0.38f, 1f);
        private Color sphereBeltEmissionColor = new Color(0.34f, 0.92f, 0.28f, 1f);
        private float spherePulseMarkUntil;
        private Material ramPlateMaterial;
        private Color ramPlateBaseColor = new Color(0.92f, 0.58f, 0.18f, 1f);
        private Color ramPlateEmissionColor = new Color(0.78f, 0.36f, 0.06f, 1f);
        private float ramBreachMarkUntil;
        private Material saucerRimMaterial;
        private Color saucerRimBaseColor = new Color(0.28f, 0.82f, 0.88f, 1f);
        private Color saucerRimEmissionColor = new Color(0.16f, 0.58f, 0.66f, 1f);
        private float saucerDashMarkUntil;
        private Material spikeTipMaterial;
        private Color spikeTipBaseColor = new Color(0.78f, 1f, 0.16f, 1f);
        private Color spikeTipEmissionColor = new Color(0.52f, 0.82f, 0.08f, 1f);
        private float spikeBurstMarkUntil;
        private Material crusherSeamMaterial;
        private Color crusherSeamBaseColor = new Color(0.35f, 0.72f, 1f, 1f);
        private Color crusherSeamEmissionColor = new Color(0.18f, 0.42f, 0.85f, 1f);
        private float crusherSlamMarkUntil;

        private float baseAcceleration;
        private float baseMaxPlanarSpeed;
        private float basePlanarDrag;
        private float baseBounceCoefficient;

        private float speedMultiplier = 1f;
        private float impactMultiplier = 1f;
        private float temporarySpeedMultiplier = 1f;
        private float temporaryImpactMultiplier = 1f;
        private float permanentImpactMultiplier = 1f;
        private bool drillMode;
        private FormType currentBaseForm = FormType.Sphere;
        private float formSpeedMultiplier = 1f;
        private float formImpactMultiplier = 1f;
        private float lastLandingShockwaveAt = -10f;
        private Vector2 smoothedMovementInput = Vector2.zero;
        private float externalSlowMultiplier = 1f;
        private float externalSlowUntilTime;
        private float counterSpeedMultiplier = 1f;
        private float counterImpactMultiplier = 1f;
        private float counterUntilTime;
        private readonly List<Renderer> counterSurgeRenderers = new List<Renderer>(16);
        private MaterialPropertyBlock counterSurgePropertyBlock;
        private float counterSurgeVisualDurationReference = 1f;
        private bool counterSurgeVisualApplied;
        private TrailRenderer counterSurgeTrail;
        private Material counterSurgeTrailMaterial;
        private bool counterSurgeTrailActive;

        public readonly struct LandingShockwaveData
        {
            public readonly Vector3 Point;
            public readonly float Intensity;
            public readonly float PlanarSpeed;
            public readonly float DownwardSpeed;
            public readonly bool DrillMode;
            public readonly float ImpactMultiplier;

            public LandingShockwaveData(Vector3 point, float intensity, float planarSpeed, float downwardSpeed, bool drillMode, float impactMultiplier)
            {
                Point = point;
                Intensity = intensity;
                PlanarSpeed = planarSpeed;
                DownwardSpeed = downwardSpeed;
                DrillMode = drillMode;
                ImpactMultiplier = impactMultiplier;
            }
        }

        public event Action<LandingShockwaveData> LandingShockwaveTriggered;

        public Transform VfxAnchor => vfxAnchor;
        public FormType CurrentBaseForm => currentBaseForm;
        public float ImpactMultiplier => impactMultiplier * temporaryImpactMultiplier * permanentImpactMultiplier * formImpactMultiplier * counterImpactMultiplier;
        public bool DrillMode => drillMode;
        public bool HasCounterSurge => counterUntilTime > Time.time;
        public float CounterSurgeRemaining => Mathf.Max(0f, counterUntilTime - Time.time);

        public float GetBodySmashScale()
        {
            return FormCatalog.GetSmashMethod(currentBaseForm) switch
            {
                FormSmashMethod.UfoRay => 0.12f,
                FormSmashMethod.MagnetGrab => 0.42f,
                FormSmashMethod.ChargeBurst => 0.68f,
                FormSmashMethod.DrillBurrow => 0.52f,
                _ => 1f
            };
        }

        public Vector3 GetPlanarFacing()
        {
            if (CollisionContactGuard.IsUnityAlive(body))
            {
                var velocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
                if (velocity.sqrMagnitude > 0.04f)
                {
                    return velocity.normalized;
                }
            }

            var input = smoothedMovementInput;
            if (input.sqrMagnitude > 0.01f)
            {
                return new Vector3(input.x, 0f, input.y).normalized;
            }

            var forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        public void SetFormSmashCharge(float charge01)
        {
            if (currentBaseForm != FormType.Ram || ramVisual == null)
            {
                return;
            }

            var pulse = Mathf.Clamp01(charge01);
            ramVisual.localScale = Vector3.one * (1f + pulse * 0.18f);
            ApplyFormAccentPulse(ramPlateMaterial, ramPlateBaseColor, ramPlateEmissionColor, new Color(1f, 0.82f, 0.4f, 1f), pulse);
        }

        public void SetFormSmashBeam(bool active, Vector3 facing, float length)
        {
            EnsureSaucerRayEmitter();
            if (saucerRayEmitter == null)
            {
                return;
            }

            saucerRayEmitter.gameObject.SetActive(active && currentBaseForm == FormType.Saucer);
            if (!active)
            {
                return;
            }

            facing.y = 0f;
            if (facing.sqrMagnitude < 0.0001f)
            {
                facing = Vector3.forward;
            }

            facing.Normalize();
            var safeLength = Mathf.Clamp(length, 1.2f, 10f);
            saucerRayEmitter.localPosition = new Vector3(0f, -0.08f, safeLength * 0.18f);
            saucerRayEmitter.localRotation = Quaternion.Euler(90f, 0f, 0f);
            saucerRayEmitter.localScale = new Vector3(0.18f, 0.08f, 0.18f);
            ApplyFormAccentPulse(saucerRimMaterial, saucerRimBaseColor, saucerRimEmissionColor, new Color(0.72f, 0.98f, 1f, 1f), 1f);
        }

        public void SetFormSmashMagnet(bool active)
        {
            EnsureCrusherMagnetRing();
            if (crusherMagnetRing == null)
            {
                return;
            }

            crusherMagnetRing.gameObject.SetActive(active && currentBaseForm == FormType.Crusher);
            if (active)
            {
                ApplyFormAccentPulse(crusherSeamMaterial, crusherSeamBaseColor, crusherSeamEmissionColor, new Color(0.7f, 0.9f, 1f, 1f), 1f);
            }
        }

        public void SetBaseForm(FormType form)
        {
            currentBaseForm = form;
            baseForm = form;
            ApplyFormStats();
            if (ramVisual != null)
            {
                ramVisual.localScale = Vector3.one;
            }

            if (saucerVisual != null && form != FormType.Saucer)
            {
                saucerVisual.localPosition = Vector3.zero;
            }

            SetFormSmashCharge(0f);
            SetFormSmashBeam(false, Vector3.forward, 0f);
            SetFormSmashMagnet(false);
            UpdateFormVisibility();
            UpdateCounterSurgeVisual();
        }
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            counterSurgePropertyBlock = new MaterialPropertyBlock();
            if (virtualJoystick == null)
            {
                virtualJoystick = Object.FindAnyObjectByType<VirtualJoystickUI>(FindObjectsInactive.Include);
            }

            feedbackSystem = Object.FindAnyObjectByType<FeedbackSystem>();

            cachedRenderer = GetComponent<Renderer>();
            EnsureDrillVisual();
            EnsureSpikeVisual();
            EnsureRamVisual();
            EnsureSaucerVisual();
            EnsureCrusherVisual();
            EnsureSphereVisual();
            currentBaseForm = baseForm;
            UpdateFormVisibility();

            baseAcceleration = acceleration;
            baseMaxPlanarSpeed = maxPlanarSpeed;
            basePlanarDrag = planarDrag;
            baseBounceCoefficient = bounceCoefficient;
            ApplyFormStats();
            RebuildCounterSurgeRenderers();
            EnsureCounterSurgeTrail();
            UpdateCounterSurgeVisual();
        }

        private void OnDisable()
        {
            ResetCounterSurgeVisual();
        }

        private void Update()
        {
            if (externalSlowUntilTime > 0f && Time.time >= externalSlowUntilTime)
            {
                externalSlowUntilTime = 0f;
                externalSlowMultiplier = 1f;
            }

            if (counterUntilTime > 0f && Time.time >= counterUntilTime)
            {
                counterUntilTime = 0f;
                counterSpeedMultiplier = 1f;
                counterImpactMultiplier = 1f;
                counterSurgeVisualDurationReference = 1f;
            }

            UpdateCounterSurgeVisual();
            TickSpherePulseMark();
            TickRamBreachMark();
            TickSaucerDashMark();
            TickSpikeBurstMark();
            TickCrusherSlamMark();
            TickSaucerHover();
        }
        private void FixedUpdate()
        {
            if (!CollisionContactGuard.IsUnityAlive(this) || !CollisionContactGuard.IsUnityAlive(body))
            {
                return;
            }

            var input = ProcessMovementInput(GetMovementInput());
            var hasInput = input.sqrMagnitude > 0.0001f;

            if (hasInput)
            {
                var direction = new Vector3(input.x, 0f, input.y);
                var effectiveSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier * temporarySpeedMultiplier * formSpeedMultiplier * externalSlowMultiplier * counterSpeedMultiplier);
                var effectiveAcceleration = baseAcceleration * effectiveSpeedMultiplier;
                var force = direction * (effectiveAcceleration * Mathf.Clamp01(input.magnitude));
                body.AddForce(force, ForceMode.Acceleration);
            }

            ClampPlanarSpeed();
            ApplyPlanarDrag(hasInput);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!CollisionContactGuard.IsBehaviourLive(this)
                || !CollisionContactGuard.IsUnityAlive(body)
                || collision == null
                || collision.contactCount <= 0
                || !CollisionContactGuard.TryGetLiveOther(collision, out _, out var other))
            {
                return;
            }

            var contact = collision.GetContact(0);
            var normal = contact.normal;
            var preVelocity = body.linearVelocity;
            TryTriggerLandingShockwave(contact.point, normal, preVelocity);

            var reflected = Vector3.Reflect(preVelocity, normal) * bounceCoefficient;
            reflected.y = preVelocity.y;
            body.linearVelocity = reflected;

            var hitDestructible = other.GetComponent<DummyDestructibleBlock>() != null ||
                                  other.GetComponentInParent<DummyDestructibleBlock>() != null;
            if (hitDestructible)
            {
                return;
            }

            var impact = collision.relativeVelocity.magnitude;
            if (impact < 3f)
            {
                return;
            }

            feedbackSystem ??= Object.FindAnyObjectByType<FeedbackSystem>();
            if (feedbackSystem == null)
            {
                return;
            }

            var impact01 = Mathf.InverseLerp(3f, 13f, impact);
            var point = contact.point;
            feedbackSystem.PlayHitFeedback(point, impact01 * 0.8f);
        }

        public void ResetUpgrades()
        {
            speedMultiplier = 1f;
            impactMultiplier = 1f;
            temporarySpeedMultiplier = 1f;
            temporaryImpactMultiplier = 1f;
            drillMode = false;
            SetFormSmashCharge(0f);
            SetFormSmashBeam(false, Vector3.forward, 0f);
            SetFormSmashMagnet(false);
            UpdateFormVisibility();

            acceleration = baseAcceleration;
            maxPlanarSpeed = baseMaxPlanarSpeed;
            planarDrag = basePlanarDrag;
            bounceCoefficient = baseBounceCoefficient;
            ApplyFormStats();
            smoothedMovementInput = Vector2.zero;
            externalSlowMultiplier = 1f;
            externalSlowUntilTime = 0f;
            counterSpeedMultiplier = 1f;
            counterImpactMultiplier = 1f;
            counterUntilTime = 0f;
            counterSurgeVisualDurationReference = 1f;
            ResetCounterSurgeVisual();
        }

        public void ApplySpeedBoost(float percent)
        {
            var scale = 1f + Mathf.Max(0f, percent);
            speedMultiplier *= scale;
        }

        
        public void ApplyImpactBoost(float percent)
        {
            var scale = 1f + Mathf.Max(0f, percent);
            impactMultiplier *= scale;
        }
        public void SetPermanentImpactMultiplier(float multiplier)
        {
            permanentImpactMultiplier = Mathf.Clamp(multiplier, 0.25f, 6f);
        }

        public void ApplyDrillMode()
        {
            drillMode = true;
            SetDrillVisualActive(true);
            bounceCoefficient = Mathf.Clamp(baseBounceCoefficient * 0.55f, 0.1f, 0.8f);
            UpdateCounterSurgeVisual();
        }

        public void ApplyTemporaryOverdrive(float speedScale, float impactScale)
        {
            temporarySpeedMultiplier = Mathf.Max(1f, speedScale);
            temporaryImpactMultiplier = Mathf.Max(1f, impactScale);
        }


        public void ApplyExternalSlow(float speedScale, float duration)
        {
            var safeScale = Mathf.Clamp(speedScale, 0.25f, 1f);
            externalSlowMultiplier = Mathf.Min(externalSlowMultiplier, safeScale);
            externalSlowUntilTime = Mathf.Max(externalSlowUntilTime, Time.time + Mathf.Max(0.05f, duration));
        }

        public void ApplyCounterSurge(float speedScale, float impactScale, float duration)
        {
            counterSpeedMultiplier = Mathf.Max(counterSpeedMultiplier, Mathf.Max(1f, speedScale));
            counterImpactMultiplier = Mathf.Max(counterImpactMultiplier, Mathf.Max(1f, impactScale));
            counterUntilTime = Mathf.Max(counterUntilTime, Time.time + Mathf.Max(0.05f, duration));
            counterSurgeVisualDurationReference = Mathf.Max(counterSurgeVisualDurationReference, Mathf.Max(0.05f, duration));
            UpdateCounterSurgeVisual();
        }
        public void ClearTemporaryOverdrive()
        {
            temporarySpeedMultiplier = 1f;
            temporaryImpactMultiplier = 1f;
        }

        private void EnsureDrillVisual()
        {
            if (drillVisual != null)
            {
                return;
            }

            var root = transform.Find("_DrillForm");
            if (root == null)
            {
                var rootGo = new GameObject("_DrillForm");
                root = rootGo.transform;
                root.SetParent(transform, false);
            }

            var bodyGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyGo.name = "DrillBody";
            bodyGo.transform.SetParent(root, false);
            bodyGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            bodyGo.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            bodyGo.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
            RemoveCollider(bodyGo);

            var tipGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tipGo.name = "DrillTip";
            tipGo.transform.SetParent(root, false);
            tipGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tipGo.transform.localPosition = new Vector3(0f, 0f, 0.85f);
            tipGo.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
            RemoveCollider(tipGo);

            drillVisual = root;
            drillRenderer = bodyGo.GetComponent<Renderer>();
            drillTipRenderer = tipGo.GetComponent<Renderer>();

            if (cachedRenderer != null)
            {
                if (drillRenderer != null) drillRenderer.sharedMaterial = cachedRenderer.sharedMaterial;
                if (drillTipRenderer != null) drillTipRenderer.sharedMaterial = cachedRenderer.sharedMaterial;
            }
        }

        private void EnsureSpikeVisual()
        {
            if (spikeVisual != null)
            {
                return;
            }

            var root = GetOrCreateFormIdentityKit("FORM_Spike_Body_Kit", "_SpikeForm");
            DeactivateNamedChild(root, "Spike_PosX");
            DeactivateNamedChild(root, "Spike_NegX");
            DeactivateNamedChild(root, "Spike_PosZ");
            DeactivateNamedChild(root, "Spike_NegZ");
            DeactivateNamedChild(root, "Spike_PosY");
            DeactivateNamedChild(root, "Spike_NegY");

            var body = new Color(0.1f, 0.07f, 0.12f, 1f);
            var needle = new Color(0.22f, 0.12f, 0.18f, 1f);
            var bodyGlow = new Color(0.08f, 0.04f, 0.06f, 1f);

            var coreGo = EnsureFormIdentityPrimitive(root, "SpikeCore", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.58f, 0.58f, 0.58f), Vector3.zero);
            ApplyFormIdentityMaterial(coreGo, "M_Runtime_SpikeCore", body, bodyGlow);

            EnsureFormIdentityPrimitive(root, "Spike_Fwd", PrimitiveType.Cylinder, new Vector3(0f, 0.08f, 0.78f), new Vector3(0.12f, 0.52f, 0.12f), new Vector3(90f, 0f, 0f));
            ApplyFormIdentityMaterial(root.Find("Spike_Fwd").gameObject, "M_Runtime_SpikeFwd", needle, spikeTipEmissionColor);
            EnsureFormIdentityPrimitive(root, "Spike_Back", PrimitiveType.Cylinder, new Vector3(0f, 0f, -0.66f), new Vector3(0.11f, 0.36f, 0.11f), new Vector3(90f, 0f, 0f));
            ApplyFormIdentityMaterial(root.Find("Spike_Back").gameObject, "M_Runtime_SpikeBack", needle, bodyGlow);
            EnsureFormIdentityPrimitive(root, "Spike_Left", PrimitiveType.Cylinder, new Vector3(-0.66f, 0.06f, 0.08f), new Vector3(0.11f, 0.34f, 0.11f), new Vector3(0f, 0f, 90f));
            ApplyFormIdentityMaterial(root.Find("Spike_Left").gameObject, "M_Runtime_SpikeLeft", needle, bodyGlow);
            EnsureFormIdentityPrimitive(root, "Spike_Right", PrimitiveType.Cylinder, new Vector3(0.66f, 0.06f, 0.08f), new Vector3(0.11f, 0.34f, 0.11f), new Vector3(0f, 0f, 90f));
            ApplyFormIdentityMaterial(root.Find("Spike_Right").gameObject, "M_Runtime_SpikeRight", needle, bodyGlow);
            EnsureFormIdentityPrimitive(root, "Spike_Up", PrimitiveType.Cylinder, new Vector3(0f, 0.82f, 0.1f), new Vector3(0.12f, 0.46f, 0.12f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("Spike_Up").gameObject, "M_Runtime_SpikeUp", needle, spikeTipEmissionColor);
            EnsureFormIdentityPrimitive(root, "Spike_Down", PrimitiveType.Cylinder, new Vector3(0f, -0.6f, 0f), new Vector3(0.11f, 0.3f, 0.11f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("Spike_Down").gameObject, "M_Runtime_SpikeDown", needle, bodyGlow);

            var tipGo = EnsureFormIdentityPrimitive(root, "SpikeTip_Fwd", PrimitiveType.Cube, new Vector3(0f, 0.08f, 1.3f), new Vector3(0.12f, 0.12f, 0.16f), Vector3.zero);
            spikeTipMaterial = ApplyFormIdentityMaterial(tipGo, "M_Runtime_SpikeTip", spikeTipBaseColor, spikeTipEmissionColor);
            EnsureFormIdentityPrimitive(root, "SpikeTip_Up", PrimitiveType.Cube, new Vector3(0f, 1.28f, 0.1f), new Vector3(0.12f, 0.16f, 0.12f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("SpikeTip_Up").gameObject, "M_Runtime_SpikeTipUp", spikeTipBaseColor, spikeTipEmissionColor);

            spikeVisual = root;
            spikeVisual.gameObject.SetActive(false);
        }

        private void EnsureRamVisual()
        {
            if (ramVisual != null)
            {
                return;
            }

            var root = GetOrCreateFormIdentityKit("FORM_Ram_Body_Kit", "_RamForm");
            DeactivateNamedChild(root, "RamBody");
            DeactivateNamedChild(root, "RamHorn");

            var shellColor = new Color(0.28f, 0.16f, 0.08f, 1f);
            var shellGlow = new Color(0.12f, 0.06f, 0.02f, 1f);
            var hornColor = new Color(0.86f, 0.48f, 0.14f, 1f);

            EnsureFormIdentityPrimitive(root, "RamShell", PrimitiveType.Cube, new Vector3(0f, -0.02f, -0.08f), new Vector3(0.98f, 0.7f, 1.08f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("RamShell") != null ? root.Find("RamShell").gameObject : null, "M_Runtime_RamShell", shellColor, shellGlow);

            EnsureFormIdentityPrimitive(root, "RamWedge", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.38f), new Vector3(0.78f, 0.52f, 0.72f), new Vector3(0f, 45f, 0f));
            ApplyFormIdentityMaterial(root.Find("RamWedge") != null ? root.Find("RamWedge").gameObject : null, "M_Runtime_RamWedge", ramPlateBaseColor, ramPlateEmissionColor);

            var plateGo = EnsureFormIdentityPrimitive(root, "RamPlate", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0.78f), new Vector3(0.48f, 0.36f, 0.2f), Vector3.zero);
            ramPlateMaterial = ApplyFormIdentityMaterial(plateGo, "M_Runtime_RamPlate", ramPlateBaseColor, ramPlateEmissionColor);

            EnsureFormIdentityPrimitive(root, "RamHorn_L", PrimitiveType.Cylinder, new Vector3(-0.46f, 0.1f, 0.52f), new Vector3(0.16f, 0.38f, 0.16f), new Vector3(78f, 0f, 18f));
            ApplyFormIdentityMaterial(root.Find("RamHorn_L") != null ? root.Find("RamHorn_L").gameObject : null, "M_Runtime_RamHornL", hornColor, ramPlateEmissionColor);
            EnsureFormIdentityPrimitive(root, "RamHorn_R", PrimitiveType.Cylinder, new Vector3(0.46f, 0.1f, 0.52f), new Vector3(0.16f, 0.38f, 0.16f), new Vector3(78f, 0f, -18f));
            ApplyFormIdentityMaterial(root.Find("RamHorn_R") != null ? root.Find("RamHorn_R").gameObject : null, "M_Runtime_RamHornR", hornColor, ramPlateEmissionColor);

            ramVisual = root;
            ramVisual.gameObject.SetActive(false);
        }

        private void EnsureSaucerVisual()
        {
            if (saucerVisual != null)
            {
                return;
            }

            var root = GetOrCreateFormIdentityKit("FORM_Saucer_Body_Kit", "_SaucerForm");
            DeactivateNamedChild(root, "SaucerBody");
            DeactivateNamedChild(root, "SaucerCore");

            var pale = new Color(0.78f, 0.88f, 0.92f, 1f);
            var paleGlow = new Color(0.22f, 0.36f, 0.4f, 1f);
            var dome = new Color(0.55f, 0.9f, 0.94f, 1f);

            var rimGo = EnsureFormIdentityPrimitive(root, "SaucerRim", PrimitiveType.Cylinder, new Vector3(0f, -0.02f, 0f), new Vector3(1.78f, 0.1f, 1.78f), Vector3.zero);
            saucerRimMaterial = ApplyFormIdentityMaterial(rimGo, "M_Runtime_SaucerRim", saucerRimBaseColor, saucerRimEmissionColor);

            EnsureFormIdentityPrimitive(root, "SaucerDisc", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0f), new Vector3(1.38f, 0.12f, 1.38f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("SaucerDisc") != null ? root.Find("SaucerDisc").gameObject : null, "M_Runtime_SaucerDisc", pale, paleGlow);

            EnsureFormIdentityPrimitive(root, "SaucerDome", PrimitiveType.Sphere, new Vector3(0f, 0.16f, 0f), new Vector3(0.42f, 0.2f, 0.42f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("SaucerDome") != null ? root.Find("SaucerDome").gameObject : null, "M_Runtime_SaucerDome", dome, saucerRimEmissionColor);

            EnsureFormIdentityPrimitive(root, "SaucerCanopy", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0.12f), new Vector3(0.22f, 0.1f, 0.18f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("SaucerCanopy") != null ? root.Find("SaucerCanopy").gameObject : null, "M_Runtime_SaucerCanopy", new Color(0.86f, 0.96f, 0.98f, 1f), paleGlow);

            EnsureSaucerRayEmitter(root);
            saucerVisual = root;
            saucerVisual.gameObject.SetActive(false);
        }

        private void EnsureSaucerRayEmitter(Transform root = null)
        {
            if (saucerRayEmitter != null)
            {
                return;
            }

            var parent = root != null ? root : saucerVisual;
            if (parent == null)
            {
                return;
            }

            var emitterGo = EnsureFormIdentityPrimitive(parent, "SaucerRayEmitter", PrimitiveType.Cylinder, new Vector3(0f, -0.12f, 0f), new Vector3(0.22f, 0.08f, 0.22f), Vector3.zero);
            ApplyFormIdentityMaterial(emitterGo, "M_Runtime_SaucerRayEmitter", new Color(0.55f, 0.95f, 1f, 1f), saucerRimEmissionColor);
            saucerRayEmitter = emitterGo.transform;
            saucerRayEmitter.gameObject.SetActive(currentBaseForm == FormType.Saucer);
        }

        private void TickSaucerHover()
        {
            if (saucerVisual == null || currentBaseForm != FormType.Saucer || drillMode)
            {
                return;
            }

            var bob = Mathf.Sin(Time.time * 3.4f) * 0.06f;
            saucerVisual.localPosition = new Vector3(0f, 0.08f + bob, 0f);
        }

        public void PlayRamBreachVisualCue()
        {
            ramBreachMarkUntil = Time.time + 0.26f;
            ApplyFormAccentPulse(ramPlateMaterial, ramPlateBaseColor, ramPlateEmissionColor, new Color(1f, 0.82f, 0.4f, 1f), 1f);
        }

        public void PlaySaucerDashVisualCue()
        {
            saucerDashMarkUntil = Time.time + 0.26f;
            ApplyFormAccentPulse(saucerRimMaterial, saucerRimBaseColor, saucerRimEmissionColor, new Color(0.72f, 0.98f, 1f, 1f), 1f);
        }
        public void PlaySpikeBurstVisualCue()
        {
            spikeBurstMarkUntil = Time.time + 0.24f;
            ApplyFormAccentPulse(spikeTipMaterial, spikeTipBaseColor, spikeTipEmissionColor, new Color(1f, 1f, 0.55f, 1f), 1f);
        }

        public void PlayCrusherSlamVisualCue()
        {
            crusherSlamMarkUntil = Time.time + 0.3f;
            ApplyFormAccentPulse(crusherSeamMaterial, crusherSeamBaseColor, crusherSeamEmissionColor, new Color(0.7f, 0.9f, 1f, 1f), 1f);
        }

        private void TickSpikeBurstMark()
        {
            TickFormAccentPulse(ref spikeBurstMarkUntil, spikeTipMaterial, spikeTipBaseColor, spikeTipEmissionColor, new Color(1f, 1f, 0.55f, 1f));
        }

        private void TickCrusherSlamMark()
        {
            TickFormAccentPulse(ref crusherSlamMarkUntil, crusherSeamMaterial, crusherSeamBaseColor, crusherSeamEmissionColor, new Color(0.7f, 0.9f, 1f, 1f));
        }


        private void TickRamBreachMark()
        {
            TickFormAccentPulse(ref ramBreachMarkUntil, ramPlateMaterial, ramPlateBaseColor, ramPlateEmissionColor, new Color(1f, 0.82f, 0.4f, 1f));
        }

        private void TickSaucerDashMark()
        {
            TickFormAccentPulse(ref saucerDashMarkUntil, saucerRimMaterial, saucerRimBaseColor, saucerRimEmissionColor, new Color(0.72f, 0.98f, 1f, 1f));
        }

        private static void TickFormAccentPulse(ref float until, Material material, Color baseColor, Color emission, Color flash)
        {
            if (material == null)
            {
                return;
            }

            if (Time.time >= until)
            {
                ApplyFormAccentPulse(material, baseColor, emission, flash, 0f);
                return;
            }

            var remaining = Mathf.Clamp01((until - Time.time) / 0.26f);
            ApplyFormAccentPulse(material, baseColor, emission, flash, remaining);
        }

        private static void ApplyFormAccentPulse(Material material, Color baseColor, Color emission, Color flash, float strength)
        {
            if (material == null)
            {
                return;
            }

            strength = Mathf.Clamp01(strength);
            var color = Color.Lerp(baseColor, flash, strength * 0.45f);
            var emit = Color.Lerp(emission, flash, strength);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emit * (1f + strength * 1.5f));
            }
        }

        private Transform GetOrCreateFormIdentityKit(string kitId, string legacyName)
        {
            var root = transform.Find(kitId);
            if (root == null)
            {
                root = transform.Find(legacyName);
            }
            if (root == null)
            {
                var rootGo = new GameObject(kitId);
                root = rootGo.transform;
                root.SetParent(transform, false);
            }
            else
            {
                root.gameObject.name = kitId;
            }

            return root;
        }

        private static void DeactivateNamedChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return;
            }

            var child = parent.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private static GameObject EnsureFormIdentityPrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Vector3 localEuler)
        {
            var existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = GameObject.CreatePrimitive(type);
                go.name = name;
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = existing.gameObject;
                go.SetActive(true);
            }

            RemoveCollider(go);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(localEuler);
            go.transform.localScale = localScale;
            return go;
        }

        private static Material ApplyFormIdentityMaterial(GameObject target, string materialName, Color color, Color emission)
        {
            if (target == null)
            {
                return null;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return null;
            }

            var material = FormIdentityMaterials.Create(materialName, color, emission);
            if (material == null)
            {
                return renderer.sharedMaterial;
            }

            renderer.sharedMaterial = material;
            return material;
        }

        private void EnsureCrusherVisual()
        {
            if (crusherVisual != null)
            {
                return;
            }

            var root = GetOrCreateFormIdentityKit("FORM_Crusher_Body_Kit", "_CrusherForm");

            var steel = new Color(0.16f, 0.2f, 0.26f, 1f);
            var dark = new Color(0.1f, 0.12f, 0.16f, 1f);
            var plate = new Color(0.22f, 0.26f, 0.32f, 1f);
            var steelGlow = new Color(0.06f, 0.08f, 0.12f, 1f);

            var coreGo = EnsureFormIdentityPrimitive(root, "CrusherCore", PrimitiveType.Cube, Vector3.zero, new Vector3(1.18f, 1.08f, 1.18f), Vector3.zero);
            ApplyFormIdentityMaterial(coreGo, "M_Runtime_CrusherCore", steel, steelGlow);
            EnsureFormIdentityPrimitive(root, "CrusherRing", PrimitiveType.Cube, Vector3.zero, new Vector3(1.58f, 0.32f, 1.58f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("CrusherRing").gameObject, "M_Runtime_CrusherRing", dark, steelGlow);
            EnsureFormIdentityPrimitive(root, "CrusherBand", PrimitiveType.Cube, new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.18f, 1.4f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("CrusherBand").gameObject, "M_Runtime_CrusherBand", plate, steelGlow);
            EnsureFormIdentityPrimitive(root, "CrusherKeel", PrimitiveType.Cube, new Vector3(0f, -0.52f, 0.04f), new Vector3(0.78f, 0.22f, 1.08f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("CrusherKeel").gameObject, "M_Runtime_CrusherKeel", dark, steelGlow);
            EnsureFormIdentityPrimitive(root, "CrusherPlate", PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.7f), new Vector3(0.92f, 0.72f, 0.24f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("CrusherPlate").gameObject, "M_Runtime_CrusherPlate", plate, steelGlow);

            var seamGo = EnsureFormIdentityPrimitive(root, "CrusherSeam_H", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0.78f), new Vector3(1.02f, 0.1f, 0.12f), Vector3.zero);
            crusherSeamMaterial = ApplyFormIdentityMaterial(seamGo, "M_Runtime_CrusherSeam", crusherSeamBaseColor, crusherSeamEmissionColor);
            EnsureFormIdentityPrimitive(root, "CrusherSeam_V", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0.78f), new Vector3(0.12f, 0.68f, 0.12f), Vector3.zero);
            ApplyFormIdentityMaterial(root.Find("CrusherSeam_V").gameObject, "M_Runtime_CrusherSeamV", crusherSeamBaseColor, crusherSeamEmissionColor);

            EnsureCrusherMagnetRing(root);
            crusherVisual = root;
            crusherVisual.gameObject.SetActive(false);
        }

        private void EnsureCrusherMagnetRing(Transform root = null)
        {
            if (crusherMagnetRing != null)
            {
                return;
            }

            var parent = root != null ? root : crusherVisual;
            if (parent == null)
            {
                return;
            }

            var ringGo = EnsureFormIdentityPrimitive(parent, "CrusherMagnetRing", PrimitiveType.Cylinder, new Vector3(0f, -0.02f, 0f), new Vector3(2.05f, 0.05f, 2.05f), Vector3.zero);
            ApplyFormIdentityMaterial(ringGo, "M_Runtime_CrusherMagnetRing", new Color(0.42f, 0.78f, 1f, 1f), crusherSeamEmissionColor);
            crusherMagnetRing = ringGo.transform;
            crusherMagnetRing.gameObject.SetActive(false);
        }

        private void EnsureSphereVisual()
        {
            if (sphereVisual != null)
            {
                return;
            }

            var root = transform.Find("FORM_Sphere_Body_Kit");
            if (root == null)
            {
                root = transform.Find("_SphereForm");
            }
            if (root == null)
            {
                var rootGo = new GameObject("FORM_Sphere_Body_Kit");
                root = rootGo.transform;
                root.SetParent(transform, false);
            }
            else
            {
                root.gameObject.name = "FORM_Sphere_Body_Kit";
            }

            var bodyGo = EnsureSpherePrimitive(root, "SphereCore", Vector3.zero, new Vector3(1.02f, 1.02f, 1.02f));
            var beltGo = EnsureSpherePrimitive(root, "SphereBelt", Vector3.zero, new Vector3(1.22f, 0.12f, 1.22f), PrimitiveType.Cylinder);
            var gemGo = EnsureSpherePrimitive(root, "SphereBeltGem", new Vector3(0f, 0f, 0.64f), new Vector3(0.18f, 0.18f, 0.18f), PrimitiveType.Cube);

            var bodyColor = new Color(0.18f, 0.74f, 0.48f, 1f);
            var bodyGlow = new Color(0.08f, 0.36f, 0.22f, 1f);
            var gemColor = new Color(0.86f, 1f, 0.52f, 1f);
            ApplySphereIdentityMaterial(bodyGo, "M_Runtime_SphereBody", bodyColor, bodyGlow);
            sphereBeltMaterial = ApplySphereIdentityMaterial(beltGo, "M_Runtime_SphereBelt", sphereBeltBaseColor, sphereBeltEmissionColor);
            ApplySphereIdentityMaterial(gemGo, "M_Runtime_SphereBeltGem", gemColor, gemColor * 0.85f);

            sphereBeltRenderer = beltGo != null ? beltGo.GetComponent<Renderer>() : null;
            sphereVisual = root;
            sphereVisual.gameObject.SetActive(false);
        }

        public void PlaySpherePulseVisualCue()
        {
            spherePulseMarkUntil = Time.time + 0.28f;
            ApplySphereBeltPulse(1f);
        }

        private void TickSpherePulseMark()
        {
            if (sphereBeltRenderer == null || sphereBeltMaterial == null)
            {
                return;
            }

            if (Time.time >= spherePulseMarkUntil)
            {
                ApplySphereBeltPulse(0f);
                return;
            }

            var remaining = Mathf.Clamp01((spherePulseMarkUntil - Time.time) / 0.28f);
            ApplySphereBeltPulse(remaining);
        }

        private void ApplySphereBeltPulse(float strength)
        {
            if (sphereBeltMaterial == null)
            {
                return;
            }

            strength = Mathf.Clamp01(strength);
            var color = Color.Lerp(sphereBeltBaseColor, Color.white, strength * 0.35f);
            var emission = Color.Lerp(sphereBeltEmissionColor, new Color(0.85f, 1f, 0.55f, 1f), strength);
            if (sphereBeltMaterial.HasProperty("_BaseColor")) sphereBeltMaterial.SetColor("_BaseColor", color);
            if (sphereBeltMaterial.HasProperty("_Color")) sphereBeltMaterial.SetColor("_Color", color);
            if (sphereBeltMaterial.HasProperty("_EmissionColor"))
            {
                sphereBeltMaterial.EnableKeyword("_EMISSION");
                sphereBeltMaterial.SetColor("_EmissionColor", emission * (1f + strength * 1.6f));
            }
        }

        private static GameObject EnsureSpherePrimitive(Transform parent, string name, Vector3 localPosition, Vector3 localScale, PrimitiveType type = PrimitiveType.Sphere)
        {
            var existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = GameObject.CreatePrimitive(type);
                go.name = name;
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = existing.gameObject;
            }

            RemoveCollider(go);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;
            return go;
        }

        private static Material ApplySphereIdentityMaterial(GameObject target, string materialName, Color color, Color emission)
        {
            if (target == null)
            {
                return null;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return null;
            }

            var material = FormIdentityMaterials.Create(materialName, color, emission);
            if (material == null)
            {
                return renderer.sharedMaterial;
            }

            renderer.sharedMaterial = material;
            return material;
        }

        private void CreateSpike(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var spikeGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spikeGo.name = name;
            spikeGo.transform.SetParent(parent, false);
            spikeGo.transform.localPosition = localPosition;
            spikeGo.transform.localRotation = localRotation;
            spikeGo.transform.localScale = new Vector3(0.2f, 0.55f, 0.2f);
            RemoveCollider(spikeGo);
        }

        private void ApplyMaterialToForm(Transform formRoot)
        {
            if (cachedRenderer == null || formRoot == null)
            {
                return;
            }

            var renderers = formRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.sharedMaterial = cachedRenderer.sharedMaterial;
                }
            }
        }

        private void ApplyFormStats()
        {
            formSpeedMultiplier = 1f;
            formImpactMultiplier = 1f;
            var bounceScale = 1f;

            switch (currentBaseForm)
            {
                case FormType.Spike:
                    formSpeedMultiplier = 1.06f;
                    formImpactMultiplier = 1.12f;
                    bounceScale = 0.96f;
                    break;
                case FormType.Ram:
                    formSpeedMultiplier = 0.9f;
                    formImpactMultiplier = 1.28f;
                    bounceScale = 0.9f;
                    break;
                case FormType.Saucer:
                    formSpeedMultiplier = 1.28f;
                    formImpactMultiplier = 0.82f;
                    bounceScale = 1.04f;
                    break;
                case FormType.Crusher:
                    formSpeedMultiplier = 0.78f;
                    formImpactMultiplier = 1.55f;
                    bounceScale = 0.82f;
                    break;
            }

            if (!drillMode)
            {
                bounceCoefficient = Mathf.Clamp(baseBounceCoefficient * bounceScale, 0.1f, 1.2f);
            }
        }

        private void UpdateFormVisibility()
        {
            var drillActive = drillMode;

            if (drillVisual != null)
            {
                drillVisual.gameObject.SetActive(drillActive);
            }

            if (cachedRenderer != null)
            {
                cachedRenderer.enabled = !drillActive && currentBaseForm == FormType.Sphere && sphereVisual == null;
            }

            if (spikeVisual != null)
            {
                spikeVisual.gameObject.SetActive(!drillActive && currentBaseForm == FormType.Spike);
            }

            if (ramVisual != null)
            {
                ramVisual.gameObject.SetActive(!drillActive && currentBaseForm == FormType.Ram);
            }

            if (saucerVisual != null)
            {
                saucerVisual.gameObject.SetActive(!drillActive && currentBaseForm == FormType.Saucer);
            }

            if (saucerRayEmitter != null)
            {
                saucerRayEmitter.gameObject.SetActive(!drillActive && currentBaseForm == FormType.Saucer);
            }

            if (crusherVisual != null)
            {
                crusherVisual.gameObject.SetActive(!drillActive && currentBaseForm == FormType.Crusher);
            }

            if (crusherMagnetRing != null && (drillActive || currentBaseForm != FormType.Crusher))
            {
                crusherMagnetRing.gameObject.SetActive(false);
            }

            if (sphereVisual != null)
            {
                sphereVisual.gameObject.SetActive(!drillActive && currentBaseForm == FormType.Sphere);
            }
        }
        private void TryTriggerLandingShockwave(Vector3 hitPoint, Vector3 contactNormal, Vector3 preCollisionVelocity)
        {
            if (!enableLandingShockwaveSignal)
            {
                return;
            }

            if (Time.time - lastLandingShockwaveAt < Mathf.Max(0.05f, landingShockwaveCooldown))
            {
                return;
            }

            if (contactNormal.y < landingShockwaveGroundNormalMinY)
            {
                return;
            }

            var downwardSpeed = Mathf.Max(0f, -preCollisionVelocity.y);
            if (downwardSpeed < Mathf.Max(0.5f, landingShockwaveMinDownwardSpeed))
            {
                return;
            }

            var planarSpeed = new Vector3(preCollisionVelocity.x, 0f, preCollisionVelocity.z).magnitude;
            if (planarSpeed < Mathf.Max(0.5f, landingShockwaveMinPlanarSpeed))
            {
                return;
            }

            var vertical01 = Mathf.InverseLerp(landingShockwaveMinDownwardSpeed, landingShockwaveMinDownwardSpeed * 2.4f, downwardSpeed);
            var planar01 = Mathf.InverseLerp(landingShockwaveMinPlanarSpeed, landingShockwaveMinPlanarSpeed * 2.1f, planarSpeed);
            var intensity = Mathf.Clamp01(vertical01 * 0.62f + planar01 * 0.38f);
            if (drillMode)
            {
                intensity = Mathf.Clamp01(intensity + 0.1f);
            }

            var payload = new LandingShockwaveData(hitPoint, intensity, planarSpeed, downwardSpeed, drillMode, ImpactMultiplier);
            lastLandingShockwaveAt = Time.time;
            LandingShockwaveTriggered?.Invoke(payload);
        }

        private void RebuildCounterSurgeRenderers()
        {
            counterSurgeRenderers.Clear();
            var renderers = GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer is TrailRenderer || counterSurgeRenderers.Contains(renderer))
                {
                    continue;
                }

                counterSurgeRenderers.Add(renderer);
            }
        }

        private void UpdateCounterSurgeVisual()
        {
            if (!enableCounterSurgeVisual)
            {
                ResetCounterSurgeVisual();
                return;
            }

            if (counterUntilTime <= Time.time)
            {
                ResetCounterSurgeVisual();
                return;
            }

            if (counterSurgeRenderers.Count == 0)
            {
                RebuildCounterSurgeRenderers();
            }

            var duration = Mathf.Max(0.1f, counterSurgeVisualDurationReference);
            var remaining01 = Mathf.Clamp01(CounterSurgeRemaining / duration);
            var pulse = 0.5f + Mathf.Sin(Time.time * Mathf.Max(1f, counterSurgePulseSpeed)) * 0.5f;
            var tint = Color.Lerp(counterSurgeTintA, counterSurgeTintB, pulse);
            var tintStrength = Mathf.Clamp01(counterSurgeTintStrength * (0.45f + remaining01 * 0.85f) * (0.6f + pulse * 0.45f));
            var emissionScale = Mathf.Max(0f, counterSurgeEmissionBoost) * (0.35f + remaining01 * 0.9f) * (0.45f + pulse * 0.55f);
            var emissionColor = tint * emissionScale;
            UpdateCounterSurgeTrail(tint, remaining01, pulse);

            var appliedAny = false;
            for (var i = 0; i < counterSurgeRenderers.Count; i++)
            {
                var renderer = counterSurgeRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                var material = renderer.sharedMaterial;
                if (material == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(counterSurgePropertyBlock);
                var hasOverride = false;
                if (material.HasProperty("_BaseColor"))
                {
                    var baseColor = material.GetColor("_BaseColor");
                    counterSurgePropertyBlock.SetColor("_BaseColor", Color.Lerp(baseColor, tint, tintStrength));
                    hasOverride = true;
                }

                if (material.HasProperty("_Color"))
                {
                    var colorBase = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.GetColor("_Color");
                    counterSurgePropertyBlock.SetColor("_Color", Color.Lerp(colorBase, tint, tintStrength));
                    hasOverride = true;
                }

                if (material.HasProperty("_EmissionColor"))
                {
                    counterSurgePropertyBlock.SetColor("_EmissionColor", emissionColor);
                    hasOverride = true;
                }

                if (hasOverride)
                {
                    renderer.SetPropertyBlock(counterSurgePropertyBlock);
                    appliedAny = true;
                }
                else
                {
                    renderer.SetPropertyBlock(null);
                }
            }

            counterSurgeVisualApplied = appliedAny;
        }

        private void ResetCounterSurgeVisual()
        {
            if (!counterSurgeVisualApplied && counterSurgeRenderers.Count == 0)
            {
                return;
            }

            for (var i = 0; i < counterSurgeRenderers.Count; i++)
            {
                var renderer = counterSurgeRenderers[i];
                if (renderer != null)
                {
                    renderer.SetPropertyBlock(null);
                }
            }

            counterSurgeVisualApplied = false;
            SetCounterSurgeTrailActive(false);
        }

        private void EnsureCounterSurgeTrail()
        {
            if (counterSurgeTrail != null)
            {
                return;
            }

            var trailRoot = transform.Find("_CounterSurgeTrail");
            if (trailRoot == null)
            {
                var trailGo = new GameObject("_CounterSurgeTrail");
                trailRoot = trailGo.transform;
                trailRoot.SetParent(transform, false);
            }

            trailRoot.localPosition = Vector3.zero;
            trailRoot.localRotation = Quaternion.identity;
            trailRoot.localScale = Vector3.one;

            counterSurgeTrail = trailRoot.GetComponent<TrailRenderer>();
            if (counterSurgeTrail == null)
            {
                counterSurgeTrail = trailRoot.gameObject.AddComponent<TrailRenderer>();
            }

            counterSurgeTrail.alignment = LineAlignment.View;
            counterSurgeTrail.textureMode = LineTextureMode.Stretch;
            counterSurgeTrail.numCornerVertices = 2;
            counterSurgeTrail.numCapVertices = 2;
            counterSurgeTrail.minVertexDistance = 0.05f;
            counterSurgeTrail.autodestruct = false;
            counterSurgeTrail.time = Mathf.Max(0.05f, counterSurgeTrailTime);
            counterSurgeTrail.startWidth = Mathf.Max(0.01f, counterSurgeTrailMinWidth);
            counterSurgeTrail.endWidth = Mathf.Max(0.005f, counterSurgeTrailMinWidth * 0.2f);
            counterSurgeTrail.emitting = false;
            counterSurgeTrail.material = GetCounterSurgeTrailMaterial();
            counterSurgeTrail.gameObject.SetActive(enableCounterSurgeTrail);
        }

        private void UpdateCounterSurgeTrail(Color tint, float remaining01, float pulse)
        {
            if (!enableCounterSurgeTrail)
            {
                SetCounterSurgeTrailActive(false);
                return;
            }

            EnsureCounterSurgeTrail();
            if (counterSurgeTrail == null)
            {
                return;
            }

            ResolveCounterSurgeTrailStyle(tint, pulse, out var trailTint, out var widthScale, out var timeScale, out var alphaScale);

            counterSurgeTrail.gameObject.SetActive(true);
            counterSurgeTrail.time = Mathf.Max(0.05f, counterSurgeTrailTime * timeScale);
            var width01 = Mathf.Clamp01(remaining01 * (0.72f + pulse * 0.28f));
            var startWidth = Mathf.Lerp(Mathf.Max(0.01f, counterSurgeTrailMinWidth), Mathf.Max(0.02f, counterSurgeTrailMaxWidth), width01) * widthScale;
            counterSurgeTrail.startWidth = Mathf.Max(0.01f, startWidth);
            counterSurgeTrail.endWidth = Mathf.Max(0.005f, startWidth * 0.14f);

            var alpha = Mathf.Clamp01(counterSurgeTrailAlpha * (0.55f + remaining01 * 0.45f) * alphaScale);
            var startColor = trailTint;
            startColor.a = alpha;
            var endColor = trailTint;
            endColor.a = 0f;
            counterSurgeTrail.startColor = startColor;
            counterSurgeTrail.endColor = endColor;
            SetCounterSurgeTrailActive(true);
        }

        private void ResolveCounterSurgeTrailStyle(Color baseTint, float pulse, out Color trailTint, out float widthScale, out float timeScale, out float alphaScale)
        {
            trailTint = baseTint;
            widthScale = 1f;
            timeScale = 1f;
            alphaScale = 1f;

            switch (currentBaseForm)
            {
                case FormType.Spike:
                    trailTint = Color.Lerp(baseTint, new Color(0.92f, 0.98f, 1f, 1f), 0.46f);
                    widthScale = 0.84f;
                    timeScale = 0.88f;
                    alphaScale = 0.93f;
                    break;
                case FormType.Ram:
                    trailTint = Color.Lerp(baseTint, new Color(1f, 0.76f, 0.36f, 1f), 0.52f);
                    widthScale = 1.18f;
                    timeScale = 0.92f;
                    alphaScale = 1.05f;
                    break;
                case FormType.Saucer:
                    trailTint = Color.Lerp(baseTint, new Color(0.58f, 0.95f, 1f, 1f), 0.54f);
                    widthScale = 0.78f;
                    timeScale = 1.34f;
                    alphaScale = 0.82f;
                    break;
                case FormType.Crusher:
                    trailTint = Color.Lerp(baseTint, new Color(0.28f, 0.8f, 1f, 1f), 0.4f);
                    widthScale = 1.34f;
                    timeScale = 1.06f;
                    alphaScale = 1.18f;
                    break;
            }

            if (drillMode)
            {
                trailTint = Color.Lerp(trailTint, new Color(1f, 0.88f, 0.44f, 1f), 0.35f);
                widthScale *= 1.14f;
                timeScale *= 0.9f;
                alphaScale *= 1.08f;
            }

            widthScale *= 0.88f + pulse * 0.22f;
            alphaScale *= 0.84f + pulse * 0.16f;
        }

        private void SetCounterSurgeTrailActive(bool active)
        {
            if (counterSurgeTrail == null)
            {
                return;
            }

            if (counterSurgeTrailActive == active)
            {
                return;
            }

            counterSurgeTrailActive = active;
            counterSurgeTrail.emitting = active;
            if (counterSurgeTrail.gameObject.activeSelf != active)
            {
                counterSurgeTrail.gameObject.SetActive(active);
            }

            if (!active)
            {
                counterSurgeTrail.Clear();
            }
        }

        private Material GetCounterSurgeTrailMaterial()
        {
            if (counterSurgeTrailMaterial != null)
            {
                return counterSurgeTrailMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Unlit/Color");

            if (shader == null)
            {
                return null;
            }

            counterSurgeTrailMaterial = new Material(shader)
            {
                name = "M_Runtime_CounterSurgeTrail"
            };

            if (counterSurgeTrailMaterial.HasProperty("_Surface")) counterSurgeTrailMaterial.SetFloat("_Surface", 1f);
            if (counterSurgeTrailMaterial.HasProperty("_Blend")) counterSurgeTrailMaterial.SetFloat("_Blend", 0f);
            if (counterSurgeTrailMaterial.HasProperty("_BaseColor")) counterSurgeTrailMaterial.SetColor("_BaseColor", Color.white);
            if (counterSurgeTrailMaterial.HasProperty("_Color")) counterSurgeTrailMaterial.SetColor("_Color", Color.white);
            return counterSurgeTrailMaterial;
        }

        private static void RemoveCollider(GameObject target)
        {
            var col = target.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }
        }

        private void SetDrillVisualActive(bool active)
        {
            drillMode = active;
            UpdateFormVisibility();
            UpdateCounterSurgeVisual();
        }

        private Vector2 GetMovementInput()
        {
            if (virtualJoystick != null)
            {
                var stick = virtualJoystick.InputVector;
                if (stick.sqrMagnitude > 0f)
                {
                    return Vector2.ClampMagnitude(stick, 1f);
                }
            }

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                var x = 0f;
                var y = 0f;

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;

                return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            }
#endif

            return Vector2.zero;
        }

        private Vector2 ProcessMovementInput(Vector2 rawInput)
        {
            var inputAttack = Mathf.Max(0.1f, inputSmoothingAttack);
            var inputRelease = Mathf.Max(0.1f, inputSmoothingRelease);
            var rawMagnitude = Mathf.Clamp01(rawInput.magnitude);
            var currentMagnitude = Mathf.Clamp01(smoothedMovementInput.magnitude);
            var sharpness = rawMagnitude >= currentMagnitude ? inputAttack : inputRelease;
            var blend = 1f - Mathf.Exp(-sharpness * Time.fixedDeltaTime);
            smoothedMovementInput = Vector2.Lerp(smoothedMovementInput, rawInput, blend);

            var magnitude = Mathf.Clamp01(smoothedMovementInput.magnitude);
            if (magnitude <= 0.0001f)
            {
                return Vector2.zero;
            }

            var direction = smoothedMovementInput / magnitude;
            var responseExponent = Mathf.Clamp(inputResponseExponent, 0.6f, 1.6f);
            var curvedMagnitude = Mathf.Pow(magnitude, responseExponent);
            var sensitivityBoost = Mathf.Clamp01(inputSensitivityBoost);
            curvedMagnitude = Mathf.Clamp01(curvedMagnitude + (magnitude * sensitivityBoost * (1f - curvedMagnitude)));
            return direction * curvedMagnitude;
        }

        private void ClampPlanarSpeed()
        {
            var velocity = body.linearVelocity;
            var planar = new Vector3(velocity.x, 0f, velocity.z);
            var effectiveSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier * temporarySpeedMultiplier * formSpeedMultiplier * externalSlowMultiplier * counterSpeedMultiplier);
            var maxSpeed = baseMaxPlanarSpeed * effectiveSpeedMultiplier;

            if (planar.magnitude > maxSpeed)
            {
                planar = planar.normalized * maxSpeed;
                body.linearVelocity = new Vector3(planar.x, velocity.y, planar.z);
            }
        }

        private void ApplyPlanarDrag(bool hasInput)
        {
            var velocity = body.linearVelocity;
            var planar = new Vector3(velocity.x, 0f, velocity.z);

            var effectiveDrag = hasInput ? basePlanarDrag * 0.35f : basePlanarDrag;
            var lerp = Mathf.Clamp01(effectiveDrag * Time.fixedDeltaTime);
            planar = Vector3.Lerp(planar, Vector3.zero, lerp);

            body.linearVelocity = new Vector3(planar.x, velocity.y, planar.z);
        }
    }
}




















