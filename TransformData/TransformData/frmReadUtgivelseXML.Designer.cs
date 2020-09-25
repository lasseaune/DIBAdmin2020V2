namespace TransformData
{
    partial class frmReadUtgivelseXML
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
            this.btnFileName = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnSaveXML = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnKommentarutg = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnFileName
            // 
            this.btnFileName.Location = new System.Drawing.Point(482, 25);
            this.btnFileName.Name = "btnFileName";
            this.btnFileName.Size = new System.Drawing.Size(32, 20);
            this.btnFileName.TabIndex = 5;
            this.btnFileName.Text = "...";
            this.btnFileName.UseVisualStyleBackColor = true;
            this.btnFileName.Click += new System.EventHandler(this.btnFileName_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Utgivelse XML-fil navn:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(464, 20);
            this.textBox1.TabIndex = 3;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnSaveXML
            // 
            this.btnSaveXML.Location = new System.Drawing.Point(12, 231);
            this.btnSaveXML.Name = "btnSaveXML";
            this.btnSaveXML.Size = new System.Drawing.Size(75, 23);
            this.btnSaveXML.TabIndex = 6;
            this.btnSaveXML.Text = "Lagre XML";
            this.btnSaveXML.UseVisualStyleBackColor = true;
            this.btnSaveXML.Click += new System.EventHandler(this.btnSaveXML_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(93, 231);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(82, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Transform";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnKommentarutg
            // 
            this.btnKommentarutg.Location = new System.Drawing.Point(182, 231);
            this.btnKommentarutg.Name = "btnKommentarutg";
            this.btnKommentarutg.Size = new System.Drawing.Size(123, 23);
            this.btnKommentarutg.TabIndex = 9;
            this.btnKommentarutg.Text = "Lag Kommentarutgave";
            this.btnKommentarutg.UseVisualStyleBackColor = true;
            this.btnKommentarutg.Click += new System.EventHandler(this.btnKommentarutg_Click);
            // 
            // frmReadUtgivelseXML
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 266);
            this.Controls.Add(this.btnKommentarutg);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnSaveXML);
            this.Controls.Add(this.btnFileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Name = "frmReadUtgivelseXML";
            this.Text = "frmReadUtgivelseXML";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnSaveXML;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnKommentarutg;
    }
}