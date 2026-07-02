param(
    [string]$BuildDir
)

# =========================
# CONFIG
# =========================

$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong"
$PluginDir = Join-Path $GameDir "BepInEx\plugins\WeaverNet"
$Exe = Join-Path $GameDir "Hollow Knight Silksong.exe"

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
# DEPLOY
# =========================

Write-Host "Deploying mod..."

if (Test-Path $PluginDir) {
    Remove-Item $PluginDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PluginDir | Out-Null

# Copy ALL build outputs (important for Core.dll, dependencies, etc.)
Copy-Item (Join-Path $BuildDir "*") $PluginDir -Recurse -Force

Write-Host "Mod deployed successfully."

# =========================
# RUN GAME
# =========================

Write-Host "Launching game..."

Start-Process $Exe