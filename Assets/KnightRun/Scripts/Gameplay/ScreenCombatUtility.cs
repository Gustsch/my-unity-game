using System.Collections.Generic;
using KnightRun.Player;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class ScreenCombatUtility
    {
        const float ViewportPadding = 0.02f;
        const float FallbackAheadDistance = 28f;
        const float FallbackBehindDistance = 2f;

        public static bool TryGetRandomOnScreenTarget(out Transform target)
        {
            target = null;
            Camera camera = Camera.main;
            var candidates = new List<Transform>();

            Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null)
                    continue;

                if (IsOnScreen(enemy.transform.position, camera))
                    candidates.Add(enemy.transform);
            }

            BatEnemy[] bats = Object.FindObjectsByType<BatEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < bats.Length; i++)
            {
                BatEnemy bat = bats[i];
                if (bat == null)
                    continue;

                if (IsOnScreen(bat.transform.position, camera))
                    candidates.Add(bat.transform);
            }

            Boss[] bosses = Object.FindObjectsByType<Boss>(FindObjectsSortMode.None);
            for (int i = 0; i < bosses.Length; i++)
            {
                Boss boss = bosses[i];
                if (boss == null)
                    continue;

                if (IsOnScreen(boss.transform.position, camera))
                    candidates.Add(boss.transform);
            }

            if (candidates.Count == 0)
                return false;

            target = candidates[Random.Range(0, candidates.Count)];
            return true;
        }

        public static int KillAllEnemiesOnScreen()
        {
            Camera camera = Camera.main;
            Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            var toKill = new List<Enemy>(enemies.Length);
            int killed = 0;

            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null)
                    continue;

                if (IsOnScreen(enemy.transform.position, camera))
                    toKill.Add(enemy);
            }

            for (int i = 0; i < toKill.Count; i++)
            {
                if (toKill[i] != null)
                {
                    toKill[i].ForceKill();
                    killed++;
                }
            }

            BatEnemy[] bats = Object.FindObjectsByType<BatEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < bats.Length; i++)
            {
                BatEnemy bat = bats[i];
                if (bat == null)
                    continue;

                if (!IsOnScreen(bat.transform.position, camera))
                    continue;

                bat.ForceKill();
                killed++;
            }

            return killed;
        }

        public static bool IsOnScreen(Vector3 worldPosition, Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;

            if (camera == null)
                return IsOnScreenFallback(worldPosition);

            Vector3 viewport = camera.WorldToViewportPoint(worldPosition + Vector3.up * 0.5f);
            if (viewport.z <= 0f)
                return false;

            return viewport.x >= -ViewportPadding
                && viewport.x <= 1f + ViewportPadding
                && viewport.y >= -ViewportPadding
                && viewport.y <= 1f + ViewportPadding;
        }

        static bool IsOnScreenFallback(Vector3 worldPosition)
        {
            RunnerController runner = Object.FindFirstObjectByType<RunnerController>();
            if (runner == null)
                return true;

            float relativeZ = worldPosition.z - runner.transform.position.z;
            return relativeZ >= -FallbackBehindDistance && relativeZ <= FallbackAheadDistance;
        }
    }
}
