import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

required = [
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "Assets/Scripts/GalaxyExplorer/GalaxyExplorer.Runtime.asmdef",
    "Assets/Scripts/GalaxyExplorer/GalaxyWorldBootstrap.cs",
    "Assets/GalaxyExplorer/Scope/SolarSystemScopeRuntime.cs",
    "Assets/GalaxyExplorer/Scope/SolarSystemScopeCamera.cs",
    "Assets/GalaxyExplorer/Scope/GalaxyExplorerVRSupport.cs",
    "Assets/GalaxyExplorer/Scope/DeepSpaceDestinationCatalog.cs",
    "Assets/GalaxyExplorer/Scope/NearbyStarCatalog.cs",
    "Assets/Scripts/GalaxyExplorer/PlanetDiscovery.cs",
    "Assets/Scripts/GalaxyExplorer/DiscoveryLog.cs",
    "Assets/Scripts/GalaxyExplorer/SpaceHUD.cs",
    "Assets/Scripts/GalaxyExplorer/SpaceGameMenu.cs",
    "Assets/Scripts/GalaxyExplorer/GalaxyQualityManager.cs",
    "Assets/Scripts/GalaxyExplorer/PlanetTextureFactory.cs",
    "Assets/Scripts/GalaxyExplorer/NASAImageGallery.cs",
    "Assets/Editor/NASAResourceImporter.cs",
    "Assets/Art/NASA/NASA_ASSET_CREDITS.json",
    "Assets/Art/NASA/earth_gsfc_20171208_archive_e002131.jpg",
    "Assets/Art/NASA/mars_pia24264.jpg",
    "Assets/Art/NASA/jupiter_pia25020.jpg",
    "Assets/Art/NASA/saturn_pia14858.jpg",
    "Assets/Art/NASA/nebula_gsfc_20171208_archive_e000273.jpg",
    "Assets/Art/NASA/galaxy_pia23123.jpg",
    "Assets/Art/NASA/moon_pia15515.jpg",
    "Assets/Scripts/GalaxyExplorer/ProceduralSpaceAudio.cs",
    "Assets/Editor/GalaxyProjectSetup.cs",
    "Assets/Editor/GalaxySceneInstaller.cs",
    "Assets/Scripts/GalaxyExplorer/GalaxyWorldBootstrap.cs.meta",
    "Assets/Tests/EditMode/GalaxyExplorer.Tests.asmdef",
    "Assets/Tests/EditMode/DiscoveryLogTests.cs",
    "Assets/Tests/EditMode/PlanetTextureFactoryTests.cs",
    "Assets/Scenes/.gitkeep",
]

missing = [path for path in required if not (ROOT / path).is_file()]
if missing:
    raise SystemExit("Missing required project files:\n" + "\n".join(missing))

for relative in [
    "Packages/manifest.json",
    "Assets/Scripts/GalaxyExplorer/GalaxyExplorer.Runtime.asmdef",
    "Assets/Tests/EditMode/GalaxyExplorer.Tests.asmdef",
]:
    with (ROOT / relative).open(encoding="utf-8") as handle:
        json.load(handle)

version = (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8")
if "m_EditorVersion: 6000." not in version:
    raise SystemExit("Expected a Unity 6 editor version.")

print("Source validation passed: required files and JSON configuration are present.")
print("Note: this check does not compile Unity C# or create an APK.")

# Prevent custom runtime asmdef scripts from referencing predefined Assembly-CSharp Scope scripts.
bootstrap_source = (ROOT / "Assets/Scripts/GalaxyExplorer/GalaxyWorldBootstrap.cs").read_text(encoding="utf-8")
if "GalaxyExplorer.Scope.SolarSystemScopeCamera" in bootstrap_source or "GalaxyExplorer.Scope.GalaxyExplorerVRSupport" in bootstrap_source:
    raise SystemExit("GalaxyExplorer.Runtime asmdef must not reference Scope scripts in Assembly-CSharp.")
if "typeof(GalaxyExplorer.Scope." in bootstrap_source or "AddComponent<GalaxyExplorer.Scope." in bootstrap_source or "GetComponent<GalaxyExplorer.Scope." in bootstrap_source:
    raise SystemExit("Runtime bootstrap must detect the Scope runtime without a compile-time assembly reference.")
print("Unity assembly-boundary checks passed.")

# Keep the documented Unity menu path aligned with the editor installer.
scope_readme = (ROOT / "Assets/GalaxyExplorer/Scope/README.md").read_text(encoding="utf-8")
if "Galaxy Explorer → Setup → Create or Repair Starter Scene" not in scope_readme:
    raise SystemExit("Scope README must use the actual starter-scene installer menu path.")
scope_runtime = (ROOT / "Assets/GalaxyExplorer/Scope/SolarSystemScopeRuntime.cs").read_text(encoding="utf-8")
if scope_runtime.count('new BodyDefinition("') != 8:
    raise SystemExit("Solar System Scope runtime must define all eight planets.")
print("Solar System Scope documentation and planet catalog checks passed.")

# Saturn's ring system must have one owner and tolerate shader lookup failure.
ring_source = (ROOT / "Assets/GalaxyExplorer/Scope/PlanetaryRingSystem.cs").read_text(encoding="utf-8")
if "if (shader == null) shader = Shader.Find(\"Hidden/InternalErrorShader\");" not in ring_source:
    raise SystemExit("Saturn ring material must provide a final shader fallback.")
if "if (shader == null)" not in ring_source or "Destroy(texture);" not in ring_source:
    raise SystemExit("Saturn ring material creation must safely handle unavailable shaders.")
if "if (material == null)" not in ring_source:
    raise SystemExit("Saturn ring renderer must handle failed material creation.")
if "CreateSaturnRings(planet.transform" in scope_runtime:
    raise SystemExit("Saturn rings must be generated only by PlanetaryRingSystem, not duplicated by the base runtime.")
print("Saturn ring uniqueness and shader-fallback checks passed.")

# Ensure the generated starter scene launches the actual Solar System Scope runtime.
scene_installer = (ROOT / "Assets/Editor/GalaxySceneInstaller.cs").read_text(encoding="utf-8")
if 'scopeRoot.AddComponent<GalaxyExplorer.Scope.SolarSystemScopeRuntime>();' not in scene_installer:
    raise SystemExit("Starter scene must launch the eight-planet Solar System Scope runtime.")
if 'bootstrap.AddComponent<GalaxyWorldBootstrap>();' in scene_installer:
    raise SystemExit("Starter scene must not replace the Solar System Scope with the generic fallback world.")
if "starterCamera.transform.position = new Vector3(0f, 16f, -34f);" not in scene_installer:
    raise SystemExit("Starter scene camera must begin from the elevated Solar System overview position.")
if "starterCamera.transform.LookAt(Vector3.zero);" not in scene_installer:
    raise SystemExit("Starter scene camera must initially face the Solar System origin.")
print("Solar System Scope starter-scene wiring and camera checks passed.")

# Guard against regressions in runtime material creation and ownership.
nearby_source = (ROOT / "Assets/GalaxyExplorer/Scope/NearbyStarCatalog.cs").read_text(encoding="utf-8")
deep_source = (ROOT / "Assets/GalaxyExplorer/Scope/DeepSpaceDestinationCatalog.cs").read_text(encoding="utf-8")
runtime_source = (ROOT / "Assets/GalaxyExplorer/Scope/SolarSystemScopeRuntime.cs").read_text(encoding="utf-8")
if "FindCompatibleShader" not in nearby_source or "starMaterials" not in nearby_source or "Destroy(material)" not in nearby_source:
    raise SystemExit("Nearby star catalog must use safe shader lookup and release owned materials.")
if "Hidden/InternalErrorShader" not in deep_source or "Hidden/InternalErrorShader" not in runtime_source:
    raise SystemExit("Runtime catalogs must include a final shader fallback.")
if "if (shader == null) return null;" not in runtime_source:
    raise SystemExit("Runtime material factory must handle missing shaders safely.")
print("Runtime material safety checks passed.")

# Guard against selecting planets through the on-screen IMGUI panels.
if "IsPointerOverGui(Vector2 screenPosition)" not in runtime_source:
    raise SystemExit("Runtime selection must use screen-position based UI hit testing.")
if "Input.GetMouseButtonDown(0) && !IsPointerOverGui(Input.mousePosition)" not in runtime_source:
    raise SystemExit("Mouse selection must ignore clicks on the exploration UI.")
if "if (!IsPointerOverGui(touchPosition))" not in runtime_source:
    raise SystemExit("Touch selection must ignore taps on the exploration UI.")
if "Event.current != null && Event.current.type == EventType.Used" in runtime_source:
    raise SystemExit("Do not inspect Event.current from Update; it is not a reliable IMGUI hit test.")
print("Exploration UI input guards passed.")

if "Matrix4x4 previousMatrix = GUI.matrix;" not in runtime_source or "GUI.matrix = previousMatrix;" not in runtime_source:
    raise SystemExit("Scope HUD must restore the shared IMGUI matrix after drawing.")
print("IMGUI state restoration checks passed.")

camera_source = (ROOT / "Assets/GalaxyExplorer/Scope/SolarSystemScopeCamera.cs").read_text(encoding="utf-8")
if "IsPointerOverUi(Input.mousePosition)" not in camera_source:
    raise SystemExit("Mouse camera gestures must ignore exploration UI panels.")
if "IsPointerOverUi(touch.position)" not in camera_source:
    raise SystemExit("Single-touch camera gestures must ignore exploration UI panels.")
if "IsPointerOverUi(first.position) || IsPointerOverUi(second.position)" not in camera_source:
    raise SystemExit("Pinch gestures must ignore touches that begin on exploration UI panels.")
print("Camera gesture UI guards passed.")

satellite_source = (ROOT / "Assets/GalaxyExplorer/Scope/NaturalSatelliteSystem.cs").read_text(encoding="utf-8")
if "return shader != null ? new Material(shader) : null;" not in satellite_source:
    raise SystemExit("Satellite material creation must handle missing shaders.")
if "if (orbitMaterial == null)" not in satellite_source:
    raise SystemExit("Satellite orbit rendering must handle missing shaders.")
if "satelliteMaterials.Add(moonMaterial)" not in satellite_source or "Destroy(material)" not in satellite_source:
    raise SystemExit("Per-moon materials must be tracked and released.")
if "moonRenderer.sharedMaterial = moonMaterial;" not in satellite_source:
    raise SystemExit("Satellite materials must avoid implicit Renderer.material cloning.")
print("Natural satellite material lifecycle checks passed.")





with (ROOT / "Assets/Art/NASA/NASA_ASSET_CREDITS.json").open(encoding="utf-8") as handle:
    nasa_manifest = json.load(handle)
if len(nasa_manifest.get("assets", [])) < 7:
    raise SystemExit("Expected at least seven NASA image records.")
for asset in nasa_manifest["assets"]:
    if not (ROOT / "Assets/Art/NASA" / asset["file"]).is_file():
        raise SystemExit("Missing NASA image asset: " + asset["file"])


# Keep Android CI from spending a full Unity build attempt without its license secret.
android_workflow = (ROOT / ".github/workflows/android-apk.yml").read_text(encoding="utf-8")
if "Check Unity license configuration" not in android_workflow:
    raise SystemExit("Android workflow must check Unity license configuration before invoking Unity.")
if 'if [[ -z "${UNITY_LICENSE//[[:space:]]/}" ]]; then' not in android_workflow:
    raise SystemExit("Android workflow must reject an empty or whitespace-only Unity license secret.")
print("Android build license preflight checks passed.")

# Keep the Android build instructions aligned with the supported license workflow.
build_guide = (ROOT / "BUILD_ANDROID.md").read_text(encoding="utf-8")
for required_text in [
    "Unity Personal license",
    "Settings → Licenses → Add",
    "UNITY_LICENSE",
    "UNITY_EMAIL",
    "UNITY_PASSWORD",
    "No Unity compilation was attempted",
]:
    if required_text not in build_guide:
        raise SystemExit("Android build guide is missing current license guidance: " + required_text)
if "bypass Unity's license checks" not in build_guide:
    raise SystemExit("Android build guide must explicitly reject license-check bypasses.")
print("Android license documentation checks passed.")


# Planet surface maps must not create hidden Renderer.material clones.
surface_texture_source = (ROOT / "Assets/GalaxyExplorer/Scope/PlanetSurfaceTextures.cs").read_text(encoding="utf-8")
if "renderer.material" in surface_texture_source:
    raise SystemExit("Planet surface textures must not create untracked Renderer.material clones.")
if "renderer.sharedMaterial" not in surface_texture_source:
    raise SystemExit("Planet surface textures must update the runtime-owned shared material.")
if "private readonly List<Material> ownedMaterials" not in runtime_source:
    raise SystemExit("Solar System runtime must track all owned material instances.")
if "private Mesh starfieldMesh" not in runtime_source or "if (starfieldMesh != null) Destroy(starfieldMesh);" not in runtime_source:
    raise SystemExit("Solar System runtime must explicitly release its generated starfield mesh.")
if "foreach (Material material in ownedMaterials)" not in runtime_source:
    raise SystemExit("Solar System runtime must release all tracked materials on destruction.")
print("Planet material ownership and starfield cleanup checks passed.")
