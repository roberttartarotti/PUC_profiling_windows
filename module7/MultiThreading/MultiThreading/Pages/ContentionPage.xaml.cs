using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MultiThreading
{
    public sealed partial class ContentionPage : Page
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly object[] _contentionHotspots;
        private int _activeThreads;
        private long _totalContentionEvents;
        private long _threadsWaiting;
        private readonly Random _random = new ();
        private readonly DispatcherTimer _timer;

        public ContentionPage()
        {
            // Initialize contention hotspots
            _contentionHotspots = new object[5];
            for (int i = 0; i < _contentionHotspots.Length; i++)
            {
                _contentionHotspots[i] = new object();
            }

            this.InitializeComponent();
            
            // Status update timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
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
            StatusText.Text = $"Threads Ativas: {_activeThreads}, Eventos de Contenção: {_totalContentionEvents:N0}, Threads Esperando: {_threadsWaiting}";
        }

        private async void LowContention_Click(object sender, RoutedEventArgs e)
        {
            await StartContentionDemo(100, 5); // 100 threads, 5 locks
        }

        private async void HighContention_Click(object sender, RoutedEventArgs e)
        {
            await StartContentionDemo(200, 2); // 200 threads, 2 locks
        }

        private async void ExtremeContention_Click(object sender, RoutedEventArgs e)
        {
            await StartContentionDemo(500, 1); // 500 threads, 1 lock
        }

        private void StopDemo_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task StartContentionDemo(int threadCount, int lockCount)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _activeThreads = 0;
            _totalContentionEvents = 0;
            _threadsWaiting = 0;

            Debug.WriteLine($"=== INICIANDO TESTE DE CONTENÇÃO ===");
            Debug.WriteLine($"Threads: {threadCount}, Locks: {lockCount}");

            try
            {
                await RunLockContentionDemo(threadCount, lockCount, token);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Teste de contenção cancelado");
            }
        }

        private async Task RunLockContentionDemo(int threadCount, int lockCount, CancellationToken token)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < threadCount; i++)
            {
                var threadId = i;
                var task = Task.Run(async () =>
                {
                    Interlocked.Increment(ref _activeThreads);
                    
                    try
                    {
                        for (int iter = 0; iter < 1000; iter++)
                        {
                            token.ThrowIfCancellationRequested();

                            var lockIndex = lockCount == 1 ? 0 : 
                                           lockCount == 2 ? (threadId % 10 < 8 ? 0 : 1) : 
                                           threadId % lockCount;

                            var waitStart = DateTime.UtcNow;
                            Interlocked.Increment(ref _threadsWaiting);

                            lock (_contentionHotspots[lockIndex])
                            {
                                var waitTime = (DateTime.UtcNow - waitStart).TotalMilliseconds;
                                if (waitTime > 1)
                                {
                                    Interlocked.Increment(ref _totalContentionEvents);
                                }
                                
                                Interlocked.Decrement(ref _threadsWaiting);

                                Thread.Sleep(_random.Next(5, 25));

                                for (int j = 0; j < 1000; j++)
                                {
                                    Math.Sin(j * threadId);
                                }
                            }

                            await Task.Delay(_random.Next(1, 10), token);
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeThreads);
                    }
                }, token);

                tasks.Add(task);
                await Task.Delay(5, token);
            }

            await Task.WhenAll(tasks);
            Debug.WriteLine($"Teste de contenção completado. Total de eventos: {_totalContentionEvents}");
        }
    }
}