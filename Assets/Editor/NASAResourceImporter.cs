#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Copies NASA source images into Resources so the runtime gallery can display them.</summary>
[InitializeOnLoad]
public static class NASAResourceImporter
{
    private const string Source = "Assets/Art/NASA";
    private const string Destination = "Assets/Resources/NASA";

    static NASAResourceImporter()
    {
        EditorApplication.delayCall += EnsureResources;
    }

    [MenuItem("Galaxy Explorer/Import NASA Gallery Assets")]
    public static void EnsureResources()
    {
        if (!Directory.Exists(Source)) return;
        Directory.CreateDirectory(Destination);
        bool changed = false;
        foreach (string sourcePath in Directory.GetFiles(Source))
        {
            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".json")
                continue;

            string destinationPath = Path.Combine(Destination, Path.GetFileName(sourcePath)).Replace("\\", "/");
            var sourceInfo = new FileInfo(sourcePath);
            var destinationInfo = File.Exists(destinationPath) ? new FileInfo(destinationPath) : null;
            if (destinationInfo == null || destinationInfo.Length != sourceInfo.Length ||
                destinationInfo.LastWriteTimeUtc < sourceInfo.LastWriteTimeUtc)
            {
                File.Copy(sourcePath, destinationPath, true);
                changed = true;
            }
        }

        if (changed) AssetDatabase.Refresh();
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Destination }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            bool needsUpdate = importer.maxTextureSize != 1024 || importer.mipmapEnabled ||
                               importer.textureCompression != TextureImporterCompression.Compressed;
            if (!needsUpdate) continue;
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
        }
    }
}
#endif
