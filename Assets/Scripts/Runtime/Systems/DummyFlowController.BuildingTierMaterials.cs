using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private const string BuildingMaterialSmallId = "MAT_Building_Small";
		private const string BuildingMaterialMidId = "MAT_Building_Mid";
		private const string BuildingMaterialLargeId = "MAT_Building_Large";
		private const string BuildingMaterialBossId = "MAT_Boss_Structure";
		private const string BossSentinelKitName = "BOSS_Sentinel_Body_Kit";
		private const string BossShieldPylonKitName = "BOSS_Shield_Pylon_Kit";
		private const string BossPhase2DroneKitName = "BOSS_Phase2_Drone_Kit";

		private static readonly Color BuildingSmallHealthy = new Color(0.86f, 0.78f, 0.64f, 1f);
		private static readonly Color BuildingSmallHit = new Color(0.96f, 0.70f, 0.46f, 1f);
		private static readonly Color BuildingMidHealthy = new Color(0.46f, 0.42f, 0.38f, 1f);
		private static readonly Color BuildingMidHit = new Color(0.78f, 0.48f, 0.28f, 1f);
		private static readonly Color BuildingLargeHealthy = new Color(0.16f, 0.17f, 0.20f, 1f);
		private static readonly Color BuildingLargeHit = new Color(0.92f, 0.36f, 0.14f, 1f);
		private static readonly Color BuildingBossHealthy = new Color(0.20f, 0.30f, 0.46f, 1f);
		private static readonly Color BuildingBossHit = new Color(0.58f, 0.80f, 1f, 1f);

		private static Material buildingMaterialSmall;
		private static Material buildingMaterialMid;
		private static Material buildingMaterialLarge;
		private static Material buildingMaterialBoss;
		private static bool buildingTierCallbackRegistered;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void RegisterBuildingTierReadability()
		{
			DummyDestructibleBlock.AfterScaffolderConfigured = ApplyBuildingTierMaterial;
			buildingTierCallbackRegistered = true;
		}

		private static void EnsureBuildingTierCallbackRegistered()
		{
			if (buildingTierCallbackRegistered && DummyDestructibleBlock.AfterScaffolderConfigured == ApplyBuildingTierMaterial)
			{
				return;
			}

			DummyDestructibleBlock.AfterScaffolderConfigured = ApplyBuildingTierMaterial;
			buildingTierCallbackRegistered = true;
		}

		private void ApplyBuildingTierMaterialsToAll()
		{
			EnsureBuildingTierCallbackRegistered();
			RefreshDestructibleCache();
			for (int i = 0; i < destructibleCache.Count; i++)
			{
				ApplyBuildingTierMaterial(destructibleCache[i]);
			}
		}

		private static void ApplyBuildingTierMaterial(DummyDestructibleBlock block)
		{
			if ((Object)(object)block == (Object)null)
			{
				return;
			}

			EnsureBuildingTierCallbackRegistered();
			EnsureBuildingTierMaterials();

			Material material;
			Color healthy;
			Color hit;
			if (IsBossStructureTarget(block))
			{
				material = buildingMaterialBoss;
				healthy = BuildingBossHealthy;
				hit = BuildingBossHit;
			}
			else if (block.IsLargeBuildingTarget)
			{
				material = buildingMaterialLarge;
				healthy = BuildingLargeHealthy;
				hit = BuildingLargeHit;
			}
			else if (IsSmallBuildingTarget(block))
			{
				material = buildingMaterialSmall;
				healthy = BuildingSmallHealthy;
				hit = BuildingSmallHit;
			}
			else
			{
				material = buildingMaterialMid;
				healthy = BuildingMidHealthy;
				hit = BuildingMidHit;
			}

			block.ApplyReadabilitySurface(material, healthy, hit);
		}

		private static bool IsBossStructureTarget(DummyDestructibleBlock block)
		{
			if (block.IsStageBoss || block.CurrentStageEncounterRole == DummyDestructibleBlock.StageEncounterRole.BossSentinel)
			{
				return true;
			}

			string name = ((Object)block).name;
			if (!string.IsNullOrEmpty(name)
			    && (name.StartsWith("Sentinel", System.StringComparison.OrdinalIgnoreCase)
			        || name.IndexOf("ShieldPylon", System.StringComparison.OrdinalIgnoreCase) >= 0
			        || name.IndexOf("GateBlock", System.StringComparison.OrdinalIgnoreCase) >= 0))
			{
				return true;
			}

			return HasActiveBossIdentityKit(block.transform);
		}

		private static bool IsSmallBuildingTarget(DummyDestructibleBlock block)
		{
			string name = ((Object)block).name;
			if (!string.IsNullOrEmpty(name)
			    && (name.StartsWith("Prop_", System.StringComparison.OrdinalIgnoreCase)
			        || name.StartsWith("StarterLotProp_", System.StringComparison.OrdinalIgnoreCase)
			        || name.StartsWith("StarterLaneProp_", System.StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}

			return block.IsSmallBuildingTier();
		}

		private static bool HasActiveBossIdentityKit(Transform host)
		{
			if ((Object)(object)host == (Object)null)
			{
				return false;
			}

			return IsActiveNamedChild(host, BossSentinelKitName)
			       || IsActiveNamedChild(host, BossShieldPylonKitName)
			       || IsActiveNamedChild(host, BossPhase2DroneKitName);
		}

		private static bool IsActiveNamedChild(Transform host, string childName)
		{
			Transform child = host.Find(childName);
			return (Object)(object)child != (Object)null && child.gameObject.activeSelf;
		}

		private static void EnsureBuildingTierMaterials()
		{
			if ((Object)(object)buildingMaterialSmall == (Object)null)
			{
				buildingMaterialSmall = CreateBuildingTierMaterial(
					BuildingMaterialSmallId,
					BuildingSmallHealthy,
					metallic: 0f,
					smoothness: 0.06f,
					emission: Color.black);
			}

			if ((Object)(object)buildingMaterialMid == (Object)null)
			{
				buildingMaterialMid = CreateBuildingTierMaterial(
					BuildingMaterialMidId,
					BuildingMidHealthy,
					metallic: 0.04f,
					smoothness: 0.16f,
					emission: Color.black);
			}

			if ((Object)(object)buildingMaterialLarge == (Object)null)
			{
				buildingMaterialLarge = CreateBuildingTierMaterial(
					BuildingMaterialLargeId,
					BuildingLargeHealthy,
					metallic: 0.14f,
					smoothness: 0.28f,
					emission: new Color(0.02f, 0.02f, 0.03f, 1f));
			}

			if ((Object)(object)buildingMaterialBoss == (Object)null)
			{
				buildingMaterialBoss = CreateBuildingTierMaterial(
					BuildingMaterialBossId,
					BuildingBossHealthy,
					metallic: 0.32f,
					smoothness: 0.44f,
					emission: new Color(0.04f, 0.08f, 0.16f, 1f));
			}
		}

		private static Material CreateBuildingTierMaterial(string id, Color color, float metallic, float smoothness, Color emission)
		{
			Shader shader = FindStylizedShaderRuntime();
			if ((Object)(object)shader == (Object)null)
			{
				return null;
			}

			Material material = new Material(shader);
			((Object)material).name = id;
			ConfigureStylizedMaterial(material);
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
