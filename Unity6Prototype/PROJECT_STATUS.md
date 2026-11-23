# Prototype 0.1 status

## Source implemented
- [x] Unity 6-oriented source layout
- [x] Editor scene generator with high-detail planet mesh generation
- [x] Cinematic URP post-processing volume (Bloom, ACES, color grading, vignette)
- [x] High-quality lighting, soft shadows, HDR/MSAA camera defaults and expanded starfield
- [x] Android high-visual-quality player defaults (Linear, 4× MSAA setting, high shadows, anisotropic filtering, LOD bias)
- [x] Sun, Earth, Moon and Mars placeholder bodies
- [x] Explorer Rigidbody flight controller
- [x] Configurable spherical gravity sources for Earth, Moon and Mars
- [x] Follow camera with first-/third-person toggle
- [x] Dual virtual joystick UI with independent pointer tracking
- [x] Throttle and brake controls
- [x] Flight HUD with nearest-body altitude readout
- [x] Gentle-touchdown detection, landed hold and throttle takeoff release
- [x] Editor structure validator for ship, Rigidbody, camera, HUD, EventSystem, celestial bodies and gravity sources

## Still requires Unity Editor / device verification
- [ ] Import into Unity 6 and resolve editor-version API differences
- [ ] Generate scene in Unity Editor
- [ ] Run Play Mode checks, including gravity falloff, landing thresholds and multi-touch input
- [ ] Verify URP asset HDR/MSAA settings and profile GPU/memory cost on the target Android device
- [ ] Build APK
- [ ] Install and test on a physical Android device

## Scope notes
Gravity is a gameplay-scale inverse-square approximation using configurable surface acceleration, body radius and influence radius. It is not an N-body orbital simulation. A basic landing assist detects low-speed, low-radial-speed, near-upright touchdown and holds the craft; throttle releases the hold for takeoff. It is not terrain-aware and still needs Play Mode tuning. Surface movement and astronaut mode are not implemented.

Unchecked items must not be represented as complete without evidence.
