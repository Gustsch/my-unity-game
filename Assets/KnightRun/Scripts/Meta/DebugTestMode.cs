using UnityEngine;

namespace KnightRun.Meta
{
    public static class DebugTestMode
    {
        public const string Tag = "KnightRunTester";
        const string PrefsKey = "KnightRun_DebugTestMode";
        const string ManagedAnchorName = "DebugTestAnchor";

        static bool enabled;

        public static bool IsActive => enabled;

        public static void Load()
        {
            enabled = PlayerPrefs.GetInt(PrefsKey, 0) > 0;

            // Scene tag only force-enables once; menu toggle remains the authority afterwards.
            if (!enabled && HasSceneTag())
            {
                enabled = true;
                PlayerPrefs.SetInt(PrefsKey, 1);
                PlayerPrefs.Save();
            }
        }

        public static void SetEnabled(bool value)
        {
            enabled = value;
            PlayerPrefs.SetInt(PrefsKey, value ? 1 : 0);
            PlayerPrefs.Save();

            if (!value)
                DestroyManagedAnchors();
        }

        public static void Toggle()
        {
            SetEnabled(!enabled);
        }

        public static void AttachAnchorIfNeeded(Transform playerRoot)
        {
            if (playerRoot == null)
                return;

            Transform existing = playerRoot.Find(ManagedAnchorName);
            if (!enabled)
            {
                if (existing != null)
                    Object.Destroy(existing.gameObject);
                return;
            }

            if (existing == null)
            {
                var anchor = new GameObject(ManagedAnchorName);
                anchor.transform.SetParent(playerRoot, false);
            }
        }

        static bool HasSceneTag()
        {
            try
            {
                return GameObject.FindGameObjectWithTag(Tag) != null;
            }
            catch
            {
                return false;
            }
        }

        static void DestroyManagedAnchors()
        {
            var players = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                Transform t = players[i];
                if (t != null && t.name == ManagedAnchorName)
                    Object.Destroy(t.gameObject);
            }
        }
    }
}
