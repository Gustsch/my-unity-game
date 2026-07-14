using System.Collections.Generic;
using UnityEngine;

namespace KnightRun
{
    public static class LocalizationTables
    {
        static readonly Dictionary<GameLanguage, Dictionary<string, string>> Cache =
            new Dictionary<GameLanguage, Dictionary<string, string>>();

        static readonly Dictionary<GameLanguage, string> ResourceNames =
            new Dictionary<GameLanguage, string>
            {
                { GameLanguage.Portuguese, "KnightRun/Localization/pt" },
                { GameLanguage.English, "KnightRun/Localization/en" },
                { GameLanguage.Spanish, "KnightRun/Localization/es" },
                { GameLanguage.French, "KnightRun/Localization/fr" },
                { GameLanguage.Japanese, "KnightRun/Localization/ja" },
                { GameLanguage.German, "KnightRun/Localization/de" }
            };

        public static Dictionary<string, string> Get(GameLanguage language)
        {
            if (Cache.TryGetValue(language, out Dictionary<string, string> cached))
                return cached;

            Dictionary<string, string> table = Load(language);
            Cache[language] = table;
            return table;
        }

        public static void ClearCache()
        {
            Cache.Clear();
        }

        static Dictionary<string, string> Load(GameLanguage language)
        {
            var table = new Dictionary<string, string>();
            if (!ResourceNames.TryGetValue(language, out string resourceName))
                return table;

            TextAsset asset = Resources.Load<TextAsset>(resourceName);
            if (asset == null)
            {
                Debug.LogWarning($"Localization table missing: {resourceName}");
                return table;
            }

            string[] lines = asset.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                int separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim().Replace("\\n", "\n");
                table[key] = value;
            }

            return table;
        }
    }
}
