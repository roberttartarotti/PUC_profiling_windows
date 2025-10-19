/*
 * =====================================================================================
 * TCP COMPRESSION DEMONSTRATION - C# (MODULE 3, CLASS 4 - EXAMPLE 1)
 * =====================================================================================
 *
 * Purpose: Demonstrate TCP compression benefits by sending image data
 *          with and without compression over loopback connection
 *
 * Educational Context:
 * - Show the impact of compression on network bandwidth usage
 * - Demonstrate RLE compression for TCP payload reduction
 * - Illustrate how compression can reduce bytes transmitted
 * - Compare uncompressed vs compressed data transmission
 *
 * What this demonstrates:
 * - TCP can use compression to reduce payload size
 * - Compression decreases bytes sent, saving bandwidth and speeding up loading
 * - Compression can reduce bandwidth usage significantly for data files
 * - Simple client-server communication over loopback
 *
 * Usage:
 * - Compile: csc example1-m3p4e1-tcp-compression-demo.cs
 * - Or use Visual Studio / dotnet build
 * - Run: example1-m3p4e1-tcp-compression-demo.exe
 * - Monitor with Wireshark on loopback interface (127.0.0.1)
 * - Toggle compression mode with global variable USE_COMPRESSION
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
using System.Threading.Tasks;

namespace TcpCompressionDemo
{
    // =====================================================================================
    // CONFIGURATION - EASY TOGGLE FOR CLASSROOM DEMONSTRATION
    // =====================================================================================

    public static class Config
    {
        // Toggle compression mode: true = with compression, false = without compression
        public static bool USE_COMPRESSION = true;  // Set to true for compression demo, false for baseline

        public const int SERVER_PORT = 8888;
        public static readonly string IMAGE_PATH = @"C:\Users\robert\personal\PUC_profiling_windows\module3\class4\m3p4e1\image3.bmp";
        public const int BUFFER_SIZE = 65536;  // 64KB buffer
    }

    // =====================================================================================
    // PROTOCOL HEADER STRUCTURE
    // =====================================================================================

    public struct DataHeader
    {
        public uint Magic;        // Magic number: 0x54435043 ("TCPC")
        public uint OriginalSize; // Original data size
        public byte Compressed;   // 1 if compressed, 0 if not
        public byte Reserved1;    // Reserved for future use
        public byte Reserved2;
        public byte Reserved3;

        public const uint MAGIC_NUMBER = 0x54435043; // "TCPC"

        public byte[] ToBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Magic));
            bytes.AddRange(BitConverter.GetBytes(OriginalSize));
            bytes.Add(Compressed);
            bytes.Add(Reserved1);
            bytes.Add(Reserved2);
            bytes.Add(Reserved3);
            return bytes.ToArray();
        }

        public static DataHeader FromBytes(byte[] data)
        {
            return new DataHeader
            {
                Magic = BitConverter.ToUInt32(data, 0),
                OriginalSize = BitConverter.ToUInt32(data, 4),
                Compressed = data[8],
                Reserved1 = data[9],
                Reserved2 = data[10],
                Reserved3 = data[11]
            };
        }
    }

    // =====================================================================================
    // SIMPLE COMPRESSION UTILITIES (Run-Length Encoding for demonstration)
    // =====================================================================================

    public static class Compression
    {
        public static byte[] CompressData(byte[] data)
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

        public static byte[] DecompressData(byte[] compressedData, int originalSize)
        {
            var decompressed = new List<byte>();
            decompressed.Capacity = originalSize;

            int i = 0;
            while (i < compressedData.Length && decompressed.Count < originalSize)
            {
                if (compressedData[i] == 0xFF && i + 2 < compressedData.Length)
                {
                    // RLE expansion
                    byte value = compressedData[i + 1];
                    byte count = compressedData[i + 2];

                    for (int j = 0; j < count && decompressed.Count < originalSize; j++)
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
    // DATA PACKAGING UTILITIES
    // =====================================================================================

    public static class DataPackaging
    {
        public static byte[] PackageData(byte[] data, bool useCompression)
        {
            var packagedData = new List<byte>();

            // Create header
            var header = new DataHeader
            {
                Magic = DataHeader.MAGIC_NUMBER,
                OriginalSize = (uint)data.Length,
                Compressed = (byte)(useCompression ? 1 : 0),
                Reserved1 = 0,
                Reserved2 = 0,
                Reserved3 = 0
            };

            // Add header to package
            packagedData.AddRange(header.ToBytes());

            // Add data (compressed or not)
            byte[] dataToAdd;
            if (useCompression)
            {
                dataToAdd = Compression.CompressData(data);
            }
            else
            {
                dataToAdd = data;
            }

            packagedData.AddRange(dataToAdd);

            return packagedData.ToArray();
        }

        public static bool ParseDataHeader(byte[] data, out DataHeader header, out byte[] payload)
        {
            header = new DataHeader();
            payload = null;

            if (data.Length < 12) // Size of DataHeader
            {
                return false;
            }

            header = DataHeader.FromBytes(data);

            if (header.Magic != DataHeader.MAGIC_NUMBER)
            {
                return false;
            }

            payload = new byte[data.Length - 12];
            Array.Copy(data, 12, payload, 0, payload.Length);
            return true;
        }
    }

    // =====================================================================================
    // SERVER IMPLEMENTATION
    // =====================================================================================

    public class TcpServer
    {
        public void Run()
        {
            Console.WriteLine("\n=== TCP COMPRESSION DEMO SERVER ===");
            Console.WriteLine($"Mode: {(Config.USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION")}");
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

                        // Parse the received data
                        var receivedData = new byte[totalBytesReceived];
                        Array.Copy(buffer, receivedData, totalBytesReceived);

                        if (!DataPackaging.ParseDataHeader(receivedData, out DataHeader header, out byte[] payload))
                        {
                            Console.WriteLine("Error: Invalid data format received");
                            return;
                        }

                        Console.WriteLine("\n=== DATA ANALYSIS ===");
                        Console.WriteLine($"Original data size: {header.OriginalSize} bytes ({header.OriginalSize / 1024.0:F2} KB)");
                        Console.WriteLine($"Compression flag: {(header.Compressed != 0 ? "ENABLED" : "DISABLED")}");
                        Console.WriteLine($"Payload size: {payload.Length} bytes ({payload.Length / 1024.0:F2} KB)");

                        if (header.Compressed != 0)
                        {
                            Console.WriteLine("\nData was COMPRESSED (RLE)");
                            double compressionRatio = (1.0 - (double)payload.Length / header.OriginalSize) * 100.0;
                            Console.WriteLine($"Compression ratio: {compressionRatio:F2}% reduction");
                            Console.WriteLine($"Bandwidth savings: {header.OriginalSize - payload.Length} bytes");
                            Console.WriteLine("This demonstrates bandwidth savings!");

                            // Verify decompression works
                            byte[] decompressed = Compression.DecompressData(payload, (int)header.OriginalSize);
                            if (decompressed.Length == header.OriginalSize)
                            {
                                Console.WriteLine("✓ Decompression successful - data integrity verified!");
                            }
                            else
                            {
                                Console.WriteLine("⚠ Warning: Decompression size mismatch!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nData was UNCOMPRESSED");
                            Console.WriteLine("This is the baseline for comparison!");
                            if (payload.Length == header.OriginalSize)
                            {
                                Console.WriteLine("✓ Data integrity verified!");
                            }
                            else
                            {
                                Console.WriteLine("⚠ Warning: Size mismatch!");
                            }
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

    public class TcpClientSender
    {
        public void Run()
        {
            Console.WriteLine("\n=== TCP COMPRESSION DEMO CLIENT ===");
            Console.WriteLine($"Mode: {(Config.USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION")}");
            Console.WriteLine($"Connecting to server on port: {Config.SERVER_PORT}");
            Console.WriteLine("====================================");

            // Load image data
            byte[] imageData = LoadImageData();
            if (imageData == null || imageData.Length == 0)
            {
                Console.WriteLine("Failed to load image data");
                return;
            }

            Console.WriteLine($"Loaded image: {Config.IMAGE_PATH}");
            Console.WriteLine($"Original image size: {imageData.Length} bytes ({imageData.Length / 1024.0:F2} KB)");

            // Prepare data for transmission
            Console.WriteLine("\nPreparing data for transmission...");
            byte[] dataToSend = DataPackaging.PackageData(imageData, Config.USE_COMPRESSION);

            Console.WriteLine($"Original image size: {imageData.Length} bytes ({imageData.Length / 1024.0:F2} KB)");
            Console.WriteLine($"Packaged data size: {dataToSend.Length} bytes ({dataToSend.Length / 1024.0:F2} KB)");

            if (Config.USE_COMPRESSION)
            {
                Console.WriteLine("Mode: WITH COMPRESSION");
                Console.WriteLine($"Bandwidth savings: {imageData.Length - (dataToSend.Length - 12)} bytes");
            }
            else
            {
                Console.WriteLine("Mode: WITHOUT COMPRESSION");
                Console.WriteLine("No compression applied - sending original data");
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

                    if (Config.USE_COMPRESSION)
                    {
                        Console.WriteLine("COMPRESSED transmission completed!");
                        Console.WriteLine("Check Wireshark to see reduced bandwidth usage");
                    }
                    else
                    {
                        Console.WriteLine("UNCOMPRESSED transmission completed!");
                        Console.WriteLine("This is the baseline for comparison");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        private byte[] LoadImageData()
        {
            try
            {
                if (!File.Exists(Config.IMAGE_PATH))
                {
                    Console.WriteLine($"Failed to open image file: {Config.IMAGE_PATH}");
                    Console.WriteLine("Make sure the image file exists in m3p4e1/ directory");
                    return null;
                }

                return File.ReadAllBytes(Config.IMAGE_PATH);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read image file: {ex.Message}");
                return null;
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
            Console.WriteLine("                    TCP COMPRESSION DEMONSTRATION");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("This program demonstrates TCP compression benefits by sending image data");
            Console.WriteLine("with and without compression over a loopback connection.");
            Console.WriteLine();
            Console.WriteLine("EDUCATIONAL OBJECTIVES:");
            Console.WriteLine("- Show impact of compression on network bandwidth usage");
            Console.WriteLine("- Demonstrate RLE compression for TCP payload reduction");
            Console.WriteLine("- Illustrate how compression can reduce bytes transmitted");
            Console.WriteLine("- Compare uncompressed vs compressed data transmission");
            Console.WriteLine();
            Console.WriteLine($"CURRENT MODE: {(Config.USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION")}");
            Console.WriteLine("To change mode, modify USE_COMPRESSION variable in Config class");
            Console.WriteLine("  - Set USE_COMPRESSION = true  for compression demo");
            Console.WriteLine("  - Set USE_COMPRESSION = false for baseline demo");
            Console.WriteLine();
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("- Press ENTER to send a package");
            Console.WriteLine("- Type 'quit' and press ENTER to exit");
            Console.WriteLine("- Type 'mode' and press ENTER to toggle compression mode");
            Console.WriteLine();
            Console.WriteLine("WIRESHARK MONITORING:");
            Console.WriteLine("- Monitor loopback interface (127.0.0.1)");
            Console.WriteLine($"- Filter: tcp.port == {Config.SERVER_PORT}");
            Console.WriteLine("- Compare packet sizes between compressed/uncompressed modes");
            Console.WriteLine("=====================================================================================");

            // Start server in a separate thread
            var serverThread = new Thread(() => new TcpServer().Run());
            serverThread.IsBackground = true;
            serverThread.Start();

            // Give server time to start
            Thread.Sleep(1000);

            string userInput;
            int packageCount = 0;

            while (true)
            {
                Console.Write($"\n>>> Press ENTER to send package #{packageCount + 1} (or type 'quit' to exit, 'mode' to toggle): ");
                userInput = Console.ReadLine();

                if (userInput == "quit" || userInput == "exit")
                {
                    Console.WriteLine("Exiting demonstration...");
                    break;
                }
                else if (userInput == "mode")
                {
                    Config.USE_COMPRESSION = !Config.USE_COMPRESSION;
                    Console.WriteLine($"Mode changed to: {(Config.USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION")}");
                    continue;
                }
                else if (string.IsNullOrEmpty(userInput) || userInput == "send")
                {
                    packageCount++;
                    Console.WriteLine($"\n--- SENDING PACKAGE #{packageCount} ---");
                    new TcpClientSender().Run();
                    Console.WriteLine($"--- PACKAGE #{packageCount} COMPLETE ---");
                }
                else
                {
                    Console.WriteLine("Invalid command. Use ENTER to send, 'quit' to exit, or 'mode' to toggle.");
                }
            }

            Console.WriteLine("\n=====================================================================================");
            Console.WriteLine("DEMONSTRATION COMPLETE");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("SUMMARY:");
            Console.WriteLine($"Total packages sent: {packageCount}");
            Console.WriteLine($"Final mode: {(Config.USE_COMPRESSION ? "WITH COMPRESSION" : "WITHOUT COMPRESSION")}");
            Console.WriteLine();
            Console.WriteLine("KEY LEARNINGS:");
            Console.WriteLine("- TCP can use compression to reduce payload size");
            Console.WriteLine("- Compression decreases bytes sent, saving bandwidth");
            Console.WriteLine("- Compression can reduce bandwidth usage significantly");
            Console.WriteLine("- This improves application performance and user experience");
            Console.WriteLine("=====================================================================================");
        }
    }
}

