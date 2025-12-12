using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfXamlPerformanceDemo
{
    public class PerformanceMonitor
    {
        private static PerformanceMonitor instance;
        private DispatcherTimer timer;
        private List<double> frameTimes = new List<double>();
        private Stopwatch frameTimer = Stopwatch.StartNew();
        private long lastFrameTime;
        private int frameCount = 0;
        private DateTime lastFpsCalculation;
        
        // Contador simples para FPS
        private int framesInLastSecond = 0;
        private int lastReportedFps = 0; // Guardar o último FPS válido
        private DateTime lastSecond = DateTime.Now;

        public static PerformanceMonitor Instance => instance ??= new PerformanceMonitor();

        public event Action<double> FPSUpdated;

        public void Start()
        {
            // Reset state for a fresh measurement window
            frameTimes.Clear();
            frameCount = 0;
            framesInLastSecond = 0;
            lastReportedFps = 0;
            lastFrameTime = frameTimer.ElapsedMilliseconds;
            lastFpsCalculation = DateTime.Now;
            lastSecond = DateTime.Now;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Monitorar frames
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            
            Debug.WriteLine("PerformanceMonitor iniciado");
        }

        public void Stop()
        {
            timer?.Stop();
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            Debug.WriteLine("PerformanceMonitor parado");
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var currentTime = frameTimer.ElapsedMilliseconds;
            var frameTime = currentTime - lastFrameTime;

            // Evitar contar o mesmo frame múltiplas vezes
            // CompositionTarget.Rendering pode ser chamado várias vezes por frame
            if (frameTime >= 1) // Mínimo de 1ms entre frames
            {
                frameTimes.Add(frameTime);
                frameCount++;
                framesInLastSecond++;

                // Manter apenas últimos 60 frames para cálculo suave
                if (frameTimes.Count > 60)
                    frameTimes.RemoveAt(0);

                lastFrameTime = currentTime;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Capturar o FPS antes de resetar
            lastReportedFps = framesInLastSecond;
            
            Debug.WriteLine($"FPS (contagem simples): {lastReportedFps} frames no último segundo");
            
            // Método alternativo baseado em tempo médio
            var avgFps = CalculateFPS();
            Debug.WriteLine($"FPS (tempo médio): {avgFps:F1}");
            
            // Resetar contador DEPOIS de capturar o valor
            framesInLastSecond = 0;
            
            // Notificar com o valor capturado
            FPSUpdated?.Invoke(lastReportedFps);
        }

        public static double CalculateFPS()
        {
            // Garantir que a instância existe
            if (instance == null)
            {
                instance = new PerformanceMonitor();
            }

            if (instance.frameTimes.Count == 0)
            {
                return 0;
            }

            // Método baseado na média de tempos de frame
            double totalTime = 0;
            foreach (var time in instance.frameTimes)
                totalTime += time;

            double avgFrameTime = totalTime / instance.frameTimes.Count;
            
            if (avgFrameTime <= 0)
            {
                return 0;
            }

            double fps = 1000.0 / avgFrameTime;
            
            // Limitar FPS a valores razoáveis (evitar valores absurdos)
            if (fps > 1000) fps = 1000;
            if (fps < 0) fps = 0;
            
            return fps;
        }

        // Método alternativo mais simples para FPS
        public double GetCurrentFPS()
        {
            if (frameTimes.Count < 2)
                return lastReportedFps; // Retornar último FPS válido se não há dados suficientes

            // Pegar os últimos frames para cálculo mais responsivo
            var recentFrames = Math.Min(10, frameTimes.Count);
            double recentTotal = 0;
            
            for (int i = frameTimes.Count - recentFrames; i < frameTimes.Count; i++)
            {
                recentTotal += frameTimes[i];
            }
            
            double avgRecentFrameTime = recentTotal / recentFrames;
            return avgRecentFrameTime > 0 ? 1000.0 / avgRecentFrameTime : lastReportedFps;
        }

        // Método mais simples e direto
        public int GetSimpleFPS()
        {
            // Retornar o último FPS reportado, não o contador atual
            // O contador atual pode estar em qualquer ponto entre 0 e o FPS real
            return lastReportedFps;
        }

        // Método para obter estatísticas detalhadas
        public void LogDetailedStats()
        {
            Debug.WriteLine($"=== Performance Monitor Stats ===");
            Debug.WriteLine($"Total frames registrados: {frameCount}");
            Debug.WriteLine($"Frames no buffer: {frameTimes.Count}");
            Debug.WriteLine($"Frames acumulados neste segundo: {framesInLastSecond}");
            Debug.WriteLine($"Último FPS reportado: {lastReportedFps}");
            
            if (frameTimes.Count > 0)
            {
                double min = double.MaxValue;
                double max = 0;
                double total = 0;
                
                foreach (var time in frameTimes)
                {
                    if (time < min) min = time;
                    if (time > max) max = time;
                    total += time;
                }
                
                double avg = total / frameTimes.Count;
                Debug.WriteLine($"Frame time - Min: {min:F2}ms, Max: {max:F2}ms, Avg: {avg:F2}ms");
                Debug.WriteLine($"FPS estimado - Min: {1000/max:F1}, Max: {1000/min:F1}, Avg: {1000/avg:F1}");
            }
            
            Debug.WriteLine($"Timer ativo: {timer?.IsEnabled}");
            Debug.WriteLine($"Instance inicializada: {instance != null}");
            Debug.WriteLine($"================================");
        }
    }
}