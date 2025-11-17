using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace Importador.Models
{
    public class DataIn
    {
        public int Conta { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; }
    }
}
