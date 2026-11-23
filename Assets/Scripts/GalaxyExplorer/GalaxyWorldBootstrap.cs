using UnityEngine;

/// <summary>
/// Creates a playable starter galaxy automatically when a scene is opened.
/// No manually placed GameObjects are required.
/// </summary>
public sealed class GalaxyWorldBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateWorld()
    {
        if (Object.FindObjectOfType<GalaxyWorldBootstrap>() != null) return;
        new GameObject("Galaxy Explorer World").AddComponent<GalaxyWorldBootstrap>();
    }

    private void Start()
    {
        Random.InitState(24112026);
        CreateLighting();
        CreateStars();
        CreatePlanets();
        CreatePlayerShip();
    }

    private static Material MakeMaterial(Color color, float metallic = 0f, float smoothness = 0.35f)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        return material;
    }

    private void CreateLighting()
    {
        var sunObject = new GameObject("Distant Sun");
        sunObject.transform.SetParent(transform, false);
        sunObject.transform.position = new Vector3(0f, 35f, 90f);
        var light = sunObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = new Color(0.78f, 0.86f, 1f);
        RenderSettings.ambientLight = new Color(0.075f, 0.09f, 0.15f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

    private void CreateStars()
    {
        var stars = new GameObject("Deep Space Starfield");
        stars.transform.SetParent(transform, false);
        var particles = stars.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = 2200;
        main.startLifetime = 100000f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.42f);
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        var emission = particles.emission;
        emission.enabled = false;
        var shape = particles.shape;
        shape.enabled = false;
        var renderer = stars.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = MakeMaterial(Color.white);
        var batch = new ParticleSystem.EmitParams();
        for (int i = 0; i < 2200; i++)
        {
            batch.position = Random.onUnitSphere * Random.Range(180f, 420f);
            batch.startSize = Random.Range(0.08f, 0.3f);
            batch.startColor = Color.Lerp(new Color(0.55f, 0.68f, 1f), Color.white, Random.value);
            particles.Emit(batch, 1);
        }
        particles.Play();
    }

    private void CreatePlanets()
    {
        Color[] palette = {
            new Color(0.18f, 0.48f, 0.85f), new Color(0.75f, 0.32f, 0.18f),
            new Color(0.72f, 0.68f, 0.48f), new Color(0.28f, 0.62f, 0.43f),
            new Color(0.48f, 0.38f, 0.78f), new Color(0.78f, 0.58f, 0.28f),
            new Color(0.34f, 0.68f, 0.72f)
        };

        for (int i = 0; i < palette.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / palette.Length;
            float orbit = 42f + i * 17f;
            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "Discoverable Planet " + (i + 1);
            planet.transform.SetParent(transform, false);
            planet.transform.localPosition = new Vector3(Mathf.Cos(angle) * orbit, Random.Range(-5f, 5f), Mathf.Sin(angle) * orbit);
            float size = 3.5f + (i % 3) * 1.4f;
            planet.transform.localScale = Vector3.one * size;
            planet.GetComponent<Renderer>().sharedMaterial = MakeMaterial(palette[i], 0.12f, 0.62f);
            planet.AddComponent<PlanetRotator>().degreesPerSecond = Random.Range(2f, 8f);
            var identity = planet.AddComponent<PlanetDiscovery>();
            identity.displayName = "World " + (i + 1);
            identity.description = "An uncharted world. Approach to record this discovery.";
            CreateOrbitRing(orbit);
        }
    }

    private void CreateOrbitRing(float radius)
    {
        var ring = new GameObject("Orbital Path");
        ring.transform.SetParent(transform, false);
        var line = ring.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 96;
        line.startWidth = 0.045f;
        line.endWidth = 0.045f;
        line.material = MakeMaterial(new Color(0.24f, 0.48f, 0.8f, 0.3f));
        line.startColor = new Color(0.24f, 0.48f, 0.8f, 0.3f);
        line.endColor = new Color(0.24f, 0.48f, 0.8f, 0.3f);
        for (int i = 0; i < line.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
        }
    }

    private void CreatePlayerShip()
    {
        var ship = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        ship.name = "Player Exploration Ship";
        ship.transform.SetParent(transform, false);
        ship.transform.position = new Vector3(0f, 0f, -18f);
        ship.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        ship.transform.localScale = new Vector3(1.5f, 2.6f, 1.5f);
        ship.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(0.72f, 0.82f, 0.95f), 0.72f, 0.8f);
        var body = ship.AddComponent<Rigidbody>();
        body.mass = 8f;
        body.useGravity = false;
        body.drag = 0.25f;
        body.angularDrag = 2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        ship.AddComponent<SpacecraftFlightController>();

        var cameraObject = new GameObject("Exploration Camera");
        cameraObject.transform.SetParent(transform, false);
        var camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.08f;
        camera.farClipPlane = 1200f;
        cameraObject.AddComponent<AudioListener>();
        var follow = cameraObject.AddComponent<SpacecraftFollowCamera>();
        follow.target = ship.transform;
        follow.offset = new Vector3(0f, 5.5f, -13f);
    }
}
