using System;
using System.Collections.Generic;
using AlienCrusher.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Systems
{
	public partial class DummyFlowController
	{
		private void EnsureRuntimeMapFallback()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			DummyDestructibleBlock[] existingDestructibles = Object.FindObjectsByType<DummyDestructibleBlock>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			if (existingDestructibles.Length < 68)
			{
				Transform val = FindChildByName(null, "_Gameplay");
				if ((Object)(object)val == (Object)null)
				{
					val = new GameObject("_Gameplay").transform;
				}
				Transform orCreateDirectChild = GetOrCreateDirectChild(val, "MapRoot");
				BuildFallbackMap(orCreateDirectChild);
				EnsureSpawnPointFallback(val);
				Debug.Log((object)"[AlienCrusher] Destructible density was low, so fallback map objects were generated at runtime.");
				return;
			}
			EnsureOpeningDestructionDensity(existingDestructibles);
		}

		private void BuildFallbackMap(Transform mapRoot)
		{
			//IL_0502: Unknown result type (might be due to invalid IL or missing references)
			//IL_0516: Unknown result type (might be due to invalid IL or missing references)
			//IL_051b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0538: Unknown result type (might be due to invalid IL or missing references)
			//IL_054c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0551: Unknown result type (might be due to invalid IL or missing references)
			//IL_056e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0582: Unknown result type (might be due to invalid IL or missing references)
			//IL_0587: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_05da: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0700: Unknown result type (might be due to invalid IL or missing references)
			//IL_0709: Unknown result type (might be due to invalid IL or missing references)
			//IL_070e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0744: Unknown result type (might be due to invalid IL or missing references)
			//IL_074b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0787: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0805: Unknown result type (might be due to invalid IL or missing references)
			//IL_0807: Unknown result type (might be due to invalid IL or missing references)
			//IL_0810: Unknown result type (might be due to invalid IL or missing references)
			//IL_0815: Unknown result type (might be due to invalid IL or missing references)
			//IL_0865: Unknown result type (might be due to invalid IL or missing references)
			//IL_086a: Unknown result type (might be due to invalid IL or missing references)
			//IL_086c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0871: Unknown result type (might be due to invalid IL or missing references)
			//IL_089d: Unknown result type (might be due to invalid IL or missing references)
			//IL_089f: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_090c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0911: Unknown result type (might be due to invalid IL or missing references)
			//IL_0913: Unknown result type (might be due to invalid IL or missing references)
			//IL_0918: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0af0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0af2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0af6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0afb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b10: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b1e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b29: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b2e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b88: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b8d: Invalid comparison between Unknown and I4
			//IL_0b95: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bb3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bcd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bf5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bf7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c00: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c05: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c0c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c25: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c3b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c42: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c67: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c69: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c6b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c6d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cb1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cc5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ce9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cfd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d02: Unknown result type (might be due to invalid IL or missing references)
			RuntimeCityThemeProfile runtimeCityThemeProfile = ResolveFallbackCityTheme(mapRoot);
			System.Random random = new System.Random(2117 + (int)runtimeCityThemeProfile * 97);
			Color color = default(Color);
			color = new Color(0.06f, 0.09f, 0.14f);
			Color color2 = default(Color);
			color2 = new Color(0.14f, 0.19f, 0.28f);
			Color val = default(Color);
			val = new Color(0.7f, 0.76f, 0.84f);
			Color val2 = default(Color);
			val2 = new Color(0.92f, 0.75f, 0.46f);
			Color val3 = default(Color);
			val3 = new Color(0.86f, 0.24f, 0.2f);
			Color val4 = default(Color);
			val4 = new Color(0.18f, 0.62f, 0.92f);
			Color val5 = default(Color);
			val5 = new Color(0.24f, 0.55f, 0.27f);
			Color val6 = default(Color);
			val6 = new Color(0.42f, 0.72f, 0.35f);
			Color val7 = default(Color);
			val7 = new Color(0.82f, 0.87f, 0.95f);
			Color val8 = default(Color);
			val8 = new Color(1f, 0.62f, 0.29f);
			Color val9 = default(Color);
			val9 = new Color(0.98f, 0.38f, 0.18f);
			Color val10 = default(Color);
			val10 = new Color(1f, 0.74f, 0.24f);
			float num = 0.32f;
			float num2 = 0.22f;
			float num3 = 0.2f;
			float num4 = 0.1f;
			float num5 = 0.05f;
			float num6 = 2f;
			float num7 = 12f;
			float num8 = 0.995f;
			float num9 = 0.91f;
			float num10 = 0.62f;
			float num11 = 0.85f;
			float num12 = 1.65f;
			float num13 = 1.8f;
			float num14 = 3.1f;
			float num15 = 2.3f;
			float num16 = 4.2f;
			float num17 = 0.55f;
			float num18 = 1.45f;
			float num19 = 1.8f;
			float num20 = 4.5f;
			float num21 = 4.3f;
			float num22 = 8.8f;
			float num23 = 0.08f;
			float num24 = 0.18f;
			float num25 = 0.72f;
			float num25a = 0.6f;
			float num25b = 0.58f;
			float num25c = 0.42f;
			float num25d = 0.36f;
			float num25e = 0.42f;
			float num25f = 0.42f;
			float num25g = 0.3f;
			float num26 = 0.62f;
			float num27BoundaryPadding = 1.15f;
			Color color3 = default(Color);
			color3 = new Color(0.97f, 0.62f, 0.25f);
			Color color3a = default(Color);
			color3a = new Color(1f, 0.74f, 0.34f, 0.72f);
			Color color3b = default(Color);
			color3b = new Color(0.12f, 0.14f, 0.17f, 1f);
			Color color3c = default(Color);
			color3c = new Color(0.32f, 0.34f, 0.37f, 1f);
			Color color3d = default(Color);
			color3d = new Color(0.16f, 0.18f, 0.2f, 1f);
			Color color3g = default(Color);
			color3g = new Color(0.92f, 0.92f, 0.9f, 0.92f);
			Color color3h = default(Color);
			color3h = new Color(0.26f, 0.82f, 0.94f, 0.88f);
			Color color3i = default(Color);
			color3i = new Color(0.54f, 0.56f, 0.58f, 0.9f);
			Color color3e = default(Color);
			color3e = new Color(0.92f, 0.34f, 0.22f, 1f);
			Color color3f = default(Color);
			color3f = new Color(0.22f, 0.72f, 0.96f, 1f);
			switch (runtimeCityThemeProfile)
			{
			case RuntimeCityThemeProfile.IndustrialHarbor:
				color = new Color(0.08f, 0.1f, 0.12f);
				color2 = new Color(0.18f, 0.2f, 0.24f);
				val = new Color(0.66f, 0.69f, 0.75f);
				val2 = new Color(0.81f, 0.74f, 0.66f);
				val3 = new Color(0.91f, 0.43f, 0.16f);
				val4 = new Color(0.3f, 0.49f, 0.78f);
				val5 = new Color(0.18f, 0.42f, 0.21f);
				val6 = new Color(0.28f, 0.54f, 0.27f);
				val7 = new Color(0.74f, 0.79f, 0.85f);
				val8 = new Color(0.94f, 0.54f, 0.22f);
				val9 = new Color(0.92f, 0.33f, 0.14f);
				val10 = new Color(1f, 0.64f, 0.2f);
				num = 0.42f;
				num2 = 0.27f;
				num3 = 0.09f;
				num4 = 0.16f;
				num5 = 0.08f;
				num9 = 0.83f;
				num10 = 0.5f;
				num15 = 2.5f;
				num16 = 5f;
				num21 = 4.8f;
				num22 = 10.5f;
				num25 = 0.6f;
				color3 = new Color(1f, 0.68f, 0.3f);
				break;
			case RuntimeCityThemeProfile.GardenResidential:
				color = new Color(0.1f, 0.14f, 0.1f);
				color2 = new Color(0.17f, 0.24f, 0.18f);
				val = new Color(0.81f, 0.87f, 0.92f);
				val2 = new Color(0.98f, 0.9f, 0.72f);
				val3 = new Color(0.92f, 0.36f, 0.3f);
				val4 = new Color(0.26f, 0.74f, 0.9f);
				val5 = new Color(0.2f, 0.58f, 0.24f);
				val6 = new Color(0.5f, 0.78f, 0.42f);
				val7 = new Color(0.88f, 0.92f, 0.96f);
				val8 = new Color(1f, 0.7f, 0.4f);
				val9 = new Color(1f, 0.42f, 0.22f);
				val10 = new Color(1f, 0.78f, 0.36f);
				num = 0.24f;
				num2 = 0.28f;
				num3 = 0.34f;
				num4 = 0.07f;
				num5 = 0.03f;
				num6 = 3f;
				num7 = 11f;
				num9 = 0.9f;
				num10 = 0.68f;
				num18 = 1.3f;
				num21 = 3.7f;
				num22 = 7.2f;
				num25 = 0.84f;
				num25a = 0.72f;
				num25b = 0.72f;
				num25c = 0.54f;
				num25e = 0.4f;
				num25f = 0.46f;
				num25g = 0.5f;
				color3 = new Color(0.96f, 0.7f, 0.34f);
				color3a = new Color(0.98f, 0.78f, 0.4f, 0.72f);
				color3b = new Color(0.16f, 0.17f, 0.19f, 1f);
				color3c = new Color(0.38f, 0.39f, 0.4f, 1f);
				color3d = new Color(0.19f, 0.21f, 0.18f, 1f);
				color3g = new Color(0.94f, 0.93f, 0.88f, 0.92f);
				color3h = new Color(0.3f, 0.86f, 0.98f, 0.88f);
				color3i = new Color(0.62f, 0.64f, 0.62f, 0.9f);
				color3e = new Color(0.95f, 0.46f, 0.28f, 1f);
				color3f = new Color(0.28f, 0.82f, 0.98f, 1f);
				break;
			}
			EnsurePrimitive(mapRoot, "Ground", (PrimitiveType)3, new Vector3(0f, -0.5f, 0f), new Vector3(50f, 1f, 50f), color);
			EnsurePrimitive(mapRoot, "Wall_North", (PrimitiveType)3, new Vector3(0f, 2f, 25f), new Vector3(50f, 4f, 1f), color2);
			EnsurePrimitive(mapRoot, "Wall_South", (PrimitiveType)3, new Vector3(0f, 2f, -25f), new Vector3(50f, 4f, 1f), color2);
			EnsurePrimitive(mapRoot, "Wall_East", (PrimitiveType)3, new Vector3(25f, 2f, 0f), new Vector3(1f, 4f, 50f), color2);
			EnsurePrimitive(mapRoot, "Wall_West", (PrimitiveType)3, new Vector3(-25f, 2f, 0f), new Vector3(1f, 4f, 50f), color2);
			Transform orCreateDirectChild = GetOrCreateDirectChild(mapRoot, "CityBlocks");
			Transform orCreateDirectChild2 = GetOrCreateDirectChild(mapRoot, "MicroProps");
			Transform orCreateDirectChild3 = GetOrCreateDirectChild(mapRoot, "StreetProps");
			Transform orCreateDirectChild4 = GetOrCreateDirectChild(mapRoot, "TargetMarkers");
			Transform orCreateDirectChild5 = GetOrCreateDirectChild(mapRoot, "GroundDetails");
			ClearChildrenRuntime(orCreateDirectChild);
			ClearChildrenRuntime(orCreateDirectChild2);
			ClearChildrenRuntime(orCreateDirectChild3);
			ClearChildrenRuntime(orCreateDirectChild4);
			ClearChildrenRuntime(orCreateDirectChild5);
			float num27 = -24.650002f;
			float num28 = -20f;
			int num29 = Mathf.CeilToInt(7.9f);
			int num30 = 0;
			int num31 = 0;
			int num32 = 0;
			int num33 = 0;
			int num34 = 0;
			int num35 = 0;
			int num36 = 0;
			List<Vector4> footprints = new List<Vector4>(270);
			Vector3 val11 = default(Vector3);
			Vector3 val12 = default(Vector3);
			Vector3 val15 = default(Vector3);
			for (int i = 0; i < 15; i++)
			{
				float num37 = num28 + (float)i * 2.9f;
				bool flag = i % 5 == 2;
				for (int j = 0; j < 18; j++)
				{
					float num38 = num27 + (float)j * 2.9f;
					bool flag2 = j % 6 == 3;
					bool flag3 = num37 < -13.5f && Mathf.Abs(num38) < 5.4f;
					bool flag3a = num37 > num7 + 3.2f && Mathf.Abs(num38) < 4.4f;
					Vector3 val15a = new Vector3(2.668f, 0.02f, 2.668f);
					if (flag || flag2)
					{
						EnsurePrimitive(orCreateDirectChild5, $"RoadTile_{i:00}_{j:00}", (PrimitiveType)3, new Vector3(num38, -0.01f, num37), val15a, color3b);
					}
					else if (!flag3)
					{
						float num38a = Mathf.InverseLerp(-20f, 20f, num37);
						Color lotColor = Color.Lerp(color3c, color3d, num38a * 0.65f);
						EnsurePrimitive(orCreateDirectChild5, $"LotTile_{i:00}_{j:00}", (PrimitiveType)3, new Vector3(num38, -0.015f, num37), new Vector3(2.494f, 0.015f, 2.494f), lotColor);
						if (flag3a)
						{
							EnsurePrimitive(orCreateDirectChild5, $"BossApproachLane_{i:00}_{j:00}", (PrimitiveType)3, new Vector3(num38, -0.007f, num37), new Vector3(1.34f, 0.01f, 0.24f), new Color(color3h.r, color3h.g, color3h.b, 0.34f));
						}
					}
					if (flag || flag2 || flag3)
					{
						if ((flag || flag2) && random.NextDouble() < (double)num)
						{
							Color bodyColor = Color.Lerp(val3, val4, (float)random.NextDouble());
							float yaw = ((random.Next(0, 2) == 0) ? 0f : 90f);
							EnsureTrafficVehicleRuntime(orCreateDirectChild3, $"Car_{num32:000}", new Vector3(num38, 0f, num37), yaw, bodyColor);
							num32++;
						}
						if ((flag || flag2) && random.NextDouble() < (double)num2)
						{
							EnsureStreetLampRuntime(orCreateDirectChild3, $"Lamp_{num33:000}", new Vector3(num38, 0f, num37));
							num33++;
						}
						if ((flag || flag2) && random.NextDouble() < (double)num3)
						{
							Color leafColor = Color.Lerp(val5, val6, (float)random.NextDouble());
							EnsureStreetTreeRuntime(orCreateDirectChild3, $"Tree_{num34:000}", new Vector3(num38, 0f, num37), leafColor);
							num34++;
						}
						if (!flag3 && (flag || flag2) && random.NextDouble() < (double)num4)
						{
							Color bodyColor2 = Color.Lerp(val9, val10, (float)random.NextDouble());
							val11 = new Vector3(((float)random.NextDouble() - 0.5f) * 0.72f, 0f, ((float)random.NextDouble() - 0.5f) * 0.72f);
							EnsureExplosiveBarrelRuntime(orCreateDirectChild3, $"Barrel_{num35:000}", new Vector3(num38, 0f, num37) + val11, bodyColor2);
							num35++;
						}
						if (!flag3 && (flag || flag2) && random.NextDouble() < (double)num5)
						{
							Color baseColor = Color.Lerp(val10, Color.white, 0.18f + (float)random.NextDouble() * 0.22f);
							val12 = new Vector3(((float)random.NextDouble() - 0.5f) * 0.62f, 0f, ((float)random.NextDouble() - 0.5f) * 0.62f);
							EnsureTransformerRuntime(orCreateDirectChild3, $"Transformer_{num36:000}", new Vector3(num38, 0f, num37) + val12, baseColor);
							num36++;
						}
						if (!flag3 && flag && !flag2 && random.NextDouble() < 0.58)
						{
							float num38b = ((random.Next(0, 2) == 0) ? (-1f) : 1f) * Mathf.Lerp(0.84f, 1.02f, (float)random.NextDouble());
							Vector3 val12a = new Vector3(num38 + Mathf.Lerp(-0.16f, 0.16f, (float)random.NextDouble()), 0f, num37 + num38b);
							EnsureStreetLampRuntime(orCreateDirectChild3, $"Lamp_{num33:000}", val12a);
							num33++;
							if (random.NextDouble() < 0.38)
							{
								Color leafColor2 = Color.Lerp(val5, val6, (float)random.NextDouble());
								Vector3 val12b = new Vector3(num38 + Mathf.Lerp(-0.24f, 0.24f, (float)random.NextDouble()), 0f, num37 - num38b * Mathf.Lerp(0.92f, 1.08f, (float)random.NextDouble()));
								EnsureStreetTreeRuntime(orCreateDirectChild3, $"Tree_{num34:000}", val12b, leafColor2);
								num34++;
							}
						}
						else if (!flag3 && flag2 && !flag && random.NextDouble() < 0.52)
						{
							float num38c = ((random.Next(0, 2) == 0) ? (-1f) : 1f) * Mathf.Lerp(0.84f, 1.02f, (float)random.NextDouble());
							Vector3 val12c = new Vector3(num38 + num38c, 0f, num37 + Mathf.Lerp(-0.16f, 0.16f, (float)random.NextDouble()));
							EnsureStreetLampRuntime(orCreateDirectChild3, $"Lamp_{num33:000}", val12c);
							num33++;
							if (random.NextDouble() < 0.34)
							{
								Color leafColor3 = Color.Lerp(val5, val6, (float)random.NextDouble());
								Vector3 val12d = new Vector3(num38 - num38c * Mathf.Lerp(0.92f, 1.08f, (float)random.NextDouble()), 0f, num37 + Mathf.Lerp(-0.22f, 0.22f, (float)random.NextDouble()));
								EnsureStreetTreeRuntime(orCreateDirectChild3, $"Tree_{num34:000}", val12d, leafColor3);
								num34++;
							}
						}
						continue;
					}
					float num39 = Mathf.InverseLerp(-20f, 20f, num37);
					bool flag4 = num37 < num6;
					bool flag5 = num37 >= num6 && num37 < num7;
					float num39a = num8;
					if (i < num29)
					{
						num39a = 1f;
					}
					float num40 = (float)random.NextDouble();
					if ((flag4 && num40 > num39a) || (flag5 && num40 > num9) || (!flag4 && !flag5 && num40 > num10))
					{
						continue;
					}
					float num41 = (flag4 ? num11 : (flag5 ? num13 : num15));
					float num42 = (flag4 ? num12 : (flag5 ? num14 : num16));
					float num43 = (flag4 ? num17 : (flag5 ? num19 : num21));
					float num44 = (flag4 ? num18 : (flag5 ? num20 : num22));
					if (i < num29)
					{
						num41 = Mathf.Min(num41, 0.92f);
						num42 = Mathf.Min(num42, 1.62f);
						num43 = Mathf.Min(num43, 0.56f);
						num44 = Mathf.Min(num44, 1.55f);
					}
					float num45 = Mathf.Lerp(num41, num42, (float)random.NextDouble());
					float num46 = Mathf.Lerp(num41, num42, (float)random.NextDouble());
					float num47 = Mathf.Lerp(num43, num44, (float)random.NextDouble());
					float num48 = (flag4 ? num23 : num24);
					float num49 = num38 + ((float)random.NextDouble() - 0.5f) * num48;
					float num50 = num37 + ((float)random.NextDouble() - 0.5f) * num48;
					bool flag4a = flag4 && i < num29;
					if (flag4a)
					{
						num45 *= Mathf.Lerp(0.72f, 0.88f, (float)random.NextDouble());
						num46 *= Mathf.Lerp(0.76f, 0.92f, (float)random.NextDouble());
						num47 *= Mathf.Lerp(0.82f, 0.94f, (float)random.NextDouble());
						float num48a = ((j % 2 == 0) ? (-1f) : 1f) * Mathf.Lerp(0.18f, 0.32f, (float)random.NextDouble());
						num49 = num38 + Mathf.Lerp(-0.16f, 0.16f, (float)random.NextDouble());
						num50 = num37 + num48a;
					}
					float halfX = num45 * 0.5f + num26;
					float halfZ = num46 * 0.5f + num26;
					Vector2 val13 = ClampFootprintCenterToMapInterior(num49, num50, halfX, halfZ, num27BoundaryPadding);
					num49 = val13.x;
					num50 = val13.y;
					if (OverlapsAnyFootprintRuntime(footprints, num49, num50, halfX, halfZ))
					{
						continue;
					}
					int hp = Mathf.Clamp(Mathf.RoundToInt(num47 * Mathf.Lerp(0.75f, 1.2f, num39)), 1, 10);
					PrimitiveType type = (PrimitiveType)((flag4a && random.NextDouble() < 0.18) ? 2 : ((flag4 && random.NextDouble() < 0.1) ? 2 : 3));
					Color blockColor = Color.Lerp(val, val2, num39);
					EnsureDestructiblePrimitive(orCreateDirectChild, $"Block_{num30:000}", type, new Vector3(num49, num47 * 0.5f, num50), new Vector3(num45, num47, num46), blockColor, hp);
					num30++;
					AddFootprintRuntime(footprints, num49, num50, halfX, halfZ);
					bool flag5b = !flag4 && (num45 >= 1.36f || num46 >= 1.36f);
					bool flag5d = !flag4 && num50 > num7 + 3.2f && Mathf.Abs(num49) < 4.4f;
					double num50a = (flag5 ? 0.26 : 0.38);
					if (flag5d)
					{
						num50a *= 0.3;
					}
					if (flag5b && random.NextDouble() < num50a)
					{
						int num51b = flag5 ? random.Next(1, 3) : random.Next(2, 4);
						bool flag5c = num45 >= num46;
						for (int n = 0; n < num51b; n++)
						{
							float num52b = ((float)n - (float)(num51b - 1) * 0.5f) * (flag5c ? 0.92f : 0.76f);
							float num53b = num49 + (flag5c ? num52b : (((n % 2 == 0) ? (-1f) : 1f) * (num45 * 0.5f + 0.72f)));
							float num54b = num50 + (flag5c ? (0f - (num46 * 0.5f + 0.68f)) : num52b);
							int num55c = random.Next(0, 6);
							Vector3 val13f = new Vector3(num53b, 0.5f, num54b);
							Vector3 val13g = new Vector3(Mathf.Lerp(0.62f, 1.08f, (float)random.NextDouble()), Mathf.Lerp(0.64f, 1.18f, (float)random.NextDouble()), Mathf.Lerp(0.44f, 0.84f, (float)random.NextDouble()));
							float halfX4 = 0.32f;
							float halfZ4 = 0.32f;
							if (num55c == 0)
							{
								halfX4 = Mathf.Max(0.22f, val13g.x * 0.5f);
								halfZ4 = Mathf.Max(0.18f, val13g.z * 0.5f);
							}
							else if (num55c == 1)
							{
								halfX4 = 0.5f;
								halfZ4 = 0.2f;
								val13f.y = 0.22f;
							}
							else if (num55c == 2)
							{
								halfX4 = 0.62f;
								halfZ4 = 0.24f;
								val13f.y = 0.63f;
							}
							else if (num55c == 3)
							{
								halfX4 = 0.34f;
								halfZ4 = 0.3f;
								val13f.y = 0.51f;
							}
							else if (num55c == 4)
							{
								halfX4 = 0.32f;
								halfZ4 = 0.32f;
								val13f.y = 0f;
							}
							else
							{
								halfX4 = 0.56f;
								halfZ4 = 0.4f;
								val13f.y = 0f;
							}
							Vector2 val13h = ClampFootprintCenterToMapInterior(val13f.x, val13f.z, halfX4, halfZ4, num27BoundaryPadding);
							val13f.x = val13h.x;
							val13f.z = val13h.y;
							if (OverlapsAnyFootprintRuntime(footprints, val13f.x, val13f.z, halfX4, halfZ4))
							{
								continue;
							}
							Color color4c = Color.Lerp(color3e, color3f, (float)random.NextDouble());
							string text = $"Prop_{num31:000}";
							if (num55c == 0)
							{
								EnsureCommercialKioskRuntime(orCreateDirectChild2, text, val13f, val13g, color4c);
							}
							else if (num55c == 1)
							{
								EnsureCommercialBenchRuntime(orCreateDirectChild2, text, val13f, color4c);
							}
							else if (num55c == 2)
							{
								EnsureCommercialBusStopRuntime(orCreateDirectChild2, text, val13f, color4c);
							}
							else if (num55c == 3)
							{
								EnsureCommercialVendingRuntime(orCreateDirectChild2, text, val13f, color4c);
							}
							else if (num55c == 4)
							{
								EnsureExplosiveBarrelRuntime(orCreateDirectChild2, text, new Vector3(val13f.x, 0f, val13f.z), color4c);
							}
							else
							{
								EnsureTransformerRuntime(orCreateDirectChild2, text, new Vector3(val13f.x, 0f, val13f.z), color4c);
							}
							AddFootprintRuntime(footprints, val13f.x, val13f.z, halfX4, halfZ4);
							num31++;
						}
					}
					if (flag5 && random.NextDouble() < (double)num25f)
					{
						Color color4b = Color.Lerp(color3e, color3f, (float)random.NextDouble());
						if (random.Next(0, 2) == 0)
						{
							EnsureCommercialAwningRuntime(orCreateDirectChild, $"ShopAwning_{num30:000}", new Vector3(num49, Mathf.Max(0.16f, num47 * 0.28f), num50 - num46 * 0.5f - 0.06f), new Vector3(Mathf.Clamp(num45 * 0.82f, 1f, 2.6f), 0.16f, 0.42f), color4b);
						}
						else
						{
							EnsureCommercialSignRuntime(orCreateDirectChild, $"ShopSign_{num30:000}", new Vector3(num49, Mathf.Max(0.3f, num47 * 0.46f), num50 - num46 * 0.5f - 0.05f), new Vector3(Mathf.Clamp(num45 * 0.58f, 0.8f, 1.8f), 0.42f, 0.12f), color4b);
						}
					}
					if (flag5 && num45 >= 1.45f && random.NextDouble() < (double)num25g)
					{
						int num51a = Mathf.Clamp(Mathf.RoundToInt(num45 / 0.82f), 2, 3);
						float num52a = Mathf.Clamp(num45 / Mathf.Max(2f, (float)num51a), 0.72f, 1.12f);
						float num53a = num50 - (num46 * 0.5f + 0.52f);
						float num54a = Mathf.Clamp(num45 + 0.34f, 1.8f, 3.6f);
						EnsurePrimitive(orCreateDirectChild4, $"RetailStrip_{num30:000}_{i:00}_{j:00}", (PrimitiveType)3, new Vector3(num49, 0.022f, num53a), new Vector3(num54a, 0.025f, 0.26f), new Color(color3e.r, color3e.g, color3e.b, 0.52f));
						EnsurePrimitive(orCreateDirectChild5, $"SidewalkPocket_{num30:000}_{i:00}_{j:00}", (PrimitiveType)3, new Vector3(num49, -0.006f, num53a), new Vector3(num54a, 0.012f, 0.62f), color3i);
						int num55b = Mathf.Clamp(Mathf.RoundToInt(num54a / 0.46f), 3, 6);
						for (int m = 0; m < num55b; m++)
						{
							float num56b = ((float)m - (float)(num55b - 1) * 0.5f) * 0.42f;
							EnsurePrimitive(orCreateDirectChild5, $"Crosswalk_{num30:000}_{i:00}_{j:00}_{m:00}", (PrimitiveType)3, new Vector3(num49 + num56b, -0.002f, num53a + 0.28f), new Vector3(0.22f, 0.01f, 0.12f), color3g);
						}
						EnsurePrimitive(orCreateDirectChild5, $"BusStopMark_{num30:000}_{i:00}_{j:00}", (PrimitiveType)3, new Vector3(num49, -0.001f, num53a - 0.24f), new Vector3(Mathf.Clamp(num54a * 0.72f, 1.3f, 2.6f), 0.01f, 0.08f), color3h);
						for (int l = 0; l < num51a; l++)
						{
							float num55a = ((float)l - (float)(num51a - 1) * 0.5f) * num52a;
							Vector3 val13c = new Vector3(num49 + num55a, 0.52f, num53a);
							Vector3 val13d = new Vector3(Mathf.Lerp(0.66f, 0.96f, (float)random.NextDouble()), Mathf.Lerp(0.72f, 1.06f, (float)random.NextDouble()), Mathf.Lerp(0.5f, 0.76f, (float)random.NextDouble()));
							float halfX2 = Mathf.Max(0.24f, val13d.x * 0.5f);
							float halfZ2 = Mathf.Max(0.2f, val13d.z * 0.5f);
							Vector2 val13e = ClampFootprintCenterToMapInterior(val13c.x, val13c.z, halfX2, halfZ2, num27BoundaryPadding);
							val13c.x = val13e.x;
							val13c.z = val13e.y;
							if (OverlapsAnyFootprintRuntime(footprints, val13c.x, val13c.z, halfX2, halfZ2))
							{
								continue;
							}
							Color color4c = Color.Lerp(color3e, color3f, (float)random.NextDouble());
							int num56a = random.Next(0, 5);
							if (num56a == 0)
							{
								EnsureCommercialKioskRuntime(orCreateDirectChild2, $"Prop_{num31:000}", val13c, val13d, color4c);
							}
							else if (num56a == 1)
							{
								EnsureCommercialAwningRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val13c.x, Mathf.Max(0.24f, val13d.y * 0.52f), val13c.z), new Vector3(Mathf.Clamp(val13d.x * 0.96f, 0.68f, 1.05f), 0.16f, Mathf.Clamp(val13d.z * 0.88f, 0.38f, 0.62f)), color4c);
							}
							else if (num56a == 2)
							{
								EnsureCommercialBenchRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val13c.x, 0.22f, val13c.z), color4c);
							}
							else if (num56a == 3)
							{
								EnsureCommercialBusStopRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val13c.x, 0.63f, val13c.z), color4c);
							}
							else
							{
								EnsureCommercialVendingRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val13c.x, 0.51f, val13c.z), color4c);
							}
							AddFootprintRuntime(footprints, val13c.x, val13c.z, halfX2, halfZ2);
							num31++;
						}
					}
					bool flag6 = flag4a;
					double num58b = flag6 ? Math.Min(0.98, num25a + 0.26) : num25a;
					double num59b = flag6 ? Math.Min(0.96, num25 + 0.2) : num25;
					double num60b = flag6 ? Math.Min(0.88, num25c + 0.22) : num25c;
					double num61b = flag6 ? Math.Min(0.8, num25d + 0.18) : num25d;
					if (flag6 && random.NextDouble() < num58b)
					{
						float num51 = Mathf.Lerp(0.48f, 0.92f, (float)random.NextDouble());
						float num52 = Mathf.Lerp(0.48f, 0.92f, (float)random.NextDouble());
						float num53 = Mathf.Lerp(0.32f, Mathf.Max(0.5f, num47 * 0.78f), (float)random.NextDouble());
						float num54 = ((random.NextDouble() < 0.5) ? (-1f) : 1f) * (num45 * 0.45f + num51 * 0.42f + 0.16f);
						float num55 = ((float)random.NextDouble() - 0.5f) * 0.48f;
						if (Mathf.Abs(num55) < 0.12f)
						{
							num55 = Mathf.Sign((num55 == 0f) ? 1f : num55) * 0.12f;
						}
						float num56 = num49 + num54;
						float num57 = num50 + num55;
						float halfX2 = num51 * 0.5f + Mathf.Max(0.18f, num26 * 0.5f);
						float halfZ2 = num52 * 0.5f + Mathf.Max(0.18f, num26 * 0.5f);
						Vector2 val13a = ClampFootprintCenterToMapInterior(num56, num57, halfX2, halfZ2, num27BoundaryPadding);
						num56 = val13a.x;
						num57 = val13a.y;
						if (!OverlapsAnyFootprintRuntime(footprints, num56, num57, halfX2, halfZ2))
						{
							Vector3 val13b = new Vector3(num51, num53, num52);
							Color color4a = Color.Lerp(blockColor, val7, 0.24f + (float)random.NextDouble() * 0.18f);
							EnsureDestructiblePrimitive(orCreateDirectChild, $"Block_{num30:000}", (PrimitiveType)3, new Vector3(num56, num53 * 0.5f, num57), val13b, color4a, Mathf.Max(1, hp - 1));
							num30++;
							AddFootprintRuntime(footprints, num56, num57, halfX2, halfZ2);
						}
					}
					if (flag6 && random.NextDouble() < num60b)
					{
						bool flag7a = random.Next(0, 2) == 0;
						int num58a = random.Next(2, 4);
						float num59a = 0.94f;
						Color color5a = Color.Lerp(val7, val8, (float)random.NextDouble());
						float num60a = num49;
						float num61a = num50;
						if (flag7a)
						{
							num61a += 0f - (num46 * 0.5f + 0.34f);
						}
						else
						{
							num60a += ((random.Next(0, 2) == 0) ? (-1f) : 1f) * (num45 * 0.5f + 0.34f);
						}
						for (int k = 0; k < num58a; k++)
						{
							float num62 = ((float)k - (float)(num58a - 1) * 0.5f) * num59a;
							Vector3 val14a = new Vector3(flag7a ? (num60a + num62) : num60a, 0.21f, flag7a ? num61a : (num61a + num62));
							float halfX2 = flag7a ? 0.52f : 0.12f;
							float halfZ2 = flag7a ? 0.12f : 0.52f;
							Vector2 val14b = ClampFootprintCenterToMapInterior(val14a.x, val14a.z, halfX2, halfZ2, num27BoundaryPadding);
							val14a.x = val14b.x;
							val14a.z = val14b.y;
							if (!OverlapsAnyFootprintRuntime(footprints, val14a.x, val14a.z, halfX2, halfZ2))
							{
								string text = $"Prop_{num31:000}";
								EnsureResidentialFenceRuntime(orCreateDirectChild2, text, val14a, color5a);
								num31++;
								AddFootprintRuntime(footprints, val14a.x, val14a.z, halfX2, halfZ2);
							}
						}
					}
					if (flag6 && random.NextDouble() < num61b)
					{
						bool flag7b = random.Next(0, 2) == 0;
						float num63 = flag7b ? Mathf.Clamp(num45 + 1.6f, 1.8f, 4.4f) : Mathf.Clamp(num46 + 1.6f, 1.8f, 4.4f);
						Vector3 val14c = flag7b ? new Vector3(num63, 0.03f, 0.22f) : new Vector3(0.22f, 0.03f, num63);
						Vector3 val14d = new Vector3(flag7b ? num49 : (num49 + ((random.Next(0, 2) == 0) ? (-1f) : 1f) * (num45 * 0.5f + 0.46f)), 0.02f, flag7b ? (num50 - (num46 * 0.5f + 0.46f)) : num50);
						EnsurePrimitive(orCreateDirectChild4, $"AlleyGuide_{num30:000}_{i:00}_{j:00}", (PrimitiveType)3, val14d, val14c, color3a);
					}
					if (flag4 && random.NextDouble() < num59b)
					{
						float num58 = Mathf.Lerp(0.28f, 0.85f, (float)random.NextDouble());
						PrimitiveType val14 = (PrimitiveType)((random.Next(0, 3) == 0) ? 2 : 3);
						Vector3 val16 = (((int)val14 == 2) ? new Vector3(num58 * 0.7f, num58, num58 * 0.7f) : new Vector3(num58, num58, num58));
						val15 = new Vector3(num49 + ((float)random.NextDouble() - 0.5f) * 1.6f, val16.y * 0.5f, num50 + ((float)random.NextDouble() - 0.5f) * 1.6f);
						Color color5 = Color.Lerp(val7, val8, (float)random.NextDouble());
						float halfX3 = Mathf.Max(0.22f, val16.x * 0.5f);
						float halfZ3 = Mathf.Max(0.22f, val16.z * 0.5f);
						if (flag6)
						{
							int num59a = random.Next(0, 4);
							float num60 = 0f - (num46 * 0.5f + halfZ3 + 0.18f);
							float num61 = ((random.Next(0, 2) == 0) ? (-1f) : 1f) * (num45 * 0.5f + halfX3 + 0.18f);
							switch (num59a)
							{
							case 0:
								val15.x = num49 + Mathf.Lerp(0f - num45 * 0.22f, num45 * 0.22f, (float)random.NextDouble());
								val15.z = num50 + num60;
								break;
							case 1:
								val15.x = num49 + num61;
								val15.z = num50 + Mathf.Lerp(0f - num46 * 0.24f, num46 * 0.24f, (float)random.NextDouble());
								break;
							case 2:
								val15.x = num49 + Mathf.Lerp(0f - num45 * 0.46f, num45 * 0.46f, (float)random.NextDouble());
								val15.z = num50 + num60 + Mathf.Lerp(-0.08f, 0.08f, (float)random.NextDouble());
								break;
							default:
								val15.x = num49 + num61;
								val15.z = num50 + num60 * 0.38f;
								break;
							}
						}
						Vector2 val17 = ClampFootprintCenterToMapInterior(val15.x, val15.z, halfX3, halfZ3, num27BoundaryPadding);
						val15.x = val17.x;
						val15.z = val17.y;
						if (!OverlapsAnyFootprintRuntime(footprints, val15.x, val15.z, halfX3, halfZ3))
						{
							string text = $"Prop_{num31:000}";
						bool flag7 = flag6 && random.NextDouble() < (double)Math.Min(0.9f, num25b + 0.12f);
							if (flag7)
							{
								int num59 = random.Next(0, 3);
								if (num59 == 0)
								{
									EnsureResidentialMailboxRuntime(orCreateDirectChild2, text, val15, new Color(0.34f, 0.28f, 0.22f), color5);
								}
								else if (num59 == 1)
								{
									EnsureResidentialFenceRuntime(orCreateDirectChild2, text, val15, color5);
								}
								else
								{
									Vector3 val18 = new Vector3(Mathf.Lerp(0.65f, 1.15f, (float)random.NextDouble()), Mathf.Lerp(0.52f, 0.9f, (float)random.NextDouble()), Mathf.Lerp(0.7f, 1.2f, (float)random.NextDouble()));
									EnsureResidentialShedRuntime(orCreateDirectChild2, text, val15, val18, color5);
								}
							}
							else
							{
								EnsureDestructiblePrimitive(orCreateDirectChild2, text, val14, val15, val16, color5, 1);
							}
							num31++;
						}
					}
					else if (flag5 && !flag5d && random.NextDouble() < (double)num25e)
					{
						Vector3 val18a = new Vector3(num49 + Mathf.Lerp(0f - num45 * 0.36f, num45 * 0.36f, (float)random.NextDouble()), 0.52f, num50 - (num46 * 0.5f + 0.48f));
						Vector3 val19 = new Vector3(Mathf.Lerp(0.7f, 1.25f, (float)random.NextDouble()), Mathf.Lerp(0.75f, 1.2f, (float)random.NextDouble()), Mathf.Lerp(0.52f, 0.9f, (float)random.NextDouble()));
						float halfX3 = Mathf.Max(0.26f, val19.x * 0.5f);
						float halfZ3 = Mathf.Max(0.22f, val19.z * 0.5f);
						Vector2 val20 = ClampFootprintCenterToMapInterior(val18a.x, val18a.z, halfX3, halfZ3, num27BoundaryPadding);
						val18a.x = val20.x;
						val18a.z = val20.y;
						if (!OverlapsAnyFootprintRuntime(footprints, val18a.x, val18a.z, halfX3, halfZ3))
						{
							Color color5b = Color.Lerp(color3e, color3f, (float)random.NextDouble());
							int num57a = random.Next(0, 4);
							if (num57a == 0)
							{
								EnsureCommercialKioskRuntime(orCreateDirectChild2, $"Prop_{num31:000}", val18a, val19, color5b);
							}
							else if (num57a == 1)
							{
								EnsureCommercialBenchRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val18a.x, 0.22f, val18a.z), color5b);
							}
							else if (num57a == 2)
							{
								EnsureCommercialBusStopRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val18a.x, 0.63f, val18a.z), color5b);
							}
							else
							{
								EnsureCommercialVendingRuntime(orCreateDirectChild2, $"Prop_{num31:000}", new Vector3(val18a.x, 0.51f, val18a.z), color5b);
							}
							AddFootprintRuntime(footprints, val18a.x, val18a.z, halfX3, halfZ3);
							num31++;
						}
					}
				}
			}
			EnsureStarterDestructionCluster(orCreateDirectChild, orCreateDirectChild2, orCreateDirectChild5, footprints, color3d, val7, val8, color3e, color3f, num27BoundaryPadding);
			EnsurePrimitive(orCreateDirectChild4, "Target_A", (PrimitiveType)2, new Vector3(-15f, 0.15f, 12f), new Vector3(1.5f, 0.15f, 1.5f), color3);
			EnsurePrimitive(orCreateDirectChild4, "Target_B", (PrimitiveType)2, new Vector3(15f, 0.15f, -12f), new Vector3(1.5f, 0.15f, 1.5f), color3);
			Debug.Log((object)$"[AlienCrusher] Runtime city theme generated: {runtimeCityThemeProfile}");
		}

		private void EnsureOpeningDestructionDensity(DummyDestructibleBlock[] existingDestructibles)
		{
			Transform mapRoot = FindChildByName(null, "MapRoot");
			if ((Object)(object)mapRoot == (Object)null || (Object)(object)FindChildByName(mapRoot, "StarterClusterBlock_00_00") != (Object)null)
			{
				return;
			}
			if (CountOpeningDestructibles(existingDestructibles, mapRoot) >= 18)
			{
				return;
			}
			Transform cityBlocksRoot = GetOrCreateDirectChild(mapRoot, "CityBlocks");
			Transform microPropsRoot = GetOrCreateDirectChild(mapRoot, "MicroProps");
			Transform groundDetailsRoot = GetOrCreateDirectChild(mapRoot, "GroundDetails");
			List<Vector4> openingFootprints = BuildOpeningFootprints(existingDestructibles, mapRoot);
			EnsureStarterDestructionCluster(cityBlocksRoot, microPropsRoot, groundDetailsRoot, openingFootprints, new Color(0.19f, 0.21f, 0.18f, 1f), new Color(0.88f, 0.92f, 0.96f), new Color(1f, 0.7f, 0.4f), new Color(0.95f, 0.46f, 0.28f), new Color(0.28f, 0.82f, 0.98f), 1.15f);
			Debug.Log((object)"[AlienCrusher] Opening density was low, so starter crush lane objects were generated at runtime.");
		}

		private static int CountOpeningDestructibles(DummyDestructibleBlock[] destructibles, Transform mapRoot)
		{
			if (destructibles == null || (Object)(object)mapRoot == (Object)null)
			{
				return 0;
			}
			int count = 0;
			for (int i = 0; i < destructibles.Length; i++)
			{
				DummyDestructibleBlock block = destructibles[i];
				if ((Object)(object)block == (Object)null || !block.transform.IsChildOf(mapRoot))
				{
					continue;
				}
				Vector3 localPosition = mapRoot.InverseTransformPoint(block.transform.position);
				if (Mathf.Abs(localPosition.x) <= 5.8f && localPosition.z >= -20.3f && localPosition.z <= -9.4f)
				{
					count++;
				}
			}
			return count;
		}

		private static List<Vector4> BuildOpeningFootprints(DummyDestructibleBlock[] destructibles, Transform mapRoot)
		{
			List<Vector4> footprints = new List<Vector4>(64);
			if (destructibles == null || (Object)(object)mapRoot == (Object)null)
			{
				return footprints;
			}
			for (int i = 0; i < destructibles.Length; i++)
			{
				DummyDestructibleBlock block = destructibles[i];
				if ((Object)(object)block == (Object)null || !block.transform.IsChildOf(mapRoot))
				{
					continue;
				}
				Renderer renderer = block.GetComponentInChildren<Renderer>();
				if ((Object)(object)renderer == (Object)null)
				{
					continue;
				}
				Bounds bounds = renderer.bounds;
				Vector3 center = mapRoot.InverseTransformPoint(bounds.center);
				if (Mathf.Abs(center.x) > 9.5f || center.z < -22f || center.z > -6f)
				{
					continue;
				}
				AddFootprintRuntime(footprints, center.x, center.z, Mathf.Max(0.2f, bounds.extents.x), Mathf.Max(0.2f, bounds.extents.z));
			}
			return footprints;
		}

		private static void EnsureStarterDestructionCluster(Transform cityBlocksRoot, Transform microPropsRoot, Transform groundDetailsRoot, List<Vector4> footprints, Color asphaltColor, Color smallBlockColorA, Color smallBlockColorB, Color commercialColorA, Color commercialColorB, float mapPadding)
		{
			if ((Object)(object)cityBlocksRoot == (Object)null || footprints == null)
			{
				return;
			}
			Vector2 val = default(Vector2);
			val = new Vector2(0f, -16.2f);
			EnsurePrimitive(groundDetailsRoot, "StarterParkingPad", (PrimitiveType)3, new Vector3(val.x, -0.012f, val.y), new Vector3(8.6f, 0.014f, 5.8f), asphaltColor);
			for (int i = 0; i < 5; i++)
			{
				float num = -2.8f + (float)i * 1.4f;
				EnsurePrimitive(groundDetailsRoot, $"StarterParkingStripe_{i:00}", (PrimitiveType)3, new Vector3(val.x + num, -0.002f, val.y + 0.58f), new Vector3(0.12f, 0.01f, 3.9f), new Color(0.94f, 0.94f, 0.9f, 0.9f));
			}
			EnsurePrimitive(groundDetailsRoot, "StarterLaneGuide", (PrimitiveType)3, new Vector3(val.x, -0.001f, val.y + 4.4f), new Vector3(2.4f, 0.012f, 7.4f), new Color(0.96f, 0.74f, 0.28f, 0.84f));
			EnsurePrimitive(groundDetailsRoot, "StarterLaneBorder_L", (PrimitiveType)3, new Vector3(val.x - 1.34f, 0.004f, val.y + 4.4f), new Vector3(0.12f, 0.016f, 7.45f), new Color(1f, 0.9f, 0.44f, 0.92f));
			EnsurePrimitive(groundDetailsRoot, "StarterLaneBorder_R", (PrimitiveType)3, new Vector3(val.x + 1.34f, 0.004f, val.y + 4.4f), new Vector3(0.12f, 0.016f, 7.45f), new Color(1f, 0.9f, 0.44f, 0.92f));
			for (int i = 0; i < 5; i++)
			{
				float num2a = -1.1f + (float)i * 0.55f;
				EnsurePrimitive(groundDetailsRoot, $"StarterLaneTick_{i:00}", (PrimitiveType)3, new Vector3(val.x + num2a, 0.006f, val.y + 2.45f), new Vector3(0.18f, 0.016f, 0.34f), new Color(1f, 0.95f, 0.72f, 0.94f));
			}
			for (int j = 0; j < 4; j++)
			{
				float num2b = -0.84f + (float)j * 0.56f;
				EnsurePrimitive(groundDetailsRoot, $"StarterLanePulse_{j:00}", (PrimitiveType)3, new Vector3(val.x + num2b, 0.005f, val.y + 5.92f), new Vector3(0.24f, 0.014f, 0.54f), new Color(1f, 0.82f, 0.34f, 0.9f));
			}
			EnsurePrimitive(groundDetailsRoot, "StarterArrowStem", (PrimitiveType)3, new Vector3(val.x, 0.02f, val.y + 6.05f), new Vector3(0.32f, 0.03f, 1.2f), commercialColorA);
			EnsurePrimitive(groundDetailsRoot, "StarterArrowHeadL", (PrimitiveType)3, new Vector3(val.x - 0.42f, 0.02f, val.y + 6.72f), new Vector3(0.32f, 0.03f, 0.9f), commercialColorB);
			EnsurePrimitive(groundDetailsRoot, "StarterArrowHeadR", (PrimitiveType)3, new Vector3(val.x + 0.42f, 0.02f, val.y + 6.72f), new Vector3(0.32f, 0.03f, 0.9f), commercialColorB);
			EnsurePrimitive(microPropsRoot, "StarterGuidePost_L", (PrimitiveType)2, new Vector3(val.x - 2.35f, 1.1f, val.y + 3.55f), new Vector3(0.16f, 1.1f, 0.16f), commercialColorA);
			EnsurePrimitive(microPropsRoot, "StarterGuidePost_R", (PrimitiveType)2, new Vector3(val.x + 2.35f, 1.1f, val.y + 3.55f), new Vector3(0.16f, 1.1f, 0.16f), commercialColorA);
			EnsurePrimitive(microPropsRoot, "StarterGuideGlow_L", (PrimitiveType)3, new Vector3(val.x - 2.35f, 2.08f, val.y + 3.55f), new Vector3(0.54f, 0.18f, 0.54f), commercialColorB);
			EnsurePrimitive(microPropsRoot, "StarterGuideGlow_R", (PrimitiveType)3, new Vector3(val.x + 2.35f, 2.08f, val.y + 3.55f), new Vector3(0.54f, 0.18f, 0.54f), commercialColorB);
			EnsurePrimitive(microPropsRoot, "StarterGuideCrossbar", (PrimitiveType)3, new Vector3(val.x, 2.24f, val.y + 3.55f), new Vector3(4.9f, 0.16f, 0.28f), Color.Lerp(commercialColorA, commercialColorB, 0.42f));
			for (int j = 0; j < 14; j++)
			{
				float num2 = (float)(j % 7) - 3f;
				float num3 = (float)(j / 7);
				Vector3 val2 = new Vector3(val.x + num2 * 0.92f, 0.32f, val.y + 0.76f + num3 * 0.96f);
				float num4 = 0.36f + num3 * 0.07f;
				Vector3 val3 = new Vector3(num4, num4, num4);
				float num5 = Mathf.Max(0.22f, val3.x * 0.5f);
				float num6 = Mathf.Max(0.22f, val3.z * 0.5f);
				Vector2 val4 = ClampFootprintCenterToMapInterior(val2.x, val2.z, num5, num6, mapPadding);
				val2.x = val4.x;
				val2.z = val4.y;
				if (!OverlapsAnyFootprintRuntime(footprints, val2.x, val2.z, num5, num6))
				{
					Color color = Color.Lerp(smallBlockColorA, smallBlockColorB, (float)j / 13f);
					EnsureDestructiblePrimitive(microPropsRoot, $"StarterLotProp_{j:00}", (PrimitiveType)((j % 4 == 0) ? 2 : 3), val2, val3, color, 1);
					AddFootprintRuntime(footprints, val2.x, val2.z, num5, num6);
				}
			}
			for (int k = 0; k < 4; k++)
			{
				for (int l = 0; l < 5; l++)
				{
					Vector3 val5 = new Vector3(val.x - 4.2f + (float)l * 1.68f, 0.46f + (float)k * 0.05f, val.y - 2.05f + (float)k * 1.22f);
					Vector3 val6 = new Vector3(0.72f + (float)k * 0.07f, 0.68f + (float)k * 0.12f, 0.74f + (float)l * 0.04f);
					float num7 = Mathf.Max(0.28f, val6.x * 0.5f + 0.22f);
					float num8 = Mathf.Max(0.28f, val6.z * 0.5f + 0.22f);
					Vector2 val7 = ClampFootprintCenterToMapInterior(val5.x, val5.z, num7, num8, mapPadding);
					val5.x = val7.x;
					val5.z = val7.y;
					if (OverlapsAnyFootprintRuntime(footprints, val5.x, val5.z, num7, num8))
					{
						continue;
					}
					Color color2 = Color.Lerp(smallBlockColorA, smallBlockColorB, (float)(k * 5 + l) / 19f);
					EnsureDestructiblePrimitive(cityBlocksRoot, $"StarterClusterBlock_{k:00}_{l:00}", (PrimitiveType)3, val5, val6, color2, 1 + k);
					AddFootprintRuntime(footprints, val5.x, val5.z, num7, num8);
				}
			}
			for (int m = 0; m < 4; m++)
			{
				Vector3 val8 = new Vector3(val.x + 3.4f, 0.48f, val.y - 1.35f + (float)m * 1.05f);
				float num9 = 0.42f;
				float num10 = 0.36f;
				Vector2 val9 = ClampFootprintCenterToMapInterior(val8.x, val8.z, num9, num10, mapPadding);
				val8.x = val9.x;
				val8.z = val9.y;
				if (!OverlapsAnyFootprintRuntime(footprints, val8.x, val8.z, num9, num10))
				{
					Color color3 = Color.Lerp(commercialColorA, commercialColorB, (float)m / 3f);
					if (m % 2 == 0)
					{
						EnsureCommercialVendingRuntime(microPropsRoot, $"StarterCommercial_{m:00}", val8, color3);
					}
					else
					{
						EnsureCommercialBenchRuntime(microPropsRoot, $"StarterCommercial_{m:00}", new Vector3(val8.x, 0.22f, val8.z), color3);
					}
					AddFootprintRuntime(footprints, val8.x, val8.z, num9, num10);
				}
			}
			for (int n = 0; n < 8; n++)
			{
				float num11 = ((float)n - 3.5f) * 0.62f;
				Vector3 val10 = new Vector3(val.x + num11, 0.28f, val.y + 4.82f + (float)(n % 2) * 0.38f);
				Vector3 val11 = new Vector3(0.34f + (float)(n % 2) * 0.06f, 0.34f + (float)(n % 2) * 0.06f, 0.34f + (float)(n % 2) * 0.06f);
				float num12 = Mathf.Max(0.2f, val11.x * 0.5f);
				float num13 = Mathf.Max(0.2f, val11.z * 0.5f);
				Vector2 val12 = ClampFootprintCenterToMapInterior(val10.x, val10.z, num12, num13, mapPadding);
				val10.x = val12.x;
				val10.z = val12.y;
				if (!OverlapsAnyFootprintRuntime(footprints, val10.x, val10.z, num12, num13))
				{
					Color color4 = Color.Lerp(smallBlockColorA, smallBlockColorB, (float)n / 7f);
					EnsureDestructiblePrimitive(microPropsRoot, $"StarterLaneProp_{n:00}", (PrimitiveType)((n % 3 == 0) ? 2 : 3), val10, val11, color4, 1);
					AddFootprintRuntime(footprints, val10.x, val10.z, num12, num13);
				}
			}
			for (int num14 = 0; num14 < 5; num14++)
			{
				Vector3 val13 = new Vector3(val.x - 2.3f + (float)num14 * 1.14f, 0.4f, val.y + 6.86f + (float)num14 * 0.24f);
				Vector3 val14 = new Vector3(0.56f + (float)(num14 % 2) * 0.08f, 0.54f + (float)num14 * 0.06f, 0.62f);
				float num15 = Mathf.Max(0.24f, val14.x * 0.5f + 0.16f);
				float num16 = Mathf.Max(0.24f, val14.z * 0.5f + 0.16f);
				Vector2 val15 = ClampFootprintCenterToMapInterior(val13.x, val13.z, num15, num16, mapPadding);
				val13.x = val15.x;
				val13.z = val15.y;
				if (!OverlapsAnyFootprintRuntime(footprints, val13.x, val13.z, num15, num16))
				{
					Color color5 = Color.Lerp(smallBlockColorA, smallBlockColorB, 0.28f + (float)num14 * 0.14f);
					EnsureDestructiblePrimitive(cityBlocksRoot, $"StarterLaneBlock_{num14:00}", (PrimitiveType)3, val13, val14, color5, 1);
					AddFootprintRuntime(footprints, val13.x, val13.z, num15, num16);
				}
			}
		}

		private RuntimeCityThemeProfile ResolveFallbackCityTheme(Transform mapRoot)
		{
			if (fallbackCityTheme != 0)
			{
				return fallbackCityTheme;
			}

			int num = Mathf.Max(1, currentStageNumber);
			if (num <= 2)
			{
				return RuntimeCityThemeProfile.GardenResidential;
			}
			if (num <= 5)
			{
				return RuntimeCityThemeProfile.DenseCore;
			}
			if (num <= 8)
			{
				return RuntimeCityThemeProfile.IndustrialHarbor;
			}

			int num2 = Environment.TickCount ^ (((Object)(object)mapRoot != (Object)null) ? (((Object)mapRoot).GetInstanceID() * 397) : 0) ^ num * 733;
			return (new System.Random(num2).Next(0, 2) == 0) ? RuntimeCityThemeProfile.DenseCore : RuntimeCityThemeProfile.IndustrialHarbor;
		}

		private static void EnsureStreetLampRuntime(Transform parent, string name, Vector3 localPosition)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			Transform orCreateDirectChild = GetOrCreateDirectChild(parent, name);
			orCreateDirectChild.localPosition = localPosition;
			orCreateDirectChild.localRotation = Quaternion.identity;
			orCreateDirectChild.localScale = Vector3.one;
			((Component)orCreateDirectChild).gameObject.SetActive(true);
			GameObject hitTarget = EnsurePrimitive(orCreateDirectChild, "Pole", (PrimitiveType)2, new Vector3(0f, 1.05f, 0f), new Vector3(0.08f, 1.05f, 0.08f), new Color(0.7f, 0.74f, 0.8f));
			EnsurePrimitive(orCreateDirectChild, "Head", (PrimitiveType)3, new Vector3(0f, 2.1f, 0f), new Vector3(0.36f, 0.16f, 0.36f), new Color(1f, 0.9f, 0.55f));
			EnsureStreetPropReactiveRuntime(hitTarget, orCreateDirectChild, DummyStreetPropReactive.PropKind.Lamp);
		}

		private static void EnsureStreetTreeRuntime(Transform parent, string name, Vector3 localPosition, Color leafColor)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			Transform orCreateDirectChild = GetOrCreateDirectChild(parent, name);
			orCreateDirectChild.localPosition = localPosition;
			orCreateDirectChild.localRotation = Quaternion.identity;
			orCreateDirectChild.localScale = Vector3.one;
			((Component)orCreateDirectChild).gameObject.SetActive(true);
			GameObject hitTarget = EnsurePrimitive(orCreateDirectChild, "Trunk", (PrimitiveType)2, new Vector3(0f, 0.55f, 0f), new Vector3(0.18f, 0.55f, 0.18f), new Color(0.35f, 0.25f, 0.16f));
			EnsurePrimitive(orCreateDirectChild, "Leaves", (PrimitiveType)0, new Vector3(0f, 1.35f, 0f), new Vector3(0.95f, 0.95f, 0.95f), leafColor);
			EnsureStreetPropReactiveRuntime(hitTarget, orCreateDirectChild, DummyStreetPropReactive.PropKind.Tree);
		}

		private static void EnsureExplosiveBarrelRuntime(Transform parent, string name, Vector3 localPosition, Color bodyColor)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			Transform orCreateDirectChild = GetOrCreateDirectChild(parent, name);
			orCreateDirectChild.localPosition = localPosition;
			orCreateDirectChild.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			orCreateDirectChild.localScale = Vector3.one;
			((Component)orCreateDirectChild).gameObject.SetActive(true);
			GameObject hitTarget = EnsurePrimitive(orCreateDirectChild, "Body", (PrimitiveType)2, new Vector3(0f, 0.42f, 0f), new Vector3(0.55f, 0.42f, 0.55f), bodyColor);
			EnsurePrimitive(orCreateDirectChild, "Band_A", (PrimitiveType)2, new Vector3(0f, 0.61f, 0f), new Vector3(0.57f, 0.05f, 0.57f), Color.Lerp(bodyColor, Color.white, 0.22f));
			EnsurePrimitive(orCreateDirectChild, "Band_B", (PrimitiveType)2, new Vector3(0f, 0.23f, 0f), new Vector3(0.57f, 0.05f, 0.57f), Color.Lerp(bodyColor, Color.black, 0.18f));
			EnsurePrimitive(orCreateDirectChild, "Core", (PrimitiveType)0, new Vector3(0f, 0.42f, 0f), new Vector3(0.22f, 0.22f, 0.22f), Color.Lerp(bodyColor, new Color(1f, 0.86f, 0.3f), 0.4f));
			EnsureStreetPropReactiveRuntime(hitTarget, orCreateDirectChild, DummyStreetPropReactive.PropKind.ChainBarrel);
		}

		private static void EnsureTransformerRuntime(Transform parent, string name, Vector3 localPosition, Color baseColor)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			Transform orCreateDirectChild = GetOrCreateDirectChild(parent, name);
			orCreateDirectChild.localPosition = localPosition;
			orCreateDirectChild.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			orCreateDirectChild.localScale = Vector3.one;
			((Component)orCreateDirectChild).gameObject.SetActive(true);
			GameObject hitTarget = EnsurePrimitive(orCreateDirectChild, "Base", (PrimitiveType)3, new Vector3(0f, 0.36f, 0f), new Vector3(1.05f, 0.72f, 0.72f), baseColor);
			EnsurePrimitive(orCreateDirectChild, "Top", (PrimitiveType)3, new Vector3(0f, 0.82f, 0f), new Vector3(0.82f, 0.18f, 0.52f), Color.Lerp(baseColor, Color.white, 0.28f));
			EnsurePrimitive(orCreateDirectChild, "Pole_L", (PrimitiveType)2, new Vector3(-0.26f, 0.95f, 0f), new Vector3(0.08f, 0.32f, 0.08f), new Color(0.74f, 0.76f, 0.8f));
			EnsurePrimitive(orCreateDirectChild, "Pole_R", (PrimitiveType)2, new Vector3(0.26f, 0.95f, 0f), new Vector3(0.08f, 0.32f, 0.08f), new Color(0.74f, 0.76f, 0.8f));
			EnsurePrimitive(orCreateDirectChild, "Core", (PrimitiveType)0, new Vector3(0f, 0.78f, 0f), new Vector3(0.24f, 0.24f, 0.24f), Color.Lerp(baseColor, new Color(1f, 0.86f, 0.36f), 0.48f));
			EnsureStreetPropReactiveRuntime(hitTarget, orCreateDirectChild, DummyStreetPropReactive.PropKind.Transformer);
		}

		private static void EnsureResidentialMailboxRuntime(Transform parent, string name, Vector3 localPosition, Color postColor, Color boxColor)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, new Vector3(0.36f, 0.34f, 0.3f), boxColor, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "Post", (PrimitiveType)2, new Vector3(0f, -0.18f, 0f), new Vector3(0.08f, 0.18f, 0.08f), postColor);
			EnsurePrimitive(transform, "Flag", (PrimitiveType)3, new Vector3(0.19f, 0.06f, 0f), new Vector3(0.06f, 0.16f, 0.04f), Color.Lerp(boxColor, Color.white, 0.22f));
		}

		private static void EnsureResidentialFenceRuntime(Transform parent, string name, Vector3 localPosition, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, new Vector3(0.9f, 0.42f, 0.14f), color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "RailTop", (PrimitiveType)3, new Vector3(0f, 0.16f, 0f), new Vector3(0.94f, 0.08f, 0.06f), Color.Lerp(color, Color.white, 0.12f));
			EnsurePrimitive(transform, "RailBottom", (PrimitiveType)3, new Vector3(0f, -0.08f, 0f), new Vector3(0.94f, 0.08f, 0.06f), Color.Lerp(color, Color.black, 0.08f));
			EnsurePrimitive(transform, "Post_L", (PrimitiveType)3, new Vector3(-0.34f, 0f, 0f), new Vector3(0.08f, 0.48f, 0.08f), Color.Lerp(color, Color.black, 0.12f));
			EnsurePrimitive(transform, "Post_R", (PrimitiveType)3, new Vector3(0.34f, 0f, 0f), new Vector3(0.08f, 0.48f, 0.08f), Color.Lerp(color, Color.black, 0.12f));
		}

		private static void EnsureResidentialShedRuntime(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, scale, color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "Roof", (PrimitiveType)3, new Vector3(0f, scale.y * 0.62f, 0f), new Vector3(scale.x * 1.08f, scale.y * 0.18f, scale.z * 1.08f), Color.Lerp(color, Color.black, 0.18f));
			EnsurePrimitive(transform, "Door", (PrimitiveType)3, new Vector3(0f, 0f - scale.y * 0.08f, scale.z * 0.5f + 0.01f), new Vector3(scale.x * 0.28f, scale.y * 0.55f, 0.04f), Color.Lerp(color, Color.black, 0.24f));
		}

		private static void EnsureCommercialAwningRuntime(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, scale, color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "AwningTrim", (PrimitiveType)3, new Vector3(0f, scale.y * 0.38f, 0f), new Vector3(scale.x * 1.06f, scale.y * 0.24f, scale.z * 0.55f), Color.Lerp(color, Color.white, 0.18f));
			EnsurePrimitive(transform, "Bracket_L", (PrimitiveType)3, new Vector3(0f - scale.x * 0.38f, 0f - scale.y * 0.2f, scale.z * 0.08f), new Vector3(0.08f, scale.y * 0.72f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
			EnsurePrimitive(transform, "Bracket_R", (PrimitiveType)3, new Vector3(scale.x * 0.38f, 0f - scale.y * 0.2f, scale.z * 0.08f), new Vector3(0.08f, scale.y * 0.72f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
		}

		private static void EnsureCommercialSignRuntime(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, scale, color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "SignFace", (PrimitiveType)3, new Vector3(0f, 0f, 0f - scale.z * 0.1f), new Vector3(scale.x * 0.92f, scale.y * 0.8f, scale.z * 1.4f), Color.Lerp(color, Color.white, 0.24f));
			EnsurePrimitive(transform, "BackPlate", (PrimitiveType)3, new Vector3(0f, 0f, scale.z * 0.1f), new Vector3(scale.x, scale.y, scale.z * 0.75f), Color.Lerp(color, Color.black, 0.18f));
			EnsurePrimitive(transform, "Mount", (PrimitiveType)3, new Vector3(0f, 0f - scale.y * 0.62f, scale.z * 0.08f), new Vector3(scale.x * 0.18f, scale.y * 0.48f, scale.z * 0.7f), Color.Lerp(color, Color.black, 0.3f));
		}

		private static void EnsureCommercialKioskRuntime(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, scale, color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "ShopRoof", (PrimitiveType)3, new Vector3(0f, scale.y * 0.52f, 0f), new Vector3(scale.x * 1.12f, scale.y * 0.18f, scale.z * 1.12f), Color.Lerp(color, Color.black, 0.22f));
			EnsurePrimitive(transform, "ShopCounter", (PrimitiveType)3, new Vector3(0f, 0f - scale.y * 0.08f, scale.z * 0.5f + 0.03f), new Vector3(scale.x * 0.76f, scale.y * 0.32f, 0.08f), Color.Lerp(color, Color.white, 0.12f));
			EnsurePrimitive(transform, "ShopStripe", (PrimitiveType)3, new Vector3(0f, scale.y * 0.14f, scale.z * 0.5f + 0.02f), new Vector3(scale.x * 0.82f, scale.y * 0.18f, 0.05f), Color.Lerp(color, Color.white, 0.28f));
		}

		private static void EnsureCommercialBenchRuntime(Transform parent, string name, Vector3 localPosition, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, new Vector3(0.92f, 0.34f, 0.32f), color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "BenchSeat", (PrimitiveType)3, new Vector3(0f, 0.02f, 0f), new Vector3(0.98f, 0.08f, 0.34f), Color.Lerp(color, Color.white, 0.12f));
			EnsurePrimitive(transform, "BenchBack", (PrimitiveType)3, new Vector3(0f, 0.16f, -0.1f), new Vector3(0.98f, 0.22f, 0.08f), Color.Lerp(color, Color.black, 0.1f));
			EnsurePrimitive(transform, "BenchLeg_L", (PrimitiveType)3, new Vector3(-0.28f, -0.12f, 0f), new Vector3(0.08f, 0.22f, 0.08f), Color.Lerp(color, Color.black, 0.22f));
			EnsurePrimitive(transform, "BenchLeg_R", (PrimitiveType)3, new Vector3(0.28f, -0.12f, 0f), new Vector3(0.08f, 0.22f, 0.08f), Color.Lerp(color, Color.black, 0.22f));
		}

		private static void EnsureCommercialBusStopRuntime(Transform parent, string name, Vector3 localPosition, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, new Vector3(1.08f, 1.26f, 0.32f), color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "StopRoof", (PrimitiveType)3, new Vector3(0f, 0.48f, 0f), new Vector3(1.12f, 0.08f, 0.42f), Color.Lerp(color, Color.black, 0.16f));
			EnsurePrimitive(transform, "StopPanel", (PrimitiveType)3, new Vector3(0f, 0f, -0.07f), new Vector3(0.76f, 0.74f, 0.06f), Color.Lerp(color, Color.white, 0.22f));
			EnsurePrimitive(transform, "StopPole_L", (PrimitiveType)3, new Vector3(-0.34f, 0f, 0f), new Vector3(0.08f, 0.96f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
			EnsurePrimitive(transform, "StopPole_R", (PrimitiveType)3, new Vector3(0.34f, 0f, 0f), new Vector3(0.08f, 0.96f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
		}

		private static void EnsureCommercialVendingRuntime(Transform parent, string name, Vector3 localPosition, Color color)
		{
			GameObject gameObject = EnsureDestructiblePrimitive(parent, name, (PrimitiveType)3, localPosition, new Vector3(0.58f, 1.02f, 0.52f), color, 1);
			Transform transform = gameObject.transform;
			EnsurePrimitive(transform, "VendFace", (PrimitiveType)3, new Vector3(0f, 0.04f, 0.27f), new Vector3(0.46f, 0.78f, 0.04f), Color.Lerp(color, Color.white, 0.26f));
			EnsurePrimitive(transform, "VendSlot", (PrimitiveType)3, new Vector3(0f, -0.28f, 0.28f), new Vector3(0.28f, 0.08f, 0.05f), Color.Lerp(color, Color.black, 0.24f));
			EnsurePrimitive(transform, "VendCap", (PrimitiveType)3, new Vector3(0f, 0.44f, 0f), new Vector3(0.62f, 0.08f, 0.56f), Color.Lerp(color, Color.black, 0.18f));
		}

		private static void EnsureStreetPropReactiveRuntime(GameObject hitTarget, Transform propRoot, DummyStreetPropReactive.PropKind kind)
		{
			if (!((Object)(object)hitTarget == (Object)null) && !((Object)(object)propRoot == (Object)null))
			{
				DummyStreetPropReactive dummyStreetPropReactive = hitTarget.GetComponent<DummyStreetPropReactive>();
				if ((Object)(object)dummyStreetPropReactive == (Object)null)
				{
					dummyStreetPropReactive = hitTarget.AddComponent<DummyStreetPropReactive>();
				}
				dummyStreetPropReactive.ConfigureForScaffolder(kind, propRoot);
			}
		}

		private static bool OverlapsAnyFootprintRuntime(List<Vector4> footprints, float x, float z, float halfX, float halfZ)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < footprints.Count; i++)
			{
				Vector4 val = footprints[i];
				if (Mathf.Abs(val.x - x) <= val.z + halfX && Mathf.Abs(val.y - z) <= val.w + halfZ)
				{
					return true;
				}
			}
			return false;
		}

		private static void AddFootprintRuntime(List<Vector4> footprints, float x, float z, float halfX, float halfZ)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			footprints.Add(new Vector4(x, z, halfX, halfZ));
		}

		private static Vector2 ClampFootprintCenterToMapInterior(float x, float z, float halfX, float halfZ, float padding)
		{
			float num = Mathf.Max(0.5f, 25f - Mathf.Max(0f, padding) - Mathf.Max(0f, halfX));
			float num2 = Mathf.Max(0.5f, 25f - Mathf.Max(0f, padding) - Mathf.Max(0f, halfZ));
			return new Vector2(Mathf.Clamp(x, -num, num), Mathf.Clamp(z, -num2, num2));
		}

		private static void ClearChildrenRuntime(Transform root)
		{
			if ((Object)(object)root == (Object)null)
			{
				return;
			}
			for (int num = root.childCount - 1; num >= 0; num--)
			{
				Transform child = root.GetChild(num);
				if (!((Object)(object)child == (Object)null))
				{
					if (Application.isPlaying)
					{
						Object.Destroy((Object)(object)((Component)child).gameObject);
					}
					else
					{
						Object.DestroyImmediate((Object)(object)((Component)child).gameObject);
					}
				}
			}
		}

		private void EnsureSpawnPointFallback(Transform gameplayRoot)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			Transform orCreateDirectChild = GetOrCreateDirectChild(GetOrCreateDirectChild(gameplayRoot, "SpawnRoot"), "PlayerSpawn");
			orCreateDirectChild.localPosition = new Vector3(0f, 0.6f, -18f);
			orCreateDirectChild.localRotation = Quaternion.identity;
		}
	}
}
