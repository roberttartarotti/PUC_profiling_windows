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

        public static PerformanceMonitor Instance => instance ??= new PerformanceMonitor();

        public event Action<double> FPSUpdated;

        public void Start()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Monitorar frames
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void Stop()
        {
            timer?.Stop();
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var currentTime = frameTimer.ElapsedMilliseconds;
            var frameTime = currentTime - lastFrameTime;

            if (frameTime > 0)
            {
                frameTimes.Add(frameTime);

                // Manter apenas últimos 60 frames
                if (frameTimes.Count > 60)
                    frameTimes.RemoveAt(0);
            }

            lastFrameTime = currentTime;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var fps = CalculateFPS();
            FPSUpdated?.Invoke(fps);
        }

        public static double CalculateFPS()
        {
            // Garantir que a instância existe
            if (instance == null)
            {
                instance = new PerformanceMonitor();
            }

            if (instance.frameTimes.Count == 0)
                return 0;

            double totalTime = 0;
            foreach (var time in instance.frameTimes)
                totalTime += time;

            double avgFrameTime = totalTime / instance.frameTimes.Count;
            return 1000.0 / avgFrameTime;
        }
    }
}