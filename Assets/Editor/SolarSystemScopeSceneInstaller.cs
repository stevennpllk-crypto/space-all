#if UNITY_EDITOR
using GalaxyExplorer.Scope;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GalaxyExplorer.Editor
{
    public static class SolarSystemScopeSceneInstaller
    {
        [MenuItem("Galaxy Explorer/Create Solar System Scope Demo")]
        public static void CreateDemoScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Solar System Scope");
            root.AddComponent<SolarSystemScopeRuntime>();

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 16f, -34f);
            cameraObject.transform.LookAt(Vector3.zero);
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<SolarSystemScopeCamera>();

            EditorSceneManager.MarkSceneDirty(scene);
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Solar System Scope Scene",
                "SolarSystemScope",
                "unity",
                "Choose where to save the generated demo scene.");
            if (!string.IsNullOrEmpty(path))
            {
                EditorSceneManager.SaveScene(scene, path);
                AssetDatabase.Refresh();
                Selection.activeGameObject = root;
                Debug.Log("Solar System Scope demo scene created at: " + path);
            }
        }
    }
}
#endif
