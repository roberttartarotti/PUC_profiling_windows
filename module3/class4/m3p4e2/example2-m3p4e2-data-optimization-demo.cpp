/*
 * =====================================================================================
 * DATA OPTIMIZATION DEMONSTRATION - C++ (MODULE 3, CLASS 4 - EXAMPLE 2)
 * =====================================================================================
 *
 * Purpose: Demonstrate various data optimization techniques to reduce bandwidth usage
 *          and improve application performance
 *
 * Educational Context:
 * - Show deduplication techniques to avoid redundant data
 * - Demonstrate compact data formats (JSON vs Binary formats)
 * - Illustrate payload optimization and header minimization
 * - Compare different compression algorithms (gzip, custom)
 * - Show benefits: lower bandwidth, faster loading, energy savings
 *
 * What this demonstrates:
 * - Data deduplication reduces redundant information
 * - Binary formats are more efficient than text formats
 * - Compression algorithms can significantly reduce data size
 * - Optimized payloads improve network performance
 * - Multiple optimization techniques can be combined
 *
 * Usage:
 * - Compile: g++ -o data_optimization_demo example2-m3p4e2-data-optimization-demo.cpp
 * - Run: ./data_optimization_demo
 * - Monitor with Wireshark on loopback interface (127.0.0.1)
 * - Toggle optimization mode with global variable OPTIMIZATION_MODE
 *
 * =====================================================================================
 */

#include <iostream>
#include <vector>
#include <string>
#include <cstring>
#include <thread>
#include <chrono>
#include <map>

#ifdef _WIN32
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")
#define close closesocket
#else
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#endif

// =====================================================================================
// CONFIGURATION - EASY TOGGLE FOR CLASSROOM DEMONSTRATION
// =====================================================================================

// Optimization modes: 0=no optimization, 1=deduplication, 2=binary format, 3=compression, 4=all
// CHANGE THIS LINE TO DEMONSTRATE DIFFERENT MODES IN YOUR CLASS:
int OPTIMIZATION_MODE = 0;  // Set to 0-4 to test different optimization levels

const int SERVER_PORT = 8888;
const int BUFFER_SIZE = 65536;  // 64KB buffer

// =====================================================================================
// DATA STRUCTURES FOR DEMONSTRATION
// =====================================================================================

struct UserData {
    int id;
    std::string name;
    std::string email;
    std::string department;
    double salary;
    bool active;
};

struct OptimizedUserData {
    int id;
    int name_id;        // Reference to deduplicated name
    int email_id;       // Reference to deduplicated email
    int dept_id;        // Reference to deduplicated department
    double salary;
    bool active;
};

// =====================================================================================
// DEDUPLICATION SYSTEM
// =====================================================================================

class DeduplicationManager {
private:
    std::map<std::string, int> string_to_id;
    std::vector<std::string> id_to_string;
    int next_id;

public:
    DeduplicationManager() : next_id(1) {}
    
    int addString(const std::string& str) {
        if (string_to_id.find(str) != string_to_id.end()) {
            return string_to_id[str];  // Return existing ID
        }
        
        int id = next_id++;
        string_to_id[str] = id;
        id_to_string.push_back(str);
        return id;
    }
    
    std::string getString(int id) const {
        if (id > 0 && id <= id_to_string.size()) {
            return id_to_string[id - 1];
        }
        return "";
    }
    
    size_t getDictionarySize() const {
        return id_to_string.size();
    }
    
    size_t getTotalDictionaryBytes() const {
        size_t total = 0;
        for (const auto& str : id_to_string) {
            total += str.length();
        }
        return total;
    }
};

// =====================================================================================
// COMPRESSION UTILITIES
// =====================================================================================

std::vector<uint8_t> simpleCompress(const std::vector<uint8_t>& data) {
    if (data.empty()) return data;
    
    std::vector<uint8_t> compressed;
    compressed.reserve(data.size() / 2);
    
    size_t i = 0;
    while (i < data.size()) {
        uint8_t current = data[i];
        size_t count = 1;
        
        // Count consecutive identical bytes
        while (i + count < data.size() && data[i + count] == current && count < 255) {
            count++;
        }
        
        // If we have 3 or more consecutive bytes, use RLE compression
        if (count >= 3) {
            compressed.push_back(0xFF); // RLE marker
            compressed.push_back(current);
            compressed.push_back(static_cast<uint8_t>(count));
            i += count;
        } else {
            // Store bytes as-is
            for (size_t j = 0; j < count; j++) {
                compressed.push_back(data[i + j]);
            }
            i += count;
        }
    }
    
    return compressed;
}

std::vector<uint8_t> decompress(const std::vector<uint8_t>& compressedData) {
    std::vector<uint8_t> decompressed;
    decompressed.reserve(compressedData.size() * 2);
    
    size_t i = 0;
    while (i < compressedData.size()) {
        if (compressedData[i] == 0xFF && i + 2 < compressedData.size()) {
            // RLE expansion
            uint8_t value = compressedData[i + 1];
            uint8_t count = compressedData[i + 2];
            
            for (int j = 0; j < count; j++) {
                decompressed.push_back(value);
            }
            i += 3;
        } else {
            decompressed.push_back(compressedData[i]);
            i++;
        }
    }
    
    return decompressed;
}

// =====================================================================================
// DATA FORMAT CONVERTERS
// =====================================================================================

std::string userDataToJSON(const std::vector<UserData>& users) {
    std::string json = "{\"users\":[";
    
    for (size_t i = 0; i < users.size(); i++) {
        if (i > 0) json += ",";
        json += "{";
        json += "\"id\":" + std::to_string(users[i].id) + ",";
        json += "\"name\":\"" + users[i].name + "\",";
        json += "\"email\":\"" + users[i].email + "\",";
        json += "\"department\":\"" + users[i].department + "\",";
        json += "\"salary\":" + std::to_string(users[i].salary) + ",";
        json += "\"active\":";
        json += (users[i].active ? "true" : "false");
        json += "}";
    }
    
    json += "]}";
    return json;
}

std::vector<uint8_t> userDataToBinary(const std::vector<UserData>& users) {
    std::vector<uint8_t> binary;
    
    // Header: number of users (4 bytes)
    uint32_t userCount = static_cast<uint32_t>(users.size());
    binary.insert(binary.end(), (uint8_t*)&userCount, (uint8_t*)&userCount + 4);
    
    for (const auto& user : users) {
        // ID (4 bytes)
        binary.insert(binary.end(), (uint8_t*)&user.id, (uint8_t*)&user.id + 4);
        
        // Name length + name
        uint32_t nameLen = static_cast<uint32_t>(user.name.length());
        binary.insert(binary.end(), (uint8_t*)&nameLen, (uint8_t*)&nameLen + 4);
        binary.insert(binary.end(), user.name.begin(), user.name.end());
        
        // Email length + email
        uint32_t emailLen = static_cast<uint32_t>(user.email.length());
        binary.insert(binary.end(), (uint8_t*)&emailLen, (uint8_t*)&emailLen + 4);
        binary.insert(binary.end(), user.email.begin(), user.email.end());
        
        // Department length + department
        uint32_t deptLen = static_cast<uint32_t>(user.department.length());
        binary.insert(binary.end(), (uint8_t*)&emailLen, (uint8_t*)&emailLen + 4);
        binary.insert(binary.end(), user.department.begin(), user.department.end());
        
        // Salary (8 bytes)
        binary.insert(binary.end(), (uint8_t*)&user.salary, (uint8_t*)&user.salary + 8);
        
        // Active (1 byte)
        uint8_t active = user.active ? 1 : 0;
        binary.push_back(active);
    }
    
    return binary;
}

std::vector<uint8_t> userDataToOptimizedBinary(const std::vector<UserData>& users, DeduplicationManager& dedup) {
    std::vector<uint8_t> binary;
    
    // Header: number of users (4 bytes)
    uint32_t userCount = static_cast<uint32_t>(users.size());
    binary.insert(binary.end(), (uint8_t*)&userCount, (uint8_t*)&userCount + 4);
    
    for (const auto& user : users) {
        // ID (4 bytes)
        binary.insert(binary.end(), (uint8_t*)&user.id, (uint8_t*)&user.id + 4);
        
        // Name ID (4 bytes)
        int nameId = dedup.addString(user.name);
        binary.insert(binary.end(), (uint8_t*)&nameId, (uint8_t*)&nameId + 4);
        
        // Email ID (4 bytes)
        int emailId = dedup.addString(user.email);
        binary.insert(binary.end(), (uint8_t*)&emailId, (uint8_t*)&emailId + 4);
        
        // Department ID (4 bytes)
        int deptId = dedup.addString(user.department);
        binary.insert(binary.end(), (uint8_t*)&deptId, (uint8_t*)&deptId + 4);
        
        // Salary (8 bytes)
        binary.insert(binary.end(), (uint8_t*)&user.salary, (uint8_t*)&user.salary + 8);
        
        // Active (1 byte)
        uint8_t active = user.active ? 1 : 0;
        binary.push_back(active);
    }
    
    return binary;
}

// =====================================================================================
// NETWORK UTILITIES
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

int createServerSocket() {
#ifdef _WIN32
    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create server socket" << std::endl;
        return -1;
    }
#else
    int serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket < 0) {
        std::cerr << "Failed to create server socket" << std::endl;
        return -1;
    }
#endif

    // Allow socket reuse
    int opt = 1;
#ifdef _WIN32
    setsockopt(serverSocket, SOL_SOCKET, SO_REUSEADDR, (char*)&opt, sizeof(opt));
#else
    setsockopt(serverSocket, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));
#endif

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = INADDR_ANY;
    serverAddr.sin_port = htons(SERVER_PORT);

    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) < 0) {
        std::cerr << "Failed to bind server socket" << std::endl;
        close(serverSocket);
        return -1;
    }

    if (listen(serverSocket, 1) < 0) {
        std::cerr << "Failed to listen on server socket" << std::endl;
        close(serverSocket);
        return -1;
    }

    return static_cast<int>(serverSocket);
}

int createClientSocket() {
#ifdef _WIN32
    SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create client socket" << std::endl;
        return -1;
    }
#else
    int clientSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (clientSocket < 0) {
        std::cerr << "Failed to create client socket" << std::endl;
        return -1;
    }
#endif

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
    serverAddr.sin_port = htons(SERVER_PORT);

    if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) < 0) {
        std::cerr << "Failed to connect to server" << std::endl;
        close(clientSocket);
        return -1;
    }

    return static_cast<int>(clientSocket);
}

// =====================================================================================
// SERVER IMPLEMENTATION
// =====================================================================================

void runServer() {
    std::cout << "\n=== DATA OPTIMIZATION DEMO SERVER ===" << std::endl;
    std::cout << "Mode: " << OPTIMIZATION_MODE << " (0=None, 1=Dedup, 2=Binary, 3=Compress, 4=All)" << std::endl;
    std::cout << "Listening on port: " << SERVER_PORT << std::endl;
    std::cout << "Monitor with Wireshark on 127.0.0.1:" << SERVER_PORT << std::endl;
    std::cout << "=====================================" << std::endl;

    int serverSocket = createServerSocket();
    if (serverSocket < 0) return;

    std::cout << "Server waiting for client connection..." << std::endl;

    sockaddr_in clientAddr;
    socklen_t clientLen = sizeof(clientAddr);
#ifdef _WIN32
    SOCKET clientSocket = accept(serverSocket, (sockaddr*)&clientAddr, &clientLen);
    if (clientSocket == INVALID_SOCKET) {
        std::cerr << "Failed to accept client connection" << std::endl;
        close(serverSocket);
        return;
    }
#else
    int clientSocket = accept(serverSocket, (sockaddr*)&clientAddr, &clientLen);
    if (clientSocket < 0) {
        std::cerr << "Failed to accept client connection" << std::endl;
        close(serverSocket);
        return;
    }
#endif

    std::cout << "Client connected from " << inet_ntoa(clientAddr.sin_addr)
        << ":" << ntohs(clientAddr.sin_port) << std::endl;

    // Receive data from client
    std::vector<uint8_t> buffer(BUFFER_SIZE);
    int totalBytesReceived = 0;
    int bytesReceived;

    std::cout << "\nReceiving data..." << std::endl;

    while ((bytesReceived = recv(clientSocket, (char*)buffer.data() + totalBytesReceived,
        BUFFER_SIZE - totalBytesReceived, 0)) > 0) {
        totalBytesReceived += bytesReceived;
        std::cout << "Received " << bytesReceived << " bytes (total: "
            << totalBytesReceived << " bytes)" << std::endl;
    }

    if (bytesReceived < 0) {
        std::cerr << "Error receiving data" << std::endl;
        close(clientSocket);
        close(serverSocket);
        return;
    }

    std::cout << "\n=== RECEPTION COMPLETE ===" << std::endl;
    std::cout << "Total bytes received: " << totalBytesReceived << std::endl;
    std::cout << "Data size: " << (totalBytesReceived / 1024.0) << " KB" << std::endl;

    std::cout << "\n=== OPTIMIZATION ANALYSIS ===" << std::endl;
    switch (OPTIMIZATION_MODE) {
        case 0:
            std::cout << "Mode: NO OPTIMIZATION" << std::endl;
            std::cout << "This is the baseline for comparison!" << std::endl;
            break;
        case 1:
            std::cout << "Mode: DEDUPLICATION" << std::endl;
            std::cout << "Redundant data has been eliminated!" << std::endl;
            break;
        case 2:
            std::cout << "Mode: BINARY FORMAT" << std::endl;
            std::cout << "Binary format is more efficient than text!" << std::endl;
            break;
        case 3:
            std::cout << "Mode: COMPRESSION" << std::endl;
            std::cout << "Data has been compressed!" << std::endl;
            break;
        case 4:
            std::cout << "Mode: ALL OPTIMIZATIONS" << std::endl;
            std::cout << "Maximum optimization applied!" << std::endl;
            break;
    }

    close(clientSocket);
    close(serverSocket);
}

// =====================================================================================
// CLIENT IMPLEMENTATION
// =====================================================================================

std::vector<UserData> generateTestData() {
    std::vector<UserData> users;
    
    // Generate test data with some redundancy for deduplication demo
    std::vector<std::string> departments = {"Engineering", "Marketing", "Sales", "HR", "Finance"};
    std::vector<std::string> domains = {"company.com", "corp.net", "business.org"};
    
    for (int i = 1; i <= 100; i++) {
        UserData user;
        user.id = i;
        user.name = "User" + std::to_string(i);
        user.email = "user" + std::to_string(i) + "@" + domains[i % domains.size()];
        user.department = departments[i % departments.size()];
        user.salary = 50000.0 + (i * 1000.0);
        user.active = (i % 10 != 0);
        
        users.push_back(user);
    }
    
    return users;
}

void runClient() {
    std::cout << "\n=== DATA OPTIMIZATION DEMO CLIENT ===" << std::endl;
    std::cout << "Mode: " << OPTIMIZATION_MODE << " (0=None, 1=Dedup, 2=Binary, 3=Compress, 4=All)" << std::endl;
    std::cout << "Connecting to server on port: " << SERVER_PORT << std::endl;
    std::cout << "====================================" << std::endl;

    // Generate test data
    std::vector<UserData> users = generateTestData();
    std::cout << "Generated " << users.size() << " user records" << std::endl;

    // Prepare data based on optimization mode
    std::vector<uint8_t> dataToSend;
    size_t originalSize = 0;

    switch (OPTIMIZATION_MODE) {
        case 0: { // No optimization - JSON format
            std::cout << "\nUsing JSON format (no optimization)..." << std::endl;
            std::string json = userDataToJSON(users);
            dataToSend.assign(json.begin(), json.end());
            originalSize = json.length();
            std::cout << "JSON size: " << originalSize << " bytes" << std::endl;
            break;
        }
        case 1: { // Deduplication
            std::cout << "\nUsing deduplication optimization..." << std::endl;
            DeduplicationManager dedup;
            std::vector<uint8_t> binary = userDataToOptimizedBinary(users, dedup);
            
            // Calculate original size (JSON)
            std::string json = userDataToJSON(users);
            originalSize = json.length();
            
            // Add dictionary to data
            std::vector<uint8_t> dictionary;
            for (size_t i = 0; i < dedup.getDictionarySize(); i++) {
                std::string str = dedup.getString(static_cast<int>(i + 1));
                uint32_t len = static_cast<uint32_t>(str.length());
                dictionary.insert(dictionary.end(), (uint8_t*)&len, (uint8_t*)&len + 4);
                dictionary.insert(dictionary.end(), str.begin(), str.end());
            }
            
            // Combine dictionary + data
            uint32_t dictSize = static_cast<uint32_t>(dictionary.size());
            dataToSend.insert(dataToSend.end(), (uint8_t*)&dictSize, (uint8_t*)&dictSize + 4);
            dataToSend.insert(dataToSend.end(), dictionary.begin(), dictionary.end());
            dataToSend.insert(dataToSend.end(), binary.begin(), binary.end());
            
            std::cout << "Original JSON size: " << originalSize << " bytes" << std::endl;
            std::cout << "Optimized size: " << dataToSend.size() << " bytes" << std::endl;
            std::cout << "Dictionary entries: " << dedup.getDictionarySize() << std::endl;
            break;
        }
        case 2: { // Binary format
            std::cout << "\nUsing binary format optimization..." << std::endl;
            std::string json = userDataToJSON(users);
            originalSize = json.length();
            dataToSend = userDataToBinary(users);
            
            std::cout << "JSON size: " << originalSize << " bytes" << std::endl;
            std::cout << "Binary size: " << dataToSend.size() << " bytes" << std::endl;
            break;
        }
        case 3: { // Compression
            std::cout << "\nUsing compression optimization..." << std::endl;
            std::string json = userDataToJSON(users);
            originalSize = json.length();
            std::vector<uint8_t> jsonBytes(json.begin(), json.end());
            dataToSend = simpleCompress(jsonBytes);
            
            std::cout << "Original size: " << originalSize << " bytes" << std::endl;
            std::cout << "Compressed size: " << dataToSend.size() << " bytes" << std::endl;
            break;
        }
        case 4: { // All optimizations
            std::cout << "\nUsing ALL optimizations..." << std::endl;
            std::string json = userDataToJSON(users);
            originalSize = json.length();
            
            DeduplicationManager dedup;
            std::vector<uint8_t> binary = userDataToOptimizedBinary(users, dedup);
            
            // Add dictionary
            std::vector<uint8_t> dictionary;
            for (size_t i = 0; i < dedup.getDictionarySize(); i++) {
                std::string str = dedup.getString(static_cast<int>(i + 1));
                uint32_t len = static_cast<uint32_t>(str.length());
                dictionary.insert(dictionary.end(), (uint8_t*)&len, (uint8_t*)&len + 4);
                dictionary.insert(dictionary.end(), str.begin(), str.end());
            }
            
            // Combine and compress
            std::vector<uint8_t> combined;
            uint32_t dictSize = static_cast<uint32_t>(dictionary.size());
            combined.insert(combined.end(), (uint8_t*)&dictSize, (uint8_t*)&dictSize + 4);
            combined.insert(combined.end(), dictionary.begin(), dictionary.end());
            combined.insert(combined.end(), binary.begin(), binary.end());
            
            dataToSend = simpleCompress(combined);
            
            std::cout << "Original JSON size: " << originalSize << " bytes" << std::endl;
            std::cout << "Fully optimized size: " << dataToSend.size() << " bytes" << std::endl;
            std::cout << "Dictionary entries: " << dedup.getDictionarySize() << std::endl;
            break;
        }
    }

    // Connect to server and send data
    int clientSocket = createClientSocket();
    if (clientSocket < 0) return;

    std::cout << "\nConnected to server. Sending data..." << std::endl;

    // Send data in chunks
    size_t totalSent = 0;
    size_t chunkSize = 4096;  // 4KB chunks

    while (totalSent < dataToSend.size()) {
        size_t remaining = dataToSend.size() - totalSent;
        size_t currentChunk = (chunkSize < remaining) ? chunkSize : remaining;

        int bytesSent = send(clientSocket, (char*)dataToSend.data() + totalSent,
            static_cast<int>(currentChunk), 0);

        if (bytesSent < 0) {
            std::cerr << "Error sending data" << std::endl;
            break;
        }

        totalSent += bytesSent;
        std::cout << "Sent " << bytesSent << " bytes (total: " << totalSent << " bytes)" << std::endl;
    }

    std::cout << "\n=== TRANSMISSION COMPLETE ===" << std::endl;
    std::cout << "Total bytes sent: " << totalSent << std::endl;
    std::cout << "Data size: " << (totalSent / 1024.0) << " KB" << std::endl;

    if (originalSize > 0) {
        double reduction = (1.0 - (double)totalSent / originalSize) * 100.0;
        std::cout << "Size reduction: " << reduction << "%" << std::endl;
        std::cout << "Bytes saved: " << (originalSize - totalSent) << " bytes" << std::endl;
    }

    close(clientSocket);
}

// =====================================================================================
// MAIN PROGRAM
// =====================================================================================

int main() {
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    DATA OPTIMIZATION DEMONSTRATION" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "This program demonstrates various data optimization techniques to reduce" << std::endl;
    std::cout << "bandwidth usage and improve application performance." << std::endl;
    std::cout << std::endl;
    std::cout << "EDUCATIONAL OBJECTIVES:" << std::endl;
    std::cout << "- Show deduplication techniques to avoid redundant data" << std::endl;
    std::cout << "- Demonstrate compact data formats (JSON vs Binary)" << std::endl;
    std::cout << "- Illustrate payload optimization and header minimization" << std::endl;
    std::cout << "- Compare different compression algorithms" << std::endl;
    std::cout << "- Show benefits: lower bandwidth, faster loading, energy savings" << std::endl;
    std::cout << std::endl;
    std::cout << "CURRENT MODE: " << OPTIMIZATION_MODE << std::endl;
    std::cout << "To change mode, modify OPTIMIZATION_MODE variable at line 57" << std::endl;
    std::cout << "  - Mode 0: No optimization (JSON baseline)" << std::endl;
    std::cout << "  - Mode 1: Deduplication (eliminate redundant data)" << std::endl;
    std::cout << "  - Mode 2: Binary format (more efficient than text)" << std::endl;
    std::cout << "  - Mode 3: Compression (reduce data size)" << std::endl;
    std::cout << "  - Mode 4: All optimizations combined" << std::endl;
    std::cout << std::endl;
    std::cout << "CONTROLS:" << std::endl;
    std::cout << "- Press ENTER to send a package" << std::endl;
    std::cout << "- Type 'quit' and press ENTER to exit" << std::endl;
    std::cout << "- Type 'mode' and press ENTER to cycle through optimization modes" << std::endl;
    std::cout << std::endl;
    std::cout << "WIRESHARK MONITORING:" << std::endl;
    std::cout << "- Monitor loopback interface (127.0.0.1)" << std::endl;
    std::cout << "- Filter: tcp.port == " << SERVER_PORT << std::endl;
    std::cout << "- Compare packet sizes between different optimization modes" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    initializeNetwork();

    // Start server in a separate thread
    std::thread serverThread(runServer);

    // Give server time to start
    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    std::string userInput;
    int packageCount = 0;

    while (true) {
        std::cout << "\n>>> Press ENTER to send package #" << (packageCount + 1) 
                  << " (or type 'quit' to exit, 'mode' to cycle): ";
        std::getline(std::cin, userInput);

        if (userInput == "quit" || userInput == "exit") {
            std::cout << "Exiting demonstration..." << std::endl;
            break;
        }
        else if (userInput == "mode") {
            OPTIMIZATION_MODE = (OPTIMIZATION_MODE + 1) % 5;
            std::cout << "Mode changed to: " << OPTIMIZATION_MODE << std::endl;
            std::cout << "  - Mode 0: No optimization" << std::endl;
            std::cout << "  - Mode 1: Deduplication" << std::endl;
            std::cout << "  - Mode 2: Binary format" << std::endl;
            std::cout << "  - Mode 3: Compression" << std::endl;
            std::cout << "  - Mode 4: All optimizations" << std::endl;
            continue;
        }
        else if (userInput.empty() || userInput == "send") {
            packageCount++;
            std::cout << "\n--- SENDING PACKAGE #" << packageCount << " ---" << std::endl;
            runClient();
            std::cout << "--- PACKAGE #" << packageCount << " COMPLETE ---" << std::endl;
        }
        else {
            std::cout << "Invalid command. Use ENTER to send, 'quit' to exit, or 'mode' to cycle." << std::endl;
        }
    }

    // Wait for server to finish
    serverThread.join();

    cleanupNetwork();

    std::cout << "\n=====================================================================================" << std::endl;
    std::cout << "DEMONSTRATION COMPLETE" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "SUMMARY:" << std::endl;
    std::cout << "Total packages sent: " << packageCount << std::endl;
    std::cout << "Final mode: " << OPTIMIZATION_MODE << std::endl;
    std::cout << std::endl;
    std::cout << "KEY LEARNINGS:" << std::endl;
    std::cout << "- Deduplication eliminates redundant data" << std::endl;
    std::cout << "- Binary formats are more efficient than text formats" << std::endl;
    std::cout << "- Compression can significantly reduce data size" << std::endl;
    std::cout << "- Multiple optimization techniques can be combined" << std::endl;
    std::cout << "- Optimized data reduces bandwidth usage and improves performance" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    return 0;
}
