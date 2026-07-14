using System.Collections.Generic;
using KnightRun.Player;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class CombatTarget
    {
        public static bool TryApplyDamage(Collider hit, float damage, HashSet<int> alreadyHitTargets = null)
        {
            return TryApplyDamage(hit, damage, swordMeleeOnly: false, alreadyHitTargets);
        }

        public static bool TryApplySwordDamage(Collider hit, float damage, HashSet<int> alreadyHitTargets = null)
        {
            return TryApplyDamage(hit, damage, swordMeleeOnly: true, alreadyHitTargets);
        }

        static bool TryApplyDamage(Collider hit, float damage, bool swordMeleeOnly, HashSet<int> alreadyHitTargets)
        {
            if (hit == null || hit.CompareTag("Player"))
                return false;

            if (!TryResolveTarget(hit, swordMeleeOnly, out Component target))
                return false;

            int targetId = target.GetInstanceID();
            if (alreadyHitTargets != null && !alreadyHitTargets.Add(targetId))
                return false;

            float finalDamage = ApplyCritical(damage);
            int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage));
            ApplyDamage(target, roundedDamage);
            return true;
        }

        static float ApplyCritical(float damage)
        {
            HeroUpgradeStats stats = Object.FindFirstObjectByType<HeroUpgradeStats>();
            if (stats == null || stats.CriticalChance <= 0f)
                return damage;

            if (Random.value >= stats.CriticalChance)
                return damage;

            return damage * SkillPool.CriticalStrikeDamageMultiplier;
        }

        static bool TryResolveTarget(Collider hit, bool swordMeleeOnly, out Component target)
        {
            target = null;

            Enemy enemy = hit.GetComponent<Enemy>() ?? hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                target = enemy;
                return true;
            }

            BatEnemy bat = hit.GetComponent<BatEnemy>() ?? hit.GetComponentInParent<BatEnemy>();
            if (bat != null)
            {
                if (swordMeleeOnly && !bat.IsOpenToSwordAttack)
                    return false;

                target = bat;
                return true;
            }

            Boss boss = hit.GetComponent<Boss>() ?? hit.GetComponentInParent<Boss>();
            if (boss != null)
            {
                target = boss;
                return true;
            }

            return false;
        }

        static void ApplyDamage(Component target, int damage)
        {
            switch (target)
            {
                case Enemy enemy:
                    enemy.TakeDamage(damage);
                    break;
                case BatEnemy bat:
                    bat.TakeDamage(damage);
                    break;
                case Boss boss:
                    boss.TakeDamage(damage);
                    break;
            }
        }

        public static Transform FindNearest(Vector3 origin)
        {
            Transform nearest = null;
            float bestDistance = float.MaxValue;
            RunnerController runner = Object.FindFirstObjectByType<RunnerController>();
            float playerZ = runner != null ? runner.transform.position.z : origin.z;

            Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in enemies)
            {
                if (enemy == null || enemy.transform.position.z <= playerZ)
                    continue;

                float distance = (enemy.transform.position - origin).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                nearest = enemy.transform;
            }

            BatEnemy[] bats = Object.FindObjectsByType<BatEnemy>(FindObjectsSortMode.None);
            foreach (BatEnemy bat in bats)
            {
                if (bat == null || bat.transform.position.z <= playerZ)
                    continue;

                float distance = (bat.transform.position - origin).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                nearest = bat.transform;
            }

            Boss[] bosses = Object.FindObjectsByType<Boss>(FindObjectsSortMode.None);
            foreach (Boss boss in bosses)
            {
                if (boss == null || boss.transform.position.z <= playerZ)
                    continue;

                float distance = (boss.transform.position - origin).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                nearest = boss.transform;
            }

            return nearest;
        }
    }
}
