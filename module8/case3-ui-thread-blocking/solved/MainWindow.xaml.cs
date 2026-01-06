using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UIThreadBlocking.Solved
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private CancellationTokenSource? _cancellationTokenSource;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // SOLUÇÃO: async/await para manter UI responsiva!
        private async void LoadData_Click(object sender, RoutedEventArgs e)
        {
            // Criar token de cancelamento
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            StatusLabel.Content = "Carregando dados...";
            ProgressBar.Visibility = Visibility.Visible;
            LoadButton.IsEnabled = false;
            CancelButton.Visibility = Visibility.Visible;

            try
            {
                // SOLUÇÃO 1: Download assíncrono
                var data = await DownloadDataAsync(cancellationToken);
                
                // Verificar cancelamento
                cancellationToken.ThrowIfCancellationRequested();
                
                // SOLUÇÃO 2: Processamento em background thread
                var progress = new Progress<int>(value =>
                {
                    // Este callback executa na UI thread automaticamente!
                    ProgressBar.Value = value;
                    StatusLabel.Content = $"Processando... {value}%";
                });
                
                var processed = await ProcessDataAsync(data, progress, cancellationToken);
                
                // Verificar cancelamento
                cancellationToken.ThrowIfCancellationRequested();
                
                // SOLUÇÃO 3: I/O assíncrono
                await SaveDataAsync(processed, cancellationToken);

                ResultTextBox.Text = processed;
                StatusLabel.Content = "Concluído!";
            }
            catch (OperationCanceledException)
            {
                StatusLabel.Content = "Operação cancelada";
                MessageBox.Show("Operação cancelada pelo usuário", "Cancelado", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Content = "Erro ao carregar";
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0;
                LoadButton.IsEnabled = true;
                CancelButton.Visibility = Visibility.Collapsed;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task<string> DownloadDataAsync(CancellationToken cancellationToken)
        {
            StatusLabel.Content = "Baixando dados da internet...";
            
            // ASYNC: Não bloqueia UI thread
            var response = await _httpClient.GetStringAsync(
                "https://jsonplaceholder.typicode.com/posts", 
                cancellationToken);
            
            // Simular delay
            await Task.Delay(2000, cancellationToken);
            
            return response;
        }

        private async Task<string> ProcessDataAsync(string data, IProgress<int> progress, 
            CancellationToken cancellationToken)
        {
            StatusLabel.Content = "Processando dados...";
            
            // SOLUÇÃO: Executar em thread pool, não na UI thread
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== DADOS PROCESSADOS (ASYNC) ===\n");
                
                // Processamento CPU-bound em background
                for (int i = 0; i < 100; i++)
                {
                    // Verificar cancelamento
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Trabalho pesado
                    for (int j = 0; j < 1000000; j++)
                    {
                        _ = Math.Sqrt(j);
                    }
                    
                    sb.AppendLine($"Registro {i + 1} processado");
                    
                    // Reportar progresso (vai para UI thread automaticamente)
                    progress.Report(i + 1);
                }
                
                return sb.ToString();
            }, cancellationToken);
        }

        private async Task SaveDataAsync(string data, CancellationToken cancellationToken)
        {
            StatusLabel.Content = "Salvando em arquivo...";
            
            // I/O ASSÍNCRONO
            var filePath = Path.Combine(Path.GetTempPath(), "resultado_async.txt");
            await File.WriteAllTextAsync(filePath, data, cancellationToken);
            
            await Task.Delay(1000, cancellationToken);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
            StatusLabel.Content = "Pronto";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            CancelButton.IsEnabled = false;
            StatusLabel.Content = "Cancelando...";
        }

        // Cleanup
        protected override void OnClosed(EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _httpClient.Dispose();
            base.OnClosed(e);
        }
    }
}
