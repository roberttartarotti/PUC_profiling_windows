# run-comparison.ps1
# Script para executar comparação lado a lado: Monitor + Problem vs Solved2

param(
    [switch]$AsAdmin
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$baseDir = Split-Path -Parent $scriptDir

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Performance Comparison Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Paths
$monitorPath = Join-Path $scriptDir "bin\Debug\net8.0-windows\PerformanceMonitor.exe"
$problemPath = Join-Path $baseDir "problem"
$solved2Path = Join-Path $baseDir "solved2"

# Verifica se monitor está compilado
if (-not (Test-Path $monitorPath)) {
    Write-Host "`n[BUILD] Compilando Performance Monitor..." -ForegroundColor Yellow
    Push-Location $scriptDir
    dotnet build
    Pop-Location
}

Write-Host "`n[STEP 1] Abrindo duas instâncias do Performance Monitor" -ForegroundColor Green

if ($AsAdmin) {
    Start-Process -FilePath $monitorPath -Verb RunAs
    Start-Sleep -Seconds 2
    Start-Process -FilePath $monitorPath -Verb RunAs
} else {
    Start-Process -FilePath $monitorPath
    Start-Sleep -Seconds 2
    Start-Process -FilePath $monitorPath
}

Write-Host "[OK] Monitors abertos!" -ForegroundColor Green

Write-Host "`n============================================" -ForegroundColor Yellow
Write-Host "  INSTRUÇÕES:" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Posicione as janelas do Monitor lado a lado" -ForegroundColor White
Write-Host ""
Write-Host "2. Execute os comandos abaixo em terminais separados:" -ForegroundColor White
Write-Host ""
Write-Host "   [PROBLEM - Lento]" -ForegroundColor Red
Write-Host "   cd '$problemPath'" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "   [SOLVED2 - Otimizado]" -ForegroundColor Green
Write-Host "   cd '$solved2Path'" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "3. No primeiro Monitor, selecione 'StartupPerformance.Problem'" -ForegroundColor White
Write-Host "4. No segundo Monitor, selecione 'StartupPerformance.Solved2'" -ForegroundColor White
Write-Host "5. Compare as métricas de CPU e memória!" -ForegroundColor White
Write-Host ""
Write-Host "============================================" -ForegroundColor Yellow

# Pergunta se deseja iniciar as aplicações automaticamente
$choice = Read-Host "Deseja iniciar Problem e Solved2 agora? (S/N)"
if ($choice -eq "S" -or $choice -eq "s") {
    Write-Host "`n[STEP 2] Iniciando aplicações de teste..." -ForegroundColor Green
    
    # Inicia Problem
    Write-Host "  -> Iniciando Problem..." -ForegroundColor Red
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$problemPath'; dotnet run"
    
    Start-Sleep -Seconds 3
    
    # Inicia Solved2
    Write-Host "  -> Iniciando Solved2..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$solved2Path'; dotnet run"
    
    Write-Host "`n[OK] Aplicações iniciadas!" -ForegroundColor Green
    Write-Host "Agora selecione os processos nos Monitors e clique 'Iniciar Monitoramento'" -ForegroundColor Yellow
}

Write-Host "`n[DONE] Setup completo!" -ForegroundColor Cyan
