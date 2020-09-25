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
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Dib.Transform;
using Dib.ConvertToXHTML;
using System.Threading;
using SimMetricsApi;
using SimMetricsMetricUtilities;
using SimMetricsUtilities;
using DIB.Data;



namespace TransformData
{
    public partial class frmImportHtml : Form
    {
        private int _vedlCounter = 0; 
        private bool bCancel = false;
        private string shortName = "";
        private string documentDate = "";
        private string url = "";

        private static string _documentImportFile = @"c:\DIB-Import\toimport.xml";
        private static string _documentLibry = @"c:\DIB-Import\Lovdata\";
        private string xmlPath = _documentLibry + @"import.xml";
        private bool doNotselect = false;
        private bool doNotAfterNavigate = false;
        private IDictionary<HtmlElement, string> elementStyles = new Dictionary<HtmlElement, string>();
        private IDictionary<HtmlElement, string> element = new Dictionary<HtmlElement, string>();

        
        
        private int _minPara = 100;
        private int _maxPara = 0;

        public class listItem
        {
            public string type { get; set; }
            public string tag { get; set; }
            public int indent { get; set; }
        }


        private void LoadComboBox()
        {
            if (File.Exists(xmlPath))
            {
                XmlDocument document = new XmlDocument();
                document.Load(xmlPath);
                foreach (XmlElement e in document.SelectNodes("documents/document/name"))
                {
                    textAdress.Items.Add(e.InnerText.ToString());
                }
            }
        }

        public frmImportHtml()
        {
            InitializeComponent();
            CreateWebBrowser();
            ResetImportDocument();
            LoadComboBox();
            textBox1.KeyUp  += new KeyEventHandler(textBox1_KeyUp);

            System.Windows.Forms.ContextMenu contextMenu1;
            contextMenu1 = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem menuItem1;
            menuItem1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem menuItem2;
            menuItem2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem menuItem3;
            menuItem3 = new System.Windows.Forms.MenuItem();

            contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { menuItem1, menuItem2, menuItem3 });
            menuItem1.Index = 0;
            menuItem1.Text = "MenuItem1";
            menuItem2.Index = 1;
            menuItem2.Text = "MenuItem2";
            menuItem3.Index = 2;
            menuItem3.Text = "MenuItem3";

            myWebBrowser.ContextMenu = contextMenu1; 

        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnsearchList.Select();
                btnsearchList_Click(sender, e);
            }
        }

        private void CreateWebBrowser()
        {
            myWebBrowser = new System.Windows.Forms.WebBrowser();
            splitContainer1.Panel2.Controls.Add(myWebBrowser);
            myWebBrowser.Dock = DockStyle.Fill;
            myWebBrowser.Visible = true;
            myWebBrowser.BringToFront();
            myWebBrowser.Navigate(@"about:blank");
            myWebBrowser.Navigated += new WebBrowserNavigatedEventHandler(browser_Navigated);

            
        }

       
        private WebBrowser myWebBrowser;

        private void btnNavigate_Click(object sender, EventArgs e)
        {
            
            if (myWebBrowser == null)
                CreateWebBrowser();
            textAdress.SelectedItem = null;
            textAdress.Text = "";
            myWebBrowser.Navigate("www.lovdata.no");
            myWebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

            adress.Text = e.Url.ToString();
            if (doNotAfterNavigate)
            {

                return;
            }
            else
            {
                XElement d = GetImportDocumentByAdress(adress.Text);
                doNotselect = true;
                //textAdress.SelectedItem = null;
                if (d != null)
                {
                    doNotselect = true;
                    textAdress.Text = d.Element("name").Value;
                }
                else
                {
                    textAdress.SelectedItem = null;
                    textAdress.Text = "";
                }
                doNotselect = false;
            }
        }


        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (doNotAfterNavigate)
            {
                doNotAfterNavigate = false;
                return;
            }
            else
            {
                myWebBrowser.Document.AttachEventHandler("onload", OpenDoc);
                adress.Text = e.Url.ToString();
                XElement d = GetImportDocumentByAdress(adress.Text);
                doNotselect = true;
                //textAdress.SelectedItem = null;
                if (d != null)
                {
                    doNotselect = true;
                    textAdress.Text = d.Element("name").Value;
                }
                doNotselect = false;

                if (myWebBrowser.ReadyState == WebBrowserReadyState.Complete)
                {

                    Application.DoEvents();

                    if (myWebBrowser.Document != null)
                    {

                        myWebBrowser.Document.Body.AttachEventHandler("onLoad", OpenDoc);
                        HtmlElementCollection links = myWebBrowser.Document.Links;
                        foreach (HtmlElement var in links)
                        {
                            var.AttachEventHandler("onclick", LinkClicked);
                        }
                    }
                }
            }
        }
        private void LinkClicked(object sender, EventArgs e)
        {
            HtmlElement link = myWebBrowser.Document.ActiveElement;
            url = link.GetAttribute("href");
            string target = link.GetAttribute("target");
            if (target != "" && target != "_top")
            {
                //MessageBox.Show(url + "////" + target);
                bCancel = true;
                myWebBrowser.Navigate(url);

                while (myWebBrowser.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
                if (myWebBrowser.Document != null)
                {
                    myWebBrowser.Document.AttachEventHandler("onload", OpenDoc);
                    HtmlElementCollection links = myWebBrowser.Document.Links;
                    foreach (HtmlElement var in links)
                    {
                        var.AttachEventHandler("onclick", LinkClicked);
                    }
                }

            }
        }
        private void OpenDoc(object sender, EventArgs e)
        {
            if (myWebBrowser.Document != null)
            {
                HtmlElementCollection body = myWebBrowser.Document.GetElementsByTagName("body");
                foreach (HtmlElement el in body)
                {
                    if (!this.elementStyles.ContainsKey(el))
                    {
                        string style = el.Style;
                        this.elementStyles.Add(el, style);
                        el.Style = style + "; background-color: #ffc; border:solid 1px black;";
                        //this.Text = el.GetAttribute("className").ToString() ?? "(no id)";
                    }

                }

            }
        }


        private void _v0_SetParaValue(XElement e, string testId)
        {
            string fromPara = "";
            string toPara = "";
            if (testId.IndexOf("til") != -1)
            {
                fromPara = testId.Split("til".ToCharArray()).ElementAt(0).Trim();
                toPara = testId.Split("til".ToCharArray()).ElementAt(1).Trim();
            }
            else if (testId.IndexOf((char)0x2013) != -1)
            {
                fromPara = testId.Split((char)0x2013).ElementAt(0).Trim();
                toPara = testId.Split((char)0x2013).ElementAt(1).Trim();
            }
            else if (testId.IndexOf((char)0x2014) != -1)
            {
                fromPara = testId.Split((char)0x2014).ElementAt(0).Trim();
                toPara = testId.Split((char)0x2014).ElementAt(1).Trim();
            }

            testId = testId.Replace(" ", "_").ToUpper();
            if (!testId.StartsWith("ART"))
            {
                testId = "P" + testId;
            }
            XAttribute hid = new XAttribute("hid", testId);
            e.Add(hid);
            if (fromPara != "")
            {
                XAttribute fa = new XAttribute("frompara", fromPara);
                e.Add(fa);
                fa = new XAttribute("topara", toPara);
                e.Add(fa);
            }
        }




        private void _v0_GetFirstLevel(XElement sections
                                ,  List<XElement> h
                                , int min
                                , int max
                                , int level)
        {
            XElement e1 = null;
            int inMin = min;
            while (min < max)
            {
                for (; min < max; min++)
                {
                    e1 = h.ElementAt(min);
                    if (_v0_GetType(e1) == 200)
                    {
                        _v0_ExtractHeader(sections, h, ref min, ref max, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }
                    if (_v0_GetType(e1) == 100 &&  cbFirstLevelText.Checked == true)
                    {
                        _v0_ExtractHeader(sections, h, ref min, ref max, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }
                    else if (_v0_GetType(e1) == 100 && cbFirstLevelText.Checked == false)
                    {
                        int newMin = min + 1;
                        _v0_ExtractHeader(sections, h, ref min, ref newMin, _minPara, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }
                    else if (_v0_GetPara(e1) >= _minPara)
                    {
                        int paraLevel = _v0_GetPara(e1);
                        _v0_ExtractPara(sections, h, ref min, ref max, paraLevel, level + 1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                        
                    }
                    else
                    {
                        _v0_ExtractHeader(sections, h, ref min, ref max, _minPara, level+1);
                        break;
                        //if (min >= max) return;
                        //if (_v0_GetType(h.ElementAt(min)) == 200) break;
                    }

                }
            }
 
        }

        private void _v0_ExtractHeader(XElement section
                        , List<XElement> h
                        , ref int min
                        , ref int max
                        , int paraLevel
                        , int level)
        {
            XElement e1 = null;
            XElement last = null;
            int type = -1;
            while (min < max)
            {
                for (; min < max; min++)
                {
                    e1 = h.ElementAt(min);
                    


                    if (type == -1) type = _v0_GetType(e1);
                    //if (e1.Value.Trim().StartsWith("Alminne")) Debug.Print("xxx");
                    if (_v0_GetType(e1) == type)
                    {
                        if (last != null && type==100)
                        {
                            
                            int ln = Convert.ToInt32(last.Attribute("n").Value);
                            int para00 = _v0_GetPara(h.ElementAt(ln - 1));
                            int type00 = _v0_GetType(h.ElementAt(ln - 1));
                            int para01 = _v0_GetPara(h.ElementAt(min+1));
                            int type01 = _v0_GetType(h.ElementAt(min+1));
                            //if (para00 > 0 && para01 > 0 && para00 > para01)
                            if (para00 > 0 && para00 == para01 && type00 == type01)
                            {
                                para00 = _v0_GetPara(h.ElementAt(ln + 1));
                                if (para00  > 0 && para00 != para01)
                                return;
                            }

                        }
                        last = new XElement("section"
                                , e1.Attributes()
                                , new XAttribute("text", _v0_GetText(e1))
                                , new XAttribute("n", min.ToString())
                                , new XAttribute("level", level));
                        section.Add(last);
                    }
                    else
                    {
                        break;
                    }
                   
                }
                if (min >= max) return;
                int para0 = _v0_GetPara(h.ElementAt(min));
                int type0 = _v0_GetType(h.ElementAt(min));

                if (para0 == paraLevel)
                {
                    _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                    //if (_v0_GetType(h.ElementAt(min)) != type0) return;
                    if (min >= max) return;
                    //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                }
                else if (para0 == paraLevel + 1)
                {
                    _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel +1, level + 1);
                    if (min >= max) return;
                    //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                }

                else if (para0 == 0 && type0 != 100 && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() != 0)
                {
                    return;
                }
                else if (para0 == 0 
                    && type0 == 100
                    && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() != 0)
                {
                    if ((min + 1) < max)
                    {
                        int para1 = _v0_GetPara(h.ElementAt(min + 1));
                        int type1 = _v0_GetType(h.ElementAt(min + 1));
                        if (para1 != 0 && para1 <= paraLevel)
                        {
                            return;
                        }
                    }
                }
                else if (type0 == 200) return;

                else
                {
                    if ((min + 2) < max)
                    {
                        int para1 = _v0_GetPara(h.ElementAt(min + 1));
                        int type1 = _v0_GetType(h.ElementAt(min + 1));
                        int para2 = _v0_GetPara(h.ElementAt(min + 2));
                        int type2 = _v0_GetType(h.ElementAt(min + 2));

                        if (level == 3 && type0 == 100)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                            if (min >= max) return;
                            //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else if (para0 == 0 && para1 == paraLevel)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                            if (min >= max) return;
                            //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else if (para0 == 0 && para1 == 0)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                            if (min >= max) return;
                            //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else if (para0 == 0 && para1 > paraLevel)
                        {
                            _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                            if (min >= max) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                        }
                        else
                            return;
                    }
                    else
                    {
                        if ((min + 1) < max)
                        {
                            int para1 = _v0_GetPara(h.ElementAt(min + 1));
                            int type1 = _v0_GetType(h.ElementAt(min + 1));
                            if (para1 > paraLevel)
                            {
                                _v0_ExtractPara(last, h, ref min, ref max, paraLevel + 1, level + 1);
                                if (min >= max) return;
                                //if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                                if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last) && level != 3 && _v0_GetType(h.ElementAt(min)) != 100) return;
                            }
                            else
                                return;
                        }
                    }

                }
            }

        }

        private void _v0_ExtractPara(XElement section
                                , List<XElement> h
                                , ref int min
                                , ref int max
                                , int paraLevel
                                , int level)
        {

            XElement e1 = null;
            XElement last = null;
            while (min < max)
            {
                
                for (;min < max; min++)
                {
                    e1 = h.ElementAt(min);
                    //if (_v0_GetText(e1).StartsWith("§ 7-3-4")) Debug.Print("xxx");
                    if (_v0_GetPara(e1) == paraLevel)
                    {
                        last = new XElement("section"
                                , e1.Attributes()
                                , new XAttribute("text", _v0_GetText(e1))
                                , new XAttribute("n", min.ToString())
                                , new XAttribute("level", level));
                        section.Add(last);
                    }
                    else
                        break;
                }
                if (min >= max) return;
                int para0 = _v0_GetPara(h.ElementAt(min));
                int type0 = _v0_GetType(h.ElementAt(min));

                if (type0 == 200) return;
                else if (para0 > 0 &&  para0 < paraLevel) return;
                else if (para0 > paraLevel)
                {
                    _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                    if (min >= max) return;
                    if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                }
                else if (para0 == 0 && type0 != 100 && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() != 0)
                {
                    return;
                }
                else if ((min + 2) < max)
                {
                    int para1 = _v0_GetPara(h.ElementAt(min + 1));
                    int type1 = _v0_GetType(h.ElementAt(min + 1));
                    int para2 = _v0_GetPara(h.ElementAt(min + 2));
                    int type2 = _v0_GetType(h.ElementAt(min + 2));


                    if (para0 > paraLevel)
                    {
                        _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                        if (min >= max) return;
                        if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    }
                    else if (para0 == 0 && (para1 > paraLevel || para2 > paraLevel))
                    {
                        _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                        if (min >= max) return;
                        if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                    }
                    else if (_v0_NextParaLevel(h, min, max, level) == paraLevel
                        && section.Elements("section").Last().Ancestors().Where(p => (p.Attribute("htype") == null ? -1 : Convert.ToInt32(p.Attribute("htype").Value)) == type0).Count() == 0)
                    {
                        _v0_ExtractHeader(section.Elements("section").Last(), h, ref min, ref max, paraLevel, level + 1);
                        if (min >= max) return;
                        if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;

                    }
                    else
                        return;

                }
                else
                {
                    if ((min + 1) < max)
                    {
                        int para1 = _v0_GetPara(h.ElementAt(min + 1));
                        int type1 = _v0_GetType(h.ElementAt(min + 1));
                        if (para0 > paraLevel)
                        {
                            _v0_ExtractPara(section.Elements("section").Last(), h, ref min, ref max, paraLevel + 1, level + 1);
                            if (min >= max) return;
                            if (_v0_GetType(h.ElementAt(min)) != _v0_GetType(last)) return;
                        }
                        else
                            return;
                    }
                    else
                        return;
                }
            }
        }

        private int _v0_NextParaLevel(List<XElement> h, int min,int max, int paraLevel)
        {
            int returnValue = 0;
            for (; min < max; min++)
            {
                XElement e = h.ElementAt(min);
                int pLevel = _v0_GetPara(e);
                if (pLevel > 0)
                {
                    returnValue = pLevel;
                    break;
                }
            }
            return returnValue;
        }

        private string _v0_GetText(XElement e)
        {
            return e.Value.Trim();
        }

        
        private string _v0_GetId(XElement e)
        {
            return e.Attribute("hid") == null ? "" : e.Attribute("hid").Value;
        }

        private int _v0_GetPara(XElement e)
        {
            return e.Attribute("hpara") == null ? 0 : Convert.ToInt32( e.Attribute("hpara").Value);
        }

        private int _v0_GetType(XElement e)
        {
            return e.Attribute("htype") == null ? 0 : Convert.ToInt32(e.Attribute("htype").Value);
        }

        private void _v0_GetHeaderType(List<XElement> h)
        {
            bool found = false;
            foreach (XElement e in h)
            {
                found = false;
                string testValue = e.Value.Trim();
                //if (testValue.EndsWith("§ 19-2")) Debug.Print("xxx");
                for (int i = 0; i < 20; i++)
                {
                    string testRegexp = "";
                    int para = 0;
                    switch (i)
                    {
                        case 0:
                            testRegexp = @"^(?<id>(del\s(\s+)?(\d+)))(\s+)?(\.)";
                            break;
                        case 1:
                            testRegexp = @"^(?<id>(del\s(\s+)?([ivx]+)))(\s+)?(\.)";
                            break;
                        case 2:
                            testRegexp = @"^(?<id>(del\s(\s+)?([a-z]+)))(\s+)?(\.)";
                            break;
                        case 3:
                            testRegexp = @"(?<id>(([^\.]+)\sdel))(\s+)?(\.)";
                            break;
                        case 4:
                            testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?(\d)(\d+)?(\s+)?([a-z])?))(\:|\.|\s|$)";
                            break;
                        case 5:
                            testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?[ivx]+))(\:|\.|\s|$)";
                            break;
                        case 6:
                            testRegexp = @"^(?<id>(\d+(\s)?(\.)?(\s)?kapit(t)?(el)?(let)?))(\.|\s|$)";
                            break;
                        case 7:
                            testRegexp = @"^(?<id>((fyrste|første|andre|annet|tredje|fjerde|femte|sjette|sjuande|sjuende|åttende|åttande|niende|niande|tiende|tiande)(\s)?(\.)?(\s)?kapit(t)?el))(\.|\s|$)";
                            break;
                        case 8:
                            testRegexp = @"^(?<id>([ivx]+))(\.|\s|$)";
                            break;
                        case 9:
                            testRegexp = @"^(?<id>([a-h]))(\s+)?(\.)";
                            break;
                        case 10:
                            testRegexp = @"^(?<id>(avdeling(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.)";
                            break;
                        case 11:
                            testRegexp = @"^(?<id>(avdeling(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.)";
                            break;
                        case 12:
                            testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.)";
                            break;
                        case 13:
                            testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.)";
                            break;
                        case 14:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.)";
                            para = 1;
                            break;
                        case 15:
                            testRegexp = @"^(?<id>(art(\s)?(\s+)?(\d+)))(\s)?(\s+)?(\.|$)";
                            para = 1;
                            break;
                        case 16:
                            testRegexp = @"^(?<id>(art(\s)?(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)";
                            para = 1;
                            break;
                        case 17:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)";
                            para = 2;
                            break;
                        case 18:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$)";
                            para = 3;
                            break;
                        case 19:
                            testRegexp = @"(^|^[^\.]+)((?<!\si\s)§+)(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$)";
                            para = 4;
                            break;

                    }

                    string testId = "";
                    Match m = Regex.Match(testValue, testRegexp, RegexOptions.IgnoreCase);
                    if (m.Groups["id"].Value != "")
                    {
                        testId = m.Groups["id"].Value;
                        testId = testId.Replace(".", "");
                        testId = testId.Replace(" ", "_").ToUpper();
                        XAttribute htype = new XAttribute("htype", i);
                        e.Add(htype);
                        if (para > 0)
                        {
                            _v0_SetParaValue(e, testId);
                        }
                        else
                        {
                            XAttribute hid = new XAttribute("hid", testId);
                            e.Add(hid);
                        }
                        XAttribute hpara = new XAttribute("hpara", para);
                        e.Add(hpara);
                        if (para > 0)
                        {
                            if (_minPara > para)
                            {
                                _minPara = para;
                            }
                            if (_maxPara < para)
                            {
                                _maxPara = para;
                            }
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string testRegexp = @"^(merknader|vedlegg\s|[^\.]+\s(vedlegg|\(ef\))\s)";
                    Match m = Regex.Match(testValue, testRegexp, RegexOptions.IgnoreCase);
                    if (m.Value != "")
                    {

                        XAttribute htype = new XAttribute("htype", 200);
                        e.Add(htype);
                        _vedlCounter++;
                        XAttribute hid = new XAttribute("hid", "vedlegg_" + _vedlCounter.ToString());
                        e.Add(hid);
                        XAttribute hpara = new XAttribute("hpara", 0);
                        e.Add(hpara);
                        
                    }
                    else
                    {
                        XAttribute htype = new XAttribute("htype", 100);
                        e.Add(htype);
                        XAttribute hid = new XAttribute("hid", testValue.GetHashCode().ToString());
                        e.Add(hid);
                        XAttribute hpara = new XAttribute("hpara", 0);
                        e.Add(hpara);
                        
                    }
                }
            }
        }
        
        
        private string GetTestRegExpPara(int i)
        {
            string testRegexp = "";
            switch (i)
            {
                case 0:
                    testRegexp = @"(^|[^\.]+)§+(\s)?(?<id>(\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)";
                    break;
                case 1:
                    testRegexp = @"(^|[^\.]+)(?<id>(art(\s)?(\s+)?(\d+)))(\s)?(\s+)?(\.|$)";
                    break;
                case 2:
                    testRegexp = @"(^|[^\.]+)(?<id>(art(\s)?(\s+)?([ivx]+)))(\s)?(\s+)?(\.|$)";
                    break;
                case 3:
                    testRegexp = @"(^|[^\.]+)§+(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?((\s+)?(til|" + (char)0x2013 + @")(\s+)?\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?)?))(\s)?(\s+)?(\.|$)";
                    break;
                case 4:
                    testRegexp = @"(^|[^\.]+)§(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$)";
                    break;
                case 5:
                    testRegexp = @"(^|[^\.]+)§(\s)?(?<id>(\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?\-\d+(\s)?([a-z])?))(\s)?(\s+)?(\.|$)";
                    break;
            }

            return testRegexp;
        }
        private string GetTestRegExpHeader(int i)
        {
            string testRegexp = "";
            switch (i)
            {
                case 0:
                    testRegexp = @"^(?<id>(del\s(\s+)?(\d+)))(\s+)?(\.)";
                    break;
                case 1:
                    testRegexp = @"^(?<id>(del\s(\s+)?([ivx]+)))(\s+)?(\.)";
                    break;
                case 2:
                    testRegexp = @"^(?<id>(del\s(\s+)?([a-z]+)))(\s+)?(\.)";
                    break;
                case 3:
                    testRegexp = @"(?<id>(([^\.]+)\sdel))(\s+)?(\.)";
                    break;
                case 4:
                    testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?(\d)(\d+)?(\s+)?([a-z])?))(\:|\.|\s|$)";
                    break;
                case 5:
                    testRegexp = @"^(?<id>(kap(\.)?(it(t)?el)?(\s)?(\s+)?[ivx]+))(\:|\.|\s|$)";
                    break;
                case 6:
                    testRegexp = @"^(?<id>(\d+((\s)?(ste|net|dje|de|te)?)?(\s)?(\.)?(\s)?(K|k)apit(t)?(el)?(let)?))(\.|\s|$)";
                    break;
                case 7:
                    testRegexp = @"^(?<id>((fyrste|første|andre|annet|tredje|fjerde|femte|sjette|sjuande|sjuende|åttende|åttande|niende|niande|tiende|tiande)(\s)?(\.)?(\s)?kapit(t)?el))(\.|\s|$)";
                    break;
                case 8:
                    testRegexp = @"^(?<id>([ivx]+))(\.|\s|$)";
                    break;
                case 9:
                    testRegexp = @"^(?<id>([a-h]))(\s+)?(\.)";
                    break;
                case 10:
                    testRegexp = @"^(?<id>(avdeling(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.)";
                    break;
                case 11:
                    testRegexp = @"^(?<id>(avdeling(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.)";
                    break;
                case 12:
                    testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?(\d+)))(\s)?(\s+)?(\.)";
                    break;
                case 13:
                    testRegexp = @"^(?<id>(avsnitt(\s)(\s+)?([ivx]+)))(\s)?(\s+)?(\.)";
                    break;
                case 14:
                    testRegexp = @"^(vedlegg\s|[^\.]+\s(vedlegg|\(ef\))\s)";
                    break;
                case 15:
                    testRegexp = @"^[a-zæøåÆØÅ]+(((\s)?(\,)?\s+[a-zæøåÆØÅ]+)+)?";
                    break;
            }

            return testRegexp;
        }

        private XElement GetIndexBranchNoIndex(List<XElement> h, bool AsIs)
        {
            bool found = false;

            int min = 0;
            int level = 1;
            XElement returnValue = null;
            while (!found)
            {
                XElement first = h.ElementAt(min);
                switch (MessageBox.Show(first.Value, "Er dette tittelen på documentet", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        {
                            returnValue = new XElement("section",
                                       new XAttribute("text", first.Value),
                                       new XAttribute("n", 0),
                                       new XAttribute("id", "I00"),
                                       new XAttribute("level", level),
                                       new XAttribute("from", first.Attribute("idx").Value),
                                       first.Attributes());
                            min = min + 1;
                            found = true;
                        }

                        break;
                    case DialogResult.No:
                        returnValue = null;
                        min = min + 1;
                        break;
                    default:
                        returnValue = null;
                        found = true;
                        break;

                }
            }
            return returnValue;
        }


        private XElement GetIndexBranch(List<XElement> h, bool AsIs)
        {
            bool found = false;

            int min = 0;
            int level = 1;
            XElement returnValue = null;
            while (!found)
            {
                XElement first = h.ElementAt(min);
                switch (MessageBox.Show(first.Value, "Er dette tittelen på documentet", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        {
                            returnValue = new XElement("section",
                                       new XAttribute("text", first.Value),
                                       new XAttribute("n", 0),
                                       new XAttribute("id", "I00"),
                                       new XAttribute("level", level),
                                       first.Attributes());
                            min = min + 1;
                            found = true;
                        }

                        break;
                    case DialogResult.No:
                        returnValue = null;
                        min = min + 1;
                        break;
                    default:
                        returnValue = null;
                        found = true;
                        break;

                }
            }
            if (returnValue != null)
            {
                
                //XElement sections = FindLevel(h, min, h.Count() - 1, level + 1);
                //XElement sections = FindIndexLevel(h, min, h.Count() - 1, level + 1, 0, 3);
                XElement sections = new XElement("sections");
                _v0_GetFirstLevel(sections, h, min, h.Count(), level);
                if (sections.HasElements)
                {
                    returnValue.Add(sections.Elements());
                    sections = returnValue;
                    //AddPara(sections, h);

                    foreach (XAttribute sa in sections.Descendants().Where(p=>(p.Attribute("tid") == null ? "" : p.Attribute("tid").Value) != "").Attributes("tid"))
                    {
                        sa.Value = sa.Value.Replace("Ø", "OE");
                        sa.Value = sa.Value.Replace("Æ", "AE");
                        sa.Value = sa.Value.Replace("Å", "AA");
                    }
                    List<string> unik = new List<string>();
                    List<string> NotUnik = new List<string>();
                    found = true;
                    int ansCounter = 1;
                    while (found)
                    {
                        found = false;
                        foreach (XElement s in sections.Descendants().Where(p => p.Ancestors().Count() == ansCounter))
                        {
                            found = true;
                            string tid = s.Attribute("hid").Value;// == null ? s.Attribute("id").Value : s.Attribute("tid").Value;
                            if (tid == "P19-2") Debug.Print("P19-2");
                            string result = unik.Find(
                               delegate(string p)
                               {
                                   return p == tid;
                               });
                            if (result == null)
                            {
                                unik.Add(tid);
                            }
                            else
                            {
                               string resultNot = NotUnik.Find(
                               delegate(string p)
                               {
                                   return p == tid;
                               });
                                if (resultNot == null)
                                {
                                    NotUnik.Add(tid);
                                }
                            }
                        }
                        ansCounter++;
                    }
                    foreach (string u in NotUnik)
                    {
                        unik.Remove(u);
                    }
                    ansCounter = 1;
                    found = true;
                    while (found)
                    {
                        found = false;
                        foreach (XElement s in sections.Descendants().Where(p => p.Ancestors().Count() == ansCounter))
                        {
                            found = true;
                            string tid = s.Attribute("hid").Value;// == null ? s.Attribute("id").Value : s.Attribute("tid").Value;
                            string result = NotUnik.Find(
                               delegate(string p)
                               {
                                   return p == tid;
                               });
                            if (result == null)
                            {
                                unik.Add(tid);
                                if (s.Attribute("id") == null)
                                {
                                    XAttribute id = new XAttribute("id", tid);
                                    s.Add(id);
                                }
                                else
                                {
                                    s.Attribute("id").Value = tid;
                                }
                            }
                            else
                            {

                                string parentId = s.Ancestors("section").Where(p => !TryConvertToInt(p.Attribute("hid").Value)).First().Attribute("hid").Value;//s.Parent.Attribute("hid").Value;// == null ? s.Parent.Attribute("tid").Value : s.Parent.Attribute("id").Value;
                                string newId = parentId + "_" + tid;
                                string resultNot = unik.Find(
                                delegate(string p)
                                {
                                    return p == newId;
                                });
                                if (resultNot == null)
                                {
                                    unik.Add(newId);
                                    if (s.Attribute("id") == null)
                                    {
                                        XAttribute id = new XAttribute("id", newId);
                                        s.Add(id);
                                    }
                                    else
                                    {
                                        s.Attribute("id").Value = newId;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Ikke unik ID!");
                                }
                            }
                        }
                        ansCounter++;
                    }


                    returnValue = sections;

                }
                
            }


            return returnValue;

        }




        private void ExtractMetadata(XElement el, XElement metadata)
        {
            XElement newEl = null;
            string search = "";
            string tagName = "";
            string tagNameSub = "";
            XElement myTable = el;
            XElement tableCell = null;
            
            if (el.Name != "table")
            {
                MessageBox.Show("Finner ikke metadata!"); return;
            }
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == "dato:").Count() == 0)
            {
                MessageBox.Show("Finner ikke start på metadata!"); return;
            }

            //hent id
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == "dato:").Count() != 0)
            {
                newEl = new XElement("id"
                    , el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower() == "dato:").Elements("td").ElementAt(1).Value.Trim());
                metadata.Add(newEl);
            }
            //hent dept
            search = "departement:";
            tagName = "dept";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                string test = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower().Trim() == search)
                    .Elements("td").ElementAt(1).Value.Trim();

                string shortName = Regex.Match(test, @"(?<short>((.+?)))(\()(?<full>((.+?)))(\))", RegexOptions.Multiline | RegexOptions.Singleline).Groups["short"].Value.Trim();
                string fullName = Regex.Match(test, @"(?<short>((.+?)))(\()(?<full>((.+?)))(\))", RegexOptions.Multiline | RegexOptions.Singleline).Groups["full"].Value.Trim();
                if (shortName != "" && fullName != "")
                {
                    newEl = new XElement(tagName
                        , new XElement("shortname", shortName)
                        , new XElement("fullname", fullName));
                }
                else
                    newEl = new XElement(tagName
                        , new XElement("fullname", test));
                    metadata.Add(newEl);
            }

            //hent subdept
            search = "avd/dir:";
            tagName = "sub_dept";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                string test = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower().Trim() == search)
                    .Elements("td").ElementAt(1).Value.Trim();
                if (test != "") newEl = new XElement(tagName, test);
                metadata.Add(newEl);
            }


            //hent publishedin
            search = "publisert:";
            tagName = "publishedin";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                string test = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower().Trim() == search)
                    .Elements("td").ElementAt(1).Value.Trim();
                if (test != "") newEl = new XElement(tagName, test);
                metadata.Add(newEl);
            }

            //hent into_force
            search = "ikrafttredelse:";
            tagName = "into_force";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                string test = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower().Trim() == search)
                    .Elements("td").ElementAt(1).Value.Trim();
                if (test != "") newEl = new XElement(tagName, test);
                metadata.Add(newEl);
            }

            //hent announced
            search = "kunngjort:";
            tagName = "announced";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                string test = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower().Trim() == search)
                    .Elements("td").ElementAt(1).Value.Trim();
                if (Regex.IsMatch(test, @"\d\d?\.\d\d?\.\d\d\d\d", RegexOptions.Multiline | RegexOptions.Singleline))
                    test = Regex.Replace(test, @"(\d+)(\.)(\d+)(\.)(\d+)"
                        ,
                        //"$5-$3-$1"
                        delegate(Match m)
                        {
                            string text = "";
                            text = text + m.Groups[5].Value;
                            text = text + "-";
                            text = text + m.Groups[3].Value;
                            text = text + "-";
                            text = text + m.Groups[1].Value;
                            return text;
                        }
                        , RegexOptions.Multiline | RegexOptions.Singleline);
                if (test != "") newEl = new XElement(tagName, test);
                metadata.Add(newEl);
            }
            //hent valid_for
            search = "gjelder for:";
            tagName = "valid_for";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                string test = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.Trim().ToLower().Trim() == search)
                    .Elements("td").ElementAt(1).Value.Trim();
                if (test != "") newEl = new XElement(tagName, test);
                metadata.Add(newEl);
            }


            //hent last_edit
            search = "sist-endret:";
            tagName = "last_edits";
            tagNameSub = "last_edit";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                newEl = new XElement(tagName);
                tableCell = el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Elements("td").ElementAt(1);
                if (tableCell.Elements("a").Count() != 0)
                {
                    string lastText = "";
                    XElement lastPara = null;
                    XElement lastLaw = null;
                    foreach (XNode n in tableCell.Nodes())
                    {
                        if (n.NodeType == XmlNodeType.Text)
                        {
                            if (lastLaw != null)
                            {
                                lastText = n.ToString().Trim();
                                if (lastText.TrimStart().StartsWith("fra"))
                                {
                                    string date = Regex.Match(lastText, @"\d\d\d\d\-\d\d\-\d\d", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).Value;

                                    XAttribute fromDate = new XAttribute("from_date", date);
                                    lastLaw.Add(fromDate);
                                }

                            }
                            
                        }
                        else if (n.NodeType == XmlNodeType.Element)
                        {
                            XElement eTest = (XElement)n;
                            
                            if (eTest.Name == "a")
                            {
                                string name = eTest.Value.Trim();
                                string id = "";
                                string para = "";
                                if (name.IndexOf("-§") == -1)
                                {
                                    id = name.TrimEnd('-');
                                }
                                else
                                {
                                    id = name.Split('§').ElementAt(0).TrimEnd('-');
                                    para = "P" + name.Split('§').ElementAt(1);
                                }
                                XElement h = null;
                                if (newEl.Elements(tagNameSub).Count() != 0)
                                {
                                    if (newEl.Elements(tagNameSub).Where(p => p.Attribute("id").Value == id).Count() != 0)
                                    {
                                        h = newEl.Elements(tagNameSub).Where(p => p.Attribute("id").Value == id).First();
                                    }
                                }

                                if (h == null)
                                {
                                    
                                    h = new XElement(tagNameSub,
                                        new XAttribute("id", id));
                                    lastLaw = h;
                                    newEl.Add(h);
                                }
                                if (para != "")
                                {
                                    if (h.Elements("para").Where(p => p.Attribute("id").Value == para).Count() == 0)
                                    {
                                        lastPara = new XElement("para"
                                            , new XAttribute("id", para));
                                        h.Add(lastPara);
                                    }
                                }
                                
                            }
                        }

                    }

                }
                if (newEl.HasElements)
                {
                    metadata.Add(newEl);
                }

            }

            //hent effects
            search = "endrer:";
            tagName = "effects";
            tagNameSub = "effect";

            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                newEl = new XElement(tagName);
                tableCell = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search)
                    .Elements("td").ElementAt(1);
                if (tableCell.Elements("a").Count() != 0)
                {
                    string lastText = "";
                    foreach (XNode n in tableCell.Nodes())
                    {
                        if (n.NodeType == XmlNodeType.Text)
                        {
                            lastText = n.ToString().Trim();
                        }
                        else if (n.NodeType == XmlNodeType.Element)
                        {
                            XElement eTest = (XElement)n;
                            if (eTest.Name == "a")
                            {
                                string name = eTest.Value.Trim();
                                string id = "";
                                string para = "";
                                if (name.IndexOf("-§") == -1)
                                {
                                    id = name.TrimEnd('-');
                                }
                                else
                                {
                                    id = name.Split('§').ElementAt(0).TrimEnd('-');
                                    para = "P" + name.Split('§').ElementAt(1);
                                }
                                XElement h = null;
                                if (newEl.Elements(tagNameSub).Count() != 0)
                                {
                                    if (newEl.Elements(tagNameSub).Where(p => p.Attribute("id").Value == id).Count() != 0)
                                    {
                                        h = newEl.Elements(tagNameSub).Where(p => p.Attribute("id").Value == id).First();
                                    }
                                }

                                if (h == null)
                                {
                                    h = new XElement(tagNameSub,
                                        new XAttribute("id", id));
                                    newEl.Add(h);
                                }
                                if (para != "")
                                {
                                    if (h.Elements("para").Where(p => p.Attribute("id").Value == para).Count() == 0)
                                        h.Add(new XElement("para"
                                            , new XAttribute("id", para)));
                                }
                            }
                        }

                    }

                }
                if (newEl.HasElements)
                {
                    metadata.Add(newEl);
                }

            }


            //hent warrants
            search = "hjemmel:";
            tagName = "warrants";
            tagNameSub = "warrant";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                newEl = new XElement(tagName);
                tableCell = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search)
                    .Elements("td").ElementAt(1);
                if (tableCell.Elements("a").Count() != 0)
                {
                    string lastText = "";
                    foreach (XNode n in tableCell.Nodes())
                    {
                        if (n.NodeType == XmlNodeType.Text)
                        {
                            lastText = n.ToString().Trim();
                        }
                        else if (n.NodeType == XmlNodeType.Element)
                        {
                            XElement eTest = (XElement)n;
                            if (eTest.Name == "a")
                            {
                                string name = eTest.Value.Trim();
                                string id = "";
                                string para = "";
                                if (name.IndexOf("-§") == -1)
                                {
                                    id = name.TrimEnd('-');
                                }
                                else
                                {
                                    id = name.Split('§').ElementAt(0).TrimEnd('-'); 
                                    para = "P" + name.Split('§').ElementAt(1);
                                }
                                XElement h = null;
                                if (newEl.Elements(tagNameSub).Count() != 0)
                                {
                                    if (newEl.Elements(tagNameSub).Where(p => p.Attribute("id").Value == id).Count() != 0)
                                    {
                                        h = newEl.Elements(tagNameSub).Where(p => p.Attribute("id").Value == id).First();
                                    }
                                }

                                if (h == null)
                                {
                                    h = new XElement(tagNameSub, 
                                        new XAttribute("id", id));
                                    newEl.Add(h);
                                }
                                if (para != "")
                                {
                                    if (h.Elements("para").Where(p => p.Attribute("id").Value == para).Count() == 0)
                                        h.Add(new XElement("para"
                                            , new XAttribute("id", para)));
                                }
                            }
                        }

                    }

                }
                if (newEl.HasElements)
                {
                    metadata.Add(newEl);
                }

            }
            //hent names
            search = "korttittel:";
            tagName = "names";
            tagNameSub = "name";
            if (el.Elements("tr").Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search).Count() != 0)
            {
                if (metadata.Elements(tagName).Count() == 0)
                {
                    newEl = new XElement(tagName);
                    metadata.Add(newEl);
                }
                else
                {
                    newEl = metadata.Elements(tagName).First();
                }

                tableCell = el.Elements("tr")
                    .Where(p => p.Elements("td").ElementAt(0).Value.ToLower().Trim() == search)
                    .Elements("td").ElementAt(1);
                string test = tableCell.Value;
                
                char splitChar = '-';
                if (test.IndexOf((char)0x2013) != -1)
                {
                    splitChar = (char)0x2013;
                }
                else if (test.IndexOf((char)0x2014) != -1)
                {
                    splitChar = (char)0x2014;
                }

                else if (test.IndexOf('-') == -1)
                    splitChar = ',';

                foreach (string s in test.Split(splitChar))
                {
                    if (newEl.Elements(tagNameSub).Where(p => p.Value == s.ToLower().Trim()).Count() == 0)
                    {
                        XElement sub = new XElement(tagNameSub, s.ToLower().Trim());
                        newEl.Add(sub);
                    }
                }
            }
        }

   
        private XmlDocument LoadDocumentList(ref XmlElement root)
        {
            XmlDocument documentlist = new XmlDocument(); ;
            XmlElement xroot = null;
            if (File.Exists(xmlPath))
            {
                documentlist.Load(xmlPath);
                root = (XmlElement)documentlist.SelectSingleNode("documents");
            }
            else
            {
                root = documentlist.CreateElement("documents");
                documentlist.AppendChild(root);
            }
            return documentlist;
        }
            

        private void GetIndexFromDocument(XElement first, XElement root)
        {

            XElement content = new XElement("content");
            foreach (XElement e in first.Elements("document").Elements("section"))
            {
                XElement item = new XElement("item",
                                    new XAttribute("id", e.Attribute("id").Value),
                                    new XAttribute("text", e.Element("title").Value),
                                    new XAttribute("pid", e.Attribute("id").Value));

                GetIndexItems(e, item);
                content.Add(item);

            }
            root.Add(content);
        }


        private void GetIndexItems(XElement el, XElement items)
        {

            foreach (XElement e in el.Elements("section"))
            {
                XElement item = new XElement("item",
                                    new XAttribute("id", e.Attribute("id").Value),
                                    new XAttribute("text", e.Element("title").Value),
                                    new XAttribute("pid", e.Attribute("id").Value));

                GetIndexItems(e, item);
                items.Add(item);

            }
        }

        private bool TryConvertToInt(string s)
        {
            
            int j;
            bool returnVal = Int32.TryParse(s, out j);
            
            return returnVal;
        }
        private void GetBookmarksFromDocument(XElement first, XElement metadata)
        {
            XElement bookmarks = new XElement("bookmarks");
            int i = 1;
            foreach (XElement e in first.Descendants("section").Where(p => !TryConvertToInt(p.Attribute("id").Value)))
            {
                XElement bookmark = new XElement("bookmark",
                                    new XAttribute("key", e.Attribute("id").Value),
                                    new XAttribute("title", e.Element("title").Value),
                                    new XAttribute("idx", i.ToString()));
                bookmarks.Add(bookmark);

                i++;
            }
            metadata.Add(bookmarks);
        }



        private  XElement AddLedd(XElement addTo
                        , string idx
                        , string id
                        , int leddNo
                        , int bytoken)
        {
            XElement ledd = new XElement("ledd",
                new XAttribute("bytoken", bytoken),
                new XAttribute("level", "5"),
                new XAttribute("type", "ledd"),
                new XAttribute("idx", idx),
                new XAttribute("value", leddNo),
                new XAttribute("id", addTo.Attribute("id").Value + "_L" + leddNo));
            addTo.Add(ledd);
            return ledd;
        }


        private static string TrimElementText(string text)
        {
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        private string FindEnd(XDocument topElement)
        {
            string returnValue = "";
            //XElement idx = topElement.XPathSelectElement("descendant::*[contains(text(), 'Databasen sist oppdatert')]/..");

            XElement idx = null;
            if (topElement.DescendantNodes().Where(p => TrimElementText(p.ToString()).IndexOf("Databasen sist oppdatert") > 0).Count() > 0)
                idx = topElement.DescendantNodes().Where(p => TrimElementText(p.ToString()).IndexOf("Databasen sist oppdatert") > 0).Ancestors("p").First();

            if (idx != null)
            {
                returnValue = idx.Attribute("idx").Value.ToString();
            }
            else
            {
                var query = from p in topElement.Descendants().Attributes("idx")
                            select Convert.ToInt32(p.Value);
                returnValue = query.Max().ToString();
            }
            return returnValue;
        }

        private void SaveWebBrowserDocument(WebBrowser myBrowser, string fileName, string encoding)
        {

            string returnHTML = "";
            if (myBrowser.Document != null)
            {
                Stream documentStream = myWebBrowser.DocumentStream;
                if (documentStream != null)
                {
                    StreamReader reader = new StreamReader(documentStream, Encoding.GetEncoding(encoding));
                    documentStream.Position = 0;
                    returnHTML = reader.ReadToEnd();

                    StringBuilder outputHtml = new StringBuilder();
                    outputHtml.Append(returnHTML);

                    File.WriteAllText(fileName, outputHtml.ToString(), Encoding.GetEncoding(encoding));
                }
            }

        }

       private string ReadStringFromFile(string filName)
        {
            string returnValue = "";
            if (File.Exists(filName))
            {
                FileStream file = new FileStream(filName, FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader sr = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1"));
                returnValue = sr.ReadToEnd();
                sr.Close();
                file.Close();
            }
            return returnValue;
        }

       public static string HEXREPL(Match m)
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

       private List<string> _HexAndHtmlChar;

       private void GetHexAndHtmlChar(string html)
       {
           MatchCollection mc = Regex.Matches(html, @"\&[\#0-9a-z]+\;((\&[\#0-9a-z]+\;)+)?", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
           foreach (Match m in mc)
           {

               switch (m.Value)
               {
                   case "&#160;":
                   case "&#167":
                   case "&#171;":
                   case "&#173;":
                   case "&#187;":
                   case "&#225;":
                   case "&#228;":
                   case "&#232;":
                   case "&#233;":
                   case "&#242;":
                   case "&#253;":
                   case "&#8211;":
                   case "&#8217;":
                   case "&amp;":
                       break;
                   default:
                       {
                           string result = _HexAndHtmlChar.Find(
                           delegate(string p)
                           {
                               return p == m.Value;
                           });
                           if (result == null)
                           {
                               _HexAndHtmlChar.Add(m.Value);
                           }

                       }
                       break;
               }
           }
       }


       private string CleanUpInputHtml(string html)
       {
           string returnValue = "";
           try
           {
               html = Regex.Replace(html, @"(\<\!DOCTYPE)((.)+?)(\>)", "", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&shy\;", "-");
               html = Regex.Replace(html, @"\&nbsp\;", " ");

               html = Regex.Replace(html, @"\r\n", "");
               html = Regex.Replace(html, @"(\sxmlns\=\"")((.)+?)(\"")", "");
               html = Regex.Replace(html, @"(\sxml:lang\=\"")((.)+?)(\"")", "");
               html = Regex.Replace(html, @"(\slang\=\"")((.)+?)(\"")", "");

               Regex regit = new Regex("([^\\x00-\\x7E])");
               html = regit.Replace(html, new MatchEvaluator(HEXREPL), -1);



               html = Regex.Replace(html, @"\&\#160\;", " ", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#167\;", "§", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#171; &laquo; <<
               html = Regex.Replace(html, @"\&laquo\;", "&#171;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK soft hypen (-) &#173; &shy;
               html = Regex.Replace(html, @"\&shy\;", "&#173;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#187; &raquo; >>
               html = Regex.Replace(html, @"\&raquo\;", "&#187;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#197\;", "Å", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#216\;", "Ø", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#225;&aacute;
               html = Regex.Replace(html, @"\&aacute\;", "&#225;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#228;&auml;
               html = Regex.Replace(html, @"\&auml\;", "&#228;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#229\;", "å", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#230\;", "æ", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#232;&egrave;
               html = Regex.Replace(html, @"\&egrave\;", "&#232;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#233;&eacute;
               html = Regex.Replace(html, @"\&eacute\;", "&#233;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#242; &ograve;
               html = Regex.Replace(html, @"\&ograve\;", "&#242;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#248\;", "ø", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

               //OK &#253; &yacute;
               html = Regex.Replace(html, @"\&\#253\;\&\#253\;", "ø", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&yacute\;", "&#253;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               html = Regex.Replace(html, @"\&\#269\;", " ", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#8211; &ndash;
               html = Regex.Replace(html, @"\&ndash\;", "&#8211;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //OK &#8217;&rsquo;
               html = Regex.Replace(html, @"\&rsquo\;", "&#8217;", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
               //Ok&amp;

               html = Regex.Replace(html, @"\&agrave\;", @"&#192;");

               html = Regex.Replace(html, @"\&AElig\;", @"Æ");
               html = Regex.Replace(html, @"\&aelig\;", @"æ");
               html = Regex.Replace(html, @"\&Oslash\;", @"Ø");
               html = Regex.Replace(html, @"\&oslash\;", @"ø");
               html = Regex.Replace(html, @"\&Aring\;", @"Å");
               html = Regex.Replace(html, @"\&aring\;", @"å");
               html = Regex.Replace(html, @"\&ograve\;", @"o");
               html = Regex.Replace(html, @"\&Ograve\;", @"O");
               html = Regex.Replace(html, @"\&egrave\;", @"e");
               html = Regex.Replace(html, @"\&Egrave\;", @"e");
               html = Regex.Replace(html, @"\&ocirc\;", @"&#244;");
               html = Regex.Replace(html, @"\&0circ\;", @"0");

               //&alpha;
               html = Regex.Replace(html, @"\&alpha\;", @"a");
               html = Regex.Replace(html, @"\&Alpha\;", @"A");
               //&ge;&#8805;
               html = Regex.Replace(html, @"\&ge\;", @"&#x2265;");
               //&gt;
               html = Regex.Replace(html, @"\&gt\;", @"&gt;");
               //&le;
               html = Regex.Replace(html, @"\&le\;", @"&#x2264;");
               //&lt;
               html = Regex.Replace(html, @"\&lt\;", @"&lt;");
               //&omega;
               html = Regex.Replace(html, @"\&omega\;", @"&#x3C9;");
               //&part;&#x2202;
               html = Regex.Replace(html, @"\&part\;", @"&#x2202;");
               //&radic;
               html = Regex.Replace(html, @"\&radic\;", @"&#x221A;");
               //&Sigma;
               html = Regex.Replace(html, @"\&Sigma\;", @"&#x3A3;");
               //&tau;&#x3C4;
               html = Regex.Replace(html, @"\&tau\;", @"&#x3C4;");
               //&oacute;
               html = Regex.Replace(html, @"\&oacute\;", @"&#xF3;");
               html = Regex.Replace(html, @"\&Oacute\;", @"&#xD3;");
                //&ouml;
               html = Regex.Replace(html, @"\&ouml\;", @"&#xF6;");
               html = Regex.Replace(html, @"\&Ouml\;", @"&#xD6;");
               html = Regex.Replace(html, @"\&frac14\;", @"&#xBC;");
               html = Regex.Replace(html, @"\&frac12\;", @"&#xBD;");
               html = Regex.Replace(html, @"\&frac24\;", @"&#xBE;");
               html = Regex.Replace(html, @"\&uuml\;", @"&#xDC;");

               html = Regex.Replace(html, @"\&deg\;", @"&#xB0;");
               html = Regex.Replace(html, @"\&iuml\;", @"&#xCF;");
               html = Regex.Replace(html, @"\&#198\;", @"&#xC6;");
               html = Regex.Replace(html, @"\&#244\;", @"&#xF4;");

               GetHexAndHtmlChar(html);

               html = TrimElementText(html);

               returnValue = html;
           }
           catch
           {
               MessageBox.Show("En feil oppstod i CleanUpInputHtml()");
           }
           return returnValue;
       }


        private void CleanXHTMLText(ref string s)
        {
            s = Regex.Replace(s, @"(\<\!DOCTYPE)((.)+?)(\>)", "", RegexOptions.Multiline | RegexOptions.Singleline);
            //s = Regex.Replace(s, @"\&shy\;", "-");
            //s = Regex.Replace(s, @"\&nbsp\;", " ");
            s = Regex.Replace(s, @"\&eacute\;", @"&#233;");
            s = Regex.Replace(s, @"\&laquo\;", @"&#171;");
            s = Regex.Replace(s, @"\&raquo\;", @"&#187;");

            s = Regex.Replace(s, @"\&shy\;", " ");
            s = Regex.Replace(s, @"\&nbsp\;", " ");
            //s = Regex.Replace(s, @"\&eacute\;", @" ");
            //s = Regex.Replace(s, @"\&laquo\;", @" ");
            //s = Regex.Replace(s, @"\&raquo\;", @" ");
            s = Regex.Replace(s, @"\&agrave\;", @"#192;");

             s = Regex.Replace(s, @"\&\#C6\;", @"Æ");
             s = Regex.Replace(s, @"\&AElig\;", @"Æ");
             s = Regex.Replace(s, @"\&aelig\;", @"æ");
             s = Regex.Replace(s, @"\&Oslash\;", @"Ø");
             s = Regex.Replace(s, @"\&oslash\;", @"ø");
             s = Regex.Replace(s, @"\&Aring\;", @"Å");
             s = Regex.Replace(s, @"\&aring\;", @"å");
             s = Regex.Replace(s, @"\&ograve\;", @"o");
             s = Regex.Replace(s, @"\&Ograve\;", @"O");
             s = Regex.Replace(s, @"\&egrave\;", @"e");
             s = Regex.Replace(s, @"\&Egrave\;", @"e");
             s = Regex.Replace(s, @"\&ocirc\;", @"&#244;");
             s = Regex.Replace(s, @"\&0circ\;", @"0");
             s = Regex.Replace(s, @"\&mdash\;", @"-");
             s = Regex.Replace(s, @"\&\#152\;", @"-");
             s = Regex.Replace(s, @"\&ndash\;", @"-");
             s = Regex.Replace(s, @"\&\#151\;", @"-");
             s = Regex.Replace(s, @"\&\#8211\;", @"-");
             
            
            
            s = Regex.Replace(s, @"\r\n", "");
            s = Regex.Replace(s, @"(\sxmlns\=\"")((.)+?)(\"")", "");

            MatchCollection mc = Regex.Matches(s, @"(\d)(\s)\&ndash\;(\s)(\d)");
            foreach (Match m in mc)
            {
                foreach (char v in m.Value)
                {
                    Debug.Print(((int)v).ToString());
                }
                Debug.Print(m.Value);
            }

            
            mc = Regex.Matches(s, @"\&[\#a-zA-z]+\;");
            foreach (Match m in mc)
            {
                Debug.Print(m.Value);
            }
        }

        private XDocument CleanXHTMLfile(string inputFile, string outputfile)
        {
            XDocument topDoc = new XDocument();
            try
            {
                string stringXHTML = "";
                while (stringXHTML == "")
                {
                    Application.DoEvents();
                    stringXHTML = ReadStringFromFile(inputFile);
                }

                //CleanXHTMLText(ref stringXHTML);
                stringXHTML = CleanUpInputHtml(stringXHTML);

                topDoc = XDocument.Parse(stringXHTML);
                topDoc.Save(outputfile);
                return topDoc;
            }
            catch
            {
                return null;
            }
        }

        private void textAdress_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (doNotselect) return;
            doNotAfterNavigate = false;
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString();
                if (File.Exists(xmlPath))
                {
                    XDocument d = XDocument.Load(xmlPath);
                    if (d != null)
                    {
                        XElement document = d.Descendants("document").Where(p=>p.Element("name").Value == s).First();
                        if (document != null)
                        {
                           
                            string xadress = document.Element("adress").Value;
                            textAdress.Text = document.Element("name").Value.Trim();
                            if (xadress != "")
                            {
                                adress.Text = xadress;
                                myWebBrowser.Navigate(adress.Text);
                            }
                            else
                            {
                                adress.Text = @"www.lovdata.no";
                                myWebBrowser.Navigate(adress.Text);
                                return;
                            }
                        }
                    }
                }

            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            myWebBrowser.Document.MouseOver += new HtmlElementEventHandler(document_MouseOver);
            myWebBrowser.Document.MouseLeave += new HtmlElementEventHandler(document_MouseLeave);
            myWebBrowser.Document.MouseDown += new HtmlElementEventHandler(document_MouseDown);
            myWebBrowser.Document.ContextMenuShowing += new HtmlElementEventHandler(WebContextMenuShowing);
        }

        private void WebContextMenuShowing(object sender, HtmlElementEventArgs e)
        {
            e.ReturnValue = false;
        }

        
        private void document_MouseOver(object sender, HtmlElementEventArgs e)
        {
            //HtmlElement element = e.ToElement;
            
            //if (!this.elementStyles.ContainsKey(element))
            //{
            //    string style = element.Style;
            //    this.elementStyles.Add(element, style);
            //    element.Style = style + "; background-color: #ffc; border:solid 1px black;";
            //    this.Text = element.GetAttribute("className").ToString() ?? "(no id)";
            //}
        }

        private void document_MouseLeave(object sender, HtmlElementEventArgs e)
        {
            HtmlElement element = e.FromElement;
            if (this.elementStyles.ContainsKey(element))
            {
                string style = this.elementStyles[element];
                this.elementStyles.Remove(element);
                element.Style = style;

            }
        }


        private void document_MouseDown(object sender, HtmlElementEventArgs e)
        {
            Point MPoint = new Point(e.MousePosition.X, e.MousePosition.Y);
            HtmlDocument CurrentDocument = myWebBrowser.Document;
            HtmlElement element = CurrentDocument.GetElementFromPoint(MPoint);


            if (!this.elementStyles.ContainsKey(element))
            {
                string style = element.Style;
                this.elementStyles.Add(element, style);
                element.Style = style + "; background-color: #ffc; border:solid 1px black;";
                this.Text = element.GetAttribute("className").ToString() ?? "(no id)";
            }

            if (e.MouseButtonsPressed == System.Windows.Forms.MouseButtons.Right)
            {
                int x = element.OffsetRectangle.Left + e.MousePosition.X;
                int y = element.OffsetRectangle.Top + e.MousePosition.Y;
                Point p = e.OffsetMousePosition;
                myWebBrowser.ContextMenu.Show(this, new Point(x,y));
                Debug.Print(element.InnerHtml.ToString());
            }


        }


        private class topic
        {
            public string topic_id { get; set; }
            public string name { get; set; }
            public XElement xml { get; set; }
        }

        private List<topic> GetTopicByDateNrAlt(string search)
        {
            List<topic> tl = new List<topic>();

            commxml cx = new commxml();
            string sourceRegexp = cx.Execute("diba0706.dbo._ImpClientGetDocumentList");
            XDocument d = XDocument.Parse(sourceRegexp);
            XElement eT = new XElement((XElement)d.Descendants("root").First().Elements("root").First());
            XDocument toImport = XDocument.Parse(eT.ToString());

            //XDocument toImport = XDocument.Load(_documentImportFile);


            search = Regex.Replace(search
                    , @"(.+?)(?<year>((\d\d\d\d)))(\-)(0+)?(?<m>((\d+)))(\-)(0+)?(?<d>((\d+)))"
                    , delegate(Match m)
                    {
                        string returnValue = "";
                        returnValue = returnValue + m.Groups["year"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["m"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["d"].Value;
                        return returnValue;
                    }
                    , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);




            foreach (XElement e in toImport.Root.Elements("document")
                .Where(p => Regex.Replace((p.Element("name_ext") == null ? "" : p.Element("name_ext").Value.Trim())
                    , @"(?<year>((\d\d\d\d)))(\-)(0+)?(?<m>((\d+)))(\-)(0+)?(?<d>((\d+)))"
                    , delegate(Match m)
                    {
                        string returnValue = "";
                        returnValue = returnValue + m.Groups["year"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["m"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["d"].Value;
                        return returnValue;
                    }
                    , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).ToString() ==

                    search
                    ||
                    Regex.Replace((p.Element("name") == null ? "" : p.Element("name").Value.Trim())
                    , @"(?<year>((\d\d\d\d)))(\-)(0+)?(?<m>((\d+)))(\-)(0+)?(?<d>((\d+)))"
                    , delegate(Match m)
                    {
                        string returnValue = "";
                        returnValue = returnValue + m.Groups["year"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["m"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["d"].Value;
                        return returnValue;
                    }
                    , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).ToString() ==

                    search
                    ))
            {
                topic t = new topic();
                t.name = e.Element("name").Value;
                t.topic_id = e.Element("topic_id").Value;
                t.xml = e;
                tl.Add(t);
            }

            return tl;
        }

        private List<topic> GetTopicByDateNr(string search)
        {
            List<topic> tl = new List<topic>();

            commxml cx = new commxml();
            string sourceRegexp = cx.Execute("diba0706.dbo._ImpClientGetDocumentList");
            XDocument d = XDocument.Parse(sourceRegexp);
            XElement eT = new XElement((XElement)d.Descendants("root").First().Elements("root").First());
            XDocument toImport = XDocument.Parse(eT.ToString());
            //XDocument toImport = XDocument.Load(_documentImportFile);


            search = Regex.Replace(search
                    , @"(.+?)(?<year>((\d\d\d\d)))(\-)(0+)?(?<m>((\d+)))(\-)(0+)?(?<d>((\d+)))(\-)(0+)?(?<nr>((\d+)))"
                    , delegate(Match m)
                    {
                        string returnValue = "";
                        returnValue = returnValue + m.Groups["year"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["m"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["d"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["nr"].Value;
                        return returnValue;
                    }
                    , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);




            foreach (XElement e in toImport.Root.Elements("document")
                .Where(p => Regex.Replace((p.Element("name_ext") == null ? "" : p.Element("name_ext").Value.Trim())
                    , @"(?<year>((\d\d\d\d)))(\-)(0+)?(?<m>((\d+)))(\-)(0+)?(?<d>((\d+)))(\s+nr(\.)?)?(\s+)?(\-)?(0+)?(?<nr>((\d+)))"
                    , delegate(Match m)
                    {
                        string returnValue = "";
                        returnValue = returnValue + m.Groups["year"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["m"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["d"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["nr"].Value; 
                        return returnValue;
                    }
                    , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).ToString() ==

                    search
                    ||
                    Regex.Replace((p.Element("name") == null ? "" : p.Element("name").Value.Trim())
                    , @"(?<year>((\d\d\d\d)))(\-)(0+)?(?<m>((\d+)))(\-)(0+)?(?<d>((\d+)))(\s+nr(\.)?)?(\s+)?(\-)?(0+)?(?<nr>((\d+)))"
                    , delegate(Match m)
                    {
                        string returnValue = "";
                        returnValue = returnValue + m.Groups["year"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["m"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["d"].Value;
                        returnValue = returnValue + "-";
                        returnValue = returnValue + m.Groups["nr"].Value;
                        return returnValue;
                    }
                    , RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase).ToString() ==

                    search
                    ))
            {
                topic t = new topic();
                t.name = e.Element("name").Value;
                t.topic_id = e.Element("topic_id").Value;
                t.xml = e;
                tl.Add(t);
            }

            return tl;
        }

        private XElement GetTopicByTopicId(string topic_id)
        {
            XElement returnValue = new XElement("bookmarks");

            commxml cx = new commxml();
            string sourceRegexp = cx.Execute("diba0706.dbo._ImpClientGetMetadata '" + topic_id + "'");
            XDocument d = XDocument.Parse(sourceRegexp);
            XElement eT = new XElement((XElement)d.Descendants("root").First().Elements("root").First());
            XDocument toImport = XDocument.Parse(eT.ToString());

            //XDocument toImport = XDocument.Load(_documentImportFile);

            if (toImport.Root.Elements("document")
                        .Where(p => p.Element("topic_id").Value == topic_id).Count() != 0)
            {

                returnValue = toImport.Root.Elements("document")
                            .Where(p => p.Element("topic_id").Value == topic_id).First().Element("dk_metaxml").Element("metadata").Element("bookmarks");

            }
            return returnValue;
        }
        private void SetElementId(ref XElement el)
        {
            int i = 0;
            foreach (XElement subEl in el.Descendants())
            {
                i++;
                subEl.Add(new XAttribute("idx", string.Format("{0}", i)));
            }

        }
        private void btnLoadDocument_Click(object sender, EventArgs e)
        {
            string _folder = "";
            
            XElement importD = null;
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString().Trim();
                if (File.Exists(xmlPath))
                {
                    importD = GetImportDocument(s);
                    if (importD != null)
                    {
                        string xadress = importD.Element("adress").Value;
                        if (xadress != "")
                        {
                            adress.Text = xadress;
                            myWebBrowser.Navigate(adress.Text);
                        }
                    }

                }

            }

            if (myWebBrowser == null) return;

            if (myWebBrowser.Document == null) return;

            foreach (HtmlElement f in myWebBrowser.Document.GetElementsByTagName("A"))
            {
                if (f.InnerText != null)
                {
                    if (f.InnerText.Trim().ToLower() == "hele forskriften" || f.InnerText.Trim().ToLower() == "hele loven")
                    {
                        MessageBox.Show("Trykk 'Hele forskriften/loven' for å importere!");
                        return;
                    }
                }
            }


            _HexAndHtmlChar = new List<string>();

            string url = myWebBrowser.Url.ToString();

            string charset = "ISO-8859-1";


            string tempFolder = _documentLibry + @"temp\";
            if (Directory.Exists(tempFolder))  Directory.Delete(tempFolder, true);

            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            
            string htmlFile = tempFolder + "temp.html";
            SaveWebBrowserDocument(myWebBrowser, htmlFile, charset);

            if (!File.Exists(htmlFile)) return;

            string xhtmlFile = tempFolder + "temp.xhtml";

            ConvertToXHTML conv = new ConvertToXHTML();
            if (!conv.ConvertDocumentToXHTML(htmlFile, xhtmlFile, charset, charset))
            {
                MessageBox.Show("Ikke konvertert!");
                return;
            }

            while (!TestFile(xhtmlFile));

            string cleanFile = tempFolder + "clean.xml";

            XDocument cleanXML = CleanXHTMLfile(xhtmlFile, cleanFile);
            if (cleanXML == null)
            {
                foreach (string s in _HexAndHtmlChar.OrderBy(p => p))
                    Debug.Print(s);


                MessageBox.Show("Kunne ikke konvertere filen!");
                return;
            }

            foreach (string s in _HexAndHtmlChar.OrderBy(p => p))
                Debug.Print(s);


            if (cleanXML.Descendants("td").Nodes().Where(p => p.NodeType == XmlNodeType.Text).Count() != 0)
            {
                int ant = cleanXML.Descendants("td").Nodes().Where(p => p.NodeType == XmlNodeType.Text).Count();
                for (int i = ant - 1; i >= 0; i--)
                {
                    XNode x = cleanXML.Descendants("td").Nodes().Where(p => p.NodeType == XmlNodeType.Text).ElementAt(i);

                    XElement xp = new XElement("t", x.ToString());
                    x.ReplaceWith(xp);
                }
            }

            XElement metadata = new XElement("metadata");

            XElement topElement = cleanXML.XPathSelectElement("html/body/table[1]/tr[1]/td[2]/table[1]/tr[2]/td[1]");


            if (topElement == null) return;
            string title = "";
            string lovTitle = "";
            string documentName = "";
            string clean1File = tempFolder + "clean1.xml";
            XDocument xd = new XDocument();
            XElement html = new XElement("html");
            
            html.Add(topElement.Elements());
            xd.Add(html);

            if (topElement != null && xd.XPathSelectElement("html/table[1]/tr[1]/td[1]/font[1]/b[1]") != null)
            {
                
                title = xd.XPathSelectElement("html/table[1]/tr[1]/td[1]/font[1]/b[1]").Value.ToString();
                if (topElement.Elements("table").Count() > 0)
                {
                    lovTitle = TrimElementText(topElement.Elements("table").ElementAt(0).Element("tr").Element("td").Value);
                }
                topElement.Elements("span").Remove();
                topElement.Elements("hr").Remove();
                
                SetElementId(ref html);
               
                xd.Save(clean1File);

            }
            else if (topElement != null && xd.XPathSelectElement("html/table[1]/tr[1]/td[1]/font[1]/b[1]") == null)
            {
                if (topElement.Elements("h2").Count() != 0)
                {
                    title = topElement.Elements("h2").First().Value;
                }
                lovTitle = title;
                topElement.Elements("span").Remove();
                topElement.Elements("hr").Remove();
               
                SetElementId(ref html);
                
                xd.Save(clean1File);

            }

            if (topElement != null)
            {



                if (topElement.Elements("table").Count() > 0)
                {
                    

                    if (lovTitle.Split(':').Count() == 2)
                    {
                        documentName = TrimElementText(lovTitle.Split(':').ElementAt(0));

                        if (lovTitle.Split(':').Count() != 2)
                        {
                            MessageBox.Show("Trykk 'Hele forskriften' for å importere!");
                            return;
                        }

                        lovTitle = lovTitle.Split(':').ElementAt(1);

                        lovTitle = lovTitle.Replace(".", "");
                        lovTitle = lovTitle.Replace("[", "(");
                        lovTitle = lovTitle.Replace("]", ")");
                        lovTitle = lovTitle.Trim();

                        string shortName = Regex.Match(lovTitle, @"(?<short>((.+?)))(\()(?<full>((.+?)))(\))", RegexOptions.Multiline | RegexOptions.Singleline).Groups["short"].Value.Trim();
                        string fullName = Regex.Match(lovTitle, @"(?<short>((.+?)))(\()(?<full>((.+?)))(\))", RegexOptions.Multiline | RegexOptions.Singleline).Groups["full"].Value.Trim();

                        shortName = shortName.Trim();
                        fullName = fullName.Trim();


                        XElement elTitle = new XElement("doc_name", shortName == "" ? lovTitle : shortName);
                        metadata.Add(elTitle);

                        if (fullName != "")
                        {
                            XElement names = new XElement("names"
                                , new XElement("name", fullName.ToLower()));
                            metadata.Add(names);
                        }
                        ExtractMetadata(topElement.Elements("table").ElementAt(1), metadata);
                    }
                    else
                    {
                        documentName = TrimElementText(lovTitle);
                        lovTitle = documentName.Trim();
                        XElement elTitle = new XElement("doc_name", lovTitle);
                        metadata.Add(elTitle);
                        ExtractMetadata(topElement.Elements("table").ElementAt(1), metadata);
                    }


                    _folder = metadata.Element("id").Value;

                    string search = _folder.ToString();


                    List<topic> tl = GetTopicByDateNr(search);

                    if (tl.Count() == 0)
                    {
                        tl = GetTopicByDateNrAlt(search);
                    }
                    List<string> topics = new List<string>();
                    foreach (topic t in tl)
                        topics.Add(t.name);

                    Form frm = new frmSelectTopic(topics);
                    frm.ShowDialog();

                    string topic_id = "";
                    if (frm.DialogResult == DialogResult.OK)
                    {
                        if (frm.Tag != null)
                            topic_id = tl.Where(p => p.name == frm.Tag.ToString()).First().topic_id;
                    }
                    else return;



                    XElement document = GetImportDocument(lovTitle);
                    
                    
                    if (document == null)
                    {
                        if (MessageBox.Show("Vil du importere dokumentet '" + lovTitle + "'", "Importere", MessageBoxButtons.OKCancel) != DialogResult.Cancel)
                        {
                            if (!Directory.Exists(_documentLibry + _folder + @"\")) Directory.CreateDirectory(_documentLibry + _folder + @"\");
                            if (File.Exists(_documentLibry + _folder + @"\clean1.xml")) File.Delete(_documentLibry + _folder + @"\clean1.xml");
                            if (File.Exists(_documentLibry + _folder + @"\metadata.xml")) File.Delete(_documentLibry + _folder + @"\metadata.xml");
                            File.Move(clean1File, _documentLibry + _folder + @"\clean1.xml");


                            document = new XElement("document");
                            document.Add(new XElement("name", lovTitle));
                            document.Add(new XElement("adress", adress.Text));
                            document.Add(new XElement("folder", _folder));
                            document.Add(new XElement("file", _documentLibry + _folder + @"\clean1.xml"));
                            document.Add(new XElement("topic_id", topic_id));
                            XmlDocument impD  = new XmlDocument();
                            impD.Load(_documentLibry + "import.xml");
                            XDocument updateD = XDocument.Load(impD.DocumentElement.CreateNavigator().ReadSubtree());

                            updateD.Root.Add(document);
                            updateD.Save(_documentLibry + "import.xml");

                            XDocument meta = new XDocument();
                            meta.Add(metadata);
                            meta.Save(_documentLibry + _folder + @"\metadata.xml");
                            LoadComboBox();
                            textAdress.Text = lovTitle;
                        }

                    }
                    else
                    {
                        if (MessageBox.Show("'" + lovTitle + "' er allerede lastet ned, vil du oppdaterer!", "Importere", MessageBoxButtons.OKCancel) != DialogResult.Cancel)
                        
                        {
                            if (!Directory.Exists(_documentLibry + _folder + @"\")) Directory.CreateDirectory(_documentLibry + _folder + @"\");
                            if (File.Exists(_documentLibry + _folder + @"\clean1.xml")) File.Delete(_documentLibry + _folder + @"\clean1.xml");
                            if (File.Exists(_documentLibry + _folder + @"\metadata.xml")) File.Delete(_documentLibry + _folder + @"\metadata.xml");
                            File.Move(clean1File, _documentLibry + _folder + @"\clean1.xml");

                            if (document.Element("name") != null)
                                document.Element("name").Value = lovTitle;
                            else
                                document.Add(new XElement("name", lovTitle));

                            if (document.Element("adress") != null)
                                document.Element("adress").Value = adress.Text;
                            else
                                document.Add(new XElement("adress", adress.Text));

                            if (document.Element("folder") != null)
                                document.Element("folder").Value = _folder;
                            else
                                document.Add(new XElement("folder", _folder));


                            if (document.Element("file") != null)
                                document.Element("file").Value = _documentLibry + _folder + @"\clean1.xml";
                            else
                                document.Add(new XElement("file", _documentLibry + _folder + @"\clean1.xml"));



                            if (document.Element("topic_id") != null)
                                document.Element("topic_id").Value = topic_id;
                            else
                                document.Add(new XElement("topic_id", topic_id));

                            

                            XmlDocument impD = new XmlDocument();
                            impD.Load(_documentLibry + "import.xml");
                            XDocument updateD = XDocument.Load(impD.DocumentElement.CreateNavigator().ReadSubtree());
                            updateD.Root.Elements("document").Where(p=>p.Element("name").Value == document.Element("name").Value).First().ReplaceWith(document);
                            updateD.Save(_documentLibry + "import.xml");
                            XDocument meta = new XDocument();
                            meta.Add(metadata);
                            meta.Save(_documentLibry + _folder + @"\metadata.xml");
                            LoadComboBox();
                            textAdress.Text = lovTitle;
                        }
                    }
                }
            }
        }

        private bool TestFile(string fileName)
        {
            FileStream fs;
            try
            {
                fs = new FileStream(fileName, FileMode.Open,
                FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
            return false;
            }
            fs.Close();
            return true;
        }

        private XElement ResetImportDocument()
        {
            XElement returnValue = null;
            if (File.Exists(_documentLibry + "import.xml"))
            {
                XDocument d = XDocument.Load(_documentLibry + "import.xml");

                if (d != null)
                {
                    foreach (XElement e in d.Descendants("document").Elements())
                    {
                        e.Value = e.Value.Trim();
                        e.Value = e.Value.Replace("[", "(");
                        e.Value = e.Value.Replace("]", ")");
                    }
                    bool found = true;
                    while (found)
                    {
                        found = false;
                        foreach (XElement e in d.Descendants("document"))
                        {
                            string adress = e.Element("adress").Value;
                            if (d.Descendants("document").Where(p => p.Element("adress").Value == adress).Count() != 1)
                            {
                                d.Descendants("document").Where(p => p.Element("adress").Value == adress).First().Remove();
                                found = true;
                                break;
                            }
                        }
                    }
                }
                d.Save(_documentLibry + "import.xml");
            }
            return returnValue;
        }

        private XElement GetImportDocumentByAdress(string adress)
        {
            XElement returnValue = null;
            if (File.Exists(_documentLibry + "import.xml"))
            {
                XDocument d = XDocument.Load(_documentLibry + "import.xml");

                if (d != null)
                {
                    if (d.Descendants("document").Where(p => p.Element("adress").Value == adress).Count() == 1)
                        returnValue = d.Descendants("document").Where(p => p.Element("adress").Value == adress).First();
                    else if (d.Descendants("document").Where(p => p.Element("adress").Value == adress).Count() > 1)
                        for (int i = d.Descendants("document").Where(p => p.Element("adress").Value == adress).Count() - 1; i > 0; i--)
                        {
                            d.Descendants("document").Where(p => p.Element("adress").Value == adress).ElementAt(i).Remove();
                        }
                }
            }
            return returnValue;
        }

        private XElement GetImportDocument(string name)
        {
            XElement returnValue = null;
            if (File.Exists(_documentLibry + "import.xml"))
            {
                XDocument d = XDocument.Load(_documentLibry + "import.xml");

                if (d != null)
                {
                    if (d.Descendants("document").Where(p => p.Element("name").Value == name).Count()  == 1)
                        returnValue = d.Descendants("document").Where(p => p.Element("name").Value == name).First();
                    else if (d.Descendants("document").Where(p => p.Element("name").Value == name).Count()  > 1)
                        for (int i = d.Descendants("document").Where(p => p.Element("name").Value == name).Count() - 1; i > 0; i--)
                        {
                            d.Descendants("document").Where(p => p.Element("name").Value == name).ElementAt(i).Remove();
                        }
                }
            }
            return returnValue;
        }

        private void btnGoToWeb_Click(object sender, EventArgs e)
        {
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString();
                if (File.Exists(xmlPath))
                {
                    XElement document = GetImportDocument(s);
                    if (document != null)
                    {

                        adress.Text = document.Element("adress").Value;
                        myWebBrowser.Navigate(adress.Text);
                    }
                }
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            XElement importD = null;
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString();
                if (File.Exists(xmlPath))
                {
                    importD = GetImportDocument(s);
                    if (importD == null)
                    {
                        MessageBox.Show("Fant ikke dokumentet!");
                        return;
                    }
                }
                bool AsIs = false;
                if (cebAsIs.Checked) AsIs = true;

                string _folder = importD.Element("folder").Value;
                string clear1File = _documentLibry + _folder + @"\tocContent.xml";
                if (!File.Exists(clear1File))
                {
                    MessageBox.Show("Dokumentet er ikke lastet ned!");
                    return;
                }

                string tocFile = _documentLibry + _folder + @"\toc.xml";
                if (!File.Exists(tocFile))
                {
                    MessageBox.Show("Generer innhold (TOC) først");
                    return;
                }



                XDocument Content = XDocument.Load(clear1File);
                XElement toc = XElement.Load(tocFile);

                for (int i = 0; i < toc.DescendantsAndSelf("section").Where(p => p.Attribute("frompara") != null && p.Attribute("fDone")==null).Count(); i++)
                {
                    XElement curr = toc.DescendantsAndSelf("section").Where(p => p.Attribute("frompara") != null && p.Attribute("fDone") == null).ElementAt(i);
                    if (curr.Attribute("frompara") != null)
                    {
                        if (curr.Attribute("frompara").Value.Split('-').Count() == 1)
                        {
                            //int from = Convert.ToInt32(curr.Attribute("frompara").Value.Split('-').ElementAt(0));
                            //int to = Convert.ToInt32(curr.Attribute("topara").Value.Split('-').ElementAt(0));
                            //XElement self = curr;
                            //for (int n = from + 1; n < to; n++)
                            //{
                            //    XElement newSection = new XElement(curr);
                            //}
                        }
                        else if (curr.Attribute("frompara").Value.Split('-').Count() == 2)
                        {
                            //int from = Convert.ToInt32(curr.Attribute("frompara").Value.Split('-').ElementAt(1));
                            //int to = Convert.ToInt32(curr.Attribute("topara").Value.Split('-').ElementAt(1));
                        }
                    }
                }

                for (int i = 0; i<toc.DescendantsAndSelf("section").Count();i++)
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
                        part.Add(Content.Root.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) >= Convert.ToInt32(curr.Attribute("from").Value)
                                && Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(next.Attribute("from").Value)));

                        
                    }
                    else
                    {
                        part = new XElement("part");
                        part.Add(Content.Root.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) >= Convert.ToInt32(curr.Attribute("from").Value)));
                    }
                    //if (curr.Attribute("text").Value.Trim().StartsWith("V Mellombelse")) Debug.Print("xxxx");


                    part = ConvertPart(part);
                   
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

                XElement document = new XElement("document",
                    new XAttribute("type", "word"),
                    new XAttribute("variant", "lov"));
                document.Add(toc);
                XElement documents = new XElement("documents",
                    new XAttribute("type", "word"),
                    new XAttribute("variant", "lov"));
                documents.Add(document);
                

                string saveFile = _documentLibry + _folder + @"\document.xml";
                if (File.Exists(saveFile)) File.Delete(saveFile);
                documents.Save(saveFile);
                doNotAfterNavigate = true;
                myWebBrowser.Navigate(saveFile);

            }
        }

        private XElement ConvertPart(XElement part)
        {

            part.Descendants("a").Remove();

            XElement newPart = new XElement("part");

            bool found = true;
            while (found)
            {
                found = false;
                if (part.Descendants("sup").Count() != 0)
                {
                    foreach (XElement sup in part.Descendants("sup"))
                    {
                        string supId = sup.Value;

                        if (part.Descendants("td").Where(p => p.Elements("small").Count() == 1 && p.Value.Trim() == supId).Count() != 0)
                        {
                            XElement tr = part.Descendants("td").Where(p => p.Elements("small").Count() == 1 && p.Value.Trim() == supId).First().Parent;
                            string fTitle = tr.Elements("td").ElementAt(1).Value.Trim();

                            XElement footnote = new XElement("footnotelov", supId);
                            XAttribute aTitle = new XAttribute("title", fTitle);
                            footnote.Add(aTitle);
                            sup.ReplaceWith(footnote);

                            tr.Parent.Remove();
                            found = true;
                            break;
                        }
                        else if (part.Descendants("td").Where(p => p.Elements("small").Count() == 0 && p.Value.Trim() == supId).Count() != 0 )
                        {
                            XElement tr = part.Descendants("td").Where(p => p.Elements("small").Count() == 0 && p.Value.Trim() == supId).First().Parent;

                            string fTitle = tr.Elements("td").ElementAt(1).Value.Trim();

                            XElement footnote = new XElement("footnotelov", supId);
                            XAttribute aTitle = new XAttribute("title", fTitle);
                            footnote.Add(aTitle);
                            sup.ReplaceWith(footnote);

                            tr.Parent.Remove();
                            found = true;

                            break;
                        }
                    }
                }
            }

            string sLeddRegexp = @"^\((?<nr>(\d+))\)\s";
            
            List<XElement> ledd = part.Elements("p").Where(p => Regex.IsMatch(p.Value.TrimStart(), sLeddRegexp)).Select(p=> new XElement(p)).ToList();

            part.Elements().Where(p => p.Name == "p" && p.Value == "").Remove();
            
            if (part.Elements().Count() == 0) return newPart;

            XElement e = part.Elements().ElementAt(0);
            
            //if (e.Name.ToString() == "h4") Debug.Print("xxx");
            if (e.Elements("small").Count() != 0)
            {
                MessageBox.Show("Her er det feil!" + e.Value);
                return null;
            }

            if ((e.Name.ToString() == "p" || e.Name.ToString() == "h3" || e.Name.ToString() == "h4")
                        && e.Elements("small").Count() == 0 
                        && ledd.Where(p => p == e).Count() == 0)
            {
                if (e.Name.ToString() == "p" && e.Elements("b").Count() != 0 && e.Elements("i").Count() != 0)
                {

                    string sTitle = "";
                    foreach (XElement subE in e.Elements())
                    {
                        sTitle = sTitle + (sTitle != "" ? " " : sTitle) + subE.Value;
                    }
                    newPart.Add(new XElement("title",
                            new XElement("t", sTitle)));
                    e.Remove();
                }
                else if (e.Name.ToString() == "p" && e.Elements("b").Count() != 0 && e.Elements("i").Count() == 0)
                {
                    newPart.Add(new XElement("title",
                            new XElement("t", e.Elements("b").First().Value)));
                    XElement newP = new XElement(e);
                    newP.Elements("b").Remove();
                    newPart.Add(new XElement("p", e.Attributes(), EvalNode(newP).Nodes()));
                    e.Remove();
                }
                else if (e.Name.ToString() == "h3" || e.Name.ToString() == "h4")
                {
                    newPart.Add(new XElement("title",
                            new XElement("t", e.Value.Trim())));
                    e.Remove();
                }
                else
                    MessageBox.Show("Her er det feil i tittel! " + e.Value);
            }

            if (ledd.Count() != 0)
            {
                string firstLeddIdx = ledd.ElementAt(0).Attribute("idx").Value;
                found = true;
                while (found)
                {
                    found = false;
                    foreach (XElement bl in part.Elements()
                        .Where(p => Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(firstLeddIdx)))
                    {
                        XElement newBl = new XElement(bl);
                        GetPartElement(newBl, newPart);
                        bl.Remove();
                        found = true;
                        break;
                    }
                }
                
                for (int l = 0; l < ledd.Count(); l++)
                {
                    //Henter ledd element
                    //================================================================
                    XElement currLedd = new XElement(ledd.ElementAt(l));
                    XElement nextLedd = null;
                    string leddValue = Regex.Match(currLedd.Value.TrimStart(), sLeddRegexp).Groups["nr"].Value;
                    XElement xledd = new XElement("ledd",
                        new XAttribute("value", leddValue));

                    part.Elements().Where(p => p.Attribute("idx").Value == currLedd.Attribute("idx").Value).Remove();
                    newPart.Add(xledd); 
                    
                    XElement newLeddElement = new XElement(currLedd);
                    if (!newLeddElement.HasElements)
                    {
                        newLeddElement.Value = newLeddElement.Value.TrimStart().Substring(Regex.Match(currLedd.Value.TrimStart(), sLeddRegexp).Value.Length);
                    }

                    //Legger til ledd element
                    //================================================================
                    GetPartElement(newLeddElement, xledd);


                    //Heneter elementer etter/mellom ledd og legger til ledd element eller parent-element
                    //================================================================

                    //Hvis flere ledd
                    if ((l+1)<ledd.Count())
                    {
                        nextLedd = new XElement(ledd.ElementAt(l + 1));
                        while (part.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currLedd.Attribute("idx").Value)
                                                                        && Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(nextLedd.Attribute("idx").Value))
                                                            .Count()!=0)

                        {
                            e = part.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currLedd.Attribute("idx").Value)
                                                                        && Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(nextLedd.Attribute("idx").Value))
                                                        .First();
                            //GetPartElement(e, xledd.Parent);

                            if (!(e.Name == "p" && e.Elements("small").Count() != 0) && !(e.Name == "table"
                                    && e.Elements("tr").Elements("td").Count() == 2
                                    && e.Elements("tr").First().Elements("td").First().Elements("small").Count() != 0))
                            {
                                GetPartElement(e, xledd);
                                part.Elements().Where(p => p.Attribute("idx").Value == e.Attribute("idx").Value).Remove();
                            }
                            else
                            {
                                GetPartElement(e, xledd.Parent);
                                part.Elements().Where(p => p.Attribute("idx").Value == e.Attribute("idx").Value).Remove();
                            }
                        }
                    }
                    //Hvis siste ledd
                    else
                    {
                        while (part.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currLedd.Attribute("idx").Value))
                                                            .Count() != 0)
                        
                        {
                            e = part.Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(currLedd.Attribute("idx").Value))
                                                    .First();
                            if (!(e.Name == "p" && e.Elements("small").Count() != 0) && !(e.Name == "table"
                                    && e.Elements("tr").Elements("td").Count() == 2
                                    && e.Elements("tr").First().Elements("td").First().Elements("small").Count() != 0))
                            {
                                //GetPartElement(e, xledd.Parent);
                                GetPartElement(e, xledd);
                                part.Elements().Where(p => p.Attribute("idx").Value == e.Attribute("idx").Value).Remove();
                            }
                            else
                            {
                                GetPartElement(e, xledd.Parent);
                                part.Elements().Where(p => p.Attribute("idx").Value == e.Attribute("idx").Value).Remove();
                            }

                        }
                    }
                      
                }
            }
            
            //legg ut de siste
            //=============================================================================

            for (int i = 0; i < part.Elements().Count(); i++)
            {
                e = part.Elements().ElementAt(i);
                GetPartElement(e, newPart);
            }
            
            return newPart;
        }

        private void GetPartElement(XElement e, XElement newPart)
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
                && e.Descendants("tr").Count()==1
                && e.Descendants("td").Count()==2
                && e.Descendants("th").Count() == 0
                && e.Descendants("small").Count()==0)
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



        private void btnShowHtml_Click(object sender, EventArgs e)
        {
            XElement importD = null;
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString();
                if (File.Exists(xmlPath))
                {
                    importD = GetImportDocument(s);
                    if (importD == null)
                    {
                        MessageBox.Show("Fant ikke dokumentet i importdatabasen!");
                        return;
                    }
                }

                string xmlFile =  _documentLibry + importD.Element("folder").Value + @"\document.xml";
                if (!File.Exists(xmlFile))
                {
                    MessageBox.Show("Filen er ikke konvertert!");
                    return;
                }
                //XDocument xd = XDocument.Load(xmlFile);
                XmlTextReader xmlreader = new XmlTextReader(xmlFile);
                XsltArgumentList xslArg = new XsltArgumentList();

                string path = Path.GetDirectoryName(Application.ExecutablePath);
                string xsltPath = path + @"\xsl\lovprint.xslt";

                if (!File.Exists(xsltPath))
                {
                    MessageBox.Show("Finner ikke filen '" + xsltPath + "'");
                    return;
                }


                string outString = transformDocument.TransformXmlWithXslToString(xmlreader
                    , xsltPath, xslArg);
                xmlreader.Close();
                if (!Directory.Exists(path + @"\html")) Directory.CreateDirectory(path + @"\html");
                string htmlFile = path + @"\html" + @"\document.html";

                File.WriteAllText(htmlFile, outString, Encoding.UTF8);
                doNotAfterNavigate = true;
                myWebBrowser.Navigate(htmlFile);
                //doNotAfterNavigate = false;
            }
            else
            {
                MessageBox.Show("Velg dokument!");
                return;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            XElement importD = null;
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString();
                if (File.Exists(xmlPath))
                {
                    importD = GetImportDocument(s);
                    if (importD == null)
                    {
                        MessageBox.Show("Fant ikke dokumentet i importdatabasen!");
                        return;
                    }
                }

                string folder = _documentLibry + importD.Element("folder").Value + @"\";
                string xmlFile = folder + "document.xml";
                if (!File.Exists(xmlFile))
                {
                    MessageBox.Show("Filen er ikke konvertert til 'document.xml'!");
                    return;
                }
                XDocument xdDoc = XDocument.Load(xmlFile);

                xdDoc.Root.DescendantsAndSelf().Attributes("n").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("href").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("from").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("frompara").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("topara").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("text").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("tid").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("hid").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("htype").Remove();
                xdDoc.Root.DescendantsAndSelf().Attributes("hpara").Remove();


                string xmlMeta = folder + "metadata.xml";
                if (!File.Exists(xmlFile))
                {
                    MessageBox.Show("Filen er ikke konvertert til 'document.xml'!");
                    return;
                }
                XDocument xdMeta = XDocument.Load(xmlMeta);

                XElement root = new XElement("root");

                root.Add(xdMeta.Root);


                
                GetIndexFromDocument( xdDoc.Root, root);

                GetBookmarksFromDocument(xdDoc.Root, root);

                //--------------------------------------------------------
                //--------------------------------------------------------
                //koble bokmerker ny && gammel

                root.Add(xdDoc.Root);


                string fileName =  folder +  xdMeta.Descendants("id").First().Value.ToString() + ".xml";

                XDocument temp = new XDocument();
                temp.Add(root);
                temp.Save(fileName);

            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
             XElement importD = null;
             if (textAdress.SelectedIndex != -1)
             {
                 string s = textAdress.SelectedItem.ToString();
                 if (File.Exists(xmlPath))
                 {
                     importD = GetImportDocument(s);
                     if (importD == null)
                     {
                         MessageBox.Show("Fant ikke dokumentet i importdatabasen!");
                         return;
                     }
                 }

                 string folder = _documentLibry + importD.Element("folder").Value + @"\";
                 string xmlFile = folder +  importD.Element("folder").Value + ".xml";
                 if (!File.Exists(xmlFile))
                 {
                     MessageBox.Show("Filen er ikke konvertert til '" + importD.Element("folder").Value + "'.xml'!");
                     return;
                 }

                 
                 
                 XDocument export = XDocument.Load(xmlFile);
                 XElement newBookmarks = export.Descendants("bookmarks").First();
                 XElement newContent = export.Descendants("content").First();
                 string topic_id = importD.Element("topic_id") == null ? "" : importD.Element("topic_id").Value;
                 XElement oldBookmarks = null;
                 if (topic_id == "")
                 {
                     MessageBox.Show("Ingen Topic ID");
                     topic_id = "NOTID";
                 }
                 else
                 {
                     oldBookmarks = GetTopicByTopicId(topic_id);


                 }
                 XElement bmPointers = new XElement("pointers"
                        , new XAttribute("from_topic_id", topic_id)
                        );

                 string twoFirstWordRegex = @"(?<num>((^[^\s]+(\s[^\.\s]+)?)))(\s|\.|$)";

                 
                 //Finn alle bokmerker som ma1tcher med bokmerker
                 if ((oldBookmarks!=null ? oldBookmarks.Elements().Count():0)!=0 )
                 {
                     foreach (XElement bm in newBookmarks.Elements("bookmark"))
                     {

                         string title = bm.Attribute("title").Value.ToLower().Trim();
                         string title1 = Regex.Match(title, twoFirstWordRegex)
                             .Groups["num"].Value.Trim().ToLower()
                             .Replace("§ ", "§").Replace("§ ", "§");
                         title = title.Replace("§ ", "§").Replace("§ ", "§");
                         if (title1.StartsWith("§9-14")) Debug.Print("16-5");
                         int queryNo = 0;
                         int ant = oldBookmarks
                                        .Elements("bookmark")
                                        .Where(p => Regex.Equals(p.Attribute("title").Value.Trim().ToLower()
                                            .Replace((char)0xA0, (char)0x20)
                                            .Replace("§ ", "§").Replace("§ ", "§"), title)
                                        )
                                        .Count();

                         if (ant == 0)
                         {
                             ant = oldBookmarks
                                    .Elements("bookmark")
                                    .Where(p => Regex.Match(p.Attribute("title").Value.Trim().ToLower()
                                        .Replace((char)0xA0, (char)0x20), twoFirstWordRegex)
                                        .Groups["num"].Value.Trim().ToLower()
                                        .Replace("§ ", "§").Replace("§ ", "§") == title1
                                    )
                                    .Count();
                             if (ant != 0)
                                 queryNo = 3;
                         }

                         double res = 0;
                         if (ant == 0)
                         {
                             //CosineSimilarity c = new CosineSimilarity();
                             JaroWinkler c = new JaroWinkler();
                             res = oldBookmarks
                               .Elements("bookmark")
                               .Max(p => c.GetSimilarity(p.Attribute("title").Value.ToLower()
                                   .Replace((char)0xA0, (char)0x20)
                                   .Replace("§ ", "§").Replace("§ ", "§"), title));

                             c = new JaroWinkler();

                             XElement ob = oldBookmarks
                                    .Elements("bookmark")
                                    .Where(p => c.GetSimilarity(p.Attribute("title").Value.ToLower()
                                        .Replace((char)0xA0, (char)0x20)
                                        .Replace("§ ", "§").Replace("§ ", "§"), title) == res
                                    )
                                    .ElementAt(0);
                             if (res > 0.9)
                             {
                                 ant = 1;
                                 queryNo = 2;
                                 //Debug.Print(title + "//" + res.ToString() + ob.Attribute("title").Value);
                             }
                         }

                         if (ant == 0)
                         {
                             XElement bmPointer = new XElement("pointer"
                                         , new XAttribute("new_key", bm.Attribute("key").Value)
                                         , new XAttribute("status", "nomatch")
                                         );
                         }
                         else
                         {
                             XElement bmPointer = null;
                             XElement matches = null;
                             for (int i = 0; i < ant; i++)
                             {
                                 XElement oldBm = null;
                                 if (queryNo == 0)
                                 {
                                     oldBm = oldBookmarks
                                             .Elements("bookmark")
                                             .Where(p => Regex.Equals(p.Attribute("title").Value.Trim().ToLower()
                                                 .Replace((char)0xA0, (char)0x20)
                                                 .Replace("§ ", "§").Replace("§ ", "§"), title)
                                             )
                                             .ElementAt(i);
                                 }
                                 else if (queryNo == 2)
                                 {
                                     //CosineSimilarity c = new CosineSimilarity();
                                     JaroWinkler c = new JaroWinkler();
                                     oldBm = oldBookmarks
                                            .Elements("bookmark")
                                            .Where(p => c.GetSimilarity(p.Attribute("title").Value.ToLower()
                                                .Replace((char)0xA0, (char)0x20).Replace("§ ", "§").Replace("§ ", "§"), title) == res
                                            )
                                            .ElementAt(i);
                                 }

                                 else if (queryNo == 3)
                                 {
                                     oldBm = oldBookmarks
                                     .Elements("bookmark")
                                     .Where(p => Regex.Match(p.Attribute("title").Value.Trim().ToLower()
                                         .Replace((char)0xA0, (char)0x20), twoFirstWordRegex)
                                         .Groups["num"].Value.Trim().ToLower()
                                         .Replace("§ ", "§").Replace("§ ", "§") == title1
                                     ).ElementAt(i);


                                 }
                                 if (i > 0)
                                 {

                                     bmPointer.Attribute("status").Value = "multi";
                                     if (bmPointer.Element("matches") == null)
                                     {
                                         matches = new XElement("matches");
                                     }
                                     XElement match = new XElement("match"
                                         , new XAttribute("old_key", oldBm.Attribute("key").Value)
                                         );
                                     matches.Add(match);
                                 }
                                 else
                                 {

                                     bmPointer = new XElement("pointer"
                                         , new XAttribute("new_key", bm.Attribute("key").Value)
                                         , new XAttribute("old_key", oldBm.Attribute("key").Value)
                                         , new XAttribute("status", "ok")
                                         );

                                     bmPointers.Add(bmPointer);
                                 }
                             }
                         }
                     }

                     //Finn alle bokmerker som ikke matcher med bokmerker i index
                     foreach (XElement bm in oldBookmarks.Elements("bookmark")
                            .Where(p => bmPointers.Elements("pointer")
                                .Where(q => q.Attribute("old_key").Value == p.Attribute("key").Value).Count() == 0)
                                )
                     {

                         string title = bm.Attribute("title").Value.Trim().ToLower().Replace((char)0xA0, (char)0x20);
                         string title1 = Regex.Match(title, twoFirstWordRegex).Groups["num"].Value.Replace("§ ", "§").Replace("§ ", "§");
                         title = title.Replace("§ ", "§").Replace("§ ", "§");
                         int queryNo = 0;
                         int ant = newContent
                                        .Descendants("item")
                                        .Where(p => p.Attribute("text").Value.Trim().ToLower()
                                            .Replace("§ ", "§").Replace("§ ", "§") == title)
                                        .Count();

                         if (ant == 0)
                         {
                             ant = newContent
                                        .Descendants("item")
                                        .Where(p => Regex.Match(p.Attribute("text").Value.Trim().ToLower(), twoFirstWordRegex)
                                            .Groups["num"].Value
                                            .Replace("§ ", "§").Replace("§ ", "§") == title1)
                                        .Count();
                             if (ant != 0)
                             {
                                 queryNo = 3;
                             }
                         }
                         double res = 0;
                         if (ant == 0)
                         {
                             JaroWinkler c = new JaroWinkler();
                             res = newContent
                               .Descendants("item")
                               .Max(p => c.GetSimilarity(p.Attribute("text").Value.ToLower()
                                   .Replace("§ ", "§").Replace("§ ", "§"), title));
                             if (res > 0.9)
                             {
                                 ant = 1;
                                 queryNo = 2;
                             }
                         }

                         XElement bmPointer = null;
                         XElement matches = null;
                         if (ant != 0)
                         {
                             for (int i = 0; i < ant; i++)
                             {
                                 XElement oldBm = null;
                                 if (queryNo == 0)
                                 {
                                     oldBm = newContent
                                                .Descendants("item")
                                                .Where(p => p.Attribute("text").Value.Trim().ToLower()
                                                    .Replace("§ ", "§").Replace("§ ", "§") == title)
                                                .ElementAt(i);
                                 }
                                 else if (queryNo == 2)
                                 {
                                     JaroWinkler c = new JaroWinkler();
                                     oldBm = newContent
                                       .Descendants("item")
                                       .Where(p => c.GetSimilarity(p.Attribute("text").Value.ToLower()
                                           .Replace("§ ", "§").Replace("§ ", "§"), title) == res)
                                       .ElementAt(i);

                                 }
                                 else if (queryNo == 3)
                                 {
                                     oldBm = newContent
                                                .Descendants("item")
                                                .Where(p => Regex.Match(p.Attribute("text").Value.Trim().ToLower(), twoFirstWordRegex)
                                                    .Groups["num"].Value
                                                    .Replace("§ ", "§").Replace("§ ", "§") == title1)
                                                .ElementAt(i);
                                 }

                                 if (i > 0)
                                 {

                                     bmPointer.Attribute("status").Value = "multi";
                                     if (bmPointer.Element("matches") == null)
                                     {
                                         matches = new XElement("matches");
                                     }
                                     XElement match = new XElement("match"
                                         , new XAttribute("old_key", bm.Attribute("key").Value)
                                         );
                                     matches.Add(match);
                                 }
                                 else
                                 {

                                     bmPointer = new XElement("pointer"
                                         , new XAttribute("new_key", oldBm.Attribute("id").Value)
                                         , new XAttribute("old_key", bm.Attribute("key").Value)
                                         , new XAttribute("status", "ok")
                                         );

                                     bmPointers.Add(bmPointer);
                                 }
                             }

                         }
                     }

                     //skriv ut alle bokmerker som ikke matcher
                     foreach (XElement bm in oldBookmarks.Elements("bookmark")
                                             .Where(p => bmPointers.Elements("pointer")
                                                 .Where(q => q.Attribute("old_key").Value == p.Attribute("key").Value).Count() == 0)
                                                 )
                     {


                         string title = bm.Attribute("title").Value.Trim().Replace("kapittel", "kapitel")
                             .Replace((char)0xA0, (char)0x20);

                         int ant = newContent
                                        .Descendants("item")
                                        .Where(p => p.Attribute("text").Value.Trim().Replace("kapittel", "kapitel").StartsWith(title))
                                        .Count();


                         XElement bmPointer = null;
                         if (ant == 0)
                         {
                             bmPointer = new XElement("pointer"
                            , new XAttribute("new_key", "")
                            , new XAttribute("old_key", bm.Attribute("key").Value)
                            , new XAttribute("status", "none")
                            , new XAttribute("text", title)
                            );

                             bmPointers.Add(bmPointer);

                         }
                     }
                 }
                 export.Root.Descendants("pointers").Remove();
                     export.Root.Add(bmPointers);
                     export.Save(xmlFile);

                     string sConnected=_documentLibry + "_connected"; 
                     if (!Directory.Exists(sConnected)) Directory.CreateDirectory(sConnected);

                     string name = Regex.Match(importD.Element("name").Value,@"\((?<name>([a-zæøå]+))\)", RegexOptions.IgnoreCase).Groups["name"].Value;
                     if (name != "")
                     {
                         export.Save(sConnected + @"\" + name + "_" + importD.Element("folder").Value + "_" + topic_id + ".xml");

                     }
                     else
                     {
                         name = "lovnavn";
                         if (InputBox("Tast inn navn på lov", "Navn", ref name) == DialogResult.OK)
                         {
                             export.Save(sConnected + @"\" + name + "_" + importD.Element("folder").Value + "_" + topic_id + ".xml");

                         }
                     }

                     string fileName = sConnected + @"\" + name + "_" + importD.Element("folder").Value + "_" + topic_id + ".xml";
                     string XmlString = "";
                     if (File.Exists(fileName))
                     {
                         FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read);
                         StreamReader sr = new StreamReader(file, Encoding.ASCII);
                         XmlString = sr.ReadToEnd();
                         sr.Close();
                         file.Close();


                         XmlString = Regex.Replace(XmlString, @"<\?xml[^<]+?>","", RegexOptions.Multiline | RegexOptions.Singleline);
                         XmlString = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(XmlString));

                         //EncodeTo64(XmlString.TrimStart());
                         commxml cx = new commxml();
                         string sourceRegexp = cx.Execute("diba0706.dbo._ImpClientImportDocument '" + topic_id + "', '" + XmlString + "'");
                         XDocument d = XDocument.Parse(sourceRegexp);
                         Debug.Print(d.ToString());
                        // XElement eT = new XElement((XElement)d.Descendants("root").First().Elements("root").First());
                         //XDocument toImport = XDocument.Parse(eT.ToString());
                     } 
                     

                 }
             

        }

        private string EncodeTo64(string toEncode)
        {

            byte[] toEncodeAsBytes

                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);

            string returnValue

                  = System.Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;

        }


        private DialogResult InputBox(string title, string promptText, ref string value)
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



        private class bookmark
        {
            public string id { get; set; }
        }

        private void btnTOC_Click(object sender, EventArgs e)
        {
            _vedlCounter = 0;
            XElement importD = null;
            if (textAdress.SelectedIndex != -1)
            {
                string s = textAdress.SelectedItem.ToString();
                if (File.Exists(xmlPath))
                {
                    importD = GetImportDocument(s);
                    if (importD == null)
                    {
                        MessageBox.Show("Fant ikke dokumentet!");
                        return;
                    }
                }
                if (importD.Element("folder") == null)
                {
                    MessageBox.Show("Dokumentet er ikke lastet ned!");
                    return;
                }

                string _folder = importD.Element("folder").Value;
                string clear1File = _documentLibry + _folder + @"\clean1.xml";
                if (!File.Exists(clear1File))
                {
                    MessageBox.Show("Dokumentet er ikke lastet ned!");
                    return;
                }

               

                XDocument xd = XDocument.Load(clear1File);

                string idx1 = xd.XPathSelectElement("html/h3[1]").Attribute("idx").Value.ToString();
                string idx2 = xd.XPathSelectElement("html/a[@name='map0']") == null ? null : xd.XPathSelectElement("html/a[@name='map0']").Attribute("idx").Value.ToString();
                string max = FindEnd(xd);

                
                XElement toc = null;
                if (idx2 != null)
                {
                    List<XElement> l = xd
                                        .Descendants("a")
                                        .Where(p => Convert.ToInt32(p.Attribute("idx").Value) > Convert.ToInt32(idx1)
                                            && Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(idx2)
                                            && p.Value.Trim() != "")
                                            
                                        .ToList();

                    _minPara = 100;
                    _maxPara = 0;
                    _v0_GetHeaderType(l);
                    toc = GetIndexBranch(l, true);
                    if (toc != null)
                    {
                        foreach (XElement section in toc.DescendantNodesAndSelf())
                        {
                            string href = section.Attribute("href").Value;
                            string org_href = "";
                            href = href.Replace("#", "").Trim();
                            href = href.Replace(" ", "_");
                            org_href = href;
                            int ant = 0;
                            bool replace = false;
                            while (ant == 0)
                            {
                                ant = xd.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value.Trim()) == href).Count();

                                if (ant != 0)
                                {
                                    string from = "";
                                    XElement a = xd.Descendants("a").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value.Trim()) == href).First();
                                    switch (a.Parent.Name.ToString())
                                    {
                                        case "html":
                                            from = a.Attribute("idx").Value;
                                            break;
                                        default:
                                            from = a.Parent.Attribute("idx").Value;
                                            break;
                                    }
                                    XAttribute aFrom = new XAttribute("from", from);
                                    section.Add(aFrom);
                                }
                                else
                                {
                                    if (!replace)
                                    {
                                        replace = true;
                                        if (href.IndexOf((char)0x2014) != -1)
                                        {
                                            href = href.Replace(" ", "_");
                                            href = href.Replace(((char)0x2014).ToString(), "_ndash_");
                                        }
                                        else if (href.IndexOf((char)0x2013) != -1)
                                        {
                                            href = href.Replace(" ", "_");
                                            href = href.Replace(((char)0x2013).ToString(), "_ndash_");
                                        }
                                    }
                                    else
                                    {
                                        foreach (char c in href)
                                        {
                                            Debug.Print(c.ToString());
                                        }
                                        foreach (XElement a in xd.Descendants("a")
                                            .Where(p =>
                                                Regex.IsMatch(p.Attribute("name") != null ? p.Attribute("name").Value : "", "[a-z]+", RegexOptions.IgnoreCase)))
                                        {
                                            string test = a.Attribute("name").Value;
                                            Debug.Print(test);
                                            foreach (char c in test)
                                            {
                                                Debug.Print(c.ToString());
                                            }



                                        }
                                        ant = 0;
                                        MessageBox.Show("Finner ikke bokmerke: " + org_href);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (idx1 != null)
                {
                    List<XElement> l = xd
                                        .Descendants("h3")
                                        .Where(p => Convert.ToInt32(p.Attribute("idx").Value) >= Convert.ToInt32(idx1))
                                        .ToList();
                    toc = GetIndexBranchNoIndex(l, true);

                }
                
                if (toc!= null)
                {
                    string tocFile = _documentLibry + _folder + @"\toc.xml";
                    string tocContentFile = _documentLibry + _folder + @"\tocContent.xml";
                    toc.Save(tocFile);
                    XElement document = new XElement("html");
                    document.Add(xd.Element("html").Elements().Where(p => Convert.ToInt32(p.Attribute("idx").Value) >= Convert.ToInt32(toc.Attribute("from").Value)
                                                    && Convert.ToInt32(p.Attribute("idx").Value) < Convert.ToInt32(max)));
                    document.Save(tocContentFile);

                    XmlTextReader xmlreader = new XmlTextReader(tocFile);
                    XsltArgumentList xslArg = new XsltArgumentList();

                    string path = Path.GetDirectoryName(Application.ExecutablePath);
                    string xsltPath = path + @"\xsl\toc.xslt";

                    if (!File.Exists(xsltPath))
                    {
                        MessageBox.Show("Finner ikke filen '" + xsltPath + "'");
                        return;
                    }


                    string outString = transformDocument.TransformXmlWithXslToString(xmlreader
                        , xsltPath, xslArg);
                    xmlreader.Close();
                    if (!Directory.Exists(_documentLibry + _folder + @"\html")) Directory.CreateDirectory(_documentLibry + _folder + @"\html");
                    string htmlFile = _documentLibry + _folder + @"\html" + @"\toc.html";

                    File.WriteAllText(htmlFile, outString, Encoding.UTF8);
                    doNotAfterNavigate = true;
                    string navTo = importD.Element("name").Value;
                    //textAdress.Text = navTo;
                    myWebBrowser.Navigate(htmlFile);

                }   
            }
            
        }

        private void btnsearchList_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                for (int i = 0; i < textAdress.Items.Count;i++ )
                {
                    string f = textAdress.Items[i].ToString();
                    if (f.IndexOf(textBox1.Text) != -1)
                    {
                        if (MessageBox.Show(f, "Søk", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            textAdress.Text = f;
                            break;
                        }

                    }
                }
                
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            
            textAdress.SelectedItem = null;
            textAdress.Text = "";
            myWebBrowser.Navigate(adress.Text);
        }

        private void btnInternal_Click(object sender, EventArgs e)
        {
        }

        private void kopierToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void mnuCatch_Click(object sender, EventArgs e)
        {

        }

        private void mnuCatch_Open(object sender, EventArgs e)
        {

        }
        
    }
}
