using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WpfXamlPerformanceDemo
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<DataItem> items;
        private DispatcherTimer statsTimer;
        private PerformanceCounter cpuCounter;
        private Process currentProcess;
        private Random random = new();
        private int bindingUpdateCount = 0;
        private int layoutPassCount = 0;
        private DispatcherTimer stressTimer;

        // Properties para bindings
        public ObservableCollection<DataItem> Items => items;
        public string FilterText { get; set; }
        public bool ShowInactive { get; set; }
        public bool GroupItems { get; set; }
        public int ItemCount => items?.Count ?? 0;
        public bool IsLoading { get; set; }
        public string SourceText { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Configurar DataContext
            this.DataContext = this;

            // Inicializar coleção
            items = new ObservableCollection<DataItem>();

            // Configurar PerformanceCounter
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch { }

            currentProcess = Process.GetCurrentProcess();

            // Configurar timers
            SetupTimers();

            // Carregar dados iniciais
            LoadInitialData();

            // Monitorar eventos de layout
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            // Monitorar binding updates
            PresentationTraceSources.DataBindingSource.Listeners.Add(
                new BindingTraceListener(this));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Iniciar animações problemáticas
            StartProblematicAnimations();
        }

        private void SetupTimers()
        {
            // Timer para estatísticas
            statsTimer = new DispatcherTimer();
            statsTimer.Interval = TimeSpan.FromSeconds(1);
            statsTimer.Tick += StatsTimer_Tick;
            statsTimer.Start();

            // Timer para stress test
            stressTimer = new DispatcherTimer();
            stressTimer.Interval = TimeSpan.FromMilliseconds(100);
            stressTimer.Tick += StressTimer_Tick;
        }

        private void LoadInitialData()
        {
            IsLoading = true;

            // Carregar dados pesados
            Task.Run(() =>
            {
                var data = GenerateDataItems(1000);

                Dispatcher.Invoke(() =>
                {
                    foreach (var item in data)
                    {
                        items.Add(item);
                    }
                    IsLoading = false;
                });
            });
        }

        private List<DataItem> GenerateDataItems(int count)
        {
            var result = new List<DataItem>();
            var names = new[] { "John", "Jane", "Bob", "Alice", "Charlie", "Diana", "Eve", "Frank" };
            var statuses = new[] { "Active", "Inactive", "Pending", "Completed" };

            for (int i = 0; i < count; i++)
            {
                result.Add(new DataItem
                {
                    Id = i,
                    Name = $"{names[i % names.Length]} {i}",
                    Description = $"This is a long description for item {i} with some additional text to make it heavier for rendering.",
                    Status = statuses[i % statuses.Length],
                    Initials = names[i % names.Length].Substring(0, 1),
                    Details = $"Detailed information for item {i}. " +
                              new string('X', random.Next(50, 200))
                });
            }

            return result;
        }

        private void StartProblematicAnimations()
        {
            // Adicionar mais animações para causar redraws
            var blinkAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };

            var footerStatusBar = this.FindName("FooterStatusBar") as StatusBar;
            if (footerStatusBar != null)
            {
                Storyboard.SetTarget(blinkAnimation, footerStatusBar);
                Storyboard.SetTargetProperty(blinkAnimation, new PropertyPath(OpacityProperty));

                var storyboard = new System.Windows.Media.Animation.Storyboard();
                storyboard.Children.Add(blinkAnimation);
                storyboard.Begin();
            }
        }

        // ===== EVENT HANDLERS =====

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            // Adicionar item com animação complexa
            var newItem = new DataItem
            {
                Id = items.Count,
                Name = $"New Item {items.Count}",
                Description = $"Newly added item with auto-generated content",
                Status = "Active",
                Initials = "N",
                Details = new string('*', 100)
            };

            items.Add(newItem);

            // Forçar scroll para o novo item (causa layout)
            ItemsListView.ScrollIntoView(newItem);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            // Limpar todos os itens
            items.Clear();

            // Adicionar delay artificial
            Task.Delay(500).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => LoadInitialData());
            });
        }

        private void StressTest_Click(object sender, RoutedEventArgs e)
        {
            if (stressTimer.IsEnabled)
            {
                stressTimer.Stop();
                ((Button)sender).Content = "Stress Test";
            }
            else
            {
                stressTimer.Start();
                ((Button)sender).Content = "Stop Stress";
            }
        }

        private void StartProfiling_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Para profiling:\n\n" +
                "1. PIX: Capture frames durante interação\n" +
                "2. VS Profiler: Analise CPU/GPU usage\n" +
                "3. PerfView: Capture eventos WPF\n" +
                "4. WPR: Use o preset 'GeneralProfile'\n\n" +
                "Problemas a observar:\n" +
                "- ListView sem virtualization\n" +
                "- TwoWay bindings cascateando\n" +
                "- Animações constantes\n" +
                "- Controles aninhados profundamente",
                "Profiling Instructions",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void StatsTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Atualizar FPS usando múltiplos métodos para garantir que funcione
                var perfMonitor = PerformanceMonitor.Instance;
                
                // Tentar o método mais simples primeiro
                var simpleFps = perfMonitor.GetSimpleFPS();
                var avgFps = perfMonitor.GetCurrentFPS();
                var calculatedFps = PerformanceMonitor.CalculateFPS();
                
                // Usar o melhor FPS disponível
                var displayFps = simpleFps > 0 ? simpleFps : (avgFps > 0 ? avgFps : calculatedFps);
                
                if (FpsCounter != null)
                {
                    FpsCounter.Text = displayFps.ToString("F1");
                }

                // Debug detalhado a cada 3 segundos
                if (DateTime.Now.Second % 3 == 0)
                {
                    Debug.WriteLine($"=== FPS Debug ===");
                    Debug.WriteLine($"FPS Simple: {simpleFps}");
                    Debug.WriteLine($"FPS Average: {avgFps:F1}");
                    Debug.WriteLine($"FPS Calculated: {calculatedFps:F1}");
                    Debug.WriteLine($"FPS Displayed: {displayFps:F1}");
                    Debug.WriteLine($"=================");
                }

                // Atualizar uso de memória
                currentProcess.Refresh();
                var memoryMB = currentProcess.WorkingSet64 / (1024 * 1024);
                if (MemoryUsage != null)
                {
                    MemoryUsage.Text = memoryMB.ToString();
                }

                // Atualizar contadores
                if (BindingUpdateCount != null)
                {
                    BindingUpdateCount.Text = bindingUpdateCount.ToString();
                }
                if (LayoutPassCount != null)
                {
                    LayoutPassCount.Text = layoutPassCount.ToString();
                }

                // Reset contadores
                bindingUpdateCount = 0;
                layoutPassCount = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro em StatsTimer_Tick: {ex.Message}");
            }
        }

        private void StressTimer_Tick(object sender, EventArgs e)
        {
            // Simular atividade pesada
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Adicionar/remover itens
                if (random.Next(0, 2) == 0 && items.Count > 0)
                {
                    items.RemoveAt(random.Next(0, items.Count));
                }
                else
                {
                    var newItem = new DataItem
                    {
                        Id = items.Count,
                        Name = $"Stress {items.Count}",
                        Description = "Stress test item",
                        Status = "Active",
                        Initials = "S"
                    };
                    items.Add(newItem);
                }

                // Forçar redraw
                InvalidateVisual();
            }), DispatcherPriority.Background);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Contar passes de renderização
            layoutPassCount++;
        }

        public void IncrementBindingUpdate()
        {
            bindingUpdateCount++;
        }

        // Classe para item de dados
        public class DataItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public string Initials { get; set; }
            public string Details { get; set; }
        }

        // Listener para binding updates
        private class BindingTraceListener : TraceListener
        {
            private MainWindow window;

            public BindingTraceListener(MainWindow window)
            {
                this.window = window;
            }

            public override void Write(string? message) { }

            public override void WriteLine(string? message)
            {
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    window.IncrementBindingUpdate();
                }));
            }
        }
    }
}