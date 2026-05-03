using System.Collections.Generic;
using AlienCrusher.Gameplay;
using UnityEngine;

namespace AlienCrusher.Systems
{
    public class FormUnlockSystem : MonoBehaviour
    {
        public enum MetaUpgradeType
        {
            SizeCore = 0,
            ImpactCore = 1,
            DpAmplifier = 2
        }

        private const string UnlockKeyPrefix = "AlienCrusher.FormUnlocked.";
        private const string SelectedKey = "AlienCrusher.FormSelected";
        private const string DpBalanceKey = "AlienCrusher.MetaDp";
        private const string MetaUpgradeLevelKeyPrefix = "AlienCrusher.MetaUpgradeLevel.";
        private const string HighestUnlockedStageKey = "AlienCrusher.HighestUnlockedStage";
        private const string CurrentLobbyStageKey = "AlienCrusher.CurrentLobbyStage";

        [SerializeField] private FormType defaultForm = FormType.Sphere;
        [SerializeField] private int spikeUnlockCost = 450;
        [SerializeField] private int ramUnlockCost = 900;
        [SerializeField] private int saucerUnlockCost = 1350;
        [SerializeField] private int crusherUnlockCost = 2100;

        [Header("Meta Shop")]
        [SerializeField] private int sizeCoreBaseCost = 220;
        [SerializeField] private int impactCoreBaseCost = 280;
        [SerializeField] private int dpAmplifierBaseCost = 340;
        [SerializeField] private float metaCostGrowth = 1.45f;
        [SerializeField] private int sizeCoreMaxLevel = 8;
        [SerializeField] private int impactCoreMaxLevel = 8;
        [SerializeField] private int dpAmplifierMaxLevel = 6;
        [SerializeField] private float sizeCoreBonusPerLevel = 0.08f;
        [SerializeField] private float impactCoreBonusPerLevel = 0.12f;
        [SerializeField] private float dpAmplifierBonusPerLevel = 0.12f;

        private ProgressionSaveSystem progressionSaveSystem;
        private PlayerProgressionData progressionData;

        public FormType CurrentForm { get; private set; }
        public int DpBalance => HasProgressionData ? Mathf.Max(0, progressionData.meta.dpBalance) : Mathf.Max(0, PlayerPrefs.GetInt(DpBalanceKey, 0));
        public int HighestUnlockedStage => HasProgressionData ? Mathf.Max(1, progressionData.stage.highestStageReached) : Mathf.Max(1, PlayerPrefs.GetInt(HighestUnlockedStageKey, 1));
        public int CurrentLobbyStage => HasProgressionData
            ? Mathf.Clamp(progressionData.stage.currentLobbyStage, 1, Mathf.Max(1, progressionData.stage.highestStageReached))
            : Mathf.Clamp(PlayerPrefs.GetInt(CurrentLobbyStageKey, 1), 1, Mathf.Max(1, PlayerPrefs.GetInt(HighestUnlockedStageKey, 1)));

        private bool HasProgressionData => progressionData != null && progressionData.meta != null && progressionData.stage != null;

        private void Awake()
        {
            ResolveSaveSystem();
            EnsureDefaultUnlocked();
            LoadSelection();
        }

        public bool IsUnlocked(FormType form)
        {
            if (HasProgressionData)
            {
                if (form == defaultForm)
                {
                    return true;
                }

                return progressionData.meta.unlockedForms.Contains((int)form);
            }

            var key = UnlockKeyPrefix + form;
            var fallback = form == defaultForm ? 1 : 0;
            return PlayerPrefs.GetInt(key, fallback) == 1;
        }

        public int GetUnlockCost(FormType form)
        {
            switch (form)
            {
                case FormType.Spike:
                    return Mathf.Max(0, spikeUnlockCost);
                case FormType.Ram:
                    return Mathf.Max(0, ramUnlockCost);
                case FormType.Saucer:
                    return Mathf.Max(0, saucerUnlockCost);
                case FormType.Crusher:
                    return Mathf.Max(0, crusherUnlockCost);
                default:
                    return 0;
            }
        }

        public int GetMetaUpgradeLevel(MetaUpgradeType upgradeType)
        {
            if (HasProgressionData)
            {
                var level = FindMetaUpgradeLevel(upgradeType);
                return Mathf.Clamp(level, 0, GetMetaUpgradeMaxLevel(upgradeType));
            }

            var key = MetaUpgradeLevelKeyPrefix + upgradeType;
            var legacyLevel = PlayerPrefs.GetInt(key, 0);
            return Mathf.Clamp(legacyLevel, 0, GetMetaUpgradeMaxLevel(upgradeType));
        }

        public int GetMetaUpgradeMaxLevel(MetaUpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case MetaUpgradeType.SizeCore:
                    return Mathf.Max(0, sizeCoreMaxLevel);
                case MetaUpgradeType.ImpactCore:
                    return Mathf.Max(0, impactCoreMaxLevel);
                case MetaUpgradeType.DpAmplifier:
                    return Mathf.Max(0, dpAmplifierMaxLevel);
                default:
                    return 0;
            }
        }

        public bool IsMetaUpgradeMaxed(MetaUpgradeType upgradeType)
        {
            return GetMetaUpgradeLevel(upgradeType) >= GetMetaUpgradeMaxLevel(upgradeType);
        }

        public int GetMetaUpgradeCost(MetaUpgradeType upgradeType)
        {
            if (IsMetaUpgradeMaxed(upgradeType))
            {
                return 0;
            }

            var baseCost = GetMetaUpgradeBaseCost(upgradeType);
            var level = GetMetaUpgradeLevel(upgradeType);
            var growth = Mathf.Max(1f, metaCostGrowth);
            var cost = baseCost * Mathf.Pow(growth, level);
            return Mathf.Max(1, Mathf.RoundToInt(cost));
        }

        public bool TryPurchaseMetaUpgrade(MetaUpgradeType upgradeType, out int requiredCost)
        {
            requiredCost = GetMetaUpgradeCost(upgradeType);
            if (requiredCost <= 0)
            {
                return false;
            }

            if (!TrySpendDp(requiredCost))
            {
                return false;
            }

            var next = Mathf.Clamp(GetMetaUpgradeLevel(upgradeType) + 1, 0, GetMetaUpgradeMaxLevel(upgradeType));
            SetMetaUpgradeLevel(upgradeType, next);
            return true;
        }

        public float GetPermanentStartScaleBonus()
        {
            return Mathf.Max(0, GetMetaUpgradeLevel(MetaUpgradeType.SizeCore)) * Mathf.Max(0f, sizeCoreBonusPerLevel);
        }

        public float GetPermanentImpactMultiplier()
        {
            var bonus = Mathf.Max(0, GetMetaUpgradeLevel(MetaUpgradeType.ImpactCore)) * Mathf.Max(0f, impactCoreBonusPerLevel);
            return Mathf.Max(1f, 1f + bonus);
        }

        public float GetMetaDpRewardMultiplier()
        {
            var bonus = Mathf.Max(0, GetMetaUpgradeLevel(MetaUpgradeType.DpAmplifier)) * Mathf.Max(0f, dpAmplifierBonusPerLevel);
            return Mathf.Max(1f, 1f + bonus);
        }

        public void AddDp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (HasProgressionData)
            {
                progressionData.meta.dpBalance = Mathf.Max(0, progressionData.meta.dpBalance + amount);
                SaveProgression();
                return;
            }

            var next = DpBalance + amount;
            PlayerPrefs.SetInt(DpBalanceKey, next);
            PlayerPrefs.Save();
        }

        public bool TrySpendDp(int amount)
        {
            var safeAmount = Mathf.Max(0, amount);
            if (safeAmount <= 0)
            {
                return true;
            }

            var balance = DpBalance;
            if (balance < safeAmount)
            {
                return false;
            }

            if (HasProgressionData)
            {
                progressionData.meta.dpBalance = balance - safeAmount;
                SaveProgression();
                return true;
            }

            PlayerPrefs.SetInt(DpBalanceKey, balance - safeAmount);
            PlayerPrefs.Save();
            return true;
        }

        public void Unlock(FormType form)
        {
            if (IsUnlocked(form))
            {
                return;
            }

            if (HasProgressionData)
            {
                if (!progressionData.meta.unlockedForms.Contains((int)form))
                {
                    progressionData.meta.unlockedForms.Add((int)form);
                    SaveProgression();
                }

                return;
            }

            var key = UnlockKeyPrefix + form;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        public bool TryUnlockWithCost(FormType form, out int requiredCost)
        {
            requiredCost = GetUnlockCost(form);
            if (IsUnlocked(form))
            {
                return true;
            }

            if (!TrySpendDp(requiredCost))
            {
                return false;
            }

            Unlock(form);
            return true;
        }

        public bool TrySelect(FormType form)
        {
            if (!IsUnlocked(form))
            {
                return false;
            }

            CurrentForm = form;

            if (HasProgressionData)
            {
                progressionData.meta.selectedForm = (int)form;
                SaveProgression();
                return true;
            }

            PlayerPrefs.SetInt(SelectedKey, (int)form);
            PlayerPrefs.Save();
            return true;
        }

        public bool TryUnlockAndSelectWithCost(FormType form, out int requiredCost)
        {
            if (!TryUnlockWithCost(form, out requiredCost))
            {
                return false;
            }

            return TrySelect(form);
        }

        public void ApplyToPlayer(PlayerBallDummyController player)
        {
            if (player == null)
            {
                return;
            }

            player.SetBaseForm(CurrentForm);
        }

        public bool RegisterClearedStage(int clearedStageNumber)
        {
            var clearedStage = Mathf.Max(1, clearedStageNumber);
            var unlockedStage = Mathf.Max(1, clearedStage + 1);

            if (HasProgressionData)
            {
                var changed = false;

                if (clearedStage > progressionData.stage.highestStageCleared)
                {
                    progressionData.stage.highestStageCleared = clearedStage;
                    changed = true;
                }

                if (unlockedStage > progressionData.stage.highestStageReached)
                {
                    progressionData.stage.highestStageReached = unlockedStage;
                    changed = true;
                }

                if (progressionData.stage.currentLobbyStage < progressionData.stage.highestStageReached)
                {
                    progressionData.stage.currentLobbyStage = progressionData.stage.highestStageReached;
                    changed = true;
                }

                if (changed)
                {
                    SaveProgression();
                }

                return changed;
            }

            if (unlockedStage <= HighestUnlockedStage)
            {
                return false;
            }

            PlayerPrefs.SetInt(HighestUnlockedStageKey, unlockedStage);
            PlayerPrefs.Save();
            return true;
        }

        public void SetCurrentLobbyStage(int stageNumber)
        {
            var clampedStage = Mathf.Clamp(stageNumber, 1, Mathf.Max(1, HighestUnlockedStage));

            if (HasProgressionData)
            {
                if (progressionData.stage.currentLobbyStage != clampedStage)
                {
                    progressionData.stage.currentLobbyStage = clampedStage;
                    SaveProgression();
                }

                return;
            }

            PlayerPrefs.SetInt(CurrentLobbyStageKey, clampedStage);
            PlayerPrefs.Save();
        }

        private int GetMetaUpgradeBaseCost(MetaUpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case MetaUpgradeType.SizeCore:
                    return Mathf.Max(1, sizeCoreBaseCost);
                case MetaUpgradeType.ImpactCore:
                    return Mathf.Max(1, impactCoreBaseCost);
                case MetaUpgradeType.DpAmplifier:
                    return Mathf.Max(1, dpAmplifierBaseCost);
                default:
                    return 1;
            }
        }

        private void SetMetaUpgradeLevel(MetaUpgradeType upgradeType, int level)
        {
            var clamped = Mathf.Clamp(level, 0, GetMetaUpgradeMaxLevel(upgradeType));

            if (HasProgressionData)
            {
                var id = upgradeType.ToString();
                for (var i = 0; i < progressionData.meta.metaUpgradeLevels.Count; i++)
                {
                    var entry = progressionData.meta.metaUpgradeLevels[i];
                    if (entry != null && entry.upgradeId == id)
                    {
                        entry.level = clamped;
                        SaveProgression();
                        return;
                    }
                }

                progressionData.meta.metaUpgradeLevels.Add(new MetaUpgradeLevelEntry
                {
                    upgradeId = id,
                    level = clamped
                });
                SaveProgression();
                return;
            }

            var key = MetaUpgradeLevelKeyPrefix + upgradeType;
            PlayerPrefs.SetInt(key, clamped);
            PlayerPrefs.Save();
        }

        private void EnsureDefaultUnlocked()
        {
            if (!IsUnlocked(defaultForm))
            {
                Unlock(defaultForm);
            }
        }

        private void LoadSelection()
        {
            FormType saved;
            if (HasProgressionData)
            {
                saved = (FormType)Mathf.Clamp(progressionData.meta.selectedForm, 0, (int)FormType.Crusher);
            }
            else
            {
                saved = (FormType)PlayerPrefs.GetInt(SelectedKey, (int)defaultForm);
            }

            if (!IsUnlocked(saved))
            {
                saved = defaultForm;
            }

            CurrentForm = saved;
        }

        private void ResolveSaveSystem()
        {
            progressionSaveSystem = FindFirstObjectByType<ProgressionSaveSystem>();
            if (progressionSaveSystem == null)
            {
                var systemsRoot = GameObject.Find("_Systems");
                if (systemsRoot == null)
                {
                    systemsRoot = new GameObject("_Systems");
                }

                var saveObject = new GameObject("ProgressionSaveSystem");
                saveObject.transform.SetParent(systemsRoot.transform, false);
                progressionSaveSystem = saveObject.AddComponent<ProgressionSaveSystem>();
            }

            progressionSaveSystem.LoadOrCreate();
            progressionData = progressionSaveSystem.Current;
            if (IsProgressionDataEmpty(progressionData))
            {
                MigrateFromLegacyPlayerPrefs(progressionData);
                SaveProgression();
            }
        }

        private void SaveProgression()
        {
            if (progressionSaveSystem == null)
            {
                return;
            }

            progressionSaveSystem.MarkDirtyAndSave();
        }

        private int FindMetaUpgradeLevel(MetaUpgradeType upgradeType)
        {
            if (!HasProgressionData)
            {
                return 0;
            }

            var id = upgradeType.ToString();
            for (var i = 0; i < progressionData.meta.metaUpgradeLevels.Count; i++)
            {
                var entry = progressionData.meta.metaUpgradeLevels[i];
                if (entry != null && entry.upgradeId == id)
                {
                    return entry.level;
                }
            }

            return 0;
        }

        private bool IsProgressionDataEmpty(PlayerProgressionData data)
        {
            if (data == null || data.meta == null || data.stage == null)
            {
                return true;
            }

            return data.meta.dpBalance <= 0
                && data.meta.selectedForm == 0
                && (data.meta.unlockedForms == null || data.meta.unlockedForms.Count == 0)
                && (data.meta.metaUpgradeLevels == null || data.meta.metaUpgradeLevels.Count == 0)
                && data.stage.highestStageReached <= 1
                && data.stage.highestStageCleared <= 0
                && data.stage.currentLobbyStage <= 1;
        }

        private void MigrateFromLegacyPlayerPrefs(PlayerProgressionData data)
        {
            if (data == null)
            {
                return;
            }

            if (data.meta == null)
            {
                data.meta = new MetaProgressionData();
            }

            if (data.stage == null)
            {
                data.stage = new StageProgressionData();
            }

            data.meta.dpBalance = Mathf.Max(0, PlayerPrefs.GetInt(DpBalanceKey, 0));
            data.meta.selectedForm = PlayerPrefs.GetInt(SelectedKey, (int)defaultForm);

            if (data.meta.unlockedForms == null)
            {
                data.meta.unlockedForms = new List<int>();
            }
            else
            {
                data.meta.unlockedForms.Clear();
            }

            foreach (FormType form in System.Enum.GetValues(typeof(FormType)))
            {
                var fallback = form == defaultForm ? 1 : 0;
                if (PlayerPrefs.GetInt(UnlockKeyPrefix + form, fallback) == 1 && !data.meta.unlockedForms.Contains((int)form))
                {
                    data.meta.unlockedForms.Add((int)form);
                }
            }

            if (!data.meta.unlockedForms.Contains((int)defaultForm))
            {
                data.meta.unlockedForms.Add((int)defaultForm);
            }

            if (data.meta.metaUpgradeLevels == null)
            {
                data.meta.metaUpgradeLevels = new List<MetaUpgradeLevelEntry>();
            }
            else
            {
                data.meta.metaUpgradeLevels.Clear();
            }

            foreach (MetaUpgradeType upgradeType in System.Enum.GetValues(typeof(MetaUpgradeType)))
            {
                data.meta.metaUpgradeLevels.Add(new MetaUpgradeLevelEntry
                {
                    upgradeId = upgradeType.ToString(),
                    level = Mathf.Max(0, PlayerPrefs.GetInt(MetaUpgradeLevelKeyPrefix + upgradeType, 0))
                });
            }

            data.stage.highestStageReached = Mathf.Max(1, PlayerPrefs.GetInt(HighestUnlockedStageKey, 1));
            data.stage.highestStageCleared = Mathf.Max(0, data.stage.highestStageReached - 1);
            data.stage.currentLobbyStage = Mathf.Clamp(PlayerPrefs.GetInt(CurrentLobbyStageKey, data.stage.highestStageReached), 1, Mathf.Max(1, data.stage.highestStageReached));
        }
    }
}
