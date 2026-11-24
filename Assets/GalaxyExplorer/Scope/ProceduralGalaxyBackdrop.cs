using UnityEngine;
using UnityEngine.Rendering;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Builds a deterministic, low-cost stylized Milky Way star disc from one point mesh.
    /// The distribution is artistic and is not a measured map of the Milky Way.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProceduralGalaxyBackdrop : MonoBehaviour
    {
        [SerializeField, Range(2000, 16000)] private int starCount = 9000;
        [SerializeField, Min(40f)] private float galaxyRadius = 175f;
        [SerializeField, Range(0.1f, 1f)] private float discThickness = 0.42f;
        [SerializeField, Range(2, 6)] private int spiralArms = 4;
        [SerializeField] private bool showGalaxy = true;
        [SerializeField] private Vector3 localCenter = new Vector3(0f, 68f, 155f);
        [SerializeField] private Vector3 localRotation = new Vector3(58f, -12f, 8f);

        private GameObject galaxyRoot;
        private Material starMaterial;
        private Mesh galaxyMesh;

        public bool IsVisible { get { return showGalaxy; } }

        private void Start()
        {
            BuildGalaxy();
        }

        public void SetVisible(bool visible)
        {
            showGalaxy = visible;
            if (galaxyRoot != null) galaxyRoot.SetActive(visible);
        }

        private void BuildGalaxy()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return;

            galaxyRoot = new GameObject("Procedural Milky Way - Spiral Disc");
            galaxyRoot.transform.SetParent(transform, false);
            galaxyRoot.transform.localPosition = localCenter;
            galaxyRoot.transform.localRotation = Quaternion.Euler(localRotation);

            galaxyMesh = new Mesh { name = "Milky Way Procedural Star Points" };
            if (starCount > 65000) galaxyMesh.indexFormat = IndexFormat.UInt32;

            Vector3[] vertices = new Vector3[starCount];
            Color[] colors = new Color[starCount];
            int[] indices = new int[starCount];

            Random.State previousState = Random.state;
            Random.InitState(24681357);
            for (int i = 0; i < starCount; i++)
            {
                float radius = Mathf.Pow(Random.value, 0.72f) * galaxyRadius;
                float arm = Random.Range(0, spiralArms);
                float angle = arm * Mathf.PI * 2f / spiralArms
                    + radius * 0.038f
                    + Random.Range(-0.30f, 0.30f);

                // A broad central bulge and four loose spiral arms share one compact mesh.
                float coreWeight = Mathf.Exp(-radius / 34f);
                float x = Mathf.Cos(angle) * radius + Random.Range(-2.2f, 2.2f);
                float z = Mathf.Sin(angle) * radius + Random.Range(-2.2f, 2.2f);
                float y = Random.Range(-1f, 1f) * (1.2f + discThickness * radius * 0.035f);
                if (Random.value < coreWeight * 0.55f)
                {
                    x *= 0.38f;
                    z *= 0.38f;
                    y *= 0.65f;
                }

                vertices[i] = new Vector3(x, y, z);
                float brightness = Random.Range(0.48f, 1f);
                float tint = Random.value;
                Color starColor = tint < 0.16f
                    ? new Color(1f, 0.72f, 0.48f)
                    : (tint < 0.42f ? new Color(0.58f, 0.72f, 1f) : new Color(0.86f, 0.91f, 1f));
                float coreGlow = Mathf.Clamp01(1f - radius / (galaxyRadius * 0.82f));
                colors[i] = starColor * (brightness * (0.78f + coreGlow * 0.35f));
                colors[i].a = 1f;
                indices[i] = i;
            }
            Random.state = previousState;

            galaxyMesh.vertices = vertices;
            galaxyMesh.colors = colors;
            galaxyMesh.SetIndices(indices, MeshTopology.Points, 0, false);
            galaxyMesh.bounds = new Bounds(Vector3.zero, Vector3.one * (galaxyRadius * 2.5f));

            MeshFilter filter = galaxyRoot.AddComponent<MeshFilter>();
            filter.sharedMesh = galaxyMesh;
            MeshRenderer renderer = galaxyRoot.AddComponent<MeshRenderer>();
            starMaterial = new Material(shader) { name = "Milky Way Point Star Material" };
            renderer.sharedMaterial = starMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            galaxyRoot.SetActive(showGalaxy);
        }

        private void OnDestroy()
        {
            if (starMaterial != null) Destroy(starMaterial);
            if (galaxyMesh != null) Destroy(galaxyMesh);
        }
    }
}
