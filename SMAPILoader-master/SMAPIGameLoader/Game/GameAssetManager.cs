
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewValley;
using System;
using System.IO;
using System.IO.Compression;
using HarmonyLib;

namespace SMAPIGameLoader;

[HarmonyPatch]
static class GameAssetManager
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TitleContainer), nameof(TitleContainer.OpenStream))]
    static bool PrefixOpenStream(ref Stream __result, string name)
    {
        __result = FixOpenStream(name);
        return false;
    }
    public const string StardewAssetFolderName = "Stardew Assets";
    static string _gameAssetDir = null;
    static string GetGameAssetsDir
    {
        get
        {
            if (_gameAssetDir == null)
                _gameAssetDir = Path.Combine(FileTool.ExternalFilesDir, StardewAssetFolderName);
            return _gameAssetDir;
        }
    }
    public delegate Stream OnOpenStreamDelegate(string assetName);
    public static OnOpenStreamDelegate OnOpenStream;
    static Stream FixOpenStream(string assetName)
    {
        try
        {
            //example: Cotent\\BigCraftables
            assetName = FileTool.SafePath(assetName);

            //load form other stream
            var hookOpenStream = OnOpenStream?.Invoke(assetName);
            if (hookOpenStream != null)
                return hookOpenStream;

            //load vanilla
            string assetFullPath = Path.Combine(GetGameAssetsDir, assetName);
            //Console.WriteLine("on OpenStream: " + assetName);
            return File.OpenRead(assetFullPath);
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex);
            throw;
        }
    }


    public static void VerifyAssets()
    {
        //check & update game content
        var contentApkPath = StardewApkTool.ContentApkPath;
        if (contentApkPath == null)
            throw new FileNotFoundException(
                "Game assets (assets/Content/) were not found in any installed Stardew Valley APK. " +
                "Check whether the game's content split is installed.");

        Console.WriteLine($"Extracting game content from {contentApkPath}");
        using (FileStream apkFileStream = new FileStream(contentApkPath, FileMode.Open, FileAccess.Read))
        using (ZipArchive apkArchive = new ZipArchive(apkFileStream, ZipArchiveMode.Read))
        {
            var externalAssetsDir = Path.Combine(FileTool.ExternalFilesDir, StardewAssetFolderName);
            if (Directory.Exists(externalAssetsDir))
                Directory.Delete(externalAssetsDir, recursive: true);
            Directory.CreateDirectory(externalAssetsDir);

            foreach (ZipArchiveEntry entry in apkArchive.Entries)
            {
                if (!entry.FullName.StartsWith("assets/Content/", StringComparison.OrdinalIgnoreCase) ||
                    entry.FullName.EndsWith("/", StringComparison.Ordinal))
                    continue;

                // Normalize ZIP paths and prevent traversal from untrusted APK entries.
                var relativePath = entry.FullName.Substring("assets/".Length).Replace('/', Path.DirectorySeparatorChar);
                var destFilePath = Path.GetFullPath(Path.Combine(externalAssetsDir, relativePath));
                var allowedPrefix = Path.GetFullPath(externalAssetsDir) + Path.DirectorySeparatorChar;
                if (!destFilePath.StartsWith(allowedPrefix, StringComparison.Ordinal))
                    throw new InvalidDataException($"Invalid content entry path: {entry.FullName}");

                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath)!);
                using var entryStream = entry.Open();
                using var destFileStream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write);
                entryStream.CopyTo(destFileStream);
            }
        }

        Console.WriteLine("successfully verify & clone assets");
    }
}

