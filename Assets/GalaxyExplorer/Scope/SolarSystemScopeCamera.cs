using UnityEngine;
using UnityEngine.XR;

namespace GalaxyExplorer.Scope
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class SolarSystemScopeCamera : MonoBehaviour
    {
        [Header("Orbit and zoom")]
        [SerializeField] private float orbitSensitivity = 0.22f;
        [SerializeField] private float zoomSensitivity = 0.12f;
        [SerializeField] private float minimumDistance = 0.45f;
        [SerializeField] private float maximumDistance = 260f;
        [SerializeField] private float smoothness = 10f;

        [Header("Free exploration")]
        [SerializeField] private float panSensitivity = 0.012f;
        [SerializeField] private float flySpeed = 18f;
        [SerializeField] private float fastFlyMultiplier = 4f;
        [SerializeField] private bool allowKeyboardFlight = true;

        private Vector3 focusPoint;
        private Vector3 targetFocusPoint;
        private float targetDistance = 34f;
        private float focusTransitionSpeed = 3.2f;
        private float yaw;
        private float pitch = 24f;
        private float distance = 34f;
        private Vector2 lastTouchPosition;
        private Vector2 lastTwoFingerCenter;
        private float lastPinchDistance;
        private Vector3 desiredPan;
        private Camera scopeCamera;
        private bool vrPoseAvailable;
        private float saveTimer;
        private const string SavePrefix = "GalaxyExplorer.Camera.";
        private const string SaveVersionKey = SavePrefix + "Version";

        private void Awake()
        {
            scopeCamera = GetComponent<Camera>();
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
            focusPoint = Vector3.zero;
            targetFocusPoint = focusPoint;
            distance = Mathf.Clamp(Vector3.Distance(transform.position, focusPoint), minimumDistance, maximumDistance);
            targetDistance = distance;
            LoadExplorationState();
        }

        private void LateUpdate()
        {
            ReadMouse();
            ReadTouch();
            ReadKeyboardFlight();

            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= 5f)
            {
                saveTimer = 0f;
                SaveExplorationState();
            }

            float focusBlend = 1f - Mathf.Exp(-focusTransitionSpeed * Time.unscaledDeltaTime);
            focusPoint = Vector3.Lerp(focusPoint, targetFocusPoint, focusBlend);
            distance = Mathf.Lerp(distance, targetDistance, focusBlend);

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = focusPoint + orbitRotation * new Vector3(0f, 0f, -distance);

            if (XRSettings.isDeviceActive && XRSettings.enabled)
            {
                // The XR runtime supplies the tracked head pose; this controller supplies the world-space exploration rig.
                Vector3 headPosition = InputTracking.GetLocalPosition(XRNode.Head);
                Quaternion headRotation = InputTracking.GetLocalRotation(XRNode.Head);
                transform.position = desiredPosition + orbitRotation * headPosition;
                transform.rotation = orbitRotation * headRotation;
                vrPoseAvailable = true;
            }
            else
            {
                vrPoseAvailable = false;
                float blend = 1f - Mathf.Exp(-smoothness * Time.unscaledDeltaTime);
                transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
                transform.rotation = Quaternion.Slerp(transform.rotation, orbitRotation, blend);
            }
        }

        private void ReadMouse()
        {
            if (IsPointerOverUi(Input.mousePosition))
                return;

            if (Input.GetMouseButton(0))
            {
                yaw += Input.GetAxis("Mouse X") * orbitSensitivity * 8f;
                pitch -= Input.GetAxis("Mouse Y") * orbitSensitivity * 8f;
                pitch = Mathf.Clamp(pitch, -85f, 85f);
            }

            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                Pan(delta * (panSensitivity * Mathf.Max(distance, 2f) * 70f));
            }

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.001f)
                targetDistance = Mathf.Clamp(targetDistance * (1f - wheel * zoomSensitivity), minimumDistance, maximumDistance);
        }

        private void ReadTouch()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (IsPointerOverUi(touch.position))
                {
                    lastPinchDistance = 0f;
                    return;
                }
                if (touch.phase == TouchPhase.Moved)
                {
                    yaw += touch.deltaPosition.x * orbitSensitivity * 0.12f;
                    pitch -= touch.deltaPosition.y * orbitSensitivity * 0.12f;
                    pitch = Mathf.Clamp(pitch, -85f, 85f);
                }
                lastPinchDistance = 0f;
            }
            else if (Input.touchCount >= 2)
            {
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                if (IsPointerOverUi(first.position) || IsPointerOverUi(second.position))
                {
                    lastPinchDistance = 0f;
                    return;
                }
                float currentDistance = Vector2.Distance(first.position, second.position);
                Vector2 center = (first.position + second.position) * 0.5f;

                if (lastPinchDistance > 0f)
                {
                    float pinchDelta = currentDistance - lastPinchDistance;
                    targetDistance = Mathf.Clamp(targetDistance * (1f - pinchDelta * 0.002f), minimumDistance, maximumDistance);
                    Vector2 centerDelta = center - lastTwoFingerCenter;
                    Pan(centerDelta * (panSensitivity * Mathf.Max(distance, 2f) * 0.9f));
                }

                lastPinchDistance = currentDistance;
                lastTwoFingerCenter = center;
            }
            else
            {
                lastPinchDistance = 0f;
            }
        }

        private bool IsPointerOverUi(Vector2 screenPosition)
        {
            // Keep camera gestures from moving the scene while the user interacts
            // with the runtime IMGUI catalogs and simulation controls.
            float scale = Mathf.Clamp(Screen.width / 1100f, 0.72f, 1.15f);
            Vector2 point = new Vector2(screenPosition.x / scale,
                (Screen.height - screenPosition.y) / scale);
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            return new Rect(18f, 18f, 310f, 94f).Contains(point) ||
                   new Rect(18f, 125f, 310f, 190f).Contains(point) ||
                   new Rect(width - 190f, 18f, 170f, 130f).Contains(point) ||
                   new Rect(18f, height - 48f, 540f, 42f).Contains(point) ||
                   new Rect(width - 230f, 164f, 210f, 222f).Contains(point) ||
                   new Rect(18f, 326f, 310f, 120f).Contains(point) ||
                   new Rect(width - 230f, 398f, 210f, 194f).Contains(point) ||
                   new Rect(18f, 455f, 310f, 116f).Contains(point);
        }

        private void ReadKeyboardFlight()
        {
            if (!allowKeyboardFlight || Application.isMobilePlatform || XRSettings.isDeviceActive)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            float altitude = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
            float speed = flySpeed * (Input.GetKey(KeyCode.LeftShift) ? fastFlyMultiplier : 1f);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 movement = rotation * new Vector3(horizontal, altitude, vertical);
            if (movement.sqrMagnitude > 1f) movement.Normalize();
            Vector3 step = movement * speed * Time.unscaledDeltaTime;
            focusPoint += step;
            targetFocusPoint += step;
        }

        private void Pan(Vector2 delta)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 movement = -right * delta.x - up * delta.y;
            focusPoint += movement;
            targetFocusPoint += movement;
        }

        public void Focus(Transform target)
        {
            if (target == null) return;
            targetFocusPoint = target.position;
            targetDistance = Mathf.Clamp(Mathf.Max(1.5f, target.lossyScale.x * 3.5f), minimumDistance, maximumDistance);
        }

        public void FocusPoint(Vector3 point, float preferredDistance = 24f)
        {
            targetFocusPoint = point;
            targetDistance = Mathf.Clamp(preferredDistance, minimumDistance, maximumDistance);
        }

        public void FocusOrigin()
        {
            targetFocusPoint = Vector3.zero;
            targetDistance = 34f;
            yaw = 0f;
            pitch = 24f;
        }

        public void SetFlySpeed(float value)
        {
            flySpeed = Mathf.Max(0.1f, value);
        }

        private void LoadExplorationState()
        {
            if (PlayerPrefs.GetInt(SaveVersionKey, 0) != 1)
                return;

            focusPoint = new Vector3(
                PlayerPrefs.GetFloat(SavePrefix + "FocusX", focusPoint.x),
                PlayerPrefs.GetFloat(SavePrefix + "FocusY", focusPoint.y),
                PlayerPrefs.GetFloat(SavePrefix + "FocusZ", focusPoint.z));
            targetFocusPoint = new Vector3(
                PlayerPrefs.GetFloat(SavePrefix + "TargetX", focusPoint.x),
                PlayerPrefs.GetFloat(SavePrefix + "TargetY", focusPoint.y),
                PlayerPrefs.GetFloat(SavePrefix + "TargetZ", focusPoint.z));
            yaw = PlayerPrefs.GetFloat(SavePrefix + "Yaw", yaw);
            pitch = Mathf.Clamp(PlayerPrefs.GetFloat(SavePrefix + "Pitch", pitch), -85f, 85f);
            distance = Mathf.Clamp(PlayerPrefs.GetFloat(SavePrefix + "Distance", distance), minimumDistance, maximumDistance);
            targetDistance = Mathf.Clamp(PlayerPrefs.GetFloat(SavePrefix + "TargetDistance", distance), minimumDistance, maximumDistance);
        }

        private void SaveExplorationState()
        {
            PlayerPrefs.SetInt(SaveVersionKey, 1);
            PlayerPrefs.SetFloat(SavePrefix + "FocusX", focusPoint.x);
            PlayerPrefs.SetFloat(SavePrefix + "FocusY", focusPoint.y);
            PlayerPrefs.SetFloat(SavePrefix + "FocusZ", focusPoint.z);
            PlayerPrefs.SetFloat(SavePrefix + "TargetX", targetFocusPoint.x);
            PlayerPrefs.SetFloat(SavePrefix + "TargetY", targetFocusPoint.y);
            PlayerPrefs.SetFloat(SavePrefix + "TargetZ", targetFocusPoint.z);
            PlayerPrefs.SetFloat(SavePrefix + "Yaw", yaw);
            PlayerPrefs.SetFloat(SavePrefix + "Pitch", pitch);
            PlayerPrefs.SetFloat(SavePrefix + "Distance", distance);
            PlayerPrefs.SetFloat(SavePrefix + "TargetDistance", targetDistance);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveExplorationState();
        }

        private void OnApplicationQuit()
        {
            SaveExplorationState();
        }

        public bool IsVRActive { get { return vrPoseAvailable; } }
    }
}
