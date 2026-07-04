using KnightRun.Core;
using KnightRun.Gameplay;
using UnityEngine;

namespace KnightRun.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class KnightSwordAttack : MonoBehaviour
    {
        SwordVisual swordVisual;
        CharacterController body;
        GameManager gameManager;

        float attackTimer;
        float swingTimer;
        bool isSwinging;
        bool hitApplied;

        const float AttackInterval = 0.5f;
        const float SwingDuration = 0.32f;
        const float HitSizeMultiplier = 1.5f;
        const float ArcWidthMultiplier = 2.2f;

        static readonly Quaternion IdleRotation = Quaternion.Euler(-20f, 70f, 0f);
        static readonly Quaternion SlashRotation = Quaternion.Euler(-20f, -70f, 0f);

        void Awake()
        {
            swordVisual = GetComponent<SwordVisual>();
            body = GetComponent<CharacterController>();
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            attackTimer = AttackInterval * 0.5f;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (isSwinging)
            {
                UpdateSwing();
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                StartSwing();
        }

        void StartSwing()
        {
            isSwinging = true;
            swingTimer = 0f;
            hitApplied = false;
            attackTimer = AttackInterval;
        }

        void UpdateSwing()
        {
            swingTimer += Time.deltaTime;
            float t = swingTimer / SwingDuration;

            if (swordVisual != null && swordVisual.Pivot != null)
            {
                float arcT = Mathf.SmoothStep(0f, 1f, t);
                swordVisual.Pivot.localRotation = Quaternion.Slerp(IdleRotation, SlashRotation, arcT);
            }

            if (!hitApplied && t >= 0.45f)
            {
                hitApplied = true;
                DetectHits();
            }

            if (t >= 1f)
            {
                isSwinging = false;
                if (swordVisual != null && swordVisual.Pivot != null)
                    swordVisual.Pivot.localRotation = IdleRotation;
            }
        }

        void DetectHits()
        {
            float bodyWidth = body.radius * 2f;
            float bodyHeight = body.height;
            float bodyDepth = bodyWidth;

            float hitWidth = bodyWidth * HitSizeMultiplier * ArcWidthMultiplier;
            float hitHeight = bodyHeight * HitSizeMultiplier;
            float hitDepth = bodyDepth * HitSizeMultiplier;

            Vector3 center = transform.position + Vector3.forward * (bodyDepth * 0.5f + hitDepth * 0.5f);
            center.y = transform.position.y + bodyHeight * 0.5f;
            Vector3 halfExtents = new Vector3(hitWidth * 0.5f, hitHeight * 0.5f, hitDepth * 0.5f);

            Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                    continue;

                Enemy enemy = hit.GetComponent<Enemy>() ?? hit.GetComponentInParent<Enemy>();
                if (enemy != null)
                    enemy.BreakBySword();
            }
        }
    }
}
