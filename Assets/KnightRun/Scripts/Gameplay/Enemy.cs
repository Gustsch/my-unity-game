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
        Animator goblinAnimator;
        Animator skeletonAnimator;
        RuntimeAnimatorController skeletonWalkController;
        GameManager gameManager;
        bool isDead;
        bool isKnockedDown;
        bool usesSkeletonVisual;
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
        const string GoblinRunAnimatorPath = "KnightRun/Enemies/goblin_run";
        const string GoblinRunStateName = "Run";
        const string SkeletonVisualResourcePath = "KnightRun/Skeletons/skeleton";
        const string SkeletonWalkAnimatorPath = "KnightRun/Skeletons/walk";
        const string SkeletonDeathAnimatorPath = "KnightRun/Skeletons/death";
        const string SkeletonIdleAnimatorPath = "KnightRun/Skeletons/skeleton_idle";
        const string SkeletonAnimationStateName = "anim";
        const float SkeletonDeathDuration = 1.45f;
        const float DeathShrinkDuration = 0.5f;
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
        const float SkeletonVisualScale = 1f;
        const float SkeletonEliteVisualMultiplier = 1.5f;
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
        static RuntimeAnimatorController goblinRunController;
        static GameObject skeletonVisualPrefab;
        static bool skeletonVisualLoadAttempted;
        static RuntimeAnimatorController skeletonWalkControllerAsset;
        static RuntimeAnimatorController skeletonDeathController;
        static RuntimeAnimatorController skeletonIdleController;
        static bool skeletonAnimatorLoadAttempted;
        static Shader urpLitShader;
        static MaterialPropertyBlock tintPropertyBlock;

        public void Build(int difficultyPhaseNumber = 1)
        {
            gameObject.name = "Enemy";

            meshTransform = new GameObject("EnemyVisual").transform;
            meshTransform.SetParent(transform, false);
            meshTransform.localScale = MeshStandScale;
            meshTransform.localPosition = MeshStandPosition;

            BuildVisual(meshTransform, difficultyPhaseNumber >= 2);

            bodyCollider = gameObject.AddComponent<BoxCollider>();
            bodyCollider.isTrigger = true;
            bodyCollider.size = Vector3.one * BodySize;
            bodyCollider.center = new Vector3(0f, BodySize * 0.5f, 0f);
        }

        void BuildVisual(Transform root, bool useSkeleton)
        {
            GameObject prefab = useSkeleton ? GetSkeletonVisual() : GetRandomGoblinVisual();
            bool isSkeletonVisual = useSkeleton && prefab != null;
            if (prefab == null && useSkeleton)
                prefab = GetRandomGoblinVisual();

            if (prefab != null)
            {
                usesSkeletonVisual = isSkeletonVisual;
                GameObject visual = Instantiate(prefab, root, false);
                visual.name = isSkeletonVisual ? "Skeleton" : "Goblin";
                visual.transform.localPosition = new Vector3(0f, -BodySize * 0.5f, 0f);
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = isSkeletonVisual
                    ? Vector3.one * SkeletonVisualScale
                    : Vector3.one;

                if (isSkeletonVisual)
                {
                    EnsureUrpMaterials(visual);
                    SetupSkeletonAnimator(visual);
                }
                else
                {
                    EnsureUrpMaterials(visual);
                    SetupGoblinAnimator(visual);
                }

                foreach (Collider visualCollider in visual.GetComponentsInChildren<Collider>(true))
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

        void SetupGoblinAnimator(GameObject visual)
        {
            goblinAnimator = visual.GetComponentInChildren<Animator>();
            if (goblinAnimator == null)
                return;

            if (goblinRunController == null)
                goblinRunController = Resources.Load<RuntimeAnimatorController>(GoblinRunAnimatorPath);

            goblinAnimator.applyRootMotion = false;
            if (goblinRunController != null)
                goblinAnimator.runtimeAnimatorController = goblinRunController;

            goblinAnimator.Play(GoblinRunStateName, 0, Random.Range(0f, 0.8f));
        }

        void UpdateGoblinAnimation(bool isMoving)
        {
            if (goblinAnimator == null || usesSkeletonVisual || isDead)
                return;

            bool paused = !isMoving || isKnockedDown || EnemyFreezeController.IsActive || IsStunned;
            goblinAnimator.speed = paused ? 0f : 1f;
            if (paused)
                return;

            AnimatorStateInfo state = goblinAnimator.GetCurrentAnimatorStateInfo(0);
            if (!state.IsName(GoblinRunStateName) || state.normalizedTime >= 0.95f)
                goblinAnimator.Play(GoblinRunStateName, 0, 0f);
        }

        static GameObject GetSkeletonVisual()
        {
            if (!skeletonVisualLoadAttempted)
            {
                skeletonVisualLoadAttempted = true;
                skeletonVisualPrefab = Resources.Load<GameObject>(SkeletonVisualResourcePath);

                if (skeletonVisualPrefab == null)
                    Debug.LogWarning($"Skeleton visual not found at Resources/{SkeletonVisualResourcePath}. Using goblin fallback.");
            }

            return skeletonVisualPrefab;
        }

        static void LoadSkeletonAnimators()
        {
            if (skeletonAnimatorLoadAttempted)
                return;

            skeletonAnimatorLoadAttempted = true;
            skeletonWalkControllerAsset = Resources.Load<RuntimeAnimatorController>(SkeletonWalkAnimatorPath);
            skeletonDeathController = Resources.Load<RuntimeAnimatorController>(SkeletonDeathAnimatorPath);
            skeletonIdleController = Resources.Load<RuntimeAnimatorController>(SkeletonIdleAnimatorPath);

            if (skeletonWalkControllerAsset == null)
                Debug.LogWarning($"Skeleton walk animator not found at Resources/{SkeletonWalkAnimatorPath}.");
            if (skeletonDeathController == null)
                Debug.LogWarning($"Skeleton death animator not found at Resources/{SkeletonDeathAnimatorPath}.");
        }

        void SetupSkeletonAnimator(GameObject visual)
        {
            skeletonAnimator = visual.GetComponentInChildren<Animator>();
            if (skeletonAnimator == null)
                return;

            skeletonAnimator.applyRootMotion = false;
            LoadSkeletonAnimators();
            skeletonWalkController = skeletonWalkControllerAsset != null
                ? skeletonWalkControllerAsset
                : skeletonAnimator.runtimeAnimatorController;
            skeletonAnimator.runtimeAnimatorController = skeletonWalkController;
            skeletonAnimator.Play(SkeletonAnimationStateName, 0, 0f);
        }

        void UpdateSkeletonAnimation(bool isMoving)
        {
            if (skeletonAnimator == null || !usesSkeletonVisual || isDead)
                return;

            bool paused = isKnockedDown || EnemyFreezeController.IsActive || IsStunned;
            if (paused)
            {
                skeletonAnimator.speed = 0f;
                return;
            }

            skeletonAnimator.speed = 1f;

            RuntimeAnimatorController desiredController = isMoving
                ? skeletonWalkController
                : skeletonIdleController ?? skeletonWalkController;

            if (desiredController == null)
                return;

            if (skeletonAnimator.runtimeAnimatorController != desiredController)
            {
                skeletonAnimator.runtimeAnimatorController = desiredController;
                skeletonAnimator.Play(SkeletonAnimationStateName, 0, 0f);
            }
        }

        void PlaySkeletonDeathAnimation()
        {
            if (skeletonAnimator == null || skeletonDeathController == null)
                return;

            skeletonAnimator.speed = 1f;
            skeletonAnimator.runtimeAnimatorController = skeletonDeathController;
            skeletonAnimator.Play(SkeletonAnimationStateName, 0, 0f);
        }

        static void EnsureUrpMaterials(GameObject visual)
        {
            if (urpLitShader == null)
                urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

            if (urpLitShader == null)
                return;

            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null || material.shader == urpLitShader)
                        continue;

                    Texture mainTexture = material.HasProperty("_MainTex")
                        ? material.GetTexture("_MainTex")
                        : material.mainTexture;
                    Color baseColor = material.HasProperty("_Color") ? material.color : Color.white;

                    material.shader = urpLitShader;
                    material.SetColor("_BaseColor", baseColor);
                    if (material.HasProperty("_Color"))
                        material.SetColor("_Color", baseColor);

                    if (mainTexture != null)
                    {
                        if (material.HasProperty("_BaseMap"))
                            material.SetTexture("_BaseMap", mainTexture);
                        if (material.HasProperty("_MainTex"))
                            material.SetTexture("_MainTex", mainTexture);
                    }

                    changed = true;
                }

                if (changed)
                    renderer.sharedMaterials = materials;
            }
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
            float visualSizeMultiplier = isElite
                ? (usesSkeletonVisual ? SkeletonEliteVisualMultiplier : EliteSizeMultiplier)
                : 1f;
            float colliderSizeMultiplier = isElite ? EliteSizeMultiplier : 1f;

            if (meshTransform != null)
            {
                meshTransform.localPosition = MeshStandPosition * visualSizeMultiplier;
                meshTransform.localScale = MeshStandScale * visualSizeMultiplier;
                meshTransform.localRotation = MeshStandRotation;
            }

            if (bodyCollider != null)
            {
                bodyCollider.size = Vector3.one * BodySize * colliderSizeMultiplier;
                bodyCollider.center = new Vector3(0f, BodySize * 0.5f * colliderSizeMultiplier, 0f);
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
                UpdateGoblinAnimation(false);
                UpdateSkeletonAnimation(false);

                if (transform.position.z < player.position.z - DespawnBehindDistance)
                    Destroy(gameObject);

                return;
            }

            if (EnemyFreezeController.IsActive)
            {
                UpdateDamagePopupBatch();
                UpdateFreezeVisual();
                UpdateGoblinAnimation(false);
                UpdateSkeletonAnimation(false);

                if (transform.position.z < player.position.z - DespawnBehindDistance)
                    Destroy(gameObject);

                return;
            }

            if (IsStunned)
            {
                UpdateDamagePopupBatch();
                UpdateFreezeVisual();
                UpdateGoblinAnimation(false);
                UpdateSkeletonAnimation(false);

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
            bool isMoving = offset.sqrMagnitude > 0.01f;
            if (isMoving)
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

            UpdateGoblinAnimation(isMoving);
            UpdateSkeletonAnimation(isMoving);

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

            if (meshTransform != null && !usesSkeletonVisual)
            {
                float visualSizeMultiplier = isElite
                    ? (usesSkeletonVisual ? SkeletonEliteVisualMultiplier : EliteSizeMultiplier)
                    : 1f;
                meshTransform.localPosition = MeshKnockedPosition * visualSizeMultiplier;
                meshTransform.localScale = MeshKnockedScale * visualSizeMultiplier;
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

            if (color == Color.white)
            {
                renderer.SetPropertyBlock(null);
                return;
            }

            if (tintPropertyBlock == null)
                tintPropertyBlock = new MaterialPropertyBlock();

            tintPropertyBlock.Clear();
            tintPropertyBlock.SetColor("_Color", color);
            tintPropertyBlock.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(tintPropertyBlock);
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
            position.x += Random.Range(-0.12f, 0.12f);
            FloatingDamageNumber.Spawn(position, damage, isCritical);
        }

        public void TakeDamage(int damage)
        {
            TakeDamage((float)damage);
        }

        public void TakeDamage(int damage, bool isCritical)
        {
            TakeDamage((float)damage, isCritical);
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

            if (bodyCollider != null)
                bodyCollider.enabled = false;

            GameManager.Instance?.AddEnemyDefeated();
            Vector3 dropPosition = transform.position;
            ExperienceOrb.Spawn(
                dropPosition,
                isElite ? ExperienceOrb.EliteValue : ExperienceOrb.DefaultValue);
            if (spawnSpecialLoot)
                EnemyLootDrop.TrySpawnSpecialDrop(dropPosition);

            if (usesSkeletonVisual && skeletonAnimator != null && skeletonDeathController != null)
            {
                PlaySkeletonDeathAnimation();
                DeathShrinkEffect.Play(gameObject, SkeletonDeathDuration);
                return;
            }

            DeathShrinkEffect.Play(gameObject, DeathShrinkDuration);
        }
    }
}
