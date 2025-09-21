/*
 * PROFILING EXAMPLE: QuickSort Performance Investigation
 * 
 * This example demonstrates QuickSort performance issues:
 * - Poor pivot selection causing O(n²) worst case
 * - Inefficient partitioning algorithms
 * - Stack overflow with deep recursion
 * - No optimization for small arrays
 * 
 * OBJECTIVES:
 * - Measure QuickSort performance with problematic inputs
 * - Demonstrate O(n²) worst case scenarios
 * - Compare inefficient vs optimized QuickSort implementations
 * - Identify performance bottlenecks in sorting
 * - Prepare reflection on algorithm efficiency
 * 
 * NOTE: This code intentionally contains inefficient QuickSort implementation.
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
const int ARRAY_SIZE = 5000;              // Array size for profiling
const int TEST_ITERATIONS = 3;            // Number of test iterations
const int RANDOM_SEED = 42;               // Random seed for reproducible results

// Performance Tracking
long long totalComparisons = 0;
long long totalSwaps = 0;
int maxRecursionDepth = 0;
int currentRecursionDepth = 0;

// ============================================================================

/*
 * SCENARIO 1: Inefficient QuickSort Implementation
 * Demonstrates poor pivot selection and O(n²) worst case
 */

// MAJOR PROBLEM: Inefficient QuickSort with poor pivot selection
void quickSortInefficient(vector<int>& arr, int low, int high) {
    currentRecursionDepth++;
    maxRecursionDepth = max(maxRecursionDepth, currentRecursionDepth);

    if (low < high) {
        // MAJOR PROBLEM: Always choose first element as pivot (worst case for sorted arrays)
        int pivotIndex = partitionInefficient(arr, low, high);

        // MAJOR PROBLEM: Recursive calls without optimization
        quickSortInefficient(arr, low, pivotIndex - 1);
        quickSortInefficient(arr, pivotIndex + 1, high);
    }

    currentRecursionDepth--;
}

// MAJOR PROBLEM: Inefficient partitioning algorithm
int partitionInefficient(vector<int>& arr, int low, int high) {
    // MAJOR PROBLEM: Always choose first element as pivot
    int pivot = arr[low];
    int i = low + 1;

    // MAJOR PROBLEM: Inefficient partitioning with multiple passes
    for (int j = low + 1; j <= high; j++) {
        totalComparisons++;
        if (arr[j] < pivot) {
            // MAJOR PROBLEM: Inefficient swap operations
            swap(arr[i], arr[j]);
            totalSwaps++;
            i++;
        }
    }

    // MAJOR PROBLEM: Multiple operations to place pivot
    swap(arr[low], arr[i - 1]);
    totalSwaps++;
    return i - 1;
}

// MAJOR PROBLEM: Even more inefficient QuickSort with redundant operations
void quickSortVeryInefficient(vector<int>& arr, int low, int high) {
    currentRecursionDepth++;
    maxRecursionDepth = max(maxRecursionDepth, currentRecursionDepth);

    if (low < high) {
        // MAJOR PROBLEM: Multiple partitioning attempts
        for (int attempt = 0; attempt < 3; attempt++) {
            partitionVeryInefficient(arr, low, high);
        }

        // MAJOR PROBLEM: Choose worst possible pivot (first element)
        int pivotIndex = partitionVeryInefficient(arr, low, high);

        // MAJOR PROBLEM: Recursive calls without tail recursion optimization
        quickSortVeryInefficient(arr, low, pivotIndex - 1);
        quickSortVeryInefficient(arr, pivotIndex + 1, high);
    }

    currentRecursionDepth--;
}

// MAJOR PROBLEM: Very inefficient partitioning with redundant operations
int partitionVeryInefficient(vector<int>& arr, int low, int high) {
    // MAJOR PROBLEM: Always choose first element as pivot
    int pivot = arr[low];
    int i = low + 1;

    // MAJOR PROBLEM: Multiple passes over the same data
    for (int pass = 0; pass < 2; pass++) {
        for (int j = low + 1; j <= high; j++) {
            totalComparisons++;
            if (arr[j] < pivot) {
                // MAJOR PROBLEM: Redundant swap operations
                swap(arr[i], arr[j]);
                totalSwaps++;
                i++;

                // MAJOR PROBLEM: Redundant comparison after swap
                if (arr[i - 1] > arr[j]) {
                    totalComparisons++;
                }
            }
        }
    }

    // MAJOR PROBLEM: Multiple operations to place pivot
    swap(arr[low], arr[i - 1]);
    totalSwaps++;
    return i - 1;
}

/*
 * SCENARIO 2: Performance Testing Functions
 */

void testQuickSortPerformance(int iterations) {
    cout << "=== TESTING QUICKSORT PERFORMANCE ===" << endl;
    cout << "This demonstrates O(n²) worst case scenarios" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing QuickSort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test inefficient QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        maxRecursionDepth = 0;
        currentRecursionDepth = 0;
        quickSortInefficient(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "QuickSort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Max recursion depth: " << maxRecursionDepth << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testVeryInefficientQuickSort(int iterations) {
    cout << "=== TESTING VERY INEFFICIENT QUICKSORT ===" << endl;
    cout << "This demonstrates severe O(n²) performance issues" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing Very Inefficient QuickSort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test very inefficient QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        maxRecursionDepth = 0;
        currentRecursionDepth = 0;
        quickSortVeryInefficient(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "Very Inefficient QuickSort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Max recursion depth: " << maxRecursionDepth << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testWithWorstCaseInput() {
    cout << "=== TESTING QUICKSORT WITH WORST CASE INPUT ===" << endl;
    cout << "This demonstrates O(n²) worst case performance" << endl;
    cout << endl;

    auto start = high_resolution_clock::now();

    // MAJOR PROBLEM: Create worst case input (sorted array)
    vector<int> arr(ARRAY_SIZE);
    for (int i = 0; i < ARRAY_SIZE; i++) {
        arr[i] = i; // Sorted array - worst case for first element pivot
    }

    cout << "Testing with sorted array (worst case) - size: " << ARRAY_SIZE << endl;

    // Test inefficient QuickSort with worst case input
    start = high_resolution_clock::now();
    totalComparisons = 0;
    totalSwaps = 0;
    maxRecursionDepth = 0;
    currentRecursionDepth = 0;
    quickSortInefficient(arr, 0, arr.size() - 1);
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);

    cout << "QuickSort with worst case input completed:" << endl;
    cout << "  Time: " << duration.count() << " ms" << endl;
    cout << "  Comparisons: " << totalComparisons << endl;
    cout << "  Swaps: " << totalSwaps << endl;
    cout << "  Max recursion depth: " << maxRecursionDepth << endl;
    cout << "  Expected comparisons for O(n²): " << (ARRAY_SIZE * ARRAY_SIZE) << endl;
    cout << endl;
}

void testWithDifferentArraySizes() {
    cout << "=== TESTING QUICKSORT WITH DIFFERENT ARRAY SIZES ===" << endl;
    cout << "This demonstrates performance scaling" << endl;
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

        // Test QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        maxRecursionDepth = 0;
        currentRecursionDepth = 0;
        quickSortInefficient(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Max recursion depth: " << maxRecursionDepth << endl;
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
    totalComparisons = 0;
    totalSwaps = 0;
    maxRecursionDepth = 0;
    currentRecursionDepth = 0;
    quickSortInefficient(arr, 0, arr.size() - 1);

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
    cout << "Max recursion depth: " << maxRecursionDepth << endl;
    cout << endl;
}

int main() {
    cout << "=== QUICKSORT PERFORMANCE INVESTIGATION ===" << endl;
    cout << "This program demonstrates QuickSort performance issues:" << endl;
    cout << "1. Inefficient QuickSort with poor pivot selection" << endl;
    cout << "2. Very inefficient QuickSort with redundant operations" << endl;
    cout << "3. Worst case input scenarios" << endl;
    cout << "4. Performance scaling with different array sizes" << endl;
    cout << "5. Sorting correctness verification" << endl;
    cout << endl;
    cout << "Array size: " << ARRAY_SIZE << " elements" << endl;
    cout << "This will demonstrate severe sorting performance issues!" << endl;
    cout << endl;

    // Test different scenarios
    testQuickSortPerformance(TEST_ITERATIONS);
    testVeryInefficientQuickSort(TEST_ITERATIONS);
    testWithWorstCaseInput();
    testWithDifferentArraySizes();
    verifySortingCorrectness();

    cout << "=== OVERALL ANALYSIS NOTES ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Observe the O(n²) worst case performance!" << endl;
    cout << "3. Look for functions with high call counts and individual time consumption" << endl;
    cout << "4. Analyze the recursion depth and stack usage" << endl;
    cout << "5. Examine comparison and swap operation costs" << endl;
    cout << "6. Look for redundant operations in sorting algorithms" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called functions" << endl;
    cout << "8. Check for inefficient algorithm implementations" << endl;
    cout << endl;
    cout << "Key QuickSort Performance Issues Demonstrated:" << endl;
    cout << "- O(n²) worst case time complexity with poor pivot selection" << endl;
    cout << "- Deep recursion causing stack overflow potential" << endl;
    cout << "- Inefficient partitioning algorithms" << endl;
    cout << "- No optimization for small arrays" << endl;
    cout << "- Redundant operations in sorting process" << endl;
    cout << "- Poor performance with sorted or nearly sorted data" << endl;
    cout << "- Multiple passes over the same data" << endl;
    cout << "- Array size tested: " << ARRAY_SIZE << " elements" << endl;

    return 0;
}
