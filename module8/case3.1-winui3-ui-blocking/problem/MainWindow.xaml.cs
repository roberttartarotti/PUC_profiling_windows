using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace UIThreadBlocking.WinUI3.Problem
{
    public sealed partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            this.InitializeComponent();
            Title = "Caso 3.1: UI Thread Bloqueada - WinUI 3 PROBLEMA";
        }

        // PROBLEMA: Operação síncrona na UI thread!
        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Carregando dados...";
            LoadingProgressRing.IsActive = true;
            LoadButton.IsEnabled = false;

            try
            {
                // PROBLEMA 1: Download síncrono bloqueia UI
                var data = DownloadDataSync();

                // PROBLEMA 2: Processamento pesado bloqueia UI
                var processed = ProcessDataSync(data);

                // PROBLEMA 3: Escrita de arquivo bloqueia UI
                SaveDataSync(processed);

                ResultTextBox.Text = processed;
                StatusTextBlock.Text = "Concluído!";
            }
            catch (Exception ex)
            {
                ShowErrorDialog(ex.Message);
                StatusTextBlock.Text = "Erro ao carregar";
            }
            finally
            {
                LoadingProgressRing.IsActive = false;
                LoadButton.IsEnabled = true;
            }
        }

        private string DownloadDataSync()
        {
            StatusTextBlock.Text = "Baixando dados da internet...";

            // ❌ PROBLEMA: .Result bloqueia a thread!
            var response = _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/posts").Result;

            // ❌ PROBLEMA: Thread.Sleep congela a UI!
            Thread.Sleep(2000);

            return response;
        }

        private string ProcessDataSync(string data)
        {
            StatusTextBlock.Text = "Processando dados...";

            var sb = new StringBuilder();
            sb.AppendLine("=== DADOS PROCESSADOS (WinUI 3) ===\n");

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
                    sb.AppendLine($"Processando lote {i}/100...");
                }
            }

            sb.AppendLine($"\nTotal de caracteres recebidos: {data.Length}");
            sb.AppendLine($"Processado em: {DateTime.Now:HH:mm:ss}");

            return sb.ToString();
        }

        private void SaveDataSync(string data)
        {
            StatusTextBlock.Text = "Salvando arquivo...";

            var tempPath = Path.Combine(Path.GetTempPath(), "winui3_data_problem.txt");

            // ❌ PROBLEMA: I/O síncrono bloqueia UI!
            File.WriteAllText(tempPath, data);

            // Adicionar delay para evidenciar
            Thread.Sleep(500);

            StatusTextBlock.Text = $"Salvo em: {tempPath}";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = string.Empty;
            StatusTextBlock.Text = "Pronto";
        }

        private async void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Erro",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
