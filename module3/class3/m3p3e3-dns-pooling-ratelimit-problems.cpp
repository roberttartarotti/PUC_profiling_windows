/*
 * DNS, Connection Pooling, and Rate Limiting PROBLEMS
 * Module 3, Class 3, Example 3 - PROBLEM VERSION (C++)
 * 
 * This demonstrates severe issues with:
 * - DNS resolution overhead (repeated lookups)
 * - Connection pool starvation
 * - No rate limiting (overwhelming servers)
 * - No retry logic or exponential backoff
 * - Socket resource exhaustion
 * - Blocking DNS lookups
 * 
 * CRITICAL: This code demonstrates BAD practices!
 * 
 * Compile with: cl /EHsc /std:c++17 m3p3e3-dns-pooling-ratelimit-problems.cpp /link ws2_32.lib
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
#include <mutex>

#pragma comment(lib, "ws2_32.lib")

using namespace std;
using namespace std::chrono;

// PROBLEM CONFIGURATION - All intentionally bad
const string TARGET_HOST = "httpbin.org";      // Public test API
const int TARGET_PORT = 80;
const int CONCURRENT_REQUESTS = 200;           // Way too many (causes issues)
const int REQUEST_DELAY_MS = 10;               // Too aggressive
const int MAX_CONNECTIONS = 2;                 // Too few (starvation)
const bool CACHE_DNS = false;                  // No DNS caching!
const bool IMPLEMENT_RETRY = false;            // No retry logic
const int SOCKET_TIMEOUT_MS = 5000;            // Too short

// Statistics
atomic<long long> RequestsSent(0);
atomic<long long> RequestsSucceeded(0);
atomic<long long> RequestsFailed(0);
atomic<long long> DnsLookups(0);
atomic<long long> TimeoutErrors(0);
atomic<long long> ConnectionPoolStarvation(0);

bool g_running = true;
mutex g_consoleMutex;

// PROBLEM: DNS lookup for every request (no caching)
bool ResolveDNS(const string& hostname, sockaddr_in& addr) {
    DnsLookups++;
    
    // PROBLEM: Blocking DNS lookup every time
    struct addrinfo hints = {}, *result = nullptr;
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    
    if (getaddrinfo(hostname.c_str(), nullptr, &hints, &result) != 0) {
        return false;
    }
    
    if (result) {
        addr = *(sockaddr_in*)result->ai_addr;
        freeaddrinfo(result);
        return true;
    }
    
    return false;
}

string BuildHttpGetRequest(const string& path, const string& host) {
    ostringstream request;
    request << "GET " << path << " HTTP/1.1\r\n";
    request << "Host: " << host << "\r\n";
    request << "Connection: close\r\n";
    request << "User-Agent: NetworkDemo/1.0\r\n";
    request << "\r\n";
    return request.str();
}

void MakeProblematicRequest(int clientId) {
    random_device rd;
    mt19937 gen(rd() + clientId);
    uniform_int_distribution<> pathDis(1, 5);
    
    while (g_running) {
        SOCKET clientSocket = INVALID_SOCKET;
        
        try {
            // PROBLEM: Create new socket for each request (no pooling!)
            clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
            if (clientSocket == INVALID_SOCKET) {
                RequestsFailed++;
                ConnectionPoolStarvation++;
                this_thread::sleep_for(milliseconds(REQUEST_DELAY_MS));
                continue;
            }
            
            // PROBLEM: Very short timeout
            int timeout = SOCKET_TIMEOUT_MS;
            setsockopt(clientSocket, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout));
            setsockopt(clientSocket, SOL_SOCKET, SO_SNDTIMEO, (char*)&timeout, sizeof(timeout));
            
            RequestsSent++;
            
            // PROBLEM: DNS lookup for EVERY request (no caching!)
            sockaddr_in serverAddr = {};
            serverAddr.sin_family = AF_INET;
            serverAddr.sin_port = htons(TARGET_PORT);
            
            if (!CACHE_DNS) {
                if (!ResolveDNS(TARGET_HOST, serverAddr)) {
                    RequestsFailed++;
                    closesocket(clientSocket);
                    this_thread::sleep_for(milliseconds(REQUEST_DELAY_MS));
                    continue;
                }
            }
            
            // Connect
            if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
                RequestsFailed++;
                closesocket(clientSocket);
                this_thread::sleep_for(milliseconds(REQUEST_DELAY_MS));
                continue;
            }
            
            // Send HTTP GET request
            string paths[] = {"/get", "/status/200", "/delay/1", "/headers", "/user-agent"};
            string request = BuildHttpGetRequest(paths[pathDis(gen) - 1], TARGET_HOST);
            
            // PROBLEM: No retry logic - fails immediately
            int sent = send(clientSocket, request.c_str(), (int)request.length(), 0);
            if (sent == SOCKET_ERROR) {
                RequestsFailed++;
                closesocket(clientSocket);
                this_thread::sleep_for(milliseconds(REQUEST_DELAY_MS));
                continue;
            }
            
            // Receive response
            char buffer[4096];
            int received = recv(clientSocket, buffer, sizeof(buffer), 0);
            
            if (received > 0) {
                // Check for rate limiting (429) or other errors
                string response(buffer, received);
                if (response.find("429") != string::npos || response.find("Too Many") != string::npos) {
                    // PROBLEM: No backoff - immediately try again
                    RequestsFailed++;
                } else if (response.find("200 OK") != string::npos) {
                    RequestsSucceeded++;
                } else {
                    RequestsFailed++;
                }
            } else if (received == 0) {
                RequestsFailed++;
            } else {
                TimeoutErrors++;
                RequestsFailed++;
            }
            
        } catch (...) {
            RequestsFailed++;
        }
        
        // PROBLEM: Not always disposing properly (30% leak)
        if (clientSocket != INVALID_SOCKET) {
            if ((gen() % 100) < 70) {  // Only close 70% of the time
                closesocket(clientSocket);
            }
            // 30% leak!
        }
        
        // PROBLEM: No delay or very short delay (hammers the server)
        this_thread::sleep_for(milliseconds(REQUEST_DELAY_MS));
    }
}

void MonitorPerformance() {
    auto startTime = steady_clock::now();
    
    while (g_running) {
        this_thread::sleep_for(seconds(2));
        
        auto runtime = duration_cast<seconds>(steady_clock::now() - startTime).count();
        if (runtime == 0) runtime = 1;
        
        system("cls");
        cout << "=======================================================" << endl;
        cout << "  DNS/Pooling/RateLimit PROBLEMS - Real-Time Stats" << endl;
        cout << "=======================================================" << endl;
        cout << endl;
        
        cout << "Runtime: " << (runtime / 60) << "m " << (runtime % 60) << "s" << endl;
        cout << endl;
        
        cout << "Request Statistics:" << endl;
        cout << "  Sent:          " << RequestsSent.load() << endl;
        cout << "  Succeeded:     " << RequestsSucceeded.load() << endl;
        cout << "  Failed:        " << RequestsFailed.load() << endl;
        
        if (RequestsSent > 0) {
            double successRate = (RequestsSucceeded.load() * 100.0) / RequestsSent.load();
            cout << "  Success Rate:  " << fixed << setprecision(1) 
                 << successRate << "%" << endl;
            cout << "  Throughput:    " << fixed << setprecision(1)
                 << (RequestsSucceeded.load() / (double)runtime) << " req/sec" << endl;
        }
        cout << endl;
        
        cout << "Problem Indicators:" << endl;
        cout << "  DNS Lookups:             " << DnsLookups.load() << " (no caching!)" << endl;
        cout << "  Timeout Errors:          " << TimeoutErrors.load() << endl;
        cout << "  Pool Starvation:         " << ConnectionPoolStarvation.load() << endl;
        cout << endl;
        
        cout << "PROBLEMS OBSERVED:" << endl;
        double failureRate = RequestsSent > 0 ? (RequestsFailed.load() * 100.0) / RequestsSent.load() : 0;
        cout << "  x High failure rate: " << fixed << setprecision(1) << failureRate << "%" << endl;
        cout << "  x No DNS caching: " << DnsLookups.load() << " lookups" << endl;
        cout << "  x Connection starvation: " << ConnectionPoolStarvation.load() << endl;
        cout << "  x Timeout issues: " << TimeoutErrors.load() << endl;
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
    cout << "  DNS, Connection Pooling, Rate Limiting PROBLEMS" << endl;
    cout << "  WARNING: This demonstrates BAD practices!" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "PROBLEM VERSION - Intentional Bad Practices:" << endl;
    cout << "x " << CONCURRENT_REQUESTS << " concurrent requests (overwhelming)" << endl;
    cout << "x " << REQUEST_DELAY_MS << "ms delay (too aggressive)" << endl;
    cout << "x " << MAX_CONNECTIONS << " max connections (starvation)" << endl;
    cout << "x No DNS caching (lookup every request)" << endl;
    cout << "x No retry logic (fails immediately)" << endl;
    cout << "x No exponential backoff" << endl;
    cout << "x " << SOCKET_TIMEOUT_MS << "ms timeout (too short)" << endl;
    cout << "x New socket per request (no pooling)" << endl;
    cout << endl;
    
    cout << "Expected Problems:" << endl;
    cout << "- High DNS query rate (no caching)" << endl;
    cout << "- High failure rate (>30%)" << endl;
    cout << "- Connection pool starvation" << endl;
    cout << "- Timeout errors" << endl;
    cout << "- Socket leaks (30%)" << endl;
    cout << endl;
    
    cout << "Press any key to start problematic demonstration..." << endl;
    cin.get();
    
    // Start monitoring
    thread monitorThread(MonitorPerformance);
    
    // Start many concurrent clients
    vector<thread> clientThreads;
    for (int i = 0; i < CONCURRENT_REQUESTS; i++) {
        clientThreads.push_back(thread(MakeProblematicRequest, i));
        this_thread::sleep_for(milliseconds(5));
    }
    
    cout << "Started " << CONCURRENT_REQUESTS << " concurrent clients" << endl;
    cout << "Hammering server without proper rate limiting..." << endl;
    cout << endl;
    
    // Wait for threads
    if (monitorThread.joinable()) monitorThread.join();
    for (auto& t : clientThreads) {
        if (t.joinable()) t.join();
    }
    
    // Display final statistics
    cout << endl;
    cout << "=======================================================" << endl;
    cout << "           FINAL STATISTICS - PROBLEM VERSION" << endl;
    cout << "=======================================================" << endl;
    cout << endl;
    
    cout << "Request Performance:" << endl;
    cout << "  Total Sent:       " << RequestsSent.load() << endl;
    cout << "  Succeeded:        " << RequestsSucceeded.load() << endl;
    cout << "  Failed:           " << RequestsFailed.load() << endl;
    
    if (RequestsSent > 0) {
        double successRate = (RequestsSucceeded.load() * 100.0) / RequestsSent.load();
        cout << "  Success Rate:     " << fixed << setprecision(1) << successRate << "%" << endl;
    }
    cout << endl;
    
    cout << "Problems Encountered:" << endl;
    cout << "  DNS Lookups:           " << DnsLookups.load() << endl;
    cout << "  Timeout Errors:        " << TimeoutErrors.load() << endl;
    cout << "  Pool Starvation:       " << ConnectionPoolStarvation.load() << endl;
    cout << endl;
    
    cout << "PROBLEMS DEMONSTRATED:" << endl;
    cout << "x No DNS caching - every request does DNS lookup" << endl;
    cout << "x Connection starvation - limited sockets" << endl;
    cout << "x No rate limiting - overwhelming server" << endl;
    cout << "x No retry logic - gives up immediately" << endl;
    cout << "x Socket leaks - not disposing properly" << endl;
    cout << "x Aggressive timing - triggers rate limits" << endl;
    cout << endl;
    
    cout << "Solutions needed:" << endl;
    cout << "  - Implement DNS caching" << endl;
    cout << "  - Use connection pooling" << endl;
    cout << "  - Add client-side rate limiting" << endl;
    cout << "  - Implement retry with exponential backoff" << endl;
    cout << "  - Proper socket disposal" << endl;
    cout << "  - Increase timeouts" << endl;
    
    WSACleanup();
    return 0;
}

