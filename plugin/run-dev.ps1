param(
    [string]$BuildDir
)

# =========================
# CONFIG
# =========================

$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong"
$PluginDir = Join-Path $GameDir "BepInEx\plugins\WeaverNet"
$Exe = Join-Path $GameDir "Hollow Knight Silksong.exe"

# Project containing the Data folder
$ProjectDir = Join-Path $PSScriptRoot "WeaverNet.Mod"
$DataDir = Join-Path $ProjectDir "Data"

# =========================
# INPUT SANITIZATION
# =========================

$BuildDir = $BuildDir.Trim('"')

Write-Host "BuildDir: $BuildDir"

# =========================
# VALIDATION
# =========================

if (!(Test-Path $BuildDir)) {
    Write-Host "ERROR: Build directory not found:"
    Write-Host $BuildDir
    exit 1
}

if (!(Test-Path $Exe)) {
    Write-Host "ERROR: Game executable not found:"
    Write-Host $Exe
    exit 1
}

# =========================
# STOP RUNNING GAME
# =========================

$ProcessName = [System.IO.Path]::GetFileNameWithoutExtension($Exe)
$RunningGame = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue

if ($RunningGame) {
    Write-Host "Closing running game..."

    $RunningGame | Stop-Process -Force

    # Wait until it has fully exited
    while (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue) {
        Start-Sleep -Milliseconds 200
    }

    # Give Windows a moment to release resources
    Start-Sleep -Seconds 2

    Write-Host "Game closed."
}

# =========================
# DEPLOY
# =========================

Write-Host "Deploying mod..."

if (Test-Path $PluginDir) {
    Remove-Item $PluginDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PluginDir | Out-Null

# Copy all build outputs (DLLs, PDBs, dependencies, etc.)
Copy-Item (Join-Path $BuildDir "*") $PluginDir -Recurse -Force

# Copy Data folder if it exists
if (Test-Path $DataDir) {
    Copy-Item $DataDir $PluginDir -Recurse -Force
    Write-Host "Copied Data directory."
}
else {
    Write-Host "WARNING: Data directory not found:"
    Write-Host $DataDir
}

Write-Host "Mod deployed successfully."

# =========================
# RUN GAME
# =========================

Write-Host "Launching game..."

Start-Process $Exe