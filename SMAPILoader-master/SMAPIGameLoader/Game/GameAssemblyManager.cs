using Android.App;
using MonoGame.Framework.Utilities;
using SMAPIGameLoader.Game;
using SMAPIGameLoader.Launcher;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Xamarin.Android.AssemblyStore;

namespace SMAPIGameLoader;

internal class GameAssemblyManager
{
    public const string AssembliesDirName = "Stardew Assemblies";
    public static string AssembliesDirPath => Path.Combine(FileTool.ExternalFilesDir, AssembliesDirName);
    public const string StardewDllName = "StardewValley.dll";
    public const string MonoGameDLLFileName = "MonoGame.Framework.dll";
    public static string StardewValleyFilePath => Path.Combine(AssembliesDirPath, StardewDllName);
    public static void VerifyAssemblies()
    {
        Console.WriteLine("Verify Assemblies");
        var assembliesOutputDirPath = AssembliesDirPath;
        // Don't mistake assemblies from an older game APK for successful extraction.
        if (Directory.Exists(assembliesOutputDirPath))
            Directory.Delete(assembliesOutputDirPath, recursive: true);
        Directory.CreateDirectory(assembliesOutputDirPath);

        {
            Console.WriteLine("try clone stardew assemblies from installed APKs");
            // A sideloaded APK may contain the assembly store in base.apk,
            // in a different split name, or in one of several installed splits.
            // Look inside every APK rather than assuming a Play Store layout.
            foreach (var apkPath in StardewApkTool.InstalledApkPaths)
            {
                try
                {
                    var store = new AssemblyStoreExplorer(apkPath, keepStoreInMemory: true);
                    if (store.Assemblies.Count == 0)
                        continue;

                    Console.WriteLine($"Found {store.Assemblies.Count} game assemblies in {apkPath}");
                    foreach (var asm in store.Assemblies)
                        asm.ExtractImage(assembliesOutputDirPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to read game assemblies from {apkPath}: {ex}");
                }
            }

            if (!File.Exists(StardewValleyFilePath))
                throw new FileNotFoundException(
                    "StardewValley.dll was not found in the installed game's base/split APKs. " +
                    "Verify that the Stardew Valley installation includes its .NET assemblies.",
                    StardewValleyFilePath);

            string monoGamePath = Path.Combine(assembliesOutputDirPath, MonoGameDLLFileName);
            if (!File.Exists(monoGamePath))
                throw new FileNotFoundException(
                    "MonoGame.Framework.dll was not found in the installed game's base/split APKs.",
                    monoGamePath);

            Console.WriteLine("done clone stardew assemblies");
        }

        {
            Console.WriteLine("try clone SMAPI Game Loader Assemblies");
            //clone dll & no trimming from this app 
            var appInfo = Application.Context.ApplicationInfo;
            var store = new AssemblyStoreExplorer(appInfo.PublicSourceDir, keepStoreInMemory: true);
            foreach (var asm in store.Assemblies)
            {
                asm.ExtractImage(assembliesOutputDirPath);
            }
            Console.WriteLine("done clone SMAPI Game Loader Assemblies");
        }
    }
    public static Assembly LoadAssembly(string dllFileName)
    {
        return Assembly.LoadFrom(Path.Combine(AssembliesDirPath, dllFileName));
    }

    static string LibDirPath => Path.Combine(FileTool.ExternalFilesDir, "lib");
    internal static void VerifyLibs()
    {
        Console.WriteLine("try setup libs");
        //clean lib first
        Console.WriteLine("clean up lib dir");
        if (Directory.Exists(LibDirPath))
            Directory.Delete(LibDirPath, true);
        Console.WriteLine("done setup libs");
    }
}
