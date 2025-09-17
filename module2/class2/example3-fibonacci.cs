using System;
using System.Diagnostics;

namespace FibonacciRecursivoDemo
{
    class Program
    {
        // FUNÇÃO RECURSIVA FIBONACCI - PROBLEMA CRÍTICO DE PERFORMANCE
        // Esta implementação demonstra o pior caso de recursão para profiling
        // ATENÇÃO: Consumirá 100% do CPU por vários minutos!

        // Fibonacci recursivo puro - EXTREMAMENTE INEFICIENTE
        // Esta função será o principal alvo do sampling profiler
        static long Fib(int n)
        {
            // Caso base - condições de parada da recursão
            if (n <= 1)
            {
                return n;
            }
            
            // PROBLEMA: Chamadas recursivas redundantes
            // Cada chamada gera duas novas chamadas, criando uma árvore exponencial
            // Fib(n) = Fib(n-1) + Fib(n-2)
            // Complexidade: O(2^n) - CATASTRÓFICO!
            return Fib(n - 1) + Fib(n - 2);
        }

        // Função auxiliar para demonstrar múltiplas chamadas recursivas
        // Será visível no call tree do profiler
        static long FibonacciMultiplo(int inicio, int fim)
        {
            long soma = 0;
            
            Console.WriteLine($"Calculando Fibonacci de {inicio} até {fim}...");
            
            // Loop que chama fibonacci para vários valores
            // Cada valor gerará uma árvore de recursão exponencial
            for (int i = inicio; i <= fim; i++)
            {
                Console.WriteLine($"  Calculando Fib({i})...");
                
                Stopwatch stopwatch = Stopwatch.StartNew();
                long resultado = Fib(i);
                stopwatch.Stop();
                
                Console.WriteLine($"    Fib({i}) = {resultado} (tempo: {stopwatch.ElapsedMilliseconds} ms)");
                
                soma += resultado;
            }
            
            return soma;
        }

        // Função para demonstrar diferentes padrões de chamada recursiva
        static void DemonstrarPadroesRecursivos()
        {
            Console.WriteLine("\n=== DEMONSTRAÇÃO DE PADRÕES RECURSIVOS ===");
            Console.WriteLine("Testando diferentes valores para análise do call tree...");
            
            // Valores pequenos para warm-up
            Console.WriteLine("\n1. AQUECIMENTO - Valores pequenos:");
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"Fib({i}) = {Fib(i)}");
            }
            
            // Valores médios - começam a mostrar o problema
            Console.WriteLine("\n2. VALORES MÉDIOS - Problema começa a aparecer:");
            FibonacciMultiplo(20, 25);
            
            // Valores altos - PROBLEMA CRÍTICO
            Console.WriteLine("\n3. VALORES ALTOS - PROBLEMA CRÍTICO DE PERFORMANCE:");
            Console.WriteLine("⚠️ ATENÇÃO: Os próximos cálculos levarão MUITO tempo!");
            FibonacciMultiplo(30, 35);
        }

        // Função principal de demonstração
        static void ExecutarDemonstracao()
        {
            Console.WriteLine("=== DEMONSTRAÇÃO DE PROFILING - FIBONACCI RECURSIVO ===");
            Console.WriteLine("Objetivo: Analisar performance de funções recursivas com sampling profiler");
            Console.WriteLine();
            
            Console.WriteLine("CARACTERÍSTICAS DESTA DEMONSTRAÇÃO:");
            Console.WriteLine("✓ Recursão exponencial O(2^n) - pior caso possível");
            Console.WriteLine("✓ Milhões de chamadas de função para análise");
            Console.WriteLine("✓ Call tree profundo para visualização");
            Console.WriteLine("✓ Tempo de execução crescente exponencialmente");
            Console.WriteLine();
            
            Console.WriteLine("ANÁLISE ESPERADA NO PROFILER:");
            Console.WriteLine("1. Função 'Fib()' dominará 95%+ do tempo de CPU");
            Console.WriteLine("2. Call tree mostrará profundidade e ramificações");
            Console.WriteLine("3. Número de chamadas crescerá exponencialmente");
            Console.WriteLine("4. Sampling capturará padrão recursivo claramente");
            Console.WriteLine();
            
            Console.WriteLine("CONFIGURAÇÃO DO PROBLEMA:");
            Console.WriteLine("- Algoritmo: Fibonacci recursivo puro (sem memoização)");
            Console.WriteLine("- Complexidade: O(2^n) - exponencial");
            Console.WriteLine("- Valores testados: 1 a 35");
            Console.WriteLine("- Tempo estimado: 5-15 minutos dependendo do hardware");
            Console.WriteLine();
            
            Console.WriteLine("LIMITAÇÕES DO SAMPLING:");
            Console.WriteLine("⚠️ Funções muito rápidas podem não ser capturadas");
            Console.WriteLine("⚠️ Sampling rate pode perder chamadas individuais");
            Console.WriteLine("✅ MAS: Padrão geral será claramente visível");
            Console.WriteLine();
            
            Console.WriteLine("Pressione ENTER para iniciar a demonstração...");
            Console.ReadLine();
            Console.WriteLine();
            
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            
            // Executar demonstração completa
            DemonstrarPadroesRecursivos();
            
            stopwatchTotal.Stop();
            
            Console.WriteLine();
            Console.WriteLine("=== RESULTADOS FINAIS ===");
            Console.WriteLine($"Tempo Total de Execução: {stopwatchTotal.ElapsedMilliseconds / 1000} segundos");
            Console.WriteLine();
            
            Console.WriteLine("ANÁLISE PARA PROFILING:");
            Console.WriteLine("1. HOTSPOT PRINCIPAL: função Fib() consumiu maior parte do tempo");
            Console.WriteLine("2. CALL TREE: Visualize a profundidade das chamadas recursivas");
            Console.WriteLine("3. SAMPLING RATE: Observe como o profiler capturou as chamadas");
            Console.WriteLine("4. PERFORMANCE PATTERN: Tempo cresce exponencialmente com n");
            Console.WriteLine();
            
            Console.WriteLine("PRÓXIMOS PASSOS:");
            Console.WriteLine("✓ Salve o relatório do profiler para comparação futura");
            Console.WriteLine("✓ Analise o call tree para entender a recursão");
            Console.WriteLine("✓ Compare com versão otimizada (memoização/iterativa)");
            Console.WriteLine("✓ Discuta limitações do sampling em funções curtas");
        }

        // Função adicional para testar valores extremos (OPCIONAL)
        static void TesteExtremoPerigosoFibonacci()
        {
            Console.WriteLine("\n🔥 TESTE EXTREMO - APENAS PARA DEMONSTRAÇÃO AVANÇADA 🔥");
            Console.WriteLine("⚠️ ATENÇÃO: Isso pode levar 30+ minutos para completar!");
            Console.Write("Deseja calcular Fibonacci de valores ainda maiores? (s/n): ");
            
            string resposta = Console.ReadLine();
            
            if (resposta?.ToLower().StartsWith("s") == true)
            {
                Console.WriteLine("🚨 ÚLTIMA CHANCE DE CANCELAR!");
                Console.WriteLine("Pressione ENTER para continuar ou feche a janela para cancelar...");
                Console.ReadLine();
                
                Console.WriteLine("\n💀 CALCULANDO FIBONACCI EXTREMO...");
                
                // Valores que realmente vão torturar o CPU
                for (int i = 36; i <= 40; i++)
                {
                    Console.WriteLine($"\nCalculando Fib({i}) - Prepare-se para esperar...");
                    
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    long resultado = Fib(i);
                    stopwatch.Stop();
                    
                    Console.WriteLine($"Fib({i}) = {resultado} (tempo: {stopwatch.ElapsedMilliseconds / 1000} segundos)");
                }
                
                Console.WriteLine("\n🎯 TESTE EXTREMO CONCLUÍDO!");
                Console.WriteLine("O profiler deve mostrar dados MUITO claros agora!");
            }
        }

        // Função adicional para análise comparativa de complexidade
        static void AnaliseComparativaComplexidade()
        {
            Console.WriteLine("\n=== ANÁLISE COMPARATIVA DE COMPLEXIDADE ===");
            Console.WriteLine("Demonstrando como o tempo cresce exponencialmente:");
            Console.WriteLine();
            
            Console.WriteLine("Valor | Tempo (ms) | Chamadas Estimadas | Crescimento");
            Console.WriteLine("------|------------|-------------------|------------");
            
            for (int n = 25; n <= 35; n++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                long resultado = Fib(n);
                sw.Stop();
                
                // Estimativa do número de chamadas: aproximadamente 2^n
                long chamadasEstimadas = (long)Math.Pow(2, n);
                
                Console.WriteLine($"{n,5} | {sw.ElapsedMilliseconds,10} | {chamadasEstimadas,17:N0} | {(n > 25 ? (double)sw.ElapsedMilliseconds / 1 : 1):F1}x");
                
                // Parar se estiver levando muito tempo
                if (sw.ElapsedMilliseconds > 10000) // Mais de 10 segundos
                {
                    Console.WriteLine("⚠️ Interrompendo análise - tempo excessivo detectado");
                    break;
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("OBSERVAÇÕES PARA PROFILING:");
            Console.WriteLine("- Cada incremento em 'n' dobra aproximadamente o tempo");
            Console.WriteLine("- Número de chamadas cresce exponencialmente");
            Console.WriteLine("- Profiler capturará este padrão claramente");
            Console.WriteLine("- Call tree mostrará a árvore de recursão");
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("DEMONSTRAÇÃO DE PROFILING - FIBONACCI RECURSIVO");
                Console.WriteLine("===============================================");
                Console.WriteLine();
                
                // Demonstração principal
                ExecutarDemonstracao();
                
                // Análise comparativa opcional
                Console.WriteLine("\nDeseja executar análise comparativa de complexidade? (s/n)");
                string respostaAnalise = Console.ReadLine();
                
                if (respostaAnalise?.ToLower().StartsWith("s") == true)
                {
                    AnaliseComparativaComplexidade();
                }
                
                // Teste opcional extremo
                TesteExtremoPerigosoFibonacci();
                
                Console.WriteLine();
                Console.WriteLine("INSTRUÇÕES FINAIS PARA PROFILING:");
                Console.WriteLine("1. No Visual Studio: Debug -> Performance Profiler");
                Console.WriteLine("2. Selecione 'CPU Usage' (sampling profiler)");
                Console.WriteLine("3. Execute e observe:");
                Console.WriteLine("   - Função Fib() como hotspot principal");
                Console.WriteLine("   - Call tree com profundidade recursiva");
                Console.WriteLine("   - Número total de chamadas de função");
                Console.WriteLine("   - Distribuição de tempo por função");
                Console.WriteLine("4. Compare com implementação otimizada futura");
                Console.WriteLine();
                
                Console.WriteLine("PONTOS DE DISCUSSÃO:");
                Console.WriteLine("✓ Por que o sampling profiler é ideal para funções recursivas?");
                Console.WriteLine("✓ Como interpretar o call tree em recursões profundas?");
                Console.WriteLine("✓ Limitações do sampling em funções muito rápidas");
                Console.WriteLine("✓ Diferença entre tempo exclusivo vs. inclusivo");
                Console.WriteLine("✓ Como identificar padrões de recursão ineficiente");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante execução: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}
