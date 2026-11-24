using UnityEngine;

/// <summary>Navigation HUD for free-flight exploration without a vehicle.</summary>
public sealed class SpaceHUD : MonoBehaviour
{
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(14, Screen.height / 54),
                normal = { textColor = Color.white }
            };
            titleStyle = new GUIStyle(labelStyle)
            {
                fontSize = Mathf.Max(19, Screen.height / 34),
                fontStyle = FontStyle.Bold
            };
        }

        GUI.color = new Color(0.02f, 0.05f, 0.12f, 0.72f);
        GUI.Box(new Rect(18, 18, 330, 112), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(32, 26, 300, 32), "GALAXY EXPLORER", titleStyle);
        GUI.Label(new Rect(32, 64, 300, 24), "FREE EXPLORATION  •  NO SPACECRAFT", labelStyle);
        GUI.Label(new Rect(32, 92, 300, 24), "Explore planets, moons and deep space", labelStyle);

        if (Application.isMobilePlatform)
        {
            GUI.color = new Color(0.1f, 0.45f, 0.9f, 0.36f);
            GUI.Box(new Rect(20, Screen.height - 116, 150, 88), "DRAG: ORBIT");
            GUI.Box(new Rect(Screen.width - 170, Screen.height - 116, 150, 88), "TWO FINGERS: PAN / ZOOM");
            GUI.color = Color.white;
        }
    }
}
