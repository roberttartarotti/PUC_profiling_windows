using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StartupPerformance.Solved2;

// SOLUÇÃO 2: Background Preloading Service
// Inicia carregamento em background no startup da aplicação
// Permite interceptar se usuário solicitar antes de terminar
public class PreloadService
{
    private Task<List<Cliente>>? _preloadTask;
    private CancellationTokenSource? _cts;
    private readonly DataService _dataService;
    private List<Cliente>? _cachedClientes;

    public event EventHandler<PreloadProgressEventArgs>? ProgressChanged;
    public event EventHandler<bool>? PreloadCompleted; // bool = success

    public bool IsPreloading => _preloadTask != null && !_preloadTask.IsCompleted;
    public bool IsCompleted => _cachedClientes != null;

    public PreloadService(string csvFileName = "clientes-prod.csv")
    {
        _dataService = new DataService(csvFileName);
    }

    // Inicia o preload em background
    public void StartPreload()
    {
        if (_preloadTask != null)
            return; // Já está carregando

        _cts = new CancellationTokenSource();
        
        var progress = new Progress<(int current, int total, string message)>(report =>
        {
            ProgressChanged?.Invoke(this, new PreloadProgressEventArgs
            {
                Current = report.current,
                Total = report.total,
                Message = report.message
            });
        });

        _preloadTask = Task.Run(async () =>
        {
            try
            {
                var clientes = await _dataService.CarregarClientesAsync(progress, _cts.Token);
                _cachedClientes = clientes;
                PreloadCompleted?.Invoke(this, true);
                return clientes;
            }
            catch (OperationCanceledException)
            {
                PreloadCompleted?.Invoke(this, false);
                throw;
            }
        });
    }

    // Obtém os clientes - espera se ainda estiver carregando
    public async Task<List<Cliente>> GetClientesAsync(
        IProgress<(int current, int total, string message)>? progress = null)
    {
        // Se já está em cache, retorna imediatamente
        if (_cachedClientes != null)
        {
            progress?.Report((1, 1, "Dados já carregados!"));
            return _cachedClientes;
        }

        // Se está preloading, aguarda completar
        if (_preloadTask != null)
        {
            // Registra um handler temporário para redirecionar o progresso
            EventHandler<PreloadProgressEventArgs>? progressHandler = null;
            if (progress != null)
            {
                progressHandler = (sender, e) =>
                {
                    progress.Report((e.Current, e.Total, e.Message));
                };
                ProgressChanged += progressHandler;
            }

            try
            {
                return await _preloadTask;
            }
            catch (OperationCanceledException)
            {
                // Se foi cancelado, inicia um novo carregamento
                return await LoadFreshAsync(progress);
            }
            finally
            {
                // Remove o handler temporário
                if (progressHandler != null)
                {
                    ProgressChanged -= progressHandler;
                }
            }
        }

        // Se não tem nada, carrega agora
        return await LoadFreshAsync(progress);
    }

    private async Task<List<Cliente>> LoadFreshAsync(
        IProgress<(int current, int total, string message)>? progress)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        _cachedClientes = await _dataService.CarregarClientesAsync(progress, _cts.Token);
        return _cachedClientes;
    }

    public void CancelPreload()
    {
        _cts?.Cancel();
    }
}

public class PreloadProgressEventArgs : EventArgs
{
    public int Current { get; set; }
    public int Total { get; set; }
    public string Message { get; set; } = string.Empty;
}
