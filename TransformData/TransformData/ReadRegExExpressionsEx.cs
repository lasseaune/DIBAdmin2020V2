using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DIB.Data;
using System.Text.RegularExpressions;

namespace DIB.RegExp.Util
{
    class ReadRegExExpressionsEx
    {

        private void GetSourceTokens(ref Dictionary<string, string> dict)
        {
            commxml cx = new commxml();
            string sourceRegexp = cx.Execute("_ImpClientGetSouresToken");
            XDocument d = XDocument.Parse(sourceRegexp);
            string sourceToken = d.Descendants("row").First().Attributes().First().Value;
            //sourceToken = "(?<sourcename>(" + sourceToken + "))";
            dict.Add("idsources", sourceToken);
        }
        
        public Dictionary<string, string> Read()
        {
            
            Dictionary<string, string> d = new Dictionary<string, string>();

            GetSourceTokens(ref d);
            
            string fileName = System.Windows.Forms.Application.StartupPath + @"\xml\regexp.xml";
            XDocument xDoc = XDocument.Load(fileName);
            foreach (XElement e in xDoc.Root.Elements("regexp")
                            .Where(p=>(p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "single"))
            {
                string result = "";
                string id = "";

                XElement expression = e.Element("expression");
                id = e.Element("id").Value;
                result = GetPart(d, expression);
                Regex test = new Regex(result);
                d.Add(id, result);
            }

            foreach (XElement e in xDoc.Root.Elements("regexp")
                .Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "single_multi"))
            {
                string result = "";
                string id = "";

                XElement expression = e.Element("expression");
                id = e.Element("id").Value;
                result = GetPart(d, expression);
                Regex test = new Regex(result);
                d.Add(id, result);
            }



            foreach (XElement e in xDoc.Root.Elements("regexp")
                .Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == "multi"))
            {
                string result = "";
                string id = "";

                XElement expression = e.Element("expression");
                id = e.Element("id").Value;
                result = GetPart(d,expression);
                Regex test = new Regex(result);
                d.Add(id, result);
            }



            return d;
        }

        private string GetPart(Dictionary<string, string> d,XElement part)
        {
            string result = "";
            foreach (XElement e in part.Elements("part"))
            {
                if (e.Parent.Attribute("type") != null)
                {
                    if (e.Parent.Attribute("type").Value.ToString() == "or" && result != "")
                    {
                        result = result + '|';
                    }
                }

                switch (e.Attribute("type").Value.ToString())
                {
                    case "regexp":
                        if (e.Attribute("name") != null)
                        {
                            if (d.Keys.Contains(e.Attribute("name").Value.ToString()))
                                result = result + d[e.Attribute("name").Value.ToString()];
                        }
                        break;
                    case "single":
                        if (e.Element("text") != null)
                        {
                            string sTest = e.Element("text").Value.ToString();
                            result = result + sTest;
                        }
                        break;
                    case "chain":
                        result = result + "(" + GetPart(d, e) + ")";
                        break;
                    case "optional":
                        if (e.Elements("part").Count() != 0)
                        {
                            result = result + "(" + GetPart(d, e) + ")?";
                        }
                        else
                        {
                            string sTest = e.Element("text").Value.ToString();
                            result = result + "(" + sTest + ")?";
                        }
                        break;
                    case "multiple":
                        result = result + "(" + GetPart(d, e) + ")+";
                        break;
                    case "container":
                        result = result + "(" + GetPart(d, e) + ")";
                        break;
                    case "positive lookbehind":
                        result = result + "(?<=" + GetPart(d, e) + ")";
                        break;
                    case "negative lookbehind":
                        result = result + "(?<!" + GetPart(d, e) + ")";
                        break;
                    case "positive lookahead":
                        result = result + "(?=" + GetPart(d, e) + ")";
                        break;
                    case "negative lookahead":
                        result = result + "(?!" + GetPart(d, e) + ")";
                        break;
                    case "or":
                        result = result + "(" + GetPart(d, e) + ")";
                        break;
                    case "named":
                        result = result + "(?<" + e.Attribute("name").Value.ToString() + ">(" + GetPart(d, e) + "))";
                        break;

                }
            }
            return result;
        }



    }
}
