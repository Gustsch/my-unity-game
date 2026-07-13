using UnityEngine;

namespace KnightRun.Core
{
    public static class PhaseTrackLayout
    {
        public const float DefaultTrackHalfWidth = 4f;
        public const float DefaultPlayableHalfWidth = 3.2f;
        public const float WallThickness = 0.5f;
        public const float PlayableInsetFromWall = DefaultTrackHalfWidth - DefaultPlayableHalfWidth;

        public static RunPhaseSettings GetSettings()
        {
            RunPhaseManager manager = RunPhaseManager.Instance;
            return manager != null ? manager.CurrentSettings : RunPhaseDefaults.All[0];
        }

        public static float GetTrackHalfWidth(RunPhaseSettings settings)
        {
            return settings.trackHalfWidth > 0f ? settings.trackHalfWidth : DefaultTrackHalfWidth;
        }

        public static float GetTrackHalfWidth()
        {
            return GetTrackHalfWidth(GetSettings());
        }

        public static float GetPlayableHalfWidth(RunPhaseSettings settings)
        {
            if (settings.playableHalfWidth > 0f)
                return settings.playableHalfWidth;

            return Mathf.Max(1f, GetTrackHalfWidth(settings) - PlayableInsetFromWall);
        }

        public static float GetPlayableHalfWidth()
        {
            return GetPlayableHalfWidth(GetSettings());
        }

        public static float GetPlayableMinX(RunPhaseSettings settings)
        {
            return -GetPlayableHalfWidth(settings);
        }

        public static float GetPlayableMaxX(RunPhaseSettings settings)
        {
            return GetPlayableHalfWidth(settings);
        }

        public static float GetPlayableMinX()
        {
            return GetPlayableMinX(GetSettings());
        }

        public static float GetPlayableMaxX()
        {
            return GetPlayableMaxX(GetSettings());
        }

        public static float GetWallCenterX(RunPhaseSettings settings, int sideSign)
        {
            return sideSign * (GetTrackHalfWidth(settings) + WallThickness * 0.5f);
        }

        public static float GetGroundWidth(RunPhaseSettings settings)
        {
            return GetTrackHalfWidth(settings) * 2f;
        }
    }
}
