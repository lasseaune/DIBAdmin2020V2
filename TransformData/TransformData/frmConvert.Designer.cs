namespace TransformData
{
    partial class frmConvert
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnCheckDoc = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnConvertToXml = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnConvertByContent = new System.Windows.Forms.Button();
            this.btnViewConverted = new System.Windows.Forms.Button();
            this.cbLanguage = new System.Windows.Forms.ComboBox();
            this.btnConvertHtag = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(300, 49);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Konverter";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 23);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(371, 20);
            this.textBox1.TabIndex = 1;
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(391, 23);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(24, 19);
            this.btnOpenFile.TabIndex = 2;
            this.btnOpenFile.Text = "...";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnCheckDoc
            // 
            this.btnCheckDoc.Location = new System.Drawing.Point(300, 135);
            this.btnCheckDoc.Name = "btnCheckDoc";
            this.btnCheckDoc.Size = new System.Drawing.Size(115, 23);
            this.btnCheckDoc.TabIndex = 3;
            this.btnCheckDoc.Text = "Sjekk Dokument";
            this.btnCheckDoc.UseVisualStyleBackColor = true;
            this.btnCheckDoc.Click += new System.EventHandler(this.btnCheckDoc_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "MS word filnavn";
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(336, 244);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 5;
            this.btnAbort.Text = "Avbryt";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(255, 244);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnConvertToXml
            // 
            this.btnConvertToXml.Enabled = false;
            this.btnConvertToXml.Location = new System.Drawing.Point(12, 135);
            this.btnConvertToXml.Name = "btnConvertToXml";
            this.btnConvertToXml.Size = new System.Drawing.Size(127, 23);
            this.btnConvertToXml.TabIndex = 7;
            this.btnConvertToXml.Text = "Konverter til XML";
            this.btnConvertToXml.UseVisualStyleBackColor = true;
            this.btnConvertToXml.Click += new System.EventHandler(this.btnConvertToXml_Click);
            // 
            // btnConvertByContent
            // 
            this.btnConvertByContent.Enabled = false;
            this.btnConvertByContent.Location = new System.Drawing.Point(12, 164);
            this.btnConvertByContent.Name = "btnConvertByContent";
            this.btnConvertByContent.Size = new System.Drawing.Size(127, 23);
            this.btnConvertByContent.TabIndex = 8;
            this.btnConvertByContent.Text = "Konverter m/Content";
            this.btnConvertByContent.UseVisualStyleBackColor = true;
            this.btnConvertByContent.Click += new System.EventHandler(this.btnConvertByContent_Click);
            // 
            // btnViewConverted
            // 
            this.btnViewConverted.Enabled = false;
            this.btnViewConverted.Location = new System.Drawing.Point(12, 193);
            this.btnViewConverted.Name = "btnViewConverted";
            this.btnViewConverted.Size = new System.Drawing.Size(127, 23);
            this.btnViewConverted.TabIndex = 9;
            this.btnViewConverted.Text = "Vis konvertert";
            this.btnViewConverted.UseVisualStyleBackColor = true;
            this.btnViewConverted.Click += new System.EventHandler(this.btnViewConverted_Click);
            // 
            // cbLanguage
            // 
            this.cbLanguage.DisplayMember = "no";
            this.cbLanguage.FormattingEnabled = true;
            this.cbLanguage.Items.AddRange(new object[] {
            "no",
            "en"});
            this.cbLanguage.Location = new System.Drawing.Point(12, 51);
            this.cbLanguage.Name = "cbLanguage";
            this.cbLanguage.Size = new System.Drawing.Size(121, 21);
            this.cbLanguage.TabIndex = 10;
            // 
            // btnConvertHtag
            // 
            this.btnConvertHtag.Location = new System.Drawing.Point(145, 164);
            this.btnConvertHtag.Name = "btnConvertHtag";
            this.btnConvertHtag.Size = new System.Drawing.Size(134, 23);
            this.btnConvertHtag.TabIndex = 11;
            this.btnConvertHtag.Text = "Konverter H-tag";
            this.btnConvertHtag.UseVisualStyleBackColor = true;
            this.btnConvertHtag.Click += new System.EventHandler(this.btnConvertHtag_Click);
            // 
            // frmConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 279);
            this.Controls.Add(this.btnConvertHtag);
            this.Controls.Add(this.cbLanguage);
            this.Controls.Add(this.btnViewConverted);
            this.Controls.Add(this.btnConvertByContent);
            this.Controls.Add(this.btnConvertToXml);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCheckDoc);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "frmConvert";
            this.Text = "Konverter MS Word dokumenter (.docx)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnCheckDoc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnConvertToXml;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnConvertByContent;
        private System.Windows.Forms.Button btnViewConverted;
        private System.Windows.Forms.ComboBox cbLanguage;
        private System.Windows.Forms.Button btnConvertHtag;
    }
}