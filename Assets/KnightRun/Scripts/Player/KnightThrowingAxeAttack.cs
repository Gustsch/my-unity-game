using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightThrowingAxeAttack : MonoBehaviour
    {
        const float ThrowHeight = 1f;

        ThrowingAxeVisual axeVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;
        float attackTimer;

        float AttackInterval
        {
            get
            {
                if (UpgradeStats == null)
                    return SkillPool.ThrowingAxeAttackInterval;

                float speedMultiplier = UpgradeStats.AttackSpeedMultiplier * MetaBonuses.AttackSpeedMultiplier;
                return Mathf.Max(
                    SkillPool.ThrowingAxeMinAttackInterval,
                    SkillPool.ThrowingAxeAttackInterval / speedMultiplier);
            }
        }

        HeroUpgradeStats UpgradeStats
        {
            get
            {
                if (upgradeStats == null)
                    upgradeStats = GetComponent<HeroUpgradeStats>();
                return upgradeStats;
            }
        }

        void Awake()
        {
            axeVisual = GetComponent<ThrowingAxeVisual>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            attackTimer = AttackInterval * 0.5f;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateAxeState;

            UpdateAxeState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateAxeState;
        }

        void UpdateAxeState()
        {
            if (axeVisual == null || UpgradeStats == null)
                return;

            axeVisual.SetVisible(UpgradeStats.HasThrowingAxe);
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasThrowingAxe)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
                return;

            ThrowAxes();
            attackTimer = AttackInterval;
        }

        void ThrowAxes()
        {
            int volleyCount = UpgradeStats.AttackVolleyCount;
            int pierceExtraHits = UpgradeStats.RollPierceExtraHits();
            for (int i = 0; i < volleyCount; i++)
            {
                Vector3 spawnPosition = axeVisual != null
                    ? axeVisual.ThrowPosition
                    : transform.position + Vector3.up * ThrowHeight;

                Vector3 direction = GetThrowDirection(spawnPosition);

                ThrowingAxeProjectile.Spawn(
                    spawnPosition,
                    direction,
                    UpgradeStats.ThrowingAxeDamage,
                    SkillPool.ThrowingAxeSpeed,
                    UpgradeStats.AttackAreaMultiplier,
                    pierceExtraHits);
            }
        }

        static Vector3 GetThrowDirection(Vector3 spawnPosition)
        {
            Transform target = null;
            if (ScreenCombatUtility.TryGetRandomOnScreenTarget(out Transform onScreenTarget))
                target = onScreenTarget;
            else
                target = CombatTarget.FindNearest(spawnPosition);

            if (target == null)
                return Vector3.forward;

            Vector3 direction = target.position - spawnPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                return Vector3.forward;

            direction.Normalize();
            return direction;
        }
    }
}
