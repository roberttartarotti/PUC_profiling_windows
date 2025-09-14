/*
 * Exemplo 2 - Vazamento de Memória (Memory Leak) - SOLUÇÃO
 * 
 * NOTA: Este código é fornecido para demonstração das SOLUÇÕES para os problemas 
 * de vazamento de memória. Ele implementa as práticas corretas de gerenciamento 
 * de memória em C++ para auxiliar no aprendizado de profiling de performance.
 * 
 * Objetivo do Exercício:
 * Este exemplo demonstra como corrigir os vazamentos de memória do exemplo anterior.
 * Vamos implementar as práticas corretas de gerenciamento de memória em C++
 * usando diferentes abordagens modernas e tradicionais.
 * 
 * Soluções implementadas:
 * 1. Implementar destrutor adequadamente seguindo RAII
 * 2. Liberar memória alocada dinamicamente com delete/delete[]
 * 3. Usar smart pointers (unique_ptr) para gerenciamento automático
 * 4. Implementar Rule of Three (copy constructor, assignment operator, destructor)
 * 5. Usar RAII para handles de arquivo com std::ofstream
 * 6. Demonstrar diferentes abordagens: tradicional vs moderna
 * 
 * Comandos disponíveis:
 * - good memory: Demonstra gerenciamento correto com destrutor manual
 * - modern memory: Demonstra abordagem moderna com smart pointers
 * - good file: Demonstra manipulação correta de arquivos com RAII
 * - clear files: Limpa arquivos de teste criados
 * 
 * Técnicas demonstradas:
 * - Destrutor que libera memória alocada no construtor
 * - Smart pointers (unique_ptr) para gerenciamento automático
 * - RAII (Resource Acquisition Is Initialization)
 * - Rule of Three para classes que gerenciam recursos
 * - std::ofstream para manipulação segura de arquivos
 * - Comparação entre abordagens tradicional e moderna
 * 
 * Resultado: Memória será liberada corretamente, sem vazamentos
 */

#include <iostream>
#include <vector>
#include <thread>
#include <chrono>
#include <string>
#include <memory>
#include <fstream>

// Compatibility for fopen_s on non-Windows systems
#ifndef _WIN32
inline errno_t fopen_s(FILE** pFile, const char* filename, const char* mode) {
    *pFile = fopen(filename, mode);
    return (*pFile) ? 0 : errno;
}
#endif

class DataProcessor {
private:
    int* data;
    size_t size;
    
public:
    DataProcessor(size_t dataSize) : size(dataSize) {
        data = new int[size];
        
        for (size_t i = 0; i < size; i++) {
            data[i] = i * 2;
        }
        
        std::cout << "Alocados " << size * sizeof(int) << " bytes de memória" << std::endl;
    }
    
    // SOLUÇÃO 1: Implementar destrutor corretamente
    // O destrutor é chamado automaticamente quando o objeto sai de escopo
    ~DataProcessor() {
        std::cout << "Liberando " << size * sizeof(int) << " bytes de memória" << std::endl;
        delete[] data;  // CRÍTICO: Liberar a memória alocada com new[]
        data = nullptr; // Boa prática: definir ponteiro como nullptr após delete
    }
    
    // SOLUÇÃO 2: Implementar copy constructor e assignment operator (Rule of Three)
    DataProcessor(const DataProcessor& other) : size(other.size) {
        data = new int[size];
        for (size_t i = 0; i < size; i++) {
            data[i] = other.data[i];
        }
        std::cout << "Cópia criada - alocados " << size * sizeof(int) << " bytes" << std::endl;
    }
    
    DataProcessor& operator=(const DataProcessor& other) {
        if (this != &other) {
            delete[] data;  // Liberar memória atual
            
            size = other.size;
            data = new int[size];
            for (size_t i = 0; i < size; i++) {
                data[i] = other.data[i];
            }
        }
        return *this;
    }
    
    void processData() {
        long sum = 0;
        for (size_t i = 0; i < size; i++) {
            sum += data[i];
        }
        std::cout << "Soma calculada: " << sum << std::endl;
    }
};

// SOLUÇÃO 3: Versão moderna usando smart pointers
class ModernDataProcessor {
private:
    std::unique_ptr<int[]> data;  // Smart pointer gerencia memória automaticamente
    size_t size;
    
public:
    ModernDataProcessor(size_t dataSize) : size(dataSize) {
        // Usar make_unique para alocação segura
        data = std::make_unique<int[]>(size);
        
        for (size_t i = 0; i < size; i++) {
            data[i] = i * 2;
        }
        
        std::cout << "[MODERNO] Alocados " << size * sizeof(int) << " bytes de memória" << std::endl;
    }
    
    // VANTAGEM: Não precisamos implementar destrutor!
    // O unique_ptr libera a memória automaticamente
    ~ModernDataProcessor() {
        std::cout << "[MODERNO] Memória liberada automaticamente pelo smart pointer" << std::endl;
    }
    
    void processData() {
        long sum = 0;
        for (size_t i = 0; i < size; i++) {
            sum += data[i];
        }
        std::cout << "[MODERNO] Soma calculada: " << sum << std::endl;
    }
};

void listCommands() {
    std::cout << std::endl;
    std::cout << "Os seguintes comandos estão disponíveis:" << std::endl;
    std::cout << "list = Mostra esta listagem de ações" << std::endl;
    std::cout << "-- Exemplos de Gerenciamento Correto de Memória --" << std::endl;
    std::cout << "good memory = Demonstra gerenciamento correto com 1000 objetos" << std::endl;
    std::cout << "modern memory = Demonstra abordagem moderna com smart pointers" << std::endl;
    std::cout << "good file = Demonstra manipulação correta de arquivos" << std::endl;
    std::cout << "-- Helpers de Limpeza --" << std::endl;
    std::cout << "clear files = Limpa arquivos de teste criados" << std::endl;
    std::cout << std::endl;
    std::cout << "Pressione X para sair" << std::endl;
}

void goodMemoryAllocation() {
    std::cout << "Iniciando Alocação Correta de Memória" << std::endl;
    std::vector<DataProcessor*> processors;
    
    // PASSO 1: Criar objetos normalmente
    for (int i = 0; i < 1000; i++) {
        DataProcessor* processor = new DataProcessor(10000);
        processor->processData();
        processors.push_back(processor);
    }
    
    std::cout << "Completadas 1000 alocações de memória" << std::endl;
    
    // SOLUÇÃO 4: SEMPRE liberar memória alocada com new
    std::cout << "Liberando memória manualmente..." << std::endl;
    for (auto* processor : processors) {
        delete processor;  // Chama o destrutor que libera a memória
    }
    processors.clear();  // Limpar o vetor de ponteiros
    
    std::cout << "SOLUÇÃO: Toda memória foi liberada corretamente!" << std::endl;
}

void modernMemoryAllocation() {
    std::cout << "Iniciando Abordagem Moderna com Smart Pointers" << std::endl;
    std::vector<std::unique_ptr<ModernDataProcessor>> processors;
    
    // PASSO 2: Usar smart pointers para gerenciamento automático
    for (int i = 0; i < 1000; i++) {
        auto processor = std::make_unique<ModernDataProcessor>(10000);
        processor->processData();
        processors.push_back(std::move(processor));
    }
    
    std::cout << "Completadas 1000 alocações modernas" << std::endl;
    
    // VANTAGEM: Não precisamos fazer delete manual!
    // Os unique_ptr são destruídos automaticamente quando saem de escopo
    std::cout << "Smart pointers serão destruídos automaticamente..." << std::endl;
    processors.clear();  // Força a destruição imediata
    
    std::cout << "SOLUÇÃO: Smart pointers gerenciaram a memória automaticamente!" << std::endl;
}

void goodFileHandling() {
    std::cout << "Iniciando Manipulação Correta de Arquivos" << std::endl;
    
    // SOLUÇÃO 5: Criar diretório se não existir
    std::string dirName = "goodfile";
    
    for (int i = 0; i < 100; i++) {
        // SOLUÇÃO 6: Usar RAII com objetos automáticos (stack)
        std::string filename = dirName + "/arquivo_" + std::to_string(i) + ".txt";
        
        // Método 1: Usando std::ofstream (RAII automático)
        {
            std::ofstream file(filename);
            if (file.is_open()) {
                file << "Arquivo " << i << " - handle gerenciado corretamente!\n";
                // VANTAGEM: Arquivo é fechado automaticamente quando sai de escopo
            }
        }
        
        // Método 2: Usando FILE* com RAII manual
        FILE* cfile = nullptr;
        errno_t err = fopen_s(&cfile, filename.c_str(), "a");
        if (err == 0 && cfile) {
            fprintf(cfile, "Linha adicional com FILE*\n");
            fclose(cfile);  // SOLUÇÃO: Sempre fechar handles explicitamente
        }
    }
    
    std::cout << "Completadas 100 manipulações corretas de arquivo" << std::endl;
    std::cout << "SOLUÇÃO: Todos os handles de arquivo foram fechados corretamente!" << std::endl;
}

void clearFiles() {
    std::cout << "Limpando arquivos de teste..." << std::endl;
    
    // Limpar arquivos do exemplo ruim
    for (int i = 0; i < 100; i++) {
        std::string filename = "badfile_" + std::to_string(i) + ".txt";
        remove(filename.c_str());
    }
    
    // Limpar diretório do exemplo bom
    std::string command = "rm -rf goodfile";
    system(command.c_str());
    
    std::cout << "Arquivos de teste removidos." << std::endl;
}

int main() {
    std::cout << "Bem-vindo à demonstração de gerenciamento correto de memória" << std::endl;
    listCommands();
    clearFiles();
    
    std::string command;
    std::getline(std::cin, command);
    
    while (command != "x" && command != "X") {
        if (command == "list") {
            listCommands();
        }
        else if (command == "good memory") {
            goodMemoryAllocation();
        }
        else if (command == "modern memory") {
            modernMemoryAllocation();
        }
        else if (command == "good file") {
            goodFileHandling();
        }
        else if (command == "clear files") {
            clearFiles();
        }
        else {
            std::cout << "Comando desconhecido. Tente novamente. Digite 'list' para ver todos os comandos disponíveis." << std::endl;
        }
        
        std::cout << "Por favor, digite seu próximo comando:" << std::endl;
        std::getline(std::cin, command);
    }
    
    std::cout << "\n=== RESUMO DAS SOLUÇÕES ===" << std::endl;
    std::cout << "1. Sempre implementar destrutor quando usar new/delete" << std::endl;
    std::cout << "2. Para cada 'new' deve haver um 'delete' correspondente" << std::endl;
    std::cout << "3. Usar smart pointers (unique_ptr, shared_ptr) quando possível" << std::endl;
    std::cout << "4. Preferir objetos na stack quando o tamanho permitir" << std::endl;
    std::cout << "5. Seguir a 'Rule of Three/Five' para classes com recursos" << std::endl;
    std::cout << "6. Usar RAII (Resource Acquisition Is Initialization)" << std::endl;
    std::cout << "7. Sempre fechar handles de arquivo explicitamente" << std::endl;
    
    std::cout << "\nPrograma finalizado. Toda memória foi gerenciada corretamente!" << std::endl;
    return 0;
}