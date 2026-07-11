using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class Enemy : MonoBehaviour
    {
        Transform player;
        Transform meshTransform;
        BoxCollider bodyCollider;
        GameManager gameManager;
        bool isDead;
        bool isKnockedDown;
        float knockdownTimer;
        int maxHealth;
        float currentHealth;
        int contactDamage;
        bool isElite;
        bool isFrozenTintApplied;
        float pendingPopupDamage;
        float popupBatchTimer;
        float contactCooldownTimer;
        float pendingKnockbackZ;

        public int MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsElite => isElite;

        static readonly Color EliteTint = new Color(0.28f, 0.28f, 0.32f);
        static readonly Color FrozenTint = new Color(0.55f, 0.82f, 1f);
        const float BodySize = 1f;
        const float MoveSpeed = 5f;
        const float DespawnBehindDistance = 10f;
        const float KnockdownDuration = 2.5f;
        const float HeadPopupHeight = 1.35f;
        const float PopupBatchInterval = 0.12f;
        const float ImmediatePopupThreshold = 5f;
        const float MinKnockbackDistance = 0.1f;
        const float MaxKnockbackDistance = 0.4f;
        const float EliteMaxKnockbackDistance = 0.22f;
        const float KnockbackCloseDistance = 3f;
        const float KnockbackFarDistance = 18f;

        static readonly Vector3 MeshStandPosition = new Vector3(0f, BodySize * 0.5f, 0f);
        static readonly Vector3 MeshStandScale = Vector3.one * BodySize;
        static readonly Quaternion MeshStandRotation = Quaternion.identity;
        static readonly Vector3 MeshKnockedPosition = new Vector3(0f, 0.18f, 0f);
        static readonly Vector3 MeshKnockedScale = new Vector3(BodySize, BodySize * 0.35f, BodySize);
        static readonly Quaternion MeshKnockedRotation = Quaternion.Euler(0f, 0f, 90f);

        public void Build()
        {
            gameObject.name = "Enemy";

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.transform.SetParent(transform, false);
            mesh.transform.localScale = MeshStandScale;
            mesh.transform.localPosition = MeshStandPosition;
            mesh.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
            Destroy(mesh.GetComponent<Collider>());
            meshTransform = mesh.transform;

            bodyCollider = gameObject.AddComponent<BoxCollider>();
            bodyCollider.isTrigger = true;
            bodyCollider.size = Vector3.one * BodySize;
            bodyCollider.center = new Vector3(0f, BodySize * 0.5f, 0f);
        }

        public void Initialize(int health, int damage = EnemyCombatStats.BaseContactDamage, bool elite = false)
        {
            maxHealth = Mathf.Max(1, health);
            currentHealth = maxHealth;
            contactDamage = Mathf.Max(1, damage);
            isElite = elite;
            gameObject.name = elite ? $"Enemy_Elite_HP{maxHealth}" : $"Enemy_HP{maxHealth}";

            if (elite && meshTransform != null)
                ApplyEliteVisual();
        }

        void ApplyEliteVisual()
        {
            var renderer = meshTransform.GetComponent<Renderer>();
            if (renderer == null)
                return;

            Material material = renderer.material;
            material.color = EliteTint;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", EliteTint);
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

            if (isKnockedDown)
            {
                knockdownTimer -= Time.deltaTime;
                if (knockdownTimer <= 0f)
                    RecoverFromKnockdown();

                UpdateDamagePopupBatch();
                UpdateFreezeVisual();

                if (transform.position.z < player.position.z - DespawnBehindDistance)
                    Destroy(gameObject);

                return;
            }

            if (EnemyFreezeController.IsActive)
            {
                UpdateDamagePopupBatch();
                UpdateFreezeVisual();

                if (transform.position.z < player.position.z - DespawnBehindDistance)
                    Destroy(gameObject);

                return;
            }

            UpdateFreezeVisual();

            if (contactCooldownTimer > 0f)
                contactCooldownTimer -= Time.deltaTime;

            Vector3 target = player.position;
            target.y = transform.position.y;

            Vector3 offset = target - transform.position;
            if (offset.sqrMagnitude > 0.01f)
                transform.position += offset.normalized * MoveSpeed * Time.deltaTime;

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void LateUpdate()
        {
            if (isDead || gameManager == null || gameManager.State != GameState.Running)
                return;

            if (Mathf.Approximately(pendingKnockbackZ, 0f))
                return;

            transform.position += new Vector3(0f, 0f, pendingKnockbackZ);
            pendingKnockbackZ = 0f;
            ClampToTrack();
        }

        void ClampToTrack()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, RunnerController.TrackMinX, RunnerController.TrackMaxX);
            transform.position = position;
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }

        void OnTriggerEnter(Collider other)
        {
            TryDealContactDamage(other);
        }

        void OnTriggerStay(Collider other)
        {
            TryDealContactDamage(other);
        }

        void TryDealContactDamage(Collider other)
        {
            if (isDead || isKnockedDown || EnemyFreezeController.IsActive || !other.CompareTag("Player"))
                return;

            if (contactCooldownTimer > 0f)
                return;

            var runner = other.GetComponent<RunnerController>();
            if (runner == null)
                return;

            if (runner.IsSlideInvulnerable)
            {
                KnockDown();
                return;
            }

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(contactDamage);
            contactCooldownTimer = EnemyCombatStats.ContactDamageCooldown;
        }

        void KnockDown()
        {
            if (isKnockedDown || isDead)
                return;

            isKnockedDown = true;
            knockdownTimer = KnockdownDuration;

            if (meshTransform != null)
            {
                meshTransform.localPosition = MeshKnockedPosition;
                meshTransform.localScale = MeshKnockedScale;
                meshTransform.localRotation = MeshKnockedRotation;
            }

            if (bodyCollider != null)
            {
                bodyCollider.size = new Vector3(BodySize, BodySize * 0.35f, BodySize);
                bodyCollider.center = new Vector3(0f, 0.18f, 0f);
            }
        }

        void RecoverFromKnockdown()
        {
            isKnockedDown = false;
            knockdownTimer = 0f;

            if (meshTransform != null)
            {
                meshTransform.localPosition = MeshStandPosition;
                meshTransform.localScale = MeshStandScale;
                meshTransform.localRotation = MeshStandRotation;
            }

            if (bodyCollider != null)
            {
                bodyCollider.size = Vector3.one * BodySize;
                bodyCollider.center = new Vector3(0f, BodySize * 0.5f, 0f);
            }

            isFrozenTintApplied = false;
            if (isElite)
                ApplyEliteVisual();
        }

        void UpdateFreezeVisual()
        {
            if (meshTransform == null)
                return;

            if (EnemyFreezeController.IsActive)
            {
                if (!isFrozenTintApplied)
                {
                    ApplyTint(FrozenTint);
                    isFrozenTintApplied = true;
                }

                return;
            }

            if (!isFrozenTintApplied)
                return;

            isFrozenTintApplied = false;
            if (isElite)
                ApplyEliteVisual();
            else
                ApplyTint(Color.white);
        }

        void ApplyTint(Color color)
        {
            var renderer = meshTransform.GetComponent<Renderer>();
            if (renderer == null)
                return;

            Material material = renderer.material;
            material.color = color;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f)
                return;

            currentHealth -= damage;
            QueueDamagePopup(damage);
            ApplyHitKnockback();

            if (currentHealth <= 0f)
                Die();
        }

        void ApplyHitKnockback()
        {
            if (player == null)
                ResolvePlayer();

            if (player == null)
                return;

            float maxKnockback = isElite ? EliteMaxKnockbackDistance : MaxKnockbackDistance;
            float distanceZ = Mathf.Abs(transform.position.z - player.position.z);
            float closeness = 1f - Mathf.InverseLerp(KnockbackFarDistance, KnockbackCloseDistance, distanceZ);
            float knockbackStrength = Mathf.Lerp(MinKnockbackDistance, maxKnockback, closeness);

            float pushZ = Mathf.Sign(transform.position.z - player.position.z);
            if (Mathf.Approximately(pushZ, 0f))
                pushZ = 1f;

            pendingKnockbackZ += pushZ * knockbackStrength;
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
            position.x += Random.Range(-0.12f, 0.12f);
            FloatingDamageNumber.Spawn(position, damage);
        }

        public void TakeDamage(int damage)
        {
            TakeDamage((float)damage);
        }

        void Die()
        {
            if (isDead)
                return;

            FlushDamagePopup();
            isDead = true;
            GameManager.Instance?.AddEnemyDefeated();
            Vector3 dropPosition = transform.position;
            ExperienceOrb.Spawn(
                dropPosition,
                isElite ? ExperienceOrb.EliteValue : ExperienceOrb.DefaultValue);
            EnemyLootDrop.TrySpawnSpecialDrop(dropPosition);
            Destroy(gameObject);
        }
    }
}
