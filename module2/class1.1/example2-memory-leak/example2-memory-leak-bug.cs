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
 * 
 * Como usar este exemplo:
 * 1. Compile e execute o programa
 * 2. Use ferramentas de profiling (.NET Diagnostic Tools, PerfView, dotMemory)
 * 3. Execute "bad memory" múltiplas vezes e observe o crescimento da memória
 * 4. Execute "force collection" e note que a memória não diminui
 * 5. Execute "bad file" e tente acessar os arquivos externamente
 * 
 * Ferramentas recomendadas para análise:
 * - Visual Studio: Diagnostic Tools (Debug → Windows → Show Diagnostic Tools)
 * - JetBrains dotMemory: Profiler dedicado para memória .NET
 * - PerfView: Ferramenta gratuita da Microsoft para análise ETW
 * - Application Insights: Para monitoramento em produção
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
        
        // PROBLEMA CRÍTICO: Lista estática mantém referências vivas para todos os objetos
        // Isso causa vazamento de memória porque o GC nunca pode coletar estes objetos
        // Cada objeto adicionado aqui fica "imortal" até o processo terminar
        private static List<DataProcessor> allProcessors = new List<DataProcessor>();
        
        public DataProcessor(int dataSize)
        {
            data = new int[dataSize];
            
            // Inicializa com dados para simular processamento real
            for (int i = 0; i < dataSize; i++)
            {
                data[i] = i * 2;
            }
            
            // PROBLEMA: Adicionamos referência estática que nunca é removida
            // Isso impede que o GC colete este objeto, mesmo quando não há outras referências
            allProcessors.Add(this);
            
            Console.WriteLine($"✓ Alocados {dataSize * sizeof(int)} bytes de memória (objeto #{allProcessors.Count})");
        }
        
        public void ProcessData()
        {
            long sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            Console.WriteLine($"  → Soma calculada: {sum} (processando {data.Length} elementos)");
        }
        
        public static int GetProcessorCount()
        {
            return allProcessors.Count;
        }
        
        // Método para demonstrar que os objetos ainda estão vivos
        public static void ShowMemoryUsage()
        {
            long totalMemory = 0;
            foreach (var processor in allProcessors)
            {
                if (processor.data != null)
                {
                    totalMemory += processor.data.Length * sizeof(int);
                }
            }
            
            Console.WriteLine($"📊 Objetos vivos: {allProcessors.Count}");
            Console.WriteLine($"📊 Memória total ocupada: ≈{totalMemory:N0} bytes");
        }
    }
    
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("================================================");
            Console.WriteLine("  BEM-VINDO À DEMONSTRAÇÃO DE VAZAMENTO DE MEMÓRIA");
            Console.WriteLine("================================================");
            Console.WriteLine();
            Console.WriteLine("Este programa demonstra problemas comuns de vazamento");
            Console.WriteLine("de memória em C# para fins educacionais.");
            Console.WriteLine();
            Console.WriteLine("IMPORTANTE: Use ferramentas de profiling para observar");
            Console.WriteLine("           o comportamento da memória em tempo real!");
            Console.WriteLine();
            
            ListCommands();
            ClearFiles();
            
            var command = Console.ReadLine();
            while (command?.ToLower() != "x")
            {
                Console.WriteLine();
                
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
                    case "":
                        Console.WriteLine("⚠️  Comando vazio. Digite 'list' para ver os comandos disponíveis.");
                        break;
                    default:
                        Console.WriteLine($"❌ Comando desconhecido: '{command}'");
                        Console.WriteLine("   Digite 'list' para ver todos os comandos disponíveis.");
                        break;
                }
                
                Console.WriteLine();
                Console.WriteLine("Digite seu próximo comando:");
                command = Console.ReadLine();
            }
            
            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine("  PROGRAMA FINALIZADO");
            Console.WriteLine("================================================");
            Console.WriteLine();
            Console.WriteLine("RESUMO DOS PROBLEMAS DEMONSTRADOS:");
            Console.WriteLine("• Lista estática impede coleta pelo GC");
            Console.WriteLine("• FileStream/StreamWriter sem using statements");
            Console.WriteLine("• Objetos acumulam indefinidamente na memória");
            Console.WriteLine("• Handles de arquivo não liberados adequadamente");
            Console.WriteLine();
            Console.WriteLine($"ATENÇÃO: {DataProcessor.GetProcessorCount()} objetos permanecem na memória!");
            Console.WriteLine("         Eles só serão liberados quando o processo terminar.");
            Console.WriteLine();
            Console.WriteLine("Para ver as soluções, execute: example2-memory-leak-solved.cs");
            Console.WriteLine();
        }
        
        /// <summary>
        /// Exibe para o usuário uma listagem dos comandos disponíveis
        /// </summary>
        private static void ListCommands()
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("  COMANDOS DISPONÍVEIS PARA TESTE");
            Console.WriteLine("========================================");
            Console.WriteLine("list            = Mostra esta listagem de ações");
            Console.WriteLine();
            Console.WriteLine("-- Helpers de Garbage Collection --");
            Console.WriteLine("force collection = Força o garbage collector a executar");
            Console.WriteLine();
            Console.WriteLine("-- Exemplos de Vazamento de Memória --");
            Console.WriteLine("bad memory      = Demonstra vazamento com 1000 objetos (≈40MB)");
            Console.WriteLine("bad file        = Demonstra vazamento de handles (100 arquivos)");
            Console.WriteLine();
            Console.WriteLine("-- Utilitários de Limpeza --");
            Console.WriteLine("clear files     = Remove arquivos de teste criados");
            Console.WriteLine();
            Console.WriteLine("DICA: Use ferramentas de profiling para observar os vazamentos!");
            Console.WriteLine("      Visual Studio: Debug → Windows → Show Diagnostic Tools");
            Console.WriteLine("      Ou use dotMemory, PerfView, Application Insights");
            Console.WriteLine();
            Console.WriteLine("Digite 'X' para sair");
            Console.WriteLine("========================================");
        }
        
        #region Garbage Collection
        
        /// <summary>
        /// Força o garbage collector a executar, durante a demo pode ser útil para 
        /// demonstrar que objetos com referências estáticas não são coletados
        /// </summary>
        private static void ForceCollection()
        {
            Console.WriteLine("🔄 FORÇANDO GARBAGE COLLECTION...");
            Console.WriteLine();
            
            // Mostra estado antes da coleta
            long memoryBefore = GC.GetTotalMemory(false);
            Console.WriteLine($"📊 Memória antes do GC: {memoryBefore:N0} bytes");
            DataProcessor.ShowMemoryUsage();
            
            Console.WriteLine();
            Console.WriteLine("Executando GC.Collect()...");
            
            // Força coleta completa
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            Thread.Sleep(100); // Aguarda um pouco para garantir que o GC terminou
            
            // Mostra estado depois da coleta
            long memoryAfter = GC.GetTotalMemory(false);
            Console.WriteLine($"📊 Memória depois do GC: {memoryAfter:N0} bytes");
            Console.WriteLine($"📊 Diferença: {memoryAfter - memoryBefore:N0} bytes");
            
            Console.WriteLine();
            DataProcessor.ShowMemoryUsage();
            
            Console.WriteLine();
            Console.WriteLine("🚨 PROBLEMA DEMONSTRADO:");
            Console.WriteLine("   Mesmo após forçar o GC, os objetos DataProcessor não foram coletados!");
            Console.WriteLine("   Isso acontece porque a lista estática mantém referências vivas.");
            Console.WriteLine("   O GC não pode coletar objetos que ainda têm referências ativas.");
            Console.WriteLine();
            Console.WriteLine("NOTA: Execute 'bad memory' algumas vezes, depois 'force collection'");
            Console.WriteLine("      e observe que o número de objetos nunca diminui!");
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
        /// Isso pode se acumular rapidamente e causar problemas de performance e OutOfMemoryException.
        /// </remarks>
        private static void BadMemoryAllocation()
        {
            Console.WriteLine("🔴 INICIANDO DEMONSTRAÇÃO DE VAZAMENTO DE MEMÓRIA");
            Console.WriteLine("   Criando 1000 objetos DataProcessor com 10.000 inteiros cada...");
            Console.WriteLine("   Memória total que será 'vazada': ≈40MB");
            Console.WriteLine();
            
            int objectsCreated = 0;
            
            // PROBLEMA PRINCIPAL: Criamos muitos objetos que ficam na lista estática
            // Isso impede que o GC colete os objetos, causando vazamento efetivo
            for (int i = 0; i < 1000; i++)
            {
                var processor = new DataProcessor(10000);
                processor.ProcessData();
                objectsCreated++;
                
                // Mostra progresso a cada 100 objetos
                if (objectsCreated % 100 == 0)
                {
                    Console.WriteLine($"  Criados {objectsCreated} objetos...");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("✅ Completadas 1000 alocações de memória");
            Console.WriteLine("🚨 PROBLEMA CRÍTICO: Objetos nunca serão coletados pelo GC!");
            Console.WriteLine();
            Console.WriteLine("DETALHES DO VAZAMENTO:");
            Console.WriteLine($"• {DataProcessor.GetProcessorCount()} objetos DataProcessor na lista estática");
            Console.WriteLine("• Cada objeto contém array de 10.000 inteiros (≈40KB)");
            Console.WriteLine("• Lista estática impede coleta pelo Garbage Collector");
            Console.WriteLine("• Total 'vazado': ≈40MB que permanece na memória indefinidamente");
            Console.WriteLine("• Objetos ficam 'imortais' até o processo terminar");
            Console.WriteLine();
            Console.WriteLine("IMPACTO NO SISTEMA:");
            Console.WriteLine("• Memória cresce continuamente a cada execução");
            Console.WriteLine("• Em aplicações reais, pode causar OutOfMemoryException");
            Console.WriteLine("• Performance degrada com o tempo (GC pressure)");
            Console.WriteLine("• Pode causar paginação excessiva em sistemas com pouca RAM");
            Console.WriteLine();
            Console.WriteLine("NOTA: Execute este comando múltiplas vezes e depois 'force collection'");
            Console.WriteLine("      Observe que os objetos nunca são coletados!");
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
        /// exemplo 2x seguidas sem reiniciar o programa.
        /// </remarks>
        private static void BadFileHandling()
        {
            Console.WriteLine("🔴 INICIANDO DEMONSTRAÇÃO DE VAZAMENTO DE HANDLES DE ARQUIVO");
            Console.WriteLine("   Abrindo 100 arquivos sem usar using statements...");
            Console.WriteLine();
            
            if (!Directory.Exists("badfile")) 
                Directory.CreateDirectory("badfile");
            
            int successfulCreations = 0;
            int failedCreations = 0;
            
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    WriteFileBad(i);
                    successfulCreations++;
                    
                    // Mostra progresso a cada 20 arquivos
                    if ((i + 1) % 20 == 0)
                    {
                        Console.WriteLine($"  Processados {i + 1} arquivos...");
                    }
                }
                catch (Exception ex)
                {
                    failedCreations++;
                    Console.WriteLine($"  ⚠️  Erro no arquivo {i}: {ex.Message}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("✅ Processo concluído:");
            Console.WriteLine($"   • Arquivos criados com sucesso: {successfulCreations}");
            Console.WriteLine($"   • Falhas na criação: {failedCreations}");
            Console.WriteLine();
            Console.WriteLine("🚨 PROBLEMA CRÍTICO: Handles de arquivo nunca foram liberados!");
            Console.WriteLine();
            Console.WriteLine("DETALHES DO VAZAMENTO:");
            Console.WriteLine($"• {successfulCreations} FileStream e StreamWriter não foram disposed");
            Console.WriteLine("• Cada handle consome recursos do sistema operacional");
            Console.WriteLine("• Arquivos ficam 'travados' - mostram 'em uso por outro processo'");
            Console.WriteLine("• Sistema tem limite máximo de handles por processo");
            Console.WriteLine("• Pode causar IOException: 'Too many open files'");
            Console.WriteLine();
            Console.WriteLine("IMPACTO NO SISTEMA:");
            Console.WriteLine("• Windows: Limite típico de 2048 handles por processo");
            Console.WriteLine("• Linux: Limite configurável (ulimit -n), típico 1024");
            Console.WriteLine("• Após atingir limite, criação de arquivos falhará");
            Console.WriteLine("• Outros processos podem ser afetados");
            Console.WriteLine();
            Console.WriteLine("TESTE: Tente abrir alguns arquivos em 'badfile/' com editor de texto");
            Console.WriteLine("       Você verá erro 'arquivo em uso por outro processo'!");
        }
        
        /// <summary>
        /// Implementação interna de escrita de um único arquivo - COM PROBLEMA
        /// </summary>
        /// <param name="fileNumber"></param>
        private static void WriteFileBad(int fileNumber)
        {
            try
            {
                // PROBLEMA CRÍTICO: Criamos FileStream e StreamWriter mas não os liberamos
                // Isso causa vazamento de handles de arquivo - um recurso limitado do sistema
                var fs = new FileStream($"badfile/{fileNumber}-example.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var writer = new StreamWriter(fs);
                
                writer.WriteLine($"Arquivo {fileNumber} - Este handle nunca será fechado!");
                writer.WriteLine("Conteúdo adicional para simular uso real do arquivo.");
                writer.WriteLine("PROBLEMA: Dispose() nunca é chamado!");
                writer.WriteLine($"Criado em: {DateTime.Now}");
                
                // PROBLEMA: Nunca chamamos Dispose() ou usamos using statements!
                // Os handles ficam abertos indefinidamente, consumindo recursos do sistema
                // 
                // SOLUÇÃO seria:
                // fs.Dispose();
                // writer.Dispose();
                // 
                // Ou melhor ainda, usar using statements:
                // using var fs = new FileStream(...);
                // using var writer = new StreamWriter(fs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao criar arquivo {fileNumber}: {ex.Message}");
            }
        }
        
        private static void ClearFiles()
        {
            Console.WriteLine("🧹 LIMPANDO ARQUIVOS DE TESTE...");
            
            try
            {
                if (Directory.Exists("badfile"))
                {
                    // Tenta remover arquivos individuais primeiro
                    string[] files = Directory.GetFiles("badfile");
                    int removedFiles = 0;
                    int lockedFiles = 0;
                    
                    foreach (string file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            removedFiles++;
                        }
                        catch (IOException)
                        {
                            lockedFiles++;
                            // Arquivo provavelmente está com handle aberto
                        }
                    }
                    
                    Console.WriteLine($"✅ Arquivos removidos: {removedFiles}");
                    
                    if (lockedFiles > 0)
                    {
                        Console.WriteLine($"⚠️  Arquivos travados: {lockedFiles}");
                        Console.WriteLine("   Estes arquivos têm handles abertos (demonstração do vazamento!)");
                        Console.WriteLine("   Eles só podem ser removidos após o programa terminar.");
                    }
                    
                    // Tenta remover o diretório se estiver vazio
                    try
                    {
                        Directory.Delete("badfile", false);
                        Console.WriteLine("✅ Diretório 'badfile' removido");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("ℹ️  Diretório 'badfile' mantido (contém arquivos travados)");
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️  Nenhum arquivo de teste encontrado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao limpar arquivos: {ex.Message}");
            }
            
            Console.WriteLine();
        }
        
        #endregion
    }
}