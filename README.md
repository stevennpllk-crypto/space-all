# Galaxy Explorer

A mobile-first 3D space exploration game built with Unity 6.

## Project status

This repository is in the **preparation phase**. Scripts, package configuration, and procedural world systems are being assembled. The project has **not** been built, packaged, or tested in the Unity Editor yet. No APK exists.

## Current systems in source

- Runtime procedural starfield and seven discoverable planets with 512×256 multi-layer surface maps.
- Procedural tangent-space normal maps for additional lighting and surface relief.
- A user-adjustable graphics slider from 50% (performance) to 150% (ultra), saved locally.
- A free-flight exploration camera; no spacecraft or vehicle is required.
- Original procedural engine hum and deep-space ambient audio.
- Planet rotation and orbital path visuals.
- Touch-first free-flight navigation with orbit, pan, pinch zoom, and selectable celestial destinations.
- Smooth orbital and free-flight camera transitions.
- Proximity-based planet discovery and local discovery journal.
- Runtime HUD showing ship speed and discovery count.
- Pause overlay and discovery journal.
- Adaptive low/balanced/high graphics defaults based on device RAM and GPU memory.
- Editor utility that creates a starter scene asset and registers it in Build Settings when Unity imports the project.
- Editor command for Android landscape, ARM64, IL2CPP, Android API 26+, and dual input handling.
- Initial EditMode tests for discovery persistence.
- Unity package manifest for URP, Input System, uGUI, and Test Framework.

## Target platform and quality

- Unity 6 (6000.0.32f1 baseline)
- Android first; iOS is a later target.
- Universal Render Pipeline.
- Scalable quality settings: the in-game slider adjusts URP render scale, LOD, texture mip limits, shadows, and target frame rate.
- 150% quality is a maximum setting, not a performance guarantee; use a lower value on memory-limited phones.
- Touch-first controls, with keyboard support in the Editor.

## Controls (current prototype)

- Desktop: drag to orbit, right/middle drag to pan, wheel to zoom, WASD to fly, Q/E for vertical movement, Shift for faster flight.
- Mobile: one finger to orbit; two fingers to pan and pinch-zoom.
- Selecting a planet, moon, star, or deep-space destination smoothly focuses the camera.
- Touch controls and the selected Unity Input handling mode still require Editor/device verification.

## Preparation roadmap

- [x] Initial Unity C# scaffold
- [x] Procedural galaxy bootstrap and starter planets
- [x] Free-flight exploration camera
- [x] Smooth focus, pan, and zoom controls
- [x] Discovery journal and HUD
- [x] Pause menu and journal overlay
- [x] Adaptive mobile quality profile
- [x] Procedural planet surface maps and ring geometry
- [x] Starter scene installer and Android setup utility
- [x] Initial EditMode tests
- [x] Runtime and test assembly definitions
- [x] Unity package manifest and ignore rules
- [ ] Validate package versions in Unity Package Manager
- [ ] Add authored planet textures, ship model, skybox, and audio assets
- [ ] Add main menu, settings, map, and discovery screens
- [ ] Add automated EditMode/PlayMode tests
- [ ] Configure Android quality tiers and input settings
- [ ] Verify the generated starter scene in Unity Editor
- [ ] Android profiling and build pipeline

## Android APK build

An automated Unity 6 Android APK workflow is available at `.github/workflows/android-apk.yml`. It requires a Unity Personal or Pro license file stored as the repository Actions secret `UNITY_LICENSE`. The first build attempt confirmed that this secret is not configured, so no APK was produced. See [BUILD_ANDROID.md](BUILD_ANDROID.md) for setup instructions. Never paste Unity credentials or license contents into chat or commit them to the repository.

## Important

The runtime bootstrap creates a starter scene when Play starts, so the code can be tested in a new empty scene. This is not a substitute for a finished authored scene. Visual assets are currently procedural placeholders. The APK/build stage is intentionally deferred until the user says **جمع**.


## Solar System Scope implementation

A new self-contained Unity 6 prototype has been added under `Assets/GalaxyExplorer/Scope`.
It generates the Sun, eight planets, orbit paths, Saturn's rings, a deterministic starfield,
animated orbital motion, planet selection, educational fact panels, and touch/mouse camera
navigation at runtime.

To create a demo scene, open Unity and use **Galaxy Explorer → Create Solar System Scope Demo**.
Save the generated scene and press Play. The prototype uses compressed distances and
non-linear planet sizes for readability; it is an educational visualization rather than a
precision ephemeris. See `Assets/GalaxyExplorer/Scope/README.md` for setup and limitations.

The new source has been committed but has not yet been compiled or tested in Unity Editor.
