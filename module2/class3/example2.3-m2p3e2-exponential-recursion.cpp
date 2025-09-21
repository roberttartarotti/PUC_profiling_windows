/*
 * PROFILING EXAMPLE: Exponential Recursion Patterns Performance Investigation
 * 
 * This example demonstrates exponential recursion performance issues:
 * - Binary tree traversal with exponential growth
 * - Matrix path finding with exponential recursion
 * - Multiple recursive calls causing exponential complexity
 * 
 * OBJECTIVES:
 * - Measure exponential recursion impact via instrumentation
 * - Detect exponential growth in recursive calls
 * - Compare inefficient recursive vs optimized solutions
 * - Identify exponential time complexity patterns
 * - Prepare reflection on algorithm design
 * 
 * NOTE: This code intentionally contains severe recursive performance problems.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe exponential recursive call patterns and performance bottlenecks.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <cmath>
#include <atomic>

using namespace std;
using namespace std::chrono;

// ============================================================================
// CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
// ============================================================================

// Recursion Configuration  
const int RECURSION_DEPTH_LIMIT = 15;     // Maximum recursion depth (15 = safe for exponential growth)
const int TREE_SIZE = 1000;               // Binary tree size for traversal
const int MATRIX_SIZE = 15;               // Matrix size for path finding (15x15)

// Test Iterations Configuration
const int TREE_TRAVERSAL_ITERATIONS = 3;   // Tree traversal test iterations
const int MATRIX_PATH_ITERATIONS = 2;     // Matrix path test iterations

// ============================================================================

// Global variables for tracking
atomic<int> total_recursive_calls(0);
random_device rd;
mt19937 gen(rd());

// Forward declarations
void test_binary_tree_traversal(int iterations);
void test_matrix_path_finding(int iterations);
void binary_tree_traversal_recursive(vector<int>& tree, int index, int depth);
void matrix_path_recursive(vector<vector<int>>& matrix, int row, int col, int depth);

/*
 * SCENARIO 1: Binary Tree Traversal with Exponential Recursion
 * Demonstrates exponential growth in recursive calls
 */

// MAJOR PROBLEM: Binary tree traversal with deep recursion
void binary_tree_traversal_recursive(vector<int>& tree, int index, int depth) {
    total_recursive_calls++;
    
    if (index >= tree.size() || depth > RECURSION_DEPTH_LIMIT) {
        return;
    }
    
    // MAJOR PROBLEM: Expensive operations in every recursive call
    tree[index] = static_cast<int>(sin(depth) + cos(depth) + sqrt(depth + 1));
    
    // MAJOR PROBLEM: Multiple recursive calls causing exponential growth
    binary_tree_traversal_recursive(tree, 2 * index + 1, depth + 1);
    binary_tree_traversal_recursive(tree, 2 * index + 2, depth + 1);
}

void test_binary_tree_traversal(int iterations) {
    cout << "=== TESTING BINARY TREE TRAVERSAL RECURSIVE FUNCTION ===" << endl;
    cout << "This demonstrates exponential growth in recursive calls" << endl;
    cout << "Tree size: " << TREE_SIZE << endl;
    cout << "Recursion depth limit: " << RECURSION_DEPTH_LIMIT << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing binary tree traversal (iteration " << (i + 1) << ")..." << endl;
        
        // MAJOR PROBLEM: Binary tree traversal with deep recursion
        vector<int> tree(TREE_SIZE);
        binary_tree_traversal_recursive(tree, 0, 0);
        
        cout << "Completed binary tree traversal. Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== BINARY TREE TRAVERSAL RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

/*
 * SCENARIO 2: Matrix Path Finding with Exponential Recursion
 * Demonstrates exponential complexity in path finding
 */

// MAJOR PROBLEM: Matrix path finding with exponential recursion
void matrix_path_recursive(vector<vector<int>>& matrix, int row, int col, int depth) {
    total_recursive_calls++;
    
    if (row >= matrix.size() || col >= matrix[0].size() || depth > RECURSION_DEPTH_LIMIT) {
        return;
    }
    
    // MAJOR PROBLEM: Expensive calculations in every recursive call
    matrix[row][col] = static_cast<int>(sin(row + col + depth) + cos(row + col + depth));
    
    // MAJOR PROBLEM: Multiple recursive calls causing exponential growth
    matrix_path_recursive(matrix, row + 1, col, depth + 1);
    matrix_path_recursive(matrix, row, col + 1, depth + 1);
    matrix_path_recursive(matrix, row + 1, col + 1, depth + 1);
}

void test_matrix_path_finding(int iterations) {
    cout << "=== TESTING MATRIX PATH FINDING RECURSIVE FUNCTION ===" << endl;
    cout << "This demonstrates exponential complexity in path finding" << endl;
    cout << "Matrix size: " << MATRIX_SIZE << "x" << MATRIX_SIZE << endl;
    cout << "Recursion depth limit: " << RECURSION_DEPTH_LIMIT << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;
    
    auto start = high_resolution_clock::now();
    
    for (int i = 0; i < iterations; ++i) {
        cout << "Testing matrix path finding (iteration " << (i + 1) << ")..." << endl;
        
        // MAJOR PROBLEM: Matrix path finding with exponential recursion
        vector<vector<int>> matrix(MATRIX_SIZE, vector<int>(MATRIX_SIZE));
        matrix_path_recursive(matrix, 0, 0, 0);
        
        cout << "Completed matrix path finding. Total recursive calls so far: " << total_recursive_calls.load() << endl;
        cout << endl;
    }
    
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);
    
    cout << "=== MATRIX PATH FINDING RESULTS ===" << endl;
    cout << "Total recursive calls: " << total_recursive_calls.load() << endl;
    cout << "Time taken: " << duration.count() << " ms" << endl;
    cout << "Average time per iteration: " << (double)duration.count() / iterations << " ms" << endl;
    cout << "Average recursive calls per iteration: " << total_recursive_calls.load() / iterations << endl;
    cout << endl;
}

int main() {
    cout << "=== EXPONENTIAL RECURSION PATTERNS PERFORMANCE INVESTIGATION ===" << endl;
    cout << "This program demonstrates exponential recursion performance issues:" << endl;
    cout << "1. Binary tree traversal with exponential growth" << endl;
    cout << "2. Matrix path finding with exponential recursion" << endl;
    cout << endl;
    cout << "This will demonstrate severe exponential recursion performance issues!" << endl;
    cout << endl;
    
    // Test each exponential recursion pattern
    test_binary_tree_traversal(TREE_TRAVERSAL_ITERATIONS);
    test_matrix_path_finding(MATRIX_PATH_ITERATIONS);
    
    cout << "=== OVERALL ANALYSIS NOTES ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Observe the exponential growth in recursive calls!" << endl;
    cout << "3. Look for functions with extremely high call counts" << endl;
    cout << "4. Analyze call graph for exponential recursive patterns" << endl;
    cout << "5. Examine exponential time complexity patterns" << endl;
    cout << "6. Look for redundant calculations in recursive calls" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called recursive functions" << endl;
    cout << "8. Check for exponential vs linear time complexity patterns" << endl;
    cout << endl;
    cout << "Key Exponential Recursion Performance Issues Demonstrated:" << endl;
    cout << "- Exponential time complexity in recursive algorithms" << endl;
    cout << "- Multiple recursive calls per function causing exponential growth" << endl;
    cout << "- Redundant calculations in recursive calls" << endl;
    cout << "- Expensive operations in every recursive call" << endl;
    cout << "- No memoization or caching of recursive results" << endl;
    cout << "- Deep recursion with exponential call growth" << endl;
    cout << "- Total recursive calls: " << total_recursive_calls.load() << endl;
    
    return 0;
}
