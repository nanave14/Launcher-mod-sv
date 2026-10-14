# Stardew Valley installed outside the Play Store

This patch makes the SMAPI Launcher **discover an installed game by its Android package name**, regardless of whether the installer was the Play Store, Galaxy Store, or an APK installed manually. It does **not** remove licensing checks from the game, patch game binaries, or download a copy of the game. Use a legitimately obtained game installation.

## Changes

- Removes the `SplitSourceDirs.Count == 2` launcher restriction.
- Finds `assets/Content/` in the base APK or any installed split.
- Searches each APK for .NET assemblies (`StardewValley.dll`, `MonoGame.Framework.dll`) rather than assuming a particular ARM64 split name.
- Detects when an installed APK is replaced with another build of the same version and retries extraction.
- Prints useful errors when actual game content or assemblies are missing.

## Limits

- The installed package name must be `com.chucklefish.stardewvalley` or `com.chucklefish.stardewvalleysamsung`. Different package names are not auto-discovered.
- An incomplete or modified APK missing assets or .NET assemblies still won't run.
- A matching Stardew Valley version and a compatible Android SMAPI ZIP are required.
- This is a **source patch**, not a compiled APK. Building requires the .NET 9 Android workload, Android SDK, compatible game DLLs, and the runtime prerequisites described in `docs/building.md`.

## How to apply

From the root of the unmodified source tree:

```sh
git apply ../SMAPILoader-sideload.patch
```

Then follow `docs/building.md`. If the launcher displays `FileNotFoundException` after installing the patch, capture the full stack trace or SMAPI log to identify the missing file.
