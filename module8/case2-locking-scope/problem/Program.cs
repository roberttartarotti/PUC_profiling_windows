using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockingScope.Problem
{
    public class FileProcessor
    {
        private readonly object _lock = new object();
        private readonly List<ProcessResult> _results = new List<ProcessResult>();
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
            Console.WriteLine("PROBLEMA: Lock com escopo muito amplo!\n");

            var sw = Stopwatch.StartNew();

            // Criar threads manualmente para demonstrar o problema
            var threads = new List<Thread>();
            int filesPerThread = filePaths.Length / threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                int startIndex = threadIndex * filesPerThread;
                int endIndex = (threadIndex == threadCount - 1) ? filePaths.Length : (threadIndex + 1) * filesPerThread;

                var thread = new Thread(() =>
                {
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        ProcessFileBadLocking(filePaths[j]);
                    }
                });

                thread.Start();
                threads.Add(thread);
            }

            // Aguardar todas as threads
            foreach (var thread in threads)
            {
                thread.Join();
            }

            sw.Stop();

            Console.WriteLine($"\n=== RESULTADO ===");
            Console.WriteLine($"Tempo total: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Arquivos processados: {_processedCount}");
            Console.WriteLine($"Throughput: {_processedCount / sw.Elapsed.TotalSeconds:F2} arquivos/segundo");
            Console.WriteLine($"Tempo médio por arquivo: {sw.ElapsedMilliseconds / (double)_processedCount:F2}ms");
        }

        private void ProcessFileBadLocking(string filePath)
        {
            // PROBLEMA: Lock protege TUDO, incluindo I/O e processamento
            lock (_lock)
            {
                var sw = Stopwatch.StartNew();

                // I/O NÃO PRECISA DE LOCK - threads diferentes lendo arquivos diferentes
                string content = File.ReadAllText(filePath);

                // PROCESSAMENTO NÃO PRECISA DE LOCK - operação local
                int lineCount = content.Split('\n').Length;
                int wordCount = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                long fileSize = new FileInfo(filePath).Length;

                // Simular processamento adicional (CPU-bound)
                Thread.Sleep(10); // Simula trabalho
                var hash = ComputeSimpleHash(content);

                sw.Stop();

                // Apenas ISSO precisa de lock (atualização de estrutura compartilhada)
                var result = new ProcessResult
                {
                    FileName = Path.GetFileName(filePath),
                    LineCount = lineCount,
                    WordCount = wordCount,
                    FileSize = fileSize,
                    ProcessingTime = sw.Elapsed
                };

                _results.Add(result);
                _processedCount++;

                if (_processedCount % 10 == 0)
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Processados {_processedCount} arquivos");
                }
            }
            // Thread fica bloqueada durante TODO o processamento!
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
            Console.WriteLine("\n=== ESTATÍSTICAS ===");
            Console.WriteLine($"Total de arquivos: {_results.Count}");
            Console.WriteLine($"Total de linhas: {_results.Sum(r => r.LineCount)}");
            Console.WriteLine($"Total de palavras: {_results.Sum(r => r.WordCount)}");
            Console.WriteLine($"Tamanho total: {_results.Sum(r => r.FileSize) / 1024.0:F2} KB");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("======================================");
            Console.WriteLine("CASO 2: LOCKING SCOPE - PROBLEMA");
            Console.WriteLine("======================================\n");

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
                processor.ProcessFiles(testFiles, threadCount);
                processor.PrintStatistics();
                Console.WriteLine();

                // Reset para próximo teste
                Thread.Sleep(1000);
            }

            Console.WriteLine("\n=== ANÁLISE ===");
            Console.WriteLine("Observe que aumentar o número de threads não melhora muito a performance!");
            Console.WriteLine("Isso indica LOCK CONTENTION - threads esperando umas pelas outras.");
            Console.WriteLine("\nUse o Concurrency Visualizer para ver as threads bloqueadas.");

            // Limpar arquivos de teste
            CleanupTestFiles(testFiles);

            Console.WriteLine("\n[Pressione qualquer tecla para sair]");
            Console.ReadKey();
        }

        static string[] CreateTestFiles(int count)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "LockingScope_Test");
            Directory.CreateDirectory(tempDir);

            var files = new string[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                var filePath = Path.Combine(tempDir, $"test_file_{i:D3}.txt");
                var sb = new StringBuilder();

                // Gerar conteúdo aleatório
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
