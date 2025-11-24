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
using System.Threading;

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

            //gridData.ItemsSource = contasManager.ContasControl; // Assuming MyDataGrid is the name of your DataGrid

            timer.Interval = TimeSpan.FromMilliseconds(100); // Fires every second
            timer.Tick += timer_Tick;

            synchronizationContext = SynchronizationContext.Current;
        }

        private DispatcherTimer timer = new DispatcherTimer();

        private ContasManager contasManager;

        private SynchronizationContext synchronizationContext;

        private void timer_Tick(object sender, object e)
        {

        }

        private async Task UpdateMaxProgress(double value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatus.Maximum = (prbStatus.Value + value) - prbStatus.Maximum;
                prbStatus.Value = 0;
            });
        }

        private async Task UpdateProgress(double value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatus.Value += value;
            });
        }

        private DateTime lastUpdate;

        private async Task UpdateFileMaxProgress(double value)
        {
            lastUpdate = DateTime.Now;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatusFile.Maximum = value;
            });
            await UpdateFileTextProgress(0);
        }

        private async Task UpdateFileProgress(double value)
        {
            TimeSpan diffTime = DateTime.Now - lastUpdate;
            double millis = diffTime.TotalMilliseconds;
            double bytesPerSecond = (millis > 0) ? (value / millis) * 1000 : 0;
            lastUpdate = DateTime.Now;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                prbStatusFile.Value += value;
            });
            await UpdateFileTextProgress(bytesPerSecond);
        }

        private async Task UpdateFileTextProgress(double bytesPerSecond)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtFileStatus.Text = $"Importando {FormatBytes(prbStatusFile.Value)} de {FormatBytes(prbStatusFile.Maximum)} com uma taxa de {FormatBytes(bytesPerSecond)}PS";
            });
        }

        private string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }
            return String.Format("{0:0.##} {1}", bytes, sizes[order]);
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
