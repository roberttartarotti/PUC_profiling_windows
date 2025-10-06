/*
 * =====================================================================================
 * DISK I/O PERFORMANCE PROBLEMS DEMONSTRATION - C# (MODULE 3, CLASS 2)
 * =====================================================================================
 * 
 * Purpose: Demonstrate severe disk I/O performance problems that will cause
 *          noticeable bottlenecks even on powerful computers
 * 
 * Educational Context:
 * - Show inefficient disk I/O patterns that cause severe performance degradation
 * - Demonstrate the impact of synchronous I/O on system performance
 * - Illustrate problems with small buffer sizes and frequent disk access
 * - Show the effects of random I/O patterns vs sequential
 * - Demonstrate the cost of not using asynchronous operations
 * 
 * Performance Problems Demonstrated:
 * - Synchronous I/O blocking threads
 * - Small buffer sizes causing excessive disk access
 * - Random I/O patterns instead of sequential
 * - No caching or buffering strategies
 * - Frequent file open/close operations
 * - No batch processing
 * - No prefetching
 * - No asynchronous operations
 * 
 * Expected Performance Impact:
 * - CPU usage: High (blocking I/O)
 * - Disk I/O: Excessive (small buffers, random access)
 * - Memory usage: Inefficient (no caching)
 * - Response time: Poor (synchronous operations)
 * - Throughput: Low (inefficient patterns)
 * 
 * =====================================================================================
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace DiskIOProblems
{
    // =====================================================================================
    // CONFIGURATION PARAMETERS - DESIGNED TO CAUSE SEVERE PERFORMANCE PROBLEMS
    // =====================================================================================
    
    public static class Config
    {
        // File and Buffer Configuration (DESIGNED TO BE INEFFICIENT)
        public const int SMALL_BUFFER_SIZE = 64;                        // Very small buffer - causes excessive disk access
        public const int MEDIUM_BUFFER_SIZE = 1024;                     // Still small for modern systems
        public const int LARGE_BUFFER_SIZE = 65536;                     // Reasonable size for comparison
        public const int FILE_COUNT = 100;                              // Many files to manage
        public const int OPERATIONS_PER_FILE = 1000;                    // Many operations per file
        public const int MAX_FILE_SIZE = 10 * 1024 * 1024;             // 10MB files
        
        // Performance Problem Settings
        public const bool USE_SYNCHRONOUS_IO = true;                    // Force synchronous I/O
        public const bool USE_SMALL_BUFFERS = true;                     // Use inefficient buffer sizes
        public const bool USE_RANDOM_ACCESS = true;                     // Random I/O patterns
        public const bool USE_FREQUENT_FILE_OPS = true;                // Open/close files frequently
        public const bool USE_NO_CACHING = true;                        // No caching strategy
        public const bool USE_NO_BATCHING = true;                       // No batch operations
        public const bool USE_NO_PREFETCHING = true;                    // No prefetching
        public const bool USE_BLOCKING_OPERATIONS = true;               // Blocking operations
        
        // Timing and Display
        public const int DISPLAY_INTERVAL_MS = 1000;                    // Show stats every second
        public const int PERFORMANCE_CHECK_INTERVAL_MS = 5000;          // Check performance every 5 seconds
        public const int STATS_RESET_INTERVAL_MS = 30000;              // Reset stats every 30 seconds
        
        // Threading Configuration (DESIGNED TO CAUSE CONTENTION)
        public const int THREAD_COUNT = 20;                             // Many threads causing contention
        public const int MAX_CONCURRENT_OPERATIONS = 50;                // High concurrency
        public const int THREAD_SLEEP_MS = 1;                          // Minimal sleep
        
        // File System Stress Settings
        public const bool CREATE_TEMP_FILES = true;                     // Create temporary files
        public const bool USE_MULTIPLE_DIRECTORIES = true;              // Spread across directories
        public const bool SIMULATE_REAL_WORKLOAD = true;                // Simulate real-world patterns
    }

    // =====================================================================================
    // INEFFICIENT DISK I/O OPERATIONS CLASS
    // =====================================================================================
    
    public class InefficientDiskIOProcessor
    {
        private readonly string _baseDirectory;
        private readonly Random _random;
        private readonly object _lockObject = new object();
        private int _operationCount = 0;
        private long _totalBytesProcessed = 0;
        private readonly Stopwatch _stopwatch;
        
        // Performance counters
        private long _fileOpenCount = 0;
        private long _fileCloseCount = 0;
        private long _readOperations = 0;
        private long _writeOperations = 0;
        private long _seekOperations = 0;
        
        public InefficientDiskIOProcessor(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
            _random = new Random();
            _stopwatch = Stopwatch.StartNew();
            
            // Create base directory
            Directory.CreateDirectory(_baseDirectory);
        }
        
        // PROBLEM 1: Synchronous I/O with small buffers
        public void PerformInefficientRead(string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateTestFile(filePath);
            }
            
            // PROBLEM: Open file for each operation (no caching)
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Interlocked.Increment(ref _fileOpenCount);
                
                // PROBLEM: Very small buffer causing excessive disk access
                byte[] buffer = new byte[Config.SMALL_BUFFER_SIZE];
                
                // PROBLEM: Random access pattern instead of sequential
                if (Config.USE_RANDOM_ACCESS)
                {
                    long fileSize = fileStream.Length;
                    if (fileSize > 0)
                    {
                        long randomPosition = _random.Next(0, (int)Math.Max(1, fileSize - Config.SMALL_BUFFER_SIZE));
                        fileStream.Seek(randomPosition, SeekOrigin.Begin);
                        Interlocked.Increment(ref _seekOperations);
                    }
                }
                
                // PROBLEM: Synchronous read blocking the thread
                int bytesRead = fileStream.Read(buffer, 0, Config.SMALL_BUFFER_SIZE);
                Interlocked.Increment(ref _readOperations);
                Interlocked.Add(ref _totalBytesProcessed, bytesRead);
                
                // PROBLEM: Process data inefficiently (no batching)
                ProcessDataInefficiently(buffer, bytesRead);
            }
            
            Interlocked.Increment(ref _fileCloseCount);
            Interlocked.Increment(ref _operationCount);
        }
        
        // PROBLEM 2: Inefficient write operations
        public void PerformInefficientWrite(string filePath, byte[] data)
        {
            // PROBLEM: Open file for each write operation
            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Interlocked.Increment(ref _fileOpenCount);
                
                // PROBLEM: Write data in very small chunks
                int offset = 0;
                while (offset < data.Length)
                {
                    int chunkSize = Math.Min(Config.SMALL_BUFFER_SIZE, data.Length - offset);
                    
                    // PROBLEM: Random position for writes
                    if (Config.USE_RANDOM_ACCESS)
                    {
                        long randomPosition = _random.Next(0, Math.Max(1, (int)fileStream.Length));
                        fileStream.Seek(randomPosition, SeekOrigin.Begin);
                        Interlocked.Increment(ref _seekOperations);
                    }
                    
                    // PROBLEM: Synchronous write blocking the thread
                    fileStream.Write(data, offset, chunkSize);
                    fileStream.Flush(); // PROBLEM: Flush after every small write
                    
                    Interlocked.Increment(ref _writeOperations);
                    Interlocked.Add(ref _totalBytesProcessed, chunkSize);
                    
                    offset += chunkSize;
                    
                    // PROBLEM: No batching - process each chunk separately
                    ProcessDataInefficiently(data, chunkSize);
                }
            }
            
            Interlocked.Increment(ref _fileCloseCount);
            Interlocked.Increment(ref _operationCount);
        }
        
        // PROBLEM 3: Inefficient file operations
        public void PerformInefficientFileOperations()
        {
            string fileName = $"temp_file_{_random.Next(1, Config.FILE_COUNT)}.dat";
            string filePath = Path.Combine(_baseDirectory, fileName);
            
            // PROBLEM: Create file if it doesn't exist (inefficient)
            if (!File.Exists(filePath))
            {
                CreateTestFile(filePath);
            }
            
            // PROBLEM: Multiple operations on same file without keeping it open
            for (int i = 0; i < Config.OPERATIONS_PER_FILE / 10; i++)
            {
                // PROBLEM: Read operation
                PerformInefficientRead(filePath);
                
                // PROBLEM: Write operation
                byte[] writeData = GenerateRandomData(Config.SMALL_BUFFER_SIZE);
                PerformInefficientWrite(filePath, writeData);
                
                // PROBLEM: File info operations
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    // PROBLEM: Access properties that require disk access
                    long length = fileInfo.Length;
                    DateTime lastWrite = fileInfo.LastWriteTime;
                }
            }
        }
        
        // PROBLEM 4: No caching or buffering
        private void ProcessDataInefficiently(byte[] data, int length)
        {
            // PROBLEM: Process data without any caching or buffering
            for (int i = 0; i < length; i++)
            {
                // PROBLEM: Inefficient processing
                byte processedByte = (byte)(data[i] ^ 0xFF);
                
                // PROBLEM: No batching - process each byte individually
                if (processedByte > 128)
                {
                    // Simulate some processing
                    Thread.Sleep(Config.THREAD_SLEEP_MS);
                }
            }
        }
        
        // PROBLEM 5: Inefficient file creation
        private void CreateTestFile(string filePath)
        {
            // PROBLEM: Create file with small writes
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                byte[] data = GenerateRandomData(Config.MAX_FILE_SIZE);
                
                // PROBLEM: Write in very small chunks
                int offset = 0;
                while (offset < data.Length)
                {
                    int chunkSize = Math.Min(Config.SMALL_BUFFER_SIZE, data.Length - offset);
                    fileStream.Write(data, offset, chunkSize);
                    fileStream.Flush(); // PROBLEM: Flush after every chunk
                    offset += chunkSize;
                }
            }
        }
        
        // PROBLEM 6: Inefficient data generation
        private byte[] GenerateRandomData(int size)
        {
            byte[] data = new byte[size];
            
            // PROBLEM: Generate data inefficiently
            for (int i = 0; i < size; i++)
            {
                data[i] = (byte)_random.Next(0, 256);
            }
            
            return data;
        }
        
        // Get performance statistics
        public (long operations, long bytesProcessed, long fileOps, long readOps, long writeOps, long seekOps, double elapsedMs) GetStats()
        {
            return (
                _operationCount,
                _totalBytesProcessed,
                _fileOpenCount + _fileCloseCount,
                _readOperations,
                _writeOperations,
                _seekOperations,
                _stopwatch.Elapsed.TotalMilliseconds
            );
        }
    }

    // =====================================================================================
    // MULTI-THREADED DISK I/O STRESS TEST
    // =====================================================================================
    
    public static class DiskIOStressTest
    {
        private static volatile bool _isRunning = true;
        private static readonly List<InefficientDiskIOProcessor> _processors = new List<InefficientDiskIOProcessor>();
        private static readonly object _statsLock = new object();
        
        public static void RunInefficientDiskIOTest()
        {
            Console.WriteLine("=== STARTING INEFFICIENT DISK I/O STRESS TEST ===");
            Console.WriteLine("WARNING: This will cause severe performance problems!");
            Console.WriteLine("Press Ctrl+C to stop the test");
            Console.WriteLine();
            
            // Create multiple processors to simulate contention
            for (int i = 0; i < Config.THREAD_COUNT; i++)
            {
                string directory = Path.Combine(Path.GetTempPath(), $"disk_io_test_{i}");
                _processors.Add(new InefficientDiskIOProcessor(directory));
            }
            
            // Start multiple threads to cause contention
            var tasks = new List<Task>();
            for (int i = 0; i < Config.MAX_CONCURRENT_OPERATIONS; i++)
            {
                int processorIndex = i % _processors.Count;
                tasks.Add(Task.Run(() => RunInefficientOperations(_processors[processorIndex])));
            }
            
            // Start performance monitoring
            var monitorTask = Task.Run(MonitorPerformance);
            
            // Wait for user to stop
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _isRunning = false;
                Console.WriteLine("\nStopping test...");
            };
            
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Test stopped by user");
            }
            
            _isRunning = false;
            monitorTask.Wait();
            
            ShowFinalResults();
        }
        
        private static void RunInefficientOperations(InefficientDiskIOProcessor processor)
        {
            while (_isRunning)
            {
                try
                {
                    // PROBLEM: Continuous inefficient operations
                    processor.PerformInefficientFileOperations();
                    
                    // PROBLEM: No intelligent scheduling
                    Thread.Sleep(Config.THREAD_SLEEP_MS);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in thread: {ex.Message}");
                }
            }
        }
        
        private static void MonitorPerformance()
        {
            var lastStats = new Dictionary<InefficientDiskIOProcessor, (long ops, long bytes, long fileOps, long readOps, long writeOps, long seekOps, double elapsed)>();
            
            while (_isRunning)
            {
                Thread.Sleep(Config.DISPLAY_INTERVAL_MS);
                
                if (!_isRunning) break;
                
                lock (_statsLock)
                {
                    Console.WriteLine($"\n=== PERFORMANCE STATISTICS (Inefficient) ===");
                    Console.WriteLine($"Time: {DateTime.Now:HH:mm:ss}");
                    
                    long totalOperations = 0;
                    long totalBytes = 0;
                    long totalFileOps = 0;
                    long totalReadOps = 0;
                    long totalWriteOps = 0;
                    long totalSeekOps = 0;
                    double totalElapsed = 0;
                    
                    foreach (var processor in _processors)
                    {
                        var stats = processor.GetStats();
                        totalOperations += stats.operations;
                        totalBytes += stats.bytesProcessed;
                        totalFileOps += stats.fileOps;
                        totalReadOps += stats.readOps;
                        totalWriteOps += stats.writeOps;
                        totalSeekOps += stats.seekOps;
                        totalElapsed = Math.Max(totalElapsed, stats.elapsedMs);
                    }
                    
                    Console.WriteLine($"Total Operations: {totalOperations:N0}");
                    Console.WriteLine($"Total Bytes Processed: {totalBytes / 1024 / 1024:N2} MB");
                    Console.WriteLine($"File Operations: {totalFileOps:N0}");
                    Console.WriteLine($"Read Operations: {totalReadOps:N0}");
                    Console.WriteLine($"Write Operations: {totalWriteOps:N0}");
                    Console.WriteLine($"Seek Operations: {totalSeekOps:N0}");
                    Console.WriteLine($"Elapsed Time: {totalElapsed / 1000:N2} seconds");
                    
                    if (totalElapsed > 0)
                    {
                        double opsPerSecond = totalOperations / (totalElapsed / 1000);
                        double mbPerSecond = (totalBytes / 1024.0 / 1024.0) / (totalElapsed / 1000);
                        Console.WriteLine($"Operations/Second: {opsPerSecond:N0}");
                        Console.WriteLine($"Throughput: {mbPerSecond:N2} MB/s");
                    }
                    
                    Console.WriteLine("PERFORMANCE PROBLEMS:");
                    Console.WriteLine("- Synchronous I/O blocking threads");
                    Console.WriteLine("- Small buffers causing excessive disk access");
                    Console.WriteLine("- Random I/O patterns instead of sequential");
                    Console.WriteLine("- No caching or buffering strategies");
                    Console.WriteLine("- Frequent file open/close operations");
                    Console.WriteLine("- No batch processing");
                    Console.WriteLine("- No prefetching");
                    Console.WriteLine("- No asynchronous operations");
                }
            }
        }
        
        private static void ShowFinalResults()
        {
            Console.WriteLine("\n=== FINAL RESULTS (Inefficient Disk I/O) ===");
            
            long totalOperations = 0;
            long totalBytes = 0;
            long totalFileOps = 0;
            long totalReadOps = 0;
            long totalWriteOps = 0;
            long totalSeekOps = 0;
            double totalElapsed = 0;
            
            foreach (var processor in _processors)
            {
                var stats = processor.GetStats();
                totalOperations += stats.operations;
                totalBytes += stats.bytesProcessed;
                totalFileOps += stats.fileOps;
                totalReadOps += stats.readOps;
                totalWriteOps += stats.writeOps;
                totalSeekOps += stats.seekOps;
                totalElapsed = Math.Max(totalElapsed, stats.elapsedMs);
            }
            
            Console.WriteLine($"Total Operations: {totalOperations:N0}");
            Console.WriteLine($"Total Bytes Processed: {totalBytes / 1024 / 1024:N2} MB");
            Console.WriteLine($"File Operations: {totalFileOps:N0}");
            Console.WriteLine($"Read Operations: {totalReadOps:N0}");
            Console.WriteLine($"Write Operations: {totalWriteOps:N0}");
            Console.WriteLine($"Seek Operations: {totalSeekOps:N0}");
            Console.WriteLine($"Elapsed Time: {totalElapsed / 1000:N2} seconds");
            
            if (totalElapsed > 0)
            {
                double opsPerSecond = totalOperations / (totalElapsed / 1000);
                double mbPerSecond = (totalBytes / 1024.0 / 1024.0) / (totalElapsed / 1000);
                Console.WriteLine($"Operations/Second: {opsPerSecond:N0}");
                Console.WriteLine($"Throughput: {mbPerSecond:N2} MB/s");
            }
            
            Console.WriteLine("\nPERFORMANCE PROBLEMS IDENTIFIED:");
            Console.WriteLine("1. Synchronous I/O causing thread blocking");
            Console.WriteLine("2. Small buffer sizes causing excessive disk access");
            Console.WriteLine("3. Random I/O patterns instead of sequential");
            Console.WriteLine("4. No caching or buffering strategies");
            Console.WriteLine("5. Frequent file open/close operations");
            Console.WriteLine("6. No batch processing");
            Console.WriteLine("7. No prefetching");
            Console.WriteLine("8. No asynchronous operations");
            Console.WriteLine("9. High thread contention");
            Console.WriteLine("10. Inefficient data processing patterns");
        }
    }

    // =====================================================================================
    // MAIN PROGRAM
    // =====================================================================================
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("                    DISK I/O PERFORMANCE PROBLEMS DEMONSTRATION");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("This program demonstrates severe disk I/O performance problems");
            Console.WriteLine("that will cause noticeable bottlenecks even on powerful computers.");
            Console.WriteLine();
            Console.WriteLine("PROBLEMS DEMONSTRATED:");
            Console.WriteLine("- Synchronous I/O blocking threads");
            Console.WriteLine("- Small buffer sizes causing excessive disk access");
            Console.WriteLine("- Random I/O patterns instead of sequential");
            Console.WriteLine("- No caching or buffering strategies");
            Console.WriteLine("- Frequent file open/close operations");
            Console.WriteLine("- No batch processing");
            Console.WriteLine("- No prefetching");
            Console.WriteLine("- No asynchronous operations");
            Console.WriteLine();
            Console.WriteLine("EXPECTED PERFORMANCE IMPACT:");
            Console.WriteLine("- CPU usage: High (blocking I/O)");
            Console.WriteLine("- Disk I/O: Excessive (small buffers, random access)");
            Console.WriteLine("- Memory usage: Inefficient (no caching)");
            Console.WriteLine("- Response time: Poor (synchronous operations)");
            Console.WriteLine("- Throughput: Low (inefficient patterns)");
            Console.WriteLine();
            Console.WriteLine("WARNING: This will cause severe performance problems!");
            Console.WriteLine("Press Ctrl+C to stop the test");
            Console.WriteLine("=====================================================================================");
            
            Console.WriteLine("\nPress ENTER to start the inefficient disk I/O test...");
            Console.ReadLine();
            
            try
            {
                DiskIOStressTest.RunInefficientDiskIOTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }
}

/*
 * =====================================================================================
 * PERFORMANCE ANALYSIS - INEFFICIENT DISK I/O VERSION
 * =====================================================================================
 * 
 * What to observe in Performance Monitor:
 * 
 * 1. CPU USAGE:
 *    - High CPU usage due to thread blocking
 *    - Many threads waiting for I/O operations
 *    - Inefficient processing patterns
 * 
 * 2. DISK I/O:
 *    - Excessive disk access due to small buffers
 *    - Random I/O patterns causing disk head movement
 *    - High number of read/write operations
 *    - Frequent file open/close operations
 * 
 * 3. MEMORY USAGE:
 *    - Inefficient memory usage due to no caching
 *    - Small buffers causing memory fragmentation
 *    - No buffering strategies
 * 
 * 4. THREADING:
 *    - Many threads blocked on I/O operations
 *    - Thread contention for file resources
 *    - Inefficient thread utilization
 * 
 * 5. PERFORMANCE METRICS:
 *    - Low operations per second
 *    - Low throughput (MB/s)
 *    - High latency for operations
 *    - Poor resource utilization
 * 
 * 6. EDUCATIONAL VALUE:
 *    - Shows real-world performance problems
 *    - Demonstrates the cost of inefficient I/O patterns
 *    - Illustrates the impact of synchronous operations
 *    - Proves the need for optimization techniques
 * 
 * =====================================================================================
 */
