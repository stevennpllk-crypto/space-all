# GALAXY EXPLORER — Unity 6 Prototype 0.1

Source-first Unity foundation for an Android space-flight prototype. This is not an APK and has not yet been built or device-tested.

## Requirements
- Unity 6 (6000.0 LTS recommended)
- Universal 3D / URP project template
- Android Build Support, Android SDK & NDK Tools, OpenJDK

## Generate the scene
1. Create a new Universal 3D project in Unity Hub.
2. Copy the contents of this folder into the Unity project root, merging the Assets and Packages folders.
3. Wait for Unity to compile the scripts.
4. Select **Galaxy Explorer > Build Prototype 0.1 Scene**.
5. The editor tool generates `Assets/Scenes/MainScene.unity` and adds it to Build Settings.
6. Select **Galaxy Explorer > Configure Android Player Settings** to apply the package identifier, landscape orientation, IL2CPP and ARM64 defaults.
7. Select **Galaxy Explorer > Validate Prototype 0.1** for a scene-structure check.
8. Press Play to test in the Editor. Use landscape orientation.

## Android build
1. Open File > Build Profiles and select Android; switch platform if prompted.
2. Confirm the package name is `com.galaxyexplorer.game`.
3. Confirm Default Orientation is Landscape Left.
4. Use IL2CPP and ARM64 for release.
5. Enable Development Build for the first diagnostic APK.
6. Confirm MainScene is included in the active build profile.
7. Build to `Builds/Android/GalaxyExplorer_Prototype_0.1.apk`.
8. Install on a physical Android device and test launch, multi-touch input, flight, camera toggle, braking and throttle.

## Controls
- Left virtual stick: forward thrust and roll.
- Right virtual stick: pitch and yaw.
- Throttle slider: base forward thrust.
- CAMERA: toggle cockpit / external camera.
- Hold BRAKE: damp velocity.

## Known limitations
- Procedural primitive geometry and flat colors are placeholders, not final NASA textures or production spacecraft art.
- Planet sizes and positions are gameplay-scaled, not astronomical distances.
- Flight currently uses local Rigidbody motion with gravity disabled; orbital mechanics, planetary landing, atmospheric flight and astronaut mode are later milestones.
- The scene uses Unity UI/EventSystem pointer handlers; it does not yet use a dedicated Input System action map.
- URP renderer, particle starfield and Android graphics settings still require target-device verification.
- No successful Unity build, runtime test or APK is claimed at this stage.
