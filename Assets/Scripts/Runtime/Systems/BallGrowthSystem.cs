using UnityEngine;

namespace AlienCrusher.Systems
{
    public class BallGrowthSystem : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string playerBallName = "PlayerBall";

        [Header("Growth")]
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float growthPerDestruction = 0.04f;
        [SerializeField] private float growthPerLevelUp = 0.08f;
        [SerializeField] private float maxScale = 2.2f;
        [SerializeField] private float scaleLerpSpeed = 12f;

        [Header("Physics")]
        [SerializeField] private float baseMass = 10f;
        [SerializeField] private float massBonusAtMaxScale = 12f;

        private ScoreSystem scoreSystem;
        private Transform playerBall;
        private Rigidbody playerBody;

        private int lastDestroyedCount = -1;
        private int levelUpGrowthCount;
        private Vector3 targetScale = Vector3.one;
        private float permanentBaseScaleBonus;

        private void Awake()
        {
            scoreSystem = Object.FindFirstObjectByType<ScoreSystem>();
            ResolvePlayerReferences();
            ResetGrowth();
        }

        private void Update()
        {
            if (scoreSystem == null)
            {
                scoreSystem = Object.FindFirstObjectByType<ScoreSystem>();
            }

            ResolvePlayerReferences();
            UpdateGrowthFromScore();
            SmoothScale();
        }

        public void ResetGrowth()
        {
            lastDestroyedCount = -1;
            levelUpGrowthCount = 0;
            ApplyGrowth(0, immediate: true);
        }

        public void RegisterLevelUpGrowth()
        {
            levelUpGrowthCount++;
            var destroyed = Mathf.Max(0, scoreSystem != null ? scoreSystem.DestroyedCount : 0);
            ApplyGrowth(destroyed, immediate: false);
        }

        public void SetPermanentBaseScaleBonus(float bonus)
        {
            permanentBaseScaleBonus = Mathf.Max(0f, bonus);
            var destroyed = Mathf.Max(0, scoreSystem != null ? scoreSystem.DestroyedCount : 0);
            ApplyGrowth(destroyed, immediate: true);
        }

        private void ResolvePlayerReferences()
        {
            if (playerBall == null)
            {
                var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var item in transforms)
                {
                    if (item.name == playerBallName)
                    {
                        playerBall = item;
                        break;
                    }
                }
            }

            if (playerBall != null && playerBody == null)
            {
                playerBody = playerBall.GetComponent<Rigidbody>();
            }
        }

        private void UpdateGrowthFromScore()
        {
            if (scoreSystem == null)
            {
                return;
            }

            var destroyed = Mathf.Max(0, scoreSystem.DestroyedCount);
            if (destroyed == lastDestroyedCount)
            {
                return;
            }

            lastDestroyedCount = destroyed;
            ApplyGrowth(destroyed, immediate: false);
        }

        private void ApplyGrowth(int destroyedCount, bool immediate)
        {
            var minScale = baseScale + Mathf.Max(0f, permanentBaseScaleBonus);
            var safeMax = Mathf.Max(minScale + 0.01f, maxScale);
            var levelUpBonus = Mathf.Max(0, levelUpGrowthCount) * Mathf.Max(0f, growthPerLevelUp);
            var size = Mathf.Clamp(minScale + destroyedCount * growthPerDestruction + levelUpBonus, minScale, safeMax);
            targetScale = Vector3.one * size;

            if (playerBall != null && immediate)
            {
                playerBall.localScale = targetScale;
            }

            if (playerBody != null)
            {
                var normalized = Mathf.InverseLerp(minScale, safeMax, size);
                playerBody.mass = baseMass + massBonusAtMaxScale * normalized;
            }
        }

        private void SmoothScale()
        {
            if (playerBall == null)
            {
                return;
            }

            var t = 1f - Mathf.Exp(-scaleLerpSpeed * Time.deltaTime);
            playerBall.localScale = Vector3.Lerp(playerBall.localScale, targetScale, t);
        }
    }
}