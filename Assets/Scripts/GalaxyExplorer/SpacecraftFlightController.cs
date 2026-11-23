using UnityEngine;

/// <summary>
/// Rigidbody-based spacecraft flight controller.
/// Desktop: WASD move, Q/E roll, Shift/Ctrl change speed.
/// Mobile: hold and drag on the left half to steer; drag on the right half to look.
/// Attach to the spacecraft root and assign the camera pivot.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class SpacecraftFlightController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;

    [Header("Flight")]
    [Min(1f)] public float acceleration = 18f;
    [Min(1f)] public float maxSpeed = 42f;
    [Min(1f)] public float rotationSpeed = 75f;
    [Min(0f)] public float drag = 0.35f;

    [Header("Touch")]
    [Range(0.1f, 0.8f)] public float steeringAreaWidth = 0.5f;
    [Min(20f)] public float touchSensitivity = 0.18f;

    private Rigidbody body;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float rollInput;
    private float speedScale = 1f;
    private int leftFingerId = -1;
    private int rightFingerId = -1;
    private Vector2 leftStart;
    private Vector2 rightPrevious;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.drag = drag;
        body.angularDrag = 2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        ReadDesktopInput();
        ReadTouchInput();
    }

    private void FixedUpdate()
    {
        Vector3 localMove = new Vector3(moveInput.x, 0f, moveInput.y);
        if (localMove.sqrMagnitude > 1f) localMove.Normalize();
        body.AddForce(transform.TransformDirection(localMove) * acceleration * speedScale, ForceMode.Acceleration);

        float limit = maxSpeed * speedScale;
        if (body.velocity.sqrMagnitude > limit * limit)
            body.velocity = body.velocity.normalized * limit;

        Vector3 rotation = new Vector3(-lookInput.y, lookInput.x, -rollInput);
        body.MoveRotation(body.rotation * Quaternion.Euler(rotation * rotationSpeed * Time.fixedDeltaTime));
    }

    private void ReadDesktopInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical);
        lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 2f;
        rollInput = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);

        if (Input.GetKey(KeyCode.LeftShift)) speedScale = 1.75f;
        else if (Input.GetKey(KeyCode.LeftControl)) speedScale = 0.45f;
        else speedScale = 1f;

        if (cameraPivot != null && !Application.isMobilePlatform)
        {
            cameraPivot.position = transform.position;
            cameraPivot.rotation = transform.rotation;
        }
    }

    private void ReadTouchInput()
    {
        if (Input.touchCount == 0)
        {
            leftFingerId = -1;
            rightFingerId = -1;
            return;
        }

        Vector2 left = Vector2.zero;
        Vector2 right = Vector2.zero;
        bool hasLeft = false;
        bool hasRight = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                if (touch.position.x < Screen.width * steeringAreaWidth && leftFingerId < 0)
                {
                    leftFingerId = touch.fingerId;
                    leftStart = touch.position;
                }
                else if (touch.position.x >= Screen.width * steeringAreaWidth && rightFingerId < 0)
                {
                    rightFingerId = touch.fingerId;
                    rightPrevious = touch.position;
                }
            }

            if (touch.fingerId == leftFingerId)
            {
                hasLeft = true;
                left = (touch.position - leftStart) / Mathf.Max(1f, Screen.height * 0.16f);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) leftFingerId = -1;
            }
            else if (touch.fingerId == rightFingerId)
            {
                hasRight = true;
                right = touch.deltaPosition / Mathf.Max(1f, touchSensitivity * Screen.dpi / 160f);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) rightFingerId = -1;
            }
        }

        if (hasLeft) moveInput = Vector2.ClampMagnitude(left, 1f);
        else if (Application.isMobilePlatform) moveInput = Vector2.zero;

        if (hasRight) lookInput = Vector2.ClampMagnitude(right * 0.01f, 1f);
        else if (Application.isMobilePlatform) lookInput = Vector2.zero;
    }

    /// <summary>Can be connected to an on-screen boost button.</summary>
    public void SetBoost(bool enabled)
    {
        speedScale = enabled ? 1.75f : 1f;
    }

    /// <summary>Can be connected to an on-screen brake button.</summary>
    public void SetBrake(bool enabled)
    {
        speedScale = enabled ? 0.45f : 1f;
    }
}
