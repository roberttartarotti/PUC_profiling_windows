using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DirectXProfilingDemo
{
    public class DirectXRenderer : IDisposable
    {
        // Dispositivos DirectX
        private Device device;
        private DeviceContext context;
        private SwapChain swapChain;
        private RenderTargetView renderTargetView;
        private Texture2D depthBuffer;
        private DepthStencilView depthStencilView;
        private DepthStencilState depthStencilState;
        private DepthStencilState noDepthStencilState;

        // Pipeline gráfico
        private InputLayout inputLayout;
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private PixelShader pixelShaderComplex;
        private Buffer vertexBuffer;
        private Buffer constantBuffer;
        private RasterizerState solidRasterizer;
        private RasterizerState wireframeRasterizer;

        // Configurações
        public bool UseComplexShader { get; set; }
        public bool EnableOverdraw { get; set; }
        public bool EnableWireframe { get; set; }
        public bool EnableAnimation { get; set; } = false;
        public int DrawCallMultiplier { get; set; } = 1;
        public float RotationSpeed { get; set; } = 1.0f;
        public int TriangleCount { get; set; } = 1000;

        // Estatísticas
        public float CurrentFPS { get; private set; }
        public float FrameTime { get; private set; }
        public int DrawCallCount { get; private set; }

        private Stopwatch frameTimer;
        private long frameCount;
        private float totalFrameTime;
        private float rotationAngle;

        // Estruturas para buffers
        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public SharpDX.Vector3 Position;
            public SharpDX.Vector4 Color;

            public Vertex(SharpDX.Vector3 pos, SharpDX.Vector4 color)
            {
                Position = pos;
                Color = color;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ConstantBuffer
        {
            public Matrix WorldViewProjection;
            public float Time;
            public SharpDX.Vector3 Padding;
        }

        public DirectXRenderer(IntPtr windowHandle)
        {
            try
            {
                InitializeDeviceAndSwapChain(windowHandle);
                InitializeShaders();
                InitializeBuffers();

                frameTimer = Stopwatch.StartNew();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectX initialization failed: {ex.Message}");
                throw;
            }
        }

        private void InitializeDeviceAndSwapChain(IntPtr windowHandle)
        {
            var modeDescription = new ModeDescription(
                800, 600, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            var swapChainDescription = new SwapChainDescription()
            {
                ModeDescription = modeDescription,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 2,
                OutputHandle = windowHandle,
                IsWindowed = true,
                SwapEffect = SwapEffect.Discard
            };

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.Debug,
                swapChainDescription,
                out device,
                out swapChain);

            context = device.ImmediateContext;

            // Configurar render target
            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(device, backBuffer);
            }

            // Criar depth buffer
            var depthBufferDesc = new Texture2DDescription()
            {
                Width = 800,
                Height = 600,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            depthBuffer = new Texture2D(device, depthBufferDesc);
            depthStencilView = new DepthStencilView(device, depthBuffer);

            // Configurar depth stencil state
            var depthStencilDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                IsStencilEnabled = false
            };

            depthStencilState = new DepthStencilState(device, depthStencilDesc);

            // Criar depth stencil state desabilitado para modo estático
            var noDepthStencilDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                IsStencilEnabled = false
            };

            noDepthStencilState = new DepthStencilState(device, noDepthStencilDesc);

            // Configurar viewport
            var viewport = new Viewport(0, 0, 800, 600, 0.0f, 1.0f);
            context.Rasterizer.SetViewport(viewport);
        }

        private void InitializeShaders()
        {
            var vertexShaderByteCode = LoadAndCompileShader("Shaders/VertexShader.hlsl", "VS", "vs_5_0");
            var pixelShaderByteCode = LoadAndCompileShader("Shaders/PixelShader.hlsl", "PS", "ps_5_0");
            var pixelShaderComplexByteCode = LoadAndCompileShader("Shaders/PixelShaderComplex.hlsl", "PS", "ps_5_0");

            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            pixelShaderComplex = new PixelShader(device, pixelShaderComplexByteCode);

            var inputElements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
            };

            inputLayout = new InputLayout(device, vertexShaderByteCode, inputElements);
        }

        private byte[] LoadAndCompileShader(string filePath, string entryPoint, string profile)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Shader file not found: {filePath}");
                }

                string shaderCode = File.ReadAllText(filePath);
                
                var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    shaderCode,
                    entryPoint,
                    profile,
                    SharpDX.D3DCompiler.ShaderFlags.Debug,
                    SharpDX.D3DCompiler.EffectFlags.None);

                Console.WriteLine($"[Shader] Compilado com sucesso: {filePath} ({entryPoint})");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Shader] Erro ao compilar {filePath}: {ex.Message}");
                throw new InvalidOperationException($"Failed to compile shader {filePath}: {ex.Message}", ex);
            }
        }

        private void InitializeBuffers()
        {
            var vertexBufferDesc = new BufferDescription()
            {
                SizeInBytes = 1000000,
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            vertexBuffer = new Buffer(device, vertexBufferDesc);

            var constantBufferDesc = new BufferDescription()
            {
                SizeInBytes = Utilities.SizeOf<ConstantBuffer>(),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            constantBuffer = new Buffer(device, constantBufferDesc);

            var solidRasterizerDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None, // SEM culling para ver todos os triângulos
                IsFrontCounterClockwise = false,
                DepthBias = 0,
                SlopeScaledDepthBias = 0,
                DepthBiasClamp = 0,
                IsDepthClipEnabled = true,
                IsScissorEnabled = false,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false
            };

            solidRasterizer = new RasterizerState(device, solidRasterizerDesc);

            var wireframeRasterizerDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Wireframe,
                CullMode = CullMode.None,
                IsFrontCounterClockwise = false,
                DepthBias = 0,
                SlopeScaledDepthBias = 0,
                DepthBiasClamp = 0,
                IsDepthClipEnabled = true,
                IsScissorEnabled = false,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = true
            };

            wireframeRasterizer = new RasterizerState(device, wireframeRasterizerDesc);
        }

        public void Render()
        {
            if (device == null) return;

            PixHelper.BeginPixEvent($"Frame {frameCount} - Triangles: {TriangleCount}", 0xFF00FF00);

            var startTime = frameTimer.ElapsedMilliseconds;

            // Limpar buffers
            PixHelper.BeginPixEvent("Clear", 0xFFFF0000);
            context.ClearRenderTargetView(renderTargetView, new SharpDX.Color(0.1f, 0.1f, 0.2f, 1.0f));
            context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            PixHelper.EndPixEvent();

            // Configurar pipeline
            PixHelper.BeginPixEvent("Setup Pipeline", 0xFF0000FF);
            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));

            context.Rasterizer.State = EnableWireframe ? wireframeRasterizer : solidRasterizer;

            context.VertexShader.Set(vertexShader);
            context.VertexShader.SetConstantBuffer(0, constantBuffer);

            // Debug: log qual shader será usado
            string shaderType = UseComplexShader ? "ComplexShader" : "SimpleShader";
            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[Render] Usando: {shaderType}, Animation: {EnableAnimation}, Wireframe: {EnableWireframe}");
            }

            context.PixelShader.Set(UseComplexShader ? pixelShaderComplex : pixelShader);
            context.PixelShader.SetConstantBuffer(0, constantBuffer);

            context.OutputMerger.SetDepthStencilState(EnableAnimation ? depthStencilState : noDepthStencilState);
            context.OutputMerger.SetBlendState(null); // Desabilitar blend state explicitamente
            context.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            PixHelper.EndPixEvent();

            // Atualizar buffers
            PixHelper.BeginPixEvent("Update Constant Buffer", 0xFFFFFF00);
            UpdateConstantBuffer();
            PixHelper.EndPixEvent();

            PixHelper.BeginPixEvent("Update Vertex Buffer", 0xFF00FFFF);
            UpdateVertexBuffer();
            PixHelper.EndPixEvent();

            DrawCallCount = 0;

            if (EnableOverdraw)
            {
                PixHelper.BeginPixEvent("Overdraw Rendering (5 passes)", 0xFFFF00FF);
                int overdrawPasses = 5;
                for (int pass = 0; pass < overdrawPasses; pass++)
                {
                    PixHelper.SetPixMarker($"Overdraw Pass {pass + 1}", 0xFFFF0080);
                    RenderTriangles();
                    DrawCallCount++;
                }
                PixHelper.EndPixEvent();
            }
            else
            {
                PixHelper.BeginPixEvent("Normal Rendering", 0xFF80FF00);
                RenderTriangles();
                DrawCallCount = 1;
                PixHelper.EndPixEvent();
            }

            if (DrawCallMultiplier > 1)
            {
                PixHelper.BeginPixEvent($"Additional Draw Calls ({DrawCallMultiplier - 1}x)", 0xFFFF8000);
                for (int i = 1; i < DrawCallMultiplier; i++)
                {
                    PixHelper.SetPixMarker($"Draw Call {i + 1}", 0xFFFF4000);
                    RenderTriangles();
                    DrawCallCount++;
                }
                PixHelper.EndPixEvent();
            }

            PixHelper.BeginPixEvent("Present", 0xFF8000FF);
            swapChain.Present(0, PresentFlags.None);
            PixHelper.EndPixEvent();

            PixHelper.EndPixEvent();

            var endTime = frameTimer.ElapsedMilliseconds;
            FrameTime = endTime - startTime;

            frameCount++;
            totalFrameTime += FrameTime;

            if (frameCount % 60 == 0)
            {
                CurrentFPS = 1000.0f / (totalFrameTime / 60);
                totalFrameTime = 0;
            }
        }

        private void UpdateConstantBuffer()
        {
            context.MapSubresource(constantBuffer, MapMode.WriteDiscard,
                MapFlags.None, out var dataStream);

            var constantData = new ConstantBuffer();

            if (EnableAnimation)
            {
                rotationAngle += 0.008f * RotationSpeed;
                
                var world = Matrix.RotationY(rotationAngle) *
                           Matrix.RotationX(rotationAngle * 0.3f) *
                           Matrix.RotationZ(rotationAngle * 0.1f);

                var view = Matrix.LookAtLH(
                    new SharpDX.Vector3(0, 0, -2.5f),
                    new SharpDX.Vector3(0, 0, 0),
                    SharpDX.Vector3.UnitY);

                var projection = Matrix.PerspectiveFovLH(
                    (float)Math.PI / 3.0f,
                    800f / 600f,
                    0.1f,
                    100.0f);

                constantData.WorldViewProjection = world * view * projection;
            }
            else
            {
                // Modo estático: usar projeção ortográfica com escala apropriada
                var world = Matrix.Identity;
                var view = Matrix.Identity;
                
                // Projeção ortográfica com mais espaço para ver todos os triângulos
                // Triângulos estão em -0.9 a +0.9, então usamos -1.2 a +1.2 para ter margem
                var projection = Matrix.OrthoOffCenterLH(-1.2f, 1.2f, -1.2f, 1.2f, 0.1f, 10.0f);

                constantData.WorldViewProjection = world * view * projection;
                
                rotationAngle += 0.008f * RotationSpeed;
            }

            constantData.Time = rotationAngle * 10;

            dataStream.Write(constantData);
            context.UnmapSubresource(constantBuffer, 0);
        }

        private void UpdateVertexBuffer()
        {
            int verticesPerTriangle = 3;
            int vertexCount = TriangleCount * verticesPerTriangle;

            context.MapSubresource(vertexBuffer, MapMode.WriteDiscard,
                MapFlags.None, out var dataStream);

            var random = new Random(42);

            int gridSize = (int)Math.Ceiling(Math.Sqrt(TriangleCount));
            float spacing = 1.8f / gridSize;
            float startX = -0.9f;
            float startY = -0.9f;

            int triangleIndex = 0;
            int verticesWritten = 0;

            for (int row = 0; row < gridSize && triangleIndex < TriangleCount; row++)
            {
                for (int col = 0; col < gridSize && triangleIndex < TriangleCount; col++)
                {
                    float centerX = startX + col * spacing;
                    float centerY = startY + row * spacing;
                    float centerZ = 0.0f;

                    centerX += ((float)random.NextDouble() - 0.5f) * spacing * 0.2f;
                    centerY += ((float)random.NextDouble() - 0.5f) * spacing * 0.2f;

                    float size = spacing * 0.4f;

                    float normalizedRow = (float)row / Math.Max(1, gridSize - 1);
                    float normalizedCol = (float)col / Math.Max(1, gridSize - 1);
                    
                    var baseColor = new SharpDX.Vector4(
                        0.2f + normalizedCol * 0.8f,
                        0.2f + normalizedRow * 0.8f,
                        0.9f - (normalizedRow * normalizedCol) * 0.4f,
                        1.0f);

                    for (int j = 0; j < 3; j++)
                    {
                        float angle = j * (2.0f * (float)Math.PI / 3.0f);
                        float x = centerX + size * (float)Math.Cos(angle);
                        float y = centerY + size * (float)Math.Sin(angle);
                        float z = centerZ;

                        // Garantir que as cores estão sempre vibrantes
                        var vertexColor = new SharpDX.Vector4(
                            Math.Min(1.0f, Math.Max(0.1f, baseColor.X + j * 0.05f)),
                            Math.Min(1.0f, Math.Max(0.1f, baseColor.Y + j * 0.05f)),
                            Math.Min(1.0f, Math.Max(0.1f, baseColor.Z + j * 0.05f)),
                            1.0f);

                        var vertex = new Vertex(new SharpDX.Vector3(x, y, z), vertexColor);
                        dataStream.Write(vertex);
                        verticesWritten++;
                    }

                    triangleIndex++;
                }
            }

            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[UpdateVertexBuffer] Generated {verticesWritten} vertices for {TriangleCount} triangles");
            }

            context.UnmapSubresource(vertexBuffer, 0);
        }

        private void RenderTriangles()
        {
            int verticesPerTriangle = 3;
            int vertexCount = TriangleCount * verticesPerTriangle;
            
            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[RenderTriangles] Rendering {vertexCount} vertices ({TriangleCount} triangles)");
            }
            
            context.Draw(vertexCount, 0);
        }

        public void Dispose()
        {
            renderTargetView?.Dispose();
            depthStencilView?.Dispose();
            depthBuffer?.Dispose();
            depthStencilState?.Dispose();
            noDepthStencilState?.Dispose();
            swapChain?.Dispose();
            context?.Dispose();
            device?.Dispose();

            vertexShader?.Dispose();
            pixelShader?.Dispose();
            pixelShaderComplex?.Dispose();
            inputLayout?.Dispose();
            vertexBuffer?.Dispose();
            constantBuffer?.Dispose();
            
            solidRasterizer?.Dispose();
            wireframeRasterizer?.Dispose();
        }
    }
}