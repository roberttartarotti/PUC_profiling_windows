namespace SendRequests
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
            lblServerName = new Label();
            txtServerName = new TextBox();
            lblNumerRequest = new Label();
            txtNumerRequest = new TextBox();
            btnSend = new Button();
            txtReturn = new TextBox();
            prbRequests = new ProgressBar();
            SuspendLayout();
            // 
            // lblServerName
            // 
            lblServerName.AutoSize = true;
            lblServerName.Location = new Point(12, 9);
            lblServerName.Name = "lblServerName";
            lblServerName.Size = new Size(157, 32);
            lblServerName.TabIndex = 0;
            lblServerName.Text = "Server Name:";
            // 
            // txtServerName
            // 
            txtServerName.Location = new Point(12, 44);
            txtServerName.Name = "txtServerName";
            txtServerName.Size = new Size(461, 39);
            txtServerName.TabIndex = 1;
            txtServerName.Text = "https://localhost:7286/";
            // 
            // lblNumerRequest
            // 
            lblNumerRequest.AutoSize = true;
            lblNumerRequest.Location = new Point(12, 86);
            lblNumerRequest.Name = "lblNumerRequest";
            lblNumerRequest.Size = new Size(223, 32);
            lblNumerRequest.TabIndex = 2;
            lblNumerRequest.Text = "Number of Request";
            // 
            // txtNumerRequest
            // 
            txtNumerRequest.Location = new Point(12, 121);
            txtNumerRequest.Name = "txtNumerRequest";
            txtNumerRequest.Size = new Size(461, 39);
            txtNumerRequest.TabIndex = 3;
            txtNumerRequest.Text = "1";
            // 
            // btnSend
            // 
            btnSend.Location = new Point(479, 86);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(207, 74);
            btnSend.TabIndex = 4;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtReturn
            // 
            txtReturn.Location = new Point(29, 246);
            txtReturn.Multiline = true;
            txtReturn.Name = "txtReturn";
            txtReturn.Size = new Size(920, 231);
            txtReturn.TabIndex = 5;
            // 
            // prbRequests
            // 
            prbRequests.Location = new Point(12, 178);
            prbRequests.Name = "prbRequests";
            prbRequests.Size = new Size(937, 47);
            prbRequests.TabIndex = 6;
            // 
            // frmPrincipal
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1408, 573);
            Controls.Add(prbRequests);
            Controls.Add(txtReturn);
            Controls.Add(btnSend);
            Controls.Add(txtNumerRequest);
            Controls.Add(lblNumerRequest);
            Controls.Add(txtServerName);
            Controls.Add(lblServerName);
            Name = "frmPrincipal";
            Text = "Send Requests";
            Load += frmPrincipal_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblServerName;
        private TextBox txtServerName;
        private Label lblNumerRequest;
        private TextBox txtNumerRequest;
        private Button btnSend;
        private TextBox txtReturn;
        private ProgressBar prbRequests;
    }
}
