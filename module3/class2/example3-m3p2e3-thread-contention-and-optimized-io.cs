/*
 * =====================================================================================
 * EXAMPLE 3 - THREAD CONTENTION VS OPTIMIZED DISK I/O (COMBINED DEMO)
 * =====================================================================================
 * 
 * Based on:
 * - example1-m3p2e1-thread-contention-disk-io.cs (creates contention/inefficiency)
 * - example2-m3p2e2-disk-io-solved.cs (optimized async I/O, batching, caching)
 * 
 * Goal: Run two modes side-by-side (toggle) to show dramatic performance contrast:
 * - Problem mode: many tiny synchronous operations with random access and locking
 * - Optimized mode: async I/O, large buffers, sequential access, batching, caching
 * 
 * Press Ctrl+C to stop. Both loops run continuously until interrupted.
 * =====================================================================================
 */

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace DiskIOCombinedDemo
{
    public static class Config
    {
        // General
        public const bool RUN_PROBLEM_MODE = true;                  // true = contention demo; false = optimized demo
        public const int DISPLAY_INTERVAL_MS = 1000;

        // Problem mode (from example1)
        public const int DISK_THRASHING_THREADS = 32;
        public const int OPERATIONS_PER_THREAD = int.MaxValue;      // loop forever
        public const int TINY_WRITE_SIZE = 64;
        public const int TINY_READ_SIZE = 32;
        public const int RANDOM_FILES_COUNT = 200;
        public const int SEEK_OPERATIONS_PER_CYCLE = 50;
        public const int FILE_FRAGMENT_SIZE = 128;
        public static readonly string BASE_DIR = Path.Combine(Path.GetTempPath(), "m3p2e3_problem");

        // Optimized mode (from example2)
        public const int THREAD_COUNT = 8;
        public const int MAX_CONCURRENT_OPERATIONS = 16;
        public const int LARGE_BUFFER_SIZE = 1 * 1024 * 1024;       // 1MB
        public const int MEDIUM_BUFFER_SIZE = 64 * 1024;            // 64KB
        public const int MAX_FILE_SIZE = 10 * 1024 * 1024;          // 10MB
        public const int FILE_COUNT = 100;
        public const int OPERATIONS_PER_FILE = 10_000;              // effectively forever
        public const int BATCH_SIZE = 100;
        public const int BATCH_QUEUE_CAPACITY = 1000;
        public static readonly string OPT_DIR = Path.Combine(Path.GetTempPath(), "m3p2e3_optimized");
    }

    // =====================================================================================
    // PROBLEM IMPLEMENTATION (thread contention + tiny I/O)
    // =====================================================================================
    public sealed class ProblemDiskWorker
    {
        private readonly object _serializeLock = new object();
        private readonly List<string> _randomFiles = new List<string>();
        private readonly Random _random = new Random();
        private long _ops, _bytes;

        public ProblemDiskWorker()
        {
            Directory.CreateDirectory(Config.BASE_DIR);
            Directory.CreateDirectory(Path.Combine(Config.BASE_DIR, "random"));
            for (int i = 0; i < Config.RANDOM_FILES_COUNT; i++)
            {
                string f = Path.Combine(Config.BASE_DIR, "random", $"random_{i}.dat");
                File.WriteAllBytes(f, new byte[Config.FILE_FRAGMENT_SIZE]);
                _randomFiles.Add(f);
            }
        }

        public async Task RunAsync(CancellationToken ct)
        {
            var tasks = new List<Task>();
            for (int t = 0; t < Config.DISK_THRASHING_THREADS; t++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    int localId = Environment.CurrentManagedThreadId;
                    int op = 0;
                    while (!ct.IsCancellationRequested)
                    {
                        await TinyWriteAsync(localId, op++);
                        await TinyReadAsync();
                        await RandomSeekBurstAsync();
                    }
                }, ct));
            }

            // monitor
            var monitor = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(Config.DISPLAY_INTERVAL_MS, ct);
                    double sec = Math.Max(1, sw.Elapsed.TotalSeconds);
                    long ops = Interlocked.Read(ref _ops);
                    long bytes = Interlocked.Read(ref _bytes);
                    Console.WriteLine($"[PROBLEM] ops/s={(ops/sec):N0}  MB/s={(bytes/1048576.0/sec):N2}");
                }
            }, ct);

            await Task.WhenAll(tasks.Concat(new[] { monitor }));
        }

        private Task TinyWriteAsync(int threadId, int op)
        {
            byte[] data = new byte[Config.TINY_WRITE_SIZE];
            for (int i = 0; i < data.Length; i++) data[i] = (byte)(threadId & 0xFF);
            string file = Path.Combine(Config.BASE_DIR, $"tiny_{threadId}_{op}.dat");
            lock (_serializeLock)
            {
                using var fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
                fs.Write(data, 0, data.Length);
                fs.Flush(true);
            }
            Interlocked.Add(ref _bytes, data.Length);
            Interlocked.Increment(ref _ops);
            return Task.CompletedTask;
        }

        private Task TinyReadAsync()
        {
            if (_randomFiles.Count == 0) return Task.CompletedTask;
            string file = _randomFiles[_random.Next(_randomFiles.Count)];
            lock (_serializeLock)
            {
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] buf = new byte[Config.TINY_READ_SIZE];
                int read = fs.Read(buf, 0, buf.Length);
                Interlocked.Add(ref _bytes, read);
            }
            Interlocked.Increment(ref _ops);
            return Task.CompletedTask;
        }

        private Task RandomSeekBurstAsync()
        {
            if (_randomFiles.Count == 0) return Task.CompletedTask;
            for (int i = 0; i < Config.SEEK_OPERATIONS_PER_CYCLE; i++)
            {
                string file = _randomFiles[_random.Next(_randomFiles.Count)];
                lock (_serializeLock)
                {
                    using var fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    long pos = _random.Next(0, (int)Math.Max(1, fs.Length - 8));
                    fs.Seek(pos, SeekOrigin.Begin);
                    byte[] b = new byte[8];
                    int r = fs.Read(b, 0, b.Length);
                    Interlocked.Add(ref _bytes, r);
                }
                Interlocked.Increment(ref _ops);
            }
            return Task.CompletedTask;
        }
    }

    // =====================================================================================
    // OPTIMIZED IMPLEMENTATION (async I/O, batching, caching)
    // =====================================================================================
    public sealed class OptimizedDiskWorker : IDisposable
    {
        private readonly Random _random = new Random();
        private readonly ConcurrentDictionary<string, FileStream> _handles = new();
        private readonly BlockingCollection<string> _reads = new(new ConcurrentQueue<string>(), Config.BATCH_QUEUE_CAPACITY);
        private readonly BlockingCollection<byte[]> _writes = new(new ConcurrentQueue<byte[]>(), Config.BATCH_QUEUE_CAPACITY);
        private long _ops, _bytes;

        public OptimizedDiskWorker()
        {
            Directory.CreateDirectory(Config.OPT_DIR);
            // seed files
            for (int i = 0; i < Config.FILE_COUNT; i++)
            {
                string p = Path.Combine(Config.OPT_DIR, $"file_{i}.dat");
                if (!File.Exists(p)) File.WriteAllBytes(p, new byte[Config.MAX_FILE_SIZE]);
            }
        }

        public async Task RunAsync(CancellationToken ct)
        {
            var producers = new List<Task>();
            for (int i = 0; i < Config.MAX_CONCURRENT_OPERATIONS; i++)
            {
                producers.Add(Task.Run(async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        string file = Path.Combine(Config.OPT_DIR, $"file_{_random.Next(Config.FILE_COUNT)}.dat");
                        _reads.Add(file, ct);
                        byte[] data = GeneratePayload(Config.LARGE_BUFFER_SIZE);
                        _writes.Add(data, ct);
                        if (_reads.Count >= Config.BATCH_SIZE)
                            await ProcessBatchAsync(ct);
                    }
                }, ct));
            }

            var monitor = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(Config.DISPLAY_INTERVAL_MS, ct);
                    double sec = Math.Max(1, sw.Elapsed.TotalSeconds);
                    long ops = Interlocked.Read(ref _ops);
                    long bytes = Interlocked.Read(ref _bytes);
                    Console.WriteLine($"[OPTIMIZED] ops/s={(ops/sec):N0}  MB/s={(bytes/1048576.0/sec):N2}");
                }
            }, ct);

            await Task.WhenAll(producers.Concat(new[] { monitor }));
        }

        private async Task ProcessBatchAsync(CancellationToken ct)
        {
            var readTasks = new List<Task>();
            var writeTasks = new List<Task>();

            while (_reads.TryTake(out string? read, 0, ct))
            {
                readTasks.Add(ReadAsync(read, ct));
            }

            while (_writes.TryTake(out byte[]? data, 0, ct))
            {
                // write to a random file sequentially
                string target = Path.Combine(Config.OPT_DIR, $"file_{_random.Next(Config.FILE_COUNT)}.dat");
                writeTasks.Add(WriteAsync(target, data, ct));
            }

            await Task.WhenAll(readTasks.Concat(writeTasks));
        }

        private async Task ReadAsync(string filePath, CancellationToken ct)
        {
            var fs = await GetHandleAsync(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[Config.LARGE_BUFFER_SIZE];
            int read = await fs.ReadAsync(buffer, 0, buffer.Length, ct);
            Interlocked.Add(ref _bytes, read);
            Interlocked.Increment(ref _ops);
        }

        private async Task WriteAsync(string filePath, byte[] data, CancellationToken ct)
        {
            var fs = await GetHandleAsync(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            int offset = 0;
            while (offset < data.Length)
            {
                int chunk = Math.Min(Config.LARGE_BUFFER_SIZE, data.Length - offset);
                await fs.WriteAsync(data, offset, chunk, ct);
                offset += chunk;
                Interlocked.Add(ref _bytes, chunk);
                Interlocked.Increment(ref _ops);
            }
            await fs.FlushAsync(ct);
        }

        private async Task<FileStream> GetHandleAsync(string path, FileMode mode, FileAccess access)
        {
            if (_handles.TryGetValue(path, out var cached)) return cached;
            var fs = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite, Config.MEDIUM_BUFFER_SIZE, true);
            _handles[path] = fs;
            await Task.CompletedTask;
            return fs;
        }

        private static byte[] GeneratePayload(int size)
        {
            var data = new byte[size];
            var rnd = new Random();
            for (int i = 0; i < size; i += 4)
            {
                int r = rnd.Next();
                int left = Math.Min(4, size - i);
                for (int j = 0; j < left; j++) data[i + j] = (byte)((r >> (8 * j)) & 0xFF);
            }
            return data;
        }

        public void Dispose()
        {
            foreach (var s in _handles.Values) s.Dispose();
            _handles.Clear();
        }
    }

    // =====================================================================================
    // MAIN
    // =====================================================================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("EXAMPLE 3 - THREAD CONTENTION VS OPTIMIZED DISK I/O (COMBINED)");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine(Config.RUN_PROBLEM_MODE ? "Mode: PROBLEM (contention, tiny I/O)" : "Mode: OPTIMIZED (async, batching, caching)");
            Console.WriteLine("Press Ctrl+C to stop.");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); Console.WriteLine("\nStopping..."); };

            try
            {
                if (Config.RUN_PROBLEM_MODE)
                {
                    var worker = new ProblemDiskWorker();
                    await worker.RunAsync(cts.Token);
                }
                else
                {
                    using var worker = new OptimizedDiskWorker();
                    await worker.RunAsync(cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}


