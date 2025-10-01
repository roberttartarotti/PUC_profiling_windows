using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace DatabaseIOIntensiveDemo
{
    // ====================================================================
    // DATABASE I/O INTENSIVE PARAMETERS - SIMULATE DATABASE WORKLOADS
    // ====================================================================
    public static class DatabaseConfig
    {
        public const int NUM_DATABASE_THREADS = 8;               // Concurrent database connections
        public const int NUM_LOGGER_THREADS = 3;                 // Transaction log writers
        public const int NUM_CHECKPOINT_THREADS = 2;             // Background checkpoint threads
        public const int TRANSACTIONS_PER_THREAD = 200;          // Database transactions per thread
        public const int LOG_ENTRIES_PER_TRANSACTION = 5;        // Log entries per transaction
        public const int DATABASE_PAGES = 1000;                  // Number of database pages
        public const int PAGE_SIZE_BYTES = 8192;                 // Standard database page size (8KB)
        public const int LOG_BUFFER_SIZE = 4096;                 // Transaction log buffer size
        public const int CHECKPOINT_INTERVAL_MS = 2000;          // Checkpoint every 2 seconds
        public const int TRANSACTION_DELAY_MS = 10;              // Delay between transactions
        public const string DATABASE_DIRECTORY = "database_io_test/";
        public const string DATABASE_FILE = "main_database.db";
        public const string TRANSACTION_LOG = "transaction.log";
        public const string CHECKPOINT_LOG = "checkpoint.log";
        public const string PERFORMANCE_LOG = "database_performance.log";
        public const bool ENABLE_WRITE_AHEAD_LOGGING = true;     // Enable WAL (problematic implementation)
        public const bool ENABLE_CONCURRENT_READS = true;        // Multiple readers
        public const bool ENABLE_CONCURRENT_WRITES = true;       // Multiple writers (problematic)
        public const bool ENABLE_CHECKPOINT_OPERATIONS = true;   // Background checkpoints
        public const bool ENABLE_LOCK_CONTENTION = true;         // Simulate lock contention
    }
    // ====================================================================

    // Database transaction structure
    public class DatabaseTransaction
    {
        public int TransactionId { get; set; }
        public int ThreadId { get; set; }
        public string Operation { get; set; }  // INSERT, UPDATE, DELETE, SELECT
        public int PageId { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Committed { get; set; }
    }

    // Database page structure
    public class DatabasePage
    {
        public int PageId { get; set; }
        public byte[] Data { get; set; }
        public bool Dirty { get; set; }
        public DateTime LastModified { get; set; }
        public int LockCount { get; set; }  // PROBLEM: Not thread-safe
    }

    class DatabaseIOIntensiveDemo
    {
        private long totalTransactions = 0;
        private long totalLogWrites = 0;
        private long totalPageReads = 0;
        private long totalPageWrites = 0;
        private long totalCheckpoints = 0;
        private long errorCount = 0;
        private long lockContentions = 0;
        
        private readonly object logLock = new object();
        private readonly object databaseLock = new object();  // PROBLEM: Single lock for entire database
        private readonly Stopwatch stopwatch;
        private readonly Random random;
        
        // PROBLEMATIC data structures - not properly synchronized
        private readonly List<DatabasePage> databasePages;
        private readonly Queue<DatabaseTransaction> transactionQueue;  // PROBLEM: Not thread-safe
        private readonly List<string> transactionLog;                  // PROBLEM: Not thread-safe
        private readonly Dictionary<int, bool> pageLocks;             // PROBLEM: Not thread-safe
        
        private volatile bool userStopped = false;
        private long activeTransactions = 0;

        public DatabaseIOIntensiveDemo()
        {
            stopwatch = Stopwatch.StartNew();
            random = new Random();
            
            // Initialize problematic data structures
            databasePages = new List<DatabasePage>();
            transactionQueue = new Queue<DatabaseTransaction>();
            transactionLog = new List<string>();
            pageLocks = new Dictionary<int, bool>();
            
            // Initialize database pages
            for (int i = 0; i < DatabaseConfig.DATABASE_PAGES; i++)
            {
                databasePages.Add(new DatabasePage
                {
                    PageId = i,
                    Data = GenerateDatabasePageData(i, 0),
                    Dirty = false,
                    LockCount = 0
                });
                pageLocks[i] = false;
            }
            
            // Create database directory
            Directory.CreateDirectory(DatabaseConfig.DATABASE_DIRECTORY);
            
            // Initialize log files
            try
            {
                using (var writer = new StreamWriter(DatabaseConfig.PERFORMANCE_LOG, false))
                {
                    writer.WriteLine("=== DATABASE I/O INTENSIVE PERFORMANCE LOG ===");
                }
                
                using (var writer = new StreamWriter(Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, DatabaseConfig.TRANSACTION_LOG), false))
                {
                    writer.WriteLine("=== TRANSACTION LOG ===");
                }
                
                using (var writer = new StreamWriter(Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, DatabaseConfig.CHECKPOINT_LOG), false))
                {
                    writer.WriteLine("=== CHECKPOINT LOG ===");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log file initialization error: {ex.Message}");
            }
            
            LogPerformance("Database I/O Intensive Demo initialized");
        }

        private void LogPerformance(string message)
        {
            // PROBLEM: Excessive logging without buffering
            lock (logLock)
            {
                try
                {
                    using (var writer = new StreamWriter(DatabaseConfig.PERFORMANCE_LOG, append: true))
                    {
                        writer.WriteLine($"[{DateTimeOffset.Now.ToUnixTimeMilliseconds()}] {message}");
                        writer.Flush();  // PROBLEM: Excessive flushing
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }

        private byte[] GenerateDatabasePageData(int pageId, int threadId)
        {
            var sb = new StringBuilder();
            
            // Simulate database page header
            sb.Append($"PAGE_ID:{pageId:D8}|");
            sb.Append($"THREAD:{threadId}|");
            sb.Append($"TIMESTAMP:{DateTimeOffset.Now.ToUnixTimeMilliseconds()}|");
            
            string header = sb.ToString();
            var pageData = new List<byte>();
            
            // Add header
            pageData.AddRange(Encoding.UTF8.GetBytes(header));
            
            // Fill rest with simulated database records
            for (int i = header.Length; i < DatabaseConfig.PAGE_SIZE_BYTES; i++)
            {
                if (i % 100 == 99)
                {
                    pageData.Add((byte)'\n');
                }
                else if (i % 50 == 49)
                {
                    pageData.Add((byte)'|');
                }
                else
                {
                    pageData.Add((byte)('A' + (i % 26)));
                }
            }
            
            return pageData.ToArray();
        }

        // PROBLEM: Write-Ahead Logging without proper synchronization
        private async Task WriteTransactionLogUnsafeAsync(DatabaseTransaction transaction)
        {
            try
            {
                // PROBLEM: Direct file I/O without buffering
                string logPath = Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, DatabaseConfig.TRANSACTION_LOG);
                using (var writer = new StreamWriter(logPath, append: true))
                {
                    await writer.WriteLineAsync($"TXN:{transaction.TransactionId}" +
                                              $"|THREAD:{transaction.ThreadId}" +
                                              $"|OP:{transaction.Operation}" +
                                              $"|PAGE:{transaction.PageId}" +
                                              $"|DATA_SIZE:{transaction.Data?.Length ?? 0}" +
                                              $"|TIMESTAMP:{((DateTimeOffset)transaction.Timestamp).ToUnixTimeMilliseconds()}" +
                                              $"|COMMITTED:{(transaction.Committed ? "YES" : "NO")}");
                    await writer.FlushAsync();  // PROBLEM: Immediate flush for every log entry
                }
                
                Interlocked.Increment(ref totalLogWrites);
                
                Console.WriteLine($"[LOG] TXN {transaction.TransactionId} ({transaction.Operation}) - Thread {transaction.ThreadId}");
                
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                LogPerformance($"ERROR in transaction logging: {ex.Message}");
            }
        }

        // PROBLEM: Unsafe database page access
        private async Task ReadDatabasePageUnsafeAsync(int pageId, int threadId)
        {
            try
            {
                // PROBLEM: No proper locking mechanism
                if (pageLocks.ContainsKey(pageId) && pageLocks[pageId])
                {
                    Interlocked.Increment(ref lockContentions);
                    await Task.Delay(1); // Simulate contention
                }
                
                pageLocks[pageId] = true;  // PROBLEM: Race condition here
                
                string filename = Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, $"page_{pageId}.dbp");
                
                if (File.Exists(filename))
                {
                    byte[] buffer = await File.ReadAllBytesAsync(filename);
                    Interlocked.Increment(ref totalPageReads);
                    
                    Console.WriteLine($"[READ] Page {pageId} by Thread {threadId}");
                    
                    // Simulate processing time
                    await Task.Delay(TimeSpan.FromMicroseconds(100));
                }
                
                pageLocks[pageId] = false;  // PROBLEM: Race condition here too
                
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                LogPerformance($"ERROR in page read: {ex.Message}");
            }
        }

        // PROBLEM: Unsafe database page write
        private async Task WriteDatabasePageUnsafeAsync(int pageId, int threadId)
        {
            try
            {
                // PROBLEM: No proper locking mechanism
                if (pageLocks.ContainsKey(pageId) && pageLocks[pageId])
                {
                    Interlocked.Increment(ref lockContentions);
                    await Task.Delay(2); // Simulate contention
                }
                
                pageLocks[pageId] = true;  // PROBLEM: Race condition
                
                byte[] pageData = GenerateDatabasePageData(pageId, threadId);
                
                string filename = Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, $"page_{pageId}.dbp");
                
                using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.WriteAsync(pageData, 0, pageData.Length);
                    await fileStream.FlushAsync();  // PROBLEM: Immediate flush
                }
                
                Interlocked.Increment(ref totalPageWrites);
                
                Console.WriteLine($"[WRITE] Page {pageId} by Thread {threadId}");
                
                pageLocks[pageId] = false;  // PROBLEM: Race condition
                
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                LogPerformance($"ERROR in page write: {ex.Message}");
            }
        }

        // PROBLEM: Database transactions without proper ACID properties
        private async Task PerformDatabaseTransactionUnsafeAsync(int threadId)
        {
            string[] operations = { "SELECT", "INSERT", "UPDATE", "DELETE" };
            
            for (int txn = 0; txn < DatabaseConfig.TRANSACTIONS_PER_THREAD && !userStopped; txn++)
            {
                try
                {
                    var transaction = new DatabaseTransaction
                    {
                        TransactionId = threadId * 1000 + txn,
                        ThreadId = threadId,
                        PageId = random.Next(0, DatabaseConfig.DATABASE_PAGES),
                        Timestamp = DateTime.Now,
                        Committed = false,
                        Operation = operations[random.Next(0, operations.Length)],
                        Data = $"DATA_{threadId * 1000 + txn}"
                    };
                    
                    Interlocked.Increment(ref activeTransactions);
                    
                    // PROBLEM: Write-ahead logging without proper ordering
                    if (DatabaseConfig.ENABLE_WRITE_AHEAD_LOGGING)
                    {
                        await WriteTransactionLogUnsafeAsync(transaction);
                    }
                    
                    // PROBLEM: Database operations without proper isolation
                    if (transaction.Operation == "SELECT")
                    {
                        await ReadDatabasePageUnsafeAsync(transaction.PageId, threadId);
                    }
                    else
                    {
                        // INSERT, UPDATE, DELETE all require page writes
                        await WriteDatabasePageUnsafeAsync(transaction.PageId, threadId);
                    }
                    
                    // PROBLEM: Commit without ensuring durability
                    transaction.Committed = true;
                    if (DatabaseConfig.ENABLE_WRITE_AHEAD_LOGGING)
                    {
                        await WriteTransactionLogUnsafeAsync(transaction);  // Log commit
                    }
                    
                    Interlocked.Increment(ref totalTransactions);
                    Interlocked.Decrement(ref activeTransactions);
                    
                    // PROBLEM: Inconsistent delays
                    await Task.Delay(DatabaseConfig.TRANSACTION_DELAY_MS);
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    Interlocked.Decrement(ref activeTransactions);
                    LogPerformance($"ERROR in database transaction: {ex.Message}");
                }
            }
        }

        // PROBLEM: Checkpoint operations interfering with normal operations
        private async Task PerformCheckpointOperationsUnsafeAsync(int threadId)
        {
            while (!userStopped)
            {
                try
                {
                    await Task.Delay(DatabaseConfig.CHECKPOINT_INTERVAL_MS);
                    
                    if (userStopped) break;
                    
                    Console.WriteLine($"[CHECKPOINT] Starting checkpoint operation - Thread {threadId}");
                    
                    // PROBLEM: Checkpoint blocks all other operations
                    lock (databaseLock)
                    {
                        // PROBLEM: Checkpoint writes all pages without optimization
                        for (int pageId = 0; pageId < DatabaseConfig.DATABASE_PAGES && !userStopped; pageId++)
                        {
                            string filename = Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, $"page_{pageId}.dbp");
                            
                            // Force write all pages (even clean ones)
                            byte[] pageData = GenerateDatabasePageData(pageId, threadId);
                            File.WriteAllBytes(filename, pageData);
                            
                            // PROBLEM: No batching, individual I/O for each page
                            if (pageId % 100 == 99)
                            {
                                Console.WriteLine($"[CHECKPOINT] Processed {pageId + 1} pages");
                            }
                        }
                        
                        // Write checkpoint log
                        string checkpointPath = Path.Combine(DatabaseConfig.DATABASE_DIRECTORY, DatabaseConfig.CHECKPOINT_LOG);
                        using (var writer = new StreamWriter(checkpointPath, append: true))
                        {
                            writer.WriteLine($"CHECKPOINT:{DateTimeOffset.Now.ToUnixTimeMilliseconds()}" +
                                           $"|THREAD:{threadId}" +
                                           $"|PAGES:{DatabaseConfig.DATABASE_PAGES}");
                            writer.Flush();
                        }
                        
                        Interlocked.Increment(ref totalCheckpoints);
                        
                        Console.WriteLine($"[CHECKPOINT] Completed checkpoint operation - Thread {threadId}");
                    }
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    LogPerformance($"ERROR in checkpoint operation: {ex.Message}");
                }
            }
        }

        // PROBLEM: Concurrent readers without proper read locks
        private async Task PerformConcurrentReadsUnsafeAsync(int threadId)
        {
            for (int read = 0; read < DatabaseConfig.TRANSACTIONS_PER_THREAD * 2 && !userStopped; read++)
            {
                try
                {
                    int pageId = random.Next(0, DatabaseConfig.DATABASE_PAGES);
                    
                    // PROBLEM: Multiple readers can interfere with writers
                    await ReadDatabasePageUnsafeAsync(pageId, threadId);
                    
                    await Task.Delay(TimeSpan.FromMicroseconds(500));
                    
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    LogPerformance($"ERROR in concurrent read: {ex.Message}");
                }
            }
        }

        public async Task RunDatabaseIOIntensiveDemoAsync()
        {
            Console.WriteLine("=== DATABASE I/O INTENSIVE DEMONSTRATION (C#) ===");
            Console.WriteLine("This program simulates PROBLEMATIC database I/O patterns:");
            Console.WriteLine("1. Unsafe Write-Ahead Logging (WAL) implementation");
            Console.WriteLine("2. Race conditions in page locking");
            Console.WriteLine("3. Blocking checkpoint operations");
            Console.WriteLine("4. Concurrent read/write conflicts");
            Console.WriteLine("5. Excessive I/O flushing and logging");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("DATABASE PARAMETERS:");
            Console.WriteLine($"- Database threads: {DatabaseConfig.NUM_DATABASE_THREADS}");
            Console.WriteLine($"- Logger threads: {DatabaseConfig.NUM_LOGGER_THREADS}");
            Console.WriteLine($"- Checkpoint threads: {DatabaseConfig.NUM_CHECKPOINT_THREADS}");
            Console.WriteLine($"- Transactions per thread: {DatabaseConfig.TRANSACTIONS_PER_THREAD}");
            Console.WriteLine($"- Database pages: {DatabaseConfig.DATABASE_PAGES}");
            Console.WriteLine($"- Page size: {DatabaseConfig.PAGE_SIZE_BYTES} bytes");
            Console.WriteLine($"- Checkpoint interval: {DatabaseConfig.CHECKPOINT_INTERVAL_MS} ms");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("WARNING: This simulates problematic database I/O patterns!");
            Console.WriteLine("Press any key to stop the demonstration...");
            Console.WriteLine(new string('-', 70));

            // Start monitoring for user input
            var keyTask = Task.Run(() =>
            {
                Console.ReadKey(true);
                userStopped = true;
                Console.WriteLine("\n>>> User requested stop. Finishing current operations...");
            });

            var tasks = new List<Task>();

            // Launch database transaction threads
            for (int i = 0; i < DatabaseConfig.NUM_DATABASE_THREADS; i++)
            {
                int threadId = i; // Capture for closure
                tasks.Add(Task.Run(async () =>
                {
                    Console.WriteLine($"  Database Thread {threadId} started - TRANSACTION PROCESSING");
                    
                    while (!userStopped)
                    {
                        await PerformDatabaseTransactionUnsafeAsync(threadId);
                        await Task.Delay(100);
                    }
                    
                    Console.WriteLine($"  Database Thread {threadId} completed");
                }));
            }

            // Launch checkpoint threads
            if (DatabaseConfig.ENABLE_CHECKPOINT_OPERATIONS)
            {
                for (int i = 0; i < DatabaseConfig.NUM_CHECKPOINT_THREADS; i++)
                {
                    int threadId = i; // Capture for closure
                    tasks.Add(Task.Run(async () =>
                    {
                        Console.WriteLine($"  Checkpoint Thread {threadId} started - BACKGROUND CHECKPOINTS");
                        await PerformCheckpointOperationsUnsafeAsync(threadId);
                        Console.WriteLine($"  Checkpoint Thread {threadId} completed");
                    }));
                }
            }

            // Launch concurrent reader threads
            if (DatabaseConfig.ENABLE_CONCURRENT_READS)
            {
                for (int i = 0; i < DatabaseConfig.NUM_LOGGER_THREADS; i++)
                {
                    int threadId = i; // Capture for closure
                    tasks.Add(Task.Run(async () =>
                    {
                        Console.WriteLine($"  Reader Thread {threadId} started - CONCURRENT READS");
                        
                        while (!userStopped)
                        {
                            await PerformConcurrentReadsUnsafeAsync(threadId + 100);
                            await Task.Delay(50);
                        }
                        
                        Console.WriteLine($"  Reader Thread {threadId} completed");
                    }));
                }
            }

            // Performance monitoring task
            var perfTask = Task.Run(async () =>
            {
                while (!userStopped)
                {
                    await Task.Delay(3000);
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

            Console.WriteLine("\nDatabase I/O intensive demonstration completed.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private void DisplayRealTimePerformance()
        {
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine("REAL-TIME DATABASE I/O PERFORMANCE");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Running time: {elapsedSeconds:F1} seconds");
            Console.WriteLine($"Total transactions: {Interlocked.Read(ref totalTransactions):N0}");
            Console.WriteLine($"Active transactions: {Interlocked.Read(ref activeTransactions):N0}");
            Console.WriteLine($"Transaction log writes: {Interlocked.Read(ref totalLogWrites):N0}");
            Console.WriteLine($"Database page reads: {Interlocked.Read(ref totalPageReads):N0}");
            Console.WriteLine($"Database page writes: {Interlocked.Read(ref totalPageWrites):N0}");
            Console.WriteLine($"Checkpoint operations: {Interlocked.Read(ref totalCheckpoints):N0}");
            Console.WriteLine($"Lock contentions: {Interlocked.Read(ref lockContentions):N0}");
            Console.WriteLine($"Transactions/sec: {(Interlocked.Read(ref totalTransactions) / elapsedSeconds):F2}");
            Console.WriteLine($"Page I/O operations/sec: {((Interlocked.Read(ref totalPageReads) + Interlocked.Read(ref totalPageWrites)) / elapsedSeconds):F2}");
            Console.WriteLine($"Errors: {Interlocked.Read(ref errorCount):N0}");
            Console.WriteLine(new string('=', 60));
            
            LogPerformance($"Real-time stats - TXN: {Interlocked.Read(ref totalTransactions)}, " +
                          $"PageR: {Interlocked.Read(ref totalPageReads)}, " +
                          $"PageW: {Interlocked.Read(ref totalPageWrites)}, " +
                          $"Locks: {Interlocked.Read(ref lockContentions)}");
        }

        private void DisplayFinalResults()
        {
            stopwatch.Stop();
            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"\n{new string('=', 70)}");
            Console.WriteLine("FINAL DATABASE I/O INTENSIVE RESULTS");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"Total execution time: {stopwatch.ElapsedMilliseconds:N0} ms");
            Console.WriteLine($"Database threads: {DatabaseConfig.NUM_DATABASE_THREADS}");
            Console.WriteLine($"Total transactions processed: {Interlocked.Read(ref totalTransactions):N0}");
            Console.WriteLine($"Total transaction log writes: {Interlocked.Read(ref totalLogWrites):N0}");
            Console.WriteLine($"Total database page reads: {Interlocked.Read(ref totalPageReads):N0}");
            Console.WriteLine($"Total database page writes: {Interlocked.Read(ref totalPageWrites):N0}");
            Console.WriteLine($"Total checkpoint operations: {Interlocked.Read(ref totalCheckpoints):N0}");
            Console.WriteLine($"Total lock contentions: {Interlocked.Read(ref lockContentions):N0}");
            Console.WriteLine($"Average transactions/sec: {(Interlocked.Read(ref totalTransactions) / elapsedSeconds):F2}");
            Console.WriteLine($"Average page I/O ops/sec: {((Interlocked.Read(ref totalPageReads) + Interlocked.Read(ref totalPageWrites)) / elapsedSeconds):F2}");
            Console.WriteLine($"Total errors encountered: {Interlocked.Read(ref errorCount):N0}");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("DATABASE I/O PROBLEMS DEMONSTRATED:");
            Console.WriteLine("❌ Write-Ahead Logging without proper synchronization");
            Console.WriteLine("❌ Race conditions in page locking mechanisms");
            Console.WriteLine("❌ Blocking checkpoint operations");
            Console.WriteLine("❌ Concurrent read/write conflicts");
            Console.WriteLine("❌ Excessive I/O flushing and immediate writes");
            Console.WriteLine("❌ Lock contention and poor concurrency control");
            Console.WriteLine($"- Check {DatabaseConfig.PERFORMANCE_LOG} for detailed metrics");
            Console.WriteLine($"- Check {DatabaseConfig.DATABASE_DIRECTORY} for transaction and checkpoint logs");
            Console.WriteLine(new string('=', 70));
            
            LogPerformance($"Final results - Duration: {stopwatch.ElapsedMilliseconds}ms, " +
                          $"TXN: {Interlocked.Read(ref totalTransactions)}, " +
                          $"Errors: {Interlocked.Read(ref errorCount)}, " +
                          $"Contentions: {Interlocked.Read(ref lockContentions)}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var demo = new DatabaseIOIntensiveDemo();
                await demo.RunDatabaseIOIntensiveDemoAsync();
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
