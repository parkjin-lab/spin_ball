using System.IO;
using UnityEngine;

namespace AlienCrusher.Systems
{
    public class ProgressionSaveSystem : MonoBehaviour
    {
        private const string SaveFileName = "aliencrusher_progression.json";
        private const string BackupFileName = "aliencrusher_progression.bak.json";

        public PlayerProgressionData Current { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        private void Awake()
        {
            LoadOrCreate();
        }

        public void LoadOrCreate()
        {
            Current = TryLoadFromDisk();
            if (Current == null)
            {
                Current = CreateDefault();
                Save();
            }

            Sanitize(Current);
        }

        public void Save()
        {
            if (Current == null)
            {
                Current = CreateDefault();
            }

            Sanitize(Current);

            var json = JsonUtility.ToJson(Current, true);
            var tempPath = SavePath + ".tmp";
            File.WriteAllText(tempPath, json);

            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupPath, true);
            }

            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }

            File.Move(tempPath, SavePath);
        }

        public void MarkDirtyAndSave()
        {
            Save();
        }

        public PlayerProgressionData CreateDefault()
        {
            return new PlayerProgressionData();
        }

        private PlayerProgressionData TryLoadFromDisk()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var json = File.ReadAllText(SavePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonUtility.FromJson<PlayerProgressionData>(json);
                    }
                }

                if (File.Exists(BackupPath))
                {
                    var json = File.ReadAllText(BackupPath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonUtility.FromJson<PlayerProgressionData>(json);
                    }
                }
            }
            catch (IOException)
            {
            }

            return null;
        }

        private static void Sanitize(PlayerProgressionData data)
        {
            if (data == null)
            {
                return;
            }

            data.schemaVersion = Mathf.Max(1, data.schemaVersion);
            if (data.meta == null)
            {
                data.meta = new MetaProgressionData();
            }

            if (data.stage == null)
            {
                data.stage = new StageProgressionData();
            }

            if (data.meta.unlockedForms == null)
            {
                data.meta.unlockedForms = new System.Collections.Generic.List<int>();
            }

            if (data.meta.metaUpgradeLevels == null)
            {
                data.meta.metaUpgradeLevels = new System.Collections.Generic.List<MetaUpgradeLevelEntry>();
            }

            data.stage.highestStageReached = Mathf.Max(1, data.stage.highestStageReached);
            data.stage.currentLobbyStage = Mathf.Max(1, data.stage.currentLobbyStage);
            data.stage.highestStageCleared = Mathf.Clamp(data.stage.highestStageCleared, 0, Mathf.Max(data.stage.highestStageReached, 1));
        }
    }
}
