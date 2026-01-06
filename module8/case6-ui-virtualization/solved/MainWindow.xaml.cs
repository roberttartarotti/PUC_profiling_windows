using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace UIVirtualization.Solved;

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
        // SOLUÇÃO: Com virtualização, binding é instantâneo
        // Apenas elementos visíveis são criados
        var sw = Stopwatch.StartNew();
        
        var dataService = new DataService();
        _clientes = dataService.CarregarClientes();
        
        // SOLUÇÃO: ItemsSource COM virtualização = apenas elementos visíveis criados
        ClientesComboBox.ItemsSource = _clientes;
        
        sw.Stop();
        
        Title = $"UI Virtualization - Solved (Carregado em {sw.ElapsedMilliseconds}ms - {_clientes.Count} clientes)";
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
