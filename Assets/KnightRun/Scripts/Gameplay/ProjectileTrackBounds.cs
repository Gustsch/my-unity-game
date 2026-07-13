using KnightRun.Core;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public static class ProjectileTrackBounds
    {
        public static bool IsBeyondWalls(Vector3 position)
        {
            float halfWidth = PhaseTrackLayout.GetTrackHalfWidth();
            return position.x <= -halfWidth || position.x >= halfWidth;
        }

        public static bool IsBeyondWalls(Vector3 position, RunPhaseSettings settings)
        {
            float halfWidth = PhaseTrackLayout.GetTrackHalfWidth(settings);
            return position.x <= -halfWidth || position.x >= halfWidth;
        }
    }
}
