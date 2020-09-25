namespace TransformData
{
    partial class frmBookmarks
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.a4 = new System.Windows.Forms.TextBox();
            this.a3 = new System.Windows.Forms.TextBox();
            this.a2 = new System.Windows.Forms.TextBox();
            this.a1 = new System.Windows.Forms.TextBox();
            this.s4 = new System.Windows.Forms.TextBox();
            this.s3 = new System.Windows.Forms.TextBox();
            this.s2 = new System.Windows.Forms.TextBox();
            this.s1 = new System.Windows.Forms.TextBox();
            this.btnKoble = new System.Windows.Forms.Button();
            this.r4 = new System.Windows.Forms.TextBox();
            this.r3 = new System.Windows.Forms.TextBox();
            this.r2 = new System.Windows.Forms.TextBox();
            this.r1 = new System.Windows.Forms.TextBox();
            this.t4 = new System.Windows.Forms.TextBox();
            this.t3 = new System.Windows.Forms.TextBox();
            this.t2 = new System.Windows.Forms.TextBox();
            this.t1 = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.limInnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.bookmarkOrg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bookmarkReset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topicName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topicBm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topicBmTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.topicBmId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnClose);
            this.splitContainer1.Panel2.Controls.Add(this.btnExport);
            this.splitContainer1.Panel2.Controls.Add(this.a4);
            this.splitContainer1.Panel2.Controls.Add(this.a3);
            this.splitContainer1.Panel2.Controls.Add(this.a2);
            this.splitContainer1.Panel2.Controls.Add(this.a1);
            this.splitContainer1.Panel2.Controls.Add(this.s4);
            this.splitContainer1.Panel2.Controls.Add(this.s3);
            this.splitContainer1.Panel2.Controls.Add(this.s2);
            this.splitContainer1.Panel2.Controls.Add(this.s1);
            this.splitContainer1.Panel2.Controls.Add(this.btnKoble);
            this.splitContainer1.Panel2.Controls.Add(this.r4);
            this.splitContainer1.Panel2.Controls.Add(this.r3);
            this.splitContainer1.Panel2.Controls.Add(this.r2);
            this.splitContainer1.Panel2.Controls.Add(this.r1);
            this.splitContainer1.Panel2.Controls.Add(this.t4);
            this.splitContainer1.Panel2.Controls.Add(this.t3);
            this.splitContainer1.Panel2.Controls.Add(this.t2);
            this.splitContainer1.Panel2.Controls.Add(this.t1);
            this.splitContainer1.Size = new System.Drawing.Size(974, 490);
            this.splitContainer1.SplitterDistance = 667;
            this.splitContainer1.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bookmarkOrg,
            this.bookmarkReset,
            this.topicName,
            this.topicBm,
            this.topicBmTitle,
            this.topicBmId});
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(667, 490);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // a4
            // 
            this.a4.Location = new System.Drawing.Point(186, 90);
            this.a4.Name = "a4";
            this.a4.Size = new System.Drawing.Size(100, 20);
            this.a4.TabIndex = 16;
            // 
            // a3
            // 
            this.a3.Location = new System.Drawing.Point(186, 64);
            this.a3.Name = "a3";
            this.a3.Size = new System.Drawing.Size(100, 20);
            this.a3.TabIndex = 15;
            // 
            // a2
            // 
            this.a2.Location = new System.Drawing.Point(186, 38);
            this.a2.Name = "a2";
            this.a2.Size = new System.Drawing.Size(100, 20);
            this.a2.TabIndex = 14;
            // 
            // a1
            // 
            this.a1.Location = new System.Drawing.Point(186, 12);
            this.a1.Name = "a1";
            this.a1.Size = new System.Drawing.Size(100, 20);
            this.a1.TabIndex = 13;
            // 
            // s4
            // 
            this.s4.Location = new System.Drawing.Point(153, 90);
            this.s4.Name = "s4";
            this.s4.Size = new System.Drawing.Size(27, 20);
            this.s4.TabIndex = 12;
            // 
            // s3
            // 
            this.s3.Location = new System.Drawing.Point(153, 64);
            this.s3.Name = "s3";
            this.s3.Size = new System.Drawing.Size(27, 20);
            this.s3.TabIndex = 11;
            // 
            // s2
            // 
            this.s2.Location = new System.Drawing.Point(153, 38);
            this.s2.Name = "s2";
            this.s2.Size = new System.Drawing.Size(27, 20);
            this.s2.TabIndex = 10;
            // 
            // s1
            // 
            this.s1.Location = new System.Drawing.Point(154, 12);
            this.s1.Name = "s1";
            this.s1.Size = new System.Drawing.Size(26, 20);
            this.s1.TabIndex = 9;
            // 
            // btnKoble
            // 
            this.btnKoble.Location = new System.Drawing.Point(15, 130);
            this.btnKoble.Name = "btnKoble";
            this.btnKoble.Size = new System.Drawing.Size(99, 23);
            this.btnKoble.TabIndex = 8;
            this.btnKoble.Text = "Koble";
            this.btnKoble.UseVisualStyleBackColor = true;
            this.btnKoble.Click += new System.EventHandler(this.btnKoble_Click);
            // 
            // r4
            // 
            this.r4.Location = new System.Drawing.Point(120, 90);
            this.r4.Name = "r4";
            this.r4.Size = new System.Drawing.Size(27, 20);
            this.r4.TabIndex = 7;
            // 
            // r3
            // 
            this.r3.Location = new System.Drawing.Point(120, 64);
            this.r3.Name = "r3";
            this.r3.Size = new System.Drawing.Size(27, 20);
            this.r3.TabIndex = 6;
            // 
            // r2
            // 
            this.r2.Location = new System.Drawing.Point(120, 38);
            this.r2.Name = "r2";
            this.r2.Size = new System.Drawing.Size(27, 20);
            this.r2.TabIndex = 5;
            // 
            // r1
            // 
            this.r1.Location = new System.Drawing.Point(121, 12);
            this.r1.Name = "r1";
            this.r1.Size = new System.Drawing.Size(26, 20);
            this.r1.TabIndex = 4;
            // 
            // t4
            // 
            this.t4.Location = new System.Drawing.Point(14, 90);
            this.t4.Name = "t4";
            this.t4.Size = new System.Drawing.Size(100, 20);
            this.t4.TabIndex = 3;
            // 
            // t3
            // 
            this.t3.Location = new System.Drawing.Point(14, 64);
            this.t3.Name = "t3";
            this.t3.Size = new System.Drawing.Size(100, 20);
            this.t3.TabIndex = 2;
            // 
            // t2
            // 
            this.t2.Location = new System.Drawing.Point(14, 38);
            this.t2.Name = "t2";
            this.t2.Size = new System.Drawing.Size(100, 20);
            this.t2.TabIndex = 1;
            // 
            // t1
            // 
            this.t1.Location = new System.Drawing.Point(14, 12);
            this.t1.Name = "t1";
            this.t1.Size = new System.Drawing.Size(100, 20);
            this.t1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.limInnToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(107, 26);
            // 
            // limInnToolStripMenuItem
            // 
            this.limInnToolStripMenuItem.Name = "limInnToolStripMenuItem";
            this.limInnToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.limInnToolStripMenuItem.Text = "Lim inn";
            this.limInnToolStripMenuItem.Click += new System.EventHandler(this.limInnToolStripMenuItem_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(121, 455);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 17;
            this.btnExport.Text = "Lagre";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(211, 455);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 18;
            this.btnClose.Text = "Lukk";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // bookmarkOrg
            // 
            this.bookmarkOrg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.bookmarkOrg.HeaderText = "Bokmerke";
            this.bookmarkOrg.Name = "bookmarkOrg";
            this.bookmarkOrg.Width = 80;
            // 
            // bookmarkReset
            // 
            this.bookmarkReset.HeaderText = "Resatt";
            this.bookmarkReset.Name = "bookmarkReset";
            // 
            // topicName
            // 
            this.topicName.HeaderText = "Topic";
            this.topicName.Name = "topicName";
            // 
            // topicBm
            // 
            this.topicBm.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.topicBm.HeaderText = "Bm";
            this.topicBm.Name = "topicBm";
            this.topicBm.Width = 47;
            // 
            // topicBmTitle
            // 
            this.topicBmTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.topicBmTitle.HeaderText = "Bokmerke tittle";
            this.topicBmTitle.Name = "topicBmTitle";
            this.topicBmTitle.Width = 102;
            // 
            // topicBmId
            // 
            this.topicBmId.HeaderText = "ID";
            this.topicBmId.Name = "topicBmId";
            // 
            // frmBookmarks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 490);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmBookmarks";
            this.Text = "Form1";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox r4;
        private System.Windows.Forms.TextBox r3;
        private System.Windows.Forms.TextBox r2;
        private System.Windows.Forms.TextBox r1;
        private System.Windows.Forms.TextBox t4;
        private System.Windows.Forms.TextBox t3;
        private System.Windows.Forms.TextBox t2;
        private System.Windows.Forms.TextBox t1;
        private System.Windows.Forms.Button btnKoble;
        private System.Windows.Forms.TextBox a4;
        private System.Windows.Forms.TextBox a3;
        private System.Windows.Forms.TextBox a2;
        private System.Windows.Forms.TextBox a1;
        private System.Windows.Forms.TextBox s4;
        private System.Windows.Forms.TextBox s3;
        private System.Windows.Forms.TextBox s2;
        private System.Windows.Forms.TextBox s1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem limInnToolStripMenuItem;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.DataGridViewTextBoxColumn bookmarkOrg;
        private System.Windows.Forms.DataGridViewTextBoxColumn bookmarkReset;
        private System.Windows.Forms.DataGridViewTextBoxColumn topicName;
        private System.Windows.Forms.DataGridViewTextBoxColumn topicBm;
        private System.Windows.Forms.DataGridViewTextBoxColumn topicBmTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn topicBmId;
    }
}