# Questionário de Revisão - Aula 1: Fundamentos do Profiling Gráfico

## Instruções
- Este questionário cobre os conceitos fundamentais da Aula 1
- Responda todas as 15 questões
- Gabarito e explicações estão no final
- Tempo sugerido: 20 minutos

---

## Seção 1: Conceitos Básicos (5 questões)

### Questão 1
**O que é profiling gráfico?**

A) Um processo de criar gráficos bonitos  
B) Medir e analisar o desempenho de renderização para identificar gargalos  
C) Desenhar diagramas de fluxo de dados  
D) Otimizar apenas o código da CPU  

---

### Questão 2
**Qual a principal ferramenta da Microsoft para profiling de DirectX 12?**

A) Visual Studio Performance Profiler  
B) RenderDoc  
C) PIX para Windows  
D) NVIDIA Nsight  

---

### Questão 3
**Qual a diferença fundamental entre CPU profiling e GPU profiling?**

A) CPU profiling mede lógica de jogo; GPU profiling mede renderização  
B) São a mesma coisa  
C) CPU profiling é mais importante  
D) GPU profiling só funciona em placas NVIDIA  

---

### Questão 4
**Qual é o tempo alvo por frame para atingir 60 FPS?**

A) 33.3 ms  
B) 16.7 ms  
C) 8.3 ms  
D) 100 ms  

---

### Questão 5
**O que significa quando a GPU está com uso de 99% e a CPU com 40%?**

A) CPU é o gargalo  
B) GPU é o gargalo  
C) Sistema está balanceado  
D) Há um erro de medição  

---

## Seção 2: Pipeline Gráfico (5 questões)

### Questão 6
**Cite TRÊS etapas do pipeline gráfico do DirectX em ordem:**

Sua resposta:
1. _______________________
2. _______________________
3. _______________________

---

### Questão 7
**Qual estágio do pipeline é responsável por converter triângulos em pixels?**

A) Vertex Shader  
B) Rasterizer  
C) Pixel Shader  
D) Output Merger  

---

### Questão 8
**Por que triângulos são a primitiva mais usada em 3D?**

A) São bonitos  
B) São sempre planares e fáceis de rasterizar  
C) São os únicos que a GPU suporta  
D) Não há motivo especial  

---

### Questão 9
**Qual estágio do pipeline é PROGRAMÁVEL (você escreve código HLSL)?**

A) Rasterizer  
B) Input Assembler  
C) Pixel Shader  
D) Output Merger  

---

### Questão 10
**O que o Vertex Shader faz?**

A) Desenha pixels na tela  
B) Transforma posições de vértices (projeção, rotação, etc)  
C) Mescla cores de múltiplos render targets  
D) Detecta colisões entre objetos  

---

## Seção 3: Métricas e Diagnóstico (5 questões)

### Questão 11
**O que são draw calls?**

A) Funções que desenham texto na tela  
B) Comandos da CPU instruindo a GPU a renderizar geometria  
C) Ligações telefônicas durante o desenvolvimento  
D) Chamadas de API do sistema operacional  

---

### Questão 12
**Quantos draw calls por frame são considerados aceitáveis?**

A) Menos de 100  
B) 100-500  
C) 2000-5000  
D) Não há limite  

---

### Questão 13
**O que é overdraw?**

A) Desenhar fora da tela  
B) Desenhar o mesmo pixel múltiplas vezes no mesmo frame  
C) Desenhar com cores muito saturadas  
D) Usar mais draw calls que o necessário  

---

### Questão 14
**Se a timeline do PIX mostra CPU sempre ocupada e GPU com gaps (ociosa), qual é o diagnóstico?**

A) GPU-bound (GPU é o gargalo)  
B) CPU-bound (CPU é o gargalo)  
C) Sistema está perfeito  
D) Memória insuficiente  

---

### Questão 15
**Qual arquivo deve estar presente junto com o .exe para o PIX mostrar nomes de funções?**

A) .dll  
B) .lib  
C) .pdb (símbolos de debug)  
D) .xml  

---

## Seção 4: Caso Prático

### Questão 16 (Dissertativa)
**Cenário:** Você capturou um frame no PIX e observou:
- FPS: 35
- Tempo por frame: 28.5 ms
- Draw calls: 8,500
- GPU Utilization: 45%
- CPU sempre 100% ocupada
- Pixel Shader rápido: 2ms total

**Responda:**

A) Qual é o gargalo principal? (CPU ou GPU?)  

_______________________

B) Cite DUAS técnicas para resolver este problema:  

1. _______________________
2. _______________________

---

## Gabarito e Explicações

### Seção 1: Conceitos Básicos

**Q1: B**  
Profiling gráfico é o processo de medir e analisar o desempenho para identificar onde estão os gargalos (CPU ou GPU).

**Q2: C**  
PIX para Windows é a ferramenta oficial da Microsoft para DirectX 12 profiling.

**Q3: A**  
CPU profiling foca na lógica, preparação de comandos, gerenciamento. GPU profiling foca em shaders, rasterização, renderização.

**Q4: B**  
60 FPS = 1000ms / 60 = 16.7ms por frame. Se o frame levar mais que isso, o FPS cai.

**Q5: B**  
GPU sempre ocupada (99%) e CPU ociosa (40%) = GPU é o gargalo (GPU-bound). A GPU não consegue processar rápido o suficiente.

---

### Seção 2: Pipeline Gráfico

**Q6: Possíveis respostas corretas (qualquer sequência de 3):**
- Input Assembler -> Vertex Shader -> Rasterizer
- Vertex Shader -> Rasterizer -> Pixel Shader
- Rasterizer -> Pixel Shader -> Output Merger

**Q7: B**  
O Rasterizer converte primitivas (triângulos) em pixels (fragments).

**Q8: B**  
Triângulos são sempre planares (3 pontos definem um plano), convexos, e fáceis de rasterizar. Hardware é otimizado para eles.

**Q9: C**  
Pixel Shader é programável (você escreve HLSL). Também são programáveis: Vertex Shader, Geometry Shader, Hull/Domain Shaders.

**Q10: B**  
Vertex Shader transforma posições de vértices (aplica matrizes de mundo, view, projeção). Também pode calcular iluminação por vértice.

---

### Seção 3: Métricas e Diagnóstico

**Q11: B**  
Draw calls são comandos da CPU para a GPU dizendo "desenhe esta geometria com estes shaders/texturas".

**Q12: C**  
2000-5000 draw calls é aceitável. Menos de 2000 é ótimo. Mais de 5000 pode causar gargalo de CPU.

**Q13: B**  
Overdraw é desenhar o mesmo pixel múltiplas vezes (ex: camadas transparentes sobrepostas). Consome Pixel Shader sem benefício visual.

**Q14: B**  
CPU sempre ocupada e GPU ociosa = CPU-bound. A CPU não consegue enviar comandos rápido o suficiente (geralmente muitos draw calls).

**Q15: C**  
O arquivo .pdb (Program Database) contém símbolos de debug, permitindo que o PIX mostre nomes de funções ao invés de endereços.

---

### Seção 4: Caso Prático

**Q16A: CPU é o gargalo (CPU-bound)**

**Justificativa:**
- CPU sempre 100% ocupada
- GPU apenas 45% (ociosa esperando comandos)
- 8,500 draw calls (muito alto!)
- Pixel Shader rápido (não é problema de GPU)

**Q16B: Técnicas de solução:**

1. **Batching** - Combinar múltiplos objetos em menos draw calls
2. **Instancing** - Desenhar muitas cópias do mesmo objeto de uma vez
3. **Culling** - Não enviar para GPU o que não está visível (frustum culling)
4. **Reduzir draw calls** - Simplificar lógica de renderização
5. **Usar indirect drawing** - Deixar a GPU decidir o que desenhar


**Autor**: Material didático PUC - Profiling no Windows  
**Atualizado**: 2025  
**Versão**: 1.0
