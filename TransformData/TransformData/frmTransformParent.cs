using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TransformData
{
    public partial class frmTransformParent : Form
    {
        private int childFormNumber = 0;

        public frmTransformParent()
        {
            InitializeComponent();
            Form newMDIChild = new frmSectionEdit();
            // Set the Parent Form of the Child window.
            newMDIChild.Text = "Rediger dokument";
            newMDIChild.MdiParent = this;
            // Display the new form.
            newMDIChild.Show();

            //Form newMDIChild = new frmHtmlImport(ref toolStripProgressBar1, ref toolStripStatusLabel);
            //// Set the Parent Form of the Child window.
            //newMDIChild.MdiParent = this;
            //// Display the new form.
            //newMDIChild.Show();
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            Form childForm = new Form();
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void referanserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

        }

        private void referanserToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form newMDIChild = new frmReferance();
            // Set the Parent Form of the Child window.
            newMDIChild.MdiParent = this;
            // Display the new form.
            newMDIChild.Show();
        }

        private void analyserDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form newMDIChild = new frmHtmlImport(ref toolStripProgressBar1, ref toolStripStatusLabel);
            // Set the Parent Form of the Child window.
            newMDIChild.MdiParent = this;
            // Display the new form.
            newMDIChild.Show();
        }

        private void importerLovdataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form newMDIChild = new frmImportLD();
            // Set the Parent Form of the Child window.
            newMDIChild.MdiParent = this;
            // Display the new form.
            newMDIChild.Show();
        }

        private void toolStripStatusLabel_Click(object sender, EventArgs e)
        {

        }

        private void kobleMetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new frmConnectDocuments();
            f.Show();
        }

        private void redigerDokumentToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Form newMDIChild = new frmSectionEdit();
            // Set the Parent Form of the Child window.
            newMDIChild.Text = "Rediger dokument";
            newMDIChild.MdiParent = this;
            // Display the new form.
            newMDIChild.Show();
        }

        private void konverterWorddokumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new frmConvert();
            f.Show();

        }

        private void regExpEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new frmRegexEditor();
            f.Show();

        }

        private void kodeoversiktToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new frmImportKodeOversikt();
            f.Show();

        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new frmTest();
            f.Show();
        }
    }
}
