using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace UIVirtualization.Problem;

public partial class MainWindow : Window
{
    private List<Cliente> _clientes = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        ClientesComboBox.SelectionChanged += ClientesComboBox_SelectionChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // PROBLEMA: Carrega todos os clientes e popula ComboBox
        // Sem virtualização, cada item cria um visual element
        var sw = Stopwatch.StartNew();
        
        var dataService = new DataService();
        _clientes = dataService.CarregarClientes();
        
        // PROBLEMA: ItemsSource sem virtualização = todos os elementos criados
        ClientesComboBox.ItemsSource = _clientes;
        
        sw.Stop();
        
        Title = $"UI Virtualization - Problem (Carregado em {sw.ElapsedMilliseconds}ms - {_clientes.Count} clientes)";
    }

    private void ClientesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ClientesComboBox.SelectedItem is Cliente cliente)
        {
            SelectedClienteText.Text = $"ID: {cliente.Id}\n" +
                                       $"Nome: {cliente.Nome}\n" +
                                       $"Email: {cliente.Email}\n" +
                                       $"Telefone: {cliente.Telefone}\n" +
                                       $"Cidade: {cliente.Cidade}/{cliente.Estado}";
        }
        else
        {
            SelectedClienteText.Text = "Nenhum cliente selecionado";
        }
    }
}
