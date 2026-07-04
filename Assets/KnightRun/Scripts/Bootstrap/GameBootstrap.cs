using KnightRun.CameraSystem;
using KnightRun.Core;
using KnightRun.Player;
using KnightRun.UI;
using KnightRun.World;
using UnityEngine;

namespace KnightRun
{
    public static class GameBootstrap
    {
        static GameObject runtimeRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (runtimeRoot != null)
                return;

            BuildGame();
        }

        public static void BuildGame()
        {
            if (runtimeRoot != null)
                Object.Destroy(runtimeRoot);

            runtimeRoot = new GameObject("KnightRun_Root");

            var gameManagerGo = new GameObject("GameManager");
            gameManagerGo.transform.SetParent(runtimeRoot.transform, false);
            gameManagerGo.AddComponent<GameManager>();
            gameManagerGo.AddComponent<RunPhaseManager>();

            var worldGo = new GameObject("World");
            worldGo.transform.SetParent(runtimeRoot.transform, false);
            worldGo.AddComponent<TrackSegmentSpawner>();

            var player = CreatePlayer();
            var cameraFollow = SetupCamera(player.transform);

            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(runtimeRoot.transform, false);
            uiGo.AddComponent<RunUI>().Build();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = RunPhaseDefaults.All[0].ambientColor;

            var lightGo = new GameObject("Directional Light");
            lightGo.transform.SetParent(runtimeRoot.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.88f);

            Object.DontDestroyOnLoad(runtimeRoot);
        }

        static RunnerController CreatePlayer()
        {
            var playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Knight";
            playerGo.tag = "Player";
            playerGo.transform.position = RunnerController.StartPosition;

            Object.Destroy(playerGo.GetComponent<CapsuleCollider>());

            var controller = playerGo.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 1f, 0f);

            var bodyRenderer = playerGo.GetComponent<Renderer>();
            bodyRenderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightArmor);

            var helmet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            helmet.name = "Helmet";
            helmet.transform.SetParent(playerGo.transform, false);
            helmet.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
            helmet.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            helmet.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightHelmet);
            Object.Destroy(helmet.GetComponent<Collider>());

            var mineCartVisual = playerGo.AddComponent<MineCartVisual>();
            mineCartVisual.Build(playerGo.transform);

            var swordVisual = playerGo.AddComponent<SwordVisual>();
            swordVisual.Build(playerGo.transform);

            var slideVisual = playerGo.AddComponent<KnightSlideVisual>();
            slideVisual.Build(helmet.transform, swordVisual.Pivot, bodyRenderer);

            playerGo.AddComponent<KnightSwordAttack>();
            playerGo.AddComponent<KnightHealth>();

            return playerGo.AddComponent<RunnerController>();
        }

        static CameraFollow SetupCamera(Transform target)
        {
            Camera mainCamera = Camera.main;
            GameObject cameraGo;

            if (mainCamera == null)
            {
                cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                mainCamera = cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
            }
            else
            {
                cameraGo = mainCamera.gameObject;
            }

            var follow = cameraGo.GetComponent<CameraFollow>() ?? cameraGo.AddComponent<CameraFollow>();
            follow.SetTarget(target);
            return follow;
        }

        public static void RestartGame()
        {
            var player = Object.FindFirstObjectByType<RunnerController>();
            if (player != null)
                player.ResetToStart(RunnerController.StartPosition);

            var spawner = Object.FindFirstObjectByType<TrackSegmentSpawner>();
            spawner?.ResetSpawner();

            var phaseManager = RunPhaseManager.Instance;
            phaseManager?.ResetPhases();
            if (phaseManager != null)
                player?.SetMineCartMode(false);

            RenderSettings.ambientLight = RunPhaseDefaults.All[0].ambientColor;

            GameManager.Instance?.RestartRun();
            GameManager.Instance?.StartRun();
        }
    }
}
