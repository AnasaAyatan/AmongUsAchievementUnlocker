# Windows Setup — v0.1.0 Release Candidate

**Status:** GUI and CLI have worked locally, but the latest GUI revisions and a fresh packaged ZIP have not yet completed final validation. No installer is included.

## Package layout

```text
AmongUsAchievementUnlocker.Gui.exe
AmongUsAchievementUnlocker.Gui.dll
START-HERE.md
cli/
  AmongUsAchievementUnlocker.exe
  AmongUsAchievementUnlocker.dll
  steam_appid.txt
  ...supporting runtime files
```

Actual framework support file names may differ. Keep every file and the entire `cli` folder together.

## Requirements
Windows 10/11 x64, Steam running and signed into an account that owns Among Us, .NET 8 Windows Desktop Runtime for the GUI and matching .NET runtime for CLI. For developers, install the .NET 8 SDK. The ZIP does not contain the proprietary native Steamworks SDK binary.

## Install the GUI ZIP
1. Extract the whole GitHub Actions artifact ZIP, then extract the application ZIP inside if applicable.
2. Obtain a compatible licensed Steamworks SDK 1.60 Windows x64 `steam_api64.dll` from an authorized source.
3. Place it next to `cli/AmongUsAchievementUnlocker.exe`, not beside the GUI executable. Do not download random third-party DLLs.
4. Keep the provided `cli/steam_appid.txt` intact.
5. Start Steam, sign in, and run `AmongUsAchievementUnlocker.Gui.exe`.
6. Click **Refresh** and confirm the achievement list and Steam ID. No achievement changes occur until you click **Unlock remaining** and approve the dialog.

## Controls
- **Refresh**: read current Steam achievement data.
- **Unlock remaining**: request changes to locked achievements; requires a confirmation dialog. Disabled when nothing is locked.
- **Search**: search generated friendly labels and internal achievement IDs.
- **All / Unlocked / Locked**: filter results.
- **Progress**: unlocked proportion across all fetched achievements.
- **Show / Hide diagnostics**: toggle the backend log.

Friendly labels are derived from achievement IDs; they are not official Steam names or descriptions.

## VS Code source build
```powershell
dotnet publish .\AmongUsAchievementUnlocker.csproj -c Release -r win-x64 --self-contained false -o .\dist\cli
dotnet publish .\gui\AmongUsAchievementUnlocker.Gui.csproj -c Release -r win-x64 --self-contained false -o .\dist
# Put compatible authorized DLL in dist\cli before running.
& .\dist\AmongUsAchievementUnlocker.Gui.exe
```

## Troubleshooting
- **GUI says CLI executable missing:** restore the `cli/` folder next to the GUI.
- **DLL missing/incompatible:** verify `cli/steam_api64.dll` and compatible SDK version; older game-provided copies may have different exports.
- **Unable to connect:** check Steam, ownership, App ID and DLL dependencies.
- **No achievements in GUI, but CLI works:** use the Show diagnostics control and ensure the latest GUI is installed.
- **Unlock not available:** all achievements may already be unlocked, or the GUI is busy.
- **Changes not visible in Steam:** refresh and inspect Steam's own achievement display; StoreStats can fail.

This tool is unofficial, changes Steam achievement state only, and is not endorsed by Valve or Innersloth.
