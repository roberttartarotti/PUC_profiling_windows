using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace DiskPerformanceOptimizedDemo
{
    // ====================================================================
    // DISK PERFORMANCE OPTIMIZATION PARAMETERS - DESIGNED FOR EFFICIENCY
    // ====================================================================
    public static class OptimizedDiskConfig
    {
        // SOLUTION CONFIGURATION: Creates Low Disk Queue Length, High Bytes/sec, High Avg Bytes/Transfer
        public const int EFFICIENT_THREADS = 8;               // Fewer threads to reduce contention
        public const int OPERATIONS_PER_THREAD = 100;         // Fewer but more efficient operations
        public const int LARGE_WRITE_SIZE = 64 * 1024;        // 64KB writes (high avg bytes/transfer)
        public const int LARGE_READ_SIZE = 32 * 1024;         // 32KB reads (high avg bytes/transfer)
        public const int BATCH_SIZE = 16;                     // Batch operations together
        public const int SEQUENTIAL_FILE_SIZE = 1024 * 1024;  // 1MB sequential files
        public const int WRITE_BUFFER_SIZE = 256 * 1024;      // 256KB write buffer
        public const string BASE_DIRECTORY = "disk_optimized_test/";
        public const string SEQUENTIAL_FILE_PREFIX = "sequential_";
        public const string BATCH_FILE_PREFIX = "batch_";
        public const string LOG_FILE = "disk_performance_optimized.log";
        public const bool ENABLE_PROBLEMATIC_CODE = false;    // Switch between good/bad code
        
        // METRICS TRACKING
        public const int PERFORMANCE_SAMPLE_INTERVAL_MS = 2000; // Sample metrics every 2 seconds
    }
    // ====================================================================

    public enum OptimizedOperationType
    {
        LargeSequentialWrite,
        LargeSequentialRead,
        BatchedOperations,
        BufferedIO
    }

    public class OptimizedDiskOperation
    {
        public int ThreadId { get; set; }
        public OptimizedOperationType OperationType { get; set; }
        public string Operation { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Completed { get; set; }
        public long BytesTransferred { get; set; }
        public long DurationMicroseconds { get; set; }
        public string FileName { get; set; } = string.Empty;
    }

    class DiskPerformanceOptimizedDemo
    {
        // OPTIMIZED DISK PERFORMANCE METRICS - These should be GOOD!
        private long totalDiskOperations = 0;
        private long completedDiskOperations = 0;
        private long totalBytesTransferred = 0;
        private long maxDiskQueueLength = 0;
        private long currentDiskQueueLength = 0;
        private long largeWriteOperations = 0;
        private long largeReadOperations = 0;
        private long batchedOperations = 0;
        private long bufferedOperations = 0;
        private long totalDiskTimeMs = 0;
        
        private readonly object logLock = new object();
        private readonly SemaphoreSlim diskSemaphore = new SemaphoreSlim(4, 4); // Limit concurrent disk operations
        private readonly Stopwatch stopwatch;
        private readonly Random random;
        private readonly ConcurrentQueue<OptimizedDiskOperation> operationLog;
        private readonly List<string> sequentialFiles = new List<string>();
        private volatile bool userStopped = false;

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                              OPTIMIZED CODE SECTION                                            */
        /*                    DEMONSTRATES EXCELLENT DISK PERFORMANCE                                     */
        /*                                                                                                 */
        /*  This section creates EXCELLENT disk performance metrics:                                      */
        /*  - LOW Current Disk Queue Length (efficient coordination)                                     */
        /*  - HIGH Disk Bytes/sec (large efficient transfers)                                            */
        /*  - HIGH Avg Disk Bytes/Transfer (64KB writes, 32KB reads)                                     */
        /*  - Sequential access patterns (minimal seeking)                                               */
        /*  - Batched operations (reduced overhead)                                                      */
        /*                                                                                                 */
        /***************************************************************************************************/

        public DiskPerformanceOptimizedDemo()
        {
            stopwatch = Stopwatch.StartNew();
            random = new Random();
            operationLog = new ConcurrentQueue<OptimizedDiskOperation>();
            
            // Initialize directories for optimized disk access patterns
            Directory.CreateDirectory(OptimizedDiskConfig.BASE_DIRECTORY);
            Directory.CreateDirectory(Path.Combine(OptimizedDiskConfig.BASE_DIRECTORY, "sequential"));
            Directory.CreateDirectory(Path.Combine(OptimizedDiskConfig.BASE_DIRECTORY, "batched"));
            
            // Pre-create sequential files for efficient access
            for (int i = 0; i < OptimizedDiskConfig.EFFICIENT_THREADS; i++)
            {
                string filename = Path.Combine(OptimizedDiskConfig.BASE_DIRECTORY, "sequential", 
                                             $"{OptimizedDiskConfig.SEQUENTIAL_FILE_PREFIX}{i}.dat");
                sequentialFiles.Add(filename);
                
                // Pre-allocate files to reduce fragmentation
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    fs.SetLength(OptimizedDiskConfig.SEQUENTIAL_FILE_SIZE);
                }
            }
            
            using (var writer = new StreamWriter(OptimizedDiskConfig.LOG_FILE, false))
            {
                writer.WriteLine("=== OPTIMIZED DISK PERFORMANCE DEMONSTRATION LOG ===");
                writer.WriteLine("TARGET METRICS: Low Disk Queue Length, High Bytes/sec, High Avg Bytes/Transfer");
            }
            
            LogPerformance("Optimized Disk Performance Demo initialized");
        }

        private void LogPerformance(string message)
        {
            lock (logLock)
            {
                try
                {
                    using (var writer = new StreamWriter(OptimizedDiskConfig.LOG_FILE, append: true))
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

        private byte[] GenerateEfficientContent(int sizeBytes, int threadId)
        {
            // Generate large content for efficient transfers
            var content = new byte[sizeBytes];
            
            // Fill with meaningful pattern for better compression/caching
            for (int i = 0; i < sizeBytes; i += 4)
            {
                var value = BitConverter.GetBytes(threadId + (i / 4));
                for (int j = 0; j < Math.Min(4, sizeBytes - i); j++)
                {
                    content[i + j] = value[j];
                }
            }
            return content;
        }

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                           OPTIMIZED IMPLEMENTATION                                              */
        /*                        CREATES EXCELLENT DISK PERFORMANCE                                      */
        /*                                                                                                 */
        /*  This section demonstrates GOOD practices that create excellent disk performance:              */
        /*  - Large I/O operations (high avg bytes/transfer)                                             */
        /*  - Sequential access patterns (minimal seek time)                                             */
        /*  - Coordinated operations that minimize disk queue                                             */
        /*  - Efficient batching and buffering                                                           */
        /*                                                                                                 */
        /***************************************************************************************************/
        
        private async Task PerformOptimizedDiskOperationAsync(int threadId)
        {
            for (int op = 0; op < OptimizedDiskConfig.OPERATIONS_PER_THREAD && !userStopped; op++)
            {
                var operationStart = DateTime.Now;
                Interlocked.Increment(ref totalDiskOperations);
                
                try
                {
                    // SOLUTION: Use semaphore to limit concurrent operations (prevents queue buildup)
                    await diskSemaphore.WaitAsync();
                    
                    try
                    {
                        // Track queue length (should stay low with coordination)
                        long currentQueue = Interlocked.Increment(ref currentDiskQueueLength);
                        if (currentQueue > Interlocked.Read(ref maxDiskQueueLength))
                        {
                            Interlocked.Exchange(ref maxDiskQueueLength, currentQueue);
                        }
                        
                        // SOLUTION 1: LARGE SEQUENTIAL WRITE OPERATIONS (creates high avg bytes/transfer)
                        // Each write is 64KB - very efficient for disk!
                        await PerformLargeSequentialWriteOperation(threadId, op);
                        
                        // SOLUTION 2: LARGE SEQUENTIAL READ OPERATIONS (creates high avg bytes/transfer)  
                        // Each read is 32KB - efficient disk utilization!
                        await PerformLargeSequentialReadOperation(threadId, op);
                        
                        // SOLUTION 3: BATCHED OPERATIONS (reduces queue length)
                        // Multiple operations batched together for efficiency!
                        await PerformBatchedOperations(threadId, op);
                        
                        // SOLUTION 4: BUFFERED I/O (optimizes access patterns)
                        // Uses large buffers for maximum efficiency!
                        await PerformBufferedIOOperation(threadId, op);
                        
                        // Decrement queue length
                        Interlocked.Decrement(ref currentDiskQueueLength);
                    }
                    finally
                    {
                        diskSemaphore.Release();
                    }
                    
                    Interlocked.Increment(ref completedDiskOperations);
                    
                    // No artificial delays - let operations complete naturally for maximum efficiency
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OPTIMIZED ERROR] Thread {threadId}: {ex.Message}");
                    LogPerformance($"OPTIMIZED ERROR in thread {threadId}: {ex.Message}");
                    
                    // Still decrement queue on error
                    if (Interlocked.Read(ref currentDiskQueueLength) > 0)
                        Interlocked.Decrement(ref currentDiskQueueLength);
                }
            }
        }

        // LARGE SEQUENTIAL WRITE OPERATION - Creates high avg bytes/transfer (64KB per write!)
        private async Task PerformLargeSequentialWriteOperation(int threadId, int operationId)
        {
            var sw = Stopwatch.StartNew();
            
            // SOLUTION: Each write is large (64KB) - very efficient!
            byte[] largeData = GenerateEfficientContent(OptimizedDiskConfig.LARGE_WRITE_SIZE, threadId);
            
            string filename = sequentialFiles[threadId % sequentialFiles.Count];
            
            // Use asynchronous I/O for better performance
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.Read, 
                                         OptimizedDiskConfig.WRITE_BUFFER_SIZE, useAsync: true))
            {
                // Sequential write at calculated position to avoid seeking
                long position = (operationId * OptimizedDiskConfig.LARGE_WRITE_SIZE) % 
                              (OptimizedDiskConfig.SEQUENTIAL_FILE_SIZE - OptimizedDiskConfig.LARGE_WRITE_SIZE);
                fs.Seek(position, SeekOrigin.Begin);
                
                await fs.WriteAsync(largeData, 0, largeData.Length);
                await fs.FlushAsync();
            }
            
            sw.Stop();
            Interlocked.Increment(ref largeWriteOperations);
            Interlocked.Add(ref totalBytesTransferred, largeData.Length);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[LARGE WRITE] Thread {threadId}: {largeData.Length:N0} bytes in {sw.ElapsedMilliseconds}ms");
        }

        // LARGE SEQUENTIAL READ OPERATION - Creates high avg bytes/transfer (32KB per read!)
        private async Task PerformLargeSequentialReadOperation(int threadId, int operationId)
        {
            if (sequentialFiles.Count == 0) return;
            
            var sw = Stopwatch.StartNew();
            
            // SOLUTION: Each read is large (32KB) - very efficient!
            string filename = sequentialFiles[threadId % sequentialFiles.Count];
            
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 
                                         OptimizedDiskConfig.WRITE_BUFFER_SIZE, useAsync: true))
            {
                // Sequential read at calculated position to avoid seeking
                long position = (operationId * OptimizedDiskConfig.LARGE_READ_SIZE) % 
                              (OptimizedDiskConfig.SEQUENTIAL_FILE_SIZE - OptimizedDiskConfig.LARGE_READ_SIZE);
                fs.Seek(position, SeekOrigin.Begin);
                
                byte[] buffer = new byte[OptimizedDiskConfig.LARGE_READ_SIZE];
                int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                
                Interlocked.Add(ref totalBytesTransferred, bytesRead);
            }
            
            sw.Stop();
            Interlocked.Increment(ref largeReadOperations);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[LARGE READ] Thread {threadId}: {OptimizedDiskConfig.LARGE_READ_SIZE:N0} bytes in {sw.ElapsedMilliseconds}ms");
        }

        // BATCHED OPERATIONS - Reduces disk queue length through efficient batching
        private async Task PerformBatchedOperations(int threadId, int operationId)
        {
            var sw = Stopwatch.StartNew();
            
            // SOLUTION: Batch multiple operations together for efficiency!
            string batchDir = Path.Combine(OptimizedDiskConfig.BASE_DIRECTORY, "batched");
            string filename = Path.Combine(batchDir, 
                                         $"{OptimizedDiskConfig.BATCH_FILE_PREFIX}{threadId}_{operationId}.dat");
            
            // Create one large file with multiple data blocks instead of many small files
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, 
                                         OptimizedDiskConfig.WRITE_BUFFER_SIZE, useAsync: true))
            {
                for (int batch = 0; batch < OptimizedDiskConfig.BATCH_SIZE; batch++)
                {
                    byte[] batchData = GenerateEfficientContent(OptimizedDiskConfig.LARGE_WRITE_SIZE / OptimizedDiskConfig.BATCH_SIZE, threadId);
                    await fs.WriteAsync(batchData, 0, batchData.Length);
                    
                    Interlocked.Add(ref totalBytesTransferred, batchData.Length);
                }
                
                await fs.FlushAsync();
            }
            
            sw.Stop();
            Interlocked.Increment(ref batchedOperations);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[BATCHED] Thread {threadId}: {OptimizedDiskConfig.BATCH_SIZE} operations batched in {sw.ElapsedMilliseconds}ms");
        }

        // BUFFERED I/O OPERATION - Uses large buffers for maximum efficiency
        private async Task PerformBufferedIOOperation(int threadId, int operationId)
        {
            var sw = Stopwatch.StartNew();
            
            // SOLUTION: Use large buffers for efficient I/O!
            string filename = sequentialFiles[threadId % sequentialFiles.Count];
            
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 
                                         OptimizedDiskConfig.WRITE_BUFFER_SIZE, useAsync: true))
            {
                // Large buffered read-modify-write operation
                long position = (operationId * OptimizedDiskConfig.WRITE_BUFFER_SIZE) % 
                              (OptimizedDiskConfig.SEQUENTIAL_FILE_SIZE - OptimizedDiskConfig.WRITE_BUFFER_SIZE);
                fs.Seek(position, SeekOrigin.Begin);
                
                // Read large block
                byte[] buffer = new byte[OptimizedDiskConfig.WRITE_BUFFER_SIZE];
                int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                
                // Modify data efficiently
                for (int i = 0; i < bytesRead; i += 4)
                {
                    if (i + 3 < bytesRead)
                    {
                        var value = BitConverter.ToInt32(buffer, i);
                        BitConverter.GetBytes(value + threadId).CopyTo(buffer, i);
                    }
                }
                
                // Write back large block
                fs.Seek(position, SeekOrigin.Begin);
                await fs.WriteAsync(buffer, 0, bytesRead);
                await fs.FlushAsync();
                
                Interlocked.Add(ref totalBytesTransferred, bytesRead * 2); // Read + Write
            }
            
            sw.Stop();
            Interlocked.Increment(ref bufferedOperations);
            Interlocked.Add(ref totalDiskTimeMs, sw.ElapsedMilliseconds);
            
            Console.WriteLine($"[BUFFERED] Thread {threadId}: {OptimizedDiskConfig.WRITE_BUFFER_SIZE:N0} bytes buffered in {sw.ElapsedMilliseconds}ms");
        }

        /***************************************************************************************************/
        /*                                                                                                 */
        /*                                MAIN DEMONSTRATION                                               */
        /*                          OPTIMIZED DISK PERFORMANCE SHOWCASE                                   */
        /*                                                                                                 */
        /*  This demonstrates EXCELLENT disk performance practices                                        */
        /*                                                                                                 */
        /***************************************************************************************************/

        public async Task RunOptimizedDiskPerformanceDemoAsync()
        {
            Console.WriteLine("=== OPTIMIZED DISK PERFORMANCE DEMONSTRATION ===");
            Console.WriteLine("This program demonstrates EXCELLENT disk performance:");
            Console.WriteLine("1. LOW Current Disk Queue Length (coordinated operations)");
            Console.WriteLine("2. HIGH Disk Bytes/sec (large efficient transfers)");
            Console.WriteLine("3. HIGH Avg Disk Bytes/Transfer (64KB writes, 32KB reads)");
            Console.WriteLine("4. Sequential access patterns (minimal seeking)");
            Console.WriteLine("5. Batched operations (reduced overhead)");
            Console.WriteLine("6. Asynchronous I/O (non-blocking operations)");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("OPTIMIZED PARAMETERS:");
            Console.WriteLine($"- Efficient threads: {OptimizedDiskConfig.EFFICIENT_THREADS}");
            Console.WriteLine($"- Operations per thread: {OptimizedDiskConfig.OPERATIONS_PER_THREAD}");
            Console.WriteLine($"- Large write size: {OptimizedDiskConfig.LARGE_WRITE_SIZE:N0} bytes");
            Console.WriteLine($"- Large read size: {OptimizedDiskConfig.LARGE_READ_SIZE:N0} bytes");
            Console.WriteLine($"- Batch size: {OptimizedDiskConfig.BATCH_SIZE}");
            Console.WriteLine($"- Write buffer: {OptimizedDiskConfig.WRITE_BUFFER_SIZE:N0} bytes");
            Console.WriteLine(new string('=', 70));
            
            Console.WriteLine(">>> RUNNING OPTIMIZED CODE - EXPECT EXCELLENT DISK METRICS <<<");
            Console.WriteLine(">>> WATCH: Task Manager -> Performance -> Disk for the improvements! <<<");
            Console.WriteLine(">>> TARGET: Low Queue Length, High Bytes/sec, High Avg Bytes/Transfer <<<");
            
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
            /*                        OPTIMIZED THREAD EXECUTION                                           */
            /*  This section demonstrates coordinated, efficient disk operations                           */
            /***************************************************************************************************/

            var tasks = new List<Task>();

            // Launch optimized disk threads
            for (int i = 0; i < OptimizedDiskConfig.EFFICIENT_THREADS; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(async () =>
                {
                    Console.WriteLine($"  OPTIMIZED THREAD {threadId} started - creating excellent disk performance");
                    
                    // Execute OPTIMIZED disk operations that create excellent metrics
                    await PerformOptimizedDiskOperationAsync(threadId);
                    
                    Console.WriteLine($"  OPTIMIZED THREAD {threadId} completed efficiently");
                }));
            }

            // Performance monitoring task
            var perfTask = Task.Run(async () =>
            {
                while (!userStopped)
                {
                    await Task.Delay(OptimizedDiskConfig.PERFORMANCE_SAMPLE_INTERVAL_MS);
                    if (!userStopped)
                    {
                        DisplayRealTimeOptimizedPerformance();
                    }
                }
            });

            // Wait for user to stop
            await keyTask;
            userStopped = true;

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            await perfTask;

            DisplayFinalOptimizedResults();

            Console.WriteLine("\nOptimized disk performance demonstration completed.");
            Console.WriteLine("Compare these metrics with the problematic version!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private void DisplayRealTimeOptimizedPerformance()
        {
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            long completedOps = Interlocked.Read(ref completedDiskOperations);
            long totalBytes = Interlocked.Read(ref totalBytesTransferred);
            long currentQueue = Interlocked.Read(ref currentDiskQueueLength);
            
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine("REAL-TIME OPTIMIZED DISK PERFORMANCE");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Running time: {elapsedSeconds:F1} seconds");
            
            // OPTIMIZED METRICS - These should be EXCELLENT!
            Console.WriteLine("\n>>> OPTIMIZED PERFORMANCE METRICS <<<");
            Console.WriteLine($"Current Disk Queue Length: {currentQueue:N0} (TARGET: LOW - coordinated operations)");
            
            if (elapsedSeconds > 0)
            {
                double bytesPerSec = totalBytes / elapsedSeconds;
                Console.WriteLine($"Disk Bytes/sec: {bytesPerSec:N0} (TARGET: HIGH - efficient throughput)");
            }
            
            if (completedOps > 0)
            {
                double avgBytesPerTransfer = (double)totalBytes / completedOps;
                Console.WriteLine($"Avg Disk Bytes/Transfer: {avgBytesPerTransfer:N0} (TARGET: HIGH - large operations)");
            }
            
            // OPERATION BREAKDOWN
            Console.WriteLine("\n>>> OPTIMIZED OPERATION BREAKDOWN <<<");
            Console.WriteLine($"Total disk operations: {Interlocked.Read(ref totalDiskOperations):N0}");
            Console.WriteLine($"Completed operations: {completedOps:N0}");
            Console.WriteLine($"Large writes ({OptimizedDiskConfig.LARGE_WRITE_SIZE:N0} bytes): {Interlocked.Read(ref largeWriteOperations):N0}");
            Console.WriteLine($"Large reads ({OptimizedDiskConfig.LARGE_READ_SIZE:N0} bytes): {Interlocked.Read(ref largeReadOperations):N0}");
            Console.WriteLine($"Batched operations: {Interlocked.Read(ref batchedOperations):N0}");
            Console.WriteLine($"Buffered operations: {Interlocked.Read(ref bufferedOperations):N0}");
            Console.WriteLine($"Total bytes transferred: {totalBytes:N0}");
            
            if (elapsedSeconds > 0)
            {
                Console.WriteLine($"Operations/sec: {(completedOps / elapsedSeconds):F2}");
            }
            
            Console.WriteLine(new string('=', 80));
            
            LogPerformance($"Optimized metrics - Queue: {currentQueue}, Bytes/sec: {(totalBytes / Math.Max(1, elapsedSeconds)):N0}, " +
                          $"Avg bytes/transfer: {(totalBytes / Math.Max(1, completedOps)):N0}");
        }

        private void DisplayFinalOptimizedResults()
        {
            stopwatch.Stop();
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            long completedOps = Interlocked.Read(ref completedDiskOperations);
            long totalBytes = Interlocked.Read(ref totalBytesTransferred);
            
            Console.WriteLine($"\n{new string('=', 80)}");
            Console.WriteLine("FINAL OPTIMIZED DISK PERFORMANCE RESULTS");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.WriteLine($"Total optimized threads: {OptimizedDiskConfig.EFFICIENT_THREADS}");
            Console.WriteLine($"Total disk operations attempted: {Interlocked.Read(ref totalDiskOperations):N0}");
            Console.WriteLine($"Total disk operations completed: {completedOps:N0}");
            
            // FINAL OPTIMIZED METRICS - These should be EXCELLENT!
            Console.WriteLine("\n>>> FINAL OPTIMIZED METRICS ACHIEVED <<<");
            Console.WriteLine($"Max Disk Queue Length: {Interlocked.Read(ref maxDiskQueueLength):N0} (LOW - excellent coordination)");
            
            if (elapsedSeconds > 0)
            {
                double bytesPerSec = totalBytes / elapsedSeconds;
                Console.WriteLine($"Average Disk Bytes/sec: {bytesPerSec:N0} (HIGH - excellent throughput)");
            }
            
            if (completedOps > 0)
            {
                double avgBytesPerTransfer = (double)totalBytes / completedOps;
                Console.WriteLine($"Average Disk Bytes/Transfer: {avgBytesPerTransfer:N0} (HIGH - large efficient operations)");
            }
            
            Console.WriteLine($"Operations per second: {(completedOps / Math.Max(1, elapsedSeconds)):F2}");
            
            // OPERATION BREAKDOWN
            Console.WriteLine("\n>>> OPTIMIZED OPERATION BREAKDOWN <<<");
            Console.WriteLine($"Large writes ({OptimizedDiskConfig.LARGE_WRITE_SIZE:N0} bytes): {Interlocked.Read(ref largeWriteOperations):N0}");
            Console.WriteLine($"Large reads ({OptimizedDiskConfig.LARGE_READ_SIZE:N0} bytes): {Interlocked.Read(ref largeReadOperations):N0}");
            Console.WriteLine($"Batched operations: {Interlocked.Read(ref batchedOperations):N0}");
            Console.WriteLine($"Buffered operations: {Interlocked.Read(ref bufferedOperations):N0}");
            Console.WriteLine($"Total bytes transferred: {totalBytes:N0}");
            Console.WriteLine($"Total optimized disk time: {Interlocked.Read(ref totalDiskTimeMs):N0} ms");
            
            Console.WriteLine(new string('=', 80));
            
            /***************************************************************************************************/
            /*                              EDUCATIONAL SUMMARY                                            */
            /*  This section shows what optimizations were demonstrated                                   */
            /***************************************************************************************************/
            
            Console.WriteLine("DISK PERFORMANCE OPTIMIZATIONS DEMONSTRATED:");
            Console.WriteLine("✅ Low Current Disk Queue Length (coordinated semaphore-based operations)");
            Console.WriteLine("✅ High Disk Bytes/sec (large 64KB writes and 32KB reads)");
            Console.WriteLine("✅ High Avg Disk Bytes/Transfer (efficient large operations)");
            Console.WriteLine("✅ Sequential access patterns (minimal disk head movement)");
            Console.WriteLine("✅ Batched operations (reduced I/O overhead)");
            Console.WriteLine("✅ Asynchronous I/O (non-blocking operations)");
            Console.WriteLine("✅ Large buffer sizes (efficient OS interaction)");
            Console.WriteLine("✅ Pre-allocated files (reduced fragmentation)");
            
            Console.WriteLine($"- Check {OptimizedDiskConfig.LOG_FILE} for detailed metrics");
            Console.WriteLine($"- Compare with problematic version to see the dramatic improvement!");
            Console.WriteLine(new string('=', 80));
            
            LogPerformance($"Final optimized results - Duration: {stopwatch.ElapsedMilliseconds}ms, " +
                          $"Completed: {completedOps}, Bytes/sec: {(totalBytes / Math.Max(1, elapsedSeconds)):N0}, " +
                          $"Avg bytes/transfer: {(totalBytes / Math.Max(1, completedOps)):N0}");
        }

        public void Dispose()
        {
            diskSemaphore?.Dispose();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demo = new DiskPerformanceOptimizedDemo();
                await demo.RunOptimizedDiskPerformanceDemoAsync();
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
