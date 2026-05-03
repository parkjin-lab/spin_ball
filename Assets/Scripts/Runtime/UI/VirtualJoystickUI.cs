using UnityEngine;
using UnityEngine.EventSystems;

namespace AlienCrusher.UI
{
    public class VirtualJoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform joystickBase;
        [SerializeField] private RectTransform joystickKnob;

        [Header("Settings")]
        [SerializeField] private float radius = 110f;
        [SerializeField] private float deadZone = 0.06f;
        [SerializeField] [Range(0.6f, 1.6f)] private float responseExponent = 0.82f;
        [SerializeField] [Range(0f, 0.45f)] private float responseBoost = 0.18f;
        [SerializeField] private bool hideWhenIdle = true;

        private RectTransform touchAreaRect;
        private int activePointerId = int.MinValue;
        private bool isPressed;
        private bool inputEnabled = true;
        private Vector2 originLocalPoint;
        private Vector2 inputVector;

        public Vector2 InputVector => inputVector;
        public bool IsPressed => isPressed;

        private void Awake()
        {
            touchAreaRect = transform as RectTransform;
            ResetJoystickState();
        }

        private void OnDisable()
        {
            ResetJoystickState();
        }

        public void Configure(RectTransform baseRect, RectTransform knobRect, float joystickRadius)
        {
            joystickBase = baseRect;
            joystickKnob = knobRect;
            radius = Mathf.Max(10f, joystickRadius);
            ResetJoystickState();
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!inputEnabled)
            {
                ResetJoystickState();
            }
            else
            {
                SetVisible(!hideWhenIdle);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!inputEnabled || isPressed)
            {
                return;
            }

            if (!TryGetLocalPoint(eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            isPressed = true;
            activePointerId = eventData.pointerId;
            originLocalPoint = localPoint;

            if (joystickBase != null)
            {
                joystickBase.anchoredPosition = originLocalPoint;
            }

            SetVisible(true);
            UpdateInput(eventData.position, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!inputEnabled || !isPressed || eventData.pointerId != activePointerId)
            {
                return;
            }

            UpdateInput(eventData.position, eventData.pressEventCamera);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed || eventData.pointerId != activePointerId)
            {
                return;
            }

            ResetJoystickState();
        }

        private void UpdateInput(Vector2 screenPosition, Camera eventCamera)
        {
            if (!TryGetLocalPoint(screenPosition, eventCamera, out var localPoint))
            {
                return;
            }

            var delta = localPoint - originLocalPoint;
            var clamped = Vector2.ClampMagnitude(delta, radius);
            var normalized = clamped / radius;
            var magnitude = Mathf.Clamp01(normalized.magnitude);
            if (magnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }
            else
            {
                var range = Mathf.Max(0.0001f, 1f - deadZone);
                var remapped = Mathf.Clamp01((magnitude - deadZone) / range);
                var exponent = Mathf.Clamp(responseExponent, 0.5f, 2f);
                var curvedMagnitude = Mathf.Pow(remapped, exponent);
                var boost = Mathf.Clamp01(responseBoost);
                curvedMagnitude = Mathf.Clamp01(curvedMagnitude + remapped * boost * (1f - curvedMagnitude));
                inputVector = normalized.normalized * curvedMagnitude;
            }

            if (joystickKnob != null)
            {
                joystickKnob.anchoredPosition = clamped;
            }
        }

        private bool TryGetLocalPoint(Vector2 screenPosition, Camera eventCamera, out Vector2 localPoint)
        {
            if (touchAreaRect == null)
            {
                touchAreaRect = transform as RectTransform;
            }

            if (touchAreaRect == null)
            {
                localPoint = default;
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(touchAreaRect, screenPosition, eventCamera, out localPoint);
        }

        private void ResetJoystickState()
        {
            isPressed = false;
            activePointerId = int.MinValue;
            inputVector = Vector2.zero;

            if (joystickKnob != null)
            {
                joystickKnob.anchoredPosition = Vector2.zero;
            }

            SetVisible(!hideWhenIdle);
        }

        private void SetVisible(bool visible)
        {
            if (joystickBase != null)
            {
                joystickBase.gameObject.SetActive(inputEnabled && (visible || !hideWhenIdle));
            }
        }
    }
}
