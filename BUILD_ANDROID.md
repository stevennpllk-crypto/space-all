# Build the first Android APK

The project targets Unity 6.0.32f1 and builds an ARM64 Android APK with GitHub Actions.

## Required Unity license

GitHub Actions cannot compile this Unity project until a valid Unity license is supplied. The workflow now checks for the `UNITY_LICENSE` secret before starting Unity and stops early if it is missing.

For a Unity Personal license, use the current GameCI activation guidance:

1. On a computer with Unity Hub installed, sign in to the Unity account that owns the project.
2. In Unity Hub, open **Preferences → Licenses → Add** and complete the **Get a free Personal license** flow.
3. Confirm that Unity Hub has actually created the license file (`.ulf`). Typical locations are:
   - Windows: `C:\\ProgramData\\Unity\\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. In the GitHub repository, open **Settings → Secrets and variables → Actions → New repository secret**.
5. Create these three repository secrets exactly as named:
   - `UNITY_LICENSE`: paste the complete contents of the `.ulf` file.
   - `UNITY_EMAIL`: the email address used to sign in to Unity.
   - `UNITY_PASSWORD`: the password for that Unity account. GameCI uses the account credentials together with the Personal license during CI activation.
6. Keep all three values private. Do not commit them to the repository, place them in issues, or send them in chat. Never share your Unity password or license contents with another person. Only add them to the repository's Actions secrets if you are authorized to manage that Unity and GitHub account.

If Unity Hub does not create a Personal license file, use Unity's current support/activation guidance for the account and license type. The older manual `.alf` upload route is not a dependable workaround for Personal licenses; do not use scripts that attempt to bypass Unity's license checks.

## Run the build

Open **Actions → Build Android APK → Run workflow** on the `main` branch after the secret is configured.

On success, the workflow uploads `Build/Android/GalaxyExplorer.apk` as the artifact **Galaxy-Explorer-Android-APK**.

The generated APK is an unsigned/debug-install test build unless a release keystore is configured separately. It is intended for device testing, not Play Store distribution.

## What the workflow verifies

- Unity Editor version: 6000.0.32f1
- Android target and ARM64 architecture
- Unity license secret is non-empty before Unity starts
- APK exists and is non-empty
- SHA-256 checksum is printed in the build log

A successful source-validation workflow is not evidence of a successful Unity compile or APK build.

## Current blocker

The latest Android build attempt stopped at the license preflight because `UNITY_LICENSE` is not configured in repository Actions secrets. No Unity compilation was attempted and no APK was produced. The CI activation also expects `UNITY_EMAIL` and `UNITY_PASSWORD`, which are now documented above. An authorized repository/account owner must add these secrets in GitHub Settings; automation in this chat cannot read or create private GitHub secrets.
