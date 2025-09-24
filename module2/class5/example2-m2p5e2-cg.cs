/*
 * =====================================================================================
 * GARBAGE COLLECTION AND ALLOCATION TRACKING DEMONSTRATION - C#
 * =====================================================================================
 * 
 * Purpose: Demonstrate memory leaks in .NET that prevent garbage collection
 *          for analysis with Visual Studio .NET Object Allocation Tracking
 * 
 * Educational Context:
 * - Detect high consumption with objects that aren't collected by GC
 * - Use .NET Object Allocation Tracking in Visual Studio
 * - Experiment with reference leaks, creating objects "stuck" in heap
 * - Visualize impact of event handlers, static lists, etc.
 * - Reflect on .NET best practices for lifetime management
 * 
 * Key Scenarios:
 * - Objects referenced by static lists are never collected
 * - Simulates common behavior in desktop/server applications
 * - Event handler memory leaks
 * - Circular references that prevent collection
 * 
 * How to use this example:
 * 1. Compile and run this program
 * 2. Open .NET Object Allocation Tracking in Visual Studio
 * 3. Enable allocation tracking and take snapshots
 * 4. Observe heap growth and objects that resist GC
 * 5. Analyze allocation patterns and lifetime issues
 * 
 * =====================================================================================
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

// =====================================================================================
// CONFIGURATION PARAMETERS - MODIFY THESE TO ADJUST DEMONSTRATION BEHAVIOR
// =====================================================================================

namespace MemoryLeakDemonstration
{
    // Configuration constants for demonstration scenarios
    public static class DemoConfig
    {
        // Static Reference Leak Parameters
        public const int STATIC_ITERATIONS = 15;                    // Number of objects to create
        public const int STATIC_PROCESSOR_BASE_SIZE = 10000;        // Base size for data processors
        public const int STATIC_PROCESSOR_SIZE_INCREMENT = 2000;    // Size increment per iteration
        public const int STATIC_ARRAY_BASE_SIZE = 5000;            // Base size for arrays
        public const int STATIC_ARRAY_SIZE_INCREMENT = 1000;       // Size increment per iteration

        // Event Handler Leak Parameters
        public const int EVENT_HANDLER_ITERATIONS = 20;            // Number of event handlers to create
        public const int EVENT_SIMULATION_DURATION_MS = 100;       // Duration to simulate events

        // Continuous Growth Parameters
        public const int CONTINUOUS_DURATION_SECONDS = 90;         // Duration of continuous simulation
        public const int CONTINUOUS_CREATION_INTERVAL = 3;         // Create objects every N iterations
        public const int CONTINUOUS_PROCESSOR_BASE_SIZE = 8000;    // Base size for continuous processors
        public const int CONTINUOUS_PROCESSOR_SIZE_INCREMENT = 500; // Size increment per iteration

        // Timing Parameters
        public const int SNAPSHOT_INTERVAL = 5;                    // Take snapshot every N iterations
        public const int SNAPSHOT_PAUSE_MS = 300;                  // Pause duration for snapshots (ms)
        public const int STATUS_INTERVAL = 10;                     // Show status every N iterations
        public const int LOOP_PAUSE_MS = 150;                      // Pause between iterations (ms)

        // Display Parameters
        public const bool SHOW_DETAILED_MEMORY_INFO = true;        // Show detailed memory calculations
        public const bool SHOW_GC_INFORMATION = true;              // Show garbage collection info
        public const bool FORCE_GC_ATTEMPTS = true;                // Force GC to demonstrate resistance
    }

    // =====================================================================================
    // CLASS THAT SIMULATES A COMPLEX OBJECT WITH LARGE MEMORY USAGE
    // =====================================================================================
    public class DataProcessor
    {
        private List<int> largeDataArray;
        private string description;
        private double[] calculations;
        private byte[] binaryData;
        private int arraySize;
        private DateTime creationTime;
        private static int instanceCount = 0;
        public int InstanceId { get; private set; }

        // Event that can cause memory leaks if not properly unsubscribed
        public event Action<string> DataProcessed;

        // Constructor that allocates large amounts of memory
        public DataProcessor(int size = 10000)
        {
            arraySize = size;
            creationTime = DateTime.Now;
            InstanceId = ++instanceCount;
            
            Console.WriteLine($"  [CONSTRUCTOR] Creating DataProcessor #{InstanceId} with {size} elements...");
            
            // Allocate large integer list
            largeDataArray = new List<int>(size);
            for (int i = 0; i < size; i++)
            {
                largeDataArray.Add(i * i); // Simulate complex data
            }
            
            // Allocate string with substantial content
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                sb.AppendLine($"Data processor #{InstanceId} with {size} elements - Line {i}");
            }
            description = sb.ToString();
            
            // Allocate calculation array
            calculations = new double[size];
            for (int i = 0; i < size; i++)
            {
                calculations[i] = Math.Sqrt(i * Math.PI);
            }
            
            // Allocate binary data to increase memory footprint
            binaryData = new byte[size * 4];
            Random rand = new Random();
            rand.NextBytes(binaryData);
            
            long estimatedMemory = (size * (sizeof(int) + sizeof(double) + 4)) + 
                                  description.Length * sizeof(char);
            Console.WriteLine($"  [CONSTRUCTOR] Estimated memory allocated: ~{estimatedMemory / 1024} KB");
        }

        public void ProcessData()
        {
            Console.WriteLine($"  [PROCESSING] Processing {arraySize} elements in DataProcessor #{InstanceId}...");
            
            // Simulate heavy processing
            for (int i = 0; i < Math.Min(arraySize, 1000); i++) // Limit for performance
            {
                largeDataArray[i] = largeDataArray[i] * 2 + i;
                calculations[i] = Math.Sin(calculations[i]);
            }
            
            // Trigger event (potential memory leak source)
            DataProcessed?.Invoke($"Processed {arraySize} elements");
        }

        public int GetDataSize() => arraySize;
        public DateTime GetCreationTime() => creationTime;
        public string GetDescription() => description;
        
        // Finalizer to track object destruction
        ~DataProcessor()
        {
            Console.WriteLine($"  [FINALIZER] DataProcessor #{InstanceId} is being finalized");
        }
    }

    // =====================================================================================
    // EVENT PUBLISHER CLASS THAT CREATES MEMORY LEAKS THROUGH EVENT HANDLERS
    // =====================================================================================
    public class EventPublisher
    {
        public event Action<string> SomeEvent;
        private List<string> eventLog = new List<string>();
        
        public void TriggerEvent(string message)
        {
            eventLog.Add($"{DateTime.Now:HH:mm:ss.fff}: {message}");
            SomeEvent?.Invoke(message);
        }
        
        public int GetEventLogSize() => eventLog.Count;
    }

    // =====================================================================================
    // STATIC CONTAINER THAT PREVENTS GARBAGE COLLECTION (MEMORY LEAK SOURCE!)
    // =====================================================================================
    public static class StaticReferenceContainer
    {
        // STATIC COLLECTIONS THAT PREVENT GC - MEMORY LEAK SOURCE!
        private static List<DataProcessor> staticProcessors = new List<DataProcessor>();
        private static List<EventPublisher> staticPublishers = new List<EventPublisher>();
        private static Dictionary<int, object> staticObjectRegistry = new Dictionary<int, object>();
        
        public static void AddProcessor(DataProcessor processor)
        {
            staticProcessors.Add(processor);
            staticObjectRegistry[processor.InstanceId] = processor;
            Console.WriteLine($"  [STATIC LEAK] Added DataProcessor #{processor.InstanceId} to static collection. Total: {staticProcessors.Count}");
        }
        
        public static void AddPublisher(EventPublisher publisher)
        {
            staticPublishers.Add(publisher);
            Console.WriteLine($"  [STATIC LEAK] Added EventPublisher to static collection. Total: {staticPublishers.Count}");
        }
        
        public static int GetProcessorCount() => staticProcessors.Count;
        public static int GetPublisherCount() => staticPublishers.Count;
        public static int GetRegistryCount() => staticObjectRegistry.Count;
        
        // Method to demonstrate that objects are still alive
        public static void ShowStaticReferences()
        {
            Console.WriteLine($"\n  [STATIC ANALYSIS] Current static references:");
            Console.WriteLine($"    - Processors: {staticProcessors.Count}");
            Console.WriteLine($"    - Publishers: {staticPublishers.Count}");
            Console.WriteLine($"    - Registry entries: {staticObjectRegistry.Count}");
            
            if (staticProcessors.Count > 0)
            {
                long totalMemoryEstimate = 0;
                foreach (var processor in staticProcessors)
                {
                    totalMemoryEstimate += processor.GetDataSize() * 16; // Rough estimate
                }
                Console.WriteLine($"    - Estimated memory held: ~{totalMemoryEstimate / 1024} KB");
            }
        }
        
        // DELIBERATELY NO CLEAR METHOD - SIMULATES PERMANENT REFERENCES
    }

    // =====================================================================================
    // MEMORY LEAK DEMONSTRATION METHODS
    // =====================================================================================
    public class MemoryLeakDemo
    {
        // =====================================================================================
        // DEMONSTRATION 1: STATIC REFERENCE LEAKS
        // =====================================================================================
        public static void CreateStaticReferenceLeaks()
        {
            Console.WriteLine("\n=== DEMONSTRATION 1: STATIC REFERENCE LEAKS ===");
            Console.WriteLine("Creating objects that will be held by static references...");
            Console.WriteLine("These objects will NEVER be garbage collected!");
            
            for (int i = 0; i < DemoConfig.STATIC_ITERATIONS; i++)
            {
                Console.WriteLine($"\n--- Static Reference Iteration {i + 1} ---");
                
                // Create DataProcessor and add to static collection (MEMORY LEAK!)
                int processorSize = DemoConfig.STATIC_PROCESSOR_BASE_SIZE + (i * DemoConfig.STATIC_PROCESSOR_SIZE_INCREMENT);
                DataProcessor processor = new DataProcessor(processorSize);
                processor.ProcessData();
                
                // ADD TO STATIC COLLECTION - PREVENTS GARBAGE COLLECTION!
                StaticReferenceContainer.AddProcessor(processor);
                
                // Create arrays and add references (simulating additional leaks)
                int[] tempArray = new int[DemoConfig.STATIC_ARRAY_BASE_SIZE + (i * DemoConfig.STATIC_ARRAY_SIZE_INCREMENT)];
                for (int j = 0; j < tempArray.Length; j++)
                {
                    tempArray[j] = j * j;
                }
                
                // Store array reference in static collection
                StaticReferenceContainer.AddPublisher(new EventPublisher());
                
                if (DemoConfig.SHOW_DETAILED_MEMORY_INFO)
                {
                    long estimatedMemoryKB = (processorSize * 16 + tempArray.Length * sizeof(int)) / 1024;
                    Console.WriteLine($"  [LEAK] Object added to static collection (~{estimatedMemoryKB} KB this iteration)");
                }
                
                // Force garbage collection attempt to demonstrate resistance
                if (DemoConfig.FORCE_GC_ATTEMPTS && i % 3 == 0)
                {
                    Console.WriteLine("  [GC] Forcing garbage collection...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    Console.WriteLine($"  [GC] After GC - Memory: {GC.GetTotalMemory(false) / 1024} KB");
                }
                
                // Pause for snapshot opportunities
                if (i % DemoConfig.SNAPSHOT_INTERVAL == 0)
                {
                    Console.WriteLine("  [SNAPSHOT] Take an allocation snapshot now!");
                    Thread.Sleep(DemoConfig.SNAPSHOT_PAUSE_MS);
                }
            }
            
            StaticReferenceContainer.ShowStaticReferences();
        }

        // =====================================================================================
        // DEMONSTRATION 2: EVENT HANDLER LEAKS
        // =====================================================================================
        public static void CreateEventHandlerLeaks()
        {
            Console.WriteLine("\n=== DEMONSTRATION 2: EVENT HANDLER LEAKS ===");
            Console.WriteLine("Creating event handler subscriptions that prevent GC...");
            
            EventPublisher mainPublisher = new EventPublisher();
            List<DataProcessor> eventSubscribers = new List<DataProcessor>();
            
            for (int i = 0; i < DemoConfig.EVENT_HANDLER_ITERATIONS; i++)
            {
                Console.WriteLine($"\n--- Event Handler Iteration {i + 1} ---");
                
                // Create processor and subscribe to events (POTENTIAL MEMORY LEAK!)
                DataProcessor processor = new DataProcessor(5000 + (i * 1000));
                
                // Subscribe to events - creates reference chain
                processor.DataProcessed += (message) => {
                    Console.WriteLine($"    [EVENT] DataProcessor #{processor.InstanceId}: {message}");
                };
                
                mainPublisher.SomeEvent += (message) => {
                    Console.WriteLine($"    [EVENT] Received by DataProcessor #{processor.InstanceId}: {message}");
                    processor.ProcessData(); // This creates a reference!
                };
                
                eventSubscribers.Add(processor); // Keep reference to prevent immediate GC
                
                // Trigger some events
                mainPublisher.TriggerEvent($"Event triggered for iteration {i + 1}");
                processor.ProcessData();
                
                Thread.Sleep(DemoConfig.EVENT_SIMULATION_DURATION_MS);
            }
            
            Console.WriteLine($"\n  [EVENT ANALYSIS] Created {eventSubscribers.Count} event subscribers");
            Console.WriteLine($"  [EVENT ANALYSIS] Publisher has {mainPublisher.GetEventLogSize()} logged events");
            
            // Add publisher to static collection to prevent its GC
            StaticReferenceContainer.AddPublisher(mainPublisher);
            
            // Force GC to demonstrate that objects are still referenced
            if (DemoConfig.FORCE_GC_ATTEMPTS)
            {
                Console.WriteLine("  [GC] Forcing garbage collection on event handlers...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine($"  [GC] After GC - Memory: {GC.GetTotalMemory(false) / 1024} KB");
            }
        }

        // =====================================================================================
        // DEMONSTRATION 3: CONTINUOUS GROWTH SIMULATION
        // =====================================================================================
        public static async Task SimulateContinuousGrowth()
        {
            Console.WriteLine("\n=== DEMONSTRATION 3: CONTINUOUS GROWTH SIMULATION ===");
            Console.WriteLine($"Simulating {DemoConfig.CONTINUOUS_DURATION_SECONDS} seconds of continuous object creation...");
            Console.WriteLine("This simulates a real application with gradual memory leakage...");
            
            var stopwatch = Stopwatch.StartNew();
            var endTime = TimeSpan.FromSeconds(DemoConfig.CONTINUOUS_DURATION_SECONDS);
            
            int iteration = 0;
            
            while (stopwatch.Elapsed < endTime)
            {
                iteration++;
                
                // Create objects periodically (simulates user events, requests, etc.)
                if (iteration % DemoConfig.CONTINUOUS_CREATION_INTERVAL == 0)
                {
                    int processorSize = DemoConfig.CONTINUOUS_PROCESSOR_BASE_SIZE + 
                                      (iteration * DemoConfig.CONTINUOUS_PROCESSOR_SIZE_INCREMENT);
                    
                    DataProcessor newProcessor = new DataProcessor(processorSize);
                    newProcessor.ProcessData();
                    
                    // Add to static collection (MEMORY LEAK!)
                    StaticReferenceContainer.AddProcessor(newProcessor);
                    
                    Console.WriteLine($"  [GROWTH] Iteration {iteration} - Total static objects: {StaticReferenceContainer.GetProcessorCount()}");
                }
                
                // Show status and memory information
                if (iteration % DemoConfig.STATUS_INTERVAL == 0)
                {
                    long currentMemory = GC.GetTotalMemory(false);
                    Console.WriteLine($"  [STATUS] Iteration {iteration} - Current memory: {currentMemory / 1024} KB");
                    Console.WriteLine($"  [STATUS] Static references: {StaticReferenceContainer.GetProcessorCount()} processors");
                    
                    if (DemoConfig.SHOW_GC_INFORMATION)
                    {
                        Console.WriteLine($"  [GC INFO] Generation 0 collections: {GC.CollectionCount(0)}");
                        Console.WriteLine($"  [GC INFO] Generation 1 collections: {GC.CollectionCount(1)}");
                        Console.WriteLine($"  [GC INFO] Generation 2 collections: {GC.CollectionCount(2)}");
                    }
                    
                    Console.WriteLine("  [TIP] Observe allocation tracking and heap growth!");
                }
                
                // Pause for execution control
                await Task.Delay(DemoConfig.LOOP_PAUSE_MS);
            }
            
            stopwatch.Stop();
            
            Console.WriteLine("\n=== CONTINUOUS SIMULATION COMPLETED ===");
            Console.WriteLine($"Total static objects created: {StaticReferenceContainer.GetProcessorCount()}");
            Console.WriteLine($"Final memory usage: {GC.GetTotalMemory(false) / 1024} KB");
            StaticReferenceContainer.ShowStaticReferences();
        }
    }

    // =====================================================================================
    // MAIN PROGRAM
    // =====================================================================================
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("           GARBAGE COLLECTION AND ALLOCATION TRACKING DEMONSTRATION - C#");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("This program demonstrates memory leaks in .NET that prevent garbage collection");
            Console.WriteLine("for analysis with Visual Studio .NET Object Allocation Tracking.");
            Console.WriteLine("\nINSTRUCTIONS FOR PROFESSOR:");
            Console.WriteLine("1. Open .NET Object Allocation Tracking in Visual Studio");
            Console.WriteLine("2. Enable allocation tracking");
            Console.WriteLine("3. Take snapshots before, during, and after execution");
            Console.WriteLine("4. Observe objects that resist garbage collection");
            Console.WriteLine("5. Analyze allocation patterns and lifetime issues");
            Console.WriteLine("=====================================================================================");
            
            Console.WriteLine("\nPress ENTER to start demonstration...");
            Console.ReadLine();
            
            // Show initial memory state
            long initialMemory = GC.GetTotalMemory(true); // Force GC first
            Console.WriteLine($"\n[INITIAL STATE] Starting memory: {initialMemory / 1024} KB");
            
            try
            {
                // Demonstration 1: Static reference leaks
                Console.WriteLine("\n\n[DEMONSTRATION 1] Creating static reference leaks...");
                MemoryLeakDemo.CreateStaticReferenceLeaks();
                
                // Show memory after first demonstration
                long afterStatic = GC.GetTotalMemory(false);
                Console.WriteLine($"\n[MEMORY] After static leaks: {afterStatic / 1024} KB (+{(afterStatic - initialMemory) / 1024} KB)");
                
                // Demonstration 2: Event handler leaks
                Console.WriteLine("\n\n[DEMONSTRATION 2] Creating event handler leaks...");
                MemoryLeakDemo.CreateEventHandlerLeaks();
                
                // Show memory after second demonstration
                long afterEvents = GC.GetTotalMemory(false);
                Console.WriteLine($"\n[MEMORY] After event leaks: {afterEvents / 1024} KB (+{(afterEvents - afterStatic) / 1024} KB)");
                
                // Demonstration 3: Continuous growth
                Console.WriteLine("\n\n[DEMONSTRATION 3] Simulating continuous growth...");
                await MemoryLeakDemo.SimulateContinuousGrowth();
                
                // Final memory analysis
                long finalMemory = GC.GetTotalMemory(false);
                Console.WriteLine($"\n[MEMORY] Final memory: {finalMemory / 1024} KB (+{(finalMemory - initialMemory) / 1024} KB total)");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An error occurred: {ex.Message}");
            }
            
            // Final analysis and recommendations
            Console.WriteLine("\n=====================================================================================");
            Console.WriteLine("                           DEMONSTRATION COMPLETED");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("Now analyze the allocation tracking data to observe:");
            Console.WriteLine("- Objects held by static references that resist GC");
            Console.WriteLine("- Event handler subscription chains");
            Console.WriteLine("- Allocation patterns and object lifetimes");
            Console.WriteLine("- Impact of forced garbage collection attempts");
            Console.WriteLine("\nLESSONS LEARNED:");
            Console.WriteLine("- Importance of proper object lifetime management in .NET");
            Console.WriteLine("- Dangers of static collections and long-lived references");
            Console.WriteLine("- Need to unsubscribe from events properly");
            Console.WriteLine("- Use of weak references for loose coupling");
            Console.WriteLine("- Validation with .NET allocation tracking tools");
            Console.WriteLine("- Understanding GC generations and collection patterns");
            Console.WriteLine("=====================================================================================");
            
            // Attempt final cleanup (will fail due to static references)
            Console.WriteLine("\n[CLEANUP] Attempting final garbage collection...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long cleanupMemory = GC.GetTotalMemory(false);
            Console.WriteLine($"[CLEANUP] Memory after final GC: {cleanupMemory / 1024} KB");
            Console.WriteLine("Notice: Memory is NOT freed due to static references!");
            
            StaticReferenceContainer.ShowStaticReferences();
            
            Console.WriteLine("\nPress ENTER to finish...");
            Console.ReadLine();
        }
    }
}

/*
 * =====================================================================================
 * .NET OBJECT ALLOCATION TRACKING ANALYSIS
 * =====================================================================================
 * 
 * What to observe in .NET Object Allocation Tracking:
 * 
 * 1. HEAP GROWTH RESISTANCE:
 *    - Objects that survive multiple GC generations
 *    - Static references preventing collection
 *    - Event handler chains keeping objects alive
 * 
 * 2. OBJECT TYPES TO MONITOR:
 *    - DataProcessor instances
 *    - Large arrays (int[], double[], byte[])
 *    - String objects and StringBuilder content
 *    - Event delegate chains
 *    - Static collection contents
 * 
 * 3. ALLOCATION PATTERNS:
 *    - Objects moving to Generation 2 and staying there
 *    - Continuous allocation without corresponding deallocation
 *    - Memory pressure despite GC attempts
 * 
 * 4. LIFETIME MANAGEMENT ISSUES:
 *    - Static collections acting as "root references"
 *    - Event subscriptions creating reference chains
 *    - Circular references (if present)
 *    - Long-lived objects holding references to short-lived data
 * 
 * 5. PERFORMANCE IMPACT:
 *    - Increased GC pressure
 *    - Objects promoted to higher generations unnecessarily
 *    - Memory fragmentation
 *    - Potential OutOfMemoryException in long-running scenarios
 * 
 * =====================================================================================
 */
