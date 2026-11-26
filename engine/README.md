# JASSIM Engine

JASSIM Engine is an independent, mobile-first 3D engine project. It is not a Unity fork and does not use Unity runtime or editor code.

## Current milestone: foundation

This first milestone establishes a small dependency-free C++ core:
- vector and transform math;
- scene/entity storage with stable numeric IDs;
- a portable CMake build;
- a smoke test executed in CI.

It is **not yet a complete 3D engine** and does not produce an APK. Rendering, Android input/window integration, asset import, physics, editor tooling, and APK packaging are later milestones.

## Technical direction

- Core language: C++17.
- Build system: CMake.
- Android target: ARM64 first, with Android NDK.
- Rendering backend: OpenGL ES 3.x initially, with runtime capability checks and a conservative fallback path.
- App integration: Android NativeActivity or a thin Android Activity/JNI shell, selected after the renderer prototype.
- Design goal: small runtime, explicit memory ownership, scalable quality settings for low-memory phones.

Android's NDK officially supports C/C++ with CMake, and OpenGL ES 3.x is available through the NDK graphics APIs. Device support for later GLES versions must be queried at runtime.

## Build the portable core

```sh
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release
ctest --test-dir build --output-on-failure
```

## Planned milestones

1. **Core foundation** — math, entities, transforms, scene lifecycle, tests.
2. **Android platform layer** — lifecycle, touch input, surface and frame loop.
3. **Renderer** — GLES backend, shaders, mesh buffers, camera, depth and lighting.
4. **3D content** — sphere/mesh primitives, materials, texture loading and asset manifest.
5. **World systems** — scene serialization, resource cache, basic physics and audio interfaces.
6. **Editor** — lightweight desktop scene editor and project inspector.
7. **Galaxy Explorer port** — rebuild the solar-system experience on this runtime.
8. **APK pipeline** — reproducible ARM64 APK build and device validation.

No Unity license, paid service, or hosted game-engine platform is required for the engine source itself. Builds can be run locally or in a repository CI runner; the CI runner is only a build machine, not an engine dependency.
