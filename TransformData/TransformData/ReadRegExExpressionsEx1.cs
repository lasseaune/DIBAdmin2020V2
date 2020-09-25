using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;


namespace DIB.RegExp.ExternalStaticLinks
{
    public class ReadRegExExpressionsEx1
    {
        public int _SERVER = 0;
        public string _USER = "abbrev_admin";
        private ExternalStaticLinksData _data = new ExternalStaticLinksData();
        
        public ReadRegExExpressionsEx1(int server)
        {
            _SERVER = server;
        }

        public bool UpdateGlobalRegexpsXML()
        {
            bool returnValue = false;
            XElement regexps = LoadGlobalRegexpsXML();
            Dictionary<string, string> d = Build_Regexp_Dictionary(regexps);
            if (d != null)
            {
                returnValue = StoreAbbrevSourceRegexp(d);
            }
            return returnValue;
        }

        private bool StoreAbbrevSourceRegexp(Dictionary<string, string> _R)
        {
            bool returnValue = false;
            try
            {
                if (_R != null)
                {
                    bool clean = true;
                    foreach (KeyValuePair<string, string> kvp in _R)
                    {
                        try
                        {
                            Regex r = new Regex(kvp.Value);
                        }
                        catch (SystemException e)
                        {
                            clean = false;
                        }
                    }
                    if (clean)
                    {
                        foreach (KeyValuePair<string, string> kvp in _R)
                        {
                            _data.Global_Regexp_Update(kvp.Key, kvp.Value, _USER, _SERVER);
                        }
                        returnValue = true;
                    }
                }
            }
            catch (SyntaxErrorException e)
            {
                throw new Exception(string.Format("StoreAbbrevSourceRegexp Error: {0}", e.Message.ToString()));
            }
            return returnValue;
        }

        private XElement LoadGlobalRegexpsXML()
        {
            XElement returnValue = null;
            try
            {
                XElement gXML = _data.LoadGlobalXML("_regexp", _SERVER);
                if (gXML.Descendants("regexps").Count() != 0)
                {
                    returnValue = gXML.Descendants("regexps").First();
                }
            }
            catch (SyntaxErrorException e)
            {
                throw new Exception(string.Format("LoadGlobalRegexpsXML Error: {0}", e.Message.ToString()));

            }
            return returnValue;
        }

        public Dictionary<string, string> Build_Regexp_Dictionary(XElement regexps)
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>();
            try
            {
                string abbrevRegexp = string.Empty;
                abbrevRegexp = _data.Abbrevations_Build(_SERVER);
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
            catch (SyntaxErrorException e)
            {
                throw new Exception(string.Format("Build_Regexp_Dictionary Error: {0}", e.Message.ToString()));
            }

            return returnValue;
        }

        private bool BuildRegExp(ref Dictionary<string, string> d , XElement regexps, string type)
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
                throw new SystemException(string.Format("BuildRegExp - Error: {0}. Error i Id: {1} " ,  e.Message.ToString(), id));
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
