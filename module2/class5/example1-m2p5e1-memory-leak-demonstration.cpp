/*
 * =====================================================================================
 * MEMORY LEAK DEMONSTRATION - C++
 * =====================================================================================
 * 
 * Purpose: Demonstrate memory leaks in a visible way for analysis with 
 *          Visual Studio Memory Usage Tool
 * 
 * Educational Context:
 * - Detect manual memory leaks (classic leak)
 * - Use Visual Studio Memory Usage tool to identify the problem
 * - Understand heap growth due to improper allocation
 * - Reflect on the importance of proper memory management
 * - Integrate snapshot analysis for validation
 * 
 * How to use this example:
 * 1. Compile and run this program
 * 2. Open Memory Usage tool in Visual Studio (Debug > Memory Usage)
 * 3. Take snapshots before and after execution
 * 4. Observe dramatic heap growth
 * 5. Analyze the types of objects that are leaking
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
// CLASS THAT SIMULATES A COMPLEX OBJECT WITH LARGE MEMORY USAGE
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
        
        // Allocate string dynamically (MEMORY LEAK!)
        description = new string("Data processor with " + to_string(size) + " elements");
        
        // Allocate calculation array (MEMORY LEAK!)
        calculations = new double[size];
        for (int i = 0; i < size; i++) {
            calculations[i] = sqrt(i * 3.14159);
        }
        
        cout << "  [CONSTRUCTOR] Memory allocated: ~" << (size * (sizeof(int) + sizeof(double)) + description->size()) << " bytes" << endl;
    }
    
    // DESTRUCTOR DELIBERATELY OMITTED TO CREATE MEMORY LEAK!
    // ~DataProcessor() {
    //     delete description;    // THIS LINE IS COMMENTED = MEMORY LEAK!
    //     delete[] calculations; // THIS LINE IS COMMENTED = MEMORY LEAK!
    // }
    
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
// FUNCTION THAT CREATES VISIBLE MEMORY LEAKS
// =====================================================================================
void createMemoryLeaks(int iterations) {
    cout << "\n=== STARTING MEMORY LEAK CREATION ===" << endl;
    cout << "Iterations: " << iterations << endl;
    
    // Calculate estimated memory based on configuration parameters
    int estimatedMemoryKB = iterations * (
        (BATCH_PROCESSOR1_BASE_SIZE + BATCH_PROCESSOR2_BASE_SIZE + BATCH_PROCESSOR3_BASE_SIZE) * 
        (sizeof(int) + sizeof(double)) + 
        (BATCH_ARRAY1_BASE_SIZE + BATCH_ARRAY2_BASE_SIZE + BATCH_ARRAY3_BASE_SIZE) * sizeof(int)
    ) / 1024;
    
    cout << "Estimated memory to be leaked: ~" << estimatedMemoryKB << " KB" << endl;
    
    for (int i = 0; i < iterations; i++) {
        cout << "\n--- Iteration " << (i + 1) << " ---" << endl;
        
        // CREATE OBJECTS WITHOUT FREEING MEMORY (MEMORY LEAK!)
        DataProcessor* processor1 = new DataProcessor(BATCH_PROCESSOR1_BASE_SIZE + (i * BATCH_PROCESSOR1_SIZE_INCREMENT));
        DataProcessor* processor2 = new DataProcessor(BATCH_PROCESSOR2_BASE_SIZE + (i * BATCH_PROCESSOR2_SIZE_INCREMENT));
        DataProcessor* processor3 = new DataProcessor(BATCH_PROCESSOR3_BASE_SIZE + (i * BATCH_PROCESSOR3_SIZE_INCREMENT));
        
        // Simulate object usage
        processor1->processData();
        processor2->processData();
        processor3->processData();
        
        // DELIBERATELY DO NOT FREE MEMORY!
        // delete processor1; // MEMORY LEAK!
        // delete processor2; // MEMORY LEAK!
        // delete processor3; // MEMORY LEAK!
        
        // Allocate additional arrays without freeing
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
        
        // DELIBERATELY DO NOT FREE ARRAYS!
        // delete[] tempArray1; // MEMORY LEAK!
        // delete[] tempArray2; // MEMORY LEAK!
        // delete[] tempArray3; // MEMORY LEAK!
        
        if (SHOW_DETAILED_MEMORY_INFO) {
            int iterationMemoryKB = ((BATCH_PROCESSOR1_BASE_SIZE + i * BATCH_PROCESSOR1_SIZE_INCREMENT) * 
                                   (sizeof(int) + sizeof(double))) / 1024;
            cout << "  [LEAK] Objects created but memory NOT freed! (~" << iterationMemoryKB << " KB this iteration)" << endl;
        } else {
            cout << "  [LEAK] Objects created but memory NOT freed!" << endl;
        }
        
        // Pause to visualize in Memory Usage tool
        if (i % BATCH_SNAPSHOT_INTERVAL == 0) {
            cout << "  [SNAPSHOT] Take a snapshot in Memory Usage tool now!" << endl;
            this_thread::sleep_for(chrono::milliseconds(BATCH_SNAPSHOT_PAUSE_MS));
        }
    }
    
    cout << "\n=== MEMORY LEAKS CREATED SUCCESSFULLY! ===" << endl;
    cout << "Total leaked objects: " << (iterations * 6) << endl;
    cout << "Estimated leaked memory: ~" << estimatedMemoryKB << " KB" << endl;
}

// =====================================================================================
// FUNCTION THAT SIMULATES REAL-WORLD CONTINUOUS GROWTH SCENARIO
// =====================================================================================
void simulateContinuousGrowth(int duration_seconds) {
    cout << "\n=== SIMULATING CONTINUOUS MEMORY GROWTH ===" << endl;
    cout << "Duration: " << duration_seconds << " seconds" << endl;
    cout << "This scenario simulates a real application with gradual leakage..." << endl;
    
    auto start_time = chrono::high_resolution_clock::now();
    auto end_time = start_time + chrono::seconds(duration_seconds);
    
    int iteration = 0;
    vector<DataProcessor*> leakedObjects; // Store pointers to demonstrate growth
    vector<int*> leakedArrays; // Store additional arrays for more visible leaks
    
    while (chrono::high_resolution_clock::now() < end_time) {
        iteration++;
        
        // Create objects periodically (simulates user events, requests, etc.)
        if (iteration % CONTINUOUS_CREATION_INTERVAL == 0) {
            DataProcessor* newProcessor = new DataProcessor(CONTINUOUS_PROCESSOR_BASE_SIZE + (iteration * CONTINUOUS_PROCESSOR_SIZE_INCREMENT));
            leakedObjects.push_back(newProcessor); // Store to demonstrate growth
            
            // Create additional arrays for more visible leaks
            int* newArray = new int[CONTINUOUS_ARRAY_BASE_SIZE + (iteration * CONTINUOUS_ARRAY_SIZE_INCREMENT)];
            leakedArrays.push_back(newArray);
            
            cout << "  [GROWTH] Object " << iteration << " created. Total leaked: " << leakedObjects.size() << 
                    " objects, " << leakedArrays.size() << " arrays" << endl;
        }
        
        // Simulate processing
        if (SHOW_PROCESSING_MESSAGES && iteration % 4 == 0) {
            cout << "  [PROCESSING] Simulating normal application operation..." << endl;
        }
        
        // Pause for execution control
        this_thread::sleep_for(chrono::milliseconds(CONTINUOUS_LOOP_PAUSE_MS));
        
        // Show status periodically
        if (iteration % CONTINUOUS_STATUS_INTERVAL == 0) {
            cout << "  [STATUS] Iteration " << iteration << " - Leaked objects: " << leakedObjects.size() << 
                    ", Arrays: " << leakedArrays.size() << endl;
            cout << "  [TIP] Observe the growth in Memory Usage tool!" << endl;
        }
    }
    
    cout << "\n=== SIMULATION COMPLETED ===" << endl;
    cout << "Total leaked objects: " << leakedObjects.size() << endl;
    cout << "Total leaked arrays: " << leakedArrays.size() << endl;
    
    int estimatedMemoryKB = ((leakedObjects.size() * CONTINUOUS_PROCESSOR_BASE_SIZE + 
                             leakedArrays.size() * CONTINUOUS_ARRAY_BASE_SIZE) * sizeof(int)) / 1024;
    cout << "Estimated leaked memory: ~" << estimatedMemoryKB << " KB" << endl;
    
    // DELIBERATELY DO NOT CLEAR VECTORS - PERMANENT MEMORY LEAK!
}

// =====================================================================================
// MAIN FUNCTION
// =====================================================================================
int main() {
    cout << "=====================================================================================" << endl;
    cout << "                    MEMORY LEAK DEMONSTRATION - C++" << endl;
    cout << "=====================================================================================" << endl;
    cout << "This program demonstrates memory leaks in a VISIBLE way" << endl;
    cout << "for analysis with Visual Studio Memory Usage tool." << endl;
    cout << "\nINSTRUCTIONS FOR PROFESSOR:" << endl;
    cout << "1. Open Memory Usage tool (Debug > Memory Usage)" << endl;
    cout << "2. Take a snapshot BEFORE running" << endl;
    cout << "3. Run the program" << endl;
    cout << "4. Take snapshots during execution" << endl;
    cout << "5. Compare snapshots to see dramatic heap growth" << endl;
    cout << "=====================================================================================" << endl;
    
    cout << "\nPress ENTER to start demonstration...";
    cin.get();
    
    // Demonstration 1: Batch memory leaks
    cout << "\n\n[DEMONSTRATION 1] Creating batch memory leaks..." << endl;
    createMemoryLeaks(BATCH_ITERATIONS); // Use configured iterations
    
    // Demonstration 2: Continuous growth (no pause, runs immediately)
    cout << "\n\n[DEMONSTRATION 2] Simulating continuous growth..." << endl;
    simulateContinuousGrowth(CONTINUOUS_DURATION_SECONDS); // Use configured duration
    
    cout << "\n=====================================================================================" << endl;
    cout << "                    DEMONSTRATION COMPLETED" << endl;
    cout << "=====================================================================================" << endl;
    cout << "Now analyze the snapshots in Memory Usage tool to see:" << endl;
    cout << "- Dramatic heap growth" << endl;
    cout << "- Types of objects that are leaking" << endl;
    cout << "- Allocation patterns" << endl;
    cout << "- System performance impact" << endl;
    cout << "\nLESSONS LEARNED:" << endl;
    cout << "- Importance of proper memory management" << endl;
    cout << "- Need for RAII (Resource Acquisition Is Initialization)" << endl;
    cout << "- Use of smart pointers (unique_ptr, shared_ptr)" << endl;
    cout << "- Validation with profiling tools" << endl;
    cout << "=====================================================================================" << endl;
    
    cout << "\nPress ENTER to finish...";
    cin.get();
    
    return 0;
}

/*
 * =====================================================================================
 * MEMORY USAGE TOOL ANALYSIS
 * =====================================================================================
 * 
 * What to observe in Memory Usage tool:
 * 
 * 1. HEAP GROWTH:
 *    - Initial snapshot: Small heap
 *    - Intermediate snapshots: Constant growth
 *    - Final snapshot: Very large heap
 * 
 * 2. OBJECT TYPES:
 *    - DataProcessor objects
 *    - int arrays
 *    - double arrays
 *    - string objects
 * 
 * 3. ALLOCATION PATTERNS:
 *    - Multiple allocations of same type
 *    - Linear growth over time
 *    - Absence of corresponding deallocation
 * 
 * 4. PERFORMANCE IMPACT:
 *    - Heap fragmentation
 *    - Possible thrashing
 *    - Gradual performance degradation
 * 
 * =====================================================================================
 */
