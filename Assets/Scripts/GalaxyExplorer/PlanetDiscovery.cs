using UnityEngine;

/// <summary>Marks a planet as discovered when the player approaches it.</summary>
public sealed class PlanetDiscovery : MonoBehaviour
{
    public string displayName = "Unknown World";
    [TextArea] public string description = "Uncharted space.";
    [Min(1f)] public float discoveryDistance = 18f;

    private bool discovered;
    private Transform player;

    private void Update()
    {
        if (discovered) return;
        if (player == null)
        {
            GameObject ship = GameObject.Find("Player Exploration Ship");
            if (ship == null) return;
            player = ship.transform;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= discoveryDistance)
        {
            discovered = true;
            DiscoveryLog.Record(displayName, description);
            Debug.Log("Galaxy Explorer: discovered " + displayName);
        }
    }
}
