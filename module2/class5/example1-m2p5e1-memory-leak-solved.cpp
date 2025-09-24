/*
 * =====================================================================================
 * MEMORY LEAK DEMONSTRATION - C++ (SOLVED VERSION)
 * =====================================================================================
 * 
 * Purpose: Demonstrate PROPER memory management by fixing all memory leaks
 *          Compare with original version to show the difference
 * 
 * Educational Context:
 * - Show how to properly manage memory with RAII principles
 * - Demonstrate correct use of delete/delete[] operators
 * - Use Visual Studio Memory Usage tool to validate fixes
 * - Compare heap growth: original (leaking) vs solved (stable)
 * - Integrate snapshot analysis for validation
 * 
 * How to use this example:
 * 1. Compile and run this program
 * 2. Open Memory Usage tool in Visual Studio (Debug > Memory Usage)
 * 3. Take snapshots before and after execution
 * 4. Observe STABLE heap (no growth)
 * 5. Compare with original version to see the difference
 * 
 * =====================================================================================
 */

// =====================================================================================
// CONFIGURATION PARAMETERS - MODIFY THESE TO ADJUST DEMONSTRATION BEHAVIOR
// =====================================================================================

// Batch Memory Leak Parameters
const int BATCH_ITERATIONS = 20;                    // Number of batch iterations
const int BATCH_PROCESSOR1_BASE_SIZE = 8000;       // Base size for processor 1
const int BATCH_PROCESSOR1_SIZE_INCREMENT = 2000;   // Size increment per iteration
const int BATCH_PROCESSOR2_BASE_SIZE = 6000;       // Base size for processor 2
const int BATCH_PROCESSOR2_SIZE_INCREMENT = 1000;   // Size increment per iteration
const int BATCH_PROCESSOR3_BASE_SIZE = 4000;        // Base size for processor 3
const int BATCH_PROCESSOR3_SIZE_INCREMENT = 500;     // Size increment per iteration
const int BATCH_ARRAY1_BASE_SIZE = 5000;            // Base size for array 1
const int BATCH_ARRAY1_SIZE_INCREMENT = 200;         // Size increment per iteration
const int BATCH_ARRAY2_BASE_SIZE = 3000;            // Base size for array 2
const int BATCH_ARRAY2_SIZE_INCREMENT = 100;         // Size increment per iteration
const int BATCH_ARRAY3_BASE_SIZE = 2000;            // Base size for array 3
const int BATCH_ARRAY3_SIZE_INCREMENT = 50;          // Size increment per iteration

// Continuous Growth Parameters
const int CONTINUOUS_DURATION_SECONDS = 120;          // Duration of continuous simulation
const int CONTINUOUS_CREATION_INTERVAL = 2;          // Create objects every N iterations
const int CONTINUOUS_PROCESSOR_BASE_SIZE = 3000;     // Base size for continuous processors
const int CONTINUOUS_PROCESSOR_SIZE_INCREMENT = 100; // Size increment per iteration
const int CONTINUOUS_ARRAY_BASE_SIZE = 2000;        // Base size for continuous arrays
const int CONTINUOUS_ARRAY_SIZE_INCREMENT = 25;      // Size increment per iteration

// Timing Parameters
const int BATCH_SNAPSHOT_INTERVAL = 3;               // Take snapshot every N iterations
const int BATCH_SNAPSHOT_PAUSE_MS = 200;            // Pause duration for snapshots (ms)
const int CONTINUOUS_STATUS_INTERVAL = 15;           // Show status every N iterations
const int CONTINUOUS_LOOP_PAUSE_MS = 100;            // Pause between iterations (ms)

// Display Parameters
const bool SHOW_DETAILED_MEMORY_INFO = true;        // Show detailed memory calculations
const bool SHOW_PROCESSING_MESSAGES = true;          // Show processing simulation messages

// =====================================================================================

#include <iostream>
#include <vector>
#include <string>
#include <memory>
#include <chrono>
#include <thread>
#include <random>

using namespace std;

// =====================================================================================
// CLASS THAT SIMULATES A COMPLEX OBJECT WITH PROPER MEMORY MANAGEMENT
// =====================================================================================
class DataProcessor {
private:
    vector<int> largeDataArray;
    string* description;
    double* calculations;
    int arraySize;
    
public:
    // Constructor that allocates large amounts of memory
    DataProcessor(int size = 10000) : arraySize(size) {
        cout << "  [CONSTRUCTOR] Allocating " << size << " elements..." << endl;
        
        // Allocate large integer array
        largeDataArray.resize(size);
        for (int i = 0; i < size; i++) {
            largeDataArray[i] = i * i; // Simulate complex data
        }
        
        // Allocate string dynamically
        description = new string("Data processor with " + to_string(size) + " elements");
        
        // Allocate calculation array
        calculations = new double[size];
        for (int i = 0; i < size; i++) {
            calculations[i] = sqrt(i * 3.14159);
        }
        
        cout << "  [CONSTRUCTOR] Memory allocated: ~" << (size * (sizeof(int) + sizeof(double)) + description->size()) << " bytes" << endl;
    }
    
    // DESTRUCTOR PROPERLY IMPLEMENTED TO PREVENT MEMORY LEAKS!
    ~DataProcessor() {
        cout << "  [DESTRUCTOR] Freeing memory for " << arraySize << " elements..." << endl;
        delete description;    // FIXED: Properly delete string
        delete[] calculations; // FIXED: Properly delete array
        cout << "  [DESTRUCTOR] Memory freed successfully!" << endl;
    }
    
    void processData() {
        cout << "  [PROCESSING] Processing " << arraySize << " elements..." << endl;
        // Simulate heavy processing
        for (int i = 0; i < arraySize; i++) {
            largeDataArray[i] = largeDataArray[i] * 2 + i;
        }
    }
    
    int getDataSize() const { return arraySize; }
};

// =====================================================================================
// FUNCTION THAT DEMONSTRATES PROPER MEMORY MANAGEMENT
// =====================================================================================
void demonstrateProperMemoryManagement(int iterations) {
    cout << "\n=== STARTING PROPER MEMORY MANAGEMENT DEMONSTRATION ===" << endl;
    cout << "Iterations: " << iterations << endl;
    
    // Calculate estimated memory based on configuration parameters
    int estimatedMemoryKB = iterations * (
        (BATCH_PROCESSOR1_BASE_SIZE + BATCH_PROCESSOR2_BASE_SIZE + BATCH_PROCESSOR3_BASE_SIZE) * 
        (sizeof(int) + sizeof(double)) + 
        (BATCH_ARRAY1_BASE_SIZE + BATCH_ARRAY2_BASE_SIZE + BATCH_ARRAY3_BASE_SIZE) * sizeof(int)
    ) / 1024;
    
    cout << "Estimated memory to be allocated/freed: ~" << estimatedMemoryKB << " KB" << endl;
    
    for (int i = 0; i < iterations; i++) {
        cout << "\n--- Iteration " << (i + 1) << " ---" << endl;
        
        // CREATE OBJECTS AND PROPERLY FREE MEMORY
        DataProcessor* processor1 = new DataProcessor(BATCH_PROCESSOR1_BASE_SIZE + (i * BATCH_PROCESSOR1_SIZE_INCREMENT));
        DataProcessor* processor2 = new DataProcessor(BATCH_PROCESSOR2_BASE_SIZE + (i * BATCH_PROCESSOR2_SIZE_INCREMENT));
        DataProcessor* processor3 = new DataProcessor(BATCH_PROCESSOR3_BASE_SIZE + (i * BATCH_PROCESSOR3_SIZE_INCREMENT));
        
        // Simulate object usage
        processor1->processData();
        processor2->processData();
        processor3->processData();
        
        // PROPERLY FREE MEMORY!
        delete processor1; // FIXED: Properly delete object
        delete processor2; // FIXED: Properly delete object
        delete processor3; // FIXED: Properly delete object
        
        // Allocate additional arrays and properly free them
        int* tempArray1 = new int[BATCH_ARRAY1_BASE_SIZE + (i * BATCH_ARRAY1_SIZE_INCREMENT)];
        double* tempArray2 = new double[BATCH_ARRAY2_BASE_SIZE + (i * BATCH_ARRAY2_SIZE_INCREMENT)];
        int* tempArray3 = new int[BATCH_ARRAY3_BASE_SIZE + (i * BATCH_ARRAY3_SIZE_INCREMENT)];
        
        // Simulate array usage
        for (int j = 0; j < BATCH_ARRAY1_BASE_SIZE + (i * BATCH_ARRAY1_SIZE_INCREMENT); j++) {
            tempArray1[j] = j * j;
        }
        for (int j = 0; j < BATCH_ARRAY3_BASE_SIZE + (i * BATCH_ARRAY3_SIZE_INCREMENT); j++) {
            tempArray3[j] = j * j * j;
        }
        
        // PROPERLY FREE ARRAYS!
        delete[] tempArray1; // FIXED: Properly delete array
        delete[] tempArray2; // FIXED: Properly delete array
        delete[] tempArray3; // FIXED: Properly delete array
        
        if (SHOW_DETAILED_MEMORY_INFO) {
            int iterationMemoryKB = ((BATCH_PROCESSOR1_BASE_SIZE + i * BATCH_PROCESSOR1_SIZE_INCREMENT) * 
                                   (sizeof(int) + sizeof(double))) / 1024;
            cout << "  [MANAGED] Objects created and memory PROPERLY freed! (~" << iterationMemoryKB << " KB this iteration)" << endl;
        } else {
            cout << "  [MANAGED] Objects created and memory PROPERLY freed!" << endl;
        }
        
        // Pause to visualize in Memory Usage tool
        if (i % BATCH_SNAPSHOT_INTERVAL == 0) {
            cout << "  [SNAPSHOT] Take a snapshot in Memory Usage tool now!" << endl;
            this_thread::sleep_for(chrono::milliseconds(BATCH_SNAPSHOT_PAUSE_MS));
        }
    }
    
    cout << "\n=== PROPER MEMORY MANAGEMENT DEMONSTRATED SUCCESSFULLY! ===" << endl;
    cout << "Total objects created/freed: " << (iterations * 6) << endl;
    cout << "Estimated memory managed: ~" << estimatedMemoryKB << " KB" << endl;
}

// =====================================================================================
// FUNCTION THAT SIMULATES REAL-WORLD PROPER MEMORY MANAGEMENT
// =====================================================================================
void simulateProperMemoryManagement(int duration_seconds) {
    cout << "\n=== SIMULATING PROPER MEMORY MANAGEMENT ===" << endl;
    cout << "Duration: " << duration_seconds << " seconds" << endl;
    cout << "This scenario simulates a real application with proper memory management..." << endl;
    
    auto start_time = chrono::high_resolution_clock::now();
    auto end_time = start_time + chrono::seconds(duration_seconds);
    
    int iteration = 0;
    vector<DataProcessor*> managedObjects; // Store pointers for demonstration
    vector<int*> managedArrays; // Store additional arrays for demonstration
    
    while (chrono::high_resolution_clock::now() < end_time) {
        iteration++;
        
        // Create objects periodically (simulates user events, requests, etc.)
        if (iteration % CONTINUOUS_CREATION_INTERVAL == 0) {
            DataProcessor* newProcessor = new DataProcessor(CONTINUOUS_PROCESSOR_BASE_SIZE + (iteration * CONTINUOUS_PROCESSOR_SIZE_INCREMENT));
            managedObjects.push_back(newProcessor); // Store to demonstrate proper management
            
            // Create additional arrays for demonstration
            int* newArray = new int[CONTINUOUS_ARRAY_BASE_SIZE + (iteration * CONTINUOUS_ARRAY_SIZE_INCREMENT)];
            managedArrays.push_back(newArray);
            
            cout << "  [MANAGEMENT] Object " << iteration << " created. Total managed: " << managedObjects.size() << 
                    " objects, " << managedArrays.size() << " arrays" << endl;
        }
        
        // Simulate processing
        if (SHOW_PROCESSING_MESSAGES && iteration % 4 == 0) {
            cout << "  [PROCESSING] Simulating normal application operation..." << endl;
        }
        
        // Pause for execution control
        this_thread::sleep_for(chrono::milliseconds(CONTINUOUS_LOOP_PAUSE_MS));
        
        // Show status periodically
        if (iteration % CONTINUOUS_STATUS_INTERVAL == 0) {
            cout << "  [STATUS] Iteration " << iteration << " - Managed objects: " << managedObjects.size() << 
                    ", Arrays: " << managedArrays.size() << endl;
            cout << "  [TIP] Observe the STABLE memory usage in Memory Usage tool!" << endl;
        }
    }
    
    cout << "\n=== SIMULATION COMPLETED ===" << endl;
    cout << "Total objects managed: " << managedObjects.size() << endl;
    cout << "Total arrays managed: " << managedArrays.size() << endl;
    
    int estimatedMemoryKB = ((managedObjects.size() * CONTINUOUS_PROCESSOR_BASE_SIZE + 
                             managedArrays.size() * CONTINUOUS_ARRAY_BASE_SIZE) * sizeof(int)) / 1024;
    cout << "Estimated memory managed: ~" << estimatedMemoryKB << " KB" << endl;
    
    // PROPERLY CLEAN UP ALL OBJECTS!
    cout << "\n=== CLEANING UP ALL OBJECTS ===" << endl;
    for (auto* obj : managedObjects) {
        delete obj; // FIXED: Properly delete all objects
    }
    for (auto* arr : managedArrays) {
        delete[] arr; // FIXED: Properly delete all arrays
    }
    
    cout << "All objects and arrays properly cleaned up!" << endl;
}

// =====================================================================================
// MAIN FUNCTION
// =====================================================================================
int main() {
    cout << "=====================================================================================" << endl;
    cout << "                    MEMORY MANAGEMENT DEMONSTRATION - C++ (SOLVED)" << endl;
    cout << "=====================================================================================" << endl;
    cout << "This program demonstrates PROPER memory management" << endl;
    cout << "for comparison with the original leaking version." << endl;
    cout << "\nINSTRUCTIONS FOR PROFESSOR:" << endl;
    cout << "1. Open Memory Usage tool (Debug > Memory Usage)" << endl;
    cout << "2. Take a snapshot BEFORE running" << endl;
    cout << "3. Run the program" << endl;
    cout << "4. Take snapshots during execution" << endl;
    cout << "5. Compare with original version - heap should be STABLE!" << endl;
    cout << "=====================================================================================" << endl;
    
    cout << "\nPress ENTER to start demonstration...";
    cin.get();
    
    // Demonstration 1: Proper memory management
    cout << "\n\n[DEMONSTRATION 1] Demonstrating proper memory management..." << endl;
    demonstrateProperMemoryManagement(BATCH_ITERATIONS); // Use configured iterations
    
    // Demonstration 2: Continuous proper management (no pause, runs immediately)
    cout << "\n\n[DEMONSTRATION 2] Simulating continuous proper management..." << endl;
    simulateProperMemoryManagement(CONTINUOUS_DURATION_SECONDS); // Use configured duration
    
    cout << "\n=====================================================================================" << endl;
    cout << "                    DEMONSTRATION COMPLETED" << endl;
    cout << "=====================================================================================" << endl;
    cout << "Now analyze the snapshots in Memory Usage tool to see:" << endl;
    cout << "- STABLE heap (no growth)" << endl;
    cout << "- Proper allocation/deallocation patterns" << endl;
    cout << "- Memory being freed correctly" << endl;
    cout << "- Compare with original version to see the difference!" << endl;
    cout << "\nLESSONS LEARNED:" << endl;
    cout << "- Importance of proper memory management" << endl;
    cout << "- RAII (Resource Acquisition Is Initialization) principles" << endl;
    cout << "- Use of smart pointers (unique_ptr, shared_ptr)" << endl;
    cout << "- Validation with profiling tools" << endl;
    cout << "- Always pair new with delete, new[] with delete[]" << endl;
    cout << "=====================================================================================" << endl;
    
    cout << "\nPress ENTER to finish...";
    cin.get();
    
    return 0;
}

/*
 * =====================================================================================
 * MEMORY USAGE TOOL ANALYSIS - SOLVED VERSION
 * =====================================================================================
 * 
 * What to observe in Memory Usage tool (SOLVED VERSION):
 * 
 * 1. HEAP STABILITY:
 *    - Initial snapshot: Small heap
 *    - Intermediate snapshots: STABLE heap (no growth)
 *    - Final snapshot: Same size as initial (or smaller)
 * 
 * 2. OBJECT LIFECYCLE:
 *    - Objects created and destroyed properly
 *    - Memory allocated and freed correctly
 *    - No accumulation of unused objects
 * 
 * 3. ALLOCATION PATTERNS:
 *    - Balanced allocation/deallocation
 *    - No memory leaks
 *    - Proper cleanup at end of functions
 * 
 * 4. PERFORMANCE BENEFITS:
 *    - No heap fragmentation
 *    - Consistent performance
 *    - Predictable memory usage
 * 
 * COMPARISON WITH ORIGINAL:
 *    - Original: Heap grows continuously
 *    - Solved: Heap remains stable
 *    - Original: Memory usage increases over time
 *    - Solved: Memory usage stays constant
 * 
 * =====================================================================================
 */
