# JASSIM Engine

JASSIM Engine is an independent, mobile-first 3D engine project. It is not a Unity fork and does not use Unity runtime or editor code.

## Current milestone: Android renderer prototype v0.8

Implemented:
- C++17 vector math (dot/cross), column-major 4x4 matrices, and scene/entity storage;
- a free-flight 3D camera with view, perspective projection, and local-space movement;
- an Android NativeActivity entry point with EGL lifecycle handling and an OpenGL ES 3 renderer;
- runtime GLSL ES 3 shader compilation/linking with diagnostic logging;
- vertex-array and vertex-buffer setup, interleaved position/color mesh data, depth testing, and camera view-projection uniform upload;
- a first procedural RGB triangle mesh rendered through the engine's camera pipeline;
- native multi-touch event forwarding for pointer down/move/up/cancel into the engine input state;
- an Android manifest declaring GLES 3.0 and the native engine library;
- type-safe service registry with explicit shared ownership;
- synchronous typed event bus with subscription removal;
- conservative Low / Medium / High device quality profiles;
- fixed-capacity, allocation-free multi-touch state with press, move, release, and frame-reset transitions;
- bounded fixed-step frame clock with delta clamping and catch-up limits;
- CMake build and automated smoke tests covering portable core systems;
- reproducible Android ARM64 debug APK packaging in GitHub Actions.

The current renderer is intentionally a minimal triangle prototype. It proves the GLES shader -> vertex buffer -> camera matrix -> draw-call path, not production-ready scene rendering. No 3D asset importer, texture/material system, lighting, physics, or editor is implemented yet.

The quality selector treats unknown device metrics conservatively. A device with 3 GB RAM is assigned the Low profile by design; this is a starting policy, not a measured performance guarantee.

## Technical direction

- Core language: C++17.
- Build system: CMake.
- Android target: ARM64 first, with Android NDK.
- Rendering backend: OpenGL ES 3.x initially, with runtime capability checks and a conservative fallback path.
- App integration: Android NativeActivity for the first native runtime.
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
2. **Android platform layer** — NativeActivity lifecycle, EGL surface/context, GLES3 frame loop and touch event forwarding.
3. **Renderer prototype** — shader programs, mesh buffers, camera integration and depth testing; first triangle prototype is implemented.
4. **Scene renderer** — reusable mesh/material resources, texture loading, render queues and lighting.
5. **3D content** — sphere/mesh primitives, procedural planet meshes and asset manifest.
6. **World systems** — scene serialization, resource cache, basic physics and audio interfaces.
7. **Editor** — lightweight project and scene authoring tools.
8. **Galaxy Explorer port** — rebuild the solar-system experience on this runtime.
9. **Device validation** — install and profile the APK on physical ARM64 Android devices, then optimize memory and frame pacing.

No Unity license, paid service, or hosted game-engine platform is required for the engine source itself. CI is currently used as a build/test machine, not as an engine dependency.
