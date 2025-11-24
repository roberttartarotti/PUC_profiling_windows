using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace Importador.Models
{
    public class Conta
    {
        public int ID { get; set; }
        private double _Saldo;
        public double Saldo
        {
            get
            {
                return _Saldo;
            }
        }
        public List<DataIn> Operacoes { get; set; }

        public Conta(int id)
        {
            ID = id;
            _Saldo = 0;
            Operacoes = new List<DataIn>(); 
        }

        public void AddOperacao(DataIn dataIn)
        {
            double saldoCorrente;
            double saldoNovo;

            do
            {
                saldoCorrente = _Saldo;
                saldoNovo = saldoCorrente + dataIn.Valor;
            } while(Interlocked.CompareExchange(ref _Saldo, saldoNovo, saldoCorrente) != saldoCorrente);

            Operacoes.Add(dataIn);
        }
    }
}
