using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Small local discovery journal persisted on the device.</summary>
public static class DiscoveryLog
{
    [Serializable] private sealed class SaveData { public List<string> names = new List<string>(); }
    private const string SaveKey = "GalaxyExplorer.Discoveries.v1";

    public static void Record(string name, string description)
    {
        SaveData data = Load();
        if (data.names.Contains(name)) return;
        data.names.Add(name);
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        Debug.Log("Discovery saved: " + name + " — " + description);
    }

    public static IReadOnlyList<string> GetDiscoveredNames()
    {
        return Load().names.AsReadOnly();
    }

    private static SaveData Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return new SaveData();
        try { return JsonUtility.FromJson<SaveData>(json) ?? new SaveData(); }
        catch { return new SaveData(); }
    }
}
