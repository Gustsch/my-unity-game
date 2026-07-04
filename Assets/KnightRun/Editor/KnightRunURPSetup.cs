#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KnightRun.Editor
{
    [InitializeOnLoad]
    public static class KnightRunURPSetup
    {
        const string PipelinePath = "Assets/KnightRun/Settings/KnightRun_URP.asset";
        const string RendererPath = "Assets/KnightRun/Settings/KnightRun_Renderer.asset";

        static KnightRunURPSetup()
        {
            EditorApplication.delayCall += EnsureRenderPipeline;
        }

        [MenuItem("Knight Run/Setup URP Pipeline")]
        public static void EnsureRenderPipeline()
        {
            if (GraphicsSettings.defaultRenderPipeline != null)
                return;

            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            UniversalRendererData renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);

            if (pipeline == null || renderer == null)
            {
                CreatePipelineAssets(out pipeline, out renderer);
            }

            if (pipeline == null)
                return;

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;

            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
                QualitySettings.renderPipeline = pipeline;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Knight Run] URP pipeline configurado — materiais rosa devem sumir após recompilar.");
        }

        static void CreatePipelineAssets(out UniversalRenderPipelineAsset pipeline, out UniversalRendererData renderer)
        {
            System.IO.Directory.CreateDirectory("Assets/KnightRun/Settings");

            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            renderer.name = "KnightRun_Renderer";
            AssetDatabase.CreateAsset(renderer, RendererPath);

            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "KnightRun_URP";
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
