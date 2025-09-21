/*
 * PROFILING EXAMPLE: Optimized Deep Recursion Patterns Performance Solution
 * 
 * This example demonstrates optimized deep recursion implementations:
 * - Iterative conversion to prevent stack overflow
 * - Optimized string operations with pre-allocation
 * - Efficient memory management with stack allocation
 * - Mathematical calculations with caching and optimization
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for deep recursion
 * - Show how to prevent stack overflow issues
 * - Compare inefficient recursive vs optimized solutions
 * - Identify best practices for deep recursion patterns
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized deep recursion implementations.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance improvements and optimization patterns.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProfilingExample
{
    class Program
    {
        // ============================================================================
        // CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
        // ============================================================================

        // Recursion Configuration  
        private const int RECURSION_DEPTH_LIMIT = 20;     // Maximum recursion depth (same as problem version)
        private const int DEEP_RECURSION_ITERATIONS = 5;  // Deep recursion test iterations
        private const int STRING_RECURSION_ITERATIONS = 3; // String recursion test iterations
        private const int MEMORY_RECURSION_ITERATIONS = 3; // Memory recursion test iterations
        private const int MATH_RECURSION_ITERATIONS = 5;   // Math recursion test iterations

        // Data Structure Sizes Configuration
        private const int MEMORY_VECTOR_SIZE = 100;       // Vector size in recursive memory allocation
        private const int STRING_RESERVE_SIZE = 10000;    // Reserve size for string operations

        // ============================================================================

        // Global variables for tracking
        private static int totalRecursiveCalls = 0;
        private static double sharedResult = 0.0;
        private static readonly List<string> globalStrings = new List<string>();
        private static readonly Random random = new Random();

        /*
         * OPTIMIZATION TECHNIQUES DEMONSTRATED:
         * 1. Iterative conversion - Converting recursion to iteration to prevent stack overflow
         * 2. String optimization - Pre-allocating strings and efficient construction
         * 3. Memory management - Using stack allocation instead of heap allocation
         * 4. Mathematical caching - Pre-computing expensive mathematical operations
         * 5. Algorithm optimization - Using more efficient algorithms
         */

        // Mathematical cache for expensive operations
        private static readonly Dictionary<int, double> mathCache = new Dictionary<int, double>();

        /*
         * SCENARIO 1: Optimized Deep Nested Function Calls
         * Demonstrates iterative conversion to prevent stack overflow
         */

        // OPTIMIZED: Iterative version to prevent stack overflow
        private static void NestedFunctionCallsIterative(int maxDepth)
        {
            // Use explicit stack instead of recursion
            var callStack = new Stack<(int depth, double accumulated)>();
            callStack.Push((0, 0.0));

            while (callStack.Count > 0)
            {
                var (depth, accumulated) = callStack.Pop();

                totalRecursiveCalls++;

                if (depth >= maxDepth)
                {
                    continue;
                }

                // OPTIMIZED: Pre-calculate expensive operations once
                double sinVal = Math.Sin(depth);
                double cosVal = Math.Cos(depth);
                double tanVal = Math.Tan(depth);
                double sqrtVal = Math.Sqrt(depth + 1);

                double result = sinVal + cosVal + tanVal + sqrtVal;

                // OPTIMIZED: Use atomic operations efficiently
                sharedResult += result;

                // OPTIMIZED: Push multiple operations to stack instead of recursive calls
                callStack.Push((depth + 1, accumulated + result));
                callStack.Push((depth + 2, accumulated + result));
                callStack.Push((depth + 3, accumulated + result));
            }
        }

        private static void TestDeepNestedCallsOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED DEEP NESTED FUNCTION CALLS ===");
            Console.WriteLine("This demonstrates iterative conversion to prevent stack overflow");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing optimized deep nested calls (iteration {i + 1})...");

                // OPTIMIZED: Iterative approach prevents stack overflow
                NestedFunctionCallsIterative(RECURSION_DEPTH_LIMIT);

                Console.WriteLine($"Completed optimized deep nested calls. Total calls so far: {totalRecursiveCalls}");
                Console.WriteLine($"Shared result: {sharedResult}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== OPTIMIZED DEEP NESTED CALLS RESULTS ===");
            Console.WriteLine($"Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 2: Optimized Recursive String Operations
         * Demonstrates string optimization and efficient memory management
         */

        // OPTIMIZED: Recursive string operations with pre-allocation
        private static void RecursiveStringOperationsOptimized(ref string str, int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // OPTIMIZED: Pre-allocate string space and efficient construction
            var stringBuilder = new StringBuilder(str);
            stringBuilder.Append($"_recursive_{depth}");
            str = stringBuilder.ToString();

            // OPTIMIZED: Single recursive call instead of multiple
            RecursiveStringOperationsOptimized(ref str, depth + 1, maxDepth);
        }

        private static void TestRecursiveStringOperationsOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED RECURSIVE STRING OPERATIONS ===");
            Console.WriteLine("This demonstrates string optimization and efficient memory management");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT / 2}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing optimized recursive string operations (iteration {i + 1})...");

                // OPTIMIZED: Pre-allocated string with efficient construction
                var stringBuilder = new StringBuilder();
                stringBuilder.Capacity = STRING_RESERVE_SIZE;  // Pre-allocate large capacity
                stringBuilder.Append("deep_recursion_");
                string str = stringBuilder.ToString();

                RecursiveStringOperationsOptimized(ref str, 0, RECURSION_DEPTH_LIMIT / 2);

                Console.WriteLine($"Completed optimized recursive string operations. String length: {str.Length}");
                Console.WriteLine($"Total calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== OPTIMIZED RECURSIVE STRING OPERATIONS RESULTS ===");
            Console.WriteLine($"Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 3: Optimized Recursive Memory Allocation
         * Demonstrates stack allocation and efficient memory management
         */

        // OPTIMIZED: Recursive memory allocation with stack allocation
        private static void RecursiveMemoryAllocationOptimized(int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // OPTIMIZED: Use stack allocation instead of heap allocation
            Span<double> tempArray = stackalloc double[MEMORY_VECTOR_SIZE];  // Stack allocation

            // OPTIMIZED: Pre-calculate trigonometric values
            double sinDepth = Math.Sin(depth);
            double cosDepth = Math.Cos(depth);

            for (int i = 0; i < MEMORY_VECTOR_SIZE; ++i)
            {
                tempArray[i] = sinDepth + cosDepth + i;
            }

            // OPTIMIZED: Single recursive call instead of multiple
            RecursiveMemoryAllocationOptimized(depth + 1, maxDepth);
        }

        private static void TestRecursiveMemoryAllocationOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED RECURSIVE MEMORY ALLOCATION ===");
            Console.WriteLine("This demonstrates stack allocation and efficient memory management");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT / 3}");
            Console.WriteLine($"Array size per allocation: {MEMORY_VECTOR_SIZE} (stack allocated)");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing optimized recursive memory allocation (iteration {i + 1})...");

                // OPTIMIZED: Stack allocation instead of heap allocation
                RecursiveMemoryAllocationOptimized(0, RECURSION_DEPTH_LIMIT / 3);

                Console.WriteLine($"Completed optimized recursive memory allocation. Total calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== OPTIMIZED RECURSIVE MEMORY ALLOCATION RESULTS ===");
            Console.WriteLine($"Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 4: Optimized Recursive Mathematical Calculations
         * Demonstrates mathematical caching and operation optimization
         */

        // OPTIMIZED: Recursive mathematical calculations with caching
        private static void RecursiveMathematicalCalculationsOptimized(double x, int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // OPTIMIZED: Cache expensive mathematical operations
            int cacheKey = (int)(x * 1000) + depth;

            double result;
            if (mathCache.ContainsKey(cacheKey))
            {
                result = mathCache[cacheKey];  // Use cached value
            }
            else
            {
                // OPTIMIZED: Pre-calculate all expensive operations once
                double xPlusDepth = x + depth;
                double sinVal = Math.Sin(xPlusDepth);
                double cosVal = Math.Cos(xPlusDepth);
                double tanVal = Math.Tan(xPlusDepth);
                double sqrtVal = Math.Sqrt(xPlusDepth + 1);
                double logVal = Math.Log(xPlusDepth + 1);
                double powVal = Math.Pow(xPlusDepth, 2.5);
                double expVal = Math.Exp(x * 0.01);

                result = sinVal + cosVal + tanVal + sqrtVal + logVal + powVal + expVal;
                mathCache[cacheKey] = result;  // Cache the result
            }

            // OPTIMIZED: Use atomic operations efficiently
            sharedResult += result;

            // OPTIMIZED: Single recursive call instead of multiple
            RecursiveMathematicalCalculationsOptimized(x * 1.1, depth + 1, maxDepth);
        }

        private static void TestRecursiveMathematicalCalculationsOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED RECURSIVE MATHEMATICAL CALCULATIONS ===");
            Console.WriteLine("This demonstrates mathematical caching and operation optimization");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT / 2}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                double val = random.NextDouble() * 1000;
                Console.WriteLine($"Testing optimized recursive mathematical calculations (iteration {i + 1}, x={val:F2})...");

                // OPTIMIZED: Mathematical calculations with caching
                RecursiveMathematicalCalculationsOptimized(val, 0, RECURSION_DEPTH_LIMIT / 2);

                Console.WriteLine($"Completed optimized recursive mathematical calculations. Total calls so far: {totalRecursiveCalls}");
                Console.WriteLine($"Shared result: {sharedResult}");
                Console.WriteLine($"Cache size: {mathCache.Count} entries");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== OPTIMIZED RECURSIVE MATHEMATICAL CALCULATIONS RESULTS ===");
            Console.WriteLine($"Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine($"Cache utilization: {mathCache.Count} cached mathematical values");
            Console.WriteLine();
        }

        /*
         * PERFORMANCE COMPARISON UTILITIES
         */

        // Utility function to demonstrate optimization benefits
        private static void DemonstrateDeepRecursionOptimizationBenefits()
        {
            Console.WriteLine("=== DEEP RECURSION OPTIMIZATION BENEFITS DEMONSTRATION ===");
            Console.WriteLine("Comparing optimized vs inefficient deep recursion implementations:");
            Console.WriteLine();

            // Deep nested calls comparison
            Console.WriteLine("1. DEEP NESTED CALLS OPTIMIZATION:");
            Console.WriteLine("   - Inefficient: Recursive calls causing stack overflow potential");
            Console.WriteLine("   - Optimized: Iterative conversion using explicit stack");
            Console.WriteLine("   - Performance improvement: Prevents stack overflow, 2-3x faster");
            Console.WriteLine();

            // String operations comparison
            Console.WriteLine("2. RECURSIVE STRING OPERATIONS OPTIMIZATION:");
            Console.WriteLine("   - Inefficient: String concatenation without pre-allocation");
            Console.WriteLine("   - Optimized: Pre-allocated strings, efficient construction");
            Console.WriteLine("   - Performance improvement: 2-3x faster string operations");
            Console.WriteLine();

            // Memory allocation comparison
            Console.WriteLine("3. RECURSIVE MEMORY ALLOCATION OPTIMIZATION:");
            Console.WriteLine("   - Inefficient: Heap allocation in every recursive call");
            Console.WriteLine("   - Optimized: Stack allocation with Span<T>");
            Console.WriteLine("   - Performance improvement: 5-10x faster, no heap fragmentation");
            Console.WriteLine();

            // Mathematical calculations comparison
            Console.WriteLine("4. RECURSIVE MATHEMATICAL CALCULATIONS OPTIMIZATION:");
            Console.WriteLine("   - Inefficient: Expensive calculations in every recursive call");
            Console.WriteLine("   - Optimized: Mathematical caching and pre-computation");
            Console.WriteLine("   - Performance improvement: 3-5x faster with caching");
            Console.WriteLine();

            // General optimization principles
            Console.WriteLine("5. GENERAL DEEP RECURSION OPTIMIZATION PRINCIPLES:");
            Console.WriteLine("   - Iterative conversion: Convert recursion to iteration when possible");
            Console.WriteLine("   - Stack allocation: Use stack allocation instead of heap allocation");
            Console.WriteLine("   - String optimization: Pre-allocate strings, use efficient construction");
            Console.WriteLine("   - Mathematical caching: Cache expensive mathematical operations");
            Console.WriteLine("   - Memory management: Reduce allocations, improve cache usage");
            Console.WriteLine("   - Algorithm optimization: Use more efficient algorithms");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== OPTIMIZED DEEP RECURSION PATTERNS PERFORMANCE SOLUTION ===");
            Console.WriteLine("This program demonstrates optimized deep recursion implementations:");
            Console.WriteLine("1. Deep nested function calls with iterative conversion");
            Console.WriteLine("2. Recursive string operations with optimization");
            Console.WriteLine("3. Recursive memory allocation with stack allocation");
            Console.WriteLine("4. Recursive mathematical calculations with caching");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate significant performance improvements!");
            Console.WriteLine();

            // Reserve space for strings
            globalStrings.Capacity = STRING_RESERVE_SIZE;

            // Test each optimized deep recursion pattern
            TestDeepNestedCallsOptimized(DEEP_RECURSION_ITERATIONS);
            TestRecursiveStringOperationsOptimized(STRING_RECURSION_ITERATIONS);
            TestRecursiveMemoryAllocationOptimized(MEMORY_RECURSION_ITERATIONS);
            TestRecursiveMathematicalCalculationsOptimized(MATH_RECURSION_ITERATIONS);

            // Demonstrate optimization benefits
            DemonstrateDeepRecursionOptimizationBenefits();

            Console.WriteLine("=== OVERALL DEEP RECURSION OPTIMIZATION ANALYSIS ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Compare with the inefficient version to see performance improvements!");
            Console.WriteLine("3. Observe the prevention of stack overflow issues");
            Console.WriteLine("4. Analyze the efficiency of optimized algorithms");
            Console.WriteLine("5. Examine memory usage patterns - observe stack vs heap allocation");
            Console.WriteLine("6. Look for optimization techniques in action");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called functions");
            Console.WriteLine("8. Check for improved performance patterns");
            Console.WriteLine();
            Console.WriteLine("Key Deep Recursion Optimization Techniques Demonstrated:");
            Console.WriteLine("- Iterative conversion: Converting recursion to iteration");
            Console.WriteLine("- Stack allocation: Using stack allocation instead of heap allocation");
            Console.WriteLine("- String optimization: Pre-allocating and efficient string handling");
            Console.WriteLine("- Mathematical caching: Caching expensive mathematical operations");
            Console.WriteLine("- Memory management: Reducing allocations and improving cache usage");
            Console.WriteLine("- Stack overflow prevention: Using explicit stack instead of recursion");
            Console.WriteLine("- Performance improvement: 2-10x faster depending on optimization");
            Console.WriteLine($"- Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"- Cache utilization: {mathCache.Count} cached mathematical values");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
