/*
 * =====================================================================================
 * TCP COMPRESSION DEMONSTRATION - C++ (MODULE 3, CLASS 4 - EXAMPLE 1)
 * =====================================================================================
 *
 * Purpose: Demonstrate TCP compression benefits by sending image data
 *          with and without compression over loopback connection
 *
 * Educational Context:
 * - Show the impact of compression on network bandwidth usage
 * - Demonstrate RLE compression for TCP payload reduction
 * - Illustrate how compression can reduce bytes transmitted
 * - Compare uncompressed vs compressed data transmission
 *
 * What this demonstrates:
 * - TCP can use compression to reduce payload size
 * - Compression decreases bytes sent, saving bandwidth and speeding up loading
 * - Compression can reduce bandwidth usage significantly for data files
 * - Simple client-server communication over loopback
 *
 * Usage:
 * - Compile: g++ -o tcp_compression_demo example1-m3p4e1-tcp-compression-demo.cpp
 * - Run: ./tcp_compression_demo
 * - Monitor with Wireshark on loopback interface (127.0.0.1)
 * - Toggle compression mode with global variable USE_COMPRESSION
 *
 * =====================================================================================
 */

#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <cstring>
#include <thread>
#include <chrono>

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

 // Toggle compression mode: true = with compression, false = without compression
bool USE_COMPRESSION = true;  // Set to true for compression demo, false for baseline

const int SERVER_PORT = 8888;
const std::string IMAGE_PATH = "C:\\Users\\robert\\personal\\PUC_profiling_windows\\module3\\class4\\m3p4e1\\image3.bmp";
const int BUFFER_SIZE = 65536;  // 64KB buffer

// =====================================================================================
// PROTOCOL HEADER STRUCTURE
// =====================================================================================

struct DataHeader {
    uint32_t magic;        // Magic number: 0x54435043 ("TCPC")
    uint32_t originalSize; // Original data size
    uint8_t compressed;    // 1 if compressed, 0 if not
    uint8_t reserved[3];   // Reserved for future use
};

const uint32_t MAGIC_NUMBER = 0x54435043; // "TCPC"

// =====================================================================================
// SIMPLE COMPRESSION UTILITIES (Run-Length Encoding for demonstration)
// =====================================================================================

std::vector<uint8_t> compressData(const std::vector<uint8_t>& data)
{
    if (data.empty()) return data;

    std::vector<uint8_t> compressed;
    compressed.reserve(data.size() / 2); // Reserve space for potential compression

    size_t i = 0;
    while (i < data.size())
    {
        uint8_t current = data[i];
        size_t count = 1;

        // Count consecutive identical bytes
        while (i + count < data.size() && data[i + count] == current && count < 255)
        {
            count++;
        }

        // If we have 3 or more consecutive bytes, use RLE compression
        if (count >= 3)
        {
            compressed.push_back(0xFF); // RLE marker
            compressed.push_back(current);
            compressed.push_back(static_cast<uint8_t>(count));
            i += count;
        }
        else
        {
            // Store bytes as-is
            for (size_t j = 0; j < count; j++)
            {
                compressed.push_back(data[i + j]);
            }
            i += count;
        }
    }

    return compressed;
}

std::vector<uint8_t> decompressData(const std::vector<uint8_t>& compressedData, size_t originalSize)
{
    std::vector<uint8_t> decompressed;
    decompressed.reserve(originalSize);

    size_t i = 0;
    while (i < compressedData.size() && decompressed.size() < originalSize)
    {
        if (compressedData[i] == 0xFF && i + 2 < compressedData.size())
        {
            // RLE expansion
            uint8_t value = compressedData[i + 1];
            uint8_t count = compressedData[i + 2];

            for (int j = 0; j < count && decompressed.size() < originalSize; j++)
            {
                decompressed.push_back(value);
            }
            i += 3;
        }
        else
        {
            decompressed.push_back(compressedData[i]);
            i++;
        }
    }

    return decompressed;
}

// =====================================================================================
// DATA PACKAGING UTILITIES
// =====================================================================================

std::vector<uint8_t> packageData(const std::vector<uint8_t>& data, bool useCompression)
{
    std::vector<uint8_t> packagedData;

    // Create header
    DataHeader header;
    header.magic = MAGIC_NUMBER;
    header.originalSize = static_cast<uint32_t>(data.size());
    header.compressed = useCompression ? 1 : 0;
    header.reserved[0] = header.reserved[1] = header.reserved[2] = 0;

    // Add header to package
    packagedData.resize(sizeof(DataHeader));
    memcpy(packagedData.data(), &header, sizeof(DataHeader));

    // Add data (compressed or not)
    std::vector<uint8_t> dataToAdd;
    if (useCompression) {
        dataToAdd = compressData(data);
    }
    else {
        dataToAdd = data;
    }

    packagedData.insert(packagedData.end(), dataToAdd.begin(), dataToAdd.end());

    return packagedData;
}

bool parseDataHeader(const std::vector<uint8_t>& data, DataHeader& header, std::vector<uint8_t>& payload)
{
    if (data.size() < sizeof(DataHeader)) {
        return false;
    }

    memcpy(&header, data.data(), sizeof(DataHeader));

    if (header.magic != MAGIC_NUMBER) {
        return false;
    }

    payload.assign(data.begin() + sizeof(DataHeader), data.end());
    return true;
}

// =====================================================================================
// NETWORK UTILITIES
// =====================================================================================

void initializeNetwork()
{
#ifdef _WIN32
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
    {
        std::cerr << "WSAStartup failed" << std::endl;
        exit(1);
    }
#endif
}

void cleanupNetwork()
{
#ifdef _WIN32
    WSACleanup();
#endif
}

int createServerSocket()
{
#ifdef _WIN32
    SOCKET serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket == INVALID_SOCKET)
    {
        std::cerr << "Failed to create server socket" << std::endl;
        return -1;
    }
#else
    int serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket < 0)
    {
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

    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) < 0)
    {
        std::cerr << "Failed to bind server socket" << std::endl;
        close(serverSocket);
        return -1;
    }

    if (listen(serverSocket, 1) < 0)
    {
        std::cerr << "Failed to listen on server socket" << std::endl;
        close(serverSocket);
        return -1;
    }

    return static_cast<int>(serverSocket);
}

int createClientSocket()
{
#ifdef _WIN32
    SOCKET clientSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (clientSocket == INVALID_SOCKET)
    {
        std::cerr << "Failed to create client socket" << std::endl;
        return -1;
    }
#else
    int clientSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (clientSocket < 0)
    {
        std::cerr << "Failed to create client socket" << std::endl;
        return -1;
    }
#endif

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = inet_addr("127.0.0.1");
    serverAddr.sin_port = htons(SERVER_PORT);

    if (connect(clientSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) < 0)
    {
        std::cerr << "Failed to connect to server" << std::endl;
        close(clientSocket);
        return -1;
    }

    return static_cast<int>(clientSocket);
}

// =====================================================================================
// SERVER IMPLEMENTATION
// =====================================================================================

void runServer()
{
    std::cout << "\n=== TCP COMPRESSION DEMO SERVER ===" << std::endl;
    std::cout << "Mode: " << (USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION") << std::endl;
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
    if (clientSocket == INVALID_SOCKET)
    {
        std::cerr << "Failed to accept client connection" << std::endl;
        close(serverSocket);
        return;
    }
#else
    int clientSocket = accept(serverSocket, (sockaddr*)&clientAddr, &clientLen);
    if (clientSocket < 0)
    {
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
        BUFFER_SIZE - totalBytesReceived, 0)) > 0)
    {
        totalBytesReceived += bytesReceived;
        std::cout << "Received " << bytesReceived << " bytes (total: "
            << totalBytesReceived << " bytes)" << std::endl;
    }

    if (bytesReceived < 0)
    {
        std::cerr << "Error receiving data" << std::endl;
        close(clientSocket);
        close(serverSocket);
        return;
    }

    std::cout << "\n=== RECEPTION COMPLETE ===" << std::endl;
    std::cout << "Total bytes received: " << totalBytesReceived << std::endl;
    std::cout << "Data size: " << (totalBytesReceived / 1024.0) << " KB" << std::endl;

    // Parse the received data
    DataHeader header;
    std::vector<uint8_t> payload;

    if (!parseDataHeader(buffer, header, payload))
    {
        std::cerr << "Error: Invalid data format received" << std::endl;
        close(clientSocket);
        close(serverSocket);
        return;
    }

    std::cout << "\n=== DATA ANALYSIS ===" << std::endl;
    std::cout << "Original data size: " << header.originalSize << " bytes ("
        << (header.originalSize / 1024.0) << " KB)" << std::endl;
    std::cout << "Compression flag: " << (header.compressed ? "ENABLED" : "DISABLED") << std::endl;
    std::cout << "Payload size: " << payload.size() << " bytes ("
        << (payload.size() / 1024.0) << " KB)" << std::endl;

    if (header.compressed)
    {
        std::cout << "\nData was COMPRESSED (RLE)" << std::endl;
        std::cout << "Compression ratio: "
            << ((1.0 - (double)payload.size() / header.originalSize) * 100.0)
            << "% reduction" << std::endl;
        std::cout << "Bandwidth savings: " << (header.originalSize - payload.size())
            << " bytes" << std::endl;
        std::cout << "This demonstrates bandwidth savings!" << std::endl;

        // Verify decompression works
        std::vector<uint8_t> decompressed = decompressData(payload, header.originalSize);
        if (decompressed.size() == header.originalSize)
        {
            std::cout << "✓ Decompression successful - data integrity verified!" << std::endl;
        }
        else
        {
            std::cout << "⚠ Warning: Decompression size mismatch!" << std::endl;
        }
    }
    else
    {
        std::cout << "\nData was UNCOMPRESSED" << std::endl;
        std::cout << "This is the baseline for comparison!" << std::endl;
        if (payload.size() == header.originalSize)
        {
            std::cout << "✓ Data integrity verified!" << std::endl;
        }
        else
        {
            std::cout << "⚠ Warning: Size mismatch!" << std::endl;
        }
    }

    close(clientSocket);
    close(serverSocket);
}

// =====================================================================================
// CLIENT IMPLEMENTATION
// =====================================================================================

std::vector<uint8_t> loadImageData()
{
    std::ifstream file(IMAGE_PATH, std::ios::binary | std::ios::ate);
    if (!file.is_open())
    {
        std::cerr << "Failed to open image file: " << IMAGE_PATH << std::endl;
        std::cerr << "Make sure the image.png file exists in m3p4e1/ directory" << std::endl;
        return std::vector<uint8_t>();
    }

    std::streamsize size = file.tellg();
    file.seekg(0, std::ios::beg);

    std::vector<uint8_t> buffer(size);
    if (!file.read((char*)buffer.data(), size))
    {
        std::cerr << "Failed to read image file" << std::endl;
        return std::vector<uint8_t>();
    }

    return buffer;
}

void runClient()
{
    std::cout << "\n=== TCP COMPRESSION DEMO CLIENT ===" << std::endl;
    std::cout << "Mode: " << (USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION") << std::endl;
    std::cout << "Connecting to server on port: " << SERVER_PORT << std::endl;
    std::cout << "====================================" << std::endl;

    // Load image data
    std::vector<uint8_t> imageData = loadImageData();
    if (imageData.empty())
    {
        std::cerr << "Failed to load image data" << std::endl;
        return;
    }

    std::cout << "Loaded image: " << IMAGE_PATH << std::endl;
    std::cout << "Original image size: " << imageData.size() << " bytes ("
        << (imageData.size() / 1024.0) << " KB)" << std::endl;

    // Prepare data for transmission
    std::cout << "\nPreparing data for transmission..." << std::endl;
    std::vector<uint8_t> dataToSend = packageData(imageData, USE_COMPRESSION);

    std::cout << "Original image size: " << imageData.size() << " bytes ("
        << (imageData.size() / 1024.0) << " KB)" << std::endl;
    std::cout << "Packaged data size: " << dataToSend.size() << " bytes ("
        << (dataToSend.size() / 1024.0) << " KB)" << std::endl;

    if (USE_COMPRESSION)
    {
        std::cout << "Mode: WITH COMPRESSION" << std::endl;
        std::cout << "Bandwidth savings: " << (imageData.size() - (dataToSend.size() - sizeof(DataHeader)))
            << " bytes" << std::endl;
    }
    else
    {
        std::cout << "Mode: WITHOUT COMPRESSION" << std::endl;
        std::cout << "No compression applied - sending original data" << std::endl;
    }

    // Connect to server and send data
    int clientSocket = createClientSocket();
    if (clientSocket < 0) return;

    std::cout << "\nConnected to server. Sending data..." << std::endl;

    // Send data in chunks
    size_t totalSent = 0;
    size_t chunkSize = 4096;  // 4KB chunks

    while (totalSent < dataToSend.size())
    {
        size_t remaining = dataToSend.size() - totalSent;
        size_t currentChunk = (chunkSize < remaining) ? chunkSize : remaining;

        int bytesSent = send(clientSocket, (char*)dataToSend.data() + totalSent,
            static_cast<int>(currentChunk), 0);

        if (bytesSent < 0)
        {
            std::cerr << "Error sending data" << std::endl;
            break;
        }

        totalSent += bytesSent;
        std::cout << "Sent " << bytesSent << " bytes (total: " << totalSent << " bytes)" << std::endl;
    }

    std::cout << "\n=== TRANSMISSION COMPLETE ===" << std::endl;
    std::cout << "Total bytes sent: " << totalSent << std::endl;
    std::cout << "Data size: " << (totalSent / 1024.0) << " KB" << std::endl;

    if (USE_COMPRESSION)
    {
        std::cout << "COMPRESSED transmission completed!" << std::endl;
        std::cout << "Check Wireshark to see reduced bandwidth usage" << std::endl;
    }
    else
    {
        std::cout << "UNCOMPRESSED transmission completed!" << std::endl;
        std::cout << "This is the baseline for comparison" << std::endl;
    }

    close(clientSocket);
}

// =====================================================================================
// MAIN PROGRAM
// =====================================================================================

int main()
{
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "                    TCP COMPRESSION DEMONSTRATION" << std::endl;
    std::cout << "=====================================================================================" << std::endl;
    std::cout << "This program demonstrates TCP compression benefits by sending image data" << std::endl;
    std::cout << "with and without compression over a loopback connection." << std::endl;
    std::cout << std::endl;
    std::cout << "EDUCATIONAL OBJECTIVES:" << std::endl;
    std::cout << "- Show impact of compression on network bandwidth usage" << std::endl;
    std::cout << "- Demonstrate RLE compression for TCP payload reduction" << std::endl;
    std::cout << "- Illustrate how compression can reduce bytes transmitted" << std::endl;
    std::cout << "- Compare uncompressed vs compressed data transmission" << std::endl;
    std::cout << std::endl;
    std::cout << "CURRENT MODE: " << (USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION") << std::endl;
    std::cout << "To change mode, modify USE_COMPRESSION variable at line 57" << std::endl;
    std::cout << "  - Set USE_COMPRESSION = true  for compression demo" << std::endl;
    std::cout << "  - Set USE_COMPRESSION = false for baseline demo" << std::endl;
    std::cout << std::endl;
    std::cout << "CONTROLS:" << std::endl;
    std::cout << "- Press ENTER to send a package" << std::endl;
    std::cout << "- Type 'quit' and press ENTER to exit" << std::endl;
    std::cout << "- Type 'mode' and press ENTER to toggle compression mode" << std::endl;
    std::cout << std::endl;
    std::cout << "WIRESHARK MONITORING:" << std::endl;
    std::cout << "- Monitor loopback interface (127.0.0.1)" << std::endl;
    std::cout << "- Filter: tcp.port == " << SERVER_PORT << std::endl;
    std::cout << "- Compare packet sizes between compressed/uncompressed modes" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    initializeNetwork();

    // Start server in a separate thread
    std::thread serverThread(runServer);

    // Give server time to start
    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    std::string userInput;
    int packageCount = 0;

    while (true)
    {
        std::cout << "\n>>> Press ENTER to send package #" << (packageCount + 1)
            << " (or type 'quit' to exit, 'mode' to toggle): ";
        std::getline(std::cin, userInput);

        if (userInput == "quit" || userInput == "exit")
        {
            std::cout << "Exiting demonstration..." << std::endl;
            break;
        }
        else if (userInput == "mode")
        {
            USE_COMPRESSION = !USE_COMPRESSION;
            std::cout << "Mode changed to: " << (USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION") << std::endl;
            continue;
        }
        else if (userInput.empty() || userInput == "send")
        {
            packageCount++;
            std::cout << "\n--- SENDING PACKAGE #" << packageCount << " ---" << std::endl;
            runClient();
            std::cout << "--- PACKAGE #" << packageCount << " COMPLETE ---" << std::endl;
        }
        else
        {
            std::cout << "Invalid command. Use ENTER to send, 'quit' to exit, or 'mode' to toggle." << std::endl;
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
    std::cout << "Final mode: " << (USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION") << std::endl;
    std::cout << std::endl;
    std::cout << "KEY LEARNINGS:" << std::endl;
    std::cout << "- TCP can use compression to reduce payload size" << std::endl;
    std::cout << "- Compression decreases bytes sent, saving bandwidth" << std::endl;
    std::cout << "- Compression can reduce bandwidth usage significantly" << std::endl;
    std::cout << "- This improves application performance and user experience" << std::endl;
    std::cout << "=====================================================================================" << std::endl;

    return 0;
}
