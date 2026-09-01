using UnityEngine;
using UnityEngine.EventSystems;

namespace AlienCrusher.Systems
{
    public sealed class FormSkillHoldTracker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool Held { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            Held = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Held = false;
        }

        private void OnDisable()
        {
            Held = false;
        }
    }
}
