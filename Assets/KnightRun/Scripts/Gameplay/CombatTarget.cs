using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class CombatTarget
    {
        public static bool TryApplyDamage(Collider hit, float damage)
        {
            if (hit == null || hit.CompareTag("Player"))
                return false;

            int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));

            Enemy enemy = hit.GetComponent<Enemy>() ?? hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(roundedDamage);
                return true;
            }

            Boss boss = hit.GetComponent<Boss>() ?? hit.GetComponentInParent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(roundedDamage);
                return true;
            }

            return false;
        }

        public static Transform FindNearest(Vector3 origin)
        {
            Transform nearest = null;
            float bestDistance = float.MaxValue;

            Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in enemies)
            {
                if (enemy == null)
                    continue;

                float distance = (enemy.transform.position - origin).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                nearest = enemy.transform;
            }

            Boss[] bosses = Object.FindObjectsByType<Boss>(FindObjectsSortMode.None);
            foreach (Boss boss in bosses)
            {
                if (boss == null)
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
