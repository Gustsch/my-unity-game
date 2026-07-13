using UnityEngine;

namespace KnightRun.Core
{
    [System.Serializable]
    public struct RunPhaseSettings
    {
        public RunPhase phase;
        public string displayName;
        public Color groundColor;
        public Color wallColor;
        public Color accentColor;
        public Color ambientColor;
        public float speedMultiplier;
        public float obstacleChance;
        public float enemySpawnMultiplier;
        public bool useLaneMovement;
        public bool useSlideMovement;
        public float trackHalfWidth;
        public float playableHalfWidth;
        public int laneCount;
        public int enemyHealthMin;
        public int enemyHealthMax;
    }

    public static class RunPhaseDefaults
    {
        public static readonly RunPhaseSettings[] All =
        {
            new RunPhaseSettings
            {
                phase = RunPhase.Forest,
                displayName = "Floresta Encantada",
                groundColor = new Color(0.22f, 0.45f, 0.18f),
                wallColor = new Color(0.12f, 0.32f, 0.10f),
                accentColor = new Color(0.35f, 0.55f, 0.20f),
                ambientColor = new Color(0.55f, 0.65f, 0.50f),
                speedMultiplier = 1f,
                obstacleChance = 0.35f,
                enemySpawnMultiplier = 0.6f,
                useLaneMovement = false,
                useSlideMovement = false,
                trackHalfWidth = 8f,
                enemyHealthMin = 10,
                enemyHealthMax = 10
            },
            new RunPhaseSettings
            {
                phase = RunPhase.Cave,
                displayName = "Caverna Sombria",
                groundColor = new Color(0.28f, 0.24f, 0.20f),
                wallColor = new Color(0.15f, 0.13f, 0.12f),
                accentColor = new Color(0.40f, 0.35f, 0.28f),
                ambientColor = new Color(0.25f, 0.22f, 0.20f),
                speedMultiplier = 1.15f,
                obstacleChance = 0.45f,
                enemySpawnMultiplier = 0.8f,
                useLaneMovement = false,
                useSlideMovement = false,
                trackHalfWidth = 4f,
                enemyHealthMin = 20,
                enemyHealthMax = 40
            },
            new RunPhaseSettings
            {
                phase = RunPhase.MineCart,
                displayName = "Trilho da Mina",
                groundColor = new Color(0.35f, 0.22f, 0.12f),
                wallColor = new Color(0.10f, 0.08f, 0.07f),
                accentColor = new Color(0.55f, 0.38f, 0.18f),
                ambientColor = new Color(0.18f, 0.14f, 0.10f),
                speedMultiplier = 1.3f,
                obstacleChance = 0.55f,
                enemySpawnMultiplier = 0.8f,
                useLaneMovement = true,
                useSlideMovement = false,
                trackHalfWidth = 7f,
                playableHalfWidth = 5.2f,
                laneCount = 5,
                enemyHealthMin = 40,
                enemyHealthMax = 70
            },
            new RunPhaseSettings
            {
                phase = RunPhase.Volcano,
                displayName = "Vulcao Ardente",
                groundColor = new Color(0.28f, 0.10f, 0.06f),
                wallColor = new Color(0.18f, 0.08f, 0.06f),
                accentColor = new Color(0.95f, 0.35f, 0.08f),
                ambientColor = new Color(0.45f, 0.18f, 0.10f),
                speedMultiplier = 1.45f,
                obstacleChance = 0.6f,
                enemySpawnMultiplier = 0.8f,
                useLaneMovement = false,
                useSlideMovement = false,
                trackHalfWidth = 6f,
                enemyHealthMin = 70,
                enemyHealthMax = 120
            },
            new RunPhaseSettings
            {
                phase = RunPhase.IceCavern,
                displayName = "Cavernas de Gelo",
                groundColor = new Color(0.10f, 0.28f, 0.35f),
                wallColor = new Color(0.08f, 0.18f, 0.22f),
                accentColor = new Color(0.35f, 0.95f, 0.90f),
                ambientColor = new Color(0.10f, 0.28f, 0.35f),
                speedMultiplier = 1.6f,
                obstacleChance = 0.7f,
                enemySpawnMultiplier = 0.8f,
                useLaneMovement = false,
                useSlideMovement = true,
                trackHalfWidth = 6f,
                enemyHealthMin = 120,
                enemyHealthMax = 200
            },
            new RunPhaseSettings
            {
                phase = RunPhase.Desert,
                displayName = "Deserto Ardente",
                groundColor = new Color(0.28f, 0.10f, 0.06f),
                wallColor = new Color(0.18f, 0.08f, 0.06f),
                accentColor = new Color(0.95f, 0.35f, 0.08f),
                ambientColor = new Color(0.45f, 0.18f, 0.10f),
                speedMultiplier = 1.75f,
                obstacleChance = 0.8f,
                enemySpawnMultiplier = 1f,
                useLaneMovement = false,
                useSlideMovement = false,
                trackHalfWidth = 20f,
                enemyHealthMin = 200,
                enemyHealthMax = 350
            }
        };
    }
}
