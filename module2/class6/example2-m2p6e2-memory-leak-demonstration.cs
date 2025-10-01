/*
 * =====================================================================================
 * MEMORY LEAK DEMONSTRATION - C# (CLASS 6)
 * =====================================================================================
 * 
 * Purpose: Demonstrate memory leaks caused by forgetting to call Dispose()
 *          This example shows the effects of improper resource management
 * 
 * Educational Context:
 * - Demonstrate memory leaks in a measurable way
 * - Show how forgetting Dispose() affects system resources
 * - Use Visual Studio Diagnostic Tools to identify memory leaks
 * - Understand resource exhaustion and memory management
 * - Show consequences of improper resource disposal
 * 
 * How to use this example:
 * 1. Compile and run this program
 * 2. Open Diagnostic Tools in Visual Studio (Debug > Windows > Diagnostic Tools)
 * 3. Take snapshots before and after execution
 * 4. Observe heap growth and memory consumption patterns
 * 5. Analyze resource allocation without corresponding disposal
 * 
 * WARNING: This program is designed to consume significant amounts of memory.
 * It will create memory leaks that are not freed during execution.
 * Run in a controlled environment with sufficient RAM available.
 * EXPECTED MEMORY CONSUMPTION: 2+ GB of allocated memory.
 * 
 * =====================================================================================
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace MemoryLeakDemonstration
{
    // =====================================================================================
    // CONFIGURATION PARAMETERS - MODIFY THESE TO ADJUST DEMONSTRATION INTENSITY
    // =====================================================================================

    // Memory Leak Parameters - Increased for demonstration
    public static class Config
    {
        public const int MEGA_ITERATIONS = 50;                         // Number of iterations
        public const int OBJECTS_PER_ITERATION = 100;                   // Objects created per iteration
        public const int MEGA_ARRAY_SIZE = 50000;                      // Size of arrays
        public const int STRING_BUFFER_SIZE = 10000;                    // Size of string buffers
        public const int IMAGE_DIMENSION = 1000;                        // Image dimensions

        // Memory Leak Types - All enabled for comprehensive demonstration
        public const bool CREATE_FILE_STREAMS = true;                   // Create file streams
        public const bool CREATE_MEMORY_STREAMS = true;                // Create memory streams
        public const bool CREATE_IMAGES = true;                        // Create images
        public const bool CREATE_HTTP_CLIENTS = true;                  // Create HTTP clients
        public const bool CREATE_DATABASE_CONNECTIONS = true;          // Create database connections
        public const bool CREATE_STRING_BUILDERS = true;               // Create string builders
        public const bool CREATE_TASKS = true;                         // Create tasks
        public const bool CREATE_TIMERS = true;                        // Create timers

        // Timing and Display
        public const int DISPLAY_INTERVAL = 5;                          // Show progress every N iterations
        public const int MEMORY_CHECK_INTERVAL = 10;                    // Check memory usage every N iterations
        public const int PAUSE_FOR_SNAPSHOT_MS = 500;                  // Pause for memory snapshots
    }

    // =====================================================================================
    // COMPLEX CLASS THAT ALLOCATES SIGNIFICANT RESOURCES
    // =====================================================================================
    public class MegaResourceProcessor
    {
        private FileStream _fileStream;
        private MemoryStream _memoryStream;
        private HttpClient _httpClient;
        private StringBuilder _stringBuilder;
        private Timer _timer;
        private List<byte[]> _dataBuffers;
        private int _processorId;
        _dataBuffers = new List<byte[]>();

        // Constructor that allocates significant amounts of resources
        public MegaResourceProcessor(int id, int size = Config.MEGA_ARRAY_SIZE)
        {
            _processorId = id;
            Console.WriteLine($"  [CONSTRUCTOR] Processor {id} allocating resources...");

            // Create file stream (MEMORY LEAK - not disposed)
            if (Config.CREATE_FILE_STREAMS)
            {
                _fileStream = new FileStream($"temp_file_{id}.dat", FileMode.Create, FileAccess.Write);
                // Fill file with data
                byte[] fileData = new byte[Config.STRING_BUFFER_SIZE];
                for (int i = 0; i < Config.STRING_BUFFER_SIZE; i++)
                {
                    fileData[i] = (byte)(i % 256);
                }
                _fileStream.Write(fileData, 0, fileData.Length);
                _fileStream.Flush();
            }

            // Create memory stream (MEMORY LEAK - not disposed)
            if (Config.CREATE_MEMORY_STREAMS)
            {
                _memoryStream = new MemoryStream(size * sizeof(int));
                byte[] memoryData = new byte[size * sizeof(int)];
                for (int i = 0; i < size; i++)
                {
                    byte[] intBytes = BitConverter.GetBytes(i * i * i);
                    Array.Copy(intBytes, 0, memoryData, i * sizeof(int), sizeof(int));
                }
                _memoryStream.Write(memoryData, 0, memoryData.Length);
            }

            // Create additional memory allocation (MEMORY LEAK - not disposed)
            if (Config.CREATE_IMAGES)
            {
                // Simulate image-like memory allocation
                byte[] imageData = new byte[Config.IMAGE_DIMENSION * Config.IMAGE_DIMENSION * 4]; // 4 bytes per pixel
                for (int i = 0; i < imageData.Length; i++)
                {
                    imageData[i] = (byte)((id + i) % 256);
                }
                // Store in data buffers to simulate image processing
                _dataBuffers.Add(imageData);
            }

            // Create HTTP client (MEMORY LEAK - not disposed)
            if (Config.CREATE_HTTP_CLIENTS)
            {
                _httpClient = new HttpClient();
                _httpClient.Timeout = TimeSpan.FromSeconds(30);
            }

            // Create additional network resources (MEMORY LEAK - not disposed)
            if (Config.CREATE_DATABASE_CONNECTIONS)
            {
                // Simulate database-like resource allocation
                byte[] dbBuffer = new byte[Config.MEGA_ARRAY_SIZE / 2];
                for (int i = 0; i < dbBuffer.Length; i++)
                {
                    dbBuffer[i] = (byte)((id * i) % 256);
                }
                _dataBuffers.Add(dbBuffer);
            }

            // Create string builder (MEMORY LEAK - not disposed)
            if (Config.CREATE_STRING_BUILDERS)
            {
                _stringBuilder = new StringBuilder(Config.STRING_BUFFER_SIZE * 10);
                for (int i = 0; i < Config.STRING_BUFFER_SIZE; i++)
                {
                    _stringBuilder.Append($"String data {i} with lots of content ");
                }
            }

            // Create timer (MEMORY LEAK - not disposed)
            if (Config.CREATE_TIMERS)
            {
                _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            }

            // Create data buffers
            _dataBuffers = new List<byte[]>();
            for (int i = 0; i < 10; i++)
            {
                byte[] buffer = new byte[size / 10];
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = (byte)((i + j) % 256);
                }
                _dataBuffers.Add(buffer);
            }

            // Calculate total memory allocated
            long totalMemory = (size * sizeof(int)) + (Config.STRING_BUFFER_SIZE * 10) + 
                             (Config.IMAGE_DIMENSION * Config.IMAGE_DIMENSION * 4) + // 4 bytes per pixel
                             (size / 10 * 10);

            Console.WriteLine($"  [CONSTRUCTOR] Processor {id} allocated ~{totalMemory / 1024 / 1024} MB");
        }

        // DESTRUCTOR DELIBERATELY OMITTED TO CREATE MEMORY LEAK!
        // public void Dispose()
        // {
        //     _fileStream?.Dispose();        // Memory leak
        //     _memoryStream?.Dispose();      // Memory leak
        //     _bitmap?.Dispose();            // Memory leak
        //     _httpClient?.Dispose();        // Memory leak
        //     _sqlConnection?.Dispose();     // Memory leak
        //     _timer?.Dispose();             // Memory leak
        // }

        public void ProcessResources()
        {
            Console.WriteLine($"  [PROCESSING] Processor {_processorId} processing resources...");

            // Simulate heavy processing on all resources
            if (_memoryStream != null)
            {
                _memoryStream.Position = 0;
                byte[] buffer = new byte[1024];
                _memoryStream.Read(buffer, 0, buffer.Length);
            }

            // Simulate image processing on image data
            if (_dataBuffers.Count > 0)
            {
                byte[] imageBuffer = _dataBuffers[0];
                for (int i = 0; i < Math.Min(1000, imageBuffer.Length); i++)
                {
                    imageBuffer[i] = (byte)(imageBuffer[i] ^ 0xFF); // XOR operation
                }
            }

            if (_stringBuilder != null)
            {
                string result = _stringBuilder.ToString();
                // Simulate string processing
                result = result.ToUpper();
            }

            // Process data buffers
            foreach (var buffer in _dataBuffers)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)(buffer[i] ^ 0xFF); // XOR operation
                }
            }
        }

        public int GetId() => _processorId;

        public long GetMemoryUsage()
        {
            return (Config.MEGA_ARRAY_SIZE * sizeof(int)) + (Config.STRING_BUFFER_SIZE * 10) + 
                   (Config.IMAGE_DIMENSION * Config.IMAGE_DIMENSION * 4) + (Config.MEGA_ARRAY_SIZE / 10 * 10);
        }

        // Timer callback method
        private void TimerCallback(object? state)
        {
            // Simulate timer work
            Thread.Sleep(1);
        }
    }

    // =====================================================================================
    // FUNCTION THAT CREATES MEMORY LEAKS
    // =====================================================================================
    public static class MemoryLeakCreator
    {
        public static void CreateMemoryLeaks()
        {
            Console.WriteLine("\n=== STARTING MEMORY LEAK CREATION ===");
            Console.WriteLine("WARNING: This will consume significant amounts of memory!");
            Console.WriteLine($"Iterations: {Config.MEGA_ITERATIONS}");
            Console.WriteLine($"Objects per iteration: {Config.OBJECTS_PER_ITERATION}");
            Console.WriteLine($"Estimated total objects: {Config.MEGA_ITERATIONS * Config.OBJECTS_PER_ITERATION}");

            List<MegaResourceProcessor> leakedProcessors = new List<MegaResourceProcessor>();
            List<FileStream> leakedFileStreams = new List<FileStream>();
            List<MemoryStream> leakedMemoryStreams = new List<MemoryStream>();
            List<byte[]> leakedImageData = new List<byte[]>();
            List<HttpClient> leakedHttpClients = new List<HttpClient>();
            List<StringBuilder> leakedStringBuilders = new List<StringBuilder>();
            List<Timer> leakedTimers = new List<Timer>();

            long totalLeakedMemory = 0;

            for (int iteration = 0; iteration < Config.MEGA_ITERATIONS; iteration++)
            {
                Console.WriteLine($"\n--- ITERATION {iteration + 1} ---");

                // Create multiple processors (each creates memory leaks)
                for (int obj = 0; obj < Config.OBJECTS_PER_ITERATION; obj++)
                {
                    int processorId = iteration * Config.OBJECTS_PER_ITERATION + obj;
                    MegaResourceProcessor processor = new MegaResourceProcessor(processorId);
                    leakedProcessors.Add(processor);

                    // Simulate usage
                    processor.ProcessResources();

                    // Memory leak: object is not disposed
                }

                // Create additional file streams for demonstration
                if (Config.CREATE_FILE_STREAMS)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        FileStream fileStream = new FileStream($"additional_file_{iteration}_{i}.dat", 
                                                              FileMode.Create, FileAccess.Write);
                        byte[] data = new byte[Config.MEGA_ARRAY_SIZE / 10];
                        fileStream.Write(data, 0, data.Length);
                        fileStream.Flush();
                        leakedFileStreams.Add(fileStream);

                        // Memory leak: file stream is not disposed
                    }
                }

                // Create additional memory streams
                if (Config.CREATE_MEMORY_STREAMS)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        MemoryStream memoryStream = new MemoryStream(Config.MEGA_ARRAY_SIZE);
                        byte[] data = new byte[Config.MEGA_ARRAY_SIZE];
                        memoryStream.Write(data, 0, data.Length);
                        leakedMemoryStreams.Add(memoryStream);

                        // Memory leak: memory stream is not disposed
                    }
                }

                // Create additional image data
                if (Config.CREATE_IMAGES)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        byte[] imageData = new byte[Config.IMAGE_DIMENSION * Config.IMAGE_DIMENSION / 4];
                        for (int j = 0; j < imageData.Length; j++)
                        {
                            imageData[j] = (byte)((iteration + i + j) % 256);
                        }
                        leakedImageData.Add(imageData);

                        // Memory leak: image data is not disposed
                    }
                }

                // Create additional HTTP clients
                if (Config.CREATE_HTTP_CLIENTS)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        HttpClient httpClient = new HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(30);
                        leakedHttpClients.Add(httpClient);

                        // Memory leak: HTTP client is not disposed
                    }
                }

                // Create additional string builders
                if (Config.CREATE_STRING_BUILDERS)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        StringBuilder stringBuilder = new StringBuilder(Config.STRING_BUFFER_SIZE * 5);
                        for (int j = 0; j < Config.STRING_BUFFER_SIZE; j++)
                        {
                            stringBuilder.Append($"Additional string {j} ");
                        }
                        leakedStringBuilders.Add(stringBuilder);

                        // Memory leak: string builder is not disposed (though it doesn't implement IDisposable)
                    }
                }

                // Create additional timers
                if (Config.CREATE_TIMERS)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Timer timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
                        leakedTimers.Add(timer);

                        // Memory leak: timer is not disposed
                    }
                }

                // Calculate current memory usage
                totalLeakedMemory += (Config.OBJECTS_PER_ITERATION * Config.MEGA_ARRAY_SIZE * sizeof(int)) +
                                   (5 * Config.MEGA_ARRAY_SIZE / 10) + (3 * Config.MEGA_ARRAY_SIZE) +
                                   (2 * Config.IMAGE_DIMENSION * Config.IMAGE_DIMENSION / 4) +
                                   (3 * Config.STRING_BUFFER_SIZE * 5);

                // Display progress
                if (iteration % Config.DISPLAY_INTERVAL == 0)
                {
                    Console.WriteLine($"  [PROGRESS] Iteration {iteration + 1} completed!");
                    Console.WriteLine($"  [MEMORY-STATS] Total processors leaked: {leakedProcessors.Count}");
                    Console.WriteLine($"  [MEMORY-STATS] Total file streams leaked: {leakedFileStreams.Count}");
                    Console.WriteLine($"  [MEMORY-STATS] Total memory streams leaked: {leakedMemoryStreams.Count}");
                    Console.WriteLine($"  [MEMORY-STATS] Total image data leaked: {leakedImageData.Count}");
                    Console.WriteLine($"  [MEMORY-STATS] Total HTTP clients leaked: {leakedHttpClients.Count}");
                    Console.WriteLine($"  [MEMORY-STATS] Total timers leaked: {leakedTimers.Count}");
                    Console.WriteLine($"  [MEMORY-STATS] Estimated leaked memory: ~{totalLeakedMemory / 1024 / 1024} MB");
                    Console.WriteLine("  [INFO] Memory consumption increasing with iterations.");
                }

                // Pause for memory snapshots
                if (iteration % Config.MEMORY_CHECK_INTERVAL == 0)
                {
                    Console.WriteLine("  [SNAPSHOT] Take a memory snapshot now. Memory usage is increasing.");
                    Thread.Sleep(Config.PAUSE_FOR_SNAPSHOT_MS);
                }

                // Simulate additional processing
                if (iteration % 3 == 0)
                {
                    Console.WriteLine("  [PROCESSING] Simulating additional processing load...");
                    // Create temporary objects for additional processing
                    List<int> tempStress = new List<int>(10000);
                    for (int i = 0; i < 10000; i++)
                    {
                        tempStress.Add(i * i * i * i * i); // Pentic growth
                    }
                }
            }

            Console.WriteLine("\n=== MEMORY LEAKS CREATED ===");
            Console.WriteLine("FINAL STATISTICS:");
            Console.WriteLine($"- Total processors leaked: {leakedProcessors.Count}");
            Console.WriteLine($"- Total file streams leaked: {leakedFileStreams.Count}");
            Console.WriteLine($"- Total memory streams leaked: {leakedMemoryStreams.Count}");
            Console.WriteLine($"- Total image data leaked: {leakedImageData.Count}");
            Console.WriteLine($"- Total HTTP clients leaked: {leakedHttpClients.Count}");
            Console.WriteLine($"- Total timers leaked: {leakedTimers.Count}");
            Console.WriteLine($"- Estimated leaked memory: ~{totalLeakedMemory / 1024 / 1024} MB");
            Console.WriteLine("- Memory consumption: Significantly increased");
            Console.WriteLine("- System impact: High memory usage");
            Console.WriteLine("- Resource exhaustion: Present");
            Console.WriteLine("- Performance impact: Degraded");
            Console.WriteLine("- Note: Resources will not be disposed during execution");
        }

        // Timer callback method
        private static void TimerCallback(object? state)
        {
            // Simulate timer work
            Thread.Sleep(1);
        }
    }

    // =====================================================================================
    // FUNCTION THAT SIMULATES REAL-WORLD MEMORY EXHAUSTION SCENARIO
    // =====================================================================================
    public static class MemoryExhaustionSimulator
    {
        public static void SimulateMemoryExhaustion()
        {
            Console.WriteLine("\n=== SIMULATING MEMORY EXHAUSTION SCENARIO ===");
            Console.WriteLine("This simulates a real application that gradually exhausts system memory...");

            List<MegaResourceProcessor> exhaustionProcessors = new List<MegaResourceProcessor>();
            int iteration = 0;
            long totalMemory = 0;

            try
            {
                while (iteration < 200) // Limit iterations to prevent actual system crash
                {
                    iteration++;

                    // Create processors in batches
                    for (int batch = 0; batch < 10; batch++)
                    {
                        MegaResourceProcessor processor = new MegaResourceProcessor(iteration * 1000 + batch);
                        exhaustionProcessors.Add(processor);

                        // Simulate usage
                        processor.ProcessResources();

                        // Memory leak: processor is not disposed
                    }

                    totalMemory += (10 * Config.MEGA_ARRAY_SIZE * sizeof(int));

                    if (iteration % 10 == 0)
                    {
                        Console.WriteLine($"  [EXHAUSTION] Iteration {iteration} - Processors: {exhaustionProcessors.Count}");
                        Console.WriteLine($"  [EXHAUSTION] Estimated memory: ~{totalMemory / 1024 / 1024} MB");
                        Console.WriteLine("  [EXHAUSTION] System memory pressure increasing...");

                        // Pause for observation
                        Thread.Sleep(200);
                    }

                    // Simulate system becoming slower
                    if (iteration % 20 == 0)
                    {
                        Console.WriteLine("  [SYSTEM-SLOWDOWN] Memory pressure causing system slowdown...");
                        Thread.Sleep(500);
                    }
                }
            }
            catch (OutOfMemoryException e)
            {
                Console.WriteLine("\n=== MEMORY EXHAUSTION ACHIEVED! ===");
                Console.WriteLine($"Exception caught: {e.Message}");
                Console.WriteLine($"Total processors before exhaustion: {exhaustionProcessors.Count}");
                Console.WriteLine($"Total memory consumed: ~{totalMemory / 1024 / 1024} MB");
                Console.WriteLine("System impact: Resource exhaustion!");
            }

            Console.WriteLine($"\n=== MEMORY EXHAUSTION SIMULATION COMPLETED ===");
            Console.WriteLine($"Total processors created: {exhaustionProcessors.Count}");
            Console.WriteLine($"Total memory consumed: ~{totalMemory / 1024 / 1024} MB");
            Console.WriteLine("System impact: High memory usage");
            Console.WriteLine("Resource exhaustion: Demonstrated");
        }
    }

    // =====================================================================================
    // MAIN PROGRAM
    // =====================================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("                    MEMORY LEAK DEMONSTRATION - C#");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("This program demonstrates memory leaks caused by");
            Console.WriteLine("forgetting to call Dispose() on IDisposable objects.");
            Console.WriteLine("\nEDUCATIONAL OBJECTIVES:");
            Console.WriteLine("- Show consequences of memory leaks");
            Console.WriteLine("- Demonstrate resource exhaustion due to memory leaks");
            Console.WriteLine("- Visualize memory growth patterns");
            Console.WriteLine("- Understand the importance of proper resource disposal");
            Console.WriteLine("\nWARNING: This program will consume significant amounts of memory.");
            Console.WriteLine("It will create memory leaks that will not be disposed during execution.");
            Console.WriteLine("EXPECTED CONSUMPTION: 2+ GB of allocated memory.");
            Console.WriteLine("Run in a controlled environment with sufficient RAM.");
            Console.WriteLine("=====================================================================================");

            Console.WriteLine("\nPress ENTER to start the memory leak demonstration...");
            Console.ReadLine();

            // Demonstration 1: Memory leaks
            Console.WriteLine("\n\n[DEMONSTRATION 1] Creating memory leaks...");
            MemoryLeakCreator.CreateMemoryLeaks();

            // Demonstration 2: Memory exhaustion simulation
            Console.WriteLine("\n\n[DEMONSTRATION 2] Simulating memory exhaustion...");
            MemoryExhaustionSimulator.SimulateMemoryExhaustion();

            Console.WriteLine("\n=====================================================================================");
            Console.WriteLine("                    MEMORY LEAK DEMONSTRATION COMPLETED");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("LESSONS LEARNED:");
            Console.WriteLine("- Memory leaks can cause system resource exhaustion");
            Console.WriteLine("- Forgetting Dispose() leads to memory growth");
            Console.WriteLine("- Resource exhaustion can affect application performance");
            Console.WriteLine("- Proper resource disposal is important for system stability");
            Console.WriteLine("- using statements automatically call Dispose()");
            Console.WriteLine("- Always dispose IDisposable objects");
            Console.WriteLine("- Implement IDisposable pattern for custom classes");
            Console.WriteLine("\nPROFESSOR NOTES:");
            Console.WriteLine("- Use Diagnostic Tools to observe heap growth");
            Console.WriteLine("- Compare snapshots to see memory consumption patterns");
            Console.WriteLine("- Show students the consequences of improper resource management");
            Console.WriteLine("- Demonstrate how memory leaks can affect system performance");
            Console.WriteLine("=====================================================================================");

            Console.WriteLine("\nPress ENTER to finish...");
            Console.ReadLine();
        }
    }
}

/*
 * =====================================================================================
 * DIAGNOSTIC TOOLS ANALYSIS - MEMORY LEAK VERSION
 * =====================================================================================
 * 
 * What to observe in Diagnostic Tools:
 * 
 * 1. HEAP GROWTH:
 *    - Initial snapshot: Small heap
 *    - Intermediate snapshots: Growth over time
 *    - Final snapshot: Large heap (significant memory usage)
 * 
 * 2. OBJECT TYPES LEAKING:
 *    - MegaResourceProcessor objects (each containing multiple resources)
 *    - FileStream objects (file handles not released)
 *    - MemoryStream objects (memory not released)
 *    - Bitmap objects (image memory not released)
 *    - HttpClient objects (network resources not released)
 *    - Timer objects (system resources not released)
 * 
 * 3. ALLOCATION PATTERNS:
 *    - Multiple allocations of same resource types
 *    - Growth over time
 *    - Complete absence of disposal
 *    - Resource accumulation without cleanup
 * 
 * 4. SYSTEM IMPACT:
 *    - Heap fragmentation
 *    - System slowdown
 *    - Possible memory exhaustion
 *    - Performance degradation
 * 
 * 5. EDUCATIONAL VALUE:
 *    - Shows real-world consequences of memory leaks
 *    - Demonstrates why proper resource disposal is critical
 *    - Illustrates how small leaks can become significant
 *    - Proves the importance of using statements and Dispose()
 * 
 * =====================================================================================
 */