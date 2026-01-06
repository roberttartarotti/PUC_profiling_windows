using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StartupPerformance.Solved2;

public partial class MainWindow : Window
{
    private readonly PreloadService _preloadService;
    private List<Cliente>? _displayedClientes;

    public MainWindow()
    {
        InitializeComponent();
        
        // SOLUÇÃO 2: Inicia preload em background
        _preloadService = new PreloadService("clientes-prod.csv"); // Mude para "clientes-dev.csv" para testar
        
        _preloadService.ProgressChanged += OnPreloadProgress;
        _preloadService.PreloadCompleted += OnPreloadCompleted;
        
        // Inicia o preload, mas não bloqueia a UI
        _preloadService.StartPreload();
    }

    private void OnPreloadProgress(object? sender, PreloadProgressEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var percentage = e.Total > 0 ? (e.Current * 100) / e.Total : 0;
            PreloadStatusText.Text = $"⏳ Preload: {percentage}% ({e.Current}/{e.Total})";
            StatusText.Text = e.Message;
        });
    }

    private void OnPreloadCompleted(object? sender, bool success)
    {
        Dispatcher.Invoke(async () =>
        {
            if (success)
            {
                var clientes = await _preloadService.GetClientesAsync();
                PreloadStatusText.Text = $"✓ Preload concluído";
                ClientesCountText.Text = $"Clientes: {clientes.Count} prontos";
                PreloadInfoText.Text = $"✓ {clientes.Count} clientes prontos para uso";
                StatusText.Text = "Sistema pronto - Dados precarregados";
            }
            else
            {
                PreloadStatusText.Text = "✗ Preload cancelado";
                PreloadInfoText.Text = "Preload foi cancelado";
            }
        });
    }

    private async void MenuClientes_Click(object sender, RoutedEventArgs e)
    {
        await ShowClientesScreenAsync();
    }

    private void MenuFornecedores_Click(object sender, RoutedEventArgs e)
    {
        ShowSimpleScreen("Fornecedores", "Lista de fornecedores aqui...");
    }

    private void MenuProdutos_Click(object sender, RoutedEventArgs e)
    {
        ShowSimpleScreen("Produtos", "Catálogo de produtos aqui...");
    }

    private void MenuNovaVenda_Click(object sender, RoutedEventArgs e)
    {
        ShowSimpleScreen("Nova Venda", "Formulário de venda aqui...");
    }

    private void MenuPedidos_Click(object sender, RoutedEventArgs e)
    {
        ShowSimpleScreen("Pedidos", "Lista de pedidos aqui...");
    }

    private void MenuRelatorios_Click(object sender, RoutedEventArgs e)
    {
        ShowSimpleScreen("Relatórios", "Relatórios e dashboards aqui...");
    }

    private void MenuConfiguracoes_Click(object sender, RoutedEventArgs e)
    {
        ShowSimpleScreen("Configurações", "Configurações do sistema aqui...");
    }

    private void MenuSair_Click(object sender, RoutedEventArgs e)
    {
        _preloadService.CancelPreload();
        Application.Current.Shutdown();
    }

    // SOLUÇÃO 2: Obtém dados do preload (instantâneo se já carregou, ou aguarda se ainda está carregando)
    private async Task ShowClientesScreenAsync()
    {
        // Se já mostramos anteriormente, reutiliza
        if (_displayedClientes != null)
        {
            ShowClientesData(_displayedClientes);
            return;
        }

        try
        {
            List<Cliente> clientes;

            // Se preload já completou, é instantâneo
            if (_preloadService.IsCompleted)
            {
                StatusText.Text = "Carregando clientes do cache...";
                clientes = await _preloadService.GetClientesAsync();
                StatusText.Text = "✓ Dados carregados do cache (instantâneo)";
            }
            // Se está preloading, mostra progresso e aguarda
            else if (_preloadService.IsPreloading)
            {
                ShowInterceptedLoadingScreen();
                StatusText.Text = "Aguardando preload em andamento...";
                
                var progress = new Progress<(int current, int total, string message)>(report =>
                {
                    UpdateLoadingProgress(report.current, report.total, 
                        $"Interceptando preload... {report.message}");
                });
                
                clientes = await _preloadService.GetClientesAsync(progress);
                StatusText.Text = "✓ Preload interceptado com sucesso";
            }
            // Não deveria acontecer, mas carrega se necessário
            else
            {
                ShowLoadingScreen();
                StatusText.Text = "Carregando clientes...";
                
                var progress = new Progress<(int current, int total, string message)>(report =>
                {
                    UpdateLoadingProgress(report.current, report.total, report.message);
                });
                
                clientes = await _preloadService.GetClientesAsync(progress);
            }

            _displayedClientes = clientes;
            ShowClientesData(clientes);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar clientes: {ex.Message}", "Erro",
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Erro ao carregar clientes";
        }
    }

    private void ShowLoadingScreen()
    {
        ContentArea.Children.Clear();

        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var titleBlock = new TextBlock
        {
            Text = "⏳ Carregando Clientes...",
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x2C, 0x3E, 0x50)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(titleBlock);

        var progressBar = new ProgressBar
        {
            Name = "LoadingProgressBar",
            Width = 400,
            Height = 20,
            Margin = new Thickness(0, 30, 0, 0),
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };
        panel.Children.Add(progressBar);

        var statusBlock = new TextBlock
        {
            Name = "LoadingStatusText",
            Text = "Preparando...",
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 15, 0, 0)
        };
        panel.Children.Add(statusBlock);

        ContentArea.Children.Add(panel);
    }

    private void ShowInterceptedLoadingScreen()
    {
        ContentArea.Children.Clear();

        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var titleBlock = new TextBlock
        {
            Text = "🔄 Interceptando Preload...",
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(titleBlock);

        var infoBlock = new TextBlock
        {
            Text = "Dados já estão sendo carregados em background",
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        panel.Children.Add(infoBlock);

        var progressBar = new ProgressBar
        {
            Name = "LoadingProgressBar",
            Width = 400,
            Height = 20,
            Margin = new Thickness(0, 30, 0, 0),
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };
        panel.Children.Add(progressBar);

        var statusBlock = new TextBlock
        {
            Name = "LoadingStatusText",
            Text = "Aguardando...",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 15, 0, 0)
        };
        panel.Children.Add(statusBlock);

        ContentArea.Children.Add(panel);
    }

    private void UpdateLoadingProgress(int current, int total, string message)
    {
        if (ContentArea.Children.Count > 0 && ContentArea.Children[0] is StackPanel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is ProgressBar progressBar)
                {
                    progressBar.Maximum = total;
                    progressBar.Value = current;
                }
                else if (child is TextBlock textBlock && textBlock.Name == "LoadingStatusText")
                {
                    textBlock.Text = message;
                }
            }
        }
    }

    private void ShowClientesData(List<Cliente> clientes)
    {
        ContentArea.Children.Clear();

        var panel = new StackPanel { Margin = new Thickness(20) };

        var title = new TextBlock
        {
            Text = "👥 Clientes",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkSlateGray,
            Margin = new Thickness(0, 0, 0, 20)
        };
        panel.Children.Add(title);

        var countText = new TextBlock
        {
            Text = $"Total de clientes: {clientes.Count}",
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(countText);

        var infoText = new TextBlock
        {
            Text = _preloadService.IsCompleted 
                ? "🚀 Dados carregados via Background Preloading (instantâneo)" 
                : "🔄 Dados interceptados do preload em andamento",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
            FontStyle = FontStyles.Italic,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(infoText);

        var dataGrid = new DataGrid
        {
            ItemsSource = clientes,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            Height = 380,
            Margin = new Thickness(0, 10, 0, 0)
        };
        panel.Children.Add(dataGrid);

        ContentArea.Children.Add(panel);
        StatusText.Text = $"✓ {clientes.Count} clientes disponíveis";
    }

    private void ShowSimpleScreen(string title, string content)
    {
        ContentArea.Children.Clear();

        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 32,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkSlateGray,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(titleBlock);

        var contentBlock = new TextBlock
        {
            Text = content,
            FontSize = 16,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };
        panel.Children.Add(contentBlock);

        ContentArea.Children.Add(panel);
        StatusText.Text = $"Tela: {title}";
    }
}
