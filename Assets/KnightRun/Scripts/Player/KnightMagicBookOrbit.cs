using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Meta;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    public class KnightMagicBookOrbit : MonoBehaviour
    {
        const float HitHeight = 3f;
        const float ContactTickInterval = 0.35f;

        MagicBookVisual bookVisual;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;
        int lastBookCount = -1;
        float lastAreaMultiplier = -1f;
        float contactTickTimer;

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
                stats.OnBonusesChanged += RefreshBooks;

            RefreshBooks();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= RefreshBooks;
        }

        void RefreshBooks()
        {
            if (bookVisual == null || UpgradeStats == null)
                return;

            int bookCount = UpgradeStats.MagicBookCount;
            float areaMultiplier = UpgradeStats.AttackAreaMultiplier;
            bool hasBooks = bookCount > 0;

            bookVisual.SetVisible(hasBooks);

            if (!hasBooks)
            {
                bookVisual.RebuildBooks(0, 1f);
                lastBookCount = 0;
                lastAreaMultiplier = areaMultiplier;
                return;
            }

            if (bookCount == lastBookCount && Mathf.Approximately(areaMultiplier, lastAreaMultiplier))
                return;

            bookVisual.RebuildBooks(bookCount, SkillPool.MagicBookBaseScale * areaMultiplier);
            lastBookCount = bookCount;
            lastAreaMultiplier = areaMultiplier;
        }

        void Update()
        {
            if (UpgradeStats == null || !UpgradeStats.HasMagicBook || bookVisual == null)
                return;

            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            float areaMultiplier = Mathf.Max(1f, UpgradeStats.AttackAreaMultiplier);
            float spinSpeed = SkillPool.MagicBookBaseSpinSpeed
                * UpgradeStats.AttackSpeedMultiplier
                * MetaBonuses.AttackSpeedMultiplier;
            float radius = SkillPool.MagicBookOrbitRadius * areaMultiplier;

            bookVisual.UpdateOrbit(radius, SkillPool.MagicBookOrbitHeight, spinSpeed);

            contactTickTimer -= Time.deltaTime;
            bool applyContactDamage = contactTickTimer <= 0f;
            if (applyContactDamage)
                contactTickTimer = ContactTickInterval;

            DetectHits(areaMultiplier, applyContactDamage);
        }

        void DetectHits(float areaMultiplier, bool applyContactDamage)
        {
            float hitRadius = SkillPool.MagicBookBaseHitRadius * areaMultiplier;
            float damage = UpgradeStats.MagicBookContactDamage;
            var books = bookVisual.Books;

            for (int i = 0; i < books.Count; i++)
            {
                Transform book = books[i];
                if (book == null)
                    continue;

                Vector3 center = book.position;
                Vector3 halfExtents = new Vector3(hitRadius, HitHeight * 0.5f, hitRadius);

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

                    if (BossProjectile.TryBreak(hit))
                        continue;

                    if (applyContactDamage && damage > 0f)
                        CombatTarget.TryApplyDamage(hit, damage);
                }
            }
        }
    }
}
