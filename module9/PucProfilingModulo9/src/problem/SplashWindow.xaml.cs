using System;
using System.Collections.Generic;
using System.Windows;

namespace StartupPerformance.Problem;

public partial class SplashWindow : Window
{
    public List<Cliente> Clientes { get; private set; } = new();

    public SplashWindow()
    {
        InitializeComponent();
        ContentRendered += SplashWindow_ContentRendered;
    }

    private void SplashWindow_ContentRendered(object? sender, EventArgs e)
    {
        // PROBLEMA: Carregamento síncrono no UI thread
        // Bloqueia completamente a interface durante o carregamento
        
        // Usa Dispatcher para garantir que a splash seja renderizada antes de bloquear
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                StatusText.Text = "Carregando base de clientes...";
                
                // Force render da mensagem inicial
                Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                
                // Escolher qual CSV usar (troque aqui para testar dev vs prod)
                var csvFileName = "clientes-prod.csv"; // Mude para "clientes-dev.csv" para testar rápido
                
                var dataService = new DataService(csvFileName);
                Clientes = dataService.CarregarClientesSincrono();

                StatusText.Text = $"✓ {Clientes.Count} clientes carregados";
                
                // Pequeno delay para mostrar a mensagem
                System.Threading.Thread.Sleep(500);

                // Abre janela principal
                var mainWindow = new MainWindow(Clientes);
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }
}
