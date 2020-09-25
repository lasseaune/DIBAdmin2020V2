namespace TransformData
{
    partial class frmRegexEditor
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.tbActionsFileName = new System.Windows.Forms.TextBox();
            this.btnBuild = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cbListOfRegExp = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.filToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.åpneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.åpneTotalRegexpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lagreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.åpneAksjonsfillToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redigerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validerFilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.twRegexps = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.endreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.egenskaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.tbActionsFileName);
            this.splitContainer1.Panel1.Controls.Add(this.btnBuild);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.cbListOfRegExp);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.tbFileName);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(917, 662);
            this.splitContainer1.SplitterDistance = 126;
            this.splitContainer1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Actions";
            // 
            // tbActionsFileName
            // 
            this.tbActionsFileName.Location = new System.Drawing.Point(70, 57);
            this.tbActionsFileName.Name = "tbActionsFileName";
            this.tbActionsFileName.Size = new System.Drawing.Size(812, 20);
            this.tbActionsFileName.TabIndex = 15;
            // 
            // btnBuild
            // 
            this.btnBuild.Location = new System.Drawing.Point(209, 87);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(75, 23);
            this.btnBuild.TabIndex = 14;
            this.btnBuild.Text = "Bygg";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Start regex:";
            // 
            // cbListOfRegExp
            // 
            this.cbListOfRegExp.FormattingEnabled = true;
            this.cbListOfRegExp.Location = new System.Drawing.Point(70, 87);
            this.cbListOfRegExp.Name = "cbListOfRegExp";
            this.cbListOfRegExp.Size = new System.Drawing.Size(133, 21);
            this.cbListOfRegExp.TabIndex = 12;
            this.cbListOfRegExp.SelectedIndexChanged += new System.EventHandler(this.cbListOfRegExp_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filnavn:";
            // 
            // tbFileName
            // 
            this.tbFileName.Enabled = false;
            this.tbFileName.Location = new System.Drawing.Point(70, 31);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(812, 20);
            this.tbFileName.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filToolStripMenuItem,
            this.redigerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(917, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // filToolStripMenuItem
            // 
            this.filToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.åpneToolStripMenuItem,
            this.åpneTotalRegexpToolStripMenuItem,
            this.lagreToolStripMenuItem,
            this.åpneAksjonsfillToolStripMenuItem});
            this.filToolStripMenuItem.Name = "filToolStripMenuItem";
            this.filToolStripMenuItem.Size = new System.Drawing.Size(31, 20);
            this.filToolStripMenuItem.Text = "Fil";
            // 
            // åpneToolStripMenuItem
            // 
            this.åpneToolStripMenuItem.Name = "åpneToolStripMenuItem";
            this.åpneToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.åpneToolStripMenuItem.Text = "Åpne";
            this.åpneToolStripMenuItem.Click += new System.EventHandler(this.åpneToolStripMenuItem_Click);
            // 
            // åpneTotalRegexpToolStripMenuItem
            // 
            this.åpneTotalRegexpToolStripMenuItem.Name = "åpneTotalRegexpToolStripMenuItem";
            this.åpneTotalRegexpToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.åpneTotalRegexpToolStripMenuItem.Text = "Åpne total Regexp";
            this.åpneTotalRegexpToolStripMenuItem.Click += new System.EventHandler(this.åpneTotalRegexpToolStripMenuItem_Click);
            // 
            // lagreToolStripMenuItem
            // 
            this.lagreToolStripMenuItem.Name = "lagreToolStripMenuItem";
            this.lagreToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.lagreToolStripMenuItem.Text = "Lagre";
            this.lagreToolStripMenuItem.Click += new System.EventHandler(this.lagreToolStripMenuItem_Click);
            // 
            // åpneAksjonsfillToolStripMenuItem
            // 
            this.åpneAksjonsfillToolStripMenuItem.Name = "åpneAksjonsfillToolStripMenuItem";
            this.åpneAksjonsfillToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.åpneAksjonsfillToolStripMenuItem.Text = "Åpne Aksjonsfill";
            this.åpneAksjonsfillToolStripMenuItem.Click += new System.EventHandler(this.åpneAksjonsfillToolStripMenuItem_Click);
            // 
            // redigerToolStripMenuItem
            // 
            this.redigerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validerFilToolStripMenuItem});
            this.redigerToolStripMenuItem.Name = "redigerToolStripMenuItem";
            this.redigerToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.redigerToolStripMenuItem.Text = "Rediger";
            // 
            // validerFilToolStripMenuItem
            // 
            this.validerFilToolStripMenuItem.Name = "validerFilToolStripMenuItem";
            this.validerFilToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.validerFilToolStripMenuItem.Text = "Valider fil";
            this.validerFilToolStripMenuItem.Click += new System.EventHandler(this.validerFilToolStripMenuItem_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.twRegexps);
            this.splitContainer2.Size = new System.Drawing.Size(917, 532);
            this.splitContainer2.SplitterDistance = 283;
            this.splitContainer2.TabIndex = 0;
            // 
            // twRegexps
            // 
            this.twRegexps.ContextMenuStrip = this.contextMenuStrip1;
            this.twRegexps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.twRegexps.Location = new System.Drawing.Point(0, 0);
            this.twRegexps.Name = "twRegexps";
            this.twRegexps.Size = new System.Drawing.Size(283, 532);
            this.twRegexps.TabIndex = 0;
            this.twRegexps.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.twRegexps_AfterSelect);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nyToolStripMenuItem,
            this.endreToolStripMenuItem,
            this.egenskaperToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(135, 70);
            // 
            // nyToolStripMenuItem
            // 
            this.nyToolStripMenuItem.Name = "nyToolStripMenuItem";
            this.nyToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.nyToolStripMenuItem.Text = "Ny";
            // 
            // endreToolStripMenuItem
            // 
            this.endreToolStripMenuItem.Name = "endreToolStripMenuItem";
            this.endreToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.endreToolStripMenuItem.Text = "Endre";
            // 
            // egenskaperToolStripMenuItem
            // 
            this.egenskaperToolStripMenuItem.Name = "egenskaperToolStripMenuItem";
            this.egenskaperToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.egenskaperToolStripMenuItem.Text = "Egenskaper";
            this.egenskaperToolStripMenuItem.Click += new System.EventHandler(this.egenskaperToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // frmRegexEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 662);
            this.Controls.Add(this.splitContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmRegexEditor";
            this.Text = "frmRegexEditor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem filToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem åpneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lagreToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView twRegexps;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.ToolStripMenuItem redigerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validerFilToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbListOfRegExp;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbActionsFileName;
        private System.Windows.Forms.ToolStripMenuItem åpneAksjonsfillToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem åpneTotalRegexpToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem nyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem endreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem egenskaperToolStripMenuItem;
    }
}