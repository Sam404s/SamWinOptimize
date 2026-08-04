#Requires -Version 5.1
<#
.SYNOPSIS
    SamWinOptimize Installer / Uninstaller
.DESCRIPTION
    Zero-dependency PowerShell script to install or uninstall SamWinOptimize.
    Creates Desktop + Start Menu shortcuts, Add/Remove Programs registry entry.
.EXAMPLE
    .\install.ps1                  # Install (copies files from script directory)
    .\install.ps1 -Uninstall       # Uninstall
    .\install.ps1 -SourceDir "C:\path\to\files"
#>

param(
    [switch]$Uninstall,
    [string]$SourceDir = ""
)

$ErrorActionPreference = "Stop"

# ── Constants ──────────────────────────────────────────────────────────
$AppName        = "SamWinOptimize"
$AppVersion     = "1.0.1"
$AppPublisher   = "SamWinOptimize"
$AppExeName     = "SamWinOptimize.exe"
$InstallDir     = Join-Path $env:ProgramFiles $AppName
$RegKey         = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppName"
$StartMenuDir   = Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs"
$DesktopPath    = [Environment]::GetFolderPath("Desktop")

# ── Helper: Test admin ─────────────────────────────────────────────────
function Test-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    $wp = New-Object Security.Principal.WindowsPrincipal($id)
    return $wp.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Admin)) {
    Write-Host "[!] Need administrator privileges. Re-launching elevated..." -ForegroundColor Yellow
    $argList = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$PSCommandPath`"")
    if ($Uninstall)  { $argList += "-Uninstall" }
    if ($SourceDir)  { $argList += "-SourceDir"; $argList += "`"$SourceDir`"" }
    Start-Process powershell -ArgumentList $argList -Verb RunAs -Wait
    exit
}

# ── Uninstall ──────────────────────────────────────────────────────────
if ($Uninstall) {
    Write-Host ""
    Write-Host "=== Uninstalling $AppName $AppVersion ===" -ForegroundColor Cyan
    Write-Host ""

    # Kill running process
    $proc = Get-Process -Name "SamWinOptimize" -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "  Stopping running process..." -ForegroundColor Yellow
        $proc | Stop-Process -Force
        Start-Sleep -Milliseconds 500
    }

    # Remove shortcuts
    $desktopLnk = Join-Path $DesktopPath "$AppName.lnk"
    $startLnk   = Join-Path $StartMenuDir "$AppName.lnk"
    foreach ($lnk in @($desktopLnk, $startLnk)) {
        if (Test-Path $lnk) {
            Write-Host "  Removing shortcut: $lnk"
            Remove-Item $lnk -Force
        }
    }

    # Remove install directory
    if (Test-Path $InstallDir) {
        Write-Host "  Removing install directory: $InstallDir"
        Remove-Item $InstallDir -Recurse -Force
    }

    # Remove registry
    if (Test-Path $RegKey) {
        Write-Host "  Removing registry entry..."
        Remove-Item $RegKey -Force
    }

    Write-Host ""
    Write-Host "[OK] $AppName has been uninstalled." -ForegroundColor Green
    Write-Host ""
    exit 0
}

# ── Install ────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "=== Installing $AppName $AppVersion ===" -ForegroundColor Cyan
Write-Host ""

# Resolve source directory
if (-not $SourceDir) {
    $SourceDir = Split-Path -Parent $PSCommandPath
}
$SourceDir = (Resolve-Path $SourceDir).Path

$exePath = Join-Path $SourceDir $AppExeName
if (-not (Test-Path $exePath)) {
    Write-Host "[ERROR] Cannot find $AppExeName in $SourceDir" -ForegroundColor Red
    Write-Host "        Place this script next to the published files, or use -SourceDir." -ForegroundColor Red
    exit 1
}

# Kill running process
$proc = Get-Process -Name "SamWinOptimize" -ErrorAction SilentlyContinue
if ($proc) {
    Write-Host "  Stopping running process..." -ForegroundColor Yellow
    $proc | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

# Create install directory
Write-Host "  Install directory: $InstallDir"
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

# Copy files
Write-Host "  Copying files from $SourceDir ..."
$files = Get-ChildItem -Path $SourceDir -File | Where-Object { $_.Extension -ne ".ps1" }
foreach ($f in $files) {
    Copy-Item -Path $f.FullName -Destination $InstallDir -Force
    Write-Host "    $($f.Name) ($([math]::Round($f.Length/1MB, 2)) MB)"
}

$installedExe = Join-Path $InstallDir $AppExeName

# Create shortcuts
$WshShell = New-Object -ComObject WScript.Shell

$desktopLnk = Join-Path $DesktopPath "$AppName.lnk"
$sc = $WshShell.CreateShortcut($desktopLnk)
$sc.TargetPath = $installedExe
$sc.WorkingDirectory = $InstallDir
$sc.Description = "Windows Optimization Tool"
$sc.Save()
Write-Host "  Desktop shortcut created."

$startLnk = Join-Path $StartMenuDir "$AppName.lnk"
$sc2 = $WshShell.CreateShortcut($startLnk)
$sc2.TargetPath = $installedExe
$sc2.WorkingDirectory = $InstallDir
$sc2.Description = "Windows Optimization Tool"
$sc2.Save()
Write-Host "  Start Menu shortcut created."

# Registry (Add/Remove Programs)
if (-not (Test-Path $RegKey)) {
    New-Item -Path $RegKey -Force | Out-Null
}
Set-ItemProperty -Path $RegKey -Name "DisplayName"       -Value $AppName
Set-ItemProperty -Path $RegKey -Name "DisplayVersion"    -Value $AppVersion
Set-ItemProperty -Path $RegKey -Name "Publisher"         -Value $AppPublisher
Set-ItemProperty -Path $RegKey -Name "InstallLocation"   -Value $InstallDir
Set-ItemProperty -Path $RegKey -Name "DisplayIcon"       -Value $installedExe
Set-ItemProperty -Path $RegKey -Name "UninstallString"   -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"$InstallDir\install.ps1\" -Uninstall"
Set-ItemProperty -Path $RegKey -Name "QuietUninstallString" -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"$InstallDir\install.ps1\" -Uninstall"
Set-ItemProperty -Path $RegKey -Name "NoModify"          -Value 1 -Type DWord
Set-ItemProperty -Path $RegKey -Name "NoRepair"          -Value 1 -Type DWord

# Estimated size in KB
$totalSize = ($files | Measure-Object -Property Length -Sum).Sum
Set-ItemProperty -Path $RegKey -Name "EstimatedSize" -Value ([math]::Ceiling($totalSize / 1024)) -Type DWord

Write-Host "  Registry entry created (Add/Remove Programs)."

Write-Host ""
Write-Host "[OK] $AppName $AppVersion installed successfully!" -ForegroundColor Green
Write-Host "     Location: $InstallDir" -ForegroundColor Gray
Write-Host ""

