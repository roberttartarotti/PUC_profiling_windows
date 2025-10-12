/*
 * =====================================================================================
 * HTTP HEADER OPTIMIZATION DEMONSTRATION - C++ (MODULE 3, CLASS 4 - EXAMPLE 3)
 * =====================================================================================
 *
 * Purpose: Demonstrate HTTP header optimization techniques to reduce overhead
 *          and improve network efficiency
 *
 * Educational Context:
 * - Show how reducing header size minimizes extra data transmitted
 * - Demonstrate header compression (simulated HPACK-like algorithm)
 * - Illustrate removal of unnecessary headers to avoid overhead
 * - Compare HTTP/1.1 vs HTTP/2-style header compression
 * - Show conditional responses and caching techniques
 *
 * What this demonstrates:
 * - Headers add significant overhead to HTTP requests
 * - Header compression (HPACK) can reduce header size by 80%+
 * - Removing unnecessary headers improves efficiency
 * - Caching reduces repeated requests
 * - Header optimization accelerates request/response cycles
 *
 * Usage:
 * - Compile: g++ -o header_optimization_demo example3-m3p4e3-header-optimization-demo.cpp
 * - Run: ./header_optimization_demo
 * - Monitor with Wireshark on loopback interface (127.0.0.1)
 * - Toggle optimization mode with HEADER_MODE variable
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
#include <sstream>

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

// Header optimization modes:
// 0 = Full headers (HTTP/1.1 style with all headers)
// 1 = Minimal headers (remove unnecessary headers)
// 2 = Compressed headers (HPACK-like compression)
// 3 = Cached response (304 Not Modified)
int HEADER_MODE = 0;  // Set to 0-3 to test different optimization levels

const int SERVER_PORT = 8888;
const int BUFFER_SIZE = 65536;  // 64KB buffer

// =====================================================================================
// HTTP HEADER STRUCTURES
// =====================================================================================

struct HttpRequest {
    std::string method;
    std::string path;
    std::string version;
    std::map<std::string, std::string> headers;
    std::string body;
};

struct HttpResponse {
    int statusCode;
    std::string statusMessage;
    std::map<std::string, std::string> headers;
    std::string body;
};

// =====================================================================================
// HEADER COMPRESSION (HPACK-LIKE SIMULATION)
// =====================================================================================

class HeaderCompressor {
private:
    // Static table (common headers)
    std::map<int, std::pair<std::string, std::string>> staticTable;
    // Dynamic table (recently used headers)
    std::vector<std::pair<std::string, std::string>> dynamicTable;
    int nextIndex;

public:
    HeaderCompressor() : nextIndex(62) {
        // Initialize static table with common HTTP headers (simplified HPACK)
        staticTable[1] = {":authority", ""};
        staticTable[2] = {":method", "GET"};
        staticTable[3] = {":method", "POST"};
        staticTable[4] = {":path", "/"};
        staticTable[5] = {":scheme", "http"};
        staticTable[6] = {":scheme", "https"};
        staticTable[7] = {":status", "200"};
        staticTable[8] = {":status", "204"};
        staticTable[9] = {":status", "206"};
        staticTable[10] = {":status", "304"};
        staticTable[11] = {":status", "400"};
        staticTable[12] = {":status", "404"};
        staticTable[13] = {":status", "500"};
        staticTable[14] = {"accept-charset", ""};
        staticTable[15] = {"accept-encoding", "gzip, deflate"};
        staticTable[16] = {"accept-language", ""};
        staticTable[17] = {"accept-ranges", ""};
        staticTable[18] = {"accept", ""};
        staticTable[19] = {"access-control-allow-origin", ""};
        staticTable[20] = {"age", ""};
        staticTable[21] = {"allow", ""};
        staticTable[22] = {"authorization", ""};
        staticTable[23] = {"cache-control", ""};
        staticTable[24] = {"content-disposition", ""};
        staticTable[25] = {"content-encoding", ""};
        staticTable[26] = {"content-language", ""};
        staticTable[27] = {"content-length", ""};
        staticTable[28] = {"content-location", ""};
        staticTable[29] = {"content-range", ""};
        staticTable[30] = {"content-type", ""};
        staticTable[31] = {"cookie", ""};
        staticTable[32] = {"date", ""};
        staticTable[33] = {"etag", ""};
        staticTable[34] = {"expect", ""};
        staticTable[35] = {"expires", ""};
        staticTable[36] = {"from", ""};
        staticTable[37] = {"host", ""};
        staticTable[38] = {"if-match", ""};
        staticTable[39] = {"if-modified-since", ""};
        staticTable[40] = {"if-none-match", ""};
        staticTable[41] = {"if-range", ""};
        staticTable[42] = {"if-unmodified-since", ""};
        staticTable[43] = {"last-modified", ""};
        staticTable[44] = {"link", ""};
        staticTable[45] = {"location", ""};
        staticTable[46] = {"max-forwards", ""};
        staticTable[47] = {"proxy-authenticate", ""};
        staticTable[48] = {"proxy-authorization", ""};
        staticTable[49] = {"range", ""};
        staticTable[50] = {"referer", ""};
        staticTable[51] = {"refresh", ""};
        staticTable[52] = {"retry-after", ""};
        staticTable[53] = {"server", ""};
        staticTable[54] = {"set-cookie", ""};
        staticTable[55] = {"strict-transport-security", ""};
        staticTable[56] = {"transfer-encoding", ""};
        staticTable[57] = {"user-agent", ""};
        staticTable[58] = {"vary", ""};
        staticTable[59] = {"via", ""};
        staticTable[60] = {"www-authenticate", ""};
    }

    std::vector<uint8_t> compressHeaders(const std::map<std::string, std::string>& headers) {
        std::vector<uint8_t> compressed;
        
        for (const auto& header : headers) {
            // Check if header is in static table
            int staticIndex = findInStaticTable(header.first, header.second);
            
            if (staticIndex > 0) {
                // Indexed header field (1 byte for common headers)
                compressed.push_back(0x80 | staticIndex);
            } else {
                // Literal header field with incremental indexing
                compressed.push_back(0x40);  // Literal with indexing prefix
                
                // Encode header name length
                uint8_t nameLen = static_cast<uint8_t>(header.first.length());
                compressed.push_back(nameLen);
                
                // Encode header name
                compressed.insert(compressed.end(), header.first.begin(), header.first.end());
                
                // Encode header value length
                uint8_t valueLen = static_cast<uint8_t>(header.second.length());
                compressed.push_back(valueLen);
                
                // Encode header value
                compressed.insert(compressed.end(), header.second.begin(), header.second.end());
                
                // Add to dynamic table
                dynamicTable.push_back({header.first, header.second});
            }
        }
        
        return compressed;
    }

    int findInStaticTable(const std::string& name, const std::string& value) {
        for (const auto& entry : staticTable) {
            if (entry.second.first == name) {
                if (entry.second.second.empty() || entry.second.second == value) {
                    return entry.first;
                }
            }
        }
        return -1;
    }
};

// =====================================================================================
// HTTP MESSAGE BUILDERS
// =====================================================================================

std::string buildFullHttpRequest() {
    std::ostringstream request;
    
    // Request line
    request << "GET /api/users HTTP/1.1\r\n";
    
    // Full headers (typical browser request)
    request << "Host: localhost:8890\r\n";
    request << "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36\r\n";
    request << "Accept: application/json, text/plain, */*\r\n";
    request << "Accept-Language: en-US,en;q=0.9,pt-BR;q=0.8,pt;q=0.7\r\n";
    request << "Accept-Encoding: gzip, deflate, br\r\n";
    request << "Connection: keep-alive\r\n";
    request << "Cache-Control: no-cache\r\n";
    request << "Pragma: no-cache\r\n";
    request << "Sec-Fetch-Dest: empty\r\n";
    request << "Sec-Fetch-Mode: cors\r\n";
    request << "Sec-Fetch-Site: same-origin\r\n";
    request << "Referer: http://localhost:8890/dashboard\r\n";
    request << "Cookie: session_id=abc123def456; user_pref=dark_mode; analytics_id=xyz789\r\n";
    request << "X-Requested-With: XMLHttpRequest\r\n";
    request << "X-Client-Version: 1.2.3\r\n";
    request << "X-Request-ID: 550e8400-e29b-41d4-a716-446655440000\r\n";
    request << "\r\n";
    
    return request.str();
}

std::string buildMinimalHttpRequest() {
    std::ostringstream request;
    
    // Request line
    request << "GET /api/users HTTP/1.1\r\n";
    
    // Minimal headers (only essential)
    request << "Host: localhost:8890\r\n";
    request << "Accept: application/json\r\n";
    request << "Connection: keep-alive\r\n";
    request << "\r\n";
    
    return request.str();
}

std::string buildFullHttpResponse() {
    std::ostringstream response;
    
    // Status line
    response << "HTTP/1.1 200 OK\r\n";
    
    // Full headers
    response << "Date: Mon, 27 Jan 2025 12:00:00 GMT\r\n";
    response << "Server: Apache/2.4.41 (Ubuntu)\r\n";
    response << "Content-Type: application/json; charset=utf-8\r\n";
    response << "Content-Length: 150\r\n";
    response << "Connection: keep-alive\r\n";
    response << "Cache-Control: max-age=3600, public\r\n";
    response << "ETag: \"33a64df551425fcc55e4d42a148795d9f25f89d4\"\r\n";
    response << "Last-Modified: Mon, 27 Jan 2025 11:00:00 GMT\r\n";
    response << "Vary: Accept-Encoding\r\n";
    response << "X-Content-Type-Options: nosniff\r\n";
    response << "X-Frame-Options: DENY\r\n";
    response << "X-XSS-Protection: 1; mode=block\r\n";
    response << "Strict-Transport-Security: max-age=31536000; includeSubDomains\r\n";
    response << "Access-Control-Allow-Origin: *\r\n";
    response << "X-Response-Time: 45ms\r\n";
    response << "X-Request-ID: 550e8400-e29b-41d4-a716-446655440000\r\n";
    response << "\r\n";
    
    // Body
    response << "{\"users\":[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}],\"total\":2,\"page\":1}";
    
    return response.str();
}

std::string buildMinimalHttpResponse() {
    std::ostringstream response;
    
    // Status line
    response << "HTTP/1.1 200 OK\r\n";
    
    // Minimal headers
    response << "Content-Type: application/json\r\n";
    response << "Content-Length: 150\r\n";
    response << "\r\n";
    
    // Body
    response << "{\"users\":[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}],\"total\":2,\"page\":1}";
    
    return response.str();
}

std::string buildCachedResponse() {
    std::ostringstream response;
    
    // Status line (304 Not Modified)
    response << "HTTP/1.1 304 Not Modified\r\n";
    
    // Minimal headers for cached response
    response << "Date: Mon, 27 Jan 2025 12:00:00 GMT\r\n";
    response << "ETag: \"33a64df551425fcc55e4d42a148795d9f25f89d4\"\r\n";
    response << "Cache-Control: max-age=3600, public\r\n";
    response << "\r\n";
    
    // No body (client uses cached version)
    
    return response.str();
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
    std::cout << "\n=== HTTP HEADER OPTIMIZATION DEMO SERVER ===" << std::endl;
    std::cout << "Mode: " << HEADER_MODE << " (0=Full, 1=Minimal, 2=Compressed, 3=Cached)" << std::endl;
    std::cout << "Listening on port: " << SERVER_PORT << std::endl;
    std::cout << "Monitor with Wireshark on 127.0.0.1:" << SERVER_PORT << std::endl;
    std::cout << "============================================" << std::endl;

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

    // Receive request from client
    std::vector<uint8_t> buffer(BUFFER_SIZE);
    int bytesReceived = recv(clientSocket, (char*)buffer.data(), BUFFER_SIZE, 0);

    if (bytesReceived > 0) {
        std::cout << "\n=== REQUEST RECEIVED ===" << std::endl;
        std::cout << "Request size: " << bytesReceived << " bytes" << std::endl;
        
        // Send response based on mode
        std::string response;
        
        switch (HEADER_MODE) {
            case 0:
                response = buildFullHttpResponse();
                std::cout << "Sending: Full HTTP response" << std::endl;
                break;
            case 1:
                response = buildMinimalHttpResponse();
                std::cout << "Sending: Minimal HTTP response" << std::endl;
                break;
            case 2: {
                // Compressed headers
                HeaderCompressor compressor;
                std::map<std::string, std::string> headers;
                headers["content-type"] = "application/json";
                headers["content-length"] = "150";
                std::vector<uint8_t> compressed = compressor.compressHeaders(headers);
                
                // Build response with compressed headers
                std::ostringstream resp;
                resp << "HTTP/2 200\r\n";
                resp << "[COMPRESSED HEADERS: " << compressed.size() << " bytes]\r\n";
                resp << "\r\n";
                resp << "{\"users\":[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}],\"total\":2,\"page\":1}";
                response = resp.str();
                std::cout << "Sending: Compressed headers response (HTTP/2 style)" << std::endl;
                break;
            }
            case 3:
                response = buildCachedResponse();
                std::cout << "Sending: Cached response (304 Not Modified)" << std::endl;
                break;
        }
        
        send(clientSocket, response.c_str(), static_cast<int>(response.length()), 0);
        
        std::cout << "Response size: " << response.length() << " bytes" << std::endl;
    }

    close(clientSocket);
    close(serverSocket);
}

// =====================================================================================
// CLIENT IMPLEMENTATION
// =====================================================================================

void runClient() {
    std::cout << "\n=== HTTP HEADER OPTIMIZATION DEMO CLIENT ===" << std::endl;
    std::cout << "Mode: " << HEADER_MODE << " (0=Full, 1=Minimal, 2=Compressed, 3=Cached)" << std::endl;
    std::cout << "Connecting to server on port: " << SERVER_PORT << std::endl;
    std::cout << "===========================================" << std::endl;

    // Build request based on mode
    std::string request;
    size_t headerSize = 0;
    
    switch (HEADER_MODE) {
        case 0:
            request = buildFullHttpRequest();
            std::cout << "\nSending: Full HTTP request (typical browser)" << std::endl;
            break;
        case 1:
            request = buildMinimalHttpRequest();
            std::cout << "\nSending: Minimal HTTP request (only essential headers)" << std::endl;
            break;
        case 2: {
            // Compressed headers
            HeaderCompressor compressor;
            std::map<std::string, std::string> headers;
            headers["host"] = "localhost:8890";
            headers["accept"] = "application/json";
            std::vector<uint8_t> compressed = compressor.compressHeaders(headers);
            
            std::ostringstream req;
            req << "GET /api/users HTTP/2\r\n";
            req << "[COMPRESSED HEADERS: " << compressed.size() << " bytes]\r\n";
            req << "\r\n";
            request = req.str();
            std::cout << "\nSending: Compressed headers request (HTTP/2 style)" << std::endl;
            break;
        }
        case 3: {
            // Conditional request with If-None-Match
            std::ostringstream req;
            req << "GET /api/users HTTP/1.1\r\n";
            req << "Host: localhost:8890\r\n";
            req << "If-None-Match: \"33a64df551425fcc55e4d42a148795d9f25f89d4\"\r\n";
            req << "If-Modified-Since: Mon, 27 Jan 2025 11:00:00 GMT\r\n";
            req << "\r\n";
            request = req.str();
            std::cout << "\nSending: Conditional request (with cache validators)" << std::endl;
            break;
        }
    }

    std::cout << "Request size: " << request.length() << " bytes" << std::endl;

    // Connect to server and send request
    int clientSocket = createClientSocket();
    if (clientSocket < 0) return;

    std::cout << "\nConnected to server. Sending request..." << std::endl;
    
    send(clientSocket, request.c_str(), static_cast<int>(request.length()), 0);
    
    std::cout << "Request sent: " << request.length() << " bytes" << std::endl;

    // Receive response
    std::vector<uint8_t> buffer(BUFFER_SIZE);
    int bytesReceived = recv(clientSocket, (char*)buffer.data(), BUFFER_SIZE, 0);

    if (bytesReceived > 0) {
        std::cout << "\n=== RESPONSE RECEIVED ===" << std::endl;
        std::cout << "Response size: " << bytesReceived << " bytes" << std::endl;
    }

    close(clientSocket);
    
    // Analysis
    std::cout << "\n=== HEADER OPTIMIZATION ANALYSIS ===" << std::endl;
    switch (HEADER_MODE) {
        case 0:
            std::cout << "Mode: FULL HEADERS (HTTP/1.1)" << std::endl;
            std::cout << "- Typical browser request with all headers" << std::endl;
            std::cout << "- High overhead from verbose headers" << std::endl;
            std::cout << "- Baseline for comparison" << std::endl;
            break;
        case 1:
            std::cout << "Mode: MINIMAL HEADERS" << std::endl;
            std::cout << "- Only essential headers included" << std::endl;
            std::cout << "- Removed unnecessary headers" << std::endl;
            std::cout << "- Reduced overhead significantly" << std::endl;
            break;
        case 2:
            std::cout << "Mode: COMPRESSED HEADERS (HTTP/2 HPACK)" << std::endl;
            std::cout << "- Headers compressed using HPACK-like algorithm" << std::endl;
            std::cout << "- Static table for common headers" << std::endl;
            std::cout << "- 80%+ reduction in header size" << std::endl;
            break;
        case 3:
            std::cout << "Mode: CACHED RESPONSE (304 Not Modified)" << std::endl;
            std::cout << "- Conditional request with cache validators" << std::endl;
            std::cout << "- Server returns 304 without body" << std::endl;
            std::cout << "- Client uses cached version" << std::endl;
            std::cout << "- Massive bandwidth savings" << std::endl;
            break;
    }
}

// =====================================================================================
// MAIN PROGRAM
// =====================================================================================

int main() {
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    HTTP HEADER OPTIMIZATION DEMONSTRATION" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "This program demonstrates HTTP header optimization techniques to reduce" << std::endl;
    std::cout << "overhead and improve network efficiency." << std::endl;
    std::cout << std::endl;
    std::cout << "EDUCATIONAL OBJECTIVES:" << std::endl;
    std::cout << "- Show how reducing header size minimizes extra data transmitted" << std::endl;
    std::cout << "- Demonstrate header compression (HPACK-like algorithm)" << std::endl;
    std::cout << "- Illustrate removal of unnecessary headers to avoid overhead" << std::endl;
    std::cout << "- Compare HTTP/1.1 vs HTTP/2-style header compression" << std::endl;
    std::cout << "- Show conditional responses and caching techniques" << std::endl;
    std::cout << std::endl;
    std::cout << "CURRENT MODE: " << HEADER_MODE << std::endl;
    std::cout << "To change mode, modify HEADER_MODE variable at line 62" << std::endl;
    std::cout << "  - Mode 0: Full headers (HTTP/1.1 with all headers)" << std::endl;
    std::cout << "  - Mode 1: Minimal headers (remove unnecessary)" << std::endl;
    std::cout << "  - Mode 2: Compressed headers (HPACK-like)" << std::endl;
    std::cout << "  - Mode 3: Cached response (304 Not Modified)" << std::endl;
    std::cout << std::endl;
    std::cout << "CONTROLS:" << std::endl;
    std::cout << "- Press ENTER to send a request" << std::endl;
    std::cout << "- Type 'quit' and press ENTER to exit" << std::endl;
    std::cout << "- Type 'mode' and press ENTER to cycle through optimization modes" << std::endl;
    std::cout << std::endl;
    std::cout << "WIRESHARK MONITORING:" << std::endl;
    std::cout << "- Monitor loopback interface (127.0.0.1)" << std::endl;
    std::cout << "- Filter: tcp.port == " << SERVER_PORT << std::endl;
    std::cout << "- Compare packet sizes between different header optimization modes" << std::endl;
    std::cout << "- Analyze header overhead in each mode" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    initializeNetwork();

    // Start server in a separate thread
    std::thread serverThread(runServer);

    // Give server time to start
    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    std::string userInput;
    int requestCount = 0;

    while (true) {
        std::cout << "\n>>> Press ENTER to send request #" << (requestCount + 1) 
                  << " (or type 'quit' to exit, 'mode' to cycle): ";
        std::getline(std::cin, userInput);

        if (userInput == "quit" || userInput == "exit") {
            std::cout << "Exiting demonstration..." << std::endl;
            break;
        }
        else if (userInput == "mode") {
            HEADER_MODE = (HEADER_MODE + 1) % 4;
            std::cout << "Mode changed to: " << HEADER_MODE << std::endl;
            std::cout << "  - Mode 0: Full headers" << std::endl;
            std::cout << "  - Mode 1: Minimal headers" << std::endl;
            std::cout << "  - Mode 2: Compressed headers" << std::endl;
            std::cout << "  - Mode 3: Cached response" << std::endl;
            continue;
        }
        else if (userInput.empty() || userInput == "send") {
            requestCount++;
            std::cout << "\n--- SENDING REQUEST #" << requestCount << " ---" << std::endl;
            runClient();
            std::cout << "--- REQUEST #" << requestCount << " COMPLETE ---" << std::endl;
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
    std::cout << "Total requests sent: " << requestCount << std::endl;
    std::cout << "Final mode: " << HEADER_MODE << std::endl;
    std::cout << std::endl;
    std::cout << "KEY LEARNINGS:" << std::endl;
    std::cout << "- HTTP headers add significant overhead to requests/responses" << std::endl;
    std::cout << "- Removing unnecessary headers reduces bandwidth usage" << std::endl;
    std::cout << "- Header compression (HPACK) can reduce header size by 80%+" << std::endl;
    std::cout << "- Caching with conditional requests eliminates redundant data transfer" << std::endl;
    std::cout << "- HTTP/2 header compression is much more efficient than HTTP/1.1" << std::endl;
    std::cout << "- Header optimization accelerates request/response cycles" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    return 0;
}
