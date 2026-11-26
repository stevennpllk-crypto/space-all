# JASSIM Engine

JASSIM Engine is an independent, mobile-first 3D engine project. It is not a Unity fork and does not use Unity runtime or editor code.

## Current milestone: portable core v0.5

Implemented:
- C++17 vector math (dot/cross), column-major 4x4 matrices, and scene/entity storage;
- a free-flight 3D camera with view, perspective projection, and local-space movement;
- type-safe service registry with explicit shared ownership;
- synchronous typed event bus with subscription removal;
- conservative Low / Medium / High device quality profiles;
- fixed-capacity, allocation-free multi-touch state with press, move, release, and frame-reset transitions;
- bounded fixed-step frame clock with delta clamping and catch-up limits;
- CMake build and automated smoke tests covering these systems.

The quality selector treats unknown device metrics conservatively. A device with 3 GB RAM is assigned the Low profile by design; this is a starting policy, not a measured performance guarantee.

This is **not yet a complete 3D engine** and does not produce an APK. Android lifecycle/surface integration, an OpenGL ES renderer backend, asset loading, physics, editor tooling, and APK packaging remain future milestones.

## Technical direction

- Core language: C++17.
- Build system: CMake.
- Android target: ARM64 first, with Android NDK.
- Rendering backend: OpenGL ES 3.x initially, with runtime capability checks and a conservative fallback path.
- App integration: Android NativeActivity or a thin Android Activity/JNI shell, selected after the renderer prototype.
- Design goal: small runtime, explicit memory ownership, scalable quality settings for low-memory phones.

Android's NDK supports C/C++ with CMake. The runtime must query graphics capabilities on the actual device rather than assuming a particular GLES version.

## Build the portable core

From the repository root:

```sh
cmake -S engine -B build/engine -DCMAKE_BUILD_TYPE=Release
cmake --build build/engine --parallel 2
ctest --test-dir build/engine --output-on-failure
```

## Planned milestones

1. **Portable core** — math, scene/entity lifecycle, services, events, quality profiles, touch input, frame timing.
2. **Android platform layer** — lifecycle, native surface, touch event forwarding, and frame loop.
3. **Renderer** — GLES backend, shader programs, mesh buffers, camera integration, depth and lighting.
4. **3D content** — sphere/mesh primitives, materials, texture loading and asset manifest.
5. **World systems** — scene serialization, resource cache, basic physics and audio interfaces.
6. **Editor** — lightweight desktop scene editor and project inspector.
7. **Galaxy Explorer port** — rebuild the solar-system experience on this runtime.
8. **APK pipeline** — reproducible ARM64 APK build and device validation.

No Unity license, paid service, or hosted game-engine platform is required for the engine source itself. CI is currently used as a build/test machine, not as an engine dependency.
