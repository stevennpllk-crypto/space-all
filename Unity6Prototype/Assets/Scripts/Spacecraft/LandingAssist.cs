using UnityEngine;
using GalaxyExplorer.World;

namespace GalaxyExplorer.Spacecraft
{
    /// <summary>
    /// Detects a gentle surface touchdown and holds the craft in place.
    /// This is a gameplay landing aid, not terrain physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SpacecraftController))]
    public sealed class LandingAssist : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float landingAltitude = 0.75f;
        [SerializeField, Min(0f)] private float maximumLandingSpeed = 2.5f;
        [SerializeField, Min(0f)] private float maximumRadialSpeed = 1.25f;
        [SerializeField, Range(1f, 45f)] private float maximumTilt = 25f;
        [SerializeField, Min(0f)] private float takeoffThrottle = 0.2f;

        private Rigidbody body;
        private SpacecraftController ship;
        private GravitySource nearestBody;
        private float altitude = float.PositiveInfinity;
        private bool landed;

        public bool IsLanded => landed;
        public float Altitude => altitude;
        public string TargetName => nearestBody != null ? nearestBody.name : "DEEP SPACE";
        public string Status => landed ? "LANDED / " + TargetName :
            nearestBody == null ? "CRUISE / DEEP SPACE" : "APPROACH / " + TargetName;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            ship = GetComponent<SpacecraftController>();
        }

        private void FixedUpdate()
        {
            FindNearestBody();
            if (landed)
            {
                if (ship.Throttle >= takeoffThrottle)
                {
                    landed = false;
                    body.isKinematic = false;
                    body.WakeUp();
                }
                return;
            }

            if (nearestBody == null || altitude > landingAltitude)
                return;

            Vector3 outward = (body.worldCenterOfMass - nearestBody.transform.position).normalized;
            float radialSpeed = Mathf.Abs(Vector3.Dot(body.linearVelocity, outward));
            float tilt = Vector3.Angle(transform.up, outward);

            if (ship.Speed <= maximumLandingSpeed &&
                radialSpeed <= maximumRadialSpeed &&
                tilt <= maximumTilt)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                landed = true;
            }
        }

        private void FindNearestBody()
        {
            nearestBody = null;
            altitude = float.PositiveInfinity;
            GravitySource[] sources = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);

            foreach (GravitySource source in sources)
            {
                if (source == null || source.SurfaceGravity <= 0f)
                    continue;

                float surfaceDistance = Vector3.Distance(
                    body.worldCenterOfMass, source.transform.position) - source.BodyRadius;
                if (surfaceDistance < altitude)
                {
                    altitude = Mathf.Max(0f, surfaceDistance);
                    nearestBody = source;
                }
            }
        }
    }
}
