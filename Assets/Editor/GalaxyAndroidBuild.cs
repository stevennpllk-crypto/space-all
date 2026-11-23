#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class GalaxyAndroidBuild
{
    public static void Build()
    {
        const string scenePath = "Assets/Scenes/GalaxyExplorer.unity";
        const string outputPath = "Build/Android/GalaxyExplorer.apk";

        GalaxyProjectSetup.ApplyAndroidDefaults();
        GalaxySceneInstaller.EnsureStarterScene();

        if (!File.Exists(scenePath))
            throw new FileNotFoundException("Starter scene was not generated.", scenePath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("Galaxy Explorer Android APK build started.");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Galaxy Explorer build result: " + report.summary.result
            + "; errors=" + report.summary.totalErrors
            + "; warnings=" + report.summary.totalWarnings
            + "; size=" + report.summary.totalSize + " bytes");

        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Android APK build failed: " + report.summary.result);

        if (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0)
            throw new System.Exception("Build reported success but the APK is missing or empty.");

        Debug.Log("APK_READY: " + Path.GetFullPath(outputPath));
    }
}
#endif
