using UnityEngine;

/// <summary>Chooses conservative graphics defaults from device capabilities.</summary>
public sealed class GalaxyQualityManager : MonoBehaviour
{
    public enum DeviceTier { Low, Balanced, High }

    public static DeviceTier ActiveTier { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyDeviceProfile()
    {
        if (FindObjectOfType<GalaxyQualityManager>() != null) return;
        new GameObject("Galaxy Quality Manager").AddComponent<GalaxyQualityManager>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        int ram = SystemInfo.systemMemorySize;
        int gpu = SystemInfo.graphicsMemorySize;
        bool high = ram >= 8000 && gpu >= 3000;
        bool balanced = ram >= 4000 && gpu >= 1500;
        ActiveTier = high ? DeviceTier.High : balanced ? DeviceTier.Balanced : DeviceTier.Low;

        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ActiveTier == DeviceTier.Low ? ShadowQuality.Disable : ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = ActiveTier == DeviceTier.High ? ShadowResolution.High : ShadowResolution.Medium;
        QualitySettings.shadowDistance = ActiveTier == DeviceTier.High ? 65f : ActiveTier == DeviceTier.Balanced ? 35f : 0f;
        QualitySettings.antiAliasing = ActiveTier == DeviceTier.High ? 4 : ActiveTier == DeviceTier.Balanced ? 2 : 0;
        QualitySettings.lodBias = ActiveTier == DeviceTier.High ? 1.5f : ActiveTier == DeviceTier.Balanced ? 1f : 0.65f;
        QualitySettings.pixelLightCount = ActiveTier == DeviceTier.High ? 4 : 2;
        Application.targetFrameRate = ActiveTier == DeviceTier.Low ? 30 : 60;

        Debug.Log("Galaxy Explorer quality tier: " + ActiveTier +
                  " | RAM " + ram + " MB | GPU memory " + gpu + " MB");
    }
}
