using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BatEnemy : MonoBehaviour
    {
        enum Behavior
        {
            Align,
            Hover,
            Dive
        }

        const float FlyHeight = 3.2f;
        const float AlignSpeed = 15f;
        const float AlignTolerance = 0.2f;
        const float EngageDistance = 12f;
        const float HoverDuration = 0.25f;
        const float DiveSpeed = 32f;
        const float DiveArrivalDistance = 0.55f;
        const float BodySize = 0.7f;
        const float DespawnBehindDistance = 14f;
        const float WingFlapSpeed = 14f;
        const float HeadPopupHeight = 1.1f;
        const float PopupBatchInterval = 0.12f;
        const float ImmediatePopupThreshold = 5f;
        const float SwordReachHeight = 1.75f;

        Transform player;
        Transform meshRoot;
        Transform leftWing;
        Transform rightWing;
        GameManager gameManager;
        Behavior behavior = Behavior.Align;
        bool isDead;
        int maxHealth;
        float currentHealth;
        int contactDamage;
        float hoverTimer;
        float contactCooldownTimer;
        float pendingPopupDamage;
        float popupBatchTimer;
        float diveLockX;
        Vector3 diveTarget;

        public int MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsOpenToSwordAttack =>
            !isDead && behavior == Behavior.Dive && transform.position.y <= SwordReachHeight;

        public static BatEnemy Spawn(Transform parent, Vector3 worldPosition, int health, int damage)
        {
            var go = new GameObject("Bat");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(worldPosition.x, FlyHeight, worldPosition.z);

            var bat = go.AddComponent<BatEnemy>();
            bat.Build();
            bat.Initialize(health, damage);
            return bat;
        }

        void Build()
        {
            meshRoot = new GameObject("Mesh").transform;
            meshRoot.SetParent(transform, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(meshRoot, false);
            body.transform.localScale = new Vector3(0.45f, 0.35f, 0.55f);
            body.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
            ApplyTint(body.GetComponent<Renderer>(), new Color(0.18f, 0.12f, 0.22f));
            Object.Destroy(body.GetComponent<Collider>());

            leftWing = CreateWing("LeftWing", new Vector3(-0.45f, 0.05f, 0f));
            rightWing = CreateWing("RightWing", new Vector3(0.45f, 0.05f, 0f));

            var hitbox = gameObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            // Cobertura vertical alta para projeteis rasteiros (shuriken/bumerangue) acertarem.
            float hitboxHeight = FlyHeight + 1.2f;
            hitbox.size = new Vector3(BodySize * 1.6f, hitboxHeight, BodySize * 1.4f);
            hitbox.center = new Vector3(0f, -FlyHeight * 0.5f + 0.4f, 0f);
        }

        Transform CreateWing(string name, Vector3 localPosition)
        {
            var wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = name;
            wing.transform.SetParent(meshRoot, false);
            wing.transform.localScale = new Vector3(0.55f, 0.08f, 0.28f);
            wing.transform.localPosition = localPosition;
            wing.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.Enemy);
            ApplyTint(wing.GetComponent<Renderer>(), new Color(0.28f, 0.16f, 0.35f));
            Object.Destroy(wing.GetComponent<Collider>());
            return wing.transform;
        }

        static void ApplyTint(Renderer renderer, Color color)
        {
            if (renderer == null)
                return;

            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        public void Initialize(int health, int damage)
        {
            maxHealth = Mathf.Max(1, health);
            currentHealth = maxHealth;
            contactDamage = Mathf.Max(1, damage);
            gameObject.name = $"Bat_HP{maxHealth}";
            behavior = Behavior.Align;
            hoverTimer = 0f;
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

            if (contactCooldownTimer > 0f)
                contactCooldownTimer -= Time.deltaTime;

            UpdateDamagePopupBatch();
            AnimateWings();

            switch (behavior)
            {
                case Behavior.Align:
                    UpdateAlign();
                    break;
                case Behavior.Hover:
                    UpdateHover();
                    break;
                case Behavior.Dive:
                    UpdateDive();
                    break;
            }

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void UpdateAlign()
        {
            Vector3 position = transform.position;
            position.y = FlyHeight;
            position.x = Mathf.MoveTowards(position.x, player.position.x, AlignSpeed * Time.deltaTime);
            transform.position = position;

            float aheadDistance = position.z - player.position.z;
            bool aligned = Mathf.Abs(position.x - player.position.x) <= AlignTolerance;
            bool closeEnough = aheadDistance <= EngageDistance && aheadDistance >= -1f;

            if (aligned && closeEnough)
            {
                diveLockX = player.position.x;
                behavior = Behavior.Hover;
                hoverTimer = HoverDuration;
            }
        }

        void UpdateHover()
        {
            Vector3 position = transform.position;
            position.x = Mathf.MoveTowards(position.x, diveLockX, AlignSpeed * Time.deltaTime);
            position.y = FlyHeight;
            transform.position = position;

            hoverTimer -= Time.deltaTime;
            if (hoverTimer > 0f)
                return;

            diveTarget = player.position + Vector3.up * 0.6f;
            diveTarget.x = diveLockX;
            behavior = Behavior.Dive;
        }

        void UpdateDive()
        {
            diveTarget = player.position + Vector3.up * 0.55f;
            diveTarget.x = diveLockX;

            Vector3 next = Vector3.MoveTowards(transform.position, diveTarget, DiveSpeed * Time.deltaTime);
            transform.position = next;

            Vector3 look = diveTarget - transform.position;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);

            if ((transform.position - diveTarget).sqrMagnitude <= DiveArrivalDistance * DiveArrivalDistance)
            {
                // remonta para tentar de novo se ainda estiver vivo
                behavior = Behavior.Align;
                transform.rotation = Quaternion.identity;
                Vector3 reset = transform.position;
                reset.y = FlyHeight;
                transform.position = reset;
            }
        }

        void AnimateWings()
        {
            if (leftWing == null || rightWing == null)
                return;

            float flap = Mathf.Sin(Time.time * WingFlapSpeed) * 28f;
            leftWing.localRotation = Quaternion.Euler(0f, 0f, flap);
            rightWing.localRotation = Quaternion.Euler(0f, 0f, -flap);
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
            if (isDead || !other.CompareTag("Player") || contactCooldownTimer > 0f)
                return;

            if (behavior != Behavior.Dive)
                return;

            var runner = other.GetComponent<RunnerController>();
            if (runner == null)
                return;

            if (runner.IsSlideInvulnerable)
                return;

            KnightHealth health = other.GetComponent<KnightHealth>();
            if (health == null)
                return;

            health.TakeDamage(contactDamage);
            contactCooldownTimer = EnemyCombatStats.ContactDamageCooldown;
        }

        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f)
                return;

            int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            currentHealth -= roundedDamage;
            QueueDamagePopup(roundedDamage);

            if (currentHealth <= 0f)
                Die();
        }

        public void TakeDamage(int damage)
        {
            TakeDamage((float)damage);
        }

        public void ForceKill()
        {
            Die();
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

        void Die()
        {
            if (isDead)
                return;

            FlushDamagePopup();
            isDead = true;
            GameManager.Instance?.AddEnemyDefeated();
            ExperienceOrb.Spawn(transform.position, ExperienceOrb.DefaultValue);
            EnemyLootDrop.TrySpawnSpecialDrop(transform.position);
            Destroy(gameObject);
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }
    }
}
