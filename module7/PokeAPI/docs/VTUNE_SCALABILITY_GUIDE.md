# 📈 Guia Completo: Intel VTune para Análise de Escalabilidade

## 📚 Índice
1. [Configuração Inicial do VTune](#configuração-inicial)
2. [Análises Específicas por Conceito](#análises-por-conceito)
3. [Métricas Chave por Tipo de Problema](#métricas-chave)
4. [Workflows Recomendados](#workflows-recomendados)
5. [Interpretação de Resultados](#interpretação)

---

## ⚙️ Configuração Inicial do VTune

### 1. Preparar o Projeto
```bash
# Build em Release com símbolos de debug
dotnet build -c Release /p:DebugType=pdbonly
```

### 2. Abrir Intel VTune
1. Inicie Intel VTune Profiler
2. **Configure → Analysis Target**
   - **Application**: `PokeProfiler.UI.exe`
   - **Working Directory**: `src\PokeProfiler.UI\bin\Release\net8.0-windows`
   - **Command Line Arguments**: (deixar vazio)

### 3. Análises Disponíveis no VTune

| Análise | Quando Usar | Conceito Relacionado |
|---------|------------|---------------------|
| **Threading** | Primeira análise - visão geral | Escalabilidade geral |
| **HPC Performance** | CPU-bound, paralelismo | Balanceamento de carga |
| **Microarchitecture** | Hotspots, cache | False sharing, afinidade |
| **Memory Access** | Contenção de memória | Lock contention, cache |
| **Hotspots** | Identificar funções lentas | Gargalos gerais |

---

## 🔍 Análises por Conceito de Escalabilidade

### 1️⃣ **ESCALABILIDADE GERAL**

#### VTune Analysis: **Threading**
**Como fazer:**
```
1. New Analysis → Threading
2. Configure → Hardware Event-Based Sampling
3. Start
4. Na UI do app: Execute "Load Balance Demo" (10-50 IDs)
5. Stop Collection
```

**O que observar:**
- **Timeline View**:
  - Verde = Running (CPU ativo)
  - Vermelho = Waiting (bloqueado)
  - Azul = Idle
  
- **Bottom-up View → Group by Thread**:
  - Distribuição de tempo entre threads
  - Threads ociosas indicam má escalabilidade
  
- **Top-down Tree**:
  - Tempo em funções paralelas vs sequenciais

**Métricas Chave:**
```
✅ Boa Escalabilidade:
- CPU Time distribuído uniformemente entre threads
- Wait Time < 10% do total
- Thread Count próximo de ProcessorCount

⚠️ Má Escalabilidade:
- 1-2 threads com 80%+ do CPU Time
- Wait Time > 30%
- Thread Count >> ProcessorCount
```

**Comparação Prática:**
```bash
# Execute e compare:
1. "Sequential" → 1 thread ativa, 0% paralelismo
2. "Load Balance Demo" → N threads (N = cores), 90%+ paralelismo
3. "Oversubscription Demo" → 10×N threads, contenção visível
```

#### VTune Analysis: **Threading**
#### 🛠️ Como Fazer
### 2️⃣ **CONTENÇÃO DE LOCKS**
**Como fazer:**
```
2. Expand Hardware Events → Enable "Lock Contention"
3. Start
4. Execute "Lock Contention" strategy
5. Stop Collection
```

#### VTune Analysis: **Threading** ou **Locks and Waits**
#### 🛠️ Como Fazer

Function                          Wait Time    Wait Count
LockContentionStrategy.FetchAsync  8,500 ms    12,450
SpinLock.Enter                     5,100 ms     8,320
```

**Timeline View:**
- Vermelho (Waiting) deve ser MÍNIMO
- Verde (Running) deve dominar
#### VTune Analysis: **Threading** + **CPU Usage**
#### 🛠️ Como Fazer

📊 Lock Contention Metrics:
- Wait Time: Tempo bloqueado esperando locks
- Average Wait: Wait Time / Wait Count

✅ Baixa Contenção:
- Wait Time < 5% do Total Time
- Average Wait < 1ms

#### VTune Analysis: **Memory Access** ou **Microarchitecture**
#### 🛠️ Como Fazer
- Wait Time > 30% do Total Time
**Comparação Prática:**
# Compare no VTune:
Strategy                 Wait Time   Wait Count   Avg Wait
Lock Contention          8,500 ms    12,450       0.68 ms
Lock-Free Demo             120 ms       250       0.48 ms
Thread Local Storage        15 ms        50       0.30 ms
```
#### VTune Analysis: **HPC Performance Characterization**
#### 🛠️ Como Fazer
---


**Como fazer:**
```
1. New Analysis → Threading
2. Enable "Context Switch Analysis"
3. Start
#### VTune Analysis: **Microarchitecture** + **Platform View**
#### 🛠️ Como Fazer
5. Stop Collection
**O que observar:**
**Platform View → CPU Usage:**
```
Core 0: 95% utilization
Core 1: 92%
Core 2: 94%
Core 3: 93%
#### VTune Analysis: **Threading** + **Locks and Waits**
#### 🛠️ Como Fazer
Total Context Switches: 45,230 (muito alto!)
**Bottom-up → Group by Thread:**
```
Active Threads: 12 threads simultâneos
Context Switches per Thread: 564 avg
```

**Métricas Chave:**
```
📊 Context Switch Analysis:
- Average Thread Life

✅ Boa Configuração:
- Thread Count ≈ ProcessorCount × 2

⚠️ Oversubscription:
- Thread Count >> ProcessorCount × 5
- Context Switches > 10,000/sec
- Many short-lived threads
```
```
Oversubscription Demo:

Load Balance Demo:

---

### 4️⃣ **FALSE SHARING E CACHE**

#### VTune Analysis: **Memory Access** ou **Microarchitecture**
```
1. New Analysis → Memory Access
4. Execute estratégia sem Thread Local
5. Stop e compare com Thread Local Storage
```
**O que observar:**

**Memory Access View:**
```
Cache Metric                     Without TLS    With TLS
L1 Cache Hit Rate                    65%          92%
Memory Bound                         25%           5%
```
**Bottom-up → Memory Objects:**
```
Object                  L1 Misses    L2 Misses    False Sharing
```

```
Memory Bandwidth Usage:
Without Thread Local: [saturado]
With Thread Local:    [eficiente]
```

**Métricas Chave:**
```
📊 Cache Performance:
- L1 Hit Rate: > 90% = bom
- L2 Hit Rate: > 80% = bom
- Memory Bound: < 10% = bom
- False Sharing: eventos de invalidação

✅ Sem False Sharing:
- Alta taxa de cache hits
- Baixa invalidação de cache lines
- Memory Bound < 10%

⚠️ Com False Sharing:
- Cache misses altos (> 30%)
- Muitas invalidaçães de cache
- Memory Bound > 25%
```

**Hotspot no Código (VTune Source View):**
```csharp
// ⚠️ False Sharing - threads modificam _results compartilhado
_results.Add(pokemon);  // Cache line invalidation!

// ✅ Thread Local - cada thread tem sua lista
_threadLocalResults.Value!.Add(pokemon);  // Sem invalidação
```

---

### 5️⃣ **BALANCEAMENTO DE CARGA**


**Como fazer:**
```
1. New Analysis → HPC Performance Characterization
2. Start
3. Execute "Load Balance Demo"
4. Stop Collection
```

**O que observar:**

**Platform View → Thread Utilization:**
```
Thread Utilization Histogram:
100% | ████████████  (ideal - todas threads ativas)
 75% | ████████
 50% | █████
 25% | ██
  0% |_______________
     Thread 0 1 2 3 4 5 6 7
```

**Bottom-up → CPU Time by Thread:**
```
Thread      CPU Time    % of Total    Load Balance
Thread 0    1,250 ms       12.5%         Balanced
Thread 1    1,240 ms       12.4%         
Thread 2    1,255 ms       12.5%         
Thread 3    1,248 ms       12.5%         
...
Standard Deviation: 15 ms  (baixo = bom)
```

**Comparação com Desbalanceado:**
```
Desbalanceado:
Thread 0    8,500 ms       85.0%         Overloaded!
Thread 1      450 ms        4.5%         Idle
Thread 2      380 ms        3.8%         Idle
...
Standard Deviation: 2,850 ms  (muito alto!)
```

```
📊 Load Balance Metrics:
- CPU Time Standard Deviation
- Thread Utilization %
- Idle Time per Thread

✅ Bem Balanceado:
- Std Dev < 100ms
- Utilization > 90% em todas threads
- Idle Time < 5%

⚠️ Desbalanceado:
- Std Dev > 1,000ms
- 1-2 threads com > 70% do trabalho
- Outras threads > 50% idle
```

---

### 6️⃣ **AFINIDADE DE THREADS E NÚCLEOS**

#### VTune Analysis: **Microarchitecture** + **Platform View**

**Como fazer:**
```
1. New Analysis → Microarchitecture Exploration
2. Start
3. Execute qualquer estratégia paralela
4. Stop Collection
```

**O que observar:**

**Platform View → CPU Utilization:**
```
Core Utilization Over Time:
Core 0: Thread A, B migrou de Core 2
Core 1: Thread C
Core 2: Thread A migrou para Core 0
Core 3: Thread D

**Bottom-up → Filter by Thread → Column "CPU":**
```
Thread    CPU 0    CPU 1    CPU 2    CPU 3    Migrations
Thread A   45%       5%      40%      10%         12
Thread B   90%       5%       3%       2%          3
Thread C    2%      95%       2%       1%          2
```

**Métricas Chave:**
```
📊 Thread Affinity Metrics:
- Thread Migrations: quantas vezes mudou de core
- Time on Primary Core: % tempo no core principal
- Cache Misses after Migration

✅ Boa Afinidade:
- Migrations < 10 por thread
- Time on Primary Core > 80%
- Poucos cache misses

⚠️ Má Afinidade:
- Migrations > 50
- Thread "saltando" entre cores
- Cache misses após migração
```

**Como Melhorar (código):**
```csharp
// VTune mostrará menos migrações com Thread Local
// Dados ficam na cache do core atual
```

---
#### VTune Analysis: **Threading** + **Locks and Waits**

**Como fazer:**
```
4. Execute "SpinLock Contention"
5. Stop e compare com "Lock Contention"

**O que observar:**

**Bottom-up → Synchronization Objects:**

**CPU Usage During Contention:**
Total Waste:   [alto consumo CPU]
Monitor (Lock):
CPU Usage: [baixo - threads dormem]
Wait Time:     [mais bloqueio]
Total Waste:   [menos desperdício]
```

**Métricas Chave:**
```
📊 SpinLock vs Lock:

SpinLock:
✅ Bom para: Seções críticas muito curtas (< 100µs)
⚠️ Ruim para: Seções longas ou alta contenção
- Spin Time (tempo girando): deve ser mínimo
- CPU Utilization: 100% mesmo esperando

Lock Tradicional (Monitor):
✅ Bom para: Seções críticas médias/longas
✅ Bom para: Alta contenção
- Wait Time: threads bloqueadas liberam CPU
- Context Switches: mais frequentes, mas eficiente
```

**Recomendação do VTune:**
```
VTune Warning:
⚠️ "High spin time detected in SpinLock.Enter"
   Consider using Monitor.Enter for longer critical sections
```

---

## 🧭 Workflows Recomendados por Problema

### Workflow 1: "Minha aplicação não escala com mais núcleos"

```
Step 1: Threading Analysis
- Verificar Thread Count vs Core Count
- Identificar threads ociosas
→ Se Wait Time alto: Workflow 2 (Contenção)
→ Se threads desbalanceadas: Workflow 3 (Balanceamento)

Step 2: HPC Performance
- Analisar paralelismo efetivo
- Medir speedup real vs ideal
```

### Workflow 2: "Muita contenção de locks"

```
Step 1: Threading + Locks and Waits
- Identificar locks com alta contenção
- Medir Wait Time por lock

Step 2: Memory Access
- Verificar false sharing
- Analisar cache misses

Solução:
- Usar estruturas lock-free (ConcurrentQueue)
- Thread Local Storage
- Reduzir escopo de locks
```

### Workflow 3: "Threads desbalanceadas"

```
Step 1: HPC Performance Characterization
- Medir CPU Time por thread
- Calcular Standard Deviation

Step 2: Bottom-up ? CPU Time by Function
- Identificar funçães que dominam tempo
- Verificar distribuição de trabalho

Solução:
 - Particionamento dinâmico (SemaphoreSlim)
- Batching (processar em lotes)
```

### Workflow 4: "Alto uso de CPU mas baixo throughput"

```
Step 1: Microarchitecture Exploration
- Verificar context switches
- Analisar thread migrations

Step 2: Threading
- Contar threads vs cores
→ Se threads >> cores: Oversubscription!

Solução:
- Reduzir número de threads
- Usar ThreadPool.SetMinThreads()
```

---

## 📊 Métricas Chave e Valores de Referência

### CPU e Paralelismo
```
Métrica                        Bom        Aceitável    Ruim
CPU Utilization                > 90%      70-90%       < 70%
Thread Count / Core Count      1.0-2.0    2.0-4.0      > 5.0
Parallel Efficiency            > 80%      60-80%       < 60%
Context Switches/sec           < 1,000    1K-10K       > 10K
```

### Contenção e Sincronização
```
Métrica                        Bom        Aceitável    Ruim
Wait Time / Total Time         < 5%       5-15%        > 15%
Lock Contention Count          < 100      100-1K       > 1K
Average Wait Time              < 1ms      1-10ms       > 10ms
Spinlock Spin Time             < 100µs    100µs-1ms    > 1ms
```

### Cache e Memória
```
Métrica                        Bom        Aceitável    Ruim
L1 Cache Hit Rate              > 95%      90-95%       < 90%
L2 Cache Hit Rate              > 85%      75-85%       < 75%
Memory Bound                   < 10%      10-25%       > 25%
False Sharing Events           0          < 10         > 10
```

### Balanceamento
```
Métrica                              Bom        Aceitável    Ruim
CPU Time Std Deviation               < 100ms    100-500ms    > 500ms
Thread Utilization (all threads)     > 90%      70-90%       < 70%
Max Thread Time / Min Thread Time    < 1.2x     1.2-2.0x     > 2.0x
```

---

## 🧪 Interpretação de Resultados Práticos

### Cenário 1: "Oversubscription Demo"

**VTune Threading Analysis mostra:**
```
• Thread Count: 80 (ProcessorCount = 8)  → 10× excesso!
• Context Switches: 45,230 (4,523/sec)
• CPU Utilization: 95% mas baixo throughput
• Wait Time: 35% do total

Diagnóstico: OVERSUBSCRIPTION severa
Solução: Reduzir threads para ProcessorCount ou usar Load Balance Demo
```

### Cenário 2: "Lock Contention"

**VTune Locks and Waits mostra:**
```
• Wait Time: 8,500ms (65% do total)
• Lock Contention Count: 12,450
• Average Wait: 0.68ms por lock
• Função: LockContentionStrategy.FetchAsync

Diagnóstico: Alta contenção em lock compartilhado
Solução: Usar Lock-Free Demo (ConcurrentQueue)
```

### Cenário 3: "Load Balance Demo" (IDEAL)

**VTune HPC Performance mostra:**
```
• Thread Count: 8 (= ProcessorCount)
• CPU Time Std Dev: 15ms
• Thread Utilization: 92% (média)
• Wait Time: 3% do total
• Context Switches: 450 (45/sec)

Diagnóstico: ESCALABILIDADE EXCELENTE
Resultado: Speedup próximo do ideal
```

---

## 📝 Exercícios Práticos com VTune

### Exercício 1: Comparação Threading
```
1. Execute VTune Threading em "Sequential"
2. Execute VTune Threading em "Load Balance Demo"
3. Compare side-by-side:
   - Thread Count
   - CPU Utilization per Thread
   - Timeline (cores ociosos)
4. Calcule: Speedup = Time(Sequential) / Time(LoadBalance)
5. Compare com speedup ideal (ProcessorCount)
```

### Exercício 2: Identificar Contenção
```
1. Execute "Lock Contention" com VTune Locks and Waits
2. Identifique:
   - Função com maior Wait Time
   - Lock com mais contençães
   - Average Wait Time
3. Execute "Lock-Free Demo"
4. Compare reduçães:
   - Wait Time reduction %
   - Contention Count reduction %
   - Throughput improvement
```

### Exercício 3: Cache Performance
```
1. Execute "Task.WhenAll" com Memory Access analysis
2. Execute "Thread Local Storage"
3. Compare:
   - L1/L2 Cache Hit Rates
   - Memory Bound %
   - False Sharing events
4. Explique: Por que Thread Local tem melhor cache performance?
```

---

## 💡 Dicas Práticas

### Configurar VTune para .NET
```
1. Install Intel VTune Profiler
2. Ensure .NET 8 SDK installed
3. Build com símbolos: dotnet build -c Release /p:DebugType=pdbonly
4. VTune → Configure → .NET Profiling → Enable
```

### Melhores Práticas
```
• Sempre compare: problema vs solução
• Execute múltiplas vezes para médias
• Use mesmos inputs (ex: "1-50")
• Feche outros aplicativos durante profiling
• Salve resultados para comparação futura
```

### Atalhos úteis VTune
```
F5              - Start Analysis
Shift+F5        - Stop Collection
Ctrl+B          - Bottom-up View
Ctrl+T          - Top-down Tree
Ctrl+L          - Timeline View
Ctrl+F          - Find Function
```

---

## 📖 Recursos Adicionais

### Intel VTune Documentation
- [Threading Analysis Guide](https://software.intel.com/content/www/us/en/develop/documentation/vtune-help/top/analyze-performance/threading-analysis.html)
- [HPC Performance Characterization](https://software.intel.com/content/www/us/en/develop/documentation/vtune-help/top/analyze-performance/hpc-performance-characterization.html)
- [Memory Access Analysis](https://software.intel.com/content/www/us/en/develop/documentation/vtune-help/top/analyze-performance/memory-access-analysis.html)

### Complemento com Visual Studio
```
Combine:
- VTune para análise detalhada de CPU/cache
- Visual Studio Concurrency Visualizer para timeline visual
- PerfView para .NET-specific metrics
```

---

## 📋 Checklist Final

Antes da aula:
- [ ] Instalar Intel VTune Profiler
- [ ] Build do projeto em Release com PDB
- [ ] Testar análises em cada estratégia
- [ ] Gerar screenshots para slides
- [ ] Preparar datasets de teste (1-10, 1-50, 1-100)

Durante a aula:
- [ ] Demonstrar Threading analysis em 2-3 estratégias
- [ ] Mostrar comparação side-by-side
- [ ] Explicar métricas principais
- [ ] Alunos executam exercícios práticos

---

**🧾 Resumo Executivo para Professores:**

| Conceito | VTune Analysis | Métrica Principal | Comparar |
|----------|---------------|-------------------|----------|
| Escalabilidade Geral | Threading | CPU Utilization, Thread Count | Sequential vs LoadBalance |
| Contenção | Locks and Waits | Wait Time, Contention Count | LockContention vs LockFree |
| Oversubscription | Threading + CPU Usage | Context Switches, Thread Count | Oversubscription vs LoadBalance |
| Cache/False Sharing | Memory Access | Cache Hit Rate, Memory Bound | Normal vs ThreadLocal |
| Balanceamento | HPC Performance | CPU Time Std Dev | Manual vs LoadBalance |

**Tempo estimado de profiling por análise:** 2-5 minutos  
**Total para demonstração completa:** ~30 minutos

