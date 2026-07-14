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
        float stunUntil;

        public int MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsElite => isElite;

        public bool IsStunned => Time.time < stunUntil;

        static readonly Color EliteTint = new Color(0.28f, 0.28f, 0.32f);
        static readonly Color FrozenTint = new Color(0.55f, 0.82f, 1f);
        const string GoblinVisualResourcesPath = "KnightRun/Enemies";
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
        const float EliteSizeMultiplier = 2f;
        const float KnockbackCloseDistance = 3f;
        const float KnockbackFarDistance = 18f;

        static readonly Vector3 MeshStandPosition = new Vector3(0f, BodySize * 0.5f, 0f);
        static readonly Vector3 MeshStandScale = Vector3.one * BodySize;
        static readonly Quaternion MeshStandRotation = Quaternion.identity;
        static readonly Vector3 MeshKnockedPosition = new Vector3(0f, 0.18f, 0f);
        static readonly Vector3 MeshKnockedScale = new Vector3(BodySize, BodySize * 0.35f, BodySize);
        static readonly Quaternion MeshKnockedRotation = Quaternion.Euler(0f, 0f, 90f);
        static GameObject[] goblinVisualPrefabs;
        static bool goblinVisualLoadAttempted;

        public void Build()
        {
            gameObject.name = "Enemy";

            meshTransform = new GameObject("EnemyVisual").transform;
            meshTransform.SetParent(transform, false);
            meshTransform.localScale = MeshStandScale;
            meshTransform.localPosition = MeshStandPosition;

            BuildVisual(meshTransform);

            bodyCollider = gameObject.AddComponent<BoxCollider>();
            bodyCollider.isTrigger = true;
            bodyCollider.size = Vector3.one * BodySize;
            bodyCollider.center = new Vector3(0f, BodySize * 0.5f, 0f);
        }

        void BuildVisual(Transform root)
        {
            GameObject prefab = GetRandomGoblinVisual();
            if (prefab != null)
            {
                GameObject goblin = Instantiate(prefab, root, false);
                goblin.name = "Goblin";
                goblin.transform.localPosition = new Vector3(0f, -BodySize * 0.5f, 0f);
                goblin.transform.localRotation = Quaternion.identity;
                goblin.transform.localScale = Vector3.one;

                foreach (Collider visualCollider in goblin.GetComponentsInChildren<Collider>(true))
                {
                    visualCollider.enabled = false;
                    Destroy(visualCollider);
                }

                return;
            }

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "Body";
            mesh.transform.SetParent(root, false);
            mesh.transform.localScale = Vector3.one;
            mesh.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
            Destroy(mesh.GetComponent<Collider>());
        }

        static GameObject GetRandomGoblinVisual()
        {
            if (!goblinVisualLoadAttempted)
            {
                goblinVisualLoadAttempted = true;
                goblinVisualPrefabs = Resources.LoadAll<GameObject>(GoblinVisualResourcesPath);

                if (goblinVisualPrefabs.Length == 0)
                    Debug.LogWarning($"No goblin visuals found at Resources/{GoblinVisualResourcesPath}. Using enemy fallback.");
            }

            if (goblinVisualPrefabs == null || goblinVisualPrefabs.Length == 0)
                return null;

            return goblinVisualPrefabs[Random.Range(0, goblinVisualPrefabs.Length)];
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
            ApplyStandingDimensions();
            ApplyTint(EliteTint);
        }

        void ApplyStandingDimensions()
        {
            float sizeMultiplier = isElite ? EliteSizeMultiplier : 1f;

            if (meshTransform != null)
            {
                meshTransform.localPosition = MeshStandPosition * sizeMultiplier;
                meshTransform.localScale = MeshStandScale * sizeMultiplier;
                meshTransform.localRotation = MeshStandRotation;
            }

            if (bodyCollider != null)
            {
                bodyCollider.size = Vector3.one * BodySize * sizeMultiplier;
                bodyCollider.center = new Vector3(0f, BodySize * 0.5f * sizeMultiplier, 0f);
            }
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

            if (IsStunned)
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
            {
                Vector3 lookDirection = new Vector3(offset.x, 0f, offset.z);
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(lookDirection.normalized, Vector3.up),
                        10f * Time.deltaTime);
                }

                Vector3 delta = offset.normalized * MoveSpeed * Time.deltaTime;
                delta.x *= RunForwardMotion.GetEnemyHorizontalMoveScale();
                transform.position += delta;
            }

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
            if (isDead || isKnockedDown || EnemyFreezeController.IsActive || IsStunned || !other.CompareTag("Player"))
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
                float sizeMultiplier = isElite ? EliteSizeMultiplier : 1f;
                meshTransform.localPosition = MeshKnockedPosition * sizeMultiplier;
                meshTransform.localScale = MeshKnockedScale * sizeMultiplier;
                meshTransform.localRotation = MeshKnockedRotation;
            }

            if (bodyCollider != null)
            {
                float sizeMultiplier = isElite ? EliteSizeMultiplier : 1f;
                bodyCollider.size = new Vector3(BodySize, BodySize * 0.35f, BodySize) * sizeMultiplier;
                bodyCollider.center = new Vector3(0f, 0.18f * sizeMultiplier, 0f);
            }
        }

        void RecoverFromKnockdown()
        {
            isKnockedDown = false;
            knockdownTimer = 0f;

            ApplyStandingDimensions();

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
            if (meshTransform == null)
                return;

            Renderer[] renderers = meshTransform.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
                ApplyRendererTint(renderer, color);
        }

        static void ApplyRendererTint(Renderer renderer, Color color)
        {
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

            int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            currentHealth -= roundedDamage;
            QueueDamagePopup(roundedDamage);
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

        public void ApplyStun(float duration)
        {
            if (isDead || duration <= 0f)
                return;

            stunUntil = Mathf.Max(stunUntil, Time.time + duration);
        }

        public void ForceKill()
        {
            Die(spawnSpecialLoot: false);
        }

        void Die(bool spawnSpecialLoot = true)
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
            if (spawnSpecialLoot)
                EnemyLootDrop.TrySpawnSpecialDrop(dropPosition);
            Destroy(gameObject);
        }
    }
}
