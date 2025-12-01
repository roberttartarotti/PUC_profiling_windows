# 🔎 Guia Rápido - Cenários de Investigação

## 🎯 Objetivo
Usar Intel VTune para identificar e analisar problemas de performance em aplicações multithreading.

## 📋 Preparação

### 1. Build em Release
```pwsh
dotnet build "PUC.PokeProfiler.sln" -c Release
```

### 2. Iniciar VTune
- Abra Intel VTune Profiler
- Create New Project → "PokeProfiler"
- Analysis Target: `dotnet.exe`
- Application Parameters: `run --project .\src\PokeProfiler.UI\PokeProfiler.UI.csproj -c Release --no-build`

---

## 🔍 Cenários de Investigação

### Cenário 1: Memory Leak 🧠
**Estratégia**: `⚠ Memory Leak`
**Input**: `1-50`
**VTune Analysis Type**: Memory Consumption *Linux ou Python*

**O que procurar**:
- Heap crescendo continuamente
- Objetos não coletados pelo GC
- Gen2 collections aumentando

**Pergunta**: Por que o GC não libera a memória?

---

### Cenário 2: GC Pressure 💥
**Estratégia**: `⚠ Excessive Alloc`
**Input**: `1-20`
**VTune Analysis Type**: Hotspots

**O que procurar**:
- Tempo em GC collections
- Taxa de alocação (MB/s)
- Gen0/Gen1 frequency

**Pergunta**: Quantas alocações temporárias são feitas por Pokémon?

---

### Cenário 3: CPU Spinning 🔄
**Estratégia**: `⚠ CPU Spin` vs `Sequential`
**Input**: `1-10` + Delay: 200ms
**VTune Analysis Type**: Threading

**O que procurar**:
- CPU utilization durante I/O wait
- Thread.SpinWait no flamegraph
- Diferença de consumo energético

**Pergunta**: Por que o CPU está a 100% esperando rede?

---

### Cenário 4: Deadlock 🔒
**Estratégia**: `⚠ Deadlock Risk`
**Input**: `1-20` (executar múltiplas vezes)
**VTune Analysis Type**: Threading

**O que procurar**:
- Threads blocked em locks
- Wait chain circular
- Lock ordering problem

**Pergunta**: Qual é o padrão que causa o deadlock?

---

### Cenário 5: Algoritmo Ineficiente 🐌
**Estratégia**: `⚠ Inefficient Algorithm` vs `Sequential`
**Input**: `1-30`
**VTune Analysis Type**: Hotspots

**O que procurar**:
- Regex constructor no flamegraph
- String concatenation overhead
- O(n²) behavior

**Pergunta**: Qual operação domina o tempo de CPU?

---

### Cenário 6: Lock Contention ⚔️
**Estratégia**: `Lock Contention`
**Input**: `1-100`
**VTune Analysis Type**: Threading + Hotspots

**O que procurar**:
- Lock wait time %
- Serialization de threads
- BusyWork hotspot

**Pergunta**: Quanto tempo é perdido esperando locks?

---

## 🛠️ Controles Avançados

### ThreadPool Configuration
- **MinThreads**: Testar com 1, 4, 16
- **Observar**: Thread starvation e ramp-up time

### Artificial Delay
- **0ms**: Baseline local
- **50-200ms**: Simular latência de rede
- **Observar**: I/O wait time vs CPU time

### ActivitySource Tracing
- **Habilitado**: Correlacionar com VTune timeline
- **Observar**: Async task flow e handoffs

---

## 📊 Métricas Importantes

### Hotspots Analysis
- **Top Functions**: Onde o CPU está sendo usado
- **Call Stack**: Quem chama as funções lentas
- **Self Time**: Tempo gasto na função (sem filhas)

### Threading Analysis
- **CPU Time**: Tempo executando
- **Wait Time**: Tempo bloqueado (locks, I/O)
- **Context Switches**: Frequência de troca de thread

### Memory Analysis
- **Allocation Rate**: MB/s alocados
- **GC Time**: % tempo em garbage collection
- **Heap Size**: Memória total usada

---

## ✅ Checklist de Análise

Para cada cenário problemático:

- [ ] Capturar baseline (estratégia boa)
- [ ] Capturar problema (estratégia ⚠)
- [ ] Comparar métricas principais
- [ ] Identificar hotspot/bottleneck
- [ ] Formular hipótese da causa
- [ ] Propor solução
- [ ] (Opcional) Implementar fix e validar

---

## 💡 Dicas

1. **Build Release**: Sempre profile código otimizado
2. **Múltiplas Runs**: Alguns problemas são probabilísticos (deadlock)
3. **Filter System Code**: Foque no código do usuário
4. **Compare Side-by-Side**: Use VTune's comparison feature
5. **Document Findings**: Screenshot de flamegraphs e métricas chave

---

