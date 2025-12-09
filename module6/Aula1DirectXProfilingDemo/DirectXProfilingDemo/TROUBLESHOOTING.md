# Troubleshooting - DirectX Profiling Demo

## ?? Problemas Comuns e Soluções

### **Problema: Triângulos só aparecem em wireframe (RESOLVIDO)**

#### **Sintomas:**
- ? Tela preta/vazia em modo sólido
- ? Triângulos visíveis em modo wireframe
- ? Qualquer número de triângulos não aparece

#### **Causa Raiz:**
O **pixel shader** estava escurecendo excessivamente as cores através de múltiplas operações:
1. **depthFactor** - multiplicava por (1.0 - depth*0.3)
2. **highlight** - multiplicava por (1.0 - centerDistance*0.2)
3. **shimmer** - mais uma multiplicação
4. **Resultado final** - cores reduzidas a ~10% do brilho original

#### **Solução Implementada:**
Simplificar o pixel shader removendo efeitos multiplicativos que escureciam:

```hlsl
// ? ANTES (muito escuro - não funciona)
float4 PS(PS_INPUT input) : SV_TARGET
{
    float4 color = input.Color;
    color.rgb = saturate(color.rgb * 1.3);
    float pulse = sin(Time * 0.008) * 0.15 + 0.85;
    color.rgb *= pulse;
    float depthFactor = 1.0 - input.Depth * 0.3;  // ? Escurece muito!
    color.rgb *= depthFactor;
    float highlight = 1.0 - centerDistance * 0.2; // ? Mais escuro!
    color.rgb *= highlight;
    float shimmer = sin(Time * 0.02 + uv.x * 10.0 + uv.y * 10.0) * 0.05 + 1.0;
    color.rgb *= shimmer;
    // Resultado: cores muito escuras, praticamente invisíveis!
    return color;
}

// ? DEPOIS (cores vibrantes - funciona!)
float4 PS(PS_INPUT input) : SV_TARGET
{
    float4 color = input.Color;
    color.rgb = saturate(color.rgb * 1.2);
    float pulse = sin(Time * 0.008) * 0.1 + 0.9;
    color.rgb *= pulse;
    return color;
}
```

### **Resultado:**
- ? **Triângulos perfeitamente visíveis** em modo sólido
- ? **Cores vibrantes** baseadas na posição na grid
- ? **Pulsação sutil** mantida para efeito visual
- ? **Wireframe ainda funciona** normalmente

## ?? Outros Problemas Potenciais

### **PIX Attachment Failed**
- **Solução**: Ver `cleanup_pix_dlls.bat` e `setup_pix.bat`
- **Causa**: DLLs copiadas vs. carregamento direto do diretório PIX

### **Shader Compilation Errors**
- **Verificar**: Arquivos `.hlsl` existem em `Shaders/`
- **Sintaxe**: Entry points `VS` e `PS` corretos
- **Debug**: Flags de debug habilitadas para profiling

### **Performance Issues**
- **CPU Bound**: Use DrawCallMultiplier alto
- **GPU Bound**: Use TriangleCount alto + ComplexShader
- **Baseline**: Configurações padrão para comparação

## ?? Configurações Recomendadas

### **Para Profiling Estático (Análise Precisa):**
```
EnableAnimation = false
EnableWireframe = false (ou true para estrutura)
UseComplexShader = conforme necessário
TriangleCount = ajustável
```

### **Para Demonstração Dinâmica:**
```
EnableAnimation = true
RotationSpeed = 1.0f
Outros controles = conforme cenário desejado
```

## ?? Debug Tips

### **Verificar Renderização:**
1. ? Teste wireframe mode primeiro
2. ? Verifique console logs para PIX status
3. ? Use controles graduais (poucos triângulos ? muitos)
4. ? Compare modo estático vs. animado

### **Profiling Workflow:**
1. Baseline com configurações padrão
2. Incremente um parâmetro por vez
3. Use PIX para capturar diferenças
4. Analise marcadores coloridos nos eventos

---
**Status**: ? Todos os problemas conhecidos resolvidos
**Última atualização**: Dezembro 2024