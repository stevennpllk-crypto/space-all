# SOLAR EXPLORER PSP — implementation roadmap

## Approved product direction
- Realistic astronomy-inspired 3D presentation, optimized for PSP/PPSSPP.
- Open exploration combined with structured missions.
- Arabic and English interface.
- Stable performance takes priority over expensive effects.

## Milestone status

### M1 — Native PSP foundation
- [x] Isolated PSP Homebrew target in PSPHomebrew/
- [x] PSPDEV GitHub Actions build pipeline
- [x] PSP GU/GUM 3D camera and depth-buffer scene
- [x] Procedural shaded sphere mesh for Sun and eight planets
- [x] Orbital motion, orbit lines, Saturn-style ring
- [x] Low-poly spacecraft and directional flight input
- [x] Planet selection and mission target visualization
- [ ] Confirm clean EBOOT.PBP build in Actions
- [ ] Run in PPSSPP and verify controls/rendering

### M2 — Actual game loop
- [ ] Mission objectives with completion conditions
- [ ] Discoveries and planet information
- [ ] Landing approach and surface zones
- [ ] Save/load progress
- [ ] Pause and settings screens

### M3 — Art and localization
- [ ] Procedural or authored planet surface textures
- [ ] Atmosphere and restrained glow effects
- [ ] Arabic bitmap font atlas with right-to-left shaping
- [ ] English and Arabic localized menus and descriptions
- [ ] Original sound effects and ambient audio

### M4 — Performance and release
- [ ] Low/balanced/high graphics profiles
- [ ] Frame-time and memory profiling on PPSSPP
- [ ] Test multiple PPSSPP rendering backends
- [ ] Package EBOOT.PBP and installable folder ZIP
- [ ] Release only after successful build and emulator testing

## Engineering constraints
PSP Homebrew is not a Unity build. The PSP has a fixed hardware budget, so the project uses low-poly geometry, compact textures, limited overdraw, and scalable effects. Distances and planet sizes are compressed for readable gameplay; they are not a physically scaled solar-system model.

## Truthful release gate
A source commit is not a tested game. A downloadable build is only considered ready when GitHub Actions produces a non-empty EBOOT.PBP and the resulting package is verified in PPSSPP.
