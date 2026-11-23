#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Explicit editor commands for configuring the prepared Android project.</summary>
public static class GalaxyProjectSetup
{
    [MenuItem("Galaxy Explorer/Setup/Apply Android Defaults")]
    public static void ApplyAndroidDefaults()
    {
        PlayerSettings.companyName = "Galaxy Explorer Studio";
        PlayerSettings.productName = "Galaxy Explorer";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.galaxyexplorer.mobile");
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.activeInputHandler = PlayerSettings.ActiveInputHandler.Both;
        Debug.Log("Galaxy Explorer Android defaults applied. No build was started.");
    }

    [MenuItem("Galaxy Explorer/Setup/Show Preparation Status")]
    public static void ShowPreparationStatus()
    {
        EditorUtility.DisplayDialog(
            "Galaxy Explorer",
            "Preparation mode is active. Android defaults can be applied from this menu. No APK is built until the project owner requests the جمع stage.",
            "OK");
    }
}
#endif
