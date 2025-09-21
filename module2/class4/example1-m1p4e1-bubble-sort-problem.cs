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
        private const int ARRAY_SIZE = 5000;              // Array size for profiling (5000 = shows O(n²) complexity)
        private const int TEST_ITERATIONS = 3;            // Number of test iterations
        private const int RANDOM_SEED = 42;               // Random seed for reproducible results

        // Performance Tracking
        private static long totalComparisons = 0;
        private static long totalSwaps = 0;

        // ============================================================================

        /*
         * SCENARIO 1: Inefficient Bubble Sort Implementation
         * Demonstrates O(n²) time complexity and performance issues
         */

        // MAJOR PROBLEM: Inefficient Bubble Sort with O(n²) complexity
        private static void BubbleSortInefficient(int[] arr)
        {
            int n = arr.Length;
            totalComparisons = 0;
            totalSwaps = 0;

            // MAJOR PROBLEM: Nested loops causing O(n²) complexity
            for (int i = 0; i < n - 1; i++)
            {
                // MAJOR PROBLEM: No early termination optimization
                for (int j = 0; j < n - i - 1; j++)
                {
                    totalComparisons++;

                    // MAJOR PROBLEM: Inefficient comparison and swap
                    if (arr[j] > arr[j + 1])
                    {
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
        private static void BubbleSortVeryInefficient(int[] arr)
        {
            int n = arr.Length;
            totalComparisons = 0;
            totalSwaps = 0;

            // MAJOR PROBLEM: Extra outer loop causing unnecessary iterations
            for (int pass = 0; pass < n; pass++)
            {
                // MAJOR PROBLEM: Nested loops with redundant comparisons
                for (int i = 0; i < n - 1; i++)
                {
                    for (int j = 0; j < n - i - 1; j++)
                    {
                        totalComparisons++;

                        // MAJOR PROBLEM: Multiple comparisons for same elements
                        if (arr[j] > arr[j + 1])
                        {
                            // MAJOR PROBLEM: Inefficient swap with multiple operations
                            int temp = arr[j];
                            arr[j] = arr[j + 1];
                            arr[j + 1] = temp;
                            totalSwaps++;

                            // MAJOR PROBLEM: Redundant comparison after swap
                            if (arr[j] < arr[j + 1])
                            {
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

        private static void TestBubbleSortPerformance(int iterations)
        {
            Console.WriteLine("=== TESTING BUBBLE SORT PERFORMANCE ===");
            Console.WriteLine("This demonstrates O(n²) time complexity issues");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Bubble Sort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test inefficient Bubble Sort
                Console.WriteLine("  Starting inefficient Bubble Sort (no early termination)...");
                stopwatch.Restart();
                BubbleSortInefficient(arr);
                stopwatch.Stop();
                Console.WriteLine("  Bubble Sort completed!");

                Console.WriteLine($"Bubble Sort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestVeryInefficientBubbleSort(int iterations)
        {
            Console.WriteLine("=== TESTING VERY INEFFICIENT BUBBLE SORT ===");
            Console.WriteLine("This demonstrates severe O(n²) performance issues");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Very Inefficient Bubble Sort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test very inefficient Bubble Sort
                Console.WriteLine("  Starting very inefficient Bubble Sort (redundant operations)...");
                stopwatch.Restart();
                BubbleSortVeryInefficient(arr);
                stopwatch.Stop();
                Console.WriteLine("  Very Inefficient Bubble Sort completed!");

                Console.WriteLine($"Very Inefficient Bubble Sort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestWithDifferentArraySizes()
        {
            Console.WriteLine("=== TESTING BUBBLE SORT WITH DIFFERENT ARRAY SIZES ===");
            Console.WriteLine("This demonstrates O(n²) complexity scaling");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            int[] sizes = { 100, 500, 1000, 2000, 3000 };

            foreach (int size in sizes)
            {
                Console.WriteLine($"Testing with array size: {size}");
                Console.WriteLine($"  Creating array with {size} random elements...");

                // Create array with random data
                int[] arr = new int[size];
                for (int j = 0; j < size; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created successfully");

                // Test Bubble Sort
                Console.WriteLine($"  Starting inefficient Bubble Sort for {size} elements...");
                stopwatch.Restart();
                BubbleSortInefficient(arr);
                stopwatch.Stop();
                Console.WriteLine($"  Sorting completed for {size} elements!");

                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
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
            Console.WriteLine("Creating small test array with 100 elements...");
            int[] arr = new int[100];

            // Create array with random data
            for (int i = 0; i < 100; i++)
            {
                arr[i] = random.Next(1, 1000);
            }
            Console.WriteLine("Test array created successfully!");

            Console.WriteLine("Original array (first 10 elements):");
            for (int i = 0; i < 10; i++)
            {
                Console.Write($"{arr[i]} ");
            }
            Console.WriteLine();

            Console.WriteLine("Starting Bubble Sort verification...");
            // Sort the array
            BubbleSortInefficient(arr);
            Console.WriteLine("Bubble Sort verification completed!");

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
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== BUBBLE SORT PERFORMANCE INVESTIGATION ===");
            Console.WriteLine("This program demonstrates Bubble Sort performance issues:");
            Console.WriteLine("1. Inefficient Bubble Sort with O(n²) complexity");
            Console.WriteLine("2. Very inefficient Bubble Sort with redundant operations");
            Console.WriteLine("3. Performance scaling with different array sizes");
            Console.WriteLine("4. Sorting correctness verification");
            Console.WriteLine();
            Console.WriteLine($"Array size: {ARRAY_SIZE} elements");
            Console.WriteLine($"This will demonstrate severe sorting performance issues!");
            Console.WriteLine();

            // Test different scenarios
            TestBubbleSortPerformance(TEST_ITERATIONS);
            TestVeryInefficientBubbleSort(TEST_ITERATIONS);
            TestWithDifferentArraySizes();
            VerifySortingCorrectness();

            Console.WriteLine("=== OVERALL ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Observe the O(n²) time complexity scaling!");
            Console.WriteLine("3. Look for functions with high call counts and individual time consumption");
            Console.WriteLine("4. Analyze the nested loop performance patterns");
            Console.WriteLine("5. Examine comparison and swap operation costs");
            Console.WriteLine("6. Look for redundant operations in sorting algorithms");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called functions");
            Console.WriteLine("8. Check for inefficient algorithm implementations");
            Console.WriteLine();
            Console.WriteLine("Key Bubble Sort Performance Issues Demonstrated:");
            Console.WriteLine("- O(n²) time complexity causing poor performance with large datasets");
            Console.WriteLine("- Nested loops with redundant comparisons");
            Console.WriteLine("- Inefficient swap operations");
            Console.WriteLine("- No early termination optimization");
            Console.WriteLine("- Multiple passes over the same data");
            Console.WriteLine("- Redundant operations in sorting process");
            Console.WriteLine("- Poor scaling with increasing array size");
            Console.WriteLine($"- Array size tested: {ARRAY_SIZE} elements");

            Console.WriteLine("Program completed successfully!");
            Console.WriteLine("Exiting now...");
            
            // Exit immediately to stop profiler timing
            return;
        }
    }
}
