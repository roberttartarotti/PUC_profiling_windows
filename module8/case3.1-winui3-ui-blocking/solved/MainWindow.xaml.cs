using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UIThreadBlocking.WinUI3.Solved
{
    public sealed partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            this.InitializeComponent();
            Title = "Caso 3.1: UI Thread Bloqueada - WinUI 3 RESOLVIDO";
        }

        // ✅ SOLUÇÃO: Operação ASSÍNCRONA com async/await
        private async void LoadData_Click(object sender, RoutedEventArgs e)
        {
            // Criar novo CancellationToken para esta operação
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            StatusTextBlock.Text = "Carregando dados...";
            LoadingProgressRing.IsActive = true;
            LoadButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            ProgressBar.Value = 0;
            ProgressBar.Visibility = Visibility.Visible;

            try
            {
                var progress = new Progress<int>(percent =>
                {
                    ProgressBar.Value = percent;
                    StatusTextBlock.Text = $"Progresso: {percent}%";
                });

                // ✅ SOLUÇÃO 1: Download assíncrono com await
                var data = await DownloadDataAsync(_cts.Token, progress);

                // ✅ SOLUÇÃO 2: Processamento pesado em Task.Run
                var processed = await ProcessDataAsync(data, _cts.Token, progress);

                // ✅ SOLUÇÃO 3: I/O assíncrono
                await SaveDataAsync(processed, _cts.Token);

                ResultTextBox.Text = processed;
                StatusTextBlock.Text = "Concluído! ✅";
            }
            catch (OperationCanceledException)
            {
                StatusTextBlock.Text = "Operação cancelada pelo usuário";
                await ShowInfoDialogAsync("Operação cancelada com sucesso.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(ex.Message);
                StatusTextBlock.Text = "Erro ao carregar";
            }
            finally
            {
                LoadingProgressRing.IsActive = false;
                LoadButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        // ✅ Método assíncrono com HttpClient.GetStringAsync
        private async Task<string> DownloadDataAsync(CancellationToken cancellationToken, IProgress<int> progress)
        {
            progress.Report(10);
            StatusTextBlock.Text = "Baixando dados da internet...";

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

        // ✅ Processamento CPU-bound com Task.Run
        private async Task<string> ProcessDataAsync(string data, CancellationToken cancellationToken, IProgress<int> progress)
        {
            progress.Report(50);
            StatusTextBlock.Text = "Processando dados...";

            // ✅ SOLUÇÃO: Task.Run para não bloquear UI thread
            var result = await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== DADOS PROCESSADOS (WinUI 3 - ASYNC) ===\n");

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
                        
                        // ✅ Reportar progresso de background thread
                        var currentProgress = 50 + (i * 30 / 100);
                        progress.Report(currentProgress);
                    }
                }

                sb.AppendLine($"\n📊 Total de caracteres recebidos: {data.Length}");
                sb.AppendLine($"⏰ Processado em: {DateTime.Now:HH:mm:ss}");
                sb.AppendLine($"🧵 Thread ID: {Environment.CurrentManagedThreadId}");
                sb.AppendLine($"\n✅ UI permaneceu RESPONSIVA durante todo o processamento!");

                return sb.ToString();
            }, cancellationToken);

            progress.Report(80);
            return result;
        }

        // ✅ I/O assíncrono com WriteAllTextAsync
        private async Task SaveDataAsync(string data, CancellationToken cancellationToken)
        {
            StatusTextBlock.Text = "Salvando arquivo...";

            var tempPath = Path.Combine(Path.GetTempPath(), "winui3_data_solved.txt");

            // ✅ File I/O assíncrono
            await File.WriteAllTextAsync(tempPath, data, cancellationToken);

            // ✅ Delay assíncrono
            await Task.Delay(500, cancellationToken);

            StatusTextBlock.Text = $"✅ Salvo em: {tempPath}";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            StatusTextBlock.Text = "Cancelando operação...";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = string.Empty;
            StatusTextBlock.Text = "Pronto";
            ProgressBar.Value = 0;
        }

        private async Task ShowErrorDialogAsync(string message)
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

        private async Task ShowInfoDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Informação",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
