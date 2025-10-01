#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <chrono>
#include <thread>
#include <iomanip>
#include <random>
#include <sstream>
#include <conio.h>  // For _kbhit() on Windows

// ====================================================================
// CONFIGURATION VARIABLES - EASY TO MODIFY FOR DIFFERENT SCENARIOS
// ====================================================================
const size_t MIN_FILE_SIZE_KB = 100;        // Minimum file size in KB
const size_t MAX_FILE_SIZE_KB = 500;        // Maximum file size in KB
const size_t WRITE_CHUNK_SIZE = 4096;       // Write chunk size in bytes (4KB)
const size_t READ_BUFFER_SIZE = 1024;       // Read buffer size in bytes (1KB)
const int READ_REPETITIONS = 3;             // How many times to read each file
const int STATISTICS_INTERVAL = 10;         // Show statistics every N cycles
const int CYCLE_DELAY_MS = 500;             // Delay between cycles in milliseconds
const int WRITE_DELAY_MICROSECONDS = 100;   // Delay between write operations
const int READ_DELAY_MICROSECONDS = 50;     // Delay between read operations
const std::string BASE_FILENAME = "intensive_io_file_";  // Base name for temp files
// ====================================================================

class IntensiveIODemonstration {
private:
    std::string baseFileName;
    int fileCounter;
    size_t totalBytesWritten;
    size_t totalBytesRead;
    int totalOperations;
    std::chrono::high_resolution_clock::time_point startTime;
    
    // Generate large dummy data
    std::string generateLargeContent(size_t sizeKB) {
        std::stringstream ss;
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> dis(65, 90); // A-Z characters
        
        size_t totalChars = sizeKB * 1024;
        ss << "=== INTENSIVE I/O DEMONSTRATION DATA ===\n";
        ss << "File #" << fileCounter << " - Timestamp: " 
           << std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::high_resolution_clock::now().time_since_epoch()).count() 
           << "\n";
        ss << "Size: " << sizeKB << " KB\n";
        ss << std::string(50, '=') << "\n\n";
        
        // Fill with random data to reach desired size
        for (size_t i = ss.str().length(); i < totalChars; ++i) {
            if (i % 80 == 79) {
                ss << '\n';  // Add line breaks for readability
            } else {
                ss << static_cast<char>(dis(gen));
            }
        }
        
        return ss.str();
    }
    
    void performIntensiveWrite() {
        std::string fileName = baseFileName + std::to_string(fileCounter) + ".tmp";
        
        // Generate large content (using configured size range)
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> sizeDis(MIN_FILE_SIZE_KB, MAX_FILE_SIZE_KB);
        size_t fileSize = sizeDis(gen);
        
        std::string content = generateLargeContent(fileSize);
        
        // Perform multiple write operations to the same file
        std::ofstream file(fileName, std::ios::binary | std::ios::trunc);
        if (file.is_open()) {
            // Write in chunks to create more I/O operations
            for (size_t i = 0; i < content.length(); i += WRITE_CHUNK_SIZE) {
                size_t currentChunkSize = std::min(WRITE_CHUNK_SIZE, content.length() - i);
                file.write(content.c_str() + i, currentChunkSize);
                file.flush(); // Force immediate write to disk
                totalBytesWritten += currentChunkSize;
                totalOperations++;
                
                // Small delay to make operations visible
                std::this_thread::sleep_for(std::chrono::microseconds(WRITE_DELAY_MICROSECONDS));
            }
            file.close();
        }
        
        std::cout << "WRITE: Created " << fileName << " (" << fileSize << " KB)" << std::endl;
    }
    
    void performIntensiveRead() {
        std::string fileName = baseFileName + std::to_string(fileCounter) + ".tmp";
        
        // Read the file in small chunks multiple times
        for (int readAttempt = 0; readAttempt < READ_REPETITIONS; ++readAttempt) {
            std::ifstream file(fileName, std::ios::binary);
            if (file.is_open()) {
                file.seekg(0, std::ios::end);
                size_t fileSize = file.tellg();
                file.seekg(0, std::ios::beg);
                
                // Read in small chunks
                char buffer[READ_BUFFER_SIZE];
                while (file.read(buffer, sizeof(buffer)) || file.gcount() > 0) {
                    totalBytesRead += file.gcount();
                    totalOperations++;
                    
                    // Small delay to make operations visible
                    std::this_thread::sleep_for(std::chrono::microseconds(READ_DELAY_MICROSECONDS));
                }
                file.close();
                
                std::cout << "READ #" << (readAttempt + 1) << ": " << fileName 
                         << " (" << fileSize << " bytes)" << std::endl;
            }
        }
    }
    
    void deleteTemporaryFile() {
        std::string fileName = baseFileName + std::to_string(fileCounter) + ".tmp";
        if (std::remove(fileName.c_str()) == 0) {
            std::cout << "DELETE: Removed " << fileName << std::endl;
        }
    }
    
    void displayStatistics() {
        auto currentTime = std::chrono::high_resolution_clock::now();
        auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(currentTime - startTime);
        
        std::cout << "\n" << std::string(60, '=') << std::endl;
        std::cout << "INTENSIVE I/O STATISTICS" << std::endl;
        std::cout << std::string(60, '=') << std::endl;
        std::cout << "Running time: " << elapsed.count() << " seconds" << std::endl;
        std::cout << "Total operations: " << totalOperations << std::endl;
        std::cout << "Files processed: " << fileCounter << std::endl;
        std::cout << "Total bytes written: " << std::fixed << std::setprecision(2) 
                 << (totalBytesWritten / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Total bytes read: " << std::fixed << std::setprecision(2) 
                 << (totalBytesRead / 1024.0 / 1024.0) << " MB" << std::endl;
        std::cout << "Operations per second: " << std::fixed << std::setprecision(2) 
                 << (elapsed.count() > 0 ? totalOperations / static_cast<double>(elapsed.count()) : 0) << std::endl;
        std::cout << std::string(60, '=') << std::endl;
    }
    
public:
    IntensiveIODemonstration() : 
        baseFileName(BASE_FILENAME), 
        fileCounter(0), 
        totalBytesWritten(0), 
        totalBytesRead(0), 
        totalOperations(0) {
        startTime = std::chrono::high_resolution_clock::now();
    }
    
    void run() {
        std::cout << "=== INTENSIVE I/O DEMONSTRATION (C++) ===" << std::endl;
        std::cout << "This program will perform excessive disk I/O operations" << std::endl;
        std::cout << "WARNING: This will stress your disk subsystem!" << std::endl;
        std::cout << "Press any key to stop the demonstration..." << std::endl;
        std::cout << std::string(50, '-') << std::endl;
        
        while (true) {
            // Check if user wants to stop
            if (_kbhit()) {
                char ch = _getch();
                std::cout << "\nStopping demonstration..." << std::endl;
                break;
            }
            
            fileCounter++;
            
            std::cout << "\n--- Cycle #" << fileCounter << " ---" << std::endl;
            
            // Perform intensive I/O operations
            performIntensiveWrite();
            performIntensiveRead();
            deleteTemporaryFile();
            
            // Display statistics every configured interval
            if (fileCounter % STATISTICS_INTERVAL == 0) {
                displayStatistics();
            }
            
            // Brief pause between cycles
            std::this_thread::sleep_for(std::chrono::milliseconds(CYCLE_DELAY_MS));
        }
        
        // Final statistics
        displayStatistics();
        
        std::cout << "\nDemonstration completed. Press any key to exit..." << std::endl;
        _getch();
    }
};

int main() {
    try {
        IntensiveIODemonstration demo;
        demo.run();
    } catch (const std::exception& e) {
        std::cerr << "Error: " << e.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        _getch();
        return 1;
    }
    
    return 0;
}
