// VertexShader.hlsl - Vertex Shader para visualização estática
cbuffer ConstantBuffer : register(b0)
{
    float4x4 WorldViewProjection;
    float Time;
    float3 Padding;
};

struct VS_INPUT
{
    float3 Position : POSITION;
    float4 Color : COLOR;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
    float2 ScreenPos : TEXCOORD0;
    float Depth : TEXCOORD1;
};

PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;
    
    // Transformação direta - os triângulos já estão posicionados corretamente
    float4 worldPos = float4(input.Position, 1.0);
    
    // Projeção final (ortográfica)
    output.Position = mul(worldPos, WorldViewProjection);
    output.Color = input.Color;
    
    // Calcular posição de tela para o pixel shader
    output.ScreenPos = output.Position.xy / output.Position.w;
    
    // Passar depth para o pixel shader para efeitos baseados em profundidade
    output.Depth = output.Position.z / output.Position.w;
    
    return output;
}