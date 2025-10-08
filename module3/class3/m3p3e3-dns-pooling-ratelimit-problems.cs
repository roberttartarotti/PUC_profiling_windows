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

namespace DNSPoolingRateLimitProblems
{
    /// <summary>
    /// DNS, Connection Pooling, and Rate Limiting PROBLEMS
    /// Module 3, Class 3, Example 3 - PROBLEM VERSION
    /// 
    /// This demonstrates severe issues with:
    /// - DNS resolution overhead and caching problems
    /// - Connection pool starvation and exhaustion
    /// - API rate limiting failures (429 Too Many Requests)
    /// - No retry logic or exponential backoff
    /// - ServicePoint connection limit issues
    /// - DNS round-robin not being utilized
    /// - Blocking DNS lookups
    /// 
    /// CRITICAL: This code demonstrates BAD practices!
    /// 
    /// Monitor in Windows PerfMon:
    /// - Network Interface: DNS Queries/sec (will be very high)
    /// - Process: Handle Count (growing from connection leaks)
    /// - TCPv4: Connection Failures (from rate limiting)
    /// - HTTP request failures and slow performance
    /// </summary>
    class Program
    {
        // PROBLEM CONFIGURATION - All intentionally bad
        private static readonly string API_ENDPOINT = "http://httpbin.org"; // Public API for testing
        private static readonly int CONCURRENT_REQUESTS = 200;      // Way too many (causes rate limiting)
        private static readonly int REQUEST_DELAY_MS = 10;          // Too aggressive (causes 429 errors)
        private static readonly int MAX_CONNECTIONS_PER_SERVER = 2; // Too few (connection starvation)
        private static readonly bool CACHE_DNS = false;             // No DNS caching (performance killer)
        private static readonly bool USE_DNS_ROUND_ROBIN = false;   // Don't use round-robin
        private static readonly bool IMPLEMENT_RETRY = false;       // No retry logic
        private static readonly bool IMPLEMENT_BACKOFF = false;     // No exponential backoff
        private static readonly int HTTP_TIMEOUT_MS = 5000;         // Too short (causes failures)
        
        // Statistics
        private static long RequestsSent = 0;
        private static long RequestsSucceeded = 0;
        private static long RequestsFailed = 0;
        private static long RateLimitErrors = 0;
        private static long TimeoutErrors = 0;
        private static long DnsLookups = 0;
        private static long ConnectionPoolStarvation = 0;
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   DNS, Connection Pooling, Rate Limiting PROBLEMS               ║");
            Console.WriteLine("║   WARNING: This demonstrates BAD practices!                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.WriteLine("⚠️  PROBLEM VERSION - Intentional Bad Practices:");
            Console.WriteLine($"✗ {CONCURRENT_REQUESTS} concurrent requests (causes rate limiting)");
            Console.WriteLine($"✗ {REQUEST_DELAY_MS}ms delay (too aggressive - triggers 429 errors)");
            Console.WriteLine($"✗ {MAX_CONNECTIONS_PER_SERVER} max connections (connection starvation)");
            Console.WriteLine($"✗ No DNS caching (every request does DNS lookup)");
            Console.WriteLine($"✗ No DNS round-robin (doesn't use multiple IPs)");
            Console.WriteLine($"✗ No retry logic (gives up immediately on failure)");
            Console.WriteLine($"✗ No exponential backoff (keeps hammering on errors)");
            Console.WriteLine($"✗ {HTTP_TIMEOUT_MS}ms timeout (too short for API calls)");
            Console.WriteLine($"✗ Creating new HttpClient per request (socket exhaustion)");
            Console.WriteLine();
            
            Console.WriteLine("Expected Problems:");
            Console.WriteLine("• High DNS query rate (no caching)");
            Console.WriteLine("• HTTP 429 (Too Many Requests) errors");
            Console.WriteLine("• Connection pool starvation (waiting for connections)");
            Console.WriteLine("• High failure rate (>30%)");
            Console.WriteLine("• Timeout errors from aggressive timeout");
            Console.WriteLine("• Poor throughput despite high concurrency");
            Console.WriteLine();
            
            Console.WriteLine("Press any key to start problematic demonstration...");
            Console.ReadKey(true);
            Console.Clear();
            
            // PROBLEM: Configure ServicePoint poorly
            ConfigureServicePointProblematic();
            
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cts.Cancel();
            };
            
            try
            {
                var monitorTask = MonitorPerformance(cts.Token);
                
                // Start many concurrent clients
                var clientTasks = new List<Task>();
                for (int i = 0; i < CONCURRENT_REQUESTS; i++)
                {
                    clientTasks.Add(MakeProblematicRequests(cts.Token, i));
                    await Task.Delay(5); // Very fast startup
                }
                
                Console.WriteLine($"Started {CONCURRENT_REQUESTS} concurrent clients");
                Console.WriteLine("Hammering API without proper rate limiting...");
                Console.WriteLine();
                
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nShutting down...");
            }
            
            DisplayFinalStatistics();
        }
        
        static void ConfigureServicePointProblematic()
        {
            // PROBLEM: Very low connection limit causes starvation
            ServicePointManager.DefaultConnectionLimit = MAX_CONNECTIONS_PER_SERVER;
            
            // PROBLEM: Disable DNS caching
            if (!CACHE_DNS)
            {
                ServicePointManager.DnsRefreshTimeout = 0;
            }
            
            // PROBLEM: Don't use DNS round-robin
            ServicePointManager.EnableDnsRoundRobin = USE_DNS_ROUND_ROBIN;
            
            // PROBLEM: Short idle timeout causes connection churn
            ServicePointManager.MaxServicePointIdleTime = 5000; // 5 seconds
            
            // PROBLEM: Enable Nagle (adds latency)
            ServicePointManager.UseNagleAlgorithm = true;
            
            // PROBLEM: Expect 100-continue (adds round trip)
            ServicePointManager.Expect100Continue = true;
        }
        
        static async Task MakeProblematicRequests(CancellationToken cancellationToken, int clientId)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpClient client = null;
                
                try
                {
                    // PROBLEM: Create new HttpClient for each request (socket exhaustion!)
                    client = new HttpClient
                    {
                        Timeout = TimeSpan.FromMilliseconds(HTTP_TIMEOUT_MS)
                    };
                    
                    Interlocked.Increment(ref RequestsSent);
                    
                    // PROBLEM: Synchronous DNS lookup happens here (not cached)
                    Interlocked.Increment(ref DnsLookups);
                    
                    // Make various API calls
                    var endpoint = GetRandomEndpoint();
                    
                    // PROBLEM: No retry logic - fails immediately
                    var response = await client.GetAsync($"{API_ENDPOINT}{endpoint}", cancellationToken);
                    
                    if (response.StatusCode == HttpStatusCode.TooManyRequests) // 429
                    {
                        Interlocked.Increment(ref RateLimitErrors);
                        Interlocked.Increment(ref RequestsFailed);
                        
                        // PROBLEM: No backoff - immediately try again
                        continue;
                    }
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref RequestsSucceeded);
                    }
                    else
                    {
                        Interlocked.Increment(ref RequestsFailed);
                    }
                }
                catch (TaskCanceledException)
                {
                    Interlocked.Increment(ref TimeoutErrors);
                    Interlocked.Increment(ref RequestsFailed);
                }
                catch (HttpRequestException ex)
                {
                    if (ex.Message.Contains("pool"))
                    {
                        Interlocked.Increment(ref ConnectionPoolStarvation);
                    }
                    Interlocked.Increment(ref RequestsFailed);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref RequestsFailed);
                }
                finally
                {
                    // PROBLEM: Not disposing properly (sometimes)
                    if (new Random().Next(100) < 70) // Only dispose 70% of the time
                    {
                        client?.Dispose();
                    }
                    // 30% leak!
                }
                
                // PROBLEM: No delay or very short delay (causes rate limiting)
                await Task.Delay(REQUEST_DELAY_MS, cancellationToken);
            }
        }
        
        static string GetRandomEndpoint()
        {
            var endpoints = new[]
            {
                "/get",
                "/status/200",
                "/delay/1",
                "/headers",
                "/user-agent"
            };
            
            return endpoints[new Random().Next(endpoints.Length)];
        }
        
        static async Task MonitorPerformance(CancellationToken cancellationToken)
        {
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.Now;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(2000, cancellationToken);
                
                process.Refresh();
                var runtime = DateTime.Now - startTime;
                
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║     DNS/Pooling/RateLimit PROBLEMS - Real-Time Stats        ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                
                Console.WriteLine($"⏱️  Runtime: {runtime:hh\\:mm\\:ss}");
                Console.WriteLine();
                
                Console.WriteLine($"📊 Request Statistics:");
                Console.WriteLine($"   Sent:          {RequestsSent:N0}");
                Console.WriteLine($"   Succeeded:     {RequestsSucceeded:N0}");
                Console.WriteLine($"   Failed:        {RequestsFailed:N0}");
                Console.WriteLine($"   Success Rate:  {(RequestsSent > 0 ? (RequestsSucceeded * 100.0 / RequestsSent) : 0):F1}%");
                Console.WriteLine($"   Throughput:    {(RequestsSucceeded / runtime.TotalSeconds):F1} req/sec");
                Console.WriteLine();
                
                Console.WriteLine($"❌ Problem Indicators:");
                Console.WriteLine($"   Rate Limit (429):        {RateLimitErrors:N0}");
                Console.WriteLine($"   Timeout Errors:          {TimeoutErrors:N0}");
                Console.WriteLine($"   DNS Lookups:             {DnsLookups:N0} (no caching!)");
                Console.WriteLine($"   Pool Starvation:         {ConnectionPoolStarvation:N0}");
                Console.WriteLine();
                
                Console.WriteLine($"💾 System Resources:");
                Console.WriteLine($"   Handle Count:  {process.HandleCount:N0} (growing = leaks!)");
                Console.WriteLine($"   Thread Count:  {process.Threads.Count:N0}");
                Console.WriteLine($"   Memory (MB):   {process.WorkingSet64 / 1024 / 1024:N0}");
                Console.WriteLine();
                
                Console.WriteLine("⚠️  PROBLEMS OBSERVED:");
                Console.WriteLine($"   ✗ High failure rate: {(RequestsSent > 0 ? (RequestsFailed * 100.0 / RequestsSent) : 0):F1}%");
                Console.WriteLine($"   ✗ Rate limiting errors: {RateLimitErrors:N0}");
                Console.WriteLine($"   ✗ No DNS caching: {DnsLookups:N0} lookups");
                Console.WriteLine($"   ✗ Connection starvation: {ConnectionPoolStarvation:N0}");
                Console.WriteLine($"   ✗ Timeout issues: {TimeoutErrors:N0}");
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
            
            Console.WriteLine("Request Performance:");
            Console.WriteLine($"  Total Sent:       {RequestsSent:N0}");
            Console.WriteLine($"  Succeeded:        {RequestsSucceeded:N0}");
            Console.WriteLine($"  Failed:           {RequestsFailed:N0}");
            Console.WriteLine($"  Success Rate:     {(RequestsSent > 0 ? (RequestsSucceeded * 100.0 / RequestsSent) : 0):F1}%");
            Console.WriteLine();
            
            Console.WriteLine("Problems Encountered:");
            Console.WriteLine($"  Rate Limit (429):      {RateLimitErrors:N0}");
            Console.WriteLine($"  Timeout Errors:        {TimeoutErrors:N0}");
            Console.WriteLine($"  DNS Lookups:           {DnsLookups:N0}");
            Console.WriteLine($"  Pool Starvation:       {ConnectionPoolStarvation:N0}");
            Console.WriteLine();
            
            Console.WriteLine("═══ PROBLEMS DEMONSTRATED ═══");
            Console.WriteLine("✗ No DNS caching - every request does DNS lookup");
            Console.WriteLine("✗ Connection pool starvation - only 2 connections");
            Console.WriteLine("✗ Rate limiting failures - no backoff strategy");
            Console.WriteLine("✗ No retry logic - gives up immediately");
            Console.WriteLine("✗ HttpClient per request - socket exhaustion");
            Console.WriteLine("✗ Aggressive timing - triggers API limits");
            Console.WriteLine("✗ Resource leaks - not disposing properly");
            Console.WriteLine();
            
            Console.WriteLine("💡 Solutions needed:");
            Console.WriteLine("   • Enable DNS caching");
            Console.WriteLine("   • Increase connection pool size");
            Console.WriteLine("   • Implement exponential backoff");
            Console.WriteLine("   • Add retry logic with circuit breaker");
            Console.WriteLine("   • Reuse HttpClient instance");
            Console.WriteLine("   • Implement rate limiting on client side");
            Console.WriteLine("   • Proper resource disposal");
        }
    }
}

