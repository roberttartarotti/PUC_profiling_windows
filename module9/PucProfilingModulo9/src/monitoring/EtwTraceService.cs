using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;

namespace PerformanceMonitor;

/// <summary>
/// Serviço de captura de eventos ETW (Event Tracing for Windows).
/// Permite capturar eventos do CLR como GC, JIT, Exceptions, etc.
/// REQUER EXECUÇÃO COMO ADMINISTRADOR para sessões de kernel.
/// </summary>
public class EtwTraceService : IDisposable
{
    private TraceEventSession? _session;
    private ETWTraceEventSource? _source;
    private Thread? _processingThread;
    private CancellationTokenSource? _cts;
    
    private readonly int _targetProcessId;
    private readonly string _sessionName;
    
    // Eventos
    public event EventHandler<GcEventArgs>? GcCollectionStarted;
    public event EventHandler<GcEventArgs>? GcCollectionCompleted;
    public event EventHandler<ExceptionEventArgs>? ExceptionThrown;
    public event EventHandler<JitEventArgs>? MethodJitted;
    public event EventHandler<ContentionEventArgs>? ContentionDetected;
    public event EventHandler<ThreadPoolEventArgs>? ThreadPoolEvent;
    public event EventHandler<string>? TraceMessage;

    public bool IsRunning { get; private set; }

    public EtwTraceService(int targetProcessId)
    {
        _targetProcessId = targetProcessId;
        _sessionName = $"PerformanceMonitor_{Guid.NewGuid():N}";
    }

    public async Task<bool> StartAsync()
    {
        if (IsRunning)
            return true;

        try
        {
            // Verifica se está executando como admin
            if (!IsAdministrator())
            {
                TraceMessage?.Invoke(this, "⚠️ ETW Trace requer execução como Administrador");
                return false;
            }

            _cts = new CancellationTokenSource();
            
            // Cria sessão ETW
            _session = new TraceEventSession(_sessionName);
            
            // Habilita provider do CLR para eventos .NET
            _session.EnableProvider(
                ClrTraceEventParser.ProviderGuid,
                TraceEventLevel.Informational,
                (ulong)(
                    ClrTraceEventParser.Keywords.GC |
                    ClrTraceEventParser.Keywords.Exception |
                    ClrTraceEventParser.Keywords.Jit |
                    ClrTraceEventParser.Keywords.Contention |
                    ClrTraceEventParser.Keywords.Threading
                )
            );

            TraceMessage?.Invoke(this, "✓ Sessão ETW criada com sucesso");

            // Processa eventos em thread separada
            _processingThread = new Thread(ProcessEvents)
            {
                IsBackground = true,
                Name = "ETW Processing Thread"
            };
            _processingThread.Start();

            IsRunning = true;
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            TraceMessage?.Invoke(this, "❌ Acesso negado. Execute como Administrador.");
            return false;
        }
        catch (Exception ex)
        {
            TraceMessage?.Invoke(this, $"❌ Erro ao iniciar ETW: {ex.Message}");
            return false;
        }
    }

    private void ProcessEvents()
    {
        try
        {
            _source = _session?.Source;
            if (_source == null) return;

            var clrParser = new ClrTraceEventParser(_source);

            // GC Events
            clrParser.GCStart += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    GcCollectionStarted?.Invoke(this, new GcEventArgs
                    {
                        Generation = data.Depth,
                        Reason = data.Reason.ToString(),
                        Type = data.Type.ToString(),
                        Timestamp = data.TimeStamp
                    });
                }
            };

            clrParser.GCStop += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    GcCollectionCompleted?.Invoke(this, new GcEventArgs
                    {
                        Generation = data.Depth,
                        Timestamp = data.TimeStamp
                    });
                }
            };

            // Exception Events
            clrParser.ExceptionStart += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    ExceptionThrown?.Invoke(this, new ExceptionEventArgs
                    {
                        ExceptionType = data.ExceptionType,
                        Message = data.ExceptionMessage,
                        Timestamp = data.TimeStamp
                    });
                }
            };

            // JIT Events
            clrParser.MethodJittingStarted += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    MethodJitted?.Invoke(this, new JitEventArgs
                    {
                        MethodName = data.MethodName,
                        MethodNamespace = data.MethodNamespace,
                        Timestamp = data.TimeStamp
                    });
                }
            };

            // Contention Events (locks, deadlocks)
            clrParser.ContentionStart += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    ContentionDetected?.Invoke(this, new ContentionEventArgs
                    {
                        ContentionFlags = data.ContentionFlags.ToString(),
                        Timestamp = data.TimeStamp
                    });
                }
            };

            // Thread Pool Events
            clrParser.ThreadPoolWorkerThreadStart += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    ThreadPoolEvent?.Invoke(this, new ThreadPoolEventArgs
                    {
                        EventType = "WorkerThreadStart",
                        ActiveWorkerThreadCount = data.ActiveWorkerThreadCount,
                        Timestamp = data.TimeStamp
                    });
                }
            };

            clrParser.ThreadPoolWorkerThreadStop += (data) =>
            {
                if (data.ProcessID == _targetProcessId)
                {
                    ThreadPoolEvent?.Invoke(this, new ThreadPoolEventArgs
                    {
                        EventType = "WorkerThreadStop",
                        ActiveWorkerThreadCount = data.ActiveWorkerThreadCount,
                        Timestamp = data.TimeStamp
                    });
                }
            };

            TraceMessage?.Invoke(this, "🔍 Processando eventos ETW...");
            
            // Processa eventos (bloqueante)
            _source.Process();
        }
        catch (OperationCanceledException)
        {
            // Normal durante shutdown
        }
        catch (Exception ex)
        {
            TraceMessage?.Invoke(this, $"❌ Erro no processamento ETW: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        try
        {
            _cts?.Cancel();
            _session?.Stop();
            _source?.StopProcessing();
            _processingThread?.Join(TimeSpan.FromSeconds(2));
            
            TraceMessage?.Invoke(this, "⏹️ Sessão ETW encerrada");
        }
        catch (Exception ex)
        {
            TraceMessage?.Invoke(this, $"Erro ao parar ETW: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    private static bool IsAdministrator()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Stop();
        _session?.Dispose();
        _cts?.Dispose();
    }
}

// Event Args classes
public class GcEventArgs : EventArgs
{
    public int Generation { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ExceptionEventArgs : EventArgs
{
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class JitEventArgs : EventArgs
{
    public string MethodName { get; set; } = string.Empty;
    public string MethodNamespace { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ContentionEventArgs : EventArgs
{
    public string ContentionFlags { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ThreadPoolEventArgs : EventArgs
{
    public string EventType { get; set; } = string.Empty;
    public int ActiveWorkerThreadCount { get; set; }
    public DateTime Timestamp { get; set; }
}
