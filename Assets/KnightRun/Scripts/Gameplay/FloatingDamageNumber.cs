using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class FloatingDamageNumber : MonoBehaviour
    {
        const float Lifetime = 0.75f;
        const float RiseSpeed = 1.6f;
        const float CharacterSize = 0.07f;

        TextMesh textMesh;
        float timer;
        Color startColor;

        public static void Spawn(Vector3 worldPosition, float damage)
        {
            var go = new GameObject("DamagePopup");
            go.transform.position = worldPosition;
            var popup = go.AddComponent<FloatingDamageNumber>();
            popup.Initialize(damage);
        }

        void Initialize(float damage)
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.text = FormatDamage(damage);
            textMesh.fontSize = 64;
            textMesh.characterSize = CharacterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0.25f, 0.05f, 0.04f);
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            startColor = textMesh.color;
            timer = 0f;
            FaceCamera();
        }

        static string FormatDamage(float damage)
        {
            if (damage >= 10f)
                return Mathf.RoundToInt(damage).ToString();

            if (Mathf.Approximately(damage, Mathf.Round(damage)))
                return Mathf.RoundToInt(damage).ToString();

            return damage.ToString("0.#");
        }

        void Update()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.State != GameState.Running)
                return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / Lifetime);

            transform.position += Vector3.up * RiseSpeed * Time.deltaTime;
            FaceCamera();

            if (textMesh != null)
            {
                float alpha = 1f - t;
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                textMesh.characterSize = CharacterSize * Mathf.Lerp(1f, 1.1f, t);
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
