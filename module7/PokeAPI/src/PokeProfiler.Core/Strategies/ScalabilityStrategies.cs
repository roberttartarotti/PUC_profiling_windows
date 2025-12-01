using System.Collections.Concurrent;

namespace PokeProfiler.Core.Strategies;

// ============================================================================
// ESTRATÉGIAS PARA DEMONSTRAÇÃO DE ESCALABILIDADE EM MULTITHREADING
// ============================================================================

/// <summary>
/// PROBLEMA: Sobresubscrição - Cria threads excessivas além do número de núcleos
/// Demonstra: Overhead de context switching e contenção no ThreadPool
/// </summary>
public class OversubscriptionStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;

    public string Name => "Oversubscription Demo";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();

        // PROBLEMA: Cria muito mais threads que núcleos disponíveis
        // Isso causa overhead de context switching
        int excessiveThreadCount = Environment.ProcessorCount * 10;

        var tasks = new List<Task<Pokemon?>>();
        var semaphore = new SemaphoreSlim(excessiveThreadCount, excessiveThreadCount);

        foreach (var id in idArray)
        {
            await semaphore.WaitAsync(ct);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Simular trabalho CPU-bound em thread dedicada
                    Thread.SpinWait(10000); // Context switching overhead
                    return await _client.GetPokemonAsync(id, ct);
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct));
        }

        var results = await Task.WhenAll(tasks);
        return results.Where(p => p != null).ToList()!;
    }
}

/// <summary>
/// SOLUÇÃO: Balanceamento de Carga - Distribui trabalho uniformemente
/// Demonstra: Uso eficiente do ThreadPool com concorrência otimizada
/// </summary>
public class LoadBalancedStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;

    public string Name => "Load Balanced";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();

        // SOLUÇÃO: Usa número ideal de threads baseado em núcleos
        int optimalConcurrency = Environment.ProcessorCount;
        var results = new ConcurrentBag<Pokemon>();

        // Usa Parallel.ForEach com balanceamento automático
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = optimalConcurrency,
            CancellationToken = ct
        };

        var tasks = idArray.Select(async id =>
        {
            var pokemon = await _client.GetPokemonAsync(id, ct);
            if (pokemon != null)
                results.Add(pokemon);
        });

        // Limita concorrência usando SemaphoreSlim
        var semaphore = new SemaphoreSlim(optimalConcurrency);
        var limitedTasks = idArray.Select(async id =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var pokemon = await _client.GetPokemonAsync(id, ct);
                if (pokemon != null)
                    results.Add(pokemon);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(limitedTasks);

        return [.. results];
    }
}

/// <summary>
/// DEMONSTRAÇÃO: Estrutura Lock-Free usando ConcurrentQueue
/// Demonstra: Redução de contenção com operações atômicas
/// </summary>
public class LockFreeStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;

    public string Name => "Lock-Free Queue";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();

        // SOLUÇÃO: Usa estrutura lock-free para reduzir contenção
        var lockFreeQueue = new ConcurrentQueue<Pokemon>();
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var tasks = new List<Task>();

        foreach (var id in idArray)
        {
            await semaphore.WaitAsync(ct);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var pokemon = await _client.GetPokemonAsync(id, ct);
                    if (pokemon != null)
                        lockFreeQueue.Enqueue(pokemon); // Operação lock-free
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        return [.. lockFreeQueue];
    }
}

/// <summary>
/// DEMONSTRAÇÃO: Contenção com SpinLock vs Lock tradicional
/// Demonstra: Comportamento de SpinLock em cenários de alta contenção
/// </summary>
public class SpinLockContentionStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;
    private SpinLock _spinLock = new(enableThreadOwnerTracking: false);
    private readonly List<Pokemon> _results = [];
    private int _counter = 0;

    public string Name => "SpinLock Contention";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();
        _results.Clear();

        await Parallel.ForEachAsync(
            idArray,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
            async (id, token) =>
            {
                var pokemon = await _client.GetPokemonAsync(id, token);

                if (pokemon != null)
                {
                    // Demonstra contenção com SpinLock
                    bool lockTaken = false;
                    try
                    {
                        _spinLock.Enter(ref lockTaken);
                        Interlocked.Increment(ref _counter);
                        _results.Add(pokemon); // Operação protegida
                    }
                    finally
                    {
                        if (lockTaken) _spinLock.Exit();
                    }
                }
            });

        return [.. _results];
    }
}

/// <summary>
/// DEMONSTRAÇÃO: Task-based Parallelism com controle de concorrência
/// Demonstra: Uso eficiente de Tasks e throttling
/// </summary>
public class TaskParallelismStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;

    public string Name => "Task Parallelism";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();

        // Demonstra paralelismo baseado em tarefas
        var concurrencyLevel = Math.Min(Environment.ProcessorCount, idArray.Length);
        var throttler = new SemaphoreSlim(concurrencyLevel);

        var tasks = idArray.Select(async id =>
        {
            await throttler.WaitAsync(ct);
            try
            {
                return await _client.GetPokemonAsync(id, ct);
            }
            finally
            {
                throttler.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(p => p != null).ToList()!;
    }
}

/// <summary>
/// DEMONSTRAÇÃO: Thread Local Storage para evitar contenção
/// Demonstra: Isolamento de dados por thread para eliminar sincronização
/// </summary>
public class ThreadLocalStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;
    private readonly ThreadLocal<List<Pokemon>> _threadLocalResults = new(() => []);

    public string Name => "Thread Local Storage";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();

        // Cada thread tem sua própria lista local - sem contenção
        await Parallel.ForEachAsync(
            idArray,
            new ParallelOptions { CancellationToken = ct },
            async (id, token) =>
            {
                var pokemon = await _client.GetPokemonAsync(id, token);
                if (pokemon != null)
                {
                    // Acesso sem lock - cada thread tem sua lista
                    _threadLocalResults.Value!.Add(pokemon);
                }
            });

        // Combina resultados de todas as threads
        var allResults = new List<Pokemon>();
        foreach (var localList in _threadLocalResults.Values)
        {
            allResults.AddRange(localList);
        }

        return allResults;
    }
}

/// <summary>
/// DEMONSTRAÇÃO: Concurrent Bag para agregação paralela
/// Demonstra: Estrutura otimizada para cenários de producer-consumer
/// </summary>
public class ConcurrentBagStrategy(PokeApiClient client) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;

    public string Name => "Concurrent Bag";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();
        var results = new ConcurrentBag<Pokemon>();

        // ConcurrentBag é otimizado para cada thread adicionar seus próprios itens
        await Parallel.ForEachAsync(
            idArray,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = ct
            },
            async (id, token) =>
            {
                var pokemon = await _client.GetPokemonAsync(id, token);
                if (pokemon != null)
                    results.Add(pokemon); // Thread-safe sem locks pesados
            });

        return [.. results];
    }
}

/// <summary>
/// DEMONSTRAÇÃO: Batching para reduzir overhead
/// Demonstra: Processamento em lotes para amortizar custos de sincronização
/// </summary>
public class BatchProcessingStrategy(PokeApiClient client, int batchSize = 10) : IPokemonFetchStrategy
{
    private readonly PokeApiClient _client = client;
    private readonly int _batchSize = batchSize;

    public string Name => "Batch Processing";

    public async Task<IReadOnlyList<Pokemon>> FetchAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var idArray = ids.ToArray();
        var results = new ConcurrentBag<Pokemon>();

        // Divide IDs em lotes para reduzir overhead de coordenação
        var batches = idArray
            .Select((id, index) => new { id, index })
            .GroupBy(x => x.index / _batchSize)
            .Select(g => g.Select(x => x.id).ToArray());

        await Parallel.ForEachAsync(
            batches,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = ct
            },
            async (batch, token) =>
            {
                // Processa lote inteiro em uma thread
                foreach (var id in batch)
                {
                    var pokemon = await _client.GetPokemonAsync(id, token);
                    if (pokemon != null)
                        results.Add(pokemon);
                }
            });

        return [.. results];
    }
}
