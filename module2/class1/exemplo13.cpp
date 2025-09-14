/*
================================================================================
ATIVIDADE PRÁTICA 13 - BUSY WAITING VS EVENT-DRIVEN (C++)
================================================================================

OBJETIVO:
- Demonstrar ineficiência de busy waiting (polling)
- Usar CPU profiler para identificar wasted CPU cycles
- Otimizar usando condition variables e event-driven approach
- Comparar CPU usage entre polling e blocking wait

PROBLEMA:
- Busy waiting consome CPU continuamente sem fazer trabalho útil
- Loop de polling com sleep ainda desperdiça cycles
- CPU Profiler mostrará alta utilização sem progresso

SOLUÇÃO:
- Usar condition_variable para blocking wait eficiente
- Event-driven programming reduz CPU waste

================================================================================
*/

#include <iostream>
#include <chrono>
#include <thread>
#include <atomic>
#include <mutex>
#include <condition_variable>
using namespace std;

atomic<bool> dataReady{false};
atomic<int> processedItems{0};

void dataProducer() {
    this_thread::sleep_for(chrono::seconds(3)); // Simulate work
    dataReady = true;
    cout << "Data producer: Data is ready!" << endl;
}

void demonstrateBusyWaiting() {
    cout << "Starting busy waiting demonstration..." << endl;
    cout << "Monitor CPU profiler - should see wasted CPU cycles in polling" << endl;
    
    thread producer(dataProducer);
    
    auto start = chrono::high_resolution_clock::now();
    
    // PERFORMANCE ISSUE: Busy waiting wastes CPU cycles
    while (!dataReady) {
        processedItems++; // Increment counter to show CPU work
        this_thread::sleep_for(chrono::milliseconds(1)); // Still wastes CPU with frequent wake-ups
        
        if (processedItems % 1000 == 0) {
            cout << "Busy waiting... checked " << processedItems << " times" << endl;
        }
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    producer.join();
    
    cout << "Busy waiting completed in: " << duration.count() << " ms" << endl;
    cout << "Total polling attempts: " << processedItems << endl;
    cout << "CPU wasted on unnecessary polling" << endl;
}

int main() {
    cout << "Starting busy waiting vs event-driven demonstration..." << endl;
    cout << "Task: Waiting for data to become available" << endl;
    cout << "Monitor CPU Usage Tool for polling overhead" << endl << endl;
    
    demonstrateBusyWaiting();
    
    cout << endl << "=== PROFILING ANALYSIS ===" << endl;
    cout << "Check CPU profiler for:" << endl;
    cout << "- High CPU usage during waiting period" << endl;
    cout << "- Wasted cycles in polling loop" << endl;
    cout << "- Frequent context switches due to sleep" << endl;
    
    return 0;
}

/*
================================================================================
VERSÃO CORRIGIDA (DESCOMENTE PARA USAR EVENT-DRIVEN APPROACH)
================================================================================

#include <iostream>
#include <chrono>
#include <thread>
#include <mutex>
#include <condition_variable>
using namespace std;

mutex dataMutex;
condition_variable dataCondition;
bool dataReady = false;

void eventDataProducer() {
    this_thread::sleep_for(chrono::seconds(3)); // Simulate work
    
    {
        lock_guard<mutex> lock(dataMutex);
        dataReady = true;
    }
    
    dataCondition.notify_one(); // Wake up waiting thread
    cout << "Event producer: Data is ready, notifying waiters!" << endl;
}

void demonstrateEventDriven() {
    cout << "Starting event-driven demonstration..." << endl;
    cout << "Monitor CPU profiler - should see minimal CPU usage during wait" << endl;
    
    thread producer(eventDataProducer);
    
    auto start = chrono::high_resolution_clock::now();
    
    // CORREÇÃO: Event-driven waiting - no CPU waste
    {
        unique_lock<mutex> lock(dataMutex);
        dataCondition.wait(lock, [] { return dataReady; }); // Efficient blocking wait
    }
    
    auto end = chrono::high_resolution_clock::now();
    auto duration = chrono::duration_cast<chrono::milliseconds>(end - start);
    
    producer.join();
    
    cout << "Event-driven wait completed in: " << duration.count() << " ms" << endl;
    cout << "No CPU wasted during wait period" << endl;
    cout << "Thread was blocked efficiently until notification" << endl;
}

int main() {
    cout << "Starting optimized event-driven demonstration..." << endl;
    cout << "Task: Waiting efficiently for data availability" << endl;
    cout << "Monitor CPU Usage Tool for reduced CPU consumption" << endl << endl;
    
    demonstrateEventDriven();
    
    cout << endl << "=== OPTIMIZATION RESULTS ===" << endl;
    cout << "Improvements:" << endl;
    cout << "- Zero CPU usage during wait period" << endl;
    cout << "- No wasted polling cycles" << endl;
    cout << "- Immediate response to events" << endl;
    cout << "- Better system resource utilization" << endl;
    cout << "- Scalable to many waiting threads" << endl;
    
    return 0;
}

================================================================================
*/
