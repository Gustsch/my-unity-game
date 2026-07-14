using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightBowAttack : MonoBehaviour
    {
        const float ArrowTravelHeight = 0.5f;

        BowVisual bowVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;

        float attackTimer;

        float AttackInterval
        {
            get
            {
                if (UpgradeStats == null)
                    return SkillPool.BowAttackInterval;

                float speedMultiplier = UpgradeStats.AttackSpeedMultiplier * MetaBonuses.AttackSpeedMultiplier;
                return Mathf.Max(SkillPool.BowMinAttackInterval, SkillPool.BowAttackInterval / speedMultiplier);
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
            bowVisual = GetComponent<BowVisual>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            attackTimer = AttackInterval * 0.5f;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateBowState;

            UpdateBowState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateBowState;
        }

        void UpdateBowState()
        {
            if (bowVisual == null || UpgradeStats == null)
                return;

            bool hasBow = UpgradeStats.HasBow;
            bowVisual.SetVisible(hasBow);

            if (hasBow)
                bowVisual.SetAttackAreaMultiplier(UpgradeStats.AttackAreaMultiplier);
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasBow)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                FireArrow();
                attackTimer = AttackInterval;
            }
        }

        void FireArrow()
        {
            int volleyCount = UpgradeStats.AttackVolleyCount;
            int pierceExtraHits = UpgradeStats.RollPierceExtraHits();

            for (int i = 0; i < volleyCount; i++)
            {
                Vector3 spawnPosition = GetArrowSpawnPosition();
                float spread = (i - (volleyCount - 1) * 0.5f) * 0.45f;
                spawnPosition.x += spread;

                ArrowProjectile.Spawn(
                    spawnPosition,
                    UpgradeStats.BowDamage,
                    UpgradeStats.AttackAreaMultiplier,
                    pierceExtraHits);
            }
        }

        Vector3 GetArrowSpawnPosition()
        {
            Vector3 spawnPosition = bowVisual != null
                ? bowVisual.ArrowSpawnPosition
                : transform.position + Vector3.forward * 1.2f;

            spawnPosition.y = ArrowTravelHeight;
            return spawnPosition;
        }
    }
}
