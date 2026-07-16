using System.Collections.Generic;
using KnightRun.Core;
using UnityEngine;

namespace KnightRun.World
{
    public enum KnightRunTexture
    {
        KnightArmor,
        KnightHelmet,
        KnightSword,
        Enemy,
        GroundForest,
        GroundCave,
        GroundMine,
        GroundVolcano,
        WallForest,
        WallCave,
        WallVolcano,
        TreeTrunk,
        TreeLeaves,
        RockObstacle,
        LogObstacle,
        Coin,
        Stalactite,
        MineCart,
        MineRail,
        LavaPool,
        VolcanoRock
    }

    public static class KnightRunMaterials
    {
        static readonly Dictionary<KnightRunTexture, Material> Cache = new Dictionary<KnightRunTexture, Material>();
        static Shader cachedShader;

        static readonly Dictionary<KnightRunTexture, string> TexturePaths = new Dictionary<KnightRunTexture, string>
        {
            { KnightRunTexture.KnightArmor, "KnightRun/Textures/knight_armor" },
            { KnightRunTexture.KnightHelmet, "KnightRun/Textures/knight_helmet" },
            { KnightRunTexture.KnightSword, "KnightRun/Textures/knight_sword" },
            { KnightRunTexture.Enemy, "KnightRun/Textures/enemy" },
            { KnightRunTexture.GroundForest, "KnightRun/Textures/ground_forest" },
            { KnightRunTexture.GroundCave, "KnightRun/Textures/ground_cave" },
            { KnightRunTexture.GroundMine, "KnightRun/Textures/ground_mine" },
            { KnightRunTexture.GroundVolcano, "KnightRun/Textures/ground_volcano" },
            { KnightRunTexture.WallForest, "KnightRun/Textures/wall_forest" },
            { KnightRunTexture.WallCave, "KnightRun/Textures/wall_cave" },
            { KnightRunTexture.WallVolcano, "KnightRun/Textures/wall_volcano" },
            { KnightRunTexture.TreeTrunk, "KnightRun/Textures/tree_trunk" },
            { KnightRunTexture.TreeLeaves, "KnightRun/Textures/tree_leaves" },
            { KnightRunTexture.RockObstacle, "KnightRun/Textures/rock_obstacle" },
            { KnightRunTexture.LogObstacle, "KnightRun/Textures/log_obstacle" },
            { KnightRunTexture.Coin, "KnightRun/Textures/coin" },
            { KnightRunTexture.Stalactite, "KnightRun/Textures/stalactite" },
            { KnightRunTexture.MineCart, "KnightRun/Textures/mine_cart" },
            { KnightRunTexture.MineRail, "KnightRun/Textures/mine_rail" },
            { KnightRunTexture.LavaPool, "KnightRun/Textures/lava_pool" },
            { KnightRunTexture.VolcanoRock, "KnightRun/Textures/volcano_rock" },
        };

        static readonly Dictionary<KnightRunTexture, Color> FallbackColors = new Dictionary<KnightRunTexture, Color>
        {
            { KnightRunTexture.KnightArmor, new Color(0.25f, 0.35f, 0.75f) },
            { KnightRunTexture.KnightHelmet, new Color(0.72f, 0.72f, 0.78f) },
            { KnightRunTexture.KnightSword, new Color(0.82f, 0.84f, 0.88f) },
            { KnightRunTexture.Enemy, Color.white },
            { KnightRunTexture.GroundForest, new Color(0.22f, 0.45f, 0.18f) },
            { KnightRunTexture.GroundCave, new Color(0.28f, 0.24f, 0.20f) },
            { KnightRunTexture.GroundMine, new Color(0.35f, 0.22f, 0.12f) },
            { KnightRunTexture.GroundVolcano, new Color(0.28f, 0.10f, 0.06f) },
            { KnightRunTexture.WallForest, new Color(0.12f, 0.32f, 0.10f) },
            { KnightRunTexture.WallCave, new Color(0.15f, 0.13f, 0.12f) },
            { KnightRunTexture.WallVolcano, new Color(0.18f, 0.08f, 0.06f) },
            { KnightRunTexture.TreeTrunk, new Color(0.35f, 0.22f, 0.10f) },
            { KnightRunTexture.TreeLeaves, new Color(0.35f, 0.55f, 0.20f) },
            { KnightRunTexture.RockObstacle, new Color(0.45f, 0.45f, 0.48f) },
            { KnightRunTexture.LogObstacle, new Color(0.45f, 0.28f, 0.12f) },
            { KnightRunTexture.Coin, new Color(0.95f, 0.78f, 0.15f) },
            { KnightRunTexture.Stalactite, new Color(0.50f, 0.48f, 0.45f) },
            { KnightRunTexture.MineCart, new Color(0.45f, 0.28f, 0.12f) },
            { KnightRunTexture.MineRail, new Color(0.55f, 0.38f, 0.18f) },
            { KnightRunTexture.LavaPool, new Color(0.95f, 0.35f, 0.08f) },
            { KnightRunTexture.VolcanoRock, new Color(0.22f, 0.12f, 0.10f) },
        };

        public static Material Get(KnightRunTexture id, Vector2? tiling = null)
        {
            Vector2 tile = tiling ?? Vector2.one;

            if (Cache.TryGetValue(id, out Material cached))
            {
                ApplyTiling(cached, tile);
                return cached;
            }

            Material material = CreateMaterial(id, tile);
            Cache[id] = material;
            return material;
        }

        public static Material GetForPhase(RunPhase phase, PhaseSurface surface, Vector2? tiling = null)
        {
            KnightRunTexture id = surface switch
            {
                PhaseSurface.Ground => phase switch
                {
                    RunPhase.Forest => KnightRunTexture.GroundForest,
                    RunPhase.Cave => KnightRunTexture.GroundCave,
                    RunPhase.Volcano => KnightRunTexture.GroundVolcano,
                    _ => KnightRunTexture.GroundMine
                },
                PhaseSurface.Wall => phase switch
                {
                    RunPhase.Forest => KnightRunTexture.WallForest,
                    RunPhase.Volcano => KnightRunTexture.WallVolcano,
                    _ => KnightRunTexture.WallCave
                },
                _ => KnightRunTexture.GroundForest
            };

            Vector2 tile = tiling ?? (surface == PhaseSurface.Ground ? new Vector2(2f, 4f) : Vector2.one);

            // Grass looks better with denser tiling on the long forest track.
            if (phase == RunPhase.Forest && surface == PhaseSurface.Ground && tiling == null)
                tile = new Vector2(8f, 16f);

            return Get(id, tile);
        }

        static Material CreateMaterial(KnightRunTexture id, Vector2 tiling)
        {
            Shader shader = ResolveShader();
            var material = new Material(shader);
            material.name = $"KnightRun_{id}";

            Texture2D texture = Resources.Load<Texture2D>(TexturePaths[id]);
            FallbackColors.TryGetValue(id, out Color fallbackColor);

            if (texture != null)
            {
                ApplyTexture(material, texture, tiling);
            }
            else
            {
                ApplyColor(material, fallbackColor);
            }

            return material;
        }

        static Shader ResolveShader()
        {
            if (cachedShader != null)
                return cachedShader;

            string[] shaderNames =
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit",
                "Unlit/Texture",
                "Legacy Shaders/Diffuse",
                "Standard"
            };

            foreach (string name in shaderNames)
            {
                Shader shader = Shader.Find(name);
                if (shader != null)
                {
                    cachedShader = shader;
                    return cachedShader;
                }
            }

            cachedShader = Shader.Find("Hidden/InternalErrorShader");
            return cachedShader;
        }

        static void ApplyTexture(Material material, Texture2D texture, Vector2 tiling)
        {
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);

            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);

            ApplyTiling(material, tiling);

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);
        }

        static void ApplyTiling(Material material, Vector2 tiling)
        {
            if (material.HasProperty("_BaseMap"))
                material.SetTextureScale("_BaseMap", tiling);

            if (material.HasProperty("_MainTex"))
                material.SetTextureScale("_MainTex", tiling);
        }

        static void ApplyColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }
    }

    public enum PhaseSurface
    {
        Ground,
        Wall
    }
}
