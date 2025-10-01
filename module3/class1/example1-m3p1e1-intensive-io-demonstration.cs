using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace IntensiveIODemonstration
{
    // ====================================================================
    // CONFIGURATION VARIABLES - EASY TO MODIFY FOR DIFFERENT SCENARIOS
    // ====================================================================
    public static class IOConfig
    {
        public const int MIN_FILE_SIZE_KB = 100;        // Minimum file size in KB
        public const int MAX_FILE_SIZE_KB = 500;        // Maximum file size in KB
        public const int WRITE_CHUNK_SIZE = 4096;       // Write chunk size in bytes (4KB)
        public const int READ_BUFFER_SIZE = 1024;       // Read buffer size in bytes (1KB)
        public const int READ_REPETITIONS = 3;          // How many times to read each file
        public const int STATISTICS_INTERVAL = 10;      // Show statistics every N cycles
        public const int CYCLE_DELAY_MS = 500;          // Delay between cycles in milliseconds
        public const int WRITE_DELAY_MS = 1;            // Delay between write operations in ms
        public const int READ_DELAY_MS = 1;             // Delay between read operations in ms
        public const int GC_DEMO_INTERVAL = 5;          // Perform GC demo every N cycles
        public const int GC_WASTE_OBJECTS = 10;         // Number of waste objects for GC demo
        public const int GC_WASTE_SIZE_MB = 1;          // Size of each waste object in MB
        public const string BASE_FILENAME = "intensive_io_file_";  // Base name for temp files
    }
    // ====================================================================

    class IntensiveIODemo
    {
        private readonly string baseFileName;
        private int fileCounter;
        private long totalBytesWritten;
        private long totalBytesRead;
        private int totalOperations;
        private readonly Stopwatch stopwatch;
        private readonly Random random;

        public IntensiveIODemo()
        {
            baseFileName = IOConfig.BASE_FILENAME;
            fileCounter = 0;
            totalBytesWritten = 0;
            totalBytesRead = 0;
            totalOperations = 0;
            stopwatch = Stopwatch.StartNew();
            random = new Random();
        }

        private string GenerateLargeContent(int sizeKB)
        {
            var sb = new StringBuilder();
            
            // Header information
            sb.AppendLine("=== INTENSIVE I/O DEMONSTRATION DATA ===");
            sb.AppendLine($"File #{fileCounter} - Timestamp: {DateTimeOffset.Now.ToUnixTimeMilliseconds()}");
            sb.AppendLine($"Size: {sizeKB} KB");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine();

            // Calculate remaining size needed
            int currentSize = Encoding.UTF8.GetByteCount(sb.ToString());
            int targetSize = sizeKB * 1024;
            int remainingSize = targetSize - currentSize;

            // Fill with random data
            var chars = new char[remainingSize];
            for (int i = 0; i < remainingSize; i++)
            {
                if (i % 80 == 79)
                {
                    chars[i] = '\n'; // Add line breaks for readability
                }
                else
                {
                    chars[i] = (char)random.Next(65, 91); // A-Z characters
                }
            }

            sb.Append(chars);
            return sb.ToString();
        }

        private void PerformIntensiveWrite()
        {
            string fileName = $"{baseFileName}{fileCounter}.tmp";
            
            // Generate large content (using configured size range)
            int fileSize = random.Next(IOConfig.MIN_FILE_SIZE_KB, IOConfig.MAX_FILE_SIZE_KB + 1);
            string content = GenerateLargeContent(fileSize);
            
            // Perform multiple write operations to the same file
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(fileStream))
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                
                // Write in chunks to create more I/O operations
                for (int i = 0; i < contentBytes.Length; i += IOConfig.WRITE_CHUNK_SIZE)
                {
                    int currentChunkSize = Math.Min(IOConfig.WRITE_CHUNK_SIZE, contentBytes.Length - i);
                    byte[] chunk = new byte[currentChunkSize];
                    Array.Copy(contentBytes, i, chunk, 0, currentChunkSize);
                    
                    writer.Write(chunk);
                    fileStream.Flush(true); // Force immediate write to disk
                    
                    totalBytesWritten += currentChunkSize;
                    totalOperations++;
                    
                    // Small delay to make operations visible
                    Thread.Sleep(IOConfig.WRITE_DELAY_MS);
                }
            }
            
            Console.WriteLine($"WRITE: Created {fileName} ({fileSize} KB)");
        }

        private void PerformIntensiveRead()
        {
            string fileName = $"{baseFileName}{fileCounter}.tmp";
            
            // Read the file in small chunks multiple times
            for (int readAttempt = 0; readAttempt < IOConfig.READ_REPETITIONS; readAttempt++)
            {
                if (File.Exists(fileName))
                {
                    using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new BinaryReader(fileStream))
                    {
                        long fileSize = fileStream.Length;
                        
                        // Read in small chunks
                        byte[] buffer = new byte[IOConfig.READ_BUFFER_SIZE];
                        int bytesRead;
                        
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            totalOperations++;
                            
                            // Small delay to make operations visible
                            Thread.Sleep(IOConfig.READ_DELAY_MS);
                        }
                        
                        Console.WriteLine($"READ #{readAttempt + 1}: {fileName} ({fileSize} bytes)");
                    }
                }
            }
        }

        private void DeleteTemporaryFile()
        {
            string fileName = $"{baseFileName}{fileCounter}.tmp";
            
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Console.WriteLine($"DELETE: Removed {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {fileName}: {ex.Message}");
            }
        }

        private void DisplayStatistics()
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            
            Console.WriteLine();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("INTENSIVE I/O STATISTICS");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Running time: {elapsed.TotalSeconds:F1} seconds");
            Console.WriteLine($"Total operations: {totalOperations:N0}");
            Console.WriteLine($"Files processed: {fileCounter:N0}");
            Console.WriteLine($"Total bytes written: {totalBytesWritten / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Total bytes read: {totalBytesRead / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"Operations per second: {(elapsed.TotalSeconds > 0 ? totalOperations / elapsed.TotalSeconds : 0):F2}");
            Console.WriteLine($"Memory usage: {GC.GetTotalMemory(false) / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine(new string('=', 60));
        }

        private void PerformGarbageCollectionDemo()
        {
            // Intentionally create and abandon large objects to demonstrate GC pressure
            var wastefulList = new List<byte[]>();
            
            for (int i = 0; i < IOConfig.GC_WASTE_OBJECTS; i++)
            {
                // Create large byte arrays that will be collected
                byte[] wasteData = new byte[IOConfig.GC_WASTE_SIZE_MB * 1024 * 1024]; // Configurable MB each
                wastefulList.Add(wasteData);
            }
            
            // Clear the list to make objects eligible for GC
            wastefulList.Clear();
            
            // Force garbage collection to demonstrate memory pressure
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            Console.WriteLine("GC: Performed aggressive garbage collection");
        }

        public void Run()
        {
            Console.WriteLine("=== INTENSIVE I/O DEMONSTRATION (C#) ===");
            Console.WriteLine("This program will perform excessive disk I/O operations");
            Console.WriteLine("WARNING: This will stress your disk subsystem!");
            Console.WriteLine("Press any key to stop the demonstration...");
            Console.WriteLine(new string('-', 50));

            bool continueRunning = true;
            
            // Start a background task to monitor for key presses
            var keyMonitorTask = System.Threading.Tasks.Task.Run(() =>
            {
                Console.ReadKey(true);
                continueRunning = false;
                Console.WriteLine("\nStopping demonstration...");
            });

            while (continueRunning)
            {
                fileCounter++;
                
                Console.WriteLine($"\n--- Cycle #{fileCounter} ---");
                
                try
                {
                    // Perform intensive I/O operations
                    PerformIntensiveWrite();
                    PerformIntensiveRead();
                    DeleteTemporaryFile();
                    
                    // Occasionally demonstrate memory pressure
                    if (fileCounter % IOConfig.GC_DEMO_INTERVAL == 0)
                    {
                        PerformGarbageCollectionDemo();
                    }
                    
                    // Display statistics every configured interval
                    if (fileCounter % IOConfig.STATISTICS_INTERVAL == 0)
                    {
                        DisplayStatistics();
                    }
                    
                    // Brief pause between cycles
                    Thread.Sleep(IOConfig.CYCLE_DELAY_MS);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in cycle {fileCounter}: {ex.Message}");
                    Thread.Sleep(1000); // Longer pause on error
                }
            }
            
            // Clean up any remaining temporary files
            CleanupTemporaryFiles();
            
            // Final statistics
            DisplayStatistics();
            
            Console.WriteLine("\nDemonstration completed. Press any key to exit...");
            Console.ReadKey(true);
        }

        private void CleanupTemporaryFiles()
        {
            try
            {
                string[] tempFiles = Directory.GetFiles(".", $"{IOConfig.BASE_FILENAME}*.tmp");
                foreach (string file in tempFiles)
                {
                    File.Delete(file);
                    Console.WriteLine($"CLEANUP: Removed {file}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var demo = new IntensiveIODemo();
                demo.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }
    }
}
