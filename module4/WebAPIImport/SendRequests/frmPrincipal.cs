using Newtonsoft.Json;
using SendRequests.Models;
using System.Text;
using System.Windows.Forms;

namespace SendRequests
{
    public partial class frmPrincipal : Form
    {
        private Random random = new Random();
        private FormAsync formAsync;

        public frmPrincipal()
        {
            formAsync = new FormAsync(this);
            InitializeComponent();
        }

        private async void sendRequest()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var url = txtServerName.Text + "api/recorddata";
                    var content = new StringContent(JsonConvert.SerializeObject(new RecordDataOut()
                    {
                        value1 = random.Next(1, 10000),
                        value2 = random.Next(1, 10000),
                        value3 = random.Next(1, 10000),
                    }), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        RecordDataIn recordData = JsonConvert.DeserializeObject<RecordDataIn>(responseBody);
                        new Task(() => { formAsync.ExecMethodAsync("WriteMessagge", $"id: {recordData.id}"); }).Start();
                    }
                    else
                    {
                        //MessageBox.Show("Erro ao enviar: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                new Task(() => { formAsync.ExecMethodAsync("WriteMessagge", $"ERRO"); }).Start();
            }
        }

        public void WriteMessagge(string message)
        {
            txtReturn.Text += message + Environment.NewLine;
            prbRequests.Value++;
        }   



        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtNumerRequest.Text, out int numberOfRequest))
            {
                prbRequests.Maximum = numberOfRequest;
                prbRequests.Value = 0;
                for (int i = 0; i < numberOfRequest; i++)
                {
                    new Task(() =>
                    {
                        sendRequest();
                    }).Start();
                }
            }

        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {

        }
    }
}
