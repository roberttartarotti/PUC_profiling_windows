using Importador.Logic;
using Importador.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Importador.Manager
{
    public delegate Task UpdateContaUIHandler(Conta conta);
    public delegate Task UpdateMaxProgressHandler(int value);

    public class ContasManager
    {
        public List<Models.Conta> Contas { get; private set; }
        public ObservableCollection<ContaControl> ContasControl { get; set; }

        private ContasLogic contasLogic;

        public UpdateContaUIHandler contaUIHandler;
        public UpdateMaxProgressHandler maxProgressHandler;

        public ContasManager()
        {
            Contas = new List<Models.Conta>();
            ContasControl = new ObservableCollection<ContaControl>();
            contasLogic = new ContasLogic();
        }

        public async Task ImportCSV(StorageFile file)
        {
            try
            {
                Log.ImportadorProvider.Log.ProcessFile(file.Path);

                // Lê o conteúdo do arquivo CSV
                var conteudo = await FileIO.ReadTextAsync(file);
                var linhas = conteudo.Split("\r\n");

                if (maxProgressHandler != null)
                    await maxProgressHandler(linhas.Length - 2);

                bool primeiro = true;

                // Processa cada linha do CSV
                foreach (var linha in linhas)
                {
                    if ((!primeiro) && (!string.IsNullOrWhiteSpace(linha)))
                    {
                        var conta = ProcessLine(linha, file.Path, Guid.NewGuid());

                        if (contaUIHandler != null)
                            await contaUIHandler(conta);

                    }
                    else
                    {
                        primeiro = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorProcess(file.Path);
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Erro",
                    Content = $"Erro ao importar o arquivo CSV: {ex.Message}",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
            }

        }

        private Conta ProcessLine(string line, string path, Guid lineId)
        {
            Log.ImportadorProvider.Log.ProcessLine(path, line, lineId.ToString());

            try
            {
                var data = contasLogic.addOperacaoLine(path, line, lineId);

                Log.ImportadorProvider.Log.ProcessData(path, lineId.ToString(), data.Conta, data.Valor.ToString(), data.Descricao);

                try
                {
                    var conta = Contas.Where(c => c.ID == data.Conta).FirstOrDefault();

                    if (conta == null)
                    {
                        conta = new Models.Conta(data.Conta);   
                        Contas.Add(conta);
                    }
                    conta.Saldo += data.Valor;
                    conta.Operacoes.Add(data);

                    var control = ContasControl.Where(c => c.NumConta == conta.ID).FirstOrDefault();
                    if (control == null)
                    {
                        control = new ContaControl(conta.ID);
                        ContasControl.Add(control);
                    }

                    control.Saldo = conta.Saldo;
                    control.Events = conta.Operacoes.Count;

                    return conta;

                }
                catch (Exception ex)
                {
                    Log.ImportadorProvider.Log.ErrorProcessConta(path, lineId.ToString(), data.Conta);
                    throw new Exception($"Erro ao processar a conta {data.Conta}");
                }
            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorLine(path, line, lineId.ToString());
                throw new Exception($"Erro ao processar a linha {line}");
            }
        }
    }
}
