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
        public ContasLogic()
        {

        }

        public Models.DataIn addOperacaoLine(string fileName, string line, Guid lineCode)
        {
            string[] valores = line.Split(';');

            int conta = int.Parse(valores[0]);
            decimal valor = decimal.Parse(valores[1]);
            string descricao = valores[2];

            // Adiciona os dados ao gridData
            return new Models.DataIn { Conta = conta, Valor = valor, Descricao = descricao };

        }
    }
}
