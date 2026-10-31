# Among Us Achievement Unlocker

Unofficial Windows x64 achievement manager for **Among Us** (Steam App ID `945360`). This is a standalone Windows Forms GUI with a Steamworks.NET command-line backend, **not** an injected game mod. It does not alter multiplayer gameplay.

> Not affiliated with Innersloth, Valve, or Steam. Use only with a Steam account you control. Achievement modifications may be visible and difficult to reverse; review applicable rules.

## Release status

**v0.1.0 release candidate (not yet a verified published release).** Local Windows testing confirmed CLI compilation, achievement access, 33/33 achievements unlocked, and operation of the GUI with dark table styling. The latest UI additions (progress bar, status filters, humanized labels, collapsible diagnostics) and the downloadable ZIP **still require validation**. Do not represent these features as tested until the release candidate is run.

## Features

### Windows GUI (`gui/`)
- Three-column achievement table: readable generated label, exact Steam achievement ID, and unlock status
- Search by ID or generated name; filter All / Unlocked / Locked
- Progress bar and unlocked/remaining counts
- Refresh from Steam; confirmation-protected **Unlock remaining**
- Collapsible diagnostics with the raw backend output
- Dark table styling and improved button contrast

The display labels are generated from IDs, **not official Steam achievement titles**. Individual achievement selection, official descriptions, and an installer are **not implemented**.

### CLI (`Program.cs`)
- `--list`: read-only achievement listing
- `--unlock`: confirmation-gated attempt to unlock remaining achievements
- No arguments: interactive menu, defaulting to a read-only listing
- Steam App ID verification, asynchronous callback handling, and native-library error messages

## Requirements

Windows 10/11 x64; Steam running and signed in with Among Us owned; .NET 8 runtime (including **Windows Desktop Runtime** for the GUI); .NET 8 SDK for development. The project references Steamworks.NET `2024.8.0` (Steamworks SDK 1.60). A compatible, lawfully obtained x64 `steam_api64.dll` must be installed **locally beside the CLI executable**. Neither the repository nor the GitHub ZIP redistributes Valve's proprietary native binary. Do not use arbitrary DLL download sites or assume a DLL copied from a game is compatible.

## Installation and launch

See **[Windows GUI Setup](docs/WINDOWS-SETUP.md)** for ZIP contents, runtime installation, build commands, verification and troubleshooting. The GUI is `AmongUsAchievementUnlocker.Gui.exe` in the extracted ZIP root, while the backend is `cli/AmongUsAchievementUnlocker.exe`.

### Develop in VS Code

```powershell
git pull --ff-only origin main
dotnet publish .\AmongUsAchievementUnlocker.csproj -c Release -r win-x64 --self-contained false -o .\dist\cli
dotnet publish .\gui\AmongUsAchievementUnlocker.Gui.csproj -c Release -r win-x64 --self-contained false -o .\dist
# Install your compatible native DLL into dist\cli locally before launching:
& .\dist\AmongUsAchievementUnlocker.Gui.exe
```

The application requires its `cli` folder alongside the GUI. For CLI-only testing, use `dotnet run -c Release -- --list` with the compatible native DLL in its runtime search directory.

## GitHub release process

1. Pull the latest `main`, build both projects, and validate the latest UI.
2. Run the manual [Windows Release Candidate workflow](.github/workflows/release-candidate.yml).
3. Download the generated artifact, extract to a **new** directory, and supply the native runtime locally as described above.
4. Test GUI refresh, search, filters, progress, diagnostics and CLI listing from that extracted ZIP; check no proprietary DLL is bundled.
5. Only then create a **draft** `v0.1.0` release, review the [release notes](docs/RELEASE-NOTES-v0.1.0.md), and publish when satisfied.

GitHub Actions compilation and packaging do **not** test a live Steam session.

## Documentation

- [Windows setup](docs/WINDOWS-SETUP.md)
- [Developer guide and test checklist](docs/DEVELOPMENT.md)
- [v0.1.0 release notes](docs/RELEASE-NOTES-v0.1.0.md)
- [Changelog](CHANGELOG.md)

## Troubleshooting

| Problem | Action |
| --- | --- |
| Missing CLI executable | Preserve the `cli` directory next to the GUI executable |
| `DllNotFoundException` | Check the native Steamworks DLL is present beside the CLI and that its dependencies are installed |
| `EntryPointNotFoundException` | Native DLL version is incompatible with Steamworks.NET |
| `BadImageFormatException` | Verify Windows x64 native DLL |
| `SteamAPI.Init failed` | Check Steam client, ownership and application context; initialization cannot be forced |
| Achievements appear in CLI but not GUI | Open diagnostics, check parsing and use the current build |
| Failed `StoreStats` | Do not assume a write was saved; refresh and check Steam |

Redact Steam IDs and personal details from shared logs. Do not commit `dist/`, binary files, game files, credentials, or the native Steamworks DLL.
