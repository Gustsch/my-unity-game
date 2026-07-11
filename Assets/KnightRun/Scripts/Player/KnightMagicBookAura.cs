using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightMagicBookAura : MonoBehaviour
    {
        const float AuraCenterHeight = 0.55f;
        const float AuraHeight = 4f;

        MagicBookVisual bookVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;

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
            bookVisual = GetComponent<MagicBookVisual>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateBookState;

            UpdateBookState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateBookState;
        }

        void UpdateBookState()
        {
            if (bookVisual == null || UpgradeStats == null)
                return;

            bool hasBook = UpgradeStats.HasMagicBook;
            bookVisual.SetVisible(hasBook);

            if (hasBook)
                bookVisual.SetAuraRadiusMultiplier(UpgradeStats.AttackAreaMultiplier);
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasMagicBook)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            ApplyAuraDamage();
        }

        void ApplyAuraDamage()
        {
            float radius = SkillPool.MagicBookBaseAuraRadius * UpgradeStats.AttackAreaMultiplier;
            float dps = UpgradeStats.MagicBookAuraDps * UpgradeStats.AttackSpeedMultiplier * MetaBonuses.AttackSpeedMultiplier;
            float damage = dps * Time.deltaTime;

            if (damage <= 0f)
                return;

            Vector3 center = transform.position + Vector3.up * AuraCenterHeight;
            Vector3 halfExtents = new Vector3(radius, AuraHeight * 0.5f, radius);

            Collider[] hits = Physics.OverlapBox(
                center,
                halfExtents,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                    continue;

                CombatTarget.TryApplyDamage(hit, damage);
            }
        }
    }
}
