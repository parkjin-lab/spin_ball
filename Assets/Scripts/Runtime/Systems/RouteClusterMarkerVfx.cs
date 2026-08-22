using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public static class RouteClusterMarkerVfx
	{
		public const string MarkerVfxId = "VFX_RouteCluster_Marker";
		public const string MarkerHostId = "RouteClusterMarker";

		private static readonly Color FrameBase = new Color(0.36f, 0.7f, 0.52f, 0.82f);

		public static void EnsureRouteClusterMarker(Transform groundDetailsRoot, Vector3 worldCenter, float radius, Color payoffColor, string layoutId)
		{
			if ((Object)(object)groundDetailsRoot == (Object)null)
			{
				return;
			}

			string hostName = MarkerHostId + "_" + (string.IsNullOrWhiteSpace(layoutId) ? "Default" : layoutId);
			Transform host = groundDetailsRoot.Find(hostName);
			if ((Object)(object)host == (Object)null)
			{
				GameObject hostGo = new GameObject(hostName);
				host = hostGo.transform;
				host.SetParent(groundDetailsRoot, false);
			}

			Vector3 local = groundDetailsRoot.InverseTransformPoint(worldCenter);
			local.y = 0.04f;
			host.localPosition = local;
			host.localRotation = Quaternion.identity;
			host.localScale = Vector3.one;
			host.gameObject.SetActive(true);

			Transform ring = host.Find(MarkerVfxId);
			if ((Object)(object)ring == (Object)null)
			{
				GameObject ringGo = new GameObject(MarkerVfxId);
				ring = ringGo.transform;
				ring.SetParent(host, false);
			}

			ring.localPosition = Vector3.zero;
			ring.localRotation = Quaternion.identity;
			ring.localScale = Vector3.one;
			BuildRingFrame(ring, Mathf.Max(1.8f, radius), Color.Lerp(FrameBase, payoffColor, 0.28f));
		}

		private static void BuildRingFrame(Transform ring, float radius, Color color)
		{
			ClearChildren(ring);
			int segments = 10;
			float reach = radius * 1.04f;
			float arc = (Mathf.PI * 2f * reach) / segments;
			Vector3 segmentScale = new Vector3(Mathf.Max(0.1f, arc * 0.58f), 0.08f, 0.11f);
			for (int i = 0; i < segments; i++)
			{
				float angle = (360f / segments) * i;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				CreateSegment(ring, "Frame_" + i.ToString("00"), dir * reach, Quaternion.LookRotation(dir, Vector3.up), segmentScale, color);
			}

			for (int i = 0; i < 4; i++)
			{
				float angle = 90f * i + 45f;
				Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
				CreateSegment(ring, "Tick_" + i.ToString("00"), dir * (reach * 0.9f), Quaternion.LookRotation(dir, Vector3.up), new Vector3(0.1f, 0.08f, 0.28f), Color.Lerp(color, Color.white, 0.22f));
			}
		}

		private static void CreateSegment(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color)
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = name;
			go.transform.SetParent(parent, false);
			go.transform.localPosition = localPosition;
			go.transform.localRotation = localRotation;
			go.transform.localScale = localScale;
			Collider collider = go.GetComponent<Collider>();
			if ((Object)(object)collider != (Object)null)
			{
				Object.Destroy(collider);
			}

			Renderer renderer = go.GetComponent<Renderer>();
			if ((Object)(object)renderer == (Object)null)
			{
				return;
			}

			Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
			if ((Object)(object)shader == (Object)null)
			{
				return;
			}

			Material material = new Material(shader)
			{
				name = "M_Runtime_RouteClusterMarker"
			};
			if (material.HasProperty("_BaseColor"))
			{
				material.SetColor("_BaseColor", color);
			}

			if (material.HasProperty("_Color"))
			{
				material.SetColor("_Color", color);
			}

			renderer.sharedMaterial = material;
		}

		private static void ClearChildren(Transform parent)
		{
			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				Transform child = parent.GetChild(i);
				if (Application.isPlaying)
				{
					Object.Destroy(child.gameObject);
				}
				else
				{
					Object.DestroyImmediate(child.gameObject);
				}
			}
		}
	}
}
