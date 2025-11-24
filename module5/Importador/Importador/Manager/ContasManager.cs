using Importador.Log;
using Importador.Logic;
using Importador.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Importador.Manager
{
    public delegate Task UpdateProgressHandler(double value);
    public delegate Task EndProgressHandler();

    public class ContasManager
    {
        private Dictionary<int, Models.Conta> _Contas;
        

        private ContasLogic contasLogic;

        public UpdateProgressHandler progressHandler;
        public UpdateProgressHandler maxProgressHandler;

        public UpdateProgressHandler fileProgressHandler;
        public UpdateProgressHandler maxFileProgressHandler;

        public EndProgressHandler endProgressHandler;

        public ContasManager()
        {
            _Contas = new Dictionary<int, Models.Conta>();
            contasLogic = new ContasLogic();
        }

        public List<Models.Conta> Contas
        {
            get
            {
                lock (_Contas)
                {
                    return _Contas.Values.ToList();
                }
            }
        }

        public async Task ImportCSV(StorageFile file, CancellationToken ct)
        {
            Log.ImportadorProvider.Log.StartedImport();

            try
            {
                Log.ImportadorProvider.Log.ProcessFile(file.Path);

                int sizeMax = 1024;

                var buffer = await FileIO.ReadBufferAsync(file);

                using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
                {
                    if (maxFileProgressHandler != null)
                        await maxFileProgressHandler(buffer.Length);

                    uint size = buffer.Length;
                    string text = "";
                    bool primeiraLinha = false;
                    SemaphoreSlim semaphore = new SemaphoreSlim(20);
                    List<Task> trackedTasks = new List<Task>();
                    while (size > 0)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            try
                            {
                                ct.ThrowIfCancellationRequested();
                            }
                            catch (OperationCanceledException)
                            {
                                ImportadorProvider.Log.ProcessCancel(file.Path);
                            }
                        }

                        List<string> linhas = new List<string>();

                        await semaphore.WaitAsync();

                        if (size < sizeMax)
                        {
                            text += dataReader.ReadString(size);
                            var templinhas = text.Split("\r\n");
                            linhas.AddRange(templinhas);

                            if (fileProgressHandler != null)
                                await fileProgressHandler((int)size);

                            size = 0;
                        }
                        else
                        {
                            text += dataReader.ReadString((uint)sizeMax);
                            var tempLinhas = text.Split("\r\n");
                            linhas.AddRange(tempLinhas.Take(tempLinhas.Length - 1));
                            text = tempLinhas.Last();
                            size -= (uint)sizeMax;

                            if (fileProgressHandler != null)
                                await fileProgressHandler(sizeMax);
                        }

                        if (!primeiraLinha)
                        {
                            if (linhas.Count > 0)
                                linhas.RemoveAt(0); // Remove o cabeçalho
                            primeiraLinha = true;
                        }

                        linhas = linhas.Where(x => !string.IsNullOrEmpty(x)).ToList();

                        if (maxProgressHandler != null)
                            await maxProgressHandler(linhas.Count);

                        trackedTasks.Add(Task.Run(() =>
                        {
                            ProcessLines(linhas, file.Path, ct);
                            semaphore.Release();
                        }, ct));
                    }
                    await Task.WhenAll(trackedTasks);

                    if (endProgressHandler != null)
                        endProgressHandler();

                }

                Log.ImportadorProvider.Log.CompletedImport();

            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorProcess(file.Path);
                //ContentDialog dialog = new ContentDialog
                //{
                //    Title = "Erro",
                //    Content = $"Erro ao importar o arquivo CSV: {ex.Message}",
                //    CloseButtonText = "OK"
                //};
                //await dialog.ShowAsync();
            }

        }

        private void ProcessLines(List<string> lines, string path, CancellationToken ct)
        {
            lines.AsParallel().ForAll(async linha =>
            {
                await ProcessLine(linha, path, Guid.NewGuid(), ct);
            });
        }

        private Conta getConta(int id)
        {
            lock (_Contas)
            {
                if (!_Contas.ContainsKey(id))
                {
                    _Contas.Add(id, new Conta(id));
                }
                return _Contas[id];
            }
        }

        private async Task ProcessLine(string line, string path, Guid lineId, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            Log.ImportadorProvider.Log.ProcessLine(path, line, lineId.ToString());

            try
            {
                var data = contasLogic.addOperacaoLine(path, line, lineId);

                Log.ImportadorProvider.Log.ProcessData(path, lineId.ToString(), data.Conta, data.Valor.ToString(), data.Descricao);

                try
                {
                    Conta conta = getConta(data.Conta);

                    conta.AddOperacao(data);                    

                    if (progressHandler != null)
                        await progressHandler(1);
                }
                catch (Exception ex)
                {
                    Log.ImportadorProvider.Log.ErrorProcessConta(path, lineId.ToString(), data.Conta);
                    throw new Exception($"Erro ao processar a conta {data.Conta} {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorLine(path, line, lineId.ToString());
                throw new Exception($"Erro ao processar a linha {line} {ex.Message}");
            }
        }
    }
}