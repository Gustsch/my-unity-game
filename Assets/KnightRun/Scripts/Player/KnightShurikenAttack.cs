using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightShurikenAttack : MonoBehaviour
    {
        const float VolleyAngleSpread = 6f;

        ShurikenVisual shurikenVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;

        float attackTimer;

        float AttackInterval
        {
            get
            {
                if (UpgradeStats == null)
                    return SkillPool.ShurikenAttackInterval;

                float speedMultiplier = UpgradeStats.AttackSpeedMultiplier * MetaBonuses.AttackSpeedMultiplier;
                return Mathf.Max(
                    SkillPool.ShurikenMinAttackInterval,
                    SkillPool.ShurikenAttackInterval / speedMultiplier);
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
            shurikenVisual = GetComponent<ShurikenVisual>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            attackTimer = AttackInterval * 0.5f;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateShurikenState;

            UpdateShurikenState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateShurikenState;
        }

        void UpdateShurikenState()
        {
            if (shurikenVisual == null || UpgradeStats == null)
                return;

            bool hasShuriken = UpgradeStats.HasShuriken;
            shurikenVisual.SetVisible(hasShuriken);

            if (hasShuriken)
                shurikenVisual.SetAttackAreaMultiplier(UpgradeStats.AttackAreaMultiplier);
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasShuriken)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f)
                return;

            Transform target = CombatTarget.FindNearest(transform.position);
            if (target == null)
            {
                attackTimer = AttackInterval * 0.25f;
                return;
            }

            ThrowShurikens(target);
            attackTimer = AttackInterval;
        }

        void ThrowShurikens(Transform target)
        {
            Vector3 spawnPosition = shurikenVisual != null
                ? shurikenVisual.ThrowPosition
                : transform.position + Vector3.up;

            Vector3 aimDirection = target.position - spawnPosition;
            if (aimDirection.sqrMagnitude < 0.001f)
                aimDirection = Vector3.forward;
            aimDirection.Normalize();

            int volleyCount = UpgradeStats.AttackVolleyCount;
            int pierceExtraHits = UpgradeStats.RollPierceExtraHits();
            for (int i = 0; i < volleyCount; i++)
            {
                float angleOffset = (i - (volleyCount - 1) * 0.5f) * VolleyAngleSpread;
                Vector3 direction = Quaternion.AngleAxis(angleOffset, Vector3.up) * aimDirection;

                ShurikenProjectile.Spawn(
                    spawnPosition,
                    UpgradeStats.ShurikenDamage,
                    direction,
                    UpgradeStats.AttackAreaMultiplier,
                    pierceExtraHits);
            }
        }

    }
}
