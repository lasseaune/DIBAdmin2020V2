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
using TransformData.Global;
using DIB.RegExp.ExternalStaticLinks;
using System.Diagnostics;

namespace TransformData
{
    public partial class frmReadDocument : Form
    {
        public frmReadDocument()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            XElement doc = XElement.Load(@"C:\_Work\2009\Mva\2012\output\xml\document.xml");

            foreach (XElement c in doc.Descendants().Where(p => "/documents/document/section/level/".IndexOf("/" + p.Name.LocalName + "/") != -1))
            {

                string title = c.Elements()
                                .Where(p => p.Name.LocalName == "title")
                                .DescendantNodes()
                                .OfType<XText>()
                                .Select(p =>  (string)p.ToString() + " ")
                                .StringConcatenate();

                string text =  c.Elements()
                                .Where(p => "/documents/document/section/title/level/contentbox/".IndexOf("/" + p.Name.LocalName + "/") == -1)
                                .DescendantNodes()
                                .OfType<XText>()
                                .Select(p =>  (string)p.ToString() + " ")
                                .StringConcatenate();

                Debug.Print("Tittel: " + title);
                Debug.Print("Text: " + text);
            }
        }
    }
}
