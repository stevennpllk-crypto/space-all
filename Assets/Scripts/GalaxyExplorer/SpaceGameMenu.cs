using UnityEngine;

/// <summary>Pause, discovery journal, and live graphics quality controls.</summary>
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

        float panelWidth = Mathf.Min(560f, Screen.width / scale - 36f);
        float panelHeight = Mathf.Min(660f, Screen.height / scale - 70f);
        Rect panel = new Rect((Screen.width / scale - panelWidth) * 0.5f,
                              (Screen.height / scale - panelHeight) * 0.5f,
                              panelWidth, panelHeight);
        GUI.Box(panel, GUIContent.none);
        GUILayout.BeginArea(new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, panel.height - 40f));
        GUILayout.Label("GALAXY EXPLORER", heading);
        GUILayout.Space(12f);

        if (GUILayout.Button("RESUME FLIGHT", GUILayout.Height(46f))) ToggleMenu();
        if (GUILayout.Button(showJournal ? "CLOSE JOURNAL" : "DISCOVERY JOURNAL", GUILayout.Height(46f)))
            showJournal = !showJournal;

        if (showJournal)
        {
            GUILayout.Space(12f);
            GUILayout.Label("RECORDED WORLDS", heading);
            journalScroll = GUILayout.BeginScrollView(journalScroll, GUILayout.Height(panelHeight - 250f));
            var names = DiscoveryLog.GetDiscoveredNames();
            if (names.Count == 0) GUILayout.Label("No discoveries yet. Explore nearby planets.", body);
            for (int i = 0; i < names.Count; i++) GUILayout.Label("• " + names[i], body);
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Space(14f);
            GUILayout.Label("GRAPHICS QUALITY", heading);
            GalaxyQualityManager quality = GalaxyQualityManager.Instance;
            if (quality != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("QUALITY", body, GUILayout.Width(90f));
                GUILayout.FlexibleSpace();
                GUILayout.Label(quality.QualityPercent + "%", heading, GUILayout.Width(76f));
                GUILayout.EndHorizontal();

                float selected = GUILayout.HorizontalSlider(quality.QualityPercent, 50f, 150f, GUILayout.Height(34f));
                if (Mathf.Abs(selected - quality.QualityPercent) >= 2f)
                    quality.SetQualityPercent(selected);

                GUILayout.BeginHorizontal();
                GUILayout.Label("50%  PERFORMANCE", body);
                GUILayout.FlexibleSpace();
                GUILayout.Label("150%  ULTRA", body);
                GUILayout.EndHorizontal();
                GUILayout.Label("Higher values increase render resolution, detail and shadow distance. Performance depends on your device.", body);
            }
            else GUILayout.Label("Graphics manager is starting...", body);

            GUILayout.Space(14f);
            GUILayout.Label("Explore nearby worlds and approach planets to record discoveries.", body);
            GUILayout.Space(8f);
            GUILayout.Label("Desktop: WASD / mouse / Q-E / Shift-Ctrl", body);
            GUILayout.Label("Mobile: left-side drag to steer, right-side drag to look.", body);
            GUILayout.Label("Device profile: " + GalaxyQualityManager.ActiveTier, body);
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
        if (!menuOpen)
        {
            showJournal = false;
            if (GalaxyQualityManager.Instance != null)
                GalaxyQualityManager.Instance.SaveSettings();
        }
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
