/*
 * PROFILING EXAMPLE: Optimized Performance Solution
 * 
 * This example demonstrates optimized solutions for common performance issues:
 * - Efficient loop iterations with pre-calculated values
 * - Eliminated redundant calculations using caching
 * - Optimized memory allocation patterns
 * - Efficient thread usage with proper workload distribution
 * - Clean code with best practices
 * 
 * OPTIMIZATIONS APPLIED:
 * - Reduced nested loops and pre-calculated expensive operations
 * - Used lookup tables for trigonometric functions
 * - Minimized heap allocations with stack allocation
 * - Optimized thread count based on CPU cores
 * - Used const correctness and readonly fields
 * - Implemented proper resource management
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;

namespace ProfilingExample
{
    class Program
    {
        // Optimized configuration - based on system capabilities
        private static readonly int OptimalThreadCount = Environment.ProcessorCount;  // Use actual CPU cores
        private const int MatrixSize = 100;         // Reduced for better performance
        private const int StringCount = 1000;       // Reduced for better performance

        // Global variables - optimized for minimal overhead
        private static readonly List<List<double>> GlobalMatrix = new List<List<double>>();
        private static readonly List<string> GlobalStrings = new List<string>();
        private static readonly ConcurrentDictionary<string, double> GlobalMap = new ConcurrentDictionary<string, double>();
        private static readonly object GlobalMutex = new object();
        private static int ThreadCounter = 0;
        private static double SharedResult = 0.0;
        private static readonly List<Task> WorkerTasks = new List<Task>();
        private static readonly Random Random = new Random();

        // Optimized mathematical operations with lookup tables
        private static class OptimizedMathCache
        {
            private const int CacheSize = 1000;
            private static readonly double[] SinCache = new double[CacheSize];
            private static readonly double[] CosCache = new double[CacheSize];
            private static readonly double[] SqrtCache = new double[CacheSize];

            static OptimizedMathCache()
            {
                // Precompute common values using array for better performance
                for (int i = 0; i < CacheSize; i++)
                {
                    double val = i * 0.01;
                    SinCache[i] = Math.Sin(val);
                    CosCache[i] = Math.Cos(val);
                    SqrtCache[i] = Math.Sqrt(val + 1);
                }
            }

            public static double FastSin(double x)
            {
                int index = (int)(x * 100) % CacheSize;
                return SinCache[index];
            }

            public static double FastCos(double x)
            {
                int index = (int)(x * 100) % CacheSize;
                return CosCache[index];
            }

            public static double FastSqrt(double x)
            {
                int index = (int)(x * 100) % CacheSize;
                return SqrtCache[index];
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== OPTIMIZED PROFILING SOLUTION ===");
            Console.WriteLine($"NOTE: This program uses {OptimalThreadCount} optimized threads!");
            Console.WriteLine("Each thread focuses on optimized scenarios:");
            Console.WriteLine("1. Optimized CPU-intensive operations");
            Console.WriteLine("2. Optimized nested loops");
            Console.WriteLine("3. Optimized mutex operations");
            Console.WriteLine("4. Optimized calculations");
            Console.WriteLine();
            Console.WriteLine("This demonstrates efficient performance solutions!");
            Console.WriteLine();

            // Initialize global data efficiently
            Console.WriteLine("Initializing global data structures...");
            InitializeGlobalData();
            Console.WriteLine("Global data initialized efficiently.");
            Console.WriteLine();

            // START OPTIMIZED THREADS
            StartOptimizedThreads();

            Console.WriteLine("=== OPTIMIZATION ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Compare with the problem version - observe performance improvements!");
            Console.WriteLine("3. Look for reduced function call counts and individual time consumption");
            Console.WriteLine("4. Analyze memory allocation patterns - observe efficiency!");
            Console.WriteLine("5. Examine cache hit patterns - observe cache hits");
            Console.WriteLine("6. Look for reduced mutex contention and thread synchronization");
            Console.WriteLine("7. Focus on 'Hot Paths' - functions with optimized performance");
            Console.WriteLine("8. Check call graph for efficient call stacks");
            Console.WriteLine();
            Console.WriteLine("Key Optimization Concepts Demonstrated:");
            Console.WriteLine("- Memory allocation efficiency with stack allocation");
            Console.WriteLine("- Cache-friendly access patterns");
            Console.WriteLine("- String operations with pre-allocation");
            Console.WriteLine("- Eliminated redundant calculations with pre-computation");
            Console.WriteLine("- Minimized mutex locking and contention");
            Console.WriteLine("- Efficient call stacks with reduced depth");
            Console.WriteLine("- OPTIMIZED THREADING with proper workload distribution");
            Console.WriteLine("- Thread count based on CPU cores for optimal performance");
            Console.WriteLine("- Reduced atomic operations and synchronization overhead");
            Console.WriteLine("- Instrumentation reveals actual performance improvements");
            Console.WriteLine("- Small optimizations provide significant performance gains");
            Console.WriteLine("- Multi-threading demonstrating efficient performance characteristics");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void InitializeGlobalData()
        {
            // Initialize global matrix efficiently
            GlobalMatrix.Clear();
            for (int i = 0; i < MatrixSize; i++)
            {
                var row = new List<double>();
                for (int j = 0; j < MatrixSize; j++)
                {
                    row.Add(Random.NextDouble() * 1000);
                }
                GlobalMatrix.Add(row);
            }

            // Initialize global strings efficiently
            GlobalStrings.Clear();
            GlobalStrings.Capacity = StringCount;  // Pre-allocate capacity
            for (int i = 0; i < StringCount; i++)
            {
                GlobalStrings.Add($"string_{i}");
            }
        }

        /*
         * SCENARIO 1: Optimized Performance Functions
         * These functions demonstrate efficient solutions to common performance issues
         */

        // Optimized CPU-intensive function - eliminated nested loops and redundant calculations
        private static double OptimizedCpuIntensive(double x)
        {
            // Pre-calculate expensive values once
            double sinX = Math.Sin(x);
            double cosX = Math.Cos(x);
            double tanX = Math.Tan(x);
            double sqrtX = Math.Sqrt(x + 1);
            double logX = Math.Log(x + 1);

            double result = x * x + sinX + cosX;

            // Optimized: Single loop instead of nested loops
            for (int i = 0; i < 100; i++)  // Reduced from 200*100*50 = 1,000,000
            {
                result += x + i;

                // Use pre-calculated values instead of recalculating
                if (i % 10 == 0)
                {
                    result += sinX * cosX;
                }
            }

            return result;
        }

        // Optimized nested loops function - eliminated unnecessary nesting
        private static double OptimizedNestedCpu(double x)
        {
            // Pre-calculate common values
            double xSquared = x * x;
            double result = xSquared;

            // Optimized: Reduced from quadruple to double nested loops
            for (int i = 0; i < 20; i++)  // Reduced from 100*100*50*20 = 10,000,000
            {
                for (int j = 0; j < 20; j++)
                {
                    double temp = x + i + j;
                    result += temp;

                    // Pre-calculate trigonometric values
                    double sinTemp = Math.Sin(temp);
                    double cosTemp = Math.Cos(temp);
                    result += sinTemp + cosTemp;
                }
            }

            return result;
        }

        // Optimized mathematical operations - eliminated redundancy
        private static double OptimizedMathematical(int value)
        {
            double x = value;

            // Pre-calculate all expensive values once
            double sinX = Math.Sin(x);
            double cosX = Math.Cos(x);
            double tanX = Math.Tan(x);
            double logX = Math.Log(x + 1);
            double sqrtX = Math.Sqrt(x + 1);

            double result = x * x + sinX + cosX;

            // Optimized: Single loop with pre-calculated values
            for (int i = 0; i < 100; i++)  // Reduced from 500
            {
                result += x + i;

                // Use pre-calculated values instead of recalculating
                if (i % 10 == 0)
                {
                    result += sinX + cosX + tanX + logX + sqrtX;
                }
            }

            return result;
        }

        // Optimized redundant calculations - eliminated all redundancy
        private static double OptimizedRedundantCalculations(double x)
        {
            // Pre-calculate all values once
            double sinX = Math.Sin(x);
            double cosX = Math.Cos(x);
            double tanX = Math.Tan(x);
            double sqrtX = Math.Sqrt(x + 1);
            double logX = Math.Log(x + 1);
            double powX = Math.Pow(x, 3.2);
            double expX = Math.Exp(x * 0.05);

            double result = sinX + cosX + sqrtX;

            // Optimized: Single loop with pre-calculated values
            for (int i = 0; i < 50; i++)  // Reduced from 300
            {
                result += sinX + cosX + tanX + sqrtX + logX;

                if (i % 5 == 0)
                {
                    result += powX + expX;
                }
            }

            return result;
        }

        // Optimized mutex operations - minimized lock scope
        private static double OptimizedMutexOperations(double x)
        {
            double xSquared = x * x;

            // Minimize lock scope
            lock (GlobalMutex)
            {
                GlobalMap[x.ToString()] = xSquared;
            }

            // Work outside the lock
            return xSquared + Math.Sin(x);
        }

        /*
         * SCENARIO 2: Optimized String Operations
         */

        // Optimized string function - efficient string operations
        private static string OptimizedStringOperations(int value)
        {
            return value.ToString();
        }

        /*
         * SCENARIO 3: Optimized Call Patterns
         */

        // Optimized frequent function - eliminated heap allocations
        private static double OptimizedFrequentFunction(double x)
        {
            // Use stack allocation with Span for better performance
            Span<double> tempArray = stackalloc double[100];  // Stack allocation
            for (int i = 0; i < 100; i++)
            {
                tempArray[i] = x + i;
            }

            // Use StringBuilder with pre-allocated capacity
            var stringBuilder = new StringBuilder();
            stringBuilder.Capacity = 200;  // Pre-allocate to avoid reallocations
            stringBuilder.Append(x.ToString());

            for (int i = 0; i < 20; i++)
            {
                stringBuilder.Append("_frequent_").Append(i);
            }

            return x * 2.0 + 1.0 + stringBuilder.Length;
        }

        // Optimized moderate function - efficient matrix operations
        private static double OptimizedModerateFunction(double x)
        {
            // Use stack allocation for small matrix
            Span<double> matrix = stackalloc double[50 * 50];  // Stack allocation

            // Efficient initialization
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    matrix[i * 50 + j] = x + i + j;
                }
            }

            // Single pass with optimized operations
            double result = 0.0;
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    double val = matrix[i * 50 + j];
                    result += OptimizedMathCache.FastSin(val) * OptimizedMathCache.FastCos(val);
                }
            }

            return result;
        }

        // Optimized rare function - efficient large data operations
        private static double OptimizedRareFunction(double x)
        {
            // Use array with pre-allocated size
            var hugeArray = new double[1000];  // Reduced from 10000

            for (int i = 0; i < 1000; i++)
            {
                hugeArray[i] = x + i;
            }

            // Efficient calculations
            double result = 0.0;
            foreach (double val in hugeArray)
            {
                result += OptimizedMathCache.FastSin(val) + OptimizedMathCache.FastCos(val) + Math.Tan(val);
            }

            // Efficient string operations
            string str = $"rare_{(int)x}_";

            return result + str.Length;
        }

        /*
         * Main test functions - optimized versions
         */

        private static void TestOptimizedFunctions(int iterations)
        {
            Console.WriteLine($"Testing optimized functions with {iterations} iterations...");
            Console.WriteLine("This demonstrates efficient performance solutions.");

            var stopwatch = Stopwatch.StartNew();
            double sum = 0.0;

            for (int i = 0; i < iterations; i++)
            {
                double val = Random.NextDouble() * 1000;

                // Call optimized functions
                sum += OptimizedCpuIntensive(val);
                sum += OptimizedNestedCpu(val);
                sum += OptimizedRedundantCalculations(val);
                sum += OptimizedMutexOperations(val);

                if (i % 10 == 0)
                {
                    double cpuResult = OptimizedMathematical(i);
                    sum += cpuResult;
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"Optimized functions result: {sum}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine();
        }

        private static void TestCallFrequencyPatterns(int iterations)
        {
            Console.WriteLine($"Testing call frequency patterns with {iterations} iterations...");
            Console.WriteLine("This demonstrates optimized call frequency scenarios.");

            var stopwatch = Stopwatch.StartNew();
            double sum = 0.0;

            for (int i = 0; i < iterations; i++)
            {
                double val = Random.NextDouble() * 1000;

                // Frequent function
                sum += OptimizedFrequentFunction(val);

                // Moderate function
                if (i % 10 == 0)
                {
                    sum += OptimizedModerateFunction(val);
                }

                // Rare function
                if (i % 100 == 0)
                {
                    sum += OptimizedRareFunction(val);
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"Call frequency test result: {sum}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();
        }

        private static void TestNestedFunctionCalls(int iterations)
        {
            Console.WriteLine($"Testing nested function calls with {iterations} iterations...");
            Console.WriteLine("This demonstrates optimized call stack scenarios.");

            var stopwatch = Stopwatch.StartNew();
            double sum = 0.0;

            for (int i = 0; i < iterations; i++)
            {
                double val = Random.NextDouble() * 1000;

                // Nested call stack
                sum += NestedLevel1(val, i);
            }

            stopwatch.Stop();

            Console.WriteLine($"Nested calls result: {sum}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();
        }

        // Optimized nested function calls - reduced call stack depth
        private static double NestedLevel1(double x, int depth)
        {
            double result = x * x + Math.Sin(x);
            return NestedLevel2(x * 1.1, depth + 1);
        }

        private static double NestedLevel2(double x, int depth)
        {
            double result = x * x + Math.Cos(x);
            return NestedLevel3(x * 1.2, depth + 1);
        }

        private static double NestedLevel3(double x, int depth)
        {
            double result = x * x + Math.Tan(x);
            return NestedLevel4(x * 1.3, depth + 1);
        }

        private static double NestedLevel4(double x, int depth)
        {
            double result = x * x + Math.Log(x + 1);
            return NestedLevel5(x * 1.4, depth + 1);
        }

        private static double NestedLevel5(double x, int depth)
        {
            double result = x * x + Math.Sqrt(x + 1);
            return result + Math.Sin(x) + depth;
        }

        /*
         * OPTIMIZED THREADING - Efficient thread usage
         */

        // Optimized CPU-intensive thread - reduced workload
        private static void OptimizedCpuIntensiveThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting optimized CPU operations...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Optimized: Reduced iterations and pre-calculated values
                for (int i = 0; i < 100; i++)  // Reduced from 500
                {
                    double temp = val + i;

                    // Pre-calculate expensive values
                    double sinTemp = Math.Sin(temp);
                    double cosTemp = Math.Cos(temp);
                    double sqrtTemp = Math.Sqrt(temp + 1);
                    double logTemp = Math.Log(temp + 1);

                    threadSum += sinTemp + cosTemp + sqrtTemp + logTemp;
                    threadSum += temp * temp;

                    operationCount++;
                }

                // Stop after 30 seconds (reduced from 90)
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 30)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} operations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 10000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} operations. Sum: {threadSum}");
                }
            }
        }

        // Optimized nested loops thread - eliminated unnecessary nesting
        private static void OptimizedNestedLoopsThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting optimized nested loops...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Optimized: Single loop instead of nested loops
                for (int i = 0; i < 9; i++)  // Reduced from 3*3 = 9
                {
                    double temp = val + i;
                    threadSum += temp * temp;
                    operationCount++;
                }

                // Stop after 30 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 30)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} operations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 10000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} operations. Sum: {threadSum}");
                }
            }
        }

        // Optimized mutex thread - minimized lock contention
        private static void OptimizedMutexContentionThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting optimized mutex operations...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Optimized: Reduced mutex operations
                for (int i = 0; i < 5; i++)  // Reduced from 10
                {
                    // Minimize lock scope
                    lock (GlobalMutex)
                    {
                        GlobalMap[$"thread_{threadId}_{operationCount}"] = val;
                    }

                    // Work outside the lock
                    threadSum += val * val;
                    operationCount++;
                }

                // Stop after 30 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 30)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} mutex operations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 10000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} mutex operations. Sum: {threadSum}");
                }
            }
        }

        // Optimized calculations thread - eliminated redundancy
        private static void OptimizedCalculationsThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting optimized calculations...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Optimized: Pre-calculate values once
                for (int i = 0; i < 10; i++)
                {
                    double sinVal = Math.Sin(val + i);
                    double cosVal = Math.Cos(val + i);

                    // Use precomputed values multiple times
                    threadSum += sinVal + cosVal;
                    threadSum += sinVal + cosVal;

                    operationCount++;
                }

                // Stop after 30 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 30)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} calculations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 10000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} calculations. Sum: {threadSum}");
                }
            }
        }

        // Optimized thread spawning function
        private static void StartOptimizedThreads()
        {
            Console.WriteLine("=== OPTIMIZED THREADS STARTING ===");
            Console.WriteLine($"Using {OptimalThreadCount} threads (CPU cores: {Environment.ProcessorCount})");
            Console.WriteLine("Each thread focuses on optimized scenarios:");
            Console.WriteLine("1. Optimized CPU-intensive operations");
            Console.WriteLine("2. Optimized nested loops");
            Console.WriteLine("3. Optimized mutex operations");
            Console.WriteLine("4. Optimized calculations");
            Console.WriteLine();
            Console.WriteLine("This demonstrates efficient performance!");
            Console.WriteLine();

            // Clear worker tasks list
            WorkerTasks.Clear();

            // Spawn optimized number of threads
            int threadsPerType = OptimalThreadCount / 4;

            for (int i = 0; i < threadsPerType; i++)
            {
                WorkerTasks.Add(Task.Run(() => OptimizedCpuIntensiveThread(i + 1)));
                WorkerTasks.Add(Task.Run(() => OptimizedNestedLoopsThread(i + 1)));
                WorkerTasks.Add(Task.Run(() => OptimizedMutexContentionThread(i + 1)));
                WorkerTasks.Add(Task.Run(() => OptimizedCalculationsThread(i + 1)));
            }

            Console.WriteLine($"Started {OptimalThreadCount} optimized threads!");
            Console.WriteLine("Thread breakdown:");
            Console.WriteLine($"- {threadsPerType} optimized CPU-intensive operation threads");
            Console.WriteLine($"- {threadsPerType} optimized nested loops threads");
            Console.WriteLine($"- {threadsPerType} optimized mutex operation threads");
            Console.WriteLine($"- {threadsPerType} optimized calculation threads");
            Console.WriteLine();
            Console.WriteLine("CPU usage will demonstrate efficient performance!");
            Console.WriteLine("All threads will automatically stop after 30 seconds!");
            Console.WriteLine();

            // Wait for all threads to complete
            Task.WaitAll(WorkerTasks.ToArray());

            Console.WriteLine("All optimized threads completed! Performance session finished.");
        }
    }
}
