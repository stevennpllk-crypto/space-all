# JASSIM Engine

JASSIM Engine is an independent, mobile-first 3D engine project. It is not a Unity fork and does not use Unity runtime or editor code.

## Current milestone: procedural 3D renderer v0.8

Implemented:
- C++17 vector math (dot/cross), column-major 4x4 matrices, and scene/entity storage;
- a free-flight 3D camera with view, perspective projection, and local-space movement;
- Android NativeActivity lifecycle with EGL and OpenGL ES 3;
- runtime GLSL ES 3 shader compilation/linking with diagnostic logging;
- reusable indexed mesh data and a procedural UV-sphere generator;
- position and normal vertex attributes, indexed GPU buffers, depth testing and camera view-projection upload;
- per-fragment directional diffuse lighting with ambient contribution;
- an initial blue sphere rendered using the procedural mesh pipeline;
- native multi-touch event forwarding for pointer down/move/up/cancel into the engine input state;
- type-safe service registry, synchronous typed event bus, conservative device quality profiles, bounded fixed-step frame clock and fixed-capacity touch input;
- portable CMake smoke tests, including sphere topology, vertex-normal and index-bound checks;
- reproducible Android ARM64 debug APK packaging in GitHub Actions.

The sphere is a procedural renderer milestone, not a finished planet system. Texture mapping, physically based materials, atmosphere, shadows, scene render queues, asset import, physics and editor tooling are not implemented yet.

The quality selector treats unknown device metrics conservatively. A device with 3 GB RAM is assigned the Low profile by design; this is a starting policy, not a measured performance guarantee.

## Technical direction

- Core language: C++17.
- Build system: CMake.
- Android target: ARM64 first, with Android NDK.
- Rendering backend: OpenGL ES 3.x initially.
- App integration: Android NativeActivity for the first native runtime.
- Design goal: small runtime, explicit memory ownership, scalable quality settings for low-memory phones.

## Build the portable core

```sh
cmake -S engine -B build/engine -DCMAKE_BUILD_TYPE=Release
cmake --build build/engine --parallel 2
ctest --test-dir build/engine --output-on-failure
```

## Planned milestones

1. **Portable core** — math, scene/entity lifecycle, services, events, quality profiles, touch input and frame timing.
2. **Android platform layer** — NativeActivity lifecycle, EGL surface/context and touch forwarding.
3. **Procedural renderer** — indexed meshes, shaders, camera integration and directional lighting; initial UV sphere is implemented.
4. **Planet materials** — UV coordinates, texture sampling, material parameters and asset loading.
5. **Scene renderer** — reusable render components, render queues, resource cache and lighting extensions.
6. **World systems** — scene serialization, basic physics and audio interfaces.
7. **Editor** — lightweight project and scene authoring tools.
8. **Galaxy Explorer port** — rebuild the solar-system experience on this runtime.
9. **Device validation** — install and profile the APK on physical ARM64 Android devices, then optimize memory and frame pacing.

No Unity license, paid service, or hosted game-engine platform is required for the engine source itself. CI is used as a build/test machine, not as an engine dependency.
