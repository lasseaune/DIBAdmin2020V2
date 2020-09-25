namespace TransformData
{
    partial class frmImportKodeOversikt
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
            this.btmImport = new System.Windows.Forms.Button();
            this.webadress = new System.Windows.Forms.TextBox();
            this.btnAbort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btmImport
            // 
            this.btmImport.Location = new System.Drawing.Point(376, 21);
            this.btmImport.Name = "btmImport";
            this.btmImport.Size = new System.Drawing.Size(75, 23);
            this.btmImport.TabIndex = 0;
            this.btmImport.Text = "Import";
            this.btmImport.UseVisualStyleBackColor = true;
            this.btmImport.Click += new System.EventHandler(this.btmImport_Click);
            // 
            // webadress
            // 
            this.webadress.Location = new System.Drawing.Point(12, 21);
            this.webadress.Name = "webadress";
            this.webadress.Size = new System.Drawing.Size(358, 20);
            this.webadress.TabIndex = 1;
            this.webadress.Text = "http://www.skatteetaten.no/no/Bedrift-og-organisasjon/Rapportering-til-Skatteetat" +
    "en/Lonn-og-arbeidsgiveravgift/Lonns--og-trekkoppgaver/Kodeoversikten/Kodeoversik" +
    "t-for-lonns--og-trekkoppgaver/";
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(376, 179);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 2;
            this.btnAbort.Text = "Avbryt";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // frmImportKodeOversikt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 211);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.webadress);
            this.Controls.Add(this.btmImport);
            this.Name = "frmImportKodeOversikt";
            this.Text = "frmImportKodeOversikt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btmImport;
        private System.Windows.Forms.TextBox webadress;
        private System.Windows.Forms.Button btnAbort;
    }
}