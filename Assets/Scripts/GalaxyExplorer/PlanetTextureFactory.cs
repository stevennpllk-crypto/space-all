using UnityEngine;

/// <summary>Generates lightweight deterministic surface maps at runtime.</summary>
public static class PlanetTextureFactory
{
    public static Texture2D Create(Color baseColor, int seed, bool gasGiant)
    {
        const int width = 256;
        const int height = 128;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, true);
        texture.name = "Procedural Planet Surface " + seed;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        float offsetX = seed * 17.13f;
        float offsetY = seed * 7.91f;
        Color dark = Color.Lerp(baseColor, Color.black, 0.42f);
        Color light = Color.Lerp(baseColor, Color.white, 0.32f);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float latitude = (float)y / (height - 1);
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width;
                float n1 = Mathf.PerlinNoise(u * 7f + offsetX, latitude * 5f + offsetY);
                float n2 = Mathf.PerlinNoise(u * 19f + offsetY, latitude * 13f + offsetX);
                float detail = n1 * 0.72f + n2 * 0.28f;

                if (gasGiant)
                {
                    float bands = Mathf.Sin((latitude * 18f + n1 * 1.8f) * Mathf.PI);
                    detail = Mathf.Clamp01(0.5f + bands * 0.22f + (n2 - 0.5f) * 0.3f);
                }

                float polarFade = Mathf.Abs(latitude - 0.5f) * 2f;
                Color color = Color.Lerp(dark, light, Mathf.Clamp01(detail));
                if (!gasGiant && polarFade > 0.82f)
                    color = Color.Lerp(color, new Color(0.82f, 0.88f, 0.94f), (polarFade - 0.82f) * 0.65f);
                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(true, true);
        return texture;
    }
}
