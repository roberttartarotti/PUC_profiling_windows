param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('problem', 'solved')]
    [string]$Target,
    
    [Parameter(Mandatory=$false)]
    [switch]$CIMode
)

$ErrorActionPreference = "Stop"

# Import helper functions
. (Join-Path $PSScriptRoot "common\profiler-helpers.ps1")

# Configuration
$caseName = "case6-ui-virtualization"
$projectPath = Join-Path $PSScriptRoot "..\..\module8\case6-ui-virtualization\$Target"

# Results
$metrics = @{}
$errors = @()
$passed = $true

try {
    Write-ResultHeader "Case 6: UI Virtualization Check - $Target"
    
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
    
    # Measure dropdown open time (3 runs, take average)
    Write-Host "Measuring dropdown open time (3 runs)..." -ForegroundColor Yellow
    $times = @()
    
    for ($i = 1; $i -le 3; $i++) {
        Write-Host "  Run $i..." -ForegroundColor Gray
        $time = Measure-DropdownOpenTime -WorkingDirectory $projectPath
        $times += $time
        Write-Host "    Dropdown open time: $time ms" -ForegroundColor White
        Start-Sleep -Seconds 2  # Cooldown between runs
    }
    
    $avgTime = [math]::Round(($times | Measure-Object -Average).Average, 0)
    $metrics["dropdownOpenTimeMs"] = $avgTime
    
    Write-Host "`n Results:" -ForegroundColor Cyan
    Write-Host "  Run 1: $($times[0]) ms" -ForegroundColor White
    Write-Host "  Run 2: $($times[1]) ms" -ForegroundColor White
    Write-Host "  Run 3: $($times[2]) ms" -ForegroundColor White
    Write-Host "  Average: $avgTime ms" -ForegroundColor Yellow
    
    # Validate threshold
    Write-Host "`n Validating Threshold:" -ForegroundColor Cyan
    
    $result = Test-Threshold `
        -MetricName "Dropdown Open Time" `
        -ActualValue $avgTime `
        -Threshold $thresholds.dropdownOpenTimeMs
    
    if ($result.Passed) {
        Write-Host "  =)$($result.Message)" -ForegroundColor Green
    } else {
        Write-Host "  X $($result.Message)" -ForegroundColor Red
        $errors += $result.Message
        $passed = $false
    }
    
    # Summary
    Write-Host "`n Summary:" -ForegroundColor Cyan
    if ($passed) {
        Write-Host "  =) PASSED: Dropdown open time within expected threshold" -ForegroundColor Green
        
        if ($Target -eq "problem") {
            Write-Host "  → Problem correctly shows slow dropdown (10k items without virtualization)" -ForegroundColor Gray
        } else {
            Write-Host "  → Solved shows fast dropdown with VirtualizingStackPanel" -ForegroundColor Gray
        }
    } else {
        Write-Host "  X FAILED: Dropdown open time outside expected threshold" -ForegroundColor Red
        foreach ($merror in $errors) {
            Write-Host "    - $merror" -ForegroundColor Red
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
