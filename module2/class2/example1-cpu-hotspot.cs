using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CPUHotspotDemo
{
    class Program
    {
        // FUNÇÃO EXTREMAMENTE INTENSIVA DE CPU - PROBLEMA CRÍTICO DE PERFORMANCE
        // Esta função foi projetada para ser um PESADELO de performance
        // ATENÇÃO: Esta função consumirá 100% do CPU por vários minutos!
        static double CalcularSomaVetorIntensiva(List<double> vetor)
        {
            double soma = 0.0;
            
            // Loop principal que vai DEVASTAR o CPU
            for (int i = 0; i < vetor.Count; i++)
            {
                double valor = vetor[i];
                
                // PRIMEIRA CAMADA DE TORTURA - Operações matemáticas pesadas
                for (int j = 0; j < 1000; j++)  // Aumentado de 100 para 1000
                {
                    valor = valor * 1.001 + 0.001;
                    valor = Math.Sqrt(valor * valor + 1.0);
                    valor = Math.Sin(Math.Cos(Math.Tan(valor))) + 1.0;
                    valor = Math.Log(Math.Abs(valor) + 1.0);
                    valor = Math.Pow(valor, 1.1);
                }
                
                // SEGUNDA CAMADA DE TORTURA - Loops aninhados adicionais
                for (int k = 0; k < 500; k++)  // Loop adicional para multiplicar a complexidade
                {
                    double temp = valor;
                    for (int l = 0; l < 100; l++)
                    {
                        temp = Math.Sqrt(temp * temp + k + l);
                        temp = Math.Sin(temp) * Math.Cos(temp) + 1.0;
                        temp = Math.Exp(temp / 1000.0);  // Exponencial para mais carga
                    }
                    valor += temp * 0.001;  // Pequena contribuição para não explodir o valor
                }
                
                // TERCEIRA CAMADA DE TORTURA - Operações de matriz simuladas
                for (int m = 0; m < 200; m++)
                {
                    for (int n = 0; n < 50; n++)
                    {
                        double matrixVal = valor + m * n;
                        matrixVal = Math.Sqrt(matrixVal * matrixVal + 1.0);
                        matrixVal = Math.Sin(matrixVal) + Math.Cos(matrixVal);
                        valor += matrixVal * 0.0001;
                    }
                }
                
                soma += valor;
                
                // Mostrar progresso para não parecer travado (a cada 1000 elementos)
                if (i % 1000 == 0 && i > 0)
                {
                    Console.WriteLine($"    Processando elemento {i}/{vetor.Count} (Soma parcial: {soma:F2})");
                }
            }
            
            return soma;
        }

        // Função auxiliar para processamento adicional
        // Esta função também consumirá CPU mas em menor escala
        static double ProcessamentoSecundario(List<double> vetor)
        {
            double resultado = 0.0;
            
            for (int i = 0; i < vetor.Count; i += 10)
            {
                double temp = vetor[i];
                // Operações matemáticas menos intensivas
                for (int k = 0; k < 50; k++)
                {
                    temp = temp * 0.999 + 0.1;
                    temp = Math.Log(Math.Abs(temp) + 1.0);
                }
                resultado += temp;
            }
            
            return resultado;
        }

        // Função para inicializar vetor com dados aleatórios
        static List<double> PreencherVetorAleatorio(int tamanho)
        {
            Random random = new Random();
            List<double> vetor = new List<double>(tamanho);
            
            for (int i = 0; i < tamanho; i++)
            {
                vetor.Add(random.NextDouble() * 999.0 + 1.0); // Valores entre 1.0 e 1000.0
            }
            
            return vetor;
        }

        // Função principal de demonstração
        static void ExecutarDemonstracao()
        {
            Console.WriteLine("=== DEMONSTRAÇÃO DE PROFILING - CPU HOTSPOT ===");
            Console.WriteLine("Objetivo: Identificar funções que consomem mais CPU");
            Console.WriteLine("Preparando dados para processamento intensivo...");
            
            // CONFIGURAÇÃO EXTREMA - PREPARE-SE PARA ESPERAR MUITO TEMPO!
            const int TAMANHO_VETOR_PRINCIPAL = 10000;   // Reduzido para 10k mas com MUITO mais processamento por elemento
            const int TAMANHO_VETOR_SECUNDARIO = 5000;   // Reduzido para 5k elementos
            const int NUMERO_ITERACOES = 3;              // Apenas 3 iterações (cada uma levará MINUTOS)
            
            // Preenchimento dos vetores
            List<double> vetorPrincipal = PreencherVetorAleatorio(TAMANHO_VETOR_PRINCIPAL);
            List<double> vetorSecundario = PreencherVetorAleatorio(TAMANHO_VETOR_SECUNDARIO);
            
            Console.WriteLine();
            Console.WriteLine("⚠️  ATENÇÃO: PROCESSAMENTO EXTREMAMENTE INTENSIVO! ⚠️");
            Console.WriteLine("Este programa vai consumir 100% do CPU por VÁRIOS MINUTOS!");
            Console.WriteLine("Cada elemento do vetor passa por ~65.000 operações matemáticas!");
            Console.WriteLine("Tempo estimado: 5-15 minutos dependendo do hardware");
            Console.WriteLine();
            Console.WriteLine("Configuração do problema:");
            Console.WriteLine($"- Vetor Principal: {TAMANHO_VETOR_PRINCIPAL} elementos");
            Console.WriteLine($"- Vetor Secundário: {TAMANHO_VETOR_SECUNDARIO} elementos");
            Console.WriteLine($"- Iterações: {NUMERO_ITERACOES}");
            Console.WriteLine("- Operações por elemento: ~65.000 (3 camadas de loops aninhados)");
            Console.WriteLine();
            Console.WriteLine("Pressione ENTER para iniciar o processamento (ou feche a janela para cancelar)...");
            Console.ReadLine();
            Console.WriteLine();
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            double somaTotal = 0.0;
            double processamentoTotal = 0.0;
            
            // Loop principal que será facilmente identificado no profiler
            for (int iteracao = 1; iteracao <= NUMERO_ITERACOES; iteracao++)
            {
                Console.WriteLine($"Processando iteração {iteracao}/{NUMERO_ITERACOES}...");
                
                // HOTSPOT PRINCIPAL - Esta função dominará o tempo de CPU
                double somaIteracao = CalcularSomaVetorIntensiva(vetorPrincipal);
                somaTotal += somaIteracao;
                
                // Processamento secundário - Menor impacto no CPU
                double procIteracao = ProcessamentoSecundario(vetorSecundario);
                processamentoTotal += procIteracao;
                
                // Mostrar progresso
                if (iteracao % 2 == 0)
                {
                    Console.WriteLine($"  -> Soma parcial: {somaIteracao:F2}");
                }
            }
            
            stopwatch.Stop();
            
            // Resultados finais
            Console.WriteLine();
            Console.WriteLine("=== RESULTADOS ===");
            Console.WriteLine($"Soma Total: {somaTotal:F2}");
            Console.WriteLine($"Processamento Secundário: {processamentoTotal:F2}");
            Console.WriteLine($"Tempo Total de Execução: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();
            Console.WriteLine("INSTRUÇÕES PARA PROFILING:");
            Console.WriteLine("1. Compile em modo Release para resultados de produção");
            Console.WriteLine("2. Use modo Debug para aprendizado e debugging");
            Console.WriteLine("3. No Visual Studio: Debug -> Performance Profiler");
            Console.WriteLine("4. Selecione 'CPU Usage' e execute");
            Console.WriteLine("5. A função 'CalcularSomaVetorIntensiva' deve aparecer como hotspot principal");
        }

        // Função adicional para demonstrar diferentes padrões de uso de CPU
        static void ProcessamentoMatricial()
        {
            Console.WriteLine("\n=== PROCESSAMENTO MATRICIAL ADICIONAL ===");
            Console.WriteLine("Demonstrando operações com matrizes para variar o perfil de CPU...");
            
            const int tamanhoMatriz = 800;  // Aumentado para tornar ainda mais intensivo
            double[,] matriz1 = new double[tamanhoMatriz, tamanhoMatriz];
            double[,] matriz2 = new double[tamanhoMatriz, tamanhoMatriz];
            double[,] resultado = new double[tamanhoMatriz, tamanhoMatriz];
            
            Random random = new Random();
            
            // Preencher matrizes
            for (int i = 0; i < tamanhoMatriz; i++)
            {
                for (int j = 0; j < tamanhoMatriz; j++)
                {
                    matriz1[i, j] = random.NextDouble() * 10.0;
                    matriz2[i, j] = random.NextDouble() * 10.0;
                }
            }
            
            Stopwatch sw = Stopwatch.StartNew();
            
            // Multiplicação de matrizes - operação CPU intensiva
            for (int i = 0; i < tamanhoMatriz; i++)
            {
                for (int j = 0; j < tamanhoMatriz; j++)
                {
                    double soma = 0.0;
                    for (int k = 0; k < tamanhoMatriz; k++)
                    {
                        soma += matriz1[i, k] * matriz2[k, j];
                    }
                    resultado[i, j] = soma;
                }
                
                // Mostrar progresso a cada 100 linhas
                if ((i + 1) % 100 == 0)
                {
                    Console.WriteLine($"Processando linha {i + 1}/{tamanhoMatriz}...");
                }
            }
            
            sw.Stop();
            
            // Calcular soma da diagonal para verificação
            double somaDiagonal = 0.0;
            for (int i = 0; i < tamanhoMatriz; i++)
            {
                somaDiagonal += resultado[i, i];
            }
            
            Console.WriteLine($"Multiplicação matricial concluída em {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Soma da diagonal principal: {somaDiagonal:F2}");
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("DEMONSTRAÇÃO DE PROFILING DE CPU - VERSÃO C#");
                Console.WriteLine("==============================================");
                Console.WriteLine();
                
                // Demonstração principal
                ExecutarDemonstracao();
                
                Console.WriteLine("\n⚠️  ATENÇÃO: Há processamento matricial AINDA MAIS intensivo disponível!");
                Console.WriteLine("Isso pode levar 30+ minutos para completar!");
                Console.WriteLine("Deseja executar processamento matricial adicional? (s/n)");
                string resposta = Console.ReadLine();
                
                if (resposta?.ToLower().StartsWith("s") == true)
                {
                    Console.WriteLine("🔥 Preparando o INFERNO matricial... Última chance de desistir!");
                    Console.WriteLine("Pressione ENTER para continuar ou feche a janela para escapar...");
                    Console.ReadLine();
                    ProcessamentoMatricial();
                }
                
                Console.WriteLine("\n=== RESUMO PARA PROFILING ===");
                Console.WriteLine("Funções principais a serem observadas no profiler:");
                Console.WriteLine("1. CalcularSomaVetorIntensiva() - HOTSPOT PRINCIPAL");
                Console.WriteLine("2. ProcessamentoSecundario() - Consumo moderado");
                Console.WriteLine("3. ProcessamentoMatricial() - Padrão diferente de CPU");
                Console.WriteLine();
                Console.WriteLine("CONFIGURAÇÕES RECOMENDADAS:");
                Console.WriteLine("- Build Configuration: Release (para produção)");
                Console.WriteLine("- Build Configuration: Debug (para aprendizado)");
                Console.WriteLine("- Profiler: CPU Usage no Visual Studio");
                Console.WriteLine("- Target: .NET Framework ou .NET Core/5+");
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
