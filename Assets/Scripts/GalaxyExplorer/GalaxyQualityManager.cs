using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Adaptive device defaults plus a user-controlled 50-150 percent quality scale.</summary>
public sealed class GalaxyQualityManager : MonoBehaviour
{
    public enum DeviceTier { Low, Balanced, High }

    private const string QualityKey = "GalaxyExplorer.QualityPercent";
    public static DeviceTier ActiveTier { get; private set; }
    public static GalaxyQualityManager Instance { get; private set; }
    public int QualityPercent { get; private set; } = 75;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyDeviceProfile()
    {
        if (FindObjectOfType<GalaxyQualityManager>() != null) return;
        new GameObject("Galaxy Quality Manager").AddComponent<GalaxyQualityManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        int ram = SystemInfo.systemMemorySize;
        int gpu = SystemInfo.graphicsMemorySize;
        bool high = ram >= 8000 && gpu >= 3000;
        bool balanced = ram >= 4000 && gpu >= 1500;
        ActiveTier = high ? DeviceTier.High : balanced ? DeviceTier.Balanced : DeviceTier.Low;

        int deviceDefault = ActiveTier == DeviceTier.High ? 120 :
                            ActiveTier == DeviceTier.Balanced ? 95 : 75;
        QualityPercent = PlayerPrefs.GetInt(QualityKey, deviceDefault);
        ApplyQuality();

        Debug.Log("Galaxy Explorer quality tier: " + ActiveTier +
                  " | user quality: " + QualityPercent + "%" +
                  " | RAM " + ram + " MB | GPU memory " + gpu + " MB");
    }

    public void SetQualityPercent(float value)
    {
        int next = Mathf.Clamp(Mathf.RoundToInt(value / 5f) * 5, 50, 150);
        if (next == QualityPercent) return;
        QualityPercent = next;
        PlayerPrefs.SetInt(QualityKey, QualityPercent);
        ApplyQuality();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt(QualityKey, QualityPercent);
        PlayerPrefs.Save();
    }

    private void ApplyQuality()
    {
        float scale = QualityPercent / 100f;
        UniversalRenderPipelineAsset urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urp != null)
            urp.renderScale = Mathf.Clamp(scale, 0.5f, 1.5f);

        QualitySettings.vSyncCount = 0;
        QualitySettings.lodBias = Mathf.Lerp(0.55f, 2.0f, Mathf.InverseLerp(50f, 150f, QualityPercent));
        QualitySettings.maximumLODLevel = QualityPercent >= 125 ? 0 : QualityPercent >= 85 ? 1 : 2;
        QualitySettings.shadowDistance = QualityPercent < 65 ? 12f :
                                         QualityPercent < 90 ? 28f :
                                         QualityPercent < 120 ? 55f : 90f;
        QualitySettings.shadows = QualityPercent < 60 ? ShadowQuality.Disable : ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = QualityPercent >= 130 ? ShadowResolution.VeryHigh :
                                           QualityPercent >= 95 ? ShadowResolution.High :
                                           ShadowResolution.Medium;
        QualitySettings.masterTextureLimit = QualityPercent < 65 ? 1 : 0;
        QualitySettings.pixelLightCount = QualityPercent >= 120 ? 4 : 2;
        Application.targetFrameRate = QualityPercent < 70 ? 30 : 60;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
