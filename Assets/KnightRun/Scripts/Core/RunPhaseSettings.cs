using UnityEngine;

namespace KnightRun.Core
{
    [System.Serializable]
    public struct RunPhaseSettings
    {
        public RunPhase phase;
        public string displayName;
        public float distanceStart;
        public float distanceEnd;
        public Color groundColor;
        public Color wallColor;
        public Color accentColor;
        public Color ambientColor;
        public float speedMultiplier;
        public float obstacleChance;
        public bool useLaneMovement;
        public bool useSlideMovement;
    }

    public static class RunPhaseDefaults
    {
        public static readonly RunPhaseSettings[] All =
        {
            new RunPhaseSettings
            {
                phase = RunPhase.Forest,
                displayName = "Floresta Encantada",
                distanceStart = 0f,
                distanceEnd = 600f,
                groundColor = new Color(0.22f, 0.45f, 0.18f),
                wallColor = new Color(0.12f, 0.32f, 0.10f),
                accentColor = new Color(0.35f, 0.55f, 0.20f),
                ambientColor = new Color(0.55f, 0.65f, 0.50f),
                speedMultiplier = 1f,
                obstacleChance = 0.35f,
                useLaneMovement = false,
                useSlideMovement = false
            },
            new RunPhaseSettings
            {
                phase = RunPhase.Cave,
                displayName = "Caverna Sombria",
                distanceStart = 600f,
                distanceEnd = 1200f,
                groundColor = new Color(0.28f, 0.24f, 0.20f),
                wallColor = new Color(0.15f, 0.13f, 0.12f),
                accentColor = new Color(0.40f, 0.35f, 0.28f),
                ambientColor = new Color(0.25f, 0.22f, 0.20f),
                speedMultiplier = 1.15f,
                obstacleChance = 0.45f,
                useLaneMovement = false,
                useSlideMovement = false
            },
            new RunPhaseSettings
            {
                phase = RunPhase.MineCart,
                displayName = "Trilho da Mina",
                distanceStart = 1200f,
                distanceEnd = 1800f,
                groundColor = new Color(0.35f, 0.22f, 0.12f),
                wallColor = new Color(0.10f, 0.08f, 0.07f),
                accentColor = new Color(0.55f, 0.38f, 0.18f),
                ambientColor = new Color(0.18f, 0.14f, 0.10f),
                speedMultiplier = 1.3f,
                obstacleChance = 0.55f,
                useLaneMovement = true,
                useSlideMovement = false
            },
            new RunPhaseSettings
            {
                phase = RunPhase.Volcano,
                displayName = "Vulcao Ardente",
                distanceStart = 1800f,
                distanceEnd = 2400f,
                groundColor = new Color(0.28f, 0.10f, 0.06f),
                wallColor = new Color(0.18f, 0.08f, 0.06f),
                accentColor = new Color(0.95f, 0.35f, 0.08f),
                ambientColor = new Color(0.45f, 0.18f, 0.10f),
                speedMultiplier = 1.45f,
                obstacleChance = 0.6f,
                useLaneMovement = false,
                useSlideMovement = false
            },
            new RunPhaseSettings
            {
                phase = RunPhase.IceCavern,
                displayName = "Cavernas de Gelo",
                distanceStart = 2400f,
                distanceEnd = 3000f,
                groundColor = new Color(0.10f, 0.28f, 0.35f),
                wallColor = new Color(0.08f, 0.18f, 0.22f),
                accentColor = new Color(0.35f, 0.95f, 0.90f),
                ambientColor = new Color(0.10f, 0.28f, 0.35f),
                speedMultiplier = 1.6f,
                obstacleChance = 0.7f,
                useLaneMovement = false,
                useSlideMovement = true
            },
            new RunPhaseSettings
            {
                phase = RunPhase.Desert,
                displayName = "Deserto Ardente",
                distanceStart = 3000f,
                distanceEnd = 3600f,
                groundColor = new Color(0.28f, 0.10f, 0.06f),
                wallColor = new Color(0.18f, 0.08f, 0.06f),
                accentColor = new Color(0.95f, 0.35f, 0.08f),
                ambientColor = new Color(0.45f, 0.18f, 0.10f),
                speedMultiplier = 1.75f,
                obstacleChance = 0.8f,
                useLaneMovement = false,
                useSlideMovement = false
            }
        };
    }
}
