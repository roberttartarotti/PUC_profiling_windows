using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing;

namespace MonitorWeb
{
    public partial class frmPrincipal : Form
    {
        private FormAsync formAsync;

        public frmPrincipal()
        {
            formAsync = new FormAsync(this);
            InitializeComponent();
        }

        private int Requests = 0;

        public void WriteMessagge(int eventCode)
        {
            switch (eventCode)
            {
                case 1000:
                    txtConsole.Text = "Web server started" + Environment.NewLine;
                    break;
                case 1200:
                    txtConsole.Text += "Request" + Environment.NewLine;
                    Requests++;
                    break;
                case 1300:
                    txtConsole.Text += "Error on request" + Environment.NewLine;
                    txtErroConsole.Text += "Error on request" + Environment.NewLine;
                    break;
                default:
                    break;
            }
        }

        private void Monitor()
        {
            using (var session = new TraceEventSession("MyEventSession"))
            {
                session.EnableProvider("WebAPIImportApp");

                session.EnableProvider(new Guid("{B2345678-1234-1234-1234-123456789ABC}"));

                session.Source.AllEvents += delegate (TraceEvent data)
                {
                    Task.Run(() =>
                    {
                        formAsync.ExecMethodAsync("WriteMessagge", (int)data.ID);
                    });
                };

                Task.Run(() => session.Source.Process());

                while (true) ;
            }
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            Task.Run(() => Monitor());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtRecSec.Text = Requests.ToString();
            Requests = 0;
        }

        
    }
}
