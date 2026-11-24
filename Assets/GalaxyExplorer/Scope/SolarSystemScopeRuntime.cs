using System;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer.Scope
{
    [DisallowMultipleComponent]
    public sealed class SolarSystemScopeRuntime : MonoBehaviour
    {
        [Serializable]
        private sealed class BodyDefinition
        {
            public string name;
            public string type;
            public float radius;
            public float orbitRadius;
            public float orbitalPeriodDays;
            public float rotationHours;
            public Color color;
            public string description;

            public BodyDefinition(string name, string type, float radius, float orbitRadius,
                float period, float rotation, Color color, string description)
            {
                this.name = name;
                this.type = type;
                this.radius = radius;
                this.orbitRadius = orbitRadius;
                orbitalPeriodDays = period;
                rotationHours = rotation;
                this.color = color;
                this.description = description;
            }
        }

        private sealed class BodyView
        {
            public BodyDefinition data;
            public Transform transform;
            public LineRenderer orbit;
        }

        [Header("Simulation")]
        [SerializeField, Min(0f)] private float simulationSpeed = 1f;
        [SerializeField] private bool animateOrbits = true;
        [SerializeField] private bool showOrbitPaths = true;
        [SerializeField] private bool showLabels = true;
        [SerializeField] private bool startPaused;

        private readonly List<BodyView> bodies = new List<BodyView>();
        private BodyView selected;
        private Material lineMaterial;
        private Material starMaterial;
        private bool paused;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle panelStyle;

        private const int OrbitSegments = 180;
        private const float DistanceScale = 4f;
        private const float RadiusScale = 0.42f;

        private static readonly BodyDefinition[] Definitions =
        {
            new BodyDefinition("Mercury", "Terrestrial planet", 0.383f, 0.39f, 87.969f, 1407.6f,
                new Color(0.58f, 0.55f, 0.50f), "Smallest planet. Its surface is heavily cratered and it has almost no atmosphere."),
            new BodyDefinition("Venus", "Terrestrial planet", 0.949f, 0.72f, 224.701f, -5832.5f,
                new Color(0.88f, 0.67f, 0.38f), "A rocky world with a dense carbon-dioxide atmosphere and extreme surface temperatures."),
            new BodyDefinition("Earth", "Terrestrial planet", 1f, 1f, 365.256f, 23.934f,
                new Color(0.20f, 0.48f, 0.92f), "Our home world. Liquid water is stable at the surface and life is known to exist here."),
            new BodyDefinition("Mars", "Terrestrial planet", 0.532f, 1.524f, 686.980f, 24.623f,
                new Color(0.78f, 0.30f, 0.18f), "A cold desert world with volcanoes, ancient river features and two small moons."),
            new BodyDefinition("Jupiter", "Gas giant", 11.21f, 5.203f, 4332.59f, 9.925f,
                new Color(0.78f, 0.61f, 0.43f), "The largest planet. Its atmosphere contains powerful storms, including the Great Red Spot."),
            new BodyDefinition("Saturn", "Gas giant", 9.45f, 9.537f, 10759.2f, 10.7f,
                new Color(0.82f, 0.75f, 0.57f), "A gas giant famous for its extensive ring system, made mostly of icy particles."),
            new BodyDefinition("Uranus", "Ice giant", 4.01f, 19.191f, 30688.5f, -17.24f,
                new Color(0.48f, 0.80f, 0.83f), "An ice giant with a strongly tilted rotation axis and a cold atmosphere."),
            new BodyDefinition("Neptune", "Ice giant", 3.88f, 30.07f, 60182f, 16.11f,
                new Color(0.22f, 0.34f, 0.88f), "A distant ice giant with very fast atmospheric winds.")
        };

        public float SimulationSpeed { get { return simulationSpeed; } }
        public bool IsPaused { get { return paused; } }

        private void Start()
        {
            paused = startPaused;
            BuildScene();
        }

        private void Update()
        {
            if (!paused && animateOrbits)
            {
                float dayStep = Time.deltaTime * simulationSpeed * 12f;
                foreach (BodyView body in bodies)
                {
                    float period = Mathf.Max(1f, body.data.orbitalPeriodDays);
                    body.transform.RotateAround(Vector3.zero, Vector3.up, 360f * dayStep / period);
                    float spinHours = Mathf.Abs(body.data.rotationHours);
                    if (spinHours > 0.01f)
                    {
                        float direction = body.data.rotationHours < 0f ? -1f : 1f;
                        body.transform.Rotate(Vector3.up, direction * 360f * Time.deltaTime * 24f / spinHours,
                            Space.Self);
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverGui())
                SelectAt(Input.mousePosition);
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
                SelectAt(Input.GetTouch(0).position);
        }

        private void BuildScene()
        {
            CreateLighting();
            CreateStarfield();
            CreateSun();

            foreach (BodyDefinition definition in Definitions)
            {
                BodyView view = CreatePlanet(definition);
                bodies.Add(view);
            }

            if (Camera.main == null)
            {
                GameObject cameraObject = new GameObject("Scope Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 16f, -34f);
                cameraObject.transform.LookAt(Vector3.zero);
                cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            Camera camera = Camera.main;
            if (camera != null && camera.GetComponent<SolarSystemScopeCamera>() == null)
                camera.gameObject.AddComponent<SolarSystemScopeCamera>();

            selected = bodies.Count > 2 ? bodies[2] : null;
        }

        private void CreateLighting()
        {
            GameObject lightObject = new GameObject("Solar System Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.91f, 0.75f);
            light.intensity = 2200f;
            light.range = 500f;
            lightObject.transform.position = Vector3.zero;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.10f, 0.12f, 0.19f);
        }

        private void CreateSun()
        {
            GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sun.name = "Sun";
            sun.transform.SetParent(transform, false);
            sun.transform.localScale = Vector3.one * 2.8f;
            Renderer renderer = sun.GetComponent<Renderer>();
            Material material = NewMaterial(new Color(1f, 0.62f, 0.12f));
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(1f, 0.38f, 0.04f) * 2.5f);
            }
            renderer.material = material;
            Destroy(sun.GetComponent<Collider>());
            AddLabel(sun.transform, "SUN", 2.0f);
        }

        private BodyView CreatePlanet(BodyDefinition definition)
        {
            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = definition.name;
            planet.transform.SetParent(transform, false);
            float visualRadius = Mathf.Max(0.22f, Mathf.Pow(definition.radius, 0.55f) * RadiusScale);
            planet.transform.localScale = Vector3.one * visualRadius * 2f;
            planet.transform.position = new Vector3(definition.orbitRadius * DistanceScale, 0f, 0f);
            planet.GetComponent<Renderer>().material = NewMaterial(definition.color);

            BodyView view = new BodyView { data = definition, transform = planet.transform };
            if (showOrbitPaths)
                view.orbit = CreateOrbit(definition.orbitRadius * DistanceScale, definition.name + " Orbit");

            if (definition.name == "Saturn")
                CreateSaturnRings(planet.transform, visualRadius);

            AddLabel(planet.transform, definition.name, visualRadius + 0.45f);
            return view;
        }

        private LineRenderer CreateOrbit(float radius, string objectName)
        {
            GameObject orbitObject = new GameObject(objectName);
            orbitObject.transform.SetParent(transform, false);
            LineRenderer line = orbitObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = OrbitSegments;
            line.widthMultiplier = 0.025f;
            line.material = GetLineMaterial();
            line.startColor = new Color(0.35f, 0.48f, 0.68f, 0.38f);
            line.endColor = line.startColor;

            for (int i = 0; i < OrbitSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / OrbitSegments;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
            return line;
        }

        private void CreateSaturnRings(Transform parent, float radius)
        {
            GameObject ringObject = new GameObject("Saturn Rings");
            ringObject.transform.SetParent(parent, false);
            ringObject.transform.localRotation = Quaternion.Euler(25f, 0f, 12f);
            LineRenderer ring = ringObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 128;
            ring.widthMultiplier = radius * 0.55f;
            ring.material = GetLineMaterial();
            ring.startColor = new Color(0.78f, 0.68f, 0.48f, 0.75f);
            ring.endColor = ring.startColor;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius * 1.55f, 0f,
                    Mathf.Sin(angle) * radius * 1.55f));
            }
        }

        private void CreateStarfield()
        {
            GameObject starRoot = new GameObject("Procedural Starfield");
            starRoot.transform.SetParent(transform, false);
            Mesh mesh = new Mesh { name = "Starfield Points" };
            const int count = 1800;
            Vector3[] vertices = new Vector3[count];
            Color[] colors = new Color[count];
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(7321);
            for (int i = 0; i < count; i++)
            {
                Vector3 direction = UnityEngine.Random.onUnitSphere;
                vertices[i] = direction * UnityEngine.Random.Range(180f, 260f);
                float brightness = UnityEngine.Random.Range(0.45f, 1f);
                colors[i] = new Color(brightness, brightness, brightness * UnityEngine.Random.Range(0.85f, 1f), 1f);
            }
            UnityEngine.Random.state = oldState;
            mesh.vertices = vertices;
            mesh.colors = colors;
            int[] indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            MeshFilter filter = starRoot.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = starRoot.AddComponent<MeshRenderer>();
            starMaterial = new Material(Shader.Find("Sprites/Default"));
            renderer.sharedMaterial = starMaterial;
        }

        private void AddLabel(Transform target, string text, float height)
        {
            if (!showLabels) return;
            GameObject labelObject = new GameObject(text + " Label");
            labelObject.transform.SetParent(target, false);
            labelObject.transform.localPosition = Vector3.up * height;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.16f;
            label.fontSize = 48;
            label.color = new Color(0.82f, 0.90f, 1f);
        }

        private Material NewMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }

        private Material GetLineMaterial()
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
                lineMaterial = new Material(shader);
            }
            return lineMaterial;
        }

        private void SelectAt(Vector2 screenPosition)
        {
            Camera camera = Camera.main;
            if (camera == null) return;
            Ray ray = camera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                foreach (BodyView body in bodies)
                {
                    if (hit.transform == body.transform || hit.transform.IsChildOf(body.transform))
                    {
                        selected = body;
                        SolarSystemScopeCamera controller = camera.GetComponent<SolarSystemScopeCamera>();
                        if (controller != null) controller.Focus(body.transform);
                        return;
                    }
                }
            }
        }

        private bool IsPointerOverGui()
        {
            return Event.current != null && Event.current.type == EventType.Used;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.width / 1100f, 0.72f, 1.15f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            GUI.Box(new Rect(18, 18, 310, 94), GUIContent.none, panelStyle);
            GUI.Label(new Rect(34, 28, 280, 32), "GALAXY EXPLORER", titleStyle);
            GUI.Label(new Rect(34, 62, 280, 30), "SOLAR SYSTEM SCOPE  •  SCIENCE MODE", smallStyle);

            if (selected != null)
            {
                GUI.Box(new Rect(18, 125, 310, 190), GUIContent.none, panelStyle);
                GUI.Label(new Rect(34, 137, 275, 30), selected.data.name, titleStyle);
                GUI.Label(new Rect(34, 172, 275, 24), selected.data.type, smallStyle);
                GUI.Label(new Rect(34, 200, 275, 90), selected.data.description, bodyStyle);
            }

            float rightX = Screen.width / scale - 190f;
            GUI.Box(new Rect(rightX, 18, 170, 130), GUIContent.none, panelStyle);
            GUI.Label(new Rect(rightX + 12, 28, 150, 24), "SIMULATION", smallStyle);
            if (GUI.Button(new Rect(rightX + 12, 58, 146, 30), paused ? "Resume" : "Pause"))
                paused = !paused;
            if (GUI.Button(new Rect(rightX + 12, 94, 146, 30), "Reset speed"))
                simulationSpeed = 1f;

            GUI.Label(new Rect(20, Screen.height / scale - 38, 520, 24),
                "Drag to orbit  •  Scroll / pinch to zoom  •  Select a planet for facts", smallStyle);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.white;
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            bodyStyle.normal.textColor = new Color(0.85f, 0.90f, 0.98f);
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            smallStyle.normal.textColor = new Color(0.60f, 0.78f, 1f);
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = Texture2D.grayTexture;
        }

        private void OnDestroy()
        {
            if (lineMaterial != null) Destroy(lineMaterial);
            if (starMaterial != null) Destroy(starMaterial);
        }
    }
}
