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
using DIB.BuildHeaderHierachy;
using System.Text.RegularExpressions;
using DIB.RegExp.ExternalStaticLinks;

namespace TransformData
{
    public partial class frmAnalyseXML : Form
    {
        public XElement XML = null;
        private XElement _part = null;
        private XElement _parent = null;
        private XmlEditor myXMLEditor;
        public frmAnalyseXML(XElement part)
        {
            InitializeComponent();

            CreateXMLEditor();
            myXMLEditor.AllowXmlFormatting = true;
            _part = part;
            myXMLEditor.Text = part.ToString();

        }

        private void CreateXMLEditor()
        {
            myXMLEditor = new XmlEditor();
            splitContainer1.Panel1.Controls.Add(myXMLEditor);
            myXMLEditor.Dock = DockStyle.Fill;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                XML = XElement.Parse(myXMLEditor.Text);
                if (XML.Attribute("inspected") == null)
                {
                    XAttribute a = new XAttribute("inspected", "OK");
                    XML.Add(a);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SystemException err)
            {
                MessageBox.Show("Kunne ikke parse XML!");
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        private void btnViewParent_Click(object sender, EventArgs e)
        {
            if (_parent == null)
            {
                myXMLEditor.Text = _part.Parent.ToString();
                _parent = _part.Parent;
            }
            else
            {
                if (_parent.Parent != null)
                {
                    myXMLEditor.Text = _parent.Parent.ToString();
                    _parent = _parent.Parent;
                }
            }
        }

        private void btnElement_Click(object sender, EventArgs e)
        {
            myXMLEditor.Text = _part.ToString();
            _parent = null;
        }

        private void btnLaw_Click(object sender, EventArgs e)
        {
            XElement part = XElement.Parse(myXMLEditor.Text);

            XElement toc = null;
            List<XElement> l = part
                            .Descendants()
                            .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")
                                && p.Value.Trim() != "")

                            .ToList();

            if (l.Count() > 0)
            {
                int minPara = 100;
                int maxPara = 0;
                BuildHeaderHierachy bh = new BuildHeaderHierachy(minPara, maxPara, false);
                bh._v0_GetHeaderType(l);

                XElement container = new XElement("container");
                toc = bh.GetIndexBranch(l, true, container);
                myXMLEditor.Text = toc.ToString();

            }
        }

        private void btnHeaderToSection_Click(object sender, EventArgs e)
        {
            XElement newS = new XElement("section");
            XElement section = XElement.Parse(myXMLEditor.Text);
            XElement currS = null;
            foreach (XElement el in section.Elements())
            {
                if (el.Name.LocalName == "title")
                {
                    newS.Add(new XElement(el));
                }
                else if (Regex.IsMatch(el.Name.LocalName, @"h\d"))
                {
                    currS = new XElement("section", el.Attribute("idx"));
                    currS.Add(new XElement("title", el.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate())); 
                    newS.Add(currS);
                }
                else
                {
                    if (currS != null)
                        currS.Add(el);
                    else
                        newS.Add(el);

                }
            }
            section.Elements().Remove();
            section.Add(newS.Nodes());

            myXMLEditor.Text = section.ToString();
        }

        private void btnHeaderToHeader_Click(object sender, EventArgs e)
        {
            XElement newS = new XElement("section");
            XElement section = XElement.Parse(myXMLEditor.Text);
            XElement currS = null;
            foreach (XElement el in section.Elements())
            {
                if (Regex.IsMatch(el.Name.LocalName, @"h\d"))
                {
                    newS.Add(new XElement("mtit",el.Attribute("idx") , el.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate()));
                }
                else
                {
                    if (currS != null)
                        currS.Add(el);
                    else
                        newS.Add(el);

                }
            }
            section.Elements().Remove();
            section.Add(newS.Nodes());

            myXMLEditor.Text = section.ToString();

        }

        private void btnSectionLovkom_Click(object sender, EventArgs e)
        {
            XElement newS = new XElement("sections");
            XElement section = XElement.Parse(myXMLEditor.Text);

            int n = section.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-DEL-") != -1).Count();

            for (int i = n; i > 0; i--)
            {
                XElement div = section.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-DEL-") != -1).ElementAt(i - 1);
                div.ReplaceWith(div.Nodes());
            }

            foreach (XElement el in section.Elements())
            {
                if (Regex.IsMatch(el.Name.LocalName, @"h\d"))
                {
                    newS.Add(new XElement("section"
                        , el.Attribute("idx")
                        , new XAttribute("text",el.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate())
                        )); 
                }
            }
            frmSetHeaderLevel f = new frmSetHeaderLevel(newS);
            f.ShowDialog();


        }

        private void btnHeaderToTree_Click(object sender, EventArgs e)
        {
            XElement section = XElement.Parse(myXMLEditor.Text);
            
            //finn lavest header nivå
            HtmlHierarchy h = new HtmlHierarchy();
            int counter = 1;
            h.SetElementId(section, ref counter);

            XElement toc = h.GetHtmlToc(section);
            XElement newS = new XElement(section.Name.LocalName, section.Attributes());

            newS.Add(section.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(toc.Elements("section").First().Attribute("idx").Value)));
            newS.Add(toc.Nodes());
            int n = newS.Descendants("section").Count();

            for (int i = 0; i < n; i ++)
            {
                XElement el = newS.Descendants("section").ElementAt(i);

                XElement next = null;
                if (i+1 < n)
                    next = toc.Descendants("section").ElementAt(i + 1);

                if (next != null)
                {
                    el.Element("title").AddAfterSelf(section.Elements().Where(p => 
                        Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(el.Attribute("idx").Value)
                        &&
                        Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("idx").Value)
                        ));
                }
                else
                {
                    el.Element("title").AddAfterSelf(section.Elements().Where(p =>
                        Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(el.Attribute("idx").Value)
                    ));

                }
            }
            myXMLEditor.Text = newS.ToString();
        }
    }
}
