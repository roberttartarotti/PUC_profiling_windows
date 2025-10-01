#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <thread>
#include <mutex>
#include <chrono>
#include <atomic>
#include <sstream>
#include <algorithm>
#include <queue>
#include <conio.h>  // For _kbhit() on Windows
#include <cstdio>   // For remove()
#include <map>

// ====================================================================
// OPTIMIZED DISK SCHEDULING PARAMETERS - PROPER I/O OPTIMIZATION
// ====================================================================
const int NUM_THREADS = 6;                     // Reduced threads for better coordination
const int OPERATIONS_PER_THREAD = 50;          // Fewer operations but more efficient
const int NUM_FILES = 100;                     // Fewer files, better organized
const int MIN_FILE_SIZE_KB = 100;              // Larger files for better sequential access
const int MAX_FILE_SIZE_KB = 500;              // Larger chunks reduce seek overhead
const int WRITE_CHUNK_SIZE = 64 * 1024;        // 64KB chunks for optimal throughput
const int READ_BUFFER_SIZE = 64 * 1024;        // Large read buffers
const int BATCH_SIZE = 10;                     // Batch operations for efficiency
const int DELAY_BETWEEN_BATCHES_MS = 50;       // Coordinated delays between batches
const std::string BASE_DIRECTORY = "optimized_disk_test/";
const std::string BASE_FILENAME = "optimized_file_";
const std::string LOG_FILE = "optimized_disk_performance.log";
const bool ENABLE_ELEVATOR_ALGORITHM = true;   // Enable elevator disk scheduling
const bool ENABLE_SEQUENTIAL_OPTIMIZATION = true; // Optimize for sequential access
const bool ENABLE_WRITE_BATCHING = true;       // Batch writes for efficiency
const bool ENABLE_READ_AHEAD = true;           // Enable read-ahead optimization
// ====================================================================

class OptimizedDiskSchedulingDemo {
private:
    std::atomic<long long> totalBytesWritten{0};
    std::atomic<long long> totalBytesRead{0};
    std::atomic<int> totalOperations{0};
    std::atomic<int> errorCount{0};
    std::atomic<int> optimizedOperations{0};
    std::mutex logMutex;
    std::mutex schedulerMutex;
    std::chrono::high_resolution_clock::time_point startTime;
    
    // Optimized disk scheduling structures
    struct IORequest {
        int threadId;
        std::string filename;
        size_t position;
        size_t size;
        bool isWrite;
        std::string data;
        std::chrono::high_resolution_clock::time_point timestamp;
        int priority;
        
        // For elevator algorithm - sort by file position
        bool operator<(const IORequest& other) const {
            return position < other.position;
        }
    };
    
    std::priority_queue<IORequest> writeQueue;
    std::priority_queue<IORequest> readQueue;
    std::map<std::string, std::vector<IORequest>> batchedOperations;
    
    void logPerformance(const std::string& message) {
        std::lock_guard<std::mutex> lock(logMutex);
        std::ofstream logFile(LOG_FILE, std::ios::app);
        if (logFile.is_open()) {
            auto now = std::chrono::high_resolution_clock::now();
            auto timestamp = std::chrono::duration_cast<std::chrono::milliseconds>(
                now.time_since_epoch()).count();
            logFile << "[" << timestamp << "] " << message << std::endl;
            logFile.close();
        }
    }
    
    std::string generateOptimizedContent(size_t sizeKB, int threadId, int operation) {
        std::stringstream ss;
        
        // Header with metadata
        ss << "=== OPTIMIZED DISK SCHEDULING DATA ===\n";
        ss << "Thread: " << threadId << " | Operation: " << operation << "\n";
        ss << "Timestamp: " << std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::high_resolution_clock::now().time_since_epoch()).count() << "\n";
        ss << "Optimized Size: " << sizeKB << " KB\n";
        ss << std::string(60, '=') << "\n";
        
        size_t currentSize = ss.str().length();
        size_t targetSize = sizeKB * 1024;
        
        // Fill with structured data patterns for better compression/caching
        for (size_t i = currentSize; i < targetSize; ++i) {
            if (i % 1024 == 1023) {
                ss << '\n';
            } else if (i % 64 == 63) {
                ss << ' ';
            } else {
                // Use predictable patterns for better disk cache performance
                ss << static_cast<char>('A' + (i % 26));
            }
        }
        
        return ss.str();
    }
    
    // SOLUTION 1: Elevator Algorithm Implementation (SCAN/C-SCAN)
    void performElevatorScheduling(int threadId) {
        std::vector<IORequest> requests;
        
        // Generate batch of requests
        for (int op = 0; op < BATCH_SIZE; ++op) {
            IORequest req;
            req.threadId = threadId;
            req.filename = BASE_DIRECTORY + BASE_FILENAME + std::to_string(threadId) + "_" + std::to_string(op) + ".opt";
            req.position = op * WRITE_CHUNK_SIZE; // Sequential positioning
            req.size = WRITE_CHUNK_SIZE;
            req.isWrite = true;
            req.data = generateOptimizedContent(MIN_FILE_SIZE_KB, threadId, op);
            req.timestamp = std::chrono::high_resolution_clock::now();
            req.priority = op; // Lower numbers = higher priority
            
            requests.push_back(req);
        }
        
        // Sort requests by position (Elevator Algorithm)
        std::sort(requests.begin(), requests.end());
        
        // Execute requests in optimized order
        for (const auto& req : requests) {
            try {
                std::ofstream file(req.filename, std::ios::binary | std::ios::out);
                if (file.is_open()) {
                    // Write in large, sequential chunks
                    file.write(req.data.c_str(), req.data.length());
                    file.flush();
                    file.close();
                    
                    totalBytesWritten += req.data.length();
                    optimizedOperations++;
                    
                    std::cout << "[THREAD " << threadId << "] ELEVATOR WRITE: " << req.filename 
                             << " (Pos: " << req.position << ", Size: " << req.size << ")" << std::endl;
                }
                
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in elevator scheduling: " + std::string(e.what()));
            }
        }
    }
    
    // SOLUTION 2: Sequential Access Optimization
    void performSequentialOptimization(int threadId) {
        // Create one large file instead of many small ones
        std::string filename = BASE_DIRECTORY + "sequential_optimized_" + std::to_string(threadId) + ".seq";
        
        try {
            std::ofstream file(filename, std::ios::binary | std::ios::out);
            if (file.is_open()) {
                // Write large sequential blocks
                for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
                    std::string content = generateOptimizedContent(MAX_FILE_SIZE_KB / OPERATIONS_PER_THREAD, threadId, op);
                    
                    // Write entire content in one operation (no seeks)
                    file.write(content.c_str(), content.length());
                    
                    totalBytesWritten += content.length();
                    
                    // No flush until end to minimize disk operations
                }
                
                file.flush(); // Single flush at end
                file.close();
                
                // Now read back sequentially with large buffer
                std::ifstream readFile(filename, std::ios::binary);
                if (readFile.is_open()) {
                    char buffer[READ_BUFFER_SIZE];
                    while (readFile.read(buffer, sizeof(buffer)) || readFile.gcount() > 0) {
                        totalBytesRead += readFile.gcount();
                    }
                    readFile.close();
                }
                
                std::cout << "[THREAD " << threadId << "] SEQUENTIAL OPTIMIZED: " << filename 
                         << " (" << (MAX_FILE_SIZE_KB) << " KB total)" << std::endl;
                
                optimizedOperations++;
                totalOperations++;
            }
            
        } catch (const std::exception& e) {
            errorCount++;
            logPerformance("ERROR in sequential optimization: " + std::string(e.what()));
        }
    }
    
    // SOLUTION 3: Write Batching and Coalescing
    void performWriteBatching(int threadId) {
        std::map<std::string, std::string> batchedWrites;
        
        // Collect multiple writes to same files
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            std::string filename = BASE_DIRECTORY + "batched_" + std::to_string(threadId % 3) + ".batch";
            std::string content = generateOptimizedContent(MIN_FILE_SIZE_KB / 10, threadId, op);
            
            // Accumulate writes instead of immediate I/O
            batchedWrites[filename] += content;
        }
        
        // Execute batched writes (fewer I/O operations)
        for (const auto& batch : batchedWrites) {
            try {
                std::ofstream file(batch.first, std::ios::binary | std::ios::app);
                if (file.is_open()) {
                    // Single large write instead of many small ones
                    file.write(batch.second.c_str(), batch.second.length());
                    file.flush();
                    file.close();
                    
                    totalBytesWritten += batch.second.length();
                    optimizedOperations++;
                    
                    std::cout << "[THREAD " << threadId << "] BATCHED WRITE: " << batch.first 
                             << " (" << batch.second.length() / 1024 << " KB batched)" << std::endl;
                }
                
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in write batching: " + std::string(e.what()));
            }
        }
    }
    
    // SOLUTION 4: Read-Ahead Optimization
    void performReadAheadOptimization(int threadId) {
        // Create files first
        std::vector<std::string> filenames;
        for (int i = 0; i < 5; ++i) {
            std::string filename = BASE_DIRECTORY + "readahead_" + std::to_string(threadId) + "_" + std::to_string(i) + ".ra";
            filenames.push_back(filename);
            
            // Create file with predictable content
            std::ofstream file(filename, std::ios::binary);
            if (file.is_open()) {
                std::string content = generateOptimizedContent(MAX_FILE_SIZE_KB / 5, threadId, i);
                file.write(content.c_str(), content.length());
                file.close();
                totalBytesWritten += content.length();
            }
        }
        
        // Read with large buffers and read-ahead pattern
        for (const auto& filename : filenames) {
            try {
                std::ifstream file(filename, std::ios::binary);
                if (file.is_open()) {
                    // Use large buffer for read-ahead
                    char buffer[READ_BUFFER_SIZE];
                    
                    while (file.read(buffer, sizeof(buffer)) || file.gcount() > 0) {
                        totalBytesRead += file.gcount();
                        
                        // Simulate processing without additional I/O
                        // In real scenario, this would be actual data processing
                    }
                    file.close();
                    
                    std::cout << "[THREAD " << threadId << "] READ-AHEAD: " << filename << std::endl;
                }
                
                optimizedOperations++;
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in read-ahead optimization: " + std::string(e.what()));
            }
        }
    }
    
    // SOLUTION 5: Coordinated Thread Scheduling
    void performCoordinatedAccess(int threadId) {
        // Threads coordinate to avoid conflicts
        {
            std::lock_guard<std::mutex> lock(schedulerMutex);
            
            // Only one thread accesses shared resources at a time
            std::string sharedFile = BASE_DIRECTORY + "coordinated_shared.coord";
            
            try {
                std::string content = generateOptimizedContent(MIN_FILE_SIZE_KB, threadId, 0);
                
                std::ofstream file(sharedFile, std::ios::binary | std::ios::app);
                if (file.is_open()) {
                    file.write(content.c_str(), content.length());
                    file.flush();
                    file.close();
                    
                    totalBytesWritten += content.length();
                    optimizedOperations++;
                    
                    std::cout << "[THREAD " << threadId << "] COORDINATED ACCESS: " << sharedFile << std::endl;
                }
                
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in coordinated access: " + std::string(e.what()));
            }
        }
        
        // Brief delay to allow other threads
        std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_BATCHES_MS));
    }
    
public:
    OptimizedDiskSchedulingDemo() {
        startTime = std::chrono::high_resolution_clock::now();
        
        // Create base directory
        #ifdef _WIN32
            system(("mkdir " + BASE_DIRECTORY).c_str());
        #else
            system(("mkdir -p " + BASE_DIRECTORY).c_str());
        #endif
        
        // Clear log file
        std::ofstream logFile(LOG_FILE, std::ios::trunc);
        if (logFile.is_open()) {
            logFile << "=== OPTIMIZED DISK SCHEDULING PERFORMANCE LOG ===\n";
            logFile.close();
        }
        
        logPerformance("Optimized Disk Scheduling Demo initialized");
    }
    
    void runOptimizedDiskSchedulingDemo() {
        std::cout << "=== OPTIMIZED DISK SCHEDULING DEMONSTRATION ===" << std::endl;
        std::cout << "This program demonstrates PROPER disk scheduling optimization:" << std::endl;
        std::cout << "1. Elevator Algorithm (SCAN/C-SCAN) for minimal seek time" << std::endl;
        std::cout << "2. Sequential access optimization" << std::endl;
        std::cout << "3. Write batching and coalescing" << std::endl;
        std::cout << "4. Read-ahead optimization" << std::endl;
        std::cout << "5. Coordinated thread scheduling" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "OPTIMIZATION PARAMETERS:" << std::endl;
        std::cout << "- Threads: " << NUM_THREADS << " (reduced for coordination)" << std::endl;
        std::cout << "- Operations per thread: " << OPERATIONS_PER_THREAD << std::endl;
        std::cout << "- Chunk size: " << WRITE_CHUNK_SIZE / 1024 << " KB (optimized)" << std::endl;
        std::cout << "- Batch size: " << BATCH_SIZE << std::endl;
        std::cout << "- File size range: " << MIN_FILE_SIZE_KB << "-" << MAX_FILE_SIZE_KB << " KB" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "This version optimizes for maximum throughput and minimal seeks!" << std::endl;
        std::cout << "Press any key to stop the demonstration..." << std::endl;
        std::cout << std::string(70, '-') << std::endl;
        
        bool userStopped = false;
        std::thread monitorThread([&userStopped]() {
            if (_kbhit()) {
                _getch();
                userStopped = true;
                std::cout << "\n>>> User requested stop. Finishing current operations..." << std::endl;
            }
        });
        
        std::vector<std::thread> threads;
        
        // Launch optimized disk operations
        for (int i = 0; i < NUM_THREADS; ++i) {
            threads.emplace_back([this, i, &userStopped]() {
                std::cout << "  Thread " << i << " started - OPTIMIZED DISK OPERATIONS" << std::endl;
                
                while (!userStopped) {
                    if (ENABLE_ELEVATOR_ALGORITHM) {
                        performElevatorScheduling(i);
                    }
                    
                    if (ENABLE_SEQUENTIAL_OPTIMIZATION && !userStopped) {
                        performSequentialOptimization(i);
                    }
                    
                    if (ENABLE_WRITE_BATCHING && !userStopped) {
                        performWriteBatching(i);
                    }
                    
                    if (ENABLE_READ_AHEAD && !userStopped) {
                        performReadAheadOptimization(i);
                    }
                    
                    // Coordinated access (always enabled for demonstration)
                    if (!userStopped) {
                        performCoordinatedAccess(i);
                    }
                    
                    // Coordinated pause between cycles
                    std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_BATCHES_MS * 2));
                }
                
                std::cout << "  Thread " << i << " completed optimized operations" << std::endl;
            });
        }
        
        // Performance monitoring thread
        std::thread perfThread([this, &userStopped]() {
            while (!userStopped) {
                std::this_thread::sleep_for(std::chrono::seconds(5));
                if (!userStopped) {
                    displayRealTimePerformance();
                }
            }
        });
        
        // Wait for user to stop or threads to complete
        if (monitorThread.joinable()) {
            monitorThread.join();
        }
        
        userStopped = true;
        
        // Wait for all threads to complete
        for (auto& thread : threads) {
            thread.join();
        }
        
        if (perfThread.joinable()) {
            perfThread.join();
        }
        
        displayFinalResults();
        
        std::cout << "\nOptimized disk scheduling demonstration completed." << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
    }
    
private:
    void displayRealTimePerformance() {
        auto currentTime = std::chrono::high_resolution_clock::now();
        auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(currentTime - startTime);
        
        std::cout << "\n" << std::string(50, '=') << std::endl;
        std::cout << "REAL-TIME OPTIMIZED PERFORMANCE" << std::endl;
        std::cout << std::string(50, '=') << std::endl;
        std::cout << "Running time: " << elapsed.count() << " seconds" << std::endl;
        std::cout << "Total operations: " << totalOperations.load() << std::endl;
        std::cout << "Optimized operations: " << optimizedOperations.load() << std::endl;
        std::cout << "Data written: " << (totalBytesWritten.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Data read: " << (totalBytesRead.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Write throughput: " << (totalBytesWritten.load() / 1024.0 / 1024.0 / elapsed.count()) << " MB/s" << std::endl;
        std::cout << "Read throughput: " << (totalBytesRead.load() / 1024.0 / 1024.0 / elapsed.count()) << " MB/s" << std::endl;
        std::cout << "Operations/sec: " << (totalOperations.load() / static_cast<double>(elapsed.count())) << std::endl;
        std::cout << "Optimization ratio: " << (optimizedOperations.load() * 100.0 / totalOperations.load()) << "%" << std::endl;
        std::cout << "Errors: " << errorCount.load() << std::endl;
        std::cout << std::string(50, '=') << std::endl;
        
        logPerformance("Optimized stats - Ops: " + std::to_string(totalOperations.load()) + 
                      ", Optimized: " + std::to_string(optimizedOperations.load()) +
                      ", Write: " + std::to_string(totalBytesWritten.load() / 1024 / 1024) + "MB");
    }
    
    void displayFinalResults() {
        auto endTime = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime);
        
        std::cout << "\n" << std::string(70, '=') << std::endl;
        std::cout << "FINAL OPTIMIZED DISK SCHEDULING RESULTS" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "Total execution time: " << duration.count() << " ms" << std::endl;
        std::cout << "Total threads used: " << NUM_THREADS << std::endl;
        std::cout << "Total operations completed: " << totalOperations.load() << std::endl;
        std::cout << "Optimized operations: " << optimizedOperations.load() << std::endl;
        std::cout << "Total bytes written: " << (totalBytesWritten.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Total bytes read: " << (totalBytesRead.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Average write throughput: " << (totalBytesWritten.load() / 1024.0 / 1024.0 / (duration.count() / 1000.0)) << " MB/s" << std::endl;
        std::cout << "Average read throughput: " << (totalBytesRead.load() / 1024.0 / 1024.0 / (duration.count() / 1000.0)) << " MB/s" << std::endl;
        std::cout << "Operations per second: " << (totalOperations.load() / (duration.count() / 1000.0)) << std::endl;
        std::cout << "Optimization efficiency: " << (optimizedOperations.load() * 100.0 / totalOperations.load()) << "%" << std::endl;
        std::cout << "Total errors encountered: " << errorCount.load() << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "OPTIMIZATION TECHNIQUES DEMONSTRATED:" << std::endl;
        std::cout << "✓ Elevator Algorithm: Minimizes disk head movement" << std::endl;
        std::cout << "✓ Sequential Access: Reduces seek time overhead" << std::endl;
        std::cout << "✓ Write Batching: Coalesces multiple small writes" << std::endl;
        std::cout << "✓ Read-Ahead: Uses large buffers for efficiency" << std::endl;
        std::cout << "✓ Thread Coordination: Prevents resource conflicts" << std::endl;
        std::cout << "- Compare with intensive version to see performance difference!" << std::endl;
        std::cout << "- Check " << LOG_FILE << " for detailed optimization metrics" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        
        logPerformance("Final optimized results - Duration: " + std::to_string(duration.count()) + "ms, " +
                      "Ops: " + std::to_string(totalOperations.load()) + ", " +
                      "Optimized: " + std::to_string(optimizedOperations.load()) + ", " +
                      "Errors: " + std::to_string(errorCount.load()));
    }
};

int main() {
    try {
        OptimizedDiskSchedulingDemo demo;
        demo.runOptimizedDiskSchedulingDemo();
    } catch (const std::exception& e) {
        std::cerr << "Fatal error: " << e.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
        return 1;
    }
    
    return 0;
}
