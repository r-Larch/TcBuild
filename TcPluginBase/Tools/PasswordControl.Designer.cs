namespace OY.TotalCommander.TcPluginTools
{
    partial class PasswordControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnClearPassword = new System.Windows.Forms.Button();
            this.cbxUseMasterPassword = new System.Windows.Forms.CheckBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnClearPassword
            // 
            this.btnClearPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearPassword.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClearPassword.Location = new System.Drawing.Point(291, 38);
            this.btnClearPassword.Name = "btnClearPassword";
            this.btnClearPassword.Size = new System.Drawing.Size(50, 23);
            this.btnClearPassword.TabIndex = 2;
            this.btnClearPassword.Text = "Clear";
            this.btnClearPassword.UseVisualStyleBackColor = true;
            this.btnClearPassword.Click += new System.EventHandler(this.btnClearPassword_Click);
            // 
            // cbxUseMasterPassword
            // 
            this.cbxUseMasterPassword.AutoSize = true;
            this.cbxUseMasterPassword.Location = new System.Drawing.Point(17, 42);
            this.cbxUseMasterPassword.Name = "cbxUseMasterPassword";
            this.cbxUseMasterPassword.Size = new System.Drawing.Size(263, 17);
            this.cbxUseMasterPassword.TabIndex = 1;
            this.cbxUseMasterPassword.Text = "Use TC master password to protect your password";
            this.cbxUseMasterPassword.UseVisualStyleBackColor = true;
            this.cbxUseMasterPassword.CheckedChanged += new System.EventHandler(this.cbxUseMasterPassword_CheckedChanged);
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(76, 3);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(265, 20);
            this.txtPassword.TabIndex = 0;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.WordWrap = false;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(14, 6);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password:";
            // 
            // lblWarning
            // 
            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new System.Drawing.Point(14, 26);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(208, 13);
            this.lblWarning.TabIndex = 4;
            this.lblWarning.Text = "Warning: Storing the password is insecure!";
            // 
            // PasswordControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.btnClearPassword);
            this.Controls.Add(this.cbxUseMasterPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.MinimumSize = new System.Drawing.Size(341, 66);
            this.Name = "PasswordControl";
            this.Size = new System.Drawing.Size(350, 66);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClearPassword;
        private System.Windows.Forms.CheckBox cbxUseMasterPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblWarning;

    }
}
