using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows;

namespace UIThreadBlocking.Problem
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // PROBLEMA: Operação síncrona na UI thread!
        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Content = "Carregando dados...";
            ProgressBar.Visibility = Visibility.Visible;
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
                StatusLabel.Content = "Concluído!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
                StatusLabel.Content = "Erro ao carregar";
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                LoadButton.IsEnabled = true;
            }
        }

        private string DownloadDataSync()
        {
            StatusLabel.Content = "Baixando dados da internet...";
            
            // Simular download lento (bloqueando!)
            var response = _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/posts").Result;
            
            // Adicionar delay para evidenciar o problema
            Thread.Sleep(2000);
            
            return response;
        }

        private string ProcessDataSync(string data)
        {
            StatusLabel.Content = "Processando dados...";
            
            // Simular processamento pesado
            var sb = new StringBuilder();
            sb.AppendLine("=== DADOS PROCESSADOS ===\n");
            
            // Processamento CPU-bound (bloqueando!)
            for (int i = 0; i < 100; i++)
            {
                // Simular trabalho pesado
                for (int j = 0; j < 1000000; j++)
                {
                    _ = Math.Sqrt(j);
                }
                
                sb.AppendLine($"Registro {i + 1} processado");
                
                // Tentar atualizar UI (ainda na mesma thread!)
                ProgressBar.Value = (i + 1);
            }
            
            return sb.ToString();
        }

        private void SaveDataSync(string data)
        {
            StatusLabel.Content = "Salvando em arquivo...";
            
            // I/O síncrono (bloqueando!)
            var filePath = Path.Combine(Path.GetTempPath(), "resultado.txt");
            File.WriteAllText(filePath, data);
            
            Thread.Sleep(1000); // Simular I/O lento
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Clear();
            StatusLabel.Content = "Pronto";
        }
    }
}
