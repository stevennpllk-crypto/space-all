#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GalaxyExplorer.Spacecraft;
using GalaxyExplorer.Cameras;
using GalaxyExplorer.World;

namespace GalaxyExplorer.Editor
{
    public static class GalaxyPrototypeValidator
    {
        [MenuItem("Galaxy Explorer/Validate Prototype 0.1")]
        public static void Validate()
        {
            const string path = "Assets/Scenes/MainScene.unity";
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(path))
            {
                Debug.LogError("Validation failed: MainScene is missing. Run Build Prototype 0.1 Scene first.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            bool shipFound = false, cameraFound = false;
            int bodies = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                shipFound |= root.GetComponentInChildren<SpacecraftController>(true) != null;
                cameraFound |= root.GetComponentInChildren<CameraRigController>(true) != null;
                bodies += root.GetComponentsInChildren<CelestialBody>(true).Length;
            }
            EditorSceneManager.CloseScene(scene, true);

            bool pass = shipFound && cameraFound && bodies >= 4;
            if (pass)
                Debug.Log($"GALAXY EXPLORER validation passed: ship={shipFound}, camera={cameraFound}, celestial bodies={bodies}. This is an editor scene-structure check, not a runtime or APK test.");
            else
                Debug.LogError($"GALAXY EXPLORER validation failed: ship={shipFound}, camera={cameraFound}, celestial bodies={bodies}. Rebuild the prototype scene.");
        }
    }
}
#endif
