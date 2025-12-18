# Profiler Helper Functions
# Funções compartilhadas para os scripts de profiling

function Get-Thresholds {
    param(
        [string]$CaseName,
        [string]$Target
    )
    
    $thresholdsPath = Join-Path $PSScriptRoot "thresholds.json"
    $thresholds = Get-Content $thresholdsPath | ConvertFrom-Json
    
    return $thresholds.$CaseName.$Target
}

function Write-ResultHeader {
    param([string]$Title)
    
    Write-Host "`n==========================================" -ForegroundColor Cyan
    Write-Host " $Title" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
}

function Write-ResultFooter {
    param([int]$ExitCode)
    
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Exit Code: $ExitCode" -ForegroundColor $(if ($ExitCode -eq 0) { "Green" } else { "Red" })
    Write-Host "==========================================`n" -ForegroundColor Cyan
}

function Test-Threshold {
    param(
        [string]$MetricName,
        [double]$ActualValue,
        [PSCustomObject]$Threshold
    )
    
    $passed = $true
    $message = ""
    
    # Sempre usa limite superior (max)
    if ($Threshold.PSObject.Properties.Name -contains "max") {
        if ($ActualValue -gt $Threshold.max) {
            $passed = $false
            $message = "$MetricName = $ActualValue (expected <= $($Threshold.max))"
        } else {
            $message = "$MetricName = $ActualValue (OK)"
        }
    } else {
        # Fallback se não tiver max definido
        $message = "$MetricName = $ActualValue (no threshold defined)"
    }
    
    return @{
        Passed = $passed
        Message = $message
    }
}

function Start-ProcessAndWait {
    param(
        [string]$WorkingDirectory,
        [string]$Command = "dotnet",
        [string[]]$Arguments = @("run"),
        [int]$WaitSeconds = 5
    )
    
    Write-Host "Starting process..." -ForegroundColor Yellow
    Write-Host "  Directory: $WorkingDirectory" -ForegroundColor Gray
    Write-Host "  Command: $Command $($Arguments -join ' ')" -ForegroundColor Gray
    
    $process = Start-Process -FilePath $Command `
                             -ArgumentList $Arguments `
                             -WorkingDirectory $WorkingDirectory `
                             -PassThru `
                             -WindowStyle Hidden
    
    if (-not $process) {
        throw "Failed to start process"
    }
    
    Write-Host "  PID: $($process.Id)" -ForegroundColor Green
    
    # Aguardar processo inicializar
    Start-Sleep -Seconds $WaitSeconds
    
    # Verificar se ainda está rodando
    if ($process.HasExited) {
        throw "Process exited prematurely with code $($process.ExitCode)"
    }
    
    return $process
}

function Stop-ProcessSafely {
    param(
        [System.Diagnostics.Process]$Process
    )
    
    if ($Process -and -not $Process.HasExited) {
        try {
            $Process.Kill()
            $Process.WaitForExit(5000)
            Write-Host "Process stopped" -ForegroundColor Gray
        }
        catch {
            Write-Warning "Failed to stop process: $_"
        }
    }
}

function ConvertTo-JsonResult {
    param(
        [string]$CaseName,
        [string]$Target,
        [hashtable]$Metrics,
        [bool]$Passed,
        [string[]]$Errors
    )
    
    return @{
        case = $CaseName
        target = $Target
        timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ss"
        passed = $Passed
        metrics = $Metrics
        errors = $Errors
    } | ConvertTo-Json -Depth 5
}

function Get-DotnetCounters {
    param(
        [int]$ProcessId,
        [int]$DurationSeconds = 10,
        [string[]]$Counters = @(
            "System.Runtime",
            "Microsoft.AspNetCore.Hosting",
            "System.Net.Http"
        )
    )
    
    Write-Host "Collecting metrics for $DurationSeconds seconds..." -ForegroundColor Yellow
    
    $tempFile = [System.IO.Path]::GetTempFileName()
    
    try {
        # Capturar counters
        $counterArgs = @(
            "collect",
            "--process-id", $ProcessId,
            "--refresh-interval", "1",
            "--format", "csv",
            "--output", $tempFile
        )
        
        foreach ($counter in $Counters) {
            $counterArgs += "--counters"
            $counterArgs += $counter
        }
        
        $job = Start-Job -ScriptBlock {
            param($args)
            dotnet-counters @args
        } -ArgumentList (,$counterArgs)
        
        # Aguardar duração especificada
        Start-Sleep -Seconds $DurationSeconds
        
        # Parar coleta
        Stop-Job $job
        Remove-Job $job
        
        # Ler e parsear resultados
        if (Test-Path $tempFile) {
            $content = Get-Content $tempFile
            return $content
        }
        
        return $null
    }
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force
        }
    }
}

function Measure-DropdownOpenTime {
    param(
        [string]$WorkingDirectory,
        [string]$WindowTitlePattern = "UI Virtualization*",
        [string]$ComboBoxAutomationId = "ClientesComboBox"
    )
    
    # Load UIAutomation assemblies (ignore if already loaded)
    try {
        [void][System.Reflection.Assembly]::Load('UIAutomationClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35')
        [void][System.Reflection.Assembly]::Load('UIAutomationTypes, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35')
    } catch {}
    
    $dropdownTime = 0
    $process = $null
    $appProcess = $null
    
    try {
        # Start the application
        $process = Start-Process -FilePath "dotnet" `
                                 -ArgumentList "run" `
                                 -WorkingDirectory $WorkingDirectory `
                                 -PassThru
        
        if (-not $process) {
            throw "Failed to start process"
        }
        
        Write-Host "    Started dotnet process PID: $($process.Id)" -ForegroundColor Gray
        
        # Wait for the window to appear
        $timeout = 30
        $mainWindow = $null
        $startTime = Get-Date
        
        while (((Get-Date) - $startTime).TotalSeconds -lt $timeout) {
            if ($process.HasExited) {
                throw "Process exited prematurely"
            }
            
            # Find the app process
            if (-not $appProcess) {
                $appProcess = Get-Process -Name "UIVirtualization*" -ErrorAction SilentlyContinue | 
                    Select-Object -First 1
            }
            
            if ($appProcess) {
                try {
                    $rootElement = [System.Windows.Automation.AutomationElement]::RootElement
                    $condition = [System.Windows.Automation.Condition]::TrueCondition
                    $windows = $rootElement.FindAll(
                        [System.Windows.Automation.TreeScope]::Children,
                        $condition
                    )
                    
                    foreach ($window in $windows) {
                        try {
                            $windowPid = $window.GetCurrentPropertyValue(
                                [System.Windows.Automation.AutomationElement]::ProcessIdProperty
                            )
                            
                            if ($windowPid -eq $appProcess.Id) {
                                $windowTitle = $window.GetCurrentPropertyValue(
                                    [System.Windows.Automation.AutomationElement]::NameProperty
                                )
                                
                                if ($windowTitle -like $WindowTitlePattern) {
                                    $mainWindow = $window
                                    Write-Host "    Found window: '$windowTitle'" -ForegroundColor Green
                                    break
                                }
                            }
                        }
                        catch {}
                    }
                }
                catch {}
            }
            
            if ($mainWindow) {
                break
            }
            
            Start-Sleep -Milliseconds 200
        }
        
        if (-not $mainWindow) {
            throw "Could not find main window"
        }
        
        # Wait a bit for the window to fully load
        Start-Sleep -Milliseconds 1000
        
        # Find the ComboBox by AutomationId
        Write-Host "    Looking for ComboBox..." -ForegroundColor Gray
        $comboBox = $null
        
        try {
            $automationIdProperty = [System.Windows.Automation.AutomationElement]::AutomationIdProperty
            $comboBoxCondition = New-Object System.Windows.Automation.PropertyCondition(
                $automationIdProperty, 
                $ComboBoxAutomationId
            )
            $comboBox = $mainWindow.FindFirst(
                [System.Windows.Automation.TreeScope]::Descendants,
                $comboBoxCondition
            )
        }
        catch {
            Write-Host "    Error finding ComboBox by AutomationId: $_" -ForegroundColor Yellow
        }
        
        # If not found by AutomationId, try by ControlType
        if (-not $comboBox) {
            Write-Host "    ComboBox not found by AutomationId, trying by ControlType..." -ForegroundColor Yellow
            try {
                $comboBoxType = [System.Windows.Automation.ControlType]::ComboBox
                $typeCondition = New-Object System.Windows.Automation.PropertyCondition(
                    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    $comboBoxType
                )
                $comboBox = $mainWindow.FindFirst(
                    [System.Windows.Automation.TreeScope]::Descendants,
                    $typeCondition
                )
            }
            catch {
                Write-Host "    Error finding ComboBox by ControlType: $_" -ForegroundColor Red
            }
        }
        
        if (-not $comboBox) {
            throw "Could not find ComboBox"
        }
        
        Write-Host "    Found ComboBox, clicking to open dropdown..." -ForegroundColor Gray
        
        # Get ExpandCollapse pattern to open the dropdown
        $expandCollapsePattern = $null
        try {
            $expandCollapsePattern = $comboBox.GetCurrentPattern(
                [System.Windows.Automation.ExpandCollapsePattern]::Pattern
            )
        }
        catch {
            Write-Host "    ExpandCollapsePattern not available: $_" -ForegroundColor Yellow
        }
        
        if ($expandCollapsePattern) {
            # Measure time to expand dropdown
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            
            $expandCollapsePattern.Expand()
            
            # Wait for dropdown to fully expand (check for list items)
            $dropdownOpened = $false
            $expandTimeout = 30
            $expandStart = Get-Date
            
            while (((Get-Date) - $expandStart).TotalSeconds -lt $expandTimeout) {
                try {
                    # Check if expanded
                    $state = $expandCollapsePattern.Current.ExpandCollapseState
                    if ($state -eq [System.Windows.Automation.ExpandCollapseState]::Expanded) {
                        # Look for list items to confirm dropdown is populated
                        $listItemType = [System.Windows.Automation.ControlType]::ListItem
                        $listCondition = New-Object System.Windows.Automation.PropertyCondition(
                            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                            $listItemType
                        )
                        
                        # Find list items in the combobox
                        $listItems = $comboBox.FindAll(
                            [System.Windows.Automation.TreeScope]::Descendants,
                            $listCondition
                        )
                        
                        if ($listItems -and $listItems.Count -gt 0) {
                            $dropdownOpened = $true
                            break
                        }
                    }
                }
                catch {}
                
                Start-Sleep -Milliseconds 50
            }
            
            $sw.Stop()
            $dropdownTime = $sw.ElapsedMilliseconds
            
            if ($dropdownOpened) {
                Write-Host "    Dropdown opened with items visible" -ForegroundColor Green
            } else {
                Write-Host "    Dropdown state check completed (may not have found items)" -ForegroundColor Yellow
            }
        }
        else {
            throw "Could not get ExpandCollapsePattern from ComboBox"
        }
        
        return $dropdownTime
    }
    catch {
        throw "Failed to measure dropdown time: $_"
    }
    finally {
        # Cleanup processes
        if ($appProcess -and -not $appProcess.HasExited) {
            try { 
                $null = $appProcess.Kill()
                $null = $appProcess.WaitForExit(2000)
            } catch {}
        }
        
        if ($process -and -not $process.HasExited) {
            try { 
                $null = $process.Kill()
                $null = $process.WaitForExit(2000)
            } catch {}
        }
    }
}

function Measure-StartupTime {
    param(
        [string]$WorkingDirectory
    )
    
    # Load UIAutomation assemblies (ignore if already loaded)
    try {
        [void][System.Reflection.Assembly]::Load('UIAutomationClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35')
        [void][System.Reflection.Assembly]::Load('UIAutomationTypes, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35')
    } catch {
        # Assemblies already loaded or not needed
    }
    
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    
    try {
        $process = Start-Process -FilePath "dotnet" `
                                 -ArgumentList "run" `
                                 -WorkingDirectory $WorkingDirectory `
                                 -PassThru
        
        if (-not $process) {
            throw "Failed to start process"
        }
        
        Write-Host "    Started dotnet process PID: $($process.Id)" -ForegroundColor Gray
        
        # Aguardar window com título que começa com "ERP System" aparecer
        $timeout = 30
        $found = $false
        $startTime = Get-Date
        $windowsChecked = 0
        $appProcess = $null
        
        while (((Get-Date) - $startTime).TotalSeconds -lt $timeout) {
            # Verificar se processo não crashou
            if ($process.HasExited) {
                throw "Process exited prematurely with code $($process.ExitCode)"
            }
            
            # Encontrar o processo real da aplicação (child process de dotnet run)
            if (-not $appProcess) {
                $appProcess = Get-Process -Name "StartupPerformance*" -ErrorAction SilentlyContinue | 
                    Select-Object -First 1
            }
            
            try {
                # Buscar todas as windows no desktop
                $rootElement = [System.Windows.Automation.AutomationElement]::RootElement
                $condition = [System.Windows.Automation.Condition]::TrueCondition
                $windows = $rootElement.FindAll(
                    [System.Windows.Automation.TreeScope]::Children,
                    $condition
                )
                
                foreach ($window in $windows) {
                    try {
                        # Verificar se a window pertence ao processo da aplicação
                        $windowPid = $window.GetCurrentPropertyValue(
                            [System.Windows.Automation.AutomationElement]::ProcessIdProperty
                        )
                        
                        # Comparar com o PID do processo da aplicação
                        if ($appProcess -and $windowPid -eq $appProcess.Id) {
                            # Obter o nome/título da window
                            $windowTitle = $window.GetCurrentPropertyValue(
                                [System.Windows.Automation.AutomationElement]::NameProperty
                            )
                            
                            # Obter class name
                            $className = $window.GetCurrentPropertyValue(
                                [System.Windows.Automation.AutomationElement]::ClassNameProperty
                            )
                            
                            if ($windowTitle -like "ERP System*") {
                                $found = $true
                                break
                            }
                        }
                    }
                    catch {
                        # Continuar para próxima window
                    }
                }
                
                if ($found) {
                    break
                }
                
                $windowsChecked++
            }
            catch {
                Write-Host "    Error during window enumeration: $_" -ForegroundColor Red
            }
            
            Start-Sleep -Milliseconds 400
        }
        
        $sw.Stop()
        
        if (-not $found) {
            Write-Warning "Window with title 'ERP System*' not found within timeout, using elapsed time"
        }
        
        # Limpar processos
        if ($appProcess -and -not $appProcess.HasExited) {
            try { 
                $null = $appProcess.Kill()
                $null = $appProcess.WaitForExit(2000)
            } catch {}
        }
        
        if (-not $process.HasExited) {
            try { 
                $null = $process.Kill()
                $null = $process.WaitForExit(2000)
            } catch {}
        }
        
        return $sw.ElapsedMilliseconds
    }
    catch {
        $sw.Stop()
        if ($process -and -not $process.HasExited) {
            try { $process.Kill() } catch {}
        }
        throw "Failed to measure startup time: $_"
    }
}

# All functions are automatically available when dot-sourced
