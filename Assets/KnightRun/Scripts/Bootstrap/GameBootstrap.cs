using KnightRun.CameraSystem;
using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Player;
using KnightRun.Progression;
using KnightRun.Meta;
using KnightRun.UI;
using KnightRun.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnightRun
{
    public static class GameBootstrap
    {
        static GameObject runtimeRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            runtimeRoot = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            BuildGame();
        }

        public static void BuildGame()
        {
            if (runtimeRoot != null)
            {
                Object.Destroy(runtimeRoot);
                runtimeRoot = null;
            }

            SuppressDefaultSceneVisuals();
            ConfigureInput();

            runtimeRoot = new GameObject("KnightRun_Root");

            var gameManagerGo = new GameObject("GameManager");
            gameManagerGo.transform.SetParent(runtimeRoot.transform, false);
            gameManagerGo.AddComponent<GameManager>();
            gameManagerGo.AddComponent<RunPhaseManager>();
            gameManagerGo.AddComponent<PhaseBossController>();
            gameManagerGo.AddComponent<MetaProgression>();
            gameManagerGo.AddComponent<UpgradeManager>();

            var worldGo = new GameObject("World");
            worldGo.transform.SetParent(runtimeRoot.transform, false);
            worldGo.AddComponent<TrackSegmentSpawner>();

            var player = CreatePlayer();
            player.transform.SetParent(runtimeRoot.transform, false);
            var cameraFollow = SetupCamera(player.transform);

            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(runtimeRoot.transform, false);
            var runUI = uiGo.AddComponent<RunUI>();
            runUI.Build();
            uiGo.AddComponent<UpgradeSelectionUI>().Build();
            uiGo.AddComponent<MainMenuUI>().Build();

            var playerUpgrades = player.GetComponent<HeroUpgradeStats>();
            if (playerUpgrades == null)
                playerUpgrades = player.gameObject.AddComponent<HeroUpgradeStats>();

            UpgradeManager.Instance?.BindHero(playerUpgrades);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = RunPhaseDefaults.All[0].ambientColor;

            var lightGo = new GameObject("Directional Light");
            lightGo.transform.SetParent(runtimeRoot.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.88f);

            RunUI.Instance?.Hide();
            MainMenuUI.Instance?.Show();
            GameManager.Instance?.EnterMainMenu();
        }

        public static void StartRunFromMenu()
        {
            MainMenuUI.Instance?.Hide();
            RunUI.Instance?.Show();
            RestartGame();
        }

        public static void ReturnToMainMenu()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null &&
                (gameManager.State == GameState.Running || gameManager.State == GameState.ChoosingUpgrade))
            {
                MetaProgression.Instance?.BankRun(
                    gameManager.Coins,
                    gameManager.Score,
                    gameManager.Distance,
                    gameManager.EnemiesDefeated);
            }

            UpgradeManager.Instance?.ResetProgression();

            var phaseManager = RunPhaseManager.Instance;
            phaseManager?.ResetPhases();

            var player = Object.FindFirstObjectByType<RunnerController>();
            if (player != null)
                player.ResetToStart(RunnerController.StartPosition);

            var spawner = Object.FindFirstObjectByType<TrackSegmentSpawner>();
            spawner?.ResetSpawner();

            player?.SetMineCartMode(false);
            RenderSettings.ambientLight = RunPhaseDefaults.All[0].ambientColor;

            GameManager.Instance?.EnterMainMenu();
            RunUI.Instance?.Hide();
            MainMenuUI.Instance?.Show();
        }

        static void ConfigureInput()
        {
            if (InputSystem.settings == null)
                return;

            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        }

        static void SuppressDefaultSceneVisuals()
        {
            HideRootObject("Plane");
            HideRootObject("Cube");
        }

        static void HideRootObject(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            if (target != null && target.transform.parent == null)
                target.SetActive(false);
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

            var bowVisual = playerGo.AddComponent<BowVisual>();
            bowVisual.Build(playerGo.transform);

            var shurikenVisual = playerGo.AddComponent<ShurikenVisual>();
            shurikenVisual.Build(playerGo.transform);

            var magicBookVisual = playerGo.AddComponent<MagicBookVisual>();
            magicBookVisual.Build(playerGo.transform);

            var bombVisual = playerGo.AddComponent<BombVisual>();
            bombVisual.Build(playerGo.transform);

            var boomerangVisual = playerGo.AddComponent<BoomerangVisual>();
            boomerangVisual.Build(playerGo.transform);

            var throwingAxeVisual = playerGo.AddComponent<ThrowingAxeVisual>();
            throwingAxeVisual.Build(playerGo.transform);

            var slideVisual = playerGo.AddComponent<KnightSlideVisual>();
            slideVisual.Build(helmet.transform, swordVisual.Pivot, bodyRenderer);

            playerGo.AddComponent<HeroUpgradeStats>();
            playerGo.AddComponent<KnightSwordAttack>();
            playerGo.AddComponent<KnightBowAttack>();
            playerGo.AddComponent<KnightShurikenAttack>();
            playerGo.AddComponent<KnightMagicBookOrbit>();
            playerGo.AddComponent<KnightBombAttack>();
            playerGo.AddComponent<KnightBoomerangAttack>();
            playerGo.AddComponent<KnightThrowingAxeAttack>();
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
            GameManager.Instance?.RestartRun();

            var phaseManager = RunPhaseManager.Instance;
            phaseManager?.ResetPhases();

            UpgradeManager.Instance?.ResetProgression();

            var player = Object.FindFirstObjectByType<RunnerController>();
            if (player != null)
                player.ResetToStart(RunnerController.StartPosition);

            var spawner = Object.FindFirstObjectByType<TrackSegmentSpawner>();
            spawner?.ResetSpawner();

            if (phaseManager != null)
                player?.SetMineCartMode(false);

            RenderSettings.ambientLight = RunPhaseDefaults.All[0].ambientColor;

            GameManager.Instance?.StartRun();
        }
    }
}
