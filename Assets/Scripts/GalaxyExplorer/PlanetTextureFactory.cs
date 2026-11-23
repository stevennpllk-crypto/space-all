using UnityEngine;

/// <summary>Builds seamless, multi-scale procedural planet surface maps.</summary>
public static class PlanetTextureFactory
{
    public static Texture2D Create(Color baseColor, int seed, bool gasGiant)
    {
        const int width = 512;
        const int height = 256;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, true);
        texture.name = "High Detail Procedural Planet Surface " + seed;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = 8;

        float offsetX = seed * 17.13f;
        float offsetY = seed * 7.91f;
        Color deep = Color.Lerp(baseColor, new Color(0.012f, 0.02f, 0.055f), 0.58f);
        Color bright = Color.Lerp(baseColor, Color.white, 0.38f);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float latitude = (float)y / (height - 1);
            float polar = Mathf.Abs(latitude - 0.5f) * 2f;
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width;
                float broad = Mathf.PerlinNoise(u * 4.2f + offsetX, latitude * 3.8f + offsetY);
                float medium = Mathf.PerlinNoise(u * 11.5f + offsetY, latitude * 9.5f + offsetX);
                float fine = Mathf.PerlinNoise(u * 31f + offsetX * 0.37f, latitude * 27f + offsetY * 0.41f);
                float micro = Mathf.PerlinNoise(u * 73f + offsetY * 0.19f, latitude * 61f + offsetX * 0.23f);
                float detail = broad * 0.48f + medium * 0.27f + fine * 0.17f + micro * 0.08f;

                Color color;
                if (gasGiant)
                {
                    float turbulence = (medium - 0.5f) * 0.34f + (fine - 0.5f) * 0.12f;
                    float bands = Mathf.Sin((latitude * 25f + turbulence * 3f) * Mathf.PI);
                    float storm = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(broad - 0.72f) * 5f), 3f);
                    float bandValue = Mathf.Clamp01(0.5f + bands * 0.25f + turbulence + storm * 0.22f);
                    color = Color.Lerp(deep, bright, bandValue);
                }
                else
                {
                    float continent = Mathf.SmoothStep(0.40f, 0.58f, detail);
                    Color ocean = Color.Lerp(new Color(0.008f, 0.035f, 0.12f), baseColor * 0.58f, medium);
                    Color land = Color.Lerp(baseColor * 0.62f, bright, fine);
                    color = Color.Lerp(ocean, land, continent);
                    float cloudNoise = Mathf.PerlinNoise(u * 17f + offsetX * 0.6f, latitude * 14f + offsetY * 0.6f);
                    float clouds = Mathf.SmoothStep(0.70f, 0.84f, cloudNoise) * 0.24f;
                    color = Color.Lerp(color, new Color(0.82f, 0.88f, 0.96f), clouds);
                    if (polar > 0.82f)
                        color = Color.Lerp(color, new Color(0.76f, 0.84f, 0.94f), (polar - 0.82f) * 0.72f);
                }

                float latitudeShade = 1f - polar * 0.16f;
                pixels[y * width + x] = color * latitudeShade;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(true, true);
        return texture;
    }
}
