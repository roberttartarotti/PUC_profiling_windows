using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StartupPerformance.Solved1;

public partial class MainWindow : Window
{
    private List<Cliente>? _clientes;
    private CancellationTokenSource? _loadingCts;

    public MainWindow()
    {
        InitializeComponent();
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
        _loadingCts?.Cancel();
        Application.Current.Shutdown();
    }

    // SOLUÇÃO 1: Lazy Loading - carrega apenas quando necessário
    private async Task ShowClientesScreenAsync()
    {
        // Se já carregou, mostra imediatamente
        if (_clientes != null)
        {
            ShowClientesData(_clientes);
            return;
        }

        // Cancela qualquer carregamento anterior
        _loadingCts?.Cancel();
        _loadingCts = new CancellationTokenSource();

        // Mostra tela de loading
        ShowLoadingScreen();

        try
        {
            StatusText.Text = "Carregando clientes...";

            // Escolher qual CSV usar
            var csvFileName = "clientes-prod.csv"; // Mude para "clientes-dev.csv" para testar rápido

            var dataService = new DataService(csvFileName);
            
            // Carrega com progresso
            var progress = new Progress<(int current, int total, string message)>(report =>
            {
                UpdateLoadingProgress(report.current, report.total, report.message);
            });

            _clientes = await dataService.CarregarClientesAsync(progress, _loadingCts.Token);

            // Mostra os dados
            ShowClientesData(_clientes);
            ClientesCountText.Text = $"Clientes: {_clientes.Count} carregados";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Carregamento cancelado";
            ShowSimpleScreen("Carregamento Cancelado", "O carregamento foi interrompido.");
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

        var cancelButton = new Button
        {
            Content = "Cancelar",
            Width = 100,
            Height = 30,
            Margin = new Thickness(0, 20, 0, 0)
        };
        cancelButton.Click += (s, e) => _loadingCts?.Cancel();
        panel.Children.Add(cancelButton);

        ContentArea.Children.Add(panel);
    }

    private void UpdateLoadingProgress(int current, int total, string message)
    {
        // Encontra os controles na tela de loading
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

        StatusText.Text = message;
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
            Text = "✨ Dados carregados sob demanda (Lazy Loading)",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
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
        StatusText.Text = $"✓ {clientes.Count} clientes carregados";
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
