param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('problem', 'solved1', 'solved2')]
    [string]$Target,
    
    [Parameter(Mandatory=$false)]
    [switch]$CIMode
)

$ErrorActionPreference = "Stop"

# Import helper functions
. (Join-Path $PSScriptRoot "common\profiler-helpers.ps1")

# Configuration
$caseName = "case4-startup-performance"
$projectPath = Join-Path $PSScriptRoot "..\..\module8\case4-wpf-startup-performance\$Target"

# Results
$metrics = @{}
$errors = @()
$passed = $true

try {
    Write-ResultHeader "Case 4: Startup Performance Check - $Target"
    
    # Get thresholds
    $thresholds = Get-Thresholds -CaseName $caseName -Target $Target
    
    Write-Host "Target: $Target" -ForegroundColor Yellow
    Write-Host "Project: $projectPath`n" -ForegroundColor Gray
    
    # Build project
    Write-Host "Building project..." -ForegroundColor Yellow
    Push-Location $projectPath
    $buildOutput = dotnet build -c Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed: $buildOutput"
    }
    Pop-Location
    Write-Host "=) Build successful`n" -ForegroundColor Green
    
    # Measure startup time (3 runs, take average)
    Write-Host "Measuring startup time (3 runs)..." -ForegroundColor Yellow
    $times = @()
    
    for ($i = 1; $i -le 3; $i++) {
        Write-Host "  Run $i..." -ForegroundColor Gray
        $time = Measure-StartupTime -WorkingDirectory $projectPath
        $times += $time
        Write-Host "    Time: $time ms" -ForegroundColor White
        Start-Sleep -Seconds 2  # Cooldown between runs
    }
    
    $avgTime = [math]::Round(($times | Measure-Object -Average).Average, 0)
    $metrics["startupTimeMs"] = $avgTime
    
    Write-Host "`n Results:" -ForegroundColor Cyan
    Write-Host "  Run 1: $($times[0]) ms" -ForegroundColor White
    Write-Host "  Run 2: $($times[1]) ms" -ForegroundColor White
    Write-Host "  Run 3: $($times[2]) ms" -ForegroundColor White
    Write-Host "  Average: $avgTime ms" -ForegroundColor Yellow
    
    # Validate threshold
    Write-Host "`n Validating Threshold:" -ForegroundColor Cyan
    
    $result = Test-Threshold `
        -MetricName "Startup Time" `
        -ActualValue $avgTime `
        -Threshold $thresholds.startupTimeMs
    
    if ($result.Passed) {
        Write-Host "  =)$($result.Message)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $($result.Message)" -ForegroundColor Red
        $errors += $result.Message
        $passed = $false
    }
    
    # Summary
    Write-Host "`n Summary:" -ForegroundColor Cyan
    if ($passed) {
        Write-Host "  =) PASSED: Startup time within expected threshold" -ForegroundColor Green
        
        # Additional context
        if ($Target -eq "problem") {
            Write-Host "  → Problem correctly shows slow startup (loading 10k+ records synchronously)" -ForegroundColor Gray
        } elseif ($Target -eq "solved1") {
            Write-Host "  → Solved1 shows fast startup with lazy loading" -ForegroundColor Gray
        } else {
            Write-Host "  → Solved2 shows fast startup with background preload" -ForegroundColor Gray
        }
    } else {
        Write-Host "  X FAILED: Startup time outside expected threshold" -ForegroundColor Red
        foreach ($myerror in $errors) {
            Write-Host "    - $myerror" -ForegroundColor Red
        }
    }
    
    # CI Mode output
    if ($CIMode) {
        $jsonResult = ConvertTo-JsonResult `
            -CaseName $caseName `
            -Target $Target `
            -Metrics $metrics `
            -Passed $passed `
            -Errors $errors
        
        $outputFile = Join-Path $PSScriptRoot "..\results\$caseName-$Target.json"
        New-Item -Path (Split-Path $outputFile) -ItemType Directory -Force | Out-Null
        $jsonResult | Out-File -FilePath $outputFile -Encoding UTF8
        
        Write-Host "`n Results saved to: $outputFile" -ForegroundColor Gray
    }
}
catch {
    Write-Host "`nX Error: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Gray
    $passed = $false
}
finally {
    Write-ResultFooter -ExitCode $(if ($passed) { 0 } else { 1 })
    exit $(if ($passed) { 0 } else { 1 })
}
