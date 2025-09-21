/*
 * PROFILING EXAMPLE: Classic Recursive Functions Performance Investigation
 * 
 * This example demonstrates severe recursive function performance issues:
 * - Fibonacci with exponential time complexity O(2^n)
 * - Tower of Hanoi with exponential complexity O(2^n)
 * - Permutation generation with factorial complexity O(n!)
 * 
 * OBJECTIVES:
 * - Measure recursive function impact via instrumentation
 * - Detect exponential growth in recursive calls
 * - Compare inefficient recursive vs optimized solutions
 * - Identify time differences and variance
 * - Prepare reflection on algorithm design
 * 
 * NOTE: This code intentionally contains severe recursive performance problems.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe recursive call patterns and performance bottlenecks.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <cmath>
#include <algorithm>
#include <string>
#include <atomic>

using namespace std;
using namespace std::chrono;

// ============================================================================
// CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
// ============================================================================

// Recursion Configuration  
const int FIBONACCI_LIMIT = 35;           // Fibonacci input limit (35 = shows exponential growth)
const int TOWER_OF_HANOI_DISKS = 15;      // Tower of Hanoi disks (15 = exponential complexity)
const int PERMUTATION_SIZE = 8;           // Permutation array size (8 = factorial complexity)

// Test Iterations Configuration
const int FIBONACCI_ITERATIONS = 10;      // Fibonacci test iterations
const int TOWER_ITERATIONS = 5;           // Tower of Hanoi test iterations
const int PERMUTATION_ITERATIONS = 3;      // Permutation test iterations

// ============================================================================

// Global variables for tracking
atomic<int> total_recursive_calls(0);
vector<string> global_strings;
random_device rd;
mt19937 gen(rd());
uniform_int_distribution<int> int_dis(1, FIBONACCI_LIMIT);

// Forward declarations
void test_fibonacci_recursive(int iterations);
void test_tower_of_hanoi_recursive(int iterations);
void test_permutation_recursive(int iterations);
long long fibonacci_recursive(int n);
void tower_of_hanoi_recursive(int n, char from, char to, char aux);
void generate_permutations_recursive(vector<int>& arr, int start, int end);

/*
 * SCENARIO 1: Fibonacci Recursive Function
 * Demonstrates exponential time complexity O(2^n)
 */

// MAJOR PROBLEM: Classic Fibonacci with exponential time complexity O(2^n)
long long fibonacci_recursive(int n) {
    total_recursive_calls++;
    
    // MAJOR PROBLEM: No base case optimization, redundant calculations
    if (n <= 1) {
        return n;
    }
    
    // MAJOR PROBLEM: Double recursive calls causing exponential growth
    return fibonacci_recursive(n - 1) + fibonacci_recursive(n - 2);
}

void test_fibonacci_recursive(int iterations) {
    cout << "=== TESTING FIBONACCI RECURSIVE FUNCTION ===" << endl;
    cout << "This demonstrates exponential time complexity O(2^n)" << endl;
    cout << "Fibonacci limit: " << FIBONACCI_LIMIT << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    long long sum = 0;
    
    for (int i = 0; i < iterations; ++i) {
        int n = int_dis(gen) % FIBONACCI_LIMIT + 1;
        
        cout << "Computing Fibonacci(" << n << ")..." << endl;
        
        // MAJOR PROBLEM: Call expensive recursive function
        long long result = fibonacci_recursive(n);
        sum += result;
        
        cout << "Fibonacci(" << n << ") = " << result << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== FIBONACCI RESULTS ===" << endl;
    cout << "Total sum: " << sum << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 2: Tower of Hanoi Recursive Function
 * Demonstrates exponential complexity O(2^n)
 */

// MAJOR PROBLEM: Tower of Hanoi with exponential complexity O(2^n)
void tower_of_hanoi_recursive(int n, char from, char to, char aux) {
    total_recursive_calls++;
    
    if (n == 1) {
        // MAJOR PROBLEM: Expensive string operations in every recursive call
        string move = "Move disk 1 from " + string(1, from) + " to " + string(1, to);
        global_strings.push_back(move);
        return;
    }
    
    // MAJOR PROBLEM: Triple recursive calls for each disk
    tower_of_hanoi_recursive(n - 1, from, aux, to);
    tower_of_hanoi_recursive(n - 1, aux, to, from);
}

void test_tower_of_hanoi_recursive(int iterations) {
    cout << "=== TESTING TOWER OF HANOI RECURSIVE FUNCTION ===" << endl;
    cout << "This demonstrates exponential complexity O(2^n)" << endl;
    cout << "Number of disks: " << TOWER_OF_HANOI_DISKS << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Solving Tower of Hanoi with " << TOWER_OF_HANOI_DISKS << " disks..." << endl;
        
        // Clear previous moves
        global_strings.clear();
        
        // MAJOR PROBLEM: Tower of Hanoi with high disk count
        tower_of_hanoi_recursive(TOWER_OF_HANOI_DISKS, 'A', 'C', 'B');
        
        cout << "Completed Tower of Hanoi. Total moves: " << global_strings.size() << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== TOWER OF HANOI RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 3: Permutation Generation Recursive Function
 * Demonstrates factorial complexity O(n!)
 */

// MAJOR PROBLEM: Permutation generation with factorial complexity O(n!)
void generate_permutations_recursive(vector<int>& arr, int start, int end) {
    total_recursive_calls++;
    
    if (start == end) {
        // MAJOR PROBLEM: Expensive operations for each permutation
        string permutation = "";
        for (int i = 0; i < arr.size(); ++i) {
            permutation += to_string(arr[i]) + " ";
        }
        global_strings.push_back(permutation);
        return;
    }
    
    // MAJOR PROBLEM: Recursive calls for each position
    for (int i = start; i <= end; ++i) {
        swap(arr[start], arr[i]);
        generate_permutations_recursive(arr, start + 1, end);
        swap(arr[start], arr[i]);  // Backtrack
    }
}

void test_permutation_recursive(int iterations) {
    cout << "=== TESTING PERMUTATION GENERATION RECURSIVE FUNCTION ===" << endl;
    cout << "This demonstrates factorial complexity O(n!)" << endl;
    cout << "Array size: " << PERMUTATION_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Generating permutations for array of size " << PERMUTATION_SIZE << "..." << endl;
        
        // Clear previous permutations
        global_strings.clear();
        
        // Create array
        vector<int> arr(PERMUTATION_SIZE);
        for (int j = 0; j < PERMUTATION_SIZE; ++j) {
            arr[j] = j + 1;
        }
        
        // MAJOR PROBLEM: Permutation generation with factorial complexity
        generate_permutations_recursive(arr, 0, PERMUTATION_SIZE - 1);
        
        cout << "Completed permutation generation. Total permutations: " << global_strings.size() << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== PERMUTATION RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

int main() {
    cout << "=== CLASSIC RECURSIVE FUNCTIONS PERFORMANCE INVESTIGATION ===" << endl;
    cout << "This program demonstrates severe recursive function performance issues:" << endl;
    cout << "1. Fibonacci recursive function (exponential complexity O(2^n))" << endl;
    cout << "2. Tower of Hanoi recursive function (exponential complexity O(2^n))" << endl;
    cout << "3. Permutation generation recursive function (factorial complexity O(n!))" << endl;
    cout << endl;
    cout << "This will demonstrate severe recursive performance issues!" << endl;
    cout << endl;
    
    // Reserve space for strings
    global_strings.reserve(100000);
    
    // Test each recursive function type
    test_fibonacci_recursive(FIBONACCI_ITERATIONS);
    test_tower_of_hanoi_recursive(TOWER_ITERATIONS);
    test_permutation_recursive(PERMUTATION_ITERATIONS);
    
    cout << "=== OVERALL ANALYSIS NOTES ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Observe the exponential growth in recursive calls!" << endl;
    cout << "3. Look for functions with extremely high call counts" << endl;
    cout << "4. Analyze call graph for recursive patterns" << endl;
    cout << "5. Examine time complexity differences" << endl;
    cout << "6. Look for redundant calculations in recursive calls" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called recursive functions" << endl;
    cout << "8. Check for exponential vs factorial time complexity patterns" << endl;
    cout << endl;
    cout << "Key Recursive Performance Issues Demonstrated:" << endl;
    cout << "- Exponential time complexity O(2^n) in Fibonacci and Tower of Hanoi" << endl;
    cout << "- Factorial complexity O(n!) in permutation generation" << endl;
    cout << "- Redundant calculations in recursive calls" << endl;
    cout << "- String operations causing memory allocation" << endl;
    cout << "- Multiple recursive calls per function" << endl;
    cout << "- No memoization or caching of recursive results" << endl;
    cout << "- Expensive operations in base cases and recursive cases" << endl;
    cout << "- Total recursive calls: " << total_recursive_calls.load() << endl;
    
    return 0;
}
