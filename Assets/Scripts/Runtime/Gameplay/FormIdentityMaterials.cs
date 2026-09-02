using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Gameplay
{
	public static class FormIdentityMaterials
	{
		public const string SphereId = "MAT_Form_Sphere";
		public const string RamId = "MAT_Form_Ram";
		public const string SaucerId = "MAT_Form_Saucer";
		public const string SpikeId = "MAT_Form_Spike";
		public const string CrusherId = "MAT_Form_Crusher";

		private const string SphereResourcesPath = "Materials/Forms/MAT_Form_Sphere";
		private const string RamResourcesPath = "Materials/Forms/MAT_Form_Ram";
		private const string SaucerResourcesPath = "Materials/Forms/MAT_Form_Saucer";
		private const string SpikeResourcesPath = "Materials/Forms/MAT_Form_Spike";
		private const string CrusherResourcesPath = "Materials/Forms/MAT_Form_Crusher";
		private const string SphereAssetPath = "Assets/Art/Materials/Forms/MAT_Form_Sphere.mat";
		private const string RamAssetPath = "Assets/Art/Materials/Forms/MAT_Form_Ram.mat";
		private const string SaucerAssetPath = "Assets/Art/Materials/Forms/MAT_Form_Saucer.mat";
		private const string SpikeAssetPath = "Assets/Art/Materials/Forms/MAT_Form_Spike.mat";
		private const string CrusherAssetPath = "Assets/Art/Materials/Forms/MAT_Form_Crusher.mat";

		public static Material Create(string materialName, Color color, Color emission)
		{
			Material source = LoadFamilyDraft(materialName);
			Material material;
			if ((Object)(object)source != (Object)null)
			{
				material = new Material(source)
				{
					name = materialName
				};
			}
			else
			{
				Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
				                ?? Shader.Find("Unlit/Color")
				                ?? Shader.Find("Universal Render Pipeline/Lit")
				                ?? Shader.Find("Standard");
				if ((Object)(object)shader == (Object)null)
				{
					return null;
				}

				material = new Material(shader)
				{
					name = materialName
				};
			}

			if (material.HasProperty("_BaseColor"))
			{
				material.SetColor("_BaseColor", color);
			}

			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", color);
			}

			if (material.HasProperty("_EmissionColor"))
			{
				material.EnableKeyword("_EMISSION");
				material.SetColor("_EmissionColor", emission);
			}

			return material;
		}

		private static Material LoadFamilyDraft(string materialName)
		{
			ResolveFamilyPaths(materialName, out string resourcesPath, out string assetPath);
			if (string.IsNullOrEmpty(resourcesPath))
			{
				return null;
			}

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

		private static void ResolveFamilyPaths(string materialName, out string resourcesPath, out string assetPath)
		{
			resourcesPath = null;
			assetPath = null;
			if (string.IsNullOrEmpty(materialName))
			{
				return;
			}

			if (materialName.IndexOf("Sphere", System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				resourcesPath = SphereResourcesPath;
				assetPath = SphereAssetPath;
				return;
			}

			if (materialName.IndexOf("Ram", System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				resourcesPath = RamResourcesPath;
				assetPath = RamAssetPath;
				return;
			}

			if (materialName.IndexOf("Saucer", System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				resourcesPath = SaucerResourcesPath;
				assetPath = SaucerAssetPath;
				return;
			}

			if (materialName.IndexOf("Spike", System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				resourcesPath = SpikeResourcesPath;
				assetPath = SpikeAssetPath;
				return;
			}

			if (materialName.IndexOf("Crusher", System.StringComparison.OrdinalIgnoreCase) >= 0)
			{
				resourcesPath = CrusherResourcesPath;
				assetPath = CrusherAssetPath;
			}
		}
	}
}
