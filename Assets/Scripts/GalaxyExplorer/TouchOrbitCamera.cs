using UnityEngine;

/// <summary>
/// Simple orbit camera with one-finger/mouse rotation and pinch/scroll zoom.
/// Place on the Main Camera and assign a target transform.
/// </summary>
public sealed class TouchOrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 35f;
    public float minDistance = 8f;
    public float maxDistance = 100f;
    public float rotationSpeed = 0.18f;
    public float zoomSpeed = 0.02f;

    private float yaw;
    private float pitch = 18f;
    private Vector2 previousTouch;
    private float previousPinchDistance;

    private void LateUpdate()
    {
        if (target == null) return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                yaw += touch.deltaPosition.x * rotationSpeed;
                pitch -= touch.deltaPosition.y * rotationSpeed;
                pitch = Mathf.Clamp(pitch, -75f, 75f);
            }
        }
        else if (Input.touchCount >= 2)
        {
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);
            float current = Vector2.Distance(a.position, b.position);
            if (previousPinchDistance > 0f)
                distance = Mathf.Clamp(distance - (current - previousPinchDistance) * zoomSpeed, minDistance, maxDistance);
            previousPinchDistance = current;
        }
        else
        {
            previousPinchDistance = 0f;
            if (Input.GetMouseButton(0))
            {
                yaw += Input.GetAxis("Mouse X") * 4f;
                pitch -= Input.GetAxis("Mouse Y") * 4f;
                pitch = Mathf.Clamp(pitch, -75f, 75f);
            }
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * distance, minDistance, maxDistance);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target.position + rotation * new Vector3(0f, 0f, -distance);
        transform.rotation = rotation;
    }
}
