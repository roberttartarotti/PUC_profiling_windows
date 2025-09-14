/*
 * Exemplo 2 - Vazamento de Memória (Memory Leak) - SOLUÇÃO
 * 
 * NOTA: Este código é fornecido para demonstração das SOLUÇÕES para os problemas 
 * de vazamento de memória. Ele implementa as práticas corretas de gerenciamento 
 * de memória em C# para auxiliar no aprendizado de profiling de performance.
 * 
 * Objetivo do Exercício:
 * Este exemplo demonstra como corrigir os vazamentos de memória do exemplo anterior.
 * Vamos implementar as práticas corretas de gerenciamento de memória em C#
 * trabalhando com o Garbage Collector de forma adequada.
 * 
 * Soluções implementadas:
 * 1. Implementar IDisposable corretamente para limpeza de recursos
 * 2. Usar WeakReference em vez de referências diretas em coleções estáticas
 * 3. Usar using statements para limpeza automática de recursos
 * 4. Implementar padrão Dispose adequadamente com finalizer como backup
 * 5. Demonstrar limpeza de WeakReferences mortas
 * 6. Mostrar abordagem simples sem referências globais
 * 
 * Comandos disponíveis:
 * - good memory: Demonstra gerenciamento correto com WeakReferences e IDisposable
 * - simple memory: Demonstra abordagem simples sem referências globais
 * - good file: Demonstra manipulação correta de arquivos com using statements
 * - force collection: Força GC para demonstrar que objetos são coletados corretamente
 * - clear files: Limpa arquivos de teste criados
 * 
 * Técnicas demonstradas:
 * - IDisposable com padrão Dispose correto
 * - WeakReference para permitir coleta pelo GC mesmo em listas estáticas
 * - Using statements para liberação automática de recursos
 * - GC.SuppressFinalize() para otimizar performance
 * - Finalizer como backup para casos onde Dispose não foi chamado
 * - Limpeza automática de WeakReferences mortas
 * - Medição de uso de memória antes/depois das operações
 * 
 * Resultado: Memória será liberada pelo GC corretamente, sem vazamentos
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MemoryLeakDemo
{
    // SOLUÇÃO 1: Implementar IDisposable para limpeza adequada de recursos
    internal class DataProcessor : IDisposable
    {
        private int[] data;
        private bool disposed = false;
        
        // SOLUÇÃO 2: Usar WeakReference em vez de referências diretas na coleção estática
        // Isso permite que o GC colete os objetos mesmo estando na lista
        private static List<WeakReference> allProcessors = new List<WeakReference>();
        
        public DataProcessor(int dataSize)
        {
            data = new int[dataSize];
            
            for (int i = 0; i < dataSize; i++)
            {
                data[i] = i * 2;
            }
            
            // PASSO 1: Usar WeakReference em vez de referência direta
            // Isso não impede que o GC colete o objeto
            allProcessors.Add(new WeakReference(this));
            
            Console.WriteLine($"Alocados {dataSize * sizeof(int)} bytes de memória");
        }
        
        public void ProcessData()
        {
            // PASSO 2: Verificar se o objeto foi disposed antes de usar
            if (disposed)
                throw new ObjectDisposedException(nameof(DataProcessor));
                
            long sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            Console.WriteLine($"Soma calculada: {sum}");
        }
        
        // SOLUÇÃO 3: Implementar IDisposable corretamente
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Não precisa chamar o finalizer
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // PASSO 3: Liberar recursos gerenciados
                    Console.WriteLine($"Liberando {data?.Length * sizeof(int) ?? 0} bytes de memória");
                    data = null;  // Permitir que o GC colete o array
                    
                    // PASSO 4: Remover da lista de WeakReferences mortas
                    CleanupWeakReferences();
                }
                
                disposed = true;
            }
        }
        
        // SOLUÇÃO 4: Método para limpar WeakReferences mortas
        private static void CleanupWeakReferences()
        {
            for (int i = allProcessors.Count - 1; i >= 0; i--)
            {
                if (!allProcessors[i].IsAlive)
                {
                    allProcessors.RemoveAt(i);
                }
            }
        }
        
        // SOLUÇÃO 5: Método estático para obter contagem de objetos vivos
        public static int GetAliveCount()
        {
            CleanupWeakReferences();
            return allProcessors.Count;
        }
        
        // SOLUÇÃO 6: Finalizer como backup (só deve ser usado se Dispose não foi chamado)
        ~DataProcessor()
        {
            Console.WriteLine("AVISO: Finalizer chamado - Dispose() deveria ter sido usado!");
            Dispose(false);
        }
    }
    
    // SOLUÇÃO 7: Classe alternativa que não mantém referências globais
    internal class SimpleDataProcessor : IDisposable
    {
        private int[] data;
        private bool disposed = false;
        
        public SimpleDataProcessor(int dataSize)
        {
            data = new int[dataSize];
            
            for (int i = 0; i < dataSize; i++)
            {
                data[i] = i * 2;
            }
            
            Console.WriteLine($"[SIMPLES] Alocados {dataSize * sizeof(int)} bytes de memória");
        }
        
        public void ProcessData()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SimpleDataProcessor));
                
            long sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            Console.WriteLine($"[SIMPLES] Soma calculada: {sum}");
        }
        
        public void Dispose()
        {
            if (!disposed)
            {
                Console.WriteLine($"[SIMPLES] Liberando {data?.Length * sizeof(int) ?? 0} bytes de memória");
                data = null;
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
    
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Bem-vindo à demonstração de gerenciamento correto de memória");
            ListCommands();
            ClearFiles();
            
            var command = Console.ReadLine();
            while (command?.ToLower() != "x")
            {
                switch (command?.ToLower())
                {
                    case "list":
                        ListCommands();
                        break;
                    case "good memory":
                        GoodMemoryAllocation();
                        break;
                    case "simple memory":
                        SimpleMemoryAllocation();
                        break;
                    case "good file":
                        GoodFileHandling();
                        break;
                    case "force collection":
                        ForceCollection();
                        break;
                    case "clear files":
                        ClearFiles();
                        break;
                    default:
                        Console.WriteLine("Comando desconhecido. Tente novamente. Digite 'list' para ver todos os comandos disponíveis.");
                        break;
                }
                
                Console.WriteLine("Por favor, digite seu próximo comando:");
                command = Console.ReadLine();
            }
        }
        
        /// <summary>
        /// Exibe para o usuário uma listagem dos comandos disponíveis
        /// </summary>
        private static void ListCommands()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("Os seguintes comandos estão disponíveis:");
            Console.WriteLine("list = Mostra esta listagem de ações");
            Console.WriteLine("-- Helpers de Garbage Collection --");
            Console.WriteLine("force collection = Força o garbage collector a executar");
            Console.WriteLine("-- Exemplos de Gerenciamento Correto de Memória --");
            Console.WriteLine("good memory = Demonstra gerenciamento correto com WeakReferences");
            Console.WriteLine("simple memory = Demonstra abordagem simples sem referências globais");
            Console.WriteLine("good file = Demonstra manipulação correta de arquivos");
            Console.WriteLine("-- Helpers de Limpeza --");
            Console.WriteLine("clear files = Limpa arquivos de teste criados");
            Console.WriteLine(string.Empty);
            Console.WriteLine("Pressione X para sair");
        }
        
        #region Garbage Collection
        
        /// <summary>
        /// Força o garbage collector a executar, durante a demo pode ser útil para 
        /// redefinir/reduzir o uso de memória para testes adicionais de baseline/exemplo
        /// </summary>
        private static void ForceCollection()
        {
            Console.WriteLine("Forçando Garbage Collection...");
            long memoryBefore = GC.GetTotalMemory(false);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfter = GC.GetTotalMemory(false);
            
            Console.WriteLine($"Memória antes: {memoryBefore:N0} bytes");
            Console.WriteLine($"Memória depois: {memoryAfter:N0} bytes");
            Console.WriteLine($"Objetos DataProcessor vivos: {DataProcessor.GetAliveCount()}");
        }
        
        #endregion
        
        #region Memory Management
        
        /// <summary>
        /// SOLUÇÃO 8: Usar using statements para garantir limpeza automática
        /// </summary>
        private static void GoodMemoryAllocation()
        {
            Console.WriteLine("Iniciando Gerenciamento Correto de Memória com Using Statements");
            
            // PASSO 5: Using garante que Dispose() será chamado automaticamente
            for (int i = 0; i < 1000; i++)
            {
                using (var processor = new DataProcessor(10000))
                {
                    processor.ProcessData();
                } // Dispose() é chamado automaticamente aqui
            }
            
            Console.WriteLine("Completadas 1000 alocações com using statements");
            Console.WriteLine($"SOLUÇÃO: Objetos vivos após using: {DataProcessor.GetAliveCount()}");
        }
        
        /// <summary>
        /// SOLUÇÃO 9: Abordagem simples sem referências globais
        /// </summary>
        private static void SimpleMemoryAllocation()
        {
            Console.WriteLine("Iniciando Abordagem Simples (sem referências globais)");
            
            for (int i = 0; i < 1000; i++)
            {
                using (var processor = new SimpleDataProcessor(10000))
                {
                    processor.ProcessData();
                }
            }
            
            // PASSO 6: Forçar coleta de lixo para demonstrar limpeza
            Console.WriteLine("Forçando Garbage Collection para demonstrar limpeza...");
            long memoryBefore = GC.GetTotalMemory(false);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfter = GC.GetTotalMemory(false);
            
            Console.WriteLine("Completadas 1000 alocações simples");
            Console.WriteLine($"SOLUÇÃO: Memória liberada pelo GC: {memoryBefore - memoryAfter:N0} bytes");
        }
        
        #endregion
        
        #region File Handles
        
        /// <summary>
        /// SOLUÇÃO 10: Demonstra manipulação correta de arquivos usando using statements 
        /// para garantir que handles sejam liberados adequadamente
        /// </summary>
        private static void GoodFileHandling()
        {
            Console.WriteLine("Iniciando Manipulação Correta de Arquivos");
            
            if (!Directory.Exists("goodfile")) 
                Directory.CreateDirectory("goodfile");
            
            for (int i = 0; i < 100; i++)
            {
                WriteFileGood(i);
            }
            
            Console.WriteLine("Completadas 100 manipulações corretas de arquivo");
            Console.WriteLine("SOLUÇÃO: Todos os handles foram liberados com using statements!");
        }
        
        /// <summary>
        /// Implementação correta de escrita de arquivo usando using statements
        /// </summary>
        /// <param name="fileNumber"></param>
        private static void WriteFileGood(int fileNumber)
        {
            try
            {
                // SOLUÇÃO: Usar using statements para garantir liberação automática
                using var fs = new FileStream($"goodfile/{fileNumber}-example.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using var writer = new StreamWriter(fs);
                
                writer.WriteLine("Eu sou um escritor bom de arquivo!");
                
                // VANTAGEM: Dispose() é chamado automaticamente ao sair do escopo
                // Não precisamos chamar fs.Dispose() ou writer.Dispose() manualmente
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
        
        private static void ClearFiles()
        {
            Console.WriteLine("Limpando arquivos de teste...");
            
            try
            {
                if (Directory.Exists("badfile")) 
                    Directory.Delete("badfile", true);
                
                if (Directory.Exists("goodfile")) 
                    Directory.Delete("goodfile", true);
                
                Console.WriteLine("Arquivos de teste removidos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar arquivos: {ex.Message}");
            }
        }
        
        #endregion
        
        /// <summary>
        /// SOLUÇÃO 11: Demonstrar o uso correto de memória com medições
        /// </summary>
        private static void DemonstrateMemoryUsage()
        {
            Console.WriteLine("\n--- Demonstração de Uso Correto de Memória ---");
            
            Console.WriteLine("Antes de criar objetos:");
            long memoryBefore = GC.GetTotalMemory(true);
            Console.WriteLine($"Memória usada: {memoryBefore:N0} bytes");
            
            // Criar objetos com using (serão limpos automaticamente)
            for (int i = 0; i < 100; i++)
            {
                using (var processor = new SimpleDataProcessor(50000))
                {
                    processor.ProcessData();
                }
            }
            
            Console.WriteLine("\nApós criar e limpar objetos:");
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"Memória usada: {memoryAfter:N0} bytes");
            Console.WriteLine($"Diferença: {memoryAfter - memoryBefore:N0} bytes");
            Console.WriteLine("SOLUÇÃO: Memória foi gerenciada corretamente!");
        }
    }
}