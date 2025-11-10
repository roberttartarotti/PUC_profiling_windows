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
            Contas = new ContasLogic();
            MyData = new ObservableCollection<Models.DataIn>();
            gridData.ItemsSource = MyData; // Assuming MyDataGrid is the name of your DataGrid
        }

        public Logic.ContasLogic Contas = new Logic.ContasLogic();

        public ObservableCollection<Models.DataIn> MyData { get; set; }

        private async void ProcessLine(string linha, string fileName, Guid lineCode)
        {
            Log.ImportadorProvider.Log.ProcessLine(fileName, linha, lineCode.ToString());

            try
            {
                // Adiciona os dados ao gridData
                MyData.Add(Contas.addOperacaoLine(fileName, linha, lineCode));

                prbStatus.Value++;
            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorLine(fileName, linha, lineCode.ToString());
                throw new Exception($"Erro ao processar a linha {linha}");
            }
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

            try
            {
                Log.ImportadorProvider.Log.ProcessFile(arquivoSelecionado.Path);
                txtArquivo.Text = arquivoSelecionado.Path;
                // Lê o conteúdo do arquivo CSV
                var conteudo = await FileIO.ReadTextAsync(arquivoSelecionado);
                var linhas = conteudo.Split("\r\n");

                prbStatus.Maximum = linhas.Length - 1;

                bool primeiro = true;

                // Processa cada linha do CSV
                foreach (var linha in linhas)
                {
                    if ((!primeiro) && (!string.IsNullOrWhiteSpace(linha)))
                        ProcessLine(linha, arquivoSelecionado.Path, Guid.NewGuid());
                    else
                    {
                        primeiro = false;
                    }
                    // Fazer algo com os valores
                }
            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorProcess(arquivoSelecionado.Path);
                // Tratar erros de leitura
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Erro",
                    Content = $"Erro ao importar o arquivo CSV: {ex.Message}",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
            }

            Log.ImportadorProvider.Log.CompletedImport();
        }

    }
}
