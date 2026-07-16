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

            ConfigureRuntimePerformance();
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

        static void ConfigureRuntimePerformance()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
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
            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            playerGo.transform.position = RunnerController.StartPosition;

            var controller = playerGo.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0f, 1f, 0f);

            playerGo.AddComponent<PlayerAnimationDriver>();

            var characterVisual = playerGo.AddComponent<PlayerCharacterVisual>();
            characterVisual.Build();
            DebugTestMode.AttachAnchorIfNeeded(playerGo.transform);

            var mineCartVisual = playerGo.AddComponent<MineCartVisual>();
            mineCartVisual.Build(playerGo.transform);

            var swordVisual = playerGo.AddComponent<SwordVisual>();
            var bowVisual = playerGo.AddComponent<BowVisual>();
            var shurikenVisual = playerGo.AddComponent<ShurikenVisual>();
            var magicBookVisual = playerGo.AddComponent<MagicBookVisual>();
            var bombVisual = playerGo.AddComponent<BombVisual>();
            var boomerangVisual = playerGo.AddComponent<BoomerangVisual>();
            var throwingAxeVisual = playerGo.AddComponent<ThrowingAxeVisual>();
            var slideVisual = playerGo.AddComponent<KnightSlideVisual>();

            void BindVisualAttachments()
            {
                Transform fallback = playerGo.transform;
                swordVisual.Build(characterVisual.GetWeaponParent(WeaponMount.RightHand, fallback));
                bowVisual.Build(characterVisual.GetWeaponParent(WeaponMount.LeftHand, fallback));
                shurikenVisual.Build(characterVisual.GetWeaponParent(WeaponMount.Throw, fallback));
                bombVisual.Build(characterVisual.GetWeaponParent(WeaponMount.Throw, fallback));
                boomerangVisual.Build(characterVisual.GetWeaponParent(WeaponMount.Throw, fallback));
                throwingAxeVisual.Build(characterVisual.GetWeaponParent(WeaponMount.Throw, fallback));
                magicBookVisual.Build(characterVisual.VisualRoot != null ? characterVisual.VisualRoot : fallback);

                if (characterVisual.IsLegacyVisual)
                {
                    slideVisual.BuildLegacy(
                        characterVisual.LegacyHelmet,
                        swordVisual.Pivot,
                        characterVisual.LegacyBodyRenderer);
                }
                else
                {
                    slideVisual.BuildModular(characterVisual.VisualRoot, swordVisual.Pivot);
                }
            }

            BindVisualAttachments();
            characterVisual.OnVisualRebuilt += () =>
            {
                BindVisualAttachments();
                playerGo.GetComponent<HeroUpgradeStats>()?.RefreshConsumers();
            };

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
