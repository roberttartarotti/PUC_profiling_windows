# DirectX Profiling Demo - PIX Integration Guide

## Sobre
Esta aplicação demonstra técnicas de profiling DirectX com suporte completo ao **PIX for Windows**. O projeto resolve o problema comum de "WinPixGpuCapturer.dll not loaded" carregando as DLLs **DIRETAMENTE** do diretório de instalação do PIX, garantindo compatibilidade de versão.

## Recursos Implementados

### **DirectX Real**
- SharpDX com DirectX 11
- Vertex e Pixel Shaders HLSL
- Geometria dinâmica (triângulos organizados)
- Pipeline gráfico completo

### **PIX Integration**
- Carregamento automático de WinPixGpuCapturer.dll **do diretório PIX**
- Marcadores PIX coloridos por função
- Suporte a PIX Events para profiling detalhado
- Verificação automática de status do PIX
- **Compatibilidade de versão garantida**

### **Controles de Performance**
- **Triângulos**: 100-10,000 (workload de geometria)
- **Shader Complexo**: Pixel shader com loops pesados
- **Overdraw**: 5x renderização (fillrate bottleneck)
- **Draw Calls**: 1-10x multiplicador (CPU bottleneck)
- **Wireframe**: Visualização de geometria
- **Animação**: Alternância entre visualização estática 2D e animada 3D
- **Velocidade**: Controle de animação (quando habilitada)

## Setup PIX for Windows

### **1. Instalar PIX**
```
1. Abra a https://devblogs.microsoft.com/pix/download/
2. Procure "Latest main version"
3. Baixe e instale conforme a arquitetura de seu processador
```

### **2. Verificar Instalação**
```batch
# Execute o script de verificação
setup_pix.bat
```

### **3. NÃO Copie DLLs (IMPORTANTE!)**
```
NÃO copie WinPixGpuCapturer.dll para o diretório da aplicação
A aplicação carrega automaticamente do diretório PIX
Isso garante compatibilidade de versão
```

## Como Usar com PIX

### **Passo a Passo**
1. **Execute a aplicação** - PIX será detectado automaticamente
   ```bash
   dotnet run
   ```
2. **Verificar no console**: "PIX inicializado com sucesso"
3. **Abra PIX for Windows** como **Administrador**
4. **Clique em "Attach to Process"** no PIX
5. **Selecione "DirectXProfilingDemo.exe"** da lista
6. **Attach deve funcionar sem erros!**
7. **Configure cenários** usando os controles da aplicação
8. **Clique "Take GPU Capture"** no PIX
9. **Interaja com a aplicação** (mude configurações)
10. **Pare a captura** no PIX para analisar

### **Cenários de Teste Recomendados**

#### **Baseline (Performance Normal)**
- Triângulos: 1000
- Shader: Simples
- Overdraw: Desabilitado
- Draw Calls: 1x
- Animação: Desabilitada (visualização estática)

#### **GPU Bottleneck**
- Triângulos: 10000
- Shader: **Complexo**
- Overdraw: **Habilitado**
- Draw Calls: 1x
- Animação: Opcional

#### **CPU Bottleneck**
- Triângulos: 5000
- Shader: Simples
- Overdraw: Desabilitado
- Draw Calls: **10x**
- Animação: Habilitada (aumenta CPU load)

#### **Fillrate Issues**
- Triângulos: 5000
- Shader: Complexo
- Overdraw: **Habilitado**
- Draw Calls: 1x
- Animação: Desabilitada (foco no fillrate)

#### **Visualização Estática (Novo!)**
- Triângulos: Qualquer quantidade
- Shader: Qualquer
- Overdraw: Conforme necessário
- Draw Calls: Conforme necessário
- Animação: **Desabilitada** (triângulos estáticos para análise precisa)

## PIX Events Implementados

A aplicação inclui marcadores PIX coloridos para análise detalhada:

- **Frame Completo**: Verde (0xFF00FF00)
- **Clear**: Vermelho (0xFFFF0000)  
- **Setup Pipeline**: Azul (0xFF0000FF)
- **Update Buffers**: Amarelo (0xFFFFFF00)
- **Overdraw**: Magenta (0xFFFF00FF)
- **Draw Calls**: Laranja (0xFFFF8000)
- **Present**: Ciano (0xFF8000FF)

## Análise no PIX

### **GPU Timeline**
- Observe os **marcadores coloridos** por função
- Identifique **gargalos** nas diferentes fases
- Analise **utilização de GPU** por shader

### **Draw Calls**
- Contagem exata de **draw calls** por frame
- **Estado do pipeline** para cada draw call
- **Recursos utilizados** (buffers, texturas, shaders)

### **Shaders**
- **Compilação HLSL** com símbolos de debug
- **Análise de performance** por shader
- **Registers utilizados** e **instruções executadas**

### **Memory Usage**
- **Vertex Buffer** de 1MB dinâmico
- **Constant Buffer** para transformações
- **Render Targets** e **recursos DirectX**

## Troubleshooting

### **PIX não inicializa**
```
Problema: "PIX não está disponível"
Solução: 
   1. Reinstale PIX do site oficial
   2. Execute como Administrador
   3. Verifique se o caminho existe: C:\Program Files\Microsoft PIX
```

### **"Incompatible version" Error**
```
Problema: "incompatible version of WinPixGpuCapturer.dll"
Solução:
   1. NÃO copie DLLs do PIX
   2. Aplicação carrega do diretório original
   3. Execute cleanup_pix_dlls.bat se necessário
   4. Reinicie a aplicação
```

### **Attach falha**
```
Problema: "Failed to attach"
Solução:
   1. Verifique se PIX foi inicializado (console log)
   2. Execute PIX como Administrador
   3. Certifique-se que não há DLLs copiadas
   4. Use setup_pix.bat para verificar instalação
```

### **DirectX falha**
```
Problema: Erro na inicialização DirectX
Solução:
   1. Verifique drivers da placa de vídeo
   2. Use placa dedicada se disponível
   3. Execute como Administrador
```

## Métricas Disponíveis

### **Tempo Real**
- **FPS**: Frames por segundo
- **Frame Time**: Tempo em milissegundos
- **Draw Calls**: Contagem por frame
- **CPU Usage**: Via performance counters
- **GPU Usage**: Simulado baseado na carga

### **Configurações Dinâmicas**
- Todos os parâmetros ajustáveis em tempo real
- Efeito imediato nas métricas
- Perfect para demonstrações educacionais

## Scripts Auxiliares

### **setup_pix.bat**
- Verifica instalação do PIX
- Não copia DLLs (importante!)
- Mostra instruções de uso

### **cleanup_pix_dlls.bat**
- Remove DLLs PIX copiadas (se existirem)
- Resolve problemas de compatibilidade
- Execute se tiver problemas de versão

## Uso Educacional

Esta ferramenta é ideal para:
- **Aulas de profiling** DirectX/Graphics
- **Demonstrações** de bottlenecks
- **Treinamento** em PIX for Windows
- **Análise** de performance gráfica
- **Identificação** de problemas comuns

## Notas Técnicas

- **Target Framework**: .NET 10 Windows
- **DirectX**: 11 via SharpDX 4.2.0
- **Shaders**: HLSL com símbolos de debug
- **PIX**: WinPixEventRuntime 1.0.x
- **Architecture**: x64 (requerido pelo PIX)
- **PIX Compatibility**: Versão garantida

## Principais Correções

### **Problema Original**
```
"incompatible version of WinPixGpuCapturer.dll"
"loaded from an incompatible location"
```

### **Solução Implementada**
```
Carregamento direto do diretório PIX
Sem cópia de DLLs
Compatibilidade de versão garantida
Attach funcionando perfeitamente
```

---
**Desenvolvido para demonstrações educacionais de profiling DirectX com PIX compatibility**

# DirectX Profiling Demo - Módulo 6, Aula 1

## Visão Geral

Este projeto é uma **demonstração educacional completa** para a **Aula 1 - Fundamentos do Profiling Gráfico no Windows** do curso de Profiling de Performance.

> **Navegação Completa:** [**Acesse o ÍNDICE GERAL**](docs/INDEX.md) para encontrar rapidamente qualquer documento ou tópico.

### Objetivos da Aula
- Entender o que é profiling gráfico e sua importância
- Conhecer o pipeline gráfico DirectX 12
- Dominar o uso do PIX para Windows
- Identificar e diagnosticar gargalos de performance (CPU vs GPU)
- Realizar análises práticas com métricas reais

---

## Início Rápido (5 minutos)

### Para Alunos (Primeira Vez)
```bash
# 1. Verificar ambiente
setup_pix.bat

# 2. Compilar com símbolos de debug
dotnet build -c Debug /p:DebugType=portable /p:DebugSymbols=true

# 3. Executar aplicação
dotnet run

# 4. Seguir guia de primeira captura
# Abrir docs/PIX_FIRST_CAPTURE_GUIDE.md
```

### Para Professores (Preparação de Aula)
```bash
# 1. Ler roteiro cronometrado
# Abrir docs/PROFESSOR_QUICK_GUIDE.md

# 2. Testar ambiente
setup_pix.bat
dotnet build -c Debug /p:DebugType=portable

# 3. Gerar capturas de exemplo (3 cenários)
dotnet run
# Capturar: Baseline, GPU Bottleneck, CPU Bottleneck

# 4. Preparar material de aula
# Revisar slides mapeados em docs/INDEX.md
```

---

## Documentação Completa

> **Dica:** Todos os documentos estão indexados em [**docs/INDEX.md**](docs/INDEX.md) com navegação rápida por tópico.

### Para Alunos

| Ordem | Documento | Tempo | Descrição |
|-------|-----------|-------|-----------|
| 1 | **[PROFILING_CONCEPTS.md](docs/PROFILING_CONCEPTS.md)** | 30 min | Teoria: pipeline, métricas, diagnóstico |
| 2 | **[BUILD_AND_PDB_GUIDE.md](docs/BUILD_AND_PDB_GUIDE.md)** | 15 min | Como compilar com símbolos de debug |
| 3 | **[PIX_FIRST_CAPTURE_GUIDE.md](docs/PIX_FIRST_CAPTURE_GUIDE.md)** | 45 min | Passo a passo: primeira captura |
| 4 | **[PRACTICAL_CASES.md](docs/PRACTICAL_CASES.md)** | 1-2h | 4 casos práticos com diagnóstico |
| 5 | **[QUIZ_AULA1.md](docs/QUIZ_AULA1.md)** | 20 min | 15 questões + gabarito |
| 6 | **[HOMEWORK_AULA1.md](docs/HOMEWORK_AULA1.md)** | 1-2h | Atividade estruturada para entregar |

### Para Professores

| Documento | Uso | Descrição |
|-----------|-----|-----------|
| **[PROFESSOR_QUICK_GUIDE.md](docs/PROFESSOR_QUICK_GUIDE.md)** | Durante aula | Roteiro cronometrado (2h45) + FAQ |
| **[AULA1_COMPLETE_CHECKLIST.md](docs/AULA1_COMPLETE_CHECKLIST.md)** | Referência | Status de cobertura dos 15 slides |
| **[INDEX.md](docs/INDEX.md)** | Navegação | Índice geral com busca por tópico |