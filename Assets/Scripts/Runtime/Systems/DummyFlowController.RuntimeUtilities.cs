using AlienCrusher.Gameplay;
using AlienCrusher.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private static Transform GetOrCreateDirectChild(Transform parent, string name)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			Transform val = FindDirectChild(parent, name);
			if ((Object)(object)val != (Object)null)
			{
				return val;
			}
			GameObject val2 = new GameObject(name);
			val2.transform.SetParent(parent, false);
			return val2.transform;
		}

		private static Transform FindDirectChild(Transform parent, string name)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (((Object)child).name == name)
				{
					return child;
				}
			}
			return null;
		}

		private static GameObject EnsurePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			Transform val = FindDirectChild(parent, name);
			GameObject val2;
			if ((Object)(object)val == (Object)null)
			{
				val2 = GameObject.CreatePrimitive(type);
				((Object)val2).name = name;
				val2.transform.SetParent(parent, false);
			}
			else
			{
				val2 = ((Component)val).gameObject;
			}
			val2.transform.localPosition = localPosition;
			val2.transform.localRotation = Quaternion.identity;
			val2.transform.localScale = SanitizeScale(localScale);
			TintObject(val2, color);
			return val2;
		}

		private static GameObject EnsureDestructiblePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, int hp)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = EnsurePrimitive(parent, name, type, localPosition, localScale, color);
			DummyDestructibleBlock dummyDestructibleBlock = val.GetComponent<DummyDestructibleBlock>();
			if ((Object)(object)dummyDestructibleBlock == (Object)null)
			{
				dummyDestructibleBlock = val.AddComponent<DummyDestructibleBlock>();
			}
			dummyDestructibleBlock.ConfigureForScaffolder(hp, color);
			return val;
		}

		private static void TintObject(GameObject target, Color color)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			Renderer component = target.GetComponent<Renderer>();
			if (!((Object)(object)component == (Object)null))
			{
				Material val = GetRuntimeStylizedMaterial();
				if ((Object)(object)val != (Object)null && (Object)(object)component.sharedMaterial != (Object)(object)val)
				{
					component.sharedMaterial = val;
				}
				MaterialPropertyBlock val2 = new MaterialPropertyBlock();
				component.GetPropertyBlock(val2);
				val2.SetColor("_BaseColor", color);
				val2.SetColor("_Color", color);
				component.SetPropertyBlock(val2);
			}
		}

		private static Material GetRuntimeStylizedMaterial()
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			if ((Object)(object)runtimeStylizedMaterial != (Object)null)
			{
				return runtimeStylizedMaterial;
			}
			Shader val = FindStylizedShaderRuntime();
			if ((Object)(object)val == (Object)null)
			{
				return null;
			}
			Material val2 = new Material(val);
			((Object)val2).name = "M_Runtime_StylizedLowPoly";
			runtimeStylizedMaterial = val2;
			ConfigureStylizedMaterial(runtimeStylizedMaterial);
			return runtimeStylizedMaterial;
		}

		private static Shader FindStylizedShaderRuntime()
		{
			return Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
		}

		private static void ConfigureStylizedMaterial(Material material)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)material == (Object)null))
			{
				material.enableInstancing = true;
				if (material.HasProperty("_Smoothness"))
				{
					material.SetFloat("_Smoothness", 0f);
				}
				if (material.HasProperty("_Glossiness"))
				{
					material.SetFloat("_Glossiness", 0f);
				}
				if (material.HasProperty("_SpecColor"))
				{
					material.SetColor("_SpecColor", Color.black);
				}
				if (material.HasProperty("_SpecularHighlights"))
				{
					material.SetFloat("_SpecularHighlights", 0f);
				}
				if (material.HasProperty("_EnvironmentReflections"))
				{
					material.SetFloat("_EnvironmentReflections", 0f);
				}
			}
		}

		private static void MoveChildIfExists(Transform from, Transform to, string name)
		{
			Transform val = FindDirectChild(from, name);
			if (!((Object)(object)val == (Object)null))
			{
				val.SetParent(to, false);
			}
		}

		private static void StretchRect(RectTransform rect)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
		}

		private static Transform FindChildByName(Transform root, string targetName)
		{
			if (string.IsNullOrWhiteSpace(targetName))
			{
				return null;
			}
			if ((Object)(object)root == (Object)null)
			{
				Transform[] array = Object.FindObjectsByType<Transform>((FindObjectsInactive)1, (FindObjectsSortMode)0);
				foreach (Transform val in array)
				{
					if (((Object)val).name == targetName)
					{
						return val;
					}
				}
				return null;
			}
			if (((Object)root).name == targetName)
			{
				return root;
			}
			for (int j = 0; j < root.childCount; j++)
			{
				Transform val2 = FindChildByName(root.GetChild(j), targetName);
				if ((Object)(object)val2 != (Object)null)
				{
					return val2;
				}
			}
			return null;
		}

		private static void SetPanelState(DummyPanelTag panel, bool active)
		{
			if (!((Object)(object)panel == (Object)null))
			{
				panel.Configure(panel.PanelId, active);
			}
		}

		private static Vector3 SanitizeScale(Vector3 scale)
		{
			// Prevent zeroed scaffold geometry from becoming invisible at runtime.
			float num = ((Mathf.Abs(scale.x) < 0.08f) ? 1f : scale.x);
			float num2 = ((Mathf.Abs(scale.y) < 0.08f) ? 1f : scale.y);
			float num3 = ((Mathf.Abs(scale.z) < 0.08f) ? 1f : scale.z);
			return new Vector3(num, num2, num3);
		}
	}
}
