using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Adds subtle transparent atmospheric and cloud shells to the procedural planets.
    /// This is a stylized real-time effect, not a physically based atmospheric scattering model.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlanetAtmosphereEffects : MonoBehaviour
    {
        private sealed class EffectDefinition
        {
            public string body;
            public Color atmosphere;
            public Color clouds;
            public bool hasClouds;

            public EffectDefinition(string body, Color atmosphere, Color clouds, bool hasClouds)
            {
                this.body = body;
                this.atmosphere = atmosphere;
                this.clouds = clouds;
                this.hasClouds = hasClouds;
            }
        }

        private static readonly EffectDefinition[] Catalog =
        {
            new EffectDefinition("Earth", new Color(0.16f, 0.48f, 1f, 0.15f), new Color(0.92f, 0.96f, 1f, 0.13f), true),
            new EffectDefinition("Venus", new Color(1f, 0.72f, 0.32f, 0.20f), new Color(1f, 0.84f, 0.55f, 0.17f), true),
            new EffectDefinition("Mars", new Color(0.92f, 0.38f, 0.20f, 0.09f), Color.clear, false),
            new EffectDefinition("Jupiter", new Color(0.90f, 0.70f, 0.46f, 0.12f), new Color(0.92f, 0.77f, 0.59f, 0.10f), true),
            new EffectDefinition("Saturn", new Color(0.94f, 0.79f, 0.52f, 0.13f), new Color(0.95f, 0.86f, 0.67f, 0.09f), true),
            new EffectDefinition("Uranus", new Color(0.35f, 0.85f, 0.95f, 0.16f), Color.clear, false),
            new EffectDefinition("Neptune", new Color(0.22f, 0.48f, 1f, 0.17f), Color.clear, false)
        };

        [SerializeField, Range(1.01f, 1.12f)] private float atmosphereScale = 1.045f;
        [SerializeField, Range(1.005f, 1.08f)] private float cloudScale = 1.018f;
        [SerializeField] private bool animateClouds = true;
        [SerializeField, Min(0f)] private float cloudRotationSpeed = 2.2f;

        private readonly List<Transform> cloudLayers = new List<Transform>();
        private readonly List<Material> ownedMaterials = new List<Material>();

        private void Start()
        {
            foreach (EffectDefinition definition in Catalog)
            {
                Transform planet = FindDirectChild(transform, definition.body);
                if (planet == null) continue;

                CreateShell(planet, definition.body + " Atmosphere", atmosphereScale, definition.atmosphere, false);

                if (definition.hasClouds)
                {
                    Transform clouds = CreateShell(planet, definition.body + " Cloud Layer",
                        cloudScale, definition.clouds, true);
                    if (clouds != null) cloudLayers.Add(clouds);
                }
            }
        }

        private void Update()
        {
            if (!animateClouds) return;
            foreach (Transform clouds in cloudLayers)
                if (clouds != null)
                    clouds.Rotate(Vector3.up, cloudRotationSpeed * Time.deltaTime, Space.Self);
        }

        private Transform CreateShell(Transform planet, string objectName, float scale, Color tint, bool clouds)
        {
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shell.name = objectName;
            shell.transform.SetParent(planet, false);
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localRotation = Quaternion.identity;
            shell.transform.localScale = Vector3.one * scale;

            Collider collider = shell.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Destroy(shell);
                return null;
            }

            Material material = new Material(shader);
            ownedMaterials.Add(material);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            Renderer renderer = shell.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return shell.transform;
        }

        private static Transform FindDirectChild(Transform root, string objectName)
        {
            foreach (Transform child in root)
                if (child.name == objectName)
                    return child;
            return null;
        }

        private void OnDestroy()
        {
            foreach (Material material in ownedMaterials)
                if (material != null)
                    Destroy(material);
            ownedMaterials.Clear();
        }
    }
}
