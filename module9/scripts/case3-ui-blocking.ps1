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
$caseName = "case3-ui-blocking"
$projectPath = Join-Path $PSScriptRoot "..\..\module8\case3-ui-thread-blocking\$Target"

# Results
$metrics = @{}
$errors = @()
$passed = $true

try {
    Write-ResultHeader "Case 3: UI Thread Blocking Check - $Target"
    
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
    
    # Measure UI responsiveness
    Write-Host "Starting application and measuring UI responsiveness..." -ForegroundColor Yellow
    
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $process = Start-Process -FilePath "dotnet" `
                             -ArgumentList "run" `
                             -WorkingDirectory $projectPath `
                             -PassThru `
                             -WindowStyle Normal
    
    try {
        # Wait for UI to initialize
        Start-Sleep -Seconds 3
        
        # Measure time until UI is responsive
        # For WPF, we check if main window thread is not blocked
        $uiBlockedTime = 0
        $checkInterval = 100 # ms
        $maxWait = 10000 # 10 seconds max
        $elapsed = 0
        
        while ($elapsed -lt $maxWait -and -not $process.HasExited) {
            $cpuBefore = $process.TotalProcessorTime
            Start-Sleep -Milliseconds $checkInterval
            $cpuAfter = $process.TotalProcessorTime
            
            $cpuUsed = ($cpuAfter - $cpuBefore).TotalMilliseconds
            
            # If CPU is maxed out, UI is likely blocked
            if ($cpuUsed -gt ($checkInterval * 0.8)) {
                $uiBlockedTime += $checkInterval
            }
            
            $elapsed += $checkInterval
            
            # For "problem", expect blocking; for "solved", expect responsiveness
            if ($Target -eq "solved" -and $elapsed -gt 2000) {
                break # Solved should be responsive quickly
            }
            if ($Target -eq "problem" -and $uiBlockedTime -gt 3000) {
                break # Problem should show blocking
            }
        }
        
        $sw.Stop()
        
        $metrics["uiThreadBlockedMs"] = $uiBlockedTime
        $metrics["uiResponsive"] = $uiBlockedTime -lt 500
        
        Write-Host "`n Metrics Collected:" -ForegroundColor Cyan
        Write-Host "  UI Thread Blocked Time: $uiBlockedTime ms" -ForegroundColor White
        Write-Host "  UI Responsive: $($metrics.uiResponsive)" -ForegroundColor $(if ($metrics.uiResponsive) { "Green" } else { "Red" })
        
        # Validate thresholds
        Write-Host "`n Validating Thresholds:" -ForegroundColor Cyan
        
        # Check UI Thread Blocked Time
        $result = Test-Threshold `
            -MetricName "UI Thread Blocked Time" `
            -ActualValue $metrics.uiThreadBlockedMs `
            -Threshold $thresholds.uiThreadBlockedMs
        
        if ($result.Passed) {
            Write-Host "  =)$($result.Message)" -ForegroundColor Green
        } else {
            Write-Host "  X $($result.Message)" -ForegroundColor Red
            $errors += $result.Message
            $passed = $false
        }
        
        # Check UI Responsiveness expectation
        $expectedResponsive = $thresholds.uiResponsive
        if ($metrics.uiResponsive -eq $expectedResponsive) {
            Write-Host "  =) UI Responsiveness matches expected: $expectedResponsive" -ForegroundColor Green
        } else {
            $msg = "UI Responsiveness = $($metrics.uiResponsive) (expected $expectedResponsive)"
            Write-Host "  X $msg" -ForegroundColor Red
            $errors += $msg
            $passed = $false
        }
    }
    catch {
        throw
    }
    finally {
        Stop-ProcessSafely -Process $process
    }
    
    # Summary
    Write-Host "`n Summary:" -ForegroundColor Cyan
    if ($passed) {
        Write-Host "  =) PASSED: UI behavior matches expectations" -ForegroundColor Green
        
        if ($Target -eq "problem") {
            Write-Host "  → Problem correctly shows UI thread blocking" -ForegroundColor Gray
        } else {
            Write-Host "  → Solved shows responsive UI with async operations" -ForegroundColor Gray
        }
    } else {
        Write-Host "  X FAILED: $($errors.Count) metric(s) outside expectations" -ForegroundColor Red
        foreach ($myError in $errors) {
            Write-Host "    - $myError" -ForegroundColor Red
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
