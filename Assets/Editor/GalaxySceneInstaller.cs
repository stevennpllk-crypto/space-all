#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates a minimal playable Unity scene asset during Editor import.
/// This only prepares source assets; it never invokes a player build.
/// </summary>
[InitializeOnLoad]
public static class GalaxySceneInstaller
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ScenePath = SceneFolder + "/GalaxyExplorer.unity";

    static GalaxySceneInstaller()
    {
        EditorApplication.delayCall += EnsureStarterScene;
    }

    [MenuItem("Galaxy Explorer/Setup/Create or Repair Starter Scene")]
    public static void EnsureStarterScene()
    {
        if (!Directory.Exists(SceneFolder)) Directory.CreateDirectory(SceneFolder);

        if (!File.Exists(ScenePath))
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            GameObject defaultLight = GameObject.Find("Directional Light");
            if (defaultLight != null) Object.DestroyImmediate(defaultLight);

            // The default Unity camera starts almost level with the orbital plane.
            // Give the Scope runtime a useful elevated overview before its camera
            // controller captures the initial orbit state in Awake.
            Camera starterCamera = Camera.main;
            if (starterCamera != null)
            {
                starterCamera.transform.position = new Vector3(0f, 16f, -34f);
                starterCamera.transform.LookAt(Vector3.zero);
                starterCamera.fieldOfView = 60f;
                starterCamera.nearClipPlane = 0.1f;
                starterCamera.farClipPlane = 600f;
            }

            // Start the real eight-planet Solar System Scope runtime.
            // GalaxyWorldBootstrap remains a fallback for scenes without this component.
            var scopeRoot = new GameObject("Solar System Scope Runtime");
            scopeRoot.AddComponent<GalaxyExplorer.Scope.SolarSystemScopeRuntime>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
            Debug.Log("Galaxy Explorer starter scene created. No player build was started.");
        }

        string guid = AssetDatabase.AssetPathToGUID(ScenePath);
        if (!string.IsNullOrEmpty(guid))
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
#endif
