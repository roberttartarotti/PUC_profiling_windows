using PokeProfiler.Core.Instrumentation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeProfiler.Core.Strategies
{
    public class OptimizedScalableStrategy(PokeApiClient client) : IPokemonFetchStrategy
    {
        private readonly PokeApiClient _client = client;

        string IPokemonFetchStrategy.Name => "Optimized Scalable";

        async Task<IReadOnlyList<Pokemon>> IPokemonFetchStrategy.FetchAsync(IEnumerable<string> idsOrNames, CancellationToken ct)
        {
            using var activity = Telemetry.ActivitySource.StartActivity("OptimizedScalableStrategy.FetchAsync");
            
            // Concorrência ideal (baseada em ProcessorCount)
            var maxConcurrency = Math.Min(Environment.ProcessorCount * 2, 16);
            
            // Convert to array for efficient partitioning
            var items = idsOrNames.ToArray();
            if (items.Length == 0)
                return [];

            // Estruturas lock-free (ConcurrentQueue para melhor performance)
            var results = new ConcurrentQueue<Pokemon>();
            
            // Throttling apropriado (SemaphoreSlim)
            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            
            // Approach using direct task creation with semaphore (simpler and more efficient)
            var tasks = items.Select(async id =>
            {
                // Cancelamento (CancellationToken) - check before acquiring semaphore
                ct.ThrowIfCancellationRequested();
                
                await semaphore.WaitAsync(ct);
                try
                {
                    using var itemActivity = Telemetry.ActivitySource.StartActivity($"Fetch-{id}");
                    
                    // Cancelamento - check before API call
                    ct.ThrowIfCancellationRequested();
                    
                    var pokemon = await _client.GetPokemonAsync(id, ct);
                    if (pokemon != null)
                    {
                        results.Enqueue(pokemon);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Re-throw cancellation exceptions
                    throw;
                }
                catch (Exception ex)
                {
                    // Log other exceptions but don't fail the entire operation
                    activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);
            
            // Convert to ordered list efficiently
            return results.ToArray()
                         .OrderBy(p => p.id)
                         .ToArray();
        }
    }
}
