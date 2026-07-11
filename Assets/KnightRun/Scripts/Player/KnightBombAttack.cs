using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightBombAttack : MonoBehaviour
    {
        const float VolleySpread = 1.2f;

        BombVisual bombVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;

        float attackTimer;

        float AttackInterval
        {
            get
            {
                if (UpgradeStats == null)
                    return SkillPool.BombAttackInterval;

                float speedMultiplier = UpgradeStats.AttackSpeedMultiplier * MetaBonuses.AttackSpeedMultiplier;
                return Mathf.Max(SkillPool.BombMinAttackInterval, SkillPool.BombAttackInterval / speedMultiplier);
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
            bombVisual = GetComponent<BombVisual>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            attackTimer = AttackInterval * 0.5f;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateBombState;

            UpdateBombState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateBombState;
        }

        void UpdateBombState()
        {
            if (bombVisual == null || UpgradeStats == null)
                return;

            bombVisual.SetVisible(UpgradeStats.HasBomb);
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasBomb)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                ThrowBombs();
                attackTimer = AttackInterval;
            }
        }

        void ThrowBombs()
        {
            int volleyCount = UpgradeStats.AttackVolleyCount;
            Vector3 throwOrigin = bombVisual != null
                ? bombVisual.ThrowPosition
                : transform.position + Vector3.up * 1.1f;

            for (int i = 0; i < volleyCount; i++)
            {
                float spread = (i - (volleyCount - 1) * 0.5f) * VolleySpread;
                Vector3 from = throwOrigin + new Vector3(spread * 0.25f, 0f, 0f);
                Vector3 landPosition = transform.position + new Vector3(spread, 0f, SkillPool.BombFixedThrowDistance);

                BombProjectile.Throw(
                    from,
                    landPosition,
                    UpgradeStats.BombDamage,
                    UpgradeStats.AttackAreaMultiplier);
            }
        }
    }
}
