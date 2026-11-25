# Solar System Scope prototype

This folder adds a self-contained Unity 6 Solar System Scope prototype to Galaxy Explorer.

## Create the demo scene
1. Open the project in Unity 6.
2. Wait for scripts to compile.
3. Select **Galaxy Explorer → Setup → Create or Repair Starter Scene**.
4. Open `Assets/Scenes/GalaxyExplorer.unity` (the installer also adds it to Build Settings).
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

`PlanetSurfaceTextures` generates deterministic 256×128 color maps at runtime for the eight planets. It uses layered Perlin noise, stylized terrestrial color regions, polar caps on Earth, and banding on the giant planets. Longitude sampling wraps around a circular domain so the left and right edges of each equirectangular map meet without the obvious vertical seam common in simple 2D noise maps. Mild anisotropic filtering improves texture readability at grazing angles, and missing planet materials are handled safely. The maps are illustrative visual material only: they are not measured satellite imagery, scientifically validated geology, or height/displacement maps. Texture resolution can be adjusted in the component Inspector; the default is intended to keep mobile memory use modest.

## Saturn ring system

`PlanetaryRingSystem` adds a generated annulus mesh around Saturn, with a low-resolution radial band texture, a stylized Cassini-like gap, adjustable opacity, and an Inspector-controlled ring-plane tilt. The ring geometry is intentionally lightweight for mobile rendering. Its dimensions, band structure, and opacity are illustrative and are not a physically accurate particle simulation.

## Deep-space nebula backdrop

`DeepSpaceNebulaBackdrop` creates a low-opacity, procedural violet/blue nebula texture on a large transparent sphere surrounding the scene. The texture is generated once at startup using layered Perlin noise and uses a modest 512×256 default resolution. The effect is illustrative rather than a real astronomical image or volumetric gas simulation. `maximumOpacity` and texture dimensions can be adjusted in the Inspector; keep opacity conservative so the starfield and planet silhouettes remain readable on mobile devices.


## Procedural Milky Way backdrop

`ProceduralGalaxyBackdrop` creates a stylized spiral galaxy from one point mesh (9,000 stars by default), with a denser central bulge, four loose spiral arms, deterministic placement, and configurable radius, tilt, thickness, and visibility. It avoids creating one GameObject per star and is intended to keep CPU and scene-object overhead low on mobile. The point rendering appearance depends on the active Unity render pipeline and graphics device, so it still requires an in-Editor/device visual check. This is an artistic procedural distribution, not an astronomically measured Milky Way map.


## Nearby star destinations

`NearbyStarCatalog` adds five selectable educational destinations: Sirius, Proxima Centauri, Alpha Centauri A, Vega, and Betelgeuse. Selecting a catalog entry focuses the scope camera on its star and displays a short spectral classification and fact. Their scene positions and rendered radii are deliberately illustrative so the destinations can be explored in this compressed prototype; they are not a 3D astrometric map and must not be interpreted as true interstellar navigation coordinates. The catalog creates only five star objects and does not create a large per-star object hierarchy.


## Free-flight exploration controls

The scope camera is a free exploration camera; there is no spacecraft or vehicle required.
- Desktop: left mouse drag orbits the view, right/middle drag pans through space, mouse wheel zooms, WASD moves through the scene, Q/E moves vertically, and Shift increases flight speed.
- Touch: one finger orbits, two-finger pinch zooms, and two-finger movement pans the view.
- Selecting a planet, moon, or catalog destination focuses the camera on that object. The focus transition is smoothed by the camera controller.

The scene uses compressed distances. Free-flight movement is a navigation aid, not a physical spacecraft simulation.

## Offline operation and local progress

The core exploration scene is generated from project assets and runtime code; it does not require a remote API or a live service to create the Solar System, navigate the free-flight camera, or inspect the built-in destinations. NASA image assets included under `Assets/Art/NASA` are bundled with the project and remain available offline after installation.

The following progress is stored locally with Unity PlayerPrefs:
- Discovered-object journal entries.
- Graphics quality preference.
- Free-flight camera focus, target focus, yaw, pitch, current distance, and target zoom distance. Camera state is saved periodically and when the app is paused or closed.

An internet connection is still required for downloading/installing the app, remote repository access, cloud builds, and any future optional online content. This offline design has source-level checks; it must still be verified in a built APK by launching the app in airplane mode. Do not treat source validation as proof of a successful Unity build or device test.

## Optional VR mode

`GalaxyExplorerVRSupport` exposes VR runtime status and configures the camera for both eyes. Head pose is read from Unity XR when an XR runtime is active. A working headset experience still requires an OpenXR-compatible XR Plug-in Management loader and the correct provider settings for the target headset; this repository does not claim headset validation. The standard non-VR camera controls remain available when XR is inactive. Test comfort, frame rate, and controller interaction on the actual target headset before release.


## Smooth navigation and deep-space destinations

Camera focus now eases toward selected targets instead of snapping the focus point. Zoom changes also ease toward their requested distance. The deep-space destination catalog adds selectable illustrative markers for the Orion Nebula, Crab Nebula, Andromeda Galaxy, and Sombrero Galaxy alongside the nearby-star catalog.

These markers are educational navigation landmarks: their scene positions, apparent sizes, and simple sphere visuals are not astronomical scale models or scientific nebula/galaxy renderings. The free-flight camera remains the navigation method; no spacecraft is spawned.
