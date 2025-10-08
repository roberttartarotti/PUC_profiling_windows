/*
 * Thread Contention and Disk I/O PROBLEM Demonstration (C++)
 * Module 3, Class 2, Example 1 - PROBLEM VERSION
 * 
 * This demonstrates SEVERE disk I/O and thread contention issues:
 * - High disk queue length from many competing threads
 * - Low bytes/sec from tiny read/write operations
 * - Thread contention and lock waits
 * - Disk thrashing from random access patterns
 * - File fragmentation issues
 * 
 * Monitor in Windows PerfMon:
 * - PhysicalDisk: Avg. Disk Queue Length (will be very high)
 * - PhysicalDisk: Disk Bytes/sec (will be very low despite activity)
 * - PhysicalDisk: Avg. Disk Bytes/Transfer (will be tiny)
 * - Process: Thread Count (will be high)
 * 
 * Compile with: cl /EHsc /std:c++17 example1-m3p2e1-thread-contention-disk-io.cpp
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
#include <windows.h>

using namespace std;
using namespace std::chrono;
using namespace std::filesystem;

// PROBLEM CONFIGURATION - Creates BAD disk metrics
const int DISK_THRASHING_THREADS = 32;        // Many threads competing
const int OPERATIONS_PER_THREAD = 500;
const int TINY_WRITE_SIZE = 64;                // Very small writes (low avg bytes/transfer)
const int TINY_READ_SIZE = 32;                 // Very small reads
const int RANDOM_FILES_COUNT = 200;            // Many files for random access
const int SEEK_OPERATIONS_PER_CYCLE = 50;
const string BASE_DIRECTORY = "disk_problem_test/";
const string FRAGMENT_FILE_PREFIX = "fragment_";
const string RANDOM_FILE_PREFIX = "random_";

// Statistics
atomic<long long> TotalBytesWritten(0);
atomic<long long> TotalBytesRead(0);
atomic<long long> TotalOperations(0);
atomic<int> ActiveThreads(0);
atomic<int> ContentionEvents(0);

mutex g_fileMutex;  // PROBLEM: Single mutex causing contention!
bool g_running = true;

void CreateTestDirectory() {
    create_directories(BASE_DIRECTORY);
    
    // Create many random files for thrashing
    for (int i = 0; i < RANDOM_FILES_COUNT; i++) {
        string filename = BASE_DIRECTORY + RANDOM_FILE_PREFIX + to_string(i) + ".dat";
        ofstream file(filename, ios::binary);
        
        // Write some initial data
        char buffer[1024] = {};
        file.write(buffer, sizeof(buffer));
        file.close();
    }
}

// PROBLEM: Tiny writes causing low Avg Bytes/Transfer
void PerformTinyWrites(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, RANDOM_FILES_COUNT - 1);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + RANDOM_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Lock contention on every operation
            {
                lock_guard<mutex> lock(g_fileMutex);
                ContentionEvents++;
                
                // PROBLEM: Tiny write (64 bytes) - causes low Avg Bytes/Transfer
                ofstream file(filename, ios::binary | ios::app);
                char buffer[TINY_WRITE_SIZE];
                memset(buffer, threadId % 256, TINY_WRITE_SIZE);
                
                file.write(buffer, TINY_WRITE_SIZE);
                file.flush();  // Force to disk
                file.close();
                
                TotalBytesWritten += TINY_WRITE_SIZE;
                TotalOperations++;
            }
            
            // PROBLEM: Minimal delay causes thread thrashing
            this_thread::sleep_for(microseconds(100));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// PROBLEM: Tiny reads causing low Avg Bytes/Transfer
void PerformTinyReads(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, RANDOM_FILES_COUNT - 1);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + RANDOM_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Lock contention
            {
                lock_guard<mutex> lock(g_fileMutex);
                ContentionEvents++;
                
                // PROBLEM: Tiny read (32 bytes) - causes low Avg Bytes/Transfer
                ifstream file(filename, ios::binary);
                if (file) {
                    char buffer[TINY_READ_SIZE];
                    file.read(buffer, TINY_READ_SIZE);
                    
                    TotalBytesRead += file.gcount();
                    TotalOperations++;
                }
                file.close();
            }
            
            this_thread::sleep_for(microseconds(100));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// PROBLEM: Random seeks causing disk thrashing
void PerformRandomSeeks(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, RANDOM_FILES_COUNT - 1);
    uniform_int_distribution<> seekDis(0, 900);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + RANDOM_FILE_PREFIX + to_string(fileDis(gen)) + ".dat";
            
            // PROBLEM: Lock contention + random seeking
            {
                lock_guard<mutex> lock(g_fileMutex);
                ContentionEvents++;
                
                fstream file(filename, ios::binary | ios::in | ios::out);
                if (file) {
                    // PROBLEM: Random seek to different position
                    file.seekg(seekDis(gen), ios::beg);
                    
                    char buffer[TINY_READ_SIZE];
                    file.read(buffer, TINY_READ_SIZE);
                    
                    // Write at different position
                    file.seekp(seekDis(gen), ios::beg);
                    file.write(buffer, TINY_READ_SIZE);
                    file.flush();
                    
                    TotalOperations++;
                }
                file.close();
            }
            
            this_thread::sleep_for(microseconds(100));
            
        } catch (...) {
            // Ignore errors
        }
    }
}

// PROBLEM: File fragmentation through small writes
void PerformFragmentedWrites(int threadId) {
    random_device rd;
    mt19937 gen(rd() + threadId);
    uniform_int_distribution<> fileDis(0, RANDOM_FILES_COUNT - 1);
    
    for (int i = 0; i < OPERATIONS_PER_THREAD && g_running; i++) {
        try {
            string filename = BASE_DIRECTORY + FRAGMENT_FILE_PREFIX + to_string(threadId) + ".dat";
            
            // PROBLEM: Lock contention
            {
                lock_guard<mutex> lock(g_fileMutex);
                ContentionEvents++;
                
                // PROBLEM: Many small writes causing fragmentation
                ofstream file(filename, ios::binary | ios::app);
                char buffer[128];  // Small fragment
                memset(buffer, i % 256, sizeof(buffer));
                
                file.write(buffer, sizeof(buffer));
                file.flush();
                file.close();
                
                TotalBytesWritten += sizeof(buffer);
                TotalOperations++;
            }
            
            this_thread::sleep_for(microseconds(100));
            
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
        this_thread::sleep_for(seconds(1));
        
        auto currentTime = steady_clock::now();
        auto runtime = duration_cast<seconds>(currentTime - startTime).count();
        if (runtime == 0) runtime = 1;
        
        long long currentWritten = TotalBytesWritten.load();
        long long currentRead = TotalBytesRead.load();
        
        double writtenPerSec = (currentWritten - lastBytesWritten);
        double readPerSec = (currentRead - lastBytesRead);
        
        system("cls");
        cout << "=======================================================" << endl;
        cout << "  Thread Contention & Disk I/O PROBLEMS - Real-Time" << endl;
        cout << "=======================================================" << endl;
        cout << endl;
        
        cout << "Runtime: " << (runtime / 60) << "m " << (runtime % 60) << "s" << endl;
        cout << endl;
        
        cout << "Disk I/O Statistics:" << endl;
        cout << "  Bytes Written/sec:  " << fixed << setprecision(2)
             << (writtenPerSec / 1024) << " KB/s" << endl;
        cout << "  Bytes Read/sec:     " << fixed << setprecision(2)
             << (readPerSec / 1024) << " KB/s" << endl;
        cout << "  Total Operations:   " << TotalOperations.load() << endl;
        
        if (TotalOperations > 0) {
            double avgBytesPerOp = (currentWritten + currentRead) / (double)TotalOperations.load();
            cout << "  Avg Bytes/Transfer: " << fixed << setprecision(1)
                 << avgBytesPerOp << " bytes (TINY!)" << endl;
        }
        cout << endl;
        
        cout << "Thread Statistics:" << endl;
        cout << "  Active Threads:     " << ActiveThreads.load() << endl;
        cout << "  Contention Events:  " << ContentionEvents.load() << endl;
        cout << endl;
        
        cout << "Cumulative:" << endl;
        cout << "  Total Written:  " << (currentWritten / 1024 / 1024) << " MB" << endl;
        cout << "  Total Read:     " << (currentRead / 1024 / 1024) << " MB" << endl;
        cout << endl;
        
        cout << "PROBLEMS YOU SHOULD SEE IN PERFMON:" << endl;
        cout << "  x HIGH Disk Queue Length (many threads waiting)" << endl;
        cout << "  x LOW Bytes/sec (despite high activity)" << endl;
        cout << "  x TINY Avg Bytes/Transfer (~" << fixed << setprecision(0)
             << ((TINY_WRITE_SIZE + TINY_READ_SIZE) / 2.0) << " bytes)" << endl;
        cout << "  x HIGH Thread Count" << endl;
        cout << "  x HIGH Contention Events: " << ContentionEvents.load() << endl;
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
    cout << "  Thread Contention and Disk I/O PROBLEMS Demo" << endl;
    cout << "  WARNING: This demonstrates BAD practices!" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "PROBLEM CONFIGURATION:" << endl;
    cout << "x " << DISK_THRASHING_THREADS << " competing threads (thread thrashing)" << endl;
    cout << "x " << TINY_WRITE_SIZE << " byte writes (causes low Avg Bytes/Transfer)" << endl;
    cout << "x " << TINY_READ_SIZE << " byte reads (causes low Avg Bytes/Transfer)" << endl;
    cout << "x " << RANDOM_FILES_COUNT << " random files (disk thrashing)" << endl;
    cout << "x Single global lock (massive contention)" << endl;
    cout << "x Random seeks (disk head movement)" << endl;
    cout << endl;
    
    cout << "Expected PerfMon Metrics:" << endl;
    cout << "- Avg. Disk Queue Length: 10-50+ (very high)" << endl;
    cout << "- Disk Bytes/sec: Low (despite activity)" << endl;
    cout << "- Avg. Disk Bytes/Transfer: <100 bytes (terrible)" << endl;
    cout << "- Thread Count: " << DISK_THRASHING_THREADS << "+" << endl;
    cout << endl;
    
    cout << "Press any key to start problematic demonstration..." << endl;
    cin.get();
    
    cout << "\nCreating test files..." << endl;
    CreateTestDirectory();
    cout << "Created " << RANDOM_FILES_COUNT << " test files" << endl;
    cout << endl;
    
    // Start monitoring
    thread monitorThread(MonitorPerformance);
    
    // Start many threads doing different problematic operations
    vector<thread> threads;
    
    // Tiny writes threads
    for (int i = 0; i < DISK_THRASHING_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformTinyWrites(i);
            ActiveThreads--;
        }));
    }
    
    // Tiny reads threads
    for (int i = 0; i < DISK_THRASHING_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformTinyReads(i + 100);
            ActiveThreads--;
        }));
    }
    
    // Random seeks threads
    for (int i = 0; i < DISK_THRASHING_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformRandomSeeks(i + 200);
            ActiveThreads--;
        }));
    }
    
    // Fragmented writes threads
    for (int i = 0; i < DISK_THRASHING_THREADS / 4; i++) {
        threads.push_back(thread([i]() {
            ActiveThreads++;
            PerformFragmentedWrites(i + 300);
            ActiveThreads--;
        }));
    }
    
    cout << "Started " << DISK_THRASHING_THREADS << " problem threads" << endl;
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
    cout << "           FINAL STATISTICS - PROBLEM VERSION" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "Disk I/O:" << endl;
    cout << "  Total Written:      " << (TotalBytesWritten / 1024 / 1024) << " MB" << endl;
    cout << "  Total Read:         " << (TotalBytesRead / 1024 / 1024) << " MB" << endl;
    cout << "  Total Operations:   " << TotalOperations.load() << endl;
    
    if (TotalOperations > 0) {
        double avgBytes = (TotalBytesWritten.load() + TotalBytesRead.load()) / (double)TotalOperations.load();
        cout << "  Avg Bytes/Transfer: " << fixed << setprecision(1) << avgBytes << " bytes" << endl;
    }
    cout << endl;
    
    cout << "Threading:" << endl;
    cout << "  Contention Events:  " << ContentionEvents.load() << endl;
    cout << endl;
    
    cout << "PROBLEMS DEMONSTRATED:" << endl;
    cout << "x Tiny reads/writes causing low Avg Bytes/Transfer" << endl;
    cout << "x Many threads causing high disk queue length" << endl;
    cout << "x Single global lock causing massive contention" << endl;
    cout << "x Random access patterns causing disk thrashing" << endl;
    cout << "x File fragmentation from small writes" << endl;
    cout << endl;
    
    cout << "Cleaning up test files..." << endl;
    try {
        remove_all(BASE_DIRECTORY);
    } catch (...) {
        cout << "Note: You may need to manually delete: " << BASE_DIRECTORY << endl;
    }
    
    return 0;
}

