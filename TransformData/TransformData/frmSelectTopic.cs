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
    
    public partial class frmSelectTopic : Form
    {


        public frmSelectTopic(List<string> topics)
        {
            InitializeComponent();
            foreach (string t in topics)
            {
                listBox1.Items.Add(t);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
                this.Tag = listBox1.SelectedItem.ToString();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.Close();
            DialogResult = DialogResult.Cancel;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
