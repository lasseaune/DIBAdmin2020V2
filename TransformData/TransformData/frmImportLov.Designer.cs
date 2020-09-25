namespace TransformData
{
    partial class frmImportLov
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnDownload = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLawSite = new System.Windows.Forms.TextBox();
            this.btnLoadLaw = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnGo = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.webAdress = new System.Windows.Forms.TextBox();
            this.lawName = new System.Windows.Forms.ComboBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnDownload);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.txtLawSite);
            this.splitContainer1.Panel1.Controls.Add(this.btnLoadLaw);
            this.splitContainer1.Panel1.Controls.Add(this.btnAbort);
            this.splitContainer1.Panel1.Controls.Add(this.btnGo);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.webAdress);
            this.splitContainer1.Panel1.Controls.Add(this.lawName);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(885, 575);
            this.splitContainer1.SplitterDistance = 142;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(691, 76);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 18;
            this.btnDownload.Text = "Last ned";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Lov-site:";
            // 
            // txtLawSite
            // 
            this.txtLawSite.Location = new System.Drawing.Point(77, 12);
            this.txtLawSite.Name = "txtLawSite";
            this.txtLawSite.Size = new System.Drawing.Size(241, 20);
            this.txtLawSite.TabIndex = 21;
            this.txtLawSite.Text = "http://lovdata.no/register/lover?sort=alpha";
            this.txtLawSite.TextChanged += new System.EventHandler(this.txtLawSite_TextChanged);
            // 
            // btnLoadLaw
            // 
            this.btnLoadLaw.Location = new System.Drawing.Point(331, 10);
            this.btnLoadLaw.Name = "btnLoadLaw";
            this.btnLoadLaw.Size = new System.Drawing.Size(76, 23);
            this.btnLoadLaw.TabIndex = 20;
            this.btnLoadLaw.Text = "Last Lover";
            this.btnLoadLaw.UseVisualStyleBackColor = true;
            this.btnLoadLaw.Click += new System.EventHandler(this.btnLoadLaw_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(691, 105);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 19;
            this.btnAbort.Text = "Lukk";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnGo
            // 
            this.btnGo.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnGo.Location = new System.Drawing.Point(641, 107);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(27, 19);
            this.btnGo.TabIndex = 17;
            this.btnGo.Text = "...";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Adresse";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Navn:";
            // 
            // webAdress
            // 
            this.webAdress.Location = new System.Drawing.Point(64, 107);
            this.webAdress.Name = "webAdress";
            this.webAdress.Size = new System.Drawing.Size(571, 20);
            this.webAdress.TabIndex = 14;
            this.webAdress.TextChanged += new System.EventHandler(this.webAdress_TextChanged);
            // 
            // lawName
            // 
            this.lawName.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lawName.FormattingEnabled = true;
            this.lawName.Location = new System.Drawing.Point(77, 47);
            this.lawName.MaxDropDownItems = 30;
            this.lawName.Name = "lawName";
            this.lawName.Size = new System.Drawing.Size(558, 23);
            this.lawName.TabIndex = 13;
            this.lawName.SelectedIndexChanged += new System.EventHandler(this.lawName_SelectedIndexChanged);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(885, 429);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // frmImportLov
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 575);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmImportLov";
            this.Text = "Importer lov";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox webAdress;
        private System.Windows.Forms.ComboBox lawName;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnLoadLaw;
        private System.Windows.Forms.TextBox txtLawSite;
        private System.Windows.Forms.Label label3;

    }
}