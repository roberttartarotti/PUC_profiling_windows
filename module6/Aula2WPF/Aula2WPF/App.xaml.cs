using System;
using System.Diagnostics;
using System.Windows;

namespace WpfXamlPerformanceDemo
{
    public partial class App : Application
    {
        private PerformanceMonitor perfMonitor;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Adicionar handlers para exceções não tratadas
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            try
            {
                // Iniciar monitoramento
                perfMonitor = new PerformanceMonitor();
                perfMonitor.Start();

                // Configurar tracing para diagnóstico
                PresentationTraceSources.Refresh();
                PresentationTraceSources.DataBindingSource.Listeners.Add(
                    new DebugTraceListener());
                PresentationTraceSources.DataBindingSource.Switch.Level =
                    System.Diagnostics.SourceLevels.Warning;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro na inicialização: {ex.Message}");
                MessageBox.Show($"Erro na inicialização da aplicação:\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            perfMonitor?.Stop();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Debug.WriteLine($"Exceção não tratada (AppDomain): {ex?.Message}\n{ex?.StackTrace}");
            MessageBox.Show($"Erro não tratado:\n{ex?.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Exceção não tratada (Dispatcher): {e.Exception.Message}\n{e.Exception.StackTrace}");
            MessageBox.Show($"Erro não tratado:\n{e.Exception.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }

    public class DebugTraceListener : System.Diagnostics.TraceListener
    {
        public override void Write(string? message) { }

        public override void WriteLine(string? message)
        {
            // Log para diagnóstico de bindings
            Debug.WriteLine($"[Binding] {message}");
        }
    }
}