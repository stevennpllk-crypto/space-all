using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Creates a lightweight stylized Saturn ring mesh with procedural radial bands.
    /// Ring dimensions and opacity are illustrative, not a photometric simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlanetaryRingSystem : MonoBehaviour
    {
        [SerializeField, Range(96, 384)] private int radialSegments = 256;
        [SerializeField, Range(2, 16)] private int radialRows = 6;
        [SerializeField, Range(0f, 45f)] private float axialTiltDegrees = 26.7f;
        [SerializeField, Range(0f, 1f)] private float opacity = 0.78f;

        private readonly List<Mesh> ownedMeshes = new List<Mesh>();
        private readonly List<Material> ownedMaterials = new List<Material>();
        private readonly List<Texture2D> ownedTextures = new List<Texture2D>();

        private void Start()
        {
            Transform saturn = FindDirectChild(transform, "Saturn");
            if (saturn != null)
                CreateSaturnRings(saturn);
        }

        private void CreateSaturnRings(Transform planet)
        {
            GameObject ringObject = new GameObject("Saturn Main Ring System");
            ringObject.transform.SetParent(planet, false);
            ringObject.transform.localPosition = Vector3.zero;
            ringObject.transform.localRotation = Quaternion.Euler(axialTiltDegrees, 0f, 0f);
            ringObject.transform.localScale = Vector3.one;

            Mesh mesh = BuildAnnulus();
            ownedMeshes.Add(mesh);
            MeshFilter filter = ringObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            Material material = BuildRingMaterial();
            if (material == null)
            {
                Destroy(mesh);
                ownedMeshes.Remove(mesh);
                Destroy(ringObject);
                return;
            }

            ownedMaterials.Add(material);
            MeshRenderer renderer = ringObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private Mesh BuildAnnulus()
        {
            const float innerRadius = 1.18f;
            const float outerRadius = 2.35f;
            int vertexCount = (radialSegments + 1) * radialRows;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            int[] triangles = new int[radialSegments * (radialRows - 1) * 6];

            for (int row = 0; row < radialRows; row++)
            {
                float t = row / (float)(radialRows - 1);
                float radius = Mathf.Lerp(innerRadius, outerRadius, t);
                for (int segment = 0; segment <= radialSegments; segment++)
                {
                    float angle = segment / (float)radialSegments * Mathf.PI * 2f;
                    int index = row * (radialSegments + 1) + segment;
                    vertices[index] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                    normals[index] = Vector3.up;
                    uv[index] = new Vector2(t, segment / (float)radialSegments);
                }
            }

            int triangle = 0;
            for (int row = 0; row < radialRows - 1; row++)
            {
                for (int segment = 0; segment < radialSegments; segment++)
                {
                    int a = row * (radialSegments + 1) + segment;
                    int b = a + radialSegments + 1;
                    triangles[triangle++] = a;
                    triangles[triangle++] = b;
                    triangles[triangle++] = a + 1;
                    triangles[triangle++] = a + 1;
                    triangles[triangle++] = b;
                    triangles[triangle++] = b + 1;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Saturn Procedural Ring Annulus";
            mesh.indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private Material BuildRingMaterial()
        {
            Texture2D texture = new Texture2D(256, 8, TextureFormat.RGBA32, false, true);
            texture.name = "Saturn Ring Radial Bands";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[256 * 8];

            for (int x = 0; x < 256; x++)
            {
                float t = x / 255f;
                float bands = 0.5f + 0.5f * Mathf.Sin(t * 190f + Mathf.Sin(t * 37f) * 2.2f);
                float fineBands = 0.5f + 0.5f * Mathf.Sin(t * 620f);
                float alpha = Mathf.Lerp(0.26f, 0.92f, bands * 0.72f + fineBands * 0.28f) * opacity;
                // Cassini-like division: a deliberately clear narrow gap in the outer-middle rings.
                if (t > 0.64f && t < 0.69f) alpha *= 0.12f;
                Color color = Color.Lerp(new Color(0.35f, 0.27f, 0.17f), new Color(0.88f, 0.78f, 0.60f), bands);
                color.a = alpha;
                for (int y = 0; y < 8; y++)
                    pixels[y * 256 + x] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            ownedTextures.Add(texture);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");
            if (shader == null)
            {
                Destroy(texture);
                return null;
            }

            Material material = new Material(shader);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            material.mainTexture = texture;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetInt("_Cull", (int)CullMode.Off);
            return material;
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
            foreach (Mesh mesh in ownedMeshes)
                if (mesh != null) Destroy(mesh);
            foreach (Material material in ownedMaterials)
                if (material != null) Destroy(material);
            foreach (Texture2D texture in ownedTextures)
                if (texture != null) Destroy(texture);
        }
    }
}
