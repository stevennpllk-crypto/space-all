using System;
using UnityEngine;

namespace GalaxyExplorer.Scope
{
    [DisallowMultipleComponent]
    public sealed class NearbyStarCatalog : MonoBehaviour
    {
        [Serializable]
        private sealed class StarEntry
        {
            public string name, spectralClass, fact;
            public Vector3 position;
            public Color color;
            public float visualRadius;
            public StarEntry(string n, string s, string f, Vector3 p, Color c, float r)
            { name=n; spectralClass=s; fact=f; position=p; color=c; visualRadius=r; }
        }

        private static readonly StarEntry[] Catalog =
        {
            new StarEntry("Sirius","A1V • Binary system","The brightest star in Earth's night sky. Sirius is a nearby binary star system.",new Vector3(205f,24f,42f),new Color(.72f,.84f,1f),2.7f),
            new StarEntry("Proxima Centauri","M5.5Ve • Red dwarf","The nearest known stellar neighbour to the Sun. It is part of the Alpha Centauri system.",new Vector3(-218f,-18f,65f),new Color(1f,.34f,.20f),2.1f),
            new StarEntry("Alpha Centauri A","G2V • Sun-like star","The primary star of the Alpha Centauri pair. Its spectral class is similar to the Sun's.",new Vector3(-246f,30f,-48f),new Color(1f,.82f,.55f),2.8f),
            new StarEntry("Vega","A0V • Main-sequence star","A bright blue-white star in the constellation Lyra and part of the Summer Triangle.",new Vector3(120f,92f,-224f),new Color(.72f,.84f,1f),2.6f),
            new StarEntry("Betelgeuse","M1-M2 Ia • Red supergiant","A red supergiant in Orion. Its size and brightness vary over time.",new Vector3(-95f,135f,250f),new Color(1f,.36f,.19f),4f)
        };

        private Transform[] starTransforms;
        private Material[] starMaterials;
        private int selectedIndex = -1;
        private GUIStyle headingStyle, detailStyle, buttonStyle, panelStyle;

        private static Shader FindCompatibleShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");
            return shader;
        }

        private void Start()
        {
            starTransforms = new Transform[Catalog.Length];
            starMaterials = new Material[Catalog.Length];
            for (int i=0;i<Catalog.Length;i++) CreateStar(Catalog[i],i);
        }

        private void CreateStar(StarEntry entry, int index)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Nearby Star - " + entry.name;
            star.transform.SetParent(transform,false);
            star.transform.localPosition = entry.position;
            star.transform.localScale = Vector3.one * entry.visualRadius * 2f;

            Renderer renderer = star.GetComponent<Renderer>();
            Shader shader = FindCompatibleShader();
            if (renderer != null && shader != null)
            {
                Material material = new Material(shader) { name = entry.name + " Material" };
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor",entry.color);
                if (material.HasProperty("_Color")) material.SetColor("_Color",entry.color);
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor",entry.color*2.2f);
                }
                renderer.sharedMaterial = material;
                starMaterials[index] = material;
            }

            Collider collider = star.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            GameObject labelObject = new GameObject(entry.name + " Label");
            labelObject.transform.SetParent(star.transform,false);
            labelObject.transform.localPosition = Vector3.up*(entry.visualRadius+1.8f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text=entry.name; label.anchor=TextAnchor.MiddleCenter;
            label.alignment=TextAlignment.Center; label.characterSize=.22f;
            label.fontSize=48; label.color=new Color(.82f,.90f,1f);
            starTransforms[index]=star.transform;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale=Mathf.Clamp(Screen.width/1100f,.72f,1.1f);
            Matrix4x4 previous=GUI.matrix;
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1f));
            float x=Screen.width/scale-230f;
            GUI.Box(new Rect(x,164f,210f,222f),GUIContent.none,panelStyle);
            GUI.Label(new Rect(x+12f,174f,190f,26f),"NEARBY STARS",headingStyle);
            for(int i=0;i<Catalog.Length;i++)
            {
                int row=i;
                if(GUI.Button(new Rect(x+12f,204f+i*32f,186f,27f),Catalog[i].name,buttonStyle)) FocusStar(row);
            }
            if(selectedIndex>=0)
            {
                StarEntry entry=Catalog[selectedIndex];
                GUI.Box(new Rect(18f,326f,310f,120f),GUIContent.none,panelStyle);
                GUI.Label(new Rect(32f,336f,280f,24f),entry.name+" • "+entry.spectralClass,headingStyle);
                GUI.Label(new Rect(32f,366f,280f,70f),entry.fact,detailStyle);
            }
            GUI.matrix=previous;
        }

        private void FocusStar(int index)
        {
            if(index<0||index>=Catalog.Length||starTransforms==null||starTransforms[index]==null)return;
            selectedIndex=index;
            Camera camera=Camera.main;
            if(camera==null)return;
            SolarSystemScopeCamera controller=camera.GetComponent<SolarSystemScopeCamera>();
            if(controller!=null)controller.Focus(starTransforms[index]);
            else camera.transform.LookAt(starTransforms[index]);
        }

        private void EnsureStyles()
        {
            if(headingStyle!=null)return;
            headingStyle=new GUIStyle(GUI.skin.label){fontSize=13,fontStyle=FontStyle.Bold};
            headingStyle.normal.textColor=Color.white;
            detailStyle=new GUIStyle(GUI.skin.label){fontSize=11,wordWrap=true};
            detailStyle.normal.textColor=new Color(.85f,.90f,.98f);
            buttonStyle=new GUIStyle(GUI.skin.button){fontSize=11};
            panelStyle=new GUIStyle(GUI.skin.box);
        }

        private void OnDestroy()
        {
            if(starMaterials==null)return;
            foreach(Material material in starMaterials) if(material!=null)Destroy(material);
        }
    }
}