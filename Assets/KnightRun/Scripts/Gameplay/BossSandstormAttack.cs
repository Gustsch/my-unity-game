using KnightRun.Core;
using KnightRun.Player;
using KnightRun.World;
using UnityEngine;

namespace KnightRun.Gameplay
{
    public class BossSandstormAttack : MonoBehaviour
    {
        public const float StormDuration = 4f;
        public const float SpineWarning = 0.9f;
        public const int SpineDamage = 300;
        public const int SpineCount = 3;
        public const float PhaseSpineInterval = 1.35f;

        const string SmokeMaterialPath = "KnightRun/Particles/SandstormSmoke";
        const string DustMaterialPath = "KnightRun/Particles/SandstormDust";

        static readonly Color SandTint = new Color(0.86f, 0.66f, 0.36f, 0.85f);
        static readonly Color DustTint = new Color(0.94f, 0.78f, 0.48f, 0.9f);

        public bool IsPhaseStorm { get; private set; }

        Transform player;
        GameManager gameManager;
        float lifeTimer;
        float spineTimer;
        float lockedX;
        int spinesSpawned;
        Transform stormRoot;
        ParticleSystem smokeParticles;
        ParticleSystem dustParticles;

        public static BossSandstormAttack Spawn(Transform player)
        {
            var go = new GameObject("BossSandstorm");
            go.transform.position = player.position;

            var attack = go.AddComponent<BossSandstormAttack>();
            attack.Build(player, phaseStorm: false);
            return attack;
        }

        public static BossSandstormAttack EnsurePhaseStorm(Transform player)
        {
            BossSandstormAttack existing = FindFirstObjectByType<BossSandstormAttack>();
            if (existing != null)
            {
                existing.MakePhaseStorm(player);
                return existing;
            }

            var go = new GameObject("DesertSandstorm");
            go.transform.position = player.position;

            var attack = go.AddComponent<BossSandstormAttack>();
            attack.Build(player, phaseStorm: true);
            return attack;
        }

        public static void StopPhaseStorm()
        {
            foreach (BossSandstormAttack attack in FindObjectsByType<BossSandstormAttack>(FindObjectsSortMode.None))
            {
                if (attack != null)
                    Object.Destroy(attack.gameObject);
            }
        }

        void Build(Transform playerTransform, bool phaseStorm)
        {
            player = playerTransform;
            IsPhaseStorm = phaseStorm;
            lockedX = player.position.x;
            lifeTimer = phaseStorm ? float.PositiveInfinity : StormDuration;
            spineTimer = phaseStorm ? PhaseSpineInterval * 0.5f : SpineWarning;
            spinesSpawned = 0;

            stormRoot = new GameObject("SandstormParticles").transform;
            stormRoot.SetParent(transform, false);

            smokeParticles = CreateSmokeEmitter(stormRoot);
            dustParticles = CreateDustEmitter(stormRoot);
            UpdateStormFollow();
        }

        void MakePhaseStorm(Transform playerTransform)
        {
            if (playerTransform != null)
                player = playerTransform;

            IsPhaseStorm = true;
            lifeTimer = float.PositiveInfinity;
            if (spineTimer <= 0f)
                spineTimer = PhaseSpineInterval;
        }

        static ParticleSystem CreateSmokeEmitter(Transform parent)
        {
            var go = new GameObject("SandSmoke");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.4f, 4.2f);
            main.startColor = Color.white;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = -0.02f;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 18f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 3.2f, 10f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-5.5f, -2.5f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.6f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.8f);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.55f, 1f, 1.35f));

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(SandTint, 0f),
                    new GradientColorKey(new Color(0.72f, 0.52f, 0.28f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.55f, 0.2f),
                    new GradientAlphaKey(0.4f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = LoadStormMaterial(SmokeMaterialPath, SandTint);
            renderer.sortingFudge = 2f;

            ps.Play(true);
            return ps;
        }

        static ParticleSystem CreateDustEmitter(Transform parent)
        {
            var go = new GameObject("SandDust");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 4f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.45f);
            main.startColor = Color.white;
            main.maxParticles = 160;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 55f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(12f, 2.4f, 8f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-10f, -5f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.5f, 0.8f);
            velocity.z = new ParticleSystem.MinMaxCurve(-1f, 2f);

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(DustTint, 0f),
                    new GradientColorKey(new Color(0.78f, 0.58f, 0.3f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.85f, 0.15f),
                    new GradientAlphaKey(0.35f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = LoadStormMaterial(DustMaterialPath, DustTint);
            renderer.sortingFudge = 1f;

            ps.Play(true);
            return ps;
        }

        static Material LoadStormMaterial(string resourcePath, Color tint)
        {
            Material source = Resources.Load<Material>(resourcePath);
            if (source != null)
            {
                var instance = new Material(source)
                {
                    name = source.name + "_Runtime"
                };
                if (instance.HasProperty("_BaseColor"))
                    instance.SetColor("_BaseColor", tint);
                if (instance.HasProperty("_Color"))
                    instance.SetColor("_Color", tint);
                return instance;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");
            var fallback = new Material(shader)
            {
                name = "SandstormFallback_Runtime",
                color = tint
            };
            if (fallback.HasProperty("_BaseColor"))
                fallback.SetColor("_BaseColor", tint);
            if (fallback.HasProperty("_Surface"))
                fallback.SetFloat("_Surface", 1f);
            if (fallback.HasProperty("_SrcBlend"))
                fallback.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (fallback.HasProperty("_DstBlend"))
                fallback.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (fallback.HasProperty("_ZWrite"))
                fallback.SetFloat("_ZWrite", 0f);
            return fallback;
        }

        void Start()
        {
            gameManager = GameManager.Instance;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (player == null)
            {
                var runner = FindFirstObjectByType<RunnerController>();
                if (runner == null)
                {
                    Destroy(gameObject);
                    return;
                }

                player = runner.transform;
            }

            UpdateStormFollow();
            UpdateSpines();

            if (IsPhaseStorm)
                return;

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
                Destroy(gameObject);
        }

        void UpdateStormFollow()
        {
            if (player == null || stormRoot == null)
                return;

            float sway = Mathf.Sin(Time.time * 1.8f) * 0.35f;
            stormRoot.position = new Vector3(
                player.position.x + sway,
                1.2f,
                player.position.z + 3.5f);
        }

        void UpdateSpines()
        {
            if (!IsPhaseStorm && spinesSpawned >= SpineCount)
                return;

            spineTimer -= Time.deltaTime;
            if (spineTimer > 0f)
                return;

            SpawnSpine(lockedX);
            if (!IsPhaseStorm)
                spinesSpawned++;

            lockedX = player.position.x;
            spineTimer = IsPhaseStorm ? PhaseSpineInterval : SpineWarning;
        }

        void SpawnSpine(float targetX)
        {
            float z = player.position.z + 10f;
            BossDesertSpine.Spawn(new Vector3(targetX, 0.05f, player.position.z), new Vector3(targetX, 3.5f, z));
        }
    }

    public class BossDesertSpine : MonoBehaviour
    {
        Transform warningMark;
        Transform spine;
        Transform player;
        GameManager gameManager;
        float warningTimer = 0.75f;
        bool falling;
        float fallProgress;
        Vector3 start;
        Vector3 end;
        bool hasHit;

        public static void Spawn(Vector3 impactPoint, Vector3 spawnPoint)
        {
            var go = new GameObject("BossDesertSpine");
            go.transform.position = impactPoint;

            var spineAttack = go.AddComponent<BossDesertSpine>();
            spineAttack.Build(impactPoint, spawnPoint);
        }

        void Build(Vector3 impactPoint, Vector3 spawnPoint)
        {
            start = spawnPoint;
            end = impactPoint + Vector3.up * 0.7f;

            var mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mark.name = "SpineWarning";
            mark.transform.SetParent(transform, false);
            mark.transform.position = impactPoint;
            mark.transform.localScale = new Vector3(1.3f, 0.05f, 1.3f);
            mark.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.RockObstacle);
            ApplyTint(mark.GetComponent<Renderer>(), new Color(0.9f, 0.7f, 0.2f));
            Object.Destroy(mark.GetComponent<Collider>());
            warningMark = mark.transform;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Spine";
            body.transform.SetParent(transform, false);
            body.transform.position = start;
            body.transform.localScale = new Vector3(0.35f, 1.4f, 0.35f);
            body.GetComponent<Renderer>().sharedMaterial = KnightRunMaterials.Get(KnightRunTexture.RockObstacle);
            ApplyTint(body.GetComponent<Renderer>(), new Color(0.55f, 0.38f, 0.18f));
            Object.Destroy(body.GetComponent<Collider>());
            spine = body.transform;
            spine.gameObject.SetActive(false);
        }

        static void ApplyTint(Renderer renderer, Color color)
        {
            if (renderer == null)
                return;

            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }

        void Start()
        {
            gameManager = GameManager.Instance;
            var runner = FindFirstObjectByType<RunnerController>();
            if (runner != null)
                player = runner.transform;
        }

        void Update()
        {
            if (gameManager == null || gameManager.State != GameState.Running)
                return;

            if (!falling)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.2f;
                if (warningMark != null)
                    warningMark.localScale = new Vector3(1.3f * pulse, 0.05f, 1.3f * pulse);

                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                {
                    falling = true;
                    if (warningMark != null)
                        warningMark.gameObject.SetActive(false);
                    if (spine != null)
                        spine.gameObject.SetActive(true);
                }

                return;
            }

            fallProgress = Mathf.MoveTowards(fallProgress, 1f, Time.deltaTime * 2.8f);
            if (spine != null)
                spine.position = Vector3.Lerp(start, end, fallProgress);

            if (fallProgress >= 1f)
            {
                TryHit();
                Destroy(gameObject, 0.25f);
            }
        }

        void TryHit()
        {
            if (hasHit || player == null)
                return;

            Vector3 delta = player.position - end;
            delta.y = 0f;
            if (delta.sqrMagnitude > 1.15f * 1.15f)
                return;

            var runner = player.GetComponent<RunnerController>();
            if (runner != null && !runner.IsGrounded)
                return;

            KnightHealth health = player.GetComponent<KnightHealth>();
            health?.TakeDamage(BossSandstormAttack.SpineDamage);
            hasHit = true;
        }
    }
}
