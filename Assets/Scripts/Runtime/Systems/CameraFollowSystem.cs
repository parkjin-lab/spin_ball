using UnityEngine;

namespace AlienCrusher.Systems
{
    [DisallowMultipleComponent]
    public class CameraFollowSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private string playerBallName = "PlayerBall";
        [SerializeField] private string gameplayCameraName = "GameplayCamera";

        [Header("Comfort Follow")]
        [SerializeField] private bool comfortMode = true;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 27f, -18f);
        [SerializeField] private float positionSmoothTime = 0.22f;
        [SerializeField] private float maxFollowSpeed = 55f;
        [SerializeField] private float deadZoneRadius = 1.6f;
        [SerializeField] private float velocityLookAhead = 1.1f;
        [SerializeField] private float maxLookAhead = 2.2f;

        [Header("Rotation")]
        [SerializeField] private bool rotateWithMovement = false;
        [SerializeField] private Vector2 fixedAngles = new Vector2(58f, 0f);
        [SerializeField] private float rotationSmooth = 10f;

        [Header("Bounds")]
        [SerializeField] private bool clampToMap = true;
        [SerializeField] private Vector2 mapHalfExtent = new Vector2(23f, 23f);

        [Header("Impulse")]
        [SerializeField] private float impulseDamping = 13f;
        [SerializeField] private float maxImpulseOffset = 1.15f;

        [Header("Overdrive Camera")]
        [SerializeField] private Vector3 overdriveOffsetDelta = new Vector3(0f, 2.2f, -3.2f);
        [SerializeField] private float overdrivePitchDelta = -2.5f;
        [SerializeField] private float overdriveTransitionSpeed = 8f;
        [SerializeField] private float overdriveSmoothMultiplier = 0.78f;

        [Header("Finish Shot")]
        [SerializeField] private float finishShotDuration = 1.1f;
        [SerializeField] private Vector3 finishShotOffset = new Vector3(0f, 12f, -9f);
        [SerializeField] private float finishShotPositionLerp = 6.5f;
        [SerializeField] private float finishShotRotationLerp = 7.5f;

        [Header("Stage Start Framing")]
        [SerializeField] private Vector3 stageStartFocusOffset = new Vector3(0f, 0f, 4.6f);
        [SerializeField] private float stageStartFocusDuration = 1.4f;
        [SerializeField] private float stageStartReleaseSpeed = 4.8f;
        [SerializeField] [Range(1f, 3f)] private float stageStartFocusEaseExponent = 1.7f;

        private Transform cameraTransform;
        private Transform playerTarget;
        private Rigidbody playerBody;

        private Vector3 focusPoint;
        private Vector3 followVelocity;
        private bool focusInitialized;

        private Vector3 impulseOffset;
        private bool overdriveCameraActive;
        private bool baselineCached;
        private Vector3 baseFollowOffset;
        private Vector2 baseFixedAngles;
        private float basePositionSmoothTime;
        private Vector3 targetFollowOffset;
        private Vector2 targetFixedAngles;
        private float targetPositionSmoothTime;
        private bool finishShotActive;
        private float finishShotRemaining;
        private Vector3 finishShotFocusPoint;
        private Vector3 finishShotAnchor;
        private float stageStartFocusRemaining;
        private float stageStartFocusDurationRuntime;

        private void Awake()
        {
            ResolveReferences(forceCreateCamera: true);
            ApplyComfortPreset();
            CacheCameraBaseline();
            SnapToTarget();
        }

        private void LateUpdate()
        {
            ResolveReferences(forceCreateCamera: true);
            if (cameraTransform == null || playerTarget == null)
            {
                return;
            }

            if (UpdateFinishShot())
            {
                return;
            }

            var desiredFocus = GetDesiredFocusPoint();
            UpdateFocusPoint(desiredFocus);
            UpdateImpulseOffset();

            var blend = 1f - Mathf.Exp(-Mathf.Max(0.1f, overdriveTransitionSpeed) * Time.deltaTime);
            followOffset = Vector3.Lerp(followOffset, targetFollowOffset, blend);
            fixedAngles = Vector2.Lerp(fixedAngles, targetFixedAngles, blend);
            positionSmoothTime = Mathf.Lerp(positionSmoothTime, targetPositionSmoothTime, blend);

            var desiredPosition = focusPoint + followOffset + impulseOffset;
            desiredPosition = ClampToBounds(desiredPosition);

            cameraTransform.position = Vector3.SmoothDamp(
                cameraTransform.position,
                desiredPosition,
                ref followVelocity,
                Mathf.Max(0.02f, positionSmoothTime),
                Mathf.Max(1f, maxFollowSpeed));

            var targetRotation = GetTargetRotation();
            var rotT = 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationSmooth) * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, rotT);
        }

        public void Configure(Transform cameraOverride)
        {
            if (cameraOverride != null)
            {
                cameraTransform = cameraOverride;
            }

            ResolveReferences(forceCreateCamera: true);
        }

        public void ApplyComfortPreset()
        {
            comfortMode = true;
            rotateWithMovement = false;
            positionSmoothTime = Mathf.Clamp(positionSmoothTime, 0.18f, 0.35f);
            deadZoneRadius = Mathf.Clamp(deadZoneRadius, 1.1f, 2.2f);
            velocityLookAhead = Mathf.Clamp(velocityLookAhead, 0.6f, 1.8f);
            maxLookAhead = Mathf.Clamp(maxLookAhead, 1.2f, 2.8f);
            followOffset.y = Mathf.Clamp(followOffset.y, 24f, 32f);
            followOffset.z = Mathf.Clamp(followOffset.z, -23f, -14f);
            fixedAngles.x = Mathf.Clamp(fixedAngles.x, 54f, 63f);
            fixedAngles.y = 0f;
            impulseDamping = Mathf.Clamp(impulseDamping, 9f, 18f);
            maxImpulseOffset = Mathf.Clamp(maxImpulseOffset, 0.6f, 1.6f);
            if (!overdriveCameraActive)
            {
                baseFollowOffset = followOffset;
                baseFixedAngles = fixedAngles;
                basePositionSmoothTime = positionSmoothTime;
                baselineCached = true;
                targetFollowOffset = baseFollowOffset;
                targetFixedAngles = baseFixedAngles;
                targetPositionSmoothTime = basePositionSmoothTime;
            }
        }

        public void PlayFinishShot(Vector3 worldPoint, float durationScale = 1f)
        {
            ResolveReferences(forceCreateCamera: true);
            if (cameraTransform == null)
            {
                return;
            }

            finishShotActive = true;
            finishShotRemaining = Mathf.Max(0.35f, finishShotDuration * Mathf.Max(0.5f, durationScale));
            finishShotFocusPoint = worldPoint;
            finishShotAnchor = worldPoint;
            followVelocity = Vector3.zero;
        }

        public void SnapToTarget()
        {
            ResolveReferences(forceCreateCamera: true);
            if (cameraTransform == null || playerTarget == null)
            {
                return;
            }

            focusPoint = GetDesiredFocusPoint();
            focusInitialized = true;

            var desiredPosition = ClampToBounds(focusPoint + followOffset);
            cameraTransform.position = desiredPosition;
            cameraTransform.rotation = GetTargetRotation();
            followVelocity = Vector3.zero;
            impulseOffset = Vector3.zero;
        }

        public void TriggerStageStartFocus()
        {
            stageStartFocusDurationRuntime = Mathf.Max(0.01f, stageStartFocusDuration);
            stageStartFocusRemaining = stageStartFocusDurationRuntime;
        }

        public void AddImpulse(float magnitude)
        {
            if (magnitude <= 0f)
            {
                return;
            }

            var random = Random.insideUnitSphere;
            random.y *= 0.35f;
            if (random.sqrMagnitude < 0.0001f)
            {
                random = Vector3.right;
            }

            impulseOffset += random.normalized * magnitude;
            impulseOffset = Vector3.ClampMagnitude(impulseOffset, maxImpulseOffset);
        }

        public void SetOverdriveCameraState(bool active)
        {
            CacheCameraBaseline();
            overdriveCameraActive = active;

            if (active)
            {
                targetFollowOffset = baseFollowOffset + overdriveOffsetDelta;
                targetFixedAngles = new Vector2(baseFixedAngles.x + overdrivePitchDelta, baseFixedAngles.y);
                targetPositionSmoothTime = Mathf.Clamp(basePositionSmoothTime * overdriveSmoothMultiplier, 0.1f, 0.35f);
            }
            else
            {
                targetFollowOffset = baseFollowOffset;
                targetFixedAngles = baseFixedAngles;
                targetPositionSmoothTime = basePositionSmoothTime;
            }
        }

        private void CacheCameraBaseline()
        {
            if (!baselineCached)
            {
                baseFollowOffset = followOffset;
                baseFixedAngles = fixedAngles;
                basePositionSmoothTime = positionSmoothTime;
                baselineCached = true;
            }

            if (!overdriveCameraActive)
            {
                targetFollowOffset = baseFollowOffset;
                targetFixedAngles = baseFixedAngles;
                targetPositionSmoothTime = basePositionSmoothTime;
            }
        }

        private void UpdateImpulseOffset()
        {
            if (impulseOffset.sqrMagnitude <= 0.000001f)
            {
                impulseOffset = Vector3.zero;
                return;
            }

            var t = 1f - Mathf.Exp(-Mathf.Max(0.1f, impulseDamping) * Time.deltaTime);
            impulseOffset = Vector3.Lerp(impulseOffset, Vector3.zero, t);
        }

        private void ResolveReferences(bool forceCreateCamera)
        {
            if (playerTarget == null)
            {
                var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var item in all)
                {
                    if (item.name == playerBallName)
                    {
                        playerTarget = item;
                        break;
                    }
                }

                if (playerTarget != null)
                {
                    playerBody = playerTarget.GetComponent<Rigidbody>();
                }
            }

            if (cameraTransform == null)
            {
                var main = Camera.main;
                if (main != null)
                {
                    cameraTransform = main.transform;
                }
            }

            if (cameraTransform == null)
            {
                var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var item in all)
                {
                    if (item.name == gameplayCameraName && item.GetComponent<Camera>() != null)
                    {
                        cameraTransform = item;
                        break;
                    }
                }
            }

            if (cameraTransform == null && forceCreateCamera)
            {
                var cameraGo = new GameObject(gameplayCameraName, typeof(Camera));
                cameraGo.tag = "MainCamera";
                cameraTransform = cameraGo.transform;

                if (Object.FindFirstObjectByType<AudioListener>() == null)
                {
                    cameraGo.AddComponent<AudioListener>();
                }
            }

            if (cameraTransform != null)
            {
                var cam = cameraTransform.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.fieldOfView = 50f;
                    cam.nearClipPlane = 0.1f;
                    cam.farClipPlane = 200f;
                }
            }
        }

        private Vector3 GetDesiredFocusPoint()
        {
            var focus = playerTarget != null ? playerTarget.position : Vector3.zero;
            var planarVelocity = playerBody != null
                ? new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z)
                : Vector3.zero;

            if (stageStartFocusRemaining > 0f)
            {
                float num = Mathf.Clamp01(stageStartFocusRemaining / Mathf.Max(0.01f, stageStartFocusDurationRuntime));
                float num2 = Mathf.Pow(num, Mathf.Max(1f, stageStartFocusEaseExponent));
                focus += stageStartFocusOffset * num2;
                float num3 = 1f;
                if (planarVelocity.sqrMagnitude > 0.01f)
                {
                    float num4 = planarVelocity.magnitude;
                    num3 += Mathf.Clamp01(num4 / Mathf.Max(0.1f, stageStartReleaseSpeed));
                }
                stageStartFocusRemaining = Mathf.Max(0f, stageStartFocusRemaining - Time.deltaTime * num3);
            }

            if (planarVelocity.sqrMagnitude > 0.01f)
            {
                var lookAhead = planarVelocity.normalized * Mathf.Min(planarVelocity.magnitude * velocityLookAhead * 0.2f, maxLookAhead);
                focus += new Vector3(lookAhead.x, 0f, lookAhead.z);
            }

            return focus;
        }

        private void UpdateFocusPoint(Vector3 desiredFocus)
        {
            if (!focusInitialized)
            {
                focusPoint = desiredFocus;
                focusInitialized = true;
                return;
            }

            if (!comfortMode)
            {
                var t = 1f - Mathf.Exp(-8f * Time.deltaTime);
                focusPoint = Vector3.Lerp(focusPoint, desiredFocus, t);
                return;
            }

            var planarDelta = new Vector2(desiredFocus.x - focusPoint.x, desiredFocus.z - focusPoint.z);
            var planarDistance = planarDelta.magnitude;
            if (planarDistance > deadZoneRadius)
            {
                var move = planarDelta.normalized * (planarDistance - deadZoneRadius);
                focusPoint += new Vector3(move.x, 0f, move.y);
            }

            focusPoint.y = desiredFocus.y;
        }

        private Quaternion GetTargetRotation()
        {
            if (!rotateWithMovement)
            {
                return Quaternion.Euler(fixedAngles.x, fixedAngles.y, 0f);
            }

            var lookPoint = focusPoint + Vector3.up * 1.2f;
            var direction = lookPoint - cameraTransform.position;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Vector3 ClampToBounds(Vector3 desired)
        {
            if (!clampToMap)
            {
                return desired;
            }

            desired.x = Mathf.Clamp(desired.x, -mapHalfExtent.x, mapHalfExtent.x);
            desired.z = Mathf.Clamp(desired.z, -mapHalfExtent.y - 12f, mapHalfExtent.y + 8f);
            return desired;
        }

        private bool UpdateFinishShot()
        {
            if (!finishShotActive || cameraTransform == null)
            {
                return false;
            }

            finishShotRemaining -= Time.deltaTime;
            if (finishShotRemaining <= 0f)
            {
                finishShotActive = false;
                return false;
            }

            Vector3 targetPoint = finishShotAnchor;
            if (playerTarget != null)
            {
                targetPoint = Vector3.Lerp(playerTarget.position, finishShotAnchor, 0.82f);
            }

            finishShotFocusPoint = Vector3.Lerp(finishShotFocusPoint, targetPoint, 1f - Mathf.Exp(-4.5f * Time.deltaTime));

            Vector3 desiredPosition = ClampToBounds(finishShotFocusPoint + finishShotOffset + impulseOffset * 0.35f);
            float posT = 1f - Mathf.Exp(-Mathf.Max(0.1f, finishShotPositionLerp) * Time.deltaTime);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, posT);

            Vector3 lookPoint = finishShotFocusPoint + Vector3.up * 1.35f;
            Vector3 direction = lookPoint - cameraTransform.position;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            float rotT = 1f - Mathf.Exp(-Mathf.Max(0.1f, finishShotRotationLerp) * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, rotT);
            UpdateImpulseOffset();
            return true;
        }
    }
}






