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
    public partial class frmXMLAttributesEdit : Form
    {
        public frmXMLAttributesEdit(XElement element)
        {
            InitializeComponent();

            var arr = new TextBox[element.Attributes().Count()];
            var lab_arr = new Label[element.Attributes().Count()];
            for (var i = 0; i < element.Attributes().Count(); i++)
            {
                XAttribute att = element.Attributes().ElementAt(i);
                var tbox = new TextBox();
                var lBox = new Label();
                tbox.Width = this.splitContainer1.Panel1.Width;
                tbox.Text = att.Value;
                tbox.Tag = att.Name.LocalName;
                lBox.Text = att.Name.LocalName;
                // Other properties sets for tbox

                tbox.Width = this.splitContainer1.Panel1.Width;
                if (i == 0)
                {
                    lBox.Top = 0;
                    tbox.Top = (lBox.Top + lBox.Height) + 2;
                }
                else
                {
                    lBox.Top = (arr[i - 1].Top + arr[i - 1].Size.Height) + 2;
                    tbox.Top = (lBox.Top + lBox.Height) + 2;
                }
                this.splitContainer1.Panel1.Controls.Add(lBox);
                this.splitContainer1.Panel1.Controls.Add(tbox);
                arr[i] = tbox;
                lab_arr[i] = lBox;
            }
        }
    }
}
