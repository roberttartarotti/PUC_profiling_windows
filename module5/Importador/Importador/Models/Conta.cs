using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importador.Models
{
    public class Conta
    {
        public int ID { get; set; }
        public decimal Saldo { get; set; }
        public List<DataIn> Operacoes { get; set; }

        public Conta(int id)
        {
            ID = id;
            Saldo = 0;
            Operacoes = new List<DataIn>(); 
        }
    }
}
