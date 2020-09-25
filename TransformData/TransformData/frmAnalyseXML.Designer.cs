namespace TransformData
{
    partial class frmAnalyseXML
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.btnSectionLovkom = new System.Windows.Forms.Button();
            this.btnHeaderToHeader = new System.Windows.Forms.Button();
            this.btnHeaderToSection = new System.Windows.Forms.Button();
            this.btnLaw = new System.Windows.Forms.Button();
            this.btnElement = new System.Windows.Forms.Button();
            this.btnViewParent = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnHeaderToTree = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(776, 584);
            this.splitContainer1.SplitterDistance = 580;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.btnHeaderToTree);
            this.splitContainer2.Panel1.Controls.Add(this.btnSectionLovkom);
            this.splitContainer2.Panel1.Controls.Add(this.btnHeaderToHeader);
            this.splitContainer2.Panel1.Controls.Add(this.btnHeaderToSection);
            this.splitContainer2.Panel1.Controls.Add(this.btnLaw);
            this.splitContainer2.Panel1.Controls.Add(this.btnElement);
            this.splitContainer2.Panel1.Controls.Add(this.btnViewParent);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnAbort);
            this.splitContainer2.Panel2.Controls.Add(this.btnSave);
            this.splitContainer2.Size = new System.Drawing.Size(192, 584);
            this.splitContainer2.SplitterDistance = 515;
            this.splitContainer2.TabIndex = 0;
            // 
            // btnSectionLovkom
            // 
            this.btnSectionLovkom.Location = new System.Drawing.Point(3, 218);
            this.btnSectionLovkom.Name = "btnSectionLovkom";
            this.btnSectionLovkom.Size = new System.Drawing.Size(185, 23);
            this.btnSectionLovkom.TabIndex = 5;
            this.btnSectionLovkom.Text = "HX>Lovkom";
            this.btnSectionLovkom.UseVisualStyleBackColor = true;
            this.btnSectionLovkom.Click += new System.EventHandler(this.btnSectionLovkom_Click);
            // 
            // btnHeaderToHeader
            // 
            this.btnHeaderToHeader.Location = new System.Drawing.Point(3, 134);
            this.btnHeaderToHeader.Name = "btnHeaderToHeader";
            this.btnHeaderToHeader.Size = new System.Drawing.Size(185, 23);
            this.btnHeaderToHeader.TabIndex = 4;
            this.btnHeaderToHeader.Text = "H1-9 til overskrift";
            this.btnHeaderToHeader.UseVisualStyleBackColor = true;
            this.btnHeaderToHeader.Click += new System.EventHandler(this.btnHeaderToHeader_Click);
            // 
            // btnHeaderToSection
            // 
            this.btnHeaderToSection.Location = new System.Drawing.Point(3, 106);
            this.btnHeaderToSection.Name = "btnHeaderToSection";
            this.btnHeaderToSection.Size = new System.Drawing.Size(185, 22);
            this.btnHeaderToSection.TabIndex = 3;
            this.btnHeaderToSection.Text = "H1-9 til Section samme nivå";
            this.btnHeaderToSection.UseVisualStyleBackColor = true;
            this.btnHeaderToSection.Click += new System.EventHandler(this.btnHeaderToSection_Click);
            // 
            // btnLaw
            // 
            this.btnLaw.Location = new System.Drawing.Point(4, 259);
            this.btnLaw.Name = "btnLaw";
            this.btnLaw.Size = new System.Drawing.Size(92, 23);
            this.btnLaw.TabIndex = 2;
            this.btnLaw.Text = "Lov";
            this.btnLaw.UseVisualStyleBackColor = true;
            this.btnLaw.Click += new System.EventHandler(this.btnLaw_Click);
            // 
            // btnElement
            // 
            this.btnElement.Location = new System.Drawing.Point(99, 28);
            this.btnElement.Name = "btnElement";
            this.btnElement.Size = new System.Drawing.Size(90, 22);
            this.btnElement.TabIndex = 1;
            this.btnElement.Text = "Vis element";
            this.btnElement.UseVisualStyleBackColor = true;
            this.btnElement.Click += new System.EventHandler(this.btnElement_Click);
            // 
            // btnViewParent
            // 
            this.btnViewParent.Location = new System.Drawing.Point(3, 28);
            this.btnViewParent.Name = "btnViewParent";
            this.btnViewParent.Size = new System.Drawing.Size(96, 23);
            this.btnViewParent.TabIndex = 0;
            this.btnViewParent.Text = "Vis parent";
            this.btnViewParent.UseVisualStyleBackColor = true;
            this.btnViewParent.Click += new System.EventHandler(this.btnViewParent_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(94, 21);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 1;
            this.btnAbort.Text = "Avbryt";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(13, 21);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Lagre";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnHeaderToTree
            // 
            this.btnHeaderToTree.Location = new System.Drawing.Point(4, 77);
            this.btnHeaderToTree.Name = "btnHeaderToTree";
            this.btnHeaderToTree.Size = new System.Drawing.Size(184, 23);
            this.btnHeaderToTree.TabIndex = 6;
            this.btnHeaderToTree.Text = "H1-9 til Section hierarki";
            this.btnHeaderToTree.UseVisualStyleBackColor = true;
            this.btnHeaderToTree.Click += new System.EventHandler(this.btnHeaderToTree_Click);
            // 
            // frmAnalyseXML
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 584);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmAnalyseXML";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Analyser XML";
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnViewParent;
        private System.Windows.Forms.Button btnElement;
        private System.Windows.Forms.Button btnLaw;
        private System.Windows.Forms.Button btnHeaderToSection;
        private System.Windows.Forms.Button btnHeaderToHeader;
        private System.Windows.Forms.Button btnSectionLovkom;
        private System.Windows.Forms.Button btnHeaderToTree;
    }
}