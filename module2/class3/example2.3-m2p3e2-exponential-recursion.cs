/*
 * PROFILING EXAMPLE: Exponential Recursion Patterns Performance Investigation
 * 
 * This example demonstrates exponential recursion performance issues:
 * - Binary tree traversal with exponential growth
 * - Matrix path finding with exponential recursion
 * - Multiple recursive calls causing exponential complexity
 * 
 * OBJECTIVES:
 * - Measure exponential recursion impact via instrumentation
 * - Detect exponential growth in recursive calls
 * - Compare inefficient recursive vs optimized solutions
 * - Identify exponential time complexity patterns
 * - Prepare reflection on algorithm design
 * 
 * NOTE: This code intentionally contains severe recursive performance problems.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe exponential recursive call patterns and performance bottlenecks.
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
        private const int RECURSION_DEPTH_LIMIT = 15;     // Maximum recursion depth (15 = safe for exponential growth)
        private const int TREE_SIZE = 1000;               // Binary tree size for traversal
        private const int MATRIX_SIZE = 15;               // Matrix size for path finding (15x15)

        // Test Iterations Configuration
        private const int TREE_TRAVERSAL_ITERATIONS = 3;   // Tree traversal test iterations
        private const int MATRIX_PATH_ITERATIONS = 2;     // Matrix path test iterations

        // ============================================================================

        // Global variables for tracking
        private static int totalRecursiveCalls = 0;
        private static readonly Random random = new Random();

        /*
         * SCENARIO 1: Binary Tree Traversal with Exponential Recursion
         * Demonstrates exponential growth in recursive calls
         */

        // MAJOR PROBLEM: Binary tree traversal with deep recursion
        private static void BinaryTreeTraversalRecursive(int[] tree, int index, int depth)
        {
            totalRecursiveCalls++;

            if (index >= tree.Length || depth > RECURSION_DEPTH_LIMIT)
            {
                return;
            }

            // MAJOR PROBLEM: Expensive operations in every recursive call
            tree[index] = (int)(Math.Sin(depth) + Math.Cos(depth) + Math.Sqrt(depth + 1));

            // MAJOR PROBLEM: Multiple recursive calls causing exponential growth
            BinaryTreeTraversalRecursive(tree, 2 * index + 1, depth + 1);
            BinaryTreeTraversalRecursive(tree, 2 * index + 2, depth + 1);
        }

        private static void TestBinaryTreeTraversal(int iterations)
        {
            Console.WriteLine("=== TESTING BINARY TREE TRAVERSAL RECURSIVE FUNCTION ===");
            Console.WriteLine("This demonstrates exponential growth in recursive calls");
            Console.WriteLine($"Tree size: {TREE_SIZE}");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing binary tree traversal (iteration {i + 1})...");

                // MAJOR PROBLEM: Binary tree traversal with deep recursion
                int[] tree = new int[TREE_SIZE];
                BinaryTreeTraversalRecursive(tree, 0, 0);

                Console.WriteLine($"Completed binary tree traversal. Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== BINARY TREE TRAVERSAL RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 2: Matrix Path Finding with Exponential Recursion
         * Demonstrates exponential complexity in path finding
         */

        // MAJOR PROBLEM: Matrix path finding with exponential recursion
        private static void MatrixPathRecursive(int[,] matrix, int row, int col, int depth)
        {
            totalRecursiveCalls++;

            if (row >= matrix.GetLength(0) || col >= matrix.GetLength(1) || depth > RECURSION_DEPTH_LIMIT)
            {
                return;
            }

            // MAJOR PROBLEM: Expensive calculations in every recursive call
            matrix[row, col] = (int)(Math.Sin(row + col + depth) + Math.Cos(row + col + depth));

            // MAJOR PROBLEM: Multiple recursive calls causing exponential growth
            MatrixPathRecursive(matrix, row + 1, col, depth + 1);
            MatrixPathRecursive(matrix, row, col + 1, depth + 1);
            MatrixPathRecursive(matrix, row + 1, col + 1, depth + 1);
        }

        private static void TestMatrixPathFinding(int iterations)
        {
            Console.WriteLine("=== TESTING MATRIX PATH FINDING RECURSIVE FUNCTION ===");
            Console.WriteLine("This demonstrates exponential complexity in path finding");
            Console.WriteLine($"Matrix size: {MATRIX_SIZE}x{MATRIX_SIZE}");
            Console.WriteLine($"Recursion depth limit: {RECURSION_DEPTH_LIMIT}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing matrix path finding (iteration {i + 1})...");

                // MAJOR PROBLEM: Matrix path finding with exponential recursion
                int[,] matrix = new int[MATRIX_SIZE, MATRIX_SIZE];
                MatrixPathRecursive(matrix, 0, 0, 0);

                Console.WriteLine($"Completed matrix path finding. Total recursive calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== MATRIX PATH FINDING RESULTS ===");
            Console.WriteLine($"Total recursive calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average recursive calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== EXPONENTIAL RECURSION PATTERNS PERFORMANCE INVESTIGATION ===");
            Console.WriteLine("This program demonstrates exponential recursion performance issues:");
            Console.WriteLine("1. Binary tree traversal with exponential growth");
            Console.WriteLine("2. Matrix path finding with exponential recursion");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate severe exponential recursion performance issues!");
            Console.WriteLine();

            // Test each exponential recursion pattern
            TestBinaryTreeTraversal(TREE_TRAVERSAL_ITERATIONS);
            TestMatrixPathFinding(MATRIX_PATH_ITERATIONS);

            Console.WriteLine("=== OVERALL ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Observe the exponential growth in recursive calls!");
            Console.WriteLine("3. Look for functions with extremely high call counts");
            Console.WriteLine("4. Analyze call graph for exponential recursive patterns");
            Console.WriteLine("5. Examine exponential time complexity patterns");
            Console.WriteLine("6. Look for redundant calculations in recursive calls");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called recursive functions");
            Console.WriteLine("8. Check for exponential vs linear time complexity patterns");
            Console.WriteLine();
            Console.WriteLine("Key Exponential Recursion Performance Issues Demonstrated:");
            Console.WriteLine("- Exponential time complexity in recursive algorithms");
            Console.WriteLine("- Multiple recursive calls per function causing exponential growth");
            Console.WriteLine("- Redundant calculations in recursive calls");
            Console.WriteLine("- Expensive operations in every recursive call");
            Console.WriteLine("- No memoization or caching of recursive results");
            Console.WriteLine("- Deep recursion with exponential call growth");
            Console.WriteLine($"- Total recursive calls: {totalRecursiveCalls}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
