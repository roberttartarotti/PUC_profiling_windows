/*
 * =====================================================================================
 * MEMORY LEAK DEMONSTRATION - C++ (CLASS 6)
 * =====================================================================================
 * 
 * Purpose: Demonstrate memory leaks caused by improper memory management
 *          This example shows the effects of forgetting to use 'delete' operators
 * 
 * Educational Context:
 * - Demonstrate memory leaks in a measurable way
 * - Show how forgetting 'delete' affects system resources
 * - Use Visual Studio Memory Usage tool to identify memory leaks
 * - Understand heap exhaustion and resource management
 * - Show consequences of improper memory management
 * 
 * How to use this example:
 * 1. Compile and run this program
 * 2. Open Memory Usage tool in Visual Studio (Debug > Memory Usage)
 * 3. Take snapshots before and after execution
 * 4. Observe heap growth and memory consumption patterns
 * 5. Analyze memory allocation without corresponding deallocation
 * 
 * WARNING: This program is designed to consume significant amounts of memory.
 * It will create memory leaks that are not freed during execution.
 * Run in a controlled environment with sufficient RAM available.
 * EXPECTED MEMORY CONSUMPTION: 10+ GB of allocated memory.
 * 
 * =====================================================================================
 */

// =====================================================================================
// CONFIGURATION PARAMETERS - MODIFY THESE TO ADJUST DEMONSTRATION INTENSITY
// =====================================================================================

// Memory Leak Parameters - Increased for demonstration
const int MEGA_ITERATIONS = 100;                         // Number of iterations
const int OBJECTS_PER_ITERATION = 200;                   // Objects created per iteration
const int MEGA_ARRAY_SIZE = 100000;                      // Size of arrays
const int STRING_BUFFER_SIZE = 50000;                    // Size of string buffers
const int MATRIX_DIMENSION = 2000;                       // Matrix dimensions

// Memory Leak Types - All enabled for comprehensive demonstration
const bool CREATE_MEGA_ARRAYS = true;                    // Create large arrays
const bool CREATE_COMPLEX_OBJECTS = true;                // Create complex nested objects
const bool CREATE_STRING_BUFFERS = true;                 // Create string buffers
const bool CREATE_MATRICES = true;                       // Create matrices
const bool CREATE_RECURSIVE_STRUCTURES = true;          // Create recursive structures
const bool CREATE_ADDITIONAL_LEAKS = true;              // Create additional memory leaks
const bool CREATE_CHAIN_LEAKS = true;                   // Create chained memory leaks
const bool CREATE_EXPONENTIAL_LEAKS = true;             // Create exponential memory leaks

// Timing and Display
const int DISPLAY_INTERVAL = 5;                          // Show progress every N iterations
const int MEMORY_CHECK_INTERVAL = 10;                    // Check memory usage every N iterations
const int PAUSE_FOR_SNAPSHOT_MS = 500;                  // Pause for memory snapshots

// =====================================================================================

#include <iostream>
#include <vector>
#include <string>
#include <chrono>
#include <thread>
#include <cmath>

using namespace std;

// =====================================================================================
// COMPLEX NESTED CLASS THAT ALLOCATES SIGNIFICANT MEMORY
// =====================================================================================
class MegaDataProcessor {
private:
    // Multiple large data structures
    vector<int>* megaArray1;
    vector<double>* megaArray2;
    vector<string>* stringCollection;
    double** matrix;
    int matrixSize;
    MegaDataProcessor** childProcessors;  // Recursive structure
    int childCount;
    string* largeDescription;
    int processorId;
    
public:
    // Constructor that allocates significant amounts of memory
    MegaDataProcessor(int id, int size = MEGA_ARRAY_SIZE) : processorId(id), matrixSize(MATRIX_DIMENSION) {
        cout << "  [CONSTRUCTOR] Processor " << id << " allocating memory..." << endl;
        
        // Allocate mega arrays
        megaArray1 = new vector<int>(size);
        megaArray2 = new vector<double>(size);
        stringCollection = new vector<string>(size / 100); // Smaller but still large
        
        // Fill arrays with data
        for (int i = 0; i < size; i++) {
            (*megaArray1)[i] = i * i * i; // Cubic growth
            (*megaArray2)[i] = sqrt(i * 3.14159 * 2.71828); // Complex calculations
        }
        
        // Fill string collection
        for (int i = 0; i < size / 100; i++) {
            (*stringCollection)[i] = "Large string buffer " + to_string(i) + " with lots of data " + 
                                   string(STRING_BUFFER_SIZE, 'X');
        }
        
        // Allocate large matrix
        matrix = new double*[matrixSize];
        for (int i = 0; i < matrixSize; i++) {
            matrix[i] = new double[matrixSize];
            for (int j = 0; j < matrixSize; j++) {
                matrix[i][j] = sin(i * j * 0.001); // Complex matrix calculations
            }
        }
        
        // Create recursive child processors (memory leak multiplier)
        childCount = 10; // Each processor creates 10 children
        childProcessors = new MegaDataProcessor*[childCount];
        for (int i = 0; i < childCount; i++) {
            childProcessors[i] = new MegaDataProcessor(id * 10 + i, size / 2);
        }
        
        // Create additional memory leaks for demonstration
        if (CREATE_ADDITIONAL_LEAKS) {
            // Create additional arrays that will not be freed
            int* additionalArray1 = new int[size * 2]; // Double size array
            double* additionalArray2 = new double[size * 2]; // Double size array
            string* additionalStrings = new string[size / 50]; // Additional strings
            
            // Fill additional arrays with data
            for (int i = 0; i < size * 2; i++) {
                additionalArray1[i] = i * i * i * i; // Quartic growth
                additionalArray2[i] = sqrt(i * i * i * 3.14159);
            }
            
            // Fill additional strings
            for (int i = 0; i < size / 50; i++) {
                additionalStrings[i] = "Additional string " + to_string(i) + " " + 
                                     string(STRING_BUFFER_SIZE * 2, 'A'); // Double size strings
            }
            
            // Memory leak: additional arrays are not deleted
        }
        
        // Allocate large description string
        largeDescription = new string("MegaDataProcessor " + to_string(id) + 
                                    " with massive memory allocation " + 
                                    string(STRING_BUFFER_SIZE, 'M'));
        
        // Calculate total memory allocated
        long long totalMemory = (size * sizeof(int)) + (size * sizeof(double)) + 
                               (size / 100 * STRING_BUFFER_SIZE) + 
                               (matrixSize * matrixSize * sizeof(double)) +
                               (childCount * sizeof(MegaDataProcessor*)) +
                               STRING_BUFFER_SIZE;
        
        cout << "  [CONSTRUCTOR] Processor " << id << " allocated ~" << (totalMemory / 1024 / 1024) << " MB" << endl;
    }
    
    // Destructor omitted to demonstrate memory leak
    // ~MegaDataProcessor() {
    //     delete megaArray1;           // Memory leak
    //     delete megaArray2;           // Memory leak
    //     delete stringCollection;     // Memory leak
    //     for (int i = 0; i < matrixSize; i++) {
    //         delete[] matrix[i];       // Memory leak
    //     }
    //     delete[] matrix;              // Memory leak
    //     for (int i = 0; i < childCount; i++) {
    //         delete childProcessors[i]; // Memory leak
    //     }
    //     delete[] childProcessors;     // Memory leak
    //     delete largeDescription;     // Memory leak
    // }
    
    void processMegaData() {
        cout << "  [PROCESSING] Processor " << processorId << " processing data..." << endl;
        
        // Simulate heavy processing on all data structures
        for (int i = 0; i < megaArray1->size(); i++) {
            (*megaArray1)[i] = (*megaArray1)[i] * 3 + i * i;
        }
        
        for (int i = 0; i < megaArray2->size(); i++) {
            (*megaArray2)[i] = (*megaArray2)[i] * 2.71828 + sin(i);
        }
        
        // Process matrix
        for (int i = 0; i < matrixSize; i++) {
            for (int j = 0; j < matrixSize; j++) {
                matrix[i][j] = matrix[i][j] * matrix[i][j] + cos(i * j * 0.001);
            }
        }
        
        // Process child processors recursively
        for (int i = 0; i < childCount; i++) {
            childProcessors[i]->processMegaData();
        }
    }
    
    int getId() const { return processorId; }
    long long getMemoryUsage() const {
        return (megaArray1->size() * sizeof(int)) + 
               (megaArray2->size() * sizeof(double)) + 
               (stringCollection->size() * STRING_BUFFER_SIZE) +
               (matrixSize * matrixSize * sizeof(double)) +
               STRING_BUFFER_SIZE;
    }
};

// =====================================================================================
// FUNCTION THAT CREATES MEMORY LEAKS
// =====================================================================================
void createMemoryLeaks() {
    cout << "\n=== STARTING MEMORY LEAK CREATION ===" << endl;
    cout << "WARNING: This will consume significant amounts of memory!" << endl;
    cout << "Iterations: " << MEGA_ITERATIONS << endl;
    cout << "Objects per iteration: " << OBJECTS_PER_ITERATION << endl;
    cout << "Estimated total objects: " << (MEGA_ITERATIONS * OBJECTS_PER_ITERATION) << endl;
    
    vector<MegaDataProcessor*> leakedProcessors;
    vector<int*> leakedArrays;
    vector<double*> leakedDoubleArrays;
    vector<string*> leakedStrings;
    
    long long totalLeakedMemory = 0;
    
    for (int iteration = 0; iteration < MEGA_ITERATIONS; iteration++) {
        cout << "\n--- ITERATION " << (iteration + 1) << " ---" << endl;
        
        // Create multiple processors (each creates memory leaks)
        for (int obj = 0; obj < OBJECTS_PER_ITERATION; obj++) {
            int processorId = iteration * OBJECTS_PER_ITERATION + obj;
            MegaDataProcessor* processor = new MegaDataProcessor(processorId);
            leakedProcessors.push_back(processor);
            
            // Simulate usage
            processor->processMegaData();
            
            // Memory leak: object is not deleted
        }
        
        // Create additional arrays for demonstration
        if (CREATE_MEGA_ARRAYS) {
            int* megaIntArray = new int[MEGA_ARRAY_SIZE];
            double* megaDoubleArray = new double[MEGA_ARRAY_SIZE];
            
            // Fill arrays
            for (int i = 0; i < MEGA_ARRAY_SIZE; i++) {
                megaIntArray[i] = i * i * i * i; // Quartic growth
                megaDoubleArray[i] = sqrt(i * i * i * 3.14159);
            }
            
            leakedArrays.push_back(megaIntArray);
            leakedDoubleArrays.push_back(megaDoubleArray);
            
            // Memory leak: arrays are not deleted
        }
        
        // Create exponential memory leaks for demonstration
        if (CREATE_EXPONENTIAL_LEAKS) {
            // Create exponentially growing arrays
            int exponentialSize = MEGA_ARRAY_SIZE * (iteration + 1);
            int* exponentialArray = new int[exponentialSize];
            double* exponentialDoubleArray = new double[exponentialSize];
            
            // Fill exponential arrays
            for (int i = 0; i < exponentialSize; i++) {
                exponentialArray[i] = i * i * i * i * i; // Pentic growth
                exponentialDoubleArray[i] = sqrt(i * i * i * i * 3.14159);
            }
            
            leakedArrays.push_back(exponentialArray);
            leakedDoubleArrays.push_back(exponentialDoubleArray);
            
            // Memory leak: exponential arrays are not deleted
        }
        
        // Create chained memory leaks
        if (CREATE_CHAIN_LEAKS) {
            // Create chains of linked memory allocations
            for (int chain = 0; chain < 5; chain++) {
                int* chainArray = new int[MEGA_ARRAY_SIZE / 2];
                string* chainString = new string(STRING_BUFFER_SIZE * 3, 'C');
                
                // Fill chain arrays
                for (int i = 0; i < MEGA_ARRAY_SIZE / 2; i++) {
                    chainArray[i] = i * i * i * i * i * i; // Sextic growth
                }
                
                leakedArrays.push_back(chainArray);
                leakedStrings.push_back(chainString);
                
                // Memory leak: chain arrays are not deleted
            }
        }
        
        // Create string buffers
        if (CREATE_STRING_BUFFERS) {
            string* megaString = new string(STRING_BUFFER_SIZE * 10, 'L'); // Large string
            leakedStrings.push_back(megaString);
            
            // Memory leak: string is not deleted
        }
        
        // Calculate current memory usage
        totalLeakedMemory += (OBJECTS_PER_ITERATION * MEGA_ARRAY_SIZE * (sizeof(int) + sizeof(double))) +
                            (MEGA_ARRAY_SIZE * sizeof(int)) + (MEGA_ARRAY_SIZE * sizeof(double)) +
                            (STRING_BUFFER_SIZE * 10);
        
        // Display progress
        if (iteration % DISPLAY_INTERVAL == 0) {
            cout << "  [PROGRESS] Iteration " << (iteration + 1) << " completed!" << endl;
            cout << "  [MEMORY-STATS] Total processors leaked: " << leakedProcessors.size() << endl;
            cout << "  [MEMORY-STATS] Total arrays leaked: " << (leakedArrays.size() + leakedDoubleArrays.size()) << endl;
            cout << "  [MEMORY-STATS] Total strings leaked: " << leakedStrings.size() << endl;
            cout << "  [MEMORY-STATS] Estimated leaked memory: ~" << (totalLeakedMemory / 1024 / 1024) << " MB" << endl;
            cout << "  [INFO] Memory consumption increasing with iterations." << endl;
        }
        
        // Pause for memory snapshots
        if (iteration % MEMORY_CHECK_INTERVAL == 0) {
            cout << "  [SNAPSHOT] Take a memory snapshot now. Memory usage is increasing." << endl;
            this_thread::sleep_for(chrono::milliseconds(PAUSE_FOR_SNAPSHOT_MS));
        }
        
        // Simulate additional processing
        if (iteration % 3 == 0) {
            cout << "  [PROCESSING] Simulating additional processing load..." << endl;
            // Create temporary objects for additional processing
            vector<int> tempStress(10000);
            for (int i = 0; i < 10000; i++) {
                tempStress[i] = i * i * i * i * i; // Pentic growth for stress
            }
        }
    }
    
    cout << "\n=== MEMORY LEAKS CREATED ===" << endl;
    cout << "FINAL STATISTICS:" << endl;
    cout << "- Total processors leaked: " << leakedProcessors.size() << endl;
    cout << "- Total arrays leaked: " << (leakedArrays.size() + leakedDoubleArrays.size()) << endl;
    cout << "- Total strings leaked: " << leakedStrings.size() << endl;
    cout << "- Estimated leaked memory: ~" << (totalLeakedMemory / 1024 / 1024) << " MB" << endl;
    cout << "- Memory consumption: Significantly increased" << endl;
    cout << "- System impact: High memory usage" << endl;
    cout << "- Memory fragmentation: Present" << endl;
    cout << "- Performance impact: Degraded" << endl;
    cout << "- Note: Memory will not be freed during execution" << endl;
}

// =====================================================================================
// FUNCTION THAT CREATES PERSISTENT MEMORY LEAKS
// =====================================================================================
void createPersistentMemoryLeaks() {
    cout << "\n=== CREATING PERSISTENT MEMORY LEAKS ===" << endl;
    cout << "WARNING: These memory leaks will not be freed during execution." << endl;
    cout << "They will persist until the program terminates." << endl;
    
    // Create global persistent leaks that will not be freed during execution
    static vector<int*> persistentIntArrays;
    static vector<double*> persistentDoubleArrays;
    static vector<string*> persistentStrings;
    static vector<MegaDataProcessor*> persistentProcessors;
    
    // Create massive persistent arrays
    for (int i = 0; i < 50; i++) {
        int* persistentArray = new int[MEGA_ARRAY_SIZE];
        double* persistentDoubleArray = new double[MEGA_ARRAY_SIZE];
        string* persistentString = new string(STRING_BUFFER_SIZE * 5, 'P');
        
        // Fill persistent arrays
        for (int j = 0; j < MEGA_ARRAY_SIZE; j++) {
            persistentArray[j] = j * j * j * j * j * j * j; // Septic growth
            persistentDoubleArray[j] = sqrt(j * j * j * j * j * 3.14159);
        }
        
        persistentIntArrays.push_back(persistentArray);
        persistentDoubleArrays.push_back(persistentDoubleArray);
        persistentStrings.push_back(persistentString);
        
        // Create persistent processors
        MegaDataProcessor* persistentProcessor = new MegaDataProcessor(999999 + i);
        persistentProcessors.push_back(persistentProcessor);
        
        // Memory leak: persistent objects are not deleted
    }
    
    cout << "Created " << persistentIntArrays.size() << " persistent int arrays" << endl;
    cout << "Created " << persistentDoubleArrays.size() << " persistent double arrays" << endl;
    cout << "Created " << persistentStrings.size() << " persistent strings" << endl;
    cout << "Created " << persistentProcessors.size() << " persistent processors" << endl;
    cout << "These will not be freed until program termination." << endl;
}

// =====================================================================================
// FUNCTION THAT SIMULATES REAL-WORLD MEMORY EXHAUSTION SCENARIO
// =====================================================================================
void simulateMemoryExhaustion() {
    cout << "\n=== SIMULATING MEMORY EXHAUSTION SCENARIO ===" << endl;
    cout << "This simulates a real application that gradually exhausts system memory..." << endl;
    
    vector<MegaDataProcessor*> exhaustionProcessors;
    int iteration = 0;
    long long totalMemory = 0;
    
    try {
        while (true) { // Continue until memory exhaustion
            iteration++;
            
            // Create processors in batches
            for (int batch = 0; batch < 20; batch++) {
                MegaDataProcessor* processor = new MegaDataProcessor(iteration * 1000 + batch);
                exhaustionProcessors.push_back(processor);
                
                // Simulate usage
                processor->processMegaData();
                
                // DELIBERATELY DO NOT DELETE! (MEMORY EXHAUSTION!)
            }
            
            totalMemory += (20 * MEGA_ARRAY_SIZE * (sizeof(int) + sizeof(double)));
            
            if (iteration % 5 == 0) {
                cout << "  [EXHAUSTION] Iteration " << iteration << " - Processors: " << exhaustionProcessors.size() << endl;
                cout << "  [EXHAUSTION] Estimated memory: ~" << (totalMemory / 1024 / 1024) << " MB" << endl;
                cout << "  [EXHAUSTION] System memory pressure increasing..." << endl;
                
                // Pause for observation
                this_thread::sleep_for(chrono::milliseconds(200));
            }
            
            // Simulate system becoming slower
            if (iteration % 10 == 0) {
                cout << "  [SYSTEM-SLOWDOWN] Memory pressure causing system slowdown..." << endl;
                this_thread::sleep_for(chrono::milliseconds(500));
            }
        }
    } catch (const bad_alloc& e) {
        cout << "\n=== MEMORY EXHAUSTION ACHIEVED! ===" << endl;
        cout << "Exception caught: " << e.what() << endl;
        cout << "Total processors before exhaustion: " << exhaustionProcessors.size() << endl;
        cout << "Total memory consumed: ~" << (totalMemory / 1024 / 1024) << " MB" << endl;
        cout << "System impact: CATASTROPHIC FAILURE!" << endl;
    }
}

// =====================================================================================
// MAIN FUNCTION
// =====================================================================================
int main() {
    cout << "=====================================================================================" << endl;
    cout << "                    MEMORY LEAK DEMONSTRATION - C++" << endl;
    cout << "=====================================================================================" << endl;
    cout << "This program demonstrates memory leaks caused by" << endl;
    cout << "forgetting to use 'delete' operators." << endl;
    cout << "\nEDUCATIONAL OBJECTIVES:" << endl;
    cout << "- Show consequences of memory leaks" << endl;
    cout << "- Demonstrate resource exhaustion due to memory leaks" << endl;
    cout << "- Visualize memory growth patterns" << endl;
    cout << "- Understand the importance of proper memory management" << endl;
    cout << "\nWARNING: This program will consume significant amounts of memory." << endl;
    cout << "It will create memory leaks that will not be freed during execution." << endl;
    cout << "EXPECTED CONSUMPTION: 10+ GB of allocated memory." << endl;
    cout << "Run in a controlled environment with sufficient RAM." << endl;
    cout << "=====================================================================================" << endl;
    
    cout << "\nPress ENTER to start the memory leak demonstration...";
    cin.get();
    
    // Demonstration 1: Memory leaks
    cout << "\n\n[DEMONSTRATION 1] Creating memory leaks..." << endl;
    createMemoryLeaks();
    
    // Demonstration 2: Persistent memory leaks
    cout << "\n\n[DEMONSTRATION 2] Creating persistent memory leaks..." << endl;
    createPersistentMemoryLeaks();
    
    // Demonstration 3: Memory exhaustion simulation
    cout << "\n\n[DEMONSTRATION 3] Simulating memory exhaustion..." << endl;
    simulateMemoryExhaustion();
    
    cout << "\n=====================================================================================" << endl;
    cout << "                    MEMORY LEAK DEMONSTRATION COMPLETED" << endl;
    cout << "=====================================================================================" << endl;
    cout << "LESSONS LEARNED:" << endl;
    cout << "- Memory leaks can cause system resource exhaustion" << endl;
    cout << "- Forgetting 'delete' leads to memory growth" << endl;
    cout << "- Memory exhaustion can affect application performance" << endl;
    cout << "- Proper memory management is important for system stability" << endl;
    cout << "- RAII (Resource Acquisition Is Initialization) prevents leaks" << endl;
    cout << "- Smart pointers (unique_ptr, shared_ptr) eliminate manual management" << endl;
    cout << "- Always pair 'new' with 'delete', 'new[]' with 'delete[]'" << endl;
    cout << "\nPROFESSOR NOTES:" << endl;
    cout << "- Use Memory Usage tool to observe heap growth" << endl;
    cout << "- Compare snapshots to see memory consumption patterns" << endl;
    cout << "- Show students the consequences of improper memory management" << endl;
    cout << "- Demonstrate how memory leaks can affect system performance" << endl;
    cout << "=====================================================================================" << endl;
    
    cout << "\nPress ENTER to finish...";
    cin.get();
    
    return 0;
}

/*
 * =====================================================================================
 * MEMORY USAGE TOOL ANALYSIS - CATASTROPHIC VERSION
 * =====================================================================================
 * 
 * What to observe in Memory Usage tool:
 * 
 * 1. EXPONENTIAL HEAP GROWTH:
 *    - Initial snapshot: Small heap
 *    - Intermediate snapshots: EXPONENTIAL growth
 *    - Final snapshot: MASSIVE heap (potentially system-crashing)
 * 
 * 2. OBJECT TYPES LEAKING:
 *    - MegaDataProcessor objects (each containing multiple large structures)
 *    - Massive int arrays (50,000+ elements each)
 *    - Massive double arrays (50,000+ elements each)
 *    - Large string buffers (10,000+ characters each)
 *    - Large matrices (1000x1000 elements each)
 *    - Recursive child processors (5 children per processor)
 * 
 * 3. ALLOCATION PATTERNS:
 *    - Multiple allocations of same massive types
 *    - Exponential growth over time
 *    - Complete absence of deallocation
 *    - Recursive memory allocation (children creating more children)
 * 
 * 4. SYSTEM IMPACT:
 *    - Severe heap fragmentation
 *    - System slowdown and potential freezing
 *    - Possible memory exhaustion and crashes
 *    - Dramatic performance degradation
 * 
 * 5. EDUCATIONAL VALUE:
 *    - Shows real-world consequences of memory leaks
 *    - Demonstrates why proper memory management is critical
 *    - Illustrates how small leaks can become catastrophic
 *    - Proves the importance of RAII and smart pointers
 * 
 * =====================================================================================
 */
