// PixelShaderComplex.hlsl - Versão simplificada para teste
cbuffer ConstantBuffer : register(b0)
{
    float4x4 WorldViewProjection;
    float Time;
    float3 Padding;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
    float2 ScreenPos : TEXCOORD0;
};

float4 PS(PS_INPUT input) : SV_TARGET
{
    // Versão simples: cor base com pulsação
    float4 color = input.Color;
    float pulse = sin(Time * 0.008) * 0.2 + 0.8;
    color.rgb *= pulse;
    return color;
}