using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace NetworkIssuesDemo
{
    /// <summary>
    /// Network Issues Demonstration for PUC Profiling Course - Module 3, Class 3, Example 1
    /// 
    /// IMPORTANT NOTE FOR PROFESSORS:
    /// 
    /// This demonstration shows network programming issues that cause performance problems.
    /// To see these issues in Windows PerfMon counters, you need to:
    /// 
    /// OPTION 1 (RECOMMENDED): Use this code with external network simulation tools:
    ///   - Windows: Install "Clumsy" (free tool) to add packet loss/latency to real traffic
    ///   - Or use "NetLimiter" to throttle bandwidth and create queue buildup
    ///   - Run this code connecting to actual IP (not localhost) while tool is active
    /// 
    /// OPTION 2: Deploy server and client on separate physical machines
    ///   - Run server on one machine (use actual network IP)
    ///   - Run clients from other machines on same network
    ///   - This will show real network adapter activity in PerfMon
    /// 
    /// WHAT THIS CODE DEMONSTRATES (visible in console and behavior):
    ///   - Connection failures due to aggressive reconnection attempts
    ///   - Throughput degradation from small buffers and poor socket configuration  
    ///   - Latency issues from blocking operations and buffer starvation
    ///   - Queue buildup simulation through buffer management
    /// 
    /// Monitor Windows PerfMon counters:
    ///   - TCPv4: Connection Failures (increases with aggressive reconnects)
    ///   - TCPv4: Segments Retransmitted/sec (use with Clumsy packet loss)
    ///   - Network Interface: Bytes Sent/Received per sec (use real network IP)
    ///   - Network Interface: Output Queue Length (use with bandwidth limiting)
    /// </summary>
    class Program
    {
        // ===== CONFIGURATION =====
        // Set to false to run the PROBLEM version (with network issues)
        // Set to true to run the SOLVED version (optimized network handling)
        private static readonly bool USE_SOLVED_VERSION = false;
        
        // Network configuration
        // CRITICAL: YOU MUST CHANGE THIS TO YOUR ACTUAL IP FOR PERFMON TO SHOW TRAFFIC!
        // Run "ipconfig" in cmd to find your IP address
        // Localhost (127.0.0.1) will NOT appear in PerfMon Network Interface counters!
        // Example: "192.168.1.100" or "10.0.0.5"
        private static readonly string SERVER_IP = GetLocalIPAddress();  // Automatically gets your IP!
        private static readonly int SERVER_PORT = 8888;
        
        // Traffic generation parameters
        private static readonly int CHUNK_SIZE = 8192;  // 8KB chunks
        private static readonly int CONCURRENT_CONNECTIONS = USE_SOLVED_VERSION ? 5 : 25;
        private static readonly int DATA_PER_CYCLE = 1024 * 1024;  // 1MB per cycle
        
        // Problem simulation parameters (only when USE_SOLVED_VERSION = false)
        private static readonly int PROBLEM_SEND_BUFFER = 4096;  // Tiny 4KB buffer causes issues
        private static readonly int PROBLEM_RECEIVE_BUFFER = 4096;
        private static readonly int PROBLEM_DELAY_MS = 200;  // Blocking delays
        private static readonly double CONNECTION_ABORT_RATE = 0.15;  // 15% random disconnects
        private static readonly int RECONNECT_DELAY_MS = 50;  // Aggressive reconnects
        
        // Solution parameters (only when USE_SOLVED_VERSION = true)
        private static readonly int SOLVED_SEND_BUFFER = 65536;  // Proper 64KB buffer
        private static readonly int SOLVED_RECEIVE_BUFFER = 65536;
        private static readonly int SOLVED_RECONNECT_DELAY_MS = 2000;  // Graceful reconnects
        
        // Statistics
        private static long TotalBytesSent = 0;
        private static long TotalBytesReceived = 0;
        private static int ActiveConnections = 0;
        private static int ConnectionFailures = 0;
        private static int ConnectionAttempts = 0;
        private static int AbruptDisconnects = 0;
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Network Issues Demonstration - PerfMon Monitoring Guide   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"Mode: {(USE_SOLVED_VERSION ? "✓ SOLVED VERSION" : "✗ PROBLEM VERSION")}");
            Console.WriteLine($"Server will bind to: {SERVER_IP}:{SERVER_PORT}");
            Console.WriteLine();
            
            // Display local IP addresses
            Console.WriteLine("📍 Available IP Addresses on this machine:");
            ShowLocalIPAddresses();
            Console.WriteLine();
            
            if (SERVER_IP == "127.0.0.1" || SERVER_IP == "localhost")
            {
                Console.WriteLine("⚠️  WARNING: Using loopback address (127.0.0.1)");
                Console.WriteLine("   Network Interface counters in PerfMon will NOT show this traffic!");
                Console.WriteLine("   To see traffic in PerfMon:");
                Console.WriteLine("   1. Change SERVER_IP to your actual IP address (shown above)");
                Console.WriteLine("   2. Or run server/clients on separate machines");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("✓ Using actual network IP - traffic will appear in PerfMon!");
                Console.WriteLine();
            }
            
            if (!USE_SOLVED_VERSION)
            {
                Console.WriteLine("═══ PROBLEM VERSION - Network Performance Issues ═══");
                Console.WriteLine($"• {CONCURRENT_CONNECTIONS} concurrent connections (excessive)");
                Console.WriteLine($"• {PROBLEM_SEND_BUFFER} byte send buffer (too small - causes blocking)");
                Console.WriteLine($"• {PROBLEM_DELAY_MS}ms artificial delays (causes timeouts)");
                Console.WriteLine($"• {CONNECTION_ABORT_RATE:P0} connection abort rate (causes failures)");
                Console.WriteLine($"• {RECONNECT_DELAY_MS}ms reconnect delay (too aggressive)");
                Console.WriteLine("• NoDelay=false (Nagle's algorithm enabled - adds latency)");
                Console.WriteLine();
                Console.WriteLine("Expected PerfMon observations:");
                Console.WriteLine("  ✗ TCPv4: Connection Failures - will increase constantly");
                Console.WriteLine("  ✗ TCPv4: Connections Established - rapid fluctuations");
                Console.WriteLine("  ✗ Network: Bytes Sent/Received - inconsistent throughput");
                Console.WriteLine("  ✗ Poor overall performance with connection churn");
            }
            else
            {
                Console.WriteLine("═══ SOLVED VERSION - Optimized Network Handling ═══");
                Console.WriteLine($"• {CONCURRENT_CONNECTIONS} concurrent connections (reasonable)");
                Console.WriteLine($"• {SOLVED_SEND_BUFFER} byte send buffer (properly sized)");
                Console.WriteLine("• No artificial delays");
                Console.WriteLine("• Minimal connection aborts");
                Console.WriteLine($"• {SOLVED_RECONNECT_DELAY_MS}ms reconnect delay (graceful)");
                Console.WriteLine("• NoDelay=true (Nagle disabled - low latency)");
                Console.WriteLine();
                Console.WriteLine("Expected PerfMon observations:");
                Console.WriteLine("  ✓ TCPv4: Connection Failures - minimal/none");
                Console.WriteLine("  ✓ TCPv4: Connections Established - stable");
                Console.WriteLine("  ✓ Network: Bytes Sent/Received - consistent throughput");
                Console.WriteLine("  ✓ Smooth, reliable performance");
            }
            
            Console.WriteLine();
            Console.WriteLine("═══ Windows Performance Monitor Setup ═══");
            Console.WriteLine("1. Open PerfMon: Press Win+R, type 'perfmon', press Enter");
            Console.WriteLine("2. Click the green '+' button to add counters");
            Console.WriteLine("3. Add these counters:");
            Console.WriteLine("   • TCPv4 → Connection Failures");
            Console.WriteLine("   • TCPv4 → Connections Established");
            Console.WriteLine("   • TCPv4 → Segments Retransmitted/sec (use with Clumsy tool)");
            Console.WriteLine("   • Network Interface → Bytes Total/sec (select your adapter)");
            Console.WriteLine("   • Network Interface → Output Queue Length (select your adapter)");
            Console.WriteLine();
            Console.WriteLine("💡 TIP: For Segments Retransmitted/sec, download 'Clumsy' tool:");
            Console.WriteLine("   https://jagt.github.io/clumsy/");
            Console.WriteLine("   Set 10-20% packet loss while this demo runs");
            Console.WriteLine();
            Console.WriteLine("Press any key to start demonstration...");
            Console.ReadKey(true);
            Console.Clear();
            
            // Setup cancellation
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };
            
            try
            {
                // Start server
                var serverTask = StartServer(cancellationTokenSource.Token);
                await Task.Delay(500); // Let server start
                
                // Start clients
                var clientTasks = new List<Task>();
                for (int i = 0; i < CONCURRENT_CONNECTIONS; i++)
                {
                    clientTasks.Add(StartClient(cancellationTokenSource.Token, i));
                    await Task.Delay(100); // Stagger starts
                }
                
                // Start monitoring
                var monitorTask = MonitorPerformance(cancellationTokenSource.Token);
                
                // Wait for cancellation
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nShutting down...");
            }
            
            DisplayFinalStatistics();
        }
        
        static string GetLocalIPAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up && 
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && 
                            !IPAddress.IsLoopback(ip.Address))
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return "127.0.0.1";  // Fallback to localhost
        }
        
        static void ShowLocalIPAddresses()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            Console.WriteLine($"   {ip.Address} ({ni.Name})");
                        }
                    }
                }
            }
        }
        
        static async Task StartServer(CancellationToken cancellationToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);
            var listener = new TcpListener(endpoint);
            
            listener.Start(100);
            Console.WriteLine($"✓ Server started on {SERVER_IP}:{SERVER_PORT}");
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        _ = Task.Run(() => HandleClient(client, cancellationToken));
                    }
                    catch (Exception)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Interlocked.Increment(ref ConnectionFailures);
                        }
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        
        static async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
        {
            var random = new Random();
            
            try
            {
                Interlocked.Increment(ref ActiveConnections);
                
                // Configure socket based on mode
                if (USE_SOLVED_VERSION)
                {
                    client.SendBufferSize = SOLVED_SEND_BUFFER;
                    client.ReceiveBufferSize = SOLVED_RECEIVE_BUFFER;
                    client.NoDelay = true;  // Disable Nagle for low latency
                }
                else
                {
                    client.SendBufferSize = PROBLEM_SEND_BUFFER;
                    client.ReceiveBufferSize = PROBLEM_RECEIVE_BUFFER;
                    client.NoDelay = false;  // Enable Nagle - adds latency
                }
                
                var stream = client.GetStream();
                var buffer = new byte[CHUNK_SIZE];
                
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    // Problem version: Add blocking delays
                    if (!USE_SOLVED_VERSION && random.NextDouble() < 0.3)
                    {
                        await Task.Delay(PROBLEM_DELAY_MS, cancellationToken);
                    }
                    
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;
                    
                    Interlocked.Add(ref TotalBytesReceived, bytesRead);
                    
                    // Problem version ONLY: Randomly abort connections (causes Connection Failures)
                    if (!USE_SOLVED_VERSION && random.NextDouble() < CONNECTION_ABORT_RATE)
                    {
                        Interlocked.Increment(ref AbruptDisconnects);
                        break;  // Abrupt disconnect - PROBLEM VERSION ONLY
                    }
                    
                    // Echo back
                    await stream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    Interlocked.Add(ref TotalBytesSent, bytesRead);
                    
                    // Solved version ONLY: Add small yield for cooperative multitasking
                    if (USE_SOLVED_VERSION)
                    {
                        await Task.Yield();  // Better CPU usage in solved version
                    }
                }
            }
            catch (Exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Interlocked.Increment(ref ConnectionFailures);
                }
            }
            finally
            {
                Interlocked.Decrement(ref ActiveConnections);
                client.Close();
            }
        }
        
        static async Task StartClient(CancellationToken cancellationToken, int clientId)
        {
            var random = new Random(clientId);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = null;
                
                try
                {
                    Interlocked.Increment(ref ConnectionAttempts);
                    
                    client = new TcpClient();
                    
                    // Configure based on mode
                    if (USE_SOLVED_VERSION)
                    {
                        client.SendBufferSize = SOLVED_SEND_BUFFER;
                        client.ReceiveBufferSize = SOLVED_RECEIVE_BUFFER;
                        client.NoDelay = true;
                    }
                    else
                    {
                        client.SendBufferSize = PROBLEM_SEND_BUFFER;
                        client.ReceiveBufferSize = PROBLEM_RECEIVE_BUFFER;
                        client.NoDelay = false;
                    }
                    
                    // Connect
                    await client.ConnectAsync(SERVER_IP, SERVER_PORT);
                    var stream = client.GetStream();
                    
                    // Send data
                    var sendBuffer = new byte[DATA_PER_CYCLE];
                    random.NextBytes(sendBuffer);
                    
                    var receiveBuffer = new byte[CHUNK_SIZE];
                    
                    for (int offset = 0; offset < sendBuffer.Length; offset += CHUNK_SIZE)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        
                        var bytesToSend = Math.Min(CHUNK_SIZE, sendBuffer.Length - offset);
                        
                        await stream.WriteAsync(sendBuffer, offset, bytesToSend, cancellationToken);
                        Interlocked.Add(ref TotalBytesSent, bytesToSend);
                        
                        var bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length, cancellationToken);
                        Interlocked.Add(ref TotalBytesReceived, bytesRead);
                        
                        // Solved version: Add small delay for stability
                        if (USE_SOLVED_VERSION)
                        {
                            await Task.Delay(10, cancellationToken);
                        }
                    }
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref ConnectionFailures);
                }
                finally
                {
                    client?.Close();
                }
                
                // Reconnect delay
                var reconnectDelay = USE_SOLVED_VERSION ? SOLVED_RECONNECT_DELAY_MS : RECONNECT_DELAY_MS;
                await Task.Delay(reconnectDelay, cancellationToken);
            }
        }
        
        static async Task MonitorPerformance(CancellationToken cancellationToken)
        {
            var lastBytesSent = 0L;
            var lastBytesReceived = 0L;
            var interval = TimeSpan.FromSeconds(2);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                
                var currentSent = Interlocked.Read(ref TotalBytesSent);
                var currentReceived = Interlocked.Read(ref TotalBytesReceived);
                
                var sentPerSec = (currentSent - lastBytesSent) / interval.TotalSeconds;
                var receivedPerSec = (currentReceived - lastBytesReceived) / interval.TotalSeconds;
                
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine($"  Mode: {(USE_SOLVED_VERSION ? "SOLVED" : "PROBLEM")} | {DateTime.Now:HH:mm:ss}");
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine();
                Console.WriteLine($"📊 Current Throughput:");
                Console.WriteLine($"   Sent:     {sentPerSec / 1024 / 1024:F2} MB/s");
                Console.WriteLine($"   Received: {receivedPerSec / 1024 / 1024:F2} MB/s");
                Console.WriteLine();
                Console.WriteLine($"🔗 Connection Statistics:");
                Console.WriteLine($"   Active:         {ActiveConnections}");
                Console.WriteLine($"   Attempts:       {ConnectionAttempts}");
                Console.WriteLine($"   Failures:       {ConnectionFailures}");
                Console.WriteLine($"   Disconnects:    {AbruptDisconnects}");
                Console.WriteLine($"   Failure Rate:   {(ConnectionAttempts > 0 ? (ConnectionFailures * 100.0 / ConnectionAttempts) : 0):F1}%");
                Console.WriteLine();
                Console.WriteLine($"📈 Cumulative:");
                Console.WriteLine($"   Total Sent:     {TotalBytesSent / 1024 / 1024:N0} MB");
                Console.WriteLine($"   Total Received: {TotalBytesReceived / 1024 / 1024:N0} MB");
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════════");
                Console.WriteLine("Check PerfMon to observe network behavior!");
                Console.WriteLine("Press Ctrl+C to stop...");
                
                lastBytesSent = currentSent;
                lastBytesReceived = currentReceived;
            }
        }
        
        static void DisplayFinalStatistics()
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("               FINAL STATISTICS");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine($"Mode: {(USE_SOLVED_VERSION ? "SOLVED" : "PROBLEM")}");
            Console.WriteLine();
            Console.WriteLine($"Total Data Transferred:");
            Console.WriteLine($"  Sent:     {TotalBytesSent / 1024 / 1024:N0} MB");
            Console.WriteLine($"  Received: {TotalBytesReceived / 1024 / 1024:N0} MB");
            Console.WriteLine();
            Console.WriteLine($"Connection Statistics:");
            Console.WriteLine($"  Attempts:     {ConnectionAttempts}");
            Console.WriteLine($"  Failures:     {ConnectionFailures}");
            Console.WriteLine($"  Disconnects:  {AbruptDisconnects}");
            Console.WriteLine($"  Failure Rate: {(ConnectionAttempts > 0 ? (ConnectionFailures * 100.0 / ConnectionAttempts) : 0):F1}%");
            Console.WriteLine();
            Console.WriteLine("Review Windows PerfMon to compare PROBLEM vs SOLVED versions!");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }
    }
}
