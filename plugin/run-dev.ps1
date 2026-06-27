param(
    [string]$DllPath
)

# =========================
# CONFIG
# =========================
$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight Silksong\"
$PluginDir = Join-Path $GameDir "BepInEx\plugins\WeaverNet"
$Exe = Join-Path $GameDir "Hollow Knight Silksong.exe"

# =========================
# INPUT SANITIZATION
# =========================

$DllPath = $DllPath.Trim('"')

Write-Host "DLL Path: $DllPath"

# =========================
# VALIDATION
# =========================

if (!(Test-Path $DllPath)) {
    Write-Host "ERROR: DLL not found at build output:"
    Write-Host $DllPath
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

Copy-Item $DllPath $PluginDir -Force

Write-Host "Mod deployed successfully."

# =========================
# RUN GAME
# =========================

Write-Host "Launching game..."

Start-Process $Exe