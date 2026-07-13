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
        BoxCollider bodyCollider;
        GameManager gameManager;
        bool isDead;
        int maxHealth;
        float currentHealth;
        float shootTimer;
        float screenAheadDistance;
        float pendingPopupDamage;
        float popupBatchTimer;

        public int MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;

        public event Action<float, float> OnHealthChanged;
        public event Action<Boss> OnDefeated;

        static readonly Color BossTint = new Color(0.55f, 0.08f, 0.12f);
        const float PopupBatchInterval = 0.12f;
        const float ImmediatePopupThreshold = 5f;

        public void Build()
        {
            gameObject.name = "Boss";

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = Vector3.one * BodySize;
            mesh.transform.localPosition = new Vector3(0f, BodySize * 0.5f, 0f);

            var renderer = mesh.GetComponent<Renderer>();
            renderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
            Material material = renderer.material;
            material.color = BossTint;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", BossTint);

            Destroy(mesh.GetComponent<Collider>());
            meshTransform = mesh.transform;

            bodyCollider = gameObject.AddComponent<BoxCollider>();
            bodyCollider.isTrigger = true;
            bodyCollider.size = Vector3.one * BodySize;
            bodyCollider.center = new Vector3(0f, BodySize * 0.5f, 0f);
        }

        public void Initialize(int health, float aheadDistance)
        {
            maxHealth = Mathf.Max(1, health);
            currentHealth = maxHealth;
            screenAheadDistance = aheadDistance;
            gameObject.name = $"Boss_HP{maxHealth}";
            shootTimer = ShootInterval * 0.5f;
            NotifyHealthChanged();
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
            int damage = Mathf.Max(1, Mathf.RoundToInt(ProjectileDamage * (RunPhaseManager.Instance.CurrentPhaseIndex + 1)));
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
            if (isDead || damage <= 0f)
                return;

            int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            currentHealth -= roundedDamage;
            QueueDamagePopup(roundedDamage);
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

        void QueueDamagePopup(float damage)
        {
            if (damage >= ImmediatePopupThreshold)
            {
                FlushDamagePopup();
                SpawnDamagePopup(damage);
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

            SpawnDamagePopup(pendingPopupDamage);
            pendingPopupDamage = 0f;
            popupBatchTimer = 0f;
        }

        void SpawnDamagePopup(float damage)
        {
            Vector3 position = transform.position + Vector3.up * HeadPopupHeight;
            position.x += UnityEngine.Random.Range(-0.2f, 0.2f);
            FloatingDamageNumber.Spawn(position, damage);
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
            Destroy(gameObject);
        }
    }
}
