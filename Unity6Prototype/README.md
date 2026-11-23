# GALAXY EXPLORER — Unity 6 Prototype 0.1
Source-first Unity foundation for an Android space-flight prototype. This is not an APK and has not yet been built or device-tested.

## Requirements
- Unity 6 (6000.0 LTS recommended)
- Universal 3D / URP project template
- Android Build Support, Android SDK & NDK Tools, OpenJDK

## Generate the playable scene
1. Create a new Universal 3D project in Unity Hub.
2. Copy the contents of this folder into the Unity project root, merging Assets.
3. Wait for scripts to compile.
4. Select Galaxy Explorer > Build Prototype 0.1 Scene.
5. The editor tool generates Assets/Scenes/MainScene.unity and adds it to Build Settings.
6. Press Play to test the scene in the Editor. Use landscape orientation.

## Android build
1. Open File > Build Profiles and select Android; switch platform if prompted.
2. Set Package Name to com.galaxyexplorer.game.
3. Set Default Orientation to Landscape Left.
4. Use IL2CPP and ARM64 for release.
5. Enable Development Build for the first diagnostic APK.
6. Confirm MainScene is included in the active build profile.
7. Build to Builds/Android/GalaxyExplorer_Prototype_0.1.apk.
8. Install on a physical Android device and test launch, touch input, flight, camera toggle, braking and throttle.

## Controls
- Left virtual stick: forward thrust and roll.
- Right virtual stick: pitch and yaw.
- Throttle slider: base forward thrust.
- CAMERA: toggle external / cockpit camera.
- Hold BRAKE: damp velocity.

## Known limitations
- Procedural primitive geometry and flat colors are placeholders, not final NASA textures or production spacecraft art.
- Planet sizes and positions are gameplay-scaled, not astronomical distances.
- Flight uses local Rigidbody motion with gravity disabled; orbital mechanics, planetary landing, atmospheric flight and astronaut mode are later milestones.
- This first foundation uses Unity UI/EventSystem touch handlers for compatibility. A dedicated Input System action map is a follow-up.
- URP renderer and Android graphics API still require target-device verification.
- No successful Unity build or device test is claimed at this stage.
