# Among Us Achievement Unlocker

Experimental standalone Windows utility for viewing and requesting Steam achievement changes for **Among Us** (Steam App ID `945360`).

> Unofficial community project. Not affiliated with, endorsed by, or supported by Innersloth or Valve. This tool changes Steam achievement records; it does not modify matches, provide gameplay advantages, inject into the game, or bypass anti-cheat.

## Status

**v0.1.0 release candidate — source development.** The developer's Windows test successfully read all 33 Steam achievements and a subsequent read showed 33/33 unlocked. This is a report from one installation, not a guarantee of compatibility for other accounts or future game updates. Packaged release testing is still outstanding.

## Features

- Read achievement states with `--list`
- Confirmation-gated unlock requests with `--unlock`
- Interactive menu when launched with no arguments (defaults to read-only mode)
- Checks that Steam returns App ID `945360`
- Waits for Steam user stats and save callbacks
- Clearer diagnostics for missing, wrong-architecture or incompatible native Steamworks libraries
- No account credentials, Steam passwords, or login tokens collected

**Not yet implemented:** graphical Windows interface, per-achievement selection, automatic installation of a licensed native Steamworks runtime, or a tested installer.

## Requirements

- Windows x64, Steam running and signed in to the account that owns Among Us
- Among Us installed
- .NET 8 SDK (development) or the appropriate .NET 8 desktop/runtime dependencies for packaged builds
- Steamworks.NET NuGet version `2024.8.0` (Steamworks SDK 1.60)
- A compatible x64 `steam_api64.dll` obtained from an authorized Steamworks SDK distribution (not included in this repository)

Do **not** blindly copy `steam_api64.dll` from an unrelated Steam game: it may expose a different SDK interface, producing `EntryPointNotFoundException`. Do not download native DLLs from untrusted third-party DLL sites.

## Quick start for developers

Open this directory in VS Code, then use its PowerShell terminal:

```powershell
dotnet restore
dotnet build -c Release
dotnet run -c Release -- --list
```

The native Steamworks SDK x64 DLL must be available in the executable's runtime search path before starting. When using `dotnet run`, check the output under `bin/Release/net8.0/win-x64/`. Keep proprietary SDK binaries **out of source control**.

To open the interactive menu:

```powershell
dotnet run -c Release --
```

To request unlocking remaining achievements:

```powershell
dotnet run -c Release -- --unlock
```

The tool shows current states first and requires the exact confirmation word `UNLOCK` before writing. It uses SteamUserStats.SetAchievement and StoreStats, then waits for acknowledgement. Always check the Steam achievement page and re-run `--list` to confirm persistence.

## Updating a GitHub checkout

```powershell
git status
git pull --ff-only origin main
dotnet restore
dotnet build -c Release
```

Avoid committing compiled binaries, local game files, Steam IDs, secrets, or proprietary `steam_api64.dll`.

## Troubleshooting

| Symptom | Likely cause / resolution |
| --- | --- |
| `DllNotFoundException` | A compatible x64 `steam_api64.dll` or its transitive dependencies are missing from the runtime search path. |
| `EntryPointNotFoundException` | The installed native Steam DLL does not match the managed Steamworks.NET SDK version. |
| `BadImageFormatException` | 32-bit/64-bit native library mismatch; use Windows x64 DLL. |
| `SteamAPI.Init failed` | Steam is closed, game context was rejected, or configuration is incomplete; ownership and `steam_appid.txt` alone do not guarantee successful initialization. |
| Wrong App ID | The tool refuses to proceed when Steam does not report `945360`. |
| `StoreStats` failed or timed out | Steam did not acknowledge saving; retry reading state, check connectivity, and do not assume achievements changed. |
| Build succeeds but run fails | Compilation does not verify native DLL deployment or Steam authorization. |

For diagnostic support, share the exception text and app output, but **redact Steam account IDs and other identifying data**.

## Safety and limitations

Use only on a Steam account you control. Unlocking achievements may be visible to other users, irreversible in practice, or governed by Valve/Among Us rules. This software provides no guarantee against account restrictions or game updates. It does not impersonate Steam, alter protected processes, or circumvent authorization failures.

## Development and releases

The GitHub Actions build checks C# compilation on Windows without requiring Steam credentials. A successful CI build does **not** mean Steam API behavior was tested. Official Steamworks native redistributables must be handled according to their licenses and are not uploaded by this repository.

See [CHANGELOG.md](CHANGELOG.md) for changes and [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for contributor guidance.
