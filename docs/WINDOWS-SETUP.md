# Windows Setup — GUI Release Candidate

## What is included
- `AmongUsAchievementUnlocker.Gui.exe`: Windows Forms desktop interface.
- `cli/AmongUsAchievementUnlocker.exe`: the working Steamworks command-line backend.
- `cli/steam_appid.txt`: app context configuration.
- `START-HERE.md`: this file.

## Requirements
Windows 10/11 x64, Steam running with Among Us owned by the active Steam account, and the .NET 8 runtime (framework-dependent build). The package does not include the proprietary Steamworks native library.

## Installation
1. Extract the entire release ZIP into a folder of your choice, keeping the `cli` subdirectory intact.
2. Copy your legitimately obtained, compatible Windows x64 Steamworks SDK 1.60 `steam_api64.dll` into the `cli` folder beside `AmongUsAchievementUnlocker.exe`. Use only an authorized distribution. Never use arbitrary DLL download sites.
3. Keep the `steam_appid.txt` file included in `cli`.
4. Open Steam, sign in, and double-click `AmongUsAchievementUnlocker.Gui.exe`.
5. Click **Refresh** to read status. The app does not modify achievements until you click **Unlock remaining** and confirm.
6. Re-read results and check your Steam achievements page to verify saved changes.

## Local developer build from VS Code
```powershell
dotnet publish .\AmongUsAchievementUnlocker.csproj -c Release -r win-x64 --self-contained false -o .\dist\cli
dotnet publish .\gui\AmongUsAchievementUnlocker.Gui.csproj -c Release -r win-x64 --self-contained false -o .\dist
```
Put the compatible native SDK library in `dist/cli` locally and run `dist/AmongUsAchievementUnlocker.Gui.exe`. Do not commit proprietary DLLs.

## Troubleshooting
- **CLI executable missing**: Keep the packaged `cli` folder beside the GUI application.
- **Native DLL not found / entry point missing**: Install the correct authorized SDK 1.60 x64 redistributable in `cli`. Do not use the game's own older or incompatible DLL.
- **Steam API initialization failed**: Check Steam and ownership; the app cannot bypass Steam authorization.
- **No achievements found**: Inspect the diagnostic output and verify that the CLI works with `cli/AmongUsAchievementUnlocker.exe --list`.
- **Nothing left to unlock**: The unlock button is disabled once all reported achievements are unlocked.

This is unofficial software. Steam achievement changes can be visible and may be hard to reverse. No guarantee is made about compatibility, account policies, or future game updates.
