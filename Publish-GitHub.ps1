$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
foreach ($cmd in @('git','gh')) {
  if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
    throw "Missing '$cmd'. Install Git and GitHub CLI before running."
  }
}
gh auth status
if ($LASTEXITCODE -ne 0) { throw 'Run gh auth login first.' }
if (-not (Test-Path .git)) {
  git init -b main
  if ($LASTEXITCODE -ne 0) { throw 'git init failed.' }
}
git add .
if ($LASTEXITCODE -ne 0) { throw 'git add failed.' }
git diff --cached --quiet
if ($LASTEXITCODE -ne 0) {
  git commit -m 'Initial Among Us achievement tool project'
  if ($LASTEXITCODE -ne 0) { throw 'Git commit failed. Configure git user.name and user.email.' }
}
$remote = git remote get-url origin 2>$null
if ($LASTEXITCODE -eq 0 -and $remote) {
  Write-Host "Existing GitHub remote: $remote" -ForegroundColor Yellow
  Write-Host 'No repository created. Push explicitly with: git push -u origin main'
  exit 0
}
Write-Host 'This creates a PRIVATE GitHub repository and pushes your code.' -ForegroundColor Yellow
$answer = Read-Host 'Type PUBLISH to continue'
if ($answer -cne 'PUBLISH') { Write-Host 'Cancelled'; exit 0 }
gh repo create AmongUsAchievementUnlocker --private --source . --remote origin --push
if ($LASTEXITCODE -ne 0) { throw 'GitHub repository creation/push failed.' }
gh repo view --web
