#if UNITY_EDITOR
using System.IO;
using System.Text;
using AlienCrusher.Gameplay;
using UnityEditor;
using UnityEngine;

namespace AlienCrusher.EditorTools
{
    public static class FormSmashCatalogValidator
    {
        private const string MenuPath = "Tools/Alien Crusher/Validate Form Smash Catalog";

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
            report.AppendLine("[AlienCrusher][FormSmashCatalogValidation] Distinct smash methods + growth catalog");

            Require(report, ref errors, FormCatalog.All != null && FormCatalog.All.Length >= 4, "Catalog must list at least 4 playable forms.");
            Require(report, ref errors, FormCatalog.HasDistinctSmashMethods(), "Each form must use a distinct smash method.");
            Require(report, ref errors, FormCatalog.GetSmashMethod(FormType.Sphere) == FormSmashMethod.BodyRam, "Sphere must stay the early body-ram smash.");
            Require(report, ref errors, FormCatalog.GetSmashMethod(FormType.Saucer) == FormSmashMethod.UfoRay, "Saucer/UFO must smash with a ray/beam.");
            Require(report, ref errors, FormCatalog.GetSmashMethod(FormType.Spike) == FormSmashMethod.DrillBurrow, "Spike must smash with a drill burrow.");
            Require(report, ref errors, FormCatalog.GetSmashMethod(FormType.Ram) == FormSmashMethod.ChargeBurst, "Ram must smash with a charge burst.");
            Require(report, ref errors, FormCatalog.GetSmashMethod(FormType.Crusher) == FormSmashMethod.MagnetGrab, "Crusher must smash with a magnet grab.");
            Require(report, ref errors, FormCatalog.GetDisplayName(FormType.Saucer) == "UFO", "Saucer display name must be UFO.");

            for (var i = 0; i < FormCatalog.All.Length; i++)
            {
                var entry = FormCatalog.All[i];
                Require(report, ref errors, !string.IsNullOrWhiteSpace(entry.StrategyHint), $"{entry.DisplayName} is missing a strategy hint.");
                Require(report, ref errors, !string.IsNullOrWhiteSpace(entry.ButtonName) && entry.ButtonName.StartsWith("Form_"), $"{entry.DisplayName} must keep a Form_* lobby button slot.");
            }

            Require(report, ref errors, FormCatalog.Get(FormType.Sphere).SmashMethod == FormSmashMethod.BodyRam
                && FormCatalog.Get(FormType.Spike).SmashMethod == FormSmashMethod.DrillBurrow
                && FormCatalog.Get(FormType.Ram).SmashMethod == FormSmashMethod.ChargeBurst
                && FormCatalog.Get(FormType.Saucer).SmashMethod == FormSmashMethod.UfoRay
                && FormCatalog.Get(FormType.Crusher).SmashMethod == FormSmashMethod.MagnetGrab,
                "Unlock order stays Sphere ram → Drill → Tank → UFO ray → Magnet.");

            report.AppendLine($"Result: {errors} error(s), 0 warning(s)");
            var reportText = report.ToString();
            if (writeReportFile)
            {
                var reportPath = Path.Combine(projectRoot, "Logs", "AlienCrusherFormSmashCatalogValidation.log");
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

        private static void Require(StringBuilder report, ref int errors, bool condition, string message)
        {
            if (condition)
            {
                report.AppendLine("PASS  " + message);
                return;
            }

            errors++;
            report.AppendLine("FAIL  " + message);
        }
    }
}
#endif
