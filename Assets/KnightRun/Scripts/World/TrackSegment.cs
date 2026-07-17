using KnightRun.Core;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.World
{
    public class TrackSegment : MonoBehaviour
    {
        public float Length = 20f;
        public RunPhase Phase { get; private set; }
        public RunPhaseSettings Settings => currentSettings;

        public float StartZ => transform.position.z;
        public float EndZ => transform.position.z + Length;

        public bool ContainsZ(float z)
        {
            return z >= StartZ && z < EndZ;
        }

        Transform leftWall;
        Transform rightWall;
        Transform ground;
        Transform decorRoot;
        Transform forestDecorRoot;
        Transform biomeDecorRoot;
        RunPhaseSettings currentSettings;
        bool phaseTransitionActive;
        bool transitionMaterialsSwapped;
        float phaseTransitionElapsed;
        float phaseTransitionDuration;
        Vector3 transitionGroundScale;
        Vector3 transitionLeftWallPosition;
        Vector3 transitionRightWallPosition;
        Material transitionGroundMaterial;
        Material transitionWallMaterial;

        public void Build(RunPhaseSettings settings, float zPosition)
        {
            currentSettings = settings;
            Phase = settings.phase;
            transform.position = new Vector3(0f, 0f, zPosition);

            Material groundMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Ground);
            Material wallMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Wall, new Vector2(1f, 4f));

            ground = CreateBox("Ground", new Vector3(PhaseTrackLayout.GetGroundWidth(settings), 0.4f, Length), new Vector3(0f, -0.2f, Length * 0.5f), groundMat, keepCollider: true);
            ground.gameObject.tag = "Ground";
            leftWall = CreateBox("LeftWall", new Vector3(PhaseTrackLayout.WallThickness, 3f, Length), new Vector3(PhaseTrackLayout.GetWallCenterX(settings, -1), 1.5f, Length * 0.5f), wallMat, keepCollider: true);
            rightWall = CreateBox("RightWall", new Vector3(PhaseTrackLayout.WallThickness, 3f, Length), new Vector3(PhaseTrackLayout.GetWallCenterX(settings, 1), 1.5f, Length * 0.5f), wallMat, keepCollider: true);

            bool useBiomePrefabs = LowPolyBiomeCatalog.UsesPrefabEnvironment(settings.phase);
            LowPolyBiomeCatalog.BiomeSet previewSet = useBiomePrefabs
                ? LowPolyBiomeCatalog.Instance?.GetSet(settings.phase)
                : null;
            bool hideGround = previewSet != null && previewSet.replaceGround
                && previewSet.groundPrefabs != null && previewSet.groundPrefabs.Length > 0;
            bool hideWalls = settings.phase == RunPhase.Forest
                || (previewSet != null && previewSet.replaceWalls
                    && ((previewSet.wallPrefabs != null && previewSet.wallPrefabs.Length > 0)
                        || (previewSet.rockPrefabs != null && previewSet.rockPrefabs.Length > 0)));

            SetWallRenderersVisible(!hideWalls);
            SetRendererVisible(ground, !hideGround);

            decorRoot = new GameObject("Decor").transform;
            decorRoot.SetParent(transform, false);

            switch (settings.phase)
            {
                case RunPhase.Forest:
                    BuildForestDecor();
                    break;
                case RunPhase.Cave:
                    BuildBiomeEnvironment();
                    BuildCaveDecor();
                    break;
                case RunPhase.MineCart:
                    BuildBiomeEnvironment();
                    BuildMineCartDecor();
                    break;
                case RunPhase.Volcano:
                    BuildBiomeEnvironment();
                    BuildVolcanoDecor();
                    break;
                case RunPhase.IceCavern:
                case RunPhase.Desert:
                    BuildBiomeEnvironment();
                    break;
            }
        }

        void BuildBiomeEnvironment()
        {
            LowPolyBiomeCatalog catalog = LowPolyBiomeCatalog.Instance;
            if (catalog == null)
            {
                Debug.LogWarning($"Low poly biome catalog not found at Resources/{LowPolyBiomeCatalog.ResourcePath}. Run Knight Run → Build Low Poly Biome Catalog.");
                SetRendererVisible(ground, true);
                SetWallRenderersVisible(true);
                return;
            }

            LowPolyBiomeCatalog.BiomeSet set = catalog.GetSet(currentSettings.phase);
            if (set == null)
            {
                SetRendererVisible(ground, true);
                SetWallRenderersVisible(true);
                return;
            }

            biomeDecorRoot = new GameObject("BiomeEnvironment").transform;
            biomeDecorRoot.SetParent(decorRoot, false);

            int segmentIndex = Mathf.RoundToInt(transform.position.z / Length);
            float groundWidth = PhaseTrackLayout.GetGroundWidth(currentSettings);
            float wallX = PhaseTrackLayout.GetWallCenterX(currentSettings, 1);
            float playableEdgeX = PhaseTrackLayout.GetPlayableMaxX(currentSettings);

            bool hasGround = set.replaceGround
                && set.groundPrefabs != null
                && set.groundPrefabs.Length > 0;
            bool hasWalls = set.replaceWalls
                && ((set.wallPrefabs != null && set.wallPrefabs.Length > 0)
                    || (set.rockPrefabs != null && set.rockPrefabs.Length > 0));

            if (hasGround)
                SpawnGroundTiles(catalog, set, segmentIndex, groundWidth);
            else
                SetRendererVisible(ground, true);

            if (hasWalls)
            {
                SpawnWallPieces(catalog, set, segmentIndex, wallX);
                SpawnRockBarriers(catalog, set, segmentIndex, wallX);
            }
            else
            {
                SetWallRenderersVisible(currentSettings.phase != RunPhase.Forest);
                // Mine (and similar): optional edge rocks without hiding the wall cubes.
                if (!set.replaceWalls && set.rockPrefabs != null && set.rockPrefabs.Length > 0)
                    SpawnRockBarriers(catalog, set, segmentIndex, wallX);
            }

            if (currentSettings.phase != RunPhase.Desert)
                SpawnBiomeDecorations(catalog, set, segmentIndex, playableEdgeX);
            else
                SpawnDesertGrassDecor(catalog, set, segmentIndex, playableEdgeX);
        }

        void SpawnDesertGrassDecor(
            LowPolyBiomeCatalog catalog,
            LowPolyBiomeCatalog.BiomeSet set,
            int segmentIndex,
            float playableEdgeX)
        {
            if (set.decorationPrefabs == null || set.decorationPrefabs.Length == 0)
                return;

            const int grassCount = 6;
            float wallX = PhaseTrackLayout.GetWallCenterX(currentSettings, 1);
            for (int i = 0; i < grassCount; i++)
            {
                GameObject prefab = catalog.GetDecoration(set, segmentIndex * grassCount + i);
                GameObject grass = PrefabVisualUtility.InstantiateVisual(prefab, biomeDecorRoot);
                if (grass == null)
                    continue;

                float side = Random.value < 0.5f ? -1f : 1f;
                grass.name = $"Grass_{prefab.name}";
                grass.transform.localPosition = new Vector3(
                    side * Random.Range(playableEdgeX * 0.45f, wallX - 0.4f),
                    0f,
                    Random.Range(0.5f, Length - 0.5f));
                grass.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                PrefabVisualUtility.FitHeight(grass, Random.Range(0.45f, 0.85f), maxWidth: 1.2f);
            }
        }

        void SpawnGroundTiles(
            LowPolyBiomeCatalog catalog,
            LowPolyBiomeCatalog.BiomeSet set,
            int segmentIndex,
            float groundWidth)
        {
            if (set.groundPrefabs == null || set.groundPrefabs.Length == 0)
            {
                SetRendererVisible(ground, true);
                return;
            }

            float tileDepth = Mathf.Max(0.8f, set.groundTileDepth);
            if (currentSettings.phase == RunPhase.Desert)
                tileDepth = Mathf.Max(tileDepth, 3.5f);

            int tilesAlongZ = Mathf.Clamp(Mathf.CeilToInt(Length / tileDepth), 1, 8);
            float spacingZ = Length / tilesAlongZ;

            // Wide phases (desert) tile across X with larger cells to keep instance count low.
            float cellTarget = currentSettings.phase == RunPhase.Desert ? 16f : groundWidth;
            int tilesAcrossX = Mathf.Clamp(Mathf.CeilToInt(groundWidth / cellTarget), 1, 6);
            float cellWidth = groundWidth / tilesAcrossX;

            for (int ix = 0; ix < tilesAcrossX; ix++)
            {
                for (int iz = 0; iz < tilesAlongZ; iz++)
                {
                    GameObject prefab = catalog.GetGround(set, segmentIndex + ix * 17 + iz * 3);
                    GameObject tile = PrefabVisualUtility.InstantiateVisual(prefab, biomeDecorRoot);
                    if (tile == null)
                        continue;

                    tile.name = $"Ground_{prefab.name}";
                    float x = tilesAcrossX == 1
                        ? 0f
                        : -groundWidth * 0.5f + (ix + 0.5f) * cellWidth;
                    tile.transform.localPosition = new Vector3(x, 0f, (iz + 0.5f) * spacingZ);
                    tile.transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f);
                    PrefabVisualUtility.FitCoverXZ(tile, cellWidth * 1.02f, spacingZ * 1.02f, targetHeight: 0.22f);
                }
            }
        }

        void SpawnWallPieces(
            LowPolyBiomeCatalog catalog,
            LowPolyBiomeCatalog.BiomeSet set,
            int segmentIndex,
            float wallX)
        {
            if (set.wallPrefabs == null || set.wallPrefabs.Length == 0)
                return;

            int piecesPerSide = currentSettings.phase == RunPhase.Desert ? 5 : 4;
            float spacing = Length / piecesPerSide;

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < piecesPerSide; i++)
                {
                    int sideOffset = side < 0 ? 0 : piecesPerSide;
                    GameObject prefab = catalog.GetWall(set, segmentIndex + sideOffset + i * 2);
                    GameObject wall = PrefabVisualUtility.InstantiateVisual(prefab, biomeDecorRoot);
                    if (wall == null)
                        continue;

                    float height = set.wallTargetHeight * Random.Range(0.85f, 1.15f);
                    float maxWidth = currentSettings.phase == RunPhase.Cave
                        ? 3.5f
                        : PhaseTrackLayout.GetGroundWidth(currentSettings) * 0.85f;
                    wall.name = $"Wall_{prefab.name}";
                    wall.transform.localPosition = new Vector3(
                        side * (wallX + Random.Range(1.4f, 2.8f)),
                        0f,
                        (i + 0.5f) * spacing + Random.Range(-0.4f, 0.4f));
                    wall.transform.localRotation = Quaternion.Euler(
                        0f,
                        side < 0 ? Random.Range(70f, 110f) : Random.Range(-110f, -70f),
                        0f);
                    PrefabVisualUtility.FitHeight(wall, height, maxWidth);

                    // Keep large meshes outside the playable corridor.
                    if (PrefabVisualUtility.TryGetBounds(wall, out Bounds bounds))
                    {
                        float halfDepth = Mathf.Max(bounds.extents.x, bounds.extents.z);
                        float desiredX = side * (wallX + halfDepth * 0.55f + 0.75f);
                        Vector3 pos = wall.transform.localPosition;
                        if (Mathf.Abs(desiredX) > Mathf.Abs(pos.x))
                            pos.x = desiredX;
                        wall.transform.localPosition = pos;
                        PrefabVisualUtility.SnapToGround(wall);
                    }
                }
            }
        }

        void SpawnRockBarriers(
            LowPolyBiomeCatalog catalog,
            LowPolyBiomeCatalog.BiomeSet set,
            int segmentIndex,
            float wallX)
        {
            if (set.rockPrefabs == null || set.rockPrefabs.Length == 0)
                return;

            int rocksPerSide = currentSettings.phase switch
            {
                RunPhase.Desert => 10,
                RunPhase.Cave => 6,
                RunPhase.MineCart => 4,
                _ => 8
            };
            float spacing = Length / rocksPerSide;

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < rocksPerSide; i++)
                {
                    int sideOffset = side < 0 ? 0 : rocksPerSide;
                    GameObject prefab = catalog.GetRock(set, segmentIndex + sideOffset + i);
                    GameObject rock = PrefabVisualUtility.InstantiateVisual(prefab, biomeDecorRoot);
                    if (rock == null)
                        continue;

                    rock.name = $"RockWall_{prefab.name}";
                    // Always sit outside the wall line so the corridor keeps its full width.
                    float rockX = currentSettings.phase == RunPhase.Cave
                        ? wallX + Random.Range(0.55f, 1.35f)
                        : wallX + Random.Range(0.2f, 0.7f);
                    rock.transform.localPosition = new Vector3(
                        side * rockX,
                        0f,
                        (i + 0.5f) * spacing + Random.Range(-0.35f, 0.35f));
                    rock.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    PrefabVisualUtility.FitHeight(
                        rock,
                        set.rockTargetHeight * Random.Range(0.85f, 1.2f),
                        maxWidth: currentSettings.phase == RunPhase.Cave ? 2.2f : 2.8f);
                }
            }
        }

        void SpawnBiomeDecorations(
            LowPolyBiomeCatalog catalog,
            LowPolyBiomeCatalog.BiomeSet set,
            int segmentIndex,
            float playableEdgeX)
        {
            if (set.decorationPrefabs == null || set.decorationPrefabs.Length == 0)
                return;

            int count = currentSettings.phase switch
            {
                RunPhase.Desert => 4,
                RunPhase.IceCavern => 3,
                RunPhase.MineCart => 3,
                _ => 7
            };

            float wallX = PhaseTrackLayout.GetWallCenterX(currentSettings, 1);
            float minAbsX;
            float maxAbsX;

            if (currentSettings.phase == RunPhase.MineCart)
            {
                // Stay clear of every rail lane (outermost lane ± rail offset).
                float[] lanes = PhaseTrackLayout.GetLanePositions(currentSettings);
                float outerLane = 0f;
                for (int l = 0; l < lanes.Length; l++)
                    outerLane = Mathf.Max(outerLane, Mathf.Abs(lanes[l]));

                minAbsX = outerLane + 0.95f;
                maxAbsX = wallX - 0.35f;
                if (maxAbsX <= minAbsX)
                    return;
            }
            else
            {
                minAbsX = Mathf.Max(0.6f, playableEdgeX * 0.55f);
                maxAbsX = Mathf.Max(minAbsX, playableEdgeX - 0.2f);
            }

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = catalog.GetDecoration(set, segmentIndex * count + i);
                GameObject decoration = PrefabVisualUtility.InstantiateVisual(prefab, biomeDecorRoot);
                if (decoration == null)
                    continue;

                float side = Random.value < 0.5f ? -1f : 1f;
                float sideBias = side * Random.Range(minAbsX, maxAbsX);

                decoration.name = $"Decor_{prefab.name}";
                decoration.transform.localPosition = new Vector3(
                    sideBias,
                    0f,
                    Random.Range(0.6f, Length - 0.6f));
                decoration.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                float decorHeight = currentSettings.phase == RunPhase.MineCart
                    ? Random.Range(0.65f, 1.0f)
                    : Random.Range(1.1f, 2.4f);
                PrefabVisualUtility.FitHeight(decoration, decorHeight, maxWidth: 1.5f);
            }
        }

        void BuildForestDecor()
        {
            SimpleNatureCatalog catalog = SimpleNatureCatalog.Instance;
            if (catalog == null)
            {
                Debug.LogWarning($"Simple Nature catalog not found at Resources/{SimpleNatureCatalog.ResourcePath}.");
                return;
            }

            forestDecorRoot = new GameObject("ForestNature").transform;
            forestDecorRoot.SetParent(decorRoot, false);

            const int wallTreeRows = 2;
            const int wallTreesPerRow = 10;
            float wallX = PhaseTrackLayout.GetWallCenterX(currentSettings, 1);
            float treeSpacing = Length / wallTreesPerRow;
            int segmentIndex = Mathf.RoundToInt(transform.position.z / Length);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < wallTreeRows; row++)
                {
                    for (int i = 0; i < wallTreesPerRow; i++)
                    {
                        int sideOffset = side < 0 ? 0 : wallTreesPerRow * wallTreeRows;
                        int variantIndex = segmentIndex + sideOffset + row * wallTreesPerRow + i;
                        GameObject prefab = catalog.GetWall(variantIndex);
                        GameObject tree = SimpleNatureCatalog.InstantiateVisual(prefab, forestDecorRoot);
                        if (tree == null)
                            continue;

                        float scale = Random.Range(1f, 1.3f);
                        float stagger = row * treeSpacing * 0.5f;
                        tree.name = $"Wall_{prefab.name}";
                        tree.transform.localPosition = new Vector3(
                            side * (wallX + 0.3f + row * 1.25f + Random.Range(-0.15f, 0.15f)),
                            0f,
                            Mathf.Repeat((i + 0.5f) * treeSpacing + stagger, Length));
                        tree.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        tree.transform.localScale = Vector3.one * scale;
                    }
                }
            }

            const int bushesPerSide = 10;
            float playableEdgeX = PhaseTrackLayout.GetPlayableMaxX(currentSettings);
            float bushSpacing = Length / bushesPerSide;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < bushesPerSide; i++)
                {
                    int sideOffset = side < 0 ? 0 : bushesPerSide;
                    GameObject prefab = catalog.GetBushBarrier(segmentIndex + sideOffset + i);
                    GameObject bush = SimpleNatureCatalog.InstantiateVisual(prefab, forestDecorRoot);
                    if (bush == null)
                        continue;

                    bush.name = $"BushWall_{prefab.name}";
                    bush.transform.localPosition = new Vector3(
                        side * (wallX - 0.35f + Random.Range(-0.12f, 0.12f)),
                        0f,
                        (i + 0.5f) * bushSpacing + Random.Range(-0.35f, 0.35f));
                    bush.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    bush.transform.localScale = Vector3.one * Random.Range(0.9f, 1.25f);
                }
            }

            const int decorationCount = 12;
            for (int i = 0; i < decorationCount; i++)
            {
                GameObject prefab = catalog.GetDecoration(segmentIndex * decorationCount + i);
                GameObject decoration = SimpleNatureCatalog.InstantiateVisual(prefab, forestDecorRoot);
                if (decoration == null)
                    continue;

                float scale = Random.Range(0.8f, 1.35f);
                decoration.name = $"Decor_{prefab.name}";
                decoration.transform.localPosition = new Vector3(
                    Random.Range(-playableEdgeX + 0.35f, playableEdgeX - 0.35f),
                    0f,
                    Random.Range(0.5f, Length - 0.5f));
                decoration.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                decoration.transform.localScale = Vector3.one * scale;
            }
        }

        void BuildCaveDecor()
        {
            float wallX = PhaseTrackLayout.GetWallCenterX(currentSettings, 1);
            // Sit on the rock-wall line (visual rocks are outside wallX), facing the lane.
            float torchX = wallX + 0.55f;
            for (int i = 0; i < 4; i++)
            {
                float z = 2.5f + i * 4.5f;
                CreateWallTorch(decorRoot, new Vector3(-torchX, 1.55f, z), -1);
                CreateWallTorch(decorRoot, new Vector3(torchX, 1.55f, z + 2.2f), 1);
            }
        }

        void CreateWallTorch(Transform parent, Vector3 localPos, int sideSign)
        {
            var torch = new GameObject("WallTorch");
            torch.transform.SetParent(parent, false);
            torch.transform.localPosition = localPos;
            torch.transform.localRotation = Quaternion.Euler(0f, sideSign < 0 ? 90f : -90f, 0f);

            Material woodMat = KnightRunMaterials.Get(KnightRunTexture.TreeTrunk);
            Material flameMat = KnightRunMaterials.Get(KnightRunTexture.LavaPool);

            var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "Bracket";
            bracket.transform.SetParent(torch.transform, false);
            bracket.transform.localScale = new Vector3(0.12f, 0.08f, 0.28f);
            bracket.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            bracket.GetComponent<Renderer>().sharedMaterial = woodMat;
            Object.Destroy(bracket.GetComponent<Collider>());

            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick.name = "Stick";
            stick.transform.SetParent(torch.transform, false);
            stick.transform.localScale = new Vector3(0.07f, 0.22f, 0.07f);
            stick.transform.localPosition = new Vector3(0f, 0.18f, 0.12f);
            stick.GetComponent<Renderer>().sharedMaterial = woodMat;
            Object.Destroy(stick.GetComponent<Collider>());

            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "Flame";
            flame.transform.SetParent(torch.transform, false);
            flame.transform.localScale = new Vector3(0.18f, 0.28f, 0.18f);
            flame.transform.localPosition = new Vector3(0f, 0.42f, 0.12f);
            flame.GetComponent<Renderer>().sharedMaterial = flameMat;
            Object.Destroy(flame.GetComponent<Collider>());
        }

        void BuildMineCartDecor()
        {
            Material railMat = KnightRunMaterials.Get(KnightRunTexture.MineRail, new Vector2(1f, 8f));
            float[] lanes = PhaseTrackLayout.GetLanePositions(currentSettings);
            for (int lane = 0; lane < lanes.Length; lane++)
            {
                float x = lanes[lane];
                CreateRail(decorRoot, new Vector3(x - 0.35f, 0.05f, Length * 0.5f), railMat);
                CreateRail(decorRoot, new Vector3(x + 0.35f, 0.05f, Length * 0.5f), railMat);
            }
        }

        void CreateRail(Transform parent, Vector3 localPos, Material material)
        {
            CreateBox("Rail", new Vector3(0.12f, 0.12f, Length), localPos, material, parent);
        }

        void BuildVolcanoDecor()
        {
            Material lavaMat = KnightRunMaterials.Get(KnightRunTexture.LavaPool);
            Material rockMat = KnightRunMaterials.Get(KnightRunTexture.VolcanoRock);
            float playable = PhaseTrackLayout.GetPlayableMaxX(currentSettings);

            // Dense lava patches so the red ground reads clearly end-to-end.
            for (int i = 0; i < 6; i++)
            {
                float z = 1.5f + i * 3.1f;
                float x = Mathf.Lerp(-playable + 0.8f, playable - 0.8f, (i % 3) / 2f);
                CreateBox(
                    "LavaPool",
                    new Vector3(Random.Range(1.6f, 2.4f), 0.06f, Random.Range(1.2f, 1.8f)),
                    new Vector3(x, 0.03f, z),
                    lavaMat,
                    decorRoot);
                CreateBox(
                    "VolcanoRock",
                    new Vector3(0.85f, 0.55f, 0.85f),
                    new Vector3(-x * 0.75f, 0.28f, z + 1.1f),
                    rockMat,
                    decorRoot);
            }
        }

        Transform CreateBox(string name, Vector3 scale, Vector3 localPosition, Material material, Transform parent = null, bool keepCollider = false)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent != null ? parent : transform, false);
            box.transform.localScale = scale;
            box.transform.localPosition = localPosition;
            box.GetComponent<Renderer>().sharedMaterial = material;

            if (keepCollider)
            {
                box.isStatic = true;
            }
            else
            {
                Object.Destroy(box.GetComponent<Collider>());
            }

            return box.transform;
        }

        public void ApplyTheme(RunPhaseSettings settings)
        {
            currentSettings = settings;
            Phase = settings.phase;
            ApplyLayout(settings);

            Material groundMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Ground);
            Material wallMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Wall, new Vector2(1f, 4f));

            SetMaterial(ground, groundMat);
            SetMaterial(leftWall, wallMat);
            SetMaterial(rightWall, wallMat);
            ApplyEnvironmentVisibility(settings.phase);
        }

        public void BeginPhaseTransition(RunPhaseSettings settings, float duration)
        {
            currentSettings = settings;
            Phase = settings.phase;
            phaseTransitionDuration = Mathf.Max(0.1f, duration);
            phaseTransitionElapsed = 0f;
            phaseTransitionActive = true;
            transitionMaterialsSwapped = false;

            transitionGroundScale = ground != null
                ? ground.localScale
                : new Vector3(PhaseTrackLayout.GetGroundWidth(settings), 0.4f, Length);
            transitionLeftWallPosition = leftWall != null
                ? leftWall.localPosition
                : Vector3.zero;
            transitionRightWallPosition = rightWall != null
                ? rightWall.localPosition
                : Vector3.zero;

            transitionGroundMaterial = KnightRunMaterials.GetForPhase(
                settings.phase,
                PhaseSurface.Ground);
            transitionWallMaterial = KnightRunMaterials.GetForPhase(
                settings.phase,
                PhaseSurface.Wall,
                new Vector2(1f, 4f));
        }

        void Update()
        {
            if (!phaseTransitionActive)
                return;

            phaseTransitionElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(phaseTransitionElapsed / phaseTransitionDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            ApplyTransitionLayout(smoothProgress);
            ApplyTransitionTheme(progress);

            if (progress < 1f)
                return;

            phaseTransitionActive = false;
            ApplyLayout(currentSettings);
            SetMaterial(ground, transitionGroundMaterial);
            SetMaterial(leftWall, transitionWallMaterial);
            SetMaterial(rightWall, transitionWallMaterial);
        }

        void ApplyTransitionLayout(float progress)
        {
            float targetGroundWidth = PhaseTrackLayout.GetGroundWidth(currentSettings);
            float targetLeftX = PhaseTrackLayout.GetWallCenterX(currentSettings, -1);
            float targetRightX = PhaseTrackLayout.GetWallCenterX(currentSettings, 1);

            if (ground != null)
            {
                ground.localScale = Vector3.Lerp(
                    transitionGroundScale,
                    new Vector3(targetGroundWidth, 0.4f, Length),
                    progress);
            }

            if (leftWall != null)
            {
                leftWall.localPosition = Vector3.Lerp(
                    transitionLeftWallPosition,
                    new Vector3(targetLeftX, 1.5f, Length * 0.5f),
                    progress);
            }

            if (rightWall != null)
            {
                rightWall.localPosition = Vector3.Lerp(
                    transitionRightWallPosition,
                    new Vector3(targetRightX, 1.5f, Length * 0.5f),
                    progress);
            }
        }

        void ApplyTransitionTheme(float progress)
        {
            if (!transitionMaterialsSwapped && progress >= 0.5f)
            {
                transitionMaterialsSwapped = true;
                SetMaterial(ground, transitionGroundMaterial);
                SetMaterial(leftWall, transitionWallMaterial);
                SetMaterial(rightWall, transitionWallMaterial);
                ApplyEnvironmentVisibility(currentSettings.phase);
            }

            float brightness = progress < 0.5f
                ? Mathf.Lerp(1f, 0.35f, progress * 2f)
                : Mathf.Lerp(0.35f, 1f, (progress - 0.5f) * 2f);

            SetBrightness(ground, brightness);
            SetBrightness(leftWall, brightness);
            SetBrightness(rightWall, brightness);
        }

        static void SetBrightness(Transform target, float brightness)
        {
            if (target == null)
                return;

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
                return;

            Color color = Color.white * brightness;
            color.a = 1f;
            renderer.material.color = color;
            if (renderer.material.HasProperty("_BaseColor"))
                renderer.material.SetColor("_BaseColor", color);
        }

        public void ApplyLayout(RunPhaseSettings settings)
        {
            currentSettings = settings;

            if (ground != null)
            {
                ground.localScale = new Vector3(PhaseTrackLayout.GetGroundWidth(settings), 0.4f, Length);
                ground.localPosition = new Vector3(0f, -0.2f, Length * 0.5f);
            }

            if (leftWall != null)
            {
                leftWall.localScale = new Vector3(PhaseTrackLayout.WallThickness, 3f, Length);
                leftWall.localPosition = new Vector3(PhaseTrackLayout.GetWallCenterX(settings, -1), 1.5f, Length * 0.5f);
            }

            if (rightWall != null)
            {
                rightWall.localScale = new Vector3(PhaseTrackLayout.WallThickness, 3f, Length);
                rightWall.localPosition = new Vector3(PhaseTrackLayout.GetWallCenterX(settings, 1), 1.5f, Length * 0.5f);
            }
        }

        static void SetMaterial(Transform target, Material material)
        {
            if (target == null)
                return;

            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        void ApplyEnvironmentVisibility(RunPhase phase)
        {
            LowPolyBiomeCatalog.BiomeSet set = LowPolyBiomeCatalog.UsesPrefabEnvironment(phase)
                ? LowPolyBiomeCatalog.Instance?.GetSet(phase)
                : null;

            bool hideGround = set != null
                && set.replaceGround
                && set.groundPrefabs != null
                && set.groundPrefabs.Length > 0
                && biomeDecorRoot != null;

            bool hideWalls = phase == RunPhase.Forest
                || (set != null
                    && set.replaceWalls
                    && biomeDecorRoot != null
                    && ((set.wallPrefabs != null && set.wallPrefabs.Length > 0)
                        || (set.rockPrefabs != null && set.rockPrefabs.Length > 0)));

            SetWallRenderersVisible(!hideWalls);
            SetRendererVisible(ground, !hideGround);
            SetForestDecorVisible(phase == RunPhase.Forest);
            if (biomeDecorRoot != null)
            {
                bool showBiome = LowPolyBiomeCatalog.UsesPrefabEnvironment(phase);
                biomeDecorRoot.gameObject.SetActive(showBiome);
            }
        }

        void SetWallRenderersVisible(bool visible)
        {
            SetRendererVisible(leftWall, visible);
            SetRendererVisible(rightWall, visible);
        }

        void SetForestDecorVisible(bool visible)
        {
            if (forestDecorRoot != null)
                forestDecorRoot.gameObject.SetActive(visible);
        }

        static void SetRendererVisible(Transform target, bool visible)
        {
            if (target == null)
                return;

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = visible;
        }
    }
}
