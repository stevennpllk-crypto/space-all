# Galaxy Explorer

A mobile-first 3D space exploration game built with Unity.

## Current project status
This repository was empty when development started. The initial commit adds a Unity C# starter scaffold; it is not yet a complete Unity project or an Android APK.

## Target
- Unity 6 (6000.0 LTS or newer compatible Unity 6 release)
- Android
- High-quality space visuals with scalable quality settings
- Touch-friendly camera controls
- Procedural planet placement

## Setup
1. Install Unity Hub and a Unity 6 Editor with Android Build Support (SDK, NDK, and OpenJDK).
2. Create/open a 3D Core project.
3. Copy the `Assets/` folder from this repository into the project.
4. Create an empty GameObject named `GalaxyBootstrap` and attach `ProceduralGalaxy.cs`.
5. Press Play to generate a sample star field and planets.

## Controls
The starter camera controller supports one-finger drag rotation and pinch zoom when attached to the active camera. Desktop mouse drag and scroll are also supported in the Editor.

## Roadmap
- [x] Initial procedural scene-generation scripts
- [x] Touch and mouse camera controller
- [ ] Ship flight and navigation
- [ ] Planet surface exploration
- [ ] Save system and discovery log
- [ ] Android performance profiling and device testing
- [ ] Android build and release pipeline

## Important
Ultra graphics are a target, not a verified performance setting. Quality must be profiled on the actual Android device and scaled dynamically to avoid overheating or frame drops.
