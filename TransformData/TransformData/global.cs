using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using DIB.RegExp.ExternalStaticLinks;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using TidyNet;



namespace TransformData.Global
{
    public class LawList
    {
        public List<lawObject> Laws {get;set;}
        
        public LawList (List<lawObject> Laws)
        {
            this.Laws = Laws;
        }
        public LawList(string fileName)
        {
            XElement lawfile = XElement.Load(fileName);
            this.Laws = lawfile
                    .Elements("law")
                    .Select(p=> new lawObject {
                          name = p.Element("name").Value,
                          shortName = p.Element("shortName").Value,
                          href = p.Element("href").Value,
                          year = p.Element("year").Value,
                          dep = p.Element("dep").Value,
                          yyyy = p.Element("yyyy").Value,
                          mm = p.Element("mm").Value,
                          dd = p.Element("dd").Value,
                          nr = p.Element("nr").Value,
                          importhref = p.Element("importhref") == null ? null : p.Element("importhref").Value
                    })
                    .ToList();
        }
        public void Save(string fileName)
        {
            XElement xLaws = new XElement("laws",
                            this.Laws.Select(p=>new XElement("law",
                                        new XElement("name",p.name),
                                        new XElement("shortName", p.shortName),
                                        new XElement("href", p.href),
                                        new XElement("year", p.year),
                                        new XElement("dep", p.dep),
                                        new XElement("yyyy", p.yyyy),
                                        new XElement("mm",p.mm),
                                        new XElement("dd",p.dd),
                                        new XElement("nr",p.nr),
                                        p.importhref == null ? null : new XElement("importhref", p.importhref)
                                        )
                                ));
            xLaws.Save(fileName);
        }
        public List<lawObject> GetList()
        {
            return this.Laws;
        }
        public List<lawObject> GetList(string search)
        {
            string regexpLovNo = @"(?<yyyy>(\d\d\d\d))\-(?<mm>(\d(\d)?))\-(?<dd>(\d(\d)?))\-(?<nr>(\d+))";
            Match m = Regex.Match(search, regexpLovNo);
            List<lawObject>  laws = null;
            if (m.Success)
            {
                laws = this.Laws.Where(p =>
                                    Convert.ToInt32(p.yyyy) == Convert.ToInt32(m.Groups["yyyy"].Value)
                                && Convert.ToInt32(p.mm) == Convert.ToInt32(m.Groups["mm"].Value)
                                && Convert.ToInt32(p.dd) == Convert.ToInt32(m.Groups["dd"].Value)
                                && Convert.ToInt32(p.nr) == Convert.ToInt32(m.Groups["nr"].Value)
                                ).ToList();
            }
            else
            {
                laws = this.Laws.Where(p =>
                                    p.name.ToLower().IndexOf(search.ToLower()) != -1
                                || p.shortName.ToLower().IndexOf(search.ToLower()) != -1
                                ).ToList();
            }
                

            
            return laws;
        }
    }

    public class lawObject
    {
        public string name { get; set; }
        public string shortName { get; set; }
        public string href { get; set; }
        public string importhref { get; set; }
        public string year { get; set; }
        public string dep { get; set; }
        public string yyyy { get; set; }
        public string mm { get; set; }
        public string dd { get; set; }
        public string nr { get; set; }
        public lawObject(){}
        public lawObject(XElement li, Uri uri)
        {
            string tempName = li.Element("a").GetElementText(" ").Trim();
            string regexpName = @"(?<name>([^\(\[]+))((\(|\[)(?<short>([^\)\]]+))(\)|\]))?";
            this.shortName = Regex.Match(tempName, regexpName).Groups["short"].Value.Trim();
            this.name = Regex.Match(tempName, regexpName).Groups["name"].Value.Trim();
            Uri tempUri = new Uri(uri, li.Element("a").Attribute("href").Value.ToString());
            this.href = tempUri.AbsoluteUri;

            string regexpYearDep = @"\((\s+)?(?<year>(\d+))(\s+)?(?<dep>(.+))(\s+)?\)";
            string tempYearDep = li.Nodes().OfType<XText>().First().ToString();
            this.year = Regex.Match(tempYearDep, regexpYearDep).Groups["year"].Value;
            this.dep = Regex.Match(tempYearDep, regexpYearDep).Groups["dep"].Value;

            string regexpNr = @"\/[^\-\/]+\-(?<yyyy>(\d\d\d\d))(?<mm>(\d\d))(?<dd>(\d\d))\-(?<nr>(\d+))\.html";
            this.yyyy = Regex.Match(this.href, regexpNr).Groups["yyyy"].Value;
            this.mm = Regex.Match(this.href, regexpNr).Groups["mm"].Value;
            this.dd = Regex.Match(this.href, regexpNr).Groups["dd"].Value;
            this.nr = Regex.Match(this.href, regexpNr).Groups["nr"].Value;
        }
        public override string ToString()
        {
            return this.name;
        }
        public string GetId()
        {
            return this.yyyy + "-" + this.mm + "-" + this.dd + "-" + this.nr;
        }

    }

    public class GlobalVar
    {
        public static LawList _LawList = null;
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }

    public class existing
    {
        public string key { get; set; }
        public string name { get; set; }
        public string area { get; set; }
        public string topic_id { get; set; }
    }

    public class topic
    {
        public string topic_id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string info { get; set; }
        public string year { get; set; }
        public string name1 { get; set; }
        public string number { get; set; }
        public string type { get; set; }
        public string year1 { get; set; }
        public string year2 { get; set; }
        public string doctype { get; set; }
    }


    public class ImportData
    {
        public string menuArea { get; set; }
    }

    public class cssValues
    {
        public string className { get; set; }
        public string property { get; set; }
        public string value { get; set; }
    }

    public class styleValues
    {
        public List<string> styles { get; set; }
        public List<cssValues> cssValues { get; set; }
    }

    public enum DibDocumentType
    {
        None = 0,
        Level7 = 1,
        IfrsBooks = 2,
        LigningsABC = 3,
        MVAHandbok = 4,
        Lover = 5,
        SectionWithLevel = 6,
        SectionDocument = 7,
        Navigation = 8,
        NavigationExt = 9,
        Level6 = 10,
        Level5 = 11
    }

    public class kodeElements
    {
        public XElement p { get; set; }
        public string id = "";
        public string text = "";
        public string number = "";
        public kodeElements(XElement e)
        {
            p = e;
            string regExp = @"^(?<number>(\d\d\d))(\-A)?(\s|$)";
            text = p.Nodes().OfType<XText>().First().ToString().TrimStart();
            Match m = Regex.Match(text, regExp);
            number = m.Groups["number"].Value;
        }

    }
    public class docElement
    {
        public XElement p { get; set; }
        public string text = "";
        public string t1 = "";
        public string t2 = "";
        public string t3 = "";
        public string t4 = "";
        public string t5 = "";
        public string number = "";
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
            string regExp = @"^(?<number>("
                + @"(?<n1>(\d+))"
                + @"("
                + @"((\.)(?<n2>(\d+)))"
                + @"((\.)(?<n3>(\d+)))?"
                + @"((\.)(?<n4>(\d+)))?"
                + @"((\.)(?<n5>(\d+)))?"
                + @")?))"
                + @"(\.)?"
                + @"(\s)";

            text = p.Nodes().OfType<XText>().First().ToString().TrimStart();
            Match m = Regex.Match(text, regExp);
            number = m.Groups["number"].Value;
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
    public class qResult
    {
        public int n { get; set; }
        public int length { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public List<qResultDiff> listqR { get; set; }
        public override string ToString()
        {
            return name.ToString();
        }
    }
    public class qResultDiff
    {
        public int diff { get; set; }
        public qResult qR { get; set; }
    }

    public class lovPunkt
    {
        public string type { get; set; }
        public string value { get; set; }
        List<lovPunkt> punkt = new List<lovPunkt>();
    }
    public class lovPunkts
    {
        public List<lovPunkt> punkts = new List<lovPunkt>();
    }
    public class rGroup
    {
        public string exp { get; set; }
        public List<string> groups { get; set; }
    }

    public class rReplace
    {
        public List<string> exp { get; set; }
    }
    public class metods
    {
        public DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private List<string> _STYLES;
        private List<cssValues> _cssValues;

        public styleValues GetCssProperties(string styleText)
        {
            _STYLES = new List<string>();
            _cssValues = new List<cssValues>();
            _GetCssProperties(styleText);

            return new styleValues
            {
                styles = _STYLES,
                cssValues = _cssValues
            };
        }
        public void GetCssProperties(string styleText, ref List<string> styles, ref List<cssValues> cssValues)
        {
            _STYLES = styles;
            _cssValues = cssValues;
            _GetCssProperties(styleText);
            cssValues = _cssValues;
            styles = _STYLES;

        }


        private void _GetCssProperties(string styleText)
        {

            styleText = Regex.Replace(styleText, @"/\*[^*]*.*?\*/", "", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"(?<!})\r\n", "", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"\t", " ", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"(?<=(}(\s+)?\r\n))(\s+)", "", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"\n", " ", RegexOptions.Singleline);
            styleText = styleText.Replace("  ", " ");
            styleText = styleText.Replace("  ", " ");
            styleText = styleText.Replace("  ", " ");
            styleText = styleText.Replace("  ", " ");

            List<string> styles = styleText.Split('}').ToList();
            foreach (string _s in styles)
            {
                string s = _s.Trim();
                if (s != "")
                {
                    _STYLES.Add(s + "}");
                    string tags = s.Split('{')[0];
                    string props = s.Split('{')[1];
                    List<string> values = props.Split(';').ToList();
                    foreach (string _prop in values)
                    {
                        string prop = _prop.Trim();
                        if (prop != "")
                        {
                            string name = prop.Split(':')[0];
                            string value = prop.Split(':')[1];

                            foreach (string _tag in tags.Split(','))
                            {
                                string tag = _tag.Trim();
                                if (tag != "")
                                {
                                    cssValues result = _cssValues.Find(
                                        delegate(cssValues p)
                                        {
                                            return p.className == tag.Trim()
                                                && p.property == name.Trim()
                                                && p.value == value.Trim();
                                        });
                                    if (result == null)
                                    {
                                        cssValues newCss = new cssValues();
                                        newCss.className = tag.Trim();
                                        newCss.property = name.Trim();
                                        newCss.value = value.Trim();
                                        _cssValues.Add(newCss);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void _GetCssPropertiesX(string styleText)
        {
            styleText = Regex.Replace(styleText, @"/\*[^*]*.*?\*/", "", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"(?<!})\r\n", "", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"\t", " ", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"(?<=(}(\s+)?\r\n))(\s+)", "", RegexOptions.Singleline);
            styleText = Regex.Replace(styleText, @"\n", " ", RegexOptions.Singleline);
            styleText = styleText.Replace("  ", " ");
            styleText = styleText.Replace("  ", " ");
            styleText = styleText.Replace("  ", " ");
            styleText = styleText.Replace("  ", " ");

            //string expression = @"(?<selector>(?:(?:[^,{]+),?)*?)\{(?:(?<name>[^}:]+):?(?<value>[^};]+);?)*?\}");
            //string expression = @"((^|\s+|(\s+)?\,)(\s+)?(?<selector>([A-Za-z\-\@\.0-9\#\:]+)))+(\s+)?\{((\s+)?(?<properties>([A-Za-z0-9-_]+))(\s+)?:((\s+)?(?<values>(\s[A-Za-z0-9\#\,\.\""\-%\(\)\\\[\]]+)))+(\s+)?;)+(\s+)?(\})?";
            string expression = @"((^|\s+|(\s+)?\,)(\s+)?(?<selector>([A-Za-z\-\@\.0-9\#\:]+)))+(\s+)?\{((\s+)?(?<properties>([A-Za-z0-9\-_]+))(\s+)?:((\s+)?(?<values>(\s[A-Za-z0-9\:\?\!\-\=\+_\/\#\,\.\'\""\-%\(\)\\\[\]]*)))*(\s+)?;)*(\s+)?(\})?";
            Regex ccsMatch = new Regex(expression);

            MatchCollection mc = null;
            int j = 0;
            try
            {
                mc = ccsMatch.Matches(styleText);
                j = mc.Count;
            }
            catch
            {
                return;
            }

            //Debug.Print(j.ToString());
            for (int i = 0; i < j; i++)
            {
                Match m = mc[i];
                string sResult = _STYLES.Find(
                            delegate(string p)
                            {
                                return p == m.Value.Trim();
                            });
                if (sResult == null)
                {
                    _STYLES.Add(m.Value.Trim());
                }

                foreach (Capture g in m.Groups["selector"].Captures)
                {
                    //Debug.Print(g.Value);
                    for (int n = 0; n < m.Groups["properties"].Captures.Count; n++)
                    {
                        cssValues result = _cssValues.Find(
                            delegate(cssValues p)
                            {
                                return p.className == g.Value.Trim()
                                    && p.property == m.Groups["properties"].Captures[n].Value.Trim()
                                    && p.value == m.Groups["values"].Captures[n].Value.Trim();
                            });
                        if (result == null)
                        {
                            cssValues newCss = new cssValues();
                            newCss.className = g.Value.Trim();
                            newCss.property = m.Groups["properties"].Captures[n].Value.Trim();
                            newCss.value = m.Groups["values"].Captures[n].Value.Trim();
                            _cssValues.Add(newCss);
                        }
                    }
                }
            }
        }
    }

    
    public static class global
    {
        public static string m_Projects = @"D:\Dokumenter\Visual Studio 2010\Projects\TransformData";
        public static string m_LawDocumentLibry = @"c:\DIB-Import\Lovdata\";
        public static string m_LawDocumentFile = m_LawDocumentLibry + @"import.xml";
        public static string m_LawsFile = m_LawDocumentLibry + @"laws.xml";
        public static string m_LawsFileRegexp = m_Projects + @"\TransformData\xml\LawHeadersRegex.xml";

        public static List<topic> m_TOPICS = new List<topic>();
        public static List<topic> m_DB_TOPICS = new List<topic>();
        public static string m_dbPath = @"C:\DIB-Import\documents\";
        public static string m_dbArea = @"";
        public static string m_forarbeidertoimport = @"C:\DIB-Import\forarbeidertoimport.xml";
        public static string m_InternalRegexpFile = m_Projects + @"\TransformData\xml\forarbeider_regexp.xml" ;
        public static string m_InternalDocumentRegexpFile = m_Projects + @"\TransformData\xml\InTextLinksRegexp.xml";
        public static string m_InternalActionsFile = m_Projects + @"\TransformData\xml\InTextLinksActions.xml";
        public static string m_InternalActionsInternalFile = m_Projects + @"\TransformData\xml\InTextLinksActionsInternal.xml";



        public static DibDocumentType GetDocumentType(this XElement d)
        {
            DibDocumentType returnType = DibDocumentType.None;
            #region //GetDocumentType
            if (d != null)
            {
                if (d.Descendants("documents").Count() > 0)
                #region //Document with documents tag
                {
                    #region //Lignings ABC og skatte ABC
                    if (d.Descendants("documents")
                        .Where(p =>
                               (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "handbook"
                            && (
                                (p.Attribute("variant") == null ? "" : p.Attribute("variant").Value) == "abc"
                                || (p.Attribute("variant") == null ? "" : p.Attribute("variant").Value) == "skatt"
                            )
                        )
                        .Count() > 0)
                    {
                        returnType = DibDocumentType.LigningsABC;
                    }
                    #endregion
                    #region //MVA handbok
                    else if (d.Descendants("documents")
                        .Where(p =>
                               (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "handbook"
                            && (p.Attribute("variant") == null ? "" : p.Attribute("variant").Value) == "mva"
                        )
                        .Count() > 0)
                    {
                        returnType = DibDocumentType.MVAHandbok;
                    }
                    #endregion
                    #region //IFRS Greenbook/Redbook
                    else if (d.Descendants("documents")
                        .Where(p =>
                            (p.Elements("document").First().Attribute("dibt") == null ? "" : p.Elements("document").First().Attribute("dibt").Value) != ""
                        )
                        .Count() > 0)
                    {
                        returnType = DibDocumentType.IfrsBooks;
                    }
                    #endregion
                    else if (d.Descendants("document").Where(p => p.Attributes("type").Count() > 0).Where(t => t.Attribute("type").Value.Equals("word")).Count() > 0)
                    #region //Singel document
                    {
                        string documentsVariant = d.Descendants("document").First().Attribute("variant") != null ? d.Descendants("document").First().Attribute("variant").Value : string.Empty;

                        if (documentsVariant == "lov")
                        {
                            returnType = DibDocumentType.Lover;
                        }
                        else
                        {
                            returnType = DibDocumentType.SectionWithLevel;
                        }
                    }
                    #endregion

                }
                #endregion
                else if (d.Descendants("document").Where(p => p.Attributes("type").Count() > 0).Where(t => t.Attribute("type").Value.Equals("word")).Count() > 0)
                #region //Singel document
                {
                    string documentsVariant = d.Descendants("document").First().Attribute("variant") != null ? d.Descendants("document").First().Attribute("variant").Value : string.Empty;

                    if (documentsVariant == "lov")
                    {
                        returnType = DibDocumentType.Lover;
                    }
                    else
                    {
                        returnType = DibDocumentType.SectionWithLevel;
                    }
                }
                #endregion
                else if (d.Descendants("section").Count() > 0)
                #region //dibxml document withouth documents
                {
                    returnType = DibDocumentType.SectionDocument;
                }
                #endregion
                else if (d.Descendants("document").Where(t => t.Attribute("doctypeid").Value.Equals("5")).Count() > 0)
                #region //process && multilevel documents, laget manuelt eller med doc2xml import Doctypeid 5
                {
                    returnType = DibDocumentType.Level5;
                }
                #endregion
                else if (d.Descendants("document").Where(t => t.Attribute("doctypeid").Value.Equals("7")).Count() > 0)
                #region //process && multilevel documents, laget manuelt eller med doc2xml import Doctypeid 7
                {
                    returnType = DibDocumentType.Level7;
                }
                #endregion
                else if (d.Descendants("navigation").Count() > 0)
                #region //Documents with navigation
                {
                    returnType = DibDocumentType.Navigation;
                }
                #endregion
                else if (d.Descendants("navigation-ext").Count() > 0)
                #region //Document with navigation-ext
                {
                    returnType = DibDocumentType.NavigationExt;
                }
                #endregion
                else if (d.Descendants("document").Where(t => t.Attribute("doctypeid").Value.Equals("6")).Count() > 0)
                #region //Temacontent
                {
                    returnType = DibDocumentType.Level6;
                }
                #endregion
            }
            #endregion
            return returnType;
        }

        public static Dictionary<string, string> _REGEXPOUERY
        {
            get { return BuildRegexp(m_InternalRegexpFile); }
        }

        public static Dictionary<string, string> m_REGEXPOUERY = _REGEXPOUERY;
        
        public static string _REGEX_FORARBEIDER = global.m_REGEXPOUERY.Where(p => p.Key == "forarbeidertot").First().Value;
        public static string _dbPath
        {
            get { return m_dbPath; }
            set { m_dbPath = value; }
        }

        public static string _dbArea
        {
            get { return m_dbArea; }
            set { m_dbArea = value; }
        }


        public static WebResponse GetWebPage(this string Url)
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
            return webRequestObject.GetResponse();
        }

        public static string CleaningInvisibleSymbols(this string str)
        {
            //Removes invisible symbols like "soft hypen"
            return Regex.Replace(str, @"\xAD", "");
        }


        public static string GetElementText(this XElement e)
        {
                return e.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate();
        }

        public static string GetElementText(this XElement e, string sep)
        {
            return e.DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate(sep);
        }

        public static string GetNodeText(this XNode e)
        {
            if (e.NodeType == XmlNodeType.Text)
            {
                return e.ToString();
            }
            else if (e.NodeType == XmlNodeType.Element)
                return ((XElement)e).DescendantNodes().OfType<XText>().Select(s => s.ToString()).StringConcatenate();
            else
                return "";
        }

        #region //Lov funksjoner//
        //=====================================================================================================
        //Lov dokument funksjoner START
        //=====================================================================================================
        
        public static XElement ConvertLawParagraf(XElement inPart)
        {
            XElement part = new XElement(inPart);
            XElement paragrafPart = new XElement("para");
            List<XNode> nodes = part.Nodes()
                            .ToList();
            foreach (XNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    if (node.ToString().Trim() == "")
                    {
                        node.Remove();
                    }
                    else
                    {
                        node.ReplaceWith(new XElement("p", node));
                    }
                }
                else if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.GetNodeText().Trim() == "")
                    {
                        node.Remove();
                    }
                }
            }

            //Ledd nummer er angitt
            List<XElement> nNodes = part
                            .Elements()
                            .Where(p =>
                                p.Name.LocalName == "p"
                            && Regex.IsMatch(p.GetElementText().Trim(), @"^\((\s+)?\d+(\s+)?\)")
                            )
                            .ToList();
            if (nNodes.Count() != 0)
            {
                List<XElement> elements = part
                                          .Elements()
                                          .ToList();
            }
            //Ledd nummer ikke angitt
            else
            {
            }

            return paragrafPart;
        }

        private static void GetPartElement(XElement e, XElement newPart)
        {
            if (e.Name == "p" && e.Elements("small").Count() != 0)
            {
                newPart.Add(new XElement("comment", EvalNode(e.Elements("small").First()).Nodes()));
            }
            else if (e.Name == "t")
            {
                newPart.Add(new XElement("p", e.Attributes(), EvalNode(e).Nodes()));
            }

            else if (e.Name == "p" && e.Elements("small").Count() == 0)
            {
                newPart.Add(new XElement("p", e.Attributes(), EvalNode(e).Nodes()));
            }
            else if (e.Name == "table"
                    && e.Elements("tr").Elements("td").Count() == 2
                    && e.Elements("tr").First().Elements("td").First().Elements("small").Count() != 0)
            {
                if (e.Elements("tr").First().Elements("td").First().Value.Trim() != "")
                {
                    newPart.Add(new XElement("footnote",
                        new XAttribute("fid", "0"),
                        new XElement("sup", e.Elements("tr").First().Elements("td").First().Value),
                        new XElement("t", e.Elements("tr").First().Elements("td").Last().Value)));
                }
                else
                {
                    newPart.Add(new XElement("footnote",
                        new XAttribute("fid", "0"),
                        new XElement("t", e.Elements("tr").First().Elements("td").Last().Value)));
                }
            }
            else if (e.Name == "table"
                && e.Descendants("tr").Count() == 1
                && e.Descendants("td").Count() == 2
                && e.Descendants("th").Count() == 0
                && e.Descendants("small").Count() == 0)
            {
                newPart.Add(new XElement("table", e.Attributes(), new XAttribute("type", "asis"), EvalNode(e).Nodes()));
            }
            else if (e.Name == "table"
                && e.Descendants("tr").Count() != 1
                && e.Descendants("td").Count() != 0
                && e.Descendants("th").Count() != 0
                && e.Descendants("small").Count() == 0)
            {
                newPart.Add(new XElement("table", e.Attributes(), new XAttribute("type", "asis"), EvalNode(e).Nodes()));
            }
            else
                newPart.Add(new XElement(e.Name.LocalName, e.Attributes(), EvalNode(e).Nodes()));

        }

        private static XElement EvalNode(XElement el)
        {
            XElement returnElement = new XElement("X");
            foreach (XNode xe in el.Nodes())
            {
                if (xe.NodeType == XmlNodeType.Text)
                {
                    returnElement.Add(new XElement("t", xe.ToString()));
                }
                else if (xe.NodeType == XmlNodeType.Element)
                {
                    XElement myEl = (XElement)xe;
                    if (myEl.Name.LocalName == "footnotelov")
                    {
                        returnElement.Add(new XElement(myEl));
                    }
                    else if (myEl.Name.LocalName == "sub")
                    {
                        returnElement.Add(new XElement(myEl));
                    }
                    else if (myEl.Elements().Count() == 0
                            && (
                                myEl.Name.LocalName == "i"
                                || myEl.Name.LocalName == "b"
                                || myEl.Name.LocalName == "u"
                                )
                             )
                    {
                        returnElement.Add(new XElement("t", myEl.Value.ToString(),
                            new XAttribute("type", myEl.Name.LocalName)));
                    }
                    else if (myEl.Elements().Count() != 0
                            && (
                                myEl.Name.LocalName == "i"
                                || myEl.Name.LocalName == "b"
                                || myEl.Name.LocalName == "u"
                                )
                             )
                    {
                        returnElement.Add(new XElement("uth"
                            , new XAttribute("type", myEl.Name.LocalName)
                            , EvalNode(myEl).Nodes()));
                    }

                    else
                        returnElement.Add(new XElement(myEl.Name.ToString(), myEl.Attributes(), EvalNode(myEl).Nodes()));
                }
            }
            return returnElement;
        }

        //=====================================================================================================
        //Lov dokument funksjoner SLUTT
        //=====================================================================================================
#endregion

        public static bool HasLevelOrSections(this XNode p)
        {
            bool returnValue = false;
            if ("/section/level/document/".IndexOf(p.NodeType == XmlNodeType.Element ? "/" + ((XElement)p).Name.LocalName + "/" : "//") != -1)
            {
                returnValue = true;
            }
            return returnValue;
        }

        public static bool IsLevelDocument(this XElement d)
        {
            bool returnValue = false;
            if ((d.DescendantsAndSelf("document").Count() == 0 ? ""
                    :
                    (d.DescendantsAndSelf("document").First().Attribute("doctypeid") == null ?
                    "" : d.DescendantsAndSelf("document").First().Attribute("doctypeid").Value)) == "7")
            {
                returnValue = true;
            }
            return returnValue;
        }

        public static bool IsExactMatch(this string s, string regEx)
        {
            if (Regex.IsMatch(s, regEx))
            {
                return true;
            }
            return false;
        }

        public static XElement IdentifySectionAsKode(this XElement d)
        {
            List<XElement> ids = d
                    .Descendants()
                    .Where(p => (
                        (
                            p.Name.LocalName == "document"
                        || p.Name.LocalName == "section"
                        || p.Name.LocalName == "level"
                        )
                        && (p.Attribute("id")==null ? false : p.Attribute("id").Value.StartsWith("K"))
                        )
                    )
                    .ToList();
            foreach (XElement e in ids)
            {
                e.SetSectionID(true);
            }

            List<kodeElements> index = d
                    .Descendants()
                    .Where(p => (
                        (
                            p.Name.LocalName == "document"
                        || p.Name.LocalName == "section"
                        || p.Name.LocalName == "level"
                        )
                        && p.Elements("title").Count() == 1
                        && (p.Elements("title").First().Nodes().Count() != 0 ? p.Elements("title").First().Nodes().First().NodeType : XmlNodeType.None) == XmlNodeType.Text)
                        && (p.Elements("title").First().Nodes().Count() != 0 ? Regex.IsMatch(p.Elements("title").First().Nodes().First().ToString().TrimStart(), @"^\d\d\d(\-A)?") : false)
                        )
                    .Select(p => new kodeElements(p.Elements("title").First()))
                    .ToList();

            for (int i = 0; i < index.Count; i++)
            {
                kodeElements q = index.ElementAt(i);
                if (q == index.Where(p => p.number == q.number).Last())
                {
                    q.p.Parent.Attribute("id").Value = "K" + q.number.ToString();
                }
            }
            return d;
        }

        public static XElement IdentifySectionAsKap(this XElement d)
        {
            List<docElement> index = d
                    .Descendants()
                    .Where(p => (
                        (
                            p.Name.LocalName == "document"
                        ||  p.Name.LocalName == "section"
                        ||  p.Name.LocalName == "level"
                        )
                        && p.Elements("title").Count() == 1
                        && (p.Elements("title").First().Nodes().Count() != 0 ? p.Elements("title").First().Nodes().First().NodeType : XmlNodeType.None) == XmlNodeType.Text)
                        && (p.Elements("title").First().Nodes().Count() != 0 ? Regex.IsMatch(p.Elements("title").First().Nodes().First().ToString().TrimStart(), @"^((\d+(\.)?\s)|\d+(\.\d+)+(\.)?\s)") : false)
                        )
                    .Select(p => new docElement(p.Elements("title").First()))
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
                        //p.p.ReplaceWith(new XElement("h1", p.p.Nodes()));
                        p.p.Parent.Attribute("id").Value = "KAP" + p.number;
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
                                //p.p.ReplaceWith(new XElement("h1", p.p.Nodes()));
                                p.p.Parent.Attribute("id").Value = "KAP" + p.number;
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
                        //p.p.ReplaceWith(new XElement("h1", p.p.Nodes()));
                        p.p.Parent.Attribute("id").Value = "KAP" + p.number;
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
                    if (p.t3 != "") { n3 = Convert.ToInt32(p.t3); }
                    if (p.t4 != "") { n4 = Convert.ToInt32(p.t4); }
                    if (p.t5 != "") { n5 = Convert.ToInt32(p.t5); }

                    if (string.CompareOrdinal(p.test, current) > 0)
                    {
                        current = p.test;
                        bTitle = true;
                        nl1 = 0;

                        //p.p.ReplaceWith(new XElement("h" + p.hx, p.p.Nodes()));
                        p.p.Parent.Attribute("id").Value = "KAP" + p.number;
                    }

                }

                if (bTitle)
                {
                    Debug.Print(p.text);
                }
            }
            return d;
        }

        


        public static bool PartElementSave(this XElement part, string partName, XElement xml, string fileName)
        {
            
            string targetDir = part.PartGetTargetFolder();
            if (targetDir != "") return false;
            if (part.Element(partName) == null)
            {
                part.Add(new XElement(partName, partName + @"\" + fileName));
            }
            else
            {
                part.Element(partName).Value = partName + @"\" + fileName;
            }

            if (!Directory.Exists(targetDir + partName)) Directory.CreateDirectory(targetDir + partName);
            xml.Save(targetDir + partName + @"\" + fileName);
            return true;
        }

        public static bool SetFolderName(this string folderName)
        {
            bool returnValue = false;
            try
            {
                if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
                returnValue = true;
            }
            catch
            {
            }
            return returnValue;
        }

        public static string PartGetTargetFolder(this XElement part)
        {
            string path = global._dbPath;
            string folder =
                (
                    part.Ancestors("document").Count() == 0 ?
                    ""
                    :
                    (part.Ancestors("document").First().Element("foldername") == null ?
                    ""
                    :
                    part.Ancestors("document").First().Element("foldername").Value
                    )
                );
            if (folder == "")
                return "";
            if (!path.EndsWith(@"\"))
                path = path + @"\";

            if (!folder.EndsWith(@"\"))
                folder = folder + @"\";
            string targetDir = path + folder;
            return targetDir;
        }
        
        public static string PartElementExists(this XElement part, string name)
        {
            string targetDir = part.PartGetTargetFolder();
            if (targetDir == "") return "";
            return 
            (part.Element(name) == null ?
                ""
                :
                (!File.Exists(targetDir + part.Element(name).Value) ?
                ""
                :
                targetDir + part.Element(name).Value
                )
            );

        }
        
        public static string EditFinalOrXML(this XElement part)
        {
            string targetDir = part.PartGetTargetFolder();
            string final = part.PartElementExists("final");
            string xml = part.PartElementExists("xml");
            return (final != "" ? final : (xml != "" ? xml : ""));

        }



        public static XElement GetContentDocument(this XElement document, bool referenceTopParent)
        {

            XElement content = new XElement("content");
            foreach (XElement e in document.Elements("section"))
            {
                e.GetContentItem(content, referenceTopParent);
            }
            return content;
        }

        public static void GetIndexItems(this XElement el, XElement items, bool referenceTopParent)
        {

            foreach (XElement e in el.Elements().Where(p=>
                    p.Name.LocalName =="section"
                    || p.Name.LocalName == "lovledd"
                    || p.Name.LocalName == "lovpunktum"
                    || p.Name.LocalName == "lovpunkt"
                    ))
            {
                e.SetSectionID();
                string strTitle = "";
                string id = e.Attribute("id").Value; 
                string pid = "";
                
                if (!referenceTopParent) pid = id;
                else pid = items.AncestorsAndSelf("item").Last().Attribute("id").Value;

                if ((e.Element("title") == null ? "" : e.Element("title").Value).Trim() == "")
                {
                    switch (e.Name.LocalName)
                    {
                        case "section": strTitle = e.GetElementText(); break;
                        case "lovledd": strTitle = "Ledd " + (e.NodesBeforeSelf().OfType<XElement>().Where(p=>p.Name.LocalName=="lovledd").Count() + 1).ToString(); break;
                        case "lovpunktum": strTitle = "Punktum " + (e.NodesBeforeSelf().OfType<XElement>().Where(p => p.Name.LocalName == "lovpunktum").Count() + 1).ToString(); break;
                        case "lovpunkt": strTitle = "(" + e.Attribute("value").Value + ")"; break;
                        default: strTitle = e.GetElementText(); break;
                    }
                    if (strTitle.Length > 50)
                        strTitle = strTitle.Substring(0,49) + "..."; 
                }
                else
                {
                    strTitle = e.Element("title").Value.Trim();
                }

                XElement item = new XElement("item",
                                    new XAttribute("id", id),
                                    new XAttribute("text", strTitle),
                                    new XAttribute("pid", pid));

                GetIndexItems(e, item, referenceTopParent);
                items.Add(item);

            }
        }


        public static void AddLovPunkt(this XElement leddHolder, XElement lovpunkt)
        {
            if (leddHolder.Descendants("lovpunkt").Count() == 0)
            {
                leddHolder.Add(lovpunkt);
                lovPunkts lp = new lovPunkts();
                leddHolder.AddAnnotation(lp);
            }
            else
            {
                XElement lastLh = leddHolder.Descendants("lovpunkt").Last();
                if (lovpunkt.Attribute("value").Value == "I")
                {
                    XElement lastAlfa = leddHolder.Descendants("lovpunkt").Where(p => p.Attribute("value").Value == "H").FirstOrDefault();
                    if (lastAlfa != null)
                    {
                        XElement lastElement = lastLh.NodesAfterSelf().OfType<XElement>().LastOrDefault();
                        if (lastElement != null)
                        {
                            lastElement.AddAfterSelf(lovpunkt);
                            return;
                        }
                        else
                        {
                            lastAlfa.AddAfterSelf(lovpunkt);
                            return;
                        }
                    }
                    else
                    {
                        lovpunkt.Attribute("punkttype").Value = "upperroman";
                        return;
                    }
                }
                
                if (lastLh.Attribute("punkttype").Value == lovpunkt.Attribute("punkttype").Value 
                    && lastLh.Attribute("level").Value == lovpunkt.Attribute("level").Value)
                {
                    lastLh.AddAfterSelf(lovpunkt);
                }
                else if (lastLh.Attribute("punkttype").Value != lovpunkt.Attribute("punkttype").Value 
                    && Convert.ToInt32(lastLh.Attribute("level").Value) > Convert.ToInt32(lovpunkt.Attribute("level").Value))
                {
                    lastLh = leddHolder.Descendants("lovpunkt")
                        .Where(p => 
                            p.Attribute("level").Value == lovpunkt.Attribute("level").Value
                            )
                            .LastOrDefault();
                    if (lastLh == null ? false : (lastLh.Ancestors("lovpunkt").Count() == 0))
                    {
                        lastLh.AddAfterSelf(lovpunkt);
                    }
                    else if (lastLh.Ancestors("lovpunkt").Last().NodesAfterSelf().Count() != 0)
                    {
                        lastLh.Ancestors("lovpunkt").Last().NodesAfterSelf().Last().AddAfterSelf(lovpunkt);
                    }
                    else
                    {
                    }
                }
                else if (lastLh.Attribute("punkttype").Value != lovpunkt.Attribute("punkttype").Value
                    && Convert.ToInt32(lastLh.Attribute("level").Value) < Convert.ToInt32(lovpunkt.Attribute("level").Value))
                {
                    if (lastLh.Ancestors("lovpunkt").Count() == 0)
                    {
                        lastLh.Add(lovpunkt);
                    }
                    else
                    {
                    }
                }
                else if (lastLh.Attribute("punkttype").Value != lovpunkt.Attribute("punkttype").Value
                    && Convert.ToInt32(lastLh.Attribute("level").Value) == Convert.ToInt32(lovpunkt.Attribute("level").Value)
                    )
                {
                    XElement lastOfThisPunktType = leddHolder.Descendants("lovpunkt").Where(p =>
                            p.Attribute("punkttype").Value == lovpunkt.Attribute("punkttype").Value).LastOrDefault();
                    if (lastOfThisPunktType != null)
                    {
                        if (lastOfThisPunktType.Parent.Name.LocalName == "lovpunkt")
                        {
                            int lovpunktLevel = lastOfThisPunktType.Ancestors("lovpunkt").Count();
                            XElement top = lastOfThisPunktType.Ancestors("lovpunkt").Last();
                            int sib = top.NodesAfterSelf().OfType<XElement>().Where(p => p.Name.LocalName == "lovpunkt").Count();
                            if (sib == 0)
                            {
                                XElement lastOfThisParent = lastOfThisPunktType.Parent;

                                XElement lastOfThisParentLast = lastOfThisParent.NodesAfterSelf().OfType<XElement>().LastOrDefault();
                                if (lastOfThisParentLast != null) lastOfThisParent = lastOfThisParentLast;

                                lastOfThisParent.Add(lovpunkt);
                            }
                            else
                            {
                                XElement lastTop = top.NodesAfterSelf().OfType<XElement>().Where(p => p.Name.LocalName == "lovpunkt").Last();
                                XElement nextlp = null;
                                for (int i = 0; i < lovpunktLevel - 1; i++)
                                {
                                    if (lastTop.Elements("lovpunkt").LastOrDefault() != null)
                                    {
                                        nextlp = lastTop.Elements("lovpunkt").LastOrDefault();
                                    }
                                }
                                if (nextlp != null)
                                {
                                    nextlp.Add(lovpunkt);
                                }
                                else
                                {
                                    lastTop.Add(lovpunkt); 
                                }

                            }
                        }
                        else 
                        {
                            lastOfThisPunktType.Parent.Add(lovpunkt);
                        }

                    }
                    else
                    {
                        lastLh = leddHolder.Descendants("lovpunkt").Where(p =>
                                p.Attribute("punkttype").Value != lovpunkt.Attribute("punkttype").Value
                                && Convert.ToInt32(p.Attribute("level").Value) == Convert.ToInt32(lovpunkt.Attribute("level").Value))
                                .LastOrDefault();
                        if (lastLh != null ? lastLh.Parent.Name.LocalName == "lovledd" : false)
                        {
                            XElement lastElement = lastLh.NodesAfterSelf().OfType<XElement>().LastOrDefault();
                            if (lastElement != null)
                            {
                                lastElement.AddAfterSelf(lovpunkt);
                            }
                            else
                            {
                                lastLh.Add(lovpunkt);
                            }
                        }
                        else
                        {
                            if (lastLh == leddHolder.Descendants().Where(p =>
                                p.Name.LocalName == "lovpunktum"
                                || p.Name.LocalName == "lovpunkt"
                                || p.Name.LocalName == "lovledd"
                                ).Last())
                            {
                                lastLh.Add(lovpunkt);
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }

        }



        public static List<qResult> GetMatchGroups(this GroupCollection groups, List<string> groupNames)
        {
            return groupNames
                   .Where(o => groups[o].Success)
                   .SelectMany(g => from c in groups[g].Captures.Cast<Capture>()
                                select new qResult
                                {
                                    n = c.Index,
                                    length = c.Length,
                                    name = g,
                                    value = c.Value
                                })
                   .OrderBy(p => p.n)
                   .OrderByDescending(q => q.length) 
                   .ToList();
                   
        }

        private static IEnumerable<qResult> GetCaptures(this CaptureCollection captureCollection, string p)
        {
            return from c in captureCollection.Cast<Capture>() 
                       select new qResult { 
                            n = c.Index,
                            length = c.Length,
                            name = p,
                            value = c.Value
                       };
        }
        
        public static List<qResult> MatchEx(this Match m, Regex r)
        {
            List<string> groupNames = r.GetGroupNames()
                                    .Where(p => !Regex.IsMatch(p, @"^\d+$"))
                                    .ToList();
            
            List<qResult> qr = GetMatchGroups(m.Groups, groupNames);

            foreach (qResult q in qr.OrderBy(p => p.n).OrderByDescending(q => q.length))
            {
                List<qResultDiff> qrdiffs = new List<qResultDiff>();
                foreach (qResult o in qr.Where(p => p!=q
                                && p.n.Between(q.n, q.n + q.length,true) 
                                && (p.n + p.length).Between(q.n, q.n + q.length,true)))
                {
                    qResultDiff qrdiff = new qResultDiff();
                    qrdiff.diff = (q.n - o.n) + ((o.n + o.length) - (q.n + q.length));
                    qrdiff.qR = o;
                    qrdiffs.Add(qrdiff);
                }

                if (qrdiffs.Count() != 0)
                {
                    q.listqR = qrdiffs; //double min = qrdiffs.Min(p => p.diff);
                }
            }
            return qr;
        }

        public static Dictionary<string, string> BuildRegexp(this string regexpFile)
        {
            Dictionary<string, string> returnValue = null;
            if (!File.Exists(regexpFile)) return returnValue;
            try
            {
                XElement regExp = XElement.Load(regexpFile);
                ReadRegExExpressionsEx1 rBuild = new ReadRegExExpressionsEx1(0);

                Dictionary<string, string> dict = rBuild.Build_Regexp_Dictionary(regExp);
                returnValue = dict;
            }
            catch (SystemException e)
            {
                returnValue = null;
            }
            return returnValue;
        }

        public static string GetHtml(this string sURL, ref Encoding encoding)
        {
            string strWebPage = "";
            // create request
            System.Net.WebRequest objRequest = System.Net.HttpWebRequest.Create(sURL);
            // get response
            System.Net.HttpWebResponse objResponse;
            objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse();
            // get correct charset and encoding from the server's header
            string Charset = objResponse.CharacterSet;
            encoding = Encoding.GetEncoding(Charset);
            // read response
            using (StreamReader sr =
                   new StreamReader(objResponse.GetResponseStream(), encoding))
            {
                strWebPage = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }

            // Check real charset meta-tag in HTML
            string regexp = @"charset=\""(?<charset>([^\""]+))\""";
            Match m = Regex.Match(strWebPage,regexp );
            if (m.Success)

            //int CharsetStart = strWebPage.IndexOf("charset=");
            //if (CharsetStart > 0)
            {
                string RealCharset = m.Groups["charset"].Value;

                // real charset meta-tag in HTML differs from supplied server header???
                if (RealCharset.ToLower() != Charset.ToLower())
                {
                    // get correct encoding
                    Encoding CorrectEncoding = Encoding.GetEncoding(RealCharset);

                    // read the web page again, but with correct encoding this time
                    //   create request
                    System.Net.WebRequest objRequest2 = System.Net.HttpWebRequest.Create(sURL);
                    //   get response
                    System.Net.HttpWebResponse objResponse2;
                    objResponse2 = (System.Net.HttpWebResponse)objRequest2.GetResponse();
                    //   read response
                    using (StreamReader sr =
                      new StreamReader(objResponse2.GetResponseStream(), CorrectEncoding))
                    {
                        strWebPage = sr.ReadToEnd();
                        // Close and clean up the StreamReader
                        sr.Close();
                        encoding = CorrectEncoding;
                        return strWebPage;
                    }
                }
            }
            return strWebPage;
        }


        public static string ConvertTidy(this string html, Encoding en, ref string err)
        {
            Tidy tidy = new Tidy();

            /* Set the options you want */
            tidy.Options.DocType = DocType.Omit;
            tidy.Options.DropFontTags = true;
            tidy.Options.LogicalEmphasis = true;
            tidy.Options.Xhtml = true;
            tidy.Options.XmlOut = true;
            tidy.Options.MakeClean = true;
            tidy.Options.TidyMark = false;
            

            /* Declare the parameters that is needed */
            TidyMessageCollection tmc = new TidyMessageCollection();
            MemoryStream input = new MemoryStream();
            MemoryStream output = new MemoryStream();

            byte[] byteArray = Encoding.UTF8.GetBytes(html);
            input.Write(byteArray, 0, byteArray.Length);
            input.Position = 0;
            tidy.Parse(input, output, tmc);
            if (tmc.Errors !=0)
            {
                string errorMsg = "";
                string warningMsg = "";
                string infoMsg = "";
                foreach (TidyMessage m in tmc)
                {
                    if (m.Level == MessageLevel.Error)
                    {
                        errorMsg = errorMsg + m.Message + "\r\n";
                    }
                }
                foreach (TidyMessage m in tmc)
                {
                    if (m.Level == MessageLevel.Warning)
                    {
                        warningMsg = warningMsg + m.Message + "\r\n";
                    }
                }
                foreach (TidyMessage m in tmc)
                {
                    if (m.Level == MessageLevel.Warning)
                    {
                        infoMsg = infoMsg + m.Message + "\r\n";
                    }
                }

                err = (infoMsg == "" ? "" : "Tidy Info:\r\n" + infoMsg + "\r\n")
                    + (warningMsg == "" ? "" : "Tidy Warning:\r\n" + warningMsg + "\r\n")
                    + (errorMsg == "" ? "" : "Tidy Error:\r\n" + errorMsg + "\r\n");

            }
            string result = Encoding.UTF8.GetString(output.ToArray());
            return result;
        }


        public static XElement GetWebPageHTML(this string href, ref string err)
        {
            XElement returnValue = null;
            try
            {

                Encoding encoding = null;

                string strHtml = GetHtml(href, ref encoding);
                //strHtml = Regex.Replace(strHtml, @"\<([a-zA-Z0-9])+\>", "");
                strHtml = Regex.Replace(strHtml, @"\<mb\>", "");
                strHtml = Regex.Replace(strHtml, @"\<ms\>", "");
                strHtml = Regex.Replace(strHtml, @"\<m3\>", "");
                strHtml = Regex.Replace(strHtml, @"\<gh\>", "");
                strHtml = Regex.Replace(strHtml, @"\<x\>", "");
                if (strHtml != "" && encoding != null)
                {

                    
                    strHtml = ConvertTidy(strHtml, encoding, ref err);
                    if (strHtml == "")
                    {
                        return null;
                    }

                    HtmlAgilityPack.HtmlDocument h = new HtmlAgilityPack.HtmlDocument();
                    h.OptionWriteEmptyNodes = false;
                    h.OptionAutoCloseOnEnd = false;
                    h.OptionFixNestedTags = true;
                    h.OptionOutputAsXml = true;
                    h.LoadHtml(strHtml);

                    using (StringWriter writer = new StringWriter())
                    {
                        h.Save(writer);
                        using (StringReader reader = new StringReader(writer.ToString().CleanHtml(true)))
                        {
                            returnValue = XElement.Load(reader);
                        }
                    }

                    returnValue.CollapsAllTagsOfName("pb");
                    returnValue.CollapsAllTagsOfName("fn");

                    
                    //string tempFile = Path.GetTempFileName();
                    //h.Save(tempFile);

                    //XDocument xd = XDocument.Load(tempFile, LoadOptions.PreserveWhitespace);
                    //File.Delete(tempFile);
                    //XNamespace xn = "http://www.w3.org/1999/xhtml";
                    //returnValue = xd.Descendants("html").FirstOrDefault();
                    //string shtml = returnValue.ToString();
                    //shtml = shtml.CleanHtml(true);
                    //returnValue = XElement.Parse(shtml);
                    return returnValue;
                }
            }
            catch (SystemException error)
            {
                err = err + "GetWebPageHTML - Error:\r\n" +  error.Message;
            }
            return returnValue;
        }

        public static void CollapsAllTagsOfName(this XElement e, string name)
        {
            List<XElement> items = e.Descendants(name).ToList();
            while (items.Count() != 0)
            {
                e.Descendants(name).First().ReplaceWith(e.Descendants(name).First().Nodes());
                items = e.Descendants(name).ToList();
            }
        }
        public static XElement GetWebpageHtml(this string href, Encoding en)
        {
            XElement returnValue = null;
            try
            {
                HtmlAgilityPack.HtmlDocument h = new HtmlAgilityPack.HtmlDocument();
                StreamReader reader = new StreamReader(WebRequest.Create(href).GetResponse().GetResponseStream(),en); //put your encoding            
                //StreamReader reader = new StreamReader(WebRequest.Create(href).GetResponse().GetResponseStream(), true); //read encoding            
                //Encoding cEncode = reader.CurrentEncoding;

                h.OptionWriteEmptyNodes = true;
                h.OptionAutoCloseOnEnd = false;
                h.OptionOutputAsXml = true;
                h.Load(reader);
                string tempFile = Path.GetTempFileName();
                h.Save(tempFile);
                XElement x = XElement.Load(tempFile, LoadOptions.PreserveWhitespace);
                File.Delete(tempFile);
                XElement html = x.Descendants("html").First();
                string shtml = html.ToString();
                shtml = shtml.CleanHtml(true);
                html = XElement.Parse(shtml);
                returnValue = html;
            }
            catch
            {
                return returnValue;
            }
            return returnValue;

        }
        public static string CleanHtml(this string s, bool removeamp)
        {

            s = Regex.Replace(s, @"(\<\!DOCTYPE)((.)+?)(\>)", "", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            s = ReplaceAmpXXX(s);
            s = Regex.Replace(s, @"(\&|\&amp\;)shy\;", "-");
            s = Regex.Replace(s, @"(\&|\&amp\;)nbsp\;", " ");
            s = Regex.Replace(s, @"\&amp\;gt\;", "&gt;");
            s = Regex.Replace(s, @"\&amp\;lt\;", "&lt;");
            //s = Regex.Replace(s, @"(\&|\&amp\;)le\;", "&#x2264;");
            //s = Regex.Replace(s, @"(\&|\&amp\;)ge\;", "&#x2265;");
            //s = Regex.Replace(s, @"(\&|\&amp\;)quot\;", "&quot;");

            s = Regex.Replace(s, @"\r\n", " ");
            s = Regex.Replace(s, @"(\sxmlns\=\"")((.)+?)(\"")", "");
            s = Regex.Replace(s, @"(\sxml:lang\=\"")((.)+?)(\"")", "");
            s = Regex.Replace(s, @"(\slang\=\"")((.)+?)(\"")", "");

            s = Regex.Replace(s, @"(\&|\&amp\;)rdquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"(\&|\&amp\;)ldquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"(\&|\&amp\;)lsquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"(\&|\&amp\;)rsquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"(\&|\&amp\;)ndash\;", "&#8211;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"(\&|\&amp\;)hellip\;", "&#133;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"(\&|\&amp\;)mdash\;", "&#8212;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            s = Regex.Replace(s, @"(\r\n)+", " ");
            s = Regex.Replace(s, @"\t+", " ");
            s = Regex.Replace(s, @"\s+", " ");
            s = Regex.Replace(s, @"\t", @" ", RegexOptions.IgnoreCase); //non-breaking space			

            s = Regex.Replace(s, @"\<br(\s+)?(\/)?\>", @"<br/>", RegexOptions.IgnoreCase); //non-breaking space			
            s = Regex.Replace(s, @"(\&|\&amp\;)alpha\;", "&#x3B1;"); //greek alpha			
            s = Regex.Replace(s, @"(\&|\&amp\;)Alpha\;", @"&#x391;"); //greek alpha			
            s = Regex.Replace(s, @"(\&|\&amp\;)nbsp\;", @"&#xA0;"); //non-breaking space			
            s = Regex.Replace(s, @"(\&|\&amp\;)iexcl\;", @"&#xA1;"); //inverted exclamation mark			
            s = Regex.Replace(s, @"(\&|\&amp\;)cent\;", @"&#xA2;"); //cent sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)pound\;", @"&#xA3;"); //pound sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)curren\;", @"&#xA4;"); //currency sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)yen\;", @"&#xA5;"); //yen sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)brvbar\;", @"&#xA6;"); //broken vertical bar			
            //s = Regex.Replace(s, @"(\&|\&amp\;)sect\;", @"&#xA7;"); //section sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)sect\;", @"§"); //section sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)uml\;", @"&#xA8;"); //spacing diaeresis - umlaut			
            s = Regex.Replace(s, @"(\&|\&amp\;)copy\;", @"&#xA9;"); //copyright sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)ordf\;", @"&#xAA;"); //feminine ordinal indicator			
            s = Regex.Replace(s, @"(\&|\&amp\;)laquo\;", @"&#xAB;"); //left double angle quotes			
            s = Regex.Replace(s, @"(\&|\&amp\;)not\;", @"&#xAC;"); //not sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)shy\;", @"&#xAD;"); //soft hyphen			
            s = Regex.Replace(s, @"(\&|\&amp\;)reg\;", @"&#xAE;"); //registered trade mark sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)macr\;", @"&#xAF;"); //spacing macron - overline			
            s = Regex.Replace(s, @"(\&|\&amp\;)deg\;", @"&#xB0;"); //degree sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)plusmn\;", @"&#xB1;"); //plus-or-minus sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)sup2\;", @"&#xB2;"); //superscript two - squared			
            s = Regex.Replace(s, @"(\&|\&amp\;)sup3\;", @"&#xB3;"); //superscript three - cubed			
            s = Regex.Replace(s, @"(\&|\&amp\;)acute\;", @"&#xB4;"); //acute accent - spacing acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)micro\;", @"&#xB5;"); //micro sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)para\;", @"&#xB6;"); //pilcrow sign - paragraph sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)middot\;", @"&#xB7;"); //middle dot - Georgian comma			
            s = Regex.Replace(s, @"(\&|\&amp\;)cedil\;", @"&#xB8;"); //	spacing cedilla			
            s = Regex.Replace(s, @"(\&|\&amp\;)sup1\;", @"&#xB9;"); //	superscript one			
            s = Regex.Replace(s, @"(\&|\&amp\;)ordm\;", @"&#xBA;"); //	masculine ordinal indicator			
            s = Regex.Replace(s, @"(\&|\&amp\;)raquo\;", @"&#xBB;"); //	right double angle quotes			
            s = Regex.Replace(s, @"(\&|\&amp\;)frac14\;", @"&#xBC;"); //	fraction one quarter			
            s = Regex.Replace(s, @"(\&|\&amp\;)frac12\;", @"&#xBD;"); //	fraction one half			
            s = Regex.Replace(s, @"(\&|\&amp\;)frac34\;", @"&#xBE;"); //	fraction three quarters			
            s = Regex.Replace(s, @"(\&|\&amp\;)iquest\;", @"&#xBF;"); //	inverted question mark			
            s = Regex.Replace(s, @"(\&|\&amp\;)Agrave\;", @"&#xC0;"); //	latin capital letter A with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)Aacute\;", @"&#xC1;"); //	latin capital letter A with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)Acirc\;", @"&#xC2;"); //latin capital letter A with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)Atilde\;", @"&#xC3;"); //	latin capital letter A with tilde			
            s = Regex.Replace(s, @"(\&|\&amp\;)Auml\;", @"&#xC4;"); //	latin capital letter A with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)Aring\;", @"&#xC5;"); //	latin capital letter A with ring above			
            s = Regex.Replace(s, @"(\&|\&amp\;)AElig\;", @"&#xC6;"); //	latin capital letter AE			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ccedil\;", @"&#xC7;"); //	latin capital letter C with cedilla			
            s = Regex.Replace(s, @"(\&|\&amp\;)Egrave\;", @"&#xC8;"); //	latin capital letter E with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)Eacute\;", @"&#xC9;"); //	latin capital letter E with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ecirc\;", @"&#xCA;"); //	latin capital letter E with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)Euml\;", @"&#xCB;"); //	latin capital letter E with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)Igrave\;", @"&#xCC;"); //	latin capital letter I with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)Iacute\;", @"&#xCD;"); //	latin capital letter I with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)Icirc\;", @"&#xCE;"); //	latin capital letter I with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)Iuml\;", @"&#xCF;"); //	latin capital letter I with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)ETH\;", @"&#xD0;"); //	latin capital letter ETH			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ntilde\;", @"&#xD1;"); //	latin capital letter N with tilde			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ograve\;", @"&#xD2;"); //	latin capital letter O with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)Oacute\;", @"&#xD3;"); //	latin capital letter O with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ocirc\;", @"&#xD4;"); //	latin capital letter O with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)Otilde\;", @"&#xD5;"); //	latin capital letter O with tilde			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ouml\;", @"&#xD6;"); //	latin capital letter O with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)times\;", @"&#xD7;"); //	multiplication sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)Oslash\;", @"&#xD8;"); //	latin capital letter O with slash			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ugrave\;", @"&#xD9;"); //	latin capital letter U with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)Uacute\;", @"&#xDA;"); //	latin capital letter U with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)Ucirc\;", @"&#xDB;"); //	latin capital letter U with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)Uuml\;", @"&#xDC;"); //	latin capital letter U with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)Yacute\;", @"&#xDD;"); //	latin capital letter Y with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)THORN\;", @"&#xDE;"); //	latin capital letter THORN			
            s = Regex.Replace(s, @"(\&|\&amp\;)szlig\;", @"&#xDF;"); //	latin small letter sharp s - ess-zed			
            s = Regex.Replace(s, @"(\&|\&amp\;)agrave\;", @"&#xE0;"); //	latin small letter a with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)aacute\;", @"&#xE1;"); //	latin small letter a with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)acirc\;", @"&#xE2;"); //	latin small letter a with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)atilde\;", @"&#xE3;"); //	latin small letter a with tilde			
            s = Regex.Replace(s, @"(\&|\&amp\;)auml\;", @"&#xE4;"); //	latin small letter a with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)aring\;", @"&#xE5;"); //	latin small letter a with ring above			
            s = Regex.Replace(s, @"(\&|\&amp\;)aelig\;", @"&#xE6;"); //	latin small letter ae			
            s = Regex.Replace(s, @"(\&|\&amp\;)ccedil\;", @"&#xE7;"); //	latin small letter c with cedilla			
            s = Regex.Replace(s, @"(\&|\&amp\;)egrave\;", @"&#xE8;"); //	latin small letter e with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)eacute\;", @"&#xE9;"); //	latin small letter e with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)ecirc\;", @"&#xEA;"); //	latin small letter e with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)euml\;", @"&#xEB;"); //	latin small letter e with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)igrave\;", @"&#xEC;"); //	latin small letter i with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)iacute\;", @"&#xED;"); //	latin small letter i with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)icirc\;", @"&#xEE;"); //	latin small letter i with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)iuml\;", @"&#xEF;"); //	latin small letter i with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)eth\;", @"&#xF0;"); //	latin small letter eth			
            s = Regex.Replace(s, @"(\&|\&amp\;)ntilde\;", @"&#xF1;"); //	latin small letter n with tilde			
            s = Regex.Replace(s, @"(\&|\&amp\;)ograve\;", @"&#xF2;"); //	latin small letter o with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)oacute\;", @"&#xF3;"); //	latin small letter o with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)ocirc\;", @"&#xF4;"); //	latin small letter o with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)otilde\;", @"&#xF5;"); //	latin small letter o with tilde			
            s = Regex.Replace(s, @"(\&|\&amp\;)ouml\;", @"&#xF6;"); //	latin small letter o with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)divide\;", @"&#xF7;"); //	division sign			
            s = Regex.Replace(s, @"(\&|\&amp\;)oslash\;", @"&#xF8;"); //	latin small letter o with slash			
            s = Regex.Replace(s, @"(\&|\&amp\;)ugrave\;", @"&#xF9;"); //	latin small letter u with grave			
            s = Regex.Replace(s, @"(\&|\&amp\;)uacute\;", @"&#xFA;"); //	latin small letter u with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)ucirc\;", @"&#xFB;"); //	latin small letter u with circumflex			
            s = Regex.Replace(s, @"(\&|\&amp\;)uuml\;", @"&#xFC;"); //	latin small letter u with diaeresis			
            s = Regex.Replace(s, @"(\&|\&amp\;)yacute\;", @"&#xFD;"); //	latin small letter y with acute			
            s = Regex.Replace(s, @"(\&|\&amp\;)thorn\;", @"&#xFE;"); //	latin small letter thorn			
            s = Regex.Replace(s, @"(\&|\&amp\;)yuml\;", @"&#xFF;"); //	latin small letter y with diaeresis			

            s = Regex.Replace(s, @"(\&|\&amp\;)Delta\;", @"&#x" + ConvertIntToHex(916) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)iota\;", @"&#x" + ConvertIntToHex(953) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)kappa\;", @"&#x" + ConvertIntToHex(954) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)eta\;", @"&#x" + ConvertIntToHex(951) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)gamma\;", @"&#x" + ConvertIntToHex(947) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)rho\;", @"&#x" + ConvertIntToHex(961) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)eth\;", @"&#x" + ConvertIntToHex(240) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)sigmaf\;", @"&#x" + ConvertIntToHex(962) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)Uuml\;", @"&#x" + ConvertIntToHex(220) + ";");
            s = Regex.Replace(s, @"(\&|\&amp\;)pound\;", @"&#x" + ConvertIntToHex(163) + ";");


            s = Regex.Replace(s, @"(\&|\&amp\;)ge\;", @"&#x2265;");
            s = Regex.Replace(s, @"(\&|\&amp\;)le\;", @"&#x2264;");
            s = Regex.Replace(s, @"(\&|\&amp\;)omega\;", @"&#x3C9;");
            s = Regex.Replace(s, @"(\&|\&amp\;)part\;", @"&#x2202;");
            s = Regex.Replace(s, @"(\&|\&amp\;)radic\;", @"&#x221A;");
            s = Regex.Replace(s, @"(\&|\&amp\;)Sigma\;", @"&#x3A3;");
            s = Regex.Replace(s, @"(\&|\&amp\;)tau\;", @"&#x3C4;");

            s = Regex.Replace(s, @"(\&|\&amp\;)#8209\;", @"-");
            s = Regex.Replace(s, @"(\&|\&amp\;)quot\;", @"&#x201D;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#13\;\&#10\;", @"");
            s = Regex.Replace(s, @"(\&|\&amp\;)#133\;", @"&#x2026;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#145\;", @"&#x2018;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#146\;", @"&#x2019;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#147\;", @"&#x201C;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#148\;", @"&#x201D;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#150\;", @"-");
            s = Regex.Replace(s, @"(\&|\&amp\;)#9658\;", @"&#x2022;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#151\;", "-");//&#151;
            //s = Regex.Replace(s, @"(\&|\&amp\;)#172\;", @"&#xAC");//&#172;
            s = Regex.Replace(s, @"(\&|\&amp\;)#8209\;", "-");//&#8209;
            //&Sigma;
            s = Regex.Replace(s, @"(\&|\&amp\;)Sigma\;", @"&#x3A3;");
            s = Regex.Replace(s, @"(\&|\&amp\;)#8721\;", "&#931;");//&#8721;
            s = Regex.Replace(s, @"(\&|\&amp\;)#8722\;", "&#x2212;");//&#8722;
            s = Regex.Replace(s, @"(\&|\&amp\;)#945\;", @"&#x03B1;");//&#945;
            s = Regex.Replace(s, @"(\&|\&amp\;)#946\;", @"&#x03B2;");//&#946;
            s = Regex.Replace(s, @"(\&|\&amp\;)#949\;", "&#x03B5;");//&#949;
            s = Regex.Replace(s, @"(\&|\&amp\;)#963\;", "&#x03C3;");//&#963;
            s = Regex.Replace(s, @"(\&|\&amp\;)#8730\;", "&#x221A;");//&#8730;

            return s;
        }


        public static string CleanHtml(this string s)
        {

            s = Regex.Replace(s, @"(\<\!DOCTYPE)((.)+?)(\>)", "", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&shy\;", "-");
            s = Regex.Replace(s, @"\&nbsp\;", " ");

            s = Regex.Replace(s, @"\r\n", "");
            s = Regex.Replace(s, @"(\sxmlns\=\"")((.)+?)(\"")", "");
            s = Regex.Replace(s, @"(\sxml:lang\=\"")((.)+?)(\"")", "");
            s = Regex.Replace(s, @"(\slang\=\"")((.)+?)(\"")", "");
            s = Regex.Replace(s, @"\(…\);", "(...);", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&rdquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&ldquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&lsquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&rsquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&ndash\;", "&#8211;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&hellip\;", "&#x2026;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&mdash\;", "&#8212;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            s = Regex.Replace(s, @"(\r\n)+", " ");
            s = Regex.Replace(s, @"\t+", " ");
            s = Regex.Replace(s, @"\s+", " ");
            s = Regex.Replace(s, @"\t", @" ", RegexOptions.IgnoreCase); //non-breaking space			
            s = Regex.Replace(s, @"\<br(\s+)?(\/)?\>", @"<br/>", RegexOptions.IgnoreCase); //non-breaking space			
            s = Regex.Replace(s, @"\&nbsp\;", @"&#xA0;"); //non-breaking space			
            s = Regex.Replace(s, @"\&iexcl\;", @"&#xA1;"); //inverted exclamation mark			
            s = Regex.Replace(s, @"\&cent\;", @"&#xA2;"); //cent sign			
            s = Regex.Replace(s, @"\&pound\;", @"&#xA3;"); //pound sign			
            s = Regex.Replace(s, @"\&curren\;", @"&#xA4;"); //currency sign			
            s = Regex.Replace(s, @"\&yen\;", @"&#xA5;"); //yen sign			
            s = Regex.Replace(s, @"\&brvbar\;", @"&#xA6;"); //broken vertical bar			
            s = Regex.Replace(s, @"\&sect\;", @"&#xA7;"); //section sign			
            s = Regex.Replace(s, @"\&uml\;", @"&#xA8;"); //spacing diaeresis - umlaut			
            s = Regex.Replace(s, @"\&copy\;", @"&#xA9;"); //copyright sign			
            s = Regex.Replace(s, @"\&ordf\;", @"&#xAA;"); //feminine ordinal indicator			
            s = Regex.Replace(s, @"\&laquo\;", @"&#xAB;"); //left double angle quotes			
            s = Regex.Replace(s, @"\&not\;", @"&#xAC;"); //not sign			
            s = Regex.Replace(s, @"\&shy\;", @"&#xAD;"); //soft hyphen			
            s = Regex.Replace(s, @"\&reg\;", @"&#xAE;"); //registered trade mark sign			
            s = Regex.Replace(s, @"\&macr\;", @"&#xAF;"); //spacing macron - overline			
            s = Regex.Replace(s, @"\&deg\;", @"&#xB0;"); //degree sign			
            s = Regex.Replace(s, @"\&plusmn\;", @"&#xB1;"); //plus-or-minus sign			
            s = Regex.Replace(s, @"\&sup2\;", @"&#xB2;"); //superscript two - squared			
            s = Regex.Replace(s, @"\&sup3\;", @"&#xB3;"); //superscript three - cubed			
            s = Regex.Replace(s, @"\&acute\;", @"&#xB4;"); //acute accent - spacing acute			
            s = Regex.Replace(s, @"\&micro\;", @"&#xB5;"); //micro sign			
            s = Regex.Replace(s, @"\&para\;", @"&#xB6;"); //pilcrow sign - paragraph sign			
            s = Regex.Replace(s, @"\&middot\;", @"&#xB7;"); //middle dot - Georgian comma			
            s = Regex.Replace(s, @"\&cedil\;", @"&#xB8;"); //	spacing cedilla			
            s = Regex.Replace(s, @"\&sup1\;", @"&#xB9;"); //	superscript one			
            s = Regex.Replace(s, @"\&ordm\;", @"&#xBA;"); //	masculine ordinal indicator			
            s = Regex.Replace(s, @"\&raquo\;", @"&#xBB;"); //	right double angle quotes			
            s = Regex.Replace(s, @"\&frac14\;", @"&#xBC;"); //	fraction one quarter			
            s = Regex.Replace(s, @"\&frac12\;", @"&#xBD;"); //	fraction one half			
            s = Regex.Replace(s, @"\&frac34\;", @"&#xBE;"); //	fraction three quarters			
            s = Regex.Replace(s, @"\&iquest\;", @"&#xBF;"); //	inverted question mark			
            s = Regex.Replace(s, @"\&Agrave\;", @"&#xC0;"); //	latin capital letter A with grave			
            s = Regex.Replace(s, @"\&Aacute\;", @"&#xC1;"); //	latin capital letter A with acute			
            s = Regex.Replace(s, @"\&Acirc\;", @"&#xC2;"); //latin capital letter A with circumflex			
            s = Regex.Replace(s, @"\&Atilde\;", @"&#xC3;"); //	latin capital letter A with tilde			
            s = Regex.Replace(s, @"\&Auml\;", @"&#xC4;"); //	latin capital letter A with diaeresis			
            s = Regex.Replace(s, @"\&Aring\;", @"&#xC5;"); //	latin capital letter A with ring above			
            s = Regex.Replace(s, @"\&AElig\;", @"&#xC6;"); //	latin capital letter AE			
            s = Regex.Replace(s, @"\&Ccedil\;", @"&#xC7;"); //	latin capital letter C with cedilla			
            s = Regex.Replace(s, @"\&Egrave\;", @"&#xC8;"); //	latin capital letter E with grave			
            s = Regex.Replace(s, @"\&Eacute\;", @"&#xC9;"); //	latin capital letter E with acute			
            s = Regex.Replace(s, @"\&Ecirc\;", @"&#xCA;"); //	latin capital letter E with circumflex			
            s = Regex.Replace(s, @"\&Euml\;", @"&#xCB;"); //	latin capital letter E with diaeresis			
            s = Regex.Replace(s, @"\&Igrave\;", @"&#xCC;"); //	latin capital letter I with grave			
            s = Regex.Replace(s, @"\&Iacute\;", @"&#xCD;"); //	latin capital letter I with acute			
            s = Regex.Replace(s, @"\&Icirc\;", @"&#xCE;"); //	latin capital letter I with circumflex			
            s = Regex.Replace(s, @"\&Iuml\;", @"&#xCF;"); //	latin capital letter I with diaeresis			
            s = Regex.Replace(s, @"\&ETH\;", @"&#xD0;"); //	latin capital letter ETH			
            s = Regex.Replace(s, @"\&Ntilde\;", @"&#xD1;"); //	latin capital letter N with tilde			
            s = Regex.Replace(s, @"\&Ograve\;", @"&#xD2;"); //	latin capital letter O with grave			
            s = Regex.Replace(s, @"\&Oacute\;", @"&#xD3;"); //	latin capital letter O with acute			
            s = Regex.Replace(s, @"\&Ocirc\;", @"&#xD4;"); //	latin capital letter O with circumflex			
            s = Regex.Replace(s, @"\&Otilde\;", @"&#xD5;"); //	latin capital letter O with tilde			
            s = Regex.Replace(s, @"\&Ouml\;", @"&#xD6;"); //	latin capital letter O with diaeresis			
            s = Regex.Replace(s, @"\&times\;", @"&#xD7;"); //	multiplication sign			
            s = Regex.Replace(s, @"\&Oslash\;", @"&#xD8;"); //	latin capital letter O with slash			
            s = Regex.Replace(s, @"\&Ugrave\;", @"&#xD9;"); //	latin capital letter U with grave			
            s = Regex.Replace(s, @"\&Uacute\;", @"&#xDA;"); //	latin capital letter U with acute			
            s = Regex.Replace(s, @"\&Ucirc\;", @"&#xDB;"); //	latin capital letter U with circumflex			
            s = Regex.Replace(s, @"\&Uuml\;", @"&#xDC;"); //	latin capital letter U with diaeresis			
            s = Regex.Replace(s, @"\&Yacute\;", @"&#xDD;"); //	latin capital letter Y with acute			
            s = Regex.Replace(s, @"\&THORN\;", @"&#xDE;"); //	latin capital letter THORN			
            s = Regex.Replace(s, @"\&szlig\;", @"&#xDF;"); //	latin small letter sharp s - ess-zed			
            s = Regex.Replace(s, @"\&agrave\;", @"&#xE0;"); //	latin small letter a with grave			
            s = Regex.Replace(s, @"\&aacute\;", @"&#xE1;"); //	latin small letter a with acute			
            s = Regex.Replace(s, @"\&acirc\;", @"&#xE2;"); //	latin small letter a with circumflex			
            s = Regex.Replace(s, @"\&atilde\;", @"&#xE3;"); //	latin small letter a with tilde			
            s = Regex.Replace(s, @"\&auml\;", @"&#xE4;"); //	latin small letter a with diaeresis			
            s = Regex.Replace(s, @"\&aring\;", @"&#xE5;"); //	latin small letter a with ring above			
            s = Regex.Replace(s, @"\&aelig\;", @"&#xE6;"); //	latin small letter ae			
            s = Regex.Replace(s, @"\&ccedil\;", @"&#xE7;"); //	latin small letter c with cedilla			
            s = Regex.Replace(s, @"\&egrave\;", @"&#xE8;"); //	latin small letter e with grave			
            s = Regex.Replace(s, @"\&eacute\;", @"&#xE9;"); //	latin small letter e with acute			
            s = Regex.Replace(s, @"\&ecirc\;", @"&#xEA;"); //	latin small letter e with circumflex			
            s = Regex.Replace(s, @"\&euml\;", @"&#xEB;"); //	latin small letter e with diaeresis			
            s = Regex.Replace(s, @"\&igrave\;", @"&#xEC;"); //	latin small letter i with grave			
            s = Regex.Replace(s, @"\&iacute\;", @"&#xED;"); //	latin small letter i with acute			
            s = Regex.Replace(s, @"\&icirc\;", @"&#xEE;"); //	latin small letter i with circumflex			
            s = Regex.Replace(s, @"\&iuml\;", @"&#xEF;"); //	latin small letter i with diaeresis			
            s = Regex.Replace(s, @"\&eth\;", @"&#xF0;"); //	latin small letter eth			
            s = Regex.Replace(s, @"\&ntilde\;", @"&#xF1;"); //	latin small letter n with tilde			
            s = Regex.Replace(s, @"\&ograve\;", @"&#xF2;"); //	latin small letter o with grave			
            s = Regex.Replace(s, @"\&oacute\;", @"&#xF3;"); //	latin small letter o with acute			
            s = Regex.Replace(s, @"\&ocirc\;", @"&#xF4;"); //	latin small letter o with circumflex			
            s = Regex.Replace(s, @"\&otilde\;", @"&#xF5;"); //	latin small letter o with tilde			
            s = Regex.Replace(s, @"\&ouml\;", @"&#xF6;"); //	latin small letter o with diaeresis			
            s = Regex.Replace(s, @"\&divide\;", @"&#xF7;"); //	division sign			
            s = Regex.Replace(s, @"\&oslash\;", @"&#xF8;"); //	latin small letter o with slash			
            s = Regex.Replace(s, @"\&ugrave\;", @"&#xF9;"); //	latin small letter u with grave			
            s = Regex.Replace(s, @"\&uacute\;", @"&#xFA;"); //	latin small letter u with acute			
            s = Regex.Replace(s, @"\&ucirc\;", @"&#xFB;"); //	latin small letter u with circumflex			
            s = Regex.Replace(s, @"\&uuml\;", @"&#xFC;"); //	latin small letter u with diaeresis			
            s = Regex.Replace(s, @"\&yacute\;", @"&#xFD;"); //	latin small letter y with acute			
            s = Regex.Replace(s, @"\&thorn\;", @"&#xFE;"); //	latin small letter thorn			
            s = Regex.Replace(s, @"\&yuml\;", @"&#xFF;"); //	latin small letter y with diaeresis			

            s = Regex.Replace(s, @"\&Delta\;", @"&#x" + ConvertIntToHex(916) + ";");
            s = Regex.Replace(s, @"\&iota\;", @"&#x" + ConvertIntToHex(953) + ";");
            s = Regex.Replace(s, @"\&kappa\;", @"&#x" + ConvertIntToHex(954) + ";");
            s = Regex.Replace(s, @"\&eta\;", @"&#x" + ConvertIntToHex(951) + ";");
            s = Regex.Replace(s, @"\&gamma\;", @"&#x" + ConvertIntToHex(947) + ";");
            s = Regex.Replace(s, @"\&rho\;", @"&#x" + ConvertIntToHex(961) + ";");
            s = Regex.Replace(s, @"\&eth\;", @"&#x" + ConvertIntToHex(240) + ";");
            s = Regex.Replace(s, @"\&sigmaf\;", @"&#x" + ConvertIntToHex(962) + ";");
            s = Regex.Replace(s, @"\&Uuml\;", @"&#x" + ConvertIntToHex(220) + ";");
            s = Regex.Replace(s, @"\&pound\;", @"&#x" + ConvertIntToHex(163) + ";");


            s = Regex.Replace(s, @"\&ge\;", @"&#x2265;");
            s = Regex.Replace(s, @"\&le\;", @"&#x2264;");
            s = Regex.Replace(s, @"\&omega\;", @"&#x3C9;");
            s = Regex.Replace(s, @"\&part\;", @"&#x2202;");
            s = Regex.Replace(s, @"\&radic\;", @"&#x221A;");
            s = Regex.Replace(s, @"\&Sigma\;", @"&#x3A3;");
            s = Regex.Replace(s, @"\&tau\;", @"&#x3C4;");

            s = Regex.Replace(s, @"\&#8209\;", @"-");
            s = Regex.Replace(s, @"\&quot\;", @"&#x201D;");
            s = Regex.Replace(s, @"\&#13\;\&#10\;", @"");
            s = Regex.Replace(s, @"\&#133\;", @"&#x2026;");
            s = Regex.Replace(s, @"\&#145\;", @"&#x2018;");
            s = Regex.Replace(s, @"\&#146\;", @"&#x2019;");
            s = Regex.Replace(s, @"\&#147\;", @"&#x201C;");
            s = Regex.Replace(s, @"\&#148\;", @"&#x201D;");
            s = Regex.Replace(s, @"\&#150\;", @"-");
            s = Regex.Replace(s, @"\&#9658\;", @"&#x2022;");
            s = Regex.Replace(s, @"\&#151\;", "-");//&#151;
            //s = Regex.Replace(s, @"\&#172\;", @"&#xAC");//&#172;
            s = Regex.Replace(s, @"\&#8209\;", "-");//&#8209;
            //&Sigma;
            s = Regex.Replace(s, @"\&Sigma\;", @"&#x3A3;");
            s = Regex.Replace(s, @"\&#8721\;", "&#931;");//&#8721;
            s = Regex.Replace(s, @"\&#8722\;", "&#x2212;");//&#8722;
            s = Regex.Replace(s, @"\&#945\;", @"&#x03B1;");//&#945;
            s = Regex.Replace(s, @"\&#946\;", @"&#x03B2;");//&#946;
            s = Regex.Replace(s, @"\&#949\;", "&#x03B5;");//&#949;
            s = Regex.Replace(s, @"\&#963\;", "&#x03C3;");//&#963;
            s = Regex.Replace(s, @"\&#8730\;", "&#x221A;");//&#8730;

            return s;
        }

        public static string CleanHtmlText(this string s)
        {

            char c = (char)8195;
            s = s.Replace(c, ' ');
            //s = Regex.Replace(s, @"\r\n", " ");
            s = Regex.Replace(s, @"\&dagger\;", "&#8224;");
            s = Regex.Replace(s, @"\&Dagger\;", "&#8225;"); 
            s = Regex.Replace(s, @"\x02003", " ");
            s = Regex.Replace(s, @"\x02004", " ");
            s = Regex.Replace(s, @"\x02005", " ");
            s = Regex.Replace(s, @"\&nbsp\;", " ");
            s = Regex.Replace(s, @"\&bull\;", "&#x2022;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            //s = Regex.Replace(s, @"\&nbsp\;", @"&#xA0;"); //non-breaking space			
            s = Regex.Replace(s, @"\&#xA0\;", @" "); //non-breaking space			
            s = Regex.Replace(s, @"\&sum\;", "&#x2211;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&minus\;", "&#x2212;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&alpha\;", "&#x3B1;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&Alpha\;", "&#x391;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&beta\;", "&#x3B2;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&Beta\;", "&#x392;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&Epsilon\;", "&#x395;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&epsilon\;", "&#x3B5;", RegexOptions.Multiline | RegexOptions.Singleline);

            s = Regex.Replace(s, @"\&Sigma\;", "&#x3a3;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&Sigma\;", "&#x3a3;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&Sigma\;", "&#x3a3;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&Sigma\;", "&#x3a3;", RegexOptions.Multiline | RegexOptions.Singleline);
            s = Regex.Replace(s, @"\&sigma\;", "&#x3c3;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&rdquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&ldquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&lsquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&rsquo\;", "&#8220;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&ndash\;", "&#8211;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&hellip\;", "&#133;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\&mdash\;", "&#8212;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

            s = Regex.Replace(s, @"\&iexcl\;", @"&#xA1;"); //inverted exclamation mark			
            s = Regex.Replace(s, @"\&cent\;", @"&#xA2;"); //cent sign			
            s = Regex.Replace(s, @"\&pound\;", @"&#xA3;"); //pound sign			
            s = Regex.Replace(s, @"\&curren\;", @"&#xA4;"); //currency sign			
            s = Regex.Replace(s, @"\&yen\;", @"&#xA5;"); //yen sign			
            s = Regex.Replace(s, @"\&brvbar\;", @"&#xA6;"); //broken vertical bar			
            s = Regex.Replace(s, @"\&sect\;", @"&#xA7;"); //section sign			
            s = Regex.Replace(s, @"\&uml\;", @"&#xA8;"); //spacing diaeresis - umlaut			
            s = Regex.Replace(s, @"\&copy\;", @"&#xA9;"); //copyright sign			
            s = Regex.Replace(s, @"\&ordf\;", @"&#xAA;"); //feminine ordinal indicator			
            s = Regex.Replace(s, @"\&laquo\;", @"&#xAB;"); //left double angle quotes			
            s = Regex.Replace(s, @"\&not\;", @"&#xAC;"); //not sign			
            s = Regex.Replace(s, @"\&shy\;", @"&#xAD;"); //soft hyphen			
            s = Regex.Replace(s, @"\&reg\;", @"&#xAE;"); //registered trade mark sign			
            s = Regex.Replace(s, @"\&macr\;", @"&#xAF;"); //spacing macron - overline			
            s = Regex.Replace(s, @"\&deg\;", @"&#xB0;"); //degree sign			
            s = Regex.Replace(s, @"\&plusmn\;", @"&#xB1;"); //plus-or-minus sign			
            s = Regex.Replace(s, @"\&sup2\;", @"&#xB2;"); //superscript two - squared			
            s = Regex.Replace(s, @"\&sup3\;", @"&#xB3;"); //superscript three - cubed			
            s = Regex.Replace(s, @"\&acute\;", @"&#xB4;"); //acute accent - spacing acute			
            s = Regex.Replace(s, @"\&micro\;", @"&#xB5;"); //micro sign			
            s = Regex.Replace(s, @"\&para\;", @"&#xB6;"); //pilcrow sign - paragraph sign			
            s = Regex.Replace(s, @"\&middot\;", @"&#xB7;"); //middle dot - Georgian comma			
            s = Regex.Replace(s, @"\&cedil\;", @"&#xB8;"); //	spacing cedilla			
            s = Regex.Replace(s, @"\&sup1\;", @"&#xB9;"); //	superscript one			
            s = Regex.Replace(s, @"\&ordm\;", @"&#xBA;"); //	masculine ordinal indicator			
            s = Regex.Replace(s, @"\&raquo\;", @"&#xBB;"); //	right double angle quotes			
            s = Regex.Replace(s, @"\&frac14\;", @"&#xBC;"); //	fraction one quarter			
            s = Regex.Replace(s, @"\&frac12\;", @"&#xBD;"); //	fraction one half			
            s = Regex.Replace(s, @"\&frac34\;", @"&#xBE;"); //	fraction three quarters			
            s = Regex.Replace(s, @"\&iquest\;", @"&#xBF;"); //	inverted question mark			
            s = Regex.Replace(s, @"\&Agrave\;", @"&#xC0;"); //	latin capital letter A with grave			
            s = Regex.Replace(s, @"\&Aacute\;", @"&#xC1;"); //	latin capital letter A with acute			
            s = Regex.Replace(s, @"\&Acirc\;", @"&#xC2;"); //latin capital letter A with circumflex			
            s = Regex.Replace(s, @"\&Atilde\;", @"&#xC3;"); //	latin capital letter A with tilde			
            s = Regex.Replace(s, @"\&Auml\;", @"&#xC4;"); //	latin capital letter A with diaeresis			
            s = Regex.Replace(s, @"\&Aring\;", @"&#xC5;"); //	latin capital letter A with ring above			
            s = Regex.Replace(s, @"\&AElig\;", @"&#xC6;"); //	latin capital letter AE			
            s = Regex.Replace(s, @"\&Ccedil\;", @"&#xC7;"); //	latin capital letter C with cedilla			
            s = Regex.Replace(s, @"\&Egrave\;", @"&#xC8;"); //	latin capital letter E with grave			
            s = Regex.Replace(s, @"\&Eacute\;", @"&#xC9;"); //	latin capital letter E with acute			
            s = Regex.Replace(s, @"\&Ecirc\;", @"&#xCA;"); //	latin capital letter E with circumflex			
            s = Regex.Replace(s, @"\&Euml\;", @"&#xCB;"); //	latin capital letter E with diaeresis			
            s = Regex.Replace(s, @"\&Igrave\;", @"&#xCC;"); //	latin capital letter I with grave			
            s = Regex.Replace(s, @"\&Iacute\;", @"&#xCD;"); //	latin capital letter I with acute			
            s = Regex.Replace(s, @"\&Icirc\;", @"&#xCE;"); //	latin capital letter I with circumflex			
            s = Regex.Replace(s, @"\&Iuml\;", @"&#xCF;"); //	latin capital letter I with diaeresis			
            s = Regex.Replace(s, @"\&ETH\;", @"&#xD0;"); //	latin capital letter ETH			
            s = Regex.Replace(s, @"\&Ntilde\;", @"&#xD1;"); //	latin capital letter N with tilde			
            s = Regex.Replace(s, @"\&Ograve\;", @"&#xD2;"); //	latin capital letter O with grave			
            s = Regex.Replace(s, @"\&Oacute\;", @"&#xD3;"); //	latin capital letter O with acute			
            s = Regex.Replace(s, @"\&Ocirc\;", @"&#xD4;"); //	latin capital letter O with circumflex			
            s = Regex.Replace(s, @"\&Otilde\;", @"&#xD5;"); //	latin capital letter O with tilde			
            s = Regex.Replace(s, @"\&Ouml\;", @"&#xD6;"); //	latin capital letter O with diaeresis			
            s = Regex.Replace(s, @"\&times\;", @"&#xD7;"); //	multiplication sign			
            s = Regex.Replace(s, @"\&Oslash\;", @"&#xD8;"); //	latin capital letter O with slash			
            s = Regex.Replace(s, @"\&Ugrave\;", @"&#xD9;"); //	latin capital letter U with grave			
            s = Regex.Replace(s, @"\&Uacute\;", @"&#xDA;"); //	latin capital letter U with acute			
            s = Regex.Replace(s, @"\&Ucirc\;", @"&#xDB;"); //	latin capital letter U with circumflex			
            s = Regex.Replace(s, @"\&Uuml\;", @"&#xDC;"); //	latin capital letter U with diaeresis			
            s = Regex.Replace(s, @"\&Yacute\;", @"&#xDD;"); //	latin capital letter Y with acute			
            s = Regex.Replace(s, @"\&THORN\;", @"&#xDE;"); //	latin capital letter THORN			
            s = Regex.Replace(s, @"\&szlig\;", @"&#xDF;"); //	latin small letter sharp s - ess-zed			
            s = Regex.Replace(s, @"\&agrave\;", @"&#xE0;"); //	latin small letter a with grave			
            s = Regex.Replace(s, @"\&aacute\;", @"&#xE1;"); //	latin small letter a with acute			
            s = Regex.Replace(s, @"\&acirc\;", @"&#xE2;"); //	latin small letter a with circumflex			
            s = Regex.Replace(s, @"\&atilde\;", @"&#xE3;"); //	latin small letter a with tilde			
            s = Regex.Replace(s, @"\&auml\;", @"&#xE4;"); //	latin small letter a with diaeresis			
            s = Regex.Replace(s, @"\&aring\;", @"&#xE5;"); //	latin small letter a with ring above			
            s = Regex.Replace(s, @"\&aelig\;", @"&#xE6;"); //	latin small letter ae			
            s = Regex.Replace(s, @"\&ccedil\;", @"&#xE7;"); //	latin small letter c with cedilla			
            s = Regex.Replace(s, @"\&egrave\;", @"&#xE8;"); //	latin small letter e with grave			
            s = Regex.Replace(s, @"\&eacute\;", @"&#xE9;"); //	latin small letter e with acute			
            s = Regex.Replace(s, @"\&ecirc\;", @"&#xEA;"); //	latin small letter e with circumflex			
            s = Regex.Replace(s, @"\&euml\;", @"&#xEB;"); //	latin small letter e with diaeresis			
            s = Regex.Replace(s, @"\&igrave\;", @"&#xEC;"); //	latin small letter i with grave			
            s = Regex.Replace(s, @"\&iacute\;", @"&#xED;"); //	latin small letter i with acute			
            s = Regex.Replace(s, @"\&icirc\;", @"&#xEE;"); //	latin small letter i with circumflex			
            s = Regex.Replace(s, @"\&iuml\;", @"&#xEF;"); //	latin small letter i with diaeresis			
            s = Regex.Replace(s, @"\&eth\;", @"&#xF0;"); //	latin small letter eth			
            s = Regex.Replace(s, @"\&ntilde\;", @"&#xF1;"); //	latin small letter n with tilde			
            s = Regex.Replace(s, @"\&ograve\;", @"&#xF2;"); //	latin small letter o with grave			
            s = Regex.Replace(s, @"\&oacute\;", @"&#xF3;"); //	latin small letter o with acute			
            s = Regex.Replace(s, @"\&ocirc\;", @"&#xF4;"); //	latin small letter o with circumflex			
            s = Regex.Replace(s, @"\&otilde\;", @"&#xF5;"); //	latin small letter o with tilde			
            s = Regex.Replace(s, @"\&ouml\;", @"&#xF6;"); //	latin small letter o with diaeresis			
            s = Regex.Replace(s, @"\&divide\;", @"&#xF7;"); //	division sign			
            s = Regex.Replace(s, @"\&oslash\;", @"&#xF8;"); //	latin small letter o with slash			
            s = Regex.Replace(s, @"\&ugrave\;", @"&#xF9;"); //	latin small letter u with grave			
            s = Regex.Replace(s, @"\&uacute\;", @"&#xFA;"); //	latin small letter u with acute			
            s = Regex.Replace(s, @"\&ucirc\;", @"&#xFB;"); //	latin small letter u with circumflex			
            s = Regex.Replace(s, @"\&uuml\;", @"&#xFC;"); //	latin small letter u with diaeresis			
            s = Regex.Replace(s, @"\&yacute\;", @"&#xFD;"); //	latin small letter y with acute			
            s = Regex.Replace(s, @"\&thorn\;", @"&#xFE;"); //	latin small letter thorn			
            s = Regex.Replace(s, @"\&yuml\;", @"&#xFF;"); //	latin small letter y with diaeresis			

            s = Regex.Replace(s, @"\&Delta\;", @"&#x" + ConvertIntToHex(916) + ";");
            s = Regex.Replace(s, @"\&iota\;", @"&#x" + ConvertIntToHex(953) + ";");
            s = Regex.Replace(s, @"\&kappa\;", @"&#x" + ConvertIntToHex(954) + ";");
            s = Regex.Replace(s, @"\&eta\;", @"&#x" + ConvertIntToHex(951) + ";");
            s = Regex.Replace(s, @"\&gamma\;", @"&#x" + ConvertIntToHex(947) + ";");
            s = Regex.Replace(s, @"\&rho\;", @"&#x" + ConvertIntToHex(961) + ";");
            s = Regex.Replace(s, @"\&eth\;", @"&#x" + ConvertIntToHex(240) + ";");
            s = Regex.Replace(s, @"\&sigmaf\;", @"&#x" + ConvertIntToHex(962) + ";");
            s = Regex.Replace(s, @"\&Uuml\;", @"&#x" + ConvertIntToHex(220) + ";");
            s = Regex.Replace(s, @"\&pound\;", @"&#x" + ConvertIntToHex(163) + ";");


            s = Regex.Replace(s, @"\&ge\;", @"&#x2265;");
            s = Regex.Replace(s, @"\&le\;", @"&#x2264;");
            s = Regex.Replace(s, @"\&omega\;", @"&#x3C9;");
            s = Regex.Replace(s, @"\&part\;", @"&#x2202;");
            s = Regex.Replace(s, @"\&radic\;", @"&#x221A;");
            s = Regex.Replace(s, @"\&Sigma\;", @"&#x3A3;");
            s = Regex.Replace(s, @"\&tau\;", @"&#x3C4;");

            s = Regex.Replace(s, @"\&#8209\;", @"-");
            s = Regex.Replace(s, @"\&quot\;", @"&#x201D;");
            s = Regex.Replace(s, @"\&#13\;\&#10\;", @"");
            s = Regex.Replace(s, @"\&#133\;", @"&#x2026;");
            s = Regex.Replace(s, @"\&#145\;", @"&#x2018;");
            s = Regex.Replace(s, @"\&#146\;", @"&#x2019;");
            s = Regex.Replace(s, @"\&#147\;", @"&#x201C;");
            s = Regex.Replace(s, @"\&#148\;", @"&#x201D;");
            s = Regex.Replace(s, @"\&#150\;", @"-");
            s = Regex.Replace(s, @"\&#9658\;", @"&#x2022;");
            s = Regex.Replace(s, @"\&#151\;", "-");//&#151;
            //s = Regex.Replace(s, @"\&#172\;", @"&#xAC");//&#172;
            s = Regex.Replace(s, @"\&#8209\;", "-");//&#8209;
            //&Sigma;
            s = Regex.Replace(s, @"\&Sigma\;", @"&#x3A3;");
            s = Regex.Replace(s, @"\&#8721\;", "&#931;");//&#8721;
            s = Regex.Replace(s, @"\&#8722\;", "&#x2212;");//&#8722;
            s = Regex.Replace(s, @"\&#945\;", @"&#x03B1;");//&#945;
            s = Regex.Replace(s, @"\&#946\;", @"&#x03B2;");//&#946;
            s = Regex.Replace(s, @"\&#949\;", "&#x03B5;");//&#949;
            s = Regex.Replace(s, @"\&#963\;", "&#x03C3;");//&#963;
            s = Regex.Replace(s, @"\&#8730\;", "&#x221A;");//&#8730;

            return s;
        }

        
        public static string GetLegalHtmlLetters()
        {
            string s = "";
            for (int i = 32; i < 128; i++)
            {
                if (i > 90 && i < 95)
                {
                    s = s + @"\"  + (char)i;
                }
                else if (i > 39 && i < 48)
                {
                    s = s + @"\"  + (char)i;
                }
                else if (i == 60
                    || i == 62
                    //|| i == 63
                    || i == 33
                    )
                {
                }
                else
                {
                    s = s + (char)i;
                }
            }
            for (int i = 160; i < 256; i++)
            {
                s = s + (char)i;
            }
            s = s + (char)338;
            s = s + (char)339;
            s = s + (char)352;
            s = s + (char)353;
            s = s + (char)376;
            s = s + (char)402;

            //s = s + (char)8211;
            s = s + (char)8212;
            s = s + (char)8216;
            s = s + (char)8217;
            s = s + (char)8218;
            s = s + (char)8220;
            s = s + (char)8221;
            s = s + (char)8222;
            s = s + (char)8224;
            s = s + (char)8225;
            //s = s + (char)8226;
            s = s + (char)8230;
            s = s + (char)8240;
            s = s + (char)8364;
            s = s + (char)8482;
            return s;
        }

        public static string ReplaceAmpXXX(this string s)
        {
            string sRegex = @"(?<amp>(\&amp\;))(?<rest>(#\d+\;))";
            s = Regex.Replace(s, sRegex
                        ,
                        delegate(Match m)
                        {
                            string text = "&" + m.Groups["rest"].Value;
                            return text.Trim();
                        }
                        , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return s;
        }


        public static List<string> GetTokens(this string exp)
        {
            MatchCollection mc = Regex.Matches(exp, @"(?<!\\)(\$\d+)");
            List<string> result = new List<string>();
            if (mc.Count == 0)
            {
                result.Add(exp);
                return  result;
            }
            else
            {
                int pos = 0;
                foreach (Match m in mc)
                {
                    if ((m.Index - pos) > 0)
                    {
                        result.Add(exp.Substring(pos, m.Index));
                    }
                    result.Add(m.Value.Replace("$","g"));
                    pos = m.Index + m.Length;
                }
                if ((exp.Length - pos) > 0)
                {
                    result.Add(exp.Substring(pos, exp.Length - pos));
                }
                return result;
            }
        }

        public static string ReplaceTextWith(this string s, string tokens)
        {
            string result = "";
            try
            {
                List<string> tokenStrings = Regex.Split(tokens, @"\;\;\;").ToList();

                if (tokenStrings.Count == 0) return s;

                List<rGroup> regexpStrings = tokenStrings
                                                .Where(q => Regex.Split(q, @"\|\|\|").Count() == 2)
                                                .Select(p => Regex.Split(p, @"\|\|\|")[0])
                                                .Select(p=> new rGroup {
                                                    exp = p,
                                                    groups = (from Match m in Regex.Matches(p,@"\(\?\<(?<name>(g\d+))\>") select m.Groups["name"].Value).ToList() 
                                                    }
                                                )
                                                .ToList();

                List<rReplace> replaceStrings = tokenStrings
                                                .Where(q => Regex.Split(q, @"\|\|\|").Count() == 2)
                                                .Select(p => Regex.Split(p, @"\|\|\|")[1])
                                                .Select(p => new rReplace
                                                {
                                                    //exp = (from Match m in Regex.Matches(p, @"(\$\d+|.+)") select m.Value).ToList()
                                                    exp = p.GetTokens()
                                                }
                                                )

                                                .ToList();

                if (regexpStrings.Count() != replaceStrings.Count()) return s;

                string sRegex = "";
                for (int i = 0; i < regexpStrings.Count(); i++)
                {
                    try
                    {
                        Regex r = new Regex(regexpStrings.ElementAt(i).exp);
                        if (i > 0) sRegex = sRegex + "|";
                        sRegex = sRegex + "(?<name" + i.ToString() + ">(" + r.ToString() + "))";
                    }
                    catch(SystemException err)
                    {
                        return err.Message;
                    }
                }

                result = Regex.Replace(s, sRegex
                          ,
                          delegate(Match m)
                          {
                              string text = "";
                              bool succsess = false;
                              for (int j = 0; j < regexpStrings.Count(); j++)
                              {
                                  if (m.Groups["name" + j].Success)
                                  {
                                      if (regexpStrings.ElementAt(j).groups.Count() == 0)
                                      {
                                          text = replaceStrings.ElementAt(j).exp.First();
                                      }
                                      else
                                      {
                                          foreach (string repl in replaceStrings.ElementAt(j).exp)
                                          {
                                              if (Regex.IsMatch(repl,@"g\d+"))
                                              {
                                                  text = text + m.Groups[repl].Value;
                                              }
                                              else
                                              {
                                                  text = text + repl;
                                              }
                                          }
                                      }
                                      succsess = true;
                                      break;
                                  }
                              }
                              if (succsess) return text;
                              else return m.Value;
                          }
                            , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
            catch
            {
                result = s;
            }

            return result;
        }
        public static string GetForarbeidNumber(this string s, string sRegex)
        {
            s = Regex.Replace(s, sRegex
                        ,
                        delegate(Match m)
                        {
                            string text = "";
                            if (m.Groups["name1"].Value.ToLower() == "nou")
                            {
                                text = m.Groups["name1"].Value.ToLower()
                                       + " " + m.Groups["year1"].Value.ToLower()
                                       + ":" + m.Groups["nr2"].Value.ToLower();
                            }
                            else
                            {
                                text = m.Groups["name1"].Value.ToLower()
                                    + (m.Groups["name2"].Value == "" ? "" : " " + m.Groups["name2"].Value.ToLower())
                                    + " " + m.Groups["nr1"].Value.ToLower()
                                    + (m.Groups["type"].Value == "" ? "" : " " + m.Groups["type"].Value.ToLower())
                                    + " " + m.Groups["year1"].Value.ToLower()
                                    + (m.Groups["year2"].Value == "" ? "" : "-" + m.Groups["year2"].Value.ToLower())
                                    ;
                            }
                            return text.Trim();
                        }
                        , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return s;
        }

        public static string GetForarbeidTot(this string s, string sRegex)
        {
            if (Regex.IsMatch(s,sRegex, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase))
            {
                s = Regex.Replace(s, sRegex
                        ,
                        delegate(Match m)
                        {
                            string text = "";
                                text = m.Groups["name1"].Value.ToLower()
                                    + (m.Groups["name2"].Value == "" ? "" : "_" + m.Groups["name2"].Value.ToLower().Replace("0","o"))
                                    + (m.Groups["number"].Value == "" ? "" : "_" + m.Groups["number"].Value.ToLower())
                                    //+ (m.Groups["type"].Value == "" ? "" : "_" + m.Groups["type"].Value.ToLower())
                                    + (m.Groups["year"].Value == "" ? "" : m.Groups["year"].Value.ToLower().GetYearData())
                                    ;
                            return text.Trim();
                        }
                        , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
            else
            {
                s = Regex.Replace(s, @"\.|\s+|\(|\)", "");
            }
            return s;
        }

        public static string GetYearData(this string year)
        {
            int n = Convert.ToInt32(year);
            if (n > 1000)
                return year;
            else if (n < 100 && n > 11)
                return "19" + year;
            else if (n < 100 && n <= 11)
                return "20" + year;
            else
                return "";
        }

        public static void IdentifyMatchForarbeider(this string name, XElement d)
        {
            string regEx = _REGEX_FORARBEIDER;
            Regex q = new Regex(regEx);
            Match m = q.Match(name);
            if (m.Success)
            {

                if (m.Groups["name1"].Success && !m.Groups["name2"].Success)
                {
                    if (d.Attribute("name1") != null) d.Attribute("name1").Remove();
                    d.Add(new XAttribute("name1", m.Groups["name1"].Value.Trim().Replace("0", "o")));
                }
                if (m.Groups["name1"].Success && m.Groups["name2"].Success)
                {
                    if (d.Attribute("name1") != null) d.Attribute("name1").Remove();
                    d.Add(new XAttribute("name1", m.Groups["name1"].Value.Trim().Replace("0", "o") + "_" + m.Groups["name2"].Value.Trim().Replace("0", "o")));
                }

                if (m.Groups["number"].Success)
                {
                    if (d.Attribute("number") != null) d.Attribute("number").Remove();
                    d.Add(new XAttribute("number", m.Groups["number"].Value.Trim().Replace("O", "0")));
                }
                if (m.Groups["type"].Success)
                {
                    if (d.Attribute("type") != null) d.Attribute("type").Remove();
                    d.Add(new XAttribute("type", m.Groups["type"].Value.Trim()));
                }
                if (m.Groups["year1"].Success && m.Groups["year2"].Success)
                {
                    if (d.Attribute("year1") != null) d.Attribute("year1").Remove();
                    if (m.Groups["year1"].Value.Trim().GetYearData() != "")
                        d.Add(new XAttribute("year1", m.Groups["year1"].Value.Trim().GetYearData()));

                    if (d.Attribute("year2") != null) d.Attribute("year2").Remove();
                    if (m.Groups["year2"].Value.Trim().GetYearData() != "")
                        d.Add(new XAttribute("year2", m.Groups["year2"].Value.Trim().GetYearData()));
                }
                if (m.Groups["year1"].Success && !m.Groups["year2"].Success)
                {
                    if (d.Attribute("year1") != null) d.Attribute("year1").Remove();
                    if (m.Groups["year1"].Value.Trim().GetYearData() != "")
                        d.Add(new XAttribute("year1", m.Groups["year1"].Value.Trim().GetYearData()));
                }

                if (d.Attribute("noid") != null) d.Attribute("noid").Remove();
                d.Add(new XAttribute("noid", "0"));
            }
            else
            {
                if (d.Attribute("noid") != null) d.Attribute("noid").Remove();
                d.Add(new XAttribute("noid", "1"));
            }

        }

        public static string ConvertIntToHex(this int decValue)
        {
            return decValue.ToString("X");
        }

        public static void SetSectionIdValue(this XElement e, string value)
        {
            if (e.Attribute("id") == null) e.Add(new XAttribute("id", value));
            else e.Attribute("id").Value = value;
        }

        public static void SetSectionID(this XElement section, bool reset)
        {

            if (section.Attribute("id") == null)
            {
                XAttribute id = new XAttribute("id", Guid.NewGuid().GetHashCode().ToString());
                section.Add(id);
            }
            else
            {
                if (reset)
                {
                    section.Attribute("id").Value = Guid.NewGuid().GetHashCode().ToString();
                }
            }
        }

        public static void SetSectionID(this XElement section)
        {

            if (section.Attribute("id") == null)
            {
                XAttribute id = new XAttribute("id", Guid.NewGuid().GetHashCode().ToString());
                section.Add(id);
            }
            else
            {
                if (section.Attribute("id").Value == "")
                {
                    section.Attribute("id").Value = Guid.NewGuid().GetHashCode().ToString();
                }
            }

        }

        public static void CheckSections(this XElement toc, string parentValue)
        {
            int n = 1;
            foreach (XElement s in toc.Elements("section"))
            {
                string titleValue = "";
                titleValue = CheckTitleNumber(s, parentValue);
                CheckSections(s, titleValue);
            }

        }

        public static string CheckTitleNumber(this XElement section, string parentValue)
        {
            string returnValue = "";
            if (section.Element("title") != null)
            {
                XElement e = section.Element("title");
                string titleText = e.DescendantNodes().OfType<XText>().Select(p => (string)p.ToString()).StringConcatenate();
                if (Regex.IsMatch(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))(\.)?\s"))
                {
                    string nr = Regex.Match(titleText.Trim(), @"^(?<nr>(\d+((\.\d+)+)?))(\.)?\s").Groups["nr"].Value;
                    if ((nr != "" && nr.StartsWith(parentValue) && parentValue != "") || parentValue=="new")
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

        public static XElement BuildLevelsFromHeader(this XElement el, string startVal, string sectionName)
        {
            string section = sectionName;
            XElement body = el.Elements("body").FirstOrDefault();
            if (body != null) el = body;

            XElement result = null;
            HtmlHierarchy h = new HtmlHierarchy();
            int counter = 1;
            h.SetElementId(el, ref counter);

            if (el.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0)
            {
                XElement toc = h.GetHtmlToc(el);
                if (toc == null)
                {
                    return el;
                }
                else
                {
                    CheckSections(toc, startVal);

                    XElement newS = new XElement("container");

                    newS.Add(new XElement("text", el.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(toc.Elements("section").First().Attribute("idx").Value))));
                    newS.Add(toc.Nodes());
                    int n = newS.Descendants(section).Count();

                    for (int i = 0; i < n; i++)
                    {
                        XElement sect = newS.Descendants(section).ElementAt(i);

                        XElement next = null;
                        if (i + 1 < n)
                            next = toc.Descendants(section).ElementAt(i + 1);

                        if (next != null)
                        {
                            sect.Element("title").AddAfterSelf(new XElement("text", el.Elements().Where(p =>
                                Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                                &&
                                Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("idx").Value)
                                )));
                        }
                        else
                        {
                            sect.Element("title").AddAfterSelf(new XElement("text", el.Elements().Where(p =>
                                Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                            )));

                        }
                    }

                    result = newS;
                }
            }
            else
            {
                result = el;
            }
            return result;
        }

        public static XElement BuildLevelsFromHeader(this XElement el, string startVal)
        {
            el.Descendants("div").Where(p => p.Nodes().OfType<XText>().Select(q => q.ToString()).StringConcatenate().Trim() == "").Reverse().ToList().ForEach(p => p.ReplaceWith(p.Nodes()));
            el.Descendants("a").Where(p => p.Nodes().OfType<XText>().Select(q => q.ToString()).StringConcatenate().Trim() == "").Remove();
            el.Descendants("a").Where(p => p.Nodes().OfType<XText>().Select(q => q.ToString()).StringConcatenate().Trim().ToLower() == "Til toppen av siden".ToLower()).Select(p=>p.Parent).Remove();
            el = new XElement(el);
            int idx = 0;
            el.Descendants().Attributes("idx").Remove();
            el.Descendants().ToList().ForEach(p => p.Add(new XAttribute("idx",(idx++).ToString())));
            string section = "section";
            XElement body = el.Elements("body").FirstOrDefault();
            if (body != null) el = body;
            
            XElement result = null;
            HtmlHierarchy h = new HtmlHierarchy();
            int counter = 1;
            h.SetElementId(el, ref counter);

            if (el.Descendants().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).Count() != 0)
            {
                XElement toc = h.GetHtmlToc(el);
                if (toc == null)
                {
                    return el;
                }
                else
                {
                    CheckSections(toc, startVal);

                    XElement newS = new XElement("container");

                    newS.Add(new XElement("text", el.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(toc.Elements("section").First().Attribute("idx").Value))));
                    newS.Add(toc.Nodes());
                    int n = newS.Descendants(section).Count();

                    for (int i = 0; i < n; i++)
                    {
                        XElement sect = newS.Descendants(section).ElementAt(i);

                        XElement next = null;
                        if (i + 1 < n)
                            next = newS.Descendants(section).ElementAt(i + 1);

                        if (next != null)
                        {
                           
                                sect.Element("title").AddAfterSelf(new XElement("text", el.Elements().Where(p =>
                                    Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                                    &&
                                    Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("idx").Value)
                                    )));
                           
                        }
                        else
                        {
                                sect.Element("title").AddAfterSelf(new XElement("text", el.Elements().Where(p =>
                                    Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(sect.Attribute("idx").Value)
                                )));

                        }
                    }

                    result = newS;
                }
            }
            else
            {
                result = el;
            }
            return result;
        }

        public static XElement ReplaceSections(this XElement element, string sectionName)
        {
            if (sectionName != "section")
            {
                List<XElement> s = element.DescendantsAndSelf("section").ToList();
                foreach (XElement e in s.OrderBy(p=>p.Descendants("section").Count()))
                {
                    e.ReplaceWith(new XElement(sectionName, e.Attributes(), e.Nodes()));
                }
            }
            return element;
        }
        public static XElement GetContentMain(this XElement root, bool referenceTopParent)
        {
            if (root.Elements("documents").Count() != 0)
            {
                XElement content = new XElement("content");
                foreach (XElement d in root.Elements("documents").First().Elements())
                {
                    XElement d_content = GetContentMain(d, referenceTopParent);
                    if (d_content.HasElements)
                    {
                        content.Add(d_content.Nodes());
                    }

                }
                return content;
            }
            else if (root.DescendantsAndSelf("document").Count() != 0)
            {
                XElement content = new XElement("content");
                foreach (XElement d in root.DescendantsAndSelf("document"))
                {
                    if (d.Elements("title").Count() == 0 && d.Elements("text").Count() != 0)
                    {
                        d.GetContentItem(content, referenceTopParent);
                    }
                    else if (d.Elements("title").Count() != 0)
                    {
                        d.GetContentItem(content, referenceTopParent);
                    }
                    else
                    {
                        XElement contentPart = GetContentMain(d, referenceTopParent);
                        if (contentPart.HasElements)
                        {
                            content.Add(contentPart.Nodes());
                        }
                    }

                }
                return content;
            }
            else if (root.Elements("section").Count() != 0)
            {
                return GetContentDocument(root, referenceTopParent);
            }

            return new XElement("content");
        }

        public static void GetContentItem(this XElement e, XElement parent, bool referenceTopParent)
        {
            e.SetSectionID();
            string strTitle = "";
            string id = e.Attribute("id").Value;
            string pid = id;

            if (referenceTopParent)
            {
                XElement p = parent.AncestorsAndSelf("item").LastOrDefault();
                if (p != null) pid = p.Attribute("id").Value;
            }

            if ((e.Element("title") == null ? "" : e.Element("title").Value).Trim() == "")
            {
                strTitle = e.GetElementText();
                if (strTitle.Length > 26)
                    strTitle = strTitle.Substring(0, 25) + "...";
            }
            else
            {
                strTitle = e.Element("title").Value.Trim();
            }

            XElement item = new XElement("item",
                                new XAttribute("id", id),
                                new XAttribute("text", strTitle),
                                new XAttribute("pid", pid));

            GetIndexItems(e, item, referenceTopParent);
            parent.Add(item);

        }



        
    }
}
