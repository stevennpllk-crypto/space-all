using UnityEngine;
using UnityEngine.XR;

namespace GalaxyExplorer.Scope
{
    /// <summary>
    /// Optional VR status and runtime bridge. Actual stereo rendering and headset tracking
    /// require an OpenXR-compatible loader configured for the target device in Unity.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GalaxyExplorerVRSupport : MonoBehaviour
    {
        [SerializeField] private bool showStatus = true;
        [SerializeField] private bool requestStereoCamera = true;

        private Camera targetCamera;
        private GUIStyle statusStyle;
        private bool xrLoaderDetected;

        public bool IsRuntimeActive
        {
            get { return XRSettings.isDeviceActive && XRSettings.enabled; }
        }

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null && Camera.main != null)
                targetCamera = Camera.main;

            xrLoaderDetected = XRSettings.loadedDeviceName != "MockHMD" &&
                               !string.IsNullOrEmpty(XRSettings.loadedDeviceName);

            if (targetCamera != null && requestStereoCamera)
                targetCamera.stereoTargetEye = StereoTargetEyeMask.Both;
        }

        public string GetStatus()
        {
            if (IsRuntimeActive)
                return "VR ACTIVE  •  " + XRSettings.loadedDeviceName;
            if (xrLoaderDetected)
                return "VR loader detected • waiting for headset";
            return "VR READY WHEN OPENXR IS CONFIGURED";
        }

        private void OnGUI()
        {
            if (!showStatus) return;
            if (statusStyle == null)
            {
                statusStyle = new GUIStyle(GUI.skin.label);
                statusStyle.fontSize = 12;
                statusStyle.fontStyle = FontStyle.Bold;
                statusStyle.normal.textColor = IsRuntimeActive
                    ? new Color(0.35f, 1f, 0.75f)
                    : new Color(0.72f, 0.82f, 1f);
            }

            GUI.Label(new Rect(18f, Screen.height - 30f, 430f, 22f), GetStatus(), statusStyle);
        }
    }
}
