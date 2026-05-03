using UnityEngine;
using UnityEngine.UI;

namespace AlienCrusher.UI
{
    [DisallowMultipleComponent]
    public class MobileCanvasAdaptor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private RectTransform safeAreaRoot;

        [Header("Portrait")]
        [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);
        [SerializeField] [Range(0f, 1f)] private float portraitMatch = 0.5f;

        [Header("Landscape")]
        [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] [Range(0f, 1f)] private float landscapeMatch = 0.5f;

        private Vector2Int lastScreenSize;
        private Rect lastSafeArea;

        private void Awake()
        {
            if (canvasScaler == null)
            {
                canvasScaler = GetComponent<CanvasScaler>();
            }

            Apply(force: true);
        }

        private void OnEnable()
        {
            Apply(force: true);
        }

        private void Update()
        {
            Apply(force: false);
        }

        public void Configure(CanvasScaler scaler, RectTransform safeRoot)
        {
            canvasScaler = scaler;
            safeAreaRoot = safeRoot;
            Apply(force: true);
        }

        private void Apply(bool force)
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            var safeArea = Screen.safeArea;

            if (!force && screenSize == lastScreenSize && safeArea.Equals(lastSafeArea))
            {
                return;
            }

            var isPortrait = Screen.height >= Screen.width;
            ApplyScaler(isPortrait);
            ApplySafeArea();

            lastScreenSize = screenSize;
            lastSafeArea = safeArea;
        }

        private void ApplyScaler(bool isPortrait)
        {
            if (canvasScaler == null)
            {
                return;
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referenceResolution = isPortrait ? portraitReferenceResolution : landscapeReferenceResolution;
            canvasScaler.matchWidthOrHeight = isPortrait ? portraitMatch : landscapeMatch;
        }

        private void ApplySafeArea()
        {
            if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var min = safeArea.position;
            var max = safeArea.position + safeArea.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            safeAreaRoot.anchorMin = min;
            safeAreaRoot.anchorMax = max;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
        }
    }
}
