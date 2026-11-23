using UnityEngine;

/// <summary>Slow axial rotation for a generated planet.</summary>
public sealed class PlanetRotator : MonoBehaviour
{
    [Min(0f)] public float degreesPerSecond = 5f;

    private void Update()
    {
        transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
