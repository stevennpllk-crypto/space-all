# Galaxy Explorer — Godot 4 migration

This is the new Godot 4 implementation track. The existing Unity source is intentionally preserved on the `main` branch while the Godot port is developed on `godot4-migration`.

## Open and run
1. Install Godot Engine 4.x (standard build; no paid license or cloud account is required).
2. In the Project Manager, choose **Import** and select this folder's `project.godot`.
3. Open the project and press **F6/F5** to run the main scene.

## Current Godot implementation
- Native Godot 4 project and a generated 3D Solar System scene.
- Sun and eight planets with animated orbital pivots.
- Procedural starfield using a single MultiMesh.
- Saturn ring and orbital path visuals.
- Free camera: mouse orbit/pan/zoom and keyboard flight.
- Touch drag and pinch input path for mobile.
- Local camera and discovery persistence under `user://`.
- No runtime network calls; the core scene is generated locally and works offline.

## Important status
This is the first migration foundation, not a completed port of every Unity feature. The current scene uses stylized procedural spheres and compressed distances. Moons, deep-space destinations, nebulae, photo texture hookup, complete touch gesture validation, VR/OpenXR, Android export templates, and device profiling remain to be ported and tested.

The Godot project has not yet been opened in the Godot editor or exported to Android in this environment. Do not treat source creation as proof of a successful engine import, desktop run, or APK build.

## Controls
- Desktop: left drag to orbit, right drag to pan, mouse wheel to zoom, WASD/QE to fly, Shift to move faster.
- Mobile: one-finger orbit, two-finger pinch/pan, tap a body to select.
- Space pauses/resumes the simulation; + / - adjusts simulation speed; R resets simulation speed.

## Offline and save behavior
The base scene and procedural content are local. Discoveries and camera state are stored in Godot's local `user://` directory. Internet is not required while exploring after the project/app is installed. Cloud builds, repository access, and future optional downloads still require connectivity.
