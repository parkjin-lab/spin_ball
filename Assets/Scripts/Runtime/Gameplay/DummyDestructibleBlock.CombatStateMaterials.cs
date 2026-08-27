using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Gameplay
{
    public partial class DummyDestructibleBlock
    {
        private const string DamageCrackOverlayId = "MAT_Damage_CrackOverlay";
        private const string WeakPointGlowId = "MAT_WeakPoint_Glow";
        private const string ShieldedPylonId = "MAT_Shielded_Pylon";
        private const string ExposedCoreId = "MAT_Exposed_Core";
        private const string DamageCrackOverlayResourcesPath = "Materials/Destruction/MAT_Damage_CrackOverlay";
        private const string WeakPointGlowResourcesPath = "Materials/Destruction/MAT_WeakPoint_Glow";
        private const string ShieldedPylonResourcesPath = "Materials/Destruction/MAT_Shielded_Pylon";
        private const string ExposedCoreResourcesPath = "Materials/Destruction/MAT_Exposed_Core";
        private const string DamageCrackOverlayAssetPath = "Assets/Art/Materials/Destruction/MAT_Damage_CrackOverlay.mat";
        private const string WeakPointGlowAssetPath = "Assets/Art/Materials/Destruction/MAT_WeakPoint_Glow.mat";
        private const string ShieldedPylonAssetPath = "Assets/Art/Materials/Destruction/MAT_Shielded_Pylon.mat";
        private const string ExposedCoreAssetPath = "Assets/Art/Materials/Destruction/MAT_Exposed_Core.mat";
        private const string CombatCrackRootName = "_CombatCrackRoot";
        private const string WeakPointHaloName = "_WeakPointHalo";
        private const string ShieldBarrierName = "ShieldBarrier";
        private const string SentinelKitName = "BOSS_Sentinel_Body_Kit";
        private const string ShieldPylonKitName = "BOSS_Shield_Pylon_Kit";

        private static readonly Color WeakPointGlowColor = new Color(1f, 0.84f, 0.14f, 1f);
        private static readonly Color WeakPointGlowEmission = new Color(0.72f, 0.42f, 0.04f, 1f);
        private static readonly Color ShieldedPylonColor = new Color(0.16f, 0.78f, 0.94f, 1f);
        private static readonly Color ShieldedPylonEmission = new Color(0.06f, 0.38f, 0.58f, 1f);
        private static readonly Color ExposedCoreColor = new Color(1f, 0.4f, 0.08f, 1f);
        private static readonly Color ExposedCoreEmission = new Color(0.95f, 0.32f, 0.04f, 1f);
        private static readonly Color ClosedCoreColor = new Color(0.72f, 0.94f, 1f, 1f);
        private static readonly Color CrackOverlayColor = new Color(0.07f, 0.04f, 0.03f, 1f);
        private static readonly Color CrackOverlayEmission = new Color(0.2f, 0.05f, 0.02f, 1f);

        private static Material damageCrackOverlayMaterial;
        private static Material weakPointGlowMaterial;
        private static Material shieldedPylonMaterial;
        private static Material exposedCoreMaterial;

        private Transform combatCrackRoot;
        private readonly System.Collections.Generic.List<Transform> combatCrackPieces = new System.Collections.Generic.List<Transform>(8);
        private bool combatCracksBuilt;
        private Transform weakPointHalo;
        private Renderer weakPointHaloRenderer;
        private MaterialPropertyBlock combatStatePropertyBlock;
        private bool lastCombatPylonActive;
        private bool lastCombatExposureActive;
        private bool combatPylonStateApplied;
        private bool combatExposureStateApplied;
        private int lastCombatCrackCount = -1;

        private MaterialPropertyBlock EnsureCombatStatePropertyBlock()
        {
            return combatStatePropertyBlock ??= new MaterialPropertyBlock();
        }

        private void RefreshCombatStateReadability()
        {
            EnsureCombatStatePropertyBlock();
            EnsureCombatStateMaterials();
            ApplyWeakPointCombatMaterial();

            bool pylon = HasActiveNamedKitOnSelf(ShieldPylonKitName);
            if (!combatPylonStateApplied || pylon != lastCombatPylonActive)
            {
                ApplyShieldedPylonCombatMaterial();
                lastCombatPylonActive = pylon;
                combatPylonStateApplied = true;
            }

            if (!combatExposureStateApplied || bossCoreExposureActive != lastCombatExposureActive)
            {
                ApplyExposedCoreCombatMaterial();
                lastCombatExposureActive = bossCoreExposureActive;
                combatExposureStateApplied = true;
            }

            RefreshDamageCrackOverlay();
        }

        private void ApplyWeakPointCombatMaterial()
        {
            if (weakPointRenderer == null)
            {
                return;
            }

            bool pylon = HasActiveNamedKitOnSelf(ShieldPylonKitName);
            Material material;
            Color color;
            Color emission;
            if (pylon)
            {
                material = shieldedPylonMaterial;
                color = ShieldedPylonColor;
                emission = ShieldedPylonEmission;
                SetWeakPointHaloVisible(false);
            }
            else if (bossCoreExposureActive)
            {
                material = exposedCoreMaterial;
                color = ExposedCoreColor;
                emission = ExposedCoreEmission;
                SetWeakPointHaloVisible(false);
            }
            else
            {
                material = weakPointGlowMaterial;
                color = WeakPointGlowColor;
                emission = WeakPointGlowEmission;
                EnsureWeakPointHalo();
                SetWeakPointHaloVisible(weakPointActive);
            }

            AssignNamedMaterial(weakPointRenderer, material);
            ApplyEmissiveBlock(weakPointRenderer, weakPointPropertyBlock, color, emission);
            if (weakPointHaloRenderer != null && weakPointHalo != null && weakPointHalo.gameObject.activeSelf)
            {
                AssignNamedMaterial(weakPointHaloRenderer, weakPointGlowMaterial);
                ApplyEmissiveBlock(weakPointHaloRenderer, EnsureCombatStatePropertyBlock(), WeakPointGlowColor, WeakPointGlowEmission);
            }
        }

        private void EnsureWeakPointHalo()
        {
            if (weakPointVisual == null)
            {
                return;
            }

            if (weakPointHalo == null)
            {
                Transform existing = weakPointVisual.Find(WeakPointHaloName);
                if (existing == null)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.name = WeakPointHaloName;
                    existing = go.transform;
                    existing.SetParent(weakPointVisual, false);
                    RemoveCollider(go);
                }

                weakPointHalo = existing;
                weakPointHalo.localPosition = Vector3.zero;
                weakPointHalo.localRotation = Quaternion.identity;
                weakPointHalo.localScale = new Vector3(1.7f, 0.1f, 1.7f);
                weakPointHaloRenderer = weakPointHalo.GetComponent<Renderer>();
            }
        }

        private void SetWeakPointHaloVisible(bool visible)
        {
            if (weakPointHalo == null)
            {
                if (!visible)
                {
                    return;
                }

                EnsureWeakPointHalo();
            }

            if (weakPointHalo != null && weakPointHalo.gameObject.activeSelf != visible)
            {
                weakPointHalo.gameObject.SetActive(visible);
            }
        }

        private void ApplyShieldedPylonCombatMaterial()
        {
            Transform kit = FindDirectChild(transform, ShieldPylonKitName);
            if (kit == null || !kit.gameObject.activeSelf)
            {
                Transform leftover = FindDirectChild(transform, ShieldPylonKitName);
                if (leftover != null)
                {
                    Transform barrier = leftover.Find(ShieldBarrierName);
                    if (barrier != null && barrier.gameObject.activeSelf)
                    {
                        barrier.gameObject.SetActive(false);
                    }
                }

                return;
            }

            ApplyNamedMaterialToChild(kit, "EnergyPaneAccent", shieldedPylonMaterial, ShieldedPylonColor, ShieldedPylonEmission);
            ApplyNamedMaterialToChild(kit, "Cap_L", shieldedPylonMaterial, ShieldedPylonColor, ShieldedPylonEmission);
            ApplyNamedMaterialToChild(kit, "Cap_R", shieldedPylonMaterial, ShieldedPylonColor, ShieldedPylonEmission);
            EnsureShieldBarrier(kit);
        }

        private void EnsureShieldBarrier(Transform kit)
        {
            Transform pane = kit.Find("EnergyPaneAccent");
            Transform barrier = kit.Find(ShieldBarrierName);
            if (barrier == null)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = ShieldBarrierName;
                barrier = go.transform;
                barrier.SetParent(kit, false);
                RemoveCollider(go);
            }

            if (pane != null)
            {
                barrier.localPosition = pane.localPosition;
                barrier.localRotation = pane.localRotation;
                Vector3 paneScale = pane.localScale;
                barrier.localScale = new Vector3(paneScale.x * 1.12f, paneScale.y * 1.08f, Mathf.Max(0.12f, paneScale.z * 1.8f));
            }
            else
            {
                barrier.localPosition = Vector3.zero;
                barrier.localRotation = Quaternion.identity;
                barrier.localScale = new Vector3(1.1f, 1.4f, 0.14f);
            }

            if (!barrier.gameObject.activeSelf)
            {
                barrier.gameObject.SetActive(true);
            }

            Renderer renderer = barrier.GetComponent<Renderer>();
            AssignNamedMaterial(renderer, shieldedPylonMaterial);
            ApplyEmissiveBlock(renderer, EnsureCombatStatePropertyBlock(), ShieldedPylonColor, ShieldedPylonEmission);
        }

        private void ApplyExposedCoreCombatMaterial()
        {
            Transform kit = FindDirectChild(transform, SentinelKitName);
            if (kit != null && kit.gameObject.activeSelf)
            {
                if (bossCoreExposureActive)
                {
                    ApplyNamedMaterialToChild(kit, "ChestCore", exposedCoreMaterial, ExposedCoreColor, ExposedCoreEmission);
                    ApplyNamedMaterialToChild(kit, "VisorAccent", exposedCoreMaterial, Color.Lerp(ExposedCoreColor, Color.white, 0.35f), ExposedCoreEmission);
                }
                else
                {
                    ApplyNamedMaterialToChild(kit, "ChestCore", null, ClosedCoreColor, new Color(0.12f, 0.22f, 0.3f, 1f));
                    ApplyNamedMaterialToChild(kit, "VisorAccent", null, new Color(0.78f, 0.9f, 1f, 1f), new Color(0.1f, 0.18f, 0.28f, 1f));
                }
            }

            if (bossCoreRingRenderer != null)
            {
                AssignNamedMaterial(bossCoreRingRenderer, exposedCoreMaterial);
            }

            if (bossCoreGroundTelegraphRenderer != null)
            {
                AssignNamedMaterial(bossCoreGroundTelegraphRenderer, exposedCoreMaterial);
            }
        }

        private void RefreshDamageCrackOverlay()
        {
            float remainingRatio = maxDurability > 0f ? Mathf.Clamp01(currentDurability / maxDurability) : 0f;
            float brokenRatio = 1f - remainingRatio;
            if (currentDurability <= 0f || brokenRatio < 0.22f)
            {
                if (lastCombatCrackCount != 0)
                {
                    SetCombatCrackCount(0);
                }

                return;
            }

            EnsureCombatCrackOverlay();
            int maxPieces = combatCrackPieces.Count;
            if (maxPieces <= 0)
            {
                return;
            }

            float t = Mathf.InverseLerp(0.22f, 0.88f, brokenRatio);
            int count = Mathf.Clamp(Mathf.RoundToInt(t * maxPieces), 1, maxPieces);
            if (count == lastCombatCrackCount)
            {
                return;
            }

            SetCombatCrackCount(count);
        }

        private void EnsureCombatCrackOverlay()
        {
            if (combatCracksBuilt)
            {
                return;
            }

            combatCracksBuilt = true;
            combatCrackPieces.Clear();

            Transform root = transform.Find(CombatCrackRootName);
            if (root == null)
            {
                GameObject rootGo = new GameObject(CombatCrackRootName);
                root = rootGo.transform;
                root.SetParent(transform, false);
            }

            combatCrackRoot = root;
            combatCrackRoot.localPosition = Vector3.zero;
            combatCrackRoot.localRotation = Quaternion.identity;
            combatCrackRoot.localScale = Vector3.one;

            Vector3 size = SanitizeScale(initialScale);
            Vector3 half = size * 0.5f;
            float thickness = isLargeBuilding ? 0.1f : 0.08f;
            float length = Mathf.Max(0.28f, Mathf.Min(size.x, size.z) * (isLargeBuilding ? 0.55f : 0.42f));
            float width = Mathf.Max(0.08f, Mathf.Min(size.x, size.z) * 0.08f);
            int pieceCount = isLargeBuilding ? 8 : (IsSmallBuildingTier() ? 3 : 5);

            for (int i = 0; i < pieceCount; i++)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "CombatCrack_" + i.ToString("00");
                go.transform.SetParent(combatCrackRoot, false);
                RemoveCollider(go);

                Vector3 localScale;
                Vector3 localPosition;
                Quaternion localRotation;
                switch (i % 6)
                {
                    case 0:
                        localPosition = new Vector3(half.x + thickness * 0.35f, 0.1f, 0f);
                        localRotation = Quaternion.Euler(0f, 90f, 18f);
                        localScale = new Vector3(thickness, width, length);
                        break;
                    case 1:
                        localPosition = new Vector3(-half.x - thickness * 0.35f, -0.08f, 0.05f);
                        localRotation = Quaternion.Euler(0f, 90f, -22f);
                        localScale = new Vector3(thickness, width, length);
                        break;
                    case 2:
                        localPosition = new Vector3(0.08f, 0.04f, half.z + thickness * 0.35f);
                        localRotation = Quaternion.Euler(0f, 0f, 12f);
                        localScale = new Vector3(width, thickness, length);
                        break;
                    case 3:
                        localPosition = new Vector3(-0.06f, 0.12f, -half.z - thickness * 0.35f);
                        localRotation = Quaternion.Euler(0f, 0f, -16f);
                        localScale = new Vector3(width, thickness, length);
                        break;
                    case 4:
                        localPosition = new Vector3(0.04f, half.y + thickness * 0.35f, 0.06f);
                        localRotation = Quaternion.Euler(90f, 22f, 0f);
                        localScale = new Vector3(width, thickness, length * 0.85f);
                        break;
                    default:
                        localPosition = new Vector3(half.x * 0.35f, -half.y * 0.1f, half.z + thickness * 0.35f);
                        localRotation = Quaternion.Euler(0f, 28f, 8f);
                        localScale = new Vector3(width, thickness, length * 0.7f);
                        break;
                }

                go.transform.localPosition = localPosition;
                go.transform.localRotation = localRotation;
                go.transform.localScale = new Vector3(
                    Mathf.Max(0.08f, Mathf.Abs(localScale.x)),
                    Mathf.Max(0.08f, Mathf.Abs(localScale.y)),
                    Mathf.Max(0.08f, Mathf.Abs(localScale.z)));
                Renderer renderer = go.GetComponent<Renderer>();
                AssignNamedMaterial(renderer, damageCrackOverlayMaterial);
                ApplyEmissiveBlock(renderer, EnsureCombatStatePropertyBlock(), CrackOverlayColor, CrackOverlayEmission);
                go.SetActive(false);
                combatCrackPieces.Add(go.transform);
            }
        }

        private void SetCombatCrackCount(int count)
        {
            if (!combatCracksBuilt && count <= 0)
            {
                return;
            }

            if (count > 0)
            {
                EnsureCombatCrackOverlay();
            }

            for (int i = 0; i < combatCrackPieces.Count; i++)
            {
                Transform piece = combatCrackPieces[i];
                if (piece == null)
                {
                    continue;
                }

                bool active = i < count;
                if (piece.gameObject.activeSelf != active)
                {
                    piece.gameObject.SetActive(active);
                }
            }

            lastCombatCrackCount = count;
        }

        private static void EnsureCombatStateMaterials()
        {
            if (damageCrackOverlayMaterial == null)
            {
                damageCrackOverlayMaterial = CoalesceCombatStateMaterial(
                    DamageCrackOverlayId,
                    DamageCrackOverlayResourcesPath,
                    DamageCrackOverlayAssetPath,
                    CrackOverlayColor,
                    metallic: 0.02f,
                    smoothness: 0.08f,
                    CrackOverlayEmission);
            }

            if (weakPointGlowMaterial == null)
            {
                weakPointGlowMaterial = CoalesceCombatStateMaterial(
                    WeakPointGlowId,
                    WeakPointGlowResourcesPath,
                    WeakPointGlowAssetPath,
                    WeakPointGlowColor,
                    metallic: 0.08f,
                    smoothness: 0.42f,
                    WeakPointGlowEmission);
            }

            if (shieldedPylonMaterial == null)
            {
                shieldedPylonMaterial = BossIdentityMaterials.ShieldPylon;
                if (shieldedPylonMaterial == null)
                {
                    shieldedPylonMaterial = CoalesceCombatStateMaterial(
                        ShieldedPylonId,
                        ShieldedPylonResourcesPath,
                        ShieldedPylonAssetPath,
                        ShieldedPylonColor,
                        metallic: 0.22f,
                        smoothness: 0.55f,
                        ShieldedPylonEmission);
                }
            }

            if (exposedCoreMaterial == null)
            {
                exposedCoreMaterial = BossIdentityMaterials.CoreExposed;
                if (exposedCoreMaterial == null)
                {
                    exposedCoreMaterial = CoalesceCombatStateMaterial(
                        ExposedCoreId,
                        ExposedCoreResourcesPath,
                        ExposedCoreAssetPath,
                        ExposedCoreColor,
                        metallic: 0.18f,
                        smoothness: 0.62f,
                        ExposedCoreEmission);
                }
            }
        }

        private static Material CoalesceCombatStateMaterial(
            string id,
            string resourcesPath,
            string assetPath,
            Color color,
            float metallic,
            float smoothness,
            Color emission)
        {
            Material loaded = LoadDraftCombatStateMaterial(resourcesPath, assetPath);
            if ((Object)(object)loaded != (Object)null)
            {
                return loaded;
            }

            return CreateCombatStateMaterial(id, color, metallic, smoothness, emission);
        }

        private static Material LoadDraftCombatStateMaterial(string resourcesPath, string assetPath)
        {
            Material material = Resources.Load<Material>(resourcesPath);
            if ((Object)(object)material != (Object)null)
            {
                return material;
            }

#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
            _ = assetPath;
            return null;
#endif
        }

        private static Material CreateCombatStateMaterial(string id, Color color, float metallic, float smoothness, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                            ?? Shader.Find("Unlit/Color")
                            ?? Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = id,
                enableInstancing = true
            };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }

            return material;
        }

        private void ApplyNamedMaterialToChild(Transform parent, string childName, Material material, Color color, Color emission)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            if (material != null)
            {
                AssignNamedMaterial(renderer, material);
            }

            ApplyEmissiveBlock(renderer, EnsureCombatStatePropertyBlock(), color, emission);
        }

        private static void AssignNamedMaterial(Renderer renderer, Material material)
        {
            if (renderer == null || material == null || renderer.sharedMaterial == material)
            {
                return;
            }

            renderer.sharedMaterial = material;
        }

        private static void ApplyEmissiveBlock(Renderer renderer, MaterialPropertyBlock reuse, Color color, Color emission)
        {
            if (renderer == null)
            {
                return;
            }

            MaterialPropertyBlock block = reuse ?? new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_EmissionColor"))
            {
                block.SetColor("_EmissionColor", emission);
            }

            renderer.SetPropertyBlock(block);
        }

        private bool HasActiveNamedKitOnSelf(string kitName)
        {
            Transform kit = FindDirectChild(transform, kitName);
            return kit != null && kit.gameObject.activeSelf;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        private static void RemoveCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(collider);
            }
            else
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}
