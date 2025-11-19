using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Importador
{
    public sealed partial class ContaControl : UserControl
    {
        public int NumConta { get; private set; }

        public int Events { set
            {
                txtNumberEvents.Text = value.ToString();
            }
        }

        public decimal Saldo
        {
            set
            {
                txtSaldo.Text = "R$ " + value.ToString("0.00");
            }
        }

        public ContaControl(int numConta)
        {
            this.InitializeComponent();
            NumConta = numConta;
            txtName.Text = $"Conta {numConta}";
            txtSaldo.Text = "R$ 0.00";
        }

        

    }
}
