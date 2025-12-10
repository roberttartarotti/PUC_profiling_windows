// PixelShader.hlsl - Pixel Shader simplificado para debug
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
    float Depth : TEXCOORD1;
};

float4 PS(PS_INPUT input) : SV_TARGET
{
    // Versão mínima: apenas retornar a cor sem nenhum processamento
    return input.Color;
}