# Build the first Android APK without a local computer

This route uses **Unity Build Automation (UBA)** to run Unity 6 on Unity's cloud build machines. It does not require installing Unity on an Android phone or using GitHub Actions Unity-license secrets.

## Cost and limits

Unity's Unity Personal / Unity Cloud Personal offering includes a free DevOps tier with **200 Windows build minutes per month**. Android is supported on the Windows 11 24H2 builder. Check the current usage meter in Unity Dashboard before starting; do not enable pay-as-you-go or add a payment method just to build this test APK.

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
6. Select **Android** as the build platform and a Windows 11 24H2 builder. Configure an APK build (not an Android App Bundle) and ARM64 architecture if the target settings expose it.
7. Save the configuration and manually start its first build. Keep auto-build disabled until the first test succeeds, to conserve the free build minutes.
8. When the build completes, download the APK from the build result page and install it on the Android test device.

## Project-specific notes

- The editor build entry point is `GalaxyAndroidBuild.Build` in `Assets/Editor/GalaxyAndroidBuild.cs`; it sets Android landscape, ARM64, IL2CPP and creates the starter scene before building.
- UBA's standard dashboard build may use the scenes listed in Editor Build Settings rather than the GameCI custom build method. The project has an Editor initialization hook that creates and registers the starter scene when Unity imports it. If the first UBA build reports no scenes or a project configuration error, stop and inspect the build log before changing settings.
- The current GitHub Actions workflow remains available as a separate route, but it still needs Unity activation secrets and is not the phone-only route.

## Safety / billing

Do not enable a paid consumption plan, add a payment card, or accept paid upgrades for this project. If the dashboard shows that the free quota is exhausted or asks for payment, stop the build and report the exact message instead of proceeding.

A cloud build is not complete until UBA reports success and an APK file is available to download. Source validation alone is not an APK build.
