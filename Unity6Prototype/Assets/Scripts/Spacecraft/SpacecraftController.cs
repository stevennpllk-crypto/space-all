using UnityEngine;
using GalaxyExplorer.Input;
using GalaxyExplorer.World;

namespace GalaxyExplorer.Spacecraft
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class SpacecraftController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float thrust = 18f;
        [SerializeField, Min(0f)] private float rotationSpeed = 65f;
        [SerializeField, Min(1f)] private float maxSpeed = 85f;
        [SerializeField, Min(0f)] private float brakeStrength = 2.5f;
        [SerializeField] private bool usePlanetaryGravity = true;
        [SerializeField] private VirtualJoystick moveStick;
        [SerializeField] private VirtualJoystick lookStick;

        private Rigidbody body;
        private float throttle;
        private bool braking;

        public float Speed => body == null ? 0f : body.linearVelocity.magnitude;
        public float Throttle => throttle;
        public bool IsBraking => braking;

        public void SetMoveStick(VirtualJoystick value) => moveStick = value;
        public void SetLookStick(VirtualJoystick value) => lookStick = value;
        public void SetThrottle(float value) => throttle = Mathf.Clamp01(value);
        public void SetBraking(bool value) => braking = value;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = .08f;
            body.angularDamping = 1.2f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            if (body.isKinematic)
                return;

            Vector2 movement = moveStick != null ? moveStick.Value : Vector2.zero;
            Vector2 looking = lookStick != null ? lookStick.Value : Vector2.zero;

            body.AddRelativeTorque(
                new Vector3(-looking.y, looking.x, -movement.x) *
                rotationSpeed * Mathf.Deg2Rad,
                ForceMode.Acceleration);

            body.AddRelativeForce(
                Vector3.forward * (movement.y + throttle) * thrust,
                ForceMode.Acceleration);

            if (usePlanetaryGravity)
                GravitySource.ApplyTo(body);

            if (braking)
                body.linearVelocity = Vector3.Lerp(
                    body.linearVelocity, Vector3.zero, brakeStrength * Time.fixedDeltaTime);

            if (body.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }
    }
}