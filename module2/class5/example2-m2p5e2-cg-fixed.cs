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
using System.Linq;

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
        
        // Method to clear all event subscriptions (for proper cleanup)
        public void ClearEventSubscriptions()
        {
            DataProcessed = null;
        }

        // Constructor that allocates large amounts of memory - REFACTORED VERSION
        public DataProcessor(int size = 10000)
        {
            // Validate input
            if (size <= 0)
                throw new ArgumentException("Size must be positive", nameof(size));
            if (size > 1000000)
                throw new ArgumentException("Size too large, maximum 1,000,000", nameof(size));
            
            arraySize = size;
            creationTime = DateTime.Now;
            InstanceId = ++instanceCount;
            
            Console.WriteLine($"  [CONSTRUCTOR] Creating DataProcessor #{InstanceId} with {size} elements...");
            
            try
            {
                // Initialize collections with proper capacity
                InitializeDataCollections(size);
                
                // Calculate and log memory usage
                long estimatedMemory = CalculateEstimatedMemory(size);
                Console.WriteLine($"  [CONSTRUCTOR] Estimated memory allocated: ~{estimatedMemory / 1024} KB");
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine($"  [CONSTRUCTOR ERROR] Failed to allocate memory for size {size}: {ex.Message}");
                throw;
            }
        }
        
        private void InitializeDataCollections(int size)
        {
            // Optimized initialization with pre-allocated arrays
            InitializeIntegerArray(size);
            description = GenerateDescription(size);
            InitializeCalculations(size);
            InitializeBinaryData(size);
        }
        
        private void InitializeIntegerArray(int size)
        {
            // Use array instead of List for better performance
            var tempArray = new int[size];
            for (int i = 0; i < size; i++)
            {
                tempArray[i] = i * i; // Simulate complex data
            }
            largeDataArray = new List<int>(tempArray);
        }
        
        private void InitializeCalculations(int size)
        {
            // Pre-calculate PI for performance
            const double PI = Math.PI;
            calculations = new double[size];
            
            // Use parallel processing for large arrays
            if (size > 10000)
            {
                Parallel.For(0, size, i => {
                    calculations[i] = Math.Sqrt(i * PI);
                });
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    calculations[i] = Math.Sqrt(i * PI);
                }
            }
        }
        
        private void InitializeBinaryData(int size)
        {
            binaryData = new byte[size * 4];
            var rand = new Random();
            rand.NextBytes(binaryData);
        }
        
        private string GenerateDescription(int size)
        {
            var sb = new StringBuilder(size * 50); // Pre-allocate capacity
            for (int i = 0; i < 100; i++)
            {
                sb.AppendLine($"Data processor #{InstanceId} with {size} elements - Line {i}");
            }
            return sb.ToString();
        }
        
        private long CalculateEstimatedMemory(int size)
        {
            return (size * (sizeof(int) + sizeof(double) + 4)) + 
                   (description?.Length ?? 0) * sizeof(char);
        }

        public void ProcessData()
        {
            Console.WriteLine($"  [PROCESSING] Processing {arraySize} elements in DataProcessor #{InstanceId}...");
            
            try
            {
                // Optimized processing with bounds checking
                int processCount = Math.Min(arraySize, 1000);
                
                // Use parallel processing for better performance
                if (processCount > 100)
                {
                    Parallel.For(0, processCount, i => {
                        if (i < largeDataArray.Count)
                            largeDataArray[i] = largeDataArray[i] * 2 + i;
                        if (i < calculations.Length)
                            calculations[i] = Math.Sin(calculations[i]);
                    });
                }
                else
                {
                    // Sequential processing for small datasets
                    for (int i = 0; i < processCount; i++)
                    {
                        if (i < largeDataArray.Count)
                            largeDataArray[i] = largeDataArray[i] * 2 + i;
                        if (i < calculations.Length)
                            calculations[i] = Math.Sin(calculations[i]);
                    }
                }
                
                // Trigger event safely
                DataProcessed?.Invoke($"Processed {processCount} elements");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [PROCESSING ERROR] Failed to process data: {ex.Message}");
            }
        }

        public int GetDataSize() => arraySize;
        public DateTime GetCreationTime() => creationTime;
        public string GetDescription() => description;
        
        // Proper disposal pattern - FIXED VERSION
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up managed resources
                largeDataArray?.Clear();
                largeDataArray = null;
                
                description = null;
                calculations = null;
                binaryData = null;
                
                // Unsubscribe from events to prevent memory leaks
                DataProcessed = null;
                
                Console.WriteLine($"  [DISPOSE] DataProcessor #{InstanceId} disposed properly");
            }
        }
        
        // Finalizer to track object destruction
        ~DataProcessor()
        {
            Console.WriteLine($"  [FINALIZER] DataProcessor #{InstanceId} is being finalized");
            Dispose(false);
        }
    }

    // =====================================================================================
    // EVENT PUBLISHER CLASS - FIXED VERSION WITH PROPER MEMORY MANAGEMENT
    // =====================================================================================
    public class EventPublisher : IDisposable
    {
        public event Action<string> SomeEvent;
        private List<string> eventLog = new List<string>();
        private readonly object lockObject = new object();
        private bool disposed = false;
        
        public void TriggerEvent(string message)
        {
            if (disposed) return;
            
            lock (lockObject)
            {
                // Limit event log size to prevent memory growth
                if (eventLog.Count >= 1000)
                {
                    eventLog.RemoveRange(0, 500); // Remove oldest 500 entries
                }
                eventLog.Add($"{DateTime.Now:HH:mm:ss.fff}: {message}");
            }
            SomeEvent?.Invoke(message);
        }
        
        public int GetEventLogSize() 
        {
            lock (lockObject)
            {
                return eventLog.Count;
            }
        }
        
        public void ClearEventLog()
        {
            lock (lockObject)
            {
                eventLog.Clear();
            }
        }
        
        // Method to clear all event subscriptions (for proper cleanup)
        public void ClearEventSubscriptions()
        {
            SomeEvent = null;
        }
        
        public void Dispose()
        {
            if (!disposed)
            {
                ClearEventSubscriptions(); // Unsubscribe all event handlers
                eventLog.Clear();
                disposed = true;
            }
        }
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
        
        // CLEANUP METHODS - FIXED VERSION WITH PROPER MEMORY MANAGEMENT
        public static void RemoveProcessor(DataProcessor processor)
        {
            if (staticProcessors.Remove(processor))
            {
                staticObjectRegistry.Remove(processor.InstanceId);
                Console.WriteLine($"  [CLEANUP] Removed DataProcessor #{processor.InstanceId} from static collection. Remaining: {staticProcessors.Count}");
            }
        }
        
        public static void RemovePublisher(EventPublisher publisher)
        {
            if (staticPublishers.Remove(publisher))
            {
                Console.WriteLine($"  [CLEANUP] Removed EventPublisher from static collection. Remaining: {staticPublishers.Count}");
            }
        }
        
        public static void ClearAllReferences()
        {
            int processorCount = staticProcessors.Count;
            int publisherCount = staticPublishers.Count;
            int registryCount = staticObjectRegistry.Count;
            
            staticProcessors.Clear();
            staticPublishers.Clear();
            staticObjectRegistry.Clear();
            
            Console.WriteLine($"  [CLEANUP] Cleared all static references: {processorCount} processors, {publisherCount} publishers, {registryCount} registry entries");
        }
        
        public static void ClearOldReferences(int maxAgeMinutes = 5)
        {
            var cutoffTime = DateTime.Now.AddMinutes(-maxAgeMinutes);
            var toRemove = staticProcessors.Where(p => p.GetCreationTime() < cutoffTime).ToList();
            
            foreach (var processor in toRemove)
            {
                RemoveProcessor(processor);
            }
            
            Console.WriteLine($"  [CLEANUP] Removed {toRemove.Count} old processors (older than {maxAgeMinutes} minutes)");
        }
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
            
            // DEMONSTRATE PROPER CLEANUP - FIXED VERSION
            Console.WriteLine("\n=== DEMONSTRATING PROPER CLEANUP ===");
            Console.WriteLine("Cleaning up old references to allow garbage collection...");
            StaticReferenceContainer.ClearOldReferences(0); // Remove all old references
            
            // Force GC after cleanup
            Console.WriteLine("  [CLEANUP] Forcing garbage collection after cleanup...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine($"  [CLEANUP] Memory after cleanup: {GC.GetTotalMemory(false) / 1024} KB");
        }

        // =====================================================================================
        // DEMONSTRATION 2: EVENT HANDLER LEAKS - FIXED VERSION
        // =====================================================================================
        public static void CreateEventHandlerLeaks()
        {
            Console.WriteLine("\n=== DEMONSTRATION 2: EVENT HANDLER LEAKS (FIXED) ===");
            Console.WriteLine("Demonstrating proper event handler management...");
            
            using (var mainPublisher = new EventPublisher())
            {
                var eventSubscribers = new List<DataProcessor>();
                var eventHandlers = new List<Action<string>>();
                
                for (int i = 0; i < DemoConfig.EVENT_HANDLER_ITERATIONS; i++)
                {
                    Console.WriteLine($"\n--- Event Handler Iteration {i + 1} ---");
                    
                    // Create processor
                    var processor = new DataProcessor(5000 + (i * 1000));
                    
                    // Create event handlers with proper cleanup
                    Action<string> processorHandler = (message) => {
                        Console.WriteLine($"    [EVENT] DataProcessor #{processor.InstanceId}: {message}");
                    };
                    
                    Action<string> publisherHandler = (message) => {
                        Console.WriteLine($"    [EVENT] Received by DataProcessor #{processor.InstanceId}: {message}");
                        processor.ProcessData();
                    };
                    
                    // Subscribe to events
                    processor.DataProcessed += processorHandler;
                    mainPublisher.SomeEvent += publisherHandler;
                    
                    // Store handlers for later cleanup
                    eventHandlers.Add(processorHandler);
                    eventHandlers.Add(publisherHandler);
                    eventSubscribers.Add(processor);
                    
                    // Trigger some events
                    mainPublisher.TriggerEvent($"Event triggered for iteration {i + 1}");
                    processor.ProcessData();
                    
                    Thread.Sleep(DemoConfig.EVENT_SIMULATION_DURATION_MS);
                }
                
                Console.WriteLine($"\n  [EVENT ANALYSIS] Created {eventSubscribers.Count} event subscribers");
                Console.WriteLine($"  [EVENT ANALYSIS] Publisher has {mainPublisher.GetEventLogSize()} logged events");
                
                // PROPER CLEANUP - FIXED VERSION
                Console.WriteLine("\n=== DEMONSTRATING PROPER EVENT CLEANUP ===");
                
                // Unsubscribe all event handlers
                foreach (var processor in eventSubscribers)
                {
                    processor.ClearEventSubscriptions(); // Clear all event subscriptions
                }
                
                // Clear publisher events
                mainPublisher.ClearEventSubscriptions();
                
                // Dispose processors
                foreach (var processor in eventSubscribers)
                {
                    processor.Dispose();
                }
                
                // Clear collections
                eventSubscribers.Clear();
                eventHandlers.Clear();
                
                // Force GC after cleanup
                Console.WriteLine("  [CLEANUP] Forcing garbage collection after event cleanup...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine($"  [CLEANUP] Memory after event cleanup: {GC.GetTotalMemory(false) / 1024} KB");
            } // Publisher automatically disposed here
        }

        // =====================================================================================
        // DEMONSTRATION 3: CONTINUOUS GROWTH SIMULATION - FIXED VERSION
        // =====================================================================================
        public static async Task SimulateContinuousGrowth()
        {
            Console.WriteLine("\n=== DEMONSTRATION 3: CONTINUOUS GROWTH SIMULATION (FIXED) ===");
            Console.WriteLine($"Simulating {DemoConfig.CONTINUOUS_DURATION_SECONDS} seconds with proper memory management...");
            Console.WriteLine("This demonstrates proper object lifecycle management...");
            
            var stopwatch = Stopwatch.StartNew();
            var endTime = TimeSpan.FromSeconds(DemoConfig.CONTINUOUS_DURATION_SECONDS);
            var createdProcessors = new List<DataProcessor>();
            
            int iteration = 0;
            
            while (stopwatch.Elapsed < endTime)
            {
                iteration++;
                
                // Create objects periodically with proper lifecycle management
                if (iteration % DemoConfig.CONTINUOUS_CREATION_INTERVAL == 0)
                {
                    int processorSize = DemoConfig.CONTINUOUS_PROCESSOR_BASE_SIZE + 
                                      (iteration * DemoConfig.CONTINUOUS_PROCESSOR_SIZE_INCREMENT);
                    
                    var newProcessor = new DataProcessor(processorSize);
                    newProcessor.ProcessData();
                    
                    // Add to temporary list for proper cleanup
                    createdProcessors.Add(newProcessor);
                    
                    // Add to static collection for demonstration
                    StaticReferenceContainer.AddProcessor(newProcessor);
                    
                    Console.WriteLine($"  [GROWTH] Iteration {iteration} - Total objects: {createdProcessors.Count}");
                }
                
                // Cleanup old objects periodically (FIXED VERSION)
                if (iteration % (DemoConfig.CONTINUOUS_CREATION_INTERVAL * 3) == 0)
                {
                    Console.WriteLine("  [CLEANUP] Cleaning up old objects...");
                    
                    // Remove old processors from static collection
                    var oldProcessors = createdProcessors.Where(p => 
                        DateTime.Now - p.GetCreationTime() > TimeSpan.FromMinutes(1)).ToList();
                    
                    foreach (var processor in oldProcessors)
                    {
                        StaticReferenceContainer.RemoveProcessor(processor);
                        processor.Dispose();
                        createdProcessors.Remove(processor);
                    }
                    
                    Console.WriteLine($"  [CLEANUP] Removed {oldProcessors.Count} old processors");
                }
                
                // Show status and memory information
                if (iteration % DemoConfig.STATUS_INTERVAL == 0)
                {
                    long currentMemory = GC.GetTotalMemory(false);
                    Console.WriteLine($"  [STATUS] Iteration {iteration} - Current memory: {currentMemory / 1024} KB");
                    Console.WriteLine($"  [STATUS] Active objects: {createdProcessors.Count}");
                    Console.WriteLine($"  [STATUS] Static references: {StaticReferenceContainer.GetProcessorCount()} processors");
                    
                    if (DemoConfig.SHOW_GC_INFORMATION)
                    {
                        Console.WriteLine($"  [GC INFO] Generation 0 collections: {GC.CollectionCount(0)}");
                        Console.WriteLine($"  [GC INFO] Generation 1 collections: {GC.CollectionCount(1)}");
                        Console.WriteLine($"  [GC INFO] Generation 2 collections: {GC.CollectionCount(2)}");
                    }
                    
                    Console.WriteLine("  [TIP] Observe proper memory management with cleanup!");
                }
                
                // Pause for execution control
                await Task.Delay(DemoConfig.LOOP_PAUSE_MS);
            }
            
            stopwatch.Stop();
            
            // PROPER CLEANUP AT END
            Console.WriteLine("\n=== CLEANING UP CONTINUOUS SIMULATION ===");
            foreach (var processor in createdProcessors)
            {
                processor.Dispose();
            }
            createdProcessors.Clear();
            
            Console.WriteLine("\n=== CONTINUOUS SIMULATION COMPLETED ===");
            Console.WriteLine($"Total objects processed: {StaticReferenceContainer.GetProcessorCount()}");
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
            
            // PROPER FINAL CLEANUP - FIXED VERSION
            Console.WriteLine("\n[CLEANUP] Performing proper final cleanup...");
            
            // Clear all static references to allow garbage collection
            StaticReferenceContainer.ClearAllReferences();
            
            // Force garbage collection after cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long cleanupMemory = GC.GetTotalMemory(false);
            Console.WriteLine($"[CLEANUP] Memory after proper cleanup: {cleanupMemory / 1024} KB");
            Console.WriteLine("Notice: Memory IS now freed due to proper cleanup!");
            
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
