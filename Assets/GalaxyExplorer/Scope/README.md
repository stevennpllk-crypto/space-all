# Solar System Scope prototype

This folder adds a self-contained Unity 6 Solar System Scope prototype to Galaxy Explorer.

## Create the demo scene
1. Open the project in Unity 6.
2. Wait for scripts to compile.
3. Select **Galaxy Explorer → Create Solar System Scope Demo**.
4. Save the generated scene under `Assets/Scenes` (or another project scene folder).
5. Press Play.

## Included in this prototype
- Procedural Sun, eight planets, orbit paths and a Saturn ring representation.
- A deterministic procedural starfield.
- Animated orbital motion and axial rotation using approximate sidereal periods.
- Mouse drag / touch drag camera orbit and scroll / pinch zoom.
- Planet selection with short educational descriptions.
- Pause and simulation-speed reset controls.
- Runtime-generated geometry and materials; no external asset downloads required.

## Scientific and visual limitations
The scene uses compressed orbital distances and non-linear planet sizes so the whole system can be viewed interactively. Orbit paths are circular and the simulation is an educational visualization, not a precision ephemeris. Planet surface meshes are smooth procedural placeholders; NASA/ESA texture maps, moons, dwarf planets, asteroid belts, eclipses and accurate inclination/eccentricity are planned follow-up work.

## Input note
The camera and selection scripts use Unity's legacy `Input` API for broad compatibility. Set **Project Settings → Player → Active Input Handling** to **Both** if the project is configured for the new Input System only, then restart the Editor.

## Status
Source files are committed, but this prototype has not yet been compiled or run in the Unity Editor. An APK must not be considered available until the Unity Android workflow completes successfully.
