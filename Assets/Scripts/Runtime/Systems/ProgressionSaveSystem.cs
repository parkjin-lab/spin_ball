using System;
using System.IO;
using System.Text;
using AlienCrusher.Gameplay;
using UnityEngine;

namespace AlienCrusher.Systems
{
    public class ProgressionSaveSystem : MonoBehaviour
    {
        private const string SaveFileName = "aliencrusher_progression.json";
        private const string BackupFileName = "aliencrusher_progression.bak.json";
        private const string CorruptFileName = "aliencrusher_progression.corrupt.json";
        private const int DefaultFormIndex = (int)FormType.Sphere;
        private const int MaxKnownFormIndex = (int)FormType.Crusher;

        public PlayerProgressionData Current { get; private set; }

#if UNITY_EDITOR
        private static string validationStorageDirectory;
#endif

        private string SavePath => Path.Combine(StorageDirectory, SaveFileName);
        private string BackupPath => Path.Combine(StorageDirectory, BackupFileName);
        private string CorruptPath => Path.Combine(StorageDirectory, CorruptFileName);

        private static string StorageDirectory
        {
            get
            {
#if UNITY_EDITOR
                if (!string.IsNullOrWhiteSpace(validationStorageDirectory))
                {
                    return validationStorageDirectory;
                }
#endif
                return Application.persistentDataPath;
            }
        }

#if UNITY_EDITOR
        public static void SetValidationStorageDirectory(string directory)
        {
            validationStorageDirectory = directory;
        }
#endif

        private void Awake()
        {
            LoadOrCreate();
        }

        public void LoadOrCreate()
        {
            Current = TryLoadFromDisk(out var loadedFromBackup);
            if (Current == null)
            {
                Current = CreateDefault();
                Save();
                return;
            }

            var repaired = Sanitize(Current);
            if (loadedFromBackup)
            {
                TrySave(preserveExistingBackup: true);
            }
            else if (repaired)
            {
                Save();
            }
        }

        public bool Save()
        {
            return TrySave(preserveExistingBackup: false);
        }

        public bool TryCommit(Action<PlayerProgressionData> mutation)
        {
            if (mutation == null)
            {
                throw new ArgumentNullException(nameof(mutation));
            }

            if (Current == null)
            {
                Current = CreateDefault();
            }

            var snapshotJson = JsonUtility.ToJson(Current);
            try
            {
                mutation(Current);
                if (Save())
                {
                    return true;
                }
            }
            catch
            {
                Current = JsonUtility.FromJson<PlayerProgressionData>(snapshotJson) ?? CreateDefault();
                throw;
            }

            Current = JsonUtility.FromJson<PlayerProgressionData>(snapshotJson) ?? CreateDefault();
            return false;
        }

        private bool TrySave(bool preserveExistingBackup)
        {
            if (Current == null)
            {
                Current = CreateDefault();
            }

            Sanitize(Current);

            var json = JsonUtility.ToJson(Current, true);
            var tempPath = SavePath + ".tmp";
            try
            {
                WriteAndFlushTempFile(tempPath, json);
                if (TryLoadProgressionFile(tempPath) == null)
                {
                    return false;
                }

                if (!File.Exists(SavePath))
                {
                    File.Move(tempPath, SavePath);
                    return true;
                }

                var replacementBackupPath = preserveExistingBackup ? CorruptPath : BackupPath;
                ReplaceSaveFile(tempPath, replacementBackupPath);
                return true;
            }
            catch (Exception exception) when (IsExpectedSaveException(exception))
            {
                Debug.LogWarning($"[AlienCrusher] Progression save failed: {exception.Message}");
                return false;
            }
            finally
            {
                TryDeleteFile(tempPath);
            }
        }

        public bool MarkDirtyAndSave()
        {
            return Save();
        }

        public PlayerProgressionData CreateDefault()
        {
            return new PlayerProgressionData();
        }

        private PlayerProgressionData TryLoadFromDisk(out bool loadedFromBackup)
        {
            loadedFromBackup = false;
            var savedProgression = TryLoadProgressionFile(SavePath);
            if (savedProgression != null)
            {
                return savedProgression;
            }
            savedProgression = TryLoadProgressionFile(BackupPath);
            loadedFromBackup = savedProgression != null;
            return savedProgression;
        }

        private static void WriteAndFlushTempFile(string path, string json)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(json);
            writer.Flush();
            stream.Flush(flushToDisk: true);
        }

        private void ReplaceSaveFile(string tempPath, string replacementBackupPath)
        {
            try
            {
                File.Replace(tempPath, SavePath, replacementBackupPath, ignoreMetadataErrors: true);
            }
            catch (PlatformNotSupportedException)
            {
                File.Copy(SavePath, replacementBackupPath, overwrite: true);
                MoveOverwrite(tempPath, SavePath);
            }
        }

        private static bool IsExpectedSaveException(Exception exception)
        {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is NotSupportedException
                || exception is System.Security.SecurityException;
        }

        private static void MoveOverwrite(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(sourcePath, destinationPath);
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception exception) when (IsExpectedSaveException(exception))
            {
            }
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

        private static bool Sanitize(PlayerProgressionData data)
        {
            if (data == null)
            {
                return false;
            }

            var beforeJson = JsonUtility.ToJson(data);

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

            data.meta.dpBalance = Mathf.Max(0, data.meta.dpBalance);
            data.meta.selectedForm = Mathf.Clamp(data.meta.selectedForm, DefaultFormIndex, MaxKnownFormIndex);
            SanitizeUnlockedForms(data.meta);
            SanitizeMetaUpgradeLevels(data.meta);
            data.stage.highestStageReached = Mathf.Max(1, data.stage.highestStageReached);
            data.stage.highestStageCleared = Mathf.Clamp(data.stage.highestStageCleared, 0, Mathf.Max(0, data.stage.highestStageReached - 1));
            data.stage.currentLobbyStage = Mathf.Clamp(data.stage.currentLobbyStage, 1, data.stage.highestStageReached);

            return beforeJson != JsonUtility.ToJson(data);
        }

        private static void SanitizeUnlockedForms(MetaProgressionData meta)
        {
            var seenForms = new System.Collections.Generic.HashSet<int>();
            for (var i = meta.unlockedForms.Count - 1; i >= 0; i--)
            {
                var formIndex = meta.unlockedForms[i];
                if (formIndex < DefaultFormIndex || formIndex > MaxKnownFormIndex || !seenForms.Add(formIndex))
                {
                    meta.unlockedForms.RemoveAt(i);
                }
            }

            if (!meta.unlockedForms.Contains(DefaultFormIndex))
            {
                meta.unlockedForms.Add(DefaultFormIndex);
            }

            if (!meta.unlockedForms.Contains(meta.selectedForm))
            {
                meta.selectedForm = DefaultFormIndex;
            }
        }

        private static void SanitizeMetaUpgradeLevels(MetaProgressionData meta)
        {
            var seenUpgradeIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            for (var i = meta.metaUpgradeLevels.Count - 1; i >= 0; i--)
            {
                var entry = meta.metaUpgradeLevels[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.upgradeId))
                {
                    meta.metaUpgradeLevels.RemoveAt(i);
                    continue;
                }

                entry.upgradeId = entry.upgradeId.Trim();
                if (!seenUpgradeIds.Add(entry.upgradeId))
                {
                    meta.metaUpgradeLevels.RemoveAt(i);
                    continue;
                }

                entry.level = Mathf.Max(0, entry.level);
            }
        }
    }
}
