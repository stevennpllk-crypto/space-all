using UnityEngine;

/// <summary>
/// Creates a lightweight procedural star field and a small set of planets.
/// Attach to an empty GameObject in a Unity 3D scene.
/// </summary>
public sealed class ProceduralGalaxy : MonoBehaviour
{
    [Header("Star field")]
    [Min(100)] public int starCount = 1800;
    [Min(20f)] public float starRadius = 450f;
    public Material starMaterial;

    [Header("Planets")]
    public int planetCount = 7;
    public float planetOrbitRadius = 24f;
    public float planetSize = 2.5f;

    private void Start()
    {
        GenerateStars();
        GeneratePlanets();
    }

    private void GenerateStars()
    {
        var root = new GameObject("Generated Stars");
        root.transform.SetParent(transform, false);
        var mesh = new Mesh { name = "Procedural Star Points" };
        var vertices = new Vector3[starCount];
        var colors = new Color[starCount];
        for (int i = 0; i < starCount; i++)
        {
            vertices[i] = Random.onUnitSphere * starRadius;
            float brightness = Random.Range(0.55f, 1f);
            colors[i] = Color.Lerp(new Color(0.55f, 0.68f, 1f), Color.white, Random.value) * brightness;
        }
        mesh.indexFormat = starCount > 65535
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = vertices;
        mesh.colors = colors;
        var indices = new int[starCount];
        for (int i = 0; i < starCount; i++) indices[i] = i;
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.RecalculateBounds();

        var filter = root.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        var renderer = root.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = starMaterial != null
            ? starMaterial
            : new Material(Shader.Find("Sprites/Default"));
    }

    private void GeneratePlanets()
    {
        int count = Mathf.Clamp(planetCount, 1, 12);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            float radius = planetOrbitRadius + i * 5f;
            Vector3 position = new Vector3(Mathf.Cos(angle), Random.Range(-0.8f, 0.8f), Mathf.Sin(angle)) * radius;
            var planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = "Planet " + (i + 1);
            planet.transform.SetParent(transform, false);
            planet.transform.localPosition = position;
            float scale = planetSize * Random.Range(0.55f, 1.65f);
            planet.transform.localScale = Vector3.one * scale;
            var renderer = planet.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Standard"));
            material.color = Color.HSVToRGB(Random.value, Random.Range(0.35f, 0.8f), Random.Range(0.55f, 1f));
            renderer.sharedMaterial = material;
        }
    }
}
