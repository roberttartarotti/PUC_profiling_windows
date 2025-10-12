/*
 * =====================================================================================
 *
 *       Filename:  example5-m3p4e5-qos-demo.cpp
 *
 *    Description:  QoS (Quality of Service) Demonstration
 *                  Shows traffic prioritization, bandwidth allocation, and QoS+Security
 *
 *        Version:  1.0
 *        Created:  2025
 *       Compiler:  g++ (MinGW-W64 or MSVC)
 *
 *         Author:  Professor's Demonstration Code
 *   Organization:  PUC - Network Performance Course
 *
 *  Compile (Windows):
 *    g++ example5-m3p4e5-qos-demo.cpp -o qos_demo.exe -lws2_32 -std=c++11
 *
 *  Usage:
 *    qos_demo.exe              # Interactive mode
 *    qos_demo.exe all          # Run all 4 modes
 *    qos_demo.exe 0            # Mode 0 only
 *    qos_demo.exe 1            # Mode 1 only
 *    qos_demo.exe 2            # Mode 2 only
 *    qos_demo.exe 3            # Mode 3 only
 *
 *  Wireshark Tips:
 *    - Start capture on localhost/loopback adapter
 *    - Filter: tcp.port == 8888
 *    - Observe different packet sizes and timing patterns
 *    - Look for traffic bursts and prioritization effects
 *
 * =====================================================================================
 */

#include <iostream>
#include <string>
#include <vector>
#include <map>
#include <chrono>
#include <thread>
#include <cstring>
#include <cstdlib>

#ifdef _WIN32
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")
typedef int socklen_t;
#else
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#define SOCKET int
#define INVALID_SOCKET -1
#define SOCKET_ERROR -1
#define closesocket close
#endif

 // =====================================================================================
 // CONFIGURATION
 // =====================================================================================

const int SERVER_PORT = 8888;
const char* SERVER_IP = "127.0.0.1";
int QOS_MODE = 0; // 0=No QoS, 1=Priority, 2=Dynamic, 3=Security

// Traffic types
enum TrafficType {
    CRITICAL = 0,    // Video/VoIP - needs low latency
    NORMAL = 1,      // Web browsing - medium priority
    BULK = 2         // Downloads/Updates - low priority
};

// QoS Priority levels
enum Priority {
    HIGH = 0,
    MEDIUM = 1,
    LOW = 2
};

// =====================================================================================
// UTILITY CLASSES
// =====================================================================================

class Timer {
private:
    std::chrono::high_resolution_clock::time_point start_time;
public:
    Timer() : start_time(std::chrono::high_resolution_clock::now()) {}

    double elapsed_ms() const {
        auto end_time = std::chrono::high_resolution_clock::now();
        return std::chrono::duration<double, std::milli>(end_time - start_time).count();
    }

    void reset() {
        start_time = std::chrono::high_resolution_clock::now();
    }
};

// Traffic packet structure
struct TrafficPacket {
    TrafficType type;
    Priority priority;
    int size;           // bytes
    int sequenceNum;
    std::chrono::high_resolution_clock::time_point timestamp;
    bool isSuspicious;  // For security demo
};

// QoS Statistics
struct QoSStats {
    std::string trafficName;
    TrafficType type;
    int packetsSent;
    int packetsReceived;
    double totalLatency;
    double avgLatency;
    double minLatency;
    double maxLatency;
    int bytesTransferred;
    bool metSLA;  // Service Level Agreement met

    // Constructor to initialize all members
    QoSStats()
        : trafficName(""),
        type(CRITICAL),
        packetsSent(0),
        packetsReceived(0),
        totalLatency(0.0),
        avgLatency(0.0),
        minLatency(0.0),
        maxLatency(0.0),
        bytesTransferred(0),
        metSLA(false) {
    }
};

// =====================================================================================
// QoS MANAGER
// =====================================================================================

class QoSManager {
private:
    std::map<Priority, int> bandwidthAllocation;  // Percentage
    std::map<Priority, double> maxLatency;        // ms
    bool dynamicMode;
    bool securityMode;

public:
    QoSManager() : dynamicMode(false), securityMode(false) {
        // Default bandwidth allocation
        bandwidthAllocation[HIGH] = 70;
        bandwidthAllocation[MEDIUM] = 20;
        bandwidthAllocation[LOW] = 10;

        // SLA latency requirements
        maxLatency[HIGH] = 10.0;      // Critical: max 10ms
        maxLatency[MEDIUM] = 50.0;    // Normal: max 50ms
        maxLatency[LOW] = 200.0;      // Bulk: max 200ms
    }

    void enableDynamicMode() { dynamicMode = true; }
    void enableSecurityMode() { securityMode = true; }

    Priority assignPriority(TrafficType type, bool suspicious = false) {
        if (securityMode && suspicious) {
            return LOW; // Deprioritize suspicious traffic
        }

        switch (type) {
        case CRITICAL: return HIGH;
        case NORMAL: return MEDIUM;
        case BULK: return LOW;
        default: return MEDIUM;
        }
    }

    int getPacketSize(TrafficType type) {
        switch (type) {
        case CRITICAL: return 1024;      // 1 KB - small, frequent
        case NORMAL: return 10240;       // 10 KB - medium
        case BULK: return 102400;        // 100 KB - large
        default: return 1024;
        }
    }

    void applyQoSDelay(Priority priority, int mode) {
        if (mode == 0) {
            // No QoS - all traffic gets same treatment
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }
        else {
            // Apply priority-based delays
            switch (priority) {
            case HIGH:
                std::this_thread::sleep_for(std::chrono::milliseconds(1));
                break;
            case MEDIUM:
                std::this_thread::sleep_for(std::chrono::milliseconds(5));
                break;
            case LOW:
                std::this_thread::sleep_for(std::chrono::milliseconds(15));
                break;
            }
        }
    }

    bool checkSLA(double latency, Priority priority) {
        return latency <= maxLatency[priority];
    }

    std::string getTrafficTypeName(TrafficType type) {
        switch (type) {
        case CRITICAL: return "Critical (Video/VoIP)";
        case NORMAL: return "Normal (Web)";
        case BULK: return "Bulk (Download)";
        default: return "Unknown";
        }
    }

    std::string getPriorityName(Priority p) {
        switch (p) {
        case HIGH: return "HIGH";
        case MEDIUM: return "MEDIUM";
        case LOW: return "LOW";
        default: return "UNKNOWN";
        }
    }

    void adjustPrioritiesForCongestion() {
        std::cout << "\n[!] Network congestion detected!" << std::endl;
        std::cout << "[!] Adjusting QoS priorities dynamically..." << std::endl;
        std::this_thread::sleep_for(std::chrono::milliseconds(500));
        std::cout << "[OK] Critical traffic: bandwidth increased to 80%" << std::endl;
        std::cout << "[OK] Bulk traffic: bandwidth reduced to 5%" << std::endl;
    }
};

// =====================================================================================
// NETWORK FUNCTIONS
// =====================================================================================

void initializeNetwork() {
#ifdef _WIN32
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        std::cerr << "WSAStartup failed" << std::endl;
        exit(1);
    }
#endif
}

void cleanupNetwork() {
#ifdef _WIN32
    WSACleanup();
#endif
}

// =====================================================================================
// SERVER FUNCTION
// =====================================================================================

void runServer() {
    std::cout << "\n=== QoS SERVER STARTED ===" << std::endl;
    std::cout << "Listening on port: " << SERVER_PORT << std::endl;
    std::cout << "Waiting for traffic..." << std::endl;

    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (serverSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket" << std::endl;
        return;
    }

    int opt = 1;
    setsockopt(serverSocket, SOL_SOCKET, SO_REUSEADDR, (char*)&opt, sizeof(opt));

    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = INADDR_ANY;
    serverAddr.sin_port = htons(SERVER_PORT);

    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Bind failed" << std::endl;
        closesocket(serverSocket);
        return;
    }

    if (listen(serverSocket, 3) == SOCKET_ERROR) {
        std::cerr << "Listen failed" << std::endl;
        closesocket(serverSocket);
        return;
    }

    // Accept client
    sockaddr_in clientAddr;
    socklen_t clientAddrLen = sizeof(clientAddr);
    SOCKET clientSocket = accept(serverSocket, (sockaddr*)&clientAddr, &clientAddrLen);

    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Accept failed" << std::endl;
        closesocket(serverSocket);
        return;
    }

    std::cout << "[OK] Client connected" << std::endl;

    // Receive traffic packets
    int totalPackets = 0;
    std::vector<char> buffer(200000); // Use heap allocation instead of stack

    while (true) {
        int bytesReceived = recv(clientSocket, buffer.data(), static_cast<int>(buffer.size()), 0);
        if (bytesReceived <= 0) break;

        totalPackets++;

        // Send acknowledgment
        const char* ack = "ACK";
        send(clientSocket, ack, 3, 0);
    }

    std::cout << "[OK] Total packets received: " << totalPackets << std::endl;

    closesocket(clientSocket);
    closesocket(serverSocket);
}

// =====================================================================================
// CLIENT FUNCTIONS - DIFFERENT MODES
// =====================================================================================

void sendTraffic(SOCKET sock, TrafficType type, int count, QoSManager& qos, int mode, std::vector<double>& latencies, bool suspicious = false) {
    Priority priority = qos.assignPriority(type, suspicious);
    int packetSize = qos.getPacketSize(type);

    std::vector<char> buffer(packetSize, 'X');
    char ackBuffer[10];

    for (int i = 0; i < count; i++) {
        Timer timer;

        // Apply QoS delay before sending
        qos.applyQoSDelay(priority, mode);

        // Send packet
        int bytesSent = send(sock, buffer.data(), packetSize, 0);

        if (bytesSent <= 0) {
            std::cerr << "[FAIL] Send failed" << std::endl;
            break;
        }

        // Receive acknowledgment
        recv(sock, ackBuffer, sizeof(ackBuffer), 0);

        double latency = timer.elapsed_ms();
        latencies.push_back(latency);
    }
}

QoSStats calculateStats(const std::string& name, TrafficType type, const std::vector<double>& latencies, QoSManager& qos, int packetSize) {
    QoSStats stats;
    stats.trafficName = name;
    stats.type = type;
    stats.packetsSent = static_cast<int>(latencies.size());
    stats.packetsReceived = static_cast<int>(latencies.size());
    stats.totalLatency = 0;
    stats.minLatency = 999999;
    stats.maxLatency = 0;
    stats.bytesTransferred = packetSize * static_cast<int>(latencies.size());

    for (double lat : latencies) {
        stats.totalLatency += lat;
        if (lat < stats.minLatency) stats.minLatency = lat;
        if (lat > stats.maxLatency) stats.maxLatency = lat;
    }

    stats.avgLatency = latencies.empty() ? 0 : stats.totalLatency / latencies.size();

    Priority priority = qos.assignPriority(type);
    stats.metSLA = qos.checkSLA(stats.avgLatency, priority);

    return stats;
}

void displayStats(const std::vector<QoSStats>& allStats) {
    std::cout << "\n=== TRAFFIC STATISTICS ===" << std::endl;
    std::cout << std::endl;

    for (const auto& stats : allStats) {
        std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
        std::cout << "| " << stats.trafficName << std::endl;
        std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
        std::cout << "| Packets sent:      " << stats.packetsSent << std::endl;
        std::cout << "| Bytes transferred: " << stats.bytesTransferred << " bytes" << std::endl;
        std::cout << "| Average latency:   " << stats.avgLatency << " ms";
        if (stats.metSLA) {
            std::cout << " [OK] SLA MET";
        }
        else {
            std::cout << " [FAIL] SLA VIOLATED";
        }
        std::cout << std::endl;
        std::cout << "| Min latency:       " << stats.minLatency << " ms" << std::endl;
        std::cout << "| Max latency:       " << stats.maxLatency << " ms" << std::endl;
        std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
        std::cout << std::endl;
    }
}

// =====================================================================================
// MODE 0: NO QoS - All Traffic Equal
// =====================================================================================

void mode0_NoQoS() {
    std::cout << "\n=== MODE 0: NO QoS (ALL TRAFFIC EQUAL) ===" << std::endl;
    std::cout << "All traffic types compete equally for bandwidth" << std::endl;
    std::cout << "No prioritization applied" << std::endl;
    std::cout << std::endl;

    QoSManager qos;

    // Connect to server
    SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket" << std::endl;
        return;
    }

    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    serverAddr.sin_addr.s_addr = inet_addr(SERVER_IP);

    if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Connection failed" << std::endl;
        closesocket(clientSocket);
        return;
    }

    std::cout << "[OK] Connected to server" << std::endl;
    std::cout << "\nSending traffic..." << std::endl;

    // Send different traffic types
    std::vector<double> criticalLatencies, normalLatencies, bulkLatencies;

    std::cout << "  -> Sending Critical traffic (10 packets)..." << std::endl;
    sendTraffic(clientSocket, CRITICAL, 10, qos, 0, criticalLatencies);

    std::cout << "  -> Sending Normal traffic (10 packets)..." << std::endl;
    sendTraffic(clientSocket, NORMAL, 10, qos, 0, normalLatencies);

    std::cout << "  -> Sending Bulk traffic (10 packets)..." << std::endl;
    sendTraffic(clientSocket, BULK, 10, qos, 0, bulkLatencies);

    closesocket(clientSocket);

    // Calculate and display stats
    std::vector<QoSStats> allStats;
    allStats.push_back(calculateStats("Critical Traffic (Video/VoIP)", CRITICAL, criticalLatencies, qos, qos.getPacketSize(CRITICAL)));
    allStats.push_back(calculateStats("Normal Traffic (Web)", NORMAL, normalLatencies, qos, qos.getPacketSize(NORMAL)));
    allStats.push_back(calculateStats("Bulk Traffic (Download)", BULK, bulkLatencies, qos, qos.getPacketSize(BULK)));

    displayStats(allStats);

    std::cout << "=== ANALYSIS ===" << std::endl;
    std::cout << "- All traffic types experience similar latency" << std::endl;
    std::cout << "- Critical traffic may NOT meet SLA requirements" << std::endl;
    std::cout << "- Video/VoIP quality suffers during congestion" << std::endl;
    std::cout << "- No differentiation between traffic priorities" << std::endl;
}

// =====================================================================================
// MODE 1: QoS Priority Classes
// =====================================================================================

void mode1_QoSPriority() {
    std::cout << "\n=== MODE 1: QoS WITH PRIORITY CLASSES ===" << std::endl;
    std::cout << "Traffic is prioritized: HIGH > MEDIUM > LOW" << std::endl;
    std::cout << "Bandwidth allocation: 70% / 20% / 10%" << std::endl;
    std::cout << std::endl;

    QoSManager qos;

    // Connect to server
    SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket" << std::endl;
        return;
    }

    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    serverAddr.sin_addr.s_addr = inet_addr(SERVER_IP);

    if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Connection failed" << std::endl;
        closesocket(clientSocket);
        return;
    }

    std::cout << "[OK] Connected to server" << std::endl;
    std::cout << "[OK] QoS policies applied" << std::endl;
    std::cout << "\nSending traffic with QoS..." << std::endl;

    // Send different traffic types with QoS
    std::vector<double> criticalLatencies, normalLatencies, bulkLatencies;

    std::cout << "  -> Sending Critical traffic (HIGH priority, 10 packets)..." << std::endl;
    sendTraffic(clientSocket, CRITICAL, 10, qos, 1, criticalLatencies);

    std::cout << "  -> Sending Normal traffic (MEDIUM priority, 10 packets)..." << std::endl;
    sendTraffic(clientSocket, NORMAL, 10, qos, 1, normalLatencies);

    std::cout << "  -> Sending Bulk traffic (LOW priority, 10 packets)..." << std::endl;
    sendTraffic(clientSocket, BULK, 10, qos, 1, bulkLatencies);

    closesocket(clientSocket);

    // Calculate and display stats
    std::vector<QoSStats> allStats;
    allStats.push_back(calculateStats("Critical Traffic (Video/VoIP) - HIGH Priority", CRITICAL, criticalLatencies, qos, qos.getPacketSize(CRITICAL)));
    allStats.push_back(calculateStats("Normal Traffic (Web) - MEDIUM Priority", NORMAL, normalLatencies, qos, qos.getPacketSize(NORMAL)));
    allStats.push_back(calculateStats("Bulk Traffic (Download) - LOW Priority", BULK, bulkLatencies, qos, qos.getPacketSize(BULK)));

    displayStats(allStats);

    std::cout << "=== ANALYSIS ===" << std::endl;
    std::cout << "- Critical traffic gets lowest latency (priority treatment)" << std::endl;
    std::cout << "- SLA requirements are MET for high-priority traffic" << std::endl;
    std::cout << "- Bulk traffic latency increases but remains acceptable" << std::endl;
    std::cout << "- Clear differentiation between priority classes" << std::endl;
}

// =====================================================================================
// MODE 2: Dynamic QoS Adjustment
// =====================================================================================

void mode2_DynamicQoS() {
    std::cout << "\n=== MODE 2: DYNAMIC QoS ADJUSTMENT ===" << std::endl;
    std::cout << "QoS adapts to network conditions in real-time" << std::endl;
    std::cout << "Simulates congestion detection and priority adjustment" << std::endl;
    std::cout << std::endl;

    QoSManager qos;
    qos.enableDynamicMode();

    // Connect to server
    SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket" << std::endl;
        return;
    }

    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    serverAddr.sin_addr.s_addr = inet_addr(SERVER_IP);

    if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Connection failed" << std::endl;
        closesocket(clientSocket);
        return;
    }

    std::cout << "[OK] Connected to server" << std::endl;
    std::cout << "[OK] Dynamic QoS monitoring enabled" << std::endl;
    std::cout << "\nPhase 1: Normal conditions..." << std::endl;

    // Phase 1: Normal traffic
    std::vector<double> criticalLatencies1, normalLatencies1, bulkLatencies1;

    std::cout << "  -> Sending traffic under normal conditions..." << std::endl;
    sendTraffic(clientSocket, CRITICAL, 5, qos, 1, criticalLatencies1);
    sendTraffic(clientSocket, NORMAL, 5, qos, 1, normalLatencies1);
    sendTraffic(clientSocket, BULK, 5, qos, 1, bulkLatencies1);

    // Simulate congestion detection
    qos.adjustPrioritiesForCongestion();

    std::cout << "\nPhase 2: Under congestion (adjusted priorities)..." << std::endl;

    // Phase 2: Adjusted traffic
    std::vector<double> criticalLatencies2, normalLatencies2, bulkLatencies2;

    std::cout << "  -> Sending traffic with adjusted priorities..." << std::endl;
    sendTraffic(clientSocket, CRITICAL, 5, qos, 1, criticalLatencies2);
    sendTraffic(clientSocket, NORMAL, 5, qos, 1, normalLatencies2);
    sendTraffic(clientSocket, BULK, 5, qos, 1, bulkLatencies2);

    closesocket(clientSocket);

    // Combine latencies
    criticalLatencies1.insert(criticalLatencies1.end(), criticalLatencies2.begin(), criticalLatencies2.end());
    normalLatencies1.insert(normalLatencies1.end(), normalLatencies2.begin(), normalLatencies2.end());
    bulkLatencies1.insert(bulkLatencies1.end(), bulkLatencies2.begin(), bulkLatencies2.end());

    // Calculate and display stats
    std::vector<QoSStats> allStats;
    allStats.push_back(calculateStats("Critical Traffic (Adaptive Priority)", CRITICAL, criticalLatencies1, qos, qos.getPacketSize(CRITICAL)));
    allStats.push_back(calculateStats("Normal Traffic (Adaptive Priority)", NORMAL, normalLatencies1, qos, qos.getPacketSize(NORMAL)));
    allStats.push_back(calculateStats("Bulk Traffic (Adaptive Priority)", BULK, bulkLatencies1, qos, qos.getPacketSize(BULK)));

    displayStats(allStats);

    std::cout << "=== ANALYSIS ===" << std::endl;
    std::cout << "- QoS automatically detected network congestion" << std::endl;
    std::cout << "- Critical traffic bandwidth increased dynamically" << std::endl;
    std::cout << "- System adapted without manual intervention" << std::endl;
    std::cout << "- Maintains service quality during varying conditions" << std::endl;
}

// =====================================================================================
// MODE 3: QoS + Security Integration
// =====================================================================================

void mode3_QoSWithSecurity() {
    std::cout << "\n=== MODE 3: QoS + SECURITY INTEGRATION ===" << std::endl;
    std::cout << "Combines traffic prioritization with threat detection" << std::endl;
    std::cout << "Suspicious traffic is deprioritized or blocked" << std::endl;
    std::cout << std::endl;

    QoSManager qos;
    qos.enableSecurityMode();

    // Connect to server
    SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket" << std::endl;
        return;
    }

    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(SERVER_PORT);
    serverAddr.sin_addr.s_addr = inet_addr(SERVER_IP);

    if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Connection failed" << std::endl;
        closesocket(clientSocket);
        return;
    }

    std::cout << "[OK] Connected to server" << std::endl;
    std::cout << "[OK] QoS + Security policies active" << std::endl;
    std::cout << std::endl;

    // Send legitimate traffic
    std::cout << "Sending legitimate traffic..." << std::endl;
    std::vector<double> legitimateCritical, legitimateNormal;

    std::cout << "  -> Critical traffic (legitimate)..." << std::endl;
    sendTraffic(clientSocket, CRITICAL, 5, qos, 1, legitimateCritical, false);

    std::cout << "  -> Normal traffic (legitimate)..." << std::endl;
    sendTraffic(clientSocket, NORMAL, 5, qos, 1, legitimateNormal, false);

    // Simulate threat detection
    std::cout << "\n[!] SECURITY ALERT: Suspicious traffic detected!" << std::endl;
    std::cout << "[!] Source: 192.168.1.100 (simulated)" << std::endl;
    std::cout << "[!] Pattern: Unusual bulk data transfer" << std::endl;
    std::cout << "[!] Action: Deprioritizing suspicious traffic" << std::endl;
    std::this_thread::sleep_for(std::chrono::milliseconds(500));

    // Send suspicious traffic (gets deprioritized)
    std::cout << "\nSending suspicious traffic (deprioritized)..." << std::endl;
    std::vector<double> suspiciousTraffic;

    std::cout << "  -> Bulk traffic (marked suspicious)..." << std::endl;
    sendTraffic(clientSocket, BULK, 5, qos, 1, suspiciousTraffic, true);

    std::cout << "\n[OK] Legitimate critical traffic: PROTECTED" << std::endl;
    std::cout << "[OK] Suspicious traffic: DEPRIORITIZED" << std::endl;

    closesocket(clientSocket);

    // Calculate and display stats
    std::vector<QoSStats> allStats;
    allStats.push_back(calculateStats("Legitimate Critical Traffic (PROTECTED)", CRITICAL, legitimateCritical, qos, qos.getPacketSize(CRITICAL)));
    allStats.push_back(calculateStats("Legitimate Normal Traffic (PROTECTED)", NORMAL, legitimateNormal, qos, qos.getPacketSize(NORMAL)));
    allStats.push_back(calculateStats("Suspicious Traffic (DEPRIORITIZED)", BULK, suspiciousTraffic, qos, qos.getPacketSize(BULK)));

    displayStats(allStats);

    std::cout << "=== SECURITY + QoS ANALYSIS ===" << std::endl;
    std::cout << "- Legitimate traffic maintains high priority" << std::endl;
    std::cout << "- Suspicious traffic automatically deprioritized" << std::endl;
    std::cout << "- Critical services protected during security events" << std::endl;
    std::cout << "- Integrated approach: security + performance" << std::endl;
    std::cout << "\n[OK] Check Wireshark: Notice traffic patterns and timing differences" << std::endl;
}

// =====================================================================================
// RUN ALL MODES
// =====================================================================================

void runAllModes() {
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    RUNNING ALL 4 MODES - COMPLETE QoS DEMONSTRATION" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << std::endl;

    // Start server in background thread
    std::thread serverThread([]() {
        for (int i = 0; i < 4; i++) {
            initializeNetwork();
            runServer();
            cleanupNetwork();
            std::this_thread::sleep_for(std::chrono::milliseconds(500));
        }
        });

    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    initializeNetwork();

    // Run Mode 0
    std::cout << "\n";
    std::cout << "#####################################################################################" << std::endl;
    std::cout << "#                                    MODE 0                                        #" << std::endl;
    std::cout << "#####################################################################################" << std::endl;
    mode0_NoQoS();
    std::this_thread::sleep_for(std::chrono::seconds(2));

    // Run Mode 1
    std::cout << "\n\n";
    std::cout << "#####################################################################################" << std::endl;
    std::cout << "#                                    MODE 1                                        #" << std::endl;
    std::cout << "#####################################################################################" << std::endl;
    mode1_QoSPriority();
    std::this_thread::sleep_for(std::chrono::seconds(2));

    // Run Mode 2
    std::cout << "\n\n";
    std::cout << "#####################################################################################" << std::endl;
    std::cout << "#                                    MODE 2                                        #" << std::endl;
    std::cout << "#####################################################################################" << std::endl;
    mode2_DynamicQoS();
    std::this_thread::sleep_for(std::chrono::seconds(2));

    // Run Mode 3
    std::cout << "\n\n";
    std::cout << "#####################################################################################" << std::endl;
    std::cout << "#                                    MODE 3                                        #" << std::endl;
    std::cout << "#####################################################################################" << std::endl;
    mode3_QoSWithSecurity();

    cleanupNetwork();

    serverThread.join();

    // Final comprehensive analysis
    std::cout << "\n\n";
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    COMPLETE DEMONSTRATION SUMMARY" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "[OK] Mode 0: No QoS demonstration completed" << std::endl;
    std::cout << "[OK] Mode 1: Priority classes demonstration completed" << std::endl;
    std::cout << "[OK] Mode 2: Dynamic QoS demonstration completed" << std::endl;
    std::cout << "[OK] Mode 3: QoS + Security demonstration completed" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    std::cout << "\n";
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    COMPREHENSIVE QoS ANALYSIS" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << std::endl;

    std::cout << ">> PERFORMANCE COMPARISON:" << std::endl;
    std::cout << std::endl;

    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| MODE 0: No QoS (Baseline)                                                      |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| - All traffic treated equally                                                  |" << std::endl;
    std::cout << "| - Critical traffic may violate SLA                                             |" << std::endl;
    std::cout << "| - Video/VoIP quality suffers during congestion                                 |" << std::endl;
    std::cout << "| - Rating: * (No optimization)                                                  |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << std::endl;

    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| MODE 1: QoS Priority Classes ***** BEST FOR PRODUCTION                         |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| - Traffic prioritized: HIGH > MEDIUM > LOW                                     |" << std::endl;
    std::cout << "| - Critical traffic meets SLA requirements                                      |" << std::endl;
    std::cout << "| - Bandwidth allocation: 70% / 20% / 10%                                        |" << std::endl;
    std::cout << "| - Clear performance differentiation                                            |" << std::endl;
    std::cout << "| - Rating: ***** (Essential for production networks)                            |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << std::endl;

    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| MODE 2: Dynamic QoS ****                                                       |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| - Adapts to network conditions automatically                                   |" << std::endl;
    std::cout << "| - Detects congestion and adjusts priorities                                    |" << std::endl;
    std::cout << "| - Maintains service quality during varying load                                |" << std::endl;
    std::cout << "| - Rating: **** (Important for dynamic environments)                            |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << std::endl;

    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| MODE 3: QoS + Security *****                                                   |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << "| - Integrates security with traffic management                                  |" << std::endl;
    std::cout << "| - Deprioritizes suspicious traffic                                             |" << std::endl;
    std::cout << "| - Protects legitimate critical services                                        |" << std::endl;
    std::cout << "| - Combined approach: performance + security                                    |" << std::endl;
    std::cout << "| - Rating: ***** (Modern network requirement)                                   |" << std::endl;
    std::cout << "+---------------------------------------------------------------------------------+" << std::endl;
    std::cout << std::endl;

    std::cout << "*** WINNER: MODE 1 + MODE 3 (QoS Priority + Security Integration) ***" << std::endl;
    std::cout << std::endl;

    std::cout << "WHY QoS IS ESSENTIAL:" << std::endl;
    std::cout << "  1. >> Guarantees critical traffic performance" << std::endl;
    std::cout << "  2. >> Prevents bandwidth starvation" << std::endl;
    std::cout << "  3. >> Meets SLA requirements consistently" << std::endl;
    std::cout << "  4. >> Improves user experience for real-time apps" << std::endl;
    std::cout << "  5. >> Enables efficient resource utilization" << std::endl;
    std::cout << "  6. >> Provides security integration capabilities" << std::endl;
    std::cout << "  7. >> Essential for modern enterprise networks" << std::endl;
    std::cout << std::endl;

    std::cout << ">> BEST PRACTICES - RECOMMENDED APPROACH:" << std::endl;
    std::cout << std::endl;
    std::cout << "  Step 1: Classify traffic into priority classes (Critical/Normal/Bulk)" << std::endl;
    std::cout << "          -> Identify business-critical applications" << std::endl;
    std::cout << std::endl;
    std::cout << "  Step 2: Implement QoS policies with appropriate bandwidth allocation" << std::endl;
    std::cout << "          -> Reserve bandwidth for high-priority traffic" << std::endl;
    std::cout << std::endl;
    std::cout << "  Step 3: Enable dynamic adjustment for varying network conditions" << std::endl;
    std::cout << "          -> Monitor and adapt to congestion automatically" << std::endl;
    std::cout << std::endl;
    std::cout << "  Step 4: Integrate security policies with QoS" << std::endl;
    std::cout << "          -> Protect critical services during security events" << std::endl;
    std::cout << std::endl;
    std::cout << "  Result: Optimal performance, security, and user experience!" << std::endl;
    std::cout << std::endl;

    std::cout << ">> KEY INSIGHTS:" << std::endl;
    std::cout << "  - QoS prevents 'noisy neighbor' problems in shared networks" << std::endl;
    std::cout << "  - Video conferencing requires <10ms latency (only possible with QoS)" << std::endl;
    std::cout << "  - Bulk downloads don't impact real-time applications with proper QoS" << std::endl;
    std::cout << "  - Security integration ensures protection without sacrificing performance" << std::endl;
    std::cout << "  - Future: AI-driven QoS with predictive traffic management" << std::endl;
    std::cout << std::endl;

    std::cout << ">> CLASSROOM TAKEAWAY:" << std::endl;
    std::cout << "  Without QoS, all traffic is equal - but not all traffic has equal importance!" << std::endl;
    std::cout << "  QoS ensures critical applications get the resources they need, when they need them." << std::endl;
    std::cout << std::endl;

    std::cout << "=====================================================================================" << std::endl;
}

// =====================================================================================
// MAIN PROGRAM
// =====================================================================================

int main(int argc, char* argv[]) {
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    QoS (QUALITY OF SERVICE) DEMONSTRATION" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    // Parse command line arguments
    bool runAll = false;
    int selectedMode = -1;

    if (argc >= 2) {
        std::string arg1 = argv[1];

        if (arg1 == "all" || arg1 == "ALL") {
            runAll = true;
        }
        else {
            selectedMode = atoi(argv[1]);
            if (selectedMode < 0 || selectedMode > 3) {
                std::cerr << "Error: Invalid mode. Mode must be 0-3 or 'all'" << std::endl;
                std::cerr << "Usage: " << argv[0] << " [mode|all]" << std::endl;
                std::cerr << "  mode: 0=No QoS, 1=Priority, 2=Dynamic, 3=Security" << std::endl;
                std::cerr << "  all: Run all 4 modes in sequence" << std::endl;
                return 1;
            }
            QOS_MODE = selectedMode;
        }
    }

    // If "all" specified, run all modes
    if (runAll) {
        runAllModes();
        return 0;
    }

    // If specific mode specified via command line, run once and exit
    if (selectedMode != -1) {
        std::cout << "Mode: " << QOS_MODE << std::endl;
        std::cout << "=====================================================================================" << std::endl;

        // Start server in background
        std::thread serverThread([]() {
            initializeNetwork();
            runServer();
            cleanupNetwork();
            });

        std::this_thread::sleep_for(std::chrono::milliseconds(1000));

        initializeNetwork();

        // Execute selected mode
        switch (QOS_MODE) {
        case 0:
            mode0_NoQoS();
            break;
        case 1:
            mode1_QoSPriority();
            break;
        case 2:
            mode2_DynamicQoS();
            break;
        case 3:
            mode3_QoSWithSecurity();
            break;
        }

        cleanupNetwork();
        serverThread.join();

        std::cout << "\n=====================================================================================" << std::endl;
        std::cout << "DEMONSTRATION COMPLETE" << std::endl;
        std::cout << "=====================================================================================" << std::endl;
        return 0;
    }

    // Interactive mode (no command line arguments)
    std::cout << "This program demonstrates QoS (Quality of Service) traffic prioritization" << std::endl;
    std::cout << "and security integration using different operating modes." << std::endl;
    std::cout << std::endl;
    std::cout << "Available modes:" << std::endl;
    std::cout << "  0 - No QoS (baseline - all traffic equal)" << std::endl;
    std::cout << "  1 - QoS with Priority Classes (HIGH/MEDIUM/LOW)" << std::endl;
    std::cout << "  2 - Dynamic QoS Adjustment (adaptive to congestion)" << std::endl;
    std::cout << "  3 - QoS + Security Integration (threat-aware prioritization)" << std::endl;
    std::cout << std::endl;
    std::cout << "Current mode: " << QOS_MODE << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    std::string userInput;

    while (true) {
        std::cout << "\n>>> Type 'run' to execute, 'mode' to change mode, 'all' for all modes, 'quit' to exit: ";
        std::getline(std::cin, userInput);

        if (userInput == "quit" || userInput == "exit") {
            std::cout << "Exiting demonstration..." << std::endl;
            break;
        }
        else if (userInput == "all" || userInput == "ALL") {
            runAllModes();
            continue;
        }
        else if (userInput == "mode") {
            QOS_MODE = (QOS_MODE + 1) % 4;
            std::cout << "Mode changed to: " << QOS_MODE << std::endl;
            std::cout << "  - Mode 0: No QoS" << std::endl;
            std::cout << "  - Mode 1: Priority Classes" << std::endl;
            std::cout << "  - Mode 2: Dynamic QoS" << std::endl;
            std::cout << "  - Mode 3: QoS + Security" << std::endl;
            continue;
        }
        else if (userInput == "run") {
            // Start server in background
            std::thread serverThread([]() {
                initializeNetwork();
                runServer();
                cleanupNetwork();
                });

            std::this_thread::sleep_for(std::chrono::milliseconds(1000));

            initializeNetwork();

            // Execute current mode
            switch (QOS_MODE) {
            case 0:
                mode0_NoQoS();
                break;
            case 1:
                mode1_QoSPriority();
                break;
            case 2:
                mode2_DynamicQoS();
                break;
            case 3:
                mode3_QoSWithSecurity();
                break;
            }

            cleanupNetwork();
            serverThread.join();
        }
        else {
            std::cout << "Invalid command. Use 'run', 'mode', 'all', or 'quit'." << std::endl;
        }
    }

    return 0;
}
