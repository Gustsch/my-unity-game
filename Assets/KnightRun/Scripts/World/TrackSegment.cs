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
        RunPhaseSettings currentSettings;

        public void Build(RunPhaseSettings settings, float zPosition)
        {
            currentSettings = settings;
            Phase = settings.phase;
            transform.position = new Vector3(0f, 0f, zPosition);

            Material groundMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Ground, new Vector2(2f, 4f));
            Material wallMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Wall, new Vector2(1f, 4f));

            ground = CreateBox("Ground", new Vector3(8f, 0.4f, Length), new Vector3(0f, -0.2f, Length * 0.5f), groundMat, keepCollider: true);
            leftWall = CreateBox("LeftWall", new Vector3(0.5f, 3f, Length), new Vector3(-4.25f, 1.5f, Length * 0.5f), wallMat);
            rightWall = CreateBox("RightWall", new Vector3(0.5f, 3f, Length), new Vector3(4.25f, 1.5f, Length * 0.5f), wallMat);

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
            for (int i = 0; i < 6; i++)
            {
                float z = 2f + i * 3.2f;
                CreateTree(decorRoot, new Vector3(-5.5f, 0f, z));
                CreateTree(decorRoot, new Vector3(5.5f, 0f, z + 1.5f));
            }
        }

        void CreateTree(Transform parent, Vector3 localPos)
        {
            var trunk = CreateBox("TreeTrunk", new Vector3(0.35f, 1.2f, 0.35f), localPos + new Vector3(0f, 0.6f, 0f),
                KnightRunMaterials.Get(KnightRunTexture.TreeTrunk), parent);
            CreateBox("TreeTop", new Vector3(1.2f, 1.4f, 1.2f), localPos + new Vector3(0f, 1.8f, 0f),
                KnightRunMaterials.Get(KnightRunTexture.TreeLeaves), parent);
            Object.Destroy(trunk.GetComponent<Collider>());
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
            for (int lane = 0; lane < 3; lane++)
            {
                float x = RunnerController.LanePositions[lane];
                CreateRail(decorRoot, new Vector3(x - 0.45f, 0.05f, Length * 0.5f), railMat);
                CreateRail(decorRoot, new Vector3(x + 0.45f, 0.05f, Length * 0.5f), railMat);
            }

            for (int i = 0; i < 3; i++)
            {
                float z = 4f + i * 6f;
                CreateBox("SupportBeam", new Vector3(8.5f, 0.25f, 0.35f), new Vector3(0f, 2.6f, z), wallMat, decorRoot);
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
                if (name == "Ground")
                    box.tag = "Ground";
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

            Material groundMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Ground, new Vector2(2f, 4f));
            Material wallMat = KnightRunMaterials.GetForPhase(settings.phase, PhaseSurface.Wall, new Vector2(1f, 4f));

            SetMaterial(ground, groundMat);
            SetMaterial(leftWall, wallMat);
            SetMaterial(rightWall, wallMat);
        }

        static void SetMaterial(Transform target, Material material)
        {
            if (target == null)
                return;

            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }
    }
}
