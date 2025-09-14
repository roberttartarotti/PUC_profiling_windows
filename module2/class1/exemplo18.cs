/*
================================================================================
ATIVIDADE PRÁTICA 18 - JSON SERIALIZATION PERFORMANCE (C#)
================================================================================

OBJETIVO:
- Demonstrar overhead de JSON serialization repeated em loops
- Usar CPU profiler para identificar tempo gasto em serialization
- Otimizar usando JsonSerializer com options caching
- Comparar Newtonsoft.Json vs System.Text.Json performance

PROBLEMA:
- Repeated serialization/deserialization é custosa
- Reflection overhead em serializers
- CPU Profiler mostrará tempo gasto em JSON operations

SOLUÇÃO:
- Cache serializer options e settings
- Use System.Text.Json para better performance
- Serialize em batches quando possível

================================================================================
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;

class DataRecord {
    public int Id { get; set; }
    public string Name { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; }
    
    public static DataRecord CreateSample(int id) {
        return new DataRecord {
            Id = id,
            Name = $"Record_{id:D6}",
            Value = id * 3.14159,
            Timestamp = DateTime.Now.AddMinutes(-id),
            IsActive = id % 2 == 0,
            Tags = new List<string> { $"tag_{id % 3}", $"category_{id % 5}" }
        };
    }
}

class Program {
    static void DemonstrateInefficiientSerialization() {
        Console.WriteLine("Starting inefficient JSON serialization demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see time spent in JSON operations");
        
        const int ITERATIONS = 10000;
        var records = new List<DataRecord>();
        var jsonResults = new List<string>();
        
        // Create test data
        for (int i = 0; i < ITERATIONS; i++) {
            records.Add(DataRecord.CreateSample(i));
        }
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Creating new JsonSerializerSettings every time
            var settings = new JsonSerializerSettings {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
            
            // PERFORMANCE ISSUE: Newtonsoft.Json with repeated settings creation
            string json = JsonConvert.SerializeObject(records[i], settings);
            jsonResults.Add(json);
            
            // PERFORMANCE ISSUE: Immediate deserialization with new settings
            var deserializedSettings = new JsonSerializerSettings {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore
            };
            
            DataRecord deserialized = JsonConvert.DeserializeObject<DataRecord>(json, deserializedSettings);
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Serialized {i}/{ITERATIONS} objects...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Inefficient JSON serialization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total objects serialized: {jsonResults.Count}");
        Console.WriteLine($"Average JSON size: {jsonResults[0].Length} chars");
    }
    
    static void Main() {
        Console.WriteLine("Starting JSON serialization performance demonstration...");
        Console.WriteLine("Task: Serializing objects with repeated settings creation");
        Console.WriteLine("Monitor CPU Usage Tool for JSON serialization overhead");
        Console.WriteLine();
        
        DemonstrateInefficiientSerialization();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check CPU profiler for:");
        Console.WriteLine("- Time spent in JsonConvert.SerializeObject");
        Console.WriteLine("- Reflection overhead in serialization");
        Console.WriteLine("- Repeated settings object creation");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR EFFICIENT SERIALIZATION)
================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

class DataRecord {
    public int Id { get; set; }
    public string Name { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; }
    
    public static DataRecord CreateSample(int id) {
        return new DataRecord {
            Id = id,
            Name = $"Record_{id:D6}",
            Value = id * 3.14159,
            Timestamp = DateTime.Now.AddMinutes(-id),
            IsActive = id % 2 == 0,
            Tags = new List<string> { $"tag_{id % 3}", $"category_{id % 5}" }
        };
    }
}

class Program {
    // CORREÇÃO: Cache JsonSerializerOptions to avoid repeated creation
    private static readonly JsonSerializerOptions CachedOptions = new JsonSerializerOptions {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    
    static void DemonstrateEfficientSerialization() {
        Console.WriteLine("Starting efficient JSON serialization demonstration...");
        Console.WriteLine("Monitor CPU profiler - should see reduced JSON overhead");
        
        const int ITERATIONS = 10000;
        var records = new List<DataRecord>();
        var jsonResults = new List<string>();
        
        // Create test data
        for (int i = 0; i < ITERATIONS; i++) {
            records.Add(DataRecord.CreateSample(i));
        }
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Use cached options with System.Text.Json (faster than Newtonsoft)
            string json = System.Text.Json.JsonSerializer.Serialize(records[i], CachedOptions);
            jsonResults.Add(json);
            
            // CORREÇÃO: Reuse same cached options for deserialization
            DataRecord deserialized = System.Text.Json.JsonSerializer.Deserialize<DataRecord>(json, CachedOptions);
            
            if (i % 1000 == 0) {
                Console.WriteLine($"Efficiently serialized {i}/{ITERATIONS} objects...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Efficient JSON serialization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total objects serialized: {jsonResults.Count}");
        Console.WriteLine($"Using System.Text.Json with cached options");
    }
    
    static void DemonstrateBatchSerialization() {
        Console.WriteLine("Starting batch serialization demonstration...");
        
        const int ITERATIONS = 10000;
        const int BATCH_SIZE = 1000;
        
        var records = new List<DataRecord>();
        var batchResults = new List<string>();
        
        // Create test data
        for (int i = 0; i < ITERATIONS; i++) {
            records.Add(DataRecord.CreateSample(i));
        }
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Serialize in batches for better efficiency
        for (int i = 0; i < records.Count; i += BATCH_SIZE) {
            var batch = records.GetRange(i, Math.Min(BATCH_SIZE, records.Count - i));
            
            // Serialize entire batch at once - much more efficient
            string batchJson = System.Text.Json.JsonSerializer.Serialize(batch, CachedOptions);
            batchResults.Add(batchJson);
            
            if (i % 2000 == 0) {
                Console.WriteLine($"Batch serialized {i}/{records.Count} objects...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Batch serialization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total batches: {batchResults.Count}");
        Console.WriteLine($"Objects per batch: {BATCH_SIZE}");
    }
    
    static void DemonstrateMemoryEfficientSerialization() {
        Console.WriteLine("Starting memory-efficient serialization...");
        
        const int ITERATIONS = 10000;
        var records = new List<DataRecord>();
        
        for (int i = 0; i < ITERATIONS; i++) {
            records.Add(DataRecord.CreateSample(i));
        }
        
        var sw = Stopwatch.StartNew();
        long totalBytes = 0;
        
        // CORREÇÃO: Use Utf8JsonWriter for maximum performance
        for (int i = 0; i < ITERATIONS; i++) {
            using var buffer = new System.Buffers.ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            
            System.Text.Json.JsonSerializer.Serialize(writer, records[i], CachedOptions);
            totalBytes += buffer.WrittenCount;
        }
        
        sw.Stop();
        
        Console.WriteLine($"Memory-efficient serialization completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Total bytes written: {totalBytes:N0}");
        Console.WriteLine($"Using Utf8JsonWriter for maximum performance");
    }
    
    static void Main() {
        Console.WriteLine("Starting optimized JSON serialization demonstration...");
        Console.WriteLine("Task: Efficient JSON serialization strategies");
        Console.WriteLine("Monitor CPU Usage Tool for improved performance");
        Console.WriteLine();
        
        DemonstrateEfficientSerialization();
        Console.WriteLine();
        DemonstrateBatchSerialization();
        Console.WriteLine();
        DemonstrateMemoryEfficientSerialization();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- System.Text.Json is faster than Newtonsoft.Json");
        Console.WriteLine("- Cached options eliminate repeated setup overhead");
        Console.WriteLine("- Batch serialization reduces per-object overhead");
        Console.WriteLine("- Utf8JsonWriter provides maximum performance");
        Console.WriteLine("- Significantly reduced CPU usage and memory allocations");
    }
}

================================================================================
*/
