#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using AlienCrusher.Systems;
using UnityEditor;
using UnityEngine;

namespace AlienCrusher.EditorTools
{
    public static class TimeScaleChannelOverlapValidator
    {
        private const string MenuPath = "Tools/Alien Crusher/Validate Time Scale Channel Overlap";

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

            report.AppendLine("[AlienCrusher][TimeScaleChannelOverlapValidation] Time channel overlap policy");
            CheckState(report, ref errors, "No active channel", false, false, 1f, false, 1f, 1f, 0.02f);
            CheckState(report, ref errors, "Overdrive only", false, true, 0.72f, false, 1f, 0.72f, 0.0144f);
            CheckState(report, ref errors, "Boss finish only", false, false, 1f, true, 0.25f, 0.25f, 0.005f);
            CheckState(report, ref errors, "Slower active channel wins", false, true, 0.72f, true, 0.25f, 0.25f, 0.005f);
            CheckState(report, ref errors, "Pause overrides both effects", true, true, 0.72f, true, 0.25f, 0f, 0.02f);
            CheckState(report, ref errors, "Releasing pause preserves both effects", false, true, 0.72f, true, 0.25f, 0.25f, 0.005f);
            CheckState(report, ref errors, "Releasing boss preserves overdrive", false, true, 0.72f, false, 0.25f, 0.72f, 0.0144f);
            CheckState(report, ref errors, "Releasing every channel restores baseline", false, false, 0.72f, false, 0.25f, 1f, 0.02f);
            CheckState(report, ref errors, "Requested scales are clamped", false, true, 2f, true, 0.001f, 0.01f, 0.0002f);

            report.AppendLine($"Result: {errors} error(s), 0 warning(s)");
            var reportText = report.ToString();
            if (writeReportFile)
            {
                var reportPath = Path.Combine(projectRoot, "Logs", "AlienCrusherTimeScaleChannelOverlapValidation.log");
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

        private static void CheckState(
            StringBuilder report,
            ref int errors,
            string label,
            bool pauseActive,
            bool overdriveActive,
            float overdriveScale,
            bool bossFinishActive,
            float bossFinishScale,
            float expectedScale,
            float expectedFixedDeltaTime)
        {
            var resolved = DummyFlowController.ResolveTimeStateForValidation(
                pauseActive,
                overdriveActive,
                overdriveScale,
                bossFinishActive,
                bossFinishScale);

            if (Approximately(resolved.x, expectedScale) && Approximately(resolved.y, expectedFixedDeltaTime))
            {
                report.AppendLine($"OK: {label}: scale={resolved.x:0.####}, fixedDelta={resolved.y:0.####}");
                return;
            }

            errors++;
            report.AppendLine(
                $"ERROR: {label}: expected scale={expectedScale:0.####}, fixedDelta={expectedFixedDeltaTime:0.####}; " +
                $"actual scale={resolved.x:0.####}, fixedDelta={resolved.y:0.####}");
        }

        private static bool Approximately(float left, float right)
        {
            return Math.Abs(left - right) <= 0.00001f;
        }
    }
}
#endif
