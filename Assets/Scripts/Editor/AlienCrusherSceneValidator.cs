#if UNITY_EDITOR
using System.IO;
using System.Text;
using AlienCrusher.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AlienCrusher.EditorTools
{
    public static class AlienCrusherSceneValidator
    {
        private const string MenuPath = "Tools/Alien Crusher/Validate Current Scene";

        [MenuItem(MenuPath)]
        public static void ValidateCurrentScene()
        {
            ValidateLoadedScene(showDialog: true, writeReportFile: true, exitAfterRun: false);
        }

        public static void ValidateCurrentSceneBatch()
        {
            OpenDefaultValidationSceneIfNeeded();
            ValidateLoadedScene(showDialog: false, writeReportFile: true, exitAfterRun: true);
        }

        private static void ValidateLoadedScene(bool showDialog, bool writeReportFile, bool exitAfterRun)
        {
            var report = new StringBuilder();
            var errors = 0;
            var warnings = 0;

            report.AppendLine("[AlienCrusher][Validation] Current scene check");
            CheckRequiredComponent<DummyFlowController>(report, ref errors, "DummyFlowController");
            CheckRequiredComponent<GameFlowSystem>(report, ref errors, "GameFlowSystem");
            CheckRequiredComponent<ScoreSystem>(report, ref errors, "ScoreSystem");
            CheckRequiredComponent<FeedbackSystem>(report, ref warnings, "FeedbackSystem");
            CheckRequiredComponent<DamageNumberSystem>(report, ref warnings, "DamageNumberSystem");
            CheckRequiredComponent<FormUnlockSystem>(report, ref warnings, "FormUnlockSystem");
            CheckRequiredComponent<CameraFollowSystem>(report, ref warnings, "CameraFollowSystem");

            var flow = Object.FindFirstObjectByType<DummyFlowController>();
            if (flow != null)
            {
                ValidateRouteHoldSettings(flow, report, ref warnings);
            }

            CheckSceneTransform("MapRoot", report, ref warnings, "Runtime fallback can create this, but route/playtest validation is stronger when it exists in scene.");
            CheckSceneTransform("TargetMarkers", report, ref warnings, "ROUTE HOLD beacon needs Target_A/Target_B under this root.");
            CheckSceneTransform("Target_A", report, ref warnings, "ROUTE HOLD needs at least one reachable target marker.");
            CheckSceneTransform("Target_B", report, ref warnings, "Second target marker improves route selection.");
            CheckSceneTransform("Canvas_Dummy", report, ref warnings, "HUD can be scaffolded/generated, but mobile UX validation needs it visible.");
            CheckSceneTransform("HUD_Dummy", report, ref warnings, "HUD panel should contain objective, route indicator, and gauge UI.");
            CheckSceneText("ObjectiveText", report, ref warnings);
            CheckSceneText("HudRouteIndicatorText", report, ref warnings);
            CheckSceneRouteArrow(report, ref warnings);

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
                    EditorUtility.DisplayDialog("Alien Crusher Validation", $"Scene validation found {errors} error(s) and {warnings} warning(s). See Console.", "OK");
                }
            }
            else if (warnings > 0)
            {
                Debug.LogWarning(report.ToString());
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Alien Crusher Validation", $"Scene validation passed with {warnings} warning(s). See Console.", "OK");
                }
            }
            else
            {
                Debug.Log(report.ToString());
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Alien Crusher Validation", "Scene validation passed.", "OK");
                }
            }

            if (exitAfterRun)
            {
                EditorApplication.Exit(errors > 0 ? 1 : 0);
            }
        }

        private static void ValidateRouteHoldSettings(DummyFlowController flow, StringBuilder report, ref int warnings)
        {
            var serialized = new SerializedObject(flow);
            CheckFloat(serialized, "routeHoldWindowSeconds", 5f, 120f, report, ref warnings);
            CheckFloat(serialized, "routeHoldProgressThreshold", 0.25f, 0.75f, report, ref warnings);
            CheckInt(serialized, "routeHoldBonusScore", 1, 10000, report, ref warnings);
            CheckFloat(serialized, "routeHoldCounterDuration", 0.1f, 10f, report, ref warnings);
            CheckInt(serialized, "routeHoldTrailPipCount", 3, 8, report, ref warnings);
            CheckFloat(serialized, "routeHoldTrailMaxDistance", 4f, 40f, report, ref warnings);
            CheckFloat(serialized, "routeHoldTrailMinPipSpacing", 0.75f, 4f, report, ref warnings);
            CheckFloat(serialized, "routeHoldTrailCloseHideDistance", 0.75f, 6f, report, ref warnings);
        }

        private static void CheckRequiredComponent<T>(StringBuilder report, ref int count, string label) where T : Object
        {
            if (Object.FindFirstObjectByType<T>() != null)
            {
                report.AppendLine($"OK: {label}");
                return;
            }

            count++;
            report.AppendLine($"MISSING: {label}");
        }

        private static void CheckSceneTransform(string objectName, StringBuilder report, ref int warnings, string detail)
        {
            if (FindSceneTransform(objectName) != null)
            {
                report.AppendLine($"OK: {objectName}");
                return;
            }

            warnings++;
            report.AppendLine($"WARN: Missing {objectName}. {detail}");
        }

        private static void CheckSceneText(string objectName, StringBuilder report, ref int warnings)
        {
            var transform = FindSceneTransform(objectName);
            if (transform != null && transform.GetComponent<Text>() != null)
            {
                report.AppendLine($"OK: {objectName} Text");
                return;
            }

            warnings++;
            report.AppendLine($"WARN: Missing Text binding for {objectName}.");
        }

        private static void CheckSceneRouteArrow(StringBuilder report, ref int warnings)
        {
            var arrow = FindSceneTransform("HudRouteArrow");
            if (arrow == null || arrow.GetComponent<RectTransform>() == null)
            {
                warnings++;
                report.AppendLine("WARN: Missing HudRouteArrow RectTransform.");
                return;
            }

            var arrowText = FindDirectChild(arrow, "ArrowText");
            if (arrowText == null || arrowText.GetComponent<Text>() == null)
            {
                warnings++;
                report.AppendLine("WARN: Missing HudRouteArrow/ArrowText Text binding.");
                return;
            }

            report.AppendLine("OK: HudRouteArrow");
        }

        private static void CheckFloat(SerializedObject serialized, string propertyName, float min, float max, StringBuilder report, ref int warnings)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                warnings++;
                report.AppendLine($"WARN: Missing serialized field {propertyName}.");
                return;
            }

            var value = property.floatValue;
            if (value < min || value > max)
            {
                warnings++;
                report.AppendLine($"WARN: {propertyName}={value:0.###} outside expected range {min:0.###}-{max:0.###}.");
                return;
            }

            report.AppendLine($"OK: {propertyName}={value:0.###}");
        }

        private static void CheckInt(SerializedObject serialized, string propertyName, int min, int max, StringBuilder report, ref int warnings)
        {
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                warnings++;
                report.AppendLine($"WARN: Missing serialized field {propertyName}.");
                return;
            }

            var value = property.intValue;
            if (value < min || value > max)
            {
                warnings++;
                report.AppendLine($"WARN: {propertyName}={value} outside expected range {min}-{max}.");
                return;
            }

            report.AppendLine($"OK: {propertyName}={value}");
        }

        private static Transform FindSceneTransform(string objectName)
        {
            var transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate == null || candidate.name != objectName || !candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child != null && child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private static void OpenDefaultValidationSceneIfNeeded()
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
                Debug.LogWarning($"[AlienCrusher][Validation] Could not open validation scene at {scenePath}.");
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
            File.WriteAllText(Path.Combine(logsPath, "AlienCrusherSceneValidation.log"), report);
        }
    }
}
#endif
