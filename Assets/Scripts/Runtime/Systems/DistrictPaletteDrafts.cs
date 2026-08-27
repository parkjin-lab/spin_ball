using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Systems
{
	public static class DistrictPaletteDrafts
	{
		private const string DistrictsResourcesFolder = "Palettes/Districts/";
		private const string RouteResourcesPath = "Palettes/UI/PAL_RouteMarker_Tints";
		private const string AmbientResourcesPath = "Palettes/Lighting/PAL_Ambient_StageBands";
		private const string DistrictsAssetFolder = "Assets/Art/Palettes/Districts/";
		private const string RouteAssetPath = "Assets/Art/Palettes/UI/PAL_RouteMarker_Tints.mat";
		private const string AmbientAssetPath = "Assets/Art/Palettes/Lighting/PAL_Ambient_StageBands.mat";

		public static Material LoadDistrict(string paletteId)
		{
			if (string.IsNullOrWhiteSpace(paletteId))
			{
				return null;
			}

			return LoadDraft(DistrictsResourcesFolder + paletteId, DistrictsAssetFolder + paletteId + ".mat");
		}

		public static Material LoadRoute()
		{
			return LoadDraft(RouteResourcesPath, RouteAssetPath);
		}

		public static Material LoadAmbient()
		{
			return LoadDraft(AmbientResourcesPath, AmbientAssetPath);
		}

		public static Color ReadColor(Material material, string property, Color fallback)
		{
			if ((Object)(object)material == (Object)null || string.IsNullOrEmpty(property) || !material.HasProperty(property))
			{
				return fallback;
			}

			Color color = material.GetColor(property);
			if (color.maxColorComponent <= 0.02f && color.a <= 0.02f)
			{
				return fallback;
			}

			return color;
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
	}
}
