# Prototype 0.1 status

## Source implemented
- [x] Unity 6-oriented source layout
- [x] Editor scene generator
- [x] Sun, Earth, Moon and Mars placeholder bodies
- [x] Explorer Rigidbody flight controller
- [x] Configurable spherical gravity sources for Earth, Moon and Mars
- [x] Follow camera with first-/third-person toggle
- [x] Dual virtual joystick UI with independent pointer tracking
- [x] Throttle and brake controls
- [x] Flight HUD
- [x] Editor structure validator for ship, Rigidbody, camera, HUD, EventSystem, celestial bodies and gravity sources

## Still requires Unity Editor / device verification
- [ ] Import into Unity 6 and resolve editor-version API differences
- [ ] Generate scene in Unity Editor
- [ ] Run Play Mode checks, including gravity falloff and multi-touch input
- [ ] Configure URP Android renderer and build profiles
- [ ] Build APK
- [ ] Install and test on a physical Android device

## Scope notes
Gravity is a gameplay-scale inverse-square approximation using configurable surface acceleration, body radius and influence radius. It is not an N-body orbital simulation. Planet collision exists through primitive colliders, but controlled landing, surface movement and astronaut mode are not implemented yet.

Unchecked items must not be represented as complete without evidence.
