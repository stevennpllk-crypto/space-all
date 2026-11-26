# Build the first Android APK without a local computer

This route uses **Unity Build Automation (UBA)** to run Unity 6 on Unity's cloud build machines. It does not require installing Unity on an Android phone or using GitHub Actions Unity-license secrets. Unity documents Android as a supported UBA target, and its configuration screen lets you select the Unity version, Android SDK, source-control branch, and APK output settings.

## Cost and limits

Unity Personal / Unity Cloud Personal includes **200 Windows build minutes per month**, plus 5 GB of DevOps storage. Build minutes are metered; do not enable pay-as-you-go or add a payment method. Check the current usage meter before starting. Unity's official Android SDK compatibility table lists SDK 35 and SDK 36 for Unity 6000.0.18–6000.0.37, which includes this project's pinned editor 6000.0.32f1. Choose the highest SDK offered by the UBA target configuration (currently SDK 36 if available).

Official references:
- Unity Personal: https://unity.com/products
- Unity Cloud Personal / DevOps free tier: https://activation.unity3d.com/products/unity-cloud/faq
- Unity Build Automation: https://docs.unity.com/en-us/build-automation
- Connect GitHub source control: https://docs.unity.com/en-us/build-automation/get-started-with-build-automation/connect-your-version-control-system

## One-time setup from a phone browser

1. Sign in to https://cloud.unity.com/ with the Unity ID that owns the project. If you do not have one, create a Unity ID and choose the free Personal tier.
2. Open **DevOps → Build Automation**. Create or select a Unity Cloud project for Galaxy Explorer.
3. In Build Automation, create a build configuration/target.
4. Choose **GitHub** as the source-control provider. This repository is public, so use the public repository URL:
   `https://github.com/stevennpllk-crypto/space-all`
   Select branch `main`. Do not create a token for this public repository unless Unity's current UI explicitly requires one.
5. Set the project subdirectory to blank (the Unity project is at the repository root). Set Unity version to auto-detect; `ProjectSettings/ProjectVersion.txt` pins `6000.0.32f1`.
6. Select **Android** as the build platform. Select Android SDK 36 if offered for this Unity version; otherwise select the highest compatible SDK shown by the dashboard. Configure an APK build (not an Android App Bundle), and ARM64 architecture if the target settings expose it.
7. Save the configuration and manually start its first build. Keep auto-build disabled until the first test succeeds, to conserve the free build minutes.
8. When the build completes, download the APK from the build result page and install it on the Android test device.

## Project-specific notes

- The editor build entry point is `GalaxyAndroidBuild.Build` in `Assets/Editor/GalaxyAndroidBuild.cs`; it sets Android landscape, ARM64, IL2CPP and creates the starter scene before building.
- UBA's standard dashboard build uses its configured build target and project settings; it does not automatically invoke the GameCI custom method `GalaxyAndroidBuild.Build`. The project includes an Editor initialization hook that creates and registers the starter scene when Unity imports it. This behavior has not yet been verified in a real UBA run. If the first UBA build reports no scenes or a project configuration error, stop and inspect the build log before changing settings.
- The current GitHub Actions workflow remains available as a separate route, but it still needs Unity activation secrets and is not the phone-only route.

## Safety / billing

Do not enable a paid consumption plan, add a payment card, or accept paid upgrades for this project. If the dashboard shows that the free quota is exhausted or asks for payment, stop the build and report the exact message instead of proceeding.

A cloud build is not complete until UBA reports success and an APK file is available to download. Source validation alone is not an APK build.
