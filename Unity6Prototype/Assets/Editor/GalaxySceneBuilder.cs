#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GalaxyExplorer.Spacecraft;
using GalaxyExplorer.Cameras;
using GalaxyExplorer.Input;
using GalaxyExplorer.UI;
using GalaxyExplorer.World;
namespace GalaxyExplorer.Editor {
 public static class GalaxySceneBuilder {
  [MenuItem("Galaxy Explorer/Build Prototype 0.1 Scene")]
  public static void Build(){
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   RenderSettings.ambientLight=new Color(.16f,.19f,.28f);
   var lightGo=new GameObject("Solar Directional Light");var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;lightGo.transform.rotation=Quaternion.Euler(35,-35,0);
   Body("SUN",Vector3.zero,8,new Color(1,.55f,.12f),true,.15f);
   Body("EARTH",new Vector3(0,0,42),3.2f,new Color(.08f,.32f,.85f),false,5);
   Body("MOON",new Vector3(7,1,47),.9f,new Color(.48f,.5f,.54f),false,1);
   Body("MARS",new Vector3(0,0,76),2.2f,new Color(.72f,.25f,.12f),false,4);
   Stars();var ship=Ship();
   var cameraGo=new GameObject("Main Camera");cameraGo.tag="MainCamera";var cam=cameraGo.AddComponent<Camera>();cam.fieldOfView=68;cam.nearClipPlane=.05f;cam.farClipPlane=2500;cameraGo.AddComponent<AudioListener>();
   var rig=cameraGo.AddComponent<CameraRigController>();rig.SetTarget(ship.transform);cameraGo.transform.position=ship.transform.position+new Vector3(0,3,-9);cameraGo.transform.LookAt(ship.transform);
   CanvasUI(ship,rig);
   EditorSceneManager.SaveScene(scene,"Assets/Scenes/MainScene.unity");
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity",true)};
   Selection.activeGameObject=ship;Debug.Log("GALAXY EXPLORER Prototype 0.1 scene generated.");
  }
  static Material Mat(Color c,bool glow=false){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard"));m.color=c;if(glow){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*2.4f);}return m;}
  static void Body(string n,Vector3 p,float r,Color c,bool glow,float spin){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=n;g.transform.position=p;g.transform.localScale=Vector3.one*r*2;g.GetComponent<Renderer>().sharedMaterial=Mat(c,glow);g.AddComponent<CelestialBody>().Configure(n,spin);}
  static void Stars(){var g=new GameObject("Starfield");var mesh=new Mesh();var v=new Vector3[650];Random.InitState(2026);for(int i=0;i<v.Length;i++)v[i]=Random.onUnitSphere*Random.Range(250,900);mesh.vertices=v;var ids=new int[v.Length];for(int i=0;i<ids.Length;i++)ids[i]=i;mesh.SetIndices(ids,MeshTopology.Points,0);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Unlit/Color"));}
  static GameObject Ship(){var root=new GameObject("EXPLORER - Player Ship");root.transform.position=new Vector3(0,0,-8);
   var hull=GameObject.CreatePrimitive(PrimitiveType.Capsule);hull.name="Hull";hull.transform.SetParent(root.transform,false);hull.transform.localRotation=Quaternion.Euler(90,0,0);hull.transform.localScale=new Vector3(1.25f,2.7f,1.25f);hull.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.22f,.31f,.42f));
   var cockpit=GameObject.CreatePrimitive(PrimitiveType.Sphere);cockpit.name="Cockpit";cockpit.transform.SetParent(root.transform,false);cockpit.transform.localPosition=new Vector3(0,.15f,1.4f);cockpit.transform.localScale=new Vector3(.85f,.6f,.8f);cockpit.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.05f,.65f,.82f),true);
   foreach(var c in root.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);var col=root.AddComponent<CapsuleCollider>();col.direction=2;col.height=4.5f;col.radius=.8f;root.AddComponent<Rigidbody>();root.AddComponent<SpacecraftController>();return root;
  }
  static void CanvasUI(GameObject ship,CameraRigController rig){
   new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   var go=new GameObject("Flight HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);
   var left=Stick(canvas.transform,"MOVE / ROLL",new Vector2(170,170),new Vector2(0,0));var right=Stick(canvas.transform,"LOOK / ROTATE",new Vector2(-170,170),new Vector2(1,0));
   var speed=Label(canvas.transform,"VELOCITY 0.0 m/s",new Vector2(0,-48),new Vector2(.5f,1),28);var mode=Label(canvas.transform,"CAM / EXTERNAL",new Vector2(0,-88),new Vector2(.5f,1),20);
   var slider=Throttle(canvas.transform);var cam=ButtonUI(canvas.transform,"CAMERA",new Vector2(-170,95));var brake=ButtonUI(canvas.transform,"BRAKE",new Vector2(-170,35));
   var hud=go.AddComponent<FlightHUD>();hud.Bind(ship.GetComponent<SpacecraftController>(),rig,left,right,speed,mode,slider);cam.onClick.AddListener(hud.ToggleCamera);
   var trigger=brake.gameObject.AddComponent<EventTrigger>();var down=new EventTrigger.Entry{eventID=EventTriggerType.PointerDown};down.callback.AddListener(_=>hud.SetBrake(true));trigger.triggers.Add(down);var up=new EventTrigger.Entry{eventID=EventTriggerType.PointerUp};up.callback.AddListener(_=>hud.SetBrake(false));trigger.triggers.Add(up);
  }
  static VirtualJoystick Stick(Transform parent,string label,Vector2 pos,Vector2 anchor){
   var g=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(VirtualJoystick));g.transform.SetParent(parent,false);var r=(RectTransform)g.transform;r.anchorMin=anchor;r.anchorMax=anchor;r.pivot=new Vector2(anchor.x,0);r.anchoredPosition=pos;r.sizeDelta=new Vector2(210,210);g.GetComponent<Image>().color=new Color(.05f,.55f,.75f,.22f);
   var h=new GameObject("Handle",typeof(RectTransform),typeof(Image));h.transform.SetParent(g.transform,false);var hr=(RectTransform)h.transform;hr.sizeDelta=new Vector2(76,76);h.GetComponent<Image>().color=new Color(.2f,.85f,1,.72f);
   typeof(VirtualJoystick).GetField("handle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(g.GetComponent<VirtualJoystick>(),hr);Label(g.transform,label,new Vector2(0,-118),new Vector2(.5f,.5f),13);return g.GetComponent<VirtualJoystick>();
  }
  static Text Label(Transform p,string value,Vector2 pos,Vector2 anchor,int size){var g=new GameObject(value,typeof(RectTransform),typeof(Text));g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.anchorMin=anchor;r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=new Vector2(650,50);var t=g.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=size;t.color=new Color(.55f,.9f,1);t.alignment=TextAnchor.MiddleCenter;return t;}
  static Slider Throttle(Transform p){var g=new GameObject("THROTTLE",typeof(RectTransform),typeof(Slider));g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,48);r.sizeDelta=new Vector2(330,24);
   var bg=new GameObject("Background",typeof(RectTransform),typeof(Image));bg.transform.SetParent(g.transform,false);var b=(RectTransform)bg.transform;b.anchorMin=Vector2.zero;b.anchorMax=Vector2.one;b.sizeDelta=Vector2.zero;bg.GetComponent<Image>().color=new Color(.04f,.16f,.23f,.85f);
   var area=new GameObject("Fill Area",typeof(RectTransform));area.transform.SetParent(g.transform,false);var a=(RectTransform)area.transform;a.anchorMin=Vector2.zero;a.anchorMax=Vector2.one;a.offsetMin=new Vector2(5,5);a.offsetMax=new Vector2(-5,-5);
   var f=new GameObject("Fill",typeof(RectTransform),typeof(Image));f.transform.SetParent(area.transform,false);var fr=(RectTransform)f.transform;fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.sizeDelta=Vector2.zero;f.GetComponent<Image>().color=new Color(.1f,.75f,.95f);
   var ha=new GameObject("Handle Slide Area",typeof(RectTransform));ha.transform.SetParent(g.transform,false);var har=(RectTransform)ha.transform;har.anchorMin=Vector2.zero;har.anchorMax=Vector2.one;har.offsetMin=new Vector2(10,0);har.offsetMax=new Vector2(-10,0);
   var h=new GameObject("Handle",typeof(RectTransform),typeof(Image));h.transform.SetParent(ha.transform,false);var hr=(RectTransform)h.transform;hr.sizeDelta=new Vector2(28,34);var s=g.GetComponent<Slider>();s.fillRect=fr;s.handleRect=hr;s.targetGraphic=h.GetComponent<Image>();s.minValue=0;s.maxValue=1;s.value=.65f;return s;
  }
  static Button ButtonUI(Transform p,string label,Vector2 pos){var g=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(1,1);r.anchoredPosition=pos;r.sizeDelta=new Vector2(150,48);g.GetComponent<Image>().color=new Color(.04f,.28f,.4f,.85f);var t=Label(g.transform,label,Vector2.zero,new Vector2(.5f,.5f),17);t.color=Color.white;return g.GetComponent<Button>();}
 }
}
#endif