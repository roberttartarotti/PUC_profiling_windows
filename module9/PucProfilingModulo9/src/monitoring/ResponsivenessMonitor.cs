using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceMonitor;

/// <summary>
/// Mede a responsividade de uma aplicação monitorando delays na mensagem de UI.
/// Envia mensagens para a janela e mede o tempo de resposta.
/// </summary>
public class ResponsivenessMonitor : IDisposable
{
    private readonly int _targetProcessId;
    private readonly CancellationTokenSource _cts = new();
    private IntPtr _targetWindowHandle;
    private readonly List<double> _responseTimes = new();
    private readonly object _lock = new();
    
    // Estatísticas
    public double LastResponseTimeMs { get; private set; }
    public double AverageResponseTimeMs { get; private set; }
    public double MaxResponseTimeMs { get; private set; }
    public int UnresponsiveCount { get; private set; } // > 200ms
    public int FrozenCount { get; private set; } // > 1000ms
    public bool IsResponsive => LastResponseTimeMs < 200;
    
    // Threshold para considerar "não responsivo"
    public double UnresponsiveThresholdMs { get; set; } = 200;
    public double FrozenThresholdMs { get; set; } = 1000;
    
    // Eventos
    public event EventHandler<ResponsivenessEventArgs>? ResponsivenessChecked;
    public event EventHandler<string>? UnresponsiveDetected;
    
    // Windows API
    private const int WM_NULL = 0x0000;
    private const uint SMTO_ABORTIFHUNG = 0x0002;
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd, 
        uint Msg, 
        IntPtr wParam, 
        IntPtr lParam, 
        uint fuFlags, 
        uint uTimeout, 
        out IntPtr lpdwResult);
    
    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern bool IsHungAppWindow(IntPtr hWnd);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    public ResponsivenessMonitor(int targetProcessId)
    {
        _targetProcessId = targetProcessId;
        FindTargetWindow();
    }

    private void FindTargetWindow()
    {
        _targetWindowHandle = IntPtr.Zero;
        
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;
                
            GetWindowThreadProcessId(hWnd, out uint processId);
            if (processId == _targetProcessId)
            {
                _targetWindowHandle = hWnd;
                return false; // Stop enumeration
            }
            return true;
        }, IntPtr.Zero);
    }

    /// <summary>
    /// Mede o tempo de resposta da janela alvo.
    /// Retorna o tempo em milissegundos.
    /// </summary>
    public double MeasureResponseTime()
    {
        if (_targetWindowHandle == IntPtr.Zero || !IsWindow(_targetWindowHandle))
        {
            FindTargetWindow();
            if (_targetWindowHandle == IntPtr.Zero)
                return -1; // Janela não encontrada
        }
        
        // Verifica se está "hung" usando API nativa
        if (IsHungAppWindow(_targetWindowHandle))
        {
            lock (_lock)
            {
                FrozenCount++;
                LastResponseTimeMs = FrozenThresholdMs;
            }
            
            UnresponsiveDetected?.Invoke(this, $"🔴 Aplicação CONGELADA detectada!");
            return FrozenThresholdMs;
        }
        
        // Mede tempo de resposta enviando WM_NULL
        var sw = Stopwatch.StartNew();
        
        IntPtr result;
        var success = SendMessageTimeout(
            _targetWindowHandle,
            WM_NULL,
            IntPtr.Zero,
            IntPtr.Zero,
            SMTO_ABORTIFHUNG,
            (uint)FrozenThresholdMs, // Timeout em ms
            out result);
        
        sw.Stop();
        var responseTime = sw.Elapsed.TotalMilliseconds;
        
        lock (_lock)
        {
            LastResponseTimeMs = responseTime;
            _responseTimes.Add(responseTime);
            
            // Mantém últimos 60 valores
            if (_responseTimes.Count > 60)
                _responseTimes.RemoveAt(0);
            
            // Calcula estatísticas
            double sum = 0, max = 0;
            foreach (var time in _responseTimes)
            {
                sum += time;
                if (time > max) max = time;
            }
            AverageResponseTimeMs = sum / _responseTimes.Count;
            MaxResponseTimeMs = max;
            
            // Contadores de problemas
            if (responseTime >= FrozenThresholdMs)
            {
                FrozenCount++;
                UnresponsiveDetected?.Invoke(this, $"🔴 FROZEN: {responseTime:F0}ms");
            }
            else if (responseTime >= UnresponsiveThresholdMs)
            {
                UnresponsiveCount++;
                UnresponsiveDetected?.Invoke(this, $"🟡 Slow response: {responseTime:F0}ms");
            }
        }
        
        ResponsivenessChecked?.Invoke(this, new ResponsivenessEventArgs
        {
            ResponseTimeMs = responseTime,
            IsResponsive = responseTime < UnresponsiveThresholdMs,
            IsFrozen = responseTime >= FrozenThresholdMs
        });
        
        return responseTime;
    }

    /// <summary>
    /// Retorna uma classificação da responsividade.
    /// </summary>
    public string GetResponsivenessStatus()
    {
        if (LastResponseTimeMs < 0)
            return "❓ Não monitorado";
        if (LastResponseTimeMs < 16)
            return "🟢 Excelente (<16ms)";
        if (LastResponseTimeMs < 50)
            return "🟢 Bom (<50ms)";
        if (LastResponseTimeMs < 100)
            return "🟡 Aceitável (<100ms)";
        if (LastResponseTimeMs < UnresponsiveThresholdMs)
            return "🟠 Lento (<200ms)";
        if (LastResponseTimeMs < FrozenThresholdMs)
            return "🔴 Não responsivo";
        return "⛔ CONGELADO";
    }

    public void Reset()
    {
        lock (_lock)
        {
            _responseTimes.Clear();
            LastResponseTimeMs = 0;
            AverageResponseTimeMs = 0;
            MaxResponseTimeMs = 0;
            UnresponsiveCount = 0;
            FrozenCount = 0;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class ResponsivenessEventArgs : EventArgs
{
    public double ResponseTimeMs { get; set; }
    public bool IsResponsive { get; set; }
    public bool IsFrozen { get; set; }
}
