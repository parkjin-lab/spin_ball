using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Gameplay
{
	public static class BossIdentityMaterials
	{
		public const string SentinelArmorId = "MAT_Boss_Sentinel_Armor";
		public const string ShieldPylonId = "MAT_Boss_Shield_Pylon";
		public const string CoreExposedId = "MAT_Boss_Core_Exposed";

		private const string SentinelArmorResourcesPath = "Materials/Boss/MAT_Boss_Sentinel_Armor";
		private const string ShieldPylonResourcesPath = "Materials/Boss/MAT_Boss_Shield_Pylon";
		private const string CoreExposedResourcesPath = "Materials/Boss/MAT_Boss_Core_Exposed";
		private const string SentinelArmorAssetPath = "Assets/Art/Materials/Boss/MAT_Boss_Sentinel_Armor.mat";
		private const string ShieldPylonAssetPath = "Assets/Art/Materials/Boss/MAT_Boss_Shield_Pylon.mat";
		private const string CoreExposedAssetPath = "Assets/Art/Materials/Boss/MAT_Boss_Core_Exposed.mat";
		private const string ShieldPylonFallbackResourcesPath = "Materials/Destruction/MAT_Shielded_Pylon";
		private const string CoreExposedFallbackResourcesPath = "Materials/Destruction/MAT_Exposed_Core";
		private const string ShieldPylonFallbackAssetPath = "Assets/Art/Materials/Destruction/MAT_Shielded_Pylon.mat";
		private const string CoreExposedFallbackAssetPath = "Assets/Art/Materials/Destruction/MAT_Exposed_Core.mat";

		private static readonly Color SentinelArmorColor = new Color(0.24f, 0.36f, 0.54f, 1f);
		private static readonly Color SentinelArmorEmission = new Color(0.06f, 0.12f, 0.22f, 1f);
		private static readonly Color ShieldPylonColor = new Color(0.14f, 0.76f, 0.92f, 1f);
		private static readonly Color ShieldPylonEmission = new Color(0.05f, 0.36f, 0.56f, 1f);
		private static readonly Color CoreExposedColor = new Color(1f, 0.38f, 0.06f, 1f);
		private static readonly Color CoreExposedEmission = new Color(0.96f, 0.3f, 0.03f, 1f);

		private static Material sentinelArmorMaterial;
		private static Material shieldPylonMaterial;
		private static Material coreExposedMaterial;

		public static Material Armor => Ensure(
			ref sentinelArmorMaterial,
			SentinelArmorId,
			SentinelArmorResourcesPath,
			SentinelArmorAssetPath,
			null,
			null,
			SentinelArmorColor,
			0.38f,
			0.48f,
			SentinelArmorEmission);

		public static Material ShieldPylon => Ensure(
			ref shieldPylonMaterial,
			ShieldPylonId,
			ShieldPylonResourcesPath,
			ShieldPylonAssetPath,
			ShieldPylonFallbackResourcesPath,
			ShieldPylonFallbackAssetPath,
			ShieldPylonColor,
			0.24f,
			0.58f,
			ShieldPylonEmission);

		public static Material CoreExposed => Ensure(
			ref coreExposedMaterial,
			CoreExposedId,
			CoreExposedResourcesPath,
			CoreExposedAssetPath,
			CoreExposedFallbackResourcesPath,
			CoreExposedFallbackAssetPath,
			CoreExposedColor,
			0.16f,
			0.64f,
			CoreExposedEmission);

		private static Material Ensure(
			ref Material cache,
			string id,
			string resourcesPath,
			string assetPath,
			string fallbackResourcesPath,
			string fallbackAssetPath,
			Color color,
			float metallic,
			float smoothness,
			Color emission)
		{
			if ((Object)(object)cache != (Object)null)
			{
				return cache;
			}

			Material loaded = LoadDraft(resourcesPath, assetPath);
			if ((Object)(object)loaded == (Object)null && !string.IsNullOrEmpty(fallbackResourcesPath))
			{
				loaded = LoadDraft(fallbackResourcesPath, fallbackAssetPath);
			}

			if ((Object)(object)loaded != (Object)null)
			{
				cache = loaded;
				return cache;
			}

			cache = CreateFallback(id, color, metallic, smoothness, emission);
			return cache;
		}

		private static Material LoadDraft(string resourcesPath, string assetPath)
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

		private static Material CreateFallback(string id, Color color, float metallic, float smoothness, Color emission)
		{
			Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
			                ?? Shader.Find("Unlit/Color")
			                ?? Shader.Find("Universal Render Pipeline/Lit")
			                ?? Shader.Find("Standard");
			if ((Object)(object)shader == (Object)null)
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
	}
}
