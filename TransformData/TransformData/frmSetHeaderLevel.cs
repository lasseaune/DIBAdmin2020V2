using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace TransformData
{
    public partial class frmSetHeaderLevel : Form
    {
        public frmSetHeaderLevel(XElement sections)
        {
            InitializeComponent();
            foreach (XElement section in sections.Elements())
            {
                dataGridView1.Rows.Add(section.Attribute("idx").Value, section.Attribute("text").Value, "1");
            }
        }
    }
}
