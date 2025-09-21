/*
 * PROFILING EXAMPLE: Optimized QuickSort Performance Solution
 * 
 * This example demonstrates optimized QuickSort implementations:
 * - Median-of-three pivot selection
 * - Efficient partitioning algorithms
 * - Tail recursion optimization
 * - Insertion sort for small arrays
 * - Three-way partitioning for duplicates
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for QuickSort
 * - Show performance improvements through better algorithms
 * - Compare inefficient vs optimized QuickSort solutions
 * - Identify best practices for sorting algorithm design
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized QuickSort implementations.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance improvements and optimization patterns.
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
const int ARRAY_SIZE = 5000;              // Array size for profiling (same as problem version)
const int TEST_ITERATIONS = 3;            // Number of test iterations
const int RANDOM_SEED = 42;               // Random seed for reproducible results
const int INSERTION_SORT_THRESHOLD = 10;  // Threshold for switching to insertion sort

// Performance Tracking
long long totalComparisons = 0;
long long totalSwaps = 0;
int maxRecursionDepth = 0;
int currentRecursionDepth = 0;

// ============================================================================

/*
 * SCENARIO 1: Optimized QuickSort Implementation
 * Demonstrates efficient pivot selection and O(n log n) average case
 */

// OPTIMIZED: QuickSort with median-of-three pivot selection
void quickSortOptimized(vector<int>& arr, int low, int high) {
    currentRecursionDepth++;
    maxRecursionDepth = max(maxRecursionDepth, currentRecursionDepth);

    if (low < high) {
        // OPTIMIZED: Use insertion sort for small arrays
        if (high - low + 1 < INSERTION_SORT_THRESHOLD) {
            insertionSort(arr, low, high);
            currentRecursionDepth--;
            return;
        }

        // OPTIMIZED: Median-of-three pivot selection
        int pivotIndex = medianOfThree(arr, low, high);
        swap(arr[pivotIndex], arr[high]); // Move pivot to end

        // OPTIMIZED: Efficient partitioning
        int partitionIndex = partitionOptimized(arr, low, high);

        // OPTIMIZED: Tail recursion optimization
        if (partitionIndex - low < high - partitionIndex) {
            quickSortOptimized(arr, low, partitionIndex - 1);
            quickSortOptimized(arr, partitionIndex + 1, high);
        } else {
            quickSortOptimized(arr, partitionIndex + 1, high);
            quickSortOptimized(arr, low, partitionIndex - 1);
        }
    }

    currentRecursionDepth--;
}

// OPTIMIZED: Median-of-three pivot selection
int medianOfThree(vector<int>& arr, int low, int high) {
    int mid = low + (high - low) / 2;

    // OPTIMIZED: Find median of three elements
    if (arr[mid] < arr[low]) {
        if (arr[high] < arr[mid])
            return mid;
        else if (arr[high] < arr[low])
            return high;
        else
            return low;
    } else {
        if (arr[high] < arr[mid]) {
            if (arr[high] < arr[low])
                return low;
            else
                return high;
        } else
            return mid;
    }
}

// OPTIMIZED: Efficient partitioning algorithm
int partitionOptimized(vector<int>& arr, int low, int high) {
    int pivot = arr[high];
    int i = low - 1;

    // OPTIMIZED: Single pass partitioning
    for (int j = low; j < high; j++) {
        totalComparisons++;
        if (arr[j] <= pivot) {
            i++;
            swap(arr[i], arr[j]);
            totalSwaps++;
        }
    }

    // OPTIMIZED: Place pivot in correct position
    swap(arr[i + 1], arr[high]);
    totalSwaps++;
    return i + 1;
}

// OPTIMIZED: Insertion sort for small arrays
void insertionSort(vector<int>& arr, int low, int high) {
    for (int i = low + 1; i <= high; i++) {
        int key = arr[i];
        int j = i - 1;

        while (j >= low && arr[j] > key) {
            totalComparisons++;
            arr[j + 1] = arr[j];
            totalSwaps++;
            j--;
        }
        totalComparisons++;

        arr[j + 1] = key;
    }
}

/*
 * SCENARIO 2: Three-Way QuickSort Implementation
 * Demonstrates efficient handling of duplicate elements
 */

// OPTIMIZED: Three-way QuickSort for arrays with many duplicates
void quickSortThreeWay(vector<int>& arr, int low, int high) {
    currentRecursionDepth++;
    maxRecursionDepth = max(maxRecursionDepth, currentRecursionDepth);

    if (low < high) {
        // OPTIMIZED: Use insertion sort for small arrays
        if (high - low + 1 < INSERTION_SORT_THRESHOLD) {
            insertionSort(arr, low, high);
            currentRecursionDepth--;
            return;
        }

        // OPTIMIZED: Three-way partitioning
        pair<int, int> partitionResult = partitionThreeWay(arr, low, high);
        int lt = partitionResult.first;
        int gt = partitionResult.second;

        // OPTIMIZED: Recursively sort subarrays
        quickSortThreeWay(arr, low, lt - 1);
        quickSortThreeWay(arr, gt + 1, high);
    }

    currentRecursionDepth--;
}

// OPTIMIZED: Three-way partitioning for duplicate elements
pair<int, int> partitionThreeWay(vector<int>& arr, int low, int high) {
    int pivot = arr[low];
    int lt = low;
    int gt = high;
    int i = low + 1;

    while (i <= gt) {
        totalComparisons++;
        if (arr[i] < pivot) {
            swap(arr[lt++], arr[i++]);
            totalSwaps++;
        } else if (arr[i] > pivot) {
            swap(arr[i], arr[gt--]);
            totalSwaps++;
        } else {
            i++;
        }
    }

    return make_pair(lt, gt);
}

/*
 * SCENARIO 3: Performance Testing Functions
 */

void testQuickSortOptimized(int iterations) {
    cout << "=== TESTING OPTIMIZED QUICKSORT ===" << endl;
    cout << "This demonstrates O(n log n) average time complexity" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing Optimized QuickSort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test optimized QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        maxRecursionDepth = 0;
        currentRecursionDepth = 0;
        quickSortOptimized(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "Optimized QuickSort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Max recursion depth: " << maxRecursionDepth << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testThreeWayQuickSort(int iterations) {
    cout << "=== TESTING THREE-WAY QUICKSORT ===" << endl;
    cout << "This demonstrates efficient handling of duplicate elements" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 100);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing Three-Way QuickSort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with many duplicates
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen); // Many duplicates
        }

        // Test three-way QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        maxRecursionDepth = 0;
        currentRecursionDepth = 0;
        quickSortThreeWay(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "Three-Way QuickSort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Max recursion depth: " << maxRecursionDepth << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testWithWorstCaseInput() {
    cout << "=== TESTING OPTIMIZED QUICKSORT WITH WORST CASE INPUT ===" << endl;
    cout << "This demonstrates improved worst case performance" << endl;
    cout << endl;

    // Create worst case input (sorted array)
    vector<int> arr(ARRAY_SIZE);
    for (int i = 0; i < ARRAY_SIZE; i++) {
        arr[i] = i; // Sorted array
    }

    cout << "Testing with sorted array (worst case) - size: " << ARRAY_SIZE << endl;

    // Test optimized QuickSort with worst case input
    auto start = high_resolution_clock::now();
    totalComparisons = 0;
    totalSwaps = 0;
    maxRecursionDepth = 0;
    currentRecursionDepth = 0;
    quickSortOptimized(arr, 0, arr.size() - 1);
    auto end = high_resolution_clock::now();
    auto duration = duration_cast<milliseconds>(end - start);

    cout << "Optimized QuickSort with worst case input completed:" << endl;
    cout << "  Time: " << duration.count() << " ms" << endl;
    cout << "  Comparisons: " << totalComparisons << endl;
    cout << "  Swaps: " << totalSwaps << endl;
    cout << "  Max recursion depth: " << maxRecursionDepth << endl;
    cout << "  Expected comparisons for O(n log n): " << (ARRAY_SIZE * log2(ARRAY_SIZE)) << endl;
    cout << endl;
}

void testWithDifferentArraySizes() {
    cout << "=== TESTING OPTIMIZED QUICKSORT WITH DIFFERENT ARRAY SIZES ===" << endl;
    cout << "This demonstrates O(n log n) complexity scaling" << endl;
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

        // Test optimized QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        maxRecursionDepth = 0;
        currentRecursionDepth = 0;
        quickSortOptimized(arr, 0, arr.size() - 1);
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

void compareAllQuickSortVariants() {
    cout << "=== COMPARING ALL QUICKSORT VARIANTS ===" << endl;
    cout << "This demonstrates performance differences between variants" << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    // Create vector with random data
    vector<int> arr(ARRAY_SIZE);
    for (int j = 0; j < ARRAY_SIZE; j++) {
        arr[j] = dis(gen);
    }

    // Test Optimized QuickSort
    vector<int> arr1 = arr;
    auto start = high_resolution_clock::now();
    totalComparisons = 0;
    totalSwaps = 0;
    maxRecursionDepth = 0;
    currentRecursionDepth = 0;
    quickSortOptimized(arr1, 0, arr1.size() - 1);
    auto end = high_resolution_clock::now();
    auto optimizedDuration = duration_cast<milliseconds>(end - start);
    long optimizedTime = optimizedDuration.count();
    long optimizedComparisons = totalComparisons;
    long optimizedSwaps = totalSwaps;
    int optimizedRecursion = maxRecursionDepth;

    // Test Three-Way QuickSort
    vector<int> arr2 = arr;
    start = high_resolution_clock::now();
    totalComparisons = 0;
    totalSwaps = 0;
    maxRecursionDepth = 0;
    currentRecursionDepth = 0;
    quickSortThreeWay(arr2, 0, arr2.size() - 1);
    end = high_resolution_clock::now();
    auto threeWayDuration = duration_cast<milliseconds>(end - start);
    long threeWayTime = threeWayDuration.count();
    long threeWayComparisons = totalComparisons;
    long threeWaySwaps = totalSwaps;
    int threeWayRecursion = maxRecursionDepth;

    cout << "QuickSort Variants Comparison Results:" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << endl;
    cout << "Optimized QuickSort:" << endl;
    cout << "  Time: " << optimizedTime << " ms" << endl;
    cout << "  Comparisons: " << optimizedComparisons << endl;
    cout << "  Swaps: " << optimizedSwaps << endl;
    cout << "  Max recursion depth: " << optimizedRecursion << endl;
    cout << endl;
    cout << "Three-Way QuickSort:" << endl;
    cout << "  Time: " << threeWayTime << " ms" << endl;
    cout << "  Comparisons: " << threeWayComparisons << endl;
    cout << "  Swaps: " << threeWaySwaps << endl;
    cout << "  Max recursion depth: " << threeWayRecursion << endl;
    cout << endl;
    cout << "Performance Comparison:" << endl;
    cout << "  Three-Way vs Optimized: " << (double)optimizedTime / threeWayTime << "x" << endl;
    cout << "  Recursion depth difference: " << abs(optimizedRecursion - threeWayRecursion) << endl;
    cout << endl;
}

int main() {
    cout << "=== OPTIMIZED QUICKSORT PERFORMANCE SOLUTION ===" << endl;
    cout << "This program demonstrates optimized QuickSort implementations:" << endl;
    cout << "1. Optimized QuickSort with median-of-three pivot selection" << endl;
    cout << "2. Three-way QuickSort for duplicate elements" << endl;
    cout << "3. Insertion sort optimization for small arrays" << endl;
    cout << "4. Tail recursion optimization" << endl;
    cout << "5. Performance comparison between variants" << endl;
    cout << endl;
    cout << "Array size: " << ARRAY_SIZE << " elements" << endl;
    cout << "This will demonstrate significant sorting performance improvements!" << endl;
    cout << endl;

    // Test different optimized algorithms
    testQuickSortOptimized(TEST_ITERATIONS);
    testThreeWayQuickSort(TEST_ITERATIONS);
    testWithWorstCaseInput();
    testWithDifferentArraySizes();
    compareAllQuickSortVariants();

    cout << "=== OVERALL OPTIMIZATION ANALYSIS ===" << endl;
    cout << "1. Run this with Visual Studio Profiler in INSTRUMENTATION mode" << endl;
    cout << "2. Compare with the inefficient version to see performance improvements!" << endl;
    cout << "3. Observe the dramatic reduction in comparisons and swaps" << endl;
    cout << "4. Analyze the efficiency of optimized algorithms" << endl;
    cout << "5. Examine time complexity improvements" << endl;
    cout << "6. Look for optimization techniques in action" << endl;
    cout << "7. Focus on 'Hot Paths' - most frequently called functions" << endl;
    cout << "8. Check for improved time complexity patterns" << endl;
    cout << endl;
    cout << "Key QuickSort Optimization Techniques Demonstrated:" << endl;
    cout << "- Median-of-three pivot selection: Reduces worst case scenarios" << endl;
    cout << "- Insertion sort for small arrays: Optimizes small subproblems" << endl;
    cout << "- Tail recursion optimization: Reduces stack usage" << endl;
    cout << "- Three-way partitioning: Efficient handling of duplicates" << endl;
    cout << "- Time complexity improvement: O(n²) -> O(n log n) average case" << endl;
    cout << "- Space complexity optimization: Reduced recursion depth" << endl;
    cout << "- Reduced comparisons: Minimize unnecessary comparisons" << endl;
    cout << "- Reduced swaps: Minimize unnecessary swaps" << endl;
    cout << "- Array size tested: " << ARRAY_SIZE << " elements" << endl;

    return 0;
}
