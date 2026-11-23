# Build the first Android APK

The repository uses Unity 6.0.32f1 and GitHub Actions to produce an ARM64 Android APK.

## One-time Unity license setup

The first automated build stopped before Unity could open the project because the repository has no Unity license secret configured.

1. Activate Unity Personal in Unity Hub on a device where you can sign in to your Unity account.
2. Obtain the Unity license file (`.ulf`) using Unity's official manual activation flow if needed.
3. Open this GitHub repository, then go to **Settings → Secrets and variables → Actions**.
4. Choose **New repository secret**.
5. Set the name to `UNITY_LICENSE` and paste the complete contents of the Unity license file as the secret value.
6. Do not commit the license file, publish it, or paste it into chat. Do not add your Unity password or account credentials to the repository.

## Run the build

Open **Actions → Build Android APK → Run workflow** on the `main` branch. The workflow builds an ARM64 APK at `Build/Android/GalaxyExplorer.apk` and uploads it as the artifact **Galaxy-Explorer-Android-APK** when successful.

The generated APK is an unsigned/debug-install test build unless a release keystore is configured separately. It is intended for device testing, not Play Store distribution.

## What the workflow verifies

- Unity Editor version: 6000.0.32f1
- Android target and ARM64 architecture
- APK exists and is non-empty
- SHA-256 checksum is printed in the build log

A successful source-validation workflow is not evidence of a successful Unity compile or APK build.
