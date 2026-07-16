using KnightRun.Core;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.World
{
    public class TrackSegment : MonoBehaviour
    {
        public float Length = 20f;
        public RunPhase Phase { get; private set; }

        Transform leftWall;
        Transform rightWall;
        Transform ground;
        Transform decorRoot;
        Transform forestDecorRoot;
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
            SetWallRenderersVisible(settings.phase != RunPhase.Forest);

            decorRoot = new GameObject("Decor").transform;
            decorRoot.SetParent(transform, false);

            switch (settings.phase)
            {
                case RunPhase.Forest:
                    BuildForestDecor();
                    break;
                case RunPhase.Cave:
                    BuildCaveDecor();
                    break;
                case RunPhase.MineCart:
                    BuildMineCartDecor(wallMat);
                    break;
                case RunPhase.Volcano:
                    BuildVolcanoDecor();
                    break;
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
            Material stalactiteMat = KnightRunMaterials.Get(KnightRunTexture.Stalactite);
            for (int i = 0; i < 4; i++)
            {
                float z = 3f + i * 4.5f;
                CreateStalactite(decorRoot, new Vector3(-3.8f, 2.8f, z), stalactiteMat);
                CreateStalactite(decorRoot, new Vector3(3.8f, 2.8f, z + 2f), stalactiteMat);
            }
        }

        void CreateStalactite(Transform parent, Vector3 localPos, Material material)
        {
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = "Stalactite";
            spike.transform.SetParent(parent, false);
            spike.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            spike.transform.localPosition = localPos;
            spike.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            spike.GetComponent<Renderer>().sharedMaterial = material;
            Object.Destroy(spike.GetComponent<Collider>());
        }

        void BuildMineCartDecor(Material wallMat)
        {
            Material railMat = KnightRunMaterials.Get(KnightRunTexture.MineRail, new Vector2(1f, 8f));
            float[] lanes = PhaseTrackLayout.GetLanePositions(currentSettings);
            for (int lane = 0; lane < lanes.Length; lane++)
            {
                float x = lanes[lane];
                CreateRail(decorRoot, new Vector3(x - 0.35f, 0.05f, Length * 0.5f), railMat);
                CreateRail(decorRoot, new Vector3(x + 0.35f, 0.05f, Length * 0.5f), railMat);
            }

            float beamWidth = PhaseTrackLayout.GetGroundWidth(currentSettings) + 0.5f;
            for (int i = 0; i < 3; i++)
            {
                float z = 4f + i * 6f;
                CreateBox("SupportBeam", new Vector3(beamWidth, 0.25f, 0.35f), new Vector3(0f, 2.6f, z), wallMat, decorRoot);
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

            for (int i = 0; i < 4; i++)
            {
                float z = 2.5f + i * 4.2f;
                CreateBox("LavaPool", new Vector3(1.4f, 0.08f, 1.1f), new Vector3(-2.2f + i * 1.1f, 0.04f, z), lavaMat, decorRoot);
                CreateBox("VolcanoRock", new Vector3(0.9f, 0.7f, 0.9f), new Vector3(2.6f - i * 0.8f, 0.35f, z + 1.2f), rockMat, decorRoot);
            }

            for (int i = 0; i < 3; i++)
            {
                float z = 6f + i * 5.5f;
                CreateStalactite(decorRoot, new Vector3(-3.5f, 2.6f, z), rockMat);
                CreateStalactite(decorRoot, new Vector3(3.5f, 2.6f, z + 1.8f), rockMat);
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
            SetWallRenderersVisible(settings.phase != RunPhase.Forest);
            SetForestDecorVisible(settings.phase == RunPhase.Forest);
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
                SetWallRenderersVisible(currentSettings.phase != RunPhase.Forest);
                SetForestDecorVisible(currentSettings.phase == RunPhase.Forest);
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
