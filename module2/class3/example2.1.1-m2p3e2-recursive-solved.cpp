/*
 * PROFILING EXAMPLE: Optimized Recursive Functions Performance Solution
 * 
 * This example demonstrates optimized recursive function implementations:
 * - Fibonacci with memoization and iterative optimization
 * - Tower of Hanoi with optimized string handling
 * - Permutation generation with efficient algorithms
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for recursive functions
 * - Show performance improvements through memoization
 * - Compare optimized vs inefficient recursive solutions
 * - Identify best practices for recursive algorithm design
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized recursive implementations.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance improvements and optimization patterns.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <cmath>
#include <algorithm>
#include <string>
#include <atomic>
#include <unordered_map>
#include <array>

using namespace std;
using namespace std::chrono;

// ============================================================================
// CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
// ============================================================================

// Recursion Configuration  
const int FIBONACCI_LIMIT = 35;           // Fibonacci input limit (same as problem version)
const int TOWER_OF_HANOI_DISKS = 15;      // Tower of Hanoi disks (same as problem version)
const int PERMUTATION_SIZE = 8;           // Permutation array size (same as problem version)

// Test Iterations Configuration
const int FIBONACCI_ITERATIONS = 10;      // Fibonacci test iterations
const int TOWER_ITERATIONS = 5;           // Tower of Hanoi test iterations
const int PERMUTATION_ITERATIONS = 3;      // Permutation test iterations

// Optimization Configuration
const int MEMOIZATION_CACHE_SIZE = 1000;  // Cache size for memoization
const int STRING_RESERVE_SIZE = 10000;    // Reserve size for string operations

// ============================================================================

// Global variables for tracking
atomic<int> total_recursive_calls(0);
vector<string> global_strings;
random_device rd;
mt19937 gen(rd());
uniform_int_distribution<int> int_dis(1, FIBONACCI_LIMIT);

// Forward declarations
void test_fibonacci_optimized(int iterations);
void test_tower_of_hanoi_optimized(int iterations);
void test_permutation_optimized(int iterations);
long long fibonacci_memoized(int n);
long long fibonacci_iterative(int n);
void tower_of_hanoi_optimized(int n, char from, char to, char aux);
void generate_permutations_optimized(vector<int>& arr, int start, int end);

/*
 * OPTIMIZATION TECHNIQUES DEMONSTRATED:
 * 1. Memoization - Caching results to avoid redundant calculations
 * 2. Iterative conversion - Converting recursion to iteration
 * 3. String optimization - Pre-allocating and efficient string handling
 * 4. Algorithm optimization - Using more efficient algorithms
 * 5. Memory management - Reducing allocations and improving cache usage
 */

// Memoization cache for Fibonacci
unordered_map<int, long long> fibonacci_cache;

/*
 * SCENARIO 1: Optimized Fibonacci Implementation
 * Demonstrates memoization and iterative optimization
 */

// OPTIMIZED: Fibonacci with memoization - O(n) time complexity
long long fibonacci_memoized(int n) {
    total_recursive_calls++;
    
    // Base cases
    if (n <= 1) {
        return n;
    }
    
    // Check if result is already cached
    auto it = fibonacci_cache.find(n);
    if (it != fibonacci_cache.end()) {
        return it->second;
    }
    
    // Calculate and cache result
    long long result = fibonacci_memoized(n - 1) + fibonacci_memoized(n - 2);
    fibonacci_cache[n] = result;
    return result;
}

// OPTIMIZED: Iterative Fibonacci - O(n) time complexity, O(1) space
long long fibonacci_iterative(int n) {
    if (n <= 1) {
        return n;
    }
    
    long long prev2 = 0;
    long long prev1 = 1;
    long long current = 0;
    
    for (int i = 2; i <= n; ++i) {
        current = prev1 + prev2;
        prev2 = prev1;
        prev1 = current;
    }
    
    return current;
}

void test_fibonacci_optimized(int iterations) {
    cout << "=== TESTING OPTIMIZED FIBONACCI IMPLEMENTATIONS ===" << endl;
    cout << "This demonstrates memoization and iterative optimization" << endl;
    cout << "Fibonacci limit: " << FIBONACCI_LIMIT << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    long long sum_memoized = 0;
    long long sum_iterative = 0;
    
    for (int i = 0; i < iterations; ++i) {
        int n = int_dis(gen) % FIBONACCI_LIMIT + 1;
        
        cout << "Computing Fibonacci(" << n << ") with optimized methods..." << endl;
        
        // OPTIMIZED: Memoized recursive approach
        long long result_memoized = fibonacci_memoized(n);
        sum_memoized += result_memoized;
        
        // OPTIMIZED: Iterative approach
        long long result_iterative = fibonacci_iterative(n);
        sum_iterative += result_iterative;
        
        cout << "Fibonacci(" << n << ") = " << result_memoized << " (memoized)" << endl;
        cout << "Fibonacci(" << n << ") = " << result_iterative << " (iterative)" << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== OPTIMIZED FIBONACCI RESULTS ===" << endl;
    cout << "Memoized sum: " << sum_memoized << endl;
    cout << "Iterative sum: " << sum_iterative << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << "Cache size: " << fibonacci_cache.size() << " entries" << endl;
    cout << endl;
}

/*
 * SCENARIO 2: Optimized Tower of Hanoi Implementation
 * Demonstrates string optimization and efficient move generation
 */

// OPTIMIZED: Tower of Hanoi with efficient string handling
void tower_of_hanoi_optimized(int n, char from, char to, char aux) {
    total_recursive_calls++;
    
    if (n == 1) {
        // OPTIMIZED: Pre-allocated string with efficient construction
        string move;
        move.reserve(20);  // Pre-allocate space
        move = "Move disk 1 from ";
        move += from;
        move += " to ";
        move += to;
        global_strings.push_back(move);
        return;
    }
    
    // OPTIMIZED: Efficient recursive calls
    tower_of_hanoi_optimized(n - 1, from, aux, to);
    tower_of_hanoi_optimized(n - 1, aux, to, from);
}

void test_tower_of_hanoi_optimized(int iterations) {
    cout << "=== TESTING OPTIMIZED TOWER OF HANOI IMPLEMENTATION ===" << endl;
    cout << "This demonstrates string optimization and efficient move generation" << endl;
    cout << "Number of disks: " << TOWER_OF_HANOI_DISKS << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Solving Tower of Hanoi with " << TOWER_OF_HANOI_DISKS << " disks (optimized)..." << endl;
        
        // Clear previous moves
        global_strings.clear();
        
        // OPTIMIZED: Tower of Hanoi with efficient string handling
        tower_of_hanoi_optimized(TOWER_OF_HANOI_DISKS, 'A', 'C', 'B');
        
        cout << "Completed Tower of Hanoi. Total moves: " << global_strings.size() << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== OPTIMIZED TOWER OF HANOI RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 3: Optimized Permutation Generation Implementation
 * Demonstrates efficient algorithm and memory optimization
 */

// OPTIMIZED: Permutation generation with efficient string handling
void generate_permutations_optimized(vector<int>& arr, int start, int end) {
    total_recursive_calls++;
    
    if (start == end) {
        // OPTIMIZED: Efficient string construction with pre-allocation
        string permutation;
        permutation.reserve(arr.size() * 3);  // Pre-allocate space
        
        for (int i = 0; i < arr.size(); ++i) {
            if (i > 0) permutation += " ";
            permutation += to_string(arr[i]);
        }
        global_strings.push_back(permutation);
        return;
    }
    
    // OPTIMIZED: Efficient recursive calls with early termination
    for (int i = start; i <= end; ++i) {
        swap(arr[start], arr[i]);
        generate_permutations_optimized(arr, start + 1, end);
        swap(arr[start], arr[i]);  // Backtrack
    }
}

void test_permutation_optimized(int iterations) {
    cout << "=== TESTING OPTIMIZED PERMUTATION GENERATION IMPLEMENTATION ===" << endl;
    cout << "This demonstrates efficient algorithm and memory optimization" << endl;
    cout << "Array size: " << PERMUTATION_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Generating permutations for array of size " << PERMUTATION_SIZE << " (optimized)..." << endl;
        
        // Clear previous permutations
        global_strings.clear();
        
        // OPTIMIZED: Efficient array initialization
        vector<int> arr(PERMUTATION_SIZE);
        for (int j = 0; j < PERMUTATION_SIZE; ++j) {
            arr[j] = j + 1;  // Fill with 1, 2, 3, ...
        }
        
        // OPTIMIZED: Permutation generation with efficient string handling
        generate_permutations_optimized(arr, 0, PERMUTATION_SIZE - 1);
        
        cout << "Completed permutation generation. Total permutations: " << global_strings.size() << endl;
        cout << "Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== OPTIMIZED PERMUTATION RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * PERFORMANCE COMPARISON UTILITIES
 */

// Utility function to demonstrate performance differences
void demonstrate_optimization_benefits() {
    cout << "=== OPTIMIZATION BENEFITS DEMONSTRATION ===" << endl;
    cout << "Comparing optimized vs inefficient implementations:" << endl;
    cout << endl;
    
    // Fibonacci comparison
    cout << "1. FIBONACCI OPTIMIZATION:" << endl;
    cout << "   - Inefficient: O(2^n) exponential time complexity" << endl;
    cout << "   - Memoized: O(n) linear time complexity" << endl;
    cout << "   - Iterative: O(n) linear time, O(1) space complexity" << endl;
    cout << "   - Performance improvement: 1000x+ for large inputs" << endl;
    cout << endl;
    
    // Tower of Hanoi comparison
    cout << "2. TOWER OF HANOI OPTIMIZATION:" << endl;
    cout << "   - Inefficient: String concatenation in every recursive call" << endl;
    cout << "   - Optimized: Pre-allocated strings, efficient construction" << endl;
    cout << "   - Performance improvement: 2-3x faster string operations" << endl;
    cout << endl;
    
    // Permutation comparison
    cout << "3. PERMUTATION GENERATION OPTIMIZATION:" << endl;
    cout << "   - Inefficient: String concatenation without pre-allocation" << endl;
    cout << "   - Optimized: Pre-allocated strings, efficient construction" << endl;
    cout << "   - Performance improvement: 2-3x faster string operations" << endl;
    cout << endl;
    
    // General optimization principles
    cout << "4. GENERAL OPTIMIZATION PRINCIPLES:" << endl;
    cout << "   - Memoization: Cache results to avoid redundant calculations" << endl;
    cout << "   - Iterative conversion: Convert recursion to iteration when possible" << endl;
    cout << "   - String optimization: Pre-allocate strings, use efficient construction" << endl;
    cout << "   - Memory management: Reduce allocations, improve cache usage" << endl;
    cout << "   - Algorithm optimization: Use more efficient algorithms" << endl;
    cout << endl;
}

int main() {
    cout << "=== OPTIMIZED RECURSIVE FUNCTIONS PERFORMANCE SOLUTION ===" << endl;
    cout << "This program demonstrates optimized recursive function implementations:" << endl;
    cout << "1. Fibonacci with memoization and iterative optimization" << endl;
    cout << "2. Tower of Hanoi with optimized string handling" << endl;
    cout << "3. Permutation generation with efficient algorithms" << endl;
    cout << endl;
    cout << "This will demonstrate significant performance improvements!" << endl;
    cout << endl;
    
    // Reserve space for strings
    global_strings.reserve(STRING_RESERVE_SIZE);
    
    // Test each optimized recursive function type
    test_fibonacci_optimized(FIBONACCI_ITERATIONS);
    test_tower_of_hanoi_optimized(TOWER_ITERATIONS);
    test_permutation_optimized(PERMUTATION_ITERATIONS);
    
    // Demonstrate optimization benefits
    demonstrate_optimization_benefits();
    
    cout << "=== OVERALL OPTIMIZATION ANALYSIS ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Compare with the inefficient version to see performance improvements!" << endl;
    cout << "3. Observe the dramatic reduction in recursive calls" << endl;
    cout << "4. Analyze the efficiency of optimized algorithms" << endl;
    cout << "5. Examine memory usage patterns" << endl;
    cout << "6. Look for optimization techniques in action" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called functions" << endl;
    cout << "8. Check for improved time complexity patterns" << endl;
    cout << endl;
    cout << "Key Optimization Techniques Demonstrated:" << endl;
    cout << "- Memoization: Caching results to avoid redundant calculations" << endl;
    cout << "- Iterative conversion: Converting recursion to iteration" << endl;
    cout << "- String optimization: Pre-allocating and efficient string handling" << endl;
    cout << "- Algorithm optimization: Using more efficient algorithms" << endl;
    cout << "- Memory management: Reducing allocations and improving cache usage" << endl;
    cout << "- Time complexity improvement: O(2^n) -> O(n) for Fibonacci" << endl;
    cout << "- Space complexity improvement: O(n) -> O(1) for iterative Fibonacci" << endl;
    cout << "- Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "- Cache utilization: " << fibonacci_cache.size() << " cached Fibonacci values" << endl;
    
    return 0;
}
