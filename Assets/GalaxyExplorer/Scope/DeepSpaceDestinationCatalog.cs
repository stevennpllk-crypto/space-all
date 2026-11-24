using System;
using UnityEngine;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Lightweight selectable deep-space destinations. Coordinates and apparent sizes
    /// are illustrative scene layout values, not astronomical scale distances.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeepSpaceDestinationCatalog : MonoBehaviour
    {
        [Serializable]
        private sealed class Destination
        {
            public string name;
            public string kind;
            public string fact;
            public Vector3 position;
            public Color color;
            public float radius;

            public Destination(string name, string kind, string fact, Vector3 position, Color color, float radius)
            {
                this.name = name;
                this.kind = kind;
                this.fact = fact;
                this.position = position;
                this.color = color;
                this.radius = radius;
            }
        }

        private static readonly Destination[] Catalog =
        {
            new Destination("Orion Nebula", "Star-forming nebula",
                "A nearby stellar nursery where gas and dust form new stars.",
                new Vector3(145f, 42f, 150f), new Color(0.35f, 0.52f, 1f), 7f),
            new Destination("Crab Nebula", "Supernova remnant",
                "The expanding remains of a supernova recorded by astronomers in 1054.",
                new Vector3(-155f, 58f, 132f), new Color(0.35f, 0.78f, 0.86f), 6f),
            new Destination("Andromeda Galaxy", "Spiral galaxy",
                "The large spiral galaxy nearest to the Milky Way and a major Local Group member.",
                new Vector3(210f, -34f, -92f), new Color(0.72f, 0.58f, 1f), 12f),
            new Destination("Sombrero Galaxy", "Unbarred lenticular galaxy",
                "A bright galaxy known for its prominent central bulge and dark dust lane.",
                new Vector3(-205f, -72f, -105f), new Color(1f, 0.72f, 0.46f), 9f)
        };

        private Transform[] destinationTransforms;
        private int selectedIndex = -1;
        private GUIStyle headingStyle;
        private GUIStyle detailStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;

        private void Start()
        {
            destinationTransforms = new Transform[Catalog.Length];
            for (int i = 0; i < Catalog.Length; i++)
                CreateDestination(Catalog[i], i);
        }

        private void CreateDestination(Destination entry, int index)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Deep Space Destination - " + entry.name;
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = entry.position;
            marker.transform.localScale = Vector3.one * entry.radius * 2f;

            Renderer renderer = marker.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            Material material = new Material(shader);
            material.name = entry.name + " Marker Material";
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", entry.color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", entry.color);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", entry.color * 1.4f);
            }
            renderer.sharedMaterial = material;

            Collider collider = marker.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            GameObject labelObject = new GameObject(entry.name + " Label");
            labelObject.transform.SetParent(marker.transform, false);
            labelObject.transform.localPosition = Vector3.up * (entry.radius + 2.2f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = entry.name;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.22f;
            label.fontSize = 48;
            label.color = new Color(0.86f, 0.91f, 1f);
            destinationTransforms[index] = marker.transform;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.width / 1100f, 0.72f, 1.1f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float x = Screen.width / scale - 230f;
            GUI.Box(new Rect(x, 398f, 210f, 194f), GUIContent.none, panelStyle);
            GUI.Label(new Rect(x + 12f, 408f, 190f, 24f), "DEEP SPACE", headingStyle);

            for (int i = 0; i < Catalog.Length; i++)
            {
                int row = i;
                if (GUI.Button(new Rect(x + 12f, 438f + i * 34f, 186f, 29f), Catalog[i].name, buttonStyle))
                    FocusDestination(row);
            }

            if (selectedIndex >= 0)
            {
                Destination entry = Catalog[selectedIndex];
                GUI.Box(new Rect(18f, 455f, 310f, 116f), GUIContent.none, panelStyle);
                GUI.Label(new Rect(32f, 465f, 280f, 24f), entry.name + " • " + entry.kind, headingStyle);
                GUI.Label(new Rect(32f, 494f, 280f, 68f), entry.fact, detailStyle);
            }
            GUI.matrix = previous;
        }

        private void FocusDestination(int index)
        {
            if (index < 0 || index >= Catalog.Length || destinationTransforms == null) return;
            selectedIndex = index;
            Camera camera = Camera.main;
            if (camera == null) return;
            SolarSystemScopeCamera controller = camera.GetComponent<SolarSystemScopeCamera>();
            if (controller != null) controller.Focus(destinationTransforms[index]);
            else camera.transform.LookAt(destinationTransforms[index]);
        }

        private void EnsureStyles()
        {
            if (headingStyle != null) return;
            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            headingStyle.normal.textColor = Color.white;
            detailStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            detailStyle.normal.textColor = new Color(0.85f, 0.90f, 0.98f);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11 };
            panelStyle = new GUIStyle(GUI.skin.box);
        }

        private void OnDestroy()
        {
            if (destinationTransforms == null) return;
            for (int i = 0; i < destinationTransforms.Length; i++)
            {
                if (destinationTransforms[i] == null) continue;
                Renderer renderer = destinationTransforms[i].GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                    Destroy(renderer.sharedMaterial);
            }
        }
    }
}
