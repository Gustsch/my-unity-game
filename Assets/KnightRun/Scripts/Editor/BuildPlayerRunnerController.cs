#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KnightRun.EditorTools
{
    public static class BuildPlayerRunnerController
    {
        const string OutputFolder = "Assets/KnightRun/Resources/KnightRun/Animations";
        const string OutputPath = OutputFolder + "/PlayerRunner.controller";

        const string RunPath =
            "Assets/Blink/Art/Animations/Animations_Starter_Pack/Movement/RunForward.fbx";
        const string JumpPath =
            "Assets/Blink/Art/Animations/Animations_Starter_Pack/Movement/JumpWhileRunning.fbx";
        const string RollPath =
            "Assets/Blink/Art/Animations/Animations_Starter_Pack/Movement/RollForward.fbx";
        const string FallPath =
            "Assets/Blink/Art/Animations/Animations_Starter_Pack/Movement/FallingLoop.fbx";
        const string IdleCombatPath =
            "Assets/Blink/Art/Animations/Animations_Starter_Pack/Combat/IdleCombat.fbx";
        const string GetHitPath =
            "Assets/Blink/Art/Animations/Animations_Starter_Pack/Combat/GetHit.fbx";

        public const int BuildVersion = 3;

        [MenuItem("KnightRun/Build Player Runner Animator")]
        public static void Build()
        {
            AnimationClip run = LoadClip(RunPath, "RunForward");
            AnimationClip jump = LoadClip(JumpPath, "JumpWhileRunning");
            AnimationClip roll = LoadClip(RollPath, "RollForward");
            AnimationClip fall = LoadClip(FallPath, "FallingLoop");
            AnimationClip idleCombat = LoadClip(IdleCombatPath, "IdleCombat");
            AnimationClip getHit = LoadClip(GetHitPath, "GetHit");

            if (run == null || jump == null || roll == null || fall == null ||
                idleCombat == null || getHit == null)
            {
                Debug.LogError(
                    "KnightRun: Missing Blink animation clips. " +
                    $"run={run != null} jump={jump != null} roll={roll != null} " +
                    $"fall={fall != null} idleCombat={idleCombat != null} getHit={getHit != null}");
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/KnightRun/Resources"))
                AssetDatabase.CreateFolder("Assets/KnightRun", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/KnightRun/Resources/KnightRun"))
                AssetDatabase.CreateFolder("Assets/KnightRun/Resources", "KnightRun");
            if (!AssetDatabase.IsValidFolder(OutputFolder))
                AssetDatabase.CreateFolder("Assets/KnightRun/Resources/KnightRun", "Animations");

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(OutputPath) != null)
                AssetDatabase.DeleteAsset(OutputPath);

            AssetDatabase.Refresh();

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Sliding", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Stumbling", AnimatorControllerParameterType.Bool);
            controller.AddParameter("InMineCart", AnimatorControllerParameterType.Bool);
            controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);

            AnimatorStateMachine root = controller.layers[0].stateMachine;

            AnimatorState runState = root.AddState("Run", new Vector3(300f, 120f, 0f));
            runState.motion = run;

            AnimatorState mineCartIdleState = root.AddState("MineCartIdle", new Vector3(80f, 120f, 0f));
            mineCartIdleState.motion = idleCombat;

            AnimatorState jumpState = root.AddState("Jump", new Vector3(300f, 0f, 0f));
            jumpState.motion = jump;

            AnimatorState fallState = root.AddState("Fall", new Vector3(520f, 0f, 0f));
            fallState.motion = fall;

            AnimatorState slideState = root.AddState("Slide", new Vector3(300f, 240f, 0f));
            slideState.motion = roll;

            AnimatorState stumbleState = root.AddState("Stumble", new Vector3(520f, 240f, 0f));
            stumbleState.motion = getHit;

            root.defaultState = runState;

            // Any State -> Stumble
            AnimatorStateTransition anyToStumble = root.AddAnyStateTransition(stumbleState);
            anyToStumble.AddCondition(AnimatorConditionMode.If, 0f, "Stumbling");
            anyToStumble.hasExitTime = false;
            anyToStumble.duration = 0.08f;
            anyToStumble.canTransitionToSelf = false;

            // Any State -> Slide
            AnimatorStateTransition anyToSlide = root.AddAnyStateTransition(slideState);
            anyToSlide.AddCondition(AnimatorConditionMode.If, 0f, "Sliding");
            anyToSlide.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            anyToSlide.hasExitTime = false;
            anyToSlide.duration = 0.08f;
            anyToSlide.canTransitionToSelf = false;

            // Any State -> MineCartIdle
            AnimatorStateTransition anyToMineCart = root.AddAnyStateTransition(mineCartIdleState);
            anyToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "InMineCart");
            anyToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            anyToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            anyToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            anyToMineCart.hasExitTime = false;
            anyToMineCart.duration = 0.12f;
            anyToMineCart.canTransitionToSelf = false;

            // Run <-> MineCartIdle / Jump
            AnimatorStateTransition runToJump = runState.AddTransition(jumpState);
            runToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            runToJump.AddCondition(AnimatorConditionMode.Greater, 0.5f, "VerticalVelocity");
            runToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            runToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            runToJump.hasExitTime = false;
            runToJump.duration = 0.05f;

            AnimatorStateTransition mineCartToJump = mineCartIdleState.AddTransition(jumpState);
            mineCartToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            mineCartToJump.AddCondition(AnimatorConditionMode.Greater, 0.5f, "VerticalVelocity");
            mineCartToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            mineCartToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            mineCartToJump.hasExitTime = false;
            mineCartToJump.duration = 0.05f;

            AnimatorStateTransition mineCartToRun = mineCartIdleState.AddTransition(runState);
            mineCartToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "InMineCart");
            mineCartToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            mineCartToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            mineCartToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            mineCartToRun.hasExitTime = false;
            mineCartToRun.duration = 0.12f;

            // Jump / Fall landings
            AnimatorStateTransition jumpToFall = jumpState.AddTransition(fallState);
            jumpToFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");
            jumpToFall.AddCondition(AnimatorConditionMode.Less, 0.1f, "VerticalVelocity");
            jumpToFall.hasExitTime = false;
            jumpToFall.duration = 0.08f;

            AnimatorStateTransition jumpToRun = jumpState.AddTransition(runState);
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            jumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "InMineCart");
            jumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            jumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            jumpToRun.hasExitTime = false;
            jumpToRun.duration = 0.08f;

            AnimatorStateTransition jumpToMineCart = jumpState.AddTransition(mineCartIdleState);
            jumpToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            jumpToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "InMineCart");
            jumpToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            jumpToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            jumpToMineCart.hasExitTime = false;
            jumpToMineCart.duration = 0.08f;

            AnimatorStateTransition fallToRun = fallState.AddTransition(runState);
            fallToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            fallToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "InMineCart");
            fallToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            fallToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            fallToRun.hasExitTime = false;
            fallToRun.duration = 0.1f;

            AnimatorStateTransition fallToMineCart = fallState.AddTransition(mineCartIdleState);
            fallToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            fallToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "InMineCart");
            fallToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            fallToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            fallToMineCart.hasExitTime = false;
            fallToMineCart.duration = 0.1f;

            // Slide / Stumble exits
            AnimatorStateTransition slideToRun = slideState.AddTransition(runState);
            slideToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            slideToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            slideToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "InMineCart");
            slideToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            slideToRun.hasExitTime = false;
            slideToRun.duration = 0.1f;

            AnimatorStateTransition slideToMineCart = slideState.AddTransition(mineCartIdleState);
            slideToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            slideToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            slideToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "InMineCart");
            slideToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            slideToMineCart.hasExitTime = false;
            slideToMineCart.duration = 0.1f;

            AnimatorStateTransition stumbleToRun = stumbleState.AddTransition(runState);
            stumbleToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            stumbleToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            stumbleToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "InMineCart");
            stumbleToRun.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            stumbleToRun.hasExitTime = false;
            stumbleToRun.duration = 0.12f;

            AnimatorStateTransition stumbleToMineCart = stumbleState.AddTransition(mineCartIdleState);
            stumbleToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stumbling");
            stumbleToMineCart.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sliding");
            stumbleToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "InMineCart");
            stumbleToMineCart.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            stumbleToMineCart.hasExitTime = false;
            stumbleToMineCart.duration = 0.12f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorPrefs.SetInt("KnightRun_PlayerRunnerBuildVersion", BuildVersion);
            Debug.Log("KnightRun: Built PlayerRunner.controller at " + OutputPath);
        }

        static AnimationClip LoadClip(string fbxPath, string clipName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            AnimationClip fallback = null;
            foreach (Object asset in assets)
            {
                if (asset is not AnimationClip clip)
                    continue;

                if (clip.name == clipName)
                    return clip;

                if (!clip.name.Contains("tpose") && fallback == null)
                    fallback = clip;
            }

            return fallback;
        }
    }
}
#endif
