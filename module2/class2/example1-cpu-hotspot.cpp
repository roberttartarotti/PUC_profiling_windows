#include <iostream>
#include <vector>
#include <chrono>
#include <random>
#include <iomanip>

// FUNÇÃO EXTREMAMENTE INTENSIVA DE CPU - PROBLEMA CRÍTICO DE PERFORMANCE
// Esta função foi projetada para ser um PESADELO de performance
// ATENÇÃO: Esta função consumirá 100% do CPU por vários minutos!
double calcularSomaVetorIntensiva(const std::vector<double>& vetor) {
    double soma = 0.0;
    
    // Loop principal que vai DEVASTAR o CPU
    for (size_t i = 0; i < vetor.size(); ++i) {
        double valor = vetor[i];
        
        // PRIMEIRA CAMADA DE TORTURA - Operações matemáticas pesadas
        for (int j = 0; j < 1000; ++j) {  // Aumentado de 100 para 1000
            valor = valor * 1.001 + 0.001;
            valor = sqrt(valor * valor + 1.0);
            valor = sin(cos(tan(valor))) + 1.0;
            valor = log(abs(valor) + 1.0);
            valor = pow(valor, 1.1);
        }
        
        // SEGUNDA CAMADA DE TORTURA - Loops aninhados adicionais
        for (int k = 0; k < 500; ++k) {  // Loop adicional para multiplicar a complexidade
            double temp = valor;
            for (int l = 0; l < 100; ++l) {
                temp = sqrt(temp * temp + k + l);
                temp = sin(temp) * cos(temp) + 1.0;
                temp = exp(temp / 1000.0);  // Exponencial para mais carga
            }
            valor += temp * 0.001;  // Pequena contribuição para não explodir o valor
        }
        
        // TERCEIRA CAMADA DE TORTURA - Operações de matriz simuladas
        for (int m = 0; m < 200; ++m) {
            for (int n = 0; n < 50; ++n) {
                double matrixVal = valor + m * n;
                matrixVal = sqrt(matrixVal * matrixVal + 1.0);
                matrixVal = sin(matrixVal) + cos(matrixVal);
                valor += matrixVal * 0.0001;
            }
        }
        
        soma += valor;
        
        // Mostrar progresso para não parecer travado (a cada 1000 elementos)
        if (i % 1000 == 0 && i > 0) {
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
    
    // CONFIGURAÇÃO EXTREMA - PREPARE-SE PARA ESPERAR MUITO TEMPO!
    const size_t TAMANHO_VETOR_PRINCIPAL = 10000;   // Reduzido para 10k mas com MUITO mais processamento por elemento
    const size_t TAMANHO_VETOR_SECUNDARIO = 5000;   // Reduzido para 5k elementos
    const int NUMERO_ITERACOES = 3;                  // Apenas 3 iterações (cada uma levará MINUTOS)
    
    std::vector<double> vetorPrincipal;
    std::vector<double> vetorSecundario;
    
    // Preenchimento dos vetores
    preencherVetorAleatorio(vetorPrincipal, TAMANHO_VETOR_PRINCIPAL);
    preencherVetorAleatorio(vetorSecundario, TAMANHO_VETOR_SECUNDARIO);
    
    std::cout << std::endl;
    std::cout << "⚠️  ATENÇÃO: PROCESSAMENTO EXTREMAMENTE INTENSIVO! ⚠️" << std::endl;
    std::cout << "Este programa vai consumir 100% do CPU por VÁRIOS MINUTOS!" << std::endl;
    std::cout << "Cada elemento do vetor passa por ~65.000 operações matemáticas!" << std::endl;
    std::cout << "Tempo estimado: 5-15 minutos dependendo do hardware" << std::endl;
    std::cout << std::endl;
    std::cout << "Configuração do problema:" << std::endl;
    std::cout << "- Vetor Principal: " << TAMANHO_VETOR_PRINCIPAL << " elementos" << std::endl;
    std::cout << "- Vetor Secundário: " << TAMANHO_VETOR_SECUNDARIO << " elementos" << std::endl;
    std::cout << "- Iterações: " << NUMERO_ITERACOES << std::endl;
    std::cout << "- Operações por elemento: ~65.000 (3 camadas de loops aninhados)" << std::endl;
    std::cout << std::endl;
    std::cout << "Pressione ENTER para iniciar o processamento (ou Ctrl+C para cancelar)..." << std::endl;
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
    std::cout << "INSTRUÇÕES PARA PROFILING:" << std::endl;
    std::cout << "1. Compile em modo Release para resultados de produção" << std::endl;
    std::cout << "2. Use modo Debug para aprendizado e debugging" << std::endl;
    std::cout << "3. No Visual Studio: Debug -> Performance Profiler" << std::endl;
    std::cout << "4. Selecione 'CPU Usage' e execute" << std::endl;
    std::cout << "5. A função 'calcularSomaVetorIntensiva' deve aparecer como hotspot principal" << std::endl;
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
