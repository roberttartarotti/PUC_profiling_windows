# Conceitos Fundamentais de Profiling Gráfico

## Índice
1. [O que é Profiling Gráfico?](#o-que-é-profiling-gráfico)
2. [CPU vs GPU Profiling](#cpu-vs-gpu-profiling)
3. [O Pipeline Gráfico](#o-pipeline-gráfico)
4. [Como Primitivas São Desenhadas](#como-primitivas-são-desenhadas)
5. [DirectX vs OpenGL](#directx-vs-opengl)
6. [Métricas Essenciais](#métricas-essenciais)
7. [Gargalos Comuns](#gargalos-comuns)

---

## O que é Profiling Gráfico?

### Definição

**Profiling gráfico** é o processo de **medir e analisar** o desempenho de uma aplicação que renderiza gráficos, identificando:
- Onde o tempo está sendo gasto (CPU ou GPU)
- Quais operações são mais custosas
- Oportunidades de otimização

### Analogia: A Cozinha de um Restaurante

Imagine um restaurante movimentado:

| Elemento | Analogia de Profiling |
|----------|----------------------|
| **Chef (CPU)** | Organiza pedidos, prepara ingredientes, coordena equipe |
| **Cozinha (GPU)** | Executa muitas tarefas simultaneamente (frituras, assados, etc) |
| **Profiling** | Filmar a cozinha durante o serviço para ver onde há gargalos |
| **Gargalo** | Se o chef está sobrecarregado, a cozinha fica parada (CPU-bound) |
| | Se a cozinha está cheia, o chef não consegue adicionar mais pedidos (GPU-bound) |

### Por que Profiling Gráfico é Importante?

**Para Jogos:**
- 60 FPS (16.7ms por frame) é o padrão mínimo
- Quedas de FPS arruínam a experiência
- Jogos AAA têm budgets de performance extremamente apertados

**Para Aplicações Corporativas:**
- Dashboards com muitos gráficos podem travar
- UIs ricas (WPF, WinUI) podem ter animações travadas
- Visualização de dados 3D (CAD, simulações) precisa ser fluida

**Para Interfaces Ricas:**
- UIs modernas usam aceleração de GPU (mesmo em 2D)
- Animações complexas exigem 60 FPS
- Overdraw em elementos transparentes é um problema comum

---

## CPU vs GPU Profiling

### Diferenças Fundamentais

| Aspecto | CPU Profiling | GPU Profiling |
|---------|---------------|---------------|
| **O que mede** | Lógica, preparação de comandos, gerenciamento | Execução de shaders, rasterização, processamento de pixels |
| **Paralelismo** | Threads (4-16 cores típicos) | Milhares de threads simultâneos |
| **Gargalo típico** | Muitos draw calls, lógica de jogo complexa | Shaders pesados, muita geometria, overdraw |
| **Sintoma** | GPU ociosa esperando comandos | CPU ociosa esperando GPU terminar |
| **Ferramenta** | CPU Profiler (Visual Studio, VTune) | GPU Profiler (PIX, Nsight, RenderDoc) |

### Como Identificar o Gargalo?

**No PIX, observe a Timeline:**

#### Gargalo de CPU (CPU-Bound):
```
Timeline:
|======CPU (100% busy)======|
|--GPU (idle)--GPU--idle----|
```
- **CPU sempre ocupada**
- **GPU com gaps (ociosa)**
- **Sintomas**: Muitos draw calls pequenos, lógica complexa entre frames

#### Gargalo de GPU (GPU-Bound):
```
Timeline:
|--CPU--idle--CPU--idle-----|
|========GPU (100% busy)====|
```
- **GPU sempre ocupada**
- **CPU com gaps (ociosa)**
- **Sintomas**: Shaders complexos, muita geometria, alta resolução

#### Balanceado (Ideal):
```
Timeline:
|====CPU====|====CPU====|
|====GPU====|====GPU====|
```
- **Ambos trabalhando continuamente**
- **Poucos gaps**
- **Melhor utilização de recursos**

### Exemplo Prático

**Cenário: UI com 1000 botões transparentes sobrepostos**

**Análise:**
```
Draw Calls: 1000 (um por botão)
Tempo de CPU: 5ms (preparando comandos)
Tempo de GPU: 15ms (desenhando pixels + transparência)

Diagnóstico: GPU-Bound (Pixel Shader trabalhando muito)
Solução: Reduzir overdraw, combinar geometrias, usar atlases
```

---

## O Pipeline Gráfico

### O que é um Pipeline?

Um **pipeline gráfico** é uma sequência **fixa de etapas** que transformam:
- **Entrada**: Dados brutos (vértices, texturas, parâmetros)
- **Saída**: Pixels na tela

### Analogia: Linha de Montagem de Carros

```
[Chassis] -> [Motor] -> [Pintura] -> [Rodas] -> [Inspeção] -> [Carro Pronto]
```

Cada estação faz uma tarefa específica. Se uma estação fica lenta, a linha inteira desacelera.

### Estágios do Pipeline DirectX 12

```
+---------------------------------------------------------------+
| 1. Input Assembler (IA)                                       |
|    - Lê dados de vértices (posição, cor, texcoord)           |
|    - Agrupa em primitivas (triângulos, linhas)               |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 2. Vertex Shader (VS) - PROGRAMÁVEL                           |
|    - Transforma posição de vértices (projeção, rotação)      |
|    - Calcula iluminação por vértice                          |
|    - Passa dados para próximo estágio                        |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 3. Hull Shader (HS) - OPCIONAL/PROGRAMÁVEL                    |
|    - Tesselação (divide geometria em mais detalhes)          |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 4. Tessellator - FIXO                                         |
|    - Gera novos vértices baseado no Hull Shader              |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 5. Domain Shader (DS) - OPCIONAL/PROGRAMÁVEL                  |
|    - Transforma vértices tesselados                          |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 6. Geometry Shader (GS) - OPCIONAL/PROGRAMÁVEL                |
|    - Pode criar/destruir primitivas                          |
|    - Ex: criar linhas a partir de pontos                     |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 7. Rasterizer (Rasterization) - FIXO                          |
|    - Converte triângulos em pixels (fragments)               |
|    - Determina quais pixels cobrir                           |
|    - Interpola atributos (cor, texcoord) entre vértices      |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 8. Pixel Shader (PS) - PROGRAMÁVEL                            |
|    - Calcula cor final de cada pixel                         |
|    - Aplica texturas, iluminação, efeitos                    |
|    - Pode descartar pixels (alpha test, discard)             |
+---------------------------------------------------------------+
                                 |
                                 v
+---------------------------------------------------------------+
| 9. Output Merger (OM) - FIXO                                  |
|    - Depth test (Z-buffer)                                   |
|    - Blending (transparência)                                |
|    - Escreve no render target (frame buffer)                 |
+---------------------------------------------------------------+
```

### Onde Ocorrem os Gargalos?

| Estágio | Gargalo Típico | Sintoma | Solução |
|---------|---------------|---------|---------|
| **Input Assembler** | Muitos draw calls | CPU alta, GPU ociosa | Batch/instancing |
| **Vertex Shader** | Transformações complexas por vértice | Tempo no VS alto | Simplificar VS, reduzir geometria |
| **Rasterizer** | Muitos triângulos pequenos | Tempo no Rasterizer alto | LOD, frustum culling |
| **Pixel Shader** | Cálculos complexos, muitas texturas | Tempo no PS alto | Simplificar shader, reduzir resolução |
| **Output Merger** | Overdraw, blending complexo | Tempo no OM alto | Z-sorting, reduzir transparência |

---

## Como Primitivas São Desenhadas

### Primitivas Básicas

```
1. PONTO (Point)
   *

2. LINHA (Line)
   *----*

3. TRIÂNGULO (Triangle) - A MAIS IMPORTANTE!
    *
   / \
  *---*
```

### Por que Triângulos?

**Triângulos são o "tijolo básico" da computação gráfica 3D porque:**

1. **Sempre planares**: 3 pontos sempre definem um único plano
2. **Convexos**: Facilita cálculos de rasterização
3. **Simples**: Fácil interpolar atributos entre 3 vértices
4. **Eficiente**: Hardware de GPU é otimizado para triângulos

### Fluxo Completo: Do Vértice ao Pixel

#### Exemplo: Desenhar um Triângulo Vermelho

**1. Dados Iniciais (CPU envia para GPU):**
```csharp
Vertex[] vertices = {
    new Vertex { Position = (-0.5f, -0.5f, 0), Color = (1, 0, 0, 1) },  // Vermelho
    new Vertex { Position = ( 0.5f, -0.5f, 0), Color = (1, 0, 0, 1) },
    new Vertex { Position = ( 0.0f,  0.5f, 0), Color = (1, 0, 0, 1) }
};
```

**2. Input Assembler:**
```
Agrupa os 3 vértices em 1 triângulo
```

**3. Vertex Shader (Transformação):**
```hlsl
VSOutput main(VSInput input) {
    VSOutput output;
    // Multiplicar posição pelas matrizes de transformação
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    return output;
}
```

**Resultado:**
```
Vértice A: Tela (100, 400), Cor (1,0,0,1)
Vértice B: Tela (700, 400), Cor (1,0,0,1)
Vértice C: Tela (400, 100), Cor (1,0,0,1)
```

**4. Rasterizer:**
```
Determina quais pixels estão dentro do triângulo:

      C (400, 100)
       *
      /|\
     //|\\
    ///|\\\
   ////|\\\\
  /////|\\\\\
  *---------*
A (100,400) B (700,400)

Pixels cobertos: ~50,000 pixels (exemplo)
```

**5. Pixel Shader (para CADA pixel):**
```hlsl
float4 main(PSInput input) : SV_Target {
    // Simplesmente retornar a cor interpolada
    return input.Color; // (1, 0, 0, 1) = Vermelho
}
```

**6. Output Merger:**
```
Escrever cor vermelha no frame buffer para cada pixel
Resultado: Triângulo vermelho na tela!
```

---

## DirectX vs OpenGL

### Comparação Rápida

| Aspecto | DirectX | OpenGL |
|---------|---------|--------|
| **Plataforma** | Windows, Xbox | Windows, Linux, macOS, Android, iOS |
| **Desenvolvedor** | Microsoft | Khronos Group (consórcio aberto) |
| **Versão atual** | DirectX 12 (2015) | OpenGL 4.6 (2017), Vulkan (sucessor) |
| **Integração Windows** | Nativa, profunda | Via drivers |
| **Ferramentas** | PIX, Visual Studio | RenderDoc, Nsight (NVIDIA) |
| **Ecossistema** | Windows, XNA, Unity, Unreal | Multiplataforma, LWJGL, SDL |

### Qual Escolher?

**Use DirectX se:**
- Desenvolve exclusivamente para Windows
- Precisa das melhores ferramentas de profiling (PIX)
- Trabalha com tecnologias Microsoft (WPF, UWP, WinUI)
- Quer acesso a recursos mais recentes no Windows

**Use OpenGL/Vulkan se:**
- Precisa de multiplataforma
- Desenvolve para Android/iOS
- Trabalha em simulações científicas/CAD
- Quer controle de baixo nível (Vulkan)

### Contexto do Curso

**Focamos em DirectX porque:**
1. É o padrão no ecossistema Windows
2. PIX é a ferramenta de profiling mais avançada para Windows
3. Aplicações corporativas da Samsung no Windows usam DirectX por baixo dos panos
4. WPF, WinUI e UWP usam DirectX nativamente

---

## Métricas Essenciais

### 1. FPS (Frames Per Second)

**O que é:** Quantos quadros completos são renderizados por segundo.

**Alvos comuns:**
- **60 FPS** (16.7ms por frame) - Padrão para jogos e UIs fluidas
- **30 FPS** (33.3ms por frame) - Aceitável para aplicações não-interativas
- **144+ FPS** (6.9ms ou menos) - Para jogos competitivos

**Como calcular:**
```
FPS = 1000 / Tempo_por_Frame_ms

Exemplo:
Tempo por frame = 16.7ms
FPS = 1000 / 16.7 = 60 FPS
```

**No código:**
```csharp
private Stopwatch frameTimer = Stopwatch.StartNew();
private int frameCount = 0;
private double fps = 0;

void OnRenderFrame() {
    // Renderizar...
    
    frameCount++;
    if (frameTimer.ElapsedMilliseconds >= 1000) {
        fps = frameCount * 1000.0 / frameTimer.ElapsedMilliseconds;
        Console.WriteLine($"FPS: {fps:F1}");
        frameCount = 0;
        frameTimer.Restart();
    }
}
```

### 2. Tempo por Frame (ms)

**O que é:** Quanto tempo a GPU levou para renderizar UM frame completo.

**Por que é melhor que FPS:**
- Mais preciso para diagnóstico
- Mostra variações (stuttering)
- Linear (FPS não é)

**Exemplo:**
```
Frame 1: 16.2ms -> 61.7 FPS
Frame 2: 16.8ms -> 59.5 FPS (pequena variação)
Frame 3: 33.4ms -> 29.9 FPS (spike! Investigar!)
```

### 3. Draw Calls

**O que é:** Comandos enviados pela CPU para a GPU dizendo "desenhe isto".

**Por que importam:**
- Cada draw call tem overhead na CPU
- Milhares de draw calls = CPU sobrecarregada
- GPU pode ficar ociosa esperando comandos

**Exemplo no código:**
```csharp
// RUIM: 10,000 draw calls
for (int i = 0; i < 10000; i++) {
    commandList.DrawIndexedInstanced(3, 1, 0, 0, 0); // 1 triângulo
}

// BOM: 1 draw call
commandList.DrawIndexedInstanced(30000, 1, 0, 0, 0); // 10,000 triângulos de uma vez
```

**Alvos:**
- Menos de 2000 draw calls por frame = bom
- 2000-5000 = aceitável
- Mais de 5000 = provável gargalo de CPU

### 4. Uso de GPU (%)

**O que é:** Percentual de tempo que a GPU está ocupada executando trabalho.

**Interpretação:**
```
GPU Usage: 99-100% -> GPU é o gargalo (GPU-bound)
GPU Usage: 50-70%  -> Balanceado ou CPU-bound
GPU Usage: <30%    -> CPU é definitivamente o gargalo
```

**Como medir:**
- Gerenciador de Tarefas do Windows (aba Performance -> GPU)
- PIX (na timeline)
- GPU-Z (ferramenta dedicada)

### 5. Overdraw

**O que é:** Número médio de vezes que cada pixel é desenhado no mesmo frame.

**Por que é ruim:**
- Cada desenho consome Pixel Shader
- Elementos transparentes sobrepondo = overdraw alto
- Pode causar gargalo de GPU sem sinais óbvios

**Exemplo:**
```
Cena: 10 janelas transparentes empilhadas

Pixel no centro da tela:
- Janela 10 (fundo): desenhada
- Janela 9: desenhada (com blending)
- Janela 8: desenhada (com blending)
- ...
- Janela 1 (frente): desenhada

Overdraw neste pixel: 10x!
```

**Como medir:**
- PIX: Visualização de overdraw (heat map)
- RenderDoc: Overdraw visualization
- Manual: Contar camadas transparentes

**Alvos:**
- Overdraw médio < 2x = ótimo
- 2x-4x = aceitável
- > 4x = problema (otimizar!)

---

## Gargalos Comuns

### 1. Muitos Draw Calls (CPU-Bound)

**Sintomas:**
- FPS baixo mesmo com GPU ociosa
- CPU sempre 100%
- Muitos objetos pequenos na cena

**Solução:**
- **Batching**: Combinar múltiplos objetos em um único draw call
- **Instancing**: Desenhar muitas cópias do mesmo objeto de uma vez
- **Culling**: Não enviar para GPU o que não está visível

**Exemplo:**
```csharp
// ANTES: 1000 draw calls
foreach (var tree in trees) {
    DrawTree(tree); // 1 draw call por árvore
}

// DEPOIS: 1 draw call
DrawTreesInstanced(trees, count: 1000);
```

### 2. Pixel Shader Complexo (GPU-Bound)

**Sintomas:**
- GPU sempre 100%
- CPU ociosa
- Tempo alto no estágio Pixel Shader (PIX)

**Causas comuns:**
- Cálculos complexos por pixel
- Muitas amostras de textura
- Loops no pixel shader

**Solução:**
- Simplificar cálculos
- Pré-calcular o que for possível
- Usar LOD (Level of Detail)
- Reduzir resolução de render

**Exemplo:**
```hlsl
// LENTO: Pixel Shader complexo
float4 PSMain(PSInput input) : SV_Target {
    float3 color = 0;
    for (int i = 0; i < 100; i++) { // Loop caro!
        color += ComputeLighting(input, lights[i]);
    }
    return float4(color, 1);
}

// RÁPIDO: Pré-calcular iluminação
float4 PSMain(PSInput input) : SV_Target {
    // Usar lightmap pré-calculado
    float3 color = LightMapTexture.Sample(sampler, input.TexCoord);
    return float4(color, 1);
}
```

### 3. Overdraw Alto (GPU-Bound)

**Sintomas:**
- GPU sempre 100%
- Muitos elementos transparentes
- Tempo alto no Pixel Shader mesmo com shader simples

**Solução:**
- Z-sorting (desenhar objetos opacos primeiro)
- Reduzir número de camadas transparentes
- Usar alpha test ao invés de blending quando possível

### 4. Geometria Excessiva (GPU-Bound)

**Sintomas:**
- Tempo alto no Vertex Shader
- Muitos triângulos na cena (>1M visíveis)

**Solução:**
- LOD (usar modelos menos detalhados à distância)
- Frustum culling (não enviar o que está fora da câmera)
- Occlusion culling (não desenhar o que está escondido)

---

## Resumo: Fluxo de Diagnóstico

```
1. Capture um frame no PIX
   |
   v
2. Observe o tempo total do frame
   |
   v
3. É maior que 16.7ms (60 FPS)?
   | SIM
   v
4. Verifique na timeline: CPU ou GPU está 100%?
   |
   +-> CPU 100%, GPU ociosa -> CPU-BOUND
   |  +-> Verifique draw calls (muitos?) -> Batch/Instancing
   |
   +-> GPU 100%, CPU ociosa -> GPU-BOUND
      |
      v
      5. Qual estágio está lento?
         +-> Vertex Shader -> Muita geometria ou VS complexo
         +-> Pixel Shader -> Shader complexo ou overdraw
         +-> Rasterizer -> Muitos triângulos pequenos
```

---

**Próximos Passos:**
- Leia: `PIX_FIRST_CAPTURE_GUIDE.md` - Passo a passo prático
- Pratique: Capture um frame e identifique onde está o tempo

---

**Autor**: Material didático PUC - Profiling no Windows  
**Atualizado**: 2025  
**Versão**: 1.0
