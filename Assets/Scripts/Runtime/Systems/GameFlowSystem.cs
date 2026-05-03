using UnityEngine;

namespace AlienCrusher.Systems
{
    public class GameFlowSystem : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private bool autoStartStage = false;
        [SerializeField] private float stageDurationSeconds = 90f;

        public bool AutoStartStage => autoStartStage;
        public float StageDurationSeconds => stageDurationSeconds;
    }
}

