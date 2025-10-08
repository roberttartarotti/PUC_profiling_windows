/*
 * Network Issues Demonstration for PUC Profiling Course
 * Module 3, Class 3, Example 1 - C++ Version
 * 
 * This example demonstrates various network performance issues:
 * - Network Latency (artificial delays)
 * - Throughput Problems (packet loss simulation)
 * - Network Jitter (timing variations)
 * - TCP Retransmission Issues
 * - Connection Failures
 * - Network Interface Queue Length simulation
 * 
 * Use Windows PerfMon to monitor:
 * - Network Interface: Bytes Total/sec, Packets/sec, Current Bandwidth
 * - TCPv4: Segments Retransmitted/sec, Connection Failures
 * - Network Interface: Queue Length, Output Queue Length
 * 
 * Compile with: cl /EHsc m3p3e1-network-issues-demo.cpp /link ws2_32.lib
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
#include <string>
#include <sstream>
#include <iomanip>

#pragma comment(lib, "ws2_32.lib")

using namespace std;
using namespace std::chrono;

// ===== CONFIGURATION =====
// Set to false to run the PROBLEM version (with artificial network issues)
// Set to true to run the SOLVED version (optimized network handling)
const bool USE_SOLVED_VERSION = false;

// Network configuration
const int SERVER_PORT = 8888;
const int BUFFER_SIZE = 64 * 1024;  // 64KB buffer
const int CONCURRENT_CONNECTIONS = USE_SOLVED_VERSION ? 5 : 50;
const int DATA_PER_SEND = 1024 * 1024;  // 1MB per send

// Problem simulation parameters
const int ARTIFICIAL_DELAY_MS = 100;
const double DROP_RATE = 0.30;  // 30% connection drop
const int SOCKET_SEND_BUFFER = 8192;
const int SOCKET_RECEIVE_BUFFER = 8192;

// Statistics
atomic<long long> TotalBytesSent(0);
atomic<long long> TotalBytesReceived(0);
atomic<int> ActiveConnections(0);
atomic<int> ConnectionFailures(0);
atomic<int> ForcedDisconnects(0);

bool g_running = true;

string GetLocalIPAddress() {
    char hostname[256];
    gethostname(hostname, sizeof(hostname));
    
    struct addrinfo hints = {}, *result;
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    
    if (getaddrinfo(hostname, nullptr, &hints, &result) == 0) {
        char ip[INET_ADDRSTRLEN];
        struct sockaddr_in* addr = (struct sockaddr_in*)result->ai_addr;
        inet_ntop(AF_INET, &addr->sin_addr, ip, INET_ADDRSTRLEN);
        freeaddrinfo(result);
        return string(ip);
    }
    
    return "127.0.0.1";
}

void HandleClient(SOCKET clientSocket) {
    random_device rd;
    mt19937 gen(rd());
    uniform_real_distribution<> dis(0.0, 1.0);
    
    char buffer[BUFFER_SIZE];
    int bytesRead;
    
    while (g_running && (bytesRead = recv(clientSocket, buffer, BUFFER_SIZE, 0)) > 0) {
        TotalBytesReceived += bytesRead;
        
        // Problem version: Add delays and random disconnects
        if (!USE_SOLVED_VERSION) {
            if (dis(gen) < 0.3) {
                this_thread::sleep_for(milliseconds(ARTIFICIAL_DELAY_MS));
            }
            
            if (dis(gen) < DROP_RATE) {
                ForcedDisconnects++;
                break;
            }
        }
        
        // Echo back
        if (send(clientSocket, buffer, bytesRead, 0) > 0) {
            TotalBytesSent += bytesRead;
        }
    }
    
    closesocket(clientSocket);
    ActiveConnections--;
}

void StartServer() {
    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (serverSocket == INVALID_SOCKET) {
        cerr << "Failed to create server socket" << endl;
        return;
    }
    
    if (!USE_SOLVED_VERSION) {
        int sendBuf = SOCKET_SEND_BUFFER;
        int recvBuf = SOCKET_RECEIVE_BUFFER;
        setsockopt(serverSocket, SOL_SOCKET, SO_SNDBUF, (char*)&sendBuf, sizeof(sendBuf));
        setsockopt(serverSocket, SOL_SOCKET, SO_RCVBUF, (char*)&recvBuf, sizeof(recvBuf));
    }
    
    sockaddr_in serverAddr = {};
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    serverAddr.sin_addr.s_addr = INADDR_ANY;
    
    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        cerr << "Bind failed: " << WSAGetLastError() << endl;
        closesocket(serverSocket);
        return;
    }
    
    if (listen(serverSocket, 100) == SOCKET_ERROR) {
        cerr << "Listen failed" << endl;
        closesocket(serverSocket);
        return;
    }
    
    cout << "Server started on port " << SERVER_PORT << endl;
    
    vector<thread> clientThreads;
    
    while (g_running) {
        SOCKET clientSocket = accept(serverSocket, nullptr, nullptr);
        if (clientSocket != INVALID_SOCKET) {
            ActiveConnections++;
            clientThreads.push_back(thread(HandleClient, clientSocket));
        }
    }
    
    // Cleanup
    for (auto& t : clientThreads) {
        if (t.joinable()) t.join();
    }
    closesocket(serverSocket);
}

void StartClient(int clientId) {
    random_device rd;
    mt19937 gen(rd() + clientId);
    uniform_int_distribution<> delayDis(100, 300);
    uniform_real_distribution<> dropDis(0.0, 1.0);
    
    while (g_running) {
        SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
        if (clientSocket == INVALID_SOCKET) {
            ConnectionFailures++;
            this_thread::sleep_for(milliseconds(USE_SOLVED_VERSION ? 1000 : 100));
            continue;
        }
        
        if (!USE_SOLVED_VERSION) {
            int sendBuf = SOCKET_SEND_BUFFER;
            int recvBuf = SOCKET_RECEIVE_BUFFER;
            setsockopt(clientSocket, SOL_SOCKET, SO_SNDBUF, (char*)&sendBuf, sizeof(sendBuf));
            setsockopt(clientSocket, SOL_SOCKET, SO_RCVBUF, (char*)&recvBuf, sizeof(recvBuf));
        }
        
        sockaddr_in serverAddr = {};
        serverAddr.sin_family = AF_INET;
        serverAddr.sin_port = htons(SERVER_PORT);
        inet_pton(AF_INET, "127.0.0.1", &serverAddr.sin_addr);
        
        if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
            ConnectionFailures++;
            closesocket(clientSocket);
            this_thread::sleep_for(milliseconds(USE_SOLVED_VERSION ? 1000 : 100));
            continue;
        }
        
        // Send data
        vector<char> sendBuffer(DATA_PER_SEND);
        for (int offset = 0; offset < DATA_PER_SEND && g_running; offset += BUFFER_SIZE) {
            int toSend = min(BUFFER_SIZE, DATA_PER_SEND - offset);
            
            int sent = send(clientSocket, sendBuffer.data() + offset, toSend, 0);
            if (sent > 0) {
                TotalBytesSent += sent;
                
                // Receive echo
                char recvBuffer[BUFFER_SIZE];
                int received = recv(clientSocket, recvBuffer, BUFFER_SIZE, 0);
                if (received > 0) {
                    TotalBytesReceived += received;
                }
            }
            
            if (USE_SOLVED_VERSION) {
                this_thread::sleep_for(milliseconds(10));
            }
        }
        
        closesocket(clientSocket);
        this_thread::sleep_for(milliseconds(USE_SOLVED_VERSION ? 1000 : delayDis(gen)));
    }
}

void MonitorPerformance() {
    long long lastBytesSent = 0;
    long long lastBytesReceived = 0;
    auto lastTime = steady_clock::now();
    
    while (g_running) {
        this_thread::sleep_for(seconds(2));
        
        auto currentTime = steady_clock::now();
        auto elapsed = duration_cast<seconds>(currentTime - lastTime).count();
        if (elapsed == 0) elapsed = 1;
        
        long long currentSent = TotalBytesSent;
        long long currentReceived = TotalBytesReceived;
        
        double sentPerSec = (currentSent - lastBytesSent) / (double)elapsed;
        double receivedPerSec = (currentReceived - lastBytesReceived) / (double)elapsed;
        
        system("cls");
        cout << "=== Real-Time Network Metrics ===" << endl;
        cout << "Mode: " << (USE_SOLVED_VERSION ? "SOLVED" : "PROBLEM") << endl;
        cout << endl;
        
        cout << "Network Throughput:" << endl;
        cout << "  Bytes Sent/sec:     " << fixed << setprecision(2) 
             << (sentPerSec / 1024 / 1024) << " MB/s" << endl;
        cout << "  Bytes Received/sec: " << fixed << setprecision(2) 
             << (receivedPerSec / 1024 / 1024) << " MB/s" << endl;
        cout << endl;
        
        cout << "Connection Stats:" << endl;
        cout << "  Active Connections:  " << ActiveConnections.load() << endl;
        cout << "  Connection Failures: " << ConnectionFailures.load() << endl;
        cout << "  Forced Disconnects:  " << ForcedDisconnects.load() << endl;
        cout << endl;
        
        cout << "Cumulative Traffic:" << endl;
        cout << "  Total Sent:     " << (TotalBytesSent / 1024 / 1024) << " MB" << endl;
        cout << "  Total Received: " << (TotalBytesReceived / 1024 / 1024) << " MB" << endl;
        cout << endl;
        
        cout << "Monitor Windows PerfMon for network metrics!" << endl;
        cout << "Press Ctrl+C to stop..." << endl;
        
        lastBytesSent = currentSent;
        lastBytesReceived = currentReceived;
        lastTime = currentTime;
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
    // Initialize Winsock
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        cerr << "WSAStartup failed" << endl;
        return 1;
    }
    
    SetConsoleCtrlHandler(ConsoleHandler, TRUE);
    
    cout << "=== Network Issues Demonstration ===" << endl;
    cout << "Mode: " << (USE_SOLVED_VERSION ? "SOLVED VERSION" : "PROBLEM VERSION") << endl;
    cout << "Running continuously - Press Ctrl+C to stop" << endl;
    cout << endl;
    
    if (!USE_SOLVED_VERSION) {
        cout << "PROBLEM VERSION - Demonstrating severe network performance issues:" << endl;
        cout << "- " << CONCURRENT_CONNECTIONS << " concurrent connections (excessive)" << endl;
        cout << "- " << SOCKET_SEND_BUFFER << " byte send buffer (too small)" << endl;
        cout << "- " << ARTIFICIAL_DELAY_MS << "ms artificial delays" << endl;
        cout << "- " << (int)(DROP_RATE * 100) << "% connection abort rate" << endl;
    } else {
        cout << "SOLVED VERSION - Optimized network handling:" << endl;
        cout << "- " << CONCURRENT_CONNECTIONS << " connections (reasonable)" << endl;
        cout << "- Proper buffer sizes" << endl;
        cout << "- No artificial delays" << endl;
        cout << "- Minimal connection aborts" << endl;
    }
    cout << endl;
    
    cout << "Starting server and clients..." << endl;
    cout << endl;
    
    // Start server
    thread serverThread(StartServer);
    this_thread::sleep_for(milliseconds(500));
    
    // Start clients
    vector<thread> clientThreads;
    for (int i = 0; i < CONCURRENT_CONNECTIONS; i++) {
        clientThreads.push_back(thread(StartClient, i));
        this_thread::sleep_for(milliseconds(100));
    }
    
    // Start monitoring
    thread monitorThread(MonitorPerformance);
    
    // Wait for threads
    if (serverThread.joinable()) serverThread.join();
    for (auto& t : clientThreads) {
        if (t.joinable()) t.join();
    }
    if (monitorThread.joinable()) monitorThread.join();
    
    // Cleanup
    WSACleanup();
    
    cout << endl;
    cout << "=== FINAL STATISTICS ===" << endl;
    cout << "Total Data Sent:        " << (TotalBytesSent / 1024 / 1024) << " MB" << endl;
    cout << "Total Data Received:    " << (TotalBytesReceived / 1024 / 1024) << " MB" << endl;
    cout << "Connection Failures:    " << ConnectionFailures.load() << endl;
    cout << "Forced Disconnects:     " << ForcedDisconnects.load() << endl;
    cout << endl;
    
    return 0;
}

