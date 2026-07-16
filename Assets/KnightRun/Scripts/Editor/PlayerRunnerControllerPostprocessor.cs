#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KnightRun.EditorTools
{
    public class PlayerRunnerControllerPostprocessor : AssetPostprocessor
    {
        const string OutputPath = "Assets/KnightRun/Resources/KnightRun/Animations/PlayerRunner.controller";
        const string BlinkMarker = "Assets/Blink/Art/Animations/Animations_Starter_Pack/Movement/";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool blinkTouched = false;
            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i].StartsWith(BlinkMarker) ||
                    importedAssets[i].Contains("BuildPlayerRunnerController") ||
                    importedAssets[i].Contains("AutoBuildPlayerRunnerController"))
                {
                    blinkTouched = true;
                    break;
                }
            }

            if (!blinkTouched)
                return;

            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OutputPath) != null)
                return;

            EditorApplication.delayCall += () =>
            {
                if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OutputPath) == null)
                    BuildPlayerRunnerController.Build();
            };
        }
    }
}
#endif
