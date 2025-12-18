using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace UIThreadBlocking.WinForms.Problem
{
    public partial class MainForm : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private TextBox resultTextBox;
        private Button loadButton;
        private Button clearButton;
        private Label statusLabel;
        private ProgressBar progressBar;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Caso 3.2: UI Thread Bloqueada - Windows Forms PROBLEMA";
            this.Size = new System.Drawing.Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(15);

            // Título
            var titleLabel = new Label
            {
                Text = "UI Thread Bloqueada - Windows Forms PROBLEMA",
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(15, 15)
            };

            // Painel de aviso
            var warningPanel = new Panel
            {
                Location = new System.Drawing.Point(15, 50),
                Size = new System.Drawing.Size(650, 120),
                BackColor = System.Drawing.Color.FromArgb(255, 244, 244),
                BorderStyle = BorderStyle.FixedSingle
            };

            var warningLabel = new Label
            {
                Text = "⚠️ PROBLEMA: UI Thread Bloqueada\n\n" +
                       "Clique em 'Carregar Dados' e tente:\n" +
                       "  • Mover a janela\n" +
                       "  • Clicar no botão 'Limpar'\n" +
                       "  • Interagir com qualquer controle\n\n" +
                       "A UI vai congelar completamente! O formulário pode\n" +
                       "até mostrar 'Não Respondendo' no título. 🧊",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(625, 100),
                ForeColor = System.Drawing.Color.DarkRed
            };
            warningPanel.Controls.Add(warningLabel);

            // Botões
            loadButton = new Button
            {
                Text = "Carregar Dados",
                Location = new System.Drawing.Point(15, 185),
                Size = new System.Drawing.Size(130, 35),
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            loadButton.Click += LoadButton_Click;

            clearButton = new Button
            {
                Text = "Limpar",
                Location = new System.Drawing.Point(155, 185),
                Size = new System.Drawing.Size(90, 35),
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            clearButton.Click += ClearButton_Click;

            // TextBox de resultado
            resultTextBox = new TextBox
            {
                Location = new System.Drawing.Point(15, 235),
                Size = new System.Drawing.Size(650, 210),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = System.Drawing.Color.WhiteSmoke,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            // Status bar
            statusLabel = new Label
            {
                Text = "Pronto",
                Location = new System.Drawing.Point(15, 460),
                Size = new System.Drawing.Size(550, 20),
                ForeColor = System.Drawing.Color.Gray
            };

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(575, 460),
                Size = new System.Drawing.Size(90, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Adicionar controles ao form
            this.Controls.Add(titleLabel);
            this.Controls.Add(warningPanel);
            this.Controls.Add(loadButton);
            this.Controls.Add(clearButton);
            this.Controls.Add(resultTextBox);
            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
        }

        // ❌ PROBLEMA: Operação síncrona na UI thread!
        private void LoadButton_Click(object? sender, EventArgs e)
        {
            statusLabel.Text = "Carregando dados...";
            progressBar.Visible = true;
            loadButton.Enabled = false;

            try
            {
                // ❌ PROBLEMA 1: Download síncrono bloqueia UI
                var data = DownloadDataSync();

                // ❌ PROBLEMA 2: Processamento pesado bloqueia UI
                var processed = ProcessDataSync(data);

                // ❌ PROBLEMA 3: I/O síncrono bloqueia UI
                SaveDataSync(processed);

                resultTextBox.Text = processed;
                statusLabel.Text = "Concluído!";
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
            }
        }

        private string DownloadDataSync()
        {
            statusLabel.Text = "Baixando dados da internet...";
            statusLabel.Refresh(); // ⚠️ Force repaint, mas ainda bloqueia

            // ❌ .Result bloqueia a thread!
            var response = _httpClient.GetStringAsync(
                "https://jsonplaceholder.typicode.com/posts").Result;

            // ❌ Thread.Sleep congela a UI!
            Thread.Sleep(2000);

            return response;
        }

        private string ProcessDataSync(string data)
        {
            statusLabel.Text = "Processando dados...";
            statusLabel.Refresh();

            var sb = new StringBuilder();
            sb.AppendLine("=== DADOS PROCESSADOS (Windows Forms) ===\r\n");

            // ❌ PROBLEMA: Processamento CPU-bound na UI thread!
            for (int i = 0; i < 100; i++)
            {
                // Simular trabalho pesado
                for (int j = 0; j < 1000000; j++)
                {
                    _ = Math.Sqrt(j);
                }

                if (i % 10 == 0)
                {
                    sb.AppendLine($"❌ Processando lote {i}/100...");

                    // ⚠️ ANTI-PATTERN: Application.DoEvents()
                    // Cria problemas de reentrada, mas às vezes usado em código legado
                    Application.DoEvents(); // NÃO faça isso!
                }
            }

            sb.AppendLine($"\r\n📊 Total de caracteres: {data.Length}");
            sb.AppendLine($"⏰ Processado em: {DateTime.Now:HH:mm:ss}");
            sb.AppendLine($"🧵 Thread ID: {Environment.CurrentManagedThreadId}");

            return sb.ToString();
        }

        private void SaveDataSync(string data)
        {
            statusLabel.Text = "Salvando arquivo...";
            statusLabel.Refresh();

            var tempPath = Path.Combine(Path.GetTempPath(), "winforms_data_problem.txt");

            // ❌ I/O síncrono bloqueia UI!
            File.WriteAllText(tempPath, data);

            // Adicionar delay para evidenciar
            Thread.Sleep(500);

            statusLabel.Text = $"Salvo em: {tempPath}";
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            resultTextBox.Clear();
            statusLabel.Text = "Pronto";
        }
    }
}
