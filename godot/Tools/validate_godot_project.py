#!/usr/bin/env python3
"""Static checks for the Godot 4 Galaxy Explorer migration."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
REQUIRED = [
    "project.godot",
    "scenes/main.tscn",
    "scripts/main.gd",
    "scripts/free_camera.gd",
    "README_GODOT4.md",
]

def main() -> int:
    errors = []
    for relative in REQUIRED:
        if not (ROOT / relative).is_file():
            errors.append(f"Missing required project file: {relative}")

    project = ROOT / "project.godot"
    if project.exists():
        text = project.read_text(encoding="utf-8")
        if 'run/main_scene="res://scenes/main.tscn"' not in text:
            errors.append("Main scene is not configured.")
        if 'renderer/rendering_method.mobile="gl_compatibility"' not in text:
            errors.append("Mobile renderer must use Compatibility mode for broad Android support.")

    scene = ROOT / "scenes/main.tscn"
    if scene.exists() and 'res://scripts/main.gd' not in scene.read_text(encoding="utf-8"):
        errors.append("Main scene does not reference scripts/main.gd.")

    main_script = ROOT / "scripts/main.gd"
    if main_script.exists():
        text = main_script.read_text(encoding="utf-8")
        for token in ("Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune"):
            if f'"name":"{token}"' not in text:
                errors.append(f"Missing Solar System body definition: {token}")
        for token in ("MultiMeshInstance3D", "TorusMesh", "PhysicsRayQueryParameters3D", "user://discoveries.cfg"):
            if token not in text:
                errors.append(f"Missing expected runtime feature: {token}")

    camera_script = ROOT / "scripts/free_camera.gd"
    if camera_script.exists():
        text = camera_script.read_text(encoding="utf-8")
        for token in ("InputEventScreenDrag", "active_touches", "save_exploration_state", "user://exploration.cfg"):
            if token not in text:
                errors.append(f"Missing camera/offline feature: {token}")

    if errors:
        print("Godot project validation: FAILED")
        for error in errors:
            print(f"- {error}")
        return 1
    print("Godot project validation: PASSED (static source checks only)")
    print("Godot Editor parsing, runtime, Android export, and device tests are not covered.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
