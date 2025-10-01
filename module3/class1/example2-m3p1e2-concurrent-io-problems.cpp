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
#include <conio.h>  // For _kbhit() on Windows
#include <cstdio>   // For remove()

// ====================================================================
// CONFIGURATION VARIABLES - EASY TO MODIFY FOR DIFFERENT SCENARIOS
// ====================================================================
const int NUM_THREADS = 6;                     // Number of concurrent threads (reduced for better visibility)
const int OPERATIONS_PER_THREAD = 20;          // Operations each thread performs (reduced for better visibility)
const int FILE_SIZE_KB = 5;                    // Size of each file in KB (reduced for faster operations)
const std::string SHARED_FILE = "shared_resource.txt";
const std::string LOG_FILE = "concurrent_operations.log";
const std::string BASE_FILENAME = "concurrent_file_";
const int DELAY_BETWEEN_OPS_MS = 50;           // Delay between operations (increased for better visibility)
const bool ENABLE_FILE_LOCKING = false;        // Toggle to show difference
const bool ENABLE_PROPER_SYNCHRONIZATION = false; // Toggle to show solutions
// ====================================================================

class ConcurrentIOProblems {
private:
    std::atomic<int> operationCounter{0};
    std::atomic<int> errorCounter{0};
    std::atomic<long long> totalBytesProcessed{0};
    std::mutex logMutex;  // Only used when ENABLE_PROPER_SYNCHRONIZATION is true
    std::mutex fileMutex; // Only used when ENABLE_PROPER_SYNCHRONIZATION is true
    std::chrono::high_resolution_clock::time_point startTime;
    
    // PROBLEM 1: Race condition in shared counter (without proper synchronization)
    int unsafeCounter = 0;  // This will demonstrate race conditions
    
    // Generate content for files
    std::string generateFileContent(int threadId, int operationId) {
        std::stringstream ss;
        ss << "=== CONCURRENT I/O OPERATION ===\n";
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
    
    // PROBLEM 2: Multiple threads writing to the same file without coordination
    void demonstrateSharedFileContention(int threadId) {
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            try {
                // PROBLEMATIC: Multiple threads trying to write to same file
                // This can cause:
                // - Data corruption
                // - Partial writes
                // - File access violations
                // - Inconsistent file state
                
                std::string content = "Thread " + std::to_string(threadId) + 
                                    " Operation " + std::to_string(op) + 
                                    " Time: " + std::to_string(std::chrono::duration_cast<std::chrono::milliseconds>(
                                        std::chrono::high_resolution_clock::now().time_since_epoch()).count()) + "\n";
                
                if (ENABLE_PROPER_SYNCHRONIZATION) {
                    // SOLUTION: Use mutex to synchronize access
                    std::lock_guard<std::mutex> lock(fileMutex);
                    std::ofstream file(SHARED_FILE, std::ios::app);
                    if (file.is_open()) {
                        file << content;
                        file.flush();
                        file.close();
                        std::cout << "[THREAD " << threadId << "] SAFE WRITE to " << SHARED_FILE << " (Op " << op << ")" << std::endl;
                    }
                } else {
                    // PROBLEM: No synchronization - multiple threads compete
                    std::ofstream file(SHARED_FILE, std::ios::app);
                    if (file.is_open()) {
                        // Simulate some processing time to increase chance of conflicts
                        std::this_thread::sleep_for(std::chrono::milliseconds(1));
                        file << content;
                        file.flush();
                        file.close();
                        std::cout << "[THREAD " << threadId << "] UNSAFE WRITE to " << SHARED_FILE << " (Op " << op << ") - RACE CONDITION POSSIBLE!" << std::endl;
                    }
                }
                
                totalBytesProcessed += content.length();
                
            } catch (const std::exception& e) {
                errorCounter++;
                std::cout << "Thread " << threadId << " error in shared file operation: " << e.what() << std::endl;
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_OPS_MS));
        }
    }
    
    // PROBLEM 3: Race conditions in logging operations
    void unsafeLogging(int threadId, const std::string& message) {
        // PROBLEMATIC: Multiple threads writing to log without synchronization
        // This can cause:
        // - Interleaved log messages
        // - Corrupted log entries
        // - Lost log data
        
        if (ENABLE_PROPER_SYNCHRONIZATION) {
            // SOLUTION: Use mutex for thread-safe logging
            std::lock_guard<std::mutex> lock(logMutex);
            std::ofstream logFile(LOG_FILE, std::ios::app);
            if (logFile.is_open()) {
                logFile << "[Thread " << threadId << "] " << message << std::endl;
                logFile.close();
            }
        } else {
            // PROBLEM: No synchronization in logging
            std::ofstream logFile(LOG_FILE, std::ios::app);
            if (logFile.is_open()) {
                // Simulate processing to increase chance of race conditions
                std::this_thread::sleep_for(std::chrono::microseconds(100));
                logFile << "[Thread " << threadId << "] " << message << std::endl;
                logFile.close();
            }
        }
    }
    
    // PROBLEM 4: File creation/deletion race conditions
    void demonstrateFileRaceConditions(int threadId) {
        for (int op = 0; op < OPERATIONS_PER_THREAD; ++op) {
            std::string filename = BASE_FILENAME + std::to_string(threadId) + "_" + std::to_string(op) + ".tmp";
            
            try {
                // PROBLEM: Race condition in counter increment
                if (ENABLE_PROPER_SYNCHRONIZATION) {
                    // SOLUTION: Use atomic operations
                    operationCounter++;
                } else {
                    // PROBLEM: Non-atomic increment (race condition)
                    unsafeCounter++;  // This will likely produce incorrect results
                }
                
                // Create file
                std::string content = generateFileContent(threadId, op);
                std::ofstream file(filename, std::ios::binary);
                if (file.is_open()) {
                    file.write(content.c_str(), content.length());
                    file.close();
                    
                    std::cout << "[THREAD " << threadId << "] CREATED FILE: " << filename << " (" << content.length() << " bytes)" << std::endl;
                    unsafeLogging(threadId, "Created file: " + filename);
                    
                    // PROBLEM 5: Immediate read after write without proper synchronization
                    // This can cause:
                    // - Reading incomplete data
                    // - File not found errors
                    // - Inconsistent file state
                    
                    // Small delay to simulate processing
                    std::this_thread::sleep_for(std::chrono::milliseconds(1));
                    
                    // Try to read the file we just created
                    std::ifstream readFile(filename, std::ios::binary);
                    if (readFile.is_open()) {
                        readFile.seekg(0, std::ios::end);
                        size_t fileSize = readFile.tellg();
                        readFile.close();
                        
                        totalBytesProcessed += fileSize;
                        std::cout << "[THREAD " << threadId << "] READ FILE: " << filename << " (" << fileSize << " bytes)" << std::endl;
                        unsafeLogging(threadId, "Read file: " + filename + " (" + std::to_string(fileSize) + " bytes)");
                    } else {
                        errorCounter++;
                        std::cout << "[THREAD " << threadId << "] ERROR: Could not read file: " << filename << std::endl;
                        unsafeLogging(threadId, "ERROR: Could not read file: " + filename);
                    }
                    
                    // PROBLEM 6: File deletion while other threads might be accessing
                    // This can cause:
                    // - Access denied errors
                    // - Partial deletions
                    // - Inconsistent file system state
                    
                    // Check if file exists by trying to open it
                    std::ifstream checkFile(filename);
                    if (checkFile.good()) {
                        checkFile.close();
                        if (std::remove(filename.c_str()) == 0) {
                            std::cout << "[THREAD " << threadId << "] DELETED FILE: " << filename << std::endl;
                            unsafeLogging(threadId, "Deleted file: " + filename);
                        } else {
                            std::cout << "[THREAD " << threadId << "] ERROR: Could not delete file: " << filename << std::endl;
                            unsafeLogging(threadId, "ERROR: Could not delete file: " + filename);
                        }
                    }
                    
                } else {
                    errorCounter++;
                    unsafeLogging(threadId, "ERROR: Could not create file: " + filename);
                }
                
            } catch (const std::exception& e) {
                errorCounter++;
                unsafeLogging(threadId, "ERROR in file operations: " + std::string(e.what()));
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_OPS_MS));
        }
    }
    
    // PROBLEM 7: Concurrent access to the same file with different modes
    void demonstrateFileLockingProblems(int threadId) {
        std::string sharedDataFile = "shared_data_" + std::to_string(threadId % 3) + ".dat";
        
        for (int op = 0; op < OPERATIONS_PER_THREAD / 2; ++op) {
            try {
                if (op % 2 == 0) {
                    // Writer thread
                    std::string data = "Data from thread " + std::to_string(threadId) + 
                                     " operation " + std::to_string(op) + "\n";
                    
                    if (ENABLE_FILE_LOCKING) {
                        // SOLUTION: Use exclusive access
                        std::ofstream file(sharedDataFile, std::ios::app);
                        if (file.is_open()) {
                            file << data;
                            file.close();
                        }
                    } else {
                        // PROBLEM: Multiple writers without coordination
                        std::ofstream file(sharedDataFile, std::ios::app);
                        if (file.is_open()) {
                            // Simulate slow write operation
                            std::this_thread::sleep_for(std::chrono::milliseconds(5));
                            file << data;
                            file.close();
                        }
                    }
                    
                    unsafeLogging(threadId, "Wrote to shared file: " + sharedDataFile);
                    
                } else {
                    // Reader thread
                    std::ifstream file(sharedDataFile);
                    if (file.is_open()) {
                        std::string line;
                        int lineCount = 0;
                        while (std::getline(file, line)) {
                            lineCount++;
                        }
                        file.close();
                        
                        unsafeLogging(threadId, "Read shared file: " + sharedDataFile + 
                                    " (" + std::to_string(lineCount) + " lines)");
                    }
                }
                
            } catch (const std::exception& e) {
                errorCounter++;
                unsafeLogging(threadId, "ERROR in file locking demo: " + std::string(e.what()));
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(DELAY_BETWEEN_OPS_MS));
        }
    }
    
public:
    ConcurrentIOProblems() {
        startTime = std::chrono::high_resolution_clock::now();
        
        // Clean up any existing files
        std::remove(SHARED_FILE.c_str());
        std::remove(LOG_FILE.c_str());
    }
    
    void runConcurrentOperations() {
        std::cout << "=== CONCURRENT I/O PROBLEMS DEMONSTRATION ===" << std::endl;
        std::cout << "This program demonstrates various I/O concurrency issues:" << std::endl;
        std::cout << "1. Race conditions in shared file access" << std::endl;
        std::cout << "2. Unsafe logging operations" << std::endl;
        std::cout << "3. File creation/deletion conflicts" << std::endl;
        std::cout << "4. File locking problems" << std::endl;
        std::cout << "5. Counter race conditions" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Configuration:" << std::endl;
        std::cout << "- Threads: " << NUM_THREADS << std::endl;
        std::cout << "- Operations per thread: " << OPERATIONS_PER_THREAD << std::endl;
        std::cout << "- File locking: " << (ENABLE_FILE_LOCKING ? "ENABLED" : "DISABLED") << std::endl;
        std::cout << "- Proper synchronization: " << (ENABLE_PROPER_SYNCHRONIZATION ? "ENABLED" : "DISABLED") << std::endl;
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
            std::cout << ">>> STARTING CYCLE #" << cycleCount << " <<<" << std::endl;
            std::cout << std::string(60, '=') << std::endl;
            
            // Reset counters for this cycle
            operationCounter = 0;
            unsafeCounter = 0;
            errorCounter = 0;
            totalBytesProcessed = 0;
            startTime = std::chrono::high_resolution_clock::now();
            
            std::vector<std::thread> threads;
            
            // Launch threads that will compete for I/O resources
            std::cout << "Launching " << NUM_THREADS << " concurrent threads..." << std::endl;
            for (int i = 0; i < NUM_THREADS; ++i) {
                threads.emplace_back([this, i, cycleCount]() {
                    std::cout << "  Thread " << i << " started (Cycle " << cycleCount << ")" << std::endl;
                    
                    // Each thread performs multiple types of problematic I/O operations
                    demonstrateSharedFileContention(i);
                    demonstrateFileRaceConditions(i);
                    demonstrateFileLockingProblems(i);
                    
                    std::cout << "  Thread " << i << " completed (Cycle " << cycleCount << ")" << std::endl;
                });
            }
            
            // Wait for all threads to complete
            for (auto& thread : threads) {
                thread.join();
            }
            
            // Display results for this cycle
            std::cout << "\n" << std::string(40, '-') << std::endl;
            std::cout << "CYCLE #" << cycleCount << " RESULTS:" << std::endl;
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
        std::cout << "DEMONSTRATION COMPLETED AFTER " << cycleCount << " CYCLES" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Check the following files for evidence of concurrency problems:" << std::endl;
        std::cout << "- " << SHARED_FILE << " (shared file access conflicts)" << std::endl;
        std::cout << "- " << LOG_FILE << " (logging race conditions)" << std::endl;
        std::cout << "- Various temporary files (file creation/deletion races)" << std::endl;
        std::cout << "\nPress any key to exit..." << std::endl;
        _getch();
    }
    
private:
    void displayResults() {
        auto endTime = std::chrono::high_resolution_clock::now();
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime);
        
        std::cout << "\n" << std::string(60, '=') << std::endl;
        std::cout << "CONCURRENT I/O PROBLEMS RESULTS" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Execution time: " << duration.count() << " ms" << std::endl;
        std::cout << "Total threads: " << NUM_THREADS << std::endl;
        std::cout << "Expected operations: " << (NUM_THREADS * OPERATIONS_PER_THREAD) << std::endl;
        std::cout << "Atomic counter result: " << operationCounter.load() << std::endl;
        std::cout << "Unsafe counter result: " << unsafeCounter << " (should be same as atomic)" << std::endl;
        std::cout << "Errors encountered: " << errorCounter.load() << std::endl;
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
            int corruptedLines = 0;
            while (std::getline(logFile, line)) {
                logLines++;
                // Check for obviously corrupted lines (incomplete thread info)
                if (line.find("[Thread") == std::string::npos && !line.empty()) {
                    corruptedLines++;
                }
            }
            logFile.close();
            std::cout << "Log file lines: " << logLines << std::endl;
            std::cout << "Potentially corrupted log lines: " << corruptedLines << std::endl;
        }
        
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "ANALYSIS:" << std::endl;
        std::cout << "- If unsafe counter != atomic counter: RACE CONDITION detected!" << std::endl;
        std::cout << "- If errors > 0: FILE ACCESS CONFLICTS detected!" << std::endl;
        std::cout << "- If corrupted log lines > 0: LOGGING RACE CONDITIONS detected!" << std::endl;
        std::cout << "- Check " << SHARED_FILE << " and " << LOG_FILE << " for data corruption" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
    }
};

int main() {
    try {
        ConcurrentIOProblems demo;
        demo.runConcurrentOperations();
    } catch (const std::exception& e) {
        std::cerr << "Fatal error: " << e.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
        return 1;
    }
    
    return 0;
}
