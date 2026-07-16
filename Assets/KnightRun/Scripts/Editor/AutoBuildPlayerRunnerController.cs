#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace KnightRun.EditorTools
{
    [InitializeOnLoad]
    public static class AutoBuildPlayerRunnerController
    {
        const string VersionKey = "KnightRun_PlayerRunnerBuildVersion";
        static bool buildQueued;

        static AutoBuildPlayerRunnerController()
        {
            QueueBuild();
        }

        [DidReloadScripts]
        static void OnScriptsReloaded()
        {
            QueueBuild();
        }

        static void QueueBuild()
        {
            if (buildQueued)
                return;

            buildQueued = true;
            EditorApplication.delayCall += () =>
            {
                buildQueued = false;
                TryBuildOnce();
            };
        }

        static void TryBuildOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (AssetDatabase.LoadAssetAtPath<Object>(
                    "Assets/Blink/Art/Animations/Animations_Starter_Pack/Combat/GetHit.fbx") == null)
                return;

            if (EditorPrefs.GetInt(VersionKey, 0) >= BuildPlayerRunnerController.BuildVersion &&
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/KnightRun/Resources/KnightRun/Animations/PlayerRunner.controller") != null)
                return;

            BuildPlayerRunnerController.Build();
        }
    }
}
#endif
