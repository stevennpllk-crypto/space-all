import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

required = [
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "Assets/Scripts/GalaxyExplorer/GalaxyExplorer.Runtime.asmdef",
    "Assets/Scripts/GalaxyExplorer/GalaxyWorldBootstrap.cs",
    "Assets/Scripts/GalaxyExplorer/SpacecraftFlightController.cs",
    "Assets/Scripts/GalaxyExplorer/SpacecraftFollowCamera.cs",
    "Assets/Scripts/GalaxyExplorer/PlanetDiscovery.cs",
    "Assets/Scripts/GalaxyExplorer/DiscoveryLog.cs",
    "Assets/Scripts/GalaxyExplorer/SpaceHUD.cs",
    "Assets/Scripts/GalaxyExplorer/SpaceGameMenu.cs",
    "Assets/Scripts/GalaxyExplorer/GalaxyQualityManager.cs",
    "Assets/Scripts/GalaxyExplorer/PlanetTextureFactory.cs",
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
