using UnityEngine;

/// <summary>
/// Creates a playable starter galaxy automatically when a scene is opened.
/// No manually placed GameObjects are required.
/// </summary>
public sealed class GalaxyWorldBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
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
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
        renderer.material = new Material(particleShader);
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
            var surfaceMaterial = MakeMaterial(palette[i], 0.12f, 0.62f);
            surfaceMaterial.mainTexture = PlanetTextureFactory.Create(palette[i], i + 1, i == 0 || i == 5);
            if (surfaceMaterial.HasProperty("_BumpMap"))
            {
                surfaceMaterial.SetTexture("_BumpMap", PlanetTextureFactory.CreateNormalMap(i + 1));
                surfaceMaterial.EnableKeyword("_NORMALMAP");
                if (surfaceMaterial.HasProperty("_BumpScale")) surfaceMaterial.SetFloat("_BumpScale", 0.65f);
            }
            planet.GetComponent<Renderer>().sharedMaterial = surfaceMaterial;
            planet.AddComponent<PlanetRotator>().degreesPerSecond = Random.Range(2f, 8f);
            if (i == 5) CreatePlanetRing(planet.transform, size);
            var identity = planet.AddComponent<PlanetDiscovery>();
            identity.displayName = "World " + (i + 1);
            identity.description = "An uncharted world. Approach to record this discovery.";
            CreateOrbitRing(orbit);
        }
    }

    private void CreatePlanetRing(Transform parent, float planetSize)
    {
        const int segments = 96;
        float inner = planetSize * 0.78f;
        float outer = planetSize * 1.28f;
        var mesh = new Mesh { name = "Gas Giant Ring Mesh" };
        var vertices = new Vector3[segments * 2];
        var triangles = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[i * 2] = direction * inner;
            vertices[i * 2 + 1] = direction * outer;
            int next = (i + 1) % segments;
            int t = i * 6;
            triangles[t] = i * 2;
            triangles[t + 1] = next * 2 + 1;
            triangles[t + 2] = i * 2 + 1;
            triangles[t + 3] = i * 2;
            triangles[t + 4] = next * 2;
            triangles[t + 5] = next * 2 + 1;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        var ring = new GameObject("Planetary Ring");
        ring.transform.SetParent(parent, false);
        var filter = ring.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        var renderer = ring.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = MakeMaterial(new Color(0.72f, 0.61f, 0.42f), 0.55f, 0.72f);
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
        BuildShipModel(ship.transform);
        var body = ship.AddComponent<Rigidbody>();
        body.mass = 8f;
        body.useGravity = false;
        body.drag = 0.25f;
        body.angularDrag = 2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        ship.AddComponent<SpacecraftFlightController>();
        ship.AddComponent<ProceduralSpaceAudio>();

        Camera camera = Camera.main;
        GameObject cameraObject;
        if (camera == null)
        {
            cameraObject = new GameObject("Exploration Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }
        else cameraObject = camera.gameObject;

        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.08f;
        camera.farClipPlane = 1200f;
        var follow = cameraObject.GetComponent<SpacecraftFollowCamera>();
        if (follow == null) follow = cameraObject.AddComponent<SpacecraftFollowCamera>();
        follow.target = ship.transform;
        follow.offset = new Vector3(0f, 5.5f, -13f);

        if (FindObjectOfType<SpaceHUD>() == null)
            new GameObject("Galaxy Explorer HUD").AddComponent<SpaceHUD>();
        if (FindObjectOfType<SpaceGameMenu>() == null)
            new GameObject("Galaxy Explorer Menu").AddComponent<SpaceGameMenu>();
    }

    private void BuildShipModel(Transform root)
    {
        Material wingMaterial = MakeMaterial(new Color(0.16f, 0.24f, 0.38f), 0.8f, 0.72f);
        Material glassMaterial = MakeMaterial(new Color(0.12f, 0.62f, 0.88f), 0.35f, 0.9f);
        Material engineMaterial = MakeMaterial(new Color(0.12f, 0.42f, 1f), 0.15f, 0.8f);

        CreateShipPart(root, "Left Wing", PrimitiveType.Cube, new Vector3(-1.25f, 0f, -0.15f), new Vector3(1.7f, 0.16f, 1.5f), wingMaterial);
        CreateShipPart(root, "Right Wing", PrimitiveType.Cube, new Vector3(1.25f, 0f, -0.15f), new Vector3(1.7f, 0.16f, 1.5f), wingMaterial);
        CreateShipPart(root, "Cockpit", PrimitiveType.Sphere, new Vector3(0f, 0.48f, 1.05f), new Vector3(0.72f, 0.52f, 1.15f), glassMaterial);
        CreateShipPart(root, "Left Engine", PrimitiveType.Sphere, new Vector3(-0.48f, 0f, -2.15f), Vector3.one * 0.42f, engineMaterial);
        CreateShipPart(root, "Right Engine", PrimitiveType.Sphere, new Vector3(0.48f, 0f, -2.15f), Vector3.one * 0.42f, engineMaterial);
    }

    private static void CreateShipPart(Transform parent, string partName, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        var collider = part.GetComponent<Collider>();
        if (collider != null) Object.Destroy(collider);
    }
}
