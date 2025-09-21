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

using System;
using System.Diagnostics;

namespace ProfilingExample
{
    class Program
    {
        // ============================================================================
        // CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
        // ============================================================================

        // Array Configuration
        private const int ARRAY_SIZE = 5000;              // Array size for profiling
        private const int TEST_ITERATIONS = 3;            // Number of test iterations
        private const int RANDOM_SEED = 42;               // Random seed for reproducible results

        // Performance Tracking
        private static long totalComparisons = 0;
        private static long totalSwaps = 0;
        private static int maxRecursionDepth = 0;
        private static int currentRecursionDepth = 0;

        // ============================================================================

        /*
         * SCENARIO 1: Inefficient QuickSort Implementation
         * Demonstrates poor pivot selection and O(n²) worst case
         */

        // MAJOR PROBLEM: Inefficient QuickSort with poor pivot selection
        private static void QuickSortInefficient(int[] arr, int low, int high)
        {
            currentRecursionDepth++;
            maxRecursionDepth = Math.Max(maxRecursionDepth, currentRecursionDepth);

            if (low < high)
            {
                // MAJOR PROBLEM: Always choose first element as pivot (worst case for sorted arrays)
                int pivotIndex = PartitionInefficient(arr, low, high);

                // MAJOR PROBLEM: Recursive calls without optimization
                QuickSortInefficient(arr, low, pivotIndex - 1);
                QuickSortInefficient(arr, pivotIndex + 1, high);
            }

            currentRecursionDepth--;
        }

        // MAJOR PROBLEM: Inefficient partitioning algorithm
        private static int PartitionInefficient(int[] arr, int low, int high)
        {
            // MAJOR PROBLEM: Always choose first element as pivot
            int pivot = arr[low];
            int i = low + 1;

            // MAJOR PROBLEM: Inefficient partitioning with multiple passes
            for (int j = low + 1; j <= high; j++)
            {
                totalComparisons++;
                if (arr[j] < pivot)
                {
                    // MAJOR PROBLEM: Inefficient swap operations
                    Swap(arr, i, j);
                    totalSwaps++;
                    i++;
                }
            }

            // MAJOR PROBLEM: Multiple operations to place pivot
            Swap(arr, low, i - 1);
            totalSwaps++;
            return i - 1;
        }

        // MAJOR PROBLEM: Even more inefficient QuickSort with redundant operations
        private static void QuickSortVeryInefficient(int[] arr, int low, int high)
        {
            currentRecursionDepth++;
            maxRecursionDepth = Math.Max(maxRecursionDepth, currentRecursionDepth);

            if (low < high)
            {
                // MAJOR PROBLEM: Multiple partitioning attempts
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    PartitionVeryInefficient(arr, low, high);
                }

                // MAJOR PROBLEM: Choose worst possible pivot (first element)
                int pivotIndex = PartitionVeryInefficient(arr, low, high);

                // MAJOR PROBLEM: Recursive calls without tail recursion optimization
                QuickSortVeryInefficient(arr, low, pivotIndex - 1);
                QuickSortVeryInefficient(arr, pivotIndex + 1, high);
            }

            currentRecursionDepth--;
        }

        // MAJOR PROBLEM: Very inefficient partitioning with redundant operations
        private static int PartitionVeryInefficient(int[] arr, int low, int high)
        {
            // MAJOR PROBLEM: Always choose first element as pivot
            int pivot = arr[low];
            int i = low + 1;

            // MAJOR PROBLEM: Multiple passes over the same data
            for (int pass = 0; pass < 2; pass++)
            {
                for (int j = low + 1; j <= high; j++)
                {
                    totalComparisons++;
                    if (arr[j] < pivot)
                    {
                        // MAJOR PROBLEM: Redundant swap operations
                        Swap(arr, i, j);
                        totalSwaps++;
                        i++;

                        // MAJOR PROBLEM: Redundant comparison after swap
                        if (arr[i - 1] > arr[j])
                        {
                            totalComparisons++;
                        }
                    }
                }
            }

            // MAJOR PROBLEM: Multiple operations to place pivot
            Swap(arr, low, i - 1);
            totalSwaps++;
            return i - 1;
        }

        // MAJOR PROBLEM: Inefficient swap operation
        private static void Swap(int[] arr, int i, int j)
        {
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        /*
         * SCENARIO 2: Performance Testing Functions
         */

        private static void TestQuickSortPerformance(int iterations)
        {
            Console.WriteLine("=== TESTING QUICKSORT PERFORMANCE ===");
            Console.WriteLine("This demonstrates O(n²) worst case scenarios");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing QuickSort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test inefficient QuickSort
                Console.WriteLine("  Starting inefficient QuickSort (poor pivot selection)...");
                stopwatch.Restart();
                totalComparisons = 0;
                totalSwaps = 0;
                maxRecursionDepth = 0;
                currentRecursionDepth = 0;
                QuickSortInefficient(arr, 0, arr.Length - 1);
                stopwatch.Stop();
                Console.WriteLine("  QuickSort completed!");

                Console.WriteLine($"QuickSort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestVeryInefficientQuickSort(int iterations)
        {
            Console.WriteLine("=== TESTING VERY INEFFICIENT QUICKSORT ===");
            Console.WriteLine("This demonstrates severe O(n²) performance issues");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Very Inefficient QuickSort (iteration {i + 1})...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }

                // Test very inefficient QuickSort
                stopwatch.Restart();
                totalComparisons = 0;
                totalSwaps = 0;
                maxRecursionDepth = 0;
                currentRecursionDepth = 0;
                QuickSortVeryInefficient(arr, 0, arr.Length - 1);
                stopwatch.Stop();

                Console.WriteLine($"Very Inefficient QuickSort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestWithWorstCaseInput()
        {
            Console.WriteLine("=== TESTING QUICKSORT WITH WORST CASE INPUT ===");
            Console.WriteLine("This demonstrates O(n²) worst case performance");
            Console.WriteLine();

            var stopwatch = new Stopwatch();

            // MAJOR PROBLEM: Create worst case input (sorted array)
            int[] arr = new int[ARRAY_SIZE];
            for (int i = 0; i < ARRAY_SIZE; i++)
            {
                arr[i] = i; // Sorted array - worst case for first element pivot
            }

            Console.WriteLine($"Testing with sorted array (worst case) - size: {ARRAY_SIZE}");

            // Test inefficient QuickSort with worst case input
            stopwatch.Restart();
            totalComparisons = 0;
            totalSwaps = 0;
            maxRecursionDepth = 0;
            currentRecursionDepth = 0;
            QuickSortInefficient(arr, 0, arr.Length - 1);
            stopwatch.Stop();

            Console.WriteLine($"QuickSort with worst case input completed:");
            Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
            Console.WriteLine($"  Swaps: {totalSwaps:N0}");
            Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
            Console.WriteLine($"  Expected comparisons for O(n²): {ARRAY_SIZE * ARRAY_SIZE:N0}");
            Console.WriteLine();
        }

        private static void TestWithDifferentArraySizes()
        {
            Console.WriteLine("=== TESTING QUICKSORT WITH DIFFERENT ARRAY SIZES ===");
            Console.WriteLine("This demonstrates performance scaling");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            int[] sizes = { 100, 500, 1000, 2000, 3000 };

            foreach (int size in sizes)
            {
                Console.WriteLine($"Testing with array size: {size}");

                // Create array with random data
                int[] arr = new int[size];
                for (int j = 0; j < size; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }

                // Test QuickSort
                stopwatch.Restart();
                totalComparisons = 0;
                totalSwaps = 0;
                maxRecursionDepth = 0;
                currentRecursionDepth = 0;
                QuickSortInefficient(arr, 0, arr.Length - 1);
                stopwatch.Stop();

                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
                Console.WriteLine($"  Time per element: {(double)stopwatch.ElapsedMilliseconds / size:F4} ms");
                Console.WriteLine();
            }
        }

        private static void VerifySortingCorrectness()
        {
            Console.WriteLine("=== VERIFYING SORTING CORRECTNESS ===");
            Console.WriteLine("This verifies that the sorting algorithm works correctly");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            int[] arr = new int[100];

            // Create array with random data
            for (int i = 0; i < 100; i++)
            {
                arr[i] = random.Next(1, 1000);
            }

            Console.WriteLine("Original array (first 10 elements):");
            for (int i = 0; i < 10; i++)
            {
                Console.Write($"{arr[i]} ");
            }
            Console.WriteLine();

            // Sort the array
            totalComparisons = 0;
            totalSwaps = 0;
            maxRecursionDepth = 0;
            currentRecursionDepth = 0;
            QuickSortInefficient(arr, 0, arr.Length - 1);

            Console.WriteLine("Sorted array (first 10 elements):");
            for (int i = 0; i < 10; i++)
            {
                Console.Write($"{arr[i]} ");
            }
            Console.WriteLine();

            // Verify sorting correctness
            bool isSorted = true;
            for (int i = 0; i < arr.Length - 1; i++)
            {
                if (arr[i] > arr[i + 1])
                {
                    isSorted = false;
                    break;
                }
            }

            Console.WriteLine($"Array is correctly sorted: {isSorted}");
            Console.WriteLine($"Total comparisons: {totalComparisons:N0}");
            Console.WriteLine($"Total swaps: {totalSwaps:N0}");
            Console.WriteLine($"Max recursion depth: {maxRecursionDepth}");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== QUICKSORT PERFORMANCE INVESTIGATION ===");
            Console.WriteLine("This program demonstrates QuickSort performance issues:");
            Console.WriteLine("1. Inefficient QuickSort with poor pivot selection");
            Console.WriteLine("2. Very inefficient QuickSort with redundant operations");
            Console.WriteLine("3. Worst case input scenarios");
            Console.WriteLine("4. Performance scaling with different array sizes");
            Console.WriteLine("5. Sorting correctness verification");
            Console.WriteLine();
            Console.WriteLine($"Array size: {ARRAY_SIZE} elements");
            Console.WriteLine("This will demonstrate severe sorting performance issues!");
            Console.WriteLine();

            // Test different scenarios
            TestQuickSortPerformance(TEST_ITERATIONS);
            TestVeryInefficientQuickSort(TEST_ITERATIONS);
            TestWithWorstCaseInput();
            TestWithDifferentArraySizes();
            VerifySortingCorrectness();

            Console.WriteLine("=== OVERALL ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Observe the O(n²) worst case performance!");
            Console.WriteLine("3. Look for functions with high call counts and individual time consumption");
            Console.WriteLine("4. Analyze the recursion depth and stack usage");
            Console.WriteLine("5. Examine comparison and swap operation costs");
            Console.WriteLine("6. Look for redundant operations in sorting algorithms");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called functions");
            Console.WriteLine("8. Check for inefficient algorithm implementations");
            Console.WriteLine();
            Console.WriteLine("Key QuickSort Performance Issues Demonstrated:");
            Console.WriteLine("- O(n²) worst case time complexity with poor pivot selection");
            Console.WriteLine("- Deep recursion causing stack overflow potential");
            Console.WriteLine("- Inefficient partitioning algorithms");
            Console.WriteLine("- No optimization for small arrays");
            Console.WriteLine("- Redundant operations in sorting process");
            Console.WriteLine("- Poor performance with sorted or nearly sorted data");
            Console.WriteLine("- Multiple passes over the same data");
            Console.WriteLine($"- Array size tested: {ARRAY_SIZE} elements");

            Console.WriteLine("Program completed successfully!");
            Console.WriteLine("Exiting now...");
            
            // Exit immediately to stop profiler timing
            return;
        }
    }
}
