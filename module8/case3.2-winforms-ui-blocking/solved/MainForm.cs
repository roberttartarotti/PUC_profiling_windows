using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIThreadBlocking.WinForms.Solved
{
    public partial class MainForm : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private CancellationTokenSource? _cts;
        private TextBox resultTextBox;
        private Button loadButton;
        private Button cancelButton;
        private Button clearButton;
        private Label statusLabel;
        private ProgressBar progressBar;

        public MainForm()
        {
            InitializeComponent();
        }

        #region UI Initialization
        private void InitializeComponent()
        {
            this.Text = "Caso 3.2: UI Thread Bloqueada - Windows Forms RESOLVIDO ✅";
            this.Size = new System.Drawing.Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(15);

            // Título
            var titleLabel = new Label
            {
                Text = "UI Thread Bloqueada - Windows Forms RESOLVIDO ✅",
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(15, 15)
            };

            // Painel de solução
            var solutionPanel = new Panel
            {
                Location = new System.Drawing.Point(15, 50),
                Size = new System.Drawing.Size(650, 120),
                BackColor = System.Drawing.Color.FromArgb(240, 255, 244),
                BorderStyle = BorderStyle.FixedSingle
            };

            var solutionLabel = new Label
            {
                Text = "✅ SOLUÇÃO: async/await + Task.Run\n\n" +
                       "Agora você pode interagir livremente enquanto carrega:\n" +
                       "  ✅ Mover a janela\n" +
                       "  ✅ Clicar em 'Cancelar' para interromper\n" +
                       "  ✅ Ver progresso em tempo real\n\n" +
                       "A UI permanece 100% responsiva! 🚀",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(625, 100),
                ForeColor = System.Drawing.Color.DarkGreen
            };
            solutionPanel.Controls.Add(solutionLabel);

            // Botões
            loadButton = new Button
            {
                Text = "Carregar Dados",
                Location = new System.Drawing.Point(15, 185),
                Size = new System.Drawing.Size(130, 35),
                Font = new System.Drawing.Font("Segoe UI", 10),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loadButton.Click += LoadButton_Click;

            cancelButton = new Button
            {
                Text = "Cancelar",
                Location = new System.Drawing.Point(155, 185),
                Size = new System.Drawing.Size(90, 35),
                Font = new System.Drawing.Font("Segoe UI", 10),
                Enabled = false
            };
            cancelButton.Click += CancelButton_Click;

            clearButton = new Button
            {
                Text = "Limpar",
                Location = new System.Drawing.Point(255, 185),
                Size = new System.Drawing.Size(90, 35),
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            clearButton.Click += ClearButton_Click;

            // TextBox de resultado
            resultTextBox = new TextBox
            {
                Location = new System.Drawing.Point(15, 235),
                Size = new System.Drawing.Size(650, 180),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = System.Drawing.Color.WhiteSmoke,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(15, 430),
                Size = new System.Drawing.Size(650, 25),
                Minimum = 0,
                Maximum = 100,
                Visible = false
            };

            // Status label
            statusLabel = new Label
            {
                Text = "Pronto",
                Location = new System.Drawing.Point(15, 465),
                Size = new System.Drawing.Size(650, 20),
                ForeColor = System.Drawing.Color.Gray
            };

            // Adicionar controles ao form
            this.Controls.Add(titleLabel);
            this.Controls.Add(solutionPanel);
            this.Controls.Add(loadButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(clearButton);
            this.Controls.Add(resultTextBox);
            this.Controls.Add(progressBar);
            this.Controls.Add(statusLabel);
        }
        #endregion
        // ✅ SOLUÇÃO: async/await em event handler
        private async void LoadButton_Click(object? sender, EventArgs e)
        {
            // Criar novo CancellationToken
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            statusLabel.Text = "Carregando dados...";
            progressBar.Visible = true;
            progressBar.Value = 0;
            loadButton.Enabled = false;
            cancelButton.Enabled = true;

            try
            {
                // ✅ IProgress para atualizar UI thread
                var progress = new Progress<int>(percent =>
                {
                    progressBar.Value = percent;
                    statusLabel.Text = $"Progresso: {percent}%";
                });

                // ✅ SOLUÇÃO 1: Download assíncrono
                var data = await DownloadDataAsync(_cts.Token, progress);

                // ✅ SOLUÇÃO 2: Processamento em Task.Run
                var processed = await ProcessDataAsync(data, _cts.Token, progress);

                // ✅ SOLUÇÃO 3: I/O assíncrono
                await SaveDataAsync(processed, _cts.Token);

                resultTextBox.Text = processed;
                statusLabel.Text = "Concluído! ✅";
            }
            catch (OperationCanceledException)
            {
                statusLabel.Text = "Operação cancelada pelo usuário";
                MessageBox.Show("Operação cancelada com sucesso.", "Informação",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Erro ao carregar";
            }
            finally
            {
                progressBar.Visible = false;
                loadButton.Enabled = true;
                cancelButton.Enabled = false;
            }
        }

        // ✅ HttpClient assíncrono
        private async Task<string> DownloadDataAsync(
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            progress.Report(10);
            statusLabel.Text = "Baixando dados da internet...";

            // ✅ await em vez de .Result
            var response = await _httpClient.GetStringAsync(
                "https://jsonplaceholder.typicode.com/posts",
                cancellationToken);

            progress.Report(30);

            // ✅ Task.Delay em vez de Thread.Sleep
            await Task.Delay(2000, cancellationToken);

            progress.Report(40);
            return response;
        }

        // ✅ Task.Run para CPU-bound work
        private async Task<string> ProcessDataAsync(
            string data,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            progress.Report(50);
            
            // ✅ Atualizar UI antes de iniciar background work
            statusLabel.Text = "Processando dados...";

            // ✅ SOLUÇÃO: Task.Run move processamento para thread pool
            var result = await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== DADOS PROCESSADOS (Windows Forms - ASYNC) ===\r\n");

                // Processamento pesado executado em background thread
                for (int i = 0; i < 100; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Simular trabalho pesado
                    for (int j = 0; j < 1000000; j++)
                    {
                        _ = Math.Sqrt(j);
                    }

                    if (i % 10 == 0)
                    {
                        sb.AppendLine($"✅ Processando lote {i}/100...");

                        // ✅ Progress<T> automaticamente marshal para UI thread
                        var currentProgress = 50 + (i * 30 / 100);
                        progress.Report(currentProgress);
                    }
                }

                sb.AppendLine($"\r\n📊 Total de caracteres: {data.Length}");
                sb.AppendLine($"⏰ Processado em: {DateTime.Now:HH:mm:ss}");
                sb.AppendLine($"🧵 Thread ID: {Environment.CurrentManagedThreadId}");
                sb.AppendLine($"\r\n✅ UI permaneceu RESPONSIVA durante todo o processamento!");

                return sb.ToString();
            }, cancellationToken);

            progress.Report(80);
            return result;
        }

        // ✅ I/O assíncrono
        private async Task SaveDataAsync(string data, CancellationToken cancellationToken)
        {
            statusLabel.Text = "Salvando arquivo...";

            var tempPath = Path.Combine(Path.GetTempPath(), "winforms_data_solved.txt");

            // ✅ File I/O assíncrono
            await File.WriteAllTextAsync(tempPath, data, cancellationToken);

            // ✅ Delay assíncrono
            await Task.Delay(500, cancellationToken);

            statusLabel.Text = $"✅ Salvo em: {tempPath}";
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            statusLabel.Text = "Cancelando operação...";
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            resultTextBox.Clear();
            statusLabel.Text = "Pronto";
            progressBar.Value = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Dispose();
                _httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
