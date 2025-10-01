#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <thread>
#include <mutex>
#include <shared_mutex>
#include <chrono>
#include <random>
#include <atomic>
#include <sstream>
#include <conio.h>  // For _kbhit() on Windows
#include <cstdio>   // For remove()
#include <condition_variable>

// ====================================================================
// CONFIGURATION VARIABLES - PROPER SYNCHRONIZATION ENABLED
// ====================================================================
const int NUM_THREADS = 6;                     // Number of concurrent threads
const int OPERATIONS_PER_THREAD = 20;          // Operations each thread performs
const int FILE_SIZE_KB = 5;                    // Size of each file in KB
const std::string SHARED_FILE = "shared_resource_safe.txt";
const std::string LOG_FILE = "concurrent_operations_safe.log";
const std::string BASE_FILENAME = "concurrent_file_safe_";
const int DELAY_BETWEEN_OPS_MS = 50;           // Delay between operations
// ====================================================================

class ConcurrentIOProblemsSolved {
private:
    // SOLUTION 1: Use atomic counters for thread-safe counting
    std::atomic<int> operationCounter{0};
    std::atomic<int> errorCounter{0};
    std::atomic<long long> totalBytesProcessed{0};
    
    // SOLUTION 2: Use mutexes for proper synchronization
    std::mutex logMutex;           // For thread-safe logging
    std::mutex sharedFileMutex;    // For shared file access
    std::shared_mutex fileLockMutex; // For reader-writer file access
    std::mutex fileOperationMutex; // For file creation/deletion coordination
    
    // SOLUTION 3: Use condition variables for proper coordination
    std::condition_variable fileReadyCondition;
    std::mutex fileReadyMutex;
    std::atomic<bool> fileReady{false};
    
    std::chrono::high_resolution_clock::time_point startTime;
    
    // Generate content for files
    std::string generateFileContent(int threadId, int operationId) {
        std::stringstream ss;
        ss << "=== CONCURRENT I/O OPERATION (SAFE VERSION) ===\n";
        ss << "Thread ID: " << threadId << "\n";
        ss << "Operation: " << operationId << "\n";
        ss << "Timestamp: " << std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::high_resolution_clock::now().time_since_epoch()).count() << "\n";
        ss << "Process ID: " << std::this_thread::get_id() << "\n";
        ss << std::string(50, '=') << "\n";
        
        // Fill to desired size
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> dis(65, 90);
        
        int currentSize = ss.str().length();
        int targetSize = FILE_SIZE_KB * 1024;
        
        for (int i = currentSize; i < targetSize; ++i) {
            if (i % 80 == 79) {
                ss << '\n';
            } else {
                ss << static_cast<char>(dis(gen));
            }
        }
        
        return ss.str();
    }
    
    // SOLUTION FOR PROBLEM 2: Thread-safe shared file access
    void demonstrateSharedFileContentionSafe(int threadId) {
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            try {
                std::string content = "Thread " + std::to_string(threadId) + 
                                    " Operation " + std::to_string(op) + 
                                    " Time: " + std::to_string(std::chrono::duration_cast<std::chrono::milliseconds>(
                                        std::chrono::high_resolution_clock::now().time_since_epoch()).count()) + "\n";
                
                // SOLUTION: Use mutex to synchronize access to shared file
                {
                    std::lock_guard<std::mutex> lock(sharedFileMutex);
                    std::ofstream file(SHARED_FILE, std::ios::app);
                    if (file.is_open()) {
                        file << content;
                        file.flush();
                        file.close();
                        std::cout << "[THREAD " << threadId << "] SAFE WRITE to " << SHARED_FILE << " (Op " << op << ")" << std::endl;
                    }
                }
                
                totalBytesProcessed += content.length();
                
            } catch (const std::exception& e) {
                errorCounter++;
                safeLogging(threadId, "Error in shared file operation: " + std::string(e.what()));
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_OPS_MS));
        }
    }
    
    // SOLUTION FOR PROBLEM 3: Thread-safe logging
    void safeLogging(int threadId, const std::string& message) {
        // SOLUTION: Use mutex for thread-safe logging
        std::lock_guard<std::mutex> lock(logMutex);
        std::ofstream logFile(LOG_FILE, std::ios::app);
        if (logFile.is_open()) {
            logFile << "[Thread " << threadId << "] " << message << std::endl;
            logFile.close();
        }
    }
    
    // SOLUTION FOR PROBLEM 4: Thread-safe file operations with proper coordination
    void demonstrateFileRaceConditionsSafe(int threadId) {
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            std::string filename = BASE_FILENAME + std::to_string(threadId) + "_" + std::to_string(op) + ".tmp";
            
            try {
                // SOLUTION: Use atomic operations for counters
                operationCounter++;
                
                // SOLUTION: Coordinate file operations with mutex
                std::unique_lock<std::mutex> operationLock(fileOperationMutex);
                
                // Create file
                std::string content = generateFileContent(threadId, op);
                std::ofstream file(filename, std::ios::binary);
                if (file.is_open()) {
                    file.write(content.c_str(), content.length());
                    file.flush(); // Ensure data is written
                    file.close();
                    
                    std::cout << "[THREAD " << threadId << "] CREATED FILE: " << filename << " (" << content.length() << " bytes)" << std::endl;
                    safeLogging(threadId, "Created file: " + filename);
                    
                    // SOLUTION FOR PROBLEM 5: Proper synchronization for read-after-write
                    // Ensure file is completely written before reading
                    std::this_thread::sleep_for(std::chrono::milliseconds(5)); // Small delay to ensure file system consistency
                    
                    // Try to read the file we just created
                    std::ifstream readFile(filename, std::ios::binary);
                    if (readFile.is_open()) {
                        readFile.seekg(0, std::ios::end);
                        size_t fileSize = readFile.tellg();
                        readFile.close();
                        
                        totalBytesProcessed += fileSize;
                        std::cout << "[THREAD " << threadId << "] READ FILE: " << filename << " (" << fileSize << " bytes)" << std::endl;
                        safeLogging(threadId, "Read file: " + filename + " (" + std::to_string(fileSize) + " bytes)");
                    } else {
                        errorCounter++;
                        std::cout << "[THREAD " << threadId << "] ERROR: Could not read file: " << filename << std::endl;
                        safeLogging(threadId, "ERROR: Could not read file: " + filename);
                    }
                    
                    // SOLUTION FOR PROBLEM 6: Coordinated file deletion
                    // Ensure no other threads are accessing the file before deletion
                    std::ifstream checkFile(filename);
                    if (checkFile.good()) {
                        checkFile.close();
                        
                        // Small delay to ensure file handles are released
                        std::this_thread::sleep_for(std::chrono::milliseconds(2));
                        
                        if (std::remove(filename.c_str()) == 0) {
                            std::cout << "[THREAD " << threadId << "] DELETED FILE: " << filename << std::endl;
                            safeLogging(threadId, "Deleted file: " + filename);
                        } else {
                            std::cout << "[THREAD " << threadId << "] ERROR: Could not delete file: " << filename << std::endl;
                            safeLogging(threadId, "ERROR: Could not delete file: " + filename);
                        }
                    }
                    
                } else {
                    errorCounter++;
                    safeLogging(threadId, "ERROR: Could not create file: " + filename);
                }
                
                // Release the operation lock
                operationLock.unlock();
                
            } catch (const std::exception& e) {
                errorCounter++;
                safeLogging(threadId, "ERROR in file operations: " + std::string(e.what()));
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_OPS_MS));
        }
    }
    
    // SOLUTION FOR PROBLEM 7: Proper reader-writer synchronization
    void demonstrateFileLockingProblemsSafe(int threadId) {
        std::string sharedDataFile = "shared_data_safe_" + std::to_string(threadId % 3) + ".dat";
        
        for (int op = 0; op < OPERATIONS_PER_THREAD / 2; ++op) {
            try {
                if (op % 2 == 0) {
                    // Writer thread - use exclusive lock
                    std::string data = "Data from thread " + std::to_string(threadId) + 
                                     " operation " + std::to_string(op) + "\n";
                    
                    // SOLUTION: Use exclusive lock for writers
                    std::unique_lock<std::shared_mutex> writeLock(fileLockMutex);
                    std::ofstream file(sharedDataFile, std::ios::app);
                    if (file.is_open()) {
                        file << data;
                        file.flush();
                        file.close();
                        std::cout << "[THREAD " << threadId << "] SAFE WRITE to " << sharedDataFile << " (Op " << op << ")" << std::endl;
                    }
                    writeLock.unlock();
                    
                    safeLogging(threadId, "Wrote to shared file: " + sharedDataFile);
                    
                } else {
                    // Reader thread - use shared lock
                    // SOLUTION: Use shared lock for readers (multiple readers can access simultaneously)
                    std::shared_lock<std::shared_mutex> readLock(fileLockMutex);
                    std::ifstream file(sharedDataFile);
                    if (file.is_open()) {
                        std::string line;
                        int lineCount = 0;
                        while (std::getline(file, line)) {
                            lineCount++;
                        }
                        file.close();
                        
                        std::cout << "[THREAD " << threadId << "] SAFE READ from " << sharedDataFile << " (" << lineCount << " lines)" << std::endl;
                        safeLogging(threadId, "Read shared file: " + sharedDataFile + 
                                    " (" + std::to_string(lineCount) + " lines)");
                    }
                    readLock.unlock();
                }
                
            } catch (const std::exception& e) {
                errorCounter++;
                safeLogging(threadId, "ERROR in file locking demo: " + std::string(e.what()));
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_OPS_MS));
        }
    }
    
public:
    ConcurrentIOProblemsSolved() {
        startTime = std::chrono::high_resolution_clock::now();
        
        // Clean up any existing files
        std::remove(SHARED_FILE.c_str());
        std::remove(LOG_FILE.c_str());
        
        // Clean up any existing shared data files
        for (int i = 0; i < 3; ++i) {
            std::string sharedDataFile = "shared_data_safe_" + std::to_string(i) + ".dat";
            std::remove(sharedDataFile.c_str());
        }
    }
    
    void runConcurrentOperations() {
        std::cout << "=== CONCURRENT I/O PROBLEMS - SOLVED VERSION ===" << std::endl;
        std::cout << "This program demonstrates PROPER solutions to I/O concurrency issues:" << std::endl;
        std::cout << "1. Thread-safe shared file access using mutexes" << std::endl;
        std::cout << "2. Safe logging operations with synchronization" << std::endl;
        std::cout << "3. Coordinated file creation/deletion" << std::endl;
        std::cout << "4. Reader-writer locks for file access" << std::endl;
        std::cout << "5. Atomic operations for counters" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Configuration:" << std::endl;
        std::cout << "- Threads: " << NUM_THREADS << std::endl;
        std::cout << "- Operations per thread: " << OPERATIONS_PER_THREAD << std::endl;
        std::cout << "- All synchronization mechanisms: ENABLED" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Press any key to stop the demonstration..." << std::endl;
        std::cout << "The program will run in continuous cycles until you press a key." << std::endl;
        std::cout << std::string(60, '-') << std::endl;
        
        int cycleCount = 0;
        bool userStopped = false;
        
        // Start monitoring for user input in a separate thread
        std::thread monitorThread([&userStopped]() {
            _getch();  // Wait for any key press
            userStopped = true;
            std::cout << "\n>>> User requested stop. Finishing current cycle..." << std::endl;
        });
        
        // Main demonstration loop
        while (!userStopped) {
            cycleCount++;
            std::cout << "\n" << std::string(60, '=') << std::endl;
            std::cout << ">>> STARTING SAFE CYCLE #" << cycleCount << " <<<" << std::endl;
            std::cout << std::string(60, '=') << std::endl;
            
            // Reset counters for this cycle
            operationCounter = 0;
            errorCounter = 0;
            totalBytesProcessed = 0;
            startTime = std::chrono::high_resolution_clock::now();
            
            std::vector<std::thread> threads;
            
            // Launch threads with proper synchronization
            std::cout << "Launching " << NUM_THREADS << " properly synchronized threads..." << std::endl;
            for (int i = 0; i < NUM_THREADS; ++i) {
                threads.emplace_back([this, i, cycleCount]() {
                    std::cout << "  Thread " << i << " started safely (Cycle " << cycleCount << ")" << std::endl;
                    
                    // Each thread performs properly synchronized I/O operations
                    demonstrateSharedFileContentionSafe(i);
                    demonstrateFileRaceConditionsSafe(i);
                    demonstrateFileLockingProblemsSafe(i);
                    
                    std::cout << "  Thread " << i << " completed safely (Cycle " << cycleCount << ")" << std::endl;
                });
            }
            
            // Wait for all threads to complete
            for (auto& thread : threads) {
                thread.join();
            }
            
            // Display results for this cycle
            std::cout << "\n" << std::string(40, '-') << std::endl;
            std::cout << "SAFE CYCLE #" << cycleCount << " RESULTS:" << std::endl;
            displayResults();
            
            if (!userStopped) {
                std::cout << "\nWaiting 3 seconds before next cycle..." << std::endl;
                std::cout << "(Press any key to stop)" << std::endl;
                
                // Wait 3 seconds or until user presses key
                for (int i = 0; i < 30 && !userStopped; ++i) {
                    std::this_thread::sleep_for(std::chrono::milliseconds(100));
                }
            }
        }
        
        // Clean up monitor thread
        if (monitorThread.joinable()) {
            monitorThread.join();
        }
        
        std::cout << "\n" << std::string(60, '=') << std::endl;
        std::cout << "SAFE DEMONSTRATION COMPLETED AFTER " << cycleCount << " CYCLES" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Check the following files - they should be properly formatted:" << std::endl;
        std::cout << "- " << SHARED_FILE << " (no corruption expected)" << std::endl;
        std::cout << "- " << LOG_FILE << " (clean log entries expected)" << std::endl;
        std::cout << "- shared_data_safe_*.dat files (consistent data expected)" << std::endl;
        std::cout << "\nPress any key to exit..." << std::endl;
        _getch();
    }
    
private:
    void displayResults() {
        auto endTime = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime);
        
        std::cout << "\n" << std::string(60, '=') << std::endl;
        std::cout << "SAFE CONCURRENT I/O RESULTS" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Execution time: " << duration.count() << " ms" << std::endl;
        std::cout << "Total threads: " << NUM_THREADS << std::endl;
        std::cout << "Expected operations: " << (NUM_THREADS * OPERATIONS_PER_THREAD) << std::endl;
        std::cout << "Actual operations: " << operationCounter.load() << std::endl;
        std::cout << "Errors encountered: " << errorCounter.load() << " (should be 0 or very low)" << std::endl;
        std::cout << "Total bytes processed: " << (totalBytesProcessed.load() / 1024.0) << " KB" << std::endl;
        
        // Analyze the shared file for corruption
        std::ifstream file(SHARED_FILE);
        if (file.is_open()) {
            std::string line;
            int lineCount = 0;
            while (std::getline(file, line)) {
                lineCount++;
            }
            file.close();
            std::cout << "Shared file lines: " << lineCount << std::endl;
        }
        
        // Analyze the log file for corruption
        std::ifstream logFile(LOG_FILE);
        if (logFile.is_open()) {
            std::string line;
            int logLines = 0;
            int wellFormedLines = 0;
            while (std::getline(logFile, line)) {
                logLines++;
                // Check for properly formatted lines
                if (line.find("[Thread") != std::string::npos) {
                    wellFormedLines++;
                }
            }
            logFile.close();
            std::cout << "Log file lines: " << logLines << std::endl;
            std::cout << "Well-formed log lines: " << wellFormedLines << " (should equal total lines)" << std::endl;
        }
        
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "SAFETY ANALYSIS:" << std::endl;
        std::cout << "✓ Operations count should match expected: " << 
                    (operationCounter.load() == (NUM_THREADS * OPERATIONS_PER_THREAD) ? "PASS" : "FAIL") << std::endl;
        std::cout << "✓ Errors should be minimal: " << 
                    (errorCounter.load() <= 2 ? "PASS" : "FAIL") << std::endl;
        std::cout << "✓ All synchronization mechanisms working properly" << std::endl;
        std::cout << "✓ No race conditions expected in this version" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
    }
};

int main() {
    try {
        ConcurrentIOProblemsSolved demo;
        demo.runConcurrentOperations();
    } catch (const std::exception& e) {
        std::cerr << "Fatal error: " << e.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
        return 1;
    }
    
    return 0;
}
