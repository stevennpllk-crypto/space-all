# SOLAR EXPLORER — PSP Homebrew

A native PSP Homebrew game prototype for PPSSPP, developed separately from the Unity 6 project.

## Accepted design
- Realistic astronomical presentation within PSP hardware limits.
- Free exploration plus mission-oriented progression.
- Arabic and English localization target.
- Quality settings prioritized for stable frame pacing in PPSSPP.

## Current implementation milestone
- PSP GU/GUM 3D perspective scene.
- Procedural low-poly shaded 3D spheres for the Sun and eight planets.
- Orbital paths and animated planetary motion.
- Saturn-style ring geometry.
- Starfield, low-poly exploration spacecraft, and free-flight movement.
- Camera distance control and runtime state for mission/planet selection/language toggle.
- Geometry kept intentionally low-poly to reduce fill-rate and memory pressure.

## Controls
- D-pad left/right: rotate craft.
- D-pad up/down: thrust forward/reverse.
- L: precision turn / camera closer.
- R: faster thrust / camera farther.
- CROSS: cycle selected planet.
- TRIANGLE: toggle exploration/mission mode state.
- CIRCLE: toggle language state.
- START: exit.

## Important current limitations
This is an early 3D rendering/gameplay milestone, not a finished commercial game. The mission state and language toggle are currently runtime state only; mission objectives, visible bilingual text, Arabic shaping/font atlas, planet surface textures, landing, save data, and sound still need implementation. Planet distances and sizes are compressed for readability and are not to scale.

## Build
The GitHub Actions workflow `.github/workflows/psp-homebrew.yml` uses PSPDEV/PSPSDK. It uploads `EBOOT.PBP` and a folder artifact preserving the PPSSPP install path. A successful workflow must produce a non-empty `EBOOT.PBP` before the game can be tested in PPSSPP.

## Install in PPSSPP
Download the workflow artifact and copy the file from `build/PSP/GAME/SOLAR_EXPLORER/EBOOT.PBP` to your PPSSPP storage path:
`PSP/GAME/SOLAR_EXPLORER/EBOOT.PBP`

The Unity 6 project remains untouched in the parent repository. This PSP implementation is isolated under `PSPHomebrew/`.
