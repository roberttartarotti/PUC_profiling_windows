#include <iostream>
#include <chrono>
#include <iomanip>
#include <vector>

// FUNÇÃO RECURSIVA FIBONACCI - PROBLEMA CRÍTICO DE PERFORMANCE
// Esta implementação demonstra o pior caso de recursão para profiling
// ATENÇÃO: Consumirá 100% do CPU por vários minutos!

// Fibonacci recursivo puro - EXTREMAMENTE INEFICIENTE
// Esta função será o principal alvo do sampling profiler
long long fib(int n) {
    // Caso base - condições de parada da recursão
    if (n <= 1) {
        return n;
    }
    
    // PROBLEMA: Chamadas recursivas redundantes
    // Cada chamada gera duas novas chamadas, criando uma árvore exponencial
    // fib(n) = fib(n-1) + fib(n-2)
    // Complexidade: O(2^n) - CATASTRÓFICO!
    return fib(n - 1) + fib(n - 2);
}

// Função auxiliar para demonstrar múltiplas chamadas recursivas
// Será visível no call tree do profiler
long long fibonacciMultiplo(int inicio, int fim) {
    long long soma = 0;
    
    std::cout << "Calculando Fibonacci de " << inicio << " até " << fim << "..." << std::endl;
    
    // Loop que chama fibonacci para vários valores
    // Cada valor gerará uma árvore de recursão exponencial
    for (int i = inicio; i <= fim; ++i) {
        std::cout << "  Calculando fib(" << i << ")..." << std::endl;
        
        auto inicioTempo = std::chrono::high_resolution_clock::now();
        long long resultado = fib(i);
        auto fimTempo = std::chrono::high_resolution_clock::now();
        
        auto duracao = std::chrono::duration_cast<std::chrono::milliseconds>(fimTempo - inicioTempo);
        
        std::cout << "    fib(" << i << ") = " << resultado 
                  << " (tempo: " << duracao.count() << " ms)" << std::endl;
        
        soma += resultado;
    }
    
    return soma;
}

// Função para demonstrar diferentes padrões de chamada recursiva
void demonstrarPadroesRecursivos() {
    std::cout << "\n=== DEMONSTRAÇÃO DE PADRÕES RECURSIVOS ===" << std::endl;
    std::cout << "Testando diferentes valores para análise do call tree..." << std::endl;
    
    // Valores pequenos para warm-up
    std::cout << "\n1. AQUECIMENTO - Valores pequenos:" << std::endl;
    for (int i = 1; i <= 10; ++i) {
        std::cout << "fib(" << i << ") = " << fib(i) << std::endl;
    }
    
    // Valores médios - começam a mostrar o problema
    std::cout << "\n2. VALORES MÉDIOS - Problema começa a aparecer:" << std::endl;
    fibonacciMultiplo(20, 25);
    
    // Valores altos - PROBLEMA CRÍTICO
    std::cout << "\n3. VALORES ALTOS - PROBLEMA CRÍTICO DE PERFORMANCE:" << std::endl;
    std::cout << "⚠️ ATENÇÃO: Os próximos cálculos levarão MUITO tempo!" << std::endl;
    fibonacciMultiplo(30, 35);
}

// Função principal de demonstração
void executarDemonstracao() {
    std::cout << "=== DEMONSTRAÇÃO DE PROFILING - FIBONACCI RECURSIVO ===" << std::endl;
    std::cout << "Objetivo: Analisar performance de funções recursivas com sampling profiler" << std::endl;
    std::cout << std::endl;
    
    std::cout << "CARACTERÍSTICAS DESTA DEMONSTRAÇÃO:" << std::endl;
    std::cout << "✓ Recursão exponencial O(2^n) - pior caso possível" << std::endl;
    std::cout << "✓ Milhões de chamadas de função para análise" << std::endl;
    std::cout << "✓ Call tree profundo para visualização" << std::endl;
    std::cout << "✓ Tempo de execução crescente exponencialmente" << std::endl;
    std::cout << std::endl;
    
    std::cout << "ANÁLISE ESPERADA NO PROFILER:" << std::endl;
    std::cout << "1. Função 'fib()' dominará 95%+ do tempo de CPU" << std::endl;
    std::cout << "2. Call tree mostrará profundidade e ramificações" << std::endl;
    std::cout << "3. Número de chamadas crescerá exponencialmente" << std::endl;
    std::cout << "4. Sampling capturará padrão recursivo claramente" << std::endl;
    std::cout << std::endl;
    
    std::cout << "CONFIGURAÇÃO DO PROBLEMA:" << std::endl;
    std::cout << "- Algoritmo: Fibonacci recursivo puro (sem memoização)" << std::endl;
    std::cout << "- Complexidade: O(2^n) - exponencial" << std::endl;
    std::cout << "- Valores testados: 1 a 35" << std::endl;
    std::cout << "- Tempo estimado: 5-15 minutos dependendo do hardware" << std::endl;
    std::cout << std::endl;
    
    std::cout << "LIMITAÇÕES DO SAMPLING:" << std::endl;
    std::cout << "⚠️ Funções muito rápidas podem não ser capturadas" << std::endl;
    std::cout << "⚠️ Sampling rate pode perder chamadas individuais" << std::endl;
    std::cout << "✅ MAS: Padrão geral será claramente visível" << std::endl;
    std::cout << std::endl;
    
    std::cout << "Pressione ENTER para iniciar a demonstração..." << std::endl;
    std::cin.get();
    std::cout << std::endl;
    
    auto inicioTotal = std::chrono::high_resolution_clock::now();
    
    // Executar demonstração completa
    demonstrarPadroesRecursivos();
    
    auto fimTotal = std::chrono::high_resolution_clock::now();
    auto duracaoTotal = std::chrono::duration_cast<std::chrono::seconds>(fimTotal - inicioTotal);
    
    std::cout << std::endl;
    std::cout << "=== RESULTADOS FINAIS ===" << std::endl;
    std::cout << "Tempo Total de Execução: " << duracaoTotal.count() << " segundos" << std::endl;
    std::cout << std::endl;
    
    std::cout << "ANÁLISE PARA PROFILING:" << std::endl;
    std::cout << "1. HOTSPOT PRINCIPAL: função fib() consumiu maior parte do tempo" << std::endl;
    std::cout << "2. CALL TREE: Visualize a profundidade das chamadas recursivas" << std::endl;
    std::cout << "3. SAMPLING RATE: Observe como o profiler capturou as chamadas" << std::endl;
    std::cout << "4. PERFORMANCE PATTERN: Tempo cresce exponencialmente com n" << std::endl;
    std::cout << std::endl;
    
    std::cout << "PRÓXIMOS PASSOS:" << std::endl;
    std::cout << "✓ Salve o relatório do profiler para comparação futura" << std::endl;
    std::cout << "✓ Analise o call tree para entender a recursão" << std::endl;
    std::cout << "✓ Compare com versão otimizada (memoização/iterativa)" << std::endl;
    std::cout << "✓ Discuta limitações do sampling em funções curtas" << std::endl;
}

// Função adicional para testar valores extremos (OPCIONAL)
void testeExtremoPerigosoFibonacci() {
    std::cout << "\n🔥 TESTE EXTREMO - APENAS PARA DEMONSTRAÇÃO AVANÇADA 🔥" << std::endl;
    std::cout << "⚠️ ATENÇÃO: Isso pode levar 30+ minutos para completar!" << std::endl;
    std::cout << "Deseja calcular Fibonacci de valores ainda maiores? (s/n): ";
    
    char resposta;
    std::cin >> resposta;
    
    if (resposta == 's' || resposta == 'S') {
        std::cout << "🚨 ÚLTIMA CHANCE DE CANCELAR!" << std::endl;
        std::cout << "Pressione ENTER para continuar ou Ctrl+C para cancelar..." << std::endl;
        std::cin.ignore();
        std::cin.get();
        
        std::cout << "\n💀 CALCULANDO FIBONACCI EXTREMO..." << std::endl;
        
        // Valores que realmente vão torturar o CPU
        for (int i = 36; i <= 40; ++i) {
            std::cout << "\nCalculando fib(" << i << ") - Prepare-se para esperar..." << std::endl;
            
            auto inicio = std::chrono::high_resolution_clock::now();
            long long resultado = fib(i);
            auto fim = std::chrono::high_resolution_clock::now();
            
            auto duracao = std::chrono::duration_cast<std::chrono::seconds>(fim - inicio);
            
            std::cout << "fib(" << i << ") = " << resultado 
                      << " (tempo: " << duracao.count() << " segundos)" << std::endl;
        }
        
        std::cout << "\n🎯 TESTE EXTREMO CONCLUÍDO!" << std::endl;
        std::cout << "O profiler deve mostrar dados MUITO claros agora!" << std::endl;
    }
}

int main() {
    try {
        std::cout << "DEMONSTRAÇÃO DE PROFILING - FIBONACCI RECURSIVO" << std::endl;
        std::cout << "===============================================" << std::endl;
        std::cout << std::endl;
        
        // Demonstração principal
        executarDemonstracao();
        
        // Teste opcional extremo
        testeExtremoPerigosoFibonacci();
        
        std::cout << std::endl;
        std::cout << "INSTRUÇÕES FINAIS PARA PROFILING:" << std::endl;
        std::cout << "1. No Visual Studio: Debug -> Performance Profiler" << std::endl;
        std::cout << "2. Selecione 'CPU Usage' (sampling profiler)" << std::endl;
        std::cout << "3. Execute e observe:" << std::endl;
        std::cout << "   - Função fib() como hotspot principal" << std::endl;
        std::cout << "   - Call tree com profundidade recursiva" << std::endl;
        std::cout << "   - Número total de chamadas de função" << std::endl;
        std::cout << "   - Distribuição de tempo por função" << std::endl;
        std::cout << "4. Compare com implementação otimizada futura" << std::endl;
        
    } catch (const std::exception& e) {
        std::cerr << "Erro durante execução: " << e.what() << std::endl;
        return 1;
    }
    
    std::cout << std::endl << "Pressione qualquer tecla para sair..." << std::endl;
    std::cin.ignore();
    std::cin.get();
    
    return 0;
}
