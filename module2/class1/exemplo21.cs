/*
================================================================================
ATIVIDADE PRÁTICA 21 - FINALIZER PERFORMANCE OVERHEAD (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de finalizers no GC performance
- Usar Memory profiler para identificar finalizer pressure
- Otimizar removendo finalizers desnecessários e usando IDisposable
- Medir impacto de finalizers no GC generations

PROBLEMA:
- Finalizers forçam objetos para generation 1
- Finalizer thread pode criar gargalos
- Memory profiler mostrará finalizer queue growth

SOLUÇÃO:
- Implement IDisposable properly
- Remove finalizers quando não necessários
- Use SafeHandle para unmanaged resources

================================================================================
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// PERFORMANCE ISSUE: Unnecessary finalizer
class BadResourceWrapper {
    private IntPtr unmanagedResource;
    
    public BadResourceWrapper() {
        // Simulate unmanaged resource allocation
        unmanagedResource = Marshal.AllocHGlobal(1024);
    }
    
    // PERFORMANCE ISSUE: Finalizer forces object to Gen 1 and requires finalizer thread
    ~BadResourceWrapper() {
        if (unmanagedResource != IntPtr.Zero) {
            Marshal.FreeHGlobal(unmanagedResource);
            unmanagedResource = IntPtr.Zero;
        }
    }
}

class Program {
    static void DemonstrateFinalizerOverhead() {
        Console.WriteLine("Starting finalizer overhead demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see finalizer queue pressure");
        
        const int ITERATIONS = 50000;
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialGen0 = GC.CollectionCount(0);
        int initialGen1 = GC.CollectionCount(1);
        int initialGen2 = GC.CollectionCount(2);
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Creating objects with finalizers
            var wrapper = new BadResourceWrapper();
            
            // Objects with finalizers survive first GC and go to Gen 1
            // This puts pressure on the finalizer queue
            
            if (i % 5000 == 0) {
                Console.WriteLine($"Created {i}/{ITERATIONS} objects with finalizers...");
                Console.WriteLine($"  Gen 0 collections: {GC.CollectionCount(0)}");
                Console.WriteLine($"  Gen 1 collections: {GC.CollectionCount(1)}");
                Console.WriteLine($"  Current memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        // Force GC to show finalizer impact
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalGen0 = GC.CollectionCount(0);
        int finalGen1 = GC.CollectionCount(1);
        int finalGen2 = GC.CollectionCount(2);
        
        Console.WriteLine($"Finalizer overhead test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Objects created: {ITERATIONS}");
        Console.WriteLine($"Memory growth: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"Gen 0 collections: {finalGen0 - initialGen0}");
        Console.WriteLine($"Gen 1 collections: {finalGen1 - initialGen1}");
        Console.WriteLine($"Gen 2 collections: {finalGen2 - initialGen2}");
        Console.WriteLine("Finalizers caused objects to survive to Gen 1");
    }
    
    static void Main() {
        Console.WriteLine("Starting finalizer performance demonstration...");
        Console.WriteLine("Task: Creating objects with finalizers");
        Console.WriteLine("Monitor Memory Usage Tool for finalizer pressure");
        Console.WriteLine();
        
        DemonstrateFinalizerOverhead();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check Memory profiler for:");
        Console.WriteLine("- Finalizer queue growth");
        Console.WriteLine("- Objects surviving to Gen 1 due to finalizers");
        Console.WriteLine("- Increased GC pressure and collections");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR PROPER DISPOSAL)
================================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// CORREÇÃO: Proper IDisposable implementation without finalizer
class GoodResourceWrapper : IDisposable {
    private IntPtr unmanagedResource;
    private bool disposed = false;
    
    public GoodResourceWrapper() {
        unmanagedResource = Marshal.AllocHGlobal(1024);
    }
    
    // CORREÇÃO: Implement IDisposable properly
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this); // Prevent finalizer from running
    }
    
    protected virtual void Dispose(bool disposing) {
        if (!disposed) {
            if (unmanagedResource != IntPtr.Zero) {
                Marshal.FreeHGlobal(unmanagedResource);
                unmanagedResource = IntPtr.Zero;
            }
            disposed = true;
        }
    }
    
    // No finalizer needed - using statements handle cleanup
}

// CORREÇÃO: Even better - use SafeHandle for unmanaged resources
class SafeResourceHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid {
    public SafeResourceHandle() : base(true) {
        SetHandle(Marshal.AllocHGlobal(1024));
    }
    
    protected override bool ReleaseHandle() {
        if (handle != IntPtr.Zero) {
            Marshal.FreeHGlobal(handle);
            return true;
        }
        return false;
    }
}

class OptimalResourceWrapper : IDisposable {
    private SafeResourceHandle resource;
    
    public OptimalResourceWrapper() {
        resource = new SafeResourceHandle();
    }
    
    public void Dispose() {
        resource?.Dispose();
    }
}

class Program {
    static void DemonstrateProperDisposal() {
        Console.WriteLine("Starting proper disposal demonstration...");
        Console.WriteLine("Monitor Memory profiler - should see reduced GC pressure");
        
        const int ITERATIONS = 50000;
        
        long initialMemory = GC.GetTotalMemory(false);
        int initialGen0 = GC.CollectionCount(0);
        int initialGen1 = GC.CollectionCount(1);
        int initialGen2 = GC.CollectionCount(2);
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Using statement ensures proper disposal
            using (var wrapper = new GoodResourceWrapper()) {
                // Resource is properly disposed when using block exits
                // No finalizer needed, objects can be collected in Gen 0
            }
            
            if (i % 5000 == 0) {
                Console.WriteLine($"Created {i}/{ITERATIONS} properly disposed objects...");
                Console.WriteLine($"  Gen 0 collections: {GC.CollectionCount(0)}");
                Console.WriteLine($"  Gen 1 collections: {GC.CollectionCount(1)}");
                Console.WriteLine($"  Current memory: {GC.GetTotalMemory(false):N0} bytes");
            }
        }
        
        sw.Stop();
        
        GC.Collect();
        
        long finalMemory = GC.GetTotalMemory(false);
        int finalGen0 = GC.CollectionCount(0);
        int finalGen1 = GC.CollectionCount(1);
        int finalGen2 = GC.CollectionCount(2);
        
        Console.WriteLine($"Proper disposal test completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Objects created: {ITERATIONS}");
        Console.WriteLine($"Memory growth: {(finalMemory - initialMemory):N0} bytes");
        Console.WriteLine($"Gen 0 collections: {finalGen0 - initialGen0}");
        Console.WriteLine($"Gen 1 collections: {finalGen1 - initialGen1}");
        Console.WriteLine($"Gen 2 collections: {finalGen2 - initialGen2}");
        Console.WriteLine("Proper disposal allows Gen 0 collection");
    }
    
    static void DemonstrateSafeHandleApproach() {
        Console.WriteLine("Starting SafeHandle demonstration...");
        
        const int ITERATIONS = 50000;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: SafeHandle provides automatic cleanup
            using (var wrapper = new OptimalResourceWrapper()) {
                // SafeHandle handles the complexity of finalizers correctly
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"SafeHandle approach completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine("SafeHandle provides optimal unmanaged resource management");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized disposal demonstration...");
        Console.WriteLine("Task: Proper resource disposal without finalizer overhead");
        Console.WriteLine("Monitor Memory Usage Tool for reduced GC pressure");
        Console.WriteLine();
        
        DemonstrateProperDisposal();
        Console.WriteLine();
        DemonstrateSafeHandleApproach();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- IDisposable eliminates finalizer overhead");
        Console.WriteLine("- Objects can be collected in Gen 0");
        Console.WriteLine("- Reduced GC pressure and collections");
        Console.WriteLine("- SafeHandle provides optimal unmanaged resource handling");
        Console.WriteLine("- Much better memory management performance");
    }
}

================================================================================
*/
