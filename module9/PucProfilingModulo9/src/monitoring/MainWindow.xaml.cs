using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace PerformanceMonitor;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ObservableValue> _cpuValues = new();
    private readonly ObservableCollection<ObservableValue> _memoryValues = new();
    private readonly ObservableCollection<ObservableValue> _uiThreadValues = new();
    private readonly ObservableCollection<ObservableValue> _contentionValues = new();
    private readonly ObservableCollection<AlertLogEntry> _alertLog = new();
    
    private readonly DispatcherTimer _monitorTimer;
    private readonly DispatcherTimer _processRefreshTimer;
    private readonly DispatcherTimer _elapsedTimer;
    
    private ProcessMonitor? _processMonitor;
    private ResponsivenessMonitor? _responsivenessMonitor;
    private Process? _targetProcess;
    private DateTime _monitoringStartTime;
    
    private readonly List<double> _cpuHistory = new();
    private readonly List<double> _memoryHistory = new();
    private readonly List<double> _uiThreadHistory = new();
    
    // Para tracking de GC e contention
    private int _lastGcGen2Count = 0;
    private int _gcGen2Delta = 0;
    private double _totalContentions = 0;
    
    private const int MaxDataPoints = 60; // 1 minuto de dados com intervalo de 1s

    public MainWindow()
    {
        InitializeComponent();
        
        // Setup charts
        SetupCharts();
        
        // Setup alert log
        AlertLogList.ItemsSource = _alertLog;
        
        // Setup timers
        _monitorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _monitorTimer.Tick += MonitorTimer_Tick;
        
        _processRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _processRefreshTimer.Tick += (s, e) => RefreshProcessList();
        _processRefreshTimer.Start();
        
        _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _elapsedTimer.Tick += ElapsedTimer_Tick;
        
        // Initial process list load
        RefreshProcessList();
        
        AddAlert("Sistema iniciado. Selecione um processo para monitorar.", "#2196F3");
    }

    private void SetupCharts()
    {
        // CPU Chart
        CpuChart.Series = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = _cpuValues,
                Fill = new SolidColorPaint(SKColors.DodgerBlue.WithAlpha(50)),
                Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.5
            }
        };
        
        CpuChart.YAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                TextSize = 12
            }
        };

        // Memory Chart
        MemoryChart.Series = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = _memoryValues,
                Fill = new SolidColorPaint(SKColors.LimeGreen.WithAlpha(50)),
                Stroke = new SolidColorPaint(SKColors.LimeGreen, 2),
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.5
            }
        };
        
        MemoryChart.YAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                TextSize = 12
            }
        };
        
        // UI Thread & Contention Chart
        UiThreadChart.Series = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Name = "UI Thread %",
                Values = _uiThreadValues,
                Fill = new SolidColorPaint(SKColors.Orange.WithAlpha(30)),
                Stroke = new SolidColorPaint(SKColors.Orange, 2),
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.5,
                ScalesYAt = 0
            },
            new LineSeries<ObservableValue>
            {
                Name = "Contention/sec",
                Values = _contentionValues,
                Fill = new SolidColorPaint(SKColors.Red.WithAlpha(30)),
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.5,
                ScalesYAt = 1
            }
        };
        
        UiThreadChart.YAxes = new Axis[]
        {
            new Axis
            {
                Name = "UI %",
                MinLimit = 0,
                MaxLimit = 100,
                LabelsPaint = new SolidColorPaint(SKColors.Orange),
                TextSize = 10,
                NamePaint = new SolidColorPaint(SKColors.Orange),
                NameTextSize = 10
            },
            new Axis
            {
                Name = "Cont.",
                MinLimit = 0,
                Position = LiveChartsCore.Measure.AxisPosition.End,
                LabelsPaint = new SolidColorPaint(SKColors.Red),
                TextSize = 10,
                NamePaint = new SolidColorPaint(SKColors.Red),
                NameTextSize = 10
            }
        };
    }

    private void RefreshProcessList()
    {
        var currentSelection = ProcessComboBox.Text;
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle) || 
                       p.ProcessName.Contains("StartupPerformance", StringComparison.OrdinalIgnoreCase) ||
                       p.ProcessName.Contains("Solved", StringComparison.OrdinalIgnoreCase) ||
                       p.ProcessName.Contains("Problem", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.ProcessName)
            .Select(p => new ProcessInfo(p.Id, p.ProcessName, p.MainWindowTitle))
            .ToList();
        
        // Adiciona processos específicos do curso no topo
        var priorityProcesses = processes
            .Where(p => p.Name.Contains("StartupPerformance", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        var otherProcesses = processes
            .Where(p => !p.Name.Contains("StartupPerformance", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        ProcessComboBox.ItemsSource = priorityProcesses.Concat(otherProcesses).ToList();
        ProcessComboBox.DisplayMemberPath = "DisplayName";
        
        // Restaura seleção se ainda existir
        if (!string.IsNullOrEmpty(currentSelection))
        {
            var match = ((List<ProcessInfo>)ProcessComboBox.ItemsSource)
                .FirstOrDefault(p => p.DisplayName == currentSelection);
            if (match != null)
            {
                ProcessComboBox.SelectedItem = match;
            }
        }
    }

    private void RefreshProcesses_Click(object sender, RoutedEventArgs e)
    {
        RefreshProcessList();
        AddAlert("Lista de processos atualizada.", "#2196F3");
    }

    private void ProcessComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var selected = ProcessComboBox.SelectedItem as ProcessInfo;
        if (selected != null)
        {
            StatusText.Text = $"✅ Processo selecionado: {selected.Name} (PID: {selected.Pid})";
            StartButton.IsEnabled = true;
        }
    }

    private void StartMonitoring_Click(object sender, RoutedEventArgs e)
    {
        var selected = ProcessComboBox.SelectedItem as ProcessInfo;
        if (selected == null)
        {
            MessageBox.Show("Selecione um processo para monitorar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _targetProcess = Process.GetProcessById(selected.Pid);
            _processMonitor = new ProcessMonitor(_targetProcess);
            _responsivenessMonitor = new ResponsivenessMonitor(_targetProcess.Id);
            
            // Hook responsiveness events
            _responsivenessMonitor.UnresponsiveDetected += (s, msg) =>
            {
                Dispatcher.Invoke(() => AddAlert(msg, "#F44336"));
            };
            
            // Limpa dados anteriores
            _cpuValues.Clear();
            _memoryValues.Clear();
            _uiThreadValues.Clear();
            _contentionValues.Clear();
            _cpuHistory.Clear();
            _memoryHistory.Clear();
            _uiThreadHistory.Clear();
            _lastGcGen2Count = 0;
            _gcGen2Delta = 0;
            _totalContentions = 0;
            
            // Atualiza UI
            PidText.Text = _targetProcess.Id.ToString();
            ProcessNameText.Text = _targetProcess.ProcessName;
            
            // Inicia monitoramento
            _monitoringStartTime = DateTime.Now;
            _monitorTimer.Start();
            _elapsedTimer.Start();
            
            // Atualiza botões
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ProcessComboBox.IsEnabled = false;
            
            StatusText.Text = $"🔍 Monitorando: {selected.Name}";
            AddAlert($"Monitoramento iniciado: {selected.Name} (PID: {selected.Pid})", "#4CAF50");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao iniciar monitoramento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            AddAlert($"Erro: {ex.Message}", "#F44336");
        }
    }

    private void StopMonitoring_Click(object sender, RoutedEventArgs e)
    {
        StopMonitoring();
    }

    private void StopMonitoring()
    {
        _monitorTimer.Stop();
        _elapsedTimer.Stop();
        _processMonitor?.Dispose();
        _processMonitor = null;
        _responsivenessMonitor?.Dispose();
        _responsivenessMonitor = null;
        
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        ProcessComboBox.IsEnabled = true;
        
        StatusText.Text = "⏸️ Monitoramento parado";
        AddAlert("Monitoramento parado.", "#FF9800");
        
        // Oculta alertas visuais
        CpuAlertBadge.Visibility = Visibility.Collapsed;
        MemoryAlertBadge.Visibility = Visibility.Collapsed;
        GcGen2AlertBadge.Visibility = Visibility.Collapsed;
        ContentionAlertBadge.Visibility = Visibility.Collapsed;
    }

    private void MonitorTimer_Tick(object? sender, EventArgs e)
    {
        if (_targetProcess == null || _processMonitor == null)
            return;

        try
        {
            _targetProcess.Refresh();
            
            if (_targetProcess.HasExited)
            {
                AddAlert("⚠️ Processo encerrado!", "#F44336");
                StopMonitoring();
                return;
            }

            // Coleta métricas
            var metrics = _processMonitor.GetMetrics();
            
            // Mede responsividade
            double responseTime = 0;
            if (_responsivenessMonitor != null)
            {
                responseTime = _responsivenessMonitor.MeasureResponseTime();
            }
            
            // Atualiza dados do gráfico
            _cpuValues.Add(new ObservableValue(metrics.CpuPercent));
            _memoryValues.Add(new ObservableValue(metrics.MemoryMB));
            _uiThreadValues.Add(new ObservableValue(metrics.UiThreadCpuPercent));
            _contentionValues.Add(new ObservableValue(metrics.ContentionRate));
            
            // Mantém limite de pontos
            if (_cpuValues.Count > MaxDataPoints)
            {
                _cpuValues.RemoveAt(0);
                _memoryValues.RemoveAt(0);
                _uiThreadValues.RemoveAt(0);
                _contentionValues.RemoveAt(0);
            }
            
            // Histórico para estatísticas
            _cpuHistory.Add(metrics.CpuPercent);
            _memoryHistory.Add(metrics.MemoryMB);
            _uiThreadHistory.Add(metrics.UiThreadCpuPercent);
            
            // Track GC Gen2 delta
            if (_lastGcGen2Count > 0)
            {
                _gcGen2Delta = metrics.GcGen2Collections - _lastGcGen2Count;
            }
            _lastGcGen2Count = metrics.GcGen2Collections;
            
            // Track total contentions
            _totalContentions += metrics.ContentionRate;
            
            // Atualiza UI
            UpdateMetricsDisplay(metrics, responseTime);
            CheckAlerts(metrics);
            UpdateStatistics(metrics);
        }
        catch (InvalidOperationException)
        {
            AddAlert("⚠️ Processo não está mais disponível!", "#F44336");
            StopMonitoring();
        }
        catch (Exception ex)
        {
            AddAlert($"Erro ao coletar métricas: {ex.Message}", "#F44336");
        }
    }

    private void UpdateMetricsDisplay(ProcessMetrics metrics, double responseTime)
    {
        // CPU
        CpuCurrentText.Text = $" {metrics.CpuPercent:F1}%";
        UiThreadCpuText.Text = $"{metrics.UiThreadCpuPercent:F1}%";
        MemoryCurrentText.Text = $" {metrics.MemoryMB:F1} MB";
        
        // Process info
        ThreadCountText.Text = metrics.ThreadCount.ToString();
        HandleCountText.Text = metrics.HandleCount.ToString();
        
        // GC
        GcGen0Text.Text = metrics.GcGen0Collections.ToString();
        GcGen1Text.Text = metrics.GcGen1Collections.ToString();
        GcGen2Text.Text = metrics.GcGen2Collections.ToString();
        GcHeapText.Text = $"{metrics.GcTotalMemoryMB:F1} MB";
        
        // Contention
        ContentionRateText.Text = $"{metrics.ContentionRate:F1}";
        ContentionTotalText.Text = $"{_totalContentions:F0}";
        ExceptionsText.Text = $"{metrics.ExceptionsPerSec:F1}/sec";
        
        // Responsiveness
        if (_responsivenessMonitor != null)
        {
            ResponsivenessStatusText.Text = _responsivenessMonitor.GetResponsivenessStatus();
            ResponseTimeText.Text = $"{responseTime:F1} ms";
            AvgResponseTimeText.Text = $"{_responsivenessMonitor.AverageResponseTimeMs:F1} ms";
            FreezeCountText.Text = _responsivenessMonitor.FrozenCount.ToString();
            
            // Color code response time
            if (responseTime < 50)
                ResponseTimeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            else if (responseTime < 200)
                ResponseTimeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
            else
                ResponseTimeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
        }
    }

    private void CheckAlerts(ProcessMetrics metrics)
    {
        // CPU Alert
        if (double.TryParse(CpuThresholdBox.Text, out var cpuThreshold) && metrics.CpuPercent > cpuThreshold)
        {
            CpuAlertBadge.Visibility = Visibility.Visible;
            AddAlert($"🔥 CPU alta: {metrics.CpuPercent:F1}% (limite: {cpuThreshold}%)", "#F44336");
            PlayAlertSound();
        }
        else
        {
            CpuAlertBadge.Visibility = Visibility.Collapsed;
        }
        
        // Memory Alert
        if (double.TryParse(MemoryThresholdBox.Text, out var memThreshold) && metrics.MemoryMB > memThreshold)
        {
            MemoryAlertBadge.Visibility = Visibility.Visible;
            AddAlert($"💾 Memória alta: {metrics.MemoryMB:F1} MB (limite: {memThreshold} MB)", "#FF9800");
            PlayAlertSound();
        }
        else
        {
            MemoryAlertBadge.Visibility = Visibility.Collapsed;
        }
        
        // GC Gen2 Alert
        if (double.TryParse(GcGen2ThresholdBox.Text, out var gcThreshold) && _gcGen2Delta > 0)
        {
            GcGen2AlertBadge.Visibility = Visibility.Visible;
            AddAlert($"🗑️ GC Gen2 detectado! Total: {metrics.GcGen2Collections}", "#F44336");
            PlayAlertSound();
        }
        else
        {
            GcGen2AlertBadge.Visibility = Visibility.Collapsed;
        }
        
        // Contention Alert
        if (double.TryParse(ContentionThresholdBox.Text, out var contentionThreshold) && metrics.ContentionRate > contentionThreshold)
        {
            ContentionAlertBadge.Visibility = Visibility.Visible;
            AddAlert($"🔒 Lock contention alto: {metrics.ContentionRate:F1}/sec (limite: {contentionThreshold})", "#FF9800");
            PlayAlertSound();
        }
        else
        {
            ContentionAlertBadge.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateStatistics(ProcessMetrics metrics)
    {
        if (_cpuHistory.Count > 0)
        {
            CpuAvgText.Text = $"{_cpuHistory.Average():F1}%";
            CpuMaxText.Text = $"{_cpuHistory.Max():F1}%";
        }
        
        if (_memoryHistory.Count > 0)
        {
            MemAvgText.Text = $"{_memoryHistory.Average():F1} MB";
            MemMaxText.Text = $"{_memoryHistory.Max():F1} MB";
        }
        
        GcGen2TotalText.Text = metrics.GcGen2Collections.ToString();
    }

    private void ElapsedTimer_Tick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.Now - _monitoringStartTime;
        ElapsedTimeText.Text = elapsed.ToString(@"hh\:mm\:ss");
    }

    private void PlayAlertSound()
    {
        if (SoundAlertCheckBox.IsChecked == true)
        {
            try
            {
                SystemSounds.Exclamation.Play();
            }
            catch { }
        }
    }

    private void AddAlert(string message, string colorHex)
    {
        var entry = new AlertLogEntry
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Message = message,
            Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
        };
        
        _alertLog.Insert(0, entry);
        
        // Limita o log a 100 entradas
        while (_alertLog.Count > 100)
        {
            _alertLog.RemoveAt(_alertLog.Count - 1);
        }
    }

    private void ClearAlerts_Click(object sender, RoutedEventArgs e)
    {
        _alertLog.Clear();
        AddAlert("Log de alertas limpo.", "#888888");
    }

    protected override void OnClosed(EventArgs e)
    {
        _monitorTimer.Stop();
        _processRefreshTimer.Stop();
        _elapsedTimer.Stop();
        _processMonitor?.Dispose();
        _responsivenessMonitor?.Dispose();
        base.OnClosed(e);
    }
}

// Classes auxiliares
public record ProcessInfo(int Pid, string Name, string WindowTitle)
{
    public string DisplayName => string.IsNullOrEmpty(WindowTitle) 
        ? $"{Name} (PID: {Pid})" 
        : $"{Name} - {WindowTitle} (PID: {Pid})";
}

public class AlertLogEntry
{
    public string Time { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Brush Color { get; set; } = Brushes.White;
}
