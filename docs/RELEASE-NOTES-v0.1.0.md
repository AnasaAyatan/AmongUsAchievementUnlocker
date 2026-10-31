# Among Us Achievement Unlocker v0.1.0 — Release Candidate

**Status: DRAFT / TESTING. Not yet certified as a stable public release.**

## Highlights
- Windows Forms GUI with dark achievement list, generated readable names and original Steam IDs.
- Progress bar, total/remaining counts, text search and All / Unlocked / Locked filters.
- Collapsible diagnostics, refresh, and confirmation-protected unlock action.
- Steamworks.NET CLI backend and documented Windows release packaging.

## Requirements and installation
Windows x64, Steam with Among Us, .NET 8 Windows Desktop Runtime, compatible licensed Steamworks SDK 1.60 x64 `steam_api64.dll` (not bundled). Extract the full ZIP, place the authorized DLL in its `cli` folder, and launch `AmongUsAchievementUnlocker.Gui.exe`. Read [Windows Setup](WINDOWS-SETUP.md) before installing.

## Validation status
The CLI was confirmed to list 33/33 achievements unlocked on one Windows account. The GUI previously loaded them and the dark table appearance was verified. The latest GUI filter, progress, naming, and collapsible diagnostics changes **still need testing**, as does the fresh release ZIP. GitHub Actions compilation alone does not confirm Steam connectivity.

## Known limitations
- No installer; use ZIP extraction.
- Proprietary Steamworks native DLL cannot be bundled.
- Humanized achievement labels are not official Steam titles/descriptions.
- No per-achievement selection.
- Compatibility and achievement writes depend on Steam initialization and account permissions.
- This is not an in-game DLL mod and does not affect multiplayer gameplay.

## Publication checklist
Build CLI + GUI; verify latest controls; run packaging workflow; test fresh ZIP, DLL deployment, Steam refresh and absence of proprietary files; then review and publish draft release.

Unofficial project; not affiliated with Valve, Steam, or Innersloth.
