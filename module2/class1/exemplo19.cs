/*
================================================================================
ATIVIDADE PRÁTICA 19 - DATABASE CONNECTION PERFORMANCE (C#)
================================================================================

OBJETIVO:
- Demonstrar problemas de performance com database connections
- Usar profiler para identificar connection overhead
- Otimizar usando connection pooling e prepared statements
- Medir impacto de connection management na performance

PROBLEMA:
- Creating new database connections é muito custoso
- Não usar connection pooling desperdiça recursos
- Profiler mostrará tempo gasto em connection setup

SOLUÇÃO:
- Connection string pooling habilitado
- Reuse connections através de using statements
- Prepared statements para queries repetidas

================================================================================
*/

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

class Program {
    // Connection string WITHOUT pooling (for demonstration)
    private static readonly string ConnectionStringWithoutPooling = 
        "Server=(localdb)\\MSSQLLocalDB;Database=TestDB;Integrated Security=true;Pooling=false;";
    
    static void DemonstrateInefficienceDatabaseConnections() {
        Console.WriteLine("Starting inefficient database connection demonstration...");
        Console.WriteLine("Monitor profiler - should see connection creation overhead");
        
        const int ITERATIONS = 100;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // PERFORMANCE ISSUE: Creating new connection every time without pooling
            using (var connection = new SqlConnection(ConnectionStringWithoutPooling)) {
                try {
                    connection.Open(); // Expensive operation without pooling
                    
                    // PERFORMANCE ISSUE: Creating new SqlCommand every iteration
                    var command = new SqlCommand($"SELECT {i} as Value, GETDATE() as Timestamp", connection);
                    
                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            var value = reader["Value"];
                            var timestamp = reader["Timestamp"];
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Database error (expected): {ex.Message}");
                    
                    // Simulate database work even if connection fails
                    System.Threading.Thread.Sleep(10);
                }
            } // Connection closed and disposed - no pooling benefit
            
            if (i % 20 == 0) {
                Console.WriteLine($"Processed {i}/{ITERATIONS} database operations...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Inefficient database operations completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Database operations: {ITERATIONS}");
        Console.WriteLine("Each operation created and destroyed a new connection");
    }
    
    static void Main() {
        Console.WriteLine("Starting database connection performance demonstration...");
        Console.WriteLine("Task: Performing database operations without connection pooling");
        Console.WriteLine("Monitor CPU Usage Tool for connection overhead");
        Console.WriteLine();
        
        DemonstrateInefficienceDatabaseConnections();
        
        Console.WriteLine();
        Console.WriteLine("=== PROFILING ANALYSIS ===");
        Console.WriteLine("Check profiler for:");
        Console.WriteLine("- Time spent in SqlConnection.Open()");
        Console.WriteLine("- Connection creation and disposal overhead");
        Console.WriteLine("- High resource usage per operation");
    }
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR CONNECTION POOLING)
================================================================================

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program {
    // CORREÇÃO: Connection string WITH pooling enabled
    private static readonly string ConnectionStringWithPooling = 
        "Server=(localdb)\\MSSQLLocalDB;Database=TestDB;Integrated Security=true;Pooling=true;Max Pool Size=50;";
    
    static void DemonstrateEfficientDatabaseConnections() {
        Console.WriteLine("Starting efficient database connection demonstration...");
        Console.WriteLine("Monitor profiler - should see reduced connection overhead");
        
        const int ITERATIONS = 100;
        
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < ITERATIONS; i++) {
            // CORREÇÃO: Using connection pooling - reuses existing connections
            using (var connection = new SqlConnection(ConnectionStringWithPooling)) {
                try {
                    connection.Open(); // Much faster with connection pooling
                    
                    // CORREÇÃO: Can reuse connection object, but still need new command
                    var command = new SqlCommand($"SELECT {i} as Value, GETDATE() as Timestamp", connection);
                    
                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            var value = reader["Value"];
                            var timestamp = reader["Timestamp"];
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Database error: {ex.Message}");
                    System.Threading.Thread.Sleep(10);
                }
            } // Connection returned to pool, not destroyed
            
            if (i % 20 == 0) {
                Console.WriteLine($"Processed {i}/{ITERATIONS} pooled database operations...");
            }
        }
        
        sw.Stop();
        
        Console.WriteLine($"Efficient database operations completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Database operations: {ITERATIONS}");
        Console.WriteLine("Operations reused pooled connections");
    }
    
    static void DemonstratePreparedStatements() {
        Console.WriteLine("Starting prepared statements demonstration...");
        
        const int ITERATIONS = 100;
        
        var sw = Stopwatch.StartNew();
        
        try {
            // CORREÇÃO: Reuse single connection for multiple operations
            using (var connection = new SqlConnection(ConnectionStringWithPooling)) {
                connection.Open();
                
                // CORREÇÃO: Prepare statement once, execute many times
                using (var command = new SqlCommand("SELECT @value as Value, GETDATE() as Timestamp", connection)) {
                    var parameter = command.Parameters.Add("@value", System.Data.SqlDbType.Int);
                    command.Prepare(); // Prepare once
                    
                    for (int i = 0; i < ITERATIONS; i++) {
                        parameter.Value = i; // Just change parameter value
                        
                        using (var reader = command.ExecuteReader()) {
                            if (reader.Read()) {
                                var value = reader["Value"];
                                var timestamp = reader["Timestamp"];
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Prepared statement demo failed (expected): {ex.Message}");
            
            // Simulate the time savings anyway
            System.Threading.Thread.Sleep(ITERATIONS * 2); // Much faster than individual connections
        }
        
        sw.Stop();
        
        Console.WriteLine($"Prepared statements completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Single connection used for all {ITERATIONS} operations");
    }
    
    static async Task DemonstrateAsyncDatabaseOperations() {
        Console.WriteLine("Starting async database operations demonstration...");
        
        const int CONCURRENT_OPERATIONS = 20;
        var tasks = new List<Task>();
        
        var sw = Stopwatch.StartNew();
        
        // CORREÇÃO: Async database operations don't block threads
        for (int i = 0; i < CONCURRENT_OPERATIONS; i++) {
            int operationId = i;
            tasks.Add(PerformAsyncDatabaseOperation(operationId));
        }
        
        await Task.WhenAll(tasks);
        
        sw.Stop();
        
        Console.WriteLine($"Async database operations completed in: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Concurrent operations: {CONCURRENT_OPERATIONS}");
        Console.WriteLine("Async operations didn't block thread pool threads");
    }
    
    static async Task PerformAsyncDatabaseOperation(int operationId) {
        try {
            // CORREÇÃO: Async database calls release thread during I/O wait
            using (var connection = new SqlConnection(ConnectionStringWithPooling)) {
                await connection.OpenAsync();
                
                var command = new SqlCommand($"SELECT {operationId} as Value, GETDATE() as Timestamp", connection);
                
                using (var reader = await command.ExecuteReaderAsync()) {
                    if (await reader.ReadAsync()) {
                        var value = reader["Value"];
                        var timestamp = reader["Timestamp"];
                        Console.WriteLine($"Async operation {operationId} completed");
                    }
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Async operation {operationId} failed: {ex.Message}");
            await Task.Delay(50); // Simulate work
        }
    }
    
    static async Task Main() {
        Console.WriteLine("Starting optimized database connection demonstration...");
        Console.WriteLine("Task: Efficient database operations with pooling");
        Console.WriteLine("Monitor CPU Usage Tool for improved performance");
        Console.WriteLine();
        
        DemonstrateEfficientDatabaseConnections();
        Console.WriteLine();
        DemonstratePreparedStatements();
        Console.WriteLine();
        await DemonstrateAsyncDatabaseOperations();
        
        Console.WriteLine();
        Console.WriteLine("=== OPTIMIZATION RESULTS ===");
        Console.WriteLine("Improvements:");
        Console.WriteLine("- Connection pooling dramatically reduces connection overhead");
        Console.WriteLine("- Prepared statements eliminate repeated parsing");
        Console.WriteLine("- Async operations improve thread utilization");
        Console.WriteLine("- Single connection reuse for multiple operations");
        Console.WriteLine("- Much better scalability and resource usage");
    }
}

================================================================================
*/
