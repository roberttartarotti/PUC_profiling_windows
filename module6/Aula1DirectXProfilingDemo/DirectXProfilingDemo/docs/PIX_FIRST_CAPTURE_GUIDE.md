# Guia: Sua Primeira Captura com PIX

## Objetivo
Este guia vai levá-lo passo a passo pela sua primeira captura de frame usando PIX para Windows.

---

## Pré-requisitos

Antes de começar, certifique-se de que:

- [ ] Windows 10 (versão 1809+) ou Windows 11
- [ ] PIX para Windows instalado (verificar com `setup_pix.bat`)
- [ ] Drivers de vídeo atualizados
- [ ] Aplicação compilada em modo **Debug** com símbolos PDB

---

## Passo a Passo: Primeira Captura

### 1. Preparar a Aplicação

**A. Compilar em Debug com PDB:**
```cmd
cd DirectXProfilingDemo
dotnet build -c Debug /p:DebugType=portable /p:DebugSymbols=true
```

**B. Verificar que o PDB foi gerado:**
```cmd
dir bin\Debug\net10.0-windows\*.pdb
```

Você deve ver:
```
DirectXProfilingDemo.pdb
```

**C. Executar a aplicação:**
```cmd
dotnet run
```

A aplicação deve abrir e exibir a cena 3D.

---

### 2. Abrir PIX para Windows

**A. Executar PIX como Administrador:**
- Localização: `C:\Program Files\Microsoft PIX\[versão]\WinPix.exe`
- **Clique com botão direito** -> "Executar como administrador"

**B. Verificar janela inicial:**

Você verá 3 opções principais:
- **GPU Capture** - Captura detalhada de um frame
- **Timing Capture** - Análise de performance ao longo do tempo
- **Computer** - Análise de sistema (CPU + GPU)

---

### 3. Configurar Captura de Timing

**A. Selecionar "Timing Capture":**

1. Clique em **"Timing Capture"** na tela inicial
2. Na janela que abrir, você verá campos para configuração

**B. Selecionar a Aplicação:**

**Método 1 - Attach to Process (Recomendado para primeira vez):**
1. Com `DirectXProfilingDemo.exe` rodando
2. No PIX, clique em **"Select Target Process"**
3. Procure por `DirectXProfilingDemo.exe` na lista
4. Selecione e clique **"Attach"**

**Método 2 - Launch Executable:**
1. Clique em **"Select Target Application"**
2. Navegue até: `bin\Debug\net10.0-windows\DirectXProfilingDemo.exe`
3. Clique **"Launch"**

**C. Configurar Opções de Captura:**

Configurações recomendadas para primeira captura:
- **Capture Duration**: 10 segundos
- **Capture Mode**: "Timing and Call Information"
- **PIX Markers**: [X] Enabled (para ver nossos marcadores coloridos)
- **GPU Events**: [X] Enabled

---

### 4. Realizar a Captura

**A. Iniciar Captura:**

1. No PIX, clique no botão grande **"Start"** (ícone de play)
2. Você verá uma contagem regressiva: "Capturing... 10, 9, 8..."

**B. Interagir com a Aplicação:**

Durante os 10 segundos de captura, na aplicação:
1. Movimente o mouse (rotaciona a câmera)
2. Clique nos botões de cenário:
   - "GPU Bottleneck" (gera shader complexo)
   - "CPU Bottleneck" (muitos draw calls)
3. Ajuste o slider de triângulos

**C. Parar Captura:**

- A captura para automaticamente após 10 segundos
- OU clique no botão **"Stop"** (ícone de stop) para parar antes

---

### 5. Salvar o Arquivo de Captura

**A. Janela de Salvamento:**

Após parar, o PIX pergunta onde salvar:
1. Escolha um local (ex: `Desktop` ou `Documents\PIX_Captures`)
2. Nome sugerido: `DirectXDemo_FirstCapture_[data].wpix`
3. Clique **"Save"**

**B. Aguardar Processamento:**

O PIX vai processar os dados (pode levar 10-30 segundos):
```
Processing capture data...
Analyzing GPU events...
Building timeline...
```

---

### 6. Explorar os Resultados

Quando o processamento terminar, você verá 4 painéis principais:

#### **A. Timeline (Linha do Tempo) - Topo**

```
|----CPU Thread 1-----|
|========GPU Queue=========|
```

**O que observar:**
- Barras coloridas representam atividade
- Verde = CPU enviando comandos
- Azul = GPU executando
- Vermelho = Espera/sincronização

**Controles:**
- **Scroll horizontal**: arrastar ou usar a roda do mouse
- **Zoom**: Ctrl + scroll ou botões +/-
- **Selecionar região**: clicar e arrastar

#### **B. Events List (Lista de Eventos) - Centro/Esquerda**

```
Event Name                    | Duration (ms) | Type
------------------------------|---------------|----------
BeginFrame                    | 0.001         | Marker
DrawTriangles (Baseline)      | 2.345         | Draw
  VertexShader                | 0.234         | Shader
  PixelShader                 | 2.000         | Shader
DrawTriangles (Complex)       | 8.567         | Draw
EndFrame                      | 0.002         | Marker
```

**O que observar:**
- Cada linha é um evento (draw call, marker, etc)
- Coluna "Duration" mostra tempo em milissegundos
- Ordene por Duration (clique no cabeçalho) para ver o mais lento

#### **C. Performance Metrics - Centro/Direita**

```
Frame Time: 16.7 ms (60 FPS)
GPU Utilization: 78%
CPU Utilization: 45%
Draw Calls: 156
Triangles Rendered: 45,000
```

**O que observar:**
- Frame Time acima de 16.7ms = abaixo de 60 FPS
- GPU Util > 90% = gargalo de GPU
- Muitos Draw Calls = possível gargalo de CPU

#### **D. Call Stack - Parte Inferior**

```
DirectXRenderer.RenderScene()
  DirectXRenderer.DrawBaseline()
    ID3D12GraphicsCommandList.DrawIndexedInstanced()
```

**O que observar:**
- Mostra quem chamou cada draw call
- Útil para rastrear origem de problemas
- Nomes aparecem se o PDB está presente

---

### 7. Primeira Análise: Identificar Gargalos

#### **Exercício Prático:**

**A. Encontrar o Draw Call Mais Lento:**

1. Na "Events List", clique no cabeçalho **"Duration"** para ordenar
2. O topo da lista mostra os eventos mais lentos
3. Procure por eventos do tipo "Draw" ou "DrawIndexed"

**Exemplo do que você pode ver:**
```
DrawTriangles (ComplexShader)  | 8.567 ms  | Draw
```

**B. Analisar no Pipeline:**

1. Clique com botão direito no evento lento
2. Selecione **"View in Pipeline"**
3. Você verá os estágios do pipeline:

```
Input Assembler -> Vertex Shader -> Rasterizer -> Pixel Shader -> Output Merger
     (fast)           (fast)        (fast)      (SLOW! 8ms)      (fast)
```

**Interpretação:**
- Se Pixel Shader está lento = muitos pixels ou shader complexo
- Se Vertex Shader está lento = muita geometria ou vertex shader complexo

**C. Verificar Marcadores Coloridos:**

Se implementamos corretamente, você verá nossos marcadores:
- "GPU Bottleneck Scenario" (vermelho)
- "CPU Bottleneck Scenario" (azul)
- "Baseline Scenario" (verde)

---

## Checklist

Após completar este guia, você deve ser capaz de:

- [ ] Abrir PIX e anexar a uma aplicação rodando
- [ ] Iniciar uma captura de timing
- [ ] Interagir com a aplicação durante a captura
- [ ] Salvar o arquivo .wpix
- [ ] Navegar pela timeline e eventos
- [ ] Identificar o draw call mais lento
- [ ] Interpretar métricas básicas (FPS, GPU%, Draw Calls)
- [ ] Ver em qual estágio do pipeline está o gargalo

---

## Problemas Comuns

### Problema: "PIX não encontra minha aplicação"

**Soluções:**
1. Certifique-se de que a aplicação está rodando
2. Execute PIX como administrador
3. Compile em Debug (não Release otimizado)
4. Verifique se não há antivírus bloqueando

### Problema: "Captura termina instantaneamente"

**Soluções:**
1. Aumente "Capture Duration" para 30 segundos
2. Verifique se DirectX está sendo usado (não software rendering)
3. Tente "Launch Executable" ao invés de "Attach to Process"

### Problema: "Não vejo nomes de funções, apenas endereços"

**Solução:**
- O PDB não está sendo carregado. Recompile:
  ```cmd
  dotnet build -c Debug /p:DebugType=portable /p:DebugSymbols=true
  ```

### Problema: "Marcadores coloridos não aparecem"

**Solução:**
- Certifique-se de que `PixHelper.cs` está sendo usado:
  ```csharp
  PixHelper.BeginEvent("Meu Evento", PixColors.Red);
  // ... código ...
  PixHelper.EndEvent();
  ```



**Autor**: Material didático PUC - Profiling no Windows  
**Atualizado**: 2025  
**Versão**: 1.0
