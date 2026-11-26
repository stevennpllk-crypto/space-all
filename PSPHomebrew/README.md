# SOLAR EXPLORER — PSP Homebrew

This folder is a separate PSP-compatible prototype for PPSSPP. It does not convert the Unity project into a PSP game; it is a native PSP Homebrew renderer built with PSPDEV/PSPSDK.

## Build
The GitHub Actions workflow `.github/workflows/psp-homebrew.yml` builds this folder in the PSPDEV container. The expected output is `EBOOT.PBP`, plus a ZIP arranged for PPSSPP.

## Install in PPSSPP
Extract the generated ZIP, preserving this structure:

```
PSP/GAME/SOLAR_EXPLORER/EBOOT.PBP
```

Open PPSSPP → Games → `SOLAR_EXPLORER`.

## Prototype controls
- D-pad: move the exploration marker
- L / R: zoom out / in
- START: exit

## Scope and limitations
This first milestone is a lightweight 2D orbital visualization using the PSP graphics hardware interface: animated stars, orbit ellipses, a sun, orbiting planets, and a movable exploration marker. It is not yet a fully 3D planet landing simulator. Later milestones can add textured 3D spheres, a spacecraft model, menus, Arabic bitmap-font rendering, sound, and exploration missions.

The Unity 6 project remains untouched in the parent repository. This PSP implementation is isolated under `PSPHomebrew/`.
