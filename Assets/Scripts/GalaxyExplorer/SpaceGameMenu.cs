using UnityEngine;

/// <summary>Lightweight runtime pause and discovery overlay.</summary>
public sealed class SpaceGameMenu : MonoBehaviour
{
    private bool menuOpen;
    private bool showJournal;
    private Vector2 journalScroll;
    private GUIStyle heading;
    private GUIStyle body;

    private void OnGUI()
    {
        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 900f, 0.8f, 1.5f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

        if (GUI.Button(new Rect(Screen.width / scale - 150f, 22f, 126f, 48f), menuOpen ? "RESUME" : "PAUSE"))
            ToggleMenu();

        if (!menuOpen) return;

        float panelWidth = Mathf.Min(520f, Screen.width / scale - 36f);
        float panelHeight = Mathf.Min(600f, Screen.height / scale - 70f);
        Rect panel = new Rect((Screen.width / scale - panelWidth) * 0.5f,
                              (Screen.height / scale - panelHeight) * 0.5f,
                              panelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none);
        GUILayout.BeginArea(new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, panel.height - 40f));
        GUILayout.Label("GALAXY EXPLORER", heading);
        GUILayout.Space(18f);

        if (GUILayout.Button("RESUME FLIGHT", GUILayout.Height(48f))) ToggleMenu();
        if (GUILayout.Button(showJournal ? "CLOSE JOURNAL" : "DISCOVERY JOURNAL", GUILayout.Height(48f)))
            showJournal = !showJournal;

        if (showJournal)
        {
            GUILayout.Space(12f);
            GUILayout.Label("RECORDED WORLDS", heading);
            journalScroll = GUILayout.BeginScrollView(journalScroll, GUILayout.Height(panelHeight - 210f));
            var names = DiscoveryLog.GetDiscoveredNames();
            if (names.Count == 0) GUILayout.Label("No discoveries yet. Explore nearby planets.", body);
            for (int i = 0; i < names.Count; i++) GUILayout.Label("• " + names[i], body);
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Space(20f);
            GUILayout.Label("Explore the nearby worlds. Approach a planet to add it to your journal.", body);
            GUILayout.Space(8f);
            GUILayout.Label("Desktop: WASD / mouse / Q-E / Shift-Ctrl", body);
            GUILayout.Label("Mobile: left-side drag to steer, right-side drag to look.", body);
            GUILayout.Label("Graphics: " + GalaxyQualityManager.ActiveTier, body);
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("CONTINUE EXPLORING", GUILayout.Height(44f))) ToggleMenu();
        GUILayout.EndArea();
    }

    private void EnsureStyles()
    {
        if (heading != null) return;
        heading = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.height / 36), fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        body = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(14, Screen.height / 52), wordWrap = true, normal = { textColor = new Color(0.82f, 0.88f, 1f) } };
    }

    private void ToggleMenu()
    {
        menuOpen = !menuOpen;
        if (!menuOpen) showJournal = false;
        Time.timeScale = menuOpen ? 0f : 1f;
        Cursor.visible = menuOpen;
        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
