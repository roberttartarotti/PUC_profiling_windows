using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PerformanceMonitor;

/// <summary>
/// Monitora métricas de performance de um processo específico.
/// Utiliza Performance Counters do Windows para coletar dados em tempo real.
/// </summary>
public class ProcessMonitor : IDisposable
{
    private readonly Process _process;
    private readonly PerformanceCounter? _cpuCounter;
    private readonly PerformanceCounter? _memoryCounter;
    private readonly PerformanceCounter? _gcGen0Counter;
    private readonly PerformanceCounter? _gcGen1Counter;
    private readonly PerformanceCounter? _gcGen2Counter;
    private readonly PerformanceCounter? _contentionRateCounter;
    private readonly PerformanceCounter? _exceptionsCounter;
    private readonly int _processorCount;
    
    private DateTime _lastCpuTime;
    private TimeSpan _lastTotalProcessorTime;
    private bool _firstMeasurement = true;
    
    // Para cálculo de UI Thread usage
    private readonly Dictionary<int, ThreadCpuInfo> _threadCpuInfo = new();
    private int? _uiThreadId;

    public ProcessMonitor(Process process)
    {
        _process = process;
        _processorCount = Environment.ProcessorCount;
        
        try
        {
            // Usa contadores de performance se disponíveis
            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", _process.ProcessName, true);
            _memoryCounter = new PerformanceCounter("Process", "Working Set - Private", _process.ProcessName, true);
            
            // Contadores .NET CLR
            _gcGen0Counter = new PerformanceCounter(".NET CLR Memory", "# Gen 0 Collections", _process.ProcessName, true);
            _gcGen1Counter = new PerformanceCounter(".NET CLR Memory", "# Gen 1 Collections", _process.ProcessName, true);
            _gcGen2Counter = new PerformanceCounter(".NET CLR Memory", "# Gen 2 Collections", _process.ProcessName, true);
            
            // Lock contention
            _contentionRateCounter = new PerformanceCounter(".NET CLR LocksAndThreads", "Contention Rate / sec", _process.ProcessName, true);
            
            // Exceptions
            _exceptionsCounter = new PerformanceCounter(".NET CLR Exceptions", "# of Exceps Thrown / sec", _process.ProcessName, true);
            
            // Primeira leitura para inicializar
            _cpuCounter.NextValue();
            _gcGen0Counter.NextValue();
            _gcGen1Counter.NextValue();
            _gcGen2Counter.NextValue();
            _contentionRateCounter.NextValue();
            _exceptionsCounter.NextValue();
        }
        catch
        {
            // Se contadores não estão disponíveis, usa método alternativo
            _cpuCounter = null;
            _memoryCounter = null;
        }
        
        // Inicializa para cálculo manual de CPU
        _lastCpuTime = DateTime.UtcNow;
        try
        {
            _lastTotalProcessorTime = _process.TotalProcessorTime;
            
            // Tenta identificar UI thread (geralmente a primeira thread)
            if (_process.Threads.Count > 0)
            {
                _uiThreadId = _process.Threads[0].Id;
            }
        }
        catch
        {
            _lastTotalProcessorTime = TimeSpan.Zero;
        }
    }

    public ProcessMetrics GetMetrics()
    {
        _process.Refresh();
        
        var metrics = new ProcessMetrics();
        
        // CPU
        metrics.CpuPercent = GetCpuUsage();
        
        // UI Thread CPU
        metrics.UiThreadCpuPercent = GetUiThreadCpuUsage();
        
        // Memória
        metrics.MemoryMB = _process.WorkingSet64 / (1024.0 * 1024.0);
        metrics.PrivateMemoryMB = _process.PrivateMemorySize64 / (1024.0 * 1024.0);
        metrics.VirtualMemoryMB = _process.VirtualMemorySize64 / (1024.0 * 1024.0);
        
        // Outras métricas
        metrics.ThreadCount = _process.Threads.Count;
        metrics.HandleCount = _process.HandleCount;
        
        // GC Collections via Performance Counters
        try
        {
            if (_gcGen0Counter != null)
                metrics.GcGen0Collections = (int)_gcGen0Counter.NextValue();
            if (_gcGen1Counter != null)
                metrics.GcGen1Collections = (int)_gcGen1Counter.NextValue();
            if (_gcGen2Counter != null)
                metrics.GcGen2Collections = (int)_gcGen2Counter.NextValue();
        }
        catch { }
        
        // Lock Contention
        try
        {
            if (_contentionRateCounter != null)
                metrics.ContentionRate = _contentionRateCounter.NextValue();
        }
        catch { }
        
        // Exceptions
        try
        {
            if (_exceptionsCounter != null)
                metrics.ExceptionsPerSec = _exceptionsCounter.NextValue();
        }
        catch { }
        
        // GC info (se disponível para processos .NET)
        try
        {
            metrics.GcTotalMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        }
        catch
        {
            metrics.GcTotalMemoryMB = 0;
        }
        
        return metrics;
    }

    private double GetCpuUsage()
    {
        try
        {
            // Tenta usar Performance Counter primeiro
            if (_cpuCounter != null)
            {
                var value = _cpuCounter.NextValue();
                return value / _processorCount; // Normaliza para porcentagem total
            }
        }
        catch
        {
            // Fall through para método manual
        }
        
        // Método manual de cálculo de CPU
        try
        {
            var currentTime = DateTime.UtcNow;
            var currentTotalProcessorTime = _process.TotalProcessorTime;
            
            if (_firstMeasurement)
            {
                _firstMeasurement = false;
                _lastCpuTime = currentTime;
                _lastTotalProcessorTime = currentTotalProcessorTime;
                return 0;
            }
            
            var cpuUsedMs = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds;
            var totalMsPassed = (currentTime - _lastCpuTime).TotalMilliseconds;
            
            _lastCpuTime = currentTime;
            _lastTotalProcessorTime = currentTotalProcessorTime;
            
            if (totalMsPassed > 0)
            {
                var cpuUsageTotal = (cpuUsedMs / (totalMsPassed * _processorCount)) * 100;
                return Math.Min(100, Math.Max(0, cpuUsageTotal));
            }
        }
        catch
        {
            // Retorna 0 se não conseguir calcular
        }
        
        return 0;
    }

    private double GetUiThreadCpuUsage()
    {
        try
        {
            if (_uiThreadId == null || _process.Threads.Count == 0)
                return 0;
            
            // Encontra a UI thread
            ProcessThread? uiThread = null;
            foreach (ProcessThread thread in _process.Threads)
            {
                if (thread.Id == _uiThreadId)
                {
                    uiThread = thread;
                    break;
                }
            }
            
            if (uiThread == null)
            {
                // UI thread pode ter mudado, tenta a primeira
                uiThread = _process.Threads[0];
                _uiThreadId = uiThread.Id;
            }
            
            var currentTime = DateTime.UtcNow;
            var currentTotalTime = uiThread.TotalProcessorTime;
            
            if (!_threadCpuInfo.TryGetValue(uiThread.Id, out var info))
            {
                info = new ThreadCpuInfo { LastTime = currentTime, LastProcessorTime = currentTotalTime };
                _threadCpuInfo[uiThread.Id] = info;
                return 0;
            }
            
            var cpuUsedMs = (currentTotalTime - info.LastProcessorTime).TotalMilliseconds;
            var totalMsPassed = (currentTime - info.LastTime).TotalMilliseconds;
            
            info.LastTime = currentTime;
            info.LastProcessorTime = currentTotalTime;
            
            if (totalMsPassed > 0)
            {
                // UI thread usage como % do tempo total de uma CPU
                var usage = (cpuUsedMs / totalMsPassed) * 100;
                return Math.Min(100, Math.Max(0, usage));
            }
        }
        catch
        {
            // Erro ao acessar thread info
        }
        
        return 0;
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
        _gcGen0Counter?.Dispose();
        _gcGen1Counter?.Dispose();
        _gcGen2Counter?.Dispose();
        _contentionRateCounter?.Dispose();
        _exceptionsCounter?.Dispose();
    }
    
    private class ThreadCpuInfo
    {
        public DateTime LastTime { get; set; }
        public TimeSpan LastProcessorTime { get; set; }
    }
}

/// <summary>
/// Contém as métricas coletadas de um processo.
/// </summary>
public class ProcessMetrics
{
    // CPU
    public double CpuPercent { get; set; }
    public double UiThreadCpuPercent { get; set; }
    
    // Memory
    public double MemoryMB { get; set; }
    public double PrivateMemoryMB { get; set; }
    public double VirtualMemoryMB { get; set; }
    
    // Process Info
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    
    // GC Metrics
    public double GcTotalMemoryMB { get; set; }
    public int GcGen0Collections { get; set; }
    public int GcGen1Collections { get; set; }
    public int GcGen2Collections { get; set; }
    
    // Lock Contention
    public double ContentionRate { get; set; }
    
    // Exceptions
    public double ExceptionsPerSec { get; set; }
}
