using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MultiThreading
{
    public sealed partial class ConcurrencyPage : Page
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private int _activeThreads;
        private long _totalOperations;
        private readonly object _lockObject = new();
        private readonly DispatcherTimer _timer;

        public ConcurrencyPage()
        {
            this.InitializeComponent();
            
            // Start UI update timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += UpdateUI;
            _timer.Start();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _timer?.Stop();
            Frame.GoBack();
        }

        private void UpdateUI(object? sender, object? e)
        {
            StatusText.Text = $"Threads ativas: {_activeThreads}, Operações totais: {_totalOperations:N0}";
        }

        private async void StartConcurrentDemo_Click(object sender, RoutedEventArgs e)
        {
            await StartDemo(true);
        }

        private async void StartParallelDemo_Click(object sender, RoutedEventArgs e)
        {
            await StartDemo(false);
        }

        private void StopDemo_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private async Task StartDemo(bool isConcurrent)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _activeThreads = 0;
            _totalOperations = 0;

            var threadCount = 50;
            var iterations = 1000;

            try
            {
                if (isConcurrent)
                {
                    await RunConcurrentDemo(threadCount, iterations, token);
                }
                else
                {
                    await RunParallelDemo(threadCount, iterations, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Demo cancelled
            }
        }

        private async Task RunConcurrentDemo(int threadCount, int iterations, CancellationToken token)
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
                        for (int iter = 0; iter < iterations; iter++)
                        {
                            token.ThrowIfCancellationRequested();

                            await Task.Delay(10, token);

                            for (int j = 0; j < 1000; j++)
                            {
                                Math.Sin(j * threadId);
                            }

                            lock (_lockObject)
                            {
                                Thread.Sleep(1);
                            }

                            await Task.Yield();
                            Interlocked.Increment(ref _totalOperations);
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
        }

        private async Task RunParallelDemo(int threadCount, int iterations, CancellationToken token)
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Task.Run(() =>
            {
                Parallel.For(0, threadCount, parallelOptions, threadId =>
                {
                    Interlocked.Increment(ref _activeThreads);
                    try
                    {
                        for (int iter = 0; iter < iterations; iter++)
                        {
                            token.ThrowIfCancellationRequested();

                            var result = 0.0;
                            for (int j = 0; j < 10000; j++)
                            {
                                result += Math.Sin(j * threadId) * Math.Cos(j);
                            }

                            lock (_lockObject)
                            {
                                // Quick operation
                            }

                            Interlocked.Increment(ref _totalOperations);

                            if (iter % 100 == 0)
                                Thread.Yield();
                        }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeThreads);
                    }
                });
            }, token);
        }
    }
}