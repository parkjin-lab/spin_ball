#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AlienCrusher.EditorTools
{
    public static class AlienCrusherSceneRepair
    {
        private const string MenuPath = "Tools/Alien Crusher/Repair Scene Essentials";

        [MenuItem(MenuPath)]
        public static void RepairCurrentSceneEssentials()
        {
            RepairLoadedScene(saveScene: true, exitAfterRun: false);
        }

        public static void RepairCurrentSceneEssentialsBatch()
        {
            OpenDefaultSceneIfNeeded();
            RepairLoadedScene(saveScene: true, exitAfterRun: true);
        }

        private static void RepairLoadedScene(bool saveScene, bool exitAfterRun)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var repairedCount = 0;

            if (EnsureHudRouteIndicatorText())
            {
                repairedCount++;
            }

            if (EnsureHudRouteArrow())
            {
                repairedCount++;
            }

            if (saveScene && scene.IsValid() && !string.IsNullOrEmpty(scene.path))
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log($"[AlienCrusher][Repair] Scene essentials repaired: {repairedCount}");

            if (exitAfterRun)
            {
                EditorApplication.Exit(0);
            }
        }

        private static bool EnsureHudRouteIndicatorText()
        {
            var hud = FindSceneTransform("HUD_Dummy");
            if (hud == null)
            {
                Debug.LogWarning("[AlienCrusher][Repair] HUD_Dummy not found; route indicator text was not created.");
                return false;
            }

            var existing = FindSceneTransform("HudRouteIndicatorText");
            if (existing != null && existing.GetComponent<Text>() != null)
            {
                return false;
            }

            var routeTextObject = existing != null
                ? existing.gameObject
                : new GameObject("HudRouteIndicatorText", typeof(RectTransform));

            routeTextObject.transform.SetParent(hud, false);
            var rect = routeTextObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = routeTextObject.AddComponent<RectTransform>();
            }

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -74f);
            rect.sizeDelta = new Vector2(720f, 42f);

            var text = routeTextObject.GetComponent<Text>();
            if (text == null)
            {
                text = routeTextObject.AddComponent<Text>();
            }

            text.text = string.Empty;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = new Color(1f, 0.92f, 0.7f, 1f);
            text.raycastTarget = false;
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return true;
        }

        private static bool EnsureHudRouteArrow()
        {
            var hud = FindSceneTransform("HUD_Dummy");
            if (hud == null)
            {
                Debug.LogWarning("[AlienCrusher][Repair] HUD_Dummy not found; route arrow was not created.");
                return false;
            }

            var repaired = false;
            var routeArrow = FindSceneTransform("HudRouteArrow");
            var routeArrowObject = routeArrow != null
                ? routeArrow.gameObject
                : new GameObject("HudRouteArrow", typeof(RectTransform));

            if (routeArrow == null)
            {
                routeArrow = routeArrowObject.transform;
                repaired = true;
            }

            routeArrow.SetParent(hud, false);
            var rect = routeArrowObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = routeArrowObject.AddComponent<RectTransform>();
                repaired = true;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(84f, 84f);
            routeArrowObject.SetActive(false);

            var arrowText = FindDirectChild(routeArrow, "ArrowText");
            var arrowTextObject = arrowText != null
                ? arrowText.gameObject
                : new GameObject("ArrowText", typeof(RectTransform));

            if (arrowText == null)
            {
                arrowText = arrowTextObject.transform;
                repaired = true;
            }

            arrowText.SetParent(routeArrow, false);
            var arrowRect = arrowTextObject.GetComponent<RectTransform>();
            if (arrowRect == null)
            {
                arrowRect = arrowTextObject.AddComponent<RectTransform>();
                repaired = true;
            }

            StretchRect(arrowRect);
            var text = arrowTextObject.GetComponent<Text>();
            if (text == null)
            {
                text = arrowTextObject.AddComponent<Text>();
                repaired = true;
            }

            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 46;
            text.fontStyle = FontStyle.Bold;
            text.text = "^";
            text.color = new Color(1f, 0.9f, 0.54f, 1f);
            text.raycastTarget = false;
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return repaired;
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

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
        }
    }
}
#endif
