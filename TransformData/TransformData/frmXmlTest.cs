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
using System.Diagnostics;

namespace TransformData
{
    public partial class frmXmlTest : Form
    {
        public frmXmlTest()
        {
            InitializeComponent();
        }

        private void GetReferances(XElement e)
        {
            int m = e.Nodes().Count();

            //foreach (XNode n in e.Nodes())
            for (int i = 0; i < m; i++)
            {
                XNode n = e.Nodes().ElementAt(i);
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement test = (XElement)n;
                    if (test.Name.ToString() == "title" 
                        && (test.Attributes("type").Count() != 0 ? test.Attribute("type").Value : "") == "enkeltsaker-tittel")
                    {
                        GetReferances((XElement)n);
                    }
                    else if (!test.Name.ToString().EndsWith("ref") && !(test.Name.ToString() == "title"))
                    {
                        GetReferances((XElement)n);
                    }
                }
                else if (n.NodeType == XmlNodeType.Text)
                {
                    Debug.Print(n.ToString());
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string test = textBox1.Text;
            XElement r = XElement.Parse(test);

            GetReferances(r);
        }

    }
}
