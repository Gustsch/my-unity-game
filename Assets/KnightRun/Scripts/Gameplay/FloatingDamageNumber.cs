using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class FloatingDamageNumber : MonoBehaviour
    {
        const float Lifetime = 0.75f;
        const float RiseSpeed = 1.6f;
        const float CritRiseSpeed = 2.1f;
        const float CharacterSize = 0.07f;
        const float CritCharacterSize = 0.115f;

        static readonly Color NormalColor = new Color(0.16f, 0.03f, 0.02f);
        static readonly Color CritColor = new Color(0.55f, 0.16f, 0.02f);

        TextMesh textMesh;
        float timer;
        Color startColor;
        bool isCritical;
        float baseCharacterSize;
        float riseSpeed;

        public static void Spawn(Vector3 worldPosition, float damage, bool isCritical = false)
        {
            var go = new GameObject("DamagePopup");
            go.transform.position = worldPosition;
            var popup = go.AddComponent<FloatingDamageNumber>();
            popup.Initialize(damage, isCritical);
        }

        void Initialize(float damage, bool critical)
        {
            isCritical = critical;
            baseCharacterSize = critical ? CritCharacterSize : CharacterSize;
            riseSpeed = critical ? CritRiseSpeed : RiseSpeed;

            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = FormatDamage(damage, critical);
            textMesh.fontSize = critical ? 96 : 64;
            textMesh.fontStyle = critical ? FontStyle.Bold : FontStyle.Normal;
            textMesh.characterSize = baseCharacterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = critical ? CritColor : NormalColor;
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            startColor = textMesh.color;
            timer = 0f;
            FaceCamera();
        }

        static string FormatDamage(float damage, bool critical)
        {
            int value = Mathf.Max(1, Mathf.RoundToInt(damage));
            return critical ? $"{value}!" : value.ToString();
        }

        void Update()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.State != GameState.Running)
                return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / Lifetime);

            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            FaceCamera();

            if (textMesh != null)
            {
                float alpha = 1f - t;
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                if (isCritical)
                {
                    float punch = 1f + Mathf.Sin(Mathf.Clamp01(timer / 0.18f) * Mathf.PI) * 0.35f;
                    float settle = Mathf.Lerp(1f, 1.15f, t);
                    textMesh.characterSize = baseCharacterSize * punch * settle;
                }
                else
                {
                    textMesh.characterSize = baseCharacterSize * Mathf.Lerp(1f, 1.1f, t);
                }
            }

            if (timer >= Lifetime)
                Destroy(gameObject);
        }

        void FaceCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            Vector3 direction = transform.position - cam.transform.position;
            if (direction.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
