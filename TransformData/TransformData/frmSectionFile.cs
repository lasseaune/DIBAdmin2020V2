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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace TransformData
{
    public partial class frmSectionFile : Form
    {
        public frmSectionFile()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "XML-filer (*.xml)|*.xml";
            if (of.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = of.FileName;
            }
        }

        private class docElement
        {
            public XElement p { get; set; }
            public string text = "";
            public string t1 = "";
            public string t2 = "";
            public string t3 = "";
            public string t4 = "";
            public string t5 = "";
            public int n1 = 0;
            public int n2 = 0;
            public int n3 = 0;
            public int n4 = 0;
            public int n5 = 0;
            public string test = "";
            public string hx = "";
            public docElement(XElement e)
            {
                p = e;
                string regExp = @"^"
                    + @"(?<n1>(\d+))"
                    + @"("
                    + @"((\.)(?<n2>(\d+)))"
                    + @"((\.)(?<n3>(\d+)))?"
                    + @"((\.)(?<n4>(\d+)))?"
                    + @"((\.)(?<n5>(\d+)))?"
                    + @")?"
                    + @"(\.)?"
                    + @"(\s)";

                text = p.Nodes().OfType<XText>().First().ToString().TrimStart();
                Match m = Regex.Match(text, regExp);
                t1 = m.Groups["n1"].Value;
                t2 = m.Groups["n2"].Value;
                t3 = m.Groups["n3"].Value;
                t4 = m.Groups["n4"].Value;
                t5 = m.Groups["n5"].Value;

                if (t1 != "") { n1 = Convert.ToInt32(t1); hx = "1"; }
                if (t2 != "") { n2 = Convert.ToInt32(t2); hx = "2"; }
                if (t3 != "") { n3 = Convert.ToInt32(t3); hx = "3"; }
                if (t4 != "") { n4 = Convert.ToInt32(t4); hx = "4"; }
                if (t5 != "") { n5 = Convert.ToInt32(t5); hx = "5"; }

                test = t1.PadLeft(5, '0') + "."
                        + t2.PadLeft(5, '0') + "."
                        + t3.PadLeft(5, '0') + "."
                        + t4.PadLeft(5, '0') + "."
                        + t5.PadLeft(5, '0') + ".";
            }
        }

        private void btnElevateNumber_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                FileInfo fi = new FileInfo(textBox1.Text);
                XElement d = XElement.Load(textBox1.Text);
                List<docElement> index = d
                       .Elements("p")
                       .Where(p => (
                              (p.Nodes().Count() != 0 ? p.Nodes().First().NodeType : XmlNodeType.None) == XmlNodeType.Text)
                           && (p.Nodes().Count() != 0 ? Regex.IsMatch(p.Nodes().First().ToString().TrimStart(), @"^((\d+(\.)?\s)|\d+(\.\d+)+(\.)?\s)") : false)
                           )
                       .Select(p => new docElement(p))
                       .ToList();

                string current = "";
                int nl1 = 0;
                bool bTitle = false;
                int n1 = 0;
                int n2 = 0;
                int n3 = 0;
                int n4 = 0;
                int n5 = 0;

                for (int i = 0; i < index.Count; i++)
                {
                    bTitle = false; 
                    docElement p = index.ElementAt(i);
                    if (p.t1 != "" && p.t2 == "")
                    {
                        if (p.t1 == "1" && n1 == 0)
                        {
                            n1 = 1;
                            current = p.test;
                            bTitle = true;
                            nl1 = 0;
                            p.p.ReplaceWith(new XElement("h1", p.p.Nodes()));
                        }
                        else if (p.t1 == "1")
                        {
                            nl1 = 1;
                        }
                        else if (p.n1 - 1 == nl1 && p.n1 - 1 == n1)
                        {
                            if (i + 1 < index.Count)
                            {
                                docElement next = index.ElementAt(i + 1);
                                if (next.t2 == "" && (next.n1 - 1) == p.n1)
                                {
                                    nl1 = nl1 + 1;
                                }
                                else
                                {
                                    n1 = p.n1;
                                    current = p.test;
                                    bTitle = true;
                                    nl1 = 0;
                                    p.p.ReplaceWith(new XElement("h1", p.p.Nodes()));
                                }
                            }
                        }
                        else if (p.n1 - 1 == nl1)
                        {
                            nl1 = nl1 + 1;
                        }
                        else if (p.n1 - 1 == n1)
                        {
                            n1 = p.n1;
                            current = p.test;
                            bTitle = true;
                            nl1 = 0;
                            p.p.ReplaceWith(new XElement("h1", p.p.Nodes()));
                        }
                        else
                        {
                            MessageBox.Show("Her kan det være en feil!");
                        }
                    }
                    else if (p.t1 != "" && p.t2 != "")
                    {
                        if (p.t1 != "") { n1 = Convert.ToInt32(p.t1); }
                        if (p.t2 != "") { n2 = Convert.ToInt32(p.t2); }
                        if (p.t3 != "") { n3 = Convert.ToInt32(p.t3);}
                        if (p.t4 != "") { n4 = Convert.ToInt32(p.t4); }
                        if (p.t5 != "") { n5 = Convert.ToInt32(p.t5); }

                        if (string.CompareOrdinal(p.test, current) > 0)
                        {
                            current = p.test;
                            bTitle = true;
                            nl1 = 0;

                            p.p.ReplaceWith(new XElement("h" + p.hx, p.p.Nodes()));
                        }
                        else
                        {
                            MessageBox.Show("Her kan det være en feil!");
                        }

                    }
                    else
                    {
                        MessageBox.Show("Her kan det være en feil!");
                    }

                    if (bTitle)
                    {
                        Debug.Print(p.text);
                    }
                }
                d.Save(fi.Directory + @"\" + fi.Name.Replace(fi.Extension, "") + "_hx.xml");
            }
        }
    }
}
