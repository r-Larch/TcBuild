namespace OY.TotalCommander.TcPluginTools
{
    partial class ErrorDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rtbCallStack = new System.Windows.Forms.RichTextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblException = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbCallStack
            // 
            this.rtbCallStack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbCallStack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbCallStack.Location = new System.Drawing.Point(12, 23);
            this.rtbCallStack.Name = "rtbCallStack";
            this.rtbCallStack.ReadOnly = true;
            this.rtbCallStack.Size = new System.Drawing.Size(493, 300);
            this.rtbCallStack.TabIndex = 3;
            this.rtbCallStack.TabStop = false;
            this.rtbCallStack.Text = "";
            this.rtbCallStack.WordWrap = false;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(349, 329);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 20);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.butClose_Click);
            // 
            // lblException
            // 
            this.lblException.AutoSize = true;
            this.lblException.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblException.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblException.Location = new System.Drawing.Point(12, 7);
            this.lblException.Name = "lblException";
            this.lblException.Size = new System.Drawing.Size(51, 13);
            this.lblException.TabIndex = 2;
            this.lblException.Text = "ERROR";
            this.lblException.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAbort
            // 
            this.btnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btnAbort.Location = new System.Drawing.Point(430, 329);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 20);
            this.btnAbort.TabIndex = 1;
            this.btnAbort.Text = "Abort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.butClose_Click);
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 354);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.lblException);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.rtbCallStack);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "ErrorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".NET Plugin Error";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbCallStack;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblException;
        private System.Windows.Forms.Button btnAbort;
    }
}