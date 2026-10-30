# Among Us Achievement Unlocker (experimental)

C# / .NET 8 utility for reading and requesting Steam achievement changes for Among Us (Steam App ID `945360`). **This is not a verified in-game mod**; Steam API initialization may reject a separate process. No bypasses, injection, anti-cheat modification or multiplayer gameplay modifications are provided.

## Get started on Windows with VS Code

1. Install Git, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [VS Code](https://code.visualstudio.com/), [GitHub CLI](https://cli.github.com/) and the recommended VS Code C# extensions.
2. Extract this folder to `%USERPROFILE%\Documents\AmongUsAchievementUnlocker` (or your actual Windows Documents location).
3. Open the folder in VS Code using **File > Open Folder**.
4. Open **Terminal > New Terminal** and run:

```powershell
dotnet --info
dotnet restore
dotnet build -c Release
dotnet run -c Release -- --list
```

Use **Terminal > Run Task** for preconfigured restore/build/preview tasks. The `--list` command only reads Steam achievements. You can also run `dotnet run -c Release -- --unlock` to request achievement changes after typing `UNLOCK`.

## Publish to GitHub

Log in first: `gh auth login`. Then run `./Publish-GitHub.ps1` from the VS Code PowerShell terminal. It initializes Git, commits the project and, after **PUBLISH** confirmation, creates a **private** GitHub repository and pushes the source. It does not overwrite existing GitHub repositories. If a remote already exists, it only prints the remote and the manual push command.

Alternatively, use `git init -b main`, `git add .`, `git commit -m "Initial commit"`, then `gh repo create AmongUsAchievementUnlocker --private --source . --remote origin --push`.

## Project structure

- `Program.cs`: read-only preview and confirmation-gated Steam unlock request
- `AmongUsAchievementUnlocker.csproj`: .NET and Steamworks.NET dependency configuration
- `.vscode/tasks.json`: restore, build, preview, unlock tasks
- `.github/workflows/build.yml`: Windows GitHub Actions build check
- `Publish-GitHub.ps1`: optional private repo creation
- `steam_appid.txt`: Steam app context marker (not proof of ownership)

## Caveats

- **Build is not verified in this delivery environment**, and no live Steam client was available for testing.
- Steam may refuse to initialize or persist updates in a standalone executable. The app ID file cannot grant permissions or establish game ownership.
- Achievements may be visible and changes potentially irreversible. Use only on an account you control and review Steam/game rules.
- CI validates compilation only; it cannot authenticate to Steam or unlock achievements.
- If `SteamAPI.Init` fails, stop: this project does not circumvent Steam integrity or authorization checks.
