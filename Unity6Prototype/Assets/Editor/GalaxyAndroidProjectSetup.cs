#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GalaxyExplorer.Editor
{
    public static class GalaxyAndroidProjectSetup
    {
        [MenuItem("Galaxy Explorer/Configure Android Player Settings")]
        public static void Configure()
        {
            PlayerSettings.companyName = "Galaxy Explorer Studio";
            PlayerSettings.productName = "GALAXY EXPLORER";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.galaxyexplorer.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            Debug.Log("Galaxy Explorer Android settings applied. Select Android in Build Profiles and install the required Unity modules.");
        }
    }
}
#endif