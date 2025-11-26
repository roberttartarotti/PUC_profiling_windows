using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MultiThreading
{
    public sealed partial class DeadlockPage : Page
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly object _resourceA = new ();
        private readonly object _resourceB = new ();
        private int _threadsInDeadlock = 0;
        private readonly DispatcherTimer _timer;

        public DeadlockPage()
        {
            this.InitializeComponent();

            // Status update timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += UpdateStatus;
            _timer.Start();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _timer?.Stop();
            Frame.GoBack();
        }

        private void UpdateStatus(object? sender, object? e)
        {
            if (_threadsInDeadlock > 0)
            {
                StatusText.Text = $"🚨 DEADLOCK DETECTADO! {_threadsInDeadlock} threads em ciclo de dependência";
            }
            else
            {
                StatusText.Text = "Status: Nenhum deadlock detectado - Sistema funcionando normalmente";
            }
        }

        private async void TriggerSimpleDeadlock_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            Debug.WriteLine("=== INICIANDO DEMO DE DEADLOCK SIMPLES ===");

            var task1 = Task.Run(() =>
            {
                var threadId = Environment.CurrentManagedThreadId;
                Debug.WriteLine($"Thread {threadId}: Iniciando - vai adquirir A depois B");

                lock (_resourceA)
                {
                    Debug.WriteLine($"Thread {threadId}: ADQUIRIU recurso A");
                    Thread.Sleep(100);

                    Debug.WriteLine($"Thread {threadId}: Tentando adquirir recurso B...");
                    Interlocked.Increment(ref _threadsInDeadlock);

                    lock (_resourceB)
                    {
                        Debug.WriteLine($"Thread {threadId}: ADQUIRIU ambos os recursos!");
                        Thread.Sleep(1000);
                        Interlocked.Decrement(ref _threadsInDeadlock);
                    }
                }
            });

            var task2 = Task.Run(() =>
            {
                var threadId = Environment.CurrentManagedThreadId;
                Debug.WriteLine($"Thread {threadId}: Iniciando - vai adquirir B depois A");

                lock (_resourceB)
                {
                    Debug.WriteLine($"Thread {threadId}: ADQUIRIU recurso B");
                    Thread.Sleep(100);

                    Debug.WriteLine($"Thread {threadId}: Tentando adquirir recurso A...");
                    Interlocked.Increment(ref _threadsInDeadlock);

                    lock (_resourceA)
                    {
                        Debug.WriteLine($"Thread {threadId}: ADQUIRIU ambos os recursos!");
                        Thread.Sleep(1000);
                        Interlocked.Decrement(ref _threadsInDeadlock);
                    }
                }
            });

            try
            {
                await Task.WhenAll(task1, task2);
                Debug.WriteLine("Demo de deadlock completada sem deadlock");
                _threadsInDeadlock = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro na demo de deadlock: {ex.Message}");
            }
        }

        private async void TriggerPreventionDemo_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            Debug.WriteLine("=== DEMO DE PREVENÇÃO DE DEADLOCK ===");

            var task1 = Task.Run(() =>
            {
                var threadId = Environment.CurrentManagedThreadId;
                Debug.WriteLine($"Thread {threadId}: Usando ordem consistente A → B");

                lock (_resourceA)
                {
                    Debug.WriteLine($"Thread {threadId}: ADQUIRIU recurso A");
                    Thread.Sleep(100);
                    Interlocked.Increment(ref _threadsInDeadlock);

                    lock (_resourceB)
                    {
                        Debug.WriteLine($"Thread {threadId}: ADQUIRIU recurso B - SUCESSO!");
                        Thread.Sleep(1000);
                        Interlocked.Decrement(ref _threadsInDeadlock);
                    }
                }
            });

            var task2 = Task.Run(() =>
            {
                var threadId = Environment.CurrentManagedThreadId;
                Debug.WriteLine($"Thread {threadId}: Usando MESMA ordem A → B (prevenção)");

                lock (_resourceA)
                {
                    Debug.WriteLine($"Thread {threadId}: ADQUIRIU recurso A");
                    Thread.Sleep(100);
                    Interlocked.Increment(ref _threadsInDeadlock);

                    lock (_resourceB)
                    {
                        Debug.WriteLine($"Thread {threadId}: ADQUIRIU recurso B - SUCESSO!");
                        Thread.Sleep(1000);
                        Interlocked.Decrement(ref _threadsInDeadlock);
                    }
                }
            });

            try
            {
                await Task.WhenAll(task1, task2);
                Debug.WriteLine("Demo de prevenção completada - Nenhum deadlock!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro na demo de prevenção: {ex.Message}");
            }
        }

        private void EmergencyStop_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _threadsInDeadlock = 0;
            Debug.WriteLine("PARADA DE EMERGÊNCIA - Todos os cenários de deadlock cancelados");
        }
    }
}