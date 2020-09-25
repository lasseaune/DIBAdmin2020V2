namespace TransformData
{
    partial class frmImportLD
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
            this.btnInternal = new System.Windows.Forms.Button();
            this.cbFirstLevelText = new System.Windows.Forms.CheckBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.btnsearchList = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnTOC = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.cebAsIs = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.adress = new System.Windows.Forms.TextBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnShowHtml = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnGoToWeb = new System.Windows.Forms.Button();
            this.btnLoadDocument = new System.Windows.Forms.Button();
            this.textAdress = new System.Windows.Forms.ComboBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnNavigate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnInternal);
            this.splitContainer1.Panel1.Controls.Add(this.cbFirstLevelText);
            this.splitContainer1.Panel1.Controls.Add(this.btnGo);
            this.splitContainer1.Panel1.Controls.Add(this.btnsearchList);
            this.splitContainer1.Panel1.Controls.Add(this.textBox1);
            this.splitContainer1.Panel1.Controls.Add(this.btnTOC);
            this.splitContainer1.Panel1.Controls.Add(this.btnConnect);
            this.splitContainer1.Panel1.Controls.Add(this.cebAsIs);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.adress);
            this.splitContainer1.Panel1.Controls.Add(this.btnExport);
            this.splitContainer1.Panel1.Controls.Add(this.btnShowHtml);
            this.splitContainer1.Panel1.Controls.Add(this.btnConvert);
            this.splitContainer1.Panel1.Controls.Add(this.btnGoToWeb);
            this.splitContainer1.Panel1.Controls.Add(this.btnLoadDocument);
            this.splitContainer1.Panel1.Controls.Add(this.textAdress);
            this.splitContainer1.Panel1.Controls.Add(this.btnSearch);
            this.splitContainer1.Panel1.Controls.Add(this.btnNavigate);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(865, 626);
            this.splitContainer1.SplitterDistance = 101;
            this.splitContainer1.TabIndex = 1;
            // 
            // btnInternal
            // 
            this.btnInternal.Location = new System.Drawing.Point(530, 62);
            this.btnInternal.Name = "btnInternal";
            this.btnInternal.Size = new System.Drawing.Size(53, 22);
            this.btnInternal.TabIndex = 20;
            this.btnInternal.Text = "Interne";
            this.btnInternal.UseVisualStyleBackColor = true;
            this.btnInternal.Click += new System.EventHandler(this.btnInternal_Click);
            // 
            // cbFirstLevelText
            // 
            this.cbFirstLevelText.AutoSize = true;
            this.cbFirstLevelText.Location = new System.Drawing.Point(678, 85);
            this.cbFirstLevelText.Name = "cbFirstLevelText";
            this.cbFirstLevelText.Size = new System.Drawing.Size(98, 17);
            this.cbFirstLevelText.TabIndex = 19;
            this.cbFirstLevelText.Text = "Første nivå text";
            this.cbFirstLevelText.UseVisualStyleBackColor = true;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(815, 36);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(37, 19);
            this.btnGo.TabIndex = 18;
            this.btnGo.Text = "GO";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // btnsearchList
            // 
            this.btnsearchList.Location = new System.Drawing.Point(814, 8);
            this.btnsearchList.Name = "btnsearchList";
            this.btnsearchList.Size = new System.Drawing.Size(39, 20);
            this.btnsearchList.TabIndex = 2;
            this.btnsearchList.Text = "...";
            this.btnsearchList.UseVisualStyleBackColor = true;
            this.btnsearchList.Click += new System.EventHandler(this.btnsearchList_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(719, 9);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(87, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnTOC
            // 
            this.btnTOC.Location = new System.Drawing.Point(207, 62);
            this.btnTOC.Name = "btnTOC";
            this.btnTOC.Size = new System.Drawing.Size(56, 22);
            this.btnTOC.TabIndex = 15;
            this.btnTOC.Text = "TOC";
            this.btnTOC.UseVisualStyleBackColor = true;
            this.btnTOC.Click += new System.EventHandler(this.btnTOC_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(467, 62);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(57, 22);
            this.btnConnect.TabIndex = 14;
            this.btnConnect.Text = "Koble";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // cebAsIs
            // 
            this.cebAsIs.AutoSize = true;
            this.cebAsIs.Location = new System.Drawing.Point(678, 64);
            this.cebAsIs.Name = "cebAsIs";
            this.cebAsIs.Size = new System.Drawing.Size(84, 17);
            this.cebAsIs.TabIndex = 13;
            this.cebAsIs.Text = "Enkel import";
            this.cebAsIs.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Adresse";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Dokument";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // adress
            // 
            this.adress.Location = new System.Drawing.Point(81, 36);
            this.adress.Name = "adress";
            this.adress.Size = new System.Drawing.Size(684, 20);
            this.adress.TabIndex = 10;
            this.adress.TextChanged += new System.EventHandler(this.adress_TextChanged);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(401, 62);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(60, 22);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "Eksporter";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnShowHtml
            // 
            this.btnShowHtml.Location = new System.Drawing.Point(335, 62);
            this.btnShowHtml.Name = "btnShowHtml";
            this.btnShowHtml.Size = new System.Drawing.Size(60, 22);
            this.btnShowHtml.TabIndex = 8;
            this.btnShowHtml.Text = "Vis html";
            this.btnShowHtml.UseVisualStyleBackColor = true;
            this.btnShowHtml.Click += new System.EventHandler(this.btnShowHtml_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(269, 62);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(60, 22);
            this.btnConvert.TabIndex = 7;
            this.btnConvert.Text = "Konverter";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnGoToWeb
            // 
            this.btnGoToWeb.Location = new System.Drawing.Point(81, 62);
            this.btnGoToWeb.Name = "btnGoToWeb";
            this.btnGoToWeb.Size = new System.Drawing.Size(55, 22);
            this.btnGoToWeb.TabIndex = 6;
            this.btnGoToWeb.Text = "Gå til web";
            this.btnGoToWeb.UseVisualStyleBackColor = true;
            this.btnGoToWeb.Click += new System.EventHandler(this.btnGoToWeb_Click);
            // 
            // btnLoadDocument
            // 
            this.btnLoadDocument.Location = new System.Drawing.Point(142, 62);
            this.btnLoadDocument.Name = "btnLoadDocument";
            this.btnLoadDocument.Size = new System.Drawing.Size(59, 22);
            this.btnLoadDocument.TabIndex = 5;
            this.btnLoadDocument.Text = "Last ned";
            this.btnLoadDocument.UseVisualStyleBackColor = true;
            this.btnLoadDocument.Click += new System.EventHandler(this.btnLoadDocument_Click);
            // 
            // textAdress
            // 
            this.textAdress.FormattingEnabled = true;
            this.textAdress.Location = new System.Drawing.Point(81, 10);
            this.textAdress.MaxDropDownItems = 30;
            this.textAdress.Name = "textAdress";
            this.textAdress.Size = new System.Drawing.Size(623, 21);
            this.textAdress.TabIndex = 4;
            this.textAdress.SelectedIndexChanged += new System.EventHandler(this.textAdress_SelectedIndexChanged);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(771, 61);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(36, 20);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Søk";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnNavigate
            // 
            this.btnNavigate.Location = new System.Drawing.Point(771, 36);
            this.btnNavigate.Name = "btnNavigate";
            this.btnNavigate.Size = new System.Drawing.Size(36, 19);
            this.btnNavigate.TabIndex = 1;
            this.btnNavigate.Text = "...";
            this.btnNavigate.UseVisualStyleBackColor = true;
            this.btnNavigate.Click += new System.EventHandler(this.btnNavigate_Click);
            // 
            // frmImportLD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 626);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmImportLD";
            this.Text = "Importer fra Lovdata";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnNavigate;
        private System.Windows.Forms.ComboBox textAdress;
        private System.Windows.Forms.Button btnLoadDocument;
        private System.Windows.Forms.Button btnGoToWeb;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnShowHtml;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox adress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cebAsIs;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnTOC;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnsearchList;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.CheckBox cbFirstLevelText;
        private System.Windows.Forms.Button btnInternal;
    }
}