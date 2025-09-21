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
        private const int INSERTION_SORT_THRESHOLD = 10;  // Threshold for switching to insertion sort

        // Performance Tracking
        private static long totalComparisons = 0;
        private static long totalSwaps = 0;
        private static int maxRecursionDepth = 0;
        private static int currentRecursionDepth = 0;

        // ============================================================================

        /*
         * SCENARIO 1: Optimized QuickSort Implementation
         * Demonstrates efficient pivot selection and O(n log n) average case
         */

        // OPTIMIZED: QuickSort with median-of-three pivot selection
        private static void QuickSortOptimized(int[] arr, int low, int high)
        {
            currentRecursionDepth++;
            maxRecursionDepth = Math.Max(maxRecursionDepth, currentRecursionDepth);

            if (low < high)
            {
                // OPTIMIZED: Use insertion sort for small arrays
                if (high - low + 1 < INSERTION_SORT_THRESHOLD)
                {
                    InsertionSort(arr, low, high);
                    currentRecursionDepth--;
                    return;
                }

                // OPTIMIZED: Median-of-three pivot selection
                int pivotIndex = MedianOfThree(arr, low, high);
                Swap(arr, pivotIndex, high); // Move pivot to end

                // OPTIMIZED: Efficient partitioning
                int partitionIndex = PartitionOptimized(arr, low, high);

                // OPTIMIZED: Tail recursion optimization
                if (partitionIndex - low < high - partitionIndex)
                {
                    QuickSortOptimized(arr, low, partitionIndex - 1);
                    QuickSortOptimized(arr, partitionIndex + 1, high);
                }
                else
                {
                    QuickSortOptimized(arr, partitionIndex + 1, high);
                    QuickSortOptimized(arr, low, partitionIndex - 1);
                }
            }

            currentRecursionDepth--;
        }

        // OPTIMIZED: Median-of-three pivot selection
        private static int MedianOfThree(int[] arr, int low, int high)
        {
            int mid = low + (high - low) / 2;

            // OPTIMIZED: Find median of three elements
            if (arr[mid] < arr[low])
            {
                if (arr[high] < arr[mid])
                    return mid;
                else if (arr[high] < arr[low])
                    return high;
                else
                    return low;
            }
            else
            {
                if (arr[high] < arr[mid])
                {
                    if (arr[high] < arr[low])
                        return low;
                    else
                        return high;
                }
                else
                    return mid;
            }
        }

        // OPTIMIZED: Efficient partitioning algorithm
        private static int PartitionOptimized(int[] arr, int low, int high)
        {
            int pivot = arr[high];
            int i = low - 1;

            // OPTIMIZED: Single pass partitioning
            for (int j = low; j < high; j++)
            {
                totalComparisons++;
                if (arr[j] <= pivot)
                {
                    i++;
                    Swap(arr, i, j);
                    totalSwaps++;
                }
            }

            // OPTIMIZED: Place pivot in correct position
            Swap(arr, i + 1, high);
            totalSwaps++;
            return i + 1;
        }

        // OPTIMIZED: Insertion sort for small arrays
        private static void InsertionSort(int[] arr, int low, int high)
        {
            for (int i = low + 1; i <= high; i++)
            {
                int key = arr[i];
                int j = i - 1;

                while (j >= low && arr[j] > key)
                {
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
        private static void QuickSortThreeWay(int[] arr, int low, int high)
        {
            currentRecursionDepth++;
            maxRecursionDepth = Math.Max(maxRecursionDepth, currentRecursionDepth);

            if (low < high)
            {
                // OPTIMIZED: Use insertion sort for small arrays
                if (high - low + 1 < INSERTION_SORT_THRESHOLD)
                {
                    InsertionSort(arr, low, high);
                    currentRecursionDepth--;
                    return;
                }

                // OPTIMIZED: Three-way partitioning
                var (lt, gt) = PartitionThreeWay(arr, low, high);

                // OPTIMIZED: Recursively sort subarrays
                QuickSortThreeWay(arr, low, lt - 1);
                QuickSortThreeWay(arr, gt + 1, high);
            }

            currentRecursionDepth--;
        }

        // OPTIMIZED: Three-way partitioning for duplicate elements
        private static (int lt, int gt) PartitionThreeWay(int[] arr, int low, int high)
        {
            int pivot = arr[low];
            int lt = low;
            int gt = high;
            int i = low + 1;

            while (i <= gt)
            {
                totalComparisons++;
                if (arr[i] < pivot)
                {
                    Swap(arr, lt++, i++);
                    totalSwaps++;
                }
                else if (arr[i] > pivot)
                {
                    Swap(arr, i, gt--);
                    totalSwaps++;
                }
                else
                {
                    i++;
                }
            }

            return (lt, gt);
        }

        /*
         * SCENARIO 3: Performance Testing Functions
         */

        private static void TestQuickSortOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED QUICKSORT ===");
            Console.WriteLine("This demonstrates O(n log n) average time complexity");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Optimized QuickSort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with random data...");

                // Create array with random data
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 10000);
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} random elements");

                // Test optimized QuickSort
                Console.WriteLine("  Starting Optimized QuickSort with median-of-three pivot...");
                stopwatch.Restart();
                totalComparisons = 0;
                totalSwaps = 0;
                maxRecursionDepth = 0;
                currentRecursionDepth = 0;
                QuickSortOptimized(arr, 0, arr.Length - 1);
                stopwatch.Stop();
                Console.WriteLine("  Optimized QuickSort completed!");

                Console.WriteLine($"Optimized QuickSort completed:");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
                Console.WriteLine($"  Array size: {ARRAY_SIZE}");
                Console.WriteLine();
            }
        }

        private static void TestThreeWayQuickSort(int iterations)
        {
            Console.WriteLine("=== TESTING THREE-WAY QUICKSORT ===");
            Console.WriteLine("This demonstrates efficient handling of duplicate elements");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var random = new Random(RANDOM_SEED);
            var stopwatch = new Stopwatch();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Testing Three-Way QuickSort (iteration {i + 1})...");
                Console.WriteLine("  Creating array with many duplicate elements...");

                // Create array with many duplicates
                int[] arr = new int[ARRAY_SIZE];
                for (int j = 0; j < ARRAY_SIZE; j++)
                {
                    arr[j] = random.Next(1, 100); // Many duplicates
                }
                Console.WriteLine($"  Array created with {ARRAY_SIZE} elements (many duplicates)");

                // Test three-way QuickSort
                Console.WriteLine("  Starting Three-Way QuickSort for duplicate handling...");
                stopwatch.Restart();
                totalComparisons = 0;
                totalSwaps = 0;
                maxRecursionDepth = 0;
                currentRecursionDepth = 0;
                QuickSortThreeWay(arr, 0, arr.Length - 1);
                stopwatch.Stop();
                Console.WriteLine("  Three-Way QuickSort completed!");

                Console.WriteLine($"Three-Way QuickSort completed:");
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
            Console.WriteLine("=== TESTING OPTIMIZED QUICKSORT WITH WORST CASE INPUT ===");
            Console.WriteLine("This demonstrates improved worst case performance");
            Console.WriteLine();

            var stopwatch = new Stopwatch();

            Console.WriteLine($"Creating sorted array (worst case scenario) - size: {ARRAY_SIZE}");
            // Create worst case input (sorted array)
            int[] arr = new int[ARRAY_SIZE];
            for (int i = 0; i < ARRAY_SIZE; i++)
            {
                arr[i] = i; // Sorted array
            }
            Console.WriteLine("Sorted array created successfully!");

            Console.WriteLine("Testing Optimized QuickSort with worst case input...");
            // Test optimized QuickSort with worst case input
            stopwatch.Restart();
            totalComparisons = 0;
            totalSwaps = 0;
            maxRecursionDepth = 0;
            currentRecursionDepth = 0;
            QuickSortOptimized(arr, 0, arr.Length - 1);
            stopwatch.Stop();
            Console.WriteLine("Worst case test completed!");

            Console.WriteLine($"Optimized QuickSort with worst case input completed:");
            Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
            Console.WriteLine($"  Swaps: {totalSwaps:N0}");
            Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
            Console.WriteLine($"  Expected comparisons for O(n log n): {ARRAY_SIZE * Math.Log2(ARRAY_SIZE):N0}");
            Console.WriteLine();
        }

        private static void TestWithDifferentArraySizes()
        {
            Console.WriteLine("=== TESTING OPTIMIZED QUICKSORT WITH DIFFERENT ARRAY SIZES ===");
            Console.WriteLine("This demonstrates O(n log n) complexity scaling");
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

                // Test optimized QuickSort
                Console.WriteLine($"  Starting Optimized QuickSort for {size} elements...");
                stopwatch.Restart();
                totalComparisons = 0;
                totalSwaps = 0;
                maxRecursionDepth = 0;
                currentRecursionDepth = 0;
                QuickSortOptimized(arr, 0, arr.Length - 1);
                stopwatch.Stop();
                Console.WriteLine($"  Sorting completed for {size} elements!");

                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Comparisons: {totalComparisons:N0}");
                Console.WriteLine($"  Swaps: {totalSwaps:N0}");
                Console.WriteLine($"  Max recursion depth: {maxRecursionDepth}");
                Console.WriteLine($"  Time per element: {(double)stopwatch.ElapsedMilliseconds / size:F4} ms");
                Console.WriteLine();
            }
        }

        private static void CompareAllQuickSortVariants()
        {
            Console.WriteLine("=== COMPARING ALL QUICKSORT VARIANTS ===");
            Console.WriteLine("This demonstrates performance differences between variants");
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
            Console.WriteLine("Testing Optimized QuickSort...");
            // Test Optimized QuickSort
            int[] arr1 = (int[])arr.Clone();
            stopwatch.Restart();
            totalComparisons = 0;
            totalSwaps = 0;
            maxRecursionDepth = 0;
            currentRecursionDepth = 0;
            QuickSortOptimized(arr1, 0, arr1.Length - 1);
            stopwatch.Stop();
            long optimizedTime = stopwatch.ElapsedMilliseconds;
            long optimizedComparisons = totalComparisons;
            long optimizedSwaps = totalSwaps;
            int optimizedRecursion = maxRecursionDepth;
            Console.WriteLine("Optimized QuickSort completed!");

            Console.WriteLine();
            Console.WriteLine("Testing Three-Way QuickSort...");
            // Test Three-Way QuickSort
            int[] arr2 = (int[])arr.Clone();
            stopwatch.Restart();
            totalComparisons = 0;
            totalSwaps = 0;
            maxRecursionDepth = 0;
            currentRecursionDepth = 0;
            QuickSortThreeWay(arr2, 0, arr2.Length - 1);
            stopwatch.Stop();
            long threeWayTime = stopwatch.ElapsedMilliseconds;
            long threeWayComparisons = totalComparisons;
            long threeWaySwaps = totalSwaps;
            int threeWayRecursion = maxRecursionDepth;
            Console.WriteLine("Three-Way QuickSort completed!");

            Console.WriteLine();
            Console.WriteLine($"QuickSort Variants Comparison Results:");
            Console.WriteLine($"Array size: {ARRAY_SIZE}");
            Console.WriteLine();
            Console.WriteLine($"Optimized QuickSort:");
            Console.WriteLine($"  Time: {optimizedTime} ms");
            Console.WriteLine($"  Comparisons: {optimizedComparisons:N0}");
            Console.WriteLine($"  Swaps: {optimizedSwaps:N0}");
            Console.WriteLine($"  Max recursion depth: {optimizedRecursion}");
            Console.WriteLine();
            Console.WriteLine($"Three-Way QuickSort:");
            Console.WriteLine($"  Time: {threeWayTime} ms");
            Console.WriteLine($"  Comparisons: {threeWayComparisons:N0}");
            Console.WriteLine($"  Swaps: {threeWaySwaps:N0}");
            Console.WriteLine($"  Max recursion depth: {threeWayRecursion}");
            Console.WriteLine();
            Console.WriteLine($"Performance Comparison:");
            Console.WriteLine($"  Three-Way vs Optimized: {(double)optimizedTime / threeWayTime:F2}x");
            Console.WriteLine($"  Recursion depth difference: {Math.Abs(optimizedRecursion - threeWayRecursion)}");
            Console.WriteLine();
        }

        // OPTIMIZED: Efficient swap operation
        private static void Swap(int[] arr, int i, int j)
        {
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== OPTIMIZED QUICKSORT PERFORMANCE SOLUTION ===");
            Console.WriteLine("This program demonstrates optimized QuickSort implementations:");
            Console.WriteLine("1. Optimized QuickSort with median-of-three pivot selection");
            Console.WriteLine("2. Three-way QuickSort for duplicate elements");
            Console.WriteLine("3. Insertion sort optimization for small arrays");
            Console.WriteLine("4. Tail recursion optimization");
            Console.WriteLine("5. Performance comparison between variants");
            Console.WriteLine();
            Console.WriteLine($"Array size: {ARRAY_SIZE} elements");
            Console.WriteLine("This will demonstrate significant sorting performance improvements!");
            Console.WriteLine();

            // Test different optimized algorithms
            TestQuickSortOptimized(TEST_ITERATIONS);
            TestThreeWayQuickSort(TEST_ITERATIONS);
            TestWithWorstCaseInput();
            TestWithDifferentArraySizes();
            CompareAllQuickSortVariants();

            Console.WriteLine("=== OVERALL OPTIMIZATION ANALYSIS ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Compare with the inefficient version to see performance improvements!");
            Console.WriteLine("3. Observe the dramatic reduction in comparisons and swaps");
            Console.WriteLine("4. Analyze the efficiency of optimized algorithms");
            Console.WriteLine("5. Examine time complexity improvements");
            Console.WriteLine("6. Look for optimization techniques in action");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called functions");
            Console.WriteLine("8. Check for improved time complexity patterns");
            Console.WriteLine();
            Console.WriteLine("Key QuickSort Optimization Techniques Demonstrated:");
            Console.WriteLine("- Median-of-three pivot selection: Reduces worst case scenarios");
            Console.WriteLine("- Insertion sort for small arrays: Optimizes small subproblems");
            Console.WriteLine("- Tail recursion optimization: Reduces stack usage");
            Console.WriteLine("- Three-way partitioning: Efficient handling of duplicates");
            Console.WriteLine("- Time complexity improvement: O(n²) -> O(n log n) average case");
            Console.WriteLine("- Space complexity optimization: Reduced recursion depth");
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