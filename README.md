# Galaxy Explorer

A mobile-first 3D space exploration game built with Unity 6.

## Project status

This repository is in the **preparation phase**. Scripts, package configuration, and procedural world systems are being assembled. The project has **not** been built, packaged, or tested in the Unity Editor yet. No APK exists.

## Current systems in source

- Runtime procedural starfield and seven discoverable planets with generated surface maps.\n- A ringed gas giant and a custom primitive spacecraft model.
- Planet rotation and orbital path visuals.
- Rigidbody spacecraft movement with keyboard and touch input.
- Smooth third-person chase camera.
- Proximity-based planet discovery and local discovery journal.
- Runtime HUD showing ship speed and discovery count.\n- Pause overlay and discovery journal.\n- Adaptive low/balanced/high graphics defaults based on device RAM and GPU memory.\n- Editor utility that creates a starter scene asset and registers it in Build Settings when Unity imports the project.\n- Editor command for Android landscape, ARM64, IL2CPP, Android API 26+, and dual input handling.\n- Initial EditMode tests for discovery persistence.
- Unity package manifest for URP, Input System, uGUI, and Test Framework.

## Target platform and quality

- Unity 6 (6000.0.32f1 baseline)
- Android first; iOS is a later target.
- Universal Render Pipeline.
- Scalable quality settings: mobile performance must be profiled before enabling expensive effects.
- Touch-first controls, with keyboard support in the Editor.

## Controls (current prototype)

- Desktop: WASD to accelerate/steer, mouse movement to rotate, Q/E to roll, Shift to boost, Ctrl to brake.
- Mobile: hold and drag on the left side to steer; hold and drag on the right side to rotate.
- Touch controls and the selected Unity Input handling mode still require Editor/device verification.

## Preparation roadmap

- [x] Initial Unity C# scaffold
- [x] Procedural galaxy bootstrap and starter planets
- [x] Spacecraft flight controller
- [x] Chase camera
- [x] Discovery journal and HUD\n- [x] Pause menu and journal overlay\n- [x] Adaptive mobile quality profile\n- [x] Procedural planet surface maps and ring geometry\n- [x] Starter scene installer and Android setup utility\n- [x] Initial EditMode tests\n- [x] Runtime and test assembly definitions
- [x] Unity package manifest and ignore rules
- [ ] Validate package versions in Unity Package Manager
- [ ] Add authored planet textures, ship model, skybox, and audio assets
- [ ] Add main menu, settings, map, and discovery screens
- [ ] Add automated EditMode/PlayMode tests
- [ ] Configure Android quality tiers and input settings
- [ ] Verify the generated starter scene in Unity Editor
- [ ] Android profiling and build pipeline

## Important

The runtime bootstrap creates a starter scene when Play starts, so the code can be tested in a new empty scene. This is not a substitute for a finished authored scene. Visual assets are currently procedural placeholders. The APK/build stage is intentionally deferred until the user says **جمع**.
