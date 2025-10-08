using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;

namespace HTTPTCPIntensiveProblems
{
    /// <summary>
    /// HTTP and TCP/IP Intensive Problems Demonstration
    /// Module 3, Class 3, Example 2 - PROBLEM VERSION ONLY
    /// 
    /// This demonstrates SEVERE network issues with HTTP and TCP/IP:
    /// - HTTP connection pooling exhaustion
    /// - TCP port exhaustion (ephemeral port starvation)
    /// - Socket leaks and resource exhaustion
    /// - Synchronous blocking I/O causing thread starvation
    /// - HTTP timeout issues and cascading failures
    /// - TCP backlog overflow
    /// - Keep-alive abuse and connection thrashing
    /// 
    /// CRITICAL: This code is intentionally BAD to demonstrate problems!
    /// 
    /// Monitor in Windows PerfMon:
    /// - TCPv4: Connections Established (will spike very high)
    /// - TCPv4: Connection Failures (will increase rapidly)
    /// - TCPv4: Segments Retransmitted/sec (with network issues)
    /// - Network Interface: Bytes Sent/Received per sec
    /// - .NET CLR Networking: Connections Established
    /// - Process: Handle Count (will increase - socket leak)
    /// - Process: Thread Count (will increase - thread starvation)
    /// </summary>
    class Program
    {
        // PROBLEM CONFIGURATION - All values are intentionally bad!
        private static readonly int HTTP_SERVER_PORT = 9000;
        private static readonly int TCP_SERVER_PORT = 9001;
        
        // HTTP Problems
        private static readonly int HTTP_CLIENTS_COUNT = 100;  // Excessive concurrent HTTP requests
        private static readonly int HTTP_TIMEOUT_MS = 1000;     // Too short timeout (causes failures)
        private static readonly int HTTP_MAX_CONNECTIONS = 2;   // Too few connections (causes queuing)
        
        // TCP Problems  
        private static readonly int TCP_CLIENTS_COUNT = 500;    // Massive concurrent connections (port exhaustion)
        private static readonly int TCP_BACKLOG = 5;            // Tiny backlog (causes connection refusal)
        private static readonly int SOCKET_TIMEOUT_MS = 500;    // Too aggressive timeout
        
        // Problem behaviors
        private static readonly bool USE_SYNCHRONOUS_IO = true;      // Blocking I/O (thread starvation)
        private static readonly bool DISABLE_KEEPALIVE = false;      // Keep-alive enabled (connection thrashing)
        private static readonly bool LEAK_CONNECTIONS = true;        // Don't dispose properly (resource leak)
        private static readonly bool REUSE_HTTP_CLIENT = false;      // Create new HttpClient each time (socket exhaustion)
        
        // Data sizes
        private static readonly int HTTP_RESPONSE_SIZE = 1024 * 1024; // 1MB HTTP responses
        private static readonly int TCP_DATA_SIZE = 512 * 1024;       // 512KB TCP data
        
        // Statistics
        private static int HttpRequestsSent = 0;
        private static int HttpRequestsFailed = 0;
        private static int HttpTimeouts = 0;
        private static int TcpConnectionsOpened = 0;
        private static int TcpConnectionsFailed = 0;
        private static int SocketsLeaked = 0;
        private static int ThreadsCreated = 0;
        private static long TotalBytesTransferred = 0;
        
        // Leaked resources (intentional for demonstration)
        private static List<Socket> LeakedSockets = new List<Socket>();
        private static List<HttpClient> LeakedHttpClients = new List<HttpClient>();
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   HTTP and TCP/IP Intensive PROBLEMS Demonstration              ║");
            Console.WriteLine("║   WARNING: This code is intentionally BAD!                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("⚠️  CRITICAL: This is a PROBLEM-ONLY demonstration!");
            Console.WriteLine("   All coding practices here are WRONG and cause severe issues.");
            Console.WriteLine();
            
            Console.WriteLine("═══ Intentional Problems Being Demonstrated ═══");
            Console.WriteLine($"✗ HTTP: {HTTP_CLIENTS_COUNT} concurrent clients, {HTTP_MAX_CONNECTIONS} max connections (pool exhaustion)");
            Console.WriteLine($"✗ HTTP: {HTTP_TIMEOUT_MS}ms timeout (too aggressive, causes failures)");
            Console.WriteLine($"✗ HTTP: Creating new HttpClient per request (socket exhaustion)");
            Console.WriteLine($"✗ TCP: {TCP_CLIENTS_COUNT} concurrent connections (port exhaustion)");
            Console.WriteLine($"✗ TCP: Backlog of {TCP_BACKLOG} (causes connection refusal)");
            Console.WriteLine($"✗ Using synchronous blocking I/O (thread starvation)");
            Console.WriteLine($"✗ Not disposing resources properly (memory and socket leaks)");
            Console.WriteLine($"✗ Keep-alive mismanagement (connection thrashing)");
            Console.WriteLine();
            
            Console.WriteLine("═══ Expected Problems You Will Observe ═══");
            Console.WriteLine("📊 Windows PerfMon:");
            Console.WriteLine("   • TCPv4 Connections Established - will spike to thousands");
            Console.WriteLine("   • TCPv4 Connection Failures - will increase rapidly");
            Console.WriteLine("   • Process Handle Count - will grow (socket leaks)");
            Console.WriteLine("   • Process Thread Count - will increase (thread starvation)");
            Console.WriteLine("   • Network Interface Bytes - high irregular traffic");
            Console.WriteLine();
            Console.WriteLine("💻 System Impact:");
            Console.WriteLine("   • Port exhaustion (may see 'No buffer space available' errors)");
            Console.WriteLine("   • Thread pool starvation");
            Console.WriteLine("   • High CPU usage from context switching");
            Console.WriteLine("   • Memory growth from leaked connections");
            Console.WriteLine();
            
            Console.WriteLine("Press any key to start the problematic demonstration...");
            Console.ReadKey(true);
            Console.Clear();
            
            // Setup cancellation
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cts.Cancel();
            };
            
            try
            {
                // Start HTTP server
                var httpServerTask = StartProblematicHttpServer(cts.Token);
                
                // Start TCP server
                var tcpServerTask = StartProblematicTcpServer(cts.Token);
                
                await Task.Delay(1000); // Let servers start
                
                // Start monitoring
                var monitorTask = MonitorSystemResources(cts.Token);
                
                // Start HTTP clients (connection pool exhaustion)
                var httpClientTasks = new List<Task>();
                for (int i = 0; i < HTTP_CLIENTS_COUNT; i++)
                {
                    httpClientTasks.Add(StartProblematicHttpClient(cts.Token, i));
                    await Task.Delay(20); // Slight stagger
                }
                
                // Start TCP clients (port exhaustion)
                var tcpClientTasks = new List<Task>();
                for (int i = 0; i < TCP_CLIENTS_COUNT; i++)
                {
                    tcpClientTasks.Add(StartProblematicTcpClient(cts.Token, i));
                    await Task.Delay(10); // Rapid fire
                }
                
                Console.WriteLine($"Started {HTTP_CLIENTS_COUNT} HTTP clients and {TCP_CLIENTS_COUNT} TCP clients");
                Console.WriteLine("Generating intensive problematic network traffic...");
                Console.WriteLine();
                
                // Wait for cancellation
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nShutting down...");
            }
            
            DisplayFinalStatistics();
            
            // Show leaked resources
            Console.WriteLine();
            Console.WriteLine($"⚠️  LEAKED RESOURCES (intentional for demonstration):");
            Console.WriteLine($"   Sockets not disposed: {LeakedSockets.Count}");
            Console.WriteLine($"   HttpClients not disposed: {LeakedHttpClients.Count}");
            Console.WriteLine();
            Console.WriteLine("In production code, these would cause serious memory leaks!");
        }
        
        static async Task StartProblematicHttpServer(CancellationToken cancellationToken)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{HTTP_SERVER_PORT}/");
            listener.Start();
            
            Console.WriteLine($"✓ HTTP Server started on port {HTTP_SERVER_PORT}");
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = await listener.GetContextAsync();
                    
                    // PROBLEM: Synchronous blocking I/O on thread pool thread
                    if (USE_SYNCHRONOUS_IO)
                    {
                        _ = Task.Run(() => HandleHttpRequestSync(context));
                        Interlocked.Increment(ref ThreadsCreated);
                    }
                    else
                    {
                        _ = HandleHttpRequestAsync(context, cancellationToken);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        
        static void HandleHttpRequestSync(HttpListenerContext context)
        {
            // PROBLEM: Synchronous blocking operations
            try
            {
                // PROBLEM: Simulate slow processing (blocks thread)
                Thread.Sleep(new Random().Next(100, 500));
                
                var response = context.Response;
                var responseData = new byte[HTTP_RESPONSE_SIZE];
                new Random().NextBytes(responseData);
                
                response.ContentLength64 = responseData.Length;
                response.ContentType = "application/octet-stream";
                
                // PROBLEM: Synchronous write (blocks thread)
                response.OutputStream.Write(responseData, 0, responseData.Length);
                response.OutputStream.Close(); // At least we close this one
                
                Interlocked.Add(ref TotalBytesTransferred, responseData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Server error: {ex.Message}");
            }
        }
        
        static async Task HandleHttpRequestAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                var response = context.Response;
                var responseData = new byte[HTTP_RESPONSE_SIZE];
                new Random().NextBytes(responseData);
                
                response.ContentLength64 = responseData.Length;
                
                await response.OutputStream.WriteAsync(responseData, 0, responseData.Length, cancellationToken);
                response.OutputStream.Close();
                
                Interlocked.Add(ref TotalBytesTransferred, responseData.Length);
            }
            catch { }
        }
        
        static async Task StartProblematicTcpServer(CancellationToken cancellationToken)
        {
            var listener = new TcpListener(IPAddress.Any, TCP_SERVER_PORT);
            
            // PROBLEM: Tiny backlog causes connection refusal
            listener.Start(TCP_BACKLOG);
            
            Console.WriteLine($"✓ TCP Server started on port {TCP_SERVER_PORT} (backlog: {TCP_BACKLOG})");
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    
                    // PROBLEM: Synchronous handling blocks threads
                    if (USE_SYNCHRONOUS_IO)
                    {
                        _ = Task.Run(() => HandleTcpClientSync(client));
                        Interlocked.Increment(ref ThreadsCreated);
                    }
                    else
                    {
                        _ = HandleTcpClientAsync(client, cancellationToken);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        
        static void HandleTcpClientSync(TcpClient client)
        {
            try
            {
                // PROBLEM: Very aggressive timeout causes premature disconnects
                client.ReceiveTimeout = SOCKET_TIMEOUT_MS;
                client.SendTimeout = SOCKET_TIMEOUT_MS;
                
                var stream = client.GetStream();
                var buffer = new byte[8192];
                
                // PROBLEM: Synchronous blocking read
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                if (bytesRead > 0)
                {
                    // PROBLEM: Synchronous blocking write
                    stream.Write(buffer, 0, bytesRead);
                    Interlocked.Add(ref TotalBytesTransferred, bytesRead * 2);
                }
                
                // PROBLEM: Sometimes we "forget" to close (leak simulation)
                if (!LEAK_CONNECTIONS || new Random().Next(100) < 80)
                {
                    client.Close();
                }
                else
                {
                    // LEAK: Intentionally not closing
                    lock (LeakedSockets)
                    {
                        LeakedSockets.Add(client.Client);
                    }
                    Interlocked.Increment(ref SocketsLeaked);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref TcpConnectionsFailed);
            }
        }
        
        static async Task HandleTcpClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                client.ReceiveTimeout = SOCKET_TIMEOUT_MS;
                client.SendTimeout = SOCKET_TIMEOUT_MS;
                
                var stream = client.GetStream();
                var buffer = new byte[8192];
                
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                
                if (bytesRead > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    Interlocked.Add(ref TotalBytesTransferred, bytesRead * 2);
                }
            }
            catch
            {
                Interlocked.Increment(ref TcpConnectionsFailed);
            }
            finally
            {
                client?.Close();
            }
        }
        
        static async Task StartProblematicHttpClient(CancellationToken cancellationToken, int clientId)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpClient client = null;
                
                try
                {
                    // PROBLEM: Creating new HttpClient for each request (socket exhaustion!)
                    if (!REUSE_HTTP_CLIENT)
                    {
                        // CRITICAL PROBLEM: This creates socket exhaustion!
                        client = new HttpClient();
                        client.Timeout = TimeSpan.FromMilliseconds(HTTP_TIMEOUT_MS);
                        
                        // PROBLEM: Setting max connections per server (too low)
                        ServicePointManager.DefaultConnectionLimit = HTTP_MAX_CONNECTIONS;
                        
                        // Track leaked clients
                        if (LEAK_CONNECTIONS)
                        {
                            lock (LeakedHttpClients)
                            {
                                LeakedHttpClients.Add(client);
                            }
                        }
                    }
                    
                    Interlocked.Increment(ref HttpRequestsSent);
                    
                    // PROBLEM: Synchronous HTTP call on async method (blocks threads)
                    if (USE_SYNCHRONOUS_IO)
                    {
                        var response = client.GetAsync($"http://localhost:{HTTP_SERVER_PORT}/data").Result; // ANTI-PATTERN!
                        var content = response.Content.ReadAsByteArrayAsync().Result; // ANTI-PATTERN!
                    }
                    else
                    {
                        var response = await client.GetAsync($"http://localhost:{HTTP_SERVER_PORT}/data", cancellationToken);
                        var content = await response.Content.ReadAsByteArrayAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    Interlocked.Increment(ref HttpTimeouts);
                    Interlocked.Increment(ref HttpRequestsFailed);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref HttpRequestsFailed);
                }
                finally
                {
                    // PROBLEM: Only dispose if not leaking
                    if (!LEAK_CONNECTIONS && client != null)
                    {
                        client.Dispose();
                    }
                }
                
                // PROBLEM: No delay - aggressive request rate
                await Task.Delay(50, cancellationToken);
            }
        }
        
        static async Task StartProblematicTcpClient(CancellationToken cancellationToken, int clientId)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Socket socket = null;
                
                try
                {
                    Interlocked.Increment(ref TcpConnectionsOpened);
                    
                    // PROBLEM: Creating lots of sockets (port exhaustion)
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    
                    // PROBLEM: Aggressive timeout
                    socket.ReceiveTimeout = SOCKET_TIMEOUT_MS;
                    socket.SendTimeout = SOCKET_TIMEOUT_MS;
                    
                    // PROBLEM: Disable keep-alive causes more connection churn
                    if (!DISABLE_KEEPALIVE)
                    {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    }
                    
                    await socket.ConnectAsync("127.0.0.1", TCP_SERVER_PORT);
                    
                    var data = new byte[TCP_DATA_SIZE];
                    new Random(clientId).NextBytes(data);
                    
                    // PROBLEM: Synchronous send on async method
                    if (USE_SYNCHRONOUS_IO)
                    {
                        socket.Send(data); // Blocking call!
                        
                        var receiveBuffer = new byte[8192];
                        socket.Receive(receiveBuffer); // Blocking call!
                    }
                    else
                    {
                        await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                        
                        var receiveBuffer = new byte[8192];
                        await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), SocketFlags.None);
                    }
                    
                    // PROBLEM: Intentional socket leak (20% of the time)
                    if (LEAK_CONNECTIONS && new Random().Next(100) < 20)
                    {
                        lock (LeakedSockets)
                        {
                            LeakedSockets.Add(socket);
                        }
                        Interlocked.Increment(ref SocketsLeaked);
                    }
                    else
                    {
                        socket.Close();
                        socket.Dispose();
                    }
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref TcpConnectionsFailed);
                    socket?.Close();
                    socket?.Dispose();
                }
                
                // PROBLEM: Very short delay causes connection thrashing
                await Task.Delay(100, cancellationToken);
            }
        }
        
        static async Task MonitorSystemResources(CancellationToken cancellationToken)
        {
            var process = Process.GetCurrentProcess();
            var interval = TimeSpan.FromSeconds(2);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                
                process.Refresh();
                
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║        HTTP/TCP Intensive PROBLEMS - Real-Time Stats        ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                
                Console.WriteLine($"🌐 HTTP Statistics:");
                Console.WriteLine($"   Requests Sent:     {HttpRequestsSent:N0}");
                Console.WriteLine($"   Requests Failed:   {HttpRequestsFailed:N0}");
                Console.WriteLine($"   Timeouts:          {HttpTimeouts:N0}");
                Console.WriteLine($"   Failure Rate:      {(HttpRequestsSent > 0 ? (HttpRequestsFailed * 100.0 / HttpRequestsSent) : 0):F1}%");
                Console.WriteLine();
                
                Console.WriteLine($"🔌 TCP Statistics:");
                Console.WriteLine($"   Connections Opened: {TcpConnectionsOpened:N0}");
                Console.WriteLine($"   Connections Failed: {TcpConnectionsFailed:N0}");
                Console.WriteLine($"   Sockets Leaked:     {SocketsLeaked:N0}");
                Console.WriteLine($"   Failure Rate:       {(TcpConnectionsOpened > 0 ? (TcpConnectionsFailed * 100.0 / TcpConnectionsOpened) : 0):F1}%");
                Console.WriteLine();
                
                Console.WriteLine($"💾 System Resources:");
                Console.WriteLine($"   Process Handle Count: {process.HandleCount:N0} (growing = socket leak!)");
                Console.WriteLine($"   Process Thread Count: {process.Threads.Count:N0} (high = thread starvation!)");
                Console.WriteLine($"   Working Set (RAM):    {process.WorkingSet64 / 1024 / 1024:N0} MB");
                Console.WriteLine($"   Threads Created:      {ThreadsCreated:N0}");
                Console.WriteLine();
                
                Console.WriteLine($"📊 Data Transfer:");
                Console.WriteLine($"   Total Transferred:  {TotalBytesTransferred / 1024 / 1024:N0} MB");
                Console.WriteLine();
                
                Console.WriteLine("⚠️  PROBLEMS YOU SHOULD SEE:");
                Console.WriteLine("   ✗ High HTTP failure rate (timeout issues)");
                Console.WriteLine("   ✗ TCP connection failures (port exhaustion)");
                Console.WriteLine("   ✗ Growing handle count (socket leaks)");
                Console.WriteLine("   ✗ High thread count (thread starvation)");
                Console.WriteLine("   ✗ Increasing memory usage");
                Console.WriteLine();
                Console.WriteLine("📈 Check Windows PerfMon for:");
                Console.WriteLine("   • TCPv4 → Connections Established (high!)");
                Console.WriteLine("   • TCPv4 → Connection Failures (increasing!)");
                Console.WriteLine("   • Process → Handle Count (growing!)");
                Console.WriteLine("   • Process → Thread Count (high!)");
                Console.WriteLine();
                Console.WriteLine("Press Ctrl+C to stop...");
            }
        }
        
        static void DisplayFinalStatistics()
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              FINAL STATISTICS - PROBLEM VERSION              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.WriteLine("HTTP Issues:");
            Console.WriteLine($"  Total Requests:    {HttpRequestsSent:N0}");
            Console.WriteLine($"  Failed Requests:   {HttpRequestsFailed:N0}");
            Console.WriteLine($"  Timeout Failures:  {HttpTimeouts:N0}");
            Console.WriteLine($"  Failure Rate:      {(HttpRequestsSent > 0 ? (HttpRequestsFailed * 100.0 / HttpRequestsSent) : 0):F1}%");
            Console.WriteLine();
            
            Console.WriteLine("TCP Issues:");
            Console.WriteLine($"  Connections Opened: {TcpConnectionsOpened:N0}");
            Console.WriteLine($"  Connection Failures: {TcpConnectionsFailed:N0}");
            Console.WriteLine($"  Sockets Leaked:     {SocketsLeaked:N0}");
            Console.WriteLine($"  Failure Rate:       {(TcpConnectionsOpened > 0 ? (TcpConnectionsFailed * 100.0 / TcpConnectionsOpened) : 0):F1}%");
            Console.WriteLine();
            
            Console.WriteLine("Resource Leaks:");
            Console.WriteLine($"  Leaked Sockets:     {LeakedSockets.Count:N0}");
            Console.WriteLine($"  Leaked HttpClients: {LeakedHttpClients.Count:N0}");
            Console.WriteLine();
            
            Console.WriteLine("═══ PROBLEMS DEMONSTRATED ═══");
            Console.WriteLine("✗ Socket exhaustion from creating HttpClient per request");
            Console.WriteLine("✗ Port exhaustion from too many concurrent TCP connections");
            Console.WriteLine("✗ Thread starvation from synchronous I/O on async methods");
            Console.WriteLine("✗ Resource leaks from not disposing connections properly");
            Console.WriteLine("✗ Connection pool exhaustion from low MaxConnectionsPerServer");
            Console.WriteLine("✗ Timeout cascading failures from aggressive timeout values");
            Console.WriteLine("✗ TCP backlog overflow from tiny server queue");
            Console.WriteLine();
            Console.WriteLine("💡 Teaching Points:");
            Console.WriteLine("   1. Always reuse HttpClient (or use IHttpClientFactory)");
            Console.WriteLine("   2. Use proper async/await patterns (no .Result or .Wait())");
            Console.WriteLine("   3. Always dispose network resources properly");
            Console.WriteLine("   4. Configure appropriate timeouts and connection limits");
            Console.WriteLine("   5. Use adequate TCP backlog for server load");
            Console.WriteLine("   6. Monitor system resources to detect leaks early");
        }
    }
}

