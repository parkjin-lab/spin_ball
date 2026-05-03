#if UNITY_EDITOR
using AlienCrusher.Gameplay;
using AlienCrusher.Systems;
using AlienCrusher.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace AlienCrusher.EditorTools
{
    public static class AlienCrusherSceneScaffolder
    {
        private const string MenuPath = "Tools/Alien Crusher/Generate Dummy Scene Structure";
        private const string StylizedMaterialPath = "Assets/Art/Materials/M_AlienCrusher_StylizedLowPoly.mat";

        private static Material cachedStylizedMaterial;
        private enum CityThemeProfile
        {
            DenseCore = 0,
            IndustrialHarbor = 1,
            GardenResidential = 2
        }

        [MenuItem(MenuPath)]
        public static void GenerateDummySceneStructure()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[AlienCrusher] No valid active scene found.");
                return;
            }

            var appRoot = CreateOrGetRoot("_App");
            var systemsRoot = CreateOrGetRoot("_Systems");
            var gameplayRoot = CreateOrGetRoot("_Gameplay");
            var uiRoot = CreateOrGetRoot("_UI");
            var debugRoot = CreateOrGetRoot("_Debug");

            ConfigureAppRoot(appRoot.transform);
            ConfigureSystems(systemsRoot.transform);
            ConfigureGameplay(gameplayRoot.transform);
            ConfigureUi(uiRoot.transform);
            ConfigureDebug(debugRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = uiRoot;
            Debug.Log("[AlienCrusher] Dummy hierarchy, joystick, and map scaffold generated.");
        }

        private static void ConfigureAppRoot(Transform parent)
        {
            CreateOrGetChild(parent, "Bootstrap");
            CreateOrGetChild(parent, "AddressablesPlaceholder");
        }

        private static void ConfigureSystems(Transform parent)
        {
            AddComponentIfMissing<GameFlowSystem>(CreateOrGetChild(parent, "GameFlowSystem"));
            AddComponentIfMissing<ScoreSystem>(CreateOrGetChild(parent, "ScoreSystem"));
            AddComponentIfMissing<FeedbackSystem>(CreateOrGetChild(parent, "FeedbackSystem"));
            AddComponentIfMissing<DamageNumberSystem>(CreateOrGetChild(parent, "DamageNumberSystem"));
            AddComponentIfMissing<ProgressionSaveSystem>(CreateOrGetChild(parent, "ProgressionSaveSystem"));
            AddComponentIfMissing<FormUnlockSystem>(CreateOrGetChild(parent, "FormUnlockSystem"));
            AddComponentIfMissing<BallGrowthSystem>(CreateOrGetChild(parent, "BallGrowthSystem"));
            AddComponentIfMissing<CameraFollowSystem>(CreateOrGetChild(parent, "CameraFollowSystem"));
            AddComponentIfMissing<DummyFlowController>(CreateOrGetChild(parent, "DummyFlowController"));
        }

        private static void ConfigureGameplay(Transform parent)
        {
            var playerBall = FindChild(parent, "PlayerBall");
            if (playerBall == null)
            {
                playerBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                playerBall.name = "PlayerBall";
                playerBall.transform.SetParent(parent, false);
            }

            playerBall.transform.localScale = Vector3.one;

            var rigidbody = AddComponentIfMissing<Rigidbody>(playerBall);
            rigidbody.mass = 10f;
            rigidbody.linearDamping = 0.5f;
            rigidbody.angularDamping = 0.05f;

            AddComponentIfMissing<PlayerBallDummyController>(playerBall);

            var mapRoot = CreateOrGetChild(parent, "MapRoot");
            var spawnRoot = CreateOrGetChild(parent, "SpawnRoot");

            ConfigureDummyMap(mapRoot.transform);
            RepairZeroScaleMapObjects(mapRoot.transform);
            ConfigureSpawnPoints(spawnRoot.transform, playerBall.transform, rigidbody);
            ConfigureGameplayCamera(parent);
            EnsureGameplayLighting(parent);
        }

        private static void ConfigureDummyMap(Transform mapRoot)
        {
            const float mapSize = 50f;
            const float wallHeight = 4f;
            const float wallThickness = 1f;
            const float half = mapSize * 0.5f;

            var theme = PickEditorCityTheme(mapRoot);
            var rng = new System.Random(2117 + (int)theme * 97);

            var groundColor = new Color(0.06f, 0.09f, 0.14f);
            var wallColor = new Color(0.14f, 0.19f, 0.28f);
            var blockNearColor = new Color(0.7f, 0.76f, 0.84f);
            var blockFarColor = new Color(0.92f, 0.75f, 0.46f);
            var carColorA = new Color(0.86f, 0.24f, 0.2f);
            var carColorB = new Color(0.18f, 0.62f, 0.92f);
            var treeColorA = new Color(0.24f, 0.55f, 0.27f);
            var treeColorB = new Color(0.42f, 0.72f, 0.35f);
            var clutterColorA = new Color(0.82f, 0.87f, 0.95f);
            var clutterColorB = new Color(1f, 0.62f, 0.29f);
            var chainColorA = new Color(0.98f, 0.38f, 0.18f);
            var chainColorB = new Color(1f, 0.74f, 0.24f);

            var roadCarChance = 0.32f;
            var roadLampChance = 0.22f;
            var roadTreeChance = 0.2f;
            var roadBarrelChance = 0.1f;
            var roadTransformerChance = 0.05f;
            var lowDistrictEndZ = 2f;
            var midDistrictEndZ = 12f;
            var lowSkipThreshold = 0.995f;
            var midSkipThreshold = 0.88f;
            var highSkipThreshold = 0.56f;
            var lowFootMin = 0.85f;
            var lowFootMax = 1.65f;
            var midFootMin = 1.8f;
            var midFootMax = 3.1f;
            var highFootMin = 2.3f;
            var highFootMax = 4.2f;
            var lowHeightMin = 0.55f;
            var lowHeightMax = 1.45f;
            var midHeightMin = 1.8f;
            var midHeightMax = 4.5f;
            var highHeightMin = 4.3f;
            var highHeightMax = 8.8f;
            var lowPosJitter = 0.08f;
            var otherPosJitter = 0.18f;
            var lowClutterChance = 0.58f;
            var lowAnnexChance = 0.42f;
            var lowResidentialPropChance = 0.46f;
            var lowFenceLineChance = 0.28f;
            var lowAlleyGuideChance = 0.24f;
            var midCommercialPropChance = 0.34f;
            var midFacadeAccentChance = 0.42f;
            var midRetailStripChance = 0.3f;
            var buildingSpacing = 0.62f;
            var targetColor = new Color(0.97f, 0.62f, 0.25f);
            var alleyGuideColor = new Color(1f, 0.74f, 0.34f, 0.72f);
            var roadColor = new Color(0.12f, 0.14f, 0.17f, 1f);
            var sidewalkColor = new Color(0.32f, 0.34f, 0.37f, 1f);
            var yardColor = new Color(0.16f, 0.18f, 0.2f, 1f);
            var crosswalkColor = new Color(0.92f, 0.92f, 0.9f, 0.92f);
            var busStopMarkColor = new Color(0.26f, 0.82f, 0.94f, 0.88f);
            var sidewalkPocketColor = new Color(0.54f, 0.56f, 0.58f, 0.9f);
            var commercialAccentA = new Color(0.92f, 0.34f, 0.22f, 1f);
            var commercialAccentB = new Color(0.22f, 0.72f, 0.96f, 1f);

            switch (theme)
            {
                case CityThemeProfile.IndustrialHarbor:
                    groundColor = new Color(0.08f, 0.1f, 0.12f);
                    wallColor = new Color(0.18f, 0.2f, 0.24f);
                    blockNearColor = new Color(0.66f, 0.69f, 0.75f);
                    blockFarColor = new Color(0.81f, 0.74f, 0.66f);
                    carColorA = new Color(0.91f, 0.43f, 0.16f);
                    carColorB = new Color(0.3f, 0.49f, 0.78f);
                    treeColorA = new Color(0.18f, 0.42f, 0.21f);
                    treeColorB = new Color(0.28f, 0.54f, 0.27f);
                    clutterColorA = new Color(0.74f, 0.79f, 0.85f);
                    clutterColorB = new Color(0.94f, 0.54f, 0.22f);
                    chainColorA = new Color(0.92f, 0.33f, 0.14f);
                    chainColorB = new Color(1f, 0.64f, 0.2f);
                    roadCarChance = 0.42f;
                    roadLampChance = 0.27f;
                    roadTreeChance = 0.09f;
                    roadBarrelChance = 0.16f;
                    roadTransformerChance = 0.08f;
                    midSkipThreshold = 0.83f;
                    highSkipThreshold = 0.5f;
                    highFootMin = 2.5f;
                    highFootMax = 5f;
                    highHeightMin = 4.8f;
                    highHeightMax = 10.5f;
                    lowClutterChance = 0.46f;
                    targetColor = new Color(1f, 0.68f, 0.3f);
                    break;
                case CityThemeProfile.GardenResidential:
                    groundColor = new Color(0.1f, 0.14f, 0.1f);
                    wallColor = new Color(0.17f, 0.24f, 0.18f);
                    blockNearColor = new Color(0.81f, 0.87f, 0.92f);
                    blockFarColor = new Color(0.98f, 0.9f, 0.72f);
                    carColorA = new Color(0.92f, 0.36f, 0.3f);
                    carColorB = new Color(0.26f, 0.74f, 0.9f);
                    treeColorA = new Color(0.2f, 0.58f, 0.24f);
                    treeColorB = new Color(0.5f, 0.78f, 0.42f);
                    clutterColorA = new Color(0.88f, 0.92f, 0.96f);
                    clutterColorB = new Color(1f, 0.7f, 0.4f);
                    chainColorA = new Color(1f, 0.42f, 0.22f);
                    chainColorB = new Color(1f, 0.78f, 0.36f);
                    roadCarChance = 0.24f;
                    roadLampChance = 0.28f;
                    roadTreeChance = 0.34f;
                    roadBarrelChance = 0.07f;
                    roadTransformerChance = 0.03f;
                    lowDistrictEndZ = 3f;
                    midDistrictEndZ = 11f;
                    midSkipThreshold = 0.9f;
                    highSkipThreshold = 0.68f;
                    lowHeightMax = 1.3f;
                    highHeightMin = 3.7f;
                    highHeightMax = 7.2f;
                    lowClutterChance = 0.72f;
                    lowAnnexChance = 0.52f;
                    lowResidentialPropChance = 0.58f;
                    lowFenceLineChance = 0.36f;
                    targetColor = new Color(0.96f, 0.7f, 0.34f);
                    alleyGuideColor = new Color(0.98f, 0.78f, 0.4f, 0.72f);
                    roadColor = new Color(0.16f, 0.17f, 0.19f, 1f);
                    sidewalkColor = new Color(0.38f, 0.39f, 0.4f, 1f);
                    yardColor = new Color(0.19f, 0.21f, 0.18f, 1f);
                    crosswalkColor = new Color(0.94f, 0.93f, 0.88f, 0.92f);
                    busStopMarkColor = new Color(0.3f, 0.86f, 0.98f, 0.88f);
                    sidewalkPocketColor = new Color(0.62f, 0.64f, 0.62f, 0.9f);
                    midCommercialPropChance = 0.28f;
                    midFacadeAccentChance = 0.34f;
                    midRetailStripChance = 0.38f;
                    commercialAccentA = new Color(0.95f, 0.46f, 0.28f, 1f);
                    commercialAccentB = new Color(0.28f, 0.82f, 0.98f, 1f);
                    break;
            }

            EnsureCube(mapRoot, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(mapSize, 1f, mapSize), groundColor);
            EnsureCube(mapRoot, "Wall_North", new Vector3(0f, wallHeight * 0.5f, half), new Vector3(mapSize, wallHeight, wallThickness), wallColor);
            EnsureCube(mapRoot, "Wall_South", new Vector3(0f, wallHeight * 0.5f, -half), new Vector3(mapSize, wallHeight, wallThickness), wallColor);
            EnsureCube(mapRoot, "Wall_East", new Vector3(half, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, mapSize), wallColor);
            EnsureCube(mapRoot, "Wall_West", new Vector3(-half, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, mapSize), wallColor);

            var cityBlocks = CreateOrGetChild(mapRoot, "CityBlocks").transform;
            var microProps = CreateOrGetChild(mapRoot, "MicroProps").transform;
            var streetProps = CreateOrGetChild(mapRoot, "StreetProps").transform;
            var targetMarkers = CreateOrGetChild(mapRoot, "TargetMarkers").transform;
            var groundDetails = CreateOrGetChild(mapRoot, "GroundDetails").transform;

            ClearChildrenImmediate(cityBlocks);
            ClearChildrenImmediate(microProps);
            ClearChildrenImmediate(streetProps);
            ClearChildrenImmediate(targetMarkers);
            ClearChildrenImmediate(groundDetails);

            const int xCells = 18;
            const int zCells = 15;
            const float cellSize = 2.9f;
            var startX = -((xCells - 1) * cellSize) * 0.5f;
            var startZ = -20f;
            var earlyZoneRows = Mathf.CeilToInt(zCells * 0.45f);

            var blockIndex = 0;
            var clutterIndex = 0;
            var carIndex = 0;
            var lampIndex = 0;
            var treeIndex = 0;
            var barrelIndex = 0;
            var transformerIndex = 0;
            var buildingFootprints = new System.Collections.Generic.List<Vector4>(xCells * zCells);

            for (var zi = 0; zi < zCells; zi++)
            {
                var z = startZ + zi * cellSize;
                var roadRow = zi % 5 == 2;

                for (var xi = 0; xi < xCells; xi++)
                {
                    var x = startX + xi * cellSize;
                    var roadCol = xi % 6 == 3;
                    var spawnLane = z < -13.5f && Mathf.Abs(x) < 5.4f;
                    var sidewalkScale = new Vector3(cellSize * 0.92f, 0.02f, cellSize * 0.92f);

                    if (roadRow || roadCol)
                    {
                        EnsureCube(groundDetails, $"RoadTile_{zi:00}_{xi:00}", new Vector3(x, -0.01f, z), sidewalkScale, roadColor);
                    }
                    else if (!spawnLane)
                    {
                        var districtBlend = Mathf.InverseLerp(-20f, 20f, z);
                        var tileColor = Color.Lerp(sidewalkColor, yardColor, districtBlend * 0.65f);
                        EnsureCube(groundDetails, $"LotTile_{zi:00}_{xi:00}", new Vector3(x, -0.015f, z), new Vector3(cellSize * 0.86f, 0.015f, cellSize * 0.86f), tileColor);
                    }

                    if (roadRow || roadCol || spawnLane)
                    {
                        if ((roadRow || roadCol) && rng.NextDouble() < roadCarChance)
                        {
                            var carColor = Color.Lerp(carColorA, carColorB, (float)rng.NextDouble());
                            var yaw = rng.Next(0, 2) == 0 ? 0f : 90f;
                            EnsureTrafficVehicle(streetProps, $"Car_{carIndex:000}", new Vector3(x, 0f, z), yaw, carColor);
                            carIndex++;
                        }

                        if ((roadRow || roadCol) && rng.NextDouble() < roadLampChance)
                        {
                            EnsureStreetLamp(streetProps, $"Lamp_{lampIndex:000}", new Vector3(x, 0f, z));
                            lampIndex++;
                        }

                        if ((roadRow || roadCol) && rng.NextDouble() < roadTreeChance)
                        {
                            var treeColor = Color.Lerp(treeColorA, treeColorB, (float)rng.NextDouble());
                            EnsureStreetTree(streetProps, $"Tree_{treeIndex:000}", new Vector3(x, 0f, z), treeColor);
                            treeIndex++;
                        }

                        if (!spawnLane && (roadRow || roadCol) && rng.NextDouble() < roadBarrelChance)
                        {
                            var barrelColor = Color.Lerp(chainColorA, chainColorB, (float)rng.NextDouble());
                            var barrelOffset = new Vector3(((float)rng.NextDouble() - 0.5f) * 0.72f, 0f, ((float)rng.NextDouble() - 0.5f) * 0.72f);
                            EnsureExplosiveBarrel(streetProps, $"Barrel_{barrelIndex:000}", new Vector3(x, 0f, z) + barrelOffset, barrelColor);
                            barrelIndex++;
                        }

                        if (!spawnLane && (roadRow || roadCol) && rng.NextDouble() < roadTransformerChance)
                        {
                            var transformerColor = Color.Lerp(chainColorB, Color.white, 0.18f + (float)rng.NextDouble() * 0.22f);
                            var transformerOffset = new Vector3(((float)rng.NextDouble() - 0.5f) * 0.62f, 0f, ((float)rng.NextDouble() - 0.5f) * 0.62f);
                            EnsureTransformer(streetProps, $"Transformer_{transformerIndex:000}", new Vector3(x, 0f, z) + transformerOffset, transformerColor);
                            transformerIndex++;
                        }

                        continue;
                    }

                    var districtT = Mathf.InverseLerp(-20f, 20f, z);
                    var lowDistrict = z < lowDistrictEndZ;
                    var midDistrict = z >= lowDistrictEndZ && z < midDistrictEndZ;
                    var effectiveLowSkipThreshold = lowSkipThreshold;
                    if (zi < earlyZoneRows)
                    {
                        effectiveLowSkipThreshold = Mathf.Min(effectiveLowSkipThreshold, 0.965f);
                    }

                    var densityGate = (float)rng.NextDouble();
                    if (lowDistrict && densityGate > effectiveLowSkipThreshold)
                    {
                        continue;
                    }

                    if (midDistrict && densityGate > midSkipThreshold)
                    {
                        continue;
                    }

                    if (!lowDistrict && !midDistrict && densityGate > highSkipThreshold)
                    {
                        continue;
                    }

                    var footprintMin = lowDistrict ? lowFootMin : (midDistrict ? midFootMin : highFootMin);
                    var footprintMax = lowDistrict ? lowFootMax : (midDistrict ? midFootMax : highFootMax);
                    var heightMin = lowDistrict ? lowHeightMin : (midDistrict ? midHeightMin : highHeightMin);
                    var heightMax = lowDistrict ? lowHeightMax : (midDistrict ? midHeightMax : highHeightMax);

                    if (zi < earlyZoneRows)
                    {
                        footprintMin = Mathf.Min(footprintMin, 0.85f);
                        footprintMax = Mathf.Min(footprintMax, 1.45f);
                        heightMin = Mathf.Min(heightMin, 0.48f);
                        heightMax = Mathf.Min(heightMax, 1.35f);
                    }

                    var sizeX = Mathf.Lerp(footprintMin, footprintMax, (float)rng.NextDouble());
                    var sizeZ = Mathf.Lerp(footprintMin, footprintMax, (float)rng.NextDouble());
                    var height = Mathf.Lerp(heightMin, heightMax, (float)rng.NextDouble());
                    var posJitter = lowDistrict ? lowPosJitter : otherPosJitter;
                    var posX = x + ((float)rng.NextDouble() - 0.5f) * posJitter;
                    var posZ = z + ((float)rng.NextDouble() - 0.5f) * posJitter;
                    var halfX = sizeX * 0.5f + buildingSpacing;
                    var halfZ = sizeZ * 0.5f + buildingSpacing;
                    ClampFootprintCenterToMapInterior(ref posX, ref posZ, halfX, halfZ, half, wallThickness, 0.65f);
                    if (OverlapsAnyFootprint(buildingFootprints, posX, posZ, halfX, halfZ))
                    {
                        continue;
                    }

                    var hp = Mathf.Clamp(Mathf.RoundToInt(height * Mathf.Lerp(0.75f, 1.2f, districtT)), 1, 10);
                    var type = lowDistrict && rng.NextDouble() < 0.1 ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                    var color = Color.Lerp(blockNearColor, blockFarColor, districtT);

                    EnsureDestructiblePrimitive(
                        cityBlocks,
                        $"Block_{blockIndex:000}",
                        type,
                        new Vector3(posX, height * 0.5f, posZ),
                        new Vector3(sizeX, height, sizeZ),
                        color,
                        hp);

                    blockIndex++;
                    AddFootprint(buildingFootprints, posX, posZ, halfX, halfZ);

                    var isEarlyLowDistrict = lowDistrict && zi < earlyZoneRows;
                    if (midDistrict && rng.NextDouble() < midFacadeAccentChance)
                    {
                        var accentColor = Color.Lerp(commercialAccentA, commercialAccentB, (float)rng.NextDouble());
                        var accentMode = rng.Next(0, 2);
                        if (accentMode == 0)
                        {
                            EnsureCommercialAwning(
                                cityBlocks,
                                $"ShopAwning_{blockIndex:000}",
                                new Vector3(posX, Mathf.Max(0.16f, height * 0.28f), posZ - sizeZ * 0.5f - 0.06f),
                                new Vector3(Mathf.Clamp(sizeX * 0.82f, 1f, 2.6f), 0.16f, 0.42f),
                                accentColor);
                        }
                        else
                        {
                            EnsureCommercialSign(
                                cityBlocks,
                                $"ShopSign_{blockIndex:000}",
                                new Vector3(posX, Mathf.Max(0.3f, height * 0.46f), posZ - sizeZ * 0.5f - 0.05f),
                                new Vector3(Mathf.Clamp(sizeX * 0.58f, 0.8f, 1.8f), 0.42f, 0.12f),
                                accentColor);
                        }
                    }

                    if (midDistrict && sizeX >= 1.45f && rng.NextDouble() < midRetailStripChance)
                    {
                        var stripSegments = Mathf.Clamp(Mathf.RoundToInt(sizeX / 0.82f), 2, 3);
                        var stripSpacing = Mathf.Clamp(sizeX / Mathf.Max(2f, stripSegments), 0.72f, 1.12f);
                        var storefrontZ = posZ - (sizeZ * 0.5f + 0.52f);
                        var stripWidth = Mathf.Clamp(sizeX + 0.34f, 1.8f, 3.6f);
                        EnsureCube(
                            targetMarkers,
                            $"RetailStrip_{blockIndex:000}_{zi:00}_{xi:00}",
                            new Vector3(posX, 0.022f, storefrontZ),
                            new Vector3(stripWidth, 0.025f, 0.26f),
                            new Color(commercialAccentA.r, commercialAccentA.g, commercialAccentA.b, 0.52f));
                        EnsureCube(
                            groundDetails,
                            $"SidewalkPocket_{blockIndex:000}_{zi:00}_{xi:00}",
                            new Vector3(posX, -0.006f, storefrontZ),
                            new Vector3(stripWidth, 0.012f, 0.62f),
                            sidewalkPocketColor);

                        var crosswalkCount = Mathf.Clamp(Mathf.RoundToInt(stripWidth / 0.46f), 3, 6);
                        for (var crosswalkIndex = 0; crosswalkIndex < crosswalkCount; crosswalkIndex++)
                        {
                            var crossOffset = (crosswalkIndex - (crosswalkCount - 1) * 0.5f) * 0.42f;
                            EnsureCube(
                                groundDetails,
                                $"Crosswalk_{blockIndex:000}_{zi:00}_{xi:00}_{crosswalkIndex:00}",
                                new Vector3(posX + crossOffset, -0.002f, storefrontZ + 0.28f),
                                new Vector3(0.22f, 0.01f, 0.12f),
                                crosswalkColor);
                        }

                        EnsureCube(
                            groundDetails,
                            $"BusStopMark_{blockIndex:000}_{zi:00}_{xi:00}",
                            new Vector3(posX, -0.001f, storefrontZ - 0.24f),
                            new Vector3(Mathf.Clamp(stripWidth * 0.72f, 1.3f, 2.6f), 0.01f, 0.08f),
                            busStopMarkColor);

                        for (var stripIndex = 0; stripIndex < stripSegments; stripIndex++)
                        {
                            var laneOffset = (stripIndex - (stripSegments - 1) * 0.5f) * stripSpacing;
                            var stallPos = new Vector3(posX + laneOffset, 0.52f, storefrontZ);
                            var stallScale = new Vector3(
                                Mathf.Lerp(0.66f, 0.96f, (float)rng.NextDouble()),
                                Mathf.Lerp(0.72f, 1.06f, (float)rng.NextDouble()),
                                Mathf.Lerp(0.5f, 0.76f, (float)rng.NextDouble()));
                            var stallHalfX = Mathf.Max(0.24f, stallScale.x * 0.5f);
                            var stallHalfZ = Mathf.Max(0.2f, stallScale.z * 0.5f);
                            var stallX = stallPos.x;
                            var stallZ = stallPos.z;
                            ClampFootprintCenterToMapInterior(ref stallX, ref stallZ, stallHalfX, stallHalfZ, half, wallThickness, 0.65f);
                            stallPos.x = stallX;
                            stallPos.z = stallZ;
                            if (OverlapsAnyFootprint(buildingFootprints, stallPos.x, stallPos.z, stallHalfX, stallHalfZ))
                            {
                                continue;
                            }

                            var stallColor = Color.Lerp(commercialAccentA, commercialAccentB, (float)rng.NextDouble());
                            var stripVariant = rng.Next(0, 5);
                            if (stripVariant == 0)
                            {
                                EnsureCommercialKiosk(microProps, $"Prop_{clutterIndex:000}", stallPos, stallScale, stallColor);
                            }
                            else if (stripVariant == 1)
                            {
                                EnsureCommercialAwning(
                                    microProps,
                                    $"Prop_{clutterIndex:000}",
                                    new Vector3(stallPos.x, Mathf.Max(0.24f, stallScale.y * 0.52f), stallPos.z),
                                    new Vector3(Mathf.Clamp(stallScale.x * 0.96f, 0.68f, 1.05f), 0.16f, Mathf.Clamp(stallScale.z * 0.88f, 0.38f, 0.62f)),
                                    stallColor);
                            }
                            else if (stripVariant == 2)
                            {
                                EnsureCommercialBench(microProps, $"Prop_{clutterIndex:000}", new Vector3(stallPos.x, 0.22f, stallPos.z), stallColor);
                            }
                            else if (stripVariant == 3)
                            {
                                EnsureCommercialBusStop(microProps, $"Prop_{clutterIndex:000}", new Vector3(stallPos.x, 0.63f, stallPos.z), stallColor);
                            }
                            else
                            {
                                EnsureCommercialVending(microProps, $"Prop_{clutterIndex:000}", new Vector3(stallPos.x, 0.51f, stallPos.z), stallColor);
                            }

                            AddFootprint(buildingFootprints, stallPos.x, stallPos.z, stallHalfX, stallHalfZ);
                            clutterIndex++;
                        }
                    }

                    if (isEarlyLowDistrict && rng.NextDouble() < lowAnnexChance)
                    {
                        var annexSizeX = Mathf.Lerp(0.48f, 0.92f, (float)rng.NextDouble());
                        var annexSizeZ = Mathf.Lerp(0.48f, 0.92f, (float)rng.NextDouble());
                        var annexHeight = Mathf.Lerp(0.32f, Mathf.Max(0.5f, height * 0.78f), (float)rng.NextDouble());
                        var annexOffset = new Vector3(
                            ((float)rng.NextDouble() < 0.5f ? -1f : 1f) * (sizeX * 0.45f + annexSizeX * 0.42f + 0.16f),
                            0f,
                            ((float)rng.NextDouble() - 0.5f) * 0.48f);

                        if (Mathf.Abs(annexOffset.z) < 0.12f)
                        {
                            annexOffset.z = Mathf.Sign(annexOffset.z == 0f ? 1f : annexOffset.z) * 0.12f;
                        }

                        var annexX = posX + annexOffset.x;
                        var annexZ = posZ + annexOffset.z;
                        var annexHalfX = annexSizeX * 0.5f + Mathf.Max(0.18f, buildingSpacing * 0.5f);
                        var annexHalfZ = annexSizeZ * 0.5f + Mathf.Max(0.18f, buildingSpacing * 0.5f);
                        ClampFootprintCenterToMapInterior(ref annexX, ref annexZ, annexHalfX, annexHalfZ, half, wallThickness, 0.65f);

                        if (!OverlapsAnyFootprint(buildingFootprints, annexX, annexZ, annexHalfX, annexHalfZ))
                        {
                            var annexScale = new Vector3(annexSizeX, annexHeight, annexSizeZ);
                            var annexColor = Color.Lerp(color, clutterColorA, 0.24f + (float)rng.NextDouble() * 0.18f);
                            EnsureDestructiblePrimitive(
                                cityBlocks,
                                $"Block_{blockIndex:000}",
                                PrimitiveType.Cube,
                                new Vector3(annexX, annexHeight * 0.5f, annexZ),
                                annexScale,
                                annexColor,
                                Mathf.Max(1, hp - 1));
                            blockIndex++;
                            AddFootprint(buildingFootprints, annexX, annexZ, annexHalfX, annexHalfZ);
                        }
                    }

                    if (isEarlyLowDistrict && rng.NextDouble() < lowFenceLineChance)
                    {
                        var fenceAlongFront = rng.Next(0, 2) == 0;
                        var fenceSegments = rng.Next(2, 4);
                        var fenceSpacing = 0.94f;
                        var fenceColor = Color.Lerp(clutterColorA, clutterColorB, (float)rng.NextDouble());
                        var fenceBaseX = posX;
                        var fenceBaseZ = posZ;
                        if (fenceAlongFront)
                        {
                            fenceBaseZ += -(sizeZ * 0.5f + 0.34f);
                        }
                        else
                        {
                            fenceBaseX += (rng.Next(0, 2) == 0 ? -1f : 1f) * (sizeX * 0.5f + 0.34f);
                        }

                        for (var fenceIndex = 0; fenceIndex < fenceSegments; fenceIndex++)
                        {
                            var lineOffset = (fenceIndex - (fenceSegments - 1) * 0.5f) * fenceSpacing;
                            var fencePos = new Vector3(
                                fenceAlongFront ? fenceBaseX + lineOffset : fenceBaseX,
                                0.21f,
                                fenceAlongFront ? fenceBaseZ : fenceBaseZ + lineOffset);
                            var fenceHalfX = fenceAlongFront ? 0.52f : 0.12f;
                            var fenceHalfZ = fenceAlongFront ? 0.12f : 0.52f;
                            var fenceX = fencePos.x;
                            var fenceZ = fencePos.z;
                            ClampFootprintCenterToMapInterior(ref fenceX, ref fenceZ, fenceHalfX, fenceHalfZ, half, wallThickness, 0.65f);
                            fencePos.x = fenceX;
                            fencePos.z = fenceZ;
                            if (OverlapsAnyFootprint(buildingFootprints, fencePos.x, fencePos.z, fenceHalfX, fenceHalfZ))
                            {
                                continue;
                            }

                            var fenceName = $"Prop_{clutterIndex:000}";
                            EnsureResidentialFence(microProps, fenceName, fencePos, fenceColor);
                            clutterIndex++;
                            AddFootprint(buildingFootprints, fencePos.x, fencePos.z, fenceHalfX, fenceHalfZ);
                        }
                    }

                    if (isEarlyLowDistrict && rng.NextDouble() < lowAlleyGuideChance)
                    {
                        var guideAlongFront = rng.Next(0, 2) == 0;
                        var guideLength = guideAlongFront
                            ? Mathf.Clamp(sizeX + 1.6f, 1.8f, 4.4f)
                            : Mathf.Clamp(sizeZ + 1.6f, 1.8f, 4.4f);
                        var guideScale = guideAlongFront
                            ? new Vector3(guideLength, 0.03f, 0.22f)
                            : new Vector3(0.22f, 0.03f, guideLength);
                        var guidePos = new Vector3(
                            guideAlongFront ? posX : posX + (rng.Next(0, 2) == 0 ? -1f : 1f) * (sizeX * 0.5f + 0.46f),
                            0.02f,
                            guideAlongFront ? posZ - (sizeZ * 0.5f + 0.46f) : posZ);
                        EnsureCube(targetMarkers, $"AlleyGuide_{blockIndex:000}_{zi:00}_{xi:00}", guidePos, guideScale, alleyGuideColor);
                    }

                    if (lowDistrict && rng.NextDouble() < lowClutterChance)
                    {
                        var clutterSize = Mathf.Lerp(0.28f, 0.85f, (float)rng.NextDouble());
                        var clutterType = rng.Next(0, 3) == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                        var clutterScale = clutterType == PrimitiveType.Cylinder
                            ? new Vector3(clutterSize * 0.7f, clutterSize, clutterSize * 0.7f)
                            : new Vector3(clutterSize, clutterSize, clutterSize);
                        var clutterPos = new Vector3(
                            posX + ((float)rng.NextDouble() - 0.5f) * 1.6f,
                            clutterScale.y * 0.5f,
                            posZ + ((float)rng.NextDouble() - 0.5f) * 1.6f);
                        var clutterColor = Color.Lerp(clutterColorA, clutterColorB, (float)rng.NextDouble());
                        var clutterHalfX = Mathf.Max(0.22f, clutterScale.x * 0.5f);
                        var clutterHalfZ = Mathf.Max(0.22f, clutterScale.z * 0.5f);
                        if (isEarlyLowDistrict)
                        {
                            var residentialPattern = rng.Next(0, 4);
                            var frontOffsetZ = -(sizeZ * 0.5f + clutterHalfZ + 0.18f);
                            var sideOffsetX = (rng.Next(0, 2) == 0 ? -1f : 1f) * (sizeX * 0.5f + clutterHalfX + 0.18f);
                            switch (residentialPattern)
                            {
                                case 0:
                                    clutterPos.x = posX + Mathf.Lerp(-sizeX * 0.22f, sizeX * 0.22f, (float)rng.NextDouble());
                                    clutterPos.z = posZ + frontOffsetZ;
                                    break;
                                case 1:
                                    clutterPos.x = posX + sideOffsetX;
                                    clutterPos.z = posZ + Mathf.Lerp(-sizeZ * 0.24f, sizeZ * 0.24f, (float)rng.NextDouble());
                                    break;
                                case 2:
                                    clutterPos.x = posX + Mathf.Lerp(-sizeX * 0.46f, sizeX * 0.46f, (float)rng.NextDouble());
                                    clutterPos.z = posZ + frontOffsetZ + Mathf.Lerp(-0.08f, 0.08f, (float)rng.NextDouble());
                                    break;
                                default:
                                    clutterPos.x = posX + sideOffsetX;
                                    clutterPos.z = posZ + frontOffsetZ * 0.38f;
                                    break;
                            }
                        }
                        var clutterX = clutterPos.x;
                        var clutterZ = clutterPos.z;
                        ClampFootprintCenterToMapInterior(ref clutterX, ref clutterZ, clutterHalfX, clutterHalfZ, half, wallThickness, 0.65f);
                        clutterPos.x = clutterX;
                        clutterPos.z = clutterZ;
                        if (OverlapsAnyFootprint(buildingFootprints, clutterPos.x, clutterPos.z, clutterHalfX, clutterHalfZ))
                        {
                            continue;
                        }

                        var propName = $"Prop_{clutterIndex:000}";
                        var useResidentialProp = isEarlyLowDistrict && rng.NextDouble() < lowResidentialPropChance;
                        if (useResidentialProp)
                        {
                            var residentialVariant = rng.Next(0, 3);
                            if (residentialVariant == 0)
                            {
                                EnsureResidentialMailbox(microProps, propName, clutterPos, new Color(0.34f, 0.28f, 0.22f), clutterColor);
                            }
                            else if (residentialVariant == 1)
                            {
                                EnsureResidentialFence(microProps, propName, clutterPos, clutterColor);
                            }
                            else
                            {
                                var shedScale = new Vector3(
                                    Mathf.Lerp(0.65f, 1.15f, (float)rng.NextDouble()),
                                    Mathf.Lerp(0.52f, 0.9f, (float)rng.NextDouble()),
                                    Mathf.Lerp(0.7f, 1.2f, (float)rng.NextDouble()));
                                EnsureResidentialShed(microProps, propName, clutterPos, shedScale, clutterColor);
                            }
                        }
                        else
                        {
                            EnsureDestructiblePrimitive(
                                microProps,
                                propName,
                                clutterType,
                                clutterPos,
                                clutterScale,
                                clutterColor,
                                1);
                        }

                        clutterIndex++;
                    }
                    else if (midDistrict && rng.NextDouble() < midCommercialPropChance)
                    {
                        var kioskPos = new Vector3(
                            posX + Mathf.Lerp(-sizeX * 0.36f, sizeX * 0.36f, (float)rng.NextDouble()),
                            0.52f,
                            posZ - (sizeZ * 0.5f + 0.48f));
                        var kioskScale = new Vector3(
                            Mathf.Lerp(0.7f, 1.25f, (float)rng.NextDouble()),
                            Mathf.Lerp(0.75f, 1.2f, (float)rng.NextDouble()),
                            Mathf.Lerp(0.52f, 0.9f, (float)rng.NextDouble()));
                        var kioskHalfX = Mathf.Max(0.26f, kioskScale.x * 0.5f);
                        var kioskHalfZ = Mathf.Max(0.22f, kioskScale.z * 0.5f);
                        var kioskX = kioskPos.x;
                        var kioskZ = kioskPos.z;
                        ClampFootprintCenterToMapInterior(ref kioskX, ref kioskZ, kioskHalfX, kioskHalfZ, half, wallThickness, 0.65f);
                        kioskPos.x = kioskX;
                        kioskPos.z = kioskZ;
                        if (!OverlapsAnyFootprint(buildingFootprints, kioskPos.x, kioskPos.z, kioskHalfX, kioskHalfZ))
                        {
                            var kioskColor = Color.Lerp(commercialAccentA, commercialAccentB, (float)rng.NextDouble());
                            var midPropVariant = rng.Next(0, 4);
                            if (midPropVariant == 0)
                            {
                                EnsureCommercialKiosk(microProps, $"Prop_{clutterIndex:000}", kioskPos, kioskScale, kioskColor);
                            }
                            else if (midPropVariant == 1)
                            {
                                EnsureCommercialBench(microProps, $"Prop_{clutterIndex:000}", new Vector3(kioskPos.x, 0.22f, kioskPos.z), kioskColor);
                            }
                            else if (midPropVariant == 2)
                            {
                                EnsureCommercialBusStop(microProps, $"Prop_{clutterIndex:000}", new Vector3(kioskPos.x, 0.63f, kioskPos.z), kioskColor);
                            }
                            else
                            {
                                EnsureCommercialVending(microProps, $"Prop_{clutterIndex:000}", new Vector3(kioskPos.x, 0.51f, kioskPos.z), kioskColor);
                            }
                            AddFootprint(buildingFootprints, kioskPos.x, kioskPos.z, kioskHalfX, kioskHalfZ);
                            clutterIndex++;
                        }
                    }
                }
            }

            EnsureCylinder(targetMarkers, "Target_A", new Vector3(-15f, 0.15f, 12f), new Vector3(1.5f, 0.15f, 1.5f), targetColor);
            EnsureCylinder(targetMarkers, "Target_B", new Vector3(15f, 0.15f, -12f), new Vector3(1.5f, 0.15f, 1.5f), targetColor);

            Debug.Log($"[AlienCrusher] City theme generated: {theme}");
        }

        private static CityThemeProfile PickEditorCityTheme(Transform mapRoot)
        {
            var seed = unchecked((int)(System.DateTime.UtcNow.Ticks ^ (mapRoot != null ? mapRoot.GetInstanceID() * 397L : 0L)));
            var rng = new System.Random(seed);
            return (CityThemeProfile)rng.Next(0, 3);
        }
private static void EnsureTrafficVehicle(Transform parent, string name, Vector3 localPosition, float yaw, Color bodyColor)
        {
            var root = CreateOrGetChild(parent, name).transform;
            root.localPosition = localPosition;
            root.localRotation = Quaternion.Euler(0f, yaw, 0f);
            root.localScale = Vector3.one;

            var body = EnsureCube(root, "Body", new Vector3(0f, 0.24f, 0f), new Vector3(1.15f, 0.35f, 2.05f), bodyColor);
            EnsureCube(root, "Cabin", new Vector3(0f, 0.5f, -0.12f), new Vector3(0.82f, 0.28f, 0.95f), Color.Lerp(bodyColor, Color.white, 0.18f));

            var wheelColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            EnsureCylinder(root, "Wheel_FL", new Vector3(0.48f, 0.12f, 0.62f), new Vector3(0.2f, 0.12f, 0.2f), wheelColor);
            EnsureCylinder(root, "Wheel_FR", new Vector3(-0.48f, 0.12f, 0.62f), new Vector3(0.2f, 0.12f, 0.2f), wheelColor);
            EnsureCylinder(root, "Wheel_RL", new Vector3(0.48f, 0.12f, -0.62f), new Vector3(0.2f, 0.12f, 0.2f), wheelColor);
            EnsureCylinder(root, "Wheel_RR", new Vector3(-0.48f, 0.12f, -0.62f), new Vector3(0.2f, 0.12f, 0.2f), wheelColor);

            EnsureStreetPropReactive(body, root, DummyStreetPropReactive.PropKind.Vehicle);
        }

private static void EnsureStreetLamp(Transform parent, string name, Vector3 localPosition)
        {
            var root = CreateOrGetChild(parent, name).transform;
            root.localPosition = localPosition;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;

            var pole = EnsureCylinder(root, "Pole", new Vector3(0f, 1.05f, 0f), new Vector3(0.08f, 1.05f, 0.08f), new Color(0.7f, 0.74f, 0.8f));
            EnsureCube(root, "Head", new Vector3(0f, 2.1f, 0f), new Vector3(0.36f, 0.16f, 0.36f), new Color(1f, 0.9f, 0.55f));
            EnsureStreetPropReactive(pole, root, DummyStreetPropReactive.PropKind.Lamp);
        }

private static void EnsureStreetTree(Transform parent, string name, Vector3 localPosition, Color leafColor)
        {
            var root = CreateOrGetChild(parent, name).transform;
            root.localPosition = localPosition;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;

            var trunk = EnsureCylinder(root, "Trunk", new Vector3(0f, 0.55f, 0f), new Vector3(0.18f, 0.55f, 0.18f), new Color(0.35f, 0.25f, 0.16f));
            EnsureSphere(root, "Leaves", new Vector3(0f, 1.35f, 0f), new Vector3(0.95f, 0.95f, 0.95f), leafColor);
            EnsureStreetPropReactive(trunk, root, DummyStreetPropReactive.PropKind.Tree);
        }
private static void EnsureExplosiveBarrel(Transform parent, string name, Vector3 localPosition, Color bodyColor)
        {
            var root = CreateOrGetChild(parent, name).transform;
            root.localPosition = localPosition;
            root.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            root.localScale = Vector3.one;

            var body = EnsureCylinder(root, "Body", new Vector3(0f, 0.42f, 0f), new Vector3(0.55f, 0.42f, 0.55f), bodyColor);
            EnsureCylinder(root, "Band_A", new Vector3(0f, 0.61f, 0f), new Vector3(0.57f, 0.05f, 0.57f), Color.Lerp(bodyColor, Color.white, 0.22f));
            EnsureCylinder(root, "Band_B", new Vector3(0f, 0.23f, 0f), new Vector3(0.57f, 0.05f, 0.57f), Color.Lerp(bodyColor, Color.black, 0.18f));
            EnsureSphere(root, "Core", new Vector3(0f, 0.42f, 0f), new Vector3(0.22f, 0.22f, 0.22f), Color.Lerp(bodyColor, new Color(1f, 0.86f, 0.3f), 0.4f));
            EnsureStreetPropReactive(body, root, DummyStreetPropReactive.PropKind.ChainBarrel);
        }

private static void EnsureTransformer(Transform parent, string name, Vector3 localPosition, Color baseColor)
        {
            var root = CreateOrGetChild(parent, name).transform;
            root.localPosition = localPosition;
            root.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            root.localScale = Vector3.one;

            var baseBlock = EnsureCube(root, "Base", new Vector3(0f, 0.36f, 0f), new Vector3(1.05f, 0.72f, 0.72f), baseColor);
            EnsureCube(root, "Top", new Vector3(0f, 0.82f, 0f), new Vector3(0.82f, 0.18f, 0.52f), Color.Lerp(baseColor, Color.white, 0.28f));
            EnsureCylinder(root, "Pole_L", new Vector3(-0.26f, 0.95f, 0f), new Vector3(0.08f, 0.32f, 0.08f), new Color(0.74f, 0.76f, 0.8f));
            EnsureCylinder(root, "Pole_R", new Vector3(0.26f, 0.95f, 0f), new Vector3(0.08f, 0.32f, 0.08f), new Color(0.74f, 0.76f, 0.8f));
            EnsureSphere(root, "Core", new Vector3(0f, 0.78f, 0f), new Vector3(0.24f, 0.24f, 0.24f), Color.Lerp(baseColor, new Color(1f, 0.86f, 0.36f), 0.48f));
            EnsureStreetPropReactive(baseBlock, root, DummyStreetPropReactive.PropKind.Transformer);
        }

        private static void EnsureResidentialMailbox(Transform parent, string name, Vector3 localPosition, Color postColor, Color boxColor)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, new Vector3(0.36f, 0.34f, 0.3f), boxColor, 1).transform;
            EnsureCylinder(root, "Post", new Vector3(0f, -0.18f, 0f), new Vector3(0.08f, 0.18f, 0.08f), postColor);
            EnsureCube(root, "Flag", new Vector3(0.19f, 0.06f, 0f), new Vector3(0.06f, 0.16f, 0.04f), Color.Lerp(boxColor, Color.white, 0.22f));
        }

        private static void EnsureResidentialFence(Transform parent, string name, Vector3 localPosition, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, new Vector3(0.9f, 0.42f, 0.14f), color, 1).transform;
            EnsureCube(root, "RailTop", new Vector3(0f, 0.16f, 0f), new Vector3(0.94f, 0.08f, 0.06f), Color.Lerp(color, Color.white, 0.12f));
            EnsureCube(root, "RailBottom", new Vector3(0f, -0.08f, 0f), new Vector3(0.94f, 0.08f, 0.06f), Color.Lerp(color, Color.black, 0.08f));
            EnsureCube(root, "Post_L", new Vector3(-0.34f, 0f, 0f), new Vector3(0.08f, 0.48f, 0.08f), Color.Lerp(color, Color.black, 0.12f));
            EnsureCube(root, "Post_R", new Vector3(0.34f, 0f, 0f), new Vector3(0.08f, 0.48f, 0.08f), Color.Lerp(color, Color.black, 0.12f));
        }

        private static void EnsureResidentialShed(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, scale, color, 1).transform;
            EnsureCube(root, "Roof", new Vector3(0f, scale.y * 0.62f, 0f), new Vector3(scale.x * 1.08f, scale.y * 0.18f, scale.z * 1.08f), Color.Lerp(color, Color.black, 0.18f));
            EnsureCube(root, "Door", new Vector3(0f, -scale.y * 0.08f, scale.z * 0.5f + 0.01f), new Vector3(scale.x * 0.28f, scale.y * 0.55f, 0.04f), Color.Lerp(color, Color.black, 0.24f));
        }

        private static void EnsureCommercialAwning(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, scale, color, 1).transform;
            EnsureCube(root, "AwningTrim", new Vector3(0f, scale.y * 0.38f, 0f), new Vector3(scale.x * 1.06f, scale.y * 0.24f, scale.z * 0.55f), Color.Lerp(color, Color.white, 0.18f));
            EnsureCube(root, "Bracket_L", new Vector3(-scale.x * 0.38f, -scale.y * 0.2f, scale.z * 0.08f), new Vector3(0.08f, scale.y * 0.72f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
            EnsureCube(root, "Bracket_R", new Vector3(scale.x * 0.38f, -scale.y * 0.2f, scale.z * 0.08f), new Vector3(0.08f, scale.y * 0.72f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
        }

        private static void EnsureCommercialSign(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, scale, color, 1).transform;
            EnsureCube(root, "SignFace", new Vector3(0f, 0f, -scale.z * 0.1f), new Vector3(scale.x * 0.92f, scale.y * 0.8f, scale.z * 1.4f), Color.Lerp(color, Color.white, 0.24f));
            EnsureCube(root, "BackPlate", new Vector3(0f, 0f, scale.z * 0.1f), new Vector3(scale.x, scale.y, scale.z * 0.75f), Color.Lerp(color, Color.black, 0.18f));
            EnsureCube(root, "Mount", new Vector3(0f, -scale.y * 0.62f, scale.z * 0.08f), new Vector3(scale.x * 0.18f, scale.y * 0.48f, scale.z * 0.7f), Color.Lerp(color, Color.black, 0.3f));
        }

        private static void EnsureCommercialKiosk(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, scale, color, 1).transform;
            EnsureCube(root, "ShopRoof", new Vector3(0f, scale.y * 0.52f, 0f), new Vector3(scale.x * 1.12f, scale.y * 0.18f, scale.z * 1.12f), Color.Lerp(color, Color.black, 0.22f));
            EnsureCube(root, "ShopCounter", new Vector3(0f, -scale.y * 0.08f, scale.z * 0.5f + 0.03f), new Vector3(scale.x * 0.76f, scale.y * 0.32f, 0.08f), Color.Lerp(color, Color.white, 0.12f));
            EnsureCube(root, "ShopStripe", new Vector3(0f, scale.y * 0.14f, scale.z * 0.5f + 0.02f), new Vector3(scale.x * 0.82f, scale.y * 0.18f, 0.05f), Color.Lerp(color, Color.white, 0.28f));
        }

        private static void EnsureCommercialBench(Transform parent, string name, Vector3 localPosition, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, new Vector3(0.92f, 0.34f, 0.32f), color, 1).transform;
            EnsureCube(root, "BenchSeat", new Vector3(0f, 0.02f, 0f), new Vector3(0.98f, 0.08f, 0.34f), Color.Lerp(color, Color.white, 0.12f));
            EnsureCube(root, "BenchBack", new Vector3(0f, 0.16f, -0.1f), new Vector3(0.98f, 0.22f, 0.08f), Color.Lerp(color, Color.black, 0.1f));
            EnsureCube(root, "BenchLeg_L", new Vector3(-0.28f, -0.12f, 0f), new Vector3(0.08f, 0.22f, 0.08f), Color.Lerp(color, Color.black, 0.22f));
            EnsureCube(root, "BenchLeg_R", new Vector3(0.28f, -0.12f, 0f), new Vector3(0.08f, 0.22f, 0.08f), Color.Lerp(color, Color.black, 0.22f));
        }

        private static void EnsureCommercialBusStop(Transform parent, string name, Vector3 localPosition, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, new Vector3(1.08f, 1.26f, 0.32f), color, 1).transform;
            EnsureCube(root, "StopRoof", new Vector3(0f, 0.48f, 0f), new Vector3(1.12f, 0.08f, 0.42f), Color.Lerp(color, Color.black, 0.16f));
            EnsureCube(root, "StopPanel", new Vector3(0f, 0f, -0.07f), new Vector3(0.76f, 0.74f, 0.06f), Color.Lerp(color, Color.white, 0.22f));
            EnsureCube(root, "StopPole_L", new Vector3(-0.34f, 0f, 0f), new Vector3(0.08f, 0.96f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
            EnsureCube(root, "StopPole_R", new Vector3(0.34f, 0f, 0f), new Vector3(0.08f, 0.96f, 0.08f), Color.Lerp(color, Color.black, 0.24f));
        }

        private static void EnsureCommercialVending(Transform parent, string name, Vector3 localPosition, Color color)
        {
            var root = EnsureDestructiblePrimitive(parent, name, PrimitiveType.Cube, localPosition, new Vector3(0.58f, 1.02f, 0.52f), color, 1).transform;
            EnsureCube(root, "VendFace", new Vector3(0f, 0.04f, 0.27f), new Vector3(0.46f, 0.78f, 0.04f), Color.Lerp(color, Color.white, 0.26f));
            EnsureCube(root, "VendSlot", new Vector3(0f, -0.28f, 0.28f), new Vector3(0.28f, 0.08f, 0.05f), Color.Lerp(color, Color.black, 0.24f));
            EnsureCube(root, "VendCap", new Vector3(0f, 0.44f, 0f), new Vector3(0.62f, 0.08f, 0.56f), Color.Lerp(color, Color.black, 0.18f));
        }

        private static void EnsureStreetPropReactive(GameObject hitTarget, Transform propRoot, DummyStreetPropReactive.PropKind kind)
        {
            if (hitTarget == null || propRoot == null)
            {
                return;
            }

            var reactive = AddComponentIfMissing<DummyStreetPropReactive>(hitTarget);
            reactive.ConfigureForScaffolder(kind, propRoot);
        }

        private static bool OverlapsAnyFootprint(System.Collections.Generic.List<Vector4> footprints, float x, float z, float halfX, float halfZ)
        {
            for (var i = 0; i < footprints.Count; i++)
            {
                var f = footprints[i];
                if (Mathf.Abs(f.x - x) <= (f.z + halfX) && Mathf.Abs(f.y - z) <= (f.w + halfZ))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddFootprint(System.Collections.Generic.List<Vector4> footprints, float x, float z, float halfX, float halfZ)
        {
            footprints.Add(new Vector4(x, z, halfX, halfZ));
        }

        private static void ClampFootprintCenterToMapInterior(ref float x, ref float z, float halfX, float halfZ, float mapHalfExtent, float wallThickness, float padding)
        {
            var clampX = Mathf.Max(0.5f, mapHalfExtent - wallThickness - Mathf.Max(0f, padding) - Mathf.Max(0f, halfX));
            var clampZ = Mathf.Max(0.5f, mapHalfExtent - wallThickness - Mathf.Max(0f, padding) - Mathf.Max(0f, halfZ));
            x = Mathf.Clamp(x, -clampX, clampX);
            z = Mathf.Clamp(z, -clampZ, clampZ);
        }

        private static void ClearChildrenImmediate(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void ConfigureSpawnPoints(Transform spawnRoot, Transform playerBall, Rigidbody playerBody)
        {
            var playerSpawn = CreateOrGetChild(spawnRoot, "PlayerSpawn");
            playerSpawn.transform.localPosition = new Vector3(0f, 0.6f, -18f);
            playerSpawn.transform.localRotation = Quaternion.identity;

            var cameraFocus = CreateOrGetChild(spawnRoot, "CameraFocus");
            cameraFocus.transform.localPosition = new Vector3(0f, 0f, 0f);

            playerBall.position = playerSpawn.transform.position;
            playerBall.rotation = Quaternion.identity;

            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }
        }

        private static void ConfigureGameplayCamera(Transform gameplayRoot)
        {
            Camera targetCamera = Camera.main;
            if (targetCamera == null)
            {
                var cameraGo = FindChild(gameplayRoot, "GameplayCamera");
                if (cameraGo == null)
                {
                    cameraGo = new GameObject("GameplayCamera", typeof(Camera));
                    cameraGo.transform.SetParent(gameplayRoot, false);
                }

                targetCamera = AddComponentIfMissing<Camera>(cameraGo);
                cameraGo.tag = "MainCamera";
            }

            var cameraTransform = targetCamera.transform;
            if (cameraTransform.parent != gameplayRoot)
            {
                cameraTransform.SetParent(gameplayRoot, true);
            }

            cameraTransform.localPosition = new Vector3(0f, 30f, -20f);
            cameraTransform.localRotation = Quaternion.Euler(58f, 0f, 0f);
            targetCamera.fieldOfView = 50f;
            targetCamera.nearClipPlane = 0.1f;
            targetCamera.farClipPlane = 200f;

            if (Object.FindFirstObjectByType<AudioListener>() == null)
            {
                AddComponentIfMissing<AudioListener>(targetCamera.gameObject);
            }

            var follow = Object.FindFirstObjectByType<CameraFollowSystem>();
            if (follow != null)
            {
                follow.Configure(cameraTransform);
                follow.ApplyComfortPreset();
                follow.SnapToTarget();
            }
        }
        private static void EnsureGameplayLighting(Transform gameplayRoot)
        {
            Light keyLight = null;
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l != null && l.type == LightType.Directional)
                {
                    keyLight = l;
                    break;
                }
            }

            if (keyLight == null)
            {
                var lightGo = FindChild(gameplayRoot, "GameplayKeyLight");
                if (lightGo == null)
                {
                    lightGo = new GameObject("GameplayKeyLight", typeof(Light));
                    lightGo.transform.SetParent(gameplayRoot, false);
                }

                keyLight = AddComponentIfMissing<Light>(lightGo);
            }

            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.28f;
            keyLight.color = new Color(1f, 0.95f, 0.88f, 1f);
            keyLight.shadows = LightShadows.Soft;
            keyLight.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            keyLight.transform.position = new Vector3(0f, 18f, 0f);
        }
        private static void ConfigureUi(Transform parent)
        {
            var canvasGo = FindChild(parent, "Canvas_Dummy");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas_Dummy", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasGo.transform.SetParent(parent, false);
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var safeAreaRoot = CreateOrGetChild(canvasGo.transform, "SafeAreaRoot", true);
            var safeRect = (RectTransform)safeAreaRoot.transform;
            Stretch(safeRect);

            MoveChildIfExists(canvasGo.transform, safeRect, "HUD_Dummy");
            MoveChildIfExists(canvasGo.transform, safeRect, "ResultPopup_Dummy");
            MoveChildIfExists(canvasGo.transform, safeRect, "LevelUpPopup_Dummy");
            MoveChildIfExists(canvasGo.transform, safeRect, "LobbyPanel_Dummy");
            MoveChildIfExists(canvasGo.transform, safeRect, "PausePanel_Dummy");

            var canvasAdaptor = AddComponentIfMissing<MobileCanvasAdaptor>(canvasGo);
            canvasAdaptor.Configure(scaler, safeRect);

            EnsureEventSystem();

            var hud = CreatePanel(safeRect, "HUD_Dummy", new Color(0f, 0f, 0f, 0.25f), false);
            var result = CreatePanel(safeRect, "ResultPopup_Dummy", new Color(0f, 0f, 0f, 0.62f), false);
            var levelUp = CreatePanel(safeRect, "LevelUpPopup_Dummy", new Color(0.05f, 0.08f, 0.15f, 0.75f), false);
            var lobby = CreatePanel(safeRect, "LobbyPanel_Dummy", new Color(0.04f, 0.04f, 0.04f, 0.66f), true);
            var pause = CreatePanel(safeRect, "PausePanel_Dummy", new Color(0f, 0f, 0f, 0.72f), false);

            BuildHud(hud.transform);
            BuildResult(result.transform);
            BuildLevelUp(levelUp.transform);
            BuildLobby(lobby.transform);
            BuildPause(pause.transform);
        }

        private static void ConfigureDebug(Transform parent)
        {
            CreateOrGetChild(parent, "DebugOverlay");
            CreateOrGetChild(parent, "CheatButtons");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color, bool active)
        {
            var panel = CreateOrGetChild(parent, name, true);
            var image = AddComponentIfMissing<Image>(panel);
            image.color = color;

            Stretch((RectTransform)panel.transform);

            var tag = AddComponentIfMissing<DummyPanelTag>(panel);
            tag.Configure(name, active);
            return panel;
        }

        private static void BuildHud(Transform panel)
        {
            BuildVirtualJoystick(panel);

            var topBar = CreateUiNode(panel, "TopBar");
            var topRect = (RectTransform)topBar.transform;
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 180f);
            topRect.anchoredPosition = new Vector2(0f, 0f);
            AddComponentIfMissing<Image>(topBar).color = new Color(0f, 0f, 0f, 0.45f);

            var info = CreateText(topBar.transform, "InfoText", "TIME 01:30    SCORE 000000    LVL 1", 48, TextAnchor.MiddleCenter);
            Stretch((RectTransform)info.transform, 30f, 30f, 20f, 20f);

            var chain = CreateText(topBar.transform, "ChainText", "CHAIN x1", 40, TextAnchor.MiddleRight);
            var chainRect = chain.rectTransform;
            chainRect.anchorMin = new Vector2(1f, 0f);
            chainRect.anchorMax = new Vector2(1f, 1f);
            chainRect.pivot = new Vector2(1f, 0.5f);
            chainRect.sizeDelta = new Vector2(360f, 120f);
            chainRect.anchoredPosition = new Vector2(-28f, 0f);
            chain.color = new Color(1f, 0.8f, 0.3f, 1f);

            CreateButton(topBar.transform, "PauseButton", "II", new Vector2(470f, 0f), new Vector2(110f, 88f));

            var upgrades = CreateText(topBar.transform, "UpgradeListText", "UPGRADES: NONE", 32, TextAnchor.LowerLeft);
            var upgradeRect = upgrades.rectTransform;
            upgradeRect.anchorMin = new Vector2(0f, 0f);
            upgradeRect.anchorMax = new Vector2(1f, 0f);
            upgradeRect.pivot = new Vector2(0f, 0f);
            upgradeRect.sizeDelta = new Vector2(-60f, 60f);
            upgradeRect.anchoredPosition = new Vector2(30f, 16f);
            upgrades.color = new Color(0.82f, 0.95f, 1f, 1f);

            var objectivePanel = CreateUiNode(panel, "ObjectivePanel");
            var objectiveRect = (RectTransform)objectivePanel.transform;
            objectiveRect.anchorMin = new Vector2(0f, 1f);
            objectiveRect.anchorMax = new Vector2(0f, 1f);
            objectiveRect.pivot = new Vector2(0f, 1f);
            objectiveRect.sizeDelta = new Vector2(440f, 220f);
            objectiveRect.anchoredPosition = new Vector2(28f, -208f);
            AddComponentIfMissing<Image>(objectivePanel).color = new Color(0.03f, 0.05f, 0.08f, 0.72f);

            var objectiveText = CreateText(objectivePanel.transform, "ObjectiveText", "OBJECTIVE\nCrush 8 structures to level up", 34, TextAnchor.UpperLeft);
            var objectiveTextRect = objectiveText.rectTransform;
            objectiveTextRect.anchorMin = new Vector2(0f, 1f);
            objectiveTextRect.anchorMax = new Vector2(1f, 1f);
            objectiveTextRect.pivot = new Vector2(0f, 1f);
            objectiveTextRect.sizeDelta = new Vector2(-48f, 88f);
            objectiveTextRect.anchoredPosition = new Vector2(24f, -22f);
            objectiveText.color = new Color(0.96f, 0.97f, 1f, 1f);

            var hintText = CreateText(objectivePanel.transform, "HintText", "HINT\nDrag the virtual stick, build chain, then trigger form skill when ready", 26, TextAnchor.UpperLeft);
            var hintRect = hintText.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(1f, 1f);
            hintRect.pivot = new Vector2(0f, 1f);
            hintRect.sizeDelta = new Vector2(-48f, 88f);
            hintRect.anchoredPosition = new Vector2(24f, -108f);
            hintText.color = new Color(0.72f, 0.88f, 1f, 1f);

            var progressBlock = CreateUiNode(objectivePanel.transform, "ProgressBlock");
            var progressRect = (RectTransform)progressBlock.transform;
            progressRect.anchorMin = new Vector2(0f, 0f);
            progressRect.anchorMax = new Vector2(1f, 0f);
            progressRect.pivot = new Vector2(0.5f, 0f);
            progressRect.sizeDelta = new Vector2(-32f, 56f);
            progressRect.anchoredPosition = new Vector2(0f, 18f);
            AddComponentIfMissing<Image>(progressBlock).color = new Color(0.11f, 0.18f, 0.2f, 0.84f);

            var progressText = CreateText(progressBlock.transform, "DestructionProgressText", "PROGRESS 0 / 8  |  NEXT LV IN 8", 24, TextAnchor.MiddleCenter);
            Stretch(progressText.rectTransform, 16f, 16f, 8f, 8f);
            progressText.color = new Color(0.95f, 0.92f, 0.7f, 1f);

            var skills = CreateUiNode(panel, "SkillButtons");
            var skillsRect = (RectTransform)skills.transform;
            skillsRect.anchorMin = new Vector2(0.5f, 0f);
            skillsRect.anchorMax = new Vector2(0.5f, 0f);
            skillsRect.pivot = new Vector2(0.5f, 0f);
            skillsRect.sizeDelta = new Vector2(920f, 200f);
            skillsRect.anchoredPosition = new Vector2(0f, 80f);

            CreateButton(skills.transform, "TransformButton", "FORM SKILL", new Vector2(-300f, 0f), new Vector2(240f, 130f));
            CreateButton(skills.transform, "Special1Button", "SKILL ALT", new Vector2(0f, 0f), new Vector2(240f, 130f));
            CreateButton(skills.transform, "Special2Button", "LOCKED", new Vector2(300f, 0f), new Vector2(240f, 130f));
        }
        private static void BuildVirtualJoystick(Transform panel)
        {
            var touchArea = CreateUiNode(panel, "JoystickTouchArea");
            touchArea.transform.SetAsFirstSibling();

            var touchRect = (RectTransform)touchArea.transform;
            Stretch(touchRect);

            var touchImage = AddComponentIfMissing<Image>(touchArea);
            touchImage.color = new Color(0f, 0f, 0f, 0.001f);
            touchImage.raycastTarget = true;

            var joystickBase = CreateUiNode(touchArea.transform, "JoystickBase");
            var baseRect = (RectTransform)joystickBase.transform;
            baseRect.anchorMin = new Vector2(0f, 0f);
            baseRect.anchorMax = new Vector2(0f, 0f);
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            baseRect.anchoredPosition = new Vector2(220f, 260f);
            baseRect.sizeDelta = new Vector2(240f, 240f);
            AddComponentIfMissing<Image>(joystickBase).color = new Color(0.12f, 0.12f, 0.12f, 0.45f);

            var joystickKnob = CreateUiNode(joystickBase.transform, "JoystickKnob");
            var knobRect = (RectTransform)joystickKnob.transform;
            knobRect.anchorMin = new Vector2(0.5f, 0.5f);
            knobRect.anchorMax = new Vector2(0.5f, 0.5f);
            knobRect.pivot = new Vector2(0.5f, 0.5f);
            knobRect.anchoredPosition = Vector2.zero;
            knobRect.sizeDelta = new Vector2(110f, 110f);
            AddComponentIfMissing<Image>(joystickKnob).color = new Color(0.9f, 0.92f, 0.95f, 0.9f);

            var joystick = AddComponentIfMissing<VirtualJoystickUI>(touchArea);
            joystick.Configure(baseRect, knobRect, 110f);
        }

        private static void BuildResult(Transform panel)
        {
            CreateText(panel, "Title", "DESTRUCTION RESULT", 68, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -220f);
            CreateText(panel, "Summary", "SCORE 45,320\nDP +1,280\nCHAIN x8", 52, TextAnchor.MiddleCenter).rectTransform.anchoredPosition = new Vector2(0f, -560f);

            var breakdownPanel = CreateUiNode(panel, "ResultBreakdownPanel");
            var breakdownPanelRect = (RectTransform)breakdownPanel.transform;
            breakdownPanelRect.anchorMin = new Vector2(0.5f, 1f);
            breakdownPanelRect.anchorMax = new Vector2(0.5f, 1f);
            breakdownPanelRect.pivot = new Vector2(0.5f, 1f);
            breakdownPanelRect.sizeDelta = new Vector2(820f, 240f);
            breakdownPanelRect.anchoredPosition = new Vector2(0f, -760f);
            AddComponentIfMissing<Image>(breakdownPanel).color = new Color(0.05f, 0.07f, 0.1f, 0.76f);

            var breakdownText = CreateText(breakdownPanel.transform, "ResultBreakdownText", "BREAKDOWN\nBase Score +32,400\nChain Bonus +9,600\nTotal Destruction +3,320", 30, TextAnchor.UpperLeft);
            Stretch(breakdownText.rectTransform, 26f, 26f, 24f, 20f);
            breakdownText.color = new Color(0.9f, 0.95f, 1f, 1f);

            var continueHint = CreateText(panel, "ContinueHint", "Tap NEXT STAGE to roll straight into the next city.", 26, TextAnchor.UpperCenter);
            var continueHintRect = continueHint.rectTransform;
            continueHintRect.anchorMin = new Vector2(0.5f, 0f);
            continueHintRect.anchorMax = new Vector2(0.5f, 0f);
            continueHintRect.pivot = new Vector2(0.5f, 0f);
            continueHintRect.sizeDelta = new Vector2(900f, 60f);
            continueHintRect.anchoredPosition = new Vector2(0f, 190f);
            continueHint.color = new Color(0.75f, 0.86f, 0.94f, 1f);

            CreateButton(panel, "WatchAdButton", "RETRY +1000", new Vector2(-210f, -980f), new Vector2(360f, 140f));
            CreateButton(panel, "NextStageButton", "NEXT STAGE", new Vector2(210f, -980f), new Vector2(360f, 140f));
        }

        private static void BuildLevelUp(Transform panel)
        {
            CreateText(panel, "Title", "LEVEL UP", 72, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -180f);
            CreateText(panel, "Timer", "Choose one skill (5s)", 40, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -280f);

            CreateButton(panel, "SkillRerollButton", "REROLL", new Vector2(0f, -520f), new Vector2(280f, 110f));
            CreateButton(panel, "SkillOption_A", "SPEED +10%", new Vector2(-280f, -760f), new Vector2(300f, 200f));
            CreateButton(panel, "SkillOption_B", "IMPACT +20%", new Vector2(0f, -760f), new Vector2(300f, 200f));
            CreateButton(panel, "SkillOption_C", "DRILL FORM", new Vector2(280f, -760f), new Vector2(300f, 200f));
            CreateButton(panel, "SkillLock_A", "LOCK A", new Vector2(-280f, -940f), new Vector2(180f, 90f));
            CreateButton(panel, "SkillLock_B", "LOCK B", new Vector2(0f, -940f), new Vector2(180f, 90f));
            CreateButton(panel, "SkillLock_C", "LOCK C", new Vector2(280f, -940f), new Vector2(180f, 90f));
        }

        private static void BuildPause(Transform panel)
        {
            CreateText(panel, "PauseTitle", "PAUSED", 84, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -260f);

            var hint = CreateText(panel, "PauseHint", "Take a breath and resume the destruction.", 34, TextAnchor.UpperCenter);
            hint.rectTransform.anchoredPosition = new Vector2(0f, -380f);
            hint.color = new Color(0.84f, 0.92f, 1f, 1f);

            CreateButton(panel, "PauseResumeButton", "RESUME", new Vector2(0f, -720f), new Vector2(420f, 140f));
            CreateButton(panel, "PauseRestartButton", "RESTART", new Vector2(0f, -900f), new Vector2(420f, 130f));
            CreateButton(panel, "PauseLobbyButton", "LOBBY", new Vector2(0f, -1060f), new Vector2(420f, 130f));
        }
        private static void BuildLobby(Transform panel)
        {
            CreateText(panel, "Title", "VRAK COMMAND", 72, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -180f);
            CreateText(panel, "SubTitle", "Destroy human civilization", 44, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -280f);

            var dpText = CreateText(panel, "MetaDpText", "DP 0", 36, TextAnchor.UpperRight);
            var dpRect = dpText.rectTransform;
            dpRect.anchorMin = new Vector2(1f, 1f);
            dpRect.anchorMax = new Vector2(1f, 1f);
            dpRect.pivot = new Vector2(1f, 1f);
            dpRect.sizeDelta = new Vector2(460f, 90f);
            dpRect.anchoredPosition = new Vector2(-36f, -32f);
            dpText.color = new Color(0.92f, 0.96f, 0.72f, 1f);

            var missionHelpPanel = CreateUiNode(panel, "MissionHelpPanel");
            var missionHelpRect = (RectTransform)missionHelpPanel.transform;
            missionHelpRect.anchorMin = new Vector2(0.5f, 1f);
            missionHelpRect.anchorMax = new Vector2(0.5f, 1f);
            missionHelpRect.pivot = new Vector2(0.5f, 1f);
            missionHelpRect.sizeDelta = new Vector2(860f, 220f);
            missionHelpRect.anchoredPosition = new Vector2(0f, -360f);
            AddComponentIfMissing<Image>(missionHelpPanel).color = new Color(0.04f, 0.07f, 0.1f, 0.74f);

            var missionText = CreateText(missionHelpPanel.transform, "MissionText", "MISSION\nFlatten the district, chain destructions, and evolve before time expires.", 32, TextAnchor.UpperLeft);
            var missionTextRect = missionText.rectTransform;
            missionTextRect.anchorMin = new Vector2(0f, 1f);
            missionTextRect.anchorMax = new Vector2(1f, 1f);
            missionTextRect.pivot = new Vector2(0f, 1f);
            missionTextRect.sizeDelta = new Vector2(-48f, 84f);
            missionTextRect.anchoredPosition = new Vector2(24f, -20f);
            missionText.color = new Color(0.96f, 0.97f, 1f, 1f);

            var controlsHelpText = CreateText(missionHelpPanel.transform, "ControlsHelpText", "CONTROLS\nLeft thumb: move  |  Smash small buildings first  |  Trigger form skill on cooldown", 24, TextAnchor.UpperLeft);
            var controlsHelpRect = controlsHelpText.rectTransform;
            controlsHelpRect.anchorMin = new Vector2(0f, 1f);
            controlsHelpRect.anchorMax = new Vector2(1f, 1f);
            controlsHelpRect.pivot = new Vector2(0f, 1f);
            controlsHelpRect.sizeDelta = new Vector2(-48f, 90f);
            controlsHelpRect.anchoredPosition = new Vector2(24f, -106f);
            controlsHelpText.color = new Color(0.75f, 0.9f, 1f, 1f);

            var stagePanel = CreateUiNode(panel, "StageSelectPanel");
            var stageRect = (RectTransform)stagePanel.transform;
            stageRect.anchorMin = new Vector2(0.5f, 1f);
            stageRect.anchorMax = new Vector2(0.5f, 1f);
            stageRect.pivot = new Vector2(0.5f, 1f);
            stageRect.sizeDelta = new Vector2(720f, 120f);
            stageRect.anchoredPosition = new Vector2(0f, -610f);
            AddComponentIfMissing<Image>(stagePanel).color = new Color(0.08f, 0.11f, 0.16f, 0.78f);

            CreateButton(stagePanel.transform, "StagePrevButton", "<", new Vector2(-250f, -12f), new Vector2(120f, 88f));
            CreateButton(stagePanel.transform, "StageNextButton", ">", new Vector2(250f, -12f), new Vector2(120f, 88f));
            var stageText = CreateText(stagePanel.transform, "StageSelectText", "STAGE 01 / MAX 01", 34, TextAnchor.MiddleCenter);
            var stageTextRect = stageText.rectTransform;
            stageTextRect.anchorMin = new Vector2(0.5f, 1f);
            stageTextRect.anchorMax = new Vector2(0.5f, 1f);
            stageTextRect.pivot = new Vector2(0.5f, 1f);
            stageTextRect.sizeDelta = new Vector2(360f, 88f);
            stageTextRect.anchoredPosition = new Vector2(0f, -16f);
            stageText.color = new Color(0.96f, 0.97f, 1f, 1f);

            CreateButton(panel, "PlayButton", "PLAY", new Vector2(0f, -820f), new Vector2(420f, 160f));
            CreateButton(panel, "TreeButton", "SKILL TREE", new Vector2(-230f, -1030f), new Vector2(320f, 130f));
            CreateButton(panel, "ShopButton", "SHOP", new Vector2(230f, -1030f), new Vector2(320f, 130f));

            var formPanel = CreateUiNode(panel, "FormSelectPanel");
            var formRect = (RectTransform)formPanel.transform;
            formRect.anchorMin = new Vector2(0.5f, 0f);
            formRect.anchorMax = new Vector2(0.5f, 0f);
            formRect.pivot = new Vector2(0.5f, 0f);
            formRect.sizeDelta = new Vector2(1240f, 260f);
            formRect.anchoredPosition = new Vector2(0f, 40f);

            CreateText(formPanel.transform, "FormTitle", "FORMS", 36, TextAnchor.UpperCenter).rectTransform.anchoredPosition = new Vector2(0f, -20f);
            CreateButton(formPanel.transform, "Form_Sphere", "SPHERE", new Vector2(-480f, -110f), new Vector2(190f, 120f));
            CreateButton(formPanel.transform, "Form_Spike", "SPIKE", new Vector2(-240f, -110f), new Vector2(190f, 120f));
            CreateButton(formPanel.transform, "Form_Ram", "RAM", new Vector2(0f, -110f), new Vector2(190f, 120f));
            CreateButton(formPanel.transform, "Form_Saucer", "SAUCER", new Vector2(240f, -110f), new Vector2(190f, 120f));
            CreateButton(formPanel.transform, "Form_Crusher", "CRUSHER", new Vector2(480f, -110f), new Vector2(190f, 120f));

            CreateButton(formPanel.transform, "MetaUpgrade_SizeButton", "SIZE LV 0/0", new Vector2(-300f, -250f), new Vector2(240f, 110f));
            CreateButton(formPanel.transform, "MetaUpgrade_ImpactButton", "IMPACT LV 0/0", new Vector2(0f, -250f), new Vector2(240f, 110f));
            CreateButton(formPanel.transform, "MetaUpgrade_DpButton", "DP LV 0/0", new Vector2(300f, -250f), new Vector2(240f, 110f));
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor anchor)
        {
            var go = CreateUiNode(parent, name);
            var text = AddComponentIfMissing<Text>(go);
            text.text = value;
            text.alignment = anchor;
            text.color = Color.white;
            text.fontSize = fontSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = GetBuiltinFont();

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(980f, 180f);
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            var go = CreateUiNode(parent, name);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = AddComponentIfMissing<Image>(go);
            image.color = new Color(0.13f, 0.16f, 0.2f, 0.95f);

            var button = AddComponentIfMissing<Button>(go);
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.2f, 0.25f, 0.3f, 1f);
            colors.pressedColor = new Color(0.08f, 0.1f, 0.13f, 1f);
            button.colors = colors;

            var labelGo = CreateUiNode(go.transform, "Label");
            var labelText = AddComponentIfMissing<Text>(labelGo);
            labelText.text = label;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.fontSize = 34;
            labelText.font = GetBuiltinFont();
            Stretch((RectTransform)labelGo.transform, 12f, 12f, 8f, 8f);

            return button;
        }

        private static Font GetBuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null)
            {
                Object.DestroyImmediate(standaloneModule);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private static GameObject CreateOrGetRoot(string name)
        {
            var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return new GameObject(name);
        }

        private static GameObject CreateOrGetChild(Transform parent, string name, bool uiNode = false)
        {
            var existing = FindChild(parent, name);
            if (existing != null)
            {
                return existing;
            }

            if (uiNode)
            {
                var uiGo = new GameObject(name, typeof(RectTransform));
                uiGo.transform.SetParent(parent, false);
                return uiGo;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateOrGetPrimitive(Transform parent, string name, PrimitiveType type)
        {
            var existing = FindChild(parent, name);
            if (existing != null)
            {
                return existing;
            }

            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            return primitive;
        }

        private static GameObject EnsureCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var cube = CreateOrGetPrimitive(parent, name, PrimitiveType.Cube);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = SanitizeScale(localScale);
            TintObject(cube, color);
            return cube;
        }

        private static GameObject EnsureCylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var cylinder = CreateOrGetPrimitive(parent, name, PrimitiveType.Cylinder);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = Quaternion.identity;
            cylinder.transform.localScale = SanitizeScale(localScale);
            TintObject(cylinder, color);
            return cylinder;
        }

        private static GameObject EnsureSphere(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var sphere = CreateOrGetPrimitive(parent, name, PrimitiveType.Sphere);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localRotation = Quaternion.identity;
            sphere.transform.localScale = SanitizeScale(localScale);
            TintObject(sphere, color);
            return sphere;
        }
        private static GameObject EnsureDestructiblePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color, int hp)
        {
            var primitive = CreateOrGetPrimitive(parent, name, type);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = SanitizeScale(localScale);
            TintObject(primitive, color);

            var destructible = AddComponentIfMissing<DummyDestructibleBlock>(primitive);
            destructible.ConfigureForScaffolder(hp, color);
            return primitive;
        }

private static void RepairZeroScaleMapObjects(Transform mapRoot)
        {
            if (mapRoot == null)
            {
                return;
            }

            var all = mapRoot.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t == null || t == mapRoot)
                {
                    continue;
                }

                var isGroundedDestructible = IsGroundedMapObject(t.name);
                var scale = t.localScale;
                var needsScaleRepair = Mathf.Abs(scale.x) <= 0.001f || Mathf.Abs(scale.y) <= 0.001f || Mathf.Abs(scale.z) <= 0.001f;
                var expectedY = isGroundedDestructible ? Mathf.Max(0.05f, scale.y * 0.5f) : t.localPosition.y;
                var needsPositionRepair = isGroundedDestructible && Mathf.Abs(t.localPosition.y - expectedY) > 0.15f;

                if (!needsScaleRepair && !needsPositionRepair)
                {
                    continue;
                }

                if (needsScaleRepair)
                {
                    Vector3 repairedScale;
                    if (t.name.StartsWith("Block_"))
                    {
                        var z = t.localPosition.z;
                        if (z < 3f)
                        {
                            repairedScale = new Vector3(1.6f, 1.4f, 1.6f);
                        }
                        else if (z < 12f)
                        {
                            repairedScale = new Vector3(2.7f, 3.1f, 2.7f);
                        }
                        else
                        {
                            repairedScale = new Vector3(4.2f, 4.5f, 4.2f);
                        }
                    }
                    else if (t.name.StartsWith("Prop_"))
                    {
                        repairedScale = new Vector3(0.9f, 0.9f, 0.9f);
                    }
                    else
                    {
                        repairedScale = Vector3.one;
                    }

                    repairedScale = SanitizeScale(repairedScale);
                    t.localScale = repairedScale;
                    scale = repairedScale;
                }

                if (isGroundedDestructible)
                {
                    var local = t.localPosition;
                    local.y = Mathf.Max(0.05f, scale.y * 0.5f);
                    t.localPosition = local;
                }
            }
        }

        private static Vector3 SanitizeScale(Vector3 scale)
        {
            const float min = 0.08f;
            const float fallback = 1f;

            var x = Mathf.Abs(scale.x) < min ? fallback : scale.x;
            var y = Mathf.Abs(scale.y) < min ? fallback : scale.y;
            var z = Mathf.Abs(scale.z) < min ? fallback : scale.z;

            return new Vector3(x, y, z);
        }

        private static bool IsGroundedMapObject(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            var lower = objectName.ToLowerInvariant();
            if (lower.Contains("heli") || lower.Contains("drone") || lower.Contains("flying") || lower.Contains("ufo") || lower.Contains("airborne") || lower.StartsWith("air_") || lower.EndsWith("_air"))
            {
                return false;
            }

            return lower.StartsWith("block_")
                   || lower.StartsWith("prop_")
                   || lower.Contains("building")
                   || lower.Contains("house")
                   || lower.Contains("tower");
        }

        private static void TintObject(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var stylized = GetOrCreateStylizedMaterial();
            if (stylized != null && renderer.sharedMaterial != stylized)
            {
                renderer.sharedMaterial = stylized;
            }

            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }

        private static Material GetOrCreateStylizedMaterial()
        {
            if (cachedStylizedMaterial != null)
            {
                return cachedStylizedMaterial;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(StylizedMaterialPath);
            var shader = FindStylizedShader();
            if (shader == null)
            {
                return null;
            }

            if (material == null)
            {
                EnsureStylizedFolders();

                material = new Material(shader);
                ConfigureStylizedMaterial(material);
                AssetDatabase.CreateAsset(material, StylizedMaterialPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                if (material.shader != shader)
                {
                    material.shader = shader;
                }

                ConfigureStylizedMaterial(material);
                EditorUtility.SetDirty(material);
            }

            cachedStylizedMaterial = material;
            return cachedStylizedMaterial;
        }

        private static void EnsureStylizedFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
            {
                AssetDatabase.CreateFolder("Assets", "Art");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Art/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/Art", "Materials");
            }
        }

        private static Shader FindStylizedShader()
        {
            return Shader.Find("Universal Render Pipeline/Unlit")
                   ?? Shader.Find("Unlit/Color")
                   ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                   ?? Shader.Find("Universal Render Pipeline/Lit")
                   ?? Shader.Find("Standard");
        }

        private static void ConfigureStylizedMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.enableInstancing = true;

            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
            if (material.HasProperty("_SpecColor")) material.SetColor("_SpecColor", Color.black);
            if (material.HasProperty("_SpecularHighlights")) material.SetFloat("_SpecularHighlights", 0f);
            if (material.HasProperty("_EnvironmentReflections")) material.SetFloat("_EnvironmentReflections", 0f);
        }

        private static GameObject CreateUiNode(Transform parent, string name)
        {
            return CreateOrGetChild(parent, name, true);
        }

        private static GameObject FindChild(Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        
        private static void MoveChildIfExists(Transform from, Transform to, string childName)
        {
            var child = FindChild(from, childName);
            if (child == null)
            {
                return;
            }

            child.transform.SetParent(to, false);
        }
        private static T AddComponentIfMissing<T>(GameObject target) where T : Component
        {
            var existing = target.GetComponent<T>();
            if (existing != null)
            {
                return existing;
            }

            return target.AddComponent<T>();
        }

        private static void Stretch(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
#endif



































