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

with (ROOT / "Assets/Art/NASA/NASA_ASSET_CREDITS.json").open(encoding="utf-8") as handle:
    nasa_manifest = json.load(handle)
if len(nasa_manifest.get("assets", [])) < 7:
    raise SystemExit("Expected at least seven NASA image records.")
for asset in nasa_manifest["assets"]:
    if not (ROOT / "Assets/Art/NASA" / asset["file"]).is_file():
        raise SystemExit("Missing NASA image asset: " + asset["file"])

