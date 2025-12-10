using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DirectXProfilingDemo
{
    public class DirectXRenderer : IDisposable
    {
        // Dispositivos DirectX - Objetos principais para comunicação com a GPU
        private Device device;                          // Representa a GPU e cria recursos
        private DeviceContext context;                  // Contexto para enviar comandos para a GPU
        private SwapChain swapChain;                    // Gerencia os buffers de front/back para apresentação
        private RenderTargetView renderTargetView;      // View do buffer onde renderizamos (back buffer)
        private Texture2D depthBuffer;                  // Textura 2D que armazena informações de profundidade
        private DepthStencilView depthStencilView;      // View do buffer de profundidade/stencil
        private DepthStencilState depthStencilState;    // Estado que define como o teste de profundidade funciona
        private DepthStencilState noDepthStencilState;  // Estado que desabilita o teste de profundidade

        // Pipeline gráfico - Componentes que definem como os dados são processados
        private InputLayout inputLayout;                // Define o layout dos vértices (posição, cor, etc.)
        private VertexShader vertexShader;              // Shader que processa cada vértice
        private PixelShader pixelShader;                // Shader simples que processa cada pixel
        private PixelShader pixelShaderComplex;         // Shader complexo (mais cálculos) que processa cada pixel
        private Buffer vertexBuffer;                    // Buffer que contém os dados dos vértices
        private Buffer constantBuffer;                  // Buffer que contém dados constantes (como matriz de transformação)
        private RasterizerState solidRasterizer;        // Estado de rasterização para renderização sólida
        private RasterizerState wireframeRasterizer;    // Estado de rasterização para renderização em wireframe

        // Configurações - Parâmetros que controlam o comportamento da renderização
        public bool UseComplexShader { get; set; }      // Se true, usa shader complexo (mais pesado)
        public bool EnableOverdraw { get; set; }        // Se true, renderiza a mesma geometria 5 vezes (simula overdraw)
        public bool EnableWireframe { get; set; }       // Se true, renderiza apenas as bordas dos triângulos
        public bool EnableAnimation { get; set; } = false;  // Se true, aplica rotação 3D aos triângulos
        public int DrawCallMultiplier { get; set; } = 1;    // Número de vezes que a geometria é renderizada (1 = normal)
        public float RotationSpeed { get; set; } = 1.0f;    // Velocidade da animação de rotação (1.0 = normal)
        public int TriangleCount { get; set; } = 1000;      // Número de triângulos a serem renderizados

        // Estatísticas - Métricas de desempenho coletadas durante a renderização
        public float CurrentFPS { get; private set; }       // Frames por segundo atuais
        public float FrameTime { get; private set; }        // Tempo em milissegundos para renderizar um frame
        public int DrawCallCount { get; private set; }      // Número de draw calls no último frame

        // Variáveis internas para medição de desempenho
        private Stopwatch frameTimer;       // Cronômetro para medir tempo de renderização
        private long frameCount;            // Contador de frames renderizados
        private float totalFrameTime;       // Acumulador de tempo para calcular FPS médio
        private float rotationAngle;        // Ângulo atual de rotação (em radianos)

        // Estruturas para buffers - Definem o layout da memória enviada para a GPU
        
        /// <summary>
        /// Estrutura Vertex: representa um vértice individual
        /// Cada vértice tem posição 3D (x,y,z) e cor RGBA
        /// LayoutKind.Sequential garante que a ordem na memória seja previsível
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public SharpDX.Vector3 Position;    // Posição 3D: x, y, z (12 bytes = 3 floats * 4 bytes)
            public SharpDX.Vector4 Color;       // Cor RGBA: red, green, blue, alpha (16 bytes = 4 floats * 4 bytes)
                                                // Total: 28 bytes por vértice

            public Vertex(SharpDX.Vector3 pos, SharpDX.Vector4 color)
            {
                Position = pos;
                Color = color;
            }
        }

        /// <summary>
        /// Estrutura ConstantBuffer: dados constantes enviados para todos os shaders
        /// Usada para passar transformações e tempo para a GPU
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ConstantBuffer
        {
            public Matrix WorldViewProjection;  // Matriz 4x4 que transforma coordenadas do objeto para a tela (64 bytes)
            public float Time;                  // Tempo decorrido para animações (4 bytes)
            public SharpDX.Vector3 Padding;     // Padding para alinhar a estrutura em múltiplos de 16 bytes (12 bytes)
                                                // Total: 80 bytes (5 * 16 bytes)
        }

        /// <summary>
        /// Construtor: inicializa o renderizador DirectX
        /// </summary>
        /// <param name="windowHandle">Handle (ponteiro) da janela Windows onde renderizaremos</param>
        public DirectXRenderer(IntPtr windowHandle)
        {
            try
            {
                // Passo 1: Criar o device DirectX e swap chain
                InitializeDeviceAndSwapChain(windowHandle);
                
                // Passo 2: Compilar e carregar os shaders (programas que rodam na GPU)
                InitializeShaders();
                
                // Passo 3: Criar os buffers para vértices e constantes
                InitializeBuffers();

                // Iniciar cronômetro para medição de FPS
                frameTimer = Stopwatch.StartNew();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DirectX initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inicializa o dispositivo DirectX e cria a swap chain
        /// A swap chain gerencia o processo de apresentação (front/back buffer)
        /// </summary>
        private void InitializeDeviceAndSwapChain(IntPtr windowHandle)
        {
            // ModeDescription: define as propriedades da tela
            var modeDescription = new ModeDescription(
                800, 600,                           // Resolução: 800x600 pixels
                new Rational(60, 1),                // Taxa de atualização: 60 Hz (60 frames por segundo)
                Format.R8G8B8A8_UNorm);             // Formato de cor: RGBA com 8 bits por canal (32 bits total)

            // SwapChainDescription: configura o swap chain (double buffering)
            var swapChainDescription = new SwapChainDescription()
            {
                ModeDescription = modeDescription,
                SampleDescription = new SampleDescription(1, 0),    // 1 sample = sem anti-aliasing; 0 = qualidade padrão
                Usage = Usage.RenderTargetOutput,                   // Uso: buffer será alvo de renderização
                BufferCount = 2,                                    // 2 buffers = double buffering (front + back)
                OutputHandle = windowHandle,                        // Janela onde renderizar
                IsWindowed = true,                                  // Modo janela (não fullscreen)
                SwapEffect = SwapEffect.Discard                     // Discard: descarta conteúdo anterior (mais rápido)
            };

            // Criar device e swap chain juntos
            Device.CreateWithSwapChain(
                DriverType.Hardware,                // Hardware: usa a GPU física
                DeviceCreationFlags.Debug,          // Debug: ativa validação extra (útil para desenvolvimento)
                swapChainDescription,
                out device,
                out swapChain);

            context = device.ImmediateContext;      // Contexto imediato: executa comandos imediatamente

            // Configurar render target - onde desenhamos
            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))  // Buffer 0 = back buffer
            {
                renderTargetView = new RenderTargetView(device, backBuffer);
            }

            // Criar depth buffer - armazena a profundidade de cada pixel (para ordenação 3D)
            var depthBufferDesc = new Texture2DDescription()
            {
                Width = 800,
                Height = 600,
                MipLevels = 1,                                  // 1 = sem mipmaps
                ArraySize = 1,                                  // 1 = não é array de texturas
                Format = Format.D24_UNorm_S8_UInt,              // 24 bits para profundidade + 8 bits para stencil
                SampleDescription = new SampleDescription(1, 0), // Sem MSAA
                Usage = ResourceUsage.Default,                  // Default: GPU pode ler e escrever
                BindFlags = BindFlags.DepthStencil,             // Será usado como depth/stencil buffer
                CpuAccessFlags = CpuAccessFlags.None,           // CPU não acessa diretamente
                OptionFlags = ResourceOptionFlags.None
            };

            depthBuffer = new Texture2D(device, depthBufferDesc);
            depthStencilView = new DepthStencilView(device, depthBuffer);

            // Configurar depth stencil state - define regras do teste de profundidade
            var depthStencilDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,                  // Habilitar teste de profundidade
                DepthWriteMask = DepthWriteMask.All,    // Escrever todos os valores de profundidade
                DepthComparison = Comparison.Less,      // Pixel passa se estiver MAIS PRÓXIMO da câmera
                IsStencilEnabled = false                // Não usar stencil neste exemplo
            };

            depthStencilState = new DepthStencilState(device, depthStencilDesc);

            // Criar depth stencil state desabilitado para modo estático (2D)
            var noDepthStencilDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,     // Desabilitar teste de profundidade (útil para 2D)
                IsStencilEnabled = false
            };

            noDepthStencilState = new DepthStencilState(device, noDepthStencilDesc);

            // Configurar viewport - região da janela onde renderizamos
            var viewport = new Viewport(
                0, 0,           // Canto superior esquerdo (x, y)
                800, 600,       // Largura e altura
                0.0f, 1.0f);    // Profundidade mínima (0.0) e máxima (1.0)
            
            context.Rasterizer.SetViewport(viewport);
        }

        /// <summary>
        /// Compila e inicializa os shaders (programas que rodam na GPU)
        /// </summary>
        private void InitializeShaders()
        {
            // Compilar shaders a partir de arquivos HLSL
            var vertexShaderByteCode = LoadAndCompileShader("Shaders/VertexShader.hlsl", "VS", "vs_5_0");
            var pixelShaderByteCode = LoadAndCompileShader("Shaders/PixelShader.hlsl", "PS", "ps_5_0");
            var pixelShaderComplexByteCode = LoadAndCompileShader("Shaders/PixelShaderComplex.hlsl", "PS", "ps_5_0");

            // Criar objetos de shader
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            pixelShaderComplex = new PixelShader(device, pixelShaderComplexByteCode);

            // InputLayout: define como os vértices são estruturados
            var inputElements = new[]
            {
                // "POSITION": semantic name usado no shader HLSL
                // 0: semantic index (para múltiplos do mesmo tipo)
                // Format.R32G32B32_Float: 3 floats (x, y, z)
                // 0: input slot (de qual buffer vem)
                // 0: offset em bytes (começa no byte 0)
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                
                // "COLOR": semantic name para cor
                // 12: offset = começa após POSITION (3 floats * 4 bytes = 12 bytes)
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
            };

            inputLayout = new InputLayout(device, vertexShaderByteCode, inputElements);
        }

        /// <summary>
        /// Carrega e compila um arquivo de shader HLSL
        /// </summary>
        /// <param name="filePath">Caminho do arquivo .hlsl</param>
        /// <param name="entryPoint">Função principal do shader (ex: "VS" ou "PS")</param>
        /// <param name="profile">Perfil de shader (ex: "vs_5_0" = Vertex Shader 5.0)</param>
        /// <returns>Bytecode compilado do shader</returns>
        private byte[] LoadAndCompileShader(string filePath, string entryPoint, string profile)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Shader file not found: {filePath}");
                }

                string shaderCode = File.ReadAllText(filePath);

                // Compilar shader: converte HLSL para bytecode que a GPU entende
                var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    shaderCode,
                    entryPoint,
                    profile,
                    SharpDX.D3DCompiler.ShaderFlags.Debug,          // Debug: inclui informações para depuração
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

        /// <summary>
        /// Inicializa os buffers de vértices e constantes, e os estados de rasterização
        /// </summary>
        private void InitializeBuffers()
        {
            // Vertex Buffer: armazena os dados dos vértices na GPU
            var vertexBufferDesc = new BufferDescription()
            {
                SizeInBytes = 1000000,                      // 1 MB = espaço para ~35,000 vértices
                Usage = ResourceUsage.Dynamic,              // Dynamic: CPU escreve frequentemente
                BindFlags = BindFlags.VertexBuffer,         // Usado como vertex buffer
                CpuAccessFlags = CpuAccessFlags.Write,      // CPU pode escrever (mas não ler)
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0                     // 0 = não é structured buffer
            };

            vertexBuffer = new Buffer(device, vertexBufferDesc);

            // Constant Buffer: armazena dados constantes (matrizes, tempo, etc.)
            var constantBufferDesc = new BufferDescription()
            {
                SizeInBytes = Utilities.SizeOf<ConstantBuffer>(),  // Tamanho da estrutura ConstantBuffer (80 bytes)
                Usage = ResourceUsage.Dynamic,                      // Atualizado a cada frame
                BindFlags = BindFlags.ConstantBuffer,               // Usado como constant buffer
                CpuAccessFlags = CpuAccessFlags.Write,              // CPU escreve
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            constantBuffer = new Buffer(device, constantBufferDesc);

            // Rasterizer State (Solid): define como polígonos são rasterizados
            var solidRasterizerDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,                  // Preencher triângulos completamente
                CullMode = CullMode.None,                   // Não descartar triângulos (mostrar frente e verso)
                IsFrontCounterClockwise = false,            // Frente = vértices no sentido horário
                DepthBias = 0,                              // Sem bias de profundidade
                SlopeScaledDepthBias = 0,
                DepthBiasClamp = 0,
                IsDepthClipEnabled = true,                  // Clip baseado em profundidade
                IsScissorEnabled = false,                   // Sem scissor test
                IsMultisampleEnabled = false,               // Sem multisampling
                IsAntialiasedLineEnabled = false            // Sem anti-aliasing de linhas
            };

            solidRasterizer = new RasterizerState(device, solidRasterizerDesc);

            // Rasterizer State (Wireframe): renderiza apenas as bordas
            var wireframeRasterizerDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Wireframe,              // Desenhar apenas as bordas dos triângulos
                CullMode = CullMode.None,
                IsFrontCounterClockwise = false,
                DepthBias = 0,
                SlopeScaledDepthBias = 0,
                DepthBiasClamp = 0,
                IsDepthClipEnabled = true,
                IsScissorEnabled = false,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = true             // Anti-aliasing nas linhas (wireframe mais suave)
            };

            wireframeRasterizer = new RasterizerState(device, wireframeRasterizerDesc);
        }

        public void Render()
        {
            if (device == null) return;

            // Marcador PIX: ajuda a identificar este frame no PIX Graphics Debugger
            PixHelper.BeginPixEvent($"Frame {frameCount} - Triangles: {TriangleCount}", 0xFF00FF00);

            var startTime = frameTimer.ElapsedMilliseconds;

            // Limpar buffers - preparar para o novo frame
            PixHelper.BeginPixEvent("Clear", 0xFFFF0000);
            // Limpar render target com cor azul escuro (R=0.1, G=0.1, B=0.2)
            context.ClearRenderTargetView(renderTargetView, new SharpDX.Color(0.1f, 0.1f, 0.2f, 1.0f));
            // Limpar depth/stencil para valores padrão (profundidade = 1.0, stencil = 0)
            context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            PixHelper.EndPixEvent();

            // Configurar pipeline - preparar todos os estados e recursos
            PixHelper.BeginPixEvent("Setup Pipeline", 0xFF0000FF);
            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;  // Cada 3 vértices = 1 triângulo
            context.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));  // Stride = tamanho de 1 vértice

            // Escolher rasterizer: wireframe ou solid
            context.Rasterizer.State = EnableWireframe ? wireframeRasterizer : solidRasterizer;

            // Configurar vertex shader
            context.VertexShader.Set(vertexShader);
            context.VertexShader.SetConstantBuffer(0, constantBuffer);      // Slot 0

            // Debug: mostrar qual shader está sendo usado
            string shaderType = UseComplexShader ? "ComplexShader" : "SimpleShader";
            if (frameCount % 60 == 0)   // A cada 60 frames (aproximadamente 1 segundo)
            {
                Console.WriteLine($"[Render] Usando: {shaderType}, Animation: {EnableAnimation}, Wireframe: {EnableWireframe}");
            }

            // Configurar pixel shader: simples ou complexo
            context.PixelShader.Set(UseComplexShader ? pixelShaderComplex : pixelShader);
            context.PixelShader.SetConstantBuffer(0, constantBuffer);

            // Configurar output merger: depth/stencil state e render targets
            context.OutputMerger.SetDepthStencilState(EnableAnimation ? depthStencilState : noDepthStencilState);
            context.OutputMerger.SetBlendState(null);   // null = sem blending (opaco)
            context.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            PixHelper.EndPixEvent();

            // Atualizar buffers com novos dados
            PixHelper.BeginPixEvent("Update Constant Buffer", 0xFFFFFF00);
            UpdateConstantBuffer();     // Atualizar matriz de transformação e tempo
            PixHelper.EndPixEvent();

            PixHelper.BeginPixEvent("Update Vertex Buffer", 0xFF00FFFF);
            UpdateVertexBuffer();       // Gerar/atualizar posições e cores dos vértices
            PixHelper.EndPixEvent();

            DrawCallCount = 0;

            // Overdraw: renderizar múltiplas vezes para simular sobrecarga
            if (EnableOverdraw)
            {
                PixHelper.BeginPixEvent("Overdraw Rendering (5 passes)", 0xFFFF00FF);
                int overdrawPasses = 5;     // 5 passes = cada pixel é processado 5 vezes
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
                RenderTriangles();      // Renderizar uma vez apenas
                DrawCallCount = 1;
                PixHelper.EndPixEvent();
            }

            // Draw Call Multiplier: simular múltiplas draw calls
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

            // Present: trocar front e back buffer (mostrar na tela)
            PixHelper.BeginPixEvent("Present", 0xFF8000FF);
            swapChain.Present(0, PresentFlags.None);    // 0 = apresentar imediatamente (sem VSync)
            PixHelper.EndPixEvent();

            PixHelper.EndPixEvent();

            // Calcular métricas de desempenho
            var endTime = frameTimer.ElapsedMilliseconds;
            FrameTime = endTime - startTime;    // Tempo em milissegundos

            frameCount++;
            totalFrameTime += FrameTime;

            // Calcular FPS médio a cada 60 frames
            if (frameCount % 60 == 0)
            {
                CurrentFPS = 1000.0f / (totalFrameTime / 60);  // FPS = 1000ms / tempo médio por frame
                totalFrameTime = 0;
            }
        }

        /// <summary>
        /// Atualiza o constant buffer com matriz de transformação e tempo
        /// </summary>
        private void UpdateConstantBuffer()
        {
            // Map: obter ponteiro para escrever no buffer
            // WriteDiscard: descarta conteúdo anterior (mais eficiente)
            context.MapSubresource(constantBuffer, MapMode.WriteDiscard,
                MapFlags.None, out var dataStream);

            var constantData = new ConstantBuffer();

            if (EnableAnimation)
            {
                // Modo animação: rotação 3D contínua
                rotationAngle += 0.008f * RotationSpeed;    // Incrementar ângulo

                // Matriz World: combinar 3 rotações para movimento interessante
                var world = Matrix.RotationY(rotationAngle) *           // Rotação em torno do eixo Y
                           Matrix.RotationX(rotationAngle * 0.3f) *     // Rotação lenta em X
                           Matrix.RotationZ(rotationAngle * 0.1f);      // Rotação muito lenta em Z

                // Matriz View: posição da câmera
                var view = Matrix.LookAtLH(
                    new SharpDX.Vector3(0, 0, -2.5f),   // Posição da câmera (afastada 2.5 unidades em Z)
                    new SharpDX.Vector3(0, 0, 0),       // Olhando para a origem
                    SharpDX.Vector3.UnitY);             // Vetor "up" = eixo Y positivo

                // Matriz Projection: perspectiva 3D
                var projection = Matrix.PerspectiveFovLH(
                    (float)Math.PI / 3.0f,  // FOV = 60 graus (PI/3 radianos)
                    800f / 600f,            // Aspect ratio = largura / altura
                    0.1f,                   // Near plane = 0.1 unidades
                    100.0f);                // Far plane = 100 unidades

                // Combinar matrizes: World * View * Projection
                constantData.WorldViewProjection = world * view * projection;
            }
            else
            {
                // Modo estático: projeção ortográfica 2D
                var world = Matrix.Identity;        // Sem transformação
                var view = Matrix.Identity;         // Sem câmera

                // Projeção ortográfica: sem perspectiva (objetos distantes não ficam menores)
                // -1.2 a +1.2: área visível (maior que -1 a +1 para ter margem)
                var projection = Matrix.OrthoOffCenterLH(-1.2f, 1.2f, -1.2f, 1.2f, 0.1f, 10.0f);

                constantData.WorldViewProjection = world * view * projection;

                // Continuar incrementando ângulo (pode ser usado pelo shader complexo)
                rotationAngle += 0.008f * RotationSpeed;
            }

            // Time: usado para animações no shader (multiplicado por 10 para efeito mais visível)
            constantData.Time = rotationAngle * 10;

            // Escrever dados no buffer e finalizar
            dataStream.Write(constantData);
            context.UnmapSubresource(constantBuffer, 0);
        }

        /// <summary>
        /// Atualiza o vertex buffer com novos vértices (posições e cores)
        /// </summary>
        private void UpdateVertexBuffer()
        {
            int verticesPerTriangle = 3;
            int vertexCount = TriangleCount * verticesPerTriangle;

            // Map: obter acesso ao buffer
            context.MapSubresource(vertexBuffer, MapMode.WriteDiscard,
                MapFlags.None, out var dataStream);

            // Random com seed fixo (42): resultados reproduzíveis
            var random = new Random(42);

            // Organizar triângulos em grade
            int gridSize = (int)Math.Ceiling(Math.Sqrt(TriangleCount));    // Ex: 100 triângulos = grade 10x10
            float spacing = 1.8f / gridSize;                                // Espaço entre triângulos
            float startX = -0.9f;                                           // Começar à esquerda
            float startY = -0.9f;                                           // Começar embaixo

            int triangleIndex = 0;
            int verticesWritten = 0;

            // Gerar triângulos em grade
            for (int row = 0; row < gridSize && triangleIndex < TriangleCount; row++)
            {
                for (int col = 0; col < gridSize && triangleIndex < TriangleCount; col++)
                {
                    // Calcular centro do triângulo
                    float centerX = startX + col * spacing;
                    float centerY = startY + row * spacing;
                    float centerZ = 0.0f;

                    // Adicionar variação aleatória (20% do espaçamento)
                    centerX += ((float)random.NextDouble() - 0.5f) * spacing * 0.2f;
                    centerY += ((float)random.NextDouble() - 0.5f) * spacing * 0.2f;

                    float size = spacing * 0.4f;    // Tamanho do triângulo (40% do espaçamento)

                    // Calcular cor baseada na posição na grade (varia de azul a vermelho/verde)
                    float normalizedRow = (float)row / Math.Max(1, gridSize - 1);
                    float normalizedCol = (float)col / Math.Max(1, gridSize - 1);

                    var baseColor = new SharpDX.Vector4(
                        0.2f + normalizedCol * 0.8f,        // Red: aumenta da esquerda para direita
                        0.2f + normalizedRow * 0.8f,        // Green: aumenta de baixo para cima
                        0.9f - (normalizedRow * normalizedCol) * 0.4f,  // Blue: alto no início
                        1.0f);                              // Alpha: sempre opaco

                    // Gerar 3 vértices do triângulo (dispostos em círculo)
                    for (int j = 0; j < 3; j++)
                    {
                        // Ângulo: 0°, 120°, 240° (distribuição uniforme)
                        float angle = j * (2.0f * (float)Math.PI / 3.0f);
                        float x = centerX + size * (float)Math.Cos(angle);
                        float y = centerY + size * (float)Math.Sin(angle);
                        float z = centerZ;

                        // Variar cor de cada vértice ligeiramente
                        var vertexColor = new SharpDX.Vector4(
                            Math.Min(1.0f, Math.Max(0.1f, baseColor.X + j * 0.05f)),   // Ajustar vermelho
                            Math.Min(1.0f, Math.Max(0.1f, baseColor.Y + j * 0.05f)),   // Ajustar verde
                            Math.Min(1.0f, Math.Max(0.1f, baseColor.Z + j * 0.05f)),   // Ajustar azul
                            1.0f);

                        // Criar e escrever vértice no buffer
                        var vertex = new Vertex(new SharpDX.Vector3(x, y, z), vertexColor);
                        dataStream.Write(vertex);
                        verticesWritten++;
                    }

                    triangleIndex++;
                }
            }

            // Log a cada 60 frames
            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[UpdateVertexBuffer] Generated {verticesWritten} vertices for {TriangleCount} triangles");
            }

            // Unmap: finalizar escrita no buffer
            context.UnmapSubresource(vertexBuffer, 0);
        }

        /// <summary>
        /// Executa um draw call para renderizar os triângulos
        /// </summary>
        private void RenderTriangles()
        {
            int verticesPerTriangle = 3;
            int vertexCount = TriangleCount * verticesPerTriangle;

            // Log a cada 60 frames
            if (frameCount % 60 == 0)
            {
                Console.WriteLine($"[RenderTriangles] Rendering {vertexCount} vertices ({TriangleCount} triangles)");
            }

            // Draw: enviar comando para GPU renderizar os vértices
            // vertexCount: número de vértices a desenhar
            // 0: começar do índice 0 no vertex buffer
            context.Draw(vertexCount, 0);
        }

        /// <summary>
        /// Libera recursos DirectX (importante para evitar memory leaks)
        /// </summary>
        public void Dispose()
        {
            // Dispose de todos os recursos na ordem inversa da criação
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

    /// <summary>
    /// Classe para coletar e calcular métricas de desempenho da renderização
    /// </summary>
    public class PerformanceMetrics
    {
        private Stopwatch frameTimer = Stopwatch.StartNew();        // Cronômetro para medir cada frame
        private Queue<double> frameTimes = new Queue<double>(120);  // Fila com últimos 120 frames (2 segundos a 60 FPS)
        private int frameCount = 0;         // Contador de frames
        private int drawCallCount = 0;      // Número de draw calls no frame atual
        private int triangleCount = 0;      // Número de triângulos no frame atual

        // Propriedades públicas com métricas calculadas
        public double CurrentFPS { get; private set; }          // FPS atual (calculado pela média)
        public double AverageFrameTime { get; private set; }    // Tempo médio por frame em ms
        public double MinFrameTime { get; private set; } = double.MaxValue;  // Menor tempo de frame (melhor caso)
        public double MaxFrameTime { get; private set; }        // Maior tempo de frame (pior caso)
        public int TotalDrawCalls { get; private set; }         // Draw calls do último frame
        public int TotalTriangles { get; private set; }         // Triângulos do último frame
        public double GPUUtilization { get; set; }              // Utilização da GPU (definida externamente, ex: por PIX)

        /// <summary>
        /// Marca o início de um novo frame
        /// </summary>
        public void BeginFrame()
        {
            frameTimer.Restart();   // Reiniciar cronômetro
            drawCallCount = 0;      // Zerar contadores
            triangleCount = 0;
        }

        /// <summary>
        /// Registra um draw call e quantos triângulos foram renderizados
        /// </summary>
        public void RecordDrawCall(int triangles)
        {
            drawCallCount++;
            triangleCount += triangles;
        }

        /// <summary>
        /// Marca o fim de um frame e calcula estatísticas
        /// </summary>
        public void EndFrame()
        {
            double frameTime = frameTimer.Elapsed.TotalMilliseconds;

            // Adicionar tempo do frame à fila
            frameTimes.Enqueue(frameTime);
            if (frameTimes.Count > 120)     // Manter apenas últimos 120 frames
                frameTimes.Dequeue();

            frameCount++;
            TotalDrawCalls = drawCallCount;
            TotalTriangles = triangleCount;

            // Atualizar estatísticas a cada segundo (60 frames) ou quando tiver dados suficientes
            if (frameTimes.Count >= 60 || frameCount % 60 == 0)
            {
                AverageFrameTime = frameTimes.Average();    // Média dos últimos frames
                MinFrameTime = frameTimes.Min();            // Melhor caso
                MaxFrameTime = frameTimes.Max();            // Pior caso
                CurrentFPS = 1000.0 / AverageFrameTime;     // FPS = 1000ms / tempo médio
            }
        }

        /// <summary>
        /// Retorna um texto formatado com todas as estatísticas
        /// </summary>
        public string GetStatsText()
        {
            return $"FPS: {CurrentFPS:F1} | Frame: {AverageFrameTime:F2}ms (min: {MinFrameTime:F2}ms, max: {MaxFrameTime:F2}ms)\n" +
                   $"Draw Calls: {TotalDrawCalls} | Triangles: {TotalTriangles:N0}\n" +
                   $"Target: 60 FPS (16.7ms) | Status: {GetPerformanceStatus()}";
        }

        /// <summary>
        /// Avalia o status de desempenho baseado no frame time
        /// </summary>
        private string GetPerformanceStatus()
        {
            if (AverageFrameTime <= 16.7)           // 60 FPS = 16.7ms por frame
                return "? EXCELLENT (60+ FPS) ";
            else if (AverageFrameTime <= 33.3)      // 30 FPS = 33.3ms por frame
                return "?? ACCEPTABLE (30-60 FPS) ";
            else
                return "? POOR (<30 FPS) ";
        }

        /// <summary>
        /// Reseta todas as estatísticas
        /// </summary>
        public void Reset()
        {
            frameTimes.Clear();
            frameCount = 0;
            drawCallCount = 0;
            triangleCount = 0;
            MinFrameTime = double.MaxValue;
            MaxFrameTime = 0;
        }
    }
}