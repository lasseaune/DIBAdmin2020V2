using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using TransformData.Global;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using HtmlAgilityPack;
using System.Net;
using System.Diagnostics;
using DIB.RegExp.ExternalStaticLinks;

namespace TransformData
{
    public partial class frmImportKodeOversikt : Form
    {
        public XElement m_DocumentData = null;
        public frmImportKodeOversikt()
        {
            InitializeComponent();
        }

        private XElement LoadPart(string href, Encoding en)
        {
            XElement html = global.GetWebpageHtml(href, en);// XElement.Parse(shtml);

            if (html.Descendants("a").Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == "kapitteltekst").Count() == 1)
            {
                XElement mark = html.Descendants("a").Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == "kapitteltekst").First();
                html = new XElement("content");
                XNode next =  mark.NextNode;
                while (next != null)
                {
                    if (next.NodeType == XmlNodeType.Element)
                    {
                        XElement test = next as XElement;
                        //if ((test.Attribute("class") == null ? "" : test.Attribute("class").Value) == "maincontent")
                        if (test.Name.LocalName =="div")
                        {
                            html = test;
                            break;
                        }
                    }
                    next = next.NextNode;
                }
            }
            else
            {
                MessageBox.Show("maincontent finnes ikke");
                html = null;
            }
            return html;

        }
        private void btmImport_Click(object sender, EventArgs e)
        {
            XElement html = global.GetWebpageHtml(webadress.Text, Encoding.UTF8);// XElement.Parse(shtml);

            //XElement content = html.Descendants().Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == "ctl00_ctl00_FrameworkRegion_MainRegion_ContentFrameworkRegion_MainRegion_MainContentRegion_KapittelInnholdsfortegnelse1_ChapterContentSiteMap").FirstOrDefault();
            XElement content = html.Descendants().Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == "ctl00_ContentMainWrapper_ContentMain_ChapterTableOfContents1_ChapterContentSiteMap").FirstOrDefault();
            
            if (content != null)
            {
                List<XElement> refs = content.Descendants("a").Where(p => (p.Attribute("href") == null ? "" : p.Attribute("href").Value) != "").ToList();
                if (refs.Count() == 0)
                {
                    MessageBox.Show("Ingen innholdsfortegnelse funnet!"); 
                    return;
                }

                XElement document = new XElement("document");

                foreach (XElement r in refs)
                {
                    string title = r.Nodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate();
                    Uri uri = new Uri(webadress.Text);
                    string href = "http://" + uri.Host +  r.Attribute("href").Value;
                    XElement part = LoadPart(href, Encoding.UTF8);
                    if (part != null)
                    {
                        bool found = true;
                        while (found)
                        {
                            if ((part.Elements().Count() == 1 ? part.Elements().First().Name.LocalName : "") == "div")
                            {
                                part = new XElement(part.Elements().First());
                            }
                            else
                                found = false;
                        }
                        
                        List<XElement> strongs = part.Descendants("strong").ToList();
                        foreach (XElement strong in strongs)
                        {
                            List<XElement> brs = strong.Descendants("br").ToList();
                            foreach (XElement br in brs)
                            {
                                br.ReplaceWith(new XText(" "));
                            }
                        }

                        document.Add(new XElement("section"
                            , new XElement("title", title)
                            , new XElement("text", part.Nodes())
                             )
                            );
                    }
                }
                if (document.HasElements)
                {
                    m_DocumentData = new XElement("documents", document);
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Close();
        }
    }
}
