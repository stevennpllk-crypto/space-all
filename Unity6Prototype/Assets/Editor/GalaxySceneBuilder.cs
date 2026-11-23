#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GalaxyExplorer.Spacecraft;
using GalaxyExplorer.Cameras;
using GalaxyExplorer.Input;
using GalaxyExplorer.UI;
using GalaxyExplorer.World;

namespace GalaxyExplorer.Editor
{
    public static class GalaxySceneBuilder
    {
        [MenuItem("Galaxy Explorer/Build Prototype 0.1 Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(.16f, .19f, .28f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.reflectionIntensity = .7f;
            CreateCinematicVolume();

            var lightObject = new GameObject("Solar Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .9f;
            light.shadowBias = .035f;
            light.shadowNormalBias = .4f;
            lightObject.transform.rotation = Quaternion.Euler(35, -35, 0);

            CreateBody("SUN", Vector3.zero, 8, new Color(1, .55f, .12f), true, .15f, 0f, 8f, 0f);
            CreateBody("EARTH", new Vector3(0, 0, 42), 3.2f, new Color(.08f, .32f, .85f), false, 5, 9.81f, 3.2f, 32f);
            CreateBody("MOON", new Vector3(7, 1, 47), .9f, new Color(.48f, .5f, .54f), false, 1, 1.62f, .9f, 10f);
            CreateBody("MARS", new Vector3(0, 0, 76), 2.2f, new Color(.72f, .25f, .12f), false, 4, 3.71f, 2.2f, 22f);

            CreateStars();
            GameObject ship = CreateShip();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 68;
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 2500;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            cameraObject.AddComponent<AudioListener>();

            var cameraRig = cameraObject.AddComponent<CameraRigController>();
            cameraRig.SetTarget(ship.transform);
            cameraObject.transform.position = ship.transform.position + new Vector3(0, 3, -9);
            cameraObject.transform.LookAt(ship.transform);

            CreateCanvas(ship, cameraRig);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity", true)
            };

            Selection.activeGameObject = ship;
            Debug.Log("GALAXY EXPLORER Prototype 0.1 scene generated.");
        }

        private static Material CreateMaterial(Color color, bool emissive = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", emissive ? 0f : .12f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", emissive ? .45f : .82f);
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 3.2f);
            }
            return material;
        }

        private static void CreateCinematicVolume()
        {
            var volumeObject = new GameObject("Global Cinematic Post Processing");
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            Bloom bloom = volume.profile.Add<Bloom>(true);
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(.72f);
            bloom.scatter.Override(.68f);

            Tonemapping tone = volume.profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            ColorAdjustments color = volume.profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(.12f);
            color.contrast.Override(12f);
            color.saturation.Override(8f);

            Vignette vignette = volume.profile.Add<Vignette>(true);
            vignette.intensity.Override(.18f);
            vignette.smoothness.Override(.72f);
        }

        private static Mesh CreateHighDetailSphere(int longitudeSegments = 96, int latitudeSegments = 64)
        {
            var mesh = new Mesh { name = "High Detail Planet Sphere" };
            var vertices = new Vector3[(longitudeSegments + 1) * (latitudeSegments + 1)];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            int index = 0;

            for (int y = 0; y <= latitudeSegments; y++)
            {
                float v = (float)y / latitudeSegments;
                float latitude = v * Mathf.PI;
                for (int x = 0; x <= longitudeSegments; x++)
                {
                    float u = (float)x / longitudeSegments;
                    float longitude = u * Mathf.PI * 2f;
                    Vector3 normal = new Vector3(
                        -Mathf.Cos(longitude) * Mathf.Sin(latitude),
                        Mathf.Cos(latitude),
                        Mathf.Sin(longitude) * Mathf.Sin(latitude));
                    vertices[index] = normal;
                    normals[index] = normal;
                    uv[index] = new Vector2(u, v);
                    index++;
                }
            }

            int[] triangles = new int[longitudeSegments * latitudeSegments * 6];
            int t = 0;
            for (int y = 0; y < latitudeSegments; y++)
            {
                for (int x = 0; x < longitudeSegments; x++)
                {
                    int a = y * (longitudeSegments + 1) + x;
                    int b = a + longitudeSegments + 1;
                    triangles[t++] = a;
                    triangles[t++] = b;
                    triangles[t++] = a + 1;
                    triangles[t++] = a + 1;
                    triangles[t++] = b;
                    triangles[t++] = b + 1;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateAtmosphere(Transform parent, float radius, Color tint)
        {
            var atmosphere = new GameObject("Atmosphere Shell");
            atmosphere.transform.SetParent(parent, false);
            atmosphere.transform.localScale = Vector3.one * radius * 2.045f;
            var filter = atmosphere.AddComponent<MeshFilter>();
            filter.sharedMesh = CreateHighDetailSphere(64, 40);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return;
            var material = new Material(shader);
            material.SetColor("_BaseColor", new Color(tint.r, tint.g, tint.b, .16f));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = 3000;
            atmosphere.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateBody(string bodyName, Vector3 position, float radius, Color color, bool emissive, float spin, float gravity, float gravityRadius, float influenceRadius)
        {
            var body = new GameObject(bodyName);
            body.transform.position = position;
            body.transform.localScale = Vector3.one * radius * 2f;

            Mesh mesh = CreateHighDetailSphere();
            body.AddComponent<MeshFilter>().sharedMesh = mesh;
            body.AddComponent<MeshRenderer>().sharedMaterial = CreateMaterial(color, emissive);
            var collider = body.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            body.AddComponent<CelestialBody>().Configure(bodyName, spin);
            body.AddComponent<GravitySource>().Configure(gravity, gravityRadius, influenceRadius);

            if (bodyName == "EARTH")
                CreateAtmosphere(body.transform, radius, new Color(.18f, .52f, 1f));
            else if (bodyName == "MARS")
                CreateAtmosphere(body.transform, radius, new Color(1f, .32f, .12f));
        }

        private static void CreateStars()
        {
            var starObject = new GameObject("Starfield");
            var particles = starObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 2200;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(.18f, .72f);

            var emission = particles.emission;
            emission.enabled = false;

            var renderer = starObject.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                            Shader.Find("Particles/Standard Unlit");
            renderer.sharedMaterial = new Material(shader);

            var random = new System.Random(2026);
            var starParticles = new ParticleSystem.Particle[2200];
            for (int i = 0; i < starParticles.Length; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                float distance = Mathf.Lerp(250f, 900f, (float)random.NextDouble());
                starParticles[i] = new ParticleSystem.Particle
                {
                    position = direction * distance,
                    startColor = Color.white,
                    startSize = Mathf.Lerp(.18f, .72f, (float)random.NextDouble()),
                    startLifetime = 100000f,
                    remainingLifetime = 100000f
                };
            }

            particles.SetParticles(starParticles, starParticles.Length);
            particles.Play();
        }

        private static GameObject CreateShip()
        {
            var root = new GameObject("EXPLORER - Player Ship");
            root.transform.position = new Vector3(0, 0, -8);

            GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hull.name = "Hull";
            hull.transform.SetParent(root.transform, false);
            hull.transform.localRotation = Quaternion.Euler(90, 0, 0);
            hull.transform.localScale = new Vector3(1.25f, 2.7f, 1.25f);
            hull.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(.22f, .31f, .42f));

            GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cockpit.name = "Cockpit";
            cockpit.transform.SetParent(root.transform, false);
            cockpit.transform.localPosition = new Vector3(0, .15f, 1.4f);
            cockpit.transform.localScale = new Vector3(.85f, .6f, .8f);
            cockpit.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(.05f, .65f, .82f), true);

            foreach (Collider childCollider in root.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(childCollider);

            var collider = root.AddComponent<CapsuleCollider>();
            collider.direction = 2;
            collider.height = 4.5f;
            collider.radius = .8f;
            root.AddComponent<Rigidbody>();
            root.AddComponent<SpacecraftController>();
            root.AddComponent<LandingAssist>();
            return root;
        }

        private static void CreateCanvas(GameObject ship, CameraRigController cameraRig)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasObject = new GameObject("Flight HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            VirtualJoystick moveStick = CreateStick(canvas.transform, "MOVE / ROLL",
                new Vector2(170, 170), new Vector2(0, 0));
            VirtualJoystick lookStick = CreateStick(canvas.transform, "LOOK / ROTATE",
                new Vector2(-170, 170), new Vector2(1, 0));

            Text speedText = CreateLabel(canvas.transform, "VELOCITY 0.0 m/s",
                new Vector2(0, -48), new Vector2(.5f, 1), 28);
            Text modeText = CreateLabel(canvas.transform, "CAM / EXTERNAL",
                new Vector2(0, -88), new Vector2(.5f, 1), 20);
            Text statusText = CreateLabel(canvas.transform, "CRUISE / DEEP SPACE",
                new Vector2(0, -126), new Vector2(.5f, 1), 16);

            Slider throttle = CreateThrottle(canvas.transform);
            Button cameraButton = CreateButton(canvas.transform, "CAMERA", new Vector2(-170, 95));
            Button brakeButton = CreateButton(canvas.transform, "BRAKE", new Vector2(-170, 35));

            var hud = canvasObject.AddComponent<FlightHUD>();
            hud.Bind(ship.GetComponent<SpacecraftController>(), cameraRig,
                moveStick, lookStick, speedText, modeText, statusText, throttle);
            cameraButton.onClick.AddListener(hud.ToggleCamera);

            var trigger = brakeButton.gameObject.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ => hud.SetBrake(true));
            trigger.triggers.Add(down);
            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => hud.SetBrake(false));
            trigger.triggers.Add(up);
        }

        private static VirtualJoystick CreateStick(Transform parent, string label, Vector2 position, Vector2 anchor)
        {
            var stickObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(VirtualJoystick));
            stickObject.transform.SetParent(parent, false);
            var rect = (RectTransform)stickObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(210, 210);
            stickObject.GetComponent<Image>().color = new Color(.05f, .55f, .75f, .22f);

            var handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObject.transform.SetParent(stickObject.transform, false);
            var handle = (RectTransform)handleObject.transform;
            handle.sizeDelta = new Vector2(76, 76);
            handleObject.GetComponent<Image>().color = new Color(.2f, .85f, 1, .72f);

            VirtualJoystick joystick = stickObject.GetComponent<VirtualJoystick>();
            joystick.ConfigureHandle(handle);
            CreateLabel(stickObject.transform, label, new Vector2(0, -118), new Vector2(.5f, .5f), 13);
            return joystick;
        }

        private static Text CreateLabel(Transform parent, string value, Vector2 position, Vector2 anchor, int size)
        {
            var labelObject = new GameObject(value, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(650, 50);

            Text text = labelObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.color = new Color(.55f, .9f, 1);
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private static Slider CreateThrottle(Transform parent)
        {
            var sliderObject = new GameObject("THROTTLE", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            var rect = (RectTransform)sliderObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
            rect.anchoredPosition = new Vector2(0, 48);
            rect.sizeDelta = new Vector2(330, 24);

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderObject.transform, false);
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            background.GetComponent<Image>().color = new Color(.04f, .16f, .23f, .85f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            var fillRect = (RectTransform)fillArea.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(5, 5);
            fillRect.offsetMax = new Vector2(-5, -5);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillTransform = (RectTransform)fill.transform;
            fillTransform.anchorMin = Vector2.zero;
            fillTransform.anchorMax = Vector2.one;
            fillTransform.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(.1f, .75f, .95f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObject.transform, false);
            var handleAreaRect = (RectTransform)handleArea.transform;
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = (RectTransform)handle.transform;
            handleRect.sizeDelta = new Vector2(28, 34);

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fillTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0;
            return slider;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position)
        {
            var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(150, 48);
            buttonObject.GetComponent<Image>().color = new Color(.04f, .28f, .4f, .85f);

            Text text = CreateLabel(buttonObject.transform, label, Vector2.zero, new Vector2(.5f, .5f), 17);
            text.color = Color.white;
            return buttonObject.GetComponent<Button>();
        }
    }
}
#endif