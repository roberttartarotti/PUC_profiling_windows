/*
 * PROFILING EXAMPLE: Bubble Sort Performance Investigation
 * 
 * This example demonstrates Bubble Sort performance issues:
 * - O(n²) time complexity with nested loops
 * - Inefficient comparisons and swaps
 * - Poor performance with large datasets
 * - No early termination optimization
 * 
 * OBJECTIVES:
 * - Measure Bubble Sort performance with large datasets
 * - Demonstrate O(n²) time complexity issues
 * - Compare inefficient vs optimized sorting algorithms
 * - Identify performance bottlenecks in sorting
 * - Prepare reflection on algorithm efficiency
 * 
 * NOTE: This code intentionally contains inefficient Bubble Sort implementation.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe sorting performance bottlenecks and learn optimization techniques.
 */

#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <algorithm>

using namespace std;
using namespace std::chrono;

// ============================================================================
// CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
// ============================================================================

// Array Configuration
const int ARRAY_SIZE = 5000;              // Array size for profiling (5000 = shows O(n²) complexity)
const int TEST_ITERATIONS = 1;            // Number of test iterations
const int RANDOM_SEED = 42;               // Random seed for reproducible results

// Performance Tracking
long long totalComparisons = 0;
long long totalSwaps = 0;

// ============================================================================

/*
 * SCENARIO 1: Inefficient Bubble Sort Implementation
 * Demonstrates O(n²) time complexity and performance issues
 */

// MAJOR PROBLEM: Inefficient Bubble Sort with O(n²) complexity
void bubbleSortInefficient(vector<int>& arr) {
    int n = arr.size();
    totalComparisons = 0;
    totalSwaps = 0;

    // MAJOR PROBLEM: Nested loops causing O(n²) complexity
    for (int i = 0; i < n - 1; i++) {
        // MAJOR PROBLEM: No early termination optimization
        for (int j = 0; j < n - i - 1; j++) {
            totalComparisons++;

            // MAJOR PROBLEM: Inefficient comparison and swap
            if (arr[j] > arr[j + 1]) {
                // MAJOR PROBLEM: Multiple operations for swap
                int temp = arr[j];
                arr[j] = arr[j + 1];
                arr[j + 1] = temp;
                totalSwaps++;
            }
        }
    }
}

// MAJOR PROBLEM: Even more inefficient Bubble Sort with redundant operations
void bubbleSortVeryInefficient(vector<int>& arr) {
    int n = arr.size();
    totalComparisons = 0;
    totalSwaps = 0;

    // MAJOR PROBLEM: Extra outer loop causing unnecessary iterations
    for (int pass = 0; pass < n; pass++) {
        // MAJOR PROBLEM: Nested loops with redundant comparisons
        for (int i = 0; i < n - 1; i++) {
            for (int j = 0; j < n - i - 1; j++) {
                totalComparisons++;

                // MAJOR PROBLEM: Multiple comparisons for same elements
                if (arr[j] > arr[j + 1]) {
                    // MAJOR PROBLEM: Inefficient swap with multiple operations
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                    totalSwaps++;

                    // MAJOR PROBLEM: Redundant comparison after swap
                    if (arr[j] < arr[j + 1]) {
                        totalComparisons++;
                    }
                }
            }
        }
    }
}

/*
 * SCENARIO 2: Performance Testing Functions
 */

void testBubbleSortPerformance(int iterations) {
    cout << "=== TESTING BUBBLE SORT PERFORMANCE ===" << endl;
    cout << "This demonstrates O(n²) time complexity issues" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing Bubble Sort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test inefficient Bubble Sort
        auto start = high_resolution_clock::now();
        bubbleSortInefficient(arr);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "Bubble Sort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testVeryInefficientBubbleSort(int iterations) {
    cout << "=== TESTING VERY INEFFICIENT BUBBLE SORT ===" << endl;
    cout << "This demonstrates severe O(n²) performance issues" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing Very Inefficient Bubble Sort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test very inefficient Bubble Sort
        auto start = high_resolution_clock::now();
        bubbleSortVeryInefficient(arr);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "Very Inefficient Bubble Sort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testWithDifferentArraySizes() {
    cout << "=== TESTING BUBBLE SORT WITH DIFFERENT ARRAY SIZES ===" << endl;
    cout << "This demonstrates O(n²) complexity scaling" << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    vector<int> sizes = {100, 500, 1000, 2000, 3000};

    for (int size : sizes) {
        cout << "Testing with array size: " << size << endl;

        // Create vector with random data
        vector<int> arr(size);
        for (int j = 0; j < size; j++) {
            arr[j] = dis(gen);
        }

        // Test Bubble Sort
        auto start = high_resolution_clock::now();
        bubbleSortInefficient(arr);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Time per element: " << (double)duration.count() / size << " ms" << endl;
        cout << endl;
    }
}

void verifySortingCorrectness() {
    cout << "=== VERIFYING SORTING CORRECTNESS ===" << endl;
    cout << "This verifies that the sorting algorithm works correctly" << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 1000);

    vector<int> arr(100);

    // Create vector with random data
    for (int i = 0; i < 100; i++) {
        arr[i] = dis(gen);
    }

    cout << "Original array (first 10 elements):" << endl;
    for (int i = 0; i < 10; i++) {
        cout << arr[i] << " ";
    }
    cout << endl;

    // Sort the vector
    bubbleSortInefficient(arr);

    cout << "Sorted array (first 10 elements):" << endl;
    for (int i = 0; i < 10; i++) {
        cout << arr[i] << " ";
    }
    cout << endl;

    // Verify sorting correctness
    bool isSorted = true;
    for (int i = 0; i < arr.size() - 1; i++) {
        if (arr[i] > arr[i + 1]) {
            isSorted = false;
            break;
        }
    }

    cout << "Array is correctly sorted: " << (isSorted ? "true" : "false") << endl;
    cout << "Total comparisons: " << totalComparisons << endl;
    cout << "Total swaps: " << totalSwaps << endl;
    cout << endl;
}

int main() {
    cout << "=== BUBBLE SORT PERFORMANCE INVESTIGATION ===" << endl;
    cout << "This program demonstrates Bubble Sort performance issues:" << endl;
    cout << "1. Inefficient Bubble Sort with O(n²) complexity" << endl;
    cout << "2. Very inefficient Bubble Sort with redundant operations" << endl;
    cout << "3. Performance scaling with different array sizes" << endl;
    cout << "4. Sorting correctness verification" << endl;
    cout << endl;
    cout << "Array size: " << ARRAY_SIZE << " elements" << endl;
    cout << "This will demonstrate severe sorting performance issues!" << endl;
    cout << endl;

    // Test different scenarios
    testBubbleSortPerformance(TEST_ITERATIONS);
    testVeryInefficientBubbleSort(TEST_ITERATIONS);
    testWithDifferentArraySizes();
    verifySortingCorrectness();

    cout << "=== OVERALL ANALYSIS NOTES ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Observe the O(n²) time complexity scaling!" << endl;
    cout << "3. Look for functions with high call counts and individual time consumption" << endl;
    cout << "4. Analyze the nested loop performance patterns" << endl;
    cout << "5. Examine comparison and swap operation costs" << endl;
    cout << "6. Look for redundant operations in sorting algorithms" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called functions" << endl;
    cout << "8. Check for inefficient algorithm implementations" << endl;
    cout << endl;
    cout << "Key Bubble Sort Performance Issues Demonstrated:" << endl;
    cout << "- O(n²) time complexity causing poor performance with large datasets" << endl;
    cout << "- Nested loops with redundant comparisons" << endl;
    cout << "- Inefficient swap operations" << endl;
    cout << "- No early termination optimization" << endl;
    cout << "- Multiple passes over the same data" << endl;
    cout << "- Redundant operations in sorting process" << endl;
    cout << "- Poor scaling with increasing array size" << endl;
    cout << "- Array size tested: " << ARRAY_SIZE << " elements" << endl;

    return 0;
}
