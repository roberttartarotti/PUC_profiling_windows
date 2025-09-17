using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultithreadDemo
{
    class Program
    {
        // ANÁLISE MULTITHREAD COM SAMPLING PROFILER
        // Demonstra distribuição de CPU entre múltiplas threads
        // Objetivo: Observar como o profiler captura atividade paralela

        private static readonly object ConsoleLock = new object();
        private static int contadorGlobal = 0;

        // Função computacionalmente intensiva para simular trabalho real
        // Esta função será executada por múltiplas threads simultaneamente
        static double CalcularTrabalhoIntensivo(int threadId, int inicio, int fim, string tipoTrabalho)
        {
            double resultado = 0.0;

            if (tipoTrabalho == "matematico")
            {
                // Processamento matemático intensivo
                for (int i = inicio; i <= fim; i++)
                {
                    double valor = (double)i;

                    // Operações matemáticas complexas
                    for (int j = 0; j < 1000; j++)
                    {
                        valor = Math.Sin(valor) * Math.Cos(valor);
                        valor = Math.Sqrt(valor * valor + 1.0);
                        valor = Math.Log(Math.Abs(valor) + 1.0);
                    }

                    resultado += valor;
                    Interlocked.Increment(ref contadorGlobal);

                    // Mostrar progresso periodicamente
                    if (i % 1000 == 0)
                    {
                        lock (ConsoleLock)
                        {
                            Console.WriteLine($"  Thread {threadId} processando: {i}/{fim} (resultado parcial: {resultado:F2})");
                        }
                    }
                }
            }
            else if (tipoTrabalho == "simulacao")
            {
                // Simulação de processamento de dados/servidor
                Random random = new Random(threadId * 1000); // Seed baseada na thread

                for (int i = inicio; i <= fim; i++)
                {
                    // Simular processamento de requisições/dados
                    double dados = random.NextDouble() * 100.0 + 1.0;

                    // Processamento simulado
                    for (int k = 0; k < 500; k++)
                    {
                        dados = Math.Pow(dados, 1.1);
                        dados = Math.Sqrt(dados);
                        dados = Math.Sin(dados) + Math.Cos(dados);
                    }

                    resultado += dados;
                    Interlocked.Increment(ref contadorGlobal);

                    // Simular variação de carga de trabalho
                    if (i % 2 == 0)
                    {
                        Thread.Sleep(0); // Yield para outras threads
                    }

                    if (i % 1500 == 0)
                    {
                        lock (ConsoleLock)
                        {
                            Console.WriteLine($"  Thread {threadId} (simulação) processando: {i}/{fim}");
                        }
                    }
                }
            }

            lock (ConsoleLock)
            {
                Console.WriteLine($"✓ Thread {threadId} concluída! Resultado: {resultado:F2}");
            }

            return resultado;
        }

        // Demonstração usando Parallel.For
        static void DemonstracaoParallelFor()
        {
            Console.WriteLine("\n=== DEMONSTRAÇÃO: PARALLEL.FOR (.NET) ===");
            Console.WriteLine("Usando Parallel.For para processamento paralelo automático...");

            contadorGlobal = 0;
            const int TOTAL_ITERACOES = 20000;
            const int TRABALHO_POR_ITERACAO = 500;

            Console.WriteLine("Configuração:");
            Console.WriteLine($"- Total de iterações: {TOTAL_ITERACOES}");
            Console.WriteLine($"- Trabalho por iteração: {TRABALHO_POR_ITERACAO} operações matemáticas");
            Console.WriteLine($"- Paralelismo: Automático (.NET Thread Pool)");
            Console.WriteLine($"- Threads disponíveis: {Environment.ProcessorCount}");
            Console.WriteLine();

            double[] resultados = new double[TOTAL_ITERACOES];
            object progressLock = new object();
            int progressCounter = 0;

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Parallel.For com monitoramento de progresso
            ParallelLoopResult resultado = Parallel.For(0, TOTAL_ITERACOES, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, i =>
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                double valor = (double)i;

                // Processamento matemático por iteração
                for (int j = 0; j < TRABALHO_POR_ITERACAO; j++)
                {
                    valor = Math.Sin(valor) * Math.Cos(valor) + 1.0;
                    valor = Math.Sqrt(valor * valor + 1.0);
                    valor = Math.Log(Math.Abs(valor) + 1.0);
                }

                resultados[i] = valor;
                Interlocked.Increment(ref contadorGlobal);

                // Progresso thread-safe
                int currentProgress = Interlocked.Increment(ref progressCounter);
                if (currentProgress % 2000 == 0)
                {
                    lock (progressLock)
                    {
                        Console.WriteLine($"  Progresso: {currentProgress}/{TOTAL_ITERACOES} (Thread ID: {threadId})");
                    }
                }
            });

            stopwatch.Stop();

            double somaTotal = resultados.Sum();

            Console.WriteLine();
            Console.WriteLine("RESULTADOS PARALLEL.FOR:");
            Console.WriteLine($"- Tempo total: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"- Soma total: {somaTotal:F2}");
            Console.WriteLine($"- Operações processadas: {contadorGlobal}");
            Console.WriteLine($"- Loop completado: {resultado.IsCompleted}");
            Console.WriteLine($"- Throughput: {(double)TOTAL_ITERACOES / stopwatch.ElapsedMilliseconds * 1000:F0} iterações/s");
        }

        // Demonstração usando Task.Run para tarefas assíncronas
        static async Task DemonstracaoTaskAsync()
        {
            Console.WriteLine("\n=== DEMONSTRAÇÃO: TASK.RUN (ASYNC/AWAIT) ===");
            Console.WriteLine("Usando Task.Run para processamento assíncrono...");

            contadorGlobal = 0;
            const int NUM_TASKS = 8;
            const int TRABALHO_POR_TASK = 3000;

            Console.WriteLine("Configuração:");
            Console.WriteLine($"- Número de Tasks: {NUM_TASKS}");
            Console.WriteLine($"- Trabalho por Task: {TRABALHO_POR_TASK} iterações");
            Console.WriteLine($"- Tipo de processamento: Simulação de servidor");
            Console.WriteLine();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Criar array de tasks
            Task<double>[] tasks = new Task<double>[NUM_TASKS];

            for (int i = 0; i < NUM_TASKS; i++)
            {
                int taskId = i;
                int inicio = taskId * TRABALHO_POR_TASK;
                int fim = inicio + TRABALHO_POR_TASK - 1;

                tasks[i] = Task.Run(() =>
                {
                    return CalcularTrabalhoIntensivo(taskId, inicio, fim, "simulacao");
                });
            }

            // Aguardar todas as tasks completarem
            double[] resultados = await Task.WhenAll(tasks);

            stopwatch.Stop();

            double resultadoTotal = resultados.Sum();

            Console.WriteLine();
            Console.WriteLine("RESULTADOS TASK.RUN:");
            Console.WriteLine($"- Tempo total: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"- Resultado combinado: {resultadoTotal:F2}");
            Console.WriteLine($"- Operações processadas: {contadorGlobal}");
            Console.WriteLine($"- Tasks completadas: {tasks.Count(t => t.IsCompletedSuccessfully)}");
        }

        // Simulação de processamento de servidor web com ThreadPool
        static void SimulacaoServidorWeb()
        {
            Console.WriteLine("\n=== SIMULAÇÃO: SERVIDOR WEB MULTITHREAD ===");
            Console.WriteLine("Simulando processamento de requisições HTTP paralelas...");

            contadorGlobal = 0;
            const int TOTAL_REQUISICOES = 5000;
            const int BATCH_SIZE = 100;

            Console.WriteLine("Cenário do servidor:");
            Console.WriteLine($"- Total de requisições: {TOTAL_REQUISICOES}");
            Console.WriteLine($"- Processamento em batches de: {BATCH_SIZE}");
            Console.WriteLine($"- Thread Pool: Gerenciado pelo .NET");
            Console.WriteLine($"- Worker Threads: {ThreadPool.ThreadCount}");
            Console.WriteLine();

            // Fila thread-safe para requisições
            ConcurrentQueue<int> filaRequisicoes = new ConcurrentQueue<int>();
            for (int i = 0; i < TOTAL_REQUISICOES; i++)
            {
                filaRequisicoes.Enqueue(i);
            }

            int requisicoesConcluidas = 0;
            object statsLock = new object();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Criar workers usando ThreadPool
            int numWorkers = Environment.ProcessorCount * 2;
            Task[] workers = new Task[numWorkers];

            for (int workerId = 0; workerId < numWorkers; workerId++)
            {
                int id = workerId;
                workers[workerId] = Task.Run(() =>
                {
                    Random random = new Random(id * 1000);
                    int processadasPorWorker = 0;

                    while (filaRequisicoes.TryDequeue(out int requisicaoId))
                    {
                        // Simular processamento de requisição HTTP
                        double carga = random.NextDouble() * 10.0 + 1.0;
                        double resultado = 0.0;

                        // Processamento variável por requisição
                        int iteracoes = random.Next(100, 1000);
                        for (int i = 0; i < iteracoes; i++)
                        {
                            resultado += Math.Sin(carga * i) * Math.Cos(carga * i);
                            resultado = Math.Sqrt(resultado * resultado + 1.0);
                        }

                        processadasPorWorker++;
                        int totalConcluidas = Interlocked.Increment(ref requisicoesConcluidas);
                        Interlocked.Increment(ref contadorGlobal);

                        // Log periódico thread-safe
                        if (totalConcluidas % 500 == 0)
                        {
                            lock (statsLock)
                            {
                                Console.WriteLine($"  Worker {id} - Total processadas: {totalConcluidas}/{TOTAL_REQUISICOES} " +
                                                $"(este worker: {processadasPorWorker})");
                            }
                        }

                        // Simular tempo de I/O ocasional
                        if (requisicaoId % 50 == 0)
                        {
                            Thread.Sleep(1);
                        }
                    }

                    lock (statsLock)
                    {
                        Console.WriteLine($"✓ Worker {id} finalizou! Processou {processadasPorWorker} requisições");
                    }
                });
            }

            // Aguardar todos os workers
            Task.WaitAll(workers);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("RESULTADOS SIMULAÇÃO SERVIDOR:");
            Console.WriteLine($"- Tempo total: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"- Requisições processadas: {requisicoesConcluidas}");
            Console.WriteLine($"- Throughput: {(double)requisicoesConcluidas / stopwatch.ElapsedMilliseconds * 1000:F0} req/s");
            Console.WriteLine($"- Workers utilizados: {numWorkers}");

            // Estatísticas do ThreadPool
            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

            Console.WriteLine($"- ThreadPool - Workers disponíveis: {availableWorkerThreads}/{maxWorkerThreads}");
        }

        // Demonstração de PLINQ (Parallel LINQ)
        static void DemonstracaoPLINQ()
        {
            Console.WriteLine("\n=== DEMONSTRAÇÃO: PLINQ (PARALLEL LINQ) ===");
            Console.WriteLine("Usando PLINQ para processamento paralelo de dados...");

            const int TAMANHO_DADOS = 100000;
            Console.WriteLine($"Processando {TAMANHO_DADOS} elementos com PLINQ...");

            // Gerar dados de teste
            var dados = Enumerable.Range(1, TAMANHO_DADOS).ToArray();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Processamento paralelo com PLINQ
            var resultados = dados
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(x =>
                {
                    // Processamento matemático intensivo
                    double valor = (double)x;
                    for (int i = 0; i < 200; i++)
                    {
                        valor = Math.Sin(valor) * Math.Cos(valor);
                        valor = Math.Sqrt(valor * valor + 1.0);
                    }
                    return valor;
                })
                .Where(x => x > 0.5)
                .OrderBy(x => x)
                .ToArray();

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("RESULTADOS PLINQ:");
            Console.WriteLine($"- Tempo de processamento: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"- Elementos processados: {TAMANHO_DADOS}");
            Console.WriteLine($"- Elementos filtrados: {resultados.Length}");
            Console.WriteLine($"- Throughput: {(double)TAMANHO_DADOS / stopwatch.ElapsedMilliseconds * 1000:F0} elementos/s");
            Console.WriteLine($"- Primeiro resultado: {resultados.FirstOrDefault():F4}");
            Console.WriteLine($"- Último resultado: {resultados.LastOrDefault():F4}");
        }

        // Função principal de demonstração
        static async Task ExecutarDemonstracao()
        {
            Console.WriteLine("=== ANÁLISE MULTITHREAD COM SAMPLING PROFILER ===");
            Console.WriteLine("Objetivo: Observar distribuição de CPU entre threads paralelas");
            Console.WriteLine();

            Console.WriteLine("CENÁRIOS DE ANÁLISE:");
            Console.WriteLine("1. Parallel.For - Paralelismo automático do .NET");
            Console.WriteLine("2. Task.Run - Tarefas assíncronas com async/await");
            Console.WriteLine("3. Simulação de servidor web - ThreadPool workers");
            Console.WriteLine("4. PLINQ - Processamento paralelo de dados");
            Console.WriteLine();

            Console.WriteLine("ANÁLISE ESPERADA NO PROFILER:");
            Console.WriteLine("✓ CPU distribuído entre múltiplas threads");
            Console.WriteLine("✓ Hotspots em funções de trabalho paralelo");
            Console.WriteLine("✓ Utilização do ThreadPool do .NET");
            Console.WriteLine("✓ Padrões diferentes para cada tipo de paralelismo");
            Console.WriteLine();

            Console.WriteLine("HARDWARE DETECTADO:");
            Console.WriteLine($"- Processadores lógicos: {Environment.ProcessorCount}");
            Console.WriteLine($"- Threads de worker recomendadas: {Environment.ProcessorCount}");

            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            Console.WriteLine($"- ThreadPool max workers: {maxWorkerThreads}");
            Console.WriteLine();

            Console.WriteLine("Pressione ENTER para iniciar as demonstrações...");
            Console.ReadLine();
            Console.WriteLine();

            // Executar todas as demonstrações
            DemonstracaoParallelFor();
            await DemonstracaoTaskAsync();
            SimulacaoServidorWeb();
            DemonstracaoPLINQ();
        }

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("DEMONSTRAÇÃO MULTITHREAD - SAMPLING PROFILER");
                Console.WriteLine("============================================");
                Console.WriteLine();

                // Demonstração principal
                await ExecutarDemonstracao();

                Console.WriteLine();
                Console.WriteLine("=== INSTRUÇÕES PARA ANÁLISE NO PROFILER ===");
                Console.WriteLine();
                Console.WriteLine("1. CONFIGURAÇÃO DO PROFILER:");
                Console.WriteLine("   - Use 'CPU Usage' (sampling profiler)");
                Console.WriteLine("   - Ative visualização por threads");
                Console.WriteLine("   - Configure sampling rate adequado");
                Console.WriteLine();

                Console.WriteLine("2. PONTOS DE ANÁLISE:");
                Console.WriteLine("   ✓ Distribuição de CPU entre threads");
                Console.WriteLine("   ✓ Identificação de threads mais ativas");
                Console.WriteLine("   ✓ Utilização do ThreadPool do .NET");
                Console.WriteLine("   ✓ Padrões de execução paralela vs sequencial");
                Console.WriteLine();

                Console.WriteLine("3. MÉTRICAS IMPORTANTES:");
                Console.WriteLine("   - Utilização total de CPU (deve ser alta)");
                Console.WriteLine("   - Balanceamento entre threads");
                Console.WriteLine("   - Eficiência do paralelismo");
                Console.WriteLine("   - Overhead de sincronização");
                Console.WriteLine();

                Console.WriteLine("4. TÉCNICAS .NET DEMONSTRADAS:");
                Console.WriteLine("   - Parallel.For: Paralelismo de loops");
                Console.WriteLine("   - Task.Run: Tarefas assíncronas");
                Console.WriteLine("   - ThreadPool: Pool de threads gerenciado");
                Console.WriteLine("   - PLINQ: Processamento paralelo de dados");
                Console.WriteLine();

                Console.WriteLine("5. APLICAÇÕES REAIS:");
                Console.WriteLine("   - APIs web (ASP.NET Core)");
                Console.WriteLine("   - Processamento de dados em lote");
                Console.WriteLine("   - Sistemas de análise paralela");
                Console.WriteLine("   - Aplicações de alta concorrência");
                Console.WriteLine();

                Console.WriteLine("6. LIMITAÇÕES DO SAMPLING EM MULTITHREAD:");
                Console.WriteLine("   ⚠️ Pode perder sincronizações muito rápidas");
                Console.WriteLine("   ⚠️ ThreadPool pode mascarar detalhes de threads");
                Console.WriteLine("   ✅ Excelente para identificar padrões gerais");
                Console.WriteLine("   ✅ Mostra distribuição de carga efetivamente");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante execução: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}
