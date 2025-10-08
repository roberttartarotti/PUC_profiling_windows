/*
 * HTTP and TCP/IP Intensive PROBLEMS Demonstration
 * Module 3, Class 3, Example 2 - PROBLEM VERSION (C++)
 * 
 * This demonstrates SEVERE network issues with HTTP and TCP/IP:
 * - TCP port exhaustion (ephemeral port starvation)
 * - Socket leaks and resource exhaustion
 * - Synchronous blocking I/O causing thread starvation
 * - TCP timeout issues and cascading failures
 * - TCP backlog overflow
 * - Connection thrashing
 * 
 * CRITICAL: This code is intentionally BAD to demonstrate problems!
 * 
 * Compile with: cl /EHsc /std:c++17 m3p3e2-http-tcpip-intensive-problems.cpp /link ws2_32.lib
 */

#include <iostream>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <thread>
#include <vector>
#include <atomic>
#include <chrono>
#include <random>
#include <memory>
#include <mutex>

#pragma comment(lib, "ws2_32.lib")

using namespace std;
using namespace std::chrono;

// PROBLEM CONFIGURATION - All values intentionally bad!
const int HTTP_SERVER_PORT = 9000;
const int TCP_SERVER_PORT = 9001;
const int HTTP_RESPONSE_SIZE = 1024 * 1024;  // 1MB
const int TCP_DATA_SIZE = 512 * 1024;        // 512KB
const int BUFFER_SIZE = 8192;

// Problem parameters
const int TCP_CLIENTS_COUNT = 500;           // Massive (port exhaustion!)
const int TCP_BACKLOG = 5;                   // Tiny backlog
const int SOCKET_TIMEOUT_MS = 500;           // Too aggressive
const bool LEAK_CONNECTIONS = true;          // Don't close properly
const bool USE_SYNCHRONOUS_IO = true;        // Blocking I/O

// Statistics
atomic<int> TcpConnectionsOpened(0);
atomic<int> TcpConnectionsFailed(0);
atomic<int> SocketsLeaked(0);
atomic<int> ThreadsCreated(0);
atomic<long long> TotalBytesTransferred(0);

bool g_running = true;
vector<SOCKET> g_leakedSockets;  // Intentional leak for demonstration
mutex g_leakMutex;

void HandleTcpClientSync(SOCKET clientSocket) {
    ThreadsCreated++;
    
    try {
        // PROBLEM: Very aggressive timeout
        int timeout = SOCKET_TIMEOUT_MS;
        setsockopt(clientSocket, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout));
        setsockopt(clientSocket, SOL_SOCKET, SO_SNDTIMEO, (char*)&timeout, sizeof(timeout));
        
        char buffer[BUFFER_SIZE];
        
        // PROBLEM: Synchronous blocking read
        int bytesRead = recv(clientSocket, buffer, BUFFER_SIZE, 0);
        
        if (bytesRead > 0) {
            // PROBLEM: Synchronous blocking write
            send(clientSocket, buffer, bytesRead, 0);
            TotalBytesTransferred += bytesRead * 2;
        }
        
        // PROBLEM: Sometimes "forget" to close (leak simulation)
        random_device rd;
        mt19937 gen(rd());
        uniform_int_distribution<> dis(0, 99);
        
        if (!LEAK_CONNECTIONS || dis(gen) < 80) {
            closesocket(clientSocket);
        } else {
            // LEAK: Intentionally not closing
            lock_guard<mutex> lock(g_leakMutex);
            g_leakedSockets.push_back(clientSocket);
            SocketsLeaked++;
        }
    } catch (...) {
        TcpConnectionsFailed++;
    }
}

void StartProblematicTcpServer() {
    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (serverSocket == INVALID_SOCKET) {
        cerr << "Failed to create TCP server socket" << endl;
        return;
    }
    
    sockaddr_in serverAddr = {};
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(TCP_SERVER_PORT);
    serverAddr.sin_addr.s_addr = INADDR_ANY;
    
    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        cerr << "TCP bind failed: " << WSAGetLastError() << endl;
        closesocket(serverSocket);
        return;
    }
    
    // PROBLEM: Tiny backlog causes connection refusal
    if (listen(serverSocket, TCP_BACKLOG) == SOCKET_ERROR) {
        cerr << "TCP listen failed" << endl;
        closesocket(serverSocket);
        return;
    }
    
    cout << "TCP Server started on port " << TCP_SERVER_PORT 
         << " (backlog: " << TCP_BACKLOG << ")" << endl;
    
    vector<thread> clientThreads;
    
    while (g_running) {
        SOCKET clientSocket = accept(serverSocket, nullptr, nullptr);
        if (clientSocket != INVALID_SOCKET) {
            // PROBLEM: Synchronous handling blocks threads
            if (USE_SYNCHRONOUS_IO) {
                clientThreads.push_back(thread(HandleTcpClientSync, clientSocket));
            }
        }
    }
    
    // Cleanup
    for (auto& t : clientThreads) {
        if (t.joinable()) t.join();
    }
    closesocket(serverSocket);
}

void StartProblematicTcpClient(int clientId) {
    random_device rd;
    mt19937 gen(rd() + clientId);
    uniform_int_distribution<> delayDis(100, 200);
    
    while (g_running) {
        try {
            TcpConnectionsOpened++;
            
            // PROBLEM: Creating lots of sockets (port exhaustion)
            SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
            if (clientSocket == INVALID_SOCKET) {
                TcpConnectionsFailed++;
                this_thread::sleep_for(milliseconds(100));
                continue;
            }
            
            // PROBLEM: Aggressive timeout
            int timeout = SOCKET_TIMEOUT_MS;
            setsockopt(clientSocket, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout));
            setsockopt(clientSocket, SOL_SOCKET, SO_SNDTIMEO, (char*)&timeout, sizeof(timeout));
            
            sockaddr_in serverAddr = {};
            serverAddr.sin_family = AF_INET;
            serverAddr.sin_port = htons(TCP_SERVER_PORT);
            inet_pton(AF_INET, "127.0.0.1", &serverAddr.sin_addr);
            
            if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
                TcpConnectionsFailed++;
                closesocket(clientSocket);
                this_thread::sleep_for(milliseconds(100));
                continue;
            }
            
            // Send data
            vector<char> data(TCP_DATA_SIZE);
            
            // PROBLEM: Synchronous send
            if (USE_SYNCHRONOUS_IO) {
                send(clientSocket, data.data(), TCP_DATA_SIZE, 0);
                
                char receiveBuffer[BUFFER_SIZE];
                recv(clientSocket, receiveBuffer, BUFFER_SIZE, 0);
            }
            
            // PROBLEM: Intentional socket leak (20% of the time)
            if (LEAK_CONNECTIONS && (gen() % 100) < 20) {
                lock_guard<mutex> lock(g_leakMutex);
                g_leakedSockets.push_back(clientSocket);
                SocketsLeaked++;
            } else {
                closesocket(clientSocket);
            }
            
        } catch (...) {
            TcpConnectionsFailed++;
        }
        
        // PROBLEM: Very short delay causes connection thrashing
        this_thread::sleep_for(milliseconds(100));
    }
}

void MonitorSystemResources() {
    auto startTime = steady_clock::now();
    
    while (g_running) {
        this_thread::sleep_for(seconds(2));
        
        auto runtime = duration_cast<seconds>(steady_clock::now() - startTime).count();
        
        system("cls");
        cout << "=======================================================" << endl;
        cout << "  HTTP/TCP Intensive PROBLEMS - Real-Time Stats" << endl;
        cout << "=======================================================" << endl;
        cout << endl;
        
        cout << "TCP Statistics:" << endl;
        cout << "  Connections Opened: " << TcpConnectionsOpened.load() << endl;
        cout << "  Connections Failed: " << TcpConnectionsFailed.load() << endl;
        cout << "  Sockets Leaked:     " << SocketsLeaked.load() << endl;
        
        if (TcpConnectionsOpened > 0) {
            double failureRate = (TcpConnectionsFailed.load() * 100.0) / TcpConnectionsOpened.load();
            cout << "  Failure Rate:       " << fixed << setprecision(1) 
                 << failureRate << "%" << endl;
        }
        cout << endl;
        
        cout << "System Resources:" << endl;
        cout << "  Threads Created:      " << ThreadsCreated.load() << endl;
        cout << "  Total Transferred:    " << (TotalBytesTransferred / 1024 / 1024) << " MB" << endl;
        cout << endl;
        
        cout << "PROBLEMS YOU SHOULD SEE:" << endl;
        cout << "  x High TCP connection failures (port exhaustion)" << endl;
        cout << "  x Growing socket leaks: " << SocketsLeaked.load() << endl;
        cout << "  x High thread count (thread starvation)" << endl;
        cout << "  x Increasing memory usage" << endl;
        cout << endl;
        
        cout << "Check Windows PerfMon for:" << endl;
        cout << "  - TCPv4 -> Connections Established (high!)" << endl;
        cout << "  - TCPv4 -> Connection Failures (increasing!)" << endl;
        cout << "  - Process -> Handle Count (growing!)" << endl;
        cout << endl;
        
        cout << "Press Ctrl+C to stop..." << endl;
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
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        cerr << "WSAStartup failed" << endl;
        return 1;
    }
    
    SetConsoleCtrlHandler(ConsoleHandler, TRUE);
    
    cout << "=======================================================" << endl;
    cout << "  HTTP and TCP/IP Intensive PROBLEMS Demonstration" << endl;
    cout << "  WARNING: This code is intentionally BAD!" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "PROBLEM VERSION - Intentional Bad Practices:" << endl;
    cout << "x " << TCP_CLIENTS_COUNT << " concurrent TCP connections (port exhaustion)" << endl;
    cout << "x Backlog of " << TCP_BACKLOG << " (causes connection refusal)" << endl;
    cout << "x " << SOCKET_TIMEOUT_MS << "ms timeout (too aggressive)" << endl;
    cout << "x Synchronous blocking I/O (thread starvation)" << endl;
    cout << "x Socket leaks (not closing properly)" << endl;
    cout << endl;
    
    cout << "Expected Problems:" << endl;
    cout << "- Port exhaustion from too many connections" << endl;
    cout << "- High connection failure rate" << endl;
    cout << "- Socket leaks visible in handle count" << endl;
    cout << "- Thread starvation from blocking I/O" << endl;
    cout << endl;
    
    cout << "Press any key to start problematic demonstration..." << endl;
    cin.get();
    
    // Start TCP server
    thread tcpServerThread(StartProblematicTcpServer);
    this_thread::sleep_for(milliseconds(1000));
    
    // Start monitoring
    thread monitorThread(MonitorSystemResources);
    
    // Start TCP clients (port exhaustion)
    vector<thread> tcpClientThreads;
    for (int i = 0; i < TCP_CLIENTS_COUNT; i++) {
        tcpClientThreads.push_back(thread(StartProblematicTcpClient, i));
        this_thread::sleep_for(milliseconds(10));
    }
    
    cout << "Started " << TCP_CLIENTS_COUNT << " TCP clients" << endl;
    cout << "Generating intensive problematic network traffic..." << endl;
    cout << endl;
    
    // Wait for threads
    if (tcpServerThread.joinable()) tcpServerThread.join();
    for (auto& t : tcpClientThreads) {
        if (t.joinable()) t.join();
    }
    if (monitorThread.joinable()) monitorThread.join();
    
    // Display final statistics
    cout << endl;
    cout << "=======================================================" << endl;
    cout << "           FINAL STATISTICS - PROBLEM VERSION" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "TCP Issues:" << endl;
    cout << "  Connections Opened: " << TcpConnectionsOpened.load() << endl;
    cout << "  Connection Failures: " << TcpConnectionsFailed.load() << endl;
    cout << "  Sockets Leaked:     " << SocketsLeaked.load() << endl;
    cout << endl;
    
    cout << "Resource Leaks:" << endl;
    cout << "  Leaked Sockets:     " << g_leakedSockets.size() << endl;
    cout << endl;
    
    cout << "PROBLEMS DEMONSTRATED:" << endl;
    cout << "x Socket exhaustion from creating too many connections" << endl;
    cout << "x Port exhaustion from excessive concurrent connections" << endl;
    cout << "x Thread starvation from synchronous I/O" << endl;
    cout << "x Resource leaks from not closing sockets" << endl;
    cout << "x TCP backlog overflow from tiny server queue" << endl;
    cout << endl;
    
    // Cleanup leaked sockets
    for (SOCKET s : g_leakedSockets) {
        closesocket(s);
    }
    
    WSACleanup();
    return 0;
}

