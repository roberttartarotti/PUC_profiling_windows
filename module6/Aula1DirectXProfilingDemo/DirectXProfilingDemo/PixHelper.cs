using System.Runtime.InteropServices;

namespace DirectXProfilingDemo
{
    public static class PixHelper
    {
        // P/Invoke para carregar WinPixGpuCapturer.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // Eventos PIX (opcionais, mas úteis)
        [DllImport("WinPixEventRuntime.dll", EntryPoint = "PIXBeginEvent", CallingConvention = CallingConvention.StdCall)]
        public static extern void PIXBeginEvent(ulong color, string name);

        [DllImport("WinPixEventRuntime.dll", EntryPoint = "PIXEndEvent", CallingConvention = CallingConvention.StdCall)]
        public static extern void PIXEndEvent();

        [DllImport("WinPixEventRuntime.dll", EntryPoint = "PIXSetMarker", CallingConvention = CallingConvention.StdCall)]
        public static extern void PIXSetMarker(ulong color, string name);

        private static IntPtr pixGpuCapturerHandle = IntPtr.Zero;
        private static bool pixInitialized = false;
        private static string pixInstallPath = string.Empty;

        /// <summary>
        /// Inicializa PIX carregando WinPixGpuCapturer.dll DIRETAMENTE do diretório de instalação do PIX
        /// </summary>
        public static bool InitializePIX()
        {
            if (pixInitialized)
                return true;

            try
            {
                // Lista de caminhos possíveis para PIX (DIRETÓRIO DE INSTALAÇÃO)
                string[] possiblePaths = {
                    @"C:\Program Files\Microsoft PIX\2509.25",
                    @"C:\Program Files\Microsoft PIX",
                    @"C:\Program Files (x86)\Microsoft PIX\2509.25",
                    @"C:\Program Files (x86)\Microsoft PIX",
                    // Versões mais recentes
                    @"C:\Program Files\Microsoft PIX\2510.01",
                    @"C:\Program Files\Microsoft PIX\2511.01"
                };

                foreach (string pixPath in possiblePaths)
                {
                    string pixDllPath = System.IO.Path.Combine(pixPath, "WinPixGpuCapturer.dll");

                    if (System.IO.File.Exists(pixDllPath))
                    {
                        Console.WriteLine($"[PIX] ?? Tentando carregar do diretório PIX: {pixPath}");

                        // IMPORTANTE: Carregar DIRETAMENTE do diretório de instalação do PIX
                        pixGpuCapturerHandle = LoadLibrary(pixDllPath);

                        if (pixGpuCapturerHandle != IntPtr.Zero)
                        {
                            pixInitialized = true;
                            pixInstallPath = pixPath;
                            Console.WriteLine($"[PIX] ? WinPixGpuCapturer.dll carregado com sucesso!");
                            Console.WriteLine($"[PIX] ?? Diretório PIX: {pixPath}");
                            Console.WriteLine($"[PIX] ?? PIX GPU capture habilitado - pronto para attach!");
                            Console.WriteLine($"[PIX] ? Versão compatível detectada");
                            return true;
                        }
                        else
                        {
                            int error = Marshal.GetLastWin32Error();
                            Console.WriteLine($"[PIX] ? Falha ao carregar de {pixPath} - Erro: {error}");
                        }
                    }
                }

                // Se chegou aqui, não encontrou PIX
                Console.WriteLine("[PIX] ? WinPixGpuCapturer.dll não encontrada nos diretórios de instalação PIX:");
                foreach (string path in possiblePaths)
                {
                    Console.WriteLine($"[PIX]   ? {path}");
                }
                Console.WriteLine("[PIX] ?? Para habilitar PIX:");
                Console.WriteLine("[PIX]   1. Instale PIX for Windows da Microsoft Store ou site oficial");
                Console.WriteLine("[PIX]   2. NÃO copie DLLs - PIX deve usar arquivos originais");
                Console.WriteLine("[PIX]   3. Execute PIX from Windows como Administrador");
                Console.WriteLine("[PIX]   4. Use 'Attach to Process' no PIX");
                Console.WriteLine("[PIX] ?? Mais info: https://devblogs.microsoft.com/pix/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PIX] ? Exceção durante inicialização PIX: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Cleanup PIX resources
        /// </summary>
        public static void CleanupPIX()
        {
            if (pixGpuCapturerHandle != IntPtr.Zero)
            {
                FreeLibrary(pixGpuCapturerHandle);
                pixGpuCapturerHandle = IntPtr.Zero;
                pixInitialized = false;
                Console.WriteLine("[PIX] ?? WinPixGpuCapturer.dll descarregado");
            }
        }

        /// <summary>
        /// Inicia um evento PIX para profiling
        /// </summary>
        public static void BeginPixEvent(string eventName, uint color = 0xFF00FF00)
        {
            if (!pixInitialized) return;

            try
            {
                PIXBeginEvent(color, eventName);
            }
            catch
            {
                // PIX events são opcionais, não falhar se não funcionarem
            }
        }

        /// <summary>
        /// Termina o evento PIX atual
        /// </summary>
        public static void EndPixEvent()
        {
            if (!pixInitialized) return;

            try
            {
                PIXEndEvent();
            }
            catch
            {
                // PIX events são opcionais, não falhar se não funcionarem
            }
        }

        /// <summary>
        /// Define um marker PIX
        /// </summary>
        public static void SetPixMarker(string markerName, uint color = 0xFFFF0000)
        {
            if (!pixInitialized) return;

            try
            {
                PIXSetMarker(color, markerName);
            }
            catch
            {
                // PIX events são opcionais, não falhar se não funcionarem
            }
        }

        /// <summary>
        /// Verifica se PIX está inicializado e pronto para capture
        /// </summary>
        public static bool IsPixReady => pixInitialized && pixGpuCapturerHandle != IntPtr.Zero;

        /// <summary>
        /// Retorna o diretório de instalação do PIX que foi detectado
        /// </summary>
        public static string PixInstallPath => pixInstallPath;

        /// <summary>
        /// Informações de status do PIX para debugging
        /// </summary>
        public static string GetPixStatus()
        {
            if (!pixInitialized)
                return "? PIX não inicializado";

            return $"? PIX ativo | Diretório: {pixInstallPath} | Handle: 0x{pixGpuCapturerHandle:X}";
        }
    }
}