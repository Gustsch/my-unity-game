using System;
using KnightRun.Meta;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Player
{
    public class PlayerCharacterVisual : MonoBehaviour
    {
        public const string VisualRootName = "VisualRoot";
        public const string RightHandSocketName = "RightHandSocket";
        public const string LeftHandSocketName = "LeftHandSocket";
        public const string ThrowSocketName = "ThrowSocket";
        public const string BackSocketName = "BackSocket";

        Transform visualRoot;
        Transform characterInstance;
        Transform legacyHelmet;
        Renderer legacyBodyRenderer;
        bool usingLegacyVisual;
        float currentStrafeYaw;

        const float MaxStrafeYawDegrees = 35f;
        const float StrafeYawTurnSpeed = 10f;

        public Transform VisualRoot => visualRoot;
        public Transform RightHandSocket { get; private set; }
        public Transform LeftHandSocket { get; private set; }
        public Transform ThrowSocket { get; private set; }
        public Transform BackSocket { get; private set; }
        public Transform LegacyHelmet => legacyHelmet;
        public Renderer LegacyBodyRenderer => legacyBodyRenderer;
        public bool IsLegacyVisual => usingLegacyVisual;
        public HeroCharacterId CurrentVisualId { get; private set; }

        public event Action OnVisualRebuilt;

        public void Build()
        {
            EnsureVisualRoot();
            CharacterSelection.OnCharacterChanged -= HandleCharacterChanged;
            CharacterSelection.OnCharacterChanged += HandleCharacterChanged;
            RebuildFor(CharacterSelection.SelectedCharacter);
        }

        void OnDestroy()
        {
            CharacterSelection.OnCharacterChanged -= HandleCharacterChanged;
        }

        void HandleCharacterChanged()
        {
            RebuildFor(CharacterSelection.SelectedCharacter);
        }

        public void RebuildFor(HeroCharacterId id)
        {
            EnsureVisualRoot();
            ClearVisualChildren();

            CurrentVisualId = id;
            CharacterVisualDefinition definition = CharacterVisualCatalog.GetDefinition(id);
            usingLegacyVisual = definition.useLegacyCapsuleVisual;

            GetComponent<PlayerAnimationDriver>()?.Unbind();

            if (usingLegacyVisual)
                BuildLegacyVisual();
            else
                BuildModularVisual(definition.preset);

            ResetStrafeFacing();
            OnVisualRebuilt?.Invoke();
        }

        /// <summary>
        /// Turns the character diagonally while strafing. strafeAmount in [-1, 1]
        /// (negative = left, positive = right). Pass 0 to face forward.
        /// </summary>
        public void SetStrafeFacing(float strafeAmount, bool allowTurn = true)
        {
            if (visualRoot == null)
                return;

            float targetYaw = allowTurn
                ? Mathf.Clamp(strafeAmount, -1f, 1f) * MaxStrafeYawDegrees
                : 0f;

            currentStrafeYaw = Mathf.LerpAngle(
                currentStrafeYaw,
                targetYaw,
                1f - Mathf.Exp(-StrafeYawTurnSpeed * Time.deltaTime));

            visualRoot.localRotation = Quaternion.Euler(0f, currentStrafeYaw, 0f);
        }

        public void ResetStrafeFacing()
        {
            currentStrafeYaw = 0f;
            if (visualRoot != null)
                visualRoot.localRotation = Quaternion.identity;
        }

        void EnsureVisualRoot()
        {
            if (visualRoot != null)
                return;

            Transform existing = transform.Find(VisualRootName);
            if (existing != null)
            {
                visualRoot = existing;
                return;
            }

            var rootGo = new GameObject(VisualRootName);
            rootGo.transform.SetParent(transform, false);
            rootGo.transform.localPosition = Vector3.zero;
            rootGo.transform.localRotation = Quaternion.identity;
            rootGo.transform.localScale = Vector3.one;
            visualRoot = rootGo.transform;
        }

        void ClearVisualChildren()
        {
            RightHandSocket = null;
            LeftHandSocket = null;
            ThrowSocket = null;
            BackSocket = null;
            legacyHelmet = null;
            legacyBodyRenderer = null;
            characterInstance = null;

            if (visualRoot == null)
                return;

            for (int i = visualRoot.childCount - 1; i >= 0; i--)
                Destroy(visualRoot.GetChild(i).gameObject);
        }

        void BuildLegacyVisual()
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "LegacyBody";
            body.transform.SetParent(visualRoot, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = Vector3.one;
            Destroy(body.GetComponent<Collider>());
            legacyBodyRenderer = body.GetComponent<Renderer>();
            legacyBodyRenderer.sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightArmor);

            var helmet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            helmet.name = "Helmet";
            helmet.transform.SetParent(visualRoot, false);
            helmet.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
            helmet.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            helmet.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.KnightHelmet);
            Destroy(helmet.GetComponent<Collider>());
            legacyHelmet = helmet.transform;

            CreateFallbackSockets();
        }

        void BuildModularVisual(ModularCharacterPreset preset)
        {
            GameObject prefab = CharacterVisualCatalog.LoadCharacterPrefab(CurrentVisualId);
            if (prefab == null)
            {
                Debug.LogWarning("KnightRun: Modular character prefab missing. Falling back to legacy capsule.");
                BuildLegacyVisual();
                usingLegacyVisual = true;
                return;
            }

            GameObject instance = Instantiate(prefab, visualRoot);
            instance.name = $"Character_{CurrentVisualId}";
            instance.transform.localPosition = preset.visualOffset;
            instance.transform.localRotation = Quaternion.Euler(preset.visualEuler);
            instance.transform.localScale = Vector3.one;
            characterInstance = instance.transform;

            ModularCharacterAssembler.ApplyPreset(instance, preset);

            Animator animator = instance.GetComponentInChildren<Animator>();
            GetComponent<PlayerAnimationDriver>()?.Bind(animator);

            Transform rightHand = ModularCharacterAssembler.ResolveBone(
                animator, HumanBodyBones.RightHand, instance.transform, "hand_r");
            Transform leftHand = ModularCharacterAssembler.ResolveBone(
                animator, HumanBodyBones.LeftHand, instance.transform, "hand_l");

            RightHandSocket = ModularCharacterAssembler.CreateSocket(
                rightHand != null ? rightHand : instance.transform,
                RightHandSocketName,
                new Vector3(0.04f, 0.02f, 0.02f),
                new Vector3(0f, 90f, 90f));

            LeftHandSocket = ModularCharacterAssembler.CreateSocket(
                leftHand != null ? leftHand : instance.transform,
                LeftHandSocketName,
                new Vector3(-0.04f, 0.02f, 0.02f),
                new Vector3(0f, -90f, -90f));

            ThrowSocket = ModularCharacterAssembler.CreateSocket(
                RightHandSocket != null ? RightHandSocket : instance.transform,
                ThrowSocketName,
                new Vector3(0f, 0.05f, 0.08f),
                Vector3.zero);

            Transform spine = ModularCharacterAssembler.ResolveBone(
                animator, HumanBodyBones.Spine, instance.transform, "spine_02");
            BackSocket = ModularCharacterAssembler.CreateSocket(
                spine != null ? spine : instance.transform,
                BackSocketName,
                new Vector3(0f, 0.1f, -0.12f),
                new Vector3(0f, 180f, 0f));
        }

        void CreateFallbackSockets()
        {
            RightHandSocket = ModularCharacterAssembler.CreateSocket(
                visualRoot, RightHandSocketName, new Vector3(0.45f, 0.9f, 0.15f), new Vector3(-20f, 70f, 0f));
            LeftHandSocket = ModularCharacterAssembler.CreateSocket(
                visualRoot, LeftHandSocketName, new Vector3(-0.55f, 0.95f, 0.05f), new Vector3(0f, -90f, 0f));
            ThrowSocket = ModularCharacterAssembler.CreateSocket(
                visualRoot, ThrowSocketName, new Vector3(0.45f, 1.05f, 0.2f), Vector3.zero);
            BackSocket = ModularCharacterAssembler.CreateSocket(
                visualRoot, BackSocketName, new Vector3(0f, 1.1f, -0.2f), Vector3.zero);
        }

        public Transform GetWeaponParent(WeaponMount mount, Transform fallback)
        {
            Transform socket = mount switch
            {
                WeaponMount.RightHand => RightHandSocket,
                WeaponMount.LeftHand => LeftHandSocket,
                WeaponMount.Throw => ThrowSocket,
                WeaponMount.Back => BackSocket,
                _ => null
            };

            return socket != null ? socket : fallback;
        }
    }

    public enum WeaponMount
    {
        RightHand,
        LeftHand,
        Throw,
        Back,
        Root
    }
}
