using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DIB.InTextLinking
{
    public class InTextLinkingBuildRegexp
    {
        
        public string GetRegexExpression(string name, XElement regexps, string abbrevRegexp)
        {
            string returnValue = string.Empty;
            Dictionary<string, string> d = BuildRegexpDictionary(regexps,abbrevRegexp);

            KeyValuePair<string, string> r = d.Where(p => p.Key == name).FirstOrDefault();
            if (r.Key != null) returnValue = r.Value;
            return returnValue;
        }
        
        public Dictionary<string, string> BuildRegexpDictionary(XElement regexps, string abbrevRegexp)
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>();
            try
            {
                try
                {
                    Regex r = new Regex(abbrevRegexp);
                }
                catch (SystemException e)
                {
                    returnValue = null;
                    throw new Exception(string.Format("Build_Regexp_Dictionary Error: {0}", e.Message.ToString()));
                }

                returnValue.Add("idsources", abbrevRegexp);
                if (!BuildRegExp(ref returnValue, regexps, "single")) return null;
                if (!BuildRegExp(ref returnValue, regexps, "single_multi")) return null;
                if (!BuildRegExp(ref returnValue, regexps, "multi")) return null;

            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("BuildRegexpDictionary Error: {0}", e.Message.ToString()));
            }

            return returnValue;
        }

        private bool BuildRegExp(ref Dictionary<string, string> d, XElement regexps, string type)
        {
            bool returnValue = false;
            string id = "";
            try
            {

                foreach (XElement e in regexps.Elements("regexp")
                    .Where(p => (p.Attribute("type") == null ? "" : p.Attribute("type").Value) == type))
                {
                    string result = "";


                    XElement expression = e.Element("expression");
                    id = e.Element("id").Value;
                    result = GetPart(d, expression);
                    Regex test = new Regex(result);
                    d.Add(id, result);
                }
                returnValue = true;
            }

            catch (SystemException e)
            {
                throw new Exception(string.Format("BuildRegExp - Error: {0}. Error i Id: {1} ", e.Message.ToString(), id));
            }
            return returnValue;
        }


        private string GetPart(Dictionary<string, string> d, XElement part)
        {

            string result = "";
            try
            {
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
                                string sTest = e.Element("text").Value.Trim().ToString();
                                result = result + sTest;
                            }
                            break;
                        case "container":
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
                                string sTest = e.Element("text").Value.Trim().ToString();
                                result = result + "(" + sTest + ")?";
                            }
                            break;
                        case "multiple":
                            result = result + "(" + GetPart(d, e) + ")+";
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
            }
            catch (SystemException e)
            {
                throw new Exception(string.Format("GetPart Error: {0}", e.Message.ToString()));
            }
            return result;
        }

    }
}
