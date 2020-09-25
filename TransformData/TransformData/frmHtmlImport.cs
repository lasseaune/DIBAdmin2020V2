using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using HtmlAgilityPack;
using TransformData.Global;
using System.Windows.Forms.ComponentModel;
using DIB.RegExp.ExternalStaticLinks;
using DIB.BuildHeaderHierachy;
using Dib.Transform;
using DIB.Data;

namespace TransformData
{
    [PermissionSet(SecurityAction.Demand, Name ="FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public partial class frmHtmlImport : Form
    {
        private List<string> _IDS = new List<string>();
        private bool _NEW = false;
        private Dictionary<string, string> _REGEXPQ = null;
        public frmProject _project;
        private string _sourceAdress = "";
        private int _elementIndex;
        private EditItem _EDITITEM = null;
        
        private class EditItem
        {
            public XElement partxml {set;get;}
            public string partname { set; get; }
            public string sectionId { set; get; }
        }
        private class cXDoc : System.EventArgs
        {
            private XElement m_XDoc = null;
            public delegate void ChangingHandler(object sender);
            
            public event ChangingHandler Change;

            public void SetXDoc(XElement xd)
            {
                this.m_XDoc = xd;
                XDocument xmlDoc = null;
                if (File.Exists(global._dbPath +  "import.xml"))
                {
                    xmlDoc = XDocument.Load(global._dbPath + "import.xml");
                }
                else
                {
                    xmlDoc = new XDocument(new XElement("documents"));
                }

                if (xmlDoc.Descendants("document").Where(p => p.Element("name").Value == xd.Element("name").Value).Count() == 1)
                {
                    XElement d = xmlDoc.Descendants("document").Where(p => p.Element("name").Value == xd.Element("name").Value).First();
                    d.ReplaceWith(new XElement(xd));
                }
                else
                {
                    xmlDoc.Root.Add(new XElement(xd));
                }
                xmlDoc.Save(global._dbPath +"import.xml");
                Change(this.m_XDoc);
            }
            public XElement GetXDoc()
            {
                return this.m_XDoc;
            }
        }

        cXDoc _myXDoc = null;

        private enum WebBrowserActions
        {
            none = -1,
            loadedit = 0,
            loadedithidden = 1,
            saveedithidden = 2,
            normalnavigation=3,
            import=4
        }

        private WebBrowserActions _DocumentCompletedAction = WebBrowserActions.none; 
        
        
        private TextBox _htmlText = new TextBox();

        private XmlEditor myXMLEditor;
        private WebBrowser myWebBrowser;
        private IDictionary<HtmlElement, string> elementStyles = new Dictionary<HtmlElement, string>();
        private IDictionary<HtmlElement, string> element = new Dictionary<HtmlElement, string>();
        private HtmlElement _element = null;
        private HtmlElement _mElement = null;
        private ToolStripProgressBar _PROGRESS;
        private ToolStripStatusLabel _STATUSTEXT;

        public frmHtmlImport(ref ToolStripProgressBar p, ref ToolStripStatusLabel l)
        {
            
            InitializeComponent();
            _STATUSTEXT = l;
            _PROGRESS = p;
            _PROGRESS.Value = 0;
            _REGEXPQ = BuildRegexp();
            CreateWebBrowser();
            
            
            rbArea1.Checked = false;
            rbArea1.Checked = true;
            _myXDoc = new cXDoc();
            _myXDoc.Change += new cXDoc.ChangingHandler(_myXDoc_Change);
            btnSaveHtml.Enabled = false;
        }

        private void LoadExisting()
        {
            global.m_DB_TOPICS.Clear();
            commxml cx = new commxml();
            string sourceRegexp = cx.Execute("diba0706.dbo._ImpClientGetDocumentListForarbeider");
            XDocument d = XDocument.Parse(sourceRegexp);
            XElement docs = new XElement(d.Descendants("document").First().Parent);
            foreach (XElement el in docs.Descendants("document"))
            
            {
                topic t = new topic();
                t.topic_id = el.Element("topic_id").Value;
                t.name = el.Element("name").Value;
                t.info = el.Element("info").Value;
                t.year = el.Element("year").Value;
                t.doctype = (el.Element("doc_type") == null ? "xml" : (el.Element("doc_type").Value.Length == 38 ? "xml" : el.Element("doc_type").Value));

                XElement idD = new XElement("document");
                string name = t.info;
                name.IdentifyMatchForarbeider(idD);

                t.name1 = (idD.Attribute("name1") == null ? "none" : idD.Attribute("name1").Value);
                t.number = (idD.Attribute("number") == null ? "none" : idD.Attribute("number").Value);
                t.type = (idD.Attribute("type") == null ? "none" : idD.Attribute("type").Value);
                t.year1 = (idD.Attribute("year1") == null ? "none" : idD.Attribute("year1").Value);
                t.year2 = (idD.Attribute("year2") == null ? "none" : idD.Attribute("year2").Value);
                t.key = t.name1 + "_" + t.number + "_" + t.type + "_" + t.year1 + "_" + t.year2;
                global.m_DB_TOPICS.Add(t);
            }
            
            
            
            //docs = XElement.Load(global.m_forarbeidertoimport);
            //foreach (XElement el in docs.Descendants("container").Where(p=>p.Attribute("unconnected")==null))
            //{
            //    global.topic t = new global.topic();
            //    t.topic_id = el.Attribute("topic_id").Value;
            //    t.name = el.Elements("document").First().Element("name").Value;
            //    t.info = el.Elements("document").First().Element("info").Value;
            //    t.year = el.Elements("document").First().Element("year").Value;
            //    t.name1 = (el.Attribute("name1") == null ? "none" : el.Attribute("name1").Value);
            //    t.number = (el.Attribute("number") == null ? "none" : el.Attribute("number").Value);
            //    t.type = (el.Attribute("type") == null ? "none" : el.Attribute("type").Value);
            //    t.year1 = (el.Attribute("year1") == null ? "none" : el.Attribute("year1").Value);
            //    t.year2 = (el.Attribute("year2") == null ? "none" : el.Attribute("year2").Value);
            //    t.key = t.name1 + "_" + t.number + "_" + t.type + "_" + t.year1 + "_" + t.year2;
            //    global.m_TOPICS.Add(t);
            //}

        }
        public void OpenDocument(string name)
        {
            myWebBrowser.Navigate("www.google.no/search?q=" + name);
        }
        public void OpenDocument(existing tag)
        {
            XElement myRes = null;
            XDocument xmlDoc = null;
            if (File.Exists(global._dbPath + "import.xml"))
                xmlDoc = XDocument.Load(global._dbPath + "import.xml");

            if (xmlDoc != null)
            {
                if ((xmlDoc.Descendants("document").Count() == 0 ?
                    0
                    :
                    xmlDoc.Descendants("document").Where(p =>
                            (p.Element("topic_id") == null ?
                            ""
                            :
                            p.Element("topic_id").Value) == tag.topic_id).Count()
                    ) != 0)
                {
                    myRes = xmlDoc.Descendants("document").Where(p =>
                            (p.Element("topic_id") == null ?
                                ""
                                :
                                p.Element("topic_id").Value
                            ) == tag.topic_id
                            )
                            .First();
                        
                    switch (tag.area)
                    {
                        case @"regjeringen": rbArea1.Checked = false; rbArea1.Checked = true; break;
                        case @"stortinget": rbArea2.Checked = false; rbArea2.Checked = true; break;
                    }
                }
            }

            if (myRes != null)
            {
                _myXDoc.SetXDoc(myRes);
                OpenDocumentInBrowser(myRes);
            }
            else
            {
                myWebBrowser.Navigate("www.google.no/search?q=" + tag.name);
            }
        }

        private Dictionary<string, string> BuildRegexp()
        {
            string regexpXML = global.m_Projects + @"\TransformData\xml\regexpEx01.xml";
            Dictionary<string, string> returnValue = null;
            if (!File.Exists(regexpXML)) return returnValue;
            try
            {
                XElement regExp = XElement.Load(regexpXML);
                ReadRegExExpressionsEx1 rBuild = new ReadRegExExpressionsEx1(0);

                Dictionary<string, string> dict = rBuild.Build_Regexp_Dictionary(regExp);
                returnValue = dict;
            }
            catch (SystemException e)
            {
                MessageBox.Show("BuildRegexp - Error:" + e.Message.ToString());
            }
            return returnValue;
        }

        private string SetFolderName(string s)
        {
            string result = "";
            try
            {
                result = Regex.Replace(s, "[^0-9a-zA-Z]+", "-");
            }
            catch (SystemException err)
            {
            }
            return result;
        }

        private void _project_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (_project.DialogResult != DialogResult.OK) return;

            XElement document = new XElement("document");
            for (int i = 0; i < _project.tb.Count(); i++)
            {
                switch (i)
                {
                    case 0: document.Add(new XElement("adress", _project.tb[i].Text)); break;
                    case 1: document.Add(new XElement("host", _project.tb[i].Text)); break;
                    case 2: document.Add(new XElement("name", _project.tb[i].Text)); break;
                    case 3: document.Add(new XElement("long-name", _project.tb[i].Text)); break;
                    case 4: 
                        document.Add(new XElement("topic_id", _project.tb[i].Text)); break;
                }
            }
            document.Add(new XElement("foldername", SetFolderName(document.Element("name").Value)));
            SetAreaProperties();

            _myXDoc.SetXDoc(document);

            tbTargetDir.Text = global._dbPath.ToString() +  document.Element("foldername").Value + @"\";
            _NEW = true;
            _project = null;
        }
        private string GetHtmlContent(string url)
        {

            string htmlContentText = String.Empty;

            HttpWebRequest httpWebRequest = null;

            HttpWebResponse httpWebResponse = null;

            StreamReader streamReader = null;

            try
            {

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                httpWebRequest.Timeout = 5000;



                streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.Default);

                htmlContentText = streamReader.ReadToEnd();

                streamReader.Close();

                httpWebResponse.Close();

            }

            catch { }

            finally
            {

                httpWebResponse.Close();

                streamReader.Close();

            }

            return htmlContentText;

        }



        private string TrimScript(string htmlDocText)
        {

            string bodyText = "";

            string trimJavascript = "<script type=\"text/javascript\">(.*?)</script>";
            trimJavascript = @"<script(.*?)</script>";

            Regex regexTrimJs = new Regex(trimJavascript);

            bodyText = regexTrimJs.Replace(htmlDocText, "");

            return bodyText;

        }



        private void _myXDoc_Change(object sender)
        {
            
        }


        private void CreateXMLEditor()
        {
            if (myWebBrowser != null)
            {
                splitContainer2.Panel2.Controls.Remove(myWebBrowser);
                myWebBrowser = null;
            }
            myXMLEditor = new XmlEditor();
            splitContainer2.Panel2.Controls.Add(myXMLEditor);
            myXMLEditor.Dock = DockStyle.Fill;
        }
        private void CreateWebBrowser()
        {
            if (myXMLEditor != null)
            {
                splitContainer2.Panel2.Controls.Remove(myXMLEditor);
                myXMLEditor = null;
            }
            myWebBrowser = new System.Windows.Forms.WebBrowser();
            splitContainer2.Panel2.Controls.Add(myWebBrowser);
            myWebBrowser.Dock = DockStyle.Fill;
            myWebBrowser.ScriptErrorsSuppressed = true;
            myWebBrowser.Visible = true;
            myWebBrowser.BringToFront();
            myWebBrowser.Navigate(@"about:blank");
            myWebBrowser.DocumentCompleted +=new WebBrowserDocumentCompletedEventHandler(myWebBrowser_DocumentCompleted);


        }


        private void myWebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_DocumentCompletedAction == WebBrowserActions.loadedit || _DocumentCompletedAction == WebBrowserActions.loadedithidden)
            {
                if (e.Url.ToString().EndsWith("full.html"))
                {
                    HtmlElement el = myWebBrowser.Document.GetElementById("elm1");
                    el.InnerText = _htmlText.Text;

                    switch (_DocumentCompletedAction)
                    {
                        case WebBrowserActions.loadedithidden:
                            {
                                System.Windows.Forms.HtmlDocument doc = myWebBrowser.Document;
                                while (doc.InvokeScript("save") == "none") ;
                                _htmlText.Text = doc.InvokeScript("save").ToString();
                                _DocumentCompletedAction = WebBrowserActions.saveedithidden;
                            }
                            break;
                        default:
                            {
                                _DocumentCompletedAction = WebBrowserActions.none;
                                break;
                            }
                    }
                }
                else
                {
                    _DocumentCompletedAction = WebBrowserActions.none;
                }
            }
            else
            {
                _DocumentCompletedAction = WebBrowserActions.none;
            }
            adress.Text = e.Url.ToString();
        }

        private void btnNavigate_Click(object sender, EventArgs e)
        {
            if (myWebBrowser == null)
                CreateWebBrowser();
            myWebBrowser.Navigate(adress.Text);
            _DocumentCompletedAction = WebBrowserActions.normalnavigation;
            
        }

        private void StartMark()
        {
            myWebBrowser.Document.MouseOver += new HtmlElementEventHandler(document_MouseOver);
            myWebBrowser.Document.MouseLeave += new HtmlElementEventHandler(document_MouseLeave);
            myWebBrowser.Document.MouseDown += new HtmlElementEventHandler(document_MouseDown);
            myWebBrowser.Document.ContextMenuShowing += new HtmlElementEventHandler(WebContextMenuShowing);

            System.Windows.Forms.HtmlDocument d = myWebBrowser.Document;
            foreach (HtmlElement e in d.All)
            {

                if (!this.elementStyles.ContainsKey(e))
                {
                    string style = e.Style;
                    this.elementStyles.Add(e, style);
                }
            }
        }


        private void StopMark()
        {
            btnSaveHtml.Enabled = false;
            _htmlText.Text = "";
            System.Windows.Forms.HtmlDocument cd = myWebBrowser.Document;
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

            myWebBrowser.Document.MouseOver -= document_MouseOver;
            myWebBrowser.Document.MouseLeave -= document_MouseLeave;
            myWebBrowser.Document.MouseDown -= document_MouseDown;
            myWebBrowser.Document.ContextMenuShowing -= WebContextMenuShowing;

            tbElName.Text = "";
            tbMarked.Text = "";

        }



        private void WebContextMenuShowing(object sender, HtmlElementEventArgs e)
        {
            e.ReturnValue = false;
        }


        private void document_MouseOver(object sender, HtmlElementEventArgs e)
        {
            _element = e.ToElement;

            if (!this.elementStyles.ContainsKey(_element))
            {
                string style = _element.Style;
                this.elementStyles.Add(_element, style);
                _element.Style = style + "; position:static; background-color: #ffc; border:solid 1px black;";
                tbElName.Text = _element.GetAttribute("className").ToString() ?? "(no id)";
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
            System.Windows.Forms.HtmlDocument CurrentDocument = myWebBrowser.Document;
            
            //_element = CurrentDocument.GetElementFromPoint(MPoint);
            _element = CurrentDocument.GetElementFromPoint(e.ClientMousePosition);

            string style="";
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

            tbMarked.Text = _element.GetAttribute("className").ToString() ?? "(no id)";

            string idx = _element.GetAttribute("idx").ToString() ?? "(no id)";

            if (idx != "")
            {
                XElement document = XElement.Load(adress.Text);
                XElement el = document.Descendants().Where(p => (p.Attribute("idx") == null ? "" : p.Attribute("idx").Value) == idx).FirstOrDefault();
                if (el != null)
                {
                    btnSaveHtml.Enabled = true;
                    
                    XElement h = new XElement("html",
                        new XElement("h1", "Dokument"),
                        el.Nodes()
                        );
                    
                    _htmlText.Text = h.ToString();
                }
            }
            else if (_element != null && btnSaveHtml.Enabled)
            {
                string html = _element.InnerHtml;
                html = html.CleanHtml();
                _htmlText.Text = "<html>" + html + "</html>";
            }

            

            btnSaveHtml.Enabled = true;
            if (_project != null)
            {
                _project.Activate();
                _project.SetTextBoxValue(_element.InnerText);
            }

        }

        private void btnStartM_Click(object sender, EventArgs e)
        {
            StartMark();
        }

        private void btnStopM_Click(object sender, EventArgs e)
        {
            StopMark();
        }

        private void adress_TextChanged(object sender, EventArgs e)
        {

        }

        private void adress_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                if (adress.Text.EndsWith(".xml"))
                {
                    CreateXMLEditor();
                    myXMLEditor.Text = XElement.Load(adress.Text).ToString();
                }
                else
                {
                    btnNavigate.Select();
                    btnNavigate_Click(sender, e);
                }
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                adress.Text = Clipboard.GetText(TextDataFormat.Text);
            }

        }

        private HtmlNode GetDocumentContent(HtmlAgilityPack.HtmlDocument h)
        {
            HtmlNode returnValue = null;
            if (tbDocumentConent.Text.ToString().Split('|').Count() == 1)
            {
                if (h.DocumentNode
                    .Descendants()
                    .Where(p => (
                            p.Attributes["class"] != null ?
                                p.Attributes["class"].Value 
                                : 
                                ""
                        ) == tbDocumentConent.Text.ToString()).Count() != 1)
                {
                    MessageBox.Show("Kan ikke finne ett element!");
                    return null; ;
                }
                else
                {
                    returnValue = h.DocumentNode.Descendants().Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == tbDocumentConent.Text.ToString()).First();
                }
            }
            else if (tbDocumentConent.Text.ToString().Split('|').Count() == 2)
            {
                string class1 = tbDocumentConent.Text.ToString().Split('|').ElementAt(0);
                string class2 = tbDocumentConent.Text.ToString().Split('|').ElementAt(1);


                if (h.DocumentNode.Descendants().Where(p => p.NodeType == HtmlNodeType.Comment && p.InnerText.Contains(" INNHOLD ")).Count() > 0)
                {
                    HtmlNode parent = h.DocumentNode.Descendants().Where(p => p.NodeType == HtmlNodeType.Comment && p.InnerText.Contains(" INNHOLD ")).First().ParentNode;
                    return parent;
                }

                if (h.DocumentNode
                    .Descendants()
                    .Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == class2
                        && p.Ancestors().Where(o => (o.Attributes["class"] != null ? o.Attributes["class"].Value : "") == class1).Count()==1
                        )
                    .Count() != 1)
                {
                    returnValue = h.DocumentNode
                    .Descendants()
                    .Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == class2
                        && p.Ancestors().Where(o => (o.Attributes["class"] != null ? o.Attributes["class"].Value : "") == class1).Count() == 1
                        )
                    .First();
                }
                else
                {
                    MessageBox.Show("Kan ikke finne ett element!");
                    return null; ;
                }

            }
            return returnValue;
        }

        private XElement GetLinksInPage(HtmlAgilityPack.HtmlDocument h
            , Uri uri
            , string savePath )
        {
            XElement links = new XElement("links");
            foreach (HtmlNode n in h.DocumentNode.Descendants().Where(p => p.NodeType == HtmlNodeType.Element))
            {
                switch (n.Name)
                {
                    case "link":
                        if (n.Attributes["href"] != null)
                        {
                            HtmlAttribute att = n.Attributes["href"];
                            if (!att.Value.Trim().StartsWith("#"))
                            {
                                Uri r_uri = new Uri(uri, att.Value);
                                string filename = Path.GetFileName(r_uri.LocalPath);
                                if (filename != "")
                                {
                                    att.Value = savePath + filename;
                                    if (!File.Exists(savePath + filename))
                                    {
                                        mDownloadFile(r_uri.AbsoluteUri, savePath, filename);

                                        try
                                        {
                                            TextReader tr = new StreamReader(savePath + filename);
                                            // read a line of text
                                            string css = tr.ReadToEnd();
                                            tr.Close();

                                            metods met = new metods();
                                            met.GetCssProperties(css, ref _STYLES, ref _CSS);
                                            // close the stream
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    links.Add(new XElement("link"
                                        , new XAttribute("rel", "stylesheet")
                                        , new XAttribute("type", "text/css")
                                        , new XAttribute("href", @"links/" + filename)
                                        , new XAttribute("media", "all")));
                                }
                            }
                        }
                        break;
                }

            }
            return links;
        }

        private void ReprintHtmlReferences(ref HtmlNode hn
            , Uri uri
            , string imagePath)
        {
            foreach (HtmlNode n in hn.ChildNodes.Descendants().Where(p => p.NodeType == HtmlNodeType.Element))
            {
                switch (n.Name)
                {
                    case "a":
                        if (n.Attributes["href"] != null)
                        {
                            HtmlAttribute att = n.Attributes["href"];
                            if (!att.Value.Trim().StartsWith("#"))
                            {
                                Uri r_uri = new Uri(uri, att.Value);
                                att.Value = r_uri.AbsoluteUri;
                            }
                        }
                        break;
                    case "img":
                        
                        if (n.Attributes["src"] != null)
                        {
                            if (!Directory.Exists(imagePath)) Directory.CreateDirectory(imagePath);
                            HtmlAttribute att = n.Attributes["src"];
                            if (!att.Value.Trim().StartsWith("#"))
                            {
                                Uri r_uri = new Uri(uri, att.Value);
                                string filename = Path.GetFileName(r_uri.LocalPath);
                                att.Value = imagePath + filename;
                                mDownloadFile(r_uri.AbsoluteUri, imagePath, filename);
                            }
                        }
                        else
                        {
                            n.InnerHtml = "";
                        }
                        break;
                }

            }

        }

        private List<cssValues> _CSS = null;

        private List<string> _STYLES;


        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                _STYLES = new List<string>();
                _CSS = new List<cssValues>();
                _STATUSTEXT.Text = "Importere liste";
                _STATUSTEXT.Owner.Refresh();
                _PROGRESS.Value = 0;
                _PROGRESS.Maximum = 2;
                this.UseWaitCursor = true;
                int elementCounter = 0;
                lbParts.Items.Clear();

                //Start Sett adresse til importdocument
                string importAdress = "";
                XElement hrefs = _myXDoc.GetXDoc();
                if ((hrefs.Element("adress") == null ? "" : hrefs.Element("adress").Value.Trim()) == "")
                {
                    MessageBox.Show("Dokument er ikke valgt!");
                    return;
                }
                importAdress = hrefs.Element("adress").Value;

                if (adress.Text != importAdress)
                {
                    adress.Text = importAdress;
                    myWebBrowser.Navigate(adress.Text);
                }
                this.Refresh();
                //Slutt Sett adresse til importdocument

                //Start Sett targetPath                

                if ((hrefs.Element("foldername") == null ? "" : hrefs.Element("foldername").Value.Trim()) == "")
                {
                    MessageBox.Show("Mappenavn er ikke angitt!");
                    return;
                }
                string targetPath = global._dbPath + hrefs.Element("foldername").Value + @"\";
                if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
                if (!targetPath.SetFolderName()) return;
                tbTargetDir.Text = targetPath;
                if (tbTargetDir.Text == "") return;

                //Start Sett targetPath                

                _PROGRESS.Value = 1;

                btnImportIndex_Click(sender, e);

                _PROGRESS.Value = 2;
                if (_myXDoc.GetXDoc().Elements().Where(p => p.Name.LocalName == "parts").Count() == 0)
                {
                    return;
                }

                  
                hrefs = _myXDoc.GetXDoc().Elements("parts").First();

                XElement root = null;
                if (File.Exists(global._dbPath + global._dbArea + @"_elements.xml"))
                {
                    root = XElement.Load(global._dbPath + global._dbArea + @"_elements.xml");
                }
                else
                    root = new XElement("root");

                XElement lroot = new XElement("root");
                int fileNumber = 1;
                int max = hrefs.Descendants("part").Count();
                _PROGRESS.Value = 0;
                _PROGRESS.Maximum = max;

                int docNo = 0;

                foreach (XElement href in hrefs.Descendants("part"))
                {
                    docNo = Convert.ToInt32(href.Attribute("no").Value);
                    _PROGRESS.Value = _PROGRESS.Value + 1;
                    _STATUSTEXT.Text = "Importere fil " + _PROGRESS.Value.ToString() + " av " + _PROGRESS.Maximum;
                    _STATUSTEXT.Owner.Refresh();
                    string url = href.Element("href").Value;
                    Uri uri = new Uri(url);
                    string imagePath = targetPath + @"images\";
                    string err = "";
                    //XElement ehtml = global.GetWebPageHTML(url, ref err);
                    //if (ehtml == null)
                    //{
                    //    MessageBox.Show(err);
                    //    return;
                    //}
                    //ehtml = ehtml.Descendants("html").FirstOrDefault();

                    string html = DownloadWebPage(url);
                    
                    html = html.CleaningInvisibleSymbols();
                    
                    HtmlAgilityPack.HtmlDocument h = new HtmlAgilityPack.HtmlDocument();
                    h.OptionWriteEmptyNodes = true;
                    //h.LoadHtml(ehtml.ToString());
                    h.LoadHtml(html);

                    //if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

                    string subPath = targetPath + @"links\";
                    if (!subPath.SetFolderName()) return;

                    XElement links = GetLinksInPage(h, uri, subPath);

                    HtmlNode hn = null;

                    hn = GetDocumentContent(h);

                    ReprintHtmlReferences(ref hn, uri, imagePath);

                    string filename = "";
                    if (!uri.LocalPath.EndsWith("/"))
                    {
                        filename = Path.GetFileName(uri.LocalPath).Replace(Path.GetExtension(uri.LocalPath), "");
                    }
                    if (filename == "") filename = fileNumber.ToString();

                    string hnHtml = hn.OuterHtml;

                    XElement El = SaveCleanHtml(
                        hnHtml
                        , fileNumber
                        , targetPath
                        , uri
                        , filename
                        , links
                        , href);

                    SaveXMLResult(
                        El
                        , targetPath
                        , filename
                        , href
                        , ref root
                        , lroot
                        , false
                        , ref elementCounter
                        , docNo);
                    fileNumber++;
                }

                _myXDoc.SetXDoc(hrefs.Parent);
                _NEW = false;
                if (_STYLES.Count() != 0)
                {
                    TextWriter tw = new StreamWriter(targetPath + @"style.css");
                    foreach (string c in _STYLES.OrderBy(p => p))
                    {
                        tw.WriteLine(c);
                    }
                    tw.Close();
                }


                FixNumbersParts(hrefs, targetPath);

                //int n = 0;
                //XElement numbers = new XElement("numbers");
                //bool firstFixed = false;
                //foreach (XElement href in hrefs.Descendants("part"))
                //{
                //    XElement fd = XElement.Load(targetPath +  href.Element("xml").Value);
                //    if (fd.DescendantsAndSelf("section").First().Attribute("id") != null)
                //    {

                //        string kn = fd.DescendantsAndSelf("section").First().Attribute("id").Value;
                //        if (kn.StartsWith("KAP"))
                //        {
                //            kn = kn.Replace("KAP", "");
                //            int x = -1;
                //            try
                //            {
                //                x = Convert.ToInt32(kn);
                //            }
                //            catch
                //            {
                //                MessageBox.Show("Feil kapittelnummer!");
                //                return;
                //            }
                //            n = x;

                //            if (n > -1 && !firstFixed)
                //            {

                //                int xx = numbers.Elements("number").Count();
                //                int reverse = 1;
                //                for (int i = xx; i > 0; i--)
                //                {
                //                    XElement numb = numbers.Elements("number").ElementAt(i - 1);
                //                    numb.Attribute("n").Value = (n - reverse).ToString();
                //                    reverse++;
                //                }
                //                firstFixed = true;
                //            }
                //            kn = n.ToString();

                //            numbers.Add(new XElement("number"
                //                    , new XAttribute("n", n.ToString())
                //                    , new XAttribute("number", n.ToString())
                //                    , new XAttribute("xml", href.Element("xml").Value)
                //                    ));
                //        }
                //        else
                //        {
                //            numbers.Add(new XElement("number"
                //                    , new XAttribute("n", n.ToString())
                //                    , new XAttribute("number", n.ToString())
                //                    , new XAttribute("xml", href.Element("xml").Value)
                //                    , new XAttribute("noid", "1")
                //                    ));
                //        }
                //    }
                //    else
                //    {
                //        numbers.Add(new XElement("number"
                //                    , new XAttribute("n", n.ToString())
                //                    , new XAttribute("number", n.ToString())
                //                    , new XAttribute("xml", href.Element("xml").Value)
                //                    , new XAttribute("noid", "1")
                //                        ));
                //    }

                //    n++;
                //}

                //foreach (XElement number in numbers.Elements("number").Where(p=>p.Attribute("noid")!=null))
                //{
                //    XElement last = XElement.Load(targetPath + number.Attribute("xml").Value);
                //    if (last.DescendantsAndSelf("section").First().Attribute("id") == null)
                //    {
                //        XAttribute lastId = new XAttribute("id", "KAP" + number.Attribute("number").Value);
                //        last.DescendantsAndSelf("section").First().Add(lastId);
                //        IdentyfiAllSections(last);
                //    }
                //    else
                //    {
                //        last.DescendantsAndSelf("section").First().Attribute("id").Value = "KAP" + number.Attribute("number").Value;
                //        IdentyfiAllSections(last);
                //    }
                //    last.Save(targetPath + number.Attribute("xml").Value);
                //}



                _PROGRESS.Value = 0;
                _STATUSTEXT.Text = "Importert " + _PROGRESS.Maximum + " filer";

            }
            catch (SystemException err)
            {
                Debug.Print(err.Message);
            }
            this.UseWaitCursor = false;
        }


        private bool StatValueExists(string startValue, List<string> startValues)
        {
            string result = startValues.Find(
                            delegate(string p)
                            {
                                return p == startValue;
                            });
            if (result == null)
            {
                startValues.Add(startValue);
                return false;
            }
            else
            {
                return true;
            }
        }
        
        private string FixNumbersPartCheckFirstSection(XElement sections
                                                   , string parentid
                                                   , List<string> startValues)
        {
            string returnValue = "";
            bool setId = false;
            if (parentid != "")
            {
                int j = sections.DescendantsAndSelf("section").Where(p => p.Attribute("id") != null).Count();
                for (int i = 0; i < j; i++)
                {
                    XElement s = sections.DescendantsAndSelf("section").Where(p => p.Attribute("id") != null).ElementAt(i);
                        
                    if (s.Attribute("id").Value.StartsWith("KAP"))
                    {
                        string startValue = "";
                        if (parentid != "")                             
                            startValue = parentid + "." + s.Attribute("id").Value.Replace("KAP", "");
                        else
                            startValue = s.Attribute("id").Value;

                        if (i == 0)
                        {
                            if (StatValueExists(startValue, startValues)) 
                            {
                                setId = true;
                                returnValue = "";
                            }
                            else
                            {
                                if (parentid != "")
                                {
                                    setId = true;
                                    returnValue = startValue;
                                }
                                else
                                    return startValue;
                            }
                        }
                        if (setId)
                        {
                            s.Attribute("id").Value = startValue;
                        }
                        else
                        {
                            s.Attribute("id").Value = "";
                        }
                    }
                }
            }
            return returnValue; 
        }        
        
        private void FixNumbersParts(XElement parts, string targetPath)
        {
            List<string> startValues = new List<string>();
            XElement numbers = new XElement("numbers");
            int nContainers = 0;
            int nParts = 0;
            foreach (XElement part in parts.Elements())
            {
                if (part.Name.LocalName == "container")
                {
                    nContainers++;
                    string parentid = "DEL" + nContainers;
                    foreach (XElement subPart in part.Elements("part"))
                    {
                        nParts++;
                        XElement fd = XElement.Load(targetPath + subPart.Element("xml").Value);
                        FixNumbersPartCheckFirstSection(fd, parentid, startValues);
                        fd.Save(targetPath + subPart.Element("xml").Value);
                    }
                }
                else if (part.Name.LocalName == "part")
                {
                    nParts++;
                    XElement fd = XElement.Load(targetPath + part.Element("xml").Value);
                    FixNumbersPartCheckFirstSection(fd, "", startValues);
                    fd.Save(targetPath + part.Element("xml").Value);
                }
            }
         
        }

        private void SaveXMLResult(
            XElement El
            , string path
            , string fileName
            , XElement part
            , ref XElement root
            , XElement lroot
            , bool singlePart
            , ref int elementCounter
            , int docNo)
        {
            if (part.Ancestors("document").First().Element("host").Value == "regjeringen")
            {
                int counter = 0;
                HtmlHierarchy h = new HtmlHierarchy();
                h.SetElementId(El, ref counter);

                XElement res = null;

                if (!singlePart)
                    res = CleanRegjeringen(El, docNo);
                else
                {
                    res = El;
                }
                if (res == null) return;


                GetElements(res, ref lroot);
                lroot.Save(path + fileName + "_elements.xml");

                GetElements(res, ref root);

                if (!Directory.Exists(path + "xml"))
                    Directory.CreateDirectory(path + "xml");

                if (res.Name.LocalName != "document")
                {
                    XElement section = new XElement("document"
                    , new XElement("title", part.Elements("name").First().Value)
                    , res.Nodes()
                    );
                    res = new XElement(section);
                }
                

                h.SetElementId(res, ref elementCounter);


                res.Save(path + @"xml\" + fileName + ".xml");
                if (part.Element("xml") == null)
                    part.Add(new XElement("xml", @"xml\" + fileName + ".xml"));
                else
                    part.Element("xml").Value = @"xml\" + fileName + ".xml";

                root.Save(global._dbPath + part.Ancestors("document").First().Element("host").Value + @"_elements.xml");
            }
            else if (part.Ancestors("document").First().Element("host").Value == "stortinget")
            {
                int counter = 0;

                string titleText = part.Elements("name").First().Value.Trim();
                string startVal = "";
                if (Regex.IsMatch(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))(\.)?\s"))
                {
                    string nr = Regex.Match(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))(\.)?\s").Groups["nr"].Value;
                    if (nr != "")
                    {
                        startVal = nr;
                    }
                }

                HtmlHierarchy h = new HtmlHierarchy();
                XElement res = null;
                h.SetElementId(El, ref counter);
                if (!singlePart)
                    res = CleanStortinget(El, startVal);
                else
                {
                    res = El;
                }

                if (res == null) return;

                GetElements(res, ref lroot);
                lroot.Save(path + fileName + "_elements.xml");

                GetElements(res, ref root);

                if (!Directory.Exists(path + "xml"))
                    Directory.CreateDirectory(path + "xml");

                XElement section = new XElement("document"
                        ,new XElement("section"
                            , new XAttribute("id", (startVal == "" ? "" : "KAP" + startVal) )
                            , new XElement("title", part.Elements("name").First().Value)
                            , res.Nodes())
                );

                res = new XElement(section);

                h.SetElementId(res, ref elementCounter);

                res.Save(path + @"xml\" + fileName + ".xml");
                if (part.Element("xml") == null)
                    part.Add(new XElement("xml", @"xml\" + fileName + ".xml"));
                else
                    part.Element("xml").Value = @"xml\" + fileName + ".xml";

                root.Save(global._dbPath + part.Ancestors("document").First().Element("host").Value + @"_elements.xml");
            }
        }

        private XElement SaveCleanHtml(string hnHtml
            , int fileNumber
            , string path
            , Uri uri
            , string fname
            , XElement links
            , XElement part)
        {
            hnHtml = hnHtml.CleanHtml();
            
            XDocument d = XDocument.Parse(hnHtml);
            XElement El = d.Root;
            

            if (fname == "") fname = fileNumber.ToString();
            XElement org_res = new XElement("html");
            org_res.Add(links.Nodes());
            org_res.Add(new XElement("body", new XAttribute("style", "font-size:12px;"), new XElement("div", new XAttribute("class", "AreaFullPageCenter"), new XElement(El))));

            if (org_res != null) org_res.Save(path + fname + ".html");
            if (part.Element("html") == null)
                part.Add(new XElement("html", fname + ".html"));
            else
                part.Element("html").Value = fname + ".html";

            

            if (!Directory.Exists(path + "clean"))
                Directory.CreateDirectory(path + "clean");

            El.Save(path + @"clean\" + fname + ".xml");

            if (part.Element("clean") == null)
                part.Add(new XElement("clean", @"clean\" + fname + ".xml"));
            else
                part.Element("clean").Value = fname + ".xml";

            int m = El.DescendantNodes().OfType<XText>().Count();
            for (int j = m; j > 0; j--)
            {
                XText n = El.DescendantNodes().OfType<XText>().ElementAt(j - 1);
                n.ReplaceWith(new XText(Regex.Replace(n.ToString(), Environment.NewLine.ToString(), " ")));
            }

            return El;
        }

        
        private void GetElements(XElement document, ref XElement root)
        {
            foreach (XElement e in document.DescendantsAndSelf())
            {
                int n = e.AncestorsAndSelf().Count();
                XElement level = root;
                for (int i = n; i > 0; i--)
                {
                    XElement anc = e.AncestorsAndSelf().ElementAt(i-1);
                    XElement test = new XElement(anc.Name.LocalName);
                    if (anc.Attribute("class")!=null)
                    {
                        test.Add(new XAttribute(anc.Attribute("class")));
                    }
                    if (level.Elements()
                        .Where(p => p.Name.LocalName == test.Name.LocalName 
                            && (p.Attribute("class")==null ? "" : p.Attribute("class").Value)
                                == (test.Attribute("class")==null ? "" : test.Attribute("class").Value)
                                )
                        .Count() == 0)
                    {
                        level.Add(test);
                        level = test;
                    }
                    else
                    {
                        level = level.Elements()
                        .Where(p => p.Name.LocalName == test.Name.LocalName
                            && (p.Attribute("class") == null ? "" : p.Attribute("class").Value)
                                == (test.Attribute("class") == null ? "" : test.Attribute("class").Value)
                                ).First();
                    }
                }
                    
            }
        }


        private void CheckSections(XElement toc, string parentValue)
        {
            foreach (XElement s in toc.Elements("section"))
            {
                string titleValue = "";
                titleValue = CheckTitleNumber(s, parentValue);
                CheckSections(s, titleValue);
            }

        }

        private string CheckTitleNumber(XElement section, string parentValue)
        {
            string returnValue = "";
            if (section.Element("title") != null)
            {
                XElement e = section.Element("title");
                string titleText = e.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate();
                if (Regex.IsMatch(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))(\.)?\s"))
                {
                    string nr = Regex.Match(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))(\.)?\s").Groups["nr"].Value;
                    if (nr != "" && nr.StartsWith(parentValue) && parentValue != "")
                    {
                        returnValue = nr;
                        if (e.Parent.Attribute("id") == null)
                        {
                            e.Parent.Add(new XAttribute("id", "KAP" + nr));
                        }
                        else
                        {
                            e.Parent.Attribute("id").Value = "KAP" + nr;
                        }

                    }
                }
            }
            return returnValue;
        }

        private XElement CleanStortinget(XElement el, string startVal)
        {
            XElement result = null;

            el.Nodes().Where(p => p.NodeType == XmlNodeType.Comment).Remove();
            int t = el.Nodes().Where(p => p.NodeType == XmlNodeType.Text).Count();
            for (int i = t; i > 0; i--)
            {
                XNode no = el.Nodes().Where(p => p.NodeType == XmlNodeType.Text).ElementAt(i-1);
                no.ReplaceWith(new XElement("p", no));
            }

            el.Descendants("div").Where(p => p.Nodes().Count() == 0).Remove();
            if ((el.Elements().Count() == 1 ? el.Elements().First().Name.LocalName : "") == "div")
            {
                el.Elements().First().ReplaceWith(el.Elements().First().Nodes());
            }

            //fjern alle span med italic
            int n = el.Descendants("span").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "innst-italic").Count();
            for (int i = n; i > 0; i--)
            {
                XElement test = el.Descendants("span").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "innst-italic").ElementAt(i - 1);
                XElement newEl = new XElement("i", test.Attribute("idx"), test.Nodes());
                test.ReplaceWith(newEl);
            }

            //fjern alle span
            n = el.Descendants("span").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "").Count();
            for (int i = n; i > 0; i--)
            {
                XElement test = el.Descendants("span").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "").ElementAt(i - 1);
                XElement newEl = new XElement("i", test.Attribute("idx"), test.Nodes());
                test.ReplaceWith(newEl);
            }

            n = el.Descendants().Where(p => p.Attribute("class") != null).Count();
            for (int i = n; i > 0; i--)
            {
                XElement test = el.Descendants().Where(p => p.Attribute("class") != null).ElementAt(i - 1);
                if (test.Attribute("class").Value != "")
                {
                    XAttribute newAtt = new XAttribute("type", test.Attribute("class").Value);
                    test.Attribute("class").Remove();
                    test.Add(newAtt);
                }
            }


            //finn lavest header nivå
            HtmlHierarchy h = new HtmlHierarchy();
            int counter = 1;
            h.SetElementId(el, ref counter);

            if (el.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0)
            {
                XElement toc = h.GetHtmlToc(el);
                XElement newS = new XElement(el.Name.LocalName, el.Attributes());
                if (toc == null)
                {
                    newS.Add(new XElement("section"
                            , el.Nodes()
                        ));
                }
                else
                {
                    CheckSections(toc, startVal);


                    newS.Add(el.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(toc.Elements("section").First().Attribute("idx").Value)));
                    newS.Add(toc.Nodes());
                    n = newS.Descendants("section").Count();

                    for (int i = 0; i < n; i++)
                    {
                        XElement sect = newS.Descendants("section").ElementAt(i);

                        XElement next = null;
                        if (i + 1 < n)
                            next = toc.Descendants("section").ElementAt(i + 1);

                        if (next != null)
                        {
                            sect.Element("title").AddAfterSelf(el.Elements().Where(p =>
                                Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                                &&
                                Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("idx").Value)
                                ));
                        }
                        else
                        {
                            sect.Element("title").AddAfterSelf(el.Elements().Where(p =>
                                Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                            ));

                        }
                    }
                }

                result = newS;
            }
            else
            {
                result = el;
            }

            return result; 
        }

        private XElement BuildSectionsFromHX(XElement el, string startVal)
        {
            XElement result = null;
            //finn lavest header nivå
            HtmlHierarchy h = new HtmlHierarchy();
            int counter = 1;
            h.SetElementId(el, ref counter);

            if (el.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0)
            {
                XElement toc = h.GetHtmlToc(el);

                CheckSections(toc, startVal);

                XElement newS = new XElement(el.Name.LocalName, el.Attributes());

                newS.Add(el.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(toc.Elements("section").First().Attribute("idx").Value)));
                newS.Add(toc.Nodes());
                int n = newS.Descendants("section").Count();

                for (int i = 0; i < n; i++)
                {
                    XElement sect = newS.Descendants("section").ElementAt(i);

                    XElement next = null;
                    if (i + 1 < n)
                        next = toc.Descendants("section").ElementAt(i + 1);

                    if (next != null)
                    {
                        sect.Element("title").AddAfterSelf(el.Elements().Where(p =>
                            Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                            &&
                            Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("idx").Value)
                            ));
                    }
                    else
                    {
                        sect.Element("title").AddAfterSelf(el.Elements().Where(p =>
                            Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                        ));

                    }
                }


                result = newS;
            }
            else
            {
                result = el;
            }
            return result;
        }
        private XElement CleanRegjeringen(XElement el, int docNo)
        {
            XElement result = null;
            if (el.DescendantsAndSelf().Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-DEL-") != -1).Count() != 0)
            {
                XElement footnotes = null;
                foreach (XElement f in el.DescendantsAndSelf("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "EG-FNOTE"))
                {
                    XElement fn = new XElement(f);
                    if (footnotes == null)
                        footnotes = new XElement("section"
                            , new XElement ("title", "Fotnoter")
                            );
                    XElement a = new XElement(fn.Elements("a").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "EG-FNOTEA").First());
                    fn.Elements("a").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "EG-FNOTEA").First().Remove();

                    string name = a.Attribute("name").Value;
                    string href = a.Attribute("href").Value.Replace("#","");

                    if (el.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == href).Count() != 0)
                    {
                        int n = el.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == href).Count();
                        for (int i = n; i > 0; i--)
                        {
                            XElement ar = el.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == href).ElementAt(i - 1);
                            XElement fp = ar.Parent;
                            XElement xref = new XElement("footnote"
                                , new XAttribute("ref", docNo + "_" + name)
                                , new XAttribute("type", "sectionnote")
                                , ar.Value
                                );
                            fp.ReplaceWith(xref);
                        }

                    }

                    XElement footnote = new XElement("section"
                            , new XAttribute("id", docNo + "_" + name)
                            , new XElement("title","[" + a.Value.Trim() + "]")
                        );
                    XElement t = new XElement(fn.Descendants("span").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-NOTETEXT").First());
                    footnote.Add(t.Nodes());
                    footnotes.Add(footnote);
                }
                result = el.DescendantsAndSelf().Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-DEL-") != -1).First().Parent;
                result = BuildXMLTree(result);
                if (footnotes != null)
                {
                    if (result.Descendants("section").Count()!=0)
                        result.Descendants("section").First().Add(footnotes);
                }

            }
            else
                result = el;
            return result;
        }

        private XElement BuildXMLTree(XElement el)
        {
            XElement result = new XElement("document");
            XElement p1 = null;
            XElement p2 = null;
            XElement p3 = null;
            XElement p4 = null;
            XElement p5 = null;
            XElement p6 = null;
            XElement p7 = null;
            XElement p8 = null;
            XElement p10 = null;
            int n = el.Elements().Count();
            for (int i = 0; i<n;i++)
            {
                XElement e = el.Elements().ElementAt(i);
                XElement last = null;
                string className = e.Attribute("class")==null ? "" : e.Attribute("class").Value;
                switch (className)
                {
                    case "O-DEL-NIVA-1":
                        p1 = new XElement("section", e.Attribute("idx"), e.Attribute("class"), new XAttribute("level","1"), e.Nodes());
                        last = p1;
                        result.Add(p1);
                        break;
                    case "K-DEL-NIVA-2":
                        p2 = new XElement("section", e.Attribute("idx"), e.Attribute("class"), new XAttribute("level", "2"), e.Nodes());
                        last = p2;
                        if (p1 == null)
                        {
                            result.Add(p2);
                        }
                        else
                            p1.Add(p2);
                        break;
                    case "K-DEL-NIVA-3":
                        p3 = new XElement("section", e.Attribute("idx"), e.Attribute("class"), new XAttribute("level", "3"), e.Nodes());
                        last = p3;
                        p2.Add(p3);
                        break;
                    case "K-DEL-NIVA-4":
                        p4 = new XElement("section", e.Attribute("idx"), e.Attribute("class"), new XAttribute("level", "4"), e.Nodes());
                        last = p4;
                        p3.Add(p4);
                        break;
                    case "K-DEL-NIVA-5":
                        p5 = new XElement("section", e.Attribute("idx"), e.Attribute("class"), new XAttribute("level", "5"), e.Nodes());
                        last = p5;
                        p4.Add(p5);
                        break;
                    case "K-DEL-SUBSEK2":
                        p10 = new XElement("section", e.Attribute("idx"), e.Attribute("class"), new XAttribute("level", "5"), e.Nodes());
                        last.Add(p10);
                        break;
                }
            }
            
            result = CheckNodes(result);

            return result;
        }

        private XElement CheckNodes(XElement el)
        {
            XElement result = new XElement(el);



            int n = 0;
            foreach (XElement e in result.Descendants().Where(p => p.Elements("p").Count() != 0 && p.Name.LocalName != "p"))
            {
                n = e.Nodes().Count();
                XElement container = null;
                for (int i = n; i > 0;i-- )
                {
                    
                    XNode no = e.Nodes().ElementAt(i - 1);
                    if (no.NodeType == XmlNodeType.Element)
                    {
                        XElement test = (XElement)no;
                        if (test.Name.LocalName == "p" && container != null)
                        {
                            test.Add(container.Nodes());
                            container = null;
                        }
                        else if (test.Name.LocalName == "div" && (test.Attribute("class") == null ? "" : test.Attribute("class").Value) == "K-NY-ANF")
                        {
                            if (container == null) container = new XElement("container");
                            container.AddFirst(new XElement("ny-anf", test.Attribute("idx"), test.Nodes()));
                            no.Remove();
                        }

                        else if (test.Name.LocalName == "span" && (test.Attribute("class") == null ? "" : test.Attribute("class").Value) == "K-UTHEVET")
                        {
                            if (container == null) container = new XElement("container");
                            container.AddFirst(new XElement("uth", test.Attributes(), test.Nodes()));
                            no.Remove();
                        }
                        else if (test.Name.LocalName == "span" && (test.Attribute("class") == null ? "" : test.Attribute("class").Value) == "K-XREF")
                        {
                            if (container == null) container = new XElement("container");
                            container.AddFirst(new XElement("uth", test.Attributes(), test.Nodes()));
                            no.Remove();
                        }

                        else if (test.Name.LocalName == "b" && (test.Attribute("class") == null ? "" : test.Attribute("class").Value) == "K-b")
                        {
                            if (container == null) container = new XElement("container");
                            container.AddFirst(new XElement("uth", test.Attributes(), test.Nodes()));
                            no.Remove();
                        }

                        else if (container != null)
                        {
                            Debug.Print("hva er feil her!!");
                        }
                    }
                    else if (no.NodeType == XmlNodeType.Text)
                    {
                        if (container == null) container = new XElement("container");
                        XText newN = new XText(no.ToString());
                        no.Remove();
                        container.AddFirst(newN);
                    }
                }
            }

            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-BLOKKSIT").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-BLOKKSIT").ElementAt(i - 1);
                e.ReplaceWith(new XElement("blokk", e.Attribute("idx"), new XAttribute("type", "sitat"), e.Nodes()));
            }

            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-DEL-MEDL").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-DEL-MEDL").ElementAt(i - 1);
                e.ReplaceWith(new XElement("blokk", e.Attribute("idx"), new XAttribute("type", "medlem"), e.Nodes()));
            }

            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-DEL-SEKR").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-DEL-SEKR").ElementAt(i - 1);
                e.ReplaceWith(new XElement("blokk", e.Attribute("idx"),  new XAttribute("type", "medlem"), e.Nodes()));
            }

            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-RAMME").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-RAMME").ElementAt(i - 1);
                e.ReplaceWith(new XElement("blokk", e.Attribute("idx"), new XAttribute("type", "ramme"), e.Nodes()));
            }



            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-DEL-SUBSEK2").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-DEL-SUBSEK2").ElementAt(i - 1);
                e.ReplaceWith(new XElement("section", e.Attribute("idx"), e.Attribute("class"), e.Nodes()));
            }

            //fjerner div rundt tabeller
            
            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-TBL").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-TBL").ElementAt(i - 1);
                if (e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() == 1) 
                {
                    XElement hx = e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).First();
                    hx.ReplaceWith(new XElement("p", new XAttribute("type", "tblTitle"),  hx.Attribute("idx"),  hx.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate()));
                }
                e.ReplaceWith(e.Nodes());
            }




            //fjerner div med class K-FIGGRP
            n = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-FIGGRP").Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-FIGGRP").ElementAt(i - 1);
                if (e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() == 1)
                {
                    XElement hx = e.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).First();
                    hx.ReplaceWith(new XElement("p", new XAttribute("type", "figTitle"), hx.Attribute("idx"), hx.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate()));
                }
                if (e.Elements("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-FIG").Count()!=0)
                {
                    XElement fig = e.Elements("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "K-FIG").First();
                    XElement hx = e.Descendants("img").First();
                    string alt = (hx.Attribute("alt") == null ? "" : hx.Attribute("alt").Value);
                    string src = (hx.Attribute("src") == null ? "" : hx.Attribute("src").Value);
                    FileInfo f = new FileInfo(src);
                    string filenName = f.Name;
                    string topFolder = @"dibimages\";
                    XElement dInfo = _myXDoc.GetXDoc();
                    string areaFolder = dInfo.Element("topic_id").Value;
                    string imgFolder = tbTargetDir.Text + @"output\" + topFolder + areaFolder + @"\";
                    if (!Directory.Exists(imgFolder)) Directory.CreateDirectory(imgFolder);
                    File.Copy(src, imgFolder + filenName);
                    XElement graphic = new XElement("graphic"
                        , new XAttribute("folder", areaFolder)
                        , new XAttribute("file", filenName)
                        );
                    fig.ReplaceWith(graphic);
                }
                e.ReplaceWith(new XElement("blokk", new XAttribute("type", "fig"), e.Attribute("idx"), e.Nodes()));
            }
            
            //setter id på alle sectioner der tittel starter med tall
            n = result.Descendants()
                .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") 
                    && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).Count();
            for (int i = 0; i < n; i++)
            {
                XElement e = result.Descendants()
                    .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") 
                        && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).OrderBy(q=>Convert.ToInt32(q.Attribute("idx").Value)).ElementAt(i);
                if (e.Parent.Name.LocalName == "section" && e == e.Parent.Elements().Where(q=>q.Name.LocalName!="a").First())
                {
                    
                    string titleText = e.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate();
                    if (Regex.IsMatch(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))\s"))
                    {
                        string nr = Regex.Match(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))\s").Groups["nr"].Value;
                        if (nr != "")
                        {
                            if (e.Parent.Attribute("id") == null)
                            {
                                e.Parent.Add(new XAttribute("id", "KAP" + nr));
                            }
                            else
                            {
                                e.Parent.Attribute("id").Value = "KAP" + nr;
                            }

                        }
                        else
                        {
                            MessageBox.Show("ingen id");
                        }
                    }
                    e.ReplaceWith(new XElement("title", e.Attribute("idx"), titleText));
                    n = result.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).Count();
                    i = i - 1;
                }
                else if (e.Parent.Name.LocalName == "blokk")
                {
                    e.ReplaceWith(new XElement("p",e.Attributes(), e.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate()));
                    n = result.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).Count();
                    i = i - 1;
                }
            }


            n = result.Descendants("section")
                .Where(q => q.Attribute("id") != null
                    && q.Elements().Where(p =>
                        Regex.IsMatch(p.Name.LocalName, @"h\d")
                        && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1
                        && ("KAP" + p.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim()).StartsWith(p.Parent.Attribute("id").Value)
                        ).Count() != 0
                    ).Count();

            for (int i = n; i > 0; i--)
            {
                
                XElement e = result.Descendants("section")
                .Where(q => q.Attribute("id")!=null 
                    &&  q.Elements().Where(p=> 
                        Regex.IsMatch(p.Name.LocalName, @"h\d")
                        && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1
                        && ("KAP" + p.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim()).StartsWith(p.Parent.Attribute("id").Value)
                        ).Count() != 0
                    ).ElementAt(i-1);
                XElement newS = new XElement(e.Name.LocalName, e.Attributes());
                XElement currS = null;
                foreach (XElement ee in e.Elements())
                {
                    if (Regex.IsMatch(ee.Name.LocalName, @"h\d")
                        && (ee.Attribute("class") == null ? "" : ee.Attribute("class").Value).IndexOf("-TIT-") != -1
                        && ("KAP" + ee.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim()).StartsWith(ee.Parent.Attribute("id").Value))
                    {
                        string titleText = ee.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim();
                        string nr = Regex.Match(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))\s").Groups["nr"].Value;
                        if (nr != "")
                        {
                            currS = new XElement("section", ee.Attribute("idx")
                                , new XAttribute("id", "KAP" + nr)
                                , new XElement("title", titleText));
                            newS.Add(currS);
                        }
                        else
                        {
                            if (currS != null)
                            {
                                currS.Add(ee);
                            }
                            else
                            {
                                newS.Add(ee);
                            }
                        }
                    }
                    else
                    {
                        if (currS != null)
                        {
                            currS.Add(ee);
                        }
                        else
                        {
                            newS.Add(ee);
                        }

                    }
                }
                e.ReplaceWith(newS);
            }

            SectionVedtaksDel(result);
            SectionTilrarpost(result);

            //ExtractLovDel(result, "K-DEL-LOVER");
            //ExtractLovDel(result, "K-DEL-LOVDEL");
            //ExtractLovDel(result, "K-DEL-LOVKAP");
            //ExtractLovDel(result, "K-DEL-PARAGRAF");

            ConnectLovDel(result);


            n = result.Descendants("section")
                .Where(p => p.Elements()
                    .Where(q=>(q.Attribute("class")==null ? "": q.Attribute("class").Value).IndexOf("-DEL-")!=-1
                        && q.Name.LocalName != "title").Count()==0)
                .Count();
            for (int i = 0; i < n; i++)
            {
                XElement e = result.Descendants("section")
                .Where(p => p.Elements()
                    .Where(q => (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("-DEL-") != -1
                        && q.Name.LocalName != "title").Count() == 0)
                .ElementAt(i);
                int m = e.Descendants()
                    .Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") 
                        && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).Count();
                //for (int j = 0; j < m; j++)
                //{
                //    XElement ee = e.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).ElementAt(j);
                //    Debug.Print("hva her! " + ee.Parent.Name.LocalName + " " + ee.Name.LocalName + " " + (ee.Attribute("class") == null ? "" : ee.Attribute("class").Value) + " text:" + ee.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate());
                //}
                for (int j = m; j > 0; j--)
                {
                    XElement ee = e.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).ElementAt(j - 1);
                    XElement mtit = new XElement("mtit"
                        , new XAttribute(ee.Attribute("idx"))
                        , new XAttribute("type",  ee.Attribute("class").Value)
                        , ee.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim()
                        );
                    ee.ReplaceWith(mtit);
                }
            }




            n = result.Descendants("div").Where(p => (p.Attribute("class") != null ? p.Attribute("class").Value : "") == "K-NY-ANF").Count();

            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") != null ? p.Attribute("class").Value : "") == "K-NY-ANF").ElementAt(i - 1);
                e.ReplaceWith(new XElement("ny-anf", e.Attribute("idx"), e.Nodes()));
            }


            n = result.Descendants("div").Where(p => (p.Attribute("class") != null ? p.Attribute("class").Value : "") == "K-DEL-SUBSEK").Count();

            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants("div").Where(p => (p.Attribute("class") != null ? p.Attribute("class").Value : "") == "K-DEL-SUBSEK").ElementAt(i - 1);
                e.ReplaceWith(e.Nodes());
            }

            n = result.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).Count();
            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d") && (p.Attribute("class") == null ? "" : p.Attribute("class").Value).IndexOf("-TIT-") != -1).ElementAt(i - 1);
                XElement mtit = new XElement("mtit"
                    , new XAttribute(e.Attribute("idx"))
                    , new XAttribute("type", e.Attribute("class").Value)
                    , e.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim()
                    );
                e.ReplaceWith(mtit);
            }

            
            
            //for (int i = 0; i < result.Descendants("div").Count();i++ )
            //{
            //    XElement e = result.Descendants("div").ElementAt(i);
            //    Debug.Print(e.Attribute("class").Value);
            //}

            n = result.Descendants().Where(p => p.Attribute("class") != null).Count();

            for (int i = n; i > 0; i--)
            {
                XElement e = result.Descendants().Where(p => p.Attribute("class") != null).ElementAt(i - 1);
                XAttribute t = new XAttribute("type", e.Attribute("class").Value.ToLower());
                e.Attribute("class").Remove();
                e.Add(t);
            }

            return result;
        }

        private void ConnectLovDel(XElement document)
        {
            List<string> l = new List<string>();
            l.Add("K-DEL-LOVER");
            l.Add("K-DEL-LOVDEL");
            l.Add("K-DEL-LOVKAP");
            l.Add("K-DEL-PARAGRAF");
            l.Add("K-DEL-LOVAVSNITT");
            l.Add("K-DEL-LEDD");

            int n = document.Descendants("div")
                    .Where(p => l.Where(o=>o == (p.Attribute("class") == null ? "" : p.Attribute("class").Value).Trim()).Count()==1
                        && p.Parent.Name=="section"
                    ).Count();
            if (n == 0) return;
            XElement parent = document.Descendants("div")
                    .Where(p => l.Where(o => o == (p.Attribute("class") == null ? "" : p.Attribute("class").Value).Trim()).Count() == 1
                        && p.Parent.Name == "section"
                    )
                    .ElementAt(0).Parent;
            XElement container = new XElement("container");
            while (parent.Descendants("div")
                    .Where(p => l.Where(o => o == (p.Attribute("class") == null ? "" : p.Attribute("class").Value).Trim()).Count() == 1
                    ).Count() != 0)
            {
                n = parent.Descendants("div")
                        .Where(p => l.Where(o => o == (p.Attribute("class") == null ? "" : p.Attribute("class").Value).Trim()).Count() == 1
                        ).Count();

                for (int i = n; i > 0; i--)
                {
                    XElement next = parent.Descendants("div")
                        .Where(p => l.Where(o => o == (p.Attribute("class") == null ? "" : p.Attribute("class").Value).Trim()).Count() == 1
                        ).ElementAt(i - 1);

                    switch (next.Attribute("class") == null ? "" : next.Attribute("class").Value)
                    {
                        case "K-DEL-LEDD":
                            XElement ledd = new XElement("ledd", next.Attribute("idx"), next.Nodes());
                            next.ReplaceWith(ledd);
                            break;
                        default:
                            next.ReplaceWith(next.Nodes());
                            break;
                            
                    }
                    break;
                }
            }
        }

        private void ExtractLovDel(XElement document, string className)
        {
            while (document.Descendants("div")
                    .Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == className
                          && p.Ancestors("div").Count() == 0
                          && !Regex.IsMatch((p.Elements().Count() == 0 ? "" : p.Elements().First().Name.LocalName), @"h\d")
                          )
                  .Count()!=0)
            {
                
                XElement del = document.Descendants("div")
                    .Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == className
                          && p.Ancestors("div").Count() == 0
                          && !Regex.IsMatch((p.Elements().Count() == 0 ? "" : p.Elements().First().Name.LocalName), @"h\d")
                          )
                  .ElementAt(0);
                del.ReplaceWith(del.Nodes());
            }

        }

        private void ReplaceDivClassName(XElement document, string className)
        {
            int n = document.Descendants("div")
                .Where(p => (p.Attribute("class")== null ? "": p.Attribute("class").Value)==className
                        && p.Ancestors("div").Count()==0
                        &&  Regex.IsMatch((p.Elements().Count()==0  ? "" : p.Elements().First().Name.LocalName),@"h\d")
                        )
                .Count();
            for (int i = n; i>0; i--)
            {

                XElement div = document.Descendants("div")
                        .Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == className)
                        .ElementAt(i-1);

                div.ReplaceWith(div.Nodes());    
            }
        }
        
        
        
        
        private void SectionTilrarpost(XElement document)
        {
            int n = document.Descendants("section")
            .Where(p => p.Elements()
                .Where(q => (
                    (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-TILRPOST") != -1
                    || (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-KONGESIDE") != -1
                    )
                    && q.Name.LocalName == "div")
                    .Count() != 0)
            .Count();
            for (int i = 0; i < n; i++)
            {

                XElement section = document.Descendants("section")
                    .Where(p => p.Elements()
                        .Where(q => (
                            (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-TILRPOST") != -1
                            || (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-KONGESIDE") != -1
                            )
                            && q.Name.LocalName == "div")
                            .Count() != 0)
                            .ElementAt(i);

                int m = section.Elements().Where(q => (
                            (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-TILRPOST") != -1
                            || (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-KONGESIDE") != -1
                            )
                            && q.Name.LocalName == "div")
                        .Count();
                for (int j = m; j > 0; j--)
                {
                    XElement div = section.Elements().Where(q => (
                            (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-TILRPOST") != -1
                            || (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-KONGESIDE") != -1
                            )
                            && q.Name.LocalName == "div")
                        .ElementAt(j -1);
                    div.ReplaceWith(new XElement("blokk", div.Attributes(), div.Nodes()));
                }
            }
        }
        
        private void SectionVedtaksDel(XElement document)
        {
            int n = document.Descendants("section")
                .Where(p => p.Elements()
                    .Where(q=>(q.Attribute("class")==null ? "": q.Attribute("class").Value).IndexOf("K-DEL-VEDTAKDEL")!=-1
                        && q.Name.LocalName == "div")
                        .Count()!=0)
                .Count();
            for (int i = 0; i < n; i++)
            {
                
                XElement section = document.Descendants("section")
                .Where(p => p.Elements()
                    .Where(q => (q.Attribute("class") == null ? "" : q.Attribute("class").Value).IndexOf("K-DEL-VEDTAKDEL") != -1
                        && q.Name.LocalName == "div")
                        .Count() != 0)
                .ElementAt(i);
                section.ReplaceWith(ExtractDel(section));
            }
        }

        private XElement ExtractDel(XElement div)
        {
            XElement newS = new XElement("section", div.Attributes());
            int n = div.Elements().Count();
            for (int i = 0; i < n; i++)
            {
                XElement el = div.Elements().ElementAt(i);
                if (el.Name.LocalName == "div" 
                    && (el.Attribute("class") ==null ? "" : el.Attribute("class").Value).IndexOf("-DEL")!=-1
                    )
                {
                    XElement sTest = ExtractDel(el);
                    if ((sTest.Elements().Count() == 0 ? "" : sTest.Elements().First().Name.LocalName) == "title")
                    {
                        newS.Add(sTest);
                    }
                    else
                    {
                        newS.Add(sTest.Nodes());
                    }
                }

                else if (Regex.IsMatch(el.Name.LocalName, @"h\d"))
                {
                    newS.Add(new XElement("title", el.Attribute("idx"), el.DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim()));
                }
                else
                    newS.Add(el);
            }

            if (newS.Elements("title").Count() > 1)
            {
                int nt = newS.Elements("title").Count();
                for (int j = nt; j > 1; j--)
                {
                    XElement t = newS.Elements("title").ElementAt(j - 1);
                    t.ReplaceWith(new XElement("p", new XElement("strong", t.Nodes())));
                }
            }
            return newS;
        }

        private void ViewHtmlInIE()
        {
            if ((_htmlText.Text == null ? "" : _htmlText.Text) == "") return;

            string html = _htmlText.Text;

            StopMark();

            if (!html.StartsWith("html"))
            {
                html = "<html>" + html + "</html>";
            }
            myWebBrowser.DocumentText = html;

        }



        public string DownloadWebPage(string Url)
        {
            // Open a connection
            HttpWebRequest webRequestObject = (HttpWebRequest)HttpWebRequest.Create(Url);

            // You can also specify additional header values like 
            // the user agent or the referer:
            webRequestObject.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US)";
            webRequestObject.Referer = "http://www.google.com/";

            // Will contain the content of the page
            string pageContent = string.Empty;

            // Request response:
            using (WebResponse response = webRequestObject.GetResponse())
            // Open stream and create reader object:
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                // Read the entire stream content:
                pageContent = reader.ReadToEnd();
            }

            return pageContent;
        }

        private bool mDownloadFile(string url, string destPath, string fileName)
        {
            bool returnValue = false;
            try
            {
                WebClient webClient = new WebClient();
                if (!File.Exists(destPath + fileName))
                    webClient.DownloadFile(url, destPath + fileName);
                returnValue = true;
            }
            catch (SystemException e)
            {

            }


            return returnValue;
        }
       
        public void Submit(string html)
        {

            _htmlText.Text = html;
        }


        private void SetArea()
        {
            switch (global._dbArea)
            {
                case @"regjeringen\": rbArea1.Checked = true; break;
                case @"stortinget\": rbArea2.Checked = true; break;
            }
        }

        private void ResetForm()
        {
            lbParts.Items.Clear();
            tbTargetDir.Text = "";
            if (myWebBrowser == null) CreateWebBrowser(); 
        }

        private void btnOpenDoc_Click(object sender, EventArgs e)
        {

            LoadExisting();

            XElement myRes = null;
            
            ResetForm();

            string regEx = _REGEXPQ.Where(p => p.Key == "forarbeider").First().Value;
            frmGetDocuments f = new frmGetDocuments(this);
            f.ShowDialog();
            if (f.DialogResult == DialogResult.OK)
            {
                SetArea();
                if (f.Tag != null)
                {
                    myRes = f.resultXML;
                    _myXDoc.SetXDoc(myRes);
                }
            }
            else return;

            if (myRes != null)
            {
                OpenDocumentInBrowser(myRes);
            }
        }


        private void OpenDocumentInBrowser(XElement myRes)
        {
            string name = "";
            string wAdress  = "";
            if (myRes.Element("foldername") == null)
            {
                XElement folder = new XElement("foldername", SetFolderName(myRes.Element("name").Value));
                myRes.Add(folder);
            }

            tbTargetDir.Text = global._dbPath.ToString() +  myRes.Element("foldername").Value + @"\";
            
            if (myRes.Descendants("part").Count() == 0)
            {
                wAdress = myRes.Element("adress").Value;
            }
            else
            {
                if (myRes.Descendants("part").First().Element("html") != null)
                {
                    wAdress = tbTargetDir.Text + myRes.Descendants("part").First().Element("html").Value;
                    name = myRes.Descendants("part").First().Element("name").Value;
                }
                else
                {
                    wAdress = myRes.Descendants("part").First().Element("href").Value;
                    name = myRes.Descendants("part").First().Element("name") == null ? "" : myRes.Descendants("part").First().Element("name").Value;
                }
            }
            adress.Text = wAdress;
            if (myWebBrowser == null) CreateWebBrowser(); 
            myWebBrowser.Navigate(wAdress);

            UpdatePartNo(myRes);
            LoadPartsList(myRes);

            _DocumentCompletedAction = WebBrowserActions.normalnavigation;
        }



        private void UpdatePartNo(XElement document)
        {
            int partNo = 0;
            if (document.Descendants("part").Where(p => p.Attribute("no") == null).Count() != 0)
            {
                foreach (XElement part in document.Descendants("part"))
                {
                    XAttribute attNo = new XAttribute("no", partNo.ToString());
                    part.Add(attNo);
                    partNo++;
                }
                _myXDoc.SetXDoc(document);
            }
        }
        private void LoadPartsList(XElement parts)
        {
            foreach (XElement part in parts.Descendants("part"))
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = part.Element("name").Value;
                item.Value = Convert.ToInt32(part.Attribute("no").Value);
                lbParts.Items.Add(item);
            }

        }


        private XElement ReadDocument()
        {
            XElement result = null;
            if (adress.Text == "") return result;

            string html = DownloadWebPage(adress.Text);
            HtmlAgilityPack.HtmlDocument h = new HtmlAgilityPack.HtmlDocument();

            h.LoadHtml(html);
            h.OptionWriteEmptyNodes = true;
            XDocument d = null;

            HtmlNode hn = null;
            if (rbArea1.Checked)
            {
                string className = "documentInfo";
                if (h.DocumentNode.Descendants("h1").Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == "K-TIT-PUBLIKASJONSTITTEL FORSIDE").Count() != 1)
                {
                    MessageBox.Show("Kan ikke finne dokumentinfo");
                    return null;

                }
                else
                {
                    hn = h.DocumentNode.Descendants("h1").Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == "K-TIT-PUBLIKASJONSTITTEL FORSIDE").First().ParentNode;
                }
            }
            else if (rbArea2.Checked)
            {
                if (h.DocumentNode.Descendants("a").Where(p => p.InnerText.Trim().ToLower() == "første kapittel").Count() == 0)
                {
                    if (h.DocumentNode.Descendants("a").Where(p => p.InnerText.Trim().ToLower() == "hele publikasjonen").Count() != 0)
                    {
                        MessageBox.Show("Kan ikke importere denne siden, trykk vis 'Hele publikasjonen'!");
                        return null;
                    }
                    
                }
                string className = "mainareamiddle2col";
                //if (h.DocumentNode.Descendants("A").Where(p => (p.Attributes["name"] != null ? p.Attributes["name"].Value : "") == "StartSkriv").Count() != 1)
                //{
                //    HtmlAgilityPack.HtmlNode dn = h.DocumentNode.Descendants("a").Where(p => (p.Attributes["name"] != null ? p.Attributes["name"].Value : "") == "StartSkriv").First().ParentNode.ParentNode;
                //    XElement newHtm = new XElement("html");
                    
                //    //System.Xml.XPath.XPathNodeIterator nav = dn
                //    //XmlReader stanzaReader = dn.CreateNavigator().ReadSubtree();
                //    string strHtml = dn.OuterHtml;
                //    strHtml = strHtml.CleanHtml();
                //    newHtm.Add(XElement.Parse(strHtml));
                   
                //    return newHtm;
                //}
                if (h.DocumentNode.Descendants().Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == className).Count() != 1)
                {
                    className = "mainregion2col";
                    if (h.DocumentNode.Descendants().Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == className).Count() == 1)
                    {
                        hn = h.DocumentNode.Descendants().Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == className).First();
                    }
                    else
                    {
                        MessageBox.Show("Kan ikke finne ett element!");
                        return null;
                    }
                }
                else
                {
                    hn = h.DocumentNode.Descendants().Where(p => (p.Attributes["class"] != null ? p.Attributes["class"].Value : "") == className).First();
                }
            }

            string s = hn.OuterHtml;
            s = s.CleanHtml();
            result = XElement.Parse(s);

            return result;
        }

        private void ImportSaveLink(HtmlNode n, Uri uri, string path)
        {
            if (n.Attributes["href"] != null)
            {
                HtmlAttribute att = n.Attributes["href"];
                if (!att.Value.Trim().StartsWith("#"))
                {
                    Uri r_uri = new Uri(uri, att.Value);
                    string filename = Path.GetFileName(r_uri.LocalPath);
                    if (filename != "")
                    {
                        att.Value = path + filename;
                        mDownloadFile(r_uri.AbsoluteUri, path, filename);
                    }
                }
            }
        }

        private void ImportSaveScript(HtmlNode n, Uri uri, string path)
        {
            if (n.Attributes["src"] != null)
            {
                HtmlAttribute att = n.Attributes["src"];
                if (!att.Value.Trim().StartsWith("#"))
                {
                    Uri r_uri = new Uri(uri, att.Value);
                    string filename = Path.GetFileName(r_uri.LocalPath);
                    if (filename != "")
                    {
                        att.Value = path + filename;
                        mDownloadFile(r_uri.AbsoluteUri, path, filename);
                    }
                }
            }
            else
            {
                n.InnerHtml = "";
            }
        }

        private void ImportSaveBody(HtmlNode n)
        {
            if (n.Attributes["onload"] != null)
            {
                HtmlAttribute att = n.Attributes["onload"];
                att.Value = "";
            }
        }

        private HtmlNode GetIndexElementById(HtmlAgilityPack.HtmlDocument d, string ElementName,  string ElementId )
        {
            
            HtmlNode hn = null;        
            if (d.DocumentNode
                        .Descendants()
                        .Where(p => 
                            p.NodeType == HtmlNodeType.Element
                            && (p.Attributes[ElementName] != null ? p.Attributes[ElementName].Value : "").Trim().ToLower() == ElementId
                            )
                        .Count() != 0)
            {
                    hn = d.DocumentNode
                        .Descendants()
                        .Where(p => 
                            p.NodeType == HtmlNodeType.Element
                            && (p.Attributes[ElementName] != null ? p.Attributes[ElementName].Value : "").Trim().ToLower() == ElementId
                            )
                        .First();
            }
            else
            {
                foreach (HtmlNode n in d.DocumentNode.Descendants())
                {
                    if ((n.Attributes[ElementName] != null ? n.Attributes[ElementName].Value : "") != "")
                    {
                        if ((n.Attributes[ElementName] != null ? n.Attributes[ElementName].Value : "") == ElementId)
                        {
                            hn = n;
                        }
                    }
                }
            }

            if (hn == null)
            {
                hn = d.DocumentNode
                        .Descendants()
                        .Where(p =>
                            p.NodeType == HtmlNodeType.Element
                            && (p.Attributes["class"] != null ? p.Attributes["class"].Value : "").Trim().ToLower() == "mainregion2col"
                            )
                        .FirstOrDefault();
            }

            if (hn == null) return null;
            return hn;
        }

        private string CleanTitle(string title)
        {
            title = Regex.Replace(title, @"(\r\n)+", " ");
            title = Regex.Replace(title, @"\t+", " ");
            title = Regex.Replace(title, @"\s+", " ");
            return title;
        }

        private void ExtractRegjeringenParts(HtmlNode ul, Uri uri, XElement part, ref int partNo)
        {
            XElement subPart = null;
            foreach (HtmlNode n in ul.Descendants("li").Where(p => p.Ancestors("ul").Count() == 1))
            {
                if ((n.Attributes["class"] == null ? "" : n.Attributes["class"].Value) == "" && n.Descendants("h3").Count() != 0)
                {
                    subPart = new XElement("container"
                        , new XElement("name", n.Descendants("h3").First().InnerText.Trim())
                        );
                    part.Add(subPart);
                }
                else if (n.Descendants("a").Count() != 0)
                {
                    string href = "";
                    if (n.Descendants("a").First().Attributes["href"] != null)
                    {
                        HtmlAttribute att = n.Descendants("a").First().Attributes["href"];
                        if (!att.Value.Trim().StartsWith("#"))
                        {
                            Uri r_uri = new Uri(uri, att.Value);
                            href = r_uri.AbsoluteUri;
                        }

                    }

                    string partText = CleanTitle(n.InnerText.Trim());

                    if (partText.EndsWith(".."))
                    {
                        if (n.Attributes["title"] != null)
                        {
                            partText = CleanTitle(n.Attributes["title"].Value.Trim());
                        }
                    }
                    if ((partText != "Vis detaljert innholdsfortegnelse") && (partText != "Vis komplett innhaldsliste"))
                    {
                        XElement l = new XElement("part"
                            , new XAttribute("no", partNo.ToString())
                            , new XElement("name", partText)
                            , new XElement("href", href));
                        partNo++;
                        if (subPart != null)
                        {
                            subPart.Add(l);
                        }
                        else
                        {
                            part.Add(l);
                        }
                    }
                }
            }

        }
        private XElement GetRegjeringenParts(HtmlAgilityPack.HtmlDocument h, Uri uri)
        {
            HtmlNode hn = null;
            hn = GetIndexElementById(h, "id", "innholdsfortegnelse");
            if (hn == null) return null;
            XElement parts = new XElement("parts");
            HtmlNode ul = null;
            if (hn.Descendants("ul").Count() == 0) return null;
            ul = hn.Elements("ul").First();
            int partNo = 0;
            ExtractRegjeringenParts(ul, uri, parts, ref partNo);
            
            hn = GetIndexElementById(h, "id", "vedleggogregistre");
            if (hn != null)
            {
                if (hn.Descendants("ul").Count() == 0) return parts;
                ul = hn.Elements("ul").First();
                ExtractRegjeringenParts(ul, uri, parts, ref partNo);
            }
            return parts;
        }

        private XElement GetStortingetParts(HtmlNode hn, Uri uri)
        {
            XElement part = new XElement("parts");
            
            HtmlNode ul = null;
            if (hn.Elements("ul").Count() == 1)
            {
                ul = hn.Elements("ul").First();
            }
            else if (hn.Elements("ul").Count() > 1)
            {
                ul = hn.Elements("ul").ElementAt(1);
            }
            int partNo = 0;
            foreach (HtmlNode n in ul.Descendants("a").Where(p => p.Ancestors("ul").Count() == 1))
            {
                string partText = "";
                string href = "";
                if (n.Attributes["href"] != null)
                {
                    HtmlAttribute att = n.Attributes["href"];
                    if (!att.Value.Trim().StartsWith("#"))
                    {
                        Uri r_uri = new Uri(uri, att.Value);
                        href = r_uri.AbsoluteUri;
                    }
                    else
                    {
                        Uri r_uri = new Uri(uri, att.Value);
                        href = r_uri.AbsoluteUri;
                    }
                }

                partText = CleanTitle(n.InnerText.Trim());

                if (partText.EndsWith(".."))
                {
                    if (n.Attributes["title"] != null)
                    {
                        partText = CleanTitle(n.Attributes["title"].Value.Trim());
                    }
                }

                XElement l = new XElement("part"
                    , new XAttribute("no", partNo.ToString())
                    , new XElement("name", partText)
                    , new XElement("href", href));

                partNo++;
                part.Add(l);
            }
            return part;
        }


        private void btnImportIndex_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (adress.Text == "" || tbTargetDir.Text == "") return;
                string url = adress.Text;
                Uri uri = new Uri(url);
                string path = tbTargetDir.Text;
                string html = DownloadWebPage(adress.Text);
                HtmlAgilityPack.HtmlDocument h = new HtmlAgilityPack.HtmlDocument();
                h.OptionWriteEmptyNodes = true;
                h.OptionOutputAsXml = true;
                h.LoadHtml(html);
                
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string subPath = path + @"_files\";
                if (!Directory.Exists(subPath)) Directory.CreateDirectory(subPath);

                foreach (HtmlNode n in h.DocumentNode.Descendants().Where(p => p.NodeType == HtmlNodeType.Element))
                {
                    switch (n.Name)
                    {
                        //case "link": ImportSaveLink(n, uri, subPath); break;
                        //case "script": ImportSaveScript(n, uri, subPath); break;
                        case "body": ImportSaveBody(n); break;
                    }
                }

                HtmlNode hn = null;
                XElement parts = null;
                if (uri.OriginalString.IndexOf("www.regjeringen.no") != -1)
                {
                    parts =  GetRegjeringenParts(h, uri);
                }
                else if (uri.OriginalString.IndexOf("www.stortinget.no") != -1)
                {
                    hn = GetIndexElementById(h, "id", tbSelectName.Text);
                    if (hn == null) return;
                    parts = GetStortingetParts(hn, uri);
                }

                LoadPartsList(parts);


                _PROGRESS.Maximum = parts.Elements().Count() + 1;
                _PROGRESS.Value = 1;

                XElement x = _myXDoc.GetXDoc();
                if (x.Elements(parts.Name).Count() == 0)
                {
                    x.Add(parts);
                }
                else
                {
                    x.Elements(parts.Name).Remove();
                    x.Add(parts);
                }
                if (hn != null)
                {
                    string shtml = hn.OuterHtml;

                    XDocument d = XDocument.Parse(shtml);
                    XElement El = d.Root;
                    string fileName = Path.GetFileName(uri.LocalPath);
                    if (fileName == "") fileName = "index.xml";
                    El.Save(path + fileName);
                }
            }

            catch (SystemException err)
            {
                Debug.Print(err.Message);
            }

        }

        private XElement BuildPartFromToc(XElement content, XElement toc)
        {
            for (int i = 0; i < toc.DescendantsAndSelf("section").Count(); i++)
            {
                XElement curr = toc.DescendantsAndSelf("section").ElementAt(i);
                XElement next = null;
                if ((i + 1) < toc.DescendantsAndSelf("section").Count())
                {
                    next = toc.DescendantsAndSelf("section").ElementAt(i + 1);
                }

                XElement part = null;
                if (next != null)
                {
                    part = new XElement("part");
                    part.Add(content.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) >= Convert.ToInt32(curr.Attribute("from")==null ? curr.Attribute("idx").Value : curr.Attribute("from").Value)
                            && Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("from") == null ? next.Attribute("idx").Value : next.Attribute("from").Value)));


                }
                else
                {
                    part = new XElement("part");
                    part.Add(content.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) >= Convert.ToInt32(curr.Attribute("from") == null ? curr.Attribute("idx").Value : curr.Attribute("from").Value)));
                }

                if (part.HasElements)
                {
                    if (curr.Elements("section").Count() != 0)
                    {
                        curr.Elements("section").First().AddBeforeSelf(part.Elements());
                    }
                    else
                        curr.Add(part.Elements());
                }

            }
            if (toc.Elements("section").Count()!=0)
                toc.AddFirst(content.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(toc.Elements("section").First().Attribute("from") == null ? toc.Elements("section").First().Attribute("idx").Value : toc.Elements("section").First().Attribute("from").Value)));
            return toc;

        }
        private void btnSectionDocument_Click(object sender, EventArgs e)
        {
            if (tbLawElement.Text == "") { MessageBox.Show("Angi lov-element!"); return; }

            if (lbParts.SelectedItem == null) { MessageBox.Show("Velg fil!"); return; }

            XElement El = _myXDoc.GetXDoc();

            ComboboxItem item = lbParts.SelectedItem as ComboboxItem;
            XElement currentPart = El.Descendants("part").Where(p => p.Attribute("no").Value == item.Value.ToString()).First();
            int partIndex = currentPart.NodesBeforeSelf().Count();
            if (currentPart.Element("xml") == null) { MessageBox.Show("Importer dokument!"); return; }
            if (currentPart.Element("xml") != null)
            {
                if (!File.Exists(tbTargetDir.Text + currentPart.Element("xml").Value)) { MessageBox.Show("XML-fil finnes ikke, importer dokument!"); return; }
                FileInfo fi = new FileInfo(tbTargetDir.Text + currentPart.Element("xml").Value);
                if (myWebBrowser == null) CreateWebBrowser();
                adress.Text = tbTargetDir.Text + currentPart.Element("xml").Value;
                XElement xd = XElement.Load(adress.Text);

               

                int n = xd
                        .Descendants(tbLawElement.Text)
                        .Where(p=>p.Descendants("section").Count()==0)
                        .Count();
                if (n == 0) { MessageBox.Show("Elementet '" + tbLawElement.Text + "' finnes ikke i filen!"); return; }
                for (int i = 0; i < n; i++)
                {
                    XElement section = xd
                        .Descendants(tbLawElement.Text)
                        .Where(p=>p.Descendants("section").Count()==0)
                        .ElementAt(i);
                    XAttribute analyse = new XAttribute("action", "lovsearch");
                    section.Add(analyse);
                }

                n = xd
                    .Descendants(tbLawElement.Text)
                    .Where(p => (p.Attribute("action") == null ? "" : p.Attribute("action").Value) == "lovsearch")
                    .Count();
                for (int i = 0; i < n; i++)
                {
                    XElement section = xd
                        .Descendants(tbLawElement.Text)
                        .Where(p => (p.Attribute("action") == null ? "" : p.Attribute("action").Value) == "lovsearch")
                        .ElementAt(i);

                    XElement part = new XElement("part");
                    
                    foreach (XElement el in section.Elements())
                    {
                        if (el.Name.LocalName != "title")
                        {
                            part.Add(new XElement(el));
                        }
                    }
                    
                    XElement toc = null;
                    List<XElement> l = part
                                    .Descendants()
                                    .Where(p => p.Name.LocalName=="mtit"
                                        && p.Value.Trim() != "")

                                    .ToList();
                    
                    if (l.Count() > 0)
                    {
                        section.Elements().Where(p => p.Name.LocalName != "title").Remove();
                        int minPara = 100;
                        int maxPara = 0;
                        BuildHeaderHierachy bh = new BuildHeaderHierachy(minPara, maxPara, false);
                        bh._v0_GetHeaderType(l);

                        XElement container = new XElement("container");
                        toc = bh.GetIndexBranch(l, true, container);
                        foreach (XElement s in toc.Descendants("section"))
                        {
                            if ((s.Attribute("id") == null ? "" : s.Attribute("id").Value) != "")
                                s.Attribute("id").Value = Guid.NewGuid().GetHashCode().ToString();
                            else
                            {
                                if (s.Attribute("id") == null)
                                {
                                    XAttribute id = new XAttribute("id", Guid.NewGuid().GetHashCode().ToString());
                                    s.Add();
                                }
                                else
                                    s.Attribute("id").Value = Guid.NewGuid().GetHashCode().ToString();
                                }
                        }
                        XElement newContent =  BuildPartFromToc(part, toc);
                        int ns = newContent.Descendants("section").Count();
                        section.Add(newContent.Nodes());
                    }
                }
                int h = xd.Descendants("section").Where(p => p.Elements().Count() == 0 ? false : p.Elements().First().Name.LocalName == "mtit").Count();
                for (; h > 0; h--)
                {
                    XElement h2 = xd.Descendants("section").Where(p => p.Elements().Count() == 0 ? false : p.Elements().First().Name.LocalName == "mtit").ElementAt(h - 1).Elements().First();
                    h2.ReplaceWith(new XElement("title", h2.Attributes(), h2.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate()));
                }

                string path = global._dbPath + El.Element("foldername").Value + @"\final\";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                IdentyfiAllSections(xd);

                xd.Save(path + fi.Name);
                if (currentPart.Element("final") == null)
                    currentPart.Add(new XElement("final", @"final\" + fi.Name));
                else
                    currentPart.Element("final").Value = @"final\" + fi.Name;

                _myXDoc.SetXDoc(El);
            }
        }

        private void IdentyfiAllSections(XElement xd)
        {
            int n = xd.DescendantsAndSelf("section").Count();
            for (int i = n; i > 0 ;i--)
            {
                XElement s = xd.DescendantsAndSelf("section").ElementAt(i - 1);
                XElement text = new XElement("text", s.Nodes().Where(p => "/section/title/".IndexOf("/" + (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") + "/") == -1));
                if (text.Elements("text").Count() == 1 && text.Elements().Count()==1)
                {
                    text = text.Elements("text").First();
                }
                s.Nodes().Where(p => "/section/title/".IndexOf("/" + (p.NodeType == XmlNodeType.Element ? ((XElement)p).Name.LocalName : "") + "/") == -1).Remove();
                if (s.Element("title")== null)
                    s.AddFirst(new XElement("title", text.Value.Trim().Substring(0,20)+"..."));
                s.Element("title").AddAfterSelf(text);
            }



            foreach (XElement s in xd.DescendantsAndSelf("section"))
            {

                string lastId = "";
                string newId = "";
                string testID = s.Attribute("id") == null ? "" : s.Attribute("id").Value;
                
                if (testID != "")
                {
                    string result = _IDS.Find(
                            delegate(string p)
                            {
                                return p == testID;
                            });
                    if (result == null)
                    {
                        _IDS.Add(testID);
                    }
                    else
                    {
                        s.Attribute("id").Value = "";
                        testID = "";
                    }


                    if (!(testID.StartsWith("DEL") || testID.StartsWith("KAP") || testID.IndexOf("_note") != -1))
                    {
                        if (s.Parent.Attribute("id") != null)
                        {
                            XNode p = s.PreviousNode;
                            while (p != null)
                            {
                                if (p.NodeType == XmlNodeType.Element)
                                {
                                    XElement test = (XElement)p;
                                    string testID1 = test.Attribute("id") == null ? "" : test.Attribute("id").Value;
                                    if (test.Name.LocalName == "section" && (testID !="" || testID1 !=""))
                                    {
                                        if (!(testID.StartsWith("DEL") || testID1.StartsWith("KAP") || testID1.IndexOf("_note") != -1))
                                        {
                                            lastId = test.Attribute("id").Value;
                                            int lid = lastId.Split('.').Count();
                                            for (int i = 0; i < (lid - 1); i++)
                                            {
                                                if (i == 0)
                                                    newId = lastId.Split('.').ElementAt(i);
                                                else
                                                    newId = newId + "." + lastId.Split('.').ElementAt(i);
                                            }
                                            newId = newId + "." + (Convert.ToInt32(lastId.Split('.').ElementAt(lid - 1)) + 1).ToString();
                                            break;
                                        }
                                    }
                                    else if (test.Name.LocalName == "title")
                                    {
                                        if ((test.Parent.Attribute("id") == null ? false : (test.Parent.Attribute("id").Value.StartsWith("KAP") || test.Parent.Attribute("id").Value.StartsWith("DEL"))))
                                        {
                                            lastId = test.Parent.Attribute("id").Value;
                                            newId = lastId + ".1";
                                            break;
                                        }
                                    }
                                }
                                p = p.PreviousNode;

                            }
                            if (newId != "")
                            {
                                if (s.Attribute("id") == null)
                                {
                                    XAttribute aid = new XAttribute("id", newId);
                                    s.Add(aid);
                                }
                                else
                                    s.Attribute("id").Value = newId;
                            }
                        }
                    }
                }
            }
        }

        private void rbShowWeb_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShowWeb.Checked)
                lbParts_SelectedIndexChanged(sender, e);
        }

        private void rbShowHtml_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShowHtml.Checked)
                lbParts_SelectedIndexChanged(sender, e);

        }

        private void rbShowXML_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShowXML.Checked)
                lbParts_SelectedIndexChanged(sender, e);
        }

       


        private void rbShowFinal_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShowFinal.Checked)
                lbParts_SelectedIndexChanged(sender, e);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            Uri u = myWebBrowser.Document.Url;
            string path = u.AbsoluteUri;
            string host = u.Host;
            string name = "";
            string title = "";
            bool identify = true;
            XElement dInfo = null;
            switch (host)
            {
                case "www.stortinget.no": 
                    host = "stortinget";
                    rbArea2.Checked = true;
                    rbArea2_CheckedChanged(sender, e);
                    dInfo = ReadDocument();
                    if (dInfo == null) return;


                    if (dInfo != null && (dInfo.Attribute("class") == null ? "" : dInfo.Attribute("class").Value) == "mainregion2col")
                    {
                        title = (dInfo.Descendants("b").Count() == 0 ? "" : dInfo.Descendants("b").First().Value.Trim());
                        name = (dInfo.Descendants("h1").Count() == 0 ? "" : dInfo.Descendants("h1").First().Value.Trim());
                    }
                    else
                    {
                        name = (dInfo.Descendants("b").Count() == 0 ? "" : dInfo.Descendants("b").First().Value.Trim());
                        title = (dInfo.Descendants("h2").Count() == 0 ? "" : dInfo.Descendants("h2").First().Value.Trim());
                    }
                    break;
                case "www.regjeringen.no":
                    host = "regjeringen";
                    rbArea1.Checked = true;
                    rbArea1_CheckedChanged(sender, e);
                    dInfo = ReadDocument();
                    if (dInfo == null) return;
                    if (dInfo != null)
                    {
                        name = (dInfo.Elements("h1").Count() == 0 ? "" : dInfo.Elements("h1").First().Value.Trim()) + (dInfo.Elements("h3").Count() == 0 ? "" : " " + dInfo.Elements("h3").First().Value.Trim());
                        title = (dInfo.Elements("h2").Count() == 0 ? "" : dInfo.Elements("h2").First().Value.Trim());
                    }
                    break;
            }

            string topic_id = "";
            
            if (title != "" && identify)
            {
                XElement d = new XElement("document");
                name.IdentifyMatchForarbeider(d);
                string id = (d.Attribute("name1") == null ? "none" : d.Attribute("name1").Value)
                            + (d.Attribute("number") == null ? "_none" : "_" + d.Attribute("number").Value)
                            + (d.Attribute("type") == null ? "_none" : "_" + d.Attribute("type").Value)
                            + (d.Attribute("year1") == null ? "_none" : "_" + d.Attribute("year1").Value)
                            + (d.Attribute("year2") == null ? "_none" : "_" + d.Attribute("year2").Value);
                switch (global.m_DB_TOPICS.Where(t => (t.key).ToLower() == id.ToLower()).Count())
                {
                    case 0:
                        break;
                    case 1 : topic_id = global.m_DB_TOPICS.Where(t => (t.key).ToLower() == id.ToLower()).ElementAt(0).topic_id;
                        break;
                    default:
                        {
                            MessageBox.Show("Det finnes flere Topic til dette dokumentet");
                            frmGetDocuments f = new frmGetDocuments(this, id.ToLower());
                            f.ShowDialog();
                            if (f.DialogResult == DialogResult.OK)
                            {
                                topic_id = (string)f.Tag;
                            }
                        }
                        break;
                }
            }
            _project = new frmProject(path, host, name, title, topic_id);
            _project.FormClosing += new FormClosingEventHandler(_project_FormClosing); 
            _project.Show(this);
            
        }

        private void SetAreaProperties()
        {
            if (rbArea1.Checked)
            {
                global._dbArea = rbArea1.Tag.ToString();
                tbSelectName.Text = "innholdsfortegnelse";
                rbId.Checked = true;
                tbDocumentConent.Text = "subPage";
            }
            else if (rbArea2.Checked)
            {
                global._dbArea = rbArea2.Tag.ToString();
                tbSelectName.Text = "ctl00_MainRegion_TOC1_TocList";
                rbId.Checked = true;
                tbDocumentConent.Text = "mainregion2col|mainbody";
            }
        }

        private void rbArea2_CheckedChanged(object sender, EventArgs e)
        {
            SetAreaProperties();   
        }

        private void rbArea1_CheckedChanged(object sender, EventArgs e)
        {
            SetAreaProperties(); 
        }


        private void rbShowTransform_CheckedChanged(object sender, EventArgs e)
        {
            if (rbShowTransform.Checked == false) return;
            XElement El = _myXDoc.GetXDoc();
            if (El == null) return;
            if (lbParts.SelectedItem != null)
            {
                string fileName = "";
                XElement l = El.Descendants("part").Where(p => p.Element("name").Value == lbParts.SelectedItem.ToString().Trim()).First();
                if ((l.Element("final") != null ? File.Exists(tbTargetDir.Text + l.Element("final").Value) : false)==true )
                {
                    fileName = tbTargetDir.Text + l.Element("final").Value;
                }
                else if ((l.Element("xml") != null ? File.Exists(tbTargetDir.Text + l.Element("xml").Value) : false) == true)
                {
                    fileName = tbTargetDir.Text + l.Element("xml").Value;
                }
                else
                    return;


                //XDocument xd = XDocument.Load(xmlFile);
                XmlTextReader xmlreader = new XmlTextReader(fileName);
                XsltArgumentList xslArg = new XsltArgumentList();

                //string path = Path.GetDirectoryName(Application.ExecutablePath);
                string path = global.m_Projects + @"\TransformData";
                string xsltPath = path + @"\xsl\forarbeider.xslt";

                xslArg.AddParam("cssPath","", path + @"\css\");
                
                if (!File.Exists(xsltPath))
                {
                    MessageBox.Show("Finner ikke filen '" + xsltPath + "'");
                    return;
                }


                string outString = transformDocument.TransformXmlWithXslToString(xmlreader
                    , xsltPath, xslArg);
                xmlreader.Close();
                if (!Directory.Exists(tbTargetDir.Text + @"output")) Directory.CreateDirectory(tbTargetDir.Text + @"output");
                string htmlFile = tbTargetDir.Text + @"output" + @"\document.html";

                File.WriteAllText(htmlFile, outString, Encoding.UTF8);


                if (myWebBrowser == null) CreateWebBrowser();
                adress.Text = htmlFile;
                myWebBrowser.Navigate(htmlFile);


            }

        }


        private void btnCreateTot_Click(object sender, EventArgs e)
        {
            
            _IDS = new List<string>();
            XElement el = _myXDoc.GetXDoc();
            if (el == null) return;
            if (el.Elements("parts").Count() == 0) return;

            XElement documents = new XElement("documents");
            XElement content = new XElement("content");
            XElement item = null;

            foreach (XElement part in el.Descendants().Where(p => p.Name.LocalName == "part"))
            {
                XElement document = null;
                if (part.Elements("final").Count() != 0)
                {
                    if (File.Exists(tbTargetDir.Text + part.Element("final").Value))
                    {
                        document = XElement.Load(tbTargetDir.Text + part.Element("final").Value);
                    }

                }
                if (document == null)
                {
                    if (part.Elements("xml").Count() != 0)
                    {
                        if (File.Exists(tbTargetDir.Text + part.Element("xml").Value))
                        {
                            document = XElement.Load(tbTargetDir.Text + part.Element("xml").Value);
                        }
                    }
                }
                if (document == null)
                {
                    MessageBox.Show("Dokumentet er ikke importert!");
                    return;
                }

                
            }

            int nContainer = 0;
            foreach (XElement part in el.Elements("parts").Elements().Where(p=>p.Name.LocalName == "part" ||p.Name.LocalName == "container"))
            {
                if (part.Name == "container")
                {
                    nContainer++;
                    XElement document = new XElement("section"
                            , new XAttribute("id", "DEL" + nContainer.ToString())
                            , new XElement("title", part.Element("name").Value)
                            , new XElement("text")
                        );
                    
                    GetDocumentParts(part, document, item);
                    documents.Add(document); 

                }
                else if (part.Name == "part")
                {
                    XElement document = null;
                    document = GetDocumentPart(part);
                    if (document == null)
                    {
                        MessageBox.Show("Dokumentet er ikke importert!");
                        return;
                    }
                    GetDocumentPartData(document);
                    documents.Add(document);
                }
            }

            IdentyfiAllSections(documents);
            XElement c = documents.GetContentMain(true); // GetContentDocument(document);
            content.Add(c.Nodes());

            
            XElement root = new XElement("root"
                ,  new XAttribute("type", "section")
                );

            root.Add(documents);
            if (content != null)
            {
                root.AddFirst(content);
            }
            
            if (!Directory.Exists(tbTargetDir.Text + "output")) Directory.CreateDirectory(tbTargetDir.Text + "output");
            documents.Save(tbTargetDir.Text + @"output\document.xml");


            _IDS = new List<string>();
        }

        private void GetDocumentPartData(XElement document)
        {
            int n = document.DescendantsAndSelf("text").Count();
            for (int i = n; i > 0; i--)
            {
                XElement text = document.DescendantsAndSelf("text").ElementAt(i - 1);
                if (text.Ancestors("text").Count() != 0)
                {
                    text.ReplaceWith(text.Nodes());
                }
            }
            
        }
        private XElement GetDocumentPart(XElement part)
        {
            XElement document = null;
            if (part.Elements("final").Count() != 0)
            {
                if (File.Exists(tbTargetDir.Text + part.Element("final").Value))
                {
                    document = XElement.Load(tbTargetDir.Text + part.Element("final").Value);
                }
            }
            if (document == null)
            {
                if (part.Elements("xml").Count() != 0)
                {
                    if (File.Exists(tbTargetDir.Text + part.Element("xml").Value))
                    {
                        document = XElement.Load(tbTargetDir.Text + part.Element("xml").Value);
                    }
                }
            }
            if (document != null)
            {
                return document.DescendantsAndSelf("section").First();
            }
            return null;
        }

        private void GetDocumentParts(XElement el, XElement documents, XElement content)
        {
            foreach (XElement part in el.Elements("part"))
            {
                XElement document = null;
                document = GetDocumentPart(part);
                if (document == null)
                {
                    MessageBox.Show("Dokumentet er ikke importert!");
                    return;
                }
                GetDocumentPartData(document);
                documents.Add(document);
            }
        }

        private void btnOpenTot_Click(object sender, EventArgs e)
        {
            if (!File.Exists(tbTargetDir.Text + @"output\document.xml"))
            {
                MessageBox.Show("Totalfil er ikke generert!");
                return;
            }

            XElement topic= null;
            if (_myXDoc.GetXDoc().Element("topic_id") != null)
            {
                topic = _myXDoc.GetXDoc();
            }
            string targetFolder = tbTargetDir.Text + @"output\";
            frmSectionEdit f = new frmSectionEdit(tbTargetDir.Text + @"output\document.xml", targetFolder ,topic );
            f.MdiParent = this.MdiParent;
            f.Show();
        }



        private void lbParts_SelectedIndexChanged(object sender, EventArgs e)
        {
            XElement El = _myXDoc.GetXDoc();
            if (El == null) return;
            if (lbParts.SelectedItem != null)
            {
                ComboboxItem item = lbParts.SelectedItem as ComboboxItem;
                XElement currentPart = El.Descendants("part").Where(p => p.Attribute("no").Value == item.Value.ToString()).First();
                if (currentPart == null) return;
                if (rbShowTransform.Checked)
                {
                    rbShowTransform_CheckedChanged(sender, e);
                }
                else if (rbShowFinal.Checked)
                {
                    if (currentPart.Element("final") != null)
                    {
                        if (myWebBrowser == null) CreateWebBrowser();
                        adress.Text = tbTargetDir.Text + currentPart.Element("final").Value;
                        myWebBrowser.Navigate(adress.Text);
                    }
                }
                else if (rbShowWeb.Checked)
                {
                    if (myWebBrowser == null) CreateWebBrowser();
                    string href = currentPart.Element("href").Value;
                    adress.Text = href;
                    myWebBrowser.Navigate(href);
                }
                else if (rbShowHtml.Checked)
                {
                    if (currentPart.Element("html") != null)
                    {
                        if (myWebBrowser == null) CreateWebBrowser();
                        adress.Text = tbTargetDir.Text + currentPart.Element("html").Value;
                        myWebBrowser.Navigate(adress.Text);
                    }

                }
                else if (rbShowXML.Checked)
                {
                    if (currentPart.Element("xml") != null)
                    {
                        if (myWebBrowser == null) CreateWebBrowser();
                        adress.Text = tbTargetDir.Text + currentPart.Element("xml").Value;
                        myWebBrowser.Navigate(adress.Text);
                    }

                }
            }

        }

        private void btnSaveHtml_Click(object sender, EventArgs e)
        {
            if (_htmlText.Text != "")
            {
                Form f = new frmSectionEdit(_htmlText.Text);
                f.Show();
            }
        }

        private void btnImportPage_Click(object sender, EventArgs e)
        {
            if (adress.Text == "" ) return;
            string url = adress.Text;
            Uri uri = new Uri(url);
            string path = @"c:\_test\app\";
            string html = DownloadWebPage(adress.Text);
            html = html.CleanHtml();
            HtmlAgilityPack.HtmlDocument h = new HtmlAgilityPack.HtmlDocument();
            //h.OptionWriteEmptyNodes = true;
            h.OptionAutoCloseOnEnd = true;
            h.OptionOutputOptimizeAttributeValues = true;
            h.OptionOutputAsXml = true;
            h.LoadHtml(html);
            
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string subPath = path + @"_files\";
            if (!Directory.Exists(subPath)) Directory.CreateDirectory(subPath);

            foreach (HtmlNode n in h.DocumentNode.Descendants().Where(p => p.NodeType == HtmlNodeType.Element))
            {
                switch (n.Name)
                {
                    case "link": ImportSaveLink(n, uri, subPath); break;
                    case "script": ImportSaveScript(n, uri, subPath); break;
                    case "body": ImportSaveBody(n); break;
                }
            }

            h.Save(path + "document.htm");
            XElement x = XElement.Load(path + "document.htm");
            int i = 0;
            foreach (XElement subEl in x.DescendantsAndSelf())
            {
                i++;
                if (subEl.Attribute("idx") != null)
                {
                    subEl.Attribute("idx").Value = i.ToString();
                }
                else
                    subEl.Add(new XAttribute("idx", string.Format("{0}", i)));
            }            
            
            x.Save(path + "document1.htm");
           
            myWebBrowser.Navigate(path + "document1.htm");
        }

        private void btnSimple_Click(object sender, EventArgs e)
        {

        }
    }
}
