/*
 * Exemplo 2 - Vazamento de Memória (Memory Leak) - PROBLEMA
 * 
 * NOTA: Este código é fornecido apenas para fins de demonstração. Ele contém 
 * intencionalmente problemas de vazamento de memória para auxiliar na 
 * demonstração de profiling de performance e detecção de memory leaks.
 * 
 * Objetivo do Exercício:
 * Este exemplo demonstra problemas comuns de vazamento de memória em C++.
 * Vamos criar um programa interativo que permite testar diferentes cenários
 * de vazamento de memória que podem levar ao esgotamento da memória do sistema.
 * 
 * O que vamos fazer:
 * 1. Criar uma classe que aloca memória no construtor mas não implementa destrutor
 * 2. Demonstrar vazamento de objetos criados com 'new' sem 'delete' correspondente
 * 3. Mostrar vazamento de handles de arquivo não fechados adequadamente
 * 4. Usar interface interativa para testar diferentes cenários
 * 5. Observar como a memória não é liberada usando ferramentas de profiling
 * 
 * Comandos disponíveis:
 * - bad memory: Cria 1000 objetos com vazamento de memória
 * - bad file: Abre 100 arquivos sem fechar os handles
 * - clear files: Limpa arquivos de teste criados
 * 
 * Problemas demonstrados:
 * - Destrutor não implementado causando vazamento de arrays
 * - Objetos criados com 'new' nunca liberados com 'delete'
 * - Handles de arquivo abertos com fopen() nunca fechados com fclose()
 * - Acúmulo progressivo de memória não liberada
 */

#include <iostream>
#include <vector>
#include <thread>
#include <chrono>
#include <string>

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
    
    void processData() {
        long sum = 0;
        for (size_t i = 0; i < size; i++) {
            sum += data[i];
        }
        std::cout << "Soma calculada: " << sum << std::endl;
    }
    
    // PROBLEMA: Destrutor não implementado!
    // Isso causa vazamento de memória porque o array 'data' nunca é liberado
    // ~DataProcessor() {
    //     delete[] data;
    // }
};

void listCommands() {
    std::cout << std::endl;
    std::cout << "Os seguintes comandos estão disponíveis:" << std::endl;
    std::cout << "list = Mostra esta listagem de ações" << std::endl;
    std::cout << "-- Exemplos de Vazamento de Memória --" << std::endl;
    std::cout << "bad memory = Demonstra vazamento de memória com 1000 objetos" << std::endl;
    std::cout << "bad file = Demonstra vazamento de handles de arquivo" << std::endl;
    std::cout << "-- Helpers de Limpeza --" << std::endl;
    std::cout << "clear files = Limpa arquivos de teste criados" << std::endl;
    std::cout << std::endl;
    std::cout << "Pressione X para sair" << std::endl;
}

void badMemoryAllocation() {
    std::cout << "Iniciando Alocação Ruim de Memória" << std::endl;
    std::vector<DataProcessor*> processors;
    
    // PROBLEMA: Criamos objetos com 'new' mas nunca fazemos 'delete'
    // Isso resulta em vazamento tanto dos objetos quanto dos arrays internos
    for (int i = 0; i < 1000; i++) {
        DataProcessor* processor = new DataProcessor(10000);
        processor->processData();
        processors.push_back(processor);
    }
    
    std::cout << "Completadas 1000 alocações de memória" << std::endl;
    std::cout << "PROBLEMA: Memória nunca será liberada!" << std::endl;
    
    // PROBLEMA: Não liberamos a memória dos objetos criados!
    // for (auto* processor : processors) {
    //     delete processor;
    // }
}

void badFileHandling() {
    std::cout << "Iniciando Manipulação Ruim de Arquivos" << std::endl;
    
    for (int i = 0; i < 100; i++) {
        // PROBLEMA: Abrimos arquivos mas nunca os fechamos
        // Isso causa vazamento de handles de arquivo
        FILE* file = nullptr;
        std::string filename = "badfile_" + std::to_string(i) + ".txt";
        errno_t err = fopen_s(&file, filename.c_str(), "w");
        if (err == 0 && file) {
            fprintf(file, "Arquivo %d - vazamento de handle!\n", i);
            // PROBLEMA: Nunca chamamos fclose(file)!
        }
    }
    
    std::cout << "Completadas 100 aberturas de arquivo" << std::endl;
    std::cout << "PROBLEMA: Handles de arquivo nunca foram fechados!" << std::endl;
}

void clearFiles() {
    std::cout << "Limpando arquivos de teste..." << std::endl;
    
    for (int i = 0; i < 100; i++) {
        std::string filename = "badfile_" + std::to_string(i) + ".txt";
        remove(filename.c_str());
    }
    
    std::cout << "Arquivos de teste removidos." << std::endl;
}

int main() {
    std::cout << "Bem-vindo à demonstração de vazamento de memória" << std::endl;
    listCommands();
    clearFiles();
    
    std::string command;
    std::getline(std::cin, command);
    
    while (command != "x" && command != "X") {
        if (command == "list") {
            listCommands();
        }
        else if (command == "bad memory") {
            badMemoryAllocation();
        }
        else if (command == "bad file") {
            badFileHandling();
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
    
    return 0;
}
