using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace DirectXProfilingDemo
{
    public partial class MainForm : Form
    {
        private DirectXRenderer renderer;
        private System.Windows.Forms.Timer renderTimer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter gpuCounter;

        // Controles para simular problemas
        private int triangleCount = 1000;
        private bool useComplexShader = false;
        private bool enableOverdraw = false;
        private bool enableWireframe = false;
        private bool enableAnimation = false; // Nova variável para controlar animação
        private int drawCallMultiplier = 1;
        private float rotationSpeed = 1.0f;

        // Estatísticas
        private float currentFPS = 0;
        private float frameTime = 0;
        private int currentDrawCalls = 0;

        public MainForm()
        {
            InitializeComponent();
            InitializePerformanceCounters();
            SetupRenderer();
            SetupTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "DirectX Profiling Demo - Aula 1";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.DoubleBuffered = false;

            // Panel para controles
            var controlPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Labels de estatísticas
            var statsLabel = new System.Windows.Forms.Label
            {
                Text = "ESTATÍSTICAS DE PERFORMANCE",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(10, 10),
                Size = new Size(280, 25)
            };

            var fpsLabel = new System.Windows.Forms.Label
            {
                Name = "lblFPS",
                Text = "FPS: 0",
                Location = new Point(10, 40),
                Size = new Size(280, 20)
            };

            var frameTimeLabel = new System.Windows.Forms.Label
            {
                Name = "lblFrameTime",
                Text = "Tempo por Frame: 0ms",
                Location = new Point(10, 60),
                Size = new Size(280, 20)
            };

            var drawCallsLabel = new System.Windows.Forms.Label
            {
                Name = "lblDrawCalls",
                Text = "Draw Calls: 0",
                Location = new Point(10, 80),
                Size = new Size(280, 20)
            };

            var cpuUsageLabel = new System.Windows.Forms.Label
            {
                Name = "lblCPU",
                Text = "Uso CPU: 0%",
                Location = new Point(10, 100),
                Size = new Size(280, 20)
            };

            var gpuUsageLabel = new System.Windows.Forms.Label
            {
                Name = "lblGPU",
                Text = "Uso GPU: 0%",
                Location = new Point(10, 120),
                Size = new Size(280, 20)
            };

            // Controles para simular problemas
            var controlsLabel = new System.Windows.Forms.Label
            {
                Text = "CONTROLES DE PROBLEMAS",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(10, 160),
                Size = new Size(280, 25)
            };

            // Controle: Número de triângulos
            var triangleLabel = new System.Windows.Forms.Label
            {
                Text = $"Triângulos: {triangleCount}",
                Location = new Point(10, 190),
                Size = new Size(150, 20)
            };

            var triangleTrackBar = new TrackBar
            {
                Minimum = 100,
                Maximum = 10000,
                Value = triangleCount,
                Location = new Point(10, 210),
                Size = new Size(260, 45),
                TickFrequency = 500
            };

            triangleTrackBar.ValueChanged += (s, e) =>
            {
                triangleCount = triangleTrackBar.Value;
                triangleLabel.Text = $"Triângulos: {triangleCount}";
            };

            // Controle: Shader complexo
            var shaderCheckbox = new CheckBox
            {
                Text = "Shader Complexo (Pixel Heavy)",
                Location = new Point(10, 260),
                Size = new Size(260, 20),
                Checked = useComplexShader
            };

            shaderCheckbox.CheckedChanged += (s, e) =>
            {
                useComplexShader = shaderCheckbox.Checked;
                if (renderer != null)
                    renderer.UseComplexShader = useComplexShader;
            };

            // Controle: Overdraw
            var overdrawCheckbox = new CheckBox
            {
                Text = "Simular Overdraw",
                Location = new Point(10, 290),
                Size = new Size(260, 20),
                Checked = enableOverdraw
            };

            overdrawCheckbox.CheckedChanged += (s, e) =>
            {
                enableOverdraw = overdrawCheckbox.Checked;
                if (renderer != null)
                    renderer.EnableOverdraw = enableOverdraw;
            };

            // Controle: Wireframe
            var wireframeCheckbox = new CheckBox
            {
                Text = "Modo Wireframe (Melhor Visualização)",
                Location = new Point(10, 320),
                Size = new Size(260, 20),
                Checked = enableWireframe
            };

            wireframeCheckbox.CheckedChanged += (s, e) =>
            {
                enableWireframe = wireframeCheckbox.Checked;
                if (renderer != null)
                    renderer.EnableWireframe = enableWireframe;
            };

            // Controle: Animação
            var animationCheckbox = new CheckBox
            {
                Text = "Habilitar Animação 3D",
                Location = new Point(10, 350),
                Size = new Size(260, 20),
                Checked = enableAnimation
            };

            animationCheckbox.CheckedChanged += (s, e) =>
            {
                enableAnimation = animationCheckbox.Checked;
                if (renderer != null)
                    renderer.EnableAnimation = enableAnimation;
            };

            // Controle: Multiplicador de Draw Calls
            var drawCallLabel = new System.Windows.Forms.Label
            {
                Text = $"Multiplicador Draw Calls: {drawCallMultiplier}x",
                Location = new Point(10, 380),
                Size = new Size(260, 20)
            };

            var drawCallTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = drawCallMultiplier,
                Location = new Point(10, 400),
                Size = new Size(260, 45)
            };

            drawCallTrackBar.ValueChanged += (s, e) =>
            {
                drawCallMultiplier = drawCallTrackBar.Value;
                drawCallLabel.Text = $"Multiplicador Draw Calls: {drawCallMultiplier}x";
                if (renderer != null)
                    renderer.DrawCallMultiplier = drawCallMultiplier;
            };

            // Controle: Velocidade de rotação
            var speedLabel = new System.Windows.Forms.Label
            {
                Text = $"Velocidade Rotação: {rotationSpeed:F1}x",
                Location = new Point(10, 450),
                Size = new Size(260, 20)
            };

            var speedTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 50,
                Value = (int)(rotationSpeed * 10),
                Location = new Point(10, 470),
                Size = new Size(260, 45)
            };

            speedTrackBar.ValueChanged += (s, e) =>
            {
                rotationSpeed = speedTrackBar.Value / 10f;
                speedLabel.Text = $"Velocidade Rotação: {rotationSpeed:F1}x";
                if (renderer != null)
                    renderer.RotationSpeed = rotationSpeed;
            };

            // Botões de ação
            var captureButton = new Button
            {
                Text = "CAPTURAR COM PIX (Nota: Rode PIX separadamente)",
                BackColor = Color.LightGreen,
                Location = new Point(10, 530),
                Size = new Size(260, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            captureButton.Click += (s, e) =>
            {
                string pixStatus = PixHelper.IsPixReady ? "✅ PRONTO" : "❌ NÃO DISPONÍVEL";
                string instructions = PixHelper.IsPixReady 
                    ? "1. Abra o PIX for Windows como Administrador\n2. Clique em 'Attach to Process'\n3. Selecione 'DirectXProfilingDemo.exe'\n4. Configure os controles para criar cenários específicos\n5. Clique em 'Take GPU Capture' no PIX\n6. Interaja com a aplicação\n7. Pare a captura no PIX para analisar"
                    : "1. Instale o PIX for Windows da Microsoft Store\n2. Reinicie esta aplicação\n3. O PIX será automaticamente inicializado\n\nAlternativamente, você ainda pode usar:\n- Visual Studio Diagnostic Tools\n- Outras ferramentas de profiling de CPU";

                MessageBox.Show($"Status do PIX: {pixStatus}\n\n{instructions}\n\n" +
                    "CENÁRIOS DE TESTE:\n" +
                    "• GPU Bottleneck: Muitos triângulos + Shader Complexo + Overdraw\n" +
                    "• CPU Bottleneck: Multiplicador de Draw Calls alto\n" +
                    "• Fillrate Issues: Overdraw habilitado\n" +
                    "• Baseline: Configurações padrão\n" +
                    "• Visualização Estática: Animação desabilitada (melhor para análise)",
                    "Instruções PIX - DirectX Profiling Demo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var resetButton = new Button
            {
                Text = "Resetar para Valores Normais",
                BackColor = Color.LightSkyBlue,
                Location = new Point(10, 580),
                Size = new Size(260, 40)
            };

            resetButton.Click += (s, e) =>
            {
                triangleTrackBar.Value = 1000;
                shaderCheckbox.Checked = false;
                overdrawCheckbox.Checked = false;
                wireframeCheckbox.Checked = false;
                animationCheckbox.Checked = false;
                drawCallTrackBar.Value = 1;
                speedTrackBar.Value = 10;
            };

            // Adicionar controles ao painel
            controlPanel.Controls.AddRange(new Control[]
            {
                statsLabel, fpsLabel, frameTimeLabel, drawCallsLabel,
                cpuUsageLabel, gpuUsageLabel, controlsLabel,
                triangleLabel, triangleTrackBar, shaderCheckbox,
                overdrawCheckbox, wireframeCheckbox, animationCheckbox, drawCallLabel, drawCallTrackBar,
                speedLabel, speedTrackBar, captureButton, resetButton
            });

            this.Controls.Add(controlPanel);

            // Área de renderização
            var renderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            this.Controls.Add(renderPanel);

            // Salvar referência do painel de renderização
            renderPanel.HandleCreated += (s, e) =>
            {
                if (renderer == null)
                {
                    renderer = new DirectXRenderer(renderPanel.Handle);
                    renderer.UseComplexShader = useComplexShader;
                    renderer.EnableOverdraw = enableOverdraw;
                    renderer.EnableWireframe = enableWireframe;
                    renderer.EnableAnimation = enableAnimation;
                    renderer.DrawCallMultiplier = drawCallMultiplier;
                    renderer.RotationSpeed = rotationSpeed;
                    renderer.TriangleCount = triangleCount;
                }
            };

            // Atualizar estatísticas no timer
            this.Controls.Find("lblFPS", true)[0].Name = "lblFPS";
            this.Controls.Find("lblFrameTime", true)[0].Name = "lblFrameTime";
            this.Controls.Find("lblDrawCalls", true)[0].Name = "lblDrawCalls";
            this.Controls.Find("lblCPU", true)[0].Name = "lblCPU";
            this.Controls.Find("lblGPU", true)[0].Name = "lblGPU";
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                // Nota: Contador de GPU requer configuração específica no Windows
                // Para simplificar, vamos simular valores
                gpuCounter = null;
            }
            catch
            {
                // Em caso de erro, usar contadores simulados
                cpuCounter = null;
                gpuCounter = null;
            }
        }

        private void SetupRenderer()
        {
            // Renderer será criado quando o handle do painel estiver disponível
        }

        private void SetupTimer()
        {
            renderTimer = new System.Windows.Forms.Timer();
            renderTimer.Interval = 16; // ~60 FPS
            renderTimer.Tick += (s, e) =>
            {
                if (renderer != null)
                {
                    // Atualizar renderização
                    renderer.TriangleCount = triangleCount;
                    renderer.Render();

                    // Coletar estatísticas
                    currentFPS = renderer.CurrentFPS;
                    frameTime = renderer.FrameTime;
                    currentDrawCalls = renderer.DrawCallCount;

                    // Atualizar labels
                    UpdateStatsLabels();
                }
            };
            renderTimer.Start();
        }

        private void UpdateStatsLabels()
        {
            // Encontrar labels pelo nome
            var fpsLabel = Controls.Find("lblFPS", true)[0] as System.Windows.Forms.Label;
            var frameTimeLabel = Controls.Find("lblFrameTime", true)[0] as System.Windows.Forms.Label;
            var drawCallsLabel = Controls.Find("lblDrawCalls", true)[0] as System.Windows.Forms.Label;
            var cpuLabel = Controls.Find("lblCPU", true)[0] as System.Windows.Forms.Label;
            var gpuLabel = Controls.Find("lblGPU", true)[0] as System.Windows.Forms.Label;

            if (fpsLabel != null)
                fpsLabel.Text = $"FPS: {currentFPS:F1}";

            if (frameTimeLabel != null)
                frameTimeLabel.Text = $"Tempo por Frame: {frameTime:F2}ms";

            if (drawCallsLabel != null)
                drawCallsLabel.Text = $"Draw Calls: {currentDrawCalls}";

            // Obter uso de CPU
            float cpuUsage = 0;
            if (cpuCounter != null)
            {
                try { cpuUsage = cpuCounter.NextValue(); }
                catch { cpuUsage = 0; }
            }

            if (cpuLabel != null)
                cpuLabel.Text = $"Uso CPU: {cpuUsage:F1}%";

            // Simular uso de GPU baseado na complexidade da cena
            float gpuUsage = Math.Min(100, (triangleCount / 10000f * 70f) +
                (useComplexShader ? 20 : 0) +
                (enableOverdraw ? 10 : 0));

            if (gpuLabel != null)
                gpuLabel.Text = $"Uso GPU: {gpuUsage:F1}%";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            renderTimer?.Stop();
            renderer?.Dispose();
            cpuCounter?.Dispose();
            base.OnFormClosing(e);
        }
    }
}