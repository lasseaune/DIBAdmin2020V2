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
using DIB.BuildHeaderHierachy;
using System.Text.RegularExpressions;
using DIB.RegExp.ExternalStaticLinks;
using DIB.InTextLinking;




namespace TransformData
{
    public partial class frmImportLov : Form
    {
        public XElement m_DocumentData = null;
        private Regex rxHeaderTest = null;
        public frmImportLov()
        {
            InitializeComponent();
            LoadRegexp();
            if (GlobalVar._LawList != null)
            {
                LoadComboBox();
            }
            else if (File.Exists(global.m_LawsFile))
            {
                GlobalVar._LawList = new LawList(global.m_LawsFile);
                LoadComboBox();
            }
            else
            {
                lawName.Enabled = false;
            }

            lawName.KeyUp +=new KeyEventHandler(lawName_KeyUp);
        }

        private void LoadRegexp()
        {
            if (File.Exists(global.m_LawsFileRegexp))
            {
                XElement regexps = XElement.Load(global.m_LawsFileRegexp);

                string test = regexps.RegexBuild("rxHeader");
                try
                {
                    rxHeaderTest = new Regex(test, RegexOptions.IgnoreCase);
                }
                catch
                {
                    MessageBox.Show("En feil oppstod ved bygging av header regexp! Kan ikke importerer dokumenter.");
                }
            }
        }

        private void lawName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return) return;
            if (lawName.Text.Length < 3) return;
            
            lawName.Enabled = false;
            string search = lawName.Text;
            LoadComboBox(search);
            lawName.Enabled = true;
            lawName.Focus();
            lawName.Text = search;
            lawName.SelectionStart = search.Length;

        }

        private class Document
        {
            public string name;
            public string folder;
            public string adress;
            public string topicId;
            public int max;
            public Document(
                      string folder
                    , string name
                    , string adress
                    , string topicId
                )
            {
                this.folder = folder;
                if (this.folder.Length > max)
                    max = this.folder.Length;
                this.name = name;
                this.adress = adress;
                this.topicId = topicId;
            }
            public override string ToString()
            {
                string returnValue = "(" + this.folder + ")";
                returnValue = returnValue.PadRight(23,' ');
                returnValue = returnValue + this.name;
                return returnValue;
            }
        }

        private void LoadComboBox(string search)
        {

            if ((GlobalVar._LawList == null ? 0 : GlobalVar._LawList.Laws.Count) == 0) return;
            
            {
                lawName.Items.Clear();
                List<lawObject> laws = GlobalVar._LawList.GetList(search);
                
                foreach (lawObject l in laws)
                {
                    lawName.Items.Add(l);
                }
            }
            if (lawName.Items.Count != 0)
            {
                lawName.DroppedDown = true;
            }
        }
        private void LoadComboBox()
        {
            lawName.Items.Clear();
            if (GlobalVar._LawList != null)
            {
                foreach (lawObject l in GlobalVar._LawList.Laws)
                {
                    lawName.Items.Add(l);
                }
            }
            
        }
        private void lawName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lawName.SelectedItem != null)
            {
                if (lawName.SelectedItem.GetType() == typeof(lawObject))
                {
                    lawObject l = lawName.SelectedItem as lawObject;
                    if (l.importhref != null)
                    {
                        webAdress.Text = l.importhref;
                    }
                    else
                    {
                        webAdress.Text = l.href;
                    }
                    webBrowser1.Navigate(webAdress.Text);
                }
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(webAdress.Text);
        }

        private List<XElement> GetIndexItems(XElement contentTag, ref XNode nextNode)
        {
            bool found = false;
            XNode next = contentTag.NextNode;
            List<XElement> index = new List<XElement>();
            while (!found && next != null)
            {
                if (next.NodeType == XmlNodeType.Element)
                {
                    XElement test = next as XElement;
                    if (test.Name.LocalName == "a" && (test.Attribute("name") == null ? "" : test.Attribute("name").Value) == "map0")
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        index.AddRange(test
                                        .Descendants("a")
                                        .Where(p=>p.GetElementText().Trim()!="")
                                        .ToList()
                                    );
                    }

                }
                next = next.NextNode;
            }
            if (index.Count() != 0)
            {
                nextNode = next;
                return index;
            }
            return null;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            //XElement html = global.GetWebpageHtml(webAdress.Text, Encoding.GetEncoding("iso-8859-1"));// XElement.Parse(shtml);
            if (lawName.SelectedItem != null)
            {
                lawObject lo = lawName.SelectedItem as lawObject;
                lo.importhref = webAdress.Text;
                GlobalVar._LawList.Save(global.m_LawsFile);
            }
            if (webAdress.Text == "") return;
            Uri uri = new Uri(webAdress.Text);
            string err = "";
            XElement html = global.GetWebPageHTML(webAdress.Text, ref err);
            if (html == null)
            {
                MessageBox.Show(err);
                return;
            }

            if (html == null) return;
            html.Save(@"c:\_test\xyz.xml");
            XElement nodemeta = html.DescendantNodes().OfType<XText>().Where(p => p.Value.Trim() == "DATO:" && p.Ancestors("table").Count()!=0).First().Ancestors("table").FirstOrDefault();
            nodemeta.Descendants("font").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            html.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == "c0" && p.GetElementText().Trim() == "").Remove();
            XNode contentNode = html.DescendantNodes().OfType<XText>().Where(p => p.Value.Trim() == "INNHOLD" && p.Ancestors("h3").Count() != 0).FirstOrDefault();
            XElement contentTag = null;
            XElement toc = null;
            XNode next = null;
            if (contentNode == null)
            #region //No content
            {
                contentTag = html.Descendants("h3").FirstOrDefault();
                if (contentTag != null)
                {
                    XElement text = new XElement("text");

                    toc = new XElement("root",
                            new XElement("section"
                            , new XElement("title", contentTag.DescendantNodesAndSelf().OfType<XText>().Select(s=>s.ToString()).StringConcatenate())
                            , text
                            )
                        );
                    if (contentTag.NextNode != null)
                    {
                        next = contentTag.NextNode;
                        XElement content = GetPageContent(next);
                        
                        if (content.HasElements)
                        {
                            text.Add(content.Nodes());
                        }
                    }
                }
            }
            #endregion
            else
            #region //Has content
            {

                contentTag = contentNode.Ancestors("h3").FirstOrDefault();
                List<XElement> index = GetIndexItems(contentTag, ref next);
                string delimeter = " ";
                
                List<string> hTest = index
                                 .Select(p => rxHeaderTest.Match(p.GetElementText()))
                                 .Select(q => q.MatchEx(rxHeaderTest))
                                 .Select(r=>r.Skip(1).Aggregate(new StringBuilder().Append(r.Count==0 ? "" : r.FirstOrDefault().name ?? ""),(sb, x) => sb.Append(delimeter).Append(x)))
                                 .Select(r=>r.ToString())
                                 .ToList();
                                 
                ////                     test.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate()










                //List<string> hTest = index
                //                 .Select(p => Regex.Split(p.GetElementText(), @"kapit(e)?l(et)?\.|del\.|\.(?!\s(kapit(e)?l(et)?|del)+\.)", RegexOptions.IgnoreCase).FirstOrDefault())
                //                 .ToList();


                //foreach (string s in hTest)
                //{
                //    if (Regex.IsMatch(s, @"^lov\s|forskrift\s"))
                //    {
                //    }
                //    else if (Regex.IsMatch(s, "vedlegg"))
                //    {
                //    }
                //    else if (Regex.IsMatch(s, "kap(it(t)?el(et)?)?$"))
                //    {
                //    }
                //    else if (Regex.IsMatch(s, "^kap(it(t)?el(et)?)?"))
                //    {
                //    }
                //    else if (Regex.IsMatch(s, "^§§"))
                //    {
                //    }
                //    else if (Regex.IsMatch(s, "^§"))
                //    {
                //    }

                //}







                int minPara = 100;
                int maxPara = 0;
                BuildHeaderHierachy bh = new BuildHeaderHierachy(minPara, maxPara, false);
                bh._v0_GetHeaderType(index);

                XElement container = new XElement("container");
                toc = bh.GetIndexBranch(index, true, container);
                if (toc.HasElements)
                {
                    if ((toc.Elements("section").First().Attribute("href") == null ? "" : toc.Elements("section").First().Attribute("href").Value) == "#map0")
                    {
                        XElement newToc = new XElement(toc);
                        XElement top = new XElement(newToc.Elements("section").Where(p => (p.Attribute("href") == null ? "" : p.Attribute("href").Value) == "#map0").First());
                        newToc.Elements("section").Where(p => (p.Attribute("href") == null ? "" : p.Attribute("href").Value) == "#map0").First().Remove();
                        top.Add(newToc.Elements());
                        toc.Elements().Remove();
                        toc.Add(new XElement(top));
                    }
                    XElement content = GetPageContent(next);
                    
                    if (content.HasElements)
                    {
                        next = content.FirstNode;
                        int n = toc.Descendants("section").Count();
                        for (int i = 0; i < n; i++)
                        {
                            XElement currentSection = toc.Descendants("section").ElementAt(i);
                            XElement nextSection = null;
                            string currentBm = currentSection.Attribute("href").Value.Replace("#", "");
                            string nextBm = "";
                            if ((i + 1) < n)
                            {
                                nextSection = toc.Descendants("section").ElementAt(i + 1);
                                nextBm = nextSection.Attribute("href").Value.Replace("#", "");
                            }
                            XElement text = new XElement("text");
                            currentSection.AddFirst(text);
                            while (next != null)
                            #region //Add to section
                            {
                                if (next.NodeType == XmlNodeType.Element)
                                {
                                    XElement test = next as XElement;
                                    if (test.DescendantsAndSelf("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == currentBm).Count() != 0)
                                    {
                                        if ((test.Attribute("name") == null ? "" : test.Attribute("name").Value) == currentBm)
                                        {
                                            if (next.NextNode != null)
                                            {
                                                next = next.NextNode;
                                                while (next != null)
                                                {
                                                    if (next.NodeType == XmlNodeType.Element)
                                                    {
                                                        test = next as XElement;
                                                        if (Regex.IsMatch(test.Name.LocalName, @"h\d"))
                                                        {
                                                            string title = test.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate();
                                                            currentSection.AddFirst(new XElement("title", title));
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (test.Name.LocalName == "p")
                                        {
                                            test.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == currentBm).First().Remove();
                                            string title = test
                                                            .DescendantNodes()
                                                            .OfType<XText>()
                                                            .TakeWhile(p => p.Parent.Name != "p")
                                                            .Select(s => s.ToString()).StringConcatenate(" ");
                                            currentSection.AddFirst(new XElement("title", title));
                                           
                                            List<XElement> dels = test
                                                                .DescendantNodes()
                                                                .OfType<XText>()
                                                                .TakeWhile(p => p.Parent.Name != "p")
                                                                .Select(r => 
                                                                        r.Ancestors().Where(q => q.Parent.Name == "p").First()
                                                                ).ToList();
                                            foreach (XElement el in dels)
                                                try
                                                {
                                                    el.Remove();
                                                }
                                                catch
                                                {
                                                }
                                            
                                            if (test.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() != "")
                                            {
                                                text.Add(new XElement("p", test.Nodes()));
                                            }
                                        }
                                    }
                                    else if (test.DescendantsAndSelf("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == nextBm).Count() != 0)
                                    {
                                        break;
                                    }
                                    else if (test.Name.LocalName == "br")
                                    {
                                        text.Add(test);
                                    }
                                    else
                                    {
                                        if (test.DescendantNodesAndSelf().OfType<XText>().Select(s => s.ToString()).StringConcatenate().Trim() != "")
                                        {
                                            text.Add(test);
                                        }
                                    }
                                }
                                else if (next.NodeType == XmlNodeType.Text)
                                {
                                    text.Add(new XElement("p", next));
                                }
                                next = next.NextNode;
                            }
                            #endregion
                        }
                    }
                }
            }
            #endregion

            GetElementInText(toc);

            if (toc.HasElements)
            {
                m_DocumentData = new XElement("documents"
                            , new XElement("document"
                                , toc.Nodes()
                                )
                            );
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }



            
        }

        private void EnhanceLawText(XElement text)
        {

            Regex regexpPktNrPara = new Regex(@"^("
                                      + @"(?<decimal>(\d+))"
                                + "|" + @"(?<loweralfa>([a-zæøå]))"
                                + "|" + @"(?<upperalfa>([A-ZÆØÅ]))"
                                + "|" + @"(?<lowerroman>([ivx]+))"
                                + "|" + @"(?<upperroman>([IVX]+))"
                                + "|" + @"(?<strek>(\-))"
                                + @")(\)|\.)(\s)");
            
            
            Regex regexpPktNr = new Regex(@"^((("
                                      + @"(?<decimal>(\d+))"
                                + "|" + @"(?<loweralfa>([a-zæøå]))"
                                + "|" + @"(?<upperalfa>([A-ZÆØÅ]))"
                                + "|" + @"(?<lowerroman>([ivx]+))"
                                + "|" + @"(?<upperroman>([IVX]+))"
                                + @")(\)|\.))"
                                + "|" + @"(?<sep>((\-\s+\-)))"
                                + "|" + @"(?<strek>((\-$)))"
                                + ")");
            XElement ledd = new XElement("lovledd");
            XElement footnotes = new XElement("footnotes");
            XElement container = new XElement("container");
            int leddCounter = 0;
            List<XElement> ledds = text.Elements().Where(p => Regex.IsMatch(p.GetElementText(" ").Trim(), @"^\(\d+\)")).ToList();
            List<XElement> els = text.Elements().ToList();
            XElement leddHolder = null;
            foreach (XElement e in els)
            {
                if (e.GetElementText().Trim() == "Balansen skal ha følgende oppstillingsplan:")
                {
                }
                if (e.Name.LocalName == "p")
                {
                    if (e.Nodes().OfType<XText>().Select(p => p.ToString()).StringConcatenate().Trim() == ""
                            && (e.Elements().Count() == 0 ? "" : e.Elements().First().Name.LocalName) == "small")
                    {
                        container.Add(new XElement("lovcomment", new XElement("text", e.Elements("small").First().Nodes())));
                    }
                    else if (ledds.Count() != 0 && ledds.Count() > leddCounter ? ledds.ElementAt(leddCounter) == e : false)
                    {
                        bool bPunktum = e.GetElementText().Trim().EndsWith(".");
                        leddHolder = new XElement(ledd);
                        container.Add(leddHolder);
                        leddHolder.Add(e.GetElementPeriode().Nodes());
                        leddCounter++;
                    }
                    else if (((ledds.Count() != 0 && ledds.Count() > leddCounter ? ledds.ElementAt(leddCounter) != e : false) && leddHolder != null)
                        || (ledds.Count() == 0))
                    {
                        Match m = regexpPktNrPara.Match(e.GetElementText().Trim());
                        if (m.Success)
                        {
                            int iLevel = 4;
                            string pktType = "";
                            string nr = "";
                            qResult gr = m.MatchEx(regexpPktNr).FirstOrDefault();
                            if (gr != null)
                            {
                                pktType = gr.name;
                                nr = gr.value;
                            }

                            XElement Lh = new XElement("lovpunkt",
                                    new XAttribute("value", nr),
                                    new XAttribute("punkttype", pktType),
                                    new XAttribute("level", iLevel.ToString()),
                                    new XElement("text", e));
                            leddHolder.AddLovPunkt(Lh);
                        }
                        else
                        {
                            if (ledds.Count() == 0)
                            {
                                bool bPunktum = e.GetElementText().Trim().EndsWith(".");
                                if (Regex.IsMatch(e.GetElementText().Trim(), @"^[A-ZÆØÅ][^\.\)A-ZÆØÅ]+"))
                                {
                                    leddHolder = null;
                                }
                                if (leddHolder == null)
                                {
                                    leddHolder = new XElement(ledd);
                                    container.Add(leddHolder);
                                }


                                leddHolder.Add(e.GetElementPeriode().Nodes());

                                if (bPunktum) leddHolder = null;
                            }
                            else
                            {
                                leddHolder.Add(e);
                            }
                        }
                    }
                    else if (ledds.Count() != 0 && ledds.Count() > leddCounter && leddHolder == null)
                    {
                        container.Add(e);
                    }

                    //else if (ledds.Count() == 0)
                    //{
                    //    bool bPunktum = e.GetElementText().Trim().EndsWith(".");
                    //    if (Regex.IsMatch(e.GetElementText().Trim(), @"^[A-ZÆØÅ][^\.\)]+"))
                    //    {
                    //        leddHolder = null;
                    //    }
                    //    if (leddHolder == null)
                    //    {
                    //        leddHolder = new XElement(ledd);
                    //        container.Add(leddHolder);
                    //    }
                        

                    //    leddHolder.Add(e.GetElementPeriode().Nodes());
                        
                    //    if (bPunktum) leddHolder = null;
                    //}

                }
                else if (e.Name.LocalName == "table")
                {
                    if (e.Descendants("tr").Count() == 1
                        && (e.Descendants("td").Count() == 2
                        && (e.Descendants("td").Elements().Count() == 0 ?
                            false : (e.Descendants("td").Elements().First().Name.LocalName) == "small" ?
                                Regex.IsMatch(e.Descendants("td").Elements().First().GetElementText().Trim(), @"^\d+$") : false)))
                    {
                        footnotes.Add(new XElement("lovfootnote",
                            new XAttribute("id", Guid.NewGuid().ToString()),
                            new XElement("title", "[" + e.Descendants("td").ElementAt(0).GetElementText().Trim() + "]"),
                            new XElement("text", e.Descendants("td").ElementAt(1).GetElementText(" ").Trim())));
                    }
                    else if (
                           (e.Descendants("tr").Count() == 1)
                        && (e.Descendants("td").Count() == 2 ?
                                (e.Descendants("td").ElementAt(0).Attributes("width") != null ?
                                    e.Descendants("td").ElementAt(0).Attribute("width").Value.EndsWith("%") : false) : false)
                        && (e.Descendants("td").Count() == 2 ? regexpPktNr.IsMatch(e.Descendants("td").ElementAt(0).GetElementText().Trim()) : false)
                        )
                    {
                        if (leddHolder == null)
                        {
                            leddHolder = new XElement(ledd);
                        }
                        string level = e.Descendants("td").ElementAt(0).Attribute("width").Value.Replace("%", "");
                        int iLevel = Convert.ToInt32(level);

                        Match m = regexpPktNr.Match(e.Descendants("td").ElementAt(0).GetElementText().Trim());

                        string pktType = "";
                        string nr = "";
                        qResult gr = m.MatchEx(regexpPktNr).FirstOrDefault();
                        if (gr != null)
                        {
                            pktType = gr.name;
                            nr = gr.value;
                        }

                                                
                        XElement Lh = new XElement("lovpunkt",
                                new XAttribute("value", nr),
                                new XAttribute("punkttype", pktType),
                                new XAttribute("level", iLevel.ToString()),
                                e);
                        leddHolder.AddLovPunkt(Lh);

                    }
                    else if (e.Name.LocalName == "table")
                    {
                        if (leddHolder == null)
                        {
                            leddHolder = new XElement(ledd);
                            container.Add(leddHolder);
                        }
                        leddHolder.Add(e);
                    }
                }
            }
            
            container.Add(footnotes.Nodes());
            List<XElement> sups = container.Descendants("sup").ToList();
            if (sups.Count() != 0)
            {
                foreach (XElement sup in sups)
                {
                    string suptext = sup.GetElementText().Trim();
                    XElement footnote = container.Descendants("lovfootnote").Where(p => (p.Element("title") == null ? "" : p.Element("title").Value) == "[" + suptext + "]").FirstOrDefault();
                    if (footnote == null) continue;
                    sup.ReplaceWith(new XElement("sup"
                        , new XElement("a"
                            , new XAttribute("href", "#")
                            , new XAttribute("id", Guid.NewGuid().ToString())
                            , new XAttribute("data-bm", footnote.Attribute("id").Value)
                            , new XAttribute("class", "xref")
                            , new XAttribute("onclick", "return false;")
                            , suptext)
                        ));
                }
            }
            text.ReplaceWith(container.Nodes());
        }
        private void GetElementInText(XElement toc)
        {
            List<XElement> texts = toc.Descendants("text").ToList();
            foreach (XElement text in texts)
            {
                if (text.Parent.Element("title").GetElementText().Trim().StartsWith("§"))
                {
                    EnhanceLawText(text);
                }
            }

        }

        private void RemoveFnTag(XElement content)
        {
            List<XElement> fns = content.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "fn").ToList();
            foreach (XElement fn in fns)
            {
                XElement parent = fn.Parent;
                if (fn == parent.Nodes().First())
                {
                    fn.ReplaceWith(fn.Nodes());
                }
            }
            fns = content.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "fn").ToList();
            foreach (XElement fn in fns)
            {
                if ((fn.Parent != null ? fn.Parent.Name.LocalName : "") == "p")
                {
                    fn.Parent.AddAfterSelf(new XElement("p",
                         new XElement("small",
                             fn.Nodes())));
                    fn.Remove();
                }
            }
        }
        private XElement GetPageContent(XNode n)
        {
            XElement content = new XElement("content");
            while (n != null)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement test = n as XElement;
                    content.Add(new XElement(test));
                }
                else if (n.NodeType == XmlNodeType.Text)
                {
                    content.Add(new XElement("p", n.ToString()));
                }

                n = n.NextNode;
            }
            return content;
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Close();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webAdress.Text = e.Url.AbsoluteUri; 
        }

        private void webAdress_TextChanged(object sender, EventArgs e)
        {

        }

        

        private void btnLoadLaw_Click(object sender, EventArgs e)
        {
            if (txtLawSite.Text != "")
            {
                try
                {
                    Uri uri = new Uri(txtLawSite.Text);
                    string err = "";
                    XElement html = global.GetWebPageHTML(txtLawSite.Text, ref err);
                    if (html == null)
                    {
                        MessageBox.Show(err);
                        return; 
                    }

                    XNamespace xn = "http://www.w3.org/1999/xhtml";
                    XElement h1 = html.Descendants("h1").Where(p => p.GetElementText().ToLower().Trim() == "gjeldende lover ordnet alfabetisk").FirstOrDefault();
                    XElement td = h1.Parent;

                    List<lawObject> lover = td
                        .Descendants("li")
                        .Select(p => new lawObject(p, uri)
                        ).ToList();
                    if (lover.Count() != 0)
                    {
                        GlobalVar._LawList = new LawList(lover);
                        GlobalVar._LawList.Save(global.m_LawsFile);
                        LoadComboBox();
                        lawName.Enabled = true;
                        lawName.Focus();
                    }
                }
                catch (SystemException error)
                {
                    MessageBox.Show(error.Message);
                }
            }

        }


        private void txtLawSite_TextChanged(object sender, EventArgs e)
        {

        }

       

    }
}
