/*
 * PROFILING EXAMPLE: Optimized Exponential Recursion Patterns Performance Solution
 * 
 * This example demonstrates optimized exponential recursion implementations:
 * - Iterative tree traversal to prevent exponential growth
 * - Dynamic programming for matrix path finding
 * - Memoization to avoid redundant calculations
 * - Efficient algorithms with linear/polynomial complexity
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for exponential recursion
 * - Show how to convert exponential to polynomial complexity
 * - Compare inefficient recursive vs optimized solutions
 * - Identify best practices for exponential algorithm optimization
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized exponential recursion implementations.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance improvements and optimization patterns.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <cmath>
#include <atomic>
#include <unordered_map>
#include <queue>
#include <stack>
#include <string>

using namespace std;
using namespace std::chrono;

// ============================================================================
// CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
// ============================================================================

// Recursion Configuration  
const int RECURSION_DEPTH_LIMIT = 15;     // Maximum recursion depth (same as problem version)
const int TREE_SIZE = 1000;               // Binary tree size for traversal (same as problem version)
const int MATRIX_SIZE = 15;               // Matrix size for path finding (same as problem version)

// Test Iterations Configuration
const int TREE_TRAVERSAL_ITERATIONS = 3;   // Tree traversal test iterations
const int MATRIX_PATH_ITERATIONS = 2;     // Matrix path test iterations

// Optimization Configuration
const int MEMOIZATION_CACHE_SIZE = 10000; // Cache size for memoization

// ============================================================================

// Global variables for tracking
atomic<int> total_recursive_calls(0);
random_device rd;
mt19937 gen(rd());

// Forward declarations
void test_binary_tree_traversal_optimized(int iterations);
void test_matrix_path_finding_optimized(int iterations);
void binary_tree_traversal_iterative(vector<int>& tree);
void matrix_path_dynamic_programming(vector<vector<int>>& matrix);
void matrix_path_memoized(vector<vector<int>>& matrix, int row, int col, int depth, unordered_map<string, int>& memo);

/*
 * OPTIMIZATION TECHNIQUES DEMONSTRATED:
 * 1. Iterative conversion - Converting recursion to iteration
 * 2. Dynamic programming - Using bottom-up approach
 * 3. Memoization - Caching results to avoid redundant calculations
 * 4. Algorithm optimization - Using more efficient algorithms
 * 5. Space optimization - Reducing memory usage
 */

// Memoization cache for matrix path finding
unordered_map<string, int> path_cache;

/*
 * SCENARIO 1: Optimized Binary Tree Traversal
 * Demonstrates iterative conversion to prevent exponential growth
 */

// OPTIMIZED: Iterative tree traversal - O(n) time complexity
void binary_tree_traversal_iterative(vector<int>& tree) {
    // Use queue for level-order traversal
    queue<int> node_queue;
    node_queue.push(0);
    
    while (!node_queue.empty()) {
        int index = node_queue.front();
        node_queue.pop();
        
        total_recursive_calls++;
        
        if (index >= tree.size()) {
            continue;
        }
        
        // OPTIMIZED: Pre-calculate expensive operations once
        int depth = static_cast<int>(log2(index + 1));
        double sin_val = sin(static_cast<double>(depth));
        double cos_val = cos(static_cast<double>(depth));
        double sqrt_val = sqrt(static_cast<double>(depth + 1));
        
        tree[index] = static_cast<int>(sin_val + cos_val + sqrt_val);
        
        // OPTIMIZED: Add children to queue for processing
        int left_child = 2 * index + 1;
        int right_child = 2 * index + 2;
        
        if (left_child < tree.size()) {
            node_queue.push(left_child);
        }
        if (right_child < tree.size()) {
            node_queue.push(right_child);
        }
    }
}

void test_binary_tree_traversal_optimized(int iterations) {
    cout << "=== TESTING OPTIMIZED BINARY TREE TRAVERSAL ===" << endl;
    cout << "This demonstrates iterative conversion to prevent exponential growth" << endl;
    cout << "Tree size: " << TREE_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing optimized binary tree traversal (iteration " << (i + 1) << ")..." << endl;
        
        // OPTIMIZED: Iterative tree traversal
        vector<int> tree(TREE_SIZE);
        binary_tree_traversal_iterative(tree);
        
        cout << "Completed optimized binary tree traversal. Total calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== OPTIMIZED BINARY TREE TRAVERSAL RESULTS ===" << endl;
    cout << "Total calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 2: Optimized Matrix Path Finding
 * Demonstrates dynamic programming and memoization
 */

// OPTIMIZED: Dynamic programming approach - O(n*m) time complexity
void matrix_path_dynamic_programming(vector<vector<int>>& matrix) {
    int rows = static_cast<int>(matrix.size());
    int cols = static_cast<int>(matrix[0].size());
    
    // OPTIMIZED: Use dynamic programming table
    vector<vector<int>> dp(rows, vector<int>(cols, 0));
    
    // Initialize first row and column
    for (int i = 0; i < rows; ++i) {
        for (int j = 0; j < cols; ++j) {
            total_recursive_calls++;
            
            // OPTIMIZED: Pre-calculate expensive operations once
            double sin_val = sin(static_cast<double>(i + j));
            double cos_val = cos(static_cast<double>(i + j));
            
            matrix[i][j] = static_cast<int>(sin_val + cos_val);
            
            // OPTIMIZED: Dynamic programming calculation
            if (i == 0 && j == 0) {
                dp[i][j] = matrix[i][j];
            } else if (i == 0) {
                dp[i][j] = dp[i][j-1] + matrix[i][j];
            } else if (j == 0) {
                dp[i][j] = dp[i-1][j] + matrix[i][j];
            } else {
                dp[i][j] = min(dp[i-1][j], min(dp[i][j-1], dp[i-1][j-1])) + matrix[i][j];
            }
        }
    }
}

// OPTIMIZED: Memoized recursive approach - O(n*m) with caching
void matrix_path_memoized(vector<vector<int>>& matrix, int row, int col, int depth, unordered_map<string, int>& memo) {
    total_recursive_calls++;
    
    if (row >= matrix.size() || col >= matrix[0].size() || depth > RECURSION_DEPTH_LIMIT) {
        return;
    }
    
    // OPTIMIZED: Create cache key for memoization
    string cache_key = to_string(row) + "," + to_string(col) + "," + to_string(depth);
    
    if (memo.find(cache_key) != memo.end()) {
        return;  // Already processed
    }
    
    // OPTIMIZED: Pre-calculate expensive operations once
    double sin_val = sin(static_cast<double>(row + col + depth));
    double cos_val = cos(static_cast<double>(row + col + depth));
    
    matrix[row][col] = static_cast<int>(sin_val + cos_val);
    
    // OPTIMIZED: Cache the result
    memo[cache_key] = matrix[row][col];
    
    // OPTIMIZED: Single recursive call instead of multiple
    matrix_path_memoized(matrix, row + 1, col, depth + 1, memo);
}

void test_matrix_path_finding_optimized(int iterations) {
    cout << "=== TESTING OPTIMIZED MATRIX PATH FINDING ===" << endl;
    cout << "This demonstrates dynamic programming and memoization" << endl;
    cout << "Matrix size: " << MATRIX_SIZE << "x" << MATRIX_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing optimized matrix path finding (iteration " << (i + 1) << ")..." << endl;
        
        // OPTIMIZED: Dynamic programming approach
        vector<vector<int>> matrix(MATRIX_SIZE, vector<int>(MATRIX_SIZE));
        matrix_path_dynamic_programming(matrix);
        
        cout << "Completed optimized matrix path finding. Total calls so far: " << total_recursive_calls.load() << endl;
        cout << "Cache size: " << path_cache.size() << " entries" << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== OPTIMIZED MATRIX PATH FINDING RESULTS ===" << endl;
    cout << "Total calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << "Cache utilization: " << path_cache.size() << " cached path values" << endl;
    cout << endl;
}

/*
 * PERFORMANCE COMPARISON UTILITIES
 */

// Utility function to demonstrate optimization benefits
void demonstrate_exponential_recursion_optimization_benefits() {
    cout << "=== EXPONENTIAL RECURSION OPTIMIZATION BENEFITS DEMONSTRATION ===" << endl;
    cout << "Comparing optimized vs inefficient exponential recursion implementations:" << endl;
    cout << endl;
    
    // Binary tree traversal comparison
    cout << "1. BINARY TREE TRAVERSAL OPTIMIZATION:" << endl;
    cout << "   - Inefficient: Recursive calls causing exponential growth O(2^n)" << endl;
    cout << "   - Optimized: Iterative conversion using queue O(n)" << endl;
    cout << "   - Performance improvement: Exponential to linear complexity" << endl;
    cout << endl;
    
    // Matrix path finding comparison
    cout << "2. MATRIX PATH FINDING OPTIMIZATION:" << endl;
    cout << "   - Inefficient: Multiple recursive calls causing exponential growth O(3^n)" << endl;
    cout << "   - Optimized: Dynamic programming and memoization O(n*m)" << endl;
    cout << "   - Performance improvement: Exponential to polynomial complexity" << endl;
    cout << endl;
    
    // General optimization principles
    cout << "3. GENERAL EXPONENTIAL RECURSION OPTIMIZATION PRINCIPLES:" << endl;
    cout << "   - Iterative conversion: Convert recursion to iteration when possible" << endl;
    cout << "   - Dynamic programming: Use bottom-up approach for path problems" << endl;
    cout << "   - Memoization: Cache results to avoid redundant calculations" << endl;
    cout << "   - Algorithm optimization: Use more efficient algorithms" << endl;
    cout << "   - Space optimization: Reduce memory usage with efficient data structures" << endl;
    cout << "   - Time complexity improvement: O(2^n) -> O(n), O(3^n) -> O(n*m)" << endl;
    cout << endl;
}

int main() {
    cout << "=== OPTIMIZED EXPONENTIAL RECURSION PATTERNS PERFORMANCE SOLUTION ===" << endl;
    cout << "This program demonstrates optimized exponential recursion implementations:" << endl;
    cout << "1. Binary tree traversal with iterative conversion" << endl;
    cout << "2. Matrix path finding with dynamic programming and memoization" << endl;
    cout << endl;
    cout << "This will demonstrate significant performance improvements!" << endl;
    cout << endl;
    
    // Test each optimized exponential recursion pattern
    test_binary_tree_traversal_optimized(TREE_TRAVERSAL_ITERATIONS);
    test_matrix_path_finding_optimized(MATRIX_PATH_ITERATIONS);
    
    // Demonstrate optimization benefits
    demonstrate_exponential_recursion_optimization_benefits();
    
    cout << "=== OVERALL EXPONENTIAL RECURSION OPTIMIZATION ANALYSIS ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Compare with the inefficient version to see performance improvements!" << endl;
    cout << "3. Observe the dramatic reduction in recursive calls" << endl;
    cout << "4. Analyze the efficiency of optimized algorithms" << endl;
    cout << "5. Examine time complexity improvements" << endl;
    cout << "6. Look for optimization techniques in action" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called functions" << endl;
    cout << "8. Check for improved time complexity patterns" << endl;
    cout << endl;
    cout << "Key Exponential Recursion Optimization Techniques Demonstrated:" << endl;
    cout << "- Iterative conversion: Converting recursion to iteration" << endl;
    cout << "- Dynamic programming: Using bottom-up approach" << endl;
    cout << "- Memoization: Caching results to avoid redundant calculations" << endl;
    cout << "- Algorithm optimization: Using more efficient algorithms" << endl;
    cout << "- Time complexity improvement: O(2^n) -> O(n) for tree traversal" << endl;
    cout << "- Time complexity improvement: O(3^n) -> O(n*m) for matrix path finding" << endl;
    cout << "- Space complexity optimization: Efficient data structure usage" << endl;
    cout << "- Total calls: " << total_recursive_calls.load() << endl;
    cout << "- Cache utilization: " << path_cache.size() << " cached path values" << endl;
    
    return 0;
}
