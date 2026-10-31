# Development and Test Guide

## Projects
- Root `AmongUsAchievementUnlocker.csproj`: Steamworks.NET .NET 8 **CLI**; excludes `gui/**/*.cs`.
- `Program.cs`: Steam initialization, stats read/write and error handling.
- `gui/AmongUsAchievementUnlocker.Gui.csproj`: .NET 8 Windows Forms desktop UI.
- `gui/MainForm.cs`: search, status filters, progress bar, table, diagnostics, refresh and unlock confirmation. The GUI launches the CLI in a child process from `cli/`.
- `steam_appid.txt`: expected application context; does not grant ownership.
- `.github/workflows/build.yml`: source compilation.
- `.github/workflows/release-candidate.yml`: manual Windows GUI + CLI ZIP packaging, no proprietary Steam DLL.

## Local build

On Windows with VS Code, .NET 8 SDK, Git and Steam:

```powershell
git pull --ff-only origin main
dotnet publish .\AmongUsAchievementUnlocker.csproj -c Release -r win-x64 --self-contained false -o .\dist\cli
dotnet publish .\gui\AmongUsAchievementUnlocker.Gui.csproj -c Release -r win-x64 --self-contained false -o .\dist
```

Place a licensed compatible SDK 1.60 x64 `steam_api64.dll` **locally** in `dist/cli`. Then run `& .\dist\AmongUsAchievementUnlocker.Gui.exe`. Do not commit the native binary. Both projects are built separately.

## Verification checklist

1. Build the root CLI and GUI with zero errors.
2. Run GUI with Steam open; verify a 33-achievement list for current observed game configuration.
3. Confirm overall unlocked/remaining values match CLI `--list`.
4. Test searching by both generated name and internal ID.
5. Test All / Unlocked / Locked filters. At 33/33, Locked should be empty and unlock disabled.
6. Confirm progress bar reaches 100% for 33/33.
7. Toggle diagnostics twice, checking it hides/shows Steam output and does not obscure table.
8. Test refreshing and resizing/maximizing the window.
9. Verify missing/incompatible native library failures show usable messages.
10. Extract a newly built GitHub artifact to a fresh directory, locally supply licensed runtime, test again.
11. Check the artifact **does not contain** `steam_api64.dll`.
12. Only perform write tests on your own authorized account; confirm Steam acknowledgement and verify on a new read.

## Release policy
Keep `v0.1.0` in draft until the latest GUI and extracted artifact pass. Source pushed to main is not equivalent to an approved downloadable release. Avoid tagging a release based only on CI compilation.

## Future enhancements
Official Steam achievement display names/descriptions, optional per-achievement selection, graphical icon/version branding, improved accessibility and native-dependency setup. These features are **not** part of the current release candidate.

## Security and troubleshooting
No credential collection. Avoid publishing game binaries or Steam IDs; redact account identifiers in logs. An app ID text file does not override Steam authorization. See [Windows Setup](WINDOWS-SETUP.md) for troubleshooting and runtime details.
