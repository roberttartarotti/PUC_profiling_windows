/*
 * =====================================================================================
 * HTTP HEADER OPTIMIZATION DEMONSTRATION - C# (MODULE 3, CLASS 4 - EXAMPLE 3)
 * =====================================================================================
 *
 * Purpose: Demonstrate HTTP header optimization techniques to reduce overhead
 *          and improve network efficiency
 *
 * Educational Context:
 * - Show how reducing header size minimizes extra data transmitted
 * - Demonstrate header compression (simulated HPACK-like algorithm)
 * - Illustrate removal of unnecessary headers to avoid overhead
 * - Compare HTTP/1.1 vs HTTP/2-style header compression
 * - Show conditional responses and caching techniques
 *
 * What this demonstrates:
 * - Headers add significant overhead to HTTP requests
 * - Header compression (HPACK) can reduce header size by 80%+
 * - Removing unnecessary headers improves efficiency
 * - Caching reduces repeated requests
 * - Header optimization accelerates request/response cycles
 *
 * Usage:
 * - Compile: csc example3-m3p4e3-header-optimization-demo.cs
 * - Or use Visual Studio / dotnet build
 * - Run: example3-m3p4e3-header-optimization-demo.exe
 * - Monitor with Wireshark on loopback interface (127.0.0.1)
 * - Toggle optimization mode with HEADER_MODE variable
 *
 * =====================================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HeaderOptimizationDemo
{
    // =====================================================================================
    // CONFIGURATION - EASY TOGGLE FOR CLASSROOM DEMONSTRATION
    // =====================================================================================

    public static class Config
    {
        // Header optimization modes:
        // 0 = Full headers (HTTP/1.1 style with all headers)
        // 1 = Minimal headers (remove unnecessary headers)
        // 2 = Compressed headers (HPACK-like compression)
        // 3 = Cached response (304 Not Modified)
        public static int HEADER_MODE = 0;  // Set to 0-3 to test different optimization levels

        public const int SERVER_PORT = 8888;
        public const int BUFFER_SIZE = 65536;  // 64KB buffer
    }

    // =====================================================================================
    // HEADER COMPRESSION (HPACK-LIKE SIMULATION)
    // =====================================================================================

    public class HeaderCompressor
    {
        private Dictionary<int, KeyValuePair<string, string>> staticTable;
        private List<KeyValuePair<string, string>> dynamicTable;
        private int nextIndex;

        public HeaderCompressor()
        {
            nextIndex = 62;
            staticTable = new Dictionary<int, KeyValuePair<string, string>>();
            dynamicTable = new List<KeyValuePair<string, string>>();

            // Initialize static table with common HTTP headers (simplified HPACK)
            staticTable[1] = new KeyValuePair<string, string>(":authority", "");
            staticTable[2] = new KeyValuePair<string, string>(":method", "GET");
            staticTable[3] = new KeyValuePair<string, string>(":method", "POST");
            staticTable[4] = new KeyValuePair<string, string>(":path", "/");
            staticTable[5] = new KeyValuePair<string, string>(":scheme", "http");
            staticTable[6] = new KeyValuePair<string, string>(":scheme", "https");
            staticTable[7] = new KeyValuePair<string, string>(":status", "200");
            staticTable[8] = new KeyValuePair<string, string>(":status", "204");
            staticTable[9] = new KeyValuePair<string, string>(":status", "206");
            staticTable[10] = new KeyValuePair<string, string>(":status", "304");
            staticTable[15] = new KeyValuePair<string, string>("accept-encoding", "gzip, deflate");
            staticTable[27] = new KeyValuePair<string, string>("content-length", "");
            staticTable[30] = new KeyValuePair<string, string>("content-type", "");
            staticTable[37] = new KeyValuePair<string, string>("host", "");
        }

        public byte[] CompressHeaders(Dictionary<string, string> headers)
        {
            var compressed = new List<byte>();

            foreach (var header in headers)
            {
                // Check if header is in static table
                int staticIndex = FindInStaticTable(header.Key, header.Value);

                if (staticIndex > 0)
                {
                    // Indexed header field (1 byte for common headers)
                    compressed.Add((byte)(0x80 | staticIndex));
                }
                else
                {
                    // Literal header field with incremental indexing
                    compressed.Add(0x40);  // Literal with indexing prefix

                    // Encode header name length
                    byte nameLen = (byte)header.Key.Length;
                    compressed.Add(nameLen);

                    // Encode header name
                    compressed.AddRange(Encoding.UTF8.GetBytes(header.Key));

                    // Encode header value length
                    byte valueLen = (byte)header.Value.Length;
                    compressed.Add(valueLen);

                    // Encode header value
                    compressed.AddRange(Encoding.UTF8.GetBytes(header.Value));

                    // Add to dynamic table
                    dynamicTable.Add(new KeyValuePair<string, string>(header.Key, header.Value));
                }
            }

            return compressed.ToArray();
        }

        private int FindInStaticTable(string name, string value)
        {
            foreach (var entry in staticTable)
            {
                if (entry.Value.Key == name)
                {
                    if (string.IsNullOrEmpty(entry.Value.Value) || entry.Value.Value == value)
                    {
                        return entry.Key;
                    }
                }
            }
            return -1;
        }
    }

    // =====================================================================================
    // HTTP MESSAGE BUILDERS
    // =====================================================================================

    public static class HttpMessageBuilder
    {
        public static string BuildFullHttpRequest()
        {
            var request = new StringBuilder();

            // Request line
            request.AppendLine("GET /api/users HTTP/1.1");

            // Full headers (typical browser request)
            request.AppendLine("Host: localhost:8890");
            request.AppendLine("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.AppendLine("Accept: application/json, text/plain, */*");
            request.AppendLine("Accept-Language: en-US,en;q=0.9,pt-BR;q=0.8,pt;q=0.7");
            request.AppendLine("Accept-Encoding: gzip, deflate, br");
            request.AppendLine("Connection: keep-alive");
            request.AppendLine("Cache-Control: no-cache");
            request.AppendLine("Pragma: no-cache");
            request.AppendLine("Sec-Fetch-Dest: empty");
            request.AppendLine("Sec-Fetch-Mode: cors");
            request.AppendLine("Sec-Fetch-Site: same-origin");
            request.AppendLine("Referer: http://localhost:8890/dashboard");
            request.AppendLine("Cookie: session_id=abc123def456; user_pref=dark_mode; analytics_id=xyz789");
            request.AppendLine("X-Requested-With: XMLHttpRequest");
            request.AppendLine("X-Client-Version: 1.2.3");
            request.AppendLine("X-Request-ID: 550e8400-e29b-41d4-a716-446655440000");
            request.AppendLine();

            return request.ToString();
        }

        public static string BuildMinimalHttpRequest()
        {
            var request = new StringBuilder();

            // Request line
            request.AppendLine("GET /api/users HTTP/1.1");

            // Minimal headers (only essential)
            request.AppendLine("Host: localhost:8890");
            request.AppendLine("Accept: application/json");
            request.AppendLine("Connection: keep-alive");
            request.AppendLine();

            return request.ToString();
        }

        public static string BuildFullHttpResponse()
        {
            var response = new StringBuilder();

            // Status line
            response.AppendLine("HTTP/1.1 200 OK");

            // Full headers
            response.AppendLine("Date: Mon, 27 Jan 2025 12:00:00 GMT");
            response.AppendLine("Server: Apache/2.4.41 (Ubuntu)");
            response.AppendLine("Content-Type: application/json; charset=utf-8");
            response.AppendLine("Content-Length: 150");
            response.AppendLine("Connection: keep-alive");
            response.AppendLine("Cache-Control: max-age=3600, public");
            response.AppendLine("ETag: \"33a64df551425fcc55e4d42a148795d9f25f89d4\"");
            response.AppendLine("Last-Modified: Mon, 27 Jan 2025 11:00:00 GMT");
            response.AppendLine("Vary: Accept-Encoding");
            response.AppendLine("X-Content-Type-Options: nosniff");
            response.AppendLine("X-Frame-Options: DENY");
            response.AppendLine("X-XSS-Protection: 1; mode=block");
            response.AppendLine("Strict-Transport-Security: max-age=31536000; includeSubDomains");
            response.AppendLine("Access-Control-Allow-Origin: *");
            response.AppendLine("X-Response-Time: 45ms");
            response.AppendLine("X-Request-ID: 550e8400-e29b-41d4-a716-446655440000");
            response.AppendLine();

            // Body
            response.Append("{\"users\":[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}],\"total\":2,\"page\":1}");

            return response.ToString();
        }

        public static string BuildMinimalHttpResponse()
        {
            var response = new StringBuilder();

            // Status line
            response.AppendLine("HTTP/1.1 200 OK");

            // Minimal headers
            response.AppendLine("Content-Type: application/json");
            response.AppendLine("Content-Length: 150");
            response.AppendLine();

            // Body
            response.Append("{\"users\":[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}],\"total\":2,\"page\":1}");

            return response.ToString();
        }

        public static string BuildCachedResponse()
        {
            var response = new StringBuilder();

            // Status line (304 Not Modified)
            response.AppendLine("HTTP/1.1 304 Not Modified");

            // Minimal headers for cached response
            response.AppendLine("Date: Mon, 27 Jan 2025 12:00:00 GMT");
            response.AppendLine("ETag: \"33a64df551425fcc55e4d42a148795d9f25f89d4\"");
            response.AppendLine("Cache-Control: max-age=3600, public");
            response.AppendLine();

            // No body (client uses cached version)

            return response.ToString();
        }
    }

    // =====================================================================================
    // SERVER IMPLEMENTATION
    // =====================================================================================

    public class HttpHeaderServer
    {
        public void Run()
        {
            Console.WriteLine("\n=== HTTP HEADER OPTIMIZATION DEMO SERVER ===");
            Console.WriteLine($"Mode: {Config.HEADER_MODE} (0=Full, 1=Minimal, 2=Compressed, 3=Cached)");
            Console.WriteLine($"Listening on port: {Config.SERVER_PORT}");
            Console.WriteLine($"Monitor with Wireshark on 127.0.0.1:{Config.SERVER_PORT}");
            Console.WriteLine("============================================");

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
                        int bytesReceived = stream.Read(buffer, 0, Config.BUFFER_SIZE);

                        if (bytesReceived > 0)
                        {
                            Console.WriteLine("\n=== REQUEST RECEIVED ===");
                            Console.WriteLine($"Request size: {bytesReceived} bytes");

                            // Send response based on mode
                            string response;

                            switch (Config.HEADER_MODE)
                            {
                                case 0:
                                    response = HttpMessageBuilder.BuildFullHttpResponse();
                                    Console.WriteLine("Sending: Full HTTP response");
                                    break;
                                case 1:
                                    response = HttpMessageBuilder.BuildMinimalHttpResponse();
                                    Console.WriteLine("Sending: Minimal HTTP response");
                                    break;
                                case 2:
                                    {
                                        // Compressed headers
                                        var compressor = new HeaderCompressor();
                                        var headers = new Dictionary<string, string>
                                        {
                                            ["content-type"] = "application/json",
                                            ["content-length"] = "150"
                                        };
                                        byte[] compressed = compressor.CompressHeaders(headers);

                                        var resp = new StringBuilder();
                                        resp.AppendLine("HTTP/2 200");
                                        resp.AppendLine($"[COMPRESSED HEADERS: {compressed.Length} bytes]");
                                        resp.AppendLine();
                                        resp.Append("{\"users\":[{\"id\":1,\"name\":\"John\"},{\"id\":2,\"name\":\"Jane\"}],\"total\":2,\"page\":1}");
                                        response = resp.ToString();
                                        Console.WriteLine("Sending: Compressed headers response (HTTP/2 style)");
                                        break;
                                    }
                                case 3:
                                    response = HttpMessageBuilder.BuildCachedResponse();
                                    Console.WriteLine("Sending: Cached response (304 Not Modified)");
                                    break;
                                default:
                                    response = HttpMessageBuilder.BuildFullHttpResponse();
                                    break;
                            }

                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseBytes, 0, responseBytes.Length);

                            Console.WriteLine($"Response size: {responseBytes.Length} bytes");
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

    public class HttpHeaderClient
    {
        public void Run()
        {
            Console.WriteLine("\n=== HTTP HEADER OPTIMIZATION DEMO CLIENT ===");
            Console.WriteLine($"Mode: {Config.HEADER_MODE} (0=Full, 1=Minimal, 2=Compressed, 3=Cached)");
            Console.WriteLine($"Connecting to server on port: {Config.SERVER_PORT}");
            Console.WriteLine("===========================================");

            // Build request based on mode
            string request;

            switch (Config.HEADER_MODE)
            {
                case 0:
                    request = HttpMessageBuilder.BuildFullHttpRequest();
                    Console.WriteLine("\nSending: Full HTTP request (typical browser)");
                    break;
                case 1:
                    request = HttpMessageBuilder.BuildMinimalHttpRequest();
                    Console.WriteLine("\nSending: Minimal HTTP request (only essential headers)");
                    break;
                case 2:
                    {
                        // Compressed headers
                        var compressor = new HeaderCompressor();
                        var headers = new Dictionary<string, string>
                        {
                            ["host"] = "localhost:8890",
                            ["accept"] = "application/json"
                        };
                        byte[] compressed = compressor.CompressHeaders(headers);

                        var req = new StringBuilder();
                        req.AppendLine("GET /api/users HTTP/2");
                        req.AppendLine($"[COMPRESSED HEADERS: {compressed.Length} bytes]");
                        req.AppendLine();
                        request = req.ToString();
                        Console.WriteLine("\nSending: Compressed headers request (HTTP/2 style)");
                        break;
                    }
                case 3:
                    {
                        // Conditional request with If-None-Match
                        var req = new StringBuilder();
                        req.AppendLine("GET /api/users HTTP/1.1");
                        req.AppendLine("Host: localhost:8890");
                        req.AppendLine("If-None-Match: \"33a64df551425fcc55e4d42a148795d9f25f89d4\"");
                        req.AppendLine("If-Modified-Since: Mon, 27 Jan 2025 11:00:00 GMT");
                        req.AppendLine();
                        request = req.ToString();
                        Console.WriteLine("\nSending: Conditional request (with cache validators)");
                        break;
                    }
                default:
                    request = HttpMessageBuilder.BuildFullHttpRequest();
                    break;
            }

            Console.WriteLine($"Request size: {request.Length} bytes");

            try
            {
                // Connect to server and send request
                using (var client = new TcpClient("127.0.0.1", Config.SERVER_PORT))
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("\nConnected to server. Sending request...");

                    byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    Console.WriteLine($"Request sent: {requestBytes.Length} bytes");

                    // Receive response
                    var buffer = new byte[Config.BUFFER_SIZE];
                    int bytesReceived = stream.Read(buffer, 0, Config.BUFFER_SIZE);

                    if (bytesReceived > 0)
                    {
                        Console.WriteLine("\n=== RESPONSE RECEIVED ===");
                        Console.WriteLine($"Response size: {bytesReceived} bytes");
                    }
                }

                // Analysis
                Console.WriteLine("\n=== HEADER OPTIMIZATION ANALYSIS ===");
                switch (Config.HEADER_MODE)
                {
                    case 0:
                        Console.WriteLine("Mode: FULL HEADERS (HTTP/1.1)");
                        Console.WriteLine("- Typical browser request with all headers");
                        Console.WriteLine("- High overhead from verbose headers");
                        Console.WriteLine("- Baseline for comparison");
                        break;
                    case 1:
                        Console.WriteLine("Mode: MINIMAL HEADERS");
                        Console.WriteLine("- Only essential headers included");
                        Console.WriteLine("- Removed unnecessary headers");
                        Console.WriteLine("- Reduced overhead significantly");
                        break;
                    case 2:
                        Console.WriteLine("Mode: COMPRESSED HEADERS (HTTP/2 HPACK)");
                        Console.WriteLine("- Headers compressed using HPACK-like algorithm");
                        Console.WriteLine("- Static table for common headers");
                        Console.WriteLine("- 80%+ reduction in header size");
                        break;
                    case 3:
                        Console.WriteLine("Mode: CACHED RESPONSE (304 Not Modified)");
                        Console.WriteLine("- Conditional request with cache validators");
                        Console.WriteLine("- Server returns 304 without body");
                        Console.WriteLine("- Client uses cached version");
                        Console.WriteLine("- Massive bandwidth savings");
                        break;
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
            Console.WriteLine("                    HTTP HEADER OPTIMIZATION DEMONSTRATION");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("This program demonstrates HTTP header optimization techniques to reduce");
            Console.WriteLine("overhead and improve network efficiency.");
            Console.WriteLine();
            Console.WriteLine("EDUCATIONAL OBJECTIVES:");
            Console.WriteLine("- Show how reducing header size minimizes extra data transmitted");
            Console.WriteLine("- Demonstrate header compression (HPACK-like algorithm)");
            Console.WriteLine("- Illustrate removal of unnecessary headers to avoid overhead");
            Console.WriteLine("- Compare HTTP/1.1 vs HTTP/2-style header compression");
            Console.WriteLine("- Show conditional responses and caching techniques");
            Console.WriteLine();
            Console.WriteLine($"CURRENT MODE: {Config.HEADER_MODE}");
            Console.WriteLine("To change mode, modify HEADER_MODE variable in Config class");
            Console.WriteLine("  - Mode 0: Full headers (HTTP/1.1 with all headers)");
            Console.WriteLine("  - Mode 1: Minimal headers (remove unnecessary)");
            Console.WriteLine("  - Mode 2: Compressed headers (HPACK-like)");
            Console.WriteLine("  - Mode 3: Cached response (304 Not Modified)");
            Console.WriteLine();
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("- Press ENTER to send a request");
            Console.WriteLine("- Type 'quit' and press ENTER to exit");
            Console.WriteLine("- Type 'mode' and press ENTER to cycle through optimization modes");
            Console.WriteLine();
            Console.WriteLine("WIRESHARK MONITORING:");
            Console.WriteLine("- Monitor loopback interface (127.0.0.1)");
            Console.WriteLine($"- Filter: tcp.port == {Config.SERVER_PORT}");
            Console.WriteLine("- Compare packet sizes between different header optimization modes");
            Console.WriteLine("- Analyze header overhead in each mode");
            Console.WriteLine("=====================================================================================");

            // Start server in a separate thread
            var serverThread = new Thread(() => new HttpHeaderServer().Run());
            serverThread.IsBackground = true;
            serverThread.Start();

            // Give server time to start
            Thread.Sleep(1000);

            string userInput;
            int requestCount = 0;

            while (true)
            {
                Console.Write($"\n>>> Press ENTER to send request #{requestCount + 1} (or type 'quit' to exit, 'mode' to cycle): ");
                userInput = Console.ReadLine();

                if (userInput == "quit" || userInput == "exit")
                {
                    Console.WriteLine("Exiting demonstration...");
                    break;
                }
                else if (userInput == "mode")
                {
                    Config.HEADER_MODE = (Config.HEADER_MODE + 1) % 4;
                    Console.WriteLine($"Mode changed to: {Config.HEADER_MODE}");
                    Console.WriteLine("  - Mode 0: Full headers");
                    Console.WriteLine("  - Mode 1: Minimal headers");
                    Console.WriteLine("  - Mode 2: Compressed headers");
                    Console.WriteLine("  - Mode 3: Cached response");
                    continue;
                }
                else if (string.IsNullOrEmpty(userInput) || userInput == "send")
                {
                    requestCount++;
                    Console.WriteLine($"\n--- SENDING REQUEST #{requestCount} ---");
                    new HttpHeaderClient().Run();
                    Console.WriteLine($"--- REQUEST #{requestCount} COMPLETE ---");
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
            Console.WriteLine($"Total requests sent: {requestCount}");
            Console.WriteLine($"Final mode: {Config.HEADER_MODE}");
            Console.WriteLine();
            Console.WriteLine("KEY LEARNINGS:");
            Console.WriteLine("- HTTP headers add significant overhead to requests/responses");
            Console.WriteLine("- Removing unnecessary headers reduces bandwidth usage");
            Console.WriteLine("- Header compression (HPACK) can reduce header size by 80%+");
            Console.WriteLine("- Caching with conditional requests eliminates redundant data transfer");
            Console.WriteLine("- HTTP/2 header compression is much more efficient than HTTP/1.1");
            Console.WriteLine("- Header optimization accelerates request/response cycles");
            Console.WriteLine("=====================================================================================");
        }
    }
}

