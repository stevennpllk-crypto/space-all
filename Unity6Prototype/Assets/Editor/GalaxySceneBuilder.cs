#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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

            var lightObject = new GameObject("Solar Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
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
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.4f);
            }
            return material;
        }

        private static void CreateBody(string bodyName, Vector3 position, float radius, Color color, bool emissive, float spin, float gravity, float gravityRadius, float influenceRadius)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = bodyName;
            body.transform.position = position;
            body.transform.localScale = Vector3.one * radius * 2;
            body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color, emissive);
            body.AddComponent<CelestialBody>().Configure(bodyName, spin);\n            body.AddComponent<GravitySource>().Configure(gravity, gravityRadius, influenceRadius);
        }

        private static void CreateStars()
        {
            var starObject = new GameObject("Starfield");
            var particles = starObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 650;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(.35f, 1.1f);

            var emission = particles.emission;
            emission.enabled = false;

            var renderer = starObject.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                            Shader.Find("Particles/Standard Unlit");
            renderer.sharedMaterial = new Material(shader);

            var random = new System.Random(2026);
            var starParticles = new ParticleSystem.Particle[650];
            for (int i = 0; i < starParticles.Length; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                float distance = Mathf.Lerp(250f, 900f, (float)random.NextDouble());
                starParticles[i] = new ParticleSystem.Particle
                {
                    position = direction * distance,
                    startColor = Color.white,
                    startSize = Mathf.Lerp(.35f, 1.1f, (float)random.NextDouble()),
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

            Slider throttle = CreateThrottle(canvas.transform);
            Button cameraButton = CreateButton(canvas.transform, "CAMERA", new Vector2(-170, 95));
            Button brakeButton = CreateButton(canvas.transform, "BRAKE", new Vector2(-170, 35));

            var hud = canvasObject.AddComponent<FlightHUD>();
            hud.Bind(ship.GetComponent<SpacecraftController>(), cameraRig,
                moveStick, lookStick, speedText, modeText, throttle);
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