/*
 * PROFILING EXAMPLE: Classic Recursive Functions Performance Investigation
 * 
 * This example demonstrates severe recursive function performance issues:
 * - Fibonacci with exponential time complexity O(2^n)
 * - Tower of Hanoi with exponential complexity O(2^n)
 * - Permutation generation with factorial complexity O(n!)
 * 
 * OBJECTIVES:
 * - Measure recursive function impact via instrumentation
 * - Detect exponential growth in recursive calls
 * - Compare inefficient recursive vs optimized solutions
 * - Identify time differences and variance
 * - Prepare reflection on algorithm design
 * 
 * NOTE: This code intentionally contains severe recursive performance problems.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe recursive call patterns and performance bottlenecks.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProfilingExample
{
    class Program
    {
        // ============================================================================
        // CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
        // ============================================================================

        // Recursion Configuration  
        private const int FIBONACCI_LIMIT = 35;           // Fibonacci input limit (35 = shows exponential growth)
        private const int TOWER_OF_HANOI_DISKS = 15;      // Tower of Hanoi disks (15 = exponential complexity)
        private const int PERMUTATION_SIZE = 8;           // Permutation array size (8 = factorial complexity)

        // Test Iterations Configuration
        private const int FIBONACCI_ITERATIONS = 10;      // Fibonacci test iterations
        private const int TOWER_ITERATIONS = 5;           // Tower of Hanoi test iterations
        private const int PERMUTATION_ITERATIONS = 3;      // Permutation test iterations

        // ============================================================================

        // Global variables for tracking
        private static int totalRecursiveCalls = 0;
        private static readonly List<string> globalStrings = new List<string>();
        private static readonly Random random = new Random();

        /*
         * SCENARIO 1: Fibonacci Recursive Function
         * Demonstrates exponential time complexity O(2^n)
         */

        // MAJOR PROBLEM: Classic Fibonacci with exponential time complexity O(2^n)
        private static long FibonacciRecursive(int n)
        {
            totalRecursiveCalls++;

            // MAJOR PROBLEM: No base case optimization, redundant calculations
            if (n <= 1)
            {
                return n;
            }

            // MAJOR PROBLEM: Double recursive calls causing exponential growth
            return FibonacciRecursive(n - 1) + FibonacciRecursive(n - 2);
        }

        private static void TestFibonacciRecursive(int iterations)
        {
            Console.WriteLine("=== TESTING FIBONACCI RECURSIVE FUNCTION ===");
            Console.WriteLine("This demonstrates exponential time complexity O(2^n)");
            Console.WriteLine($"Fibonacci limit: {FIBONACCI_LIMIT}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            long sum = 0;

            for (int i = 0; i < iterations; ++i)
            {
                int n = random.Next(1, FIBONACCI_LIMIT + 1);

                Console.WriteLine($"Computing Fibonacci({n})...");

                // MAJOR PROBLEM: Call expensive recursive function
                long result = FibonacciRecursive(n);
                sum += result;

                Console.WriteLine($"Fibonacci({n}) = {result}");
                Console.WriteLine($"Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== FIBONACCI RESULTS ===");
            Console.WriteLine($"Total sum: {sum}");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 2: Tower of Hanoi Recursive Function
         * Demonstrates exponential complexity O(2^n)
         */

        // MAJOR PROBLEM: Tower of Hanoi with exponential complexity O(2^n)
        private static void TowerOfHanoiRecursive(int n, char from, char to, char aux)
        {
            totalRecursiveCalls++;

            if (n == 1)
            {
                // MAJOR PROBLEM: Expensive string operations in every recursive call
                string move = $"Move disk 1 from {from} to {to}";
                globalStrings.Add(move);
                return;
            }

            // MAJOR PROBLEM: Triple recursive calls for each disk
            TowerOfHanoiRecursive(n - 1, from, aux, to);
            TowerOfHanoiRecursive(n - 1, aux, to, from);
        }

        private static void TestTowerOfHanoiRecursive(int iterations)
        {
            Console.WriteLine("=== TESTING TOWER OF HANOI RECURSIVE FUNCTION ===");
            Console.WriteLine("This demonstrates exponential complexity O(2^n)");
            Console.WriteLine($"Number of disks: {TOWER_OF_HANOI_DISKS}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Solving Tower of Hanoi with {TOWER_OF_HANOI_DISKS} disks...");

                // Clear previous moves
                globalStrings.Clear();

                // MAJOR PROBLEM: Tower of Hanoi with high disk count
                TowerOfHanoiRecursive(TOWER_OF_HANOI_DISKS, 'A', 'C', 'B');

                Console.WriteLine($"Completed Tower of Hanoi. Total moves: {globalStrings.Count}");
                Console.WriteLine($"Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== TOWER OF HANOI RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 3: Permutation Generation Recursive Function
         * Demonstrates factorial complexity O(n!)
         */

        // MAJOR PROBLEM: Permutation generation with factorial complexity O(n!)
        private static void GeneratePermutationsRecursive(int[] arr, int start, int end)
        {
            totalRecursiveCalls++;

            if (start == end)
            {
                // MAJOR PROBLEM: Expensive operations for each permutation
                string permutation = string.Join(" ", arr);
                globalStrings.Add(permutation);
                return;
            }

            // MAJOR PROBLEM: Recursive calls for each position
            for (int i = start; i <= end; ++i)
            {
                Swap(arr, start, i);
                GeneratePermutationsRecursive(arr, start + 1, end);
                Swap(arr, start, i);  // Backtrack
            }
        }

        private static void Swap(int[] arr, int i, int j)
        {
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        private static void TestPermutationRecursive(int iterations)
        {
            Console.WriteLine("=== TESTING PERMUTATION GENERATION RECURSIVE FUNCTION ===");
            Console.WriteLine("This demonstrates factorial complexity O(n!)");
            Console.WriteLine($"Array size: {PERMUTATION_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Generating permutations for array of size {PERMUTATION_SIZE}...");

                // Clear previous permutations
                globalStrings.Clear();

                // Create array
                int[] arr = new int[PERMUTATION_SIZE];
                for (int j = 0; j < PERMUTATION_SIZE; ++j)
                {
                    arr[j] = j + 1;
                }

                // MAJOR PROBLEM: Permutation generation with factorial complexity
                GeneratePermutationsRecursive(arr, 0, PERMUTATION_SIZE - 1);

                Console.WriteLine($"Completed permutation generation. Total permutations: {globalStrings.Count}");
                Console.WriteLine($"Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== PERMUTATION RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== CLASSIC RECURSIVE FUNCTIONS PERFORMANCE INVESTIGATION ===");
            Console.WriteLine("This program demonstrates severe recursive function performance issues:");
            Console.WriteLine("1. Fibonacci recursive function (exponential complexity O(2^n))");
            Console.WriteLine("2. Tower of Hanoi recursive function (exponential complexity O(2^n))");
            Console.WriteLine("3. Permutation generation recursive function (factorial complexity O(n!))");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate severe recursive performance issues!");
            Console.WriteLine();

            // Reserve space for strings
            globalStrings.Capacity = 100000;

            // Test each recursive function type
            TestFibonacciRecursive(FIBONACCI_ITERATIONS);
            TestTowerOfHanoiRecursive(TOWER_ITERATIONS);
            TestPermutationRecursive(PERMUTATION_ITERATIONS);

            Console.WriteLine("=== OVERALL ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Observe the exponential growth in recursive calls!");
            Console.WriteLine("3. Look for functions with extremely high call counts");
            Console.WriteLine("4. Analyze call graph for recursive patterns");
            Console.WriteLine("5. Examine time complexity differences");
            Console.WriteLine("6. Look for redundant calculations in recursive calls");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called recursive functions");
            Console.WriteLine("8. Check for exponential vs factorial time complexity patterns");
            Console.WriteLine();
            Console.WriteLine("Key Recursive Performance Issues Demonstrated:");
            Console.WriteLine("- Exponential time complexity O(2^n) in Fibonacci and Tower of Hanoi");
            Console.WriteLine("- Factorial complexity O(n!) in permutation generation");
            Console.WriteLine("- Redundant calculations in recursive calls");
            Console.WriteLine("- String operations causing memory allocation");
            Console.WriteLine("- Multiple recursive calls per function");
            Console.WriteLine("- No memoization or caching of recursive results");
            Console.WriteLine("- Expensive operations in base cases and recursive cases");
            Console.WriteLine($"- Total recursive calls: {totalRecursiveCalls}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
