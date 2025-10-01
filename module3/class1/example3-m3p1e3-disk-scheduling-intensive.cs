using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace IntensiveDiskSchedulingDemo
{
    // ====================================================================
    // INTENSIVE DISK SCHEDULING PARAMETERS - ADJUST FOR MAXIMUM STRESS
    // ====================================================================
    public static class DiskConfig
    {
        public const int NUM_THREADS = 12;                    // Number of concurrent I/O threads
        public const int OPERATIONS_PER_THREAD = 100;         // Operations each thread performs
        public const int NUM_FILES = 500;                     // Total number of files to create
        public const int MIN_FILE_SIZE_KB = 50;               // Minimum file size in KB
        public const int MAX_FILE_SIZE_KB = 200;              // Maximum file size in KB
        public const int WRITE_CHUNK_SIZE = 1024;             // Write chunk size in bytes
        public const int READ_BUFFER_SIZE = 4096;             // Read buffer size in bytes
        public const int RANDOM_SEEK_OPERATIONS = 1000;       // Number of random seek operations
        public const int SEQUENTIAL_OPERATIONS = 500;         // Number of sequential operations
        public const int DELAY_BETWEEN_OPS_MICROSECONDS = 10; // Very small delay for maximum stress
        public const string BASE_DIRECTORY = "disk_stress_test/";
        public const string BASE_FILENAME = "stress_file_";
        public const string LOG_FILE = "disk_scheduling_performance.log";
        public const bool ENABLE_RANDOM_SEEKS = true;         // Enable random disk seeks
        public const bool ENABLE_SEQUENTIAL_ACCESS = true;    // Enable sequential access patterns
        public const bool ENABLE_FRAGMENTATION = true;        // Create fragmented file patterns
        public const bool ENABLE_CONCURRENT_ACCESS = true;    // Multiple threads accessing same files
    }
    // ====================================================================

    public class IORequest
    {
        public int ThreadId { get; set; }
        public string Filename { get; set; }
        public long Position { get; set; }
        public int Size { get; set; }
        public bool IsWrite { get; set; }
        public DateTime Timestamp { get; set; }
    }

    class IntensiveDiskSchedulingDemo
    {
        private long totalBytesWritten = 0;
        private long totalBytesRead = 0;
        private long totalOperations = 0;
        private long errorCount = 0;
        private long seekOperations = 0;
        
        private readonly object logLock = new object();
        private readonly Stopwatch stopwatch;
        private readonly Random random;
        private readonly ConcurrentBag<string> createdFiles;
        private readonly ConcurrentQueue<IORequest> ioQueue;
        private volatile bool userStopped = false;

        public IntensiveDiskSchedulingDemo()
        {
            stopwatch = Stopwatch.StartNew();
            random = new Random();
            createdFiles = new ConcurrentBag<string>();
            ioQueue = new ConcurrentQueue<IORequest>();
            
            // Create base directory
            Directory.CreateDirectory(DiskConfig.BASE_DIRECTORY);
            
            // Clear log file
            using (var writer = new StreamWriter(DiskConfig.LOG_FILE, false))
            {
                writer.WriteLine("=== INTENSIVE DISK SCHEDULING PERFORMANCE LOG ===");
            }
            
            LogPerformance("Intensive Disk Scheduling Demo initialized");
        }

        private void LogPerformance(string message)
        {
            lock (logLock)
            {
                try
                {
                    using (var writer = new StreamWriter(DiskConfig.LOG_FILE, append: true))
                    {
                        writer.WriteLine($"[{DateTimeOffset.Now.ToUnixTimeMilliseconds()}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }

        private string GenerateIntensiveContent(int sizeKB, int threadId, int operation)
        {
            var sb = new StringBuilder();
            
            // Header with metadata
            sb.AppendLine("=== INTENSIVE DISK SCHEDULING TEST DATA ===");
            sb.AppendLine($"Thread: {threadId} | Operation: {operation}");
            sb.AppendLine($"Timestamp: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            sb.AppendLine($"Target Size: {sizeKB} KB");
            sb.AppendLine(new string('=', 60));
            
            int currentSize = Encoding.UTF8.GetByteCount(sb.ToString());
            int targetSize = sizeKB * 1024;
            int remainingSize = Math.Max(0, targetSize - currentSize);
            
            // Fill with intensive random data patterns
            for (int i = 0; i < remainingSize; i++)
            {
                if (i % 100 == 99)
                {
                    sb.Append('\n');
                }
                else if (i % 10 == 9)
                {
                    sb.Append(' ');
                }
                else
                {
                    sb.Append((char)random.Next(32, 127)); // Printable ASCII
                }
            }
            
            return sb.ToString();
        }

        // Simulate random disk seeks (worst case for mechanical drives)
        private async Task PerformRandomSeekOperationsAsync(int threadId)
        {
            for (int op = 0; op < DiskConfig.RANDOM_SEEK_OPERATIONS && !userStopped; op++)
            {
                try
                {
                    // Create random file access pattern
                    int fileIndex = random.Next(0, DiskConfig.NUM_FILES);
                    string filename = Path.Combine(DiskConfig.BASE_DIRECTORY, $"{DiskConfig.BASE_FILENAME}{fileIndex}.dat");
                    
                    int fileSize = random.Next(DiskConfig.MIN_FILE_SIZE_KB, DiskConfig.MAX_FILE_SIZE_KB + 1);
                    string content = GenerateIntensiveContent(fileSize, threadId, op);
                    byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                    
                    // INTENSIVE WRITE with random seeks
                    using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, DiskConfig.WRITE_CHUNK_SIZE, useAsync: true))
                    {
                        // Write in random chunks to simulate disk head movement
                        int bytesWritten = 0;
                        while (bytesWritten < contentBytes.Length && !userStopped)
                        {
                            int chunkSize = Math.Min(DiskConfig.WRITE_CHUNK_SIZE, contentBytes.Length - bytesWritten);
                            
                            // Random seek within file
                            long seekPos = random.Next(0, Math.Max(1, contentBytes.Length - chunkSize));
                            fileStream.Seek(seekPos, SeekOrigin.Begin);
                            
                            await fileStream.WriteAsync(contentBytes, bytesWritten, chunkSize);
                            await fileStream.FlushAsync();
                            
                            bytesWritten += chunkSize;
                            Interlocked.Add(ref totalBytesWritten, chunkSize);
                            Interlocked.Increment(ref seekOperations);
                            
                            // Micro delay to allow other threads to compete
                            if (DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS > 0)
                            {
                                await Task.Delay(TimeSpan.FromMicroseconds(DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS));
                            }
                        }
                    }
                    
                    // Add to created files list for later access
                    createdFiles.Add(filename);
                    
                    Console.WriteLine($"[THREAD {threadId}] RANDOM WRITE: {Path.GetFileName(filename)} ({fileSize} KB) - Seek Op {op}");
                    
                    Interlocked.Increment(ref totalOperations);
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    LogPerformance($"ERROR in random seek operation: {ex.Message}");
                }
            }
        }

        // Simulate sequential access patterns (best case scenario)
        private async Task PerformSequentialOperationsAsync(int threadId)
        {
            for (int op = 0; op < DiskConfig.SEQUENTIAL_OPERATIONS && !userStopped; op++)
            {
                try
                {
                    string filename = Path.Combine(DiskConfig.BASE_DIRECTORY, $"sequential_{threadId}_{op}.seq");
                    
                    // Create large sequential file
                    string content = GenerateIntensiveContent(DiskConfig.MAX_FILE_SIZE_KB, threadId, op);
                    byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                    
                    // INTENSIVE SEQUENTIAL WRITE
                    using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, DiskConfig.WRITE_CHUNK_SIZE, useAsync: true))
                    {
                        // Write in large sequential chunks
                        for (int pos = 0; pos < contentBytes.Length && !userStopped; pos += DiskConfig.WRITE_CHUNK_SIZE)
                        {
                            int chunkSize = Math.Min(DiskConfig.WRITE_CHUNK_SIZE, contentBytes.Length - pos);
                            await fileStream.WriteAsync(contentBytes, pos, chunkSize);
                            await fileStream.FlushAsync();
                            
                            Interlocked.Add(ref totalBytesWritten, chunkSize);
                            
                            // Very small delay to maintain intensity
                            if (DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS > 0)
                            {
                                await Task.Delay(TimeSpan.FromMicroseconds(DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS / 2));
                            }
                        }
                    }
                    
                    // Immediately read back sequentially
                    using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, DiskConfig.READ_BUFFER_SIZE, useAsync: true))
                    {
                        byte[] buffer = new byte[DiskConfig.READ_BUFFER_SIZE];
                        int bytesRead;
                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0 && !userStopped)
                        {
                            Interlocked.Add(ref totalBytesRead, bytesRead);
                        }
                    }
                    
                    Console.WriteLine($"[THREAD {threadId}] SEQUENTIAL: {Path.GetFileName(filename)} ({DiskConfig.MAX_FILE_SIZE_KB} KB) - Op {op}");
                    
                    Interlocked.Increment(ref totalOperations);
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    LogPerformance($"ERROR in sequential operation: {ex.Message}");
                }
            }
        }

        // Create fragmented file access patterns
        private async Task PerformFragmentationOperationsAsync(int threadId)
        {
            for (int op = 0; op < DiskConfig.OPERATIONS_PER_THREAD && !userStopped; op++)
            {
                try
                {
                    // Create multiple small files simultaneously
                    var fragmentTasks = new List<Task>();
                    
                    for (int fragment = 0; fragment < 5; fragment++)
                    {
                        int fragIndex = fragment; // Capture for closure
                        fragmentTasks.Add(Task.Run(async () =>
                        {
                            string filename = Path.Combine(DiskConfig.BASE_DIRECTORY, $"fragment_{threadId}_{op}_{fragIndex}.frag");
                            
                            int fileSize = random.Next(DiskConfig.MIN_FILE_SIZE_KB, DiskConfig.MIN_FILE_SIZE_KB + 20);
                            string content = GenerateIntensiveContent(fileSize, threadId, op);
                            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                            
                            // Write fragmented data
                            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                // Write in small, random-sized chunks to increase fragmentation
                                int pos = 0;
                                while (pos < contentBytes.Length && !userStopped)
                                {
                                    int chunkSize = Math.Min(random.Next(100, 501), contentBytes.Length - pos);
                                    
                                    await fileStream.WriteAsync(contentBytes, pos, chunkSize);
                                    await fileStream.FlushAsync();
                                    
                                    pos += chunkSize;
                                    Interlocked.Add(ref totalBytesWritten, chunkSize);
                                    
                                    // Small delay to allow disk head movement
                                    if (DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS > 0)
                                    {
                                        await Task.Delay(TimeSpan.FromMicroseconds(DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS));
                                    }
                                }
                            }
                        }));
                    }
                    
                    await Task.WhenAll(fragmentTasks);
                    
                    Console.WriteLine($"[THREAD {threadId}] FRAGMENTATION: Created 5 fragments - Op {op}");
                    Interlocked.Increment(ref totalOperations);
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    LogPerformance($"ERROR in fragmentation operation: {ex.Message}");
                }
            }
        }

        // Concurrent access to same files (causes disk scheduling conflicts)
        private async Task PerformConcurrentAccessOperationsAsync(int threadId)
        {
            for (int op = 0; op < DiskConfig.OPERATIONS_PER_THREAD && !userStopped; op++)
            {
                try
                {
                    // Multiple threads accessing the same files
                    string sharedFilename = Path.Combine(DiskConfig.BASE_DIRECTORY, $"shared_access_{op % 10}.shared");
                    
                    if (op % 2 == 0)
                    {
                        // Writer thread
                        string content = GenerateIntensiveContent(DiskConfig.MIN_FILE_SIZE_KB, threadId, op);
                        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                        
                        using (var fileStream = new FileStream(sharedFilename, FileMode.Append, FileAccess.Write, FileShare.Read))
                        {
                            await fileStream.WriteAsync(contentBytes, 0, contentBytes.Length);
                            await fileStream.FlushAsync();
                            Interlocked.Add(ref totalBytesWritten, contentBytes.Length);
                        }
                        Console.WriteLine($"[THREAD {threadId}] CONCURRENT WRITE: {Path.GetFileName(sharedFilename)} - Op {op}");
                    }
                    else
                    {
                        // Reader thread
                        if (File.Exists(sharedFilename))
                        {
                            using (var fileStream = new FileStream(sharedFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                byte[] buffer = new byte[DiskConfig.READ_BUFFER_SIZE];
                                int bytesRead;
                                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    Interlocked.Add(ref totalBytesRead, bytesRead);
                                }
                            }
                        }
                        Console.WriteLine($"[THREAD {threadId}] CONCURRENT READ: {Path.GetFileName(sharedFilename)} - Op {op}");
                    }
                    
                    Interlocked.Increment(ref totalOperations);
                    
                    // Very small delay to maintain maximum stress
                    if (DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS > 0)
                    {
                        await Task.Delay(TimeSpan.FromMicroseconds(DiskConfig.DELAY_BETWEEN_OPS_MICROSECONDS));
                    }
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    LogPerformance($"ERROR in concurrent access operation: {ex.Message}");
                }
            }
        }

        public async Task RunIntensiveDiskSchedulingDemoAsync()
        {
            Console.WriteLine("=== INTENSIVE DISK SCHEDULING DEMONSTRATION (C#) ===");
            Console.WriteLine("This program will STRESS TEST your disk subsystem with:");
            Console.WriteLine("1. Random seek operations (worst case for mechanical drives)");
            Console.WriteLine("2. Sequential access patterns (best case scenario)");
            Console.WriteLine("3. File fragmentation simulation");
            Console.WriteLine("4. Concurrent file access conflicts");
            Console.WriteLine("5. Intensive I/O operations");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("INTENSITY PARAMETERS:");
            Console.WriteLine($"- Threads: {DiskConfig.NUM_THREADS}");
            Console.WriteLine($"- Operations per thread: {DiskConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine($"- Total files to create: {DiskConfig.NUM_FILES}");
            Console.WriteLine($"- Random seek operations: {DiskConfig.RANDOM_SEEK_OPERATIONS}");
            Console.WriteLine($"- Sequential operations: {DiskConfig.SEQUENTIAL_OPERATIONS}");
            Console.WriteLine($"- File size range: {DiskConfig.MIN_FILE_SIZE_KB}-{DiskConfig.MAX_FILE_SIZE_KB} KB");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("WARNING: This will create intense disk activity!");
            Console.WriteLine("Press any key to stop the demonstration...");
            Console.WriteLine(new string('-', 70));

            // Start monitoring for user input
            var keyTask = Task.Run(() =>
            {
                Console.ReadKey(true);
                userStopped = true;
                Console.WriteLine("\n>>> User requested stop. Finishing current operations...");
            });

            // Launch intensive disk operations
            var tasks = new List<Task>();
            
            for (int i = 0; i < DiskConfig.NUM_THREADS; i++)
            {
                int threadId = i; // Capture for closure
                tasks.Add(Task.Run(async () =>
                {
                    Console.WriteLine($"  Task {threadId} started - INTENSIVE DISK OPERATIONS");
                    
                    while (!userStopped)
                    {
                        if (DiskConfig.ENABLE_RANDOM_SEEKS && !userStopped)
                        {
                            await PerformRandomSeekOperationsAsync(threadId);
                        }
                        
                        if (DiskConfig.ENABLE_SEQUENTIAL_ACCESS && !userStopped)
                        {
                            await PerformSequentialOperationsAsync(threadId);
                        }
                        
                        if (DiskConfig.ENABLE_FRAGMENTATION && !userStopped)
                        {
                            await PerformFragmentationOperationsAsync(threadId);
                        }
                        
                        if (DiskConfig.ENABLE_CONCURRENT_ACCESS && !userStopped)
                        {
                            await PerformConcurrentAccessOperationsAsync(threadId);
                        }
                        
                        // Brief pause before next intensive cycle
                        await Task.Delay(100);
                    }
                    
                    Console.WriteLine($"  Task {threadId} completed intensive operations");
                }));
            }

            // Performance monitoring task
            var perfTask = Task.Run(async () =>
            {
                while (!userStopped)
                {
                    await Task.Delay(5000);
                    if (!userStopped)
                    {
                        DisplayRealTimePerformance();
                    }
                }
            });

            // Wait for user to stop
            await keyTask;
            userStopped = true;

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            await perfTask;

            DisplayFinalResults();

            Console.WriteLine("\nIntensive disk scheduling demonstration completed.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private void DisplayRealTimePerformance()
        {
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"\n{new string('=', 50)}");
            Console.WriteLine("REAL-TIME DISK PERFORMANCE");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Running time: {elapsedSeconds:F1} seconds");
            Console.WriteLine($"Total operations: {Interlocked.Read(ref totalOperations):N0}");
            Console.WriteLine($"Seek operations: {Interlocked.Read(ref seekOperations):N0}");
            Console.WriteLine($"Data written: {Interlocked.Read(ref totalBytesWritten) / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Data read: {Interlocked.Read(ref totalBytesRead) / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Write throughput: {(Interlocked.Read(ref totalBytesWritten) / 1024.0 / 1024.0 / elapsedSeconds):F2} MB/s");
            Console.WriteLine($"Read throughput: {(Interlocked.Read(ref totalBytesRead) / 1024.0 / 1024.0 / elapsedSeconds):F2} MB/s");
            Console.WriteLine($"Operations/sec: {(Interlocked.Read(ref totalOperations) / elapsedSeconds):F2}");
            Console.WriteLine($"Errors: {Interlocked.Read(ref errorCount):N0}");
            Console.WriteLine(new string('=', 50));
            
            LogPerformance($"Real-time stats - Ops: {Interlocked.Read(ref totalOperations)}, " +
                          $"Write: {Interlocked.Read(ref totalBytesWritten) / 1024 / 1024}MB, " +
                          $"Read: {Interlocked.Read(ref totalBytesRead) / 1024 / 1024}MB");
        }

        private void DisplayFinalResults()
        {
            stopwatch.Stop();
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"\n{new string('=', 70)}");
            Console.WriteLine("FINAL INTENSIVE DISK SCHEDULING RESULTS");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.WriteLine($"Total threads used: {DiskConfig.NUM_THREADS}");
            Console.WriteLine($"Total operations completed: {Interlocked.Read(ref totalOperations):N0}");
            Console.WriteLine($"Total seek operations: {Interlocked.Read(ref seekOperations):N0}");
            Console.WriteLine($"Total bytes written: {Interlocked.Read(ref totalBytesWritten) / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Total bytes read: {Interlocked.Read(ref totalBytesRead) / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Average write throughput: {(Interlocked.Read(ref totalBytesWritten) / 1024.0 / 1024.0 / elapsedSeconds):F2} MB/s");
            Console.WriteLine($"Average read throughput: {(Interlocked.Read(ref totalBytesRead) / 1024.0 / 1024.0 / elapsedSeconds):F2} MB/s");
            Console.WriteLine($"Operations per second: {(Interlocked.Read(ref totalOperations) / elapsedSeconds):F2}");
            Console.WriteLine($"Total errors encountered: {Interlocked.Read(ref errorCount):N0}");
            Console.WriteLine($"Files created: {createdFiles.Count:N0}");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("DISK SCHEDULING ANALYSIS:");
            Console.WriteLine("- Random seeks simulate worst-case disk head movement");
            Console.WriteLine("- Sequential operations show optimal disk performance");
            Console.WriteLine("- Fragmentation demonstrates real-world disk usage patterns");
            Console.WriteLine("- Concurrent access shows scheduling algorithm effectiveness");
            Console.WriteLine($"- Check {DiskConfig.LOG_FILE} for detailed performance metrics");
            Console.WriteLine(new string('=', 70));
            
            LogPerformance($"Final results - Duration: {stopwatch.ElapsedMilliseconds}ms, " +
                          $"Ops: {Interlocked.Read(ref totalOperations)}, " +
                          $"Errors: {Interlocked.Read(ref errorCount)}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demo = new IntensiveDiskSchedulingDemo();
                await demo.RunIntensiveDiskSchedulingDemoAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }
    }
}
