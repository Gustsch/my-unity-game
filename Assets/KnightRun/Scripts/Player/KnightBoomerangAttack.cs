using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightBoomerangAttack : MonoBehaviour
    {
        BoomerangVisual boomerangVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;
        bool boomerangInFlight;

        float TravelSpeed
        {
            get
            {
                if (UpgradeStats == null)
                    return SkillPool.BoomerangBaseSpeed;

                float speedMultiplier = UpgradeStats.AttackSpeedMultiplier * MetaBonuses.AttackSpeedMultiplier;
                return SkillPool.BoomerangBaseSpeed * speedMultiplier;
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
            boomerangVisual = GetComponent<BoomerangVisual>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateBoomerangState;

            UpdateBoomerangState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateBoomerangState;
        }

        void UpdateBoomerangState()
        {
            if (boomerangVisual == null || UpgradeStats == null)
                return;

            bool visible = UpgradeStats.HasBoomerang && !boomerangInFlight;
            boomerangVisual.SetVisible(visible);
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasBoomerang || boomerangInFlight)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            Transform target = CombatTarget.FindNearest(transform.position);
            if (target == null)
                return;

            ThrowBoomerang(target);
        }

        void ThrowBoomerang(Transform target)
        {
            boomerangInFlight = true;
            UpdateBoomerangState();

            Vector3 spawnPosition = boomerangVisual != null
                ? boomerangVisual.ThrowPosition
                : transform.position + Vector3.up;

            BoomerangProjectile.Spawn(
                spawnPosition,
                transform,
                target,
                UpgradeStats.BoomerangDamage,
                TravelSpeed,
                UpgradeStats.AttackAreaMultiplier,
                HandleBoomerangReturned);
        }

        void HandleBoomerangReturned()
        {
            boomerangInFlight = false;
            UpdateBoomerangState();
        }
    }
}
