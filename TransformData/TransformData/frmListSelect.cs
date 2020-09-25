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
    public partial class frmListSelect : Form
    {
        public frmListSelect(List<string> l)
        {
            InitializeComponent();
            foreach (string s in l)
            {
                listBox1.Items.Add(s);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                this.Tag = listBox1.SelectedItem;
                this.Close();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

        }

        private void frmListSelect_Load(object sender, EventArgs e)
        {

        }
    }
}
