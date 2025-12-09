using System;
using System.Windows.Forms;

namespace DirectXProfilingDemo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // CRÍTICO: Inicializar PIX ANTES de qualquer chamada DirectX
            Console.WriteLine("=== DirectX Profiling Demo ===");
            Console.WriteLine("Inicializando suporte ao PIX...");
            
            bool pixReady = PixHelper.InitializePIX();
            if (pixReady)
            {
                Console.WriteLine("✅ PIX inicializado com sucesso - GPU capture disponível");
            }
            else
            {
                Console.WriteLine("⚠️ PIX não está disponível - instale o PIX for Windows para GPU capture");
                Console.WriteLine("   A aplicação funcionará normalmente para profiling de CPU");
            }

            // Configurar aplicação Windows Forms
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // Criar e executar aplicação
            try
            {
                Console.WriteLine("Iniciando aplicação DirectX...");
                var mainForm = new MainForm();
                
                // Cleanup PIX quando a aplicação fechar
                mainForm.FormClosed += (s, e) => PixHelper.CleanupPIX();
                
                System.Windows.Forms.Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inicializar DirectX:\n{ex.Message}\n\nVerifique se você tem uma placa gráfica compatível com DirectX 11.", 
                    "Erro de Inicialização", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                PixHelper.CleanupPIX();
            }
        }
    }
}