using System;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Adds a lightweight, illustrative set of major natural satellites.
    /// Orbital distances and displayed radii are compressed for readability;
    /// periods are approximate sidereal values in Earth days.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NaturalSatelliteSystem : MonoBehaviour
    {
        [Serializable]
        private sealed class Satellite
        {
            public string parent;
            public string name;
            public float radius;
            public float orbit;
            public float period;
            public Color color;

            public Satellite(string parent, string name, float radius, float orbit, float period, Color color)
            {
                this.parent = parent;
                this.name = name;
                this.radius = radius;
                this.orbit = orbit;
                this.period = period;
                this.color = color;
            }
        }

        private sealed class SatelliteView
        {
            public Transform pivot;
            public float period;
        }

        private static readonly Satellite[] Catalog =
        {
            new Satellite("Earth", "Moon", 0.273f, 1.15f, 27.3217f, new Color(0.68f, 0.69f, 0.72f)),
            new Satellite("Mars", "Phobos", 0.017f, 0.62f, 0.3189f, new Color(0.48f, 0.43f, 0.39f)),
            new Satellite("Mars", "Deimos", 0.009f, 0.94f, 1.263f, new Color(0.62f, 0.56f, 0.49f)),
            new Satellite("Jupiter", "Io", 0.286f, 0.78f, 1.7691f, new Color(0.91f, 0.76f, 0.43f)),
            new Satellite("Jupiter", "Europa", 0.245f, 1.02f, 3.5512f, new Color(0.78f, 0.73f, 0.62f)),
            new Satellite("Jupiter", "Ganymede", 0.413f, 1.34f, 7.1546f, new Color(0.55f, 0.52f, 0.48f)),
            new Satellite("Jupiter", "Callisto", 0.378f, 1.70f, 16.689f, new Color(0.39f, 0.36f, 0.33f)),
            new Satellite("Saturn", "Titan", 0.404f, 1.42f, 15.945f, new Color(0.82f, 0.59f, 0.31f))
        };

        [SerializeField] private bool animate = true;
        [SerializeField] private bool showOrbitPaths = true;
        [SerializeField, Min(0f)] private float simulationSpeed = 1f;

        private readonly List<SatelliteView> views = new List<SatelliteView>();
        private Material satelliteMaterial;
        private Material orbitMaterial;

        private void Start()
        {
            satelliteMaterial = CreateMaterial();
            orbitMaterial = new Material(Shader.Find("Sprites/Default"));

            foreach (Satellite data in Catalog)
            {
                Transform parent = FindDeep(transform, data.parent);
                if (parent == null) continue;

                GameObject pivotObject = new GameObject(data.name + " Orbit Pivot");
                pivotObject.transform.SetParent(parent, false);

                if (showOrbitPaths)
                    CreateOrbit(pivotObject.transform, data.orbit, data.name + " Orbit");

                GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                moon.name = data.name;
                moon.transform.SetParent(pivotObject.transform, false);
                float visualRadius = Mathf.Max(0.035f, data.radius * 0.42f);
                moon.transform.localScale = Vector3.one * visualRadius * 2f;
                moon.transform.localPosition = Vector3.right * data.orbit;
                moon.GetComponent<Renderer>().material = new Material(satelliteMaterial);
                moon.GetComponent<Renderer>().material.color = data.color;
                Collider collider = moon.GetComponent<Collider>();
                if (collider != null) Destroy(collider);

                TextMesh label = new GameObject(data.name + " Label").AddComponent<TextMesh>();
                label.transform.SetParent(moon.transform, false);
                label.transform.localPosition = Vector3.up * (visualRadius + 0.12f);
                label.text = data.name;
                label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center;
                label.characterSize = 0.10f;
                label.fontSize = 36;
                label.color = new Color(0.82f, 0.90f, 1f);

                views.Add(new SatelliteView { pivot = pivotObject.transform, period = data.period });
            }
        }

        private void Update()
        {
            if (!animate) return;
            float simulatedDays = Time.deltaTime * simulationSpeed * 12f;
            foreach (SatelliteView view in views)
            {
                float period = Mathf.Max(0.01f, view.period);
                view.pivot.Rotate(Vector3.up, 360f * simulatedDays / period, Space.Self);
            }
        }

        private void CreateOrbit(Transform parent, float radius, string objectName)
        {
            GameObject orbitObject = new GameObject(objectName);
            orbitObject.transform.SetParent(parent, false);
            LineRenderer line = orbitObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 96;
            line.widthMultiplier = 0.012f;
            line.material = orbitMaterial;
            line.startColor = new Color(0.52f, 0.66f, 0.84f, 0.48f);
            line.endColor = line.startColor;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root.name == objectName) return root;
            foreach (Transform child in root)
            {
                Transform found = FindDeep(child, objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return new Material(shader);
        }

        private void OnDestroy()
        {
            if (satelliteMaterial != null) Destroy(satelliteMaterial);
            if (orbitMaterial != null) Destroy(orbitMaterial);
        }
    }
}
