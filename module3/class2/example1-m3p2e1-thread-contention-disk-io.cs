using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace ThreadContentionDiskIODemo
{
    // ====================================================================
    // DISK PERFORMANCE PROBLEM PARAMETERS - DESIGNED TO CREATE BAD METRICS
    // ====================================================================
    public static class DiskProblemConfig
    {
        // PROBLEM CONFIGURATION: Creates High Disk Queue Length, Low Bytes/sec, Low Avg Bytes/Transfer
        public const int DISK_THRASHING_THREADS = 32;         // Many threads competing for disk
        public const int OPERATIONS_PER_THREAD = 500;         // Many operations per thread
        public const int TINY_WRITE_SIZE = 64;                // Very small writes (low avg bytes/transfer)
        public const int TINY_READ_SIZE = 32;                 // Very small reads (low avg bytes/transfer)
        public const int RANDOM_FILES_COUNT = 200;            // Many files for random access
        public const int SEEK_OPERATIONS_PER_CYCLE = 50;      // Random seeks to create queue buildup
        public const int SYNC_DELAY_MICROSECONDS = 100;       // Tiny delays to amplify queue buildup
        public const int FILE_FRAGMENT_SIZE = 128;            // Small fragments to force seeking
        public const string BASE_DIRECTORY = "disk_problem_test/";
        public const string FRAGMENT_FILE_PREFIX = "fragment_";
        public const string RANDOM_FILE_PREFIX = "random_";
        public const string LOG_FILE = "disk_performance_problems.log";
        public const bool ENABLE_PROBLEMATIC_CODE = true;     // Switch between good/bad code
        
        // METRICS TRACKING
        public const int PERFORMANCE_SAMPLE_INTERVAL_MS = 1000; // Sample metrics every second
    }
    // ====================================================================

    public enum DiskOperationType
    {
        TinyWrite,
        TinyRead,
        RandomSeek,
        FileFragment
    }

    public class DiskOperation
    {
        public int ThreadId { get; set; }
        public DiskOperationType OperationType { get; set; }
        public string Operation { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Completed { get; set; }
        public long BytesTransferred { get; set; }
        public long DurationMicroseconds { get; set; }
        public string FileName { get; set; } = string.Empty;
    }

    class DiskPerformanceProblemDemo
    {
        // DISK PERFORMANCE METRICS - These will show the problems!
        private long totalDiskOperations = 0;
        private long completedDiskOperations = 0;
        private long totalBytesTransferred = 0;
        private long totalDiskQueueLength = 0;
        private long currentDiskQueueLength = 0;
        private long tinyWriteOperations = 0;
        private long tinyReadOperations = 0;
        private long randomSeekOperations = 0;
        private long fragmentOperations = 0;
        private long totalDiskTimeMs = 0;
        
        private readonly object logLock = new object();
        private readonly object queueLock = new object();
        private readonly Stopwatch stopwatch;
        private readonly Random random;
        private readonly ConcurrentQueue<DiskOperation> operationLog;
        private readonly List<string> createdFiles = new List<string>();
        private volatile bool userStopped = false;

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                              PROBLEMATIC CODE SECTION                                          */
        /*                    DEMONSTRATES DISK PERFORMANCE PROBLEMS                                      */
        /*                                                                                                 */
        /*  This section creates the exact problems you want to see:                                      */
        /*  - HIGH Current Disk Queue Length (many small operations queued)                              */
        /*  - LOW Disk Bytes/sec (inefficient tiny transfers)                                            */
        /*  - LOW Avg Disk Bytes/Transfer (64-byte writes, 32-byte reads)                                */
        /*                                                                                                 */
        /***************************************************************************************************/
        
        // PROBLEM 1: Forces synchronous I/O to create disk queue buildup
        private readonly object diskSerializationLock = new object();
        
        // PROBLEM 2: Creates many tiny files for inefficient access patterns
        private readonly List<string> fragmentedFiles = new List<string>();
        
        // PROBLEM 3: Random access patterns that force disk head movement
        private readonly List<string> randomAccessFiles = new List<string>();

        public DiskPerformanceProblemDemo()
        {
            stopwatch = Stopwatch.StartNew();
            random = new Random();
            operationLog = new ConcurrentQueue<DiskOperation>();
            
            // Initialize directories for problematic disk access patterns
            Directory.CreateDirectory(DiskProblemConfig.BASE_DIRECTORY);
            Directory.CreateDirectory(Path.Combine(DiskProblemConfig.BASE_DIRECTORY, "fragments"));
            Directory.CreateDirectory(Path.Combine(DiskProblemConfig.BASE_DIRECTORY, "random"));
            
            // Pre-create many small files for random access (creates fragmentation)
            for (int i = 0; i < DiskProblemConfig.RANDOM_FILES_COUNT; i++)
            {
                string filename = Path.Combine(DiskProblemConfig.BASE_DIRECTORY, "random", 
                                             $"{DiskProblemConfig.RANDOM_FILE_PREFIX}{i}.dat");
                randomAccessFiles.Add(filename);
                
                // Create tiny files scattered across disk
                File.WriteAllBytes(filename, new byte[DiskProblemConfig.FILE_FRAGMENT_SIZE]);
            }
            
            using (var writer = new StreamWriter(DiskProblemConfig.LOG_FILE, false))
            {
                writer.WriteLine("=== DISK PERFORMANCE PROBLEMS DEMONSTRATION LOG ===");
                writer.WriteLine("TARGET METRICS: High Disk Queue Length, Low Bytes/sec, Low Avg Bytes/Transfer");
            }
            
            LogPerformance("Disk Performance Problem Demo initialized");
        }

        private void LogPerformance(string message)
        {
            lock (logLock)
            {
                try
                {
                    using (var writer = new StreamWriter(DiskProblemConfig.LOG_FILE, append: true))
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

        private byte[] GenerateTinyContent(int sizeBytes, int threadId)
        {
            // Generate very small content to create low avg bytes/transfer
            var content = new byte[sizeBytes];
            for (int i = 0; i < sizeBytes; i++)
            {
                content[i] = (byte)(threadId % 256); // Simple pattern based on thread ID
            }
            return content;
        }

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                           PROBLEMATIC IMPLEMENTATION                                            */
        /*                        CREATES TERRIBLE DISK PERFORMANCE                                       */
        /*                                                                                                 */
        /*  This section demonstrates BAD practices that create disk performance problems:                */
        /*  - Many tiny I/O operations (low avg bytes/transfer)                                          */
        /*  - Random access patterns (high seek time)                                                    */
        /*  - Synchronous operations that build up disk queue                                            */
        /*  - File fragmentation and inefficient access                                                  */
        /*                                                                                                 */
        /***************************************************************************************************/
        
        private async Task PerformProblematicDiskOperationAsync(int threadId)
        {
            for (int op = 0; op < DiskProblemConfig.OPERATIONS_PER_THREAD && !userStopped; op++)
            {
                var operationStart = DateTime.Now;
                Interlocked.Increment(ref totalDiskOperations);
                
                try
                {
                    // Increment queue length - simulates many operations waiting
                    lock (queueLock)
                    {
                        Interlocked.Increment(ref currentDiskQueueLength);
                        Interlocked.Increment(ref totalDiskQueueLength);
                    }
                    
                    // PROBLEM 1: TINY WRITE OPERATIONS (creates low avg bytes/transfer)
                    // Each write is only 64 bytes - terrible for disk efficiency!
                    await PerformTinyWriteOperation(threadId, op);
                    
                    // PROBLEM 2: TINY READ OPERATIONS (creates low avg bytes/transfer)  
                    // Each read is only 32 bytes - forces many disk operations!
                    await PerformTinyReadOperation(threadId, op);
                    
                    // PROBLEM 3: RANDOM SEEK OPERATIONS (creates high queue length)
                    // Random access to different files forces disk head movement!
                    await PerformRandomSeekOperation(threadId, op);
                    
                    // PROBLEM 4: FILE FRAGMENTATION (creates inefficient access)
                    // Creates many tiny files scattered across disk!
                    await PerformFileFragmentationOperation(threadId, op);
                    
                    // Decrement queue length
                    lock (queueLock)
                    {
                        Interlocked.Decrement(ref currentDiskQueueLength);
                    }
                    
                    Interlocked.Increment(ref completedDiskOperations);
                    
                    // Tiny delay to let other threads queue up (amplifies queue length)
                    await Task.Delay(1); // Small delay to allow thread switching
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DISK ERROR] Thread {threadId}: {ex.Message}");
                    LogPerformance($"DISK ERROR in thread {threadId}: {ex.Message}");
                    
                    // Still decrement queue on error
                    lock (queueLock)
                    {
                        if (Interlocked.Read(ref currentDiskQueueLength) > 0)
                            Interlocked.Decrement(ref currentDiskQueueLength);
                    }
                }
            }
        }

        // TINY WRITE OPERATION - Creates low avg bytes/transfer (only 64 bytes per write!)
        private Task PerformTinyWriteOperation(int threadId, int operationId)
        {
            var sw = Stopwatch.StartNew();
            
            // PROBLEM: Each write is tiny (64 bytes) - very inefficient!
            byte[] tinyData = GenerateTinyContent(DiskProblemConfig.TINY_WRITE_SIZE, threadId);
            
            string filename = Path.Combine(DiskProblemConfig.BASE_DIRECTORY, 
                                         $"tiny_write_{threadId}_{operationId}.dat");
            
            // Force synchronous write to build up disk queue
            lock (diskSerializationLock)
            {
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(tinyData, 0, tinyData.Length);
                    fs.Flush(true); // Force to disk immediately
                }
            }
            
            sw.Stop();
            Interlocked.Increment(ref tinyWriteOperations);
            Interlocked.Add(ref totalBytesTransferred, tinyData.Length);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[TINY WRITE] Thread {threadId}: {tinyData.Length} bytes in {sw.ElapsedMilliseconds}ms");
            
            return Task.CompletedTask;
        }

        // TINY READ OPERATION - Creates low avg bytes/transfer (only 32 bytes per read!)
        private Task PerformTinyReadOperation(int threadId, int operationId)
        {
            if (randomAccessFiles.Count == 0) return Task.CompletedTask;
            
            var sw = Stopwatch.StartNew();
            
            // PROBLEM: Each read is tiny (32 bytes) - very inefficient!
            string filename = randomAccessFiles[random.Next(randomAccessFiles.Count)];
            
            // Force synchronous read to build up disk queue
            lock (diskSerializationLock)
            {
                try
                {
                    using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] buffer = new byte[DiskProblemConfig.TINY_READ_SIZE];
                        int bytesRead = fs.Read(buffer, 0, buffer.Length);
                        
                        Interlocked.Add(ref totalBytesTransferred, bytesRead);
                    }
                }
                catch (FileNotFoundException)
                {
                    // File might have been deleted by another thread - ignore
                }
            }
            
            sw.Stop();
            Interlocked.Increment(ref tinyReadOperations);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[TINY READ] Thread {threadId}: {DiskProblemConfig.TINY_READ_SIZE} bytes in {sw.ElapsedMilliseconds}ms");
            
            return Task.CompletedTask;
        }

        // RANDOM SEEK OPERATION - Creates high disk queue length through random access
        private Task PerformRandomSeekOperation(int threadId, int operationId)
        {
            var sw = Stopwatch.StartNew();
            
            // PROBLEM: Random access to different files forces disk head movement!
            for (int i = 0; i < DiskProblemConfig.SEEK_OPERATIONS_PER_CYCLE; i++)
            {
                if (randomAccessFiles.Count == 0) break;
                
                string filename = randomAccessFiles[random.Next(randomAccessFiles.Count)];
                
                lock (diskSerializationLock)
                {
                    try
                    {
                        using (var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            // Random seek within file
                            long seekPosition = random.Next(0, (int)Math.Max(1, fs.Length - 10));
                            fs.Seek(seekPosition, SeekOrigin.Begin);
                            
                            // Tiny read at random position
                            byte[] buffer = new byte[8];
                            int bytesRead = fs.Read(buffer, 0, buffer.Length);
                            
                            Interlocked.Add(ref totalBytesTransferred, bytesRead);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore file access errors
                    }
                }
            }
            
            sw.Stop();
            Interlocked.Increment(ref randomSeekOperations);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[RANDOM SEEK] Thread {threadId}: {DiskProblemConfig.SEEK_OPERATIONS_PER_CYCLE} seeks in {sw.ElapsedMilliseconds}ms");
            
            return Task.CompletedTask;
        }

        // FILE FRAGMENTATION OPERATION - Creates many tiny files scattered across disk
        private Task PerformFileFragmentationOperation(int threadId, int operationId)
        {
            var sw = Stopwatch.StartNew();
            
            // PROBLEM: Creates many tiny files that fragment the disk!
            string fragmentDir = Path.Combine(DiskProblemConfig.BASE_DIRECTORY, "fragments");
            string filename = Path.Combine(fragmentDir, 
                                         $"{DiskProblemConfig.FRAGMENT_FILE_PREFIX}{threadId}_{operationId}_{random.Next(1000)}.dat");
            
            byte[] fragmentData = GenerateTinyContent(DiskProblemConfig.FILE_FRAGMENT_SIZE, threadId);
            
            lock (diskSerializationLock)
            {
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(fragmentData, 0, fragmentData.Length);
                    fs.Flush(true); // Force to disk
                }
            }
            
            lock (fragmentedFiles)
            {
                fragmentedFiles.Add(filename);
                
                // Occasionally delete old fragments to create more fragmentation
                if (fragmentedFiles.Count > 1000)
                {
                    string oldFile = fragmentedFiles[0];
                    fragmentedFiles.RemoveAt(0);
                    try
                    {
                        File.Delete(oldFile);
                    }
                    catch (Exception)
                    {
                        // Ignore deletion errors
                    }
                }
            }
            
            sw.Stop();
            Interlocked.Increment(ref fragmentOperations);
            Interlocked.Add(ref totalBytesTransferred, fragmentData.Length);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[FRAGMENT] Thread {threadId}: Created {fragmentData.Length}-byte fragment in {sw.ElapsedMilliseconds}ms");
            
            return Task.CompletedTask;
        }

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                              CORRECT IMPLEMENTATION                                             */
        /*                        DEMONSTRATES PROPER DISK I/O OPTIMIZATION                               */
        /*                                                                                                 */
        /*  This section will demonstrate GOOD practices that solve disk performance problems:            */
        /*  - Larger transfer sizes (better avg bytes/transfer)                                          */
        /*  - Sequential access patterns (reduced seeking)                                               */
        /*  - Batched operations (reduced queue length)                                                  */
        /*  - Efficient resource utilization                                                             */
        /*                                                                                                 */
        /*  NOTE: Optimized implementation to be added in future classroom exercises                     */
        /*                                                                                                 */
        /***************************************************************************************************/

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                                MAIN DEMONSTRATION                                               */
        /*                          SWITCH BETWEEN PROBLEMATIC AND CORRECT CODE                           */
        /*                                                                                                 */
        /*  TO DEMONSTRATE PROBLEMS: Set ENABLE_PROBLEMATIC_CODE = true                                   */
        /*  TO DEMONSTRATE SOLUTIONS: Set ENABLE_PROBLEMATIC_CODE = false                                 */
        /*                                                                                                 */
        /***************************************************************************************************/

        public async Task RunDiskPerformanceProblemDemoAsync()
        {
            Console.WriteLine("=== DISK PERFORMANCE PROBLEMS DEMONSTRATION ===");
            Console.WriteLine("This program creates TERRIBLE disk performance metrics:");
            Console.WriteLine("1. HIGH Current Disk Queue Length (many operations waiting)");
            Console.WriteLine("2. LOW Disk Bytes/sec (inefficient tiny transfers)");
            Console.WriteLine("3. LOW Avg Disk Bytes/Transfer (64-byte writes, 32-byte reads)");
            Console.WriteLine("4. Random access patterns (excessive seeking)");
            Console.WriteLine("5. File fragmentation (scattered tiny files)");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("DISK PROBLEM PARAMETERS:");
            Console.WriteLine($"- Disk thrashing threads: {DiskProblemConfig.DISK_THRASHING_THREADS}");
            Console.WriteLine($"- Operations per thread: {DiskProblemConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine($"- Tiny write size: {DiskProblemConfig.TINY_WRITE_SIZE} bytes");
            Console.WriteLine($"- Tiny read size: {DiskProblemConfig.TINY_READ_SIZE} bytes");
            Console.WriteLine($"- Random files: {DiskProblemConfig.RANDOM_FILES_COUNT}");
            Console.WriteLine($"- Seek operations per cycle: {DiskProblemConfig.SEEK_OPERATIONS_PER_CYCLE}");
            Console.WriteLine(new string('=', 70));
            
            if (DiskProblemConfig.ENABLE_PROBLEMATIC_CODE)
            {
                Console.WriteLine(">>> RUNNING PROBLEMATIC CODE - EXPECT TERRIBLE DISK METRICS <<<");
                Console.WriteLine(">>> WATCH: Task Manager -> Performance -> Disk for the problems! <<<");
                Console.WriteLine(">>> TARGET: High Queue Length, Low Bytes/sec, Low Avg Bytes/Transfer <<<");
            }
            else
            {
                Console.WriteLine(">>> RUNNING OPTIMIZED CODE - BETTER DISK PERFORMANCE <<<");
                Console.WriteLine(">>> SOLUTIONS: Larger transfers, Sequential access, Batching <<<");
            }
            
            Console.WriteLine("Press any key to stop the demonstration...");
            Console.WriteLine(new string('-', 70));

            // Start monitoring for user input
            var keyTask = Task.Run(() =>
            {
                Console.ReadKey(true);
                userStopped = true;
                Console.WriteLine("\n>>> User requested stop. Finishing current operations...");
            });

            /***************************************************************************************************/
            /*                        THREAD EXECUTION LOGIC                                               */
            /*  This section switches between problematic and optimized implementations                    */
            /*  based on the ENABLE_PROBLEMATIC_CODE configuration flag                                   */
            /***************************************************************************************************/

            var tasks = new List<Task>();

            // Launch disk thrashing threads
            for (int i = 0; i < DiskProblemConfig.DISK_THRASHING_THREADS; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(async () =>
                {
                    Console.WriteLine($"  DISK THREAD {threadId} started - creating disk performance problems");
                    
                    if (DiskProblemConfig.ENABLE_PROBLEMATIC_CODE)
                    {
                        // Execute PROBLEMATIC disk operations that create terrible metrics
                        await PerformProblematicDiskOperationAsync(threadId);
                    }
                    else
                    {
                        // Execute OPTIMIZED disk operations (to be implemented)
                        // await PerformOptimizedDiskOperationAsync(threadId);
                        Console.WriteLine($"  OPTIMIZED operations not implemented yet for thread {threadId}");
                    }
                    
                    Console.WriteLine($"  DISK THREAD {threadId} completed");
                }));
            }

            // Performance monitoring task
            var perfTask = Task.Run(async () =>
            {
                while (!userStopped)
                {
                    await Task.Delay(DiskProblemConfig.PERFORMANCE_SAMPLE_INTERVAL_MS);
                    if (!userStopped)
                    {
                        DisplayRealTimeDiskPerformance();
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

            Console.WriteLine("\nDisk performance problem demonstration completed.");
            Console.WriteLine("Check Task Manager -> Performance -> Disk to see the impact!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private void DisplayRealTimeDiskPerformance()
        {
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            long completedOps = Interlocked.Read(ref completedDiskOperations);
            long totalBytes = Interlocked.Read(ref totalBytesTransferred);
            long currentQueue = Interlocked.Read(ref currentDiskQueueLength);
            
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine("REAL-TIME DISK PERFORMANCE PROBLEMS");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Running time: {elapsedSeconds:F1} seconds");
            
            // TARGET METRICS - These should be BAD!
            Console.WriteLine("\n>>> TARGET PROBLEM METRICS <<<");
            Console.WriteLine($"Current Disk Queue Length: {currentQueue:N0} (TARGET: HIGH - many operations waiting)");
            
            if (elapsedSeconds > 0)
            {
                double bytesPerSec = totalBytes / elapsedSeconds;
                Console.WriteLine($"Disk Bytes/sec: {bytesPerSec:F0} (TARGET: LOW - inefficient throughput)");
            }
            
            if (completedOps > 0)
            {
                double avgBytesPerTransfer = (double)totalBytes / completedOps;
                Console.WriteLine($"Avg Disk Bytes/Transfer: {avgBytesPerTransfer:F1} (TARGET: LOW - tiny operations)");
            }
            
            // OPERATION BREAKDOWN
            Console.WriteLine("\n>>> OPERATION BREAKDOWN <<<");
            Console.WriteLine($"Total disk operations: {Interlocked.Read(ref totalDiskOperations):N0}");
            Console.WriteLine($"Completed operations: {completedOps:N0}");
            Console.WriteLine($"Tiny writes ({DiskProblemConfig.TINY_WRITE_SIZE} bytes): {Interlocked.Read(ref tinyWriteOperations):N0}");
            Console.WriteLine($"Tiny reads ({DiskProblemConfig.TINY_READ_SIZE} bytes): {Interlocked.Read(ref tinyReadOperations):N0}");
            Console.WriteLine($"Random seeks: {Interlocked.Read(ref randomSeekOperations):N0}");
            Console.WriteLine($"File fragments: {Interlocked.Read(ref fragmentOperations):N0}");
            Console.WriteLine($"Total bytes transferred: {totalBytes:N0}");
            
            if (elapsedSeconds > 0)
            {
                Console.WriteLine($"Operations/sec: {(completedOps / elapsedSeconds):F2}");
            }
            
            Console.WriteLine(new string('=', 80));
            
            LogPerformance($"Disk metrics - Queue: {currentQueue}, Bytes/sec: {(totalBytes / Math.Max(1, elapsedSeconds)):F0}, " +
                          $"Avg bytes/transfer: {(totalBytes / Math.Max(1, completedOps)):F1}");
        }

        private void DisplayFinalResults()
        {
            stopwatch.Stop();
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            long completedOps = Interlocked.Read(ref completedDiskOperations);
            long totalBytes = Interlocked.Read(ref totalBytesTransferred);
            
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine("FINAL DISK PERFORMANCE PROBLEM RESULTS");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.WriteLine($"Total disk threads: {DiskProblemConfig.DISK_THRASHING_THREADS}");
            Console.WriteLine($"Total disk operations attempted: {Interlocked.Read(ref totalDiskOperations):N0}");
            Console.WriteLine($"Total disk operations completed: {completedOps:N0}");
            
            // FINAL PROBLEM METRICS - These should be BAD!
            Console.WriteLine("\n>>> FINAL PROBLEM METRICS ACHIEVED <<<");
            Console.WriteLine($"Max Disk Queue Length Reached: {Interlocked.Read(ref totalDiskQueueLength):N0}");
            
            if (elapsedSeconds > 0)
            {
                double bytesPerSec = totalBytes / elapsedSeconds;
                Console.WriteLine($"Average Disk Bytes/sec: {bytesPerSec:F0} (TARGET: LOW)");
            }
            
            if (completedOps > 0)
            {
                double avgBytesPerTransfer = (double)totalBytes / completedOps;
                Console.WriteLine($"Average Disk Bytes/Transfer: {avgBytesPerTransfer:F1} (TARGET: LOW)");
            }
            
            Console.WriteLine($"Operations per second: {(completedOps / Math.Max(1, elapsedSeconds)):F2}");
            
            // OPERATION BREAKDOWN
            Console.WriteLine("\n>>> OPERATION BREAKDOWN <<<");
            Console.WriteLine($"Tiny writes ({DiskProblemConfig.TINY_WRITE_SIZE} bytes): {Interlocked.Read(ref tinyWriteOperations):N0}");
            Console.WriteLine($"Tiny reads ({DiskProblemConfig.TINY_READ_SIZE} bytes): {Interlocked.Read(ref tinyReadOperations):N0}");
            Console.WriteLine($"Random seeks: {Interlocked.Read(ref randomSeekOperations):N0}");
            Console.WriteLine($"File fragments created: {Interlocked.Read(ref fragmentOperations):N0}");
            Console.WriteLine($"Total bytes transferred: {totalBytes:N0}");
            Console.WriteLine($"Total disk time: {Interlocked.Read(ref totalDiskTimeMs):N0} ms");
            
            Console.WriteLine(new string('=', 80));
            
            /***************************************************************************************************/
            /*                              EDUCATIONAL SUMMARY                                            */
            /*  This section shows what disk performance problems were demonstrated                        */
            /***************************************************************************************************/
            
            if (DiskProblemConfig.ENABLE_PROBLEMATIC_CODE)
            {
                Console.WriteLine("DISK PERFORMANCE PROBLEMS DEMONSTRATED:");
                Console.WriteLine("❌ High Current Disk Queue Length (many operations waiting)");
                Console.WriteLine("❌ Low Disk Bytes/sec (inefficient tiny transfers)");
                Console.WriteLine("❌ Low Avg Disk Bytes/Transfer (64-byte writes, 32-byte reads)");
                Console.WriteLine("❌ Random access patterns (excessive seeking)");
                Console.WriteLine("❌ File fragmentation (scattered tiny files)");
                Console.WriteLine("❌ Synchronous I/O blocking (queue buildup)");
            }
            else
            {
                Console.WriteLine("DISK PERFORMANCE SOLUTIONS DEMONSTRATED:");
                Console.WriteLine("✓ Larger transfer sizes improve efficiency");
                Console.WriteLine("✓ Sequential access reduces seeking");
                Console.WriteLine("✓ Batched operations reduce queue length");
                Console.WriteLine("✓ Asynchronous I/O prevents blocking");
                Console.WriteLine("✓ Better resource utilization");
            }
            
            Console.WriteLine($"- Check {DiskProblemConfig.LOG_FILE} for detailed metrics");
            Console.WriteLine($"- Monitor Task Manager -> Performance -> Disk during execution");
            Console.WriteLine(new string('=', 80));
            
            LogPerformance($"Final results - Duration: {stopwatch.ElapsedMilliseconds}ms, " +
                          $"Completed: {completedOps}, Bytes/sec: {(totalBytes / Math.Max(1, elapsedSeconds)):F0}, " +
                          $"Avg bytes/transfer: {(totalBytes / Math.Max(1, completedOps)):F1}");
        }

        public void Dispose()
        {
            // Clean up any resources if needed
            // Most resources are automatically disposed when the program ends
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demo = new DiskPerformanceProblemDemo();
                await demo.RunDiskPerformanceProblemDemoAsync();
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
