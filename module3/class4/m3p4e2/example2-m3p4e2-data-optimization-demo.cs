/*
 * =====================================================================================
 * DATA OPTIMIZATION DEMONSTRATION - C# (MODULE 3, CLASS 4 - EXAMPLE 2)
 * =====================================================================================
 *
 * Purpose: Demonstrate various data optimization techniques to reduce bandwidth usage
 *          and improve application performance
 *
 * Educational Context:
 * - Show deduplication techniques to avoid redundant data
 * - Demonstrate compact data formats (JSON vs Binary formats)
 * - Illustrate payload optimization and header minimization
 * - Compare different compression algorithms (gzip, custom)
 * - Show benefits: lower bandwidth, faster loading, energy savings
 *
 * What this demonstrates:
 * - Data deduplication reduces redundant information
 * - Binary formats are more efficient than text formats
 * - Compression algorithms can significantly reduce data size
 * - Optimized payloads improve network performance
 * - Multiple optimization techniques can be combined
 *
 * Usage:
 * - Compile: csc example2-m3p4e2-data-optimization-demo.cs
 * - Or use Visual Studio / dotnet build
 * - Run: example2-m3p4e2-data-optimization-demo.exe
 * - Monitor with Wireshark on loopback interface (127.0.0.1)
 * - Toggle optimization mode with global variable OPTIMIZATION_MODE
 *
 * =====================================================================================
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DataOptimizationDemo
{
    // =====================================================================================
    // CONFIGURATION - EASY TOGGLE FOR CLASSROOM DEMONSTRATION
    // =====================================================================================

    public static class Config
    {
        // Optimization modes: 0=no optimization, 1=deduplication, 2=binary format, 3=compression, 4=all
        public static int OPTIMIZATION_MODE = 4;  // Set to 0-4 to test different optimization levels

        public const int SERVER_PORT = 8888;
        public const int BUFFER_SIZE = 65536;  // 64KB buffer
    }

    // =====================================================================================
    // DATA STRUCTURES FOR DEMONSTRATION
    // =====================================================================================

    public class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public double Salary { get; set; }
        public bool Active { get; set; }
    }

    // =====================================================================================
    // DEDUPLICATION SYSTEM
    // =====================================================================================

    public class DeduplicationManager
    {
        private Dictionary<string, int> stringToId = new Dictionary<string, int>();
        private List<string> idToString = new List<string>();
        private int nextId = 1;

        public int AddString(string str)
        {
            if (stringToId.ContainsKey(str))
            {
                return stringToId[str];  // Return existing ID
            }

            int id = nextId++;
            stringToId[str] = id;
            idToString.Add(str);
            return id;
        }

        public string GetString(int id)
        {
            if (id > 0 && id <= idToString.Count)
            {
                return idToString[id - 1];
            }
            return "";
        }

        public int GetDictionarySize()
        {
            return idToString.Count;
        }

        public long GetTotalDictionaryBytes()
        {
            return idToString.Sum(s => s.Length);
        }

        public List<string> GetAllStrings()
        {
            return new List<string>(idToString);
        }
    }

    // =====================================================================================
    // COMPRESSION UTILITIES
    // =====================================================================================

    public static class Compression
    {
        public static byte[] SimpleCompress(byte[] data)
        {
            if (data == null || data.Length == 0) return data;

            var compressed = new List<byte>();
            compressed.Capacity = data.Length / 2;

            int i = 0;
            while (i < data.Length)
            {
                byte current = data[i];
                int count = 1;

                // Count consecutive identical bytes
                while (i + count < data.Length && data[i + count] == current && count < 255)
                {
                    count++;
                }

                // If we have 3 or more consecutive bytes, use RLE compression
                if (count >= 3)
                {
                    compressed.Add(0xFF); // RLE marker
                    compressed.Add(current);
                    compressed.Add((byte)count);
                    i += count;
                }
                else
                {
                    // Store bytes as-is
                    for (int j = 0; j < count; j++)
                    {
                        compressed.Add(data[i + j]);
                    }
                    i += count;
                }
            }

            return compressed.ToArray();
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            var decompressed = new List<byte>();
            decompressed.Capacity = compressedData.Length * 2;

            int i = 0;
            while (i < compressedData.Length)
            {
                if (compressedData[i] == 0xFF && i + 2 < compressedData.Length)
                {
                    // RLE expansion
                    byte value = compressedData[i + 1];
                    byte count = compressedData[i + 2];

                    for (int j = 0; j < count; j++)
                    {
                        decompressed.Add(value);
                    }
                    i += 3;
                }
                else
                {
                    decompressed.Add(compressedData[i]);
                    i++;
                }
            }

            return decompressed.ToArray();
        }
    }

    // =====================================================================================
    // DATA FORMAT CONVERTERS
    // =====================================================================================

    public static class DataConverter
    {
        public static string UserDataToJSON(List<UserData> users)
        {
            var sb = new StringBuilder();
            sb.Append("{\"users\":[");

            for (int i = 0; i < users.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.Append($"\"id\":{users[i].Id},");
                sb.Append($"\"name\":\"{users[i].Name}\",");
                sb.Append($"\"email\":\"{users[i].Email}\",");
                sb.Append($"\"department\":\"{users[i].Department}\",");
                sb.Append($"\"salary\":{users[i].Salary},");
                sb.Append($"\"active\":{users[i].Active.ToString().ToLower()}");
                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        public static byte[] UserDataToBinary(List<UserData> users)
        {
            var binary = new List<byte>();

            // Header: number of users (4 bytes)
            binary.AddRange(BitConverter.GetBytes(users.Count));

            foreach (var user in users)
            {
                // ID (4 bytes)
                binary.AddRange(BitConverter.GetBytes(user.Id));

                // Name length + name
                binary.AddRange(BitConverter.GetBytes(user.Name.Length));
                binary.AddRange(Encoding.UTF8.GetBytes(user.Name));

                // Email length + email
                binary.AddRange(BitConverter.GetBytes(user.Email.Length));
                binary.AddRange(Encoding.UTF8.GetBytes(user.Email));

                // Department length + department
                binary.AddRange(BitConverter.GetBytes(user.Department.Length));
                binary.AddRange(Encoding.UTF8.GetBytes(user.Department));

                // Salary (8 bytes)
                binary.AddRange(BitConverter.GetBytes(user.Salary));

                // Active (1 byte)
                binary.Add((byte)(user.Active ? 1 : 0));
            }

            return binary.ToArray();
        }

        public static byte[] UserDataToOptimizedBinary(List<UserData> users, DeduplicationManager dedup)
        {
            var binary = new List<byte>();

            // Header: number of users (4 bytes)
            binary.AddRange(BitConverter.GetBytes(users.Count));

            foreach (var user in users)
            {
                // ID (4 bytes)
                binary.AddRange(BitConverter.GetBytes(user.Id));

                // Name ID (4 bytes)
                int nameId = dedup.AddString(user.Name);
                binary.AddRange(BitConverter.GetBytes(nameId));

                // Email ID (4 bytes)
                int emailId = dedup.AddString(user.Email);
                binary.AddRange(BitConverter.GetBytes(emailId));

                // Department ID (4 bytes)
                int deptId = dedup.AddString(user.Department);
                binary.AddRange(BitConverter.GetBytes(deptId));

                // Salary (8 bytes)
                binary.AddRange(BitConverter.GetBytes(user.Salary));

                // Active (1 byte)
                binary.Add((byte)(user.Active ? 1 : 0));
            }

            return binary.ToArray();
        }
    }

    // =====================================================================================
    // SERVER IMPLEMENTATION
    // =====================================================================================

    public class DataOptimizationServer
    {
        public void Run()
        {
            Console.WriteLine("\n=== DATA OPTIMIZATION DEMO SERVER ===");
            Console.WriteLine($"Mode: {Config.OPTIMIZATION_MODE} (0=None, 1=Dedup, 2=Binary, 3=Compress, 4=All)");
            Console.WriteLine($"Listening on port: {Config.SERVER_PORT}");
            Console.WriteLine($"Monitor with Wireshark on 127.0.0.1:{Config.SERVER_PORT}");
            Console.WriteLine("=====================================");

            try
            {
                var listener = new TcpListener(IPAddress.Any, Config.SERVER_PORT);
                listener.Start();

                Console.WriteLine("Server waiting for client connection...");

                using (var client = listener.AcceptTcpClient())
                {
                    Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");

                    using (var stream = client.GetStream())
                    {
                        var buffer = new byte[Config.BUFFER_SIZE];
                        int totalBytesReceived = 0;
                        int bytesReceived;

                        Console.WriteLine("\nReceiving data...");

                        while ((bytesReceived = stream.Read(buffer, totalBytesReceived,
                            Config.BUFFER_SIZE - totalBytesReceived)) > 0)
                        {
                            totalBytesReceived += bytesReceived;
                            Console.WriteLine($"Received {bytesReceived} bytes (total: {totalBytesReceived} bytes)");
                        }

                        Console.WriteLine("\n=== RECEPTION COMPLETE ===");
                        Console.WriteLine($"Total bytes received: {totalBytesReceived}");
                        Console.WriteLine($"Data size: {totalBytesReceived / 1024.0:F2} KB");

                        Console.WriteLine("\n=== OPTIMIZATION ANALYSIS ===");
                        switch (Config.OPTIMIZATION_MODE)
                        {
                            case 0:
                                Console.WriteLine("Mode: NO OPTIMIZATION");
                                Console.WriteLine("This is the baseline for comparison!");
                                break;
                            case 1:
                                Console.WriteLine("Mode: DEDUPLICATION");
                                Console.WriteLine("Redundant data has been eliminated!");
                                break;
                            case 2:
                                Console.WriteLine("Mode: BINARY FORMAT");
                                Console.WriteLine("Binary format is more efficient than text!");
                                break;
                            case 3:
                                Console.WriteLine("Mode: COMPRESSION");
                                Console.WriteLine("Data has been compressed!");
                                break;
                            case 4:
                                Console.WriteLine("Mode: ALL OPTIMIZATIONS");
                                Console.WriteLine("Maximum optimization applied!");
                                break;
                        }
                    }
                }

                listener.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }

    // =====================================================================================
    // CLIENT IMPLEMENTATION
    // =====================================================================================

    public class DataOptimizationClient
    {
        private static List<UserData> GenerateTestData()
        {
            var users = new List<UserData>();

            // Generate test data with some redundancy for deduplication demo
            var departments = new[] { "Engineering", "Marketing", "Sales", "HR", "Finance" };
            var domains = new[] { "company.com", "corp.net", "business.org" };

            for (int i = 1; i <= 100; i++)
            {
                users.Add(new UserData
                {
                    Id = i,
                    Name = $"User{i}",
                    Email = $"user{i}@{domains[i % domains.Length]}",
                    Department = departments[i % departments.Length],
                    Salary = 50000.0 + (i * 1000.0),
                    Active = (i % 10 != 0)
                });
            }

            return users;
        }

        public void Run()
        {
            Console.WriteLine("\n=== DATA OPTIMIZATION DEMO CLIENT ===");
            Console.WriteLine($"Mode: {Config.OPTIMIZATION_MODE} (0=None, 1=Dedup, 2=Binary, 3=Compress, 4=All)");
            Console.WriteLine($"Connecting to server on port: {Config.SERVER_PORT}");
            Console.WriteLine("====================================");

            // Generate test data
            var users = GenerateTestData();
            Console.WriteLine($"Generated {users.Count} user records");

            // Prepare data based on optimization mode
            byte[] dataToSend = null;
            int originalSize = 0;

            switch (Config.OPTIMIZATION_MODE)
            {
                case 0: // No optimization - JSON format
                    {
                        Console.WriteLine("\nUsing JSON format (no optimization)...");
                        string json = DataConverter.UserDataToJSON(users);
                        dataToSend = Encoding.UTF8.GetBytes(json);
                        originalSize = json.Length;
                        Console.WriteLine($"JSON size: {originalSize} bytes");
                        break;
                    }
                case 1: // Deduplication
                    {
                        Console.WriteLine("\nUsing deduplication optimization...");
                        var dedup = new DeduplicationManager();
                        byte[] binary = DataConverter.UserDataToOptimizedBinary(users, dedup);

                        // Calculate original size (JSON)
                        string json = DataConverter.UserDataToJSON(users);
                        originalSize = json.Length;

                        // Add dictionary to data
                        var dictionary = new List<byte>();
                        foreach (var str in dedup.GetAllStrings())
                        {
                            dictionary.AddRange(BitConverter.GetBytes(str.Length));
                            dictionary.AddRange(Encoding.UTF8.GetBytes(str));
                        }

                        // Combine dictionary + data
                        var combined = new List<byte>();
                        combined.AddRange(BitConverter.GetBytes(dictionary.Count));
                        combined.AddRange(dictionary);
                        combined.AddRange(binary);

                        dataToSend = combined.ToArray();

                        Console.WriteLine($"Original JSON size: {originalSize} bytes");
                        Console.WriteLine($"Optimized size: {dataToSend.Length} bytes");
                        Console.WriteLine($"Dictionary entries: {dedup.GetDictionarySize()}");
                        break;
                    }
                case 2: // Binary format
                    {
                        Console.WriteLine("\nUsing binary format optimization...");
                        string json = DataConverter.UserDataToJSON(users);
                        originalSize = json.Length;
                        dataToSend = DataConverter.UserDataToBinary(users);

                        Console.WriteLine($"JSON size: {originalSize} bytes");
                        Console.WriteLine($"Binary size: {dataToSend.Length} bytes");
                        break;
                    }
                case 3: // Compression
                    {
                        Console.WriteLine("\nUsing compression optimization...");
                        string json = DataConverter.UserDataToJSON(users);
                        originalSize = json.Length;
                        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                        dataToSend = Compression.SimpleCompress(jsonBytes);

                        Console.WriteLine($"Original size: {originalSize} bytes");
                        Console.WriteLine($"Compressed size: {dataToSend.Length} bytes");
                        break;
                    }
                case 4: // All optimizations
                    {
                        Console.WriteLine("\nUsing ALL optimizations...");
                        string json = DataConverter.UserDataToJSON(users);
                        originalSize = json.Length;

                        var dedup = new DeduplicationManager();
                        byte[] binary = DataConverter.UserDataToOptimizedBinary(users, dedup);

                        // Add dictionary
                        var dictionary = new List<byte>();
                        foreach (var str in dedup.GetAllStrings())
                        {
                            dictionary.AddRange(BitConverter.GetBytes(str.Length));
                            dictionary.AddRange(Encoding.UTF8.GetBytes(str));
                        }

                        // Combine and compress
                        var combined = new List<byte>();
                        combined.AddRange(BitConverter.GetBytes(dictionary.Count));
                        combined.AddRange(dictionary);
                        combined.AddRange(binary);

                        dataToSend = Compression.SimpleCompress(combined.ToArray());

                        Console.WriteLine($"Original JSON size: {originalSize} bytes");
                        Console.WriteLine($"Fully optimized size: {dataToSend.Length} bytes");
                        Console.WriteLine($"Dictionary entries: {dedup.GetDictionarySize()}");
                        break;
                    }
            }

            try
            {
                // Connect to server and send data
                using (var client = new TcpClient("127.0.0.1", Config.SERVER_PORT))
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("\nConnected to server. Sending data...");

                    // Send data in chunks
                    int totalSent = 0;
                    int chunkSize = 4096;  // 4KB chunks

                    while (totalSent < dataToSend.Length)
                    {
                        int remaining = dataToSend.Length - totalSent;
                        int currentChunk = Math.Min(chunkSize, remaining);

                        stream.Write(dataToSend, totalSent, currentChunk);
                        totalSent += currentChunk;

                        Console.WriteLine($"Sent {currentChunk} bytes (total: {totalSent} bytes)");
                    }

                    Console.WriteLine("\n=== TRANSMISSION COMPLETE ===");
                    Console.WriteLine($"Total bytes sent: {totalSent}");
                    Console.WriteLine($"Data size: {totalSent / 1024.0:F2} KB");

                    if (originalSize > 0)
                    {
                        double reduction = (1.0 - (double)totalSent / originalSize) * 100.0;
                        Console.WriteLine($"Size reduction: {reduction:F2}%");
                        Console.WriteLine($"Bytes saved: {originalSize - totalSent} bytes");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
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
            Console.WriteLine("                    DATA OPTIMIZATION DEMONSTRATION");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("This program demonstrates various data optimization techniques to reduce");
            Console.WriteLine("bandwidth usage and improve application performance.");
            Console.WriteLine();
            Console.WriteLine("EDUCATIONAL OBJECTIVES:");
            Console.WriteLine("- Show deduplication techniques to avoid redundant data");
            Console.WriteLine("- Demonstrate compact data formats (JSON vs Binary)");
            Console.WriteLine("- Illustrate payload optimization and header minimization");
            Console.WriteLine("- Compare different compression algorithms");
            Console.WriteLine("- Show benefits: lower bandwidth, faster loading, energy savings");
            Console.WriteLine();
            Console.WriteLine($"CURRENT MODE: {Config.OPTIMIZATION_MODE}");
            Console.WriteLine("To change mode, modify OPTIMIZATION_MODE variable in Config class");
            Console.WriteLine("  - Mode 0: No optimization (JSON baseline)");
            Console.WriteLine("  - Mode 1: Deduplication (eliminate redundant data)");
            Console.WriteLine("  - Mode 2: Binary format (more efficient than text)");
            Console.WriteLine("  - Mode 3: Compression (reduce data size)");
            Console.WriteLine("  - Mode 4: All optimizations combined");
            Console.WriteLine();
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("- Press ENTER to send a package");
            Console.WriteLine("- Type 'quit' and press ENTER to exit");
            Console.WriteLine("- Type 'mode' and press ENTER to cycle through optimization modes");
            Console.WriteLine();
            Console.WriteLine("WIRESHARK MONITORING:");
            Console.WriteLine("- Monitor loopback interface (127.0.0.1)");
            Console.WriteLine($"- Filter: tcp.port == {Config.SERVER_PORT}");
            Console.WriteLine("- Compare packet sizes between different optimization modes");
            Console.WriteLine("=====================================================================================");

            // Start server in a separate thread
            var serverThread = new Thread(() => new DataOptimizationServer().Run());
            serverThread.IsBackground = true;
            serverThread.Start();

            // Give server time to start
            Thread.Sleep(1000);

            string userInput;
            int packageCount = 0;

            while (true)
            {
                Console.Write($"\n>>> Press ENTER to send package #{packageCount + 1} (or type 'quit' to exit, 'mode' to cycle): ");
                userInput = Console.ReadLine();

                if (userInput == "quit" || userInput == "exit")
                {
                    Console.WriteLine("Exiting demonstration...");
                    break;
                }
                else if (userInput == "mode")
                {
                    Config.OPTIMIZATION_MODE = (Config.OPTIMIZATION_MODE + 1) % 5;
                    Console.WriteLine($"Mode changed to: {Config.OPTIMIZATION_MODE}");
                    Console.WriteLine("  - Mode 0: No optimization");
                    Console.WriteLine("  - Mode 1: Deduplication");
                    Console.WriteLine("  - Mode 2: Binary format");
                    Console.WriteLine("  - Mode 3: Compression");
                    Console.WriteLine("  - Mode 4: All optimizations");
                    continue;
                }
                else if (string.IsNullOrEmpty(userInput) || userInput == "send")
                {
                    packageCount++;
                    Console.WriteLine($"\n--- SENDING PACKAGE #{packageCount} ---");
                    new DataOptimizationClient().Run();
                    Console.WriteLine($"--- PACKAGE #{packageCount} COMPLETE ---");
                }
                else
                {
                    Console.WriteLine("Invalid command. Use ENTER to send, 'quit' to exit, or 'mode' to cycle.");
                }
            }

            Console.WriteLine("\n=====================================================================================");
            Console.WriteLine("DEMONSTRATION COMPLETE");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("SUMMARY:");
            Console.WriteLine($"Total packages sent: {packageCount}");
            Console.WriteLine($"Final mode: {Config.OPTIMIZATION_MODE}");
            Console.WriteLine();
            Console.WriteLine("KEY LEARNINGS:");
            Console.WriteLine("- Deduplication eliminates redundant data");
            Console.WriteLine("- Binary formats are more efficient than text formats");
            Console.WriteLine("- Compression can significantly reduce data size");
            Console.WriteLine("- Multiple optimization techniques can be combined");
            Console.WriteLine("- Optimized data reduces bandwidth usage and improves performance");
            Console.WriteLine("=====================================================================================");
        }
    }
}

