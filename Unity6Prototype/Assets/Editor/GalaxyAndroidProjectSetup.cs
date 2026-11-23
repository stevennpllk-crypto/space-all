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

            // High visual preset; validate GPU cost on the target device before release.
            PlayerSettings.colorSpace = ColorSpace.Linear;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowDistance = 120f;
            QualitySettings.shadowCascades = 4;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.lodBias = 2f;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.vSyncCount = 0;
            Debug.Log("Galaxy Explorer high-quality Android defaults applied. Confirm URP asset MSAA/HDR and profile GPU cost on target hardware.");
        }
    }
}
#endif