/*
 * PROFILING EXAMPLE: Optimized Bubble Sort Performance Solution
 * 
 * This example demonstrates optimized Bubble Sort implementations:
 * - Bubble Sort with early termination optimization
 * - Cocktail Sort (bidirectional bubble sort)
 * - Optimized Bubble Sort with reduced comparisons
 * - Efficient comparison and swap operations
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for Bubble Sort
 * - Show performance improvements through better Bubble Sort variants
 * - Compare inefficient vs optimized Bubble Sort solutions
 * - Identify best practices for Bubble Sort algorithm design
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized Bubble Sort implementations.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance improvements and optimization patterns.
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
        private const int ARRAY_SIZE = 5000;              // Array size for profiling (same as problem version)
        private const int TEST_ITERATIONS = 3;            // Number of test iterations
        private const int RANDOM_SEED = 42;               // Random seed for reproducible results

        // Performance Tracking
        private static long totalComparisons = 0;
        private static long totalSwaps = 0;

        // ============================================================================

        /*
         * SCENARIO 1: Optimized Bubble Sort Implementation
         * Demonstrates early termination and efficiency improvements
         */

        // OPTIMIZED: Bubble Sort with early termination - O(n²) worst case, O(n) best case
        private static void BubbleSortOptimized(int[] arr)
        {
            int n = arr.Length;
            totalComparisons = 0;
            totalSwaps = 0;

            // OPTIMIZED: Early termination when no swaps occur
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;

                for (int j = 0; j < n - i - 1; j++)
                {
                    totalComparisons++;

                    // OPTIMIZED: Efficient comparison and swap
                    if (arr[j] > arr[j + 1])
                    {
                        // OPTIMIZED: Efficient swap operation
                        Swap(arr, j, j + 1);
                        totalSwaps++;
                        swapped = true;
                    }
                }

                // OPTIMIZED: Early termination if no swaps occurred
                if (!swapped)
                {
                    break;
                }
            }
        }

        // OPTIMIZED: Efficient swap operation
        private static void Swap(int[] arr, int i, int j)
        {
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        /*
         * SCENARIO 2: Cocktail Sort Implementation
         * Demonstrates bidirectional bubble sort optimization
         */

        // OPTIMIZED: Cocktail Sort (bidirectional bubble sort) - O(n²) worst case, O(n) best case
        private static void CocktailSort(int[] arr)
        {
            int n = arr.Length;
            totalComparisons = 0;
            totalSwaps = 0;
            bool swapped = true;
            int start = 0;
            int end = n - 1;

            while (swapped)
            {
                swapped = false;

                // OPTIMIZED: Forward pass
                for (int i = start; i < end; i++)
                {
                    totalComparisons++;
                    if (arr[i] > arr[i + 1])
                    {
                        Swap(arr, i, i + 1);
                        totalSwaps++;
                        swapped = true;
                    }
                }

                if (!swapped)
                    break;

                swapped = false;
                end--;

                // OPTIMIZED: Backward pass
                for (int i = end; i > start; i--)
                {
                    totalComparisons++;
                    if (arr[i] < arr[i - 1])
                    {
                        Swap(arr, i, i - 1);
                        totalSwaps++;
                        swapped = true;
                    }
                }

                start++;
            }
        }

        /*
         * SCENARIO 3: Optimized Bubble Sort with Reduced Comparisons
         * Demonstrates further optimization techniques
         */

        // OPTIMIZED: Bubble Sort with reduced comparisons and early termination
        private static void BubbleSortReducedComparisons(int[] arr)
        {
            int n = arr.Length;
            totalComparisons = 0;
            totalSwaps = 0;

            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                int lastSwapIndex = 0;

                for (int j = 0; j < n - i - 1; j++)
                {
                    totalComparisons++;
                    if (arr[j] > arr[j + 1])
                    {
                        Swap(arr, j, j + 1);
                        totalSwaps++;
                        swapped = true;
                        lastSwapIndex = j;
                    }
                }

                // OPTIMIZED: Skip already sorted elements
                if (!swapped)
                    break;

                // OPTIMIZED: Reduce the range for next iteration
                n = lastSwapIndex + 1;
            }
        }

        /*
         * SCENARIO 4: Performance Testing Functions
         */

        private static void TestOptimizedBubbleSort(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED BUBBLE SORT ===");
            Console.WriteLine("This demonstrates early termination optimization");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Optimized Bubble Sort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test optimized Bubble Sort
                Console.WriteLine("  Starting Optimized Bubble Sort...");
                stopwatch.Restart();
                BubbleSortOptimized(arr);
                stopwatch.Stop();
                Console.WriteLine("  Sorting completed!");

                Console.WriteLine($"Optimized Bubble Sort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestCocktailSort(int iterations)
        {
            Console.WriteLine("=== TESTING COCKTAIL SORT ===");
            Console.WriteLine("This demonstrates bidirectional bubble sort optimization");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Cocktail Sort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test Cocktail Sort
                Console.WriteLine("  Starting Cocktail Sort (bidirectional bubble sort)...");
                stopwatch.Restart();
                CocktailSort(arr);
                stopwatch.Stop();
                Console.WriteLine("  Cocktail Sort completed!");

                Console.WriteLine($"Cocktail Sort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestBubbleSortReducedComparisons(int iterations)
        {
            Console.WriteLine("=== TESTING BUBBLE SORT WITH REDUCED COMPARISONS ===");
            Console.WriteLine("This demonstrates further bubble sort optimizations");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Bubble Sort with Reduced Comparisons (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test Bubble Sort with Reduced Comparisons
                Console.WriteLine("  Starting Bubble Sort with Reduced Comparisons...");
                stopwatch.Restart();
                BubbleSortReducedComparisons(arr);
                stopwatch.Stop();
                Console.WriteLine("  Bubble Sort with Reduced Comparisons completed!");

                Console.WriteLine($"Bubble Sort with Reduced Comparisons completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestWithDifferentArraySizes()
        {
            Console.WriteLine("=== TESTING OPTIMIZED BUBBLE SORT WITH DIFFERENT ARRAY SIZES ===");
            Console.WriteLine("This demonstrates O(n²) complexity scaling with optimizations");
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

                // Test Optimized Bubble Sort
                Console.WriteLine($"  Starting Optimized Bubble Sort for {size} elements...");
                stopwatch.Restart();
                BubbleSortOptimized(arr);
                stopwatch.Stop();
                Console.WriteLine($"  Sorting completed for {size} elements!");

                Console.WriteLine($"  Optimized Bubble Sort Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Optimized Bubble Sort Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Optimized Bubble Sort Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Time per element: {(double)stopwatch.ElapsedMilliseconds / size:F4} ms");
                Console.WriteLine();
            }
        }

        private static void CompareAllBubbleSortVariants()
        {
            Console.WriteLine("=== COMPARING ALL BUBBLE SORT VARIANTS ===");
            Console.WriteLine("This demonstrates performance differences between bubble sort optimizations");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            Console.WriteLine($"Creating base array with {ARRAY_SIZE} random elements...");
            // Create array with random data
            int[] arr = new int[ARRAY_SIZE];
            for (int j = 0; j < ARRAY_SIZE; j++)
            {
                arr[j] = random.Next(1, 10000);
            }
            Console.WriteLine("Base array created successfully!");

            Console.WriteLine();
            Console.WriteLine("Testing Optimized Bubble Sort...");
            // Test Optimized Bubble Sort
            int[] arr1 = (int[])arr.Clone();
            stopwatch.Restart();
            BubbleSortOptimized(arr1);
            stopwatch.Stop();
            long optimizedTime = stopwatch.ElapsedMilliseconds;
            long optimizedComparisons = totalComparisons;
            long optimizedSwaps = totalSwaps;
            Console.WriteLine("Optimized Bubble Sort completed!");

            Console.WriteLine();
            Console.WriteLine("Testing Cocktail Sort...");
            // Test Cocktail Sort
            int[] arr2 = (int[])arr.Clone();
            stopwatch.Restart();
            CocktailSort(arr2);
            stopwatch.Stop();
            long cocktailTime = stopwatch.ElapsedMilliseconds;
            long cocktailComparisons = totalComparisons;
            long cocktailSwaps = totalSwaps;
            Console.WriteLine("Cocktail Sort completed!");

            Console.WriteLine();
            Console.WriteLine("Testing Bubble Sort with Reduced Comparisons...");
            // Test Bubble Sort with Reduced Comparisons
            int[] arr3 = (int[])arr.Clone();
            stopwatch.Restart();
            BubbleSortReducedComparisons(arr3);
            stopwatch.Stop();
            long reducedTime = stopwatch.ElapsedMilliseconds;
            long reducedComparisons = totalComparisons;
            long reducedSwaps = totalSwaps;
            Console.WriteLine("Bubble Sort with Reduced Comparisons completed!");

            Console.WriteLine();
            Console.WriteLine($"Bubble Sort Variants Comparison Results:");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine();
            Console.WriteLine($"Optimized Bubble Sort:");
            Console.WriteLine($"  Time: {optimizedTime} ms");
            Console.WriteLine($"  Comparisons: {optimizedComparisons:N0}");
            Console.WriteLine($"  Swaps: {optimizedSwaps:N0}");
            Console.WriteLine();
            Console.WriteLine($"Cocktail Sort:");
            Console.WriteLine($"  Time: {cocktailTime} ms");
            Console.WriteLine($"  Comparisons: {cocktailComparisons:N0}");
            Console.WriteLine($"  Swaps: {cocktailSwaps:N0}");
            Console.WriteLine();
            Console.WriteLine($"Bubble Sort with Reduced Comparisons:");
            Console.WriteLine($"  Time: {reducedTime} ms");
            Console.WriteLine($"  Comparisons: {reducedComparisons:N0}");
            Console.WriteLine($"  Swaps: {reducedSwaps:N0}");
            Console.WriteLine();
            Console.WriteLine($"Performance Comparison:");
            Console.WriteLine($"  Cocktail vs Optimized: {(double)optimizedTime / cocktailTime:F2}x");
            Console.WriteLine($"  Reduced Comparisons vs Optimized: {(double)optimizedTime / reducedTime:F2}x");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== OPTIMIZED BUBBLE SORT PERFORMANCE SOLUTION ===");
            Console.WriteLine("This program demonstrates optimized Bubble Sort implementations:");
            Console.WriteLine("1. Optimized Bubble Sort with early termination");
            Console.WriteLine("2. Cocktail Sort (bidirectional bubble sort)");
            Console.WriteLine("3. Bubble Sort with reduced comparisons");
            Console.WriteLine("4. Performance comparison between bubble sort variants");
            Console.WriteLine();
            Console.WriteLine($"Array size: {ARRAY_SIZE} elements");
            Console.WriteLine("This will demonstrate bubble sort optimization improvements!");
            Console.WriteLine();

            // Test different optimized bubble sort algorithms
            TestOptimizedBubbleSort(TEST_ITERATIONS);
            TestCocktailSort(TEST_ITERATIONS);
            TestBubbleSortReducedComparisons(TEST_ITERATIONS);
            TestWithDifferentArraySizes();
            CompareAllBubbleSortVariants();

            Console.WriteLine("=== OVERALL OPTIMIZATION ANALYSIS ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Compare with the inefficient version to see performance improvements!");
            Console.WriteLine("3. Observe the reduction in comparisons and swaps");
            Console.WriteLine("4. Analyze the efficiency of optimized bubble sort algorithms");
            Console.WriteLine("5. Examine bubble sort optimization techniques");
            Console.WriteLine("6. Look for optimization techniques in action");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called functions");
            Console.WriteLine("8. Check for improved bubble sort patterns");
            Console.WriteLine();
            Console.WriteLine("Key Bubble Sort Optimization Techniques Demonstrated:");
            Console.WriteLine("- Early termination: Stop when no swaps occur in Bubble Sort");
            Console.WriteLine("- Bidirectional sorting: Cocktail sort reduces passes needed");
            Console.WriteLine("- Reduced comparisons: Skip already sorted elements");
            Console.WriteLine("- Efficient swapping: Optimize swap operations");
            Console.WriteLine("- Time complexity: Still O(n²) but with better constants");
            Console.WriteLine("- Space complexity optimization: Efficient memory usage");
            Console.WriteLine("- Reduced comparisons: Minimize unnecessary comparisons");
            Console.WriteLine("- Reduced swaps: Minimize unnecessary swaps");
            Console.WriteLine($"- Array size tested: {ARRAY_SIZE} elements");

            Console.WriteLine("Program completed successfully!");
            Console.WriteLine("Exiting now...");
            
            // Exit immediately to stop profiler timing
            return;
        }
    }
}