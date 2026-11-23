using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>In-game gallery for the real NASA images bundled with the project.</summary>
public sealed class NASAImageGallery : MonoBehaviour
{
    [Serializable] private sealed class Manifest { public AssetRecord[] assets; }
    [Serializable] private sealed class AssetRecord
    {
        public string category;
        public string file;
        public string title;
        public string secondary_creator;
        public string source_page;
        public string center;
    }

    private readonly List<Texture2D> textures = new List<Texture2D>();
    private readonly List<AssetRecord> records = new List<AssetRecord>();
    private bool open;
    private Vector2 scroll;
    private GUIStyle titleStyle;
    private GUIStyle detailStyle;

    private void Awake()
    {
        TextAsset json = Resources.Load<TextAsset>("NASA/NASA_ASSET_CREDITS");
        if (json != null)
        {
            Manifest manifest = JsonUtility.FromJson<Manifest>(json.text);
            if (manifest != null && manifest.assets != null)
                records.AddRange(manifest.assets);
        }
        for (int i = 0; i < records.Count; i++)
            textures.Add(Resources.Load<Texture2D>("NASA/" + System.IO.Path.GetFileNameWithoutExtension(records[i].file)));
    }

    private void OnGUI()
    {
        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 900f, 0.8f, 1.35f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float screenWidth = Screen.width / scale;
        float screenHeight = Screen.height / scale;

        if (GUI.Button(new Rect(18f, 78f, 154f, 42f), open ? "CLOSE NASA GALLERY" : "NASA IMAGE GALLERY"))
            open = !open;

        if (open)
        {
            float width = Mathf.Min(720f, screenWidth - 32f);
            float height = Mathf.Min(650f, screenHeight - 150f);
            Rect panel = new Rect((screenWidth - width) * 0.5f, (screenHeight - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 16f, panel.y + 14f, panel.width - 32f, panel.height - 28f));
            GUILayout.Label("NASA SPACE IMAGE ARCHIVE", titleStyle);
            GUILayout.Label("Original NASA library images bundled with this game.", detailStyle);
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < records.Count; i++)
            {
                AssetRecord record = records[i];
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(record.title, titleStyle);
                Texture2D texture = i < textures.Count ? textures[i] : null;
                if (texture != null)
                {
                    float imageHeight = Mathf.Min(220f, texture.height * Mathf.Min(1f, (width - 80f) / texture.width));
                    Rect imageRect = GUILayoutUtility.GetRect(width - 80f, imageHeight, GUILayout.ExpandWidth(true));
                    GUI.DrawTexture(imageRect, texture, ScaleMode.ScaleToFit, true);
                }
                string credit = string.IsNullOrEmpty(record.secondary_creator) ? record.center : record.secondary_creator;
                GUILayout.Label("Credit: " + credit, detailStyle);
                if (GUILayout.Button("OPEN NASA SOURCE"))
                    Application.OpenURL(record.source_page);
                GUILayout.EndVertical();
                GUILayout.Space(8f);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        GUI.matrix = previous;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(15, Screen.height / 55),
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        detailStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(11, Screen.height / 78),
            wordWrap = true,
            normal = { textColor = new Color(0.78f, 0.86f, 1f) }
        };
    }
}
