/*
 * PROFILING EXAMPLE: Deep Recursion Patterns Performance Investigation
 * 
 * This example demonstrates deep recursion performance issues:
 * - Deep nested function calls with stack overflow potential
 * - Recursive string operations with memory allocation
 * - Recursive memory allocation causing heap fragmentation
 * - Recursive mathematical calculations with expensive operations
 * 
 * OBJECTIVES:
 * - Measure deep recursion impact via instrumentation
 * - Detect deep call stacks and stack overflow potential
 * - Compare inefficient recursive vs optimized solutions
 * - Identify memory allocation patterns in recursion
 * - Prepare reflection on algorithm design
 * 
 * NOTE: This code intentionally contains severe recursive performance problems.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe deep recursive call patterns and performance bottlenecks.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <cmath>
#include <string>
#include <atomic>

using namespace std;
using namespace std::chrono;

// ============================================================================
// CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
// ============================================================================

// Recursion Configuration  
const int RECURSION_DEPTH_LIMIT = 20;     // Maximum recursion depth (20 = safe for stack)
const int DEEP_RECURSION_ITERATIONS = 5;  // Deep recursion test iterations
const int STRING_RECURSION_ITERATIONS = 3; // String recursion test iterations
const int MEMORY_RECURSION_ITERATIONS = 3; // Memory recursion test iterations
const int MATH_RECURSION_ITERATIONS = 5;   // Math recursion test iterations

// Data Structure Sizes Configuration
const int MEMORY_VECTOR_SIZE = 100;       // Vector size in recursive memory allocation

// ============================================================================

// Global variables for tracking
atomic<int> total_recursive_calls(0);
atomic<double> shared_result(0.0);
vector<string> global_strings;
random_device rd;
mt19937 gen(rd());
uniform_real_distribution<double> real_dis(0.0, 1000.0);

// Forward declarations
void test_deep_nested_calls(int iterations);
void test_recursive_string_operations(int iterations);
void test_recursive_memory_allocation(int iterations);
void test_recursive_mathematical_calculations(int iterations);
void nested_function_calls_recursive(int depth, int max_depth);
void recursive_string_operations(string& str, int depth, int max_depth);
void recursive_memory_allocation(int depth, int max_depth);
void recursive_mathematical_calculations(double x, int depth, int max_depth);

/*
 * SCENARIO 1: Deep Nested Function Calls
 * Demonstrates deep call stacks and stack overflow potential
 */

// MAJOR PROBLEM: Deep nested function calls
void nested_function_calls_recursive(int depth, int max_depth) {
    total_recursive_calls++;
    
    if (depth >= max_depth) {
        return;
    }
    
    // MAJOR PROBLEM: Expensive operations in every recursive call
    double result = sin(depth) + cos(depth) + tan(depth) + sqrt(depth + 1);
    double current = shared_result.load();
    shared_result.store(current + result);
    
    // MAJOR PROBLEM: Multiple recursive calls
    nested_function_calls_recursive(depth + 1, max_depth);
    nested_function_calls_recursive(depth + 2, max_depth);
    nested_function_calls_recursive(depth + 3, max_depth);
}

void test_deep_nested_calls(int iterations) {
    cout << "=== TESTING DEEP NESTED FUNCTION CALLS ===" << endl;
    cout << "This demonstrates deep call stacks and stack overflow potential" << endl;
    cout << "Recursion depth limit: " << RECURSION_DEPTH_LIMIT << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing deep nested calls (iteration " << (i + 1) << ")..." << endl;
        
        // MAJOR PROBLEM: Deep recursion with expensive operations
        nested_function_calls_recursive(0, RECURSION_DEPTH_LIMIT);
        
        cout << "Completed deep nested calls. Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << "Shared result: " << shared_result.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== DEEP NESTED CALLS RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 2: Recursive String Operations
 * Demonstrates memory allocation and string concatenation issues
 */

// MAJOR PROBLEM: Recursive string operations with memory allocation
void recursive_string_operations(string& str, int depth, int max_depth) {
    total_recursive_calls++;
    
    if (depth >= max_depth) {
        return;
    }
    
    // MAJOR PROBLEM: String concatenation in every recursive call
    str += "_recursive_" + to_string(depth);
    
    // MAJOR PROBLEM: Multiple recursive calls
    recursive_string_operations(str, depth + 1, max_depth);
    recursive_string_operations(str, depth + 1, max_depth);
}

void test_recursive_string_operations(int iterations) {
    cout << "=== TESTING RECURSIVE STRING OPERATIONS ===" << endl;
    cout << "This demonstrates memory allocation and string concatenation issues" << endl;
    cout << "Recursion depth limit: " << RECURSION_DEPTH_LIMIT / 2 << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing recursive string operations (iteration " << (i + 1) << ")..." << endl;
        
        // MAJOR PROBLEM: Recursive string operations
        string str = "deep_recursion_";
        recursive_string_operations(str, 0, RECURSION_DEPTH_LIMIT / 2);
        
        cout << "Completed recursive string operations. String length: " << str.length() << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== RECURSIVE STRING OPERATIONS RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 3: Recursive Memory Allocation
 * Demonstrates heap allocation and memory fragmentation
 */

// MAJOR PROBLEM: Recursive memory allocation
void recursive_memory_allocation(int depth, int max_depth) {
    total_recursive_calls++;
    
    if (depth >= max_depth) {
        return;
    }
    
    // MAJOR PROBLEM: Heap allocation in every recursive call
    vector<double> temp_vector(MEMORY_VECTOR_SIZE);
    for (int i = 0; i < MEMORY_VECTOR_SIZE; ++i) {
        temp_vector[i] = sin(depth + i) + cos(depth + i);
    }
    
    // MAJOR PROBLEM: Multiple recursive calls
    recursive_memory_allocation(depth + 1, max_depth);
    recursive_memory_allocation(depth + 1, max_depth);
}

void test_recursive_memory_allocation(int iterations) {
    cout << "=== TESTING RECURSIVE MEMORY ALLOCATION ===" << endl;
    cout << "This demonstrates heap allocation and memory fragmentation" << endl;
    cout << "Recursion depth limit: " << RECURSION_DEPTH_LIMIT / 3 << endl;
    cout << "Vector size per allocation: " << MEMORY_VECTOR_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing recursive memory allocation (iteration " << (i + 1) << ")..." << endl;
        
        // MAJOR PROBLEM: Recursive memory allocation
        recursive_memory_allocation(0, RECURSION_DEPTH_LIMIT / 3);
        
        cout << "Completed recursive memory allocation. Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== RECURSIVE MEMORY ALLOCATION RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 4: Recursive Mathematical Calculations
 * Demonstrates expensive operations in recursive calls
 */

// MAJOR PROBLEM: Recursive mathematical calculations
void recursive_mathematical_calculations(double x, int depth, int max_depth) {
    total_recursive_calls++;
    
    if (depth >= max_depth) {
        return;
    }
    
    // MAJOR PROBLEM: Expensive calculations in every recursive call
    double result = sin(x + depth) + cos(x + depth) + tan(x + depth);
    result += sqrt(x + depth + 1) + log(x + depth + 1);
    result += pow(x + depth, 2.5) + exp(x * 0.01);
    
    double current = shared_result.load();
    shared_result.store(current + result);
    
    // MAJOR PROBLEM: Multiple recursive calls
    recursive_mathematical_calculations(x * 1.1, depth + 1, max_depth);
    recursive_mathematical_calculations(x * 1.2, depth + 1, max_depth);
}

void test_recursive_mathematical_calculations(int iterations) {
    cout << "=== TESTING RECURSIVE MATHEMATICAL CALCULATIONS ===" << endl;
    cout << "This demonstrates expensive operations in recursive calls" << endl;
    cout << "Recursion depth limit: " << RECURSION_DEPTH_LIMIT / 2 << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        double val = real_dis(gen);
        cout << "Testing recursive mathematical calculations (iteration " << (i + 1) << ", x=" << val << ")..." << endl;
        
        // MAJOR PROBLEM: Recursive mathematical calculations
        recursive_mathematical_calculations(val, 0, RECURSION_DEPTH_LIMIT / 2);
        
        cout << "Completed recursive mathematical calculations. Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << "Shared result: " << shared_result.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== RECURSIVE MATHEMATICAL CALCULATIONS RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

int main() {
    cout << "=== DEEP RECURSION PATTERNS PERFORMANCE INVESTIGATION ===" << endl;
    cout << "This program demonstrates deep recursion performance issues:" << endl;
    cout << "1. Deep nested function calls (stack overflow potential)" << endl;
    cout << "2. Recursive string operations (memory allocation)" << endl;
    cout << "3. Recursive memory allocation (heap fragmentation)" << endl;
    cout << "4. Recursive mathematical calculations (expensive operations)" << endl;
    cout << endl;
    cout << "This will demonstrate severe deep recursion performance issues!" << endl;
    cout << endl;
    
    // Reserve space for strings
    global_strings.reserve(100000);
    
    // Test each deep recursion pattern
    test_deep_nested_calls(DEEP_RECURSION_ITERATIONS);
    test_recursive_string_operations(STRING_RECURSION_ITERATIONS);
    test_recursive_memory_allocation(MEMORY_RECURSION_ITERATIONS);
    test_recursive_mathematical_calculations(MATH_RECURSION_ITERATIONS);
    
    cout << "=== OVERALL ANALYSIS NOTES ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Observe the deep recursion patterns!" << endl;
    cout << "3. Look for functions with deep call stacks" << endl;
    cout << "4. Analyze call graph for deep recursive patterns" << endl;
    cout << "5. Examine stack usage and potential overflow" << endl;
    cout << "6. Look for memory allocation patterns in recursive calls" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called recursive functions" << endl;
    cout << "8. Check for expensive operations in recursive calls" << endl;
    cout << endl;
    cout << "Key Deep Recursion Performance Issues Demonstrated:" << endl;
    cout << "- Deep recursion causing stack overflow potential" << endl;
    cout << "- Memory allocation in every recursive call" << endl;
    cout << "- String operations causing memory fragmentation" << endl;
    cout << "- Multiple recursive calls per function" << endl;
    cout << "- Expensive operations in recursive calls" << endl;
    cout << "- No optimization of recursive patterns" << endl;
    cout << "- Total recursive calls: " << total_recursive_calls.load() << endl;
    
    return 0;
}
