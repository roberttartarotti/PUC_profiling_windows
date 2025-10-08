/*
 * =====================================================================================
 * DISK I/O PERFORMANCE PROBLEMS DEMONSTRATION - C++ (MODULE 3, CLASS 2)
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
 * 
 * Expected Performance Impact:
 * - CPU usage: High (blocking I/O)
 * - Disk I/O: Excessive (small buffers, random access)
 * - Memory usage: Inefficient (no caching)
 * - Response time: Poor (synchronous operations)
 * - Throughput: Low (inefficient patterns)
 * 
 * Compile with: cl /EHsc /std:c++17 example2-m3p2e2-disk-io-problems.cpp
 * =====================================================================================
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
#include <sstream>
#include <iomanip>
#include <windows.h>

using namespace std;
using namespace std::chrono;
using namespace std::filesystem;

// =====================================================================================
// CONFIGURATION PARAMETERS - DESIGNED TO CAUSE SEVERE PERFORMANCE PROBLEMS
// =====================================================================================

const int SMALL_BUFFER_SIZE = 64;              // Very small buffer - causes excessive disk access
const int FILE_COUNT = 100;                     // Many files to manage
const int OPERATIONS_PER_FILE = 1000;           // Many operations per file
const int MAX_FILE_SIZE = 10 * 1024 * 1024;    // 10MB files
const int THREAD_COUNT = 20;                    // Many threads causing contention
const string BASE_DIRECTORY = "disk_io_problems_test/";
const string DATA_FILE_PREFIX = "data_";

// =====================================================================================
// STATISTICS AND METRICS
// =====================================================================================

struct DiskStats {
    atomic<long long> TotalBytesWritten{0};
    atomic<long long> TotalBytesRead{0};
    atomic<long long> TotalWriteOperations{0};
    atomic<long long> TotalReadOperations{0};
    atomic<long long> TotalFileOpens{0};
    atomic<long long> TotalFileCloses{0};
    atomic<long long> TotalSeekOperations{0};
    atomic<int> ActiveThreads{0};
};

DiskStats g_stats;
mutex g_fileMutex;  // PROBLEM: Global lock causing massive contention
bool g_running = true;

// =====================================================================================
// PROBLEM 1: SYNCHRONOUS I/O WITH SMALL BUFFERS
// =====================================================================================

void SynchronousSmallBufferWrites(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, FILE_COUNT - 1);
    uniform_int_distribution<> dataDis(0, 255);
    
    for (int i = 0; i < OPERATIONS_PER_FILE && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + DATA_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Global lock on every operation
            {
                lock_guard<mutex> lock(g_fileMutex);
                
                // PROBLEM: Open file for every small write
                ofstream file(filename, ios::binary | ios::app);
                g_stats.TotalFileOpens++;
                
                if (file) {
                    // PROBLEM: Very small buffer (64 bytes) causing excessive I/O
                    char buffer[SMALL_BUFFER_SIZE];
                    for (int j = 0; j < SMALL_BUFFER_SIZE; j++) {
                        buffer[j] = dataDis(gen);
                    }
                    
                    // PROBLEM: Synchronous write blocking the thread
                    file.write(buffer, SMALL_BUFFER_SIZE);
                    file.flush();  // Force immediate write
                    
                    g_stats.TotalBytesWritten += SMALL_BUFFER_SIZE;
                    g_stats.TotalWriteOperations++;
                }
                
                // PROBLEM: Close file after every operation
                file.close();
                g_stats.TotalFileCloses++;
            }
            
            // PROBLEM: Minimal delay causes thread thrashing
            this_thread::sleep_for(milliseconds(1));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// =====================================================================================
// PROBLEM 2: RANDOM ACCESS PATTERN WITH SEEKS
// =====================================================================================

void RandomAccessReads(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, FILE_COUNT - 1);
    uniform_int_distribution<> seekDis(0, MAX_FILE_SIZE - SMALL_BUFFER_SIZE);
    
    for (int i = 0; i < OPERATIONS_PER_FILE && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + DATA_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Global lock
            {
                lock_guard<mutex> lock(g_fileMutex);
                
                // PROBLEM: Open for every read
                ifstream file(filename, ios::binary);
                g_stats.TotalFileOpens++;
                
                if (file) {
                    // PROBLEM: Random seek causing disk head movement
                    file.seekg(seekDis(gen), ios::beg);
                    g_stats.TotalSeekOperations++;
                    
                    // PROBLEM: Small read
                    char buffer[SMALL_BUFFER_SIZE];
                    file.read(buffer, SMALL_BUFFER_SIZE);
                    
                    g_stats.TotalBytesRead += file.gcount();
                    g_stats.TotalReadOperations++;
                }
                
                file.close();
                g_stats.TotalFileCloses++;
            }
            
            this_thread::sleep_for(milliseconds(1));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// =====================================================================================
// PROBLEM 3: FREQUENT FILE OPERATIONS
// =====================================================================================

void FrequentFileOperations(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, FILE_COUNT - 1);
    
    for (int i = 0; i < OPERATIONS_PER_FILE && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + DATA_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Open, write, close for EVERY operation
            {
                lock_guard<mutex> lock(g_fileMutex);
                
                // Open
                ofstream file(filename, ios::binary | ios::app);
                g_stats.TotalFileOpens++;
                
                if (file) {
                    char buffer[SMALL_BUFFER_SIZE] = {0};
                    file.write(buffer, SMALL_BUFFER_SIZE);
                    file.flush();
                    
                    g_stats.TotalBytesWritten += SMALL_BUFFER_SIZE;
                    g_stats.TotalWriteOperations++;
                }
                
                // Close immediately
                file.close();
                g_stats.TotalFileCloses++;
            }
            
            // PROBLEM: Repeat immediately without any batching
            this_thread::sleep_for(microseconds(500));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// =====================================================================================
// PROBLEM 4: MIXED RANDOM OPERATIONS
// =====================================================================================

void MixedRandomOperations(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, FILE_COUNT - 1);
    uniform_int_distribution<> opDis(0, 2);  // 0=read, 1=write, 2=seek
    
    for (int i = 0; i < OPERATIONS_PER_FILE && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + DATA_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Mixed operations causing random I/O pattern
            {
                lock_guard<mutex> lock(g_fileMutex);
                
                int operation = opDis(gen);
                
                if (operation == 0) {
                    // Random read
                    ifstream file(filename, ios::binary);
                    g_stats.TotalFileOpens++;
                    
                    if (file) {
                        char buffer[SMALL_BUFFER_SIZE];
                        file.read(buffer, SMALL_BUFFER_SIZE);
                        g_stats.TotalBytesRead += file.gcount();
                        g_stats.TotalReadOperations++;
                    }
                    
                    file.close();
                    g_stats.TotalFileCloses++;
                    
                } else if (operation == 1) {
                    // Random write
                    ofstream file(filename, ios::binary | ios::app);
                    g_stats.TotalFileOpens++;
                    
                    if (file) {
                        char buffer[SMALL_BUFFER_SIZE] = {0};
                        file.write(buffer, SMALL_BUFFER_SIZE);
                        file.flush();
                        g_stats.TotalBytesWritten += SMALL_BUFFER_SIZE;
                        g_stats.TotalWriteOperations++;
                    }
                    
                    file.close();
                    g_stats.TotalFileCloses++;
                    
                } else {
                    // Random seek
                    fstream file(filename, ios::binary | ios::in | ios::out);
                    g_stats.TotalFileOpens++;
                    
                    if (file) {
                        file.seekg(0, ios::end);
                        auto size = file.tellg();
                        if (size > SMALL_BUFFER_SIZE) {
                            uniform_int_distribution<> seekPosDis(0, (int)size - SMALL_BUFFER_SIZE);
                            file.seekg(seekPosDis(gen), ios::beg);
                            g_stats.TotalSeekOperations++;
                        }
                    }
                    
                    file.close();
                    g_stats.TotalFileCloses++;
                }
            }
            
            this_thread::sleep_for(milliseconds(1));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// =====================================================================================
// SETUP AND MONITORING
// =====================================================================================

void CreateTestFiles() {
    create_directories(BASE_DIRECTORY);
    
    cout << "Creating " << FILE_COUNT << " test files..." << endl;
    
    // Create initial files
    for (int i = 0; i < FILE_COUNT; i++) {
        string filename = BASE_DIRECTORY + DATA_FILE_PREFIX + to_string(i) + ".dat";
        ofstream file(filename, ios::binary);
        
        // Write some initial data
        vector<char> buffer(1024, 0);
        file.write(buffer.data(), buffer.size());
        file.close();
    }
    
    cout << "Test files created" << endl;
}

void MonitorPerformance() {
    auto startTime = steady_clock::now();
    long long lastWritten = 0;
    long long lastRead = 0;
    
    while (g_running) {
        this_thread::sleep_for(seconds(1));
        
        auto runtime = duration_cast<seconds>(steady_clock::now() - startTime).count();
        if (runtime == 0) runtime = 1;
        
        long long currentWritten = g_stats.TotalBytesWritten.load();
        long long currentRead = g_stats.TotalBytesRead.load();
        
        double writtenPerSec = (currentWritten - lastWritten);
        double readPerSec = (currentRead - lastRead);
        
        system("cls");
        cout << "=======================================================" << endl;
        cout << "  DISK I/O PROBLEMS Demonstration - Real-Time Stats" << endl;
        cout << "=======================================================" << endl;
        cout << endl;
        
        cout << "Runtime: " << (runtime / 60) << "m " << (runtime % 60) << "s" << endl;
        cout << endl;
        
        cout << "Disk I/O Throughput:" << endl;
        cout << "  Write Rate:   " << fixed << setprecision(2)
             << (writtenPerSec / 1024) << " KB/s" << endl;
        cout << "  Read Rate:    " << fixed << setprecision(2)
             << (readPerSec / 1024) << " KB/s" << endl;
        cout << endl;
        
        cout << "Operation Counts:" << endl;
        cout << "  Write Operations:  " << g_stats.TotalWriteOperations.load() << endl;
        cout << "  Read Operations:   " << g_stats.TotalReadOperations.load() << endl;
        cout << "  File Opens:        " << g_stats.TotalFileOpens.load() << endl;
        cout << "  File Closes:       " << g_stats.TotalFileCloses.load() << endl;
        cout << "  Seek Operations:   " << g_stats.TotalSeekOperations.load() << endl;
        cout << endl;
        
        cout << "Efficiency Metrics:" << endl;
        
        long long totalOps = g_stats.TotalWriteOperations.load() + g_stats.TotalReadOperations.load();
        if (totalOps > 0) {
            double avgBytes = (currentWritten + currentRead) / (double)totalOps;
            cout << "  Avg Bytes/Operation: " << fixed << setprecision(1)
                 << avgBytes << " bytes (TINY!)" << endl;
        }
        
        if (g_stats.TotalWriteOperations > 0) {
            double opsPerOpen = g_stats.TotalFileOpens.load() / (double)g_stats.TotalWriteOperations.load();
            cout << "  File Opens per Op:   " << fixed << setprecision(2)
                 << opsPerOpen << " (EXCESSIVE!)" << endl;
        }
        cout << endl;
        
        cout << "Threading:" << endl;
        cout << "  Active Threads: " << g_stats.ActiveThreads.load() << endl;
        cout << endl;
        
        cout << "Cumulative:" << endl;
        cout << "  Total Written: " << (currentWritten / 1024 / 1024) << " MB" << endl;
        cout << "  Total Read:    " << (currentRead / 1024 / 1024) << " MB" << endl;
        cout << endl;
        
        cout << "PROBLEMS YOU SHOULD SEE IN PERFMON:" << endl;
        cout << "  x HIGH Disk Queue Length (thread contention)" << endl;
        cout << "  x LOW Disk Bytes/sec (inefficient I/O)" << endl;
        cout << "  x TINY Avg Bytes/Transfer (~" << SMALL_BUFFER_SIZE << " bytes)" << endl;
        cout << "  x HIGH % Disk Time (constant activity)" << endl;
        cout << "  x EXCESSIVE File Opens: " << g_stats.TotalFileOpens.load() << endl;
        cout << endl;
        
        cout << "Press Ctrl+C to stop..." << endl;
        
        lastWritten = currentWritten;
        lastRead = currentRead;
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

// =====================================================================================
// MAIN FUNCTION
// =====================================================================================

int main() {
    SetConsoleCtrlHandler(ConsoleHandler, TRUE);
    
    cout << "=======================================================" << endl;
    cout << "  DISK I/O PERFORMANCE PROBLEMS DEMONSTRATION" << endl;
    cout << "  WARNING: This code demonstrates BAD practices!" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "PROBLEM CONFIGURATION:" << endl;
    cout << "x Synchronous I/O (blocks threads)" << endl;
    cout << "x Small buffers (" << SMALL_BUFFER_SIZE << " bytes - excessive I/O)" << endl;
    cout << "x Random access patterns (disk thrashing)" << endl;
    cout << "x Frequent file open/close (overhead)" << endl;
    cout << "x No caching (repeated disk access)" << endl;
    cout << "x No batching (inefficient)" << endl;
    cout << "x Global lock (massive contention)" << endl;
    cout << "x " << THREAD_COUNT << " threads (thread contention)" << endl;
    cout << endl;
    
    cout << "Expected PerfMon Impact:" << endl;
    cout << "- Avg. Disk Queue Length: 5-30 (very high)" << endl;
    cout << "- Disk Bytes/sec: Low (despite activity)" << endl;
    cout << "- Avg. Disk Bytes/Transfer: ~" << SMALL_BUFFER_SIZE << " bytes (terrible)" << endl;
    cout << "- % Disk Time: Near 100%" << endl;
    cout << endl;
    
    cout << "Press any key to start problematic demonstration..." << endl;
    cin.get();
    
    cout << endl;
    CreateTestFiles();
    cout << endl;
    
    // Start monitoring
    thread monitorThread(MonitorPerformance);
    
    // Start problem threads
    vector<thread> threads;
    
    // Different types of problematic operations
    int threadsPerType = THREAD_COUNT / 4;
    
    // Type 1: Small buffer writes
    for (int i = 0; i < threadsPerType; i++) {
        threads.push_back(thread([i]() {
            g_stats.ActiveThreads++;
            SynchronousSmallBufferWrites(i);
            g_stats.ActiveThreads--;
        }));
    }
    
    // Type 2: Random access reads
    for (int i = 0; i < threadsPerType; i++) {
        threads.push_back(thread([i]() {
            g_stats.ActiveThreads++;
            RandomAccessReads(i + 100);
            g_stats.ActiveThreads--;
        }));
    }
    
    // Type 3: Frequent file operations
    for (int i = 0; i < threadsPerType; i++) {
        threads.push_back(thread([i]() {
            g_stats.ActiveThreads++;
            FrequentFileOperations(i + 200);
            g_stats.ActiveThreads--;
        }));
    }
    
    // Type 4: Mixed random operations
    for (int i = 0; i < threadsPerType; i++) {
        threads.push_back(thread([i]() {
            g_stats.ActiveThreads++;
            MixedRandomOperations(i + 300);
            g_stats.ActiveThreads--;
        }));
    }
    
    cout << "Started " << THREAD_COUNT << " problem threads" << endl;
    cout << "Generating problematic disk I/O patterns..." << endl;
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
    cout << "         FINAL STATISTICS - PROBLEM VERSION" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "Total Operations:" << endl;
    cout << "  Write Operations:  " << g_stats.TotalWriteOperations.load() << endl;
    cout << "  Read Operations:   " << g_stats.TotalReadOperations.load() << endl;
    cout << "  File Opens:        " << g_stats.TotalFileOpens.load() << endl;
    cout << "  File Closes:       " << g_stats.TotalFileCloses.load() << endl;
    cout << "  Seek Operations:   " << g_stats.TotalSeekOperations.load() << endl;
    cout << endl;
    
    cout << "Data Transfer:" << endl;
    cout << "  Total Written: " << (g_stats.TotalBytesWritten / 1024 / 1024) << " MB" << endl;
    cout << "  Total Read:    " << (g_stats.TotalBytesRead / 1024 / 1024) << " MB" << endl;
    cout << endl;
    
    long long totalOps = g_stats.TotalWriteOperations.load() + g_stats.TotalReadOperations.load();
    if (totalOps > 0) {
        double avgBytes = (g_stats.TotalBytesWritten.load() + g_stats.TotalBytesRead.load()) / (double)totalOps;
        cout << "Efficiency:" << endl;
        cout << "  Avg Bytes/Operation: " << fixed << setprecision(1) << avgBytes << " bytes" << endl;
    }
    cout << endl;
    
    cout << "PROBLEMS DEMONSTRATED:" << endl;
    cout << "x Tiny read/write operations (low Avg Bytes/Transfer)" << endl;
    cout << "x Excessive file opens/closes (" << g_stats.TotalFileOpens.load() << ")" << endl;
    cout << "x Random access patterns (" << g_stats.TotalSeekOperations.load() << " seeks)" << endl;
    cout << "x Synchronous blocking I/O" << endl;
    cout << "x Thread contention on global lock" << endl;
    cout << "x No caching or batching" << endl;
    cout << endl;
    
    cout << "Cleaning up test files..." << endl;
    try {
        remove_all(BASE_DIRECTORY);
    } catch (...) {
        cout << "Note: You may need to manually delete: " << BASE_DIRECTORY << endl;
    }
    
    return 0;
}

