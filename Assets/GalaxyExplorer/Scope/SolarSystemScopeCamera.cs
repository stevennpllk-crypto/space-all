using UnityEngine;

namespace GalaxyExplorer.Scope
{
    [DisallowMultipleComponent]
    public sealed class SolarSystemScopeCamera : MonoBehaviour
    {
        [SerializeField] private float orbitSensitivity = 0.22f;
        [SerializeField] private float zoomSensitivity = 0.12f;
        [SerializeField] private float minimumDistance = 4f;
        [SerializeField] private float maximumDistance = 150f;
        [SerializeField] private float smoothness = 12f;

        private Transform focus;
        private Vector3 focusPoint;
        private float yaw;
        private float pitch = 24f;
        private float distance = 34f;
        private Vector2 lastTouchPosition;
        private float lastPinchDistance;

        private void Awake()
        {
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
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = focusPoint + rotation * new Vector3(0f, 0f, -distance);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.unscaledDeltaTime * smoothness);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.unscaledDeltaTime * smoothness);
        }

        private void ReadMouse()
        {
            if (Input.GetMouseButton(0))
            {
                yaw += Input.GetAxis("Mouse X") * orbitSensitivity * 8f;
                pitch -= Input.GetAxis("Mouse Y") * orbitSensitivity * 8f;
                pitch = Mathf.Clamp(pitch, -80f, 80f);
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
                    pitch = Mathf.Clamp(pitch, -80f, 80f);
                }
            }
            else if (Input.touchCount >= 2)
            {
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                float current = Vector2.Distance(first.position, second.position);
                if (lastPinchDistance > 0f)
                {
                    float delta = current - lastPinchDistance;
                    distance = Mathf.Clamp(distance * (1f - delta * 0.002f), minimumDistance, maximumDistance);
                }
                lastPinchDistance = current;
            }
            else
            {
                lastPinchDistance = 0f;
            }
        }

        public void Focus(Transform target)
        {
            if (target == null) return;
            focus = target;
            focusPoint = target.position;
            distance = Mathf.Clamp(Mathf.Max(5f, target.localScale.x * 5f), minimumDistance, maximumDistance);
        }

        public void FocusOrigin()
        {
            focus = null;
            focusPoint = Vector3.zero;
            distance = 34f;
        }
    }
}
