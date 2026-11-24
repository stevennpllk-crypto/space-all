using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Builds lightweight deterministic color maps for the procedural planet spheres.
    /// These are illustrative procedural maps, not scientific elevation or satellite datasets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlanetSurfaceTextures : MonoBehaviour
    {
        [SerializeField, Range(64, 512)] private int textureWidth = 256;
        [SerializeField, Range(32, 256)] private int textureHeight = 128;
        [SerializeField, Range(0f, 1f)] private float detailStrength = 0.32f;

        private readonly List<Texture2D> ownedTextures = new List<Texture2D>();

        private void Start()
        {
            string[] names = { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };
            foreach (string bodyName in names)
            {
                Transform body = FindBody(transform, bodyName);
                if (body == null) continue;
                Renderer renderer = body.GetComponent<Renderer>();
                if (renderer == null) continue;

                Texture2D map = BuildMap(bodyName);
                ownedTextures.Add(map);

                // Accessing renderer.material creates a per-planet instance.
                Material material = renderer.material;
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", map);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", map);
                material.mainTexture = map;
            }
        }

        private Texture2D BuildMap(string bodyName)
        {
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, true);
            texture.name = bodyName + " Procedural Color Map";
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            Color low, high, accent;
            GetPalette(bodyName, out low, out high, out accent);
            Color[] pixels = new Color[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                float v = y / (float)textureHeight;
                float latitude = Mathf.Abs(v - 0.5f) * 2f;
                for (int x = 0; x < textureWidth; x++)
                {
                    float u = x / (float)textureWidth;
                    float broad = Fractal(u * 5.5f, v * 3.2f, 4);
                    float fine = Fractal(u * 29f, v * 17f, 3);
                    float n = Mathf.Clamp01(broad * 0.72f + fine * detailStrength);
                    Color color;

                    if (IsGasGiant(bodyName))
                    {
                        float bands = Mathf.Sin((v * 34f + broad * 2.8f) * Mathf.PI);
                        float bandMix = Mathf.Clamp01(0.5f + bands * 0.24f + (fine - 0.5f) * 0.25f);
                        color = Color.Lerp(low, high, bandMix);
                        if (bodyName == "Jupiter")
                        {
                            float storm = Mathf.Exp(-Mathf.Pow((u - 0.73f) * 13f, 2f) - Mathf.Pow((v - 0.43f) * 34f, 2f));
                            color = Color.Lerp(color, accent, storm * 0.85f);
                        }
                    }
                    else if (bodyName == "Earth")
                    {
                        float landNoise = Fractal(u * 7.5f + 2f, v * 5f + 1f, 5);
                        bool land = landNoise > 0.53f;
                        color = land ? Color.Lerp(high, accent, n * 0.65f) : Color.Lerp(low, high, n * 0.38f);
                        if (latitude > 0.84f) color = Color.Lerp(color, Color.white, (latitude - 0.84f) * 4.2f);
                    }
                    else
                    {
                        color = Color.Lerp(low, high, n);
                        float craterNoise = Fractal(u * 48f, v * 25f, 2);
                        color = Color.Lerp(color, accent, Mathf.SmoothStep(0.70f, 0.94f, craterNoise) * 0.42f);
                    }

                    pixels[y * textureWidth + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private float Fractal(float x, float y, int octaves)
        {
            float sum = 0f;
            float amplitude = 0.5f;
            float total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Mathf.PerlinNoise(x, y) * amplitude;
                total += amplitude;
                x *= 2.03f;
                y *= 2.03f;
                amplitude *= 0.5f;
            }
            return total > 0f ? sum / total : 0.5f;
        }

        private static bool IsGasGiant(string name)
        {
            return name == "Jupiter" || name == "Saturn" || name == "Uranus" || name == "Neptune";
        }

        private static void GetPalette(string name, out Color low, out Color high, out Color accent)
        {
            low = new Color(0.16f, 0.15f, 0.14f);
            high = new Color(0.62f, 0.59f, 0.54f);
            accent = new Color(0.82f, 0.78f, 0.70f);

            switch (name)
            {
                case "Mercury":
                    low = new Color(0.18f, 0.17f, 0.16f); high = new Color(0.58f, 0.55f, 0.50f); accent = new Color(0.78f, 0.74f, 0.67f); break;
                case "Venus":
                    low = new Color(0.40f, 0.23f, 0.10f); high = new Color(0.91f, 0.68f, 0.35f); accent = new Color(1f, 0.83f, 0.55f); break;
                case "Earth":
                    low = new Color(0.015f, 0.08f, 0.27f); high = new Color(0.08f, 0.42f, 0.68f); accent = new Color(0.19f, 0.38f, 0.16f); break;
                case "Mars":
                    low = new Color(0.28f, 0.08f, 0.045f); high = new Color(0.78f, 0.31f, 0.16f); accent = new Color(0.96f, 0.68f, 0.47f); break;
                case "Jupiter":
                    low = new Color(0.40f, 0.24f, 0.15f); high = new Color(0.88f, 0.72f, 0.52f); accent = new Color(0.72f, 0.25f, 0.14f); break;
                case "Saturn":
                    low = new Color(0.48f, 0.38f, 0.23f); high = new Color(0.91f, 0.82f, 0.62f); accent = new Color(0.73f, 0.59f, 0.39f); break;
                case "Uranus":
                    low = new Color(0.18f, 0.48f, 0.56f); high = new Color(0.52f, 0.84f, 0.88f); accent = new Color(0.72f, 0.93f, 0.94f); break;
                case "Neptune":
                    low = new Color(0.035f, 0.12f, 0.42f); high = new Color(0.16f, 0.39f, 0.91f); accent = new Color(0.40f, 0.64f, 1f); break;
            }
        }

        private static Transform FindBody(Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name) return child;
            }
            return null;
        }

        private void OnDestroy()
        {
            foreach (Texture2D texture in ownedTextures)
                if (texture != null) Destroy(texture);
            ownedTextures.Clear();
        }
    }
}
