# run-monitor.ps1
# Script para executar o Performance Monitor

param(
    [switch]$AsAdmin,
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$monitorPath = $scriptDir

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Performance Monitor - Launcher" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Build se solicitado
if ($Build) {
    Write-Host "`n[BUILD] Compilando o projeto..." -ForegroundColor Yellow
    Push-Location $monitorPath
    dotnet build -c Release
    Pop-Location
}

# Verifica se o exe existe
$exePath = Join-Path $monitorPath "bin\Debug\net8.0-windows\PerformanceMonitor.exe"
if (-not (Test-Path $exePath)) {
    $exePath = Join-Path $monitorPath "bin\Release\net8.0-windows\PerformanceMonitor.exe"
}

if (-not (Test-Path $exePath)) {
    Write-Host "`n[INFO] Projeto não compilado. Compilando..." -ForegroundColor Yellow
    Push-Location $monitorPath
    dotnet build
    Pop-Location
    $exePath = Join-Path $monitorPath "bin\Debug\net8.0-windows\PerformanceMonitor.exe"
}

Write-Host "`n[INFO] Executando: $exePath" -ForegroundColor Green

if ($AsAdmin) {
    Write-Host "[INFO] Executando como Administrador (para ETW Traces)" -ForegroundColor Yellow
    Start-Process -FilePath $exePath -Verb RunAs
} else {
    Write-Host "[INFO] Executando normalmente (ETW Traces podem não funcionar)" -ForegroundColor Yellow
    Start-Process -FilePath $exePath
}

Write-Host "`n[OK] Performance Monitor iniciado!" -ForegroundColor Green
Write-Host ""
Write-Host "Dica: Use -AsAdmin para habilitar ETW Traces completos" -ForegroundColor DarkGray
