# Development Guide

## Environment
Windows x64, VS Code with C# extension, .NET 8 SDK, Git and GitHub CLI. Ensure the repository is a local checkout: `git remote -v`.

## Standard workflow
```powershell
git pull --ff-only origin main
dotnet restore
dotnet build -c Release
dotnet run -c Release -- --list
```

The read-only smoke test requires Steam to be running and a compatible native x64 Steamworks SDK runtime installed locally. CI can compile without a running Steam client.

## Code map
- `Program.cs`: application entry point, argument parsing/menu, Steam initialization, callback polling, achievement reads and confirmation-gated writes.
- `AmongUsAchievementUnlocker.csproj`: .NET 8 x64 build and Steamworks.NET package.
- `steam_appid.txt`: expected application context marker; not proof of ownership.
- `.github/workflows/build.yml`: build validation.

## Validation checklist
1. `dotnet build -c Release` returns zero errors.
2. No-argument menu defaults to read-only.
3. `--list` enumerates achievements without changes.
4. Unexpected App ID fails closed.
5. Missing/mismatched native DLL produces a usable diagnostic.
6. `--unlock` requires the literal `UNLOCK`; aborting makes no changes.
7. Observe `StoreStats` acknowledgment and verify with a **new** `--list` invocation and Steam UI.

Never test write operations against an account without the owner's explicit authorization. Do not commit game files, SDK DLLs, account identifiers or secrets.

## Roadmap
- v0.1.0: test and package working console prototype.
- Later: optional graphical interface, selection by achievement, structured diagnostic output and improved developer setup.

## Versioning
Source commits may land before a tagged release. Tag only after checking the packaged executable on a clean Windows test setup.
