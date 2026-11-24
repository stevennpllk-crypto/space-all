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
        private float yaw;
        private float pitch = 24f;
        private float distance = 34f;
        private Vector2 lastTouchPosition;
        private Vector2 lastTwoFingerCenter;
        private float lastPinchDistance;
        private Vector3 desiredPan;
        private Camera scopeCamera;
        private bool vrPoseAvailable;

        private void Awake()
        {
            scopeCamera = GetComponent<Camera>();
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
            focusPoint = Vector3.zero;
            distance = Mathf.Clamp(Vector3.Distance(transform.position, focusPoint), minimumDistance, maximumDistance);
        }

        private void LateUpdate()
        {
            ReadMouse();
            ReadTouch();
            ReadKeyboardFlight();

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
                distance = Mathf.Clamp(distance * (1f - wheel * zoomSensitivity), minimumDistance, maximumDistance);
        }

        private void ReadTouch()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
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
                float currentDistance = Vector2.Distance(first.position, second.position);
                Vector2 center = (first.position + second.position) * 0.5f;

                if (lastPinchDistance > 0f)
                {
                    float pinchDelta = currentDistance - lastPinchDistance;
                    distance = Mathf.Clamp(distance * (1f - pinchDelta * 0.002f), minimumDistance, maximumDistance);
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
            focusPoint += movement * speed * Time.unscaledDeltaTime;
        }

        private void Pan(Vector2 delta)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            focusPoint += (-right * delta.x - up * delta.y);
        }

        public void Focus(Transform target)
        {
            if (target == null) return;
            focusPoint = target.position;
            distance = Mathf.Clamp(Mathf.Max(1.5f, target.lossyScale.x * 3.5f), minimumDistance, maximumDistance);
        }

        public void FocusPoint(Vector3 point, float preferredDistance = 24f)
        {
            focusPoint = point;
            distance = Mathf.Clamp(preferredDistance, minimumDistance, maximumDistance);
        }

        public void FocusOrigin()
        {
            focusPoint = Vector3.zero;
            distance = 34f;
            yaw = 0f;
            pitch = 24f;
        }

        public void SetFlySpeed(float value)
        {
            flySpeed = Mathf.Max(0.1f, value);
        }

        public bool IsVRActive { get { return vrPoseAvailable; } }
    }
}
