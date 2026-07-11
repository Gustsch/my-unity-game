using KnightRun.Meta;
using KnightRun.Core;
using KnightRun.Gameplay;
using KnightRun.Progression;
using UnityEngine;

namespace KnightRun.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightSwordAttack : MonoBehaviour
    {
        SwordVisual swordVisual;
        CharacterController body;
        HeroUpgradeStats upgradeStats;
        GameManager gameManager;

        float attackTimer;
        float swingTimer;
        bool isSwinging;
        bool hitApplied;
        int remainingVolleySwings;

        const float BaseAttackInterval = 0.5f;
        const float BaseSwingDuration = 0.32f;
        const float HitSizeMultiplier = 1.5f;
        const float ArcWidthMultiplier = 2.2f;
        const float MinAttackInterval = 0.15f;
        const float MinSwingDuration = 0.1f;
        const float AttackHitHeight = 2f;
        const float AttackBodyWidth = 0.8f;

        public bool IsSwinging => isSwinging;

        static readonly Quaternion IdleRotation = Quaternion.Euler(-20f, 70f, 0f);
        static readonly Quaternion SlashRotation = Quaternion.Euler(-20f, -70f, 0f);

        float AttackSpeedMultiplier =>
            (UpgradeStats != null ? UpgradeStats.AttackSpeedMultiplier : 1f) * MetaBonuses.AttackSpeedMultiplier;

        float AttackInterval =>
            Mathf.Max(MinAttackInterval, BaseAttackInterval / AttackSpeedMultiplier);

        float SwingDuration =>
            Mathf.Max(MinSwingDuration, BaseSwingDuration / AttackSpeedMultiplier);

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
            swordVisual = GetComponent<SwordVisual>();
            body = GetComponent<CharacterController>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            attackTimer = AttackInterval * 0.5f;

            var stats = UpgradeStats;
            if (stats != null)
                stats.OnBonusesChanged += UpdateSwordState;

            UpdateSwordState();
        }

        void OnDestroy()
        {
            if (upgradeStats != null)
                upgradeStats.OnBonusesChanged -= UpdateSwordState;
        }

        void UpdateSwordState()
        {
            if (swordVisual == null || UpgradeStats == null)
                return;

            bool hasSword = UpgradeStats.HasSword;
            swordVisual.SetVisible(hasSword);

            if (hasSword)
                swordVisual.SetAttackAreaMultiplier(UpgradeStats.AttackAreaMultiplier);
            else if (isSwinging)
                CancelSwing();
        }

        void CancelSwing()
        {
            isSwinging = false;
            swingTimer = 0f;
            hitApplied = false;
            remainingVolleySwings = 0;

            if (swordVisual != null && swordVisual.Pivot != null)
                swordVisual.Pivot.localRotation = IdleRotation;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (UpgradeStats == null || !UpgradeStats.HasSword)
                return;

            if (isSwinging)
            {
                AdvanceSwing();
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                StartSwing();
        }

        void LateUpdate()
        {
            if (!isSwinging || swordVisual == null || swordVisual.Pivot == null)
                return;

            float t = swingTimer / SwingDuration;
            float arcT = Mathf.SmoothStep(0f, 1f, t);
            swordVisual.Pivot.localRotation = Quaternion.Slerp(IdleRotation, SlashRotation, arcT);
        }

        void StartSwing()
        {
            remainingVolleySwings = UpgradeStats != null ? UpgradeStats.AttackVolleyCount : 1;
            BeginVolleySwing();
        }

        void BeginVolleySwing()
        {
            isSwinging = true;
            swingTimer = 0f;
            hitApplied = false;
        }

        void AdvanceSwing()
        {
            swingTimer += Time.deltaTime;
            float t = swingTimer / SwingDuration;

            if (!hitApplied && t >= 0.45f)
            {
                hitApplied = true;
                DetectHits();
            }

            if (t >= 1f)
            {
                remainingVolleySwings--;
                if (remainingVolleySwings > 0)
                {
                    BeginVolleySwing();
                    return;
                }

                isSwinging = false;
                attackTimer = AttackInterval;
                if (swordVisual != null && swordVisual.Pivot != null)
                    swordVisual.Pivot.localRotation = IdleRotation;
            }
        }

        void DetectHits()
        {
            float bodyWidth = AttackBodyWidth;
            float bodyHeight = AttackHitHeight;
            float bodyDepth = bodyWidth;

            float areaMultiplier = UpgradeStats != null ? UpgradeStats.AttackAreaMultiplier : 1f;
            float hitWidth = bodyWidth * HitSizeMultiplier * ArcWidthMultiplier * areaMultiplier;
            float hitHeight = bodyHeight * HitSizeMultiplier * areaMultiplier;
            float hitDepth = bodyDepth * HitSizeMultiplier * areaMultiplier;

            Vector3 center = transform.position + Vector3.forward * (bodyDepth * 0.5f + hitDepth * 0.5f);
            center.y = transform.position.y + bodyHeight * 0.5f;
            Vector3 halfExtents = new Vector3(hitWidth * 0.5f, hitHeight * 0.5f, hitDepth * 0.5f);

            int damage = UpgradeStats != null
                ? UpgradeStats.SwordDamage
                : SkillPool.GetSwordDamage(SkillPool.StartingSwordLevel);

            SpawnSlashProjectile(damage, areaMultiplier);

            Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                    continue;

                if (BossProjectile.TryBreak(hit))
                    continue;

                CombatTarget.TryApplyDamage(hit, damage);
            }
        }

        void SpawnSlashProjectile(int damage, float areaMultiplier)
        {
            Vector3 spawnPosition = transform.position
                + Vector3.forward * (body.radius + 0.35f)
                + Vector3.up * (AttackHitHeight * 0.55f);

            SwordSlashProjectile.Spawn(spawnPosition, damage, areaMultiplier);
        }
    }
}
