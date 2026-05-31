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
            var savedProgression = TryLoadProgressionFile(SavePath);
            if (savedProgression != null)
            {
                return savedProgression;
            }

            return TryLoadProgressionFile(BackupPath);
        }

        private static PlayerProgressionData TryLoadProgressionFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                var json = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<PlayerProgressionData>(json);
            }
            catch (IOException)
            {
                return null;
            }
            catch (System.Exception exception) when (exception is System.ArgumentException || exception is System.InvalidOperationException)
            {
                return null;
            }
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
            data.stage.highestStageCleared = Mathf.Clamp(data.stage.highestStageCleared, 0, Mathf.Max(0, data.stage.highestStageReached - 1));
            data.stage.currentLobbyStage = Mathf.Clamp(data.stage.currentLobbyStage, 1, data.stage.highestStageReached);
        }
    }
}
