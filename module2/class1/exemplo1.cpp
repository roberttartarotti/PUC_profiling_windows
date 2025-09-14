
/*
================================================================================
ATIVIDADE PRÁTICA 1 - PROFILING BÁSICO DE CPU COM FUNÇÃO LENTA (C++)
================================================================================

OBJETIVO:
- Criar aplicação com código que causa overhead de CPU
- Executar fora do profiler para observar tempo de execução
- Usar Performance Profiler com CPU Usage habilitado
- Identificar função que consome mais tempo de CPU
- Corrigir código removendo atraso e validar ganhos

PROBLEMA:
- Função factorial contém loop interno artificial que causa overhead
- CPU Usage Tool mostrará hotspot na função slow_factorial()

SOLUÇÃO:
- Remover o loop artificial interno (linhas do delay)
- Resultado: dramatica redução no tempo de execução

================================================================================
*/

#include <iostream>
#include <chrono>
using namespace std;

long long slow_factorial(int n) {
    long long result = 1;
    for (int i = 1; i <= n; i++) {
        for (int j = 0; j < 100000; j++); // PERFORMANCE BOTTLENECK: Remove this artificial delay loop to fix CPU overhead
        result *= i;
    }
    return result;
}

int main() {
    auto start = chrono::high_resolution_clock::now();
    cout << "Factorial(20) = " << slow_factorial(20) << endl;
    auto end = chrono::high_resolution_clock::now();
    cout << "Time = " << chrono::duration_cast<chrono::milliseconds>(end - start).count() << " ms" << endl;
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR A VERSÃO OTIMIZADA)
================================================================================

#include <iostream>
#include <chrono>
using namespace std;

long long fast_factorial(int n) {
    long long result = 1;
    for (int i = 1; i <= n; i++) {
        // CORREÇÃO: Removido o loop artificial que causava overhead
        result *= i;
    }
    return result;
}

int main() {
    auto start = chrono::high_resolution_clock::now();
    cout << "Factorial(20) = " << fast_factorial(20) << endl;
    auto end = chrono::high_resolution_clock::now();
    cout << "Time = " << chrono::duration_cast<chrono::milliseconds>(end - start).count() << " ms" << endl;
    return 0;
}

================================================================================
*/
