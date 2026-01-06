using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StartupPerformance.Problem;

public partial class MainWindow : Window
{
    private readonly List<Cliente> _clientes;

    public MainWindow(List<Cliente> clientes)
    {
        InitializeComponent();
        _clientes = clientes;
        ClientesCountText.Text = $"{_clientes.Count} clientes carregados";
    }

    private void MenuClientes_Click(object sender, RoutedEventArgs e)
    {
        ShowClientesScreen();
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
        Application.Current.Shutdown();
    }

    private void ShowClientesScreen()
    {
        ContentArea.Children.Clear();
        
        var panel = new StackPanel { Margin = new Thickness(20) };
        
        var title = new TextBlock
        {
            Text = "👥 Clientes",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            Margin = new Thickness(0, 0, 0, 20)
        };
        panel.Children.Add(title);

        var countText = new TextBlock
        {
            Text = $"Total de clientes: {_clientes.Count}",
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(countText);

        // DataGrid simples
        var dataGrid = new DataGrid
        {
            ItemsSource = _clientes,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            Height = 400,
            Margin = new Thickness(0, 10, 0, 0)
        };
        panel.Children.Add(dataGrid);

        ContentArea.Children.Add(panel);
        StatusText.Text = $"Visualizando {_clientes.Count} clientes";
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
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(titleBlock);

        var contentBlock = new TextBlock
        {
            Text = content,
            FontSize = 16,
            Foreground = System.Windows.Media.Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };
        panel.Children.Add(contentBlock);

        ContentArea.Children.Add(panel);
        StatusText.Text = $"Tela: {title}";
    }
}
