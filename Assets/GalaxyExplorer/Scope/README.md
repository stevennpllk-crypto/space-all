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

## Atmosphere and cloud effects

`PlanetAtmosphereEffects` adds transparent outer shells to the terrestrial planets and ice/gas giants, plus slowly rotating stylized cloud shells for Earth, Venus, Jupiter and Saturn. The effect is intentionally lightweight and procedural; it is not a physically based Rayleigh/Mie scattering implementation and does not use photographic texture maps. The runtime attaches this component automatically after creating the planets.

## Natural satellites

`NaturalSatelliteSystem` adds the Moon, Phobos, Deimos, the four Galilean moons, and Titan. Orbital periods are approximate sidereal values; orbit radii and rendered sizes are compressed for legibility and are not to scale.

## Procedural planet surface maps

`PlanetSurfaceTextures` generates deterministic 256×128 color maps at runtime for the eight planets. It uses layered Perlin noise, stylized terrestrial color regions, polar caps on Earth, and banding on the giant planets. The maps are illustrative visual material only: they are not measured satellite imagery, scientifically validated geology, or height/displacement maps. Texture resolution can be adjusted in the component Inspector; the default is intended to keep mobile memory use modest.

## Saturn ring system

`PlanetaryRingSystem` adds a generated annulus mesh around Saturn, with a low-resolution radial band texture, a stylized Cassini-like gap, adjustable opacity, and an Inspector-controlled ring-plane tilt. The ring geometry is intentionally lightweight for mobile rendering. Its dimensions, band structure, and opacity are illustrative and are not a physically accurate particle simulation.

## Deep-space nebula backdrop

`DeepSpaceNebulaBackdrop` creates a low-opacity, procedural violet/blue nebula texture on a large transparent sphere surrounding the scene. The texture is generated once at startup using layered Perlin noise and uses a modest 512×256 default resolution. The effect is illustrative rather than a real astronomical image or volumetric gas simulation. `maximumOpacity` and texture dimensions can be adjusted in the Inspector; keep opacity conservative so the starfield and planet silhouettes remain readable on mobile devices.


## Procedural Milky Way backdrop

`ProceduralGalaxyBackdrop` creates a stylized spiral galaxy from one point mesh (9,000 stars by default), with a denser central bulge, four loose spiral arms, deterministic placement, and configurable radius, tilt, thickness, and visibility. It avoids creating one GameObject per star and is intended to keep CPU and scene-object overhead low on mobile. The point rendering appearance depends on the active Unity render pipeline and graphics device, so it still requires an in-Editor/device visual check. This is an artistic procedural distribution, not an astronomically measured Milky Way map.
