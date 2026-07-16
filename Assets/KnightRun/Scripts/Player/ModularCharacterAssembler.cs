using KnightRun.Meta;
using UnityEngine;

namespace KnightRun.Player
{
    public static class ModularCharacterAssembler
    {
        const string ArmorRootName = "ARMOR PARTS";
        const string FaceRootName = "FACE DETAILS PARTS";
        const float TargetHeight = 2f;

        public static void ApplyPreset(GameObject characterRoot, ModularCharacterPreset preset)
        {
            if (characterRoot == null)
                return;

            Transform armorRoot = FindDeepChild(characterRoot.transform, ArmorRootName);
            Transform faceRoot = FindDeepChild(characterRoot.transform, FaceRootName);

            if (armorRoot != null)
            {
                ActivateMatchingChild(armorRoot.Find("HEADS"),
                    $"Head Armor Type {preset.armorType} Color {preset.armorColor}",
                    activateCategory: preset.showHelmet);
                ActivateMatchingChild(armorRoot.Find("CHESTS"),
                    $"Chest Armor Type {preset.armorType} Color {preset.armorColor}");
                ActivateMatchingChild(armorRoot.Find("ARMS"),
                    $"Arm Armor Type {preset.armorType} Color {preset.armorColor}");
                ActivateMatchingChild(armorRoot.Find("BELTS"),
                    $"Belt Armor Type {preset.armorType} Color {preset.armorColor}");
                ActivateMatchingChild(armorRoot.Find("LEGS"),
                    $"Legs Armor Type {preset.armorType} Color {preset.armorColor}");
                ActivateMatchingChild(armorRoot.Find("FEET"),
                    $"Feet Armor Type {preset.armorType} Color {preset.armorColor}");
            }

            if (faceRoot != null)
            {
                faceRoot.gameObject.SetActive(!preset.showHelmet);

                if (!preset.showHelmet)
                {
                    ActivateMatchingChild(faceRoot.Find("HAIRS"),
                        $"Hair Type {preset.hairType} Color {preset.hairColor}");
                    ActivateMatchingChild(faceRoot.Find("FACE HAIRS"),
                        preset.faceHairType > 0
                            ? $"Face Hair Type {preset.faceHairType} Color {preset.faceHairColor}"
                            : null);
                    ActivateMatchingChild(faceRoot.Find("EYES"),
                        $"Eyes Type {preset.eyesType} Color {preset.eyesColor}");
                    ActivateMatchingChild(faceRoot.Find("EYEBROWS"),
                        $"Eyebrow Type {preset.eyebrowType} Color {preset.eyebrowColor}");
                    ActivateMatchingChild(faceRoot.Find("NOSES"),
                        $"Nose Type {preset.noseType}");
                    ActivateMatchingChild(faceRoot.Find("EARS"),
                        $"Ears Type {preset.earsType}");
                }
            }

            StripColliders(characterRoot);
            ConfigureAnimator(characterRoot);
            FitScale(characterRoot, preset.visualScale <= 0f ? 1f : preset.visualScale);
        }

        public static Transform CreateSocket(Transform parent, string name, Vector3 localPosition, Vector3 localEuler)
        {
            if (parent == null)
                return null;

            Transform existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                existing.localRotation = Quaternion.Euler(localEuler);
                return existing;
            }

            var socket = new GameObject(name);
            socket.transform.SetParent(parent, false);
            socket.transform.localPosition = localPosition;
            socket.transform.localRotation = Quaternion.Euler(localEuler);
            socket.transform.localScale = Vector3.one;
            return socket.transform;
        }

        public static Transform ResolveBone(Animator animator, HumanBodyBones bone, Transform fallbackRoot, string fallbackName)
        {
            if (animator != null && animator.isHuman)
            {
                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                    return boneTransform;
            }

            return FindDeepChild(fallbackRoot, fallbackName);
        }

        static void ActivateMatchingChild(Transform category, string partName, bool activateCategory = true)
        {
            if (category == null)
                return;

            category.gameObject.SetActive(activateCategory);
            if (!activateCategory)
                return;

            Transform matched = null;
            for (int i = 0; i < category.childCount; i++)
            {
                Transform child = category.GetChild(i);
                bool isMatch = !string.IsNullOrEmpty(partName) && child.name == partName;
                child.gameObject.SetActive(isMatch);
                if (isMatch)
                    matched = child;
            }

            if (matched == null && category.childCount > 0 && !string.IsNullOrEmpty(partName))
            {
                // Fallback: first child if exact name missing.
                category.GetChild(0).gameObject.SetActive(true);
            }
            else if (matched == null && string.IsNullOrEmpty(partName))
            {
                for (int i = 0; i < category.childCount; i++)
                    category.GetChild(i).gameObject.SetActive(false);
            }
        }

        static void StripColliders(GameObject root)
        {
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(collider);
                    continue;
                }
#endif
                Object.Destroy(collider);
            }
        }

        static void ConfigureAnimator(GameObject root)
        {
            Animator animator = root.GetComponentInChildren<Animator>();
            if (animator == null)
                return;

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (animator.runtimeAnimatorController == null)
            {
                RuntimeAnimatorController controller = PlayerAnimationDriver.LoadController();
                if (controller != null)
                    animator.runtimeAnimatorController = controller;
            }
        }

        static void FitScale(GameObject root, float extraScale)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Renderer firstActive = null;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
                {
                    firstActive = renderers[i];
                    break;
                }
            }

            if (firstActive == null)
            {
                root.transform.localScale = Vector3.one * extraScale;
                return;
            }

            Bounds bounds = firstActive.bounds;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer == firstActive)
                    continue;
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                    bounds.Encapsulate(renderer.bounds);
            }

            float height = Mathf.Max(0.01f, bounds.size.y);
            float scale = (TargetHeight / height) * extraScale;
            root.transform.localScale = Vector3.one * scale;

            bounds = firstActive.bounds;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer == firstActive)
                    continue;
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                    bounds.Encapsulate(renderer.bounds);
            }

            Vector3 localPos = root.transform.localPosition;
            float feetWorldY = bounds.min.y;
            float parentY = root.transform.parent != null ? root.transform.parent.position.y : 0f;
            localPos.y += parentY - feetWorldY;
            root.transform.localPosition = localPos;
        }

        static Transform FindDeepChild(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
