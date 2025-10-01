#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <thread>
#include <mutex>
#include <chrono>
#include <random>
#include <atomic>
#include <sstream>
#include <algorithm>
#include <queue>
#include <conio.h>  // For _kbhit() on Windows
#include <cstdio>   // For remove()
#include <future>
#include <unordered_map>
#include <condition_variable>
#include <iomanip>

// ====================================================================
// DATABASE I/O INTENSIVE PARAMETERS - SIMULATE DATABASE WORKLOADS
// ====================================================================
const int NUM_DATABASE_THREADS = 8;               // Concurrent database connections
const int NUM_LOGGER_THREADS = 3;                 // Transaction log writers
const int NUM_CHECKPOINT_THREADS = 2;             // Background checkpoint threads
const int TRANSACTIONS_PER_THREAD = 200;          // Database transactions per thread
const int LOG_ENTRIES_PER_TRANSACTION = 5;        // Log entries per transaction
const int DATABASE_PAGES = 1000;                  // Number of database pages
const int PAGE_SIZE_BYTES = 8192;                 // Standard database page size (8KB)
const int LOG_BUFFER_SIZE = 4096;                 // Transaction log buffer size
const int CHECKPOINT_INTERVAL_MS = 2000;          // Checkpoint every 2 seconds
const int TRANSACTION_DELAY_MS = 10;              // Delay between transactions
const std::string DATABASE_DIRECTORY = "database_io_test/";
const std::string DATABASE_FILE = "main_database.db";
const std::string TRANSACTION_LOG = "transaction.log";
const std::string CHECKPOINT_LOG = "checkpoint.log";
const std::string PERFORMANCE_LOG = "database_performance.log";
const bool ENABLE_WRITE_AHEAD_LOGGING = true;     // Enable WAL (problematic implementation)
const bool ENABLE_CONCURRENT_READS = true;        // Multiple readers
const bool ENABLE_CONCURRENT_WRITES = true;       // Multiple writers (problematic)
const bool ENABLE_CHECKPOINT_OPERATIONS = true;   // Background checkpoints
const bool ENABLE_LOCK_CONTENTION = true;         // Simulate lock contention
// ====================================================================

// Database transaction structure
struct DatabaseTransaction {
    int transactionId;
    int threadId;
    std::string operation;  // INSERT, UPDATE, DELETE, SELECT
    int pageId;
    std::string data;
    std::chrono::high_resolution_clock::time_point timestamp;
    bool committed;
};

// Database page structure
struct DatabasePage {
    int pageId;
    std::vector<char> data;
    bool dirty;
    std::chrono::high_resolution_clock::time_point lastModified;
    int lockCount;  // Problematic: not thread-safe
};

class DatabaseIOIntensiveDemo {
private:
    std::atomic<long long> totalTransactions{0};
    std::atomic<long long> totalLogWrites{0};
    std::atomic<long long> totalPageReads{0};
    std::atomic<long long> totalPageWrites{0};
    std::atomic<long long> totalCheckpoints{0};
    std::atomic<long long> errorCount{0};
    std::atomic<long long> lockContentions{0};
    
    mutable std::mutex logMutex;
    mutable std::mutex databaseMutex;  // PROBLEM: Single mutex for entire database
    std::chrono::high_resolution_clock::time_point startTime;
    std::random_device rd;
    std::mt19937 random;
    
    // Problematic data structures - not properly synchronized
    std::vector<DatabasePage> databasePages;
    std::queue<DatabaseTransaction> transactionQueue;  // PROBLEM: Not thread-safe
    std::vector<std::string> transactionLog;           // PROBLEM: Not thread-safe
    std::unordered_map<int, bool> pageLocks;          // PROBLEM: Not thread-safe
    
    std::atomic<bool> userStopped{false};
    std::atomic<int> activeTransactions{0};
    
    void logPerformance(const std::string& message) {
        // PROBLEM: Excessive logging without buffering
        std::lock_guard<std::mutex> lock(logMutex);
        std::ofstream logFile(PERFORMANCE_LOG, std::ios::app);
        if (logFile.is_open()) {
            auto now = std::chrono::high_resolution_clock::now();
            auto timestamp = std::chrono::duration_cast<std::chrono::milliseconds>(
                now.time_since_epoch()).count();
            logFile << "[" << timestamp << "] " << message << std::endl;
            logFile.flush();  // PROBLEM: Excessive flushing
            logFile.close();
        }
    }
    
    std::vector<char> generateDatabasePageData(int pageId, int threadId) {
        std::stringstream ss;
        
        // Simulate database page header
        ss << "PAGE_ID:" << std::setfill('0') << std::setw(8) << pageId << "|";
        ss << "THREAD:" << threadId << "|";
        ss << "TIMESTAMP:" << std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::high_resolution_clock::now().time_since_epoch()).count() << "|";
        
        std::string header = ss.str();
        std::vector<char> pageData;
        pageData.reserve(PAGE_SIZE_BYTES);
        
        // Add header
        pageData.insert(pageData.end(), header.begin(), header.end());
        
        // Fill rest with simulated database records
        for (size_t i = header.length(); i < PAGE_SIZE_BYTES; ++i) {
            if (i % 100 == 99) {
                pageData.push_back('\n');
            } else if (i % 50 == 49) {
                pageData.push_back('|');
            } else {
                pageData.push_back(static_cast<char>('A' + (i % 26)));
            }
        }
        
        return pageData;
    }
    
    // PROBLEM: Write-Ahead Logging without proper synchronization
    void writeTransactionLogUnsafe(const DatabaseTransaction& transaction) {
        try {
            // PROBLEM: Direct file I/O without buffering
            std::ofstream logFile(DATABASE_DIRECTORY + TRANSACTION_LOG, std::ios::app);
            if (logFile.is_open()) {
                logFile << "TXN:" << transaction.transactionId 
                       << "|THREAD:" << transaction.threadId
                       << "|OP:" << transaction.operation
                       << "|PAGE:" << transaction.pageId
                       << "|DATA_SIZE:" << transaction.data.length()
                       << "|TIMESTAMP:" << std::chrono::duration_cast<std::chrono::milliseconds>(
                           transaction.timestamp.time_since_epoch()).count()
                       << "|COMMITTED:" << (transaction.committed ? "YES" : "NO")
                       << std::endl;
                logFile.flush();  // PROBLEM: Immediate flush for every log entry
                logFile.close();
                
                totalLogWrites++;
                
                std::cout << "[LOG] TXN " << transaction.transactionId 
                         << " (" << transaction.operation << ") - Thread " << transaction.threadId << std::endl;
            }
        } catch (const std::exception& e) {
            errorCount++;
            logPerformance("ERROR in transaction logging: " + std::string(e.what()));
        }
    }
    
    // PROBLEM: Unsafe database page access
    void readDatabasePageUnsafe(int pageId, int threadId) {
        try {
            // PROBLEM: No proper locking mechanism
            if (pageLocks[pageId]) {
                lockContentions++;
                std::this_thread::sleep_for(std::chrono::milliseconds(1)); // Simulate contention
            }
            
            pageLocks[pageId] = true;  // PROBLEM: Race condition here
            
            std::string filename = DATABASE_DIRECTORY + "page_" + std::to_string(pageId) + ".dbp";
            std::ifstream pageFile(filename, std::ios::binary);
            
            if (pageFile.is_open()) {
                char buffer[PAGE_SIZE_BYTES];
                pageFile.read(buffer, sizeof(buffer));
                pageFile.close();
                
                totalPageReads++;
                
                std::cout << "[READ] Page " << pageId << " by Thread " << threadId << std::endl;
                
                // Simulate processing time
                std::this_thread::sleep_for(std::chrono::microseconds(100));
            }
            
            pageLocks[pageId] = false;  // PROBLEM: Race condition here too
            
        } catch (const std::exception& e) {
            errorCount++;
            logPerformance("ERROR in page read: " + std::string(e.what()));
        }
    }
    
    // PROBLEM: Unsafe database page write
    void writeDatabasePageUnsafe(int pageId, int threadId) {
        try {
            // PROBLEM: No proper locking mechanism
            if (pageLocks[pageId]) {
                lockContentions++;
                std::this_thread::sleep_for(std::chrono::milliseconds(2)); // Simulate contention
            }
            
            pageLocks[pageId] = true;  // PROBLEM: Race condition
            
            auto pageData = generateDatabasePageData(pageId, threadId);
            
            std::string filename = DATABASE_DIRECTORY + "page_" + std::to_string(pageId) + ".dbp";
            std::ofstream pageFile(filename, std::ios::binary);
            
            if (pageFile.is_open()) {
                pageFile.write(pageData.data(), pageData.size());
                pageFile.flush();  // PROBLEM: Immediate flush
                pageFile.close();
                
                totalPageWrites++;
                
                std::cout << "[WRITE] Page " << pageId << " by Thread " << threadId << std::endl;
            }
            
            pageLocks[pageId] = false;  // PROBLEM: Race condition
            
        } catch (const std::exception& e) {
            errorCount++;
            logPerformance("ERROR in page write: " + std::string(e.what()));
        }
    }
    
    // PROBLEM: Database transactions without proper ACID properties
    void performDatabaseTransactionUnsafe(int threadId) {
        std::uniform_int_distribution<> opDis(0, 3);
        std::uniform_int_distribution<> pageDis(0, DATABASE_PAGES - 1);
        
        for (int txn = 0; txn < TRANSACTIONS_PER_THREAD && !userStopped; ++txn) {
            try {
                DatabaseTransaction transaction;
                transaction.transactionId = threadId * 1000 + txn;
                transaction.threadId = threadId;
                transaction.pageId = pageDis(random);
                transaction.timestamp = std::chrono::high_resolution_clock::now();
                transaction.committed = false;
                
                // Determine operation type
                int opType = opDis(random);
                switch (opType) {
                    case 0: transaction.operation = "SELECT"; break;
                    case 1: transaction.operation = "INSERT"; break;
                    case 2: transaction.operation = "UPDATE"; break;
                    case 3: transaction.operation = "DELETE"; break;
                }
                
                transaction.data = "DATA_" + std::to_string(transaction.transactionId);
                
                activeTransactions++;
                
                // PROBLEM: Write-ahead logging without proper ordering
                if (ENABLE_WRITE_AHEAD_LOGGING) {
                    writeTransactionLogUnsafe(transaction);
                }
                
                // PROBLEM: Database operations without proper isolation
                if (transaction.operation == "SELECT") {
                    readDatabasePageUnsafe(transaction.pageId, threadId);
                } else {
                    // INSERT, UPDATE, DELETE all require page writes
                    writeDatabasePageUnsafe(transaction.pageId, threadId);
                }
                
                // PROBLEM: Commit without ensuring durability
                transaction.committed = true;
                if (ENABLE_WRITE_AHEAD_LOGGING) {
                    writeTransactionLogUnsafe(transaction);  // Log commit
                }
                
                totalTransactions++;
                activeTransactions--;
                
                // PROBLEM: Inconsistent delays
                std::this_thread::sleep_for(std::chrono::milliseconds(TRANSACTION_DELAY_MS));
                
            } catch (const std::exception& e) {
                errorCount++;
                activeTransactions--;
                logPerformance("ERROR in database transaction: " + std::string(e.what()));
            }
        }
    }
    
    // PROBLEM: Checkpoint operations interfering with normal operations
    void performCheckpointOperationsUnsafe(int threadId) {
        while (!userStopped) {
            try {
                std::this_thread::sleep_for(std::chrono::milliseconds(CHECKPOINT_INTERVAL_MS));
                
                if (userStopped) break;
                
                std::cout << "[CHECKPOINT] Starting checkpoint operation - Thread " << threadId << std::endl;
                
                // PROBLEM: Checkpoint blocks all other operations
                std::lock_guard<std::mutex> lock(databaseMutex);
                
                // PROBLEM: Checkpoint writes all pages without optimization
                for (int pageId = 0; pageId < DATABASE_PAGES && !userStopped; ++pageId) {
                    std::string filename = DATABASE_DIRECTORY + "page_" + std::to_string(pageId) + ".dbp";
                    
                    // Force write all pages (even clean ones)
                    auto pageData = generateDatabasePageData(pageId, threadId);
                    std::ofstream pageFile(filename, std::ios::binary);
                    if (pageFile.is_open()) {
                        pageFile.write(pageData.data(), pageData.size());
                        pageFile.flush();
                        pageFile.close();
                    }
                    
                    // PROBLEM: No batching, individual I/O for each page
                    if (pageId % 100 == 99) {
                        std::cout << "[CHECKPOINT] Processed " << (pageId + 1) << " pages" << std::endl;
                    }
                }
                
                // Write checkpoint log
                std::ofstream checkpointFile(DATABASE_DIRECTORY + CHECKPOINT_LOG, std::ios::app);
                if (checkpointFile.is_open()) {
                    auto now = std::chrono::high_resolution_clock::now();
                    auto timestamp = std::chrono::duration_cast<std::chrono::milliseconds>(
                        now.time_since_epoch()).count();
                    checkpointFile << "CHECKPOINT:" << timestamp 
                                  << "|THREAD:" << threadId 
                                  << "|PAGES:" << DATABASE_PAGES << std::endl;
                    checkpointFile.flush();
                    checkpointFile.close();
                }
                
                totalCheckpoints++;
                
                std::cout << "[CHECKPOINT] Completed checkpoint operation - Thread " << threadId << std::endl;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in checkpoint operation: " + std::string(e.what()));
            }
        }
    }
    
    // PROBLEM: Concurrent readers without proper read locks
    void performConcurrentReadsUnsafe(int threadId) {
        std::uniform_int_distribution<> pageDis(0, DATABASE_PAGES - 1);
        
        for (int read = 0; read < TRANSACTIONS_PER_THREAD * 2 && !userStopped; ++read) {
            try {
                int pageId = pageDis(random);
                
                // PROBLEM: Multiple readers can interfere with writers
                readDatabasePageUnsafe(pageId, threadId);
                
                std::this_thread::sleep_for(std::chrono::microseconds(500));
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in concurrent read: " + std::string(e.what()));
            }
        }
    }
    
public:
    DatabaseIOIntensiveDemo() : random(rd()) {
        startTime = std::chrono::high_resolution_clock::now();
        
        // Initialize database pages
        databasePages.resize(DATABASE_PAGES);
        for (int i = 0; i < DATABASE_PAGES; ++i) {
            databasePages[i].pageId = i;
            databasePages[i].data = generateDatabasePageData(i, 0);
            databasePages[i].dirty = false;
            databasePages[i].lockCount = 0;
            pageLocks[i] = false;
        }
        
        // Create database directory
        #ifdef _WIN32
            system(("mkdir " + DATABASE_DIRECTORY + " 2>nul").c_str());
        #else
            system(("mkdir -p " + DATABASE_DIRECTORY).c_str());
        #endif
        
        // Initialize log files
        try {
            std::ofstream perfLog(PERFORMANCE_LOG, std::ios::trunc);
            if (perfLog.is_open()) {
                perfLog << "=== DATABASE I/O INTENSIVE PERFORMANCE LOG ===\n";
                perfLog.close();
            }
            
            std::ofstream txnLog(DATABASE_DIRECTORY + TRANSACTION_LOG, std::ios::trunc);
            if (txnLog.is_open()) {
                txnLog << "=== TRANSACTION LOG ===\n";
                txnLog.close();
            }
            
            std::ofstream chkLog(DATABASE_DIRECTORY + CHECKPOINT_LOG, std::ios::trunc);
            if (chkLog.is_open()) {
                chkLog << "=== CHECKPOINT LOG ===\n";
                chkLog.close();
            }
        } catch (const std::exception& ex) {
            std::cout << "Log file initialization error: " << ex.what() << std::endl;
        }
        
        logPerformance("Database I/O Intensive Demo initialized");
    }
    
    void runDatabaseIOIntensiveDemo() {
        std::cout << "=== DATABASE I/O INTENSIVE DEMONSTRATION ===" << std::endl;
        std::cout << "This program simulates PROBLEMATIC database I/O patterns:" << std::endl;
        std::cout << "1. Unsafe Write-Ahead Logging (WAL) implementation" << std::endl;
        std::cout << "2. Race conditions in page locking" << std::endl;
        std::cout << "3. Blocking checkpoint operations" << std::endl;
        std::cout << "4. Concurrent read/write conflicts" << std::endl;
        std::cout << "5. Excessive I/O flushing and logging" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "DATABASE PARAMETERS:" << std::endl;
        std::cout << "- Database threads: " << NUM_DATABASE_THREADS << std::endl;
        std::cout << "- Logger threads: " << NUM_LOGGER_THREADS << std::endl;
        std::cout << "- Checkpoint threads: " << NUM_CHECKPOINT_THREADS << std::endl;
        std::cout << "- Transactions per thread: " << TRANSACTIONS_PER_THREAD << std::endl;
        std::cout << "- Database pages: " << DATABASE_PAGES << std::endl;
        std::cout << "- Page size: " << PAGE_SIZE_BYTES << " bytes" << std::endl;
        std::cout << "- Checkpoint interval: " << CHECKPOINT_INTERVAL_MS << " ms" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "WARNING: This simulates problematic database I/O patterns!" << std::endl;
        std::cout << "Press any key to stop the demonstration..." << std::endl;
        std::cout << std::string(70, '-') << std::endl;
        
        // Start monitoring for user input
        auto keyTask = std::async(std::launch::async, [this]() {
            _getch();
            userStopped = true;
            std::cout << "\n>>> User requested stop. Finishing current operations..." << std::endl;
        });
        
        std::vector<std::future<void>> tasks;
        
        // Launch database transaction threads
        for (int i = 0; i < NUM_DATABASE_THREADS; ++i) {
            tasks.emplace_back(std::async(std::launch::async, [this, i]() {
                std::cout << "  Database Thread " << i << " started - TRANSACTION PROCESSING" << std::endl;
                
                while (!userStopped) {
                    performDatabaseTransactionUnsafe(i);
                    std::this_thread::sleep_for(std::chrono::milliseconds(100));
                }
                
                std::cout << "  Database Thread " << i << " completed" << std::endl;
            }));
        }
        
        // Launch checkpoint threads
        if (ENABLE_CHECKPOINT_OPERATIONS) {
            for (int i = 0; i < NUM_CHECKPOINT_THREADS; ++i) {
                tasks.emplace_back(std::async(std::launch::async, [this, i]() {
                    std::cout << "  Checkpoint Thread " << i << " started - BACKGROUND CHECKPOINTS" << std::endl;
                    performCheckpointOperationsUnsafe(i);
                    std::cout << "  Checkpoint Thread " << i << " completed" << std::endl;
                }));
            }
        }
        
        // Launch concurrent reader threads
        if (ENABLE_CONCURRENT_READS) {
            for (int i = 0; i < NUM_LOGGER_THREADS; ++i) {
                tasks.emplace_back(std::async(std::launch::async, [this, i]() {
                    std::cout << "  Reader Thread " << i << " started - CONCURRENT READS" << std::endl;
                    
                    while (!userStopped) {
                        performConcurrentReadsUnsafe(i + 100);
                        std::this_thread::sleep_for(std::chrono::milliseconds(50));
                    }
                    
                    std::cout << "  Reader Thread " << i << " completed" << std::endl;
                }));
            }
        }
        
        // Performance monitoring task
        auto perfTask = std::async(std::launch::async, [this]() {
            while (!userStopped) {
                std::this_thread::sleep_for(std::chrono::seconds(3));
                if (!userStopped) {
                    displayRealTimePerformance();
                }
            }
        });
        
        // Wait for user to stop
        keyTask.wait();
        userStopped = true;
        
        // Wait for all tasks to complete
        for (auto& task : tasks) {
            task.wait();
        }
        
        perfTask.wait();
        
        displayFinalResults();
        
        std::cout << "\nDatabase I/O intensive demonstration completed." << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
    }
    
private:
    void displayRealTimePerformance() {
        auto currentTime = std::chrono::high_resolution_clock::now();
        auto elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(currentTime - startTime);
        double elapsedSeconds = elapsed.count() / 1000.0;
        
        std::cout << "\n" << std::string(60, '=') << std::endl;
        std::cout << "REAL-TIME DATABASE I/O PERFORMANCE" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Running time: " << std::fixed << std::setprecision(1) << elapsedSeconds << " seconds" << std::endl;
        std::cout << "Total transactions: " << totalTransactions.load() << std::endl;
        std::cout << "Active transactions: " << activeTransactions.load() << std::endl;
        std::cout << "Transaction log writes: " << totalLogWrites.load() << std::endl;
        std::cout << "Database page reads: " << totalPageReads.load() << std::endl;
        std::cout << "Database page writes: " << totalPageWrites.load() << std::endl;
        std::cout << "Checkpoint operations: " << totalCheckpoints.load() << std::endl;
        std::cout << "Lock contentions: " << lockContentions.load() << std::endl;
        std::cout << "Transactions/sec: " << std::fixed << std::setprecision(2) << (totalTransactions.load() / elapsedSeconds) << std::endl;
        std::cout << "Page I/O operations/sec: " << std::fixed << std::setprecision(2) << ((totalPageReads.load() + totalPageWrites.load()) / elapsedSeconds) << std::endl;
        std::cout << "Errors: " << errorCount.load() << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        
        logPerformance("Real-time stats - TXN: " + std::to_string(totalTransactions.load()) + 
                      ", PageR: " + std::to_string(totalPageReads.load()) +
                      ", PageW: " + std::to_string(totalPageWrites.load()) +
                      ", Locks: " + std::to_string(lockContentions.load()));
    }
    
    void displayFinalResults() {
        auto endTime = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime);
        
        std::cout << "\n" << std::string(70, '=') << std::endl;
        std::cout << "FINAL DATABASE I/O INTENSIVE RESULTS" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "Total execution time: " << duration.count() << " ms" << std::endl;
        std::cout << "Database threads: " << NUM_DATABASE_THREADS << std::endl;
        std::cout << "Total transactions processed: " << totalTransactions.load() << std::endl;
        std::cout << "Total transaction log writes: " << totalLogWrites.load() << std::endl;
        std::cout << "Total database page reads: " << totalPageReads.load() << std::endl;
        std::cout << "Total database page writes: " << totalPageWrites.load() << std::endl;
        std::cout << "Total checkpoint operations: " << totalCheckpoints.load() << std::endl;
        std::cout << "Total lock contentions: " << lockContentions.load() << std::endl;
        std::cout << "Average transactions/sec: " << (totalTransactions.load() / (duration.count() / 1000.0)) << std::endl;
        std::cout << "Average page I/O ops/sec: " << ((totalPageReads.load() + totalPageWrites.load()) / (duration.count() / 1000.0)) << std::endl;
        std::cout << "Total errors encountered: " << errorCount.load() << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "DATABASE I/O PROBLEMS DEMONSTRATED:" << std::endl;
        std::cout << "❌ Write-Ahead Logging without proper synchronization" << std::endl;
        std::cout << "❌ Race conditions in page locking mechanisms" << std::endl;
        std::cout << "❌ Blocking checkpoint operations" << std::endl;
        std::cout << "❌ Concurrent read/write conflicts" << std::endl;
        std::cout << "❌ Excessive I/O flushing and immediate writes" << std::endl;
        std::cout << "❌ Lock contention and poor concurrency control" << std::endl;
        std::cout << "- Check " << PERFORMANCE_LOG << " for detailed metrics" << std::endl;
        std::cout << "- Check " << DATABASE_DIRECTORY << " for transaction and checkpoint logs" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        
        logPerformance("Final results - Duration: " + std::to_string(duration.count()) + "ms, " +
                      "TXN: " + std::to_string(totalTransactions.load()) + ", " +
                      "Errors: " + std::to_string(errorCount.load()) + ", " +
                      "Contentions: " + std::to_string(lockContentions.load()));
    }
};

int main() {
    try {
        DatabaseIOIntensiveDemo demo;
        demo.runDatabaseIOIntensiveDemo();
    } catch (const std::exception& e) {
        std::cerr << "Fatal error: " << e.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
        return 1;
    }
    
    return 0;
}
