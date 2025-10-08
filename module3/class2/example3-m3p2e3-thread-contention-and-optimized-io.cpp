/*
 * =====================================================================================
 * EXAMPLE 3 - THREAD CONTENTION VS OPTIMIZED DISK I/O (COMBINED DEMO) - C++
 * =====================================================================================
 * 
 * Based on:
 * - example1-m3p2e1-thread-contention-disk-io.cpp (creates contention/inefficiency)
 * - example2-m3p2e2-disk-io-solved.cpp (optimized I/O, batching, caching)
 * 
 * Goal: Toggle between two modes to show dramatic performance contrast:
 * - Problem mode: many tiny synchronous operations with random access and locking
 * - Optimized mode: large buffers, sequential access, batching, file handle reuse
 * 
 * Toggle mode by changing RUN_PROBLEM_MODE constant below.
 * Press Ctrl+C to stop.
 * 
 * Compile with: cl /EHsc /std:c++17 example3-m3p2e3-thread-contention-and-optimized-io.cpp
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
#include <iomanip>
#include <windows.h>

using namespace std;
using namespace std::chrono;
using namespace std::filesystem;

// =====================================================================================
// CONFIGURATION
// =====================================================================================

const bool RUN_PROBLEM_MODE = true;             // true = problem; false = optimized

// Problem mode settings
const int DISK_THRASHING_THREADS = 32;
const int TINY_WRITE_SIZE = 64;
const int TINY_READ_SIZE = 32;
const int RANDOM_FILES_COUNT = 200;
const int SEEK_OPERATIONS_PER_CYCLE = 50;
const string PROBLEM_BASE_DIR = "m3p2e3_problem/";

// Optimized mode settings
const int EFFICIENT_THREADS = 8;
const int LARGE_BUFFER_SIZE = 1024 * 1024;      // 1MB
const int MEDIUM_BUFFER_SIZE = 64 * 1024;       // 64KB
const int FILE_COUNT = 50;
const int BATCH_SIZE = 16;
const string OPTIMIZED_BASE_DIR = "m3p2e3_optimized/";

// =====================================================================================
// STATISTICS
// =====================================================================================

atomic<long long> g_totalOps(0);
atomic<long long> g_totalBytes(0);
atomic<int> g_activeThreads(0);
bool g_running = true;

// =====================================================================================
// PROBLEM MODE IMPLEMENTATION
// =====================================================================================

mutex g_problemLock;  // PROBLEM: Global lock causing contention
vector<string> g_problemFiles;

void SetupProblemMode() {
    create_directories(PROBLEM_BASE_DIR);
    
    cout << "Creating " << RANDOM_FILES_COUNT << " test files for problem mode..." << endl;
    
    for (int i = 0; i < RANDOM_FILES_COUNT; i++) {
        string filename = PROBLEM_BASE_DIR + "random_" + to_string(i) + ".dat";
        ofstream file(filename, ios::binary);
        
        vector<char> buffer(1024, 0);
        file.write(buffer.data(), buffer.size());
        file.close();
        
        g_problemFiles.push_back(filename);
    }
    
    cout << "Problem mode setup complete" << endl;
}

void ProblemTinyWrite(int threadId, int op) {
    // PROBLEM: Create many tiny files
    string filename = PROBLEM_BASE_DIR + "tiny_" + to_string(threadId) + "_" + to_string(op) + ".dat";
    
    vector<char> buffer(TINY_WRITE_SIZE, (char)(threadId % 256));
    
    // PROBLEM: Global lock on every operation
    {
        lock_guard<mutex> lock(g_problemLock);
        
        ofstream file(filename, ios::binary);
        if (file) {
            file.write(buffer.data(), TINY_WRITE_SIZE);
            file.flush();  // PROBLEM: Force to disk immediately
            
            g_totalBytes += TINY_WRITE_SIZE;
        }
        file.close();
    }
    
    g_totalOps++;
}

void ProblemTinyRead() {
    if (g_problemFiles.empty()) return;
    
    random_device rd;
    static thread_local mt19937 gen(rd());
    uniform_int_distribution<> dis(0, (int)g_problemFiles.size() - 1);
    
    string filename = g_problemFiles[dis(gen)];
    
    // PROBLEM: Global lock
    {
        lock_guard<mutex> lock(g_problemLock);
        
        ifstream file(filename, ios::binary);
        if (file) {
            vector<char> buffer(TINY_READ_SIZE);
            file.read(buffer.data(), TINY_READ_SIZE);
            
            g_totalBytes += file.gcount();
        }
        file.close();
    }
    
    g_totalOps++;
}

void ProblemRandomSeekBurst() {
    if (g_problemFiles.empty()) return;
    
    random_device rd;
    static thread_local mt19937 gen(rd());
    uniform_int_distribution<> fileDis(0, (int)g_problemFiles.size() - 1);
    
    for (int i = 0; i < SEEK_OPERATIONS_PER_CYCLE && g_running; i++) {
        string filename = g_problemFiles[fileDis(gen)];
        
        // PROBLEM: Global lock + random seeks
        {
            lock_guard<mutex> lock(g_problemLock);
            
            fstream file(filename, ios::binary | ios::in | ios::out);
            if (file) {
                // Random seek
                file.seekg(0, ios::end);
                auto size = file.tellg();
                if (size > 8) {
                    uniform_int_distribution<> seekDis(0, (int)size - 8);
                    file.seekg(seekDis(gen), ios::beg);
                }
                
                char buffer[8];
                file.read(buffer, 8);
                
                g_totalBytes += file.gcount();
            }
            file.close();
        }
        
        g_totalOps++;
    }
}

void ProblemWorkerThread(int threadId) {
    g_activeThreads++;
    
    int op = 0;
    while (g_running) {
        ProblemTinyWrite(threadId, op++);
        ProblemTinyRead();
        ProblemRandomSeekBurst();
        
        // Minimal delay causes thrashing
        this_thread::sleep_for(microseconds(100));
    }
    
    g_activeThreads--;
}

// =====================================================================================
// OPTIMIZED MODE IMPLEMENTATION
// =====================================================================================

const int FILE_LOCK_COUNT = 64;
mutex g_fileLocks[FILE_LOCK_COUNT];

int GetFileLockIndex(const string& filename) {
    hash<string> hasher;
    return hasher(filename) % FILE_LOCK_COUNT;
}

void SetupOptimizedMode() {
    create_directories(OPTIMIZED_BASE_DIR);
    
    cout << "Creating " << FILE_COUNT << " test files for optimized mode..." << endl;
    
    for (int i = 0; i < FILE_COUNT; i++) {
        string filename = OPTIMIZED_BASE_DIR + "file_" + to_string(i) + ".dat";
        ofstream file(filename, ios::binary);
        
        // Pre-allocate with initial data
        vector<char> buffer(MEDIUM_BUFFER_SIZE, 0);
        file.write(buffer.data(), buffer.size());
        file.close();
    }
    
    cout << "Optimized mode setup complete" << endl;
}

void OptimizedLargeWrite(int threadId) {
    string filename = OPTIMIZED_BASE_DIR + "file_" + to_string(threadId % FILE_COUNT) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    // SOLUTION: Large buffer
    vector<char> buffer(LARGE_BUFFER_SIZE, (char)((threadId) % 256));
    
    // SOLUTION: Per-file lock
    {
        lock_guard<mutex> lock(g_fileLocks[lockIndex]);
        
        ofstream file(filename, ios::binary | ios::app);
        if (file) {
            file.write(buffer.data(), LARGE_BUFFER_SIZE);
            // SOLUTION: Let OS handle flushing
            
            g_totalBytes += LARGE_BUFFER_SIZE;
        }
        file.close();
    }
    
    g_totalOps++;
}

void OptimizedSequentialRead(int threadId) {
    string filename = OPTIMIZED_BASE_DIR + "file_" + to_string(threadId % FILE_COUNT) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    // SOLUTION: Medium buffer for reads
    vector<char> buffer(MEDIUM_BUFFER_SIZE);
    
    // SOLUTION: Per-file lock
    {
        lock_guard<mutex> lock(g_fileLocks[lockIndex]);
        
        ifstream file(filename, ios::binary);
        if (file) {
            // SOLUTION: Sequential read (no random seeks)
            file.read(buffer.data(), MEDIUM_BUFFER_SIZE);
            
            g_totalBytes += file.gcount();
        }
        file.close();
    }
    
    g_totalOps++;
}

void OptimizedBatchedWrite(int threadId) {
    string filename = OPTIMIZED_BASE_DIR + "batch_" + to_string(threadId) + ".dat";
    int lockIndex = GetFileLockIndex(filename);
    
    // SOLUTION: Batch multiple operations into one large I/O
    vector<char> batchBuffer(LARGE_BUFFER_SIZE * BATCH_SIZE);
    
    for (int i = 0; i < BATCH_SIZE; i++) {
        fill(batchBuffer.begin() + i * LARGE_BUFFER_SIZE,
             batchBuffer.begin() + (i + 1) * LARGE_BUFFER_SIZE,
             (char)((threadId + i) % 256));
    }
    
    // SOLUTION: Single large write
    {
        lock_guard<mutex> lock(g_fileLocks[lockIndex]);
        
        ofstream file(filename, ios::binary | ios::app);
        if (file) {
            file.write(batchBuffer.data(), batchBuffer.size());
            
            g_totalBytes += batchBuffer.size();
        }
        file.close();
    }
    
    g_totalOps += BATCH_SIZE;
}

void OptimizedWorkerThread(int threadId) {
    g_activeThreads++;
    
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> opDis(0, 2);
    
    while (g_running) {
        int opType = opDis(gen);
        
        if (opType == 0) {
            OptimizedLargeWrite(threadId);
        } else if (opType == 1) {
            OptimizedSequentialRead(threadId);
        } else {
            OptimizedBatchedWrite(threadId);
        }
        
        // SOLUTION: Reasonable delay
        this_thread::sleep_for(milliseconds(10));
    }
    
    g_activeThreads--;
}

// =====================================================================================
// MONITORING
// =====================================================================================

void MonitorPerformance() {
    auto startTime = steady_clock::now();
    long long lastOps = 0;
    long long lastBytes = 0;
    
    while (g_running) {
        this_thread::sleep_for(seconds(1));
        
        auto runtime = duration_cast<seconds>(steady_clock::now() - startTime).count();
        if (runtime == 0) runtime = 1;
        
        long long currentOps = g_totalOps.load();
        long long currentBytes = g_totalBytes.load();
        
        double opsPerSec = (currentOps - lastOps);
        double mbPerSec = (currentBytes - lastBytes) / 1048576.0;
        
        system("cls");
        cout << "=======================================================" << endl;
        if (RUN_PROBLEM_MODE) {
            cout << "  PROBLEM MODE - Thread Contention & Tiny I/O" << endl;
        } else {
            cout << "  OPTIMIZED MODE - Efficient Disk I/O" << endl;
        }
        cout << "=======================================================" << endl;
        cout << endl;
        
        cout << "Runtime: " << (runtime / 60) << "m " << (runtime % 60) << "s" << endl;
        cout << endl;
        
        cout << "Performance:" << endl;
        cout << "  Operations/sec: " << fixed << setprecision(0) << opsPerSec << endl;
        cout << "  Throughput:     " << fixed << setprecision(2) << mbPerSec << " MB/s" << endl;
        cout << endl;
        
        cout << "Cumulative:" << endl;
        cout << "  Total Ops:   " << currentOps << endl;
        cout << "  Total Bytes: " << (currentBytes / 1024 / 1024) << " MB" << endl;
        cout << endl;
        
        if (currentOps > 0) {
            double avgBytes = currentBytes / (double)currentOps;
            cout << "Efficiency:" << endl;
            cout << "  Avg Bytes/Op: " << fixed << setprecision(1);
            
            if (avgBytes < 1024) {
                cout << avgBytes << " bytes";
            } else if (avgBytes < 1048576) {
                cout << (avgBytes / 1024) << " KB";
            } else {
                cout << (avgBytes / 1048576) << " MB";
            }
            
            if (RUN_PROBLEM_MODE && avgBytes < 1024) {
                cout << " (TINY!)";
            } else if (!RUN_PROBLEM_MODE && avgBytes > 32768) {
                cout << " (LARGE!)";
            }
            cout << endl;
        }
        cout << endl;
        
        cout << "Threading:" << endl;
        cout << "  Active Threads: " << g_activeThreads.load() << endl;
        cout << endl;
        
        if (RUN_PROBLEM_MODE) {
            cout << "PROBLEMS YOU SHOULD SEE:" << endl;
            cout << "  x HIGH Disk Queue Length (contention)" << endl;
            cout << "  x LOW Throughput (tiny operations)" << endl;
            cout << "  x TINY Avg Bytes/Transfer" << endl;
            cout << "  x HIGH Thread Count (" << DISK_THRASHING_THREADS << ")" << endl;
        } else {
            cout << "OPTIMIZATIONS IN ACTION:" << endl;
            cout << "  + LOW Disk Queue Length (efficient)" << endl;
            cout << "  + HIGH Throughput (large operations)" << endl;
            cout << "  + LARGE Avg Bytes/Transfer" << endl;
            cout << "  + REASONABLE Thread Count (" << EFFICIENT_THREADS << ")" << endl;
        }
        cout << endl;
        
        cout << "Press Ctrl+C to stop..." << endl;
        
        lastOps = currentOps;
        lastBytes = currentBytes;
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
// MAIN
// =====================================================================================

int main() {
    SetConsoleCtrlHandler(ConsoleHandler, TRUE);
    
    cout << "=======================================================" << endl;
    cout << "  THREAD CONTENTION VS OPTIMIZED DISK I/O" << endl;
    cout << "  COMBINED DEMONSTRATION (C++)" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    if (RUN_PROBLEM_MODE) {
        cout << "MODE: PROBLEM (Thread Contention & Tiny I/O)" << endl;
        cout << endl;
        cout << "Configuration:" << endl;
        cout << "x " << DISK_THRASHING_THREADS << " threads (causing contention)" << endl;
        cout << "x " << TINY_WRITE_SIZE << " byte writes (tiny)" << endl;
        cout << "x " << TINY_READ_SIZE << " byte reads (tiny)" << endl;
        cout << "x " << RANDOM_FILES_COUNT << " random files" << endl;
        cout << "x " << SEEK_OPERATIONS_PER_CYCLE << " seeks per cycle" << endl;
        cout << "x Global lock (massive contention)" << endl;
        cout << "x Random access patterns" << endl;
    } else {
        cout << "MODE: OPTIMIZED (Efficient Disk I/O)" << endl;
        cout << endl;
        cout << "Configuration:" << endl;
        cout << "+ " << EFFICIENT_THREADS << " threads (efficient)" << endl;
        cout << "+ " << (LARGE_BUFFER_SIZE / 1024) << " KB writes (large)" << endl;
        cout << "+ " << (MEDIUM_BUFFER_SIZE / 1024) << " KB reads (large)" << endl;
        cout << "+ Per-file locking (reduced contention)" << endl;
        cout << "+ Sequential access patterns" << endl;
        cout << "+ Batched operations" << endl;
    }
    cout << endl;
    
    cout << "Press any key to start..." << endl;
    cin.get();
    
    cout << endl;
    
    // Setup
    if (RUN_PROBLEM_MODE) {
        SetupProblemMode();
    } else {
        SetupOptimizedMode();
    }
    
    cout << endl;
    cout << "Starting demonstration..." << endl;
    cout << endl;
    
    // Start monitoring
    thread monitorThread(MonitorPerformance);
    
    // Start worker threads
    vector<thread> threads;
    
    if (RUN_PROBLEM_MODE) {
        for (int i = 0; i < DISK_THRASHING_THREADS; i++) {
            threads.push_back(thread(ProblemWorkerThread, i));
        }
    } else {
        for (int i = 0; i < EFFICIENT_THREADS; i++) {
            threads.push_back(thread(OptimizedWorkerThread, i));
        }
    }
    
    // Wait for threads
    for (auto& t : threads) {
        if (t.joinable()) t.join();
    }
    
    g_running = false;
    if (monitorThread.joinable()) monitorThread.join();
    
    // Final statistics
    cout << endl;
    cout << "=======================================================" << endl;
    cout << "              FINAL STATISTICS" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "Mode: " << (RUN_PROBLEM_MODE ? "PROBLEM" : "OPTIMIZED") << endl;
    cout << endl;
    
    cout << "Performance:" << endl;
    cout << "  Total Operations: " << g_totalOps.load() << endl;
    cout << "  Total Data:       " << (g_totalBytes / 1024 / 1024) << " MB" << endl;
    
    if (g_totalOps > 0) {
        double avgBytes = g_totalBytes.load() / (double)g_totalOps.load();
        cout << "  Avg Bytes/Op:     " << fixed << setprecision(1);
        
        if (avgBytes < 1024) {
            cout << avgBytes << " bytes" << endl;
        } else if (avgBytes < 1048576) {
            cout << (avgBytes / 1024) << " KB" << endl;
        } else {
            cout << (avgBytes / 1048576) << " MB" << endl;
        }
    }
    cout << endl;
    
    if (RUN_PROBLEM_MODE) {
        cout << "PROBLEMS DEMONSTRATED:" << endl;
        cout << "x Tiny read/write operations" << endl;
        cout << "x Thread contention on global lock" << endl;
        cout << "x Random access patterns" << endl;
        cout << "x Excessive disk queue length" << endl;
        cout << "x Low throughput" << endl;
    } else {
        cout << "OPTIMIZATIONS DEMONSTRATED:" << endl;
        cout << "+ Large buffer I/O operations" << endl;
        cout << "+ Per-file locks (reduced contention)" << endl;
        cout << "+ Sequential access patterns" << endl;
        cout << "+ Batched operations" << endl;
        cout << "+ High throughput" << endl;
    }
    cout << endl;
    
    cout << "Cleaning up test files..." << endl;
    try {
        if (RUN_PROBLEM_MODE) {
            remove_all(PROBLEM_BASE_DIR);
        } else {
            remove_all(OPTIMIZED_BASE_DIR);
        }
    } catch (...) {
        cout << "Note: You may need to manually delete test directories" << endl;
    }
    
    cout << endl;
    cout << "To toggle modes, change RUN_PROBLEM_MODE in the source code and recompile." << endl;
    
    return 0;
}

