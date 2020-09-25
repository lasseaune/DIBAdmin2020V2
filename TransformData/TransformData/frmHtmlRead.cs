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
using System.IO;
using TransformData.Global;
using System.Text.RegularExpressions;
using System.IO.Packaging;
using Ionic.Zlib;
using Ionic.BZip2;
using Ionic.Zip;
using System.Diagnostics;
using System.Web;

namespace TransformData
{
    public partial class frmHtmlRead : Form
    {

        private XElement _DOCXML = null;
        private XElement _DOCXML_CONVERT = null;
        private List<cssValues> _cssValues = null;
        private List<string> _styles = null;
        private string _LegalLetters = "";
        private List<string> _LetterCombinationsFound = null;
        private List<char> _LetterExcluded = null;
        private int _tempImageCounter = 0;

        public frmHtmlRead()
        {
            InitializeComponent();
            _LegalLetters = global.GetLegalHtmlLetters();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private string EPUB_Unzip(string zipFilePath)
        {
            try
            {
                _tempImageCounter = 1;
                string unZipFolderLocation = string.Empty;
                using (FolderBrowserDialog unzipFolderLocation =
                       new FolderBrowserDialog())
                {
                    unzipFolderLocation.RootFolder =
                         Environment.SpecialFolder.DesktopDirectory;
                    unzipFolderLocation.Description = "Select Folder to Unzip " +
                                System.IO.Path.GetFileName(zipFilePath) + " zip file";
                    if (unzipFolderLocation.ShowDialog() != DialogResult.Cancel)
                    {
                        unZipFolderLocation = unzipFolderLocation.SelectedPath;
                        if (Directory.GetDirectories(unZipFolderLocation).Count() != 0 || Directory.GetFiles(unZipFolderLocation).Count() != 0)
                        {
                            if (MessageBox.Show("Vil du slette innholdet i katalogen '" + unZipFolderLocation + "' ", "Slette innhold i katalog?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                Directory.Delete(unZipFolderLocation, true);
                                Directory.CreateDirectory(unZipFolderLocation);
                                while (!Directory.Exists(unZipFolderLocation)) { }
                            }
                            else
                                return "";
                        }
                    }
                    else
                    {
                        //do nothing
                    }
                }
                if ((zipFilePath != string.Empty) && (unZipFolderLocation != string.Empty))
                {
                    XElement total = new XElement("html");
                    
                    List<string> toc = null;
                    bool header = true;
                    using (ZipFile zip = ZipFile.Read(zipFilePath))
                    {
                        toc =  ReadContent(zip);

                        foreach (ZipEntry z in zip)
                        {
                            if (header)
                            {
                                header = false;
                            }
                            else
                            {
                                MemoryStream ms = new MemoryStream();
                                Debug.Print(z.FileName.ToString());
                                z.Extract(ms);
                                ms.Position = 0;
                                createFile(unZipFolderLocation, z.FileName.ToString(), ms);
                                StreamReader reader = new StreamReader(ms);
                                string text = reader.ReadToEnd();
                            }
                        }
                    }
                    if (toc.Count() != 0)
                    {
                        _LetterCombinationsFound = new List<string>();
                        foreach(string fileName in toc)
                        {
                            string sfileName = Uri.UnescapeDataString(fileName);
                            //if (sfileName == "Kontantstrømoppstilling.xhtml") Debug.Print("Kontantstrømoppstilling.htm");
                            XElement doc = XElement.Load(Path.Combine(unZipFolderLocation, "OEBPS/" + sfileName));
                            FileInfo fi = new FileInfo(Path.Combine(unZipFolderLocation, "OEBPS/" + sfileName));
                            string htmlName = fi.FullName.Replace(fi.Extension, ".htm"); 
                            foreach (XElement e in doc.DescendantsAndSelf())
                            {
                                if (e.Name.Namespace != XNamespace.None)
                                {
                                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                                }
                                if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                                {
                                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                                }
                            }

                            _cssValues = new List<cssValues>();
                            _styles = new List<string>();
                            metods met = new metods();
                            foreach (XElement el in doc.Descendants("link"))
                            {
                                string cssFileName = Uri.UnescapeDataString((string)el.Attributes("href").FirstOrDefault());
                                if (cssFileName != null)
                                {


                                    if (File.Exists(Path.Combine( unZipFolderLocation, "OEBPS/" + cssFileName)))
                                    {
                                        TextReader tr = new StreamReader(Path.Combine(unZipFolderLocation, "OEBPS/" + cssFileName));
                                        // read a line of text
                                        string css = tr.ReadToEnd();
                                        tr.Close();
                                        styleValues sv = met.GetCssProperties(css);
                                        _styles.AddRange(sv.styles);
                                        _cssValues.AddRange(sv.cssValues);
                                    }
                                }
                            }

                            XElement html = null;
                            XElement body = doc.Descendants("body").FirstOrDefault();
                            if (body != null)
                            {
                                




                                html = new XElement("html", body.Nodes());
                                //html.Elements("div").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));


                                CleanImgTag(html, unZipFolderLocation);


                                while (html.Descendants("div").Count() != 0)
                                {
                                    html.Descendants("div").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                                }
                                html.Descendants("img").ToList().ForEach(p=>p.ReplaceWith(new XElement("p", new XElement(p))));

                                html.Elements("p").Where(p => p.GetElementText().Trim() == "" && p.Descendants("img").Count()==0).ToList().Remove();
                                
                                GetHeaderElements(html);


                                if (html.Elements("h1").Count() != 0)
                                {
                                    List<XNode> top = new List<XNode>();
                                    XNode next = html.Nodes().OfType<XElement>().First();
                                    while (next != null)
                                    {
                                        top.Add(next);
                                        next = next.NextNode;
                                        if ((next.NodeType == XmlNodeType.Element ? ((XElement)next).Name.LocalName : "") == "h1")
                                            break;
                                    }
                                    html.Elements("h1").First().AddAfterSelf(top.GetRange(0, top.Count()).Where(p => ((XElement)p).Name.LocalName != "img"));
                                    top.GetRange(0, top.Count()).Remove();
                                }
                                else
                                {
                                    html.AddFirst(new XElement("h1",fi.FullName.Replace(fi.Extension, "")));
                                }
                                
                                List<XElement> newFotnotes = GetFootnotesParagraph(html);

                                

                                html.Descendants().ToList().ForEach(p => GetSuperChar(p));
                                html.Descendants().ToList().ForEach(p => GetSuperSpan(p));

                                List<XElement> sup = html.Descendants("sup").ToList();

                                if (newFotnotes.Count() != 0 || sup.Count() != 0)
                                {
                                    Debug.Print("xxx");
                                }

                                //List<XAttribute> atts = html.Descendants("img").Where(p => p.Attribute("src") != null).Select(p => p.Attribute("src")).ToList();
                                //foreach (XAttribute a in atts)
                                //{
                                //    a.Value = Uri.UnescapeDataString(a.Value);
                                //}

                                //atts = html.Descendants("img").Where(p=>p.Attribute("alt")!=null).Select(p=>p.Attribute("alt")).ToList();
                                //foreach (XAttribute a in atts)
                                //{
                                //    a.Value = Uri.UnescapeDataString(a.Value);
                                //}

                                CleanInDesignElements(html);

                                total.Add(html.Nodes());

                                SaveAsEditableHtml(htmlName, html);

                            }
                        }
                        SaveAsEditableHtml(Path.Combine(unZipFolderLocation, "OEBPS/total.htm"), total);
                    }
                }
                return unZipFolderLocation;
            }
            catch (SystemException err)
            {
                throw new Exception("Error - EPUB_Unzip: " + err.Message);
            }
        }

        private void CleanImgTag(XElement doc, string docPath)
        {
            docPath = Path.Combine(docPath + @"\OEBPS\");
            string _newImgPath = Path.Combine(docPath  ,   @"tempImage\");
            if (!Directory.Exists(_newImgPath)) Directory.CreateDirectory(_newImgPath);
            List<string> filenames = doc.Descendants("img")
                    .Where(p => (p.Attribute("src") == null ? "" : p.Attribute("src").Value)!= "")
                    .Select(p=> Uri.UnescapeDataString(p.Attribute("src").Value))
                    .GroupBy(p=>p)
                    .Select(p=>p.Key)
                    .ToList();
            
            if (filenames.Count() == 0) return;
            
            Dictionary<string, string> tempName = new Dictionary<string,string>();
            foreach (string s in filenames)
            {
                if (File.Exists(Path.Combine(docPath, s)))
                {
                    string imgPath = Path.Combine(docPath, s);
                    FileInfo fi = new FileInfo(imgPath);

                    string newImageName = "img_" + _tempImageCounter.ToString() + fi.Extension;
                    File.Copy(imgPath, Path.Combine(_newImgPath, newImageName));
                    tempName.Add(s, newImageName);
                    _tempImageCounter++;
                }
                else
                {
                    MessageBox.Show("Image '" + s + "' mangler!");
                }
            }

            
            doc.Descendants("img")
                    .Where(p => (p.Attribute("src") == null ? "" : p.Attribute("src").Value)!= "")
                    .ToList()
                    .ForEach(p=>p.ReplaceWith(new XElement("img",
                        p.Attributes().Where(n=> ";src;alt;".IndexOf(";" +   n.Name.LocalName + ";")==-1), 
                        new XAttribute("src", tempName.Where(q=>q.Key==Uri.UnescapeDataString(p.Attribute("src").Value)).First().Value))));
            
        }
        private void GetHeaderElements(XElement html)
        {
            html.Elements("p")
                .Where(p => (
                    p.Attribute("class") == null ? "" 
                    : p.Attribute("class").Value).ToLower().StartsWith("overskrift-"))
                .ToList()
                .ForEach(p => p.ReplaceWith(new XElement("h" + p.Attribute("class").Value.Split('-').ElementAt(1), p.Nodes())));
        }

        private static string HEXREPL(Match m)
        {
            string xx = "&#{0};";
            int x = Convert.ToChar(m.ToString());
            if (x > 32767)
            {
                x = (x - 65536);
                x = x & 0xFF;
            }
            xx = string.Format(xx, x);
            return xx;
        }

        private void SaveAsEditableHtml(string filename, XElement doc)
        {
            XDocument xDocument = new XDocument();
            xDocument.AddFirst(new XDocumentType(
                "html",
                "-//W3C//DTD HTML 4.01 Transitional//EN",
                null,
                null
              ));

            xDocument.Add(doc);
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;
            using (XmlWriter xmlWriter = XmlWriter.Create(filename, writerSettings))
            {
                xDocument.Save(xmlWriter);
            }
        }


        /// Method to create file at the temp folder
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="contentFileURI"></param>
        /// <returns></returns>
        protected void createFile(string rootFolder,
            string contentFilePath,
            MemoryStream content)
        {
            // Initially create file under the folder specified

            contentFilePath = contentFilePath.Replace('/',
                             Path.DirectorySeparatorChar);

            if (contentFilePath.StartsWith(
                Path.DirectorySeparatorChar.ToString()))
            {
                contentFilePath = contentFilePath.TrimStart(
                                         Path.DirectorySeparatorChar);
            }
            else
            {
                //do nothing
            }

            contentFilePath = Path.Combine(rootFolder, contentFilePath);
            //contentFilePath =  System.IO.Path.Combine(rootFolder, contentFilePath); 

            //Check for the folder already exists. If not then create that folder

            if (Directory.Exists(
                Path.GetDirectoryName(contentFilePath)) != true)
            {
                Directory.CreateDirectory(
                          Path.GetDirectoryName(contentFilePath));
            }
            else
            {
                //do nothing
            }
            
            
            FileStream fs = File.OpenWrite(contentFilePath);

            /* could replace with GetBuffer() if you don't mind the padding, or you
            could set Capacity of ms to Position to crop the padding out of the
            buffer.*/
            fs.Write(content.GetBuffer(), 0, (int)content.Length);
            fs.Close();


        }
        private List<string> ReadContent(ZipFile zip)
        {
            ZipEntry z = zip["OEBPS/content.opf"];
            MemoryStream ms = new MemoryStream();
            Debug.Print(z.FileName);
            z.Extract(ms);
            ms.Position = 0;
            //StreamReader reader = new StreamReader(ms);
            XmlReader r = XmlReader.Create(ms);
            r.MoveToContent();
            XElement content = XElement.ReadFrom(r) as XElement;
            XNamespace ns = content.GetDefaultNamespace();

            return  content
                    .Descendants(ns + "spine")
                    .Descendants(ns + "itemref")
                    .Select(p =>
                        (string)content
                        .Descendants(ns + "item")
                        .Where(q => q.Attribute("id").Value.ToLower().Trim()
                            == p.Attribute("idref").Value.ToLower().Trim())
                        .Attributes("href")
                        .FirstOrDefault()
                        )
                    .ToList();
            
            
            //string text = reader.ReadToEnd();
            //<itemref idref="Innholdsfortegnelse
        }

        private void btnFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog oF = new OpenFileDialog();
            oF.RestoreDirectory = true;
            oF.Filter = "Html filer (*.htm;*.html)|*.htm;*.html|XHtml filer (*xhtml)|*.xhtml|Epub filer (*.epub)|*.epub";
            oF.DefaultExt = ".epub";
            oF.FilterIndex = 3;
            if (oF.ShowDialog() == DialogResult.OK)
            {
                
                string fileName = oF.FileName;
                textBox1.Text = fileName;
                FileInfo fi = new FileInfo(fileName);
                try
                {
                    if (fi.Extension == ".epub")
                    {
                        EPUB_Unzip(fileName);
                    }
                    else
                    {
                        FileStream file = new FileStream(oF.FileName, FileMode.OpenOrCreate, FileAccess.Read);
                        StreamReader sr = new StreamReader(file, Encoding.UTF8);
                        string XmlString = sr.ReadToEnd();
                        XmlString = XmlString.CleanHtml();
                        XElement d = XElement.Parse(XmlString);
                        //XElement d = XElement.Load(fileName);

                        foreach (XElement link in d.Descendants("link"))
                        {
                            XAttribute cssfile = ((link.Attribute("rel") == null ? "" : link.Attribute("rel").Value) == "stylesheet" ? link.Attribute("href") : null);
                            metods met = new metods();
                            _cssValues = new List<cssValues>();
                            _styles = new List<string>();
                            if (cssfile != null && File.Exists(fi.Directory + "/" + cssfile.Value))
                            {
                                TextReader tr = new StreamReader(fi.Directory + "/" + cssfile.Value);
                                // read a line of text
                                string css = tr.ReadToEnd();
                                tr.Close();
                                styleValues sv = met.GetCssProperties(css);
                                _styles.AddRange(sv.styles);
                                _cssValues.AddRange(sv.cssValues);
                            }
                        }

                        XElement body = d.Descendants("body").FirstOrDefault();
                        if (body != null)
                        {
                            body.Elements("div").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                            body.Elements("p").Where(p => p.GetElementText().Trim() == "").ToList().Remove();
                            body.Elements("p").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).ToLower().StartsWith("overskrift-")).ToList().ForEach(p => p.ReplaceWith(new XElement("h" + p.Attribute("class").Value.Split('-').ElementAt(1), p.Nodes())));

                            




                            _DOCXML = new XElement("html", body.Nodes());
                        }
                    }
                }
                catch (SystemException err)
                {
                    MessageBox.Show("Åpne fil error: " + err.Message);
                }
            }

        }


        private void CheckListsInParagraph(XElement doc)
        {
            List<string> lExpr = new List<string>();
            lExpr.Add(@"(?<alt0>(\d\.))");
            lExpr.Add(@"(?<alt1>(\d\)))");
            lExpr.Add(@"(?<alt2>([a-z]\.))");
            lExpr.Add(@"(?<alt3>([a-z]\)))");
            lExpr.Add(@"(?<alt4>([ivx]\.))");
            lExpr.Add(@"(?<alt5>([ivx]\)))");

            string regexExpr = "^(";
            bool start = true;
            foreach (string s in lExpr)
            {
                if (start)
                {
                    regexExpr = regexExpr + s;
                    start = false;
                }
                regexExpr = regexExpr + "|" + s;
            }
            regexExpr = ")";
            List<XElement> par = doc.Descendants("p").Where(p => Regex.IsMatch(p.GetElementText().TrimStart(), @"^(\d+|[a-z])(\.|\)")).ToList();
            foreach (XElement p in par)
            {
                
            }
        }


        private void RemoveElement(XElement e, string elementName, string className, string name)
        {
            List<XElement> spans = e.DescendantsAndSelf(elementName).Where(p=>(p.Attribute(className)==null ? "" :  p.Attribute(className).Value) == name).ToList();
            foreach (XElement span in spans)
                span.ReplaceWith(span.Nodes());
        }

        private void CreateImg(XElement e, string elementName)
        {
            List<XElement> imgs = e.DescendantsAndSelf(elementName).ToList();
            foreach (XElement img in imgs)
            {
                //<img class="frame-24" src="innhold-web-resources/image/45164.png" alt="45164.png" />
                string src = img.Attribute("src").Value.Replace(@"innhold-web-resources/image/","");
                img.Attribute("src").Value = @"dibimages\{xxxxx-xxxx--xxxx}\" + src;
            }
        }

        private void CreateFootnotes(XElement e, string elementName, string className, string name)
        {

            List<XElement> footnotes = e.DescendantsAndSelf(elementName).Where(p => (p.Attribute(className) == null ? "" : p.Attribute(className).Value) == name).ToList();
            foreach (XElement footnote in footnotes)
            {
                XElement footnoteParent = footnote.Ancestors("section").Last();
                XAttribute attHref = footnote.Attributes("href").FirstOrDefault();
                if (attHref != null)
                {
                    XElement footnoteRef = e.DescendantsAndSelf().Where(p => (p.Attribute("id") == null ? "" : p.Attribute("id").Value) == attHref.Value.Replace("#", "")).FirstOrDefault();
                    if (footnoteRef != null)
                    {
                        XElement container = footnoteRef.Parent;
                        string noteNo = footnoteRef.Value;
                        footnoteRef.Remove();
                        container.Attributes("class").Remove();

                        //new

                         XElement newLink = new XElement("sup"
                                , new XElement("a"
                                    , new XAttribute("href", "#")
                                    , new XAttribute("id", "fn_" + attHref.Value.Replace("#", ""))
                                    , new XAttribute("data-bm", attHref.Value.Replace("#", ""))
                                    , new XAttribute("class", "xref")
                                    , new XAttribute("onclick", "return false;")
                                    , footnote.Value)
                                );
                        XElement newFootnote = new XElement("section",
                                new XAttribute("id", attHref.Value.Replace("#", "")),
                                new XElement("title", "[" +footnote.Value + "]"),
                                new XElement("text", container)
                            );
                        
                        if (footnoteParent.Elements("section").Where(p=>(p.Element("title")==null ? "": p.Element("title").Value)== "Fotnoter").Count() ==0)
                            footnoteParent.Add(new XElement("section",
                                new XElement("title", "Fotnoter"),
                                new XElement("text")
                                    ));

                        XElement footnoteContainer = footnoteParent.Elements("section").Where(p => (p.Element("title") == null ? "" : p.Element("title").Value) == "Fotnoter").First();

                        footnote.ReplaceWith(newLink);
                        footnoteContainer.Add(newFootnote);
                        container.Parent.Remove();
                    }
                }
            }
        }

        private void RemoveEmptyElements(XElement doc)
        {
            doc
            .Descendants()
            .Where(p =>
                (p.Name.LocalName == "span"
                || p.Name.LocalName == "p"
                || p.Name.LocalName == "li")
                && p.Value.Trim() == ""
                && p.Descendants("img").Count()==0
                )
            .Remove();
        }

        private string BuildElementStylePositive(XElement e, string includeRegex)
        {
            List<string> classNames = e.Attribute("class").Value.Split(' ').Where(p => p.Trim() != "").Select(p => e.Name.LocalName + "." + p).ToList();
            List<cssValues> cssvalues = _cssValues.Where(p => classNames.Where(q => q == p.className).Count() != 0).ToList();
            string style = "";
            foreach (cssValues cv in cssvalues)
            {
                if (Regex.IsMatch(cv.property, includeRegex))
                {
                    string prop = cv.property + ":" + cv.value + ";";
                    style = style + prop;
                }

            }
            return style;
        }

        private string BuildElementStyle(XElement e, string omitRegex)
        {
            List<string> classNames = e.Attribute("class").Value.Split(' ').Where(p => p.Trim() != "").Select(p => e.Name.LocalName + "." + p).ToList();
            List<cssValues> cssvalues = _cssValues.Where(p => classNames.Where(q => q == p.className).Count() != 0).ToList();
            string style = "";
            foreach (cssValues cv in cssvalues)
            {
                if (!Regex.IsMatch(cv.property, omitRegex))
                {
                    string prop = cv.property + ":" + cv.value + ";";
                    style = style + prop;
                }

            }
            return style;
        }

        private XText ParseStringToXText(string s)
        {
            if (s == "")
            {
                return null;
            }
            else
            {
                return XElement.Parse("<root>" + s + "</root>", LoadOptions.PreserveWhitespace).Nodes().OfType<XText>().First();
            }
        }

        private void ReplaceChar(XElement doc, int DecNr, string replace)
        {
            List<XText> lt = doc.DescendantNodes().OfType<XText>().Where(p => Regex.IsMatch(p.ToString(), "[" + (char)DecNr + "]+")).ToList();
            foreach (XText t in lt)
            {
                XText newT = ParseStringToXText(Regex.Replace(t.ToString(), "[" + (char)DecNr + "]+", replace));
                t.ReplaceWith(newT);
            }
        }


        private void RemoveIllegalChar(XElement doc)
        {
            foreach (var c in doc.DescendantNodes().OfType<XText>().SelectMany(t => t.ToString().ToCharArray().AsQueryable().Where(q => q > (char)32767).Select(f => f)).GroupBy(p => p))
            {
                int x = (int)c.Key;
                ReplaceChar(doc, x, "");
                Debug.Print("Slettet char:" + c.Key.ToString());
            }

            ReplaceChar(doc, 8232, "");
            ReplaceChar(doc, 160, " ");

        }

        private void CleanInDesignElements(XElement doc)
        {

            RemoveIllegalChar(doc);

            
            foreach (XText t in doc.DescendantNodes().OfType<XText>())
            {
                

                //MatchCollection mc = Regex.Matches(t.ToString(), @"[^ \r\na-zæøåÆØÅ­­\+\=…\-–«»“”’&\?\%\**\\\/\,\.\:\;\""\'\(\)\{\}\[\]0-9_]+", RegexOptions.IgnoreCase);
                MatchCollection mc = Regex.Matches(t.ToString(), @"[^ \r\n" + _LegalLetters +  "]+", RegexOptions.IgnoreCase);
                if (mc.Count != 0)
                {
                    foreach (Match m in mc)
                    {
                        string test = _LetterCombinationsFound.Find(
                            delegate(string p)
                            {
                                return p == m.Value.ToString();
                            });
                        if (test == null)
                        {
                            if (!Regex.IsMatch(m.Value, "^[\t]+$"))
                            {
                                _LetterCombinationsFound.Add(m.Value.ToString());
                            }
                        }
                    }
                }
            }
            int si = 0;
            foreach (string s in _LetterCombinationsFound)
            {
                Debug.Print("Kombinasjon: " + si.ToString());
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s.ToArray().ElementAt(i);
                    int iChar = (int)c;
                    string hex = iChar.ConvertIntToHex();
                    Debug.Print("Boksatav: " + (i+1).ToString() + " -> " + c.ToString() + "//" + iChar.ToString() + "//" + hex);
                }
                si++;
            }

            RemoveEmptyElements(doc);

            foreach (string s in _LetterCombinationsFound)
            {
                doc
                .DescendantNodes()
                .OfType<XText>()
                .Where(p => Regex.IsMatch(p.ToString(), "^" + s + "$") && p.Parent.Name.LocalName == "span" && p.Ancestors("li").Count() != 0)
                .Select(p => p.Parent)
                .Remove();
            }

            RemoveEmptyElements(doc);

            foreach (string s in _LetterCombinationsFound)
            {
                doc
                .DescendantNodes()
                .OfType<XText>()
                .Where(p => Regex.IsMatch(p.ToString(), "^( )?" + s) && (p.Parent.Name.LocalName == "li" || p.Parent.Name.LocalName == "span"))
                .ToList()
                .ForEach(q=>q.ReplaceWith(ParseStringToXText(Regex.Replace(q.ToString(),"^( )?" + s,""))));
            }

            RemoveEmptyElements(doc);

            foreach (string s in _LetterCombinationsFound)
            {
                doc
                .DescendantNodes()
                .OfType<XText>()
                .Where(p => Regex.IsMatch(p.ToString(), "^" + s) && p.Parent.Name.LocalName == "p" && p.Parent.Nodes().Count() == 1)
                .ToList()
                .ForEach(q => q.Parent.ReplaceWith(new XElement("li", ParseStringToXText(Regex.Replace(q.ToString(), "^( )?" + s, "")))));
            }

            ReplaceChar(doc, 9, " ");
            ReplaceChar(doc, 32, " ");

            RemoveEmptyElements(doc);

            
            foreach (XElement e in doc.Descendants().Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) != ""))
            {
                string style = "";
                switch (e.Name.LocalName)
                {
                    case "p":
                        style = BuildElementStyle(e, @"^(\-epub|font\-|margin\-(t|b|r))");
                        style = style + BuildElementStylePositive(e, @"^(color|font\-style|font\-weight|text\-indent)");
                        break;
                    case "span":
                        style = BuildElementStyle(e, @"^(\-epub|font\-)");
                        style = style + BuildElementStylePositive(e, @"^(color|font\-style|font\-weight)");
                        break;
                    default:
                        style = BuildElementStyle(e, @"^(\-epub|font\-)");
                        break;
                }
                
                e.Attribute("class").Remove();
                if (style != "") e.Add(new XAttribute("style", style));
            }
            
        }
        private void ReplaceClassWithStyle(XElement doc)
        {

            CleanInDesignElements(doc);

            foreach (XElement e in doc.Descendants().Where(p=>(p.Attribute("class")==null ? "xref" : p.Attribute("class").Value )!= "xref"))
            {

                string name = e.Name.LocalName;
                string classNames =  "";
                foreach (string s in e.Attribute("class").Value.Split(' '))
                {
                    if (s != "")
                    {
                        if (classNames == "") classNames = classNames + "//";
                        if (s.IndexOf("-style-override-") != -1) classNames = classNames +  name + "." + s + "//";
                        else classNames = classNames + name + "." + s + "//";
                    }
                }

                List<cssValues> cssvalues = _cssValues.Where(p => classNames.IndexOf("//" + p.className + "//") != -1).ToList();
                string style = "";
                foreach (cssValues cv in cssvalues)
                {
                    string prop = cv.property + ":" + cv.value + ";";
                    style = style + prop;
 
                }
                e.Attribute("class").Remove();
                if (style != "")  e.Add(new XAttribute("style", style));
            }
        }

        private void GetSuperSpan(XElement d)
        {
            List<XElement> fotnoteSpan = d.Descendants("span").Where(p =>
                                (p.Attribute("class") == null ? 0 :
                                        _cssValues.Where(q => p.Attribute("class").Value.Split(' ').Where(o => o.Trim() != "").Select(n => p.Name.LocalName + "." + n).ToList().Where(m => m == q.className).Count() != 0
                                                && q.property == "vertical-align"
                                                && q.value == "super").Count()) != 0)
                                .ToList();
            foreach (XElement f in fotnoteSpan)
            {
                XElement test = new XElement("root");
                foreach (XNode n in f.DescendantNodes())
                {
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        test.Add(n);
                    }
                    else if (n.NodeType == XmlNodeType.Text)
                    {
                        if (n.ToString().Trim() == "")
                        {
                            test.Add(n);
                        }
                        else
                        {
                            string currStr = "";
                            foreach (char c in n.ToString().ToCharArray())
                            {
                                if (c.ToString().Trim() == "")
                                {
                                    if (currStr.Trim() == "")
                                        currStr = currStr + c.ToString();
                                    else
                                    {
                                        test.Add(new XText(currStr));
                                        currStr = c.ToString();
                                    }
                                }
                                else
                                {
                                    if (currStr.Trim() != "")
                                    {
                                        currStr = currStr + c.ToString();
                                    }
                                    else
                                    {
                                        if (currStr != "")
                                            test.Add(new XElement("sup",currStr));
                                        currStr = c.ToString();
                                    }
                                }

                            }
                            if (currStr != "")
                            {
                                if (currStr.Trim() == "")
                                    test.Add(new XText(currStr));
                                else
                                    test.Add(new XElement("sup", currStr));

                            }
                        }
                    }
                }
                f.ReplaceWith(test.Nodes());
            }
            
        }
        
        private void GetSuperChar(XElement d)
        {
            List<XElement> returnList = new List<XElement>();
            string regexSuperNumber = @"((?<n0>([⁰]))|(?<n1>([¹]))|(?<n2>([²]))|(?<n3>([³]))|(?<n4>([⁴]))|(?<n5>([⁵]))|(?<n6>([⁶]))|(?<n7>([⁷]))|(?<n8>([⁸]))|(?<n9>([⁹])))+";
            //"[⁰|¹|²|³|⁴|⁵|⁶|⁷|⁸|⁹]+"

            List<XText> fotnoteChar = d.DescendantNodes().OfType<XText>().Where(p =>
                    Regex.IsMatch(p.Value.Trim(), regexSuperNumber)
                ).ToList();

            foreach (XText f in fotnoteChar)
            {
                string s = f.ToString();
                s = Regex.Replace(s, regexSuperNumber
                            ,
                            delegate(Match m)
                            {
                                string returnVal = "";
                                for (int i = 0; i < 10; i++)
                                {
                                    if (m.Groups["n" + i.ToString()].Success)
                                    {
                                        returnVal = "<sup>" + i.ToString() + "</sup>";
                                    }
                                }
                                return returnVal;
                            }
                            , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                XElement test = XElement.Parse("<root>" + s + "</root>");
                f.ReplaceWith(test.Nodes());
            }
        }

        private List<XElement> GetFootnotesParagraph(XElement d)
        {
            List<XElement> returnList = new List<XElement>();
            List<XElement> fotnote = d.Descendants("p").Where(p =>
                Regex.IsMatch((p.Attribute("class") == null ? "" : p.Attribute("class").Value).ToLower(), "fo(o)?tnote")
            ).ToList();

            foreach (XElement f in fotnote)
            {
                XElement newNote = new XElement(f);
                GetSuperChar(newNote);
                GetSuperSpan(newNote);
                newNote.Descendants("span").ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
                XElement footnote = null;
                foreach (XNode n in newNote.Nodes())
                {
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        if (((XElement)n).Name.LocalName == "sup")
                        {
                            if ((footnote != null ? footnote.Value.Trim() : "") != "")
                            {
                                returnList.Add(footnote);
                            }
                            footnote = new XElement("footnote");
                            footnote.Add(n);
                        }
                    }
                    else
                    {
                        if (footnote != null)
                        {
                            footnote.Add(n);
                        }
                    }
                }

                if ((footnote != null ? footnote.Value.Trim(): "")!= "")
                {
                    returnList.Add(footnote);
                }
                f.Remove();
            }
            return returnList;
        }

        private void btnOK1_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                XElement work = new XElement(_DOCXML);


                List<XElement> hyper = work.Descendants("p").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).ToLower().IndexOf("hyperlink") != -1).ToList();
                List<XElement> hyperSpan = work.Descendants("span").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value).ToLower().IndexOf("hyperlink") != -1).ToList();
                List<XElement> newFotnotes = GetFootnotesParagraph(work);
                List<XElement> fotnoteSpan = work.Descendants("span").Where(p =>
                    Regex.IsMatch((p.Attribute("class") == null ? "" : p.Attribute("class").Value).ToLower(), "fo(o)?tnote")
                    ).ToList();

                string regexSuperNumber = @"((?<n0>([⁰]))|(?<n1>([¹]))|(?<n2>([²]))|(?<n3>([³]))|(?<n4>([⁴]))|(?<n5>([⁵]))|(?<n6>([⁶]))|(?<n7>([⁷]))|(?<n8>([⁸]))|(?<n9>([⁹])))+";
                //"[⁰|¹|²|³|⁴|⁵|⁶|⁷|⁸|⁹]+"

                List<XElement> fotnoteChar = work.Descendants("span").Where(p =>
                        Regex.IsMatch(p.Value.Trim(), regexSuperNumber)
                    ).ToList();


                if (hyper.Count() != 0
                    || hyperSpan.Count() != 0
                    || fotnoteSpan.Count() != 0
                    || fotnoteChar.Count() != 0
                    )
                {
                    

                }


                CleanInDesignElements(work);

                XElement returnEl = work.BuildLevelsFromHeader("new");

                _DOCXML_CONVERT = new XElement("document", returnEl.Nodes());


                //CreateImg(_DOCXML_CONVERT, "img");
                //CreateFootnotes(_DOCXML_CONVERT, "a", "class", "footnote-link");

                //List<XElement> fnts = _DOCXML_CONVERT.DescendantsAndSelf("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value) == "footnotes").ToList();
                //foreach (XElement fn in fnts) fn.ReplaceWith(new XText(" "));

                
            }

        }

        
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_DOCXML != null)
            {
                XElement work = new XElement(_DOCXML);

                List<XElement> brs = work.DescendantsAndSelf("br").ToList();
                foreach (XElement br in brs) br.ReplaceWith(new XText(" "));

                XElement returnEl = work.BuildLevelsFromHeader("new");
                
                _DOCXML_CONVERT = new XElement("document", returnEl.Nodes());
                
                CreateFootnotes(_DOCXML_CONVERT, "a", "class", "footnote-link");

                List<XElement> fnts = _DOCXML_CONVERT.DescendantsAndSelf("div").Where(p => (p.Attribute("class") == null ? "" : p.Attribute("class").Value ) == "footnotes").ToList();
                foreach (XElement fn in fnts) fn.ReplaceWith(new XText(" "));

                ReplaceClassWithStyle(_DOCXML_CONVERT);
            }
        }

        private void btnSaveXML_Click(object sender, EventArgs e)
        {
            if (_DOCXML_CONVERT != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Environment.CurrentDirectory;
                sfd.RestoreDirectory= true;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    _DOCXML_CONVERT.Save(sfd.FileName);
            }

        }

    }
}
