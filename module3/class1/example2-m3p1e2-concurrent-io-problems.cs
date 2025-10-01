using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConcurrentIOProblems
{
    // ====================================================================
    // CONFIGURATION VARIABLES - PROBLEMS DEMONSTRATION
    // ====================================================================
    public static class IOConfig
    {
        public const int NUM_THREADS = 6;                     // Number of concurrent threads (reduced for better visibility)
        public const int OPERATIONS_PER_THREAD = 20;          // Operations each thread performs (reduced for better visibility)
        public const int FILE_SIZE_KB = 5;                    // Size of each file in KB (reduced for faster operations)
        public const string SHARED_FILE = "shared_resource.txt";
        public const string LOG_FILE = "concurrent_operations.log";
        public const string BASE_FILENAME = "concurrent_file_";
        public const int DELAY_BETWEEN_OPS_MS = 50;           // Delay between operations (increased for better visibility)
        public const bool ENABLE_PROPER_SYNCHRONIZATION = false; // Toggle to show solutions
    }
    // ====================================================================

    class ConcurrentIOProblems
    {
        // PROBLEM 1: Race condition in shared counter (without proper synchronization)
        private int unsafeCounter = 0;  // This will demonstrate race conditions
        private int operationCounter = 0; // Non-atomic counter
        private int errorCounter = 0;
        private long totalBytesProcessed = 0;
        
        // PROBLEM: No synchronization primitives when ENABLE_PROPER_SYNCHRONIZATION is false
        private readonly object logLock = new object();           // Only used when synchronization is enabled
        private readonly object sharedFileLock = new object();    // Only used when synchronization is enabled
        
        private readonly Stopwatch stopwatch;
        private readonly Random random;

        public ConcurrentIOProblems()
        {
            stopwatch = Stopwatch.StartNew();
            random = new Random();
            
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
                    string sharedDataFile = $"shared_data_{i}.dat";
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
            sb.AppendLine("=== CONCURRENT I/O OPERATION (PROBLEMATIC C# VERSION) ===");
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

        // PROBLEM 2: Multiple threads writing to the same file without coordination
        private async Task DemonstrateSharedFileContentionAsync(int threadId)
        {
            for (int op = 0; op < IOConfig.OPERATIONS_PER_THREAD; op++)
            {
                try
                {
                    // PROBLEMATIC: Multiple threads trying to write to same file
                    // This can cause:
                    // - Data corruption
                    // - Partial writes
                    // - File access violations
                    // - Inconsistent file state
                    
                    string content = $"Thread {threadId} Operation {op} Time: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}\n";
                    
                    if (IOConfig.ENABLE_PROPER_SYNCHRONIZATION)
                    {
                        // SOLUTION: Use lock to synchronize access
                        lock (sharedFileLock)
                        {
                            using (var writer = new StreamWriter(IOConfig.SHARED_FILE, append: true))
                            {
                                writer.Write(content);
                                writer.Flush();
                            }
                            Console.WriteLine($"[THREAD {threadId}] SAFE WRITE to {IOConfig.SHARED_FILE} (Op {op})");
                        }
                    }
                    else
                    {
                        // PROBLEM: No synchronization - multiple threads compete
                        using (var writer = new StreamWriter(IOConfig.SHARED_FILE, append: true))
                        {
                            // Simulate some processing time to increase chance of conflicts
                            await Task.Delay(1);
                            writer.Write(content);
                            writer.Flush();
                        }
                        Console.WriteLine($"[THREAD {threadId}] UNSAFE WRITE to {IOConfig.SHARED_FILE} (Op {op}) - RACE CONDITION POSSIBLE!");
                    }
                    
                    totalBytesProcessed += Encoding.UTF8.GetByteCount(content);
                    
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    Console.WriteLine($"Thread {threadId} error in shared file operation: {ex.Message}");
                }
                
                await Task.Delay(IOConfig.DELAY_BETWEEN_OPS_MS);
            }
        }

        // PROBLEM 3: Race conditions in logging operations
        private void UnsafeLogging(int threadId, string message)
        {
            // PROBLEMATIC: Multiple threads writing to log without synchronization
            // This can cause:
            // - Interleaved log messages
            // - Corrupted log entries
            // - Lost log data
            
            if (IOConfig.ENABLE_PROPER_SYNCHRONIZATION)
            {
                // SOLUTION: Use lock for thread-safe logging
                lock (logLock)
                {
                    try
                    {
                        using (var writer = new StreamWriter(IOConfig.LOG_FILE, append: true))
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
            else
            {
                // PROBLEM: No synchronization in logging
                try
                {
                    using (var writer = new StreamWriter(IOConfig.LOG_FILE, append: true))
                    {
                        // Simulate processing to increase chance of race conditions
                        Thread.Sleep(1);
                        writer.WriteLine($"[Thread {threadId}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }

        // PROBLEM 4: File creation/deletion race conditions
        private async Task DemonstrateFileRaceConditionsAsync(int threadId)
        {
            for (int op = 0; op < IOConfig.OPERATIONS_PER_THREAD; op++)
            {
                string filename = $"{IOConfig.BASE_FILENAME}{threadId}_{op}.tmp";
                
                try
                {
                    // PROBLEM: Race condition in counter increment
                    if (IOConfig.ENABLE_PROPER_SYNCHRONIZATION)
                    {
                        // SOLUTION: Use atomic operations
                        Interlocked.Increment(ref operationCounter);
                    }
                    else
                    {
                        // PROBLEM: Non-atomic increment (race condition)
                        unsafeCounter++;  // This will likely produce incorrect results
                        operationCounter++; // Also unsafe
                    }
                    
                    // Create file
                    string content = GenerateFileContent(threadId, op);
                    
                    using (var writer = new StreamWriter(filename, append: false))
                    {
                        writer.Write(content);
                        // PROBLEM: Not always flushing properly
                    }
                    
                    Console.WriteLine($"[THREAD {threadId}] CREATED FILE: {filename} ({Encoding.UTF8.GetByteCount(content)} bytes)");
                    UnsafeLogging(threadId, $"Created file: {filename}");
                    
                    // PROBLEM 5: Immediate read after write without proper synchronization
                    // This can cause:
                    // - Reading incomplete data
                    // - File not found errors
                    // - Inconsistent file state
                    
                    // Small delay to simulate processing (but not enough for proper synchronization)
                    await Task.Delay(1);
                    
                    // Try to read the file we just created
                    try
                    {
                        if (File.Exists(filename))
                        {
                            using (var reader = new StreamReader(filename))
                            {
                                string fileContent = await reader.ReadToEndAsync();
                                long fileSize = Encoding.UTF8.GetByteCount(fileContent);
                                
                                totalBytesProcessed += fileSize;
                                Console.WriteLine($"[THREAD {threadId}] READ FILE: {filename} ({fileSize} bytes)");
                                UnsafeLogging(threadId, $"Read file: {filename} ({fileSize} bytes)");
                            }
                        }
                        else
                        {
                            errorCounter++;
                            Console.WriteLine($"[THREAD {threadId}] ERROR: Could not read file: {filename}");
                            UnsafeLogging(threadId, $"ERROR: Could not read file: {filename}");
                        }
                    }
                    catch (Exception readEx)
                    {
                        errorCounter++;
                        Console.WriteLine($"[THREAD {threadId}] ERROR reading file {filename}: {readEx.Message}");
                        UnsafeLogging(threadId, $"ERROR reading file {filename}: {readEx.Message}");
                    }
                    
                    // PROBLEM 6: File deletion while other threads might be accessing
                    // This can cause:
                    // - Access denied errors
                    // - Partial deletions
                    // - Inconsistent file system state
                    
                    try
                    {
                        if (File.Exists(filename))
                        {
                            // PROBLEM: No coordination before deletion
                            File.Delete(filename);
                            Console.WriteLine($"[THREAD {threadId}] DELETED FILE: {filename}");
                            UnsafeLogging(threadId, $"Deleted file: {filename}");
                        }
                    }
                    catch (Exception deleteEx)
                    {
                        Console.WriteLine($"[THREAD {threadId}] ERROR: Could not delete file: {filename} - {deleteEx.Message}");
                        UnsafeLogging(threadId, $"ERROR: Could not delete file: {filename} - {deleteEx.Message}");
                    }
                    
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    UnsafeLogging(threadId, $"ERROR in file operations: {ex.Message}");
                }
                
                await Task.Delay(IOConfig.DELAY_BETWEEN_OPS_MS);
            }
        }

        // PROBLEM 7: Concurrent access to the same file with different modes
        private async Task DemonstrateFileLockingProblemsAsync(int threadId)
        {
            string sharedDataFile = $"shared_data_{threadId % 3}.dat";
            
            for (int op = 0; op < IOConfig.OPERATIONS_PER_THREAD / 2; op++)
            {
                try
                {
                    if (op % 2 == 0)
                    {
                        // Writer thread
                        string data = $"Data from thread {threadId} operation {op}\n";
                        
                        // PROBLEM: Multiple writers without coordination
                        using (var writer = new StreamWriter(sharedDataFile, append: true))
                        {
                            // Simulate slow write operation to increase conflicts
                            await Task.Delay(5);
                            writer.Write(data);
                        }
                        
                        UnsafeLogging(threadId, $"Wrote to shared file: {sharedDataFile}");
                    }
                    else
                    {
                        // Reader thread
                        // PROBLEM: Reading while writing might be in progress
                        try
                        {
                            if (File.Exists(sharedDataFile))
                            {
                                string[] lines = File.ReadAllLines(sharedDataFile);
                                int lineCount = lines.Length;
                                
                                UnsafeLogging(threadId, $"Read shared file: {sharedDataFile} ({lineCount} lines)");
                            }
                        }
                        catch (Exception readEx)
                        {
                            errorCounter++;
                            UnsafeLogging(threadId, $"ERROR reading shared file {sharedDataFile}: {readEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorCounter++;
                    UnsafeLogging(threadId, $"ERROR in file locking demo: {ex.Message}");
                }
                
                await Task.Delay(IOConfig.DELAY_BETWEEN_OPS_MS);
            }
        }

        public async Task RunConcurrentOperationsAsync()
        {
            Console.WriteLine("=== CONCURRENT I/O PROBLEMS DEMONSTRATION (C#) ===");
            Console.WriteLine("This program demonstrates various I/O concurrency issues:");
            Console.WriteLine("1. Race conditions in shared file access");
            Console.WriteLine("2. Unsafe logging operations");
            Console.WriteLine("3. File creation/deletion conflicts");
            Console.WriteLine("4. File locking problems");
            Console.WriteLine("5. Counter race conditions");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Configuration:");
            Console.WriteLine($"- Threads: {IOConfig.NUM_THREADS}");
            Console.WriteLine($"- Operations per thread: {IOConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine($"- Proper synchronization: {(IOConfig.ENABLE_PROPER_SYNCHRONIZATION ? "ENABLED" : "DISABLED")}");
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
                Console.WriteLine($">>> STARTING CYCLE #{cycleCount} <<<");
                Console.WriteLine(new string('=', 60));

                // Reset counters for this cycle
                operationCounter = 0;
                unsafeCounter = 0;
                errorCounter = 0;
                totalBytesProcessed = 0;
                stopwatch.Restart();

                // Launch tasks that will compete for I/O resources
                Console.WriteLine($"Launching {IOConfig.NUM_THREADS} concurrent tasks...");
                
                var tasks = new List<Task>();
                for (int i = 0; i < IOConfig.NUM_THREADS; i++)
                {
                    int threadId = i; // Capture for closure
                    tasks.Add(Task.Run(async () =>
                    {
                        Console.WriteLine($"  Task {threadId} started (Cycle {cycleCount})");
                        
                        // Each task performs multiple types of problematic I/O operations
                        await DemonstrateSharedFileContentionAsync(threadId);
                        await DemonstrateFileRaceConditionsAsync(threadId);
                        await DemonstrateFileLockingProblemsAsync(threadId);
                        
                        Console.WriteLine($"  Task {threadId} completed (Cycle {cycleCount})");
                    }));
                }

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);

                // Display results for this cycle
                Console.WriteLine($"\n{new string('-', 40)}");
                Console.WriteLine($"CYCLE #{cycleCount} RESULTS:");
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
            Console.WriteLine($"DEMONSTRATION COMPLETED AFTER {cycleCount} CYCLES");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Check the following files for evidence of concurrency problems:");
            Console.WriteLine($"- {IOConfig.SHARED_FILE} (shared file access conflicts)");
            Console.WriteLine($"- {IOConfig.LOG_FILE} (logging race conditions)");
            Console.WriteLine("- Various temporary files (file creation/deletion races)");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }

        private void DisplayResults()
        {
            stopwatch.Stop();
            
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine("CONCURRENT I/O PROBLEMS RESULTS");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total threads: {IOConfig.NUM_THREADS}");
            Console.WriteLine($"Expected operations: {IOConfig.NUM_THREADS * IOConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine($"Safe counter result: {operationCounter}");
            Console.WriteLine($"Unsafe counter result: {unsafeCounter} (should be same as safe counter)");
            Console.WriteLine($"Errors encountered: {errorCounter}");
            Console.WriteLine($"Total bytes processed: {totalBytesProcessed / 1024.0:F2} KB");

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
                    int corruptedLines = 0;
                    
                    foreach (string line in lines)
                    {
                        // Check for obviously corrupted lines (incomplete thread info)
                        if (!line.Contains("[Thread") && !string.IsNullOrWhiteSpace(line))
                        {
                            corruptedLines++;
                        }
                    }
                    
                    Console.WriteLine($"Log file lines: {lines.Length}");
                    Console.WriteLine($"Potentially corrupted log lines: {corruptedLines}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading log file: {ex.Message}");
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("ANALYSIS:");
            Console.WriteLine("- If unsafe counter != safe counter: RACE CONDITION detected!");
            Console.WriteLine("- If errors > 0: FILE ACCESS CONFLICTS detected!");
            Console.WriteLine("- If corrupted log lines > 0: LOGGING RACE CONDITIONS detected!");
            Console.WriteLine($"- Check {IOConfig.SHARED_FILE} and {IOConfig.LOG_FILE} for data corruption");
            Console.WriteLine(new string('=', 60));
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demo = new ConcurrentIOProblems();
                await demo.RunConcurrentOperationsAsync();
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
