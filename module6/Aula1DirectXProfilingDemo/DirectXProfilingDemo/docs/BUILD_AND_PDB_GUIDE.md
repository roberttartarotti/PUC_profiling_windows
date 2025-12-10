# Guia de Build e Configuração para Profiling

## Objetivo
Este guia mostra como compilar o projeto corretamente para que o PIX consiga ler símbolos de debug (PDB) e mostrar nomes de funções.

---

## Requisitos

- .NET 10 SDK instalado
- Visual Studio 2022 (ou superior) **OU** .NET CLI
- PIX para Windows instalado

---

## Build via .NET CLI (Recomendado para Aula)

### 1. Build Completo em Debug

Este é o comando que **sempre funciona** para profiling:

```cmd
cd DirectXProfilingDemo
dotnet build -c Debug /p:DebugType=portable /p:DebugSymbols=true /p:Optimize=false
```

**Explicação dos parâmetros:**
- `-c Debug`: Compila em modo Debug (sem otimizações agressivas)
- `/p:DebugType=portable`: Gera PDB no formato **portable** (compatível com .NET Core/5+)
- `/p:DebugSymbols=true`: Força geração de símbolos mesmo que esteja em configuração não-padrão
- `/p:Optimize=false`: Desabilita otimizações que podem dificultar o debug

### 2. Verificar se o PDB foi Gerado

```cmd
dir bin\Debug\net10.0-windows\*.pdb
```

**Saída esperada:**
```
DirectXProfilingDemo.pdb
```

**Se não aparecer:**
- Limpe e recompile: `dotnet clean && dotnet build -c Debug ...`
- Verifique se o `.csproj` tem `<DebugType>portable</DebugType>`

### 3. Executar para Profiling

```cmd
dotnet run --no-build
```

**OU** execute diretamente:
```cmd
bin\Debug\net10.0-windows\DirectXProfilingDemo.exe
```

---

## Build via Visual Studio

### 1. Configurar o Projeto

**A. Abrir `DirectXProfilingDemo.csproj`** (clique direito no projeto -> Editar Arquivo de Projeto)

**B. Certifique-se de que tem:**
```xml
<PropertyGroup>
  <DebugType>portable</DebugType>
  <DebugSymbols>true</DebugSymbols>
  <Optimize>false</Optimize>
</PropertyGroup>
```

**C. Salvar e recarregar o projeto**

### 2. Compilar

- Menu: **Build -> Rebuild Solution** (ou `Ctrl+Shift+B`)
- Ou: Clique direito no projeto -> **Rebuild**

### 3. Verificar Output

Vá para: `bin\Debug\net10.0-windows\`

Deve conter:
```
DirectXProfilingDemo.exe
DirectXProfilingDemo.pdb  <- IMPORTANTE!
DirectXProfilingDemo.dll (se houver)
```

---

## Tipos de PDB: Portable vs Full vs Embedded

### Comparação

| Tipo | Compatibilidade | Tamanho | Recomendação PIX |
|------|----------------|---------|------------------|
| **Portable** | .NET Core, .NET 5+ | Pequeno (~100KB-2MB) | [X] **MELHOR ESCOLHA** |
| **Full (Windows)** | .NET Framework, C++ | Grande (~5MB-50MB) | [X] Funciona mas é legado |
| **Embedded** | Embutido no .exe | .exe maior | [ ] Pode funcionar mas não recomendado |

### Como Configurar Cada Tipo

**No `.csproj`:**

```xml
<!-- RECOMENDADO: Portable (padrão .NET moderno) -->
<DebugType>portable</DebugType>

<!-- Alternativa: Full (legacy, maior) -->
<DebugType>full</DebugType>

<!-- Não recomendado: Embedded -->
<DebugType>embedded</DebugType>
```

---

## Problemas Comuns e Soluções

### Problema 1: "PIX não mostra nomes de funções, apenas endereços"

**Causa:** PDB não está sendo carregado.

**Soluções:**

**A. Verificar se PDB existe:**
```cmd
dir bin\Debug\net10.0-windows\DirectXProfilingDemo.pdb
```

**B. Recompilar forçando PDB:**
```cmd
dotnet clean
dotnet build -c Debug /p:DebugType=portable /p:DebugSymbols=true
```

**C. Verificar no PIX:**
1. Abra a captura `.wpix`
2. Menu: **Tools -> Symbol Settings**
3. Verifique se o caminho do PDB está correto:
   ```
   C:\...\DirectXProfilingDemo\bin\Debug\net10.0-windows\DirectXProfilingDemo.pdb
   ```

**D. Forçar carregamento de símbolos:**
- No PIX, menu: **Debug -> Load Symbols**

---

### Problema 2: "Build falha com erro de SharpDX"

**Causa:** Pacotes NuGet não restaurados.

**Solução:**
```cmd
dotnet restore
dotnet build -c Debug
```

---

### Problema 3: "PIX captura mas não mostra detalhes de GPU"

**Causa:** Drivers de vídeo desatualizados ou incompatíveis.

**Soluções:**
1. **Atualizar drivers**: Vá para o site do fabricante (NVIDIA, AMD, Intel)
2. **Executar como Administrador**: PIX precisa de privilégios elevados
3. **Verificar DirectX 12**: Certifique-se de que sua GPU suporta DX12

---

### Problema 4: "Application crashes when PIX attaches"

**Causa:** Modo Release com otimizações agressivas.

**Solução:**
```cmd
# NUNCA use Release para profiling inicial
dotnet build -c Debug  # <- Use Debug
```

Se precisar de Release por algum motivo:
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <DebugType>portable</DebugType>
  <DebugSymbols>true</DebugSymbols>
  <Optimize>true</Optimize>  <!-- Otimizações OK, mas com símbolos -->
</PropertyGroup>
```

---

## Verificar Configuração do Projeto

### Comando Rápido

```cmd
dotnet msbuild /t:GetTargetPath /p:Configuration=Debug /v:minimal
```

**Saída esperada:**
```
DirectXProfilingDemo.dll
  DebugType: portable
  DebugSymbols: true
  Optimize: false
```

---

## Inspecionar PDB (Avançado)

### Ver Informações do PDB

**Com `dotnet-symbol` (opcional):**
```cmd
dotnet tool install --global dotnet-symbol
dotnet symbol bin\Debug\net10.0-windows\DirectXProfilingDemo.pdb
```

**Com PowerShell:**
```powershell
$pdb = "bin\Debug\net10.0-windows\DirectXProfilingDemo.pdb"
if (Test-Path $pdb) {
    Write-Host "[OK] PDB exists: $pdb"
    $size = (Get-Item $pdb).Length / 1MB
    Write-Host "Size: $($size.ToString('F2')) MB"
} else {
    Write-Host "[FAIL] PDB NOT FOUND!"
}
```


**Autor**: Material didático PUC - Profiling no Windows  
**Atualizado**: 2025  
**Versão**: 1.0
