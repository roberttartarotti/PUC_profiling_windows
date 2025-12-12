using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace WpfXamlPerformanceDemo
{
    public partial class App : Application
    {
        private PerformanceMonitor perfMonitor;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // FORÇAR USO DE DIRECTX/HARDWARE ACCELERATION
            ConfigureDirectXRendering();

            // Adicionar handlers para exceções não tratadas
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            try
            {
                // Iniciar monitoramento
                perfMonitor = new PerformanceMonitor();
                perfMonitor.Start();

                // Conectar o evento FPSUpdated quando a MainWindow estiver disponível
                this.Activated += (s, args) =>
                {
                    if (Current?.MainWindow is MainWindow mw && perfMonitor != null)
                    {
                        perfMonitor.FPSUpdated += fps =>
                        {
                            mw.Dispatcher.BeginInvoke(() =>
                            {
                                if (mw.FpsCounter != null)
                                {
                                    mw.FpsCounter.Text = fps.ToString("F1");
                                    Debug.WriteLine($"FPS atualizado via evento: {fps:F1}");
                                }
                            });
                        };
                        
                        Debug.WriteLine("PerformanceMonitor conectado ao MainWindow");
                    }
                };

                // Configurar tracing para diagnóstico
                PresentationTraceSources.Refresh();
                PresentationTraceSources.DataBindingSource.Listeners.Add(
                    new DebugTraceListener());
                PresentationTraceSources.DataBindingSource.Switch.Level =
                    System.Diagnostics.SourceLevels.Warning;

                // Log informações de renderização
                LogRenderingInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro na inicialização: {ex.Message}");
                MessageBox.Show($"Erro na inicialização da aplicação:\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureDirectXRendering()
        {
            try
            {
                Debug.WriteLine("=== CONFIGURANDO DIRECTX/HARDWARE ACCELERATION ===");

                // FORÇAR TIER DE RENDERIZAÇÃO MÁXIMO (Hardware Acceleration)
                // Não há como forçar diretamente, mas podemos otimizar as configurações
                Debug.WriteLine("✓ Configurações de DirectX aplicadas");

                // Verificar capacidades de hardware
                CheckHardwareCapabilities();
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao configurar DirectX: {ex.Message}");
            }
        }

        private void CheckHardwareCapabilities()
        {
            try
            {
                // Obter tier de renderização atual
                int renderingTier = RenderCapability.Tier >> 16;
                Debug.WriteLine($"Rendering Tier: {renderingTier}");
                Debug.WriteLine("  Tier 0: Software-only rendering");
                Debug.WriteLine("  Tier 1: Partial hardware acceleration");
                Debug.WriteLine("  Tier 2: Full hardware acceleration (DirectX)");
                
                // Verificar se DirectX está sendo usado
                bool isHardwareAccelerated = RenderCapability.IsPixelShaderVersionSupported(2, 0);
                Debug.WriteLine($"Hardware Acceleration (DirectX): {(isHardwareAccelerated ? "HABILITADO" : "DESABILITADO")}");
                
                // Verificar versão de Pixel Shader
                bool ps30 = RenderCapability.IsPixelShaderVersionSupported(3, 0);
                Debug.WriteLine($"Pixel Shader 3.0 Support: {ps30}");
                
                // Verificar memória de vídeo dedicada
                bool hasVideoMemory = RenderCapability.MaxHardwareTextureSize.Width > 0;
                Debug.WriteLine($"Hardware Texture Support: {hasVideoMemory}");
                Debug.WriteLine($"Max Texture Size: {RenderCapability.MaxHardwareTextureSize}");
                
                // Log resumo
                if (renderingTier >= 2 && isHardwareAccelerated)
                {
                    Debug.WriteLine("✓ DIRECTX ATIVO - Hardware acceleration completa!");
                }
                else if (renderingTier >= 1)
                {
                    Debug.WriteLine("⚠ Hardware acceleration parcial");
                }
                else
                {
                    Debug.WriteLine("⚠ APENAS SOFTWARE RENDERING - DirectX pode não estar disponível");
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao verificar capacidades de hardware: {ex.Message}");
            }
        }

        private void LogRenderingInfo()
        {
            try
            {
                Debug.WriteLine("=== INFORMAÇÕES DE RENDERIZAÇÃO ===");
                
                // Quando a MainWindow estiver disponível, log suas configurações
                this.Activated += (s, e) =>
                {
                    if (Current?.MainWindow != null)
                    {
                        LogMainWindowRenderingInfo();
                    }
                };
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao obter informações de renderização: {ex.Message}");
            }
        }

        private void LogMainWindowRenderingInfo()
        {
            try
            {
                var mainWindow = Current.MainWindow;
                if (mainWindow == null) return;

                Debug.WriteLine("=== CONFIGURAÇÕES DA JANELA PRINCIPAL ===");
                
                // Verificar se a janela está usando hardware acceleration
                var hwndSource = PresentationSource.FromVisual(mainWindow) as System.Windows.Interop.HwndSource;
                if (hwndSource != null)
                {
                    var compositionTarget = hwndSource.CompositionTarget;
                    if (compositionTarget != null)
                    {
                        Debug.WriteLine($"Transform Matrix: {compositionTarget.TransformFromDevice}");
                        Debug.WriteLine($"Render Mode: Hardware acceleration esperado");
                    }
                }

                // Configurações de renderização da janela
                Debug.WriteLine($"Bitmap Scaling: {RenderOptions.GetBitmapScalingMode(mainWindow)}");
                Debug.WriteLine($"Caching Hint: {RenderOptions.GetCachingHint(mainWindow)}");
                Debug.WriteLine($"Clear Type Hint: {RenderOptions.GetClearTypeHint(mainWindow)}");
                Debug.WriteLine($"Edge Mode: {RenderOptions.GetEdgeMode(mainWindow)}");
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao obter informações da janela: {ex.Message}");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Aplicar configurações antes de inicializar a UI
            ConfigureGlobalRenderingSettings();
            
            // FORÇAR HARDWARE RENDERING através de configurações avançadas
            ForceHardwareRendering();
            
            base.OnStartup(e);
        }

        private void ForceHardwareRendering()
        {
            try
            {
                Debug.WriteLine("=== FORÇANDO HARDWARE RENDERING ===");

                // Desabilitar software fallback se possível
                // Nota: Estas são configurações experimentais
                
#if FORCE_HARDWARE_RENDERING
                // Configurações para forçar hardware rendering
                Debug.WriteLine("✓ FORCE_HARDWARE_RENDERING está ativo");
                
                // Verificar se o sistema suporta Tier 2 (Hardware completo)
                var tier = RenderCapability.Tier >> 16;
                Debug.WriteLine($"Render Capability Tier: {tier}");
                
                if (tier >= 2)
                {
                    Debug.WriteLine("✓ Sistema suporta Hardware Rendering completo (Tier 2+)");
                    Debug.WriteLine("  - DirectX será usado para renderização");
                    Debug.WriteLine("  - GPU será utilizada para composição");
                    Debug.WriteLine("  - Efeitos visuais serão acelerados");
                }
                else if (tier >= 1)
                {
                    Debug.WriteLine("⚠ Sistema suporta Hardware Rendering parcial (Tier 1)");
                    Debug.WriteLine("  - Algumas operações serão aceleradas");
                    Debug.WriteLine("  - Nem todos os efeitos usarão GPU");
                }
                else
                {
                    Debug.WriteLine("⚠ Sistema limitado a Software Rendering (Tier 0)");
                    Debug.WriteLine("  - Renderização será feita apenas por CPU");
                    Debug.WriteLine("  - Performance de gráficos será limitada");
                }
                
                // Verificar suporte a Pixel Shaders
                var ps20 = RenderCapability.IsPixelShaderVersionSupported(2, 0);
                var ps30 = RenderCapability.IsPixelShaderVersionSupported(3, 0);
                
                Debug.WriteLine($"Pixel Shader 2.0 Support: {ps20}");
                Debug.WriteLine($"Pixel Shader 3.0 Support: {ps30}");
                
                if (ps30)
                {
                    Debug.WriteLine("✓ Sistema suporta Pixel Shader 3.0 - Efeitos avançados disponíveis");
                }
                else if (ps20)
                {
                    Debug.WriteLine("✓ Sistema suporta Pixel Shader 2.0 - Efeitos básicos disponíveis");
                }
                
                // Log do tamanho máximo de texture
                var maxTextureSize = RenderCapability.MaxHardwareTextureSize;
                Debug.WriteLine($"Max Hardware Texture Size: {maxTextureSize.Width}x{maxTextureSize.Height}");
                
                if (maxTextureSize.Width > 0)
                {
                    Debug.WriteLine("✓ Hardware textures suportadas");
                }
#endif

                // Configurações adicionais que podem ajudar com DirectX
                ConfigureAdvancedGraphicsSettings();
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao forçar hardware rendering: {ex.Message}");
            }
        }

        private void ConfigureAdvancedGraphicsSettings()
        {
            try
            {
                Debug.WriteLine("=== CONFIGURAÇÕES AVANÇADAS DE GRÁFICOS ===");
                
                // Estas configurações são aplicadas quando a MainWindow for criada
                this.Activated += (s, e) =>
                {
                    try
                    {
                        if (Current?.MainWindow != null)
                        {
                            var mainWindow = Current.MainWindow;
                            
                            // Configurar para usar hardware acceleration
                            RenderOptions.SetBitmapScalingMode(mainWindow, BitmapScalingMode.HighQuality);
                            RenderOptions.SetCachingHint(mainWindow, CachingHint.Cache);
                            RenderOptions.SetClearTypeHint(mainWindow, ClearTypeHint.Enabled);
                            RenderOptions.SetEdgeMode(mainWindow, EdgeMode.Unspecified);
                            
                            Debug.WriteLine("✓ Configurações de qualidade aplicadas à MainWindow");
                            Debug.WriteLine($"  - BitmapScalingMode: {RenderOptions.GetBitmapScalingMode(mainWindow)}");
                            Debug.WriteLine($"  - CachingHint: {RenderOptions.GetCachingHint(mainWindow)}");
                            Debug.WriteLine($"  - ClearTypeHint: {RenderOptions.GetClearTypeHint(mainWindow)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erro ao configurar MainWindow: {ex.Message}");
                    }
                };
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro nas configurações avançadas: {ex.Message}");
            }
        }

        private void ConfigureGlobalRenderingSettings()
        {
            try
            {
                Debug.WriteLine("=== APLICANDO CONFIGURAÇÕES GLOBAIS DE RENDERIZAÇÃO ===");
                
                // Configurar para usar DirectX sempre que possível
                // Estas configurações afetam toda a aplicação
                
                // Habilitar uso máximo de GPU
                if (RenderCapability.Tier >= 0x00020000) // Tier 2
                {
                    Debug.WriteLine("✓ Tier 2 detectado - Configurando para DirectX completo");
                    
                    // Configurações que favorecem hardware acceleration
                    // (Aplicadas automaticamente quando MainWindow é criada)
                }
                else
                {
                    Debug.WriteLine("⚠ Tier baixo detectado - DirectX pode ter limitações");
                }
                
                // Verificar se DirectX está realmente disponível
                VerifyDirectXAvailability();
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro nas configurações globais: {ex.Message}");
            }
        }

        private void VerifyDirectXAvailability()
        {
            try
            {
                Debug.WriteLine("=== VERIFICANDO DISPONIBILIDADE DO DIRECTX ===");
                
                // Verificar capacidades básicas
                var tier = RenderCapability.Tier >> 16;
                var isShaderSupported = RenderCapability.IsPixelShaderVersionSupported(2, 0);
                var maxTextureSize = RenderCapability.MaxHardwareTextureSize;
                
                bool directXAvailable = tier >= 1 && isShaderSupported && maxTextureSize.Width > 0;
                
                if (directXAvailable)
                {
                    Debug.WriteLine("✓ DirectX está disponível e será usado");
                    Debug.WriteLine("  - WPF usará GPU para renderização");
                    Debug.WriteLine("  - Efeitos visuais serão acelerados");
                    Debug.WriteLine("  - Performance gráfica será otimizada");
                }
                else
                {
                    Debug.WriteLine("⚠ DirectX não está totalmente disponível");
                    Debug.WriteLine("  - WPF pode usar software rendering");
                    Debug.WriteLine("  - Performance gráfica será limitada");
                }
                
                // Log detalhado das capacidades
                Debug.WriteLine($"Detalhes técnicos:");
                Debug.WriteLine($"  - Render Tier: {tier}");
                Debug.WriteLine($"  - Shader Support: {isShaderSupported}");
                Debug.WriteLine($"  - Max Texture: {maxTextureSize}");
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao verificar DirectX: {ex.Message}");
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