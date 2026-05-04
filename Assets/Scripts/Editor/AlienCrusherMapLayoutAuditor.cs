#if UNITY_EDITOR
using System.IO;
using System.Text;
using AlienCrusher.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.EditorTools
{
    public static class AlienCrusherMapLayoutAuditor
    {
        private const string MenuPath = "Tools/Alien Crusher/Audit Runtime Map Layout";
        private const string ReportFileName = "AlienCrusherMapLayoutAudit.log";

        [MenuItem(MenuPath)]
        public static void AuditCurrentScene()
        {
            AuditLoadedScene(showDialog: true, writeReportFile: true, exitAfterRun: false);
        }

        public static void AuditRuntimeMapLayoutBatch()
        {
            OpenDefaultSceneIfNeeded();
            AuditLoadedScene(showDialog: false, writeReportFile: true, exitAfterRun: true);
        }

        private static void AuditLoadedScene(bool showDialog, bool writeReportFile, bool exitAfterRun)
        {
            var report = new StringBuilder();
            var errors = 0;
            var warnings = 0;

            report.AppendLine("[AlienCrusher][MapLayoutAudit] Runtime stage map sweep");

            var flow = Object.FindFirstObjectByType<DummyFlowController>();
            if (flow == null)
            {
                errors++;
                report.AppendLine("ERROR: Missing DummyFlowController.");
            }
            else
            {
                var maxStage = ResolveMaxAuditStage(flow);
                report.AppendLine($"Range: Stage 01-{maxStage:00}");
                if (!flow.TryAuditRuntimeStageMapsForEditor(1, maxStage, report, out warnings))
                {
                    errors++;
                    report.AppendLine("ERROR: Runtime map audit did not run.");
                }
            }

            report.AppendLine($"Result: {errors} error(s), {warnings} warning(s)");

            if (writeReportFile)
            {
                WriteReportFile(report.ToString());
            }

            if (errors > 0)
            {
                Debug.LogError(report.ToString());
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Alien Crusher Map Layout Audit", $"Map layout audit found {errors} error(s) and {warnings} warning(s). See Console.", "OK");
                }
            }
            else if (warnings > 0)
            {
                Debug.LogWarning(report.ToString());
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Alien Crusher Map Layout Audit", $"Map layout audit found {warnings} warning(s). See Console.", "OK");
                }
            }
            else
            {
                Debug.Log(report.ToString());
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Alien Crusher Map Layout Audit", "Map layout audit passed.", "OK");
                }
            }

            if (exitAfterRun)
            {
                EditorApplication.Exit(errors > 0 || warnings > 0 ? 1 : 0);
            }
        }

        private static int ResolveMaxAuditStage(DummyFlowController flow)
        {
            var serialized = new SerializedObject(flow);
            var property = serialized.FindProperty("mapLayoutDebugMaxStage");
            if (property == null)
            {
                return 7;
            }

            return Mathf.Clamp(property.intValue, 1, 20);
        }

        private static void OpenDefaultSceneIfNeeded()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isLoaded && !string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            var scenePath = "Assets/Scenes/SampleScene.unity";
            var buildScenes = EditorBuildSettings.scenes;
            for (var i = 0; i < buildScenes.Length; i++)
            {
                if (buildScenes[i].enabled && !string.IsNullOrEmpty(buildScenes[i].path))
                {
                    scenePath = buildScenes[i].path;
                    break;
                }
            }

            if (File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                Debug.LogWarning($"[AlienCrusher][MapLayoutAudit] Could not open validation scene at {scenePath}.");
            }
        }

        private static void WriteReportFile(string report)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                return;
            }

            var logsPath = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logsPath);
            File.WriteAllText(Path.Combine(logsPath, ReportFileName), report);
        }
    }
}
#endif
