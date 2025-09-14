/*
 * Exemplo 2 - Vazamento de Memória (Memory Leak) - PROBLEMA
 * 
 * NOTA: Este código é fornecido apenas para fins de demonstração. Ele contém 
 * intencionalmente problemas de vazamento de memória para auxiliar na 
 * demonstração de profiling de performance e detecção de memory leaks.
 * 
 * Objetivo do Exercício:
 * Este exemplo demonstra problemas comuns de vazamento de memória em C#.
 * Vamos criar um programa interativo que permite testar diferentes cenários
 * onde o Garbage Collector não consegue liberar memória adequadamente.
 * 
 * O que vamos fazer:
 * 1. Criar uma classe que mantém referências estáticas impedindo coleta pelo GC
 * 2. Demonstrar vazamento através de coleções estáticas que crescem indefinidamente
 * 3. Mostrar vazamento de handles de arquivo sem usar using statements
 * 4. Usar interface interativa para testar diferentes cenários
 * 5. Observar como objetos ficam "presos" na memória usando ferramentas de profiling
 * 
 * Comandos disponíveis:
 * - bad memory: Cria 1000 objetos que ficam presos em lista estática
 * - bad file: Abre 100 arquivos sem liberar handles adequadamente
 * - force collection: Força execução do GC para demonstrar que objetos não são coletados
 * - clear files: Limpa arquivos de teste criados
 * 
 * Problemas demonstrados:
 * - Lista estática mantém referências vivas impedindo coleta pelo GC
 * - FileStream e StreamWriter criados sem using statements
 * - Objetos acumulam indefinidamente na memória mesmo após force GC
 * - Handles de arquivo não liberados causando "arquivo em uso por outro processo"
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MemoryLeakDemo
{
    // PROBLEMA: Esta classe mantém referências estáticas que nunca são limpas
    // Isso impede que o Garbage Collector libere os objetos
    internal class DataProcessor
    {
        private int[] data;
        
        // PROBLEMA: Lista estática mantém referências vivas para todos os objetos
        // Isso causa vazamento de memória porque o GC nunca pode coletar estes objetos
        private static List<DataProcessor> allProcessors = new List<DataProcessor>();
        
        public DataProcessor(int dataSize)
        {
            data = new int[dataSize];
            
            for (int i = 0; i < dataSize; i++)
            {
                data[i] = i * 2;
            }
            
            // PROBLEMA: Adicionamos referência estática que nunca é removida
            allProcessors.Add(this);
            
            Console.WriteLine($"Alocados {dataSize * sizeof(int)} bytes de memória");
        }
        
        public void ProcessData()
        {
            long sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            Console.WriteLine($"Soma calculada: {sum}");
        }
        
        public static int GetProcessorCount()
        {
            return allProcessors.Count;
        }
    }
    
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Bem-vindo à demonstração de vazamento de memória");
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
                    case "bad memory":
                        BadMemoryAllocation();
                        break;
                    case "bad file":
                        BadFileHandling();
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
            Console.WriteLine("-- Exemplos de Vazamento de Memória --");
            Console.WriteLine("bad memory = Demonstra vazamento de memória com 1000 objetos");
            Console.WriteLine("bad file = Demonstra vazamento de handles de arquivo");
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
            Console.WriteLine($"Objetos DataProcessor na memória: {DataProcessor.GetProcessorCount()}");
        }
        
        #endregion
        
        #region Memory Leaks
        
        /// <summary>
        /// Demonstra vazamento de memória criando objetos que são mantidos vivos 
        /// por referências estáticas, impedindo que o GC os colete
        /// </summary>
        /// <remarks>
        /// Embora este seja um exemplo extremo, considere as implicações no mundo real 
        /// de um site de alto tráfego com coleções estáticas que crescem indefinidamente.
        /// Isso pode se acumular rapidamente e causar problemas de performance.
        /// </remarks>
        private static void BadMemoryAllocation()
        {
            Console.WriteLine("Iniciando Alocação Ruim de Memória");
            
            // PROBLEMA: Criamos muitos objetos que ficam na lista estática
            // Isso impede que o GC colete os objetos, causando vazamento
            for (int i = 0; i < 1000; i++)
            {
                var processor = new DataProcessor(10000);
                processor.ProcessData();
            }
            
            Console.WriteLine("Completadas 1000 alocações de memória");
            Console.WriteLine($"PROBLEMA: {DataProcessor.GetProcessorCount()} objetos nunca serão coletados pelo GC!");
        }
        
        #endregion
        
        #region File Handles
        
        /// <summary>
        /// Demonstra vazamento de handles de arquivo abrindo arquivos sem usar 
        /// using statements, resultando em muitos handles sendo deixados abertos
        /// </summary>
        /// <remarks>
        /// Além dos problemas de memória com o exemplo, a maioria destes arquivos 
        /// também mostrará "Em uso por outro processo" se tentarem ser modificados.
        /// Como tal, é importante notar que você pode não conseguir executar este 
        /// exemplo 2x seguidas
        /// </remarks>
        private static void BadFileHandling()
        {
            Console.WriteLine("Iniciando Manipulação Ruim de Arquivos");
            
            if (!Directory.Exists("badfile")) 
                Directory.CreateDirectory("badfile");
            
            for (int i = 0; i < 100; i++)
            {
                WriteFileBad(i);
            }
            
            Console.WriteLine("Completadas 100 aberturas de arquivo");
            Console.WriteLine("PROBLEMA: Handles de arquivo nunca foram fechados!");
        }
        
        /// <summary>
        /// Implementação interna de escrita de um único arquivo
        /// </summary>
        /// <param name="fileNumber"></param>
        private static void WriteFileBad(int fileNumber)
        {
            try
            {
                // PROBLEMA: Criamos FileStream e StreamWriter mas não os liberamos
                // Isso causa vazamento de handles de arquivo
                var fs = new FileStream($"badfile/{fileNumber}-example.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var writer = new StreamWriter(fs);
                writer.WriteLine("Eu sou um escritor ruim de arquivo");
                
                // PROBLEMA: Nunca chamamos Dispose() ou usamos using statements!
                // fs.Dispose();
                // writer.Dispose();
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
                
                Console.WriteLine("Arquivos de teste removidos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar arquivos: {ex.Message}");
            }
        }
        
        #endregion
    }
}