using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockingScope.Solved
{
    public class FileProcessor
    {
        // SOLUÇÃO 1: Usar estrutura thread-safe ao invés de lock
        private readonly ConcurrentBag<ProcessResult> _results = new ConcurrentBag<ProcessResult>();
        private int _processedCount = 0;

        public class ProcessResult
        {
            public string FileName { get; set; }
            public int LineCount { get; set; }
            public int WordCount { get; set; }
            public long FileSize { get; set; }
            public TimeSpan ProcessingTime { get; set; }
        }

        public void ProcessFiles(string[] filePaths, int threadCount)
        {
            Console.WriteLine($"Processando {filePaths.Length} arquivos com {threadCount} threads...");
            Console.WriteLine("SOLUÇÃO: Lock apenas onde necessário + estruturas concurrent!\n");

            var sw = Stopwatch.StartNew();

            // SOLUÇÃO 2: Usar Parallel.ForEach para melhor distribuição de trabalho
            var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

            Parallel.ForEach(filePaths, options, (filePath) =>
            {
                ProcessFileGoodLocking(filePath);
            });

            sw.Stop();

            Console.WriteLine($"\n=== RESULTADO (OTIMIZADO) ===");
            Console.WriteLine($"Tempo total: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Arquivos processados: {_processedCount}");
            Console.WriteLine($"Throughput: {_processedCount / sw.Elapsed.TotalSeconds:F2} arquivos/segundo");
            Console.WriteLine($"Tempo médio por arquivo: {sw.ElapsedMilliseconds / (double)_processedCount:F2}ms");
            Console.WriteLine($"Speedup vs. versão com problema: ~3-4x mais rápido!");
        }

        private void ProcessFileGoodLocking(string filePath)
        {
            var sw = Stopwatch.StartNew();

            // I/O SEM LOCK - cada thread lê seu próprio arquivo
            string content = File.ReadAllText(filePath);

            // PROCESSAMENTO SEM LOCK - operação local, independente
            int lineCount = content.Split('\n').Length;
            int wordCount = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            long fileSize = new FileInfo(filePath).Length;

            // Simular processamento adicional (CPU-bound)
            Thread.Sleep(10);
            var hash = ComputeSimpleHash(content);

            sw.Stop();

            // SOLUÇÃO 3: ConcurrentBag é thread-safe, não precisa de lock
            var result = new ProcessResult
            {
                FileName = Path.GetFileName(filePath),
                LineCount = lineCount,
                WordCount = wordCount,
                FileSize = fileSize,
                ProcessingTime = sw.Elapsed
            };

            _results.Add(result);

            // SOLUÇÃO 4: Apenas o incremento precisa ser atômico
            var count = Interlocked.Increment(ref _processedCount);

            if (count % 10 == 0)
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Processados {count} arquivos");
            }
        }

        private int ComputeSimpleHash(string content)
        {
            int hash = 0;
            foreach (char c in content)
            {
                hash = (hash * 31 + c) & 0x7FFFFFFF;
            }
            return hash;
        }

        public void PrintStatistics()
        {
            var resultsList = _results.ToList(); // Snapshot dos resultados

            Console.WriteLine("\n=== ESTATÍSTICAS ===");
            Console.WriteLine($"Total de arquivos: {resultsList.Count}");
            Console.WriteLine($"Total de linhas: {resultsList.Sum(r => r.LineCount)}");
            Console.WriteLine($"Total de palavras: {resultsList.Sum(r => r.WordCount)}");
            Console.WriteLine($"Tamanho total: {resultsList.Sum(r => r.FileSize) / 1024.0:F2} KB");
        }

        // SOLUÇÃO 5: Alternativa usando BlockingCollection para producer-consumer
        public void ProcessFilesWithBlockingCollection(string[] filePaths, int producerThreads, int consumerThreads)
        {
            Console.WriteLine($"\nAlternativa: Producer-Consumer com BlockingCollection");
            Console.WriteLine($"Produtores: {producerThreads}, Consumidores: {consumerThreads}\n");

            var workQueue = new BlockingCollection<string>(boundedCapacity: 10);
            var sw = Stopwatch.StartNew();

            // Producers: colocam arquivos na fila
            var producerTask = Task.Run(() =>
            {
                Parallel.ForEach(filePaths,
                    new ParallelOptions { MaxDegreeOfParallelism = producerThreads },
                    (filePath) =>
                    {
                        workQueue.Add(filePath);
                    });
                workQueue.CompleteAdding();
            });

            // Consumers: processam arquivos da fila
            var consumerTasks = new List<Task>();
            for (int i = 0; i < consumerThreads; i++)
            {
                var task = Task.Run(() =>
                {
                    foreach (var filePath in workQueue.GetConsumingEnumerable())
                    {
                        ProcessFileGoodLocking(filePath);
                    }
                });
                consumerTasks.Add(task);
            }

            // Aguardar conclusão
            Task.WaitAll(consumerTasks.ToArray());
            sw.Stop();

            Console.WriteLine($"Tempo com Producer-Consumer: {sw.ElapsedMilliseconds}ms");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("CASO 2: LOCKING SCOPE - RESOLVIDO");
            Console.WriteLine("========================================\n");

            Console.WriteLine("Técnicas de otimização aplicadas:");
            Console.WriteLine("- ConcurrentBag<T> ao invés de List<T> + lock");
            Console.WriteLine("- Interlocked para operações atômicas");
            Console.WriteLine("- Lock apenas onde realmente necessário");
            Console.WriteLine("- Parallel.ForEach para melhor distribuição\n");

            // Criar arquivos de teste
            Console.WriteLine("Criando arquivos de teste...");
            var testFiles = CreateTestFiles(50);

            Console.WriteLine($"Criados {testFiles.Length} arquivos de teste.\n");

            var processor = new FileProcessor();

            // Testar com diferentes números de threads
            int[] threadCounts = { 1, 2, 4, 8 };

            foreach (var threadCount in threadCounts)
            {
                Console.WriteLine(new string('=', 50));
                processor = new FileProcessor(); // Reset
                processor.ProcessFiles(testFiles, threadCount);
                processor.PrintStatistics();
                Console.WriteLine();

                Thread.Sleep(500);
            }

            // Demonstrar alternativa com BlockingCollection
            Console.WriteLine(new string('=', 50));
            processor = new FileProcessor();
            processor.ProcessFilesWithBlockingCollection(testFiles, producerThreads: 2, consumerThreads: 4);
            processor.PrintStatistics();

            Console.WriteLine("\n=== ANÁLISE ===");
            Console.WriteLine("Observe que a performance escala com o número de threads!");
            Console.WriteLine("Isso indica BAIXA CONTENÇÃO - threads trabalhando em paralelo.");
            Console.WriteLine("\nCompare com a versão 'problem' para ver a diferença.");

            // Limpar arquivos de teste
            CleanupTestFiles(testFiles);

            Console.WriteLine("\n[Pressione qualquer tecla para sair]");
            Console.ReadKey();
        }

        static string[] CreateTestFiles(int count)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "LockingScope_Test_Solved");
            Directory.CreateDirectory(tempDir);

            var files = new string[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                var filePath = Path.Combine(tempDir, $"test_file_{i:D3}.txt");
                var sb = new StringBuilder();

                int lineCount = random.Next(50, 200);
                for (int line = 0; line < lineCount; line++)
                {
                    int wordCount = random.Next(5, 15);
                    for (int word = 0; word < wordCount; word++)
                    {
                        sb.Append(GenerateRandomWord(random));
                        sb.Append(' ');
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(filePath, sb.ToString());
                files[i] = filePath;
            }

            return files;
        }

        static string GenerateRandomWord(Random random)
        {
            int length = random.Next(3, 10);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)('a' + random.Next(26));
            }
            return new string(chars);
        }

        static void CleanupTestFiles(string[] files)
        {
            if (files.Length > 0)
            {
                var tempDir = Path.GetDirectoryName(files[0]);
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }
    }
}
