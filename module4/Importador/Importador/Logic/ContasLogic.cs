using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Shapes;

namespace Importador.Logic
{
    public class ContasLogic
    {
        public List<Models.Conta> Contas { get; private set; }

        public ContasLogic()
        {
            Contas = new List<Models.Conta>();
        }

        public Models.DataIn addOperacaoLine(string fileName, string line, Guid lineCode)
        {
            string[] valores = line.Split(';');

            int conta = int.Parse(valores[0]);
            decimal valor = decimal.Parse(valores[1]);
            string descricao = valores[2];

            // Adiciona os dados ao gridData
            Models.DataIn data = new Models.DataIn { Conta = conta, Valor = valor, Descricao = descricao };

            Log.ImportadorProvider.Log.ProcessData(fileName, lineCode.ToString(), conta, valor.ToString(), descricao);

            try
            {
                var contaMem = Contas.Where(c => c.ID == conta).FirstOrDefault();

                if (contaMem == null)
                {
                    contaMem = new Models.Conta
                    {
                        ID = conta,
                        Saldo = valor,
                        Operacoes = new List<Models.DataIn>()
                    };
                    contaMem.Operacoes.Add(data);
                    Contas.Add(contaMem);
                }
                else
                {
                    contaMem.Saldo += valor;
                    contaMem.Operacoes.Add(data);
                }

            }
            catch (Exception ex)
            {
                Log.ImportadorProvider.Log.ErrorProcessConta(fileName, lineCode.ToString(), conta);
                throw new Exception($"Erro ao processar a conta {conta}");
            }

            return data;
        }
    }
}
