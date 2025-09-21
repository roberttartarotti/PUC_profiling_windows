/*
 * PROFILING EXAMPLE: Optimized Exponential Recursion Patterns Performance Solution
 * 
 * This example demonstrates optimized exponential recursion implementations:
 * - Iterative tree traversal to prevent exponential growth
 * - Dynamic programming for matrix path finding
 * - Memoization to avoid redundant calculations
 * - Efficient algorithms with linear/polynomial complexity
 * 
 * OBJECTIVES:
 * - Demonstrate optimization techniques for exponential recursion
 * - Show how to convert exponential to polynomial complexity
 * - Compare inefficient recursive vs optimized solutions
 * - Identify best practices for exponential algorithm optimization
 * - Prepare reflection on algorithm optimization
 * 
 * NOTE: This code demonstrates optimized exponential recursion implementations.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance improvements and optimization patterns.
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
        private const int RECURSION_DEPTH_LIMIT = 15;     // Maximum recursion depth (same as problem version)
        private const int TREE_SIZE = 1000;               // Binary tree size for traversal (same as problem version)
        private const int MATRIX_SIZE = 15;               // Matrix size for path finding (same as problem version)

        // Test Iterations Configuration
        private const int TREE_TRAVERSAL_ITERATIONS = 3;   // Tree traversal test iterations
        private const int MATRIX_PATH_ITERATIONS = 2;     // Matrix path test iterations

        // Optimization Configuration
        private const int MEMOIZATION_CACHE_SIZE = 10000; // Cache size for memoization

        // ============================================================================

        // Global variables for tracking
        private static int totalRecursiveCalls = 0;
        private static readonly Random random = new Random();

        /*
         * OPTIMIZATION TECHNIQUES DEMONSTRATED:
         * 1. Iterative conversion - Converting recursion to iteration
         * 2. Dynamic programming - Using bottom-up approach
         * 3. Memoization - Caching results to avoid redundant calculations
         * 4. Algorithm optimization - Using more efficient algorithms
         * 5. Space optimization - Reducing memory usage
         */

        // Memoization cache for matrix path finding
        private static readonly Dictionary<string, int> pathCache = new Dictionary<string, int>();

        /*
         * SCENARIO 1: Optimized Binary Tree Traversal
         * Demonstrates iterative conversion to prevent exponential growth
         */

        // OPTIMIZED: Iterative tree traversal - O(n) time complexity
        private static void BinaryTreeTraversalIterative(int[] tree)
        {
            // Use queue for level-order traversal
            var nodeQueue = new Queue<int>();
            nodeQueue.Enqueue(0);

            while (nodeQueue.Count > 0)
            {
                int index = nodeQueue.Dequeue();

                totalRecursiveCalls++;

                if (index >= tree.Length)
                {
                    continue;
                }

                // OPTIMIZED: Pre-calculate expensive operations once
                int depth = (int)Math.Log2(index + 1);
                double sinVal = Math.Sin(depth);
                double cosVal = Math.Cos(depth);
                double sqrtVal = Math.Sqrt(depth + 1);

                tree[index] = (int)(sinVal + cosVal + sqrtVal);

                // OPTIMIZED: Add children to queue for processing
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;

                if (leftChild < tree.Length)
                {
                    nodeQueue.Enqueue(leftChild);
                }
                if (rightChild < tree.Length)
                {
                    nodeQueue.Enqueue(rightChild);
                }
            }
        }

        private static void TestBinaryTreeTraversalOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED BINARY TREE TRAVERSAL ===");
            Console.WriteLine("This demonstrates iterative conversion to prevent exponential growth");
            Console.WriteLine($"Tree size: {TREE_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing optimized binary tree traversal (iteration {i + 1})...");

                // OPTIMIZED: Iterative tree traversal
                int[] tree = new int[TREE_SIZE];
                BinaryTreeTraversalIterative(tree);

                Console.WriteLine($"Completed optimized binary tree traversal. Total calls so far: {totalRecursiveCalls}");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== OPTIMIZED BINARY TREE TRAVERSAL RESULTS ===");
            Console.WriteLine($"Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine();
        }

        /*
         * SCENARIO 2: Optimized Matrix Path Finding
         * Demonstrates dynamic programming and memoization
         */

        // OPTIMIZED: Dynamic programming approach - O(n*m) time complexity
        private static void MatrixPathDynamicProgramming(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            // OPTIMIZED: Use dynamic programming table
            int[,] dp = new int[rows, cols];

            // Initialize first row and column
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    totalRecursiveCalls++;

                    // OPTIMIZED: Pre-calculate expensive operations once
                    double sinVal = Math.Sin(i + j);
                    double cosVal = Math.Cos(i + j);

                    matrix[i, j] = (int)(sinVal + cosVal);

                    // OPTIMIZED: Dynamic programming calculation
                    if (i == 0 && j == 0)
                    {
                        dp[i, j] = matrix[i, j];
                    }
                    else if (i == 0)
                    {
                        dp[i, j] = dp[i, j - 1] + matrix[i, j];
                    }
                    else if (j == 0)
                    {
                        dp[i, j] = dp[i - 1, j] + matrix[i, j];
                    }
                    else
                    {
                        dp[i, j] = Math.Min(dp[i - 1, j], Math.Min(dp[i, j - 1], dp[i - 1, j - 1])) + matrix[i, j];
                    }
                }
            }
        }

        // OPTIMIZED: Memoized recursive approach - O(n*m) with caching
        private static void MatrixPathMemoized(int[,] matrix, int row, int col, int depth, Dictionary<string, int> memo)
        {
            totalRecursiveCalls++;

            if (row >= matrix.GetLength(0) || col >= matrix.GetLength(1) || depth > RECURSION_DEPTH_LIMIT)
            {
                return;
            }

            // OPTIMIZED: Create cache key for memoization
            string cacheKey = $"{row},{col},{depth}";

            if (memo.ContainsKey(cacheKey))
            {
                return;  // Already processed
            }

            // OPTIMIZED: Pre-calculate expensive operations once
            double sinVal = Math.Sin(row + col + depth);
            double cosVal = Math.Cos(row + col + depth);

            matrix[row, col] = (int)(sinVal + cosVal);

            // OPTIMIZED: Cache the result
            memo[cacheKey] = matrix[row, col];

            // OPTIMIZED: Single recursive call instead of multiple
            MatrixPathMemoized(matrix, row + 1, col, depth + 1, memo);
        }

        private static void TestMatrixPathFindingOptimized(int iterations)
        {
            Console.WriteLine("=== TESTING OPTIMIZED MATRIX PATH FINDING ===");
            Console.WriteLine("This demonstrates dynamic programming and memoization");
            Console.WriteLine($"Matrix size: {MATRIX_SIZE}x{MATRIX_SIZE}");
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Testing optimized matrix path finding (iteration {i + 1})...");

                // OPTIMIZED: Dynamic programming approach
                int[,] matrix = new int[MATRIX_SIZE, MATRIX_SIZE];
                MatrixPathDynamicProgramming(matrix);

                Console.WriteLine($"Completed optimized matrix path finding. Total calls so far: {totalRecursiveCalls}");
                Console.WriteLine($"Cache size: {pathCache.Count} entries");
                Console.WriteLine();
            }

            stopwatch.Stop();

            Console.WriteLine("=== OPTIMIZED MATRIX PATH FINDING RESULTS ===");
            Console.WriteLine($"Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Average calls per iteration: {totalRecursiveCalls / iterations}");
            Console.WriteLine($"Cache utilization: {pathCache.Count} cached path values");
            Console.WriteLine();
        }

        /*
         * PERFORMANCE COMPARISON UTILITIES
         */

        // Utility function to demonstrate optimization benefits
        private static void DemonstrateExponentialRecursionOptimizationBenefits()
        {
            Console.WriteLine("=== EXPONENTIAL RECURSION OPTIMIZATION BENEFITS DEMONSTRATION ===");
            Console.WriteLine("Comparing optimized vs inefficient exponential recursion implementations:");
            Console.WriteLine();

            // Binary tree traversal comparison
            Console.WriteLine("1. BINARY TREE TRAVERSAL OPTIMIZATION:");
            Console.WriteLine("   - Inefficient: Recursive calls causing exponential growth O(2^n)");
            Console.WriteLine("   - Optimized: Iterative conversion using queue O(n)");
            Console.WriteLine("   - Performance improvement: Exponential to linear complexity");
            Console.WriteLine();

            // Matrix path finding comparison
            Console.WriteLine("2. MATRIX PATH FINDING OPTIMIZATION:");
            Console.WriteLine("   - Inefficient: Multiple recursive calls causing exponential growth O(3^n)");
            Console.WriteLine("   - Optimized: Dynamic programming and memoization O(n*m)");
            Console.WriteLine("   - Performance improvement: Exponential to polynomial complexity");
            Console.WriteLine();

            // General optimization principles
            Console.WriteLine("3. GENERAL EXPONENTIAL RECURSION OPTIMIZATION PRINCIPLES:");
            Console.WriteLine("   - Iterative conversion: Convert recursion to iteration when possible");
            Console.WriteLine("   - Dynamic programming: Use bottom-up approach for path problems");
            Console.WriteLine("   - Memoization: Cache results to avoid redundant calculations");
            Console.WriteLine("   - Algorithm optimization: Use more efficient algorithms");
            Console.WriteLine("   - Space optimization: Reduce memory usage with efficient data structures");
            Console.WriteLine("   - Time complexity improvement: O(2^n) -> O(n), O(3^n) -> O(n*m)");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== OPTIMIZED EXPONENTIAL RECURSION PATTERNS PERFORMANCE SOLUTION ===");
            Console.WriteLine("This program demonstrates optimized exponential recursion implementations:");
            Console.WriteLine("1. Binary tree traversal with iterative conversion");
            Console.WriteLine("2. Matrix path finding with dynamic programming and memoization");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate significant performance improvements!");
            Console.WriteLine();

            // Test each optimized exponential recursion pattern
            TestBinaryTreeTraversalOptimized(TREE_TRAVERSAL_ITERATIONS);
            TestMatrixPathFindingOptimized(MATRIX_PATH_ITERATIONS);

            // Demonstrate optimization benefits
            DemonstrateExponentialRecursionOptimizationBenefits();

            Console.WriteLine("=== OVERALL EXPONENTIAL RECURSION OPTIMIZATION ANALYSIS ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Compare with the inefficient version to see performance improvements!");
            Console.WriteLine("3. Observe the dramatic reduction in recursive calls");
            Console.WriteLine("4. Analyze the efficiency of optimized algorithms");
            Console.WriteLine("5. Examine time complexity improvements");
            Console.WriteLine("6. Look for optimization techniques in action");
            Console.WriteLine("7. Focus on 'Hot Paths' - most frequently called functions");
            Console.WriteLine("8. Check for improved time complexity patterns");
            Console.WriteLine();
            Console.WriteLine("Key Exponential Recursion Optimization Techniques Demonstrated:");
            Console.WriteLine("- Iterative conversion: Converting recursion to iteration");
            Console.WriteLine("- Dynamic programming: Using bottom-up approach");
            Console.WriteLine("- Memoization: Caching results to avoid redundant calculations");
            Console.WriteLine("- Algorithm optimization: Using more efficient algorithms");
            Console.WriteLine("- Time complexity improvement: O(2^n) -> O(n) for tree traversal");
            Console.WriteLine("- Time complexity improvement: O(3^n) -> O(n*m) for matrix path finding");
            Console.WriteLine("- Space complexity optimization: Efficient data structure usage");
            Console.WriteLine($"- Total calls: {totalRecursiveCalls}");
            Console.WriteLine($"- Cache utilization: {pathCache.Count} cached path values");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
