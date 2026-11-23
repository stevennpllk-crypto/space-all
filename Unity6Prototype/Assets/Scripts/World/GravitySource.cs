using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer.World
{
    /// <summary>
    /// Applies a configurable spherical gravity field to a Rigidbody.
    /// Values are gameplay-scale approximations, not a solar-system simulator.
    /// </summary>
    public sealed class GravitySource : MonoBehaviour
    {
        private static readonly List<GravitySource> ActiveSources = new List<GravitySource>();

        [SerializeField, Min(0f)] private float surfaceGravity = 9.81f;
        [SerializeField, Min(0f)] private float influenceRadius = 30f;
        [SerializeField, Min(.01f)] private float bodyRadius = 1f;

        public float SurfaceGravity => surfaceGravity;
        public float InfluenceRadius => influenceRadius;
        public float BodyRadius => bodyRadius;

        public void Configure(float gravity, float radius, float influence)
        {
            surfaceGravity = Mathf.Max(0f, gravity);
            bodyRadius = Mathf.Max(.01f, radius);
            influenceRadius = Mathf.Max(bodyRadius, influence);
        }

        private void OnEnable()
        {
            if (!ActiveSources.Contains(this))
                ActiveSources.Add(this);
        }

        private void OnDisable()
        {
            ActiveSources.Remove(this);
        }

        public static void ApplyTo(Rigidbody target)
        {
            if (target == null)
                return;

            Vector3 position = target.worldCenterOfMass;
            for (int i = ActiveSources.Count - 1; i >= 0; i--)
            {
                GravitySource source = ActiveSources[i];
                if (source == null || !source.isActiveAndEnabled)
                {
                    ActiveSources.RemoveAt(i);
                    continue;
                }

                if (source.surfaceGravity <= 0f || source.influenceRadius <= 0f)
                    continue;

                Vector3 offset = source.transform.position - position;
                float distance = offset.magnitude;
                if (distance <= 0.001f || distance > source.influenceRadius)
                    continue;

                float safeDistance = Mathf.Max(distance, source.bodyRadius);
                float acceleration = source.surfaceGravity *
                                     (source.bodyRadius * source.bodyRadius) /
                                     (safeDistance * safeDistance);

                target.AddForce(offset / distance * acceleration, ForceMode.Acceleration);
            }
        }
    }
}