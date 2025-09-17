#include <iostream>
#include <thread>
#include <vector>
#include <chrono>
#include <mutex>
#include <atomic>
#include <future>
#include <iomanip>
#include <random>
#include <cmath>

// ANÁLISE MULTITHREAD COM SAMPLING PROFILER
// Demonstra distribuição de CPU entre múltiplas threads
// Objetivo: Observar como o profiler captura atividade paralela

// Mutex para sincronizar saída no console
std::mutex consoleMutex;
std::atomic<int> contadorGlobal{0};

// Função computacionalmente intensiva para simular trabalho real
// Esta função será executada por múltiplas threads simultaneamente
double calcularTrabalhoIntensivo(int threadId, int inicio, int fim, const std::string& tipoTrabalho) {
    double resultado = 0.0;
    
    // Simulação de diferentes tipos de processamento paralelo
    if (tipoTrabalho == "matematico") {
        // Processamento matemático intensivo
        for (int i = inicio; i <= fim; ++i) {
            double valor = static_cast<double>(i);
            
            // Operações matemáticas complexas
            for (int j = 0; j < 1000; ++j) {
                valor = std::sin(valor) * std::cos(valor);
                valor = std::sqrt(valor * valor + 1.0);
                valor = std::log(std::abs(valor) + 1.0);
            }
            
            resultado += valor;
            contadorGlobal++;
            
            // Mostrar progresso periodicamente
            if (i % 1000 == 0) {
                std::lock_guard<std::mutex> lock(consoleMutex);
                std::cout << "  Thread " << threadId << " processando: " << i 
                          << "/" << fim << " (resultado parcial: " 
                          << std::fixed << std::setprecision(2) << resultado << ")" << std::endl;
            }
        }
    }
    else if (tipoTrabalho == "simulacao") {
        // Simulação de processamento de dados/servidor
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_real_distribution<double> dis(1.0, 100.0);
        
        for (int i = inicio; i <= fim; ++i) {
            // Simular processamento de requisições/dados
            double dados = dis(gen);
            
            // Processamento simulado
            for (int k = 0; k < 500; ++k) {
                dados = std::pow(dados, 1.1);
                dados = std::sqrt(dados);
                dados = std::sin(dados) + std::cos(dados);
            }
            
            resultado += dados;
            contadorGlobal++;
            
            // Simular variação de carga de trabalho
            if (i % 2 == 0) {
                std::this_thread::sleep_for(std::chrono::microseconds(10));
            }
            
            if (i % 1500 == 0) {
                std::lock_guard<std::mutex> lock(consoleMutex);
                std::cout << "  Thread " << threadId << " (simulação) processando: " 
                          << i << "/" << fim << std::endl;
            }
        }
    }
    
    {
        std::lock_guard<std::mutex> lock(consoleMutex);
        std::cout << "✓ Thread " << threadId << " concluída! Resultado: " 
                  << std::fixed << std::setprecision(2) << resultado << std::endl;
    }
    
    return resultado;
}

// Função para executar processamento paralelo com std::thread
void demonstracaoThreadsBasicas() {
    std::cout << "\n=== DEMONSTRAÇÃO: THREADS BÁSICAS (std::thread) ===" << std::endl;
    std::cout << "Criando múltiplas threads para processamento paralelo..." << std::endl;
    
    const int NUM_THREADS = std::thread::hardware_concurrency();
    const int TRABALHO_POR_THREAD = 5000;
    
    std::cout << "Configuração:" << std::endl;
    std::cout << "- Número de threads: " << NUM_THREADS << " (baseado no hardware)" << std::endl;
    std::cout << "- Trabalho por thread: " << TRABALHO_POR_THREAD << " iterações" << std::endl;
    std::cout << "- Tipo de processamento: Matemático intensivo" << std::endl;
    std::cout << std::endl;
    
    auto inicioTempo = std::chrono::high_resolution_clock::now();
    
    // Criar e iniciar threads
    std::vector<std::thread> threads;
    std::vector<double> resultados(NUM_THREADS);
    
    for (int i = 0; i < NUM_THREADS; ++i) {
        int inicio = i * TRABALHO_POR_THREAD;
        int fim = inicio + TRABALHO_POR_THREAD - 1;
        
        threads.emplace_back([i, inicio, fim, &resultados]() {
            resultados[i] = calcularTrabalhoIntensivo(i, inicio, fim, "matematico");
        });
    }
    
    // Aguardar todas as threads terminarem
    for (auto& t : threads) {
        t.join();
    }
    
    auto fimTempo = std::chrono::high_resolution_clock::now();
    auto duracao = std::chrono::duration_cast<std::chrono::milliseconds>(fimTempo - inicioTempo);
    
    // Calcular resultado final
    double resultadoTotal = 0.0;
    for (double r : resultados) {
        resultadoTotal += r;
    }
    
    std::cout << std::endl;
    std::cout << "RESULTADOS THREADS BÁSICAS:" << std::endl;
    std::cout << "- Tempo total: " << duracao.count() << " ms" << std::endl;
    std::cout << "- Resultado combinado: " << std::fixed << std::setprecision(2) << resultadoTotal << std::endl;
    std::cout << "- Operações processadas: " << contadorGlobal.load() << std::endl;
}

// Função para demonstrar async/future para tarefas assíncronas
void demonstracaoAsyncFuture() {
    std::cout << "\n=== DEMONSTRAÇÃO: ASYNC/FUTURE (std::async) ===" << std::endl;
    std::cout << "Usando std::async para processamento assíncrono..." << std::endl;
    
    contadorGlobal = 0;
    const int NUM_TAREFAS = 6;
    const int TRABALHO_POR_TAREFA = 3000;
    
    std::cout << "Configuração:" << std::endl;
    std::cout << "- Número de tarefas assíncronas: " << NUM_TAREFAS << std::endl;
    std::cout << "- Trabalho por tarefa: " << TRABALHO_POR_TAREFA << " iterações" << std::endl;
    std::cout << "- Tipo de processamento: Simulação de servidor" << std::endl;
    std::cout << std::endl;
    
    auto inicioTempo = std::chrono::high_resolution_clock::now();
    
    // Criar tarefas assíncronas
    std::vector<std::future<double>> futures;
    
    for (int i = 0; i < NUM_TAREFAS; ++i) {
        int inicio = i * TRABALHO_POR_TAREFA;
        int fim = inicio + TRABALHO_POR_TAREFA - 1;
        
        futures.push_back(
            std::async(std::launch::async, [i, inicio, fim]() {
                return calcularTrabalhoIntensivo(i, inicio, fim, "simulacao");
            })
        );
    }
    
    // Coletar resultados
    std::vector<double> resultados;
    for (auto& future : futures) {
        resultados.push_back(future.get());
    }
    
    auto fimTempo = std::chrono::high_resolution_clock::now();
    auto duracao = std::chrono::duration_cast<std::chrono::milliseconds>(fimTempo - inicioTempo);
    
    // Calcular resultado final
    double resultadoTotal = 0.0;
    for (double r : resultados) {
        resultadoTotal += r;
    }
    
    std::cout << std::endl;
    std::cout << "RESULTADOS ASYNC/FUTURE:" << std::endl;
    std::cout << "- Tempo total: " << duracao.count() << " ms" << std::endl;
    std::cout << "- Resultado combinado: " << std::fixed << std::setprecision(2) << resultadoTotal << std::endl;
    std::cout << "- Operações processadas: " << contadorGlobal.load() << std::endl;
}

// Simulação de processamento de servidor web multithread
void simulacaoServidorWeb() {
    std::cout << "\n=== SIMULAÇÃO: SERVIDOR WEB MULTITHREAD ===" << std::endl;
    std::cout << "Simulando processamento de requisições HTTP paralelas..." << std::endl;
    
    contadorGlobal = 0;
    const int NUM_WORKERS = 8;
    const int REQUISICOES_POR_WORKER = 1000;
    
    std::cout << "Cenário do servidor:" << std::endl;
    std::cout << "- Workers threads: " << NUM_WORKERS << std::endl;
    std::cout << "- Requisições por worker: " << REQUISICOES_POR_WORKER << std::endl;
    std::cout << "- Total de requisições: " << NUM_WORKERS * REQUISICOES_POR_WORKER << std::endl;
    std::cout << std::endl;
    
    auto inicioTempo = std::chrono::high_resolution_clock::now();
    
    // Simular pool de threads de servidor
    std::vector<std::thread> workers;
    std::atomic<int> requisicoesConcluidas{0};
    
    for (int workerId = 0; workerId < NUM_WORKERS; ++workerId) {
        workers.emplace_back([workerId, REQUISICOES_POR_WORKER, &requisicoesConcluidas]() {
            std::random_device rd;
            std::mt19937 gen(rd());
            std::uniform_int_distribution<int> tempoProcessamento(100, 1000);
            std::uniform_real_distribution<double> cargaProcessamento(1.0, 10.0);
            
            for (int req = 0; req < REQUISICOES_POR_WORKER; ++req) {
                // Simular processamento de requisição HTTP
                double carga = cargaProcessamento(gen);
                double resultado = 0.0;
                
                // Processamento variável por requisição
                int iteracoes = tempoProcessamento(gen);
                for (int i = 0; i < iteracoes; ++i) {
                    resultado += std::sin(carga * i) * std::cos(carga * i);
                    resultado = std::sqrt(resultado * resultado + 1.0);
                }
                
                requisicoesConcluidas++;
                contadorGlobal++;
                
                // Log periódico
                if (req % 200 == 0 && req > 0) {
                    std::lock_guard<std::mutex> lock(consoleMutex);
                    std::cout << "  Worker " << workerId << " processou " << req 
                              << " requisições (total global: " << requisicoesConcluidas.load() << ")" << std::endl;
                }
                
                // Simular tempo de I/O ocasional
                if (req % 50 == 0) {
                    std::this_thread::sleep_for(std::chrono::microseconds(100));
                }
            }
            
            std::lock_guard<std::mutex> lock(consoleMutex);
            std::cout << "✓ Worker " << workerId << " finalizou todas as requisições!" << std::endl;
        });
    }
    
    // Aguardar todos os workers
    for (auto& worker : workers) {
        worker.join();
    }
    
    auto fimTempo = std::chrono::high_resolution_clock::now();
    auto duracao = std::chrono::duration_cast<std::chrono::milliseconds>(fimTempo - inicioTempo);
    
    std::cout << std::endl;
    std::cout << "RESULTADOS SIMULAÇÃO SERVIDOR:" << std::endl;
    std::cout << "- Tempo total: " << duracao.count() << " ms" << std::endl;
    std::cout << "- Requisições processadas: " << requisicoesConcluidas.load() << std::endl;
    std::cout << "- Throughput: " << (requisicoesConcluidas.load() * 1000.0 / duracao.count()) << " req/s" << std::endl;
}

// Função principal de demonstração
void executarDemonstracao() {
    std::cout << "=== ANÁLISE MULTITHREAD COM SAMPLING PROFILER ===" << std::endl;
    std::cout << "Objetivo: Observar distribuição de CPU entre threads paralelas" << std::endl;
    std::cout << std::endl;
    
    std::cout << "CENÁRIOS DE ANÁLISE:" << std::endl;
    std::cout << "1. Threads básicas (std::thread) - Processamento matemático" << std::endl;
    std::cout << "2. Tarefas assíncronas (std::async) - Simulação de processamento" << std::endl;
    std::cout << "3. Simulação de servidor web - Pool de workers" << std::endl;
    std::cout << std::endl;
    
    std::cout << "ANÁLISE ESPERADA NO PROFILER:" << std::endl;
    std::cout << "✓ CPU distribuído entre múltiplas threads" << std::endl;
    std::cout << "✓ Hotspots em funções de trabalho paralelo" << std::endl;
    std::cout << "✓ Contenção em pontos de sincronização" << std::endl;
    std::cout << "✓ Padrões diferentes para cada tipo de paralelismo" << std::endl;
    std::cout << std::endl;
    
    std::cout << "HARDWARE DETECTADO:" << std::endl;
    std::cout << "- Cores disponíveis: " << std::thread::hardware_concurrency() << std::endl;
    std::cout << "- Threads simultâneas recomendadas: " << std::thread::hardware_concurrency() << std::endl;
    std::cout << std::endl;
    
    std::cout << "Pressione ENTER para iniciar as demonstrações..." << std::endl;
    std::cin.get();
    std::cout << std::endl;
    
    // Executar todas as demonstrações
    demonstracaoThreadsBasicas();
    demonstracaoAsyncFuture();
    simulacaoServidorWeb();
}

int main() {
    try {
        std::cout << "DEMONSTRAÇÃO MULTITHREAD - SAMPLING PROFILER" << std::endl;
        std::cout << "============================================" << std::endl;
        std::cout << std::endl;
        
        // Demonstração principal
        executarDemonstracao();
        
        std::cout << std::endl;
        std::cout << "=== INSTRUÇÕES PARA ANÁLISE NO PROFILER ===" << std::endl;
        std::cout << std::endl;
        std::cout << "1. CONFIGURAÇÃO DO PROFILER:" << std::endl;
        std::cout << "   - Use 'CPU Usage' (sampling profiler)" << std::endl;
        std::cout << "   - Ative 'Show threads' ou 'Thread view'" << std::endl;
        std::cout << "   - Configure sampling rate adequado" << std::endl;
        std::cout << std::endl;
        
        std::cout << "2. PONTOS DE ANÁLISE:" << std::endl;
        std::cout << "   ✓ Distribuição de CPU entre threads" << std::endl;
        std::cout << "   ✓ Identificação de threads mais ativas" << std::endl;
        std::cout << "   ✓ Pontos de contenção (mutex, sincronização)" << std::endl;
        std::cout << "   ✓ Padrões de execução paralela vs sequencial" << std::endl;
        std::cout << std::endl;
        
        std::cout << "3. MÉTRICAS IMPORTANTES:" << std::endl;
        std::cout << "   - Utilização total de CPU (deve ser alta)" << std::endl;
        std::cout << "   - Balanceamento entre threads" << std::endl;
        std::cout << "   - Tempo gasto em sincronização" << std::endl;
        std::cout << "   - Eficiência do paralelismo" << std::endl;
        std::cout << std::endl;
        
        std::cout << "4. APLICAÇÕES REAIS:" << std::endl;
        std::cout << "   - Servidores web (pool de threads)" << std::endl;
        std::cout << "   - Processamento de dados paralelo" << std::endl;
        std::cout << "   - Sistemas de renderização" << std::endl;
        std::cout << "   - Aplicações científicas/matemáticas" << std::endl;
        std::cout << std::endl;
        
        std::cout << "5. LIMITAÇÕES DO SAMPLING EM MULTITHREAD:" << std::endl;
        std::cout << "   ⚠️ Pode perder sincronizações muito rápidas" << std::endl;
        std::cout << "   ⚠️ Sampling rate afeta precisão em threads rápidas" << std::endl;
        std::cout << "   ✅ Excelente para identificar padrões gerais" << std::endl;
        std::cout << "   ✅ Mostra distribuição de carga efetivamente" << std::endl;
        
    } catch (const std::exception& e) {
        std::cerr << "Erro durante execução: " << e.what() << std::endl;
        return 1;
    }
    
    std::cout << std::endl << "Pressione qualquer tecla para sair..." << std::endl;
    std::cin.get();
    
    return 0;
}
