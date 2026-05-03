using System;
using AlienCrusher.Systems;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class BossPhaseTwoDroneDummy : MonoBehaviour
    {
        [Header("Durability")]
        [SerializeField] private float maxDurability = 120f;
        [SerializeField] private float minimumImpactSpeed = 3.4f;
        [SerializeField] private float damagePerSpeed = 18f;
        [SerializeField] private float rigidbodyVelocityDamageScale = 1.2f;
        [SerializeField] private float drillDamageMultiplier = 1.35f;
        [SerializeField] private float hitCooldown = 0.08f;
        [SerializeField] private float wobbleDuration = 0.12f;
        [SerializeField] private float wobbleDistance = 0.08f;
        [SerializeField] private float wobbleTilt = 10f;

        private float currentDurability;
        private float lastHitAt = -10f;
        private bool destroyed;
        private Action<BossPhaseTwoDroneDummy> destroyedCallback;
        private Collider cachedCollider;
        private Renderer[] cachedRenderers;
        private DamageNumberSystem damageNumberSystem;
        private FeedbackSystem feedbackSystem;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float wobbleRemaining;
        private bool respawnPreviewActive;
        private Color baseTint = Color.white;

        public bool IsAlive => !destroyed;
        public bool IsDestroyed => destroyed;
        public float DurabilityRatio => maxDurability > 0.001f ? Mathf.Clamp01(currentDurability / maxDurability) : 0f;

        private void Awake()
        {
            cachedCollider = GetComponent<Collider>();
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            damageNumberSystem = Object.FindFirstObjectByType<DamageNumberSystem>();
            feedbackSystem = Object.FindFirstObjectByType<FeedbackSystem>();
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
            Restore();
        }

        private void LateUpdate()
        {
            if (destroyed)
            {
                return;
            }

            if (wobbleRemaining > 0f)
            {
                wobbleRemaining = Mathf.Max(0f, wobbleRemaining - Time.deltaTime);
                float t = 1f - wobbleRemaining / Mathf.Max(0.01f, wobbleDuration);
                float pulse = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
                transform.localPosition = baseLocalPosition + Vector3.up * (wobbleDistance * pulse);
                transform.localRotation = baseLocalRotation * Quaternion.Euler(pulse * wobbleTilt, pulse * wobbleTilt * 0.4f, -pulse * wobbleTilt);
                if (wobbleRemaining <= 0f)
                {
                    transform.localPosition = baseLocalPosition;
                    transform.localRotation = baseLocalRotation;
                }
            }
            else
            {
                baseLocalPosition = transform.localPosition;
                baseLocalRotation = transform.localRotation;
            }
        }

        public void Configure(Color tint, Action<BossPhaseTwoDroneDummy> onDestroyed)
        {
            destroyedCallback = onDestroyed;
            baseTint = tint;
            if (cachedRenderers == null || cachedRenderers.Length == 0)
            {
                cachedRenderers = GetComponentsInChildren<Renderer>(true);
            }

            ApplyTint(baseTint);
        }

        public void SetRespawnPreview(float normalized)
        {
            if (!destroyed)
            {
                return;
            }

            normalized = Mathf.Clamp01(normalized);
            respawnPreviewActive = normalized > 0.001f;
            if (!respawnPreviewActive)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            if ((Object)(object)cachedCollider != (Object)null)
            {
                cachedCollider.enabled = false;
            }

            float blink = Mathf.PingPong(Time.time * Mathf.Lerp(4f, 14f, normalized), 1f);
            float visibleGate = Mathf.Lerp(0.82f, 0.24f, normalized);
            bool visible = blink <= visibleGate;
            SetRenderersEnabled(visible);

            Color previewTint = Color.Lerp(baseTint, Color.white, Mathf.Lerp(0.2f, 0.75f, normalized));
            ApplyTint(previewTint);
            transform.localPosition = baseLocalPosition + Vector3.up * Mathf.Sin(Time.time * Mathf.Lerp(8f, 18f, normalized)) * Mathf.Lerp(0.01f, 0.06f, normalized);
            transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, Time.time * Mathf.Lerp(45f, 120f, normalized), 0f);
        }

        private void ApplyTint(Color tint)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer renderer = cachedRenderers[i];
                if ((Object)(object)renderer == (Object)null)
                {
                    continue;
                }

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                renderer.SetPropertyBlock(block);
            }
        }

        public void Restore()
        {
            destroyed = false;
            respawnPreviewActive = false;
            currentDurability = Mathf.Max(24f, maxDurability);
            if ((Object)(object)cachedCollider != (Object)null)
            {
                cachedCollider.enabled = true;
            }

            wobbleRemaining = 0f;
            SetRenderersEnabled(true);
            ApplyTint(baseTint);
            transform.localPosition = baseLocalPosition;
            transform.localRotation = baseLocalRotation;
            gameObject.SetActive(true);
        }

        public void SetDroneVisible(bool visible)
        {
            if (respawnPreviewActive && destroyed)
            {
                gameObject.SetActive(true);
                return;
            }

            gameObject.SetActive(visible && !destroyed);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (destroyed || Time.time < lastHitAt + Mathf.Max(0.02f, hitCooldown))
            {
                return;
            }

            PlayerBallDummyController player = collision.gameObject.GetComponent<PlayerBallDummyController>() ??
                                               collision.gameObject.GetComponentInParent<PlayerBallDummyController>();
            if (player == null)
            {
                return;
            }

            float relativeSpeed = collision.relativeVelocity.magnitude;
            float impactThreshold = player.DrillMode ? minimumImpactSpeed * 0.8f : minimumImpactSpeed;
            if (relativeSpeed < impactThreshold)
            {
                return;
            }

            Rigidbody body = player.GetComponent<Rigidbody>();
            float velocityDamage = (body != null ? body.linearVelocity.magnitude : relativeSpeed) * rigidbodyVelocityDamageScale;
            float damage = (relativeSpeed * damagePerSpeed + velocityDamage) * Mathf.Max(0.6f, player.ImpactMultiplier);
            if (player.DrillMode)
            {
                damage *= Mathf.Max(1f, drillDamageMultiplier);
            }

            damage = Mathf.Clamp(damage, 12f, 420f);
            lastHitAt = Time.time;
            currentDurability -= damage;

            Vector3 hitPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            float impact01 = Mathf.InverseLerp(impactThreshold, 16f, relativeSpeed);
            damageNumberSystem ??= Object.FindFirstObjectByType<DamageNumberSystem>();
            feedbackSystem ??= Object.FindFirstObjectByType<FeedbackSystem>();
            damageNumberSystem?.ShowDamage(hitPoint, damage, impact01 >= 0.65f, currentDurability <= 0f);
            feedbackSystem?.PlayHitFeedback(hitPoint, Mathf.Clamp01(impact01));
            wobbleRemaining = Mathf.Max(0.04f, wobbleDuration);

            if (currentDurability > 0f)
            {
                return;
            }

            destroyed = true;
            respawnPreviewActive = false;
            if ((Object)(object)cachedCollider != (Object)null)
            {
                cachedCollider.enabled = false;
            }

            SetRenderersEnabled(false);
            damageNumberSystem?.ShowTag(transform.position + Vector3.up * 0.9f, "DRONE DOWN", true);
            destroyedCallback?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void SetRenderersEnabled(bool enabled)
        {
            if (cachedRenderers == null)
            {
                return;
            }

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer renderer = cachedRenderers[i];
                if ((Object)(object)renderer != (Object)null)
                {
                    renderer.enabled = enabled;
                }
            }
        }
    }
}
