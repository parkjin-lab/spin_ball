using System;
using System.Collections.Generic;

namespace AlienCrusher.Systems
{
    [Serializable]
    public sealed class PlayerProgressionData
    {
        public int schemaVersion = 1;
        public MetaProgressionData meta = new MetaProgressionData();
        public StageProgressionData stage = new StageProgressionData();
    }

    [Serializable]
    public sealed class MetaProgressionData
    {
        public int dpBalance;
        public int selectedForm;
        public List<int> unlockedForms = new List<int>();
        public List<MetaUpgradeLevelEntry> metaUpgradeLevels = new List<MetaUpgradeLevelEntry>();
    }

    [Serializable]
    public sealed class StageProgressionData
    {
        public int highestStageReached = 1;
        public int highestStageCleared;
        public int currentLobbyStage = 1;
    }

    [Serializable]
    public sealed class MetaUpgradeLevelEntry
    {
        public string upgradeId;
        public int level;
    }
}
