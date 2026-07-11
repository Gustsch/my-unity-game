using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightRun.Meta
{
    [Serializable]
    public struct HighScoreEntry
    {
        public int score;
        public float distance;
        public int enemies;
    }

    public static class HighScoreTable
    {
        public const int MaxEntries = 10;
        const string CountKey = "KnightRun_HighScoreCount";

        static readonly List<HighScoreEntry> entries = new List<HighScoreEntry>();

        public static IReadOnlyList<HighScoreEntry> Entries => entries;

        public static void Load()
        {
            entries.Clear();
            int count = PlayerPrefs.GetInt(CountKey, 0);
            count = Mathf.Clamp(count, 0, MaxEntries);

            for (int i = 0; i < count; i++)
            {
                entries.Add(new HighScoreEntry
                {
                    score = PlayerPrefs.GetInt($"KnightRun_HighScore_{i}_Score", 0),
                    distance = PlayerPrefs.GetFloat($"KnightRun_HighScore_{i}_Distance", 0f),
                    enemies = PlayerPrefs.GetInt($"KnightRun_HighScore_{i}_Enemies", 0)
                });
            }
        }

        public static void TryAdd(int score, float distance, int enemies)
        {
            entries.Add(new HighScoreEntry
            {
                score = score,
                distance = distance,
                enemies = enemies
            });

            entries.Sort((a, b) => b.score.CompareTo(a.score));

            while (entries.Count > MaxEntries)
                entries.RemoveAt(entries.Count - 1);

            Save();
        }

        static void Save()
        {
            PlayerPrefs.SetInt(CountKey, entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                HighScoreEntry entry = entries[i];
                PlayerPrefs.SetInt($"KnightRun_HighScore_{i}_Score", entry.score);
                PlayerPrefs.SetFloat($"KnightRun_HighScore_{i}_Distance", entry.distance);
                PlayerPrefs.SetInt($"KnightRun_HighScore_{i}_Enemies", entry.enemies);
            }

            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            entries.Clear();
            PlayerPrefs.DeleteKey(CountKey);
            for (int i = 0; i < MaxEntries; i++)
            {
                PlayerPrefs.DeleteKey($"KnightRun_HighScore_{i}_Score");
                PlayerPrefs.DeleteKey($"KnightRun_HighScore_{i}_Distance");
                PlayerPrefs.DeleteKey($"KnightRun_HighScore_{i}_Enemies");
            }
        }
    }
}
