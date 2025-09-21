/*
 * PROFILING EXAMPLE: Deep Recursion Patterns Performance Investigation
 * 
 * This example demonstrates deep recursion performance issues:
 * - Deep nested function calls with stack overflow potential
 * - Recursive string operations with memory allocation
 * - Recursive memory allocation causing heap fragmentation
 * - Recursive mathematical calculations with expensive operations
 * 
 * OBJECTIVES:
 * - Measure deep recursion impact via instrumentation
 * - Detect deep call stacks and stack overflow potential
 * - Compare inefficient recursive vs optimized solutions
 * - Identify memory allocation patterns in recursion
 * - Prepare reflection on algorithm design
 * 
 * NOTE: This code intentionally contains severe recursive performance problems.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe deep recursive call patterns and performance bottlenecks.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProfilingExample
{
    class Program
    {
        // ============================================================================
        // CLASSROOM CONFIGURATION - EASY TO ADJUST FOR DIFFERENT DEMONSTRATIONS
        // ============================================================================

        // Recursion Configuration  
        private const int RECURSION_DEPTH_LIMIT = 20;     // Maximum recursion depth (20 = safe for stack)
        private const int DEEP_RECURSION_ITERATIONS = 5;  // Deep recursion test iterations
        private const int STRING_RECURSION_ITERATIONS = 3; // String recursion test iterations
        private const int MEMORY_RECURSION_ITERATIONS = 3; // Memory recursion test iterations
        private const int MATH_RECURSION_ITERATIONS = 5;   // Math recursion test iterations

        // Data Structure Sizes Configuration
        private const int MEMORY_VECTOR_SIZE = 100;       // Vector size in recursive memory allocation

        // ============================================================================

        // Global variables for tracking
        private static int totalRecursiveCalls = 0;
        private static double sharedResult = 0.0;
        private static readonly List<string> globalStrings = new List<string>();
        private static readonly Random random = new Random();

        /*
         * SCENARIO 1: Deep Nested Function Calls
         * Demonstrates deep call stacks and stack overflow potential
         */

        // MAJOR PROBLEM: Deep nested function calls
        private static void NestedFunctionCallsRecursive(int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // MAJOR PROBLEM: Expensive operations in every recursive call
            double result = Math.Sin(depth) + Math.Cos(depth) + Math.Tan(depth) + Math.Sqrt(depth + 1);
            sharedResult += result;

            // MAJOR PROBLEM: Multiple recursive calls
            NestedFunctionCallsRecursive(depth + 1, maxDepth);
            NestedFunctionCallsRecursive(depth + 2, maxDepth);
            NestedFunctionCallsRecursive(depth + 3, maxDepth);
        }

        private static void TestDeepNestedCalls(int iterations)
        {
            Console.WriteLine("=== TESTING DEEP NESTED FUNCTION CALLS ===");
            Console.WriteLine("This demonstrates deep call stacks and stack overflow potential");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing deep nested calls (iteration {i + 1})...");

                // MAJOR PROBLEM: Deep recursion with expensive operations
                NestedFunctionCallsRecursive(0, RECURSION_DEPTH_LIMIT);

                Console.WriteLine($"Completed deep nested calls. Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine($"Shared result: {sharedResult}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== DEEP NESTED CALLS RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 2: Recursive String Operations
         * Demonstrates memory allocation and string concatenation issues
         */

        // MAJOR PROBLEM: Recursive string operations with memory allocation
        private static void RecursiveStringOperations(ref string str, int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // MAJOR PROBLEM: String concatenation in every recursive call
            str += $"_recursive_{depth}";

            // MAJOR PROBLEM: Multiple recursive calls
            RecursiveStringOperations(ref str, depth + 1, maxDepth);
            RecursiveStringOperations(ref str, depth + 1, maxDepth);
        }

        private static void TestRecursiveStringOperations(int iterations)
        {
            Console.WriteLine("=== TESTING RECURSIVE STRING OPERATIONS ===");
            Console.WriteLine("This demonstrates memory allocation and string concatenation issues");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT / 2}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing recursive string operations (iteration {i + 1})...");

                // MAJOR PROBLEM: Recursive string operations
                string str = "deep_recursion_";
                RecursiveStringOperations(ref str, 0, RECURSION_DEPTH_LIMIT / 2);

                Console.WriteLine($"Completed recursive string operations. String length: {str.Length}");
                Console.WriteLine($"Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== RECURSIVE STRING OPERATIONS RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 3: Recursive Memory Allocation
         * Demonstrates heap allocation and memory fragmentation
         */

        // MAJOR PROBLEM: Recursive memory allocation
        private static void RecursiveMemoryAllocation(int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // MAJOR PROBLEM: Heap allocation in every recursive call
            double[] tempVector = new double[MEMORY_VECTOR_SIZE];
            for (int i = 0; i < MEMORY_VECTOR_SIZE; ++i)
            {
                tempVector[i] = Math.Sin(depth + i) + Math.Cos(depth + i);
            }

            // MAJOR PROBLEM: Multiple recursive calls
            RecursiveMemoryAllocation(depth + 1, maxDepth);
            RecursiveMemoryAllocation(depth + 1, maxDepth);
        }

        private static void TestRecursiveMemoryAllocation(int iterations)
        {
            Console.WriteLine("=== TESTING RECURSIVE MEMORY ALLOCATION ===");
            Console.WriteLine("This demonstrates heap allocation and memory fragmentation");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT / 3}");
            Console.WriteLine($"Vector size per allocation: {MEMORY_VECTOR_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing recursive memory allocation (iteration {i + 1})...");

                // MAJOR PROBLEM: Recursive memory allocation
                RecursiveMemoryAllocation(0, RECURSION_DEPTH_LIMIT / 3);

                Console.WriteLine($"Completed recursive memory allocation. Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== RECURSIVE MEMORY ALLOCATION RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 4: Recursive Mathematical Calculations
         * Demonstrates expensive operations in recursive calls
         */

        // MAJOR PROBLEM: Recursive mathematical calculations
        private static void RecursiveMathematicalCalculations(double x, int depth, int maxDepth)
        {
            totalRecursiveCalls++;

            if (depth >= maxDepth)
            {
                return;
            }

            // MAJOR PROBLEM: Expensive calculations in every recursive call
            double result = Math.Sin(x + depth) + Math.Cos(x + depth) + Math.Tan(x + depth);
            result += Math.Sqrt(x + depth + 1) + Math.Log(x + depth + 1);
            result += Math.Pow(x + depth, 2.5) + Math.Exp(x * 0.01);

            sharedResult += result;

            // MAJOR PROBLEM: Multiple recursive calls
            RecursiveMathematicalCalculations(x * 1.1, depth + 1, maxDepth);
            RecursiveMathematicalCalculations(x * 1.2, depth + 1, maxDepth);
        }

        private static void TestRecursiveMathematicalCalculations(int iterations)
        {
            Console.WriteLine("=== TESTING RECURSIVE MATHEMATICAL CALCULATIONS ===");
            Console.WriteLine("This demonstrates expensive operations in recursive calls");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT / 2}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                double val = random.NextDouble() * 1000;
                Console.WriteLine($"Testing recursive mathematical calculations (iteration {i + 1}, x={val:F2})...");

                // MAJOR PROBLEM: Recursive mathematical calculations
                RecursiveMathematicalCalculations(val, 0, RECURSION_DEPTH_LIMIT / 2);

                Console.WriteLine($"Completed recursive mathematical calculations. Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine($"Shared result: {sharedResult}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== RECURSIVE MATHEMATICAL CALCULATIONS RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== DEEP RECURSION PATTERNS PERFORMANCE INVESTIGATION ===");
            Console.WriteLine("This program demonstrates deep recursion performance issues:");
            Console.WriteLine("1. Deep nested function calls (stack overflow potential)");
            Console.WriteLine("2. Recursive string operations (memory allocation)");
            Console.WriteLine("3. Recursive memory allocation (heap fragmentation)");
            Console.WriteLine("4. Recursive mathematical calculations (expensive operations)");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate severe deep recursion performance issues!");
            Console.WriteLine();

            // Reserve space for strings
            globalStrings.Capacity = 100000;

            // Test each deep recursion pattern
            TestDeepNestedCalls(DEEP_RECURSION_ITERATIONS);
            TestRecursiveStringOperations(STRING_RECURSION_ITERATIONS);
            TestRecursiveMemoryAllocation(MEMORY_RECURSION_ITERATIONS);
            TestRecursiveMathematicalCalculations(MATH_RECURSION_ITERATIONS);

            Console.WriteLine("=== OVERALL ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Observe the deep recursion patterns!");
            Console.WriteLine("3. Look for functions with deep call stacks");
            Console.WriteLine("4. Analyze call graph for deep recursive patterns");
            Console.WriteLine("5. Examine stack usage and potential overflow");
            Console.WriteLine("6. Look for memory allocation patterns in recursive calls");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called recursive functions");
            Console.WriteLine("8. Check for expensive operations in recursive calls");
            Console.WriteLine();
            Console.WriteLine("Key Deep Recursion Performance Issues Demonstrated:");
            Console.WriteLine("- Deep recursion causing stack overflow potential");
            Console.WriteLine("- Memory allocation in every recursive call");
            Console.WriteLine("- String operations causing memory fragmentation");
            Console.WriteLine("- Multiple recursive calls per function");
            Console.WriteLine("- Expensive operations in recursive calls");
            Console.WriteLine("- No optimization of recursive patterns");
            Console.WriteLine($"- Total recursive calls: {totalRecursiveCalls}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
