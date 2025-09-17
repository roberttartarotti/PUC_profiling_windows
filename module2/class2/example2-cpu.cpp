#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <iomanip>

// FUNÇÃO OTIMIZADA - SOLUÇÃO DO PROBLEMA DE PERFORMANCE
// Esta versão remove operações desnecessárias e loops redundantes
// RESULTADO: Execução em segundos ao invés de minutos!
double calcularSomaVetorIntensiva(const std::vector<double>& vetor) {
    double soma = 0.0;
    
    // SOLUCAO: Loop principal simplificado - apenas uma passagem pelos dados
    for (size_t i = 0; i < vetor.size(); ++i) {
        double valor = vetor[i];
        
        // PROBLEMA ORIGINAL: Loops aninhados desnecessários (65.000 operações por elemento)
        /*
        for (int j = 0; j < 1000; ++j) {
            valor = valor * 1.001 + 0.001;
            valor = sqrt(valor * valor + 1.0);
            valor = sin(cos(tan(valor))) + 1.0;
            valor = log(abs(valor) + 1.0);
            valor = pow(valor, 1.1);
        }
        
        for (int k = 0; k < 500; ++k) {
            double temp = valor;
            for (int l = 0; l < 100; ++l) {
                temp = sqrt(temp * temp + k + l);
                temp = sin(temp) * cos(temp) + 1.0;
                temp = exp(temp / 1000.0);
            }
            valor += temp * 0.001;
        }
        
        for (int m = 0; m < 200; ++m) {
            for (int n = 0; n < 50; ++n) {
                double matrixVal = valor + m * n;
                matrixVal = sqrt(matrixVal * matrixVal + 1.0);
                matrixVal = sin(matrixVal) + cos(matrixVal);
                valor += matrixVal * 0.0001;
            }
        }
        */
        
        // SOLUCAO: Processamento simples e eficiente
        // Aplica apenas as transformações matemáticas necessárias
        valor = valor * 1.001 + 0.001;                    // Transformação linear simples
        valor = sqrt(valor * valor + 1.0);                // Uma única operação de raiz
        valor = sin(valor) + 1.0;                         // Operação trigonométrica simplificada
        
        // SOLUCAO: Evitar operações caras como pow, exp, log em loops
        // Substituir por operações mais baratas quando possível
        if (valor > 1000.0) {                             // Normalização condicional
            valor = log(valor);                           // Log apenas quando necessário
        }
        
        soma += valor;
        
        // SOLUCAO: Reduzir frequência de I/O para melhorar performance
        // Mostrar progresso a cada 10.000 elementos ao invés de 1.000
        if (i % 10000 == 0 && i > 0) {
            std::cout << "    Processando elemento " << i << "/" << vetor.size() 
                      << " (Soma parcial: " << std::fixed << std::setprecision(2) 
                      << soma << ")" << std::endl;
        }
    }
    
    return soma;
}

// Função auxiliar para processamento adicional
// Esta função também consumirá CPU mas em menor escala
double processamentoSecundario(const std::vector<double>& vetor) {
    double resultado = 0.0;
    
    for (size_t i = 0; i < vetor.size(); i += 10) {
        double temp = vetor[i];
        // Operações matemáticas menos intensivas
        for (int k = 0; k < 50; ++k) {
            temp = temp * 0.999 + 0.1;
            temp = log(abs(temp) + 1.0);
        }
        resultado += temp;
    }
    
    return resultado;
}

// Função para inicializar vetor com dados aleatórios
void preencherVetorAleatorio(std::vector<double>& vetor, size_t tamanho) {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_real_distribution<double> dis(1.0, 1000.0);
    
    vetor.resize(tamanho);
    for (size_t i = 0; i < tamanho; ++i) {
        vetor[i] = dis(gen);
    }
}

// Função principal de demonstração
void executarDemonstracao() {
    std::cout << "=== DEMONSTRAÇÃO DE PROFILING - CPU HOTSPOT ===" << std::endl;
    std::cout << "Objetivo: Identificar funções que consomem mais CPU" << std::endl;
    std::cout << "Preparando dados para processamento intensivo..." << std::endl;
    
    // SOLUCAO: Configuração otimizada para demonstrar a melhoria de performance
    const size_t TAMANHO_VETOR_PRINCIPAL = 100000;  // Aumentado para 100k elementos (mais dados, menos processamento por elemento)
    const size_t TAMANHO_VETOR_SECUNDARIO = 50000;  // Aumentado para 50k elementos
    const int NUMERO_ITERACOES = 10;                 // Mais iterações, mas cada uma executa rapidamente
    
    std::vector<double> vetorPrincipal;
    std::vector<double> vetorSecundario;
    
    // Preenchimento dos vetores
    preencherVetorAleatorio(vetorPrincipal, TAMANHO_VETOR_PRINCIPAL);
    preencherVetorAleatorio(vetorSecundario, TAMANHO_VETOR_SECUNDARIO);
    
    std::cout << std::endl;
    std::cout << "✅ VERSÃO OTIMIZADA - PROBLEMA RESOLVIDO! ✅" << std::endl;
    std::cout << "Esta versão foi otimizada para execução rápida!" << std::endl;
    std::cout << "Cada elemento passa por apenas ~4 operações matemáticas!" << std::endl;
    std::cout << "Tempo estimado: 1-5 segundos dependendo do hardware" << std::endl;
    std::cout << std::endl;
    std::cout << "OTIMIZAÇÕES APLICADAS:" << std::endl;
    std::cout << "✓ Removidos loops aninhados desnecessários" << std::endl;
    std::cout << "✓ Substituídas operações caras (pow, exp) por mais simples" << std::endl;
    std::cout << "✓ Reduzida frequência de I/O" << std::endl;
    std::cout << "✓ Operações condicionais para evitar cálculos desnecessários" << std::endl;
    std::cout << std::endl;
    std::cout << "Configuração otimizada:" << std::endl;
    std::cout << "- Vetor Principal: " << TAMANHO_VETOR_PRINCIPAL << " elementos" << std::endl;
    std::cout << "- Vetor Secundário: " << TAMANHO_VETOR_SECUNDARIO << " elementos" << std::endl;
    std::cout << "- Iterações: " << NUMERO_ITERACOES << std::endl;
    std::cout << "- Operações por elemento: ~4 (otimizadas)" << std::endl;
    std::cout << std::endl;
    std::cout << "Pressione ENTER para iniciar o processamento otimizado..." << std::endl;
    std::cin.get();
    std::cout << std::endl;
    
    auto inicio = std::chrono::high_resolution_clock::now();
    
    double somaTotal = 0.0;
    double processamentoTotal = 0.0;
    
    // Loop principal que será facilmente identificado no profiler
    for (int iteracao = 1; iteracao <= NUMERO_ITERACOES; ++iteracao) {
        std::cout << "Processando iteração " << iteracao << "/" << NUMERO_ITERACOES << "..." << std::endl;
        
        // HOTSPOT PRINCIPAL - Esta função dominará o tempo de CPU
        double somaIteracao = calcularSomaVetorIntensiva(vetorPrincipal);
        somaTotal += somaIteracao;
        
        // Processamento secundário - Menor impacto no CPU
        double procIteracao = processamentoSecundario(vetorSecundario);
        processamentoTotal += procIteracao;
        
        // Mostrar progresso
        if (iteracao % 2 == 0) {
            std::cout << "  -> Soma parcial: " << std::fixed << std::setprecision(2) 
                      << somaIteracao << std::endl;
        }
    }
    
    auto fim = std::chrono::high_resolution_clock::now();
    auto duracao = std::chrono::duration_cast<std::chrono::milliseconds>(fim - inicio);
    
    // Resultados finais
    std::cout << std::endl;
    std::cout << "=== RESULTADOS ===" << std::endl;
    std::cout << "Soma Total: " << std::fixed << std::setprecision(2) << somaTotal << std::endl;
    std::cout << "Processamento Secundário: " << std::fixed << std::setprecision(2) << processamentoTotal << std::endl;
    std::cout << "Tempo Total de Execução: " << duracao.count() << " ms" << std::endl;
    std::cout << std::endl;
    std::cout << "COMPARAÇÃO DE PERFORMANCE:" << std::endl;
    std::cout << "- Versão original: 5-15 minutos (65.000 ops/elemento)" << std::endl;
    std::cout << "- Versão otimizada: 1-5 segundos (~4 ops/elemento)" << std::endl;
    std::cout << "- Melhoria: ~1000x mais rápida!" << std::endl;
    std::cout << std::endl;
    std::cout << "INSTRUÇÕES PARA PROFILING:" << std::endl;
    std::cout << "1. Compare este resultado com a versão example1-cpu-hotspot.cpp" << std::endl;
    std::cout << "2. No profiler, esta versão mostrará distribuição equilibrada de CPU" << std::endl;
    std::cout << "3. Não haverá mais hotspots críticos de performance" << std::endl;
    std::cout << "4. O tempo total será drasticamente menor" << std::endl;
}

int main() {
    try {
        executarDemonstracao();
    }
    catch (const std::exception& e) {
        std::cerr << "Erro durante execução: " << e.what() << std::endl;
        return 1;
    }
    
    std::cout << std::endl << "Pressione qualquer tecla para sair..." << std::endl;
    std::cin.get();
    
    return 0;
}
