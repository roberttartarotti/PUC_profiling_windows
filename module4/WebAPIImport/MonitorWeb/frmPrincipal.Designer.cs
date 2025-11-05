namespace MonitorWeb
{
    partial class frmPrincipal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtConsole = new TextBox();
            label1 = new Label();
            txtRecSec = new TextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            txtErroConsole = new TextBox();
            saveFileDialog1 = new SaveFileDialog();
            SuspendLayout();
            // 
            // txtConsole
            // 
            txtConsole.Location = new Point(730, 292);
            txtConsole.Multiline = true;
            txtConsole.Name = "txtConsole";
            txtConsole.Size = new Size(762, 244);
            txtConsole.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(210, 32);
            label1.TabIndex = 1;
            label1.Text = "Req por Segundos";
            // 
            // txtRecSec
            // 
            txtRecSec.Location = new Point(12, 44);
            txtRecSec.Name = "txtRecSec";
            txtRecSec.Size = new Size(413, 39);
            txtRecSec.TabIndex = 2;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // txtErroConsole
            // 
            txtErroConsole.Location = new Point(730, 21);
            txtErroConsole.Multiline = true;
            txtErroConsole.Name = "txtErroConsole";
            txtErroConsole.Size = new Size(762, 244);
            txtErroConsole.TabIndex = 3;
            // 
            // frmPrincipal
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1529, 690);
            Controls.Add(txtErroConsole);
            Controls.Add(txtRecSec);
            Controls.Add(label1);
            Controls.Add(txtConsole);
            Name = "frmPrincipal";
            Text = "Monitoramento da WEBAPI";
            Load += frmPrincipal_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtConsole;
        private Label label1;
        private TextBox txtRecSec;
        private System.Windows.Forms.Timer timer1;
        private TextBox txtErroConsole;
        private SaveFileDialog saveFileDialog1;
    }
}
