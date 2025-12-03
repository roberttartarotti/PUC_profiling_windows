using PokeProfiler.Core;
using PokeProfiler.Core.Instrumentation;
using PokeProfiler.Core.Persistence;
using PokeProfiler.Core.Strategies;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;

namespace PokeProfiler.UI;

public partial class MainWindow : Window
{
    private readonly PokeApiClient _client = new();

    private readonly PokemonRepository _repo;
    private readonly (string name, IPokemonFetchStrategy strat)[] _strategies;
    private CancellationTokenSource? _cts;

    // Estruturas para demonstração de escalabilidade    
    private readonly ConcurrentQueue<string> _lockFreeQueue = new();
    private readonly object _lockObject = new();
    private long _contentionCounter = 0;

    public MainWindow()
    {
        InitializeComponent();
        var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "pokeprofiler.sqlite");
        _repo = new PokemonRepository(dbPath);
        _strategies =
        [
            ("Optimized Scalable", new OptimizedScalableStrategy(_client)),
            ("Sequential", new SequentialStrategy(_client)),
            ("Task.WhenAll", new TaskWhenAllStrategy(_client)),
            ("ThreadPool Storm", new ThreadPoolStormStrategy(_client)),
            ("Lock Contention", new LockContentionStrategy(_client)),
            ("Semaphore Batch", new SemaphoreBatchStrategy(_client, 10)),
            ("⚠ Memory Leak", new MemoryLeakStrategy(_client)),
            ("⚠ Excessive Alloc", new ExcessiveAllocStrategy(_client)),
            ("⚠ CPU Spin", new CPUSpinStrategy(_client)),
            ("⚠ Deadlock Risk", new DeadlockRiskStrategy(_client)),
            ("⚠ Inefficient Algorithm", new IneffAlgorithmStrategy(_client)),
            ("🔧 Oversubscription Demo", new OversubscriptionStrategy(_client)),
            ("🔧 Load Balance Demo", new LoadBalancedStrategy(_client)),
            ("🔧 Lock-Free Demo", new LockFreeStrategy(_client)),
            ("🔧 SpinLock Contention", new SpinLockContentionStrategy(_client)),
            ("🔧 Task Parallelism", new TaskParallelismStrategy(_client)),
            ("🔧 Thread Local Storage", new ThreadLocalStrategy(_client)),
            ("🔧 Concurrent Bag", new ConcurrentBagStrategy(_client)),
            ("🔧 Batch Processing", new BatchProcessingStrategy(_client, 10)),
            
        ];
        StrategyBox.ItemsSource = _strategies.Select(s => s.name);
        StrategyBox.SelectedIndex = 1;
        FetchBtn.Click += FetchBtn_Click;
        ApplyThreadPoolBtn.Click += ApplyThreadPoolBtn_Click;
        Closed += (_, __) => _cts?.Cancel();
        UpdateThreadPoolStatus();
    }

    private void UpdateThreadPoolStatus()
    {
        // Ensure we're on the UI thread
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(UpdateThreadPoolStatus);
            return;
        }

        try
        {
            ThreadPool.GetMinThreads(out int minWorker, out int minIO);
            ThreadPool.GetMaxThreads(out int maxWorker, out int maxIO);
            ThreadPool.GetAvailableThreads(out int availWorker, out int availIO);

            ThreadPoolStatus.Text = $"Current: MinWorker={minWorker}, MinIO={minIO}, MaxWorker={maxWorker}, MaxIO={maxIO}\n" +
                                   $"Available: Worker={availWorker}, IO={availIO}\n" +
                                   $"Logical Processors: {Environment.ProcessorCount}";
        }
        catch (Exception ex)
        {
            ThreadPoolStatus.Text = $"Error updating status: {ex.Message}";
        }
    }

    private static string[] ParsePokemonIds(string input)
    {
        var result = new List<string>();
        var parts = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (part.Contains('-'))
            {
                // Range: 1-600
                var range = part.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                {
                    for (int i = start; i <= end; i++)
                        result.Add(i.ToString());
                }
            }
            else
            {
                // Single ID
                result.Add(part);
            }
        }

        return [.. result];
    }

    private void ApplyThreadPoolBtn_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(MinThreadsBox.Text, out int minThreads) && minThreads > 0)
        {
            ThreadPool.SetMinThreads(minThreads, minThreads);
            UpdateThreadPoolStatus();
        }
        else
        {
            MessageBox.Show("Invalid MinThreads value", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void FetchBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            
            // Ensure status update is on UI thread
            StatusText.Text = "Fetching...";
            ResultsView.ItemsSource = null;

            // Apply artificial delay
            if (int.TryParse(ArtificialDelayBox.Text, out int delay))
                _client.ArtificialDelayMs = delay;

            var ids = ParsePokemonIds(IdsBox.Text);
            var strat = _strategies[StrategyBox.SelectedIndex].strat;

            Activity? activity = null;
            if (EnableTracingCheck.IsChecked == true)
            {
                activity = Telemetry.ActivitySource.StartActivity($"Fetch-{strat.Name}");
                activity?.SetTag("pokemon.count", ids.Length);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Monitorar ThreadPool antes da execução
            ThreadPool.GetAvailableThreads(out int availBefore, out _);

            var list = await strat.FetchAsync(ids, ct);

            sw.Stop();

            // Monitorar ThreadPool depois da execução
            ThreadPool.GetAvailableThreads(out int availAfter, out _);

            activity?.Stop();

            foreach (var p in list)
                _repo.Upsert(p);

            ResultsView.ItemsSource = list;

            var metrics = $"{strat.Name}: {list.Count} items in {sw.ElapsedMilliseconds} ms\n" +
                         $"ThreadPool: Used {availBefore - availAfter} threads (Avail: {availBefore}→{availAfter})\n" +
                         $"Contention Events: {Interlocked.Read(ref _contentionCounter)}";

            // Ensure status update is on UI thread
            StatusText.Text = metrics;

            // Reset contador de contenção
            Interlocked.Exchange(ref _contentionCounter, 0);

            // Atualizar status do ThreadPool
            UpdateThreadPoolStatus();
        }
        catch (Exception ex)
        {
            // Ensure error message is displayed on UI thread
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    // Demonstração de Lock-Free Queue
    private void DemonstrateSpinLock()
    {
        var spinLock = new SpinLock(enableThreadOwnerTracking: false);

        Parallel.For(0, 100, i =>
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);
                Interlocked.Increment(ref _contentionCounter);
                // Simular trabalho
                Thread.SpinWait(100);
            }
            finally
            {
                if (lockTaken) spinLock.Exit();
            }
        });
    }

    // Demonstração de Thread Affinity
    private static void DemonstrateThreadAffinity()
    {
        var currentThread = Thread.CurrentThread;
        var processorAffinity = Process.GetCurrentProcess().ProcessorAffinity;

        Debug.WriteLine($"Thread {currentThread.ManagedThreadId} - Processor Affinity: {processorAffinity}");
        Debug.WriteLine($"Current Processor: {Thread.GetCurrentProcessorId()}");
    }
}
