# JASSIM ENGINE 3D

A modular, Android-first 3D engine foundation with a portable C++20 core.

## Bootstrap 0.1
- Portable C++20 core, no third-party dependencies.
- Vec3 and Transform primitives.
- Minimal scene graph with numeric node IDs.
- CMake build and native smoke tests.

This is a foundation, not yet a complete editor or game engine. It does not yet render graphics, import assets, simulate physics, or produce an APK.

## Build
Requirements: CMake 3.20+ and a C++20 compiler.

    cmake -S . -B build -DJASSIM_ENGINE_BUILD_TESTS=ON
    cmake --build build
    ctest --test-dir build --output-on-failure

Android NDK/JNI and OpenGL ES integration are planned as a separate layer. See docs/ROADMAP.md.
