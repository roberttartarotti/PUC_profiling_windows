using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Aula3.Utilities;

/// <summary>
/// Sistema de marcadores para profiling que funciona com:
/// - Visual Studio Performance Profiler
/// - Intel VTune Profiler  
/// - Concurrency Visualizer
/// - PerfView
/// </summary>
public static class ProfilingMarkers
{
    private static readonly Aula3EventSource EventSource = new();
    
    /// <summary>
    /// Marca o início de um cenário específico
    /// </summary>
    public static void BeginScenario(string scenarioName, string description = "")
    {
        // Para Intel VTune e PerfView - ETW Events
        EventSource.ScenarioStart(scenarioName, description);
        
        // Para Concurrency Visualizer - Trace com formato específico
        Trace.WriteLine($"BEGIN_{scenarioName}", "PROFILER");
        
        // Para Visual Studio Performance Profiler - chamada de método visível
        NativeProfilerMarkers.MarkBegin(scenarioName);
        
        // Console para debug
        Console.WriteLine($"[INICIO] {scenarioName}");
        if (!string.IsNullOrEmpty(description))
            Console.WriteLine($"  {description}");
    }
    
    /// <summary>
    /// Marca o fim de um cenário específico
    /// </summary>
    public static void EndScenario(string scenarioName, string summary = "")
    {
        // Para Intel VTune e PerfView - ETW Events
        EventSource.ScenarioEnd(scenarioName, summary);
        
        // Para Concurrency Visualizer - Trace com formato específico
        Trace.WriteLine($"END_{scenarioName}", "PROFILER");
        
        // Para Visual Studio Performance Profiler - chamada de método visível
        NativeProfilerMarkers.MarkEnd(scenarioName);
        
        // Console para debug
        Console.WriteLine($"[FIM] {scenarioName}");
        if (!string.IsNullOrEmpty(summary))
            Console.WriteLine($"  {summary}");
    }
    
    /// <summary>
    /// Força flush de todos os providers
    /// </summary>
    public static void FlushAll()
    {
        try
        {
            EventSource?.Dispose();
            Trace.Flush();
            Console.Out.Flush();
        }
        catch
        {
            // Ignora erros durante flush
        }
    }
    
    /// <summary>
    /// Wrapper para marcar região de código automaticamente
    /// </summary>
    public static IDisposable CreateScenarioRegion(string scenarioName, string description = "")
    {
        return new ScenarioRegion(scenarioName, description);
    }
}

/// <summary>
/// Região de código automaticamente marcada
/// </summary>
internal sealed class ScenarioRegion : IDisposable
{
    private readonly string _scenarioName;
    private readonly Stopwatch _stopwatch;
    
    public ScenarioRegion(string scenarioName, string description)
    {
        _scenarioName = scenarioName;
        _stopwatch = Stopwatch.StartNew();
        ProfilingMarkers.BeginScenario(scenarioName, description);
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        var duration = _stopwatch.ElapsedMilliseconds;
        ProfilingMarkers.EndScenario(_scenarioName, $"Duração: {duration}ms");
    }
}

/// <summary>
/// EventSource para Intel VTune e PerfView
/// Usa ETW (Event Tracing for Windows) que é suportado por ambos
/// </summary>
[EventSource(Name = "Aula3-Profiling", Guid = "12345678-1234-5678-9012-123456789012")]
public sealed class Aula3EventSource : EventSource
{
    public static readonly Aula3EventSource Log = new();
    
    /// <summary>
    /// Evento de início de cenário - visível no Intel VTune e PerfView
    /// </summary>
    [Event(1, Level = EventLevel.Informational, 
     Message = "Scenario '{0}' started: {1}",
     Keywords = Keywords.Scenarios)]
    public void ScenarioStart(string scenarioName, string description)
    {
        WriteEvent(1, scenarioName, description ?? "");
    }
    
    /// <summary>
    /// Evento de fim de cenário - visível no Intel VTune e PerfView
    /// </summary>
    [Event(2, Level = EventLevel.Informational, 
     Message = "Scenario '{0}' completed: {1}",
     Keywords = Keywords.Scenarios)]
    public void ScenarioEnd(string scenarioName, string summary)
    {
        WriteEvent(2, scenarioName, summary ?? "");
    }
    
    /// <summary>
    /// Keywords para categorização no Intel VTune
    /// </summary>
    public static class Keywords
    {
        public const EventKeywords Scenarios = (EventKeywords)0x1;
    }
}

/// <summary>
/// Marcadores nativos que garantem visibilidade no Visual Studio Performance Profiler
/// Usa métodos com [MethodImpl(MethodImplOptions.NoInlining)] para aparecer no call stack
/// </summary>
public static class NativeProfilerMarkers
{
    /// <summary>
    /// Marca início - aparece no call stack do VS Performance Profiler
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void MarkBegin(string scenarioName)
    {
        // Operação simples que força o método a aparecer no profiler
        var hash = scenarioName.GetHashCode();
        Thread.SpinWait(1); // Garante que o método seja capturado
    }
    
    /// <summary>
    /// Marca fim - aparece no call stack do VS Performance Profiler
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void MarkEnd(string scenarioName)
    {
        // Operação simples que força o método a aparecer no profiler
        var hash = scenarioName.GetHashCode();
        Thread.SpinWait(1); // Garante que o método seja capturado
    }
}