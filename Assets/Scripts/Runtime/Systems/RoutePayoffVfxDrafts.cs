using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlienCrusher.Systems
{
	public static class RoutePayoffVfxDrafts
	{
		public static Material Instantiate(string vfxId)
		{
			if (string.IsNullOrWhiteSpace(vfxId))
			{
				return null;
			}

			Material loaded = Resources.Load<Material>("VFX/Route/" + vfxId);
			if ((Object)(object)loaded == (Object)null)
			{
#if UNITY_EDITOR
				loaded = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/VFX/Route/" + vfxId + ".mat");
#endif
			}

			if ((Object)(object)loaded == (Object)null)
			{
				return null;
			}

			return new Material(loaded);
		}
	}
}
