using UnityEngine;

namespace AlienCrusher.Systems
{
    public class ScoreSystem : MonoBehaviour
    {
        [Header("Dummy Score")]
        [SerializeField] private int currentScore;
        [SerializeField] private int chainCount;
        [SerializeField] private int highestChain;
        [SerializeField] private int destroyedCount;
        [Header("Momentum Chain")]
        [SerializeField] private bool useMomentumChainTimer = true;
        [SerializeField] private float chainWindowSeconds = 2.28f;
        [SerializeField] private float chainWindowPerChainBonus = 0.022f;
        [SerializeField] private float maxChainWindowSeconds = 3.05f;

        private float chainTimerRemaining;

        public int CurrentScore => currentScore;
        public int ChainCount => chainCount;
        public int HighestChain => highestChain;
        public int DestroyedCount => destroyedCount;
        public float ChainTimerRemaining => chainTimerRemaining;
        public bool HasActiveChainTimer => useMomentumChainTimer && chainCount > 0 && chainTimerRemaining > 0f;
        public float ChainTimerRatio => !useMomentumChainTimer ? 1f : Mathf.Clamp01(chainTimerRemaining / Mathf.Max(0.01f, GetCurrentChainWindow()));

        public void AddScore(int value)
        {
            currentScore += Mathf.Max(0, value);
        }

        public void RegisterDestruction(int scoreValue)
        {
            AddScore(scoreValue);
            destroyedCount += 1;
            RegisterChainHit();
        }

        public void RegisterChainHit()
        {
            chainCount += 1;
            if (chainCount > highestChain)
            {
                highestChain = chainCount;
            }
            RefreshChainTimer();
        }

        public bool TickChainTimer(float deltaTime, out int brokenChain)
        {
            brokenChain = 0;
            if (!useMomentumChainTimer || chainCount <= 0)
            {
                return false;
            }

            chainTimerRemaining = Mathf.Max(0f, chainTimerRemaining - Mathf.Max(0f, deltaTime));
            if (chainTimerRemaining > 0f)
            {
                return false;
            }

            brokenChain = chainCount;
            BreakChain();
            return brokenChain > 1;
        }

        public void BreakChain()
        {
            chainCount = 0;
            chainTimerRemaining = 0f;
        }

        public void ResetScore()
        {
            currentScore = 0;
            chainCount = 0;
            highestChain = 0;
            destroyedCount = 0;
            chainTimerRemaining = 0f;
        }

        private void RefreshChainTimer()
        {
            if (!useMomentumChainTimer)
            {
                return;
            }

            chainTimerRemaining = GetCurrentChainWindow();
        }

        private float GetCurrentChainWindow()
        {
            float baseWindow = Mathf.Max(0.6f, chainWindowSeconds);
            float bonus = Mathf.Max(0f, chainWindowPerChainBonus) * Mathf.Max(0, chainCount - 1);
            return Mathf.Clamp(baseWindow + bonus, 0.6f, Mathf.Max(baseWindow, maxChainWindowSeconds));
        }
    }
}
