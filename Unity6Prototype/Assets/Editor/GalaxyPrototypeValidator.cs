#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using GalaxyExplorer.Spacecraft;
using GalaxyExplorer.Cameras;
using GalaxyExplorer.UI;
using GalaxyExplorer.World;

namespace GalaxyExplorer.Editor
{
    public static class GalaxyPrototypeValidator
    {
        [MenuItem("Galaxy Explorer/Validate Prototype 0.1")]
        public static void Validate()
        {
            const string scenePath = "Assets/Scenes/MainScene.unity";
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
            {
                Debug.LogError("Validation failed: MainScene is missing. Run Build Prototype 0.1 Scene first.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            bool shipFound = false;
            bool cameraFound = false;
            bool hudFound = false;
            bool eventSystemFound = false;
            bool shipHasRigidbody = false;
            int celestialBodyCount = 0;\n            int gravitySourceCount = 0;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                SpacecraftController controller = root.GetComponentInChildren<SpacecraftController>(true);
                if (controller != null)
                {
                    shipFound = true;
                    shipHasRigidbody = controller.GetComponent<Rigidbody>() != null;
                }

                cameraFound |= root.GetComponentInChildren<CameraRigController>(true) != null;
                hudFound |= root.GetComponentInChildren<FlightHUD>(true) != null;
                eventSystemFound |= root.GetComponentInChildren<EventSystem>(true) != null;
                celestialBodyCount += root.GetComponentsInChildren<CelestialBody>(true).Length;\n                gravitySourceCount += root.GetComponentsInChildren<GravitySource>(true).Length;
            }

            EditorSceneManager.CloseScene(scene, true);

            bool passed = shipFound && shipHasRigidbody && cameraFound && hudFound &&
                          eventSystemFound && celestialBodyCount >= 4 && gravitySourceCount >= 3;

            string report = $"ship={shipFound}, rigidbody={shipHasRigidbody}, camera={cameraFound}, " +
                            $"HUD={hudFound}, EventSystem={eventSystemFound}, celestial bodies={celestialBodyCount}, gravity sources={gravitySourceCount}";

            if (passed)
                Debug.Log($"GALAXY EXPLORER structure validation PASSED: {report}. This is not a runtime or APK test.");
            else
                Debug.LogError($"GALAXY EXPLORER structure validation FAILED: {report}. Rebuild the prototype scene.");
        }
    }
}
#endif