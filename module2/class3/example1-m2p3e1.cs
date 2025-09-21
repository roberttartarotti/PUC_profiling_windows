/*
 * PROFILING EXAMPLE: Performance Problem Demonstration
 * 
 * This example demonstrates common performance issues and inefficiencies:
 * - Excessive loop iterations and nested loops
 * - Redundant calculations and repeated expensive operations
 * - Inefficient memory allocation patterns
 * - Thread contention and resource bottlenecks
 * - Deep call stacks with expensive operations
 * 
 * NOTE: This code intentionally contains performance problems for educational purposes.
 * Run this with Visual Studio Profiler in Instrumentation mode
 * to observe performance bottlenecks and learn optimization techniques.
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
        // Global configuration - demonstrating performance issues
        private const int DEFAULT_NUM_THREADS = 32;  // High thread count for demonstration
        private const int MATRIX_SIZE = 100;         // Matrix size for profiling
        private const int STRING_COUNT = 1000;       // String count for profiling

        // Global variables - demonstrating resource usage patterns
        private static List<List<double>> globalMatrix = new List<List<double>>();
        private static List<string> globalStrings = new List<string>();
        private static ConcurrentDictionary<string, double> globalMap = new ConcurrentDictionary<string, double>();
        private static ConcurrentDictionary<string, List<int>> globalConcurrentMap = new ConcurrentDictionary<string, List<int>>();
        private static readonly object globalMutex = new object();
        private static int threadCounter = 0;
        private static double sharedResult = 0.0;
        private static List<Task> workerTasks = new List<Task>();
        private static Random random = new Random();

        // Mathematical operations - precomputed values and algorithms
        private static class MathCache
        {
            private const int CACHE_SIZE = 1000;
            private static readonly double[] SinCache = new double[CACHE_SIZE];
            private static readonly double[] CosCache = new double[CACHE_SIZE];
            private static readonly double[] SqrtCache = new double[CACHE_SIZE];

            static MathCache()
            {
                // Precompute common values
                for (int i = 0; i < CACHE_SIZE; i++)
                {
                    double val = i * 0.01;  // Scale for better cache utilization
                    SinCache[i] = Math.Sin(val);
                    CosCache[i] = Math.Cos(val);
                    SqrtCache[i] = Math.Sqrt(val + 1);
                }
            }

            public static double FastSin(double x)
            {
                int index = (int)(x * 100) % CACHE_SIZE;
                return SinCache[index];
            }

            public static double FastCos(double x)
            {
                int index = (int)(x * 100) % CACHE_SIZE;
                return CosCache[index];
            }

            public static double FastSqrt(double x)
            {
                int index = (int)(x * 100) % CACHE_SIZE;
                return SqrtCache[index];
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== PROFILING THREADS ===");
            Console.WriteLine($"NOTE: This program will run {DEFAULT_NUM_THREADS} threads!");
            Console.WriteLine("Each thread focuses on different profiling scenarios:");
            Console.WriteLine("1. CPU-intensive mathematical operations");
            Console.WriteLine("2. Nested loops with calculations");
            Console.WriteLine("3. Mutex operations with contention");
            Console.WriteLine("4. Calculations with redundancy");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate performance characteristics!");
            Console.WriteLine();

            // Initialize global data
            Console.WriteLine("Initializing global data structures...");
            InitializeGlobalData();
            Console.WriteLine("Global data initialized.");
            Console.WriteLine();

            // START PROFILING THREADS IMMEDIATELY
            StartProfilingThreads();

            Console.WriteLine("=== PROFILING ANALYSIS NOTES ===");
            Console.WriteLine("1. Run this with Visual Studio Profiler in INSTRUMENTATION mode");
            Console.WriteLine("2. Compare with the solved version - observe performance differences!");
            Console.WriteLine("3. Look for functions with high call counts and individual time consumption");
            Console.WriteLine("4. Analyze memory allocation patterns - identify inefficiencies!");
            Console.WriteLine("5. Examine cache hit patterns - observe cache misses");
            Console.WriteLine("6. Look for mutex contention and thread synchronization issues");
            Console.WriteLine("7. Focus on 'Hot Paths' - functions consuming most time");
            Console.WriteLine("8. Check call graph for deep call stacks and expensive operations");
            Console.WriteLine();
            Console.WriteLine("Key Profiling Concepts Demonstrated:");
            Console.WriteLine("- Memory allocation overhead with inefficient patterns");
            Console.WriteLine("- Cache misses with unfriendly access patterns");
            Console.WriteLine("- String operations with frequent reallocations");
            Console.WriteLine("- Redundant calculations and repeated expensive operations");
            Console.WriteLine("- Mutex locking with contention and blocking");
            Console.WriteLine("- Deep call stacks with expensive operations");
            Console.WriteLine("- PARALLEL THREADS with resource contention");
            Console.WriteLine("- Multiple threads competing for shared resources");
            Console.WriteLine("- Atomic operations and shared data synchronization overhead");
            Console.WriteLine("- Instrumentation reveals actual costs vs estimates");
            Console.WriteLine("- Small inefficiencies become significant bottlenecks at scale");
            Console.WriteLine("- Multi-threading demonstrating performance characteristics");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void InitializeGlobalData()
        {
            // Initialize global matrix
            globalMatrix.Clear();
            for (int i = 0; i < MATRIX_SIZE; i++)
            {
                var row = new List<double>();
                for (int j = 0; j < MATRIX_SIZE; j++)
                {
                    row.Add(random.NextDouble() * 1000);
                }
                globalMatrix.Add(row);
            }

            // Initialize global strings
            globalStrings.Clear();
            for (int i = 0; i < STRING_COUNT; i++)
            {
                globalStrings.Add($"string_{i}");
            }
        }

        /*
         * SCENARIO 1: Performance Problem Functions
         * These functions demonstrate common performance issues and inefficiencies
         */

        // CPU-intensive function - demonstrates severe performance problems
        private static double CpuIntensiveProblem(double x)
        {
            double result = 0.0;

            // MAJOR PROBLEM: Excessive nested loops with expensive operations
            for (int i = 0; i < 200; i++)  // Increased from 50
            {
                for (int j = 0; j < 100; j++)  // Increased from 10
                {
                    for (int k = 0; k < 50; k++)  // Added third level
                    {
                        // MAJOR PROBLEM: Expensive trigonometric calculations in innermost loop
                        result += Math.Sin(x + i) + Math.Cos(x + j) + Math.Tan(x + k);
                        result += Math.Sqrt(x + i + j + k) + Math.Log(x + i + j + k + 1);
                        result += Math.Pow(x + i, 2.5) + Math.Exp(x * 0.01);

                        // MAJOR PROBLEM: Redundant calculations
                        if (k % 3 == 0)
                        {
                            result += Math.Sin(x + i) + Math.Cos(x + j) + Math.Tan(x + k);  // Recalculating
                            result += Math.Sqrt(x + i + j + k) + Math.Log(x + i + j + k + 1);  // Recalculating
                        }
                    }
                }
            }

            return result;
        }

        // Nested loops function - demonstrates severe nested loop problems
        private static double NestedCpuProblem(double x)
        {
            double result = 0.0;

            // MAJOR PROBLEM: Quadruple nested loops with expensive operations
            for (int i = 0; i < 100; i++)  // Increased from 20
            {
                for (int j = 0; j < 100; j++)  // Increased from 20
                {
                    for (int k = 0; k < 50; k++)  // Increased from 5
                    {
                        for (int l = 0; l < 20; l++)  // Added fourth level
                        {
                            // MAJOR PROBLEM: Expensive operations in innermost loop
                            result += Math.Sin(x + i + j + k + l) + Math.Cos(x + i + j + k + l);
                            result += Math.Tan(x + i + j + k + l) + Math.Log(x + i + j + k + l + 1);
                            result += Math.Sqrt(x + i + j + k + l + 1) + Math.Pow(x + i + j + k + l, 1.5);

                            // MAJOR PROBLEM: Unnecessary string operations in loop
                            string temp = (x + i + j + k + l).ToString();
                            result += temp.Length;
                        }
                    }
                }
            }

            return result;
        }

        // Mathematical operations - demonstrates severe redundant calculations
        private static double MathematicalProblem(int value)
        {
            double x = value;
            double result = 0.0;

            // MAJOR PROBLEM: Massive loops with extreme redundancy
            for (int i = 0; i < 500; i++)  // Increased from 100
            {
                // MAJOR PROBLEM: Recalculating same values repeatedly
                result += Math.Sin(x) + Math.Cos(x) + Math.Tan(x) + Math.Log(x + 1) + Math.Sqrt(x + 1);
                result += Math.Sin(x) + Math.Cos(x) + Math.Tan(x) + Math.Log(x + 1) + Math.Sqrt(x + 1);  // Duplicate
                result += Math.Sin(x) + Math.Cos(x) + Math.Tan(x) + Math.Log(x + 1) + Math.Sqrt(x + 1);  // Duplicate

                // MAJOR PROBLEM: Expensive operations in every iteration
                if (i % 5 == 0)  // Changed from 10 to make it more frequent
                {
                    result += Math.Pow(x + i, 3.7) + Math.Exp(x * 0.1);
                    result += Math.Sin(x + i) + Math.Cos(x + i) + Math.Tan(x + i);
                    result += Math.Sqrt(x + i) + Math.Log(x + i + 1) + Math.Pow(x + i, 2.3);
                }

                // MAJOR PROBLEM: Nested loops with expensive operations
                for (int j = 0; j < 20; j++)  // Increased from 3
                {
                    for (int k = 0; k < 10; k++)  // Added another level
                    {
                        result += Math.Sin(x + j + k) + Math.Cos(x + j + k) + Math.Tan(x + j + k);
                        result += Math.Pow(x + j + k, 1.8) + Math.Exp((x + j + k) * 0.01);
                    }
                }
            }

            return result;
        }

        // Redundant calculations function - demonstrates extreme redundancy
        private static double RedundantCalculationsProblem(double x)
        {
            double result = 0.0;

            // MAJOR PROBLEM: Massive redundancy with expensive calculations
            for (int i = 0; i < 300; i++)  // Increased from 50
            {
                // MAJOR PROBLEM: Same expensive calculations repeated 10 times
                for (int j = 0; j < 10; j++)
                {
                    double redundant = Math.Sin(x) + Math.Cos(x) + Math.Tan(x) + Math.Sqrt(x + 1) + Math.Log(x + 1);
                    result += redundant;
                }

                // MAJOR PROBLEM: More redundant calculations
                if (i % 3 == 0)  // Changed from 5 to make it more frequent
                {
                    result += Math.Pow(x, 3.2) + Math.Exp(x * 0.05);
                    result += Math.Sin(x) + Math.Cos(x) + Math.Tan(x) + Math.Sqrt(x + 1) + Math.Log(x + 1);  // Recalculating
                    result += Math.Pow(x, 3.2) + Math.Exp(x * 0.05);  // Recalculating
                }

                // MAJOR PROBLEM: Nested redundancy
                for (int k = 0; k < 5; k++)
                {
                    result += Math.Sin(x + k) + Math.Cos(x + k) + Math.Tan(x + k);
                    result += Math.Sin(x + k) + Math.Cos(x + k) + Math.Tan(x + k);  // Duplicate
                    result += Math.Sqrt(x + k + 1) + Math.Log(x + k + 1);
                    result += Math.Sqrt(x + k + 1) + Math.Log(x + k + 1);  // Duplicate
                }
            }

            return result;
        }

        // Mutex operations - demonstrates thread synchronization
        private static double MutexOperations(double x)
        {
            // Lock scope
            lock (globalMutex)
            {
                globalMap[x.ToString()] = x * x;
            }

            // Work outside the lock
            return x * x + Math.Sin(x);
        }

        /*
         * SCENARIO 2: String Operations
         */

        // String function - basic string operations
        private static string StringOperations(int value)
        {
            return value.ToString();
        }

        /*
         * SCENARIO 3: Call Patterns
         */

        // Frequent function - demonstrates severe memory allocation problems
        private static double FrequentFunction(double x)
        {
            double result = 0.0;

            // MAJOR PROBLEM: Excessive heap allocations in frequently called function
            for (int i = 0; i < 100; i++)
            {
                // MAJOR PROBLEM: New list allocation on every iteration
                var tempList = new List<double>();
                tempList.Capacity = 1000;  // Reserve but then...

                for (int j = 0; j < 1000; j++)
                {
                    tempList.Add(x + i + j);  // ...add many elements
                }

                // MAJOR PROBLEM: More heap allocations
                var stringList = new List<string>();
                for (int k = 0; k < 100; k++)
                {
                    stringList.Add($"frequent_{x + i + k}");
                }

                // MAJOR PROBLEM: Expensive operations on allocated data
                foreach (var val in tempList)
                {
                    result += Math.Sin(val) + Math.Cos(val) + Math.Sqrt(val + 1);
                }

                foreach (var str in stringList)
                {
                    result += str.Length;
                }
            }

            // MAJOR PROBLEM: String concatenation without StringBuilder
            string finalStr = "";
            for (int i = 0; i < 200; i++)  // Increased from 20
            {
                finalStr += $"_frequent_{x + i}";  // Causes reallocations
            }

            return result + finalStr.Length;
        }

        // Moderate function - demonstrates severe matrix problems
        private static double ModerateFunction(double x)
        {
            double result = 0.0;

            // MAJOR PROBLEM: Large matrix with expensive operations
            var matrix = new double[200, 200];  // Increased from 50x50

            // MAJOR PROBLEM: Expensive initialization
            for (int i = 0; i < 200; i++)
            {
                for (int j = 0; j < 200; j++)
                {
                    // MAJOR PROBLEM: Expensive calculations during initialization
                    matrix[i, j] = Math.Sin(x + i) + Math.Cos(x + j) + Math.Sqrt(x + i + j);
                }
            }

            // MAJOR PROBLEM: Multiple passes over matrix with expensive operations
            for (int pass = 0; pass < 5; pass++)  // Multiple passes
            {
                for (int i = 0; i < 200; i++)
                {
                    for (int j = 0; j < 200; j++)
                    {
                        // MAJOR PROBLEM: Expensive operations in nested loops
                        result += Math.Sin(matrix[i, j]) + Math.Cos(matrix[i, j]) + Math.Tan(matrix[i, j]);
                        result += Math.Sqrt(matrix[i, j] + 1) + Math.Log(matrix[i, j] + 1);
                        result += Math.Pow(matrix[i, j], 2.3) + Math.Exp(matrix[i, j] * 0.01);

                        // MAJOR PROBLEM: Cache-unfriendly access pattern
                        if (j % 2 == 0)
                        {
                            result += matrix[j, i];  // Transpose access
                        }
                    }
                }
            }

            return result;
        }

        // Rare function - demonstrates large data operations
        private static double RareFunction(double x)
        {
            // Large array allocation
            var hugeVector = new double[10000];

            for (int i = 0; i < 10000; i++)
            {
                hugeVector[i] = x + i;
            }

            // Array calculations
            double result = 0.0;
            foreach (double val in hugeVector)
            {
                result += MathCache.FastSin(val) + MathCache.FastCos(val) + Math.Tan(val);
            }

            // String operations
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("rare_").Append((int)x).Append("_");
            stringBuilder.EnsureCapacity(stringBuilder.Capacity + 1000);

            return result + stringBuilder.Length;
        }

        /*
         * Main test functions - demonstrating performance issues
         */

        private static void TestProblemFunctions(int iterations)
        {
            Console.WriteLine($"Testing problem functions with {iterations} iterations...");
            Console.WriteLine("This demonstrates performance issues and inefficiencies.");

            var stopwatch = Stopwatch.StartNew();
            double sum = 0.0;

            for (int i = 0; i < iterations; i++)
            {
                double val = random.NextDouble() * 1000;

                // Call problem functions
                sum += CpuIntensiveProblem(val);
                sum += NestedCpuProblem(val);
                sum += RedundantCalculationsProblem(val);
                sum += MutexOperations(val);

                if (i % 10 == 0)
                {
                    double cpuResult = MathematicalProblem(i);
                    sum += cpuResult;
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"Problem functions result: {sum}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            Console.WriteLine();
        }

        private static void TestCallFrequencyPatterns(int iterations)
        {
            Console.WriteLine($"Testing call frequency patterns with {iterations} iterations...");
            Console.WriteLine("This demonstrates different call frequency scenarios.");

            var stopwatch = Stopwatch.StartNew();
            double sum = 0.0;

            for (int i = 0; i < iterations; i++)
            {
                double val = random.NextDouble() * 1000;

                // Frequent function
                sum += FrequentFunction(val);

                // Moderate function
                if (i % 10 == 0)
                {
                    sum += ModerateFunction(val);
                }

                // Rare function
                if (i % 100 == 0)
                {
                    sum += RareFunction(val);
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
            Console.WriteLine("This demonstrates deep call stack scenarios.");

            var stopwatch = Stopwatch.StartNew();
            double sum = 0.0;

            for (int i = 0; i < iterations; i++)
            {
                double val = random.NextDouble() * 1000;

                // Nested call stack
                sum += NestedLevel1(val, i);
            }

            stopwatch.Stop();

            Console.WriteLine($"Nested calls result: {sum}");
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();
        }

        // Nested function calls - demonstrates deep call stacks
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
         * PARALLEL THREADS - Demonstrates threading scenarios
         */

        // Thread 1: CPU-intensive operations
        private static void CpuIntensiveThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting CPU-intensive operations...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // MAJOR PROBLEM: Severe CPU-intensive operations in threads
                for (int i = 0; i < 500; i++)  // Increased from 100
                {
                    double temp = val + i;

                    // MAJOR PROBLEM: Expensive operations in every iteration
                    threadSum += Math.Sin(temp) + Math.Cos(temp) + Math.Tan(temp) + Math.Sqrt(temp + 1) + Math.Log(temp + 1);
                    threadSum += Math.Pow(temp, 2.5) + Math.Exp(temp * 0.01);

                    // MAJOR PROBLEM: Redundant calculations
                    if (i % 3 == 0)  // Changed from 5 to make it more frequent
                    {
                        threadSum += Math.Sin(temp) + Math.Cos(temp) + Math.Tan(temp);  // Recalculating
                        threadSum += Math.Sqrt(temp + 1) + Math.Log(temp + 1) + Math.Pow(temp, 1.8);  // Recalculating
                    }

                    // MAJOR PROBLEM: Nested loops in thread
                    for (int j = 0; j < 10; j++)
                    {
                        threadSum += Math.Sin(temp + j) + Math.Cos(temp + j);
                    }

                    operationCount++;
                }

                // Stop after 90 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 90)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} math operations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 50000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} math operations. Sum: {threadSum}");
                }
            }
        }

        // Thread 2: Nested loops
        private static void NestedLoopsThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting nested loops...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Nested loops
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        double temp = val + i + j;

                        // Operations in nested loops
                        threadSum += temp * temp;

                        operationCount++;
                    }
                }

                // Stop after 90 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 90)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} nested operations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 50000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} nested operations. Sum: {threadSum}");
                }
            }
        }

        // Thread 3: Mutex operations
        private static void MutexContentionThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting mutex operations...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Mutex operations
                for (int i = 0; i < 10; i++)
                {
                    // Lock scope
                    lock (globalMutex)
                    {
                        globalMap[$"thread_{threadId}_{operationCount}"] = val;
                    }

                    // Work outside the lock
                    threadSum += val * val;

                    operationCount++;
                }

                // Stop after 90 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 90)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} mutex operations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 50000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} mutex operations. Sum: {threadSum}");
                }
            }
        }

        // Thread 4: Redundant calculations
        private static void RedundantCalculationsThread(int threadId)
        {
            Console.WriteLine($"Thread {threadId} starting calculations...");

            var threadRandom = new Random();
            double threadSum = 0.0;
            int operationCount = 0;
            var startTime = DateTime.Now;

            while (true)
            {
                double val = threadRandom.NextDouble() * 1000;

                // Calculations
                for (int i = 0; i < 10; i++)
                {
                    // Calculate once, use multiple times
                    double sinVal = Math.Sin(val + i);
                    double cosVal = Math.Cos(val + i);

                    // Use precomputed values
                    threadSum += sinVal + cosVal;
                    threadSum += sinVal + cosVal;

                    operationCount++;
                }

                // Stop after 90 seconds
                var currentTime = DateTime.Now;
                var duration = currentTime - startTime;
                if (duration.TotalSeconds >= 90)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} calculations. Sum: {threadSum}");
                    break;
                }

                if (operationCount % 50000 == 0)
                {
                    Console.WriteLine($"Thread {threadId} completed {operationCount} calculations. Sum: {threadSum}");
                }
            }
        }

        // Function that spawns profiling threads
        private static void StartProfilingThreads()
        {
            Console.WriteLine("=== PROFILING THREADS STARTING ===");
            Console.WriteLine("Each thread focuses on different profiling scenarios:");
            Console.WriteLine("1. CPU-intensive mathematical operations");
            Console.WriteLine("2. Nested loops with calculations");
            Console.WriteLine("3. Mutex operations with contention");
            Console.WriteLine("4. Calculations with redundancy");
            Console.WriteLine();
            Console.WriteLine("This will demonstrate performance characteristics!");
            Console.WriteLine();

            // Clear worker tasks list
            workerTasks.Clear();

            // Spawn threads for different scenarios
            workerTasks.Add(Task.Run(() => CpuIntensiveThread(1)));
            workerTasks.Add(Task.Run(() => NestedLoopsThread(2)));
            workerTasks.Add(Task.Run(() => MutexContentionThread(3)));
            workerTasks.Add(Task.Run(() => RedundantCalculationsThread(4)));

            // Add more threads
            for (int i = 5; i <= DEFAULT_NUM_THREADS; i++)
            {
                int threadType = (i - 1) % 4 + 1;  // Cycle through thread types
                switch (threadType)
                {
                    case 1:
                        workerTasks.Add(Task.Run(() => CpuIntensiveThread(i)));
                        break;
                    case 2:
                        workerTasks.Add(Task.Run(() => NestedLoopsThread(i)));
                        break;
                    case 3:
                        workerTasks.Add(Task.Run(() => MutexContentionThread(i)));
                        break;
                    case 4:
                        workerTasks.Add(Task.Run(() => RedundantCalculationsThread(i)));
                        break;
                }
            }

            Console.WriteLine($"Started {DEFAULT_NUM_THREADS} threads!");
            Console.WriteLine("Thread breakdown:");
            Console.WriteLine($"- {DEFAULT_NUM_THREADS / 4} CPU-intensive mathematical operation threads");
            Console.WriteLine($"- {DEFAULT_NUM_THREADS / 4} nested loops threads");
            Console.WriteLine($"- {DEFAULT_NUM_THREADS / 4} mutex operation threads");
            Console.WriteLine($"- {DEFAULT_NUM_THREADS / 4} calculation threads");
            Console.WriteLine();
            Console.WriteLine("CPU usage will demonstrate performance characteristics!");
            Console.WriteLine("All threads will automatically stop after 90 seconds!");
            Console.WriteLine();

            // Wait for all threads to complete
            Task.WaitAll(workerTasks.ToArray());

            Console.WriteLine("All threads completed! Profiling session finished.");
        }
    }
}