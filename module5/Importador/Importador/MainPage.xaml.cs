using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Importador.Logic;
using Importador.Models;
using Importador.Manager;
using Windows.Networking.NetworkOperators;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace Importador
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            contasManager = new ContasManager();

            contasManager.progressHandler = UpdateProgress;
            contasManager.maxProgressHandler = UpdateMaxProgress;

            contasManager.fileProgressHandler = UpdateFileProgress;
            contasManager.maxFileProgressHandler = UpdateFileMaxProgress;

            gridData.ItemsSource = contasManager.ContasControl; // Assuming MyDataGrid is the name of your DataGrid
        }

        private ContasManager contasManager;

        private async Task UpdateMaxProgress(double value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatus.Maximum = value;
            });
        }

        private async Task UpdateProgress(double value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatus.Value += value;
            });
        }

        private async Task UpdateFileMaxProgress(double value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatusFile.Maximum = value;
                txtFileStatus.Text = $"Importando {prbStatusFile.Value} bytes de {prbStatusFile.Maximum} bytes";
            });
        }

        private async Task UpdateFileProgress(double value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatusFile.Value += value;
                txtFileStatus.Text = $"Importando {prbStatusFile.Value} bytes de {prbStatusFile.Maximum} bytes";
            });
        }

        private async void btnArquivo_Click(object sender, RoutedEventArgs e)
        {
            // Configura o seletor de arquivos
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");

            Log.ImportadorProvider.Log.StartedImport();

            // Mostra o seletor para o usuário
            StorageFile arquivoSelecionado = await picker.PickSingleFileAsync();

            if (arquivoSelecionado == null)
            {
                // O usuário cancelou a seleção
                return;
            }

            txtArquivo.Text = arquivoSelecionado.Path;
            prbStatus.Value = 0;

            await Task.Run(async () =>
            {
                await contasManager.ImportCSV(arquivoSelecionado);
            });

            Log.ImportadorProvider.Log.CompletedImport();
        }

    }
}
