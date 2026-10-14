using Android.App;
using Android.Content.PM;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SMAPIGameLoader;

/// <summary>
/// Finds a locally installed Stardew Valley independently of its installer.
/// Supports the base-only APK layout used by some sideloaded releases as well
/// as the split APK layouts used by Google Play and other stores.
/// </summary>
internal static class StardewApkTool
{
    public const string GamePlayStorePackageName = "com.chucklefish.stardewvalley";
    public const string GameGalaxyStorePackageName = "com.chucklefish.stardewvalleysamsung";

    static readonly PackageInfo _currentPackageInfo;

    static StardewApkTool()
    {
        Console.WriteLine("Initialize Stardew Apk Tool (all installation sources)");
        // Keep the original preference for the Galaxy package if both are installed.
        foreach (var packageName in new[] { GameGalaxyStorePackageName, GamePlayStorePackageName })
        {
            var game = ApkTool.GetPackageInfo(packageName);
            var apkPath = game?.ApplicationInfo?.PublicSourceDir ?? game?.ApplicationInfo?.SourceDir;
            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath))
                continue;

            _currentPackageInfo = game;
            Console.WriteLine($"Found installed game: {packageName} ({apkPath})");
            break;
        }
    }

    public static PackageInfo CurrentPackageInfo => _currentPackageInfo;

    // Checking split count is not valid: games installed from an APK file may
    // have zero splits, while newer store releases can have more than two.
    public static bool IsInstalled => InstalledApkPaths.Count > 0;

    public static Android.Content.Context GetContext => Application.Context;

    public static string? BaseApkPath => CurrentPackageInfo?.ApplicationInfo?.PublicSourceDir
        ?? CurrentPackageInfo?.ApplicationInfo?.SourceDir;

    /// <summary>
    /// Every APK belonging to the installed package (base first, then splits).
    /// Do not assume particular split names or how many splits were installed.
    /// </summary>
    public static IReadOnlyList<string> InstalledApkPaths
    {
        get
        {
            var paths = new List<string>();
            var appInfo = CurrentPackageInfo?.ApplicationInfo;
            if (appInfo == null)
                return paths;

            void AddIfPresent(string? path)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)
                    && !paths.Contains(path, StringComparer.Ordinal))
                    paths.Add(path);
            }

            AddIfPresent(appInfo.PublicSourceDir);
            AddIfPresent(appInfo.SourceDir);
            if (appInfo.SplitSourceDirs != null)
            {
                foreach (var path in appInfo.SplitSourceDirs)
                    AddIfPresent(path);
            }
            return paths;
        }
    }

    /// <summary>
    /// Content may live in the base APK or in any split regardless of installer.
    /// </summary>
    public static string? ContentApkPath
    {
        get
        {
            foreach (var apkPath in InstalledApkPaths)
            {
                try
                {
                    using var apk = ZipFile.OpenRead(apkPath);
                    if (apk.Entries.Any(entry =>
                        entry.FullName.StartsWith("assets/Content/", StringComparison.OrdinalIgnoreCase)
                        && !entry.FullName.EndsWith("/", StringComparison.Ordinal)))
                        return apkPath;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to inspect game APK {apkPath}: {ex}");
                }
            }
            return null;
        }
    }

    public static Version GameVersionSupport => new(1, 6, 15, 3);

    public static Version CurrentGameVersion
    {
        get
        {
            if (Version.TryParse(CurrentPackageInfo?.VersionName, out var version))
                return version;
            return new Version(0, 0, 0, 0);
        }
    }

    public static bool IsGameVersionSupport => IsInstalled && CurrentGameVersion >= GameVersionSupport;
}
