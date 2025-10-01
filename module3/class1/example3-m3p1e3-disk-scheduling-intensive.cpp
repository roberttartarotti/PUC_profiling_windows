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
// INTENSIVE DISK SCHEDULING PARAMETERS - ADJUST FOR MAXIMUM STRESS
// ====================================================================
const int NUM_THREADS = 12;                    // Number of concurrent I/O threads
const int OPERATIONS_PER_THREAD = 100;         // Operations each thread performs
const int NUM_FILES = 500;                     // Total number of files to create
const int MIN_FILE_SIZE_KB = 50;               // Minimum file size in KB
const int MAX_FILE_SIZE_KB = 200;              // Maximum file size in KB
const int WRITE_CHUNK_SIZE = 1024;             // Write chunk size in bytes
const int READ_BUFFER_SIZE = 4096;             // Read buffer size in bytes
const int RANDOM_SEEK_OPERATIONS = 1000;       // Number of random seek operations
const int SEQUENTIAL_OPERATIONS = 500;         // Number of sequential operations
const int DELAY_BETWEEN_OPS_MICROSECONDS = 10; // Very small delay for maximum stress
const std::string BASE_DIRECTORY = "disk_stress_test/";
const std::string BASE_FILENAME = "stress_file_";
const std::string LOG_FILE = "disk_scheduling_performance.log";
const bool ENABLE_RANDOM_SEEKS = true;         // Enable random disk seeks
const bool ENABLE_SEQUENTIAL_ACCESS = true;    // Enable sequential access patterns
const bool ENABLE_FRAGMENTATION = true;        // Create fragmented file patterns
const bool ENABLE_CONCURRENT_ACCESS = true;    // Multiple threads accessing same files
// ====================================================================

// IORequest structure similar to C# version
struct IORequest {
    int threadId;
    std::string filename;
    long position;
    int size;
    bool isWrite;
    std::chrono::high_resolution_clock::time_point timestamp;
};

// Thread-safe queue implementation (similar to ConcurrentQueue in C#)
template<typename T>
class ConcurrentQueue {
private:
    std::queue<T> queue_;
    mutable std::mutex mutex_;
    std::condition_variable condition_;

public:
    void push(T item) {
        std::lock_guard<std::mutex> lock(mutex_);
        queue_.push(item);
        condition_.notify_one();
    }

    bool tryPop(T& item) {
        std::lock_guard<std::mutex> lock(mutex_);
        if (queue_.empty()) {
            return false;
        }
        item = queue_.front();
        queue_.pop();
        return true;
    }

    bool empty() const {
        std::lock_guard<std::mutex> lock(mutex_);
        return queue_.empty();
    }

    size_t size() const {
        std::lock_guard<std::mutex> lock(mutex_);
        return queue_.size();
    }
};

class IntensiveDiskSchedulingDemo {
private:
    std::atomic<long long> totalBytesWritten{0};
    std::atomic<long long> totalBytesRead{0};
    std::atomic<long long> totalOperations{0};
    std::atomic<long long> errorCount{0};
    std::atomic<long long> seekOperations{0};
    
    mutable std::mutex logMutex;
    std::chrono::high_resolution_clock::time_point startTime;
    std::random_device rd;
    std::mt19937 random;
    
    // Thread-safe collections similar to C# ConcurrentBag and ConcurrentQueue
    ConcurrentQueue<std::string> createdFiles;
    ConcurrentQueue<IORequest> ioQueue;
    std::unordered_map<std::string, std::vector<std::string>> batchedOperations;
    std::mutex batchMutex;
    
    std::atomic<bool> userStopped{false};
    
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
    
    std::vector<char> generateIntensiveContent(int sizeKB, int threadId, int operation) {
        std::stringstream ss;
        
        // Header with metadata
        ss << "=== INTENSIVE DISK SCHEDULING TEST DATA ===\n";
        ss << "Thread: " << threadId << " | Operation: " << operation << "\n";
        ss << "Timestamp: " << std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::high_resolution_clock::now().time_since_epoch()).count() << "\n";
        ss << "Target Size: " << sizeKB << " KB\n";
        ss << std::string(60, '=') << "\n";
        
        std::string header = ss.str();
        int currentSize = static_cast<int>(header.length());
        int targetSize = sizeKB * 1024;
        int remainingSize = std::max(0, targetSize - currentSize);
        
        std::vector<char> content;
        content.reserve(targetSize);
        
        // Add header
        content.insert(content.end(), header.begin(), header.end());
        
        // Fill with intensive random data patterns
        for (int i = 0; i < remainingSize; ++i) {
            if (i % 100 == 99) {
                content.push_back('\n');
            } else if (i % 10 == 9) {
                content.push_back(' ');
            } else {
                content.push_back(static_cast<char>(random() % 94 + 32)); // Printable ASCII
            }
        }
        
        return content;
    }
    
    // Simulate random disk seeks (worst case for mechanical drives)
    void performRandomSeekOperationsAsync(int threadId) {
        std::random_device rd;
        std::mt19937 gen(rd());
        
        for (int op = 0; op < RANDOM_SEEK_OPERATIONS; ++op) {
            try {
                // Create random file access pattern
                std::uniform_int_distribution<> fileDis(0, NUM_FILES - 1);
                std::uniform_int_distribution<> sizeDis(MIN_FILE_SIZE_KB, MAX_FILE_SIZE_KB);
                
                int fileIndex = fileDis(gen);
                std::string filename = BASE_DIRECTORY + BASE_FILENAME + std::to_string(fileIndex) + ".dat";
                
                int fileSize = sizeDis(gen);
                auto content = generateIntensiveContent(fileSize, threadId, op);
                
                // INTENSIVE WRITE with random seeks
                std::ofstream file(filename, std::ios::binary | std::ios::out);
                if (file.is_open()) {
                    // Write in random chunks to simulate disk head movement
                    size_t bytesWritten = 0;
                    while (bytesWritten < content.size() && !userStopped) {
                        size_t chunkSize = std::min(static_cast<size_t>(WRITE_CHUNK_SIZE), content.size() - bytesWritten);
                        
                        // Random seek within file
                        std::uniform_int_distribution<> seekDis(0, static_cast<int>(content.size() - chunkSize));
                        size_t seekPos = seekDis(gen);
                        
                        file.seekp(seekPos);
                        file.write(content.data() + bytesWritten, chunkSize);
                        file.flush(); // Force immediate disk write
                        
                        bytesWritten += chunkSize;
                        totalBytesWritten += chunkSize;
                        seekOperations++;
                        
                        // Micro delay to allow other threads to compete
                        if (DELAY_BETWEEN_OPS_MICROSECONDS > 0) {
                            std::this_thread::sleep_for(std::chrono::microseconds(DELAY_BETWEEN_OPS_MICROSECONDS));
                        }
                    }
                    file.close();
                    
                    // Add to created files list for later access
                    createdFiles.push(filename);
                    
                    std::cout << "[THREAD " << threadId << "] RANDOM WRITE: " << filename 
                             << " (" << fileSize << " KB) - Seek Op " << op << std::endl;
                }
                
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in random seek operation: " + std::string(e.what()));
            }
        }
    }
    
    // Simulate sequential access patterns (best case scenario)
    void performSequentialOperationsAsync(int threadId) {
        for (int op = 0; op < SEQUENTIAL_OPERATIONS; ++op) {
            try {
                std::string filename = BASE_DIRECTORY + "sequential_" + std::to_string(threadId) + "_" + std::to_string(op) + ".seq";
                
                // Create large sequential file
                auto content = generateIntensiveContent(MAX_FILE_SIZE_KB, threadId, op);
                
                // INTENSIVE SEQUENTIAL WRITE
                std::ofstream file(filename, std::ios::binary | std::ios::out);
                if (file.is_open()) {
                    // Write in large sequential chunks
                    for (size_t pos = 0; pos < content.size() && !userStopped; pos += WRITE_CHUNK_SIZE) {
                        size_t chunkSize = std::min(static_cast<size_t>(WRITE_CHUNK_SIZE), content.size() - pos);
                        file.write(content.data() + pos, chunkSize);
                        file.flush();
                        
                        totalBytesWritten += chunkSize;
                        
                        // Very small delay to maintain intensity
                        std::this_thread::sleep_for(std::chrono::microseconds(DELAY_BETWEEN_OPS_MICROSECONDS / 2));
                    }
                    file.close();
                    
                    // Immediately read back sequentially
                    std::ifstream readFile(filename, std::ios::binary);
                    if (readFile.is_open()) {
                        char buffer[READ_BUFFER_SIZE];
                        while (readFile.read(buffer, sizeof(buffer)) || readFile.gcount() > 0) {
                            totalBytesRead += readFile.gcount();
                        }
                        readFile.close();
                    }
                    
                    std::cout << "[THREAD " << threadId << "] SEQUENTIAL: " << filename 
                             << " (" << MAX_FILE_SIZE_KB << " KB) - Op " << op << std::endl;
                }
                
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in sequential operation: " + std::string(e.what()));
            }
        }
    }
    
    // Create fragmented file access patterns
    void performFragmentationOperationsAsync(int threadId) {
        std::random_device rd;
        std::mt19937 gen(rd());
        
        // Create multiple small files that will fragment the disk
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            try {
                // Create multiple small files simultaneously
                for (int fragment = 0; fragment < 5; ++fragment) {
                    std::string filename = BASE_DIRECTORY + "fragment_" + std::to_string(threadId) + 
                                         "_" + std::to_string(op) + "_" + std::to_string(fragment) + ".frag";
                    
                    std::uniform_int_distribution<> sizeDis(MIN_FILE_SIZE_KB, MIN_FILE_SIZE_KB + 20);
                    int fileSize = sizeDis(gen);
                    auto content = generateIntensiveContent(fileSize, threadId, op);
                    
                    // Write fragmented data
                    std::ofstream file(filename, std::ios::binary);
                    if (file.is_open()) {
                        // Write in small, random-sized chunks to increase fragmentation
                        size_t pos = 0;
                        while (pos < content.size() && !userStopped) {
                            std::uniform_int_distribution<> chunkDis(100, 500);
                            size_t chunkSize = std::min(static_cast<size_t>(chunkDis(gen)), content.size() - pos);
                            
                            file.write(content.data() + pos, chunkSize);
                            file.flush();
                            
                            pos += chunkSize;
                            totalBytesWritten += chunkSize;
                            
                            // Small delay to allow disk head movement
                            std::this_thread::sleep_for(std::chrono::microseconds(DELAY_BETWEEN_OPS_MICROSECONDS));
                        }
                        file.close();
                    }
                }
                
                std::cout << "[THREAD " << threadId << "] FRAGMENTATION: Created 5 fragments - Op " << op << std::endl;
                totalOperations++;
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in fragmentation operation: " + std::string(e.what()));
            }
        }
    }
    
    // Concurrent access to same files (causes disk scheduling conflicts)
    void performConcurrentAccessOperationsAsync(int threadId) {
        std::random_device rd;
        std::mt19937 gen(rd());
        
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            try {
                // Multiple threads accessing the same files
                std::string sharedFilename = BASE_DIRECTORY + "shared_access_" + std::to_string(op % 10) + ".shared";
                
                if (op % 2 == 0) {
                    // Writer thread
                    auto content = generateIntensiveContent(MIN_FILE_SIZE_KB, threadId, op);
                    std::ofstream file(sharedFilename, std::ios::binary | std::ios::app);
                    if (file.is_open()) {
                        file.write(content.data(), content.size());
                        file.flush();
                        file.close();
                        totalBytesWritten += content.size();
                    }
                    std::cout << "[THREAD " << threadId << "] CONCURRENT WRITE: " << sharedFilename << " - Op " << op << std::endl;
                } else {
                    // Reader thread
                    std::ifstream file(sharedFilename, std::ios::binary);
                    if (file.is_open()) {
                        char buffer[READ_BUFFER_SIZE];
                        while (file.read(buffer, sizeof(buffer)) || file.gcount() > 0) {
                            totalBytesRead += file.gcount();
                        }
                        file.close();
                    }
                    std::cout << "[THREAD " << threadId << "] CONCURRENT READ: " << sharedFilename << " - Op " << op << std::endl;
                }
                
                totalOperations++;
                
                // Very small delay to maintain maximum stress
                std::this_thread::sleep_for(std::chrono::microseconds(DELAY_BETWEEN_OPS_MICROSECONDS));
                
            } catch (const std::exception& e) {
                errorCount++;
                logPerformance("ERROR in concurrent access operation: " + std::string(e.what()));
            }
        }
    }
    
public:
    IntensiveDiskSchedulingDemo() : random(rd()) {
        startTime = std::chrono::high_resolution_clock::now();
        
        // Create base directory
        #ifdef _WIN32
            system(("mkdir " + BASE_DIRECTORY + " 2>nul").c_str());
        #else
            system(("mkdir -p " + BASE_DIRECTORY).c_str());
        #endif
        
        // Clear log file
        try {
            std::ofstream logFile(LOG_FILE, std::ios::trunc);
            if (logFile.is_open()) {
                logFile << "=== INTENSIVE DISK SCHEDULING PERFORMANCE LOG ===\n";
                logFile.close();
            }
        } catch (const std::exception& ex) {
            std::cout << "Log file initialization error: " << ex.what() << std::endl;
        }
        
        logPerformance("Intensive Disk Scheduling Demo initialized");
    }
    
    void runIntensiveDiskSchedulingDemo() {
        std::cout << "=== INTENSIVE DISK SCHEDULING DEMONSTRATION ===" << std::endl;
        std::cout << "This program will STRESS TEST your disk subsystem with:" << std::endl;
        std::cout << "1. Random seek operations (worst case for mechanical drives)" << std::endl;
        std::cout << "2. Sequential access patterns (best case scenario)" << std::endl;
        std::cout << "3. File fragmentation simulation" << std::endl;
        std::cout << "4. Concurrent file access conflicts" << std::endl;
        std::cout << "5. Intensive I/O operations" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "INTENSITY PARAMETERS:" << std::endl;
        std::cout << "- Threads: " << NUM_THREADS << std::endl;
        std::cout << "- Operations per thread: " << OPERATIONS_PER_THREAD << std::endl;
        std::cout << "- Total files to create: " << NUM_FILES << std::endl;
        std::cout << "- Random seek operations: " << RANDOM_SEEK_OPERATIONS << std::endl;
        std::cout << "- Sequential operations: " << SEQUENTIAL_OPERATIONS << std::endl;
        std::cout << "- File size range: " << MIN_FILE_SIZE_KB << "-" << MAX_FILE_SIZE_KB << " KB" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "WARNING: This will create intense disk activity!" << std::endl;
        std::cout << "Press any key to stop the demonstration..." << std::endl;
        std::cout << std::string(70, '-') << std::endl;
        
        // Start monitoring for user input (similar to C# keyTask)
        auto keyTask = std::async(std::launch::async, [this]() {
            _getch();  // Wait for any key press
            userStopped = true;
            std::cout << "\n>>> User requested stop. Finishing current operations..." << std::endl;
        });
        
        std::vector<std::future<void>> tasks;
        
        // Launch intensive disk operations (similar to C# Task.Run)
        for (int i = 0; i < NUM_THREADS; ++i) {
            tasks.emplace_back(std::async(std::launch::async, [this, i]() {
                std::cout << "  Task " << i << " started - INTENSIVE DISK OPERATIONS" << std::endl;
                
                while (!userStopped) {
                    if (ENABLE_RANDOM_SEEKS && !userStopped) {
                        performRandomSeekOperationsAsync(i);
                    }
                    
                    if (ENABLE_SEQUENTIAL_ACCESS && !userStopped) {
                        performSequentialOperationsAsync(i);
                    }
                    
                    if (ENABLE_FRAGMENTATION && !userStopped) {
                        performFragmentationOperationsAsync(i);
                    }
                    
                    if (ENABLE_CONCURRENT_ACCESS && !userStopped) {
                        performConcurrentAccessOperationsAsync(i);
                    }
                    
                    // Brief pause before next intensive cycle
                    std::this_thread::sleep_for(std::chrono::milliseconds(100));
                }
                
                std::cout << "  Task " << i << " completed intensive operations" << std::endl;
            }));
        }
        
        // Performance monitoring task (similar to C# perfTask)
        auto perfTask = std::async(std::launch::async, [this]() {
            while (!userStopped) {
                std::this_thread::sleep_for(std::chrono::seconds(5));
                if (!userStopped) {
                    displayRealTimePerformance();
                }
            }
        });
        
        // Wait for user to stop (similar to C# await keyTask)
        keyTask.wait();
        userStopped = true;
        
        // Wait for all tasks to complete (similar to C# Task.WhenAll)
        for (auto& task : tasks) {
            task.wait();
        }
        
        perfTask.wait();
        
        displayFinalResults();
        
        std::cout << "\nIntensive disk scheduling demonstration completed." << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
    }
    
private:
    void displayRealTimePerformance() {
        auto currentTime = std::chrono::high_resolution_clock::now();
        auto elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(currentTime - startTime);
        double elapsedSeconds = elapsed.count() / 1000.0;
        
        std::cout << "\n" << std::string(50, '=') << std::endl;
        std::cout << "REAL-TIME DISK PERFORMANCE" << std::endl;
        std::cout << std::string(50, '=') << std::endl;
        std::cout << "Running time: " << std::fixed << std::setprecision(1) << elapsedSeconds << " seconds" << std::endl;
        std::cout << "Total operations: " << totalOperations.load() << std::endl;
        std::cout << "Seek operations: " << seekOperations.load() << std::endl;
        std::cout << "Data written: " << std::fixed << std::setprecision(2) << (totalBytesWritten.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Data read: " << std::fixed << std::setprecision(2) << (totalBytesRead.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Write throughput: " << std::fixed << std::setprecision(2) << (totalBytesWritten.load() / 1024.0 / 1024.0 / elapsedSeconds) << " MB/s" << std::endl;
        std::cout << "Read throughput: " << std::fixed << std::setprecision(2) << (totalBytesRead.load() / 1024.0 / 1024.0 / elapsedSeconds) << " MB/s" << std::endl;
        std::cout << "Operations/sec: " << std::fixed << std::setprecision(2) << (totalOperations.load() / elapsedSeconds) << std::endl;
        std::cout << "Errors: " << errorCount.load() << std::endl;
        std::cout << std::string(50, '=') << std::endl;
        
        logPerformance("Real-time stats - Ops: " + std::to_string(totalOperations.load()) + 
                      ", Write: " + std::to_string(totalBytesWritten.load() / 1024 / 1024) + "MB" +
                      ", Read: " + std::to_string(totalBytesRead.load() / 1024 / 1024) + "MB");
    }
    
    void displayFinalResults() {
        auto endTime = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime);
        
        std::cout << "\n" << std::string(70, '=') << std::endl;
        std::cout << "FINAL INTENSIVE DISK SCHEDULING RESULTS" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "Total execution time: " << duration.count() << " ms" << std::endl;
        std::cout << "Total threads used: " << NUM_THREADS << std::endl;
        std::cout << "Total operations completed: " << totalOperations.load() << std::endl;
        std::cout << "Total seek operations: " << seekOperations.load() << std::endl;
        std::cout << "Total bytes written: " << (totalBytesWritten.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Total bytes read: " << (totalBytesRead.load() / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Average write throughput: " << (totalBytesWritten.load() / 1024.0 / 1024.0 / (duration.count() / 1000.0)) << " MB/s" << std::endl;
        std::cout << "Average read throughput: " << (totalBytesRead.load() / 1024.0 / 1024.0 / (duration.count() / 1000.0)) << " MB/s" << std::endl;
        std::cout << "Operations per second: " << (totalOperations.load() / (duration.count() / 1000.0)) << std::endl;
        std::cout << "Total errors encountered: " << errorCount.load() << std::endl;
        std::cout << "Files created: " << createdFiles.size() << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        std::cout << "DISK SCHEDULING ANALYSIS:" << std::endl;
        std::cout << "- Random seeks simulate worst-case disk head movement" << std::endl;
        std::cout << "- Sequential operations show optimal disk performance" << std::endl;
        std::cout << "- Fragmentation demonstrates real-world disk usage patterns" << std::endl;
        std::cout << "- Concurrent access shows scheduling algorithm effectiveness" << std::endl;
        std::cout << "- Check " << LOG_FILE << " for detailed performance metrics" << std::endl;
        std::cout << std::string(70, '=') << std::endl;
        
        logPerformance("Final results - Duration: " + std::to_string(duration.count()) + "ms, " +
                      "Ops: " + std::to_string(totalOperations.load()) + ", " +
                      "Errors: " + std::to_string(errorCount.load()));
    }
};

int main() {
    try {
        IntensiveDiskSchedulingDemo demo;
        demo.runIntensiveDiskSchedulingDemo();
    } catch (const std::exception& e) {
        std::cerr << "Fatal error: " << e.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
        return 1;
    }
    
    return 0;
}
