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

        const float BodySize = 1f;
        const float MoveSpeed = 5f;
        const float DespawnBehindDistance = 10f;
        const float KnockdownDuration = 2.5f;

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

                if (transform.position.z < player.position.z - DespawnBehindDistance)
                    Destroy(gameObject);

                return;
            }

            Vector3 target = player.position;
            target.y = transform.position.y;

            Vector3 offset = target - transform.position;
            if (offset.sqrMagnitude > 0.01f)
                transform.position += offset.normalized * MoveSpeed * Time.deltaTime;

            if (transform.position.z < player.position.z - DespawnBehindDistance)
                Destroy(gameObject);
        }

        void ResolvePlayer()
        {
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }

        void OnTriggerEnter(Collider other)
        {
            if (isDead || isKnockedDown || !other.CompareTag("Player"))
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
            if (health != null)
                health.TakeDamage(KnightHealth.EnemyTouchDamage);

            isDead = true;
            Destroy(gameObject);
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
        }

        public void BreakBySword()
        {
            if (isDead)
                return;

            isDead = true;
            GameManager.Instance?.AddEnemyDefeated();
            Destroy(gameObject);
        }
    }
}
