using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace AlienCrusher.Systems
{
    [DisallowMultipleComponent]
    public class DamageNumberSystem : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private int poolSize = 32;

        [Header("Motion")]
        [SerializeField] private float riseDistance = 1.8f;
        [SerializeField] private float duration = 0.62f;
        [SerializeField] private float spreadRadius = 0.45f;

        [Header("Style")]
        [SerializeField] private Color normalColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color heavyColor = new Color(1f, 0.62f, 0.26f, 1f);
        [SerializeField] private Color destroyColor = new Color(1f, 0.42f, 0.22f, 1f);
        [SerializeField] private Color overdriveColor = new Color(1f, 0.34f, 0.12f, 1f);
        [SerializeField] private Color overdriveDestroyColor = new Color(1f, 0.14f, 0.06f, 1f);
        [SerializeField] private Color tagColor = new Color(0.42f, 0.9f, 1f, 1f);
        [SerializeField] private Color tagEmphasisColor = new Color(0.9f, 0.98f, 1f, 1f);
        [SerializeField] private Color retailTagColor = new Color(0.26f, 0.88f, 1f, 1f);
        [SerializeField] private Color retailTagEmphasisColor = new Color(1f, 0.82f, 0.32f, 1f);
        [SerializeField] private Color propTagColor = new Color(0.84f, 0.98f, 0.92f, 1f);
        [SerializeField] private Color propVehicleTagColor = new Color(1f, 0.86f, 0.42f, 1f);
        [SerializeField] private Color propVolatileTagColor = new Color(1f, 0.58f, 0.28f, 1f);

        private readonly List<Item> items = new List<Item>(64);
        private int cursor;
        private Camera targetCamera;
        private Font damageFont;
        private bool overdriveMode;

        private sealed class Item
        {
            public Transform transform;
            public TextMesh textMesh;
            public Renderer renderer;
            public Tween moveTween;
            public Tween scaleTween;
            public Tween alphaTween;
            public float endTime;
        }

        private void Awake()
        {
            targetCamera = Camera.main;
            BuildPool();
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    return;
                }
            }

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || item.transform == null || !item.transform.gameObject.activeSelf)
                {
                    continue;
                }

                if (Time.unscaledTime >= item.endTime)
                {
                    item.transform.gameObject.SetActive(false);
                    continue;
                }

                var direction = item.transform.position - targetCamera.transform.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    item.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }
            }
        }

        public void SetOverdriveMode(bool active)
        {
            overdriveMode = active;
        }

        public void ShowDamage(Vector3 worldPosition, float damage, bool heavy, bool destroyed)
        {
            if (poolSize <= 0)
            {
                return;
            }

            if (items.Count == 0)
            {
                BuildPool();
            }

            if (items.Count == 0)
            {
                return;
            }

            var item = items[cursor];
            cursor = (cursor + 1) % items.Count;
            if (item == null || item.transform == null || item.textMesh == null)
            {
                return;
            }

            item.moveTween?.Kill();
            item.scaleTween?.Kill();
            item.alphaTween?.Kill();

            var horizontal = Random.insideUnitCircle * spreadRadius;
            var startPos = worldPosition + new Vector3(horizontal.x, 0.65f, horizontal.y);
            var endPos = startPos + Vector3.up * riseDistance;

            var rounded = Mathf.Max(1, Mathf.RoundToInt(damage));
            item.textMesh.text = overdriveMode ? $"-{rounded}!" : $"-{rounded}";
            item.textMesh.characterSize = destroyed ? 0.11f : 0.085f;
            item.textMesh.fontSize = destroyed ? 96 : (heavy ? 82 : 72);
            item.textMesh.fontStyle = destroyed || heavy ? FontStyle.Bold : FontStyle.Normal;

            var color = destroyed ? destroyColor : (heavy ? heavyColor : normalColor);
            if (overdriveMode)
            {
                color = destroyed
                    ? overdriveDestroyColor
                    : (heavy ? Color.Lerp(overdriveColor, overdriveDestroyColor, 0.35f) : overdriveColor);
            }

            item.textMesh.color = color;

            item.transform.position = startPos;
            var baseScale = destroyed ? 0.92f : (heavy ? 0.8f : 0.72f);
            if (overdriveMode)
            {
                baseScale *= 1.1f;
            }

            item.transform.localScale = Vector3.one * baseScale;
            item.transform.gameObject.SetActive(true);
            item.endTime = Time.unscaledTime + duration + 0.05f;

            item.moveTween = item.transform
                .DOMove(endPos, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            item.scaleTween = item.transform
                .DOScale(item.transform.localScale * 1.18f, 0.12f)
                .SetEase(Ease.OutBack)
                .SetLoops(2, LoopType.Yoyo)
                .SetUpdate(true);

            item.alphaTween = DOTween.To(
                    () => item.textMesh.color,
                    c => item.textMesh.color = c,
                    new Color(color.r, color.g, color.b, 0f),
                    duration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (item != null && item.transform != null)
                    {
                        item.transform.gameObject.SetActive(false);
                    }
                });
        }


        public void ShowTag(Vector3 worldPosition, string text, bool emphasis)
        {
            ShowTagInternal(worldPosition, text, emphasis, emphasis ? tagEmphasisColor : tagColor);
        }

        public void ShowRetailTag(Vector3 worldPosition, string text, bool emphasis)
        {
            ShowTagInternal(worldPosition, text, emphasis, emphasis ? retailTagEmphasisColor : retailTagColor);
        }

        public void ShowPropTag(Vector3 worldPosition, string text, bool emphasis, bool vehicle, bool volatileProp)
        {
            var color = volatileProp
                ? propVolatileTagColor
                : (vehicle ? propVehicleTagColor : propTagColor);
            ShowTagInternal(worldPosition, text, emphasis, color);
        }

        private void ShowTagInternal(Vector3 worldPosition, string text, bool emphasis, Color color)
        {
            if (poolSize <= 0 || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (items.Count == 0)
            {
                BuildPool();
            }

            if (items.Count == 0)
            {
                return;
            }

            var item = items[cursor];
            cursor = (cursor + 1) % items.Count;
            if (item == null || item.transform == null || item.textMesh == null)
            {
                return;
            }

            item.moveTween?.Kill();
            item.scaleTween?.Kill();
            item.alphaTween?.Kill();

            var horizontal = Random.insideUnitCircle * (spreadRadius * 0.6f);
            var startPos = worldPosition + new Vector3(horizontal.x, 0.82f, horizontal.y);
            var endPos = startPos + Vector3.up * (riseDistance * 0.72f);

            item.textMesh.text = text.Trim();
            item.textMesh.characterSize = emphasis ? 0.096f : 0.082f;
            item.textMesh.fontSize = emphasis ? 86 : 72;
            item.textMesh.fontStyle = FontStyle.Bold;
            item.textMesh.color = color;

            item.transform.position = startPos;
            var baseScale = emphasis ? 0.9f : 0.76f;
            item.transform.localScale = Vector3.one * baseScale;
            item.transform.gameObject.SetActive(true);
            item.endTime = Time.unscaledTime + duration + 0.08f;

            item.moveTween = item.transform
                .DOMove(endPos, duration * 0.86f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            item.scaleTween = item.transform
                .DOScale(item.transform.localScale * 1.14f, 0.11f)
                .SetEase(Ease.OutBack)
                .SetLoops(2, LoopType.Yoyo)
                .SetUpdate(true);

            item.alphaTween = DOTween.To(
                    () => item.textMesh.color,
                    c => item.textMesh.color = c,
                    new Color(color.r, color.g, color.b, 0f),
                    duration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (item != null && item.transform != null)
                    {
                        item.transform.gameObject.SetActive(false);
                    }
                });
        }

        private void BuildPool()
        {
            if (items.Count > 0)
            {
                return;
            }

            damageFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var root = new GameObject("_DamageNumberPool").transform;
            root.SetParent(transform, false);

            for (var i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"DamageNumber_{i:00}");
                go.transform.SetParent(root, false);
                go.SetActive(false);

                var text = go.AddComponent<TextMesh>();
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.richText = false;
                text.text = "-0";
                text.color = normalColor;
                text.characterSize = 0.085f;
                text.fontSize = 72;
                text.fontStyle = FontStyle.Bold;
                text.font = damageFont;

                var item = new Item
                {
                    transform = go.transform,
                    textMesh = text,
                    renderer = go.GetComponent<Renderer>()
                };

                items.Add(item);
            }
        }
    }
}




