# Development roadmap

## 0.1 — Portable core
- [x] C++20 core layout
- [x] Vec3 and Transform primitives
- [x] Minimal scene node storage and hierarchy
- [x] CMake test target
- [ ] Verify build and tests in CI

## 0.2 — Android integration
- [ ] Android Gradle application shell
- [ ] Android NDK CMake build
- [ ] JNI lifecycle bridge
- [ ] OpenGL ES 3.0 surface smoke test

## 0.3 — Rendering
- [ ] Matrix types and camera
- [ ] Vertex/index buffers and shader/material abstraction
- [ ] Depth testing, resize handling and frame timing

## 0.4 — Assets and input
- [ ] Procedural mesh primitives
- [ ] glTF 2.0 loading
- [ ] Touch/controller input abstraction

## 0.5 — Engine systems
- [ ] Collision queries and basic physics
- [ ] Audio abstraction
- [ ] Scene serialization and editor prototype

## 1.0 — Demonstration
- [ ] Solar Explorer sample scene
- [ ] Android performance profiling
- [ ] Reproducible APK build and documented device requirements

Milestones are complete only after code is built and tests pass in CI.
