using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using DIB.RegExp.Util;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Dib.Data;

namespace DIB.RegExp.InternalLinks
{

    public class InternalLinks
    {
        private static Regex _LovForToken;
        private static Regex _ParaToken;
        private static XElement _LovForTokenActions;
        private static Dictionary<string, string> _R = null;

        private static void LoadRegexp()
        {
            ReadRegExExpressionsEx r = new ReadRegExExpressionsEx();
            _R = r.Read();

            //_LovForToken = new Regex(_R["lovforpara"]);
            _LovForToken = new Regex(_R["front_para_itrail"]);
            _ParaToken = new Regex(_R["parasingle"]);
            //GetActions();
        }

        private static void GetActions()
        {
            string fileName = System.Windows.Forms.Application.StartupPath + @"\xml\regexp.xml";
            XDocument xDoc = XDocument.Load(fileName);
            _LovForTokenActions = new XElement("root");

            //foreach (XElement actions in xDoc.Root
            //                        .Elements("regexp")
            //                        .Where(p=>(p.Element("id") == null ? "" : p.Element("id").Value) == "lovforpara")
            //                        .Descendants("actions")
            //                        .Where(p=>(p.Parent.Attribute("level")==null ? "" :p.Parent.Attribute("level").Value)=="1"))
            foreach (XElement actions in xDoc.Root
                        .Descendants("actions"))

            {
                _LovForTokenActions.Add(new XElement("part"
                                    , new XAttribute(actions.Parent.Attribute("name"))
                                    , new XElement(actions)));
            }

        }


        private static void GetSingelPara(Match match)
        {
            string textNode = match.Value.ToString();
            MatchCollection mc = _ParaToken.Matches(textNode);
            XElement c = new XElement("container");
            int start = 0;
            string startText = "";
            if (mc.Count != 0)
            {
                for (int i = 0; i < mc.Count; i++)
                {
                    Match m = mc[i];
                    if (m.Groups["parasingleparanr"].Success)
                    {
                        Debug.Print(m.Groups["parasingleparanr"].Value);
                    }
                }
            }
        }
        private static XElement GetReferanceString(XNode n)
        {
            string textNode = n.ToString();
            MatchCollection mc = _LovForToken.Matches(textNode);
            XElement c = new XElement("container");
            int start = 0;
            string startText = "";
            if (mc.Count != 0)
            {
                //Debug.Print("---------------------------------------");
                //Debug.Print("TREFF i text: " + mc.Count.ToString() );
                //Debug.Print("---------------------------------------");
                for (int i = 0; i < mc.Count; i++)
                {
                    Match m = mc[i];
                    if (m.Groups["lov_para_lov"].Success)
                    {

                        if (m.Groups["para"].Success && m.Groups["lov_after"].Success && !m.Groups["lov_before"].Success)
                        {
                            Debug.Print("PL " + m.Value.ToString());
                            GetSingelPara(m);
                        }
                        else if (m.Groups["lov_before"].Success && m.Groups["para"].Success && !m.Groups["lov_after"].Success)
                        {
                            Debug.Print("LP " + m.Value.ToString());
                            GetSingelPara(m);
                        }
                        else if (!m.Groups["lov_before"].Success && m.Groups["para"].Success && !m.Groups["lov_after"].Success)
                        {
                            Debug.Print("P " + m.Value.ToString());
                            GetSingelPara(m);
                        }
                        else if (m.Groups["lov_before"].Success && m.Groups["para"].Success && m.Groups["lov_after"].Success)
                        {
                            Debug.Print("LPA " + m.Value.ToString());
                            GetSingelPara(m);
                        }
                    }
                    else if (m.Groups["lovtoken"].Success)
                        Debug.Print("L " + m.Value.ToString());

                    if (i == 0)
                    {
                        if (m.Index != 0)
                        {
                            startText = textNode.Substring(start, m.Index);
                            c.Add(new XText(textNode.Substring(start, m.Index)));
                            start = m.Index;
                        }
                    }
                    if (start < m.Index)
                    {
                        startText = textNode.Substring(start, m.Index - start);
                        c.Add(new XText(startText));
                        start = m.Index;
                    }



                    string startToken = GetTokenText(startText, 0);
                    string endText = "";
                    string endToken = "";

                    if (i + 1 < mc.Count)
                    {
                        endText = textNode.Substring(m.Index + m.Length, mc[i + 1].Index - (m.Index + m.Length));
                    }
                    else
                    {
                        endText = textNode.Substring(m.Index + m.Length, textNode.Length - (m.Index + m.Length));
                    }

                    endToken = GetTokenText(endText, 1);


                    //
                    //for hvert navgitt element
                    //

                    //foreach (XElement part in _LovForTokenActions.Descendants("part"))
                    //{
                    //    if (m.Groups[part.Attribute("name").Value].Success)
                    //    {
                    //        if (part.Element("actions")!=null)
                    //            ExecuteActions(textNode, m, ref c, new XElement(part.Element("actions")));
                    //    }
                    //}

                    //Bak siste match
                    if (!(i + 1 < mc.Count))
                        c.Add(new XText(endText));

                    start = m.Index + m.Length;

                }
            }

            if (c.Nodes().Count() != 0)
                return c;
            else
                return null;
        }

        private static void ExecuteActions(string textNode, Match m,  ref XElement c, XElement actions)
        {
            foreach (XElement action in actions.Elements("action"))
            {
                if (action.Attribute("type").Value == "create" && action.Attribute("object").Value == "element")
                {
                    XElement newElement = new XElement(action.Attribute("name").Value.ToString());
                    ExecuteActions(textNode, m, ref newElement, action);
                    c.Add(newElement);
                }
                else if (action.Attribute("type").Value == "create" && action.Attribute("object").Value == "attribute")
                {
                    XAttribute newAttribute = new XAttribute(action.Attribute("name").Value.ToString(), "");
                    ExecuteActions(textNode, m, ref newAttribute, actions);
                }
                else if (action.Attribute("type").Value == "get_text" && action.Attribute("object").Value == "group")
                {
                    XText t = new XText( m.Groups[action.Attribute("name").Value.ToString()].Value.ToString());
                    c.Add(t);
                }
                else if (action.Attribute("type").Value == "get_text" && action.Attribute("object").Value == "between")
                {
                    string g1 = action.Attribute("group1").Value;
                    string g2 = action.Attribute("group2").Value;

                    int start = m.Groups[g1].Index + m.Groups[g1].Length;
                    int length = m.Groups[g2].Index - start;
                    string sBetween = textNode.Substring(start, length);
                    c.Add(new XText(sBetween));
                }
                 
                else if (action.Attribute("type").Value == "add" && action.Attribute("object").Value == "xparas")
                {
                    string paraToken = m.Groups["para"].Value;
                    c.Add(ExtractPara(paraToken));
                }

            }
        }

        private static void ExecuteActions(string textNode,  Match m, ref XAttribute a, XElement actions)
        {
            foreach (XElement action in actions.Elements("action"))
            {
                if (action.Attribute("type").Value == "add" && action.Attribute("object").Value == "text")
                {
                    a.Value = a.Value + action.Attribute("type").Value.ToString();
                }
            }
        }
        private static XElement GetReferanceString(XNode n, string test)
        {

            string textNode = n.ToString();
            //string regexp = _R["starting_text"] + _R["paratoken"] ;
            string regexp = "("
                            + "(?<sptoken>((?<=^|\\s|\\(|\\.|\\,)(" + _R["sourcetokens"] + "\\s+" + _R["paratoken"] + ")))"
                            + "|" + "(?<pltoken>(" + _R["paratoken"] + "\\s+i\\s+" + _R["lovname"] + "))"
                            + "|" + "(?<pftoken>(" + _R["paratoken"] + "\\s+i\\s+" + _R["forname"] + "))"
                            + "|" + "(?<pldtoken>(" + _R["paratoken"]  + "\\s+i\\s+" + _R["lovdate"]+ "))"
                            + "|" + "(?<plttoken>(" + _R["paratoken"] + "\\s+" + _R["ilovom"] + "))"
                            + "|" + "(?<pdennelovtoken>(" + _R["paratoken"] + "\\s+i\\s+" + _R["dennelov"] + "))"
                            + "|" + "(?<pdennefortoken>(" + _R["paratoken"] + "\\s+i\\s+" + _R["dennefor"] + "))"
                            + "|" + "(?<ldptoken>(" + _R["lovdate"] + "\\s+" + _R["paratoken"] + "))"
                            + "|" + "(?<=^|\\s|\\(|\\.|\\,)(?<stoken>(" + _R["sourcetokens"] + "))"
                            + "|" + "(?<ltptoken>(" + _R["lovname"] + "\\s+" + _R["paratoken"] + "))"
                            + "|" + "(?<lt2ptoken>(" + _R["lovom2"] + "\\s+" + _R["paratoken"] + "))"
                            + "|" + "(?<dennelovptoken>(" + _R["dennelov"] + "\\s+" + _R["paratoken"] + "))"
                            + "|" + "(?<denneforptoken>(" + _R["dennefor"] + "\\s+" + _R["paratoken"] + "))"
                            + "|" + "(?<ldtoken>(" + _R["lovdate"] + "))" 
                            + "|" + "(?<ptoken>(" + _R["paratoken"] + "))" 
                            + "|" + "(?<ltoken>(" + _R["lovname"] + "))"
                            + "|" + "(?<ftoken>(" + _R["forname"] + "))"
                            + "|" + "(?<lt2token>(" + _R["lovom2"] + "))"
                            + ")";




            MatchCollection mc = Regex.Matches(textNode, regexp);
            XElement c = new XElement("container");
            int start = 0;
            string startText = "";
            if (mc.Count != 0)
            {
                Debug.Print("---------------------------------------");
                Debug.Print("TREFF i text: " + mc.Count.ToString() );
                Debug.Print("---------------------------------------");
                for (int i = 0; i < mc.Count; i++)
                {
                    Match m = mc[i];
                    if (i == 0)
                    {
                        if (m.Index != 0)
                        {
                            startText = textNode.Substring(start, m.Index);
                            c.Add(new XText(textNode.Substring(start, m.Index)));
                            start = m.Index;
                        }
                    }
                    if (start < m.Index)
                    {
                        startText = textNode.Substring(start, m.Index - start);
                        c.Add(new XText(startText));
                        start = m.Index;
                    }



                    string startToken = GetTokenText(startText, 0);
                    string endText = "";
                    string endToken = "";

                    if (i + 1 < mc.Count)
                    {
                        endText = textNode.Substring(m.Index + m.Length, mc[i + 1].Index - (m.Index + m.Length));
                    }
                    else
                    {
                        endText = textNode.Substring(m.Index + m.Length, textNode.Length - (m.Index + m.Length));
                    }

                    endToken = GetTokenText(endText, 1);


                    //
                    //for hvert navgitt element
                    //

                    if (m.Groups["ltoken"].Success)
                    {
                        c.Add(new XElement("xsource"
                            , m.Groups["lovname"].Value.ToString()
                            , new XAttribute("type", "lovname")));
                    }
                    else if (m.Groups["lt2token"].Success)
                    {
                        c.Add(new XElement("xsource"
                            , m.Groups["lovtext2"].Value.ToString()
                            , new XAttribute("type", "lt2token")));
                    }
                    else if (m.Groups["pltoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                            , new XAttribute("type", "pltoken"));
                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        string betW = textNode.Substring(m.Groups["para"].Index + m.Groups["para"].Length, m.Groups["lovname"].Index - (m.Groups["para"].Index + m.Groups["para"].Length));
                        xSp.Add(new XText(betW));

                        xSp.Add(m.Groups["lovname"].Value);
                        c.Add(xSp);

                    }
                        //pdennelovtoken
                    else if (m.Groups["pdennelovtoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                            , new XAttribute("type", "pdennelovtoken"));
                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        string betW = textNode.Substring(m.Groups["para"].Index + m.Groups["para"].Length, m.Groups["dlov"].Index - (m.Groups["para"].Index + m.Groups["para"].Length));
                        xSp.Add(new XText(betW));

                        xSp.Add(m.Groups["dlov"].Value);
                        c.Add(xSp);

                    }

                    else if (m.Groups["pdennefortoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                            , new XAttribute("type", "pdennefortoken"));
                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        string betW = textNode.Substring(m.Groups["para"].Index + m.Groups["para"].Length, m.Groups["dfor"].Index - (m.Groups["para"].Index + m.Groups["para"].Length));
                        xSp.Add(new XText(betW));

                        xSp.Add(m.Groups["dfor"].Value);
                        c.Add(xSp);

                    }
                        //dennelovptoken
                    else if (m.Groups["dennelovptoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , m.Groups["dlov"].Value.ToString()
                                        , new XAttribute("type", "dennelovptoken"));

                        Debug.Print("Internal dennelovptoken: " + m.Groups["dlov"].Value + " " + m.Groups["para"].Value + " start: " + startToken + " End: " + endToken);

                        string betW = textNode.Substring(m.Groups["dlov"].Index + m.Groups["dlov"].Length, m.Groups["para"].Index - (m.Groups["dlov"].Index + m.Groups["dlov"].Length));

                        xSp.Add(new XText(betW));

                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));
                        c.Add(xSp);
                    }

                        //dennelovptoken
                    else if (m.Groups["denneforptoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , m.Groups["dfor"].Value.ToString()
                                        , new XAttribute("type", "denneforptoken"));

                        Debug.Print("Internal denneforptoken: " + m.Groups["dfor"].Value + " " + m.Groups["para"].Value + " start: " + startToken + " End: " + endToken);

                        string betW = textNode.Substring(m.Groups["dfor"].Index + m.Groups["dfor"].Length, m.Groups["para"].Index - (m.Groups["dfor"].Index + m.Groups["dfor"].Length));

                        xSp.Add(new XText(betW));

                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));
                        c.Add(xSp);
                    }

                    else if (m.Groups["ftoken"].Success)
                    {
                        c.Add(new XElement("xsource"
                            , m.Groups["forname"].Value.ToString()
                            , new XAttribute("type", "forname")));
                    }
                    else if (m.Groups["pftoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                            , new XAttribute("type", "pftoken"));
                        
                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        string betW = textNode.Substring(m.Groups["para"].Index + m.Groups["para"].Length, m.Groups["forname"].Index - (m.Groups["para"].Index + m.Groups["para"].Length));
                        xSp.Add(new XText(betW));

                        xSp.Add(m.Groups["forname"].Value);
                        c.Add(xSp);

                    }
                    else if (m.Groups["ldtoken"].Success)
                    {
                        Debug.Print("External lovdata: " + m.Groups["lovdate"].Value + " start: " + startToken + " End: " + endToken);
                        

                        c.Add(new XElement("xsource"
                            , m.Groups["lovdate"].Value.ToString()
                            , new XAttribute("type", "lovdate")));
                    }
                    else if (m.Groups["stoken"].Success)
                    {
                        Debug.Print("External stoken: " + m.Groups["sourcename"].Value + " start: " + startToken + " End: " + endToken);
                        c.Add(new XElement("xsource"
                                , m.Groups["sourcename"].Value.ToString()
                                , new XAttribute("type", "stoken")));
                    }
                    else if (m.Groups["sptoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , m.Groups["sourcename"].Value.ToString()
                                        , new XAttribute("type", "stoken"));

                        Debug.Print("External sptoken: " + m.Groups["sourcename"].Value + " " + m.Groups["para"].Value + " start: " + startToken + " End: " + endToken);

                        string betW = textNode.Substring(m.Groups["sourcename"].Index + m.Groups["sourcename"].Length, m.Groups["para"].Index - (m.Groups["sourcename"].Index + m.Groups["sourcename"].Length));

                        xSp.Add(new XText(betW));

                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));
                        c.Add(xSp);
                    }
                    else if (m.Groups["ldptoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , m.Groups["lovdate"].Value.ToString()
                                        , new XAttribute("type", "ldptoken"));


                        Debug.Print("External lovdata: " + m.Groups["lovdate"].Value + " start: " + startToken + " End: " + endToken);


                        string betW = textNode.Substring(m.Groups["lovdate"].Index + m.Groups["lovdate"].Length, m.Groups["para"].Index - (m.Groups["lovdate"].Index + m.Groups["lovdate"].Length));

                        xSp.Add(new XText(betW));

                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));
                        c.Add(xSp);

                    }
                    else if (m.Groups["pldtoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , new XAttribute("type", "pldtoken"));


                        Debug.Print("External para lovdata: " + m.Groups["para"] + " " + m.Groups["lovdate"].Value + " start: " + startToken + " End: " + endToken);
                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        string betW = textNode.Substring(m.Groups["para"].Index + m.Groups["para"].Length, m.Groups["lovdate"].Index - (m.Groups["para"].Index + m.Groups["para"].Length));
                        xSp.Add(new XText(betW));

                        xSp.Add(m.Groups["lovdate"].Value);
                        c.Add(xSp);

                    }
                    else if (m.Groups["plttoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , new XAttribute("type", "plttoken"));


                        Debug.Print("External para i lov om: " + m.Groups["para"] + " " + m.Groups["lovtext"].Value + " start: " + startToken + " End: " + endToken);
                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));
                        string betW = textNode.Substring(m.Groups["para"].Index + m.Groups["para"].Length, m.Groups["lovtext"].Index - (m.Groups["para"].Index + m.Groups["para"].Length));
                        xSp.Add(new XText(betW));

                        xSp.Add(m.Groups["lovtext"].Value);
                        c.Add(xSp);

                    }
                    else if (m.Groups["ltptoken"].Success)
                    {
                        XElement xSp = new XElement("xsource"
                                        , m.Groups["lovname"].Value.ToString()
                                        , new XAttribute("type", "ltptoken"));


                        Debug.Print("External para i lov om: " + m.Groups["lovname"] + " " + m.Groups["para"].Value + " start: " + startToken + " End: " + endToken);

                        string betW = textNode.Substring(m.Groups["lovname"].Index + m.Groups["lovname"].Length, m.Groups["para"].Index - (m.Groups["lovname"].Index + m.Groups["lovname"].Length));
                        xSp.Add(new XText(betW));

                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        c.Add(xSp);

                    }
                    else if (m.Groups["lt2ptoken"].Success)// lt2ptoken
                    {
                        XElement xSp = new XElement("xsource"
                                        , m.Groups["lovtext2"].Value.ToString()
                                        , new XAttribute("type", "lt2ptoken"));


                        Debug.Print("External para i lov om: " + m.Groups["lovtext2"] + " " + m.Groups["para"].Value + " start: " + startToken + " End: " + endToken);

                        string betW = textNode.Substring(m.Groups["lovtext2"].Index + m.Groups["lovtext2"].Length, m.Groups["para"].Index - (m.Groups["lovtext2"].Index + m.Groups["lovtext2"].Length));
                        xSp.Add(new XText(betW));

                        string paraToken = m.Groups["para"].Value;
                        xSp.Add(ExtractPara(paraToken, startToken, endToken));

                        c.Add(xSp);
                    }

                    else if (m.Groups["ptoken"].Success)
                    {
                        string paraToken = m.Groups["para"].Value;

                        XElement xPs = ExtractPara(paraToken, startToken, endToken);

                        XAttribute xInt = new XAttribute("type", "internal");
                        xPs.Add(xInt);
                        c.Add(xPs);
                    }

                    //Bak siste match
                    if (!(i + 1 < mc.Count))
                        c.Add(new XText(endText));

                    start = m.Index + m.Length;
    
                }
            }

            if (c.Nodes().Count() != 0)
                return c;
            else
                return null;
        }

        private static XElement ExtractPara(string paraToken)
        {
            XElement returnElement = new XElement("xparas");
            returnElement.Add(GetSingelPara(paraToken).Nodes());
            return returnElement;
        }



        private static XElement ExtractPara(string paraToken
                                        , string startToken
                                        , string endToken)
        {
            XElement returnElement = new XElement("xparas");
            
            switch (startToken.Trim().ToLower().ToString())
            {
                case "":
                case "se":
                case "etter":
                case "gjelder":
                case "av":
                case "i":
                case "jf.":
                case "jf. likevel":
                case "jf":
                case "se likevel":
                case "se også":
                case "unntatt fra":
                case "unntak fra":
                case "under":
                case "strid med":
                    Debug.Print("Intern: " + startToken + " " + paraToken + " End: " + endToken);
                    returnElement.Add(GetSingelPara(paraToken).Nodes());
                    break;
                default:
                    {
                        bool bFound = false;
                        for (int ii = 0; ii < 4; ii++)
                        {
                            string ccToken = "";
                            switch (ii)
                            {
                                case 0: ccToken = "lova"; break;
                                case 1: ccToken = "loven"; break;
                                case 2: ccToken = "lovene"; break;
                                case 3: ccToken = "forskriften"; break;
                            }
                            if (startToken.Trim().ToLower().EndsWith(ccToken))
                            {
                                bFound = true;
                                break;
                            }
                        }
                        if (bFound)
                        {
                            Debug.Print("Extern:" + startToken + " " + paraToken + " End: " + endToken);
                            returnElement.Add(GetSingelPara(paraToken).Nodes());
                            break;
                        }
                        else
                        {
                            if (startToken.Trim().EndsWith(":"))
                            {
                                Debug.Print("Intern: " + startToken + " " + paraToken + " End: " + endToken);
                                returnElement.Add(GetSingelPara(paraToken).Nodes());
                            }
                            else
                            {
                                //Debug.Print(textNode);
                                Debug.Print("start: " + startToken + " //Ref: " + paraToken + " End: " + endToken);
                                returnElement.Add(GetSingelPara(paraToken).Nodes());
                            }
                            break;
                        }
                    }
            }
            return returnElement;
        }
        
        
        private static XElement GetSingelPara(string pareRef)
        {
            XElement returnElements = new XElement("container");
            string regexp = _R["parasingle"];
            int start = 0;
            string startText = "";
            MatchCollection mc = Regex.Matches(pareRef, regexp);
            if (mc.Count != 0)
            {
                for (int i = 0; i < mc.Count;i++)
                {
                    Match m = mc[i];

                    if (start < m.Index)
                    {
                        startText = pareRef.Substring(start, m.Index - start);
                        returnElements.Add(new XText(startText));
                        start = m.Index;
                    }

                    if (m.Groups["parasinglekapname"].Success)
                    {
                        Debug.Print("kap: " + m.Groups["parasinglekapname"].Value + " (" + m.Value + ")");
                        returnElements.Add(new XElement("xpara", m.Groups["parasinglekapname"].Value.ToString()));
                        start = m.Groups["parasinglekapname"].Index + m.Groups["parasinglekapname"].Length;
                    }
                    else if (m.Groups["parasingleparanr"].Success)
                    {
                        Debug.Print("para: " + m.Groups["parasingleparanr"].Value + " (" + m.Value + ")");
                        returnElements.Add(new XElement("xpara", m.Groups["parasingleparanr"].Value.ToString()));
                        start = m.Groups["parasingleparanr"].Index + m.Groups["parasingleparanr"].Length;
                    }

                    if ((i + 1) == mc.Count && start < pareRef.Length)
                    {
                        returnElements.Add(new XText(pareRef.Substring(start, pareRef.Length - (start))));
                    }
                }
            }
             
            return returnElements;

        }

        private static string GetTokenText(string startText, int value)
        {
            string regexp = "";
            string valueName = "";
            switch (value)
            {
                case 0: regexp = _R["start_text"]; valueName = "start_text"; break;
                case 1: regexp = _R["trail_text"]; valueName = "trail_text"; break;
            }

            string returnText = "";
            Match m = Regex.Match(startText, regexp);
            if (m.Success)
            {
                returnText = m.Groups[valueName].Value;
            }
            return returnText;
        }

        private static void GetReferances(XElement e)
        {
            foreach (XNode n in e.Nodes())
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement test = (XElement)n;
                    switch (test.Name.LocalName.ToString())
                    {
                        case "title":
                        case "xref":
                        case "yref":
                        case "comment":
                        case "small":
                            break;
                        default:
                            GetReferances((XElement)n);
                            break;
                    }   
                    
                }
                else if (n.NodeType == XmlNodeType.Text)
                {

                    XElement newE =  GetReferanceString(n);
                    if (newE != null)
                    {
                        n.ReplaceWith(newE.Nodes());
                    }
                }
            }
        }

        public XDocument GetInternalLinks(XDocument document)
        {
            LoadRegexp();


            foreach (XElement e in document.Descendants("document"))
            {
                GetReferances(e);
            }
            return document;
        }
    }
}
