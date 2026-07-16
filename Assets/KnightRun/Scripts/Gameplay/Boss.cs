using System;
using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class Boss : MonoBehaviour
    {
        public const float BodySize = 4f;
        public const float ShootInterval = 0.5f;
        public const float ProjectileDamage = 50f;
        public const float HeadPopupHeight = BodySize + 0.8f;

        Transform player;
        Transform meshTransform;
        Renderer meshRenderer;
        BoxCollider bodyCollider;
        GameManager gameManager;
        RunPhase bossPhase;
        bool isDead;
        int maxHealth;
        float currentHealth;
        float shootTimer;
        float uniqueAttackTimer;
        float screenAheadDistance;
        float pendingPopupDamage;
        float popupBatchTimer;

        public int MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public RunPhase BossPhase => bossPhase;

        public event Action<float, float> OnHealthChanged;
        public event Action<Boss> OnDefeated;

        static readonly Color BossTint = new Color(0.55f, 0.08f, 0.12f);
        static readonly Color EntTint = new Color(0.28f, 0.42f, 0.16f);
        static readonly Color BatQueenTint = new Color(0.28f, 0.12f, 0.38f);
        static readonly Color IronGolemTint = new Color(0.45f, 0.42f, 0.4f);
        static readonly Color MagmaLordTint = new Color(0.85f, 0.28f, 0.08f);
        static readonly Color CrystalTyrantTint = new Color(0.45f, 0.85f, 0.95f);
        static readonly Color DesertScorpionTint = new Color(0.72f, 0.48f, 0.18f);
        const float PopupBatchInterval = 0.12f;
        const float ImmediatePopupThreshold = 5f;
        const float UniqueAttackFirstDelay = 3.5f;
        const float UniqueAttackInterval = 7f;
        const float DeathShrinkDuration = 0.8f;

        public void Build()
        {
            gameObject.name = "Boss";

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = Vector3.one * BodySize;
            mesh.transform.localPosition = new Vector3(0f, BodySize * 0.5f, 0f);

            meshRenderer = mesh.GetComponent<Renderer>();
            meshRenderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
            ApplyTint(BossTint);

            Destroy(mesh.GetComponent<Collider>());
            meshTransform = mesh.transform;

            bodyCollider = gameObject.AddComponent<BoxCollider>();
            bodyCollider.isTrigger = true;
            bodyCollider.size = Vector3.one * BodySize;
            bodyCollider.center = new Vector3(0f, BodySize * 0.5f, 0f);
        }

        public void Initialize(int health, float aheadDistance, RunPhase phase)
        {
            maxHealth = Mathf.Max(1, health);
            currentHealth = maxHealth;
            screenAheadDistance = aheadDistance;
            bossPhase = phase;
            gameObject.name = $"{GetBossName(phase)}_HP{maxHealth}";
            shootTimer = ShootInterval * 0.5f;
            uniqueAttackTimer = UniqueAttackFirstDelay;
            ApplyPhaseAppearance();
            NotifyHealthChanged();
        }

        static string GetBossName(RunPhase phase)
        {
            return phase switch
            {
                RunPhase.Forest => "EntBoss",
                RunPhase.Cave => "BatQueenBoss",
                RunPhase.MineCart => "IronGolemBoss",
                RunPhase.Volcano => "MagmaLordBoss",
                RunPhase.IceCavern => "CrystalTyrantBoss",
                RunPhase.Desert => "DesertScorpionBoss",
                _ => "Boss"
            };
        }

        void ApplyPhaseAppearance()
        {
            if (meshTransform == null || meshRenderer == null)
                return;

            Color tint = BossTint;
            KnightRunTexture texture = KnightRunTexture.Enemy;
            Vector3 scale = Vector3.one * BodySize;

            switch (bossPhase)
            {
                case RunPhase.Forest:
                    tint = EntTint;
                    texture = KnightRunTexture.TreeTrunk;
                    scale = new Vector3(BodySize * 1.05f, BodySize * 1.25f, BodySize * 1.05f);
                    break;
                case RunPhase.Cave:
                    tint = BatQueenTint;
                    texture = KnightRunTexture.Enemy;
                    scale = new Vector3(BodySize * 1.15f, BodySize * 0.85f, BodySize * 1.15f);
                    break;
                case RunPhase.MineCart:
                    tint = IronGolemTint;
                    texture = KnightRunTexture.MineCart;
                    scale = new Vector3(BodySize * 1.2f, BodySize * 1.1f, BodySize * 1.1f);
                    break;
                case RunPhase.Volcano:
                    tint = MagmaLordTint;
                    texture = KnightRunTexture.VolcanoRock;
                    scale = new Vector3(BodySize * 1.15f, BodySize * 1.15f, BodySize * 1.15f);
                    break;
                case RunPhase.IceCavern:
                    tint = CrystalTyrantTint;
                    texture = KnightRunTexture.Stalactite;
                    scale = new Vector3(BodySize * 1.05f, BodySize * 1.3f, BodySize * 1.05f);
                    break;
                case RunPhase.Desert:
                    tint = DesertScorpionTint;
                    texture = KnightRunTexture.GroundVolcano;
                    scale = new Vector3(BodySize * 1.35f, BodySize * 0.9f, BodySize * 1.2f);
                    break;
            }

            meshTransform.localScale = scale;
            meshTransform.localPosition = new Vector3(0f, scale.y * 0.5f, 0f);
            if (bodyCollider != null)
            {
                bodyCollider.size = scale;
                bodyCollider.center = meshTransform.localPosition;
            }

            meshRenderer.sharedMaterial = KnightRunMaterials.Get(texture);
            ApplyTint(tint);
        }

        void ApplyTint(Color color)
        {
            if (meshRenderer == null)
                return;

            Material material = meshRenderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            ResolvePlayer();
        }

        void Update()
        {
            if (isDead || gameManager == null || gameManager.State != GameState.Running)
                return;

            if (player == null)
            {
                ResolvePlayer();
                return;
            }

            UpdateDamagePopupBatch();
            SyncRunWithHero();
            ClampToTrack();

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                shootTimer = ShootInterval;
                FireProjectile();
            }

            UpdateUniqueAttack();
        }

        void UpdateUniqueAttack()
        {
            uniqueAttackTimer -= Time.deltaTime;
            if (uniqueAttackTimer > 0f)
                return;

            uniqueAttackTimer = UniqueAttackInterval;
            CastUniqueAttack();
        }

        void CastUniqueAttack()
        {
            if (player == null)
                return;

            switch (bossPhase)
            {
                case RunPhase.Forest:
                    BossRootSnare.Spawn(new Vector3(player.position.x, 0f, player.position.z));
                    break;
                case RunPhase.Cave:
                    BossBatSwarmAttack.Spawn(player);
                    break;
                case RunPhase.MineCart:
                {
                    var runner = player.GetComponent<RunnerController>();
                    if (runner != null)
                        BossMineDerailAttack.Spawn(runner, transform.position);
                    break;
                }
                case RunPhase.Volcano:
                    BossMagmaRingAttack.Spawn(new Vector3(player.position.x, 0f, player.position.z));
                    break;
                case RunPhase.IceCavern:
                    BossFreezeRayAttack.Spawn(transform, player);
                    break;
                case RunPhase.Desert:
                    // Phase storm already runs for the whole desert — keep it going.
                    BossSandstormAttack.EnsurePhaseStorm(player);
                    break;
            }
        }

        void SyncRunWithHero()
        {
            if (player == null)
                return;

            Vector3 position = transform.position;
            position.z = player.position.z + screenAheadDistance;
            transform.position = position;
            FacePlayer();
        }

        void FacePlayer()
        {
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }

        void ClampToTrack()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, RunnerController.TrackMinX, RunnerController.TrackMaxX);
            transform.position = position;
        }

        void FireProjectile()
        {
            if (player == null)
                return;

            BossAttackBand band = (BossAttackBand)UnityEngine.Random.Range(0, 3);
            Vector3 spawnPosition = transform.position + Vector3.up * (BodySize * 0.75f);
            RunPhaseManager phaseManager = RunPhaseManager.Instance;
            int difficultyPhase = phaseManager != null ? phaseManager.DifficultyPhaseNumber : 1;
            int damage = Mathf.Max(1, Mathf.RoundToInt(ProjectileDamage * difficultyPhase));
            BossProjectile.Spawn(spawnPosition, player.position, damage, band);
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }

        void OnTriggerEnter(Collider other)
        {
            TryInstantKillPlayer(other);
        }

        void TryInstantKillPlayer(Collider other)
        {
            if (isDead || !other.CompareTag("Player"))
                return;

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health != null)
                health.KillInstantly();
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, false);
        }

        public void TakeDamage(float damage, bool isCritical)
        {
            if (isDead || damage <= 0f)
                return;

            int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            currentHealth -= roundedDamage;
            QueueDamagePopup(roundedDamage, isCritical);
            NotifyHealthChanged();

            if (currentHealth <= 0f)
                Die();
        }

        void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            TakeDamage((float)damage);
        }

        public void TakeDamage(int damage, bool isCritical)
        {
            TakeDamage((float)damage, isCritical);
        }

        void QueueDamagePopup(float damage, bool isCritical)
        {
            if (isCritical || damage >= ImmediatePopupThreshold)
            {
                FlushDamagePopup();
                SpawnDamagePopup(damage, isCritical);
                return;
            }

            pendingPopupDamage += damage;
            if (popupBatchTimer <= 0f)
                popupBatchTimer = PopupBatchInterval;
        }

        void UpdateDamagePopupBatch()
        {
            if (pendingPopupDamage <= 0f || popupBatchTimer <= 0f)
                return;

            popupBatchTimer -= Time.deltaTime;
            if (popupBatchTimer <= 0f)
                FlushDamagePopup();
        }

        void FlushDamagePopup()
        {
            if (pendingPopupDamage <= 0f)
                return;

            SpawnDamagePopup(pendingPopupDamage, false);
            pendingPopupDamage = 0f;
            popupBatchTimer = 0f;
        }

        void SpawnDamagePopup(float damage, bool isCritical)
        {
            Vector3 position = transform.position + Vector3.up * HeadPopupHeight;
            position.x += UnityEngine.Random.Range(-0.2f, 0.2f);
            FloatingDamageNumber.Spawn(position, damage, isCritical);
        }

        void Die()
        {
            if (isDead)
                return;

            FlushDamagePopup();
            isDead = true;
            GameManager.Instance?.AddEnemyDefeated();
            OnDefeated?.Invoke(this);
            Vector3 dropPosition = transform.position;
            ExperienceOrb.Spawn(dropPosition, ExperienceOrb.GetBossLevelXpValue());
            EnemyLootDrop.SpawnBossCoinBag(dropPosition);
            DeathShrinkEffect.Play(gameObject, DeathShrinkDuration);
        }
    }
}
