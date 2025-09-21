/*
 * PROFILING EXAMPLE: Performance Problem Demonstration
 * 
 * This example demonstrates common performance issues and inefficiencies:
 * - Excessive loop iterations and nested loops
 * - Redundant calculations and repeated expensive operations
 * - Inefficient memory allocation patterns
 * - Thread contention and resource bottlenecks
 * - Deep call stacks with expensive operations
 * 
 * NOTE: This code intentionally contains performance problems for educational purposes.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance bottlenecks and learn optimization techniques.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <cmath>
#include <algorithm>
#include <string>
#include <memory>
#include <unordered_map>
#include <thread>
#include <mutex>
#include <atomic>
#include <future>

using namespace std;
using namespace std::chrono;

// Global configuration - demonstrating performance issues
const int DEFAULT_NUM_THREADS = 60;  // High thread count for demonstration
const int MATRIX_SIZE = 1000;         // Large matrix size for profiling
const int STRING_COUNT = 10000;       // Large string count for profiling

// Global variables - demonstrating resource usage patterns
vector<vector<double>> global_matrix;
vector<string> global_strings;
unordered_map<string, double> global_map;
unordered_map<string, vector<int>> global_unordered_map;
mutex global_mutex;
atomic<int> thread_counter(0);
atomic<double> shared_result(0.0);
vector<thread> worker_threads;
random_device rd;
mt19937 gen(rd());
uniform_real_distribution<double> dis(0.0, 1000.0);

// Forward declarations
void test_problem_functions(int iterations);
void test_call_frequency_patterns(int iterations);
void test_nested_function_calls(int iterations);
void start_profiling_threads();
double nested_level_1(double x, int depth);
double nested_level_2(double x, int depth);
double nested_level_3(double x, int depth);
double nested_level_4(double x, int depth);
double nested_level_5(double x, int depth);

// Mathematical operations - precomputed values and algorithms
class MathCache {
private:
    static constexpr int CACHE_SIZE = 1000;
    vector<double> sin_cache;
    vector<double> cos_cache;
    vector<double> sqrt_cache;
    
public:
    MathCache() : sin_cache(CACHE_SIZE), cos_cache(CACHE_SIZE), sqrt_cache(CACHE_SIZE) {
        // Precompute common values
        for (int i = 0; i < CACHE_SIZE; ++i) {
            double val = i * 0.01;  // Scale for better cache utilization
            sin_cache[i] = sin(val);
            cos_cache[i] = cos(val);
            sqrt_cache[i] = sqrt(val + 1);
        }
    }
    
    double fast_sin(double x) const {
        int index = static_cast<int>(x * 100) % CACHE_SIZE;
        return sin_cache[index];
    }
    
    double fast_cos(double x) const {
        int index = static_cast<int>(x * 100) % CACHE_SIZE;
        return cos_cache[index];
    }
    
    double fast_sqrt(double x) const {
        int index = static_cast<int>(x * 100) % CACHE_SIZE;
        return sqrt_cache[index];
    }
};

// Global math cache for operations
MathCache math_cache;

/*
 * SCENARIO 1: Performance Problem Functions
 * These functions demonstrate common performance issues and inefficiencies
 */

// CPU-intensive function - demonstrates severe performance problems
double cpu_intensive_problem(double x) {
    double result = 0.0;
    
    // MAJOR PROBLEM: Excessive nested loops with expensive operations
    for (int i = 0; i < 200; ++i) {  // Increased from 50
        for (int j = 0; j < 100; ++j) {  // Increased from 10
            for (int k = 0; k < 50; ++k) {  // Added third level
                // MAJOR PROBLEM: Expensive trigonometric calculations in innermost loop
                result += sin(x + i) + cos(x + j) + tan(x + k);
                result += sqrt(x + i + j + k) + log(x + i + j + k + 1);
                result += pow(x + i, 2.5) + exp(x * 0.01);
                
                // MAJOR PROBLEM: Redundant calculations
                if (k % 3 == 0) {
                    result += sin(x + i) + cos(x + j) + tan(x + k);  // Recalculating
                    result += sqrt(x + i + j + k) + log(x + i + j + k + 1);  // Recalculating
                }
            }
        }
    }
    
    return result;
}

// Nested loops function - demonstrates severe nested loop problems
double nested_cpu_problem(double x) {
    double result = 0.0;
    
    // MAJOR PROBLEM: Quadruple nested loops with expensive operations
    for (int i = 0; i < 100; ++i) {  // Increased from 20
        for (int j = 0; j < 100; ++j) {  // Increased from 20
            for (int k = 0; k < 50; ++k) {  // Increased from 5
                for (int l = 0; l < 20; ++l) {  // Added fourth level
                    // MAJOR PROBLEM: Expensive operations in innermost loop
                    result += sin(x + i + j + k + l) + cos(x + i + j + k + l);
                    result += tan(x + i + j + k + l) + log(x + i + j + k + l + 1);
                    result += sqrt(x + i + j + k + l + 1) + pow(x + i + j + k + l, 1.5);
                    
                    // MAJOR PROBLEM: Unnecessary string operations in loop
                    string temp = to_string(x + i + j + k + l);
                    result += temp.length();
                }
            }
        }
    }
    
    return result;
}

// Mathematical operations - demonstrates severe redundant calculations
double mathematical_problem(int value) {
    double x = value;
    double result = 0.0;
    
    // MAJOR PROBLEM: Massive loops with extreme redundancy
    for (int i = 0; i < 500; ++i) {  // Increased from 100
        // MAJOR PROBLEM: Recalculating same values repeatedly
        result += sin(x) + cos(x) + tan(x) + log(x + 1) + sqrt(x + 1);
        result += sin(x) + cos(x) + tan(x) + log(x + 1) + sqrt(x + 1);  // Duplicate
        result += sin(x) + cos(x) + tan(x) + log(x + 1) + sqrt(x + 1);  // Duplicate
        
        // MAJOR PROBLEM: Expensive operations in every iteration
        if (i % 5 == 0) {  // Changed from 10 to make it more frequent
            result += pow(x + i, 3.7) + exp(x * 0.1);
            result += sin(x + i) + cos(x + i) + tan(x + i);
            result += sqrt(x + i) + log(x + i + 1) + pow(x + i, 2.3);
        }
        
        // MAJOR PROBLEM: Nested loops with expensive operations
        for (int j = 0; j < 20; ++j) {  // Increased from 3
            for (int k = 0; k < 10; ++k) {  // Added another level
                result += sin(x + j + k) + cos(x + j + k) + tan(x + j + k);
                result += pow(x + j + k, 1.8) + exp((x + j + k) * 0.01);
            }
        }
    }
    
    return result;
}

// Redundant calculations function - demonstrates extreme redundancy
double redundant_calculations_problem(double x) {
    double result = 0.0;
    
    // MAJOR PROBLEM: Massive redundancy with expensive calculations
    for (int i = 0; i < 300; ++i) {  // Increased from 50
        // MAJOR PROBLEM: Same expensive calculations repeated 10 times
        for (int j = 0; j < 10; ++j) {
            double redundant = sin(x) + cos(x) + tan(x) + sqrt(x + 1) + log(x + 1);
            result += redundant;
        }
        
        // MAJOR PROBLEM: More redundant calculations
        if (i % 3 == 0) {  // Changed from 5 to make it more frequent
            result += pow(x, 3.2) + exp(x * 0.05);
            result += sin(x) + cos(x) + tan(x) + sqrt(x + 1) + log(x + 1);  // Recalculating
            result += pow(x, 3.2) + exp(x * 0.05);  // Recalculating
        }
        
        // MAJOR PROBLEM: Nested redundancy
        for (int k = 0; k < 5; ++k) {
            result += sin(x + k) + cos(x + k) + tan(x + k);
            result += sin(x + k) + cos(x + k) + tan(x + k);  // Duplicate
            result += sqrt(x + k + 1) + log(x + k + 1);
            result += sqrt(x + k + 1) + log(x + k + 1);  // Duplicate
        }
    }
    
    return result;
}

// Mutex operations - demonstrates thread synchronization
double mutex_operations(double x) {
    // Lock scope
    {
        lock_guard<mutex> lock(global_mutex);
        global_map[to_string(x)] = x * x;
    }
    
    // Work outside the lock
    return x * x + sin(x);
}

/*
 * SCENARIO 2: String Operations
 */

// String function - basic string operations
string string_operations(int value) {
    return to_string(value);
}

/*
 * SCENARIO 3: Call Patterns
 */

// Frequent function - demonstrates severe memory allocation problems
double frequent_function(double x) {
    double result = 0.0;
    
    // MAJOR PROBLEM: Excessive heap allocations in frequently called function
    for (int i = 0; i < 100; ++i) {
        // MAJOR PROBLEM: New vector allocation on every iteration
        vector<double> temp_vector;
        temp_vector.reserve(1000);  // Reserve but then...
        
        for (int j = 0; j < 1000; ++j) {
            temp_vector.push_back(x + i + j);  // ...push many elements
        }
        
        // MAJOR PROBLEM: More heap allocations
        vector<string> string_vector;
        for (int k = 0; k < 100; ++k) {
            string_vector.push_back("frequent_" + to_string(x + i + k));
        }
        
        // MAJOR PROBLEM: Expensive operations on allocated data
        for (auto& val : temp_vector) {
            result += sin(val) + cos(val) + sqrt(val + 1);
        }
        
        for (auto& str : string_vector) {
            result += str.length();
        }
    }
    
    // MAJOR PROBLEM: String concatenation without reserve
    string final_str = "";
    for (int i = 0; i < 200; ++i) {  // Increased from 20
        final_str += "_frequent_" + to_string(x + i);  // Causes reallocations
    }
    
    return result + final_str.length();
}

// Moderate function - demonstrates severe matrix problems
double moderate_function(double x) {
    double result = 0.0;
    
    // MAJOR PROBLEM: Large matrix with expensive operations
    vector<vector<double>> matrix(200, vector<double>(200));  // Increased from 50x50
    
    // MAJOR PROBLEM: Expensive initialization
    for (int i = 0; i < 200; ++i) {
        for (int j = 0; j < 200; ++j) {
            // MAJOR PROBLEM: Expensive calculations during initialization
            matrix[i][j] = sin(x + i) + cos(x + j) + sqrt(x + i + j);
        }
    }
    
    // MAJOR PROBLEM: Multiple passes over matrix with expensive operations
    for (int pass = 0; pass < 5; ++pass) {  // Multiple passes
        for (int i = 0; i < 200; ++i) {
            for (int j = 0; j < 200; ++j) {
                // MAJOR PROBLEM: Expensive operations in nested loops
                result += sin(matrix[i][j]) + cos(matrix[i][j]) + tan(matrix[i][j]);
                result += sqrt(matrix[i][j] + 1) + log(matrix[i][j] + 1);
                result += pow(matrix[i][j], 2.3) + exp(matrix[i][j] * 0.01);
                
                // MAJOR PROBLEM: Cache-unfriendly access pattern
                if (j % 2 == 0) {
                    result += matrix[j][i];  // Transpose access
                }
            }
        }
    }
    
    return result;
}

// Rare function - demonstrates large data operations
double rare_function(double x) {
    // Large vector allocation
    vector<double> huge_vector;
    huge_vector.reserve(10000);
    
    for (int i = 0; i < 10000; ++i) {
        huge_vector.push_back(x + i);
    }
    
    // Vector calculations
    double result = 0.0;
    for (double val : huge_vector) {
        result += math_cache.fast_sin(val) + math_cache.fast_cos(val) + tan(val);
    }
    
    // String operations
    string str = "rare_" + to_string(static_cast<int>(x)) + "_";
    str.reserve(str.size() + 1000);
    
    return result + str.length();
}

/*
 * Main test functions - demonstrating performance issues
 */

void test_problem_functions(int iterations) {
    cout << "Testing problem functions with " << iterations << " iterations..." << endl;
    cout << "This demonstrates performance issues and inefficiencies." << endl;
    
    auto start = high_resolution_clock::now();
    double sum = 0.0;
    
    for (int i = 0; i < iterations; ++i) {
        double val = dis(gen);
        
        // Call problem functions
        sum += cpu_intensive_problem(val);
        sum += nested_cpu_problem(val);
        sum += redundant_calculations_problem(val);
        sum += mutex_operations(val);
        
        if (i % 10 == 0) {
            double cpu_result = mathematical_problem(i);
            sum += cpu_result;
        }
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "Problem functions result: " << sum << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << endl;
}

void test_call_frequency_patterns(int iterations) {
    cout << "Testing call frequency patterns with " << iterations << " iterations..." << endl;
    cout << "This demonstrates different call frequency scenarios." << endl;
    
    auto start = high_resolution_clock::now();
    double sum = 0.0;
    
    for (int i = 0; i < iterations; ++i) {
        double val = dis(gen);
        
        // Frequent function
        sum += frequent_function(val);
        
        // Moderate function
        if (i % 10 == 0) {
            sum += moderate_function(val);
        }
        
        // Rare function
        if (i % 100 == 0) {
            sum += rare_function(val);
        }
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "Call frequency test result: " << sum << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << endl;
}

void test_nested_function_calls(int iterations) {
    cout << "Testing nested function calls with " << iterations << " iterations..." << endl;
    cout << "This demonstrates deep call stack scenarios." << endl;
    
    auto start = high_resolution_clock::now();
    double sum = 0.0;
    
    for (int i = 0; i < iterations; ++i) {
        double val = dis(gen);
        
        // Nested call stack
        sum += nested_level_1(val, i);
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "Nested calls result: " << sum << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << endl;
}

// Nested function calls - demonstrates deep call stacks
double nested_level_1(double x, int depth) {
    double result = x * x + sin(x);
    return nested_level_2(x * 1.1, depth + 1);
}

double nested_level_2(double x, int depth) {
    double result = x * x + cos(x);
    return nested_level_3(x * 1.2, depth + 1);
}

double nested_level_3(double x, int depth) {
    double result = x * x + tan(x);
    return nested_level_4(x * 1.3, depth + 1);
}

double nested_level_4(double x, int depth) {
    double result = x * x + log(x + 1);
    return nested_level_5(x * 1.4, depth + 1);
}

double nested_level_5(double x, int depth) {
    double result = x * x + sqrt(x + 1);
    return result + sin(x) + depth;
}

/*
 * PARALLEL THREADS - Demonstrates threading scenarios
 */

// Thread 1: CPU-intensive operations
void cpu_intensive_thread(int thread_id) {
    cout << "Thread " << thread_id << " starting CPU-intensive operations..." << endl;
    
    random_device thread_rd;
    mt19937 thread_gen(thread_rd());
    uniform_real_distribution<double> thread_dis(0.0, 1000.0);
    
    double thread_sum = 0.0;
    int operation_count = 0;
    auto start_time = high_resolution_clock::now();
    
    while (true) {
        double val = thread_dis(thread_gen);
        
        // MAJOR PROBLEM: Severe CPU-intensive operations in threads
        for (int i = 0; i < 500; ++i) {  // Increased from 100
            double temp = val + i;
            
            // MAJOR PROBLEM: Expensive operations in every iteration
            thread_sum += sin(temp) + cos(temp) + tan(temp) + sqrt(temp + 1) + log(temp + 1);
            thread_sum += pow(temp, 2.5) + exp(temp * 0.01);
            
            // MAJOR PROBLEM: Redundant calculations
            if (i % 3 == 0) {  // Changed from 5 to make it more frequent
                thread_sum += sin(temp) + cos(temp) + tan(temp);  // Recalculating
                thread_sum += sqrt(temp + 1) + log(temp + 1) + pow(temp, 1.8);  // Recalculating
            }
            
            // MAJOR PROBLEM: Nested loops in thread
            for (int j = 0; j < 10; ++j) {
                thread_sum += sin(temp + j) + cos(temp + j);
            }
            
            operation_count++;
        }
        
        // Stop after 90 seconds
        auto current_time = high_resolution_clock::now();
        auto duration = duration_cast<seconds>(current_time - start_time);
        if (duration.count() >= 90) {
            cout << "Thread " << thread_id << " completed " << operation_count << " math operations. Sum: " << thread_sum << endl;
            break;
        }
        
        if (operation_count % 50000 == 0) {
            cout << "Thread " << thread_id << " completed " << operation_count << " math operations. Sum: " << thread_sum << endl;
        }
    }
}

// Thread 2: Nested loops
void nested_loops_thread(int thread_id) {
    cout << "Thread " << thread_id << " starting nested loops..." << endl;
    
    random_device thread_rd;
    mt19937 thread_gen(thread_rd());
    uniform_real_distribution<double> thread_dis(0.0, 1000.0);
    
    double thread_sum = 0.0;
    int operation_count = 0;
    auto start_time = high_resolution_clock::now();
    
    while (true) {
        double val = thread_dis(thread_gen);
        
        // Nested loops
        for (int i = 0; i < 3; ++i) {
            for (int j = 0; j < 3; ++j) {
                double temp = val + i + j;
                
                // Operations in nested loops
                thread_sum += temp * temp;
                
                operation_count++;
            }
        }
        
        // Stop after 90 seconds
        auto current_time = high_resolution_clock::now();
        auto duration = duration_cast<seconds>(current_time - start_time);
        if (duration.count() >= 90) {
            cout << "Thread " << thread_id << " completed " << operation_count << " nested operations. Sum: " << thread_sum << endl;
            break;
        }
        
        if (operation_count % 50000 == 0) {
            cout << "Thread " << thread_id << " completed " << operation_count << " nested operations. Sum: " << thread_sum << endl;
        }
    }
}

// Thread 3: Mutex operations
void mutex_contention_thread(int thread_id) {
    cout << "Thread " << thread_id << " starting mutex operations..." << endl;
    
    random_device thread_rd;
    mt19937 thread_gen(thread_rd());
    uniform_real_distribution<double> thread_dis(0.0, 1000.0);
    
    double thread_sum = 0.0;
    int operation_count = 0;
    auto start_time = high_resolution_clock::now();
    
    while (true) {
        double val = thread_dis(thread_gen);
        
        // Mutex operations
        for (int i = 0; i < 10; ++i) {
            // Lock scope
            {
                lock_guard<mutex> lock(global_mutex);
                global_map["thread_" + to_string(thread_id) + "_" + to_string(operation_count)] = val;
            }
            
            // Work outside the lock
            thread_sum += val * val;
            
            operation_count++;
        }
        
        // Stop after 90 seconds
        auto current_time = high_resolution_clock::now();
        auto duration = duration_cast<seconds>(current_time - start_time);
        if (duration.count() >= 90) {
            cout << "Thread " << thread_id << " completed " << operation_count << " mutex operations. Sum: " << thread_sum << endl;
            break;
        }
        
        if (operation_count % 50000 == 0) {
            cout << "Thread " << thread_id << " completed " << operation_count << " mutex operations. Sum: " << thread_sum << endl;
        }
    }
}

// Thread 4: Redundant calculations
void redundant_calculations_thread(int thread_id) {
    cout << "Thread " << thread_id << " starting calculations..." << endl;
    
    random_device thread_rd;
    mt19937 thread_gen(thread_rd());
    uniform_real_distribution<double> thread_dis(0.0, 1000.0);
    
    double thread_sum = 0.0;
    int operation_count = 0;
    auto start_time = high_resolution_clock::now();
    
    while (true) {
        double val = thread_dis(thread_gen);
        
        // Calculations
        for (int i = 0; i < 10; ++i) {
            // Calculate once, use multiple times
            double sin_val = sin(val + i);
            double cos_val = cos(val + i);
            
            // Use precomputed values
            thread_sum += sin_val + cos_val;
            thread_sum += sin_val + cos_val;
            
            operation_count++;
        }
        
        // Stop after 90 seconds
        auto current_time = high_resolution_clock::now();
        auto duration = duration_cast<seconds>(current_time - start_time);
        if (duration.count() >= 90) {
            cout << "Thread " << thread_id << " completed " << operation_count << " calculations. Sum: " << thread_sum << endl;
            break;
        }
        
        if (operation_count % 50000 == 0) {
            cout << "Thread " << thread_id << " completed " << operation_count << " calculations. Sum: " << thread_sum << endl;
        }
    }
}

// Function that spawns profiling threads
void start_profiling_threads() {
    cout << "=== PROFILING THREADS STARTING ===" << endl;
    cout << "Each thread focuses on different profiling scenarios:" << endl;
    cout << "1. CPU-intensive mathematical operations" << endl;
    cout << "2. Nested loops with calculations" << endl;
    cout << "3. Mutex operations with contention" << endl;
    cout << "4. Calculations with redundancy" << endl;
    cout << endl;
    cout << "This will demonstrate performance characteristics!" << endl;
    cout << endl;
    
    // Clear worker threads vector
    worker_threads.clear();
    
    // Spawn threads for different scenarios
    worker_threads.emplace_back(cpu_intensive_thread, 1);
    worker_threads.emplace_back(nested_loops_thread, 2);
    worker_threads.emplace_back(mutex_contention_thread, 3);
    worker_threads.emplace_back(redundant_calculations_thread, 4);
    
    // Add more threads
    for (int i = 5; i <= DEFAULT_NUM_THREADS; ++i) {
        int thread_type = (i - 1) % 4 + 1;  // Cycle through thread types
        switch (thread_type) {
            case 1:
                worker_threads.emplace_back(cpu_intensive_thread, i);
                break;
            case 2:
                worker_threads.emplace_back(nested_loops_thread, i);
                break;
            case 3:
                worker_threads.emplace_back(mutex_contention_thread, i);
                break;
            case 4:
                worker_threads.emplace_back(redundant_calculations_thread, i);
                break;
        }
    }
    
    cout << "Started " << DEFAULT_NUM_THREADS << " threads!" << endl;
    cout << "Thread breakdown:" << endl;
    cout << "- " << (DEFAULT_NUM_THREADS / 4) << " CPU-intensive mathematical operation threads" << endl;
    cout << "- " << (DEFAULT_NUM_THREADS / 4) << " nested loops threads" << endl;
    cout << "- " << (DEFAULT_NUM_THREADS / 4) << " mutex operation threads" << endl;
    cout << "- " << (DEFAULT_NUM_THREADS / 4) << " calculation threads" << endl;
    cout << endl;
    cout << "CPU usage will demonstrate performance characteristics!" << endl;
    cout << "All threads will automatically stop after 90 seconds!" << endl;
    cout << endl;
    
    // Wait for all threads to complete
    for (auto& t : worker_threads) {
        t.join();
    }
    
    cout << "All threads completed! Profiling session finished." << endl;
}

int main() {
    cout << "=== PROFILING THREADS ===" << endl;
    cout << "NOTE: This program will run " << DEFAULT_NUM_THREADS << " threads!" << endl;
    cout << "Each thread focuses on different profiling scenarios:" << endl;
    cout << "1. CPU-intensive mathematical operations" << endl;
    cout << "2. Nested loops with calculations" << endl;
    cout << "3. Mutex operations with contention" << endl;
    cout << "4. Calculations with redundancy" << endl;
    cout << endl;
    cout << "This will demonstrate performance characteristics!" << endl;
    cout << endl;
    
    // Initialize global data
    cout << "Initializing global data structures..." << endl;
    global_matrix.resize(MATRIX_SIZE);
    for (int i = 0; i < MATRIX_SIZE; ++i) {
        global_matrix[i].resize(MATRIX_SIZE);
        for (int j = 0; j < MATRIX_SIZE; ++j) {
            global_matrix[i][j] = dis(gen);
        }
    }
    
    global_strings.resize(STRING_COUNT);
    for (int i = 0; i < STRING_COUNT; ++i) {
        global_strings[i] = "string_" + to_string(i);
    }
    
    cout << "Global data initialized." << endl;
    cout << endl;
    
    // START PROFILING THREADS IMMEDIATELY
    start_profiling_threads();
    
    cout << "=== PROFILING ANALYSIS NOTES ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Compare with the solved version - observe performance differences!" << endl;
    cout << "3. Look for functions with high call counts and individual time consumption" << endl;
    cout << "4. Analyze memory allocation patterns - identify inefficiencies!" << endl;
    cout << "5. Examine cache hit patterns - observe cache misses" << endl;
    cout << "6. Look for mutex contention and thread synchronization issues" << endl;
    cout << "7. Focus on 'Hot Paths' - functions consuming most time" << endl;
    cout << "8. Check call graph for deep call stacks and expensive operations" << endl;
    cout << endl;
    cout << "Key Profiling Concepts Demonstrated:" << endl;
    cout << "- Memory allocation overhead with inefficient patterns" << endl;
    cout << "- Cache misses with unfriendly access patterns" << endl;
    cout << "- String operations with frequent reallocations" << endl;
    cout << "- Redundant calculations and repeated expensive operations" << endl;
    cout << "- Mutex locking with contention and blocking" << endl;
    cout << "- Deep call stacks with expensive operations" << endl;
    cout << "- PARALLEL THREADS with resource contention" << endl;
    cout << "- Multiple threads competing for shared resources" << endl;
    cout << "- Atomic operations and shared data synchronization overhead" << endl;
    cout << "- Instrumentation reveals actual costs vs estimates" << endl;
    cout << "- Small inefficiencies become significant bottlenecks at scale" << endl;
    cout << "- Multi-threading demonstrating performance characteristics" << endl;
    
    return 0;
}