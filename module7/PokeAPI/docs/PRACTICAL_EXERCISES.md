# Exercícios Práticos de Escalabilidade em Multithreading

## 📊 Atividade 1: Análise de Gargalos de Escalabilidade

### Objetivo
Identificar e documentar gargalos de escalabilidade usando ferramentas de profiling.

### Instruções

1. **Setup Inicial**
   ```
   - Abra o projeto PokeProfiler.UI
   - Configure IdsBox: "1-50" (50 Pokémon)
   - Configure ArtificialDelayBox: "100" ms
   ```

2. **Execução Sequencial**
   - Selecione "Sequential" strategy
   - Abra Debug -> Performance Profiler -> .Net Counters
   - Marque "Show Threads"
   - Click "Start" no profiler
   - Click "Fetch" no aplicativo
   - Aguarde conclusão
   - Click "Stop Collection"

3. **Análise - Responda:**
   - Quantos núcleos foram utilizados? ___1-2________
   - Qual a % de utilização total da CPU? __10%_________
   - Tempo de execução: ____5842_______ ms
   - Speedup teórico possível (ProcessorCount ÷ tempo): ___________

4. **Execução Paralela - Load Balance**
   - Selecione "🔧 Load Balance Demo"
   - Repita processo de profiling
   - Observe distribuição entre threads

5. **Análise Comparativa:**
   ```
   | Métrica                    | Sequential | Load Balance Demo | Ganho |
   |----------------------------|-----------|---------------|-------|
   | Tempo de execução (ms)     |           |               |       |
   | Núcleos utilizados         |           |               |       |
   | CPU utilization (%)        |           |               |       |
   | Speedup real               |     1.0   |               |       |
   | Eficiência (speedup/cores) |           |               |       |
   ```

6. **Sobresubscrição - Problema**
   - Selecione "🔧 Oversubscription Demo"
   - Profiling com mesmo dataset
   
7. **Documentar:**
   - Quantas threads foram criadas? ___________
   - Compare com número de núcleos: ___________
   - Observe "Context Switches" no profiler
   - Tempo de execução piorou? Quanto? ___________

### Entregáveis
    - [ ] Tabela comparativa preenchida
    - [ ] Screenshots do profiler (CPU Usage)
    - [ ] Análise: Por que sobresubscrição piora desempenho?
    - [ ] Recomendação: Número ideal de threads para este caso


## 📚 Atividade 2: Análise de Contenção de Locks

### Objetivo
Medir impacto da contenção e comparar com estratégias lock-free.

### Instruções

1. **Baseline - Lock Contention**
   - Configure IdsBox: "1-100"
   - Selecione "Lock Contention" strategy
   - Abra Analyze -> Concurrency Visualizer (se disponível)
   - Ou use Debug -> Performance Profiler -> CPU Usage
   - Execute e observe métricas:
     - Tempo: ___________ ms
     - Contention Events: ___________
     - ThreadPool Used: ___________

2. **Comparação - Lock-Free**
   - Selecione "🔧 Lock-Free Demo"
   - Mesmo dataset (1-100)
   - Execute e observe:
     - Tempo: ___________ ms
     - Contention Events: ___________
     - ThreadPool Used: ___________

3. **Análise de Contenção**
   ```
   Calcule:
   - Redução de contenção: ((ContentionBefore - ContentionAfter) / ContentionBefore) × 100 = _____ %
   - Ganho de performance: ((TimeBefore - TimeAfter) / TimeBefore) × 100 = _____ %
   - Eficiência de threads: ThreadsUsed / ProcessorCount = _____ (ideal ≈ 2.0)
   ```

4. **SpinLock Analysis**
   - Configure IdsBox: "1-200"
   - Selecione "Lock Contention" (tradicional)
   - Registre tempo: ___________ ms
   - Troque para "🔧 SpinLock Contention"
   - Registre tempo: ___________ ms
   - Qual foi mais rápido? Por quê?

5. **Experimento - Variação de Carga**
   Preencha tabela testando diferentes tamanhos:
   
   ```
   | Dataset Size | Lock Contention (ms) | Lock-Free (ms) | Speedup |
   |--------------|---------------------|----------------|---------|
   | 10           |                     |                |         |
   | 50           |                     |                |         |
   | 100          |                     |                |         |
   | 200          |                     |                |         |
   | 500          |                     |                |         |
   ```

6. **Análise Gráfica**
   - Crie gráfico de linha: Dataset Size (x) vs Tempo (y)
   - Duas linhas: Lock Contention e Lock-Free
   - O que você observa conforme escala aumenta?

### Entregáveis
    - [ ] Tabelas preenchidas com medições
    - [ ] Gráfico comparativo
    - [ ] Análise: Em que cenário lock-free traz mais benefícios?
    - [ ] Proposta: Quando usar lock tradicional vs lock-free?


## 🎯 Atividade 3: Otimização de ThreadPool

### Objetivo
Experimentar configurações de ThreadPool e observar impacto na escalabilidade.

### Instruções

1. **Baseline - Default Settings**
   - Não altere MinThreads
   - Configure IdsBox: "1-100"
   - Selecione "Task.WhenAll"
   - Observe ThreadPoolStatus antes:
     - MinWorker: ___________
     - Available: ___________
   - Execute e registre:
     - Tempo: ___________ ms
     - ThreadPool Used: ___________
     - Available After: ___________

2. **Teste 1 - ThreadPool Starvation**
   - Configure MinThreads: 1
   - Click "Apply ThreadPool Config"
   - Observe status atualizado
   - Execute mesma strategy
   - Registre:
     - Tempo: ___________ ms (esperado: mais lento)
     - Observe "ramp-up time" inicial
     - ThreadPool Used: ___________

3. **Teste 2 - Otimizado**
   - Configure MinThreads: ProcessorCount (use `Environment.ProcessorCount`)
   - Click "Apply ThreadPool Config"
   - Execute novamente
   - Registre:
     - Tempo: ___________ ms
     - ThreadPool Used: ___________
   - Tempo de inicialização: mais rápido?

4. **Teste 3 - Over-provisioning**
    - Configure MinThreads: ProcessorCount × 4
   - Execute e observe
   - Registre:
     - Tempo: ___________ ms
       - Overhead de memória observado?

5. **Comparação Completa**
   ```
   | MinThreads Config | Tempo (ms) | Threads Used | Available After | Observações |
   |-------------------|-----------|--------------|-----------------|-------------|
   | Default           |           |              |                 |             |
   | 1                 |           |              |                 |             |
   | ProcessorCount    |           |              |                 |             |
   | ProcessorCount×4  |           |              |                 |             |
   ```

6. **Análise de Strategies Diferentes**
   Repita experimento com:
   - "ThreadPool Storm" (problema de oversubscription)
   - "🔧 Load Balance Demo" (otimizado)
   
   Compare comportamento com diferentes MinThreads.

### Entregáveis
    - [ ] Tabela comparativa preenchida
    - [ ] Gráfico: MinThreads (x) vs Tempo de Execução (y)
    - [ ] Análise: Qual configuração é ideal e por quê?
    - [ ] Recomendação: Diretrizes para configuração em produção


## 1️⃣ Atividade 4: Estruturas Lock-Free na Prática

### Objetivo
Implementar e comparar estruturas lock-free com equivalentes bloqueantes.

### Parte A - Análise de Código

1. **Revise LockFreeStrategy**
   ```csharp
   var lockFreeQueue = new ConcurrentQueue<Pokemon>();
   lockFreeQueue.Enqueue(pokemon); // Operação atômica
   ```
   
   Perguntas:
   - Por que `ConcurrentQueue.Enqueue` é lock-free? ___________
   - Que operações atômicas usa internamente? ___________
   - Quando lock-free é melhor que lock tradicional? ___________

2. **Revise ThreadLocalStrategy**
   ```csharp
   private readonly ThreadLocal<List<Pokemon>> _threadLocalResults;
   _threadLocalResults.Value!.Add(pokemon); // Sem contenção
   ```
   
   Perguntas:
   - Como ThreadLocal evita contenção? ___________
   - Qual overhead de memória? ___________
   - Desvantagens dessa abordagem? ___________

### Parte B - Revisão da Estratégia "🔧 Concurrent Bag"

Execute e avalie a estratégia existente `🔧 Concurrent Bag`:

- Estrutura: `ConcurrentBag<Pokemon>` para agregação thread-safe
- Paralelismo: `Parallel.ForEachAsync` com `MaxDegreeOfParallelism = ProcessorCount`
- I/O: chamadas a `_client.GetPokemonAsync`
- Cancelamento: respeita `CancellationToken`

Checklist de Avaliação:
   - [ ] Consegue processar `1-150` sem contenção perceptível
   - [ ] Tempo compatível com `🔧 Load Balance Demo`
   - [ ] Boa escalabilidade com aumento de IDs

### Parte C - Testes e Comparação

1. **Execute Sua Implementação**
    - Dataset: "1-150"
    - Registre tempo: ___________ ms

2. **Compare com Outras**
   ```
   | Strategy                      | Tempo (ms) | Tipo de Sincronização | Contenção? |
   |-------------------------------|-----------|------------------------|-----------|
   | Lock Contention               |           | lock { }               | Alta      |
   | 🔧 Lock-Free Demo             |           | ConcurrentQueue        | Baixa     |
   | 🔧 Concurrent Bag             |           | ConcurrentBag          | Baixa     |
   | 🔧 Thread Local Storage       |           | ThreadLocal            | Nenhuma   |
   ```

3. **Análise de Trade-offs**
   ```
   Para cada estrutura, documente:
   
   ConcurrentQueue:
   - Vantagens: ________________________________
   - Desvantagens: _____________________________
   - Melhor uso: _______________________________
   
   ConcurrentBag:
   - Vantagens: ________________________________
   - Desvantagens: _____________________________
   - Melhor uso: _______________________________
   
   ThreadLocal:
   - Vantagens: ________________________________
   - Desvantagens: _____________________________
   - Melhor uso: _______________________________
   ```

### Entregáveis
    - [ ] Código implementado e funcionando
    - [ ] Tabela de comparação preenchida
    - [ ] Análise detalhada de trade-offs
    - [ ] Recomendação: Quando usar cada estrutura


## 🔄 Atividade 5: Balanceamento de Carga

### Objetivo
Analisar e melhorar distribuição de trabalho entre threads.

### Instruções

1. **Problema - Carga Desbalanceada**
   
   Crie nova estratégia com carga artificial desbalanceada:
   
   ```csharp
   public class UnbalancedWorkStrategy : IPokemonFetchStrategy
   {
       private readonly PokeApiClient _client;
       
       public string Name => "Unbalanced Work";
       
       public UnbalancedWorkStrategy(PokeApiClient client)
       {
           _client = client;
       }
       
       public async Task<List<Pokemon>> FetchAsync(string[] ids, CancellationToken ct)
       {
           var results = new ConcurrentBag<Pokemon>();
           
           // Divide trabalho de forma desigual
           int chunkSize = ids.Length / Environment.ProcessorCount;
           var tasks = new List<Task>();
           
           for (int i = 0; i < Environment.ProcessorCount; i++)
           {
               int threadId = i;
               int start = i * chunkSize;
               int end = (i == Environment.ProcessorCount - 1) ? ids.Length : start + chunkSize;
               
               tasks.Add(Task.Run(async () =>
               {
                   // Thread 0 tem trabalho extra (desbalanceado)
                   if (threadId == 0)
                   {
                       Thread.SpinWait(10000000); // Simula trabalho extra
                   }
                   
                   for (int j = start; j < end; j++)
                   {
                       var pokemon = await _client.GetPokemonAsync(ids[j], ct);
                       if (pokemon != null) results.Add(pokemon);
                   }
               }, ct));
           }
           
           await Task.WhenAll(tasks);
           return results.ToList();
       }
   }
   ```

2. **Análise com Profiler**
   - Execute "Unbalanced Work" com CPU Usage Tool
   - Observe timeline de threads
   - Identifique:
     - Thread que termina primeiro: ___________
   - Thread que termina por último: ___________
     - Tempo ocioso das outras threads: ___________ ms
   - Eficiência geral: ___________ %

3. **Solução - Work Stealing**
   
   Use `Partitioner.Create` com load balancing:
   
   ```csharp
   var partitions = Partitioner.Create(ids, loadBalance: true);
   await Parallel.ForEachAsync(partitions, ...);
   ```

4. **Comparação**
   ```
   | Estratégia             | Tempo Total | Thread Mais Lenta | Ociosidade | Eficiência |
   |------------------------|------------|--------------------|-----------|-----------|
   | Unbalanced Work        |            |                    |           |           |
   | 🔧 Load Balance Demo   |            |                    |           |           |
   | Ganho                  |            |                    |           |           |
   ```

5. **Visualização**
   - No profiler, capture screenshot das timelines
   - Mostre diferença visual entre desbalanceado e balanceado
   - Anexe ao relatório

### Entregáveis
    - [ ] Código das duas estratégias (problem/solution)
    - [ ] Screenshots do profiler mostrando diferença
    - [ ] Tabela comparativa
    - [ ] Análise: Por que balanceamento é crítico?


## 📈 Atividade 6: Análise de Escalabilidade Completa

### Objetivo Final
Análise abrangente de escalabilidade de um cenário real.

### Cenário
Sua aplicação precisa processar 1000 Pokémon (IDs 1-1000) da forma mais eficiente possível.

### Parte 1 - Linha de Base

1. **Sequential Execution**
   - Configure: "1-1000" (AVISO: pode demorar!)
   - Ou use subset representativo: "1-100" e extrapole
   - Registre tempo: ___________ ms

2. **Calcular Speedup Ideal**
   ```
   Speedup Ideal = ProcessorCount = ___________
   Tempo Ideal = TempoSequencial / SpeedupIdeal = ___________ ms
   ```

### Parte 2 - Testes de Estratégias

Teste TODAS as estratégias disponíveis e preencha:

```
   | Strategy                    | Tempo (ms) | Speedup Real | Eficiência (%) | Contenção | ThreadPool Used |
   |-----------------------------|-----------|-------------|---------------|-----------|----------------|
   | Sequential                  |           | 1.0         | 100           | N/A       | 1              |
   | Task.WhenAll                |           |             |               |           |                |
   | ThreadPool Storm            |           |             |               |           |                |
   | Lock Contention             |           |             |               |           |                |
   | Semaphore Batch             |           |             |               |           |                |
   | 🔧 Oversubscription Demo    |           |             |               |           |                |
   | 🔧 Load Balance Demo        |           |             |               |           |                |
   | 🔧 Lock-Free Demo           |           |             |               |           |                |
   | 🔧 SpinLock Contention      |           |             |               |           |                |
   | 🔧 Task Parallelism         |           |             |               |           |                |
   | 🔧 Thread Local Storage     |           |             |               |           |                |
   | 🔧 Concurrent Bag           |           |             |               |           |                |
   | 🔧 Batch Processing         |           |             |               |           |                |
```

Cálculos:
```
   Speedup Real = TempoSequencial / TempoStrategy
   Eficiência = (Speedup Real / ProcessorCount) × 100%
```

### Parte 3 - Análise e Recomendações

1. **Identifique Padrões**
   - Melhor estratégia geral: ___________
   - Pior estratégia: ___________
   - Estratégias com >80% eficiência: ___________

2. **Análise de Contenção**
   - Estratégias com alta contenção: ___________
   - Estratégias com baixa contenção: ___________
   - Relação entre contenção e desempenho? ___________

3. **ThreadPool Analysis**
   - Estratégias que usaram ThreadPool eficientemente: ___________
   - Estratégias com oversubscription: ___________
   - Configuração ideal de MinThreads: ___________

4. **Gráficos**
   Crie três gráficos:
   - Bar chart: Estratégias (x) vs Tempo de Execução (y)
   - Line chart: Estratégias (x) vs Eficiência % (y)
   - Scatter plot: ThreadPool Used (x) vs Speedup (y)

### Parte 4 - Proposta de Otimização

**Sua Tarefa**: Crie uma estratégia otimizada que combine melhores práticas:

```csharp
public class OptimizedScalableStrategy : IPokemonFetchStrategy
{
    // TODO: Implemente combinando:
   // - Concorrência ideal (baseada em ProcessorCount)
    // - Estruturas lock-free (ConcurrentQueue/Bag)
    // - Balanceamento de carga (Partitioner)
    // - Throttling apropriado (SemaphoreSlim)
    // - Cancelamento (CancellationToken)
}
```

**Requisitos de Otimização:**
    - [ ] Usar `Environment.ProcessorCount` para concorrência
    - [ ] Evitar sobresubscrição
    - [ ] Usar estruturas lock-free
    - [ ] Implementar balanceamento de carga
    - [ ] Throttling para não sobrecarregar API
    - [ ] Tratamento correto de cancelamento

**Teste Sua Solução:**
    - Deve ter Speedup > 0.8 × ProcessorCount
    - Eficiência > 80%
    - Contenção < 10% das outras estratégias
    - ThreadPool Used ≈ 2 × ProcessorCount

### Entregáveis Finais
    - [ ] Tabela completa de todas estratégias
    - [ ] Três gráficos comparativos
    - [ ] Código da estratégia otimizada
    - [ ] Relatório (2-3 páginas) com:
      - Análise detalhada dos resultados
      - Identificação de gargalos comuns
      - Melhores práticas observadas
      - Recomendações para produção
      - Lições aprendidas


## 💡 Dicas para Sucesso

1. **Sempre use profiler** - Não adivinhe, meça!
2. **Compare com baseline** - Sequencial é sua referência
3. **Documente tudo** - Screenshots, números, observações
4. **Teste múltiplas vezes** - Tire médias, descarte outliers
5. **Entenda os conceitos** - Não apenas copie código
6. **Faça perguntas** - Discuta com colegas e professor

## 🗂️ Recursos de Apoio

    - [Documentação ThreadPool](https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool)
    - [Concurrent Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
    - [Visual Studio Profiler Guide](https://docs.microsoft.com/en-us/visualstudio/profiling/)
