#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using AlienCrusher.Gameplay;
using AlienCrusher.Systems;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.EditorTools
{
    public static class ProgressionSaveTransactionValidator
    {
        private const string MenuPath = "Tools/Alien Crusher/Validate Progression Save Transactions";
        private const string SaveFileName = "aliencrusher_progression.json";
        private const string BackupFileName = "aliencrusher_progression.bak.json";
        private const string CorruptFileName = "aliencrusher_progression.corrupt.json";

        [MenuItem(MenuPath)]
        public static void ValidateFromMenu()
        {
            Validate(writeReportFile: true, exitAfterRun: false);
        }

        public static void ValidateBatch()
        {
            Validate(writeReportFile: true, exitAfterRun: true);
        }

        private static void Validate(bool writeReportFile, bool exitAfterRun)
        {
            var report = new StringBuilder();
            var errors = 0;
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            var validationParent = Path.GetFullPath(Path.Combine(projectRoot, "Temp", "ProgressionSaveValidation"));
            var runRoot = Path.Combine(validationParent, Guid.NewGuid().ToString("N"));

            report.AppendLine("[AlienCrusher][ProgressionSaveTransactionValidation] Runtime save transaction checks");
            Directory.CreateDirectory(runRoot);

            try
            {
                RunCase("Atomic commit persists DP and unlock together", report, ref errors, () =>
                {
                    var caseRoot = PrepareCaseDirectory(runRoot, "atomic_commit");
                    var system = CreateSystem(caseRoot);
                    try
                    {
                        Require(system.TryCommit(data => data.meta.dpBalance = 1000), "Seed DP commit failed.");
                        Require(system.TryCommit(data =>
                        {
                            data.meta.dpBalance -= 450;
                            data.meta.unlockedForms.Add((int)FormType.Spike);
                            data.meta.selectedForm = (int)FormType.Spike;
                        }), "Combined purchase commit failed.");

                        Require(system.Current.meta.dpBalance == 550, "In-memory DP balance was not committed.");
                        Require(system.Current.meta.unlockedForms.Contains((int)FormType.Spike), "In-memory form unlock was not committed.");
                        var saved = ReadProgression(Path.Combine(caseRoot, SaveFileName));
                        Require(saved.meta.dpBalance == 550, "Persisted DP balance does not match the transaction.");
                        Require(saved.meta.unlockedForms.Contains((int)FormType.Spike), "Persisted form unlock is missing.");
                        Require(saved.meta.selectedForm == (int)FormType.Spike, "Persisted selected form is missing.");
                        Require(File.Exists(Path.Combine(caseRoot, BackupFileName)), "Atomic replacement did not create a backup.");
                    }
                    finally
                    {
                        DestroySystem(system);
                    }
                });

                RunCase("Failed save rolls back the complete in-memory mutation", report, ref errors, () =>
                {
                    var blockedRoot = Path.Combine(runRoot, "blocked_storage");
                    File.WriteAllText(blockedRoot, "This file intentionally blocks creation of a save directory.");
                    var system = CreateSystem(blockedRoot);
                    try
                    {
                        system.Current.meta.dpBalance = 900;
                        var committed = system.TryCommit(data =>
                        {
                            data.meta.dpBalance -= 300;
                            data.meta.unlockedForms.Add((int)FormType.Spike);
                            data.meta.selectedForm = (int)FormType.Spike;
                        });

                        Require(!committed, "Commit unexpectedly succeeded against blocked storage.");
                        Require(system.Current.meta.dpBalance == 900, "DP balance did not roll back after save failure.");
                        Require(!system.Current.meta.unlockedForms.Contains((int)FormType.Spike), "Unlock mutation did not roll back after save failure.");
                        Require(system.Current.meta.selectedForm == (int)FormType.Sphere, "Selected form did not roll back after save failure.");
                    }
                    finally
                    {
                        DestroySystem(system);
                    }
                });

                RunCase("Corrupt primary recovers from backup without consuming it", report, ref errors, () =>
                {
                    var caseRoot = PrepareCaseDirectory(runRoot, "backup_recovery");
                    var primaryPath = Path.Combine(caseRoot, SaveFileName);
                    var backupPath = Path.Combine(caseRoot, BackupFileName);
                    var corruptPath = Path.Combine(caseRoot, CorruptFileName);
                    const string corruptPrimary = "{ definitely-not-valid-json";
                    File.WriteAllText(primaryPath, corruptPrimary);

                    var knownGood = new PlayerProgressionData();
                    knownGood.meta.dpBalance = 777;
                    knownGood.meta.unlockedForms.Add((int)FormType.Spike);
                    knownGood.meta.selectedForm = (int)FormType.Spike;
                    File.WriteAllText(backupPath, JsonUtility.ToJson(knownGood, true));

                    var system = CreateSystem(caseRoot);
                    try
                    {
                        Require(system.Current.meta.dpBalance == 777, "Backup data was not loaded.");
                        Require(ReadProgression(primaryPath).meta.dpBalance == 777, "Primary save was not restored from backup.");
                        Require(ReadProgression(backupPath).meta.dpBalance == 777, "Known-good backup was overwritten during recovery.");
                        Require(File.Exists(corruptPath), "Corrupt primary was not preserved for diagnosis.");
                        Require(File.ReadAllText(corruptPath) == corruptPrimary, "Preserved corrupt primary content changed.");
                    }
                    finally
                    {
                        DestroySystem(system);
                    }
                });
            }
            finally
            {
                ProgressionSaveSystem.SetValidationStorageDirectory(null);
                DeleteValidationRunDirectory(runRoot, validationParent);
            }

            report.AppendLine($"Result: {errors} error(s), 0 warning(s)");
            var reportText = report.ToString();
            if (writeReportFile)
            {
                var reportPath = Path.Combine(projectRoot, "Logs", "AlienCrusherProgressionSaveTransactionValidation.log");
                Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);
                File.WriteAllText(reportPath, reportText);
            }

            if (errors > 0)
            {
                Debug.LogError(reportText);
            }
            else
            {
                Debug.Log(reportText);
            }

            if (exitAfterRun)
            {
                EditorApplication.Exit(errors > 0 ? 1 : 0);
            }
        }

        private static string PrepareCaseDirectory(string runRoot, string name)
        {
            var path = Path.Combine(runRoot, name);
            Directory.CreateDirectory(path);
            return path;
        }

        private static ProgressionSaveSystem CreateSystem(string storageDirectory)
        {
            ProgressionSaveSystem.SetValidationStorageDirectory(storageDirectory);
            var gameObject = new GameObject("ProgressionSaveTransactionValidator");
            return gameObject.AddComponent<ProgressionSaveSystem>();
        }

        private static void DestroySystem(ProgressionSaveSystem system)
        {
            if (system != null)
            {
                Object.DestroyImmediate(system.gameObject);
            }

            ProgressionSaveSystem.SetValidationStorageDirectory(null);
        }

        private static PlayerProgressionData ReadProgression(string path)
        {
            Require(File.Exists(path), $"Expected save file is missing: {path}");
            var data = JsonUtility.FromJson<PlayerProgressionData>(File.ReadAllText(path));
            Require(data != null, $"Expected valid progression JSON: {path}");
            return data;
        }

        private static void RunCase(string name, StringBuilder report, ref int errors, Action test)
        {
            try
            {
                test();
                report.AppendLine($"OK: {name}");
            }
            catch (Exception exception)
            {
                errors++;
                report.AppendLine($"ERROR: {name}: {exception.Message}");
            }
            finally
            {
                ProgressionSaveSystem.SetValidationStorageDirectory(null);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void DeleteValidationRunDirectory(string runRoot, string validationParent)
        {
            var fullRunRoot = Path.GetFullPath(runRoot);
            var fullValidationParent = Path.GetFullPath(validationParent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!fullRunRoot.StartsWith(fullValidationParent, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Refusing to delete validation path outside the expected root: {fullRunRoot}");
            }

            if (Directory.Exists(fullRunRoot))
            {
                Directory.Delete(fullRunRoot, recursive: true);
            }
        }
    }
}
#endif
