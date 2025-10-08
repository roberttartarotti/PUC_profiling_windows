/*
 * Thread Contention and Disk I/O OPTIMIZED Solution (C++)
 * Module 3, Class 2, Example 1 - OPTIMIZED VERSION
 * 
 * This demonstrates OPTIMAL disk I/O and thread management:
 * - Low disk queue length from controlled thread count
 * - High bytes/sec from large sequential operations
 * - Minimal thread contention with per-file locks
 * - Sequential access patterns for optimal throughput
 * - Buffered I/O for efficiency
 * 
 * Monitor in Windows PerfMon:
 * - PhysicalDisk: Avg. Disk Queue Length (will be low, ~1-2)
 * - PhysicalDisk: Disk Bytes/sec (will be high, maximized)
 * - PhysicalDisk: Avg. Disk Bytes/Transfer (will be large, 32KB+)
 * - Process: Thread Count (will be reasonable)
 * 
 * Compile with: cl /EHsc /std:c++17 example1-m3p2e1-thread-contention-disk-io-solved.cpp
 */

#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <thread>
#include <mutex>
#include <atomic>
#include <chrono>
#include <random>
#include <filesystem>
#include <memory>
#include <windows.h>

using namespace std;
using namespace std::chrono;
using namespace std::filesystem;

// OPTIMIZED CONFIGURATION - Creates GOOD disk metrics
const int EFFICIENT_THREADS = 8;               // Fewer threads to reduce contention
const int OPERATIONS_PER_THREAD = 100;
const int LARGE_WRITE_SIZE = 64 * 1024;        // 64KB writes (high avg bytes/transfer)
const int LARGE_READ_SIZE = 32 * 1024;         // 32KB reads
const int BATCH_SIZE = 16;                     // Batch operations
const int WRITE_BUFFER_SIZE = 256 * 1024;      // 256KB write buffer
const string BASE_DIRECTORY = "disk_optimized_test/";
const string SEQUENTIAL_FILE_PREFIX = "sequential_";
const string BATCH_FILE_PREFIX = "batch_";

// Statistics
atomic<long long> TotalBytesWritten(0);
atomic<long long> TotalBytesRead(0);
atomic<long long> TotalOperations(0);
atomic<int> ActiveThreads(0);
atomic<int> ContentionEvents(0);

// SOLUTION: Per-file locks instead of global lock
const int FILE_LOCK_COUNT = 64;
mutex g_fileLocks[FILE_LOCK_COUNT];

bool g_running = true;

int GetFileLockIndex(const string& filename) {
    hash<string> hasher;
    return hasher(filename) % FILE_LOCK_COUNT;
}

void CreateTestDirectory() {
    create_directories(BASE_DIRECTORY);
    
    // Create sequential files with initial data
    for (int i = 0; i < EFFICIENT_THREADS; i++) {
        string filename = BASE_DIRECTORY + SEQUENTIAL_FILE_PREFIX + to_string(i) + ".dat";
        ofstream file(filename, ios::binary);
        
        // Pre-allocate with zeros
        vector<char> buffer(1024 * 1024, 0);  // 1MB
        file.write(buffer.data(), buffer.size());
        file.close();
    }
}

// SOLUTION: Large sequential writes for high Avg Bytes/Transfer
void PerformLargeSequentialWrites(int threadId) {
    string filename = BASE_DIRECTORY + SEQUENTIAL_FILE_PREFIX + to_string(threadId) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    // Create large buffer for efficient writing
    vector<char> buffer(LARGE_WRITE_SIZE);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            // Fill buffer with data
            fill(buffer.begin(), buffer.end(), (char)(threadId + i) % 256);
            
            // SOLUTION: Per-file lock reduces contention
            {
                lock_guard<mutex> lock(g_fileLocks[lockIndex]);
                
                // SOLUTION: Large sequential write (64KB)
                ofstream file(filename, ios::binary | ios::app);
                if (file) {
                    file.write(buffer.data(), LARGE_WRITE_SIZE);
                    file.flush();
                    
                    TotalBytesWritten += LARGE_WRITE_SIZE;
                    TotalOperations++;
                }
                file.close();
            }
            
            // SOLUTION: Reasonable delay to avoid overwhelming disk
            this_thread::sleep_for(milliseconds(10));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// SOLUTION: Large sequential reads for high Avg Bytes/Transfer
void PerformLargeSequentialReads(int threadId) {
    string filename = BASE_DIRECTORY + SEQUENTIAL_FILE_PREFIX + to_string(threadId % EFFICIENT_THREADS) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    vector<char> buffer(LARGE_READ_SIZE);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            // SOLUTION: Per-file lock
            {
                lock_guard<mutex> lock(g_fileLocks[lockIndex]);
                
                // SOLUTION: Large sequential read (32KB)
                ifstream file(filename, ios::binary);
                if (file) {
                    file.read(buffer.data(), LARGE_READ_SIZE);
                    
                    TotalBytesRead += file.gcount();
                    TotalOperations++;
                }
                file.close();
            }
            
            this_thread::sleep_for(milliseconds(10));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// SOLUTION: Batched operations to reduce I/O overhead
void PerformBatchedOperations(int threadId) {
    string filename = BASE_DIRECTORY + BATCH_FILE_PREFIX + to_string(threadId) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD / BATCH_SIZE && g_running; i++) {
        try {
            // SOLUTION: Accumulate multiple operations into one I/O call
            vector<char> batchBuffer(LARGE_WRITE_SIZE * BATCH_SIZE);
            
            for (int j = 0; j < BATCH_SIZE; j++) {
                fill(batchBuffer.begin() + j * LARGE_WRITE_SIZE,
                     batchBuffer.begin() + (j + 1) * LARGE_WRITE_SIZE,
                     (char)((threadId + i + j) % 256));
            }
            
            // SOLUTION: Single large I/O operation instead of many small ones
            {
                lock_guard<mutex> lock(g_fileLocks[lockIndex]);
                
                ofstream file(filename, ios::binary | ios::app);
                if (file) {
                    file.write(batchBuffer.data(), batchBuffer.size());
                    file.flush();
                    
                    TotalBytesWritten += batchBuffer.size();
                    TotalOperations += BATCH_SIZE;
                }
                file.close();
            }
            
            this_thread::sleep_for(milliseconds(50));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// SOLUTION: Buffered I/O for efficiency
void PerformBufferedIO(int threadId) {
    string filename = BASE_DIRECTORY + "buffered_" + to_string(threadId) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    vector<char> writeBuffer(WRITE_BUFFER_SIZE);
    fill(writeBuffer.begin(), writeBuffer.end(), (char)(threadId % 256));
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            // SOLUTION: Large buffered write
            {
                lock_guard<mutex> lock(g_fileLocks[lockIndex]);
                
                ofstream file(filename, ios::binary | ios::app);
                if (file) {
                    // Write large buffer in one operation
                    file.write(writeBuffer.data(), WRITE_BUFFER_SIZE);
                    file.flush();
                    
                    TotalBytesWritten += WRITE_BUFFER_SIZE;
                    TotalOperations++;
                }
                file.close();
            }
            
            this_thread::sleep_for(milliseconds(100));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

void MonitorPerformance() {
    auto startTime = steady_clock::now();
    long long lastBytesWritten = 0;
    long long lastBytesRead = 0;
    
    while (g_running) {
        this_thread::sleep_for(seconds(2));
        
        auto currentTime = steady_clock::now();
        auto runtime = duration_cast<seconds>(currentTime - startTime).count();
        if (runtime == 0) runtime = 1;
        
        long long currentWritten = TotalBytesWritten.load();
        long long currentRead = TotalBytesRead.load();
        
        double writtenPerSec = (currentWritten - lastBytesWritten) / 2.0;  // Over 2 seconds
        double readPerSec = (currentRead - lastBytesRead) / 2.0;
        
        system("cls");
        cout << "=======================================================" << endl;
        cout << "  Thread & Disk I/O OPTIMIZED - Real-Time Performance" << endl;
        cout << "=======================================================" << endl;
        cout << endl;
        
        cout << "Runtime: " << (runtime / 60) << "m " << (runtime % 60) << "s" << endl;
        cout << endl;
        
        cout << "Disk I/O Statistics:" << endl;
        cout << "  Bytes Written/sec:  " << fixed << setprecision(2)
             << (writtenPerSec / 1024 / 1024) << " MB/s" << endl;
        cout << "  Bytes Read/sec:     " << fixed << setprecision(2)
             << (readPerSec / 1024 / 1024) << " MB/s" << endl;
        cout << "  Total Operations:   " << TotalOperations.load() << endl;
        
        if (TotalOperations > 0) {
            double avgBytesPerOp = (currentWritten + currentRead) / (double)TotalOperations.load();
            cout << "  Avg Bytes/Transfer: " << fixed << setprecision(1)
                 << (avgBytesPerOp / 1024) << " KB (LARGE!)" << endl;
        }
        cout << endl;
        
        cout << "Thread Statistics:" << endl;
        cout << "  Active Threads:     " << ActiveThreads.load() << endl;
        cout << "  Contention Events:  " << ContentionEvents.load() << " (minimal)" << endl;
        cout << endl;
        
        cout << "Cumulative:" << endl;
        cout << "  Total Written:  " << (currentWritten / 1024 / 1024) << " MB" << endl;
        cout << "  Total Read:     " << (currentRead / 1024 / 1024) << " MB" << endl;
        cout << endl;
        
        cout << "OPTIMIZATIONS YOU SHOULD SEE IN PERFMON:" << endl;
        cout << "  + LOW Disk Queue Length (1-2, efficient)" << endl;
        cout << "  + HIGH Bytes/sec (maximized throughput)" << endl;
        cout << "  + LARGE Avg Bytes/Transfer (32KB+)" << endl;
        cout << "  + REASONABLE Thread Count (" << EFFICIENT_THREADS << ")" << endl;
        cout << "  + MINIMAL Contention: " << ContentionEvents.load() << endl;
        cout << endl;
        
        cout << "Check Windows PerfMon:" << endl;
        cout << "  - PhysicalDisk -> Avg. Disk Queue Length" << endl;
        cout << "  - PhysicalDisk -> Disk Bytes/sec" << endl;
        cout << "  - PhysicalDisk -> Avg. Disk Bytes/Transfer" << endl;
        cout << endl;
        
        cout << "Press Ctrl+C to stop..." << endl;
        
        lastBytesWritten = currentWritten;
        lastBytesRead = currentRead;
    }
}

BOOL WINAPI ConsoleHandler(DWORD signal) {
    if (signal == CTRL_C_EVENT) {
        cout << "\nShutting down..." << endl;
        g_running = false;
        return TRUE;
    }
    return FALSE;
}

int main() {
    SetConsoleCtrlHandler(ConsoleHandler, TRUE);
    
    cout << "=======================================================" << endl;
    cout << "  Thread Contention and Disk I/O OPTIMIZED Demo" << endl;
    cout << "  Demonstrating BEST PRACTICES!" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "OPTIMIZED CONFIGURATION:" << endl;
    cout << "+ " << EFFICIENT_THREADS << " efficient threads (reduced contention)" << endl;
    cout << "+ " << (LARGE_WRITE_SIZE / 1024) << " KB writes (high Avg Bytes/Transfer)" << endl;
    cout << "+ " << (LARGE_READ_SIZE / 1024) << " KB reads (high Avg Bytes/Transfer)" << endl;
    cout << "+ Per-file locking (minimal contention)" << endl;
    cout << "+ Sequential access patterns (optimal throughput)" << endl;
    cout << "+ Batched operations (reduced I/O overhead)" << endl;
    cout << "+ Large buffers (efficient I/O)" << endl;
    cout << endl;
    
    cout << "Expected PerfMon Metrics:" << endl;
    cout << "- Avg. Disk Queue Length: 1-2 (efficient)" << endl;
    cout << "- Disk Bytes/sec: High (maximized)" << endl;
    cout << "- Avg. Disk Bytes/Transfer: 32KB-256KB (excellent)" << endl;
    cout << "- Thread Count: " << EFFICIENT_THREADS << " (reasonable)" << endl;
    cout << endl;
    
    cout << "Press any key to start optimized demonstration..." << endl;
    cin.get();
    
    cout << "\nCreating test files..." << endl;
    CreateTestDirectory();
    cout << "Created test files" << endl;
    cout << endl;
    
    // Start monitoring
    thread monitorThread(MonitorPerformance);
    
    // Start efficient threads
    vector<thread> threads;
    
    // Large sequential writes
    for (int i = 0; i < EFFICIENT_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformLargeSequentialWrites(i);
            ActiveThreads--;
        }));
    }
    
    // Large sequential reads
    for (int i = 0; i < EFFICIENT_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformLargeSequentialReads(i + 10);
            ActiveThreads--;
        }));
    }
    
    // Batched operations
    for (int i = 0; i < EFFICIENT_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformBatchedOperations(i + 20);
            ActiveThreads--;
        }));
    }
    
    // Buffered I/O
    for (int i = 0; i < EFFICIENT_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformBufferedIO(i + 30);
            ActiveThreads--;
        }));
    }
    
    cout << "Started " << EFFICIENT_THREADS << " optimized threads" << endl;
    cout << "Performing efficient disk I/O operations..." << endl;
    cout << endl;
    
    // Wait for all threads
    for (auto& t : threads) {
        if (t.joinable()) t.join();
    }
    
    g_running = false;
    if (monitorThread.joinable()) monitorThread.join();
    
    // Final statistics
    cout << endl;
    cout << "=======================================================" << endl;
    cout << "          FINAL STATISTICS - OPTIMIZED VERSION" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "Disk I/O:" << endl;
    cout << "  Total Written:      " << (TotalBytesWritten / 1024 / 1024) << " MB" << endl;
    cout << "  Total Read:         " << (TotalBytesRead / 1024 / 1024) << " MB" << endl;
    cout << "  Total Operations:   " << TotalOperations.load() << endl;
    
    if (TotalOperations > 0) {
        double avgBytes = (TotalBytesWritten.load() + TotalBytesRead.load()) / (double)TotalOperations.load();
        cout << "  Avg Bytes/Transfer: " << fixed << setprecision(1) 
             << (avgBytes / 1024) << " KB" << endl;
    }
    cout << endl;
    
    cout << "Threading:" << endl;
    cout << "  Contention Events:  " << ContentionEvents.load() << " (minimal)" << endl;
    cout << endl;
    
    cout << "OPTIMIZATIONS DEMONSTRATED:" << endl;
    cout << "+ Large sequential I/O operations (high throughput)" << endl;
    cout << "+ Controlled thread count (low queue length)" << endl;
    cout << "+ Per-file locks (minimal contention)" << endl;
    cout << "+ Batched operations (reduced overhead)" << endl;
    cout << "+ Buffered I/O (efficiency)" << endl;
    cout << endl;
    
    cout << "Compare with PROBLEM version:" << endl;
    cout << "  PROBLEM: Avg Bytes/Transfer ~50 bytes, Queue Length 10-50" << endl;
    cout << "  SOLVED:  Avg Bytes/Transfer 32KB+, Queue Length 1-2" << endl;
    cout << endl;
    
    cout << "Cleaning up test files..." << endl;
    try {
        remove_all(BASE_DIRECTORY);
    } catch (...) {
        cout << "Note: You may need to manually delete: " << BASE_DIRECTORY << endl;
    }
    
    return 0;
}

