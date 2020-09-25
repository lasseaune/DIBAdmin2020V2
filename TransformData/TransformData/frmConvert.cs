using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using TransformData.Global;
using System.Text.RegularExpressions;


namespace TransformData
{
    public partial class frmConvert : Form
    {
        

        public string _PATH = "";
        public string _TOPICID = "{xxxxx-xxxx--xxxx}";
        private string _ConverterTag = "dibconverter";
        private CurrentValues _docPackage = new CurrentValues();
        
        public void InitializeForm()
        {
            InitializeComponent();
            _docPackage = new CurrentValues();
            _docPackage.Change += new CurrentValues.ChangingHandler(_docPackage_Change);
            btnOK.Enabled = false;

        }
        public frmConvert()
        {
            InitializeForm();
        }

        public frmConvert(Form parent)
        {
            InitializeForm();

        }

        public frmConvert(string path)
        {
            _PATH = path;

        }

        public frmConvert(string path, string topicId)
        {
            
            _PATH = path;
            _TOPICID = topicId;
            InitializeForm();
        }

        private class CurrentValues : System.EventArgs
        {
            public DibDocumentPackage _docPackage { get; set; }
            public delegate void ChangingHandler(object sender);
            public event ChangingHandler Change;
            public void Save()
            {
                if (this._docPackage.content != null) this._docPackage.content.Save(this._docPackage.OutputPath + this._docPackage.FileNameContent);
                if (this._docPackage.convertedCO != null) this._docPackage.convertedCO.Save(this._docPackage.OutputPath + this._docPackage.FileNameConvertByContent);
                Change(this._docPackage);
            }
            public void SetValues(DibDocumentPackage dp )
            {
                this._docPackage = dp;
                Change(this._docPackage);
            }
            public DibDocumentPackage GetValues()
            {
                return this._docPackage;
            }
        }

        public class DibDocumentPackage
        {
            public string status { get; set; }
            public string DocxFile { get; set; }
            public string CurrentFile { get; set; }
            public string ContainerFilePath { get; set; }
            public string Path { get; set; }
            public string OutputPath { get; set; }
            public string language { get; set; }
            public string topicId { get; set; }
            public string Title  { get; set; }
            public string format { get; set; }
            public string FileNameContent = "content.xml";
            public string FileNameContentHX = "contentHX.xml";
            public string FileNameContentMS = "contentMS.xml";
            public string FileNameDocument = "document.xml";
            public string FileNameConvertByContent = "convertbycontent.xml";
            public string FileNameConvertByHX = "convertbyhx.xml";
            public string FileNameConvertByMS = "convertbyhx.xml";
            public string FileNameFootnotes = "footnotes.xml";
            public XElement document { get; set; }
            public XElement footnotes { get; set; }
            public XElement content { get; set; }
            public XElement contentHX { get; set; }
            public XElement contentMS { get; set; }
            public XElement convertedCO { get; set; }
            public XElement convertedHX { get; set; }
            public XElement convertedMS { get; set; }
            public DibDocumentPackage()
            {
                this.status = "new";
            }
            public DibDocumentPackage(XElement e, string fileName)
            {
                this.status = "open";
                this.ContainerFilePath = fileName;
                this.DocxFile = (string)e.Elements("DocxFile").FirstOrDefault();
                this.CurrentFile = (string)e.Elements("CurrentFile").FirstOrDefault();
                this.Path = (string)e.Elements("Path").FirstOrDefault();
                this.OutputPath = (string)e.Elements("OutputPath").FirstOrDefault();
                this.language = (string)e.Elements("language").FirstOrDefault();
                this.topicId = (string)e.Elements("topicId").FirstOrDefault();
                this.Title = (string)e.Elements("Title").FirstOrDefault();
                this.format = (string)e.Elements("format").FirstOrDefault();
                this.content = GetXElementFromFile(this.OutputPath + this.FileNameContent);
                this.contentHX = GetXElementFromFile(this.OutputPath + this.FileNameContentHX);
                this.contentHX = GetXElementFromFile(this.OutputPath + this.FileNameContentMS);
                this.document = GetXElementFromFile(this.OutputPath + this.FileNameDocument);
                this.convertedCO = GetXElementFromFile(this.OutputPath + this.FileNameConvertByContent);
                this.convertedHX = GetXElementFromFile(this.OutputPath + this.FileNameConvertByHX);
                this.convertedMS = GetXElementFromFile(this.OutputPath + this.FileNameConvertByMS);
                this.footnotes = GetXElementFromFile(this.OutputPath + this.FileNameFootnotes);
             
            }
            
        }
        
        private  void _docPackage_Change(object sender)
        {
            if (sender == null) return;
            btnConvertByContent.Enabled = false;
            btnViewConverted.Enabled = false;
            DibDocumentPackage dp = sender as DibDocumentPackage;
            if (dp.document != null) btnConvertByContent.Enabled = true;
            if (dp.convertedCO != null) btnViewConverted.Enabled = true;

        }

        private static XElement GetXElementFromFile(string filename)
        {
            XElement returnvalue = null;
            if (filename == null) return null;
            try
            {
                if (File.Exists(filename))
                    returnvalue = XElement.Load(filename);
            }
            catch
            {
            }
            return returnvalue;
        }

        private void GetDocxDocument(ref DibDocumentPackage documentpackage)
        {
            try
            {
                byte[] byteArray = File.ReadAllBytes(documentpackage.OutputPath + documentpackage.CurrentFile);
                string lang = documentpackage.language;
                XElement footnotes = new XElement("footnotes");
                string imageDirectoryName = documentpackage.OutputPath + @"\" + _TOPICID;

                DirectoryInfo dirInfo = new DirectoryInfo(imageDirectoryName);
                if (dirInfo.Exists)
                #region // Delete directory and files
                {
                    
                    foreach (var f in dirInfo.GetFiles())
                        f.Delete();
                    dirInfo.Delete();
                }
                #endregion
                int imageCounter = 0;
                using (MemoryStream memoryStream = new MemoryStream())
                #region // Read docx file
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc =
                        WordprocessingDocument.Open(memoryStream, true))
                    
                    
                    #region // Red WordprocessingDocument
                    {
                        XDocument xx = doc.MainDocumentPart.GetXDocument();
                        xx.Save(documentpackage.OutputPath + @"\xmldocument.xml");
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                            PageTitle = "Test Title",
                            ConvertFormatting = false,
                            Format = documentpackage.format, 
                        };

                        HtmlConverterXMLSettings xmlSettings = new HtmlConverterXMLSettings(lang)
                        {
                            SectionName = "level",
                            Language = lang,
                        };

                        HtmlConverter._elementCounter = 0;
                        XElement html = HtmlConverter.ConvertToHtml(doc, settings,
                            imageInfo =>
                            #region //Convert document function
                            {
                                
                                DirectoryInfo localDirInfo = new DirectoryInfo(imageDirectoryName);
                                if (!localDirInfo.Exists)
                                    localDirInfo.Create();
                                ++imageCounter;
                                string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                                ImageFormat imageFormat = null;
                                if (extension.ToLower() == "png"
                                    || extension.ToLower() == "x-emf"
                                    || extension.ToLower() == "emf"
                                    || extension.ToLower() == "bmp"
                                    || extension.ToLower() == "tiff"
                                    || extension.ToLower() == "jpeg"
                                    )
                                {
                                    // Convert png to jpeg.
                                    extension = "jpg";
                                    imageFormat = ImageFormat.Jpeg;
                                }

                                // If the image format isn't one that we expect, ignore it,
                                // and don't return markup for the link.
                                if (imageFormat == null)
                                    return null;
                                string imagePath = @"dibimages\" + _TOPICID + @"\image" +
                                    imageCounter.ToString() + "." + "jpg";

                                string imageFileName = imageDirectoryName + @"\image" +
                                    imageCounter.ToString() + "." + "jpg";
                                try
                                {
                                    System.Drawing.Imaging.Encoder myEncoder;
                                    myEncoder = System.Drawing.Imaging.Encoder.Quality;
                                    EncoderParameter myEncoderParameter;
                                    EncoderParameters myEncoderParameters;
                                    ImageCodecInfo myImageCodecInfo;

                                    myEncoderParameters = new EncoderParameters(1);
                                    myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                                    myEncoderParameters.Param[0] = myEncoderParameter;

                                    Bitmap myBitmap = new Bitmap(imageInfo.Bitmap);
                                    myImageCodecInfo = GetEncoderInfo("image/jpeg");

                                    myBitmap.Save(imageFileName, myImageCodecInfo, myEncoderParameters);

                                }
                                catch (System.Runtime.InteropServices.ExternalException)
                                {
                                    return null;
                                }
                                XElement img = new XElement(Xhtml.img,
                                    new XAttribute(NoNamespace.src, imagePath),
                                    imageInfo.ImgStyleAttribute,
                                    imageInfo.AltText != null ?
                                        new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                                return img;
                            }
                            #endregion
                            , xmlSettings
                            , ref footnotes);

                        documentpackage.document = new XElement(html);
                        documentpackage.footnotes = new XElement(footnotes);
                    }
                    #endregion
                }
                #endregion
            }
            catch (SystemException err)
            {
                MessageBox.Show("En feil oppstod ved konvertering av dokumentet: " + err.Message);
            }
            
        }

        private XElement GetDibXmlHTML(byte[] byteArray, string outputPath)
        {
            string lang = "no";
            XElement footnotes = new XElement("footnotes");
            XElement returnValue = null;
            try
            {
                
                
                string imageDirectoryName = outputPath + @"\" + _TOPICID;
                
                DirectoryInfo dirInfo = new DirectoryInfo(imageDirectoryName);
                if (dirInfo.Exists)
                {
                    // Delete directory and files
                    foreach (var f in dirInfo.GetFiles())
                        f.Delete();
                    dirInfo.Delete();
                }
                
                int imageCounter = 0;
                //byte[] byteArray = File.ReadAllBytes(fileName);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);


                    using (WordprocessingDocument doc =
                        WordprocessingDocument.Open(memoryStream, true))
                    {

                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                            PageTitle = "Test Title",
                            ConvertFormatting = false,
                        };

                        HtmlConverterXMLSettings xmlSettings = new HtmlConverterXMLSettings(lang)
                        {
                            SectionName = "level"
                            
                        };

                        HtmlConverter._elementCounter = 0;
                        XElement html = HtmlConverter.ConvertToHtml(doc, settings,
                            imageInfo =>
                            {
                                DirectoryInfo localDirInfo = new DirectoryInfo(imageDirectoryName);
                                if (!localDirInfo.Exists)
                                    localDirInfo.Create();
                                ++imageCounter;
                                string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                                ImageFormat imageFormat = null;
                                if (extension.ToLower() == "png" || extension.ToLower() == "x-emf")
                                {
                                    // Convert png to jpeg.
                                    extension = "jpg";
                                    imageFormat = ImageFormat.Jpeg;
                                }
                                else if (extension.ToLower() == "bmp")
                                    imageFormat = ImageFormat.Bmp;
                                else if (extension.ToLower() == "jpeg")
                                    imageFormat = ImageFormat.Jpeg;
                                else if (extension.ToLower() == "tiff")
                                    imageFormat = ImageFormat.Tiff;

                                // If the image format isn't one that we expect, ignore it,
                                // and don't return markup for the link.
                                if (imageFormat == null)
                                    return null;
                                string imagePath = @"dibimages\" + _TOPICID + @"\image" +
                                    imageCounter.ToString() + "." + "jpg";
                                
                                string imageFileName = imageDirectoryName + @"\image" +
                                    imageCounter.ToString() + "." + "jpg";
                                try
                                {
                                    System.Drawing.Imaging.Encoder myEncoder;
                                    myEncoder = System.Drawing.Imaging.Encoder.Quality;
                                    EncoderParameter myEncoderParameter;
                                    EncoderParameters myEncoderParameters;
                                    ImageCodecInfo myImageCodecInfo;

                                    myEncoderParameters = new EncoderParameters(1);
                                    myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                                    myEncoderParameters.Param[0] = myEncoderParameter;

                                    Bitmap myBitmap = new Bitmap(imageInfo.Bitmap);
                                    myImageCodecInfo = GetEncoderInfo("image/jpeg");

                                    myBitmap.Save(imageFileName, myImageCodecInfo, myEncoderParameters);

                                }
                                catch (System.Runtime.InteropServices.ExternalException)
                                {
                                    return null;
                                }
                                XElement img = new XElement(Xhtml.img,
                                    new XAttribute(NoNamespace.src, imagePath),
                                    imageInfo.ImgStyleAttribute,
                                    imageInfo.AltText != null ?
                                        new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                                return img;
                            },
                            xmlSettings,
                            ref footnotes);

                        
                        if (html != null)
                        {
                            DirectoryInfo localDirInfo = new DirectoryInfo(outputPath);
                            if (!localDirInfo.Exists)
                                localDirInfo.Create();

                            if (html.Elements("table").Count() != 0)
                            {
                                if (html.Elements("table").First().DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim().ToLower().EndsWith("dibkunnskap")
                                    || html.Elements("table").First().DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim().ToLower().EndsWith("styresystemer"))
                                {
                                    html.Elements("table").First().Remove();
                                }
                            }

                            

                            HtmlHierarchy h = new HtmlHierarchy(xmlSettings.SectionName);
                            XElement document = h.CreateDibXMLDocument(html, footnotes, xmlSettings.FrontPageName, xmlSettings.FootnoteName );


                            XElement content = new XElement("root",  h.GetDIBXMLContent(document));
                            
                            XElement returnDoc = new XElement("root"
                                , new XElement("document"
                                    , new XAttribute("doctypeid", "7")));
                            returnDoc.Add(document.Nodes());
                            returnDoc.Descendants().Attributes("idx").Remove();


                            html.Save(outputPath + @"\document.html");
                            returnDoc.Save(outputPath + @"\document.xml");
                            content.Save(outputPath + @"\content.xml");
                            
                            returnValue = document;
                            _PATH = outputPath;
                        }
                    }
                }
            }
            catch(SystemException err)
            {
                MessageBox.Show("Error: " + err.Message);
            }
            return returnValue;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "") return;
            if (!File.Exists(textBox1.Text)) return;
 
            FileInfo fi = new FileInfo(textBox1.Text);
            
            string path = fi.Directory.FullName + @"\";
            path = path + @"output\";
            _PATH = path;
            if (Directory.Exists(path)) Directory.Delete(path, true);
            Directory.CreateDirectory(path);
            this.UseWaitCursor = true;
            XElement footnotes = new XElement("footnotes");
            byte[] byteArray = File.ReadAllBytes(textBox1.Text);
            XElement html = GetDibXmlHTML(byteArray, path);
            if (File.Exists(_PATH + "document.xml") && File.Exists(_PATH + "document.xml"))
            {
                btnOK.Enabled = true;
                MessageBox.Show("Dokumentet er ferdig generert i mappe '" + path + "'!");
            }
            this.UseWaitCursor = false;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Fil-navn (*.docx;*.xml) |*.docx;*.xml";
            openFileDialog1.FilterIndex = 0;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                this.Refresh();
                FileInfo fi = new FileInfo(openFileDialog1.FileName);
                if (fi.Extension.ToLower() == ".docx")
                {
                    //Vil du konverterer fila
                    DibDocumentPackage dp = new DibDocumentPackage();
                    dp.DocxFile = openFileDialog1.FileName;
                    _docPackage.SetValues(dp);
                    cbLanguage.SelectedItem = null;
                    cbLanguage.Text = "";

                    
                }
                else if (fi.Extension.ToLower() == ".xml")
                {
                    //Sjekk om det er en DibConverterContainer
                    XElement container = XElement.Load(openFileDialog1.FileName);
                    if (container.Name.LocalName == _ConverterTag)
                    {
                        DibDocumentPackage dp = new DibDocumentPackage(container, openFileDialog1.FileName);
                        _docPackage.SetValues(dp);
                        cbLanguage.Text = _docPackage.GetValues().language;
                    }
                }
                
                btnConvertToXml.Enabled = true;
            }

        }

        private void btnCheckDoc_Click(object sender, EventArgs e)
        {
            byte[] byteArray = File.ReadAllBytes(textBox1.Text);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);

                using (WordprocessingDocument doc =
                    WordprocessingDocument.Open(memoryStream, true))
                {
                    
                    XDocument d = doc.MainDocumentPart.GetXDocument();
                    
                    //d.Root.Element(W.body).Elements(W.tbl).ElementAt(1).ReplaceWith(d.Root.Element(W.body).Elements(W.tbl).ElementAt(1).Elements(W.tr).First().Elements(W.tc).Where(p=>p.Elements().Where(q=>q.Name!=W.tcPr)) )
                }
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.Close();

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void ConvertFirstTime()
        {
        }



        private void btnConvertToXml_Click(object sender, EventArgs e)
        {
            if (cbLanguage.SelectedItem == null)
            {
                MessageBox.Show("Velg språk!"); 
                return;
            }
            textBox1.Text = _docPackage.GetValues().CurrentFile == null ? _docPackage.GetValues().DocxFile : _docPackage.GetValues().OutputPath +  _docPackage.GetValues().CurrentFile;

            if ( File.Exists(textBox1.Text))
            {
                FileInfo fi = new FileInfo(textBox1.Text);
                if (fi.Extension.ToLower() != ".docx")
                {
                    MessageBox.Show("Valgt fil er ikke en 'Word docx fil'!");
                    return;
                }
                
                string catalogname =  fi.Name.Replace(fi.Extension,"");

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Fil-navn (*.xml) | *xml";
                sfd.FilterIndex=0;
                sfd.FileName = catalogname + " - Konvertert.xml";

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string uid = "";
                    string fileFolder = "";
                    string outputPath = Path.GetDirectoryName(sfd.FileName);
                    string outputContainer = "";
                    outputPath = outputPath + @"\";
                    XElement DibConverterContainer = null;
                    if (File.Exists(sfd.FileName))
                    {
                        DialogResult dr = MessageBox.Show("Filen er allerede konvertet. Vil du konverterer på nytt?", "Filen finnes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.Abort) return;
                        try
                        {
                            DibConverterContainer = XElement.Load(sfd.FileName);
                        }
                        catch(SyntaxErrorException err)
                        {
                            MessageBox.Show("'" + sfd.FileName + "' kunne ikke åpnes! \r\n Error:" + err.Message);
                            return;
                        }

                        if (DibConverterContainer.Name.LocalName != _ConverterTag)
                        {
                            MessageBox.Show("'" + sfd.FileName + "' er ikke en DIB-konverter fil!");
                            return;
                        }
                        uid = DibConverterContainer.Element("topicId").Value;
                        outputContainer = DibConverterContainer.Element("OutputPath").Value;
                    }
                    else
                    {
                        uid = Guid.NewGuid().ToString();
                        DibConverterContainer = new XElement(_ConverterTag);
                        fileFolder = Path.GetFileNameWithoutExtension(sfd.FileName).ToLower() + "-filer";
                        outputContainer = outputPath + fileFolder + @"\";
                        if (Directory.Exists(outputContainer))
                        {
                            Directory.Delete(outputContainer, true);
                        }
                        Directory.CreateDirectory(outputContainer);

                        File.Copy(fi.FullName, outputContainer + fi.Name);

                    }

                    DibDocumentPackage docPackage = new DibDocumentPackage();
                    docPackage.format = "xml";
                    docPackage.ContainerFilePath = sfd.FileName;
                    docPackage.OutputPath = outputContainer;
                    docPackage.CurrentFile = fi.Name;
                    docPackage.DocxFile = fi.FullName;
                    docPackage.topicId = "{" + uid + "}";
                    docPackage.Title = Path.GetFileNameWithoutExtension(sfd.FileName).ToLower();
                    docPackage.language = cbLanguage.SelectedItem.ToString();
                    
                    GetDocxDocument(ref docPackage);
                    if (docPackage.document == null)
                    {
                        MessageBox.Show("Et eller annet gikk feil!");
                        return;
                    }
                    
                    docPackage.document.Save(docPackage.OutputPath + docPackage.FileNameDocument);
                    docPackage.footnotes.Save(docPackage.OutputPath +  "footnotes.xml");
                    docPackage.UpdatePackageFolder(DibConverterContainer);
                    _docPackage.SetValues(docPackage);

                    btnConvertByContent.Enabled = true;
                }

            }
        }

        private XElement GetContentWithContentTable(List<DocumentBodyElements> contentElements, XElement frontText, ref XElement documentStart)
        {
            bool paragraf = false;
            XElement content = new XElement("document");
            string firstHeader = "";
            XElement lastHeader = null;
            string RegexpStandardNumber = @"^(?<n1>((?<b1>([A]))?(?<t1>(\d+))))((\s+)?(\-|–)(\s+)?(?<n2>((?<b2>([A]))?(?<t2>(\d+)))))?$";
            for (int n = 0; n < contentElements.Count(); n++)
            #region //HentContent
            {
                DocumentBodyElements test = contentElements.ElementAt(n);
                if (!paragraf)
                {
                    if (test.value == "paragraph" || test.value == "paragraphs")
                    {
                        paragraf = true;
                    }
                    continue;
                }
                else
                {
                    List<string> strings = test.strings; ;
                    if (strings.Count() == 0)
                    {
                        continue;
                    }
                    string startTest = GetStringsValue(strings, 0) != "" ? GetStringsValue(strings, 0).Trim().ToLower() : GetStringsValue(strings, 1).Trim().ToLower();

                    if (startTest.Trim() != "" && startTest == firstHeader.Trim().ToLower() && !Regex.IsMatch(GetStringsValue(strings, strings.Count() - 1), RegexpStandardNumber))
                    {
                        documentStart = test.element;
                        break;
                    }
                    else
                    {
                        if (firstHeader == "") firstHeader = GetStringsValue(strings, 0);

                        if (GetStringsValue(strings, 0) != "" && GetStringsValue(strings, 1) == "" && GetStringsValue(strings, 2) == "")
                        {
                            string c1 = GetStringsValue(strings, 0);
                            Match m = Regex.Match(c1, @"^(?<appendix>(Appendix(\s+\d+)?))(\s+)?\:");
                            if (m.Groups["appendix"].Success)
                            {
                                lastHeader = new XElement("level",
                                    new XElement("title", m.Groups["appendix"].Value));
                                content.Add(lastHeader);
                                continue;
                            }
                            else if (!c1.Trim().EndsWith("."))
                            {
                                lastHeader = new XElement("level",
                                    new XElement("title", GetStringsValue(strings, 0)));
                                content.Add(lastHeader);
                                continue;
                            }
                            else
                            {
                                frontText.Add(test.element);
                            }
                        }

                        if (GetStringsValue(strings, 0) != "" && (GetStringsValue(strings, 1) != "" || GetStringsValue(strings, 2) != ""))
                        {
                            string number = GetStringsValue(strings, 1) != "" ? GetStringsValue(strings, 1) : GetStringsValue(strings, 2);
                            Match m = Regex.Match(number, RegexpStandardNumber);
                            if (m != null)
                            {
                                XElement level = new XElement("level",
                                    new XElement("title", GetStringsValue(strings, 0)));

                                if (lastHeader != null)
                                    lastHeader.Add(level);
                                else
                                    content.Add(level);

                                if (!m.Groups["t2"].Success)
                                {
                                    level.Add(new XElement("level",
                                        new XElement("title", m.Groups["b1"].Value + m.Groups["t1"].Value + ".")));
                                    continue;
                                }
                                else
                                {
                                    for (int i = Convert.ToInt32(m.Groups["t1"].Value); i <= Convert.ToInt32(m.Groups["t2"].Value); i++)
                                    {
                                        level.Add(new XElement("level",
                                            new XElement("title", m.Groups["b1"].Value + i.ToString() + ".")));
                                    }
                                    continue;
                                }
                            }
                        }

                        else if (lastHeader != null && GetStringsValue(strings, 0) == "" && GetStringsValue(strings, 1) != "" && GetStringsValue(strings, 2) != "" && content.Descendants("level").Last() != null)
                        {
                            Match m = Regex.Match(GetStringsValue(strings, 2), RegexpStandardNumber);
                            if (m != null)
                            {
                                XElement level = lastHeader;

                                string title = level.Element("title").Value;
                                if (!Regex.IsMatch(title, @"^([A])?\d"))
                                {
                                    level.Element("title").Value = level.Element("title").Value + " " + GetStringsValue(strings, 1);
                                    if (!m.Groups["t2"].Success)
                                    {
                                        level.Add(new XElement("level",
                                            new XElement("title", m.Groups["b1"].Value + m.Groups["t1"].Value + ".")));
                                        continue;
                                    }
                                    else
                                    {
                                        for (int i = Convert.ToInt32(m.Groups["t1"].Value); i <= Convert.ToInt32(m.Groups["t2"].Value); i++)
                                        {
                                            level.Add(new XElement("level",
                                                new XElement("title", m.Groups["b1"].Value + i.ToString() + ".")));
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                        else if (lastHeader != null && GetStringsValue(strings, 0) == "" && GetStringsValue(strings, 1) != "" && GetStringsValue(strings, 2) == "")
                        {
                            XElement level = lastHeader;
                            if (!level.Element("title").Value.StartsWith("Appendix"))
                            {
                                level.Element("title").Value = level.Element("title").Value + " " + GetStringsValue(strings, 1);
                            }
                        }

                    }
                }

            }
            #endregion
            return content;
        }


        private void btnConvertByContent_Click(object sender, EventArgs e)
        {
            if (_docPackage == null) return;
            if (_docPackage.GetValues().document == null) return;

            XElement document = _docPackage.GetValues().document;
            document.SetTempId();
            XElement contents = document.Elements().Where(p => p.GetElementText().Trim().ToUpper().StartsWith("CONTENT")).FirstOrDefault();
            if (contents == null) return;
            XElement frontText = new XElement("text");
            foreach (XElement el in document.Elements())
            {
                if (el == contents) break;
                frontText.Add(el);
            }
            List<DocumentBodyElements> contentElements = GetDocumentBodyElements(document,contents, 1);
            XElement documentStart = null;
            XElement content = GetContentWithContentTable(contentElements, frontText, ref documentStart);
            if (!content.HasElements)
            {
                MessageBox.Show("Ingen innholdsfortegnelse funnet!");
                return;
            }
            if (documentStart == null)
            {
                MessageBox.Show("Startelementet ikke funnet funnet!");
                return;
            }

            _docPackage.GetValues().content = content;
            
            if (documentStart != null)
            {
                List<DocumentBodyElements> bodyElements =  GetDocumentBodyElements(document, documentStart, 0);
                if (bodyElements != null)
                {
                    XElement newDocument = new XElement(content);
                    IdentifiElementsEx(newDocument, bodyElements);
                    GetIdentifyedElements(newDocument, bodyElements);

                    //Kontroller om duplikate Id
                    newDocument.RemoveDuplicateTempID();
                    //Fjern referanser fra tittel
                    newDocument.RemoveReferances();

                    _docPackage.GetValues().convertedCO =  new XElement("root",
                                new XElement("document",
                                new XAttribute("doctypeid", "7")),
                                frontText==null? null : new XElement("level",
                                        new XElement("title", _docPackage.GetValues().language=="no" ?  "Forside" : "Frontpage"),
                                        frontText),
                                newDocument.Nodes(),
                                 _docPackage.GetValues().footnotes == null ? null : new XElement("level",
                                        new XElement("title", _docPackage.GetValues().language=="no" ?  "Fotnoter": "Footnotes"),
                                        new XElement("text"),
                                         _docPackage.GetValues().footnotes.Nodes())
                                );
                    _docPackage.Save();
                    
                }
                else
                {
                    MessageBox.Show("Ingen body-elementer!");
                }
            }
        }

        
        private void IdentifiElementsEx(XElement content, List<DocumentBodyElements> bodyElements)
        {
            List<XElement> index = content.Descendants("level").ToList();
            int lastId = -1;
            for (int i = 0; i < index.Count(); i++)
            {
                XElement indexItem = index.ElementAt(i);
                XElement indexItemNext = null;
                if (i+1<index.Count())
                {
                    indexItemNext = index.ElementAt(i + 1);
                }
                string title = indexItem.Element("title").GetElementText(" ").ToLower();
                string tnum = title.GetNumValue();
                string tvalues = Regex.Replace(title, @"[^a-zæøåA-ZÆØÅ0-1]", "");
                List<DocumentBodyElements> s = bodyElements
                                    .Where(p=>
                                        Convert.ToInt32(p.element.Attribute("id").Value.Replace("temp","")) >= lastId
                                        && (
                                            p.value.StartsWith(title)
                                        || (tvalues == "" ? false : tvalues == Regex.Replace(p.value.Trim(), @"[^a-zæøåA-ZÆØÅ0-1]", ""))
                                        || p.value.Trim() == title.Trim()
                                        || (tnum == "" ? false : 
                                                (tnum == p.value.GetNumValue()) ? true :
                                                     (p.element.Attribute("xml") != null ? false : 
                                                        (p.element.Descendants("td").Count() == 0 ? false :
                                                            p.element.Descendants("td").Elements().Where(q=>q.GetElementText(" ").Trim().GetNumValue()==tnum).Count() !=0)))
                                                
                                    ))
                                    .OrderBy(o => Convert.ToInt32(o.element.Attribute("id").Value.Replace("temp", "")))
                                    .ToList();
                if (s.Count() == 1)
                {
                    DocumentBodyElements f = s.First();
                    indexItem.Add(f.element.Attribute("id"));
                    lastId = Convert.ToInt32(f.element.Attribute("id").Value.Replace("temp", ""));
                }
                else if (s.Count() > 1)
                {
                    DocumentBodyElements mf = null;
                    if (indexItemNext != null)
                    {
                        title = indexItemNext.Element("title").GetElementText(" ").ToLower();
                        tnum = title.GetNumValue();
                        tvalues = Regex.Replace(title, @"[^a-zæøåA-ZÆØÅ0-1]", "");
                        foreach (DocumentBodyElements f in s)
                        {
                            List<DocumentBodyElements> sNext = bodyElements
                                        .SkipWhile(b => b != f)
                                        .Skip(1)
                                        .Where(p =>
                                            p.value.StartsWith(title)
                                            || (tvalues == "" ? false : tvalues == Regex.Replace(p.value.Trim(), @"[^a-zæøåA-ZÆØÅ0-1]", ""))
                                            || p.value.Trim() == title.Trim()
                                            || (tnum == "" ? false :
                                                    (tnum == p.value.GetNumValue()) ? true :
                                                         (p.element.Attribute("xml") != null ? false :
                                                            (p.element.Descendants("td").Count() == 0 ? false :
                                                                p.element.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tnum).Count() != 0)))

                                        )
                                        .OrderBy(o=>Convert.ToInt32(o.element.Attribute("id").Value.Replace("temp","")))
                                        .ToList();
                            if (sNext.Count() > 0)
                            {
                                mf = f;
                                break;
                            }
                        }
                        
                        if (mf==null) mf = s.First();
                        indexItem.Add(mf.element.Attribute("id"));
                        lastId = Convert.ToInt32(mf.element.Attribute("id").Value.Replace("temp", ""));
                    }
                }
            }
        }

        private void GetIdentifyedElements(XElement content, List<DocumentBodyElements> bodyElements)
        {
            List<XElement> index = content.Descendants("level").Where(p=>p.Attribute("id")!=null).ToList();
            XElement currentText = null;
            int currentLevel = 0;
            
            for (int i = 0; i < index.Count(); i++)
            {
                XElement current = index.ElementAt(i);
                XElement next = null;
                if ((i + 1) < index.Count())
                {
                    next = index.ElementAt(i+1);
                }
                DocumentBodyElements ided = bodyElements.Where(p => (p.element.Attribute("id") == null ? "" : p.element.Attribute("id").Value) == current.Attribute("id").Value).First();
                currentLevel = ided.level;
                currentText = new XElement("text");
                current.Element("title").AddAfterSelf(currentText);
                AddRestElementsToText(ided, current);
                List<DocumentBodyElements> bodyElementsBetween = new List<DocumentBodyElements>();
                if (next == null)
                {
                    bodyElementsBetween = bodyElements
                                        .SkipWhile(p => (p.element.Attribute("id") == null ? "" : p.element.Attribute("id").Value) != current.Attribute("id").Value)
                                        .Skip(1)
                                        .ToList();
                }
                else
                {
                    if (current.Attribute("id").Value != next.Attribute("id").Value)
                    {
                        bodyElementsBetween = bodyElements
                                            .SkipWhile(p => (p.element.Attribute("id") == null ? "" : p.element.Attribute("id").Value) != current.Attribute("id").Value)
                                            .Skip(1)
                                            .TakeWhile(p => (p.element.Attribute("id") == null ? "" : p.element.Attribute("id").Value) != next.Attribute("id").Value)
                                            .ToList();
                    }
                }
                foreach (DocumentBodyElements de in bodyElementsBetween)
                {
                    int elementLevel = 0;
                    int newLevel = 0;
                    int point =  Convert.ToInt32(540 / 20);
                    XElement bodyElement = de.element;
                    if (bodyElement.Name.LocalName == "table" 
                        && bodyElement.Attribute("xml_level") != null
                        && (bodyElement.Attribute("xml") != null ? bodyElement.Attribute("xml").Value : "")=="p"
                        )
                    {
                        elementLevel = Convert.ToInt32(bodyElement.Attribute("xml_level").Value);
                        newLevel = elementLevel - currentLevel;
                        newLevel = newLevel<0 ? 0 : newLevel;
                        XAttribute firstTdWidth = bodyElement.Descendants("td").Attributes("width").FirstOrDefault();
                        if (firstTdWidth != null)
                        {
                            firstTdWidth.Value = (point * newLevel).ToString()+ "pt" ;
                        }
                    }
                    else if (bodyElement.Name.LocalName == "p"
                                            && bodyElement.Attribute("xml_level") != null
                                            )
                    {
                        elementLevel = Convert.ToInt32(bodyElement.Attribute("xml_level").Value);
                        newLevel = elementLevel - currentLevel;
                        newLevel = newLevel<0 ? 0 : newLevel;

                        XAttribute style = bodyElement.Attributes("style").FirstOrDefault();
                        if (style != null)
                        {
                            style.Value = Regex.Replace(style.Value, @"margin\-left[^\;]+\;", "margin-left:" + (point * newLevel).ToString() + "pt;");
                        }
                    }
                    else if (bodyElement.Name.LocalName == "table" 
                        && bodyElement.Attribute("xml_level") != null
                        && bodyElement.Attribute("xml") != null
                        )
                    {
                    }
                    else if (Regex.IsMatch(bodyElement.Name.LocalName, @"h\d")
                                            )
                    {
                    }
                    currentText.Add(de.element); 
                }

            }
            
        }

        private void AddRestElementsToText(DocumentBodyElements de, XElement index)
        {
            XElement e = de.element;
            string firstWord = de.firstWord;
            string tNumTitle = index.Element("title").Value.GetNumValue();
            string tNumElement = de.value.GetNumValue();
            string tvaluesTitle = Regex.Replace(index.Element("title").Value, @"[^a-zæøåA-ZÆØÅ0-1]", "");
            string tvaluesElement = Regex.Replace(de.value, @"[^a-zæøåA-ZÆØÅ0-1]", "");
            if (e.Name.LocalName == "table" && e.Attribute("xml")==null && tNumTitle!="")
            {
                
                if (e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).Count() != 0)
                {
                    XElement table = new XElement(e.Name.LocalName,
                        e.Attributes(),
                        e.Elements("tr").First()
                        //e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).First().Ancestors("tr").First()
                        );
                    if (e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).First().Parent ==
                         e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).First().Ancestors("tr").First().Elements("td").First())
                    {
                        XElement td1 = e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).First().Ancestors("tr").First().Elements("td").ElementAt(0);
                        
                        List<DocumentBodyElements> td1El = td1
                                                        .Elements()
                                                        .Select(p => new DocumentBodyElements(p))
                                                        .ToList();
                        
                        List<DocumentBodyElements> el1 = td1El
                                                        .TakeWhile(q=>q.value.GetNumValue() != tNumTitle)
                                                        .ToList();
                        el1.AddRange(td1El
                                    .Where(q => q.value.GetNumValue() == tNumTitle)
                                    .ToList());

                        el1.AddRange(td1El
                                    .SkipWhile(q => q.value.GetNumValue() != tNumTitle)
                                    .Skip(1)
                                    .TakeWhile(q => q.value.GetNumValue() == "")
                                    .ToList());

                        XElement td2 = e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).First().Ancestors("tr").First().Elements("td").ElementAt(1);

                        List<DocumentBodyElements> td2El = td2
                                                        .Elements()
                                                        .Select(p => new DocumentBodyElements(p))
                                                        .ToList();

                        List<DocumentBodyElements> el2 = td2El
                                                        .TakeWhile(q => q.value.GetNumValue() != tNumTitle)
                                                        .ToList();
                        el2.AddRange(td2El
                                    .Where(q => q.value.GetNumValue() == tNumTitle)
                                    .ToList());

                        el2.AddRange(td2El
                                    .SkipWhile(q => q.value.GetNumValue() != tNumTitle)
                                    .Skip(1)
                                    .TakeWhile(q => q.value.GetNumValue() == "")
                                    .ToList());
                        
                        table.Add(new XElement("tr",
                            new XElement("td",
                                new XAttribute("valign", "top"),
                                new XAttribute("align", "left"),
                                el1.Select(p=>p.element)),
                            new XElement("td",
                                new XAttribute("valign", "top"),
                                new XAttribute("align", "left"),
                                el2.Select(p=>p.element))    
                                ));
                        el1.Select(p => p.element).Remove();
                        el2.Select(p => p.element).Remove();
                    }
                    else
                    {
                        XElement td2 = e.Descendants("td").Elements().Where(q => q.GetElementText(" ").Trim().GetNumValue() == tNumTitle).First().Ancestors("tr").First().Elements("td").ElementAt(1);
                        List<DocumentBodyElements> td2El = td2
                                                        .Elements()
                                                        .Select(p => new DocumentBodyElements(p))
                                                        .ToList();

                        List<DocumentBodyElements> el2 = td2El
                                                        .TakeWhile(q => q.value.GetNumValue() != tNumTitle)
                                                        .ToList();
                        el2.AddRange(td2El
                                    .Where(q => q.value.GetNumValue() == tNumTitle)
                                    .ToList());

                        el2.AddRange(td2El
                                    .SkipWhile(q => q.value.GetNumValue() != tNumTitle)
                                    .Skip(1)
                                    .TakeWhile(q => q.value.GetNumValue() == "")
                                    .ToList());
                        
                        table.Add(new XElement("tr",
                            new XElement("td",
                                new XAttribute("valign", "top"),
                                new XAttribute("align", "left")
                                ),
                            new XElement("td",
                                new XAttribute("valign", "top"),
                                new XAttribute("align", "left"),
                                el2.Select(p => p.element))
                                ));
                        el2.Select(p => p.element).Remove();
                    }
                    index.Element("text").Add(table);
                    return;
                }
            }

            if (e.Name.LocalName == "table" && (e.Attribute("xml") == null ? "" : e.Attribute("xml").Value) == "p")
            {
                XElement holder = new XElement("holder");
                for (int i = 0; i < e.Descendants("td").Count(); i++)
                {
                    if (i != 0)
                    {
                        holder.Add(e.Descendants("td").ElementAt(i).Nodes());
                    }
                    else
                    {
                        RemoveBR(e.Descendants("td").ElementAt(i));
                        index.Element("title").Nodes().Remove();
                        index.Element("title").Add(e.Descendants("td").ElementAt(i).Nodes());

                    }
                }
                if (holder.Nodes().Count() != 0) index.Element("text").Add(new XElement("p", holder.Nodes()));
                return;
            }

            if (e.Name.LocalName == "p" || Regex.IsMatch(e.Name.LocalName, @"^h\d+$"))
            {
                if (tvaluesTitle.ToLower() == tvaluesElement.ToLower())
                {
                    RemoveBR(e);
                    index.Element("title").Nodes().Remove();
                    index.Element("title").Add(e.Nodes());

                    return;
                }
                if (firstWord != "")
                {
                    XText t = e.DescendantNodes().OfType<XText>().FirstOrDefault();
                    if (t != null)
                    {
                        string test = t.ToString();
                        if (test.Trim().StartsWith(firstWord))
                        {
                            test = test.TrimStart();
                            test = test.Replace(firstWord, "");
                            e.DescendantNodes().OfType<XText>().First().ReplaceWith(new XText(test));
                        }
                        index.Element("text").Add(new XElement("p", e.Nodes()));
                        return;
                    }
                }
            }
            
        }

        
        
        //private void AddRestElementsToText(XElement e, string firstWord , XElement index)
        //{
        //    if (e.Name.LocalName == "table" && (e.Attribute("xml") == null ? "" : e.Attribute("xml").Value) == "p")
        //    {
        //        for (int i = 0; i < e.Descendants("td").Count(); i++)
        //        {
        //            if (i != 0)
        //            {
        //                index.Element("text").Add(e.Descendants("td").ElementAt(i).Nodes());
        //            }
        //            else
        //            {
        //                RemoveBR(e.Descendants("td").ElementAt(i));
        //                index.Element("title").Nodes().Remove();
        //                index.Element("title").Add(e.Descendants("td").ElementAt(i).Nodes());
        //            }
        //        }
        //    }
        //    else if (e.Name.LocalName =="p")
        //    {
        //        if (firstWord != "")
        //        {
        //            XText t = e.DescendantNodes().OfType<XText>().FirstOrDefault();
        //            string test = t.ToString();
        //            if (test.Trim().StartsWith(firstWord))
        //            {
        //                test = test.TrimStart();
        //                test = test.Replace(firstWord, "");
        //                t = new XText(test);
        //            }
        //            index.Element("text").Add(e);
        //        }
        //        else
        //        {
        //           RemoveBR(e); 
        //           index.Element("title").Nodes().Remove();
        //           index.Element("title").Add(e.Nodes());
        //        }


        //    }
        //    else if (Regex.IsMatch(e.Name.LocalName, @"^h\d+$"))
        //    {
        //        RemoveBR(e);
        //        index.Element("title").Nodes().Remove();
        //        index.Element("title").Add(e.Nodes());
        //    }
        //}

        private void RemoveBR(XElement e)
        {
            List<XElement> brs = e.DescendantNodes().OfType<XElement>().Where(p => p.Name.LocalName == "br").ToList();
            foreach (XElement br in brs)
                br.ReplaceWith(new XText(" "));
            XText t = e.DescendantNodes().OfType<XText>().FirstOrDefault();
            if (t != null)
            {
                t = new XText(t.ToString().TrimStart());
            }
        }
        
        private class DocumentBodyElements
        {
            public XElement element {get;set;}
            public List<string> strings {get;set;}
            public string docString { get; set; }
            public string docStringStrip { get; set; }
            public string firstWord {get;set;}
            public string value { get; set; }
            public int level = 0;
            public DocumentBodyElements(XElement e)
            {
                element = e;
                strings = GetXmlElementStrings(e);
                value = e.DescendantNodes().OfType<XText>().Where(p=>p.Ancestors("sup").Count()==0).Select(q=>q.ToString()).StringConcatenate(" ");
                value = CleanElementValue(value);
                firstWord = (string)e.GetElementText().Split(' ').FirstOrDefault();
                docString = GetStringsValue(strings, 0).Trim();
                docStringStrip = Regex.Replace(docString, @"\(Ref(\:)?.+$", "").Trim();
                level = e.Attribute("xml_level") == null ? 0 : Convert.ToInt32(e.Attribute("xml_level").Value) ;
            }
        }
        private  static string CleanElementValue(string s)
        {
            return  Regex.Replace(
                            Regex.Replace(s, @"\(Ref(\:)?.+$", "").Trim().ToLower(),
                        @"s\+", " ").Trim();
        }

        private List<DocumentBodyElements> GetDocumentBodyElements(XElement document, XElement start, int skip)
        {
                return document
                    .Elements()
                    .SkipWhile(p => p != start)
                    .Skip(skip)
                    .Select(p => new DocumentBodyElements(p))
                    .ToList();
        }

        private List<DocumentBodyElements> GetDocumentBodyElements(XNode documentStart)
        {
            List<DocumentBodyElements> returnValue = new List<DocumentBodyElements>();
            XNode next = documentStart;
            while (next != null)
            {
                if (next.NodeType == XmlNodeType.Element)
                {
                    XElement el = (XElement)next;
                    DocumentBodyElements curr = new DocumentBodyElements(el);
                    returnValue.Add(curr);
                }
                next = next.NextNode;
            }
            return returnValue;
        }
        
        private static string GetStringsValue(List<string> strings, int index)
        {
            return (strings.Count() > index ? strings[index] : "").Trim();
        }
        
        private static List<string> GetXmlElementStrings(XElement e)
        {
            List<string> strings = new List<string>();
            if (e.Name.LocalName == "table" && (e.Attribute("xml") == null ? "" : e.Attribute("xml").Value) == "p")
            {
                for (int i = 0; i < e.Descendants("td").Count(); i++)
                {
                    strings.Add(e.Descendants("td").ElementAt(i).DescendantNodes().OfType<XText>().Where(p=>p.Ancestors("sup").Count()==0).Select(q=>q.ToString()).StringConcatenate());
                }
            }
            else if (e.Name.LocalName != "table")
            {
                strings.Add(e.DescendantNodes().OfType<XText>().Where(p=>p.Ancestors("sup").Count()==0).Select(q=>q.ToString()).StringConcatenate());
            }
            return strings;
        }

        private void btnViewConverted_Click(object sender, EventArgs e)
        {
            if (_docPackage == null) return;
            if (_docPackage.GetValues().convertedCO == null) return;
            frmSectionEdit f = Application.OpenForms.OfType<frmSectionEdit>().First();
            
            if (f.Name != "")
            {
                f.LoadDataSet(_docPackage.GetValues().convertedCO);
            }
        }

        private void btnConvertHtag_Click(object sender, EventArgs e)
        {


            if (_docPackage == null) return;
            if (_docPackage.GetValues().document == null) return;
            string lang = _docPackage.GetValues().language;
            XElement footnotes = _docPackage.GetValues().footnotes;
            HtmlConverterXMLSettings xmlSettings = new HtmlConverterXMLSettings(lang)
            {
                SectionName = "level"

            };

            XElement html = _docPackage.GetValues().document;


            if (html != null)
            {

                if (html.Elements("table").Count() != 0)
                {
                    if (html.Elements("table").First().DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim().ToLower().EndsWith("dibkunnskap")
                        || html.Elements("table").First().DescendantNodes().OfType<XText>().Select(o => (string)o.ToString()).StringConcatenate().Trim().ToLower().EndsWith("styresystemer"))
                    {
                        html.Elements("table").First().Remove();
                    }
                }

                html.Descendants("h3").Where(p=>p.Ancestors().Where(s=>s.Name.LocalName=="td").Count()!=0).ToList().ForEach(p=>p.ReplaceWith(new XElement("p", p.Nodes())));

                HtmlHierarchy h = new HtmlHierarchy(xmlSettings.SectionName);
                XElement document = h.CreateDibXMLDocument(html, footnotes, xmlSettings.FrontPageName, xmlSettings.FootnoteName);


                XElement content = new XElement("root", h.GetDIBXMLContent(document));

                XElement returnDoc = new XElement("document"
                        , new XAttribute("doctypeid", "7")
                        , document.Nodes()
                );
                
                returnDoc.Descendants().Attributes("idx").Remove();

                //returnDoc.Save(outputPath + @"\document.xml");
                //content.Save(outputPath + @"\content.xml");

                _docPackage.GetValues().convertedHX = returnDoc;

                frmSectionEdit f = Application.OpenForms.OfType<frmSectionEdit>().First();

                if (f.Name != "")
                {
                    f.LoadDataSet(_docPackage.GetValues().convertedHX);
                }
            }

        }

        
    }

    public static class Extentions
    {
        public static bool UpdatePackageFolder(this frmConvert.DibDocumentPackage dp, XElement DibConverterContainer)
        {
            DibConverterContainer.Add(new XElement("DocxFile", dp.DocxFile == null ? "" : dp.DocxFile));
            DibConverterContainer.Add(new XElement("CurrentFile", dp.CurrentFile == null ? "" : dp.CurrentFile));
            DibConverterContainer.Add(new XElement("Path", dp.Path == null ? "" : dp.Path));
            DibConverterContainer.Add(new XElement("OutputPath", dp.OutputPath == null ? "" : dp.OutputPath));
            DibConverterContainer.Add(new XElement("language", dp.language == null ? "" : dp.language));
            DibConverterContainer.Add(new XElement("topicId", dp.topicId == null ? "" : dp.topicId));
            DibConverterContainer.Add(new XElement("Title", dp.Title == null ? "" : dp.Title));
            DibConverterContainer.Add(new XElement("format", dp.format == null ? "" : dp.format));
            DibConverterContainer.Save(dp.ContainerFilePath);
            return true;
        }

   
        public static void SetTempId(this XElement document)
        {
            int id = 1;
            foreach (XElement el in document.Descendants())
            {
                string strId = "temp" + id.ToString();
                if (el.Attribute("id") == null)
                {
                    el.Add(new XAttribute("id", strId));
                }
                else
                    el.Attribute("id").Value = strId;
                id++;
            }
        }


    }
}
