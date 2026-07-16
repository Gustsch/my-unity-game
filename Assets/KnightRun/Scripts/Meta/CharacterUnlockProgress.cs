using UnityEngine;

namespace KnightRun.Meta
{
    public static class CharacterUnlockProgress
    {
        const string HighestBossPhaseDefeatedKey = "KnightRun_HighestBossPhaseDefeated";

        static int highestBossPhaseDefeated = -1;

        public static int HighestBossPhaseDefeated => highestBossPhaseDefeated;

        public static void Load()
        {
            highestBossPhaseDefeated = PlayerPrefs.GetInt(HighestBossPhaseDefeatedKey, -1);
        }

        public static bool HasDefeatedBossPhase(int phaseIndex)
        {
            if (DebugTestMode.IsActive)
                return phaseIndex >= 0;

            return phaseIndex >= 0 && highestBossPhaseDefeated >= phaseIndex;
        }

        public static void MarkBossDefeated(int phaseIndex)
        {
            if (phaseIndex < 0 || highestBossPhaseDefeated >= phaseIndex)
                return;

            highestBossPhaseDefeated = phaseIndex;
            PlayerPrefs.SetInt(HighestBossPhaseDefeatedKey, highestBossPhaseDefeated);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            highestBossPhaseDefeated = -1;
            PlayerPrefs.DeleteKey(HighestBossPhaseDefeatedKey);
        }
    }
}
