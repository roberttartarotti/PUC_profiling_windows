/*
 * PROFILING EXAMPLE: Optimized Bubble Sort Performance Solution
 * 
 * This example demonstrates optimized sorting implementations:
 * - QuickSort with O(n log n) average time complexity
 * - MergeSort with O(n log n) guaranteed time complexity
 * - Optimized Bubble Sort with early termination
 * - Efficient comparison and swap operations
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for sorting algorithms
 * - Show performance improvements through better algorithms
 * - Compare inefficient vs optimized sorting solutions
 * - Identify best practices for sorting algorithm design
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized sorting implementations.
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
const int TEST_ITERATIONS = 1;            // Number of test iterations
const int RANDOM_SEED = 42;               // Random seed for reproducible results

// Performance Tracking
long long totalComparisons = 0;
long long totalSwaps = 0;

// ============================================================================

/*
 * SCENARIO 1: Optimized Bubble Sort Implementation
 * Demonstrates early termination and efficiency improvements
 */

// OPTIMIZED: Bubble Sort with early termination - O(n²) worst case, O(n) best case
void bubbleSortOptimized(vector<int>& arr) {
    int n = arr.size();
    totalComparisons = 0;
    totalSwaps = 0;

    // OPTIMIZED: Early termination when no swaps occur
    for (int i = 0; i < n - 1; i++) {
        bool swapped = false;

        for (int j = 0; j < n - i - 1; j++) {
            totalComparisons++;

            // OPTIMIZED: Efficient comparison and swap
            if (arr[j] > arr[j + 1]) {
                // OPTIMIZED: Efficient swap operation
                swap(arr[j], arr[j + 1]);
                totalSwaps++;
                swapped = true;
            }
        }

        // OPTIMIZED: Early termination if no swaps occurred
        if (!swapped) {
            break;
        }
    }
}

/*
 * SCENARIO 2: QuickSort Implementation
 * Demonstrates O(n log n) average time complexity
 */

// OPTIMIZED: QuickSort with O(n log n) average time complexity
void quickSort(vector<int>& arr, int low, int high) {
    if (low < high) {
        // OPTIMIZED: Partition the array
        int pivotIndex = partition(arr, low, high);

        // OPTIMIZED: Recursively sort subarrays
        quickSort(arr, low, pivotIndex - 1);
        quickSort(arr, pivotIndex + 1, high);
    }
}

// OPTIMIZED: Efficient partitioning for QuickSort
int partition(vector<int>& arr, int low, int high) {
    int pivot = arr[high];
    int i = low - 1;

    for (int j = low; j < high; j++) {
        totalComparisons++;
        if (arr[j] <= pivot) {
            i++;
            swap(arr[i], arr[j]);
            totalSwaps++;
        }
    }

    swap(arr[i + 1], arr[high]);
    totalSwaps++;
    return i + 1;
}

/*
 * SCENARIO 3: MergeSort Implementation
 * Demonstrates O(n log n) guaranteed time complexity
 */

// OPTIMIZED: MergeSort with O(n log n) guaranteed time complexity
void mergeSort(vector<int>& arr, int left, int right) {
    if (left < right) {
        int mid = left + (right - left) / 2;

        // OPTIMIZED: Recursively sort both halves
        mergeSort(arr, left, mid);
        mergeSort(arr, mid + 1, right);

        // OPTIMIZED: Merge the sorted halves
        merge(arr, left, mid, right);
    }
}

// OPTIMIZED: Efficient merging for MergeSort
void merge(vector<int>& arr, int left, int mid, int right) {
    int n1 = mid - left + 1;
    int n2 = right - mid;

    // OPTIMIZED: Create temporary vectors
    vector<int> leftArr(n1);
    vector<int> rightArr(n2);

    // OPTIMIZED: Copy data to temporary vectors
    for (int i = 0; i < n1; i++) {
        leftArr[i] = arr[left + i];
    }
    for (int j = 0; j < n2; j++) {
        rightArr[j] = arr[mid + 1 + j];
    }

    // OPTIMIZED: Merge the temporary vectors back
    int leftIndex = 0, rightIndex = 0;
    int mergedIndex = left;

    while (leftIndex < n1 && rightIndex < n2) {
        totalComparisons++;
        if (leftArr[leftIndex] <= rightArr[rightIndex]) {
            arr[mergedIndex] = leftArr[leftIndex];
            leftIndex++;
        } else {
            arr[mergedIndex] = rightArr[rightIndex];
            rightIndex++;
        }
        mergedIndex++;
    }

    // OPTIMIZED: Copy remaining elements
    while (leftIndex < n1) {
        arr[mergedIndex] = leftArr[leftIndex];
        leftIndex++;
        mergedIndex++;
    }

    while (rightIndex < n2) {
        arr[mergedIndex] = rightArr[rightIndex];
        rightIndex++;
        mergedIndex++;
    }
}

/*
 * SCENARIO 4: Performance Testing Functions
 */

void testOptimizedBubbleSort(int iterations) {
    cout << "=== TESTING OPTIMIZED BUBBLE SORT ===" << endl;
    cout << "This demonstrates early termination optimization" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing Optimized Bubble Sort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test optimized Bubble Sort
        auto start = high_resolution_clock::now();
        bubbleSortOptimized(arr);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "Optimized Bubble Sort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testQuickSort(int iterations) {
    cout << "=== TESTING QUICKSORT ===" << endl;
    cout << "This demonstrates O(n log n) average time complexity" << endl;
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

        // Test QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        quickSort(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "QuickSort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testMergeSort(int iterations) {
    cout << "=== TESTING MERGESORT ===" << endl;
    cout << "This demonstrates O(n log n) guaranteed time complexity" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << "Iterations: " << iterations << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    for (int i = 0; i < iterations; i++) {
        cout << "Testing MergeSort (iteration " << (i + 1) << ")..." << endl;

        // Create vector with random data
        vector<int> arr(ARRAY_SIZE);
        for (int j = 0; j < ARRAY_SIZE; j++) {
            arr[j] = dis(gen);
        }

        // Test MergeSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        mergeSort(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "MergeSort completed:" << endl;
        cout << "  Time: " << duration.count() << " ms" << endl;
        cout << "  Comparisons: " << totalComparisons << endl;
        cout << "  Swaps: " << totalSwaps << endl;
        cout << "  Array size: " << ARRAY_SIZE << endl;
        cout << endl;
    }
}

void testWithDifferentArraySizes() {
    cout << "=== TESTING OPTIMIZED ALGORITHMS WITH DIFFERENT ARRAY SIZES ===" << endl;
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

        // Test QuickSort
        auto start = high_resolution_clock::now();
        totalComparisons = 0;
        totalSwaps = 0;
        quickSort(arr, 0, arr.size() - 1);
        auto end = high_resolution_clock::now();
        auto duration = duration_cast<milliseconds>(end - start);

        cout << "  QuickSort Time: " << duration.count() << " ms" << endl;
        cout << "  QuickSort Comparisons: " << totalComparisons << endl;
        cout << "  QuickSort Swaps: " << totalSwaps << endl;
        cout << "  Time per element: " << (double)duration.count() / size << " ms" << endl;
        cout << endl;
    }
}

void compareAllAlgorithms() {
    cout << "=== COMPARING ALL SORTING ALGORITHMS ===" << endl;
    cout << "This demonstrates performance differences between algorithms" << endl;
    cout << endl;

    random_device rd;
    mt19937 gen(RANDOM_SEED);
    uniform_int_distribution<int> dis(1, 10000);

    // Create vector with random data
    vector<int> arr(ARRAY_SIZE);
    for (int j = 0; j < ARRAY_SIZE; j++) {
        arr[j] = dis(gen);
    }

    // Test Optimized Bubble Sort
    vector<int> arr1 = arr;
    auto start = high_resolution_clock::now();
    bubbleSortOptimized(arr1);
    auto end = high_resolution_clock::now();
    auto bubbleDuration = duration_cast<milliseconds>(end - start);
    long bubbleTime = bubbleDuration.count();
    long bubbleComparisons = totalComparisons;
    long bubbleSwaps = totalSwaps;

    // Test QuickSort
    vector<int> arr2 = arr;
    start = high_resolution_clock::now();
    totalComparisons = 0;
    totalSwaps = 0;
    quickSort(arr2, 0, arr2.size() - 1);
    end = high_resolution_clock::now();
    auto quickDuration = duration_cast<milliseconds>(end - start);
    long quickTime = quickDuration.count();
    long quickComparisons = totalComparisons;
    long quickSwaps = totalSwaps;

    // Test MergeSort
    vector<int> arr3 = arr;
    start = high_resolution_clock::now();
    totalComparisons = 0;
    totalSwaps = 0;
    mergeSort(arr3, 0, arr3.size() - 1);
    end = high_resolution_clock::now();
    auto mergeDuration = duration_cast<milliseconds>(end - start);
    long mergeTime = mergeDuration.count();
    long mergeComparisons = totalComparisons;
    long mergeSwaps = totalSwaps;

    cout << "Algorithm Comparison Results:" << endl;
    cout << "Array size: " << ARRAY_SIZE << endl;
    cout << endl;
    cout << "Optimized Bubble Sort:" << endl;
    cout << "  Time: " << bubbleTime << " ms" << endl;
    cout << "  Comparisons: " << bubbleComparisons << endl;
    cout << "  Swaps: " << bubbleSwaps << endl;
    cout << endl;
    cout << "QuickSort:" << endl;
    cout << "  Time: " << quickTime << " ms" << endl;
    cout << "  Comparisons: " << quickComparisons << endl;
    cout << "  Swaps: " << quickSwaps << endl;
    cout << endl;
    cout << "MergeSort:" << endl;
    cout << "  Time: " << mergeTime << " ms" << endl;
    cout << "  Comparisons: " << mergeComparisons << endl;
    cout << "  Swaps: " << mergeSwaps << endl;
    cout << endl;
    cout << "Performance Improvement:" << endl;
    cout << "  QuickSort vs Bubble Sort: " << (double)bubbleTime / quickTime << "x faster" << endl;
    cout << "  MergeSort vs Bubble Sort: " << (double)bubbleTime / mergeTime << "x faster" << endl;
    cout << endl;
}

int main() {
    cout << "=== OPTIMIZED SORTING ALGORITHMS PERFORMANCE SOLUTION ===" << endl;
    cout << "This program demonstrates optimized sorting implementations:" << endl;
    cout << "1. Optimized Bubble Sort with early termination" << endl;
    cout << "2. QuickSort with O(n log n) average time complexity" << endl;
    cout << "3. MergeSort with O(n log n) guaranteed time complexity" << endl;
    cout << "4. Performance comparison between algorithms" << endl;
    cout << endl;
    cout << "Array size: " << ARRAY_SIZE << " elements" << endl;
    cout << "This will demonstrate significant sorting performance improvements!" << endl;
    cout << endl;

    // Test different optimized algorithms
    testOptimizedBubbleSort(TEST_ITERATIONS);
    testQuickSort(TEST_ITERATIONS);
    testMergeSort(TEST_ITERATIONS);
    testWithDifferentArraySizes();
    compareAllAlgorithms();

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
    cout << "Key Sorting Optimization Techniques Demonstrated:" << endl;
    cout << "- Early termination: Stop when no swaps occur in Bubble Sort" << endl;
    cout << "- Better algorithms: Use O(n log n) algorithms instead of O(n²)" << endl;
    cout << "- Efficient partitioning: Optimize pivot selection in QuickSort" << endl;
    cout << "- Efficient merging: Optimize merge operations in MergeSort" << endl;
    cout << "- Time complexity improvement: O(n²) -> O(n log n)" << endl;
    cout << "- Space complexity optimization: Efficient memory usage" << endl;
    cout << "- Reduced comparisons: Minimize unnecessary comparisons" << endl;
    cout << "- Reduced swaps: Minimize unnecessary swaps" << endl;
    cout << "- Array size tested: " << ARRAY_SIZE << " elements" << endl;

    return 0;
}
