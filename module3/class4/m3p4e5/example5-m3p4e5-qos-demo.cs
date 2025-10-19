/*
 * =====================================================================================
 *
 *       Filename:  example5-m3p4e5-qos-demo.cs
 *
 *    Description:  QoS (Quality of Service) Demonstration
 *                  Shows traffic prioritization, bandwidth allocation, and QoS+Security
 *
 *        Version:  1.0
 *        Created:  2025
 *       Compiler:  C# .NET
 *
 *         Author:  Professor's Demonstration Code
 *   Organization:  PUC - Network Performance Course
 *
 *  Compile (Command Line):
 *    csc example5-m3p4e5-qos-demo.cs
 *
 *  Compile (Visual Studio):
 *    Open in Visual Studio and build
 *
 *  Usage:
 *    example5-m3p4e5-qos-demo.exe              # Interactive mode
 *    example5-m3p4e5-qos-demo.exe all          # Run all 4 modes
 *    example5-m3p4e5-qos-demo.exe 0            # Mode 0 only
 *    example5-m3p4e5-qos-demo.exe 1            # Mode 1 only
 *    example5-m3p4e5-qos-demo.exe 2            # Mode 2 only
 *    example5-m3p4e5-qos-demo.exe 3            # Mode 3 only
 *
 *  Wireshark Tips:
 *    - Start capture on localhost/loopback adapter
 *    - Filter: tcp.port == 8888
 *    - Observe different packet sizes and timing patterns
 *    - Look for traffic bursts and prioritization effects
 *
 * =====================================================================================
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace QoSDemo
{
    // =====================================================================================
    // CONFIGURATION
    // =====================================================================================

    public static class Config
    {
        public const int SERVER_PORT = 8888;
        public const string SERVER_IP = "127.0.0.1";
        public static int QOS_MODE = 0; // 0=No QoS, 1=Priority, 2=Dynamic, 3=Security
    }

    // =====================================================================================
    // TRAFFIC TYPES AND PRIORITIES
    // =====================================================================================

    public enum TrafficType
    {
        CRITICAL = 0,    // Video/VoIP - needs low latency
        NORMAL = 1,      // Web browsing - medium priority
        BULK = 2         // Downloads/Updates - low priority
    }

    public enum Priority
    {
        HIGH = 0,
        MEDIUM = 1,
        LOW = 2
    }

    // =====================================================================================
    // UTILITY CLASSES
    // =====================================================================================

    public class PrecisionTimer
    {
        private Stopwatch stopwatch;

        public PrecisionTimer()
        {
            stopwatch = Stopwatch.StartNew();
        }

        public double ElapsedMs()
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        public void Reset()
        {
            stopwatch.Restart();
        }
    }

    // Traffic packet structure
    public class TrafficPacket
    {
        public TrafficType Type { get; set; }
        public Priority Priority { get; set; }
        public int Size { get; set; }           // bytes
        public int SequenceNum { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSuspicious { get; set; }  // For security demo
    }

    // QoS Statistics
    public class QoSStats
    {
        public string TrafficName { get; set; } = "";
        public TrafficType Type { get; set; }
        public int PacketsSent { get; set; }
        public int PacketsReceived { get; set; }
        public double TotalLatency { get; set; }
        public double AvgLatency { get; set; }
        public double MinLatency { get; set; }
        public double MaxLatency { get; set; }
        public int BytesTransferred { get; set; }
        public bool MetSLA { get; set; }  // Service Level Agreement met
    }

    // =====================================================================================
    // QoS MANAGER
    // =====================================================================================

    public class QoSManager
    {
        private Dictionary<Priority, int> bandwidthAllocation = new Dictionary<Priority, int>();
        private Dictionary<Priority, double> maxLatency = new Dictionary<Priority, double>();
        private bool dynamicMode = false;
        private bool securityMode = false;

        public QoSManager()
        {
            // Default bandwidth allocation
            bandwidthAllocation[Priority.HIGH] = 70;
            bandwidthAllocation[Priority.MEDIUM] = 20;
            bandwidthAllocation[Priority.LOW] = 10;

            // SLA latency requirements
            maxLatency[Priority.HIGH] = 10.0;      // Critical: max 10ms
            maxLatency[Priority.MEDIUM] = 50.0;    // Normal: max 50ms
            maxLatency[Priority.LOW] = 200.0;      // Bulk: max 200ms
        }

        public void EnableDynamicMode() { dynamicMode = true; }
        public void EnableSecurityMode() { securityMode = true; }

        public Priority AssignPriority(TrafficType type, bool suspicious = false)
        {
            if (securityMode && suspicious)
            {
                return Priority.LOW; // Deprioritize suspicious traffic
            }

            switch (type)
            {
                case TrafficType.CRITICAL: return Priority.HIGH;
                case TrafficType.NORMAL: return Priority.MEDIUM;
                case TrafficType.BULK: return Priority.LOW;
                default: return Priority.MEDIUM;
            }
        }

        public int GetPacketSize(TrafficType type)
        {
            switch (type)
            {
                case TrafficType.CRITICAL: return 1024;      // 1 KB - small, frequent
                case TrafficType.NORMAL: return 10240;       // 10 KB - medium
                case TrafficType.BULK: return 102400;        // 100 KB - large
                default: return 1024;
            }
        }

        public void ApplyQoSDelay(Priority priority, int mode)
        {
            if (mode == 0)
            {
                // No QoS - all traffic gets same treatment
                Thread.Sleep(10);
            }
            else
            {
                // Apply priority-based delays
                switch (priority)
                {
                    case Priority.HIGH:
                        Thread.Sleep(1);
                        break;
                    case Priority.MEDIUM:
                        Thread.Sleep(5);
                        break;
                    case Priority.LOW:
                        Thread.Sleep(15);
                        break;
                }
            }
        }

        public bool CheckSLA(double latency, Priority priority)
        {
            return latency <= maxLatency[priority];
        }

        public string GetTrafficTypeName(TrafficType type)
        {
            switch (type)
            {
                case TrafficType.CRITICAL: return "Critical (Video/VoIP)";
                case TrafficType.NORMAL: return "Normal (Web)";
                case TrafficType.BULK: return "Bulk (Download)";
                default: return "Unknown";
            }
        }

        public string GetPriorityName(Priority p)
        {
            switch (p)
            {
                case Priority.HIGH: return "HIGH";
                case Priority.MEDIUM: return "MEDIUM";
                case Priority.LOW: return "LOW";
                default: return "UNKNOWN";
            }
        }

        public void AdjustPrioritiesForCongestion()
        {
            Console.WriteLine("\n[!] Network congestion detected!");
            Console.WriteLine("[!] Adjusting QoS priorities dynamically...");
            Thread.Sleep(500);
            Console.WriteLine("[OK] Critical traffic: bandwidth increased to 80%");
            Console.WriteLine("[OK] Bulk traffic: bandwidth reduced to 5%");
        }
    }

    // =====================================================================================
    // SERVER FUNCTION
    // =====================================================================================

    public class QoSServer
    {
        public void Run()
        {
            Console.WriteLine("\n=== QoS SERVER STARTED ===");
            Console.WriteLine($"Listening on port: {Config.SERVER_PORT}");
            Console.WriteLine("Waiting for traffic...");

            try
            {
                var listener = new TcpListener(IPAddress.Any, Config.SERVER_PORT);
                listener.Start();

                using (var client = listener.AcceptTcpClient())
                {
                    Console.WriteLine("[OK] Client connected");

                    using (var stream = client.GetStream())
                    {
                        // Receive traffic packets
                        int totalPackets = 0;
                        byte[] buffer = new byte[200000];

                        while (true)
                        {
                            int bytesReceived = stream.Read(buffer, 0, buffer.Length);
                            if (bytesReceived <= 0) break;

                            totalPackets++;

                            // Send acknowledgment
                            byte[] ack = Encoding.ASCII.GetBytes("ACK");
                            stream.Write(ack, 0, ack.Length);
                        }

                        Console.WriteLine($"[OK] Total packets received: {totalPackets}");
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
    // CLIENT FUNCTIONS - DIFFERENT MODES
    // =====================================================================================

    public class QoSClient
    {
        private static void SendTraffic(NetworkStream stream, TrafficType type, int count, QoSManager qos, 
            int mode, List<double> latencies, bool suspicious = false)
        {
            Priority priority = qos.AssignPriority(type, suspicious);
            int packetSize = qos.GetPacketSize(type);

            byte[] buffer = new byte[packetSize];
            for (int i = 0; i < packetSize; i++)
                buffer[i] = (byte)'X';

            byte[] ackBuffer = new byte[10];

            for (int i = 0; i < count; i++)
            {
                var timer = new PrecisionTimer();

                // Apply QoS delay before sending
                qos.ApplyQoSDelay(priority, mode);

                // Send packet
                stream.Write(buffer, 0, packetSize);

                // Receive acknowledgment
                stream.Read(ackBuffer, 0, ackBuffer.Length);

                double latency = timer.ElapsedMs();
                latencies.Add(latency);
            }
        }

        private static QoSStats CalculateStats(string name, TrafficType type, List<double> latencies, 
            QoSManager qos, int packetSize)
        {
            var stats = new QoSStats
            {
                TrafficName = name,
                Type = type,
                PacketsSent = latencies.Count,
                PacketsReceived = latencies.Count,
                TotalLatency = 0,
                MinLatency = double.MaxValue,
                MaxLatency = 0,
                BytesTransferred = packetSize * latencies.Count
            };

            foreach (double lat in latencies)
            {
                stats.TotalLatency += lat;
                if (lat < stats.MinLatency) stats.MinLatency = lat;
                if (lat > stats.MaxLatency) stats.MaxLatency = lat;
            }

            stats.AvgLatency = latencies.Count > 0 ? stats.TotalLatency / latencies.Count : 0;

            Priority priority = qos.AssignPriority(type);
            stats.MetSLA = qos.CheckSLA(stats.AvgLatency, priority);

            return stats;
        }

        private static void DisplayStats(List<QoSStats> allStats)
        {
            Console.WriteLine("\n=== TRAFFIC STATISTICS ===");
            Console.WriteLine();

            foreach (var stats in allStats)
            {
                Console.WriteLine("+---------------------------------------------------------------------------------+");
                Console.WriteLine($"| {stats.TrafficName}");
                Console.WriteLine("+---------------------------------------------------------------------------------+");
                Console.WriteLine($"| Packets sent:      {stats.PacketsSent}");
                Console.WriteLine($"| Bytes transferred: {stats.BytesTransferred} bytes");
                Console.Write($"| Average latency:   {stats.AvgLatency:F2} ms");
                if (stats.MetSLA)
                {
                    Console.Write(" [OK] SLA MET");
                }
                else
                {
                    Console.Write(" [FAIL] SLA VIOLATED");
                }
                Console.WriteLine();
                Console.WriteLine($"| Min latency:       {stats.MinLatency:F2} ms");
                Console.WriteLine($"| Max latency:       {stats.MaxLatency:F2} ms");
                Console.WriteLine("+---------------------------------------------------------------------------------+");
                Console.WriteLine();
            }
        }

        // =====================================================================================
        // MODE 0: NO QoS - All Traffic Equal
        // =====================================================================================

        public static void Mode0_NoQoS()
        {
            Console.WriteLine("\n=== MODE 0: NO QoS (ALL TRAFFIC EQUAL) ===");
            Console.WriteLine("All traffic types compete equally for bandwidth");
            Console.WriteLine("No prioritization applied");
            Console.WriteLine();

            var qos = new QoSManager();

            try
            {
                // Connect to server
                using (var client = new TcpClient(Config.SERVER_IP, Config.SERVER_PORT))
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("[OK] Connected to server");
                    Console.WriteLine("\nSending traffic...");

                    // Send different traffic types
                    var criticalLatencies = new List<double>();
                    var normalLatencies = new List<double>();
                    var bulkLatencies = new List<double>();

                    Console.WriteLine("  -> Sending Critical traffic (10 packets)...");
                    SendTraffic(stream, TrafficType.CRITICAL, 10, qos, 0, criticalLatencies);

                    Console.WriteLine("  -> Sending Normal traffic (10 packets)...");
                    SendTraffic(stream, TrafficType.NORMAL, 10, qos, 0, normalLatencies);

                    Console.WriteLine("  -> Sending Bulk traffic (10 packets)...");
                    SendTraffic(stream, TrafficType.BULK, 10, qos, 0, bulkLatencies);

                    // Calculate and display stats
                    var allStats = new List<QoSStats>
                    {
                        CalculateStats("Critical Traffic (Video/VoIP)", TrafficType.CRITICAL, criticalLatencies, qos, qos.GetPacketSize(TrafficType.CRITICAL)),
                        CalculateStats("Normal Traffic (Web)", TrafficType.NORMAL, normalLatencies, qos, qos.GetPacketSize(TrafficType.NORMAL)),
                        CalculateStats("Bulk Traffic (Download)", TrafficType.BULK, bulkLatencies, qos, qos.GetPacketSize(TrafficType.BULK))
                    };

                    DisplayStats(allStats);

                    Console.WriteLine("=== ANALYSIS ===");
                    Console.WriteLine("- All traffic types experience similar latency");
                    Console.WriteLine("- Critical traffic may NOT meet SLA requirements");
                    Console.WriteLine("- Video/VoIP quality suffers during congestion");
                    Console.WriteLine("- No differentiation between traffic priorities");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        // =====================================================================================
        // MODE 1: QoS Priority Classes
        // =====================================================================================

        public static void Mode1_QoSPriority()
        {
            Console.WriteLine("\n=== MODE 1: QoS WITH PRIORITY CLASSES ===");
            Console.WriteLine("Traffic is prioritized: HIGH > MEDIUM > LOW");
            Console.WriteLine("Bandwidth allocation: 70% / 20% / 10%");
            Console.WriteLine();

            var qos = new QoSManager();

            try
            {
                // Connect to server
                using (var client = new TcpClient(Config.SERVER_IP, Config.SERVER_PORT))
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("[OK] Connected to server");
                    Console.WriteLine("[OK] QoS policies applied");
                    Console.WriteLine("\nSending traffic with QoS...");

                    // Send different traffic types with QoS
                    var criticalLatencies = new List<double>();
                    var normalLatencies = new List<double>();
                    var bulkLatencies = new List<double>();

                    Console.WriteLine("  -> Sending Critical traffic (HIGH priority, 10 packets)...");
                    SendTraffic(stream, TrafficType.CRITICAL, 10, qos, 1, criticalLatencies);

                    Console.WriteLine("  -> Sending Normal traffic (MEDIUM priority, 10 packets)...");
                    SendTraffic(stream, TrafficType.NORMAL, 10, qos, 1, normalLatencies);

                    Console.WriteLine("  -> Sending Bulk traffic (LOW priority, 10 packets)...");
                    SendTraffic(stream, TrafficType.BULK, 10, qos, 1, bulkLatencies);

                    // Calculate and display stats
                    var allStats = new List<QoSStats>
                    {
                        CalculateStats("Critical Traffic (Video/VoIP) - HIGH Priority", TrafficType.CRITICAL, criticalLatencies, qos, qos.GetPacketSize(TrafficType.CRITICAL)),
                        CalculateStats("Normal Traffic (Web) - MEDIUM Priority", TrafficType.NORMAL, normalLatencies, qos, qos.GetPacketSize(TrafficType.NORMAL)),
                        CalculateStats("Bulk Traffic (Download) - LOW Priority", TrafficType.BULK, bulkLatencies, qos, qos.GetPacketSize(TrafficType.BULK))
                    };

                    DisplayStats(allStats);

                    Console.WriteLine("=== ANALYSIS ===");
                    Console.WriteLine("- Critical traffic gets lowest latency (priority treatment)");
                    Console.WriteLine("- SLA requirements are MET for high-priority traffic");
                    Console.WriteLine("- Bulk traffic latency increases but remains acceptable");
                    Console.WriteLine("- Clear differentiation between priority classes");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        // =====================================================================================
        // MODE 2: Dynamic QoS Adjustment
        // =====================================================================================

        public static void Mode2_DynamicQoS()
        {
            Console.WriteLine("\n=== MODE 2: DYNAMIC QoS ADJUSTMENT ===");
            Console.WriteLine("QoS adapts to network conditions in real-time");
            Console.WriteLine("Simulates congestion detection and priority adjustment");
            Console.WriteLine();

            var qos = new QoSManager();
            qos.EnableDynamicMode();

            try
            {
                // Connect to server
                using (var client = new TcpClient(Config.SERVER_IP, Config.SERVER_PORT))
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("[OK] Connected to server");
                    Console.WriteLine("[OK] Dynamic QoS monitoring enabled");
                    Console.WriteLine("\nPhase 1: Normal conditions...");

                    // Phase 1: Normal traffic
                    var criticalLatencies1 = new List<double>();
                    var normalLatencies1 = new List<double>();
                    var bulkLatencies1 = new List<double>();

                    Console.WriteLine("  -> Sending traffic under normal conditions...");
                    SendTraffic(stream, TrafficType.CRITICAL, 5, qos, 1, criticalLatencies1);
                    SendTraffic(stream, TrafficType.NORMAL, 5, qos, 1, normalLatencies1);
                    SendTraffic(stream, TrafficType.BULK, 5, qos, 1, bulkLatencies1);

                    // Simulate congestion detection
                    qos.AdjustPrioritiesForCongestion();

                    Console.WriteLine("\nPhase 2: Under congestion (adjusted priorities)...");

                    // Phase 2: Adjusted traffic
                    var criticalLatencies2 = new List<double>();
                    var normalLatencies2 = new List<double>();
                    var bulkLatencies2 = new List<double>();

                    Console.WriteLine("  -> Sending traffic with adjusted priorities...");
                    SendTraffic(stream, TrafficType.CRITICAL, 5, qos, 1, criticalLatencies2);
                    SendTraffic(stream, TrafficType.NORMAL, 5, qos, 1, normalLatencies2);
                    SendTraffic(stream, TrafficType.BULK, 5, qos, 1, bulkLatencies2);

                    // Combine latencies
                    criticalLatencies1.AddRange(criticalLatencies2);
                    normalLatencies1.AddRange(normalLatencies2);
                    bulkLatencies1.AddRange(bulkLatencies2);

                    // Calculate and display stats
                    var allStats = new List<QoSStats>
                    {
                        CalculateStats("Critical Traffic (Adaptive Priority)", TrafficType.CRITICAL, criticalLatencies1, qos, qos.GetPacketSize(TrafficType.CRITICAL)),
                        CalculateStats("Normal Traffic (Adaptive Priority)", TrafficType.NORMAL, normalLatencies1, qos, qos.GetPacketSize(TrafficType.NORMAL)),
                        CalculateStats("Bulk Traffic (Adaptive Priority)", TrafficType.BULK, bulkLatencies1, qos, qos.GetPacketSize(TrafficType.BULK))
                    };

                    DisplayStats(allStats);

                    Console.WriteLine("=== ANALYSIS ===");
                    Console.WriteLine("- QoS automatically detected network congestion");
                    Console.WriteLine("- Critical traffic bandwidth increased dynamically");
                    Console.WriteLine("- System adapted without manual intervention");
                    Console.WriteLine("- Maintains service quality during varying conditions");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        // =====================================================================================
        // MODE 3: QoS + Security Integration
        // =====================================================================================

        public static void Mode3_QoSWithSecurity()
        {
            Console.WriteLine("\n=== MODE 3: QoS + SECURITY INTEGRATION ===");
            Console.WriteLine("Combines traffic prioritization with threat detection");
            Console.WriteLine("Suspicious traffic is deprioritized or blocked");
            Console.WriteLine();

            var qos = new QoSManager();
            qos.EnableSecurityMode();

            try
            {
                // Connect to server
                using (var client = new TcpClient(Config.SERVER_IP, Config.SERVER_PORT))
                using (var stream = client.GetStream())
                {
                    Console.WriteLine("[OK] Connected to server");
                    Console.WriteLine("[OK] QoS + Security policies active");
                    Console.WriteLine();

                    // Send legitimate traffic
                    Console.WriteLine("Sending legitimate traffic...");
                    var legitimateCritical = new List<double>();
                    var legitimateNormal = new List<double>();

                    Console.WriteLine("  -> Critical traffic (legitimate)...");
                    SendTraffic(stream, TrafficType.CRITICAL, 5, qos, 1, legitimateCritical, false);

                    Console.WriteLine("  -> Normal traffic (legitimate)...");
                    SendTraffic(stream, TrafficType.NORMAL, 5, qos, 1, legitimateNormal, false);

                    // Simulate threat detection
                    Console.WriteLine("\n[!] SECURITY ALERT: Suspicious traffic detected!");
                    Console.WriteLine("[!] Source: 192.168.1.100 (simulated)");
                    Console.WriteLine("[!] Pattern: Unusual bulk data transfer");
                    Console.WriteLine("[!] Action: Deprioritizing suspicious traffic");
                    Thread.Sleep(500);

                    // Send suspicious traffic (gets deprioritized)
                    Console.WriteLine("\nSending suspicious traffic (deprioritized)...");
                    var suspiciousTraffic = new List<double>();

                    Console.WriteLine("  -> Bulk traffic (marked suspicious)...");
                    SendTraffic(stream, TrafficType.BULK, 5, qos, 1, suspiciousTraffic, true);

                    Console.WriteLine("\n[OK] Legitimate critical traffic: PROTECTED");
                    Console.WriteLine("[OK] Suspicious traffic: DEPRIORITIZED");

                    // Calculate and display stats
                    var allStats = new List<QoSStats>
                    {
                        CalculateStats("Legitimate Critical Traffic (PROTECTED)", TrafficType.CRITICAL, legitimateCritical, qos, qos.GetPacketSize(TrafficType.CRITICAL)),
                        CalculateStats("Legitimate Normal Traffic (PROTECTED)", TrafficType.NORMAL, legitimateNormal, qos, qos.GetPacketSize(TrafficType.NORMAL)),
                        CalculateStats("Suspicious Traffic (DEPRIORITIZED)", TrafficType.BULK, suspiciousTraffic, qos, qos.GetPacketSize(TrafficType.BULK))
                    };

                    DisplayStats(allStats);

                    Console.WriteLine("=== SECURITY + QoS ANALYSIS ===");
                    Console.WriteLine("- Legitimate traffic maintains high priority");
                    Console.WriteLine("- Suspicious traffic automatically deprioritized");
                    Console.WriteLine("- Critical services protected during security events");
                    Console.WriteLine("- Integrated approach: security + performance");
                    Console.WriteLine("\n[OK] Check Wireshark: Notice traffic patterns and timing differences");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }
    }

    // =====================================================================================
    // RUN ALL MODES
    // =====================================================================================

    class QoSDemonstration
    {
        public static void RunAllModes()
        {
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("                    RUNNING ALL 4 MODES - COMPLETE QoS DEMONSTRATION");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine();

            // Start server in background thread
            var serverThreads = new List<Thread>();
            for (int i = 0; i < 4; i++)
            {
                var serverThread = new Thread(() => new QoSServer().Run());
                serverThread.IsBackground = true;
                serverThreads.Add(serverThread);
            }

            // Start servers sequentially as needed
            int serverIndex = 0;

            // Run Mode 0
            Console.WriteLine("\n");
            Console.WriteLine("#####################################################################################");
            Console.WriteLine("#                                    MODE 0                                        #");
            Console.WriteLine("#####################################################################################");
            serverThreads[serverIndex++].Start();
            Thread.Sleep(1000);
            QoSClient.Mode0_NoQoS();
            Thread.Sleep(2000);

            // Run Mode 1
            Console.WriteLine("\n\n");
            Console.WriteLine("#####################################################################################");
            Console.WriteLine("#                                    MODE 1                                        #");
            Console.WriteLine("#####################################################################################");
            serverThreads[serverIndex++].Start();
            Thread.Sleep(1000);
            QoSClient.Mode1_QoSPriority();
            Thread.Sleep(2000);

            // Run Mode 2
            Console.WriteLine("\n\n");
            Console.WriteLine("#####################################################################################");
            Console.WriteLine("#                                    MODE 2                                        #");
            Console.WriteLine("#####################################################################################");
            serverThreads[serverIndex++].Start();
            Thread.Sleep(1000);
            QoSClient.Mode2_DynamicQoS();
            Thread.Sleep(2000);

            // Run Mode 3
            Console.WriteLine("\n\n");
            Console.WriteLine("#####################################################################################");
            Console.WriteLine("#                                    MODE 3                                        #");
            Console.WriteLine("#####################################################################################");
            serverThreads[serverIndex++].Start();
            Thread.Sleep(1000);
            QoSClient.Mode3_QoSWithSecurity();

            // Final comprehensive analysis
            Console.WriteLine("\n\n");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("                    COMPLETE DEMONSTRATION SUMMARY");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("[OK] Mode 0: No QoS demonstration completed");
            Console.WriteLine("[OK] Mode 1: Priority classes demonstration completed");
            Console.WriteLine("[OK] Mode 2: Dynamic QoS demonstration completed");
            Console.WriteLine("[OK] Mode 3: QoS + Security demonstration completed");
            Console.WriteLine("=====================================================================================");

            Console.WriteLine("\n");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine("                    COMPREHENSIVE QoS ANALYSIS");
            Console.WriteLine("=====================================================================================");
            Console.WriteLine();

            Console.WriteLine(">> PERFORMANCE COMPARISON:");
            Console.WriteLine();

            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| MODE 0: No QoS (Baseline)                                                      |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| - All traffic treated equally                                                  |");
            Console.WriteLine("| - Critical traffic may violate SLA                                             |");
            Console.WriteLine("| - Video/VoIP quality suffers during congestion                                 |");
            Console.WriteLine("| - Rating: * (No optimization)                                                  |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine();

            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| MODE 1: QoS Priority Classes ***** BEST FOR PRODUCTION                         |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| - Traffic prioritized: HIGH > MEDIUM > LOW                                     |");
            Console.WriteLine("| - Critical traffic meets SLA requirements                                      |");
            Console.WriteLine("| - Bandwidth allocation: 70% / 20% / 10%                                        |");
            Console.WriteLine("| - Clear performance differentiation                                            |");
            Console.WriteLine("| - Rating: ***** (Essential for production networks)                            |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine();

            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| MODE 2: Dynamic QoS ****                                                       |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| - Adapts to network conditions automatically                                   |");
            Console.WriteLine("| - Detects congestion and adjusts priorities                                    |");
            Console.WriteLine("| - Maintains service quality during varying load                                |");
            Console.WriteLine("| - Rating: **** (Important for dynamic environments)                            |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine();

            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| MODE 3: QoS + Security *****                                                   |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine("| - Integrates security with traffic management                                  |");
            Console.WriteLine("| - Deprioritizes suspicious traffic                                             |");
            Console.WriteLine("| - Protects legitimate critical services                                        |");
            Console.WriteLine("| - Combined approach: performance + security                                    |");
            Console.WriteLine("| - Rating: ***** (Modern network requirement)                                   |");
            Console.WriteLine("+---------------------------------------------------------------------------------+");
            Console.WriteLine();

            Console.WriteLine("*** WINNER: MODE 1 + MODE 3 (QoS Priority + Security Integration) ***");
            Console.WriteLine();

            Console.WriteLine("WHY QoS IS ESSENTIAL:");
            Console.WriteLine("  1. >> Guarantees critical traffic performance");
            Console.WriteLine("  2. >> Prevents bandwidth starvation");
            Console.WriteLine("  3. >> Meets SLA requirements consistently");
            Console.WriteLine("  4. >> Improves user experience for real-time apps");
            Console.WriteLine("  5. >> Enables efficient resource utilization");
            Console.WriteLine("  6. >> Provides security integration capabilities");
            Console.WriteLine("  7. >> Essential for modern enterprise networks");
            Console.WriteLine();

            Console.WriteLine(">> BEST PRACTICES - RECOMMENDED APPROACH:");
            Console.WriteLine();
            Console.WriteLine("  Step 1: Classify traffic into priority classes (Critical/Normal/Bulk)");
            Console.WriteLine("          -> Identify business-critical applications");
            Console.WriteLine();
            Console.WriteLine("  Step 2: Implement QoS policies with appropriate bandwidth allocation");
            Console.WriteLine("          -> Reserve bandwidth for high-priority traffic");
            Console.WriteLine();
            Console.WriteLine("  Step 3: Enable dynamic adjustment for varying network conditions");
            Console.WriteLine("          -> Monitor and adapt to congestion automatically");
            Console.WriteLine();
            Console.WriteLine("  Step 4: Integrate security policies with QoS");
            Console.WriteLine("          -> Protect critical services during security events");
            Console.WriteLine();
            Console.WriteLine("  Result: Optimal performance, security, and user experience!");
            Console.WriteLine();

            Console.WriteLine(">> KEY INSIGHTS:");
            Console.WriteLine("  - QoS prevents 'noisy neighbor' problems in shared networks");
            Console.WriteLine("  - Video conferencing requires <10ms latency (only possible with QoS)");
            Console.WriteLine("  - Bulk downloads don't impact real-time applications with proper QoS");
            Console.WriteLine("  - Security integration ensures protection without sacrificing performance");
            Console.WriteLine("  - Future: AI-driven QoS with predictive traffic management");
            Console.WriteLine();

            Console.WriteLine(">> CLASSROOM TAKEAWAY:");
            Console.WriteLine("  Without QoS, all traffic is equal - but not all traffic has equal importance!");
            Console.WriteLine("  QoS ensures critical applications get the resources they need, when they need them.");
            Console.WriteLine();

            Console.WriteLine("=====================================================================================");
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
            Console.WriteLine("                    QoS (QUALITY OF SERVICE) DEMONSTRATION");
            Console.WriteLine("=====================================================================================");

            // Parse command line arguments
            bool runAll = false;
            int selectedMode = -1;

            if (args.Length >= 1)
            {
                string arg1 = args[0];

                if (arg1.ToLower() == "all")
                {
                    runAll = true;
                }
                else
                {
                    if (int.TryParse(args[0], out selectedMode))
                    {
                        if (selectedMode < 0 || selectedMode > 3)
                        {
                            Console.WriteLine("Error: Invalid mode. Mode must be 0-3 or 'all'");
                            Console.WriteLine($"Usage: {System.AppDomain.CurrentDomain.FriendlyName} [mode|all]");
                            Console.WriteLine("  mode: 0=No QoS, 1=Priority, 2=Dynamic, 3=Security");
                            Console.WriteLine("  all: Run all 4 modes in sequence");
                            return;
                        }
                        Config.QOS_MODE = selectedMode;
                    }
                }
            }

            // If "all" specified, run all modes
            if (runAll)
            {
                QoSDemonstration.RunAllModes();
                return;
            }

            // If specific mode specified via command line, run once and exit
            if (selectedMode != -1)
            {
                Console.WriteLine($"Mode: {Config.QOS_MODE}");
                Console.WriteLine("=====================================================================================");

                // Start server in background
                var serverThread = new Thread(() => new QoSServer().Run());
                serverThread.IsBackground = true;
                serverThread.Start();

                Thread.Sleep(1000);

                // Execute selected mode
                switch (Config.QOS_MODE)
                {
                    case 0:
                        QoSClient.Mode0_NoQoS();
                        break;
                    case 1:
                        QoSClient.Mode1_QoSPriority();
                        break;
                    case 2:
                        QoSClient.Mode2_DynamicQoS();
                        break;
                    case 3:
                        QoSClient.Mode3_QoSWithSecurity();
                        break;
                }

                Console.WriteLine("\n=====================================================================================");
                Console.WriteLine("DEMONSTRATION COMPLETE");
                Console.WriteLine("=====================================================================================");
                return;
            }

            // Interactive mode (no command line arguments)
            Console.WriteLine("This program demonstrates QoS (Quality of Service) traffic prioritization");
            Console.WriteLine("and security integration using different operating modes.");
            Console.WriteLine();
            Console.WriteLine("Available modes:");
            Console.WriteLine("  0 - No QoS (baseline - all traffic equal)");
            Console.WriteLine("  1 - QoS with Priority Classes (HIGH/MEDIUM/LOW)");
            Console.WriteLine("  2 - Dynamic QoS Adjustment (adaptive to congestion)");
            Console.WriteLine("  3 - QoS + Security Integration (threat-aware prioritization)");
            Console.WriteLine();
            Console.WriteLine($"Current mode: {Config.QOS_MODE}");
            Console.WriteLine("=====================================================================================");

            string userInput;

            while (true)
            {
                Console.Write("\n>>> Type 'run' to execute, 'mode' to change mode, 'all' for all modes, 'quit' to exit: ");
                userInput = Console.ReadLine();

                if (userInput == "quit" || userInput == "exit")
                {
                    Console.WriteLine("Exiting demonstration...");
                    break;
                }
                else if (userInput == "all" || userInput == "ALL")
                {
                    QoSDemonstration.RunAllModes();
                    continue;
                }
                else if (userInput == "mode")
                {
                    Config.QOS_MODE = (Config.QOS_MODE + 1) % 4;
                    Console.WriteLine($"Mode changed to: {Config.QOS_MODE}");
                    Console.WriteLine("  - Mode 0: No QoS");
                    Console.WriteLine("  - Mode 1: Priority Classes");
                    Console.WriteLine("  - Mode 2: Dynamic QoS");
                    Console.WriteLine("  - Mode 3: QoS + Security");
                    continue;
                }
                else if (userInput == "run")
                {
                    // Start server in background
                    var serverThread = new Thread(() => new QoSServer().Run());
                    serverThread.IsBackground = true;
                    serverThread.Start();

                    Thread.Sleep(1000);

                    // Execute current mode
                    switch (Config.QOS_MODE)
                    {
                        case 0:
                            QoSClient.Mode0_NoQoS();
                            break;
                        case 1:
                            QoSClient.Mode1_QoSPriority();
                            break;
                        case 2:
                            QoSClient.Mode2_DynamicQoS();
                            break;
                        case 3:
                            QoSClient.Mode3_QoSWithSecurity();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command. Use 'run', 'mode', 'all', or 'quit'.");
                }
            }
        }
    }
}

