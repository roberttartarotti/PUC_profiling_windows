using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MemoryPressure.Problem
{
    // Classe que representa um objeto complexo com alto custo de memória
    public class DataPacket
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public string Metadata { get; set; }
        public DateTime Timestamp { get; set; }
        public List<int> Tags { get; set; }

        public DataPacket(int id, int dataSize)
        {
            Id = id;
            Data = new byte[dataSize]; // Alocação grande!
            Metadata = $"Packet-{id}-{Guid.NewGuid()}"; // String allocation
            Timestamp = DateTime.Now;
            Tags = new List<int>(); // Mais alocação
            
            // Preencher com dados dummy
            for (int i = 0; i < 10; i++)
            {
                Tags.Add(i);
            }
        }
    }

    public class DataProcessor
    {
        private const int BufferSize = 256 * 1024; // 256 KB - MAIOR para causar mais pressão!
        private const int SmallBufferSize = 4096; // 4 KB - aumentado
        private List<DataPacket> processedPackets = new List<DataPacket>();
        private List<byte[]> temporaryBuffers = new List<byte[]>(); // Manter buffers temporários

        public void ProcessLargeDataSet(int iterations)
        {
            Console.WriteLine($"Processando {iterations} iterações...");
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                // PROBLEMA 1: Criar novo objeto grande a cada iteração
                var packet = new DataPacket(i, BufferSize);

                // PROBLEMA 2: Processar dados criando mais alocações
                ProcessPacket(packet);

                // PROBLEMA 3: Manter referências desnecessárias - MAIS AGRESSIVO!
                if (i % 50 == 0)
                {
                    processedPackets.Add(packet); // Impede GC de coletar
                    
                    // PROBLEMA EXTRA: Manter buffers temporários também!
                    temporaryBuffers.Add(new byte[BufferSize]);
                }

                // PROBLEMA 4: String concatenation sem StringBuilder - PIOR!
                string log = "";
                for (int j = 0; j < 100; j++)
                {
                    log += $"Tag-{j}-{DateTime.Now.Ticks},"; // Mais alocações por iteração
                }
                
                // PROBLEMA EXTRA: Criar mais objetos temporários
                var tempList = new List<string>();
                for (int j = 0; j < 20; j++)
                {
                    tempList.Add(new string('x', 1000)); // Strings de 1KB cada
                }

                if (i % 2000 == 0) // A cada 2000 para 10000 iterações
                {
                    Console.WriteLine($"Progresso: {i}/{iterations}");
                    // Forçar informações de memória
                    var gcInfo = GC.GetGCMemoryInfo();
                    Console.WriteLine($"  Heap Size: {gcInfo.HeapSizeBytes / 1024 / 1024} MB");
                    Console.WriteLine($"  Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}");
                }
            }

            sw.Stop();
            Console.WriteLine($"\n=== RESULTADO ===");
            Console.WriteLine($"Tempo total: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Coletas GC - Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}");
            Console.WriteLine($"Memória total alocada: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
            Console.WriteLine($"Pacotes mantidos em memória: {processedPackets.Count}");
        }

        private void ProcessPacket(DataPacket packet)
        {
            // PROBLEMA 5: Criar MÚLTIPLOS buffers temporários desnecessários
            byte[] tempBuffer1 = new byte[SmallBufferSize];
            byte[] tempBuffer2 = new byte[SmallBufferSize];
            byte[] tempBuffer3 = new byte[SmallBufferSize]; // Mais buffers!
            
            // Simular processamento
            for (int i = 0; i < packet.Data.Length && i < tempBuffer1.Length; i++)
            {
                tempBuffer1[i] = (byte)(packet.Data[i] ^ 0xFF);
                if (i < tempBuffer2.Length)
                    tempBuffer2[i] = (byte)(packet.Data[i] & 0xF0);
                if (i < tempBuffer3.Length)
                    tempBuffer3[i] = (byte)(packet.Data[i] | 0x0F);
            }

            // PROBLEMA 6: Criar novos arrays e listas para resultados - PIOR!
            var results = new List<byte>();
            for (int i = 0; i < 500; i++) // 5x mais que antes!
            {
                results.Add((byte)i);
            }
            
            // PROBLEMA EXTRA: Mais listas temporárias
            var intermediateResults = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                intermediateResults.Add(i * i);
            }

            // Simular alguma computação com mais alocações de strings
            packet.Metadata += $"-Processed-{results.Count}-{string.Join(",", intermediateResults.Take(10))}";
        }

        public void ProcessWithCollections()
        {
            Console.WriteLine("\nProcessando com coleções...");
            var sw = Stopwatch.StartNew();

            // PROBLEMA 7: Múltiplas listas temporárias - MUITO PIOR!
            for (int batch = 0; batch < 200; batch++)
            {
                var batchData = new List<DataPacket>();
                var extraData = new List<byte[]>(); // Lista extra!
                
                for (int i = 0; i < 200; i++)
                {
                    // Nova alocação para cada item - MAIOR!
                    batchData.Add(new DataPacket(batch * 200 + i, BufferSize)); // Usando BufferSize em vez de SmallBufferSize!
                    extraData.Add(new byte[SmallBufferSize]); // Buffer extra
                }

                // "Processar" o batch (MUITO mais alocações)
                var processedBatch = new List<string>();
                var summaryData = new List<string>(); // Lista extra
                foreach (var packet in batchData)
                {
                    processedBatch.Add($"Processed-{packet.Id}-{DateTime.Now.Ticks}");
                    summaryData.Add(string.Join("-", packet.Tags)); // Join cria nova string
                }
                
                // PROBLEMA EXTRA: Concatenar todas as strings (muito pesado!)
                string batchSummary = string.Join(";", processedBatch.Take(10));

                if (batch % 50 == 0)
                {
                    Console.WriteLine($"Batch {batch}/200 - Gen2 Collections: {GC.CollectionCount(2)}");
                }
            }

            sw.Stop();
            Console.WriteLine($"Tempo total do processamento em lote: {sw.ElapsedMilliseconds}ms");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("CASO 1: MEMORY PRESSURE - PROBLEMA");
            Console.WriteLine("===================================\n");

            Console.WriteLine("Este programa demonstra problemas de alocação excessiva de memória.");
            Console.WriteLine("Observe o alto número de coletas GC, especialmente Gen2.\n");

            // Mostrar memória inicial
            var initialMemory = GC.GetTotalMemory(true);
            Console.WriteLine($"Memória inicial: {initialMemory / 1024 / 1024} MB\n");

            var processor = new DataProcessor();

            // Teste 1: Processamento com alocações excessivas
            processor.ProcessLargeDataSet(10000);

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Teste 2: Processamento em lotes
            processor.ProcessWithCollections();

            // Memória final
            var finalMemory = GC.GetTotalMemory(false);
            Console.WriteLine($"\n=== RESUMO FINAL ===");
            Console.WriteLine($"Memória final: {finalMemory / 1024 / 1024} MB");
            Console.WriteLine($"Incremento de memória: {(finalMemory - initialMemory) / 1024 / 1024} MB");
            Console.WriteLine($"\nTotal de coletas GC:");
            Console.WriteLine($"  Gen0: {GC.CollectionCount(0)}");
            Console.WriteLine($"  Gen1: {GC.CollectionCount(1)}");
            Console.WriteLine($"  Gen2: {GC.CollectionCount(2)} <- ALTO! Isso é ruim para performance");

            Console.WriteLine("\n[Pressione qualquer tecla para sair]");
            Console.ReadKey();
        }
    }
}
