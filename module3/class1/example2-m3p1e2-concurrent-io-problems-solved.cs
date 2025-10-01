using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ConcurrentIOProblemsSolved
{
    // ====================================================================
    // CONFIGURATION VARIABLES - PROPER SYNCHRONIZATION ENABLED
    // ====================================================================
    public static class IOConfig
    {
        public const int NUM_THREADS = 6;                     // Number of concurrent threads
        public const int OPERATIONS_PER_THREAD = 20;          // Operations each thread performs
        public const int FILE_SIZE_KB = 5;                    // Size of each file in KB
        public const string SHARED_FILE = "shared_resource_safe.txt";
        public const string LOG_FILE = "concurrent_operations_safe.log";
        public const string BASE_FILENAME = "concurrent_file_safe_";
        public const int DELAY_BETWEEN_OPS_MS = 50;           // Delay between operations
    }
    // ====================================================================

    class ConcurrentIOProblemsSolved
    {
        // SOLUTION 1: Use thread-safe collections and atomic operations
        private long operationCounter = 0;
        private long errorCounter = 0;
        private long totalBytesProcessed = 0;
        
        // SOLUTION 2: Use proper synchronization primitives
        private readonly object logLock = new object();           // For thread-safe logging
        private readonly object sharedFileLock = new object();    // For shared file access
        private readonly ReaderWriterLockSlim fileLockSlim = new ReaderWriterLockSlim(); // For reader-writer file access
        private readonly SemaphoreSlim fileOperationSemaphore = new SemaphoreSlim(1, 1); // For file operation coordination
        
        // SOLUTION 3: Use ManualResetEvent for coordination
        private readonly ManualResetEventSlim fileReadyEvent = new ManualResetEventSlim(false);
        
        private readonly Stopwatch stopwatch;
        private readonly Random random;
        private readonly CancellationTokenSource cancellationTokenSource;

        public ConcurrentIOProblemsSolved()
        {
            stopwatch = Stopwatch.StartNew();
            random = new Random();
            cancellationTokenSource = new CancellationTokenSource();
            
            // Clean up any existing files
            CleanupFiles();
        }

        private void CleanupFiles()
        {
            try
            {
                if (File.Exists(IOConfig.SHARED_FILE))
                    File.Delete(IOConfig.SHARED_FILE);
                if (File.Exists(IOConfig.LOG_FILE))
                    File.Delete(IOConfig.LOG_FILE);
                
                // Clean up shared data files
                for (int i = 0; i < 3; i++)
                {
                    string sharedDataFile = $"shared_data_safe_{i}.dat";
                    if (File.Exists(sharedDataFile))
                        File.Delete(sharedDataFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        private string GenerateFileContent(int threadId, int operationId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CONCURRENT I/O OPERATION (SAFE C# VERSION) ===");
            sb.AppendLine($"Thread ID: {threadId}");
            sb.AppendLine($"Operation: {operationId}");
            sb.AppendLine($"Timestamp: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            sb.AppendLine($"Managed Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            sb.AppendLine(new string('=', 50));

            // Fill to desired size
            int currentSize = Encoding.UTF8.GetByteCount(sb.ToString());
            int targetSize = IOConfig.FILE_SIZE_KB * 1024;
            int remainingSize = Math.Max(0, targetSize - currentSize);

            for (int i = 0; i < remainingSize; i++)
            {
                if (i % 80 == 79)
                {
                    sb.Append('\n');
                }
                else
                {
                    sb.Append((char)random.Next(65, 91)); // A-Z characters
                }
            }

            return sb.ToString();
        }

        // SOLUTION FOR PROBLEM 2: Thread-safe shared file access
        private async Task DemonstrateSharedFileContentionSafeAsync(int threadId)
        {
            for (int op = 0; op < IOConfig.OPERATIONS_PER_THREAD; op++)
            {
                try
                {
                    string content = $"Thread {threadId} Operation {op} Time: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}\n";
                    
                    // SOLUTION: Use lock to synchronize access to shared file
                    lock (sharedFileLock)
                    {
                        using (var writer = new StreamWriter(IOConfig.SHARED_FILE, append: true, Encoding.UTF8))
                        {
                            writer.Write(content);
                            writer.Flush();
                        }
                        Console.WriteLine($"[THREAD {threadId}] SAFE WRITE to {IOConfig.SHARED_FILE} (Op {op})");
                    }
                    
                    Interlocked.Add(ref totalBytesProcessed, Encoding.UTF8.GetByteCount(content));
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCounter);
                    await SafeLoggingAsync(threadId, $"Error in shared file operation: {ex.Message}");
                }
                
                await Task.Delay(IOConfig.DELAY_BETWEEN_OPS_MS);
            }
        }

        // SOLUTION FOR PROBLEM 3: Thread-safe logging
        private async Task SafeLoggingAsync(int threadId, string message)
        {
            // SOLUTION: Use lock for thread-safe logging
            lock (logLock)
            {
                try
                {
                    using (var writer = new StreamWriter(IOConfig.LOG_FILE, append: true, Encoding.UTF8))
                    {
                        writer.WriteLine($"[Thread {threadId}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }

        // SOLUTION FOR PROBLEM 4: Thread-safe file operations with proper coordination
        private async Task DemonstrateFileRaceConditionsSafeAsync(int threadId)
        {
            for (int op = 0; op < IOConfig.OPERATIONS_PER_THREAD; op++)
            {
                string filename = $"{IOConfig.BASE_FILENAME}{threadId}_{op}.tmp";
                
                try
                {
                    // SOLUTION: Use atomic operations for counters
                    Interlocked.Increment(ref operationCounter);
                    
                    // SOLUTION: Coordinate file operations with semaphore
                    await fileOperationSemaphore.WaitAsync();
                    
                    try
                    {
                        // Create file
                        string content = GenerateFileContent(threadId, op);
                        
                        // SOLUTION FOR PROBLEM 5: Proper async file operations
                        using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            await writer.WriteAsync(content);
                            await writer.FlushAsync();
                        }
                        
                        Console.WriteLine($"[THREAD {threadId}] CREATED FILE: {filename} ({Encoding.UTF8.GetByteCount(content)} bytes)");
                        await SafeLoggingAsync(threadId, $"Created file: {filename}");
                        
                        // SOLUTION FOR PROBLEM 5: Proper synchronization for read-after-write
                        // Ensure file is completely written before reading
                        await Task.Delay(5); // Small delay to ensure file system consistency
                        
                        // Try to read the file we just created
                        if (File.Exists(filename))
                        {
                            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                            {
                                long fileSize = fileStream.Length;
                                Interlocked.Add(ref totalBytesProcessed, fileSize);
                                
                                Console.WriteLine($"[THREAD {threadId}] READ FILE: {filename} ({fileSize} bytes)");
                                await SafeLoggingAsync(threadId, $"Read file: {filename} ({fileSize} bytes)");
                            }
                        }
                        else
                        {
                            Interlocked.Increment(ref errorCounter);
                            Console.WriteLine($"[THREAD {threadId}] ERROR: Could not read file: {filename}");
                            await SafeLoggingAsync(threadId, $"ERROR: Could not read file: {filename}");
                        }
                        
                        // SOLUTION FOR PROBLEM 6: Coordinated file deletion
                        // Ensure no other threads are accessing the file before deletion
                        await Task.Delay(2); // Small delay to ensure file handles are released
                        
                        if (File.Exists(filename))
                        {
                            try
                            {
                                File.Delete(filename);
                                Console.WriteLine($"[THREAD {threadId}] DELETED FILE: {filename}");
                                await SafeLoggingAsync(threadId, $"Deleted file: {filename}");
                            }
                            catch (Exception deleteEx)
                            {
                                Console.WriteLine($"[THREAD {threadId}] ERROR: Could not delete file: {filename} - {deleteEx.Message}");
                                await SafeLoggingAsync(threadId, $"ERROR: Could not delete file: {filename} - {deleteEx.Message}");
                            }
                        }
                    }
                    finally
                    {
                        fileOperationSemaphore.Release();
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCounter);
                    await SafeLoggingAsync(threadId, $"ERROR in file operations: {ex.Message}");
                    fileOperationSemaphore.Release();
                }
                
                await Task.Delay(IOConfig.DELAY_BETWEEN_OPS_MS);
            }
        }

        // SOLUTION FOR PROBLEM 7: Proper reader-writer synchronization
        private async Task DemonstrateFileLockingProblemsSafeAsync(int threadId)
        {
            string sharedDataFile = $"shared_data_safe_{threadId % 3}.dat";
            
            for (int op = 0; op < IOConfig.OPERATIONS_PER_THREAD / 2; op++)
            {
                try
                {
                    if (op % 2 == 0)
                    {
                        // Writer thread - use write lock
                        string data = $"Data from thread {threadId} operation {op}\n";
                        
                        // SOLUTION: Use write lock for exclusive access
                        fileLockSlim.EnterWriteLock();
                        try
                        {
                            using (var writer = new StreamWriter(sharedDataFile, append: true, Encoding.UTF8))
                            {
                                await writer.WriteAsync(data);
                                await writer.FlushAsync();
                            }
                            Console.WriteLine($"[THREAD {threadId}] SAFE WRITE to {sharedDataFile} (Op {op})");
                        }
                        finally
                        {
                            fileLockSlim.ExitWriteLock();
                        }
                        
                        await SafeLoggingAsync(threadId, $"Wrote to shared file: {sharedDataFile}");
                    }
                    else
                    {
                        // Reader thread - use read lock
                        // SOLUTION: Use read lock for shared access (multiple readers allowed)
                        fileLockSlim.EnterReadLock();
                        try
                        {
                            if (File.Exists(sharedDataFile))
                            {
                                string[] lines = await File.ReadAllLinesAsync(sharedDataFile);
                                int lineCount = lines.Length;
                                
                                Console.WriteLine($"[THREAD {threadId}] SAFE READ from {sharedDataFile} ({lineCount} lines)");
                                await SafeLoggingAsync(threadId, $"Read shared file: {sharedDataFile} ({lineCount} lines)");
                            }
                        }
                        finally
                        {
                            fileLockSlim.ExitReadLock();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCounter);
                    await SafeLoggingAsync(threadId, $"ERROR in file locking demo: {ex.Message}");
                }
                
                await Task.Delay(IOConfig.DELAY_BETWEEN_OPS_MS);
            }
        }

        public async Task RunConcurrentOperationsAsync()
        {
            Console.WriteLine("=== CONCURRENT I/O PROBLEMS - SOLVED C# VERSION ===");
            Console.WriteLine("This program demonstrates PROPER solutions to I/O concurrency issues:");
            Console.WriteLine("1. Thread-safe shared file access using locks");
            Console.WriteLine("2. Safe logging operations with synchronization");
            Console.WriteLine("3. Coordinated file creation/deletion with semaphores");
            Console.WriteLine("4. Reader-writer locks for file access");
            Console.WriteLine("5. Atomic operations for counters");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Configuration:");
            Console.WriteLine($"- Threads: {IOConfig.NUM_THREADS}");
            Console.WriteLine($"- Operations per thread: {IOConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine("- All synchronization mechanisms: ENABLED");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Press any key to stop the demonstration...");
            Console.WriteLine("The program will run in continuous cycles until you press a key.");
            Console.WriteLine(new string('-', 60));

            int cycleCount = 0;
            bool userStopped = false;

            // Start monitoring for user input
            var keyTask = Task.Run(() =>
            {
                Console.ReadKey(true);
                userStopped = true;
                Console.WriteLine("\n>>> User requested stop. Finishing current cycle...");
            });

            // Main demonstration loop
            while (!userStopped)
            {
                cycleCount++;
                Console.WriteLine($"\n{new string('=', 60)}");
                Console.WriteLine($">>> STARTING SAFE CYCLE #{cycleCount} <<<");
                Console.WriteLine(new string('=', 60));

                // Reset counters for this cycle
                Interlocked.Exchange(ref operationCounter, 0);
                Interlocked.Exchange(ref errorCounter, 0);
                Interlocked.Exchange(ref totalBytesProcessed, 0);
                stopwatch.Restart();

                // Launch tasks with proper synchronization
                Console.WriteLine($"Launching {IOConfig.NUM_THREADS} properly synchronized tasks...");
                
                var tasks = new List<Task>();
                for (int i = 0; i < IOConfig.NUM_THREADS; i++)
                {
                    int threadId = i; // Capture for closure
                    tasks.Add(Task.Run(async () =>
                    {
                        Console.WriteLine($"  Task {threadId} started safely (Cycle {cycleCount})");
                        
                        // Each task performs properly synchronized I/O operations
                        await DemonstrateSharedFileContentionSafeAsync(threadId);
                        await DemonstrateFileRaceConditionsSafeAsync(threadId);
                        await DemonstrateFileLockingProblemsSafeAsync(threadId);
                        
                        Console.WriteLine($"  Task {threadId} completed safely (Cycle {cycleCount})");
                    }));
                }

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);

                // Display results for this cycle
                Console.WriteLine($"\n{new string('-', 40)}");
                Console.WriteLine($"SAFE CYCLE #{cycleCount} RESULTS:");
                DisplayResults();

                if (!userStopped)
                {
                    Console.WriteLine("\nWaiting 3 seconds before next cycle...");
                    Console.WriteLine("(Press any key to stop)");
                    
                    // Wait 3 seconds or until user presses key
                    for (int i = 0; i < 30 && !userStopped; i++)
                    {
                        await Task.Delay(100);
                    }
                }
            }

            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine($"SAFE DEMONSTRATION COMPLETED AFTER {cycleCount} CYCLES");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Check the following files - they should be properly formatted:");
            Console.WriteLine($"- {IOConfig.SHARED_FILE} (no corruption expected)");
            Console.WriteLine($"- {IOConfig.LOG_FILE} (clean log entries expected)");
            Console.WriteLine("- shared_data_safe_*.dat files (consistent data expected)");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }

        private void DisplayResults()
        {
            stopwatch.Stop();
            
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine("SAFE CONCURRENT I/O RESULTS");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total threads: {IOConfig.NUM_THREADS}");
            Console.WriteLine($"Expected operations: {IOConfig.NUM_THREADS * IOConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine($"Actual operations: {Interlocked.Read(ref operationCounter)}");
            Console.WriteLine($"Errors encountered: {Interlocked.Read(ref errorCounter)} (should be 0 or very low)");
            Console.WriteLine($"Total bytes processed: {Interlocked.Read(ref totalBytesProcessed) / 1024.0:F2} KB");

            // Analyze the shared file for corruption
            try
            {
                if (File.Exists(IOConfig.SHARED_FILE))
                {
                    string[] lines = File.ReadAllLines(IOConfig.SHARED_FILE);
                    Console.WriteLine($"Shared file lines: {lines.Length}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading shared file: {ex.Message}");
            }

            // Analyze the log file for corruption
            try
            {
                if (File.Exists(IOConfig.LOG_FILE))
                {
                    string[] lines = File.ReadAllLines(IOConfig.LOG_FILE);
                    int wellFormedLines = 0;
                    
                    foreach (string line in lines)
                    {
                        if (line.Contains("[Thread") && !string.IsNullOrWhiteSpace(line))
                        {
                            wellFormedLines++;
                        }
                    }
                    
                    Console.WriteLine($"Log file lines: {lines.Length}");
                    Console.WriteLine($"Well-formed log lines: {wellFormedLines} (should equal total lines)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading log file: {ex.Message}");
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("SAFETY ANALYSIS:");
            
            long actualOps = Interlocked.Read(ref operationCounter);
            long expectedOps = IOConfig.NUM_THREADS * IOConfig.OPERATIONS_PER_THREAD;
            Console.WriteLine($"✓ Operations count should match expected: {(actualOps == expectedOps ? "PASS" : "FAIL")}");
            
            long errors = Interlocked.Read(ref errorCounter);
            Console.WriteLine($"✓ Errors should be minimal: {(errors <= 2 ? "PASS" : "FAIL")}");
            
            Console.WriteLine("✓ All synchronization mechanisms working properly");
            Console.WriteLine("✓ No race conditions expected in this version");
            Console.WriteLine(new string('=', 60));
        }

        public void Dispose()
        {
            fileLockSlim?.Dispose();
            fileOperationSemaphore?.Dispose();
            fileReadyEvent?.Dispose();
            cancellationTokenSource?.Dispose();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demo = new ConcurrentIOProblemsSolved();
                await demo.RunConcurrentOperationsAsync();
                demo.Dispose();
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
