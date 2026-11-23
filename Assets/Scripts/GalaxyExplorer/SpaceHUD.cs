using UnityEngine;

/// <summary>Runtime HUD showing ship speed and discovered worlds.</summary>
public sealed class SpaceHUD : MonoBehaviour
{
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;

    private void Awake()
    {
        var ship = GameObject.Find("Player Exploration Ship");
        if (ship != null) ship.AddComponent<SpaceHUDMarker>();
    }

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(16, Screen.height / 42), normal = { textColor = Color.white } };
            titleStyle = new GUIStyle(labelStyle) { fontSize = Mathf.Max(20, Screen.height / 32), fontStyle = FontStyle.Bold };
        }

        GUI.color = new Color(0.02f, 0.05f, 0.12f, 0.72f);
        GUI.Box(new Rect(18, 18, 300, 112), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(32, 26, 270, 32), "GALAXY EXPLORER", titleStyle);
        Rigidbody shipBody = FindShipBody();
        float speed = shipBody != null ? shipBody.velocity.magnitude : 0f;
        GUI.Label(new Rect(32, 64, 270, 26), "SPEED  " + speed.ToString("0.0") + " m/s", labelStyle);
        GUI.Label(new Rect(32, 92, 270, 26), "WORLDS  " + DiscoveryLog.GetDiscoveredNames().Count, labelStyle);
    }

    private static Rigidbody FindShipBody()
    {
        GameObject ship = GameObject.Find("Player Exploration Ship");
        return ship != null ? ship.GetComponent<Rigidbody>() : null;
    }
}

/// <summary>Marker prevents duplicate HUD creation.</summary>
public sealed class SpaceHUDMarker : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectOfType<SpaceHUD>() == null)
            new GameObject("Galaxy Explorer HUD").AddComponent<SpaceHUD>();
    }
}
