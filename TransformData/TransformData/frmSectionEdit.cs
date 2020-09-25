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
using System.Xml.Xsl;
using Dib.Transform;
using System.IO;
using DIB.RegExp.ExternalStaticLinks;
using TransformData.Global;
using System.Text.RegularExpressions;
using DIB.Data;
using System.Diagnostics;
using DIB.InTextLinking;
using TransformData.Global;


namespace TransformData
{
    public partial class frmSectionEdit : Form
    {
        private IDictionary<HtmlElement, string> elementStyles = new Dictionary<HtmlElement, string>();
        private IDictionary<HtmlElement, string> element = new Dictionary<HtmlElement, string>();
        private HtmlElement _element = null;
        private HtmlElement _mElement = null;
        private int _elementIndex;

        private XElement _DOCXML;
        private string _TARGETDIR;
        private string _FILENAME;
        private string _TOPIC_ID = "";
        private string _ImgFolder = "";
        private List<Category> items = new List<Category>();

        public frmSectionEdit()
        {
            InitializeComponent();
        }

        public frmSectionEdit(string html)
        {
            InitializeComponent();
            XElement d = XElement.Parse(html);
            _FILENAME = "";
            _TARGETDIR = Path.GetTempPath();
            LoadHtml(d);
        }
        public frmSectionEdit(string XMLFile, string targetDir, XElement xmlInfo)
        {
            this.Tag = xmlInfo;
            _FILENAME = "";
            //FEIL: Inneholder ikke hele pakken
            _DOCXML = XElement.Load(XMLFile);
            _FILENAME = XMLFile;
            CleanXML(_DOCXML);
            _TARGETDIR = targetDir;
            RemoveDoubleText();
            InitializeComponent();
            LoadDataSet();
        }


        private void CleanXML(XElement _DOCXML)
        {
            int n = _DOCXML.Descendants("span").Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "k-xref").Count();
            for (int i = n; i > 0; i--)
            {
                XElement s = _DOCXML.Descendants("span").Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "k-xref").ElementAt(i - 1);
                s.ReplaceWith(new XText(s.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate()));
            }
        }

        private void RemoveDoubleText()
        {
            int n = _DOCXML.DescendantsAndSelf("text").Count();
            for (int i = n; i > 0; i--)
            {
                XElement text = _DOCXML.DescendantsAndSelf("text").ElementAt(i - 1);
                if (text.Ancestors("text").Count() != 0)
                {
                    text.ReplaceWith(text.Nodes());
                }
            }
            _DOCXML.Save(_FILENAME);
        }
        private void WebContextMenuShowing(object sender, HtmlElementEventArgs e)
        {
            e.ReturnValue = false;
        }
        private void StartMark()
        {
            webBrowser1.Document.MouseOver += new HtmlElementEventHandler(document_MouseOver);
            webBrowser1.Document.MouseLeave += new HtmlElementEventHandler(document_MouseLeave);
            webBrowser1.Document.MouseDown += new HtmlElementEventHandler(document_MouseDown);
            webBrowser1.Document.ContextMenuShowing += new HtmlElementEventHandler(WebContextMenuShowing);

            System.Windows.Forms.HtmlDocument d = webBrowser1.Document;
            foreach (HtmlElement e in d.All)
            {

                if (!this.elementStyles.ContainsKey(e))
                {
                    string style = e.Style;
                    this.elementStyles.Add(e, style);
                    //string style = this.elementStyles[e];
                    //this.elementStyles.Remove(e);
                    //e.Style = style + "; position:static;";
                    //Debug.Print(style);
                }
            }
        }


        private void StopMark()
        {
            System.Windows.Forms.HtmlDocument cd = webBrowser1.Document;
            foreach (HtmlElement e in cd.All)
            {
                if (this.elementStyles.ContainsKey(e))
                {
                    string style = this.elementStyles[e];
                    this.elementStyles.Remove(e);
                    e.Style = style;
                }
            }

            _element = null;
            _mElement = null;

            webBrowser1.Document.MouseOver -= document_MouseOver;
            webBrowser1.Document.MouseLeave -= document_MouseLeave;
            webBrowser1.Document.MouseDown -= document_MouseDown;
            webBrowser1.Document.ContextMenuShowing -= WebContextMenuShowing;

            //tbElName.Text = "";
            //tbMarked.Text = "";

        }


        private void document_MouseOver(object sender, HtmlElementEventArgs e)
        {
            _element = e.ToElement;

            if (!this.elementStyles.ContainsKey(_element))
            {
                string style = _element.Style;
                this.elementStyles.Add(_element, style);
                _element.Style = style + "; position:static; background-color: #ffc; border:solid 1px black;";
                //tbElName.Text = _element.GetAttribute("className").ToString() ?? "(no id)";
            }
        }

        private void document_MouseLeave(object sender, HtmlElementEventArgs e)
        {
            _element = e.FromElement;
            if (_mElement == _element) return;
            if (this.elementStyles.ContainsKey(_element))
            {
                string style = this.elementStyles[_element];
                this.elementStyles.Remove(_element);
                _element.Style = style;

            }
        }


        private void document_MouseDown(object sender, HtmlElementEventArgs e)
        {
            //Point MPoint = new Point(e.MousePosition.X, e.MousePosition.Y);
            System.Windows.Forms.HtmlDocument CurrentDocument = webBrowser1.Document;

            //_element = CurrentDocument.GetElementFromPoint(MPoint);
            _element = CurrentDocument.GetElementFromPoint(e.ClientMousePosition);

            string style = "";
            if (!this.elementStyles.ContainsKey(_element))
            {
                style = _element.Style;
                this.elementStyles.Add(_element, style);
            }


            _element.Style = style + "; background-color: green; border:solid 1px black;";
            this.Text = _element.GetAttribute("className").ToString() ?? "(no id)";
            _mElement = _element;

            int counter = 0;
            foreach (HtmlElement el in CurrentDocument.All)
            {
                if (el == _element)
                {
                    _elementIndex = counter;
                }
                counter++;
            }

            //tbMarked.Text = _element.GetAttribute("className").ToString() ?? "(no id)";
            //_htmlText.Text = _element.InnerHtml;

        }


        private void LoadDataSetLevel(XElement doc)
        {
            treeView1.Nodes.Clear();
            _DOCXML = doc;
            XElement content = null;

            if (_DOCXML.Element("content") == null)
            {
                HtmlHierarchy h = new HtmlHierarchy("level");
                content = new XElement("root", h.GetDIBXMLContent(_DOCXML));
            }
            else
                content = new XElement(_DOCXML.Element("content"));


            items = new List<Category>();
            items.Add(new Category("-1", "", "INNHOLD"));

            int n = content
                .Descendants()
                .Where(p =>
                    p.Name.LocalName == "nitem")
                .Count();

            for (int i = 0; i < n; i++)
            {
                XElement item = content.Descendants("nitem").ElementAt(i);
                string id = item.Attribute("key") == null ? item.Attribute("title").Value : item.Attribute("key").Value;
                string pid = item.Ancestors("nitem").Count() == 0 ? "-1" : item.Ancestors("nitem").First().Attribute("key") == null ? item.Ancestors("nitem").First().Attribute("title").Value : item.Ancestors("nitem").First().Attribute("key").Value;
                string text = item.Attribute("title").Value;
                items.Add(new Category(id, pid, text));
            }
            buildtree();
        }

        public void LoadDataSet(XElement xml)
        {
            _DOCXML = xml;
            Load_DOCXML();
        }

        private void LoadDataSet()
        {
            treeView1.Nodes.Clear();
            GetImgFolder(_DOCXML);
            XElement content = null;
            if (_DOCXML.Element("content") != null)
            {
                content = new XElement(_DOCXML.Element("content"));
            }
            else
            {
                content = _DOCXML.DescendantsAndSelf().Where(p => p.Name.LocalName == "documents"
                                || p.Name.LocalName == "document").First().GetContentMain(false);
            }
            items = new List<Category>();
            items.Add(new Category("-1", "", "INNHOLD"));

            int n = content
                .Descendants()
                .Where(p=>
                    p.Name.LocalName=="item")
                .Count();

            for (int i = 0 ; i < n ; i++)
            {
                XElement item = content.Descendants("item").ElementAt(i);
                string id = item.Attribute("id") == null ? item.Attribute("text").Value : item.Attribute("id").Value;
                string pid = item.Ancestors().Count() == 1 ? "-1" : item.Ancestors().First().Attribute("id") == null ? item.Ancestors().First().Attribute("text").Value : item.Ancestors().First().Attribute("id").Value;
                string text = item.Attribute("text").Value;
                items.Add(new Category(id, pid, text));
            }
            buildtree();
            

        }

        private void buildtree() 
        {
            treeView1.Nodes.Clear();    // Clear any existing items
            treeView1.BeginUpdate();    // prevent overhead and flicker
            LoadBaseNodes();            // load all the lowest tree nodes
            treeView1.EndUpdate();      // re-enable the tree
            treeView1.Refresh();        // refresh the treeview display        
            treeView1.Nodes[0].Expand();
        }
        
        private void LoadBaseNodes()
        {
            try
            {
                string baseParent = "";                 // Find the lowest root category parent value
                TreeNode node;
                foreach (Category cat in items)
                {
                    if (cat.ParentID == baseParent)
                        baseParent = cat.ParentID;
                }
                foreach (Category cat in items)         // iterate through the categories
                {
                    if (cat.ParentID == baseParent)     // found a matching root item 
                    {

                        node = treeView1.Nodes.Add("(" + cat.ID + ") " + cat.NodeText); // add it to the tree
                        node.Tag = cat;                 // send the category into the tag for future processing
                        getChildren(node);              // load all the children of this node
                    }
                }
            }
            catch (SystemException err)
            {
                throw new Exception("Error - LoadBaseNodes:\r\n" + err.Message);
            }
        }
        private void getChildren(TreeNode node)
        {
            TreeNode Node = null;
            Category nodeCat = (Category)node.Tag;  // get the category for this node
            foreach (Category cat in items)         // locate all children of this category
            {
                if (cat.ParentID == nodeCat.ID)     // found a child
                {
                    Node = node.Nodes.Add("(" + cat.ID + ") " + cat.NodeText);    // add the child
                    Node.Tag = cat;                         // set its tag to its category
                    getChildren(Node);                      // find this child's children
                }
            }
        }
        class Category 
        { 
            public string ID;
            public string ParentID;
            public string NodeText;
            public Category(string ID, string ParentID, string NodeText)
            {
                this.ID = ID;
                this.ParentID = ParentID;
                this.NodeText = NodeText;
            }
            public override string ToString()
            {
                return this.NodeText;
            }
        }

        private void ShowDocument(TreeNode n)
        {
            Category c = (Category)n.Tag;
            if (c.ID != "-1")
            {
                if (c.NodeText == c.ID)
                {
                    webBrowser1.DocumentText = "";
                }
                else
                {
                    XElement section = _DOCXML
                            .Descendants()
                            .Where(p => 
                                (
                                    p.Name.LocalName == "document"
                                    || p.Name.LocalName == "section"
                                    || p.Name.LocalName == "level"
                                    || p.Name.LocalName == "lovledd"
                                    || p.Name.LocalName == "lovpunktum"
                                    || p.Name.LocalName == "lovpunkt"
                                )
                                &&
                                (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            )
                            .FirstOrDefault();
                    if (section != null)
                    {
                        ShowPart(section);
                    }
                    else
                    {
                        MessageBox.Show("ID: " + c.ID + " " + "har ikke innhold!");
                    }

                }
            }
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse)
            {
                TreeNode n = e.Node;
                ShowDocument(n);
            }
        }

        private void RemapIMG(XElement section)
        {
            if (!(_ImgFolder == "" ? false : Directory.Exists( _ImgFolder ))) return;
            //string imgPath = @"D:/EY/EPUB/HTMLFinal/OEBPS/tempimage/";
            List<XAttribute> src = section.Descendants("img")
                                   .Where(p => p.Attribute("src") != null)
                                   .Select(p => p.Attribute("src"))
                                   .ToList();
            if (src.Count() == 0) return;
            foreach (XAttribute a in src)
            {

                string imgFileName = Regex.Split(a.Value, @"\\|\/").Last();
                imgFileName = Path.Combine(_ImgFolder, imgFileName);
                if (File.Exists(imgFileName))
                {
                    a.Value = imgFileName;
                }
                //else if ((a.Parent.Attribute("alt") == null ? "" : a.Parent.Attribute("alt").Value).EndsWith(".jpg"))
                //{
                //    imgFileName = Regex.Split(a.Parent.Attribute("alt").Value, @"\\|\/").Last();
                //    a.Value = imgPath + a.Parent.Attribute("alt").Value;
                //    if (File.Exists(imgPath + imgFileName))
                //    {
                //        a.Value = imgPath + imgFileName;
                //    }
                //}
            }
        }

        private void ShowPart(XElement section)
        {

            RemapIMG(section);

            //string path = Path.GetDirectoryName(Application.ExecutablePath);
            string path = global.m_Projects + @"\TransformData";
            string xsltPath = "";
            XElement parent = section.Ancestors().Last();
            DibDocumentType docType = parent.GetDocumentType();

            if (
                docType==DibDocumentType.LigningsABC
                || docType==DibDocumentType.MVAHandbok
                || docType==DibDocumentType.IfrsBooks)
            {
            //if (section.Ancestors("documents").Where(p =>
            //           (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "handbook"
            //           && (p.Attribute("variant") == null ? "" : p.Attribute("variant").Value) == "abc"
            //           ).Count() == 1)
            //{
                section = new XElement("hbcontent", section);
                xsltPath = path + @"\xsl\hb-normal.xslt";
            }
            else if (docType == DibDocumentType.Lover)
            {
                section = new XElement("lovcontent", section);
                xsltPath = path + @"\xsl\lov-normal.xslt";
            }
            else if (docType == DibDocumentType.Level7)
            //else if (section.Ancestors().Last().Elements("document").Where(p => (p.Attribute("doctypeid") == null ? "" : p.Attribute("doctypeid").Value) == "7").Count() != 0)
            {
                xsltPath = path + @"\xsl\level-normal.xslt";
            }
            else
            {
                xsltPath = path + @"\xsl\forarbeider.xslt";
            }

            XmlReader xmlreader = section.CreateReader();
            XsltArgumentList xslArg = new XsltArgumentList();
 

            xslArg.AddParam("cssPath", "", path + @"\css\");



            string outString = transformDocument.TransformXmlWithXslToString(xmlreader
                , xsltPath, xslArg);
            xmlreader.Close();


            StringBuilder sb = new StringBuilder();
            sb.Append(outString);

            //string tempFile = _TARGETDIR + "part.htm"
            string tempFile = Path.GetTempFileName();

            using (StreamWriter outfile =
                new StreamWriter(tempFile))
            {
                outfile.Write(sb.ToString());
            }

            webBrowser1.Navigate(tempFile);
        }
        private void finnTilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            MessageBox.Show(c.ID);
        }

        private void avsluttToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private XElement SelectTreeSection(Category c)
        {
            XElement result = null;
            if (c.ID == "-1")
            {
                //<document doctypeid="7" /> 
                if (_DOCXML.DescendantsAndSelf("document").Where(p => (p.Attribute("doctypeid") == null ? "" : p.Attribute("doctypeid").Value) == "7").Count() != 0)
                {
                    result = _DOCXML;
                }
                else if (_DOCXML.DescendantsAndSelf("document").Where(p =>
                                (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "word"
                                && (p.Attribute("variant") == null ? "" : p.Attribute("variant").Value) == "lov"
                    ).Count() != 0)
                {
                    result = _DOCXML;
                }
                else
                {
                    result = _DOCXML.DescendantsAndSelf("documents").First();
                }
            }
            else
            {
                result = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "section"
                                || p.Name.LocalName == "document"
                                || p.Name.LocalName == "level"
                            )
                            && (p.Attribute("id") == null ? "" :p.Attribute("id").Value) == c.ID
                    ).First();
            }
            if (result == null) MessageBox.Show("Finner ikke valgt seksjon i dokumentet!");
            return result;
        }

        private void finnInternreferanseKapittelToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void markerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem m = (ToolStripMenuItem)sender;
                if (!m.Checked)
                {
                    m.Checked = true;
                    StartMark();
                }
                else
                {
                    m.Checked = false;
                    StopMark();
                }
            }
        }

        private void visKoblingerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((XElement)this.Tag).Element("topic_id")!= null)
            {
                string topic_id = ((XElement)this.Tag).Element("topic_id").Value;
                commxml cx = new commxml();
                string sourceRegexp = cx.Execute("TopicMap.dbo._ImpClient_Get_Bookmarks '" + topic_id + "'");
                XDocument d = XDocument.Parse(sourceRegexp);
                if (d.Descendants("result").Count() == 0) return;
                XElement result = d.Descendants("result").First();
                if (result.Descendants("associations").Count() == 0) return;
                if (result.Descendants("root").Count() == 0) return;



                XElement f = XElement.Load(global.m_forarbeidertoimport);
                if (f.Descendants("container")
                    .Where(p =>
                        (p.Attribute("topic_id") == null ? "" : p.Attribute("topic_id").Value) == ((XElement)this.Tag).Element("topic_id").Value
                        && p.Descendants("qreferance").Count() != 0
                    ).Count() != 0)
                {
                    XElement container = f.Descendants("container")
                    .Where(p =>
                        (p.Attribute("topic_id") == null ? "" : p.Attribute("topic_id").Value) == ((XElement)this.Tag).Element("topic_id").Value
                        && p.Descendants("qreferance").Count() != 0
                    ).ElementAt(0);

                    frmBookmarks frm = new frmBookmarks(container, _DOCXML);
                    frm.Show(this);

                }
            }
        }

        private void RebuildContent()
        {
            if (_DOCXML.IsLevelDocument())
            {
                HtmlHierarchy h = new HtmlHierarchy("level");
                _DOCXML.Descendants("content").Remove();
                _DOCXML.Add(new XElement("content", h.GetDIBXMLContent(_DOCXML)));
                LoadDataSetLevel(_DOCXML);
            }
            else
            {
                if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                _DOCXML.AddFirst(_DOCXML.GetContentMain(true));
                LoadDataSet();
            }

        }

        private void EditHtml()
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "") != "-1")
            {
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    
                    XElement text = _DOCXML.Descendants()
                            .Where(p =>
                                (
                                    p.Name.LocalName == "document"
                                    || p.Name.LocalName == "section"
                                    || p.Name.LocalName == "level"
                                )
                                && (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                                ).Elements("text").FirstOrDefault();
                    
                    if (text == null)
                    {
                        MessageBox.Show("Elementet mangler 'text-element'!");
                        return;
                    }
                    XElement parent = text.Parent;
                    if (text != null)
                    {
                        frmEdit f = new frmEdit(text);
                        f.ShowDialog(this);
                        if (f.DialogResult == DialogResult.OK)
                        {
                            XElement newtext = XElement.Parse("<text>" + f.returnHtml + "</text>");
                            RebuildPart(parent, text, newtext);
                            //XElement returnEl = newtext.BuildLevelsFromHeader(text.Parent.Attribute("id").Value);
                            //if (parent.Name.LocalName == "level") returnEl = returnEl.ReplaceSections("level");
                            //if (returnEl.Name.LocalName == "text")
                            //{
                            //    text.ReplaceWith(new XElement(returnEl));
                            //    RebuildContent();

                            //}
                            //else
                            //{
                            //    text.ReplaceWith(returnEl.Nodes());
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));
                            //    RebuildContent();
                            //}
                        }
                    }

                    TreeNode node = treeView1.Nodes[0];
                    FindTreeNode(node, c.ID);
                }
            }

        }

        private void redigerHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHtml();
        }

        private void GetImgFolder(XElement doc)
        {
           
            if (doc.Descendants("img").Count() == 0 )  return;
            if (_ImgFolder != "") return;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = Environment.CurrentDirectory;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _ImgFolder = fbd.SelectedPath;
            }
        }
        private void lagreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((_FILENAME == null ? "" :  _FILENAME)!="")
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.Filter = "XML filer (*.xml)|*.xml"  ;
                sf.FilterIndex = 0 ;
                sf.RestoreDirectory = true;
                sf.FileName = _FILENAME;
                if (sf.ShowDialog() != DialogResult.OK) return;
                _DOCXML.Save(sf.FileName);
            }
            else
            {
                SaveFileDialog  sf = new SaveFileDialog();
                if ((_TARGETDIR == null ? "" : _TARGETDIR) != "")
                    sf.InitialDirectory = _TARGETDIR;

                sf.Filter = "XML filer (*.xml)|*.xml";
                sf.FilterIndex = 0;
                sf.RestoreDirectory = true;

                if (sf.ShowDialog() != DialogResult.OK) return;
                _DOCXML.Save(sf.FileName);
                
            }
        }

        private void lagreSomLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string topic_id = GetDocXMLTopicId();
            if (topic_id == "") return;
            string pkgName = SaveDocXml(topic_id);
            if (!File.Exists(pkgName)) return;
            SendPkg(pkgName,topic_id, 1);
        }

        private string GetDocXMLTopicId()
        {
            string topic_id = "";
            if (this.Tag != null)
            {
                XElement XMLDocInfo = (XElement)this.Tag;

                topic_id = XMLDocInfo.Element("topic_id") == null ? "" : XMLDocInfo.Element("topic_id").Value;
            }

            if (topic_id == "")
            {
                metods met = new metods();
                if (met.InputBox("Angi TopicId", "Angi TopicId", ref topic_id) != DialogResult.OK)
                    return "";

                if (topic_id.Length != 38 || !topic_id.StartsWith("{") || !topic_id.EndsWith("}"))
                    return "";
            }

            XAttribute tId = new XAttribute("topic_id", topic_id);
            if (_DOCXML.Attribute("topic_id") != null) _DOCXML.Attribute("topic_id").Remove();
            _DOCXML.Add(tId);
            return topic_id;

        }
        private string SaveDocXml(string topic_id)
        {
            XElement rootDoc = new XElement(_DOCXML);
            XElement content = null;
            XElement references = null;
            XElement document = null;


            //List<XElement> imgs = rootDoc.Descendants("img").ToList();
            List<XElement> imgs = rootDoc.Descendants().Where(p=>p.Attribute("src") != null).ToList();

            if (rootDoc.Name.LocalName == "root"
                && (string)rootDoc.Elements("document").Attributes("type").FirstOrDefault() == "word"
                && (string)rootDoc.Elements("document").Attributes("variant").FirstOrDefault() == "lov"
                )
            {

                return null; ;
            }
            else 
            
            foreach (XElement img in imgs)
            {
                string src = (string)img.Attributes("src").FirstOrDefault();
                if (src == null ? false :(Regex.IsMatch(src.Trim().ToLower(), @"^(\\|\/)?dibimages(\\|\/)uploads(\\|\/)") ? false : true))
                {
                    string filename = img.Attribute("src").Value;
                    string a_href = (string)img.Parent.Attributes("href").FirstOrDefault();
                    //if (filename == a_href) break;
                    filename = Regex.Split(filename, @"\\|\/").LastOrDefault();
                    if (filename != "")
                    {
                        filename = "dibimages/" + topic_id + "/" + filename;
                        img.Attribute("src").Value = filename;

                        if (img.Parent.Name.LocalName == "a")
                        {
                            img.Parent.ReplaceWith(new XElement("a",
                            new XAttribute("href", filename),
                            new XAttribute("target", "_blank"),
                            new XElement(img)
                            ));
                        }
                        else
                        {
                            img.ReplaceWith(new XElement("a",
                                new XAttribute("href", filename),
                                new XAttribute("target", "_blank"),
                                new XElement(img)
                            ));
                        }
                    }
                }

            }
            HtmlHierarchy h = null;
            if (_DOCXML.IsLevelDocument())
            {

                rootDoc.Elements("content").Remove();
                if (rootDoc.Element("references") != null)
                {
                    references = new XElement(rootDoc.Element("references"));
                    rootDoc.Element("references").Remove();
                }
                document = rootDoc;
                h = new HtmlHierarchy("level");
                content = new XElement("root", h.GetDIBXMLContent(document));

            }
            else
            {
                if (rootDoc.Element("content") != null) rootDoc.Element("content").Remove();

                if (rootDoc.Element("references") != null)
                {
                    references = new XElement(rootDoc.Element("references"));
                    rootDoc.Element("references").Remove();
                }

                if (rootDoc.Name.LocalName == "document")
                {
                    rootDoc = new XElement("documents", rootDoc);
                }


                if (rootDoc.Name.LocalName == "documents")
                {
                    XmlReader xmlreader = rootDoc.DescendantsAndSelf().Where(p =>
                                        p.Name.LocalName == "documents"
                                        ).First().CreateReader();
                    XsltArgumentList xslArg = new XsltArgumentList();

                    //string path = Path.GetDirectoryName(Application.ExecutablePath);
                    string path = global.m_Projects + @"\TransformData";
                    string xsltPath = path + @"\xsl\forarbeider2xml.xslt";



                    string outString = transformDocument.TransformXmlWithXslToString(xmlreader
                        , xsltPath, xslArg);
                    xmlreader.Close();


                    StringBuilder sb = new StringBuilder();
                    sb.Append(outString);

                    string targetFile = _TARGETDIR + "root.xml";

                    if (File.Exists(targetFile)) File.Delete(targetFile);

                    StreamWriter SW;
                    SW = File.CreateText(targetFile);
                    SW.Write(outString);
                    SW.Close();

                    XDocument newDoc = XDocument.Load(targetFile);
                    newDoc.Save(_TARGETDIR + "root.xml");
                    document = newDoc.Root;
                    h = new HtmlHierarchy("level");
                    content = new XElement("root", h.GetDIBXMLContent(document));

                    content = h.GetDIBXMLContent(document);
                    content = new XElement("nitems",
                            content.Elements().Elements("nitem")
                        );
                    document = new XElement("document",
                            new XAttribute("doctypeid", "7"),
                            document.Elements("level")
                        );
                }
            }

            XElement pkg = new XElement("pkg"
                , new XElement("pkg_content", new XElement(content))
                , new XElement("pkg_document", new XElement(document))
                , (references == null ? null : new XElement("pkg_references", new XElement(references)))
                );


            string frontMsg = "Topic Id: '" + topic_id + "' er klart for eksport!\r\n";
            
            
            if ((references == null ? 0 : references.Elements().Count()) == 0)
            {
                if (MessageBox.Show(frontMsg + "Ingen referanser til dette dokumentet, vil du fortsatt eksportere?", "Manglende referanser", MessageBoxButtons.YesNo,MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return "";
                }
            }
            else
            {
                if (MessageBox.Show(frontMsg + "Vil du eksportere dette dokumentet!", "Eksporter dokument", MessageBoxButtons.YesNo,MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return "";
                }
            }

            pkg.Save(_TARGETDIR + "pkg.xml");

            return _TARGETDIR + "pkg.xml";
        }
        private void SendPkg(string pkg_name, string topic_id, int keep = 0)
        {
            FileStream file = new FileStream(_TARGETDIR + "pkg.xml", FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(file, Encoding.ASCII);
            string XmlString = sr.ReadToEnd();
            sr.Close();
            file.Close();

            XElement test = XElement.Parse(XmlString);
            

            XmlString = Regex.Replace(XmlString, @"<\?xml[^<]+?>", "", RegexOptions.Multiline | RegexOptions.Singleline);
            XmlString = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(XmlString));

            commxml cx = new commxml();
            string sourceRegexp = "";
            if ( (string)test.Descendants("document").Attributes("type").FirstOrDefault() == "word"  
                    && (string)test.Descendants("document").Attributes("variant").FirstOrDefault() == "lov"
                )
            {
                sourceRegexp = cx.Execute("TopicMap.dbo._ImpClientImportDocument_SectionEx '" + topic_id + "', '" + XmlString + "'");
            }
            else
            {
                sourceRegexp = cx.Execute("TopicMap.dbo._ImpClientImportDocument_LevelEx '" + topic_id + "', '" + XmlString + "'," + keep.ToString());
            }
            XDocument d = XDocument.Parse(sourceRegexp);
            XElement result = d.Descendants("result").First();
            result.Save(_TARGETDIR + "result.xml");

            webBrowser1.Navigate(_TARGETDIR + "result.xml");
            //Debug.Print(d.ToString());

        }

        private void stortingetKomiteensTilrådningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "")!="-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement text = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "document"
                                || p.Name.LocalName == "section"
                                || p.Name.LocalName == "level"
                            )
                            &&
                            (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            )
                            .Elements("text").FirstOrDefault();
                    XElement parent = text.Parent;
                    XElement orginal = text;
                    if (text != null)
                    {
                        switch (text.Elements("text").Count())
                        {
                            case 0:
                                break;
                            case 1: text = text.Elements("text").First(); break;
                            default:
                                MessageBox.Show("Flere text elementer!");
                                return;
                        }
                        XElement newText = new XElement("text");
                        int i = text.Nodes().Count();
                        for (int j = 0; j < i; j++)
                        {
                            XNode node = text.Nodes().ElementAt(j);
                            if (node.NodeType == XmlNodeType.Text)
                            {
                                string elText = node.ToString();
                                if (Regex.IsMatch(elText, @"^(I|II|III|IV|V|VI|VII|VIII|IX|X|XI|XII|XIII|XIV|XV|XVI|XVII|XVIII|XIX|XX)$"))
                                {
                                    newText.Add(new XElement("h1", elText));
                                }
                                else if (Regex.IsMatch(elText, @"^((I\s+(lov|forskrift).+((endring(er|ar)?)(\s)?\:))|(lov\s+(.+)(\s+oppheves(\s+)?\.)))$", RegexOptions.Singleline))
                                {
                                    newText.Add(new XElement("h2", elText));
                                }
                                else if (Regex.IsMatch(elText, @"(.+)?((§\s+\d+)|((K|k)apittel\s+\d+))((.+)?(\s+lyde(\s+)?\:)|\s+oppheves(\s+)?\.)$"))
                                {
                                    newText.Add(new XElement("h3", elText));
                                }
                                else
                                {
                                    newText.Add(new XElement("p", node));
                                }

                            }
                            else if (node.NodeType == XmlNodeType.Element)
                            {
                                XElement el = node as XElement;
                                string elText = el.DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim();
                                if (Regex.IsMatch(elText, @"^(I|II|III|IV|V|VI|VII|VIII|IX|X|XI|XII|XIII|XIV|XV|XVI|XVII|XVIII|XIX|XX)$"))
                                {
                                    newText.Add(new XElement("h1", elText));
                                }
                                else if (Regex.IsMatch(elText, @"^((I\s+(lov|forskrift).+((endring(er|ar)?)(\s)?\:))|(lov\s+(.+)(\s+oppheves(\s+)?\.)))$", RegexOptions.Singleline))
                                {
                                    newText.Add(new XElement("h2", elText));
                                }
                                else if (elText.Length > 255 ? false : Regex.IsMatch(elText, @"(.+)?((§\s+\d+)|((K|k)apittel\s+\d+))((.+)?(\s+lyde(\s+)?\:)|\s+oppheves(\s+)?\.)$"))
                                {
                                    newText.Add(new XElement("h3", elText));
                                }
                                else
                                {
                                    newText.Add(new XElement(el));
                                }
                            }
                        }

                        frmEdit f = new frmEdit(newText);
                        f.ShowDialog(this);
                        if (f.DialogResult == DialogResult.OK)
                        {
                            XElement newtext = XElement.Parse("<text>" + f.returnHtml + "</text>");
                            RebuildPart(parent, orginal, newtext);
                            //XElement returnEl = newtext.BuildLevelsFromHeader(parent.Attribute("id").Value);
                            //if (parent.Name.LocalName == "level") returnEl = returnEl.ReplaceSections("level");
                            //if (returnEl.Name.LocalName == "text")
                            //{
                            //    orginal.ReplaceWith(new XElement(returnEl));
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                            //    LoadDataSet();
                            //}
                            //else
                            //{
                            //    orginal.ReplaceWith(returnEl.Nodes());
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));
                            //    LoadDataSet();
                            //}
                        }
                    }

                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
            }
        }

        private void stortingetLovforslagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "")!="-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement text = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "document"
                                || p.Name.LocalName == "section"
                                || p.Name.LocalName == "level"
                            )
                            &&
                            (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            ).Elements("text").FirstOrDefault();
                    if (text == null)
                    {
                        MessageBox.Show("Elementet mangler 'text-element'!");
                        return;
                    }
                    XElement parent = text.Parent;
                    XElement orginal = text;
                    if (text != null)
                    {
                        switch (text.Elements("text").Count())
                        {
                            case 0:
                                break;
                            case 1: text = text.Elements("text").First(); break;
                            default:
                                MessageBox.Show("Flere text elementer!");
                                return;
                        }
                        XElement newText = new XElement("text");
                        int i = text.Nodes().Count();
                        for (int j = 0; j < i; j++)
                        {
                            XNode node = text.Nodes().ElementAt(j);
                            if (node.NodeType == XmlNodeType.Text)
                            {
                                string elText = node.ToString();
                                if (Regex.IsMatch(elText, @"^(Kapittel\s+\d+)(\s|\.|$)"))
                                {
                                    newText.Add(new XElement("h1", elText));
                                }
                                else if (Regex.IsMatch(elText, @"^(I\s+)?(§\s+\d+([a-z])?((\-\d+([a-z])?)+)?)"))
                                {
                                    newText.Add(new XElement("h2", elText));
                                }
                                else
                                {
                                    newText.Add(new XElement("p", node));
                                }

                            }
                            else if (node.NodeType == XmlNodeType.Element)
                            {
                                XElement el = node as XElement;
                                string elText = el.DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim();
                                if (Regex.IsMatch(elText, @"^(Kapittel\s+\d+)(\s|\.|$)"))
                                {
                                    newText.Add(new XElement("h1", elText));
                                }
                                else if (Regex.IsMatch(elText, @"^(I\s+)?(§\s+\d+([a-z])?((\-\d+([a-z])?)+)?)"))
                                {
                                    newText.Add(new XElement("h2", elText));
                                }
                                else
                                {
                                    newText.Add(new XElement(el));
                                }
                            }
                        }

                        frmEdit f = new frmEdit(newText);
                        f.ShowDialog(this);
                        if (f.DialogResult == DialogResult.OK)
                        {
                            XElement newtext = XElement.Parse("<text>" + f.returnHtml + "</text>");
                            RebuildPart(parent, orginal, newtext);
                            //XElement returnEl = newtext.BuildLevelsFromHeader(parent.Attribute("id").Value);
                            //if (parent.Name.LocalName == "level") returnEl = returnEl.ReplaceSections("level");
                            //if (returnEl.Name.LocalName == "text")
                            //{
                            //    orginal.ReplaceWith(new XElement(returnEl));
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                            //    LoadDataSet();
                            //}
                            //else
                            //{
                            //    orginal.ReplaceWith(returnEl.Nodes());
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));
                            //    LoadDataSet();
                            //}
                        }
                    }
                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
            }
        }

        private void GetSectionTitle(XElement top, ref XElement newSection, string parentText, ExternalStaticLinksEx1 el)
        {
            string inparentText = parentText;

            foreach (XElement s in top.Elements().Where(p=>
                p.Name.LocalName == "section"
                || p.Name.LocalName == "level"
                ))
            {
                string title = s.Element("title").Value.Trim().ToLower();
                title = StripSectionTitle(title);
                XElement newS = null;
                string idTitle = IdetifyTitle(title, el); 
                if (idTitle != "" )
                {
                    parentText = idTitle;
                    newS = new XElement("section", s.Attribute("id"), new XElement("content", idTitle));
                }
                else
                {
                    if (Regex.IsMatch(title, @"^(I|II|III|IV|V|VI|VII|VIII|IX|X|XI|XII|XIII|XIV|XV|XVI|XVII|XVIII|XIX|XX)$"))
                    {
                        newS = new XElement("section", s.Attribute("id"), new XElement("content", title));
                    }
                    else
                    {
                        newS = new XElement("section", s.Attribute("id"), new XElement("content", (parentText == "" ? "" : parentText + " ") + title));
                    }
                }

                GetSectionTitle(s, ref newS, parentText, el);
                newSection.Add(newS);
                if (parentText == title)
                    parentText = inparentText;
            }
        }

        private string StripSectionTitle(string title)
        {
            title = Regex.Replace(title.ToLower(), @"\s+gjøres\s+følgende\s+endring(ene|en|ar|er)?(\s+)?:(\s+)?$", "");
            title = Regex.Replace(title.ToLower(), @"\s+skal\s+lyde(\s+)?:(\s+)?$", "");
            title = Regex.Replace(title.ToLower(), @"^nåværende\s+§", "§");
            title = Regex.Replace(title.ToLower(), @"^til\s+endring(ene|en|er|ar)?\s+i\s+lov\s+", "lov ");
            title = Regex.Replace(title.ToLower(), @"^til\s+endring(ene|en|er|ar)?\s+i\s+forskrift\s+", "forskrift ");
            title = Regex.Replace(title.ToLower(), @"^til\s+((endring(ene|en|er|ar)?\s+i)|(nytt))((?!§)([^\.§\(]))*", "");
            title = Regex.Replace(title.ToLower(), @"^til\s+(opphevelsen\s+av\s+)?§", "§");
            title = Regex.Replace(title.ToLower(), @"^til\s+kapitteloverskrift(a|en)\s+", "");
            title = Regex.Replace(title.ToLower(), @"^overskrifta\s+til\s+kapittel", "kapittel");
            title = Regex.Replace(title.ToLower(), @"^ny\s+§", "§");
            title = Regex.Replace(title.ToLower(), @"^i\s+(ny\s+)?§", "§");
            title = Regex.Replace(title.ToLower(), @"\sblir\s+ny\s+§", ", §");
            title = Regex.Replace(title.ToLower(), @"\s+skal\s+(ny\s+)?§", " §");
            title = Regex.Replace(title.ToLower(), @"(\.)(\s+)?§", ", §");
            title = Regex.Replace(title.ToLower(), @"(\s+nytt\s+)", " ");
            title = Regex.Replace(title.ToLower(), @"(?<=((§\s+(\d+([a-z])?))(((\-)(\d+)([a-z])?)+)?))(\s+)(?=(([a-h])\s([a-zæøå]+)|§))", "");
            title = Regex.Replace(title.ToLower(), @"\s+\skal(\s+ny)?\s+§", " §");
            title = Regex.Replace(title.ToLower(), @"(:|;)(\s+)?$", "");
            title = Regex.Replace(title.ToLower(), @"(:|;)(\s+)?$", "");

            title = Regex.Replace(title.ToLower(), @"§(\s+)?\d+((\s+)(?!([a-zæøå][a-zæøå]+)))?(([a-z])(?![a-zæøå]))?(((\s+)(?=\-))?(\-)((\s+)(?=\d+))?\d+)?"
                        ,
                        delegate(Match m)
                        {
                            string text = m.Value.Replace(" ", "");
                            return text;
                        }
                        , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return title;

        }            

        private string IdetifyTitle(string title, ExternalStaticLinksEx1 el)
        {

            XElement content = new XElement("content", new XElement("p", title));
            XElement result = el.GetExternalStaticLinks(content,"no");
            List<XElement> lEl = result
                                .Descendants("a")
                                .Where(p=>
                                       (p.Attribute("class")==null ? "" : p.Attribute("class").Value)=="xref" 
                                    && p.Ancestors("span").Where(q=>
                                                        (q.Attribute("class") == null ? "" : q.Attribute("class").Value) == "hn xreflist")
                                                       .Count()==0
                                 ).ToList();
            
            
            if (lEl.Count == 1)
            {
                if (lEl.ElementAt(0).Attribute("data-xreflist") != null)
                {
                    return "";
                }
                else if (lEl.ElementAt(0).Attribute("data-bm") == null)
                {
                    return lEl.ElementAt(0).Value;
                }
                else
                {
                    if (Regex.IsMatch(title, @"(^(\d+((.\d+)+)?(\s+)?)?((((forslag|merknad(er|ar))\s+)?til\s+)?endring(er|ar|ane)\s+)?(i\s+([a-zæøå]+lov(en|a)|lov|forskrift))|^(i\s+)?lov\s+om)|^(\d+((.\d+)+)?(\s+)?)?([a-zæøå]+loven|lov(\s+av)?\s+\d+(\.)?\s+[a-zæøå]+(\.)?\s+\d+)"))
                    {
                        return title;
                    }
                }
            }
            else if (lEl.Count > 0)
            {
                List<string> sl = lEl.Select(p => p.Value).ToList();
                Form f = new frmListSelect(sl);
                if (f.ShowDialog() == DialogResult.OK)
                {
                    return f.Tag as string;
                }
                else
                {
                    if (Regex.IsMatch(title, @"(^(\d+((.\d+)+)?(\s+)?)?((((forslag|merknad(er|ar))\s+)?til\s+)?endring(er|ar|ane)\s+)?(i\s+([a-zæøå]+lov(en|a)|lov|forskrift))|^(i\s+)?lov\s+om)|^(\d+((.\d+)+)?(\s+)?)?([a-zæøå]+loven|lov(\s+av)?\s+\d+(\.)?\s+[a-zæøå]+(\.)?\s+\d+)"))
                    {
                        return title;
                    }
                    else
                        return "";
                }
            }

            if (Regex.IsMatch(title, @"(^(\d+((.\d+)+)?(\s+)?)?((((forslag|merknad(er|ar))\s+)?til\s+)?endring(er|ar|ane)\s+)?(i\s+([a-zæøå]+lov(en|a)|lov|forskrift))|^(i\s+)?lov\s+om)|^(\d+((.\d+)+)?(\s+)?)?([a-zæøå]+loven|lov(\s+av)?\s+\d+(\.)?\s+[a-zæøå]+(\.)?\s+\d+)"))
            {
                return title;
            }
            
            return "";
        }

        private void inndelingLovParagrafToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "")!="-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement top = GetContentElement(c.ID);
                    string title = top.Element("title").Value.Trim().ToLower();

                    //ExtendedStaticLinks el = new ExtendedStaticLinks(0);
                    ExternalStaticLinksEx1 el = new ExternalStaticLinksEx1(0);


                    title = StripSectionTitle(title);

                    string parentText = "";
                    string idTitle = IdetifyTitle(title, el);
                    if (idTitle != "")
                    {
                        parentText = idTitle;
                    }

                    XElement returnS = null;
                    if (parentText != "")
                    {
                        returnS = new XElement("section", top.Attribute("id"), new XElement("content", title));

                    }
                    else
                    {
                        returnS = new XElement("section", top.Attribute("id"));
                    }

                    GetSectionTitle(top, ref returnS, parentText, el);



                    XElement sectionsId = el.GetExternalStaticLinks(returnS, "no");
                    Debug.Print(sectionsId.ToString());
                    XElement references = null;
                    if (_DOCXML.Element("references") == null)
                    {
                        _DOCXML.AddFirst(new XElement("references"));
                    }
                    references = _DOCXML.Elements("references").First();

                    AddReferences(sectionsId, references);

                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
            }
            treeView1.UseWaitCursor = false;
        }


        private void AddReferences(XElement sectionsId, XElement references)
        {
            foreach (XElement a in sectionsId.DescendantsAndSelf("a"))
            {
                if (a.Attributes().Where(p=>p.Name.LocalName.StartsWith("data-tid")).Count() != 0)
                {
                    string to_bm = a.Ancestors("section").Attributes("id").First().Value;
                    string from_id = a.Attributes().Where(p=>p.Name.LocalName.StartsWith("data-tid")).First().Value;
                    string from_bm = a.Attribute("data-bm") == null ? "" : a.Attribute("data-bm").Value;
                    if (references
                        .Elements("reference")
                        .Where(p =>
                            (p.Attribute("from_id") == null ? "" : p.Attribute("from_id").Value) == from_id
                            && (p.Attribute("from_bm") == null ? "" : p.Attribute("from_bm").Value) == from_bm
                            && (p.Attribute("to_bm") == null ? "" : p.Attribute("to_bm").Value) == to_bm
                            )
                        .Count() == 0)
                    {
                        if (!(from_bm == "" && a.Parent.Elements("a").Count() > 1 && a != a.Parent.Elements("a").First()))
                        {
                            references.Add(new XElement("reference"
                                , new XAttribute("from_id", from_id)
                                , (from_bm == "" ? null : new XAttribute("from_bm", from_bm))
                                , new XAttribute("to_bm", to_bm)
                                ));
                        }
                    }
                }
            }
            TreeNode n = treeView1.Nodes[0];
            PaintTreeNodes(n, references);
            
            //Debug.Print(references.ToString());
        }
        
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void lavereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "-1") != "-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement top = GetContentElement(c.ID);
                    if (top != null)
                    {
                        if (top.NodesBeforeSelf().Where(p => p.HasLevelOrSections()).Count() != 0)
                        {
                            XElement parent = (XElement)top.NodesBeforeSelf().Where(p => p.HasLevelOrSections()).Last();
                            parentId = parent.Attribute("id").Value;
                            XElement _top = new XElement(top);
                            top.Remove();
                            parent.Add(_top);
                            if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                            Load_DOCXML();// LoadDataSet();
                        }
                    }
                }

                treeView1.UseWaitCursor = false;
                if (parentId != string.Empty)
                {
                    TreeNode node = treeView1.Nodes[0];
                    FindTreeNode(node, parentId);
                }
            }
        
        }

        private void Kollaps(bool concattitle)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "-1") != "-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement top = GetContentElement(c.ID);
                    if (top != null)
                    {
                        if ((top.NodesBeforeSelf().Where(p => p.HasLevelOrSections()).Count() == 0 ? (top.Parent.Attribute("id")==null ? "-1" : top.Parent.Attribute("id").Value) : "-1")!= "-1")
                        {
                            XElement parent = (XElement)top.Parent;
                            if ((top.Element("title") == null ? 0 : top.Element("title").Nodes().Count()) != 0)
                            {
                                if (concattitle)
                                    parent.Elements("title").First().Add(new XText(" "), top.Element("title").Nodes());
                                else if (parent.Elements("text").Count() != 0)
                                    parent.Elements("text").First().Add(new XElement("p", new XElement("strong", top.Element("title").Nodes())));

                                top.Elements("title").Remove();
                            }
                            if ((top.Element("text") == null ? 0 : top.Element("text").Nodes().Count()) != 0)
                            {

                                parent.Elements("text").First().Add(top.Element("text").Nodes());
                                top.Elements("text").Remove();
                            }
                            if (top.Elements().Count() != 0)
                            {
                                XElement container = new XElement("container", top.Nodes());
                                top.AddBeforeSelf(container.Nodes());
                            }
                            top.Remove();
                        }
                        else if (top.NodesBeforeSelf().Where(p => p.HasLevelOrSections()).Count() != 0)
                        {
                            XElement parent = (XElement)top.NodesBeforeSelf().Where(p => p.HasLevelOrSections()).Last();
                            parentId = top.Elements().Where(p => p.HasLevelOrSections()).FirstOrDefault() != null ? top.Elements().Where(p => p.HasLevelOrSections()).FirstOrDefault().Attribute("id").Value : parent.Attribute("id").Value;
                            if ((top.Element("title") == null ? 0 : top.Element("title").Nodes().Count()) != 0)
                            {
                                if (concattitle)
                                    parent.Elements("title").First().Add(new XText(" "), top.Element("title").Nodes());
                                else if (parent.Elements("text").Count() != 0)
                                    parent.Elements("text").First().Add(new XElement("p", new XElement("strong", top.Element("title").Nodes())));
                               
                                top.Elements("title").Remove();
                            }
                            if ((top.Element("text") == null ? 0 : top.Element("text").Nodes().Count()) != 0)
                            {
                                
                                parent.Elements("text").First().Add(top.Element("text").Nodes());
                                top.Elements("text").Remove();
                            }
                            if (top.Elements().Count() != 0)
                            {
                                parent.AddAfterSelf(top.Nodes());
                            }
                            top.Remove();
                        }
                        else if (top.NodesAfterSelf().Where(p => p.HasLevelOrSections()).Count() != 0)
                        {
                            XElement parent = (XElement)top.NodesAfterSelf().Where(p => p.HasLevelOrSections()).First();
                            parentId = top.Elements().Where(p => p.HasLevelOrSections()).FirstOrDefault() != null ? top.Elements().Where(p => p.HasLevelOrSections()).FirstOrDefault().Attribute("id").Value : parent.Attribute("id").Value;
                            if ((top.Element("title") == null ? 0 : top.Element("title").Nodes().Count()) != 0)
                            {
                                if (concattitle)
                                    parent.Elements("title").First().Add(new XText(" "), top.Element("title").Nodes());
                                else
                                    parent.Elements("text").First().Add(new XElement("p", new XElement("strong", top.Element("title").Nodes())));
                                top.Elements("title").Remove();
                            }
                            if ((top.Element("text") == null ? 0 : top.Element("text").Nodes().Count()) != 0)
                            {
                                parent.Elements("text").First().Add(top.Element("text").Nodes());
                                top.Elements("text").Remove();
                            }
                            if (top.Elements().Count() != 0)
                            {
                                parent.AddBeforeSelf(top.Nodes());
                            }
                            top.Remove();
                        }
                        else if (top.Parent.DescendantsAndSelf().Where(p =>
                                            p.Name.LocalName == "section"
                                            || p.Name.LocalName == "level"
                                    ).Count() != 0)
                        {
                            XElement parent = top.Parent;
                            if (parent.Name.LocalName == "documents" || parent.Attribute("id") == null)
                            {
                                parent.Add(top.Elements().Where(p =>
                                                    p.Name.LocalName == "section"
                                                    || p.Name.LocalName == "level"
                                                    || p.Name.LocalName == "document"));
                                top.Remove();
                            }
                            else
                            {
                                parentId = parent.Attribute("id").Value;
                                if ((top.Element("title") == null ? 0 : top.Element("title").Nodes().Count()) != 0)
                                {
                                    if (concattitle)
                                        parent.Elements("title").First().Add(new XText(" "), top.Element("title").Nodes());
                                    else
                                        parent.Elements("text").First().Add(new XElement("p", new XElement("strong", top.Element("title").Nodes())));
                                    top.Elements("title").Remove();
                                }
                                if ((top.Element("text") == null ? 0 : top.Element("text").Nodes().Count()) != 0)
                                {
                                    
                                    parent.Elements("text").First().Add(top.Element("text").Nodes());
                                    top.Elements("text").Remove();
                                }

                                if (top.Elements().Count() != 0)
                                {
                                    parent.Add(top.Nodes());
                                }
                                top.Remove();
                            }

                        }

                    }
                }
                if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                Load_DOCXML(); // LoadDataSet();


                treeView1.UseWaitCursor = false;
                if (parentId != string.Empty)
                {
                    TreeNode node = treeView1.Nodes[0];
                    FindTreeNode(node, parentId);
                }
            }
        }
        
        private void kollapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Kollaps(false);
        }


        private void PaintTreeNodes(TreeNode n, XElement references)
        {

            for (int i = 0; i < n.Nodes.Count; i++)
            {
                TreeNode m = n.Nodes[i];
                m.BackColor = Color.White;
                Category c = (Category)m.Tag;
                if (references.Elements("reference").Where(p => p.Attribute("to_bm").Value == c.ID).Count() != 0)
                {
                    
                    m.BackColor = Color.LightPink;
                    m.Parent.Expand();
                    m.Expand();
                }
                if (m.Nodes.Count != 0)
                {
                    PaintTreeNodes(m, references);
                }
            }
        }
        
        
        private bool FindTreeNode(TreeNode n, string id)
        {
            for (int i = 0; i < n.Nodes.Count; i++)
            {
                TreeNode m = n.Nodes[i];
                Category c = (Category)m.Tag;
                if (id == c.ID)
                {
                    treeView1.SelectedNode = m;
                    treeView1.SelectedNode.ExpandAll();
                    ShowDocument(m);
                    return true;
                }
                else if (m.Nodes.Count != 0)
                {
                    bool result = FindTreeNode(m, id);
                    if (result) return true;
                }
            }
            return false;
        }
        private void FindTreeNode(string id)
        {
            if (id != string.Empty)
            {
                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    TreeNode n =  treeView1.Nodes[i];
                    Category c = (Category)n.Tag;
                    if (id == c.ID)
                    {
                        treeView1.SelectedNode = n;
                        treeView1.SelectedNode.ExpandAll();
                        ShowDocument(n);
                        return;
                    }
                    else if (n.Nodes.Count != 0)
                    {
                        bool result = FindTreeNode(n, id);
                        if (result) return;
                    }

                }
            }
        }


        private XElement GetContentElement(string id)
        {
            if (_DOCXML == null) return null;
            return _DOCXML.Descendants().Where(p =>
                (
                    p.Name.LocalName == "document"
                    || p.Name.LocalName == "section"
                    || p.Name.LocalName == "level"
                )
                &&
                (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == id
                ).FirstOrDefault();

        }

        private Category GetTreeViewElement(TreeView t)
        {
            Category returnValue = null;
            if (t.SelectedNode != null)
            {
                this.Refresh();
                TreeNode n = t.SelectedNode;
                returnValue  = (Category)n.Tag;
            }
            return returnValue;
        }

        private void høyereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "")!="-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement top = GetContentElement(c.ID);
                    if (top != null)
                    {
                        if (top.Parent != null)
                        {
                            XElement parent = top.Parent;
                            parentId = parent.Attribute("id").Value;
                            XElement _top = new XElement(top);
                            top.Remove();
                            parent.AddAfterSelf(_top);
                        }
                        if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                        _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                        Load_DOCXML();// LoadDataSet();
                    }
                }
                
                if (parentId != string.Empty)
                {
                    TreeNode node = treeView1.Nodes[0];
                    FindTreeNode(node, parentId);
                }
                treeView1.UseWaitCursor = false;
            }
        }

        private void angiLovnavnFinnParagrafToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "-1") != "-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement top = GetContentElement(c.ID); 

                    string title = top.Element("title").Value.Trim().ToLower();
                    title = StripSectionTitle(title);

                    string parentText = "";
                    string value = "lovnavn eller lovdato";
                    metods met = new metods();
                    if (met.InputBox("Angi lov/forskrift for identifisering", "Lov/forskrift navn:", ref value) == DialogResult.OK)
                    {
                        parentText = value;
                    }
                    else
                    {
                        return;
                    }

                    //ExtendedStaticLinks el = new ExtendedStaticLinks(0);
                    ExternalStaticLinksEx1 el = new ExternalStaticLinksEx1(0);



                    XElement returnS = null;
                    if (parentText != "")
                    {
                        returnS = new XElement("section", top.Attribute("id"), new XElement("content", title));

                    }
                    else
                    {
                        returnS = new XElement("section", top.Attribute("id"));
                    }


                    GetSectionTitle(top, ref returnS, parentText, el);



                    XElement sectionsId = el.GetExternalStaticLinks(returnS, "no");
                    Debug.Print(sectionsId.ToString());
                    XElement references = null;
                    if (_DOCXML.Element("references") == null)
                    {
                        _DOCXML.AddFirst(new XElement("references"));
                    }
                    references = _DOCXML.Elements("references").First();

                    AddReferences(sectionsId, references);

                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
            }
            treeView1.UseWaitCursor = false;

        }

        private void nivåToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void slettReferanserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML.Element("references") != null) _DOCXML.Element("references").Remove();
            _DOCXML.Save(_FILENAME);
        }

        private void indentifiserToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void forsøkSeksjoneringToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void referanserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML.Element("references") != null)
            {
                XElement references = _DOCXML.Element("references");
                TreeNode n = treeView1.Nodes[0];
                PaintTreeNodes(n, references);
            }
        }

        private void SetMerknaderTilLevel(string htmlHeader)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "")!="-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement text = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "document"
                                || p.Name.LocalName == "section"
                                || p.Name.LocalName == "level"
                            )
                            &&
                            (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            ).Elements("text").FirstOrDefault();
                    XElement parent = text.Parent;
                    XElement orginal = text;
                    if (text != null)
                    {
                        switch (text.Elements("text").Count())
                        {
                            case 0:
                                break;
                            case 1: text = text.Elements("text").First(); break;
                            default:
                                MessageBox.Show("Flere text elementer!");
                                return;
                        }
                        XElement newText = new XElement("text");
                        int i = text.Nodes().Count();
                        for (int j = 0; j < i; j++)
                        {
                            XNode node = text.Nodes().ElementAt(j);
                            if (node.NodeType == XmlNodeType.Text)
                            {
                                string elText = node.ToString();
                                if (Regex.IsMatch(elText, @"^(T|t)il\s+(§|(K|k)apit(t)?el\s+\d+)"))
                                {
                                    newText.Add(new XElement(htmlHeader, elText));
                                }
                                else
                                {
                                    newText.Add(new XElement("p", node));
                                }

                            }
                            else if (node.NodeType == XmlNodeType.Element)
                            {
                                XElement el = node as XElement;
                                string elText = el.DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim();
                                if (Regex.IsMatch(elText, @"^(T|t)il\s+(§|(K|k)apit(t)?el\s+\d+)"))
                                {
                                    newText.Add(new XElement(htmlHeader, elText));
                                }
                                else
                                {
                                    newText.Add(new XElement(el));
                                }
                            }
                        }

                        frmEdit f = new frmEdit(newText);
                        f.ShowDialog(this);
                        if (f.DialogResult == DialogResult.OK)
                        {
                            XElement newtext = XElement.Parse("<text>" + f.returnHtml + "</text>");
                            RebuildPart(parent, orginal, newtext);
                            //XElement returnEl = newtext.BuildLevelsFromHeader(parent.Attribute("id").Value);
                            //if (parent.Name.LocalName == "level") returnEl = returnEl.ReplaceSections("level");
                            //if (returnEl.Name.LocalName == "text")
                            //{
                            //    orginal.ReplaceWith(new XElement(returnEl));
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                            //    LoadDataSet();
                            //}
                            //else
                            //{
                            //    orginal.ReplaceWith(returnEl.Nodes());
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));
                            //    LoadDataSet();
                            //}
                        }
                    }

                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
                
            }

        }

        private void oppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            høyereToolStripMenuItem_Click(sender, e);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lavereToolStripMenuItem_Click(sender, e);
        }

        private void kollapsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            kollapsToolStripMenuItem_Click(sender, e);
        }

        private class InTextObject
        {
            public string id { get; set; }
            public XElement text { get; set; }
        }

        private void punkt000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<InTextObject> ito = _DOCXML
                                      .Descendants("text")
                                      .Where(p => p.Value.Trim() != "")
                                      .Select(p =>
                                        new InTextObject
                                        {
                                            id = p.Parent.Attribute("id").Value
                                            ,
                                            text = p
                                        }
                                        )
                                        .ToList();


        }

        private void h1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMerknaderTilLevel("h1");
        }

        private void h2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMerknaderTilLevel("h2");
        }

        private void h3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMerknaderTilLevel("h3");
        }

        private void kopierIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "-1") != "-1")
            {
                    String replacementHtmlText = c.ID;
                    Clipboard.SetText(replacementHtmlText, TextDataFormat.Text);
            }
        }

        private void åpneTopicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";
            string topic_id = "";
            metods met = new metods();
            met.InputBox("Åpne Topic", "Tast inn TopicId", ref topic_id);
            topic_id = topic_id.Trim();
            if (topic_id.StartsWith("{") && topic_id.EndsWith("}"))
            {
                commxml cx = new commxml();
                string sourceRegexp = cx.Execute("TopicMap.dbo._ImpClientGetDocumentXML '" + topic_id + "'");
                XDocument d = XDocument.Parse(sourceRegexp);
                if (d.Descendants("result").Count() == 0) return;
                XElement result = d.Descendants("result").First();
                if (result.Descendants().Where(p => 
                       p.Name.LocalName == "root" 
                    || p.Name.LocalName == "document").Count() == 0
                ) return;

                ResetTag();
                this.Tag = new XElement("document",
                  new XElement("topic_id", topic_id)
                  );
                _DOCXML = new XElement(result.Descendants().Where(p => p.Name.LocalName == "root" || p.Name.LocalName == "document").First());

                Load_DOCXML();
                _TOPIC_ID = topic_id;
            }
        }

        private void åpneHåndbokToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";
            _TOPIC_ID = "";
            OpenFileDialog oF  = new OpenFileDialog();
            oF.Filter = "XML filer (*.xml)|*.xml";
            if (oF.ShowDialog() == DialogResult.OK)
            {
                string fileName = oF.FileName;
                try
                {
                    XElement d = XElement.Load(fileName);
                    if (d.Descendants("documents").Where(p =>
                        (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "handbook"
                        && (p.Attribute("variant") == null ? "" : p.Attribute("variant").Value) == "abc"
                        ).Count() == 1)
                    {
                        _DOCXML = d;
                        _TARGETDIR = Path.GetTempPath();
                        LoadDataSet();
                    }
                    else if (d.Descendants("documents").Elements("document").Attributes("dibt").Count() != 0)
                    {
                        _DOCXML = d;
                        _TARGETDIR = Path.GetTempPath();
                        LoadDataSet();
                    }


                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void LoadHtml(XElement d)
        {
            try
            {
               

                XElement returnEl = d.BuildLevelsFromHeader("new");
                d = new XElement("document", returnEl.Nodes());
                
                _DOCXML = d;
                LoadDataSet();

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void åpneHtmlfilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";

            OpenFileDialog oF = new OpenFileDialog();
            oF.Filter = "Html filer (*.htm;*.html)|*.htm; *.html";
            if (oF.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(oF.FileName);
                Environment.CurrentDirectory = fi.DirectoryName;
                _FILENAME = "";
                string fileName = oF.FileName;

                Encoding en = Encoding.UTF8; //
                //Encoding en = Encoding.GetEncoding(1252);//Encoding.UTF8;
                FileStream file = new FileStream(oF.FileName, FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader sr = new StreamReader(file, en);
                string htmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                string err = "";
                string strHtml = global.ConvertTidy(htmlString, en, ref err);
                strHtml = strHtml.CleanHtmlText();
                XElement d = null;
                if (strHtml != "")
                {
                    XDocument dc = XDocument.Parse(strHtml);
                    dc.Save(oF.FileName);
                    d = dc.Root;// XElement.Parse(strHtml);
                    d.Descendants("div").Where(p => p.Nodes().OfType<XText>().Where(q => q.ToString().Trim() != "").Count() == 0).ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                }
                else
                {
                    d = XElement.Load(oF.FileName);
                }
                
                LoadHtml(d);
            }

        }

        private void konverterMSWordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";
            frmConvert f = new frmConvert();
            f.ShowDialog(this);
            if (f.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string path = f._PATH;
                _TARGETDIR = f._PATH;
                _FILENAME = "";
                XElement document = XElement.Load(f._PATH + "document.xml");
                XElement content = XElement.Load(f._PATH + "content.xml");
                document.AddFirst(new XElement("content", content.Nodes()));
                LoadDataSetLevel(document);
            }
        }

        private void samleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;
            Dictionary<string, string> regexp = global.BuildRegexp(global.m_InternalDocumentRegexpFile);
            if (regexp == null) return;

            Category c = (Category)n.Tag;
            XElement section = SelectTreeSection(c);
            if (section == null) return;
            bool stop = false;
            bool found = false;
            while (!stop)
            {
                found = false;
                foreach (XElement el in section.DescendantsAndSelf().Where(p =>
                                (p.Name.LocalName == "section"
                                ||
                                p.Name.LocalName == "level")
                                && p.Element("text") != null))
                {
                    XElement text = el.Elements("text").FirstOrDefault();
                    if (text != null)
                    {
                        XElement p = text.Elements("p").Where(q => 
                            q.Attribute("concatp")==null
                            && Regex.IsMatch((string)q.DescendantNodes().OfType<XText>().Select(s => (string)s.ToString()).StringConcatenate(), @"^(\s+)?(\,)?(\s+)?[a-zæøå]")).FirstOrDefault();
                        if (p != null)
                        {
                            if ((p.PreviousNode != null ?
                                    (p.PreviousNode.NodeType == XmlNodeType.Element ?
                                        ((XElement)p.PreviousNode).Name.LocalName == "p" ?
                                            Regex.IsMatch(((XElement)p.PreviousNode).DescendantNodes().OfType<XText>().Select(s => (string)s.ToString()).StringConcatenate(), @"([a-zæøå]+(\s+)|[a-zæøå]+)")
                                            : false
                                        : false)
                                    : false) != false)
                            {
                                found = true;
                                string s1 = ((XElement)p.PreviousNode).DescendantNodes().OfType<XText>().Select(s => (string)s.ToString()).StringConcatenate();
                                string s2 = p.DescendantNodes().OfType<XText>().Select(s => (string)s.ToString()).StringConcatenate();
                                ShowPart(el);
                                this.Refresh();
                                DialogResult x = MessageBox.Show(this, s1 + Environment.NewLine + " + " + Environment.NewLine + s2, "Skal setninger samles?", MessageBoxButtons.YesNoCancel);
                                if (x == DialogResult.Yes)
                                {
                                    XElement concatN = (XElement)p.PreviousNode;
                                    concatN.Add(p.Nodes());
                                    p.Remove();
                                    break;
                                }
                                else if (x == DialogResult.No)
                                {
                                    p.Add(new XAttribute("concatp", "1"));
                                }
                                else if (x == DialogResult.Cancel)
                                {
                                    stop = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!found) stop = true;

            }
        }

        private void Load_DOCXML()
        {
            _TOPIC_ID = "";
            if (_DOCXML == null) return;
            if ((_DOCXML.DescendantsAndSelf("document").Count() == 0 ? ""
                :
                (_DOCXML.DescendantsAndSelf("document").First().Attribute("doctypeid") == null ?
                "" : _DOCXML.DescendantsAndSelf("document").First().Attribute("doctypeid").Value)) == "7")
            {
                if (_DOCXML.DescendantsAndSelf("content").Count() != 0) _DOCXML.DescendantsAndSelf("content").First().Remove();
                HtmlHierarchy h = new HtmlHierarchy("level");
                XElement content = new XElement("content", h.GetDIBXMLContent(_DOCXML));
                _DOCXML.AddFirst(content);
                LoadDataSetLevel(_DOCXML);
            }
            else
            {
                if (_DOCXML.DescendantsAndSelf("content").Count() != 0) _DOCXML.DescendantsAndSelf("content").First().Remove();
                LoadDataSet();
            }

        }

        private void åpneXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";
            OpenFileDialog oF = new OpenFileDialog();
            oF.Filter = "XML filer (*.xml)|*.xml";
            if (oF.ShowDialog() == DialogResult.OK)
            {
                string fileName = oF.FileName;
                FileInfo  fi = new FileInfo(fileName);
                XDocument dd = XDocument.Load(fileName);
                _DOCXML = dd.Root;//XElement.Load(fileName);
                _DOCXML.DescendantNodes().OfType<XText>().ToList().ForEach(p=>p.ReplaceWith(Regex.Replace(p.Value,@"\s+|\r\n", " ")));

                this.Tag = null;

                if (_DOCXML.Attribute("topic_id") != null)
                this.Tag = new XElement("document",
                    new XElement("topic_id", _DOCXML.Attribute("topic_id").Value)
                    );
                
                _TARGETDIR = fi.FullName.Replace(fi.Name, "");
                _FILENAME = fi.Name;
                if ((_DOCXML.DescendantsAndSelf("document").Count() == 0 ? "" 
                    : 
                    (_DOCXML.DescendantsAndSelf("document").First().Attribute("doctypeid") == null ? 
                    "" : _DOCXML.DescendantsAndSelf("document").First().Attribute("doctypeid").Value))=="7")
                {

                    _DOCXML.Descendants("level").Where(p => (string)p.Attributes("class").FirstOrDefault() == "lovdata-ledd").ToList().ForEach(p => p.ReplaceWith(new XElement("p", p.Attributes("id"), p.Nodes())));
                    _DOCXML.Descendants("span").Where(p => (string)p.Attributes("class").FirstOrDefault() == "lovdata_punktum").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                    _DOCXML.Descendants("a").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                    _DOCXML.Descendants("p").Where(p=>p.Parent.Name.LocalName=="p").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                    _DOCXML.Descendants("p").Attributes("class").ToList().ForEach(p => p.Remove());
                    

                    List<XElement> level = _DOCXML.Descendants("level").Where(p => p.Elements("title").FirstOrDefault() != null).Reverse().ToList();
                    foreach(XElement e1 in level)
                    {
                        List<XElement> n = e1.Elements().Skip(1).TakeWhile(p => !level.Contains(p)).ToList();
                        n.ForEach(p => p.Remove());
                        e1.Add(new XElement("text", n));

                    }

                    LoadDataSetLevel(_DOCXML);
                }
                else
                {
                    LoadDataSet();
                }
            }
        }


        private void documentsectionlevelKAPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                _DOCXML = _DOCXML.IdentifySectionAsKap();
                if (_DOCXML.IsLevelDocument())
                {
                    HtmlHierarchy h = new HtmlHierarchy("level");
                    _DOCXML.Descendants("content").Remove();
                    _DOCXML.Add(new XElement("content",  h.GetDIBXMLContent(_DOCXML)));
                    LoadDataSetLevel(_DOCXML);
                }
                else
                {
                    MessageBox.Show("Ingen handling definert");
                }
            }
        }

        private void redigerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fotnoterToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void nummerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<XElement> fnspans = _DOCXML
                                    .Descendants("span")
                                    .Where(p =>
                                        (p.Attribute("style") == null ? "" : p.Attribute("style").Value).IndexOf("font-size:xx-small") != -1
                                        && (p.Attribute("style") == null ? "" : p.Attribute("style").Value).IndexOf("vertical-align:top") != -1
                                        && Regex.IsMatch(p.Value.Trim(), @"^[0-9]+$")
                                        && (XNode)p != p.Parent.Nodes().First()
                                       )
                                       .ToList();
            
            List<XElement> footnotes = _DOCXML
                                    .Descendants("span")
                                    .Where(p =>
                                        (p.Attribute("style") == null ? "" : p.Attribute("style").Value).IndexOf("font-size:xx-small") != -1
                                        && (p.Attribute("style") == null ? "" : p.Attribute("style").Value).IndexOf("vertical-align:top") != -1
                                        && Regex.IsMatch(p.Value.Trim(), @"^[0-9]+$")
                                        && (XNode)p == p.Parent.Nodes().First()
                                       )
                                       .ToList();
            XElement footnoteSection = null;
            XElement levelParent = null;
            string sectionName = "";
            if (_DOCXML.IsLevelDocument())
            {
                levelParent = _DOCXML.Descendants("level").First().Parent;
                sectionName = "level";
            }
            else
            {
                levelParent = _DOCXML.Descendants("section").First().Parent;
                sectionName = "section";
            }

            footnoteSection = new XElement(sectionName
                , new XAttribute("id", "fn00")
                , new XElement("title","Fotnoter")
                , new XElement("text")
                );

            foreach (XElement fn in fnspans)
            {
                if (footnotes.Where(p => p.Value.Trim() == fn.Value.Trim()).Count() != 0)
                {
                    XElement parent = footnotes.Where(p => p.Value.Trim() == fn.Value.Trim()).First().Parent;
                    footnotes.Where(p => p.Value.Trim() == fn.Value.Trim()).First().Remove();
                    XElement fnContent = new XElement(parent);
                    parent.Remove();
                    string fnRefId = "fnref" + fn.Value.Trim();
                    string fnId = "fn" + fn.Value.Trim();
                    XElement returnFootnote = new XElement(sectionName
                        , new XAttribute("id", fnId)
                        , new XElement("title", "[" + fn.Value.Trim() + "]")
                        , new XElement("text"
                            , fnContent)
                        );
                    footnoteSection.Add(returnFootnote);
                    XElement z = new XElement("sup"
                        , new XElement("a"
                            , new XAttribute("href", "#")
                            , new XAttribute("id", fnRefId)
                            , new XAttribute("data-bm", fnId)
                            , new XAttribute("class", "xref")
                            , new XAttribute("onclick", "return false;")
                            , fn.Value)
                        );
                    fn.ReplaceWith(z);

                }
            }
            levelParent.Add(footnoteSection);
            RebuildContent();
        }

        private void ResetTag()
        {
            this.Tag = null;

           
        }
        private void åpneLoverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";
            frmImportLov f = new frmImportLov();
            if (f.ShowDialog() != DialogResult.OK) return;

            _DOCXML = f.m_DocumentData;

            ResetTag();

            if (_DOCXML.Attribute("topic_id") != null)
                this.Tag = new XElement("document",
                    new XElement("topic_id", _DOCXML.Attribute("topic_id").Value)
                    );
                

           

            LoadDataSet();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _ImgFolder = "";
            frmImportKodeOversikt f = new frmImportKodeOversikt();
            if (f.ShowDialog() != DialogResult.OK) return;

            _DOCXML = f.m_DocumentData;
            LoadDataSet();

        }

        private void kodeoversiktSkattToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "")!="-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement text = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "document"
                                || p.Name.LocalName == "section"
                                || p.Name.LocalName == "level"
                            )
                            &&
                            (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            ).Elements("text").FirstOrDefault();
                    if (text == null)
                    {
                        MessageBox.Show("Elementet mangler 'text-element'!");
                        return;
                    }
                    XElement parent = text.Parent;
                    XElement orginal = text;
                    if (text != null)
                    {
                        switch (text.Elements("text").Count())
                        {
                            case 0:
                                break;
                            case 1: text = text.Elements("text").First(); break;
                            default:
                                MessageBox.Show("Flere text elementer!");
                                return;
                        }
                        XElement newText = new XElement("text");
                        int i = text.Nodes().Count();
                        for (int j = 0; j < i; j++)
                        {
                            XNode node = text.Nodes().ElementAt(j);
                            if ((node.NodeType == XmlNodeType.Element ? ((XElement)node).Name.LocalName : "") == "p")
                            {
                                XElement el = node as XElement;
                                while ((el.FirstNode != null ? el.FirstNode.NodeType == XmlNodeType.Element ? ((XElement)el.FirstNode).Name.LocalName : "" : "") == "br")
                                {
                                    el.FirstNode.Remove();
                                }
                                while ((el.LastNode != null ? el.LastNode.NodeType == XmlNodeType.Element ? ((XElement)el.LastNode).Name.LocalName : "" : "") == "br")
                                {
                                    el.LastNode.Remove();
                                }

                                string elText = el.DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim();
                                if (((el.Elements().Count() == 0 ? false : (el.Elements().First().Name) == "strong" ?
                                    Regex.IsMatch(el.Elements().First().DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim(), @"^(Kode(ne)?\s)?\d\d\d(\-A)?(\s|$|\-\d\d\d)") : false)))
                                {
                                    newText.Add(new XElement("h1", el.Elements().First().DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim()));
                                    el.Elements().First().Remove();

                                    if (((el.Elements("strong").Count() == 0 ? false : (el.Elements("strong").Last().Name) == "strong" ?
                                        Regex.IsMatch(el.Elements("strong").Last().DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim(), @"^(Kode(ne)?\s)\d\d\d(\-A)?(\s|$|\-\d\d\d)") : false)))
                                    {
                                        XElement cont = new XElement("cont");
                                        cont.Add(new XElement("h1", el.Elements("strong").Last().DescendantNodesAndSelf().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate().Trim()));
                                        cont.Add(el.Elements("strong").Last().NodesAfterSelf());
                                        el.Elements("strong").Last().NodesAfterSelf().Remove();
                                        el.Elements("strong").Last().ReplaceWith(el.Elements("strong").Last().Nodes());
                                        newText.Add(new XElement(el));
                                        newText.Add(cont.Nodes());
                                    }
                                    else
                                    {
                                        newText.Add(new XElement(el));
                                    }
                                }
                                else
                                {
                                    newText.Add(new XElement(el));
                                }


                            }
                            else
                                newText.Add(node);

                        }

                        frmEdit f = new frmEdit(newText);
                        f.ShowDialog(this);
                        if (f.DialogResult == DialogResult.OK)
                        {
                            XElement newtext = XElement.Parse("<text>" + f.returnHtml + "</text>");
                            RebuildPart(parent, orginal, newtext);
                            //XElement returnEl = newtext.BuildLevelsFromHeader(parent.Attribute("id").Value);
                            //if (parent.Name.LocalName == "level") returnEl = returnEl.ReplaceSections("level");
                            //if (returnEl.Name.LocalName == "text")
                            //{
                            //    orginal.ReplaceWith(new XElement(returnEl));
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                            //    LoadDataSet();
                            //}
                            //else
                            //{
                            //    orginal.ReplaceWith(returnEl.Nodes());
                            //    if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                            //    _DOCXML.AddFirst(_DOCXML.GetContentMain(true));
                            //    LoadDataSet();
                            //}
                        }
                    }
                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
            }
        }

        private void RebuildPart(XElement parent, XElement orignal,  XElement newtext)
        {
            XElement returnEl = newtext.BuildLevelsFromHeader(parent.Attribute("id").Value);
            if (parent.Name.LocalName == "level") returnEl = returnEl.ReplaceSections("level");
            if (returnEl.Name.LocalName == "text")
            {
                orignal.ReplaceWith(new XElement(returnEl));
            }
            else
            {
                orignal.ReplaceWith(returnEl.Nodes());
            }
            RebuildContent();

        }
        private void identifiserKodeoversiktToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                _DOCXML = _DOCXML.IdentifySectionAsKode();
                if ((_DOCXML.Descendants("document").Count() == 0 ? ""
                                    :
                                    (_DOCXML.Descendants("document").First().Attribute("doctypeid") == null ?
                                    "" : _DOCXML.Descendants("document").First().Attribute("doctypeid").Value)) == "7")
                {
                    LoadDataSetLevel(_DOCXML);
                }
                else
                {
                    LoadDataSet();
                }
            }

        }

        private void interneToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void kapittekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            XElement iv = GetTagsForarbeider();
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            XElement section = SelectTreeSection(c);
            if (section == null) return;
            ApplyInternalLinks(section, iv);
        }

        private XElement GetTagsForarbeider()
        {
            XElement iv = new XElement("tags");
            foreach (string id in _DOCXML.DescendantsAndSelf().Where(p =>
                (
                       p.Name.LocalName == "section" 
                    || p.Name.LocalName == "level"
                )
                && (p.Attribute("id") == null ? "" : p.Attribute("id").Value).StartsWith("KAP")).Select(p => p.Attributes("id").First().Value)
                )
            {
                iv.Add(new XElement("tag"
                    , new XAttribute("t1", id)
                    , new XAttribute("bm", id)
                    ));
            }
            return iv;
        }
        private XElement GetTagsKodeoversikt()
        {
            XElement iv = new XElement("tags");
            foreach (string id in _DOCXML.DescendantsAndSelf().Where(p =>
                (
                       p.Name.LocalName == "section"
                    || p.Name.LocalName == "level"
                )
                && (p.Attribute("id") == null ? "" : p.Attribute("id").Value).IsExactMatch(@"^K\d\d\d$")).Select(p => p.Attributes("id").First().Value)
                )
            {
                iv.Add(new XElement("tag"
                    , new XAttribute("t1", id)
                    , new XAttribute("bm", id)
                    ));
            }
            return iv;
        }

        private XElement GetTagsKommentar(string id)
        {
            XElement lovName = _DOCXML
                .Descendants()
                .Where(q => (q.Attribute("id") == null ? "" : q.Attribute("id").Value) == id)
                .First()
                .AncestorsAndSelf()
                .Where(p =>
                    (
                       p.Name.LocalName == "section"
                    || p.Name.LocalName == "level"
                    )
                )
                .Last()
                .Elements("title")
                .FirstOrDefault();
            string regexp = @"^§\s+(?<par>(\d+\-\d+))";
            XElement iv = new XElement("tags");
            foreach (XElement section in _DOCXML
                .Descendants()
                .Where(q => (q.Attribute("id") == null ? "" : q.Attribute("id").Value) == id)
                .First()
                .DescendantsAndSelf().Where(p =>

                (
                       p.Name.LocalName == "section"
                    || p.Name.LocalName == "level"
                )
                && (p.Element("title") == null ? "" : p.Element("title").GetElementText()).IsExactMatch(regexp))
                )
            {
                string title = section.Element("title").GetElementText();
                string sid = section.Attribute("id").Value;
                string par = "P" + Regex.Match(title, @"^§\s+(?<par>(\d+\-\d+))").Groups["par"].Value;

                iv.Add(new XElement("tag"
                    , new XAttribute("t1", par)
                    , new XAttribute("bm", sid)
                    ));
            }

            if (lovName != null)
            {
                string sLovName = lovName.Value.ToLower();
                iv.Add(_DOCXML
                .Descendants()
                .Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == id)
                .First()
                .AncestorsAndSelf()
                .Where(p =>
                    (
                       p.Name.LocalName == "section"
                    || p.Name.LocalName == "level"
                    )
                )
                .Last()
                .DescendantsAndSelf()
                .Where(q => (q.Attribute("id") == null ? "" : q.Attribute("id").Value).StartsWith("note_"))
                .Select(r => new XElement("tag",
                    new XAttribute("t1", r.Attribute("id").Value.Replace(sLovName+"_","")),
                    new XAttribute("bm", r.Attribute("id").Value)
                    ))
                 );

            }
            iv.Add( _DOCXML
                .Descendants()
                .Where(q => (q.Attribute("id") == null ? "" : q.Attribute("id").Value).StartsWith("note_"))
                .Select(r => new XElement("tag",
                    new XAttribute("t1", r.Attribute("id").Value),
                    new XAttribute("bm", r.Attribute("id").Value)
                    ))
                 );


            return iv;
        }

        private XElement GetTagsLover()
        {
            XElement iv = new XElement("tags");
            foreach (string id in _DOCXML.DescendantsAndSelf().Where(p =>
                (
                       p.Name.LocalName == "section"
                    || p.Name.LocalName == "level"
                )
                && (p.Attribute("id") == null ? "" : p.Attribute("id").Value).IsExactMatch(@"^P\d")).Select(p => p.Attributes("id").First().Value)
                )
            {
                iv.Add(new XElement("tag"
                    , new XAttribute("t1", id)
                    , new XAttribute("bm", id)
                    ));
            }
            return iv;
        }

        private void ApplyExternalLinks(XElement section)
        {
            InTextLinkingData _data = new InTextLinkingData(true);

            string queryName = "total";
            string regexpQuery = _data.RegexpExpressionGet(queryName); ;

            XElement AllActions = _data.LoadGlobalXML(queryName + "_actions");
            AllActions = AllActions.Descendants("actions").FirstOrDefault();
            XElement MainActions = AllActions.Descendants("action").Where(p => (p.Attribute("query") == null ? "" : p.Attribute("query").Value) == queryName).FirstOrDefault();
            if (MainActions == null) return;

            InTextLinkingXML itl = new InTextLinkingXML(regexpQuery, MainActions, AllActions, true);
            
            XElement uid = itl.FindUnidentyfiedTags(section, "no");
            if (uid != null)
            {
                SaveFileDialog sf = new SaveFileDialog();
                if (sf.ShowDialog() == DialogResult.OK)
                    uid.Save(sf.FileName);
            }


        }
        private void ApplyExternalLinksInternal(XElement section)
        {
            string queryName = "total";

            XElement regexps = XElement.Load(global.m_InternalDocumentRegexpFile);

            string regexpQuery = regexps.RegexBuild(queryName);
            

            XElement AllActions = XElement.Load(global.m_InternalActionsFile);
            if ((AllActions == null ? 0 : AllActions.Descendants("action").Count()) == 0) return;
            XElement MainActions = AllActions.Descendants("action").Where(p => (p.Attribute("query") == null ? "" : p.Attribute("query").Value) == queryName).FirstOrDefault();
            if (MainActions == null) return;

            InTextLinkingXML itl = new InTextLinkingXML(regexpQuery, MainActions, AllActions, true);
            itl.InsertLinksInDibDocument(section, "no");
            if (itl.ExternalTags == null)
            {
                MessageBox.Show("Ingen linker funnet!");
            }
            else
            {
                Debug.Print(itl.ExternalTags.ToString());
                Debug.Print(itl.UnidentyfiedTags.ToString());
            }
        }
        private void ApplyInternalLinks(XElement section, XElement internalTags)
        {

            string queryName = "total";

            XElement regexps = XElement.Load(global.m_InternalDocumentRegexpFile);

            string regexpQuery = regexps.RegexBuild(queryName);
            
            
            //InTextLinkingData _data = new InTextLinkingData(true);
            //InTextLinkingAbbrevations _abbrev = new InTextLinkingAbbrevations();
            //string strAbbrevs = _abbrev.GetAbbrevations(_data.GetAbbrevations());

            //XElement xmlRegexps = XElement.Load(global.m_InternalDocumentRegexpFile);
            
            //InTextLinkingBuildRegexp _regexp = new InTextLinkingBuildRegexp();

            //string regexpQuery = _regexp.GetRegexExpression(queryName, xmlRegexps, strAbbrevs);

            XElement AllActions = XElement.Load(global.m_InternalActionsInternalFile);
            if ((AllActions == null ? 0 : AllActions.Descendants("action").Count()) == 0) return; 
            XElement MainActions = AllActions.Descendants("action").Where(p => (p.Attribute("query") == null ? "" : p.Attribute("query").Value) == queryName).FirstOrDefault();
            if (MainActions == null) return;

            InTextLinkingXML itl = new InTextLinkingXML(regexpQuery, MainActions, AllActions,true);

            itl.InternalTags = internalTags.HasElements ? internalTags : null;
            itl.InsertLinksInDibDocument(section, "no");

        }

        private void kodeoversiktToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            XElement iv = GetTagsKodeoversikt();

            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            XElement section = SelectTreeSection(c);
            if (section == null) return;

            ApplyInternalLinks(section, iv);
        }

        private void endreTittelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "-1") != "-1")
            {
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement top = GetContentElement(c.ID);

                    string title = top.Element("title").Value;

                    string value = title;
                    metods met = new metods();
                    if (met.InputBox("Endre tittel", "Endre tittel:", ref value) == DialogResult.OK)
                    {
                        top.Element("title").Value = value;
                        c.NodeText = value;
                        treeView1.SelectedNode.Text = "(" + c.ID + ") " + value;
                        treeView1.Refresh();
                        ShowDocument(treeView1.SelectedNode);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void redigerInnholdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHtml();
        }

        private void loverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            XElement iv = GetTagsLover();

            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            XElement section = SelectTreeSection(c);
            if (section == null) return;

            ApplyInternalLinks(section, iv);
        }

        private void loverToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "") != "-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement text = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "document"
                                || p.Name.LocalName == "section"
                                || p.Name.LocalName == "level"
                            )
                            &&
                            (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            ).Elements("text").FirstOrDefault();
                    if (text == null)
                    {
                        MessageBox.Show("Elementet mangler 'text-element'!");
                        return;
                    }
                    XElement parent = text.Parent;
                    XElement orginal = text;
                    if (text != null)
                    {
                        switch (text.Elements("text").Count())
                        {
                            case 0:
                                break;
                            case 1: text = text.Elements("text").First(); break;
                            default:
                                MessageBox.Show("Flere text elementer!");
                                return;
                        }
                        XElement container = new XElement("container");
                        int i = text.Nodes().Count();
                        for (int j = 0; j < i; j++)
                        {
                            XNode node = text.Nodes().ElementAt(j);
                            if ((node.NodeType == XmlNodeType.Element ? ((XElement)node).Name.LocalName : "") == "p")
                            {

                            }
                            else if ((node.NodeType == XmlNodeType.Element ? ((XElement)node).Name.LocalName : "") == "table")
                            {

                            }
                            else
                                container.Add(node);

                        }
                        orginal.ReplaceWith(container.Nodes());
                        if (_DOCXML.Element("content") != null) _DOCXML.Element("content").Remove();
                        _DOCXML.AddFirst(_DOCXML.GetContentMain(true));

                        LoadDataSet();
                    }
                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }
            }

        }

        private void eksterneToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void lagreLoverIDbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _TOPIC_ID = GetDocXMLTopicId();
            if ((_TOPIC_ID == null ? "" : _TOPIC_ID) == "")
            {
                MessageBox.Show("Dokumentet mangler topic_id!", "Eksporter dokument", MessageBoxButtons.YesNo);
                return;
            }
            DibDocumentType doctype = _DOCXML.GetDocumentType();
            if (doctype == DibDocumentType.Lover )
            {
                XElement document = _DOCXML;
                if (document.Element("content") != null) document.Element("content").Remove();

                HtmlHierarchy h = new HtmlHierarchy("section");
                XElement content = h.GetIndexFromSectionDocument(document);
                string metaId = "";
                metods met = new metods();
                if (met.InputBox("Angi Lov Id", "Angi Lov Id", ref metaId) != DialogResult.OK)
                        return;

                XElement metadata = new XElement("root",
                    new XElement("metadata",
                            new XElement("id", metaId)
                            )
                );
                if (content != null)
                    content = new XElement("root",
                        content);

                XElement pkg = new XElement("pkg"
                , new XElement("pkg_document", new XElement(document))
                , new XElement("pkg_content", new XElement(content))
                , new XElement("pkg_meta", new XElement(metadata))
                );

                if (MessageBox.Show("Vil du eksportere dette dokumentet topic_id=" + _TOPIC_ID + "!", "Eksporter dokument", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    pkg.Save(_TARGETDIR + "pkg.xml");
                    return;
                }

                pkg.Save(_TARGETDIR + "pkg.xml");


                FileStream file = new FileStream(_TARGETDIR + "pkg.xml", FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader sr = new StreamReader(file, Encoding.ASCII);
                string XmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();


                XmlString = Regex.Replace(XmlString, @"<\?xml[^<]+?>", "", RegexOptions.Multiline | RegexOptions.Singleline);
                XmlString = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(XmlString));

                commxml cx = new commxml();
                string sourceRegexp = cx.Execute("TopicMap.dbo._ImpClientImportDocument_SectionEx '" + _TOPIC_ID + "', '" + XmlString + "'");
                XDocument d = XDocument.Parse(sourceRegexp);
                XElement result = d.Descendants("result").FirstOrDefault();
                if (result != null)
                {
                    result.Save(_TARGETDIR + "result.xml");

                    webBrowser1.Navigate(_TARGETDIR + "result.xml");
                }

            }
            else if (_DOCXML.Descendants("documents").Elements("document").Attributes("dibt").Count() != 0)
            {

                string topic_id = "";
                if (this.Tag != null)
                {
                    XElement XMLDocInfo = (XElement)this.Tag;

                    topic_id = XMLDocInfo.Element("topic_id") == null ? "" : XMLDocInfo.Element("topic_id").Value;
                }

                if (topic_id == "")
                {
                    metods met = new metods();
                    if (met.InputBox("Angi TopicId", "Angi TopicId", ref topic_id) != DialogResult.OK)
                        return;

                    if (topic_id.Length != 38 || !topic_id.StartsWith("{") || !topic_id.EndsWith("}"))
                        return;
                }

                
                
                XElement document =  new XElement(_DOCXML);
                XElement content = null;
                if (document.Element("content") != null)
                {
                    content = new XElement(document.Element("content"));
                    document.Element("content").Remove();
                    //document.Descendants("document").ToList().ForEach(p=>p.ReplaceWith(new XElement("section",p.Attributes(), p.Nodes())));
                    document.Descendants("documents").First().ReplaceWith(                        
                        new XElement("document",
                        new XAttribute("type","handbook"),
                        new XAttribute("variant", "mva"),
                        document.Descendants("documents").First().Nodes()));
                }

                XElement pkg = new XElement("pkg"
                    , new XElement("pkg_content", new XElement("root", 
                        new XElement(content)))
                    , new XElement("pkg_document", new XElement(document))
                    );
                if (MessageBox.Show("Vil du eksportere dette dokumentet!", "Eksporter dokument", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }

                pkg.Save(_TARGETDIR + "pkg.xml");


                FileStream file = new FileStream(_TARGETDIR + "pkg.xml", FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader sr = new StreamReader(file, Encoding.ASCII);
                string XmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();


                XmlString = Regex.Replace(XmlString, @"<\?xml[^<]+?>", "", RegexOptions.Multiline | RegexOptions.Singleline);
                XmlString = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(XmlString));

                commxml cx = new commxml();
                string sourceRegexp = cx.Execute("TopicMap.dbo._ImpClientImportDocument_SectionEx '" + topic_id + "', '" + XmlString + "'");
                XDocument d = XDocument.Parse(sourceRegexp);
                XElement result = d.Descendants("result").First();
                result.Save(_TARGETDIR + "result.xml");

                webBrowser1.Navigate(_TARGETDIR + "result.xml");


            }
        }

        private void serverRegexpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            XElement iv = GetTagsLover();

            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            XElement section = SelectTreeSection(c);
            if (section == null) return;

            ApplyExternalLinks(section);
        }

        private void interneRegexpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            XElement section = SelectTreeSection(c);
            if (section == null) return;

            ApplyExternalLinksInternal(section);
        }

        private void åpneInDesignHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmHtmlRead frm = new frmHtmlRead();
            frm.ShowDialog();
        }

        private void kollapsSlåSammenTitlerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Kollaps(true);
        }

        private void fotnoterSkjulToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.ChildHidden, NavigationTypes.Footnotes);
        }

        private enum NavigationValues
        {
            Open = 0,
            Collapsed = 1,
            Hidden = 2,
            ChildHidden = 3
        }

        private enum NavigationTypes
        {
            Item = 0,
            Chapter = 1,
            Footnotes = 2,
            All = 3
        }

        private void kapittelKollapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.Collapsed, NavigationTypes.Chapter);
        }

        private void SetNavigation(NavigationValues nav, NavigationTypes mode)
        {
            List<XElement> items = null;
            if (mode == NavigationTypes.All)
            {
                items = _DOCXML.Descendants().Where(p =>
                                            (
                                                p.Name == "section"
                                                || p.Name == "level"
                                            )
                                            && p.Elements().Where(q=>
                                                q.Name == "section"
                                                || q.Name == "level"
                                                ).Count() != 0
                                            )
                                            .ToList();
            }
            else if (mode==NavigationTypes.Item)
            {
                string id = GetTreeViewId();
                if (id == "") return;
                items = _DOCXML.Descendants().Where(p =>
                                            (
                                                p.Name == "section"
                                                || p.Name == "level"
                                            )
                                            && (p.Attribute("id") == null ? "???" : p.Attribute("id").Value) == id
                                            )
                                            .ToList();
                
            }
            else if (mode==NavigationTypes.Footnotes)
            {
                items = _DOCXML.Descendants().Where(p =>
                                            (
                                                p.Name == "section"
                                                || p.Name == "level"
                                            )
                                            && (
                                                (p.Element("title") == null ? "" : p.Element("title").GetElementText()).Trim().ToLower() == "fotnoter"
                                                ||
                                                (p.Element("title") == null ? "" : p.Element("title").GetElementText()).Trim().ToLower() == "footnotes"
                                                ))
                                            .ToList();
            }
            else if (mode == NavigationTypes.Chapter)
            {
                string regexp = @"^kap(ittel)?(\.)\s+\d+(\s+|$)";
                items = _DOCXML.Descendants().Where(p =>
                                                (
                                                    p.Name == "section"
                                                    || p.Name == "level"
                                                )
                                                && (p.Element("title") == null ? false : Regex.IsMatch(p.Element("title").GetElementText().Trim().ToLower(), regexp)) == true
                                                )
                                                .ToList();

            }

            string sNav =((int)nav).ToString();
            foreach (XElement item in items)
            {
                if (item.Attribute("nav_view") == null)
                {
                    item.Add(new XAttribute("nav_view", sNav));
                }
                else
                {
                    item.Attribute("nav_view").Value = sNav;
                }
            }

        }

        private string GetTreeViewId()
        {
            string returnValue = "";
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "-1") != "-1") returnValue = c.ID;
            return returnValue;
        }


        private void åpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.Open, NavigationTypes.Item);
        }

        private void kollapsetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.Collapsed, NavigationTypes.Item);
        }

        private void skjultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.Hidden, NavigationTypes.Item);
        }

        private void skjulBarnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.ChildHidden, NavigationTypes.Item);
        }

        private void åpneUtgivelseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmReadUtgivelseXML frm = new frmReadUtgivelseXML();
            frm.ShowDialog();

        }

        private void alleKollapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetNavigation(NavigationValues.Collapsed, NavigationTypes.All);
        }

        private void kommentarutgaveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode == null) return;

            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;

            XElement iv = GetTagsKommentar(c.ID.ToString());

            XElement section = SelectTreeSection(c);
            if (section == null) return;

            ApplyInternalLinks(section, iv);
            
        }

        private void limInnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.CheckBoxes = true;
            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            string parentId = c.ID;
            XElement section = SelectTreeSection(c);
            List<TreeNode> sel = getNodes(treeView1.Nodes);
            foreach (TreeNode tn in sel)
            {
                TreeNode n1 = tn;
                Category c1 = (Category)n1.Tag;
                if (c1.ID == parentId)
                {
                    MessageBox.Show("Noden kan  ikke limes inn i seg selv!");
                    return;
                }
            }
            foreach (TreeNode tn in sel)
            {
                TreeNode n1 = tn;
                Category c1 = (Category)n1.Tag;
                XElement part = SelectTreeSection(c1);
                section.Add(new XElement(part));
                part.Remove();
                //Debug.Print(tn.Text);
            }
            if (_DOCXML.Descendants("content").Count() != 0) _DOCXML.Descendants("content").First().Remove();
            Load_DOCXML();
            if (parentId != "")
            {
                TreeNode node = treeView1.Nodes[0];
                FindTreeNode(node, parentId);
            }

        }

        public List<TreeNode> getNodes(TreeNodeCollection nc)
        {
            return nc.Cast<TreeNode>().Union(nc.Cast<TreeNode>().SelectMany(tn => getNodes(tn.Nodes))).Where(n => n.Checked).ToList();
        }

        private void sisteParagrafITekstNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            string parentId = c.ID;
            XElement section = SelectTreeSection(c);
            XElement text = section.Elements("text").FirstOrDefault();
            if (text == null) return;
            XElement lastText = text.Elements().Where(p => p.GetElementText().Trim() != "").LastOrDefault();
            string id =  (string)lastText.Attributes("id").FirstOrDefault();
            if (id == "") id = Guid.NewGuid().ToString();
            if (lastText == null) return;
            string slastText = lastText.GetElementText();
            if (MessageBox.Show("Vil du lagre '" + slastText + "' som node?","Lagre som node",MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)==DialogResult.Yes)
            {
                section.AddAfterSelf(new XElement(section.Name.LocalName,
                    new XAttribute("id", id),
                    new XElement("title", lastText.Nodes()),
                    new XElement("text")
                    ));
                lastText.Remove();
                Load_DOCXML();
                TreeNode node = treeView1.Nodes[0];
                FindTreeNode(node, id);
            }

        }

        private void sisteParagrafITextNodeNivåUnderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            string parentId = c.ID;
            XElement section = SelectTreeSection(c);
            XElement text = section.Elements("text").FirstOrDefault();
            if (text == null) return;
            XElement lastText = text.Elements().Where(p => p.GetElementText().Trim() != "").LastOrDefault();
            string id = (string)lastText.Attributes("id").FirstOrDefault();
            if (id == "") id = Guid.NewGuid().ToString();
            if (lastText == null) return;
            string slastText = lastText.GetElementText();
            if (MessageBox.Show("Vil du lagre '" + slastText + "' som node?", "Lagre som node", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                section.Element("text").AddAfterSelf(new XElement(section.Name.LocalName,
                    new XAttribute("id", id),
                    new XElement("title", lastText.Nodes()),
                    new XElement("text")
                    ));
                lastText.Remove();
                Load_DOCXML();
                TreeNode node = treeView1.Nodes[0];
                FindTreeNode(node, id);
            }


        }

        private void fjernSPANITitlerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                List<XElement> titles = _DOCXML.Descendants("title").Where(p => p.Descendants("span").Count() != 0).ToList();
                List<XElement> spans = titles.Descendants("span").ToList();
                foreach (XElement span in spans)
                    span.ReplaceWith(span.Nodes());
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void slettNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            string parentId = c.ParentID;
            XElement section = SelectTreeSection(c);
            section.Remove();
            Load_DOCXML();
            TreeNode node = treeView1.Nodes[0];
            FindTreeNode(node, parentId);

        }

        private void limInnSomFørsteNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.CheckBoxes = true;
            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;
            Category c = (Category)n.Tag;
            string parentId = c.ID;
            XElement section = SelectTreeSection(c);
            List<TreeNode> sel = getNodes(treeView1.Nodes);
            foreach (TreeNode tn in sel)
            {
                TreeNode n1 = tn;
                Category c1 = (Category)n1.Tag;
                if (c1.ID == parentId)
                {
                    MessageBox.Show("Noden kan  ikke limes inn i seg selv!");
                    return;
                }
            }
            foreach (TreeNode tn in sel)
            {
                TreeNode n1 = tn;
                Category c1 = (Category)n1.Tag;
                XElement part = SelectTreeSection(c1);
                if (section.Descendants(section.Name.LocalName).Count() != 0)
                {
                    section.Descendants(section.Name.LocalName).First().AddBeforeSelf(new XElement(part));
                    part.Remove();
                }
                //Debug.Print(tn.Text);
            }

            Load_DOCXML();
            if (parentId != "")
            {
                TreeNode node = treeView1.Nodes[0];
                FindTreeNode(node, parentId);
            }

        }

        private void leggTilStandardNrITittelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string prefix = "";
            metods met = new metods();
            if (met.InputBox("Prefix for numeriske titler", "Tast inn prefix foran i numeriske titler", ref prefix) != DialogResult.OK) return;
            if (prefix != "")
            {
                IEnumerable<XElement> titles = _DOCXML.Descendants("title").Where(p => p.GetElementText().GetNumValue() != "");
                titles.AddPrefixToTitles(prefix);
                Load_DOCXML();
            }
        }

        private void fjernRefxxxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                _DOCXML.RemoveReferances();
                Load_DOCXML();
            }
        }

        private void ResetLonnBoxer(XElement tr, string className)
        {
            if (tr.Attribute("style") != null) tr.Attribute("style").Remove();
            if (tr.Attribute("class") != null) tr.Attribute("class").Remove();
            tr.Add(new XAttribute("class", className));
            if (tr.Elements("td").Count() > 2)
            {
                string refText = tr.Elements("td").ElementAt(2).GetElementText(" ").Trim();
                tr.Elements("td").ElementAt(2).Elements().Remove();
                tr.Elements("td").ElementAt(2).Add(new XElement("p", refText));
            }

        }
        private void lønnsABCBokserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                List<XElement> tbls = _DOCXML
                                .Descendants("table")
                                .Where(p => (p.Descendants("td").FirstOrDefault() != null ? p.Descendants("td").FirstOrDefault().GetElementText().ToLower() : "") == "skatteplikt")
                                .ToList();
                bool kodeFound = false;
                foreach (XElement tbl in tbls)
                {
                    kodeFound = false;
                    if (tbl.Attribute("style")!= null)  tbl.Attribute("style").Remove();
                    if (tbl.Attribute("class")!= null)  tbl.Attribute("class").Remove();
                    tbl.Add(new XAttribute("class", "tbRapidLonn"));
                    XElement currTbl = tbl;
                    if (currTbl.Elements("tbody").Count() != 0)
                        currTbl = currTbl.Elements("tbody").First();
                    foreach (XElement tr in currTbl.Elements("tr"))
                    {
                        XElement fTd = tr.Elements("td").FirstOrDefault();
                        if (fTd != null)
                        {
                            switch (fTd.GetElementText().ToLower().Trim())
                            {
                                case "skatteplikt":
                                    ResetLonnBoxer(tr,"tbRapidSkatt");
                                    break;
                                case "trekkplikt":
                                    ResetLonnBoxer(tr,"tbRapidTrekk");
                                    break;
                                case "avgiftsplikt":
                                    ResetLonnBoxer(tr,"tbRapidAvgift");
                                    break;
                                case "oppgaveplikt":
                                    ResetLonnBoxer(tr,"tbRapidOppgave");
                                    break;
                                case "kode":
                                    kodeFound = true;
                                    XElement currRow = tr;
                                    while (tr != null)
                                    {
                                        ResetLonnBoxer(currRow, "tbRapidKode");
                                        if (currRow.Elements("td").Count() > 1)
                                        {
                                            string kode = currRow.Elements("td").ElementAt(1).GetElementText(" ").Trim();
                                            if (Regex.IsMatch(kode,@"\d\d\d"))
                                            {
                                                Match m = Regex.Match(kode, @"\d\d\d");
                                                if (m.Value != "")
                                                {
                                                    currRow.Elements("td").ElementAt(1).Elements().Remove();
                                                    currRow.Elements("td").ElementAt(1).Add(new XElement("p",
                                                        m.Index == 0 ? null : new XText(kode.Substring(0, m.Index)),
                                                        new XElement("a",
                                                            new XAttribute("class", "xref"),
                                                            new XAttribute("data-tid", "{2A921991-BC84-4CC0-9221-566021733475}"),
                                                            new XAttribute("data-bm", "K" + m.Value),
                                                            new XText(m.Value)
                                                            ),
                                                        (m.Index + m.Length) < kode.Length ? new XText(kode.Substring(m.Index + m.Length)) : null
                                                        )
                                                    );
                                                }
                                            }
                                        }

                                        XNode next = currRow.NextNode;
                                        if (next == null) break;
                                        if ((next.NodeType == XmlNodeType.Element ? ((XElement)next).Name.LocalName : "") == "tr")
                                        {
                                            currRow = (XElement)next;
                                        }

                                    }

                                    break;

                            }

                        }
                        if (kodeFound) break;

                    }
                }


                Load_DOCXML();
            }

        }

        private void merknaderTilToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void artikkelArticleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Artikkel/Article
        }

        private void TrySectioningByNameNumber(string regexp)
        {
            int counter = 0;
            Regex rx = new Regex(regexp);
            string htmlHeader = "h1";
            Category c = GetTreeViewElement(treeView1);
            if ((c != null ? c.ID : "") != "-1")
            {
                treeView1.UseWaitCursor = true;
                string parentId = string.Empty;
                if (c.NodeText != c.ID)
                {
                    XElement text = _DOCXML.Descendants().Where(p =>
                            (
                                p.Name.LocalName == "document"
                                || p.Name.LocalName == "section"
                                || p.Name.LocalName == "level"
                            )
                            &&
                            (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == c.ID
                            ).Elements("text").FirstOrDefault();
                    XElement parent = text.Parent;
                    XElement orginal = text;
                    if (text != null)
                    {
                        switch (text.Elements("text").Count())
                        {
                            case 0:
                                break;
                            case 1: text = text.Elements("text").First(); break;
                            default:
                                MessageBox.Show("Flere text elementer!");
                                return;
                        }
                        XElement newText = new XElement("text");
                        int i = text.Nodes().Count();
                        for (int j = 0; j < i; j++)
                        {
                            XNode node = text.Nodes().ElementAt(j);
                            string elText = node.GetNodeText();
                            bool found = false;
                            Match m = rx.Match(elText);
                            if (m.Success)
                            {
                                int value;
                                if (Int32.TryParse(m.Groups["number"].Value, out value))
                                {
                                    if ((value - 1) == counter)
                                    {
                                        found = true;
                                        counter = value;
                                    }
                                }
                                     
                            }
                            if (found)
                            {
                                elText = Regex.Replace(elText, @"^" + m.Value, m.Value + " ");
                                newText.Add(new XElement(htmlHeader, elText));
                            }
                            else
                            {
                                newText.Add(node);
                            }

                        }

                        frmEdit f = new frmEdit(newText);
                        f.ShowDialog(this);
                        if (f.DialogResult == DialogResult.OK)
                        {
                            XElement newtext = XElement.Parse("<text>" + f.returnHtml + "</text>");
                            RebuildPart(parent, orginal, newtext);
                        }
                    }

                    TreeNode t = treeView1.Nodes[0];
                    FindTreeNode(t, c.ID);
                }

            }

            treeView1.UseWaitCursor = true;
        }

        private void artikkelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string regexp = @"^Artikkel\s+(?<number>(\d+))";
            TrySectioningByNameNumber(regexp);
        }

        private void articleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string regexp = @"^Article\s+(?<number>(\d+))";
            TrySectioningByNameNumber(regexp);
        }

        private void lagreSomLevelKeepAsocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string topic_id = GetDocXMLTopicId();
            if (topic_id == "") return;
            string pkgName = SaveDocXml(topic_id);
            if (pkgName == null)
            {
                MessageBox.Show("Dette formatet kan ikke eksporteres som 'level'!");
                return;
            }
            if (!File.Exists(pkgName)) return;
            SendPkg(pkgName, topic_id, 0);
        }

        
    }
}
