using UnityEngine;
using UnityEngine.Rendering;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Generates a subtle all-sky nebula texture on an inward-visible sphere.
    /// This is a stylized procedural backdrop, not a real astronomical image.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeepSpaceNebulaBackdrop : MonoBehaviour
    {
        [SerializeField, Range(128, 512)] private int textureWidth = 512;
        [SerializeField, Range(64, 256)] private int textureHeight = 256;
        [SerializeField, Range(0f, 0.35f)] private float maximumOpacity = 0.13f;
        [SerializeField, Min(100f)] private float backdropRadius = 245f;

        private Texture2D nebulaTexture;
        private Material nebulaMaterial;

        private void Start()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return;

            nebulaTexture = BuildNebulaTexture();
            GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Procedural Deep Space Nebula Dome";
            dome.transform.SetParent(transform, false);
            dome.transform.localPosition = Vector3.zero;
            dome.transform.localScale = Vector3.one * backdropRadius * 2f;

            Collider collider = dome.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            nebulaMaterial = new Material(shader);
            nebulaMaterial.name = "Deep Space Nebula Transparent Material";
            if (nebulaMaterial.HasProperty("_MainTex")) nebulaMaterial.SetTexture("_MainTex", nebulaTexture);
            if (nebulaMaterial.HasProperty("_BaseMap")) nebulaMaterial.SetTexture("_BaseMap", nebulaTexture);
            if (nebulaMaterial.HasProperty("_Color")) nebulaMaterial.SetColor("_Color", Color.white);
            if (nebulaMaterial.HasProperty("_BaseColor")) nebulaMaterial.SetColor("_BaseColor", Color.white);
            if (nebulaMaterial.HasProperty("_Surface")) nebulaMaterial.SetFloat("_Surface", 1f);
            if (nebulaMaterial.HasProperty("_Blend")) nebulaMaterial.SetFloat("_Blend", 0f);
            if (nebulaMaterial.HasProperty("_SrcBlend")) nebulaMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (nebulaMaterial.HasProperty("_DstBlend")) nebulaMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (nebulaMaterial.HasProperty("_ZWrite")) nebulaMaterial.SetInt("_ZWrite", 0);
            if (nebulaMaterial.HasProperty("_Cull")) nebulaMaterial.SetInt("_Cull", (int)CullMode.Off);
            nebulaMaterial.renderQueue = 2990;
            nebulaMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            Renderer renderer = dome.GetComponent<Renderer>();
            renderer.sharedMaterial = nebulaMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private Texture2D BuildNebulaTexture()
        {
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false, true);
            texture.name = "Procedural Violet Blue Nebula";
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                float v = y / (float)textureHeight;
                for (int x = 0; x < textureWidth; x++)
                {
                    float u = x / (float)textureWidth;
                    float broad = Fractal(u * 3.4f + 1.7f, v * 2.8f + 4.1f, 4);
                    float wisps = Fractal(u * 12.5f + broad * 2.2f, v * 9.5f - broad, 3);
                    float filaments = Mathf.PerlinNoise(u * 38f + 9f, v * 23f + 2f);
                    float cloud = Mathf.SmoothStep(0.48f, 0.82f, broad * 0.68f + wisps * 0.25f + filaments * 0.07f);
                    float edgeFade = Mathf.SmoothStep(0.02f, 0.18f, v) * (1f - Mathf.SmoothStep(0.82f, 0.98f, v));
                    float alpha = cloud * edgeFade * maximumOpacity;

                    Color violet = new Color(0.34f, 0.12f, 0.68f);
                    Color blue = new Color(0.06f, 0.34f, 0.72f);
                    Color rose = new Color(0.62f, 0.20f, 0.48f);
                    Color hue = Color.Lerp(violet, blue, Mathf.Clamp01(wisps * 1.25f));
                    hue = Color.Lerp(hue, rose, Mathf.Clamp01((filaments - 0.62f) * 1.7f));
                    pixels[y * textureWidth + x] = new Color(hue.r, hue.g, hue.b, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static float Fractal(float x, float y, int octaves)
        {
            float sum = 0f;
            float amplitude = 0.5f;
            float total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Mathf.PerlinNoise(x, y) * amplitude;
                total += amplitude;
                x *= 2.01f;
                y *= 2.01f;
                amplitude *= 0.5f;
            }
            return total > 0f ? sum / total : 0.5f;
        }

        private void OnDestroy()
        {
            if (nebulaMaterial != null) Destroy(nebulaMaterial);
            if (nebulaTexture != null) Destroy(nebulaTexture);
        }
    }
}
